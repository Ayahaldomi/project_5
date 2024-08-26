using project5_voting.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace project5_voting.Controllers
{
    public class DebateController : Controller
    {
        ElectionEntities1 db = new ElectionEntities1();

        public ActionResult DebateForm()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DebateForm(Debate path)
        {
            path.status = "1";

            db.Debates.Add(path);
            db.SaveChanges();
            return RedirectToAction("UserDebate", "Home");
        }
        public ActionResult ShowDebates()
        {
            return View(db.Debates.ToList());
        }

        public ActionResult Show(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Debate uSER = db.Debates.Find(id);
            if (uSER == null)
            {
                return HttpNotFound();
            }

            var checkCandidate1 = db.localCandidates
                .Where(candidate => candidate.national_id == uSER.firstCandidateID)
                .FirstOrDefault();
            var checkCandidate2 = db.localCandidates
                .Where(candidate => candidate.national_id == uSER.secondCandidateID)
                .FirstOrDefault();


            if (checkCandidate1 == null || checkCandidate2 == null)
            {
                ViewBag.CheckID = null;
                ViewBag.ElectionAreia = null;
            }
            else if (checkCandidate1.national_id == uSER.firstCandidateID && checkCandidate2.national_id == uSER.secondCandidateID)
            {
                ViewBag.CheckID = true;

                if (checkCandidate1.election_area == uSER.electionArea && checkCandidate2.election_area == uSER.electionArea)
                {
                    ViewBag.ElectionAreia = true;
                }
                else
                {
                    ViewBag.ElectionAreia = null;
                }
            }

            return View(uSER);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Show(Debate path)
        {
            if (ModelState.IsValid)
            {
                db.Entry(path).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("ShowDebates");
            }


            return View(path);
        }

    }


}
