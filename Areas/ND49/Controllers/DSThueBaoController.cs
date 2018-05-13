using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dapper;
using Dapper.Contrib.Extensions;

namespace Portal.Areas.ND49.Controllers
{
    [Filters.AuthVinaphone()]
    public class DSThueBaoController : Portal.Controllers.BaseController
    {
        TM.Connection.SQLServer SQLServer;
        // GET: ND49/DSThueBao
        public ActionResult Index()
        {
            try
            {
                //Response.Write(new System.Web.Configuration.ScriptingJsonSerializationSection().MaxJsonLength);
                SQLServer = new TM.Connection.SQLServer();
                var qry = $"SELECT * FROM [group] WHERE app_key='{Common.Objects.groups.department}' and extras like '%,nd49,%' order by orders";
                var group = SQLServer.Connection.Query<Portal.Models.group>(qry);
                ViewBag.group = group;
                var localID = group.FirstOrDefault(d => d.id == Authentication.Auth.AuthUser.staff_id);
                if (localID != null)
                    ViewBag.localID = localID.level;
            }
            catch (Exception) { }
            return View();
        }
        public ActionResult Import()
        {
            return View();
        }
        public ActionResult ExportData()
        {
            return View();
        }
        [HttpGet]
        public ActionResult Select(objBST obj)//string sort, string order, string search, int offset = 0, int limit = 10, int flag = 1
        {
            var provider = System.Globalization.CultureInfo.InvariantCulture;
            var index = 0;
            var qry = "";
            var cdt = "";
            try
            {
                SQLServer = new TM.Connection.SQLServer();
                //
                qry = $"SELECT tb.*,g.title AS TEN_DVI FROM ND49 tb,[group] g WHERE tb.MA_DVI=g.level AND g.app_key='{Common.Objects.groups.department}'";
                if (Authentication.Auth.AuthUser.roles == Authentication.Roles.staff || Authentication.Auth.AuthUser.roles == Authentication.Roles.leader)
                {
                    var staff_id = Authentication.Auth.AuthUser.staff_id;
                    var local = db.groups.FirstOrDefault(m => m.id == staff_id);
                    cdt += $"MA_DVI='{local.level}' AND ";
                }
                else
                {
                    if (obj.maDvi != "0")
                        cdt += $"MA_DVI='{obj.maDvi}' AND ";
                }
                //
                //if (!string.IsNullOrEmpty(obj.maDvi))
                //    cdt = $"MA_DVI='{obj.maDvi}' AND "; //TYPE_BILL=9005 AND FORMAT(TIME_BILL,'MM/yyyy')='12/2017'

                //Get data for Search
                if (!string.IsNullOrEmpty(obj.search))
                    cdt += $"(tb.STB LIKE '%{obj.search}%' OR tb.BTS LIKE '%{obj.search}%' OR g.title LIKE '%{obj.search}%' OR tb.NVQL LIKE '%{obj.search}%') AND ";
                if (!string.IsNullOrEmpty(cdt))
                    qry += $" AND {cdt.Substring(0, cdt.Length - 5)}";

                //export
                if (obj.export == 2)
                {
                    var startDate = DateTime.ParseExact($"{obj.startDate}", "dd/MM/yyyy HH:mm", provider);
                    var endDate = DateTime.ParseExact($"{obj.endDate}", "dd/MM/yyyy HH:mm", provider);
                    qry += $" AND tb.FLAG=2 AND tb.UPDATEDAT>=CAST('{startDate.ToString("yyyy-MM-dd")}' as datetime) AND tb.UPDATEDAT<=CAST('{endDate.ToString("yyyy-MM-dd")}' as datetime) ORDER BY tb.MA_DVI,tb.UPDATEDAT";
                    var export = SQLServer.Connection.Query<Portal.Areas.ND49.Models.ND49Export>(qry);
                    qry = "SELECT * FROM users";
                    var user = SQLServer.Connection.Query<Authentication.user>(qry);
                    foreach (var i in export)
                    {
                        var tmp = user.FirstOrDefault(d => d.username == i.NVQL);
                        i.TEN_NVQL = tmp != null ? tmp.full_name : null;
                    }
                    var rsJson = Json(new { data = export, SHA = Guid.NewGuid() }, JsonRequestBehavior.AllowGet);
                    rsJson.MaxJsonLength = int.MaxValue;
                    return rsJson;
                }
                if (obj.export == 1)
                {
                    qry += $" AND tb.FLAG=1 ORDER BY tb.MA_DVI,tb.UPDATEDAT";
                    var export = SQLServer.Connection.Query<Portal.Areas.ND49.Models.ND49Export>(qry);
                    qry = "SELECT * FROM users";
                    var user = SQLServer.Connection.Query<Authentication.user>(qry);
                    foreach (var i in export)
                    {
                        var tmp = user.FirstOrDefault(d => d.username == i.NVQL);
                        i.TEN_NVQL = tmp != null ? tmp.full_name : null;
                    }
                    var rsJson = Json(new { data = export, SHA = Guid.NewGuid() }, JsonRequestBehavior.AllowGet);
                    rsJson.MaxJsonLength = int.MaxValue;
                    return rsJson;
                }
                if (obj.export == -1)
                {
                    qry += $" ORDER BY tb.MA_DVI,tb.UPDATEDAT";
                    var export = SQLServer.Connection.Query<Portal.Areas.ND49.Models.ND49Export>(qry);
                    qry = "SELECT * FROM users";
                    var user = SQLServer.Connection.Query<Authentication.user>(qry);
                    foreach (var i in export)
                    {
                        var tmp = user.FirstOrDefault(d => d.username == i.NVQL);
                        i.TEN_NVQL = tmp != null ? tmp.full_name : null;
                    }
                    var rsJson = Json(new { data = export, SHA = Guid.NewGuid() }, JsonRequestBehavior.AllowGet);
                    rsJson.MaxJsonLength = int.MaxValue;
                    return rsJson;
                }
                //FLAG
                if (obj.flag != -1)
                {
                    qry += $" AND tb.FLAG={obj.flag}";
                }
                //
                var data = SQLServer.Connection.Query<Portal.Areas.ND49.Models.ND49>(qry);//.Where(m => m.Flag == flag);SQLServer.Connection.Query(qry);

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
                    if (obj.sort.ToUpper() == "STB" && obj.order.ToLower() == "asc")
                        data = data.OrderBy(m => m.STB);
                    else if (obj.sort.ToUpper() == "STB" && obj.order.ToLower() == "desc")
                        data = data.OrderByDescending(m => m.STB);
                    else if (obj.sort.ToUpper() == "SO_ANH" && obj.order.ToLower() == "asc")
                        data = data.OrderBy(m => m.SO_ANH);
                    else if (obj.sort.ToUpper() == "SO_ANH" && obj.order.ToLower() == "desc")
                        data = data.OrderByDescending(m => m.SO_ANH);
                    else if (obj.sort.ToUpper() == "TD_TKC" && obj.order.ToLower() == "asc")
                        data = data.OrderBy(m => m.TD_TKC);
                    else if (obj.sort.ToUpper() == "TD_TKC" && obj.order.ToLower() == "desc")
                        data = data.OrderByDescending(m => m.TD_TKC);
                    else if (obj.sort.ToUpper() == "TB_TD3THANG" && obj.order.ToLower() == "asc")
                        data = data.OrderBy(m => m.TB_TD3THANG);
                    else if (obj.sort.ToUpper() == "TB_TD3THANG" && obj.order.ToLower() == "desc")
                        data = data.OrderByDescending(m => m.TB_TD3THANG);
                    else if (obj.sort.ToUpper() == "BTS" && obj.order.ToLower() == "asc")
                        data = data.OrderBy(m => m.BTS);
                    else if (obj.sort.ToUpper() == "BTS" && obj.order.ToLower() == "desc")
                        data = data.OrderByDescending(m => m.BTS);
                    else if (obj.sort.ToUpper() == "MA_DVI" && obj.order.ToLower() == "asc")
                        data = data.OrderBy(m => m.MA_DVI);
                    else if (obj.sort.ToUpper() == "MA_DVI" && obj.order.ToLower() == "desc")
                        data = data.OrderByDescending(m => m.MA_DVI);
                    else
                        data = data.OrderBy(m => m.MA_DVI).ThenByDescending(m => m.TB_TD3THANG);
                }
                else
                    data = data.OrderBy(m => m.MA_DVI).ThenByDescending(m => m.TB_TD3THANG);
                //Page Site
                var rs = data.Skip(obj.offset).Take(obj.limit).ToList();
                var ReturnJson = Json(new { total = total, rows = rs }, JsonRequestBehavior.AllowGet);
                ReturnJson.MaxJsonLength = int.MaxValue;
                return ReturnJson;
            }
            catch (Exception ex) { return Json(new { danger = "Không tìm thấy dữ liệu, vui lòng thực hiện lại!" }, JsonRequestBehavior.AllowGet); }
            finally { SQLServer.Close(); }
            //return Json(new { success = "Cập nhật thành công!" }, JsonRequestBehavior.AllowGet);
        }
        //Xử lý nhập Text Data
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult XuLyNhapTextData(int dataId, string txtDataVal)
        {
            var SQLServer = new TM.Connection.SQLServer();
            long index = 0;
            var provider = System.Globalization.CultureInfo.InvariantCulture;
            var msg = "Cập nhật thành công";
            try
            {
                //
                if (string.IsNullOrEmpty(txtDataVal))
                    return Json(new { danger = "Vui lòng nhập giá trị!" }, JsonRequestBehavior.AllowGet);
                var qry = "SELECT * FROM ND49";
                var dataOld = SQLServer.Connection.Query<Models.ND49>(qry);
                var dataRow = txtDataVal.Split('\n');
                var dataInsert = new List<Models.ND49>();
                var dataUpdate = new List<Models.ND49>();
                index = 0;
                foreach (var i in dataRow)
                {
                    index++;
                    var tmp = i.Trim('\r').Split('\t');
                    if (index == 1) continue;
                    var stb = tmp[0];
                    var _tmp = dataOld.FirstOrDefault(d => d.STB == stb);
                    //
                    if (tmp.Length > 6)
                    {
                        if (_tmp == null)
                        {
                            var _data = new Models.ND49();
                            _data.ID = Guid.NewGuid();
                            _data.STB = stb;
                            _data.SO_ANH = string.IsNullOrEmpty(tmp[1]) ? 0 : int.Parse(tmp[1]);
                            _data.TD_TKC = string.IsNullOrEmpty(tmp[2]) ? 0 : int.Parse(tmp[2]);
                            _data.TB_TD3THANG = string.IsNullOrEmpty(tmp[3]) ? 0 : int.Parse(tmp[3]);
                            _data.BTS = string.IsNullOrEmpty(tmp[4]) ? null : tmp[4].Trim();
                            _data.MA_DVI = string.IsNullOrEmpty(tmp[5]) ? 0 : int.Parse(tmp[5]);
                            //_data.TEN_DVI = "";// string.IsNullOrEmpty(tmp[6]) ? null : tmp[6].Trim();
                            _data.DVIQL = string.IsNullOrEmpty(tmp[6]) ? null : tmp[6].Trim();
                            _data.NVQL = string.IsNullOrEmpty(tmp[7]) ? null : tmp[7].Trim();
                            //_data.TEN_NVQL = "";
                            _data.FLAG = 1;
                            _data.CREATEDBY = Authentication.Auth.AuthUser.username;
                            _data.CREATEDAT = DateTime.Now;
                            dataInsert.Add(_data);
                        }
                        else
                        {
                            _tmp.MA_DVI = int.Parse(tmp[5]);
                            dataUpdate.Add(_tmp);
                        }
                    }
                }
                if (dataId == 2)
                {
                    qry = $"DELETE ND49";
                    SQLServer.Connection.Query(qry);
                }
                //
                if (dataInsert.Count > 0) SQLServer.Connection.Insert(dataInsert);
                if (dataUpdate.Count > 0) SQLServer.Connection.Update(dataUpdate);
                msg += " ND49";
                return Json(new { success = msg }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
            finally { SQLServer.Close(); }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult AcceptData(Guid dataId)
        {
            var SQLServer = new TM.Connection.SQLServer();
            var qry = "";
            long index = 0;
            try
            {
                qry = $"SELECT * FROM ND49 WHERE ID='{dataId}'";
                var nd49 = SQLServer.Connection.QueryFirstOrDefault<Models.ND49>(qry);
                nd49.UPDATEDBY = Authentication.Auth.AuthUser.username;
                nd49.UPDATEDAT = DateTime.Now;
                nd49.FLAG = 2;
                SQLServer.Connection.Update(nd49);
                return Json(new { success = $"Xác nhận đã cập nhật thuê bao {nd49.STB} theo ND49 thành công!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
            finally { SQLServer.Close(); }
        }
    }
    public class objBST : Common.Objects.ObjBSTable
    {
        public string maDvi { get; set; }
        public string timeBill { get; set; }
        public int export { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
    }
}