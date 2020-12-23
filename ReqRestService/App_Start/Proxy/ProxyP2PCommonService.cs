using Gep.Cumulus.CSM.Entities;
using Gep.Cumulus.ExceptionManager;
using GEP.Cumulus.Documents.Entities;
using GEP.Cumulus.Logging;
using GEP.Cumulus.P2P.BusinessEntities;
using GEP.SMART.CommunicationLayer;
using GEP.SMART.Configuration;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.ServiceModel;
using SMARTFaultException = Gep.Cumulus.ExceptionManager;
namespace GEP.Cumulus.P2P.Req.RestService.App_Start.Proxy
{
    [ExcludeFromCodeCoverage]
    internal class ProxyP2PCommonService
    {
        #region "Variables"
        public GepServiceFactory GepServices = GepServiceManager.GetInstance;

        public string ServiceUrl = UrlHelperExtensions.P2PCommonServiceStringUrl;
        private IP2PCommonServiceChannel objCommonServiceChannel = null;
        private OperationContextScope objOperationContextScope = null;

        private UserExecutionContext UserExecutionContext = null;
        private static readonly ILog Log = Logger.GetLog(MethodBase.GetCurrentMethod().DeclaringType);
        private string JWTToken = string.Empty;

        public ProxyP2PCommonService(UserExecutionContext UserExecutionContext, string jwtToken)
        {
            this.UserExecutionContext = UserExecutionContext;
            this.JWTToken = jwtToken;
        }
        #endregion

