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

namespace Portal.Areas.tratruoc.Controllers
{
    [Filters.AuthVinaphone()]
    public class EloadPTTBController : Portal.Controllers.BaseController
    {
        // GET: ccbs/EloadPTTB
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

                var rs = (from d in db.eloadPttbs
                              //join l in db.locals on d.localID equals l.id
                          select new
                          {
                              id = d.id,
                              localID = d.localID,
                              stb = d.stb,
                              tentb = d.tentb,
                              ngaysinh = d.ngaysinh,
                              socmt = d.socmt,
                              ngaycap = d.ngaycap,
                              noicap = d.noicap,
                              diachi = d.diachi,
                              nguoidk = d.nguoidk,
                              ngaydk = d.ngaydk,
                              ngaysua = d.ngaysua,
                              diachidl = d.diachidl,
                              matinh = d.matinh,
                              khuvuc = d.khuvuc,
                              anhcmt = d.anhcmt,
                              desc = d.desc,
                              createdBy = d.createdBy,
                              createdAt = d.createdAt,
                              updatedBy = d.updatedBy,
                              updatedAt = d.updatedAt,
                              flag = d.flag,
                              donvi = db.locals.FirstOrDefault(l => l.id == d.localID).title
                          }).ToList().AsEnumerable();

                if (!String.IsNullOrEmpty(searchString))
                    rs = rs.Where(d =>
                    d.tentb.Contains(searchString) ||
                    d.noicap.Contains(searchString) ||
                    d.diachi.Contains(searchString));
                //d.donvi.Contains(searchString));

                if (!String.IsNullOrEmpty(searchString) && TM.Helper.Regex.isNumber(searchString))
                    rs = rs.ToList().Where(d =>
                    d.stb.ToString() == searchString ||
                    d.socmt.ToString() == searchString);

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
                    case "stb_asc":
                        rs = rs.OrderBy(d => d.stb);
                        break;
                    case "stb_desc":
                        rs = rs.OrderByDescending(d => d.stb);
                        break;
                    case "tentb_asc":
                        rs = rs.OrderBy(d => d.tentb);
                        break;
                    case "tentb_desc":
                        rs = rs.OrderByDescending(d => d.tentb);
                        break;
                    case "socmt_asc":
                        rs = rs.OrderBy(d => d.socmt);
                        break;
                    case "socmt_desc":
                        rs = rs.OrderByDescending(d => d.socmt);
                        break;
                    case "donvi_asc":
                        rs = rs.OrderBy(d => d.donvi);
                        break;
                    case "donvi_desc":
                        rs = rs.OrderByDescending(d => d.donvi);
                        break;
                    case "createdAt_asc":
                        rs = rs.OrderBy(d => d.createdAt);
                        break;
                    default:
                        rs = rs.OrderByDescending(d => d.createdAt);
                        break;
                }

                //Export to any
                if (!String.IsNullOrEmpty(export))
                {
                    TM.Exports.ExportExcel(TM.Helper.Data.ToDataTable(rs.ToList()), "Danh sách PTTB ELoad");
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

        // GET: ccbs/EloadPTTB/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var eloadPttb = db.eloadPttbs.Find(id);
            if (eloadPttb == null)
            {
                return HttpNotFound();
            }
            return View(eloadPttb);
        }

        // GET: ccbs/EloadPTTB/Create
        public ActionResult Create()
        {
            ViewBag.local = db.locals.Where(d => d.flag > 0).ToList();
            return View();
        }

