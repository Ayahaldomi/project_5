﻿using MimeKit;
using project5_voting.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MailKit.Net.Smtp;
using Microsoft.Ajax.Utilities;
using MimeKit;
using System.Net;
using System.IO;

namespace project5_voting.Controllers
{
    public class partyController : Controller
    {
        ElectionEntities db = new ElectionEntities();

        public ActionResult Createuser()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public ActionResult Createuser([Bind(Include = "id,partyName,counter")] PartyList partyList, HttpPostedFileBase PartyImage)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Check if a party with the same name already exists
                    var existingParty = db.PartyLists.FirstOrDefault(p => p.partyName == partyList.partyName);
                    if (existingParty != null)
                    {
                        ModelState.AddModelError("partyName", "A party with the same name already exists.");
                        return View(partyList);
                    }

                    // Handle file upload
                    if (PartyImage != null && PartyImage.ContentLength > 0)
                    {
                        // Generate a unique filename
                        var fileName = Path.GetFileName(PartyImage.FileName);
                        var path = Path.Combine(Server.MapPath("~/Image1/"), fileName);

                        // Save the file
                        PartyImage.SaveAs(path);

                        // Set the image path in the model
                        partyList.partyImage = fileName;
                    }

                    // Add new party to the database
                    db.PartyLists.Add(partyList);
                    db.SaveChanges();

                    // Store party information in session
                    Session["PartyId"] = partyList.id;
                    Session["PartyName"] = partyList.partyName;
                    Session["Counter"] = 0;
                    Session["partyImage"] = partyList.partyImage;

                    // Redirect to the appropriate action
                    return RedirectToAction("index", "party");
                }
                catch (DbEntityValidationException ex)
                {
                    // Handle validation errors
                    foreach (var validationErrors in ex.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            ModelState.AddModelError(validationError.PropertyName, validationError.ErrorMessage);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception or handle it as needed
                    ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
                }
            }

