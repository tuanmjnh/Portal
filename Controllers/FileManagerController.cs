using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TM.Message;
using TM.Helper;
using PagedList;

namespace Portal.Controllers
{
    [Filters.AuthVinaphone(Role = Authentication.Roles.superadmin + "," + Authentication.Roles.admin)]
    //[Filters.AuthVinaphone()]
    public class FileManagerController : BaseController
    {
        //public ActionResult Index(string dir, int? flag, string order, string currentFilter, string searchString, int? page, string export)
        //{
        //    if (searchString != null)
        //    {
        //        page = 1;
        //        searchString = searchString.Trim();
        //    }
        //    else searchString = currentFilter;

        //    ViewBag.currentFilter = searchString;
        //    ViewBag.flag = flag;

        //    string path = "~/Uploads" + (dir != null ? dir : "");
        //    ViewBag.directory = TM.IO.FileDirectory.Directories(path);//TM.IO.Directories(path).OrderByDescending(d => d.Name).ToList();
        //    ViewBag.files = TM.IO.FileDirectory.Files(path, new[] { ".dbf", ".txt", ".xls", ".xlsx" });

        //    //Export to any
        //    //if (!String.IsNullOrEmpty(export))
        //    //{
        //    //    TM.Exports.ExportExcel(TM.Helper.Data.ToDataTable(rs.ToList()), "Danh sách tài khoản");
        //    //    return RedirectToAction("Index");
        //    //}
        //    return View();
        //}
        public ActionResult Index(string dir, int? flag, string order, string currentFilter, string searchString, int? page, string datetime, int? datetimeType, string export)
        {
            try
            {
                if (searchString != null)
                {
                    page = 1;
                    searchString = searchString.Trim();
                }
                else searchString = currentFilter;
                ViewBag.dir = dir;
                ViewBag.order = order;
                ViewBag.currentFilter = searchString;
                ViewBag.flag = flag;
                ViewBag.datetime = datetime;
                ViewBag.datetimeType = datetimeType;

                var extension = new[] { ".dbf", ".txt", ".xls", ".xlsx" };

                var rs = db.FileManagers.ToList().AsEnumerable();
                if (!Authentication.Auth.inRoles(new[] { Authentication.Roles.admin, Authentication.Roles.superadmin }))
                    rs = rs.Where(d => extension.Contains(d.Extension) || d.Type == TM.Common.Objects.FileManager.directory);

                if (!String.IsNullOrEmpty(dir))
                    rs = rs.Where(d => d.Subdirectory == dir);
                else
                    rs = rs.Where(d => d.Level == 0);

                if (!String.IsNullOrEmpty(searchString))
                    rs = rs.Where(d =>
                    d.Name.Contains(searchString) ||
                    d.FullName.Contains(searchString));

                if (!String.IsNullOrEmpty(datetime))
                {
                    var date = datetime.Split('-');
                    if (date.Length > 1)
                    {
                        var tmp0 = date[0].Split('/');
                        var tmp1 = date[1].Split('/');
                        if (tmp0.Length > 2 && tmp1.Length > 2)
                        {
                            rs = datetimeType == 0
                                ? rs.Where(d =>
                                 d.CreationTime.Value.Day >= int.Parse(tmp0[0]) &&
                                 d.CreationTime.Value.Month >= int.Parse(tmp0[1]) &&
                                 d.CreationTime.Value.Year >= int.Parse(tmp0[2]) &&
                                 d.CreationTime.Value.Day <= int.Parse(tmp1[0]) &&
                                 d.CreationTime.Value.Month <= int.Parse(tmp1[1]) &&
                                 d.CreationTime.Value.Year <= int.Parse(tmp1[2]))
                                : rs.Where(d =>
                                 d.LastWriteTime.Value.Day >= int.Parse(tmp0[0]) &&
                                 d.LastWriteTime.Value.Month >= int.Parse(tmp0[1]) &&
                                 d.LastWriteTime.Value.Year >= int.Parse(tmp0[2]) &&
                                 d.LastWriteTime.Value.Day <= int.Parse(tmp1[0]) &&
                                 d.LastWriteTime.Value.Month <= int.Parse(tmp1[1]) &&
                                 d.LastWriteTime.Value.Year <= int.Parse(tmp1[2]));
                        }
                    }
                }

                if (flag == 0) rs = rs.Where(d => d.Flag == 0);
                else rs = rs.Where(d => d.Flag > 0);

                switch (order)
                {
                    default:
                        rs = rs.OrderByDescending(d => d.CreationTime);
                        break;
                    case "name_asc":
                        rs = rs.OrderBy(d => d.Name);
                        break;
                    case "name_desc":
                        rs = rs.OrderByDescending(d => d.Name);
                        break;
                    case "type_asc":
                        rs = rs.OrderBy(d => d.Type);
                        break;
                    case "type_desc":
                        rs = rs.OrderByDescending(d => d.Type);
                        break;
                    case "size_asc":
                        rs = rs.OrderBy(d => d.Length);
                        break;
                    case "size_asc_desc":
                        rs = rs.OrderByDescending(d => d.Length);
                        break;
                    case "createdat_asc":
                        rs = rs.OrderBy(d => d.CreationTime);
                        break;
                    case "createdat_desc":
                        rs = rs.OrderByDescending(d => d.CreationTime);
                        break;
                    case "updatedat_asc":
                        rs = rs.OrderBy(d => d.LastWriteTime);
                        break;
                    case "updatedat_desc":
                        rs = rs.OrderByDescending(d => d.LastWriteTime);
                        break;
                }

                //Export to any
                if (!String.IsNullOrEmpty(export))
                {
                    TM.Exports.ExportExcel(Data.ToDataTable(rs.ToList()), "FileManagerList");
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
        [HttpGet]
        public ActionResult Download(string files)
        {
            try
            {
                string[] id = files.Split(',');
                if (id.Length == 1)
                {
                    var file = db.FileManagers.Find(Guid.Parse(id[0]));
                    if (file == null)
                        file = db.FileManagers.FirstOrDefault(m => m.Name == id[0]);
                    if (file != null)
                    {
                        byte[] fileBytes = System.IO.File.ReadAllBytes(TM.IO.FileDirectory.MapPath(TM.Common.Directories.Uploads) + file.Subdirectory + file.Name);
                        return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, file.Name);
                    }
                }
                else if (files.Length > 1)
                {
                    List<Guid> listID = new List<Guid>();
                    foreach (var i in id) listID.Add(Guid.Parse(i));
                    var list = db.FileManagers.Where(d => listID.Contains(d.ID)).ToList();

                    var listFile = new List<string>();
                    foreach (var item in list) listFile.Add(TM.Common.Directories.Uploads + item.Subdirectory + item.Name);
                    TM.IO.Zip.DownloadZipToBrowser(listFile);
                    //foreach (var item in id)
                    //{
                    //    var file = db.FileManagers.Find(Guid.Parse(item));
                    //    byte[] fileBytes = System.IO.File.ReadAllBytes(TM.IO.FileDirectory.MapPath(TM.Common.Directories.Uploads) + file.Subdirectory + file.Name);
                    //    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, file.Name);
                    //}
                    //return Json(new { success = "Thành công" }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { success = "Thành công" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = ex.Message }, JsonRequestBehavior.AllowGet); }
        }

        //[HttpGet]
        //public ActionResult Download(string dir, string files)
        //{
        //    return TM.IO.FileDirectory.FileContentResult("~/Uploads/" + HttpUtility.UrlDecode(dir) + "/" + files);
        //}
        public ActionResult LoadData()
        {
            try
            {
                var sqldb = new TM.SQL.DB();
                sqldb.Execute("delete from FileManager");
                if (InsertDirectoriesFiles(TM.Common.Directories.Uploads))
                    this.success("Thành công");
            }
            catch (Exception ex)
            {
                this.danger(ex.Message);
            }
            return RedirectToAction("Index");
        }
        public ActionResult ExtensionToLower()
        {
            try
            {
                if (ExtToLower(TM.Common.Directories.Uploads))
                    this.success("Thành công");
                else
                    this.danger(TM.Common.Language.msgError);
            }
            catch (Exception ex)
            {
                this.danger(ex.Message);
            }
            return RedirectToAction("Index");
        }
        public static bool InsertDirectoriesFiles(string path)
        {
            try
            {
                using (var db = new Models.MainContext())
                {
                    var all = getDirectoriesFiles(path.Replace('/', '\\'));
                    foreach (var item in all)
                    {
                        var fullName = item.Key.FullName.Trim('\\');
                        var FileManager = db.FileManagers.Where(d => d.FullName == fullName).FirstOrDefault();
                        if (FileManager == null)
                        {
                            FileManager = new Models.FileManager();
                            FileManager.ID = Guid.NewGuid();
                            FileManager.ParentID = Guid.Empty;
                            FileManager.Parent = item.Key.Parent.ToString();
                            FileManager.Root = item.Key.Root.ToString();
                            FileManager.Subdirectory = "\\";
                            FileManager.Level = 0;
                            FileManager.Name = item.Key.Name;
                            FileManager.FullName = item.Key.FullName;
                            FileManager.Extension = item.Key.Extension;
                            FileManager.ExtensionIcon = "fa fa-folder-open";
                            FileManager.Type = string.IsNullOrEmpty(item.Key.Extension) ? TM.Common.Objects.FileManager.directory : TM.Common.Objects.FileManager.file;
                            FileManager.Attributes = item.Key.Attributes.ToString();
                            FileManager.AttributesEnum = (int)item.Key.Attributes;//item.Key.Attributes.ToString();
                                                                                  //FileManager.Description = null;
                            FileManager.CreationTime = item.Key.CreationTime;
                            FileManager.CreationTimeUtc = item.Key.CreationTimeUtc;
                            FileManager.LastAccessTime = item.Key.LastAccessTime;
                            FileManager.LastAccessTimeUtc = item.Key.LastAccessTimeUtc;
                            FileManager.LastWriteTime = item.Key.LastWriteTime;
                            FileManager.LastWriteTimeUtc = item.Key.LastWriteTimeUtc;
                            FileManager.CreatedBy = Authentication.Auth.AuthUser.id.ToString();
                            //FileManager.LastAccessBy = Authentication.Auth.AuthUser.id.ToString();
                            //FileManager.LastWriteBy = Authentication.Auth.AuthUser.id.ToString();
                            FileManager.Exists = item.Key.Exists;
                            FileManager.Flag = 1;
                            if (item.Key.Parent.ToString() != TM.Common.Directories.Uploads.Trim('\\'))
                            {
                                var tmp = db.FileManagers.Local.Where(d => d.Name == FileManager.Parent && d.Flag > 0).FirstOrDefault();

                                if (tmp == null) continue;
                                FileManager.ParentID = tmp.ID;
                                FileManager.Subdirectory = tmp.Subdirectory + FileManager.Parent + "\\";
                                FileManager.Level = tmp.Level + 1;
                            }
                            db.FileManagers.Add(FileManager);
                        }
                        //
                        foreach (var file in item.Value)
                        {
                            if (file.Name == "Thumbs.db") continue;
                            fullName = file.FullName.Trim('\\');
                            var FileItem = db.FileManagers.Where(d => d.FullName == fullName).FirstOrDefault();
                            if (FileItem != null)
                            {
                                FileItem.ParentID = FileManager.ID;
                                db.Entry(FileItem).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                continue;
                            }
                            FileItem = new Models.FileManager();
                            FileItem.ID = Guid.NewGuid();
                            FileItem.ParentID = FileManager.ID;
                            FileItem.Parent = item.Key.Name;
                            FileItem.Root = item.Key.Root.ToString();
                            FileItem.Subdirectory = FileManager.Subdirectory + item.Key.Name + "\\";
                            FileItem.Level = FileManager.Level + 1;
                            FileItem.Name = file.Name;
                            FileItem.FullName = file.FullName;
                            FileItem.Extension = file.Extension;
                            FileItem.ExtensionIcon = getExtensionIcon(file.Extension);
                            FileItem.Type = string.IsNullOrEmpty(file.Extension) ? TM.Common.Objects.FileManager.directory : TM.Common.Objects.FileManager.file;
                            FileItem.Attributes = file.Attributes.ToString();
                            FileItem.AttributesEnum = (int)file.Attributes;//item.Key.Attributes.ToString();
                            FileItem.Length = file.Length;
                            FileItem.IsReadOnly = file.IsReadOnly;
                            //FileItem.Description = null;
                            FileItem.CreationTime = file.CreationTime;
                            FileItem.CreationTimeUtc = file.CreationTimeUtc;
                            FileItem.LastAccessTime = file.LastAccessTime;
                            FileItem.LastAccessTimeUtc = file.LastAccessTimeUtc;
                            FileItem.LastWriteTime = file.LastWriteTime;
                            FileItem.LastWriteTimeUtc = file.LastWriteTimeUtc;
                            FileItem.CreatedBy = Authentication.Auth.AuthUser.id.ToString();
                            //FileItem.LastAccessBy = Authentication.Auth.AuthUser.id.ToString();
                            //FileItem.LastWriteBy = Authentication.Auth.AuthUser.id.ToString();
                            FileItem.Exists = file.Exists;
                            FileItem.Flag = 1;
                            db.FileManagers.Add(FileItem);
                        }
                    }
                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception) { return false; }
        }
        public static bool InsertDirectory(string directory, bool IsMapPath = true)
        {
            try
            {
                using (var db = new Models.MainContext())
                {
                    TM.IO.FileDirectory.CreateDirectory(directory, IsMapPath);
                    var path = IsMapPath ? TM.IO.FileDirectory.MapPath(directory.Replace('/', '\\')).Trim('\\') : directory.Replace('/', '\\').Trim('\\');
                    var FileManager = db.FileManagers.Where(d => d.FullName == path).FirstOrDefault();
                    if (FileManager == null)
                    {
                        var item = new System.IO.DirectoryInfo(path);
                        FileManager = new Models.FileManager();
                        FileManager.ID = Guid.NewGuid();
                        FileManager.ParentID = Guid.Empty;
                        FileManager.Parent = item.Parent.ToString();
                        FileManager.Root = item.Root.ToString();
                        FileManager.Subdirectory = "\\";
                        FileManager.Level = 0;
                        FileManager.Name = item.Name;
                        FileManager.FullName = item.FullName;
                        FileManager.Extension = item.Extension;
                        FileManager.ExtensionIcon = "fa fa-folder-open";
                        FileManager.Type = string.IsNullOrEmpty(item.Extension) ? TM.Common.Objects.FileManager.directory : TM.Common.Objects.FileManager.file;
                        FileManager.Attributes = item.Attributes.ToString();
                        FileManager.AttributesEnum = (int)item.Attributes;//item.Key.Attributes.ToString();
                                                                          //FileManager.Description = null;
                        FileManager.CreationTime = item.CreationTime;
                        FileManager.CreationTimeUtc = item.CreationTimeUtc;
                        FileManager.LastAccessTime = item.LastAccessTime;
                        FileManager.LastAccessTimeUtc = item.LastAccessTimeUtc;
                        FileManager.LastWriteTime = item.LastWriteTime;
                        FileManager.LastWriteTimeUtc = item.LastWriteTimeUtc;
                        FileManager.CreatedBy = Authentication.Auth.AuthUser.id.ToString();
                        //FileManager.LastAccessBy = Authentication.Auth.AuthUser.id.ToString();
                        //FileManager.LastWriteBy = Authentication.Auth.AuthUser.id.ToString();
                        FileManager.Exists = item.Exists;
                        FileManager.Flag = 1;
                        if (item.Parent.ToString() != TM.Common.Directories.Uploads.Trim('\\'))
                        {
                            var tmp = db.FileManagers.Where(d => d.Name == FileManager.Parent && d.Flag > 0).FirstOrDefault();
                            if (tmp == null)
                            {
                                var directories = directory.Split('\\');
                                InsertDirectory(directory.Replace("\\" + FileManager.Name, ""));
                            }
                            tmp = db.FileManagers.Where(d => d.Name == FileManager.Parent && d.Flag > 0).FirstOrDefault();
                            FileManager.ParentID = tmp.ID;
                            FileManager.Subdirectory = tmp.Subdirectory + FileManager.Parent + "\\";
                            FileManager.Level = tmp.Level + 1;
                        }
                        db.FileManagers.Add(FileManager);
                    }
                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception) { return false; }
        }
        public static bool InsertFile(string file, bool IsMapPath = true)
        {
            try
            {
                using (var db = new Models.MainContext())
                {
                    file = IsMapPath ? TM.IO.FileDirectory.MapPath(file) : file;
                    var item = new System.IO.FileInfo(file);
                    item = TM.IO.FileDirectory.ReExtensionToLower(item.FullName, false);
                    var fullname = item.FullName.Replace("\\" + item.Name, "");
                    var directory = db.FileManagers.Where(d => d.FullName == fullname).FirstOrDefault();
                    if (directory == null) return false;
                    if (item.Name == "Thumbs.db") return true;
                    var FileItem = db.FileManagers.Where(d => d.FullName == item.FullName).FirstOrDefault();
                    if (FileItem != null)
                    {
                        FileItem.ParentID = directory.ID;
                        db.Entry(FileItem).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        return true;
                    }
                    FileItem = new Models.FileManager();
                    FileItem.ID = Guid.NewGuid();
                    FileItem.ParentID = directory.ID;
                    FileItem.Parent = directory.Name;
                    FileItem.Root = directory.Root;
                    FileItem.Subdirectory = directory.Subdirectory + directory.Name + "\\";
                    FileItem.Level = directory.Level + 1;
                    FileItem.Name = item.Name;
                    FileItem.FullName = item.FullName;
                    FileItem.Extension = item.Extension;
                    FileItem.ExtensionIcon = getExtensionIcon(item.Extension);
                    FileItem.Type = string.IsNullOrEmpty(item.Extension) ? TM.Common.Objects.FileManager.directory : TM.Common.Objects.FileManager.file;
                    FileItem.Attributes = item.Attributes.ToString();
                    FileItem.AttributesEnum = (int)item.Attributes;//item.Key.Attributes.ToString();
                    FileItem.Length = item.Length;
                    FileItem.IsReadOnly = item.IsReadOnly;
                    //FileItem.Description = null;
                    FileItem.CreationTime = item.CreationTime;
                    FileItem.CreationTimeUtc = item.CreationTimeUtc;
                    FileItem.LastAccessTime = item.LastAccessTime;
                    FileItem.LastAccessTimeUtc = item.LastAccessTimeUtc;
                    FileItem.LastWriteTime = item.LastWriteTime;
                    FileItem.LastWriteTimeUtc = item.LastWriteTimeUtc;
                    FileItem.CreatedBy = Authentication.Auth.AuthUser.id.ToString();
                    //FileItem.LastAccessBy = Authentication.Auth.AuthUser.id.ToString();
                    //FileItem.LastWriteBy = Authentication.Auth.AuthUser.id.ToString();
                    FileItem.Exists = item.Exists;
                    FileItem.Flag = 1;
                    db.FileManagers.Add(FileItem);
                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception) { return false; }
        }
        public static bool DeleteDirFile(string files, bool IsMapPath = true)
        {
            try
            {
                using (var db = new Models.MainContext())
                {
                    files = IsMapPath ? TM.IO.FileDirectory.MapPath(files) : files;
                    var rs = db.FileManagers.Where(d => d.FullName == files).FirstOrDefault();

                    if (rs == null)
                    {
                        TM.IO.FileDirectory.Delete(files, false);
                        TM.IO.FileDirectory.DeleteDirectory(files, false);
                        return true;
                    }

                    db.FileManagers.Remove(rs);
                    foreach (var file in db.FileManagers.Where(d => d.ParentID == rs.ID).ToList())
                        db.FileManagers.Remove(file);

                    if (rs.Type == TM.Common.Objects.FileManager.file)
                        TM.IO.FileDirectory.Delete(rs.FullName, false);
                    else
                        TM.IO.FileDirectory.DeleteDirectory(rs.FullName, false);

                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception) { return false; }
        }
        public bool ExtToLower(string path)
        {
            try
            {
                var all = getDirectoriesFiles(path.Replace('/', '\\'));
                foreach (var item in all)
                    foreach (var file in item.Value)
                    {
                        var newFile = TM.IO.FileDirectory.ReExtensionToLower(file.FullName.Trim('\\'), false);
                        var FileManagers = db.FileManagers.Where(d => d.FullName == file.FullName).FirstOrDefault();
                        if (FileManagers != null)
                        {
                            FileManagers.Name = newFile.Name;
                            FileManagers.FullName = newFile.FullName;
                            FileManagers.Extension = newFile.Extension;
                            db.Entry(FileManagers).State = System.Data.Entity.EntityState.Modified;

                        }
                    }
                db.SaveChanges();
                this.success("Thành công");
                return true;
            }
            catch (Exception) { return false; }
        }
        private List<System.IO.DirectoryInfo> getDirectories(string path)
        {
            var DirectoryInfo = new List<System.IO.DirectoryInfo>();
            foreach (var item in TM.IO.FileDirectory.Directories(path))
            {
                DirectoryInfo.Add(item);
                DirectoryInfo.AddRange(getDirectories(path + item.Name + "\\"));
            }
            return DirectoryInfo;
        }
        private static Dictionary<System.IO.DirectoryInfo, System.IO.FileInfo[]> getDirectoriesFiles(string path, string[] extension = null)
        {
            var DirectoryInfo = new Dictionary<System.IO.DirectoryInfo, System.IO.FileInfo[]>();
            DirectoryInfo.Add(new System.IO.DirectoryInfo(TM.IO.FileDirectory.MapPath(path)), TM.IO.FileDirectory.Files(path, extension));
            DirectoryInfo.AddRange(getDirFiles(path, extension));
            return DirectoryInfo;
        }
        private static Dictionary<System.IO.DirectoryInfo, System.IO.FileInfo[]> getDirFiles(string path, string[] extension = null)
        {
            var DirectoryInfo = new Dictionary<System.IO.DirectoryInfo, System.IO.FileInfo[]>();
            foreach (var item in TM.IO.FileDirectory.Directories(path))
            {
                var FileInfo = TM.IO.FileDirectory.Files(path + item.Name, extension);
                for (int i = 0; i < FileInfo.Length; i++)
                    FileInfo[i] = TM.IO.FileDirectory.ReExtensionToLower(FileInfo[i].FullName, false);
                FileInfo = TM.IO.FileDirectory.Files(path + item.Name, extension);
                DirectoryInfo.Add(item, FileInfo);
                DirectoryInfo.AddRange(getDirFiles(path + item.Name + "\\", extension));
            }
            return DirectoryInfo;
        }
        private static string getExtensionIcon(string extension)
        {
            if (TM.IO.FileDirectory.ImageCodecs().Contains("*" + extension.ToUpper()))
                return "fa fa-file-image-o";
            switch (extension)
            {
                default: return "fa fa-file";
                case ".xls": return "fa fa-file-excel-o";
                case ".xlsx": return "fa fa-file-excel-o";
                case ".dbf": return "fa fa-table";
                case ".txt": return "fa fa-file-text-o";
                case ".doc": return "fa fa-file-text";
                case ".docx": return "fa fa-file-text";
                case ".pdf": return "fa fa-file-pdf-o";
                case ".exe": return "fa fa-ravelry";
                case ".zip": return "fa fa-file-archive-o";
                case ".rar": return "fa fa-ticket";
                case ".xml": return "fa fa-file-code-o";
            }
        }
        public JsonResult update_flag(string uid)
        {
            try
            {
                string[] id = uid.Split(',');
                var flag = 0;
                foreach (var item in id)
                {
                    Guid tmp = Guid.Parse(item);
                    var rs = db.FileManagers.Find(tmp);
                    rs.Flag = flag = rs.Flag == 1 ? 0 : 1;
                }
                db.SaveChanges();
                return Json(new { success = (flag == 0 ? TM.Common.Language.msgDeleteSucsess : TM.Common.Language.msgRecoverSucsess) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception) { return Json(new { danger = TM.Common.Language.msgError }, JsonRequestBehavior.AllowGet); }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Delete(string[] uid)
        {
            try
            {
                foreach (var item in uid)
                {
                    Guid tmp = Guid.Parse(item);
                    var rs = db.FileManagers.Find(tmp);
                    db.FileManagers.Remove(rs);

                    foreach (var file in db.FileManagers.Where(d => d.ParentID == rs.ID).ToList())
                        db.FileManagers.Remove(file);

                    if (rs.Type == TM.Common.Objects.FileManager.file)
                        TM.IO.FileDirectory.Delete(rs.FullName, false);
                    else
                        TM.IO.FileDirectory.DeleteDirectory(rs.FullName, false);//TM.Common.Directories.Uploads + rs.Subdirectory.Trim('\\') + rs.Name
                }
                db.SaveChanges();
                return Json(new { success = TM.Common.Language.msgDeleteSucsess }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { danger = TM.Common.Language.msgError }, JsonRequestBehavior.AllowGet); }
        }

        public ActionResult BackupDatabase()
        {
            var SqlServer = new TM.Connection.SQLServer();
            try
            {
                var path = TM.Common.Directories.DBBak;
                TM.IO.FileDirectory.CreateDirectory(path, false);
                //TM.IO.FileDirectory.SetAccessRule(path, false);
                var backup = new TM.SQLServer.Backup(path);
                backup.BackingAll(SqlServer.Connection);
                //InsertDirectory(TM.Common.Directories.DBBak + "test\\abc");
                var a = InsertDirectoriesFiles(path);
                this.success("thành công");
            }
            catch (Exception ex)
            {
                this.danger(ex.Message);
            }
            finally
            {
                SqlServer.Close();
            }
            return RedirectToAction("Index");
        }
    }
}