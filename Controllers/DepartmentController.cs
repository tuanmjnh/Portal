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

namespace Portal.Controllers
{
    [Filters.AuthVinaphone()]
    public class DepartmentController : BaseController
    {
        // GET: Department
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

                var rs = from d in db.groups where d.app_key == Common.Objects.groups.department select d;

                if (!String.IsNullOrEmpty(searchString))
                    rs = rs.Where(d =>
                    d.title.Contains(searchString));

                if (flag == 0) rs = rs.Where(d => d.flag == 0);
                else rs = rs.Where(d => d.flag == 1);

                switch (order)
                {
                    case "key_asc":
                        rs = rs.OrderBy(d => d.extras);
                        break;
                    case "key_desc":
                        rs = rs.OrderByDescending(d => d.extras);
                        break;
                    case "level_asc":
                        rs = rs.OrderBy(d => d.level);
                        break;
                    case "level_desc":
                        rs = rs.OrderByDescending(d => d.level);
                        break;
                    case "title_desc":
                        rs = rs.OrderByDescending(d => d.title);
                        break;
                    default:
                        rs = rs.OrderBy(d => d.orders).ThenByDescending(d => d.created_at);
                        break;
                }

                //Export to any
                if (!String.IsNullOrEmpty(export))
                {
                    TM.Exports.ExportExcel(TM.Helper.Data.ToDataTable(rs.ToList()), "Danh sách phòng ban");
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

        // GET: Department/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Models.group group = db.groups.Find(id);
            if (group == null)
            {
                return HttpNotFound();
            }
            return View(group);
        }

        // GET: Department/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Department/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id_key,title,level,desc,orders,extras,flag")] Models.group group)
        {
            if (ModelState.IsValid)
            {
                group.id = Guid.NewGuid();
                group.app_key = Common.Objects.groups.department;
                group.created_by = Authentication.Auth.AuthUser.id.ToString();
                group.created_at = DateTime.Now;
                db.groups.Add(group);
                db.SaveChanges();
                this.success(TM.Common.Language.msgCreateSucsess);
                return RedirectToAction("Create");
            }
            else
                this.danger(TM.Common.Language.msgCreateError);
            return View(group);
        }

        // GET: Department/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Models.group group = db.groups.Find(id);
            if (group == null)
            {
                return HttpNotFound();
            }
            return View(group);
        }

        // POST: Department/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,id_key,title,level,desc,orders,extras,flag")] Models.group group_tmp)
        {
            if (ModelState.IsValid)
            {
                var group = db.groups.Find(group_tmp.id);
                group.id_key = group_tmp.id_key;
                group.title = group_tmp.title;
                group.level = group_tmp.level;
                group.desc = group_tmp.desc;
                group.orders = group_tmp.orders;
                group.extras = group_tmp.extras;
                group.flag = group_tmp.flag;
                group.updated_by = Authentication.Auth.AuthUser.id.ToString();
                group.updated_at = DateTime.Now;
                db.Entry(group).State = EntityState.Modified;
                db.SaveChanges();
                this.success(TM.Common.Language.msgUpdateSucsess);
                return RedirectToAction("Index");
            }
            else
                this.danger(TM.Common.Language.msgUpdateError);
            return View(group_tmp);
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
                    var tmp = Guid.Parse(item);
                    var rs = db.groups.Find(tmp);
                    rs.flag = flag = rs.flag == 1 ? 0 : 1;
                }
                db.SaveChanges();
                return Json(new { success = (flag == 0 ? TM.Common.Language.msgDeleteSucsess : TM.Common.Language.msgRecoverSucsess) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception) { return Json(new { danger = TM.Common.Language.msgError }, JsonRequestBehavior.AllowGet); }
        }
    }
}
