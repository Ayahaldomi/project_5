using project5_voting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace project5_voting.Controllers
{
    public class HomeController : Controller
    {

        ElectionEntities1 db = new ElectionEntities1();

        public ActionResult Index()
        {

            return View();
        }

        public ActionResult SelectList()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult ploice() { 

            return View();
        }

       

        public ActionResult VotersDetails()
        {
            var voter = TempData["voterDetails"] as string;

            if (!string.IsNullOrEmpty(voter))
            {
                var checkInputs = db.USERS.Where(model => model.nationalID == voter).FirstOrDefault();

                if (checkInputs != null)
                {
                    ViewBag.Error = true;
                    return View(checkInputs);
                }
                else
                {
                    ViewBag.Error = false;
                }
            }

            return View();
        }

        public ActionResult Search(string Search)
        {
            if (!string.IsNullOrEmpty(Search))
            {
                TempData["voterDetails"] = Search;
            }

            return RedirectToAction("VotersDetails");
        }

     
        public ActionResult Application()
        {
            return View(); 
        }


        public ActionResult UserDebate()
        {
            var debateCards = db.Debates.Where(model => model.status == "2").ToList();

            return View(debateCards);
        }


        public ActionResult Logout()
        {
            HttpCookie existingCookie = Request.Cookies["logedInUser"];
            HttpCookie existingCookie1 = Request.Cookies["electionArea"];

            existingCookie.Expires = DateTime.Now.AddDays(-1);
            existingCookie1.Expires = DateTime.Now.AddDays(-1);

            Response.Cookies.Set(existingCookie);
            Response.Cookies.Set(existingCookie1);

            return RedirectToAction("Index", "Home");
        }


        public ActionResult RequestForm()
        {
            return View();
        }
        
        
    }
}