using Google.Api.Ads.AdWords.v201705;

namespace GAABA.WebAPI.Models
{
    public class AdwordsCampaignModel
    {
        public long Id { get; set; }
        public CampaignStatus Status { get; set; }
        public string Name { get; set; }
    }
}