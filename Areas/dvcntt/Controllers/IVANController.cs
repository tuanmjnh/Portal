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
    public class IVANController : Portal.Controllers.BaseController
    {
        // GET: dvcntt/IVAN
        public ActionResult Index(int? flag, string order, string currentFilter, string searchString, int? page, string datetime, int? datetimeType, string export)
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
                ViewBag.datetime = datetime;
                ViewBag.datetimeType = datetimeType;

                var rs = (from d in db.items
                          join g in db.groups on d.id_key equals g.id.ToString()
                          join l in db.locals on d.url equals l.id.ToString()
                          join u in db.users on d.extras equals u.id.ToString()
                          where d.app_key == Common.Objects.groups.ivan
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
                              url = d.url,
                              author = d.author,
                              attach = d.attach,
                              startedat = d.started_at,
                              endedat = d.ended_at,
                              createdby = d.created_by,
                              createdat = d.created_at,
                              updatedby = d.updated_by,
                              updatedat = d.updated_at,
                              flag = d.flag,
                              extras = d.extras,
                              groupname = g.title,
                              donvi = l.title,
                              level = g.level,
                              staff = u.full_name,
                              hasToken = g.extras
                          }).ToList().AsEnumerable();

                if (!String.IsNullOrEmpty(searchString))
                    rs = rs.Where(d =>
                    d.title.Contains(searchString) ||
                    d.codekey.Contains(searchString) ||
                    d.extras.Contains(searchString) ||
                    d.donvi.Contains(searchString));

                if (!String.IsNullOrEmpty(datetime))
                {
                    var date = datetime.Split('-');
                    if (date.Length > 1)
                    {
                        var dateStart = TM.Format.Formating.StartOfDate(TM.Format.Formating.DateParseExactVNToEN(date[0]));
                        var dateEnd = TM.Format.Formating.EndOfDate(TM.Format.Formating.DateParseExactVNToEN(date[1]));
                        rs = datetimeType == 0 ? rs.Where(d => d.createdat >= dateStart && d.createdat <= dateEnd) : rs.Where(d => d.updatedat >= dateStart && d.updatedat <= dateEnd);
                    }
                }

                if (flag == 0) rs = rs.Where(d => d.flag == 0);
                else rs = rs.Where(d => d.flag > 0);

                switch (order)
                {
                    case "codekey_asc":
                        rs = rs.OrderBy(d => d.codekey);
                        break;
                    case "codekey_desc":
                        rs = rs.OrderByDescending(d => d.codekey);
                        break;
                    case "title_asc":
                        rs = rs.OrderBy(d => d.title);
                        break;
                    case "title_desc":
                        rs = rs.OrderByDescending(d => d.title);
                        break;
                    case "staff_asc":
                        rs = rs.OrderBy(d => d.staff);
                        break;
                    case "staff_desc":
                        rs = rs.OrderByDescending(d => d.staff);
                        break;
                    case "level_asc":
                        rs = rs.OrderBy(d => d.attach);
                        break;
                    case "level_desc":
                        rs = rs.OrderByDescending(d => d.attach);
                        break;
                    case "groupname_asc":
                        rs = rs.OrderBy(d => d.attach);
                        break;
                    case "groupname_desc":
                        rs = rs.OrderByDescending(d => d.attach);
                        break;
                    case "quantity_asc":
                        rs = rs.OrderBy(d => d.quantity);
                        break;
                    case "quantity_desc":
                        rs = rs.OrderByDescending(d => d.quantity);
                        break;
                    case "price_asc":
                        rs = rs.OrderBy(d => d.price);
                        break;
                    case "price_desc":
                        rs = rs.OrderByDescending(d => d.price);
                        break;
                    case "donvi_asc":
                        rs = rs.OrderBy(d => d.donvi);
                        break;
                    case "donvi_desc":
                        rs = rs.OrderByDescending(d => d.donvi);
                        break;
                    case "createdat_asc":
                        rs = rs.OrderBy(d => d.createdat);
                        break;
                    default:
                        rs = rs.OrderByDescending(d => d.createdat);
                        break;
                }

                //Export to any
                if (!String.IsNullOrEmpty(export))
                {
                    TM.Exports.ExportExcel(TM.Helper.Data.ToDataTable(rs.ToList()), "Danh sách CA");
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

        // GET: dvcntt/CA/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var item = db.items.Find(id);
            if (item == null)
            {
                return HttpNotFound();
            }
            return View(item);
        }
        // GET: dvcntt/CA/Create
        public ActionResult Create()
        {
            ViewBag.services = getGroups(Common.Objects.groups.ivan).OrderBy(d => d.level).ThenBy(d => d.extras).ThenBy(d => d.title).ToList();
            ViewBag.local = db.locals.Where(d => d.flag > 0).ToList();
            ViewBag.users = db.users.Where(d => d.flag > 0 && d.roles == Authentication.Roles.staff).OrderBy(d => d.staff_id).ThenBy(d => d.full_name).ToList();
            return View();
        }
        // GET: dvcntt/CA/Create
        public ActionResult PartialCreate()
        {
            ViewBag.IvanList = getGroups(Common.Objects.groups.ivan).OrderBy(d => d.level).ThenBy(d => d.extras).ThenBy(d => d.title).ToList();
            ViewBag.TokenList = getGroups(Common.Objects.groups.token).OrderBy(d => d.level).ThenBy(d => d.extras).ThenBy(d => d.title).ToList();
            ViewBag.ivanOfCustomer = "";
            ViewBag.caOfCustomer = "";
            return PartialView();
        }

        // POST: dvcntt/CA/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult Create(DVCNTT item, FormCollection collection)
        {

            if (ModelState.IsValid)
            {
                //var group = db.groups.Find(Guid.Parse(item.id_key));
                //item.id = Guid.NewGuid();
                //item.app_key = Common.Objects.groups.ivan;
                //item.price = decimal.Parse(group.parent_id) + decimal.Parse(group.parents_id);
                //item.author = "," + collection["authorUser"] + "," + collection["authorPass"] + ",";
                //var file = TM.IO.FileDirectory.Upload(Request.Files, TM.Common.Directories.IVAN, false);
                //if (file.UploadFile().Count > 0)
                //    item.attach = "," + file.UploadFileString() + ",";
                //Check group
                try { var tmp = Guid.Parse(item.groupID); }
                catch (Exception ex) { return Json(new { danger = "Vui lòng chọn gói dịch vụ!" }, JsonRequestBehavior.AllowGet); }

                var group = db.groups.Find(Guid.Parse(item.groupID));
                item.price = decimal.Parse(group.parent_id);
                if (item.extraID != "0")
                {
                    var extra = db.groups.Find(Guid.Parse(item.extraID));
                    item.priceExtra = decimal.Parse(extra.parents_id);
                }
                else item.priceExtra = 0;
                item.app_key = Common.Objects.groups.ivan;
                item.total = item.price + item.priceExtra;
                item.vat = item.total / 10;
                item.quantity = "1";
                item.createdBy = Authentication.Auth.AuthUser.id.ToString();
                item.createdAt = DateTime.Now;
                item.updatedBy = Authentication.Auth.AuthUser.id.ToString();
                item.updatedAt = DateTime.Now;
                item.flag = 1;
                db.DVCNTTs.Add(item);
                //var sub_item = new Models.sub_item();
                //sub_item.id = Guid.NewGuid();
                //sub_item.item_id = item.id;
                //sub_item.price = decimal.Parse(group.parent_id) + decimal.Parse(group.parents_id);
                //sub_item.app_key = Common.Objects.groups.caivan;
                //sub_item.quantity = item.quantity;
                //sub_item.images = item.attach;
                //db.sub_items.Add(sub_item);
                db.SaveChanges();
                return Json(new { success = TM.Common.Language.msgCreateSucsess, data = item }, JsonRequestBehavior.AllowGet);
            }
            else
                return Json(new { danger = TM.Common.Language.msgError }, JsonRequestBehavior.AllowGet);
        }
        //public ActionResult Create(DVCNTT item, FormCollection collection)
        //{
        //    try
        //    {
        //        if (ModelState.IsValid)
        //        {
        //            var group = db.groups.Find(Guid.Parse(item.id_key));
        //            item.id = Guid.NewGuid();
        //            item.app_key = Common.Objects.groups.ivan;
        //            item.price = decimal.Parse(group.parent_id) + decimal.Parse(group.parents_id);
        //            item.author = "," + collection["authorUser"] + "," + collection["authorPass"] + ",";
        //            //var file = TM.IO.FileDirectory.Upload(Request.Files, TM.Common.Directories.IVAN, false);
        //            //if (file.UploadFile().Count > 0)
        //            //    item.attach = "," + file.UploadFileString() + ",";
        //            item.createdBy = Authentication.Auth.AuthUser.id.ToString();
        //            item.createdAt = DateTime.Now;
        //            item.updatedBy = Authentication.Auth.AuthUser.id.ToString();
        //            item.updatedAt = DateTime.Now;
        //            db.DVCNTTs.Add(item);
        //            //var sub_item = new Models.sub_item();
        //            //sub_item.id = Guid.NewGuid();
        //            //sub_item.item_id = item.id;
        //            //sub_item.price = decimal.Parse(group.parent_id) + decimal.Parse(group.parents_id);
        //            //sub_item.app_key = Common.Objects.groups.caivan;
        //            //sub_item.quantity = item.quantity;
        //            //sub_item.images = item.attach;
        //            //db.sub_items.Add(sub_item);
        //            db.SaveChanges();
        //            this.success(TM.Common.Language.msgCreateSucsess);
        //            return RedirectToAction("Create");
        //        }
        //        else
        //            this.danger(TM.Common.Language.msgCreateError);
        //    }
        //    catch (Exception ex)
        //    {
        //        this.danger(ex.Message);
        //    }
        //    return View(item);
        //}

        // GET: dvcntt/CA/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var item = db.items.Find(id);
            if (item == null)
            {
                return HttpNotFound();
            }
            ViewBag.services = getGroups(Common.Objects.groups.ivan).OrderBy(d => d.level).ThenBy(d => d.extras).ThenBy(d => d.title).ToList();
            ViewBag.local = db.locals.Where(d => d.flag > 0).ToList();
            ViewBag.users = db.users.Where(d => d.flag > 0 && d.roles == Authentication.Roles.staff).OrderBy(d => d.staff_id).ThenBy(d => d.full_name).ToList();
            return View(item);
        }

        // POST: dvcntt/CA/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,app_key,id_key,code_key,title,desc,quantity,quantity_total,price_old,price,images,url,author,attach,flag,extras")] item item_tmp, FormCollection collection)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var group = db.groups.Find(Guid.Parse(item_tmp.id_key));
                    var item = db.items.Find(item_tmp.id);
                    item.id_key = item_tmp.id_key;
                    item.code_key = item_tmp.code_key;
                    item.title = item_tmp.title;
                    item.price = decimal.Parse(group.parent_id) + decimal.Parse(group.parents_id);
                    item.desc = item_tmp.desc;
                    item.quantity = item_tmp.quantity;
                    item.images = item_tmp.images;
                    item.url = item_tmp.url;
                    item.author = collection["authorUser"] + "," + collection["authorPass"];
                    var file = TM.IO.FileDirectory.Upload(Request.Files, TM.Common.Directories.IVAN, false);
                    if (file.UploadFile().Count > 0)
                        item.attach = "," + file.UploadFileString() + ",";
                    item.flag = item_tmp.flag;
                    item.extras = item_tmp.extras;
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
