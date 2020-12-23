using Gep.Cumulus.Partner.Entities;
using GEP.SMART.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper
{
    [ExcludeFromCodeCoverage]
    public class PartnerHelper
    {
        private readonly string serviceURL;
        private string appName = Environment.GetEnvironmentVariable("NewRelic.AppName") ?? "Requisition"; 
        private string useCase = "NewRequisitionManager-PartnerHelper";
        GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.WebAPI webAPI;

        public Gep.Cumulus.CSM.Entities.UserExecutionContext UserContext { get; set; }
        Req.BusinessObjects.RESTAPIHelper.RequestHeaders requestHeaders;

        public PartnerHelper(Gep.Cumulus.CSM.Entities.UserExecutionContext userExecutionContext, string jwtToken)
        {
            serviceURL = string.Concat(MultiRegionConfig.GetConfig(CloudConfig.APIMBaseURL), ServiceURLs.UserManagementServiceURL);
            this.UserContext = userExecutionContext;
            requestHeaders = new Req.BusinessObjects.RESTAPIHelper.RequestHeaders();
            requestHeaders.Set(UserContext, jwtToken);
            webAPI = new GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.WebAPI(requestHeaders, appName, useCase);
        }

        public string GetContactPreferenceByContactCode(long buyerPartnerCode, long ContactCode)
        {
            string response = string.Empty;           
            try
            {
                var url = MultiRegionConfig.GetConfig(CloudConfig.APIMBaseURL)
                    + "/platformpartner/api/v1/Partner/GetContactPreferenceByContactCode?"; 
                response = webAPI.ExecuteGet(url + "ContactCode=" + ContactCode + "&PartnerCode=" + buyerPartnerCode);                
            }
            catch (Exception httpException)
            {
                throw httpException;
            }
            return response;
        }

        public string GetUserActivitiesByContactCode(long ContactCode, long PartnerCode)
        {
            string response = string.Empty;
            var GetBuyerUserActivitiesByContactCodeRequest = new Dictionary<string, long>();
            GetBuyerUserActivitiesByContactCodeRequest.Add("contactCode", ContactCode);
            GetBuyerUserActivitiesByContactCodeRequest.Add("partnerCode", PartnerCode);

            try
            {
                var result = webAPI.ExecutePost(serviceURL + "GetBuyerUserActivitiesByContactCode", GetBuyerUserActivitiesByContactCodeRequest);
                response = JsonConvert.DeserializeObject<string>(result);
            }
            catch (Exception httpException)
            {
                throw httpException;
            }
            return response;
        }

        public List<User> GetUserDetailsByContactCodes(string contactCodes, bool bChkActiveFlag)
        {
            List<User> lstUsers = new List<User>();

            var GetUserDetailsByContactCodesRequest = new Dictionary<string, object>();
            GetUserDetailsByContactCodesRequest.Add("contactCodes", contactCodes.Split(',').ToList());
            GetUserDetailsByContactCodesRequest.Add("activeOnly", bChkActiveFlag);

            try
            {
                var result = webAPI.ExecutePost(serviceURL + "GetUserDetailsByContactCodes", GetUserDetailsByContactCodesRequest);

                var apiresponse = JsonConvert.DeserializeObject<Dictionary<string, List<User>>>(result);
                if (apiresponse != null && apiresponse.Any())
                {
                    lstUsers = apiresponse.First().Value;
                }
            }
            catch (Exception httpException)
            {
                throw httpException;
            }
            return lstUsers;
        }

        public UserContext GetUserContextDetailsByContactCode(long contactCode)
        {
            UserContext context = null;
            try
            {
                var result = webAPI.ExecutePost(serviceURL + "GetUserDetailsWithLOBMappingByContactCode", contactCode);
                var getUserDetailsWithLOBMappingByContactCodeResponse = JsonConvert.DeserializeObject<GetUserDetailsWithLOBMappingByContactCodeResponse>(result);
                context = Map(getUserDetailsWithLOBMappingByContactCodeResponse);
            }
            catch (Exception httpException)
            {
                throw httpException;
            }
            return context;
        }

        public UserContext Map(GetUserDetailsWithLOBMappingByContactCodeResponse input)
        {
            UserContext output = new UserContext();
            if (input != null)
            {
                output = new UserContext()
                {
                    UserId = input.UserDetailWithUserPreferences.UserId,
                    UserName = input.UserDetailWithUserPreferences.UserName,
                    CultureCode = input.UserDetailWithUserPreferences.CultureCode,
                    FirstName = input.UserDetailWithUserPreferences.FirstName,
                    LastName = input.UserDetailWithUserPreferences.LastName,
                    EmailAddress = input.UserDetailWithUserPreferences.EmailAddress,
                    ContactCode = input.UserDetailWithUserPreferences.ContactCode,
                    TimeZone = input.UserDetailWithUserPreferences.TimeZone,
                    ShipToLocationId = input.UserDetailWithUserPreferences.ShipToLocationId,
                    TypeOfUser = (User.UserType)input.UserDetailWithUserPreferences.UserType,
                    ChangePassword = input.UserDetailWithUserPreferences.ChangePassword,
                    HideWelComeDialogBox = input.UserDetailWithUserPreferences.HideWelComeDialogBox,
                    DefaultCurrencyCode = input.UserDetailWithUserPreferences.DefaultCurrencyCode,
                    Partners = new List<PartnerUserContext>(),
                    UserLOBMapping = new List<Gep.Cumulus.Partner.Entities.UserLOBMapping>()
                };

                if (input.PartnerCodes != null)
                {
                    foreach (var code in input.PartnerCodes)
                    {
                        output.Partners.Add(new PartnerUserContext() { PartnerCode = code });
                    }
                }

                if (input.UserLOBMapping != null)
                {
                    foreach (var a in input.UserLOBMapping)
                    {
                        output.UserLOBMapping.Add(new Gep.Cumulus.Partner.Entities.UserLOBMapping()
                        {
                            ContactLOBMappingId = a.ContactLOBMappingId,
                            EntityCode = a.EntityCode,
                            EntityDetailCode = a.EntityDetailCode,
                            EntityDisplayName = a.EntityDisplayName,
                            EntityId = a.EntityId,
                            IsDefault = a.IsDefault,
                            PreferenceLobType = a.PreferenceLobType
                        });
                    }
                }
            }
            return output;
        }


    }

    #region Partner API Entities

    public class GetUserDetailsWithLOBMappingByContactCodeResponse
    {
        public UserDetailWithUserPreferences UserDetailWithUserPreferences { get; set; }
        public List<long> PartnerCodes { get; set; }
        public List<UserLOBMapping> UserLOBMapping { get; set; }
    }

    public class UserLOBMapping
    {
        public long ContactLOBMappingId { get; set; }
        public long EntityDetailCode { get; set; }
        public string EntityCode { get; set; }
        public string EntityDisplayName { get; set; }
        public int EntityId { get; set; }
        public PreferenceLOBType PreferenceLobType { get; set; }
        public bool IsDefault { get; set; }
    }

    public class UserDetailWithUserPreferences
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string CultureCode { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public long ContactCode { get; set; }
        public string TimeZone { get; set; }
        public int ShipToLocationId { get; set; }
        public int UserType { get; set; }
        public bool ChangePassword { get; set; }
        public bool HideWelComeDialogBox { get; set; }
        public string DefaultCurrencyCode { get; set; }
        public int ActiveSessionDuration { get; set; }
        public string UserTheme { get; set; }
    }

    #endregion
}
