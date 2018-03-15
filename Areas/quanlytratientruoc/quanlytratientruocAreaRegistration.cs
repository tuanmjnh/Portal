using System.Web.Mvc;

namespace Portal.Areas.quanlytratientruoc
{
    public class quanlytratientruocAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "quanlytratientruoc";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "quanlytratientruoc_default",
                "quanlytratientruoc/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                namespaces: new string[] { "Portal.Areas.quanlytratientruoc.Controllers" }
            );
        }
    }
}