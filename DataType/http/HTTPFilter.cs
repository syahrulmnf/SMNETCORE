//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Web.Mvc;

//namespace SMNETCORE.DataType.Http
//{
//    public class TenantActionFilter : ActionFilterAttribute, IActionFilter
//    {
//        public override void OnActionExecuting(ActionExecutingContext filterContext) 
//        { 
//            var fullAddress = filterContext.HttpContext.Request.Headers["Host"].Split('.'); 
//            if (fullAddress.Length < 2) 
//            { 
//                filterContext.Result = new HttpStatusCodeResult(404); //or redirect filterContext.Result = new RedirectToRouteResult(..);
//            } 
        
//            var tenantSubdomain = fullAddress[0]; 
        
//            // Lookup tenant id (preferably use a cache) 
//            if(!tenantSubdomain.Equals("www"))
//            {
//                var tenantId = 0 ;
//                var orgCode = "" ;
//                filterContext.RouteData.Values.Add("OrganisationId", tenantId);
//                filterContext.RouteData.Values.Add("orgCode", orgCode);
//                base.OnActionExecuting(filterContext); 
//            }
//        }
//    }
//}
