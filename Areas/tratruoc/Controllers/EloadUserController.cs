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
using ex = Microsoft.Office.Interop.Excel;

namespace Portal.Areas.tratruoc.Controllers
{
    [Filters.AuthVinaphone()]
    public class EloadUserController : Portal.Controllers.BaseController
    {
        // GET: ccbs/EloadUser
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

                var rs = (from d in db.eloadUsers
                          join l in db.locals on d.localID equals l.id
                          select new
                          {
                              id = d.id,
                              localID = d.localID,
                              users = d.eloadNumber,
                              fullName = d.fullName,
                              startedAt = d.startedAt,
                              endedAt = d.endedAt,
                              createdBy = d.createdBy,
                              createdAt = d.createdAt,
                              updatedBy = d.updatedBy,
                              updatedAt = d.updatedAt,
                              isCCBS = d.isCCBS,
                              isLock = d.isLock,
                              flag = d.flag,
                              donvi = l.title
                          }).ToList().AsEnumerable();

                if (!String.IsNullOrEmpty(searchString))
                    rs = rs.Where(d =>
                    d.users.Contains(searchString) ||
                    d.fullName.Contains(searchString) ||
                    d.donvi.Contains(searchString));

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
                    case "fullName_asc":
                        rs = rs.OrderBy(d => d.fullName);
                        break;
                    case "fullName_desc":
                        rs = rs.OrderByDescending(d => d.fullName);
                        break;
                    case "startedAt_asc":
                        rs = rs.OrderBy(d => d.startedAt);
                        break;
                    case "startedAt_desc":
                        rs = rs.OrderByDescending(d => d.startedAt);
                        break;
                    case "createdAt_asc":
                        rs = rs.OrderBy(d => d.createdAt);
                        break;
                    case "createdAt_desc":
                        rs = rs.OrderByDescending(d => d.createdAt);
                        break;
                    case "donvi_asc":
                        rs = rs.OrderBy(d => d.donvi);
                        break;
                    case "donvi_desc":
                        rs = rs.OrderByDescending(d => d.donvi);
                        break;
                    case "users_desc":
                        rs = rs.OrderByDescending(d => d.users);
                        break;
                    default:
                        rs = rs.OrderBy(d => d.users);
                        break;
                }

                //Export to any
                if (!String.IsNullOrEmpty(export))
                {
                    TM.Exports.ExportExcel(TM.Helper.Data.ToDataTable(rs.ToList()), "Danh sách tài khoản ELoad");
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

        // GET: ccbs/EloadUser/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var eloadUser = db.eloadUsers.Find(id);
            if (eloadUser == null)
            {
                return HttpNotFound();
            }
            return View(eloadUser);
        }

        // GET: ccbs/EloadUser/Create
        public ActionResult Create()
        {
            ViewBag.local = db.locals.Where(d => d.flag > 0).ToList();
            ViewBag.department = db.groups.Where(d => d.flag > 0 && d.app_key == Common.Objects.groups.department).ToList();
            return View();
        }