        private void AddToken(string jwtToken)
        {
            MessageHeader<string> objMhgAuth = new MessageHeader<string>(jwtToken);
            System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
            OperationContext.Current.OutgoingMessageHeaders.Add(authorization);
        }
        public List<DocumentProxyDetails> CheckOriginalApproverNotificationStatus()
        {
            try
            {
                objCommonServiceChannel = ServiceHelper.ConfigureChannel<IP2PCommonServiceChannel>(ServiceUrl, UserExecutionContext, ref objOperationContextScope);
                using (objOperationContextScope)
                {
                    AddToken(JWTToken);
                    return objCommonServiceChannel.CheckOriginalApproverNotificationStatus();
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in CheckOriginalApproverNotificationStatus Method", ex);
                var objCustomFault = new CustomFault(ex.Message, "CheckOriginalApproverNotificationStatus", "CheckOriginalApproverNotificationStatus", "Common", ExceptionType.ApplicationException, "", false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while CheckOriginalApproverNotificationStatus");
            }
            finally
            {
                ServiceHelper.DisposeService(objCommonServiceChannel);
            }
        }

        public string GetSettingsValueByKey(P2PDocumentType docType, string key, long userId, int subAppCode)
        {

            var result = string.Empty;
            var wcfClient = new ResilientWCFClient(this, Newtonsoft.Json.JsonConvert.SerializeObject(this.UserExecutionContext));
            var gepCommunicationContext = new GEPCommunicationContext("ProxyP2PCommonService-GetSettingsValueByKey");

            gepCommunicationContext.Add("docType", docType);
            gepCommunicationContext.Add("key", key);
            gepCommunicationContext.Add("userId", userId);
            gepCommunicationContext.Add("subAppCode", subAppCode);

            var token = this.JWTToken;
            var wcfResult = wcfClient.Execute<IP2PCommonServiceChannel>((context, channel) =>
            {
                AddToken(token);
                result = channel.GetSettingsValueByKey((P2PDocumentType)context["docType"], (string)context["key"], (long)context["userId"], (int)context["subAppCode"]);
            }, gepCommunicationContext, CloudConfig.P2PCommonServiceURL, string.Empty);

            if (wcfResult.Outcome == Polly.OutcomeType.Successful)
            {
                return result;
            }
            else
            {
                LogHelper.LogError(Log, "Error occured in GetSettingsValueByKey Method", wcfResult.FinalException);
                var customFault = new CustomFault(wcfResult.FinalException.Message, "GetSettingsValueByKey", "GetSettingsValueByKey", "ProxyP2PCommonService", ExceptionType.ApplicationException, string.Empty, false);
                throw new SMARTFaultException.FaultException<CustomFault>(customFault, "Error while executing GetSettingsValueByKey in ProxyP2PCommonService");
            }
        }

        public string GetSettingsValueByKeyAndLOB(P2PDocumentType docType, string key, long userId, int subAppCode, long LOBId = 0)
        {

            var result = string.Empty;
            var wcfClient = new ResilientWCFClient(this, Newtonsoft.Json.JsonConvert.SerializeObject(this.UserExecutionContext));
            var gepCommunicationContext = new GEPCommunicationContext("ProxyP2PCommonService-GetSettingsValueByKeyAndLOB");

            gepCommunicationContext.Add("docType", docType);
            gepCommunicationContext.Add("key", key);
            gepCommunicationContext.Add("userId", userId);
            gepCommunicationContext.Add("subAppCode", subAppCode);
            gepCommunicationContext.Add("LOBId", LOBId);

            var token = this.JWTToken;

            var wcfResult = wcfClient.Execute<IP2PCommonServiceChannel>((context, channel) =>
            {
                AddToken(token);
                result = channel.GetSettingsValueByKeyAndLOB((P2PDocumentType)context["docType"], (string)context["key"], (long)context["userId"], (int)context["subAppCode"], (long)context["LOBId"]);
            }, gepCommunicationContext, CloudConfig.P2PCommonServiceURL, string.Empty);

            if (wcfResult.Outcome == Polly.OutcomeType.Successful)
            {
                return result;
            }
            else
            {
                LogHelper.LogError(Log, "Error occured in GetSettingsValueByKeyAndLOB Method", wcfResult.FinalException);
                var customFault = new CustomFault(wcfResult.FinalException.Message, "GetSettingsValueByKeyAndLOB", "GetSettingsValueByKeyAndLOB", "ProxyP2PCommonService", ExceptionType.ApplicationException, string.Empty, false);
                throw new SMARTFaultException.FaultException<CustomFault>(customFault, "Error while executing GetSettingsValueByKeyAndLOB in ProxyP2PCommonService");
            }
        }

        public CommentsGroup SaveComments(CommentsGroup objCommentsGroup, P2PDocumentType docType, int level)
        {
            try
            {
                objCommonServiceChannel = ServiceHelper.ConfigureChannel<IP2PCommonServiceChannel>(ServiceUrl, UserExecutionContext, ref objOperationContextScope);
                using (objOperationContextScope)
                {
                    AddToken(JWTToken);
                    return objCommonServiceChannel.SaveComments(objCommentsGroup, docType, level);
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in SaveComments Method", ex);
                var objCustomFault = new CustomFault(ex.Message, "SaveComments", "SaveComments", "Common", ExceptionType.ApplicationException, "", false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while SaveComments");
            }
            finally
            {
                ServiceHelper.DisposeService(objCommonServiceChannel);
            }
        }


        public List<DocumentProxyDetails> CheckOriginalReviewerNotificationStatus()
        {
            try
            {
                objCommonServiceChannel = ServiceHelper.ConfigureChannel<IP2PCommonServiceChannel>(ServiceUrl, UserExecutionContext, ref objOperationContextScope);
                using (objOperationContextScope)
                {
                    AddToken(JWTToken);
                    return objCommonServiceChannel.CheckOriginalReviewerNotificationStatus();
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in CheckOriginalReviewerNotificationStatus Method", ex);
                var objCustomFault = new CustomFault(ex.Message, "CheckOriginalReviewerNotificationStatus", "CheckOriginalReviewerNotificationStatus", "Common", ExceptionType.ApplicationException, "", false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while CheckOriginalReviewerNotificationStatus");
            }
            finally
            {
                ServiceHelper.DisposeService(objCommonServiceChannel);
            }
        }

        public Documents.Entities.DocumentLOBDetails GetDocumentLOB(long documentCode)
        {

            DocumentLOBDetails documentLOBDetails = new DocumentLOBDetails();
            var wcfClient = new ResilientWCFClient(this, Newtonsoft.Json.JsonConvert.SerializeObject(this.UserExecutionContext));
            var gepCommunicationContext = new GEPCommunicationContext("ProxyP2PCommonService-GetDocumentLOB");
            gepCommunicationContext.Add("documentCode", documentCode);

            var token = this.JWTToken;

            var wcfResult = wcfClient.Execute<IP2PCommonServiceChannel>((context, channel) =>
            {
                AddToken(token);
                documentLOBDetails = channel.GetDocumentLOB((long)context["documentCode"]);
            }, gepCommunicationContext, CloudConfig.P2PCommonServiceURL, string.Empty);

            if (wcfResult.Outcome == Polly.OutcomeType.Successful)
            {
                return documentLOBDetails;
            }
            else
            {
                LogHelper.LogError(Log, "Error occured in GetDocumentLOB Method", wcfResult.FinalException);
                var customFault = new CustomFault(wcfResult.FinalException.Message, "GetDocumentLOB", "GetDocumentLOB", "ProxyP2PCommonService", ExceptionType.ApplicationException, string.Empty, false);
                throw new SMARTFaultException.FaultException<CustomFault>(customFault, "Error while executing GetDocumentLOB in ProxyP2PCommonService");
            }
        }
    }
}