        // POST: ccbs/EloadPTTB/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "localID,stb,tendaydu,socmt,noicap,diachi,nguoidk,diachidl,matinh,khuvuc,anhcmt,flag")] Models.eloadPttb eloadPttb, FormCollection collection)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    eloadPttb.id = Guid.NewGuid();
                    eloadPttb.ngaysinh = collection["ngaysinh"].StringToShortDatetime();
                    eloadPttb.ngaycap = collection["ngaycap"].StringToShortDatetime();
                    eloadPttb.ngaydk = collection["ngaydk"].DateTimeParseExactVNToEN();
                    eloadPttb.ngaysua = collection["ngaysua"].DateTimeParseExactVNToEN();
                    eloadPttb.createdBy = Authentication.Auth.AuthUser.id.ToString();
                    eloadPttb.createdAt = DateTime.Now;
                    eloadPttb.updatedBy = Authentication.Auth.AuthUser.id.ToString();
                    eloadPttb.updatedAt = eloadPttb.createdAt;
                    db.eloadPttbs.Add(eloadPttb);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                    this.danger(TM.Common.Language.msgCreateError);
            }
            catch (Exception ex)
            {
                this.danger(ex.Message);
            }
            ViewBag.local = db.locals.Where(d => d.flag > 0).ToList();
            return View(eloadPttb);
        }

        // GET: ccbs/EloadPTTB/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var eloadPttb = db.eloadPttbs.Find(id);
            if (eloadPttb == null)
            {
                return HttpNotFound();
            }
            ViewBag.local = db.locals.Where(d => d.flag > 0).ToList();
            return View(eloadPttb);
        }

        // POST: ccbs/EloadPTTB/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,localID,stb,tendaydu,socmt,noicap,diachi,nguoidk,diachidl,matinh,khuvuc,anhcmt,flag")] Models.eloadPttb eloadPttb_tmp, FormCollection collection)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var eloadPttb = db.eloadPttbs.Find(eloadPttb_tmp.id);
                    eloadPttb.localID = eloadPttb_tmp.localID;
                    eloadPttb.stb = eloadPttb_tmp.stb;
                    eloadPttb.tentb = eloadPttb_tmp.tentb;
                    eloadPttb.ngaysinh = collection["ngaysinh"].StringToShortDatetime();
                    eloadPttb.socmt = eloadPttb_tmp.socmt;
                    eloadPttb.ngaycap = collection["ngaycap"].StringToShortDatetime();
                    eloadPttb.noicap = eloadPttb_tmp.noicap;
                    eloadPttb.diachi = eloadPttb_tmp.diachi;
                    eloadPttb.nguoidk = eloadPttb_tmp.nguoidk;
                    eloadPttb.ngaydk = collection["ngaydk"].DateTimeParseExactVNToEN();
                    eloadPttb.ngaysua = collection["ngaysua"].DateTimeParseExactVNToEN();
                    eloadPttb.diachidl = eloadPttb_tmp.diachidl;
                    eloadPttb.matinh = eloadPttb_tmp.matinh;
                    eloadPttb.khuvuc = eloadPttb_tmp.khuvuc;
                    eloadPttb.anhcmt = eloadPttb_tmp.anhcmt;
                    eloadPttb.flag = eloadPttb_tmp.flag;
                    eloadPttb.updatedBy = Authentication.Auth.AuthUser.id.ToString();
                    eloadPttb.updatedAt = DateTime.Now;
                    db.Entry(eloadPttb).State = EntityState.Modified;
                    db.SaveChanges();
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
            return View(eloadPttb_tmp);
        }
        private bool CheckExist(long stb)
        {
            try
            {
                var user = db.eloadPttbs.Local.Any(d => d.stb == stb);
                if (!user) user = db.eloadPttbs.Any(d => d.stb == stb);
                return user;
            }
            catch (Exception) { throw; }
        }
        private Models.eloadPttb CheckExistPttb(long stb)
        {
            try
            {
                var user = db.eloadPttbs.Local.Where(d => d.stb == stb).FirstOrDefault();
                if (user == null) user = db.eloadPttbs.Where(d => d.stb == stb).FirstOrDefault();
                return user;
            }
            catch (Exception) { throw; }
        }
        private long getLocal(string eloadNumber)
        {
            var rs = db.eloadUsers.FirstOrDefault(d => d.eloadNumber.Contains(eloadNumber));
            if (rs == null) return 0;
            return rs.localID.Value;
        }
        public ActionResult Upload()
        {
            try
            {
                if (Request.Files.Count < 1)
                {
                    this.warning("Vui lòng Chọn tệp!");
                    return RedirectToAction("Create");
                }
                var file = TM.IO.FileDirectory.Upload(Request.Files, TM.Common.Directories.ccbs, false, new[] { ".xls", ".xlsx" }, 5);

                foreach (var item in file.UploadFile())
                {
                    var excel = new TM.Interop.Excel(TM.IO.FileDirectory.MapPath(TM.Common.Directories.ccbs + item));
                    var list = excel.ToList();
                    for (var i = 6; i < list.Count; i++)
                    {
                        var eloadPttb = new Models.eloadPttb();
                        eloadPttb.id = Guid.NewGuid();
                        eloadPttb.stb = long.Parse(list[i][2].ToString());
                        //
                        //if (CheckExist(eloadPttb.stb.Value)) continue;
                        var eloadPttbExist = CheckExistPttb(eloadPttb.stb.Value);
                        if (eloadPttbExist != null)
                        {
                            eloadPttbExist.tentb = list[i][3].ToString();
                            eloadPttbExist.ngaysinh = list[i][5].ToString().StringToShortDatetime();
                            eloadPttbExist.socmt = long.Parse(list[i][7].ToString());
                            eloadPttbExist.ngaycap = list[i][8].ToString().StringToShortDatetime();
                            eloadPttbExist.noicap = list[i][10].ToString();
                            eloadPttbExist.diachi = list[i][11].ToString();
                            eloadPttbExist.nguoidk = list[i][20].ToString();
                            eloadPttbExist.ngaydk = list[i][16].ToString().DateTimeParseExactVNToEN();
                            eloadPttbExist.ngaysua = list[i][18].ToString().DateTimeParseExactVNToEN();
                            eloadPttbExist.diachidl = list[i][21].ToString();
                            eloadPttbExist.matinh = list[i][23].ToString();
                            eloadPttbExist.khuvuc = Int32.Parse(list[i][24].ToString());
                            eloadPttbExist.anhcmt = list[i][25].ToString() == "Co" ? 1 : 0;
                            eloadPttbExist.localID = getLocal(eloadPttbExist.nguoidk);
                            eloadPttbExist.updatedBy = Authentication.Auth.AuthUser.id.ToString();
                            eloadPttbExist.updatedAt = DateTime.Now; ;
                            db.Entry(eloadPttbExist).State = EntityState.Modified;
                            db.SaveChanges();
                            continue;
                        }
                        //
                        eloadPttb.tentb = list[i][3].ToString();
                        eloadPttb.ngaysinh = list[i][5].ToString().StringToShortDatetime();
                        eloadPttb.socmt = long.Parse(list[i][7].ToString());
                        eloadPttb.ngaycap = list[i][8].ToString().StringToShortDatetime();
                        eloadPttb.noicap = list[i][10].ToString();
                        eloadPttb.diachi = list[i][11].ToString();
                        eloadPttb.nguoidk = list[i][20].ToString();
                        eloadPttb.ngaydk = list[i][16].ToString().DateTimeParseExactVNToEN();
                        eloadPttb.ngaysua = list[i][18].ToString().DateTimeParseExactVNToEN();
                        eloadPttb.diachidl = list[i][21].ToString();
                        eloadPttb.matinh = list[i][23].ToString();
                        eloadPttb.khuvuc = Int32.Parse(list[i][24].ToString());
                        eloadPttb.anhcmt = list[i][25].ToString() == "Co" ? 1 : 0;
                        eloadPttb.flag = 1;
                        eloadPttb.localID = getLocal(eloadPttb.nguoidk);
                        eloadPttb.createdBy = Authentication.Auth.AuthUser.id.ToString();
                        eloadPttb.createdAt = DateTime.Now;
                        eloadPttb.updatedBy = Authentication.Auth.AuthUser.id.ToString();
                        eloadPttb.updatedAt = eloadPttb.createdAt;
                        db.eloadPttbs.Add(eloadPttb);
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
                    var rs = db.eloadPttbs.Find(tmp);
                    rs.flag = flag = rs.flag == 1 ? 0 : 1;
                }
                db.SaveChanges();
                return Json(new { success = (flag == 0 ? TM.Common.Language.msgDeleteSucsess : TM.Common.Language.msgRecoverSucsess) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception) { return Json(new { danger = TM.Common.Language.msgError }, JsonRequestBehavior.AllowGet); }
        }
    }
}
