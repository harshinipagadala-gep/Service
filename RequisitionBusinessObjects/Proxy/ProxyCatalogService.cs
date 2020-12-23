using Gep.Cumulus.CSM.Entities;
using GEP.Cumulus.Logging;
using GEP.Cumulus.P2P.Req.BusinessObjects.Proxy;
using GEP.Cumulus.SmartCatalog.BusinessEntities.NewCatalog;
using GEP.Cumulus.SmartCatalog.ServiceContracts;
using GEP.Cumulus.Web.Utils;
using GEP.SMART.Configuration;
using log4net;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.ServiceModel;

namespace GEP.Cumulus.P2P.BusinessObjects.Proxy
{
    [ExcludeFromCodeCoverage]
    internal class ProxyCatalogService : RequisitionBaseProxy
    {
        private UserExecutionContext UserExecutionContext = null;
        private static readonly ILog log = Logger.GetLog(MethodBase.GetCurrentMethod().DeclaringType);
        public ILog Log { get; private set; }
        private string appName = System.Environment.GetEnvironmentVariable("NewRelic.AppName") ?? "Requisition";

        public ProxyCatalogService(UserExecutionContext userExecutionContext, string jwtToken): base(userExecutionContext, jwtToken)
        {
            this.UserExecutionContext = GetUserExecutionContext();
        }

        public interface ICatalogServiceChannel : INewCatalog, IClientChannel
        {
        }
        /// <summary>
        /// Gets catalog items as per search.
        /// </summary>
        /// <param name="search">Search object.</param>
        /// <returns>Search result.</returns>
        public SearchResult GetLineItems(ItemSearchInput itemSearchInput)
        {
            ICatalogServiceChannel catalogServiceChannel = null;
            GEPBaseProxy gepBaseProxy = new GEPBaseProxy();
            OperationContextScope scope = null;
            try
            {
                catalogServiceChannel = gepBaseProxy.ConfigureChannel<ICatalogServiceChannel>(CloudConfig.NewCatalogServiceURL, UserExecutionContext, ref scope);

                MessageHeader<string> objMhgAuth = new MessageHeader<string>(GetToken());
                System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
                OperationContext.Current.OutgoingMessageHeaders.Add(authorization);

                return catalogServiceChannel.GetLineItems(itemSearchInput);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, string.Concat("Error occured in GetLineItems method "), ex);
                throw ex;
            }
        }
        public SearchResult GetLineItemsSearch(ItemSearchInput itemSearchInput)
        {

            ICatalogServiceChannel catalogServiceChannel = null;
            GEPBaseProxy gepBaseProxy = new GEPBaseProxy();
            OperationContextScope scope = null;
            try
            {
                catalogServiceChannel = gepBaseProxy.ConfigureChannel<ICatalogServiceChannel>(CloudConfig.SmartNewCatalogServiceURL, UserExecutionContext, ref scope);

                MessageHeader<string> objMhgAuth = new MessageHeader<string>(GetToken());
                System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
                OperationContext.Current.OutgoingMessageHeaders.Add(authorization);

                return catalogServiceChannel.GetLineItems(itemSearchInput);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, string.Concat("Error occured in GetLineItemsSearch method "), ex);
                throw ex;
            }
        }
    }
}
