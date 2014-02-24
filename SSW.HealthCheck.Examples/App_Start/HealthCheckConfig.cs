using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(SSW.HealthCheck.Examples.App_Start.HealthCheckConfig), "PreStart")]
namespace SSW.HealthCheck.Examples.App_Start 
{
	using SSW.HealthCheck.Infrastructure;
    using SSW.HealthCheck.Infrastructure.Tests;
    using SSW.HealthCheck.Mvc5;

    public static class HealthCheckConfig 
    {
        public static void PreStart() 
        {
            // Add your start logic here
            RegisterTests();
        }
        public static void RegisterTests()
        {
            RegisterTests(HealthCheckService.Default);
        }
        public static void RegisterTests(HealthCheckService svc)
        {
            svc.Add(new NotDebugTest());
            svc.Add(new DbConnectionTest());
            svc.Setup<SSW.HealthCheck.Examples.Hubs.HealthCheckHub>();
        }
    }
}