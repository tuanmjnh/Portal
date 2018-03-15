using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Portal.Filters
{
    public class AuthVinaphoneFilterExecuting : ActionFilterAttribute
    {
        //public override void OnActionExecuting(ActionExecutingContext filterContext)
        //{

            //AuthorizationService _authorizeService = new AuthorizationService();
            //string userId = HttpContext.Current.User.Identity.GetUserId();
            //if (userId != null)
            //{
            //    var result = _authorizeService.CanManageUser(userId);
            //    if (!result)
            //    {

            //        filterContext.Result = new RedirectToRouteResult(
            //            new RouteValueDictionary{{ "controller", "Account" },
            //                              { "action", "Login" }

            //                             });
            //    }
            //}
            //else
            //{
            //    filterContext.Result = new RedirectToRouteResult(
            //    new RouteValueDictionary{{ "controller", "Account" },
            //                              { "action", "Login" }

            //                             });

            //}
        //    base.OnActionExecuting(filterContext);
        //}
    }
}