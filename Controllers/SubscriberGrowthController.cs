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
    public class SubscriberGrowthController : BaseController
    {
        // GET: SubscriberGrowth
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

                var rs = from d in db.subscriber_growth select d;

                if (!String.IsNullOrEmpty(searchString))
                    rs = rs.Where(d =>
                    d.contract_id.Contains(searchString) ||
                    d.account.Contains(searchString) ||
                    d.technician.Contains(searchString) ||
                    d.collaborator.Contains(searchString) ||
                    d.customer_name.Contains(searchString) ||
                    d.customer_address.Contains(searchString));

                if (flag == 0) rs = rs.Where(d => d.flag == 0);
                else rs = rs.Where(d => d.flag == 1);

                switch (order)
                {
                    case "customerName_desc":
                        rs = rs.OrderByDescending(d => d.customer_name);
                        break;
                    case "customerName_asc":
                        rs = rs.OrderBy(d => d.customer_name);
                        break;
                    case "contract_desc":
                        rs = rs.OrderByDescending(d => d.contract_id);
                        break;
                    case "contract_asc":
                        rs = rs.OrderBy(d => d.contract_id);
                        break;
                    case "services_desc":
                        rs = rs.OrderByDescending(d => d.services_id);
                        break;
                    case "services_asc":
                        rs = rs.OrderBy(d => d.services_id);
                        break;
                    case "account_desc":
                        rs = rs.OrderByDescending(d => d.account);
                        break;
                    case "account_asc":
                        rs = rs.OrderBy(d => d.account);
                        break;
                    case "time_asc":
                        rs = rs.OrderBy(d => d.created_at);
                        break;
                    default:
                        rs = rs.OrderByDescending(d => d.created_at);
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
            //return View(db.subscriber_growth.ToList());
        }

        // GET: SubscriberGrowth/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            subscriber_growth subscriber_growth = db.subscriber_growth.Find(id);
            if (subscriber_growth == null)
            {
                return HttpNotFound();
            }
            return View(subscriber_growth);
        }

        // GET: SubscriberGrowth/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: SubscriberGrowth/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "contract_id,services_id,account,technician,collaborator,package,customer_name,customer_address,phone,created_by,created_at,updated_by,updated_at,flag")] subscriber_growth subscriber_growth)
        {
            if (ModelState.IsValid)
            {
                subscriber_growth.id = Guid.NewGuid();
                db.subscriber_growth.Add(subscriber_growth);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(subscriber_growth);
        }

        // GET: SubscriberGrowth/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            subscriber_growth subscriber_growth = db.subscriber_growth.Find(id);
            if (subscriber_growth == null)
            {
                return HttpNotFound();
            }
            return View(subscriber_growth);
        }

        // POST: SubscriberGrowth/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,contract_id,services_id,account,technician,collaborator,package,customer_name,customer_address,phone,created_by,created_at,updated_by,updated_at,flag")] subscriber_growth subscriber_growth)
        {
            if (ModelState.IsValid)
            {
                db.Entry(subscriber_growth).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(subscriber_growth);
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
        //            Guid tmp = Guid.Parse(item);
        //            var rs = db.subscriber_growth.Find(tmp);
        //            db.subscriber_growth.Remove(rs);
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
                    Guid tmp = Guid.Parse(item);
                    var rs = db.subscriber_growth.Find(tmp);
                    rs.flag = flag = rs.flag == 1 ? 0 : 1;
                }
                db.SaveChanges();
                return Json(new { success = (flag == 0 ? TM.Common.Language.msgDeleteSucsess : TM.Common.Language.msgRecoverSucsess) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception) { return Json(new { danger = TM.Common.Language.msgError }, JsonRequestBehavior.AllowGet); }
        }
    }
}
