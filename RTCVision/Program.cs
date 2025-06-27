using RTCVision.Classes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RTCVision
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Lib.SetupEnvironment();
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            GlobVar.Settings = new ProgramSetting();
            GlobVar.Settings.ReadSettings();


            CultureInfo customCulture =
                (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            customCulture.NumberFormat.NumberGroupSeparator = ",";

            Thread.CurrentThread.CurrentCulture = customCulture;

            Application.Run(new FrmMain());
        }
    }
}
