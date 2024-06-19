using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace MinerWebApp.App_Start
{
    /**
     * WebApiConfig is a static class 
     * It is used as the routing table for API requests (for example: http://localhost:xxxx/api/...)
     */
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            //This configures the WebApi to work as we want and binds custom HTTP Attribute paths in controllers to their functions. 
            config.MapHttpAttributeRoutes();

            //This binds API routes in the way we expect by going to http://localhost:xxxx/api/... to get to the api.
            config.Routes.MapHttpRoute(
            name: "DefaultApi",
            routeTemplate: "api/{controller}/{id}",
            defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}