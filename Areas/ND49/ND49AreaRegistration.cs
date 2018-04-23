using System.Web.Mvc;

namespace Portal.Areas.ND49
{
    public class ND49AreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "ND49";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "ND49_default",
                "ND49/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                namespaces: new string[] { "Portal.Areas.ND49.Controllers" }
            );
        }
    }
}