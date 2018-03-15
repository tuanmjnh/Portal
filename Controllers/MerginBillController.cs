using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TM.Message;
using TM.Helper;
using Dapper;
using System.Data.Entity;

namespace Portal.Controllers
{
    [Filters.AuthVinaphone()]
    public class MerginBillController : BaseController
    {
        string app_id = "app_id";
        string tratruoc = "tratruoc";
        string istratruoc = "istratruoc";
        string datcoc = "datcoc";
        string isdatcoc = "isdatcoc";
        // GET: MerginOrder
        public ActionResult Index()
        {
            ViewBag.directory = TM.IO.FileDirectory.DirectoriesToList(TM.Common.Directories.HDData).OrderByDescending(d => d).ToList();
            return View();
        }
        [HttpPost]
        public ActionResult UploadFiles(FormCollection collection)
        {
            try
            {
                var ckhMerginMonth = collection["ckhMerginMonth"] != null ? true : false;
                var time = collection["time"].ToString();
                //Kiểm tra tháng đầu vào
                if (ckhMerginMonth)
                    time = DateTime.Now.AddMonths(-1).ToString("yyyyMM");
                //Declare
                string hdcd = "hdcd" + time;
                string hddd = "hddd" + time;
                string hdnet = "hdnet" + time;
                string hdtv = "hdtv" + time;

                //Source
                //DataSource = Server.MapPath("~/Uploads/Data/");
                //TM.IO.FileDirectory.CreateDirectory(DataSource);
                var DataSource = TM.Common.Directories.HDData;
                FileManagerController.InsertDirectory(DataSource);
                var fileNameSource = new List<string>();
                var fileName = new List<string>();
                var fileSavePath = new List<string>();
                var dtMergin = new System.Data.DataTable();
                int uploadedCount = 0;
                if (Request.Files.Count > 0)
                {
                    string CurrentMonthYear = System.IO.Path.GetFileName(Request.Files[0].FileName).ToLower().Replace(".dbf", "").RemoveWord();
                    fileNameSource.Add(hdcd + ".dbf");
                    fileNameSource.Add(hddd + ".dbf");
                    fileNameSource.Add(hdnet + ".dbf");
                    fileNameSource.Add(hdtv + ".dbf");
                    DataSource += time + "/";
                    TM.OleDBF.DataSource = DataSource;
                    //TM.IO.FileDirectory.CreateDirectory(DataSource);
                    FileManagerController.InsertDirectory(DataSource);
                    //Delete old File
                    //TM.IO.Delete(DataSource, TM.IO.Files(DataSource));

                    for (int i = 0; i < Request.Files.Count; i++)
                    {
                        var file = Request.Files[i];
                        if (!file.FileName.IsExtension(".dbf"))
                        {
                            this.danger("Tệp phải định dạng .dbf");
                            return RedirectToAction("Index");
                        }

                        if (!fileNameSource.Contains(System.IO.Path.GetFileName(file.FileName).ToLower()))
                        {
                            this.danger("Sai tên tệp");
                            //return RedirectToAction("Index");
                        }

                        if (file.ContentLength > 0)
                        {
                            fileName.Add(System.IO.Path.GetFileName(file.FileName).ToLower());
                            fileSavePath.Add(TM.IO.FileDirectory.MapPath(DataSource) + fileName[i]);
                            file.SaveAs(fileSavePath[i]);
                            uploadedCount++;
                            FileManagerController.InsertFile(DataSource + fileName[i]);
                        }
                    }
                    var rs = "Tải lên thành công </br>";
                    foreach (var item in fileName)
                        rs += item + "<br/>";
                    this.success(rs);
                }
                else
                    this.danger("Vui lòng chọn đủ 4 tệp hdcd+MM+yy.dbf, hddd+MM+yy.dbf, hdnet+MM+yy.dbf, hdtv+MM+yy.dbf !");
            }
            catch (Exception ex)
            {
                this.danger(ex.Message);
            }
            return RedirectToAction("Index");
        }
        [HttpGet]
        public ActionResult PaidProcess(string time, bool ckhMerginMonth)
        {
            try
            {
                //Kiểm tra tháng đầu vào
                if (ckhMerginMonth)
                    time = DateTime.Now.AddMonths(-1).ToString("yyyyMM");
                //Declare
                var hdall = "hdall" + time;
                string hdcd = "hdcd" + time;
                string hddd = "hddd" + time;
                string hdnet = "hdnet" + time;
                string hdtv = "hdtv" + time;
                //Source
                var fileNameSource = new List<string>();
                var dtMergin = new System.Data.DataTable();
                var check_file = 0;
                fileNameSource.Add(hdcd + ".dbf");
                fileNameSource.Add(hddd + ".dbf");
                fileNameSource.Add(hdnet + ".dbf");
                fileNameSource.Add(hdtv + ".dbf");
                //Datasource
                TM.OleDBF.DataSource = TM.Common.Directories.HDData + time + "\\";

                var fileList = TM.IO.FileDirectory.FilesToList(TM.OleDBF.DataSource);
                foreach (var item in fileList)
                {
                    if (fileNameSource.Contains(item))
                        check_file++;
                }
                if (check_file < 4)
                {
                    this.danger("Chưa tải đủ tệp!");
                    return RedirectToAction("Index");
                }

                //Net
                AddPaidProcess(TM.OleDBF.DataSource, hdnet, time, "account", "tong");
                AddDepositProcess(TM.OleDBF.DataSource, hdnet, time, "account", "tongcong");
                //TV
                AddPaidProcess(TM.OleDBF.DataSource, hdtv, time, "account", "cuoc_tb");
                //AddDepositProcess(TM.OleDBF.DataSource, hdtv, time, "account", "tongcong");

                this.success("Cập nhật trả tiền trước, đặt cọc thành công");
            }
            catch (Exception ex)
            {
                this.danger(ex.Message);
            }
            return RedirectToAction("Index");
        }
        [HttpGet]
        public ActionResult MerginSelf(string time, bool ckhMerginMonth)
        {
            try
            {
                //Kiểm tra tháng đầu vào
                if (ckhMerginMonth)
                    time = DateTime.Now.AddMonths(-1).ToString("yyyyMM");
                //Declare
                string hdcd = "hdcd" + time;
                string hddd = "hddd" + time;
                string hdnet = "hdnet" + time;
                string hdtv = "hdtv" + time;
                //Source
                var fileNameSource = new List<string>();
                fileNameSource.Add(hdcd + ".dbf");
                fileNameSource.Add(hddd + ".dbf");
                fileNameSource.Add(hdnet + ".dbf");
                fileNameSource.Add(hdtv + ".dbf");
                //Datasource
                TM.OleDBF.DataSource = TM.Common.Directories.HDData + time + "\\";

                //Delete old File
                //var fileList = TM.IO.FileDirectory.FilesToList(TM.OleDBF.DataSource);
                //foreach (var item in fileList)
                //    if (!fileNameSource.Contains(item.ToLower()))
                //        FileManagerController.DeleteDirFile(TM.OleDBF.DataSource + item); //TM.IO.FileDirectory.Delete(DataSource + item);
                //    else
                //        check_file++;
                var deleteFile = RemoveFileSource(TM.OleDBF.DataSource, ".bak", fileNameSource);

                if (deleteFile.CountException < 4)
                {
                    //this.danger("Vui lòng chọn đủ 4 tệp hdcd+MM+yy.dbf, hddd+MM+yy.dbf, hdnet+MM+yy.dbf, hdtv+MM+yy.dbf !");
                    this.danger("Chưa tải đủ tệp!");
                    return RedirectToAction("Index");
                }

                //Backup File
                TM.IO.FileDirectory.Copy(TM.OleDBF.DataSource + hdcd + ".dbf", TM.OleDBF.DataSource + hdcd + "_old.dbf");
                FileManagerController.InsertFile(TM.OleDBF.DataSource + hdcd + "_old.dbf");
                TM.IO.FileDirectory.Copy(TM.OleDBF.DataSource + hdnet + ".dbf", TM.OleDBF.DataSource + hdnet + "_old.dbf");
                FileManagerController.InsertFile(TM.OleDBF.DataSource + hdnet + "_old.dbf");
                TM.IO.FileDirectory.Copy(TM.OleDBF.DataSource + hdtv + ".dbf", TM.OleDBF.DataSource + hdtv + "_old.dbf");
                FileManagerController.InsertFile(TM.OleDBF.DataSource + hdtv + "_old.dbf");
                TM.IO.FileDirectory.Copy(TM.OleDBF.DataSource + hddd + ".dbf", TM.OleDBF.DataSource + hddd + "_old.dbf");
                FileManagerController.InsertFile(TM.OleDBF.DataSource + hddd + "_old.dbf");

                //Remove Duplicate
                RemoveDuplicate(TM.OleDBF.DataSource, hdcd, new string[] { "tong", "vat", "tong_cuoc" },
                    new string[] { "ten_cq", "dia_chi", "ma_st" }, "Remove Duplicate " + hdcd, "dvql_id", "ma_kh1");

                RemoveDuplicate(TM.OleDBF.DataSource, hddd, new string[] { "cuoc_cthue", "thue", "tongcong", "cuoc_kthue", "giamtru" },
                    new string[] { "ten_tt", "diachi_tt", "ms_thue", "taikhoan" }, "Remove Duplicate " + hddd, "ma_dvi", "ma_cq");
                 
                RemoveDuplicate(TM.OleDBF.DataSource, hdnet, new string[] { "tong", "vat", "tongcong", datcoc },
                    new string[] { "ten_tb", "dia_chi", "ma_st", "taikhoan", "mobile" }, "Remove Duplicate " + hdnet, "ma_dvi", "ma_tt_hni");

                RemoveDuplicate(TM.OleDBF.DataSource, hdtv, new string[] { "tong", "vat", "tongcong", datcoc },
                    new string[] { "fullname", "address", "ma_st", "mobile" }, "Remove Duplicate " + hdtv, "ma_dvi,istratruoc", "ma_tt_hni");

                ReExtensionToLower(TM.OleDBF.DataSource);

                //Update HDCD
                TM.OleDBF.Execute("ALTER TABLE " + hdcd + " ALTER COLUMN Dia_chi char(100)", hdcd);
                TM.OleDBF.Execute("ALTER TABLE " + hdcd + " ALTER COLUMN Ma_cq char(30)", hdcd);

                //Update Ezpay HDDD
                TM.OleDBF.Execute("UPDATE " + hddd + " SET ezpay=0 WHERE ezpay is null", "UPDATE Ezpay " + hddd);

                //Remove Bak file
                deleteFile = RemoveFileSource(TM.OleDBF.DataSource);

                this.success("Ghép hóa đơn lẻ thành công!");
            }
            catch (Exception ex)
            {
                this.danger(ex.Message);
            }
            return RedirectToAction("Index");
        }
        [HttpGet]
        public ActionResult Mergin(string time, bool ckhMerginMonth)
        {
            try
            {
                //Declare
                var hdall = "hdall" + time;
                string hdcd = "hdcd" + time;
                string hddd = "hddd" + time;
                string hdnet = "hdnet" + time;
                string hdtv = "hdtv" + time;
                //Kiểm tra tháng đầu vào
                if (ckhMerginMonth)
                    time = DateTime.Now.AddMonths(-1).ToString("yyyyMM");
                //Source
                var fileNameSource = new List<string>();
                var dtMergin = new System.Data.DataTable();
                var check_file = 0;
                fileNameSource.Add(hdcd + ".dbf");
                fileNameSource.Add(hddd + ".dbf");
                fileNameSource.Add(hdnet + ".dbf");
                fileNameSource.Add(hdtv + ".dbf");
                //Datasource
                TM.OleDBF.DataSource = TM.Common.Directories.HDData + time + "\\";

                var fileList = TM.IO.FileDirectory.FilesToList(TM.OleDBF.DataSource);
                foreach (var item in fileList)
                {
                    if (fileNameSource.Contains(item))
                        check_file++;
                }
                if (check_file < 4)
                {
                    this.danger("Chưa tải đủ tệp!");
                    return RedirectToAction("Index");
                }

                //Xóa file hdall cũ
                FileManagerController.DeleteDirFile(TM.OleDBF.DataSource + hdall + ".dbf");

                //Tạo bảng hóa đơn ghép
                //Mã in
                //- gộp = 1
                //- net = 2
                //- tv = 3
                //- cd = 4
                //- dd = 5
                TM.OleDBF.CreateTable(hdall, new Dictionary<string, string>()
                {
                    {"ma_dvi", "n(2)"},
                    {"ma_cq", "c(20)"},
                    {"acc_net", "c(20)"},
                    {"acc_tv", "c(20)"},
                    {"so_dd", "c(20)"},
                    {"so_cd", "c(20)"},
                    {"ten_tb", "c(100)"},
                    {"dia_chi", "c(100)"},
                    {"mobile", "c(20)"},
                    {"ma_tuyen", "c(10)"},
                    {"ma_st", "c(15)"},
                    {"bank_num", "c(16)"},
                    {"ma_dt", "n(1)"},
                    {"ma_cbt", "n(15)"},
                    {"tong_cd", "n(15)"},
                    {"tong_dd", "n(15)"},
                    {"tong_net", "n(15)"},
                    {"tong_tv", "n(15)"},
                    {"vat", "n(15,2)"},
                    {"tong", "n(15)"},
                    {"kthue", "n(15)"},
                    {"gtru", "n(15)"},
                    {"tongcong", "n(15)"},
                    {"stk", "c(150)"},
                    {"ezpay", "n(1)"},
                    {"kieu", "c(10)"},
                    {"ghep", "n(1)"},
                    {"ma_in", "n(1)"},
                    {"kieu_tt", "n(1)"},
                    {tratruoc, "n(15,2)"},
                    {istratruoc, "n(1)"},
                    {datcoc, "n(15,2)"},
                    {isdatcoc, "n(1)"},
                    {"flag", "n(1)"},
                    {"app_id", "n(10)"},
            });
                #region old
                //TM.OleDBF.Execute(
                //    "CREATE TABLE " + hdall + @"(
                //        [Ma_dvi] n(2),
                //        [ma_cq] c(50),
                //        [acc_net] c(50),
                //        [acc_tv] c(50),
                //        [so_dd] c(50),
                //        [so_cd] c(50),
                //        [ten_tb] c(100),
                //        [dia_chi] c(100),
                //        [ma_tuyen] c(50),
                //        [ma_st] c(15),
                //        [bank_num] c(16),
                //        [ma_dt] n(1),
                //        [ma_cbt] n(15),
                //        [tong_cd] n(12),
                //        [tong_dd] n(12),
                //        [tong_net] n(12),
                //        [tong_tv] n(12),
                //        [vat] n(12, 2),
                //        [tong] n(12),
                //        [kthue] n(12),
                //        [gtru] n(12),
                //        [tongcong] n(12),
                //        [ezpay] n(1),
                //        [Kieu] c(10),
                //        [ghep] n(1),
                //        [ma_in] n(1),
                //        [flag] n(1),
                //        [app_id] n(10))", hdall);
                #endregion

                //insert_hdall
                //Nhập hóa đơn cố định vào hóa đơn ghép
                TM.OleDBF.Execute(InsertString(hdall, hdcd, new Dictionary<string, string>()
                {
                    { "ma_dvi", "dvql_id" },
                    { "ma_cq","Ma_kh1" },
                    { "so_cd","so_tb" },
                    { "ten_tb","ten_cq" },
                    { "dia_chi","dia_chi" },
                    { "ma_tuyen","ma_tuyen" },
                    { "ma_st","ma_st" },
                    { "ma_dt","ma_dt" },
                    { "ma_cbt","ma_cbt" },
                    { "tong_cd","tong" },
                    { "vat","vat" },
                    { "tong","tong" },
                    { "tongcong","tong_cuoc" },
                    { "ezpay","0" },
                    { "kieu","1" },
                    { "ghep","0" },
                    { "ma_in","4" },
                    { "flag","1" },
                }), "Insert " + hdcd);

                //Nhập hóa đơn net vào hóa đơn ghép
                TM.OleDBF.Execute(InsertString(hdall, hdnet, new Dictionary<string, string>()
                {
                    { "ma_dvi", "ma_dvi" },
                    { "ma_cq","ma_tt_hni" },
                    { "acc_net","account" },
                    { "ten_tb","ten_tb" },
                    { "dia_chi","dia_chi" },
                    { "mobile","mobile" },
                    { "ma_tuyen","ma_tuyen" },
                    { "ma_st","ma_st" },
                    { "bank_num","taikhoan" },
                    { "ma_dt","ma_dt" },
                    { "ma_cbt","ma_cbt" },
                    { "tong_net","Tong" },
                    { "vat","vat" },
                    { "tong","tong" },
                    { "tongcong","tongcong" },
                    { "ezpay","0" },
                    { "kieu","1" },
                    { "ghep","0" },
                    { "ma_in","2" },
                    {tratruoc, tratruoc},
                    {istratruoc, istratruoc},
                    {datcoc, datcoc},
                    {isdatcoc, isdatcoc},
                    { "flag","1" },
                }), "Insert " + hdnet);

                //Nhập hóa đơn tv vào hóa đơn ghép
                TM.OleDBF.Execute(InsertString(hdall, hdtv, new Dictionary<string, string>()
                {
                    { "ma_dvi", "ma_dvi" },
                    { "ma_cq","ma_tt_hni" },
                    { "acc_tv","account" },
                    { "ten_tb","fullname" },
                    { "dia_chi","address" },
                    { "mobile","mobile" },
                    { "ma_tuyen","ma_tuyen" },
                    { "ma_st","ma_st" },
                    { "ma_cbt","ma_cbt" },
                    { "tong_tv","Tong" },
                    { "vat","vat" },
                    { "tong","tong" },
                    { "tongcong","tongcong" },
                    { "ezpay","0" },
                    { "kieu","1" },
                    { "ghep","0" },
                    { "ma_in","3" },
                    {tratruoc, tratruoc},
                    {istratruoc, istratruoc},
                    {datcoc, datcoc},
                    {isdatcoc, isdatcoc},
                    { "flag","1" },
                }), "Insert " + hdtv);

                //Nhập hóa đơn di động vào hóa đơn ghép
                TM.OleDBF.Execute(InsertString(hdall, hddd, new Dictionary<string, string>()
                {
                    { "ma_dvi", "ma_dvi" },
                    { "ma_cq","ma_cq" },
                    { "so_dd","so_tb" },
                    { "ten_tb","ten_tt" },
                    { "dia_chi","diachi_tt" },
                    { "ma_tuyen","ma_tuyen" },
                    { "ma_st","ms_thue" },
                    { "bank_num","taikhoan" },
                    { "ma_dt","ma_dt" },
                    { "ma_cbt","ma_cbt" },
                    { "tong_dd","cuoc_cthue" },
                    { "vat","thue" },
                    { "tong","cuoc_cthue" },
                    { "kthue","cuoc_kthue" },
                    { "gtru","giamtru" },
                    { "tongcong","tongcong" },
                    { "ezpay","ezpay" },
                    { "kieu","1" },
                    { "ghep","0" },
                    { "ma_in","5" },
                    { "flag","1" },
                }), "Insert " + hddd);

                //Update Ma_dt 0 -> 1
                TM.OleDBF.Execute($"UPDATE {hdall} SET ma_dt=1 WHERE ma_dt=0", hdall);

                //Remove tongcong<=1000
                TM.OleDBF.Execute($"UPDATE {hdall} SET flag=0 WHERE tongcong<=1000", hdall);

                //Tạo thêm cột app_id
                //try
                //{
                //    TM.OleDBF.Execute(string.Format("ALTER TABLE {0} ADD COLUMN {1} n(10)", file, app_id), "Tạo cột app_id - " + extraEX);
                //}
                //catch (Exception) { }
                //Cập nhật app_id = Auto Increment
                //TM.OleDBF.Execute(string.Format("UPDATE {0} SET {1}=RECNO()", hdall, app_id), "Cập nhật app_id = Auto Increment ");

                //Remove Duplicate hdall
                //Xử lý trùng mã thanh toán khác đơn vị
                RemoveDuplicate(TM.OleDBF.DataSource, hdall,
                    new string[] { "tong_cd", "tong_dd", "tong_net", "tong_tv", "vat", "tong", "kthue", "gtru", "tongcong", datcoc, tratruoc, isdatcoc, istratruoc },
                    new string[] { "acc_net", "acc_tv", "so_dd", "so_cd", "dia_chi", "ma_tuyen", "ma_st", "ma_dt", "ma_cbt", "bank_num" },
                    "Remove Duplicate hoadon", "ma_dvi", "ma_cq", "ma_in=1");
                //Cập nhật STK
                UpdateSTK_MA_DVI(CommonHDALL.stk_ma_dvi, hdall);

                //Update NULL
                TM.OleDBF.Execute($"UPDATE {hdall} SET ezpay=0 WHERE ezpay is null", hdall);
                TM.OleDBF.Execute($"UPDATE {hdall} SET tong_cd=0 WHERE tong_cd is null", hdall);
                TM.OleDBF.Execute($"UPDATE {hdall} SET tong_dd=0 WHERE tong_dd is null", hdall);
                TM.OleDBF.Execute($"UPDATE {hdall} SET tong_net=0 WHERE tong_net is null", hdall);
                TM.OleDBF.Execute($"UPDATE {hdall} SET tong_tv=0 WHERE tong_tv is null", hdall);
                TM.OleDBF.Execute($"UPDATE {hdall} SET kthue=0 WHERE kthue is null", hdall);
                TM.OleDBF.Execute($"UPDATE {hdall} SET gtru=0 WHERE gtru is null", hdall);

                //Remove Bak file
                var deleteFile = RemoveFileSource(TM.OleDBF.DataSource);

                //Add hdall to FileManager
                FileManagerController.InsertFile(TM.OleDBF.DataSource + hdall + ".dbf");

                //Return Download File
                //return RedirectToAction("DownloadFiles");
                this.success("Ghép hóa đơn thành công");

            }
            catch (Exception ex)
            {
                this.danger(ex.Message);
            }
            return RedirectToAction("Index");
        }
        [HttpGet]
        public JsonResult CheckMoney(string time, bool ckhMerginMonth)
        {
            try
            {
                //Kiểm tra tháng đầu vào
                if (ckhMerginMonth)
                    time = DateTime.Now.AddMonths(-1).ToString("yyyyMM");
                TM.OleDBF.DataSource = TM.Common.Directories.HDData + time + "\\";
                //HDALL
                var TotalHDAll = new TotalHDAll();
                var dt = TM.OleDBF.ToDataTable("SELECT SUM(tong_cd) AS tong_cd,SUM(tong_dd) AS tong_dd,SUM(tong_net) AS tong_net,SUM(tong_tv) AS tong_tv,SUM(tong) AS tong,SUM(vat) AS vat,SUM(kthue) AS kthue,SUM(gtru) AS gtru,SUM(tongcong) AS tongcong,SUM(datcoc) as datcoc,SUM(tratruoc) as tratruoc FROM hdall" + time);
                TotalHDAll.tong_cd = (decimal)dt.Rows[0]["tong_cd"];
                TotalHDAll.tong_dd = (decimal)dt.Rows[0]["tong_dd"];
                TotalHDAll.tong_net = (decimal)dt.Rows[0]["tong_net"];
                TotalHDAll.tong_tv = (decimal)dt.Rows[0]["tong_tv"];
                TotalHDAll.tong = (decimal)dt.Rows[0]["tong"];
                TotalHDAll.vat = (decimal)dt.Rows[0]["vat"];
                TotalHDAll.kthue = (decimal)dt.Rows[0]["kthue"];
                TotalHDAll.gtru = (decimal)dt.Rows[0]["gtru"];
                TotalHDAll.tongcong = (decimal)dt.Rows[0]["tongcong"];
                TotalHDAll.datcoc = (decimal)dt.Rows[0][datcoc];
                TotalHDAll.tratruoc = (decimal)dt.Rows[0][tratruoc];
                //HDCD
                var TotalHDCD = new TotalHDCD();
                dt = TM.OleDBF.ToDataTable("SELECT SUM(tong) AS tong,SUM(vat) AS vat,SUM(tong_cuoc) AS tongcong FROM hdcd" + time);
                TotalHDCD.tong = (decimal)dt.Rows[0]["tong"];
                TotalHDCD.vat = (decimal)dt.Rows[0]["vat"];
                TotalHDCD.tongcong = (decimal)dt.Rows[0]["tongcong"];
                //HDDD
                var TotalHDDD = new TotalHDDD();
                dt = TM.OleDBF.ToDataTable("SELECT SUM(cuoc_cthue) AS tong,SUM(cuoc_kthue) AS kthue,SUM(giamtru) AS gtru,SUM(thue) AS vat,SUM(tongcong) AS tongcong FROM hddd" + time);
                TotalHDDD.tong = (decimal)dt.Rows[0]["tong"];
                TotalHDDD.kthue = (decimal)dt.Rows[0]["kthue"];
                TotalHDDD.gtru = (decimal)dt.Rows[0]["gtru"];
                TotalHDDD.vat = (decimal)dt.Rows[0]["vat"];
                TotalHDDD.tongcong = (decimal)dt.Rows[0]["tongcong"];
                //HDNET
                var TotalHDNET = new TotalHDNET();
                dt = TM.OleDBF.ToDataTable("SELECT SUM(tong) AS tong,SUM(vat) AS vat,SUM(tongcong) AS tongcong,SUM(datcoc) as datcoc,SUM(tratruoc) as tratruoc FROM hdnet" + time);
                TotalHDNET.datcoc = (decimal)dt.Rows[0][datcoc];
                TotalHDNET.tratruoc = (decimal)dt.Rows[0][tratruoc];
                TotalHDNET.tong = (decimal)dt.Rows[0]["tong"];
                TotalHDNET.vat = (decimal)dt.Rows[0]["vat"];
                TotalHDNET.tongcong = (decimal)dt.Rows[0]["tongcong"];
                //HDTV
                var TotalHDTV = new TotalHDTV();
                dt = TM.OleDBF.ToDataTable("SELECT SUM(tong) AS tong,SUM(vat) AS vat,SUM(tongcong) AS tongcong,SUM(datcoc) as datcoc,SUM(tratruoc) as tratruoc FROM hdtv" + time);
                TotalHDTV.datcoc = (decimal)dt.Rows[0][datcoc];
                TotalHDTV.tratruoc = (decimal)dt.Rows[0][tratruoc];
                TotalHDTV.tong = (decimal)dt.Rows[0]["tong"];
                TotalHDTV.vat = (decimal)dt.Rows[0]["vat"];
                TotalHDTV.tongcong = (decimal)dt.Rows[0]["tongcong"];

                return Json(new
                {
                    TotalHDAll = TotalHDAll,
                    TotalHDCD = TotalHDCD,
                    TotalHDDD = TotalHDDD,
                    TotalHDNET = TotalHDNET,
                    TotalHDTV = TotalHDTV,
                    success = TM.Common.Language.msgRecoverSucsess
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message }, JsonRequestBehavior.AllowGet); }
        }

