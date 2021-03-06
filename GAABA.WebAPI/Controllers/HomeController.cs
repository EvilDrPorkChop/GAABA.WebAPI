﻿using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.BingAds;
using Microsoft.BingAds.V11;
using Microsoft.BingAds.V11.Bulk;
using Microsoft.BingAds.V11.Bulk.Entities;
using Microsoft.BingAds.V11.CampaignManagement;
using Microsoft.BingAds.V11.CustomerManagement;
using System.Net.Http;

namespace BingAdsWebApp.Controllers
{
    public class HomeController : Controller
    {
        private const string ClientId = "<0d7f794e-af11-47df-97a9-60ce8772c8bc>";
        private const string ClientSecret = "<TnZbVQgeCetfS78dzmwA3ek>";
        private const string RedirectionUri = "<https://gaaba.local>";
        private const string DeveloperToken = "<1104985T8Q708909>";
        private static string ClientState = "OptionalClientStateGoesHere";

        private static AuthorizationData _authorizationData;
        private readonly string _refreshTokenFilePath = Path.Combine(Path.GetTempPath(), "refreshToken.txt");

        private static long?[,] _accountCustomerIds;
        private static ServiceClient<ICustomerManagementService> _customerService;
        private static BulkServiceManager _bulkService;

        /// <summary>
        /// Controls the contents displayed at Index.cshtml.
        /// </summary>
        public async Task<ActionResult> bing()
        {
            try
            {
                // If there is already an authenticated Microsoft account during this HTTP session, 
                // go ahead and call Bing Ads service operations.

                if (Session["auth"] != null)
                {
                    return await CallBingAdsServices((OAuthWebAuthCodeGrant)Session["auth"]);
                }

                // Prepare the OAuth object for use with the authorization code grant flow. 

                var oAuthWebAuthCodeGrant = new OAuthWebAuthCodeGrant(ClientId, ClientSecret, new Uri(RedirectionUri));

                // It is recommended that you specify a non guessable 'state' request parameter to help prevent
                // cross site request forgery (CSRF). 
                oAuthWebAuthCodeGrant.State = ClientState;

                // When calling Bing Ads services with ServiceClient or BulkServiceManager, each will refresh your access token 
                // automatically if they detect the AuthenticationTokenExpired (109) error code. 
                // As a best practice you should always use the most recent provided refresh token.
                // Save the refresh token whenever new OAuth tokens are received by subscribing to the NewOAuthTokensReceived event handler. 

                oAuthWebAuthCodeGrant.NewOAuthTokensReceived +=
                    (sender, args) => SaveRefreshToken(args.NewRefreshToken);

                // If a refresh token is already present, use it to request new access and refresh tokens.

                if (RefreshTokenExists())
                {
                    // To force the authorization prompt, you can clear a previous refresh token.
                    DeleteRefreshToken();

                    await oAuthWebAuthCodeGrant.RequestAccessAndRefreshTokensAsync(GetRefreshToken());

                    // Save the authentication object in a session for future requests.
                    Session["auth"] = oAuthWebAuthCodeGrant;

                    return await CallBingAdsServices((OAuthWebAuthCodeGrant)Session["auth"]);
                }

                // If the current HTTP request is a callback from the Microsoft Account authorization server,
                // use the current request url containing authorization code to request new access and refresh tokens
                if (Request["code"] != null)
                {
                    if (oAuthWebAuthCodeGrant.State != ClientState)
                        throw new HttpRequestException("The OAuth response state does not match the client request state.");

                    await oAuthWebAuthCodeGrant.RequestAccessAndRefreshTokensAsync(Request.Url);

                    // Save the authentication object in a session for future requests. 
                    Session["auth"] = oAuthWebAuthCodeGrant;

                    return await CallBingAdsServices((OAuthWebAuthCodeGrant)Session["auth"]);
                }

                // If there is no refresh token saved and no callback from the authorization server, 
                // then connect to the authorization server and request user consent. 
                return Redirect(oAuthWebAuthCodeGrant.GetAuthorizationEndpoint().ToString());
            }
            // OAuth classes can throw OAuthTokenRequestException
            catch (OAuthTokenRequestException ex)
            {
                ViewBag.Errors = string.Format("Couldn't get OAuth tokens. \nError: {0}. Description: {1}",
                    ex.Details.Error, ex.Details.Description);

                return View();
            }
            // Bulk service operations can throw AdApiFaultDetail.
            catch (FaultException<Microsoft.BingAds.V11.Bulk.AdApiFaultDetail> ex)
            {
                ViewBag.Errors = string.Format("Error when calling the Bulk service: ");
                ViewBag.Errors += string.Join("; ",
                    ex.Detail.Errors.Select(e => string.Format("{0}: {1}", e.Code, ex.Message)));

                return View();
            }
            // Customer Management service operations can throw AdApiFaultDetail.
            catch (FaultException<Microsoft.BingAds.V11.CustomerManagement.AdApiFaultDetail> ex)
            {
                ViewBag.Errors = string.Format("Error when calling the Customer Management service: ");
                ViewBag.Errors += string.Join("; ",
                    ex.Detail.Errors.Select(e => string.Format("{0}: {1}", e.Code, ex.Message)));

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Errors = ex.Message;

                return View();
            }
        }

