using System.Web.Mvc;

namespace Portal.Areas.dvcntt
{
    public class dvcnttAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "dvcntt";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "dvcntt_default",
                "dvcntt/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                namespaces: new string[] { "Portal.Areas.dvcntt.Controllers" }
            );
        }
    }
}