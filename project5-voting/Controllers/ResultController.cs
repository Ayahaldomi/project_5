using project5_voting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace project5_voting.Controllers
{
    public class ResultController : Controller
    {
        private ElectionEntities1 db = new ElectionEntities1();


        public ActionResult Result()
        {
            TempData["partyPercent"] = partyPercentage();
            TempData["localPercentIrbid1"] = localPercentage("اربد الاولى");
            TempData["localPercentIrbid2"] = localPercentage("اربد الثانية");
            TempData["localPercentMafraq"] = localPercentage("المفرق");

            return View();
        }

        public ActionResult detailedResults(string id)
        {

            ViewBag.currentID = id;

            return View(localListsAndCanditates(id));
        }



        public ActionResult womanSeat(string id)
        {
            var x = localWomanSeat(id).ToList();
            return View(x);
        }



        public ActionResult christianSeat(string id)
        {
            var x = localChristianSeat(id).ToList();

            return View(x);
        }




        public ActionResult staticResults()
        { return View(); }





        public ActionResult partyResults()
        {
            return View(partyListsAndCanditates().ToList());
        }


        public long usersNumber()
        {
            long countUsers = 0;

            var allUsers = db.ElectionAreas.ToList();

            foreach (var row in allUsers)
            {
                countUsers++;
            }
            return countUsers;
        }

        public long localUsersNumber(string d)
        {
            long countUsers = 0;

            var allUsers = db.USERS.ToList();

            foreach (var row in allUsers)
            {
                if (row.electionArea == d)
                {
                    countUsers++;
                }
            }
            return countUsers;
        }


        public double localPercentage(string d)
        {
            double percentage = (double)localVotersCount(d) / (double)localUsersNumber(d);

            Session["localPercentIrbid1"] = Math.Round(((double)localVotersCount("اربد الاولى") / (double)localUsersNumber("اربد الاولى")), 2) * 100;
            Session["localPercentIrbid2"] = Math.Round(((double)localVotersCount("اربد الثانية") / (double)localUsersNumber("اربد الثانية")), 2) * 100;
            Session["localPercentMafraq"] = Math.Round(((double)localVotersCount("المفرق") / (double)localUsersNumber("المفرق")), 2) * 100;

            return (Math.Round(percentage, 2)) * 100;
        }


        public double partyPercentage()
        {
            double percentage = (double)partyVotersCount() / (double)usersNumber();

            Session["partyPercent"] = Math.Round(percentage, 2) * 100;

            return (Math.Round(percentage, 2)) * 100;
        }



        //عشان اجيب عدد اصوات المحليين في حسب الدائرة
        public long localVotersCount(string d)
        {
            long localVotes = 0;

            var allUsers = db.USERS.ToList();

            foreach (var row in allUsers)
            {
                if (row.electionArea.Trim() == d)
                {
                    localVotes += Convert.ToInt64(row.localVote) + Convert.ToInt64(row.whiteLocalVote);
                }
            }
            return localVotes;
        }

        //لحساب العتبة اعتمادا على الفنكشن الاول
        public double localThreshold(string d)
        {
            double localThreshold = localVotersCount(d) * 0.07;
            return localThreshold;
        }


        public List<string> localListsAboveThreshold(string d)
        {
            List<string> thresholdLocal = new List<string>();

            var allLocalLists = db.localLists.ToList();

            foreach (var row in allLocalLists)
            {
                if (row.electionDistrict == d && row.counter > localThreshold(d))
                {
                    string addedList = $"{row.id}, {row.listName}, {row.electionDistrict}, {row.counter}";

                    thresholdLocal.Add(addedList);
                }
            }

            return thresholdLocal;

        }

        //لحساب مجموع الاصوات للقوائم الي فازت
        public long localWinningListsVotesSum(string d)
        {
            long winningListsVotesSum = 0;

            var winningLists = localListsAboveThreshold(d).ToList();

            foreach (var row in winningLists)
            {
                var winningListsRows = String.Join(", ", row);//للتحويل من string to string array

                string[] listsArray = winningListsRows.Split(',');

                long listVotes = long.Parse(listsArray.Last());

                winningListsVotesSum += listVotes;

            }

            return winningListsVotesSum;

        }

        //لحساب عدد المقاعد بكل قائمة 
        public List<string> localListsSeats(string d)
        {
            long seatsAvailable = 0;

            switch (d)
            {
                case "اربد الاولى":
                    seatsAvailable = 7;
                    break;
                case "اربد الثانية":
                    seatsAvailable = 5;
                    break;
                case "المفرق":
                    seatsAvailable = 3;
                    break;
            }

            var Lists = localListsAboveThreshold(d).ToList();

            var winningLists = new List<string>();

            foreach (var row in Lists)
            {
                string listRow = row;


                var winningListsRows = String.Join(", ", row);

                string[] listsArray = winningListsRows.Split(',');

                long listVotes = long.Parse(listsArray.Last());


                long sum = localWinningListsVotesSum(d);


                decimal percentage = (decimal)listVotes / (decimal)sum;

                decimal seatsWon = percentage * (decimal)seatsAvailable;


                listRow += $", {Math.Round(seatsWon)}";

                winningLists.Add(listRow);
            }

            return winningLists;
        }

        //بدنا نجيب اسماء المرشحين 
        public List<string> localListsCanditates(string d)
        {
            var localCan = localListsAboveThreshold(d).ToList();
            var localCanditates = db.localCandidates.ToList();

            List<string> localWinningCanditates = new List<string>();

            foreach (var row in localCan)
            {
                string listRow = row;

                var winningListsRows = String.Join(", ", row);

                string[] listsArray = winningListsRows.Split(',');

                string listName = listsArray[1].Trim();//استدعاء قيمة اليست نيم

                foreach (var can in localCanditates)
                {
                    if (can.election_area == d && can.listName == listName)
                    {
                        string canditateNameAndId = $"{can.id}, {can.candidateName}, {can.listName}, {can.typeOfChair}, {can.counter}";

                        localWinningCanditates.Add(canditateNameAndId);
                    }

                }

            }
            localWinningCanditates = localWinningCanditates.OrderByDescending(can => long.Parse(can.Split(',').Last().Trim())).ToList();

            return localWinningCanditates;
        }

        //هاي اخر اشي لحتى نرتب اسماء القوائم و المرشحين الفائزين 
        public List<string> localListsAndCanditates(string d)
        {
            var localListsWinners = localListsSeats(d).ToList();
            var localCanditates = localListsCanditates(d).ToList();

            List<string> localWinners = new List<string>();

            foreach (var row in localListsWinners)
            {
                string listRow = row;

                var winningListsRows = String.Join(", ", row);

                string[] listsArray = winningListsRows.Split(',');

                long listsSeats = long.Parse(listsArray.Last());

                string listName = listsArray[1].Trim();

                long seatsCount = 0;

                foreach (var can in localCanditates)
                {
                    string canRow = can;

                    var canRowsList = string.Join(", ", canRow);
                    string[] canArray = canRowsList.Split(',');

                    if (canArray[2].Trim() == listName && canArray[3].Trim() == "تنافس")
                    {
                        string canditateNameAndId = $"{listsArray[0]}, {listsArray[1]}, {listsArray[3]}, {d}, {canArray[0]}, {canArray[1]}, {canArray[3]}, {canArray[4]}";

                        localWinners.Add(canditateNameAndId);

                        seatsCount++;

                        if (seatsCount == listsSeats)
                        {
                            break;
                        }
                    }

                }

            }
            return localWinners;
        }

        //للكوتا
        public List<string> localWomanSeat(string d)
        {
            var localCanditate = localListsCanditates(d).ToList();

            List<string> localWomen = new List<string>();

            List<string> winningWoman = new List<string>();

            foreach (var row in localCanditate)
            {
                string canRow = row;
                var canLists = string.Join(",", canRow);
                string[] canArray = canLists.Split(',');

                if (canArray[3].Trim() == "كوتا")
                {
                    string womanCan = $"{canArray[0]}, {canArray[1]}, {canArray[2]}, {d}, {canArray[4]}";
                    localWomen.Add(womanCan);
                }

            }

            localWomen = localWomen.OrderByDescending(can => long.Parse(can.Split(',').Last().Trim())).ToList();

            winningWoman.Add(localWomen[0]);

            return winningWoman;
        }


        public string localChristianSeat(string d)
        {
            var localCanditate = localListsCanditates(d).ToList();

            List<string> localChristian = new List<string>();

            string winningChristian = "";

            foreach (var row in localCanditate)
            {
                string canRow = row;
                var canLists = string.Join(",", canRow);
                string[] canArray = canLists.Split(',');

                if (canArray[3].Trim() == "مسيحي")
                {
                    string chrisCan = $"{canArray[0]}, {canArray[1]}, {canArray[2]}, {d}, {canArray[4]}";
                    localChristian.Add(chrisCan);
                }

            }

            localChristian = localChristian.OrderByDescending(can => long.Parse(can.Split(',').Last().Trim())).ToList();

            winningChristian = localChristian[0];


            return winningChristian;
        }


        /* ///////////////////////////////////////////////////////////////////////// */
        /* ///////////////////////////////////////////////////////////////////////// */
        /* ///////////////////////////////////////////////////////////////////////// */



        public long partyVotersCount()
        {
            long partyVotes = 0;

            var allUsers = db.USERS.ToList();

            foreach (var row in allUsers)
            {
                partyVotes += Convert.ToInt64(row.partyVote);
            }

            return partyVotes;
        }


        public double partyThreshold()
        {
            double partyThreshold = partyVotersCount() * 0.025;
            return partyThreshold;
        }


        public List<string> partyListsAboveThreshold()
        {
            List<string> thresholdParty = new List<string>();

            var allPartyLists = db.PartyLists.ToList();

            foreach (var row in allPartyLists)
            {
                if (row.counter > partyThreshold())
                {
                    string addedList = $"{row.id}, {row.partyName}, {row.counter}";

                    thresholdParty.Add(addedList);
                }
            }

            return thresholdParty;
        }


        public long partyWinningListsVotesSum()
        {
            long winningListsVotesSum = 0;

            var winningLists = partyListsAboveThreshold().ToList();

            foreach (var row in winningLists)
            {
                var winningListsRows = String.Join(", ", row);

                string[] listsArray = winningListsRows.Split(',');

                long listVotes = long.Parse(listsArray.Last());

                winningListsVotesSum += listVotes;

            }

            return winningListsVotesSum;

        }


        public List<string> partyListsSeats()
        {
            var Lists = partyListsAboveThreshold().ToList();

            var winningLists = new List<string>();

            foreach (var row in Lists)
            {
                string listRow = row;


                var winningListsRows = String.Join(", ", row);

                string[] listsArray = winningListsRows.Split(',');

                long listVotes = long.Parse(listsArray.Last());


                long sum = partyWinningListsVotesSum();


                decimal percentage = (decimal)listVotes / (decimal)sum;

                decimal seatsWon = percentage * (decimal)41;


                listRow += $", {Math.Round(seatsWon)}";

                winningLists.Add(listRow);
            }

            return winningLists;
        }


        public List<string> partyListsCanditates()
        {
            var partyCan = partyListsAboveThreshold().ToList();
            var partyCanditates = db.PartyCandidates.ToList();

            List<string> partyWinningCanditates = new List<string>();

            foreach (var row in partyCan)
            {
                string listRow = row;

                var winningListsRows = String.Join(", ", row);

                string[] listsArray = winningListsRows.Split(',');

                long listID = Convert.ToInt64(listsArray[0].Trim());

                foreach (var can in partyCanditates)
                {
                    if (can.partyListID == listID)
                    {
                        string canditateNameAndId = $"{can.id}, {can.name}, {can.partyListID}, {can.id}";

                        partyWinningCanditates.Add(canditateNameAndId);
                    }

                }

            }
            partyWinningCanditates = partyWinningCanditates.OrderBy(can => long.Parse(can.Split(',').Last().Trim())).ToList();

            return partyWinningCanditates;
        }


        public List<string> partyListsAndCanditates()
        {
            var partyListsWinners = partyListsSeats().ToList();
            var partyCanditates = partyListsCanditates().ToList();

            List<string> partyWinners = new List<string>();

            foreach (var row in partyListsWinners)
            {
                string listRow = row;

                var winningListsRows = String.Join(", ", row);

                string[] listsArray = winningListsRows.Split(',');

                long listsSeats = long.Parse(listsArray.Last());

                string listName = listsArray[0];

                long seatsCount = 0;

                foreach (var can in partyCanditates)
                {
                    string canRow = can;

                    var canRowsList = string.Join(", ", canRow);
                    string[] canArray = canRowsList.Split(',');

                    if (canArray[2] == listName)
                    {
                        string canditateNameAndId = $"{listsArray[0]}, {listsArray[1]}, {listsArray[2]}, {canArray[0]}, {canArray[1]}";

                        partyWinners.Add(canditateNameAndId);

                        seatsCount++;

                        if (seatsCount == listsSeats)
                        {
                            break;
                        }
                    }

                }

            }
            return partyWinners;
        }

    }
}