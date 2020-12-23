﻿using Gep.Cumulus.CSM.Entities;
using GEP.Cumulus.P2P.ServiceContracts;
using GEP.Cumulus.Web.Utils;
using GEP.SMART.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace GEP.Cumulus.P2P.Req.RestService.App_Start
{
    #region Service Channel implementaion

    public interface IOrganizationStructureChannel : GEP.Cumulus.OrganizationStructure.ServiceContracts.IOrganizationStructure, IClientChannel
    {
    }

    public interface IProgramServiceChannel : IProgramService, IClientChannel
    {
    }

    public interface IRequisitionServiceChannel : GEP.Cumulus.P2P.Req.ServiceContracts.IRequisitionService, IClientChannel
    {
    }

    public interface IP2PDocumentServiceChannel : IP2PDocumentService, IClientChannel
    {
    }

    public interface IP2PCommonServiceChannel : IP2PCommonService, IClientChannel
    {
    }

    public interface IOperationalBudgetServiceChannel : IOperationalBudgetService, IClientChannel
    {
    }

    public interface INewRequisitionServiceChannel : GEP.Cumulus.P2P.Req.ServiceContracts.INewRequisitionService, IClientChannel
    {
    }

    [ExcludeFromCodeCoverage]
    public static class ServiceHelper
    {
        #region ConfigureChannel

        public static T ConfigureChannel<T>(string ServiceURL, UserExecutionContext UserExecutionContext, ref OperationContextScope objOperationContextScope) where T : class
        {
            T objClientChannel = GepServiceManager.GetInstance.CreateChannel<T>(ServiceURL);
            objOperationContextScope = new OperationContextScope((IClientChannel)objClientChannel);
            MessageHeader<UserExecutionContext> objMhg = new MessageHeader<UserExecutionContext>(UserExecutionContext);
            MessageHeader untypedmh = objMhg.GetUntypedHeader("GepCustomHeader", "Gep.Cumulus");
            OperationContext.Current.OutgoingMessageHeaders.Add(untypedmh);
            return objClientChannel;
        }

        public static T ConfigureChannel<T>(CloudConfig configkey, UserExecutionContext UserExecutionContext, ref OperationContextScope objOperationContextScope) where T : class
        {
            GEPBaseProxy objProxy = new GEPBaseProxy();
            return objProxy.ConfigureChannel<T>(configkey, UserExecutionContext, ref objOperationContextScope);
        }

        public static void DisposeService(ICommunicationObject objServiceChannel)
        {
            if (objServiceChannel != null)
            {
                if (objServiceChannel.State == CommunicationState.Faulted)
                    objServiceChannel.Abort();
                else if (objServiceChannel.State != CommunicationState.Closed)
                    objServiceChannel.Close();
            }
        }

        #endregion
    }
    #endregion
}
