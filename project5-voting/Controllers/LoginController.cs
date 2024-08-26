using project5_voting.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

using System.Web;
using System.Web.Mvc;
using MailKit.Net.Smtp;
using Microsoft.Ajax.Utilities;
using MimeKit;
using System.Text;


namespace project5_voting.Controllers
{
    public class LoginController : Controller
    {
        ElectionEntities1 db = new ElectionEntities1();


        public ActionResult firststeplogin()
        {
            return View();
        }

        [HttpPost]
        public ActionResult firststeplogin(USER user)
        {

            var logged_user = db.USERS.FirstOrDefault(u => u.email == user.email && u.nationalID == user.nationalID);


            if (logged_user == null)
            {
                ViewBag.massage = "يرجى التأكد من صحة الرقم الوطني الخاص بك.";
                return View();
            }
           

            if (user.password.ToLower() == "password" && logged_user.password == "password")
            {
                Random rand = new Random();

                string randomNumber = Convert.ToString(rand.Next(10000000, 100000000)); // Upper bound is exclusive
                logged_user.password = randomNumber;
                db.Entry(logged_user).State = EntityState.Modified;
                db.SaveChanges();
                try
                {
                    string fromEmail = "ayahaldomi@gmail.com";
                    string fromName = "test";
                    string subjectText = "subject";
                    string messageText = $@"
                    <html>
                    <body dir='rtl'>
                        <h2>مرحباً!</h2>
                        <p>شكراً لانضمامك إلينا. للبدء، يرجى استخدام كلمة المرور المؤقتة التالية لضبط حسابك:</p>
                        <p><strong>كلمة المرور: {randomNumber}</strong></p>
                        <p>إذا كانت لديك أي أسئلة أو تحتاج إلى مساعدة إضافية، لا تتردد في الاتصال بفريق الدعم لدينا.</p>
                        <p>مع أطيب التحيات,<br>فريق الدعم</p>
                    </body>
                    </html>";
                    string toEmail = "election2024jordan@gmail.com";
                    string smtpServer = "smtp.gmail.com";
                    int smtpPort = 465; // Port 465 for SSL

                    string smtpUsername = "election2024jordan@gmail.com";
                    string smtpPassword = "zwht jwiz ivfr viyt"; // Ensure this is correct

                    var message = new MimeMessage();
                    message.From.Add(new MailboxAddress(fromName, fromEmail));
                    message.To.Add(new MailboxAddress("", toEmail));
                    message.Subject = subjectText;
                    message.Body = new TextPart("html") { Text = messageText };

                    using (var client = new SmtpClient())
                    {
                        client.Connect(smtpServer, smtpPort, true); // Use SSL
                        client.Authenticate(smtpUsername, smtpPassword);
                        client.Send(message);
                        client.Disconnect(true);
                    }

                   


                    Session["logedInUser"] = logged_user.nationalID;
                    Session["electionArea"] = logged_user.electionArea;



                    return RedirectToAction("PasswordReset");
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "An unexpected error occurred. Please try again later.";
                    Console.WriteLine("Exception message: " + ex.Message);
                    Console.WriteLine("Stack trace: " + ex.StackTrace);
                    return View("Index");
                }
            }
            else if (user.password == logged_user.password)
            {
                Session["logedInUser"] = logged_user.nationalID;
                Session["electionArea"] = logged_user.electionArea;
                Session["name"] = logged_user.name;

                //HttpCookie startDateTime = Request.Cookies["startDateTime"];

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
                        return RedirectToAction("electionArea");
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");

                    }

                }
                else
                {
                    return RedirectToAction("Index", "Home");

                }

            }
            else
            {
                ViewBag.Title = "please enter valed info";
                return View();
            }


        }

        // GET: PasswordReset
        public ActionResult PasswordReset()
        {
            return View();
        }
        [HttpPost]
        public ActionResult PasswordReset(string TempPassword)
        {

            var logedInUser = Session["logedInUser"].ToString();

            var nationalId = logedInUser; // Extract NationalID from cookie value
            var logged_user = db.USERS.FirstOrDefault(u => u.nationalID == nationalId);
            if (TempPassword == logged_user.password)
            {
                return RedirectToAction("ResetPassword");
            }
            return View();
        }

        public ActionResult ResetPassword()
        {
            return View();
        }
        // POST: ResetPassword
        [HttpPost]
        public ActionResult ResetPassword(string NewPassword, string ConfirmPassword)
        {
            // Check if the new passwords match
            if (NewPassword != ConfirmPassword)
            {
                ViewBag.Message = "Passwords do not match.";
                return View("aaaaaaaaaaa");
            }

            // Extract the user ID from the cookie
            var logedInUser = Session["logedInUser"].ToString();


            // Find the user by ID

            var nationalId = logedInUser; // Extract NationalID from cookie value
            var logged_user = db.USERS.FirstOrDefault(u => u.nationalID == nationalId);
            if (logged_user == null)
            {
                ViewBag.Message = "User not found.";
                return View("ddddddd");
            }

            // Update the user's password
            logged_user.password = NewPassword;
            db.Entry(logged_user).State = EntityState.Modified;
            db.SaveChanges();

            // Set success message
            ViewBag.Message = "Password has been successfully reset.";

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
                    return RedirectToAction("electionArea");
                }
                else
                {
                    return RedirectToAction("Index", "Home");

                }

            }
            else
            {
                return RedirectToAction("Index", "Home");

            }
        }

        //////////////////////////////////////////////////////////////////////
        ///

        public ActionResult electionArea()
        {

            return View(db.ElectionAreas.ToList());

        }

        public ActionResult localOrparty()
        {

            return View(db.USERS.ToList());

        }

        [HttpPost]
        public ActionResult localOrparty(string hi)
        {
            var logedInUser = Session["logedInUser"].ToString();
            var nationalId = logedInUser; // Extract NationalID from cookie value
            var logged_user = db.USERS.FirstOrDefault(u => u.nationalID == nationalId);


            if (logged_user.localVote == 0)
            {
                logged_user.whiteLocalVote = 1;
                db.Entry(logged_user).State = EntityState.Modified;
                db.SaveChanges();
            }
            if (logged_user.partyVote == 0)
            {
                logged_user.whitePartyVote = 1;
                db.Entry(logged_user).State = EntityState.Modified;
                db.SaveChanges();
            }

            return RedirectToAction("LogoutUser");

        }

        public ActionResult partyVoting()
        {

            var partyLists = db.PartyLists.ToList(); // Get list of partyLists
            return View(partyLists); // Pass the list to the view

        }
        [HttpPost]
        public ActionResult partyVoting(int? selectedPartyId)
        {
            var partyVote = db.PartyLists.FirstOrDefault(u => u.id == selectedPartyId);
            var logedInUser = Session["logedInUser"].ToString();
            var nationalId = logedInUser; // Extract NationalID from cookie value
            var logged_user = db.USERS.FirstOrDefault(u => u.nationalID == nationalId);


            if (selectedPartyId == null)
            {
                logged_user.whitePartyVote = 1;
                db.Entry(logged_user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("localORparty");
            }
            partyVote.counter = partyVote.counter + 1;
            db.Entry(partyVote).State = EntityState.Modified;
            db.SaveChanges();


            logged_user.partyVote = 1;
            db.Entry(logged_user).State = EntityState.Modified;
            db.SaveChanges();



            // Get list of partyLists
            return RedirectToAction("localORparty"); // Pass the list to the view

        }

        public ActionResult localVoting()
        {
            var electionArea = Session["electionArea"].ToString();

            // Filter and group candidates by list where status is "approved"
            var candidatesGrouped = db.localLists
            .Where(l => l.status == "1" && l.electionDistrict == electionArea)
            .Select(l => new LocalCandidatesGroupedViewModel
            {
                ListName = l.listName,
                Candidates = db.localCandidates
                    .Where(c => c.listKey == l.id)
                    .ToList()
            })
            .ToList();

            return View(candidatesGrouped);

        }

        [HttpPost]
        public ActionResult localVoting(string selectedList, long[] selectedCandidates)
        {
            var logedInUser = Session["logedInUser"].ToString();
            var nationalId = logedInUser; // Extract NationalID from cookie value
            var logged_user = db.USERS.FirstOrDefault(u => u.nationalID == nationalId);

            if (selectedList != null)
            {
                var selectedListDetails = db.localLists.FirstOrDefault(l => l.listName == selectedList);
                selectedListDetails.counter = selectedListDetails.counter + 1;
                db.Entry(selectedListDetails).State = EntityState.Modified;
                db.SaveChanges();

                if (selectedCandidates != null)
                {

                    // Increment the counter for each selected candidate
                    foreach (var candidateId in selectedCandidates)
                    {
                        var candidate = db.localCandidates
                            .FirstOrDefault(c => c.id == candidateId);



                        candidate.counter = candidate.counter + 1;
                        db.Entry(candidate).State = EntityState.Modified;
                        db.SaveChanges();

                    }
                    

                }
                logged_user.localVote = 1;
                db.Entry(logged_user).State = EntityState.Modified;
                db.SaveChanges();
            }
            else
            {
                logged_user.whiteLocalVote = 1;
                db.Entry(logged_user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("localORparty");
            }
            return RedirectToAction("localORparty");

        }

        public ActionResult LoginAdmin()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LoginAdmin(Admin model)
        {
            if (ModelState.IsValid)
            {
                var admin = db.Admins
                    .Where(a => a.email == model.email && a.password == model.password)
                    .SingleOrDefault();

                if (admin != null)
                {
                    // تسجيل الدخول ناجح، يمكنك التوجيه إلى صفحة أخرى أو إنشاء جلسة
                    Session["Admin"] = admin;
                    return RedirectToAction("IndexAdmin", "party"); // أو أي صفحة تريد الانتقال إليها بعد تسجيل الدخول
                }
                else
                {
                    // البيانات غير صحيحة
                    ModelState.AddModelError("", "البريد الإلكتروني أو كلمة المرور غير صحيحة");
                }
            }

            return View();
        }
        public ActionResult Logout()
        {
            // قم بحذف بيانات الجلسة
            Session.Remove("Admin");

            // إعادة التوجيه إلى صفحة تسجيل الدخول أو الصفحة الرئيسية
            return RedirectToAction("Index", "Home");
        }

        public ActionResult LogoutUser()
        {
            // قم بحذف بيانات الجلسة
            Session.Remove("logedInUser");
            Session.Remove("electionArea");
            Session.Remove("name");

            // إعادة التوجيه إلى صفحة تسجيل الدخول أو الصفحة الرئيسية
            return RedirectToAction("Index", "Home");
        }

    }

 
}