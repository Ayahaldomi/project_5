using project5_voting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace project5_voting.Controllers
{
    public class ElectionAreaController : Controller
    {
        ElectionEntities1 db = new ElectionEntities1();

        public ActionResult localANDparty()
        {
            return View();
        }

            public ActionResult areaName()
            {

                return View(db.ElectionAreas.ToList());
            }

            public ActionResult listsDetails(string id)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                var localList = db.localLists.Where(x => x.electionDistrict == id && x.status != "0").ToList();

                return View(localList);
            }

        public ActionResult CandidatesName(int? id)
        {
            var CandidatesNames = db.localCandidates.Where(x => x.listKey == id ).ToList();

            return View(CandidatesNames);
        }



        public ActionResult partyName()
        {
            var check = db.PartyLists.Where(model => model.status != "0").ToList();
            return View(check);
        }


        public ActionResult PatryCandidatesName(int? id)
        {
            var PartyCandidatesNames = db.PartyCandidates.Where(x => x.partyListID == id && x.status != "0").ToList();

            return View(PartyCandidatesNames);
        }
    }
}