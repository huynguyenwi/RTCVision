﻿using GemBox.Spreadsheet;
using HalconDotNet;
using RTCVision.Consts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RTCVision
{
    public class Multiwindow : HDevOpMultiWindowImpl
    {

        public int flag = 0, index = 0;
        public Multiwindow(params HWindow[] window) : base(window)
        {

        }

        public override void DevOpenWindow(HTuple row, HTuple column, HTuple width, HTuple height, HTuple background, out HTuple windowID)
        {
            base.DevOpenWindow(row, column, width, height, background, out windowID);
            //if (flag > 0)
            //{
            // windowID = windowIDs[index];
            // index++;
            //}
            //flag++;
        }

        //public override void DevSetWindow(HTuple windowHandle)
        //{
        // base.DevSetWindow(windowHandle);
        // foreach (HHandle item in windowIDs.HArr)
        // {
        // if (item==windowHandle)
        // {

        // }
        // }
        // if (flag > 0)
        // {
        // windowID = windowIDs[index];
        // index++;
        // }
        // flag++;
        //}
        public override void DevSetColor(HTuple colorName)
        {
            base.DevSetColor(colorName);

        }

    }
    public static class Lib
    {
        #region Variables
        #region FORMAT
        public const string CurrencyFormat = "###,###,###.00";
        public const string CurrencyFormatVND = "###,###,###,###";
        public const string FomatShortDate = "dd/MM/yyyy";
        public const string FormatLongDate = "dd/MM/yyyy HH:mm";
        #endregion

        #region MESSAGE
        public static string Caption = "[TÂN PHÁT] - Thông báo";
        #endregion

        #region DATETIME
        public static DateTime MIN_DATE = new DateTime(1950, 1, 1);
        #endregion

        private static string[] Number_Patterns = new string[] { "{0:#,##0}", "{0:#,##0.0}", "{0:#,##0.00}", "{0:#,##0}.000", "{0:#,##0.0000}",
            "{0:#,##0.00000;#,##0.00000; }" };

        private static string[] Currency_Patterns = new string[] { "{0:$#,##0;($#,##0); }", "{0:$#,##0.0;($#,##0.0); }", "{0:$#,##0.00;($#,##0.00); }",
            "{0:$#,##0.000;($#,##0.000); }", "{0:$#,##0.0000;($#,##0.0000); }", "{0:$#,##0.00000;($#,##0.00000); }" };

        #endregion Variables

        #region Contructors

        #endregion Contructors

        #region Methods
        public static string GetDeviceNameOriginCompact(string deviceNameOrigin)
        {
            try
            {
                string newDeviceNameOrigin = string.Empty;
                string[] devices = deviceNameOrigin.Split('|');
                for (int i = 0; i < devices.Length; i++)
                {
                    devices[i] = devices[i].Trim();
                    if (devices[i].StartsWith("unique_name:"))
                    {
                        devices = devices[i].Split(':');
                        if (devices.Length >= 2)
                            deviceNameOrigin = devices[1];
                        break;
                    }
                }
                if (newDeviceNameOrigin == string.Empty)
                    newDeviceNameOrigin = deviceNameOrigin;

                return newDeviceNameOrigin;
            }
            catch
            {
                return deviceNameOrigin;
            }
        }

        public static int ExecuteQuery(string sqlQuery)
        {
            try
            {
                SQLiteConnection con = new SQLiteConnection("Data Source=VisionDB.db;Version=3;");
                SQLiteCommand cmd = new SQLiteCommand();
                con.Open();
                cmd.Connection = con;
                cmd.CommandText = sqlQuery;//"delete from Student where ID = 0";
                cmd.ExecuteNonQuery();
                con.Close();
                return 1;
            }
            catch (Exception ex)
            {
                return -1;
            }
        }

        public static int ExecuteDeleteQuery(string table, string field, object value)
        {
            try
            {
                string sqlQuery = string.Format(@"delete from {0} where {1} = {2}", table, field, value);
                SQLiteConnection con = new SQLiteConnection("Data Source=VisionDB.db;Version=3;");
                SQLiteCommand cmd = new SQLiteCommand();
                con.Open();
                cmd.Connection = con;
                cmd.CommandText = sqlQuery;//"delete from Student where ID = 0";
                cmd.ExecuteNonQuery();
                con.Close();
                return 1;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public static object ExecuteScalar(string sqlQuery)
        {
            try
            {
                SQLiteConnection con = new SQLiteConnection("Data Source=VisionDB.db;Version=3;");
                SQLiteCommand cmd = new SQLiteCommand();
                con.Open();
                cmd.Connection = con;
                cmd.CommandText = sqlQuery;//"delete from Student where ID = 0";
                object value = cmd.ExecuteScalar();
                con.Close();
                return value;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public static bool AddCol(string tableName, string columnName, string type)
        {
            try
            {
                SQLiteConnection con = new SQLiteConnection("Data Source=VisionDB.db;Version=3;");
                con.Open();

                SQLiteCommand cmd = new SQLiteCommand();
                cmd.Connection = con;

                string sSQL = string.Format("ALTER TABLE {0} ADD {1} {2}", tableName, columnName, type);
                cmd.CommandText = sSQL;
                cmd.ExecuteNonQuery();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static DataTable GetTableData(string sqlQuery)
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            try
            {
                SQLiteConnection con = new SQLiteConnection("Data Source=VisionDB.db;Version=3;");
                SQLiteDataAdapter da = new SQLiteDataAdapter(sqlQuery, con);

                con.Open();
                da.Fill(ds);
                dt = ds.Tables[0];
                con.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return dt;
        }

        #region Chuyển kiểu, ép kiểu
        public static string ToString(object x)
        {
            if (x != null)
            {
                return x.ToString().Trim();
            }
            return "";
        }

        /// <summary>
        /// Chuyển giá trị sang kiểu integer
        /// </summary>
        /// <param name="x">giá trị cần chuyển</param>
        /// <returns></returns>
        public static int ToInt(object x)
        {
            try
            {
                return Convert.ToInt32(x);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Chuyển giá trị sang kiểu long
        /// </summary>
        /// <param name="x">giá trị cần chuyển</param>
        /// <returns></returns>
        public static Int64 ToInt64(object x)
        {
            try
            {
                return Convert.ToInt64(x);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Chuyển giá trị sang kiểu decimal
        /// </summary>
        /// <param name="x">giá trị cần chuyển</param>
        /// <returns></returns>
        public static Decimal ToDecimal(object x)
        {
            try
            {
                return Convert.ToDecimal(x);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Chuyển giá trị sang kiểu double
        /// </summary>
        /// <param name="x">giá trị cần chuyển</param>
        /// <returns></returns>
        public static Double ToDouble(object x)
        {
            try
            {
                return Convert.ToDouble(x);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Chuyển giá trị sang kiểu bool
        /// </summary>
        /// <param name="x">giá trị cần chuyển</param>
        /// <returns></returns>
        public static bool ToBoolean(object x)
        {
            try
            {
                return Convert.ToBoolean(x);
            }
            catch
            {
                return false;
            }
        }
        public static Guid ToGuid(object x)
        {
            try
            {
                if (x == null)
                    return Guid.Empty;
                else if (Guid.TryParse(x.ToString(), out Guid guid))
                    return guid;
                else
                    return Guid.Empty;
            }
            catch
            {
                return Guid.Empty;
            }
        }
        /// <summary>
        /// Chuyển giá trị chuỗi sang kiểu datetime
        /// </summary>
        /// <param name="date">chuỗi cần chuyển</param>
        /// <returns></returns>
        public static DateTime ToDate(string date)
        {
            try
            {
                try
                {
                    return DateTime.Parse(date, new CultureInfo("en-US", true));
                }
                catch
                {
                    return DateTime.Parse(date, new CultureInfo("fr-FR", true));
                }
            }
            catch
            {
                return Lib.MIN_DATE;
            }
        }

        public static DateTime ToDate1(string date)
        {
            try
            {
                try
                {
                    return DateTime.Parse(date, new CultureInfo("vi-VN", true));
                }
                catch
                {
                    return DateTime.Parse(date, new CultureInfo("fr-FR", true));
                }
            }
            catch
            {
                return Lib.MIN_DATE;
            }
        }

        public static DateTime? ToDate2(object x)
        {
            string date = "";
            if (x != null)
            {
                date = x.ToString();
            }
            try
            {
                try
                {
                    return DateTime.Parse(date, new CultureInfo("en-US", true));
                }
                catch
                {
                    return DateTime.Parse(date, new CultureInfo("fr-FR", true));
                }
            }
            catch
            {
                return null;
            }
        }

        public static DateTime ToDate3(object x)
        {
            string date = "";
            if (x != null)
            {
                date = x.ToString();
            }
            try
            {
                try
                {
                    return DateTime.Parse(date, new CultureInfo("en-US", true));
                }
                catch
                {
                    return DateTime.Parse(date, new CultureInfo("fr-FR", true));
                }
            }
            catch
            {
                return Lib.MIN_DATE;
            }
        }

        /// <summary>
        /// Chuyển giá trị datetime sang kiểu chuỗi ngày tháng định dạng Việt Nam
        /// </summary>
        /// <param name="date">Ngày cần chuyển</param>
        /// <returns></returns>
        public static string DateToStringVN(DateTime date)
        {
            return date.ToString("dd/MM/yyyy");
        }

        /// <summary>
        /// Chuyển dạng số thành dạng %
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static string FormatPercentNumber(Decimal x)
        {
            return String.Format("{0:#0.00}%", x);
        }

        #endregion Các hàm chuyển kiểu
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Xử lý Chuỗi

        /// <summary>
        /// Viết hoa chữ cái đầu của một chuỗi
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToUpperFC(string str)
        {
            return str.Substring(0, 1).ToUpper() + str.Substring(1);
        }

        /// <summary>
        /// Thao
        /// Đổi từ số sang chữ tiếng việt
        /// </summary>
        /// <param name="Num">Giá trị số cần chuyển</param>
        /// <returns></returns>
        public static string NumericToString(decimal Num)
        {
            string[] Dvi = { "", "ngàn", "triệu", "tỷ", "ngàn" };
            string Result = "";
            long IntNum = (long)Num;
            for (int i = 0; i < 5; i++)
            {
                long Doc = (long)IntNum % 1000;
                if (Doc > 0) Result = NumericToString(Doc, IntNum < 1000) + " " + Dvi[i] + "" + Result;
                IntNum = (long)IntNum / 1000;
            }
            Result += " đồng" + ((((long)Num) % 1000 == 0) ? " chẵn" : "");
            if (Result.ToString().Trim().Substring(0, 1) == "m" && Result.ToString().Trim().Substring(3, 1) == "i")
                Result = "Mười " + Result.Remove(0, 5);
            return Result.Length == 4 ? "..." : Result;
        }

        /// <summary>
        /// Đổi từ số sang chữ Tiếng Anh
        /// </summary>
        /// <param name="number">Giá trị số cần chuyển</param>
        /// <returns></returns>
        public static string NumberToWordsENG(int number)
        {
            if (number == 0)
                return "zero";

            if (number < 0)
                return "minus " + NumberToWordsENG(Math.Abs(number));

            string words = "";

            if ((number / 1000000) > 0)
            {
                words += NumberToWordsENG(number / 1000000) + " million ";
                number %= 1000000;
            }

            if ((number / 1000) > 0)
            {
                words += NumberToWordsENG(number / 1000) + " thousand ";
                number %= 1000;
            }

            if ((number / 100) > 0)
            {
                words += NumberToWordsENG(number / 100) + " hundred ";
                number %= 100;
            }

            if (number > 0)
            {
                if (words != "")
                    words += "and ";

                var unitsMap = new[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
                var tensMap = new[] { "zero", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

                if (number < 20)
                    words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                        words += "-" + unitsMap[number % 10];
                }
            }

            return words;
        }

        /// <summary>
        /// Thao
        /// Dùng để đổi dấu phân biệt hàng nghìn, hàng đơn vị trong các câu lệnh UPDATE
        /// </summary>
        /// <param name="strNumber">Giá trị số cần chuyển theo dạng chuỗi</param>
        /// <returns></returns>
        public static string DoiDau(string strNumber)
        {
            int length = 0; int pos = 0; string st = ""; string DoiDau1 = "";
            st = strNumber;
            pos = st.IndexOf(".", 0);
            while (pos > 0)
            {
                st = st.Substring(0, pos) + st.Remove(0, pos);
                pos = st.IndexOf(".", 0);
            }
            length = st.Length;
            pos = st.IndexOf(",", 0);
            if (pos > 0)
                DoiDau1 = st.Substring(0, pos) + "." + st.Remove(0, pos + 1);
            else
                DoiDau1 = st;

            return DoiDau1;
        }

        public static string ArrayToString(string separator, string[] arr)
        {
            if (arr.Length > 0)
            {
                return string.Join(separator, arr);
            }
            else
            {
                return "";
            }

        }
        #endregion
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Xử lý thời gian     

        /// <summary>
        /// Lấy giá trị chênh lệch giữa 2 mốc thời gian
        /// </summary>
        /// <param name="Interval">dạng giá trị cần lấy h: Số giờ, day: số ngày, month: số tháng, year: số năm</param>
        /// <param name="Date1">Ngày bắt đầu</param>
        /// <param name="Date2">Ngày kết thúc</param>
        /// <returns></returns>
        public static int DateDiff(string Interval, DateTime Date1, DateTime Date2)
        {
            int intDateDiff = 0;
            TimeSpan time = Date1 - Date2;
            int timeHours = Math.Abs(time.Hours);
            int timeDays = Math.Abs(time.Days);

            switch (Interval.ToLower())
            {
                case "h": // hours
                    intDateDiff = timeHours;
                    break;
                case "d": // days
                    time = Date1.Date - Date2.Date;
                    intDateDiff = Math.Abs(time.Days);
                    //intDateDiff = timeDays;
                    break;
                case "w": // weeks
                    intDateDiff = timeDays / 7;
                    break;
                case "bw": // bi-weekly
                    intDateDiff = (timeDays / 7) / 2;
                    break;
                case "m": // monthly
                    timeDays = timeDays - ((timeDays / 365) * 5);
                    intDateDiff = timeDays / 30;
                    break;
                case "bm": // bi-monthly
                    timeDays = timeDays - ((timeDays / 365) * 5);
                    intDateDiff = (timeDays / 30) / 2;
                    break;
                case "q": // quarterly
                    timeDays = timeDays - ((timeDays / 365) * 5);
                    intDateDiff = (timeDays / 90);
                    break;
                case "y": // yearly
                    intDateDiff = (timeDays / 365);
                    break;
            }

            return intDateDiff;
        }

        /// <summary>
        /// Thao
        /// Lấy số tuần của 1 năm
        /// </summary>
        /// <param name="Year">Năm cần lấy</param>
        /// <returns></returns>
        public static List<string> LoadAllWeekOfYear(int Year)
        {
            List<DateTime[]> weeks = new List<DateTime[]>();
            List<string> str = new List<string>();

            DateTime beginDate = new DateTime(Year, 01, 01);
            DateTime endDate = new DateTime(Year, 12, 31);

            DateTime monday = DateTime.Today;
            DateTime satday = DateTime.Today;

            while (beginDate < endDate)
            {
                beginDate = beginDate.AddDays(1);

                if (beginDate.DayOfWeek == DayOfWeek.Monday)
                {
                    monday = beginDate;
                }
                else if (beginDate.DayOfWeek == DayOfWeek.Saturday)
                {
                    satday = beginDate;
                }
                else if (beginDate.DayOfWeek == DayOfWeek.Sunday)
                {
                    weeks.Add(new DateTime[] { monday, satday });
                }
            }
            int count = 0;
            for (int x = 1; x < weeks.Count; x++)
            {
                if (x == 1)
                {
                    int startDay = weeks[x][0].Date.Day;
                    if (startDay >= 4)
                    {
                        str.Add("Tuần 1: " + "02/01/" + Year + " - 0" + (startDay - 2) + "/01/" + Year);
                        count = 1;
                    }
                }

                str.Add("Tuần " + (x + count) + ": " + (weeks[x][0]).ToString("dd/MM/yyyy") + " - " + (weeks[x][1]).ToString("dd/MM/yyyy"));

                if (x == weeks.Count - 1)
                {
                    int endDay = weeks[x][1].Date.Day;
                    if (endDay <= 29)
                    {
                        str.Add("Tuần " + (weeks.Count + count) + ": " + (endDay + 2) + "/01/" + Year + " - 31/01/" + Year);
                    }
                }
            }
            return str;
        }

        #endregion Cac ham xu li thoi gian
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////        
        #region Xứ lý với File, Folder

        /// <summary>
        /// Lưu trũ một file dựa trên giao diện lưu trữ của window
        /// </summary>
        /// <param name="title"></param>
        /// <param name="filter">Kiểu file (.doc,xls,...)</param>
        /// <param name="FileName">Tên file</param>
        /// <returns></returns>
        public static string ShowSaveFileDialog(string title, string filter, string FileName)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Title = title;
            dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            dlg.FileName = FileName;//name;
            dlg.Filter = filter;
            if (dlg.ShowDialog() == DialogResult.OK) return dlg.FileName;
            return "";
        }

        /// <summary>
        /// Mở một file
        /// </summary>
        /// <param name="fileName">đường dẫn file</param>
        public static void OpenFile(string fileName)
        {
            if (MessageBox.Show("Do you want to open file?", Caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    System.Diagnostics.Process process = new System.Diagnostics.Process();
                    process.StartInfo.FileName = fileName;
                    process.StartInfo.Verb = "Open";
                    process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
                    process.Start();
                }
                catch
                {
                    MessageBox.Show("File not found.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Kiểm tra xem đường dẫn có phải là 1 file không
        /// </summary>
        /// <param name="filePath">Đường dẫn</param>
        /// <returns></returns>
        public static bool IsFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Kiểm tra xem đường dẫn có phải là 1 thư mục không
        /// </summary>
        /// <param name="filePath">Đường dẫn</param>
        /// <returns></returns>
        public static bool IsFolder(string filePath)
        {
            if (Directory.Exists(filePath))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Copy file nhưng không ghi đề file trùng tên mà tạo ra một phiên bản mới. VD: a(1).txt
        /// </summary>
        /// <param name="sourceFile">Đường dẫn file nguồn</param>
        /// <param name="fileName">Tên file</param>
        /// <param name="destinationPath">Thư mục chứa file</param>
        /// <returns></returns>
        public static bool FileCopyWithoutOverwriting(string sourceFile, string fileName, string destinationPath)
        {
            // if destinationPath doesn't add with a slash, add one
            if ((destinationPath.EndsWith("\\") || destinationPath.EndsWith("/")) == false)
                destinationPath += "\\";

            try
            {
                // if file already exists in destination
                if (File.Exists(destinationPath + fileName))
                {
                    // counter
                    int count = 1;

                    // extract extension
                    FileInfo info = new FileInfo(sourceFile);
                    string ext = info.Extension;
                    string prefix;

                    // if it has an extension, append it to a .
                    if (ext.Length > 0)
                    {
                        // get filename without extension
                        prefix = fileName.Substring(0, fileName.Length - ext.Length);
                        //ext = "." + ext;
                    }
                    else
                        prefix = fileName;

                    // while not found an valid destination file name, increase counter
                    while (File.Exists(destinationPath + fileName))
                    {
                        fileName = prefix + "(" + count.ToString() + ")" + ext;
                        count++;
                    }
                    // copy file
                    File.Copy(sourceFile, destinationPath + fileName);
                }
                else
                {
                    File.Copy(sourceFile, destinationPath + fileName);
                }
                return true;
            }
            catch
            {
                return false;
            }

        }

        /// <summary>
        /// CreateFolder(Server.MapPath("Album/Folder1″)); sẽ tạo thư mục Folder1 trong thư mục Album của webroot
        /// </summary>
        /// <param name="strPath">Đường dẫn</param>
        public static void CreateFolder(string strPath)
        {

            try
            {

                if (Directory.Exists(strPath) == false)
                {

                    Directory.CreateDirectory(strPath);

                }

            }

            catch { }

        }
        /// <summary>
        /// RenameFolder(Server.MapPath("Album/Folder1″), Server.MapPath("Album/Folder2″)); 
        /// Sẻ đổi tên thư mục có tên Folder1 thành Folder2 trong thư mục Album của webroot
        /// </summary>
        /// <param name="strOldFolderName"></param>
        /// <param name="strNewFolderName"></param>
        public static void RenameFolder(string strOldFolderName, string strNewFolderName)
        {
            try
            {
                Directory.Move(strOldFolderName, strNewFolderName);
            }
            catch { }
        }
        /// <summary>
        /// Hàm xóa hết các thư mục và file bên trong một thư mục: 
        /// </summary>
        /// <param name="directoryInfo"></param>
        public static void EmptyFolder(DirectoryInfo directoryInfo)
        {

            try
            {
                foreach (FileInfo file in directoryInfo.GetFiles())
                {
                    file.Delete();
                }

                foreach (DirectoryInfo subfolder in directoryInfo.GetDirectories())
                {
                    //EmptyFolder(subfolder);
                    subfolder.Delete(true);
                }
            }
            catch { }

        }
        /// <summary>
        /// Hàm Copy thư mục này đến thư mục khác
        /// </summary>
        /// <param name="ThuMucNguon"></param>
        /// <param name="ThucMucDich"></param>
        public static void CopyDirectory(DirectoryInfo ThuMucNguon, DirectoryInfo ThucMucDich)
        {
            try
            {
                if (!ThucMucDich.Exists)
                {
                    ThucMucDich.Create();
                }

                FileInfo[] files = ThuMucNguon.GetFiles(); foreach (FileInfo file in files)
                {
                    if ((File.Exists(System.IO.Path.Combine(ThucMucDich.FullName, file.Name))) == false)
                    {
                        file.CopyTo(Path.Combine(ThucMucDich.FullName, file.Name));
                    }
                }

                //Xử lý thư mục con
                DirectoryInfo[] dirs = ThuMucNguon.GetDirectories();
                foreach (DirectoryInfo dir in dirs)
                {
                    string ThucMucDichDir = Path.Combine(ThucMucDich.FullName, dir.Name); CopyDirectory(dir, new DirectoryInfo(ThucMucDichDir));
                }
            }

            catch { }

        }
        /// <summary>
        /// Hàm này sẽ xóa thư mục mục và nội dung bên trong của thư mục được chọn
        /// </summary>
        /// <param name="strFolderName"></param>
        public static void DeleteFolder(string strFolderName)
        {
            DirectoryInfo ThuMucNguonDir = new DirectoryInfo(strFolderName);

            if (Directory.Exists(strFolderName))
            {
                try
                {
                    //EmptyFolder(ThuMucNguonDir);
                    EmptyFolder(ThuMucNguonDir);
                    Directory.Delete(strFolderName);
                }

                catch { }
            }
        }


        #endregion Xứ lý với File
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Control
        #region TextBox
        /// <summary>
        /// Thao
        /// Kiểm tra dữ liệu kiểu số khi nhập vào các ô textbox
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static bool CheckNumber(KeyPressEventArgs e)
        {
            if (!Char.IsControl(e.KeyChar) && !Char.IsNumber(e.KeyChar) && !Char.IsDigit(e.KeyChar) && e.KeyChar != '.' && e.KeyChar != ',' && (e.KeyChar != '-'))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Thao
        /// Kiểm tra dữ liệu kiểu số khi nhập vào các ô textbox
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static bool CheckInterger(KeyPressEventArgs e)
        {
            if (!Char.IsControl(e.KeyChar) && !Char.IsNumber(e.KeyChar) && !Char.IsDigit(e.KeyChar))
                return true;
            else
                return false;
        }

        #endregion TextBox

        #region Combobox

        /// <summary>
        /// Nhập dữ liệu dạng ArrayList vào Combobox
        /// </summary>
        /// <param name="item">tên combobox</param>
        /// <param name="list">dữ liệu</param>
        /// <param name="display">Trường hiển thị</param>
        /// <param name="value">Trường giá trị</param>
        public static void PopulateCombo(ComboBox item, ArrayList list, string display, string value)
        {
            if (list.Count > 0)
            {
                item.DataSource = list;
                item.DisplayMember = display;
                item.ValueMember = value;
            }
            else
            {
                item.DataSource = null;
                item.Items.Clear();
            }
            item.Focus();
        }

        /// <summary>
        /// Nhập dữ liệu dạng DataTable vào Combobox
        /// </summary>
        /// <param name="comboBox">tên combobox</param>
        /// <param name="data">dữ liệu</param>
        /// <param name="DisplayField">trường hiển thị</param>
        /// <param name="ValueField">Trường giá trị</param>
        /// <param name="NotSetItem">Giá trị khởi tạo</param>
        public static void PopulateCombo(ComboBox comboBox, DataTable data, string DisplayField, string ValueField, string NotSetItem)
        {
            if (NotSetItem != "")
            {
                DataRow dr = data.NewRow();
                dr[DisplayField] = NotSetItem;
                dr[ValueField] = 0;
                data.Rows.InsertAt(dr, 0);
            }
            comboBox.DataSource = data;
            comboBox.DisplayMember = DisplayField;
            comboBox.ValueMember = ValueField;
            comboBox.SelectedIndex = -1;
        }

        #endregion Combobox

        /// <summary>
        /// Reset tất cả các textbox trong form
        /// </summary>
        /// <param name="frm"></param>
        public static void ClearTexbox(Form frm)
        {
            foreach (Control ctrl in frm.Controls)
            {
                if (ctrl.GetType() == typeof(TextBox))
                {
                    ((TextBox)ctrl).Clear();
                }
            }
        }

        /// <summary>
        /// Reset tất cả các textbox trong các container
        /// </summary>
        /// <param name="ctrCollection"></param>
        static public void ClearTextBox(System.Windows.Forms.Control.ControlCollection ctrCollection)
        {
            foreach (object myObject in ctrCollection)
            {
                if (myObject is TextBox)
                {
                    ((TextBox)myObject).Clear();
                }
            }
        }

        /// <summary>
        /// Reset tất cả các textbox trong các container, chọn textbox sẽ được focus 
        /// </summary>
        /// <param name="ctrCollection">list container control</param>
        /// <param name="txtFocus">textbox sẽ được focus </param>
        static public void ClearTextBox(System.Windows.Forms.Control.ControlCollection ctrCollection, TextBox txtFocus)
        {
            ClearTextBox(ctrCollection);
            txtFocus.Focus();
        }
        #endregion Control
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Process Excel

        /// <summary>
        /// Lấy danh sách các tên sheet trong file excel
        /// </summary>
        /// <param name="filePath">Đường dẫn file Excel</param>
        /// <returns></returns>
        public static List<string> ListSheetInExcel(string filePath)
        {
            OleDbConnectionStringBuilder sbConnection = new OleDbConnectionStringBuilder();
            String strExtendedProperties = String.Empty;
            sbConnection.DataSource = filePath;
            if (Path.GetExtension(filePath).Equals(".xls"))//for 97-03 Excel file
            {
                sbConnection.Provider = "Microsoft.Jet.OLEDB.4.0";
                strExtendedProperties = "Excel 8.0;HDR=Yes;IMEX=1";//HDR=ColumnHeader,IMEX=InterMixed
            }
            else if (Path.GetExtension(filePath).Equals(".xlsx") || Path.GetExtension(filePath).Equals(".xlsm"))  //for 2007 Excel file
            {
                sbConnection.Provider = "Microsoft.ACE.OLEDB.12.0";
                strExtendedProperties = "Excel 12.0;HDR=Yes;IMEX=1";
            }
            sbConnection.Add("Extended Properties", strExtendedProperties);

            List<string> listSheet = new List<string>();
            OleDbConnection conn = new OleDbConnection(sbConnection.ToString());
            try
            {
                conn.Open();
                DataTable dtSheet = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                foreach (DataRow drSheet in dtSheet.Rows)
                {
                    if (drSheet["TABLE_NAME"].ToString().EndsWith("$") || drSheet["TABLE_NAME"].ToString().EndsWith("$'"))
                    {
                        listSheet.Add(drSheet["TABLE_NAME"].ToString().Replace("$", "").Replace("#", ".").Replace("'", ""));
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
            finally
            {
                conn.Close();
            }

            return listSheet;
        }

        /// <summary>
        /// Lấy dữ liệu file excel dưới dạng Datatable
        /// </summary>
        /// <param name="filename">Đường dẫn file Excel</param>
        /// <param name="sheetName">Tên sheet cần lấy dữ liệu</param>
        /// <returns></returns>
        public static DataTable ExcelToDatatable(string filename)
        {
            OleDbConnectionStringBuilder sbConnection = new OleDbConnectionStringBuilder();
            String strExtendedProperties = String.Empty;
            sbConnection.DataSource = filename;
            if (Path.GetExtension(filename).Equals(".xls"))//for 97-03 Excel file
            {
                sbConnection.Provider = "Microsoft.Jet.OLEDB.4.0";
                strExtendedProperties = "Excel 8.0;HDR=Yes;IMEX=1";//HDR=ColumnHeader,IMEX=InterMixed
            }
            else if (Path.GetExtension(filename).Equals(".xlsx") || Path.GetExtension(filename).Equals(".xlsm"))  //for 2007 Excel file
            {
                sbConnection.Provider = "Microsoft.ACE.OLEDB.12.0";
                strExtendedProperties = "Excel 12.0;HDR=Yes;IMEX=1";
            }
            sbConnection.Add("Extended Properties", strExtendedProperties);
            OleDbConnection conn = new OleDbConnection(sbConnection.ToString());
            DataTable dt = new DataTable();
            try
            {
                conn.Open();
                OleDbDataAdapter da = new OleDbDataAdapter("SELECT * FROM [" + ListSheetInExcel(filename)[0] + "$]", conn);
                DataSet ds = new DataSet();
                da.Fill(ds);
                dt = ds.Tables[0];
            }
            catch (Exception ex)
            {

            }
            finally
            {
                conn.Close();
            }

            return dt;
        }

        public static DataTable ExcelToDatatable(string filename, string sheetName)
        {
            OleDbConnectionStringBuilder sbConnection = new OleDbConnectionStringBuilder();
            String strExtendedProperties = String.Empty;
            sbConnection.DataSource = filename;
            //if (Path.GetExtension(filename).Equals(".xls"))//for 97-03 Excel file
            //{
            //    sbConnection.Provider = "Microsoft.Jet.OLEDB.4.0";
            //    strExtendedProperties = "Excel 8.0;HDR=Yes;IMEX=1";//HDR=ColumnHeader,IMEX=InterMixed
            //}
            //else //if (Path.GetExtension(filename).Equals(".xlsx"))  //for 2007 Excel file
            //{
            sbConnection.Provider = "Microsoft.ACE.OLEDB.12.0";
            strExtendedProperties = "Excel 12.0;HDR=Yes;IMEX=1";
            //}
            sbConnection.Add("Extended Properties", strExtendedProperties);
            OleDbConnection conn = new OleDbConnection(sbConnection.ToString());
            DataTable dt = new DataTable();
            try
            {
                conn.Open();

                //OleDbDataAdapter da = new OleDbDataAdapter("SELECT * FROM [" + ListSheetInExcel(filename)[0] + "$]", conn);
                OleDbDataAdapter da = new OleDbDataAdapter("SELECT * FROM [" + sheetName.Replace("'", "") + "$]", conn);
                DataSet ds = new DataSet();
                da.Fill(ds);
                dt = ds.Tables[0];
            }
            catch (Exception ex)
            {

            }
            finally
            {
                conn.Close();
            }

            return dt;
        }

        public static DataTable ExcelToDatatableNoHeader(string filename, string sheetName)
        {
            OleDbConnectionStringBuilder sbConnection = new OleDbConnectionStringBuilder();
            String strExtendedProperties = String.Empty;
            sbConnection.DataSource = filename;
            //if (Path.GetExtension(filename).Equals(".xls"))//for 97-03 Excel file
            //{
            //    sbConnection.Provider = "Microsoft.Jet.OLEDB.4.0";
            //    strExtendedProperties = "Excel 8.0;HDR=No;IMEX=1";//HDR=ColumnHeader,IMEX=InterMixed
            //}
            //else //if (Path.GetExtension(filename).Equals(".xlsx"))  //for 2007 Excel file
            //{
            sbConnection.Provider = "Microsoft.ACE.OLEDB.12.0";
            strExtendedProperties = "Excel 12.0;HDR=No;IMEX=1";
            //}
            sbConnection.Add("Extended Properties", strExtendedProperties);
            OleDbConnection conn = new OleDbConnection(sbConnection.ToString());
            DataTable dt = new DataTable();
            try
            {
                conn.Open();

                //OleDbDataAdapter da = new OleDbDataAdapter("SELECT * FROM [" + ListSheetInExcel(filename)[0] + "$]", conn);
                OleDbDataAdapter da = new OleDbDataAdapter("SELECT * FROM [" + sheetName.Replace("'", "") + "$]", conn);
                DataSet ds = new DataSet();
                da.Fill(ds);
                dt = ds.Tables[0];
            }
            catch (Exception ex)
            {
            }
            finally
            {
                conn.Close();
            }

            return dt;
        }

        public static void GridToCsv(DataGridView dGV, string filePathCSV)
        {
            string stOutput = "";
            string sHeaders = "";
            for (int j = 0; j < dGV.Columns.Count; j++)
                sHeaders = sHeaders.ToString() + Convert.ToString(dGV.Columns[j].HeaderText) + "\t";
            stOutput += sHeaders + "\r\n";
            // Export data.
            for (int i = 0; i < dGV.RowCount - 1; i++)
            {
                string stLine = "";
                for (int j = 0; j < dGV.Rows[i].Cells.Count; j++)
                    stLine = stLine.ToString() + dGV.Rows[i].Cells[j].Value.ToString() + "\t";
                stOutput += stLine + "\r\n";
            }
            FileStream fs = new FileStream(filePathCSV, FileMode.Create);
            StreamWriter bw = new StreamWriter(fs, Encoding.Unicode);
            bw.Write(stOutput); //write the encoded file
            bw.Flush();
            bw.Close();
            fs.Close();
        }

        public static void DatatableToCSV(DataTable dataTable, string filePathCSV)
        {
            string stOutput = "";
            string sHeaders = "";
            for (int j = 0; j < dataTable.Columns.Count; j++)
                sHeaders = sHeaders.ToString() + Convert.ToString(dataTable.Columns[j].Caption.Replace("\n", "")) + "\t";
            stOutput += sHeaders + "\r\n";
            // Export data.
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                string stLine = "";
                for (int j = 0; j < dataTable.Rows[i].ItemArray.Count(); j++)
                    stLine = stLine.ToString() + dataTable.Rows[i].ItemArray[j].ToString().Replace("\n", "") + "\t";
                stOutput += stLine + "\r\n";
            }
            FileStream fs = new FileStream(filePathCSV, FileMode.Create);
            StreamWriter bw = new StreamWriter(fs, Encoding.Unicode);
            bw.Write(stOutput); //write the encoded file
            bw.Flush();
            bw.Close();
            fs.Close();
        }
        #endregion Process Excel       

        #region Cac ham xu li khac


        /// <summary>
        /// Hàm trả địa chỉ IP hoặc tên máy hiện tại
        /// </summary>
        /// <param name="IP"></param>
        /// <returns>IP=true: Trả lại địa chỉ IP. IP=false: Trả lại tên máy</returns>
        public static string GetHostName()
        {
            return Dns.GetHostName();
        }

        /// <summary>
        /// Kiểm tra một chuối có phải là định dạng của email không
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsEmail(string str)
        {
            bool State = true;
            if (!Regex.IsMatch(str, @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$"))
            {
                State = false;
            }

            return State;
        }

        private static string NumberToString(long Num)
        {
            string[] Number = { "không", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín" };
            return Number[Num];
        }

        public static string NumberToStringH(long Num)
        {
            string[] Number = { "không", "Một", "Hai", "Ba", "Bốn", "Năm", "Sáu", "Bảy", "Tám", "Chín" };
            return Number[Num];
        }

        /// <summary>
        /// Chuyển kiểu số sang kiểu chữ tiếna việt
        /// </summary>
        /// <param name="Num"></param>
        /// <param name="Dau"></param>
        /// <returns></returns>
        public static string NumericToString(long Num, bool Dau)
        {
            long Tram = (long)Num / 100;
            long Chuc = (long)(Num % 100) / 10;
            long Dvi = Num % 10;

            string Doc = (((Tram == 0) && (Dau)) ? "" : (" " + NumberToString(Tram) + " trăm")) + ((Chuc == 0) ? (((Tram == 0) && Dau) ? "" : ((Dvi == 0) ? "" : " lẻ")) : ((Chuc == 1) ? " mười" : (" " + NumberToString(Chuc) + " mươi"))) + (((Dvi == 5) && (Chuc > 0)) ? " năm" : ((Dvi == 0) ? "" : " " + NumberToString(Dvi)));
            return Doc;
        }

        /// <summary>
        /// Định dạng số dưới dạng tiền Việt Nam
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static string FormatVND(decimal amount)
        {
            if (amount == 0) { return "0"; }
            else
            {
                return amount.ToString(Lib.CurrencyFormatVND);
            }
        }

        /// <summary>
        /// Hiển thị Thông báo lỗi
        /// </summary>
        /// <param name="ex"></param>
        public static void ShowError(Exception ex)
        {
            MessageBox.Show(ex.Message, Lib.Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static void ShowError(string content)
        {
            MessageBox.Show(content, Lib.Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static void ShowError(string content, Exception ex)
        {
            MessageBox.Show(content + Environment.NewLine + ex.Message, Lib.Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static void ReleaseComObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception)
            {
                obj = null;
            }
        }

        public static void EndProcess(string processName)
        {
            Process[] ps = Process.GetProcesses().Where(o => o.ProcessName.ToUpper().Contains(processName.ToUpper())).ToArray();
            foreach (Process p in ps)
            {
                p.Kill();
            }
        }
        #endregion

        #endregion

        #region Halcon
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
        public static int HTuple2Int(HTuple hTuple, int defaultValue = 0)
        {
            if (hTuple == null || hTuple.Length <= 0)
                return defaultValue;
            string value = He2Str(hTuple[0]);
            if (int.TryParse(value, out int iValue))
                return iValue;
            else
                return defaultValue;
        }
        public static void SmartSetPart(HObject Image, HSmartWindowControl HSWindow)
        {
            HOperatorSet.GetImageSize(Image, out HTuple width, out HTuple height);
            int wdWidth = HSWindow.Width;
            int wdHeight = HSWindow.Height;
            HSWindow.HalconWindow.GetWindowExtents(out int row, out int col, out wdWidth, out wdHeight);
            int imgRow, imgCol;
            int imgWidth, imgHeight;
            //
            if (((float)wdHeight / (float)wdWidth) > ((float)height / (float)width))
            {
                imgCol = 0;
                imgWidth = width;
                imgHeight = wdHeight * imgWidth / wdWidth;
                imgRow = (height - imgHeight) / 2;

            }
            else
            {
                imgRow = 0;
                imgHeight = height;
                imgWidth = wdWidth * imgHeight / wdHeight;
                imgCol = (width - imgWidth) / 2;
            }


            HSWindow.HalconWindow.SetPart(new HTuple(imgRow), new HTuple(imgCol), new HTuple(imgHeight + imgRow), new HTuple(imgWidth + imgCol));
        }

        public static void SmartSetPart(HObject image, HWindow hWindow)
        {
            try
            {
                HTuple width = null;
                HTuple height = null;
                HOperatorSet.GetImageSize(image, out width, out height);
                if (width == null || width.Length <= 0 || height == null || height.Length <= 0)
                    return;
                hWindow.GetWindowExtents(out int row, out int col, out int wdWidth, out int wdHeight);

                int imgRow, imgCol;
                int imgWidth, imgHeight;
                //
                if (((float)wdHeight / (float)wdWidth) > ((float)height / (float)width))
                {
                    imgCol = 0;
                    imgWidth = width;
                    imgHeight = wdHeight * imgWidth / wdWidth;
                    imgRow = (height - imgHeight) / 2;
                }
                else
                {
                    imgRow = 0;
                    imgHeight = height;
                    imgWidth = wdWidth * imgHeight / wdHeight;
                    imgCol = (width - imgWidth) / 2;
                }
                hWindow.SetPart(new HTuple(imgRow), new HTuple(imgCol), new HTuple(imgHeight + imgRow),
                    new HTuple(imgWidth + imgCol));
            }
            catch
            {
            }
        }
        public static void ViewImage(HImage image, HWindow hWindow)
        {
            hWindow.ClearWindow();
            if (image == null || image.Key == IntPtr.Zero)
                return;
            SmartSetPart(image, hWindow);
            hWindow.DispImage(image);
        }
        public static void ViewImage(HObject image, HWindow hWindow)
        {
            hWindow.ClearWindow();
            if (image == null || image.Key == IntPtr.Zero)
                return;
            SmartSetPart(image, hWindow);
            hWindow.DispObj(image);
        }
        public static void DisposeHImage(HImage image, bool isCreateEmptyObject = false)
        {
            if (image != null && image.Key != IntPtr.Zero)
            {
                image.Dispose();
                image = null;
            }

            if (isCreateEmptyObject)
            {
                HOperatorSet.GenEmptyObj(out HObject obj);
                image = new HImage(obj);
            }
        }
        public static void DisposeHObject(HObject hobject, bool isCreateEmptyObject = false)
        {
            if (hobject != null && hobject.Key != IntPtr.Zero)
            {
                hobject.Dispose();
                hobject = null;
            }

            if (isCreateEmptyObject)
            {
                HOperatorSet.GenEmptyObj(out HObject obj);
                hobject = new HObject(obj);
            }
        }
        public static void SetupEnvironment()
        {
            try
            {
                GlobVar.MyEngine = new HDevEngine();
                GlobVar.MyEngine.SetProcedurePath(Application.StartupPath + "/Procedures");
                //GlobVar.MyEngine.SetProcedurePath(@"C:\Users\Razer\source\repos\RTCVision\RTCVision\Procedures");
                GlobVar.MyEngine.SetEngineAttribute("debug_password", "1");
                GlobVar.MyEngine.SetEngineAttribute("debug_port", new HTuple(50));
                GlobVar.MyEngine.SetEngineAttribute("debug_wait_for_connection", "true");
                SpreadsheetInfo.SetLicense("EF21-1FW1-HWZF-CLQH");
            }
            catch (Exception ex)
            {
                SaveLog(ex);
            }

        }
        #endregion

        #region Log

        public static void SaveLog(string value)
        {
            try
            {
                if (string.IsNullOrEmpty(value))
                    return;
                value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\n" + value + "\n";
                string filename = @"Log\" + DateTime.Now.ToString("yyyy-MM-dd") + "_Log.txt";
                if (!Directory.Exists("Log"))
                    Directory.CreateDirectory("Log");
                using (StreamWriter sw = new StreamWriter(filename, true))
                    sw.WriteLine(value);
            }
            catch
            {
            }
        }
        public static void SaveLog(Exception ex)
        {
            if (ex == null)
                return;
            string value = ex.Message + "\n" + ex.StackTrace + "\n";
            SaveLog(value);
        }

        #endregion
        public static bool FileIsUsed(string filename)
        {
            bool Locked = false;
            try
            {
                FileStream fs =
                    File.Open(filename, FileMode.OpenOrCreate,
                        FileAccess.ReadWrite, FileShare.None);
                fs.Close();
            }
            catch (IOException)
            {
                Locked = true;
            }

            return Locked;
        }
        public static void SwapNum(ref int x, ref int y)
        {
            int tempswap = x;
            x = y;
            y = tempswap;
        }

        public static string GetLabelValue(Label label)
        {
            string value = string.Empty;
            if (label.InvokeRequired)
            {
                label.Invoke(new Action(() =>
                {
                    value = label.Text;
                }));
            }
            else
                value = label.Text;

            return value;
        }
        public static string GetTextBoxValue(TextBox textBox)
        {
            string value = string.Empty;
            if (textBox.InvokeRequired)
            {
                textBox.Invoke(new Action(() =>
                {
                    value = textBox.Text;
                }));
            }
            else
                value = textBox.Text;

            return value;
        }

        public static void SetTextBoxBackColor(TextBox textBox, Color backColor)
        {
            if (textBox.InvokeRequired)
            {
                textBox.Invoke(new Action(() =>
                {
                    textBox.BackColor = backColor;
                }));
            }
            else
                textBox.BackColor = backColor;
        }
        public static void SetTextBoxValue(TextBox textBox, string value)
        {
            if (textBox.InvokeRequired)
            {
                textBox.Invoke(new Action(() =>
                {
                    textBox.Text = value;
                }));
            }
            else
                textBox.Text = value;
        }
        public static string GetButtonValue(Button button)
        {
            if (button.InvokeRequired)
            {
                return (string)button.Invoke(new Func<String>(() => GetButtonValue(button)));
            }
            else
                return button.Text;

        }
        public static void SetButtonValue(Button button, string value)
        {
            if (button.InvokeRequired)
            {
                button.Invoke(new Action(() =>
                {
                    button.Text = value;
                }));
            }
            else
                button.Text = value;
        }
        public static void SetLabelValue(Label label, string value)
        {
            if (label.InvokeRequired)
            {
                label.Invoke(new Action(() =>
                {
                    label.Text = value;
                }));
            }
            else
                label.Text = value;
        }
        public static void SetLabelValueWithColorStatus(Label label, string value)
        {
            if (label.InvokeRequired)
            {
                label.Invoke(new Action(() =>
                {
                    label.Text = value;
                    label.ForeColor = Color.White;
                    if (value == cResultStatus.OK)
                        label.BackColor = cResultColorStatus.OK;
                    else if (value == cResultStatus.NG)
                        label.BackColor = cResultColorStatus.NG;
                    else if (value == cResultStatus.NOREAD)
                        label.BackColor = cResultColorStatus.NOREAD;
                    else if (value == cResultStatus.DUPLICATE)
                        label.BackColor = cResultColorStatus.DUPLICATE;
                    else if (value == cResultStatus.LOWCONFIDENCE)
                        label.BackColor = cResultColorStatus.LOWCONFIDENCE;
                    else if (value == cRunningStatus.ERROR)
                        label.BackColor = cRunningColorStatus.ERROR;
                    else if (value == cRunningStatus.NONE)
                    {
                        label.BackColor = cRunningColorStatus.NONE;
                        label.ForeColor = Color.Black;
                    }
                    else if (value == cRunningStatus.WAIT)
                    {
                        label.BackColor = cRunningColorStatus.WAIT;
                        label.ForeColor = Color.Black;
                    }
                }));
            }
            else
            {
                label.Text = value;
                label.ForeColor = Color.White;
                if (value == cResultStatus.OK)
                    label.BackColor = cResultColorStatus.OK;
                else if (value == cResultStatus.NG)
                    label.BackColor = cResultColorStatus.NG;
                else if (value == cResultStatus.NOREAD)
                    label.BackColor = cResultColorStatus.NOREAD;
                else if (value == cResultStatus.DUPLICATE)
                    label.BackColor = cResultColorStatus.DUPLICATE;
                else if (value == cRunningStatus.ERROR)
                    label.BackColor = cRunningColorStatus.ERROR;
                else if (value == cRunningStatus.NONE)
                {
                    label.BackColor = cRunningColorStatus.NONE;
                    label.ForeColor = Color.Black;
                }
                else if (value == cRunningStatus.WAIT)
                {
                    label.BackColor = cRunningColorStatus.WAIT;
                    label.ForeColor = Color.Black;
                }
            }
        }
        public static void EnableControl(Control control, bool isEnable = true)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(new Action(() =>
                {
                    control.Enabled = isEnable;
                }));
            }
            else
                control.Enabled = isEnable;
        }
        public static void VisibleControl(Control control, bool isEnable = true)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(new Action(() =>
                {
                    control.Visible = isEnable;
                }));
            }
            else
                control.Visible = isEnable;
        }
        public static double GetPercent(int value, int total)
        {
            if (total == 0 || value == 0)
                return 0;
            double percent = (double)value / (double)total;
            percent = percent * 100;
            return Math.Round(percent, 2);
        }
        public static int CurrentProgramID;
        public static string get_model_folder()
        {
            string dirPath = "";
            try
            {
                string ModelName = Lib.ExecuteScalar(string.Format("select ProgramName from Program where ID='{0}'", CurrentProgramID)).ToString();

                string currentDir = Environment.CurrentDirectory;
                dirPath = currentDir + "\\" + "Projects\\" + ModelName;

            }
            catch (Exception)
            {
            }
            return dirPath;
        }
    }
}