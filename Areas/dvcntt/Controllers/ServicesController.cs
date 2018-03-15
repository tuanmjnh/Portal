using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TM.Message;
using PagedList;
using Portal.Models;

namespace Portal.Areas.dvcntt.Controllers
{
    [Filters.AuthVinaphone()]
    public class ServicesController : Portal.Controllers.BaseController
    {
        // GET: DVCNTT/Services
        public ActionResult Index(string services, int? flag, string order, string currentFilter, string searchString, int? page, string datetime, int? datetimeType, string export)
        {
            try
            {
                if (searchString != null)
                {
                    page = 1;
                    searchString = searchString.Trim();
                }
                else searchString = currentFilter;
                ViewBag.order = order;
                ViewBag.currentFilter = searchString;
                ViewBag.flag = flag;
                ViewBag.services = services;
                ViewBag.datetime = datetime;
                ViewBag.datetimeType = datetimeType;

                var rs = from d in db.groups select d;

                if (!String.IsNullOrEmpty(services))
                    rs = rs.Where(d => d.app_key == services);
                else
                    rs = rs.Where(d => d.app_key == Common.Objects.groups.token);

                if (!String.IsNullOrEmpty(searchString))
                    rs = rs.Where(d =>
                    d.title.Contains(searchString) ||
                    d.app_key.Contains(searchString)).ToList().AsQueryable();

                if (!String.IsNullOrEmpty(datetime))
                {
                    var date = datetime.Split('-');
                    if (date.Length > 1)
                    {
                        var dateStart = TM.Format.Formating.StartOfDate(TM.Format.Formating.DateParseExactVNToEN(date[0]));
                        var dateEnd = TM.Format.Formating.EndOfDate(TM.Format.Formating.DateParseExactVNToEN(date[1]));
                        rs = datetimeType == 0 ? rs.Where(d => d.created_at >= dateStart && d.created_at <= dateEnd) : rs.Where(d => d.updated_at >= dateStart && d.updated_at <= dateEnd);
                    }
                }

                if (flag == 0) rs = rs.Where(d => d.flag == 0);
                else rs = rs.Where(d => d.flag == 1);

                switch (order)
                {
                    case "title_asc":
                        rs = rs.OrderBy(d => d.title);
                        break;
                    case "title_desc":
                        rs = rs.OrderByDescending(d => d.title);
                        break;
                    case "level_desc":
                        rs = rs.OrderByDescending(d => d.level);
                        break;
                    default:
                        rs = rs.OrderBy(d => d.level).ThenBy(d => d.extras).ThenBy(d => d.title);
                        break;
                }

                //Export to any
                if (!String.IsNullOrEmpty(export))
                {
                    TM.Exports.ExportExcel(TM.Helper.Data.ToDataTable(rs.ToList()), "Danh sách dịch vụ thuê kênh riêng");
                    return RedirectToAction("Index");
                }

                ViewBag.TotalRecords = rs.Count();
                int pageSize = 15;
                int pageNumber = (page ?? 1);

                return View(rs.ToPagedList(pageNumber, pageSize));
            }
            catch (Exception ex)
            {
                this.danger(ex.Message);
            }
            return View();
        }

        // GET: DVCNTT/Services/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: DVCNTT/Services/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "app_key,title,level,parent_id,parents_id,desc,flag,extras")] group group)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    group.id = Guid.NewGuid();
                    group.created_by = Authentication.Auth.AuthUser.id.ToString();
                    group.created_at = DateTime.Now;
                    db.groups.Add(group);
                    db.SaveChanges();
                    this.success(TM.Common.Language.msgCreateSucsess);
                    return RedirectToAction("Create");
                }
                else
                    this.danger(TM.Common.Language.msgCreateError);
            }
            catch (Exception ex)
            {
                this.danger(ex.Message);
            }
            return View(group);
        }

        // GET: DVCNTT/Services/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var group = db.groups.Find(id);
            if (group == null)
            {
                return HttpNotFound();
            }
            ViewBag.services = getGroups(Common.Objects.groups.token).OrderBy(d => d.level).ThenBy(d => d.extras).ThenBy(d => d.title).ToList();
            return View(group);
        }

        // POST: DVCNTT/Services/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,app_key,title,level,parent_id,parents_id,desc,flag,extras")] group group_tmp)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var group = db.groups.Find(group_tmp.id);
                    group.title = group_tmp.title;
                    group.level = group_tmp.level;
                    group.parent_id = group_tmp.parent_id;
                    group.parents_id = group_tmp.parents_id;
                    group.desc = group_tmp.desc;
                    group.flag = group_tmp.flag;
                    group.extras = group_tmp.extras;
                    group.updated_by = Authentication.Auth.AuthUser.id.ToString();
                    group.updated_at = DateTime.Now;
                    db.Entry(group).State = EntityState.Modified;
                    db.SaveChanges();
                    this.success(TM.Common.Language.msgUpdateSucsess);
                    return RedirectToAction("Index");
                }
                else
                    this.danger(TM.Common.Language.msgUpdateError);
            }
            catch (Exception ex)
            {
                this.danger(ex.Message);
            }
            return View(group_tmp);
        }

        [HttpGet]
        public JsonResult update_flag(string uid)
        {
            try
            {
                string[] id = uid.Split(',');
                var flag = 0;
                foreach (var item in id)
                {
                    Guid tmp = Guid.Parse(item);
                    var rs = db.groups.Find(tmp);
                    rs.flag = flag = rs.flag == 1 ? 0 : 1;
                }
                db.SaveChanges();
                return Json(new { success = (flag == 0 ? TM.Common.Language.msgDeleteSucsess : TM.Common.Language.msgRecoverSucsess) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception) { return Json(new { danger = TM.Common.Language.msgError }, JsonRequestBehavior.AllowGet); }
        }
    }
}
