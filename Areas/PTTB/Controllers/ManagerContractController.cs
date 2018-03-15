using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using TM.Message;
using PagedList;
using Dapper;

namespace Portal.Areas.PTTB.Controllers
{
    public class ManagerContractController : Portal.Controllers.BaseController
    {
        const string _HNIHD = "HNIHD";
        [Filters.AuthVinaphone()]
        public ActionResult Index(string order, string currentFilter, string searchString, int? page, string datetime, int? datetimeType, int? localID, string export, int flag = 1)
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
                ViewBag.localID = localID;
                ViewBag.localList = db.groups.Where(d => d.app_key == Common.Objects.groups.department && d.level > 0).OrderBy(d => d.level).ToList();
                ViewBag.RequestUpdate = db.ManagerHD.Where(d => d.flag == 2).Count();

                var rs = db.ManagerHD.Where(d => d.flag == flag);

                if (Authentication.Auth.AuthUser.roles == Authentication.Roles.staff)
                {
                    var staff_id = Authentication.Auth.AuthUser.staff_id;
                    var local = db.groups.FirstOrDefault(m => m.id == staff_id);
                    rs = rs.Where(m => m.localID == local.level);
                }
                else
                {
                    if (localID > 0)
                        rs = rs.Where(m => m.localID == localID);
                }

                if (!String.IsNullOrEmpty(searchString))
                    rs = rs.Where(d =>
                    d.contractID.ToLower().Contains(searchString.ToLower()) ||
                    d.customerName.ToLower().Contains(searchString.ToLower()));

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

                //if (flag == 0) rs = rs.Where(d => d.flag == 0);
                //else rs = rs.Where(d => d.flag == 1);

