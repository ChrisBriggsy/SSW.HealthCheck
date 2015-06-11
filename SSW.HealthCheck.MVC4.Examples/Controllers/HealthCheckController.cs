using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using SSW.HealthCheck.Infrastructure;

namespace SSW.HealthCheck.MVC4.Examples.Controllers
{
    public partial class HealthCheckController : Controller
    {
        private static readonly JsonSerializerSettings settings = new JsonSerializerSettings()
        {
            DateFormatHandling = DateFormatHandling.IsoDateFormat
        };

        public virtual ActionResult Index()
        {
            if (this.HttpContext != null)
            {
                HealthCheckService.Default.HttpContext = this.HttpContext.ApplicationInstance.Context;
            }

            var tests = HealthCheckService.Default.GetAll();
            return View(tests);
        }

        public virtual ActionResult Check(string key)
        {
            var m = HealthCheckService.Default.GetByKey(key);
            m.RunAsync();
            var json = JsonConvert.SerializeObject(m, settings);
            return this.Content(json, "application/json");
        }
    }
}