using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTCVision.Consts
{
    public class CTableName
    {
        public const string Models = "Models";
        public const string ModelCam = "ModelCam";
        public const string Regions = "Regions";
        public const string Matching = "Matching";
        public const string RS232Settings = "RS232Settings";
        public const string RegisterSettings = "RegisterSettings";
    }
    public class CColName
    {
        public const string Id = "Id";
        public const string ModelName = "ModelName";
        public const string CamQty = "CamQty";
        public const string OrderNum = "OrderNum";
        public const string CamName = "CamName";
        public const string ModelId = "ModelId";
        public const string ReadyRegister = "ReadyRegister";
        public const string BusyRegister = "BusyRegister";
        public const string FinishRegister = "FinishRegister";
        public const string OKRegister = "OKRegister";
        public const string NGRegister = "NGRegister";
        public const string TriggerRegister = "TriggerRegister";
        public const string ComPort = "ComPort";
        public const string BaudRate = "BaudRate";
        public const string TriggerValue = "TriggerValue";
        public const string IpPLC = "IpPLC";
        public const string PortPLC = "PortPLC";
        public const string IpTCP = "IpTCP";
        public const string PortTCP = "PortTCP";
        public const string TriggerTCP = "TriggerTCP";
        public const string RegionType = "RegionType";
        public const string InterfaceName = "InterfaceName";
        public const string DeviceName = "DeviceName";
        public const string Row = "Row";
        public const string Col = "Col";
        public const string Width = "Width";
        public const string Height = "Height";
        public const string Phi = "Phi";
        public const string In_MinScore = "In_MinScore";
        public const string In_NumMatches = "In_NumMatches";
        public const string Description = "Description";
    }
    public class CRegionType
    {
        public const string MTrain = "MTrain";
        public const string MFind = "MFind";
        public const string RTrain = "RTrain";
        public const string RFind = "RFind";
    }

    public class CDesignText
    {
        public const string Add = "Add";
        public const string Edit = "Edit";
        public const string Update = "Update";
        public const string New = "New";
        public const string Warning = "Warning";
        public const string Error = "Error";
        public const string Information = "Error";
    }

    public static class cMessageContent
    {
        public static string SendDelayTimeNeedGreaterThanOrEqual0 = "Send Delay Time need to be greater than or equal to 0";
        public static string TriggerNeedGreaterThan0 = "Number Of Trigger need to be greater than 0";
        public static string SendDelayTimeMustBeIntegerValue = "Send Delay Time only accepts integer values.\nPlease check again.";
        public static string ThereAreResultsOfUnconfirmedProducts = "There are results of unconfirmed products.\nPlease check again.";
        public static string DataExisted = "Data Existed.\nPlease check again.";
        public static string ResetCounterQuestion = "Do you want to reset counter?";
    }

    public static class cCaption
    {
        public static string WARNING = "WARNING";
        public static string QUESTION = "QUESTION";
    }

    public static class cResultStatus
    {
        public static string OK = "OK";
        public static string NG = "NG";
        public static string NOREAD = "NO READ";
        public static string DUPLICATE = "DUPLICATE";
        public static string LOWCONFIDENCE = "LOW CONFIDENCE";
    }
    public static class cRunningStatus
    {
        public static string NONE = "NONE";
        public static string WAIT = "WAIT";
        public static string ERROR = "ERROR";
    }
    public static class cResultColorStatus
    {
        /// <summary>
        /// Lime
        /// </summary>
        public static Color OK = Color.Lime;
        /// <summary>
        /// Red
        /// </summary>
        public static Color NG = Color.Red;
        /// <summary>
        /// Red
        /// </summary>
        public static Color NOREAD = Color.Red;
        /// <summary>
        /// Red
        /// </summary>
        public static Color DUPLICATE = Color.Red;
        public static Color LOWCONFIDENCE = Color.Red;
    }
    public static class cRunningColorStatus
    {
        /// <summary>
        /// White
        /// </summary>
        public static Color NONE = Color.White;
        /// <summary>
        /// Yellow
        /// </summary>
        public static Color WAIT = Color.Yellow;
        /// <summary>
        /// Red
        /// </summary>
        public static Color ERROR = Color.Red;
    }
}
