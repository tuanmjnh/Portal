﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TM.Message;

namespace Portal.Controllers
{
    [Filters.AuthVinaphone]
    public class ProfileController : BaseController
    {
        // GET: Profile
        public ActionResult Index()
        {
            Guid id = Authentication.Auth.AuthUser.id;
            //ViewBag.address = db.users.SingleOrDefault(u => u.id == id).address;
            return View(db.users.SingleOrDefault(u => u.id == id));
        }
        [HttpPost]
        public ActionResult Index(FormCollection collection)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Guid id = Authentication.Auth.AuthUser.id;
                    var rs = db.users.SingleOrDefault(u => u.id == id);
                    rs.address = collection["address"].ToString(); ;
                    rs.full_name = collection["full_name"].ToString();
                    db.SaveChanges();
                    ModelState.Clear();
                    this.success("Cập nhật thông tin thành công");
                    return Redirect("Profile");
                }
            }
            catch { }
            this.danger("Cập nhật thông tin thất bại");
            return View();
        }
        public ActionResult ChangePassword()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ChangePassword(FormCollection collection)
        {

            try
            {
                if (ModelState.IsValid)
                {
                    Guid id = Authentication.Auth.AuthUser.id;
                    string password = TM.Encrypt.CryptoMD5TM(collection["oldpassword"].ToString() + Authentication.Auth.AuthUser.salt);
                    var rs = db.users.SingleOrDefault(u => u.id == id && u.password == password);

                    if (rs != null)
                    {
                        password = TM.Encrypt.CryptoMD5TM(collection["password"].ToString() + Authentication.Auth.AuthUser.salt);
                        rs.password = password;
                        db.SaveChanges();
                        ModelState.Clear();
                        this.success("Thay đổi mật khẩu thành công");
                    }
                    else
                    {
                        ModelState.Clear();
                        this.danger("Mật khẩu hiện tại không đúng");
                    }

                }
            }
            catch (Exception) { this.danger("Cập nhật thông tin thất bại"); }
            return RedirectToAction("ChangePassword");
        }
        public ActionResult Setting()
        {
            //if (id == null)
            //    return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            //if (data == null)
            //    return HttpNotFound();
            var module_key = Authentication.Auth.AuthUser.id.ToString();
            var setting = db.settings.Where(s => s.module_key.Equals(module_key) && s.sub_key.Equals(TM.Common.Config.sub_key.form_use_label));
            if (setting.Count() > 0)
                return View(setting.FirstOrDefault());
            else
                return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Setting(Guid? id, FormCollection collection)
        {

            try
            {
                if (id != null)
                {
                    var settings = db.settings.Find(id);
                    settings.value = collection[TM.Common.Config.sub_key.form_use_label];
                }
                else
                {
                    var settings = new Models.setting();
                    settings.id = Guid.NewGuid();
                    settings.module_key = Authentication.Auth.AuthUser.id.ToString();
                    settings.sub_key = TM.Common.Config.sub_key.form_use_label;
                    settings.value = collection[TM.Common.Config.sub_key.form_use_label];
                    db.settings.Add(settings);
                }
                db.SaveChanges();
                this.success("Cập nhật thông tin thành công");
            }
            catch (Exception) { this.danger("Cập nhật thông tin thất bại"); }
            return RedirectToAction("Setting");
        }
    }
}