                switch (order)
                {
                    case "contract_asc":
                        rs = rs.OrderBy(d => d.contractID);
                        break;
                    case "contract_desc":
                        rs = rs.OrderByDescending(d => d.contractID);
                        break;
                    case "name_asc":
                        rs = rs.OrderBy(d => d.customerName);
                        break;
                    case "name_desc":
                        rs = rs.OrderByDescending(d => d.customerName);
                        break;
                    case "local_asc":
                        rs = rs.OrderBy(d => d.localName);
                        break;
                    case "local_desc":
                        rs = rs.OrderByDescending(d => d.localName);
                        break;
                    case "createdBy_asc":
                        rs = rs.OrderBy(d => d.createdBy);
                        break;
                    case "createdBy_desc":
                        rs = rs.OrderByDescending(d => d.createdBy);
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
                    TM.Exports.ExportExcel(TM.Helper.Data.ToDataTable(rs.ToList()), "Danh sách hợp đồng");
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

        [Filters.AuthVinaphone()]
        public ActionResult Create()
        {
            ViewBag.local = db.groups.Where(d => d.app_key == Common.Objects.groups.department && d.level > 0).OrderBy(d => d.level).ToList();
            return View();
        }

        // POST: ManagerHD/ManagerHDs/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [Filters.AuthVinaphone(), HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create(Portal.Models.ManagerHD managerHD)
        {
            try
            {
                if (!checkHD(managerHD.contractID))
                {
                    this.danger("Hợp đồng đã tồn tại!");
                    return RedirectToAction("Create");
                }
                var Extension = ".pdf";
                var staff_id = Authentication.Auth.AuthUser.staff_id;
                var local = db.groups.FirstOrDefault(m => m.id == staff_id);
                if (System.Web.HttpContext.Current.Session[_HNIHD] == null)
                {
                    this.danger("Có lỗi xảy ra vui lòng thực hiện lại!");
                    return RedirectToAction("Create");
                }
                //
                var HNIHD = (List<Models.HD_THUEBAO_BKN>)System.Web.HttpContext.Current.Session[_HNIHD];
                if (HNIHD.Count < 1)
                {
                    this.danger("Có lỗi xảy ra vui lòng thực hiện lại!");
                    return RedirectToAction("Create");
                }
                managerHD.id = Guid.NewGuid();
                managerHD.app_key = Common.Objects.groups.datmoi;
                managerHD.localID = local.level.Value;
                managerHD.localName = local.title;
                managerHD.customerName = HNIHD[0].TEN_TB;
                managerHD.customerAddress = HNIHD[0].DIACHI_TB;
                managerHD.customerPhone = HNIHD[0].DIENTHOAI_LH;
                managerHD.accounts = ",";
                managerHD.accountNumber = 0;
                foreach (var i in HNIHD)
                {
                    managerHD.accounts += $"{i.MA_TB},";
                    managerHD.accountNumber++;
                }
                //
                if (Request.Files.Count < 1)
                {
                    this.danger("Vui lòng chọn file hợp đồng (.pdf)");
                    return RedirectToAction("Create");
                }
                else
                {
                    for (int i = 0; i < Request.Files.Count; i++)
                    {
                        var file = Request.Files[i];
                        if (file.ContentLength <= 0)
                        {
                            this.danger("Vui lòng chọn file hợp đồng (.pdf)");
                            return RedirectToAction("Create");
                        }
                        if (!file.FileName.IsExtension(Extension))
                        {
                            this.danger("Tệp phải định dạng .pdf");
                            return RedirectToAction("Create");
                        }
                        TM.IO.FileDirectory.CreateDirectory(TM.Common.Directories.Hopdong);
                        var fileName = $"{TM.Common.Directories.Hopdong}{managerHD.contractID.Replace("/", "").Replace("\\", "")}{Extension}";
                        file.SaveAs(TM.IO.FileDirectory.MapPath(fileName));
                        managerHD.attach = fileName;

                        //FileManagerController
                        Portal.Controllers.FileManagerController.InsertDirectory(TM.Common.Directories.Hopdong);
                        Portal.Controllers.FileManagerController.InsertFile(fileName);
                    }
                }
                managerHD.createdBy = Authentication.Auth.AuthUser.username;
                managerHD.createdAt = DateTime.Now;
                managerHD.flag = 1;
                db.ManagerHD.Add(managerHD);
                db.SaveChanges();
                this.success(TM.Common.Language.msgCreateSucsess);
                ViewBag.hd = managerHD;
                return RedirectToAction("Create");
            }
            catch (Exception ex)
            {
                this.danger(ex.Message);
            }
            ViewBag.hd = managerHD;
            ViewBag.local = db.groups.Where(m => m.app_key == Common.Objects.groups.department).ToList();
            return View(managerHD);
        }

        [Filters.AuthVinaphone()]
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var managerHD = new Models.ManagerHD();
            if (Authentication.Auth.AuthUser.roles == Authentication.Roles.staff)
            {
                var staff_id = Authentication.Auth.AuthUser.staff_id;
                var local = db.groups.FirstOrDefault(m => m.id == staff_id);
                managerHD = db.ManagerHD.FirstOrDefault(m => m.id == id && m.localID == local.level);
            }
            else
                managerHD = db.ManagerHD.Find(id);

            if (managerHD == null)
            {
                return HttpNotFound();
            }
            ViewBag.local = db.groups.Where(d => d.app_key == Common.Objects.groups.department && d.level > 0).OrderBy(d => d.level).ToList();
            ViewBag.listOld = db.ManagerHD.Where(d => d.contractID == managerHD.contractID).Where(d => d.flag == 3 || d.flag == 4).OrderByDescending(d => d.updatedAt).ToList();

            return View(managerHD);
        }

