using System.Web.Mvc;

namespace Portal.Areas.Cuoc
{
    public class CuocAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Cuoc";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Cuoc_default",
                "Cuoc/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                namespaces: new string[] { "Portal.Areas.Cuoc.Controllers" }
            );
        }
    }
}