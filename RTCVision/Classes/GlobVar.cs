using HalconDotNet;
using RTCVision.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTCVision
{
    static class GlobVar
    {
        public static ProgramSetting Settings = null;
        public static bool LockEvents = false;
        public static bool IsRun = false;
        /// <summary>
        /// Engine nhằm khởi tạo môi trường làm việc cho HALCON
        /// </summary>
        public static HDevEngine MyEngine;
        public static bool DebugMode = true;
        public static HImage Image = null;

        public static string ChooseRadio = "";
    }
}
