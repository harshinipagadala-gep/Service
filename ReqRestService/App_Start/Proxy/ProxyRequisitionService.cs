using Gep.Cumulus.CSM.Entities;
using Gep.Cumulus.ExceptionManager;
using GEP.Cumulus.DocumentIntegration.Entities;
using GEP.Cumulus.Documents.Entities;
using GEP.Cumulus.Logging;
using GEP.Cumulus.P2P.BusinessEntities;
using GEP.Cumulus.Web.Utils;
using GEP.SMART.CommunicationLayer;
using GEP.SMART.Configuration;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.ServiceModel;
using System.Threading.Tasks;
using SMARTFaultException = Gep.Cumulus.ExceptionManager;
namespace GEP.Cumulus.P2P.Req.RestService.App_Start.Proxy
{
    [ExcludeFromCodeCoverage]
    internal class ProxyRequisitionService
    {
        //
        // GET: /ProxyRequisitionService/
        #region "Variables"
        public GepServiceFactory GepServices = GepServiceManager.GetInstance;

        public string ServiceUrl = UrlHelperExtensions.RequisitionServiceUrl.ToString();
        private IRequisitionServiceChannel objRequisitionServiceChannel = null;
        private OperationContextScope objOperationContextScope = null;

        private UserExecutionContext UserExecutionContext = null;
        private static readonly ILog Log = Logger.GetLog(MethodBase.GetCurrentMethod().DeclaringType);

        private string JWTToken = string.Empty;

        public ProxyRequisitionService(UserExecutionContext UserExecutionContext, string jwtToken)
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

        public List<SplitAccountingFields> GetAllSplitAccountingFields(P2PDocumentType docType, LevelType levelType, int structureId = 0, long LOBId = 0, long ACEEntityDetailCode = 0)
        {
            OperationContextScope objOperationContextScope = null;
            List<SplitAccountingFields> ListSplitAccountingFields = new List<SplitAccountingFields>();
            try
            {

                objRequisitionServiceChannel = P2P.Req.RestService.App_Start.ServiceHelper.ConfigureChannel<IRequisitionServiceChannel>(ServiceUrl, UserExecutionContext, ref objOperationContextScope);
                AddToken(JWTToken);
                ListSplitAccountingFields = objRequisitionServiceChannel.GetAllSplitAccountingFields(docType, levelType, structureId, LOBId, ACEEntityDetailCode);

            }
            catch (Exception ex)
            {
                throw ex;

            }
            finally
            {
                GEPServiceManager.DisposeService(objRequisitionServiceChannel, objOperationContextScope);
            }

            return ListSplitAccountingFields;
        }
        public void SendNotificationForRequisitionApproval(long documentCode, List<ApproverDetails> lstPendingApprover, List<ApproverDetails> lstPastApprover, string eventName, DocumentStatus documentStatus, string ApprovalType, bool EnableAsycCallback)
        {            
            var wcfClient = new ResilientWCFClient(this, Newtonsoft.Json.JsonConvert.SerializeObject(this.UserExecutionContext));
            var gepCommunicationContext = new GEPCommunicationContext("ProxyRequisitionService-SendNotificationForRequisitionApproval");
            gepCommunicationContext.Add("documentCode", documentCode);
            gepCommunicationContext.Add("lstPendingApprover", lstPendingApprover);
            gepCommunicationContext.Add("lstPastApprover", lstPastApprover);
            gepCommunicationContext.Add("eventName", eventName);
            gepCommunicationContext.Add("documentStatus", documentStatus);
            gepCommunicationContext.Add("ApprovalType", ApprovalType);

            var token = this.JWTToken;
            if (EnableAsycCallback)
            {
                Task.Factory.StartNew((state) =>
                {
                    var wcfResult = wcfClient.Execute<IRequisitionServiceChannel>((context, channel) =>
                    {
                        AddToken(token);
                        channel.SendNotificationForRequisitionApproval((long)context["documentCode"], (List<ApproverDetails>)context["lstPendingApprover"], (List<ApproverDetails>)context["lstPastApprover"], (string)context["eventName"], (DocumentStatus)context["documentStatus"], (string)context["ApprovalType"]);
                    }, (GEPCommunicationContext)state, CloudConfig.RequisitionServiceURL, string.Empty);

                    if (wcfResult.Outcome != Polly.OutcomeType.Successful)
                    {
                        LogHelper.LogError(Log, "Error occured in SendNotificationForRequisitionApproval Async Method inside ProxyRequisitionService, message :" + wcfResult.FinalException.Message, wcfResult.FinalException);
                    }
                }, gepCommunicationContext);
            }
            else
            {
                var wcfResult = wcfClient.Execute<IRequisitionServiceChannel>((context, channel) =>
                {
                    AddToken(token);
                    channel.SendNotificationForRequisitionApproval((long)context["documentCode"], (List<ApproverDetails>)context["lstPendingApprover"], (List<ApproverDetails>)context["lstPastApprover"], (string)context["eventName"], (DocumentStatus)context["documentStatus"], (string)context["ApprovalType"]);
                }, gepCommunicationContext, CloudConfig.RequisitionServiceURL, string.Empty);

                if (wcfResult.Outcome != Polly.OutcomeType.Successful)
                {
                    LogHelper.LogError(Log, "Error occured in SendNotificationForRequisitionApproval Method", wcfResult.FinalException);
                    var customFault = new CustomFault(wcfResult.FinalException.Message, "SendNotificationForRequisitionApproval", "SendNotificationForRequisitionApproval", "ProxyRequisitionService", ExceptionType.ApplicationException, string.Empty, false);
                    throw new SMARTFaultException.FaultException<CustomFault>(customFault, "Error while executing SendNotificationForRequisitionApproval in ProxyRequisitionService");
                }
            }
        }