        // POST: ccbs/EloadUser/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "localID,users,fullName,startedAt,endedAt,desc,isLock,flag")] Models.eloadUser eloadUser)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (eloadUser.eloadNumber == "Không xác định")
                        return RedirectToAction("Create");
                    if (CheckExist(eloadUser.eloadNumber))
                        return RedirectToAction("Create");
                    eloadUser.id = Guid.NewGuid();
                    eloadUser.createdBy = Authentication.Auth.AuthUser.id.ToString();
                    eloadUser.createdAt = DateTime.Now;
                    eloadUser.updatedBy = Authentication.Auth.AuthUser.id.ToString();
                    eloadUser.updatedAt = eloadUser.createdAt;
                    eloadUser.isCCBS = TM.Helper.Regex.isNumber(eloadUser.eloadNumber) ? 0 : 1;
                    db.eloadUsers.Add(eloadUser);
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
            ViewBag.local = db.locals.Where(d => d.flag > 0).ToList();
            return View(eloadUser);
        }

        // GET: ccbs/EloadUser/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var eloadUser = db.eloadUsers.Find(id);
            if (eloadUser == null)
            {
                return HttpNotFound();
            }
            ViewBag.local = db.locals.Where(d => d.flag > 0).ToList();
            return View(eloadUser);
        }

        // POST: ccbs/EloadUser/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,localID,users,fullName,desc,isLock,flag")] Models.eloadUser eloadUser_tmp, FormCollection collection)
        {
            try
            {
                //var errors = ModelState.Select(x => x.Value.Errors)
                //           .Where(y => y.Count > 0)
                //           .ToList();
                if (ModelState.IsValid)
                {
                    var eloadUser = db.eloadUsers.Find(eloadUser_tmp.id);
                    eloadUser.localID = eloadUser_tmp.localID;
                    eloadUser.eloadNumber = eloadUser_tmp.eloadNumber;
                    eloadUser.fullName = eloadUser_tmp.fullName;
                    eloadUser.startedAt = collection["startedAt"].StringToDatetime();
                    eloadUser.endedAt = collection["endedAt"].StringToDatetime();
                    eloadUser.desc = eloadUser_tmp.desc;
                    eloadUser.isLock = eloadUser_tmp.isLock;
                    eloadUser.flag = eloadUser_tmp.flag;
                    eloadUser.updatedBy = Authentication.Auth.AuthUser.id.ToString();
                    eloadUser.updatedAt = DateTime.Now;
                    eloadUser.isCCBS = TM.Helper.Regex.isNumber(eloadUser.eloadNumber) ? 0 : 1;
                    db.Entry(eloadUser).State = EntityState.Modified;
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
            ViewBag.local = db.locals.Where(d => d.flag > 0).ToList();
            return View(eloadUser_tmp);
        }

        public ActionResult Upload()
        {
            try
            {
                if (Request.Files.Count < 1 || (Request.Files.Count > 0 && Request.Files[0].ContentLength < 1))
                {
                    this.warning("Vui lòng Chọn tệp!");
                    return RedirectToAction("Create");
                }
                var file = TM.IO.FileDirectory.Upload(Request.Files, TM.Common.Directories.ccbs, false, new[] { ".xls", ".xlsx" }, 5);
                foreach (var item in file.UploadFile())
                {
                    var excel = new TM.Interop.Excel(TM.IO.FileDirectory.MapPath(TM.Common.Directories.ccbs + item));
                    var list = excel.ToList();
                    for (var i = 0; i < list.Count; i++)
                    {
                        if (i == 0) continue;
                        if (list[i][5].ToString() == "Không xác định") continue;
                        //if (CheckExist(list[i][5].ToString())) continue;
                        var eloadUserExist = CheckExistUser(list[i][5].ToString());
                        if (eloadUserExist != null)
                        {
                            eloadUserExist.fullName = list[i][6].ToString();
                            eloadUserExist.updatedBy = Authentication.Auth.AuthUser.id.ToString();
                            eloadUserExist.updatedAt = DateTime.Now;
                            eloadUserExist.localID = getLocal(list[i][7].ToString());
                            db.Entry(eloadUserExist).State = EntityState.Modified;
                            db.SaveChanges();
                            continue;
                        }
                        var eloadUser = new Models.eloadUser();
                        eloadUser.id = Guid.NewGuid();
                        eloadUser.eloadNumber = list[i][5].ToString();
                        eloadUser.fullName = list[i][6].ToString();
                        eloadUser.createdBy = Authentication.Auth.AuthUser.id.ToString();
                        eloadUser.createdAt = DateTime.Now;
                        eloadUser.updatedBy = Authentication.Auth.AuthUser.id.ToString();
                        eloadUser.updatedAt = eloadUser.createdAt;
                        eloadUser.isCCBS = TM.Helper.Regex.isNumber(eloadUser.eloadNumber) ? 0 : 1;
                        eloadUser.isLock = 0;
                        eloadUser.flag = 1;
                        eloadUser.localID = getLocal(list[i][7].ToString());
                        db.eloadUsers.Add(eloadUser);
                        if (i % 100 == 0) db.SaveChanges();
                    }
                }
                db.SaveChanges();
                this.success("Cập nhật thành công!");
            }
            catch (Exception ex)
            {
                this.danger(ex.Message);
            }
            return RedirectToAction("Create");

            //try
            //{
            //    //var list = TM.Excel.ToList(TM.IO.FileDirectory.MapPath(TM.Common.Directories.ccbs + "11.xls"));
            //    var excel = new TM.Interop.Excel(TM.IO.FileDirectory.MapPath(TM.Common.Directories.ccbs + "11.xls"));
            //    var list = excel.ToList();
            //}
            //catch (Exception ex)
            //{
            //    this.danger(ex.Message);
            //}
            //return RedirectToAction("Create");
        }
        private bool CheckExist(string users)
        {
            try
            {
                var user = db.eloadUsers.Local.Any(d => d.eloadNumber.Contains(users));
                if (!user) user = db.eloadUsers.Any(d => d.eloadNumber.Contains(users));
                return user;
            }
            catch (Exception) { throw; }
        }
        private Models.eloadUser CheckExistUser(string users)
        {
            try
            {
                var user = db.eloadUsers.Local.Where(d => d.eloadNumber.Contains(users)).FirstOrDefault();
                if (user == null) user = db.eloadUsers.Where(d => d.eloadNumber.Contains(users)).FirstOrDefault();
                return user;
            }
            catch (Exception) { throw; }
        }
        private long getLocal(string localTitle)
        {
            localTitle = localTitle == "Thành phố" ? "Bắc Kạn" : localTitle;
            var local = db.locals.FirstOrDefault(d => d.title.Contains(localTitle));
            return local.id;
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
                    var rs = db.eloadUsers.Find(tmp);
                    rs.flag = flag = rs.flag == 1 ? 0 : 1;
                }
                db.SaveChanges();
                return Json(new { success = (flag == 0 ? TM.Common.Language.msgDeleteSucsess : TM.Common.Language.msgRecoverSucsess) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception) { return Json(new { danger = TM.Common.Language.msgError }, JsonRequestBehavior.AllowGet); }
        }
        [HttpGet]
        public JsonResult getUserFromLocal(Guid id)
        {
            try
            {
                var users = db.users.Where(m => m.staff_id == id).ToList();
                return Json(new { data = users, success = "Lấy dữ liệu thành công" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception) { return Json(new { danger = TM.Common.Language.msgError }, JsonRequestBehavior.AllowGet); }
        }
    }
}
