using ESIntegratorEntities.Entities;
using EventSourceIntegrator;
using Gep.Cumulus.CSM.Entities;
using Gep.Cumulus.CSM.Extensions;
using Gep.Cumulus.ExceptionManager;
using Gep.Cumulus.Partner.Entities;
using GEP.Cumulus.DocumentIntegration.Entities;
using GEP.Cumulus.Documents.Entities;
using GEP.Cumulus.Logging;
using GEP.Cumulus.OrganizationStructure.Entities;
using GEP.Cumulus.P2P.BusinessEntities;
using GEP.Cumulus.P2P.Common;
using GEP.Cumulus.P2P.Req.RestService.App_Start;
using GEP.Cumulus.P2P.Req.RestService.App_Start.Proxy;
using GEP.Cumulus.P2P.RestService.Req.App_Start.Proxy;
using GEP.Cumulus.Web.Utils;
using GEP.Cumulus.Web.Utils.Helpers;
using GEP.SMART.CommunicationLayer;
using GEP.SMART.Configuration;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using SMARTFaultException = Gep.Cumulus.ExceptionManager;

[assembly: CLSCompliant(true)]
namespace GEP.Cumulus.P2P.Req.RestService
{
    [ExcludeFromCodeCoverage]
    public class RequisitionRestService : GEP.Cumulus.P2P.Req.RestServiceContracts.IRequisitionRestService
    {
        private static readonly ILog Log = Logger.GetLog(MethodBase.GetCurrentMethod().DeclaringType);
        private HttpWebRequest req = null;