        public void SendNotificationForSkipApproval(long documentCode, List<ApproverDetails> lstSkippedApprovers, List<ApproverDetails> lstFinalApprovers, bool EnableAsycCallback)
        {
            var wcfClient = new ResilientWCFClient(this, Newtonsoft.Json.JsonConvert.SerializeObject(this.UserExecutionContext));
            var gepCommunicationContext = new GEPCommunicationContext("ProxyRequisitionService-SendNotificationForSkipApproval");
            gepCommunicationContext.Add("documentCode", documentCode);
            gepCommunicationContext.Add("lstSkippedApprovers", lstSkippedApprovers);
            gepCommunicationContext.Add("lstFinalApprovers", lstFinalApprovers);

            var token = this.JWTToken;

            if (EnableAsycCallback)
            {
                Task.Factory.StartNew((state) =>
                {
                    var wcfResult = wcfClient.Execute<IRequisitionServiceChannel>((context, channel) =>
                    {
                        AddToken(token);
                        channel.SendNotificationForSkipApproval((long)context["documentCode"], (List<ApproverDetails>)context["lstSkippedApprovers"], (List<ApproverDetails>)context["lstFinalApprovers"]);
                    }, (GEPCommunicationContext) state, CloudConfig.RequisitionServiceURL, string.Empty);

                    if (wcfResult.Outcome != Polly.OutcomeType.Successful)
                    {
                        LogHelper.LogError(Log, "Error occured in SendNotificationForSkipApproval Async Method , message :" + wcfResult.FinalException.Message, wcfResult.FinalException);                       
                    }
                }, gepCommunicationContext);
            }
            else
            {
                var wcfResult = wcfClient.Execute<IRequisitionServiceChannel>((context, channel) =>
                {
                    AddToken(token);
                    channel.SendNotificationForSkipApproval((long)context["documentCode"], (List<ApproverDetails>)context["lstSkippedApprovers"], (List<ApproverDetails>)context["lstFinalApprovers"]);
                }, gepCommunicationContext, CloudConfig.RequisitionServiceURL, string.Empty);

                if (wcfResult.Outcome != Polly.OutcomeType.Successful)
                {
                    LogHelper.LogError(Log, "Error occured in SendNotificationForSkipApproval Method", wcfResult.FinalException);
                    var customFault = new CustomFault(wcfResult.FinalException.Message, "SendNotificationForSkipApproval", "SendNotificationForSkipApproval", "ProxyRequisitionService", ExceptionType.ApplicationException, string.Empty, false);
                    throw new SMARTFaultException.FaultException<CustomFault>(customFault, "Error while executing SendNotificationForSkipApproval in ProxyRequisitionService");
                }
            }
                
        }

