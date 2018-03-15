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
    public class CAIVANController : Portal.Controllers.BaseController
    {
        // GET: dvcntt/CAIVAN
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
                          where d.app_key == Common.Objects.groups.ca
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
                              priceold = d.price_old,
                              price = d.price,
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
                        var tmp0 = date[0].Split('/');
                        var tmp1 = date[1].Split('/');
                        if (tmp0.Length > 2 && tmp1.Length > 2)
                        {
                            rs = datetimeType == 0
                                ? rs.Where(d =>
                                 d.createdat.Value.Day >= int.Parse(tmp0[0]) &&
                                 d.createdat.Value.Month >= int.Parse(tmp0[1]) &&
                                 d.createdat.Value.Year >= int.Parse(tmp0[2]) &&
                                 d.createdat.Value.Day <= int.Parse(tmp1[0]) &&
                                 d.createdat.Value.Month <= int.Parse(tmp1[1]) &&
                                 d.createdat.Value.Year <= int.Parse(tmp1[2]))
                                : rs.Where(d =>
                                 d.updatedat.Value.Day >= int.Parse(tmp0[0]) &&
                                 d.updatedat.Value.Month >= int.Parse(tmp0[1]) &&
                                 d.updatedat.Value.Year >= int.Parse(tmp0[2]) &&
                                 d.updatedat.Value.Day <= int.Parse(tmp1[0]) &&
                                 d.updatedat.Value.Month <= int.Parse(tmp1[1]) &&
                                 d.updatedat.Value.Year <= int.Parse(tmp1[2]));
                        }
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
                    case "extras_asc":
                        rs = rs.OrderBy(d => d.extras);
                        break;
                    case "extras_desc":
                        rs = rs.OrderByDescending(d => d.extras);
                        break;
                    case "attach_asc":
                        rs = rs.OrderBy(d => d.attach);
                        break;
                    case "attach_desc":
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
                    TM.Exports.ExportExcel(TM.Helper.Data.ToDataTable(rs.ToList()), "Danh sách CA-IVAN");
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

        // GET: dvcntt/CAIVAN/Details/5
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

        // GET: dvcntt/CAIVAN/Create
        public ActionResult Create()
        {
            ViewBag.services = getGroups(Common.Objects.groups.ca);
            ViewBag.local = db.locals.Where(d => d.flag > 0).ToList();
            return View();
        }

        // POST: dvcntt/CAIVAN/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id_key,code_key,title,quantity,desc,images,attach,url,author,flag,extras")] item item)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var group = db.groups.Find(Guid.Parse(item.id_key));
                    item.id = Guid.NewGuid();
                    item.app_key = Common.Objects.groups.ca;
                    item.price = decimal.Parse(group.parent_id) + decimal.Parse(group.parents_id);
                    item.created_by = Authentication.Auth.AuthUser.id.ToString();
                    item.created_at = DateTime.Now;
                    item.updated_by = Authentication.Auth.AuthUser.id.ToString();
                    item.updated_at = DateTime.Now;
                    db.items.Add(item);
                    //var sub_item = new Models.sub_item();
                    //sub_item.id = Guid.NewGuid();
                    //sub_item.item_id = item.id;
                    //sub_item.price = decimal.Parse(group.parent_id) + decimal.Parse(group.parents_id);
                    //sub_item.app_key = Common.Objects.groups.caivan;
                    //sub_item.quantity = item.quantity;
                    //sub_item.images = item.attach;
                    //db.sub_items.Add(sub_item);
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

        // GET: dvcntt/CAIVAN/Edit/5
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
            ViewBag.services = getGroups(Common.Objects.groups.ca);
            ViewBag.local = db.locals.Where(d => d.flag > 0).ToList();
            return View(item);
        }

        // POST: dvcntt/CAIVAN/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,id_key,code_key,title,quantity,desc,images,attach,url,author,flag,extras")] item item_tmp)
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
                    item.attach = item_tmp.attach;
                    item.url = item_tmp.url;
                    item.author = item_tmp.author;
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
