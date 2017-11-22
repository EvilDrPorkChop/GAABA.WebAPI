using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Google.Api.Ads.AdWords.Lib;
using Google.Api.Ads.AdWords.v201705;

namespace GAABA.WebAPI.Controllers
{
    public class TestController : Controller
    {
        // GET: Test
        public CampaignPage Index()
        {

            var user = new AdWordsUser();

            CampaignService campaignService = (CampaignService)user.GetService(AdWordsService.v201705.CampaignService);

            // Create the selector.
            Selector selector = new Selector()
            {
                fields = new string[] {
                    Campaign.Fields.Id, Campaign.Fields.Name, Campaign.Fields.Status
                },
                paging = Paging.Default
            };

            CampaignPage page = new CampaignPage();

            // Get the campaigns.
            page = campaignService.get(selector);

            return page;
        }

        // GET: Test/Details/5
        public int Details(int id)
        {
            return 5;
        }

        // GET: Test/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Test/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Test/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Test/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Test/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Test/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
