using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using project5_voting.Models;

namespace project5_voting.Controllers
{
    public class localCandidatesController : Controller
    {
        private ElectionEntities1 db = new ElectionEntities1();

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
            long listId = Convert.ToInt64(Session["listId"]);
            var localCandidates = db.localCandidates.Where(c => c.listKey == listId).ToList();

            if (!CheckListCriteria(localCandidates))
            {
                TempData["wrongId"] = "الشروط غير متحققة. يرجى مراجعة الشروط المطلوبة.";
                return RedirectToAction("localCandidate");
            }
            //to cleare the data
            Session["listName"] = null;
            Session["listId"] = null;
            Session["electionArea"] = null;


            return RedirectToAction("Index", "Home");
        }


        public ActionResult localCandidate()
        {
            Session["wrongId"] = "";

            var localCandidates = db.localCandidates.ToList();
            return View(localCandidates);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult localCandidate(localCandidate localCandidate, HttpPostedFileBase candidateImage)
        {
            long listId = Convert.ToInt64(Session["listId"]);

            // Validate the candidate's existence
            var candidate = db.USERS.FirstOrDefault(u => u.nationalID == localCandidate.national_id);
            if (candidate == null)
            {
                TempData["wrongId"] = "لقد أدخلت رقم وطني خاطئ.";
                return RedirectToAction("localCandidate");
            }

            // Check if the candidate's national ID or name is already in the list
            bool alreadyInList = db.localCandidates.Any(c => c.listKey == listId &&
                                                             (c.national_id == localCandidate.national_id ||
                                                              c.candidateName == localCandidate.candidateName));
            if (alreadyInList)
            {
                TempData["wrongId"] = "لا يمكن إدخال نفس الاسم أو الرقم الوطني مرتين في القائمة.";
                return RedirectToAction("localCandidate");
            }

            // Save the uploaded image if provided
            if (candidateImage != null && candidateImage.ContentLength > 0)
            {
                string uploadsFolder = Server.MapPath("~/Uploads/");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string fileName = Path.GetFileName(candidateImage.FileName);
                string path = Path.Combine(uploadsFolder, fileName);
                candidateImage.SaveAs(path);

                localCandidate.img = "/Uploads/" + fileName; // Save the image path
            }

            // Set additional properties
            localCandidate.listName = Session["listName"].ToString();
            localCandidate.candidateName = candidate.name;
            localCandidate.election_area = Session["electionArea"].ToString();
            localCandidate.email = candidate.email;
            localCandidate.gender = candidate.gender;
            localCandidate.birth_day = candidate.birthday;
            localCandidate.religion = candidate.religion;
            localCandidate.counter = 0;
            localCandidate.listKey = listId;

            db.localCandidates.Add(localCandidate);
            db.SaveChanges();

            return RedirectToAction("localCandidate");
        }
        public ActionResult locaListAdmin()
        {
            var lists = db.localLists.ToList();
            return View(lists);
        }

        public ActionResult localCandidateAdmin(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var selectedList = db.localLists.FirstOrDefault(u => u.id == id);
            if (selectedList == null)
            {
                return HttpNotFound();
            }

            var localCandidates = db.localCandidates
                .Where(c => c.listKey == id)
                .ToList();

            if (!CheckListCriteria(localCandidates))
            {
                ViewBag.AlertMessage = "الشروط غير متحققة. يرجى مراجعة الشروط المطلوبة.";
                return View(localCandidates);
            }

            selectedList.status = "1";
            db.Entry(selectedList).State = EntityState.Modified;
            db.SaveChanges();

            return View(localCandidates);
        }
     
       public bool CheckListCriteria(List<localCandidate> candidates)
        {
            // Check that all candidates are over 25 years old
            bool allOver25 = candidates.All(c =>
                c.birth_day.HasValue && (DateTime.Now.Year - c.birth_day.Value.Year) > 25);

            if (!allOver25)
            {
                TempData["wrongId"] = "يجب أن يكون عمر جميع المتقدمين أكبر من 25 عاماً.";
                return false;
            }

            // Retrieve election area from session
            string electionArea = Session["electionArea"] as string;
            if (string.IsNullOrEmpty(electionArea))
            {
                TempData["wrongId"] = "لم يتم تحديد الدائرة الانتخابية. يرجى المحاولة مرة أخرى.";
                return false;
            }

            bool hasAtLeastOneFemale = candidates.Any(c => c.gender == "انثى");
            bool hasAtLeastOneChristian = candidates.Any(c => c.religion == "مسيحي");
            int candidateCount = candidates.Count;

            if (electionArea == "اربد الاولى")
            {
                if (!hasAtLeastOneFemale || !hasAtLeastOneChristian)
                {
                    TempData["wrongId"] = "يجب أن يكون هناك على الأقل أنثى واحدة ومسيحي واحد في القائمة.";
                    return false;
                }
                if (candidateCount > 8)
                {
                    TempData["wrongId"] = "يجب أن يكون عدد المتقدمين 8 كحد أقصى.";
                    return false;
                }
            }
            else if (electionArea == "اربد الثانية")
            {
                if (!hasAtLeastOneFemale)
                {
                    TempData["wrongId"] = "يجب أن يكون هناك على الأقل أنثى واحدة في القائمة.";
                    return false;
                }
                if (candidateCount > 7)
                {
                    TempData["wrongId"] = "يجب أن يكون عدد المتقدمين 7 كحد أقصى.";
                    return false;
                }
            }
            else if (electionArea == "mafraq")
            {
                if (!hasAtLeastOneFemale)
                {
                    TempData["wrongId"] = "يجب أن يكون هناك على الأقل أنثى واحدة في القائمة.";
                    return false;
                }
                if (candidateCount > 5)
                {
                    TempData["wrongId"] = "يجب أن يكون عدد المتقدمين 4 كحد أقصى.";
                    return false;
                }
            }
            else
            {
                // General competitive rule
                if (!hasAtLeastOneFemale || !hasAtLeastOneChristian)
                {
                    TempData["wrongId"] = "يجب أن يكون هناك على الأقل أنثى واحدة ومسيحي واحد في القائمة.";
                    return false;
                }
                if (candidateCount > 8)
                {
                    TempData["wrongId"] = "يجب أن يكون عدد المتقدمين 8 كحد أقصى.";
                    return false;
                }
            }

            return true; // All criteria are met
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



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(long id, string status)
        {
            var list = db.localLists.Find(id);
            if (list == null)
            {
                return HttpNotFound();
            }

            list.status = status; // Update the status
            db.Entry(list).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("locaListAdmin");
        }

        // POST: localCandidates/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            var localCandidate = db.localCandidates.Find(id);
            if (localCandidate != null)
            {
                db.localCandidates.Remove(localCandidate);
                db.SaveChanges();
            }
            return RedirectToAction("locaListAdmin");
        }

        [HttpPost]
        public ActionResult Deletelist(int id)
        {
            var locallist = db.localLists.Find(id);
            var localCandidate = db.localCandidates.Where(l => l.listKey == id).ToList();
            foreach (var candidate in localCandidate)
            {

                db.localCandidates.Remove(candidate);
                db.SaveChanges();

            }
            db.localLists.Remove(locallist);
            db.SaveChanges();
            return RedirectToAction("locaListAdmin");
        }

    }
}
