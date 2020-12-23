using Gep.Cumulus.CSM.Entities;
using GEP.Cumulus.Web.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using GEP.Cumulus.Logging;
using System.Reflection;
using log4net;
using System.ServiceModel.Channels;
using System.Diagnostics.CodeAnalysis;

namespace GEP.Cumulus.P2P.Req.BusinessObjects.Proxy
{
    [ExcludeFromCodeCoverage]
    public class RequisitionBaseProxy : GEPBaseProxy
    {
        private string JWTToken { get; set; }
        public string GetToken()
        {
            return this.JWTToken;
        }
        private UserExecutionContext UserExecutionContext = null;
        private static readonly ILog log = Logger.GetLog(MethodBase.GetCurrentMethod().DeclaringType);
        public RequisitionBaseProxy(UserExecutionContext userExecutionContext, string JWTToken)
        {
            this.UserExecutionContext = userExecutionContext;
            this.JWTToken = JWTToken;           
        }

        // This method call will be from Proxy classes only, just before function call
        //public void //SetTokenInHeader(string functionName, string appName)
        //{
        //    //This code is currently commented for testing purpose.
        //    // try
        //    // {
        //    //     LogNewRelicAppForJWTTokenTracking("//SetTokenInHeader called", functionName, appName);
        //    //     var messageHeader = MessageHeader.CreateHeader("Authorization", "Gep.Cumulus", "Bearer " + this.JWTToken);
        //    //     OperationContext.Current.OutgoingMessageHeaders.Add(messageHeader);

        //    //     var messageHeaderForAppName = MessageHeader.CreateHeader("AppName", "Gep.Cumulus", "Requisition: " + appName);
        //    //     OperationContext.Current.OutgoingMessageHeaders.Add(messageHeaderForAppName);
        //    // }
        //    // catch (Exception ex)
        //    // {
        //    //     LogNewRelicAppForJWTTokenTracking("Exception:" + ex.Message, functionName, appName);
        //    // }
        //}        

        public UserExecutionContext GetUserExecutionContext()
        {
            return this.UserExecutionContext;
        }

        private void LogNewRelicAppForJWTTokenTracking(string message, string functionName, string appName)
        {
            var eventAttributes = new Dictionary<string, object>();
            eventAttributes.Add("message", message);
            eventAttributes.Add("function", functionName);
            eventAttributes.Add("app", appName);
            eventAttributes.Add("BPC", UserExecutionContext.BuyerPartnerCode);
            NewRelic.Api.Agent.NewRelic.RecordCustomEvent("RequisitionBaseProxy", eventAttributes);
        }
    }
}
