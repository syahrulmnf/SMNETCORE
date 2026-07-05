namespace SMNETCORE.DataType.Http
{
    //public class TenantRouteConstraint : IRouteConstraint
    //{
    //    public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
    //    { 
    //        var fullAddress = httpContext.Request.Headers["Host"].Split('.'); 
    //        if (fullAddress.Length < 2) 
    //        { 
    //            return false; 
    //        } 

    //        var tenantSubdomain = fullAddress[0]; 
    //        if(!tenantSubdomain.Equals("www"))
    //        {
    //            var tenantId = 0 ;
    //            var orgCode = "";
    //            httpContext.Request.RequestContext.RouteData.Values.Add("OrganisationId", tenantId);
    //            httpContext.Request.RequestContext.RouteData.Values.Add("orgCode", orgCode);
    //            if (!values.ContainsKey("tenant"))
    //            {
    //                values.Add("tenant", tenantId);
    //            } 
    //        }
            

    //        return true; 
    //    }
    //}
}