        /// <summary>
        /// Adds a campaign to an account of the current authenticated user. 
        /// </summary>
        private async Task<ActionResult> CallBingAdsServices(Authentication authentication)
        {
            // Uses the first account in a list of accounts that the current authenticated user may access. 
            // If you want to use a specific account identifier, you can set _authorizationData to the value directly
            // instead of calling GetUserDataAsync. 

            _authorizationData = await GetUserDataAsync(authentication);
            var campaignId = await AddCampaignInBulkAsync(_authorizationData);

            ViewBag.CustomerId = _authorizationData.CustomerId;
            ViewBag.AccountId = _authorizationData.AccountId;
            ViewBag.CampaignId = campaignId;

            return View();
        }

        /// <summary>
        /// Uses the BulkService class to add a campaign. 
        /// </summary>
        private async Task<long?> AddCampaignInBulkAsync(AuthorizationData authorizationData)
        {
            _bulkService = new BulkServiceManager(authorizationData);
            _bulkService.StatusPollIntervalInMilliseconds = 1000;

            var uploadResults = await _bulkService.UploadEntitiesAsync(new EntityUploadParameters
            {
                Entities = new BulkEntity[]
                {
                    new BulkCampaign
                    {
                        AccountId = authorizationData.AccountId,
                        Campaign = new Campaign
                        {
                            Name = "Campaign " + DateTime.UtcNow.ToString(CultureInfo.InvariantCulture),
                            BudgetType = BudgetLimitType.DailyBudgetAccelerated,
                            DailyBudget = 10,
                            TimeZone = "PacificTimeUSCanadaTijuana"
                        }
                    }
                },
                ResponseMode = ResponseMode.ErrorsAndResults
            });

            var campaign = uploadResults.OfType<BulkCampaign>().Single();

            if (campaign.HasErrors)
            {
                ViewBag.Errors = string.Format("Bulk Upload Error(s): ");
                ViewBag.Errors += string.Join("; ",
                    campaign.Errors.Select(e => string.Format("{0}: {1}", e.Number, e.Error)));
            }

            return campaign.Campaign.Id;
        }

