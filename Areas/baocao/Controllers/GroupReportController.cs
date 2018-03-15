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
using TM.Message;
using PagedList;


namespace Portal.Areas.baocao.Controllers
{
    [Filters.AuthVinaphone(Role = Authentication.Roles.superadmin + "," + Authentication.Roles.admin)]
    public class GroupReportController : Portal.Controllers.BaseController
    {
        // GET: baocao/GroupReport
        public async Task<ActionResult> Index(string services, int? flag, string order, string currentFilter, string searchString, int? page, string datetime, int? datetimeType, string export)
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

                var rs = db.groups.AsQueryable();

                if (!String.IsNullOrEmpty(services))
                    rs = rs.Where(d => d.app_key == services);
                else
                    rs = rs.Where(d => d.app_key == Common.Objects.groups.reportDay);

                if (!String.IsNullOrEmpty(searchString))
                    rs = rs.Where(d =>
                    d.title.Contains(searchString) ||
                    d.app_key.Contains(searchString));

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
                        rs = rs.OrderBy(d => d.orders);
                        break;
                }

                //Export to any
                if (!String.IsNullOrEmpty(export))
                {
                    TM.Exports.ExportExcel(TM.Helper.Data.ToDataTable(await rs.ToListAsync()), "ReportDay_" + Guid.NewGuid().ToString());
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

        // GET: baocao/GroupReport/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: baocao/GroupReport/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "id_key,title,level,orders,desc,flag")] group group)
        {
            try
            {
                group.id = Guid.NewGuid();
                group.app_key = Common.Objects.groups.reportDay;
                group.created_by = Authentication.Auth.AuthUser.id.ToString();
                group.created_at = DateTime.Now;
                group.updated_by = Authentication.Auth.AuthUser.id.ToString();
                group.updated_at = DateTime.Now;
                db.groups.Add(group);
                await db.SaveChangesAsync();
                this.success(TM.Common.Language.msgCreateSucsess);
                return RedirectToAction("Create");
            }
            catch (Exception)
            {
                this.danger(TM.Common.Language.msgCreateError);
            }
            return View(group);
        }

        // GET: baocao/GroupReport/Edit/5
        public async Task<ActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            group group = await db.groups.FindAsync(id);
            if (group == null)
            {
                return HttpNotFound();
            }
            return View(group);
        }

        // POST: baocao/GroupReport/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "id,id_key,title,level,orders,desc,flag")] group group)
        {
            try
            {
                db.groups.Attach(group);
                var entry = db.Entry(group);
                entry.Property(m => m.title).IsModified = true;
                entry.Property(m => m.level).IsModified = true;
                entry.Property(m => m.orders).IsModified = true;
                entry.Property(m => m.desc).IsModified = true;
                entry.Property(m => m.flag).IsModified = true;
                await db.SaveChangesAsync();
                this.success(TM.Common.Language.msgUpdateSucsess);
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                this.danger(TM.Common.Language.msgUpdateError);
            }
            return View(group);
        }

        [HttpGet]
        public async Task<JsonResult> update_flag(string uid)
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
                await db.SaveChangesAsync();
                return Json(new { success = (flag == 0 ? TM.Common.Language.msgDeleteSucsess : TM.Common.Language.msgRecoverSucsess) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception) { return Json(new { danger = TM.Common.Language.msgError }, JsonRequestBehavior.AllowGet); }
        }
    }
}