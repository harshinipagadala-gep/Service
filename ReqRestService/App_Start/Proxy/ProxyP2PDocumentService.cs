using Gep.Cumulus.CSM.Entities;
using Gep.Cumulus.ExceptionManager;
using GEP.Cumulus.Documents.Entities;
using GEP.Cumulus.Logging;
using GEP.SMART.CommunicationLayer;
using GEP.SMART.Configuration;
using log4net;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.ServiceModel;
using SMARTFaultException = Gep.Cumulus.ExceptionManager;
namespace GEP.Cumulus.P2P.Req.RestService.App_Start.Proxy
{
    [ExcludeFromCodeCoverage]
    internal class ProxyP2PDocumentService
    {
        #region "Variables"
        public GepServiceFactory GepServices = GepServiceManager.GetInstance;
        public string ServiceUrl = UrlHelperExtensions.P2PDocumentServiceStringUrl;
        private IP2PDocumentServiceChannel objP2PDocumentServiceChannel = null;
        private OperationContextScope objOperationContextScope = null;

        private UserExecutionContext UserExecutionContext = null;
        private static readonly ILog Log = Logger.GetLog(MethodBase.GetCurrentMethod().DeclaringType);
        private string JWTToken = string.Empty;
        public ProxyP2PDocumentService(UserExecutionContext userExecutionContext, string jwtToken)
        {
            this.UserExecutionContext = userExecutionContext;
            this.JWTToken = jwtToken;
        }
        #endregion

        private void AddToken(string jwtToken)
        {
            MessageHeader<string> objMhgAuth = new MessageHeader<string>(jwtToken);
            System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
            OperationContext.Current.OutgoingMessageHeaders.Add(authorization);
        }
        public bool SaveTaskActionDetails(TaskInformation objTaskInformation)
        {

            bool flag = false;
            var wcfClient = new ResilientWCFClient(this, Newtonsoft.Json.JsonConvert.SerializeObject(this.UserExecutionContext));
            var gepCommunicationContext = new GEPCommunicationContext("ProxyP2PDocumentService-SaveTaskActionDetails");

            gepCommunicationContext.Add("objTaskInformation", objTaskInformation);


            var token = this.JWTToken;
            var wcfResult = wcfClient.Execute<IP2PDocumentServiceChannel>((context, channel) =>
            {
                AddToken(token);
                flag = channel.SaveTaskActionDetails((TaskInformation)context["objTaskInformation"]);
            }, gepCommunicationContext, CloudConfig.P2PDocumentServiceURL, string.Empty);

            if (wcfResult.Outcome == Polly.OutcomeType.Successful)
            {
                return flag;
            }
            else
            {
                LogHelper.LogError(Log, "Error occured in SaveTaskActionDetails Method", wcfResult.FinalException);
                var customFault = new CustomFault(wcfResult.FinalException.Message, "SaveTaskActionDetails", "SaveTaskActionDetails", "ProxyP2PDocumentService", ExceptionType.ApplicationException, string.Empty, false);
                throw new SMARTFaultException.FaultException<CustomFault>(customFault, "Error while executing SaveTaskActionDetails in ProxyP2PDocumentService");
            }
        }
    }
}
