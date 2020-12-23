using GEP.Cumulus.Logging;
using GEP.SMART.Configuration;
using GEP.SMART.HttpCommunicationLayer;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
namespace GEP.Cumulus.P2P.Req.RestService
{
    [ExcludeFromCodeCoverage]
    public class WebAPIHelper
    {        
        private APIRequestHeaders requestHeaders;

        private Dictionary<string, string> headers;

        private string AppName { get; set; }

        private string UseCase { get; set; }

        private string TransactionId { get; set; }

        private static readonly ILog Log = Logger.GetLog(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Call a RESTful WebAPI using this helper class
        /// </summary>
        /// <param name="requestHeaders"> Provide values for Context, JWTtoken</param>
        /// <param name="appName">This field is supposed to be the New Relic APM App Name. This will be present in config file with key=NewRelic.AppName. Please DO NOT hard code the value because this is suppose to uniquely identify the environment and the region from which the call is being made.</param>
        /// <param name="useCase">This field is a string identifier for the functional area from where this API is being called and is compulsory. Recommended syntax is ProductName-FunctionalArea. Ex: Supplier-LandingPage.</param>
        public WebAPIHelper(APIRequestHeaders requestHeaders, string appName, string useCase)
        {
            this.AppName = appName;
            this.UseCase = useCase;
        
            this.requestHeaders = requestHeaders;
            headers = new Dictionary<string, string>
                                {
                                    { "UserExecutionContext", requestHeaders.ContextJSON },
                                    { "Ocp-Apim-Subscription-Key", MultiRegionConfig.GetConfig(CloudConfig.APIMSubscriptionKey) },
                                    { "BPC", requestHeaders.Context.BuyerPartnerCode.ToString() },
                                    { "RegionID", MultiRegionConfig.GetConfig(CloudConfig.PrimaryRegion) },
                                    { "Authorization", requestHeaders.JWTtoken },
                                    { "GEPSmartTransactionId", Guid.NewGuid().ToString() },
                                    { "TransactionId", Guid.NewGuid().ToString() },
                                    { "AppName", appName },
                                    { "UseCase", useCase }
            };
        }       

        /// <summary>
        /// Call RESTful service with a GET verb
        /// </summary>
        /// <param name="path">Complete URL for calling the RESTful service API</param>
        /// <param name="timeOut">Optional parameter for service request timeout, default value is 50 Seconds</param>
        /// <returns>string response value</returns>
        public string ExecuteGet(string path, int timeOut = 50000)
        {
            HttpResponseMessage responseMessage = null;
            string response = string.Empty;

            try
            {
                LogNewRelicAppMessage(path, "Before Get call");

                SmartHttpClient httpClient = new SmartHttpClient();
                httpClient.Headers = headers;

                var result = httpClient.GetAsync(path, "RequisitionCoreAPI_ExecuteGetAsync", true).Result;

                if (result.IsSuccessStatusCode)
                {
                    response = result.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    throw new HttpRequestException(result.StatusCode.ToString(), new Exception(result.Content.ReadAsStringAsync().Result));
                }

                LogNewRelicAppMessage(path, "After Get call");
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in ExecuteGet for URL:" + path + "  and exception: " + ex.Message, ex);
                LogNewRelicAppForException(ex, path, requestHeaders.Context.BuyerPartnerCode);
                throw;
            }
            finally
            {
                if (responseMessage != null) responseMessage.Dispose();
            }

            return response;
        }

        /// <summary>
        /// Call RESTful service with a POST verb
        /// </summary>
        /// <param name="path">Complete URL for calling the RESTful service API</param>
        /// <param name="body">Dictionary<string, object> to be passed or object</param>
        /// <param name="timeOut">Optional parameter for service request timeout, default value is 50 Seconds</param>
        /// <returns>string response value</returns>
        public string ExecutePost(string path, object body, int timeOut = 50000)
        {
            HttpResponseMessage responseMessage = null;

            string response = string.Empty;
            var content = JsonConvert.SerializeObject(body);
            try
            {
                LogNewRelicAppMessage(path, "Before Post call");
                SmartHttpClient httpClient = new SmartHttpClient();
                httpClient.Headers = headers;

                var result = httpClient.PostAsync<object>(path, body, "RequisitionCoreAPI_ExecutePostAsnyc", true).Result;

                if (result.IsSuccessStatusCode)
                {
                    response = result.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    throw new HttpRequestException(result.StatusCode.ToString(), new Exception(result.Content.ReadAsStringAsync().Result));
                }

                LogNewRelicAppMessage(path, "After Post call");
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in ExecutePost for URL:" + path + "  and exception: " + ex.Message, ex);
                LogNewRelicAppForException(ex, path, requestHeaders.Context.BuyerPartnerCode);
                throw;
            }
            finally
            {
                if (responseMessage != null) responseMessage.Dispose();
            }

            return response;
        }

        private void LogNewRelicAppMessage(string url, string message)
        {
            var eventAttributes = new Dictionary<string, object>();
            eventAttributes.Add("URL", url);           
            eventAttributes.Add("message", message);
            NewRelic.Api.Agent.NewRelic.RecordCustomEvent("ReqRESTService_WebAPIHelper_Msg", eventAttributes);
        }       

        private void LogNewRelicAppForException(Exception ex, string url, long buyerPartnerCode)
        {
            var eventAttributes = new Dictionary<string, object>();
            eventAttributes.Add("Exception", ex.Message);
            eventAttributes.Add("StackTrace", ex.StackTrace);            
            eventAttributes.Add("URL", url);
            string innerException = string.Empty;
            if (ex.InnerException != null)
            {
                innerException = ex.InnerException.Message;
            }
            eventAttributes.Add("innerException", innerException);
            eventAttributes.Add("BuyerPartnerCode", buyerPartnerCode.ToString());            
            NewRelic.Api.Agent.NewRelic.RecordCustomEvent("ReqRESTService_WebAPI_Exception", eventAttributes);
        }
    }
}
