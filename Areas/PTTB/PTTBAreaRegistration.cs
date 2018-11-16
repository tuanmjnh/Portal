using System.Web.Mvc;

namespace Portal.Areas.PTTB
{
    public class PTTBAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "PTTB";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "PTTB_default",
                "PTTB/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                namespaces: new string[] { "Portal.Areas.PTTB.Controllers" }
            );
        }
    }
}