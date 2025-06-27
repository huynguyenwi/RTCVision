using DevExpress.XtraEditors;
using HalconDotNet;
using RTCVision.Classes;
using RTCVision.Consts;
using SLMPTcp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RTCVision
{
    public partial class FrmMain : DevExpress.XtraEditors.XtraForm
    {
        public FrmMain()
        {
            InitializeComponent();
        }

        #region Variables
        /// <summary>
        /// Bảng chứa thông tin danh sách model của chương trình
        /// </summary>
        private DataTable _modelTable = null;
        private Guid _currentModelId = Guid.Empty;
        private string _interfaceName = string.Empty;
        private string _deviceName = string.Empty;
        private List<Device> _devices = null;
        private HImage _hImage = null;
        private HTuple _frameGrabber;
        public HTuple FrameGrabber => _frameGrabber;
        private Region _matchingTrainRegion = null;
        private Region _matchingFindRegion = null;
        private HDrawingObject _drawingObjectRegion;
        private string _currentRegionType = string.Empty;


        private HObject _matchingImageMaster = null;
        private double _matchingMinScore = 0.6;
        private int _matchingNumMatches = 1;
        private HTuple _matchingIn_ModelID = new HTuple();
        private HTuple _matchingIn_Origin = new HTuple();
        private HTuple _matchingOut_Origin = new HTuple();
        private HTuple _matchingOut_ModelID = new HTuple();
        private HTuple _matchingOut_Row = new HTuple();
        private HTuple _matchingOut_Column = new HTuple();
        private HTuple _matchingOut_Angle = new HTuple();
        private HTuple _matchingOut_Score = new HTuple();
        private HTuple _matchingOut_ActualNumberMatches = new HTuple();
        private HTuple _matchingException = new HTuple();
        private HTuple _matchingPass = new HTuple();
        private HTuple _matchingTrained = new HTuple();

        private HObject _matchingOut_ContoursTrain = new HObject();
        private HObject _matchingOut_ContoursFind = new HObject();

        private string _ipAddress = "127.0.0.1";
        private int _portNumber = 4000;
        private string _comPort = "COM2";
        private int _baudRate = 9600;
        private string _triggerValue = "C";
        private string _realTriggerValue = "";

        private Socket _socket;
        private SerialPort _serialPort;
        private byte[] _buffer;
        private bool _isRun = false;
        private Thread _threadRun = null;
        private Thread _threadReadTrigger = null;

        private string _plcIpAddress = "192.168.9.99";
        private int _plcPortNumber = 12234;       
        private string _triggerValuePLC = "1";
        private bool success;
        private Thread _threadReadySignal = null;

        private SLMPClient _plcClient;

        #endregion

        #region PLC Register Definitions
        // Định nghĩa các thanh ghi PLC
        private const string PLC_READY_REGISTER = "D100"; 
        private const string PLC_BUSY_REGISTER = "D101"; 
        private const string PLC_FINISH_REGISTER = "D102";
        private const string PLC_OK_REGISTER = "D109";    
        private const string PLC_NG_REGISTER = "D110";
        private const string PLC_TRIGGER_REGISTER = "D120";
        #endregion

        #region Functions
        private void LoadCameraInfo()
        {
            _interfaceName = Lib.ExecuteScalar($"SELECT InterfaceName FROM ModelCam WHERE ModelID='{_currentModelId}'").ToString();
            _deviceName = Lib.ExecuteScalar($"SELECT DeviceName FROM ModelCam WHERE ModelID='{_currentModelId}'").ToString();
        }
        private void LoadAllRegions()
        {
            _matchingTrainRegion = null;
            _matchingFindRegion = null;
            DataTable dataTable =
                Lib.GetTableData($"SELECT * FROM {CTableName.Regions} WHERE {CColName.ModelId} = '{_currentModelId}'");
            if (dataTable != null)
                foreach (DataRow row in dataTable.Rows)
                {
                    string regionType = Lib.ToString(row[CColName.RegionType]);
                    switch (regionType)
                    {
                        case CRegionType.MTrain:
                            {
                                _matchingTrainRegion = new Region
                                {
                                    Row = Lib.ToDouble(row[CColName.Row]),
                                    Col = Lib.ToDouble(row[CColName.Col]),
                                    Width = Lib.ToDouble(row[CColName.Width]),
                                    Height = Lib.ToDouble(row[CColName.Height]),
                                    Phi = Lib.ToDouble(row[CColName.Phi])
                                };
                                break;
                            }
                        case CRegionType.MFind:
                            {
                                _matchingFindRegion = new Region
                                {
                                    Row = Lib.ToDouble(row[CColName.Row]),
                                    Col = Lib.ToDouble(row[CColName.Col]),
                                    Width = Lib.ToDouble(row[CColName.Width]),
                                    Height = Lib.ToDouble(row[CColName.Height]),
                                    Phi = Lib.ToDouble(row[CColName.Phi])
                                };
                                break;
                            }
                    }
                }
        }
        private void LoadMatchingOptions()
        {
            DataTable dataTable =
                Lib.GetTableData($"SELECT * FROM {CTableName.Matching} WHERE {CColName.ModelId} = '{_currentModelId}'");
            if (dataTable != null && dataTable.Rows.Count > 0)
            {
                _matchingMinScore = Lib.ToDouble(dataTable.Rows[0][CColName.In_MinScore]);
                _matchingNumMatches = Lib.ToInt(dataTable.Rows[0][CColName.In_NumMatches]);
            }
            else
            {
                _matchingMinScore = 0.6;
                _matchingNumMatches = 1;
            }
        }
        private void LoadDict()
        {
            _matchingOut_ModelID = new HTuple();

            string hdictFileName = Path.Combine(Application.StartupPath, "Iconic.hdict");

            HTuple dict = null;

            if (File.Exists(hdictFileName))
            {
                HOperatorSet.ReadDict(hdictFileName, new HTuple(), new HTuple(), out dict);
                HOperatorSet.GetDictTuple(dict, _currentModelId.ToString() + "MatchingModelID", out _matchingOut_ModelID);
                HOperatorSet.GetDictObject(out _matchingImageMaster, dict, _currentModelId.ToString() + "MatchingImageMaster");
            }
        }
        private void LoadTriggerInfo()
        {
            /* _ipAddress = Lib.ExecuteScalar($"SELECT {CColName.IpAddress} FROM {CTableName.TriggerSettings} WHERE {CColName.ModelId}='{_currentModelId}'").ToString();
             _portNumber = Lib.ToInt(Lib.ExecuteScalar(
                 $"SELECT {CColName.PortNumber} FROM {CTableName.TriggerSettings} WHERE {CColName.ModelId}='{_currentModelId}'"));
             _triggerValue = Lib.ExecuteScalar($"SELECT {CColName.TriggerValue} FROM {CTableName.TriggerSettings} WHERE {CColName.ModelId}='{_currentModelId}'").ToString();*/

            _comPort = Lib.ExecuteScalar($"SELECT {CColName.IpAddress} FROM {CTableName.TriggerSettings} WHERE {CColName.ModelId}='{_currentModelId}'").ToString();
            _baudRate = Lib.ToInt(Lib.ExecuteScalar($"SELECT {CColName.PortNumber} FROM {CTableName.TriggerSettings} WHERE {CColName.ModelId}='{_currentModelId}'"));
            _triggerValue = Lib.ExecuteScalar($"SELECT {CColName.TriggerValue} FROM {CTableName.TriggerSettings} WHERE {CColName.ModelId}='{_currentModelId}'").ToString();


            _plcIpAddress = "192.168.9.99";
            _plcPortNumber = 12234;     
            _triggerValuePLC = "1";
        }
        
        private bool LoadModelInfo()
        {
            try
            {
                GetCurrentModel();
                if (_currentModelId == Guid.Empty)
                {
                    MessageBox.Show($"Model can't empty", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                // Lấy thông tin camera
                LoadCameraInfo();

                LoadAllRegions();

                LoadMatchingOptions();

                LoadDict();

                LoadTriggerInfo();

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}\n{ex.StackTrace}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        /// <summary>
        /// Nạp danh sách model từ dữ liệu vào bảng
        /// </summary>

        private void LoadAllModels()
        {
            _modelTable = Lib.GetTableData("SELECT * FROM Models");
            cbModel.Properties.Items.Clear();
            foreach (DataRow row in _modelTable.Rows)
            {
                cbModel.Properties.Items.Add(row[CColName.ModelName].ToString());
            }
            cbModel.SelectedIndex = 0;

            if (_modelTable != null)
            {
                cbModel.SelectedIndex = 0;
                if (GlobVar.Settings.CurrentModelId != Guid.Empty)
                    foreach (DataRow row in _modelTable.Rows)
                    {
                        if (GlobVar.Settings.CurrentModelId == Lib.ToGuid(row[CColName.Id]))
                            cbModel.SelectedItem = row;
                    }
            }

            GetCurrentModel();
        }
        private void GetCurrentModel()
        {
            string selectedModelName = cbModel.Text;
            if (!string.IsNullOrEmpty(selectedModelName))
            {
                DataRow[] rows = _modelTable.Select($"{CColName.ModelName} = '{selectedModelName.Replace("'", "''")}'");
                if (rows.Length > 0)
                {
                    DataRow row = rows[0];
                    _currentModelId = Lib.ToGuid(row["Id"]);
                }
            }
        }


        private void ReadTriggerValue()
        {
            _realTriggerValue = string.Empty;
            while (_isRun)
            {
                byte[] buffer = new byte[500];
                _socket.Receive(buffer);
                _realTriggerValue = Encoding.ASCII.GetString(buffer).Replace("\0", "");
            }
        }
        private bool DisconnectTCP()
        {
            try
            {
                if (_socket != null)
                {
                    _socket.Close();
                    _socket.Dispose();
                    _socket = null;
                }

                return true;
            }
            catch (Exception)
            {
                MessageBox.Show("Error disconnect", "Warning!");
                return false;
            }
        }
       /* private bool ConnectTCP()
        {
            try
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                var endPoint = new IPEndPoint(IPAddress.Parse(_ipAddress), _portNumber);
                _socket.Connect(endPoint);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Can't connect to {_ipAddress}:{_portNumber}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }*/

        private bool ConnectSerial()
        {
            try
            {
                if (_serialPort != null && _serialPort.IsOpen)
                    _serialPort.Close();

                _serialPort = new SerialPort();

                _serialPort.PortName = _comPort;
                _serialPort.BaudRate = _baudRate;
                _serialPort.DataBits = 8;
                _serialPort.Parity = Parity.None;
                _serialPort.StopBits = StopBits.One;
                _serialPort.Handshake = Handshake.None;
                _serialPort.Encoding = System.Text.Encoding.ASCII;

                _serialPort.DataReceived += SerialPort_DataReceived;

                _serialPort.Open();

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Can't connect to {_comPort} : {_baudRate} baud.\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private bool DisconnectSerial()
        {
            try
            {
                if (_serialPort != null && _serialPort.IsOpen)
                {
                    _serialPort.DataReceived -= SerialPort_DataReceived;
                    _serialPort.Close();
                    _serialPort.Dispose();
                    _serialPort = null;
                }

                return true;
            }
            catch (Exception)
            {
                MessageBox.Show("Error disconnect", "Warning!");
                return false;
            }
        }

        /// <summary>
        /// Bộ xử lý sự kiện để nhận dữ liệu từ cổng serial.
        /// </summary>
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                // Đọc dữ liệu đến
                _realTriggerValue = _serialPort.ReadExisting().Trim(); 
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Lỗi Serial", MessageBoxButtons.OK, MessageBoxIcon.Error);
                
            }
        }


        /// <summary>
        /// Kết nối đến PLC bằng SLMP.
        /// </summary>
        private bool ConnectPLC()
        {
            try
            {
                if (_plcClient != null && _plcClient.IsConnected)
                {
                    _plcClient.Disconnect();
                }
                _plcClient = new SLMPClient(_plcIpAddress, _plcPortNumber);
                _plcClient.Connect();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Can't connect to PLC ({_plcIpAddress}:{_plcPortNumber}): {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Ngắt kết nối khỏi PLC.
        /// </summary>
        private bool DisconnectPLC()
        {
            try
            {
                if (_plcClient != null && _plcClient.IsConnected)
                {
                    _plcClient.Disconnect();
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error disconnecting from PLC: {ex.Message}", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
        }

        private bool RunMatchingModel(out string errMessage)
        {
            errMessage = string.Empty;
            try
            {
                HDevProcedure procedure = new HDevProcedure("Matching");
                HDevProcedureCall procedureCall = new HDevProcedureCall(procedure);

                procedureCall.SetInputIconicParamObject("Image", _hImage);
                if (_drawingObjectRegion != null)
                    procedureCall.SetInputIconicParamObject("In_Roi", _drawingObjectRegion.GetDrawingObjectIconic());
                else
                {
                    //procedureCall.SetInputIconicParamObject("In_Roi", new HObject()); // Câu lệnh gây lỗi làm ví dụ

                    HOperatorSet.GenEmptyObj(out HObject emptyObject);
                    procedureCall.SetInputIconicParamObject("In_Roi", emptyObject);
                }

                procedureCall.SetInputCtrlParamTuple("Select_Mode", "Run");
                procedureCall.SetInputCtrlParamTuple("In_ModelID", _matchingOut_ModelID);
                procedureCall.SetInputCtrlParamTuple("In_MinScore", _matchingMinScore);
                procedureCall.SetInputCtrlParamTuple("In_NumMatches", _matchingNumMatches);
                procedureCall.SetInputCtrlParamTuple("In_Origin", _matchingIn_Origin);

                //GlobVar.MyEngine.StartDebugServer();

                procedureCall.Execute();

                //GlobVar.MyEngine.StopDebugServer();

                _matchingOut_Row = procedureCall.GetOutputCtrlParamTuple("Out_Row");
                _matchingOut_Column = procedureCall.GetOutputCtrlParamTuple("Out_Column");
                _matchingOut_Angle = procedureCall.GetOutputCtrlParamTuple("Out_Angle");
                _matchingOut_Score = procedureCall.GetOutputCtrlParamTuple("Out_Score");
                _matchingException = procedureCall.GetOutputCtrlParamTuple("Exception");
                _matchingPass = procedureCall.GetOutputCtrlParamTuple("Pass");
                _matchingOut_ActualNumberMatches = procedureCall.GetOutputCtrlParamTuple("Out_ActualNumberMatches");

                _matchingOut_ContoursFind = procedureCall.GetOutputIconicParamObject("Out_ContoursFind");

                return true;
            }
            catch (HDevEngineException ex)
            {
                errMessage = $"{ex.Message}\n{ex.StackTrace}";
                return false;
            }
            catch (Exception ex)
            {
                errMessage = $"{ex.Message}\n{ex.StackTrace}";
                return false;
            }
        }
        private void RunProgramProcess()
        {
            if (HWindowsMain.InvokeRequired)
                HWindowsMain.Invoke(new MethodInvoker(() =>
                {
                    HWindowsMain.HalconWindow.SetDraw("margin");
                    HWindowsMain.HalconWindow.SetLineWidth(2);
                }));
            else
            {
                HWindowsMain.HalconWindow.SetDraw("margin");
                HWindowsMain.HalconWindow.SetLineWidth(2);
            }

            while (_isRun)
            {
                try
                {
                    // Kiểm tra tín hiệu trigger
                    if (_realTriggerValue != _triggerValue)
                        continue;
                    _realTriggerValue = string.Empty;
                    // Chụp lấy ảnh
                    _hImage = GlobFunc.SnapImage(_frameGrabber);

                    if (_hImage == null || _hImage.Key == IntPtr.Zero)
                        continue;

                    // Chạy procedure
                    if (RunMatchingModel(out string errMessage))
                    {
                        if (HWindowsMain.InvokeRequired)
                            HWindowsMain.Invoke(new MethodInvoker(() =>
                            {
                                HWindowsMain.HalconWindow.SetColor(_matchingPass.I == 1 ? "green" : "red");
                                HWindowsMain.HalconWindow.ClearWindow();
                                HWindowsMain.HalconWindow.DispImage(_hImage);
                                if (_matchingOut_ContoursFind != null && _matchingOut_ContoursFind.Key != IntPtr.Zero)
                                    HWindowsMain.HalconWindow.DispObj(_matchingOut_ContoursFind);
                            }));
                        else
                        {
                            HWindowsMain.HalconWindow.SetColor(_matchingPass.I == 1 ? "green" : "red");
                            HWindowsMain.HalconWindow.ClearWindow();
                            HWindowsMain.HalconWindow.DispImage(_hImage);
                            if (_matchingOut_ContoursFind != null && _matchingOut_ContoursFind.Key != IntPtr.Zero)
                                HWindowsMain.HalconWindow.DispObj(_matchingOut_ContoursFind);
                        }
                        /* if (lblStatus.InvokeRequired)
                             lblStatus.Invoke(new MethodInvoker(() =>
                             {
                                 lblStatus.Visible = true;
                                 lblStatus.BackColor = _matchingPass.I == 1 ? Color.Green : Color.Red;
                                 lblStatus.Text = _matchingPass.I == 1 ? "OK" : "NG";
                             }));
                         else
                         {
                             lblStatus.Visible = false;
                             lblStatus.BackColor = _matchingPass.I == 1 ? Color.Green : Color.Red;
                             lblStatus.Text = _matchingPass.I == 1 ? "OK" : "NG";
                         }*/

                        string result = _matchingPass.I == 1 ? "OK" : "NG";

                        // Cập nhật giao diện
                        if (lblStatus.InvokeRequired)
                            lblStatus.Invoke(new MethodInvoker(() =>
                            {
                                lblStatus.Visible = true;
                                lblStatus.BackColor = result == "OK" ? Color.Green : Color.Red;
                                lblStatus.Text = result;
                            }));
                        else
                        {
                            lblStatus.Visible = true;
                            lblStatus.BackColor = result == "OK" ? Color.Green : Color.Red;
                            lblStatus.Text = result;
                        }
/*
                        // Gửi kết quả về server
                        if (_socket != null && _socket.Connected)
                        {
                            try
                            {
                                byte[] data = Encoding.ASCII.GetBytes(result + "\n");
                                _socket.Send(data);
                            }
                            catch (Exception ex)
                            {

                            }
                        }*/

                        // Gửi kết quả trở lại qua serial
                        if (_serialPort != null && _serialPort.IsOpen)
                        {
                            try
                            {
                                byte[] data = Encoding.ASCII.GetBytes(result + "\n");
                                _serialPort.Write(data, 0, data.Length);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Error: {ex.Message}", "Serial", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                }
            }
        }

        /// <summary>
        /// Luồng xử lý chính của chương trình, đọc trigger và chạy xử lý ảnh.
        /// </summary>
        private void RunProgramProcessPLC()
        {
            // Thiết lập cửa sổ Halcon
            if (HWindowsMain.InvokeRequired)
                HWindowsMain.Invoke(new MethodInvoker(() =>
                {
                    HWindowsMain.HalconWindow.SetDraw("margin");
                    HWindowsMain.HalconWindow.SetLineWidth(2);
                }));
            else
            {
                HWindowsMain.HalconWindow.SetDraw("margin");
                HWindowsMain.HalconWindow.SetLineWidth(2);
            }

            while (_isRun)
            {
                try
                { 
                    // 1. Đọc tín hiệu trigger từ D120
                    int triggerSignal = _plcClient.ReadWord(PLC_TRIGGER_REGISTER, out success);

                    if (triggerSignal.ToString() == _triggerValuePLC) // Nếu D120= trigger value (1)
                        {
                        // 2. Bật D101 (Busy) = 1
                        _plcClient.WriteInt(PLC_BUSY_REGISTER, 1);
                        _plcClient.WriteInt(PLC_FINISH_REGISTER, 0);
                        _plcClient.WriteInt(PLC_TRIGGER_REGISTER, 0); // Reset D120 để xác nhận đã nhận trigger

                        // 3. Chụp ảnh
                        _hImage = GlobFunc.SnapImage(_frameGrabber);

                        if (_hImage == null || _hImage.Key == IntPtr.Zero)
                        {
                            // Nếu không chụp được ảnh, báo NG và tắt Busy
                            _plcClient.WriteInt(PLC_NG_REGISTER, 1); // Báo NG
                            Thread.Sleep(50); // Đợi PLC kịp đọc
                            _plcClient.WriteInt(PLC_FINISH_REGISTER, 0); // Bật Finish
                            _plcClient.WriteInt(PLC_BUSY_REGISTER, 0); // Tắt Busy
                            continue;
                        }

                        // 4. Chạy thuật toán Matching
                        if (RunMatchingModel(out string errMessage))
                        {
                            // Cập nhật hiển thị Halcon Window
                            if (HWindowsMain.InvokeRequired)
                                HWindowsMain.Invoke(new MethodInvoker(() =>
                                {
                                    HWindowsMain.HalconWindow.SetColor(_matchingPass.I == 1 ? "green" : "red");
                                    HWindowsMain.HalconWindow.ClearWindow();
                                    HWindowsMain.HalconWindow.DispImage(_hImage);
                                    if (_matchingOut_ContoursFind != null && _matchingOut_ContoursFind.Key != IntPtr.Zero)
                                        HWindowsMain.HalconWindow.DispObj(_matchingOut_ContoursFind);
                                }));
                            else
                            {
                                HWindowsMain.HalconWindow.SetColor(_matchingPass.I == 1 ? "green" : "red");
                                HWindowsMain.HalconWindow.ClearWindow();
                                HWindowsMain.HalconWindow.DispImage(_hImage);
                                if (_matchingOut_ContoursFind != null && _matchingOut_ContoursFind.Key != IntPtr.Zero)
                                    HWindowsMain.HalconWindow.DispObj(_matchingOut_ContoursFind);
                            }

                            string result = _matchingPass.I == 1 ? "OK" : "NG";

                            // Cập nhật giao diện (lblStatus)
                            if (lblStatus.InvokeRequired)
                                lblStatus.Invoke(new MethodInvoker(() =>
                                {
                                    lblStatus.Visible = true;
                                    lblStatus.BackColor = result == "OK" ? Color.Green : Color.Red;
                                    lblStatus.Text = result;
                                }));
                            else
                            {
                                lblStatus.Visible = true;
                                lblStatus.BackColor = result == "OK" ? Color.Green : Color.Red;
                                lblStatus.Text = result;
                            }

                            // 5. Gửi kết quả về PLC (D109 hoặc D110)
                            if (_plcClient != null && _plcClient.IsConnected)
                            {
                                if (result == "OK")
                                {
                                    _plcClient.WriteInt(PLC_OK_REGISTER, 1); // Ghi 1 vào D109
                                }
                                else
                                {
                                    _plcClient.WriteInt(PLC_NG_REGISTER, 1); // Ghi 1 vào D110
                                }
                                Thread.Sleep(50); // Đợi một chút để PLC kịp đọc kết quả

                                // 6. Bật D102 (Finish) và Tắt D101 (Busy) 
                                _plcClient.WriteInt(PLC_FINISH_REGISTER, 1);
                                _plcClient.WriteInt(PLC_BUSY_REGISTER, 0);

                            }
                        }
                        else
                        {
                            MessageBox.Show($"Error", "Error");
                        }
                    }
                    Thread.Sleep(10); 
                }
                catch (Exception e)
                {
                    MessageBox.Show($"Error", "Error");
                }
            }
        }


        private void StopProgram()
        {
            if (!_isRun)
                return;

            _isRun = false;
            _threadRun?.Join();
            _threadRun = null;

            /* try
             {
                 _threadReadTrigger.Abort();
                 _threadReadTrigger = null;
             }
             catch
             {
             }

             DisconnectTCP();*/
            DisconnectSerial();

            GlobFunc.DisconnectCamera(_frameGrabber);

            lblStatus.Visible = false;
            btnModelManager.Enabled = !_isRun;
            btnVisionSettings.Enabled = !_isRun;

            btnStop.Enabled = _isRun;
            btnStart.Enabled = !_isRun;
        }

        /// <summary>
        /// Dừng chương trình và ngắt kết nối PLC.
        /// </summary>
        private void StopProgramPLC()
        {
            if (!_isRun)
                return;

            _isRun = false;
            // Dừng luồng xử lý chính
            _threadRun?.Join(); 
            _threadRun = null;

            // Dừng luồng Ready Signal
            if (_threadReadySignal != null && _threadReadySignal.IsAlive)
            {
                // _isRun = false sẽ khiến vòng lặp trong ReadySignalProcess kết thúc
                _threadReadySignal.Join(); // Chờ luồng kết thúc
            }
            _threadReadySignal = null;

            DisconnectPLC(); 

            GlobFunc.DisconnectCamera(_frameGrabber);

            // Cập nhật trạng thái UI
            if (lblStatus.InvokeRequired)
            {
                lblStatus.Invoke(new MethodInvoker(() => lblStatus.Visible = false));
            }
            else
            {
                lblStatus.Visible = false;
            }

            btnModelManager.Enabled = !_isRun;
            btnVisionSettings.Enabled = !_isRun;
            btnStop.Enabled = _isRun;
            btnStart.Enabled = !_isRun;
        }
        /// <summary>
        /// Phương thức kiểm tra trạng thái kết nối PLC 
        /// </summary>
        private void ReadySignalProcess()
        {
            try
            {
                while (_isRun && _plcClient != null && _plcClient.IsConnected)
                {
                    // Ghi 1 vào D100
                    _plcClient.WriteInt(PLC_READY_REGISTER, 1);
                    Thread.Sleep(500); // Đợi 500ms

                    if (!_isRun || _plcClient == null || !_plcClient.IsConnected) break; // Kiểm tra lại sau khi chờ

                    // Ghi 0 vào D100
                    _plcClient.WriteInt(PLC_READY_REGISTER, 0);
                    Thread.Sleep(500); // Đợi 500ms
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi trong luồng Ready Signal: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Đảm bảo D100 về 0 khi luồng kết thúc (nếu có thể)
                if (_plcClient != null && _plcClient.IsConnected)
                {
                    _plcClient.WriteInt(PLC_READY_REGISTER, 0);
                }
            }
        }
        private void RunProgram()
        {
            if (!LoadModelInfo())
                return;
            /* if (!ConnectTCP())
                 return;*/
            if (!ConnectSerial()) 
                 return;

           /* // 1. Kết nối PLC
            if (!ConnectPLC())
            {
                MessageBox.Show("Failed to connect to PLC.", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Đặt PLC_READY_REGISTER về 0 trước khi khởi động luồng Ready Signal
            if (_plcClient != null && _plcClient.IsConnected)
            {
                _plcClient.WriteInt(PLC_READY_REGISTER, 0);
            }
*/

            if (!GlobFunc.ConnectCamera(_interfaceName, _deviceName, out _frameGrabber, out string errMessage))
            {
                MessageBox.Show(errMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (_matchingOut_ModelID == null || _matchingOut_ModelID.H == null ||
                _matchingOut_ModelID.H == IntPtr.Zero)
            {
                MessageBox.Show("Run Matching Failed!!!.\nPlease train before run.", "Warning", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            _realTriggerValue = string.Empty;
            _isRun = true;

           /* //PLC
            // Khởi động luồng Ready Signal
            _threadReadySignal = new Thread(ReadySignalProcess);
            _threadReadySignal.IsBackground = true; // Đặt là luồng nền để nó tự kết thúc khi ứng dụng đóng
            _threadReadySignal.Start();*/


            //TCP
            //_threadReadTrigger = new Thread(ReadTriggerValue);
            //_threadReadTrigger.Start();

            //Serial
            _threadRun = new Thread(RunProgramProcess);
            _threadRun.Start();

            btnModelManager.Enabled = !_isRun;
            btnVisionSettings.Enabled = !_isRun;
            btnStop.Enabled = _isRun;
            btnStart.Enabled = !_isRun;
        }

        private void RunProgramPLC()
        {
            if (!LoadModelInfo())
                return;
            /* if (!ConnectTCP())
                 return;*/
            /*if (!ConnectSerial()) 
                 return;*/

            // 1. Kết nối PLC
            if (!ConnectPLC())
            {
                MessageBox.Show("Failed to connect to PLC.", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Reset các thanh ghi PLC về 0 trước khi ngắt kết nối
            if (_plcClient != null && _plcClient.IsConnected)
            {
                _plcClient.WriteInt(PLC_READY_REGISTER, 0);
                _plcClient.WriteInt(PLC_BUSY_REGISTER, 0);
                _plcClient.WriteInt(PLC_FINISH_REGISTER, 0);
                _plcClient.WriteInt(PLC_TRIGGER_REGISTER, 0);
                _plcClient.WriteInt(PLC_OK_REGISTER, 0);
                _plcClient.WriteInt(PLC_NG_REGISTER, 0);
            }




            if (!GlobFunc.ConnectCamera(_interfaceName, _deviceName, out _frameGrabber, out string errMessage))
            {
                MessageBox.Show(errMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (_matchingOut_ModelID == null || _matchingOut_ModelID.H == null ||
                _matchingOut_ModelID.H == IntPtr.Zero)
            {
                MessageBox.Show("Run Matching Failed!!!.\nPlease train before run.", "Warning", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            _realTriggerValue = string.Empty;
            _isRun = true;

            //PLC
            // Khởi động luồng Ready Signal
            _threadReadySignal = new Thread(ReadySignalProcess);
            _threadReadySignal.IsBackground = true; // Đặt là luồng nền để nó tự kết thúc khi ứng dụng đóng
            _threadReadySignal.Start();


            //TCP
            //_threadReadTrigger = new Thread(ReadTriggerValue);
            //_threadReadTrigger.Start();

            //Serial
            _threadRun = new Thread(RunProgramProcessPLC);
            _threadRun.Start();

            btnModelManager.Enabled = !_isRun;
            btnVisionSettings.Enabled = !_isRun;
            btnStop.Enabled = _isRun;
            btnStart.Enabled = !_isRun;
        }
        #endregion

        #region Events
        private void FrmMain_Load(object sender, EventArgs e)
        {
            try
            {
                GlobVar.LockEvents = true;             
                LoadAllModels();
                GlobVar.Settings = new ProgramSetting();
                GlobVar.Settings.ReadSettings();
            }
            finally
            {
                GlobVar.LockEvents = false;
            }
        }
        #endregion

        private void btnVisionSettings_Click(object sender, EventArgs e)
        {
            FrmVisionSettings frmVisionSettings = new FrmVisionSettings();
            frmVisionSettings.CurrentModelId = _currentModelId;
            frmVisionSettings.ShowDialog();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            RunProgram();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            StopProgram();
        }
        private void HWindowsMain_Load(object sender, EventArgs e)
        {
            this.MouseWheel += HWindowsMain.HSmartWindowControl_MouseWheel;
        }

        private void btnStartPLC_Click(object sender, EventArgs e)
        {
            RunProgramPLC();
        }

        private void btnStopPLC_Click(object sender, EventArgs e)
        {
            StopProgramPLC();
        }
    }
}