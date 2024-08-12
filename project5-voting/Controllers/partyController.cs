using MimeKit;
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
        public ActionResult Createuser([Bind(Include = "id,partyName,counter")] PartyList partyList)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var existingParty = db.PartyLists.FirstOrDefault(p => p.partyName == partyList.partyName);

                    db.PartyLists.Add(partyList);
                    db.SaveChanges();
                    Session["PartyId"] = partyList.id;
                    Session["PartyName"] = partyList.partyName;
                    Session["Counter"] = 0;

                    return RedirectToAction("Create"); // تأكد من صحة اسم الـ Controller والإجراء
                }
                catch (DbEntityValidationException ex)
                {
                    // Print the details to the error log or console
                    foreach (var validationErrors in ex.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            ModelState.AddModelError(validationError.PropertyName, validationError.ErrorMessage);
                        }
                    }
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

            if (string.IsNullOrEmpty(partyName) || counter == null)
            {
                TempData["ErrorMessage"] = "Session values are missing. Redirecting to Createuser.";
                return RedirectToAction("Createuser", "partyLists");
            }

            var partyList = db.PartyLists.FirstOrDefault(p => p.partyName == partyName);
            if (partyList == null)
            {
                TempData["ErrorMessage"] = "Party not found.";
                return RedirectToAction("Createuser", "partyLists");
            }

            int parsedPartyId;
            if (partyList.id > int.MaxValue)
            {
                TempData["ErrorMessage"] = "Party ID is too large.";
                return RedirectToAction("Createuser", "partyLists");
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
            ViewBag.partyId = new SelectList(db.PartyLists, "id", "partyName");
            return View();
        }

        // POST: partyCandidates/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,partyId,email,name,phone,nationalId,gender,birthDay,typeOfChair,religion,status,electoralDistrict")] PartyCandidate partyCandidate)
        {
            var partyName = Session["partyName"];

            if (partyName == null)
            {
                ViewBag.PartyName = "Party name not found in session";
                return View(partyCandidate);
            }
            else
            {
                ViewBag.PartyName = partyName;
            }

            var x = db.PartyLists
                       .Where(p => p.partyName == partyName.ToString())
                       .FirstOrDefault();

            if (x == null)
            {
                ModelState.AddModelError("", "Party not found.");
                ViewBag.partyId = new SelectList(db.PartyLists, "id", "partyName");
                return View(partyCandidate);
            }

            partyCandidate.partyListID = x.id;
            Session["PartyId"] = x.id;

            // Check if the party already has 41 candidates
            var candidateCount = db.PartyCandidates.Count(p => p.partyListID == x.id);
            if (candidateCount >= 41)
            {
                ModelState.AddModelError("", "The party already has 41 candidates. No more candidates can be added.");
                ViewBag.partyId = new SelectList(db.PartyLists, "id", "partyName", partyCandidate.partyListID);
                return View(partyCandidate);
            }

            // Check if the candidate is older than 25 years old
            if (partyCandidate.birthday.HasValue && (DateTime.Now.Year - partyCandidate.birthday.Value.Year) < 25)
            {
                ModelState.AddModelError("birthDay", "The candidate must be older than 25 years.");
                ViewBag.partyId = new SelectList(db.PartyLists, "id", "partyName", partyCandidate.partyListID);
                return View(partyCandidate);
            }

            // Check phone number validity
            //if (!IsValidPhoneNumber(partyCandidate.ph))
            //{
            //    ModelState.AddModelError("phone", "Phone number must be 10 digits and start with 077, 078, or 079.");
            //    ViewBag.partyId = new SelectList(db.PartyLists, "id", "partyName", partyCandidate.partyListID);
            //    return View(partyCandidate);
            //}

            // Check national ID validity
            if (!IsValidNationalId(partyCandidate.nastionalID))
            {
                ModelState.AddModelError("nationalId", "National ID must be 10 digits.");
                ViewBag.partyId = new SelectList(db.PartyLists, "id", "partyName", partyCandidate.partyListID);
                return View(partyCandidate);
            }

            // Check email validity for Gmail
            if (!partyCandidate.email.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("email", "Email must be a Gmail address.");
                ViewBag.partyId = new SelectList(db.PartyLists, "id", "partyName", partyCandidate.partyListID);
                return View(partyCandidate);
            }

            if (ModelState.IsValid)
            {
                var candidates = db.PartyCandidates
                                    .Where(p => p.partyListID == partyCandidate.partyListID)
                                    .OrderBy(p => p.id)
                                    .ToList();

                if (db.PartyCandidates.Any(p => p.email == partyCandidate.email || p.nastionalID == partyCandidate.nastionalID))
                {
                    ModelState.AddModelError("", "A candidate with the same email or national ID already exists.");
                    ViewBag.partyId = new SelectList(db.PartyLists, "id", "partyName", partyCandidate.partyListID);
                    return View(partyCandidate);
                }

                var femaleCount = candidates.Take(3).Count(c => c.gender == "female");
                var youngCandidatesCount = candidates.Take(5)
                                                      .Count(c => c.birthday.HasValue && (DateTime.Now.Year - c.birthday.Value.Year) < 35);

                if (candidates.Count < 3 && femaleCount == 0)
                {
                    partyCandidate.gender = "female";
                }

                if (youngCandidatesCount == 0)
                {
                    var today = DateTime.Today;
                    var birthDate = today.AddYears(-34);
                    partyCandidate.birthday = birthDate;
                }

                if (candidates.Count == 38 && partyCandidate.religion != "مسيحي")
                {
                    ModelState.AddModelError("religion", "The 39th candidate must be Christian.");
                    ViewBag.partyId = new SelectList(db.PartyLists, "id", "partyName", partyCandidate.partyListID);
                    return View(partyCandidate);
                }

                if (candidates.Count == 39 && partyCandidate.religion != "مسيحي")
                {
                    ModelState.AddModelError("religion", "The 40th candidate must be Christian.");
                    ViewBag.partyId = new SelectList(db.PartyLists, "id", "partyName", partyCandidate.partyListID);
                    return View(partyCandidate);
                }

                if (candidates.Count == 40 && partyCandidate.typeOfChair != "Chechen" && partyCandidate.typeOfChair != "Circassian")
                {
                    ModelState.AddModelError("typeOfChair", "The 41st candidate must be either Chechen or Circassian.");
                    ViewBag.partyId = new SelectList(db.PartyLists, "id", "partyName", partyCandidate.partyListID);
                    return View(partyCandidate);
                }

                db.PartyCandidates.Add(partyCandidate);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            ViewBag.partyId = new SelectList(db.PartyLists, "id", "partyName", partyCandidate.partyListID);
            return View(partyCandidate);
        }

        private bool IsValidPhoneNumber(string phoneNumber)
        {
            // Validate phone number based on your criteria
            return !string.IsNullOrEmpty(phoneNumber) &&
                   phoneNumber.Length == 10 &&
                   (phoneNumber.StartsWith("077") || phoneNumber.StartsWith("078") || phoneNumber.StartsWith("079"));
        }


        private bool IsValidNationalId(string nationalId)
        {
            return !string.IsNullOrEmpty(nationalId) && nationalId.Length == 10 && nationalId.All(char.IsDigit);
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



        public ActionResult IndexAdmin()
        {
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
