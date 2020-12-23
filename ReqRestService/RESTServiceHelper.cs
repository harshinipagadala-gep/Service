//using Gep.Cumulus.CSM.Config;
//using Gep.Cumulus.CSM.Entities;
//using GEP.Cumulus.Web.Utils.Helpers;
//using GEP.SMART.Configuration;
//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Security.Claims;
//using System.ServiceModel;
//using System.ServiceModel.Web;
//using System.Threading;

//namespace GEP.Cumulus.P2P.Req.RestService
//{
//    public class RESTServiceHelper
//    {
//        private string AppName { get; set; }
//        public string JWTToken { get; set; }

//        private UserExecutionContext userExecutionContext = null;
//        private GepConfig gepConfiguration = null;

//        public UserExecutionContext GetExecutionContext()
//        {
//            return this.userExecutionContext;
//        }

//        public RESTServiceHelper()
//        {
//            LogNewRelicAppForJWTTokenTracking("Inside RESTServiceHelper constructor.");
//            var headers = WebOperationContext.Current.IncomingRequest.Headers;

//            if (headers["AppName"] != null)
//            {                
//                this.AppName = headers["AppName"];
//                LogNewRelicAppForJWTTokenTracking("AppName found:" + this.AppName);
//            }

//            if (headers["Authorization"] != null)
//            {
//                LogNewRelicAppForJWTTokenTracking("authorization found:");
//                ReadJWTTokenFromHeader();
//            }

//            // Set UserExecutionContext
//            SetContextFromHeaderInformation();

//            this.gepConfiguration = InitMultiRegion();
//        }

//        private void ReadJWTTokenFromHeader()
//        {
//            LogNewRelicAppForJWTTokenTracking("Inside ReadJWTTokenFromHeader: Start");
//            JWTToken = WebOperationContext.Current.IncomingRequest.Headers["Authorization"];

//            if (string.IsNullOrEmpty(JWTToken))
//            {
//                LogNewRelicAppForJWTTokenTracking("Inside ReadJWTTokenFromHeader: JWT token is null or empty.");
//            }
//            else
//            {
//                LogNewRelicAppForJWTTokenTracking("Inside ReadJWTTokenFromHeader: JWT token is not empty");
//                try
//                {
//                    // Check the token string is valid
//                    var tokenForValidityCheck = JWTToken.Replace("Bearer ", string.Empty);
//                    var claimsPrincipal = GEP.SMART.Security.ClaimsManagerNet.JwtTokenHelper.ValidateJwtToken(tokenForValidityCheck);
//                    Thread.CurrentPrincipal = claimsPrincipal;

//                    LogNewRelicAppForJWTTokenTracking("Inside ReadJWTTokenFromHeader: JWT token is set with claims and principle.");
//                }
//                catch (Exception ex)
//                {
//                    LogNewRelicAppForJWTTokenTracking("Inside ReadJWTTokenFromHeader: Token is there but validate fails with exception : " + ex.Message, JWTToken);
//                }
//            }
//        }

//        private UserExecutionContext GetUserExecutionContext()
//        {
//            LogNewRelicAppForJWTTokenTracking("Inside GetUserExecutionContext: Start");

//            var userExecutionContext = new UserExecutionContext();
//            if (WebOperationContext.Current.IncomingRequest != null)
//            {
//                string jsonData = WebOperationContext.Current.IncomingRequest.Headers["UserExecutionContext"].ToString();
//                LogNewRelicAppForJWTTokenTracking("Inside GetUserExecutionContext: UserExecutionContext header value read with length :" + jsonData.Length.ToString());
//                if (WebOperationContext.Current.IncomingRequest.UserAgent != null && WebOperationContext.Current.IncomingRequest.UserAgent.Contains("Titanium"))
//                {
//                    jsonData = SecurityUtils.AESdecrypt(jsonData);
//                }

//                using (StringReader sr = new StringReader(jsonData))
//                {
//                    userExecutionContext = JsonSerializer.Create(new JsonSerializerSettings()).Deserialize(sr, typeof(UserExecutionContext)) as UserExecutionContext;
//                }
//            }
//            return userExecutionContext;

//        }

//        private void SetContextFromHeaderInformation()
//        {
//            LogNewRelicAppForJWTTokenTracking("Inside SetContextFromHeaderInformation: Start");
//            this.userExecutionContext = GetUserExecutionContext();

//            if (Thread.CurrentPrincipal == null)
//            {
//                LogNewRelicAppForJWTTokenTracking("Inside SetContextFromHeaderInformation: Thread.CurrentPrincipal == null");

//                // Set current thread with default principal object
//                var claims = new List<Claim>
//                    {
//                        new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", userExecutionContext.UserName),
//                        new Claim("http://www.gep.com/c/15",Convert.ToString(userExecutionContext.BuyerPartnerCode))
//                    };

//                var appIdentity = new ClaimsIdentity(claims);
//                Thread.CurrentPrincipal = new ClaimsPrincipal(appIdentity);

//                LogNewRelicAppForJWTTokenTracking("Inside SetContextFromHeaderInformation: Thread.CurrentPrincipal is set at LN 122");
//            }
//        }

//        protected void LogNewRelicAppForJWTTokenTracking(string message, string token = "")
//        {
//            var eventAttributes = new Dictionary<string, object>();            
//            eventAttributes.Add("message", message);
//            eventAttributes.Add("AppName", AppName);
//            eventAttributes.Add("JWTToken", token);
//            NewRelic.Api.Agent.NewRelic.RecordCustomEvent("RequisitionRestService", eventAttributes);
//        }

//        private GepConfig InitMultiRegion()
//        {
//            GepConfig _config = new GepConfig();
//            var SmartConfigConn = MultiRegionConfig.GetConfig(CloudConfig.ConfigSqlConn);
//            AddValueToConfig(_config, GepConfig.ConfigSqlConn, SmartConfigConn);
//            return _config;
//        }
//        private void AddValueToConfig(GepConfig _config, string key, string value)
//        {
//            if (!_config.ContainsKey(key))
//                _config.Add(key, value);
//            else
//                _config[key] = value ?? string.Empty;
//        }

//        public void SetTokenInHeader()
//        {
//            try
//            {
//                LogNewRelicAppForJWTTokenTracking("Inside SetTokenInHeader: LN 154");

//                var messageHeader = System.ServiceModel.Channels.MessageHeader.CreateHeader("Authorization", "Gep.Cumulus", this.JWTToken);
//                System.ServiceModel.OperationContext.Current.OutgoingMessageHeaders.Add(messageHeader);
                
//                LogNewRelicAppForJWTTokenTracking("Inside SetTokenInHeader: Set token into header (OperationContext.Current.OutgoingMessageHeaders): LN159");

//                var messageHeaderAppName = System.ServiceModel.Channels.MessageHeader.CreateHeader("AppName", "Gep.Cumulus", "RequisitionRestService");
//                System.ServiceModel.OperationContext.Current.OutgoingMessageHeaders.Add(messageHeaderAppName);

//                LogNewRelicAppForJWTTokenTracking("Inside SetTokenInHeader: Set AppName into header (LN164): " + this.AppName);                
//            }
//            catch (Exception ex)
//            {
//                LogNewRelicAppForJWTTokenTracking("Inside SetTokenInHeader: Error in SetTokenInHeader (LN176): " + ex.Message);
//            }
//        }
//    }
//}
