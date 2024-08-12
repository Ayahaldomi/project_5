using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using project5_voting.Models;

namespace project5_voting.Controllers
{
    public class localCandidatesController : Controller
    {
        private ElectionEntities db = new ElectionEntities();
        public ActionResult localList()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult localList(localList localList)
        {
            localList.counter = 0;
            localList.status = "0";
            db.localLists.Add(localList);
            db.SaveChanges();

            Session["listName"] = localList.listName;
            Session["listId"] = localList.id;
            Session["electionArea"] = localList.electionDistrict;

            return RedirectToAction("localCandidate");
        }

        public ActionResult ClearSessionAndRedirect()
        {
            // Clear session data
            Session["listName"] = null;
            Session["listId"] = null;
            Session["electionArea"] = null;

            // Redirect to the Index action of the Home controller
            return RedirectToAction("Index", "Home");
        }

        // GET: localCandidates
        public ActionResult localCandidate()
        {
            Session["wrongId"] = "";

            var localCandidates = db.localCandidates.ToList();
            return View(localCandidates);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult localCandidate(localCandidate localCandidate)
        {
            var candidate = db.USERS.FirstOrDefault(u => u.nationalID == localCandidate.national_id);
            if (candidate == null)
            {
                Session["wrongId"] = "You entered a wrong national ID.";
                return RedirectToAction("localCandidate");
            }

            localCandidate.listName = Session["listName"].ToString();
            localCandidate.candidateName = candidate.name;
            localCandidate.election_area = Session["electionArea"].ToString();
            localCandidate.email = candidate.email;
            localCandidate.gender = candidate.gender;
            localCandidate.birth_day = candidate.birthday;
            localCandidate.religion = candidate.religion;
            localCandidate.counter = 0;
            localCandidate.listKey = Convert.ToInt64(Session["listId"]);
            db.localCandidates.Add(localCandidate);
            db.SaveChanges();
            return RedirectToAction("localCandidate");
        }

        public ActionResult locaListAdmin()
        {
            return View(db.localLists.ToList());
        }
       
        public ActionResult localCandidateAdmin(long? id)
        {
            Session["listIdAdmin"] = id;
            var selectedList = db.localLists.FirstOrDefault(u => u.id == id);
            if (selectedList == null)
            {
                return HttpNotFound();
            }

            string electionArea = selectedList.electionDistrict;
            var localCandidates = db.localCandidates
                .Where(c => c.listKey == id)
                .ToList();

            if (electionArea == "اربد الاولى")
            {
                bool allOver25 = localCandidates.All(c =>
                    c.birth_day.HasValue && (DateTime.Now.Year - c.birth_day.Value.Year) > 25);

                bool hasType1Chair = localCandidates.Any(c => c.typeOfChair == "كوتا");
                bool hasAtLeast8Candidates = localCandidates.Count >= 8;

                if (allOver25 && hasType1Chair && hasAtLeast8Candidates)
                {
                    selectedList.status = "1";
                    db.Entry(selectedList).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }

            if (electionArea == "اربد الثانية")
            {
                bool allOver25 = localCandidates.All(c =>
                    c.birth_day.HasValue && (DateTime.Now.Year - c.birth_day.Value.Year) > 25);

                bool hasType1ChairWoman = localCandidates.Any(c => c.typeOfChair == "كوتا");
                bool hasType1Chairchristian = localCandidates.Any(c => c.typeOfChair == "مسيحي");
                bool hasAtLeast8Candidates = localCandidates.Count >= 7;

                if (allOver25 && hasType1ChairWoman && hasAtLeast8Candidates && hasType1Chairchristian)
                {
                    selectedList.status = "1";
                    db.Entry(selectedList).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }

            if (electionArea == "mafraq")
            {
                bool allOver25 = localCandidates.All(c =>
                    c.birth_day.HasValue && (DateTime.Now.Year - c.birth_day.Value.Year) > 25);

                bool hasType1ChairWoman = localCandidates.Any(c => c.typeOfChair == "كوتا");
                bool hasAtLeast8Candidates = localCandidates.Count >= 4;

                if (allOver25 && hasType1ChairWoman && hasAtLeast8Candidates)
                {
                    selectedList.status = "1";
                    db.Entry(selectedList).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }

            Session["selectedList"] = selectedList.status;

            var localCandidate = db.localCandidates.ToList();
            return View(localCandidate);
        }

        // GET: localCandidates/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            localCandidate localCandidate = db.localCandidates.Find(id);
            if (localCandidate == null)
            {
                return HttpNotFound();
            }
            return View(localCandidate);
        }

        // GET: localCandidates/Create
        public ActionResult Create()
        {
            ViewBag.listKey = new SelectList(db.localLists, "id", "listName");
            return View();
        }

        // POST: localCandidates/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,candidateName,election_area,email,national_id,gender,birth_day,religion,typeOfChair,listName,counter,listKey,img")] localCandidate localCandidate)
        {
            if (ModelState.IsValid)
            {
                db.localCandidates.Add(localCandidate);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.listKey = new SelectList(db.localLists, "id", "listName", localCandidate.listKey);
            return View(localCandidate);
        }

        // GET: localCandidates/Edit/5
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            localCandidate localCandidate = db.localCandidates.Find(id);
            if (localCandidate == null)
            {
                return HttpNotFound();
            }
            ViewBag.listKey = new SelectList(db.localLists, "id", "listName", localCandidate.listKey);
            return View(localCandidate);
        }

        // POST: localCandidates/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,candidateName,election_area,email,national_id,gender,birth_day,religion,typeOfChair,listName,counter,listKey")] localCandidate localCandidate)
        {
            if (ModelState.IsValid)
            {
                db.Entry(localCandidate).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.listKey = new SelectList(db.localLists, "id", "listName", localCandidate.listKey);
            return View(localCandidate);
        }

        // GET: localCandidates/Delete/5
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            localList localList = db.localLists.Find(id);
            if (localList == null)
            {
                return HttpNotFound();
            }
            return View(localList);
        }

        // POST: localCandidates/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            localList localList = db.localLists.Find(id);
            if (localList != null)
            {
                db.localLists.Remove(localList);
                db.SaveChanges();
            }
            return RedirectToAction("locaListAdmin");
        }
    }
}
