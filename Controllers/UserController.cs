using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TM.Message;
using PagedList;
using Dapper;

namespace Portal.Controllers
{
    [Filters.AuthVinaphone(Role = Authentication.Roles.superadmin + "," + Authentication.Roles.admin)]
    public class UserController : BaseController
    {
        // GET: User
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

                ViewBag.currentFilter = searchString;
                ViewBag.flag = flag;

                Guid id = Authentication.Auth.AuthUser.id;
                var rs = from u in db.users
                         select new
                         {
                             u.id,
                             u.username,
                             u.password,
                             u.salt,
                             fullname = u.full_name,
                             u.mobile,
                             u.email,
                             u.address,
                             u.roles,
                             createdby = u.created_by,
                             createdat = u.created_at,
                             updatedby = u.updated_by,
                             updatedat = u.updated_at,
                             lastlogin = u.last_login,
                             staffid = u.staff_id,
                             u.flag,
                             u.extras,
                             staffname = db.groups.Where(m => m.id == u.staff_id).FirstOrDefault().title
                         };

                if (flag == 0) rs = rs.Where(s => s.flag == 0);
                else rs = rs.Where(s => s.flag == 1);

                if (!String.IsNullOrWhiteSpace(searchString))
                    rs = rs.Where(d => d.username.Contains(searchString) || d.fullname.Contains(searchString));

                if (Authentication.Auth.AuthUser.roles == Authentication.Roles.director)
                    rs = rs.Where(u =>
                    u.roles == Authentication.Roles.mod ||
                    u.roles == Authentication.Roles.manager ||
                    u.roles == Authentication.Roles.leader ||
                    u.roles == Authentication.Roles.staff);
                else if (Authentication.Auth.AuthUser.roles == Authentication.Roles.manager)
                    rs = rs.Where(u =>
                    u.roles == Authentication.Roles.leader ||
                    u.roles == Authentication.Roles.staff);
                else if (Authentication.Auth.AuthUser.roles == Authentication.Roles.leader)
                    rs = rs.Where(u => u.roles == Authentication.Roles.staff);
                else if (Authentication.Auth.AuthUser.roles == Authentication.Roles.mod || Authentication.Auth.AuthUser.roles == Authentication.Roles.staff)
                    rs = null;

                rs = rs.OrderByDescending(d => d.staffid).ThenBy(d => d.roles);

                switch (order)
                {
                    case "fullname_asc":
                        rs = rs.OrderBy(d => d.fullname);
                        break;
                    case "fullname_desc":
                        rs = rs.OrderByDescending(d => d.fullname);
                        break;
                    case "username_asc":
                        rs = rs.OrderBy(d => d.username);
                        break;
                    case "username_desc":
                        rs = rs.OrderByDescending(d => d.username);
                        break;
                    case "staffname_desc":
                        rs = rs.OrderByDescending(d => d.staffname);
                        break;
                    default:
                        rs = rs.OrderBy(d => d.staffname);
                        break;
                }

                //Export to any
                if (!String.IsNullOrEmpty(export))
                {
                    TM.Exports.ExportExcel(TM.Helper.Data.ToDataTable(rs.ToList()), "Danh sách tài khoản");
                    return RedirectToAction("Index");
                }

