using project5_voting.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace project5_voting.Controllers
{
    public class DateAndTimesController : Controller
    {
        public ActionResult AddDateAndTime()
        {
            HttpCookie startDateTime = new HttpCookie("startDateTime", "0000000");
            Response.Cookies.Add(startDateTime);


            HttpCookie endDateTime = new HttpCookie("endDateTime", "0000000");
            Response.Cookies.Add(endDateTime);

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddDateAndTime(DateTime StartDateTime, DateTime EndDateTime)
        {
            
            HttpCookie startDateTime = new HttpCookie("startDateTime", StartDateTime.ToString());
            Response.Cookies.Add(startDateTime);


            HttpCookie endDateTime = new HttpCookie("endDateTime", EndDateTime.ToString());
            Response.Cookies.Add(endDateTime);

            return RedirectToAction("AddDateAndTime");
        }

    }
}