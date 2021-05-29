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
        private wrtmDBEntities1 db = new wrtmDBEntities1();
        public ActionResult Index()
        {
            Response.AddHeader("Refresh", "3");
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