using Gep.Cumulus.CSM.Entities;
using GEP.SMART.Security.ClaimsManagerNet;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper
{
    [ExcludeFromCodeCoverage]
    public class RequestHeaders
    {
        public bool GenericTokenGenerated { get; set; }
        public bool UnAuthorizedCall { get; set; }
        public string ContextJSON
        {
            get
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(this.Context);
            }
        }

        public UserExecutionContext Context { get; private set; }

        public string JWTtoken { get; private set; }

        public void Set(UserExecutionContext context, string token)
        {
            GenericTokenGenerated = false;
            UnAuthorizedCall = false;
            Context = context;

            if (string.IsNullOrEmpty(token))
            {
                try
                {                    
                    GenericTokenGenerated = true;

                    if (SmartClaimsManager.IsAuthenticated())
                    {                        
                        token = "Bearer " + GEP.SMART.Security.ClaimsManagerNet.JwtTokenHelper.CreateJwtTokenFromClaimsIdentity();
                    }
                    else
                    {
                        UnAuthorizedCall = true;
                    }                 
                }
                catch (Exception ex)
                {
                    logNewRelicApp("Error: " + ex.Message + "  StackTrace:" + ex.StackTrace);
                    logNewRelicApp("Error: UserExecutionContext = " + ContextJSON);
                }

            }

            if (!token.Contains("Bearer"))
            {
                token = "Bearer " + token;
            }

            JWTtoken = token;
        }

        private void logNewRelicApp(string message)
        {
            var eventAttributes = new Dictionary<string, object>();
            eventAttributes.Add("BuyerPartnerCode", Context.BuyerPartnerCode);
            eventAttributes.Add("message", message);
            NewRelic.Api.Agent.NewRelic.RecordCustomEvent("RequisitionService_RequestHeaders", eventAttributes);
        }
    }
}
