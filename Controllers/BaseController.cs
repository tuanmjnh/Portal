using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dapper;

namespace Portal.Controllers
{
    public class BaseController : Controller
    {
        public Models.MainContext db = new Models.MainContext();
        public Models.ContextTTKDServerTMVN ContextTTKDServerTMVN = new Models.ContextTTKDServerTMVN();
        public Models.ContextTTKDServerVNPT ContextTTKDServerVNPT = new Models.ContextTTKDServerVNPT();
        public Models.ContextORCHNIVNPTBACKAN1 ContextORCHNIVNPTBACKAN1 = new Models.ContextORCHNIVNPTBACKAN1();
        [Filters.AuthVinaphone()]
        public ActionResult DownloadFiles(string dir, string DestName = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(dir))
                {
                    dir = dir.TrimEnd('/', '\\');
                    return TM.IO.FileDirectory.FileContentResult(HttpUtility.UrlDecode(dir), DestName);
                }
                else
                    return TM.IO.FileDirectory.FileContentResult(HttpUtility.UrlDecode(dir));
            }
            catch (Exception)
            {
                return HttpNotFound();
            }
        }
        public static System.Data.SqlClient.SqlConnection Connection()
        {
            try
            {
                return TM.SQL.DBStatic.Connection("MainContext");
            }
            catch (Exception) { return null; }
        }
        public static void ConnectionString()
        {
            TM.SQL.DBStatic.ConstantConnectionString = "MainContext";
        }
        public static string GetUser(string id)
        {
            using (var dbs = new Models.MainContext())
            {
                if (id != null)
                {
                    var rs = dbs.users.Find(Guid.Parse(id));
                    if (rs != null)
                        if (!String.IsNullOrEmpty(rs.full_name))
                            return rs.full_name;
                        else return rs.username;
                    else return TM.Common.Language.emptyvl;
                }
                else return TM.Common.Language.emptyvl;
            }
        }
        public List<Models.group> getGroups(string AppKey, int flag)
        {
            try
            {
                return db.groups.Where(d => d.app_key == AppKey && d.flag == flag).OrderBy(d => d.title).ToList();
            }
            catch (Exception) { return null; }
        }
        public List<Models.group> getGroups(string AppKey)
        {
            try
            {
                return db.groups.Where(d => d.app_key == AppKey && d.flag > 0).OrderBy(d => d.title).ThenBy(d => d.level).ToList();
            }
            catch (Exception) { return null; }
        }
        //public static List<Models.setting> AllSetting { get; set; }
        public static List<dynamic> AllSetting { get; set; }
        bool setting = setSetting();
        public static List<dynamic> Settings(string module_key)
        {
            try
            {
                //return AllSetting.Where(s => s.module_key.Equals("module_key")).ToList();
                return Connection().Query("SELECT * FROM settings WHERE module_key=@module_key",
                    new { module_key = module_key }).ToList();
            }
            catch (Exception) { return null; }
        }
        public static List<dynamic> Settings(string module_key, string sub_key)
        {
            try
            {
                //return AllSetting.Where(s => s.module_key.Equals(module_key) && s.sub_key.Equals(sub_key)).ToList();
                return Connection().Query("SELECT * FROM settings WHERE module_key=@module_key AND sub_key=@sub_key",
                new { module_key = module_key, sub_key = sub_key }).ToList();
            }
            catch (Exception) { return null; }
        }
        public static dynamic Setting(string module_key, string sub_key, string value)
        {
            try
            {
                //return AllSetting.Where(s => s.module_key.Equals(module_key) && s.sub_key.Equals(sub_key) && s.value.Equals(value)).FirstOrDefault();
                return Connection().Query("SELECT * FROM settings WHERE module_key=@module_key AND sub_key=@sub_key AND value=@value",
                new { module_key = module_key, sub_key = sub_key, value = value }).First();
            }
            catch (Exception) { return null; }
        }
        public static string Value(string module_key, string sub_key)
        {
            try
            {
                return Settings(module_key, sub_key).First().value;
            }
            catch (Exception) { return null; }
        }
        public static string SubValue(string module_key, string sub_key, string value)
        {
            try
            {
                return Setting(module_key, sub_key, value).sub_value;
            }
            catch (Exception) { return null; }
        }
        public static bool setSetting()
        {
            try
            {
                if (AllSetting == null) LoadSetting();
                return true;
            }
            catch (Exception) { return false; }
        }
        public static void LoadSetting()
        {
            try
            {
                AllSetting = Connection().Query("SELECT * FROM settings").ToList();
            }
            catch (Exception) { }
            //using (var dbs = new Models.PortalContext())
            //{
            //    AllSetting = dbs.settings.ToList();
            //}
        }
        
        //public static string Root = SettingKey("host").value;

        //public static List<Models.setting> Setting()
        //{
        //    using (var dbs = new Models.PortalContext())
        //    {
        //        return dbs.settings.ToList();
        //    }
        //}
        //public static Models.setting SettingKey(string app_key)
        //{
        //    return Setting().Where(s => s.app_key == app_key).FirstOrDefault();
        //}
        public static string getMonthYear(string str)
        {
            try
            {
                return str[0].ToString() + str[1].ToString() + "/20" + str[2].ToString() + str[3].ToString();
            }
            catch (Exception) { return str; }
        }
        public static string getMonthDayYear(string str)
        {
            try
            {
                return str[0].ToString() + str[1].ToString() + "/1/20" + str[2].ToString() + str[3].ToString();
            }
            catch (Exception) { return str; }
        }
        public static string getYearMonth(string str)
        {
            try
            {
                return str[4].ToString() + str[5].ToString() + "/" + str[0].ToString() + str[1].ToString() + str[2].ToString() + str[3].ToString();
            }
            catch (Exception) { return str; }
        }
        public static bool ReExtensionToLower(string DataSource)
        {
            using (FileManagerController f = new FileManagerController())
            {
                return f.ExtToLower(DataSource);
            }
        }
        protected override void Dispose(bool disposing)
        {
            //if (RoleManager != null) RoleManager.Dispose();
            //if (UserManager != null) UserManager.Dispose();
            if (db != null) db.Dispose();
            base.Dispose(disposing);
        }
        public int UploadBase(string DataSource, string strResult = null, List<string> fileUpload = null, string Extension = ".dbf")
        {
            try
            {
                int uploadedCount = 0;
                if (Request.Files.Count > 0)
                {
                    FileManagerController.InsertDirectory(DataSource, false);
                    var fileNameSource = new List<string>();
                    var fileSavePath = new List<string>();
                    //Delete old File
                    //TM.IO.Delete(obj.DataSource, TM.IO.Files(obj.DataSource));

                    for (int i = 0; i < Request.Files.Count; i++)
                    {
                        var file = Request.Files[i];
                        if (!file.FileName.IsExtension(Extension))
                            return (int)Common.Objects.ResultCode._extension;

                        if (file.ContentLength > 0)
                        {
                            fileUpload.Add(System.IO.Path.GetFileName(file.FileName).ToLower());
                            fileSavePath.Add(DataSource + fileUpload[i]);
                            file.SaveAs(fileSavePath[i]);
                            uploadedCount++;
                            FileManagerController.InsertFile(DataSource + fileUpload[i], false);
                        }
                    }
                    var rs = "Tải lên thành công </br>";
                    foreach (var item in fileUpload) rs += item + "<br/>";
                    strResult = rs;
                    return (int)Common.Objects.ResultCode._success;
                }
                else
                    return (int)Common.Objects.ResultCode._length;

            }
            catch (Exception) { throw; }
        }
    }
}