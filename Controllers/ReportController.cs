using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TM.Message;
using Dapper;
using TM.Helper;

namespace Portal.Controllers
{
    [Filters.AuthVinaphone]
    public class ReportController : BaseController
    {
        private static string DataSource;
        static string file_pttb = "pttb";
        //static string file_bss = "bss";
        static string hdall = "hdall", hdcd = "hdcd", hddd = "hddd", hdnet = "hdnet", hdtv = "hdtv";
        // GET: Report
        public ActionResult Index()
        {

            return View();
        }
        public ActionResult ReportLocal()
        {
            DataSource = "Uploads/Data/HDData";
            ViewBag.directory = TM.IO.FileDirectory.DirectoriesToList(DataSource).OrderByDescending(d => d);
            ViewBag.local = db.locals.Where(d => d.flag > 0).ToList();
            return View();
        }
        public ActionResult ExportTotal(string datatime)
        {
            try
            {
                var dt = new System.Data.DataTable();
                dt.Columns.Add("cbt");
                dt.Columns.Add("donvi");
                dt.Columns.Add("dia_chi");
                dt.Columns.Add("ma_cq");
                dt.Columns.Add("acc_net");
                dt.Columns.Add("acc_tv");
                dt.Columns.Add("so_dd");
                dt.Columns.Add("so_cd");
                dt.Columns.Add("ten_tb");
                dt.Columns.Add("diachi_tb");
                dt.Columns.Add("tong");
                dt.Columns.Add("vat");
                dt.Columns.Add("tongcong");
                var time = datatime.Split('-')[0].Replace("time", "");
                time = "20" + time[2].ToString() + time[3].ToString() + "-" + time[0].ToString() + time[1].ToString() + "-01";
                var list = Connection().Query(@"select c.*,b.tong,b.vat,b.tongcong,cs.nvql,cs.dia_chi as dia_chi_nvql,l.title as donvi from 
                            customer_info c inner join bill_month b on c.id = b.customer_id
                            inner join collected_staff cs on c.ma_cbt = cs.id
                            inner join local l on c.ma_dvi = l.id
                            where c.created_at=@time and c.flag>0 and b.flag>0 and cs.flag>0 and l.flag>0",
                            new { time = time }).ToList();
                foreach (var item in list)
                    dt.Rows.Add(item.nvql, item.donvi, item.dia_chi_nvql,
                        item.ma_cq, item.acc_net, item.acc_tv, item.so_dd, item.so_cd, item.ten_tb, item.dia_chi, item.tong, item.vat, item.tongcong);

                //var ListCBT = GetDataCBT(0, time);
                //foreach (var item in ListCBT)
                //{
                //    var ListCustomerCBT = GetDataCustomerCBT(item[0], time);
                //    foreach (var i in ListCustomerCBT)
                //    {
                //        string acc = i.acc_net + "," + i.acc_tv + "," + i.so_dd + "," + i.so_cd;
                //        dt.Rows.Add(item[0], item[1], item[2], item[3], i.ma_cq, acc, i.ten_tb, i.dia_chi, i.tong, i.vat, i.tongcong);
                //    }
                //}
                TM.Exports.ExportExcel(dt, "ExportTotal-" + time);
            }
            catch (Exception ex) { this.danger(ex.Message); }
            return RedirectToAction("Index");
        }
        //public ActionResult ExportTotalMerginNVQL(string time, int local)
        //{
        //    var rs = new List<string[]>();
        //    try
        //    {
        //        TM.OleDBF.DataSource = "Uploads/Data/HDData/" + time;

        //        //PortalContextConnectionString();
        //        var collected_staff = new List<Models.collected_staff>();
        //        if (local == 0)
        //            collected_staff = db.collected_staff.Where(d => d.flag > 0).OrderBy(d => d.nvql).ToList();
        //        else
        //            collected_staff = db.collected_staff.Where(d => d.local_id == local && d.flag > 0).OrderBy(d => d.nvql).ToList();
        //        foreach (var item in collected_staff)
        //        {
        //            ////HDCD
        //            //var tmp = TM.OleDBF.ToDataTable("SELECT SUM(vat) as vat,SUM(tong) as tong,SUM(tongcong) as tongcong FROM " + file + " WHERE ma_cbt=" + item.id);
        //            ////var tmp = TM.SQLDB.ToDataTable("SELECT COUNT(*) as tongtb,SUM(vat) as vat,SUM(tong) as tong,SUM(tongcong) as tongcong FROM bill_month WHERE ma_cbt=" + item.id + " AND created_at='" + time + "'");
        //            //if (tmp.Rows.Count > 0)
        //            //    rs.Add(new[] {
        //            //    item.id.ToString(),
        //            //    item.local_id.ToString(),
        //            //    item.nvql,
        //            //    item.dia_chi,
        //            //    tmp.Rows[0][0] == null || String.IsNullOrEmpty(tmp.Rows[0][0].ToString()) ?  "0" : tmp.Rows[0][0].ToString(),
        //            //    tmp.Rows[0][1] == null || String.IsNullOrEmpty(tmp.Rows[0][1].ToString()) ?  "0" : decimal.Parse(tmp.Rows[0][1].ToString()).ToString("N2"),
        //            //    tmp.Rows[0][2] == null || String.IsNullOrEmpty(tmp.Rows[0][2].ToString()) ?  "0" : decimal.Parse(tmp.Rows[0][2].ToString()).ToString("N0"),
        //            //    tmp.Rows[0][3] == null || String.IsNullOrEmpty(tmp.Rows[0][3].ToString()) ?  "0" : decimal.Parse(tmp.Rows[0][3].ToString()).ToString("N0")
        //            //});

