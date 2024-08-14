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
        private ElectionEntities db = new ElectionEntities();


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