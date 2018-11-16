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

namespace Portal.Areas.dvcntt.Controllers
{
    public class ReportSalesController : Portal.Controllers.BaseController
    {
        // GET: dvcntt/ReportSales
        public async Task<ActionResult> Index()
        {
            return View(await db.items.ToListAsync());
        }

        // GET: dvcntt/ReportSales/Details/5
        public async Task<ActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var item = await db.items.FindAsync(id);
            if (item == null)
            {
                return HttpNotFound();
            }
            return View(item);
        }

        // GET: dvcntt/ReportSales/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: dvcntt/ReportSales/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "id,app_key,id_key,code_key,title,desc,quantity,quantity_total,price_old,price,images,url,author,attach,started_at,ended_at,created_by,created_at,updated_by,updated_at,flag,extras")] item item)
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

        // GET: dvcntt/ReportSales/Edit/5
        public async Task<ActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var item = await db.items.FindAsync(id);
            if (item == null)
            {
                return HttpNotFound();
            }
            return View(item);
        }

        // POST: dvcntt/ReportSales/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "id,app_key,id_key,code_key,title,desc,quantity,quantity_total,price_old,price,images,url,author,attach,started_at,ended_at,created_by,created_at,updated_by,updated_at,flag,extras")] item item)
        {
            if (ModelState.IsValid)
            {
                db.Entry(item).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(item);
        }

        // GET: dvcntt/ReportSales/Delete/5
        public async Task<ActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var item = await db.items.FindAsync(id);
            if (item == null)
            {
                return HttpNotFound();
            }
            return View(item);
        }

        // POST: dvcntt/ReportSales/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            var item = await db.items.FindAsync(id);
            db.items.Remove(item);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
