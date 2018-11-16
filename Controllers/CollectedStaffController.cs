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
using Dapper;

namespace Portal.Controllers
{
    [Filters.AuthVinaphone()]
    public class CollectedStaffController : BaseController
    {
        // GET: CollectedStaff
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
                ViewBag.local = db.locals.Where(d => d.flag > 0).ToList();

                var rs = from d in db.collected_staff select d;

                if (!String.IsNullOrEmpty(searchString) && TM.Helper.Regex.isNumber(searchString))
                {
                    var id = long.Parse(searchString);
                    rs = rs.Where(d => d.id == id || d.local_id == id);
                }
                else if (!String.IsNullOrEmpty(searchString))
                    rs = rs.Where(d =>
                    d.nvql.Contains(searchString) ||
                    d.dia_chi.Contains(searchString));

                if (flag == 0) rs = rs.Where(d => d.flag == 0);
                else rs = rs.Where(d => d.flag == 1);

                switch (order)
                {
                    case "nvql_desc":
                        rs = rs.OrderByDescending(d => d.nvql);
                        break;
                    case "nvql_asc":
                        rs = rs.OrderBy(d => d.nvql);
                        break;
                    case "local_desc":
                        rs = rs.OrderByDescending(d => d.local_id);
                        break;
                    case "local_asc":
                        rs = rs.OrderBy(d => d.local_id);
                        break;
                    case "diachi_desc":
                        rs = rs.OrderByDescending(d => d.local_id);
                        break;
                    case "diachi_asc":
                        rs = rs.OrderBy(d => d.local_id);
                        break;
                    case "xaphuong_desc":
                        rs = rs.OrderByDescending(d => d.local_id);
                        break;
                    case "xaphuong_asc":
                        rs = rs.OrderBy(d => d.local_id);
                        break;
                    case "tothon_desc":
                        rs = rs.OrderByDescending(d => d.local_id);
                        break;
                    case "tothon_asc":
                        rs = rs.OrderBy(d => d.local_id);
                        break;
                    case "id_asc":
                        rs = rs.OrderByDescending(d => d.id);
                        break;
                    default:
                        rs = rs.OrderBy(d => d.id);
                        break;
                }

                //Export to any
                if (!String.IsNullOrEmpty(export))
                {
                    TM.Exports.ExportExcel(TM.Helper.Data.ToDataTable(rs.ToList()), "Danh sách cán bộ thu");
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

        // GET: CollectedStaff/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            collected_staff collected_staff = db.collected_staff.Find(id);
            if (collected_staff == null)
            {
                return HttpNotFound();
            }
            return View(collected_staff);
        }

        // GET: CollectedStaff/Create
        public ActionResult Create()
        {
            ViewBag.local = db.locals.Where(d => d.flag > 0).ToList();
            return View();
        }

        // POST: CollectedStaff/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,local_id,nvql,dia_chi,xa_phuong,to_thon,flag")] collected_staff collected_staff)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    //var id = PortalContext().Query("select MAX(id)+1 as id from collected_staff").First().id;
                    //collected_staff.id = id != null ? (long)id : 1;
                    collected_staff.created_by = Authentication.Auth.AuthUser.id.ToString();
                    collected_staff.created_at = DateTime.Now;
                    db.collected_staff.Add(collected_staff);
                    db.SaveChanges();
                    this.success(TM.Common.Language.msgCreateSucsess);
                }
                else
                    this.danger(TM.Common.Language.msgCreateError);
            }
            catch (Exception)
            {
                this.danger("Mã nhân viên quản lý đã tồn tại");
            }
            return RedirectToAction("Create");
        }

        // GET: CollectedStaff/Edit/5
        public ActionResult Edit(long? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            collected_staff collected_staff = db.collected_staff.Find(id);
            if (collected_staff == null)
                return HttpNotFound();

            ViewBag.local = db.locals.Where(d => d.flag > 0).ToList();
            return View(collected_staff);
        }

        // POST: CollectedStaff/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,local_id,nvql,dia_chi,xa_phuong,to_thon,flag")] collected_staff collected_staff)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    collected_staff.updated_by = Authentication.Auth.AuthUser.id.ToString();
                    collected_staff.updated_at = DateTime.Now;
                    db.Entry(collected_staff).State = EntityState.Modified;
                    db.SaveChanges();
                    this.success(TM.Common.Language.msgUpdateSucsess);
                    return RedirectToAction("Index");
                }
                else
                    this.danger(TM.Common.Language.msgUpdateError);
            }
            catch (Exception)
            {
                this.danger(TM.Common.Language.msgUpdateError);
            }
            return View();
        }

        // GET: CollectedStaff/Delete/5
        //public ActionResult Delete(long? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    collected_staff collected_staff = db.collected_staff.Find(id);
        //    if (collected_staff == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(collected_staff);
        //}

        //// POST: CollectedStaff/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(long id)
        //{
        //    collected_staff collected_staff = db.collected_staff.Find(id);
        //    db.collected_staff.Remove(collected_staff);
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Delete(string[] uid)
        {
            try
            {
                foreach (var item in uid)
                {
                    long tmp = long.Parse(item);
                    var rs = db.collected_staff.Find(tmp);
                    db.collected_staff.Remove(rs);
                }
                db.SaveChanges();
                return Json(new { success = TM.Common.Language.msgDeleteSucsess }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception) { return Json(new { danger = TM.Common.Language.msgError }, JsonRequestBehavior.AllowGet); }
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
                    long tmp = long.Parse(item);
                    var rs = db.collected_staff.Find(tmp);
                    rs.flag = flag = rs.flag == 1 ? 0 : 1;
                }
                db.SaveChanges();
                return Json(new { success = (flag == 0 ? TM.Common.Language.msgDeleteSucsess : TM.Common.Language.msgRecoverSucsess) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception) { return Json(new { danger = TM.Common.Language.msgError }, JsonRequestBehavior.AllowGet); }
        }
    }
}
