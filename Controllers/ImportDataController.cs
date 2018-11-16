using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TM.Message;
using TM.Helper;
using Dapper;

namespace Portal.Controllers
{
    [Filters.AuthVinaphone()]
    public class ImportDataController : BaseController
    {
        static string datasourcehd;
        // GET: ImportData
        public ActionResult Index()
        {
            datasourcehd = "Uploads/Data/HDData";
            ViewBag.directory = TM.IO.FileDirectory.DirectoriesToList(datasourcehd);
            return View();
        }
        //public ActionResult CollectedStaff()
        //{
        //    try
        //    {
        //        //SELECT * FROM CollectedStaff o INNER JOIN (SELECT ma_cb , COUNT(*) AS dupeCount FROM CollectedStaff GROUP BY ma_cb HAVING COUNT(*) > 1) oc ON o.ma_cb=oc.ma_cb ORDER BY o.ma_cb
        //        PortalContextConnectionString();
        //        TM.SQLDB.Execute("DELETE FROM collected_staff");
        //        string sql = "";
        //        TM.OleDBF.DataSource = "Uploads/Data/TemplateData";
        //        var a = TM.OleDBF.ToDataTable("SELECT * FROM CollectedStaff.dbf");
        //        for (int i = 0; i < a.Rows.Count; i++)
        //        {
        //            sql += "INSERT INTO collected_staff VALUES(" +
        //                a.Rows[i][0] +
        //                ",N'" + a.Rows[i][2].TCVN3ToUnicode() +
        //                "',N'" + a.Rows[i][1].TCVN3ToUnicode() +
        //                "','" + Authentication.Auth.AuthUser.id.ToString() +
        //                "',GETDATE(),NULL,NULL,1);\n";

