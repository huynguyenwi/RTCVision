using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RTCVision.Classes
{
    internal class ProgramSetting
    {
        public bool SaveOkImage { get; set; }
        public bool SaveOkGraphicImage { get; set; }
        public bool SaveNgImage { get; set; }
        public bool SaveNgGraphicImage { get; set; }
        public bool SaveLog { get; set; }
        public string SaveFolder { get; set; }
        public Guid CurrentModelId { get; set; }

        public ProgramSetting()
        {
            ReadSettings();
        }
        public void ReadDefault()
        {
            SaveOkImage = true;
            SaveOkGraphicImage = true;
            SaveNgImage = true;
            SaveNgGraphicImage = true;
            SaveLog = true;
            CurrentModelId = Guid.Empty;
            SaveFolder = Path.Combine(Application.StartupPath, "Result");
        }
        public void ReadSettings()
        {
            try
            {
                ReadDefault();
                DataTable dataTable = Lib.GetTableData("SELECT * FROM Settings");
                if (dataTable == null)
                    return;
                dataTable.PrimaryKey = new[] { dataTable.Columns["Name"] };

                DataRow row = dataTable.Rows.Find(nameof(SaveOkImage));
                if (row != null) SaveOkImage = Lib.ToBoolean(row["ValueT"]);

                row = dataTable.Rows.Find(nameof(SaveOkGraphicImage));
                if (row != null) SaveOkGraphicImage = Lib.ToBoolean(row["ValueT"]);

                row = dataTable.Rows.Find(nameof(SaveNgImage));
                if (row != null) SaveNgImage = Lib.ToBoolean(row["ValueT"]);

                row = dataTable.Rows.Find(nameof(SaveNgGraphicImage));
                if (row != null) SaveNgGraphicImage = Lib.ToBoolean(row["ValueT"]);

                row = dataTable.Rows.Find(nameof(SaveLog));
                if (row != null) SaveLog = Lib.ToBoolean(row["ValueT"]);

                row = dataTable.Rows.Find(nameof(SaveFolder));
                if (row != null) SaveFolder = Lib.ToString(row["ValueT"]);

                row = dataTable.Rows.Find(nameof(CurrentModelId));
                if (row != null) CurrentModelId = Lib.ToGuid(row["ValueT"]);

                if (!Directory.Exists(SaveFolder))
                    try
                    {
                        Directory.CreateDirectory(SaveFolder);
                    }
                    catch
                    {
                        SaveFolder = Path.Combine(Application.StartupPath, "Result");
                    }
                dataTable = null;
            }
            catch (Exception ex)
            {
                ReadDefault();
                Lib.SaveLog(ex);
            }
        }
        public void SaveSettings()
        {
            try
            {
                using (DataTable dataTable = Lib.GetTableData("SELECT * FROM Settings"))
                {
                    if (dataTable == null)
                        return;
                    dataTable.PrimaryKey = new[] { dataTable.Columns["Name"] };

                    DataRow row = dataTable.Rows.Find(nameof(SaveOkImage));
                    Lib.ExecuteQuery(
                        row != null
                            ? $"UPDATE Settings SET ValueT='{SaveOkImage.ToString().ToLower()}' WHERE Name='{nameof(SaveOkImage)}'"
                            : $"INSERT INTO Settings(Name,ValueT) VALUES ('{nameof(SaveOkImage)}','{SaveOkImage.ToString().ToLower()}')");

                    row = dataTable.Rows.Find(nameof(SaveOkGraphicImage));
                    Lib.ExecuteQuery(
                        row != null
                            ? $"UPDATE Settings SET ValueT='{SaveOkGraphicImage.ToString().ToLower()}' WHERE Name='{nameof(SaveOkGraphicImage)}'"
                            : $"INSERT INTO Settings(Name,ValueT) VALUES ('{nameof(SaveOkGraphicImage)}','{SaveOkGraphicImage.ToString().ToLower()}')");

                    row = dataTable.Rows.Find(nameof(SaveNgImage));
                    Lib.ExecuteQuery(
                        row != null
                            ? $"UPDATE Settings SET ValueT='{SaveNgImage.ToString().ToLower()}' WHERE Name='{nameof(SaveNgImage)}'"
                            : $"INSERT INTO Settings(Name,ValueT) VALUES ('{nameof(SaveNgImage)}','{SaveNgImage.ToString().ToLower()}')");

                    row = dataTable.Rows.Find(nameof(SaveNgGraphicImage));
                    Lib.ExecuteQuery(
                        row != null
                            ? $"UPDATE Settings SET ValueT='{SaveNgGraphicImage.ToString().ToLower()}' WHERE Name='{nameof(SaveNgGraphicImage)}'"
                            : $"INSERT INTO Settings(Name,ValueT) VALUES ('{nameof(SaveNgGraphicImage)}','{SaveNgGraphicImage.ToString().ToLower()}')");

                    row = dataTable.Rows.Find(nameof(SaveLog));
                    Lib.ExecuteQuery(
                        row != null
                            ? $"UPDATE Settings SET ValueT='{SaveLog.ToString().ToLower()}' WHERE Name='{nameof(SaveLog)}'"
                            : $"INSERT INTO Settings(Name,ValueT) VALUES ('{nameof(SaveLog)}','{SaveLog.ToString().ToLower()}')");

                    row = dataTable.Rows.Find(nameof(CurrentModelId));
                    Lib.ExecuteQuery(
                        row != null
                            ? $"UPDATE Settings SET ValueT='{CurrentModelId}' WHERE Name='{nameof(CurrentModelId)}'"
                            : $"INSERT INTO Settings(Name,ValueT) VALUES ('{nameof(CurrentModelId)}','{CurrentModelId}')");

                    row = dataTable.Rows.Find(nameof(SaveFolder));
                    Lib.ExecuteQuery(
                        row != null
                            ? $"UPDATE Settings SET ValueT='{SaveFolder}' WHERE Name='{nameof(SaveFolder)}'"
                            : $"INSERT INTO Settings(Name,ValueT) VALUES ('{nameof(SaveFolder)}','{SaveFolder}')");
                }
            }
            catch (Exception ex)
            {
                Lib.SaveLog(ex);
                MessageBox.Show("Save settings fail!");
            }
        }
    }
}
