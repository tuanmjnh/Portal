﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dapper;

namespace Portal.Controllers
{
    public class NotificationController : BaseController
    {
        //TM.Connection.SQLServer SQLServer;
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public JsonResult getNotification(int offset = 0, int limit = 10)
        {
            try
            {
                if (!Authentication.Auth.isAuth) return Json(new { success = "Ok!" }, JsonRequestBehavior.AllowGet);

                var SQLServer = new TM.Connection.SQLServer();
                var qry = $"SELECT * FROM NOTIFICATION WHERE DESTINATION='{Authentication.Auth.AuthUser.username}' AND FLAG=1 ORDER BY FLAG,CREATEDAT";
                var data = SQLServer.Connection.Query<Billing.Models.NOTIFICATION>(qry);
                var total = data.Count();
                data = data.Skip(offset).Take(limit).ToList();
                SQLServer.Close();
                return Json(new { data = data, total = total }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message }, JsonRequestBehavior.AllowGet); }//"Không tìm thấy dữ liệu, vui lòng thực hiện lại!"
            finally { }
        }
    }
}