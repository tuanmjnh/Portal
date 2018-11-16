using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TM.Message;

namespace Portal.Controllers
{
    public class AuthController : BaseController
    {
        // GET: Auth
        public ActionResult Index()
        {
            if (Connection() == null)
                this.danger("Không thể kết nối đến CSDL!");
            return View();
        }
        [HttpPost]
        public ActionResult Index(FormCollection collection)
        {
            try
            {
                //var collection = HttpContext.Request.ReadFormAsync();
                string username = collection["username"].ToString();
                string password = collection["password"].ToString();

                //AuthStatic
                var AuthStatic = Authentication.Auth.isAuthStatic(username, password);
                if (AuthStatic != null)
                {
                    Authentication.Auth.SetAuth(AuthStatic);
                    return Redirect(TM.Url.RedirectContinue());
                }
                //AuthDB
                var user = db.users.SingleOrDefault(u => u.username == username);

                //Account not Exist
                if (user == null)
                {
                    this.danger("Sai tên tài khoản hoặc mật khẩu!");
                    return RedirectToAction("Index");
                }

                //Password wrong
                password = TM.Encrypt.CryptoMD5TM(password + user.salt);
                user = db.users.SingleOrDefault(u => u.username == username && u.password == password);
                if (user == null)
                {
                    this.danger("Sai tên tài khoản hoặc mật khẩu!");
                    return RedirectToAction("Index");
                }

                //Account is locked
                if (user.flag < 1)
                {
                    this.danger("Tài khoản đã bị khóa. Vui lòng liên hệ admin!");
                    return RedirectToAction("Index");
                }

                //Update last login
                user.last_login = DateTime.Now;
                db.Entry(user).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                //Set Auth Account
                Authentication.Auth.SetAuth(user);
                return Redirect(TM.Url.RedirectContinue());
            }
            catch (Exception ex)
            {
                this.danger("Đăng nhập không thành công, vui lòng liên hệ admin!");
            }
            return RedirectToAction("Index");

            //try
            //{
            //    string username = collection["username"].ToString();
            //    string password = collection["password"].ToString();
            //    System.Collections.ArrayList AuthItems = new System.Collections.ArrayList();

            //    //AuthStatic
            //    if (TM.Common.AuthStatic.isAuthStatic(username, password))
            //    {
            //        AuthItems.Add(TM.Common.AuthStatic.id.ToString());
            //        AuthItems.Add(TM.Common.AuthStatic.username);
            //        AuthItems.Add(TM.Common.AuthStatic.salt);
            //        AuthItems.Add(TM.Common.AuthStatic.full_name);
            //        AuthItems.Add(TM.Common.AuthStatic.mobile);
            //        AuthItems.Add(TM.Common.AuthStatic.email);
            //        AuthItems.Add(TM.Common.AuthStatic.address);
            //        AuthItems.Add(TM.Common.AuthStatic.roles);
            //        AuthItems.Add(TM.Common.AuthStatic.created_by);
            //        AuthItems.Add(DateTime.Now.ToString());
            //        AuthItems.Add(TM.Common.AuthStatic.updated_by);
            //        AuthItems.Add(DateTime.Now.ToString());
            //        AuthItems.Add(DateTime.Now.ToString());
            //        AuthItems.Add(TM.Common.AuthStatic.flag.ToString());
            //        TM.Common.Auth.setAuth(AuthItems);
            //        return Redirect(TM.Url.RedirectContinue());
            //    }

            //    //AuthDB
            //    var tmp = db.users.Where(u => u.username == username || u.email == username);
            //    if (tmp.FirstOrDefault() == null)
            //    {
            //        this.danger("Sai tên tài khoản hoặc mật khẩu!");
            //        return View();
            //    }

            //    password = TM.Encrypt.CryptoMD5TM(password + tmp.FirstOrDefault().salt);
            //    tmp = db.users.Where(u => (u.username == username || u.email == username) && u.password == password);
            //    if (tmp.FirstOrDefault() == null)
            //    {
            //        this.danger("Sai tên tài khoản hoặc mật khẩu!");
            //        return View();
            //    }

            //    var user = tmp.Where(u => u.flag > 0).FirstOrDefault();
            //    if (user == null)
            //    {
            //        this.danger("Tài khoản đã bị khóa. Vui lòng liên hệ admin!");
            //        return View();
            //    }

            //    user.last_login = DateTime.Now;
            //    db.Entry(user).State = System.Data.Entity.EntityState.Modified;
            //    db.SaveChanges();

            //    AuthItems.Add(user.id);
            //    AuthItems.Add(user.username);
            //    AuthItems.Add(user.salt);
            //    AuthItems.Add(user.full_name);
            //    AuthItems.Add(user.mobile);
            //    AuthItems.Add(user.email);
            //    AuthItems.Add(user.address);
            //    AuthItems.Add(user.roles);
            //    AuthItems.Add(user.created_by);
            //    AuthItems.Add(user.created_at);
            //    AuthItems.Add(user.updated_by);
            //    AuthItems.Add(user.updated_at);
            //    AuthItems.Add(user.last_login);
            //    AuthItems.Add(user.flag);
            //    AuthItems.Add(user.staff_id);
            //    TM.Common.Auth.setAuth(AuthItems);
            //    return Redirect(TM.Url.RedirectContinue());
            //}
            //catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
            //{
            //    Exception raise = dbEx;
            //    foreach (var validationErrors in dbEx.EntityValidationErrors)
            //    {
            //        foreach (var validationError in validationErrors.ValidationErrors)
            //        {
            //            string message = string.Format("{0}:{1}",
            //                validationErrors.Entry.Entity.ToString(),
            //                validationError.ErrorMessage);
            //            // raise a new exception nesting  
            //            // the current instance as InnerException  
            //            raise = new InvalidOperationException(message, raise);
            //        }
            //    }
            //    throw raise;
            //}
            //return View();
        }
        [Filters.AuthVinaphone]
        [HttpGet]
        public ActionResult logout()
        {
            Authentication.Auth.Logout();
            return Redirect(System.Web.HttpContext.Current.Request.UrlReferrer.ToString());//System.Web.HttpContext.Current.Request.UrlReferrer
        }
        [Filters.AuthVinaphone]
        public ActionResult ChangePassword(Guid id)
        {

            return View(db.users.SingleOrDefault(u => u.id == id));
        }
        [Filters.AuthVinaphone]
        [HttpPost]
        public ActionResult ChangePassword(Guid id, string password)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var rs = db.users.Find(id);
                    rs.password = TM.Encrypt.CryptoMD5TM(password + rs.salt);
                    db.SaveChanges();
                    ModelState.Clear();
                    this.success("Cập nhật mật khẩu thành công");
                    return RedirectToAction("Index");
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
            {
                Exception raise = dbEx;
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        string message = string.Format("{0}:{1}",
                            validationErrors.Entry.Entity.ToString(),
                            validationError.ErrorMessage);
                        // raise a new exception nesting  
                        // the current instance as InnerException  
                        raise = new InvalidOperationException(message, raise);
                    }
                }
                throw raise;
            }
            return View();
        }
    }
}