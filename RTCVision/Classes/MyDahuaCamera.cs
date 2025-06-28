using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using HalconDotNet;
using MVSDK_Net;
using RTCConst;

namespace RTCDahuaSdk
{
    public delegate void DahuaCameraStatusChangedEvents(bool isConnected);

    public class MyDahuaCamera
    {
        #region VARIABLES AND PROPERTIES

        public MyCamera MySelf = new MyCamera();
        List<IMVDefine.IMV_Frame> m_frameList = new List<IMVDefine.IMV_Frame>();
        Thread _generateImageThread = null;
        bool m_bGenerateImage = true;
        Mutex m_mutex = new Mutex();
        bool m_bGrabbing = false;
        Thread _receiveThread = null;
        IMVDefine.IMV_Frame m_frame;
        IntPtr m_pDstData;
        int m_nDataLenth = 0;
        private object _lockObject = new object();
        private Bitmap _bitmap = null;
        private Thread _checkConnectThread = null;
        private string _deviceName = string.Empty;
        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, int count);
        /// <summary>
        /// Thông báo lỗi trả ra của camera
        /// </summary>
        public string ErrorMessage { get; set; }
        /// <summary>
        /// Xác nhận cơ chế lấy ảnh của camera là ASync (lấy liên tục) hay Sync (chờ gọi lấy ảnh từ chương trình ngoài)
        /// </summary>
        public string GrabberMode { get; set; } = CGrabberMode.ASync;
        /// <summary>
        /// Cờ xác nhận trạng thái kết nối của camera
        /// </summary>
        public bool IsConnected => MySelf.IMV_IsOpen();
        /// <summary>
        /// Cờ xác nhận mode Trigger có đang được bật hay không
        /// </summary>
        public bool IsTriggerOn { get; set; } = false;
        /// <summary>
        /// Thời gian chờ lấy buffer của camera
        /// </summary>
        public int GetImageBufferTimeOut { get; set; } = 1000;
        /// <summary>
        /// Cờ báo có bật chế độ tự kết nối lại camera khi mất kết nối hay không
        /// </summary>
        public bool AutoReconnect { get; set; } = false;
        /// <summary>
        /// Cờ báo có sử dụng chế độ ghép ảnh sau khi chụp hay không
        /// </summary>
        public bool Merging { get; set; } = false;
        /// <summary>
        /// Chiều ghép ảnh (ngang hoặc dọc)
        /// </summary>
        public string DirectionMerging { get; set; } = CDirectionMerging.Vertical;
        /// <summary>
        /// Số lần lấy ảnh, mặc định là 1, >1 dùng khi ghép ảnh
        /// </summary>
        public int SnapCount { get; set; } = 1;
        /// <summary>
        /// Khoảng thời gian giữa các lần kiểm tra kết nối camera
        /// </summary>
        public int IntervalTimeCheckConnected { get; set; } = 500;
        /// <summary>
        /// Cờ báo có bật chế độ tự động kiểm tra kết nối hay không
        /// </summary>
        public bool AutoCheckConnected { get; set; } = true;
        /// <summary>
        /// Sự kiện đưa ra ngoài để báo tình trạng kết nối của camera
        /// </summary>
        public event DahuaCameraStatusChangedEvents OnDahuaCameraStatusChangedEvents;

        const int DEFAULT_INTERVAL = 40;
        Stopwatch m_stopWatch = new Stopwatch();

        #endregion

