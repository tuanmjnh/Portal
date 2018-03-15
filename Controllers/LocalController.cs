using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Portal.Models;
using TM.Message;
using PagedList;

namespace Portal.Controllers
{
    [Filters.AuthVinaphone()]
    public class LocalController : BaseController
    {
        // GET: Local
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

                var rs = from d in db.locals select d;

                if (!String.IsNullOrEmpty(searchString))
                    rs = rs.Where(d => d.title.Contains(searchString) || d.key_name.Contains(searchString));

                if (flag == 0) rs = rs.Where(d => d.flag == 0);
                else rs = rs.Where(d => d.flag == 1);

                switch (order)
                {
                    case "key_name_desc":
                        rs = rs.OrderByDescending(d => d.key_name);
                        break;
                    case "key_name_asc":
                        rs = rs.OrderBy(d => d.key_name);
                        break;
                    case "title_desc":
                        rs = rs.OrderByDescending(d => d.title);
                        break;
                    case "title_asc":
                        rs = rs.OrderBy(d => d.title);
                        break;
                    case "id_asc":
                        rs = rs.OrderByDescending(d => d.id);
                        break;
                    default:
                        rs = rs.OrderBy(d => d.id);
                        break;
                }

                //Export to any
                //if (!String.IsNullOrEmpty(export))
                //{
                //    Export(getDataTable(rs), export);
                //    return RedirectToAction("Index");
                //}

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

            //return View(db.locals.ToList());
        }

        // GET: Local/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            local local = db.locals.Find(id);
            if (local == null)
            {
                return HttpNotFound();
            }
            return View(local);
        }

        // GET: Local/Create
        public ActionResult Create()
        {
            ViewBag.local = db.locals.Where(d => d.flag > 0).ToList();
            return View();
        }

        // POST: Local/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,parent_id,key_name,title,flag")] local local)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var parent = db.locals.Find(local.parent_id);
                    var parents_id = parent != null ? parent.parents_id + local.parent_id.ToString() + "," : ",";
                    local.key_id = Common.Objects.LocalType.hd;
                    local.parents_id = parents_id;
                    local.total_item = 0;
                    local.created_by = Authentication.Auth.AuthUser.id.ToString();
                    local.created_at = DateTime.Now;
                    local.key_name = local.key_name.ToUpper();
                    db.locals.Add(local);
                    db.SaveChanges();
                    this.success(TM.Common.Language.msgCreateSucsess);
                }
            }
            catch (Exception)
            {
                this.danger(TM.Common.Language.msgCreateError);
            }
            return RedirectToAction("Create");
        }

        // GET: Local/Edit/5
        public ActionResult Edit(long? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            local local = db.locals.Find(id);
            if (local == null)
                return HttpNotFound();

            ViewBag.local = db.locals.Where(d => d.flag > 0).ToList();
            return View(local);
        }

        // POST: Local/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id, parent_id, key_name, title, flag")] local local)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var parent = db.locals.Find(local.parent_id);
                    var parents_id = parent != null ? parent.parents_id + local.parent_id.ToString() + "," : ",";
                    var key_name = local.key_name.ToUpper();
                    local = db.locals.Find(local.id);
                    local.key_id = Common.Objects.LocalType.hd;
                    local.parents_id = parents_id;
                    local.total_item = 0;
                    local.updated_by = Authentication.Auth.AuthUser.id.ToString();
                    local.updated_at = DateTime.Now;
                    local.key_name = key_name;
                    db.Entry(local).State = EntityState.Modified;
                    db.SaveChanges();
                    this.success(TM.Common.Language.msgUpdateSucsess);
                }
            }
            catch (Exception)
            {
                this.danger(TM.Common.Language.msgUpdateError);
            }
            return RedirectToAction("Index");
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public JsonResult Delete(string uid)
        //{
        //    try
        //    {
        //        string[] id = uid.Split(',');
        //        foreach (var item in id)
        //        {
        //            long tmp = long.Parse(item);
        //            var rs = db.collected_staff.Find(tmp);
        //            db.collected_staff.Remove(rs);
        //        }
        //        db.SaveChanges();
        //        return Json(new { success = TM.Common.Language.msgDeleteSucsess }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception) { return Json(new { danger = TM.Common.Language.msgError }, JsonRequestBehavior.AllowGet); }
        //}

        [HttpGet]
        public JsonResult update_flag(string uid)
        {
            try
            {
                string[] id = uid.Split(',');
                var flag = 0;
                foreach (var item in id)
                {
                    long tmp = long.Parse(item);
                    var rs = db.locals.Find(tmp);
                    rs.flag = flag = rs.flag == 1 ? 0 : 1;
                }
                db.SaveChanges();
                return Json(new { success = (flag == 0 ? TM.Common.Language.msgDeleteSucsess : TM.Common.Language.msgRecoverSucsess) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception) { return Json(new { danger = TM.Common.Language.msgError }, JsonRequestBehavior.AllowGet); }
        }
    }
}
