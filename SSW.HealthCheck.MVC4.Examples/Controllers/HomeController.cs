﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SSW.HealthCheck.Examples.Controllers
{
    public class HomeController : Controller
    {
        public virtual ActionResult Index()
        {
            return View();
        }
    }
}