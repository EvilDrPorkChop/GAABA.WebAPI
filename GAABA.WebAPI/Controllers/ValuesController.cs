using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Google.Api.Ads.AdWords.v201705;
using Google.Api.Ads.AdWords.Lib;
using System.Configuration;

namespace GAABA.WebAPI.Controllers
{
    //[Authorize]
    public class ValuesController : ApiController
    {
        // GET api/values
        public IEnumerable<string> Get()
        {
            //AdWordsUser user = null;
            //CampaignService campaignService = (CampaignService)user.GetService(AdWordsService.v201705.CampaignService);

            //var abc = new CampaignService();
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        public string Get(int id)
        {
            var idNum = ConfigurationManager.AppSettings["GoogleClientID"];
            return idNum;
        }

        // POST api/values
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
