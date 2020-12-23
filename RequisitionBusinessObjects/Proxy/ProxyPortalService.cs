using Gep.Cumulus.CSM.Entities;
using GEP.Cumulus.Logging;
using GEP.Cumulus.P2P.Req.BusinessObjects.Proxy;
using GEP.Cumulus.Portal.Entities;
using GEP.Cumulus.Web.Utils;
using GEP.SMART.Configuration;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.ServiceModel;

namespace GEP.Cumulus.P2P.BusinessObjects.Proxy
{
    [ExcludeFromCodeCoverage]
    internal class ProxyPortalService : RequisitionBaseProxy
    {
        public GepServiceFactory GepServices = GepServiceManager.GetInstance;

        private UserExecutionContext UserExecutionContext = null;

        private static readonly ILog log = Logger.GetLog(MethodBase.GetCurrentMethod().DeclaringType);

        private string appName = System.Environment.GetEnvironmentVariable("NewRelic.AppName") ?? "Requisition";
        public ProxyPortalService(UserExecutionContext userExecutionContext, string jwtToken): base(userExecutionContext, jwtToken)
        {
            this.UserExecutionContext = userExecutionContext;
        }

        public ICollection<TaxMaster> GetFilteredTaxMasterDetails(long DivisionEntityCode = 0, int EntityId = 0, long EntityDetailCode = 0)
        {
            IPortalServiceChannel _objPortalServiceChannel = null;
            OperationContextScope scope = null;
            GEPBaseProxy gepBaseProxy = new GEPBaseProxy();
            try
            {
                _objPortalServiceChannel = gepBaseProxy.ConfigureChannel<IPortalServiceChannel>(CloudConfig.PortalServiceURL, UserExecutionContext, ref scope);

                MessageHeader<string> objMhgAuth = new MessageHeader<string>(GetToken());
                System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
                OperationContext.Current.OutgoingMessageHeaders.Add(authorization);

                return _objPortalServiceChannel.GetFilteredTaxMasterDetails("", "", 0, 1000, "", DivisionEntityCode, EntityId, EntityDetailCode);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(log, "Error occurred in GetFilteredTaxMasterDetails method", ex);
                throw;
            }
            finally
            {
                GEPServiceManager.DisposeService(_objPortalServiceChannel, scope);
            }
        }

    }
}
