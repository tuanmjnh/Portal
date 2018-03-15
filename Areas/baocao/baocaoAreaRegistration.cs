using System.Web.Mvc;

namespace Portal.Areas.baocao
{
    public class baocaoAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "baocao";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "baocao_default",
                "baocao/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                namespaces: new string[] { "Portal.Areas.baocao.Controllers" }
            );
        }
    }
}