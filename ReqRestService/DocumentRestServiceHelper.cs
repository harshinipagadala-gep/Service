using Gep.Cumulus.CSM.Entities;
using Gep.Cumulus.ExceptionManager;
using GEP.Cumulus.Documents.Entities;
using GEP.Cumulus.Logging;
using GEP.Cumulus.P2P.BusinessEntities;
using GEP.Cumulus.P2P.Req.RestService.App_Start.Proxy;
using GEP.SMART.Configuration;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Web.Script.Serialization;
using SMARTFaultException = Gep.Cumulus.ExceptionManager;

namespace GEP.Cumulus.P2P.Req.RestService
{
    [ExcludeFromCodeCoverage]
    public class DocumentRestServiceHelper
    {
        private static readonly ILog Log = Logger.GetLog(MethodBase.GetCurrentMethod().DeclaringType);
        private string token = string.Empty;
        private string Token
        {
            get
            {
                if (string.IsNullOrEmpty(token))
                {
                    if (System.ServiceModel.Web.WebOperationContext.Current != null &&
                        System.ServiceModel.Web.WebOperationContext.Current.IncomingRequest != null &&
                        System.ServiceModel.Web.WebOperationContext.Current.IncomingRequest.Headers["Authorization"] != null)
                    {
                        token = System.ServiceModel.Web.WebOperationContext.Current.IncomingRequest.Headers["Authorization"];
                    }
                }
                return token;
            }
        }

        public DocumentRestServiceHelper(string jwtToken)
        {
            LogNewRelic("Inside DocumentRestServiceHelper constructor with token" + jwtToken, "DocumentRestServiceHelper", 0);
            token = jwtToken;
        }

        // Added from document rest service
        public void DeleteAllTasksForDocument(long documentCode, int wfDocTypeId, List<ApproverDetails> lstActioners, UserExecutionContext userExecutionContext)
        {
            LogNewRelic("Inside DeleteAllTasksForDocument at LN 51", "DeleteAllTasksForDocument", documentCode);

            //var objDocSer = new P2PDocumentRestService();
            TaskInformation taskInformation = new TaskInformation();
            if (lstActioners.Any())
            {
                LogNewRelic("Inside DeleteAllTasksForDocument at LN 57. lstActioners.count = " + lstActioners.Count, "DeleteAllTasksForDocument", documentCode);
                List<TaskActionDetails> lstTasksAction = new List<TaskActionDetails>();
                lstTasksAction.Add(TaskHelper.CreateActionDetails(ActionKey.Approve, TaskConstants.APPROVE));
                lstTasksAction.Add(TaskHelper.CreateActionDetails(ActionKey.Reject, TaskConstants.REJECT));               

                foreach (var actioner in lstActioners)
                {
                    taskInformation = TaskHelper.CreateTaskObject(documentCode, actioner.ApproverId, lstTasksAction, true, false,
                        userExecutionContext.BuyerPartnerCode, userExecutionContext.CompanyName);
                    SaveTaskActionDetails(taskInformation, userExecutionContext);

                    if (actioner.ProxyApproverId > 0)
                    {
                        taskInformation = TaskHelper.CreateTaskObject(documentCode, actioner.ProxyApproverId, lstTasksAction, true, false,
                                                                          userExecutionContext.BuyerPartnerCode, userExecutionContext.CompanyName);
                        SaveTaskActionDetails(taskInformation, userExecutionContext);
                    }
                }
            }

            LogNewRelic("End of DeleteAllTasksForDocument at LN 77", "DeleteAllTasksForDocument", documentCode);
        }         