        System.Text.StringBuilder ok = new System.Text.StringBuilder();
        System.Text.StringBuilder err = new System.Text.StringBuilder();
        //Hóa đơn điện tử
        [HttpGet]
        public ActionResult GetHDDTBill(string time, bool ckhMerginMonth, bool ckhTCVN3, bool ckhZipFile)
        {
            var indexError = "";
            try
            {
                //Kiểm tra tháng đầu vào
                if (ckhMerginMonth)
                    time = DateTime.Now.AddMonths(-1).ToString("yyyyMM");
                var hdall = "hdall" + time;
                var hdallhddt = "hdall_hddt" + time;
                TM.OleDBF.DataSource = TM.Common.Directories.HDData + time + "\\";
                //Khai báo biến
                var listKey = new List<string>();
                //var billTime = "20" + time[2].ToString() + time[3].ToString() + time[0].ToString() + time[1].ToString();
                var valueTime = time.Substring(4, 2) + time.Substring(0, 4);
                var NumberToLeter = new TM.Helper.NumberToLeter();
                var hasError = false;
                string fileName = "hoadon";
                string fileNameDBFHDDT = TM.OleDBF.DataSource + hdallhddt + ".dbf";
                string fileNameXMLFull = TM.OleDBF.DataSource + fileName + ".xml";
                string fileNameZIPFull = TM.OleDBF.DataSource + fileName + ".zip";
                string fileNameXMLError = TM.OleDBF.DataSource + fileName + "_Error.xml";
                //Xóa file HDDT cũ
                FileManagerController.DeleteDirFile(fileNameDBFHDDT);
                FileManagerController.DeleteDirFile(fileNameZIPFull);
                FileManagerController.DeleteDirFile(fileNameXMLError);
                //
                TM.IO.FileDirectory.Copy(TM.OleDBF.DataSource + hdall + ".dbf", fileNameDBFHDDT);
                //Xử lý trùng mã thanh toán khác đơn vị
                RemoveDuplicate(TM.OleDBF.DataSource, hdallhddt,
                    new string[] { "tong_cd", "tong_dd", "tong_net", "tong_tv", "vat", "tong", "kthue", "gtru", "tongcong" },
                    new string[] { "acc_net", "acc_tv", "so_dd", "so_cd", "dia_chi", "ma_tuyen", "ma_st", "ma_dt", "ma_cbt", "bank_num" },
                    "Remove Duplicate hoadon", null);
                //Get data from HDDT
                var dt = TM.OleDBF.ToDataTable("SELECT * FROM " + hdallhddt);
                //HDDT Hoa don
                using (System.IO.Stream outfile = new System.IO.FileStream(TM.IO.FileDirectory.MapPath(fileNameXMLFull), System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite))
                {
                    ok.Append("<Invoices><BillTime>" + time + "</BillTime>").AppendLine();
                    err.Append("<Invoices><BillTime>" + time + "</BillTime>").AppendLine();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        indexError = "Index: " + i + ", app_id:" + dt.Rows[i][app_id].ToString();
                        #region List Product
                        //<Product>
                        //<ProdName>Dịch vụ Internet</ProdName>
                        //<ProdUnit></ProdUnit>
                        //<ProdQuantity></ProdQuantity>
                        //<ProdPrice></ProdPrice>
                        //<Amount>1</Amount>
                        //</Product>
                        //<Product>
                        //<ProdName>Dịch vụ MyTv</ProdName>
                        //<ProdUnit></ProdUnit>
                        //<ProdQuantity></ProdQuantity>
                        //<ProdPrice></ProdPrice>
                        //<Amount>0</Amount>
                        //</Product>
                        //<Product>
                        //<ProdName>Dịch vụ Cố định</ProdName>
                        //<ProdUnit></ProdUnit>
                        //<ProdQuantity></ProdQuantity>
                        //<ProdPrice></ProdPrice>
                        //<Amount>20000</Amount>
                        //</Product>
                        //<Product>
                        //<ProdName>Dịch vụ Di động</ProdName>
                        //<ProdUnit></ProdUnit>
                        //<ProdQuantity></ProdQuantity>
                        //<ProdPrice></ProdPrice>
                        //<Amount>0</Amount>
                        //</Product>
                        //<Product>
                        //<ProdName>Khuyến mại, giảm trừ</ProdName>
                        //<ProdUnit></ProdUnit>
                        //<ProdQuantity></ProdQuantity>
                        //<ProdPrice></ProdPrice>
                        //<Amount>0</Amount>
                        //</Product>
                        #endregion
                        var ma_cq = dt.Rows[i]["ma_cq"].ToString().Trim();
                        var _app_id = dt.Rows[i][app_id].ToString().Trim();
                        var ezpay = int.Parse(dt.Rows[i]["ezpay"].ToString());
                        var check = true;
                        //Check EzPay
                        if (ezpay == 1)
                        {
                            err.Append($"<Inv><key>{ma_cq}</key><app_id>{_app_id}</app_id><error>Is Ezpay</error></Inv>").AppendLine();
                            check = false;
                            hasError = true;
                        }
                        //Check Error
                        if (string.IsNullOrEmpty(ma_cq))
                        {
                            err.Append($"<Inv><key>{ma_cq}</key><app_id>{_app_id}</app_id><error>Null Or Empty</error></Inv>").AppendLine();
                            check = false;
                            hasError = true;
                        }
                        else
                        {
                            if (listKey.Contains(ma_cq))
                            {
                                err.Append($"<Inv><key>{ma_cq}</key><app_id>{_app_id}</app_id><error>Trùng mã thanh toán</error></Inv>").AppendLine();
                                check = false;
                                hasError = true;
                            }
                        }
                        //Run
                        if (check)
                        {
                            var Products = $"{getProduct("Dịch vụ Internet", "", "", "", dt.Rows[i]["tong_net"].ToString().Trim())}" +
                                           $"{getProduct("Dịch vụ MyTv", "", "", "", dt.Rows[i]["tong_tv"].ToString().Trim())}" +
                                           $"{getProduct("Dịch vụ Cố định", "", "", "", dt.Rows[i]["tong_cd"].ToString().Trim())}" +
                                           $"{getProduct("Dịch vụ Di động", "", "", "", dt.Rows[i]["tong_dd"].ToString().Trim())}" +
                                           $"{getProduct("Khuyến mại, giảm trừ", "", "", "", dt.Rows[i]["gtru"].ToString().Trim())}";
                            var so_cd = string.IsNullOrEmpty(dt.Rows[i]["so_cd"].ToString().Trim());
                            var so_dd = string.IsNullOrEmpty(dt.Rows[i]["so_dd"].ToString().Trim());
                            var acc_net = string.IsNullOrEmpty(dt.Rows[i]["acc_net"].ToString().Trim());
                            var acc_tv = string.IsNullOrEmpty(dt.Rows[i]["acc_tv"].ToString().Trim());
                            var ma_in = dt.Rows[i]["ma_in"].ToString().Trim();
                            var Invoice = "<Invoice>" +
                                            $"<MaThanhToan><![CDATA[{getMaThanhToanHD(valueTime, ma_cq, int.Parse(ma_in) == 5 ? HDDT.DetailDiDong : HDDT.DetailCoDinh)}]]></MaThanhToan>" +
                                            $"<CusCode><![CDATA[{dt.Rows[i]["ma_cq"].ToString().Trim()}]]></CusCode>" +
                                            $"<CusName><![CDATA[{(ckhTCVN3 ? TM.Helper.ConvertCharacter.TCVN3ToUnicode(dt.Rows[i]["ten_tb"].ToString().Trim()) : dt.Rows[i]["ten_tb"].ToString().Trim())}]]></CusName>" +
                                            $"<CusAddress><![CDATA[{(ckhTCVN3 ? TM.Helper.ConvertCharacter.TCVN3ToUnicode(dt.Rows[i]["dia_chi"].ToString().Trim()) : dt.Rows[i]["dia_chi"].ToString().Trim())}]]></CusAddress>" +
                                            $"<CusPhone><![CDATA[{getCusPhone(dt.Rows[i]["acc_net"].ToString().Trim(), dt.Rows[i]["acc_tv"].ToString().Trim(), dt.Rows[i]["so_cd"].ToString().Trim(), dt.Rows[i]["so_dd"].ToString().Trim())}]]></CusPhone>" +
                                            $"<CusTaxCode>{dt.Rows[i]["ma_st"].ToString().Trim()}</CusTaxCode>" +
                                            $"<PaymentMethod>{"TM/CK"}</PaymentMethod>" +
                                            $"<KindOfService>{"Cước Tháng " + valueTime}</KindOfService>" +
                                            $"<Products>{Products}</Products>" +
                                            $"<Total>{dt.Rows[i]["tong"].ToString().Trim()}</Total>" +
                                            $"<DiscountAmount>{dt.Rows[i]["gtru"].ToString().Trim()}</DiscountAmount>" +
                                             "<VATRate>10</VATRate>" +
                                            $"<VATAmount>{dt.Rows[i]["vat"].ToString().Trim()}</VATAmount>" +
                                            $"<Amount>{dt.Rows[i]["tongcong"].ToString().Trim()}</Amount>" +
                                            $"<AmountInWords><![CDATA[{NumberToLeter.DocTienBangChu(long.Parse(dt.Rows[i]["tongcong"].ToString().Trim()), "")}]]></AmountInWords>" +
                                            $"<PaymentStatus>0</PaymentStatus>" +
                                            $"<Extra>{dt.Rows[i]["ma_cbt"].ToString().Trim() + ";0;0"}</Extra>" +
                                            $"<ResourceCode>{dt.Rows[i]["ma_cbt"].ToString().Trim()}</ResourceCode>" +
                                        "</Invoice>";
                            Invoice = $"<Inv><key>{ma_cq + valueTime}</key>{Invoice}</Inv>";
                            ok.Append(Invoice).AppendLine();
                            listKey.Add(ma_cq);
                        }
                        if (i % 100 == 0)
                        {
                            outfile.Write(System.Text.Encoding.UTF8.GetBytes(ok.ToString()), 0, System.Text.Encoding.UTF8.GetByteCount(ok.ToString()));
                            ok = new System.Text.StringBuilder();
                        }
                    }
                    ok.Append("</Invoices>");
                    outfile.Write(System.Text.Encoding.UTF8.GetBytes(ok.ToString()), 0, System.Text.Encoding.UTF8.GetByteCount(ok.ToString()));
                    //
                    outfile.Close();
                }
                if (hasError)
                {
                    using (System.IO.Stream outfile = new System.IO.FileStream(TM.IO.FileDirectory.MapPath(fileNameXMLError), System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite))
                    {
                        err.Append("</Invoices>");
                        outfile.Write(System.Text.Encoding.UTF8.GetBytes(err.ToString()), 0, System.Text.Encoding.UTF8.GetByteCount(err.ToString()));
                        outfile.Close();
                    }
                    this.danger("Có lỗi xảy ra Truy cập File Manager \"~\\" + TM.OleDBF.DataSource.Replace("Uploads\\", "") + "\" để tải file");
                }
                //ZipFile
                if (ckhZipFile)
                    TM.IO.Zip.ZipFile(new List<string>() { fileNameXMLFull }, fileNameZIPFull, 9);
                //Insert To File Manager
                FileManagerController.DeleteDirFile(fileNameXMLFull);
                FileManagerController.InsertFile(fileNameZIPFull);
                FileManagerController.InsertFile(fileNameXMLError);


                //HDDT Khach hang
                ok = new System.Text.StringBuilder();
                err = new System.Text.StringBuilder();
                listKey = new List<string>();
                hasError = false;
                fileName = "cus";
                fileNameDBFHDDT = TM.OleDBF.DataSource + hdallhddt + ".dbf";
                fileNameXMLFull = TM.OleDBF.DataSource + fileName + ".xml";
                fileNameZIPFull = TM.OleDBF.DataSource + fileName + ".zip";
                fileNameXMLError = TM.OleDBF.DataSource + fileName + "_Error.xml";
                //Xóa file HDDT cũ
                FileManagerController.DeleteDirFile(fileNameZIPFull);
                FileManagerController.DeleteDirFile(fileNameXMLError);
                using (System.IO.Stream outfile = new System.IO.FileStream(TM.IO.FileDirectory.MapPath(fileNameXMLFull), System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite))
                {
                    ok.Append("<Customers>").AppendLine();
                    err.Append("<Customers>").AppendLine();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        #region List Product
                        //<Product>
                        //<ProdName>Dịch vụ Internet</ProdName>
                        //<ProdUnit></ProdUnit>
                        //<ProdQuantity></ProdQuantity>
                        //<ProdPrice></ProdPrice>
                        //<Amount>1</Amount>
                        //</Product>
                        //<Product>
                        //<ProdName>Dịch vụ MyTv</ProdName>
                        //<ProdUnit></ProdUnit>
                        //<ProdQuantity></ProdQuantity>
                        //<ProdPrice></ProdPrice>
                        //<Amount>0</Amount>
                        //</Product>
                        //<Product>
                        //<ProdName>Dịch vụ Cố định</ProdName>
                        //<ProdUnit></ProdUnit>
                        //<ProdQuantity></ProdQuantity>
                        //<ProdPrice></ProdPrice>
                        //<Amount>20000</Amount>
                        //</Product>
                        //<Product>
                        //<ProdName>Dịch vụ Di động</ProdName>
                        //<ProdUnit></ProdUnit>
                        //<ProdQuantity></ProdQuantity>
                        //<ProdPrice></ProdPrice>
                        //<Amount>0</Amount>
                        //</Product>
                        //<Product>
                        //<ProdName>Khuyến mại, giảm trừ</ProdName>
                        //<ProdUnit></ProdUnit>
                        //<ProdQuantity></ProdQuantity>
                        //<ProdPrice></ProdPrice>
                        //<Amount>0</Amount>
                        //</Product>
                        #endregion
                        var ma_cq = dt.Rows[i]["ma_cq"].ToString().Trim();
                        var _app_id = dt.Rows[i][app_id].ToString().Trim();
                        var ezpay = int.Parse(dt.Rows[i]["ezpay"].ToString());
                        var check = true;
                        //Check EzPay
                        if (ezpay == 1)
                        {
                            err.Append($"<Inv><key>{ma_cq}</key><app_id>{_app_id}</app_id><error>Is Ezpay</error></Inv>").AppendLine();
                            check = false;
                            hasError = true;
                        }
                        //Check Error
                        if (string.IsNullOrEmpty(ma_cq))
                        {
                            err.Append($"<Customer><key>{ma_cq}</key><error>Null Or Empty HDDT Khach hang</error></Customer>").AppendLine();
                            check = false;
                            hasError = true;
                        }
                        else
                        {
                            if (listKey.Contains(ma_cq))
                            {
                                err.Append($"<Customer><key>{ma_cq}</key><error>Trùng mã thanh toán Khach hang</error></Customer>").AppendLine();
                                check = false;
                                hasError = true;
                            }
                        }
                        //Run
                        if (check)
                        {
                            var ten_tb = ckhTCVN3 ? TM.Helper.ConvertCharacter.TCVN3ToUnicode(dt.Rows[i]["ten_tb"].ToString().Trim()) : dt.Rows[i]["ten_tb"].ToString().Trim();
                            var ma_st = dt.Rows[i]["ma_st"].ToString().Trim();
                            var dia_chi = ckhTCVN3 ? TM.Helper.ConvertCharacter.TCVN3ToUnicode(dt.Rows[i]["dia_chi"].ToString().Trim()) : dt.Rows[i]["dia_chi"].ToString().Trim();
                            var acc_net = dt.Rows[i]["acc_net"].ToString().Trim();
                            var acc_tv = dt.Rows[i]["acc_tv"].ToString().Trim();
                            var so_dd = dt.Rows[i]["so_dd"].ToString().Trim();
                            var so_cd = dt.Rows[i]["so_cd"].ToString().Trim();
                            var bank_num = dt.Rows[i]["bank_num"].ToString().Trim();
                            var ma_in = dt.Rows[i]["ma_in"].ToString().Trim();
                            var customer = "<Customer>" +
                                                $"<Name><![CDATA[{ten_tb}]]></Name>" +
                                                $"<Code>{ma_cq}</Code>" +
                                                $"<TaxCode>{ma_st}</TaxCode>" +
                                                $"<Address><![CDATA[{dia_chi}]]></Address>" +
                                                 "<BankAccountName></BankAccountName>" +
                                                 "<BankName></BankName>" +
                                                $"<BankNumber>{bank_num}</BankNumber>" +
                                                 "<Email></Email>" +
                                                 "<Fax></Fax>" +
                                                $"<Phone>{getCusPhone(acc_net, acc_tv, so_dd, so_cd)}</Phone>" +
                                                 "<ContactPerson></ContactPerson>" +
                                                 "<RepresentPerson></RepresentPerson>" +
                                                 "<CusType>0</CusType>" +
                                                $"<MaThanhToan><![CDATA[{getMaThanhToanKH(valueTime, ma_cq, int.Parse(ma_in) == 5 ? HDDT.DetailDiDong : HDDT.DetailCoDinh)}]]></MaThanhToan>" +
                                           "</Customer>";
                            ok.Append(customer).AppendLine();
                            listKey.Add(ma_cq);
                        }
                        if (i % 100 == 0)
                        {
                            outfile.Write(System.Text.Encoding.UTF8.GetBytes(ok.ToString()), 0, System.Text.Encoding.UTF8.GetByteCount(ok.ToString()));
                            ok = new System.Text.StringBuilder();
                        }
                    }
                    ok.Append("</Customers>");
                    outfile.Write(System.Text.Encoding.UTF8.GetBytes(ok.ToString()), 0, System.Text.Encoding.UTF8.GetByteCount(ok.ToString()));
                    //
                    outfile.Close();
                }
                if (hasError)
                {
                    using (System.IO.Stream outfile = new System.IO.FileStream(TM.IO.FileDirectory.MapPath(fileNameXMLError), System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite))
                    {
                        err.Append("</Customers>");
                        outfile.Write(System.Text.Encoding.UTF8.GetBytes(err.ToString()), 0, System.Text.Encoding.UTF8.GetByteCount(err.ToString()));
                        outfile.Close();
                    }
                    this.danger("Có lỗi xảy ra Truy cập File Manager \"~\\" + TM.OleDBF.DataSource.Replace("Uploads\\", "") + "\" để tải file");
                }
                //ZipFile
                if (ckhZipFile)
                    TM.IO.Zip.ZipFile(new List<string>() { fileNameXMLFull }, fileNameZIPFull, 9);
                //Insert To File Manager
                FileManagerController.DeleteDirFile(fileNameXMLFull);
                FileManagerController.InsertFile(fileNameZIPFull);
                FileManagerController.InsertFile(fileNameXMLError);
                //Insert To File Manager hdallhddt
                FileManagerController.InsertFile(fileNameDBFHDDT);
                this.success("Tạo hóa đơn điện tử thành công! Truy cập File Manager \"~\\" + TM.OleDBF.DataSource.Replace("Uploads\\", "") + "\" để tải file");
            }
            catch (Exception ex)
            {
                this.danger(ex.Message + "/" + indexError);
            }
            return RedirectToAction("Index");
        }

        //Nhập đặt cọc trả trươc
        public ActionResult ImportDatCocTraTruoc(FormCollection collection)
        {
            var index = 0;
            try
            {
                //
                //Connection().Query("DELETE FROM cuocTraTienTruoc");
                //
                TM.IO.FileDirectory.CreateDirectory(TM.Common.Directories.DatCocTraTruoc);
                var file = TM.IO.FileDirectory.Upload(Request.Files, TM.Common.Directories.DatCocTraTruoc, false, new[] { ".xls" });
                if (file.UploadError().Count > 0)
                {
                    this.danger("Vui lòng chọn file DBF trước khi thực hiện!");
                    return RedirectToAction("Index");
                }
                var fileName = file.UploadFileString();
                var fileNameFull = TM.Common.Directories.DatCocTraTruoc + fileName;
                TM.OleExcel.DataSource = TM.IO.FileDirectory.MapPath(fileNameFull);
                var dt = TM.OleExcel.ToDataSet();

                var cuocTraTienTruoc = new Models.cuocTraTienTruoc();

                foreach (System.Data.DataRow row in dt.Tables[0].Rows)
                {
                    index++;
                    if (index == 835)
                        index = 835;
                    cuocTraTienTruoc = new Models.cuocTraTienTruoc();
                    cuocTraTienTruoc.account = row[0].ToString();
                    cuocTraTienTruoc.tien = !string.IsNullOrEmpty(row[3].ToString()) ? decimal.Parse(row[3].ToString()) : 0;
                    //
                    var tmp = db.cuocTraTienTruocs.FirstOrDefault(m => m.account == cuocTraTienTruoc.account);
                    if (tmp != null)
                    {
                        if (tmp.type == 2)
                            tmp.tien += cuocTraTienTruoc.tien;
                        db.Entry(tmp).State = EntityState.Modified;
                        continue;
                    }
                    //
                    cuocTraTienTruoc.thangNop = !string.IsNullOrEmpty(row[1].ToString()) ? int.Parse(row[1].ToString()) : 0;
                    cuocTraTienTruoc.thangThem = !string.IsNullOrEmpty(row[2].ToString()) ? int.Parse(row[2].ToString()) : 0;

                    if (!string.IsNullOrEmpty(row[4].ToString())) cuocTraTienTruoc.thang_dk = DateTime.Parse(row[4].ToString());
                    if (!string.IsNullOrEmpty(row[5].ToString())) cuocTraTienTruoc.thang_bd = DateTime.Parse(row[5].ToString());
                    if (!string.IsNullOrEmpty(row[8].ToString())) cuocTraTienTruoc.type = int.Parse(row[8].ToString());
                    if (!string.IsNullOrEmpty(row[5].ToString()) && cuocTraTienTruoc.type == 1)
                        cuocTraTienTruoc.thang_kt = DateTime.Parse(row[5].ToString()).AddHours(23).AddMinutes(59).AddSeconds(59).AddMonths(cuocTraTienTruoc.thangNop);
                    if (!string.IsNullOrEmpty(row[7].ToString())) cuocTraTienTruoc.ghiChu = row[7].ToString();
                    cuocTraTienTruoc.tientruthang = 0;
                    cuocTraTienTruoc.flag = !string.IsNullOrEmpty(row[9].ToString()) ? int.Parse(row[9].ToString()) : 0;
                    db.cuocTraTienTruocs.Add(cuocTraTienTruoc);
                }
                db.SaveChanges();
                FileManagerController.InsertDirectory(TM.Common.Directories.DatCocTraTruoc);
                FileManagerController.InsertFile(TM.OleExcel.DataSource);
                this.success("Nhập dữ liệu thành công!");
                //return DownloadFiles(TM.OleDBF.DataSource, fileName.ToLower().Replace(".dbf", "") + "_RemoveDuplicate.dbf");
            }
            catch (Exception ex)
            {
                this.danger(ex.Message + " - Index: " + index.ToString());
            }
            return RedirectToAction("Index");
        }


        //RemoveDuplicate - Xóa trùng file DBF
        public ActionResult RemoveDuplicate(FormCollection collection)
        {
            try
            {
                TM.IO.FileDirectory.CreateDirectory(TM.Common.Directories.orther);
                var file = TM.IO.FileDirectory.Upload(Request.Files, TM.Common.Directories.orther, false, new[] { ".dbf" });
                if (file.UploadError().Count > 0)
                {
                    this.danger("Vui lòng chọn file DBF trước khi thực hiện!");
                    return RedirectToAction("Index");
                }
                var fileName = file.UploadFileString();
                var fileNameFull = TM.Common.Directories.orther + fileName;
                TM.OleDBF.DataSource = fileNameFull;
                string sql = "ALTER table " + fileName + " ADD COLUMN app_id n(10)";
                try
                {
                    TM.OleDBF.Execute(sql);
                }
                catch (Exception) { }
                sql = "ALTER table " + fileName + " ADD COLUMN dupe_flag n(2)";
                try
                {
                    TM.OleDBF.Execute(sql);
                }
                catch (Exception) { }
                sql = "UPDATE " + fileName + " SET app_id=RECNO()";
                TM.OleDBF.Execute(sql);
                sql = "UPDATE " + fileName + " SET dupe_flag=0";
                TM.OleDBF.Execute(sql);

                if (!string.IsNullOrEmpty(collection["chkIsDvi"]))
                    sql = "UPDATE " + fileName + " SET dupe_flag=1 WHERE app_id in(SELECT app_id FROM " + fileName + " o INNER JOIN (SELECT " + collection["txtAccount"] + "," + collection["txtDvi"] + ",COUNT(*) AS dupeCount FROM " + fileName + " GROUP BY " + collection["txtAccount"] + "," + collection["txtDvi"] + " HAVING COUNT(*) > 1) oc ON o." + collection["txtAccount"] + "=oc." + collection["txtAccount"] + " WHERE o." + collection["txtDvi"] + "=oc." + collection["txtDvi"] + ")";
                else
                    sql = "UPDATE " + fileName + " SET dupe_flag=1 WHERE app_id in(SELECT app_id FROM " + fileName + " o INNER JOIN (SELECT " + collection["txtAccount"] + ",COUNT(*) AS dupeCount FROM " + fileName + " GROUP BY " + collection["txtAccount"] + " HAVING COUNT(*) > 1) oc ON o." + collection["txtAccount"] + "=oc." + collection["txtAccount"] + ")";
                TM.OleDBF.Execute(sql);

                if (!string.IsNullOrEmpty(collection["chkIsDvi"]))
                    sql = "UPDATE " + fileName + " SET dupe_flag=2 WHERE app_id IN(SELECT MAX(app_id) FROM " + fileName + " o INNER JOIN (SELECT " + collection["txtAccount"] + "," + collection["txtDvi"] + ",COUNT(*) AS dupeCount FROM " + fileName + " GROUP BY " + collection["txtAccount"] + "," + collection["txtDvi"] + " HAVING COUNT(*) > 1) oc ON o." + collection["txtAccount"] + "=oc." + collection["txtAccount"] + " WHERE o." + collection["txtDvi"] + "=oc." + collection["txtDvi"] + " GROUP BY o." + collection["txtAccount"] + ",o." + collection["txtDvi"] + ")";
                else
                    sql = "UPDATE " + fileName + " SET dupe_flag=2 WHERE app_id IN(SELECT MAX(app_id) FROM " + fileName + " o INNER JOIN (SELECT " + collection["txtAccount"] + ",COUNT(*) AS dupeCount FROM " + fileName + " GROUP BY " + collection["txtAccount"] + " HAVING COUNT(*) > 1) oc ON o." + collection["txtAccount"] + "=oc." + collection["txtAccount"] + " GROUP BY o." + collection["txtAccount"] + ")";
                TM.OleDBF.Execute(sql);

                sql = "DELETE FROM " + fileName + " WHERE dupe_flag=1";
                TM.OleDBF.Execute(sql);
                sql = "PACK " + fileName;
                TM.OleDBF.Execute(sql);
                FileManagerController.InsertDirectory(TM.Common.Directories.orther);
                FileManagerController.InsertFile(TM.OleDBF.DataSource);
                return DownloadFiles(TM.OleDBF.DataSource, fileName.ToLower().Replace(".dbf", "") + "_RemoveDuplicate.dbf");
            }
            catch (Exception ex)
            {
                this.danger(ex.Message);
            }
            return RedirectToAction("Index");
        }
        public ActionResult GetDuplicate(FormCollection collection)
        {
            try
            {
                TM.IO.FileDirectory.CreateDirectory(TM.Common.Directories.orther);
                var file = TM.IO.FileDirectory.Upload(Request.Files, TM.Common.Directories.orther, false, new[] { ".dbf" });
                if (file.UploadError().Count > 0)
                {
                    this.danger("Vui lòng chọn file DBF trước khi thực hiện!");
                    return RedirectToAction("Index");
                }
                var fileName = file.UploadFileString();
                var fileNameFull = TM.Common.Directories.orther + fileName;
                TM.OleDBF.DataSource = fileNameFull;
                //
                string sql = string.Format("ALTER table {0} ADD COLUMN flag_madt n(1)", fileName);
                try { TM.OleDBF.Execute(sql); } catch { }
                //
                sql = string.Format("UPDATE {0} SET flag_madt=0", fileName);
                TM.OleDBF.Execute(sql);
                //
                if (!string.IsNullOrEmpty(collection["chkIsDvi"]))
                    sql = string.Format("UPDATE {0} SET flag_madt=1 " +
                        "WHERE {1} IN(SELECT o.{1} FROM {0} o INNER JOIN (SELECT o.* FROM {0} o " +
                        "INNER JOIN (SELECT {1},COUNT(*) AS dupeCount FROM {0} GROUP BY {1} HAVING COUNT(*) > 1) oc ON " +
                        "o.{1}=oc.{1} ORDER BY o.{1}) oc ON o.{1}=oc.{1} WHERE LEFT(o.{2},3)!=LEFT(oc.{2},3))",
                        fileName, collection["txtAccount"], collection["txtDvi"]);
                else
                    sql = string.Format("UPDATE {0} SET flag_madt=1 " +
                        "WHERE {1} IN(SELECT o.{1} FROM {0} o INNER JOIN (SELECT o.* FROM {0} o " +
                        "INNER JOIN (SELECT {1},COUNT(*) AS dupeCount FROM {0} GROUP BY {1} HAVING COUNT(*) > 1) oc ON " +
                        "o.{1}=oc.{1} ORDER BY o.{1}) oc ON o.{1}=oc.{1} WHERE LEFT(o.{2},3)!=LEFT(oc.{2},3))",
                        fileName, collection["txtAccount"], collection["txtDvi"]);
                TM.OleDBF.Execute(sql);
                //
                sql = "DELETE FROM " + fileName + " WHERE flag_madt=0";
                TM.OleDBF.Execute(sql);
                sql = "PACK " + fileName;
                TM.OleDBF.Execute(sql);
                FileManagerController.InsertDirectory(TM.Common.Directories.orther);
                FileManagerController.InsertFile(TM.OleDBF.DataSource);
                return DownloadFiles(TM.OleDBF.DataSource, fileName.ToLower().Replace(".dbf", "") + "_GetDuplicate.dbf");
            }
            catch (Exception ex)
            {
                this.danger(ex.Message);
            }
            return RedirectToAction("Index");
        }

        //Private Functions
        //
        private string InsertString(string tblAll, string tblAny, Dictionary<string, string> cols)
        {
            string cols_hdAny = "";
            string cols_hdall = "";
            string insert_hdall = "INSERT INTO " + tblAll + "(";
            foreach (var item in cols)
            {
                cols_hdall += item.Key + ",";
                cols_hdAny += item.Value + " as " + item.Key + ",";
            }

            insert_hdall += cols_hdall.TrimEnd(',') + ") SELECT " + cols_hdAny.TrimEnd(',') + " FROM " + tblAny;
            return insert_hdall;
        }
        private void UpdateSTK_MA_DVI(Dictionary<int, string> arr, string table)
        {
            foreach (var item in arr)
                TM.OleDBF.Execute(string.Format("UPDATE {0} SET stk='{1}' WHERE ma_dvi={2}", table, item.Value, item.Key));
        }
        private void RemoveDuplicate(string DataSource, string file, string[] colsNumberSum, string[] colsString, string extraEX = "", string ma_dvi = "ma_dvi", string ma_cq = "ma_cq", string extraConditions = null)
        {
            #region Coment
            ////Remove Duplicate
            ////Tạo thêm cột app_id
            //ALTER table hdall ADD COLUMN app_id n(10)
            ////cập nhật app_id=Auto Increment
            //UPDATE hdall SET app_id=RECNO()
            ////Tạo thêm cột dupe_flag
            //ALTER table hdall ADD COLUMN dupe_flag n(2)
            ////cập nhật dupe_flag=0
            //UPDATE hdall SET dupe_flag=0
            ////Tìm các đối tượng trùng
            //SELECT * FROM hdall o INNER JOIN (SELECT ma_cq,ma_dvi,COUNT(*) AS dupeCount FROM hdall GROUP BY ma_cq,ma_dvi HAVING COUNT(*) > 1) oc ON o.ma_cq=oc.ma_cq WHERE o.ma_dvi=oc.ma_dvi
            ////Cập nhật các đối tượng trùng dupe_flag=1 
            //UPDATE hdall SET dupe_flag=1 WHERE app_id in(SELECT app_id FROM hdall o INNER JOIN (SELECT ma_cq,ma_dvi,COUNT(*) AS dupeCount FROM hdall GROUP BY ma_cq,ma_dvi HAVING COUNT(*) > 1) oc ON o.ma_cq=oc.ma_cq WHERE o.ma_dvi=oc.ma_dvi)
            ////Kiểm tra
            //SELECT * FROM hdall WHERE dupe_flag=1
            ////Lọc ra các đối tượng trùng giữ lại
            //SELECT MAX(app_id) FROM hdall o INNER JOIN (SELECT ma_cq,ma_dvi,COUNT(*) AS dupeCount FROM hdall GROUP BY ma_cq,ma_dvi HAVING COUNT(*) > 1) oc ON o.ma_cq=oc.ma_cq WHERE o.ma_dvi=oc.ma_dvi GROUP BY o.ma_cq,o.ma_dvi
            ////Cập nhật lại các đối tượng giữ lại và set dupe_flag=2
            //UPDATE hdall SET dupe_flag=2 WHERE app_id IN(SELECT MAX(app_id) FROM hdall o INNER JOIN (SELECT ma_cq,ma_dvi,COUNT(*) AS dupeCount FROM hdall GROUP BY ma_cq,ma_dvi HAVING COUNT(*) > 1) oc ON o.ma_cq=oc.ma_cq WHERE o.ma_dvi=oc.ma_dvi GROUP BY o.ma_cq,o.ma_dvi)
            ////Kiểm tra

            ////Tính tổng các đối tượng trùng
            //SELECT SUM(tong_cd) FROM hdall o INNER JOIN (SELECT ma_cq,ma_dvi,COUNT(*) AS dupeCount FROM hdall GROUP BY ma_cq,ma_dvi HAVING COUNT(*) > 1) oc ON o.ma_cq=oc.ma_cq WHERE o.ma_dvi=oc.ma_dvi GROUP BY o.ma_cq,o.ma_dvi
            ////Cập nhật các đối tượng bị trùng
            //UPDATE a SET tong_cd=(SELECT SUM(tong_cd) FROM hdall WHERE ma_cq=a.ma_cq AND ma_dvi=a.ma_dvi) FROM hdall AS a WHERE dupe_flag=2
            ////Xóa các đối tượng trùng
            //DELETE FROM hdall WHERE dupe_flag=1
            //PACK hdall
            #endregion
            //Declares
            string dupe_flag = "dupe_flag";
            string fileDuplication = file + "_duplication.dbf";
            string fileEmptyNull = file + "_EmptyNull.dbf";
            //Xóa File cũ
            TM.IO.FileDirectory.Delete(DataSource + file + "_duplication.dbf");
            //DeleteDirectoryFile(DataSource + file + "_duplication.dbf");
            TM.IO.FileDirectory.Delete(DataSource + file + "_EmptyNull.dbf");
            //DeleteDirectoryFile(DataSource + file + "_EmptyNull.dbf");

            //Tạo thêm cột app_id
            try { TM.OleDBF.Execute($"ALTER TABLE {file} ADD COLUMN {app_id} n(10)", "Tạo cột app_id - " + extraEX); } catch { }
            //Cập nhật app_id = Auto Increment
            TM.OleDBF.Execute($"UPDATE {file} SET {app_id}=RECNO()", "Cập nhật app_id = Auto Increment " + extraEX);

            //Tạo thêm cột dupe_flag
            try { TM.OleDBF.Execute($"ALTER table {file} ADD COLUMN {dupe_flag} n(2)", "Tạo cột dupe_flag - " + extraEX); } catch { }
            //Cập nhật dupe_flag=0
            TM.OleDBF.Execute($"UPDATE {file} SET {dupe_flag}=0", "Cập nhật dupe_flag=0 - " + extraEX);

            string sql = "";
            if (ma_dvi == null)
            {
                //Cập nhật các đối tượng trùng dupe_flag=1
                sql = $@"UPDATE {file} SET {dupe_flag}=1 WHERE {app_id} in(SELECT {app_id} FROM {file} o INNER JOIN 
                (SELECT {ma_cq},COUNT(*) AS dupeCount FROM {file} GROUP BY {ma_cq} HAVING COUNT(*) > 1) oc ON o.{ma_cq}=oc.{ma_cq} 
                WHERE NOT EMPTY(o.{ma_cq}))";
                TM.OleDBF.Execute(sql, "Cập nhật các đối tượng trùng set dupe_flag=1 - " + file + " - " + extraEX);

                //Cập nhật lại các đối tượng giữ lại và set dupe_flag=2
                sql = $@"UPDATE {file} SET {dupe_flag}=2 WHERE {app_id} IN(SELECT MAX({app_id}) FROM {file} o INNER JOIN 
                (SELECT {ma_cq},COUNT(*) AS dupeCount FROM {file} GROUP BY {ma_cq} HAVING COUNT(*) > 1) oc ON o.{ma_cq}=oc.{ma_cq} 
                WHERE NOT EMPTY(o.{ma_cq}) GROUP BY o.{ma_cq})";
                TM.OleDBF.Execute(sql, "Cập nhật lại các đối tượng giữ lại và set dupe_flag=2 - " + file + " - " + extraEX);
            }
            else
            {
                //var grouplist = "";
                //var grouplist2 = "";
                //foreach (var item in ma_dvi.Split(','))
                //{
                //    grouplist += $"o.{item}=oc.{item} AND ";
                //    grouplist2 += $"o.{item},";
                //}

                ////Cập nhật các đối tượng trùng dupe_flag=1
                //sql = $@"UPDATE {file} SET {dupe_flag}=1 WHERE {app_id} in(SELECT {app_id} FROM {file} o INNER JOIN 
                //(SELECT {ma_cq},{ma_dvi},COUNT(*) AS dupeCount FROM {file} GROUP BY {ma_cq},{ma_dvi} HAVING COUNT(*) > 1) oc ON o.{ma_cq}=oc.{ma_cq} 
                //WHERE {grouplist} NOT EMPTY(o.{ma_cq}))";
                //TM.OleDBF.Execute(sql, "Cập nhật các đối tượng trùng set dupe_flag=1 - " + file + " - " + extraEX);
                ////Cập nhật lại các đối tượng giữ lại và set dupe_flag=2
                //sql = $@"UPDATE {file} SET {dupe_flag}=2{(extraConditions != null ? "," + extraConditions + " " : "")} WHERE {app_id} IN(SELECT MAX({app_id}) FROM {file} o INNER JOIN 
                //(SELECT {ma_cq},{ma_dvi},COUNT(*) AS dupeCount FROM {file} GROUP BY {ma_cq},{ma_dvi} HAVING COUNT(*) > 1) oc ON o.{ma_cq}=oc.{ma_cq} 
                //WHERE {grouplist} NOT EMPTY(o.{ma_cq}) GROUP BY o.{ma_cq},{grouplist2.TrimEnd(',')})";
                //TM.OleDBF.Execute(sql, "Cập nhật lại các đối tượng giữ lại và set dupe_flag=2 - " + file + " - " + extraEX);

                //Cập nhật các đối tượng trùng dupe_flag=1
                sql = $@"UPDATE {file} SET {dupe_flag}=1 WHERE {app_id} in(SELECT {app_id} FROM {file} o INNER JOIN 
                (SELECT {ma_cq},{ma_dvi},COUNT(*) AS dupeCount FROM {file} GROUP BY {ma_cq},{ma_dvi} HAVING COUNT(*) > 1) oc ON o.{ma_cq}=oc.{ma_cq} 
                WHERE {ma_dvi} NOT EMPTY(o.{ma_cq}))";
                TM.OleDBF.Execute(sql, "Cập nhật các đối tượng trùng set dupe_flag=1 - " + file + " - " + extraEX);
                //Cập nhật lại các đối tượng giữ lại và set dupe_flag=2
                sql = $@"UPDATE {file} SET {dupe_flag}=2{(extraConditions != null ? "," + extraConditions + " " : "")} WHERE {app_id} IN(SELECT MAX({app_id}) FROM {file} o INNER JOIN 
                (SELECT {ma_cq},{ma_dvi},COUNT(*) AS dupeCount FROM {file} GROUP BY {ma_cq},{ma_dvi} HAVING COUNT(*) > 1) oc ON o.{ma_cq}=oc.{ma_cq} 
                WHERE {ma_dvi} NOT EMPTY(o.{ma_cq}) GROUP BY o.{ma_cq},{ma_dvi})";
                TM.OleDBF.Execute(sql, "Cập nhật lại các đối tượng giữ lại và set dupe_flag=2 - " + file + " - " + extraEX);
            }


            //Cập nhật các đối tượng bị trùng
            //sql = string.Format(@"UPDATE a SET {0}=(SELECT SUM({0}) FROM {1} WHERE {2}=a.{2}),{3}=(SELECT SUM({3}) FROM {1} 
            //WHERE {2}=a.{2}),{4}=(SELECT SUM({4}) FROM {1} WHERE {2}=a.{2}),", tong, file, ma_cq, vat, tongcong);
            //if (extraCols != null)
            //    foreach (var item in extraCols)
            //        sql += string.Format("{0}=(SELECT SUM({0}) FROM {1} WHERE {2}=a.{2}),", item, file, ma_cq);
            //sql = sql.Trim(',');
            //sql += string.Format(" FROM {0} AS a WHERE dupe_flag=2", file);
            //TM.OleDBF.Execute(string.Format(@"UPDATE a SET {0}=(SELECT SUM({0}) FROM {1} WHERE {2}=a.{2}) FROM {1} AS a WHERE {3}=2", tong, file, ma_cq, dupe_flag), "Cập nhật tổng các đối tượng bị trùng - " + extraEX);
            //TM.OleDBF.Execute(string.Format(@"UPDATE a SET {0}=(SELECT SUM({0}) FROM {1} WHERE {2}=a.{2}) FROM {1} AS a WHERE {3}=2", vat, file, ma_cq, dupe_flag), "Cập nhật VAT các đối tượng bị trùng - " + extraEX);
            //TM.OleDBF.Execute(string.Format(@"UPDATE a SET {0}=(SELECT SUM({0}) FROM {1} WHERE {2}=a.{2}) FROM {1} AS a WHERE {3}=2", tongcong, file, ma_cq, dupe_flag), "Cập nhật tổng cộng các đối tượng bị trùng - " + extraEX);
            if (colsNumberSum != null)
                foreach (var item in colsNumberSum)
                    TM.OleDBF.Execute($"UPDATE a SET {item}=(SELECT SUM({item}) FROM {file} WHERE {ma_cq}=a.{ma_cq}) FROM {file} AS a WHERE {dupe_flag}=2", "Cập nhật " + item + " của các đối tượng bị trùng - " + extraEX);

            if (colsString != null)
                foreach (var item in colsString)
                    try { TM.OleDBF.Execute($"UPDATE a SET {item}=(SELECT MAX({item}) FROM {file} WHERE {ma_cq}=a.{ma_cq} AND {item} is NOT null AND NOT EMPTY({item})) FROM {file} as a WHERE a.{item} is null OR EMPTY(a.{item})", "Cập nhật " + item + " của các đối tượng bị trùng - " + extraEX); } catch { }
            //if (colsString != null)
            //    foreach (var item in colsString)
            //    {
            //        var dt = TM.OleDBF.ToDataTable($"SELECT {ma_cq},{item} FROM {file} WHERE {item} is NOT null");
            //        foreach (System.Data.DataRow row in dt.Rows)
            //        {
            //            TM.OleDBF.Execute($"UPDATE {file} SET {item}='{row[item].ToString()}' WHERE {ma_cq}='{row[ma_cq].ToString()}'", "Cập nhật " + item + " của các đối tượng bị trùng - " + extraEX);
            //        }
            //    }

            //Tạo bảng chứa bản ghi trùng mã
            TM.IO.FileDirectory.Copy(DataSource + file + ".dbf", DataSource + fileDuplication);
            FileManagerController.InsertFile(DataSource + fileDuplication);
            TM.OleDBF.Execute($"DELETE FROM {fileDuplication}", extraEX);
            TM.OleDBF.Execute($"PACK {fileDuplication}", extraEX);
            TM.OleDBF.Execute($"INSERT INTO {fileDuplication} SELECT * FROM {file} WHERE {dupe_flag}=1", extraEX);

            //Tạo bảng chứa bản ghi có mã là NULL hoặc mã là Empty
            TM.IO.FileDirectory.Copy(DataSource + file + ".dbf", DataSource + fileEmptyNull);
            FileManagerController.InsertFile(DataSource + fileEmptyNull);
            TM.OleDBF.Execute($"DELETE FROM {fileEmptyNull}", extraEX);
            TM.OleDBF.Execute($"PACK {fileEmptyNull}", extraEX);
            TM.OleDBF.Execute($"INSERT INTO {fileEmptyNull} SELECT * FROM {file} WHERE {ma_cq} is null OR {ma_cq}==''", extraEX);

            //Xóa các đối tượng trùng
            TM.OleDBF.Execute($"DELETE FROM {file} WHERE {dupe_flag}=1", "Xóa các đối tượng trùng - " + extraEX);
            TM.OleDBF.Execute($"PACK {file}", extraEX);

            //Xóa rác
            TM.IO.FileDirectory.Delete(DataSource + file + ".BAK");
            TM.IO.FileDirectory.ReExtensionToLower(DataSource + file + ".DBF");
        }

        //Thanh toán trước, đặt cọc
        private void AddPaidProcess(string DataSource, string file, string time, string account, string cuoc_sd)
        {
            var paidTime = DateTime.Parse(time.Substring(0, 4) + "/" + time.Substring(4, 2) + "/01");
            try { TM.OleDBF.Execute(DataSource, $"ALTER TABLE {file} ADD COLUMN {tratruoc} n(15,2)", "Tạo cột - " + tratruoc); } catch { }
            try { TM.OleDBF.Execute(DataSource, $"ALTER TABLE {file} ADD COLUMN {istratruoc} n(1)", "Tạo cột - " + istratruoc); } catch { }

            TM.OleDBF.Execute(DataSource, $"UPDATE {file} SET {tratruoc}=0", "Cập nhật 0 - " + tratruoc);
            TM.OleDBF.Execute(DataSource, $"UPDATE {file} SET {istratruoc}=0", "Cập nhật 0 - " + istratruoc);

            var dataTraTruoc = db.cuocTraTienTruocs.Where(m => m.type == 1 && m.thang_bd <= paidTime && paidTime <= m.thang_kt).ToList();
            var str = "";
            var i = 0;
            //
            //var qry = $"UPDATE {file} SET cuoc_tb=0,tong=cuoc_sd,vat=ROUND(cuoc_sd/10,0),tong_cong=cuoc_sd+ROUND(cuoc_sd/10,0),{tratruoc}={cuoc_sd}*1.1,{istratruoc}=1 WHERE {account} in ({str.Trim(',')})";
            foreach (var item in dataTraTruoc)
            {
                i++;
                str += $"'{item.account}',";
                if (item.account == "quylanhcg")
                    str = "";
                if (i % 50 == 0)
                {
                    TM.OleDBF.Execute(DataSource, $"UPDATE {file} SET {tratruoc}={cuoc_sd}*1.1,{istratruoc}=1 WHERE {account} in ({str.Trim(',')})", "Xử lý - " + file + ": " + tratruoc);
                    //TM.OleDBF.Execute(DataSource, $"UPDATE {file} SET cuoc_tb=0,tong=cuoc_sd,vat=ROUND(cuoc_sd/10,0),tong_cong=cuoc_sd+ROUND(cuoc_sd/10,0),{tratruoc}={cuoc_sd}*1.1,{istratruoc}=1 WHERE {account} in ({str.Trim(',')})", "Xử lý - " + tratruoc);
                    str = "";
                }
            }
            TM.OleDBF.Execute(DataSource, $"UPDATE {file} SET {tratruoc}={cuoc_sd}*1.1,{istratruoc}=1 WHERE {account} in ({str.Trim(',')})", "Xử lý - " + file + ": " + tratruoc);
        }
        private void AddDepositProcess(string DataSource, string file, string time, string account, string tong_cong)
        {
            var paidTime = DateTime.Parse(time.Substring(0, 4) + "/" + time.Substring(4, 2) + "/01");
            try { TM.OleDBF.Execute(DataSource, $"ALTER TABLE {file} ADD COLUMN {datcoc} n(15,2)", "Tạo cột - " + datcoc); } catch { }
            try { TM.OleDBF.Execute(DataSource, $"ALTER TABLE {file} ADD COLUMN {isdatcoc} n(1)", "Tạo cột - " + isdatcoc); } catch { }

            TM.OleDBF.Execute(DataSource, $"UPDATE {file} SET {datcoc}=0", "Cập nhật 0 - " + datcoc);
            TM.OleDBF.Execute(DataSource, $"UPDATE {file} SET {isdatcoc}=0", "Cập nhật 0 - " + isdatcoc);

            var dataDatcoc = db.cuocTraTienTruocs.Where(m => m.tien > 0 && m.type == 2 && m.thang_bd <= paidTime).ToList();
            var i = 0;
            //
            foreach (var item in dataDatcoc)
            {
                i++;
                TM.OleDBF.Execute(DataSource, $"UPDATE {file} SET {datcoc}={item.tien}-{tong_cong},{isdatcoc}=1 WHERE {account}='{item.account}'", "Xử lý - " + datcoc);
            }
            //
            //var dt = TM.OleDBF.ToDataTable($"SELECT * FROM {file} WHERE {isdatcoc}=1");
            //foreach (System.Data.DataRow item in dt.Rows)
            //{
            //    if (decimal.Parse(item["datcoc"].ToString()) >= 0)
            //    {
            //        TM.OleDBF.Execute(DataSource, $"UPDATE {file} SET tong=0,vat=0,{tong_cong}=0 WHERE {account}='{item["account"].ToString()}'", "Xử lý - " + datcoc);
            //    }
            //    else
            //        TM.OleDBF.Execute(DataSource, $"UPDATE {file} SET {tong_cong}={datcoc}*-1,tong=ROUND(({datcoc}*-1)/1.1,0),vat=ROUND(({datcoc}*-1)/10,0) WHERE {account}='{item[account].ToString()}'", "Xử lý - " + datcoc);

            //    var tmpDt = TM.OleDBF.ToDataTable($"SELECT * FROM {file} WHERE {account}='{item[account].ToString()}");
            //    var objDatcoc = db.cuocTraTienTruocs.FirstOrDefault(m => m.account == item[account].ToString());
            //    var tmpDatcoc = decimal.Parse(tmpDt.Rows[0][datcoc].ToString());
            //    objDatcoc.tien = tmpDatcoc > 0 ? tmpDatcoc : 0;
            //    db.Entry(objDatcoc).State = System.Data.Entity.EntityState.Modified;
            //    db.SaveChanges();
            //}
        }
        //Check
        public string ReplaceEscape(string str)
        {
            str = str.Replace("'", "''");
            return str;
        }
        public bool checkFile(string str, string reg)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(str, reg);
        }
        public bool isHD(string str)
        {
            return checkFile(str, @"hd\d{4}.dbf");
        }
        public bool isHDNET(string str)
        {
            return checkFile(str, @"hdnet\d{4}.dbf");
        }
        public bool isHDTV(string str)
        {
            return checkFile(str, @"hdtv\d{4}.dbf");
        }
        public bool checkAll(List<string> fileName)
        {
            foreach (var item in fileName)
                if (!checkFile(item, @"(hd|hdnet|hdtv)\d{4}.dbf")) return false;
            return true;
        }
        //Fix Month Year
        private string FixMonthYear(string time)
        {
            // "20" + time[2].ToString() + time[3].ToString() + time[0].ToString() + time[1].ToString();
            return "";
        }
        //Remove Fil eSource
        private DeleteFileList RemoveFileSource(string source, string extension = ".bak", List<string> exception = null)
        {
            var rs = new DeleteFileList();
            try
            {
                var fileList = TM.IO.FileDirectory.FilesToList(source);
                if (exception == null)
                {
                    foreach (var item in fileList)
                        if (item.ToExtension().ToLower() == extension)
                        {
                            FileManagerController.DeleteDirFile(source + item);
                            rs.CountDelete++;
                        }
                }
                else
                {
                    foreach (var item in fileList)
                        if (!exception.Contains(item.ToLower()))
                        {
                            FileManagerController.DeleteDirFile(TM.OleDBF.DataSource + item);
                            rs.CountDelete++;
                        }
                        else
                            rs.CountException++;
                }
            }
            catch (Exception) { }
            return rs;
        }
        //Hóa đơn điện tử
        private string getProduct(string ProdName, string ProdUnit, string ProdQuantity, string ProdPrice, string Amount)
        {
            return string.Format(@"<Product><ProdName>{0}</ProdName><ProdUnit>{1}</ProdUnit><ProdQuantity>{2}</ProdQuantity><ProdPrice>{3}</ProdPrice><Amount>{4}</Amount></Product>",
                ProdName, ProdUnit, ProdQuantity, ProdPrice, Amount);
        }
        private string getCusPhone(string acc_net, string acc_tv, string so_cd, string so_dd)
        {
            if (!string.IsNullOrWhiteSpace(acc_net))
                return acc_net;
            else if (!string.IsNullOrWhiteSpace(acc_tv))
                return acc_tv;
            else if (!string.IsNullOrWhiteSpace(so_cd))
                return so_cd;
            else
                return so_dd;
        }
        private string fixMaThanhToan(string ma_cq, string preFixMain = "06", int dfLenght = 13)
        {
            ma_cq = ma_cq.Trim();
            var count = ma_cq.Length;
            var preFixMaCQ = "";
            if (count < dfLenght)
                for (int i = 0; i < dfLenght - count; i++)
                {
                    preFixMaCQ = " " + preFixMaCQ;
                }
            else if (count > dfLenght) dfLenght = count;

            return preFixMain + dfLenght + ma_cq + preFixMaCQ;
        }
        private string getMaThanhToanHD(string valueTime, string ma_cq, string Details)
        {
            string first = "0002010102112620970415010686973800115204123453037045802VN5910VIETINBANK6005HANOI6106100000";
            string time = "0106" + valueTime;
            string province = "0703" + HDDT.ProvinceCode.BCN;
            string QRType = "0818" + (int)HDDT.QRCodeType.HDDT;
            string last = time + fixMaThanhToan(ma_cq) + province + QRType + Details;
            string tagLength = "62" + last.Length.ToString();
            return first + tagLength + last;
            //"<MaThanhToan><![CDATA[0002010102112620970415010686973800115204123453037045802VN5909VIETINBANK6005HANOI6106100000626301060720170613  024357434690703BCN08172CUOC MANG CODINH]]></MaThanhToan>"
        }
        private string getMaThanhToanKH(string valueTime, string ma_cq, string Details)
        {
            string first = "0002010102112620970415010686973800115204123453037045802VN5910VIETINBANK6005HANOI6106100000";
            string province = "0703" + HDDT.ProvinceCode.BCN;
            string QRType = "0818" + (int)HDDT.QRCodeType.HDDT;
            string last = fixMaThanhToan(ma_cq) + province + QRType + Details;
            string tagLength = "62" + last.Length.ToString();
            return first + tagLength + last;
        }
    }
    //Class object
    public class DeleteFileList
    {
        public int CountDelete { get; set; }
        public int CountException { get; set; }
    }
    public class TotalHDAll
    {
        public decimal tong_cd, tong_dd, tong_net, tong_tv, tong, vat, kthue, gtru, tongcong, datcoc, tratruoc;
    }
    public class TotalHDCD
    {
        public decimal tong, vat, tongcong;
    }
    public class TotalHDDD
    {
        public decimal tong, kthue, gtru, vat, tongcong;
    }
    public class TotalHDNET
    {
        public decimal tong, vat, tongcong, datcoc, tratruoc;
    }
    public class TotalHDTV
    {
        public decimal tong, vat, tongcong, datcoc, tratruoc;
    }
    public static class CommonHDALL
    {
        public static Dictionary<int, string> stk_ma_dvi = new Dictionary<int, string>()
        {
            {2,"8605 201 001805 t¹i Ng©n hµng N«ng nghiÖp & PTNT huyÖn Ba BÓ, tØnh B¾c K¹n" },
            {3,"8607 201 001374 t¹i Ng©n hµng N«ng nghiÖp & PTNT huyÖn B¹ch Th«ng, tØnh B¾c K¹n" },
            {4,"8601 201 001712 t¹i Ng©n hµng N«ng nghiÖp & PTNT huyÖn Chî §ån, tØnh B¾c K¹n" },
            {5,"8606 201 001366 t¹i Ng©n hµng N«ng nghiÖp & PTNT huyÖn Chî Míi, tØnh B¾c K¹n" },
            {6,"8603 201 001541 t¹i Ng©n hµng N«ng nghiÖp & PTNT huyÖn Na R×, tØnh B¾c K¹n" },
            {7,"8604 201 001429 t¹i Ng©n hµng N«ng nghiÖp & PTNT huyÖn Ng©n S¬n, tØnh B¾c K¹n" },
            {8,"8602 201 001375 t¹i Ng©n hµng N«ng nghiÖp & PTNT huyÖn P¸c NÆm, tØnh B¾c K¹n" },
        };
    }
    public class HDDT
    {
        public enum QRCodeType
        {
            Portal = 1,
            HDDT = 2,
            HDGiay = 3,
            AnPhamThuCuoc = 4, //(thông báo cước, biên lai, phiếu thu
            AnPhamQLTT = 5 //bản kê thu cước
        }
        public enum ProvinceCode
        {
            HNI = 1,
            BCN = 2,
        }
        public const string DetailCoDinh = "CUOC MANG CO DINH";
        public const string DetailDiDong = "CUOC MANG DI DONG";
    }
}