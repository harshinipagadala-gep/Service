using Gep.Cumulus.CSM.Entities;
using Gep.Cumulus.ExceptionManager;
using GEP.Cumulus.Logging;
using GEP.Cumulus.OrganizationStructure.Entities;
using GEP.Cumulus.Web.Utils;
using GEP.SMART.Configuration;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.ServiceModel;
using OrganizationContract = GEP.Cumulus.OrganizationStructure.ServiceContracts;
using SMARTFaultException = Gep.Cumulus.ExceptionManager;
namespace GEP.Cumulus.P2P.Req.BusinessObjects.Proxy
{
    [ExcludeFromCodeCoverage]
    internal class ProxyOrganizationStructureService: RequisitionBaseProxy
    {

        //
        // GET: /ProxyIRService/
        #region "Variables"
        public GepServiceFactory GepServices = GepServiceManager.GetInstance;

        private UserExecutionContext UserExecutionContext = null;

        private readonly string _organizationEndPointAddress = MultiRegionConfig.GetConfig(CloudConfig.OrganizationStructureServiceURL);

        private static readonly ILog log = Logger.GetLog(MethodBase.GetCurrentMethod().DeclaringType);

        private string appName = System.Environment.GetEnvironmentVariable("NewRelic.AppName") ?? "Requisition";

        public ProxyOrganizationStructureService(UserExecutionContext userExecutionContext, string jwtToken): base(userExecutionContext, jwtToken)
        {
            this.UserExecutionContext = GetUserExecutionContext();
        }
        public interface IOrganizationStructureChannel : OrganizationContract.IOrganizationStructure, IClientChannel
        {
        }
        #endregion
        public OrgEntity GetOrgEntityByEntityCode(string entityCode, string entityType, string LobEntityCode = "")
        {
            OrgEntity objOrgEntity = new OrgEntity();
            IOrganizationStructureChannel objOrganizationServiceChannel = null;
            OperationContextScope scope = null;
            GEPBaseProxy gepBaseProxy = new GEPBaseProxy();
            try
            {
                objOrganizationServiceChannel = gepBaseProxy.ConfigureChannel<IOrganizationStructureChannel>(CloudConfig.OrganizationStructureServiceURL, UserExecutionContext, ref scope);

                MessageHeader<string> objMhgAuth = new MessageHeader<string>(GetToken());
                System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
                OperationContext.Current.OutgoingMessageHeaders.Add(authorization);

                objOrgEntity = objOrganizationServiceChannel.GetOrgEntityByEntityCode(entityCode, entityType, LobEntityCode);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(log, "Error occured in GetOrgEntityByEntityCode Method", ex);
                var objCustomFault = new CustomFault(ex.Message, "GetOrgEntityByEntityCode", "GetOrgEntityByEntityCode", "AppStartBO", ExceptionType.ApplicationException, "", false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while GetOrgEntityByEntityCode");
            }
            finally
            {
                GEPServiceManager.DisposeService(objOrganizationServiceChannel, scope);
            }
            return objOrgEntity;
        }
        public List<OrgSearchResult> GetEntitySearchResults(OrgSearch objSearch)
        {
            List<OrgSearchResult> result = new List<OrgSearchResult>();
            IOrganizationStructureChannel objOrganizationServiceChannel = null;
            OperationContextScope scope = null;
            GEPBaseProxy gepBaseProxy = new GEPBaseProxy();
            try
            {
                objOrganizationServiceChannel = gepBaseProxy.ConfigureChannel<IOrganizationStructureChannel>(CloudConfig.OrganizationStructureServiceURL, UserExecutionContext, ref scope);

                MessageHeader<string> objMhgAuth = new MessageHeader<string>(GetToken());
                System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
                OperationContext.Current.OutgoingMessageHeaders.Add(authorization);

                using (scope)
                    result = objOrganizationServiceChannel.GetEntitySearchResults(objSearch);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(log, "Error occured in GetEntitySearchResults Method for objSearch=" + objSearch, ex);
                var objCustomFault = new CustomFault(ex.Message, "GetEntitySearchResults", "GetEntitySearchResults", "AppStartBO", ExceptionType.ApplicationException, "", false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while GetEntitySearchResults");
            }
            finally
            {
                GEPServiceManager.DisposeService(objOrganizationServiceChannel, scope);
            }
            return result;
        }

        public List<LOBAccessControlDetail> GetLOBAccessControlDetail()
        {
            List<LOBAccessControlDetail> result = new List<LOBAccessControlDetail>();
            IOrganizationStructureChannel objOrganizationServiceChannel = null;
            OperationContextScope scope = null;
            GEPBaseProxy gepBaseProxy = new GEPBaseProxy();
            try
            {
                objOrganizationServiceChannel = gepBaseProxy.ConfigureChannel<IOrganizationStructureChannel>(CloudConfig.OrganizationStructureServiceURL, UserExecutionContext, ref scope);

                MessageHeader<string> objMhgAuth = new MessageHeader<string>(GetToken());
                System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
                OperationContext.Current.OutgoingMessageHeaders.Add(authorization);

                using (scope)
                    result = objOrganizationServiceChannel.GetLOBAccessControlDetail();
            }
            catch (Exception ex)
            {
                LogHelper.LogError(log, "Error occured in GetLOBAccessControlDetail Method", ex);
                var objCustomFault = new CustomFault(ex.Message, "GetLOBAccessControlDetail", "GetLOBAccessControlDetail", "AppStartBO", ExceptionType.ApplicationException, "", false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while GetLOBAccessControlDetail");
            }
            finally
            {
                GEPServiceManager.DisposeService(objOrganizationServiceChannel, scope);
            }
            return result;
        }

        public List<OrgEntity> GetEntityDetails(OrgSearch objORGSearch)
        {
            List<OrgEntity> result = new List<OrgEntity>();
            IOrganizationStructureChannel objOrganizationServiceChannel = null;
            OperationContextScope scope = null;
            GEPBaseProxy gepBaseProxy = new GEPBaseProxy();
            try
            {
                objOrganizationServiceChannel = gepBaseProxy.ConfigureChannel<IOrganizationStructureChannel>(CloudConfig.OrganizationStructureServiceURL, UserExecutionContext, ref scope);

                MessageHeader<string> objMhgAuth = new MessageHeader<string>(GetToken());
                System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
                OperationContext.Current.OutgoingMessageHeaders.Add(authorization);

                using (scope)
                    result = objOrganizationServiceChannel.GetEntityDetails(objORGSearch);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(log, "Error occured in GetEntityDetails Method for objSearch=" + objORGSearch, ex);
                var objCustomFault = new CustomFault(ex.Message, "GetEntityDetails", "GetEntityDetails", "AppStartBO", ExceptionType.ApplicationException, "", false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while GetEntityDetails");
            }
            finally
            {
                GEPServiceManager.DisposeService(objOrganizationServiceChannel, scope);
            }
            return result;
        }
    }
}