        public bool SaveTaskActionDetails(TaskInformation taskInformation, UserExecutionContext userExecutionContext)
        {           
            var result = true;
            try
            {
                
                LogNewRelic("Inside SaveTaskActionDetails Begin LN80" , "SaveTaskActionDetails", taskInformation.DocumentCode);

                string PostSaveTaskActionDetails = "/api/Req/SaveTaskActionDetails?oloc=266";
                string url = MultiRegionConfig.GetConfig(CloudConfig.AppURL) + PostSaveTaskActionDetails;
                string appName = Environment.GetEnvironmentVariable("NewRelic.AppName") ?? "Requisition";
                string useCase = "SaveTaskActionDetails";
                var requestHeaders = new APIRequestHeaders();
                requestHeaders.Set(userExecutionContext, Token);
                var webAPI = new WebAPIHelper(requestHeaders, appName, useCase);
                var JsonResult = webAPI.ExecutePost(url, taskInformation);

                var jsonSerializer = new JavaScriptSerializer();
                result = jsonSerializer.Deserialize<bool>(JsonResult);

                LogNewRelic("Inside SaveTaskActionDetails After LN101", "SaveTaskActionDetails", taskInformation.DocumentCode);
            }
            catch (CommunicationException commFaultEx)
            {
                LogNewRelic("Error SaveTaskActionDetails 97. Error Message: " + commFaultEx.Message, "SaveTaskActionDetails", 0);

                LogHelper.LogError(Log, "Error occured in SaveTaskActionDetails method in DocumentRestServiceHelper", commFaultEx);
            }
            catch (Exception ex)
            {
                LogNewRelic("Error SaveTaskActionDetails 103. Error Message: " + ex.Message, "SaveTaskActionDetails", 0);

                LogHelper.LogError(Log, "Error occured in SaveTaskActionDetails method in DocumentRestServiceHelper", ex);
                throw;
            }
            return result;
        }

        public List<ReviewerDetails> GetReviewersList(long documentCode, int wfDocTypeId, UserExecutionContext userExecutionContext)
        {
            List<ReviewerDetails> reviewerDetailsList = new List<ReviewerDetails>();

            try
            {
                LogNewRelic("Inside GetReviewersList LN111", "GetReviewersList", documentCode);

                ApprovalHelper reviewerHelper = new ApprovalHelper() { userExecutionContext = userExecutionContext };
                var serviceUrl = MultiRegionConfig.GetConfig(CloudConfig.WorkFlowRestURL) + "/GetReviewersList";
                reviewerHelper.CreateHttpWebRequest(serviceUrl, Token);

                Dictionary<string, object> objDict = new Dictionary<string, object>();
                objDict.Add("documentCode", documentCode);
                objDict.Add("wfDocTypeId", wfDocTypeId);

                var result = reviewerHelper.GetHttpWebResponse(objDict);

                var jSrz = new JavaScriptSerializer();

                reviewerDetailsList = jSrz.Deserialize<List<ReviewerDetails>>(result);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in GetReviewersList Method in DocumentRestServiceHelper", ex);
                throw;
            }

            return reviewerDetailsList;
        }

        public void DeleteAllTasksForReviewDocument(long documentCode, int wfDocTypeId, List<ReviewerDetails> reviewersList, UserExecutionContext userExecutionContext)
        {
            LogNewRelic("Inside DeleteAllTasksForReviewDocument LN135", "DeleteAllTasksForReviewDocument", documentCode);

            ProxyP2PDocumentService proxyP2PDocumentService = new ProxyP2PDocumentService(userExecutionContext, Token);

            TaskInformation taskInformation = new TaskInformation();

            if (reviewersList.Any())
            {
                List<TaskActionDetails> lstTasksAction = new List<TaskActionDetails>();
                lstTasksAction.Add(TaskHelper.CreateActionDetails(ActionKey.Accept, TaskConstants.ACCEPT));
                lstTasksAction.Add(TaskHelper.CreateActionDetails(ActionKey.Reject, TaskConstants.REJECT));

                foreach (ReviewerDetails reviewer in reviewersList)
                {
                    taskInformation = TaskHelper.CreateTaskObject(documentCode, reviewer.ReviewerId, lstTasksAction, true, false, userExecutionContext.BuyerPartnerCode, userExecutionContext.CompanyName);

                    proxyP2PDocumentService.SaveTaskActionDetails(taskInformation);

                    if (reviewer.ProxyReviewerId > 0)
                    {
                        taskInformation = TaskHelper.CreateTaskObject(documentCode, reviewer.ProxyReviewerId, lstTasksAction, true, false,
                                                                          userExecutionContext.BuyerPartnerCode, userExecutionContext.CompanyName);
                        proxyP2PDocumentService.SaveTaskActionDetails(taskInformation);
                    }
                }
            }
        }

