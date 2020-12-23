using Gep.Cumulus.CSM.Entities;
using Gep.Cumulus.ExceptionManager;
using GEP.Cumulus.Logging;
using GEP.Cumulus.P2P.Req.BusinessObjects.Proxy;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.ServiceModel;
using SMARTFaultException = Gep.Cumulus.ExceptionManager;
namespace GEP.Cumulus.P2P.Req.Service.Proxy
{
    public interface IOrderServiceChannel : PO.ServiceContracts.IOrderService, IClientChannel
    {

    }

    [ExcludeFromCodeCoverage]
    public class ProxyOrderService : RequisitionBaseProxy
    {


        //
        // GET: /ProxyOrderService/
        #region "Variables"
        public GepServiceFactory GepServices = GepServiceManager.GetInstance;

        public string ServiceUrl = UrlHelperExtensions.OrderServiceUrl.ToString();
        private IOrderServiceChannel objOrderServiceChannel = null;
        private OperationContextScope objOperationContextScope = null;

        private UserExecutionContext UserExecutionContext = null;
        private static readonly ILog Log = Logger.GetLog(MethodBase.GetCurrentMethod().DeclaringType);

        public ProxyOrderService(UserExecutionContext userExecutionContext, string jwtToken): base(userExecutionContext, jwtToken)
        {
            UserExecutionContext = GetUserExecutionContext();
        }
        #endregion

        private void AddToken()
        {
            MessageHeader<string> objMhgAuth = new MessageHeader<string>(this.GetToken());
            System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
            OperationContext.Current.OutgoingMessageHeaders.Add(authorization);
        }

        public List<long> AutoCreateWorkBenchOrder(long RequisitionId, int processFlag, bool isautosubmit)
        {
            OperationContextScope objOperationContextScope = null;
            try
            {
                objOrderServiceChannel = ServiceHelper.ConfigureChannel<IOrderServiceChannel>(ServiceUrl, UserExecutionContext, ref objOperationContextScope);
                using (objOperationContextScope)
                {
                    AddToken();
                    return objOrderServiceChannel.AutoCreateWorkBenchOrder(RequisitionId, processFlag, isautosubmit);
                }

            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in AutoCreateWorkBenchOrder Method", ex);
                var objCustomFault = new CustomFault(ex.Message, "AutoCreateWorkBenchOrder", "AutoCreateWorkBenchOrder", "Requisition", ExceptionType.ApplicationException, "", false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while AutoCreateWorkBenchOrder");

            }
            finally
            {
                ServiceHelper.DisposeService(objOrderServiceChannel);
            }
        }

        public long CreateOrderFromRequisitionItems(List<long> listItemIds, string supplierCode = "")
        {
            OperationContextScope objOperationContextScope = null;
            try
            {
                objOrderServiceChannel = ServiceHelper.ConfigureChannel<IOrderServiceChannel>(ServiceUrl, UserExecutionContext, ref objOperationContextScope);
                using (objOperationContextScope)
                {
                    AddToken();
                    return objOrderServiceChannel.CreateOrderFromRequisitionItems(listItemIds, supplierCode);
                }

            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in CreateOrderFromRequisitionItems Method", ex);
                var objCustomFault = new CustomFault(ex.Message, "CreateOrderFromRequisitionItems", "CreateOrderFromRequisitionItems", "Requisition", ExceptionType.ApplicationException, "", false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while CreateOrderFromRequisitionItems");

            }
            finally
            {
                ServiceHelper.DisposeService(objOrderServiceChannel);
            }
        }

        public List<NewP2PEntities.DocumentInfo> GetOrdersListForWorkBench(string listItemIds)
        {
            OperationContextScope objOperationContextScope = null;
            try
            {
                objOrderServiceChannel = ServiceHelper.ConfigureChannel<IOrderServiceChannel>(ServiceUrl, UserExecutionContext, ref objOperationContextScope);
                using (objOperationContextScope)
                {
                    AddToken();
                    return objOrderServiceChannel.GetOrdersListForWorkBench(listItemIds);
                }

            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in GetOrdersListForWorkBench Method", ex);
                var objCustomFault = new CustomFault(ex.Message, "GetOrdersListForWorkBench", "GetOrdersListForWorkBench", "Requisition", ExceptionType.ApplicationException, "", false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while GetOrdersListForWorkBench");

            }
            finally
            {
                ServiceHelper.DisposeService(objOrderServiceChannel);
            }
        }

        public bool SaveReqItemstoExistingPO(List<long> listItemIds, long OrderId)
        {
            OperationContextScope objOperationContextScope = null;
            try
            {
                objOrderServiceChannel = ServiceHelper.ConfigureChannel<IOrderServiceChannel>(ServiceUrl, UserExecutionContext, ref objOperationContextScope);
                using (objOperationContextScope)
                {
                    AddToken();
                    return objOrderServiceChannel.SaveReqItemstoExistingPO(listItemIds, OrderId);
                }

            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in SaveReqItemstoExistingPO Method", ex);
                var objCustomFault = new CustomFault(ex.Message, "SaveReqItemstoExistingPO", "SaveReqItemstoExistingPO", "Requisition", ExceptionType.ApplicationException, "", false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while SaveReqItemstoExistingPO");

            }
            finally
            {
                ServiceHelper.DisposeService(objOrderServiceChannel);
            }
        }

        public List<GEP.NewP2PEntities.ASLData> ValidateAndGetASLbyReqItems(GEP.NewP2PEntities.ListInputData lstSupplierdetails)
        {
            OperationContextScope objOperationContextScope = null;
            try
            {
                objOrderServiceChannel = ServiceHelper.ConfigureChannel<IOrderServiceChannel>(ServiceUrl, UserExecutionContext, ref objOperationContextScope);
                using (objOperationContextScope)
                {
                    AddToken();
                    return objOrderServiceChannel.ValidateAndGetASLbyReqItems(lstSupplierdetails);
                }

            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in ValidateAndGetASLbyReqItems Method", ex);
                var objCustomFault = new CustomFault(ex.Message, "ValidateAndGetASLbyReqItems", "ValidateAndGetASLbyReqItems", "Requisition", ExceptionType.ApplicationException, "", false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while ValidateAndGetASLbyReqItems");

            }
            finally
            {
                ServiceHelper.DisposeService(objOrderServiceChannel);
            }
        }

        public List<long> GetSettingsAndAutoCreateOrder(long documentCode)
        {
            OperationContextScope objOperationContextScope = null;
            try
            {
                objOrderServiceChannel = ServiceHelper.ConfigureChannel<IOrderServiceChannel>(ServiceUrl, UserExecutionContext, ref objOperationContextScope);
                using (objOperationContextScope)
                {
                    AddToken();
                    return objOrderServiceChannel.GetSettingsAndAutoCreateOrder(documentCode);
                }

            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in GetSettingsAndAutoCreateOrder Method", ex);
                var objCustomFault = new CustomFault(ex.Message, "GetSettingsAndAutoCreateOrder", "GetSettingsAndAutoCreateOrder", "Requisition", ExceptionType.ApplicationException, "", false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while GetSettingsAndAutoCreateOrder");

            }
            finally
            {
                ServiceHelper.DisposeService(objOrderServiceChannel);
            }
        }
    }

}
