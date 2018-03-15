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
    [Filters.AuthVinaphone(Role = Authentication.Roles.superadmin + "," + Authentication.Roles.admin + "," + Authentication.Roles.director)]
    public class ManagerController : Portal.Controllers.BaseController
    {
        // GET: vanban/Manager
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

                var rs = from d in db.items where d.app_key == Common.Objects.groups.document select d;

                if (!String.IsNullOrEmpty(searchString))
                    rs = rs.Where(d =>
                    d.title.Contains(searchString));

                if (flag == 0) rs = rs.Where(d => d.flag == 0);
                else rs = rs.Where(d => d.flag > 0);

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

        // GET: vanban/Manager/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Models.item item = db.items.Find(id);
            if (item == null)
            {
                return HttpNotFound();
            }
            return View(item);
        }

        // GET: vanban/Manager/Create
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
        public ActionResult Create([Bind(Include = "code_key,title,desc,flag,extras")] Models.item item, FormCollection collection)
        {
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
        public ActionResult Edit([Bind(Include = "id,code_key,title,desc")] Models.item item_tmp)
        {
            if (ModelState.IsValid)
            {
                var item = db.items.Find(item_tmp.id);
                item.code_key = item_tmp.code_key;
                item.title = item_tmp.title;
                item.desc = item_tmp.desc;
                var file = TM.IO.FileDirectory.Upload(Request.Files, TM.Common.Directories.document, false);
                if (file != null)
                    item.attach = "," + file.UploadFileString() + ",";
                item.updated_by = Authentication.Auth.AuthUser.id.ToString();
                item.updated_at = DateTime.Now;
                db.Entry(item).State = EntityState.Modified;
                db.SaveChanges();
                this.success(TM.Common.Language.msgUpdateSucsess);
                return RedirectToAction("Index");
            }
            else
                this.danger(TM.Common.Language.msgUpdateError);
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
        public ActionResult UserList(int? flag, string order, string currentFilter, string searchString, int? page, Guid? department)
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
                ViewBag.groupID = department.HasValue ? department.Value : Guid.Empty;
                ViewBag.department = getGroups(Common.Objects.groups.department);

                var auth_id = Authentication.Auth.AuthUser.id;
                var rs = from d in db.users where (d.roles == Authentication.Roles.staff || d.roles == Authentication.Roles.leader) && d.id != auth_id && d.flag > 0 select d;

                if (!String.IsNullOrEmpty(searchString))
                    rs = rs.Where(d => d.username.Contains(searchString) || d.full_name.Contains(searchString));

                if (department != null)
                    rs = rs.Where(d => d.staff_id == department);

                //if (flag == 0) rs = rs.Where(d => d.flag == 0);
                //else rs = rs.Where(d => d.flag == 1);

                switch (order)
                {
                    case "fullname_asc":
                        rs = rs.OrderBy(d => d.full_name);
                        break;
                    case "fullname_desc":
                        rs = rs.OrderByDescending(d => d.full_name);
                        break;
                    case "username_desc":
                        rs = rs.OrderByDescending(d => d.username);
                        break;
                    default:
                        rs = rs.OrderBy(d => d.username);
                        break;
                }

                ViewBag.TotalRecords = rs.Count();
                int pageSize = 10;
                int pageNumber = (page ?? 1);

                return PartialView(rs.ToPagedList(pageNumber, pageSize));
            }
            catch (Exception)
            {
                this.danger(TM.Common.Language.msgError);
            }
            return PartialView();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult LoadNotification()
        {
            try
            {
                var authID = Authentication.Auth.AuthUser.id.ToString();
                var tmp = db.items.Where(d => d.app_key == Common.Objects.groups.document && d.created_by == authID && d.flag > 0).ToList();
                Models.item item = null;
                var count = 0;
                foreach (var i in tmp)
                {
                    var images = i.images.Trim(',');
                    if (!string.IsNullOrEmpty(images))
                    {
                        var list = images.Split(',');
                        count = list.Length;
                        if (count > i.quantity_total)
                        {
                            item = i;
                            break;
                        }
                    }
                }
                if (item != null)
                {
                    var UserConfirm = item.images.Trim(',').Split(',');
                    item.quantity_total = item.quantity_total + 1;
                    //if (count == item.extras.Trim(',').Split(',').Length)
                    //    item.quantity = 2;
                    db.Entry(item).State = EntityState.Modified;
                    db.SaveChanges();
                    return Json(new
                    {
                        title = "Xác nhận văn bản!",
                        content = "Văn bản số: " + item.code_key + " - " + item.title + " - " + GetUser(UserConfirm[UserConfirm.Length - 1]) + " Xác nhận",
                        location = Url.Action("Edit", "Manager", new { area = "vanban" }) + "/" + item.id,
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