        public ApproverDetails GetContactCodeLastApprovalDetails(long documentCode, int documentTypeId, long contactCode, string jwtToken, UserExecutionContext userExecutionContext)
        {
            var approverDetails = new ApproverDetails();
            try
            {                
                int wfDocTypeId = documentTypeId;

                ApprovalHelper objApprovalHelper = new ApprovalHelper() { userExecutionContext = userExecutionContext };
                string serviceUrl = MultiRegionConfig.GetConfig(CloudConfig.WorkFlowRestURL) + "/GetContactCodeLastApprovalDetails";
                objApprovalHelper.CreateHttpWebRequest(serviceUrl, jwtToken);

                Dictionary<string, object> objDict = new Dictionary<string, object>();
                objDict.Add("documentCode", documentCode);
                objDict.Add("contactCode", contactCode);
                objDict.Add("wfDocTypeId", wfDocTypeId);
                var result = objApprovalHelper.GetHttpWebResponse(objDict);

                var jSrz = new JavaScriptSerializer();
                approverDetails = jSrz.Deserialize<ApproverDetails>(result);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in GetContactCodeLastApprovalDetails Method in DocumentRestServiceHelper", ex);
                // Log Exception here
                var objCustomFault = new CustomFault(ex.Message, "GetContactCodeLastApprovalDetails", "GetContactCodeLastApprovalDetails",
                                                  "Document", ExceptionType.ApplicationException, string.Empty, false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, string.Concat("Error while calling GetContactCodeLastApprovalDetails for  documentCode =", documentCode, " documentTypeId =", documentTypeId, " contactCode =", contactCode));
            }
            return approverDetails;
        }