        //            var tmpHDCD = TM.OleDBF.ToDataTable("SELECT " +
        //                    "(select SUM(tong) FROM " + hdcd + time + " WHERE LEFT(so_tb,2)='38' AND ma_cbt=" + item.id + ") as cd," +
        //                    "(select COUNT(tong) FROM " + hdcd + time + " WHERE LEFT(so_tb,2)='38' AND ma_cbt=" + item.id + ") as count_cd," +
        //                    "(select SUM(tong) FROM " + hdcd + time + " WHERE LEFT(so_tb,2)!='38' AND ma_cbt=" + item.id + ") as gphone," +
        //                    "(select COUNT(tong) FROM " + hdcd + time + " WHERE LEFT(so_tb,2)!='38' AND ma_cbt=" + item.id + ") as count_gphone," +
        //                    "SUM(tong) as tong,COUNT(tong) as count_cds FROM " + hdcd + time + " WHERE ma_cbt=" + item.id, hdcd);
        //            var tmpHDNET = TM.OleDBF.ToDataTable("SELECT " +
        //                    "(SELECT SUM(tong) FROM " + hdnet + time + " WHERE (kieu='BASIC' OR kieu='EASY' OR kieu='FAMILY') AND ma_cbt=" + item.id + ") as mega," +
        //                    "(SELECT COUNT(tong) FROM " + hdnet + time + " WHERE (kieu='BASIC' OR kieu='EASY' OR kieu='FAMILY') AND ma_cbt=" + item.id + ") as count_mega," +
        //                    "(SELECT SUM(tong) FROM " + hdnet + time + " WHERE kieu!='BASIC' AND kieu!='EASY' AND kieu!='FAMILY' AND ma_cbt=" + item.id + ") as fiber," +
        //                    "(SELECT COUNT(tong) FROM " + hdnet + time + " WHERE kieu!='BASIC' AND kieu!='EASY' AND kieu!='FAMILY' AND ma_cbt=" + item.id + ") as count_fiber," +
        //                    "SUM(tong) as tong,COUNT(tong) as count_net FROM " + hdnet + time + " WHERE ma_cbt=" + item.id, hdnet);
        //            var tmpHDDD = TM.OleDBF.ToDataTable("SELECT SUM(cuoc_cthue+cuoc_kthue+giamtru) as tong,COUNT(*) as count_dd FROM " + hddd + time + " WHERE ma_cbt=" + item.id, hddd);
        //            var tmpHDTV = TM.OleDBF.ToDataTable("SELECT SUM(tong) as tong,COUNT(*) as count_tv FROM " + hdtv + time + " WHERE ma_cbt=" + item.id, hdtv);
        //            rs.Add(new[] {
        //                item.id.ToString(),
        //                item.local_id.ToString(),
        //                item.nvql,
        //                item.dia_chi,
        //                //Cố định
        //                tmpHDCD.Rows[0][0] == null || String.IsNullOrEmpty(tmpHDCD.Rows[0][0].ToString()) ?  "0" : decimal.Parse(tmpHDCD.Rows[0][0].ToString()).ToString("N0"),
        //                tmpHDCD.Rows[0][1] == null || String.IsNullOrEmpty(tmpHDCD.Rows[0][1].ToString()) ?  "0" : tmpHDCD.Rows[0][1].ToString(),
        //                tmpHDCD.Rows[0][2] == null || String.IsNullOrEmpty(tmpHDCD.Rows[0][2].ToString()) ?  "0" : decimal.Parse(tmpHDCD.Rows[0][2].ToString()).ToString("N0"),
        //                tmpHDCD.Rows[0][3] == null || String.IsNullOrEmpty(tmpHDCD.Rows[0][3].ToString()) ?  "0" : tmpHDCD.Rows[0][3].ToString(),
        //                tmpHDCD.Rows[0][4] == null || String.IsNullOrEmpty(tmpHDCD.Rows[0][4].ToString()) ?  "0" : decimal.Parse(tmpHDCD.Rows[0][4].ToString()).ToString("N0"),
        //                tmpHDCD.Rows[0][5] == null || String.IsNullOrEmpty(tmpHDCD.Rows[0][5].ToString()) ?  "0" : tmpHDCD.Rows[0][5].ToString(),
        //                //Net
        //                tmpHDNET.Rows[0][0] == null || String.IsNullOrEmpty(tmpHDNET.Rows[0][0].ToString()) ?  "0" : decimal.Parse(tmpHDNET.Rows[0][0].ToString()).ToString("N0"),
        //                tmpHDNET.Rows[0][1] == null || String.IsNullOrEmpty(tmpHDNET.Rows[0][1].ToString()) ?  "0" : tmpHDNET.Rows[0][1].ToString(),
        //                tmpHDNET.Rows[0][2] == null || String.IsNullOrEmpty(tmpHDNET.Rows[0][2].ToString()) ?  "0" : decimal.Parse(tmpHDNET.Rows[0][2].ToString()).ToString("N0"),
        //                tmpHDNET.Rows[0][3] == null || String.IsNullOrEmpty(tmpHDNET.Rows[0][3].ToString()) ?  "0" : tmpHDNET.Rows[0][3].ToString(),
        //                tmpHDNET.Rows[0][4] == null || String.IsNullOrEmpty(tmpHDNET.Rows[0][4].ToString()) ?  "0" : decimal.Parse(tmpHDNET.Rows[0][4].ToString()).ToString("N0"),
        //                tmpHDNET.Rows[0][5] == null || String.IsNullOrEmpty(tmpHDNET.Rows[0][5].ToString()) ?  "0" : tmpHDNET.Rows[0][5].ToString(),
        //                //Di động
        //                tmpHDDD.Rows[0][0] == null || String.IsNullOrEmpty(tmpHDDD.Rows[0][0].ToString()) ?  "0" : decimal.Parse(tmpHDDD.Rows[0][0].ToString()).ToString("N0"),
        //                tmpHDDD.Rows[0][1] == null || String.IsNullOrEmpty(tmpHDDD.Rows[0][1].ToString()) ?  "0" : tmpHDDD.Rows[0][1].ToString(),
        //                //Mytv
        //                tmpHDTV.Rows[0][0] == null || String.IsNullOrEmpty(tmpHDTV.Rows[0][0].ToString()) ?  "0" : decimal.Parse(tmpHDTV.Rows[0][0].ToString()).ToString("N0"),
        //                tmpHDTV.Rows[0][1] == null || String.IsNullOrEmpty(tmpHDTV.Rows[0][1].ToString()) ?  "0" : tmpHDTV.Rows[0][1].ToString()
        //            });
        //        }
        //    }
        //    catch (Exception) { throw; }
        //    return rs;
        //}