                ViewBag.TotalRecords = rs.Count();
                int pageSize = 15;
                int pageNumber = (page ?? 1);
                return View(rs.AsEnumerable().Select(u => u.ToExpando()).ToPagedList(pageNumber, pageSize));
            }
            catch (Exception)
            {
                this.danger("Lỗi không tìm thấy dữ liệu. Vui lòng thử lại!");
            }
            return View();
        }
        public ActionResult Details(Guid id)
        {
            return View(db.users.SingleOrDefault(u => u.id == id));
        }

        public ActionResult Create()
        {
            ViewBag.Department = getGroups(Common.Objects.groups.department);
            return View();
        }
        [HttpPost]
        public ActionResult Create([Bind(Include = "username,password,full_name,mobile,email,address,roles,staff_id,flag")] Authentication.user user) //
        {
            if (ModelState.IsValid)
            {
                if (!db.users.Any(u => u.username.ToLower() == user.username.ToLower()))
                {
                    user.id = Guid.NewGuid();
                    user.salt = Guid.NewGuid().ToString();
                    user.password = TM.Encrypt.CryptoMD5TM(user.password + user.salt);
                    //user.roles = Authentication.Roles.mod;
                    //user.repeatPassword = user.password;
                    user.created_by = Authentication.Auth.AuthUser.id.ToString();
                    user.created_at = DateTime.Now;
                    db.users.Add(user);
                    db.SaveChanges();
                    ModelState.Clear();
                    this.success("Tạo mới tài khoản thành công");
                }
                else
                    this.danger("Tài khoản đã tồn tại");
            }
            else
                this.danger(TM.Common.Language.msgCreateError);
            return RedirectToAction("Create");
        }

        [AllowAnonymous]
        public JsonResult check_exist_cname(string username)
        {
            return Json(!db.users.Any(u => u.username.ToLower() == username.ToLower()), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult ResetPassword(Guid id)
        {
            try
            {
                var rs = db.users.Find(id);
                var newpass = Guid.NewGuid().ToString().Split('-');
                newpass[0] = "bk123456";
                rs.password = TM.Encrypt.CryptoMD5TM(newpass[0] + rs.salt);
                db.SaveChanges();
                return Json(new { success = $"Mật khẩu mới của tài khoản <strong>{rs.username}</strong> là: <strong>{newpass[0]}</strong>!" }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { error = $"Lỗi trong quá trình xử lý!" }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult Edit(Guid id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
            Authentication.user user = db.users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            ViewBag.Department = getGroups(Common.Objects.groups.department);
            return View(user);
        }
        [HttpPost]
        public ActionResult Edit([Bind(Include = "id,full_name,mobile,email,address,roles,staff_id,flag")]Authentication.user user_tmp)
        {
            if (ModelState.IsValid)
            {
                var user = db.users.Find(user_tmp.id);
                user.full_name = user_tmp.full_name;
                user.mobile = user_tmp.mobile;
                user.email = user_tmp.email;
                user.address = user_tmp.address;
                user.roles = user_tmp.roles;
                user.staff_id = user_tmp.staff_id;
                user.flag = user_tmp.flag;
                user.updated_by = Authentication.Auth.AuthUser.id.ToString();
                user.updated_at = DateTime.Now;
                db.Entry(user).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                this.success(TM.Common.Language.msgUpdateSucsess);
                return RedirectToAction("Index");
            }
            else
                this.danger(TM.Common.Language.msgUpdateError);
            return View(user_tmp);
        }
        [HttpPost]
        public ActionResult Upload()
        {
            var file = TM.IO.FileDirectory.Upload(Request.Files, TM.Common.Directories.orther, false);
            var files = file.UploadFile();
            var index = 0;
            if (files.Count > 0)
                foreach (var item in TM.IO.FileDirectory.ReadFile(TM.Common.Directories.orther + files[0], '\t'))
                {
                    index++;
                    if (index == 1) continue;
                    var username = item[0].ToLower();
                    var user = db.users.Where(m => m.username.ToLower() == username).FirstOrDefault();
                    if (user != null)
                    {
                        user.full_name = item[2];
                        user.mobile = item[3];
                        user.email = item[4];
                        user.roles = item[5];
                        user.staff_id = Guid.Parse(item[6]);
                        user.updated_by = Authentication.Auth.AuthUser.id.ToString();
                        user.updated_at = DateTime.Now;
                        db.Entry(user).State = System.Data.Entity.EntityState.Modified;
                    }
                    else
                    {
                        user = new Authentication.user();
                        user.id = Guid.NewGuid();
                        user.salt = Guid.NewGuid().ToString();
                        user.username = item[0];
                        user.password = TM.Encrypt.CryptoMD5TM(item[1] + user.salt);
                        user.full_name = item[2];
                        user.mobile = item[3];
                        user.email = item[4];
                        user.roles = item[5];
                        user.staff_id = Guid.Parse(item[6]);
                        user.flag = 1;
                        user.created_by = Authentication.Auth.AuthUser.id.ToString();
                        user.created_at = DateTime.Now;
                        user.updated_by = Authentication.Auth.AuthUser.id.ToString();
                        user.updated_at = DateTime.Now;
                        db.users.Add(user);
                    }
                }
            db.SaveChanges();
            this.success(TM.Common.Language.msgUpdateSucsess);
            return RedirectToAction("Create");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Delete(string[] uid)
        {
            try
            {
                foreach (var item in uid)
                {
                    var tmp = Guid.Parse(item);
                    var rs = db.users.Find(tmp);
                    db.users.Remove(rs);
                }
                db.SaveChanges();
                return Json(new { success = TM.Common.Language.msgDeleteSucsess }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception) { return Json(new { danger = TM.Common.Language.msgError }, JsonRequestBehavior.AllowGet); }
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
                    var rs = db.users.Find(tmp);
                    rs.flag = flag = rs.flag == 1 ? 0 : 1;
                }
                db.SaveChanges();
                return Json(new { success = (flag == 0 ? "Khóa tài khoản thành công" : "Khôi phục tài khoản thành công") }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception) { return Json(new { danger = "Xử lý lỗi vui lòng thử lại!" }, JsonRequestBehavior.AllowGet); }
        }
    }
}