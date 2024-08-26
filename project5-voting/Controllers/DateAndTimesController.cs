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
        private ElectionEntities1 db = new ElectionEntities1();


        // GET: DateAndTimes/AddDateAndTime
        public ActionResult AddDateAndTime()
        {
            var dateDefault = db.Dates.FirstOrDefault(u => u.id == 1);
            if (dateDefault != null)
            {
                ViewBag.StartDate = dateDefault.startDate.ToString().Split(' ')[0];
                ViewBag.EndDate = dateDefault.endDate.ToString().Split(' ')[0];
                ViewBag.StartTime = dateDefault.startTime;
                ViewBag.EndTime = dateDefault.endTime;
            }

            var startdate = db.Dates.Find(1).startDate.Value.ToString("dd/MM/yyyy");
            var enddate = db.Dates.Find(1).endDate.Value.ToString("dd/MM/yyyy");

            var startTime = db.Dates.Find(1).startTime.ToString();
            var endTime = db.Dates.Find(1).endTime.ToString();

            var currentDate = DateTime.Now.ToString("dd-MM-yyyy");
            var currentTime = DateTime.Now.ToString("HH:mm:ss");


            int dateResult1 = string.Compare(startdate, currentDate);
            int dateResult2 = string.Compare(enddate, currentDate);

            int timeResult1 = string.Compare(startTime, currentTime);
            int timeResult2 = string.Compare(endTime, currentTime);

            if (dateResult1 < 0 && dateResult2 > 0)
            {
                if (timeResult1 < 0 && timeResult2 > 0)
                {
                    Session["inTime"] = true;
                }
                else
                {
                    Session["inTime"] = false;
                }
            }
            else
            {
                Session["inTime"] = false;
            }
            return View(dateDefault);
        }

        // POST: DateAndTimes/AddDateAndTime
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddDateAndTime(DateTime StartDate, DateTime EndDate, TimeSpan StartTime, TimeSpan EndTime)
        {
            var dateDefault = db.Dates.FirstOrDefault(u => u.id == 1);
            if (dateDefault == null)
            {
                var newDateEntry = new Date
                {
                    startDate = StartDate,
                    endDate = EndDate,
                    startTime = StartTime,
                    endTime = EndTime,
                };

                db.Dates.Add(newDateEntry);
            }
            else
            {
                dateDefault.startDate = StartDate;
                dateDefault.endDate = EndDate;
                dateDefault.startTime = StartTime;
                dateDefault.endTime = EndTime;

                db.Entry(dateDefault).State = EntityState.Modified;
            }




            db.SaveChanges();
            return RedirectToAction("AddDateAndTime");
        }
    }
}