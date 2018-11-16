using System.Web.Mvc;

namespace Portal.Areas.tratruoc
{
    public class ccbsAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "tratruoc";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "tratruoc_default",
                "tratruoc/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                namespaces: new string[] { "Portal.Areas.tratruoc.Controllers" }
            );
        }
    }
}