        #region FUNCTIONS
        /// <summary>
        /// Lấy toàn bộ thông tin camera Dahua trong máy
        /// </summary>
        /// <returns>Danh sách camera Dahua</returns>
        public static List<IMVDefine.IMV_DeviceInfo> GetListDeviceInfos()
        {
            try
            {
                List<IMVDefine.IMV_DeviceInfo> deviceInfos = new List<IMVDefine.IMV_DeviceInfo>();

                IMVDefine.IMV_DeviceList deviceList = new IMVDefine.IMV_DeviceList();
                IMVDefine.IMV_EInterfaceType interfaceTp = IMVDefine.IMV_EInterfaceType.interfaceTypeAll;
                int res = MyCamera.IMV_EnumDevices(ref deviceList, (uint)interfaceTp);

                if (res == IMVDefine.IMV_OK && deviceList.nDevNum > 0)
                {
                    for (int i = 0; i < deviceList.nDevNum; i++)
                    {
                        IMVDefine.IMV_DeviceInfo deviceInfo =
                            (IMVDefine.IMV_DeviceInfo)
                            Marshal.PtrToStructure(
                                deviceList.pDevInfo + Marshal.SizeOf(typeof(IMVDefine.IMV_DeviceInfo)) * i,
                                typeof(IMVDefine.IMV_DeviceInfo));
                        deviceInfos.Add(deviceInfo);
                    }
                }

                return deviceInfos;
            }
            catch
            {
                return new List<IMVDefine.IMV_DeviceInfo>();
            }
        }
        /// <summary>
        /// Lấy toàn bộ tên của các camera Dahua trong máy
        /// </summary>
        /// <returns>Danh sách tên camera Dahua</returns>
        public static List<string> GetListDeviceInfoNames()
        {
            try
            {
                List<string> deviceInfoNames = new List<string>();

                IMVDefine.IMV_DeviceList deviceList = new IMVDefine.IMV_DeviceList();
                IMVDefine.IMV_EInterfaceType interfaceTp = IMVDefine.IMV_EInterfaceType.interfaceTypeAll;
                int res = MyCamera.IMV_EnumDevices(ref deviceList, (uint)interfaceTp);

                if (res == IMVDefine.IMV_OK && deviceList.nDevNum > 0)
                {
                    for (int i = 0; i < deviceList.nDevNum; i++)
                    {
                        IMVDefine.IMV_DeviceInfo deviceInfo =
                            (IMVDefine.IMV_DeviceInfo)
                            Marshal.PtrToStructure(
                                deviceList.pDevInfo + Marshal.SizeOf(typeof(IMVDefine.IMV_DeviceInfo)) * i,
                                typeof(IMVDefine.IMV_DeviceInfo));
                        deviceInfoNames.Add(deviceInfo.cameraKey);
                    }
                }

                return deviceInfoNames;
            }
            catch(Exception ex)
            {
                return new List<string>();
            }
        }
        /// <summary>
        /// Kiểm tra 1 camera Dahua có tồn tại hay không theo tên camera
        /// </summary>
        /// <param name="deviceName">Tên camera cần kiểm tra</param>
        /// <returns>True: Tồn tại; False: Không tồn tại</returns>
        public static bool DeviceExist(string deviceName)
        {
            try
            {
                List<IMVDefine.IMV_DeviceInfo> deviceInfos = GetListDeviceInfos();
                foreach (IMVDefine.IMV_DeviceInfo deviceInfo in deviceInfos)
                {
                    if (deviceInfo.cameraKey == deviceName)
                        return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// Kết nối camera
        /// </summary>
        /// <param name="deviceName">Tên camera Dahua cần kết nối</param>
        /// <returns>True: Thành công; False: Thất bại</returns>
        public bool Open(string deviceName)
        {
            try
            {
                ErrorMessage = string.Empty;
                List<IMVDefine.IMV_DeviceInfo> deviceInfos = GetListDeviceInfos();
                if (deviceInfos.Count <= 0)
                    return false;
                for (int i = 0; i < deviceInfos.Count; i++)
                {
                    IMVDefine.IMV_DeviceInfo deviceInfo = deviceInfos[i];
                    if (deviceInfo.cameraKey == deviceName)
                    {
                        MySelf.IMV_CreateHandle(IMVDefine.IMV_ECreateHandleMode.modeByIndex, i);
                        int res = MySelf.IMV_Open();
                        if (res != IMVDefine.IMV_OK)
                        {
                            ErrorMessage = "Open camera failed";
                            return false;
                        }

                        MySelf.IMV_SetBufferCount(8);
                        IMVDefine.IMV_String triggerMode = new IMVDefine.IMV_String();
                        res = MySelf.IMV_GetEnumFeatureSymbol("TriggerMode", ref triggerMode);
                        if (res == IMVDefine.IMV_OK)
                            IsTriggerOn = triggerMode.str == "On";

                        if (AutoCheckConnected && _checkConnectThread == null)
                        {
                            _checkConnectThread = new Thread(CheckConnected)
                            {
                                IsBackground = true
                            };
                            _checkConnectThread.Start();
                        }

                        _deviceName = deviceName;
                        return true;
                    }
                }

                return false;
            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
                return false;
            }
            finally
            {
                if (OnDahuaCameraStatusChangedEvents != null)
                    OnDahuaCameraStatusChangedEvents(IsConnected);
            }
        }
        /// <summary>
        /// Đóng kết nối camera
        /// </summary>
        /// <returns>True: Thành công; False: Thất bại</returns>
        public bool Close()
        {
            ErrorMessage = string.Empty;
            try
            {
                if (AutoCheckConnected)
                    cancellationTokenSourceCheckConnected.Cancel();

                if (MySelf != null)
                {
                    if (true == MySelf.IMV_IsGrabbing())
                        StopGrabbing();
                    MySelf.IMV_ClearFrameBuffer();
                    MySelf.IMV_Close();
                    MySelf.IMV_DestroyHandle();
                }

                ClearAllEvents();

                _deviceName = string.Empty;

                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return false;
            }
            finally
            {
                if (OnDahuaCameraStatusChangedEvents != null)
                    OnDahuaCameraStatusChangedEvents(IsConnected);

            }
        }
        /// <summary>
        /// Đóng kết nối camera trong vòng tua kiểm tra trạng thái connect
        /// </summary>
        /// <returns>True: Thành công; False: Thất bại</returns>
        private bool CloseWhenCheckConnect()
        {
            ErrorMessage = string.Empty;
            try
            {
                if (MySelf != null)
                {
                    if (true == MySelf.IMV_IsGrabbing())
                        StopGrabbing();
                    MySelf.IMV_ClearFrameBuffer();
                    MySelf.IMV_Close();
                    MySelf.IMV_DestroyHandle();
                }
                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return false;
            }
            finally
            {
                if (OnDahuaCameraStatusChangedEvents != null)
                    OnDahuaCameraStatusChangedEvents(IsConnected);
            }
        }
        /// <summary>
        /// Bắt đầu lấy ảnh
        /// </summary>
        /// <returns>True: Thành công; False: Thất bại</returns>
        public bool StartGrabbing()
        {
            try
            {
                ErrorMessage = string.Empty;

                if (GrabberMode == "ASync")
                {
                    m_bGrabbing = true;
                    m_bGenerateImage = true;

                    _receiveThread = new Thread(ReceiveThreadProcess)
                    {
                        IsBackground = true
                    };
                    _receiveThread.Start();

                    _generateImageThread = new Thread(GenerateImageThread)
                    {
                        IsBackground = true
                    };
                    _generateImageThread.Start();
                    m_stopWatch.Start();
                }

                if (IMVDefine.IMV_OK != MySelf.IMV_StartGrabbing())
                {
                    if (GrabberMode == "ASync")
                    {
                        m_bGrabbing = false;
                        m_bGenerateImage = false;
                        _receiveThread.Join();
                        _generateImageThread.Join();
                        m_stopWatch.Stop();
                    }
                    ErrorMessage = "Start grabbing failed";
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return false;
            }

        }
        /// <summary>
        /// Dừng lấy ảnh
        /// </summary>
        /// <returns>True: Thành công; False: Thất bại</returns>
        public bool StopGrabbing()
        {
            try
            {
                ErrorMessage = string.Empty;

                if (GrabberMode == "ASync")
                {
                    m_bGrabbing = false;
                    m_bGenerateImage = false;
                    _receiveThread.Join();
                    _generateImageThread.Join();
                    m_stopWatch.Stop();
                }

                if (IMVDefine.IMV_OK != MySelf.IMV_StopGrabbing())
                {
                    ErrorMessage = "Stop Grabbing Fail!";
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return false;
            }

        }

        private CancellationTokenSource cancellationTokenSourceCheckConnected = new CancellationTokenSource();

        /// <summary>
        /// Hàm chuyên thực hiện việc kiểm tra kết nối của cam theo thời gian thực
        /// </summary>
        private void CheckConnected()
        {
            CancellationToken token = cancellationTokenSourceCheckConnected.Token;

            try
            {
                while (!token.IsCancellationRequested)
                {
                    if (!IsConnected)
                    {

                        CloseWhenCheckConnect();

                        if (AutoReconnect)
                        {
                            Open(_deviceName);

                            if (IsConnected)
                                StartGrabbing();
                        }
                    }

                    if (OnDahuaCameraStatusChangedEvents != null)
                        OnDahuaCameraStatusChangedEvents(IsConnected);
                    Thread.Sleep(IntervalTimeCheckConnected);
                }
            }
            catch
            {
                // ignored
            }
        }
        /// <summary>
        /// Loại bỏ các event của dối tượng truyền ra ngoài
        /// </summary>
        public void ClearAllEvents()
        {
            OnDahuaCameraStatusChangedEvents = null;
        }
        /// <summary>
        /// Hàm thực hiện thao tác trigger máy ảnh bằng phần mềm
        /// </summary>
        private void TriggerBySoftware()
        {
            if (MySelf.IMV_IsGrabbing())
            {
                int res = IMVDefine.IMV_OK;
                IMVDefine.IMV_String oldTriggerSource = new IMVDefine.IMV_String();
                MySelf.IMV_GetEnumFeatureSymbol("TriggerSource", ref oldTriggerSource);
                // Chuyển về TriggerSoftware
                if (oldTriggerSource.str == "Software")
                    res = MySelf.IMV_ExecuteCommandFeature("TriggerSoftware");
                else
                {
                    res = MySelf.IMV_SetEnumFeatureSymbol("TriggerSource", "Software");
                    res = MySelf.IMV_ExecuteCommandFeature("TriggerSoftware");
                    res = MySelf.IMV_SetEnumFeatureSymbol("TriggerSource", oldTriggerSource.str);
                }
            }
        }
        /// <summary>
        /// Hàm chạy trong Thread lấy ảnh liên tục với GrabMode = ASync
        /// </summary>
        private void ReceiveThreadProcess()
        {
            while (m_bGrabbing)
            {
                try
                {
                    if (MySelf != null && (IMVDefine.IMV_OK == MySelf.IMV_GetFrame(ref m_frame, 1000)))
                    {
                        lock (_lockObject)
                            m_frameList.Add(m_frame);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
        /// <summary>
        /// Hàm thực hiện 1 lần lấy ảnh dựa vào buffer đang có trong camera
        /// </summary>
        /// <returns>Ảnh định dạng Bitmap</returns>
        private Bitmap SingleShot()
        {
            try
            {
                _bitmap = null;

                if (MySelf != null && (IMVDefine.IMV_OK == MySelf.IMV_GetFrame(ref m_frame, (uint)GetImageBufferTimeOut)))
                {
                    if (m_frame.frameInfo.pixelFormat != IMVDefine.IMV_EPixelType.gvspPixelMono8 &&
                        m_frame.frameInfo.pixelFormat != IMVDefine.IMV_EPixelType.gvspPixelBGR8)
                        if (!ConvertToBGR24(m_frame))
                            _bitmap = null;

                    if (!ConvertToBitmap(ref m_frame, ref _bitmap))
                        _bitmap = null;

                    MySelf.IMV_ReleaseFrame(ref m_frame);
                }

                return _bitmap;
            }
            catch
            {
                return null;
            }
        }
        private bool IsTimeToDisplay()
        {
            m_stopWatch.Stop();
            long m_lDisplayInterval = m_stopWatch.ElapsedMilliseconds;
            if (m_lDisplayInterval <= DEFAULT_INTERVAL)
            {
                m_stopWatch.Start();
                return false;
            }
            else
            {
                m_stopWatch.Reset();
                m_stopWatch.Start();
                return true;
            }
        }
        /// <summary>
        /// Hạy chạy trong Thread chuyên để chuyển từ mảng buffer sang ảnh
        /// </summary>
        private void GenerateImageThread()
        {
            while (m_bGenerateImage)
            {
                try
                {
                    lock (_lockObject)
                    {
                        if (m_frameList.Count == 0)
                        {
                            Thread.Sleep(10);
                            continue;
                        }

                        IMVDefine.IMV_Frame frame = m_frameList.ElementAt(m_frameList.Count - 1);
                        m_frameList.Remove(frame);

                        GC.Collect();

                        if (false == IsTimeToDisplay())
                        {
                            MySelf.IMV_ReleaseFrame(ref frame);
                            continue;
                        }

                        try
                        {
                            if (frame.frameInfo.pixelFormat != IMVDefine.IMV_EPixelType.gvspPixelMono8 &&
                                frame.frameInfo.pixelFormat != IMVDefine.IMV_EPixelType.gvspPixelBGR8)
                                if (!ConvertToBGR24(frame))
                                    _bitmap = null;

                            if (!ConvertToBitmap(ref frame, ref _bitmap))
                                _bitmap = null;
                        }
                        catch
                        {
                            _bitmap = null;
                        }
                        finally
                        {
                            MySelf.IMV_ReleaseFrame(ref frame);
                        }
                    }
                }
                catch
                {
                    _bitmap = null;
                }
                finally
                {
                    Thread.Sleep(10);
                }
            }
        }
        public Bitmap GetLatestBitmap()
        {
            try
            {
                lock (_lockObject)
                {
                    Bitmap bitmap = _bitmap;
                    _bitmap = null;
                    return bitmap;
                }
            }
            catch
            {
                return null;
            }
        }
        private bool ConvertToBGR24(IMVDefine.IMV_Frame frame)
        {
            IMVDefine.IMV_PixelConvertParam stPixelConvertParam = new IMVDefine.IMV_PixelConvertParam();

            try
            {
                if (m_pDstData == IntPtr.Zero || (int)(frame.frameInfo.width * frame.frameInfo.height * 3) > m_nDataLenth)
                {
                    if (m_pDstData != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(m_pDstData);
                    }
                    m_pDstData = Marshal.AllocHGlobal((int)(frame.frameInfo.width * frame.frameInfo.height * 3));
                    m_nDataLenth = (int)(frame.frameInfo.width * frame.frameInfo.height * 3);
                }
            }
            catch
            {
                return false;
            }

            stPixelConvertParam.nWidth = frame.frameInfo.width;
            stPixelConvertParam.nHeight = frame.frameInfo.height;
            stPixelConvertParam.ePixelFormat = frame.frameInfo.pixelFormat;
            stPixelConvertParam.pSrcData = frame.pData;
            stPixelConvertParam.nSrcDataLen = frame.frameInfo.size;
            stPixelConvertParam.nPaddingX = frame.frameInfo.paddingX;
            stPixelConvertParam.nPaddingY = frame.frameInfo.paddingY;
            stPixelConvertParam.eBayerDemosaic = IMVDefine.IMV_EBayerDemosaic.demosaicBilinear;
            stPixelConvertParam.eDstPixelFormat = IMVDefine.IMV_EPixelType.gvspPixelBGR8;
            stPixelConvertParam.pDstBuf = m_pDstData;
            stPixelConvertParam.nDstBufSize = (uint)m_nDataLenth;

            int res = MySelf.IMV_PixelConvert(ref stPixelConvertParam);
            if (res != IMVDefine.IMV_OK)
            {
                return false;
            }

            return true;
        }
        private bool ConvertToBitmap(ref IMVDefine.IMV_Frame frame, ref Bitmap bitmap)
        {
            IntPtr pDstRGB;
            BitmapData bmpData;
            Rectangle bitmapRect = new Rectangle();
            int ImgSize;

            if (frame.frameInfo.pixelFormat == IMVDefine.IMV_EPixelType.gvspPixelMono8)
            {
                bitmap = new Bitmap((int)frame.frameInfo.width, (int)frame.frameInfo.height, PixelFormat.Format8bppIndexed);
                ColorPalette colorPalette = bitmap.Palette;
                for (int i = 0; i != 256; ++i)
                {
                    colorPalette.Entries[i] = Color.FromArgb(i, i, i);
                }
                bitmap.Palette = colorPalette;

                bitmapRect.Height = bitmap.Height;
                bitmapRect.Width = bitmap.Width;
                bmpData = bitmap.LockBits(bitmapRect, ImageLockMode.ReadWrite, bitmap.PixelFormat);
                CopyMemory(bmpData.Scan0, frame.pData, bmpData.Stride * bitmap.Height);
                bitmap.UnlockBits(bmpData);
            }
            else if (frame.frameInfo.pixelFormat == IMVDefine.IMV_EPixelType.gvspPixelBGR8)
            {
                bitmap = new Bitmap((int)frame.frameInfo.width, (int)frame.frameInfo.height, PixelFormat.Format24bppRgb);

                bitmapRect.Height = bitmap.Height;
                bitmapRect.Width = bitmap.Width;
                bmpData = bitmap.LockBits(bitmapRect, ImageLockMode.ReadWrite, bitmap.PixelFormat);
                CopyMemory(bmpData.Scan0, frame.pData, bmpData.Stride * bitmap.Height);
                bitmap.UnlockBits(bmpData);
            }
            else
            {
                ImgSize = (int)frame.frameInfo.width * (int)frame.frameInfo.height * 3;

                try
                {
                    pDstRGB = Marshal.AllocHGlobal(ImgSize);
                }
                catch
                {
                    return false;
                }
                if (pDstRGB == IntPtr.Zero)
                {
                    return false;
                }

                IMVDefine.IMV_PixelConvertParam stPixelConvertParam = new IMVDefine.IMV_PixelConvertParam();
                int res = 0;
                stPixelConvertParam.nWidth = frame.frameInfo.width;
                stPixelConvertParam.nHeight = frame.frameInfo.height;
                stPixelConvertParam.ePixelFormat = frame.frameInfo.pixelFormat;
                stPixelConvertParam.pSrcData = frame.pData;
                stPixelConvertParam.nSrcDataLen = frame.frameInfo.size;
                stPixelConvertParam.nPaddingX = frame.frameInfo.paddingX;
                stPixelConvertParam.nPaddingY = frame.frameInfo.paddingY;
                stPixelConvertParam.eBayerDemosaic = IMVDefine.IMV_EBayerDemosaic.demosaicNearestNeighbor;
                stPixelConvertParam.eDstPixelFormat = IMVDefine.IMV_EPixelType.gvspPixelBGR8;
                stPixelConvertParam.pDstBuf = pDstRGB;
                stPixelConvertParam.nDstBufSize = (uint)ImgSize;

                res = MySelf.IMV_PixelConvert(ref stPixelConvertParam);
                if (IMVDefine.IMV_OK != res)
                {
                    MessageBox.Show("image convert to BGR failed!");
                    return false;
                }

                bitmap = new Bitmap((int)frame.frameInfo.width, (int)frame.frameInfo.height, PixelFormat.Format24bppRgb);

                bitmapRect.Height = bitmap.Height;
                bitmapRect.Width = bitmap.Width;
                bmpData = bitmap.LockBits(bitmapRect, ImageLockMode.ReadWrite, bitmap.PixelFormat);
                CopyMemory(bmpData.Scan0, pDstRGB, bmpData.Stride * bitmap.Height);
                bitmap.UnlockBits(bmpData);

                Marshal.FreeHGlobal(pDstRGB);
            }
            return true;
        }
        /// <summary>
        /// Hàm tạo đoạn thông báo lỗi
        /// </summary>
        /// <param name="errorMessage">Thông báo lỗi cần đẩy ra</param>
        private void SetErrorMessage(string errorMessage)
        {
            if (string.IsNullOrEmpty(ErrorMessage))
                ErrorMessage = errorMessage;
            else
                ErrorMessage = $"{ErrorMessage}\n{errorMessage}";
        }
        /// <summary>
        /// Chuyển từ ảnh định dạng Bitmap sang HImage của Halcon
        /// </summary>
        /// <param name="bitmap">Ảnh định dạng Bitmap</param>
        /// <returns>Ảnh định dạng HImage</returns>
        public HImage Bitmap2HImage(System.Drawing.Bitmap bitmap)
        {
            if (bitmap == null)
                return new HImage();
            try
            {
                System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);
                HImage himg = new HImage();
                if (bitmap.PixelFormat == System.Drawing.Imaging.PixelFormat.Format8bppIndexed)
                {
                    System.Drawing.Imaging.BitmapData srcBmpData = bitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                    himg.GenImage1("byte", bitmap.Width, bitmap.Height, srcBmpData.Scan0);
                    bitmap.UnlockBits(srcBmpData);
                }
                else if (bitmap.PixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb)
                {
                    System.Drawing.Imaging.BitmapData srcBmpData = bitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    himg.GenImageInterleaved(srcBmpData.Scan0, "bgr", bitmap.Width, bitmap.Height, -1, "byte", 0, 0, 0, 0, -1, 0);
                    bitmap.UnlockBits(srcBmpData);
                }
                else if (bitmap.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb)
                {
                    System.Drawing.Imaging.BitmapData srcBmpData = bitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);
                    himg.GenImageInterleaved(srcBmpData.Scan0, "bgrx", bitmap.Width, bitmap.Height, -1, "byte", bitmap.Width, bitmap.Height, 0, 0, -1, 0);
                    bitmap.UnlockBits(srcBmpData);
                }
                else if (bitmap.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppRgb)
                {
                    System.Drawing.Imaging.BitmapData srcBmpData = null;
                    System.Drawing.Bitmap MetaBmp = new System.Drawing.Bitmap(bitmap.Width, bitmap.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                    System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(MetaBmp);
                    g.DrawImage(bitmap, rect);
                    g.Dispose();
                    srcBmpData = MetaBmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                    himg.GenImageInterleaved(srcBmpData.Scan0, "bgrx", bitmap.Width, bitmap.Height, -1, "byte", 0, 0, 0, 0, -1, 0);
                    MetaBmp.UnlockBits(srcBmpData);
                }
                else // default: trans to color image
                {
                    //System.Drawing.Imaging.BitmapData srcBmpData = input.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    //himg.GenImageInterleaved(srcBmpData.Scan0, "bgr", input.Width, input.Height, -1, "byte", 0, 0, 0, 0, -1, 0);
                    //input.UnlockBits(srcBmpData);

                    System.Drawing.Imaging.BitmapData srcBmpData = null;
                    System.Drawing.Bitmap MetaBmp = new System.Drawing.Bitmap(bitmap.Width, bitmap.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(MetaBmp);
                    g.DrawImage(bitmap, rect);
                    g.Dispose();
                    srcBmpData = MetaBmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    himg.GenImageInterleaved(srcBmpData.Scan0, "bgr", bitmap.Width, bitmap.Height, -1, "byte", 0, 0, 0, 0, -1, 0);
                    MetaBmp.UnlockBits(srcBmpData);
                }
                return himg;
            }
            catch (HalconException ex)
            {
                SetErrorMessage(ex.Message);
                return null;
            }
            catch (System.Exception ex)
            {
                SetErrorMessage(ex.Message);
                return null;
            }
        }
        /// <summary>
        /// Lệnh chụp lấy ảnh
        /// </summary>
        /// <param name="triggerBySoftware">Cờ báo có trigger bởi phần mềm hay không</param>
        /// <returns>Hình ảnh định dạng halcon</returns>
        public HImage Snap(bool triggerBySoftware)
        {
            ErrorMessage = string.Empty;
            HImage snapImage = null;
            int snapCount = Merging ? SnapCount : 1;
            if (snapCount <= 0)
                snapCount = 1;
            try
            {
                if (GrabberMode == CGrabberMode.ASync)
                {
                    for (int i = 0; i < snapCount; i++)
                    {
                        if (triggerBySoftware)
                            TriggerBySoftware();

                        int timeout = 0;
                        HImage hImage = null;

                        if (IsTriggerOn)
                        {
                            Bitmap bitmap = null;
                            bitmap = GetLatestBitmap();
                            if (bitmap != null)
                                hImage = Bitmap2HImage(bitmap);
                        }
                        else
                        {
                            while (true)
                            {
                                Bitmap bitmap = null;
                                bitmap = GetLatestBitmap();
                                if (bitmap != null)
                                {
                                    hImage = Bitmap2HImage(bitmap);
                                    break;
                                }
                                timeout += 5;
                                Thread.Sleep(5);
                                if (timeout >= 1000)
                                    break;
                            }
                        }

                        if (hImage != null && hImage.Key != IntPtr.Zero)
                            snapImage = snapImage == null ? hImage : snapImage.ConcatObj(hImage);
                    }
                }
                else
                {
                    for (int i = 0; i < snapCount; i++)
                    {
                        HImage hImage = null;
                        Bitmap bitmap = null;

                        if (triggerBySoftware)
                            TriggerBySoftware();

                        bitmap = SingleShot();
                        if (bitmap != null)
                            hImage = Bitmap2HImage(bitmap);

                        if (hImage != null && hImage.Key != IntPtr.Zero)
                            snapImage = snapImage == null ? hImage : snapImage.ConcatObj(hImage);
                    }
                }

                return snapImage;
            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
                return null;
            }
        }

        #endregion
    }
}
