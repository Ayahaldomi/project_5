using project5_voting.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace project5_voting.Controllers
{
    public class AdsController : Controller
    {
           
        private ElectionEntities1 db = new ElectionEntities1();

        private readonly string PayPalBaseUrl = "https://api.sandbox.paypal.com/";
        private readonly string ClientId = "ARPUvF-CVZ4yA8o3ZTmyxU_9aw2AJZYjW8pDJ09OXyPNPy9aZYNPxSdc-ZCBpF8YT64dnwh6EEfXEZo5";
        private readonly string Secret = "EGQ1yUiAMYaL706LnAuZ7Iw6iP9EY055w16japE4OxnTvCbZ8INxSm7pq5TN1twjyirDkRSZo-GqA4-T";
        // GET: Ads
        public ActionResult advertisementAdmin()
        {
            return View(db.Ads.ToList());
        }

        // GET: Ads/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ad ad = db.Ads.Find(id);
            if (ad == null)
            {
                return HttpNotFound();
            }
            return View(ad);
        }

        // GET: Ads/Create
        public ActionResult addAdvertisement()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> addAdvertisement([Bind(Include = "name,nationalId,campaignSlagan,listName,partyName,areaName,description,image,status")] Ad ad, HttpPostedFileBase upload)
        {
            if (ModelState.IsValid)
            {
                if (upload != null && upload.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(upload.FileName);

                    var path = Path.Combine(Server.MapPath("~/Images/"), fileName);


                    if (!Directory.Exists(Server.MapPath("~/Images/")))
                    {
                        Directory.CreateDirectory(Server.MapPath("~/Images/"));
                    }

                    ad.status = "معلق";
                    upload.SaveAs(path);
                    ad.image = fileName;
                }
                db.Ads.Add(ad);
                db.SaveChanges();

            }
            try
            {
                var accessToken = await GetAccessToken1();

                var paymentPayload = new
                {
                    intent = "sale",
                    payer = new
                    {
                        payment_method = "paypal"
                    },
                    transactions = new[]
                    {
                        new
                        {
                            amount = new
                            {
                                total = "300",
                                currency = "USD"
                            },
                            description = "Payment description"
                        }
                    },
                    redirect_urls = new
                    {
                        return_url = "https://example.com/returnUrl",
                        cancel_url = "https://example.com/cancelUrl"
                    }
                };

                var paymentJson = Newtonsoft.Json.JsonConvert.SerializeObject(paymentPayload);

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(PayPalBaseUrl);
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    var content = new StringContent(paymentJson, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync("/v1/payments/payment", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        var responseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(responseContent);

                        string approvalUrl = null;
                        var linksArray = responseObject.links as Newtonsoft.Json.Linq.JArray;
                        if (linksArray != null)
                        {
                            var approvalLink = linksArray.FirstOrDefault(l => (string)l["rel"] == "approval_url");
                            if (approvalLink != null)
                            {
                                approvalUrl = approvalLink["href"].ToString();
                            }
                        }

                        if (!string.IsNullOrEmpty(approvalUrl))
                        {
                            return Redirect(approvalUrl);
                        }
                        else
                        {
                            ViewBag.ErrorMessage = "Approval URL not found in PayPal response.";
                            return View("Error");
                        }
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Failed to initiate PayPal payment: " + response.ReasonPhrase;
                        return View("Error");
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred: " + ex.Message;
                return View("Error");
            }
        }

        public ActionResult Publish(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ad ad = db.Ads.Find(id);
            if (ad == null)
            {
                return HttpNotFound();
            }

            // Update the status of the ad to published
            ad.status = "تم نشر الاعلان";
            db.Entry(ad).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("advertisementAdmin");
        }

        public ActionResult Publishs()
        {
            // إرجاع جميع الإعلانات من قاعدة البيانات
            var ads = db.Ads.ToList();
            return View(ads);
        }


        // GET: Ads/Edit/5
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ad ad = db.Ads.Find(id);
            if (ad == null)
            {
                return HttpNotFound();
            }
            return View(ad);
        }


        // GET: Ads/Delete/5
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ad ad = db.Ads.Find(id);
            if (ad == null)
            {
                return HttpNotFound();
            }
            return View(ad);
        }

        // POST: Ads/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            Ad ad = db.Ads.Find(id);
            db.Ads.Remove(ad);
            db.SaveChanges();
            return RedirectToAction("advertisementAdmin");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }








        //paymnt


        public async Task<ActionResult> Checkout()
        {
            try
            {
                var accessToken = await GetAccessToken();

                var paymentPayload = new
                {
                    intent = "sale",
                    payer = new
                    {
                        payment_method = "paypal"
                    },
                    transactions = new[]
                    {
                        new
                        {
                            amount = new
                            {
                                total = "300",
                                currency = "USD"
                            },
                            description = "Payment description"
                        }
                    },
                    redirect_urls = new
                    {
                        return_url = "https://example.com/returnUrl",
                        cancel_url = "https://example.com/cancelUrl"
                    }
                };

                var paymentJson = Newtonsoft.Json.JsonConvert.SerializeObject(paymentPayload);

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(PayPalBaseUrl);
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    var content = new StringContent(paymentJson, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync("/v1/payments/payment", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        var responseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(responseContent);

                        string approvalUrl = null;
                        var linksArray = responseObject.links as Newtonsoft.Json.Linq.JArray;
                        if (linksArray != null)
                        {
                            var approvalLink = linksArray.FirstOrDefault(l => (string)l["rel"] == "approval_url");
                            if (approvalLink != null)
                            {
                                approvalUrl = approvalLink["href"].ToString();
                            }
                        }

                        if (!string.IsNullOrEmpty(approvalUrl))
                        {
                            return Redirect(approvalUrl);
                        }
                        else
                        {
                            ViewBag.ErrorMessage = "Approval URL not found in PayPal response.";
                            return View("Error");
                        }
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Failed to initiate PayPal payment: " + response.ReasonPhrase;
                        return View("Error");
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred: " + ex.Message;
                return View("Error");
            }
        }




        private async Task<string> GetAccessToken()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(PayPalBaseUrl);
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{ClientId}:{Secret}")));
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                var requestData = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials")
                };

                var requestContent = new FormUrlEncodedContent(requestData);
                var response = await client.PostAsync("/v1/oauth2/token", requestContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var responseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(responseContent);
                    return responseObject.access_token;
                }
                else
                {
                    throw new Exception("Failed to retrieve PayPal access token: " + response.ReasonPhrase);
                }
            }
        }

        public ActionResult ViewCheckout()
        {
            return View();
        }



        public async Task<ActionResult> Checkout1()
        {
            try
            {
                var accessToken = await GetAccessToken1();

                var paymentPayload = new
                {
                    intent = "sale",
                    payer = new
                    {
                        payment_method = "paypal"
                    },
                    transactions = new[]
                    {
                        new
                        {
                            amount = new
                            {
                                total = "1.1",
                                currency = "USD"
                            },
                            description = "Payment description"
                        }
                    },
                    redirect_urls = new
                    {
                        return_url = "https://example.com/returnUrl",
                        cancel_url = "https://example.com/cancelUrl"
                    }
                };

                var paymentJson = Newtonsoft.Json.JsonConvert.SerializeObject(paymentPayload);

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(PayPalBaseUrl);
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    var content = new StringContent(paymentJson, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync("/v1/payments/payment", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        var responseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(responseContent);

                        string approvalUrl = null;
                        var linksArray = responseObject.links as Newtonsoft.Json.Linq.JArray;
                        if (linksArray != null)
                        {
                            var approvalLink = linksArray.FirstOrDefault(l => (string)l["rel"] == "approval_url");
                            if (approvalLink != null)
                            {
                                approvalUrl = approvalLink["href"].ToString();
                            }
                        }

                        if (!string.IsNullOrEmpty(approvalUrl))
                        {
                            return Redirect(approvalUrl);
                        }
                        else
                        {
                            ViewBag.ErrorMessage = "Approval URL not found in PayPal response.";
                            return View("Error");
                        }
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Failed to initiate PayPal payment: " + response.ReasonPhrase;
                        return View("Error");
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred: " + ex.Message;
                return View("Error");
            }
        }


        private async Task<string> GetAccessToken1()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(PayPalBaseUrl);
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{ClientId}:{Secret}")));
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                var requestData = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials")
                };

                var requestContent = new FormUrlEncodedContent(requestData);
                var response = await client.PostAsync("/v1/oauth2/token", requestContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var responseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(responseContent);
                    return responseObject.access_token;
                }
                else
                {
                    throw new Exception("Failed to retrieve PayPal access token: " + response.ReasonPhrase);
                }
            }
        }







    }
}