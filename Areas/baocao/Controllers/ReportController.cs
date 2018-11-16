using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Portal.Models;
using Dapper;

namespace Portal.Areas.baocao.Controllers
{
    [Filters.AuthVinaphone]
    public class ReportController : Portal.Controllers.BaseController
    {
        // GET: baocao/Report
        public async Task<ActionResult> Index()
        {
            //var rs = await TM.SQL.DBStatic.Connection().QueryAsync("SELECT * FROM item");
            //var item = await db.items.ToListAsync();
            return View();
        }
        [HttpGet]
        public JsonResult getItem()
        {
            try
            {
                if (Authentication.Auth.AuthUser.roles == Authentication.Roles.leader)
                {
                    var listGroup = new List<listGroup>();
                    var groups = db.groups.Where(m => m.flag > 0 && m.app_key == Common.Objects.groups.reportDay).ToList();
                    foreach (var g in groups)
                    {
                        //var tmp = db.items.Where(m => m.flag > 0
                        //&& m.author.Contains("," + g.id.ToString() + ",")
                        //&& m.attach.Contains("," + TM.Common.Auth.staff_id().ToString() + ",")).ToList();
                        //var tmp = TM.SQL.DBStatic.Connection().Query<itemReport>(
                        //    $@"select i.*,
                        //            si.main_key as simain_key,
                        //            si.value as sivalue,
                        //            si.sub_value as sisub_value,
                        //            si.quantity as siquantity,
                        //            si.[desc] as sidesc 
                        //            from item i inner join sub_item si on si.item_id=i.id where i.flag>0 
                        //    and YEAR(si.created_at) = YEAR(GETDATE())
                        //    and MONTH(si.created_at) = MONTH(GETDATE())
                        //    and DAY(si.created_at) = DAY(GETDATE())
                        //    and i.author like '%,{g.id.ToString()},%'
                        //    and i.attach like '%,{TM.Common.Auth.staff_id().ToString()},%'").ToList();
                        var tmp = TM.SQL.DBStatic.Connection().Query<item>($@"select * from item where 
                            app_key='{Common.Objects.groups.reportDay}' and 
                            author like '%,{g.id.ToString()},%' and 
                            attach like '%,{Authentication.Auth.AuthUser.staff_id.ToString()},%' and 
                            flag>0").ToList();
                        var sub_item = TM.SQL.DBStatic.Connection().Query<sub_item>($@"select * from sub_item where 
                            images='{Authentication.Auth.AuthUser.staff_id.ToString()}' and 
						    YEAR(created_at) = YEAR(GETDATE()) and 
                            MONTH(created_at) = MONTH(GETDATE()) and 
                            DAY(created_at) = DAY(GETDATE())").ToList();
                        if (tmp.Count > 0)
                        {
                            var l = new listGroup();
                            l.group = g;
                            l.item = tmp;
                            l.subitem = sub_item;
                            listGroup.Add(l);
                        }
                    }
                    return Json(new { data = listGroup, success = TM.Common.Language.msgSucsess }, JsonRequestBehavior.AllowGet);
                }
                else if (Authentication.Auth.AuthUser.roles == Authentication.Roles.director || Authentication.Auth.AuthUser.roles == Authentication.Roles.admin || Authentication.Auth.AuthUser.roles == Authentication.Roles.superadmin)
                {
                    var listGroup = new List<listGroup>();
                    var groups = db.groups.Where(m => m.flag > 0 && m.app_key == Common.Objects.groups.reportDay).ToList();
                    foreach (var g in groups)
                    {
                        //var tmp = db.items.Where(m => m.flag > 0
                        //&& m.author.Contains("," + g.id.ToString() + ",")).ToList();
                        var tmp = TM.SQL.DBStatic.Connection().Query<item>($@"select * from item where 
                            app_key='{Common.Objects.groups.reportDay}' and 
                            author like '%,{g.id.ToString()},%' and 
                            flag>0").ToList();
                        var sub_item = TM.SQL.DBStatic.Connection().Query<sub_item>($@"select * from sub_item where 
						    YEAR(created_at) = YEAR(GETDATE()) and 
                            MONTH(created_at) = MONTH(GETDATE()) and 
                            DAY(created_at) = DAY(GETDATE())").ToList();
                        if (tmp.Count > 0)
                        {
                            var l = new listGroup();
                            l.group = g;
                            l.item = tmp;
                            l.subitem = sub_item;
                            listGroup.Add(l);
                        }
                    }
                    return Json(new { data = listGroup, success = TM.Common.Language.msgSucsess }, JsonRequestBehavior.AllowGet);
                }
                else
                    return Json(new { success = TM.Common.Language.msgSucsess }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = TM.Common.Language.msgError }, JsonRequestBehavior.AllowGet); }
        }
        [HttpPost, AllowAnonymous, ValidateJsonAntiForgeryToken]
        public async Task<JsonResult> updateItem(List<WorkData> data)
        {
            try
            {
                foreach (var item in data)
                {
                    var staff_id = Authentication.Auth.AuthUser.staff_id.ToString();
                    var now = DateTime.Now;
                    var si = await db.sub_items.Where(m =>
                    m.item_id == item.iid &&
                    m.images == staff_id &&
                    m.created_at.Value.Year == now.Year &&
                    m.created_at.Value.Month == now.Month &&
                    m.created_at.Value.Day == now.Day).SingleOrDefaultAsync();
                    if (si == null)
                    {
                        var sub_item = new sub_item();
                        sub_item.id = Guid.NewGuid();
                        sub_item.app_key = Common.Objects.groups.reportDay;
                        sub_item.id_key = item.gid.ToString();
                        sub_item.item_id = item.iid;
                        sub_item.main_key = item.nvkd;
                        sub_item.value = item.ctv;
                        sub_item.sub_value = item.nvtc;
                        sub_item.quantity = item.kq;
                        sub_item.desc = item.gc;
                        sub_item.orders = 0;
                        sub_item.flag = 1;
                        sub_item.created_at = DateTime.Now;
                        sub_item.created_by = Authentication.Auth.AuthUser.id.ToString();
                        sub_item.images = Authentication.Auth.AuthUser.staff_id.ToString();
                        db.sub_items.Add(sub_item);
                    }
                    else
                    {
                        si.main_key = item.nvkd;
                        si.value = item.ctv;
                        si.sub_value = item.nvtc;
                        si.quantity = item.kq;
                        si.desc = item.gc;
                        si.updated_at = DateTime.Now;
                        si.updated_by = Authentication.Auth.AuthUser.id.ToString();
                    }
                }
                await db.SaveChangesAsync();
                return Json(new { data = data, success = TM.Common.Language.msgUpdateSucsess }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception) { return Json(new { danger = TM.Common.Language.msgError }, JsonRequestBehavior.AllowGet); }
        }
    }
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ValidateJsonAntiForgeryTokenAttribute : FilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }

            var httpContext = filterContext.HttpContext;
            var cookie = httpContext.Request.Cookies[System.Web.Helpers.AntiForgeryConfig.CookieName];
            System.Web.Helpers.AntiForgery.Validate(cookie != null ? cookie.Value : null, httpContext.Request.Headers["__RequestVerificationToken"]);
        }
    }
    public class listGroup
    {
        public group group { get; set; }
        public List<item> item { get; set; }
        public List<sub_item> subitem { get; set; }
    }
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public class itemReport : item
    {
        public string simain_key { get; set; }
        public string sivalue { get; set; }
        public string sisub_value { get; set; }
        public long? siquantity { get; set; }
        public string sidesc { get; set; }
    }
    //[Serializable]
    public class WorkData
    {
        public Guid gid { get; set; }
        public Guid iid { get; set; }
        public string nvkd { get; set; }
        public string ctv { get; set; }
        public string nvtc { get; set; }
        public int kq { get; set; }
        public string gc { get; set; }
    }
    public class Thing
    {
        public Guid Id { get; set; }
        public string Color { get; set; }
    }
}