        //        }
        //        TM.SQLDB.Execute(sql);
        //        this.success(TM.Common.Language.msgSucsessImportdata);
        //    }
        //    catch (Exception ex)
        //    {
        //        this.danger(ex.Message);
        //    }
        //    return RedirectToAction("Index");
        //}
        public ActionResult GetNotIn()
        {
            TM.OleDBF.DataSource = "Uploads/Data/TemplateData";
            var a = TM.OleDBF.ToDataTable("SELECT * FROM CollectedStaff.dbf");
            var b = Connection().Query("select id from collected_staff order by id").ToList();
            List<string> list = new List<string>();
            for (int i = 0; i < a.Rows.Count; i++)
            {
                list.Add(a.Rows[i][0].ToString());
            }
            foreach (var item in b)
            {
                if (list.Contains(item.id.ToString()))
                    list.Remove(item.id.ToString());
            }
            ViewBag.data = list;
            ViewBag.count = list.Count;
            return View();
        }
        public ActionResult CollectedStaff()
        {
            string update = "";
            var count_id = "0";
            try
            {
                //SELECT * FROM CollectedStaff o INNER JOIN (SELECT ma_cb , COUNT(*) AS dupeCount FROM CollectedStaff GROUP BY ma_cb HAVING COUNT(*) > 1) oc ON o.ma_cb=oc.ma_cb ORDER BY o.ma_cb
                ConnectionString();
                string sql = "";
                TM.OleDBF.DataSource = "Uploads/Data/TemplateData";
                var a = TM.OleDBF.ToDataTable("SELECT * FROM CollectedStaff.dbf");
                for (int i = 0; i < a.Rows.Count; i++)
                {

                    count_id = a.Rows[i][0].ToString();
                    var nvql = a.Rows[i][2].ToString().TCVN3ToUnicode();
                    var dia_chi = a.Rows[i][1].ToString().TCVN3ToUnicode();
                    sql += "INSERT INTO collected_staff(id,nvql,dia_chi,created_by,created_at,flag) VALUES(" +
                        a.Rows[i][0] +
                        ",N'" + nvql +
                        "',N'" + dia_chi +
                        "','" + Authentication.Auth.AuthUser.id.ToString() +
                        "',GETDATE(),1);\n";

                    update += "UPDATE collected_staff SET nvql=N'" + nvql + "',dia_chi=N'" + dia_chi + "' where id=" + a.Rows[i][0] + ";\n";
                }
                TM.SQL.DBStatic.Execute(sql);
                this.success(TM.Common.Language.msgImportdataSucsess);
            }
            catch (Exception ex)
            {
                this.danger(count_id + ": " + ex.Message);
            }
            TM.SQL.DBStatic.Execute(update);
            return RedirectToAction("Index");
        }
        public ActionResult CollectedStaff2()
        {
            try
            {
                ConnectionString();
                string rs = "";
                foreach (var item in TM.IO.FileDirectory.ReadFile("Uploads/Data/TemplateData/CollectedStaff2.txt", '\t'))
                {
                    if (item.Length > 1)
                    {
                        var collected_staff = db.collected_staff.Find(long.Parse(item[2]));
                        var tmp_key_name = item[0];
                        var local = db.locals.Where(d => d.key_name == tmp_key_name).FirstOrDefault();

                        if (collected_staff != null)
                        {
                            if (local != null)
                                collected_staff.local_id = local.id;
                            else
                                collected_staff.local_id = 0;
                            collected_staff.dia_chi = item[3];
                            db.Entry(collected_staff).State = System.Data.Entity.EntityState.Modified;
                        }

                        else
                        {
                            collected_staff = new Models.collected_staff();
                            if (local != null)
                                collected_staff.local_id = local.id;
                            else
                                collected_staff.local_id = 0;
                            collected_staff.dia_chi = item[3];
                            collected_staff.nvql = "";
                            collected_staff.created_by = Authentication.Auth.AuthUser.id.ToString();
                            collected_staff.created_at = DateTime.Now;
                            collected_staff.flag = 1;
                            db.collected_staff.Add(collected_staff);
                        }
                    }
                    else
                        rs += item[2] + "<br/>";
                }

                db.SaveChanges();

                if (rs.Length > 0)
                    this.danger(rs);
                else
                    this.success(TM.Common.Language.msgImportdataSucsess);
            }
            catch (Exception ex)
            {
                this.danger(ex.Message);
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        public ActionResult HDData(string[] hdfile)
        {
            try
            {
                ConnectionString();
                if (hdfile != null && hdfile.Length > 0)
                {
                    foreach (var item in hdfile)
                    {
                        TM.OleDBF.DataSource = datasourcehd + "/" + item;
                        var file = "hdall" + item;
                        var dt = TM.OleDBF.ToDataTable("SELECT * FROM " + file, file);

                        var created_at = item[0].ToString() + item[1].ToString() + "/01/20" + item[2].ToString() + item[3].ToString();
                        TM.SQL.DBStatic.Execute("DELETE FROM bill_month WHERE created_at='" + created_at + "'", "delete bill_month");
                        TM.SQL.DBStatic.Execute("DELETE FROM customer_info WHERE created_at='" + created_at + "'", "delete customer_info");

                        ImportHDData(dt, created_at);
                    }
                    this.success(TM.Common.Language.msgImportdataSucsess);
                }
            }
            catch (Exception ex)
            {
                this.danger(ex.Message); ;
            }
            return RedirectToAction("Index");
        }
        private static bool ImportHDData(System.Data.DataTable dt, string created_at)
        {
            var customer_info = "";
            var bill_month = "";
            var count = 0;
            foreach (System.Data.DataRow r in dt.Rows)
            {
                count++;
                var customer_id = Guid.NewGuid();
                customer_info += "INSERT INTO customer_info VALUES(" +
                    "'" + customer_id + "'," +
                    "'" + r["ma_cq"].ToString().Trim() + "'," +
                          r["ma_dvi"].ToString().Trim() + "," +
                    "'" + r["acc_net"].ToString().Trim() + "'," +
                    "'" + r["acc_tv"].ToString().Trim() + "'," +
                    "'" + r["so_dd"].ToString().Trim() + "'," +
                    "'" + r["so_cd"].ToString().Trim() + "'," +
                    "N'" + r["ten_tb"].ToString().TCVN3ToUnicode() + "'," +
                    "N'" + r["dia_chi"].ToString().TCVN3ToUnicode() + "'," +
                    "'" + r["ma_tuyen"].ToString().Trim() + "'," +
                    "'" + r["ma_st"].ToString().Trim() + "'," +
                          (r["ma_dt"] == null || string.IsNullOrEmpty(r["ma_dt"].ToString().Trim()) ? "0" : r["ma_dt"].ToString().Trim()) + "," +
                          r["ma_cbt"].ToString().Trim() + "," +
                    "'" + r["kieu"].ToString().Trim() + "'," +
                          r["flag"].ToString().Trim() + "," +
                    "'" + created_at + "');\n";
                bill_month += "INSERT INTO bill_month VALUES(NEWID()," +
                    "'" + customer_id + "'," +
                    "'" + r["ma_cq"].ToString().Trim() + "'," +
                          r["ma_dvi"].ToString().Trim() + "," +
                    "'" + r["ma_tuyen"].ToString().Trim() + "'," +
                          r["ma_cbt"].ToString().Trim() + "," +
                          r["ma_in"].ToString().Trim() + "," +
                          Common.Objects.BillType.hdGhep + "," +
                          r["tong_cd"].ToString().Trim() + "," +
                          r["tong_dd"].ToString().Trim() + "," +
                          r["tong_net"].ToString().Trim() + "," +
                          r["tong_tv"].ToString().Trim() + "," +
                          r["vat"].ToString().Trim() + "," +
                          r["tong"].ToString().Trim() + "," +
                          r["tongcong"].ToString().Trim() + "," +
                          r["flag"].ToString().Trim() + "," +
                    "'" + created_at + "');\n";
                if (count % 500 == 0)
                {
                    TM.SQL.DBStatic.Execute(customer_info, "customer_info");
                    TM.SQL.DBStatic.Execute(bill_month, "bill_month");
                    customer_info = "";
                    bill_month = "";
                }
            }
            TM.SQL.DBStatic.Execute(customer_info, "customer_info");
            TM.SQL.DBStatic.Execute(bill_month, "bill_month");
            return true;
        }
    }
}