        public void AddTasksForReviewDocument(long documentCode, int wfDocTypeId, List<ReviewerDetails> reviewersList, UserExecutionContext userExecutionContext)
        {
            List<ReviewerDetails> pendingReviewersList = new List<ReviewerDetails>();
            TaskInformation taskInformation = new TaskInformation();
            ProxyP2PDocumentService proxyP2PDocumentService = new ProxyP2PDocumentService(userExecutionContext, Token);
            List<DocumentProxyDetails> documentProxyDetailsLst = new List<DocumentProxyDetails>();

            try
            {
                ProxyP2PCommonService proxyP2PCommonService = new ProxyP2PCommonService(userExecutionContext, Token);
                if (proxyP2PCommonService != null)
                    documentProxyDetailsLst = proxyP2PCommonService.CheckOriginalReviewerNotificationStatus();
                if (reviewersList.Count > 0)
                {
                    reviewersList.Where(e => !e.IsProcessed && e.Status == (int)ReviewStatus.ReviewPending).ToList().ForEach(reviewer => pendingReviewersList.Add(reviewer));

                    // Add task for reviewer
                    foreach (ReviewerDetails reviewer in pendingReviewersList)
                    {
                        List<TaskActionDetails> lstTasksAction = new List<TaskActionDetails>();
                        lstTasksAction.Add(TaskHelper.CreateActionDetails(ActionKey.Accept, TaskConstants.ACCEPT));
                        lstTasksAction.Add(TaskHelper.CreateActionDetails(ActionKey.Reject, TaskConstants.REJECT));

                        if (reviewer.ProxyReviewerId > 0)
                        {
                            if (documentProxyDetailsLst.Where(e => e.ContactCode == reviewer.ReviewerId && Convert.ToDateTime(e.DateFrom.ToShortDateString()) <= Convert.ToDateTime(reviewer.StatusDate.ToShortDateString())
                                    && Convert.ToDateTime(e.DateTo.ToShortDateString()) >= Convert.ToDateTime(reviewer.StatusDate.ToShortDateString()) && e.AddTaskForOriginalApprover == true).Count() > 0)
                            {
                                taskInformation = TaskHelper.CreateTaskObject(documentCode, reviewer.ReviewerId, lstTasksAction, false, false, userExecutionContext.BuyerPartnerCode, userExecutionContext.CompanyName);
                                proxyP2PDocumentService.SaveTaskActionDetails(taskInformation);
                            }
                            taskInformation = TaskHelper.CreateTaskObject(documentCode, reviewer.ProxyReviewerId, lstTasksAction, false, false, userExecutionContext.BuyerPartnerCode, userExecutionContext.CompanyName);
                            proxyP2PDocumentService.SaveTaskActionDetails(taskInformation);
                        }
                        else
                        {
                            taskInformation = TaskHelper.CreateTaskObject(documentCode, reviewer.ReviewerId, lstTasksAction, false, false, userExecutionContext.BuyerPartnerCode, userExecutionContext.CompanyName);
                            proxyP2PDocumentService.SaveTaskActionDetails(taskInformation);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in AddTasksForReviewDocument Method in DocumentRestServiceHelper", ex);
                var objCustomFault = new CustomFault(ex.Message, "AddTasksForReviewDocument", "AddTasksForReviewDocument", "DocumentRestServiceHelper", ExceptionType.ApplicationException, string.Empty, false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while calling AddTasksForReviewDocument for DocumentCode=" + documentCode);
            }
        }

        public void RejectDocument(long documentCode, DocumentType documentType, string groupText, UserExecutionContext _userExecutionContext)
        {

            try
            {
                LogNewRelic("Inside RejectDocument LN248", "RejectDocument", documentCode);
                string serviceurl;
                ApprovalHelper objApprovalHelper = new ApprovalHelper() { userExecutionContext = _userExecutionContext };
                serviceurl = MultiRegionConfig.GetConfig(CloudConfig.WorkFlowRestURL) + "/ReceiveNotification";
                objApprovalHelper.CreateHttpWebRequest(serviceurl, Token);


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
                var objSettingsService = new ProxyP2PCommonService(_userExecutionContext, Token);
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
            catch
            {
                throw;
            }
        }

        public void AddTasksForDocument(long documentCode, int wfDocTypeId, List<ApproverDetails> lstActioners, UserExecutionContext userExecutionContext)
        {
            List<ApproverDetails> lstPendingApprovers = new List<ApproverDetails>();
            TaskInformation taskInformation = new TaskInformation();
            List<DocumentProxyDetails> documentProxyDetailsLst = new List<DocumentProxyDetails>();         
            try
            {
                LogNewRelic("Inside AddTasksForDocument LN 337", "AddTasksForDocument", documentCode);
                documentProxyDetailsLst = CheckOriginalApproverNotificationStatus(userExecutionContext);                

                if (lstActioners.Count > 0)
                {
                    LogNewRelic("Inside AddTasksForDocument LN 342. lstActioners.Count = " + lstActioners.Count.ToString(), "AddTasksForDocument", documentCode);

                    if (lstActioners.Where(e => !e.IsProcessed && e.ApproverType == 3).Count() > 0)
                        lstPendingApprovers = lstActioners.Where(e => !e.IsProcessed && e.Status == 2 && e.ApproverType == 3).ToList();
                    else
                    {
                        lstActioners.Where(e => !e.IsProcessed && e.Status == 2 && e.WorkflowId != 11).ToList().ForEach(e => lstPendingApprovers.Add(e));
                        lstActioners.Where(e => !e.IsProcessed && e.Status == 2 && e.WorkflowId == 11 && e.SubIsProcessed == false).ToList().ForEach(e => lstPendingApprovers.Add(e));
                    }

                    // Add task for approver and proxy approver
                    foreach (ApproverDetails actioner in lstPendingApprovers)
                    {
                        List<TaskActionDetails> lstTasksAction = new List<TaskActionDetails>();
                        lstTasksAction.Add(TaskHelper.CreateActionDetails(ActionKey.Approve, TaskConstants.APPROVE));
                        lstTasksAction.Add(TaskHelper.CreateActionDetails(ActionKey.Reject, TaskConstants.REJECT));
                        if (actioner.ProxyApproverId > 0)
                        {
                            if (documentProxyDetailsLst.Where(e => e.ContactCode == actioner.ApproverId && e.DoctypeId == wfDocTypeId && e.DateFrom < actioner.StatusDate
                                        && e.DateTo > actioner.StatusDate && e.AddTaskForOriginalApprover == false).Count() == 0)
                            {
                                taskInformation = TaskHelper.CreateTaskObject(documentCode, actioner.ApproverId, lstTasksAction, false, false, userExecutionContext.BuyerPartnerCode, userExecutionContext.CompanyName);
                                LogNewRelic("Before SaveTaskActionDetails LN332", "AddTasksForDocument", documentCode);
                                SaveTaskActionDetails(taskInformation, userExecutionContext);
                            }
                            taskInformation = TaskHelper.CreateTaskObject(documentCode, actioner.ProxyApproverId, lstTasksAction, false, false, userExecutionContext.BuyerPartnerCode, userExecutionContext.CompanyName);
                            LogNewRelic("Before SaveTaskActionDetails LN336", "AddTasksForDocument", documentCode);
                            SaveTaskActionDetails(taskInformation, userExecutionContext);
                        }
                        else
                        {
                            taskInformation = TaskHelper.CreateTaskObject(documentCode, actioner.ApproverId, lstTasksAction, false, false, userExecutionContext.BuyerPartnerCode, userExecutionContext.CompanyName);
                            LogNewRelic("Before SaveTaskActionDetails LN342", "AddTasksForDocument", documentCode);
                            SaveTaskActionDetails(taskInformation, userExecutionContext);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in AddTasksForDocument Method in DocumentRestServiceHelper", ex);
                var objCustomFault = new CustomFault(ex.Message, "AddTasksForDocument", "AddTasksForDocument",
                                                  "Document", ExceptionType.ApplicationException, string.Empty, false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while calling AddTasksForDocument for DocumentCode=" + documentCode);
            }
        }

        private List<DocumentProxyDetails> CheckOriginalApproverNotificationStatus(UserExecutionContext userExecutionContext)
        {
            LogNewRelic("Inside CheckOriginalApproverNotificationStatus LN 386", "CheckOriginalApproverNotificationStatus", 0);
            List<DocumentProxyDetails> documentProxyDetailsLst;
            string GetCheckOriginalApproverNotificationStatusURL = "/api/Req/CheckOriginalApproverNotificationStatus?oloc=266";
            string url = MultiRegionConfig.GetConfig(CloudConfig.AppURL) + GetCheckOriginalApproverNotificationStatusURL;

            APIRequestHeaders requestHeaders = new APIRequestHeaders();
            string appName = Environment.GetEnvironmentVariable("NewRelic.AppName");
            string useCase = "CheckOriginalApproverNotificationStatus";
            requestHeaders.Set(userExecutionContext, Token);
            var webAPI = new WebAPIHelper(requestHeaders, appName, useCase);
            var JsonResult = webAPI.ExecuteGet(url);

            LogNewRelic("Inside CheckOriginalApproverNotificationStatus LN 398. JsonResult: " + JsonResult, "CheckOriginalApproverNotificationStatus", 0);

            documentProxyDetailsLst = JsonConvert.DeserializeObject<List<DocumentProxyDetails>>(JsonResult);
            return documentProxyDetailsLst;
        }

        private void LogNewRelic(string message, string method, long documentCode)
        {
            var eventAttributes = new Dictionary<string, object>();
            eventAttributes.Add("message", message);
            eventAttributes.Add("method", method);
            eventAttributes.Add("documentCode", documentCode.ToString());
            NewRelic.Api.Agent.NewRelic.RecordCustomEvent("DocumentRestServiceHelper", eventAttributes);
        }

    }
}