        public void AutoCreateWorkBenchOrder(long RequisitionId)
        {
            var wcfClient = new ResilientWCFClient(this, Newtonsoft.Json.JsonConvert.SerializeObject(this.UserExecutionContext));
            var gepCommunicationContext = new GEPCommunicationContext("ProxyRequisitionService-AutoCreateWorkBenchOrder");
            gepCommunicationContext.Add("RequisitionId", RequisitionId);

            var token = this.JWTToken;
            var wcfResult = wcfClient.Execute<IRequisitionServiceChannel>((context, channel) =>
            {
                AddToken(token);
                channel.AutoCreateWorkBenchOrder((long)context["RequisitionId"], 2, false);
            }, gepCommunicationContext, CloudConfig.RequisitionServiceURL, string.Empty);

            if (wcfResult.Outcome != Polly.OutcomeType.Successful)
            {
                LogHelper.LogError(Log, "Error occured in AutoCreateWorkBenchOrder Method", wcfResult.FinalException);
                var customFault = new CustomFault(wcfResult.FinalException.Message, "AutoCreateWorkBenchOrder", "AutoCreateWorkBenchOrder", "ProxyRequisitionService", ExceptionType.ApplicationException, string.Empty, false);
                throw new SMARTFaultException.FaultException<CustomFault>(customFault, "Error while executing AutoCreateWorkBenchOrder in ProxyRequisitionService");
            }

        }
        public Dictionary<string, string> SentRequisitionForApproval(long contactCode, long documentCode, decimal documentAmount, int documentTypeId, string fromCurrency, string toCurrency, bool isOperationalBudgetEnabled, long headerOrgEntityCode)
        {
            OperationContextScope objOperationContextScope = null;
            try
            {
                Dictionary<string, string> result = new Dictionary<string, string>();
                objRequisitionServiceChannel = P2P.Req.RestService.App_Start.ServiceHelper.ConfigureChannel<IRequisitionServiceChannel>(ServiceUrl, UserExecutionContext, ref objOperationContextScope);
                AddToken(JWTToken);
                result = objRequisitionServiceChannel.SentRequisitionForApproval(contactCode, documentCode, documentAmount, documentTypeId, fromCurrency, toCurrency, isOperationalBudgetEnabled, headerOrgEntityCode);

                return result;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in SentRequisitionForApproval Method", ex);
                var objCustomFault = new CustomFault(ex.Message, "SentRequisitionForApproval", "SentRequisitionForApproval", "Requisition", ExceptionType.ApplicationException, "", false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while SentRequisitionForApproval");
            }
            finally
            {
                GEPServiceManager.DisposeService(objRequisitionServiceChannel, objOperationContextScope);
            }
        }
        public List<P2PDocumentValidationInfo> GetRequisitionValidationDetailsById(long requisitionId)
        {
            OperationContextScope objOperationContextScope = null;
            try
            {
                List<P2PDocumentValidationInfo> result;
                objRequisitionServiceChannel = P2P.Req.RestService.App_Start.ServiceHelper.ConfigureChannel<IRequisitionServiceChannel>(ServiceUrl, UserExecutionContext, ref objOperationContextScope);
                AddToken(JWTToken);
                result = objRequisitionServiceChannel.GetRequisitionValidationDetailsById(requisitionId);

                return result;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in GetRequisitionValidationDetailsById Method", ex);
                var objCustomFault = new CustomFault(ex.Message, "GetRequisitionValidationDetailsById", "GetRequisitionValidationDetailsById", "Requisition", ExceptionType.ApplicationException, "", false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while GetRequisitionValidationDetailsById");
            }
            finally
            {
                GEPServiceManager.DisposeService(objRequisitionServiceChannel, objOperationContextScope);
            }
        }
        public bool SaveDocumentBusinessUnit(long documentCode)
        {
            OperationContextScope objOperationContextScope = null;
            try
            {
                bool result;
                objRequisitionServiceChannel = P2P.Req.RestService.App_Start.ServiceHelper.ConfigureChannel<IRequisitionServiceChannel>(ServiceUrl, UserExecutionContext, ref objOperationContextScope);
                AddToken(JWTToken);
                result = objRequisitionServiceChannel.SaveDocumentBusinessUnit(documentCode);

                return result;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in SaveDocumentBusinessUnit Method", ex);
                var objCustomFault = new CustomFault(ex.Message, "SaveDocumentBusinessUnit", "SaveDocumentBusinessUnit", "Requisition", ExceptionType.ApplicationException, "", false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while SaveDocumentBusinessUnit");
            }
            finally
            {
                GEPServiceManager.DisposeService(objRequisitionServiceChannel, objOperationContextScope);
            }
        }
        public List<long> AutoCreateOrder(long documentCode)
        {
            var result = new List<long>();
            var wcfClient = new ResilientWCFClient(this, Newtonsoft.Json.JsonConvert.SerializeObject(this.UserExecutionContext));
            var gepCommunicationContext = new GEPCommunicationContext("ProxyRequisitionService-AutoCreateOrder");

            gepCommunicationContext.Add("documentCode", documentCode);

            var token = this.JWTToken;
            var wcfResult = wcfClient.Execute<IRequisitionServiceChannel>((context, channel) =>
            {
                AddToken(token);
                result = channel.AutoCreateOrder((long)context["documentCode"]);
            }, gepCommunicationContext, CloudConfig.RequisitionServiceURL, string.Empty);

            if (wcfResult.Outcome == Polly.OutcomeType.Successful)
            {
                return result;
            }
            else
            {
                LogHelper.LogError(Log, "Error occured in AutoCreateOrder Method", wcfResult.FinalException);
                var customFault = new CustomFault(wcfResult.FinalException.Message, "AutoCreateOrder", "AutoCreateOrder", "ProxyRequisitionService", ExceptionType.ApplicationException, string.Empty, false);
                throw new SMARTFaultException.FaultException<CustomFault>(customFault, "Error while executing AutoCreateOrder in ProxyRequisitionService");
            }
        }

        public Requisition GetRequisitionBasicDetailsById(long requisitionId, long userId, int typeOfUser)
        {
            Requisition requisition = new Requisition();
            var wcfClient = new ResilientWCFClient(this, Newtonsoft.Json.JsonConvert.SerializeObject(this.UserExecutionContext));
            var gepCommunicationContext = new GEPCommunicationContext("ProxyRequisitionService-GetRequisitionBasicDetailsById");
            gepCommunicationContext.Add("requisitionId", requisitionId);
            gepCommunicationContext.Add("userId", userId);
            gepCommunicationContext.Add("typeOfUser", typeOfUser);

            var token = this.JWTToken;
            var wcfResult = wcfClient.Execute<IRequisitionServiceChannel>((context, channel) =>
            {
                AddToken(token);
                requisition = channel.GetRequisitionBasicDetailsById((long)context["requisitionId"], (long)context["userId"], (int)context["typeOfUser"]);
            }, gepCommunicationContext, CloudConfig.RequisitionServiceURL, string.Empty);

            if (wcfResult.Outcome == Polly.OutcomeType.Successful)
            {
                return requisition;
            }
            else
            {
                LogHelper.LogError(Log, "Error occured in GetRequisitionBasicDetailsById Method", wcfResult.FinalException);
                var customFault = new CustomFault(wcfResult.FinalException.Message, "GetRequisitionBasicDetailsById", "GetRequisitionBasicDetailsById", "ProxyRequisitionService", ExceptionType.ApplicationException, string.Empty, false);
                throw new SMARTFaultException.FaultException<CustomFault>(customFault, "Error while executing GetRequisitionBasicDetailsById in ProxyRequisitionService");
            }
        }

        public void SendNotificationForRejectedRequisition(long requisition, ApproverDetails rejector, List<ApproverDetails> prevApprovers, string queryString, bool EnableAsycCallback)
        {
            var wcfClient = new ResilientWCFClient(this, Newtonsoft.Json.JsonConvert.SerializeObject(this.UserExecutionContext));
            var gepCommunicationContext = new GEPCommunicationContext("ProxyRequisitionService-SendNotificationForRejectedRequisition");
            gepCommunicationContext.Add("requisition", requisition);
            gepCommunicationContext.Add("rejector", rejector);
            gepCommunicationContext.Add("prevApprovers", prevApprovers);
            gepCommunicationContext.Add("queryString", queryString);

            var token = this.JWTToken;
            if (EnableAsycCallback)
            {
                Task.Factory.StartNew((state) =>
                {
                    var wcfResult = wcfClient.Execute<IRequisitionServiceChannel>((context, channel) =>
                    {
                        AddToken(token);
                        channel.SendNotificationForRejectedRequisition((long)context["requisition"], (ApproverDetails)context["rejector"], (List<ApproverDetails>)context["prevApprovers"], (string)context["queryString"]);
                    }, (GEPCommunicationContext)state, CloudConfig.RequisitionServiceURL, string.Empty);

                    if (wcfResult.Outcome != Polly.OutcomeType.Successful)
                    {
                        LogHelper.LogError(Log, "Error occured in SendNotificationForRejectedRequisition Async Method , message :" + wcfResult.FinalException.Message, wcfResult.FinalException);                        
                    }
                }, gepCommunicationContext);
            }
            else
            {
                var wcfResult = wcfClient.Execute<IRequisitionServiceChannel>((context, channel) =>
                {
                    AddToken(token);
                    channel.SendNotificationForRejectedRequisition((long)context["requisition"], (ApproverDetails)context["rejector"], (List<ApproverDetails>)context["prevApprovers"], (string)context["queryString"]);
                }, gepCommunicationContext, CloudConfig.RequisitionServiceURL, string.Empty);

                if (wcfResult.Outcome != Polly.OutcomeType.Successful)
                {
                    LogHelper.LogError(Log, "Error occured in SendNotificationForRejectedRequisition Method", wcfResult.FinalException);
                    var customFault = new CustomFault(wcfResult.FinalException.Message, "SendNotificationForRejectedRequisition", "SendNotificationForRejectedRequisition", "ProxyRequisitionService", ExceptionType.ApplicationException, string.Empty, false);
                    throw new SMARTFaultException.FaultException<CustomFault>(customFault, "Error while executing SendNotificationForRejectedRequisition in ProxyRequisitionService");
                }
            }
        }
        public long SaveCatalogRequisition(long userId, long reqId, string requisitionName, string requisitionNumber, long oboId = 0)
        {
            long result = 0;
            OperationContextScope objOperationContextScope = null;
            try
            {
                objRequisitionServiceChannel = P2P.Req.RestService.App_Start.ServiceHelper.ConfigureChannel<IRequisitionServiceChannel>(ServiceUrl, UserExecutionContext, ref objOperationContextScope);
                AddToken(JWTToken);
                result = objRequisitionServiceChannel.SaveCatalogRequisition(userId, reqId, requisitionName, requisitionNumber, oboId);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in SaveCatalogRequisition Method", ex);
                var objCustomFault = new CustomFault(ex.Message, "SaveCatalogRequisition", "GetAllSplitAccountingFieldsWithDefaultValues", "Requisition", ExceptionType.ApplicationException, "", false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while SaveCatalogRequisition");
            }
            finally
            {
                GEPServiceManager.DisposeService(objRequisitionServiceChannel, objOperationContextScope);
            }
            return result;
        }

        public List<SplitAccountingFields> GetAllSplitAccountingFieldsWithDefaultValues(P2PDocumentType docType, LevelType levelType, long documentCode, long OnBehalfOfId)
        {
            OperationContextScope objOperationContextScope = null;
            try
            {
                objRequisitionServiceChannel = P2P.Req.RestService.App_Start.ServiceHelper.ConfigureChannel<IRequisitionServiceChannel>(ServiceUrl, UserExecutionContext, ref objOperationContextScope);
                AddToken(JWTToken);
                return objRequisitionServiceChannel.GetAllSplitAccountingFieldsWithDefaultValues(docType, levelType, documentCode, OnBehalfOfId);

            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, string.Concat("Error occured in GetAllSplitAccountingFieldsWithDefaultValues Method with requisitionid" + documentCode), ex);
                var objCustomFault = new CustomFault(ex.Message, "GetAllSplitAccountingFieldsWithDefaultValues", "GetAllSplitAccountingFieldsWithDefaultValues", "Requisition", ExceptionType.ApplicationException, "", false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while GetAllSplitAccountingFieldsWithDefaultValues");
            }
            finally
            {
                GEPServiceManager.DisposeService(objRequisitionServiceChannel, objOperationContextScope);
            }
        }
        public Dictionary<string, string> SaveOfflineApprovalDetails(long documentCode, long contactCode, decimal documentAmount, string fromCurrency, string toCurrency, GEP.Cumulus.P2P.BusinessEntities.WorkflowInputEntities workflowEntity, long headerOrgEntityCode)
        {
            try
            {
                objRequisitionServiceChannel = P2P.Req.RestService.App_Start.ServiceHelper.ConfigureChannel<IRequisitionServiceChannel>(ServiceUrl, UserExecutionContext, ref objOperationContextScope);
                AddToken(JWTToken);
                return objRequisitionServiceChannel.SaveOfflineApprovalDetails(contactCode, documentCode, documentAmount, fromCurrency, toCurrency, workflowEntity, headerOrgEntityCode);

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                GEPServiceManager.DisposeService(objRequisitionServiceChannel, objOperationContextScope);
            }
        }
        public bool UpdateRequisitionLineStatusonRFXCreateorUpdate(long documentCode, List<long> p2pLineItemId, DocumentType docType, bool IsDocumentDeleted = false)
        {
            try
            {
                objRequisitionServiceChannel = P2P.Req.RestService.App_Start.ServiceHelper.ConfigureChannel<IRequisitionServiceChannel>(ServiceUrl, UserExecutionContext, ref objOperationContextScope);
                AddToken(JWTToken);
                return objRequisitionServiceChannel.UpdateRequisitionLineStatusonRFXCreateorUpdate(documentCode, p2pLineItemId, docType, IsDocumentDeleted);

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                GEPServiceManager.DisposeService(objRequisitionServiceChannel, objOperationContextScope);
            }
        }

        public bool UpdateLineStatusForRequisition(long RequisitionId, StockReservationStatus LineStatus, bool IsUpdateAllItems, List<P2P.BusinessEntities.LineStatusRequisition> Items)
        {
            bool result = true;
            var wcfClient = new ResilientWCFClient(this, Newtonsoft.Json.JsonConvert.SerializeObject(this.UserExecutionContext));
            var gepCommunicationContext = new GEPCommunicationContext("ProxyRequisitionService-UpdateLineStatusForRequisition");
            gepCommunicationContext.Add("RequisitionId", RequisitionId);
            gepCommunicationContext.Add("LineStatus", LineStatus);
            gepCommunicationContext.Add("IsUpdateAllItems", IsUpdateAllItems);
            gepCommunicationContext.Add("Items", Items);

            var token = this.JWTToken;
            var wcfResult = wcfClient.Execute<IRequisitionServiceChannel>((context, channel) =>
            {
                AddToken(token);
                result = channel.UpdateLineStatusForRequisition((long)context["RequisitionId"], (StockReservationStatus)context["LineStatus"], (bool)context["IsUpdateAllItems"], (List<P2P.BusinessEntities.LineStatusRequisition>)context["Items"]);
            }, gepCommunicationContext, CloudConfig.RequisitionServiceURL, string.Empty);

            if (wcfResult.Outcome == Polly.OutcomeType.Successful)
            {
                return result;
            }
            else
            {
                LogHelper.LogError(Log, "Error occured in UpdateLineStatusForRequisition Method", wcfResult.FinalException);
                var customFault = new CustomFault(wcfResult.FinalException.Message, "UpdateLineStatusForRequisition", "UpdateLineStatusForRequisition", "ProxyRequisitionService", ExceptionType.ApplicationException, string.Empty, false);
                throw new SMARTFaultException.FaultException<CustomFault>(customFault, "Error while executing UpdateLineStatusForRequisition in ProxyRequisitionService");
            }

        }

        public long SaveRequisitionItemShippingDetails(long reqLineItemShippingId, long requisitionItemId, string shippingMethod, int shiptoLocationId, int delivertoLocationId, decimal quantity, decimal totalQuantity, long userid, int precessionValue, int maxPrecessionforTotal, int maxPrecessionForTaxesAndCharges, bool prorateLineItemTax = true, string deliverTo = "")
        {
            try
            {
                objRequisitionServiceChannel = P2P.Req.RestService.App_Start.ServiceHelper.ConfigureChannel<IRequisitionServiceChannel>(ServiceUrl, UserExecutionContext, ref objOperationContextScope);
                AddToken(JWTToken);
                return objRequisitionServiceChannel.SaveRequisitionItemShippingDetails(reqLineItemShippingId, requisitionItemId, shippingMethod, shiptoLocationId, delivertoLocationId, quantity, totalQuantity, userid, precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges, prorateLineItemTax);

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                GEPServiceManager.DisposeService(objRequisitionServiceChannel, objOperationContextScope);
            }
        }

        public void SendNotificationForRequisitionReview(long documentCode, List<ReviewerDetails> lstPendingReviewers, List<ReviewerDetails> lstPastReviewer, string eventName, DocumentStatus documentStatus, string reviewType, bool EnableAsycCallback)
        {

            var wcfClient = new ResilientWCFClient(this, Newtonsoft.Json.JsonConvert.SerializeObject(this.UserExecutionContext));
            var gepCommunicationContext = new GEPCommunicationContext("ProxyRequisitionService-SendNotificationForRequisitionReview");
            gepCommunicationContext.Add("documentCode", documentCode);
            gepCommunicationContext.Add("lstPendingReviewers", lstPendingReviewers);
            gepCommunicationContext.Add("lstPastReviewer", lstPastReviewer);
            gepCommunicationContext.Add("eventName", eventName);
            gepCommunicationContext.Add("documentStatus", documentStatus);
            gepCommunicationContext.Add("reviewType", reviewType);

            var token = this.JWTToken;

            if (EnableAsycCallback)
            {
                Task.Factory.StartNew((state) =>
                {
                    var wcfResult = wcfClient.Execute<IRequisitionServiceChannel>((context, channel) =>
                    {
                        AddToken(token);
                        channel.SendNotificationForRequisitionReview((long)context["documentCode"], (List<ReviewerDetails>)context["lstPendingReviewers"], (List<ReviewerDetails>)context["lstPastReviewer"], (string)context["eventName"], (DocumentStatus)context["documentStatus"], (string)context["reviewType"]);
                    }, (GEPCommunicationContext)state, CloudConfig.RequisitionServiceURL, string.Empty);

                    if (wcfResult.Outcome != Polly.OutcomeType.Successful)
                    {
                        LogHelper.LogError(Log, "Error occured in SendNotificationForRequisitionReview Async Method , message :" + wcfResult.FinalException.Message, wcfResult.FinalException);                        
                    }
                }, gepCommunicationContext);
            }
            else
            {
                var wcfResult = wcfClient.Execute<IRequisitionServiceChannel>((context, channel) =>
                {
                    AddToken(token);
                    channel.SendNotificationForRequisitionReview((long)context["documentCode"], (List<ReviewerDetails>)context["lstPendingReviewers"], (List<ReviewerDetails>)context["lstPastReviewer"], (string)context["eventName"], (DocumentStatus)context["documentStatus"], (string)context["reviewType"]);
                }, gepCommunicationContext, CloudConfig.RequisitionServiceURL, string.Empty);

                if (wcfResult.Outcome != Polly.OutcomeType.Successful)
                {
                    LogHelper.LogError(Log, "Error occured in SendNotificationForRequisitionReview Method", wcfResult.FinalException);
                    var customFault = new CustomFault(wcfResult.FinalException.Message, "SendNotificationForRequisitionReview", "SendNotificationForRequisitionReview", "ProxyRequisitionService", ExceptionType.ApplicationException, string.Empty, false);
                    throw new SMARTFaultException.FaultException<CustomFault>(customFault, "Error while executing SendNotificationForRequisitionReview in ProxyRequisitionService");
                }
            }

            
        }

        public void SendNotificationForReviewRejectedRequisition(long requisition, ReviewerDetails rejector, List<ReviewerDetails> prevApprovers, string queryString, bool EnableAsycCallback)
        {

            var wcfClient = new ResilientWCFClient(this, Newtonsoft.Json.JsonConvert.SerializeObject(this.UserExecutionContext));
            var gepCommunicationContext = new GEPCommunicationContext("ProxyRequisitionService-SendNotificationForReviewRejectedRequisition");
            gepCommunicationContext.Add("requisition", requisition);
            gepCommunicationContext.Add("rejector", rejector);
            gepCommunicationContext.Add("prevApprovers", prevApprovers);
            gepCommunicationContext.Add("queryString", queryString);

            var token = this.JWTToken;
            if (EnableAsycCallback)
            {
                Task.Factory.StartNew((state) =>
                {
                    var wcfResult = wcfClient.Execute<IRequisitionServiceChannel>((context, channel) =>
                    {
                        AddToken(token);
                        channel.SendNotificationForReviewRejectedRequisition((long)context["requisition"], (ReviewerDetails)context["rejector"], (List<ReviewerDetails>)context["prevApprovers"], (string)context["queryString"]);
                    }, (GEPCommunicationContext) state, CloudConfig.RequisitionServiceURL, string.Empty);

                    if (wcfResult.Outcome != Polly.OutcomeType.Successful)
                    {
                        LogHelper.LogError(Log, "Error occured in SendNotificationForReviewRejectedRequisition  Async Method , message :" + wcfResult.FinalException.Message, wcfResult.FinalException);                       
                    }
                }, gepCommunicationContext);
            }
            else
            {
                var wcfResult = wcfClient.Execute<IRequisitionServiceChannel>((context, channel) =>
                {
                    AddToken(token);
                    channel.SendNotificationForReviewRejectedRequisition((long)context["requisition"], (ReviewerDetails)context["rejector"], (List<ReviewerDetails>)context["prevApprovers"], (string)context["queryString"]);
                }, gepCommunicationContext, CloudConfig.RequisitionServiceURL, string.Empty);

                if (wcfResult.Outcome != Polly.OutcomeType.Successful)
                {
                    LogHelper.LogError(Log, "Error occured in SendNotificationForReviewRejectedRequisition Method", wcfResult.FinalException);
                    var customFault = new CustomFault(wcfResult.FinalException.Message, "SendNotificationForReviewRejectedRequisition", "SendNotificationForReviewRejectedRequisition", "ProxyRequisitionService", ExceptionType.ApplicationException, string.Empty, false);
                    throw new SMARTFaultException.FaultException<CustomFault>(customFault, "Error while executing SendNotificationForReviewRejectedRequisition in ProxyRequisitionService");
                }
            }
                
        }

        public void SendReviewedRequisitionForApproval(Requisition requisition)
        {
            var wcfClient = new ResilientWCFClient(this, Newtonsoft.Json.JsonConvert.SerializeObject(this.UserExecutionContext));
            var gepCommunicationContext = new GEPCommunicationContext("ProxyRequisitionService-SendReviewedRequisitionForApproval");
            gepCommunicationContext.Add("requisition", requisition);

            var token = this.JWTToken;
            var wcfResult = wcfClient.Execute<IRequisitionServiceChannel>((context, channel) =>
            {
                AddToken(token);
                channel.SendReviewedRequisitionForApproval((Requisition)context["requisition"]);
            }, gepCommunicationContext, CloudConfig.RequisitionServiceURL, string.Empty);

            if (wcfResult.Outcome != Polly.OutcomeType.Successful)
            {
                LogHelper.LogError(Log, "Error occured in SendReviewedRequisitionForApproval Method", wcfResult.FinalException);
                var customFault = new CustomFault(wcfResult.FinalException.Message, "SendReviewedRequisitionForApproval", "SendReviewedRequisitionForApproval", "ProxyRequisitionService", ExceptionType.ApplicationException, string.Empty, false);
                throw new SMARTFaultException.FaultException<CustomFault>(customFault, "Error while executing SendReviewedRequisitionForApproval in ProxyRequisitionService");
            }
        }

        public void SendNotificationForReviewAcceptedRequisition(long requisitionId, ReviewerDetails acceptor, string queryString, bool EnableAsycCallback)
        {
            LogNewRelic("Inside proxy method SendNotificationForReviewAcceptedRequisition LN 597. EnableAsycCallback:" + EnableAsycCallback.ToString(), "SendNotificationForReviewAcceptedRequisition", requisitionId);
            var wcfClient = new ResilientWCFClient(this, Newtonsoft.Json.JsonConvert.SerializeObject(this.UserExecutionContext));
            var gepCommunicationContext = new GEPCommunicationContext("ProxyRequisitionService-SendNotificationForReviewAcceptedRequisition");
            gepCommunicationContext.Add("requisitionId", requisitionId);
            gepCommunicationContext.Add("acceptor", acceptor);
            gepCommunicationContext.Add("queryString", queryString);

            var token = this.JWTToken;
            if (EnableAsycCallback)
            {
                Task.Factory.StartNew((state) =>
                {
                    var wcfResult = wcfClient.Execute<IRequisitionServiceChannel>((context, channel) =>
                    {
                        AddToken(token);
                        LogNewRelic("Inside Task factory LN 612. token:" + token , "SendNotificationForReviewAcceptedRequisition", requisitionId);
                        channel.SendNotificationForReviewAcceptedRequisition((long)context["requisitionId"], (ReviewerDetails)context["acceptor"], (string)context["queryString"]);
                    }, (GEPCommunicationContext)state, CloudConfig.RequisitionServiceURL, string.Empty);

                    if (wcfResult.Outcome != Polly.OutcomeType.Successful)
                    {
                        LogNewRelic("Error SendNotificationForReviewAcceptedRequisition LN 618. Error:"  + wcfResult.FinalException.Message, "SendNotificationForReviewAcceptedRequisition", requisitionId);
                        LogHelper.LogError(Log, "Error occured in SendNotificationForReviewAcceptedRequisition Async Method , message :" + wcfResult.FinalException.Message, wcfResult.FinalException);                        
                    }
                }, gepCommunicationContext);
            }
            else
            {
                var wcfResult = wcfClient.Execute<IRequisitionServiceChannel>((context, channel) =>
                {
                    AddToken(token);
                    channel.SendNotificationForReviewAcceptedRequisition((long)context["requisitionId"], (ReviewerDetails)context["acceptor"], (string)context["queryString"]);
                }, gepCommunicationContext, CloudConfig.RequisitionServiceURL, string.Empty);

                if (wcfResult.Outcome != Polly.OutcomeType.Successful)
                {
                    LogHelper.LogError(Log, "Error occured in SendNotificationForReviewAcceptedRequisition Method", wcfResult.FinalException);
                    var customFault = new CustomFault(wcfResult.FinalException.Message, "SendNotificationForReviewAcceptedRequisition", "SendNotificationForReviewAcceptedRequisition", "ProxyRequisitionService", ExceptionType.ApplicationException, string.Empty, false);
                    throw new SMARTFaultException.FaultException<CustomFault>(customFault, "Error while executing SendNotificationForReviewAcceptedRequisition in ProxyRequisitionService");
                }
            }

                
        }

        public void SendNotificationForSkipOrOffLineRequisitionApproval(long documentCode, List<ApproverDetails> lstApprovers, bool EnableAsycCallback, int skipType = 0, bool isOffLine = false, long actionarId = 0)
        {
            bool result = false;
            var wcfClient = new ResilientWCFClient(this, Newtonsoft.Json.JsonConvert.SerializeObject(this.UserExecutionContext));
            var gepCommunicationContext = new GEPCommunicationContext("ProxyRequisitionService-UpdateLineStatusForRequisition");
            gepCommunicationContext.Add("documentCode", documentCode);
            gepCommunicationContext.Add("lstApprovers", lstApprovers);
            gepCommunicationContext.Add("skipType", skipType);
            gepCommunicationContext.Add("isOffLine", isOffLine);
            gepCommunicationContext.Add("actionarId", actionarId);

            var token = this.JWTToken;

            if (EnableAsycCallback)
            {
                Task.Factory.StartNew((state) =>
                {
                    var wcfResult = wcfClient.Execute<IRequisitionServiceChannel>((context, channel) =>
                    {
                        AddToken(token);
                        result = channel.SendNotificationForSkipOrOffLineRequisitionApproval((long)context["documentCode"], (List<ApproverDetails>)context["lstApprovers"], (int)context["skipType"], (bool)context["isOffLine"], (long)context["actionarId"]);
                    }, (GEPCommunicationContext) state, CloudConfig.RequisitionServiceURL, string.Empty);

                    if (wcfResult.Outcome == Polly.OutcomeType.Successful)
                    {
                        // Runns good
                        result = true;
                    }
                    else
                    {
                        LogHelper.LogError(Log, "Error occured in SendNotificationForSkipOrOffLineRequisitionApproval Async Method , message :" + wcfResult.FinalException.Message, wcfResult.FinalException);
                        result = false;
                    }
                }, gepCommunicationContext);
            }
            else
            {
                var wcfResult = wcfClient.Execute<IRequisitionServiceChannel>((context, channel) =>
                {
                    AddToken(token);
                    result = channel.SendNotificationForSkipOrOffLineRequisitionApproval((long)context["documentCode"], (List<ApproverDetails>)context["lstApprovers"], (int)context["skipType"], (bool)context["isOffLine"], (long)context["actionarId"]);
                }, gepCommunicationContext, CloudConfig.RequisitionServiceURL, string.Empty);

                if (wcfResult.Outcome == Polly.OutcomeType.Successful)
                {
                    result = true;
                }
                else
                {
                    LogHelper.LogError(Log, "Error occured in SendNotificationForSkipOrOffLineRequisitionApproval Method", wcfResult.FinalException);
                    var customFault = new CustomFault(wcfResult.FinalException.Message, "SendNotificationForSkipOrOffLineRequisitionApproval", "SendNotificationForSkipOrOffLineRequisitionApproval", "ProxyRequisitionService", ExceptionType.ApplicationException, string.Empty, false);
                    throw new SMARTFaultException.FaultException<CustomFault>(customFault, "Error while executing SendNotificationForSkipOrOffLineRequisitionApproval in ProxyRequisitionService");
                }
            }
               
        }

        public DocumentIntegrationResults CreateRequisitionFromS2C(DocumentIntegrationEntity objDocumentIntegrationEntity)
        {

            try
            {
                DocumentIntegrationResults objDocumentIntegrationResults = null;

                objRequisitionServiceChannel = P2P.Req.RestService.App_Start.ServiceHelper.ConfigureChannel<IRequisitionServiceChannel>(ServiceUrl, UserExecutionContext, ref objOperationContextScope);
                AddToken(JWTToken);
                objDocumentIntegrationResults = objRequisitionServiceChannel.CreateRequisitionFromS2C(objDocumentIntegrationEntity);
                return objDocumentIntegrationResults;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in CreateRequisitionFromS2C Method", ex);
                var objCustomFault = new CustomFault(ex.Message, "CreateRequisitionFromS2C", "CreateRequisitionFromS2C", "Requisition", ExceptionType.ApplicationException, "", false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while CreateRequisitionFromS2C");

            }
            finally
            {
                GEPServiceManager.DisposeService(objRequisitionServiceChannel, objOperationContextScope);
            }
        }

        public Requisition GetAllRequisitionDetailsByRequisitionId(long requisitionId)
        {
            try
            {
                objRequisitionServiceChannel = P2P.Req.RestService.App_Start.ServiceHelper.ConfigureChannel<IRequisitionServiceChannel>(ServiceUrl, UserExecutionContext, ref objOperationContextScope);
                AddToken(JWTToken);
                return objRequisitionServiceChannel.GetAllRequisitionDetailsByRequisitionId(requisitionId);

            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in GetAllRequisitionDetailsByRequisitionId Method", ex);
                var objCustomFault = new CustomFault(ex.Message, "GetAllRequisitionDetailsByRequisitionId", "GetAllRequisitionDetailsByRequisitionId", "Requisition", ExceptionType.ApplicationException, "", false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while GetAllRequisitionDetailsByRequisitionId");

            }
            finally
            {
                GEPServiceManager.DisposeService(objRequisitionServiceChannel, objOperationContextScope);
            }
        }

        public bool ConsumeReleaseCapitalBudget(long requisitionId, DocumentStatus documentStatus, bool isReConsume)
        {
            try
            {
                objRequisitionServiceChannel = P2P.Req.RestService.App_Start.ServiceHelper.ConfigureChannel<IRequisitionServiceChannel>(ServiceUrl, UserExecutionContext, ref objOperationContextScope);
                AddToken(JWTToken);
                return objRequisitionServiceChannel.ConsumeReleaseCapitalBudget(requisitionId, documentStatus, isReConsume);

            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in ConsumeCapitalBudget Method", ex);
                var objCustomFault = new CustomFault(ex.Message, "ConsumeCapitalBudget", "ConsumeCapitalBudget", "Requisition", ExceptionType.ApplicationException, "", false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while ConsumeCapitalBudget");

            }
            finally
            {
                GEPServiceManager.DisposeService(objRequisitionServiceChannel, objOperationContextScope);
            }
        }

        private void LogNewRelic(string message, string method, long documentCode)
        {
            var eventAttributes = new Dictionary<string, object>();
            eventAttributes.Add("message", message);
            eventAttributes.Add("method", method);
            eventAttributes.Add("documentCode", documentCode.ToString());
            NewRelic.Api.Agent.NewRelic.RecordCustomEvent("RequisitionRestService_WorkFlow", eventAttributes);
        }
    }
}
