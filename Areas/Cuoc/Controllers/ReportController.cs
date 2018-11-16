using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using TM.Message;
using PagedList;
using Dapper;
using Dapper.Contrib.Extensions;
using System.Data;
using System.Reflection;

namespace Portal.Areas.Cuoc.Controllers
{
    [Filters.AuthVinaphone()]
    public class ReportController : Portal.Controllers.BaseController
    {
        TM.Connection.SQLServer SQLServer;
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public async Task<ActionResult> Select(objBST obj)//string sort, string order, string search, int offset = 0, int limit = 10, int flag = 1
        {
            var index = 0;
            var qry = "";
            var cdt = "";
            try
            {
                SQLServer = new TM.Connection.SQLServer("SQLTTKDServerCuoc");
                //
                qry = "SELECT * FROM DB_THANHTOAN_BKN WHERE MA_TT_HNI IS NOT NULL";
                if (Authentication.Auth.AuthUser.roles == Authentication.Roles.staff)
                {
                    var staff_id = Authentication.Auth.AuthUser.staff_id;
                    var local = db.groups.FirstOrDefault(m => m.id == staff_id);
                    cdt = $"MA_DVI='{local.level}' AND ";
                }
                //
                //if (!string.IsNullOrEmpty(obj.maDvi))
                //    cdt = $"MA_DVI='{obj.maDvi}' AND "; //TYPE_BILL=9005 AND FORMAT(TIME_BILL,'MM/yyyy')='12/2017'

                //Get data for Search
                if (!string.IsNullOrEmpty(obj.search))
                    cdt = $"(MA_KH LIKE '%{obj.search}%' OR MA_TT_HNI LIKE '%{obj.search}%' OR ACCOUNT LIKE '%{obj.search}%' OR MA_TB LIKE '%{obj.search}%' OR TEN_TT LIKE '%{obj.search}%' OR DIACHI_TT LIKE '%{obj.search}%') AND ";
                if (!string.IsNullOrEmpty(cdt))
                    qry += $" AND {cdt.Substring(0, cdt.Length - 5)}";

                var data = SQLServer.Connection.Query<Billing.Models.DB_THANHTOAN_BKN>(qry);//.Where(m => m.Flag == flag);SQLServer.Connection.Query(qry);

                ////Get data for Search
                //if (!string.IsNullOrEmpty(obj.search))
                //    data = data.Where(d =>
                //    d.MA_KH.Contains(obj.search) ||
                //    d.MA_TT_HNI.Contains(obj.search) ||
                //    d.ACCOUNT.Contains(obj.search) ||
                //    d.MA_TB.Contains(obj.search) ||
                //    d.TEN_TT.Contains(obj.search) ||
                //    d.DIACHI_TT.Contains(obj.search));
                //
                if (data.ToList().Count < 1)
                    return Json(new { total = 0, rows = data }, JsonRequestBehavior.AllowGet);
                //Get total item
                var total = data.Count();
                //Sort And Orders
                if (!string.IsNullOrEmpty(obj.sort))
                {
                    if (obj.sort.ToUpper() == "MA_KH" && obj.order.ToLower() == "asc")
                        data = data.OrderBy(m => m.MA_KH);
                    else if (obj.sort.ToUpper() == "MA_KH" && obj.order.ToLower() == "desc")
                        data = data.OrderByDescending(m => m.MA_KH);
                    else if (obj.sort.ToUpper() == "MA_TT_HNI" && obj.order.ToLower() == "asc")
                        data = data.OrderBy(m => m.MA_TT_HNI);
                    else if (obj.sort.ToUpper() == "MA_TT_HNI" && obj.order.ToLower() == "desc")
                        data = data.OrderByDescending(m => m.MA_TT_HNI);
                    else if (obj.sort.ToUpper() == "ACCOUNT" && obj.order.ToLower() == "asc")
                        data = data.OrderBy(m => m.ACCOUNT);
                    else if (obj.sort.ToUpper() == "ACCOUNT" && obj.order.ToLower() == "desc")
                        data = data.OrderByDescending(m => m.ACCOUNT);
                    else if (obj.sort.ToUpper() == "MA_TB" && obj.order.ToLower() == "asc")
                        data = data.OrderBy(m => m.MA_TB);
                    else if (obj.sort.ToUpper() == "MA_TB" && obj.order.ToLower() == "desc")
                        data = data.OrderByDescending(m => m.MA_TB);
                    else if (obj.sort.ToUpper() == "TEN_TT" && obj.order.ToLower() == "asc")
                        data = data.OrderBy(m => m.TEN_TT);
                    else if (obj.sort.ToUpper() == "TEN_TT" && obj.order.ToLower() == "desc")
                        data = data.OrderByDescending(m => m.TEN_TT);
                    else if (obj.sort.ToUpper() == "DIACHI_TT" && obj.order.ToLower() == "asc")
                        data = data.OrderBy(m => m.DIACHI_TT);
                    else if (obj.sort.ToUpper() == "DIACHI_TT" && obj.order.ToLower() == "desc")
                        data = data.OrderByDescending(m => m.DIACHI_TT);
                    else
                        data = data.OrderBy(m => m.MA_TT_HNI);
                }
                else
                    data = data.OrderBy(m => m.MA_TT_HNI);
                //Page Site
                var rs = data.Skip(obj.offset).Take(obj.limit).ToList();
                return Json(new { total = total, rows = rs }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = "Không tìm thấy dữ liệu, vui lòng thực hiện lại!" }, JsonRequestBehavior.AllowGet); }
            finally { SQLServer.Close(); }
            //return Json(new { success = "Cập nhật thành công!" }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult DetailsBill()
        {
            return View();
        }
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult GetBillDetails(Guid id, string TimeBill)
        {
            var index = 0;
            try
            {
                //var provider = System.Globalization.CultureInfo.InvariantCulture;
                //var TIME_BILL = new
                SQLServer = new TM.Connection.SQLServer("SQLTTKDServerCuoc");
                var qry = $"SELECT * FROM DB_THANHTOAN_BKN WHERE ID='{id}'";
                var accounts = "";
                var dbkh = SQLServer.Connection.QueryFirstOrDefault<Billing.Models.DB_THANHTOAN_BKN>(qry);
                if (dbkh == null)
                    return Json(new { danger = "Không tìm thấy khách hàng!" }, JsonRequestBehavior.AllowGet);
                //HD CD
                qry = $"SELECT b.* FROM DB_THANHTOAN_BKN a,HD_CD b WHERE a.ID=b.DBKH_ID AND b.TYPE_BILL=1 AND a.MA_TT_HNI='{dbkh.MA_TT_HNI}' AND FORMAT(b.TIME_BILL,'MM/yyyy')='{TimeBill}'";
                var hdcd = SQLServer.Connection.Query<Billing.Models.HD_CD>(qry).ToList();
                if (hdcd.Count > 0) foreach (var i in hdcd) accounts += $"'{i.SO_TB}',";
                //HD DD
                qry = $"SELECT b.* FROM DB_THANHTOAN_BKN a,HD_DD b WHERE a.ID=b.DBKH_ID AND b.TYPE_BILL=2 AND a.MA_TT_HNI='{dbkh.MA_TT_HNI}' AND FORMAT(b.TIME_BILL,'MM/yyyy')='{TimeBill}'";
                var hddd = SQLServer.Connection.Query<Billing.Models.HD_DD>(qry).ToList();
                if (hddd.Count > 0) foreach (var i in hddd) accounts += $"'{i.SO_TB}',";
                //HD MYTV
                qry = $"SELECT b.* FROM DB_THANHTOAN_BKN a,HD_MYTV b WHERE a.ID=b.DBKH_ID AND b.TYPE_BILL=8 AND a.MA_TT_HNI='{dbkh.MA_TT_HNI}' AND FORMAT(b.TIME_BILL,'MM/yyyy')='{TimeBill}'";
                var hdtv = SQLServer.Connection.Query<Billing.Models.HD_MYTV>(qry).ToList();
                if (hdtv.Count > 0) foreach (var i in hdtv) accounts += $"'{i.ACCOUNT}',";
                //HD NET
                qry = $"SELECT b.* FROM DB_THANHTOAN_BKN a,HD_NET b WHERE a.ID=b.DBKH_ID AND b.TYPE_BILL=9005 AND a.MA_TT_HNI='{dbkh.MA_TT_HNI}' AND FORMAT(b.TIME_BILL,'MM/yyyy')='{TimeBill}'";
                var hdnet = SQLServer.Connection.Query<Billing.Models.HD_NET>(qry).ToList();
                if (hdnet.Count > 0) foreach (var i in hdnet) accounts += $"'{i.ACCOUNT}',";
                //Discount
                var discount = new List<Billing.Models.DISCOUNT>();
                if (!string.IsNullOrEmpty(accounts))
                {
                    qry = $"SELECT * FROM DISCOUNT WHERE ACCOUNT IN({accounts.Trim(',')}) AND FORMAT(TIME_BILL,'MM/yyyy')='{TimeBill}'";
                    discount = SQLServer.Connection.Query<Billing.Models.DISCOUNT>(qry).ToList();
                }
                //Comment
                SQLServer = new TM.Connection.SQLServer();
                qry = $"SELECT * FROM COMMENT_BILL WHERE MA_TT_HNI='{dbkh.MA_TT_HNI}' AND FORMAT(TIME_BILL,'MM/yyyy')='{TimeBill}' ORDER BY CREATEDAT,LEVELS";
                var cmt = SQLServer.Connection.Query<Billing.Models.COMMENT_BILL>(qry).ToList();
                return Json(new { data = new { dbkh = dbkh, hdcd = hdcd, hddd = hddd, hdtv = hdtv, hdnet = hdnet, discount = discount, cmt = cmt }, success = "Lấy dữ liệu thành công!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
            finally { SQLServer.Close(); }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult PostComment(Guid? Parent, Guid id, string MATT, string TimeBill, string Contents)
        {
            var index = 0;
            try
            {
                SQLServer = new TM.Connection.SQLServer();
                var qry = "";
                var provider = System.Globalization.CultureInfo.InvariantCulture;
                var data = new List<Billing.Models.COMMENT_BILL>();
                var cmt = new Billing.Models.COMMENT_BILL();
                cmt.ID = Guid.NewGuid();
                cmt.TIME_BILL = DateTime.ParseExact($"01/{TimeBill}", "dd/MM/yyyy", provider);
                cmt.MA_TT_HNI = MATT;
                cmt.CONTENTS = Contents;
                cmt.CREATEDBY = Authentication.Auth.AuthUser.username;
                cmt.CREATEDAT = DateTime.Now;
                cmt.FLAG = 1;
                cmt.ROOT_ID = cmt.ID.ToString();
                cmt.PARENTS_ID = $",{cmt.ID.ToString()},";
                cmt.LEVELS = 0;
                var tmp = new Billing.Models.COMMENT_BILL();
                if (Parent != null)
                {
                    qry = $"SELECT * FROM COMMENT_BILL WHERE ID='{Parent.ToString()}'";
                    tmp = SQLServer.Connection.QueryFirstOrDefault<Billing.Models.COMMENT_BILL>(qry);
                    cmt.ROOT_ID = tmp.ROOT_ID;
                    cmt.PARENT_ID = tmp.ID.ToString();
                    cmt.PARENTS_ID = $"{tmp.PARENTS_ID}{cmt.ID.ToString()},";
                    cmt.LEVELS = tmp.LEVELS + 1;
                }
                data.Add(cmt);
                SQLServer.Connection.Insert(cmt);
                //
                var extras = new EXTRAS();
                extras.MA_TT_ID = id.ToString();
                extras.MA_TT = cmt.MA_TT_HNI;
                extras.TIME_BILL = cmt.TIME_BILL;
                extras.URL = "CUOC/Report";
                //
                if (Authentication.Auth.AuthUser.roles == Authentication.Roles.admin || Authentication.Auth.AuthUser.roles == Authentication.Roles.managerBill)
                {
                    if (tmp != null)
                    {
                        var ntfc = new Billing.Models.NOTIFICATION();
                        ntfc.ID = Guid.NewGuid();
                        //ntfc.ROOT_ID = cmt.ID.ToString();
                        //ntfc.PARENT_ID = cmt.MA_TT_HNI;
                        //ntfc.URL = "CUOC/Report";
                        ntfc.SOURCE = ntfc.CREATEDBY = cmt.CREATEDBY;
                        ntfc.DESTINATION = tmp.CREATEDBY;
                        ntfc.TITLE = "Yêu cầu đối xoát cước";
                        ntfc.DESC = $"Mã thanh toán: {cmt.MA_TT_HNI} - Kỳ cước: {TimeBill}";
                        ntfc.CREATEDAT = cmt.CREATEDAT;
                        ntfc.EXTRAS = Newtonsoft.Json.JsonConvert.SerializeObject(extras);
                        ntfc.FLAG = 1;
                        SQLServer.Connection.Insert(ntfc);
                    }
                }
                else
                {
                    qry = $"SELECT * FROM users WHERE flag=1 AND (roles='{Authentication.Roles.managerBill}' OR roles='{Authentication.Roles.admin}')";
                    var user_admin = SQLServer.Connection.Query<Authentication.Users>(qry).ToList();
                    var notification = new List<Billing.Models.NOTIFICATION>();
                    foreach (var i in user_admin)
                    {
                        var ntfc = new Billing.Models.NOTIFICATION();
                        ntfc.ID = Guid.NewGuid();
                        //ntfc.ROOT_ID = cmt.ID.ToString();
                        //ntfc.PARENT_ID = cmt.MA_TT_HNI;
                        //ntfc.URL = "CUOC/Report";
                        ntfc.SOURCE = ntfc.CREATEDBY = cmt.CREATEDBY;
                        ntfc.DESTINATION = i.username;
                        ntfc.TITLE = "Yêu cầu đối xoát cước";
                        ntfc.DESC = $"Mã thanh toán: {cmt.MA_TT_HNI} - Kỳ cước: {TimeBill}";
                        ntfc.CREATEDAT = cmt.CREATEDAT;
                        ntfc.EXTRAS = Newtonsoft.Json.JsonConvert.SerializeObject(extras);
                        ntfc.FLAG = 1;
                        notification.Add(ntfc);
                    }
                    SQLServer.Connection.Insert(notification);
                }
                return Json(new { data = data, success = "Gửi yêu cầu thành công!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
            finally { SQLServer.Close(); }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult Edit(Guid id)
        {
            var index = 0;
            try
            {
                return Json(new { success = "Cập nhật thành công!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
        }
        public static DataTable CreateDataTable<T>(IEnumerable<T> list)
        {
            Type type = typeof(T);
            var properties = type.GetProperties();

            DataTable dataTable = new DataTable();
            foreach (PropertyInfo info in properties)
            {
                dataTable.Columns.Add(new DataColumn(info.Name, Nullable.GetUnderlyingType(info.PropertyType) ?? info.PropertyType));
            }

            foreach (T entity in list)
            {
                object[] values = new object[properties.Length];
                for (int i = 0; i < properties.Length; i++)
                {
                    values[i] = properties[i].GetValue(entity);
                }

                dataTable.Rows.Add(values);
            }

            return dataTable;
        }
    }
    public class objBST : Common.Objects.ObjBSTable
    {
        public string maDvi { get; set; }
        public string timeBill { get; set; }
    }
    public class EXTRAS
    {
        public string MA_TT_ID { get; set; }
        public string MA_TT { get; set; }
        public string URL { get; set; }
        public DateTime TIME_BILL { get; set; }
    }
}