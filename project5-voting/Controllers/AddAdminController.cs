using project5_voting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace project5_voting.Controllers
{
    public class AddAdminController : Controller
    {
        // GET: AddAdmin
        private ElectionEntities1 db = new ElectionEntities1();


        public ActionResult addAdmain()
        {
            return View();
        }

        public ActionResult showAdmin()
        {
            return View(db.Admins.ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult addAdmain(Admin admin)
        {
            if (ModelState.IsValid)
            {
                db.Admins.Add(admin);
                db.SaveChanges();
                return RedirectToAction("showAdmin");
            }

            return View(admin);
        }
        public ActionResult DeleteAdmin(int id)
        {
            var admin = db.Admins.Find(id);
            if (admin != null)
            {
                db.Admins.Remove(admin);
                db.SaveChanges();
            }

            return RedirectToAction("showAdmin", "AddAdmin");
        }

    }
}