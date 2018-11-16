using System.Web.Mvc;

namespace Portal.Areas.vanban
{
    public class vanbanAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "vanban";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "vanban_default",
                "vanban/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                namespaces: new string[] { "Portal.Areas.vanban.Controllers" }
            );
        }
    }
}