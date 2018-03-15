using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TM.Message;
using PagedList;

namespace Portal.Areas.dvcntt.Controllers
{
    public class ListCNTTController : Portal.Controllers.BaseController
    {
        [Filters.AuthVinaphone(Role = Authentication.Roles.superadmin + "," + Authentication.Roles.admin + "," + Authentication.Roles.staff)]
        // GET: dvcntt/ListCNTT
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

                var rs = (from d in db.items
                          join g in db.groups on d.id_key equals g.id.ToString()
                          join l in db.locals on d.url equals l.id.ToString()
                          join u in db.users on d.extras equals u.id.ToString()
                          where d.app_key == Common.Objects.groups.ca
                          select new
                          {
                              id = d.id,
                              appkey = d.app_key,
                              idkey = d.id_key,
                              codekey = d.code_key,
                              title = d.title,
                              desc = d.desc,
                              quantity = d.quantity,
                              quantitytotal = d.quantity_total,
                              price = g.parent_id,
                              priceToken = g.parents_id,
                              images = d.images,
                              url = d.url,
                              author = d.author,
                              attach = d.attach,
                              startedat = d.started_at,
                              endedat = d.ended_at,
                              createdby = d.created_by,
                              createdat = d.created_at,
                              updatedby = d.updated_by,
                              updatedat = d.updated_at,
                              flag = d.flag,
                              extras = d.extras,
                              groupname = g.title,
                              donvi = l.title,
                              level = g.level,
                              staff = u.full_name
                          }).ToList().AsEnumerable();

                if (!String.IsNullOrEmpty(searchString))
                    rs = rs.Where(d =>
                    d.title.Contains(searchString) ||
                    d.codekey.Contains(searchString) ||
                    d.extras.Contains(searchString) ||
                    d.donvi.Contains(searchString));

                if (!String.IsNullOrEmpty(datetime))
                {
                    var date = datetime.Split('-');
                    if (date.Length > 1)
                    {
                        var dateStart = TM.Format.Formating.StartOfDate(TM.Format.Formating.DateParseExactVNToEN(date[0]));
                        var dateEnd = TM.Format.Formating.EndOfDate(TM.Format.Formating.DateParseExactVNToEN(date[1]));
                        rs = datetimeType == 0 ? rs.Where(d => d.createdat >= dateStart && d.createdat <= dateEnd) : rs.Where(d => d.updatedat >= dateStart && d.updatedat <= dateEnd);
                    }
                }

                if (flag == 0) rs = rs.Where(d => d.flag == 0);
                else rs = rs.Where(d => d.flag > 0);

                switch (order)
                {
                    case "codekey_asc":
                        rs = rs.OrderBy(d => d.codekey);
                        break;
                    case "codekey_desc":
                        rs = rs.OrderByDescending(d => d.codekey);
                        break;
                    case "title_asc":
                        rs = rs.OrderBy(d => d.title);
                        break;
                    case "title_desc":
                        rs = rs.OrderByDescending(d => d.title);
                        break;
                    case "extras_asc":
                        rs = rs.OrderBy(d => d.extras);
                        break;
                    case "extras_desc":
                        rs = rs.OrderByDescending(d => d.extras);
                        break;
                    case "level_asc":
                        rs = rs.OrderBy(d => d.attach);
                        break;
                    case "level_desc":
                        rs = rs.OrderByDescending(d => d.attach);
                        break;
                    case "quantity_asc":
                        rs = rs.OrderBy(d => d.quantity);
                        break;
                    case "quantity_desc":
                        rs = rs.OrderByDescending(d => d.quantity);
                        break;
                    case "price_asc":
                        rs = rs.OrderBy(d => d.price);
                        break;
                    case "price_desc":
                        rs = rs.OrderByDescending(d => d.price);
                        break;
                    case "donvi_asc":
                        rs = rs.OrderBy(d => d.donvi);
                        break;
                    case "donvi_desc":
                        rs = rs.OrderByDescending(d => d.donvi);
                        break;
                    case "createdat_asc":
                        rs = rs.OrderBy(d => d.createdat);
                        break;
                    default:
                        rs = rs.OrderByDescending(d => d.createdat);
                        break;
                }

                //Export to any
                if (!String.IsNullOrEmpty(export))
                {
                    TM.Exports.ExportExcel(TM.Helper.Data.ToDataTable(rs.ToList()), "Danh sách CA");
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

        [Filters.AuthVinaphone(Role = Authentication.Roles.superadmin + "," + Authentication.Roles.admin)]
        public ActionResult Import()
        {
            return View();
        }

        [Filters.AuthVinaphone(Role = Authentication.Roles.superadmin + "," + Authentication.Roles.admin)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ImportIVAN()
        {
            try
            {
                var file = TM.IO.FileDirectory.Upload(Request.Files, TM.Common.Directories.orther, false);
                if (file.UploadFileString().Length < 1)
                    return RedirectToAction("Import");
                var excel = new TM.Interop.Excel(TM.IO.FileDirectory.MapPath(TM.Common.Directories.orther + file.UploadFile()[0]));
                var data = excel.ToList();
                

                this.success("Nhập liệu thành công!");
                return RedirectToAction("Import");
            }
            catch (Exception)
            {
                this.danger("Nhập liệu thất bại!");
            }
            return RedirectToAction("Import");
        }
    }
}