        private string Token
        {
            get
            {
                var token = string.Empty;
                try
                {
                    if (System.ServiceModel.Web.WebOperationContext.Current != null &&
                        System.ServiceModel.Web.WebOperationContext.Current.IncomingRequest != null &&
                        System.ServiceModel.Web.WebOperationContext.Current.IncomingRequest.Headers["Authorization"] != null)
                    {
                        token = System.ServiceModel.Web.WebOperationContext.Current.IncomingRequest.Headers["Authorization"];
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.LogError(Log, "Error occured in Token RequisitionRestService :", ex);
                }
                return token;
            }
        }


        public string HealthCheck()
        {
            return "Added for probe health check";
        }
        
        public bool UpdateRequisitionApprovalStatusById(long requisitionId, string approvalStatus)
        {
            #region Debug Logging
            if (Log.IsDebugEnabled)
            {
                Log.Debug(string.Concat("In UpdateRequisitionApprovalStatusById Method in RequisitionRestService ",
                " with parameters: requisitionId = " + requisitionId + ", " +

                "approvalStatus =" + approvalStatus));
            }
            #endregion
            var result = false;
            if (requisitionId > 0)
            {
                var objP2PDocumentServiceChannel = GepServiceManager.GetInstance.CreateChannel<IRequisitionServiceChannel>(MultiRegionConfig.GetConfig(CloudConfig.RequisitionServiceURL));
                UserExecutionContext userExecutionContext = RestService.ExceptionHelper.GetExecutionContext;
                try
                {
                    using (new OperationContextScope(objP2PDocumentServiceChannel))
                    {

                        var objMhg = new MessageHeader<UserExecutionContext>(userExecutionContext);
                        var untypedmh = objMhg.GetUntypedHeader("GepCustomHeader", "Gep.Cumulus");
                        OperationContext.Current.OutgoingMessageHeaders.Add(untypedmh);

                        MessageHeader<string> objMhgAuth = new MessageHeader<string>(Token);
                        System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
                        OperationContext.Current.OutgoingMessageHeaders.Add(authorization);

                        result = objP2PDocumentServiceChannel.UpdateRequisitionApprovalStatusById(requisitionId, (Documents.Entities.DocumentStatus)Enum.Parse(typeof(Documents.Entities.DocumentStatus), approvalStatus));
                        objP2PDocumentServiceChannel.Close();
                    }
                }
                //catch (SMARTFaultException.FaultException<CustomFault>)
                //{
                //    // Log Exception here
                //}
                //catch (GEPCustomException)
                //{
                //    // Log Exception here
                //}
                catch (Exception ex)
                {
                    LogHelper.LogError(Log, "Error occured in  UpdateRequisitionApprovalStatusById Method in RequisitionRestService", ex);
                    throw;
                }
                finally
                {
                    //check and close the channel
                    if (objP2PDocumentServiceChannel != null && objP2PDocumentServiceChannel.State != CommunicationState.Closed)
                    {
                        objP2PDocumentServiceChannel.Close();
                    }
                    else if (objP2PDocumentServiceChannel != null) objP2PDocumentServiceChannel.Abort();
                }
            }
            return result;
        }

        public bool SaveRequisitionApproverDetails(long requisitionId, int approverId, string approvalStatus, string approveUri, string rejectUri, string instanceId)
        {
            #region Debug Logging
            if (Log.IsDebugEnabled)
            {
                Log.Debug(string.Concat("In SaveRequisitionApproverDetails Method in RequisitionRestService ",
                " with parameters: requisitionId = " + requisitionId + ", " +

                "approverId =" + approverId + ", approvalStatus =" + approvalStatus + ", approveUri =" + approveUri + ", rejectUri =" + rejectUri + ", instanceId =" + instanceId));
            }
            #endregion
            var result = false;
            if (requisitionId > 0)
            {
                var objP2PDocumentServiceChannel = GepServiceManager.GetInstance.CreateChannel<IRequisitionServiceChannel>(MultiRegionConfig.GetConfig(CloudConfig.RequisitionServiceURL));
                UserExecutionContext userExecutionContext = P2P.Req.RestService.ExceptionHelper.GetExecutionContext;
                try
                {
                    using (new OperationContextScope(objP2PDocumentServiceChannel))
                    {

                        var objMhg = new MessageHeader<UserExecutionContext>(userExecutionContext);
                        var untypedmh = objMhg.GetUntypedHeader("GepCustomHeader", "Gep.Cumulus");
                        OperationContext.Current.OutgoingMessageHeaders.Add(untypedmh);

                        MessageHeader<string> objMhgAuth = new MessageHeader<string>(Token);
                        System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
                        OperationContext.Current.OutgoingMessageHeaders.Add(authorization);


                        result = objP2PDocumentServiceChannel.SaveRequisitionApproverDetails(requisitionId, approverId, (Documents.Entities.DocumentStatus)Enum.Parse(typeof(Documents.Entities.DocumentStatus), approvalStatus), approveUri, rejectUri, instanceId);
                        objP2PDocumentServiceChannel.Close();
                    }
                }
                //catch (SMARTFaultException.FaultException<CustomFault>)
                //{
                //    // Log Exception here
                //}
                //catch (GEPCustomException)
                //{
                //    // Log Exception here
                //}
                catch (Exception ex)
                {
                    LogHelper.LogError(Log, "Error occured in  SaveRequisitionApproverDetails Method in RequisitionRestService", ex);
                    throw;
                }
                finally
                {
                    //check and close the channel
                    if (objP2PDocumentServiceChannel != null && objP2PDocumentServiceChannel.State != CommunicationState.Closed)
                    {
                        objP2PDocumentServiceChannel.Close();
                    }
                    else if (objP2PDocumentServiceChannel != null) objP2PDocumentServiceChannel.Abort();
                }
            }
            return result;
        }

        public bool SaveRequisitionTrackStatusDetails(string GetPredictedPathResult)
        {
            #region Debug Logging
            if (Log.IsDebugEnabled)
            {
                Log.Debug(string.Concat("In SaveRequisitionTrackStatusDetails Method in RequisitionRestService ",
                " with parameters: GetPredictedPathResult = " + GetPredictedPathResult));
            }
            #endregion

            if (string.IsNullOrEmpty(GetPredictedPathResult))
                return false;
            GetPredictedPathResult = !GetPredictedPathResult.Contains("[") ? "[" + GetPredictedPathResult + "]" : GetPredictedPathResult;
            var javaScriptSerializer = new JavaScriptSerializer();
            List<DocumentTrackStatusDetail> lstTrack = javaScriptSerializer.Deserialize<List<DocumentTrackStatusDetail>>(GetPredictedPathResult);
            var result = false;
            foreach (var objTrack in lstTrack)
            {
                objTrack.StatusDate = objTrack.StatusDate == Convert.ToDateTime("01-01-0001 00:00:00", CultureInfo.InvariantCulture) ? Convert.ToDateTime("01-01-1753 00:00:00", CultureInfo.InvariantCulture) : objTrack.StatusDate;
                if (objTrack.RequisitionId > 0)
                {
                    var objP2PDocumentServiceChannel = GepServiceManager.GetInstance.CreateChannel<IRequisitionServiceChannel>(MultiRegionConfig.GetConfig(CloudConfig.RequisitionServiceURL));
                    UserExecutionContext userExecutionContext = P2P.Req.RestService.ExceptionHelper.GetExecutionContext;
                    try
                    {
                        using (new OperationContextScope(objP2PDocumentServiceChannel))
                        {

                            var objMhg = new MessageHeader<UserExecutionContext>(userExecutionContext);
                            var untypedmh = objMhg.GetUntypedHeader("GepCustomHeader", "Gep.Cumulus");
                            OperationContext.Current.OutgoingMessageHeaders.Add(untypedmh);

                            MessageHeader<string> objMhgAuth = new MessageHeader<string>(Token);
                            System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
                            OperationContext.Current.OutgoingMessageHeaders.Add(authorization);


                            result = objP2PDocumentServiceChannel.SaveRequisitionTrackStatusDetails(objTrack.RequisitionId, objTrack.InstanceID, objTrack.ApproverID, objTrack.ApproverName, objTrack.ApproverType, objTrack.ApproveURL, objTrack.RejectURL, objTrack.StatusDate, (RequisitionTrackStatus)Enum.Parse(typeof(RequisitionTrackStatus), objTrack.ApprovalTrackStatus.ToString()), objTrack.IsDeleted);
                            objP2PDocumentServiceChannel.Close();
                        }
                    }
                    //catch (SMARTFaultException.FaultException<CustomFault>)
                    //{
                    //    // Log Exception here
                    //}
                    //catch (GEPCustomException)
                    //{
                    //    // Log Exception here
                    //}
                    catch (Exception ex)
                    {
                        LogHelper.LogError(Log, "Error occured in  SaveRequisitionTrackStatusDetails Method in RequisitionRestService", ex);
                        throw;
                    }
                    finally
                    {
                        //check and close the channel
                        if (objP2PDocumentServiceChannel != null && objP2PDocumentServiceChannel.State != CommunicationState.Closed)
                        {
                            objP2PDocumentServiceChannel.Close();
                        }
                        else if (objP2PDocumentServiceChannel != null) objP2PDocumentServiceChannel.Abort();
                    }
                }
            }
            return result;
        }

        public string GetAllLineItemsByRequisitionId(long requisitionId, ItemType itemType, int pageIndex, int pageSize, int typeOfUser)
        {
            #region Debug Logging
            if (Log.IsDebugEnabled)
            {
                Log.Debug(string.Concat("In GetAllLineItemsByRequisitionId Method in RequisitionRestService ",
                " with parameters: requisitionId = " + requisitionId + ", " +

                "pageIndex =" + pageIndex + ", pageSize =" + pageSize));
            }
            #endregion
            string strRequisitionItem = string.Empty;
            if (requisitionId > 0)
            {
                var objP2PDocumentServiceChannel = GepServiceManager.GetInstance.CreateChannel<IRequisitionServiceChannel>(MultiRegionConfig.GetConfig(CloudConfig.RequisitionServiceURL));
                UserExecutionContext userExecutionContext = P2P.Req.RestService.ExceptionHelper.GetExecutionContext;
                try
                {
                    using (new OperationContextScope(objP2PDocumentServiceChannel))
                    {

                        var objMhg = new MessageHeader<UserExecutionContext>(userExecutionContext);
                        var untypedmh = objMhg.GetUntypedHeader("GepCustomHeader", "Gep.Cumulus");
                        OperationContext.Current.OutgoingMessageHeaders.Add(untypedmh);

                        MessageHeader<string> objMhgAuth = new MessageHeader<string>(Token);
                        System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
                        OperationContext.Current.OutgoingMessageHeaders.Add(authorization);


                        strRequisitionItem = objP2PDocumentServiceChannel.GetAllLineItemsByRequisitionId(requisitionId, itemType, pageIndex, pageSize, typeOfUser).ToJSON();
                        objP2PDocumentServiceChannel.Close();
                    }
                }
                //catch (SMARTFaultException.FaultException<CustomFault>)
                //{
                //    // Log Exception here
                //}
                //catch (GEPCustomException)
                //{
                //    // Log Exception here
                //}
                catch (CommunicationException commFaultEx)
                {
                    LogHelper.LogError(Log, "Error occured in  GetAllLineItemsByRequisitionId Method in RequisitionRestService", commFaultEx);
                }
                catch (Exception ex)
                {
                    LogHelper.LogError(Log, "Error occured in  GetAllLineItemsByRequisitionId Method in RequisitionRestService", ex);
                    throw;
                }
                finally
                {
                    //check and close the channel
                    if (objP2PDocumentServiceChannel != null && objP2PDocumentServiceChannel.State != CommunicationState.Closed)
                    {
                        objP2PDocumentServiceChannel.Close();
                    }
                    else if (objP2PDocumentServiceChannel != null) objP2PDocumentServiceChannel.Abort();
                }
            }
            return strRequisitionItem;
        }

        //public string DeleteLineItemByIds(string lineItemIds)
        //{
        //    #region Debug Logging
        //    if (Log.IsDebugEnabled)
        //    {
        //        Log.Debug(string.Concat("In DeleteLineItemByIds Method in RequisitionRestService ",
        //        " with parameters: lineItemIds = " + lineItemIds));
        //    }
        //    #endregion
        //    var result = false;
        //    if (!string.IsNullOrWhiteSpace(lineItemIds))
        //    {
        //        var objP2PDocumentServiceChannel = GepServiceManager.GetInstance.CreateChannel<IRequisitionServiceChannel>(MultiRegionConfig.GetConfig(CloudConfig.RequisitionServiceURL));
        //        UserExecutionContext userExecutionContext = P2P.Req.RestService.ExceptionHelper.GetExecutionContext;
        //        try
        //        {
        //            using (new OperationContextScope(objP2PDocumentServiceChannel))
        //            {

        //                var objMhg = new MessageHeader<UserExecutionContext>(userExecutionContext);
        //                var untypedmh = objMhg.GetUntypedHeader("GepCustomHeader", "Gep.Cumulus");
        //                OperationContext.Current.OutgoingMessageHeaders.Add(untypedmh);
        //                result = objP2PDocumentServiceChannel.DeleteLineItemByIds(lineItemIds);
        //                objP2PDocumentServiceChannel.Close();
        //            }
        //        }
        //        //catch (SMARTFaultException.FaultException<CustomFault>)
        //        //{
        //        //    // Log Exception here
        //        //}
        //        //catch (GEPCustomException)
        //        //{
        //        //    // Log Exception here
        //        //}
        //        catch (CommunicationException commFaultEx)
        //        {
        //            LogHelper.LogError(Log, "Error occured in  DeleteLineItemByIds Method in RequisitionRestService", commFaultEx);
        //        }
        //        catch (Exception ex)
        //        {
        //            LogHelper.LogError(Log, "Error occured in  DeleteLineItemByIds Method in RequisitionRestService", ex);
        //            throw;
        //        }
        //        finally
        //        {
        //            //check and close the channel
        //            if (objP2PDocumentServiceChannel != null && objP2PDocumentServiceChannel.State != CommunicationState.Closed)
        //            {
        //                objP2PDocumentServiceChannel.Close();
        //            }
        //            else if (objP2PDocumentServiceChannel != null) objP2PDocumentServiceChannel.Abort();
        //        }
        //    }
        //    return result.ToJSON();
        //}

        public string UpdateItemQuantity(long lineItemId, decimal quantity, int itemSource, int banding, decimal maxOrderQuantity, decimal minOrderQuantity)
        {
            #region Debug Logging
            if (Log.IsDebugEnabled)
            {
                Log.Debug(string.Concat("In UpdateItemQuantity Method in RequisitionRestService ",
                " with parameters: lineItemId = " + lineItemId + ", " +

                "quantity =" + quantity + ", itemSource =" + itemSource + ", " +
                "banding =" + banding + ", maxOrderQuantity =" + maxOrderQuantity + ", minOrderQuantity =" + minOrderQuantity));
            }
            #endregion
            var result = false;
            if (lineItemId > 0 && quantity > 0)
            {
                var objP2PDocumentServiceChannel = GepServiceManager.GetInstance.CreateChannel<IRequisitionServiceChannel>(MultiRegionConfig.GetConfig(CloudConfig.RequisitionServiceURL));
                UserExecutionContext userExecutionContext = P2P.Req.RestService.ExceptionHelper.GetExecutionContext;
                try
                {
                    using (new OperationContextScope(objP2PDocumentServiceChannel))
                    {

                        var objMhg = new MessageHeader<UserExecutionContext>(userExecutionContext);
                        var untypedmh = objMhg.GetUntypedHeader("GepCustomHeader", "Gep.Cumulus");
                        OperationContext.Current.OutgoingMessageHeaders.Add(untypedmh);

                        MessageHeader<string> objMhgAuth = new MessageHeader<string>(Token);
                        System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
                        OperationContext.Current.OutgoingMessageHeaders.Add(authorization);


                        result = objP2PDocumentServiceChannel.UpdateItemQuantity(lineItemId, quantity, itemSource, banding, maxOrderQuantity, minOrderQuantity);
                        objP2PDocumentServiceChannel.Close();
                    }
                }
                //catch (SMARTFaultException.FaultException<CustomFault>)
                //{
                //    // Log Exception here
                //}
                //catch (GEPCustomException)
                //{
                //    // Log Exception here
                //}
                catch (CommunicationException commFaultEx)
                {
                    LogHelper.LogError(Log, "Error occured in  UpdateItemQuantity Method in RequisitionRestService", commFaultEx);
                }
                catch (Exception ex)
                {
                    LogHelper.LogError(Log, "Error occured in  UpdateItemQuantity Method in RequisitionRestService", ex);
                    throw;
                }
                finally
                {
                    //check and close the channel
                    if (objP2PDocumentServiceChannel != null && objP2PDocumentServiceChannel.State != CommunicationState.Closed)
                    {
                        objP2PDocumentServiceChannel.Close();
                    }
                    else if (objP2PDocumentServiceChannel != null) objP2PDocumentServiceChannel.Abort();
                }
            }
            return result.ToJSON();
        }

        #region Save Requisitions
        public string SaveRequisition(Requisition objRequisition)
        {
            #region Debug Logging
            if (Log.IsDebugEnabled)
            {
                Log.Debug(string.Concat("In SaveRequisition Method in RequisitionRestService of class ", typeof(Requisition).ToString(),
                " with parameter: objRequisition = " + objRequisition.ToJSON()));
            }
            #endregion
            var result = string.Empty;
            if (objRequisition != null)
            {
                var objP2PDocumentServiceChannel = GepServiceManager.GetInstance.CreateChannel<IRequisitionServiceChannel>(MultiRegionConfig.GetConfig(CloudConfig.RequisitionServiceURL));
                UserExecutionContext userExecutionContext = P2P.Req.RestService.ExceptionHelper.GetExecutionContext;
                objRequisition.CompanyName = userExecutionContext.ClientName;
                objRequisition.CreatedBy = userExecutionContext.ContactCode;
                try
                {
                    using (new OperationContextScope(objP2PDocumentServiceChannel))
                    {

                        var objMhg = new MessageHeader<UserExecutionContext>(userExecutionContext);
                        var untypedmh = objMhg.GetUntypedHeader("GepCustomHeader", "Gep.Cumulus");
                        OperationContext.Current.OutgoingMessageHeaders.Add(untypedmh);

                        MessageHeader<string> objMhgAuth = new MessageHeader<string>(Token);
                        System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
                        OperationContext.Current.OutgoingMessageHeaders.Add(authorization);


                        result = objP2PDocumentServiceChannel.SaveRequisition(objRequisition, false).ToJSON();
                        objP2PDocumentServiceChannel.Close();
                    }

                }
                //catch (SMARTFaultException.FaultException<CustomFault>)
                //{
                //    // Log Exception here
                //}
                //catch (GEPCustomException)
                //{
                //    // Log Exception here
                //}
                catch (CommunicationException commFaultEx)
                {
                    LogHelper.LogError(Log, "Error occured in  SaveRequisition Method in RequisitionRestService", commFaultEx);
                }
                catch (Exception ex)
                {
                    LogHelper.LogError(Log, "Error occured in  SaveRequisition Method in RequisitionRestService", ex);
                    throw;
                }
                finally
                {
                    //check and close the channel
                    if (objP2PDocumentServiceChannel != null && objP2PDocumentServiceChannel.State != CommunicationState.Closed)
                    {
                        objP2PDocumentServiceChannel.Close();
                    }
                    else if (objP2PDocumentServiceChannel != null) objP2PDocumentServiceChannel.Abort();

                }
            }
            return result.ToJSON();

        }

        public string SaveRequisitionItem(RequisitionItem objRequisitionItem)
        {

            #region Debug Logging
            if (Log.IsDebugEnabled)
            {
                Log.Debug(string.Concat("In SaveRequisitionItem Method in RequisitionRestService  of class ", typeof(RequisitionItem).ToString(),
                " with parameters: objRequisitionItem = " + objRequisitionItem.ToJSON()));
            }
            #endregion
            var result = string.Empty;
            if (objRequisitionItem != null)
            {
                var objP2PDocumentServiceChannel = GepServiceManager.GetInstance.CreateChannel<IRequisitionServiceChannel>(MultiRegionConfig.GetConfig(CloudConfig.RequisitionServiceURL));
                UserExecutionContext userExecutionContext = P2P.Req.RestService.ExceptionHelper.GetExecutionContext;
                try
                {
                    objRequisitionItem.CompanyName = userExecutionContext.ClientName;
                    objRequisitionItem.CreatedBy = userExecutionContext.ContactCode;
                    using (new OperationContextScope(objP2PDocumentServiceChannel))
                    {

                        var objMhg = new MessageHeader<UserExecutionContext>(userExecutionContext);
                        var untypedmh = objMhg.GetUntypedHeader("GepCustomHeader", "Gep.Cumulus");
                        OperationContext.Current.OutgoingMessageHeaders.Add(untypedmh);

                        MessageHeader<string> objMhgAuth = new MessageHeader<string>(Token);
                        System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
                        OperationContext.Current.OutgoingMessageHeaders.Add(authorization);


                        result = objP2PDocumentServiceChannel.SaveRequisitionItem(objRequisitionItem).ToJSON();
                        objP2PDocumentServiceChannel.Close();
                    }

                }
                //catch (SMARTFaultException.FaultException<CustomFault>)
                //{
                //    // Log Exception here
                //}
                //catch (GEPCustomException)
                //{
                //    // Log Exception here
                //}
                catch (CommunicationException commFaultEx)
                {
                    LogHelper.LogError(Log, "Error occured in  SaveRequisitionItem Method in RequisitionRestService", commFaultEx);
                }
                catch (Exception ex)
                {
                    LogHelper.LogError(Log, "Error occured in  SaveRequisitionItem Method in RequisitionRestService", ex);
                    throw;
                }
                finally
                {
                    //check and close the channel
                    if (objP2PDocumentServiceChannel != null && objP2PDocumentServiceChannel.State != CommunicationState.Closed)
                    {
                        objP2PDocumentServiceChannel.Close();
                    }
                    else if (objP2PDocumentServiceChannel != null) objP2PDocumentServiceChannel.Abort();

                }
            }
            return result.ToJSON();

        }

        //public string SaveReqItemWithAdditionalDetails(RequisitionItem objRequisitionItem)
        //{

        //    #region Debug Logging
        //    if (Log.IsDebugEnabled)
        //    {
        //        Log.Debug(string.Concat("In SaveReqItemWithAdditionalDetails Method in RequisitionRestService  of class ", typeof(RequisitionItem).ToString(),
        //        " with parameters: objRequisitionItem = " + objRequisitionItem.ToJSON()));
        //    }
        //    #endregion
        //    var result = string.Empty;
        //    if (objRequisitionItem != null)
        //    {
        //        var objP2PDocumentServiceChannel = GepServiceManager.GetInstance.CreateChannel<IRequisitionServiceChannel>(MultiRegionConfig.GetConfig(CloudConfig.RequisitionServiceURL));
        //        UserExecutionContext userExecutionContext = P2P.Req.RestService.ExceptionHelper.GetExecutionContext;
        //        try
        //        {
        //            objRequisitionItem.CompanyName = userExecutionContext.ClientName;
        //            objRequisitionItem.CreatedBy = userExecutionContext.ContactCode;
        //            using (new OperationContextScope(objP2PDocumentServiceChannel))
        //            {

        //                var objMhg = new MessageHeader<UserExecutionContext>(userExecutionContext);
        //                var untypedmh = objMhg.GetUntypedHeader("GepCustomHeader", "Gep.Cumulus");
        //                OperationContext.Current.OutgoingMessageHeaders.Add(untypedmh);
        //                result = objP2PDocumentServiceChannel.SaveReqItemWithAdditionalDetails(objRequisitionItem, false).ToJSON();
        //                objP2PDocumentServiceChannel.Close();
        //            }

        //        }
        //        //catch (SMARTFaultException.FaultException<CustomFault>)
        //        //{
        //        //    // Log Exception here
        //        //}
        //        //catch (GEPCustomException)
        //        //{
        //        //    // Log Exception here
        //        //}
        //        catch (CommunicationException commFaultEx)
        //        {
        //            LogHelper.LogError(Log, "Error occured in  SaveReqItemWithAdditionalDetails Method in RequisitionRestService", commFaultEx);
        //        }
        //        catch (Exception ex)
        //        {
        //            LogHelper.LogError(Log, "Error occured in  SaveReqItemWithAdditionalDetails Method in RequisitionRestService", ex);
        //            throw;
        //        }
        //        finally
        //        {
        //            //check and close the channel
        //            if (objP2PDocumentServiceChannel != null && objP2PDocumentServiceChannel.State != CommunicationState.Closed)
        //            {
        //                objP2PDocumentServiceChannel.Close();
        //            }
        //            else if (objP2PDocumentServiceChannel != null) objP2PDocumentServiceChannel.Abort();

        //        }
        //    }
        //    return result.ToJSON();

        //}
        #endregion

        //public string GenerateDefaultRequisitionName()
        //{

        //    #region Debug Logging
        //    if (Log.IsDebugEnabled)
        //    {
        //        Log.Debug(string.Concat("In GenerateDefaultRequisitionName Method in RequisitionRestService "));
        //    }
        //    #endregion
        //    var result = string.Empty;

        //    var objP2PDocumentServiceChannel = GepServiceManager.GetInstance.CreateChannel<IRequisitionServiceChannel>(MultiRegionConfig.GetConfig(CloudConfig.RequisitionServiceURL));
        //    UserExecutionContext userExecutionContext = P2P.Req.RestService.ExceptionHelper.GetExecutionContext;
        //    try
        //    {
        //        using (new OperationContextScope(objP2PDocumentServiceChannel))
        //        {

        //            var objMhg = new MessageHeader<UserExecutionContext>(userExecutionContext);
        //            var untypedmh = objMhg.GetUntypedHeader("GepCustomHeader", "Gep.Cumulus");
        //            OperationContext.Current.OutgoingMessageHeaders.Add(untypedmh);
        //            var userId = userExecutionContext.ContactCode;
        //            result = objP2PDocumentServiceChannel.GenerateDefaultRequisitionName(userId, 0).ToJSON();
        //            objP2PDocumentServiceChannel.Close();
        //        }

        //    }
        //    //catch (SMARTFaultException.FaultException<CustomFault>)
        //    //{
        //    //    // Log Exception here
        //    //}
        //    //catch (GEPCustomException)
        //    //{
        //    //    // Log Exception here
        //    //}
        //    catch (CommunicationException commFaultEx)
        //    {
        //        LogHelper.LogError(Log, "Error occured in  GenerateDefaultRequisitionName Method in RequisitionRestService", commFaultEx);
        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.LogError(Log, "Error occured in  GenerateDefaultRequisitionName Method in RequisitionRestService", ex);
        //        throw;
        //    }
        //    finally
        //    {
        //        //check and close the channel
        //        if (objP2PDocumentServiceChannel != null && objP2PDocumentServiceChannel.State != CommunicationState.Closed)
        //        {
        //            objP2PDocumentServiceChannel.Close();
        //        }
        //        else if (objP2PDocumentServiceChannel != null) objP2PDocumentServiceChannel.Abort();

        //    }

        //    return result;

        //}

        public string GetRequisitionBasicDetailsById(long requisitionId)
        {
            #region Debug Logging
            if (Log.IsDebugEnabled)
            {
                Log.Debug(string.Concat("In GetRequisitionBasicDetailsById Method in RequisitionRestService ",
                " with parameters: requisitionId = " + requisitionId));
            }
            #endregion
            var result = string.Empty;

            var objP2PDocumentServiceChannel = GepServiceManager.GetInstance.CreateChannel<IRequisitionServiceChannel>(MultiRegionConfig.GetConfig(CloudConfig.RequisitionServiceURL));
            UserExecutionContext userExecutionContext = P2P.Req.RestService.ExceptionHelper.GetExecutionContext;
            if (requisitionId > 0)
            {
                try
                {
                    using (new OperationContextScope(objP2PDocumentServiceChannel))
                    {

                        var objMhg = new MessageHeader<UserExecutionContext>(userExecutionContext);
                        var untypedmh = objMhg.GetUntypedHeader("GepCustomHeader", "Gep.Cumulus");
                        OperationContext.Current.OutgoingMessageHeaders.Add(untypedmh);
                        var userId = userExecutionContext.ContactCode;

                        MessageHeader<string> objMhgAuth = new MessageHeader<string>(Token);
                        System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
                        OperationContext.Current.OutgoingMessageHeaders.Add(authorization);


                        result = objP2PDocumentServiceChannel.GetRequisitionBasicDetailsById(requisitionId, userId, 0).ToJSON();
                        objP2PDocumentServiceChannel.Close();
                    }

                }
                //catch (SMARTFaultException.FaultException<CustomFault>)
                //{
                //    // Log Exception here
                //}
                //catch (GEPCustomException)
                //{
                //    // Log Exception here
                //}
                catch (CommunicationException commFaultEx)
                {
                    LogHelper.LogError(Log, "Error occured in  GetRequisitionBasicDetailsById Method in RequisitionRestService", commFaultEx);
                }
                catch (Exception ex)
                {
                    LogHelper.LogError(Log, "Error occured in  GetRequisitionBasicDetailsById Method in RequisitionRestService", ex);
                    throw;
                }
                finally
                {
                    //check and close the channel
                    if (objP2PDocumentServiceChannel != null && objP2PDocumentServiceChannel.State != CommunicationState.Closed)
                    {
                        objP2PDocumentServiceChannel.Close();
                    }
                    else if (objP2PDocumentServiceChannel != null) objP2PDocumentServiceChannel.Abort();

                }
            }
            return result;

        }

        public string GetRequisitionLineItemBasicDetails(long requisitionId, P2P.BusinessEntities.ItemType itemType, int startIndex, int pageSize, string sortBy, string sortOrder, int typeOfUser)
        {
            #region Debug Logging
            if (Log.IsDebugEnabled)
            {
                Log.Debug(string.Concat("In GetRequisitionLineItemBasicDetails Method in RequisitionRestService ",
                " with parameters: requisitionId = " + requisitionId + ", " +

                "ItemType =" + itemType.ToString() + ", startIndex =" + startIndex + ", pageSize =" + pageSize + "," +
                "sortBy =" + sortBy + ", sortOrder =" + sortOrder));
            }
            #endregion
            var result = string.Empty;

            var objP2PDocumentServiceChannel = GepServiceManager.GetInstance.CreateChannel<IRequisitionServiceChannel>(MultiRegionConfig.GetConfig(CloudConfig.RequisitionServiceURL));
            UserExecutionContext userExecutionContext = P2P.Req.RestService.ExceptionHelper.GetExecutionContext;
            if (requisitionId > 0)
            {
                try
                {
                    using (new OperationContextScope(objP2PDocumentServiceChannel))
                    {

                        var objMhg = new MessageHeader<UserExecutionContext>(userExecutionContext);
                        var untypedmh = objMhg.GetUntypedHeader("GepCustomHeader", "Gep.Cumulus");
                        OperationContext.Current.OutgoingMessageHeaders.Add(untypedmh);

                        MessageHeader<string> objMhgAuth = new MessageHeader<string>(Token);
                        System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
                        OperationContext.Current.OutgoingMessageHeaders.Add(authorization);


                        result = objP2PDocumentServiceChannel.GetRequisitionLineItemBasicDetails(requisitionId, itemType, startIndex, pageSize, sortBy, sortOrder, typeOfUser).ToJSON();
                        objP2PDocumentServiceChannel.Close();
                    }

                }
                //catch (SMARTFaultException.FaultException<CustomFault>)
                //{
                //    // Log Exception here
                //}
                //catch (GEPCustomException)
                //{
                //    // Log Exception here
                //}
                catch (CommunicationException commFaultEx)
                {
                    LogHelper.LogError(Log, "Error occured in  GetRequisitionBasicDetailsById Method in RequisitionRestService", commFaultEx);
                }
                catch (Exception ex)
                {
                    LogHelper.LogError(Log, "Error occured in  GetRequisitionLineItemBasicDetails Method in RequisitionRestService", ex);
                    throw;
                }
                finally
                {
                    //check and close the channel
                    if (objP2PDocumentServiceChannel != null && objP2PDocumentServiceChannel.State != CommunicationState.Closed)
                    {
                        objP2PDocumentServiceChannel.Close();
                    }
                    else if (objP2PDocumentServiceChannel != null) objP2PDocumentServiceChannel.Abort();

                }
            }
            return result;

        }

        #region Get partner list

        public string GetAllPartnersOfRequisitionById(long requisitionId)
        {
            #region Debug Logging
            if (Log.IsDebugEnabled)
            {
                Log.Debug(string.Concat("In GetAllPartnersOfRequisitionById Method in RequisitionRestService ",
                " with parameters: requisitionId = " + requisitionId));
            }
            #endregion
            var result = string.Empty;

            var objP2PDocumentServiceChannel = GepServiceManager.GetInstance.CreateChannel<IRequisitionServiceChannel>(MultiRegionConfig.GetConfig(CloudConfig.RequisitionServiceURL));
            UserExecutionContext userExecutionContext = P2P.Req.RestService.ExceptionHelper.GetExecutionContext;
            if (requisitionId > 0)
            {
                try
                {
                    using (new OperationContextScope(objP2PDocumentServiceChannel))
                    {

                        var objMhg = new MessageHeader<UserExecutionContext>(userExecutionContext);
                        var untypedmh = objMhg.GetUntypedHeader("GepCustomHeader", "Gep.Cumulus");
                        OperationContext.Current.OutgoingMessageHeaders.Add(untypedmh);

                        MessageHeader<string> objMhgAuth = new MessageHeader<string>(Token);
                        System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
                        OperationContext.Current.OutgoingMessageHeaders.Add(authorization);


                        result = objP2PDocumentServiceChannel.GetAllPartnersOfRequisitionById(requisitionId, "", "").ToJSON();
                        objP2PDocumentServiceChannel.Close();
                    }

                }
                //catch (SMARTFaultException.FaultException<CustomFault>)
                //{
                //    // Log Exception here
                //}
                //catch (GEPCustomException ex)
                //{
                //    // Log Exception here
                //    // Log Exception here
                //    var objCustomFault = new CustomFault(ex.Message, "GetAllPartnersOfRequisitionById", "GetAllPartnersOfRequisitionById",
                //                                      "Requisition", ExceptionType.ApplicationException, string.Empty, false);
                //    throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while Getting All Partners for requisition :" + requisitionId.ToString(CultureInfo.InvariantCulture));

                //}
                catch (Exception ex)
                {
                    LogHelper.LogError(Log, "Error occured in  GetAllPartnersOfRequisitionById Method in RequisitionRestService", ex);
                    // Log Exception here
                    var objCustomFault = new CustomFault(ex.Message, "GetAllPartnersOfRequisitionById", "GetAllPartnersOfRequisitionById",
                                                      "Requisition", ExceptionType.ApplicationException, string.Empty, false);
                    throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while Getting All Partners for requisition :" + requisitionId.ToString(CultureInfo.InvariantCulture));
                }
                finally
                {
                    //check and close the channel
                    if (objP2PDocumentServiceChannel != null && objP2PDocumentServiceChannel.State != CommunicationState.Closed)
                    {
                        objP2PDocumentServiceChannel.Close();
                    }
                    else if (objP2PDocumentServiceChannel != null) objP2PDocumentServiceChannel.Abort();

                }
            }
            return result;

        }

        #endregion

        #region Get/Save Line Item Details

        public string GetPartnerDetailsByLiId(long lineItemId)
        {

            #region Debug Logging
            if (Log.IsDebugEnabled)
            {
                Log.Debug(string.Concat("In GetPartnerDetailsByLiId Method in RequisitionRestService ",
                " with parameters: lineItemId = " + lineItemId));
            }
            #endregion
            var result = string.Empty;

            var objP2PDocumentServiceChannel = GepServiceManager.GetInstance.CreateChannel<IRequisitionServiceChannel>(MultiRegionConfig.GetConfig(CloudConfig.RequisitionServiceURL));
            UserExecutionContext userExecutionContext = P2P.Req.RestService.ExceptionHelper.GetExecutionContext;
            if (lineItemId > 0)
            {
                try
                {
                    using (new OperationContextScope(objP2PDocumentServiceChannel))
                    {

                        var objMhg = new MessageHeader<UserExecutionContext>(userExecutionContext);
                        var untypedmh = objMhg.GetUntypedHeader("GepCustomHeader", "Gep.Cumulus");
                        OperationContext.Current.OutgoingMessageHeaders.Add(untypedmh);

                        MessageHeader<string> objMhgAuth = new MessageHeader<string>(Token);
                        System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
                        OperationContext.Current.OutgoingMessageHeaders.Add(authorization);


                        result = objP2PDocumentServiceChannel.GetPartnerDetailsByLiId(lineItemId).ToJSON();
                        objP2PDocumentServiceChannel.Close();
                    }

                }
                //catch (SMARTFaultException.FaultException<CustomFault>)
                //{
                //    // Log Exception here
                //}
                //catch (GEPCustomException ex)
                //{
                //    // Log Exception here
                //    var objCustomFault = new CustomFault(ex.Message, "GetPartnerDetailsByLiId", "GetPartnerDetailsByLiId",
                //                                     "Requisition", ExceptionType.ApplicationException, string.Empty, false);
                //    throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while Getting Partner details for requisition line item :" + lineItemId.ToString(CultureInfo.InvariantCulture));
                //}
                catch (Exception ex)
                {
                    LogHelper.LogError(Log, "Error occured in  GetPartnerDetailsByLiId Method in RequisitionRestService", ex);
                    // Log Exception here
                    var objCustomFault = new CustomFault(ex.Message, "GetPartnerDetailsByLiId", "GetPartnerDetailsByLiId",
                                                      "Requisition", ExceptionType.ApplicationException, string.Empty, false);
                    throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while Getting Partner Details By LiId for requisition liid:" + lineItemId.ToString(CultureInfo.InvariantCulture));
                }
                finally
                {
                    //check and close the channel
                    if (objP2PDocumentServiceChannel != null && objP2PDocumentServiceChannel.State != CommunicationState.Closed)
                    {
                        objP2PDocumentServiceChannel.Close();
                    }
                    else if (objP2PDocumentServiceChannel != null) objP2PDocumentServiceChannel.Abort();

                }
            }
            return result;
        }

        public string GetShippingSplitDetailsByLiId(long lineItemId)
        {
            #region Debug Logging
            if (Log.IsDebugEnabled)
            {
                Log.Debug(string.Concat("In GetShippingSplitDetailsByLiId Method in RequisitionRestService ",
                " with parameters: lineItemId = " + lineItemId));
            }
            #endregion
            var result = string.Empty;

            var objP2PDocumentServiceChannel = GepServiceManager.GetInstance.CreateChannel<IRequisitionServiceChannel>(MultiRegionConfig.GetConfig(CloudConfig.RequisitionServiceURL));
            UserExecutionContext userExecutionContext = P2P.Req.RestService.ExceptionHelper.GetExecutionContext;
            if (lineItemId > 0)
            {
                try
                {
                    using (new OperationContextScope(objP2PDocumentServiceChannel))
                    {

                        var objMhg = new MessageHeader<UserExecutionContext>(userExecutionContext);
                        var untypedmh = objMhg.GetUntypedHeader("GepCustomHeader", "Gep.Cumulus");
                        OperationContext.Current.OutgoingMessageHeaders.Add(untypedmh);

                        MessageHeader<string> objMhgAuth = new MessageHeader<string>(Token);
                        System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
                        OperationContext.Current.OutgoingMessageHeaders.Add(authorization);


                        result = objP2PDocumentServiceChannel.GetShippingSplitDetailsByLiId(lineItemId).ToJSON();
                        objP2PDocumentServiceChannel.Close();
                    }

                }
                //catch (SMARTFaultException.FaultException<CustomFault>)
                //{
                //    // Log Exception here
                //}
                //catch (GEPCustomException ex)
                //{
                //    // Log Exception here
                //    var objCustomFault = new CustomFault(ex.Message, "GetShippingSplitDetailsByLiId", "GetShippingSplitDetailsByLiId",
                //                                     "Requisition", ExceptionType.ApplicationException, string.Empty, false);
                //    throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while Getting Shipping Split Details By Line item Id for requisition, liid :" + lineItemId.ToString(CultureInfo.InvariantCulture));
                //}
                catch (Exception ex)
                {
                    LogHelper.LogError(Log, "Error occured in  GetShippingSplitDetailsByLiId Method in RequisitionRestService", ex);
                    // Log Exception here
                    var objCustomFault = new CustomFault(ex.Message, "GetShippingSplitDetailsByLiId", "GetShippingSplitDetailsByLiId",
                                                      "Requisition", ExceptionType.ApplicationException, string.Empty, false);
                    throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while Getting Shipping Split Details By LiId for requisition, liid :" + lineItemId.ToString(CultureInfo.InvariantCulture));
                }
                finally
                {
                    //check and close the channel
                    if (objP2PDocumentServiceChannel != null && objP2PDocumentServiceChannel.State != CommunicationState.Closed)
                    {
                        objP2PDocumentServiceChannel.Close();
                    }
                    else if (objP2PDocumentServiceChannel != null) objP2PDocumentServiceChannel.Abort();

                }
            }
            return result;
        }

        public string GetOtherItemDetailsByLiId(long lineItemId)
        {
            #region Debug Logging
            if (Log.IsDebugEnabled)
            {
                Log.Debug(string.Concat("In GetOtherItemDetailsByLiId Method in RequisitionRestService ",
                " with parameters: lineItemId = " + lineItemId));
            }
            #endregion
            var result = string.Empty;

            var objP2PDocumentServiceChannel = GepServiceManager.GetInstance.CreateChannel<IRequisitionServiceChannel>(MultiRegionConfig.GetConfig(CloudConfig.RequisitionServiceURL));
            UserExecutionContext userExecutionContext = P2P.Req.RestService.ExceptionHelper.GetExecutionContext;
            if (lineItemId > 0)
            {
                try
                {
                    using (new OperationContextScope(objP2PDocumentServiceChannel))
                    {

                        var objMhg = new MessageHeader<UserExecutionContext>(userExecutionContext);
                        var untypedmh = objMhg.GetUntypedHeader("GepCustomHeader", "Gep.Cumulus");
                        OperationContext.Current.OutgoingMessageHeaders.Add(untypedmh);

                        MessageHeader<string> objMhgAuth = new MessageHeader<string>(Token);
                        System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
                        OperationContext.Current.OutgoingMessageHeaders.Add(authorization);


                        result = objP2PDocumentServiceChannel.GetOtherItemDetailsByLiId(lineItemId).ToJSON();
                        objP2PDocumentServiceChannel.Close();
                    }

                }
                //catch (SMARTFaultException.FaultException<CustomFault>)
                //{
                //    // Log Exception here
                //}
                //catch (GEPCustomException ex)
                //{
                //    // Log Exception here
                //    var objCustomFault = new CustomFault(ex.Message, "GetOtherItemDetailsByLiId", "GetOtherItemDetailsByLiId",
                //                                     "Requisition", ExceptionType.ApplicationException, string.Empty, false);
                //    throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while Getting other details by line item id for the requisition, liid :" + lineItemId.ToString(CultureInfo.InvariantCulture));
                //}
                catch (Exception ex)
                {
                    LogHelper.LogError(Log, "Error occured in  GetOtherItemDetailsByLiId Method in RequisitionRestService", ex);
                    // Log Exception here
                    var objCustomFault = new CustomFault(ex.Message, "GetOtherItemDetailsByLiId", "GetOtherItemDetailsByLiId",
                                                      "Requisition", ExceptionType.ApplicationException, string.Empty, false);
                    throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while Getting Other Item Details By LiId for requisition, liid :" + lineItemId.ToString(CultureInfo.InvariantCulture));
                }
                finally
                {
                    //check and close the channel
                    if (objP2PDocumentServiceChannel != null && objP2PDocumentServiceChannel.State != CommunicationState.Closed)
                    {
                        objP2PDocumentServiceChannel.Close();
                    }
                    else if (objP2PDocumentServiceChannel != null) objP2PDocumentServiceChannel.Abort();

                }
            }
            return result;
        }

        public string SaveRequisitionItemPartnerDetails(RequisitionItem objRequisitionItem)
        {
            #region Debug Logging
            if (Log.IsDebugEnabled)
            {
                Log.Debug(string.Concat("In SaveRequisitionItemPartnerDetails Method in RequisitionRestService  of class ", typeof(RequisitionItem).ToString(),
                " with parameters: objRequisitionItem = " + objRequisitionItem.ToJSON()));
            }
            #endregion
            var result = Convert.ToInt64(0);

            var objP2PDocumentServiceChannel = GepServiceManager.GetInstance.CreateChannel<IRequisitionServiceChannel>(MultiRegionConfig.GetConfig(CloudConfig.RequisitionServiceURL));
            UserExecutionContext userExecutionContext = P2P.Req.RestService.ExceptionHelper.GetExecutionContext;
            if (objRequisitionItem != null)
            {
                try
                {
                    using (new OperationContextScope(objP2PDocumentServiceChannel))
                    {

                        var objMhg = new MessageHeader<UserExecutionContext>(userExecutionContext);
                        var untypedmh = objMhg.GetUntypedHeader("GepCustomHeader", "Gep.Cumulus");
                        OperationContext.Current.OutgoingMessageHeaders.Add(untypedmh);

                        MessageHeader<string> objMhgAuth = new MessageHeader<string>(Token);
                        System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
                        OperationContext.Current.OutgoingMessageHeaders.Add(authorization);


                        result = objP2PDocumentServiceChannel.SaveRequisitionItemPartnerDetails(objRequisitionItem);
                        objP2PDocumentServiceChannel.Close();
                    }

                }
                //catch (SMARTFaultException.FaultException<CustomFault>)
                //{
                //    // Log Exception here
                //}
                //catch (GEPCustomException ex)
                //{
                //    // Log Exception here
                //    var objCustomFault = new CustomFault(ex.Message, "SaveRequisitionItemPartnerDetails", "SaveRequisitionItemPartnerDetails",
                //                                      "Requisition", ExceptionType.ApplicationException, string.Empty, false);
                //    throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while Saving Line Item partner details for requisition ");
                //}
                catch (Exception ex)
                {
                    LogHelper.LogError(Log, "Error occured in  SaveRequisitionItemPartnerDetails Method in RequisitionRestService", ex);
                    // Log Exception here
                    var objCustomFault = new CustomFault(ex.Message, "SaveRequisitionItemPartnerDetails", "SaveRequisitionItemPartnerDetails",
                                                      "Requisition", ExceptionType.ApplicationException, string.Empty, false);
                    throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while Saving Item Partner Details for requisition");
                }
                finally
                {
                    //check and close the channel
                    if (objP2PDocumentServiceChannel != null && objP2PDocumentServiceChannel.State != CommunicationState.Closed)
                    {
                        objP2PDocumentServiceChannel.Close();
                    }
                    else if (objP2PDocumentServiceChannel != null) objP2PDocumentServiceChannel.Abort();

                }
            }
            return result.ToJSON();
        }

        public string SaveRequisitionItemShippingDetails(long reqLineItemShippingId, long requisitionItemId, string shippingMethod, int shiptoLocationId, int delivertoLocationId, decimal quantity, decimal totalQuantity, bool prorateLineItemTax)
        {
            #region Debug Logging
            if (Log.IsDebugEnabled)
            {
                Log.Debug(string.Concat("In SaveRequisitionItemShippingDetails Method in RequisitionRestService ",
                " with parameters: reqLineItemShippingId = " + reqLineItemShippingId + ", " +

                "requisitionItemId =" + requisitionItemId + ", shippingMethod =" + shippingMethod + ", " +
                " shiptoLocationId =" + shiptoLocationId + ", quantity =" + quantity + ", totalQuantity =" + totalQuantity));
            }
            #endregion
            var result = Convert.ToInt64(0);
            UserExecutionContext userExecutionContext = P2P.Req.RestService.ExceptionHelper.GetExecutionContext;
            if (requisitionItemId > 0)
            {
                try
                {
                    ProxyP2PCommonService proxyP2PCommonService = new ProxyP2PCommonService(userExecutionContext, Token);
                    ProxyRequisitionService proxyRequisitionService = new ProxyRequisitionService(userExecutionContext, Token);

                    var userId = userExecutionContext.ContactCode;

                    string keyValue = proxyP2PCommonService.GetSettingsValueByKey(P2PDocumentType.None, "IsOperationalBudgetEnabled", userExecutionContext.ContactCode, 107);
                    int precessionValue = Convert.ToInt16(proxyP2PCommonService.GetSettingsValueByKey(P2PDocumentType.None, "MaxPrecessionValue", userExecutionContext.ContactCode, (int)SubAppCodes.P2P));
                    int maxPrecessionforTotal = Convert.ToInt16(proxyP2PCommonService.GetSettingsValueByKey(P2PDocumentType.None, "MaxPrecessionValueforTotal", userExecutionContext.ContactCode, (int)SubAppCodes.P2P));
                    int maxPrecessionForTaxesAndCharges = Convert.ToInt16(proxyP2PCommonService.GetSettingsValueByKey(P2PDocumentType.None, "MaxPrecessionValueForTaxesAndCharges", userExecutionContext.ContactCode, (int)SubAppCodes.P2P));

                    result = proxyRequisitionService.SaveRequisitionItemShippingDetails(reqLineItemShippingId, requisitionItemId, shippingMethod, shiptoLocationId, delivertoLocationId, quantity, totalQuantity, userId, precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges, prorateLineItemTax);
                }
                catch (Exception ex)
                {
                    LogHelper.LogError(Log, "Error occured in  SaveRequisitionItemShippingDetails Method in RequisitionRestService", ex);
                    // Log Exception here
                    var objCustomFault = new CustomFault(ex.Message, "SaveRequisitionItemShippingDetails", "SaveRequisitionItemShippingDetails",
                                                      "Requisition", ExceptionType.ApplicationException, string.Empty, false);
                    throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while Saving Item Shipping Details for requisition ");
                }
            }
            return result.ToJSON();
        }

        //public string SaveRequisitionItemOtherDetails(RequisitionItem objRequisitionItem)
        //{
        //    #region Debug Logging
        //    if (Log.IsDebugEnabled)
        //    {
        //        Log.Debug(string.Concat("In SaveRequisitionItemOtherDetails Method in RequisitionRestService  of class ", typeof(RequisitionItem).ToString(),
        //        " with parameters: objRequisitionItem = " + objRequisitionItem.ToJSON()));
        //    }
        //    #endregion
        //    var result = Convert.ToInt64(0);

        //    var objP2PDocumentServiceChannel = GepServiceManager.GetInstance.CreateChannel<IRequisitionServiceChannel>(MultiRegionConfig.GetConfig(CloudConfig.RequisitionServiceURL));
        //    UserExecutionContext userExecutionContext = P2P.Req.RestService.ExceptionHelper.GetExecutionContext;
        //    if (objRequisitionItem != null)
        //    {
        //        try
        //        {
        //            using (new OperationContextScope(objP2PDocumentServiceChannel))
        //            {

        //                var objMhg = new MessageHeader<UserExecutionContext>(userExecutionContext);
        //                var untypedmh = objMhg.GetUntypedHeader("GepCustomHeader", "Gep.Cumulus");
        //                OperationContext.Current.OutgoingMessageHeaders.Add(untypedmh);
        //                result = objP2PDocumentServiceChannel.SaveRequisitionItemOtherDetails(objRequisitionItem);
        //                objP2PDocumentServiceChannel.Close();
        //            }

        //        }
        //        //catch (SMARTFaultException.FaultException<CustomFault>)
        //        //{
        //        //    // Log Exception here
        //        //}
        //        //catch (GEPCustomException ex)
        //        //{
        //        //    // Log Exception here
        //        //    var objCustomFault = new CustomFault(ex.Message, "SaveRequisitionItemOtherDetails", "SaveRequisitionItemOtherDetails",
        //        //                                      "Requisition", ExceptionType.ApplicationException, string.Empty, false);
        //        //    throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while Saving Requisition Item OtherDetails" );
        //        //}
        //        catch (Exception ex)
        //        {
        //            LogHelper.LogError(Log, "Error occured in  SaveRequisitionItemOtherDetails Method in RequisitionRestService", ex);
        //            // Log Exception here
        //            var objCustomFault = new CustomFault(ex.Message, "SaveRequisitionItemOtherDetails", "SaveRequisitionItemOtherDetails",
        //                                              "Requisition", ExceptionType.ApplicationException, string.Empty, false);
        //            throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while Saving Item Other Details for requisition");
        //        }
        //        finally
        //        {
        //            //check and close the channel
        //            if (objP2PDocumentServiceChannel != null && objP2PDocumentServiceChannel.State != CommunicationState.Closed)
        //            {
        //                objP2PDocumentServiceChannel.Close();
        //            }
        //            else if (objP2PDocumentServiceChannel != null) objP2PDocumentServiceChannel.Abort();

        //        }
        //    }
        //    return result.ToJSON();
        //}

        #endregion

        public string GetTrackDetailsofRequisitionById(long requisitionId)
        {
            #region Debug Logging
            if (Log.IsDebugEnabled)
            {
                Log.Debug(string.Concat("In SaveOrderFromRequisition Method in RequisitionRestService ",
                " with parameters: requisitionId = " + requisitionId));
            }
            #endregion
            string strResult = string.Empty;

            var objP2PDocumentServiceChannel = GepServiceManager.GetInstance.CreateChannel<IRequisitionServiceChannel>(MultiRegionConfig.GetConfig(CloudConfig.RequisitionServiceURL));
            UserExecutionContext userExecutionContext = P2P.Req.RestService.ExceptionHelper.GetExecutionContext;

            if (requisitionId > 0)
            {
                try
                {
                    using (new OperationContextScope(objP2PDocumentServiceChannel))
                    {

                        var objMhg = new MessageHeader<UserExecutionContext>(userExecutionContext);
                        var untypedmh = objMhg.GetUntypedHeader("GepCustomHeader", "Gep.Cumulus");
                        OperationContext.Current.OutgoingMessageHeaders.Add(untypedmh);

                        MessageHeader<string> objMhgAuth = new MessageHeader<string>(Token);
                        System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
                        OperationContext.Current.OutgoingMessageHeaders.Add(authorization);



                        strResult = objP2PDocumentServiceChannel.GetTrackDetailsofRequisitionById(requisitionId).ToJSON();
                        objP2PDocumentServiceChannel.Close();
                    }

                }
                //catch (SMARTFaultException.FaultException<CustomFault>)
                //{
                //    // Log Exception here
                //}
                //catch (GEPCustomException ex)
                //{
                //    // Log Exception here
                //    var objCustomFault = new CustomFault(ex.Message, "GetTrackDetailsofRequisitionById", "GetTrackDetailsofRequisitionById",
                //                                      "Requisition", ExceptionType.ApplicationException, string.Empty, false);
                //    throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while retreiving Track Status Details for requisition");
                //}
                catch (Exception ex)
                {
                    LogHelper.LogError(Log, "Error occured in  SaveOrderFromRequisition Method in RequisitionRestService", ex);
                    // Log Exception here
                    var objCustomFault = new CustomFault(ex.Message, "GetTrackDetailsofRequisitionById", "GetTrackDetailsofRequisitionById",
                                                      "Requisition", ExceptionType.ApplicationException, string.Empty, false);
                    throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while retreiving Track Status Details for requisition");
                }
                finally
                {
                    //check and close the channel
                    if (objP2PDocumentServiceChannel != null && objP2PDocumentServiceChannel.State != CommunicationState.Closed)
                    {
                        objP2PDocumentServiceChannel.Close();
                    }
                    else if (objP2PDocumentServiceChannel != null) objP2PDocumentServiceChannel.Abort();

                }
            }
            return strResult;
        }

        public bool ApprovalFinalCallBackMethod(long documentCode, string documentStatus)
        {
            #region Debug Logging
            if (Log.IsDebugEnabled)
            {
                Log.Debug(string.Concat("In ApprovalFinalCallBackMethod Method in RequisitionRestService of class ", typeof(Requisition).ToString(),
                " with parameter: documentCode = " + documentCode + ", documentStatus=" + documentStatus));
            }
            #endregion
            var result = false;
            if (documentCode > 0 && !string.IsNullOrEmpty(documentStatus) && documentStatus == DocumentStatus.Approved.ToString())
            {
                try
                {
                    UserExecutionContext userExecutionContext = P2P.Req.RestService.ExceptionHelper.GetExecutionContext;
                    ProxyRequisitionService proxyRequisitionService = new ProxyRequisitionService(userExecutionContext, Token);
                    if (proxyRequisitionService != null)
                    {
                        result = proxyRequisitionService.SaveDocumentBusinessUnit(documentCode);
                        proxyRequisitionService.AutoCreateOrder(documentCode);
                    }
                }
                catch (CommunicationException commFaultEx)
                {
                    LogHelper.LogError(Log, "Error occured in ApprovalFinalCallBackMethod Method in RequisitionRestService", commFaultEx);
                }
                catch (Exception ex)
                {
                    LogHelper.LogError(Log, "Error occured in ApprovalFinalCallBackMethod Method in RequisitionRestService", ex);
                    throw;
                }
            }
            return result;
        }

        public bool SaveRequisitionBusinessUnit(long documentCode)
        {
            #region Debug Logging
            if (Log.IsDebugEnabled)
            {
                Log.Debug(string.Concat("In SaveRequisitionBusinessUnit Method in RequisitionRestService ",
                " with parameters: documentCode =" + documentCode));
            }
            #endregion
            var objP2PDocumentServiceChannel = GepServiceManager.GetInstance.CreateChannel<IRequisitionServiceChannel>(MultiRegionConfig.GetConfig(CloudConfig.RequisitionServiceURL));
            var flag = false;
            UserExecutionContext userExecutionContext = P2P.Req.RestService.ExceptionHelper.GetExecutionContext;
            UserIdentityContext objUserIdentityContext = new UserIdentityContext(userExecutionContext);
            int acEntityId = 0;
            long serveLOBId = userExecutionContext.ServingEntityDetailCode;
            IPartnerServiceChannel objPartnerServiceChannel = null;
            try
            {
                if (serveLOBId <= 0)
                {
                    UserContext userContext = null;
                    objPartnerServiceChannel = GepServiceManager.GetInstance.CreateChannel<IPartnerServiceChannel>(MultiRegionConfig.GetConfig(CloudConfig.PartnerServiceURL));

                    MessageHeader<string> objMhgAuth = new MessageHeader<string>(Token);
                    System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
                    OperationContext.Current.OutgoingMessageHeaders.Add(authorization);

                    userContext = objPartnerServiceChannel.GetUserContextDetailsByContactCode(userExecutionContext.ContactCode);
                    serveLOBId = userContext.GetDefaultBelongingUserLOBMapping().EntityDetailCode;
                }
                acEntityId = userExecutionContext.GetAccessControlEntityId(serveLOBId);
            }
            catch (Exception ex)
            {

                LogHelper.LogError(Log, "Error occured in SaveRequisitionBusinessUnit method in OrderRestService", ex);

                var objCustomFault = new CustomFault(ex.Message, "SaveRequisitionBusinessUnit",
                                                "SaveRequisitionBusinessUnit",
                                                typeof(Requisition).ToString(),
                                                ExceptionType.ApplicationException,
                                                documentCode.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while saving BU for requisition  " +
                                                      documentCode.ToString(CultureInfo.InvariantCulture));
            }
            finally
            {
                //check and close the channel
                if (objPartnerServiceChannel != null && objPartnerServiceChannel.State != CommunicationState.Closed)
                {
                    objPartnerServiceChannel.Close();
                }
                else if (objPartnerServiceChannel != null) objPartnerServiceChannel.Abort();
            }

            try
            {
                var lstBUDetails = objUserIdentityContext.BUByContactCode(userExecutionContext.ContactCode, acEntityId);

                List<DocumentBU> lstBU = new List<DocumentBU>();
                lstBU.AddRange(lstBUDetails.Select(data => new DocumentBU() { BusinessUnitCode = data.OrgEntityCode, BusinessUnitName = data.EntityDescription }));
                using (new OperationContextScope(objP2PDocumentServiceChannel))
                {
                    var objMhg = new MessageHeader<UserExecutionContext>(userExecutionContext);
                    var untypedmh = objMhg.GetUntypedHeader("GepCustomHeader", "Gep.Cumulus");
                    OperationContext.Current.OutgoingMessageHeaders.Add(untypedmh);

                    MessageHeader<string> objMhgAuth = new MessageHeader<string>(Token);
                    System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
                    OperationContext.Current.OutgoingMessageHeaders.Add(authorization);




                    flag = objP2PDocumentServiceChannel.SaveRequisitionBusinessUnit(documentCode, lstBUDetails.Where(data => data.IsDefault).FirstOrDefault().OrgEntityCode);

                    objP2PDocumentServiceChannel.Close();
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in  SaveRequisitionBusinessUnit Method in RequisitionRestService", ex);
                // Log Exception here
                var objCustomFault = new CustomFault(ex.Message, "SaveRequisitionBusinessUnit", "SaveRequisitionBusinessUnit",
                                                  "Requisition", ExceptionType.ApplicationException, string.Empty, false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while SaveRequisitionBusinessUnit By DocumentCode  for requisition");
            }
            finally
            {
                //check and close the channel
                if (objP2PDocumentServiceChannel != null && objP2PDocumentServiceChannel.State != CommunicationState.Closed)
                {
                    objP2PDocumentServiceChannel.Close();
                }
                else if (objP2PDocumentServiceChannel != null) objP2PDocumentServiceChannel.Abort();

            }


            return flag;
        }

        public string ValidateDocumentByDocumentCode(long documentCode)
        {
            #region Debug Logging
            if (Log.IsDebugEnabled)
            {
                Log.Debug(string.Concat("In ValidateDocumentByDocumentCode Method in RequisitionRestService ",
                " with parameters: requisitionId = " + documentCode));
            }
            #endregion
            string strResult = string.Empty;
            if (documentCode > 0)
            {
                try
                {
                    UserExecutionContext userExecutionContext = P2P.Req.RestService.ExceptionHelper.GetExecutionContext;
                    ProxyRequisitionService proxyRequisitionService = new ProxyRequisitionService(userExecutionContext, Token);
                    strResult = proxyRequisitionService.GetRequisitionValidationDetailsById(documentCode).ToJSON();
                }

                catch (Exception ex)
                {
                    LogHelper.LogError(Log, "Error occured in  ValidateDocumentByDocumentCode Method in RequisitionRestService", ex);
                    // Log Exception here
                    var objCustomFault = new CustomFault(ex.Message, "ValidateDocumentByDocumentCode", "ValidateDocumentByDocumentCode",
                                                      "Requisition", ExceptionType.ApplicationException, string.Empty, false);
                    throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while Validate Document By DocumentCode  for requisition");
                }
            }
            return strResult;
        }

        public string GetAllSplitAccountingFields(P2PDocumentType docType, LevelType levelType)
        {
            #region Debug Logging
            if (Log.IsDebugEnabled)
            {
                Log.Debug(string.Concat("In GetAllSplitAccountingFields Method in RequisitionRestService ",
                " with parameters: docType = " + (int)docType));
            }
            #endregion
            string strResult = string.Empty;

            var objP2PDocumentServiceChannel = GepServiceManager.GetInstance.CreateChannel<IRequisitionServiceChannel>(MultiRegionConfig.GetConfig(CloudConfig.RequisitionServiceURL));
            UserExecutionContext userExecutionContext = P2P.Req.RestService.ExceptionHelper.GetExecutionContext;


            try
            {
                using (new OperationContextScope(objP2PDocumentServiceChannel))
                {

                    var objMhg = new MessageHeader<UserExecutionContext>(userExecutionContext);
                    var untypedmh = objMhg.GetUntypedHeader("GepCustomHeader", "Gep.Cumulus");
                    OperationContext.Current.OutgoingMessageHeaders.Add(untypedmh);

                    MessageHeader<string> objMhgAuth = new MessageHeader<string>(Token);
                    System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
                    OperationContext.Current.OutgoingMessageHeaders.Add(authorization);



                    strResult = objP2PDocumentServiceChannel.GetAllSplitAccountingFields(docType, levelType).ToJSON();
                    objP2PDocumentServiceChannel.Close();
                }

            }

            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in  GetAllSplitAccountingFields Method in RequisitionRestService", ex);
                // Log Exception here
                var objCustomFault = new CustomFault(ex.Message, "GetAllSplitAccountingFields", "GetAllSplitAccountingFields",
                                                  "Requisition", ExceptionType.ApplicationException, string.Empty, false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while Get All Split Accounting Fields  for requisition");
            }
            finally
            {
                //check and close the channel
                if (objP2PDocumentServiceChannel != null && objP2PDocumentServiceChannel.State != CommunicationState.Closed)
                {
                    objP2PDocumentServiceChannel.Close();
                }
                else if (objP2PDocumentServiceChannel != null) objP2PDocumentServiceChannel.Abort();

            }

            return strResult;
        }

        //public string GetRequisitionAccountingDetailsByItemId(long requisitionItemId, int pageIndex, int pageSize)
        //{
        //    #region Debug Logging
        //    if (Log.IsDebugEnabled)
        //    {
        //        Log.Debug(string.Concat("In GetRequisitionAccountingDetailsByItemId Method in RequisitionRestService ",
        //        " with parameters: requisitionItemId = " + requisitionItemId + "pageIndex = " + pageIndex + "pageSize = " + pageSize));
        //    }
        //    #endregion
        //    string strResult = string.Empty;

        //    var objP2PDocumentServiceChannel = GepServiceManager.GetInstance.CreateChannel<IRequisitionServiceChannel>(MultiRegionConfig.GetConfig(CloudConfig.RequisitionServiceURL));
        //    UserExecutionContext userExecutionContext = P2P.Req.RestService.ExceptionHelper.GetExecutionContext;


        //    try
        //    {
        //        using (new OperationContextScope(objP2PDocumentServiceChannel))
        //        {

        //            var objMhg = new MessageHeader<UserExecutionContext>(userExecutionContext);
        //            var untypedmh = objMhg.GetUntypedHeader("GepCustomHeader", "Gep.Cumulus");
        //            OperationContext.Current.OutgoingMessageHeaders.Add(untypedmh);
        //            strResult = objP2PDocumentServiceChannel.GetRequisitionAccountingDetailsByItemId(requisitionItemId, pageIndex, pageSize).ToJSON();
        //            objP2PDocumentServiceChannel.Close();
        //        }

        //    }

        //    catch (Exception ex)
        //    {
        //        LogHelper.LogError(Log, "Error occured in  GetRequisitionAccountingDetailsByItemId Method in RequisitionRestService", ex);
        //        // Log Exception here
        //        var objCustomFault = new CustomFault(ex.Message, "GetRequisitionAccountingDetailsByItemId", "GetRequisitionAccountingDetailsByItemId",
        //                                          "Requisition", ExceptionType.ApplicationException, string.Empty, false);
        //        throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while GetRequisitionAccountingDetailsByItemId  for requisition");
        //    }
        //    finally
        //    {
        //        //check and close the channel
        //        if (objP2PDocumentServiceChannel != null && objP2PDocumentServiceChannel.State != CommunicationState.Closed)
        //        {
        //            objP2PDocumentServiceChannel.Close();
        //        }
        //        else if (objP2PDocumentServiceChannel != null) objP2PDocumentServiceChannel.Abort();

        //    }

        //    return strResult;
        //}

        //public string SaveRequisitionAccountingDetails(List<RequisitionSplitItems> requisitionSplitItems, List<DocumentSplitItemEntity> requisitionSplitItemEntities, decimal lineItemQuantity, bool updateTaxes)
        //{
        //    #region Debug Logging
        //    if (Log.IsDebugEnabled)
        //    {
        //        Log.Debug(string.Concat("In GetRequisitionAccountingDetailsByItemId Method in RequisitionRestService ",
        //        " with parameters: requisitionSplitItems = " + requisitionSplitItems.ToJSON() + "requisitionSplitItemEntities = " + requisitionSplitItemEntities.ToJSON()));
        //    }
        //    #endregion
        //    string strResult = string.Empty;

        //    var objP2PDocumentServiceChannel = GepServiceManager.GetInstance.CreateChannel<IRequisitionServiceChannel>(MultiRegionConfig.GetConfig(CloudConfig.RequisitionServiceURL));
        //    UserExecutionContext userExecutionContext = P2P.Req.RestService.ExceptionHelper.GetExecutionContext;


        //    try
        //    {
        //        using (new OperationContextScope(objP2PDocumentServiceChannel))
        //        {

        //            var objMhg = new MessageHeader<UserExecutionContext>(userExecutionContext);
        //            var untypedmh = objMhg.GetUntypedHeader("GepCustomHeader", "Gep.Cumulus");
        //            OperationContext.Current.OutgoingMessageHeaders.Add(untypedmh);
        //            strResult = objP2PDocumentServiceChannel.SaveRequisitionAccountingDetails(requisitionSplitItems, requisitionSplitItemEntities, lineItemQuantity, updateTaxes).ToJSON();
        //            objP2PDocumentServiceChannel.Close();
        //        }

        //    }

        //    catch (Exception ex)
        //    {
        //        LogHelper.LogError(Log, "Error occured in  SaveRequisitionAccountingDetails Method in RequisitionRestService", ex);
        //        // Log Exception here
        //        var objCustomFault = new CustomFault(ex.Message, "SaveRequisitionAccountingDetails", "SaveRequisitionAccountingDetails",
        //                                          "Requisition", ExceptionType.ApplicationException, string.Empty, false);
        //        throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while SaveRequisitionAccountingDetails  for requisition");
        //    }
        //    finally
        //    {
        //        //check and close the channel
        //        if (objP2PDocumentServiceChannel != null && objP2PDocumentServiceChannel.State != CommunicationState.Closed)
        //        {
        //            objP2PDocumentServiceChannel.Close();
        //        }
        //        else if (objP2PDocumentServiceChannel != null) objP2PDocumentServiceChannel.Abort();

        //    }

        //    return strResult;
        //}

        public string GetAllSplitAccountingControlValues(int entityTypeId, int parentEntityCode, string searchText)
        {
            #region Debug Logging
            if (Log.IsDebugEnabled)
            {
                Log.Debug(string.Concat("In GetAllSplitAccountingControlValues Method in RequisitionRestService ",
                " with parameters: entityTypeId = " + entityTypeId + " parentEntityCode = " + parentEntityCode + " searchText = " + searchText));
            }
            #endregion
            string strResult = string.Empty;

            var objOrganizationStructurChannel = GepServiceManager.GetInstance.CreateChannel<IOrganizationStructureChannel>(MultiRegionConfig.GetConfig(CloudConfig.OrganizationServiceURL));
            UserExecutionContext userExecutionContext = P2P.Req.RestService.ExceptionHelper.GetExecutionContext;


            try
            {
                using (new OperationContextScope(objOrganizationStructurChannel))
                {

                    var objMhg = new MessageHeader<UserExecutionContext>(userExecutionContext);
                    List<OrgEntity> listOrg = new List<OrgEntity>();
                    var untypedmh = objMhg.GetUntypedHeader("GepCustomHeader", "Gep.Cumulus");
                    OperationContext.Current.OutgoingMessageHeaders.Add(untypedmh);

                    MessageHeader<string> objMhgAuth = new MessageHeader<string>(Token);
                    System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
                    OperationContext.Current.OutgoingMessageHeaders.Add(authorization);

                    var objOrgSearch = new OrgSearch
                    {
                        SearchText = searchText,
                        ParentOrgEntityCode = parentEntityCode,
                        AssociationTypeInfo = AssociationType.Both,
                        objEntityType = new OrgEntityType { EntityId = entityTypeId }
                    };





                    listOrg = objOrganizationStructurChannel.GetEntityDetails(objOrgSearch);//.ToJSON();//.GetRequisitionAccountingDetailsByItemId(requisitionItemId, pageIndex, pageSize).ToJSON();
                    objOrganizationStructurChannel.Close();

                    if (listOrg.Count > 0)
                        strResult = JsonConvert.SerializeObject(listOrg.AsParallel().Select(x => new
                        {
                            x.Description,
                            x.EntityCode,
                            x.ParentEntityCode,
                            x.OrgEntityCode

                        }));

                    return strResult;

                }

            }

            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in  GetAllSplitAccountingControlValues Method in RequisitionRestService", ex);
                // Log Exception here
                var objCustomFault = new CustomFault(ex.Message, "GetAllSplitAccountingControlValues", "GetAllSplitAccountingControlValues",
                                                  "Requisition", ExceptionType.ApplicationException, string.Empty, false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while GetAllSplitAccountingControlValues  for requisition");
            }
            finally
            {
                //check and close the channel
                if (objOrganizationStructurChannel != null && objOrganizationStructurChannel.State != CommunicationState.Closed)
                {
                    objOrganizationStructurChannel.Close();
                }
                else if (objOrganizationStructurChannel != null) objOrganizationStructurChannel.Abort();

            }

            return strResult;
        }

        //public string GetDocumentCodeByRequisitionId(long requisitionId)
        //{
        //    #region Debug Logging
        //    if (Log.IsDebugEnabled)
        //    {
        //        Log.Debug(string.Concat("In GetDocumentCodeByRequisitionId Method in RequisitionRestService ",
        //        " with parameters: requisitionId = " + requisitionId.ToString() ));
        //    }
        //    #endregion
        //    string strResult = string.Empty;

        //    var objP2PDocumentServiceChannel = GepServiceManager.GetInstance.CreateChannel<IRequisitionServiceChannel>(MultiRegionConfig.GetConfig(CloudConfig.RequisitionServiceURL));
        //    UserExecutionContext userExecutionContext = P2P.Req.RestService.ExceptionHelper.GetExecutionContext;


        //    try
        //    {
        //        using (new OperationContextScope(objP2PDocumentServiceChannel))
        //        {

        //            var objMhg = new MessageHeader<UserExecutionContext>(userExecutionContext);
        //            var untypedmh = objMhg.GetUntypedHeader("GepCustomHeader", "Gep.Cumulus");
        //            OperationContext.Current.OutgoingMessageHeaders.Add(untypedmh);
        //            strResult = objP2PDocumentServiceChannel.GetDocumentCodeByRequisitionId(requisitionId).ToJSON();
        //            objP2PDocumentServiceChannel.Close();
        //        }

        //    }

        //    catch (Exception ex)
        //    {
        //        LogHelper.LogError(Log, "Error occured in  GetDocumentCodeByRequisitionId Method in RequisitionRestService", ex);
        //        // Log Exception here
        //        var objCustomFault = new CustomFault(ex.Message, "GetDocumentCodeByRequisitionId", "GetDocumentCodeByRequisitionId",
        //                                          "Requisition", ExceptionType.ApplicationException, string.Empty, false);
        //        throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while GetDocumentCodeByRequisitionId  for requisition");
        //    }
        //    finally
        //    {
        //        //check and close the channel
        //        if (objP2PDocumentServiceChannel != null && objP2PDocumentServiceChannel.State != CommunicationState.Closed)
        //        {
        //            objP2PDocumentServiceChannel.Close();
        //        }
        //        else if (objP2PDocumentServiceChannel != null) objP2PDocumentServiceChannel.Abort();

        //    }

        //    return strResult;
        //}

        public string GetAllQualifiedPartners(string searchText, int pageIndex, int pageSize)
        {
            #region Debug Logging
            if (Log.IsDebugEnabled)
            {
                Log.Debug(string.Concat("In GetAllQualifiedPartners Method in RequisitionRestService ",
                " with parameters: searchText = " + searchText));
            }
            #endregion
            var result = string.Empty;

            var objIPartnerServiceChannel = GepServiceManager.GetInstance.CreateChannel<IPartnerServiceChannel>(MultiRegionConfig.GetConfig(CloudConfig.PartnerServiceURL));
            UserExecutionContext userExecutionContext = P2P.Req.RestService.ExceptionHelper.GetExecutionContext;
            try
            {
                using (new OperationContextScope(objIPartnerServiceChannel))
                {

                    var objMhg = new MessageHeader<UserExecutionContext>(userExecutionContext);
                    var untypedmh = objMhg.GetUntypedHeader("GepCustomHeader", "Gep.Cumulus");
                    OperationContext.Current.OutgoingMessageHeaders.Add(untypedmh);

                    MessageHeader<string> objMhgAuth = new MessageHeader<string>(Token);
                    System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
                    OperationContext.Current.OutgoingMessageHeaders.Add(authorization);

                    var objPartnersInfo = objIPartnerServiceChannel.GetBuyerSuppliersAutoSuggest(userExecutionContext.BuyerPartnerCode, "2", searchText, pageIndex + 1, pageSize);
                    objIPartnerServiceChannel.Close();
                    var objPartners = (from PartnerInfo objPartner in objPartnersInfo
                                       select new
                                       {

                                           value = objPartner.PartnerName,
                                           PartnerCode = objPartner.PartnerCode
                                       }).ToList();

                    result = objPartners.ToJSON();
                }
            }

            catch (CommunicationException commFaultEx)
            {
                LogHelper.LogError(Log, "Error occured in  GetAllQualifiedPartners Method in RequisitionRestService", commFaultEx);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in  GetAllQualifiedPartners Method in RequisitionRestService", ex);
                throw;
            }
            finally
            {
                //check and close the channel
                if (objIPartnerServiceChannel != null && objIPartnerServiceChannel.State != CommunicationState.Closed)
                {
                    objIPartnerServiceChannel.Close();
                }
                else if (objIPartnerServiceChannel != null) objIPartnerServiceChannel.Abort();
            }

            return result.ToJSON();
        }

        public string GetDocumentDetailsByDocumentCode(long documentCode)
        {
            #region Debug Logging
            if (Log.IsDebugEnabled)
            {
                Log.Debug(string.Concat("In GetDocumentDetailsByDocumentCode Method in RequisitionRestService ",
                " with parameters: requisitionItemId = " + documentCode));
            }
            #endregion
            string strResult = string.Empty;

            var objP2PDocumentServiceChannel = GepServiceManager.GetInstance.CreateChannel<IRequisitionServiceChannel>(MultiRegionConfig.GetConfig(CloudConfig.RequisitionServiceURL));
            UserExecutionContext userExecutionContext = P2P.Req.RestService.ExceptionHelper.GetExecutionContext;


            try
            {
                using (new OperationContextScope(objP2PDocumentServiceChannel))
                {

                    var objMhg = new MessageHeader<UserExecutionContext>(userExecutionContext);
                    var untypedmh = objMhg.GetUntypedHeader("GepCustomHeader", "Gep.Cumulus");
                    OperationContext.Current.OutgoingMessageHeaders.Add(untypedmh);

                    MessageHeader<string> objMhgAuth = new MessageHeader<string>(Token);
                    System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
                    OperationContext.Current.OutgoingMessageHeaders.Add(authorization);

                    strResult = objP2PDocumentServiceChannel.GetDocumentDetailsByDocumentCode(documentCode).ToString();
                    objP2PDocumentServiceChannel.Close();
                }

            }

            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in  GetDocumentDetailsByDocumentCode Method in RequisitionRestService", ex);
                // Log Exception here
                var objCustomFault = new CustomFault(ex.Message, "GetDocumentDetailsByDocumentCode", "GetDocumentDetailsByDocumentCode",
                                                  "Requisition", ExceptionType.ApplicationException, string.Empty, false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while GetDocumentDetailsByDocumentCode  for requisition");
            }
            finally
            {
                //check and close the channel
                if (objP2PDocumentServiceChannel != null && objP2PDocumentServiceChannel.State != CommunicationState.Closed)
                {
                    objP2PDocumentServiceChannel.Close();
                }
                else if (objP2PDocumentServiceChannel != null) objP2PDocumentServiceChannel.Abort();

            }

            return strResult;
        }

        public string GetAllSplitAccountingFieldsWithDefaultValues(P2PDocumentType docType, LevelType levelType, long documentCode, long OnBehalfOfId)
        {
            string strResult = string.Empty;
            ProxyRequisitionService proxyRequisitionService;
            try
            {
                UserExecutionContext userExecutionContext = P2P.Req.RestService.ExceptionHelper.GetExecutionContext;
                proxyRequisitionService = new ProxyRequisitionService(userExecutionContext, Token);
                strResult = proxyRequisitionService.GetAllSplitAccountingFieldsWithDefaultValues(docType, levelType, documentCode, OnBehalfOfId).ToJSON();
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in  SendNotificationForRequisitionApproval Method in OrderRestService", ex);
                throw;
            }
            return strResult;
        }
        public void SkipCallBackMethod(string eventName, long documentCode, string documentStatus, int wfDocTypeId, long contactCode, string userStatus, string approvalType, string returnEntity, List<ApproverDetails> lstApprovers, int skipType = 0, bool isOffLine = false, long actionarId = 0)
        {
            #region Debug Logging
            if (Log.IsDebugEnabled)
            {
                Log.Debug(string.Concat("In SkipOrOfflineCallBackMethod Method in RequisitionRestService of class ", typeof(Requisition).ToString(),
                " with parameter: documentCode = " + documentCode + ", documentStatus=" + documentStatus));
            }
            #endregion
            var userExecutionContext = P2P.Req.RestService.ExceptionHelper.GetExecutionContext;
            P2P.RestService.P2PDocumentRestService objDocSer = new P2P.RestService.P2PDocumentRestService() { jwtToken = Token };
            var documentStatusInfo = (DocumentStatus)Enum.Parse(typeof(DocumentStatus), documentStatus);
            string jwtToken = Token;
            var currentPrincipal = System.Threading.Thread.CurrentPrincipal;
            Task.Factory.StartNew((state) =>
            {
                System.Threading.Thread.CurrentPrincipal = currentPrincipal;

                // EnableAsycCallback
                bool EnableAsycCallback = false;
                ProxyP2PCommonService proxyP2PCommonService = new ProxyP2PCommonService((UserExecutionContext)state, jwtToken);
                string enableAsycCallback = proxyP2PCommonService.GetSettingsValueByKey(P2PDocumentType.Requisition, "EnableAsycCallback", userExecutionContext.ContactCode, 107);
                if (!string.IsNullOrEmpty(enableAsycCallback) && enableAsycCallback.ToLower() == "true")
                {
                    EnableAsycCallback = true;
                }
                
                ProxyRequisitionService proxyRequisitionService = new ProxyRequisitionService((UserExecutionContext)state, jwtToken);
                if (documentStatusInfo == DocumentStatus.ApprovalPending && lstApprovers.Any())
                    proxyRequisitionService.SendNotificationForSkipOrOffLineRequisitionApproval(documentCode, lstApprovers, EnableAsycCallback, skipType, isOffLine, actionarId);
            }, userExecutionContext);
            if (lstApprovers.Any())
            {
                ProxyP2PCommonService proxyP2PCommonService = new ProxyP2PCommonService(userExecutionContext, jwtToken);
                string EnableRequisitionDocumentServiceHelper = proxyP2PCommonService.GetSettingsValueByKey(P2PDocumentType.Requisition, "EnableRequisitionDocumentServiceHelper", userExecutionContext.ContactCode, 107);                
                if (!string.IsNullOrEmpty(EnableRequisitionDocumentServiceHelper) && EnableRequisitionDocumentServiceHelper.ToLower() == "true")
                {
                    LogNewRelic("Inside true case for EnableRequisitionDocumentServiceHelper (at LN1197) = " + EnableRequisitionDocumentServiceHelper, "ApprovalCallBackMethod", documentCode);
                    DocumentRestServiceHelper helper = new DocumentRestServiceHelper(Token);

                    helper.DeleteAllTasksForDocument(documentCode, wfDocTypeId, lstApprovers, userExecutionContext);
                }
                else
                {
                    objDocSer.DeleteAllTasksForDocument(documentCode, wfDocTypeId, lstApprovers);
                }
            }
        }
        public void ApprovalCallBackMethod(string eventName, long documentCode, string documentStatus, int wfDocTypeId, long contactCode, string userStatus, string approvalType, string returnEntity, string hierarchyIds = null)
        {
            #region Debug Logging
            if (Log.IsDebugEnabled)
            {
                Log.Debug(string.Concat("In ApprovalCallBackMethod Method in RequisitionRestService of class ", typeof(Requisition).ToString(),
                " with parameter: documentCode = " + documentCode + ", documentStatus=" + documentStatus));
            }
            #endregion

            try
            {
                LogNewRelic("Testing Log inside ApprovalCallBackMethod at LN - 2016", "ApprovalCallBackMethod", documentCode);
                LogNewRelic("Inside ApprovalCallBackMethod at LN - 2017", "ApprovalCallBackMethod", documentCode);
                List<ApproverDetails> lstActioners = new List<ApproverDetails>();
                

                var userExecutionContext = P2P.Req.RestService.ExceptionHelper.GetExecutionContext;
                var documentStatusInfo = (DocumentStatus)Enum.Parse(typeof(DocumentStatus), documentStatus);
                long wFOrderId = 0;

                lstActioners = GetActionersDetails(documentCode, wfDocTypeId);

                List<int> lstHierarchyIds = new List<int>();
                if (!string.IsNullOrEmpty(hierarchyIds) && hierarchyIds.Length > 2)
                {
                    lstHierarchyIds = hierarchyIds.Substring(1, hierarchyIds.Length - 2).Split(',').Select(int.Parse).ToList();
                }

                var jwtToken = Token;
                var currentPrincipal = System.Threading.Thread.CurrentPrincipal;
                Task.Factory.StartNew((state) =>
                {
                    System.Threading.Thread.CurrentPrincipal = currentPrincipal;
                    LogNewRelic("Inside Task Factory ApprovalCallBackMethod at LN - 2037", "ApprovalCallBackMethod", documentCode);
                    ProxyRequisitionService proxyRequisitionService = new ProxyRequisitionService((UserExecutionContext)state, jwtToken);
                    ProxyProgramService proxyProgramService = new ProxyProgramService((UserExecutionContext)state, jwtToken);
                    ProxyP2PCommonService proxyP2PCommonService = new ProxyP2PCommonService((UserExecutionContext)state, jwtToken);
                    ProxyOperationalBudgetService proxyOperationalBudegetService = new ProxyOperationalBudgetService((UserExecutionContext)state, jwtToken);
                    ProxyP2PDocumentService proxyP2PDocumentService = new ProxyP2PDocumentService((UserExecutionContext)state, jwtToken);
                    List<ApproverDetails> lstPendingApprovers = new List<ApproverDetails>();
                    List<ApproverDetails> lstSkippedApprovers = new List<ApproverDetails>();
                    List<ApproverDetails> lstFinalApprovers = new List<ApproverDetails>();
                    string keyValue = proxyP2PCommonService.GetSettingsValueByKey(P2PDocumentType.None, "IsOperationalBudgetEnabled", ((UserExecutionContext)state).ContactCode, 107);
                    string IsOperationalBudgetEnabledOnSubmit = proxyP2PCommonService.GetSettingsValueByKey(P2PDocumentType.Requisition, "IsOperationalBudgetEnabledOnSubmit", ((UserExecutionContext)state).ContactCode, 107);
                    bool showBillableAtHeader = Convert.ToBoolean(proxyP2PCommonService.GetSettingsValueByKey(P2PDocumentType.None, "ShowBillableAtHeader", ((UserExecutionContext)state).ContactCode, 107));
                    bool isAutoSourcing = Convert.ToBoolean(proxyP2PCommonService.GetSettingsValueByKey(P2PDocumentType.None, "IsAutoSourcing", ((UserExecutionContext)state).ContactCode, 107));
                    bool isManualAutosourcing = Convert.ToBoolean(proxyP2PCommonService.GetSettingsValueByKey(P2PDocumentType.Requisition, "IsManualAutosourcing", ((UserExecutionContext)state).ContactCode, 107));
                    string skippedApprovalSetting = proxyP2PCommonService.GetSettingsValueByKey(P2PDocumentType.Requisition, "EnableSkippedApproval", ((UserExecutionContext)state).ContactCode, 107);
                    bool isEnableSkippedApproval = false;
                    if (!(string.IsNullOrEmpty(skippedApprovalSetting)))
                    {
                        isEnableSkippedApproval = Convert.ToBoolean(skippedApprovalSetting);
                    }

                    // EnableAsycCallback
                    bool EnableAsycCallback = false;
                    string enableAsycCallback = proxyP2PCommonService.GetSettingsValueByKey(P2PDocumentType.Requisition, "EnableAsycCallback", ((UserExecutionContext)state).ContactCode, 107);
                    if (!string.IsNullOrEmpty(enableAsycCallback) && enableAsycCallback.ToLower() == "true")
                    {
                        EnableAsycCallback = true;
                    }


                    bool isUrgentReq = false;
                    if ((documentStatusInfo == DocumentStatus.Rejected && eventName == P2PConstants.EVENT_INTERMEDIATE))
                    {
                        if (IsOperationalBudgetEnabledOnSubmit.ToLower() == "true" && keyValue.ToLower() == "true")
                        {
                            proxyOperationalBudegetService.UpdateOperationalBudgetFundsFlow(DocumentType.Requisition, documentCode, DocumentStatus.Withdrawn, showBillableAtHeader);
                        }
                    }
                    if (documentStatusInfo == DocumentStatus.Rejected && eventName == P2PConstants.EVENT_INTERMEDIATE)
                        proxyRequisitionService.UpdateLineStatusForRequisition(documentCode, (StockReservationStatus)(DocumentStatus.Rejected), true, null);
                    else if (documentStatusInfo == DocumentStatus.Approved)
                        proxyRequisitionService.UpdateLineStatusForRequisition(documentCode, (StockReservationStatus)(DocumentStatus.Approved), true, null);
                    if (eventName == P2PConstants.EVENT_ONSUBMIT || (documentStatusInfo == DocumentStatus.Rejected && eventName == P2PConstants.EVENT_INTERMEDIATE))
                    {
                        List<TaskActionDetails> lstTasksAction = new List<TaskActionDetails>();
                        var taskInformation = new TaskInformation();
                        var requisition = proxyRequisitionService.GetRequisitionBasicDetailsById(documentCode, userExecutionContext.ContactCode, 0);
                        isUrgentReq = requisition.IsUrgent;
                        if (eventName == P2PConstants.EVENT_ONSUBMIT)
                        {
                            lstTasksAction.Add(TaskHelper.CreateActionDetails(ActionKey.SentForApproval, TaskConstants.SENT_FOR_APPROVAl));
                            lstTasksAction.Add(TaskHelper.CreateActionDetails(ActionKey.Edit, TaskConstants.EDIT));
                            taskInformation = TaskHelper.CreateTaskObject(documentCode, requisition.CreatedBy, lstTasksAction, true, false, ((UserExecutionContext)state).BuyerPartnerCode, ((UserExecutionContext)state).CompanyName);
                        }
                        else
                        {
                            lstTasksAction.Add(TaskHelper.CreateActionDetails(ActionKey.Edit, TaskConstants.EDIT));
                            taskInformation = TaskHelper.CreateTaskObject(documentCode, requisition.CreatedBy, lstTasksAction, false, false, ((UserExecutionContext)state).BuyerPartnerCode, ((UserExecutionContext)state).CompanyName);
                        }
                        proxyP2PDocumentService.SaveTaskActionDetails(taskInformation);
                    }

                    string queryString = UrlEncryptionHelper.EncryptURL("dc=" + documentCode.ToString(CultureInfo.InvariantCulture) + "&bpc=" +
                                                                            ((UserExecutionContext)state).BuyerPartnerCode.ToString(CultureInfo.InvariantCulture)) + "&oloc=" + (int)SubAppCodes.P2P;

                    lstActioners.Where(e => !e.IsProcessed && e.Status == 2 && e.WorkflowId != 11).ToList().ForEach(e => lstPendingApprovers.Add(e));

                    if (isEnableSkippedApproval && isUrgentReq)
                    {
                        lstActioners.Where(e => !e.IsProcessed && e.Status == 13 && e.WorkflowId == 1).ToList().ForEach(e => lstSkippedApprovers.Add(e));
                        lstActioners.Where(e => !e.IsProcessed && e.Status == 2 && e.WorkflowId == 1).ToList().ForEach(e => lstFinalApprovers.Add(e));
                    }

                    if (lstHierarchyIds != null && lstHierarchyIds.Count() > 0)
                    {
                        lstActioners.Where(e => !e.IsProcessed && e.Status == 2 && e.WorkflowId == 11 && e.SubIsProcessed == false && lstHierarchyIds.Contains(e.HierarchyId)).ToList().ForEach(e => lstPendingApprovers.Add(e));
                    }
                    else
                    {
                        lstActioners.Where(e => !e.IsProcessed && e.Status == 2 && e.WorkflowId == 11 && e.SubIsProcessed == false).ToList().ForEach(e => lstPendingApprovers.Add(e));
                    }

                    if (documentStatusInfo == DocumentStatus.ApprovalPending && lstActioners.Any())
                    {
                        if (isEnableSkippedApproval && isUrgentReq && eventName == P2PConstants.EVENT_ONSUBMIT)
                        {
                            proxyRequisitionService.SendNotificationForSkipApproval(documentCode, lstSkippedApprovers, lstFinalApprovers, EnableAsycCallback);
                        }
                        else
                        {
                            proxyRequisitionService.SendNotificationForRequisitionApproval(documentCode, lstPendingApprovers, (eventName == P2PConstants.EVENT_ONSUBMIT) ? new List<ApproverDetails>() : lstActioners.Where(p => p.Status == 1).ToList(), eventName, documentStatusInfo, approvalType, EnableAsycCallback);
                        }
                    }
                    if (documentStatusInfo == DocumentStatus.Rejected && lstActioners.Where(p => p.Status == 0).Any())
                        proxyRequisitionService.SendNotificationForRejectedRequisition(documentCode, lstActioners.Where(p => p.Status == 0).First(), lstActioners.Where(p => p.Status != 0).ToList(), queryString, EnableAsycCallback);

                    ProcessReminderNotifications(approvalType, wFOrderId, lstActioners, lstHierarchyIds, lstPendingApprovers, documentCode, documentStatus, wfDocTypeId, (UserExecutionContext)state, jwtToken);

                    if (eventName == P2PConstants.EVENT_INTERMEDIATE && (documentStatusInfo == DocumentStatus.Approved || documentStatusInfo == DocumentStatus.Rejected))
                    {
                        ConsumeReleaseCapitalBudget(documentCode, documentStatusInfo, (UserExecutionContext)state,true, jwtToken);
                    }
                    if (eventName == P2PConstants.EVENT_INTERMEDIATE && documentStatusInfo == DocumentStatus.Approved)
                    {
                        DocumentLOBDetails documentLob = GetDocumentLOB(documentCode, (UserExecutionContext)state, jwtToken);
                        bool checkQuickQuoteRuleRequired = Convert.ToBoolean(proxyP2PCommonService.GetSettingsValueByKeyAndLOB(P2PDocumentType.None, "EnablePolicyBased3BidAndBuy", ((UserExecutionContext)state).ContactCode, (int)SubAppCodes.P2P, documentLob != null ? documentLob.EntityDetailCode : 0));

                        if (checkQuickQuoteRuleRequired)
                        {
                            NewProxyRequisitionService prxyReqService = new NewProxyRequisitionService((UserExecutionContext)state, jwtToken);
                            bool isEnableQuickQuoteCheckSuccess = prxyReqService.EnableQuickQuoteRuleCheck(documentCode);
                        }

                        if (keyValue.ToLower() == "true" && IsOperationalBudgetEnabledOnSubmit.ToLower() == "false" && !proxyOperationalBudegetService.UpdateOperationalBudgetFundsFlow(DocumentType.Requisition, documentCode, DocumentStatus.Approved, showBillableAtHeader))
                        {                            
                            RejectDocumentOperationBudget(documentCode, DocumentType.Requisition, "Requistion", (UserExecutionContext)state, jwtToken);
                        }
                        else
                        {
                            string attributesToBeUsedForPurchaseOrderConsolidation = (proxyP2PCommonService.GetSettingsValueByKey(P2PDocumentType.Requisition, "AttributesToBeUsedForPurchaseOrderConsolidation", ((UserExecutionContext)state).ContactCode, 107));
                            if (!(string.IsNullOrEmpty(attributesToBeUsedForPurchaseOrderConsolidation)))
                            {
                                PushingRequisitionToEventHubInternal(documentCode, attributesToBeUsedForPurchaseOrderConsolidation, contactCode, jwtToken, (UserExecutionContext)state);
                            }
                            else
                            {
                                if (isAutoSourcing && isManualAutosourcing)
                                {
                                    proxyRequisitionService.AutoCreateWorkBenchOrder(documentCode);
                                }
                                proxyRequisitionService.AutoCreateOrder(documentCode);
                            }
                        }

                    }
                    
                }, userExecutionContext);

                if (lstActioners.Any())
                {
                    // read setting
                    ProxyP2PCommonService proxyP2PCommonService = new ProxyP2PCommonService(userExecutionContext, jwtToken);
                    string EnableRequisitionDocumentServiceHelper = proxyP2PCommonService.GetSettingsValueByKey(P2PDocumentType.Requisition, "EnableRequisitionDocumentServiceHelper", userExecutionContext.ContactCode, 107);

                    LogNewRelic("EnableRequisitionDocumentServiceHelper = " + EnableRequisitionDocumentServiceHelper, "ApprovalCallBackMethod", documentCode);
                    if (!string.IsNullOrEmpty(EnableRequisitionDocumentServiceHelper) && EnableRequisitionDocumentServiceHelper.ToLower() == "true")
                    {

                        LogNewRelic("Inside true case for EnableRequisitionDocumentServiceHelper = " + EnableRequisitionDocumentServiceHelper, "ApprovalCallBackMethod", documentCode);
                        DocumentRestServiceHelper helper = new DocumentRestServiceHelper(jwtToken);

                        helper.DeleteAllTasksForDocument(documentCode, wfDocTypeId, lstActioners, userExecutionContext);

                        LogNewRelic("After helper.DeleteAllTasksForDocument at LN 2190", "ApprovalCallBackMethod", documentCode);
                        if (userStatus != "Rejected" && lstActioners.Where(e => !e.IsProcessed && e.Status == 2).Count() > 0)
                        {
                            LogNewRelic("Before helper.AddTasksForDocument at LN 2193", "ApprovalCallBackMethod", documentCode);
                            helper.AddTasksForDocument(documentCode, wfDocTypeId, lstActioners.Where(e => !e.IsProcessed && e.Status == 2).ToList(), userExecutionContext);
                            LogNewRelic("After helper.AddTasksForDocument at LN 2195", "ApprovalCallBackMethod", documentCode);
                        }
                    }
                    else
                    {
                        LogNewRelic("Inside else case for EnableRequisitionDocumentServiceHelper = " + EnableRequisitionDocumentServiceHelper, "ApprovalCallBackMethod", documentCode);
                        P2P.RestService.P2PDocumentRestService objDocSer = new P2P.RestService.P2PDocumentRestService() { jwtToken = jwtToken };
                        objDocSer.DeleteAllTasksForDocument(documentCode, wfDocTypeId, lstActioners);
                        if (userStatus != "Rejected" && lstActioners.Where(e => !e.IsProcessed && e.Status == 2).Count() > 0)
                        {
                            LogNewRelic("Before AddTasksForDocument, documentCode:" + documentCode.ToString() + " wfDocTypeId:" + wfDocTypeId.ToString(), "ApprovalCallBackMethod", documentCode);
                            objDocSer.AddTasksForDocument(documentCode, wfDocTypeId, lstActioners.Where(e => !e.IsProcessed && e.Status == 2).ToList());
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, string.Concat("Error occured in ApprovalCallBackMethod method in RequisitionRestService for DocumentCode ", documentCode), ex);
                throw;
            }
        }

        public void SendNotificationForApprovedRequisition(long documentCode)
        {
            UserExecutionContext userExecutionContext = P2P.Req.RestService.ExceptionHelper.GetExecutionContext;
            string queryString = UrlEncryptionHelper.EncryptURL("dc=" + documentCode.ToString(CultureInfo.InvariantCulture) + "&bpc=" +
                userExecutionContext.BuyerPartnerCode.ToString(CultureInfo.InvariantCulture)) + "&oloc=" + (int)SubAppCodes.P2P;
            var objRequisitionServiceChannel = GepServiceManager.GetInstance.CreateChannel<IRequisitionServiceChannel>(MultiRegionConfig.GetConfig(CloudConfig.RequisitionServiceURL));
            try
            {
                using (new OperationContextScope(objRequisitionServiceChannel))
                {
                    var objMhg = new MessageHeader<UserExecutionContext>(userExecutionContext);
                    var untypedmh = objMhg.GetUntypedHeader("GepCustomHeader", "Gep.Cumulus");
                    OperationContext.Current.OutgoingMessageHeaders.Add(untypedmh);

                    MessageHeader<string> objMhgAuth = new MessageHeader<string>(Token);
                    System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
                    OperationContext.Current.OutgoingMessageHeaders.Add(authorization);



                    objRequisitionServiceChannel.SendNotificationForApprovedRequisition(documentCode, queryString);
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in  SendNotificationForApprovedRequisition Method in RequisitionRestService", ex);
                throw;
            }
            finally
            {
                //check and close the channel
                if (objRequisitionServiceChannel != null && objRequisitionServiceChannel.State != CommunicationState.Closed)
                {
                    objRequisitionServiceChannel.Close();
                }
                else if (objRequisitionServiceChannel != null) objRequisitionServiceChannel.Abort();
            }
        }

        public void SendNotificationForRejectedRequisition(long documentCode, ApproverDetails rejector, List<ApproverDetails> prevApprover)
        {
            UserExecutionContext userExecutionContext = P2P.Req.RestService.ExceptionHelper.GetExecutionContext;
            string queryString = UrlEncryptionHelper.EncryptURL("dc=" + documentCode.ToString(CultureInfo.InvariantCulture) + "&bpc=" +
                userExecutionContext.BuyerPartnerCode.ToString(CultureInfo.InvariantCulture)) + "&oloc=" + (int)SubAppCodes.P2P;
            var objRequisitionServiceChannel = GepServiceManager.GetInstance.CreateChannel<IRequisitionServiceChannel>(MultiRegionConfig.GetConfig(CloudConfig.RequisitionServiceURL));
            try
            {
                using (new OperationContextScope(objRequisitionServiceChannel))
                {
                    var objMhg = new MessageHeader<UserExecutionContext>(userExecutionContext);
                    var untypedmh = objMhg.GetUntypedHeader("GepCustomHeader", "Gep.Cumulus");
                    OperationContext.Current.OutgoingMessageHeaders.Add(untypedmh);

                    MessageHeader<string> objMhgAuth = new MessageHeader<string>(Token);
                    System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
                    OperationContext.Current.OutgoingMessageHeaders.Add(authorization);

                    objRequisitionServiceChannel.SendNotificationForRejectedRequisition(documentCode, rejector, prevApprover, queryString);
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in  SendNotificationForRejectedRequisition Method in RequisitionRestService", ex);
                throw;
            }
            finally
            {
                //check and close the channel
                if (objRequisitionServiceChannel != null && objRequisitionServiceChannel.State != CommunicationState.Closed)
                {
                    objRequisitionServiceChannel.Close();
                }
                else if (objRequisitionServiceChannel != null) objRequisitionServiceChannel.Abort();
            }
        }

        public string GetAllCategoriesByReqId(long documentCode)
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug(string.Concat("In GetAllCategoriesByReqId Method in Order RestService ",
                            " with parameters: reqId = " + documentCode));
            }

            List<long> lstCategories = new List<long>();
            UserExecutionContext userExecutionContext = P2P.Req.RestService.ExceptionHelper.GetExecutionContext;
            var wcfClient = new ResilientWCFClient(this, Newtonsoft.Json.JsonConvert.SerializeObject(userExecutionContext));
            var gepCommunicationContext = new GEPCommunicationContext("RequisitionRestService-GetAllCategoriesByReqId");
            gepCommunicationContext.Add("documentCode", documentCode);

            var wcfResult = wcfClient.Execute<IRequisitionServiceChannel>((context, channel) =>
            {
                lstCategories = channel.GetAllCategoriesByReqId((long)context["documentCode"]);
            }, gepCommunicationContext, CloudConfig.RequisitionServiceURL, string.Empty);

            if (wcfResult.Outcome == Polly.OutcomeType.Successful)
            {
                return lstCategories.ToJSON();
            }
            else
            {
                LogHelper.LogError(Log, "Error occured in GetAllCategoriesByReqId Method", wcfResult.FinalException);
                var customFault = new CustomFault(wcfResult.FinalException.Message, "GetAllCategoriesByReqId", "GetAllCategoriesByReqId", "RequisitionRestService", ExceptionType.ApplicationException, documentCode.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(customFault, "Error while getting categories based on orderId  " +
                                                      documentCode.ToString(CultureInfo.InvariantCulture));
            }
        }

        public string GetAllEntitiesByReqId(long documentCode, int entityTypeId)
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug(string.Concat("In GetAllEntitiesByReqId Method in RequisitionRestService ",
                            " with parameters: catalogId = " + documentCode));
            }
            KeyValuePair<long, decimal> lstReqEntities = new KeyValuePair<long, decimal>();
            UserExecutionContext userExecutionContext = P2P.Req.RestService.ExceptionHelper.GetExecutionContext;

            var wcfClient = new ResilientWCFClient(this, Newtonsoft.Json.JsonConvert.SerializeObject(userExecutionContext));
            var gepCommunicationContext = new GEPCommunicationContext("RequisitionRestService-GetAllEntitiesByReqId");
            gepCommunicationContext.Add("documentCode", documentCode);
            gepCommunicationContext.Add("entityTypeId", entityTypeId);

            var jwtToken = Token;
            var wcfResult = wcfClient.Execute<IRequisitionServiceChannel>((context, channel) =>
            {
                MessageHeader<string> objMhgAuth = new MessageHeader<string>(jwtToken);
                System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
                OperationContext.Current.OutgoingMessageHeaders.Add(authorization);

                lstReqEntities = channel.GetAllEntitiesByReqId((long)context["documentCode"], (int)context["entityTypeId"]);
            }, gepCommunicationContext, CloudConfig.RequisitionServiceURL, string.Empty);

            if (wcfResult.Outcome == Polly.OutcomeType.Successful)
            {
                return lstReqEntities.ToJSON();
            }
            else
            {
                LogHelper.LogError(Log, "Error occured in GetAllEntitiesByReqId Method", wcfResult.FinalException);
                var customFault = new CustomFault(wcfResult.FinalException.Message, "GetAllEntitiesByReqId", "GetAllEntitiesByReqId", "RequisitionRestService", ExceptionType.ApplicationException, string.Empty, false);
                throw new SMARTFaultException.FaultException<CustomFault>(customFault, "Error occured in GetAllEntitiesByReqId method in RequisitionRestService");
            }

        }

        public void SendNotificationForRequisitionApproval(long documentCode, List<ApproverDetails> lstPendingApprover, List<ApproverDetails> lstPastApprover, string eventName, DocumentStatus documentStatus, string ApprovalType)
        {
            ProxyRequisitionService proxyRequisitionService;
            UserExecutionContext userExecutionContext = P2P.Req.RestService.ExceptionHelper.GetExecutionContext;
            try
            {
                // EnableAsycCallback
                bool EnableAsycCallback = false;
                ProxyP2PCommonService proxyP2PCommonService = new ProxyP2PCommonService(userExecutionContext, Token);
                string enableAsycCallback = proxyP2PCommonService.GetSettingsValueByKey(P2PDocumentType.Requisition, "EnableAsycCallback", userExecutionContext.ContactCode, 107);
                if (!string.IsNullOrEmpty(enableAsycCallback) && enableAsycCallback.ToLower() == "true")
                {
                    EnableAsycCallback = true;
                }

                proxyRequisitionService = new ProxyRequisitionService(userExecutionContext, Token);
                proxyRequisitionService.SendNotificationForRequisitionApproval(documentCode, lstPendingApprover, lstPastApprover, eventName, documentStatus, ApprovalType, EnableAsycCallback);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in  SendNotificationForRequisitionApproval Method in OrderRestService", ex);
                throw;
            }
        }

        public Dictionary<string, string> SentRequisitionForApproval(long contactCode, long documentCode, decimal documentAmount, int documentTypeId, string fromCurrency, string toCurrency, bool isOperationalBudgetEnabled, long headerOrgEntityCode)
        {
            Dictionary<string, string> result;
            UserExecutionContext userExecutionContext = P2P.Req.RestService.ExceptionHelper.GetExecutionContext;
            ProxyRequisitionService proxyRequisitionService = new ProxyRequisitionService(P2P.Req.RestService.ExceptionHelper.GetExecutionContext, Token);
            try
            {
                result = proxyRequisitionService.SentRequisitionForApproval(contactCode, documentCode, documentAmount, documentTypeId, fromCurrency, toCurrency, isOperationalBudgetEnabled, headerOrgEntityCode);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in  SentRequisitionForApproval Method in RequisitionRestService", ex);
                throw;
            }
            return result;
        }

        public long SaveCatalogRequisition(long userId, long reqId, string requisitionName, string requisitionNumber, long oboId = 0)
        {
            long result = 0;
            ProxyRequisitionService proxyRequisitionService = new ProxyRequisitionService(P2P.Req.RestService.ExceptionHelper.GetExecutionContext, Token);
            try
            {
                result = proxyRequisitionService.SaveCatalogRequisition(userId, reqId, requisitionName, requisitionNumber, oboId);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in SaveCatalogRequisition method", ex);
                throw;
            }
            return result;
        }

        public string GetSelectedRequisitionBasicDetailsById(long requisitionId)
        {
            #region Debug Logging
            if (Log.IsDebugEnabled)
            {
                Log.Debug(string.Concat("In GetRequisitionBasicDetailsById Method in RequisitionRestService ",
                " with parameters: requisitionId = " + requisitionId));
            }
            #endregion
            var result = string.Empty;
            Requisition reqObj;
            List<Requisition> reqList = new List<Requisition>();
            if (requisitionId > 0)
            {
                UserExecutionContext userExecutionContext = P2P.Req.RestService.ExceptionHelper.GetExecutionContext;
                P2P.RestService.P2PDocumentRestService objP2PDocumentRestService = new P2P.RestService.P2PDocumentRestService() { jwtToken = Token };
                ApproverDetails objApproverDetails = new ApproverDetails();
                objApproverDetails = objP2PDocumentRestService.GetContactCodeLastApprovalDetails(requisitionId, (int)DocumentType.Requisition, userExecutionContext.ContactCode);

                var objP2PDocumentServiceChannel = GepServiceManager.GetInstance.CreateChannel<IRequisitionServiceChannel>(MultiRegionConfig.GetConfig(CloudConfig.RequisitionServiceURL));
                NumberFormatInfo objNumberFormatInfo = new CultureInfo(userExecutionContext.Culture, false).NumberFormat;
                try
                {
                    using (new OperationContextScope(objP2PDocumentServiceChannel))
                    {
                        var objMhg = new MessageHeader<UserExecutionContext>(userExecutionContext);
                        var untypedmh = objMhg.GetUntypedHeader("GepCustomHeader", "Gep.Cumulus");
                        OperationContext.Current.OutgoingMessageHeaders.Add(untypedmh);
                        var userId = userExecutionContext.ContactCode;
                        //result = objP2PDocumentServiceChannel.GetRequisitionBasicDetailsById(requisitionId, userId, 0).ToJSON();

                        MessageHeader<string> objMhgAuth = new MessageHeader<string>(Token);
                        System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
                        OperationContext.Current.OutgoingMessageHeaders.Add(authorization);



                        reqObj = objP2PDocumentServiceChannel.GetRequisitionBasicDetailsById(requisitionId, userId, 0);

                        reqList.Add(reqObj);
                        objP2PDocumentServiceChannel.Close();

                        if (reqObj != null)
                        {
                            result = (reqList.AsParallel().Select(x => new
                            {
                                x.DocumentId,
                                x.DocumentName,
                                x.DocumentCode,
                                x.RequesterId,
                                CreatedByName = x.RequesterName,
                                x.CreatedOn,
                                x.ShiptoLocation.ShiptoLocationName,
                                x.ShiptoLocation.ShiptoLocationId,
                                x.BilltoLocation.BilltoLocationId,
                                x.BilltoLocation.BilltoLocationName,
                                x.DelivertoLocation.DelivertoLocationId,
                                x.DelivertoLocation.DelivertoLocationName,
                                TotalAmount = x.TotalAmount.Value.ToString("N", objNumberFormatInfo),
                                x.MaterialItemCount,
                                x.Currency,
                                x.DocumentNumber,
                                ApprovedRejectedDate = (objApproverDetails.WorkflowId <= 0) ? x.CreatedOn : objApproverDetails.StatusDate,
                                x.DocumentTypeInfo,
                                x.DocumentStatusInfo,
                                UserDocumentStatus = (objApproverDetails.WorkflowId <= 0) ? -1 : objApproverDetails.Status,
                                x.PurchaseType,
                                x.PurchaseTypeDescription,
                                //Exposed lob id
                                LOBId = x.DocumentLOBDetails.FirstOrDefault().EntityDetailCode,
                                //REQ-4738: Dependency(Mobility): Getting Header Entities in Basic Details 
                                HeaderEntities = x.DocumentAdditionalEntitiesInfoList.Select(y => new
                                {
                                    EntityDetailCode = y.EntityDetailCode,
                                    EntityId = y.EntityId,
                                    FieldName = y.FieldName,
                                    EntityDisplayName = y.EntityDisplayName
                                }).ToList()
                            })).ToJSON();
                        }

                    }

                }
                catch (CommunicationException commFaultEx)
                {
                    LogHelper.LogError(Log, "Error occured in  GetRequisitionBasicDetailsById Method in RequisitionRestService", commFaultEx);
                }
                catch (Exception ex)
                {
                    LogHelper.LogError(Log, "Error occured in  GetRequisitionBasicDetailsById Method in RequisitionRestService", ex);
                    throw;
                }
                finally
                {
                    //check and close the channel
                    if (objP2PDocumentServiceChannel != null && objP2PDocumentServiceChannel.State != CommunicationState.Closed)
                    {
                        objP2PDocumentServiceChannel.Close();
                    }
                    else if (objP2PDocumentServiceChannel != null) objP2PDocumentServiceChannel.Abort();

                }
            }
            return result;

        }

        public string GetSelectedRequisitionLineItemDetailsById(long requisitionId, P2P.BusinessEntities.ItemType itemType, int startIndex, int pageSize, string sortBy, string sortOrder, int typeOfUser)
        {
            #region Debug Logging
            if (Log.IsDebugEnabled)
            {
                Log.Debug(string.Concat("In GetRequisitionLineItemBasicDetails Method in RequisitionRestService ",
                " with parameters: requisitionId = " + requisitionId + ", " +

                "ItemType =" + itemType.ToString() + ", startIndex =" + startIndex + ", pageSize =" + pageSize + "," +
                "sortBy =" + sortBy + ", sortOrder =" + sortOrder));
            }
            #endregion
            var result = string.Empty;
            ICollection<RequisitionItem> reqItemsList = new List<RequisitionItem>();

            var objP2PDocumentServiceChannel = GepServiceManager.GetInstance.CreateChannel<IRequisitionServiceChannel>(MultiRegionConfig.GetConfig(CloudConfig.RequisitionServiceURL));
            UserExecutionContext userExecutionContext = P2P.Req.RestService.ExceptionHelper.GetExecutionContext;
            NumberFormatInfo objNumberFormatInfo = new CultureInfo(userExecutionContext.Culture, false).NumberFormat;
            if (requisitionId > 0)
            {
                try
                {
                    using (new OperationContextScope(objP2PDocumentServiceChannel))
                    {

                        var objMhg = new MessageHeader<UserExecutionContext>(userExecutionContext);
                        var untypedmh = objMhg.GetUntypedHeader("GepCustomHeader", "Gep.Cumulus");
                        OperationContext.Current.OutgoingMessageHeaders.Add(untypedmh);

                        MessageHeader<string> objMhgAuth = new MessageHeader<string>(Token);
                        System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
                        OperationContext.Current.OutgoingMessageHeaders.Add(authorization);



                        reqItemsList = objP2PDocumentServiceChannel.GetRequisitionLineItemBasicDetails(requisitionId, itemType, startIndex, pageSize, sortBy, sortOrder, typeOfUser);
                        objP2PDocumentServiceChannel.Close();

                        if (reqItemsList != null && reqItemsList.Count > 0)
                        {
                            result = (reqItemsList.AsParallel().Select(x => new
                            {
                                x.ItemType,
                                x.Description,
                                x.StartDate,
                                x.EndDate,
                                x.UnitPrice,
                                x.Quantity,
                                x.Efforts,
                                x.UOM,
                                x.UOMDesc,
                                x.PartnerCode,
                                x.PartnerName,
                                x.Taxes,
                                x.Tax,
                                x.CategoryId,
                                x.CategoryName,
                                x.DateNeeded,
                                ItemTotalAmount = x.ItemTotalAmount.Value.ToString("N", objNumberFormatInfo),
                                x.ItemExtendedType,
                                x.ItemNumber,               //Buyer Item Number
                                x.ItemLineNumber,           //Line Number
                                x.SupplierPartId,           //Supplier Item Number
                                x.OrderLocationId,       //Supplier Location Id
                                x.OrderLocationName,        // Supplier Location Name
                                x.ItemSplitsDetail   //Gets all Item Splits details
                            })).ToJSON();
                        }

                    }

                }
                catch (CommunicationException commFaultEx)
                {
                    LogHelper.LogError(Log, "Error occured in  GetRequisitionBasicDetailsById Method in RequisitionRestService", commFaultEx);
                }
                catch (Exception ex)
                {
                    LogHelper.LogError(Log, "Error occured in  GetRequisitionLineItemBasicDetails Method in RequisitionRestService", ex);
                    throw;
                }
                finally
                {
                    //check and close the channel
                    if (objP2PDocumentServiceChannel != null && objP2PDocumentServiceChannel.State != CommunicationState.Closed)
                    {
                        objP2PDocumentServiceChannel.Close();
                    }
                    else if (objP2PDocumentServiceChannel != null) objP2PDocumentServiceChannel.Abort();

                }
            }
            return result;

        }

        public bool AddTemplateItemInReq(long documentCode, string templateIds, List<KeyValuePair<long, decimal>> items, int itemType, string buIds)
        {
            #region Debug Logging
            if (Log.IsDebugEnabled)
            {
                Log.Debug(string.Concat("In AddTemplateItemInReq Method in RequisitionRestService ",
                " with parameters: documentCode = " + documentCode + "templateIds = " + templateIds + "template item ids =" + items.ToJSON() + " itemTypes =" + Convert.ToString(itemType) + " buIds = " + buIds));
            }
            #endregion
            var result = false;

            var objP2PDocumentServiceChannel = GepServiceManager.GetInstance.CreateChannel<IRequisitionServiceChannel>(MultiRegionConfig.GetConfig(CloudConfig.RequisitionServiceURL));
            UserExecutionContext userExecutionContext = P2P.Req.RestService.ExceptionHelper.GetExecutionContext;

            try
            {
                using (new OperationContextScope(objP2PDocumentServiceChannel))
                {

                    var objMhg = new MessageHeader<UserExecutionContext>(userExecutionContext);
                    var untypedmh = objMhg.GetUntypedHeader("GepCustomHeader", "Gep.Cumulus");
                    OperationContext.Current.OutgoingMessageHeaders.Add(untypedmh);

                    MessageHeader<string> objMhgAuth = new MessageHeader<string>(Token);
                    System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
                    OperationContext.Current.OutgoingMessageHeaders.Add(authorization);



                    result = objP2PDocumentServiceChannel.AddTemplateItemInReq(documentCode, templateIds, items, itemType, buIds);
                    objP2PDocumentServiceChannel.Close();
                }

            }
            catch (CommunicationException commFaultEx)
            {
                LogHelper.LogError(Log, "Error occured in  AddTemplateItemInReq Method in RequisitionRestService", commFaultEx);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in  AddTemplateItemInReq Method in RequisitionRestService", ex);
                throw;
            }
            finally
            {
                //check and close the channel
                if (objP2PDocumentServiceChannel != null && objP2PDocumentServiceChannel.State != CommunicationState.Closed)
                {
                    objP2PDocumentServiceChannel.Close();
                }
                else if (objP2PDocumentServiceChannel != null) objP2PDocumentServiceChannel.Abort();

            }

            return result;

        }

        public Dictionary<string, string> SaveOfflineApprovalDetails(long contactCode, long documentCode, decimal documentAmount, string fromCurrency, string toCurrency, WorkflowInputEntities workflowEntity, long headerOrgEntityCode)
        {
            try
            {
                ProxyRequisitionService proxyRequisitionService = new ProxyRequisitionService(P2P.Req.RestService.ExceptionHelper.GetExecutionContext, Token);
                return proxyRequisitionService.SaveOfflineApprovalDetails(contactCode, documentCode, documentAmount, fromCurrency, toCurrency, workflowEntity, headerOrgEntityCode);
            }
            catch (CommunicationException commFaultEx)
            {
                LogHelper.LogError(Log, "Error occured in  AddTemplateItemInReq Method in RequisitionRestService", commFaultEx);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in  AddTemplateItemInReq Method in RequisitionRestService", ex);
                throw;
            }
            return new Dictionary<string, string>();
        }
        public bool UpdateRequisitionLineStatusonRFXCreateorUpdate(long documentCode, List<long> p2pLineItemId, DocumentType docType, bool IsDocumentDeleted = false)
        {
            try
            {
                ProxyRequisitionService proxyRequisitionService = new ProxyRequisitionService(P2P.Req.RestService.ExceptionHelper.GetExecutionContext, Token);
                return proxyRequisitionService.UpdateRequisitionLineStatusonRFXCreateorUpdate(documentCode, p2pLineItemId, docType, IsDocumentDeleted);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in  UpdateRequisitionLineStatusonRFXCreateorUpdate Method in RequisitionRestService", ex);
                throw;
            }
        }

        public void ReviewCallBackMethod(string eventName, long documentCode, string documentStatus, int wfDocTypeId, long contactCode, string userStatus, string approvalType, string returnEntity)
        {
            LogNewRelic("Inside ReviewCallBackMethod at LN - 2719", "ReviewCallBackMethod", documentCode);

            #region Debug Logging

            if (Log.IsDebugEnabled)
            {
                Log.Debug(string.Concat("In ReviewCallBackMethod Method in RequisitionRestService of class ", typeof(Requisition).ToString(), " with parameter: documentCode = " + documentCode + ", documentStatus=" + documentStatus));
            }

            #endregion

            var userExecutionContext = P2P.Req.RestService.ExceptionHelper.GetExecutionContext;
            long wFOrderId = 0;
            string jwtToken = Token;
            var currentPrincipal = System.Threading.Thread.CurrentPrincipal;
            Task.Factory.StartNew((state) =>
            {
                System.Threading.Thread.CurrentPrincipal = currentPrincipal;
                P2P.RestService.P2PDocumentRestService objDocSer = new P2P.RestService.P2PDocumentRestService() { jwtToken = jwtToken };
                List<ReviewerDetails> reviewersList = new List<ReviewerDetails>();
                ProxyRequisitionService proxyRequisitionService = new ProxyRequisitionService((UserExecutionContext)state, jwtToken);
                ProxyP2PDocumentService proxyP2PDocumentService = new ProxyP2PDocumentService((UserExecutionContext)state, jwtToken);
                List<ReviewerDetails> pendingReviewersList = new List<ReviewerDetails>();
                Requisition requisition = new Requisition();
                var documentStatusInfo = (DocumentStatus)Enum.Parse(typeof(DocumentStatus), documentStatus);

                // EnableAsycCallback
                bool EnableAsycCallback = false;
                ProxyP2PCommonService proxyP2PCommonService = new ProxyP2PCommonService((UserExecutionContext)state, jwtToken);
                string enableAsycCallback = proxyP2PCommonService.GetSettingsValueByKey(P2PDocumentType.Requisition, "EnableAsycCallback", ((UserExecutionContext)state).ContactCode, 107);
                if (!string.IsNullOrEmpty(enableAsycCallback) && enableAsycCallback.ToLower() == "true")
                {
                    EnableAsycCallback = true;
                }


                string EnableRequisitionDocumentServiceHelper = proxyP2PCommonService.GetSettingsValueByKey(P2PDocumentType.Requisition, "EnableRequisitionDocumentServiceHelper", userExecutionContext.ContactCode, 107);
                if (!string.IsNullOrEmpty(EnableRequisitionDocumentServiceHelper) && EnableRequisitionDocumentServiceHelper.ToLower() == "true")
                {
                    DocumentRestServiceHelper helper = new DocumentRestServiceHelper(jwtToken);
                    reviewersList = helper.GetReviewersList(documentCode, wfDocTypeId, (UserExecutionContext)state);
                } 
                else
                {
                    reviewersList = objDocSer.GetReviewersList(documentCode, wfDocTypeId, (UserExecutionContext)state);
                }
                

                if (eventName == P2PConstants.EVENT_ONSUBMIT || (documentStatusInfo == DocumentStatus.Rejected && eventName == P2PConstants.EVENT_INTERMEDIATE) || documentStatusInfo == DocumentStatus.Accepted && eventName == P2PConstants.EVENT_INTERMEDIATE)
                {
                    requisition = proxyRequisitionService.GetRequisitionBasicDetailsById(documentCode, userExecutionContext.ContactCode, 0);                    
                }

                if (eventName == P2PConstants.EVENT_ONSUBMIT || (documentStatusInfo == DocumentStatus.Rejected && eventName == P2PConstants.EVENT_INTERMEDIATE))
                {
                    List<TaskActionDetails> lstTasksAction = new List<TaskActionDetails>();
                    var taskInformation = new TaskInformation();

                    if (eventName == P2PConstants.EVENT_ONSUBMIT)
                    {
                        lstTasksAction.Add(TaskHelper.CreateActionDetails(ActionKey.SentForReview, TaskConstants.SENT_FOR_REVIEW));
                        lstTasksAction.Add(TaskHelper.CreateActionDetails(ActionKey.Edit, TaskConstants.EDIT));
                        taskInformation = TaskHelper.CreateTaskObject(documentCode, requisition.CreatedBy, lstTasksAction, true, false, ((UserExecutionContext)state).BuyerPartnerCode, ((UserExecutionContext)state).CompanyName);
                    }
                    else
                    {
                        lstTasksAction.Add(TaskHelper.CreateActionDetails(ActionKey.Edit, TaskConstants.EDIT));
                        taskInformation = TaskHelper.CreateTaskObject(documentCode, requisition.CreatedBy, lstTasksAction, false, false, ((UserExecutionContext)state).BuyerPartnerCode, ((UserExecutionContext)state).CompanyName);
                    }

                    proxyP2PDocumentService.SaveTaskActionDetails(taskInformation);                    
                }

                string queryString = UrlEncryptionHelper.EncryptURL("dc=" + documentCode.ToString(CultureInfo.InvariantCulture) + "&bpc=" +
                                                                        ((UserExecutionContext)state).BuyerPartnerCode.ToString(CultureInfo.InvariantCulture)) + "&oloc=" + (int)SubAppCodes.P2P;

                reviewersList.Where(e => !e.IsProcessed && e.Status == (int)ReviewStatus.ReviewPending).ToList().ForEach(reviewer => pendingReviewersList.Add(reviewer));

                if (documentStatusInfo == DocumentStatus.ReviewPending && reviewersList.Any())
                {
                    proxyRequisitionService.SendNotificationForRequisitionReview(documentCode, pendingReviewersList, (eventName == P2PConstants.EVENT_ONSUBMIT) ? new List<ReviewerDetails>() : reviewersList.Where(p => p.Status == (int)ReviewStatus.Accepted).ToList(), eventName, documentStatusInfo, approvalType, EnableAsycCallback);                    
                }

                LogNewRelic("Before ProcessReminderNotificationsForReview LN 2803, approvalType:" + approvalType, "ReviewCallBackMethod", documentCode);

                ProcessReminderNotificationsForReview(approvalType,wFOrderId, reviewersList, pendingReviewersList, documentCode, documentStatus, wfDocTypeId, (UserExecutionContext)state, jwtToken);

                LogNewRelic("After ProcessReminderNotificationsForReview LN 2807, approvalType:" + approvalType, "ReviewCallBackMethod", documentCode);

                if (documentStatusInfo == DocumentStatus.Rejected && reviewersList.Any(p => p.Status == (int)ReviewStatus.Rejected))
                {   
                    if (!string.IsNullOrEmpty(EnableRequisitionDocumentServiceHelper) && EnableRequisitionDocumentServiceHelper.ToLower() == "true")
                    {
                        DocumentRestServiceHelper helper = new DocumentRestServiceHelper(Token);
                        helper.DeleteAllTasksForReviewDocument(documentCode, wfDocTypeId, reviewersList, (UserExecutionContext)state);
                    }
                    else
                    {
                        objDocSer.DeleteAllTasksForReviewDocument(documentCode, wfDocTypeId, reviewersList, (UserExecutionContext)state);                        
                    }
                    proxyRequisitionService.SendNotificationForReviewRejectedRequisition(documentCode, reviewersList.Where(p => p.Status == (int)ReviewStatus.Rejected).First(), reviewersList.Where(p => p.Status != (int)ReviewStatus.Rejected).ToList(), queryString, EnableAsycCallback);
                }

                LogNewRelic("After ProcessReminderNotificationsForReview LN 2822, eventName:" + eventName + " userStatus:" + userStatus, "ReviewCallBackMethod", documentCode);
                if (eventName == P2PConstants.EVENT_INTERMEDIATE && userStatus == "Accepted" && reviewersList.Any(p => p.ReviewerId == contactCode && p.Status == (int)ReviewStatus.Accepted))
                {
                    LogNewRelic("Before SendNotificationForReviewAcceptedRequisition LN 2826, eventName:" + eventName + " userStatus:" + userStatus, "ReviewCallBackMethod", documentCode);
                    proxyRequisitionService.SendNotificationForReviewAcceptedRequisition(documentCode, reviewersList.First(p => p.ReviewerId == contactCode && p.Status == (int)ReviewStatus.Accepted), queryString, EnableAsycCallback);
                    LogNewRelic("After SendNotificationForReviewAcceptedRequisition LN 2828, eventName:" + eventName + " userStatus:" + userStatus, "ReviewCallBackMethod", documentCode);
                }

                LogNewRelic("After SendNotificationForReviewAcceptedRequisition LN 2831, eventName:" + eventName  + "  documentStatusInfo:" + documentStatusInfo, "ReviewCallBackMethod", documentCode);
                if (eventName == P2PConstants.EVENT_INTERMEDIATE && documentStatusInfo == DocumentStatus.Accepted)
                {
                    LogNewRelic("Before SendReviewedRequisitionForApproval LN 2834, eventName:" + eventName + "  documentStatusInfo:" + documentStatusInfo, "ReviewCallBackMethod", documentCode);
                    proxyRequisitionService.SendReviewedRequisitionForApproval(requisition);
                    LogNewRelic("After SendReviewedRequisitionForApproval LN 2836, eventName:" + eventName + "  documentStatusInfo:" + documentStatusInfo, "ReviewCallBackMethod", documentCode);
                }

                if (eventName == P2PConstants.EVENT_INTERMEDIATE && (documentStatusInfo == DocumentStatus.Accepted || documentStatusInfo == DocumentStatus.Rejected))
                {
                    ConsumeReleaseCapitalBudget(documentCode, documentStatusInfo, (UserExecutionContext)state,false, jwtToken);                    
                }
                if (reviewersList.Any())
                {                    
                    if (userStatus != "Rejected")
                    {   
                        if (!string.IsNullOrEmpty(EnableRequisitionDocumentServiceHelper) && EnableRequisitionDocumentServiceHelper.ToLower() == "true")
                        {
                            DocumentRestServiceHelper helper = new DocumentRestServiceHelper(Token);
                            helper.DeleteAllTasksForReviewDocument(documentCode, wfDocTypeId, reviewersList, (UserExecutionContext)state);
                            if (reviewersList.Where(e => !e.IsProcessed && e.Status == (int)ReviewStatus.ReviewPending).Count() > 0)
                            {
                                helper.AddTasksForReviewDocument(documentCode, wfDocTypeId, reviewersList.Where(e => !e.IsProcessed && e.Status == (int)ReviewStatus.ReviewPending).ToList(), (UserExecutionContext)state);
                            }
                        }
                        else
                        {
                            objDocSer.DeleteAllTasksForReviewDocument(documentCode, wfDocTypeId, reviewersList, (UserExecutionContext)state);
                            if (reviewersList.Where(e => !e.IsProcessed && e.Status == (int)ReviewStatus.ReviewPending).Count() > 0)
                            {
                                objDocSer.AddTasksForReviewDocument(documentCode, wfDocTypeId, reviewersList.Where(e => !e.IsProcessed && e.Status == (int)ReviewStatus.ReviewPending).ToList(), (UserExecutionContext)state);
                            }
                        }
                    }
                }

                if (eventName == P2PConstants.EVENT_ONSUBMIT
                    || (userStatus == "Accepted" && eventName == P2PConstants.EVENT_INTERMEDIATE && documentStatusInfo == DocumentStatus.Accepted)
                    || (userStatus == "ReviewPending" && eventName == P2PConstants.EVENT_INTERMEDIATE && documentStatusInfo == DocumentStatus.ReviewPending)
                    || (userStatus == "Rejected" && eventName == P2PConstants.EVENT_INTERMEDIATE && documentStatusInfo == DocumentStatus.Rejected))
                {
                    DocumentLOBDetails documentLob = GetDocumentLOB(documentCode, (UserExecutionContext)state, jwtToken);
                    string enableReviewAuditLog = (proxyP2PCommonService.GetSettingsValueByKeyAndLOB(P2PDocumentType.Requisition, "EnableReviewAuditLog", ((UserExecutionContext)state).ContactCode, 107, documentLob.EntityDetailCode));
                    if (enableReviewAuditLog.ToLower() == "true")
                    {
                        PushRequisitionAudit(documentCode, (UserExecutionContext)state, eventName, contactCode, userStatus, jwtToken);                        
                    }
                }
            }, userExecutionContext);
        }

        public void PushRequisitionAudit(long documentCode, UserExecutionContext userExecutionContext, string eventName, long contactCode, string userStatus, string jwtToken)
        {
            try
            {                
                ReviewerDetails adhocReview = CheckIsAdhocReviewerInternal(contactCode, documentCode, userExecutionContext, jwtToken);
                
                NewProxyRequisitionService prxyReqService = new NewProxyRequisitionService(userExecutionContext, jwtToken);
                var objReq = prxyReqService.GetRequisitionDisplayDetails(documentCode);
                
                var JItems = new JObject();
                var JReq = JObject.FromObject(objReq);
                for (int i = 0; i < objReq.items.Count(); i++)
                {
                    JItems.Add(objReq.items[i].id.ToString(), JReq["items"][i]);
                }
                JReq["items"] = JItems;

                var reviewDetails = new JObject();
                reviewDetails.Add("id", objReq.items.Count > 0 ? objReq.items[0].lastModifiedBy.id : 0);
                reviewDetails.Add("name", objReq.items.Count > 0 ? objReq.items[0].lastModifiedBy.name : string.Empty);
                reviewDetails.Add("reviewedOn", objReq.lastModifiedOn != null ? objReq.lastModifiedOn : DateTime.Now);
                reviewDetails.Add("userStatus", userStatus);
                JReq.Add("ReviewDetails", reviewDetails);

                if (eventName == P2PConstants.EVENT_ONSUBMIT)
                {
                    var submitDetails = new JObject();
                    submitDetails.Add("id", objReq.items.Count > 0 ? objReq.items[0].lastModifiedBy.id : 0);
                    submitDetails.Add("name", objReq.items.Count > 0 ? objReq.items[0].lastModifiedBy.name : string.Empty);
                    submitDetails.Add("submittedOn", objReq.lastModifiedOn != null ? objReq.lastModifiedOn : DateTime.Now);
                    JReq.Add("SubmitDetails", submitDetails);
                } else if (adhocReview.ReviewerId > 0) {
                    var adhocReviewDetails = new JObject();
                    adhocReviewDetails.Add("id", adhocReview.ReviewerId);
                    adhocReviewDetails.Add("name", adhocReview.Name);
                    adhocReviewDetails.Add("AddedOn", adhocReview.StatusDate);
                    JReq.Add("AdhocReviewDetails", adhocReviewDetails);
                }

                JReq.Add("BuyerPartnerCode", userExecutionContext.BuyerPartnerCode);

                var pushRequisitionAuditUrl = MultiRegionConfig.GetConfig(CloudConfig.APIMBaseURL) + "/RequisitionAudit/PushRequisitionAudit";
                var pollyHttpClient = new ResilientHttpClient(this, JsonConvert.SerializeObject(userExecutionContext));

                string strSubscriptionKey = MultiRegionConfig.GetConfig(CloudConfig.APIMSubscriptionKey);
                pollyHttpClient.Headers = new Dictionary<string, string>();
                pollyHttpClient.Headers.Add("Ocp-Apim-Subscription-Key", strSubscriptionKey);
                pollyHttpClient.Headers.Add("RegionID", MultiRegionConfig.PrimaryRegion);
                pollyHttpClient.Headers.Add("bpc", userExecutionContext.BuyerPartnerCode.ToString());
                pollyHttpClient.Headers.Add("Authorization", jwtToken);
                var httpCall = pollyHttpClient.Post(pushRequisitionAuditUrl, JReq, "PushRequisitionAudit", true);
                var httpCallResult = httpCall.Content.ReadAsStringAsync().Result;                

                if (!httpCall.IsSuccessStatusCode)
                {
                    var httpException = new Exception("HTTP Status Code Received: " + httpCall.StatusCode.ToString(), new Exception(httpCallResult));
                    LogHelper.LogError(Log, "Error occured in PushRequisitionAudit Method in RequisitionRestService", httpException);
                    throw httpException;
                }
            }
            catch (Exception ex)
            {                
                var eventAttributes = new Dictionary<string, object>();
                eventAttributes = new Dictionary<string, object>();
                eventAttributes.Add("RequisitionAuditException", ex.Message);
                NewRelic.Api.Agent.NewRelic.RecordCustomEvent("RequisitionAuditExceptionLog", eventAttributes);
                LogHelper.LogError(Log, "Error Occured in PushRequisitionAudit method", ex);
                throw;
            }
        }

        public DocumentIntegrationResults CreateRequisitionFromS2C(DocumentIntegrationEntity objDocumentIntegrationEntity)
        {
            try
            {                
                ProxyRequisitionService proxyRequisitionService = new ProxyRequisitionService(P2P.Req.RestService.ExceptionHelper.GetExecutionContext, Token);
                return proxyRequisitionService.CreateRequisitionFromS2C(objDocumentIntegrationEntity);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in  CreateRequisitionFromS2C Method in RequisitionRestService", ex);
                throw;
            }
        }

        public string GetRequisitionDisplayDetails(Int64 id)
        {
            var result = string.Empty;
            try
            {
                NewProxyRequisitionService prxyReqService = new NewProxyRequisitionService(P2P.Req.RestService.ExceptionHelper.GetExecutionContext, Token);
                result = prxyReqService.GetRequisitionDisplayDetails(id).ToJSON();
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error Occured in GetRequisitionDisplayDetails method", ex);
                throw;
            }
            return result;
        }

        public string GetAllRequisitionDetailsByRequisitionId(long requisitionId)
        {
            var result = string.Empty;
            try
            {
                ProxyRequisitionService prxyReqService = new ProxyRequisitionService(P2P.Req.RestService.ExceptionHelper.GetExecutionContext, Token);
                result = prxyReqService.GetAllRequisitionDetailsByRequisitionId(requisitionId).ToJSON();
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error Occured in GetRequisitionDisplayDetails method", ex);
                throw;
            }
            return result;
        }

        public string GetDocumentSplitAccountingFields(int docType, int levelType, long LOBId = 0, int structureId = 0)
        {
            string strResult = string.Empty;
            try
            {
                ProxyRequisitionService prxyReqService = new ProxyRequisitionService(P2P.Req.RestService.ExceptionHelper.GetExecutionContext, Token);
                strResult = prxyReqService.GetAllSplitAccountingFields((P2PDocumentType)docType, (LevelType)levelType, structureId, LOBId).ToJSON();

            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error Occured in GetDocumentSplitAccountingFields method", ex);
                throw;
            }
            return strResult;
        }

        public ReviewerDetails CheckIsAdhocReviewer(long contactCode, long documentCode, UserExecutionContext userExecutionContext)
        {
            return CheckIsAdhocReviewerInternal(contactCode, documentCode, userExecutionContext, Token);
        }

        public ReviewerDetails CheckIsAdhocReviewerInternal(long contactCode, long documentCode, UserExecutionContext userExecutionContext, string jwtToken)
        {
            ReviewerDetails adhocReviewer = null;
            LogNewRelic("Inside CheckIsAdhocReviewer at LN - 3013", "CheckIsAdhocReviewerInternal", documentCode);
            var getAdhocReviewerEndpointUrl = MultiRegionConfig.GetConfig(CloudConfig.WorkFlowRestURL) + "/CheckIsAdhocReviewer";
            var pollyHttpClient = new ResilientHttpClient(this, JsonConvert.SerializeObject(userExecutionContext));

            string strSubscriptionKey = MultiRegionConfig.GetConfig(CloudConfig.APIMSubscriptionKey);
            pollyHttpClient.Headers = new Dictionary<string, string>();
            pollyHttpClient.Headers.Add("Ocp-Apim-Subscription-Key", strSubscriptionKey);
            pollyHttpClient.Headers.Add("RegionID", MultiRegionConfig.PrimaryRegion);
            pollyHttpClient.Headers.Add("bpc", userExecutionContext.BuyerPartnerCode.ToString());
            pollyHttpClient.Headers.Add("Authorization", jwtToken);

            var getAdhocReviewerPayload = new Dictionary<string, object>()
            {
                {"contactCode", contactCode}, {"documentCode", documentCode}
            };

            LogNewRelic("Inside CheckIsAdhocReviewerInternal at LN - 3029", "CheckIsAdhocReviewerInternal", documentCode);
            var httpCall = pollyHttpClient.Post(getAdhocReviewerEndpointUrl, getAdhocReviewerPayload, "CheckIsAdhocReviewer", true);
            var httpCallResult = httpCall.Content.ReadAsStringAsync().Result;

            LogNewRelic("Inside CheckIsAdhocReviewerInternal at LN - 3033", "CheckIsAdhocReviewerInternal", documentCode);
            if (httpCall.IsSuccessStatusCode)
            {
                var reviewDetailsSerialized = httpCallResult;
                var jsonSerializer = new JavaScriptSerializer();

                adhocReviewer = jsonSerializer.Deserialize<ReviewerDetails>(reviewDetailsSerialized);

                LogNewRelic("Inside CheckIsAdhocReviewer at LN - 3041", "CheckIsAdhocReviewer", documentCode);
            }
            else
            {
                //Outer exception contains HTTP status code, and inner exception HTTP error description
                var httpException = new Exception("HTTP Status Code Received: " + httpCall.StatusCode.ToString(), new Exception(httpCallResult));
                LogNewRelic("Inside CheckIsAdhocReviewer at LN - 3047", "CheckIsAdhocReviewerInternal Inside Exception part", documentCode);
                LogHelper.LogError(Log, "Error occured in CheckIsAdhocReviewerInternal Method in RequisitionRestService", httpException);
                throw httpException;
            }

            return adhocReviewer;
        }

        public string GetBasicSettingsValueByKeys(Dictionary<string, int> keys, long contactCode)
        {
            UserExecutionContext userExecutionContext = P2P.Req.RestService.ExceptionHelper.GetExecutionContext;
            ProxyP2PCommonService proxyP2PCommonService = new ProxyP2PCommonService(userExecutionContext, Token);
            string strResult = string.Empty;

            try
            {
                List<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();

                foreach (KeyValuePair<string, int> p in keys)
                {
                    string keyValue = proxyP2PCommonService.GetSettingsValueByKey((P2PDocumentType)p.Value, p.Key, contactCode, 107);
                    result.Add(new KeyValuePair<string, string>(p.Key, keyValue));
                }
                strResult = result.ToJSON();
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error Occured in GetSettingsValueByKeys method", ex);
                throw;
            }
            return strResult;
        }

        public void PushingReqToEventHub(long requisitionId, string consolidationAttributes, long contactCode, UserExecutionContext userExecutionContext = null)
        {
            if (userExecutionContext == null)
                userExecutionContext = P2P.Req.RestService.ExceptionHelper.GetExecutionContext;
            string jwtToken = Token;
            var currentPrincipal = System.Threading.Thread.CurrentPrincipal;
            Task.Factory.StartNew((state) =>
            {
                System.Threading.Thread.CurrentPrincipal = currentPrincipal;
                try
                {
                    LogHelper.LogError(Log, string.Concat("PushingReqToEventHub : PushingReqToEventHub in RequistionRestService Started At Requisition : " + requisitionId), new Exception { });
                    NewProxyRequisitionService prxyReqService = new NewProxyRequisitionService((UserExecutionContext)state, jwtToken);
                    var objReq = prxyReqService.GetRequisitionDisplayDetails(requisitionId);
                    string[] purchaseTypeDetails = consolidationAttributes.Split(',');
                    if (purchaseTypeDetails.Contains(objReq.purchaseTypeDesc))
                    {
                        EventRequest objRequest = new EventRequest();
                        objRequest.MessageHeader.BuyerPartnerCode = ((UserExecutionContext)state).BuyerPartnerCode;
                        objRequest.MessageHeader.MessageSenderContactCode = contactCode;
                        objRequest.MessageHeader.MessageType = "Requisition";
                        objRequest.MessageHeader.MessageSubType = "Requisition";
                        objRequest.MessageHeader.ProductVersion = "2.0";
                        objRequest.MessageHeader.JWToken = "Token1";
                        objRequest.MessageHeader.SourceSystem = "InterFace";
                        objRequest.AdditionalParams.Add("UserContext", (UserExecutionContext)state);
                        objRequest.MessageRequest = Convert.ToString(objReq.ToJSON());
                        ESSendMsgOutPut objSendMsgOutPut;
                        ESIntegrator.SendMessage(objRequest, out objSendMsgOutPut);
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.LogError(Log, string.Concat("Exception Program PushingReqToEventHub with parameters in RequistionRestService :",
                                                        ", consolidationAttributes = " + consolidationAttributes + ", documentCode " + requisitionId + " Contact Code :" + userExecutionContext.ContactCode), ex);
                    throw;
                }
            }, userExecutionContext);
        }

        public List<ApproverDetails> GetActionersDetails(long documentCode, int wfDocTypeId)
        {
            List<ApproverDetails> lstApproverDetails = new List<ApproverDetails>();
            UserExecutionContext userExecutionContext = P2P.Req.RestService.ExceptionHelper.GetExecutionContext;
            var getActionersListEndpointUrl = MultiRegionConfig.GetConfig(CloudConfig.WorkFlowRestURL) + "/GetActionersList";
            var pollyHttpClient = new ResilientHttpClient(this, JsonConvert.SerializeObject(userExecutionContext));

            string strSubscriptionKey = MultiRegionConfig.GetConfig(CloudConfig.APIMSubscriptionKey);
            pollyHttpClient.Headers = new Dictionary<string, string>();            
            pollyHttpClient.Headers.Add("Authorization", Token);

            var getActionersListPayload = new Dictionary<string, object>()
            {
                {"documentCode", documentCode},
                {"wfDocTypeId", wfDocTypeId }
            };

            var httpCall = pollyHttpClient.Post(getActionersListEndpointUrl, getActionersListPayload, "GetActionersDetails", true);
            var httpCallResult = httpCall.Content.ReadAsStringAsync().Result;

            if (httpCall.IsSuccessStatusCode)
            {
                var approverDetailsSerialized = httpCallResult;
                var jsonSerializer = new JavaScriptSerializer();

                lstApproverDetails = jsonSerializer.Deserialize<List<ApproverDetails>>(approverDetailsSerialized);
            }
            else
            {
                //Outer exception contains HTTP status code, and inner exception HTTP error description
                var httpException = new Exception("HTTP Status Code Received: " + httpCall.StatusCode.ToString(), new Exception(httpCallResult));

                LogHelper.LogError(Log, "Error occured in GetPastApprovalDetails Method in RequisitionRestService", httpException);
                throw httpException;
            }

            return lstApproverDetails;
        }

        public void PushingRequisitionToEventHub(long requisitionId, string consolidationAttributes, long contactCode, UserExecutionContext userExecutionContext = null)
        {
            PushingRequisitionToEventHubInternal(requisitionId, consolidationAttributes, contactCode, Token, userExecutionContext);
        }

        private void PushingRequisitionToEventHubInternal(long requisitionId, string consolidationAttributes, long contactCode, string jwtToken, UserExecutionContext userExecutionContext = null)
        {
            if (userExecutionContext == null)
                userExecutionContext = P2P.Req.RestService.ExceptionHelper.GetExecutionContext;


            try
            {
                LogHelper.LogError(Log, string.Concat("PushingRequisitionToEventHub : PushingRequisitionToEventHub in RequistionRestService Started At Requisition : " + requisitionId + " Contact Code :" + userExecutionContext.ContactCode), new Exception { });
                NewProxyRequisitionService prxyReqService = new NewProxyRequisitionService(userExecutionContext, Token);
                var objReq = prxyReqService.GetRequisitionDisplayDetails(requisitionId);
                string[] purchaseTypeDetails = consolidationAttributes.Split(',');
                if (purchaseTypeDetails.Contains(objReq.purchaseTypeDesc))
                {
                    EventRequest objRequest = new EventRequest();
                    objRequest.MessageHeader.BuyerPartnerCode = userExecutionContext.BuyerPartnerCode;
                    objRequest.MessageHeader.MessageSenderContactCode = contactCode;
                    objRequest.MessageHeader.MessageType = "Requisition";
                    objRequest.MessageHeader.MessageSubType = "Requisition";
                    objRequest.MessageHeader.ProductVersion = "2.0";
                    objRequest.MessageHeader.JWToken = jwtToken;
                    objRequest.MessageHeader.SourceSystem = "Requisition";
                    objRequest.AdditionalParams.Add("UserContext", userExecutionContext);
                    objRequest.MessageRequest = Convert.ToString(objReq.ToJSON());
                    objRequest.MessageHeader.MessageId = Guid.NewGuid();

                    var serviceurl = MultiRegionConfig.GetConfig(CloudConfig.APIMBaseURL) + MultiRegionConfig.GetConfig(CloudConfig.ESIntegratorURL);
                    CreateHttpWebRequest(serviceurl, userExecutionContext, jwtToken);
                    var result = GetHttpWebResponse(objRequest);
                    LogHelper.LogError(Log, string.Concat("PushingRequisitionToEventHub : PushingRequisitionToEventHub in RequistionRestService returns result : " + result), new Exception { });
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, string.Concat("Exception Program PushingRequisitionToEventHub with parameters in RequistionRestService :",
                                                    ", consolidationAttributes = " + consolidationAttributes + ", documentCode " + requisitionId + " Contact Code :" + userExecutionContext.ContactCode), ex);
                throw;
            }
        }

        private void CreateHttpWebRequest(string strURL, UserExecutionContext userExecutionContext, string jwtToken)
        {
            req = WebRequest.Create(strURL) as HttpWebRequest;
            req.Method = "POST";
            req.ContentType = @"application/json";

            NameValueCollection nameValueCollection = new NameValueCollection();
            userExecutionContext.UserName = "";
            string userContextJson = userExecutionContext.ToJSON();
            nameValueCollection.Add("UserExecutionContext", userContextJson);
            string strSubscriptionKey = MultiRegionConfig.GetConfig(CloudConfig.APIMSubscriptionKey);
            nameValueCollection.Add("Ocp-Apim-Subscription-Key", strSubscriptionKey);
            nameValueCollection.Add("Authorization", jwtToken);
            req.Headers.Add(nameValueCollection);
        }

        private string GetHttpWebResponse(EventRequest odict)
        {
            JavaScriptSerializer JSrz = new JavaScriptSerializer();
            var data = JSrz.Serialize(odict);
            var byteData = Encoding.UTF8.GetBytes(data);


            req.ContentLength = byteData.Length;
            using (Stream stream = req.GetRequestStream())
            {
                stream.Write(byteData, 0, byteData.Length);
            }

            string result = null;
            using (HttpWebResponse resp = req.GetResponse() as HttpWebResponse)
            {
                using (StreamReader reader = new StreamReader(resp.GetResponseStream()))
                {
                    result = reader.ReadToEnd();
                }
            }
            return result;
        }

        public DocumentLOBDetails GetDocumentLOB(long documentCode, UserExecutionContext userExecutionContext, string jwtToken)
        {
            if (userExecutionContext == null)
                userExecutionContext = P2P.Req.RestService.ExceptionHelper.GetExecutionContext;
            try
            {
                NewProxyRequisitionService prxyReqService = new NewProxyRequisitionService(userExecutionContext, jwtToken);
                return prxyReqService.GetDocumentLOB(documentCode);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, string.Concat("Exception Program GetDocumentLOB with parameters in RequistionRestService :",
                                                    ", documentCode = " + documentCode), ex);
                throw;
            }
        }
        public string[] convertToStringArray(string settingValue)
        {
            string[] result = new string[] { "0" };
            if (!string.IsNullOrEmpty(settingValue))
            {
                settingValue = settingValue.ToLower();
                string[] enableFileManagerWebApiCall = settingValue.Split(',');
                result = enableFileManagerWebApiCall.Select(a => a.Trim()).ToArray();
            }

            return result;
        }
        public bool SaveReminderNotifications(long documentCode, List<long> contactCodes, int wfDocTypeId, long wfOrderId, string documentStatus, UserExecutionContext userExecutionContext, string jwtToken, long lob = 0, long entityDetailCode = 0)
        {
            ResilientHttpClient httpClient = new ResilientHttpClient(this, Newtonsoft.Json.JsonConvert.SerializeObject(userExecutionContext));

            string strSubscriptionKey = MultiRegionConfig.GetConfig(CloudConfig.APIMSubscriptionKey);
            httpClient.Headers = new Dictionary<string, string>();
            httpClient.Headers.Add("Ocp-Apim-Subscription-Key", strSubscriptionKey);
            httpClient.Headers.Add("RegionID", MultiRegionConfig.PrimaryRegion);
            httpClient.Headers.Add("bpc", userExecutionContext.BuyerPartnerCode.ToString());
            httpClient.Headers.Add("Authorization", jwtToken);

            bool isSaved = false;
            Dictionary<string, object> objDict = new Dictionary<string, object>();
            objDict.Add("documentCode", documentCode);
            objDict.Add("contactCodes", contactCodes);
            objDict.Add("wfDocTypeId", wfDocTypeId);
            objDict.Add("wfOrderId", wfOrderId);
            objDict.Add("reminderType", documentStatus);
            objDict.Add("lob", lob);
            objDict.Add("entityDetailCode", entityDetailCode);

            var serviceUrl = MultiRegionConfig.GetConfig(CloudConfig.WorkFlowRestURL) + "/SaveReminderNotifications";
            var result = httpClient.Post<Dictionary<string, object>>(serviceUrl, objDict, "SaveReminderNotifications");
            if (result != null)
            {
                if (result.IsSuccessStatusCode)
                {
                    var finalresult = result.Content.ReadAsStringAsync().Result;
                    var jSrz = new JavaScriptSerializer();
                    isSaved = jSrz.Deserialize<bool>(finalresult);
                }
                else
                {
                    Exception objException = new Exception();
                    LogHelper.LogError(Log, string.Concat("SaveReminderNotifications Workflow URL response returns false ", documentCode), objException);
                }
            }
            return isSaved;
        }

        public bool DeleteReminderNotifications(long documentCode, int wfDocTypeId, List<long> contactCodes, long wfOrderId,UserExecutionContext userExecutionContext, string jwtToken, bool deleteAllReminders = false)
        {
            ResilientHttpClient httpClient = new ResilientHttpClient(this, Newtonsoft.Json.JsonConvert.SerializeObject(userExecutionContext));

            string strSubscriptionKey = MultiRegionConfig.GetConfig(CloudConfig.APIMSubscriptionKey);
            httpClient.Headers = new Dictionary<string, string>();
            httpClient.Headers.Add("Ocp-Apim-Subscription-Key", strSubscriptionKey);
            httpClient.Headers.Add("RegionID", MultiRegionConfig.PrimaryRegion);
            httpClient.Headers.Add("bpc", userExecutionContext.BuyerPartnerCode.ToString());
            httpClient.Headers.Add("Authorization", jwtToken);

            bool isDeleted = false;
            Dictionary<string, object> objDict = new Dictionary<string, object>();
            objDict.Add("documentCode", documentCode);
            objDict.Add("wfdocTypeId", wfDocTypeId);
            objDict.Add("contactCodes", contactCodes);
            objDict.Add("wfOrderId", wfOrderId);
            objDict.Add("deleteAllReminders", deleteAllReminders);

            var serviceUrl = MultiRegionConfig.GetConfig(CloudConfig.WorkFlowRestURL) + "/DeleteReminderNotifications";
            var result = httpClient.Post<Dictionary<string, object>>(serviceUrl, objDict, "DeleteReminderNotifications");
            if (result != null)
            {
                if (result.IsSuccessStatusCode)
                {
                    var finalresult = result.Content.ReadAsStringAsync().Result;
                    var jSrz = new JavaScriptSerializer();
                    isDeleted = jSrz.Deserialize<bool>(finalresult);
                }
                else
                {
                    Exception objException = new Exception();
                    LogHelper.LogError(Log, string.Concat("DeleteReminderNotifications Workflow URL response returns false ", documentCode), objException);
                }
            }
            return isDeleted;
        }

        public void ProcessReminderNotifications(string approvalType, long wFOrderId, List<ApproverDetails> lstActioners, List<int> lstHierarchyIds, List<ApproverDetails> lstPendingApprovers, long documentCode, string documentStatus, int wfDocTypeId, UserExecutionContext userExecutionContext, string jwtToken)
        {
            try
            {
                var documentStatusInfo = (DocumentStatus)Enum.Parse(typeof(DocumentStatus), documentStatus);
                ProxyP2PCommonService proxyP2PCommonService = new ProxyP2PCommonService(userExecutionContext, jwtToken);
                string settingVal = proxyP2PCommonService.GetSettingsValueByKey(P2PDocumentType.Requisition, "EnableReminderFramework", userExecutionContext.ContactCode, (int)SubAppCodes.P2P);
                string[] enableReminderFramwork = convertToStringArray(settingVal);

                if ((enableReminderFramwork.Contains(Convert.ToString((int)EnableReminderFramework.EnableReminderForHR))&& approvalType == "HR") ||
                      (enableReminderFramwork.Contains(Convert.ToString((int)EnableReminderFramework.EnableReminderForPool)) && approvalType == "Pool") ||
                      (enableReminderFramwork.Contains(Convert.ToString((int)EnableReminderFramework.EnableReminderForGroup)) && approvalType == "ApprovalGroup") ||
                      (enableReminderFramwork.Contains(Convert.ToString((int)EnableReminderFramework.EnableReminderForUserDefined)) && approvalType == "UserDefinedApproval") ||
                      (enableReminderFramwork.Contains(Convert.ToString((int)EnableReminderFramework.EnableReminderForSpecificUser)) && approvalType == "SpecificUserApproval") ||
                       enableReminderFramwork.Contains(Convert.ToString((int)EnableReminderFramework.EnableReminderForAll)))
                {
                    if (documentStatusInfo == DocumentStatus.Rejected && lstActioners != null && lstActioners.Count > 0)
                    {
                        DeleteReminderNotifications(documentCode, wfDocTypeId, lstActioners.Select(x => x.ApproverId).ToList(), wFOrderId, userExecutionContext, jwtToken,true);
                    }
                    else if (documentStatusInfo == DocumentStatus.ApprovalPending && lstActioners.Any())
                    {
                        List<long> approvedApprovers = new List<long>();
                        lstActioners.Where(e => !e.IsProcessed && e.Status == 1 && e.WorkflowId != 11).ToList().ForEach(e => approvedApprovers.Add(e.ApproverId));
                        lstActioners.Where(e => !e.IsProcessed && e.Status == 1 && e.WorkflowId != 11 && e.ProxyApproverId > 0).ToList().ForEach(e => approvedApprovers.Add(e.ApproverId));

                        if (lstHierarchyIds != null && lstHierarchyIds.Count() > 0)
                        {
                            lstActioners.Where(e => !e.IsProcessed && e.Status == 2 && e.WorkflowId == 11 && e.SubIsProcessed == false && lstHierarchyIds.Contains(e.HierarchyId)).ToList().ForEach(e => lstPendingApprovers.Add(e));
                            lstActioners.Where(e => !e.IsProcessed && e.Status == 1 && e.WorkflowId == 11 && e.SubIsProcessed == false && lstHierarchyIds.Contains(e.HierarchyId)).ToList().ForEach(e => approvedApprovers.Add(e.ApproverId));
                            lstActioners.Where(e => !e.IsProcessed && e.Status == 1 && e.WorkflowId == 11 && e.SubIsProcessed == false && lstHierarchyIds.Contains(e.HierarchyId) && e.ProxyApproverId > 0).ToList().ForEach(e => approvedApprovers.Add(e.ProxyApproverId));

                        }
                        else
                        {
                            lstActioners.Where(e => !e.IsProcessed && e.Status == 2 && e.WorkflowId == 11 && e.SubIsProcessed == false).ToList().ForEach(e => lstPendingApprovers.Add(e));
                            lstActioners.Where(e => !e.IsProcessed && e.Status == 1 && e.WorkflowId == 11 && e.SubIsProcessed == false).ToList().ForEach(e => approvedApprovers.Add(e.ApproverId));
                            lstActioners.Where(e => !e.IsProcessed && e.Status == 1 && e.WorkflowId == 11 && e.SubIsProcessed == false && e.ProxyApproverId > 0).ToList().ForEach(e => approvedApprovers.Add(e.ProxyApproverId));
                        }

                        var isSaved = false;
                        var proxySaved = false;
                        var proxyDeleted = false;
                        wFOrderId = lstPendingApprovers.Select(y => y.WFOrderId).FirstOrDefault();
                        var proxyContactCodes = lstPendingApprovers.Where(x => x.ProxyApproverId > 0 && (x.ProxyApproverType == 1 || x.ProxyApproverType == 6)).Select(s => s.ProxyApproverId).ToList();
                        var originalActioners = lstPendingApprovers.Where(x => x.ProxyApproverId > 0 && (x.ProxyApproverType == 1 || x.ProxyApproverType == 6)).Select(s => s.ApproverId).ToList();
                        var approverContacts = lstPendingApprovers.Select(x => x.ApproverId).ToList();
                        var proxyContacts = lstPendingApprovers.Where(s => s.ProxyApproverId > 0).Select(s => s.ProxyApproverId).ToList();
                        approverContacts.AddRange(proxyContacts);

                        if (approverContacts.Any() && wFOrderId > 0)
                            isSaved = SaveReminderNotifications(documentCode, approverContacts, wfDocTypeId, wFOrderId, documentStatus,userExecutionContext, jwtToken, 0, 0);

                        if (proxyContactCodes.Any() && wFOrderId > 0)
                        {
                            proxyDeleted = DeleteReminderNotifications(documentCode, wfDocTypeId, originalActioners, wFOrderId, userExecutionContext, jwtToken);
                            proxySaved = SaveReminderNotifications(documentCode, proxyContactCodes, wfDocTypeId, wFOrderId, documentStatus, userExecutionContext, jwtToken, 0, 0);
                        }

                        var isDeleted = false;
                        if (approvedApprovers.Any() && wFOrderId > 0)
                        {
                            isDeleted = DeleteReminderNotifications(documentCode, wfDocTypeId, approvedApprovers, wFOrderId, userExecutionContext, jwtToken);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, string.Concat("Error occured in ReminderNotifications in RequisitionRestService for DocumentCode ", documentCode), ex);
                throw;
            }
        }
        public void ProcessReminderNotificationsForReview(string approvalType, long wFOrderId, List<ReviewerDetails> reviewersList, List<ReviewerDetails> pendingReviewersList, long documentCode, string documentStatus, int wfDocTypeId, UserExecutionContext userExecutionContext, string jwtToken)
        {
            try
            {
                ProxyP2PCommonService proxyP2PCommonService = new ProxyP2PCommonService(userExecutionContext, jwtToken);
                var documentStatusInfo = (DocumentStatus)Enum.Parse(typeof(DocumentStatus), documentStatus);
                string settingVal = proxyP2PCommonService.GetSettingsValueByKey(P2PDocumentType.Requisition, "EnableReminderFramework", userExecutionContext.ContactCode, (int)SubAppCodes.P2P);
                string[] enableReminderFramwork = convertToStringArray(settingVal);

                if (enableReminderFramwork.Contains(Convert.ToString((int)EnableReminderFramework.EnableReminderForGroupReview)) && approvalType == "ReviewGroup")
                {
                    if (documentStatusInfo == DocumentStatus.Rejected && reviewersList != null && reviewersList.Count > 0)
                    {
                        DeleteReminderNotifications(documentCode, wfDocTypeId, reviewersList.Select(x => x.ReviewerId).ToList(), wFOrderId, userExecutionContext, jwtToken, true);
                    }
                    else if (documentStatusInfo == DocumentStatus.ReviewPending && reviewersList.Any())
                    {
                        List<long> acceptedReviewers = new List<long>();
                        reviewersList.Where(e => !e.IsProcessed && e.Status == (int)ReviewStatus.Accepted).ToList().ForEach(e => acceptedReviewers.Add(e.ReviewerId));
                        reviewersList.Where(e => !e.IsProcessed && e.Status == (int)ReviewStatus.Accepted && e.ProxyReviewerId > 0).ToList().ForEach(e => acceptedReviewers.Add(e.ProxyReviewerId));

                        var isSaved = false;
                        wFOrderId = pendingReviewersList.Select(y => y.WFOrderId).FirstOrDefault();
                        var reviewerContacts = pendingReviewersList.Select(x => x.ReviewerId).ToList();
                        var proxyContacts = pendingReviewersList.Where(s => s.ProxyReviewerId > 0).Select(s => s.ProxyReviewerId).ToList();
                        reviewerContacts.AddRange(proxyContacts);

                        LogHelper.LogError(Log, "Info: SaveReminder-Before Start " + reviewerContacts?.Count + "-Reviewer Contacts Count" + documentCode, new Exception());
                        if (reviewerContacts.Any() && wFOrderId > 0)
                            isSaved = SaveReminderNotifications(documentCode, reviewerContacts, wfDocTypeId, wFOrderId, documentStatus, userExecutionContext, jwtToken, 0, 0);
                        LogHelper.LogError(Log, "Info: SaveReminder -End" + isSaved + "-Reviewer Contacts" + documentStatus + "-documentStatus", new Exception());

                        var isDeleted = false;
                        if (acceptedReviewers.Any() && wFOrderId > 0)
                        {
                            isDeleted = DeleteReminderNotifications(documentCode, wfDocTypeId, acceptedReviewers, wFOrderId, userExecutionContext, jwtToken);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, string.Concat("Error occured in ReminderNotificationsForReview in RequisitionRestService for DocumentCode ", documentCode), ex);
                throw;
            }
        }

        public bool ConsumeReleaseCapitalBudget(long documentCode, DocumentStatus documentStatus, UserExecutionContext userExecutionContext,bool approve, string jwtToken)
        {
            bool result = false;
            try
            {
                ProxyP2PCommonService proxyP2PCommonService = new ProxyP2PCommonService(userExecutionContext, jwtToken);
                string enableCapitalBudget = proxyP2PCommonService.GetSettingsValueByKey(P2PDocumentType.None, "EnableCapitalBudget", userExecutionContext.ContactCode, 107);
                bool isReConsume = false;
                if (enableCapitalBudget.ToLower() == "yes")
                {
                    var capitalBudgetConsumption = NewP2PEntities.ConsumeCapitalBudget.None;
                    string enableCapitalBudgetConsumptionSetting = proxyP2PCommonService.GetSettingsValueByKey(P2PDocumentType.Requisition, "CapitalBudgetConsumption", userExecutionContext.ContactCode, 107);
                    if (!(string.IsNullOrEmpty(enableCapitalBudgetConsumptionSetting)))
                    {
                        capitalBudgetConsumption = (NewP2PEntities.ConsumeCapitalBudget)Convert.ToInt16(enableCapitalBudgetConsumptionSetting);
                    }
                    if (((capitalBudgetConsumption == NewP2PEntities.ConsumeCapitalBudget.ALL
                            || capitalBudgetConsumption == NewP2PEntities.ConsumeCapitalBudget.OnSubmit
                            || capitalBudgetConsumption == NewP2PEntities.ConsumeCapitalBudget.OnSubmitLastReviewerAccept
                            || capitalBudgetConsumption == NewP2PEntities.ConsumeCapitalBudget.OnSubmitLastApproverApprove
                            ) && !approve))
                        isReConsume = true;
                    if (((capitalBudgetConsumption == NewP2PEntities.ConsumeCapitalBudget.ALL
                        || capitalBudgetConsumption == NewP2PEntities.ConsumeCapitalBudget.OnSubmit
                        || capitalBudgetConsumption == NewP2PEntities.ConsumeCapitalBudget.OnSubmitLastReviewerAccept
                        || capitalBudgetConsumption == NewP2PEntities.ConsumeCapitalBudget.OnSubmitLastApproverApprove
                        || capitalBudgetConsumption == NewP2PEntities.ConsumeCapitalBudget.LastReviewerAccept
                        || capitalBudgetConsumption == NewP2PEntities.ConsumeCapitalBudget.LastReviewerAcceptLastApproverApprove
                        ) && approve))
                        isReConsume = true;

                    if ((capitalBudgetConsumption == NewP2PEntities.ConsumeCapitalBudget.ALL
                        || capitalBudgetConsumption == NewP2PEntities.ConsumeCapitalBudget.LastReviewerAcceptLastApproverApprove
                    ||(((documentStatus== DocumentStatus.Approved || documentStatus == DocumentStatus.Rejected) && approve && capitalBudgetConsumption == NewP2PEntities.ConsumeCapitalBudget.LastApproverApprove
                    || capitalBudgetConsumption == NewP2PEntities.ConsumeCapitalBudget.OnSubmitLastApproverApprove))
                    ||((documentStatus == DocumentStatus.Accepted || documentStatus == DocumentStatus.Rejected) && !approve && capitalBudgetConsumption == NewP2PEntities.ConsumeCapitalBudget.OnSubmitLastReviewerAccept
                    || capitalBudgetConsumption == NewP2PEntities.ConsumeCapitalBudget.LastReviewerAccept)))
                    {
                        ProxyRequisitionService proxyRequisitionService = new ProxyRequisitionService(userExecutionContext, jwtToken);
                        result = proxyRequisitionService.ConsumeReleaseCapitalBudget(documentCode, documentStatus, isReConsume);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, string.Concat("Error occured in ConsumeReleaseCapitalBudget in RequisitionRestService for DocumentCode ", documentCode), ex);
                throw;
            }
            return result;
        }

        public void RejectDocumentOperationBudget(long documentCode, DocumentType documentType, string groupText, UserExecutionContext _userExecutionContext, string jwtToken)
        {
            try
            {

                string serviceurl;
                ApprovalHelper objApprovalHelper = new ApprovalHelper() { userExecutionContext = _userExecutionContext };
                serviceurl = MultiRegionConfig.GetConfig(CloudConfig.WorkFlowRestURL) + "/ReceiveNotification";
                objApprovalHelper.CreateHttpWebRequest(serviceurl, jwtToken);

                Dictionary<string, object> odict = new Dictionary<string, object>();
                odict.Add("contactCode", _userExecutionContext.ContactCode);
                odict.Add("documentCode", documentCode);
                odict.Add("isApproved", false);
                odict.Add("documentTypeId", (int)documentType);

                string result = string.Empty;
                result = objApprovalHelper.GetHttpWebResponse(odict);

                var javaScriptSerializer = new JavaScriptSerializer();
                result = ((Dictionary<string, object>)javaScriptSerializer.DeserializeObject(result))["ReceiveNotificationResult"].ToString();
                string strMessage = string.Empty;
                var objSettingsService = new ProxyP2PCommonService(_userExecutionContext, jwtToken);
                CommentsGroup objCommentsGroup = new CommentsGroup();
                List<Comments> objComments = new List<Comments>();
                Comments comments = new Comments();
                objCommentsGroup.ObjectID = documentCode;
                objCommentsGroup.GroupText = groupText;
                objCommentsGroup.CreatedBy = _userExecutionContext.ContactCode;
                comments.CommentType = "2"; //Reject
                comments.AccessType = "1"; // Internal Users Only
                comments.IsDeleteEnable = true;
                comments.CommentText = objSettingsService.GetSettingsValueByKey(P2PDocumentType.None, "AutoRejectDocumentBudgetValidationComments", _userExecutionContext.ContactCode, 107);
                objComments.Add(comments);
                objCommentsGroup.Comment = objComments;
                switch (documentType)
                {
                    case DocumentType.Requisition:
                        objSettingsService.SaveComments(objCommentsGroup, P2PDocumentType.Requisition, 1);
                        break;
                    case DocumentType.PO:
                        objSettingsService.SaveComments(objCommentsGroup, P2PDocumentType.Order, 1);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, string.Concat("Error occured in RejectDocumentOperationBudget in RequisitionRestService for DocumentCode ", documentCode), ex);
                throw;
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