        [HttpPost, ValidateAntiForgeryToken, Filters.AuthVinaphone()]
        public JsonResult Edit(Guid id, string details, Models.ManagerHD managerHDUpdate)
        {
            var index = 0;
            var Extension = ".pdf";
            try
            {
                TM.IO.FileDirectory.CreateDirectory(TM.Common.Directories.Hopdong);
                var managerHD = db.ManagerHD.FirstOrDefault(d => d.id == id);
                if (Authentication.Auth.AuthUser.roles == Authentication.Roles.admin)
                {
                    managerHD.customerName = managerHDUpdate.customerName;
                    managerHD.customerAddress = managerHDUpdate.customerAddress;
                    managerHD.customerPhone = managerHDUpdate.customerPhone;
                    var acc = managerHDUpdate.accounts.Trim(',').Trim().Split(',');
                    managerHD.accounts = ",";
                    managerHD.accountNumber = 0;
                    foreach (var i in acc)
                    {
                        managerHD.accounts += $"{i.Trim()},";
                        managerHD.accountNumber++;
                    }
                    managerHD.accounts = managerHD.accounts.Substring(0, managerHD.accounts.Length - 2);

                    if (Request.Files.Count > 0)
                    {
                        if (Request.Files[0].ContentLength > 0)
                        {
                            var file = Request.Files[0];
                            if (!file.FileName.IsExtension(Extension))
                            {
                                return Json(new { danger = "Tệp phải định dạng .pdf" }, JsonRequestBehavior.AllowGet);
                            }
                            var fileName = $"{TM.Common.Directories.Hopdong}{managerHD.contractID.Replace("/", "").Replace("\\", "")}_{managerHD.id}{Extension}";
                            file.SaveAs(TM.IO.FileDirectory.MapPath(fileName));
                            managerHD.attach = fileName;
                        }
                    }
                    db.Entry(managerHD).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
                else  //Gửi yc cập nhật
                {
                    if (Request.Files.Count < 1)
                        return Json(new { danger = "Vui lòng chọn file hợp đồng (.pdf)" }, JsonRequestBehavior.AllowGet);
                    else
                    {
                        var managerHDNew = new Models.ManagerHD();
                        managerHDNew.id = Guid.NewGuid();
                        managerHDNew.app_key = managerHD.app_key;
                        managerHDNew.localID = managerHD.localID;
                        managerHDNew.localName = managerHD.localName;
                        managerHDNew.contractID = managerHD.contractID;
                        managerHDNew.customerName = managerHD.customerName;
                        managerHDNew.customerAddress = managerHD.customerAddress;
                        managerHDNew.customerPhone = managerHD.customerPhone;
                        managerHDNew.accounts = managerHD.accounts;
                        managerHDNew.accountNumber = managerHD.accountNumber;
                        managerHDNew.createdBy = managerHD.createdBy;
                        managerHDNew.createdAt = managerHD.createdAt;
                        managerHDNew.updatedBy = Authentication.Auth.AuthUser.username;
                        managerHDNew.updatedAt = DateTime.Now;
                        managerHDNew.details = details;
                        managerHDNew.flag = 2;
                        //
                        for (int i = 0; i < Request.Files.Count; i++)
                        {
                            var file = Request.Files[i];
                            if (file.ContentLength <= 0)
                            {
                                return Json(new { danger = "Vui lòng chọn file hợp đồng (.pdf)" }, JsonRequestBehavior.AllowGet);
                            }
                            if (!file.FileName.IsExtension(Extension))
                            {
                                return Json(new { danger = "Tệp phải định dạng .pdf" }, JsonRequestBehavior.AllowGet);
                            }

                            var fileName = $"{TM.Common.Directories.Hopdong}{managerHD.contractID.Replace("/", "").Replace("\\", "")}_{managerHD.id}{Extension}";
                            file.SaveAs(TM.IO.FileDirectory.MapPath(fileName));
                            managerHDNew.attach += fileName;
                            //FileManagerController
                            Portal.Controllers.FileManagerController.InsertDirectory(TM.Common.Directories.Hopdong);
                            Portal.Controllers.FileManagerController.InsertFile(fileName);
                        }
                        //
                        db.ManagerHD.Add(managerHDNew);
                        db.SaveChanges();
                    }
                }
                return Json(new { success = "Gửi yêu cầu cập nhật thành công!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
        }
        [Filters.AuthVinaphone()]
        [HttpGet]
        public JsonResult RequestUpdate()
        {
            var index = 0;
            try
            {
                var rs = new List<Models.ManagerHD>();
                var flag = false;
                if (Authentication.Auth.AuthUser.roles == Authentication.Roles.staff)
                {
                    var staff_id = Authentication.Auth.AuthUser.staff_id;
                    var local = db.groups.FirstOrDefault(d => d.id == staff_id);
                    rs = db.ManagerHD.Where(d => d.localID == local.level && d.flag == 2).OrderByDescending(d => d.updatedAt).ToList();
                }
                else
                {
                    rs = db.ManagerHD.Where(d => d.flag == 2).OrderByDescending(d => d.updatedAt).ToList();
                    flag = true;
                }
                return Json(new { data = rs, flag = flag, success = "Lấy danh sách yêu cầu cập nhật thành công!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
        }
        [Filters.AuthVinaphone(Role = Authentication.Roles.superadmin + "," + Authentication.Roles.admin)]
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult RequestUpdateAccept(Guid id, string cfmNotes)
        {
            var index = 0;
            try
            {
                var rs = db.ManagerHD.Find(id);
                if (rs == null)
                    return Json(new { danger = "Lỗi không tìm thấy hợp đồng, vui lòng thử lại!" }, JsonRequestBehavior.AllowGet);
                var old = db.ManagerHD.Where(d => d.contractID == rs.contractID && d.flag == 1).ToList();
                var details_tmp = "";
                foreach (var item in old)
                {
                    item.updatedBy = rs.updatedBy;
                    item.updatedAt = rs.updatedAt;
                    item.cfmBy = Authentication.Auth.AuthUser.username;
                    item.cfmAt = DateTime.Now;
                    item.cfmNotes = cfmNotes;
                    item.flag = 3;
                    details_tmp = item.details;
                    item.details = rs.details;
                }
                rs.details = details_tmp;
                rs.flag = 1;
                rs.cfmBy = Authentication.Auth.AuthUser.username;
                rs.cfmAt = DateTime.Now;
                db.SaveChanges();
                var countData = db.ManagerHD.Where(d => d.flag == 2);
                var tmp = countData != null ? countData.Count() : 0;
                return Json(new { data = tmp, success = "Xác nhận yêu cầu thành công!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
        }
        [Filters.AuthVinaphone(Role = Authentication.Roles.superadmin + "," + Authentication.Roles.admin)]
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult RequestUpdateReject(Guid id, string cfmNotes)
        {
            var index = 0;
            try
            {
                var rs = db.ManagerHD.Find(id);
                if (rs == null)
                    return Json(new { danger = "Lỗi không tìm thấy hợp đồng, vui lòng thử lại!" }, JsonRequestBehavior.AllowGet);
                rs.flag = 4;
                rs.cfmNotes = cfmNotes;
                rs.cfmBy = Authentication.Auth.AuthUser.username;
                rs.cfmAt = DateTime.Now;
                db.SaveChanges();
                var countData = db.ManagerHD.Where(d => d.flag == 2);
                var tmp = countData != null ? countData.Count() : 0;
                return Json(new { data = tmp, success = "Hủy yêu cầu thành công!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message + " - Index: " + index }, JsonRequestBehavior.AllowGet); }
        }
        private bool checkHD(string contractID)
        {
            try
            {
                if (!db.ManagerHD.Any(m => m.contractID == contractID))
                    return true;
            }
            catch (Exception)
            {

                throw;
            }
            return false;
        }
        [HttpGet]
        public JsonResult getDataHD(string so_hd)
        {
            try
            {
                var Oracle = new TM.Connection.Oracle("ORCHNIVNPTBACKAN1");
                if (!checkHD(so_hd))
                    return Json(new { danger = "Hợp đồng đã tồn tại!" }, JsonRequestBehavior.AllowGet);
                var data = Oracle.Connection.Query<Models.HD_THUEBAO_BKN>($"SELECT * FROM HD_THUEBAO_BKN WHERE so_hd LIKE '{so_hd}%' ORDER BY LOAIHINHTB_ID").ToList();
                System.Web.HttpContext.Current.Session[_HNIHD] = data;
                return Json(new { data = data, success = TM.Common.Language.msgSucsess }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message }, JsonRequestBehavior.AllowGet); }
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
                    var rs = db.ManagerHD.Find(tmp);
                    rs.flag = flag = rs.flag == 1 ? 0 : 1;
                }
                db.SaveChanges();
                return Json(new { success = (flag == 0 ? TM.Common.Language.msgDeleteSucsess : TM.Common.Language.msgRecoverSucsess) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception) { return Json(new { danger = TM.Common.Language.msgError }, JsonRequestBehavior.AllowGet); }
        }
    }
}