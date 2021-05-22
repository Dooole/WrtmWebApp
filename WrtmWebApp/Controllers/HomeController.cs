using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WrtmWebApp.Models;

namespace WrtmWebApp.Controllers
{
    public class HomeController : Controller
    {
        private wrtmDBEntities db = new wrtmDBEntities();
        public ActionResult Index()
        {
            return View(db.Devices.ToList());
        }

        public ActionResult About()
        {
            ViewBag.Message = "OpenWRT Management interface.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "";

            return View();
        }
    }
}