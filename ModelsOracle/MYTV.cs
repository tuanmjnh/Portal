namespace Portal.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("MYTV_TH")]
    public partial class MYTV_TH
    {
        [Key]
        public string ID { get; set; }
        public string ACCOUNT_ID { get; set; }
        public string USERNAME { get; set; }
        public string STB_SERIAL { get; set; }
        public string IP_USER { get; set; }
        public string IP_SERVER { get; set; }
        public string PACKAGE { get; set; }
        public int SUB_FEE { get; set; }
        public int PAYTV_FEE { get; set; }
        public int TOTAL_FEE { get; set; }
        public int OWE_MONEY { get; set; }
        public int PERCENTAGE { get; set; }
        public DateTime USE_DATE { get; set; }
        public DateTime STOP_DATE { get; set; }
        public int DISCOUNT { get; set; }
        public int TOTAL_TIME { get; set; }
        public int TOTAL_FLUX { get; set; }
        public DateTime SUSPENDATE { get; set; }
        public DateTime RESUMEDATE { get; set; }
        public int PAYTVMONTH { get; set; }
        public int PAYTV_TIME { get; set; }
        public int GOD_FEE { get; set; }
        public int MOD_FEE { get; set; }
        public int KOD_FEE { get; set; }
        public int VOD_FEE { get; set; }
        public int ETR_FEE { get; set; }
        public int MEGA { get; set; }
        public int STO_FEE { get; set; }
        public int SPO_FEE { get; set; }
        public int BH_FEE { get; set; }
        public int TVS_FEE { get; set; }
        public string PACKCD { get; set; }
        public string REASON { get; set; }
        public int CHR_FEE { get; set; }
        public int EDU_FEE { get; set; }
        public int DTL_FEE { get; set; }
        public int LTR_FEE { get; set; }
        public int FIBER { get; set; }
        public int LST_FEE { get; set; }
        public int VOT_FEE { get; set; }
        public int VCTV_FEE { get; set; }
        public int KPL_FEE { get; set; }
        public int HBO_FEE { get; set; }
        public int FAF_FEE { get; set; }
        public int VTV_FEE { get; set; }
        public int RENT_FEE { get; set; }
        public int FIM_FEE { get; set; }
        public int CLG_FEE { get; set; }
        public int BHD_FEE { get; set; }
        public int CME_FEE { get; set; }
    }
    [Table("MYTV")]
    public partial class MYTV
    {
        //[ExplicitKey]
        [Key]
        public string ID { get; set; }
        public string ACCOUNT_ID { get; set; }
        public string USERNAME { get; set; }
        public string STB_SERIAL { get; set; }
        public string IP_USER { get; set; }
        public string IP_SERVER { get; set; }
        public string PACKAGE { get; set; }
        public int SUB_FEE { get; set; }
        public int PAYTV_FEE { get; set; }
        public int TOTAL_FEE { get; set; }
        public int OWE_MONEY { get; set; }
        public int PERCENTAGE { get; set; }
        public DateTime USE_DATE { get; set; }
        public DateTime STOP_DATE { get; set; }
        public int DISCOUNT { get; set; }
        public int TOTAL_TIME { get; set; }
        public int TOTAL_FLUX { get; set; }
        public DateTime SUSPENDATE { get; set; }
        public DateTime RESUMEDATE { get; set; }
        public int PAYTVMONTH { get; set; }
        public int PAYTV_TIME { get; set; }
        public int GOD_FEE { get; set; }
        public int MOD_FEE { get; set; }
        public int KOD_FEE { get; set; }
        public int VOD_FEE { get; set; }
        public int ETR_FEE { get; set; }
        public int MEGA { get; set; }
        public int STO_FEE { get; set; }
        public int SPO_FEE { get; set; }
        public int BH_FEE { get; set; }
        public int TVS_FEE { get; set; }
        public string PACKCD { get; set; }
        public string REASON { get; set; }
        public int CHR_FEE { get; set; }
        public int EDU_FEE { get; set; }
        public int DTL_FEE { get; set; }
        public int LTR_FEE { get; set; }
        public int FIBER { get; set; }
        public int LST_FEE { get; set; }
        public int VOT_FEE { get; set; }
        public int VCTV_FEE { get; set; }
        public int KPL_FEE { get; set; }
        public int HBO_FEE { get; set; }
        public int FAF_FEE { get; set; }
        public int VTV_FEE { get; set; }
        public int RENT_FEE { get; set; }
        public int FIM_FEE { get; set; }
        public int CLG_FEE { get; set; }
        public int BHD_FEE { get; set; }
        public int CME_FEE { get; set; }
        public int TYPE_BILL { get; set; }
        public DateTime TIME_BILL { get; set; }
    }
    //public class MYTVOraMap : ClassMapper<MYTV>
    //{
    //    public MYTVOraMap()
    //    {
    //        Table("MYTV");
    //        Map(x => x.ID).Key(KeyType.Guid);
    //        AutoMap();
    //    }
    //}
}
