namespace Portal.Common.Objects
{
    public class BillPrint
    {
        public const int hdGop = 1;
        public const int hdNet = 2;
        public const int hdTv = 3;
        public const int hdCodinh = 4;
        public const int hdDidong = 5;
    }
    public class BillType
    {
        public const int hdGhep = 1;
        public const int hdDonLe = 2;
    }
    public class LocalType
    {
        public const string hd = "bf53751b-c254-42b2-a116-803d33ad58b7";
        public const string ccbs = "82fc35a7-1130-4bcc-9be6-91e3554de5cb";
        public const string pttb = "5d4d064c-9fda-46ca-a5a7-2d9016e813a4";
        public const string bss = "a9fcd4f8-012f-436f-bf27-f5f08eb5f46d";
    }
    public class groups
    {
        public const string dvcntt = "dvcntt";
        public const string ktr = "ktr";
        public const string ca = "ca";
        public const string ca_list = "ca_list";
        public const string ivan = "ivan";
        public const string ivan_list = "ivan_list";
        public const string token = "token";
        public const string token_list = "token_list";
        public const string domain = "domain";
        public const string hosting = "hosting";
        public const string website = "website";
        public const string document = "document";
        public const string department = "department";
        public const string files = "files";
        public const string reportDay = "reportDay";
        public const string orther = "orther";
        public const string datmoi = "datmoi";
        public const string cathuy = "cathuy";
    }
    public class ObjBSTable
    {
        public string sort { get; set; }
        public string order { get; set; }
        public string search { get; set; }
        public int offset { get; set; }
        public int limit { get; set; }
        public int flag { get; set; }
    }
    public enum ResultCode
    {
        _length = 0,
        _success = 1,
        _error = 2,
        _extension = 3
    }
}
