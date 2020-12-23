using Gep.Cumulus.CSM.Entities;
using GEP.Cumulus.Logging;
using GEP.Cumulus.OrganizationStructure.Entities;
using Entities = GEP.Cumulus.P2P.Req.BusinessObjects.Entities;
using GEP.SMART.Configuration;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper
{
    [ExcludeFromCodeCoverage]
    public class CSMHelper
    {
        private UserExecutionContext UserExecutionContext = null;
        private string JWTToken = string.Empty;
        private string APIMBaseURL = MultiRegionConfig.GetConfig(CloudConfig.APIMBaseURL);
        private string appName = Environment.GetEnvironmentVariable("NewRelic.AppName") ?? "Requisition"; 
        private static readonly ILog Log = Logger.GetLog(MethodBase.GetCurrentMethod().DeclaringType);
        Req.BusinessObjects.RESTAPIHelper.RequestHeaders requestHeaders;

        GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.WebAPI webAPI;

        public CSMHelper(UserExecutionContext UserExecutionContext, string JWTToken)
        {
            this.UserExecutionContext = UserExecutionContext;
            this.JWTToken = JWTToken;
            requestHeaders = new Req.BusinessObjects.RESTAPIHelper.RequestHeaders();
            requestHeaders.Set(this.UserExecutionContext, this.JWTToken);
        }

        public List<KeyValue> GetAllCurrency()
        {
            try
            {
                var lstKeyValue = new List<KeyValue>();
                var objCurrency = new Entities.CurrencyAutoSuggestFilterModel("", 1000);
                objCurrency.CultureCode = UserExecutionContext.Culture;
                string GetCurrencyAutoSuggestUrl = APIMBaseURL + URLs.GetCurrencyAutoSuggest;
               
                webAPI = new RESTAPIHelper.WebAPI(requestHeaders, appName, "CSMHelper-GetAllCurrency");
                var JsonResult = webAPI.ExecutePost(GetCurrencyAutoSuggestUrl, objCurrency);

                List<Entities.CurrencyAutoSuggestDataModel> lstcurrencyAutoSuggestDataModels = JsonConvert.DeserializeObject<List<Entities.CurrencyAutoSuggestDataModel>>(JsonResult);
                lstKeyValue = (from Entities.CurrencyAutoSuggestDataModel objCurrencyAutoSuggestFilterModel in lstcurrencyAutoSuggestDataModels
                               select new KeyValue
                               {
                                   Id = objCurrencyAutoSuggestFilterModel.CurrencyId,
                                   Name = objCurrencyAutoSuggestFilterModel.CurrencyName,
                                   Value = objCurrencyAutoSuggestFilterModel.CurrencyCode,
                                   IsDefault = objCurrencyAutoSuggestFilterModel.IsDefault
                               }).OrderByDescending(x => x.IsDefault).ToList();

                return lstKeyValue;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in GetAllCurrency Method in CSMWebAPI", ex);
                throw ex;
            }
        }

        public ICollection<Countries> SearchCountries(dynamic objCountry)
        {
            List<Countries> countries = null;
            try
            {
                var url = APIMBaseURL + ServiceURLs.CommonReferenceServiceURL + "SearchCountriesByName";
                webAPI = new RESTAPIHelper.WebAPI(requestHeaders, appName, "CSMHelper-SearchCountries");
                var response = webAPI.ExecutePost(url, objCountry);
                if (!string.IsNullOrEmpty(response))
                {
                    var jsonResult = JsonConvert.DeserializeObject<dynamic>(response);
                    if (jsonResult.Countries != null)
                    {
                        string jsonObj = JsonConvert.SerializeObject(jsonResult.Countries);
                        countries = JsonConvert.DeserializeObject<List<Countries>>(jsonObj);
                    }
                }
            }
            catch (Exception httpException)
            {
                throw httpException;
            }
            return countries;
        }

        public ICollection<States> SearchStates(dynamic objState)
        {
            List<States> states = null;
            try
            {
                var url = APIMBaseURL + ServiceURLs.CommonReferenceServiceURL + "SearchStatesByName";
                webAPI = new RESTAPIHelper.WebAPI(requestHeaders, appName, "CSMHelper-SearchStates");
                var response = webAPI.ExecutePost(url, objState);
                if (!string.IsNullOrEmpty(response))
                {
                    var jsonResult = JsonConvert.DeserializeObject<dynamic>(response);
                    if (jsonResult.States != null)
                    {
                        string jsonObj = JsonConvert.SerializeObject(jsonResult.States);
                        states = JsonConvert.DeserializeObject<List<States>>(jsonObj);
                    }
                }
            }
            catch (Exception httpException)
            {
                throw httpException;
            }
            return states;
        }

        public States SaveState(dynamic SaveStateRequest)
        {
            States objStateInfo = null;
            try
            {
                var url = APIMBaseURL + ServiceURLs.CommonReferenceServiceURL + "SaveState";
                webAPI = new RESTAPIHelper.WebAPI(requestHeaders, appName, "CSMHelper-SaveState");
                var response = webAPI.ExecutePost(url, SaveStateRequest);
                if (!string.IsNullOrEmpty(response))
                {
                    objStateInfo = JsonConvert.DeserializeObject<States>(response);
                }
            }
            catch (Exception httpException)
            {
                throw httpException;
            }
            return objStateInfo;
        }

        public long GenerateNewEntityCode(dynamic GenerateNewEntityCodeRequest)
        {
            long result = 0;
            try
            {
                var url = APIMBaseURL + ServiceURLs.MotleyServiceURL + "GenerateNewEntityCode";
                webAPI = new RESTAPIHelper.WebAPI(requestHeaders, appName, "CSMHelper-GenerateNewEntityCode");
                var response = webAPI.ExecutePost(url, GenerateNewEntityCodeRequest);
                if (!string.IsNullOrEmpty(response))
                {
                    result = JsonConvert.DeserializeObject<long>(response);
                }
            }
            catch (Exception httpException)
            {
                throw httpException;
            }
            return result;
        }

        public List<OrgSearchResult> GetSearchedEntityDetails(dynamic objSearch)
        {
            List<OrgSearchResult> result = new List<OrgSearchResult>();
            var serviceURL = MultiRegionConfig.GetConfig(CloudConfig.APIMBaseURL) + ServiceURLs.OrganizationServiceURL + "GetSearchedEntityDetails";

            webAPI = new WebAPI(requestHeaders, appName, "GetSearchedEntityDetails");
            var response = webAPI.ExecutePost(serviceURL, objSearch);
            if (!string.IsNullOrEmpty(response))
            {
                var jsonResult = JsonConvert.DeserializeObject<dynamic>(response);
                if (jsonResult.SearchedEntityDetailsResponse != null)
                {
                    string jsonObj = JsonConvert.SerializeObject(jsonResult.SearchedEntityDetailsResponse);
                    var getCategoriesByCategoryCodesResponse = JsonConvert.DeserializeObject<List<SearchedEntityDetailsResponse>>(jsonObj);
                    result = mapOrgSearchResult(getCategoriesByCategoryCodesResponse);
                }
            }

            return result;
        }

        private List<OrgSearchResult> mapOrgSearchResult(List<SearchedEntityDetailsResponse> searchedEntityDetailsResponses)
        {
            List<OrgSearchResult> result = new List<OrgSearchResult>();

            foreach(var searchedEntity in searchedEntityDetailsResponses)
            {
                result.Add(new OrgSearchResult()
                {
                    OrgEntityCode = searchedEntity.OrgEntityCode,
                    EntityId = searchedEntity.EntityId,
                    DisplayName = searchedEntity.DisplayName,
                    EntityCode = searchedEntity.EntityCode,
                    Description = searchedEntity.EntityDescription,
                    IsActive = searchedEntity.IsActive,
                    ParentOrgEntityCode = searchedEntity.ParentOrgEntityCode
                });
            }
            return result;
        }

        public List<Entities.PASMaster> GetCategoriesByCategoryCodes(dynamic getCategoriesByCategoryCodesRequest)
        {
            List<Entities.PASMaster> pasList = null;
            try
            {
                var url = APIMBaseURL + ServiceURLs.CategoryServiceURL + "GetCategoriesByCategoryCodes";
                webAPI = new WebAPI(requestHeaders, appName, "CSMHelper-GetCategoriesByCategoryCodes");
                var response = webAPI.ExecutePost(url, getCategoriesByCategoryCodesRequest);
                if (!string.IsNullOrEmpty(response))
                {
                    var jsonResult = JsonConvert.DeserializeObject<dynamic>(response);
                    if (jsonResult.Categories != null)
                    {
                        string jsonObj = JsonConvert.SerializeObject(jsonResult.Categories);
                        var getCategoriesByCategoryCodesResponse = JsonConvert.DeserializeObject<List<GetCategoriesByCategoryCodesResponse>>(jsonObj);
                        pasList = mapPASCategory(getCategoriesByCategoryCodesResponse);
                    }
                }
            }
            catch (Exception httpException)
            {
                throw httpException;
            }
            return pasList;
        }

        private List<Entities.PASMaster> mapPASCategory(List<GetCategoriesByCategoryCodesResponse> getCategoriesByCategoryCodesResponses)
        {
            var pASMasters = new List<Entities.PASMaster>();

            if (getCategoriesByCategoryCodesResponses != null && getCategoriesByCategoryCodesResponses.Count > 0)
            {
                foreach(var getCategoriesByCategoryCodesResponse in getCategoriesByCategoryCodesResponses)
                {
                    pASMasters.Add(new Entities.PASMaster()
                    {
                        PASCode = getCategoriesByCategoryCodesResponse.CategoryCode,
                        PASName = getCategoriesByCategoryCodesResponse.CategoryName
                    });
                }
            }
            return pASMasters;
        }
    }

    public class GetCategoriesByCategoryCodesResponse
    {
        public long CategoryCode { get; set; }
        public string CategoryName { get; set; }
        public int CategoryLevel { get; set; }
        public long ParentCategoryCode { get; set; }
    }

    public class SearchedEntityDetailsResponse
    {
        public int OrgEntityCode { get; set; }
        public int EntityId { get; set; }
        public string DisplayName { get; set; }
        public string EntityCode { get; set; }
        public string EntityDescription { get; set; }
        public bool IsActive { get; set; }
        public int ParentOrgEntityCode { get; set; }
    }


}
