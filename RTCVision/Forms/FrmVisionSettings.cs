using DevExpress.XtraEditors;
using HalconDotNet;
using RTCDahuaSdk;
using RTCVision.Consts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

namespace RTCVision
{
    public partial class FrmVisionSettings : DevExpress.XtraEditors.XtraForm
    {
        public FrmVisionSettings()
        {
            InitializeComponent();
        }
        #region VARIABLES

        private const int _roundNumber = 3;

        private Guid _currentModelId;

        public Guid CurrentModelId
        {
            get => _currentModelId;
            set
            {
                _currentModelId = value;
                LoadModelInfo();
            }
        }

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
        
        private string _ipTCP = "127.0.0.1";
        private int _portTCP = 4000;
        private string _triggerTCP = "C";

        private string _comPort = "COM2";
        private int _baudRate = 9600;
        private string _triggerValue = "C";

        private string _plcIpAddress = "192.168.9.99";
        private int _plcPortNumber = 12234;
      

        private HWindow _Window;
        bool _isLive = false;
        public int ProgramID;
        int _olsPosition = 0;
        bool _isNew = false;

        private MyDahuaCamera _dahuaCamera = new MyDahuaCamera();
        private Thread _liveThread;

        #region PLC Register Definitions
        // Định nghĩa các thanh ghi PLC
        private string _Plc_Ready_Register = "D100";
        private string _Plc_Busy_Register = "D101";
        private string _Plc_Finish_Register = "D102";
        private string _Plc_Ok_Register = "D109";
        private string _Plc_Ng_Register = "D110";
        private string _Plc_Trigger_Register = "D120";
        #endregion

        #endregion


        #region FUNCTIONS

