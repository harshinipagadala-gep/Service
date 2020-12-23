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
    internal class ProxyOperationalBudgetService
    {
        //
        // GET: /ProxyOperationalBudgetService/
        #region "Variables"
        public GepServiceFactory GepServices = GepServiceManager.GetInstance;

        public string ServiceUrl = UrlHelperExtensions.OperationalBudgetServiceUrl.ToString();
        private IOperationalBudgetServiceChannel objOperationalBudgetServiceChannel = null;
        private OperationContextScope objOperationContextScope = null;

        private UserExecutionContext UserExecutionContext = null;
        private static readonly ILog Log = Logger.GetLog(MethodBase.GetCurrentMethod().DeclaringType);
        private string JWTToken = string.Empty;
        public ProxyOperationalBudgetService(UserExecutionContext UserExecutionContext, string jwtToken)
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

        public bool UpdateOperationalBudgetFundsFlow(DocumentType docType, long documentCode, DocumentStatus documentStatus, bool showBillableAtHeader)
        {

            bool flag = false;
            var wcfClient = new ResilientWCFClient(this, Newtonsoft.Json.JsonConvert.SerializeObject(this.UserExecutionContext));
            var gepCommunicationContext = new GEPCommunicationContext("ProxyOperationalBudgetService-UpdateOperationalBudgetFundsFlow");

            gepCommunicationContext.Add("docType", docType);
            gepCommunicationContext.Add("documentCode", documentCode);
            gepCommunicationContext.Add("documentStatus", documentStatus);
            gepCommunicationContext.Add("showBillableAtHeader", showBillableAtHeader);

            var token = this.JWTToken;
            var wcfResult = wcfClient.Execute<IOperationalBudgetServiceChannel>((context, channel) =>
            {
                AddToken(token);
                flag = channel.UpdateOperationalBudgetFundsFlow((DocumentType)context["docType"], (long)context["documentCode"], (DocumentStatus)context["documentStatus"], (bool)context["showBillableAtHeader"]);
            }, gepCommunicationContext, CloudConfig.OperationalBudgetServiceURL, string.Empty);

            if (wcfResult.Outcome == Polly.OutcomeType.Successful)
            {
                return flag;
            }
            else
            {
                LogHelper.LogError(Log, "Error occured in UpdateOperationalBudgetFundsFlow Method", wcfResult.FinalException);
                var customFault = new CustomFault(wcfResult.FinalException.Message, "UpdateOperationalBudgetFundsFlow", "UpdateOperationalBudgetFundsFlow", "ProxyOperationalBudgetService", ExceptionType.ApplicationException, string.Empty, false);
                throw new SMARTFaultException.FaultException<CustomFault>(customFault, "Error while executing UpdateOperationalBudgetFundsFlow in ProxyOperationalBudgetService");
            }
        }
    }
}
