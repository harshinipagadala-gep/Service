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
    public class ProxyProgramService
    {
        //
        // GET: /ProxyProgramService/
        #region "Variables"
        private GepServiceFactory GepServices = GepServiceManager.GetInstance;

        private string ServiceUrl = UrlHelperExtensions.ProgramServiceUrl.ToString();
        private IProgramServiceChannel objProgramServiceChannel = null;
        private OperationContextScope objOperationContextScope = null;

        private UserExecutionContext UserExecutionContext = null;
        private static readonly ILog Log = Logger.GetLog(MethodBase.GetCurrentMethod().DeclaringType);
        private string JWTToken = string.Empty;
        public ProxyProgramService(UserExecutionContext UserExecutionContext, string jwtToken)
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

        public bool SaveProgramDocumentMapping(long programId, long DocumentCode, DocumentType DocumentType, DocumentStatus DocumentStatus, decimal TotalAmount, bool IsActive)
        {
            bool flag = false;
            var wcfClient = new ResilientWCFClient(this, Newtonsoft.Json.JsonConvert.SerializeObject(this.UserExecutionContext));
            var gepCommunicationContext = new GEPCommunicationContext("ProxyProgramService-SaveProgramDocumentMapping");

            gepCommunicationContext.Add("programId", programId);
            gepCommunicationContext.Add("DocumentCode", DocumentCode);
            gepCommunicationContext.Add("DocumentType", DocumentType);
            gepCommunicationContext.Add("DocumentStatus", DocumentStatus);
            gepCommunicationContext.Add("TotalAmount", TotalAmount);
            gepCommunicationContext.Add("IsActive", IsActive);

            var token = this.JWTToken;
            var wcfResult = wcfClient.Execute<IProgramServiceChannel>((context, channel) =>
            {
                AddToken(token);
                flag = channel.SaveProgramDocumentMapping((long)context["programId"], (long)context["DocumentCode"], (DocumentType)context["DocumentType"], (DocumentStatus)context["DocumentStatus"], (decimal)context["TotalAmount"], (bool)context["IsActive"]);
            }, gepCommunicationContext, CloudConfig.ProgramServiceURL, string.Empty);

            if (wcfResult.Outcome == Polly.OutcomeType.Successful)
            {
                return flag;
            }
            else
            {
                LogHelper.LogError(Log, string.Concat("Program SaveProgramDocumentMapping with parameters :", ",ProgramId = " + programId +
                                            ",DocumentCode = " + DocumentCode + ",DocumentType = " + DocumentType + ",DocumentStatus=" + DocumentStatus + ", TotalAmount = " + TotalAmount.ToString() + ", IsActive=" + IsActive + " was called." + " ServiceUrl " + ServiceUrl), wcfResult.FinalException);
                var customFault = new CustomFault(wcfResult.FinalException.Message, "SaveProgramDocumentMapping", "SaveProgramDocumentMapping", "ProxyProgramService", ExceptionType.ApplicationException, string.Empty, false);
                throw new SMARTFaultException.FaultException<CustomFault>(customFault, "Error while executing SaveProgramDocumentMapping in ProxyProgramService");
            }
        }
    }
}