        private void SaveTriggerInfo()
        {
            _comPort = txtCom.Text; 
            _baudRate = Lib.ToInt(txtBaudrate.Text);
            _triggerValue = txtTriggerValue.Text;

            Lib.ExecuteQuery(
                $"DELETE FROM {CTableName.RS232Settings} WHERE {CColName.ModelId} = '{CurrentModelId}'");
            Lib.ExecuteQuery($"INSERT INTO {CTableName.RS232Settings} VALUES('{CurrentModelId}','{_comPort}',{_baudRate},'{_triggerValue}')");

            MessageBox.Show("RS232 Trigger settings saved successfully.", "Successfully!", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void SaveRegisterInfo()
        {
            _Plc_Ready_Register = txtReady.Text;
            _Plc_Busy_Register = txtBusy.Text;
            _Plc_Finish_Register = txtFinish.Text;
            _Plc_Ok_Register = txtOK.Text;
            _Plc_Ng_Register = txtNG.Text;
            _Plc_Trigger_Register = txtTriggerRegister.Text;
            _plcIpAddress = txtIpPLC.Text;
            _plcPortNumber = Lib.ToInt(txtPortPLC.Text);


        Lib.ExecuteQuery(
                $"DELETE FROM RegisterSettings WHERE {CColName.ModelId} = '{CurrentModelId}'");

            string sql = $"INSERT INTO RegisterSettings VALUES('{CurrentModelId}','{_Plc_Ready_Register}','{_Plc_Busy_Register}','{_Plc_Finish_Register}','{_Plc_Ok_Register}','{_Plc_Ng_Register}','{_Plc_Trigger_Register}','{_plcIpAddress}',{_plcPortNumber})";
            Lib.ExecuteQuery(sql);
            MessageBox.Show("Register PLC settings saved successfully.", "Successfully!", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void SaveTriggerTCPInfo()
        { 
            _ipTCP = txtIpTCP.Text;
            _portTCP = Lib.ToInt(txtPortTCP.Text);
            _triggerTCP = txtTriggerTCP.Text;

            Lib.ExecuteQuery(
                $"DELETE FROM TCPSetting WHERE {CColName.ModelId} = '{CurrentModelId}'");
            string sql = $"INSERT INTO TCPSetting VALUES('{CurrentModelId}','{_ipTCP}',{_portTCP},'{_triggerTCP}')";
            Lib.ExecuteQuery(sql);
            MessageBox.Show("TCP Trigger settings saved successfully.", "Successfully!", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void LoadTriggerInfo()
        {
            _comPort = Lib.ExecuteScalar($"SELECT {CColName.ComPort} FROM {CTableName.RS232Settings} WHERE {CColName.ModelId}='{CurrentModelId}'").ToString();
            _baudRate = Lib.ToInt(Lib.ExecuteScalar(
                $"SELECT {CColName.BaudRate} FROM {CTableName.RS232Settings} WHERE {CColName.ModelId}='{CurrentModelId}'"));
            _triggerValue = Lib.ExecuteScalar($"SELECT {CColName.TriggerValue} FROM {CTableName.RS232Settings} WHERE {CColName.ModelId}='{CurrentModelId}'").ToString();

            txtCom.Text = _comPort;
            txtBaudrate.Text = _baudRate.ToString();
            txtTriggerValue.Text = _triggerValue;
        }

        private void LoadRegisterInfo()
        {
              _Plc_Ready_Register = Lib.ExecuteScalar($"SELECT {CColName.ReadyRegister} FROM RegisterSettings WHERE {CColName.ModelId}='{CurrentModelId}'").ToString();
              _Plc_Busy_Register = Lib.ExecuteScalar($"SELECT {CColName.BusyRegister} FROM RegisterSettings WHERE {CColName.ModelId}='{CurrentModelId}'").ToString();
              _Plc_Finish_Register = Lib.ExecuteScalar($"SELECT {CColName.FinishRegister} FROM RegisterSettings WHERE {CColName.ModelId}='{CurrentModelId}'").ToString();
              _Plc_Ok_Register = Lib.ExecuteScalar($"SELECT {CColName.OKRegister} FROM RegisterSettings WHERE {CColName.ModelId}='{CurrentModelId}'").ToString();
              _Plc_Ng_Register = Lib.ExecuteScalar($"SELECT {CColName.NGRegister} FROM RegisterSettings WHERE {CColName.ModelId}='{CurrentModelId}'").ToString();
              _Plc_Trigger_Register = Lib.ExecuteScalar($"SELECT {CColName.TriggerRegister} FROM RegisterSettings WHERE {CColName.ModelId}='{CurrentModelId}'").ToString();
              _plcIpAddress = Lib.ExecuteScalar($"SELECT {CColName.IpPLC} FROM RegisterSettings WHERE {CColName.ModelId}='{CurrentModelId}'").ToString();
              _plcPortNumber = Lib.ToInt(Lib.ExecuteScalar($"SELECT {CColName.PortPLC} FROM RegisterSettings WHERE {CColName.ModelId}='{CurrentModelId}'"));


              txtReady.Text = _Plc_Ready_Register;
              txtBusy.Text = _Plc_Busy_Register;
              txtFinish.Text = _Plc_Finish_Register;
              txtOK.Text = _Plc_Ok_Register;
              txtNG.Text = _Plc_Ng_Register;
              txtTriggerRegister.Text = _Plc_Trigger_Register;
              txtIpPLC.Text = _plcIpAddress;
              txtPortPLC.Text = _plcPortNumber.ToString();
         }

        private void LoadTriggerTCPInfo()
        {
            _ipTCP = Lib.ExecuteScalar($"SELECT {CColName.IpTCP} FROM TCPSetting WHERE {CColName.ModelId}='{CurrentModelId}'").ToString();
            _portTCP = Lib.ToInt(Lib.ExecuteScalar(
                $"SELECT {CColName.PortTCP} FROM TCPSetting WHERE {CColName.ModelId}='{CurrentModelId}'"));
            _triggerTCP = Lib.ExecuteScalar($"SELECT {CColName.TriggerTCP} FROM TCPSetting WHERE {CColName.ModelId}='{CurrentModelId}'").ToString();

            txtIpTCP.Text = _ipTCP;
            txtPortTCP.Text = _portTCP.ToString();
            txtTriggerTCP.Text = _triggerTCP;
        }


        /// <summary>
        /// Lấy thông tin model hiển thị lên cửa sổ
        /// </summary>
        private void LoadModelInfo()
        {
            string modelName = Lib.ExecuteScalar($"SELECT ModelName FROM Models WHERE ID='{CurrentModelId}'").ToString();
            txtModelName.Text = modelName;
            // Lấy thông tin camera
            LoadCameraInfo();

            LoadAllRegions();

            ViewRoiInfo(true);

            EnableDisableControls();

            LoadMatchingOptions();

            LoadDict();

            LoadTriggerInfo();

            LoadRegisterInfo();

            LoadTriggerTCPInfo();

        }
        /// <summary>
        /// Lấy thông tin camera
        /// </summary>
        private void LoadCameraInfo()
        {
            _interfaceName = Lib.ExecuteScalar(
                 $"SELECT InterfaceName FROM ModelCam WHERE ModelID='{CurrentModelId}'").ToString();

            _deviceName = Lib.ExecuteScalar(
                $"SELECT DeviceName FROM ModelCam WHERE ModelID='{CurrentModelId}'").ToString();

            cbInterface.Text = _interfaceName;
            cbDevice.Text = _deviceName;
        }

        private void ViewMatchingOptions()
        {
            txtMatchingMinScore.Text = _matchingMinScore.ToString();
            txtMatchingNumMatches.Text = _matchingNumMatches.ToString();
        }
        /// <summary>
        /// Lấy thông số thiết lập matching
        /// </summary>
        private void LoadMatchingOptions()
        {
            DataTable dataTable =
                Lib.GetTableData($"SELECT * FROM {CTableName.Matching} WHERE {CColName.ModelId} = '{CurrentModelId}'");
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

            ViewMatchingOptions();
        }

        private void LoadDict()
        {
            _matchingOut_ModelID = new HTuple();

            string hdictFileName = Path.Combine(Application.StartupPath, "Iconic.hdict");

            HTuple dict = null;

            if (File.Exists(hdictFileName))
            {
                HOperatorSet.ReadDict(hdictFileName, new HTuple(), new HTuple(), out dict);
                HOperatorSet.GetDictTuple(dict, CurrentModelId.ToString() + "MatchingModelID", out _matchingOut_ModelID);
                HOperatorSet.GetDictObject(out _matchingImageMaster, dict, CurrentModelId.ToString() + "MatchingImageMaster");
            }
        }
        /// <summary>
        /// Lấy toàn bộ các camera đang kết nối trên máy
        /// </summary>
        /// <returns></returns>
        private List<string> GetAvailableInterface()
        {
            _devices = new List<Device>();

            // Detect the HALCON binary folder
            List<string> availableInterfaces = new List<string>();
            string halconRoot = Environment.GetEnvironmentVariable("HALCONROOT");
            string halconArch = Environment.GetEnvironmentVariable("HALCONARCH");

            string a = halconRoot + "/bin/" + halconArch;

            // Querry all available interfaces
            var acquisitionInterfaces = Directory.EnumerateFiles(a, "hacq*.dll");

            // For each Interface (check for non XL version) we test with InfoFramegrabber if devices are connected
            foreach (string item in acquisitionInterfaces)
            {
                //HOperatorSet.CountSeconds(out HTuple StartTime);
                // Extract the interface name with an regular expression
                string interfaceName = Regex.Match(item, "hAcq(.+)(?:\\.dll)").Groups[1].Value;
                if (interfaceName == "DirectFile") continue;
                if (interfaceName == "File")
                {
                    Device device = new Device
                    {
                        InterfaceName = interfaceName,
                        DeviceName = cbDevice.Text
                    };
                    _devices.Add(device);

                    availableInterfaces.Add(interfaceName);
                    continue;
                }
                try
                {
                    // Querry available devices
                    HTuple devices;
                    HOperatorSet.InfoFramegrabber(interfaceName, "info_boards", out HTuple info, out devices);
                    //HInfo.InfoFramegrabber();
                    // In case that devices were found add it to the available interfaces
                    if (devices.Length > 0)
                    {
                        foreach (var itemDevice in devices.SArr)
                        {
                            Device device = new Device
                            {
                                InterfaceName = interfaceName,
                                DeviceName = itemDevice
                            };
                            _devices.Add(device);
                        }
                        availableInterfaces.Add(interfaceName);
                    }
                }
                catch (Exception ex)
                { }

            }

            return availableInterfaces;
        }

        private void LoadAllRegions()
        {
            _matchingTrainRegion = null;
            _matchingFindRegion = null;
            DataTable dataTable =
                Lib.GetTableData($"SELECT * FROM {CTableName.Regions} WHERE {CColName.ModelId} = '{CurrentModelId}'");
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

        private void EnableDisableControls_MatchingFind()
        {
            btnViewRoiFindM.Enabled = _matchingFindRegion != null;
            btnAddRoiFindM.Enabled = _matchingFindRegion == null;
            btnDeleteRoiFindM.Enabled = _matchingFindRegion != null;
        }
        private void EnableDisableControls_MatchingTrain()
        {
            btnViewRoiTrainM.Enabled = _matchingTrainRegion != null;
            btnAddRoiTrainM.Enabled = _matchingTrainRegion == null;
            btnDeleteRoiTrainM.Enabled = _matchingTrainRegion != null;
        }
        private void EnableDisableControls()
        {
            EnableDisableControls_MatchingTrain();
            EnableDisableControls_MatchingFind();
        }
        /// <summary>
        /// Lấy thông tin của roi khi di chuyển, thay đổi kích thước
        /// </summary>
        /// <param name="drawingObject">Đối tượng Roi đang tác động</param>
        /// <param name="hWindow">Cửa sổ hiển thị</param>
        /// <param name="type">Kiểu roi</param>
        private void GetPosition(HDrawingObject drawingObject, HWindow hWindow, string type)
        {
            HTuple values = new HTuple(drawingObject.GetDrawingObjectParams(new HTuple(new string[] { "row", "column", "phi", "length1", "length2" })));
            double[] arrPosition = values.ToDArr();

            switch (_currentRegionType)
            {
                case CRegionType.MTrain:
                    {
                        _matchingTrainRegion.Row = Math.Round(arrPosition[0], _roundNumber);
                        _matchingTrainRegion.Col = Math.Round(arrPosition[1], _roundNumber);
                        _matchingTrainRegion.Phi = Math.Round(arrPosition[2], _roundNumber);
                        _matchingTrainRegion.Width = Math.Round(arrPosition[3], _roundNumber);
                        _matchingTrainRegion.Height = Math.Round(arrPosition[4], _roundNumber);

                        ViewRoiInfo_MTrain();
                        break;
                    }
                case CRegionType.MFind:
                    {
                        _matchingFindRegion.Row = Math.Round(arrPosition[0], _roundNumber);
                        _matchingFindRegion.Col = Math.Round(arrPosition[1], _roundNumber);
                        _matchingFindRegion.Phi = Math.Round(arrPosition[2], _roundNumber);
                        _matchingFindRegion.Width = Math.Round(arrPosition[3], _roundNumber);
                        _matchingFindRegion.Height = Math.Round(arrPosition[4], _roundNumber);
                        ViewRoiInfo_MFind();
                        break;
                    }
            }
        }

        private void ViewRoiInfo_MFind()
        {
            txtMFindRoiR.Enabled = true;
            txtMFindRoiC.Enabled = true;
            txtMFindRoiPhi.Enabled = true;
            txtMFindRoiW.Enabled = true;
            txtMFindRoiH.Enabled = true;

            if (_matchingFindRegion != null)
            {
                txtMFindRoiR.Text = _matchingFindRegion.Row.ToString();
                txtMFindRoiC.Text = _matchingFindRegion.Col.ToString();
                txtMFindRoiPhi.Text = _matchingFindRegion.Phi.ToString();
                txtMFindRoiW.Text = _matchingFindRegion.Width.ToString();
                txtMFindRoiH.Text = _matchingFindRegion.Height.ToString();
            }
            else
            {
                txtMFindRoiR.Enabled = false;
                txtMFindRoiC.Enabled = false;
                txtMFindRoiPhi.Enabled = false;
                txtMFindRoiW.Enabled = false;
                txtMFindRoiH.Enabled = false;
            }
        }
        private void ViewRoiInfo_MTrain()
        {
            txtMTrainRoiR.Enabled = true;
            txtMTrainRoiC.Enabled = true;
            txtMTrainRoiPhi.Enabled = true;
            txtMTrainRoiW.Enabled = true;
            txtMTrainRoiH.Enabled = true;
            if (_matchingTrainRegion != null)
            {
                txtMTrainRoiR.Text = _matchingTrainRegion.Row.ToString();
                txtMTrainRoiC.Text = _matchingTrainRegion.Col.ToString();
                txtMTrainRoiPhi.Text = _matchingTrainRegion.Phi.ToString();
                txtMTrainRoiW.Text = _matchingTrainRegion.Width.ToString();
                txtMTrainRoiH.Text = _matchingTrainRegion.Height.ToString();
            }
            else
            {
                txtMTrainRoiR.Enabled = false;
                txtMTrainRoiC.Enabled = false;
                txtMTrainRoiPhi.Enabled = false;
                txtMTrainRoiW.Enabled = false;
                txtMTrainRoiH.Enabled = false;
            }
        }
        private void ViewRoiInfo(bool all = false)
        {
            if (all)
            {
                ViewRoiInfo_MTrain();
                ViewRoiInfo_MFind();
            }
            else
                switch (_currentRegionType)
                {
                    case CRegionType.MTrain:
                        {
                            ViewRoiInfo_MTrain();
                            break;
                        }
                    case CRegionType.MFind:
                        {
                            ViewRoiInfo_MFind();
                            break;
                        }
                }
        }

        private void SnapImage()
        {
            //Cũ
             _hImage = GlobFunc.SnapImage(_frameGrabber);
             if (_hImage != null && _hImage.Key != IntPtr.Zero)
             {
                 Lib.SmartSetPart(_hImage, HWindowsMain);
                 HWindowsMain.HalconWindow.DispImage(_hImage);
             }

           
        }
        public void ResetControls()
        {
            _Window.ClearWindow();
        }
        public HWindow GetHalconWindow()
        {
            return _Window;
        }
        private void BackgroundWorkerLiveWebCam_DoWork(object sender,
            DoWorkEventArgs e)
        {
            while (_isLive)
            {
                HOperatorSet.GrabImageAsync(out var hObject, _frameGrabber, -1);
                if (hObject != null && hObject.Key != IntPtr.Zero)
                {
                    Lib.SmartSetPart(hObject, HWindowsMain);
                    HWindowsMain.HalconWindow.DispObj(hObject);
                }
                if (_interfaceName == "File")
                    Thread.Sleep(150);
            }
        }
        private void LiveCamera()
        {
            if (_isLive)
            {
                var backgroundWorker = new BackgroundWorker();
                backgroundWorker.DoWork += BackgroundWorkerLiveWebCam_DoWork;
                backgroundWorker.RunWorkerAsync();
            }
        }

        private void RunMatchingModel(bool withSnap = true)
        {
            try
            {
                if (withSnap)
                    SnapImage();

                if (_hImage == null || _hImage.Key == IntPtr.Zero)
                {
                    MessageBox.Show("Run Matching Failed!!!.\nImage can't empty.", "Warning", MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                if (_matchingOut_ModelID == null || _matchingOut_ModelID.H == null ||
                    _matchingOut_ModelID.H == IntPtr.Zero)
                {
                    MessageBox.Show("Run Matching Failed!!!.\nPlease train before run.", "Warning", MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }
                // Kiểm tra xem đã train hay chưa
                ViewRoiFindM();

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

                if (_matchingPass.I == 1)
                {
                    HWindowsMain.HalconWindow.SetColor("green");
                    HWindowsMain.HalconWindow.SetDraw("margin");
                    HWindowsMain.HalconWindow.SetLineWidth(2);
                    lblPassFailM.Text = "Passed";
                    lblPassFailM.ForeColor = Color.Green;
                }
                else
                {
                    HWindowsMain.HalconWindow.SetColor("red");
                    HWindowsMain.HalconWindow.SetDraw("margin");
                    HWindowsMain.HalconWindow.SetLineWidth(2);

                    lblPassFailM.Text = "Failed";
                    lblPassFailM.ForeColor = Color.Red;

                    string errMessage = GlobFunc.HTuple2Str(_matchingException);
                    if (!string.IsNullOrEmpty(errMessage))
                        MessageBox.Show($"Run Matching Failed!!!\n{errMessage}", "Warning", MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                    //else
                    //    MessageBox.Show("Run Matching Failed!!!", "Warning", MessageBoxButtons.OK,
                    //        MessageBoxIcon.Warning);

                }

                lblActualM.Text = "Actual: " + GlobFunc.HTuple2Str(_matchingOut_ActualNumberMatches);
                HWindowsMain.HalconWindow.ClearWindow();
                HWindowsMain.HalconWindow.DispImage(_hImage);
                if (_matchingOut_ContoursFind != null && _matchingOut_ContoursFind.Key != IntPtr.Zero)
                    HWindowsMain.HalconWindow.DispObj(_matchingOut_ContoursFind);
            }
            catch (HDevEngineException ex)
            {
                MessageBox.Show($"Run Matching Failed!!!\n{ex.Message}\n{ex.StackTrace}", "Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Run Matching Failed!!!\n{ex.Message}\n{ex.StackTrace}", "Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        private void TrainMatchingModel()
        {
            try
            {
                if (_hImage == null || _hImage.Key == IntPtr.Zero)
                {
                    MessageBox.Show("Train Matching Failed!!!.\nImage can't empty.", "Warning", MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                ViewRoiTrainM();

                HDevProcedure procedure = new HDevProcedure("Matching");
                HDevProcedureCall procedureCall = new HDevProcedureCall(procedure);

                procedureCall.SetInputIconicParamObject("Image", _hImage);
                procedureCall.SetInputIconicParamObject("In_Roi", _drawingObjectRegion.GetDrawingObjectIconic());

                procedureCall.SetInputCtrlParamTuple("Select_Mode", "Train");
                procedureCall.SetInputCtrlParamTuple("In_ModelID", _matchingIn_ModelID);
                procedureCall.SetInputCtrlParamTuple("In_MinScore", _matchingMinScore);
                procedureCall.SetInputCtrlParamTuple("In_NumMatches", _matchingNumMatches);
                procedureCall.SetInputCtrlParamTuple("In_Origin", _matchingIn_Origin);

                procedureCall.Execute();

                _matchingOut_Origin = procedureCall.GetOutputCtrlParamTuple("Out_Origin");
                _matchingOut_ModelID = procedureCall.GetOutputCtrlParamTuple("Out_ModelID");
                _matchingOut_Row = procedureCall.GetOutputCtrlParamTuple("Out_Row");
                _matchingOut_Column = procedureCall.GetOutputCtrlParamTuple("Out_Column");
                _matchingOut_Angle = procedureCall.GetOutputCtrlParamTuple("Out_Angle");
                _matchingOut_Score = procedureCall.GetOutputCtrlParamTuple("Out_Score");
                _matchingException = procedureCall.GetOutputCtrlParamTuple("Exception");
                _matchingPass = procedureCall.GetOutputCtrlParamTuple("Pass");

                _matchingOut_ContoursTrain = procedureCall.GetOutputIconicParamObject("Out_ContoursTrain");

                if (_matchingPass.I == 1)
                {
                    MessageBox.Show("Train Matching Success!!!", "Information", MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    HWindowsMain.HalconWindow.SetColor("cyan");
                    HWindowsMain.HalconWindow.SetDraw("margin");
                    HWindowsMain.HalconWindow.SetLineWidth(2);

                    HWindowsMain.HalconWindow.ClearWindow();
                    HWindowsMain.HalconWindow.DispImage(_hImage);
                    HWindowsMain.HalconWindow.DispObj(_matchingOut_ContoursTrain);

                    _matchingImageMaster = _hImage.CopyImage();
                }
                else
                {
                    if (!string.IsNullOrEmpty(_matchingException.S))
                        MessageBox.Show($"Train Matching Failed!!!\n{_matchingException.S}", "Warning", MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                    else
                        MessageBox.Show("Train Matching Failed!!!", "Warning", MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                }
            }
            catch (HDevEngineException ex)
            {
                MessageBox.Show($"{ex.Message}\n{ex.StackTrace}", "Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}\n{ex.StackTrace}", "Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void SaveMatchingData_Roi()
        {
            Lib.ExecuteQuery(
                $"DELETE FROM Regions WHERE {CColName.ModelId} = '{CurrentModelId}' AND {CColName.RegionType} = '{CRegionType.MTrain}'");
            if (_matchingTrainRegion != null)
                Lib.ExecuteQuery($"INSERT INTO Regions VALUES('{CurrentModelId}','{CRegionType.MTrain}'," +
                                 $" {_matchingTrainRegion.Row}, {_matchingTrainRegion.Col}," +
                                 $" {_matchingTrainRegion.Phi}, {_matchingTrainRegion.Width}, {_matchingTrainRegion.Height})");

            Lib.ExecuteQuery(
                $"DELETE FROM Regions WHERE {CColName.ModelId} = '{CurrentModelId}' AND {CColName.RegionType} = '{CRegionType.MFind}'");
            if (_matchingFindRegion != null)
                Lib.ExecuteQuery($"INSERT INTO Regions VALUES('{CurrentModelId}', '{CRegionType.MFind}'," +
                                 $" {_matchingFindRegion.Row}, {_matchingFindRegion.Col}," +
                                 $" {_matchingFindRegion.Phi}, {_matchingFindRegion.Width}, {_matchingFindRegion.Height})");
        }
        private void SaveMatchingData()
        {
            try
            {
                // Lưu trữ thông tin ROI
                SaveMatchingData_Roi();
                // Lưu trữ thông số matching
                Lib.ExecuteQuery(
                    $"DELETE FROM Matching WHERE {CColName.ModelId} = '{CurrentModelId}'");
                Lib.ExecuteQuery($"INSERT INTO Matching VALUES('{CurrentModelId}',{_matchingMinScore},{_matchingNumMatches})");

                // Lưu trữ thông tin train matching
                string hdictFileName = Path.Combine(Application.StartupPath, "Iconic.hdict");

                HTuple dict = null;

                if (File.Exists(hdictFileName))
                    HOperatorSet.ReadDict(hdictFileName, new HTuple(), new HTuple(), out dict);
                else
                    HOperatorSet.CreateDict(out dict);

                if (_matchingOut_ModelID == null)
                    _matchingOut_ModelID = new HTuple();

                HOperatorSet.SetDictTuple(dict, CurrentModelId.ToString() + "MatchingModelID", _matchingOut_ModelID);
                HOperatorSet.SetDictObject(_matchingImageMaster, dict, CurrentModelId.ToString() + "MatchingImageMaster");

                HOperatorSet.WriteDict(dict, hdictFileName, new HTuple(), new HTuple());

                MessageBox.Show("Save Matching Data Is Success", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception e)
            {
                MessageBox.Show($"{e.Message}\n{e.StackTrace}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        private void btnDetect_Click(object sender, EventArgs e)
        {
            cbInterface.Properties.Items.Clear();
            cbDevice.Properties.Items.Clear();

            // Load Halcon interfaces
            var interfaces = GetAvailableInterface();
            foreach (var item in interfaces)
                cbInterface.Properties.Items.Add(item);

           // Thêm Cam (Dahua)
            cbInterface.Properties.Items.Add("Cam SDK Dahua");

            if (cbInterface.Properties.Items.Count > 0)
                cbInterface.SelectedIndex = 0;

           
        }

        private void btnLive_Click(object sender, EventArgs e)
        {

            if (_interfaceName == "Cam SDK Dahua")
            {
                if (!_isLive)
                {
                    _isLive = true;
                    LiveCameraDahua();
                    btnSnap.Enabled = false;
                }
                else
                {
                    _isLive = false;
                    _liveThread?.Join();
                    btnSnap.Enabled = true;
                }
            }
            else //Halcon
            {
                if (_frameGrabber == null) return;
                if (!_isLive)
                {
                    _isLive = true;
                    LiveCamera();
                    btnSnap.Enabled = false;
                }
                else
                {
                    _isLive = false;
                    btnSnap.Enabled = true;
                }
            }
        }


        private void cbInterface_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedInterface = cbInterface.Text;
            cbDevice.Properties.Items.Clear();

            if (selectedInterface == "Cam SDK Dahua")
            {
                // Dahua camera
                var dahuaDevices = MyDahuaCamera.GetListDeviceInfoNames();
                foreach (var device in dahuaDevices)
                    cbDevice.Properties.Items.Add(device);

                if (cbDevice.Properties.Items.Count > 0)
                    cbDevice.SelectedIndex = 0;
            }
            else
            {
                // HALCON camera
                var devices = _devices.Where(o => o.InterfaceName == selectedInterface).ToList();
                foreach (var device in devices)
                    cbDevice.Properties.Items.Add(device.DeviceName);

                if (cbDevice.Properties.Items.Count > 0)
                    cbDevice.SelectedIndex = 0;
            }
        }

        private void btnConnectCamera_Click(object sender, EventArgs e)
        {
            string errMessage = string.Empty;

            _interfaceName = cbInterface.Text.Trim();
            _deviceName = cbDevice.Text.Trim();

            if (_interfaceName == "")
            {
                MessageBox.Show("Please choose an Interface!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (_deviceName == "")
            {
                MessageBox.Show("Please choose a Camera!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }



            if (_interfaceName == "Cam SDK Dahua")
            {
                if (btnConnectCamera.Text == "Connect")
                {
                    if (!_dahuaCamera.Open(_deviceName))
                    {
                        MessageBox.Show("Dahua connect failed");
                        return;
                    }
                    _dahuaCamera.GrabberMode = "ASync";
                    _dahuaCamera.StartGrabbing();
                    btnConnectCamera.Text = "Disconnect";
                    btnSnap.Enabled = btnLive.Enabled = true;
                    SnapImage(); // Snap ảnh đầu tiên
                }
                else
                {
                    _dahuaCamera.StopGrabbing();
                    _dahuaCamera.Close();
                    btnConnectCamera.Text = "Connect";
                    btnSnap.Enabled = btnLive.Enabled = false;
                }
            }
            else //Halcon
            {

                if (btnConnectCamera.Text.ToLower() == "Connect".ToLower())
                {
                    _interfaceName = cbInterface.Text;
                    _deviceName = cbDevice.Text;

                    if (!GlobFunc.ConnectCamera(_interfaceName, _deviceName, out _frameGrabber, out errMessage))
                    {
                        MessageBox.Show(errMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    btnConnectCamera.Text = "Disconnect";
                    btnConnectCamera.BackColor = Color.Green;
                    btnSnap.Enabled = true;
                    btnLive.Enabled = true;
                    SnapImage();
                }
                else
                {
                    if (_isLive)
                        btnLive_Click(null, null);

                    Thread.Sleep(1000);
                    GlobFunc.DisconnectCamera(_frameGrabber);

                    btnConnectCamera.Text = "Connect";
                    btnConnectCamera.BackColor = SystemColors.ButtonFace;

                    btnLive.BackColor = SystemColors.Control;
                    cbInterface.Enabled = cbDevice.Enabled = btnDetect.Enabled = true;
                    btnLive.Enabled = btnSnap.Enabled = false;
                    _hImage?.Dispose();
                    HWindowsMain.HalconWindow.DetachBackgroundFromWindow();
                    HWindowsMain.HalconWindow.ClearWindow();
                }

            }

        }
        private void my_MouseWheel(object sender, MouseEventArgs e)
        {
            Point pt = HWindowsMain.Location;
            MouseEventArgs newe = new MouseEventArgs(e.Button, e.Clicks, e.X - pt.X, e.Y - pt.Y, e.Delta);
            HWindowsMain.HSmartWindowControl_MouseWheel(sender, newe);
        }
        private void HWindowsMain_Load(object sender, EventArgs e)
        {
            _Window = HWindowsMain.HalconWindow;
            this.MouseWheel += my_MouseWheel;

        }

        private void btnSnap_Click(object sender, EventArgs e)
        {
            try
            {
                if (_interfaceName == "Cam SDK Dahua")
                {
                    SnapImageDahua();
                }
                else
                {
                    if (_frameGrabber == null)
                    {
                        MessageBox.Show("Please Connect To Camera Before Snap Image");
                        return;
                    }
                    SnapImage();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Snap Image Fail!!!\n" + ex.Message);
            }
        }

        private void FrmVisionSettings_Load(object sender, EventArgs e)
        {
            lblPassFailM.Text = string.Empty;
            // Đặt lại trạng thái radio theo GlobVar.ChooseRadio
            switch (GlobVar.ChooseRadio)
            {
                case "RS232":
                    rdRS232.Checked = true;
                    break;
                case "PLC":
                    rdPLC.Checked = true;
                    break;
                case "TCP":
                    rdTCP.Checked = true;
                    break;
                default:
                    rdTCP.Checked = true;
                    break;
            }

            UpdateGroupBoxState(); 
        }


        private void btnSaveCamera_Click(object sender, EventArgs e)
        {
            
            if (cbInterface.Text.Trim() == "")
             {
                 MessageBox.Show("Please choose an Interface.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                 return;
             }
             if (cbDevice.Text.Trim() == "")
             {
                 MessageBox.Show("Please choose a Camera.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                 return;
             }

             _interfaceName = cbInterface.Text;
             _deviceName = cbDevice.Text;

             DataTable dataTable = Lib.GetTableData($"SELECT * FROM {CTableName.ModelCam} WHERE {CColName.ModelId} = '{CurrentModelId}'");
             if (dataTable.Rows.Count == 0)
             {
                 if (Lib.ExecuteQuery(
                     $"INSERT INTO {CTableName.ModelCam} VALUES ('{Guid.NewGuid()}', '{CurrentModelId}', '{_interfaceName}','{_deviceName}','')") == 1)
                     MessageBox.Show(" Save Successful!!!");
                 else
                     MessageBox.Show("Save Camera Setting fail !");
             }
             else
             {
                 Guid camId = Lib.ToGuid(dataTable.Rows[0][CColName.Id]);
                 if (Lib.ExecuteQuery(
                     $"UPDATE {CTableName.ModelCam} SET {CColName.InterfaceName} = '{_interfaceName}', {CColName.DeviceName} = '{_deviceName}' WHERE {CColName.Id} = '{camId}'") == 1)
                     MessageBox.Show(" Save Successful!!!");
                 else
                     MessageBox.Show("Save Camera Setting fail !");
             }

           

        }

        private void FrmVisionSettings_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_isLive)
                btnLive_Click(null, null);
            Thread.Sleep(1000);
            GlobFunc.DisconnectCamera(_frameGrabber);

            DialogResult = DialogResult.OK;


            //Cam Dahua
            if (_isLive)
            {
                _isLive = false;
                _liveThread?.Join();
            }
            _dahuaCamera.StopGrabbing();
            _dahuaCamera.Close();
        }
        void creatModelRegion(int x, int y, double phi, int length1, int length2)
        {
            if (_drawingObjectRegion != null)
            {
                _drawingObjectRegion.Dispose();
                _drawingObjectRegion = null;
            }

            _drawingObjectRegion = HDrawingObject.CreateDrawingObject(HDrawingObject.HDrawingObjectType.RECTANGLE2, x, y, phi, length1, length2);

            _drawingObjectRegion.SetDrawingObjectParams("color", "red");
            _drawingObjectRegion.OnDrag(GetPosition);
            _drawingObjectRegion.OnResize(GetPosition);
            _Window.AttachDrawingObjectToWindow(_drawingObjectRegion);
        }

        #region ROI TRAIN MATCHING

        private void ViewRoiTrainM()
        {
            if (_drawingObjectRegion != null && _drawingObjectRegion.Handle != IntPtr.Zero)
            {
                HWindowsMain.HalconWindow.DetachDrawingObjectFromWindow(_drawingObjectRegion);
                _drawingObjectRegion = null;
            }

            if (_matchingTrainRegion == null)
                return;

            _currentRegionType = CRegionType.MTrain;
            _drawingObjectRegion = HDrawingObject.CreateDrawingObject(HDrawingObject.HDrawingObjectType.RECTANGLE2,
                _matchingTrainRegion.Row, _matchingTrainRegion.Col, _matchingTrainRegion.Phi, _matchingTrainRegion.Width, _matchingTrainRegion.Height);
            _drawingObjectRegion.OnDrag(GetPosition);
            _drawingObjectRegion.OnResize(GetPosition);
            _drawingObjectRegion.OnSelect(GetPosition);

            HWindowsMain.HalconWindow.AttachDrawingObjectToWindow(_drawingObjectRegion);
        }
        private void ViewRoiFindM()
        {
            if (_drawingObjectRegion != null && _drawingObjectRegion.Handle != IntPtr.Zero)
            {
                HWindowsMain.HalconWindow.DetachDrawingObjectFromWindow(_drawingObjectRegion);
                _drawingObjectRegion = null;
            }

            if (_matchingFindRegion == null)
                return;

            _currentRegionType = CRegionType.MFind;
            _drawingObjectRegion = HDrawingObject.CreateDrawingObject(HDrawingObject.HDrawingObjectType.RECTANGLE2,
                _matchingFindRegion.Row, _matchingFindRegion.Col, _matchingFindRegion.Phi, _matchingFindRegion.Width,
                _matchingFindRegion.Height);
            _drawingObjectRegion.OnDrag(GetPosition);
            _drawingObjectRegion.OnResize(GetPosition);
            _drawingObjectRegion.OnSelect(GetPosition);

            HWindowsMain.HalconWindow.AttachDrawingObjectToWindow(_drawingObjectRegion);
        }
        private void btnViewRoiTrainM_Click(object sender, EventArgs e)
        {
            ViewRoiTrainM();
        }
        private void btnAddRoiTrainM_Click(object sender, EventArgs e)
        {
            if (_drawingObjectRegion != null && _drawingObjectRegion.Handle != IntPtr.Zero)
                HWindowsMain.HalconWindow.DetachDrawingObjectFromWindow(_drawingObjectRegion);

            _matchingTrainRegion = new Region
            {
                Row = 0,
                Col = 0,
                Width = 100,
                Height = 100,
                Phi = 0
            };
            EnableDisableControls_MatchingTrain();
            ViewRoiTrainM();
            ViewRoiInfo();
        }
        private void btnDeleteRoiTrainM_Click(object sender, EventArgs e)
        {
            if (_drawingObjectRegion?.Handle != IntPtr.Zero)
                HWindowsMain.HalconWindow.DetachDrawingObjectFromWindow(_drawingObjectRegion);

            _matchingTrainRegion = null;
            EnableDisableControls_MatchingTrain();
            ViewRoiInfo();
        }
        #endregion

        private void btnViewRoiFindM_Click(object sender, EventArgs e)
        {
            ViewRoiFindM();
        }

        private void btnAddRoiFindM_Click(object sender, EventArgs e)
        {
            if (_drawingObjectRegion != null && _drawingObjectRegion.Handle != IntPtr.Zero)
                HWindowsMain.HalconWindow.DetachDrawingObjectFromWindow(_drawingObjectRegion);

            _matchingFindRegion = new Region
            {
                Row = 0,
                Col = 0,
                Width = 100,
                Height = 100,
                Phi = 0
            };
            EnableDisableControls_MatchingFind();
            ViewRoiFindM();
            ViewRoiInfo();
        }

        private void btnDeleteRoiFindM_Click(object sender, EventArgs e)
        {
            if (_drawingObjectRegion?.Handle != IntPtr.Zero)
                HWindowsMain.HalconWindow.DetachDrawingObjectFromWindow(_drawingObjectRegion);

            _matchingFindRegion = null;
            EnableDisableControls_MatchingFind();
            ViewRoiInfo();
        }

        private void txtMTrainRoiR_KeyDown(object sender, KeyEventArgs e)
        {
            if (GlobFunc.GetValueFromTextEdit(txtMTrainRoiR, out double value, true))
            {
                _matchingTrainRegion.Row = value;
                ViewRoiTrainM();
            }
            else
                ViewRoiInfo_MTrain();
        }
        private void txtMTrainRoiC_KeyDown(object sender, KeyEventArgs e)
        {
            if (GlobFunc.GetValueFromTextEdit(txtMTrainRoiC, out double value, true))
            {
                _matchingTrainRegion.Col = value;
                ViewRoiTrainM();
            }
            else
                ViewRoiInfo_MTrain();
        }
        private void txtMTrainRoiW_KeyDown(object sender, KeyEventArgs e)
        {
            if (GlobFunc.GetValueFromTextEdit(txtMTrainRoiW, out double value))
            {
                _matchingTrainRegion.Width = value;
                ViewRoiTrainM();
            }
            else
                ViewRoiInfo_MTrain();
        }
        private void txtMTrainRoiH_KeyDown(object sender, KeyEventArgs e)
        {
            if (GlobFunc.GetValueFromTextEdit(txtMTrainRoiH, out double value))
            {
                _matchingTrainRegion.Height = value;
                ViewRoiTrainM();
            }
            else
                ViewRoiInfo_MTrain();
        }

        private void txtMTrainRoiPhi_KeyDown(object sender, KeyEventArgs e)
        {
            if (GlobFunc.GetValueFromTextEdit(txtMTrainRoiH, out double value, true))
            {
                _matchingTrainRegion.Phi = value;
                ViewRoiTrainM();
            }
            else
                ViewRoiInfo_MTrain();
        }

        private void txtMFindRoiR_KeyDown(object sender, KeyEventArgs e)
        {
            if (GlobFunc.GetValueFromTextEdit(txtMFindRoiR, out double value, true))
            {
                _matchingFindRegion.Row = value;
                ViewRoiFindM();
            }
            else
                ViewRoiInfo_MFind();
        }

        private void txtMFindRoiC_KeyDown(object sender, KeyEventArgs e)
        {
            if (GlobFunc.GetValueFromTextEdit(txtMFindRoiC, out double value, true))
            {
                _matchingFindRegion.Col = value;
                ViewRoiFindM();
            }
            else
                ViewRoiInfo_MFind();
        }

        private void txtMFindRoiW_KeyDown(object sender, KeyEventArgs e)
        {
            if (GlobFunc.GetValueFromTextEdit(txtMFindRoiW, out double value))
            {
                _matchingFindRegion.Width = value;
                ViewRoiFindM();
            }
            else
                ViewRoiInfo_MFind();
        }

        private void txtMFindRoiH_KeyDown(object sender, KeyEventArgs e)
        {
            if (GlobFunc.GetValueFromTextEdit(txtMFindRoiH, out double value))
            {
                _matchingFindRegion.Height = value;
                ViewRoiFindM();
            }
            else
                ViewRoiInfo_MFind();
        }

        private void txtMFindRoiPhi_KeyDown(object sender, KeyEventArgs e)
        {
            if (GlobFunc.GetValueFromTextEdit(txtMFindRoiH, out double value, true))
            {
                _matchingFindRegion.Phi = value;
                ViewRoiFindM();
            }
            else
                ViewRoiInfo_MFind();
        }

        private void txtMatchingMinScore_KeyDown(object sender, KeyEventArgs e)
        {
            if (GlobFunc.GetValueFromTextEdit(txtMatchingMinScore, out double value, true))
                _matchingMinScore = value;
            else
                ViewMatchingOptions();
        }

        private void txtMatchingNumMatches_KeyDown(object sender, KeyEventArgs e)
        {
            if (GlobFunc.GetValueFromTextEdit(txtMatchingNumMatches, out int value, true))
                _matchingNumMatches = value;
            else
                ViewMatchingOptions();
        }

        private void btnTrainM_Click(object sender, EventArgs e)
        {
            TrainMatchingModel();
        }

        private void btnFindM_Click(object sender, EventArgs e)
        {
            RunMatchingModel(false);
        }

        private void btnSnapFindM_Click(object sender, EventArgs e)
        {
            RunMatchingModel();
        }

        private void btnSaveMatchingSettings_Click(object sender, EventArgs e)
        {
            SaveMatchingData();
        }

        private void btnViewImageMasterM_Click(object sender, EventArgs e)
        {
            if (_matchingImageMaster != null && _matchingImageMaster.Key != IntPtr.Zero)
            {
                Lib.SmartSetPart(_matchingImageMaster, HWindowsMain);
                HWindowsMain.HalconWindow.DispObj(_matchingImageMaster);
                _hImage = new HImage(_matchingImageMaster);
            }
        }

        private void btnSaveTrigger_Click(object sender, EventArgs e)
        {
            if (txtCom.Text.Trim() == "")
            {
                MessageBox.Show("Ip Address (Or Com) can't empty.", "Warning", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
            if (txtBaudrate.Text.Trim() == "")
            {
                MessageBox.Show("Port Number (Or Baudrate) can't empty.", "Warning", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
            if (!int.TryParse(txtBaudrate.Text.Trim(), out int portNumber))
            {
                MessageBox.Show("Port Number (Or Baudrate) must be a number.", "Warning", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
            if (txtTriggerValue.Text.Trim() == "")
            {
                MessageBox.Show("Trigger Value can't empty.", "Warning", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
            SaveTriggerInfo();
        }

       

        private void SnapImageDahua()
        {
            HImage image = _dahuaCamera.Snap(true); 

            if (image != null && image.Key != IntPtr.Zero)
            {
                _hImage = image;
                Lib.SmartSetPart(_hImage, HWindowsMain); 
                HWindowsMain.HalconWindow.DispImage(_hImage);
            }
            else
            {
                MessageBox.Show("Snap failed: " + _dahuaCamera.ErrorMessage);
            }
        }

        private void LiveCameraDahua()
        {
            _liveThread = new Thread(() =>
            {
                while (_isLive)
                {
                    var bmp = _dahuaCamera.GetLatestBitmap();
                    if (bmp != null)
                    {
                        var img = _dahuaCamera.Bitmap2HImage(bmp);
                        if (img != null && img.Key != IntPtr.Zero)
                        {
                            Invoke(new Action(() =>
                            {
                                Lib.SmartSetPart(img, HWindowsMain);
                                HWindowsMain.HalconWindow.DispImage(img);
                            }));
                        }
                    }
                    Thread.Sleep(30);
                }
            });
            _liveThread.IsBackground = true;
            _liveThread.Start();
        }

        private void Radio_CheckedChanged(object sender, EventArgs e)
        {
            System.Windows.Forms.RadioButton rd = sender as System.Windows.Forms.RadioButton;
            if (rd != null && rd.Checked)
            {
                GlobVar.ChooseRadio = rd.Text;
                UpdateGroupBoxState();
            }
        }
        private void UpdateGroupBoxState()
        {
            if (rdRS232.Checked)
            {
                grbRS232.Enabled = true;
                grbPLC.Enabled = false;
                grbTCP.Enabled = false;
            }
            else if (rdPLC.Checked)
            {
                grbRS232.Enabled = false;
                grbPLC.Enabled = true;
                grbTCP.Enabled = false;
            }
            else if (rdTCP.Checked)
            {
                grbRS232.Enabled = false;
                grbPLC.Enabled = false;
                grbTCP.Enabled = true;
            }
            else
            {
                grbRS232.Enabled = false;
                grbPLC.Enabled = false;
                grbTCP.Enabled = false;
            }
        }

        private void btnSaveResigter_Click(object sender, EventArgs e)
        {
            if (txtReady.Text.Trim() == "")
            {
                MessageBox.Show("Ready Register can't empty.", "Warning", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
            if (txtBusy.Text.Trim() == "")
            {
                MessageBox.Show("Busy Register can't empty.", "Warning", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
            if (txtFinish.Text.Trim() == "")
            {
                MessageBox.Show("Finish Register can't empty.", "Warning", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
            if (txtOK.Text.Trim() == "")
            {
                MessageBox.Show("OK Register can't empty.", "Warning", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
            if (txtNG.Text.Trim() == "")
            {
                MessageBox.Show("NG Register can't empty.", "Warning", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
            if (txtTriggerRegister.Text.Trim() == "")
            {
                MessageBox.Show("Trigger Register can't empty.", "Warning", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            SaveRegisterInfo();
        }

        private void btnSaveTriggerTCP_Click(object sender, EventArgs e)
        {
            if (txtIpTCP.Text.Trim() == "")
            {
                MessageBox.Show("Ip Address can't empty.", "Warning", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
            if (txtPortTCP.Text.Trim() == "")
            {
                MessageBox.Show("Port Number can't empty.", "Warning", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
            if (!int.TryParse(txtPortTCP.Text.Trim(), out int portNumber))
            {
                MessageBox.Show("Port Number must be a number.", "Warning", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
            if (txtTriggerTCP.Text.Trim() == "")
            {
                MessageBox.Show("Trigger Value can't empty.", "Warning", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
            SaveTriggerTCPInfo();
        }
 
    }

    public class Device
    {
        public string InterfaceName { get; set; }
        public string DeviceName { get; set; }
    }
    public class Region
    {
        public double Row { get; set; }
        public double Col { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Phi { get; set; }
    }
}