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
    public class ItemReportController : Portal.Controllers.BaseController
    {
        // GET: baocao/ItemReport
        public async Task<ActionResult> Index(int? flag, string order, string currentFilter, string searchString, int? page, string datetime, int? datetimeType, string export)
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
                              //join gi in db.group_item on d.id equals gi.item_id
                          where d.app_key == Common.Objects.groups.reportDay
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
                              groups = (from g in db.groups join gi in db.group_item on g.id equals gi.group_id where gi.item_id.Value == d.id select g).ToList(),
                              department = db.groups.Where(m => m.flag > 0 && m.app_key == Common.Objects.groups.department).OrderBy(m => m.title).ToList()
                              //locals = db.locals.Where(m => d.attach.Contains(m.id.ToString())).ToList(),
                          }).AsQueryable();

                if (!String.IsNullOrEmpty(searchString))
                    rs = rs.Where(d =>
                    d.title.Contains(searchString) ||
                    d.codekey.Contains(searchString) ||
                    d.extras.Contains(searchString));
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
                    case "extras_asc":
                        rs = rs.OrderBy(d => d.extras);
                        break;
                    case "extras_desc":
                        rs = rs.OrderByDescending(d => d.extras);
                        break;
                    case "level_asc":
                        rs = rs.OrderBy(d => d.attach);
                        break;
                    case "level_desc":
                        rs = rs.OrderByDescending(d => d.attach);
                        break;
                    case "quantity_asc":
                        rs = rs.OrderBy(d => d.quantity);
                        break;
                    case "quantity_desc":
                        rs = rs.OrderByDescending(d => d.quantity);
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
                    TM.Exports.ExportExcel(TM.Helper.Data.ToDataTable(await rs.ToListAsync()), "Danh sách công việc");
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


        // GET: baocao/ItemReport/Details/5
        public async Task<ActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            item item = await db.items.FindAsync(id);
            if (item == null)
            {
                return HttpNotFound();
            }
            return View(item);
        }

        // GET: baocao/ItemReport/Create
        public async Task<ActionResult> Create()
        {
            ViewBag.department = await db.groups.Where(m => m.flag > 0 && m.app_key == Common.Objects.groups.department).OrderBy(m => m.title).ToListAsync();
            ViewBag.groups = await db.groups.Where(m => m.flag > 0 && m.app_key == Common.Objects.groups.reportDay).ToListAsync();
            return View();
        }

        // POST: baocao/ItemReport/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "title,desc,url,orders,flag")] item item, FormCollection collection)
        {
            try
            {
                var author = collection["author[]"];
                var attach = collection["attach[]"];
                item.id = Guid.NewGuid();
                item.app_key = Common.Objects.groups.reportDay;
                item.created_by = Authentication.Auth.AuthUser.id.ToString();
                item.created_at = DateTime.Now;
                item.updated_by = Authentication.Auth.AuthUser.id.ToString();
                item.updated_at = DateTime.Now;
                item.author = "," + author + ",";
                item.attach = "," + attach + ",";
                if (author != null)
                    foreach (var a in author.Split(','))
                    {
                        group_item gi = new group_item();
                        gi.id = Guid.NewGuid();
                        gi.app_key = Common.Objects.groups.reportDay;
                        gi.group_id = Guid.Parse(a);
                        gi.item_id = item.id;
                        gi.orders = 0;
                        db.group_item.Add(gi);
                    }
                db.items.Add(item);
                await db.SaveChangesAsync();
                this.success(TM.Common.Language.msgCreateSucsess);
                return RedirectToAction("Create");

            }
            catch (Exception)
            {
                this.danger(TM.Common.Language.msgCreateError);
            }
            ViewBag.department = await db.groups.Where(m => m.flag > 0 && m.app_key == Common.Objects.groups.department).OrderBy(m => m.title).ToListAsync();
            ViewBag.groups = await db.groups.Where(m => m.flag > 0 && m.app_key == Common.Objects.groups.reportDay).ToListAsync();
            return View(item);
        }

        // GET: baocao/ItemReport/Edit/5
        public async Task<ActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var item = await db.items.FindAsync(id);
            //var item = await (from d in db.items
            //                  select new
            //                  {
            //                      id = d.id,
            //                      appkey = d.app_key,
            //                      idkey = d.id_key,
            //                      codekey = d.code_key,
            //                      title = d.title,
            //                      desc = d.desc,
            //                      quantity = d.quantity,
            //                      quantitytotal = d.quantity_total,
            //                      images = d.images,
            //                      url = d.url,
            //                      author = d.author,
            //                      attach = d.attach,
            //                      startedat = d.started_at,
            //                      endedat = d.ended_at,
            //                      createdby = d.created_by,
            //                      createdat = d.created_at,
            //                      updatedby = d.updated_by,
            //                      updatedat = d.updated_at,
            //                      flag = d.flag,
            //                      extras = d.extras,
            //                      groups = (from g in db.groups join gi in db.group_item on g.id equals gi.group_id where gi.item_id.Value == d.id select g).ToList(),
            //                      locals = db.locals.Where(m => d.attach.Contains(m.id.ToString())).ToList(),
            //                  });
            if (item == null)
            {
                return HttpNotFound();
            }
            ViewBag.department = await db.groups.Where(m => m.flag > 0 && m.app_key == Common.Objects.groups.department).OrderBy(m => m.title).ToListAsync();
            ViewBag.groups = await db.groups.Where(m => m.flag > 0 && m.app_key == Common.Objects.groups.reportDay).ToListAsync();
            return View(item);
        }

        // POST: baocao/ItemReport/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "id,title,desc,url,orders,flag")] item item, FormCollection collection)
        {
            try
            {
                var author = collection["author[]"];
                var attach = collection["attach[]"];
                item.author = "," + author + ",";
                item.attach = "," + attach + ",";
                db.items.Attach(item);
                var entry = db.Entry(item);
                entry.Property(m => m.title).IsModified = true;
                entry.Property(m => m.desc).IsModified = true;
                entry.Property(m => m.url).IsModified = true;
                entry.Property(m => m.flag).IsModified = true;
                entry.Property(m => m.orders).IsModified = true;
                entry.Property(m => m.author).IsModified = true;
                entry.Property(m => m.attach).IsModified = true;
                foreach (var gi in db.group_item.Where(m => m.item_id == item.id).ToList())
                    db.group_item.Remove(gi);
                foreach (var a in author.Split(','))
                {
                    group_item gi = new group_item();
                    gi.id = Guid.NewGuid();
                    gi.app_key = Common.Objects.groups.reportDay;
                    gi.group_id = Guid.Parse(a);
                    gi.item_id = item.id;
                    gi.orders = 0;
                    db.group_item.Add(gi);
                }
                item.updated_by = Authentication.Auth.AuthUser.id.ToString();
                item.updated_at = DateTime.Now;
                await db.SaveChangesAsync();
                this.success(TM.Common.Language.msgUpdateSucsess);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                this.danger(TM.Common.Language.msgUpdateError);
            }
            ViewBag.department = await db.groups.Where(m => m.flag > 0 && m.app_key == Common.Objects.groups.department).OrderBy(m => m.title).ToListAsync();
            ViewBag.groups = await db.groups.Where(m => m.flag > 0 && m.app_key == Common.Objects.groups.reportDay).ToListAsync();
            return View(item);
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
