using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Portal.Controllers
{
    public class ExcelController : BaseController
    {
        // GET: Excel
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult UploadFile()
        {
            //TM.OleExcel.DataSource = Server.MapPath("~/Downloads/Data/text.xls");
            //DataSet ds = TM.OleExcel.GetDataSet();
            //var tablename = TM.OleExcel.GetTableNameList();
            //System.Data.DataTable dt = TM.OleExcel.SelectWhere(tablename[0], "id=1");
            //System.Data.DataTable dt1 = TM.OleExcel.Connection().TMDBTable(tablename[0]).TMDBWhere("id<>1").TMDBGet();
            //var a = TM.OleExcel.Connection().TMDBTable(tablename[0]).TMDBInsert(new string[] { "22", "24", "55", "99" });

            //var a = TM.Excel.ToList(Server.MapPath("~/Downloads/Data/text.xls"));
            //DataTable dt = TM.OleExcel.Select(tablename[0]);
            //var a = TM.Excel.ToObject(Server.MapPath("~/Downloads/Data/text.xls"));
            //List<object> lst = new List<object>();
            //DataTable dt = TM.Excel.ToDataTable(Server.MapPath("~/Downloads/Data/text.xls"));
            TM.Interop.ExcelStatic.DataSource = Server.MapPath("~/Downloads/Data/text.xls");
            DataTable dt = TM.Interop.ExcelStatic.ToDataTable();
            //Clean up
            //wb.Close(false, thisFileName, null);
            //System.Runtime.InteropServices.Marshal.ReleaseComObject(wb);

            return RedirectToAction("Index");
        }
        public ActionResult Save()
        {
            string excelFile = Server.MapPath("~/Downloads/Data/text.xls");
            using (TM.ExcelWriter writer = new TM.ExcelWriter(excelFile))
            {
                DataSet ds = new DataSet();
                DataTable dt1 = new DataTable();
                DataTable dt2 = new DataTable();
                ds.Tables.Add(dt1);
                ds.Tables.Add(dt2);
                writer.WriteStartDocument();
                foreach (System.Data.DataTable table in ds.Tables)
                {
                    writer.WriteStartWorksheet(string.Format("{0}", table.TableName)); // Write the worksheet contents
                    writer.WriteStartRow(); //Write header row
                    foreach (DataColumn col in table.Columns)
                        writer.WriteExcelUnstyledCell(col.Caption);
                    writer.WriteEndRow();
                    foreach (DataRow row in table.Rows)
                    { //write data
                        writer.WriteStartRow();
                        foreach (object o in row.ItemArray)
                            writer.WriteExcelAutoStyledCell(o);
                        writer.WriteEndRow();
                    }
                    writer.WriteEndWorksheet(); // Close up the document
                }
                writer.WriteEndDocument();
                writer.Close();
            }
            return RedirectToAction("Index");
        }
    }
}