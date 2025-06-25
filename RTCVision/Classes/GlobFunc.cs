using DevExpress.XtraEditors;
using HalconDotNet;
using RTCVision.Consts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RTCVision
{
    static class GlobFunc
    {
        public static void DisconnectCamera(HTuple frameGrabber)
        {
            if (frameGrabber != null)
            {
                HOperatorSet.CloseFramegrabber(frameGrabber);
                frameGrabber = null;
            }
        }
        public static bool ConnectCamera(string interfaceName, string deviceName, out HTuple frameGrabber, out string errMessage)
        {
            errMessage = string.Empty;
            frameGrabber = null;

            try
            {
                HTuple generic = -1;
                if (interfaceName == "GigEVision2")
                {
                    //HOperatorSet.OpenFramegrabber(param[1], 0, 0, 0, 0, 0, 0, "progressive", -1, "default", "num_buffers=3", "default", "default", param[0], 0, -1, out HTuple Framegrabber);
                    //Nếu khi dải IP bị khác nhau giữa camera và máy tính thì phải lấy lại force_IP
                    HOperatorSet.InfoFramegrabber(interfaceName, "info_boards", out HTuple Information, out HTuple BoardInformation);
                    //* check the string for "misconfigured"
                    HOperatorSet.TupleRegexpTest(BoardInformation, "misconfigured", out HTuple Misconfig);
                    if (Misconfig)
                    {
                        //*now extract suggested Force ip, copy it and set to device
                        //*get the "force ip " string
                        HOperatorSet.TupleStrstr(BoardInformation, "force_ip", out HTuple PositionStart);
                        HOperatorSet.TupleStrLastN(BoardInformation, PositionStart, out generic);

                        HOperatorSet.OpenFramegrabber(interfaceName, 0, 0, 0, 0, 0, 0
                            , "progressive"
                            , -1
                            , "default"
                            , generic
                            , "default", interfaceName == "File" ? deviceName : "default"
                            , interfaceName == "File" ? "default" : deviceName
                            , 0
                            , -1
                            , out frameGrabber);
                        return true;
                    }
                    else
                    {
                        HOperatorSet.OpenFramegrabber(interfaceName, 0, 0, 0, 0, 0, 0
                            , "progressive", -1, "default", "num_buffers=3", "false", "default", deviceName, 0, -1, out frameGrabber);
                        return true;
                    }
                }
                else if (interfaceName == "DirectShow")
                {
                    HOperatorSet.OpenFramegrabber("DirectShow", 1, 1, 0, 0, 0, 0, "default", 8, "rgb",
                    -1, "false", "default", "[0] Integrated Webcam", 0, -1, out frameGrabber);
                    return true;
                }
                else
                {
                    HOperatorSet.OpenFramegrabber(interfaceName, 0, 0, 0, 0, 0, 0
                                                    , "progressive"
                                                    , -1
                                                    , "default"
                                                    , generic //generic
                                                    , "default", interfaceName == "File" ? deviceName : "default"
                                                    , interfaceName == "File" ? "default" : deviceName
                                                    , 0
                                                    , -1
                                                    , out frameGrabber);
                    return true;
                }
            }
            catch (Exception ex)
            {
                errMessage = $"{ex.Message}\n{ex.StackTrace}";
                frameGrabber = null;
                return false;
            }
        }
        public static HImage SnapImage(HTuple frameGrabber)
        {
            try
            {
                if (frameGrabber == null)
                    return null;
                HObject hObject = null;
                HOperatorSet.GrabImage(out hObject, frameGrabber);
                if (hObject != null)
                    return new HImage(hObject);
                return null;
            }
            catch
            {
                return null;
            }

        }
        public static bool GetValueFromTextEdit(TextEdit textEdit, out int value, bool acceptZeroValue = false)
        {
            value = 0;
            if (!int.TryParse(textEdit.Text, out int iValue))
                return false;
            else
            {
                if (iValue <= 0 && !acceptZeroValue)
                    return false;
                else
                {
                    value = iValue;
                    return true;
                }
            }
        }
        public static bool GetValueFromTextEdit(TextEdit textEdit, out double value, bool acceptZeroValue = false)
        {
            value = 0;
            if (!double.TryParse(textEdit.Text, out double dValue))
                return false;
            else
            {
                if (dValue <= 0 && !acceptZeroValue)
                    return false;
                else
                {
                    value = dValue;
                    return true;
                }
            }
        }
        public static string HTuple2Str(HTuple hTuple, int elementnumber = -1)
        {
            if (hTuple == null || hTuple.Length <= 0)
                return string.Empty;
            string Result = string.Empty;
            int _elementnumber = elementnumber == -1 ? hTuple.Length : elementnumber;
            for (int i = 0; i < _elementnumber; i++)
            {
                if (Result == string.Empty)
                    Result = He2Str(hTuple[i]);
                else
                    Result = Result + "," + He2Str(hTuple[i]);
            }

            return Result;
        }
        public static string He2Str(HTupleElements hTupleElements,
            bool trueFalseTo01 = false,
            bool noneZeroTo1 = false,
            int decimalPlaces = 6)
        {
            string Result = string.Empty;

            switch (hTupleElements.Type)
            {
                case HalconDotNet.HTupleType.EMPTY:
                    break;
                case HalconDotNet.HTupleType.INTEGER:
                    Result = hTupleElements.I.ToString();
                    break;
                case HalconDotNet.HTupleType.LONG:
                    Result = hTupleElements.L.ToString();
                    break;
                case HalconDotNet.HTupleType.DOUBLE:
                    Result = Math.Round(hTupleElements.D, decimalPlaces).ToString(CultureInfo.InvariantCulture);
                    break;
                case HalconDotNet.HTupleType.STRING:
                    Result = hTupleElements.S;
                    break;
                case HalconDotNet.HTupleType.HANDLE:
                    Result = hTupleElements.H.ToString();
                    break;
                case HalconDotNet.HTupleType.MIXED:
                    if (hTupleElements.O != null)
                        Result = hTupleElements.O.ToString();
                    break;
                default:
                    break;
            }

            if (trueFalseTo01) // Bo sung cho viec procedure tra ve chuoi true,false se duoc tu dong dua ve 1,0
            {
                if (Result.ToLower() == true.ToString().ToLower())
                    Result = "1";
                else if (Result.ToLower() == false.ToString().ToLower())
                    Result = "0";
            }

            if (noneZeroTo1 && Result.ToLower() != "0") // Bo sung cho viec procedure tra ve gia tri lon hon 1
                Result = "1";

            return Result;
        }

        public static void ShowWarning(string text)
        {
            MessageBox.Show(text, CDesignText.Warning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        public static void ShowError(string text)
        {
            MessageBox.Show(text, CDesignText.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
