using Gep.Cumulus.CSM.Entities;
using Gep.Cumulus.ExceptionManager;
using GEP.Cumulus.Item.Contracts;
using GEP.Cumulus.Item.Entities;
using GEP.Cumulus.Logging;
using GEP.Cumulus.Web.Utils;
using GEP.SMART.CommunicationLayer;
using GEP.SMART.Configuration;
using log4net;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.ServiceModel;
using SMARTFaultException = Gep.Cumulus.ExceptionManager;

namespace GEP.Cumulus.P2P.Req.BusinessObjects.Proxy
{
    [ExcludeFromCodeCoverage]
    internal class ProxyItemService : RequisitionBaseProxy
    {
        #region "Variables"
        public GepServiceFactory GepServices = GepServiceManager.GetInstance;
        public interface IItemServiceChannel : IItemService, IClientChannel
        {
        }

        private IItemServiceChannel objItemServiceChannel = null;

        private UserExecutionContext UserExecutionContext = null;
        private OperationContextScope scope = null;
        private static readonly ILog log = Logger.GetLog(MethodBase.GetCurrentMethod().DeclaringType);

        private string appName = System.Environment.GetEnvironmentVariable("NewRelic.AppName") ?? "Requisition";

        public ProxyItemService(UserExecutionContext userExecutionContext, string jwtToken): base(userExecutionContext, jwtToken)
        {
            this.UserExecutionContext = GetUserExecutionContext();
        }
        #endregion

        public List<ItemSearchDetails> GetLineItems(ItemSearchInput itemSearch)
        {
            List<ItemSearchDetails> lstItemSearchDetails = null;
            ResilientWCFClient rWCFClient = new ResilientWCFClient(this, Newtonsoft.Json.JsonConvert.SerializeObject(UserExecutionContext));
            GEPCommunicationContext gepCommunicationContext = new GEPCommunicationContext("Proxy-GetLineItems") { { "itemSearch", itemSearch } };

            var token = GetToken();
            var pollyResult = rWCFClient.Execute<IItemServiceChannel>(
               (context, channel) =>
               {
                   MessageHeader<string> objMhgAuth = new MessageHeader<string>(token);
                   System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
                   OperationContext.Current.OutgoingMessageHeaders.Add(authorization);

                   lstItemSearchDetails = channel.GetLineItems((ItemSearchInput)context["itemSearch"]);
               }, gepCommunicationContext, CloudConfig.ItemServiceURL, string.Empty);


            if (pollyResult.Outcome == Polly.OutcomeType.Successful)
                return lstItemSearchDetails;
            else
            {
                LogHelper.LogError(log, "Error occured in GetLineItems Method in ItemProxy", pollyResult.FinalException);
                var objCustomFault = new CustomFault(pollyResult.FinalException.Message, "GetLineItems", "GetLineItems", "AppStartBO", ExceptionType.ApplicationException, "", false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while GetLineItems");
            }
        }
    }
}
