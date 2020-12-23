using Gep.Cumulus.CSM.Entities;
using GEP.Cumulus.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.ServiceModel;
using System.Threading;
using System.Diagnostics;
using System.Security.Claims;
using System.Diagnostics.CodeAnalysis;

namespace GEP.Cumulus.P2P.Req.Service
{
    [ExcludeFromCodeCoverage]
    public class RequisitionServiceHelper
    {
        private static readonly log4net.ILog Log = Logger.GetLog(MethodBase.GetCurrentMethod().DeclaringType);

        private string JWTToken { get; set; }
        private string AppName { get; set; }

        private UserExecutionContext UserExecutionContext { get; set; }

        public string GetToken()
        {
            return this.JWTToken;
        }

        public string GetAppName()
        {
            return this.AppName;
        }

        public RequisitionServiceHelper(UserExecutionContext userExecutionContext)
        {
            this.UserExecutionContext = userExecutionContext;

            GetJwtTokenFromRequestHeader();
        }

        private void GetJwtTokenFromRequestHeader()
        {
            try
            {
                if (OperationContext.Current.IncomingMessageHeaders.FindHeader("AppName", "Gep.Cumulus") > 0)
                {
                    this.AppName = OperationContext.Current.IncomingMessageHeaders.GetHeader<string>("AppName", "Gep.Cumulus");                    
                }
                else
                {
                    this.AppName = "NA";
                }

                if (OperationContext.Current.IncomingMessageHeaders.FindHeader("Authorization", "Gep.Cumulus") > 0)
                {
                    this.JWTToken = OperationContext.Current.IncomingMessageHeaders.GetHeader<string>("Authorization", "Gep.Cumulus");
                    bool bClaimsSet = false;
                    
                    try
                    {
                        var tokenForValidityCheck = JWTToken.Replace("Bearer ", string.Empty);
                        var claimsPrincipal = GEP.SMART.Security.ClaimsManagerNet.JwtTokenHelper.ValidateJwtToken(tokenForValidityCheck);
                        Thread.CurrentPrincipal = claimsPrincipal;
                        bClaimsSet = true;
                    }
                    catch (Exception ex)
                    {
                        LogNewRelicAppForJWTTokenTracking("Exception :" + ex.Message);
                    }

                    if (!bClaimsSet)
                    {
                        var claims = new List<Claim>
                        {
                            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", this.UserExecutionContext.UserName),
                            new Claim("http://www.gep.com/c/15",Convert.ToString(this.UserExecutionContext.BuyerPartnerCode))
                        };

                        var appIdentity = new ClaimsIdentity(claims);
                        Thread.CurrentPrincipal = new ClaimsPrincipal(appIdentity);
                    }
                }
                else
                {
                    LogNewRelicAppForJWTTokenTracking("JWT token missing in header.");
                }                
            }
            catch (Exception ex)
            {
                LogNewRelicAppForJWTTokenTracking("Exception:" + ex.Message);
            }
        }

        private void LogNewRelicAppForJWTTokenTracking(string message)
        {
            var eventAttributes = new Dictionary<string, object>();            
            eventAttributes.Add("message", message);
            eventAttributes.Add("AppName", AppName);
            NewRelic.Api.Agent.NewRelic.RecordCustomEvent("RequisitionWCFServiceHelper", eventAttributes);
        }

    }
}