        public ActionResult ExportCBTNull(string datatime)
        {
            try
            {
                var time = datatime.Split('-')[0].Replace("time", "");
                //time = "20" + time[2].ToString() + time[3].ToString() + "-" + time[0].ToString() + time[1].ToString() + "-01";

                TM.OleDBF.DataSource = "Uploads/Data/HDData/" + time;
                var collectedStaffID = db.collected_staff.ToList().Select(d => decimal.Parse(d.id.ToString())).ToList();
                var a = TM.OleDBF.Connection().Query<Portal.Areas.dvcntt.Models.hdall>("select * from " + hdall + time).Where(d => !collectedStaffID.Contains(d.ma_cbt)).ToList();
                var dt = TM.Helper.Data.ToDataTable(a);
                TM.Exports.ExportExcel(dt, "danh sách cán bộ thu còn thiếu-" + time);

                //
                //return cmd.ExecuteReader();

                //var dt = TM.SQLDB.ToDataTable("select * from customer_info where ma_cbt not in (select id from collected_staff) and created_at='" + time + "'");
                //var dt = TM.OleDBF.ToDataTable("select * from " + hdall + time).wh;
                //TM.Exports.ExportExcel(dt, "danh sách cán bộ thu còn thiếu-" + time);
            }
            catch (Exception ex) { this.danger(ex.Message); }
            return RedirectToAction("Index");
        }
        [HttpGet]
        public JsonResult GetMaDonVi(string uid)
        {
            try
            {
                var local = db.locals.Where(d => d.flag > 0).ToList();
                return Json(new { success = "Xử lý thành công", data = local }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message }, JsonRequestBehavior.AllowGet); }//"Xử lý lỗi vui lòng thử lại!" 
        }
        [HttpGet]
        public JsonResult GetCBT(string data)
        {
            try
            {
                var tmp_data = data.Split('-');
                var time = tmp_data[0].Replace("#time", "");
                //time = "20" + time[2].ToString() + time[3].ToString() + "-" + time[0].ToString() + time[1].ToString() + "-01";
                var local = long.Parse(tmp_data[1].Replace("local", ""));
                return Json(new { success = "Xử lý thành công", data = GetDataCBT(local, time) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message }, JsonRequestBehavior.AllowGet); }//"Xử lý lỗi vui lòng thử lại!" 
        }
        [HttpGet]
        public JsonResult GetCustomerCBT(string dataid, string datatime)
        {
            try
            {
                var time = datatime.Split('-')[0].Replace("#time", "");
                time = "20" + time[2].ToString() + time[3].ToString() + "-" + time[0].ToString() + time[1].ToString() + "-01";

                return Json(new { success = "Xử lý thành công", data = GetDataCustomerCBT(dataid, time) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message }, JsonRequestBehavior.AllowGet); }//"Xử lý lỗi vui lòng thử lại!" 
        }
        private new List<string[]> GetDataCBT(long local, string time)
        {
            var rs = new List<string[]>();
            try
            {
                TM.OleDBF.DataSource = "Uploads/Data/HDData/" + time;

                //PortalContextConnectionString();
                var collected_staff = new List<Models.collected_staff>();
                if (local == 0)
                    collected_staff = db.collected_staff.Where(d => d.flag > 0).OrderBy(d => d.nvql).ToList();
                else
                    collected_staff = db.collected_staff.Where(d => d.local_id == local && d.flag > 0).OrderBy(d => d.nvql).ToList();
                foreach (var item in collected_staff)
                {
                    ////HDCD
                    //var tmp = TM.OleDBF.ToDataTable("SELECT SUM(vat) as vat,SUM(tong) as tong,SUM(tongcong) as tongcong FROM " + file + " WHERE ma_cbt=" + item.id);
                    ////var tmp = TM.SQLDB.ToDataTable("SELECT COUNT(*) as tongtb,SUM(vat) as vat,SUM(tong) as tong,SUM(tongcong) as tongcong FROM bill_month WHERE ma_cbt=" + item.id + " AND created_at='" + time + "'");
                    //if (tmp.Rows.Count > 0)
                    //    rs.Add(new[] {
                    //    item.id.ToString(),
                    //    item.local_id.ToString(),
                    //    item.nvql,
                    //    item.dia_chi,
                    //    tmp.Rows[0][0] == null || String.IsNullOrEmpty(tmp.Rows[0][0].ToString()) ?  "0" : tmp.Rows[0][0].ToString(),
                    //    tmp.Rows[0][1] == null || String.IsNullOrEmpty(tmp.Rows[0][1].ToString()) ?  "0" : decimal.Parse(tmp.Rows[0][1].ToString()).ToString("N2"),
                    //    tmp.Rows[0][2] == null || String.IsNullOrEmpty(tmp.Rows[0][2].ToString()) ?  "0" : decimal.Parse(tmp.Rows[0][2].ToString()).ToString("N0"),
                    //    tmp.Rows[0][3] == null || String.IsNullOrEmpty(tmp.Rows[0][3].ToString()) ?  "0" : decimal.Parse(tmp.Rows[0][3].ToString()).ToString("N0")
                    //});

                    var tmpHDCD = TM.OleDBF.ToDataTable("SELECT " +
                            "(select SUM(tong) FROM " + hdcd + time + " WHERE LEFT(so_tb,2)='38' AND ma_cbt=" + item.id + ") as cd," +
                            "(select COUNT(tong) FROM " + hdcd + time + " WHERE LEFT(so_tb,2)='38' AND ma_cbt=" + item.id + ") as count_cd," +
                            "(select SUM(tong) FROM " + hdcd + time + " WHERE LEFT(so_tb,2)!='38' AND ma_cbt=" + item.id + ") as gphone," +
                            "(select COUNT(tong) FROM " + hdcd + time + " WHERE LEFT(so_tb,2)!='38' AND ma_cbt=" + item.id + ") as count_gphone," +
                            "SUM(tong) as tong,COUNT(tong) as count_cds FROM " + hdcd + time + " WHERE ma_cbt=" + item.id, hdcd);
                    var tmpHDNET = TM.OleDBF.ToDataTable("SELECT " +
                            "(SELECT SUM(tong) FROM " + hdnet + time + " WHERE (kieu='BASIC' OR kieu='EASY' OR kieu='FAMILY') AND ma_cbt=" + item.id + ") as mega," +
                            "(SELECT COUNT(tong) FROM " + hdnet + time + " WHERE (kieu='BASIC' OR kieu='EASY' OR kieu='FAMILY') AND ma_cbt=" + item.id + ") as count_mega," +
                            "(SELECT SUM(tong) FROM " + hdnet + time + " WHERE kieu!='BASIC' AND kieu!='EASY' AND kieu!='FAMILY' AND ma_cbt=" + item.id + ") as fiber," +
                            "(SELECT COUNT(tong) FROM " + hdnet + time + " WHERE kieu!='BASIC' AND kieu!='EASY' AND kieu!='FAMILY' AND ma_cbt=" + item.id + ") as count_fiber," +
                            "SUM(tong) as tong,COUNT(tong) as count_net FROM " + hdnet + time + " WHERE ma_cbt=" + item.id, hdnet);
                    var tmpHDDD = TM.OleDBF.ToDataTable("SELECT SUM(cuoc_cthue+cuoc_kthue+giamtru) as tong,COUNT(*) as count_dd FROM " + hddd + time + " WHERE ma_cbt1=" + item.id, hddd);
                    var tmpHDTV = TM.OleDBF.ToDataTable("SELECT SUM(tong) as tong,COUNT(*) as count_tv FROM " + hdtv + time + " WHERE ma_cbt=" + item.id, hdtv);
                    rs.Add(new[] {
                        item.id.ToString(),
                        item.local_id.ToString(),
                        item.nvql,
                        item.dia_chi,
                        //Cố định
                        tmpHDCD.Rows[0][0] == null || String.IsNullOrEmpty(tmpHDCD.Rows[0][0].ToString()) ?  "0" : decimal.Parse(tmpHDCD.Rows[0][0].ToString()).ToString("N0"),
                        tmpHDCD.Rows[0][1] == null || String.IsNullOrEmpty(tmpHDCD.Rows[0][1].ToString()) ?  "0" : tmpHDCD.Rows[0][1].ToString(),
                        tmpHDCD.Rows[0][2] == null || String.IsNullOrEmpty(tmpHDCD.Rows[0][2].ToString()) ?  "0" : decimal.Parse(tmpHDCD.Rows[0][2].ToString()).ToString("N0"),
                        tmpHDCD.Rows[0][3] == null || String.IsNullOrEmpty(tmpHDCD.Rows[0][3].ToString()) ?  "0" : tmpHDCD.Rows[0][3].ToString(),
                        tmpHDCD.Rows[0][4] == null || String.IsNullOrEmpty(tmpHDCD.Rows[0][4].ToString()) ?  "0" : decimal.Parse(tmpHDCD.Rows[0][4].ToString()).ToString("N0"),
                        tmpHDCD.Rows[0][5] == null || String.IsNullOrEmpty(tmpHDCD.Rows[0][5].ToString()) ?  "0" : tmpHDCD.Rows[0][5].ToString(),
                        //Net
                        tmpHDNET.Rows[0][0] == null || String.IsNullOrEmpty(tmpHDNET.Rows[0][0].ToString()) ?  "0" : decimal.Parse(tmpHDNET.Rows[0][0].ToString()).ToString("N0"),
                        tmpHDNET.Rows[0][1] == null || String.IsNullOrEmpty(tmpHDNET.Rows[0][1].ToString()) ?  "0" : tmpHDNET.Rows[0][1].ToString(),
                        tmpHDNET.Rows[0][2] == null || String.IsNullOrEmpty(tmpHDNET.Rows[0][2].ToString()) ?  "0" : decimal.Parse(tmpHDNET.Rows[0][2].ToString()).ToString("N0"),
                        tmpHDNET.Rows[0][3] == null || String.IsNullOrEmpty(tmpHDNET.Rows[0][3].ToString()) ?  "0" : tmpHDNET.Rows[0][3].ToString(),
                        tmpHDNET.Rows[0][4] == null || String.IsNullOrEmpty(tmpHDNET.Rows[0][4].ToString()) ?  "0" : decimal.Parse(tmpHDNET.Rows[0][4].ToString()).ToString("N0"),
                        tmpHDNET.Rows[0][5] == null || String.IsNullOrEmpty(tmpHDNET.Rows[0][5].ToString()) ?  "0" : tmpHDNET.Rows[0][5].ToString(),
                        //Di động
                        tmpHDDD.Rows[0][0] == null || String.IsNullOrEmpty(tmpHDDD.Rows[0][0].ToString()) ?  "0" : decimal.Parse(tmpHDDD.Rows[0][0].ToString()).ToString("N0"),
                        tmpHDDD.Rows[0][1] == null || String.IsNullOrEmpty(tmpHDDD.Rows[0][1].ToString()) ?  "0" : tmpHDDD.Rows[0][1].ToString(),
                        //Mytv
                        tmpHDTV.Rows[0][0] == null || String.IsNullOrEmpty(tmpHDTV.Rows[0][0].ToString()) ?  "0" : decimal.Parse(tmpHDTV.Rows[0][0].ToString()).ToString("N0"),
                        tmpHDTV.Rows[0][1] == null || String.IsNullOrEmpty(tmpHDTV.Rows[0][1].ToString()) ?  "0" : tmpHDTV.Rows[0][1].ToString()
                    });
                }
            }
            catch (Exception) { throw; }
            return rs;
        }
        private new List<dynamic> GetDataCustomerCBT(string dataid, string time)
        {
            var rs = new List<dynamic>();
            try
            {
                rs = Connection().Query(
                    @"SELECT c.*,b.tong,b.vat,b.tongcong FROM customer_info c inner join bill_month b on c.id=b.customer_id 
                    WHERE c.ma_cbt=@ma_cbt AND c.created_at=@created_at",
                    new { ma_cbt = dataid, created_at = time }).ToList();
            }
            catch (Exception) { throw; }
            return rs;
        }
        [Filters.AuthVinaphone()]
        public ActionResult PTTBBSS()
        {
            DataSource = "Uploads/Data/PTTB-BSS/";
            ViewBag.directory = TM.IO.FileDirectory.DirectoriesToList(DataSource).OrderByDescending(d => d).ToList();
            return View();
        }
        [Filters.AuthVinaphone()]
        [HttpPost]
        public ActionResult PTTBBSS(FormCollection collection)
        {
            try
            {
                //Source
                TM.IO.FileDirectory.CreateDirectory(DataSource);
                var fileName = new List<string>();
                var fileSavePath = new List<string>();
                var dtMergin = new System.Data.DataTable();
                int uploadedCount = 0;
                if (Request.Files.Count > 0)
                {
                    string CurrentMonthYear = System.IO.Path.GetFileName(Request.Files[0].FileName).ToLower().RemoveWord(".");
                    DataSource += CurrentMonthYear + "/";
                    TM.IO.FileDirectory.CreateDirectory(DataSource);
                    for (int i = 0; i < Request.Files.Count; i++)
                    {
                        var file = Request.Files[i];
                        if (!file.FileName.IsExtension(new[] { ".xls" }))
                        {
                            this.danger("Tệp phải định dạng .xls");
                            return RedirectToAction("PTTBBSS");
                        }

                        if (file.ContentLength > 0)
                        {
                            fileName.Add(System.IO.Path.GetFileName(file.FileName).ToLower());
                            fileSavePath.Add(TM.IO.FileDirectory.MapPath(DataSource) + fileName[i]);
                            file.SaveAs(fileSavePath[i]);
                            uploadedCount++;
                        }
                    }
                    var rs = "Tải lên thành công </br>";
                    foreach (var item in fileName)
                        rs += item + "<br/>";
                    this.success(rs);
                }
                else
                    this.danger("Vui lòng chọn đủ tệp!");
            }
            catch (Exception ex)
            {
                this.danger(ex.Message);
            }
            return RedirectToAction("PTTBBSS");
        }
        static string[] PTTBBSSDataSource(string time)
        {
            DataSource = "Uploads/Data/PTTB-BSS/" + time + "/";
            var FilePTTB = DataSource + file_pttb + time + ".xls";
            TM.OleExcel.DataSource = TM.IO.FileDirectory.MapPath(FilePTTB);
            return new[] { DataSource, FilePTTB };
        }
        public static string ReplaceString(object str)
        {
            var tmp = str.ToString().Trim().Replace(", Việt Nam", "").Replace("- ", "-").Replace(" -", "-");
            try
            {
                tmp = System.Text.RegularExpressions.Regex.Replace(tmp, @"0[0-9]*, |0[0-9]* |[0-9]+-|SN [0-9]*, ", "");

                return System.Text.RegularExpressions.Regex.Replace(tmp, @"SN\s+", "");
            }
            catch (Exception)
            {
                return tmp;
            }
        }
        public static string[] getLocal(string local)
        {
            long local_id = 1;
            var title = "";
            switch (local.ToUpper())
            {
                case "KA":
                    local_id = 1;
                    title = "Bắc Kạn";
                    break;
                case "KB":
                    local_id = 1;
                    title = "Bắc Kạn";
                    break;
                case "K1":
                    local_id = 3;
                    title = "Bạch thông";
                    break;
                case "K2":
                    local_id = 5;
                    title = "Chợ mới";
                    break;
                case "K3":
                    local_id = 8;
                    title = "Pác Nặm";
                    break;
                case "K4":
                    local_id = 7;
                    title = "Ngân Sơn";
                    break;
                case "K5":
                    local_id = 6;
                    title = "Na rì";
                    break;
                case "K6":
                    local_id = 4;
                    title = "Chợ đồn";
                    break;
                case "K7":
                    local_id = 2;
                    title = "Ba bể";
                    break;
                default:
                    local_id = 0;
                    title = "Chưa xác định";
                    break;
            }
            return new[] { local_id.ToString(), title };
        }
        public static string getCollectedStaff()
        {
            var rs = "";
            return rs;
        }
        [Filters.AuthVinaphone()]
        public ActionResult PTTBBSSProcessData(string time)
        {
            try
            {
                PTTBBSSDataSource(time);
                var pttb_Local = TM.OleExcel.ToDataTable("SELECT stt,diachi FROM [Sheet1$]");
                for (int i = 0; i < pttb_Local.Rows.Count; i++)
                {
                    TM.OleExcel.Execute("UPDATE [Sheet1$] SET diachi='" + ReplaceString(pttb_Local.Rows[i]["diachi"]) + "' WHERE stt=" + pttb_Local.Rows[i]["stt"]);
                }
            }
            catch (Exception ex)
            {
                this.danger(ex.Message);
            }
            return RedirectToAction("PTTBBSS");
        }
        [Filters.AuthVinaphone()]
        public ActionResult PTTBBSSReport(string time3, string time4)
        {
            try
            {
                var file = PTTBBSSDataSource(time3);
                //
                TM.OleExcel.DataSource = TM.IO.FileDirectory.MapPath(file[1]);
                var pttb_monney = TM.OleExcel.ToDataTable("SELECT * FROM [Sheet1$]");
                pttb_monney.Columns.Add("donvi");
                pttb_monney.Columns.Add("nvql");
                pttb_monney.Columns.Add("diachiql");
                pttb_monney.Columns.Add("xaphuong");
                pttb_monney.Columns.Add("tothon");
                pttb_monney.Columns.Add("tongcong");
                //
                var timemonth = time3[0].ToString() + time3[1].ToString() + time3[2].ToString() + time3[3].ToString() + "-" + time3[4].ToString() + time3[5].ToString() + "-01";
                var hdall = Connection().Query("SELECT c.*,b.vat,b.tong,b.tongcong FROM customer_info c inner join bill_month b on c.id=b.customer_id WHERE c.created_at='" + timemonth + "'");
                //var filedbfall = TM.OleDBF.ToDataTable("SELECT * FROM " + filedbf);
                pttb_monney = TongTien(pttb_monney, hdall);

                //
                timemonth = time4[0].ToString() + time4[1].ToString() + time4[2].ToString() + time4[3].ToString() + "-" + time4[4].ToString() + time4[5].ToString() + "-01";
                var hdall2 = Connection().Query("SELECT c.*,b.vat,b.tong,b.tongcong FROM customer_info c inner join bill_month b on c.id=b.customer_id WHERE c.created_at='" + timemonth + "'");

                pttb_monney = TongTien(pttb_monney, hdall2);
                TM.Exports.ExportExcel(pttb_monney, "ExportPTTB" + timemonth);
            }
            catch (Exception ex)
            {
                this.danger(ex.Message);
            }
            return RedirectToAction("PTTBBSS");
        }
        private System.Data.DataTable TongTien(System.Data.DataTable dt, IEnumerable<dynamic> hdall)
        {
            var list = new List<string>();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                //Đơn vị
                var diachilap = dt.Rows[i]["diachilap"].ToString().Split(' ');
                var ngaylap = diachilap.Length > 0 ? diachilap[0].Split('/') : null;
                var ma_dvi = ngaylap.Length > 0 ? ngaylap[0] : "Null";
                var local = getLocal(ma_dvi);
                dt.Rows[i]["donvi"] = local[1];

                //Tổng tiền
                var a = dt.Rows[i]["LM"].ToString().Split(';');
                if (a.Length > 0)
                    a = a[0].Split(':');
                if (a.Length > 1 && !string.IsNullOrEmpty(a[1]))
                {
                    var id = a[1].Trim();
                    list.Add(id);
                    dynamic s = new System.Dynamic.ExpandoObject();
                    if (diachilap.Length > 1)
                        s = hdall.Where(d => d.ma_cq == diachilap[1] || d.acc_net == id || d.acc_tv == id || d.sdd == id || d.so_tb == id).FirstOrDefault();
                    else
                        s = hdall.Where(d => d.ma_cq == id || d.acc_net == id || d.acc_tv == id || d.sdd == id || d.so_tb == id).FirstOrDefault();
                    if (s != null)
                    {
                        dt.Rows[i]["tongcong"] = s.tongcong;
                        var ma_cbt = (long)s.ma_cbt;
                        var collected_staff = db.collected_staff.Where(d => d.id == ma_cbt).FirstOrDefault();
                        if (collected_staff != null)
                        {
                            dt.Rows[i]["nvql"] = collected_staff.nvql;
                            dt.Rows[i]["diachiql"] = collected_staff.dia_chi;
                            dt.Rows[i]["xaphuong"] = collected_staff.xa_phuong;
                            dt.Rows[i]["tothon"] = collected_staff.to_thon;
                        }
                    }
                }
            }
            return dt;
        }
        [Filters.AuthVinaphone()]
        public ActionResult PTTBBSSReportCatHuy(string time1, string time2)
        {
            try
            {
                //
                PTTBBSSDataSource(time1);
                var month1 = TM.OleExcel.ToDataTable("SELECT * FROM [Sheet1$] WHERE magd='DM'");

                //
                PTTBBSSDataSource(time2);
                var month2 = TM.OleExcel.ToDataTable("SELECT * FROM [Sheet1$] WHERE magd='THAOHUY'");

                var dt = month1.Clone();
                for (int i = 0; i < month1.Rows.Count; i++)
                {
                    var diachilap_month1 = month1.Rows[i]["diachilap"].ToString().Split(' ');
                    var ma_cq1 = diachilap_month1.Length < 2 ? "" : diachilap_month1[1];

                    var lm1 = month1.Rows[i]["LM"].ToString().Split(';');
                    if (lm1.Length > 0)
                        lm1 = lm1[0].Split(':');
                    if (lm1.Length > 1 && !string.IsNullOrEmpty(lm1[1]))
                        for (int j = 0; j < month2.Rows.Count; j++)
                        {
                            var diachilap_month2 = month2.Rows[j]["diachilap"].ToString().Split(' ');
                            var ma_cq2 = diachilap_month2.Length < 2 ? "" : diachilap_month2[1];

                            if (ma_cq1 == ma_cq2)
                            {
                                dt.ImportRow(month1.Rows[i]);
                                break;
                            }

                            var lm2 = month2.Rows[j]["LM"].ToString().Split(';');
                            if (lm2.Length > 0)
                                lm2 = lm2[0].Split(':');
                            if (lm2.Length > 1 && !string.IsNullOrEmpty(lm2[1]))
                                if (lm1[1] == lm2[1])
                                {
                                    dt.ImportRow(month1.Rows[i]);
                                    break;
                                }
                        }
                }
                TM.Exports.ExportExcel(dt, "Danh sách tháo hủy " + time1 + " " + time2);
            }
            catch (Exception ex)
            {
                this.danger(ex.Message);
            }
            return RedirectToAction("PTTBBSS");
        }
        public ActionResult Example()
        {
            try
            {
                var month = "0516";
                TM.OleExcel.DataSource = TM.IO.FileDirectory.MapPath("Uploads/Data/PTTB-BSS/CMSN.xlsx");
                TM.OleDBF.DataSource = "Uploads/Data/HDData/" + month + "/";
                //
                var dt = TM.OleExcel.ToDataTable("SELECT * FROM [Sheet1$]");
                //var dt2 = TM.OleDBF.ToDataTable("SELECT * FROM hdall1116.DBF");
                dt.Columns.Add("tong");
                dt.Columns.Add("tongcong");
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    var dt2 = TM.OleDBF.ToDataTable("SELECT * FROM hdall" + month + ".DBF WHERE ma_cq='" + dt.Rows[i][0].ToString() +
                        "' OR acc_net='" + dt.Rows[i][0].ToString() + "' OR acc_tv='" + dt.Rows[i][0].ToString() + "'");
                    //"' OR so_dd='" + dt.Rows[i][0].ToString() + "' OR so_cd='" + dt.Rows[i][0].ToString() + "'");
                    if (dt2 != null && dt2.Rows.Count > 0)
                    {
                        dt.Rows[i]["tong"] = dt2.Rows[0]["tong"].ToString();
                        dt.Rows[i]["tongcong"] = dt2.Rows[0]["tongcong"].ToString();
                    }
                    //for (int j = 0; j < dt2.Rows.Count; j++)
                    //{
                    //    if (dt.Rows[i][0].ToString() == dt2.Rows[j][1].ToString() ||
                    //        dt.Rows[i][0].ToString() == dt2.Rows[j][2].ToString() ||
                    //        dt.Rows[i][0].ToString() == dt2.Rows[j][3].ToString() ||
                    //        dt.Rows[i][0].ToString() == dt2.Rows[j][4].ToString() ||
                    //        dt.Rows[i][0].ToString() == dt2.Rows[j][5].ToString())
                    //    {
                    //        dt.Rows[i]["tong"] = dt2.Rows[j]["tong"].ToString();
                    //        dt.Rows[i]["tongcong"] = dt2.Rows[j]["tongcong"].ToString();
                    //    }
                    //}
                }
                TM.Exports.ExportExcel(dt, "CMSN");
            }
            catch (Exception ex)
            {
                this.danger(ex.Message);
            }
            return RedirectToAction("PTTBBSS");
        }
        public ActionResult ReportPack()
        {
            try
            {
                TM.OleExcel.DataSource = TM.IO.FileDirectory.MapPath("Uploads/Data/PTTB-BSS/pttb0117.xls");
                var dtpttb = TM.OleExcel.ToDataTable("SELECT * FROM [Sheet1$]");
                dtpttb.Columns.Add("account");
                dtpttb.Columns.Add("THDV");
                for (int i = 0; i < dtpttb.Rows.Count; i++)
                {
                    TM.OleExcel.DataSource = TM.IO.FileDirectory.MapPath("Uploads/Data/PTTB-BSS/dbin0117.xls");
                    var dtdbin = TM.OleExcel.ToDataTable("SELECT * FROM [Sheet1$] WHERE SODAIDIEN='" + dtpttb.Rows[i]["mathuebao"].ToString() + "'");
                    if (dtdbin != null && dtdbin.Rows.Count > 0)
                    {
                        dtpttb.Rows[i]["account"] = dtdbin.Rows[0]["ACCOUNT"];
                    }
                    TM.SQL.DBStatic.ConstantConnectionString = "THDV";
                    var dtthdv = TM.SQL.DBStatic.ToDataTable(@"select c.*,p.name as thdv,cs.app_key from customers c 
                                                        inner join packets p on c.packet_id = p.id
                                                        inner join customer_services cs on c.id = cs.customer_id
                                                        where app_key='" + dtpttb.Rows[i]["account"].ToString() + "'");
                    if (dtthdv != null && dtthdv.Rows.Count > 0)
                    {
                        dtpttb.Rows[i]["THDV"] = dtthdv.Rows[0]["thdv"];
                    }
                    dtthdv = TM.SQL.DBStatic.ToDataTable(@"select c.*,p.name as thdv,cs.app_key from customers c 
                                                        inner join packets p on c.packet_id = p.id
                                                        inner join customer_services cs on c.id = cs.customer_id
                                                        where app_key='" + dtpttb.Rows[i]["mathuebao"].ToString() + "'");
                    if (dtthdv != null && dtthdv.Rows.Count > 0)
                    {
                        dtpttb.Rows[i]["THDV"] = dtthdv.Rows[0]["thdv"];
                    }
                }
                TM.Exports.ExportExcel(dtpttb, "ReportPack-pttb0117");
            }
            catch (Exception ex)
            {
                this.danger(ex.Message);
            }
            return RedirectToAction("PTTBBSS");
        }
    }
}