            return View(partyList);
        }

        //////////////////////////////// ADD Candidite///////////////////////////////////////

        public ActionResult SetStatus(long id, string status)
        {
            var partyName = Session["PartyName"] as string;

            // البحث عن المرشح باستخدام المعرف
            var candidate = db.PartyCandidates.Find(id);
            if (candidate == null)
            {
                return HttpNotFound();
            }

            // تحديث حالة المرشح
            candidate.status = status;
            db.Entry(candidate).State = EntityState.Modified;
            db.SaveChanges();

            // التحقق مما إذا كان هناك أي مرشح في نفس الحزب لديه حالة "0"
            var partyId = candidate.partyListID;
            var hasRejectedCandidates = db.PartyCandidates.Any(c => c.partyListID == partyId && c.status == "0");

            if (hasRejectedCandidates)
            {
                var party = db.PartyLists.Find(partyId);
                if (party != null)
                {
                    // تحديث حالة الحزب إلى "0" إذا كان هناك مرشح مرفوض
                    party.status = "0";
                    db.Entry(party).State = EntityState.Modified;
                    db.SaveChanges();

                    try
                    {
                        SendEmail("انت مرفوض في تقديم", "election2024jordan@gmail.com");
                        ViewBag.Message = "Email sent successfully.";
                        ViewBag.PartyName = partyName;
                        return RedirectToAction("AdminView", new { partyName = partyName });
                    }
                    catch (Exception ex)
                    {
                        ViewBag.Message = "An unexpected error occurred. Please try again later.";
                        Console.WriteLine("Exception message: " + ex.Message);
                        Console.WriteLine("Stack trace: " + ex.StackTrace);
                    }
                }
            }
            else
            {
                // إذا كانت جميع المرشحين في نفس الحزب لديهم حالة "1"، تأكد من تحديث حالة الحزب
                var allCandidatesAccepted = db.PartyCandidates.Where(c => c.partyListID == partyId).All(c => c.status == "1");
                if (allCandidatesAccepted)
                {
                    var party = db.PartyLists.Find(partyId);
                    if (party != null)
                    {
                        party.status = "1"; // حالة الحزب إلى "مقبول" إذا كان جميع المرشحين مقبولين
                        db.Entry(party).State = EntityState.Modified;
                        db.SaveChanges();

                        try
                        {
                            SendEmail("تم الموافقه على طلبك", "election2024jordan@gmail.com");
                            ViewBag.Message = "Email sent successfully.";
                            ViewBag.PartyName = partyName;
                            return RedirectToAction("AdminView", new { partyName = partyName });
                        }
                        catch (Exception ex)
                        {
                            ViewBag.Message = "An unexpected error occurred. Please try again later.";
                            Console.WriteLine("Exception message: " + ex.Message);
                            Console.WriteLine("Stack trace: " + ex.StackTrace);
                        }
                    }
                }
            }

            return RedirectToAction("AdminView", new { partyName = partyName });
        }


        private void SendEmail(string messageText, string toEmail)
        {
            string fromEmail = "ayahaldomi@gmail.com";
            string fromName = "test";
            string subjectText = "نتيجتك في طلب تقديم";

            string smtpServer = "smtp.gmail.com";
            int smtpPort = 465; // Port 465 for SSL

            string smtpUsername = "election2024jordan@gmail.com";
            string smtpPassword = "zwht jwiz ivfr viyt"; // Ensure this is correct

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subjectText;
            message.Body = new TextPart("plain") { Text = messageText };

            using (var client = new SmtpClient())
            {
                client.Connect(smtpServer, smtpPort, true); // Use SSL
                client.Authenticate(smtpUsername, smtpPassword);
                client.Send(message);
                client.Disconnect(true);
            }
        }


        [HttpPost]
        public JsonResult StorePartyNameInSession(string partyName)
        {
            Session["PartyName"] = partyName;
            return Json(new { success = true });
        }


        public ActionResult AdminView(string partyName)
        {
            if (string.IsNullOrEmpty(partyName))
            {
                partyName = Session["PartyName"] as string;

            }
            else
            {
                Session["PartyName"] = partyName;
            }

            if (string.IsNullOrEmpty(partyName))
            {
                // التعامل مع الحالة عندما لا يكون partyName مخزن في الجلسة
                return RedirectToAction("Index", "PartyList"); // أو أي عرض آخر
            }

            var candidates = db.PartyCandidates.Where(pc => pc.PartyList.partyName == partyName).ToList();
            ViewBag.PartyName = partyName;
            return View(candidates);
        }


        [HttpGet]
        public ActionResult SearchUser(string nationalIdSearch)
        {
            if (string.IsNullOrEmpty(nationalIdSearch))
            {
                ViewBag.ErrorMessage = "Please enter a National ID.";
                return View("Create");
            }

            var user = db.USERS.FirstOrDefault(u => u.nationalID == nationalIdSearch);

            if (user == null)
            {
                ViewBag.ErrorMessage = "No user found with the provided National ID.";
                return View("Create");
            }

            var partyCandidate = new PartyCandidate
            {
                email = user.email,
                name = user.name,
                nastionalID = user.nationalID,
                gender = user.gender,
                birthday = user.birthday,
            };

            ViewBag.partyId = new SelectList(db.PartyLists, "id", "partyName");
            return View("Create", partyCandidate);
        }

        public ActionResult index()
        {
            var id = Session["PartyId"] as string;
            var partyName = Session["PartyName"] as string;
            var counter = Session["Counter"] as int?;
            var partyImage = Session["partyImage"];

            if (string.IsNullOrEmpty(partyName) || counter == null)
            {
                TempData["ErrorMessage"] = "Session values are missing. Redirecting to Createuser.";
                return RedirectToAction("Createuser", "partyLists");
            }

            var partyList = db.PartyLists.FirstOrDefault(p => p.partyName == partyName);
            if (partyList == null)
            {
                TempData["ErrorMessage"] = "Party not found.";
                return RedirectToAction("Createuser", "party");
            }

            int parsedPartyId;
            if (partyList.id > int.MaxValue)
            {
                TempData["ErrorMessage"] = "Party ID is too large.";
                return RedirectToAction("Createuser", "party");
            }
            else
            {
                parsedPartyId = (int)partyList.id;
            }

            var partyCandidates = db.PartyCandidates
                .Where(p => p.partyListID == parsedPartyId)
                .Include(p => p.PartyList)
                .ToList();

            if (!partyCandidates.Any())
            {
                ViewBag.ErrorMessage = "No candidates found for the selected party.";
            }

            ViewBag.PartyId = parsedPartyId;
            ViewBag.PartyName = partyName;
            ViewBag.Counter = counter;
            ViewBag.Id = id;
            ViewBag.partyImage = partyImage;
            return View(partyCandidates);
        }

        public ActionResult success()
        {
            // توليد رقم دفع عشوائي
            var paymentNumber = GenerateRandomPaymentNumber();

            ViewBag.PaymentNumber = paymentNumber;
            try
            {
                string fromEmail = "ayahaldomi@gmail.com";
                string fromName = "test";
                string subjectText = "ارسال طلبك بنجاح ";
                string messageText = "تم ارسال طلبك بنجاح الرجاء الذهاب الى اقرب مكان لدفع الرسوم خلال مده مقداره يوميا ";

                string toEmail = "election2024jordan@gmail.com";
                string smtpServer = "smtp.gmail.com";
                int smtpPort = 465; // Port 465 for SSL

                string smtpUsername = "election2024jordan@gmail.com";
                string smtpPassword = "zwht jwiz ivfr viyt"; // Ensure this is correct

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(fromName, fromEmail));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = subjectText;
                message.Body = new TextPart("plain") { Text = messageText };

                using (var client = new SmtpClient())
                {
                    client.Connect(smtpServer, smtpPort, true); // Use SSL
                    client.Authenticate(smtpUsername, smtpPassword);
                    client.Send(message);
                    client.Disconnect(true);
                }

                ViewBag.Message = "Email sent successfully.";
                return View("success");
            }
            catch (Exception ex)
            {
                ViewBag.Message = "An unexpected error occurred. Please try again later.";
                Console.WriteLine("Exception message: " + ex.Message);
                Console.WriteLine("Stack trace: " + ex.StackTrace);

            }

            return View();
        }

        private string GenerateRandomPaymentNumber()
        {
            var random = new Random();
            var paymentNumber = random.Next(1000000000, 1000000000).ToString(); // رقم دفع مكون من 10 أرقام
            return paymentNumber;
        }


        // GET: partyCandidates/Create
        public ActionResult Create()
        {
            //ViewBag.partyId = new SelectList(db.PartyLists, "id", "partyName");
            ViewBag.partyId = Session["PartyId"].ToString();
            return View();
        }

        // POST: partyCandidates/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,partyId,nastionalID,name,email,typeOfChair,religion,gender,birthday,candidateImage,status,electoralDistrict")] PartyCandidate partyCandidate, HttpPostedFileBase candidateImage)
        {
            //if (candidateImageFile != null && candidateImageFile.ContentLength > 0)
            //{
            //    try
            //    {
            //        // توليد اسم فريد للصورة باستخدام GUID
            //        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(candidateImageFile.FileName);
            //        string path = Path.Combine(Server.MapPath("~/Image1/Candidates/"), uniqueFileName);

            //        // حفظ الصورة في المسار المحدد
            //        candidateImageFile.SaveAs(path);

            //        // تخزين المسار النسبي للصورة في قاعدة البيانات
            //        partyCandidate.candidateImage = "~/Image1/Candidates/" + uniqueFileName;
            //    }
            //    catch (Exception ex)
            //    {
            //        ModelState.AddModelError("", "حدث خطأ أثناء رفع الصورة. حاول مرة أخرى.");
            //    }
            //}

            //if (ModelState.IsValid)
            //{
            //    partyCandidate.partyListID =Convert.ToInt32(Session["PartyId"].ToString());

            //    db.PartyCandidates.Add(partyCandidate);
            //    db.SaveChanges();
            //    return RedirectToAction("Index");
            //}
            if (candidateImage != null && candidateImage.ContentLength > 0)
            {
                // Generate a unique filename
                var fileName = Path.GetFileName(candidateImage.FileName);
                var path = Path.Combine(Server.MapPath("~/Image1/"), fileName);

                // Save the file
                candidateImage.SaveAs(path);

                // Set the image path in the model
                partyCandidate.candidateImage = fileName;
            }
            partyCandidate.partyListID = Convert.ToInt32(Session["PartyId"].ToString());

            // Add new party to the database
            db.PartyCandidates.Add(partyCandidate);
            db.SaveChanges();

            return RedirectToAction("Index");
        }


        // Helper function to validate the national ID
        private bool IsValidNationalId(string nationalId)
        {
            return nationalId.Length == 10 && nationalId.All(char.IsDigit);
        }










        //private bool IsValidPhoneNumber(string phoneNumber)
        //{
        //    // Validate phone number based on your criteria
        //    return !string.IsNullOrEmpty(phoneNumber) &&
        //           phoneNumber.Length == 10 &&
        //           (phoneNumber.StartsWith("077") || phoneNumber.StartsWith("078") || phoneNumber.StartsWith("079"));
        //}


        public ActionResult DeletePartyCandidates(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var partyCandidate = db.PartyCandidates.Find(id);

            if (partyCandidate == null)
            {
                return HttpNotFound();
            }

            return View(partyCandidate);
        }

        // POST: partyCandidates/Delete/5
        [HttpPost, ActionName("DeletePartyCandidates")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmedPartyCandidates(long id)
        {
            var partyCandidate = db.PartyCandidates.Find(id);

            if (partyCandidate == null)
            {
                return HttpNotFound();
            }

            db.PartyCandidates.Remove(partyCandidate);
            db.SaveChanges();
            return RedirectToAction("AdminView");
        }




        // GET: partyCandidates/Delete/5
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var partyCandidate = db.PartyCandidates.Find(id);

            if (partyCandidate == null)
            {
                return HttpNotFound();
            }

            return View(partyCandidate);
        }

        // POST: partyCandidates/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            var partyCandidate = db.PartyCandidates.Find(id);

            if (partyCandidate == null)
            {
                return HttpNotFound();
            }

            db.PartyCandidates.Remove(partyCandidate);
            db.SaveChanges();
            return RedirectToAction("Index");
        }


        public ActionResult DeleteAdmin(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var PartyLists = db.PartyLists.Find(id);

            if (PartyLists == null)
            {
                return HttpNotFound();
            }

            return View(PartyLists);
        }

        // POST: partyCandidates/Delete/5
        [HttpPost, ActionName("DeleteAdmin")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmedAdmin(long id)
        {
            var PartyLists = db.PartyLists.Find(id);

            if (PartyLists == null)
            {
                return HttpNotFound();
            }

            db.PartyLists.Remove(PartyLists);
            db.SaveChanges();
            return RedirectToAction("AdminView");
        }
        public ActionResult IndexAdmin()
        {
            // استرجاع بيانات الحزب من قاعدة البيانات
            var partyLists = db.PartyLists.ToList();
            return View(partyLists);
        }

        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PartyList partyList = db.PartyLists.Find(id);
            if (partyList == null)
            {
                return HttpNotFound();
            }
            return View(partyList);
        }




    }
}
