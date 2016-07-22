using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Owin;
using Microsoft.Owin.Hosting;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;

namespace AddressApiHost
{
    public class Startup
    {
        // This code configures Web API. The Startup class is specified as a type
        // parameter in the WebApp.Start method.
        public void Configuration(IAppBuilder app)
        {
            // Configure Web API for self-host. 
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            config.Formatters.JsonFormatter.MediaTypeMappings.Add(new QueryStringMapping("format", "json", new MediaTypeHeaderValue("application/json")));
            config.Formatters.XmlFormatter.MediaTypeMappings.Add(new QueryStringMapping("format", "xml", new MediaTypeHeaderValue("application/xml")));
            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
            app.UseWebApi(config);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var baseAddress = "http://localhost:8080";

            IDisposable app = null;
            try
            {
                app = WebApp.Start<Startup>(url: baseAddress);

                Console.WriteLine("Api running at: \"{0}/api\"", baseAddress);
                Console.WriteLine("\r\nPress ENTER to exit");
                Console.ReadLine();
            }
            finally
            {
                app.Dispose();
            }
        }
    }
}
