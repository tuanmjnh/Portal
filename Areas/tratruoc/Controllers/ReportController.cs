using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TM.Message;
using TM.Format;
using System.Data;

namespace Portal.Areas.tratruoc.Controllers
{
    public class ReportController : Portal.Controllers.BaseController
    {
        // GET: tratruoc/ReportSMSS
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Upload(int month)
        {
            try
            {
                if (Request.Files.Count < 1 || (Request.Files.Count > 0 && Request.Files[0].ContentLength < 1))
                {
                    this.warning("Vui lòng Chọn tệp!");
                    return RedirectToAction("Create");
                }
                var file = TM.IO.FileDirectory.Upload(Request.Files, TM.Common.Directories.TraSauReport, false, new[] { ".xls", ".xlsx", ".csv" }, 5);
                System.Data.DataTable dt;
                foreach (var item in file.UploadFile())
                {
                    var excel = new TM.Interop.Excel(TM.IO.FileDirectory.MapPath(TM.Common.Directories.TraSauReport + item));
                    var list = excel.ToList();
                    //var table = "danhsach_thuebao_ptm";
                    //TM.OleExcel.DataSource =TM.IO.FileDirectory.MapPath(TM.Common.Directories.TraSauReport + item);
                    //var vs = TM.OleExcel.ToDataTable("SELECT distinct dai_ly FROM " + table);
                    dt = new System.Data.DataTable();
                    dt.Columns.Add("thue_bao");
                    dt.Columns.Add("so_sim");
                    dt.Columns.Add("dai_ly");
                    dt.Columns.Add("ngaythang");
                    dt.Columns.Add("TEN_CUA_HANG");
                    dt.Columns.Add("HoTen");
                    dt.Columns.Add("ten_ctv");
                    dt.Columns.Add("ten_dai_ly");
                    dt.Columns.Add("HoTen_ban");
                    dt.Columns.Add("TEN_CUA_HANG_BAN");
                    dt.Columns.Add("the10kden20k");
                    dt.Columns.Add("the20kden30k");
                    dt.Columns.Add("the30kden40k");
                    dt.Columns.Add("the40kden50k");
                    dt.Columns.Add("the50kden60k");
                    dt.Columns.Add("the60kden70k");
                    dt.Columns.Add("the70kden80k");
                    dt.Columns.Add("the80kden100k");
                    dt.Columns.Add("thetu100ktrolen");
                    dt.Columns.Add("total");
                    for (var i = 1; i < list.Count; i++)
                    {
                        //list[i][month + 3], list[i][month + 4]
                        var row = dt.AsEnumerable().FirstOrDefault(d => d.Field<string>("dai_ly").Trim() == list[i][2].ToString().Trim());
                        if (row == null)
                        {
                            var val = Int32.Parse(list[i][(month * 2) + 3].ToString().Trim());
                            if (val >= 10000 && val < 20000)
                                dt.Rows.Add(list[i][0], list[i][1], list[i][2], list[i][3], list[i][29], list[i][30], list[i][31], list[i][32], list[i][33], list[i][34],
                                "1", "0", "0", "0", "0", "0", "0", "0", "0", (val / 2).ToString());
                            else if (val >= 20000 && val < 30000)
                                dt.Rows.Add(list[i][0], list[i][1], list[i][2], list[i][3], list[i][29], list[i][30], list[i][31], list[i][32], list[i][33], list[i][34],
                                "0", "1", "0", "0", "0", "0", "0", "0", "0", (val / 2).ToString());
                            else if (val >= 30000 && val < 40000)
                                dt.Rows.Add(list[i][0], list[i][1], list[i][2], list[i][3], list[i][29], list[i][30], list[i][31], list[i][32], list[i][33], list[i][34],
                                "0", "0", "1", "0", "0", "0", "0", "0", "0", (val / 2).ToString());
                            else if (val >= 40000 && val < 50000)
                                dt.Rows.Add(list[i][0], list[i][1], list[i][2], list[i][3], list[i][29], list[i][30], list[i][31], list[i][32], list[i][33], list[i][34],
                                "0", "0", "0", "1", "0", "0", "0", "0", "0", (val / 2).ToString());
                            else if (val >= 50000 && val < 60000)
                                dt.Rows.Add(list[i][0], list[i][1], list[i][2], list[i][3], list[i][29], list[i][30], list[i][31], list[i][32], list[i][33], list[i][34],
                                "0", "0", "0", "0", "1", "0", "0", "0", "0", (val / 2).ToString());
                            else if (val >= 60000 && val < 70000)
                                dt.Rows.Add(list[i][0], list[i][1], list[i][2], list[i][3], list[i][29], list[i][30], list[i][31], list[i][32], list[i][33], list[i][34],
                                "0", "0", "0", "0", "0", "1", "0", "0", "0", (val / 2).ToString());
                            else if (val >= 70000 && val < 80000)
                                dt.Rows.Add(list[i][0], list[i][1], list[i][2], list[i][3], list[i][29], list[i][30], list[i][31], list[i][32], list[i][33], list[i][34],
                                "0", "0", "0", "0", "0", "0", "1", "0", "0", (val / 2).ToString());
                            else if (val >= 80000 && val < 100000)
                                dt.Rows.Add(list[i][0], list[i][1], list[i][2], list[i][3], list[i][29], list[i][30], list[i][31], list[i][32], list[i][33], list[i][34],
                                "0", "0", "0", "0", "0", "0", "0", "1", "0", (val / 2).ToString());
                            else if (val >= 100000)
                                dt.Rows.Add(list[i][0], list[i][1], list[i][2], list[i][3], list[i][29], list[i][30], list[i][31], list[i][32], list[i][33], list[i][34],
                                "0", "0", "0", "0", "0", "0", "0", "0", "1", "50000");
                        }
                        else
                        {
                            var val = Int32.Parse(list[i][(month * 2) + 3].ToString().Trim());
                            var tt = val / 2;
                            if (val >= 10000 && val < 20000)
                            {
                                row["the10kden20k"] = Int32.Parse(row["the10kden20k"].ToString()) + 1;
                                row["total"] = Int32.Parse(row["total"].ToString()) + tt;
                            }
                            else if (val >= 20000 && val < 30000)
                            {
                                row["the20kden30k"] = Int32.Parse(row["the20kden30k"].ToString()) + 1;
                                row["total"] = Int32.Parse(row["total"].ToString()) + tt;
                            }
                            else if (val >= 30000 && val < 40000)
                            {
                                row["the30kden40k"] = Int32.Parse(row["the30kden40k"].ToString()) + 1;
                                row["total"] = Int32.Parse(row["total"].ToString()) + tt;
                            }
                            else if (val >= 40000 && val < 50000)
                            {
                                row["the40kden50k"] = Int32.Parse(row["the40kden50k"].ToString()) + 1;
                                row["total"] = Int32.Parse(row["total"].ToString()) + tt;
                            }
                            else if (val >= 50000 && val < 60000)
                            {
                                row["the50kden60k"] = Int32.Parse(row["the50kden60k"].ToString()) + 1;
                                row["total"] = Int32.Parse(row["total"].ToString()) + tt;
                            }
                            else if (val >= 60000 && val < 70000)
                            {
                                row["the60kden70k"] = Int32.Parse(row["the60kden70k"].ToString()) + 1;
                                row["total"] = Int32.Parse(row["total"].ToString()) + tt;
                            }
                            else if (val >= 70000 && val < 80000)
                            {
                                row["the70kden80k"] = Int32.Parse(row["the70kden80k"].ToString()) + 1;
                                row["total"] = Int32.Parse(row["total"].ToString()) + tt;
                            }
                            else if (val >= 80000 && val < 100000)
                            {
                                row["the80kden100k"] = Int32.Parse(row["the80kden100k"].ToString()) + 1;
                                row["total"] = Int32.Parse(row["total"].ToString()) + tt;
                            }
                            else if (val >= 100000)
                            {
                                row["thetu100ktrolen"] = Int32.Parse(row["thetu100ktrolen"].ToString()) + 1;
                                row["total"] = Int32.Parse(row["total"].ToString()) + 50000;
                            }
                        }
                    }
                    var the10kden20k = dt.AsEnumerable().Sum(d => Int32.Parse(d.Field<string>("the10kden20k")));
                    var the20kden30k = dt.AsEnumerable().Sum(d => Int32.Parse(d.Field<string>("the20kden30k")));
                    var the30kden40k = dt.AsEnumerable().Sum(d => Int32.Parse(d.Field<string>("the30kden40k")));
                    var the40kden50k = dt.AsEnumerable().Sum(d => Int32.Parse(d.Field<string>("the40kden50k")));
                    var the50kden60k = dt.AsEnumerable().Sum(d => Int32.Parse(d.Field<string>("the50kden60k")));
                    var the60kden70k = dt.AsEnumerable().Sum(d => Int32.Parse(d.Field<string>("the60kden70k")));
                    var the70kden80k = dt.AsEnumerable().Sum(d => Int32.Parse(d.Field<string>("the70kden80k")));
                    var the80kden100k = dt.AsEnumerable().Sum(d => Int32.Parse(d.Field<string>("the80kden100k")));
                    var thetu100ktrolen = dt.AsEnumerable().Sum(d => Int32.Parse(d.Field<string>("thetu100ktrolen")));
                    dt.Rows.Add("Tổng số", "", "", "", "", "", "", "", "", "",
                                 the10kden20k,
                                 the20kden30k,
                                 the30kden40k,
                                 the40kden50k,
                                 the50kden60k,
                                 the60kden70k,
                                 the70kden80k,
                                 the80kden100k,
                                 thetu100ktrolen);
                    the10kden20k = the10kden20k * (10000 / 2);
                    the20kden30k = the20kden30k * (20000 / 2);
                    the30kden40k = the30kden40k * (30000 / 2);
                    the40kden50k = the40kden50k * (40000 / 2);
                    the50kden60k = the50kden60k * (50000 / 2);
                    the60kden70k = the60kden70k * (60000 / 2);
                    the70kden80k = the70kden80k * (70000 / 2);
                    the80kden100k = the80kden100k * (80000 / 2);
                    thetu100ktrolen = thetu100ktrolen * 50000;
                    dt.Rows.Add("Tổng số * 50% giá trị thẻ", "", "", "", "", "", "", "", "", "",
                                 the10kden20k,
                                 the20kden30k,
                                 the30kden40k,
                                 the40kden50k,
                                 the50kden60k,
                                 the60kden70k,
                                 the70kden80k,
                                 the80kden100k,
                                 thetu100ktrolen);
                    var total = the10kden20k + the20kden30k + the30kden40k + the40kden50k + the50kden60k + the60kden70k + the70kden80k + the80kden100k+ thetu100ktrolen;
                    dt.Rows.Add("Tổng ", "", "", "", "", "", "", "", "", "",
                                  "",
                                  "",
                                  "",
                                  "",
                                  "",
                                  "",
                                  "",
                                  total);
                    var thue = total * 0.1;
                    dt.Rows.Add("Tổng VAT", "", "", "", "", "", "", "", "", "",
                                  "",
                                  "",
                                  "",
                                  "",
                                  "",
                                  "",
                                  "",
                                  thue);
                    dt.Rows.Add("Tổng Cộng", "", "", "", "", "", "", "", "", "",
                                 "",
                                 "",
                                 "",
                                 "",
                                 "",
                                 "",
                                 "",
                                 thue + total);
                    TM.Exports.ExportExcel(dt, "danhsach_thuebao_ptm_nhanvien_" + month.ToString());
                }
                //db.SaveChanges();
                this.success("Cập nhật thành công!");
            }
            catch (Exception ex)
            {
                this.danger(ex.Message);
            }
            return RedirectToAction("Index");

            //try
            //{
            //    //var list = TM.Excel.ToList(TM.IO.FileDirectory.MapPath(TM.Common.Directories.ccbs + "11.xls"));
            //    var excel = new TM.Interop.Excel(TM.IO.FileDirectory.MapPath(TM.Common.Directories.ccbs + "11.xls"));
            //    var list = excel.ToList();
            //}
            //catch (Exception ex)
            //{
            //    this.danger(ex.Message);
            //}
            //return RedirectToAction("Create");
        }
    }
}