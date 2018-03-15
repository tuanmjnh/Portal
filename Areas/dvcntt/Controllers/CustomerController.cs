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
    public class CustomerController : Portal.Controllers.BaseController
    {
        // GET: dvcntt/CA
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

                var rs = db.Customers.Where(m => m.flag > 0);

                if (!String.IsNullOrEmpty(searchString))
                    rs = rs.Where(d =>
                    d.name.Contains(searchString) ||
                    d.author.Contains(searchString) ||
                    d.code.Contains(searchString));

                if (!String.IsNullOrEmpty(datetime))
                {
                    var date = datetime.Split('-');
                    if (date.Length > 1)
                    {
                        var dateStart = TM.Format.Formating.StartOfDate(TM.Format.Formating.DateParseExactVNToEN(date[0]));
                        var dateEnd = TM.Format.Formating.EndOfDate(TM.Format.Formating.DateParseExactVNToEN(date[1]));
                        rs = datetimeType == 0 ? rs.Where(d => d.createdAt >= dateStart && d.createdAt <= dateEnd) : rs.Where(d => d.updatedAt >= dateStart && d.updatedAt <= dateEnd);
                    }
                }

                if (flag == 0) rs = rs.Where(d => d.flag == 0);
                else rs = rs.Where(d => d.flag > 0);

                switch (order)
                {
                    case "name_asc":
                        rs = rs.OrderBy(d => d.name);
                        break;
                    case "name_desc":
                        rs = rs.OrderByDescending(d => d.name);
                        break;
                    case "author_asc":
                        rs = rs.OrderBy(d => d.author);
                        break;
                    case "author_desc":
                        rs = rs.OrderByDescending(d => d.author);
                        break;
                    case "code_asc":
                        rs = rs.OrderBy(d => d.code);
                        break;
                    case "code_desc":
                        rs = rs.OrderByDescending(d => d.code);
                        break;
                    case "created_asc":
                        rs = rs.OrderBy(d => d.createdAt);
                        break;
                    default:
                        rs = rs.OrderByDescending(d => d.createdAt);
                        break;
                }
                //Export to any
                if (!String.IsNullOrEmpty(export))
                {
                    TM.Exports.ExportExcel(TM.Helper.Data.ToDataTable(rs.ToList()), "Danh sách khách hàng");
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
            return View();
        }

        // POST: dvcntt/CA/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Customer customer, FormCollection collection)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    customer.app_key = Common.Objects.groups.dvcntt;
                    customer.author = $",{customer.author},{collection["authorRole"]},";
                    customer.phone = $",{collection["mobile"]},{customer.phone},";
                    customer.createdBy = Authentication.Auth.AuthUser.id.ToString();
                    customer.createdAt = DateTime.Now;
                    customer.updatedBy = Authentication.Auth.AuthUser.id.ToString();
                    customer.updatedAt = DateTime.Now;
                    db.Customers.Add(customer);
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
            return View(customer);
        }

        // GET: dvcntt/CA/Edit/5
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var item = db.Customers.Find(id);
            if (item == null)
            {
                return HttpNotFound();
            }
            //ViewBag.services = getGroups(Common.Objects.groups.ca);
            //ViewBag.local = db.locals.Where(d => d.flag > 0).ToList();
            //ViewBag.users = db.users.Where(d => d.flag > 0 && d.roles == Authentication.Roles.staff).OrderBy(d => d.staff_id).ThenBy(d => d.full_name).ToList();
            return View(item);
        }

        // POST: dvcntt/CA/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Customer customer, FormCollection collection)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    customer.updatedBy = Authentication.Auth.AuthUser.id.ToString();
                    customer.updatedAt = DateTime.Now;
                    customer.author = $",{customer.author},{collection["authorRole"]},";
                    customer.phone = $",{collection["mobile"]},{customer.phone},";
                    db.Customers.Attach(customer);
                    var entry = db.Entry(customer);
                    entry.Property(m => m.name).IsModified = true;
                    entry.Property(m => m.author).IsModified = true;
                    entry.Property(m => m.code).IsModified = true;
                    entry.Property(m => m.phone).IsModified = true;
                    entry.Property(m => m.email).IsModified = true;
                    entry.Property(m => m.address).IsModified = true;
                    entry.Property(m => m.details).IsModified = true;
                    entry.Property(m => m.updatedBy).IsModified = true;
                    entry.Property(m => m.updatedAt).IsModified = true;
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
            return View(customer);
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
