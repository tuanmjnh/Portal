namespace Portal.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("DB_THUEBAO_DIACHI_BKN")]
    public partial class DB_THUEBAO_DIACHI_BKN
    {
        public int DONVI_ID { get; set; }
        public DateTime NGAY_HT { get; set; }
        public int CUOC_DV { get; set; }
        public string SONHA { get; set; }
        public int CONG_ID { get; set; }
        public int THUEBAO_ID_CHUYENDOI { get; set; }
        public DateTime NGAY_PHUCHUY { get; set; }
        public DateTime NGAY_KPTD { get; set; }
        public int TINHTP_ID { get; set; }
        public DateTime NGAY_CN_TU_GTCAS { get; set; }
        public DateTime NGAY_CN_VETINH { get; set; }
        public int DONVI_CU_ID { get; set; }
        public string DIENTHOAI_SMS { get; set; }
        public string DIENTHOAI_LH { get; set; }
        public int KP_NGANHAN { get; set; }
        public int KHACHHANG_ID_CUOI { get; set; }
        public int TRANGTHAI_MORONG { get; set; }
        public int DOITUONG_DB_ID { get; set; }
        public int NHOM_TONGDAI { get; set; }
        public DateTime NGAY_SUDUNG { get; set; }
        public DateTime NGAY_CAT { get; set; }
        public string MA_TB_DOISO { get; set; }
        public int TBDAYCHUNG_ID { get; set; }
        public string CHUCDANH { get; set; }
        public int HOST_ID { get; set; }
        public DateTime NGAY_CV_DVTH { get; set; }
        public DateTime NGAY_CV_DVQL { get; set; }
        public DateTime NGAY_CV_TCTY { get; set; }
        public string CV_DVTH { get; set; }
        public string CV_DVQL { get; set; }
        public string CV_TCTY { get; set; }
        public DateTime NGAY_NGANNGAY { get; set; }
        public DateTime NGAY_DS { get; set; }
        public DateTime NGAY_TD { get; set; }
        public DateTime NGAY_TRANGTHAITB { get; set; }
        public string MA_VUNG { get; set; }
        public int THANHTOAN_ID { get; set; }
        public string NGUOI_CN { get; set; }
        public DateTime NGAY_CN { get; set; }
        public string TEN_TB { get; set; }
        public string MA_TB { get; set; }
        public int THUEBAO_ID { get; set; }
        public int TRANGTHAITB_ID { get; set; }
        public int THUEBAO_CHA_ID { get; set; }
        public int LOAIHINHTB_ID { get; set; }
        public int DOITUONG_ID { get; set; }
        public string GHICHU { get; set; }
        public string MAY_CN { get; set; }
        public int CUOC_TB { get; set; }
        public string DIACHI_TB { get; set; }
        public int MAPHO_ID { get; set; }
        public string MA_TB_TONG { get; set; }
        public string TEN_QUANHUYEN { get; set; }
        public string TEN_PHUONGXA { get; set; }
        public string TEN_DUONGPHO { get; set; }
    }
}
