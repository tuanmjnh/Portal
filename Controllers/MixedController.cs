using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TM.Message;
using TM.Helper;

namespace Portal.Controllers
{
    public class MixedController : BaseController
    {
        // GET: ConvertMoney
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult Index(string money)
        {
            return View();
        }
        [HttpGet]
        public ActionResult TXTToExcel()
        {
            var txt = TM.Common.Directories.orther + "Thang1vs2.txt";
            var file = TM.IO.FileDirectory.ReadFile(txt);
            var dt = new System.Data.DataTable();
            dt.Columns.Add("SOTB");
            dt.Columns.Add("NGAY_KH");
            dt.Columns.Add("CREATION_DATE");
            dt.Columns.Add("THOIHANSUDUNG");
            dt.Columns.Add("TEN_DK");
            dt.Columns.Add("TINHDANGKY");
            dt.Columns.Add("TENTINHDK");
            dt.Columns.Add("TINHKICHHOAT");
            foreach (var item in file)
            {
                var tmp = item.Replace("\"", "").Split(' ');
                var tmp0 = tmp[0].Split(',');
                var tmp3 = tmp[3].Split(',');
                if (tmp.Length == 4)
                    dt.Rows.Add(tmp0[0], tmp0[1], tmp[1], tmp[2], tmp3[0], tmp3[1], tmp3[2] + tmp3[3]);
                else if (tmp.Length > 4)
                    dt.Rows.Add(tmp0[0], tmp0[1], tmp[1], tmp[2], tmp3[0], tmp3[1], tmp3[2] + tmp3[3], tmp[4]);
            }
            TM.Exports.ExportExcel(dt, "Thang1vs2");
            return RedirectToAction("Index");
        }
        [HttpGet]
        public ActionResult CheckAddress()
        {
            TM.OleExcel.DataSource = TM.IO.FileDirectory.MapPath(TM.Common.Directories.orther + "danh_sach_can_map_theo_KT.XLS");
            var dt = TM.OleExcel.ToDataTable("SELECT * FROM [Sheet1$]");
            dt.Columns.Add("isMatch");
            foreach (System.Data.DataRow item in dt.Rows)
            {
                //Tổ
                var match1 = System.Text.RegularExpressions.Regex.Match(item["diachi_Kt"].ToString().ToLower(), "tæ \\d+").ToString();
                var match2 = System.Text.RegularExpressions.Regex.Match(item["diachi_tt"].ToString().ToLower(), "tæ \\d+").ToString();
                var match3 = System.Text.RegularExpressions.Regex.Match(item["diachi_Kt"].ToString().ToLower(), "th«n \\d+").ToString();
                var match4 = System.Text.RegularExpressions.Regex.Match(item["diachi_tt"].ToString().ToLower(), "th«n \\d+").ToString();
                if (string.IsNullOrEmpty(match1) && string.IsNullOrEmpty(match2))
                {
                    if (string.IsNullOrEmpty(match3) && string.IsNullOrEmpty(match4))
                        item["isMatch"] = "2";
                    else
                    {
                        if (match3 == match4)
                            item["isMatch"] = "1";
                        else
                            item["isMatch"] = "0";
                    }
                }
                else
                {
                    if (match1 == match2)
                        item["isMatch"] = "1";
                    else
                    {
                        item["isMatch"] = "0";
                    }
                }
            }
            TM.Exports.ExportExcel(dt, "danh_sach_can_map_theo_KT_Checked");
            return RedirectToAction("Index");
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult ConvertExcelToDbf()
        {
            try
            {
                TM.IO.FileDirectory.CreateDirectory(TM.Common.Directories.orther);
                var file = TM.IO.FileDirectory.Upload(Request.Files, TM.Common.Directories.orther, false, new[] { ".xls", ".xlsx" });
                if (file.UploadError().Count > 0)
                {
                    this.danger("Vui lòng chọn file .xls trước khi thực hiện!");
                    return RedirectToAction("Index");
                }
                var fileName = file.UploadFileString();
                var fileNameFull = TM.Common.Directories.orther + fileName;
                FileManagerController.InsertFile(fileNameFull);
                TM.OleExcel.DataSource = TM.IO.FileDirectory.MapPath(fileNameFull);
                var dbfFileName = TM.Helper.Strings.TMAscii2(fileName.Replace(".xls", "").Replace(".xlsx", "")).Replace("-", "");
                //Create Table DBF From Excel
                var dic = new System.Collections.Generic.Dictionary<string, string>();
                var listKey = new System.Collections.Generic.List<string>();
                var sheet = TM.OleExcel.GetSchemaTable().Rows[0]["TABLE_NAME"].ToString();
                var tb = TM.OleExcel.ToDataTable("SELECT * FROM [" + sheet + "]");
                var sql = "SELECT ";
                foreach (System.Data.DataColumn col in tb.Columns)
                {
                    var colName = TM.Helper.Strings.TMAscii2(col.ColumnName).Replace("-", "_");
                    if (listKey.Contains(colName)) colName = colName + listKey.Count;
                    if (colName.Length > 10) colName = colName.Substring(0, 8) + "_";
                    listKey.Add(colName);
                    sql += "MAX(LEN([" + col.ColumnName + "])),";
                }

                var tbs = TM.OleExcel.ToDataTable(sql.Trim(',') + " FROM [" + sheet + "]");
                for (int i = 0; i < tbs.Columns.Count; i++)
                    dic.Add(listKey[i], "c(" + (tbs.Rows[0][i].ToString() == "0" ? "1" : tbs.Rows[0][i].ToString()) + ")");

                TM.OleDBF.CreateTable(TM.Common.Directories.orther, dbfFileName, dic);
                var err = TM.OleDBF.ExportDBF2(TM.Common.Directories.orther, dbfFileName, tb);
                //var err = TM.OleDBF.ExportDBF(TM.Common.Directories.orther, dbfFileName, tb);
                //CreateFile(TM.Common.Directories.orther + dbfFileName + ".dbf");
                if (err.Count > 0)
                    this.danger(err.ToString());
                return DownloadFiles(TM.Common.Directories.orther + dbfFileName + ".dbf");
                //var excel = new TM.Interop.Excel(TM.IO.FileDirectory.MapPath(fileNameFull));
                //var a = excel.ToList();
            }
            catch (Exception ex)
            {
                this.danger(ex.Message);
            }
            return RedirectToAction("Index");
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult GetOffset(FormCollection collection)
        {
            try
            {
                TM.IO.FileDirectory.CreateDirectory(TM.Common.Directories.orther);
                var file = TM.IO.FileDirectory.Upload(Request.Files, TM.Common.Directories.orther, false, new[] { ".xls", ".xlsx" });
                if (file.UploadError().Count > 0)
                {
                    this.danger("Vui lòng chọn file .xls trước khi thực hiện!");
                    return RedirectToAction("Index");
                }
                var fileName = file.UploadFileString();
                var fileNameFull = TM.Common.Directories.orther + fileName;
                FileManagerController.InsertFile(fileNameFull);
                TM.OleExcel.DataSource = TM.IO.FileDirectory.MapPath(fileNameFull);
                var sheet = TM.OleExcel.GetSchemaTable().Rows[0]["TABLE_NAME"].ToString();
                var tb = TM.OleExcel.ToDataTable("SELECT * FROM [" + sheet + "]");
                var rs = tb.Clone();
                var offset = int.Parse(collection["offset"]);
                var number = int.Parse(collection["number"]);
                for (int i = 0; i < number; i++)
                {
                    rs.ImportRow(tb.Rows[(i + 1) * offset]);
                }

                TM.Exports.ExportExcel(rs, "Danh sách cán bộ thu");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                this.danger(ex.Message);
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        public ActionResult AnhThaiMeo()
        {
            TM.IO.FileDirectory.CreateDirectory(TM.Common.Directories.orther);
            var file = TM.IO.FileDirectory.Upload(Request.Files, TM.Common.Directories.orther, false, new[] { ".txt" });
            if (file.UploadError().Count > 0)
            {
                this.danger("Vui lòng chọn file .txt trước khi thực hiện!");
                return RedirectToAction("Index");
            }
            var fileName = file.UploadFileString();
            var fileNameFull = TM.Common.Directories.orther + fileName;
            FileManagerController.InsertFile(fileNameFull);
            TM.OleExcel.DataSource = TM.IO.FileDirectory.MapPath(fileNameFull);

            var UploadFile = TM.IO.FileDirectory.ReadFile(fileNameFull, "   ");
            var dt = new System.Data.DataTable();
            dt.Columns.Add("col1");
            dt.Columns.Add("col2");
            dt.Columns.Add("col3");
            dt.Columns.Add("col4");
            dt.Columns.Add("col5");
            dt.Columns.Add("col6");
            dt.Columns.Add("col7");
            foreach (var item in UploadFile)
            {
                foreach (var i in item)
                {
                    var tmp = i.Trim().Split(' ');
                    if (tmp.Length == 1)
                        dt.Rows.Add(tmp[0]);
                    else if (tmp.Length == 2)
                        dt.Rows.Add(tmp[0], tmp[1]);
                    else if (tmp.Length == 3)
                        dt.Rows.Add(tmp[0], tmp[1], tmp[2]);
                    else if (tmp.Length == 4)
                        dt.Rows.Add(tmp[0], tmp[1], tmp[2], tmp[3]);
                    else if (tmp.Length == 5)
                        dt.Rows.Add(tmp[0], tmp[1], tmp[2], tmp[3], tmp[4]);
                    else if (tmp.Length == 6)
                        dt.Rows.Add(tmp[0], tmp[1], tmp[2], tmp[3], tmp[4], tmp[5]);
                    else if (tmp.Length == 7)
                        dt.Rows.Add(tmp[0], tmp[1], tmp[2], tmp[3], tmp[4], tmp[5], tmp[6]);
                    else
                        dt.Rows.Add(i);
                }
            }
            TM.Exports.ExportExcel(dt, fileName);
            return RedirectToAction("Index");
        }
    }
}