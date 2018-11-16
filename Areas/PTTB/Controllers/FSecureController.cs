using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dapper;
using TM.Message;

namespace Portal.Areas.PTTB.Controllers
{
    [Filters.AuthVinaphone()]
    public class FSecureController : Controller
    {
        // GET: PTTB/FSecure
        public ActionResult Index()
        {
            //var a = TM.Connection.Oracle.OracleTMVN;
            return View();
        }
        public ActionResult Export()
        {
            try
            {
                var Oracle = new TM.Connection.Oracle("ORCHNIVNPTBACKAN1");
                var rs = Oracle.Connection.Query<Portal.ModelsHNI.TB_FSECURE_BKN>("SELECT * FROM TB_FSECURE_BKN");
                //Export to any
                TM.Exports.ExportExcel(TM.Helper.Data.ToDataTable(rs.ToList()), "DanhSachFSecure");
            }
            catch (Exception ex) { this.danger(ex.Message); }
            return RedirectToAction("Index");
        }
        [HttpGet]
        public JsonResult getFSecureInformation(string SearhString)
        {
            try
            {
                var Oracle = new TM.Connection.Oracle("ORCHNIVNPTBACKAN1");
                //Remove 0 from SO_DIDONG AND +84
                //var prefix = SearhString.Substring(0, 2);
                //SearhString = prefix == "84" ? SearhString.Substring(2) : SearhString;
                //SearhString = SearhString.Replace("+84", "0");
                //SearhString = SearhString[0].ToString() == "0" ? SearhString.Substring(1) : SearhString;

                var data = Oracle.Connection.Query<Portal.ModelsHNI.TB_FSECURE_BKN>(
                            $@"SELECT * FROM TB_FSECURE_BKN WHERE 
                            SO_DIDONG='{SearhString}' OR 
                            USERNAME='+{SearhString}' OR 
                            MA_FSECURE='{SearhString}' OR 
                            ACCOUNT='{SearhString}' OR 
                            SO_MEN='{SearhString}'").FirstOrDefault();
                return Json(new { SearhString = SearhString, data = data, success = TM.Common.Language.msgSucsess }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message }, JsonRequestBehavior.AllowGet); }
        }
    }
}
