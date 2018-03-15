using InvoiceTool;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GetXMLData
{
    // Read an entire standard DBF file into a DataTable
    public class ParseDBF
    {
        // This is the file header for a DBF. We do this special layout with everything
        // packed so we can read straight from disk into the structure to populate it
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        private struct DBFHeader
        {
            public byte version;
            public byte updateYear;
            public byte updateMonth;
            public byte updateDay;
            public Int32 numRecords;
            public Int16 headerLen;
            public Int16 recordLen;
            public Int16 reserved1;
            public byte incompleteTrans;
            public byte encryptionFlag;
            public Int32 reserved2;
            public Int64 reserved3;
            public byte MDX;
            public byte language;
            public Int16 reserved4;
        }

        // This is the field descriptor structure. There will be one of these for each column in the table.
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        private struct FieldDescriptor
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
            public string fieldName;
            public char fieldType;
            public Int32 address;
            public byte fieldLen;
            public byte count;
            public Int16 reserved1;
            public byte workArea;
            public Int16 reserved2;
            public byte flag;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
            public byte[] reserved3;
            public byte indexFlag;
        }

        public List<string> lstKey = new List<string>();
        private string[] ss;
        Int64 id = 0;
        long totalOK = 0;
        long totalErr = 0;
        long totalFkey = 0; //Đếm số fkey trong file fkey.txt có trong file .DBF
        string[] kqFkey;
        StringBuilder ok = new StringBuilder();
        StringBuilder err = new StringBuilder();
        public XmlSchemaValidator validator = new XmlSchemaValidator();
        // Read an entire standard DBF file into a DataTable
        public DataTable ReadDBF(string dbfFile, String outfilePath, String errfilePath, int mode, bool isTCVN3, string ky_cuoc, string billTime, string[] sFkey)
        {
            kqFkey = sFkey;
            System.Text.UTF8Encoding encoding = new UTF8Encoding();
            long start = DateTime.Now.Ticks;
            DataTable dt = new DataTable();
            BinaryReader recReader;
            string number;
            string year;
            string month;
            string day;
            long lDate;
            long lTime;
            DataRow row;
            int fieldIndex;

            //Create StringBuilder to Store Invoice Data
            //StringBuilder sb = new StringBuilder();
            if (File.Exists(outfilePath)) File.Delete(outfilePath);
            if (File.Exists(errfilePath)) File.Delete(errfilePath);
            Stream errfile = new FileStream(errfilePath, FileMode.Create, FileAccess.ReadWrite);
            err.Append("<Invoices>").AppendLine();
            using (Stream outfile = new FileStream(outfilePath, FileMode.Create, FileAccess.ReadWrite))
            {
                ok.Append("<Invoices><BillTime>" + billTime + "</BillTime>").AppendLine();
                // If there isn't even a file, just return an empty DataTable
                if ((false == File.Exists(dbfFile)))
                {
                    return dt;
                }

                BinaryReader br = null;

                try
                {
                    // Read the header into a buffer
                    br = new BinaryReader(File.OpenRead(dbfFile));
                    byte[] buffer = br.ReadBytes(Marshal.SizeOf(typeof(DBFHeader)));

                    // Marshall the header into a DBFHeader structure
                    GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                    DBFHeader header = (DBFHeader)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(DBFHeader));
                    handle.Free();

                    //Khoi tao mang ss
                    ss = new string[header.numRecords];
                    for (int x = 0; x < ss.Length; x++)
                    {
                        ss[x] = "";
                    }

                    // Read in all the field descriptors. Per the spec, 13 (0D) marks the end of the field descriptors
                    ArrayList fields = new ArrayList();
                    while ((13 != br.PeekChar()))
                    {
                        buffer = br.ReadBytes(Marshal.SizeOf(typeof(FieldDescriptor)));
                        handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                        fields.Add((FieldDescriptor)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(FieldDescriptor)));
                        handle.Free();
                    }

                    // Read in the first row of records, we need this to help determine column types below
                    ((FileStream)br.BaseStream).Seek(header.headerLen + 1, SeekOrigin.Begin);
                    buffer = br.ReadBytes(header.recordLen);
                    recReader = new BinaryReader(new MemoryStream(buffer));

                    // Create the columns in our new DataTable
                    DataColumn col = null;
                    foreach (FieldDescriptor field in fields)
                    {
                        number = Encoding.ASCII.GetString(recReader.ReadBytes(field.fieldLen));
                        switch (field.fieldType)
                        {
                            case 'N':
                                if (number.IndexOf(".") > -1)
                                {
                                    col = new DataColumn(field.fieldName, typeof(decimal));
                                }
                                else
                                {
                                    col = new DataColumn(field.fieldName, typeof(int));
                                }
                                break;
                            case 'C':
                                col = new DataColumn(field.fieldName, typeof(string));
                                break;
                            case 'T':
                                // You can uncomment this to see the time component in the grid
                                //col = new DataColumn(field.fieldName, typeof(string));
                                col = new DataColumn(field.fieldName, typeof(DateTime));
                                break;
                            case 'D':
                                col = new DataColumn(field.fieldName, typeof(DateTime));
                                break;
                            case 'L':
                                col = new DataColumn(field.fieldName, typeof(bool));
                                break;
                            case 'F':
                                col = new DataColumn(field.fieldName, typeof(Double));
                                break;
                        }
                        dt.Columns.Add(col);
                    }

                    // Skip past the end of the header. 
                    ((FileStream)br.BaseStream).Seek(header.headerLen, SeekOrigin.Begin);

                    // Read in all the records
                    for (int counter = 0; counter <= header.numRecords - 1; counter++)
                    {
                        // First we'll read the entire record into a buffer and then read each field from the buffer
                        // This helps account for any extra space at the end of each record and probably performs better
                        buffer = br.ReadBytes(header.recordLen);
                        recReader = new BinaryReader(new MemoryStream(buffer));

                        // All dbf field records begin with a deleted flag field. Deleted - 0x2A (asterisk) else 0x20 (space)
                        try
                        {
                            if (recReader.ReadChar() == '*')
                            {
                                continue;
                            }
                        }
                        catch (Exception ex)
                        {
                            break;
                        }


                        // Loop through each field in a record
                        fieldIndex = 0;

                        row = dt.NewRow();
                        foreach (FieldDescriptor field in fields)
                        {
                            //string value = "";
                            switch (field.fieldType)
                            {
                                case 'N':  // Number
                                    // If you port this to .NET 2.0, use the Decimal.TryParse method
                                    number = Encoding.ASCII.GetString(recReader.ReadBytes(field.fieldLen));
                                    if (number.IndexOf("-") > -1)
                                    {
                                        number = number.Replace("-","");
                                    }

                                    if (IsNumber(number))
                                    {
                                        if (number.IndexOf(".") > -1)
                                        {
                                            row[fieldIndex] = decimal.Parse(number);
                                        }
                                        else
                                        {
                                            row[fieldIndex] = int.Parse(number);
                                        }
                                    }
                                    else
                                    {
                                        row[fieldIndex] = 0;
                                    }

                                    break;

                                case 'C': // String
                                    row[fieldIndex] = Encoding.Default.GetString(recReader.ReadBytes(field.fieldLen));
                                    break;

                                case 'D': // Date (YYYYMMDD)
                                    year = Encoding.ASCII.GetString(recReader.ReadBytes(4));
                                    month = Encoding.ASCII.GetString(recReader.ReadBytes(2));
                                    day = Encoding.ASCII.GetString(recReader.ReadBytes(2));
                                    row[fieldIndex] = System.DBNull.Value;
                                    try
                                    {
                                        if (IsNumber(year) && IsNumber(month) && IsNumber(day))
                                        {
                                            if ((Int32.Parse(year) > 1900))
                                            {
                                                row[fieldIndex] = new DateTime(Int32.Parse(year), Int32.Parse(month), Int32.Parse(day));
                                            }
                                        }
                                    }
                                    catch
                                    { }

                                    break;

                                case 'T': // Timestamp, 8 bytes - two integers, first for date, second for time
                                    // Date is the number of days since 01/01/4713 BC (Julian Days)
                                    // Time is hours * 3600000L + minutes * 60000L + Seconds * 1000L (Milliseconds since midnight)
                                    lDate = recReader.ReadInt32();
                                    lTime = recReader.ReadInt32() * 10000L;
                                    row[fieldIndex] = JulianToDateTime(lDate).AddTicks(lTime);
                                    break;

                                case 'L': // Boolean (Y/N)
                                    if ('Y' == recReader.ReadByte())
                                    {
                                        row[fieldIndex] = true;
                                    }
                                    else
                                    {
                                        row[fieldIndex] = false;
                                    }

                                    break;

                                case 'F':
                                    number = Encoding.ASCII.GetString(recReader.ReadBytes(field.fieldLen));
                                    if (IsNumber(number))
                                    {
                                        row[fieldIndex] = double.Parse(number);
                                    }
                                    else
                                    {
                                        row[fieldIndex] = 0.0F;
                                    }
                                    break;

                            }
                            fieldIndex++;
                        }

                        recReader.Close();

                        GetInvData(row, outfile, errfile, mode, isTCVN3, ky_cuoc);
                    }

                    if (totalOK > 0)
                    {
                        ok.Append("</Invoices>");
                        outfile.Write(Encoding.UTF8.GetBytes(ok.ToString()), 0, Encoding.UTF8.GetByteCount(ok.ToString()));
                        outfile.Close();
                    }
                    else
                    {
                        outfile.Close();
                        if (File.Exists(outfilePath)) File.Delete(outfilePath);
                    }

                    if (totalErr > 0)
                    {
                        err.Append("</Invoices>");
                        errfile.Write(Encoding.UTF8.GetBytes(err.ToString()), 0, Encoding.UTF8.GetByteCount(err.ToString()));
                        errfile.Close();
                    }
                    else
                    {
                        errfile.Close();
                        if (File.Exists(errfilePath)) File.Delete(errfilePath);
                    }

                    string notFoundFkey = "";
                    if (totalFkey > 0)
                    {
                        for (int i = 0; i < kqFkey.Length; i++)
                        {
                            if (!kqFkey[i].Equals(""))
                            {
                                notFoundFkey = notFoundFkey + ",";
                            }
                        }
                    }
                    int dem = 0;
                    dem++;
                }

                catch (Exception ex)
                {
                    errfile.Close();
                    if (File.Exists(errfilePath)) File.Delete(errfilePath);
                    outfile.Close();
                    if (File.Exists(outfilePath)) File.Delete(outfilePath);
                    throw;
                }
                finally
                {
                    if (null != br)
                    {
                        br.Close();
                    }
                }
            }
            long count = DateTime.Now.Ticks - start;
            return dt;
        }

        private int search(string[] ss, string sf)
        {
            BoyerMoore bm = new BoyerMoore(sf);
            int total = 0;
            //Parallel.For(0, ss.Length, new ParallelOptions { MaxDegreeOfParallelism = 4 }, (i, loopState) =>
            Parallel.For(0, ss.Length, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount * 2 }, (i, loopState) =>
            {
                string str = ss[i];
                if (str.Equals(""))
                {
                    loopState.Stop();
                    return;
                }
                //if (str.Replace(sf, String.Empty).Length == 0)
                //{
                //    total = 1;
                //    loopState.Stop();
                //    return;
                //}
                if (bm.Search(str, 0) == 0)
                {
                    total = 1;
                    loopState.Stop();
                    return;
                }
            }
            );
            return total;
        }

        private void GetInvData(DataRow drow, Stream outfile, Stream errfile, int mode, bool isTCVN3, string ky_cuoc)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                string rv = "";
                string fKey = "";
                string fKeyCheck = "";
                string row_ma_kh = drow["ma_kh"].ToString().Trim();
                string row_ma_tb = drow["ma_tb"].ToString().Trim();
                string row_ky_cuoc = drow["ky_cuoc"].ToString().Replace("/", "").Trim();

                fKey = string.Format("{0}{1}", row_ma_kh, row_ky_cuoc);
                fKeyCheck = row_ma_kh.ToUpper();
                
                if (!row_ky_cuoc.Equals(ky_cuoc))
                {
                    rv = string.Format("<Inv><key>{0}</key><ERR>{1}</ERR></Inv>", fKey, "Sai kỳ cước");
                    totalErr++;
                    if (totalErr % 10000 == 0)
                    {
                        errfile.Write(Encoding.UTF8.GetBytes(err.ToString()), 0, Encoding.UTF8.GetByteCount(err.ToString()));
                        errfile.Flush();
                        err.Clear();
                        return;
                    }
                    err.Append(rv).AppendLine();
                    return;
                }

                //Kiểm tra fkey trong file fkey.txt. Nếu không có thì không lấy
                if (search(kqFkey, fKey) == 0)
                {
                    return;
                }

                ////Kiểm tra fkey trong file fkey.txt. Nếu có thì không lấy
                //if (search(kqFkey, fKey) > 0)
                //{
                //    for (int i = 0; i < kqFkey.Length; i++)
                //    {
                //        if (kqFkey[i].Equals(fKeyCheck))
                //        {
                //            kqFkey[i] = "";
                //        }
                //    }
                //    totalFkey++;
                //    return;
                //}

                if (search(ss, fKeyCheck) > 0)
                {
                    rv = string.Format("<Inv><key>{0}</key><ERR>{1}</ERR></Inv>", fKey, "Trùng hóa đơn");
                    totalErr++;
                    if (totalErr % 10000 == 0)
                    {
                        errfile.Write(Encoding.UTF8.GetBytes(err.ToString()), 0, Encoding.UTF8.GetByteCount(err.ToString()));
                        errfile.Flush();
                        err.Clear();
                        return;
                    }
                    err.Append(rv).AppendLine();
                    return;
                }
                else
                {
                    ss[id] = fKeyCheck;
                    id++;
                }

                // Nếu Cước chuyển vùng quốc tế và thu khác không chịu thuế > 0 thì tách thành 2 hóa đơn
                bool b2HD = false;
                if (!drow["cuoc_kthue"].ToString().Trim().Equals(""))
                {
                    if (Int64.Parse(drow["cuoc_kthue"].ToString().Trim()) > 0) b2HD = true;
                }

                if (b2HD) // 2 hóa đơn
                {
                    #region Hóa đơn 1
                    sb = new StringBuilder();
                    sb.Append("<Inv><key>");
                    string diachi_tt = drow["diachi_tt"].ToString().Trim();
                    string ten_tt = drow["ten_tt"].ToString().Trim();
                    if (isTCVN3)
                    {
                        diachi_tt = Util.convertSpecialCharacter(Util.convertTCVN3ToUnicode(diachi_tt));
                        ten_tt = Util.convertSpecialCharacter(Util.convertTCVN3ToUnicode(ten_tt));
                    }
                    else
                    {
                        diachi_tt = Util.convertSpecialCharacter(diachi_tt);
                        ten_tt = Util.convertSpecialCharacter(ten_tt);
                    }
                    sb.Append(fKey);
                    sb.Append("</key><Invoice><CusCode>");
                    sb.Append(Util.convertSpecialCharacter(drow["ma_kh"].ToString().Trim()));
                    sb.Append("</CusCode><CusName>");
                    sb.Append(ten_tt);
                    sb.Append("</CusName><CusAddress>");
                    sb.Append(diachi_tt);
                    sb.Append("</CusAddress><CusPhone>");
                    sb.Append(Util.convertSpecialCharacter(drow["ma_tb"].ToString().Trim()));
                    sb.Append("</CusPhone><CusTaxCode>");
                    sb.Append(drow["ms_thue"].ToString().Replace(" ", string.Empty).Trim());
                    sb.Append("</CusTaxCode><PaymentMethod>");
                    //sb.Append(Util.convertSpecialCharacter(drow["httt"].ToString().Trim()));
                    sb.Append("TM/CK</PaymentMethod><KindOfService>");
                    sb.Append("Ky cuoc " + drow["ky_cuoc"].ToString().Trim());
                    sb.Append("</KindOfService><Products>");

                    sb.Append("<Product><ProdName>");
                    sb.Append("A. Cuoc cac dich vu VT-CNTT");
                    sb.Append("</ProdName><ProdUnit></ProdUnit><ProdQuantity></ProdQuantity><ProdPrice></ProdPrice><Amount>");
                    //sb.Append(drow["cuoc_psdv"].ToString().Trim());
                    sb.Append("</Amount></Product>");

                    sb.Append("<Product><ProdName>");
                    sb.Append("Cac khoan chiu thue");
                    sb.Append("</ProdName><ProdUnit></ProdUnit><ProdQuantity></ProdQuantity><ProdPrice></ProdPrice><Amount>");
                    sb.Append(drow["cuoc_cthue"].ToString().Trim());
                    sb.Append("</Amount></Product>");

                    // Tổng tiền thanh toán
                    Int64 i_Amount = 0;
                    try
                    {
                        i_Amount = Int64.Parse(drow["cuoc_cthue"].ToString().Trim()) + Int64.Parse(drow["thue"].ToString().Trim());
                    }
                    catch { }
                    // Các khoản giảm trừ
                    Int64 iGiamTru = 0;
                    try
                    {
                        iGiamTru = Int64.Parse(drow["cuoc_gtru"].ToString().Trim());
                    }
                    catch { }
                    // Còn phải thanh toán
                    Int64 i_ConThanhToan = i_Amount - iGiamTru;
                        
                    sb.Append("</Products><Total>");
                    sb.Append(drow["cuoc_cthue"].ToString().Trim());
                    sb.Append("</Total><DiscountAmount></DiscountAmount><VATRate>10</VATRate><VATAmount>");
                    sb.Append(drow["thue"].ToString().Trim());
                    sb.Append("</VATAmount><Amount>");
                    sb.Append(i_Amount);
                    sb.Append("</Amount><AmountInWords>");
                    string tienBangChu = "";
                    NumberToLeter numToLetter = new NumberToLeter();
                    tienBangChu = numToLetter.DocTienBangChu(i_Amount, tienBangChu);
                    sb.Append(tienBangChu);
                    sb.Append("</AmountInWords>");

                    // Extra
                    sb.Append("<Extra>");
                    sb.Append(iGiamTru); // Các khoản giảm trừ
                    sb.Append(";");
                    sb.Append(i_ConThanhToan); // Còn phải thanh toán
                    sb.Append("</Extra>");

                    sb.Append("</Invoice></Inv>");

                    //Kiem tra du lieu xml
                    rv = sb.ToString();
                    if (!validator.ValidXmlDoc(rv, "", @"XMLValidate\VATInVoice.xsd"))
                    {
                        rv = string.Format("<Inv><key>{0}</key><ERR>{1}</ERR></Inv>", fKey, validator.ValidationError);
                        totalErr++;
                        if (totalErr % 10000 == 0)
                        {
                            errfile.Write(Encoding.UTF8.GetBytes(err.ToString()), 0, Encoding.UTF8.GetByteCount(err.ToString()));
                            errfile.Flush();
                            err.Clear();
                        }
                        err.Append(rv).AppendLine();
                        return;
                    }

                    totalOK++;
                    if (totalOK % 10000 == 0)
                    {
                        outfile.Write(Encoding.UTF8.GetBytes(ok.ToString()), 0, Encoding.UTF8.GetByteCount(ok.ToString()));
                        outfile.Flush();
                        ok.Clear();
                    }
                    ok.Append(rv).AppendLine();
                    #endregion

                    #region Hóa đơn 2
                    sb = new StringBuilder();
                    sb.Append("<Inv><key>");
                    sb.Append(fKey + "K"); // Thêm "K" vào sau fkey cho hóa đơn thứ 2
                    sb.Append("</key><Invoice><CusCode>");
                    sb.Append(Util.convertSpecialCharacter(drow["ma_kh"].ToString().Trim()));
                    sb.Append("</CusCode><CusName>");
                    sb.Append(ten_tt);
                    sb.Append("</CusName><CusAddress>");
                    sb.Append(diachi_tt);
                    sb.Append("</CusAddress><CusPhone>");
                    sb.Append(Util.convertSpecialCharacter(drow["ma_tb"].ToString().Trim()));
                    sb.Append("</CusPhone><CusTaxCode>");
                    sb.Append(drow["ms_thue"].ToString().Replace(" ", string.Empty).Trim());
                    sb.Append("</CusTaxCode><PaymentMethod>");
                    //sb.Append(Util.convertSpecialCharacter(drow["httt"].ToString().Trim()));
                    sb.Append("TM/CK</PaymentMethod><KindOfService>");
                    sb.Append("Ky cuoc " + drow["ky_cuoc"].ToString().Trim());
                    sb.Append("</KindOfService><Products>");

                    sb.Append("<Product><ProdName>");
                    sb.Append("A. Cuoc cac dich vu VT-CNTT");
                    sb.Append("</ProdName><ProdUnit></ProdUnit><ProdQuantity></ProdQuantity><ProdPrice></ProdPrice><Amount>");
                    //sb.Append(drow["cuoc_psdv"].ToString().Trim());
                    sb.Append("</Amount></Product>");

                    sb.Append("<Product><ProdName>");
                    sb.Append("Cuoc chuyen vung quoc te va thu khac khong chiu thue");
                    sb.Append("</ProdName><ProdUnit></ProdUnit><ProdQuantity></ProdQuantity><ProdPrice></ProdPrice><Amount>");
                    sb.Append(drow["cuoc_kthue"].ToString().Trim());
                    sb.Append("</Amount></Product>");

                    sb.Append("</Products><Total>");
                    sb.Append(drow["cuoc_kthue"].ToString().Trim());
                    sb.Append("</Total><DiscountAmount></DiscountAmount><VATRate>-1</VATRate><VATAmount>");
                    sb.Append("</VATAmount><Amount>");
                    sb.Append(drow["cuoc_kthue"].ToString().Trim());
                    sb.Append("</Amount><AmountInWords>");
                    long tongtien;
                    tienBangChu = "";
                    numToLetter = new NumberToLeter();
                    if (Int64.TryParse(drow["cuoc_kthue"].ToString().Trim(), out tongtien))
                        tienBangChu = numToLetter.DocTienBangChu(tongtien, tienBangChu);
                    sb.Append(tienBangChu);
                    sb.Append("</AmountInWords>");

                    // Extra
                    sb.Append("<Extra>0;");
                    sb.Append(drow["cuoc_kthue"].ToString().Trim());
                    sb.Append("</Extra>");

                    sb.Append("</Invoice></Inv>");

                    //Kiem tra du lieu xml
                    rv = sb.ToString();
                    if (!validator.ValidXmlDoc(rv, "", @"XMLValidate\VATInVoice.xsd"))
                    {
                        rv = string.Format("<Inv><key>{0}</key><ERR>{1}</ERR></Inv>", fKey, validator.ValidationError);
                        totalErr++;
                        if (totalErr % 10000 == 0)
                        {
                            errfile.Write(Encoding.UTF8.GetBytes(err.ToString()), 0, Encoding.UTF8.GetByteCount(err.ToString()));
                            errfile.Flush();
                            err.Clear();
                        }
                        err.Append(rv).AppendLine();
                        return;
                    }

                    totalOK++;
                    if (totalOK % 10000 == 0)
                    {
                        outfile.Write(Encoding.UTF8.GetBytes(ok.ToString()), 0, Encoding.UTF8.GetByteCount(ok.ToString()));
                        outfile.Flush();
                        ok.Clear();
                    }
                    ok.Append(rv).AppendLine();
                    #endregion
                }
                else // 1 hóa đơn
                {
                    #region 1 hóa đơn
                    sb = new StringBuilder();
                    sb.Append("<Inv><key>");
                    string diachi_tt = drow["diachi_tt"].ToString().Trim();
                    string ten_tt = drow["ten_tt"].ToString().Trim();
                    if (isTCVN3)
                    {
                        diachi_tt = Util.convertSpecialCharacter(Util.convertTCVN3ToUnicode(diachi_tt));
                        ten_tt = Util.convertSpecialCharacter(Util.convertTCVN3ToUnicode(ten_tt));
                    }
                    else
                    {
                        diachi_tt = Util.convertSpecialCharacter(diachi_tt);
                        ten_tt = Util.convertSpecialCharacter(ten_tt);
                    }
                    sb.Append(fKey);
                    sb.Append("</key><Invoice><CusCode>");
                    sb.Append(Util.convertSpecialCharacter(drow["ma_kh"].ToString().Trim()));
                    sb.Append("</CusCode><CusName>");
                    sb.Append(ten_tt);
                    sb.Append("</CusName><CusAddress>");
                    sb.Append(diachi_tt);
                    sb.Append("</CusAddress><CusPhone>");
                    sb.Append(Util.convertSpecialCharacter(drow["ma_tb"].ToString().Trim()));
                    sb.Append("</CusPhone><CusTaxCode>");
                    sb.Append(drow["ms_thue"].ToString().Replace(" ", string.Empty).Trim());
                    sb.Append("</CusTaxCode><PaymentMethod>");
                    //sb.Append(Util.convertSpecialCharacter(drow["httt"].ToString().Trim()));
                    sb.Append("TM/CK</PaymentMethod><KindOfService>");
                    sb.Append("Ky cuoc " + drow["ky_cuoc"].ToString().Trim());
                    sb.Append("</KindOfService><Products>");


                    sb.Append("<Product><ProdName>");
                    sb.Append("A. Cuoc cac dich vu VT-CNTT");
                    sb.Append("</ProdName><ProdUnit></ProdUnit><ProdQuantity></ProdQuantity><ProdPrice></ProdPrice><Amount>");
                    //sb.Append(drow["cuoc_psdv"].ToString().Trim());
                    sb.Append("</Amount></Product>");

                    sb.Append("<Product><ProdName>");
                    sb.Append("Cac khoan chiu thue");
                    sb.Append("</ProdName><ProdUnit></ProdUnit><ProdQuantity></ProdQuantity><ProdPrice></ProdPrice><Amount>");
                    sb.Append(drow["cuoc_cthue"].ToString().Trim());
                    sb.Append("</Amount></Product>");

                    // Các khoản giảm trừ
                    Int64 iGiamTru = 0;
                    try
                    {
                        iGiamTru = Int64.Parse(drow["cuoc_gtru"].ToString().Trim());
                    }
                    catch { }
                    // Còn phải thanh toán
                    Int64 i_ConThanhToan = 0;
                    try
                    {
                        i_ConThanhToan = Int64.Parse(drow["cuoc_ptra"].ToString().Trim());
                    }
                    catch { }

                    sb.Append("</Products><Total>");
                    sb.Append(drow["cuoc_cthue"].ToString().Trim());
                    sb.Append("</Total><DiscountAmount></DiscountAmount><VATRate>10</VATRate><VATAmount>");
                    sb.Append(drow["thue"].ToString().Trim());
                    sb.Append("</VATAmount><Amount>");
                    sb.Append(drow["cuoc_tt"].ToString().Trim());
                    sb.Append("</Amount><AmountInWords>");
                    long tongtien;
                    string tienBangChu = "";
                    NumberToLeter numToLetter = new NumberToLeter();
                    if (Int64.TryParse(drow["cuoc_tt"].ToString().Trim(), out tongtien))
                        tienBangChu = numToLetter.DocTienBangChu(tongtien, tienBangChu);
                    sb.Append(tienBangChu);
                    sb.Append("</AmountInWords>");

                    // Extra
                    sb.Append("<Extra>");
                    sb.Append(iGiamTru); // Các khoản giảm trừ
                    sb.Append(";");
                    sb.Append(i_ConThanhToan); // Còn phải thanh toán
                    sb.Append("</Extra>");

                    sb.Append("</Invoice></Inv>");

                    //Kiem tra du lieu xml
                    rv = sb.ToString();
                    if (!validator.ValidXmlDoc(rv, "", @"XMLValidate\VATInVoice.xsd"))
                    {
                        rv = string.Format("<Inv><key>{0}</key><ERR>{1}</ERR></Inv>", fKey, validator.ValidationError);
                        totalErr++;
                        if (totalErr % 10000 == 0)
                        {
                            errfile.Write(Encoding.UTF8.GetBytes(err.ToString()), 0, Encoding.UTF8.GetByteCount(err.ToString()));
                            errfile.Flush();
                            err.Clear();
                        }
                        err.Append(rv).AppendLine();
                        return;
                    }

                    totalOK++;
                    if (totalOK % 10000 == 0)
                    {
                        outfile.Write(Encoding.UTF8.GetBytes(ok.ToString()), 0, Encoding.UTF8.GetByteCount(ok.ToString()));
                        outfile.Flush();
                        ok.Clear();
                    }
                    ok.Append(rv).AppendLine();
                    #endregion
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                return;
            }
        }

        //// Read an entire standard DBF file into a DataTable
        //public static DataTable ReadDBF(string dbfFile, String outfilePath, int mode, bool isTCVN3)
        //{
        //    System.Text.UTF8Encoding encoding = new UTF8Encoding();
        //    long start = DateTime.Now.Ticks;
        //    DataTable dt = new DataTable();
        //    BinaryReader recReader;
        //    string number;
        //    string year;
        //    string month;
        //    string day;
        //    long lDate;
        //    long lTime;
        //    DataRow row;
        //    int fieldIndex;

        //    //Create StringBuilder to Store Invoice Data
        //    //StringBuilder sb = new StringBuilder();
        //    File.Delete(outfilePath);
        //    using (StreamWriter outfile2 = new StreamWriter(outfilePath, true, Encoding.UTF8))
        //    {
        //        outfile2.AutoFlush = true;
        //        outfile2.Write("<Invoices>");
        //        // If there isn't even a file, just return an empty DataTable
        //        if ((false == File.Exists(dbfFile)))
        //        {
        //            return dt;
        //        }

        //        BinaryReader br = null;

        //        try
        //        {
        //            // Read the header into a buffer
        //            br = new BinaryReader(File.OpenRead(dbfFile));
        //            byte[] buffer = br.ReadBytes(Marshal.SizeOf(typeof(DBFHeader)));

        //            // Marshall the header into a DBFHeader structure
        //            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
        //            DBFHeader header = (DBFHeader)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(DBFHeader));
        //            handle.Free();

        //            // Read in all the field descriptors. Per the spec, 13 (0D) marks the end of the field descriptors
        //            ArrayList fields = new ArrayList();
        //            while ((13 != br.PeekChar()))
        //            {
        //                buffer = br.ReadBytes(Marshal.SizeOf(typeof(FieldDescriptor)));
        //                handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
        //                fields.Add((FieldDescriptor)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(FieldDescriptor)));
        //                handle.Free();
        //            }

        //            // Read in the first row of records, we need this to help determine column types below
        //            ((FileStream)br.BaseStream).Seek(header.headerLen + 1, SeekOrigin.Begin);
        //            buffer = br.ReadBytes(header.recordLen);
        //            recReader = new BinaryReader(new MemoryStream(buffer));

        //            // Create the columns in our new DataTable
        //            DataColumn col = null;
        //            foreach (FieldDescriptor field in fields)
        //            {
        //                number = Encoding.ASCII.GetString(recReader.ReadBytes(field.fieldLen));
        //                switch (field.fieldType)
        //                {
        //                    case 'N':
        //                        if (number.IndexOf(".") > -1)
        //                        {
        //                            col = new DataColumn(field.fieldName, typeof(decimal));
        //                        }
        //                        else
        //                        {
        //                            col = new DataColumn(field.fieldName, typeof(int));
        //                        }
        //                        break;
        //                    case 'C':
        //                        col = new DataColumn(field.fieldName, typeof(string));
        //                        break;
        //                    case 'T':
        //                        // You can uncomment this to see the time component in the grid
        //                        //col = new DataColumn(field.fieldName, typeof(string));
        //                        col = new DataColumn(field.fieldName, typeof(DateTime));
        //                        break;
        //                    case 'D':
        //                        col = new DataColumn(field.fieldName, typeof(DateTime));
        //                        break;
        //                    case 'L':
        //                        col = new DataColumn(field.fieldName, typeof(bool));
        //                        break;
        //                    case 'F':
        //                        col = new DataColumn(field.fieldName, typeof(Double));
        //                        break;
        //                }
        //                dt.Columns.Add(col);
        //            }

        //            // Skip past the end of the header. 
        //            ((FileStream)br.BaseStream).Seek(header.headerLen, SeekOrigin.Begin);


        //            // Read in all the records
        //            for (int counter = 0; counter <= header.numRecords - 1; counter++)
        //            {
        //                // First we'll read the entire record into a buffer and then read each field from the buffer
        //                // This helps account for any extra space at the end of each record and probably performs better

        //                //if (counter % 10000 == 0)
        //                //{
        //                //    outfile.Write(encoding.GetBytes(sb.ToString()), 0, sb.Length);
        //                //    sb.Clear();
        //                //}

        //                buffer = br.ReadBytes(header.recordLen);
        //                recReader = new BinaryReader(new MemoryStream(buffer));

        //                // All dbf field records begin with a deleted flag field. Deleted - 0x2A (asterisk) else 0x20 (space)
        //                try
        //                {
        //                    if (recReader.ReadChar() == '*')
        //                    {
        //                        continue;
        //                    }
        //                }
        //                catch (Exception ex)
        //                {
        //                    break;
        //                }


        //                // Loop through each field in a record
        //                fieldIndex = 0;

        //                row = dt.NewRow();
        //                foreach (FieldDescriptor field in fields)
        //                {
        //                    //string value = "";
        //                    switch (field.fieldType)
        //                    {
        //                        case 'N':  // Number
        //                            // If you port this to .NET 2.0, use the Decimal.TryParse method
        //                            number = Encoding.ASCII.GetString(recReader.ReadBytes(field.fieldLen));
        //                            if (IsNumber(number))
        //                            {
        //                                if (number.IndexOf(".") > -1)
        //                                {
        //                                    row[fieldIndex] = decimal.Parse(number);
        //                                }
        //                                else
        //                                {
        //                                    row[fieldIndex] = int.Parse(number);
        //                                }
        //                            }
        //                            else
        //                            {
        //                                row[fieldIndex] = 0;
        //                            }

        //                            break;

        //                        case 'C': // String
        //                            row[fieldIndex] = Encoding.Default.GetString(recReader.ReadBytes(field.fieldLen));
        //                            break;

        //                        case 'D': // Date (YYYYMMDD)
        //                            year = Encoding.ASCII.GetString(recReader.ReadBytes(4));
        //                            month = Encoding.ASCII.GetString(recReader.ReadBytes(2));
        //                            day = Encoding.ASCII.GetString(recReader.ReadBytes(2));
        //                            row[fieldIndex] = System.DBNull.Value;
        //                            try
        //                            {
        //                                if (IsNumber(year) && IsNumber(month) && IsNumber(day))
        //                                {
        //                                    if ((Int32.Parse(year) > 1900))
        //                                    {
        //                                        row[fieldIndex] = new DateTime(Int32.Parse(year), Int32.Parse(month), Int32.Parse(day));
        //                                    }
        //                                }
        //                            }
        //                            catch
        //                            { }

        //                            break;

        //                        case 'T': // Timestamp, 8 bytes - two integers, first for date, second for time
        //                            // Date is the number of days since 01/01/4713 BC (Julian Days)
        //                            // Time is hours * 3600000L + minutes * 60000L + Seconds * 1000L (Milliseconds since midnight)
        //                            lDate = recReader.ReadInt32();
        //                            lTime = recReader.ReadInt32() * 10000L;
        //                            row[fieldIndex] = JulianToDateTime(lDate).AddTicks(lTime);
        //                            break;

        //                        case 'L': // Boolean (Y/N)
        //                            if ('Y' == recReader.ReadByte())
        //                            {
        //                                row[fieldIndex] = true;
        //                            }
        //                            else
        //                            {
        //                                row[fieldIndex] = false;
        //                            }

        //                            break;

        //                        case 'F':
        //                            number = Encoding.ASCII.GetString(recReader.ReadBytes(field.fieldLen));
        //                            if (IsNumber(number))
        //                            {
        //                                row[fieldIndex] = double.Parse(number);
        //                            }
        //                            else
        //                            {
        //                                row[fieldIndex] = 0.0F;
        //                            }
        //                            break;

        //                    }
        //                    fieldIndex++;
        //                }

        //                recReader.Close();

        //                GetInvData(row, outfile2, mode, isTCVN3);

        //                //dt.Rows.Add(row);
        //            }

        //            outfile2.Write("</Invoices>");                    
        //            //outfile.Write(encoding.GetBytes(sb.ToString()), 0, sb.Length);
        //        }

        //        catch (Exception ex)
        //        {
        //            throw;

        //        }
        //        finally
        //        {
        //            if (null != br)
        //            {
        //                br.Close();
        //            }
        //        }
        //    }            

        //    long count = DateTime.Now.Ticks - start;

        //    return dt;
        //}

        //private static void GetInvData(DataRow drow, TextWriter sb1, int mode, bool isTCVN3)
        //{
        //    try
        //    {
        //        StringBuilder sb = new StringBuilder();
        //        sb.Append("<Inv><key>");
        //        string fKey = "";
        //        if (mode == 0) //HN
        //        {
        //            fKey = Util.convertSpecialCharacter(drow["ma_kh"].ToString().Trim()) + drow["ky_cuoc"].ToString().Replace("/", "").Trim();
        //        }
        //        else //HCM
        //        {
        //            fKey = Util.convertSpecialCharacter(drow["ma_kh"].ToString().Trim()) + Util.convertSpecialCharacter(drow["ma_tb"].ToString().Trim()) + drow["ky_cuoc"].ToString().Replace("/", "").Trim();
        //        }
        //        string diachi_tt = Util.convertSpecialCharacter(drow["diachi_tt"].ToString().Trim());
        //        string ten_tt = Util.convertSpecialCharacter(drow["ten_tt"].ToString().Trim());
        //        if (isTCVN3)
        //        {
        //            diachi_tt = Util.convertTCVN3ToUnicode(diachi_tt);
        //            ten_tt = Util.convertTCVN3ToUnicode(ten_tt);
        //        }
        //        sb.Append(fKey);
        //        sb.Append("</key><Invoice><CusCode>");
        //        sb.Append(Util.convertSpecialCharacter(drow["ma_kh"].ToString().Trim()));
        //        sb.Append("</CusCode><CusName>");
        //        sb.Append(ten_tt);
        //        sb.Append("</CusName><CusAddress>");
        //        sb.Append(diachi_tt);
        //        sb.Append("</CusAddress><CusPhone>");
        //        sb.Append(Util.convertSpecialCharacter(drow["ma_tb"].ToString().Trim()));
        //        sb.Append("</CusPhone><CusTaxCode>");
        //        sb.Append(drow["ms_thue"].ToString().Replace(" ", string.Empty).Trim());
        //        sb.Append("</CusTaxCode><PaymentMethod>");
        //        //sb.Append(Util.convertSpecialCharacter(drow["httt"].ToString().Trim()));
        //        sb.Append("</PaymentMethod><KindOfService>");
        //        sb.Append("Ky cuoc " + drow["ky_cuoc"].ToString().Trim());
        //        sb.Append("</KindOfService><Products>");

        //        if (mode == 0) //HN
        //        {
        //            sb.Append("<Product><ProdName>");
        //            sb.Append("1. Cac khoan chiu thue");
        //            sb.Append("</ProdName><ProdUnit></ProdUnit><ProdQuantity></ProdQuantity><ProdPrice></ProdPrice><Amount>");
        //            sb.Append(drow["cuoc_cthue"].ToString().Trim());
        //            sb.Append("</Amount></Product>");

        //            sb.Append("<Product><ProdName>");
        //            sb.Append("2. Cac khoan khong chiu thue va thu khac");
        //            sb.Append("</ProdName><ProdUnit></ProdUnit><ProdQuantity></ProdQuantity><ProdPrice></ProdPrice><Amount>");
        //            sb.Append(drow["cuoc_kthue"].ToString().Trim());
        //            sb.Append("</Amount></Product>");

        //            sb.Append("<Product><ProdName>");
        //            sb.Append("3. Khuyen mai (khong thu tien)d");
        //            sb.Append("</ProdName><ProdUnit></ProdUnit><ProdQuantity></ProdQuantity><ProdPrice></ProdPrice><Amount>");
        //            sb.Append(drow["cuoc_km"].ToString().Trim());
        //            sb.Append("</Amount></Product>");

        //            sb.Append("<Product><ProdName>");
        //            sb.Append("4. Cac khoan truy thu giam tru");
        //            sb.Append("</ProdName><ProdUnit></ProdUnit><ProdQuantity></ProdQuantity><ProdPrice></ProdPrice><Amount>");
        //            sb.Append(drow["cuoc_gtru"].ToString().Trim());
        //            sb.Append("</Amount></Product>");
        //        }
        //        else //HCM
        //        {
        //            sb.Append("<Product><ProdName>");
        //            sb.Append("A. Cuoc cac dich vu phat sinh");
        //            sb.Append("</ProdName><ProdUnit></ProdUnit><ProdQuantity></ProdQuantity><ProdPrice></ProdPrice><Amount>");
        //            sb.Append(drow["cuoc_psdv"].ToString().Trim());
        //            sb.Append("</Amount></Product>");

        //            sb.Append("<Product><ProdName>");
        //            sb.Append("B. Cuoc chuyen vung quoc te va thu khac khong chiu thue");
        //            sb.Append("</ProdName><ProdUnit></ProdUnit><ProdQuantity></ProdQuantity><ProdPrice></ProdPrice><Amount>");
        //            sb.Append(drow["cuoc_kthue"].ToString().Trim());
        //            sb.Append("</Amount></Product>");

        //            sb.Append("<Product><ProdName>");
        //            sb.Append("Cac khoan chiu thue");
        //            sb.Append("</ProdName><ProdUnit></ProdUnit><ProdQuantity></ProdQuantity><ProdPrice></ProdPrice><Amount>");
        //            sb.Append(drow["cuoc_cthue"].ToString().Trim());
        //            sb.Append("</Amount></Product>");

        //            sb.Append("<Product><ProdName>");
        //            sb.Append("F. Cac khoan giam tru ( dat coc. so du tai khoan.... )");
        //            sb.Append("</ProdName><ProdUnit></ProdUnit><ProdQuantity></ProdQuantity><ProdPrice></ProdPrice><Amount>");
        //            sb.Append(drow["cuoc_gtru"].ToString().Trim());
        //            sb.Append("</Amount></Product>");

        //            sb.Append("<Product><ProdName>");
        //            sb.Append("Con phai thanh toan");
        //            sb.Append("</ProdName><ProdUnit></ProdUnit><ProdQuantity></ProdQuantity><ProdPrice></ProdPrice><Amount>");
        //            sb.Append(drow["cuoc_ptra"].ToString().Trim());
        //            sb.Append("</Amount></Product>");
        //        }

        //        sb.Append("</Products><Total>");

        //        sb.Append(drow["cuoc_cthue"].ToString().Trim());
        //        sb.Append("</Total><DiscountAmount></DiscountAmount><VATRate>10</VATRate><VATAmount>");
        //        sb.Append(drow["thue"].ToString().Trim());
        //        sb.Append("</VATAmount><Amount>");
        //        sb.Append(drow["cuoc_tt"].ToString().Trim());
        //        sb.Append("</Amount><AmountInWords>");
        //        long tongtien;
        //        string tienBangChu = "";
        //        NumberToLeter numToLetter = new NumberToLeter();
        //        if (Int64.TryParse(drow["cuoc_tt"].ToString().Trim(), out tongtien))
        //            tienBangChu = numToLetter.DocTienBangChu(tongtien, tienBangChu);
        //        sb.Append(tienBangChu);
        //        sb.Append("</AmountInWords>");

        //        if (mode == 0) //HN
        //        {
        //            sb.Append("<PaymentStatus>");
        //            //if (drow["DA_TT"].ToString().Trim() != null)
        //            //{
        //            //    int i = int.Parse(drow["DA_TT"].ToString().Trim());
        //            //    if (i <= 0)
        //            //        sb.Append("-1");
        //            //    else
        //            //        sb.Append("0");
        //            //}
        //            //else
        //            //    sb.Append("0");
        //            sb.Append("</PaymentStatus>");

        //            sb.Append("<Extra>");
        //            //sb.Append(drow["DA_TT"].ToString().Trim());
        //            //sb.Append(";");
        //            //sb.Append(drow["TONG_CON_TT"].ToString().Trim());
        //            sb.Append("</Extra>");
        //        }

        //        sb.Append("</Invoice></Inv>");
        //        sb.AppendLine();

        //        sb1.Write(sb.ToString());
        //    }

        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message);
        //    }
        //}

        public static bool IsNumber(string numberString)
        {
            char[] numbers = numberString.ToCharArray();
            int number_count = 0;
            int point_count = 0;
            int space_count = 0;

            foreach (char number in numbers)
            {
                if ((number >= 48 && number <= 57))
                {
                    number_count += 1;
                }
                else if (number == 46)
                {
                    point_count += 1;
                }
                else if (number == 32)
                {
                    space_count += 1;
                }
                else
                {
                    return false;
                }
            }

            return (number_count > 0 && point_count < 2);
        }

        private static DateTime JulianToDateTime(long lJDN)
        {
            double p = Convert.ToDouble(lJDN);
            double s1 = p + 68569;
            double n = Math.Floor(4 * s1 / 146097);
            double s2 = s1 - Math.Floor((146097 * n + 3) / 4);
            double i = Math.Floor(4000 * (s2 + 1) / 1461001);
            double s3 = s2 - Math.Floor(1461 * i / 4) + 31;
            double q = Math.Floor(80 * s3 / 2447);
            double d = s3 - Math.Floor(2447 * q / 80);
            double s4 = Math.Floor(q / 11);
            double m = q + 2 - 12 * s4;
            double j = 100 * (n - 49) + i + s4;
            return new DateTime(Convert.ToInt32(j), Convert.ToInt32(m), Convert.ToInt32(d));
        }

        #region Xuất dữ liệu khách hàng
        public DataTable ReadDBF_KH(string dbfFile, String outfilePath, String errfilePath, bool isTCVN3, string ky_cuoc)
        {
            System.Text.UTF8Encoding encoding = new UTF8Encoding();
            long start = DateTime.Now.Ticks;
            DataTable dt = new DataTable();
            BinaryReader recReader;
            string number;
            string year;
            string month;
            string day;
            long lDate;
            long lTime;
            DataRow row;
            int fieldIndex;

            //Create StringBuilder to Store Invoice Data
            //StringBuilder sb = new StringBuilder();
            if (File.Exists(outfilePath)) File.Delete(outfilePath);
            if (File.Exists(errfilePath)) File.Delete(errfilePath);
            Stream errfile = new FileStream(errfilePath, FileMode.Create, FileAccess.ReadWrite);
            err.Append("<Customers>").AppendLine();
            using (Stream outfile = new FileStream(outfilePath, FileMode.Create, FileAccess.ReadWrite))
            {
                ok.Append("<Customers>").AppendLine();
                // If there isn't even a file, just return an empty DataTable
                if ((false == File.Exists(dbfFile)))
                {
                    return dt;
                }

                BinaryReader br = null;

                try
                {
                    // Read the header into a buffer
                    br = new BinaryReader(File.OpenRead(dbfFile));
                    byte[] buffer = br.ReadBytes(Marshal.SizeOf(typeof(DBFHeader)));

                    // Marshall the header into a DBFHeader structure
                    GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                    DBFHeader header = (DBFHeader)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(DBFHeader));
                    handle.Free();

                    //Khoi tao mang ss
                    ss = new string[header.numRecords];
                    for (int x = 0; x < ss.Length; x++)
                    {
                        ss[x] = "";
                    }

                    // Read in all the field descriptors. Per the spec, 13 (0D) marks the end of the field descriptors
                    ArrayList fields = new ArrayList();
                    while ((13 != br.PeekChar()))
                    {
                        buffer = br.ReadBytes(Marshal.SizeOf(typeof(FieldDescriptor)));
                        handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                        fields.Add((FieldDescriptor)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(FieldDescriptor)));
                        handle.Free();
                    }

                    // Read in the first row of records, we need this to help determine column types below
                    ((FileStream)br.BaseStream).Seek(header.headerLen + 1, SeekOrigin.Begin);
                    buffer = br.ReadBytes(header.recordLen);
                    recReader = new BinaryReader(new MemoryStream(buffer));

                    // Create the columns in our new DataTable
                    DataColumn col = null;
                    foreach (FieldDescriptor field in fields)
                    {
                        number = Encoding.ASCII.GetString(recReader.ReadBytes(field.fieldLen));
                        switch (field.fieldType)
                        {
                            case 'N':
                                if (number.IndexOf(".") > -1)
                                {
                                    col = new DataColumn(field.fieldName, typeof(decimal));
                                }
                                else
                                {
                                    col = new DataColumn(field.fieldName, typeof(int));
                                }
                                break;
                            case 'C':
                                col = new DataColumn(field.fieldName, typeof(string));
                                break;
                            case 'T':
                                // You can uncomment this to see the time component in the grid
                                //col = new DataColumn(field.fieldName, typeof(string));
                                col = new DataColumn(field.fieldName, typeof(DateTime));
                                break;
                            case 'D':
                                col = new DataColumn(field.fieldName, typeof(DateTime));
                                break;
                            case 'L':
                                col = new DataColumn(field.fieldName, typeof(bool));
                                break;
                            case 'F':
                                col = new DataColumn(field.fieldName, typeof(Double));
                                break;
                        }
                        dt.Columns.Add(col);
                    }

                    // Skip past the end of the header. 
                    ((FileStream)br.BaseStream).Seek(header.headerLen, SeekOrigin.Begin);

                    // Read in all the records
                    for (int counter = 0; counter <= header.numRecords - 1; counter++)
                    {
                        // First we'll read the entire record into a buffer and then read each field from the buffer
                        // This helps account for any extra space at the end of each record and probably performs better
                        buffer = br.ReadBytes(header.recordLen);
                        recReader = new BinaryReader(new MemoryStream(buffer));

                        // All dbf field records begin with a deleted flag field. Deleted - 0x2A (asterisk) else 0x20 (space)
                        try
                        {
                            if (recReader.ReadChar() == '*')
                            {
                                continue;
                            }
                        }
                        catch (Exception ex)
                        {
                            break;
                        }


                        // Loop through each field in a record
                        fieldIndex = 0;

                        row = dt.NewRow();
                        foreach (FieldDescriptor field in fields)
                        {
                            //string value = "";
                            switch (field.fieldType)
                            {
                                case 'N':  // Number
                                    // If you port this to .NET 2.0, use the Decimal.TryParse method
                                    number = Encoding.ASCII.GetString(recReader.ReadBytes(field.fieldLen));
                                    if (IsNumber(number))
                                    {
                                        if (number.IndexOf(".") > -1)
                                        {
                                            row[fieldIndex] = decimal.Parse(number);
                                        }
                                        else
                                        {
                                            row[fieldIndex] = int.Parse(number);
                                        }
                                    }
                                    else
                                    {
                                        row[fieldIndex] = 0;
                                    }

                                    break;

                                case 'C': // String
                                    row[fieldIndex] = Encoding.Default.GetString(recReader.ReadBytes(field.fieldLen));
                                    break;

                                case 'D': // Date (YYYYMMDD)
                                    year = Encoding.ASCII.GetString(recReader.ReadBytes(4));
                                    month = Encoding.ASCII.GetString(recReader.ReadBytes(2));
                                    day = Encoding.ASCII.GetString(recReader.ReadBytes(2));
                                    row[fieldIndex] = System.DBNull.Value;
                                    try
                                    {
                                        if (IsNumber(year) && IsNumber(month) && IsNumber(day))
                                        {
                                            if ((Int32.Parse(year) > 1900))
                                            {
                                                row[fieldIndex] = new DateTime(Int32.Parse(year), Int32.Parse(month), Int32.Parse(day));
                                            }
                                        }
                                    }
                                    catch
                                    { }

                                    break;

                                case 'T': // Timestamp, 8 bytes - two integers, first for date, second for time
                                    // Date is the number of days since 01/01/4713 BC (Julian Days)
                                    // Time is hours * 3600000L + minutes * 60000L + Seconds * 1000L (Milliseconds since midnight)
                                    lDate = recReader.ReadInt32();
                                    lTime = recReader.ReadInt32() * 10000L;
                                    row[fieldIndex] = JulianToDateTime(lDate).AddTicks(lTime);
                                    break;

                                case 'L': // Boolean (Y/N)
                                    if ('Y' == recReader.ReadByte())
                                    {
                                        row[fieldIndex] = true;
                                    }
                                    else
                                    {
                                        row[fieldIndex] = false;
                                    }

                                    break;

                                case 'F':
                                    number = Encoding.ASCII.GetString(recReader.ReadBytes(field.fieldLen));
                                    if (IsNumber(number))
                                    {
                                        row[fieldIndex] = double.Parse(number);
                                    }
                                    else
                                    {
                                        row[fieldIndex] = 0.0F;
                                    }
                                    break;

                            }
                            fieldIndex++;
                        }

                        recReader.Close();

                        GetKHData(row, outfile, errfile, isTCVN3, ky_cuoc);
                    }

                    if (totalOK > 0)
                    {
                        ok.Append("</Customers>");
                        outfile.Write(Encoding.UTF8.GetBytes(ok.ToString()), 0, Encoding.UTF8.GetByteCount(ok.ToString()));
                        outfile.Close();
                    }
                    else
                    {
                        outfile.Close();
                        if (File.Exists(outfilePath)) File.Delete(outfilePath);
                    }

                    if (totalErr > 0)
                    {
                        err.Append("</Customers>");
                        errfile.Write(Encoding.UTF8.GetBytes(err.ToString()), 0, Encoding.UTF8.GetByteCount(err.ToString()));
                        errfile.Close();
                    }
                    else
                    {
                        errfile.Close();
                        if (File.Exists(errfilePath)) File.Delete(errfilePath);
                    }
                }

                catch (Exception ex)
                {
                    errfile.Close();
                    if (File.Exists(errfilePath)) File.Delete(errfilePath);
                    outfile.Close();
                    if (File.Exists(outfilePath)) File.Delete(outfilePath);
                    throw;
                }
                finally
                {
                    if (null != br)
                    {
                        br.Close();
                    }
                }
            }
            long count = DateTime.Now.Ticks - start;
            return dt;
        }
        
        private void GetKHData(DataRow drow, Stream outfile, Stream errfile, bool isTCVN3, string ky_cuoc)
        {
            try
            {
                //if (drow["email"].ToString().Trim().Equals(""))
                //{
                //    return;
                //}

                //if (drow["email"].ToString().Trim().Equals("") || drow["email"].ToString().Trim().Length < 50)
                //{
                //    return;
                //}

                StringBuilder sb = new StringBuilder();
                string rv = "";
                string fKeyCheck = "";
                string row_ma_kh = drow["ma_kh"].ToString().Trim();
                string row_ma_tb = drow["ma_tb"].ToString().Trim();
                string row_ky_cuoc = drow["ky_cuoc"].ToString().Replace("/", "").Trim();
                fKeyCheck = row_ma_kh.ToUpper();

                if (!row_ky_cuoc.Equals(ky_cuoc))
                {
                    rv = string.Format("<Customer><Code>{0}</Code><ERR>{1}</ERR></Customer>", row_ma_kh, "Sai kỳ cước");
                    totalErr++;
                    if (totalErr % 10000 == 0)
                    {
                        errfile.Write(Encoding.UTF8.GetBytes(err.ToString()), 0, Encoding.UTF8.GetByteCount(err.ToString()));
                        errfile.Flush();
                        err.Clear();
                        return;
                    }
                    err.Append(rv).AppendLine();
                    return;
                }

                if (search(ss, fKeyCheck) > 0)
                {
                    rv = string.Format("<Customer><Code>{0}</Code><ERR>{1}</ERR></Customer>", row_ma_kh, "Trùng khách hàng");
                    totalErr++;
                    if (totalErr % 10000 == 0)
                    {
                        errfile.Write(Encoding.UTF8.GetBytes(err.ToString()), 0, Encoding.UTF8.GetByteCount(err.ToString()));
                        errfile.Flush();
                        err.Clear();
                        return;
                    }
                    err.Append(rv).AppendLine();
                    return;
                }
                else
                {
                    ss[id] = fKeyCheck;
                    id++;
                }

                //sb.Append(row_ma_kh);

                sb.Append("<Customer><Name>");
                if (isTCVN3)
                {
                    sb.Append(Util.convertSpecialCharacter(Util.convertTCVN3ToUnicode(drow["TEN_TT"].ToString().Trim())));    
                }
                else
                {
                    sb.Append(Util.convertSpecialCharacter(drow["TEN_TT"].ToString().Trim()));    
                }
                sb.Append("</Name><Code>");
                sb.Append(row_ma_kh);
                sb.Append("</Code><TaxCode>");
                sb.Append(drow["MS_THUE"].ToString().Trim());
                sb.Append("</TaxCode><Address>");
                if (isTCVN3)
                {
                    sb.Append(Util.convertSpecialCharacter(Util.convertTCVN3ToUnicode(drow["DIACHI_TT"].ToString().Trim())));
                }
                else
                {
                    sb.Append(Util.convertSpecialCharacter(drow["DIACHI_TT"].ToString().Trim()));   
                }
                sb.Append("</Address><BankAccountName></BankAccountName><BankName></BankName><BankNumber></BankNumber><Email></Email>");
                //sb.Append(Util.convertSpecialCharacter(drow["email"].ToString().Trim()));
                //sb.Append("</Email><Fax></Fax><Phone>");
                sb.Append("<Fax></Fax><Phone>");
                sb.Append(row_ma_tb);
                sb.Append("</Phone><ContactPerson></ContactPerson><RepresentPerson></RepresentPerson><CusType>");
                //string cusType = (drow["MS_THUE"].ToString().Trim() != "" && drow["MS_THUE"].ToString().Trim() != "X X  X X X X X X X  X  X X X  X") ? "1" : "0";
                //sb.Append(cusType);
                //sb.Append("</CusType></Customer>");
                sb.Append("0</CusType></Customer>");

                //Kiem tra du lieu xml
                rv = sb.ToString();
                if (!validator.ValidXmlDoc(rv, "", @"XMLValidate\CustomerValidate.xsd"))
                {
                    rv = string.Format("<Customer><Code>{0}</Code><ERR>{1}</ERR></Customer>", row_ma_kh, validator.ValidationError);
                    totalErr++;
                    if (totalErr % 10000 == 0)
                    {
                        errfile.Write(Encoding.UTF8.GetBytes(err.ToString()), 0, Encoding.UTF8.GetByteCount(err.ToString()));
                        errfile.Flush();
                        err.Clear();
                    }
                    err.Append(rv).AppendLine();
                    return;
                }

                totalOK++;
                if (totalOK % 10000 == 0)
                {
                    outfile.Write(Encoding.UTF8.GetBytes(ok.ToString()), 0, Encoding.UTF8.GetByteCount(ok.ToString()));
                    outfile.Flush();
                    ok.Clear();
                }
                ok.Append(rv).AppendLine();
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                return;
            }
        }
        #endregion
    }
}