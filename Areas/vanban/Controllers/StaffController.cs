using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TM.Message;
using TM.Format;
using PagedList;

namespace Portal.Areas.vanban.Controllers
{
    [Filters.AuthVinaphone()]
    public class StaffController : Portal.Controllers.BaseController
    {
        // GET: vanban/staff
        public ActionResult Index(int? flag, string order, string currentFilter, string searchString, int? page, string export)
        {
            try
            {
                if (Authentication.Auth.AuthUser.roles == Authentication.Roles.director)
                    return RedirectToAction("Index", "Manager", new { area = "vanban" });
                if (searchString != null)
                {
                    page = 1;
                    searchString = searchString.Trim();
                }
                else searchString = currentFilter;
                ViewBag.order = order;
                ViewBag.currentFilter = searchString;
                ViewBag.flag = flag;

                var authID = "," + Authentication.Auth.AuthUser.id.ToString() + ",";
                var rs = from d in db.items
                         where d.app_key == Common.Objects.groups.document && d.extras.Contains(authID) && d.flag > 0
                         select d;

                if (!String.IsNullOrEmpty(searchString))
                    rs = rs.Where(d =>
                    d.title.Contains(searchString));

                //if (flag == 0) rs = rs.Where(d => d.flag == 0);
                //else rs = rs.Where(d => d.flag == 1);

                switch (order)
                {
                    case "code_asc":
                        rs = rs.OrderBy(d => d.code_key);
                        break;
                    case "code_desc":
                        rs = rs.OrderByDescending(d => d.code_key);
                        break;
                    case "title_asc":
                        rs = rs.OrderBy(d => d.title);
                        break;
                    case "title_desc":
                        rs = rs.OrderByDescending(d => d.title);
                        break;
                    case "started_asc":
                        rs = rs.OrderBy(d => d.started_at);
                        break;
                    case "started_desc":
                        rs = rs.OrderByDescending(d => d.started_at);
                        break;
                    case "ended_asc":
                        rs = rs.OrderBy(d => d.ended_at);
                        break;
                    case "ended_desc":
                        rs = rs.OrderByDescending(d => d.ended_at);
                        break;
                    case "user_asc":
                        rs = rs.OrderBy(d => d.extras);
                        break;
                    case "user_desc":
                        rs = rs.OrderByDescending(d => d.extras);
                        break;
                    case "created_asc":
                        rs = rs.OrderBy(d => d.created_at);
                        break;
                    default:
                        rs = rs.OrderByDescending(d => d.created_at);
                        break;
                }

                //Export to any
                if (!String.IsNullOrEmpty(export))
                {
                    TM.Exports.ExportExcel(TM.Helper.Data.ToDataTable(rs.ToList()), "Danh sách văn bản bàn giao");
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
        public ActionResult Create()
        {
            //ViewBag.users = db.users.Where(d => d.roles == Authentication.Roles.mod && d.id != Authentication.Auth.AuthUser.id && d.flag > 0);
            return View();
        }

        // POST: vanban/Manager/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "code_key,title,desc,flag")] Models.item item, FormCollection collection)
        {
            if (Authentication.Auth.AuthUser.roles == Authentication.Roles.director)
                return RedirectToAction("Index", "Manager", new { area = "vanban" });
            if (ModelState.IsValid)
            {
                item.id = Guid.NewGuid();
                item.started_at = collection["started_at"].StringToDatetime();
                if (!string.IsNullOrEmpty(collection["ended_at"]))
                    item.ended_at = collection["ended_at"].StringToDatetime();
                item.quantity = 0;
                item.quantity_total = 0;
                item.author = ",";
                item.images = ",";
                var file = TM.IO.FileDirectory.Upload(Request.Files, TM.Common.Directories.document, false);
                if (file != null)
                    item.attach = "," + file.UploadFileString() + ",";
                item.app_key = Common.Objects.groups.document;
                item.created_by = Authentication.Auth.AuthUser.id.ToString();
                item.created_at = DateTime.Now;
                item.extras = "," + Authentication.Auth.AuthUser.id.ToString() + ",";
                db.items.Add(item);
                db.SaveChanges();
                this.success(TM.Common.Language.msgCreateSucsess);
                return RedirectToAction("Create");
            }
            else
                this.danger(TM.Common.Language.msgCreateError);
            return View(item);
        }
        // GET: vanban/Manager/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (Authentication.Auth.AuthUser.roles == Authentication.Roles.director)
                return RedirectToAction("Index", "Manager", new { area = "vanban" });
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Models.item item = db.items.Find(id);
            if (item == null)
            {
                return HttpNotFound();
            }
            //ViewBag.users = db.users.Where(d => d.roles == Authentication.Roles.mod && d.id != Authentication.Auth.AuthUser.id && d.flag > 0);
            return View(item);
        }

        // POST: vanban/Manager/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,code_key,title,desc")] Models.item item_tmp, FormCollection collection)
        {
            if (ModelState.IsValid)
            {
                var item = db.items.Find(item_tmp.id);
                item.code_key = item_tmp.code_key;
                item.title = item_tmp.title;
                item.desc = item_tmp.desc;
                item.quantity = 1;
                var file = TM.IO.FileDirectory.Upload(Request.Files, TM.Common.Directories.document, false);
                if (file != null)
                    if (collection["isAttach"] != null)
                        item.attach = item.attach + file.UploadFileString() + ",";
                    else
                        item.attach = "," + file.UploadFileString() + ",";
                item.updated_by = Authentication.Auth.AuthUser.id.ToString();
                item.updated_at = DateTime.Now;
                db.Entry(item).State = EntityState.Modified;
                db.SaveChanges();

                //
                item = db.items.Find(item_tmp.id);
                if (item.images.Trim(',').Split(',').Length >= item.extras.Trim(',').Split(',').Length)
                    item.quantity = 2;
                db.Entry(item).State = EntityState.Modified;
                db.SaveChanges();

                this.success(TM.Common.Language.msgUpdateSucsess);
                return RedirectToAction("Index");
            }
            else
                this.danger(TM.Common.Language.msgUpdateError);
            return View(item_tmp);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Accept(Guid id)
        {
            try
            {
                var item = db.items.Find(id);
                item.images = item.images + Authentication.Auth.AuthUser.id + ",";
                item.updated_by = Authentication.Auth.AuthUser.id.ToString();
                item.updated_at = DateTime.Now;
                item.quantity = 1;
                db.Entry(item).State = EntityState.Modified;
                db.SaveChanges();

                //
                item = db.items.Find(id);
                if (item.images.Trim(',').Split(',').Length >= item.extras.Trim(',').Split(',').Length)
                    item.quantity = 2;
                db.Entry(item).State = EntityState.Modified;
                db.SaveChanges();
                return Json(new { success = "Xác nhận thành công!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception) { return Json(new { danger = TM.Common.Language.msgError }, JsonRequestBehavior.AllowGet); }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult LoadNotification()
        {
            try
            {
                var authID = "," + Authentication.Auth.AuthUser.id.ToString() + ",";
                var item = db.items.Where(d => d.app_key == Common.Objects.groups.document && d.extras.Contains(authID) && !d.author.Contains(authID) && d.flag > 0).FirstOrDefault();
                if (item != null)
                {
                    item.author = item.author + Authentication.Auth.AuthUser.id.ToString() + ",";
                    db.Entry(item).State = EntityState.Modified;
                    db.SaveChanges();
                    return Json(new
                    {
                        title = "Bàn giao văn bản!",
                        content = "Văn bản số: " + item.code_key + " - " + item.title,
                        location = Url.Action("Edit", "Staff", new { area = "vanban" }) + "/" + item.id,
                        success = TM.Common.Language.msgSucsess
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = TM.Common.Language.msgSucsess }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception) { return Json(new { danger = TM.Common.Language.msgError }, JsonRequestBehavior.AllowGet); }
        }
    }
}
