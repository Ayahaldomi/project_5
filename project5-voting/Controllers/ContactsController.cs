using project5_voting.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MailKit.Net.Smtp;
using Microsoft.Ajax.Utilities;
using MimeKit;
using System.Text;

namespace project5_voting.Controllers
{
    public class ContactsController : Controller
    {
        ElectionEntities1 db = new ElectionEntities1();
        public ActionResult Contact()
        {
            return View();
        }


        [HttpPost]
        public ActionResult Contact(Contact contact)
        {
            if (ModelState.IsValid)
            {
                contact.date = DateTime.Today;
                contact.time = DateTime.Now.TimeOfDay;

                db.Contacts.Add(contact);
                db.SaveChanges();
            }

            return RedirectToAction("Contact");
        }


        public ActionResult AdminContact()
        {
            return View(db.Contacts.ToList());
        }

        public ActionResult Response(int? id)
        {

            var admin_name = Session["Admin"] as string;

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Contact contact = db.Contacts.Find(id);
            contact.adminName = admin_name;

            if (contact == null)
            {
                return HttpNotFound();
            }


            string fromEmail = "election2024jordan@gmail.com";
            string fromName = $"{contact.name}";
            string subjectText = $"{contact.subject}";
            string messageText = $@"
                    <html>
                    <body dir='rtl'>
                        <h2> مرحباَ {contact.name}! </h2>
                        <p>شكراً لتواصلك معنا:</p>
                        <p>{contact.adminResponse}</p>
                        <p>إذا كانت لديك أي أسئلة أو تحتاج إلى مساعدة إضافية، لا تتردد في الاتصال بفريق الدعم لدينا.</p>
                        <p>مع أطيب التحيات,<br>فريق الدعم</p>
                    </body>
                    </html>";

            string toEmail = $"{contact.email}";
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


            return View(contact);



        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Response(Contact contact)
        {
            contact.rsponseDate = DateTime.Today;
            contact.rsponseTime = DateTime.Now.TimeOfDay;
            contact.status = "1";

            if (ModelState.IsValid)
            {
                db.Entry(contact).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("AdminContact");
            }
            return View(contact);
        }


        public ActionResult ContactDetails(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Contact contact = db.Contacts.Find(id);

            if (contact == null)
            {
                return HttpNotFound();
            }

            return View(contact);
        }

        public ActionResult ShowResponse(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Contact contact = db.Contacts.Find(id);
            if (contact == null)
            {
                return HttpNotFound();
            }
            return View(contact);
        }


        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Contact contact = db.Contacts.Find(id);
            if (contact == null)
            {
                return HttpNotFound();
            }
            return View(contact);
        }

        // POST: Contacts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Contact contact = db.Contacts.Find(id);
            db.Contacts.Remove(contact);
            db.SaveChanges();
            return RedirectToAction("AdminContact");
        }
    }
}