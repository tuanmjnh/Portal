namespace Portal.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("HD_THUEBAO_BKN")]
    public partial class HD_THUEBAO_BKN
    {
        public string MA_CRM { get; set; }
        public string MA_TB_TINHTP { get; set; }
        public DateTime? NGAY_TINHCUOC { get; set; }
        public int CAMKET_SLA { get; set; }
        public bool TAILIEU_GPHAP { get; set; }
        public DateTime? NGAY_CV_DVQL { get; set; }
        public DateTime? NGAY_CV_DVTH { get; set; }
        public DateTime? THOIGIAN_CDL { get; set; }
        public int MUCDICH_TDKP_ID { get; set; }
        public int KIEU_YC_ID { get; set; }
        public DateTime? NGAY_CV { get; set; }
        public string MA_TB_CT { get; set; }
        public int LYDO_TH_ID { get; set; }
        public int TRANGTHAIHT_TDKP_ID { get; set; }
        public DateTime? NGAY_HT { get; set; }
        public int DOT_HTHS { get; set; }
        public int DA_IN { get; set; }
        public string NGUOI_HTHS { get; set; }
        public int DAILY_DACHUYEN { get; set; }
        public string CONVERTED { get; set; }
        public int NGUOI_NHAN_HOSO { get; set; }
        public DateTime? NGAY_NHAN_HOSO { get; set; }
        public int MAYNGOAI { get; set; }
        public string MA_TB_GANNHAT { get; set; }
        public int TIEPTHI_ID { get; set; }
        public int NHAN_TRICH_THUONG { get; set; }
        public DateTime? NGAY_HEN { get; set; }
        public string LYDO_HEN { get; set; }
        public int HD_LOAI { get; set; }
        public DateTime? NGAY_TT_THAT { get; set; }
        public int TBDAYCHUNG_ID { get; set; }
        public int THUEBAO_ID_DUNGCHUNG { get; set; }
        public DateTime? NGAYSUDUNG { get; set; }
        public DateTime? NGAYKETTHUC { get; set; }
        public string KIEU_HD { get; set; }
        public int DOITUONG_DB_ID { get; set; }
        public int TRANGTHAI_HTHS { get; set; }
        public int BOSUNG_HTHS { get; set; }
        public int XULY_DOITUONG { get; set; }
        public DateTime? NGAY_TRANGTHAI_HTHS { get; set; }
        public int KHACHHANG_ID_CUOI { get; set; }
        public string DIENTHOAI_LH { get; set; }
        public string DIENTHOAI_SMS { get; set; }
        public int STT_HOSO { get; set; }
        public int CONVERT_GTCAS { get; set; }
        public int TINHTP_ID { get; set; }
        public DateTime? NGAY_LHD { get; set; }
        public string NGUOI_HTHS_XTTT { get; set; }
        public int CAPNHAT { get; set; }
        public int CONG_ID { get; set; }
        public int NHAMANG_KHAC { get; set; }
        public int DIABAN_ID { get; set; }
        public string MA_TB_TONG { get; set; }
        public int DONVI_QLHD_ID { get; set; }
        public int THANG_LD { get; set; }
        public int DOITUONG_ID { get; set; }
        public int MAPHO_ID { get; set; }
        public int HDTB_ID { get; set; }
        public string MA_TB { get; set; }
        public string MA_TB_CU { get; set; }
        public int KIEUTT_ID { get; set; }
        public string DIACHI_TB { get; set; }
        public string CHUCDANH { get; set; }
        public DateTime? NGAY_CN { get; set; }
        public string MAY_CN { get; set; }
        public int LOAIHINHTB_ID { get; set; }
        public int TTHD_ID { get; set; }
        public string TEN_TB { get; set; }
        public string SONHA { get; set; }
        public DateTime? NGAY_TT { get; set; }
        public string NGUOI_CN { get; set; }
        public string GHICHU { get; set; }
        public int DONVI_ID { get; set; }
        public int HDTT_ID { get; set; }
        public int KIEULD_ID { get; set; }
        public int HDTB_CHA_ID { get; set; }
        public string MA_TC { get; set; }
        public int THUEBAO_ID { get; set; }
        public string SO_HD { get; set; }
        public string CONGVAN { get; set; }
        public DateTime? NGAY_TTOAN { get; set; }
        public DateTime? NGAY_CAT { get; set; }
        public string MA_VUNG { get; set; }
        public int KHUVUC_ID { get; set; }
        public int THANG_TD { get; set; }
        public DateTime? NGAY_TDKP { get; set; }
        public int KIEUHD_ID { get; set; }
        public int TYLE_VAT { get; set; }
        public int DONVI_TC_ID { get; set; }
        public DateTime? NGAY_DS { get; set; }
        public DateTime? NGAY_GIAOTC { get; set; }
        public DateTime? NGAY_NGANNGAY { get; set; }
        public string CV_TCTY { get; set; }
        public string CV_DVQL { get; set; }
        public string CV_DVTH { get; set; }
        public int TAMTHU { get; set; }
        public int CHUYEN_DL { get; set; }
        public DateTime? NGAY_CV_TCTY { get; set; }

    }
}
