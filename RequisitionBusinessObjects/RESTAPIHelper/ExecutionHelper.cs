using Gep.Cumulus.CSM.Config;
using Gep.Cumulus.CSM.Entities;
using GEP.Cumulus.Caching;
using GEP.Cumulus.P2P.BusinessEntities;
using GEP.Cumulus.P2P.BusinessObjects;
using GEP.Cumulus.P2P.Req.BusinessObjects.Entities;
using GEP.NewP2PEntities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper
{
    [ExcludeFromCodeCoverage]
    public class ExecutionHelper
    {
        string JWTToken { get; set; }
        UserExecutionContext userContext { get; set; }

        int[] CommonWebAPICall;
        int[] PartnerWebAPICall;
        int[] OrderWebAPICall;

        public enum WebAPIType
        {
            Common,
            Partner,
            Order
        }

        public ExecutionHelper(UserExecutionContext UserContext, GepConfig GepConfiguration, string jwtToken)
        {
            this.JWTToken = jwtToken;
            this.userContext = UserContext;

            RequisitionCommonManager commonManager = new RequisitionCommonManager(this.JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };

            var RequisitionSettings = GEPDataCache.GetFromCacheJSON<SettingDetails>("RequisitionDocumentSettings", UserContext.BuyerPartnerCode, "en-US");

            if (RequisitionSettings == null)
            {
                RequisitionSettings = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.Requisition, UserContext.ContactCode, (int)SubAppCodes.P2P);
                GEPDataCache.PutInCacheJSON<SettingDetails>("RequisitionDocumentSettings", UserContext.BuyerPartnerCode, "en-US", RequisitionSettings);
            }


            string commonWebAPICall = commonManager.GetSettingsValueByKey(RequisitionSettings, "CommonWebAPICall");
            if (!string.IsNullOrEmpty(commonWebAPICall))
            {
                CommonWebAPICall = commonWebAPICall.Split(',').Select(n => Convert.ToInt32(n)).ToArray();
            }

            string partnerWebAPICall = commonManager.GetSettingsValueByKey(RequisitionSettings, "PartnerWebAPICall");
            if (!string.IsNullOrEmpty(partnerWebAPICall))
            {
                PartnerWebAPICall = partnerWebAPICall.Split(',').Select(n => Convert.ToInt32(n)).ToArray();
            }
            string orderWebAPICall = commonManager.GetSettingsValueByKey(RequisitionSettings, "OrderWebAPICall");
            if (!string.IsNullOrEmpty(orderWebAPICall))
            {
                OrderWebAPICall = orderWebAPICall.Split(',').Select(n => Convert.ToInt32(n)).ToArray();
            }
        }

        public bool Check(int value, WebAPIType webAPIType)
        {
            bool result = false;
            switch (webAPIType)
            {
                case WebAPIType.Common:
                    result = CommonWebAPICall != null ? CommonWebAPICall.Contains(value) : false;
                    break;
                case WebAPIType.Partner:
                    result = PartnerWebAPICall != null ? PartnerWebAPICall.Contains(value) : false;
                    break;
                case WebAPIType.Order:
                    result = OrderWebAPICall != null ? OrderWebAPICall.Contains(value) : false;
                    break;
            }

            if (result) logNewRelicApp(value, webAPIType.ToString());

            return result;
        }

        private void logNewRelicApp(int key, string WebAPIType)
        {
            var eventAttributes = new Dictionary<string, object>();
            eventAttributes.Add("key", key.ToString());
            eventAttributes.Add("buyerPartnerCode", userContext.BuyerPartnerCode);
            eventAttributes.Add("WebAPIType", WebAPIType);
            NewRelic.Api.Agent.NewRelic.RecordCustomEvent("RequisitionService_PlatformAPICalls", eventAttributes);
        }
    }
}
