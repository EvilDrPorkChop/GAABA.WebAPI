using Google.Api.Ads.AdWords.Lib;
using Google.Api.Ads.AdWords.v201705;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace GAABA.WebAPI.Service
{
    /// <summary>
    /// This code example lists all campaigns. To add a campaign, run
    /// AddCampaign.cs.
    /// </summary>
    /// <summary>
    public class GetCampaigns : ExampleBase
    {
        /// Main method, to run this code example as a standalone application.
        /// </summary>
        ///<param name="args">The command line arguments.</param>
        public static void Main(string[] args)
        {
            GetCampaigns codeExample = new GetCampaigns();
            Console.WriteLine(codeExample.Description);
            try
            {
                codeExample.Run(new AdWordsUser());
            }
            catch (Exception e)
            {
                Console.WriteLine("An exception occurred while running this code example. {0}",
                    ExampleUtilities.FormatException(e));
            }
        }

        /// <summary>
        /// Returns a description about the code example.
        /// </summary>
        public override string Description
        {
            get
            {
                return "This code example lists all campaigns. To add a campaign, run AddCampaign.cs.";
            }
        }

        /// <summary>
        /// Runs the code example.
        /// </summary>
        /// <param name="user">The AdWords user.</param>
        public void Run(AdWordsUser user)
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
                do
                {
                    // Get the campaigns.
                    page = campaignService.get(selector);

                    // Display the results.
                    if (page != null && page.entries != null)
                    {
                        int i = selector.paging.startIndex;
                        foreach (Campaign campaign in page.entries)
                        {
                            Console.WriteLine("{0}) Campaign with id = '{1}', name = '{2}' and status = '{3}'" +
                              " was found.", i + 1, campaign.id, campaign.name, campaign.status);
                            i++;
                        }
                    }
                    selector.paging.IncreaseOffset();
                } while (selector.paging.startIndex < page.totalNumEntries);
                Console.WriteLine("Number of campaigns found: {0}", page.totalNumEntries);
            }
            catch (Exception e)
            {
                throw new System.ApplicationException("Failed to retrieve campaigns", e);
            }
        }
    }
}