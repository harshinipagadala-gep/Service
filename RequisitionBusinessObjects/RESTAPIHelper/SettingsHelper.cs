using GEP.Cumulus.P2P.Req.BusinessObjects.Entities;
using GEP.SMART.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper
{
    [ExcludeFromCodeCoverage]
    public class SettingsHelper
    {
        private readonly string serviceURL;
        private string appName = Environment.GetEnvironmentVariable("NewRelic.AppName") ?? "Requisition"; 
        private string useCase = "RequisitionCommonManager-SettingsHelper";
        GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.WebAPI webAPI;

        public Gep.Cumulus.CSM.Entities.UserExecutionContext UserContext { get; set; }
        Req.BusinessObjects.RESTAPIHelper.RequestHeaders requestHeaders;

        public SettingsHelper(Gep.Cumulus.CSM.Entities.UserExecutionContext userExecutionContext, string jwtToken)
        {
            serviceURL = string.Concat(MultiRegionConfig.GetConfig(CloudConfig.APIMBaseURL), ServiceURLs.SettingsServiceURL);
            this.UserContext = userExecutionContext;
            requestHeaders = new Req.BusinessObjects.RESTAPIHelper.RequestHeaders();
            requestHeaders.Set(UserContext, jwtToken);
            webAPI = new GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.WebAPI(requestHeaders, appName, useCase);
        }

        public SettingDetails GetFeatureSettings(dynamic getFeatureSettingsRequest)
        {
            SettingDetails settingDetails = null;
            try
            {
                var result = webAPI.ExecutePost(serviceURL + "GetFeatureSettings", getFeatureSettingsRequest);
                var jsonResult = JsonConvert.DeserializeObject<GetFeatureSettingsResponse>(result);
                if (jsonResult != null)
                {
                    settingDetails = Map(jsonResult);
                }
            }
            catch (Exception httpException)
            {
                throw httpException;
            }
            return settingDetails;
        }

        public SettingDetails Map(GetFeatureSettingsResponse input)
        {
            SettingDetails output = new SettingDetails()
            {
                ContactTypeInfo = input.ContactType,
                SettingConfigurationId = input.FeatureConfigurationId,
                SettingDetailsId = input.FeatureSettingId,
                ContactCode = input.ContactCode,
                lstSettings = new List<BasicSettings>()
            };

            foreach (var featureSetting in input.FeatureSettings)
            {
                output.lstSettings.Add(new BasicSettings()
                {
                    DefaultValue = featureSetting.DefaultValue,
                    FieldName = featureSetting.FieldName,
                    FieldValue = featureSetting.FieldValue,
                    OwnerTypeInfo = featureSetting.OwnerTypeInfo
                });
            }

            return output;
        }
    }
}
