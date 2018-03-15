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
    public class KTRController : Portal.Controllers.BaseController
    {
        // GET: DVCNTT/KTR
        public ActionResult Index(int? flag, string order, string currentFilter, string searchString, int? page, string export)
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

                var rs = from d in db.items
                         join g in db.groups on d.id_key equals g.id.ToString()
                         where d.app_key == Common.Objects.groups.ktr
                         select new
                         {
                             id = d.id,
                             appkey = d.app_key,
                             idkey = d.id_key,
                             codekey = d.code_key,
                             title = d.title,
                             desc = d.desc,
                             quantity = d.quantity,
                             quantitytotal = d.quantity_total,
                             price = g.parent_id,
                             priceToken = g.parents_id,
                             images = d.images,
                             startedat = d.started_at,
                             endedat = d.ended_at,
                             created_by = d.created_by,
                             created_at = d.created_at,
                             updated_by = d.updated_by,
                             updated_at = d.updated_at,
                             flag = d.flag,
                             extras = d.extras,
                             groupname = g.title
                         };

                if (!String.IsNullOrEmpty(searchString))
                    rs = rs.Where(d =>
                    d.title.Contains(searchString) ||
                    d.codekey.Contains(searchString) ||
                    d.extras.Contains(searchString) ||
                    d.groupname.Contains(searchString));

                if (flag == 0) rs = rs.Where(d => d.flag == 0);
                else rs = rs.Where(d => d.flag == 1);

                switch (order)
                {
                    case "group_asc":
                        rs = rs.OrderBy(d => d.groupname);
                        break;
                    case "group_desc":
                        rs = rs.OrderByDescending(d => d.groupname);
                        break;
                    case "code_asc":
                        rs = rs.OrderBy(d => d.codekey);
                        break;
                    case "code_desc":
                        rs = rs.OrderByDescending(d => d.codekey);
                        break;
                    case "account_asc":
                        rs = rs.OrderBy(d => d.extras);
                        break;
                    case "account_desc":
                        rs = rs.OrderByDescending(d => d.extras);
                        break;
                    case "title_desc":
                        rs = rs.OrderByDescending(d => d.title);
                        break;
                    default:
                        rs = rs.OrderBy(d => d.title);
                        break;
                }

                //Export to any
                if (!String.IsNullOrEmpty(export))
                {
                    TM.Exports.ExportExcel(TM.Helper.Data.ToDataTable(rs.ToList()), "Danh sách KTR");
                    return RedirectToAction("Index");
                }

                ViewBag.TotalRecords = rs.Count();
                int pageSize = 15;
                int pageNumber = (page ?? 1);

                return View(rs.AsEnumerable().Select(d => d.ToExpando()).ToPagedList(pageNumber, pageSize));
            }
            catch (Exception ex)
            {
                this.danger(ex.Message);
            }
            return View();
        }

        // GET: DVCNTT/KTR/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            item item = db.items.Find(id);
            if (item == null)
            {
                return HttpNotFound();
            }
            return View(item);
        }

        // GET: DVCNTT/KTR/Create
        public ActionResult Create()
        {
            ViewBag.services = getGroups(Common.Objects.groups.ktr);
            return View();
        }

        // POST: DVCNTT/KTR/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id_key,code_key,title,desc,quantity,images,flag,extras")] item item)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    item.id = Guid.NewGuid();
                    item.app_key = Common.Objects.groups.ktr;
                    item.created_by = Authentication.Auth.AuthUser.id.ToString();
                    item.created_at = DateTime.Now;
                    db.items.Add(item);
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
            return View(item);
        }

        // GET: DVCNTT/KTR/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            item item = db.items.Find(id);
            if (item == null)
            {
                return HttpNotFound();
            }
            ViewBag.services = getGroups(Common.Objects.groups.ktr);
            return View(item);
        }

        // POST: DVCNTT/KTR/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,id_key,code_key,title,desc,quantity,images,flag,extras")] item item_tmp)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var item = db.items.Find(item_tmp.id);
                    item.app_key = Common.Objects.groups.ktr;
                    item.updated_by = Authentication.Auth.AuthUser.id.ToString();
                    item.updated_at = DateTime.Now;
                    db.Entry(item).State = EntityState.Modified;
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
            return View(item_tmp);
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
                    var rs = db.items.Find(tmp);
                    rs.flag = flag = rs.flag == 1 ? 0 : 1;
                }
                db.SaveChanges();
                return Json(new { success = (flag == 0 ? TM.Common.Language.msgDeleteSucsess : TM.Common.Language.msgRecoverSucsess) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception) { return Json(new { danger = TM.Common.Language.msgError }, JsonRequestBehavior.AllowGet); }
        }
    }
}
