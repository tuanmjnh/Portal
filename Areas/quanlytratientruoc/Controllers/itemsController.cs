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

namespace Portal.Areas.quanlytratientruoc.Controllers
{
    public class itemsController : Portal.Controllers.BaseController
    {
        // GET: quanlytratientruoc/items
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

        // GET: quanlytratientruoc/items/Details/5
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

        // GET: quanlytratientruoc/items/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: quanlytratientruoc/items/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "id,app_key,id_key,code_key,title,desc,quantity,quantity_total,price_old,price,images,url,author,attach,orders,started_at,ended_at,created_by,created_at,updated_by,updated_at,flag,extras")] item item)
        {
            if (ModelState.IsValid)
            {
                item.id = Guid.NewGuid();
                db.items.Add(item);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(item);
        }

        // GET: quanlytratientruoc/items/Edit/5
        public async Task<ActionResult> Edit(Guid? id)
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

        // POST: quanlytratientruoc/items/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "id,app_key,id_key,code_key,title,desc,quantity,quantity_total,price_old,price,images,url,author,attach,orders,started_at,ended_at,created_by,created_at,updated_by,updated_at,flag,extras")] item item)
        {
            if (ModelState.IsValid)
            {
                db.Entry(item).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(item);
        }

        // GET: quanlytratientruoc/items/Delete/5
        public async Task<ActionResult> Delete(Guid? id)
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

        // POST: quanlytratientruoc/items/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            item item = await db.items.FindAsync(id);
            db.items.Remove(item);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
