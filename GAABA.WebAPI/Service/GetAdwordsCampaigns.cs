using GAABA.WebAPI.Models;
using Google.Api.Ads.AdWords.Lib;
using Google.Api.Ads.AdWords.v201705;
using System;
using System.Collections.Generic;

namespace GAABA.WebAPI.Service
{
    public class GetAdwordsCampaigns
    {
        /// <summary>
        /// Runs the code example.
        /// </summary>
        /// <param name="user">The AdWords user.</param>
        public List<AdwordsCampaignModel> Run(AdWordsUser user)
        {
            // Get the CampaignService.
            CampaignService campaignService =
                (CampaignService)user.GetService(AdWordsService.v201705.CampaignService);

            // Create the selector.
            Selector selector = new Selector()
            {
                fields = new string[] {
                    Campaign.Fields.Id, Campaign.Fields.Name, Campaign.Fields.Status
                },
                paging = Paging.Default
            };

            CampaignPage page = new CampaignPage();

            try
            {
                var campaignsList = new List<AdwordsCampaignModel>();

                do
                {
                    // Get the campaigns.
                    page = campaignService.get(selector);

                    if (page != null && page.entries != null)
                    {
                        foreach (Campaign camp in page.entries)
                        {
                            campaignsList.Add(new AdwordsCampaignModel()
                            {
                                Id = camp.id,
                                Name = camp.name,
                                Status = camp.status
                            });
                        }
                    }

                    //selector.paging.IncreaseOffset();                   
                    //while (selector.paging.startIndex < page.totalNumEntries);
                    while (selector.paging.numberResults > 0) ;
                    return campaignsList;





            catch (Exception e)
            {
                throw new System.ApplicationException("Failed to retrieve campaigns", e);
            }
        }
    }
}