        /// <summary>
        /// Get customer and account identifiers for the current authenticated user. 
        /// </summary>
        private async Task<AuthorizationData> GetUserDataAsync(Authentication authentication)
        {
            // Some service operations only need Authentication and DeveloperToken elements. 
            // Use these credentials at minimum to get customer and account identifiers before managing campaigns. 
            _authorizationData = new AuthorizationData
            {
                Authentication = authentication,
                DeveloperToken = DeveloperToken
            };

            _customerService = new ServiceClient<ICustomerManagementService>(_authorizationData);

            // Get the Bing Ads user identifier for the current authenticated user.
            var user = await GetUserAsync(null);

            var accounts = await SearchAccountsByUserIdAsync(user.Id);

            if (accounts.Length > 0 && accounts[0].Id != null)
            {
                _authorizationData.AccountId = (long)(accounts[0].Id);
                _authorizationData.CustomerId = accounts[0].ParentCustomerId;
            }

            // Store the parent customer identifier in the second array dimension

            _accountCustomerIds = new long?[accounts.Length, 2];

            for (var i = 0; i < accounts.Length; i++)
            {
                _accountCustomerIds[i, 0] = accounts[i].Id;
                _accountCustomerIds[i, 1] = accounts[i].ParentCustomerId;
            }

            SetUserDataByAccountIndex(0);

            return _authorizationData;
        }

        /// <summary>
        /// Gets a User object by the specified Bing Ads user identifier.
        /// </summary>
        /// <param name="userId">The identifier of the user to get. If null, this operation returns the User object 
        /// corresponding to the current authenticated user of the global customer management ServiceClient.</param>
        /// <returns>The User object corresponding to the specified Bing Ads user identifier.</returns>
        private async Task<User> GetUserAsync(long? userId)
        {
            var request = new GetUserRequest
            {
                UserId = userId
            };

            return (await _customerService.CallAsync((s, r) => s.GetUserAsync(r), request)).User;
        }

        /// <summary>
        /// Search for account details by UserId.
        /// </summary>
        /// <param name="userId">The Bing Ads user identifier.</param>
        /// <returns>List of accounts that the user can manage.</returns>
        private async Task<Account[]> SearchAccountsByUserIdAsync(long? userId)
        {
            var predicate = new Predicate
            {
                Field = "UserId",
                Operator = PredicateOperator.Equals,
                Value = userId.ToString()
            };

            var paging = new Microsoft.BingAds.V11.CustomerManagement.Paging
            {
                Index = 0,
                Size = 10
            };

            var request = new SearchAccountsRequest
            {
                Ordering = null,
                PageInfo = paging,
                Predicates = new[] { predicate }
            };

            return (await _customerService.CallAsync((s, r) => s.SearchAccountsAsync(r), request)).Accounts.ToArray();
        }

        /// <summary>
        /// Utility method for setting the customer and account identifiers within the global 
        /// <see cref="_authorizationData"/> instance. 
        /// </summary>
        /// <param name="accountIndex">The index of the account within the <see cref="_accountCustomerIds"/>
        /// multi-dimensional array.</param>
        private void SetUserDataByAccountIndex(int accountIndex)
        {
            if (accountIndex < 0 || accountIndex > _accountCustomerIds.Length) return;

            var accountId = _accountCustomerIds[accountIndex, 0];
            var customerId = _accountCustomerIds[accountIndex, 1];

            if (accountId == null || customerId == null) return;

            _authorizationData.AccountId = (long)accountId;
            _authorizationData.CustomerId = (int)customerId;
        }

        /// <summary>
        /// Saves the refresh token to the global refresh token file path
        /// </summary>
        /// <param name="refreshToken">The refresh token to save.</param>
        private void SaveRefreshToken(string refreshToken)
        {
            System.IO.File.WriteAllText(_refreshTokenFilePath, refreshToken);
        }

        /// <summary>
        /// Deletes the contents in the refresh token file
        /// </summary>
        private void DeleteRefreshToken()
        {
            System.IO.File.Delete(_refreshTokenFilePath);
        }

        /// <summary>
        /// Determines whether the global refresh token exists.
        /// </summary>
        /// <returns>Returns true if the global refresh token file exists.</returns>
        private bool RefreshTokenExists()
        {
            return System.IO.File.Exists(_refreshTokenFilePath);
        }

        /// <summary>
        /// Gets the global refresh token within the global refresh token file.
        /// </summary>
        /// <returns>The global refresh token.</returns>
        private string GetRefreshToken()
        {
            return System.IO.File.ReadAllText(_refreshTokenFilePath);
        }
    }
}