using Gep.Cumulus.CSM.Config;
using Gep.Cumulus.CSM.Entities;
using Gep.Cumulus.CSM.Extensions;
using Gep.Cumulus.Partner.Entities;
using GEP.Cumulus.Documents.Entities;
using GEP.Cumulus.Globalization.Helpers;
using GEP.Cumulus.Logging;
using GEP.Cumulus.P2P.BusinessEntities;
using GEP.Cumulus.P2P.BusinessObjects;
using GEP.Cumulus.P2P.DataAccessObjects.SQLServer;
using GEP.Cumulus.P2P.Req.BusinessObjects.Entities;
using EmailAdressInfo = GEP.Cumulus.P2P.Req.BusinessObjects.Entities.EmailAdressInfo;
using GEP.Cumulus.P2P.Req.BusinessObjects.Proxy;
using GEP.Cumulus.Web.Utils.Helpers;
using GEP.SMART.Configuration;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Text;

namespace GEP.Cumulus.P2P.Req.BusinessObjects
{
    public class RequisitionEmailNotificationManager : RequisitionBaseBO
    {
        private Contact UserContact { get; set; }

        private CultureInfo cultureInfo { get; set; }

        private Utils objUtils = new Utils();
        public RequisitionEmailNotificationManager(string jwtToken, UserExecutionContext context = null) : base(jwtToken)
        {
            if (context != null)
            {
                UserContext = context;
            }
            if (UserContext == null)
                UserContext = OperationContext.Current.IncomingMessageHeaders.GetHeader<UserExecutionContext>("GepCustomHeader", "Gep.Cumulus");
            SetUserConfig(UserContext);
        }
        public RequisitionEmailNotificationManager(UserExecutionContext userExecutionContext, string jwtToken) : base(jwtToken)
        {
            SetUserConfig(userExecutionContext);
        }
        
        public RequisitionEmailNotificationManager(UserExecutionContext userExecutionContext, GepConfig config, string jwtToken) : base(jwtToken)
        {
            GepConfiguration = config;
            SetUserConfig(userExecutionContext);

        }

        private void SetUserConfig(UserExecutionContext userExecutionContext)
        {
            UserContext = userExecutionContext;

            Proxy.ProxyPartnerService proxyPartnerService = new Proxy.ProxyPartnerService(UserContext, this.JWTToken);
            this.UserContact = proxyPartnerService.GetContactByContactCode(0, this.UserContext.ContactCode);

            cultureInfo = new CultureInfo(UserContact != null && UserContact.Address != null && UserContact.Address.CountryInfo != null
                && UserContact.Address.CountryInfo.CountryCultureInfo != null ? UserContact.Address.CountryInfo.CountryCultureInfo : this.UserContext.Culture.Trim());

            var commonManager = new RequisitionCommonManager(this.JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
            int maxPrecisionVal = Convert.ToInt32(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "MaxPrecessionValue", UserContext.UserId));

            objUtils = new Utils(maxPrecisionVal);
        }

        public static class P2PUserActivity
        {
            public const string CREATE_REQUISITION = "10700023";
            public const string CREATE_ORDER = "10700003";
            public const string CREATE_INVOICE = "10700042";
            public const string CREATE_INVOICE_BY_FLIPPING_ORDER = "10700048";
            public const string FLIP_REQUISITIONS_TO_ORDER = "10700079";
            public const string CREATE_INVOICE_BY_FLIPPING_PAYMENT = "10700076";
        }
        public static class ObjectType
        {
            public const string REQUISITION = "GEP.Cumulus.P2P.Requisition";
            public const string ORDER = "GEP.Cumulus.P2P.Order";
            public const string INVOICE = "GEP.Cumulus.P2P.Invoice";
            public const string IR = "GEP.Cumulus.P2P.IR";

            public const string PAYMENTREQUEST = "GEP.Cumulus.P2P.PaymentRequest";
            public const string ServiceConfirmation = "GEP.Cumulus.P2P.ServiceConfirmation";
        }
        private const string OB_Budget_Allocation_Not_Found = "BudgetAllocationNotFound";
        private static readonly ILog Log = Logger.GetLog(MethodBase.GetCurrentMethod().DeclaringType);

        public bool SendNotification(string eventCode, List<Entities.MailAdressInfo> toMails, SortedList<long, string> ccMails, SortedList<string, string> fieldValues, SortedList<long, string> attachments, decimal partnerCode, long documentCode = 0, DocumentType documentType = DocumentType.PO, string defaultcultureCode = null)
        {
            var commonManager = new RequisitionCommonManager(this.JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
            LogHelper.LogInfo(Log, "EmailNotificationManager SendNotification Method Started for eventCode=" + eventCode);
            bool result = false;

            LogHelper.LogInfo(Log, "EmailNotificationManager SendNotification Method - API Approach");
            /// getting the username
            UserContext userContext = null;
            ProxyPartnerService proxyPartnerService = new ProxyPartnerService(UserContext, this.JWTToken);

            try
            {
                /// get the context
                var partnerHelper = new RESTAPIHelper.PartnerHelper(this.UserContext, JWTToken);
                userContext = partnerHelper.GetUserContextDetailsByContactCode(this.UserContext.ContactCode);

                /// Filling username for creating JWT token
                if (!string.IsNullOrEmpty(userContext.UserName))
                {
                    this.UserContext.UserName = userContext.UserName;
                }

                /// Story #PLATU-226 - Removing Notification Mail Sender BO DLL dependency from your codebase
                /// generate the auth token
                string logEmailAPIURL = MultiRegionConfig.GetConfig(CloudConfig.APIMBaseURL) + URLs.NotificationServiceURL + URLs.PostLogEmailNotificationRequest;
                string sendEmailAPIURL = MultiRegionConfig.GetConfig(CloudConfig.APIMBaseURL) + URLs.NotificationServiceURL + URLs.PostSendEmailNotificationMessage;


                var logSettingValue = Convert.ToBoolean(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "EnableLogging", UserContext.ContactCode).ToLower());
                if (toMails.Count() > 0)
                {
                    GepConfig _gepConfig = new GepConfig();
                    UserExecutionContext userExecutionContext = string.IsNullOrEmpty(defaultcultureCode) ? UserContext : new UserExecutionContext
                    {
                        BuyerPartnerCode = UserContext.BuyerPartnerCode,
                        CompanyName = UserContext.CompanyName,
                        ClientName = UserContext.ClientName,
                        ContactCode = UserContext.ContactCode,
                        Culture = defaultcultureCode,
                        UserName = UserContext.UserName
                    };


                    /// Story #Req-5286 - Removing Notification Mail Sender BO DLL dependency from your codebase

                    /// TO email addresses
                    /// Distinct contacts-to prevent duplicate email to users
                    toMails = toMails.GroupBy(x => x.ContactCode).Select(y => y.First()).ToList();
                    List<EmailAdressInfo> lstMailAdressInfo = new List<EmailAdressInfo>();
                    foreach (var toMailInfo in toMails)
                    {
                        /// isSMARTRegisteredUser – ContactCode != 0
                        lstMailAdressInfo.Add(new Entities.EmailAdressInfo(toMailInfo.ContactCode != 0, toMailInfo.ContactCode, toMailInfo.EmailAddress));
                    }

                    /// CC email addresses
                    List<EmailAdressInfo> lstMailAdressInfoCc = new List<EmailAdressInfo>();
                    if (ccMails.Count() > 0)
                    {
                        foreach (var ccMailInfo in ccMails)
                        {
                            /// isSMARTRegisteredUser – ContactCode != 0
                            if (!string.IsNullOrEmpty(ccMailInfo.Value))
                                lstMailAdressInfoCc.Add(new Entities.EmailAdressInfo(ccMailInfo.Key != 0, ccMailInfo.Key, ccMailInfo.Value));
                        }
                    }

                    /// OPTIONAL - Attachments
                    var lstAttachmentInfos = new List<Entities.EmailAttachmentInfo>();
                    if (attachments.Count() > 0)
                    {
                        foreach (var attachmentInfo in attachments)
                            lstAttachmentInfos.Add(new Entities.EmailAttachmentInfo(attachmentInfo.Key, attachmentInfo.Value));
                    }

                    /// create the new email message object...
                    var objMessageInfo = new Entities.EmailMessageInfo(Guid.NewGuid().ToString(), eventCode, documentType.ToString(), lstMailAdressInfo)
                    {
                        CC = lstMailAdressInfoCc,
                        Attachments = lstAttachmentInfos,
                        NotificationTemplateID = 0,
                        EmailMessageTemplateId = 0,
                        ReferenceCode = documentCode,
                        EmailMessageTemplateFieldValues = fieldValues
                    };

                    try
                    {
                        if (logSettingValue)
                            LogHelper.LogError(Log, string.Concat("Email notification object of SendNotification Method for eventCode = {0} and notificationObj = {1}", eventCode, JSONHelper.ToJSON(objMessageInfo)), new Exception());

                        var requestHeaders = new RESTAPIHelper.RequestHeaders();
                        string appName = Environment.GetEnvironmentVariable("NewRelic.AppName") ?? "Requisition"; 
                        string useCase = "AutoSourcingManager-SendNotification";
                        requestHeaders.Set(UserContext, this.JWTToken);
                        var webAPI = new GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.WebAPI(requestHeaders, appName, useCase);
                        var logEmailAPIResponse = webAPI.ExecutePost(logEmailAPIURL, objMessageInfo);
                        var sendEmailAPIResponse = webAPI.ExecutePost(sendEmailAPIURL, objMessageInfo);

                        if (!string.IsNullOrEmpty(sendEmailAPIResponse))
                            result = JsonConvert.DeserializeObject<bool>(sendEmailAPIResponse);


                        if (logSettingValue)
                            LogHelper.LogError(Log, string.Concat("Result of email notification in EmailNotificationManager SendNotification Method for eventCode = {0}, notificationObj = {1}", eventCode, JSONHelper.ToJSON(objMessageInfo) + result), new Exception());
                    }
                    catch (Exception exception)
                    {
                        string TableConnectionString = MultiRegionConfig.GetConfig(CloudConfig.CloudStorageConn);
                        NotificationExceptionHelper objNotificationExceptionHelper = new NotificationExceptionHelper();
                        NotificationQueueMessage objNotificationQueueMessage = new NotificationQueueMessage(userExecutionContext.ContactCode);
                        objNotificationQueueMessage.BuyerPartnerCode = userExecutionContext.BuyerPartnerCode;
                        objNotificationQueueMessage.CompanyName = userExecutionContext.CompanyName;
                        objNotificationQueueMessage.ContactCode = userExecutionContext.ContactCode;
                        objNotificationQueueMessage.ErrorDetails = exception.Message;
                        objNotificationQueueMessage.EventCode = eventCode;//Pass the respective event code
                        objNotificationQueueMessage.MessageGUID = objMessageInfo.EmailNotificationGUID;
                        objNotificationQueueMessage.ReceivedTime = DateTime.UtcNow;
                        objNotificationQueueMessage.StackTraceDetails = exception.StackTrace;
                        objNotificationQueueMessage.SubAppCode = 107;
                        objNotificationQueueMessage.MessageInfo = JSONHelper.ToJSON(objMessageInfo);//Please use CSM.Extension to serialize the object

                        objNotificationExceptionHelper.AddExceptionLogAndStoreNotificationQueueInStorage(objNotificationQueueMessage, TableConnectionString);
                        throw;
                    }
                }
            }
            catch (CommunicationException commFaultEx)
            {
                LogHelper.LogError(Log, "Fault exception occured in EmailNotificationManager SendNotification Method for eventCode=" + eventCode, commFaultEx);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in EmailNotificationManager SendNotification Method for eventCode=" + eventCode, ex);
                throw;
            }

            return result;
        }

        private string GetMailActionURL(bool isSecure, RequisitionCommonManager commonManager, string type = "COMMON")
        {             
            var isEnableSecureEmailLink = commonManager.GetSettingsValueByKey(P2PDocumentType.Portal, "IsEnableSecureEmailLink", UserContext.UserId);
            bool secureEmailLink = string.IsNullOrEmpty(isEnableSecureEmailLink) ? false : isEnableSecureEmailLink.ToLower() == "true";
            LogNewRelicApp("Inside GetMailActionURL with secureEmailLink : " + secureEmailLink.ToString(), 0, "RequisitionSecureMailAction");
            if (secureEmailLink)
            {
                string url = MultiRegionConfig.GetConfig(CloudConfig.SmartBaseURL) + "SecureMailAction/";
                // Need to update here with new URL with security
                string result;
                if (type.ToUpper() == "COMMON")
                {
                    result = url + "P2PSecureMailAction";
                }
                else
                {
                    result = url + "GetRejectDocumentAndContact";
                }

                LogNewRelicApp("Inside GetMailActionURL with result : " + result, 0, "RequisitionSecureMailAction");
                return result;
            }
            else
            {
                if (isSecure)
                {
                    string SecureMailActionURl = MultiRegionConfig.GetConfig(CloudConfig.P2PSecureMailActionURL);
                    if (type.ToUpper() == "COMMON")
                        return SecureMailActionURl + "MailAction";
                    else
                        return SecureMailActionURl + "GetRejectDocumentAndContact";
                }
                else
                {
                    if (type.ToUpper() == "COMMON")
                        return MultiRegionConfig.GetConfig(CloudConfig.P2PMailActionURL);
                    else
                        return MultiRegionConfig.GetConfig(CloudConfig.SmartBaseURL) + "P2P/GetRejectDocumentAndContact";
                }
            }
        }
        private string GetBudgetDataHTML(long documentCode, DocumentType documentType, long lobId)
        {

            OperationalBudgetManager obm = new OperationalBudgetManager() { UserContext = UserContext, GepConfiguration = GepConfiguration };
            DataTable dtBudgetDetails = obm.GetBudgetSplitAccounting(documentCode, documentType, lobId);
            StringBuilder budgetHTML = new StringBuilder();

            if (dtBudgetDetails != null && dtBudgetDetails.Rows.Count > 0 && !dtBudgetDetails.Columns.Contains(OB_Budget_Allocation_Not_Found))
            {
                budgetHTML.AppendLine("<table border=\"1\" cellspacing=\"1\" cellpadding=\"1\">");
                budgetHTML.AppendLine("<tr>");

                //column headers
                foreach (DataColumn column in dtBudgetDetails.Columns)
                {
                    budgetHTML.AppendLine(string.Format("<th bgcolor=\"#A9A9A9\">{0}</th>", column.ColumnName));
                }
                budgetHTML.AppendLine("</tr>");

                //datarows
                foreach (DataRow row in dtBudgetDetails.Rows)
                {
                    budgetHTML.AppendLine("<tr>");
                    foreach (DataColumn column in dtBudgetDetails.Columns)
                    {
                        var data = (row[column] == null || string.IsNullOrEmpty(row[column].ToString())) ? "-" : row[column].ToString();
                        budgetHTML.AppendLine(string.Format("<td>{0}</td>", data));
                    }
                    budgetHTML.AppendLine("</tr>");
                }
                budgetHTML.AppendLine("</table>");
            }

            return budgetHTML.ToString();
        }
        public Tuple<string, string> GetNotificationEntitiesTable(long documentCode)
        {
            List<NotificationEntities> docEntities = GetCommonDao().GetAllEntityDetails(documentCode);
            List<string> notificationEntityNames = new List<string>();
            string table = "";
            if (docEntities != null && docEntities.Count > 1)
            {
                foreach (NotificationEntities docEntity in docEntities)
                {
                    notificationEntityNames.Add(GetRowForEntity(docEntity.EntityName, docEntity.EntityDisplayName));
                }
                foreach (var notificationEntityName in notificationEntityNames)
                {
                    table = table + notificationEntityName;
                }
            }
            table = table == "" ? "<tr><td>N.A.</td></tr>" : table;
            if (docEntities.Count > 0)
            {
                var commonManager = new RequisitionCommonManager(this.JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
                return Tuple.Create(commonManager.GetStringValueForNotificationEmails(), table);
            }
            else
                return Tuple.Create("", table);
        }
        public string GetRowForEntity(string entityName, string entityDisplayName)
        {
            string row = "";
            row = row + "<tr>";
            row = row + "<td>" + entityName + "</td>";
            row = row + "<td>" + entityDisplayName + "</td>";
            row = row + "</tr>";
            return row;
        }
        private string GetAccountingDataHTML(DataSet accountingData, string currencySymbol = "")
        {
            StringBuilder accountingHTML = new StringBuilder();
            if (accountingData != null && accountingData.Tables.Count > 0 && accountingData.Tables[0].Rows != null && accountingData.Tables[0].Rows.Count > 0)
            {
                var accountingDataTable = accountingData.Tables[0];
                var accountingHeaderTable = accountingData.Tables[1];
                int rowNumber = 0;
                foreach (DataRow headerRow in accountingHeaderTable.Rows)
                {
                    bool entityExists = false;
                    var columnName = headerRow[SqlConstants.COL_ENTITYNAME].ToString();
                    if (accountingDataTable.Columns.Contains(columnName))
                    {
                        accountingHTML.AppendLine("<table border=\"1\" cellspacing=\"1\" cellpadding=\"1\">");
                        accountingHTML.AppendLine("<tr>");
                        foreach (DataColumn column in accountingDataTable.Columns)
                        {
                            if (column.ColumnName == columnName || column.ColumnName == SqlConstants.COL_VALUE || column.ColumnName == SqlConstants.COL_TAXES || column.ColumnName == SqlConstants.COL_SHIPPINGCHARGES || column.ColumnName == SqlConstants.COL_OTHER_CHARGES || column.ColumnName == SqlConstants.COL_TOTAL)
                            {
                                var width = ((column.ColumnName == SqlConstants.COL_SHIPPINGCHARGES) || (column.ColumnName == SqlConstants.COL_OTHER_CHARGES)) ? 200 : 150;
                                accountingHTML.AppendLine(string.Format("<th bgcolor=\"#A9A9A9\" style=\"width:{0}px\">{1}</th>", width, column.ColumnName));
                            }
                        }
                        accountingHTML.AppendLine("</tr>");
                        foreach (DataRow accountingRow in accountingDataTable.Rows)
                        {
                            if (accountingRow[columnName] != null && !string.IsNullOrEmpty(accountingRow[columnName].ToString()))
                            {
                                accountingHTML.AppendLine("<tr>");
                                foreach (DataColumn column in accountingDataTable.Columns)
                                {
                                    if (column.ColumnName == columnName || column.ColumnName == SqlConstants.COL_VALUE || column.ColumnName == SqlConstants.COL_TAXES || column.ColumnName == SqlConstants.COL_SHIPPINGCHARGES || column.ColumnName == SqlConstants.COL_OTHER_CHARGES || column.ColumnName == SqlConstants.COL_TOTAL)
                                    {
                                        var data = (accountingRow[column] == null || string.IsNullOrEmpty(accountingRow[column].ToString())) ? "-" : accountingRow[column].ToString();
                                        if (column.ColumnName == SqlConstants.COL_VALUE || column.ColumnName == SqlConstants.COL_TAXES || column.ColumnName == SqlConstants.COL_SHIPPINGCHARGES || column.ColumnName == SqlConstants.COL_OTHER_CHARGES || column.ColumnName == SqlConstants.COL_TOTAL)
                                        {
                                            data = (string.IsNullOrEmpty(data) || data == "-") ? "0" : data;
                                            // data = string.Format("{0} {1}", currencySymbol, this.objUtils.FormatNumber(Convert.ToDecimal(data), this.cultureInfo));
                                        }
                                        var width = ((column.ColumnName == SqlConstants.COL_SHIPPINGCHARGES) || (column.ColumnName == SqlConstants.COL_OTHER_CHARGES)) ? 200 : 150;
                                        accountingHTML.AppendLine(string.Format("<td style=\"width:{0}px\">{1}</td>", width, data));
                                        entityExists = true;
                                    }
                                }
                                accountingHTML.AppendLine("</tr>");
                            }
                        }
                        if (!entityExists)
                        {
                            accountingHTML.AppendLine("<tr style=\"height:20px\"><td style=\"width:150px\">No values</td><td style=\"width:150px\"> -- </td><td style=\"width:150px\"> -- </td><td style=\"width:150px\"> -- </td><td style=\"width:150px\"> -- </td><td style=\"width:150px\"> -- </td></tr>");
                        }
                        accountingHTML.AppendLine("</table>");
                    }
                    rowNumber++;
                }

            }

            return accountingHTML.ToString();
        }

        public List<Comments> GetCommentsForNotification(long objectID, string objectType, long contactCode, bool isApprover, int userType)
        {
            CommentsGroup objCommentsGroup = new CommentsGroup();
            Comments objComment = new Comments();
            List<string> lstUserActivities = new List<string>();
            List<CommentsGroup> lstCommentsGroup = new List<CommentsGroup>();
            bool isAP = false;
            bool isBuyer = false;
            bool isRequester = false;

            if (contactCode != 0)
            {
                UserExecutionContext userExecutionContext = UserContext;

                var partnerHelper = new RESTAPIHelper.PartnerHelper(this.UserContext, JWTToken);
                lstUserActivities = partnerHelper.GetUserActivitiesByContactCode(contactCode, userExecutionContext.BuyerPartnerCode).Split(',').ToList();

                foreach (var activity in lstUserActivities)
                {
                    if (activity == P2PUserActivity.CREATE_INVOICE || activity == P2PUserActivity.CREATE_INVOICE_BY_FLIPPING_ORDER || activity == P2PUserActivity.CREATE_INVOICE_BY_FLIPPING_PAYMENT)
                        isAP = true;
                    else if (activity == P2PUserActivity.CREATE_REQUISITION)
                        isRequester = true;
                    else if (activity == P2PUserActivity.CREATE_ORDER || activity == P2PUserActivity.FLIP_REQUISITIONS_TO_ORDER)
                        isBuyer = true;
                }
            }

            string accessType = "4";
            if (userType != 1)
            {
                accessType += ",1";
                if (isAP)
                    accessType += ",8";
                if (isBuyer)
                    accessType += ",6";
                if (isRequester)
                    accessType += ",7";
            }
            if (isApprover)
                accessType = "2";

            List<CommentsGroupRequestModel> commentsGroupRequestModel = new List<CommentsGroupRequestModel>();
            commentsGroupRequestModel.Add(new CommentsGroupRequestModel()
            {
                AccessType = accessType,
                ObjectType = objectType,
                ObjectID = objectID
            });

            var commentHelper = new RESTAPIHelper.CommentHelper(this.UserContext, JWTToken);
            var result = commentHelper.GetCommentsWithAttachments(commentsGroupRequestModel);               
            lstCommentsGroup = commentHelper.Map(result);
                
            return lstCommentsGroup.Count != 0 ? lstCommentsGroup[0].Comment : null;
        }

        public bool SendFailureInternalNotificaiton(long documentCode, DocumentType documentType, string documentNumber, string documentName, Exception exception, string eventCode)
        {
            bool result = false;
            try
            {
                LogHelper.LogInfo(Log, string.Concat("EmailNotificationManager SendFailureInternalNotificaiton Method Started for documentCode=", documentCode));
                ProxyPartnerService proxyPartnerService = new ProxyPartnerService(UserContext, this.JWTToken);

                if (documentCode > 0)
                {
                    List<MailAdressInfo> toMails = new List<MailAdressInfo>();
                    SortedList<long, string> ccMails = new SortedList<long, string>();
                    SortedList<string, string> fieldValues = null;
                    var attachments = new SortedList<long, string>();
                    fieldValues = new SortedList<string, string>
                            {
                                {"[MethodName]", "documentType :" + documentType.GetStringValue() +" ,documentNumber :" + documentNumber +" ,EventCode :" +eventCode},
                                {"[ErrorDescription]", string.Concat( "Error for documentCode: ",documentCode.ToString()," BPC: ",this.UserContext.BuyerPartnerCode," StackTrace: ", exception.StackTrace," Errormessage: ", exception.Message) },
                                {"[ResponseURI]", ""},
                                {"[RequestURI]", ""},
                                {"[Subject]", "Error Notification for "+ this.UserContext.BuyerPartnerCode},
                                {"[BuyerCompanyLogo]", MultiRegionConfig.GetConfig(CloudConfig.BlobURL) + "cumuluscontent/logo/" + UserContext.BuyerPartnerCode + "_logo.jpg"}
                            };

                    toMails = new List<MailAdressInfo>();
                    toMails.Add(new MailAdressInfo() { ContactCode = 0, EmailAddress = "Nilang.Joshi@gep.com" });
                    toMails.Add(new MailAdressInfo() { ContactCode = 1, EmailAddress = "ravi.shankar@gep.com" });
                    toMails.Add(new MailAdressInfo() { ContactCode = 2, EmailAddress = "Manoj.Billava@gep.com" });
                    toMails.Add(new MailAdressInfo() { ContactCode = 3, EmailAddress = "dipak.mahadik@gep.com" });
                    toMails.Add(new MailAdressInfo() { ContactCode = 4, EmailAddress = "mitesh.shah@gep.com" });
                    toMails.Add(new MailAdressInfo() { ContactCode = 5, EmailAddress = "Syam.Subhakar@gep.com" });
                    result = SendNotification("BZ102", toMails, ccMails, fieldValues, attachments, UserContext.BuyerPartnerCode, documentCode, documentType, "en-us");
                }
            }
            catch (CommunicationException commFaultEx)
            {
                LogHelper.LogError(Log, string.Concat("Fault exception occured in EmailNotificationManager SendFailureInternalNotificaiton Method for documentCode=", documentCode), commFaultEx);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, string.Concat("Error occured in EmailNotificationManager SendFailureInternalNotificaiton Method for documentCode=", documentCode), ex);
                throw;
            }
            return result;
        }


        public bool SendNotificationForApprovedRequisition(long requisitionId)
        {
            bool result = false;
            List<MailAdressInfo> toMails = new List<MailAdressInfo>();
            SortedList<long, string> ccMails = new SortedList<long, string>();
            SortedList<long, string> attachments = null;
            SortedList<string, string> fieldValues = null;
            ProxyPartnerService proxyPartnerService = new ProxyPartnerService(UserContext, this.JWTToken);
            try
            {
                LogHelper.LogInfo(Log, "EmailNotificationManager SendNotificationForApprovedRequisition Method Started for requisitionId=" + requisitionId);
                var p2pcommonmanager = new RequisitionCommonManager(this.JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
                string questionIDs = p2pcommonmanager.GetSettingsValueByKey(P2PDocumentType.Requisition, "IncludeCustomAttributesInNotification", UserContext.UserId);
                DataSet objRequisitionDS = GetCommonDao().GetBasicDetailsByIdForNotification(requisitionId, UserContext.BuyerPartnerCode, 1, questionIDs);
                StringBuilder customAttributeHtml = new StringBuilder();
                if (objRequisitionDS.Tables.Count > 2 && questionIDs != string.Empty)
                {
                    DataTable dtReqCustomAttributes = objRequisitionDS.Tables[2];
                    customAttributeHtml = CustomAttributeBuilder(dtReqCustomAttributes);
                }
                DataTable objRequisition = objRequisitionDS.Tables[0];
                if (objRequisition != null)
                {
                    var OBOContactCode = (long)objRequisition.Rows[0][SqlConstants.COL_ONBEHALFOF];
                    UserExecutionContext userExecutionContext = UserContext;
                    var requesterId = (long)objRequisition.Rows[0][SqlConstants.COL_REQUESTER_ID];

                    var objRequester = proxyPartnerService.GetContactByContactCode(0, requesterId);//Partner code is not getting used in method GetContactByContactCode

                    toMails.Add(new MailAdressInfo() { ContactCode = (long)objRequisition.Rows[0][SqlConstants.COL_ONBEHALFOF], EmailAddress = objRequisition.Rows[0][SqlConstants.COL_ONBEHALFOFEMAIL].ToString() });
                    if (OBOContactCode != objRequester.ContactCode)
                        ccMails.Add(objRequester.ContactCode, objRequester.EmailAddress);

                    var requisitionUrl = "";

                    #region Smart2.0

                    requisitionUrl = GetQueryStringUrlForApprover(Convert.ToInt64(objRequisition.Rows[0][SqlConstants.COL_REQUISITION_ID].ToString()), userExecutionContext, DocumentType.Requisition, (long)objRequisition.Rows[0][SqlConstants.COL_ONBEHALFOF]);

                    #endregion Smart2.0

                    fieldValues = new SortedList<string, string>
                        {
                            {"[RequisitionName]", objRequisition.Rows[0][SqlConstants.COL_REQUISITION_NAME].ToString()},
                            {"[RequisitionNumber]", objRequisition.Rows[0][SqlConstants.COL_REQUISITION_NUMBER].ToString()},
                            {"[RequisitionAmount]", this.objUtils.FormatNumber(Convert.ToDecimal(objRequisition.Rows[0][SqlConstants.COL_REQUISITION_AMOUNT].ToString()), this.cultureInfo)},
                            {"[RequisitionAmountWithOverallLimit]", (Convert.ToDecimal(objRequisition.Rows[0][SqlConstants.COL_OVERALLTOTALAMOUNT].ToString()) > 0 )  ? this.objUtils.FormatNumber(Convert.ToDecimal(objRequisition.Rows[0][SqlConstants.COL_OVERALLTOTALAMOUNT].ToString()),this.cultureInfo):this.objUtils.FormatNumber(Convert.ToDecimal(objRequisition.Rows[0][SqlConstants.COL_REQUISITION_AMOUNT].ToString()),this.cultureInfo)},
                            {"[UserName]",OBOContactCode > 0 ? objRequisition.Rows[0][SqlConstants.COL_ONBEHALFOFNAME].ToString() : (objRequester.FirstName+" "+objRequester.LastName).Trim()},
                            {"[Currency]", GetCurrencySymbol(objRequisition.Rows[0][SqlConstants.COL_CURRENCY].ToString())},
                            {"[CurrencyCode]", Convert.ToString(objRequisition.Rows[0][SqlConstants.COL_CURRENCY].ToString())},
                            {"[ProdLink]",requisitionUrl},
                            {"[CancelLink]", ""},
                            {"[BuyerCompanyName]",Convert.ToString(objRequisition.Rows[0][SqlConstants.COL_BUYER_COMPANY_NAME],CultureInfo.CurrentCulture)},
                            {"[BuyerCompanyLogo]", MultiRegionConfig.GetConfig(CloudConfig.BlobURL) + "cumuluscontent/logo/" + userExecutionContext.BuyerPartnerCode + "_logo.jpg"},
                            {"[Urgent]", Convert.ToBoolean(objRequisition.Rows[0][SqlConstants.COL_ISURGENT]) == true ? "Urgent " : string.Empty},
                            {"[Custom Attributes]", customAttributeHtml.ToString() }
                        };

                    attachments = new SortedList<long, string>();

                    var partnerHelper = new RESTAPIHelper.PartnerHelper(this.UserContext, JWTToken);
                    List<User> UsersCollection = partnerHelper.GetUserDetailsByContactCodes(requesterId.ToString(), false);
                    
                    User currentUser = new User();
                    if (UsersCollection == null || UsersCollection.Count == 0)
                    {
                        currentUser = new User() { CultureCode = UserContext.Culture };
                    }
                    else
                    {
                        currentUser = UsersCollection[0];
                    }
                    currentUser.CultureCode = string.IsNullOrEmpty(currentUser.CultureCode) ? UserContext.Culture : currentUser.CultureCode;
                    result = SendNotification("P2P108", toMails, ccMails, fieldValues, attachments, UserContext.BuyerPartnerCode, requisitionId, DocumentType.Requisition);
                }
            }
            catch (CommunicationException commFaultEx)
            {
                LogHelper.LogError(Log, "Fault exception occured in EmailNotificationManager SendNotificationForApprovedRequisition Method for requisitionId=" + requisitionId, commFaultEx);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in EmailNotificationManager SendNotificationForApprovedRequisition Method for requisitionId=" + requisitionId, ex);
                throw;
            }
            return result;
        }

        public bool SendNotificationForRejectedRequisition(long requisitionId, ApproverDetails rejector, List<ApproverDetails> prevApprovers, string queryString)
        {
            LogHelper.LogInfo(Log, "EmailNotificationManager SendNotificationForRejectedRequisition Method Started for requisitionId=" + requisitionId);
            bool result = false;
            ProxyPartnerService proxyPartnerService = new ProxyPartnerService(UserContext, this.JWTToken);
            SortedList<long, string> ccMails = new SortedList<long, string>();
            SortedList<long, string> attachments = new SortedList<long, string>();
            SortedList<string, string> fieldValues = new SortedList<string, string>();
            long Commenttype = 2;
            try
            {
                var p2pcommonmanager = new RequisitionCommonManager(this.JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
                string questionIDs = p2pcommonmanager.GetSettingsValueByKey(P2PDocumentType.Requisition, "IncludeCustomAttributesInNotification", UserContext.UserId);
                DataSet objRequisitionDS = GetCommonDao().GetBasicDetailsByIdForNotification(requisitionId, UserContext.BuyerPartnerCode, Commenttype, questionIDs);
                StringBuilder customAttributeHtml = new StringBuilder();
                if (objRequisitionDS.Tables.Count > 2 && questionIDs != string.Empty)
                {
                    DataTable dtReqCustomAttributes = objRequisitionDS.Tables[2];
                    customAttributeHtml = CustomAttributeBuilder(dtReqCustomAttributes);
                }

                DataTable objRequisition = objRequisitionDS.Tables[0];
                DataTable objRejectionCommentDT = objRequisitionDS.Tables[1];
                if (objRequisition != null)
                {
                    UserExecutionContext userExecutionContext = UserContext;

                    List<MailAdressInfo> toMails = new List<MailAdressInfo>();
                    var objRequester = proxyPartnerService.GetContactByContactCode(0, (long)objRequisition.Rows[0][SqlConstants.COL_REQUESTER_ID]);//Partner code is not getting used in method GetContactByContactCode
                    var ONBEHALFOF = (long)objRequisition.Rows[0][SqlConstants.COL_ONBEHALFOF];
                    var requesterId = (long)objRequisition.Rows[0][SqlConstants.COL_REQUESTER_ID];

                    toMails.Add(new MailAdressInfo() { ContactCode = ONBEHALFOF, EmailAddress = objRequisition.Rows[0][SqlConstants.COL_ONBEHALFOFEMAIL].ToString() });
                    if (ONBEHALFOF != requesterId)
                        ccMails.Add(requesterId, objRequisition.Rows[0][SqlConstants.COL_REQUESTER_EMAIL_ID].ToString());

                    var requisitionUrl = "";

                    #region Smart2.0

                    requisitionUrl = GetQueryStringUrlForApprover(Convert.ToInt64(objRequisition.Rows[0][SqlConstants.COL_REQUISITION_ID].ToString()), userExecutionContext, DocumentType.Requisition, (long)objRequisition.Rows[0][SqlConstants.COL_ONBEHALFOF]);

                    #endregion Smart2.0
                    string RejectionComment = "";
                    if (objRejectionCommentDT.Rows.Count > 0)
                    {
                        RejectionComment = Convert.ToString(objRejectionCommentDT.Rows[0]["Comment"]);
                    }

                    fieldValues = new SortedList<string, string>
                          {
                              {"[RequisitionName]", objRequisition.Rows[0][SqlConstants.COL_REQUISITION_NAME].ToString()},
                              {"[RequisitionNumber]", objRequisition.Rows[0][SqlConstants.COL_REQUISITION_NUMBER].ToString()},
                              {"[RequisitionAmount]", this.objUtils.FormatNumber(Convert.ToDecimal(objRequisition.Rows[0][SqlConstants.COL_REQUISITION_AMOUNT].ToString()), this.cultureInfo)},
                              {"[RequisitionAmountWithOverallLimit]", (Convert.ToDecimal(objRequisition.Rows[0][SqlConstants.COL_OVERALLTOTALAMOUNT].ToString()) > 0 )  ? this.objUtils.FormatNumber(Convert.ToDecimal(objRequisition.Rows[0][SqlConstants.COL_OVERALLTOTALAMOUNT].ToString()),this.cultureInfo):this.objUtils.FormatNumber(Convert.ToDecimal(objRequisition.Rows[0][SqlConstants.COL_REQUISITION_AMOUNT].ToString()),this.cultureInfo)},
                              {"[ApproverName]", rejector.ActionerType == 2 ? rejector.ProxyApproverName : rejector.Name},
                              {"[RejectionDate]", rejector.StatusDate.ToString("dd/MM/yyyy HH:mm")+" (UTC)"},
                              {"[UserName]", (objRequester.FirstName+" "+objRequester.LastName).Trim()},
                              {"[Currency]", GetCurrencySymbol(objRequisition.Rows[0][SqlConstants.COL_CURRENCY].ToString())},
                              {"[CurrencyCode]", Convert.ToString(objRequisition.Rows[0][SqlConstants.COL_CURRENCY].ToString())},
                              {"[ProdLink]",requisitionUrl},
                              {"[CancelLink]",""},
                              {"[BuyerCompanyName]",Convert.ToString(objRequisition.Rows[0][SqlConstants.COL_BUYER_COMPANY_NAME],CultureInfo.CurrentCulture)},
                              {"[BuyerCompanyLogo]", MultiRegionConfig.GetConfig(CloudConfig.BlobURL) + "cumuluscontent/logo/" + UserContext.BuyerPartnerCode + "_logo.jpg"},
                              {"[Urgent]", Convert.ToBoolean(objRequisition.Rows[0][SqlConstants.COL_ISURGENT]) == true ? "Urgent " : string.Empty},
                              {"[RejectedComments]",RejectionComment == "" ? "NULL" : RejectionComment},
                              {"[Custom Attributes]", customAttributeHtml.ToString() }
                          };

                    attachments = new SortedList<long, string>();
                    List<User> UsersCollection = null;
                    var partnerHelper = new RESTAPIHelper.PartnerHelper(this.UserContext, JWTToken);
                    UsersCollection = partnerHelper.GetUserDetailsByContactCodes(ONBEHALFOF.ToString(), false);

                    User currentUser = new User();
                    if (UsersCollection == null || UsersCollection.Count == 0)
                    {
                        currentUser = new User() { CultureCode = UserContext.Culture };
                    }
                    else
                    {
                        currentUser = UsersCollection[0];
                    }

                    currentUser.CultureCode = string.IsNullOrEmpty(currentUser.CultureCode) ? this.UserContext.Culture : currentUser.CultureCode;

                    result = SendNotification("P2P111", toMails, ccMails, fieldValues, attachments, UserContext.BuyerPartnerCode, requisitionId, DocumentType.Requisition, currentUser.CultureCode);

                    ccMails.Clear();
                    List<DocumentProxyDetails> documentProxyDetailsLst = new List<DocumentProxyDetails>();
                    documentProxyDetailsLst = GetCommonDao().CheckOriginalApproverNotificationStatus();
                    string contactCodes = null;
                    if (prevApprovers != null && prevApprovers.Any())
                    {
                        contactCodes = string.Join(",", prevApprovers.Select(p => p.ApproverId));
                    }

                    if (!string.IsNullOrEmpty(contactCodes))
                    {
                        partnerHelper = new RESTAPIHelper.PartnerHelper(UserContext, JWTToken);
                        UsersCollection = partnerHelper.GetUserDetailsByContactCodes(contactCodes, false);
                    }

                    foreach (var prevApprover in prevApprovers)
                    {
                        Contact objApprover = new Contact();
                        if (((long)objRequisition.Rows[0][SqlConstants.COL_REQUESTER_ID] != prevApprover.ApproverId && prevApprover.ActionerType != 2) ||
                            ((long)objRequisition.Rows[0][SqlConstants.COL_REQUESTER_ID] != prevApprover.ProxyApproverId && prevApprover.ActionerType == 2))
                        {
                            toMails.Clear();
                            if (prevApprover.ActionerType == 2)
                                objApprover = proxyPartnerService.GetContactByContactCode(0, prevApprover.ProxyApproverId);//Partner code is not getting used in method GetContactByContactCode
                            else
                                objApprover = proxyPartnerService.GetContactByContactCode(0, prevApprover.ApproverId);//Partner code is not getting used in method GetContactByContactCode
                            toMails.Add(new MailAdressInfo() { ContactCode = prevApprover.ApproverId, EmailAddress = objApprover.EmailAddress });

                            #region Smart2.0
                            requisitionUrl = GetQueryStringUrlForApprover(Convert.ToInt64(objRequisition.Rows[0][SqlConstants.COL_REQUISITION_ID].ToString()), userExecutionContext, DocumentType.Requisition, prevApprover.ApproverId);

                            #endregion Smart2.0 

                            fieldValues = new SortedList<string, string>
                          {
                              {"[RequisitionName]", objRequisition.Rows[0][SqlConstants.COL_REQUISITION_NAME].ToString()},
                              {"[RequisitionNumber]", objRequisition.Rows[0][SqlConstants.COL_REQUISITION_NUMBER].ToString()},
                              {"[RequisitionAmount]", this.objUtils.FormatNumber(Convert.ToDecimal(objRequisition.Rows[0][SqlConstants.COL_REQUISITION_AMOUNT].ToString()),this.cultureInfo)},
                              {"[RequisitionAmountWithOverallLimit]", (Convert.ToDecimal(objRequisition.Rows[0][SqlConstants.COL_OVERALLTOTALAMOUNT].ToString()) > 0)  ? this.objUtils.FormatNumber(Convert.ToDecimal(objRequisition.Rows[0][SqlConstants.COL_OVERALLTOTALAMOUNT].ToString()),this.cultureInfo):this.objUtils.FormatNumber(Convert.ToDecimal(objRequisition.Rows[0][SqlConstants.COL_REQUISITION_AMOUNT].ToString()),this.cultureInfo)},
                              {"[ApproverName]", rejector.ActionerType == 2 ? rejector.ProxyApproverName : rejector.Name },
                              {"[RejectionDate]", rejector.StatusDate.ToString("dd/MM/yyyy HH:mm")+" (UTC)"},
                              {"[UserName]", (prevApprover.Name).Trim()},
                              {"[Currency]", GetCurrencySymbol(objRequisition.Rows[0][SqlConstants.COL_CURRENCY].ToString())},
                              {"[CurrencyCode]", Convert.ToString(objRequisition.Rows[0][SqlConstants.COL_CURRENCY].ToString())},
                              {"[ProdLink]",requisitionUrl},
                              {"[CancelLink]",""},
                              {"[BuyerCompanyName]",Convert.ToString(objRequisition.Rows[0][SqlConstants.COL_BUYER_COMPANY_NAME],CultureInfo.CurrentCulture)},
                              {"[BuyerCompanyLogo]", MultiRegionConfig.GetConfig(CloudConfig.BlobURL) + "cumuluscontent/logo/" + UserContext.BuyerPartnerCode + "_logo.jpg"},
                              {"[Urgent]", Convert.ToBoolean(objRequisition.Rows[0][SqlConstants.COL_ISURGENT]) == true ? "Urgent " : string.Empty},
                              {"[RejectedComments]",RejectionComment == "" ? "NULL" : RejectionComment},
                              {"[Custom Attributes]", customAttributeHtml.ToString() }
                          };

                            attachments = new SortedList<long, string>();
                            User approverDetail = null;
                            if (UsersCollection != null && UsersCollection.Any())
                            {
                                approverDetail = UsersCollection.FirstOrDefault(u => u.ContactCode == prevApprover.ApproverId);
                            }
                            if (approverDetail == null || string.IsNullOrEmpty(currentUser.CultureCode))
                            {
                                approverDetail = new User() { CultureCode = UserContext.Culture };
                            }
                            result = SendNotification("P2P111", toMails, ccMails, fieldValues, attachments, UserContext.BuyerPartnerCode, requisitionId, DocumentType.Requisition, approverDetail.CultureCode);
                        }
                    }
                }
            }
            catch (CommunicationException commFaultEx)
            {
                LogHelper.LogError(Log, "Fault exception occured in EmailNotificationManager SendNotificationForRejectedRequisition Method for requisitionId=" + requisitionId, commFaultEx);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in EmailNotificationManager SendNotificationForRejectedRequisition Method for requisitionId=" + requisitionId, ex);
                throw;
            }
            return result;
        }

        public bool SendNotificationForRequisitionApproval(long documentCode, List<ApproverDetails> lstPendingApprover, List<ApproverDetails> lstPastApprover, string eventName, DocumentStatus documentStatus, string approvalType)
        {
            bool result = false;
            ProxyPartnerService proxyPartnerService = new ProxyPartnerService(UserContext, this.JWTToken);
            SortedList<long, string> ccMails = new SortedList<long, string>();
            SortedList<long, string> attachments = new SortedList<long, string>();
            SortedList<string, string> fieldValues = new SortedList<string, string>();
            List<Comments> lstComments = new List<Comments>();
            long maxWFOrderId = 0;
            try
            {
                LogHelper.LogInfo(Log, "EmailNotificationManager SendNotificationForRequisitionApproval Method Started for documentCode=" + documentCode);
                UserExecutionContext userExecutionContext = UserContext;
                var commonManager = new RequisitionCommonManager(this.JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
                bool requireSecureMail = Convert.ToBoolean(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "SendSecureMailAction", UserContext.UserId));

                string questionIDs = commonManager.GetSettingsValueByKey(P2PDocumentType.Requisition, "IncludeCustomAttributesInNotification", UserContext.UserId);
                DataSet objRequisitionDS = GetCommonDao().GetBasicDetailsByIdForNotification(documentCode, userExecutionContext.BuyerPartnerCode, 1, questionIDs);
                Tuple<string, string> notificationEntities = GetNotificationEntitiesTable(documentCode);
                string notificationEntityMessage = notificationEntities.Item1;
                string notificationEntityValues = notificationEntities.Item2;

                long lobId = GetCommonDao().GetLOBByDocumentCode(documentCode);
                DataSet accountingData = null;
                var accountingHTML = string.Empty;
                var accountingEntitiesConfiguration = commonManager.GetSettingsValueByKey(P2PDocumentType.None, "EmailNotificationAccountingEntities", UserContext.ContactCode, (int)SubAppCodes.P2P, "", lobId);
                if (!string.IsNullOrEmpty(accountingEntitiesConfiguration))
                {
                    accountingData = commonManager.GetAccountingDataByDocumentCode(documentCode, accountingEntitiesConfiguration, Convert.ToInt32(DocumentType.Requisition));
                    accountingHTML = GetAccountingDataHTML(accountingData);
                }
                if (objRequisitionDS != null)
                {
                    DataTable objDTRequisition = objRequisitionDS.Tables[0];
                    StringBuilder CustomAttributeHtml = new StringBuilder();
                    if (objRequisitionDS.Tables.Count > 2 && questionIDs != string.Empty)
                    {
                        DataTable dtReqCustomAttributes = objRequisitionDS.Tables[2];
                        CustomAttributeHtml = CustomAttributeBuilder(dtReqCustomAttributes);
                    }

                    if (objDTRequisition.Rows.Count > 0)
                    {
                        List<DocumentProxyDetails> documentProxyDetailsLst = new List<DocumentProxyDetails>();
                        documentProxyDetailsLst = GetCommonDao().CheckOriginalApproverNotificationStatus();
                        string strPastApprovers = "";
                        if (eventName == "Intermediate")
                        {
                            lstComments = GetCommentsForNotification(documentCode, ObjectType.REQUISITION, 0, true, 0);

                            foreach (var pastApprover in lstPastApprover)
                            {
                                if (pastApprover.ActionerType == 2)
                                    strPastApprovers += pastApprover.ProxyApproverName + " approved this Requisition at " + pastApprover.StatusDate.ToString("dd/MM/yyyy HH:mm") + "<br/>";
                                else
                                    strPastApprovers += pastApprover.Name + " approved this Requisition at " + pastApprover.StatusDate.ToString("dd/MM/yyyy HH:mm") + "<br/>";

                                if (lstComments != null && lstComments.Any())
                                {
                                    if (lstComments.FirstOrDefault(x => (x.CreatedBy == pastApprover.ApproverId) || x.CreatedBy == pastApprover.ProxyApproverId) != null)
                                        strPastApprovers += "Comments :<br/>";
                                    foreach (var comment in lstComments)
                                    {
                                        if ((comment.CreatedBy == pastApprover.ApproverId) || (comment.CreatedBy == pastApprover.ProxyApproverId))
                                        {
                                            strPastApprovers += "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + comment.CommentText + "<br>";
                                        }
                                    }
                                }
                            }
                        }

                        // If adhoc approvers are present in Current approval process
                        if (lstPendingApprover.Any())
                            maxWFOrderId = lstPendingApprover.First().WFOrderId;
                        if (lstPendingApprover.Where(e => e.ApproverType == 3).Any() || lstPastApprover.Where(e => e.ApproverType == 3 && e.WFOrderId == maxWFOrderId).Any())
                            lstPendingApprover = lstPendingApprover.Where(e => e.ApproverType == 3).ToList();

                        #region Export Requisition for Attachments

                        for (int i = 0; i < 3 && attachments.Count <= 0; i++)
                        {
                            try
                            {
                                var objRequisitionExportManager = new RequisitionExportManager(this.JWTToken)
                                {
                                    UserContext = UserContext,
                                    GepConfiguration = GepConfiguration
                                };

                                var fileDetails = objRequisitionExportManager.RequisitionExportById(documentCode, UserContext.ContactCode, 0, "4"); // Try to use AccessType enum
                                if (fileDetails != null && fileDetails.FileId > 0)
                                    attachments.Add(fileDetails.FileId, string.Empty);
                            }
                            catch (Exception ex)
                            {
                                LogHelper.LogError(Log, string.Format("Error occured in SendNotificationForSkipOrOffLineRequisitionApproval: RequisitionExportById Method call for RequisitionID={0}, BPC={1}, ContactCode={2}, Attempt={3}", documentCode, UserContext.BuyerPartnerCode, UserContext.ContactCode, i), ex);
                                if (i == 2)
                                {
                                    SendFailureInternalNotificaiton(documentCode, DocumentType.Requisition, documentCode.ToString(), documentCode.ToString(), ex, "P2P114/P2P128");
                                }
                            }
                        }

                        #endregion Export Requisition for Attachments

                        if (lstPendingApprover != null && lstPendingApprover.Any())
                        {
                            //string contactCodes = string.Join(",", lstPendingApprover.Select(p => p.ApproverId));
                            //List<User> UsersCollection = proxyPartnerService.GetUserDetailsByContactCodes(contactCodes, false);

                            var attachmentUrl = GetMailActionURL(requireSecureMail, commonManager) + "?documentType=" + DocumentType.Requisition +
                                                                        "&dtl=" + RequisitionDocumentManager.EncryptURL(string.Concat("evc=", ((eventName == "OnSubmit") ? "P2P114" : "P2P128"),
                                                                                                                            "&bpc=", UserContext.BuyerPartnerCode, "&cc=", UserContext.ContactCode,
                                                                                                                            "&dc=", documentCode, "&isPdf=", 1, "&iswm=", string.Empty, "&PLang=", false,
                                                                                                                            "&lobId=", 0, "&dt=", DateTime.UtcNow)) +
                                                                        "&oloc=" + (int)SubAppCodes.P2P;
                            string PDFMsg = AppSettingsGlobalization.RM != null ? AppSettingsGlobalization.RM.GetString("P2P_NotificationPDFMsg") : "There was some issue generating PDF and hence the same is not attached. You may contact the support desk or to generate the PDF yourself ";
                            string PDFLink = string.Concat("<a href = ", attachmentUrl, " target = '_blank' style = 'display: block; text-decoration: none;' >", AppSettingsGlobalization.RM != null ? AppSettingsGlobalization.RM.GetString("P2P_NotificationClickHere") : "click here", "</ a >.");

                            ////budget properties
                            string btnHTML = string.Empty;
                            string budgetHtml = string.Empty;
                            string reviewBudgetUrl = string.Empty;
                            string budgetTableLabel = string.Empty;
                            bool IsOBEnabled = Convert.ToBoolean(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "IsOperationalBudgetEnabled", commonManager.UserContext.ContactCode, (int)SubAppCodes.P2P, "", lobId));
                            
                            if (IsOBEnabled)
                            {
                                budgetTableLabel = "Budget Details as on " + DateTime.UtcNow + " (UTC)";
                                btnHTML = "<td align='center' valign='middle' style='color: #FFFFFF; background: #6bb24c'>" +
                                                                        "<a href=\"[ReviewBudgetLink]\" target='_blank' style='display: block; color: #FFFFFF; text-decoration: none; background: #6bb24c; " +
                                                                        "font-family: Helvetica, Arial, sans-serif; padding: 5px; border: 1px solid #186918; font-size: 12px;'>Real-time Budget Check</a></td>";
                                budgetHtml = GetBudgetDataHTML(documentCode, DocumentType.Requisition, lobId);
                            }

                            foreach (var approver in lstPendingApprover)
                            {
                                List<MailAdressInfo> toMails = new List<MailAdressInfo>();
                                var objApprover = proxyPartnerService.GetContactByContactCode(0, approver.ApproverId);//Partner code is not getting used in method GetContactByContactCode
                                if (approver.ProxyApproverId > 0)
                                {
                                    if (documentProxyDetailsLst.Where(e => e.ContactCode == approver.ApproverId && e.DoctypeId == 7 && e.DateFrom.Date <= approver.StatusDate.Date && e.DateTo.Date >= approver.StatusDate.Date
                                        && e.EnableOriginalApproverNotifications == true).Count() > 0)
                                        toMails.Add(new MailAdressInfo() { ContactCode = approver.ApproverId, EmailAddress = objApprover.EmailAddress });
                                }
                                else
                                    toMails.Add(new MailAdressInfo() { ContactCode = approver.ApproverId, EmailAddress = objApprover.EmailAddress });

                                var requisitionUrl = "";

                                #region Smart2.0


                                requisitionUrl = GetQueryStringUrlForApprover(Convert.ToInt64(objDTRequisition.Rows[0][SqlConstants.COL_REQUISITION_ID].ToString()), userExecutionContext, DocumentType.Requisition, approver.ApproverId);
                                #endregion Smart2.0
                                if (toMails.Any())
                                {
                                    var approveUrl = GetMailActionURL(requireSecureMail, commonManager) +
                                                                                    "?documentType=" + DocumentType.Requisition +
                                                                                    "&dtl=" + RequisitionDocumentManager.EncryptURL("evc=" + ((eventName == "OnSubmit") ? "P2P114" : "P2P128") +
                                                                                                                            "&bpc=" + UserContext.BuyerPartnerCode +
                                                                                                                            "&dc=" + objDTRequisition.Rows[0][SqlConstants.COL_REQUISITION_ID] +
                                                                                                                            "&cc=" + approver.ApproverId +
                                                                                                                            "&action=" + DocumentStatus.Approved +
                                                                                                                            "&dt=" + DateTime.UtcNow +
                                                                                                                            "&lob=" + lobId) +
                                                                                    "&oloc=" + (int)SubAppCodes.P2P +
                                                                                    "&b=1" +
                                                                                    "&dd=" + RequisitionDocumentManager.EncryptURL("bpc=" + UserContext.BuyerPartnerCode +
                                                                                                                           "&dc=" + objDTRequisition.Rows[0][SqlConstants.COL_REQUISITION_ID]);
                                    var rejectUrl = GetMailActionURL(requireSecureMail, commonManager, "REJECT") +
                                                                                    "?documentType=" + DocumentType.Requisition +
                                                                                    "&dtl=" + RequisitionDocumentManager.EncryptURL("evc=" + ((eventName == "OnSubmit") ? "P2P114" : "P2P128") +
                                                                                                                            "&bpc=" + UserContext.BuyerPartnerCode +
                                                                                                                            "&dc=" + objDTRequisition.Rows[0][SqlConstants.COL_REQUISITION_ID] +
                                                                                                                            "&cc=" + approver.ApproverId +
                                                                                                                            "&action=" + DocumentStatus.Rejected +
                                                                                                                            "&dt=" + DateTime.UtcNow +
                                                                                                                            "&lob=" + lobId) +
                                                                                    "&oloc=" + (int)SubAppCodes.P2P +
                                                                                    "&b=1" +
                                                                                    "&dd=" + RequisitionDocumentManager.EncryptURL("bpc=" + UserContext.BuyerPartnerCode +
                                                                                                                           "&dc=" + objDTRequisition.Rows[0][SqlConstants.COL_REQUISITION_ID]);
                                    if (IsOBEnabled)
                                    {
                                        reviewBudgetUrl = GetMailActionURL(requireSecureMail, commonManager, "REJECT") +
                                                                                    "?documentType=" + DocumentType.Requisition +
                                                                                    "&dtl=" + RequisitionDocumentManager.EncryptURL("evc=" + ((eventName == "OnSubmit") ? "P2P114" : "P2P128") +
                                                                                                                            "&bpc=" + UserContext.BuyerPartnerCode +
                                                                                                                            "&dc=" + objDTRequisition.Rows[0][SqlConstants.COL_REQUISITION_ID] +
                                                                                                                            "&cc=" + approver.ApproverId +
                                                                                                                            "&action=CheckReviewPending" +
                                                                                                                            "&dt=" + DateTime.UtcNow +
                                                                                                                            "&lob=" + lobId) +
                                                                                    "&oloc=" + (int)SubAppCodes.P2P +
                                                                                    "&b=1" +
                                                                                      "&dd=" + RequisitionDocumentManager.EncryptURL("bpc=" + UserContext.BuyerPartnerCode +
                                                                                                                             "&dc=" + objDTRequisition.Rows[0][SqlConstants.COL_REQUISITION_ID]);
                                    }

                                    fieldValues = new SortedList<string, string>
                                  {
                                      {"[Requester]", string.IsNullOrEmpty(objDTRequisition.Rows[0][SqlConstants.COL_ONBEHALFOFNAME].ToString()) ?
                                                          objDTRequisition.Rows[0][SqlConstants.COL_REQUESTER_NAME].ToString():
                                                          objDTRequisition.Rows[0][SqlConstants.COL_ONBEHALFOFNAME].ToString()},
                                      {"[RequisitionName]", objDTRequisition.Rows[0][SqlConstants.COL_REQUISITION_NAME].ToString()},
                                      {"[RequisitionNumber]", objDTRequisition.Rows[0][SqlConstants.COL_REQUISITION_NUMBER].ToString()},
                                      {"[ApprovalType]", StringEnum.GetStringValue(((Enums.WF_ApprovalType)Enum.Parse(typeof(Enums.WF_ApprovalType), approvalType)))},
                                      {"[UserName]",  ((string.IsNullOrEmpty(objApprover.FirstName) ? " " : objApprover.FirstName) + " " + (string.IsNullOrEmpty(objApprover.LastName) ? " " : objApprover.LastName)).Trim()},
                                      {"[ApproverName]", ((string.IsNullOrEmpty(objApprover.FirstName) ? " " : objApprover.FirstName) + " " + (string.IsNullOrEmpty(objApprover.LastName) ? " " : objApprover.LastName)).Trim()},
                                      {"[Currency]", GetCurrencySymbol(Convert.ToString(objDTRequisition.Rows[0][SqlConstants.COL_CURRENCY].ToString(), CultureInfo.CurrentCulture))},
                                      {"[CurrencyCode]", Convert.ToString(objDTRequisition.Rows[0][SqlConstants.COL_CURRENCY].ToString())},
                                      {"[RequisitionAmount]", this.objUtils.FormatNumber(Convert.ToDecimal(objDTRequisition.Rows[0][SqlConstants.COL_REQUISITION_AMOUNT].ToString()),this.cultureInfo)},
                                      {"[RequisitionAmountWithOverallLimit]", (Convert.ToDecimal(objDTRequisition.Rows[0][SqlConstants.COL_OVERALLTOTALAMOUNT].ToString()) > 0 ) ? this.objUtils.FormatNumber(Convert.ToDecimal(objDTRequisition.Rows[0][SqlConstants.COL_OVERALLTOTALAMOUNT].ToString()),this.cultureInfo):this.objUtils.FormatNumber(Convert.ToDecimal(objDTRequisition.Rows[0][SqlConstants.COL_REQUISITION_AMOUNT].ToString()),this.cultureInfo)},
                                      {"[DeltaAmount]", String.IsNullOrEmpty(Convert.ToString(objDTRequisition.Rows[0][SqlConstants.COL_REVISIONNUMBER])) ?"":" (Delta: "+objDTRequisition.Rows[0][SqlConstants.COL_CURRENCY].ToString()+" "+this.objUtils.FormatNumber(Convert.ToDecimal(objDTRequisition.Rows[0][SqlConstants.COL_REQUISITIONTOTALCHANGE].ToString()),this.cultureInfo)+")"},
                                      {"[ApproverDetails]", strPastApprovers},
                                      {"[ProdLink]", requisitionUrl},
                                      {"[ApproveLink]",approveUrl},
                                      {"[RejectLink]",rejectUrl},
                                      {"[BuyerCompanyName]",objDTRequisition.Rows[0][SqlConstants.COL_BUYER_COMPANY_NAME].ToString()},
                                      {"[BuyerCompanyLogo]", MultiRegionConfig.GetConfig(CloudConfig.BlobURL) + "cumuluscontent/logo/" + userExecutionContext.BuyerPartnerCode + "_logo.jpg"},
                                      {"[CancelLink]",""},
                                      {SqlConstants.NOTN_SupplierCompanyName,objDTRequisition.Rows[0][SqlConstants.COL_LEGALCOMPANYNAME].ToString()},
                                      {"[ItemTotal]",this.objUtils.FormatNumber(Convert.ToDecimal(objDTRequisition.Rows[0][SqlConstants.COL_ITEMTOTAL].ToString()),this.cultureInfo)},
                                      {"[Tax]", this.objUtils.FormatNumber(Convert.ToDecimal(objDTRequisition.Rows[0][SqlConstants.COL_TAX].ToString()),this.cultureInfo)},
                                      {"[Urgent]", Convert.ToBoolean(objDTRequisition.Rows[0][SqlConstants.COL_ISURGENT]) == true ? "Urgent " : string.Empty},
                                      {"[Author]", objDTRequisition.Rows[0][SqlConstants.COL_REQUESTER_NAME].ToString().Trim()},
                                      {"[OnBehalfOf]",objDTRequisition.Rows[0][SqlConstants.COL_ONBEHALFOF].ToString().Trim()!=objDTRequisition.Rows[0][SqlConstants.COL_REQUESTERID].ToString().Trim()? objDTRequisition.Rows[0][SqlConstants.COL_ONBEHALFOFNAME].ToString().Trim():""},
                                      {"[NotificationEntitiesAsString]", notificationEntityValues },
                                      {"[NotificationEntityMessage]",notificationEntityMessage },
                                      {"[AccountingData]", accountingHTML },
                                      {"[BudgetTableLabel]", budgetHtml.Length > 0 ? budgetTableLabel : string.Empty},
                                      {"[ReviewBudgetButton]", budgetHtml.Length > 0 ? btnHTML : string.Empty },
                                      {"[BudgetDataTable]", budgetHtml.Length > 0 ? budgetHtml : string.Empty },
                                      {"[ReviewBudgetLink]", budgetHtml.Length > 0 ? reviewBudgetUrl : string.Empty },
                                      {"[Custom Attributes]", CustomAttributeHtml.ToString() },
                                      {"[RiskScore]", string.IsNullOrEmpty(objDTRequisition.Rows[0][SqlConstants.COL_RISKSCORE].ToString()) ?"--": Math.Round(Convert.ToDecimal(objDTRequisition.Rows[0][SqlConstants.COL_RISKSCORE])).ToString().Trim()},
                                      {"[Risk Category]", objDTRequisition.Rows[0][SqlConstants.COL_RISKCATEGORY].ToString().Trim()}
                                  };

                                    if (string.IsNullOrWhiteSpace(objApprover.ContactCultureCode))
                                        objApprover.ContactCultureCode = this.UserContext.Culture;

                                    result = SendNotification(((eventName == "OnSubmit") ? "P2P114" : "P2P128"), toMails, ccMails, fieldValues, attachments, UserContext.BuyerPartnerCode, documentCode, DocumentType.Requisition, objApprover.ContactCultureCode);
                                }
                                if (approver.ProxyApproverId > 0)
                                {
                                    var objProxyApprover = proxyPartnerService.GetContactByContactCode(0, approver.ProxyApproverId);//Partner code is not getting used in method GetContactByContactCode
                                    toMails.Clear();
                                    fieldValues.Clear();

                                    var approveUrl = GetMailActionURL(requireSecureMail, commonManager) +
                                                                                    "?documentType=" + DocumentType.Requisition +
                                                                                    "&dtl=" + RequisitionDocumentManager.EncryptURL("evc=" + ((eventName == "OnSubmit") ? "P2P114" : "P2P128") +
                                                                                                                            "&bpc=" + UserContext.BuyerPartnerCode +
                                                                                                                            "&dc=" + objDTRequisition.Rows[0][SqlConstants.COL_REQUISITION_ID] +
                                                                                                                            "&cc=" + approver.ProxyApproverId +
                                                                                                                            "&action=" + DocumentStatus.Approved +
                                                                                                                            "&dt=" + DateTime.UtcNow +
                                                                                                                            "&lob=" + lobId) +
                                                                                    "&oloc=" + (int)SubAppCodes.P2P +
                                                                                    "&b=1" +
                                                                                    "&dd=" + RequisitionDocumentManager.EncryptURL("bpc=" + UserContext.BuyerPartnerCode +
                                                                                                                           "&dc=" + objDTRequisition.Rows[0][SqlConstants.COL_REQUISITION_ID]);
                                    var rejectUrl = GetMailActionURL(requireSecureMail, commonManager, "REJECT") +
                                                                                    "?documentType=" + DocumentType.Requisition +
                                                                                    "&dtl=" + RequisitionDocumentManager.EncryptURL("evc=" + ((eventName == "OnSubmit") ? "P2P114" : "P2P128") +
                                                                                                                            "&bpc=" + UserContext.BuyerPartnerCode +
                                                                                                                            "&dc=" + objDTRequisition.Rows[0][SqlConstants.COL_REQUISITION_ID] +
                                                                                                                            "&cc=" + approver.ProxyApproverId +
                                                                                                                            "&action=" + DocumentStatus.Rejected +
                                                                                                                            "&dt=" + DateTime.UtcNow +
                                                                                                                            "&lob=" + lobId) +
                                                                                    "&oloc=" + (int)SubAppCodes.P2P +
                                                                                    "&b=1" +
                                                                                    "&dd=" + RequisitionDocumentManager.EncryptURL("bpc=" + UserContext.BuyerPartnerCode +
                                                                                                                           "&dc=" + objDTRequisition.Rows[0][SqlConstants.COL_REQUISITION_ID]);
                                    if (IsOBEnabled)
                                    {
                                        reviewBudgetUrl = GetMailActionURL(requireSecureMail, commonManager, "REJECT") +
                                                                                    "?documentType=" + DocumentType.Requisition +
                                                                                    "&dtl=" + RequisitionDocumentManager.EncryptURL("evc=" + ((eventName == "OnSubmit") ? "P2P114" : "P2P128") +
                                                                                                                            "&bpc=" + UserContext.BuyerPartnerCode +
                                                                                                                            "&dc=" + objDTRequisition.Rows[0][SqlConstants.COL_REQUISITION_ID] +
                                                                                                                            "&cc=" + approver.ProxyApproverId +
                                                                                                                            "&action=CheckReviewPending" +
                                                                                                                            "&dt=" + DateTime.UtcNow +
                                                                                                                            "&lob=" + lobId) +
                                                                                    "&oloc=" + (int)SubAppCodes.P2P +
                                                                                    "&b=1" +
                                                                                      "&dd=" + RequisitionDocumentManager.EncryptURL("bpc=" + UserContext.BuyerPartnerCode +
                                                                                                                             "&dc=" + objDTRequisition.Rows[0][SqlConstants.COL_REQUISITION_ID]);
                                    }

                                    toMails.Add(new MailAdressInfo() { ContactCode = approver.ProxyApproverId, EmailAddress = objProxyApprover.EmailAddress });

                                    #region Smart2.0

                                    requisitionUrl = GetQueryStringUrlForApprover(Convert.ToInt64(objDTRequisition.Rows[0][SqlConstants.COL_REQUISITION_ID].ToString()), userExecutionContext, DocumentType.Requisition, approver.ApproverId);

                                    #endregion Smart2.0

                                    fieldValues = new SortedList<string, string>
                                  {
                                      {"[Requester]", string.IsNullOrEmpty(objDTRequisition.Rows[0][SqlConstants.COL_ONBEHALFOFNAME].ToString()) ?
                                                          objDTRequisition.Rows[0][SqlConstants.COL_REQUESTER_NAME].ToString():
                                                          objDTRequisition.Rows[0][SqlConstants.COL_ONBEHALFOFNAME].ToString()},
                                      {"[RequisitionName]", objDTRequisition.Rows[0][SqlConstants.COL_REQUISITION_NAME].ToString()},
                                      {"[RequisitionNumber]", objDTRequisition.Rows[0][SqlConstants.COL_REQUISITION_NUMBER].ToString()},
                                      {"[ApprovalType]", StringEnum.GetStringValue(((Enums.WF_ApprovalType)Enum.Parse(typeof(Enums.WF_ApprovalType), approvalType)))},
                                      {"[UserName]",  ((string.IsNullOrEmpty(objProxyApprover.FirstName) ? " " : objProxyApprover.FirstName) + " " + (string.IsNullOrEmpty(objProxyApprover.LastName) ? " " : objProxyApprover.LastName)).Trim()},
                                      {"[ApproverName]", ((string.IsNullOrEmpty(objProxyApprover.FirstName) ? " " : objProxyApprover.FirstName) + " " + (string.IsNullOrEmpty(objProxyApprover.LastName) ? " " : objProxyApprover.LastName)).Trim()},
                                      {"[Currency]", GetCurrencySymbol(Convert.ToString(objDTRequisition.Rows[0][SqlConstants.COL_CURRENCY].ToString(),CultureInfo.CurrentCulture))},
                                      {"[CurrencyCode]", Convert.ToString(objDTRequisition.Rows[0][SqlConstants.COL_CURRENCY].ToString())},
                                      {"[RequisitionAmount]", this.objUtils.FormatNumber(Convert.ToDecimal(objDTRequisition.Rows[0][SqlConstants.COL_REQUISITION_AMOUNT].ToString()),this.cultureInfo)},
                                      {"[RequisitionAmountWithOverallLimit]", (Convert.ToDecimal(objDTRequisition.Rows[0][SqlConstants.COL_OVERALLTOTALAMOUNT].ToString()) > 0 )  ? this.objUtils.FormatNumber(Convert.ToDecimal(objDTRequisition.Rows[0][SqlConstants.COL_OVERALLTOTALAMOUNT].ToString()),this.cultureInfo):this.objUtils.FormatNumber(Convert.ToDecimal(objDTRequisition.Rows[0][SqlConstants.COL_REQUISITION_AMOUNT].ToString()),this.cultureInfo)},
                                      {"[ApproverDetails]", strPastApprovers},
                                      {"[DeltaAmount]", String.IsNullOrEmpty(Convert.ToString(objDTRequisition.Rows[0][SqlConstants.COL_REVISIONNUMBER])) ?"":" (Delta: "+objDTRequisition.Rows[0][SqlConstants.COL_CURRENCY].ToString()+" "+this.objUtils.FormatNumber(Convert.ToDecimal(objDTRequisition.Rows[0][SqlConstants.COL_REQUISITIONTOTALCHANGE].ToString()),this.cultureInfo)+")"},
                                      {"[ProdLink]", requisitionUrl},
                                      {"[ApproveLink]",approveUrl},
                                      {"[RejectLink]",rejectUrl},
                                      {"[BuyerCompanyName]",objDTRequisition.Rows[0][SqlConstants.COL_BUYER_COMPANY_NAME].ToString()},
                                      {"[BuyerCompanyLogo]", MultiRegionConfig.GetConfig(CloudConfig.BlobURL) + "cumuluscontent/logo/" + userExecutionContext.BuyerPartnerCode + "_logo.jpg"},
                                      {"[CancelLink]",""},
                                      {SqlConstants.NOTN_SupplierCompanyName, objDTRequisition.Rows[0][SqlConstants.COL_LEGALCOMPANYNAME].ToString()},
                                      {"[ItemTotal]",this.objUtils.FormatNumber(Convert.ToDecimal(objDTRequisition.Rows[0][SqlConstants.COL_ITEMTOTAL].ToString()),this.cultureInfo)},
                                      {"[Tax]", this.objUtils.FormatNumber(Convert.ToDecimal(objDTRequisition.Rows[0][SqlConstants.COL_TAX].ToString()),this.cultureInfo)},
                                      {"[Urgent]", Convert.ToBoolean(objDTRequisition.Rows[0][SqlConstants.COL_ISURGENT]) == true ? "Urgent " : string.Empty},
                                      {"[Author]", objDTRequisition.Rows[0][SqlConstants.COL_REQUESTER_NAME].ToString().Trim()},
                                      {"[OnBehalfOf]", objDTRequisition.Rows[0][SqlConstants.COL_ONBEHALFOF].ToString().Trim()!=objDTRequisition.Rows[0][SqlConstants.COL_REQUESTERID].ToString().Trim()? objDTRequisition.Rows[0][SqlConstants.COL_ONBEHALFOFNAME].ToString().Trim():""},
                                      {"[NotificationEntitiesAsString]", notificationEntityValues },
                                      {"[NotificationEntityMessage]",notificationEntityMessage},
                                      {"[AccountingData]", accountingHTML },
                                      {"[BudgetTableLabel]", budgetHtml.Length > 0 ? budgetTableLabel : string.Empty},
                                      {"[ReviewBudgetButton]", budgetHtml.Length > 0 ? btnHTML : string.Empty },
                                      {"[BudgetDataTable]", budgetHtml.Length > 0 ? budgetHtml : string.Empty },
                                      {"[ReviewBudgetLink]", budgetHtml.Length > 0 ? reviewBudgetUrl : string.Empty },
                                      {"[Custom Attributes]", CustomAttributeHtml.ToString() },
                                      {"[RiskScore]",string.IsNullOrEmpty(objDTRequisition.Rows[0][SqlConstants.COL_RISKSCORE].ToString()) ?"--": Math.Round(Convert.ToDecimal(objDTRequisition.Rows[0][SqlConstants.COL_RISKSCORE])).ToString().Trim()},
                                      {"[Risk Category]", objDTRequisition.Rows[0][SqlConstants.COL_RISKCATEGORY].ToString().Trim()}
                                  };

                                    if (string.IsNullOrWhiteSpace(objProxyApprover.ContactCultureCode))
                                        objProxyApprover.ContactCultureCode = this.UserContext.Culture;
                                    result = SendNotification(((eventName == "OnSubmit") ? "P2P114" : "P2P128"), toMails, ccMails, fieldValues, attachments, UserContext.BuyerPartnerCode, documentCode, DocumentType.Requisition, objProxyApprover.ContactCultureCode);
                                }
                            }
                            fieldValues["[UserName]"] = objDTRequisition.Rows[0][SqlConstants.COL_REQUESTER_NAME].ToString();
                            fieldValues["[Comment]"] = "";
                            List<MailAdressInfo> toRequseterMails = new List<MailAdressInfo>();
                            long requesterId = Convert.ToInt64(objDTRequisition.Rows[0][SqlConstants.COL_REQUESTER_ID].ToString(), CultureInfo.CurrentCulture);
                            toRequseterMails.Add(new MailAdressInfo
                            {
                                ContactCode = requesterId,
                                EmailAddress = objDTRequisition.Rows[0][SqlConstants.COL_REQUESTER_EMAIL_ID].ToString()
                            });
                            var objRequester = proxyPartnerService.GetContactByContactCode(0, requesterId);//Partner code is not getting used in method GetContactByContactCode
                            if (string.IsNullOrWhiteSpace(objRequester.ContactCultureCode))
                                objRequester.ContactCultureCode = this.UserContext.Culture;
                            result = SendNotification("P2P129", toRequseterMails, ccMails, fieldValues, attachments, UserContext.BuyerPartnerCode, documentCode, DocumentType.Requisition, objRequester.ContactCultureCode);
                        }
                    }
                }
            }
            catch (CommunicationException commFaultEx)
            {
                LogHelper.LogError(Log, "Fault exception occured in EmailNotificationManager SendNotificationForRequisitionApproval Method for InvoiceId=" + documentCode, commFaultEx);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in EmailNotificationManager SendNotificationForRequisitionApproval Method for InvoiceId=" + documentCode, ex);
                throw;
            }
            return result;
        }

        private string GetQueryStringUrlForApprover(long docCode, UserExecutionContext userExecutionContext, DocumentType docType, long approverId)
        {
            Smart2Helper smrtHelper = new Smart2Helper();
            bool isRedirectToSmart2 = smrtHelper.IsRedirectToSmart2(userExecutionContext, approverId, DocumentType.Requisition);
            string queryString = RequisitionDocumentManager.GetQueryString(docCode, UserContext.BuyerPartnerCode, isRedirectToSmart2);
            string url = "";
            switch (docType)
            {
                case DocumentType.Requisition:
                    if (isRedirectToSmart2)
                        url = MultiRegionConfig.GetConfig(CloudConfig.SmartBaseURL) + "P2P?dd=" + queryString + "&b=1&oloc=227#/requisitions/" + queryString;
                    else
                        url = MultiRegionConfig.GetConfig(CloudConfig.RequisitionURL) + "?dd=" + queryString + "&b=1";
                    break;
                case DocumentType.PO:
                    if (isRedirectToSmart2)
                        url = MultiRegionConfig.GetConfig(CloudConfig.SmartBaseURL) + "P2P/Order?dd=" + RequisitionDocumentManager.EncryptURL("bpc=" + UserContext.BuyerPartnerCode + "&dc=" + docCode) + "&b=1&oloc=226#/po";
                    else
                        url = MultiRegionConfig.GetConfig(CloudConfig.OrderURL) + "?dd=" + RequisitionDocumentManager.GetQueryString(docCode, UserContext.BuyerPartnerCode) + "&b=1";
                    break;
                default:
                    break;
            }
            return url;
        }

        public bool SendNotificationForSkipOrOffLineRequisitionApproval(long documentCode, List<ApproverDetails> lstApprovers, int skipType = 0, bool isOffLine = false, long actionarId = 0)
        {
            bool result = false;
            ProxyPartnerService proxyPartnerService = new ProxyPartnerService(UserContext, this.JWTToken);
            SortedList<long, string> ccMails = new SortedList<long, string>();
            SortedList<long, string> attachments = new SortedList<long, string>();
            SortedList<string, string> fieldValues = new SortedList<string, string>();
            List<Comments> lstComments = new List<Comments>();
            Contact objSkippedUser = new Contact();
            try
            {
                LogHelper.LogInfo(Log, "EmailNotificationManager SendNotificationForSkipOrOffLineRequisitionApproval Method Started for documentCode=" + documentCode);
                UserExecutionContext userExecutionContext = UserContext;
                RequisitionCommonManager commonManager = new RequisitionCommonManager(this.JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
                string questionIDs = commonManager.GetSettingsValueByKey(P2PDocumentType.Requisition, "IncludeCustomAttributesInNotification", UserContext.UserId);
                DataSet objRequisitionDS = GetCommonDao().GetBasicDetailsByIdForNotification(documentCode, UserContext.BuyerPartnerCode, 1, questionIDs);
                StringBuilder customAttributeHtml = new StringBuilder();
                if (objRequisitionDS.Tables.Count > 2 && questionIDs != string.Empty)
                {
                    DataTable dtReqCustomAttributes = objRequisitionDS.Tables[2];
                    customAttributeHtml = CustomAttributeBuilder(dtReqCustomAttributes);
                }



                long lobId = GetCommonDao().GetLOBByDocumentCode(documentCode);
                DataSet accountingData = null;
                var accountingHTML = string.Empty;
                var accountingEntitiesConfiguration = commonManager.GetSettingsValueByKey(P2PDocumentType.None, "EmailNotificationAccountingEntities", UserContext.ContactCode, (int)SubAppCodes.P2P, "", lobId);
                if (!string.IsNullOrEmpty(accountingEntitiesConfiguration))
                {
                    accountingData = commonManager.GetAccountingDataByDocumentCode(documentCode, accountingEntitiesConfiguration, Convert.ToInt32(DocumentType.Requisition));
                    accountingHTML = GetAccountingDataHTML(accountingData);
                }

                if (objRequisitionDS != null)
                {
                    DataTable objDTRequisition = objRequisitionDS.Tables[0];

                    if (objDTRequisition.Rows.Count > 0)
                    {
                        List<DocumentProxyDetails> documentProxyDetailsLst = new List<DocumentProxyDetails>();
                        documentProxyDetailsLst = GetCommonDao().CheckOriginalApproverNotificationStatus();
                        string strApprovers = "";
                        if (lstApprovers.Count() > 0)
                        {
                            foreach (var pastApprover in lstApprovers)
                            {
                                strApprovers += pastApprover.Name + "<br/>";
                            }
                        }
                        //To get the skip user details
                        if (actionarId > 0)
                        {
                            objSkippedUser = proxyPartnerService.GetContactByContactCode(0, actionarId);
                        }
                        if (lstApprovers != null && lstApprovers.Any())
                        {
                            foreach (var approver in lstApprovers)
                            {
                                List<MailAdressInfo> toMails = new List<MailAdressInfo>();
                                var objApprover = proxyPartnerService.GetContactByContactCode(0, approver.ApproverId);//Partner code is not getting used in method GetContactByContactCode
                                if (approver.ProxyApproverId > 0)
                                {
                                    if (documentProxyDetailsLst.Where(e => e.ContactCode == approver.ApproverId && e.DoctypeId == 7 //&& e.DateFrom.Date <= approver.StatusDate.Date && e.DateTo.Date >= approver.StatusDate.Date
                                        && e.EnableOriginalApproverNotifications == true).Count() > 0)
                                        toMails.Add(new MailAdressInfo() { ContactCode = approver.ApproverId, EmailAddress = objApprover.EmailAddress });
                                }
                                else
                                    toMails.Add(new MailAdressInfo() { ContactCode = approver.ApproverId, EmailAddress = objApprover.EmailAddress });

                                #region Export Requisition for Attachments
                                if (isOffLine)
                                {
                                    for (int i = 0; i < 3 && attachments.Count <= 0; i++)
                                    {
                                        try
                                        {
                                            var objRequisitionExportManager = new RequisitionExportManager(this.JWTToken)
                                            {
                                                UserContext = UserContext,
                                                GepConfiguration = GepConfiguration
                                            };

                                            var fileDetails = objRequisitionExportManager.RequisitionExportById(documentCode, UserContext.ContactCode, 0, "4"); // Try to use AccessType enum
                                            if (fileDetails != null && fileDetails.FileId > 0)
                                                attachments.Add(fileDetails.FileId, string.Empty);
                                        }
                                        catch (Exception ex)
                                        {
                                            LogHelper.LogError(Log, string.Format("Error occured in SendNotificationForSkipOrOffLineRequisitionApproval: RequisitionExportById Method call for RequisitionID={0}, BPC={1}, ContactCode={2}, Attempt={3}", documentCode, UserContext.BuyerPartnerCode, UserContext.ContactCode, i), ex);
                                            if (i == 2)
                                            {
                                                SendFailureInternalNotificaiton(documentCode, DocumentType.Requisition, documentCode.ToString(), documentCode.ToString(), ex, "P2P233");
                                            }

                                        }
                                    }
                                }
                                #endregion Export Requisition for Attachments

                                var requsitionUrl = "";

                                #region Smart2.0

                                requsitionUrl = GetQueryStringUrlForApprover(Convert.ToInt64(objDTRequisition.Rows[0][SqlConstants.COL_REQUISITION_ID].ToString()), userExecutionContext, DocumentType.Requisition, approver.ApproverId);

                                #endregion Smart2.0

                                if (toMails.Any())
                                {
                                    fieldValues = new SortedList<string, string>
                                    {
                                        {"[Requester]", string.IsNullOrEmpty(objDTRequisition.Rows[0][SqlConstants.COL_ONBEHALFOFNAME].ToString()) ?
                                                            objDTRequisition.Rows[0][SqlConstants.COL_REQUESTER_NAME].ToString():
                                                            objDTRequisition.Rows[0][SqlConstants.COL_ONBEHALFOFNAME].ToString()},
                                        {"[RequisitionName]", objDTRequisition.Rows[0][SqlConstants.COL_REQUISITION_NAME].ToString()},
                                        {"[RequisitionNumber]", objDTRequisition.Rows[0][SqlConstants.COL_REQUISITION_NUMBER].ToString()},
                                        {"[UserName]",  ((string.IsNullOrEmpty(objApprover.FirstName) ? " " : objApprover.FirstName) + " " + (string.IsNullOrEmpty(objApprover.LastName) ? " " : objApprover.LastName)).Trim()},
                                        {"[ApproverName]", ((string.IsNullOrEmpty(objSkippedUser.FirstName) ? " " : objSkippedUser.FirstName) + " " + (string.IsNullOrEmpty(objSkippedUser.LastName) ? " " : objSkippedUser.LastName)).Trim()},//skipped user
                                        {"[Currency]", GetCurrencySymbol(Convert.ToString(objDTRequisition.Rows[0][SqlConstants.COL_CURRENCY].ToString(), CultureInfo.CurrentCulture))},
                                        {"[CurrencyCode]", Convert.ToString(objDTRequisition.Rows[0][SqlConstants.COL_CURRENCY].ToString())},
                                        {"[RequisitionAmount]", this.objUtils.FormatNumber(Convert.ToDecimal(objDTRequisition.Rows[0][SqlConstants.COL_REQUISITION_AMOUNT].ToString()),this.cultureInfo)},
                                        {"[RequisitionAmountWithOverallLimit]", (Convert.ToDecimal(objDTRequisition.Rows[0][SqlConstants.COL_OVERALLTOTALAMOUNT].ToString()) > 0 ) ? this.objUtils.FormatNumber(Convert.ToDecimal(objDTRequisition.Rows[0][SqlConstants.COL_OVERALLTOTALAMOUNT].ToString()),this.cultureInfo):this.objUtils.FormatNumber(Convert.ToDecimal(objDTRequisition.Rows[0][SqlConstants.COL_REQUISITION_AMOUNT].ToString()),this.cultureInfo)},
                                        {"[DeltaAmount]", String.IsNullOrEmpty(Convert.ToString(objDTRequisition.Rows[0][SqlConstants.COL_REVISIONNUMBER])) ?"":" (Delta: "+objDTRequisition.Rows[0][SqlConstants.COL_CURRENCY].ToString()+" "+this.objUtils.FormatNumber(Convert.ToDecimal(objDTRequisition.Rows[0][SqlConstants.COL_REQUISITIONTOTALCHANGE].ToString()),this.cultureInfo)+")"},
                                        {"[ApproverDetails]", strApprovers},
                                        {"[ProdLink]", requsitionUrl},
                                        {"[BuyerCompanyName]",objDTRequisition.Rows[0][SqlConstants.COL_BUYER_COMPANY_NAME].ToString()},
                                        {"[BuyerCompanyLogo]", MultiRegionConfig.GetConfig(CloudConfig.BlobURL) + "cumuluscontent/logo/" + userExecutionContext.BuyerPartnerCode + "_logo.jpg"},
                                        {"[CancelLink]",""},
                                        {SqlConstants.NOTN_SupplierCompanyName,objDTRequisition.Rows[0][SqlConstants.COL_LEGALCOMPANYNAME].ToString()},
                                        {"[ItemTotal]",this.objUtils.FormatNumber(Convert.ToDecimal(objDTRequisition.Rows[0][SqlConstants.COL_ITEMTOTAL].ToString()),this.cultureInfo)},
                                        {"[Tax]", this.objUtils.FormatNumber(Convert.ToDecimal(objDTRequisition.Rows[0][SqlConstants.COL_TAX].ToString()),this.cultureInfo)},
                                        {"[Urgent]", Convert.ToBoolean(objDTRequisition.Rows[0][SqlConstants.COL_ISURGENT]) == true ? "Urgent " : string.Empty},
                                        {"[Author]", objDTRequisition.Rows[0][SqlConstants.COL_REQUESTER_NAME].ToString().Trim()},
                                        {"[OnBehalfOf]", objDTRequisition.Rows[0][SqlConstants.COL_ONBEHALFOFNAME].ToString().Trim()},
                                        {"[AccountingData]", accountingHTML },
                                        {"[Custom Attributes]", customAttributeHtml.ToString() }
                                    };

                                    if (string.IsNullOrWhiteSpace(objApprover.ContactCultureCode))
                                        objApprover.ContactCultureCode = this.UserContext.Culture;

                                    string eventCode = isOffLine ? "P2P233" : ((skipType == (int)Enums.WFSkipType.UrgentSkip) ? "P2P231" : "P2P232"); // P2P231-- Urgent skip ; P2P232--Manual skip ; P2P233--Offline approval

                                    result = SendNotification(eventCode, toMails, ccMails, fieldValues, attachments, UserContext.BuyerPartnerCode, documentCode, DocumentType.Requisition, objApprover.ContactCultureCode);
                                }
                            }
                        }
                    }
                }
            }
            catch (CommunicationException commFaultEx)
            {
                LogHelper.LogError(Log, "Fault exception occured in EmailNotificationManager SendNotificationForSkipOrOffLineRequisitionApproval Method for InvoiceId=" + documentCode, commFaultEx);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in EmailNotificationManager SendNotificationForSkipOrOffLineRequisitionApproval Method for InvoiceId=" + documentCode, ex);
                throw;
            }
            return result;
        }

        public bool SendNotificationForWithdrawRequisition(long documentCode, DocumentStatus documentStatus)
        {
            bool result = false;
            var commonManager = new RequisitionCommonManager(this.JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };

            try
            {
                LogHelper.LogInfo(Log, string.Format("EmailNotificationManager SendNotificationForWithdrawRequisition Method Started for documentCode={0}, documentStatus={1}", documentCode, documentStatus));
                
                string questionIDs = commonManager.GetSettingsValueByKey(P2PDocumentType.Requisition, "IncludeCustomAttributesInNotification", UserContext.UserId);
                DataSet objRequisitionDS = GetCommonDao().GetBasicDetailsByIdForNotification(documentCode, UserContext.BuyerPartnerCode, 1, questionIDs);
                DataTable objRequisition = objRequisitionDS.Tables[0];

                //Custom Attribute Table Builder. Note: customAttributeHtml is being (converted and) passed as a string and not a stringbuilder.
                StringBuilder customAttributeHtml = new StringBuilder();
                if (objRequisitionDS.Tables.Count > 2 && questionIDs != string.Empty)
                {
                    DataTable dtReqCustomAttributes = objRequisitionDS.Tables[2];
                    customAttributeHtml = CustomAttributeBuilder(dtReqCustomAttributes);
                }



                if (objRequisition != null && objRequisition.Rows.Count > 0)
                {

                    List<MailAdressInfo> toMails = new List<MailAdressInfo>();

                    //requistion creator
                    var requesterName = objRequisition.Rows[0][SqlConstants.COL_REQUESTER_NAME].ToString();
                    var requesterId = (long)objRequisition.Rows[0][SqlConstants.COL_REQUESTER_ID];
                    var requesterEmailID = objRequisition.Rows[0][SqlConstants.COL_REQUESTER_EMAIL_ID].ToString();
                    toMails.Add(new MailAdressInfo() { ContactCode = requesterId, EmailAddress = requesterEmailID });
                    result = SendFieldValueRequisitionWithdrawn(toMails, objRequisition, requesterName, requesterName, documentCode, customAttributeHtml.ToString());
                    toMails.Clear();

                    //OBO requester
                    var OBORequester = objRequisition.Rows[0][SqlConstants.COL_ONBEHALFOFNAME].ToString();
                    var OBOContactCode = (long)objRequisition.Rows[0][SqlConstants.COL_ONBEHALFOF];
                    var OBOEmail = objRequisition.Rows[0][SqlConstants.COL_ONBEHALFOFEMAIL].ToString();
                    if (OBOContactCode > 0 && OBOContactCode != requesterId)
                    {
                        toMails.Add(new MailAdressInfo() { ContactCode = OBOContactCode, EmailAddress = OBOEmail });
                        result = SendFieldValueRequisitionWithdrawn(toMails, objRequisition, OBORequester, requesterName, documentCode, customAttributeHtml.ToString());
                        toMails.Clear();
                    }


                    switch (documentStatus)
                    {
                        case DocumentStatus.ApprovalPending:

                            var lstApproverDetails = new List<ApproverDetails>();
                            var lstPendingApprovers = new List<ApproverDetails>();
                            var documentProxyDetailsLst = GetCommonDao().CheckOriginalApproverNotificationStatus();

                            lstApproverDetails = commonManager.GetActionersDetails(documentCode, (int)DocumentType.Requisition);

                            //Finding Approver Details of Approval Pending(status=2)
                            lstApproverDetails = lstApproverDetails.FindAll(s => s.Status == 2);

                            lstApproverDetails.Where(e => !e.IsProcessed && e.Status == 2 && e.WorkflowId != 11).ToList().ForEach(e => lstPendingApprovers.Add(e));
                            lstApproverDetails.Where(e => !e.IsProcessed && e.Status == 2 && e.WorkflowId == 11 && e.SubIsProcessed == false).ToList().ForEach(e => lstPendingApprovers.Add(e));

                            Contact objApprover = null;
                            Contact objProxyApprover = null;

                            foreach (var approver in lstPendingApprovers)
                            {
                                objApprover = GetCommonDao().GetContactDetailsByContactCode(approver.ApproverId);//Partner code is not getting used in method GetContactByContactCode
                                if (objApprover != null && approver.ProxyApproverId > 0)
                                    objProxyApprover = GetCommonDao().GetContactDetailsByContactCode(approver.ProxyApproverId);
                                if (documentProxyDetailsLst.Where(e => e.ContactCode == objApprover.ContactCode && e.DoctypeId == (int)DocumentType.Requisition
                                       && e.DateFrom.Date <= approver.StatusDate.Date && e.DateTo.Date >= approver.StatusDate.Date
                                       && e.EnableOriginalApproverNotifications == false).Count() > 0)
                                    objApprover = null;


                                if (objApprover != null)
                                {
                                    toMails.Clear();
                                    toMails.Add(new MailAdressInfo() { ContactCode = objApprover.ContactCode, EmailAddress = objApprover.EmailAddress });
                                    //For original approvers with approvalpending
                                    result = SendFieldValueRequisitionWithdrawn(toMails, objRequisition, (objApprover.FirstName + " " + objApprover.LastName).Trim(), requesterName, documentCode, customAttributeHtml.ToString());
                                }
                                if (objProxyApprover != null)
                                {
                                    toMails.Clear();
                                    toMails.Add(new MailAdressInfo() { ContactCode = objProxyApprover.ContactCode, EmailAddress = objProxyApprover.EmailAddress });
                                    //For delegate approvers
                                    result = SendFieldValueRequisitionWithdrawn(toMails, objRequisition, (objProxyApprover.FirstName + " " + objProxyApprover.LastName).Trim(), requesterName, documentCode, customAttributeHtml.ToString());
                                }
                            }


                            break;
                        case DocumentStatus.Approved:

                            //var commonManager = new RequisitionCommonManager(this.JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
                            var buyerAssignmentSettingValue = commonManager.GetSettingsValueByKey(P2PDocumentType.None, "RequisitionStatusForBuyerAssignment", UserContext.ContactCode).ToLower();
                            if (!string.IsNullOrEmpty(buyerAssignmentSettingValue))
                            {
                                var ordercontactEmail = objRequisition.Rows[0][SqlConstants.COL_ORDER_CONTACT_EMAIL].ToString();
                                var orderContactCode = (long)objRequisition.Rows[0][SqlConstants.COL_ORDER_CONTACTCODE];

                                if (orderContactCode > 0 && !string.IsNullOrEmpty(ordercontactEmail))
                                {
                                    toMails.Clear();
                                    toMails.Add(new MailAdressInfo() { ContactCode = orderContactCode, EmailAddress = ordercontactEmail });
                                    result = SendFieldValueRequisitionWithdrawn(toMails, objRequisition, objRequisition.Rows[0][SqlConstants.COL_ORDER_CONTACT_NAME].ToString(), requesterName, documentCode, customAttributeHtml.ToString());
                                }
                            }

                            break;
                    }

                }
            }
            catch (CommunicationException commFaultEx)
            {
                LogHelper.LogError(Log, string.Format("Fault exception occured in EmailNotificationManager SendNotificationForWithdrawRequisition Method for requisitionId={0}, documentStatus={1}", documentCode, documentStatus), commFaultEx);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, string.Format("Error occured in EmailNotificationManager SendNotificationForWithdrawRequisition Method for requisitionId={0}, documentStatus={1}", documentCode, documentStatus), ex);
                throw;
            }
            return result;
        }
        private bool SendFieldValueRequisitionWithdrawn(List<MailAdressInfo> toMails, DataTable objRequisition, string userName, string requesterName, long documentCode, string customAttributeHTML)
        {
            var ccMails = new SortedList<long, string>();
            var attachments = new SortedList<long, string>();


            var fieldValues = new SortedList<string, string>
                            {
                                {"[RequisitionNumber]", objRequisition.Rows[0][SqlConstants.COL_REQUISITION_NUMBER].ToString()},
                                {"[RequisitionTotalAmount]", (Convert.ToDecimal(objRequisition.Rows[0][SqlConstants.COL_OVERALLTOTALAMOUNT].ToString()) > 0 )  ? this.objUtils.FormatNumber(Convert.ToDecimal(objRequisition.Rows[0][SqlConstants.COL_OVERALLTOTALAMOUNT].ToString()),this.cultureInfo):this.objUtils.FormatNumber(Convert.ToDecimal(objRequisition.Rows[0][SqlConstants.COL_REQUISITION_AMOUNT].ToString()),this.cultureInfo)},
                                {"[UserName]",userName},
                                {"[Currency]", GetCurrencySymbol(objRequisition.Rows[0][SqlConstants.COL_CURRENCY].ToString())},
                                {"[WithdrawnBy]", UserContext.UserName},
                                {"[WithdrawnDate]", DateTime.Now.ToString()},
                                {"[RequisitionAuthor]",requesterName},
                                {"[OBORequestor]",objRequisition.Rows[0][SqlConstants.COL_ONBEHALFOFNAME].ToString()},
                                {"[BuyerCompanyName]",Convert.ToString(objRequisition.Rows[0][SqlConstants.COL_BUYER_COMPANY_NAME],CultureInfo.CurrentCulture)},
                                {"[BuyerCompanyLogo]", MultiRegionConfig.GetConfig(CloudConfig.BlobURL) + "cumuluscontent/logo/" + UserContext.BuyerPartnerCode + "_logo.jpg"},
                                {"[Custom Attributes]", customAttributeHTML }
                            };

            #region Export Requisition for Attachments

            var objRequisitionExportManager = new RequisitionExportManager(this.JWTToken)
            {
                UserContext = UserContext,
                GepConfiguration = GepConfiguration
            };

            var fileDetails = objRequisitionExportManager.RequisitionExportById(documentCode, UserContext.ContactCode, 0, ((int)AccessType.Both).ToString());
            if (fileDetails != null && fileDetails.FileId > 0)
                attachments.Add(fileDetails.FileId, string.Empty);

            #endregion Export Requisition for Attachments

            return SendNotification("P2P238", toMails, ccMails, fieldValues, attachments, UserContext.BuyerPartnerCode, documentCode, DocumentType.Requisition);
        }



        public bool SendNotificationForRequisitionReview(long documentCode, List<ReviewerDetails> lstPendingReviewer, List<ReviewerDetails> lstPastReviewer, string eventName, DocumentStatus documentStatus, string reviewType)
        {
            bool result = false;

            try
            {
                LogNewRelicApp("Start SendNotificationForRequisitionReview, LN 1690", documentCode);
                LogHelper.LogInfo(Log, "EmailNotificationManager SendNotificationForRequisitionReview Method Started for documentCode =" + documentCode);

                UserExecutionContext userExecutionContext = UserContext;
                ProxyPartnerService proxyPartnerService = new ProxyPartnerService(UserContext, this.JWTToken);
                SortedList<long, string> attachments = new SortedList<long, string>();
                SortedList<string, string> fieldValues = new SortedList<string, string>();
                SortedList<long, string> ccMails = new SortedList<long, string>();

                LogNewRelicApp("SendNotificationForRequisitionReview after proxyPartnerService , LN 1699", documentCode);

                string reviewDetails = "";
                var commonManager = new RequisitionCommonManager(this.JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
                string questionIDs = commonManager.GetSettingsValueByKey(P2PDocumentType.Requisition, "IncludeCustomAttributesInNotification", UserContext.UserId);
                bool requireSecureMail = Convert.ToBoolean(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "SendSecureMailAction", UserContext.UserId));
                DataSet objRequisitionDS = GetCommonDao().GetBasicDetailsByIdForNotification(documentCode, userExecutionContext.BuyerPartnerCode, 1, questionIDs);

                DataTable objRequisition = objRequisitionDS.Tables[0];
                StringBuilder CustomAttributeHtml = new StringBuilder();
                if (objRequisitionDS.Tables.Count > 2 && questionIDs != string.Empty)
                {
                    DataTable dtReqCustomAttributes = objRequisitionDS.Tables[2];
                    CustomAttributeHtml = CustomAttributeBuilder(dtReqCustomAttributes);
                }

                if (objRequisition != null && objRequisition.Rows.Count > 0)
                {

                    //get requestor information
                    var OBOContactCode = (long)objRequisition.Rows[0][SqlConstants.COL_ONBEHALFOF];
                    List<DocumentProxyDetails> documentProxyDetailsLst = new List<DocumentProxyDetails>();
                    documentProxyDetailsLst = GetCommonDao().CheckOriginalReviewerNotificationStatus();
                    var RequesterContactCode = (long)objRequisition.Rows[0][SqlConstants.COL_REQUESTER_ID];


                    if (eventName == "Intermediate")
                    {
                        foreach (var pastReviewer in lstPastReviewer)
                        {
                            if (pastReviewer.ActionerType == 4)
                                reviewDetails += pastReviewer.ProxyReviewerName + " reviewed this Requisition at " + pastReviewer.StatusDate.ToString("dd/MM/yyyy HH:mm") + "<br/>";
                            else
                                reviewDetails += pastReviewer.Name + " reviewed this Requisition at " + pastReviewer.StatusDate.ToString("dd/MM/yyyy HH:mm") + "<br/>";
                        }
                    }

                    #region Export Requisition for Attachments

                    for (int i = 0; i < 3 && attachments.Count <= 0; i++)
                    {
                        try
                        {
                            RequisitionExportManager objRequisitionExportManager = new RequisitionExportManager(this.JWTToken)
                            {
                                UserContext = UserContext,
                                GepConfiguration = GepConfiguration
                            };

                            LogNewRelicApp("SendNotificationForRequisitionReview (LN 1747) RequisitionExportManager", documentCode);

                            var fileDetails = objRequisitionExportManager.RequisitionExportById(documentCode, UserContext.ContactCode, 0, "4");

                            if (fileDetails != null && fileDetails.FileId > 0)
                                attachments.Add(fileDetails.FileId, string.Empty);
                        }
                        catch (Exception ex)
                        {
                            LogHelper.LogError(Log, string.Format("Error occured in SendNotificationForRequisitionReview: RequisitionExportById Method call for RequisitionID={0}, BPC={1}, ContactCode={2}, Attempt={3}", documentCode, UserContext.BuyerPartnerCode, UserContext.ContactCode, i), ex);
                            if (i == 2)
                            {
                                SendFailureInternalNotificaiton(documentCode, DocumentType.Requisition, documentCode.ToString(), documentCode.ToString(), ex, "P2P227/P2P228");
                            }
                        }
                    }

                    #endregion Export Requisition for Attachments

                    if (lstPendingReviewer != null && lstPendingReviewer.Any())
                    {
                        //    ccMails.Add(OBOContactCode, OBOContactEmail);

                        foreach (var reviewer in lstPendingReviewer)
                        {
                            List<MailAdressInfo> toMails = new List<MailAdressInfo>();
                            Contact objReviewer = new Contact();

                            objReviewer = proxyPartnerService.GetContactByContactCode(0, reviewer.ReviewerId);
                            if (reviewer.ProxyReviewerId > 0)
                            {
                                if (documentProxyDetailsLst.Where(e => e.ContactCode == reviewer.ReviewerId && e.DoctypeId == 7 && e.DateFrom.Date <= reviewer.StatusDate.Date && e.DateTo.Date >= reviewer.StatusDate.Date
                                    && e.EnableOriginalApproverNotifications == true).Count() > 0)
                                    toMails.Add(new MailAdressInfo() { ContactCode = reviewer.ReviewerId, EmailAddress = objReviewer.EmailAddress });
                            }
                            else
                                toMails.Add(new MailAdressInfo() { ContactCode = reviewer.ReviewerId, EmailAddress = objReviewer.EmailAddress });

                            string requisitionUrl = GetQueryStringUrlForApprover(Convert.ToInt64(objRequisition.Rows[0][SqlConstants.COL_REQUISITION_ID].ToString()), UserContext, DocumentType.Requisition, (long)objRequisition.Rows[0][SqlConstants.COL_ONBEHALFOF]);

                            if (toMails.Any())
                            {
                                var acceptUrl = RequisitionDocumentManager.EncryptURL("evc=" + ((eventName == "OnSubmit") ? "P2P227" : "P2P228") + "&bpc=" + UserContext.BuyerPartnerCode + "&dc=" + objRequisition.Rows[0][SqlConstants.COL_REQUISITION_ID] +
                                                                               "&cc=" + reviewer.ReviewerId + "&action=" + DocumentStatus.Accepted + "&dt=" + DateTime.UtcNow);

                                var rejectUrl = RequisitionDocumentManager.EncryptURL("evc=" + ((eventName == "OnSubmit") ? "P2P227" : "P2P228") + "&bpc=" + UserContext.BuyerPartnerCode + "&dc=" + objRequisition.Rows[0][SqlConstants.COL_REQUISITION_ID] +
                                                                              "&cc=" + reviewer.ReviewerId + "&action=" + DocumentStatus.Rejected + "&dt=" + DateTime.UtcNow);

                                
                                acceptUrl = GetMailActionURL(requireSecureMail, commonManager) + "?documentType=" + DocumentType.Requisition + "&dtl=" + acceptUrl + "&oloc=" + (int)SubAppCodes.P2P + "&b=1";
                                rejectUrl = GetMailActionURL(requireSecureMail, commonManager) + "?documentType=" + DocumentType.Requisition + "&dtl=" + rejectUrl + "&oloc=" + (int)SubAppCodes.P2P + "&b=1";

                                fieldValues = new SortedList<string, string>
                                    {
                                        {"[Requester]", string.IsNullOrEmpty(objRequisition.Rows[0][SqlConstants.COL_ONBEHALFOFNAME].ToString()) ?
                                                            objRequisition.Rows[0][SqlConstants.COL_REQUESTER_NAME].ToString():
                                                            objRequisition.Rows[0][SqlConstants.COL_ONBEHALFOFNAME].ToString()},
                                        {"[RequisitionName]", objRequisition.Rows[0][SqlConstants.COL_REQUISITION_NAME].ToString()},
                                        {"[RequisitionNumber]", objRequisition.Rows[0][SqlConstants.COL_REQUISITION_NUMBER].ToString()},
                                        {"[UserName]",  ((string.IsNullOrEmpty(objReviewer.FirstName) ? " " : objReviewer.FirstName) + " " + (string.IsNullOrEmpty(objReviewer.LastName) ? " " : objReviewer.LastName)).Trim()},
                                        {"[ReviewerName]", ((string.IsNullOrEmpty(objReviewer.FirstName) ? " " : objReviewer.FirstName) + " " + (string.IsNullOrEmpty(objReviewer.LastName) ? " " : objReviewer.LastName)).Trim()},
                                        {"[Currency]", GetCurrencySymbol(Convert.ToString(objRequisition.Rows[0][SqlConstants.COL_CURRENCY].ToString(), CultureInfo.CurrentCulture))},
                                        {"[RequisitionAmount]", this.objUtils.FormatNumber(Convert.ToDecimal(objRequisition.Rows[0][SqlConstants.COL_REQUISITION_AMOUNT].ToString()),this.cultureInfo)},
                                        {"[ReviewerDetails]", reviewDetails},
                                        {"[ProdLink]", requisitionUrl},
                                        {"[AcceptLink]",acceptUrl},
                                        {"[RejectLink]",rejectUrl},
                                        {"[BuyerCompanyName]",objRequisition.Rows[0][SqlConstants.COL_BUYER_COMPANY_NAME].ToString()},
                                        {"[BuyerCompanyLogo]", MultiRegionConfig.GetConfig(CloudConfig.BlobURL) + "cumuluscontent/logo/" + userExecutionContext.BuyerPartnerCode + "_logo.jpg"},
                                        {SqlConstants.NOTN_SupplierCompanyName,objRequisition.Rows[0][SqlConstants.COL_LEGALCOMPANYNAME].ToString()},
                                        {"[ItemTotal]",this.objUtils.FormatNumber(Convert.ToDecimal(objRequisition.Rows[0][SqlConstants.COL_ITEMTOTAL].ToString()),this.cultureInfo)},
                                        {"[Tax]", this.objUtils.FormatNumber(Convert.ToDecimal(objRequisition.Rows[0][SqlConstants.COL_TAX].ToString()),this.cultureInfo)},
                                        {"[Urgent]", Convert.ToBoolean(objRequisition.Rows[0][SqlConstants.COL_ISURGENT]) == true ? "Urgent " : string.Empty},
                                        {"[Author]", objRequisition.Rows[0][SqlConstants.COL_REQUESTER_NAME].ToString().Trim()},
                                        {"[OnBehalfOf]", OBOContactCode != RequesterContactCode ? objRequisition.Rows[0][SqlConstants.COL_ONBEHALFOFNAME].ToString().Trim():""},
                                        {"[Custom Attributes]", CustomAttributeHtml.ToString() },
                                         {"[RiskScore]",string.IsNullOrEmpty(objRequisition.Rows[0][SqlConstants.COL_RISKSCORE].ToString()) ?"--": Math.Round(Convert.ToDecimal(objRequisition.Rows[0][SqlConstants.COL_RISKSCORE])).ToString().Trim()},
                                         {"[Risk Category]", objRequisition.Rows[0][SqlConstants.COL_RISKCATEGORY].ToString().Trim()}
                                    };
                                if (string.IsNullOrWhiteSpace(objReviewer.ContactCultureCode))
                                    objReviewer.ContactCultureCode = this.UserContext.Culture;
                                result = SendNotification(((eventName == "OnSubmit") ? "P2P227" : "P2P228"), toMails, ccMails, fieldValues, attachments, UserContext.BuyerPartnerCode, documentCode, DocumentType.Requisition, objReviewer.ContactCultureCode);
                            }

                            if (reviewer.ProxyReviewerId > 0)
                            {
                                var objProxyReviewer = proxyPartnerService.GetContactByContactCode(0, reviewer.ProxyReviewerId);
                                toMails.Clear();
                                fieldValues.Clear();
                                toMails.Add(new MailAdressInfo() { ContactCode = reviewer.ProxyReviewerId, EmailAddress = objProxyReviewer.EmailAddress });

                                var acceptUrl = RequisitionDocumentManager.EncryptURL("evc=" + ((eventName == "OnSubmit") ? "P2P227" : "P2P228") + "&bpc=" + UserContext.BuyerPartnerCode + "&dc=" + objRequisition.Rows[0][SqlConstants.COL_REQUISITION_ID] +
                                                                               "&cc=" + reviewer.ProxyReviewerId + "&action=" + DocumentStatus.Accepted + "&dt=" + DateTime.UtcNow);

                                var rejectUrl = RequisitionDocumentManager.EncryptURL("evc=" + ((eventName == "OnSubmit") ? "P2P227" : "P2P228") + "&bpc=" + UserContext.BuyerPartnerCode + "&dc=" + objRequisition.Rows[0][SqlConstants.COL_REQUISITION_ID] +
                                                                              "&cc=" + reviewer.ProxyReviewerId + "&action=" + DocumentStatus.Rejected + "&dt=" + DateTime.UtcNow);

                                acceptUrl = GetMailActionURL(requireSecureMail, commonManager) + "?documentType=" + DocumentType.Requisition + "&dtl=" + acceptUrl + "&oloc=" + (int)SubAppCodes.P2P + "&b=1";
                                rejectUrl = GetMailActionURL(requireSecureMail, commonManager) + "?documentType=" + DocumentType.Requisition + "&dtl=" + rejectUrl + "&oloc=" + (int)SubAppCodes.P2P + "&b=1";

                                fieldValues = new SortedList<string, string>
                                    {
                                        {"[Requester]", string.IsNullOrEmpty(objRequisition.Rows[0][SqlConstants.COL_ONBEHALFOFNAME].ToString()) ?
                                                            objRequisition.Rows[0][SqlConstants.COL_REQUESTER_NAME].ToString():
                                                            objRequisition.Rows[0][SqlConstants.COL_ONBEHALFOFNAME].ToString()},
                                        {"[RequisitionName]", objRequisition.Rows[0][SqlConstants.COL_REQUISITION_NAME].ToString()},
                                        {"[RequisitionNumber]", objRequisition.Rows[0][SqlConstants.COL_REQUISITION_NUMBER].ToString()},
                                        {"[UserName]",  ((string.IsNullOrEmpty(objProxyReviewer.FirstName) ? " " : objProxyReviewer.FirstName) + " " + (string.IsNullOrEmpty(objProxyReviewer.LastName) ? " " : objProxyReviewer.LastName)).Trim()},
                                        {"[ReviewerName]", ((string.IsNullOrEmpty(objProxyReviewer.FirstName) ? " " : objProxyReviewer.FirstName) + " " + (string.IsNullOrEmpty(objProxyReviewer.LastName) ? " " : objProxyReviewer.LastName)).Trim()},
                                        {"[Currency]", GetCurrencySymbol(Convert.ToString(objRequisition.Rows[0][SqlConstants.COL_CURRENCY].ToString(), CultureInfo.CurrentCulture))},
                                        {"[RequisitionAmount]", this.objUtils.FormatNumber(Convert.ToDecimal(objRequisition.Rows[0][SqlConstants.COL_REQUISITION_AMOUNT].ToString()),this.cultureInfo)},
                                        {"[ReviewerDetails]", reviewDetails},
                                        {"[ProdLink]", requisitionUrl},
                                        {"[AcceptLink]",acceptUrl},
                                        {"[RejectLink]",rejectUrl},
                                        {"[BuyerCompanyName]",objRequisition.Rows[0][SqlConstants.COL_BUYER_COMPANY_NAME].ToString()},
                                        {"[BuyerCompanyLogo]", MultiRegionConfig.GetConfig(CloudConfig.BlobURL) + "cumuluscontent/logo/" + userExecutionContext.BuyerPartnerCode + "_logo.jpg"},
                                        {SqlConstants.NOTN_SupplierCompanyName,objRequisition.Rows[0][SqlConstants.COL_LEGALCOMPANYNAME].ToString()},
                                        {"[ItemTotal]",this.objUtils.FormatNumber(Convert.ToDecimal(objRequisition.Rows[0][SqlConstants.COL_ITEMTOTAL].ToString()),this.cultureInfo)},
                                        {"[Tax]", this.objUtils.FormatNumber(Convert.ToDecimal(objRequisition.Rows[0][SqlConstants.COL_TAX].ToString()),this.cultureInfo)},
                                        {"[Urgent]", Convert.ToBoolean(objRequisition.Rows[0][SqlConstants.COL_ISURGENT]) == true ? "Urgent " : string.Empty},
                                        {"[Author]", objRequisition.Rows[0][SqlConstants.COL_REQUESTER_NAME].ToString().Trim()},
                                        {"[OnBehalfOf]", OBOContactCode != RequesterContactCode ? objRequisition.Rows[0][SqlConstants.COL_ONBEHALFOFNAME].ToString().Trim():""},
                                        {"[Custom Attributes]", CustomAttributeHtml.ToString() },
                                        {"[RiskScore]", string.IsNullOrEmpty(objRequisition.Rows[0][SqlConstants.COL_RISKSCORE].ToString()) ?"--":Math.Round(Convert.ToDecimal(objRequisition.Rows[0][SqlConstants.COL_RISKSCORE])).ToString().Trim()},
                                        {"[Risk Category]", objRequisition.Rows[0][SqlConstants.COL_RISKCATEGORY].ToString().Trim()}
                                    };
                                if (string.IsNullOrWhiteSpace(objProxyReviewer.ContactCultureCode))
                                    objProxyReviewer.ContactCultureCode = this.UserContext.Culture;
                                result = SendNotification(((eventName == "OnSubmit") ? "P2P227" : "P2P228"), toMails, ccMails, fieldValues, attachments, UserContext.BuyerPartnerCode, documentCode, DocumentType.Requisition, objProxyReviewer.ContactCultureCode);
                            }
                        }
                    }

                    if (RequesterContactCode != OBOContactCode && eventName == "OnSubmit")
                    {
                        SendNotificationForOBORequester(documentCode, objRequisitionDS, attachments);
                    }
                }
            }
            catch (CommunicationException commFaultEx)
            {
                LogNewRelicApp("SendNotificationForRequisitionReview (LN 1885) Exception :" + commFaultEx.Message, documentCode);

                LogHelper.LogError(Log, "Fault exception occured in EmailNotificationManager SendNotificationForRequisitionReview Method for documentCode =" + documentCode, commFaultEx);
            }
            catch (Exception ex)
            {
                LogNewRelicApp("SendNotificationForRequisitionReview (LN 1890) Exception :" + ex.Message + " Stack:" + ex.StackTrace, documentCode);
                LogHelper.LogError(Log, "Error occured in EmailNotificationManager SendNotificationForRequisitionReview Method for documentCode =" + documentCode, ex);
                throw;
            }

            LogNewRelicApp("SendNotificationForRequisitionReview (LN 1896) Result :" + result.ToString(), documentCode);
            return result;
        }
        public void SendNotificationForOBORequester(long documentCode, DataSet objRequisitionDS, SortedList<long, string> attachments)
        {
            try
            {
                LogHelper.LogInfo(Log, "EmailNotificationManager SendNotificationForOBORequester Method Started for documentCode =" + documentCode);
                UserExecutionContext userExecutionContext = UserContext;
                DataTable objRequisition = objRequisitionDS.Tables[0];
                StringBuilder CustomAttributeHtml = new StringBuilder();
                if (objRequisitionDS.Tables.Count > 2)
                {
                    DataTable dtReqCustomAttributes = objRequisitionDS.Tables[2];
                    CustomAttributeHtml = CustomAttributeBuilder(dtReqCustomAttributes);
                }
                if (objRequisition != null && objRequisition.Rows.Count > 0)
                {
                    List<MailAdressInfo> toMails = new List<MailAdressInfo>();
                    //To - OBO User
                    SortedList<string, string> fieldValues = new SortedList<string, string>();
                    SortedList<long, string> ccMails = new SortedList<long, string>();

                    var OBOContactCode = (long)objRequisition.Rows[0][SqlConstants.COL_ONBEHALFOF];
                    var OBOContactName = (string)objRequisition.Rows[0][SqlConstants.COL_ONBEHALFOFNAME];
                    var OBOContactEmail = (string)objRequisition.Rows[0][SqlConstants.COL_ONBEHALFOFEMAIL];

                    toMails.Add(new MailAdressInfo() { ContactCode = OBOContactCode, EmailAddress = OBOContactEmail });

                    Contact objOBOUser = new Contact();
                    ProxyPartnerService proxyPartnerService = new ProxyPartnerService(UserContext, this.JWTToken);
                    objOBOUser = proxyPartnerService.GetContactByContactCode(0, OBOContactCode);

                    string requisitionUrl = GetQueryStringUrlForApprover(Convert.ToInt64(objRequisition.Rows[0][SqlConstants.COL_REQUISITION_ID].ToString()), UserContext, DocumentType.Requisition, (long)objRequisition.Rows[0][SqlConstants.COL_ONBEHALFOF]);

                    if (toMails.Any())
                    {
                        fieldValues = new SortedList<string, string>
                                    {
                                        {"[Requester]", objRequisition.Rows[0][SqlConstants.COL_REQUESTER_NAME].ToString() },
                                        {"[RequisitionName]", objRequisition.Rows[0][SqlConstants.COL_REQUISITION_NAME].ToString()},
                                        {"[RequisitionNumber]", objRequisition.Rows[0][SqlConstants.COL_REQUISITION_NUMBER].ToString()},
                                        {"[UserName]",  ((string.IsNullOrEmpty(objOBOUser.FirstName) ? " " : objOBOUser.FirstName) + " " + (string.IsNullOrEmpty(objOBOUser.LastName) ? " " : objOBOUser.LastName)).Trim()},
                                        {"[Currency]", GetCurrencySymbol(Convert.ToString(objRequisition.Rows[0][SqlConstants.COL_CURRENCY].ToString(), CultureInfo.CurrentCulture))},
                                        {"[RequisitionAmount]", this.objUtils.FormatNumber(Convert.ToDecimal(objRequisition.Rows[0][SqlConstants.COL_REQUISITION_AMOUNT].ToString()),this.cultureInfo)},
                                        {"[ProdLink]", requisitionUrl},
                                        {"[BuyerCompanyName]",objRequisition.Rows[0][SqlConstants.COL_BUYER_COMPANY_NAME].ToString()},
                                        {"[BuyerCompanyLogo]", MultiRegionConfig.GetConfig(CloudConfig.BlobURL) + "cumuluscontent/logo/" + userExecutionContext.BuyerPartnerCode + "_logo.jpg"},
                                        {SqlConstants.NOTN_SupplierCompanyName,objRequisition.Rows[0][SqlConstants.COL_LEGALCOMPANYNAME].ToString()},
                                        {"[ItemTotal]",this.objUtils.FormatNumber(Convert.ToDecimal(objRequisition.Rows[0][SqlConstants.COL_ITEMTOTAL].ToString()),this.cultureInfo)},
                                        {"[Tax]", this.objUtils.FormatNumber(Convert.ToDecimal(objRequisition.Rows[0][SqlConstants.COL_TAX].ToString()),this.cultureInfo)},
                                        {"[Urgent]", Convert.ToBoolean(objRequisition.Rows[0][SqlConstants.COL_ISURGENT]) == true ? "Urgent " : string.Empty},
                                        {"[Author]", objRequisition.Rows[0][SqlConstants.COL_REQUESTER_NAME].ToString().Trim()},
                                        {"[OnBehalfOf]", objRequisition.Rows[0][SqlConstants.COL_ONBEHALFOFNAME].ToString().Trim()},
                                        {"[Custom Attributes]", CustomAttributeHtml.ToString() },
                                        {"[RiskScore]",string.IsNullOrEmpty(objRequisition.Rows[0][SqlConstants.COL_RISKSCORE].ToString()) ?"--":  Math.Round(Convert.ToDecimal(objRequisition.Rows[0][SqlConstants.COL_RISKSCORE])).ToString().Trim()},
                                        {"[Risk Category]", objRequisition.Rows[0][SqlConstants.COL_RISKCATEGORY].ToString().Trim()}
                                    };
                        if (string.IsNullOrWhiteSpace(objOBOUser.ContactCultureCode))
                            objOBOUser.ContactCultureCode = this.UserContext.Culture;

                        SendNotification("P2P241", toMails, ccMails, fieldValues, attachments, UserContext.BuyerPartnerCode, documentCode, DocumentType.Requisition, objOBOUser.ContactCultureCode);
                    }
                }
            }
            catch (CommunicationException commFaultEx)
            {
                LogHelper.LogError(Log, "Fault exception occured in EmailNotificationManager SendNotificationForRequisitionReview Method for documentCode =" + documentCode, commFaultEx);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in EmailNotificationManager SendNotificationForRequisitionReview Method for documentCode =" + documentCode, ex);
                throw;
            }
        }
        public bool SendNotificationForReviewRejectedRequisition(long requisitionId, ReviewerDetails rejector, List<ReviewerDetails> prevReviewers, string queryString)
        {
            LogHelper.LogInfo(Log, "EmailNotificationManager SendNotificationForReviewRejectedRequisition Method Started for requisitionId=" + requisitionId);

            bool result = false;
            ProxyPartnerService proxyPartnerService = new ProxyPartnerService(UserContext, this.JWTToken);
            SortedList<long, string> ccMails = new SortedList<long, string>();
            SortedList<long, string> attachments = new SortedList<long, string>();
            SortedList<string, string> fieldValues = new SortedList<string, string>();

            try
            {
                var commonManager = new RequisitionCommonManager(this.JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
                string questionIDs = commonManager.GetSettingsValueByKey(P2PDocumentType.Requisition, "IncludeCustomAttributesInNotification", UserContext.UserId);
                DataSet objRequisitionDS = GetCommonDao().GetBasicDetailsByIdForNotification(requisitionId, UserContext.BuyerPartnerCode, 1, questionIDs);
                StringBuilder customAttributeHtml = new StringBuilder();
                if (objRequisitionDS.Tables.Count > 2 && questionIDs != string.Empty)
                {
                    DataTable dtReqCustomAttributes = objRequisitionDS.Tables[2];
                    customAttributeHtml = CustomAttributeBuilder(dtReqCustomAttributes);
                }

                DataTable objRequisition = objRequisitionDS.Tables[0];
                DataTable objRejectionCommentDT = objRequisitionDS.Tables[1];

                if (objRequisition != null)
                {
                    UserExecutionContext userExecutionContext = UserContext;

                    List<MailAdressInfo> toMails = new List<MailAdressInfo>();
                    Contact objRequester = proxyPartnerService.GetContactByContactCode(0, (long)objRequisition.Rows[0][SqlConstants.COL_REQUESTER_ID]);
                    long ONBEHALFOF = (long)objRequisition.Rows[0][SqlConstants.COL_ONBEHALFOF];

                    toMails.Add(new MailAdressInfo() { ContactCode = ONBEHALFOF, EmailAddress = objRequisition.Rows[0][SqlConstants.COL_ONBEHALFOFEMAIL].ToString() });
                    ccMails.Add(objRequester.ContactCode, objRequester.EmailAddress);

                    string RejectionComment = "";
                    if (objRejectionCommentDT != null && objRejectionCommentDT.Rows.Count > 0)
                    {
                        RejectionComment = Convert.ToString(objRejectionCommentDT.Rows[objRejectionCommentDT.Rows.Count - 1]["Comment"]);
                    }
                    var requisitionUrl = "";

                    #region Smart2.0

                    requisitionUrl = GetQueryStringUrlForApprover(Convert.ToInt64(objRequisition.Rows[0][SqlConstants.COL_REQUISITION_ID].ToString()), userExecutionContext, DocumentType.Requisition, (long)objRequisition.Rows[0][SqlConstants.COL_ONBEHALFOF]);

                    #endregion Smart2.0

                    fieldValues = new SortedList<string, string>
                        {
                            {"[RequisitionName]", objRequisition.Rows[0][SqlConstants.COL_REQUISITION_NAME].ToString()},
                            {"[RequisitionNumber]", objRequisition.Rows[0][SqlConstants.COL_REQUISITION_NUMBER].ToString()},
                            {"[RequisitionAmount]", this.objUtils.FormatNumber(Convert.ToDecimal(objRequisition.Rows[0][SqlConstants.COL_REQUISITION_AMOUNT].ToString()), this.cultureInfo)},
                            {"[ReviewerName]",  rejector.ActionerType == 4 ? rejector.ProxyReviewerName : rejector.Name},
                            {"[UserName]",ONBEHALFOF != objRequester.ContactCode ? objRequisition.Rows[0][SqlConstants.COL_ONBEHALFOFNAME].ToString() : (objRequester.FirstName+" "+objRequester.LastName).Trim()},
                            {"[Currency]", GetCurrencySymbol(objRequisition.Rows[0][SqlConstants.COL_CURRENCY].ToString())},
                            {"[ProdLink]",requisitionUrl},
                            {"[BuyerCompanyName]",Convert.ToString(objRequisition.Rows[0][SqlConstants.COL_BUYER_COMPANY_NAME],CultureInfo.CurrentCulture)},
                            {"[BuyerCompanyLogo]", MultiRegionConfig.GetConfig(CloudConfig.BlobURL) + "cumuluscontent/logo/" + UserContext.BuyerPartnerCode + "_logo.jpg"},
                            {"[Urgent]", Convert.ToBoolean(objRequisition.Rows[0][SqlConstants.COL_ISURGENT]) == true ? "Urgent " : string.Empty},
                            {"[Custom Attributes]", customAttributeHtml.ToString() },
                            {"[RejectedComments]",RejectionComment}
                        };

                    attachments = new SortedList<long, string>();
                    List<User> UsersCollection = null;
                    var partnerHelper = new RESTAPIHelper.PartnerHelper(UserContext, JWTToken);
                    UsersCollection = partnerHelper.GetUserDetailsByContactCodes(ONBEHALFOF.ToString(), false);

                    User currentUser = new User();

                    if (UsersCollection == null || UsersCollection.Count == 0)
                    {
                        currentUser = new User() { CultureCode = UserContext.Culture };
                    }
                    else
                    {
                        currentUser = UsersCollection[0];
                    }

                    currentUser.CultureCode = string.IsNullOrEmpty(currentUser.CultureCode) ? this.UserContext.Culture : currentUser.CultureCode;

                    result = SendNotification("P2P230", toMails, ccMails, fieldValues, attachments, UserContext.BuyerPartnerCode, requisitionId, DocumentType.Requisition, currentUser.CultureCode);

                    string contactCodes = null;

                    if (prevReviewers != null && prevReviewers.Any())
                    {
                        contactCodes = string.Join(",", prevReviewers.Select(p => p.ReviewerId));
                    }

                    if (!string.IsNullOrEmpty(contactCodes))
                    {
                        partnerHelper = new RESTAPIHelper.PartnerHelper(UserContext, JWTToken);
                        UsersCollection = partnerHelper.GetUserDetailsByContactCodes(contactCodes, false);
                    }
                    // clearing ccmails because to avoid duplicate rejection mails to requester
                    ccMails.Clear();
                    foreach (ReviewerDetails prevReviewer in prevReviewers)
                    {
                        Contact objReviewer = new Contact();

                        if (((long)objRequisition.Rows[0][SqlConstants.COL_REQUESTER_ID] != prevReviewer.ReviewerId && prevReviewer.ActionerType != 4) ||
                            ((long)objRequisition.Rows[0][SqlConstants.COL_REQUESTER_ID] != prevReviewer.ProxyReviewerId && prevReviewer.ActionerType == 4))
                        {
                            toMails.Clear();
                            if (prevReviewer.ActionerType == 4)
                                objReviewer = proxyPartnerService.GetContactByContactCode(0, prevReviewer.ProxyReviewerId);
                            else
                                objReviewer = proxyPartnerService.GetContactByContactCode(0, prevReviewer.ReviewerId);
                            toMails.Add(new MailAdressInfo() { ContactCode = prevReviewer.ReviewerId, EmailAddress = objReviewer.EmailAddress });

                            #region Smart2.0
                            requisitionUrl = GetQueryStringUrlForApprover(Convert.ToInt64(objRequisition.Rows[0][SqlConstants.COL_REQUISITION_ID].ToString()), userExecutionContext, DocumentType.Requisition, prevReviewer.ReviewerId);

                            #endregion Smart2.0 

                            fieldValues = new SortedList<string, string>
                        {
                            {"[RequisitionName]", objRequisition.Rows[0][SqlConstants.COL_REQUISITION_NAME].ToString()},
                            {"[RequisitionNumber]", objRequisition.Rows[0][SqlConstants.COL_REQUISITION_NUMBER].ToString()},
                            {"[RequisitionAmount]", this.objUtils.FormatNumber(Convert.ToDecimal(objRequisition.Rows[0][SqlConstants.COL_REQUISITION_AMOUNT].ToString()), this.cultureInfo)},
                            {"[ReviewerName]",  rejector.ActionerType == 4 ? rejector.ProxyReviewerName : rejector.Name },
                            {"[UserName]", (objReviewer.FirstName+" "+objReviewer.LastName).Trim()},
                            {"[Currency]", GetCurrencySymbol(objRequisition.Rows[0][SqlConstants.COL_CURRENCY].ToString())},
                            {"[ProdLink]",requisitionUrl},
                            {"[BuyerCompanyName]",Convert.ToString(objRequisition.Rows[0][SqlConstants.COL_BUYER_COMPANY_NAME],CultureInfo.CurrentCulture)},
                            {"[BuyerCompanyLogo]", MultiRegionConfig.GetConfig(CloudConfig.BlobURL) + "cumuluscontent/logo/" + UserContext.BuyerPartnerCode + "_logo.jpg"},
                            {"[Urgent]", Convert.ToBoolean(objRequisition.Rows[0][SqlConstants.COL_ISURGENT]) == true ? "Urgent " : string.Empty},
                            {"[Custom Attributes]", customAttributeHtml.ToString() },
                            {"[RejectedComments]",RejectionComment}
                        };

                            attachments = new SortedList<long, string>();

                            User reviewerDetails = null;

                            if (UsersCollection != null && UsersCollection.Any())
                            {
                                reviewerDetails = UsersCollection.FirstOrDefault(u => u.ContactCode == prevReviewer.ReviewerId);
                            }
                            if (reviewerDetails == null || string.IsNullOrEmpty(currentUser.CultureCode))
                            {
                                reviewerDetails = new User() { CultureCode = UserContext.Culture };
                            }

                            result = SendNotification("P2P230", toMails, ccMails, fieldValues, attachments, UserContext.BuyerPartnerCode, requisitionId, DocumentType.Requisition, reviewerDetails.CultureCode);
                        }
                    }
                }
            }
            catch (CommunicationException commFaultEx)
            {
                LogHelper.LogError(Log, "Fault exception occured in EmailNotificationManager SendNotificationForReviewRejectedRequisition Method for requisitionId=" + requisitionId, commFaultEx);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in EmailNotificationManager SendNotificationForReviewRejectedRequisition Method for requisitionId=" + requisitionId, ex);
                throw;
            }

            return result;
        }
        public bool SendNotificationForReviewAcceptedRequisition(long requisitionId, ReviewerDetails acceptor, string queryString)
        {
            LogHelper.LogInfo(Log, "EmailNotificationManager SendNotificationForReviewAcceptedRequisition Method Started for requisitionId=" + requisitionId);

            bool result = false;
            ProxyPartnerService proxyPartnerService = new ProxyPartnerService(UserContext, this.JWTToken);
            SortedList<long, string> ccMails = new SortedList<long, string>();
            SortedList<long, string> attachments = new SortedList<long, string>();
            SortedList<string, string> fieldValues = new SortedList<string, string>();
            long Commenttype = 2;

            try
            {
                var commonManager = new RequisitionCommonManager(this.JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
                string questionIDs = commonManager.GetSettingsValueByKey(P2PDocumentType.Requisition, "IncludeCustomAttributesInNotification", UserContext.UserId);
                DataSet objRequisitionDS = GetCommonDao().GetBasicDetailsByIdForNotification(requisitionId, UserContext.BuyerPartnerCode, Commenttype, questionIDs);
                StringBuilder customAttributeHtml = new StringBuilder();
                if (objRequisitionDS.Tables.Count > 2 && questionIDs != string.Empty)
                {
                    DataTable dtReqCustomAttributes = objRequisitionDS.Tables[2];
                    customAttributeHtml = CustomAttributeBuilder(dtReqCustomAttributes);
                }


                DataTable objRequisition = objRequisitionDS.Tables[0];
                DataTable objRejectionCommentDT = objRequisitionDS.Tables[1];

                if (objRequisition != null)
                {
                    UserExecutionContext userExecutionContext = UserContext;

                    List<MailAdressInfo> toMails = new List<MailAdressInfo>();
                    Contact objRequester = proxyPartnerService.GetContactByContactCode(0, (long)objRequisition.Rows[0][SqlConstants.COL_REQUESTER_ID]);
                    long ONBEHALFOF = (long)objRequisition.Rows[0][SqlConstants.COL_ONBEHALFOF];

                    toMails.Add(new MailAdressInfo() { ContactCode = ONBEHALFOF, EmailAddress = objRequisition.Rows[0][SqlConstants.COL_ONBEHALFOFEMAIL].ToString() });
                    ccMails.Add(objRequester.ContactCode, objRequester.EmailAddress);

                    var requisitionUrl = "";

                    #region Smart2.0

                    requisitionUrl = GetQueryStringUrlForApprover(Convert.ToInt64(objRequisition.Rows[0][SqlConstants.COL_REQUISITION_ID].ToString()), userExecutionContext, DocumentType.Requisition, (long)objRequisition.Rows[0][SqlConstants.COL_ONBEHALFOF]);

                    #endregion Smart2.0

                    fieldValues = new SortedList<string, string>
                        {
                            {"[RequisitionName]", objRequisition.Rows[0][SqlConstants.COL_REQUISITION_NAME].ToString()},
                            {"[RequisitionNumber]", objRequisition.Rows[0][SqlConstants.COL_REQUISITION_NUMBER].ToString()},
                            {"[RequisitionAmount]", this.objUtils.FormatNumber(Convert.ToDecimal(objRequisition.Rows[0][SqlConstants.COL_REQUISITION_AMOUNT].ToString()), this.cultureInfo)},
                            {"[ReviewerName]",  acceptor.ActionerType == 4 ? acceptor.ProxyReviewerName : acceptor.Name},
                            {"[UserName]", ONBEHALFOF != objRequester.ContactCode ? objRequisition.Rows[0][SqlConstants.COL_ONBEHALFOFNAME].ToString() : (objRequester.FirstName+" "+objRequester.LastName).Trim()},
                            {"[Currency]", GetCurrencySymbol(objRequisition.Rows[0][SqlConstants.COL_CURRENCY].ToString())},
                            {"[ProdLink]",requisitionUrl},
                            {"[BuyerCompanyName]",Convert.ToString(objRequisition.Rows[0][SqlConstants.COL_BUYER_COMPANY_NAME],CultureInfo.CurrentCulture)},
                            {"[BuyerCompanyLogo]", MultiRegionConfig.GetConfig(CloudConfig.BlobURL) + "cumuluscontent/logo/" + UserContext.BuyerPartnerCode + "_logo.jpg"},
                            {"[Urgent]", Convert.ToBoolean(objRequisition.Rows[0][SqlConstants.COL_ISURGENT]) == true ? "Urgent " : string.Empty},
                            {"[Custom Attributes]", customAttributeHtml.ToString() }
                        };

                    attachments = new SortedList<long, string>();
                    List<User> UsersCollection = null;
                    var partnerHelper = new RESTAPIHelper.PartnerHelper(UserContext, JWTToken);
                    UsersCollection = partnerHelper.GetUserDetailsByContactCodes(ONBEHALFOF.ToString(), false);

                    User currentUser = new User();

                    if (UsersCollection == null || UsersCollection.Count == 0)
                    {
                        currentUser = new User() { CultureCode = UserContext.Culture };
                    }
                    else
                    {
                        currentUser = UsersCollection[0];
                    }

                    currentUser.CultureCode = string.IsNullOrEmpty(currentUser.CultureCode) ? this.UserContext.Culture : currentUser.CultureCode;

                    result = SendNotification("P2P229", toMails, ccMails, fieldValues, attachments, UserContext.BuyerPartnerCode, requisitionId, DocumentType.Requisition, currentUser.CultureCode);
                }
            }
            catch (CommunicationException commFaultEx)
            {
                LogHelper.LogError(Log, "Fault exception occured in EmailNotificationManager SendNotificationForReviewAcceptedRequisition Method for requisitionId=" + requisitionId, commFaultEx);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in EmailNotificationManager SendNotificationForReviewAcceptedRequisition Method for requisitionId=" + requisitionId, ex);
                throw;
            }

            return result;
        }

        public bool SendNotificationForBuyerAssignee(long requisitionId, long BuyerAssigneeValue, long PrevOrderContact)
        {
            bool result = false;
            List<MailAdressInfo> toMails = new List<MailAdressInfo>();
            SortedList<long, string> ccMails = new SortedList<long, string>();
            SortedList<long, string> attachments = null;
            SortedList<string, string> fieldValues = null;
            ProxyPartnerService proxyPartnerService = new ProxyPartnerService(UserContext, this.JWTToken);
            try
            {
                LogHelper.LogInfo(Log, "EmailNotificationManager SendNotificationForBuyerAssignee Method Started for requisitionId=" + requisitionId);

                var commonManager = new RequisitionCommonManager(this.JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
                string questionIDs = commonManager.GetSettingsValueByKey(P2PDocumentType.Requisition, "IncludeCustomAttributesInNotification", UserContext.UserId);
                DataSet objRequisitionDS = GetCommonDao().GetBasicDetailsByIdForNotification(requisitionId, UserContext.BuyerPartnerCode, 1, questionIDs);
                StringBuilder customAttributeHtml = new StringBuilder();
                if (objRequisitionDS.Tables.Count > 2 && questionIDs != string.Empty)
                {
                    DataTable dtReqCustomAttributes = objRequisitionDS.Tables[2];
                    customAttributeHtml = CustomAttributeBuilder(dtReqCustomAttributes);
                }

                DataTable objRequisition = objRequisitionDS.Tables[0];

                if (objRequisition != null && objRequisition.Rows.Count > 0)
                {
                    //
                    string eventCode = string.Empty;

                    //Requisition Creator
                    var creator = (string)objRequisition.Rows[0][SqlConstants.COL_REQUESTER_NAME];

                    //get requestor information
                    var OBOContactCode = (long)objRequisition.Rows[0][SqlConstants.COL_ONBEHALFOF];
                    var OBOContactName = (string)objRequisition.Rows[0][SqlConstants.COL_ONBEHALFOFNAME];
                    var OBOContactEmail = (string)objRequisition.Rows[0][SqlConstants.COL_ONBEHALFOFEMAIL];

                    //Order contact details. order contact details can be used for assign and reassign order contact use case
                    var orderContactName = (string)objRequisition.Rows[0][SqlConstants.COL_ORDER_CONTACT_NAME];
                    var ordercontactEmail = (string)objRequisition.Rows[0][SqlConstants.COL_ORDER_CONTACT_EMAIL];
                    var orderContactCode = (long)objRequisition.Rows[0][SqlConstants.COL_ORDER_CONTACTCODE];

                    //Build to address for both Assign order contact and Reassign Order contact.
                    toMails.Add(new MailAdressInfo { EmailAddress = ordercontactEmail, ContactCode = orderContactCode });

                    //Build CC address for both Assign order contact and Reassign Order contact.
                    if (PrevOrderContact > 0 && BuyerAssigneeValue != PrevOrderContact) // Reassign order contact use case
                    {
                        eventCode = "P2P226";
                        var objprevOrderContact = proxyPartnerService.GetContactByContactCode(0, PrevOrderContact);
                        // Added Null Check
                        if (objprevOrderContact != null && objprevOrderContact.ContactCode == OBOContactCode)
                        {
                            //  Requestor
                            ccMails.Add(OBOContactCode, OBOContactEmail);
                        }
                        else
                        {
                            //  Requestor 
                            ccMails.Add(OBOContactCode, OBOContactEmail);
                            if (objprevOrderContact != null)// Added Null Check
                            {
                                // Previous Order  Contact
                                ccMails.Add(objprevOrderContact.ContactCode, objprevOrderContact.EmailAddress);
                            }
                        }
                    }
                    else
                    { // Assign Order contact use case
                        ccMails.Add(OBOContactCode, OBOContactEmail);
                        eventCode = "P2P225";
                    }

                    #region Smart2.0

                    string requisitionUrl = GetQueryStringUrlForApprover(Convert.ToInt64(objRequisition.Rows[0][SqlConstants.COL_REQUISITION_ID].ToString()), UserContext, DocumentType.Requisition, (long)objRequisition.Rows[0][SqlConstants.COL_ONBEHALFOF]);

                    #endregion Smart2.0

                    fieldValues = new SortedList<string, string>
                        {
                            {"[RequisitionName]", objRequisition.Rows[0][SqlConstants.COL_REQUISITION_NAME].ToString()},
                            {"[RequisitionNumber]", objRequisition.Rows[0][SqlConstants.COL_REQUISITION_NUMBER].ToString()},
                            {"[RequisitionAmount]", this.objUtils.FormatNumber(Convert.ToDecimal(objRequisition.Rows[0][SqlConstants.COL_REQUISITION_AMOUNT].ToString()), this.cultureInfo)},
                            {"[UserName]",orderContactName},
                            {"[Currency]", GetCurrencySymbol(objRequisition.Rows[0][SqlConstants.COL_CURRENCY].ToString())},
                            {"[CurrencyCode]", Convert.ToString(objRequisition.Rows[0][SqlConstants.COL_CURRENCY].ToString())},
                            {"[ProdLink]",requisitionUrl},
                            {"[CancelLink]", ""},
                            {"[BuyerCompanyName]",Convert.ToString(objRequisition.Rows[0][SqlConstants.COL_BUYER_COMPANY_NAME],CultureInfo.CurrentCulture)},
                            {"[BuyerCompanyLogo]", MultiRegionConfig.GetConfig(CloudConfig.BlobURL) + "cumuluscontent/logo/" + UserContext.BuyerPartnerCode + "_logo.jpg"},
                            {"[Urgent]", Convert.ToBoolean(objRequisition.Rows[0][SqlConstants.COL_ISURGENT]) == true ? "Urgent " : string.Empty},
                            {"[a requisition]", Convert.ToBoolean(objRequisition.Rows[0][SqlConstants.COL_ISURGENT]) == true ? string.Empty : " a requisition "},
                            {"[an urgent requisition]", Convert.ToBoolean(objRequisition.Rows[0][SqlConstants.COL_ISURGENT]) == true ? " an urgent requisition ": string.Empty},
                           {"[Requester]", creator},
                            {"[Author]", creator},
                            {SqlConstants.NOTN_SupplierCompanyName,objRequisition.Rows[0][SqlConstants.COL_LEGALCOMPANYNAME].ToString()},
                            {"[OnBehalfOf]", OBOContactName },
                            {"[ItemTotal]", this.objUtils.FormatNumber(Convert.ToDecimal(objRequisition.Rows[0][SqlConstants.COL_ITEMTOTAL].ToString()), this.cultureInfo)},
                            {"[Tax]", this.objUtils.FormatNumber(Convert.ToDecimal(objRequisition.Rows[0][SqlConstants.COL_TAX].ToString()), this.cultureInfo)},
                            {"[Custom Attributes]", customAttributeHtml.ToString() }

                        };

                    attachments = new SortedList<long, string>();
                    result = SendNotification(eventCode, toMails, ccMails, fieldValues, attachments, UserContext.BuyerPartnerCode, requisitionId, DocumentType.Requisition);
                }
                else
                {
                    LogHelper.LogInfo(Log, "EmailNotificationManager SendNotificationForBuyerAssignee notification not triggered for requisitionId=" + requisitionId);

                }
            }
            catch (CommunicationException commFaultEx)
            {
                LogHelper.LogError(Log, "Fault exception occured in EmailNotificationManager SendNotificationForBuyerAssignee Method for requisitionId=" + requisitionId, commFaultEx);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in EmailNotificationManager SendNotificationForBuyerAssignee Method for requisitionId=" + requisitionId, ex);
                throw;
            }


            return result;
        }

        public void SendNotificationForLineStatusUpdate(RequisitionLineStatusUpdateDetails reqDetails)
        {
            try
            {
                if (reqDetails != null)
                {
                    LogHelper.LogInfo(Log, "EmailNotificationManager SendNotificationForLineStatusUpdate Method Started for documentCode =" + reqDetails.RequisitionId);
                    UserExecutionContext userExecutionContext = UserContext;
                    ProxyPartnerService proxyPartnerService = new ProxyPartnerService(UserContext, this.JWTToken);
                    SortedList<string, string> fieldValues = new SortedList<string, string>();
                    SortedList<long, string> ccMails = new SortedList<long, string>();
                    SortedList<long, string> attachments = new SortedList<long, string>();

                    List<MailAdressInfo> toMails = new List<MailAdressInfo>();

                    //ProxyRequisitionService proxyReqService = new ProxyRequisitionService(UserContext);
                    RequisitionInterfaceManager reqInterfaceManger = new RequisitionInterfaceManager(this.JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };

                    List<RequisitionItem> reqItems = reqInterfaceManger.GetLineItemBasicDetailsForInterface(reqDetails.RequisitionId);

                    //Building Custom Attributes for the notification
                    var commonManager = new RequisitionCommonManager(this.JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
                    string questionIDs = commonManager.GetSettingsValueByKey(P2PDocumentType.Requisition, "IncludeCustomAttributesInNotification", UserContext.UserId);
                    DataSet objRequisitionDS = GetCommonDao().GetBasicDetailsByIdForNotification(reqDetails.RequisitionId, UserContext.BuyerPartnerCode, 1, questionIDs);
                    StringBuilder customAttributeHtml = new StringBuilder();
                    if (objRequisitionDS.Tables.Count > 2 && questionIDs != string.Empty)
                    {
                        DataTable dtReqCustomAttributes = objRequisitionDS.Tables[2];
                        customAttributeHtml = CustomAttributeBuilder(dtReqCustomAttributes);
                    }



                    if (reqDetails.RequesterContactCode > 0)
                    {
                        toMails.Add(new MailAdressInfo() { ContactCode = reqDetails.RequesterContactCode, EmailAddress = reqDetails.RequesterEmailAddress });

                        if (reqDetails.IsUpdateAllItems)
                        {
                            foreach (var item in reqItems.Where(i => i.InventoryType == true))
                            {
                                fieldValues = new SortedList<string, string>
                                    {
                                        {"[ContactName]", reqDetails.RequesterFirstName + " " + reqDetails.RequesterLastName},
                                        {"[LineNumber]", item.ItemLineNumber.ToString()},
                                        {"[RequistionNumber]", reqDetails.RequisitionNumber},
                                        {"[ReservationStatus]", reqDetails.AllLineStatus.ToString()},
                                        {"[Custom Attributes]", customAttributeHtml.ToString() }
                                    };

                                SendNotification("BZ123", toMails, ccMails, fieldValues, attachments, UserContext.BuyerPartnerCode, reqDetails.RequisitionId, DocumentType.Requisition);
                            }
                        }
                        else
                        {
                            foreach (var item in reqDetails.Items)
                            {
                                fieldValues = new SortedList<string, string>
                                    {
                                        {"[ContactName]", reqDetails.RequesterFirstName + " " + reqDetails.RequesterLastName},
                                        {"[LineNumber]", item.LineNumber.ToString()},
                                        {"[RequistionNumber]", reqDetails.RequisitionNumber},
                                        {"[ReservationStatus]", item.LineStatus.ToString()},
                                        {"[Custom Attributes]", customAttributeHtml.ToString() }
                                    };

                                SendNotification("BZ123", toMails, ccMails, fieldValues, attachments, UserContext.BuyerPartnerCode, reqDetails.RequisitionId, DocumentType.Requisition);
                            }
                        }
                    }


                    if (reqDetails.OBUContactCode > 0)
                    {
                        toMails.Clear();
                        toMails.Add(new MailAdressInfo() { ContactCode = reqDetails.OBUContactCode, EmailAddress = reqDetails.OBUEmailAddress });

                        if (reqDetails.IsUpdateAllItems)
                        {
                            foreach (var item in reqItems.Where(i => i.InventoryType == true))
                            {
                                fieldValues = new SortedList<string, string>
                                    {
                                        {"[ContactName]", reqDetails.OBUFirstName + " " + reqDetails.OBULastName},
                                        {"[LineNumber]", item.ItemLineNumber.ToString()},
                                        {"[RequistionNumber]", reqDetails.RequisitionNumber},
                                        {"[ReservationStatus]", reqDetails.AllLineStatus.ToString()},
                                        {"[Custom Attributes]", customAttributeHtml.ToString() }
                                    };

                                SendNotification("BZ123", toMails, ccMails, fieldValues, attachments, UserContext.BuyerPartnerCode, reqDetails.RequisitionId, DocumentType.Requisition);
                            }
                        }
                        else
                        {
                            foreach (var item in reqDetails.Items)
                            {
                                fieldValues = new SortedList<string, string>
                                    {
                                        {"[ContactName]", reqDetails.OBUFirstName + " " + reqDetails.OBULastName},
                                        {"[LineNumber]", item.LineNumber.ToString()},
                                        {"[RequistionNumber]", reqDetails.RequisitionNumber},
                                        {"[ReservationStatus]", item.LineStatus.ToString()},
                                        {"[Custom Attributes]", customAttributeHtml.ToString() }
                                    };

                                SendNotification("BZ123", toMails, ccMails, fieldValues, attachments, UserContext.BuyerPartnerCode, reqDetails.RequisitionId, DocumentType.Requisition);
                            }
                        }
                    }

                }
            }
            catch (CommunicationException commFaultEx)
            {
                LogHelper.LogError(Log, "Fault exception occured in EmailNotificationManager SendNotificationForRequisitionReview Method for documentCode =" + reqDetails.RequisitionId, commFaultEx);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in EmailNotificationManager SendNotificationForRequisitionReview Method for documentCode =" + reqDetails.RequisitionId, ex);
                throw;
            }

        }

        public bool SendFailureNotificaiton(long documentCode, DocumentType documentType, FailureAction failureAction)
        {
            bool result = false;
            try
            {
                LogHelper.LogInfo(Log, string.Concat("EmailNotificationManager SendFailureNotificaiton Method Started for documentCode=", documentCode));
                ProxyPartnerService proxyPartnerService = new ProxyPartnerService(UserContext, this.JWTToken);

                Document objDocument = GetDocumentDao().GetDocumentBasicDetails(documentCode);
                if (objDocument != null)
                {
                    var userDetails = proxyPartnerService.GetContactByContactCode(this.UserContext.BuyerPartnerCode, this.UserContext.ContactCode);
                    List<MailAdressInfo> toMails = new List<MailAdressInfo>();
                    SortedList<long, string> ccMails = new SortedList<long, string>();
                    SortedList<long, string> attachments = new SortedList<long, string>();
                    SortedList<string, string> fieldValues = null;
                    var prodUrl = "";
                    switch (documentType)
                    {
                        case DocumentType.Requisition:
                            prodUrl = MultiRegionConfig.GetConfig(CloudConfig.RequisitionURL) + "?dd=" +
              RequisitionDocumentManager.GetQueryString(documentCode, UserContext.BuyerPartnerCode) + "&b=1";
                            break;

                        case DocumentType.PO:
                            prodUrl = MultiRegionConfig.GetConfig(CloudConfig.OrderURL) + "?dd=" +
              RequisitionDocumentManager.GetQueryString(documentCode, UserContext.BuyerPartnerCode) + "&b=1";
                            break;

                        case DocumentType.Invoice:
                            prodUrl = MultiRegionConfig.GetConfig(CloudConfig.InvoiceURL) +
              RequisitionDocumentManager.GetQueryString(documentCode, UserContext.BuyerPartnerCode) + "&b=1";
                            break;

                        case DocumentType.InvoiceReconcillation:
                            prodUrl = MultiRegionConfig.GetConfig(CloudConfig.IRURL) +
              RequisitionDocumentManager.GetQueryString(documentCode, UserContext.BuyerPartnerCode) + "&b=1";
                            break;
                    }
                    fieldValues = new SortedList<string, string>
                            {
                                {"[DocumentType]", documentType.GetStringValue()},
                                {"[DocumentName]", objDocument.DocumentName},
                                {"[DocumentNo]", objDocument.DocumentNumber},
                                {"[ActionPerformed]",failureAction.GetStringValue()},
                                {"[UserName]", string.Concat(userDetails.FirstName," ",userDetails.LastName).Trim()},
                                {"[ProdLink]",prodUrl},
                                {"[BuyerCompanyName]", userDetails.LegalCompanyName},
                                {"[BuyerCompanyLogo]", MultiRegionConfig.GetConfig(CloudConfig.BlobURL) + "cumuluscontent/logo/" + UserContext.BuyerPartnerCode + "_logo.jpg"}
                            };

                    toMails = new List<MailAdressInfo>();
                    toMails.Add(new MailAdressInfo() { ContactCode = userDetails.ContactCode, EmailAddress = userDetails.EmailAddress });

                    User buyerUser = null;
                    var partnerHelper = new RESTAPIHelper.PartnerHelper(UserContext, JWTToken);
                    buyerUser = partnerHelper.GetUserDetailsByContactCodes(userDetails.ContactCode.ToString(), false).FirstOrDefault();

                    if (buyerUser == null || string.IsNullOrEmpty(buyerUser.CultureCode))
                    {
                        buyerUser = new User() { CultureCode = UserContext.Culture };
                    }
                    result = SendNotification("P2P170", toMails, ccMails, fieldValues, attachments, UserContext.BuyerPartnerCode, 0, documentType, buyerUser.CultureCode);
                }
            }
            catch (CommunicationException commFaultEx)
            {
                LogHelper.LogError(Log, string.Concat("Fault exception occured in EmailNotificationManager SendFailureNotificaiton Method for documentCode=", documentCode), commFaultEx);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, string.Concat("Error occured in EmailNotificationManager SendFailureNotificaiton Method for documentCode=", documentCode), ex);
                throw;
            }
            return result;
        }

        public StringBuilder CustomAttributeBuilder(DataTable objDTRequisitionCustomAttribute)
        {
            StringBuilder sbCustomAttribute = new StringBuilder();
            if (objDTRequisitionCustomAttribute.Rows.Count > 0)
            {
                sbCustomAttribute = sbCustomAttribute.Append("<div><table width=\"100%\" border=\"1\" cellpadding=\"5\" cellspacing=\"0\"><tbody><tr><td colspan = \"2\"><b>Additional Information</b></td></tr>");
                foreach (DataRow dataRow in objDTRequisitionCustomAttribute.Rows)
                {
                    string questionName = Convert.ToString(dataRow["QuestionText"]);
                    string answer = Convert.ToString(dataRow["ResponseValue"]);
                    sbCustomAttribute.Append("<tr><td width=\"40%\"><b>" + questionName + "</b></td><td width=\"60%\" style=\"nowrap:false\">" + answer + "</td></tr>");
                }
                sbCustomAttribute.Append("</tbody></table></div>");

            }

            return sbCustomAttribute;
        }
        public bool SendNotificationToTeamMembers(GEP.Cumulus.Documents.Entities.Document objDocument, List<DocumentStakeHolder> newTeamMembers)
        {


            long requisitionId = objDocument.DocumentCode;
            bool result = false;
            SortedList<long, string> ccMails = new SortedList<long, string>();
            SortedList<long, string> attachments = null;
            SortedList<string, string> fieldValues = null;


            try
            {


                LogHelper.LogError(Log, "EmailNotificationManager SendNotificationToTeamMembers Method Started for requisitionId=" + requisitionId, new Exception());

                var commonManager = new RequisitionCommonManager(this.JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
                string questionIDs = commonManager.GetSettingsValueByKey(P2PDocumentType.Requisition, "IncludeCustomAttributesInNotification", UserContext.UserId);
                DataSet objRequisitionDS = GetCommonDao().GetBasicDetailsByIdForNotification(requisitionId, UserContext.BuyerPartnerCode, 1, questionIDs);

                DataTable objRequisition = objRequisitionDS.Tables[0];

                if (objRequisition != null && objRequisition.Rows.Count > 0)
                {

                    string eventCode = string.Empty;
                    eventCode = "P2P242";

                    //Requisition Creator
                    var creator = (string)objRequisition.Rows[0][SqlConstants.COL_REQUESTER_NAME];


                    string requisitionUrl = GetQueryStringUrlForApprover(Convert.ToInt64(objRequisition.Rows[0][SqlConstants.COL_REQUISITION_ID].ToString()), UserContext, DocumentType.Requisition, (long)objRequisition.Rows[0][SqlConstants.COL_ONBEHALFOF]);
                    attachments = new SortedList<long, string>();

                    fieldValues = new SortedList<string, string>()
                        {
                            {"[RequisitionName]", objRequisition.Rows[0][SqlConstants.COL_REQUISITION_NAME].ToString()},
                            {"[RequisitionNumber]", objRequisition.Rows[0][SqlConstants.COL_REQUISITION_NUMBER].ToString()},
                            {"[URL]",requisitionUrl},
                            {"[BuyerCompanyName]",Convert.ToString(objRequisition.Rows[0][SqlConstants.COL_BUYER_COMPANY_NAME],CultureInfo.CurrentCulture)},
                            {"[Requester]", creator},
                            {"[BuyerCompanyLogo]", MultiRegionConfig.GetConfig(CloudConfig.BlobURL) + "cumuluscontent/logo/" + UserContext.BuyerPartnerCode + "_logo.jpg"}
                        };
                    foreach (DocumentStakeHolder documentstakeholder in newTeamMembers)
                    {
                        List<MailAdressInfo> toMails = new List<MailAdressInfo>();
                        toMails.Add(new MailAdressInfo { EmailAddress = documentstakeholder.EmailId, ContactCode = documentstakeholder.ContactCode });
                        if (toMails.Count > 0)
                        {
                            fieldValues.Add("[UserName]", (documentstakeholder.FirstName + " " + documentstakeholder.LastName).Trim());

                            result = SendNotification(eventCode, toMails, ccMails, fieldValues, attachments, UserContext.BuyerPartnerCode, requisitionId, DocumentType.Requisition);

                            fieldValues.Remove("[UserName]");
                        }
                    }

                }
                else
                {
                    LogHelper.LogError(Log, "EmailNotificationManager SendNotificationToTeamMembers notification not triggered for requisitionId=" + requisitionId, new Exception());

                }
            }
            catch (CommunicationException commFaultEx)
            {


                LogHelper.LogError(Log, "Fault exception occured in EmailNotificationManager SendNotificationToTeamMembers Method for requisitionId=" + requisitionId, commFaultEx);
            }
            catch (Exception ex)
            {

                LogHelper.LogError(Log, "Error occured in EmailNotificationManager SendNotificationToTeamMembers Method for requisitionId=" + requisitionId, ex);
                throw;
            }


            return result;
        }

        private void saveNewrelicErrors(string eventname, string key, string values)
        {
            var eventObject = new Dictionary<string, object>();
            try
            {

                eventObject.Add(key, values);
                NewRelic.Api.Agent.NewRelic.RecordCustomEvent(eventname, eventObject);
            }
            catch (Exception)
            {
                Dictionary<string, object> msg = new Dictionary<string, object>();
                NewRelic.Api.Agent.NewRelic.NoticeError("Error Occured on New Relic Writing ", msg);                
            }
        }

        public void SendNotificationForSkipApproval(long documentCode, List<ApproverDetails> lstSkippedApprovers, List<ApproverDetails> lstFinalApprovers)
        {
            SendSkipNotificationForSkippedApprovers(documentCode, lstSkippedApprovers, lstFinalApprovers);
            SendSkipNotificationForLastApprovers(documentCode, lstSkippedApprovers, lstFinalApprovers);
        }

        public bool SendSkipNotificationForSkippedApprovers(long documentCode, List<ApproverDetails> lstSkippedApprovers, List<ApproverDetails> lstFinalApprovers)
        {
            long requisitionId = documentCode;
            bool result = false;
            SortedList<long, string> ccMails = new SortedList<long, string>();
            SortedList<long, string> attachments = new SortedList<long, string>();
            SortedList<string, string> fieldValues = new SortedList<string, string>();
            ProxyPartnerService proxyPartnerService = new ProxyPartnerService(UserContext, this.JWTToken);
            try
            {
                LogHelper.LogError(Log, "EmailNotificationManager SendSkipNotificationForSkippedApprovers Method Started for requisitionId=" + requisitionId, new Exception());
                UserExecutionContext userExecutionContext = UserContext;
                var commonManager = new RequisitionCommonManager(this.JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
                string questionIDs = commonManager.GetSettingsValueByKey(P2PDocumentType.Requisition, "IncludeCustomAttributesInNotification", UserContext.UserId);
                DataSet objRequisitionDS = GetCommonDao().GetBasicDetailsByIdForNotification(requisitionId, UserContext.BuyerPartnerCode, 1, questionIDs);

                DataTable objDTRequisition = objRequisitionDS.Tables[0];

                if (objDTRequisition != null && objDTRequisition.Rows.Count > 0)
                {
                    string strApprovers = "";

                    if (lstFinalApprovers.Count() > 0)
                    {
                        foreach (var finalApprover in lstFinalApprovers)
                        {
                            strApprovers += finalApprover.Name + "<br/>";
                        }
                    }

                    foreach (var skipApprover in lstSkippedApprovers)
                    {
                        List<MailAdressInfo> toMails = new List<MailAdressInfo>();
                        List<DocumentProxyDetails> documentProxyDetailsLst = new List<DocumentProxyDetails>();
                        documentProxyDetailsLst = GetCommonDao().CheckOriginalApproverNotificationStatus();
                        var objApprover = proxyPartnerService.GetContactByContactCode(0, skipApprover.ApproverId);
                        if (skipApprover.ProxyApproverId > 0)
                        {
                            if (documentProxyDetailsLst.Where(e => e.ContactCode == skipApprover.ApproverId && e.DoctypeId == 7
                                && e.EnableOriginalApproverNotifications == true).Count() > 0)
                                toMails.Add(new MailAdressInfo() { ContactCode = skipApprover.ApproverId, EmailAddress = objApprover.EmailAddress });
                        }
                        else
                            toMails.Add(new MailAdressInfo() { ContactCode = skipApprover.ApproverId, EmailAddress = objApprover.EmailAddress });

                        string requsitionUrl = GetQueryStringUrlForApprover(Convert.ToInt64(objDTRequisition.Rows[0][SqlConstants.COL_REQUISITION_ID].ToString()), userExecutionContext, DocumentType.Requisition, skipApprover.ApproverId);

                        if (string.IsNullOrWhiteSpace(objApprover.ContactCultureCode))
                            objApprover.ContactCultureCode = this.UserContext.Culture;

                        if (toMails.Any())
                        {

                            fieldValues = generateFieldValues(userExecutionContext, objDTRequisition, strApprovers, objApprover, requsitionUrl);

                            if (string.IsNullOrWhiteSpace(objApprover.ContactCultureCode))
                                objApprover.ContactCultureCode = this.UserContext.Culture;

                            result = SendNotification("P2P243", toMails, ccMails, fieldValues, attachments, UserContext.BuyerPartnerCode, documentCode, DocumentType.Requisition, objApprover.ContactCultureCode);
                        }
                    }
                }
                else
                {
                    LogHelper.LogError(Log, "EmailNotificationManager SendSkipNotificationForSkippedApprovers notification not triggered for requisitionId=" + requisitionId, new Exception());
                }
            }
            catch (CommunicationException commFaultEx)
            {
                LogHelper.LogError(Log, "Fault exception occured in EmailNotificationManager SendSkipNotificationForSkippedApprovers Method for requisitionId=" + requisitionId, commFaultEx);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in EmailNotificationManager SendSkipNotificationForSkippedApprovers Method for requisitionId=" + requisitionId, ex);
                throw;
            }
            return result;
        }

        public bool SendSkipNotificationForLastApprovers(long documentCode, List<ApproverDetails> lstSkippedApprovers, List<ApproverDetails> lstFinalApprovers)
        {
            long requisitionId = documentCode;
            bool result = false;
            SortedList<long, string> ccMails = new SortedList<long, string>();
            SortedList<long, string> attachments = new SortedList<long, string>();
            SortedList<string, string> fieldValues = new SortedList<string, string>();
            ProxyPartnerService proxyPartnerService = new ProxyPartnerService(UserContext, this.JWTToken);
            try
            {
                LogHelper.LogError(Log, "EmailNotificationManager SendSkipNotificationForLastApprovers Method Started for requisitionId=" + requisitionId, new Exception());
                UserExecutionContext userExecutionContext = UserContext;
                var commonManager = new RequisitionCommonManager(this.JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
                string questionIDs = commonManager.GetSettingsValueByKey(P2PDocumentType.Requisition, "IncludeCustomAttributesInNotification", UserContext.UserId);
                DataSet objRequisitionDS = GetCommonDao().GetBasicDetailsByIdForNotification(requisitionId, UserContext.BuyerPartnerCode, 1, questionIDs);
                bool requireSecureMail = Convert.ToBoolean(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "SendSecureMailAction", UserContext.UserId));
                long lobId = GetCommonDao().GetLOBByDocumentCode(documentCode);
                DataTable objDTRequisition = objRequisitionDS.Tables[0];

                if (objDTRequisition != null && objDTRequisition.Rows.Count > 0)
                {
                    string strApprovers = "";

                    if (lstFinalApprovers.Count() > 0)
                    {
                        foreach (var finalApprover in lstFinalApprovers)
                        {
                            strApprovers += finalApprover.Name + "<br/>";
                        }
                    }
                    foreach (var finalApprover in lstFinalApprovers)
                    {
                        List<MailAdressInfo> toMails = new List<MailAdressInfo>();
                        List<DocumentProxyDetails> documentProxyDetailsLst = new List<DocumentProxyDetails>();
                        documentProxyDetailsLst = GetCommonDao().CheckOriginalApproverNotificationStatus();
                        var objApprover = proxyPartnerService.GetContactByContactCode(0, finalApprover.ApproverId);
                        if (finalApprover.ProxyApproverId > 0)
                        {
                            if (documentProxyDetailsLst.Where(e => e.ContactCode == finalApprover.ApproverId && e.DoctypeId == 7
                                && e.EnableOriginalApproverNotifications == true).Count() > 0)
                                toMails.Add(new MailAdressInfo() { ContactCode = finalApprover.ApproverId, EmailAddress = objApprover.EmailAddress });
                        }
                        else
                            toMails.Add(new MailAdressInfo() { ContactCode = finalApprover.ApproverId, EmailAddress = objApprover.EmailAddress });

                        string requsitionUrl = GetQueryStringUrlForApprover(Convert.ToInt64(objDTRequisition.Rows[0][SqlConstants.COL_REQUISITION_ID].ToString()), userExecutionContext, DocumentType.Requisition, finalApprover.ApproverId);

                        if (string.IsNullOrWhiteSpace(objApprover.ContactCultureCode))
                            objApprover.ContactCultureCode = this.UserContext.Culture;

                        if (toMails.Any())
                        {
                            var approveUrl = GetMailActionURL(requireSecureMail, commonManager) +
                                                                  "?documentType=" + DocumentType.Requisition +
                                                                  "&dtl=" + RequisitionDocumentManager.EncryptURL("evc=" + "P2P244" +
                                                                                                          "&bpc=" + UserContext.BuyerPartnerCode +
                                                                                                          "&dc=" + objDTRequisition.Rows[0][SqlConstants.COL_REQUISITION_ID] +
                                                                                                          "&cc=" + finalApprover.ApproverId +
                                                                                                          "&action=" + DocumentStatus.Approved +
                                                                                                          "&dt=" + DateTime.UtcNow +
                                                                                                          "&lob=" + lobId) +
                                                                  "&oloc=" + (int)SubAppCodes.P2P +
                                                                  "&b=1" +
                                                                  "&dd=" + RequisitionDocumentManager.EncryptURL("bpc=" + UserContext.BuyerPartnerCode +
                                                                                                         "&dc=" + objDTRequisition.Rows[0][SqlConstants.COL_REQUISITION_ID]);
                            var rejectUrl = GetMailActionURL(requireSecureMail, commonManager, "REJECT") +
                                                                            "?documentType=" + DocumentType.Requisition +
                                                                            "&dtl=" + RequisitionDocumentManager.EncryptURL("evc=" + "P2P244" +
                                                                                                                    "&bpc=" + UserContext.BuyerPartnerCode +
                                                                                                                    "&dc=" + objDTRequisition.Rows[0][SqlConstants.COL_REQUISITION_ID] +
                                                                                                                    "&cc=" + finalApprover.ApproverId +
                                                                                                                    "&action=" + DocumentStatus.Rejected +
                                                                                                                    "&dt=" + DateTime.UtcNow +
                                                                                                                    "&lob=" + lobId) +
                                                                            "&oloc=" + (int)SubAppCodes.P2P +
                                                                            "&b=1" +
                                                                            "&dd=" + RequisitionDocumentManager.EncryptURL("bpc=" + UserContext.BuyerPartnerCode +
                                                                                                                   "&dc=" + objDTRequisition.Rows[0][SqlConstants.COL_REQUISITION_ID]);

                            fieldValues = generateFieldValues(userExecutionContext, objDTRequisition, strApprovers, objApprover, requsitionUrl, approveUrl, rejectUrl);

                            if (string.IsNullOrWhiteSpace(objApprover.ContactCultureCode))
                                objApprover.ContactCultureCode = this.UserContext.Culture;

                            result = SendNotification("P2P244", toMails, ccMails, fieldValues, attachments, UserContext.BuyerPartnerCode, documentCode, DocumentType.Requisition, objApprover.ContactCultureCode);
                        }
                    }
                }
                else
                {
                    LogHelper.LogError(Log, "EmailNotificationManager SendSkipNotificationForLastApprovers notification not triggered for requisitionId=" + requisitionId, new Exception());
                }
            }
            catch (CommunicationException commFaultEx)
            {
                LogHelper.LogError(Log, "Fault exception occured in EmailNotificationManager SendSkipNotificationForLastApprovers Method for requisitionId=" + requisitionId, commFaultEx);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in EmailNotificationManager SendSkipNotificationForLastApprovers Method for requisitionId=" + requisitionId, ex);
                throw;
            }
            return result;
        }

        private SortedList<string, string> generateFieldValues(UserExecutionContext userExecutionContext, DataTable objDTRequisition, string strApprovers, Contact objApprover, string requsitionUrl, string approveUrl = null, string rejectUrl = null)
        {
            return new SortedList<string, string>
            {
                {"[Requester]", string.IsNullOrEmpty(objDTRequisition.Rows[0][SqlConstants.COL_ONBEHALFOFNAME].ToString()) ?
                                    objDTRequisition.Rows[0][SqlConstants.COL_REQUESTER_NAME].ToString():
                                    objDTRequisition.Rows[0][SqlConstants.COL_ONBEHALFOFNAME].ToString()},
                {"[RequisitionName]", objDTRequisition.Rows[0][SqlConstants.COL_REQUISITION_NAME].ToString()},
                {"[RequisitionNumber]", objDTRequisition.Rows[0][SqlConstants.COL_REQUISITION_NUMBER].ToString()},
                {"[UserName]",  ((string.IsNullOrEmpty(objApprover.FirstName) ? " " : objApprover.FirstName) + " " + (string.IsNullOrEmpty(objApprover.LastName) ? " " : objApprover.LastName)).Trim()},
                {"[AmountValue]", this.objUtils.FormatNumber(Convert.ToDecimal(objDTRequisition.Rows[0][SqlConstants.COL_REQUISITION_AMOUNT].ToString()),this.cultureInfo)},
                {"[HighestApproverName]", strApprovers},
                {"[URL]", requsitionUrl},
                {"[ApproveLink]",approveUrl},
                {"[RejectLink]",rejectUrl},
                {"[BuyerCompanyName]",objDTRequisition.Rows[0][SqlConstants.COL_BUYER_COMPANY_NAME].ToString()},
                {"[BuyerCompanyLogo]", MultiRegionConfig.GetConfig(CloudConfig.BlobURL) + "cumuluscontent/logo/" + userExecutionContext.BuyerPartnerCode + "_logo.jpg"},
                {"[Urgent]", Convert.ToBoolean(objDTRequisition.Rows[0][SqlConstants.COL_ISURGENT]) == true ? "Urgent " : string.Empty},
                {"[Author]", objDTRequisition.Rows[0][SqlConstants.COL_REQUESTER_NAME].ToString().Trim()},
                {"[OnBehalfOf]", objDTRequisition.Rows[0][SqlConstants.COL_ONBEHALFOFNAME].ToString().Trim()},
                {"[Currency]", GetCurrencySymbol(Convert.ToString(objDTRequisition.Rows[0][SqlConstants.COL_CURRENCY].ToString(), CultureInfo.CurrentCulture))},
                {"[Tax]", this.objUtils.FormatNumber(Convert.ToDecimal(objDTRequisition.Rows[0][SqlConstants.COL_TAX].ToString()),this.cultureInfo)},
                {"[RequisitionAmount]", this.objUtils.FormatNumber(Convert.ToDecimal(objDTRequisition.Rows[0][SqlConstants.COL_REQUISITION_AMOUNT].ToString()),this.cultureInfo)},
                {SqlConstants.NOTN_SupplierCompanyName,objDTRequisition.Rows[0][SqlConstants.COL_LEGALCOMPANYNAME].ToString()},
                {"[ItemTotal]",this.objUtils.FormatNumber(Convert.ToDecimal(objDTRequisition.Rows[0][SqlConstants.COL_ITEMTOTAL].ToString()),this.cultureInfo)}
            };
        }

        private void LogNewRelicApp(string message, long documentCode, string eventName = "RequisitionEmailNotificationManager")
        {
            var eventAttributes = new Dictionary<string, object>();            
            eventAttributes.Add("message", message);
            eventAttributes.Add("documentCode", documentCode.ToString());
            NewRelic.Api.Agent.NewRelic.RecordCustomEvent(eventName, eventAttributes);
        }
        public bool SendNotificationForCancelRequisition(long documentCode, DocumentStatus documentStatus)
        {
            bool result = false;
            long Commenttype = (long)CommentType.InternallyCancelled;
            var commonManager = new RequisitionCommonManager(this.JWTToken)
            { UserContext = UserContext, GepConfiguration = GepConfiguration };

            try
            {
                LogHelper.LogInfo(Log, string.Format("EmailNotificationManager SendNotificationForCancelRequisition Method Started for documentCode={0}, documentStatus={1}", documentCode, documentStatus));

                string questionIDs = commonManager.GetSettingsValueByKey(P2PDocumentType.Requisition, "IncludeCustomAttributesInNotification", UserContext.UserId);
                DataSet objRequisitionDS = GetCommonDao().GetBasicDetailsByIdForNotification(documentCode, UserContext.BuyerPartnerCode, Commenttype, questionIDs);
                DataTable objRequisition = objRequisitionDS.Tables[0];
                DataTable objCancellationCommentDT = objRequisitionDS.Tables[1];

                //Custom Attribute Table Builder. Note: customAttributeHtml is being (converted and) passed as a string and not a stringbuilder.
                StringBuilder customAttributeHtml = new StringBuilder();
                if (objRequisitionDS.Tables.Count > 2 && questionIDs != string.Empty)
                {
                    DataTable dtReqCustomAttributes = objRequisitionDS.Tables[2];
                    customAttributeHtml = CustomAttributeBuilder(dtReqCustomAttributes);
                }

                if (objRequisition != null && objRequisition.Rows.Count > 0)
                {

                    List<MailAdressInfo> toMails = new List<MailAdressInfo>();
                    string cancellationComment = "";
                    if (objCancellationCommentDT.Rows.Count > 0)
                    {
                        cancellationComment = Convert.ToString(objCancellationCommentDT.Rows[0]["Comment"]);
                    }
                    //requistion creator
                    var requesterName = objRequisition.Rows[0][SqlConstants.COL_REQUESTER_NAME].ToString();
                    var requesterId = (long)objRequisition.Rows[0][SqlConstants.COL_REQUESTER_ID];
                    var requesterEmailID = objRequisition.Rows[0][SqlConstants.COL_REQUESTER_EMAIL_ID].ToString();
                    toMails.Add(new MailAdressInfo() { ContactCode = requesterId, EmailAddress = requesterEmailID });
                    result = SendFieldValueRequisitionCancelled(toMails, objRequisition, requesterName, requesterName, documentCode, customAttributeHtml.ToString(), cancellationComment);
                    toMails.Clear();

                    //OBO requester
                    var OBORequester = objRequisition.Rows[0][SqlConstants.COL_ONBEHALFOFNAME].ToString();
                    var OBOContactCode = (long)objRequisition.Rows[0][SqlConstants.COL_ONBEHALFOF];
                    var OBOEmail = objRequisition.Rows[0][SqlConstants.COL_ONBEHALFOFEMAIL].ToString();
                    if (OBOContactCode > 0 && OBOContactCode != requesterId)
                    {
                        toMails.Add(new MailAdressInfo() { ContactCode = OBOContactCode, EmailAddress = OBOEmail });
                        result = SendFieldValueRequisitionCancelled(toMails, objRequisition, OBORequester, requesterName, documentCode, customAttributeHtml.ToString(), cancellationComment);
                        toMails.Clear();
                    }
                    var teamMembers = GetNewReqDao().GetDocumentStakeholderDetails(documentCode);
                    if (teamMembers.Result.Count >=0)
                    {
                        for(var i = 0; i < teamMembers.Result.Count; i++)
                        {
                            if(teamMembers.Result[i].StakeholderTypeInfo == StakeholderType.TeamMembers)
                            {
                                toMails.Add(new MailAdressInfo() { ContactCode = teamMembers.Result[i].ContactCode, EmailAddress = teamMembers.Result[i].EmailId });
                                result = SendFieldValueRequisitionCancelled(toMails, objRequisition, teamMembers.Result[i].ContactName, requesterName, documentCode, customAttributeHtml.ToString(), cancellationComment);
                                toMails.Clear();
                            }
                        }
                    }
                        if (documentStatus == DocumentStatus.Approved)
                    {
                        var buyerAssignmentSettingValue = commonManager.GetSettingsValueByKey(P2PDocumentType.None, "RequisitionStatusForBuyerAssignment", UserContext.ContactCode).ToLower();
                        if (!string.IsNullOrEmpty(buyerAssignmentSettingValue))
                        {
                            var ordercontactEmail = objRequisition.Rows[0][SqlConstants.COL_ORDER_CONTACT_EMAIL].ToString();
                            var orderContactCode = (long)objRequisition.Rows[0][SqlConstants.COL_ORDER_CONTACTCODE];

                            if (orderContactCode > 0 && !string.IsNullOrEmpty(ordercontactEmail))
                            {
                                toMails.Clear();
                                toMails.Add(new MailAdressInfo() { ContactCode = orderContactCode, EmailAddress = ordercontactEmail });
                                result = SendFieldValueRequisitionCancelled(toMails, objRequisition, objRequisition.Rows[0][SqlConstants.COL_ORDER_CONTACT_NAME].ToString(), requesterName, documentCode, customAttributeHtml.ToString(), cancellationComment);
                            }
                        }
                    }

                }
            }
            catch (CommunicationException commFaultEx)
            {
                LogHelper.LogError(Log, string.Format("Fault exception occured in EmailNotificationManager SendNotificationForCancelRequisition Method for requisitionId={0}, documentStatus={1}", documentCode, documentStatus), commFaultEx);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, string.Format("Error occured in EmailNotificationManager SendNotificationForCancelRequisition Method for requisitionId={0}, documentStatus={1}", documentCode, documentStatus), ex);
                throw;
            }
            return result;
        }
        private bool SendFieldValueRequisitionCancelled(List<MailAdressInfo> toMails, DataTable objRequisition, string userName, string requesterName, long documentCode, string customAttributeHTML, string comment)
        {
            var ccMails = new SortedList<long, string>();
            var attachments = new SortedList<long, string>();

            var fieldValues = new SortedList<string, string>
                            {
                                {"[RequisitionName]", objRequisition.Rows[0][SqlConstants.COL_REQUISITION_NAME].ToString()},
                                {"[RequisitionNumber]", objRequisition.Rows[0][SqlConstants.COL_REQUISITION_NUMBER].ToString()},
                                {"[RequisitionTotalAmount]", (Convert.ToDecimal(objRequisition.Rows[0][SqlConstants.COL_OVERALLTOTALAMOUNT].ToString()) > 0 )  ? this.objUtils.FormatNumber(Convert.ToDecimal(objRequisition.Rows[0][SqlConstants.COL_OVERALLTOTALAMOUNT].ToString()),this.cultureInfo):this.objUtils.FormatNumber(Convert.ToDecimal(objRequisition.Rows[0][SqlConstants.COL_REQUISITION_AMOUNT].ToString()),this.cultureInfo)},
                                {"[UserName]",userName},
                                {"[Currency]", GetCurrencySymbol(objRequisition.Rows[0][SqlConstants.COL_CURRENCY].ToString())},
                                {"[CancelledBy]", UserContext.UserName},
                                {"[CancelledDate]", DateTime.Now.ToString()},
                                {"[RequisitionAuthor]",requesterName},
                                {"[OBORequestor]",objRequisition.Rows[0][SqlConstants.COL_ONBEHALFOFNAME].ToString()},
                                {"[BuyerCompanyName]",Convert.ToString(objRequisition.Rows[0][SqlConstants.COL_BUYER_COMPANY_NAME],CultureInfo.CurrentCulture)},
                                {"[BuyerCompanyLogo]", MultiRegionConfig.GetConfig(CloudConfig.BlobURL) + "cumuluscontent/logo/" + UserContext.BuyerPartnerCode + "_logo.jpg"},
                                {"[Custom Attributes]", customAttributeHTML },
                                {"[Comment]", comment }
                            };

            #region Export Requisition for Attachments

            var objRequisitionExportManager = new RequisitionExportManager(this.JWTToken)
            {
                UserContext = UserContext,
                GepConfiguration = GepConfiguration
            };

            var fileDetails = objRequisitionExportManager.RequisitionExportById(documentCode, UserContext.ContactCode, 0, ((int)AccessType.Both).ToString());
            if (fileDetails != null && fileDetails.FileId > 0)
                attachments.Add(fileDetails.FileId, string.Empty);

            #endregion Export Requisition for Attachments

            return SendNotification("P2P420", toMails, ccMails, fieldValues, attachments, UserContext.BuyerPartnerCode, documentCode, DocumentType.Requisition);
        }
        public bool SendNotificationForHoldRequisition(long documentCode, int extendedStatus)
        {
            bool result = false;
            long Commenttype = (long)CommentType.Hold;
           
            var commonManager = new RequisitionCommonManager(this.JWTToken)
            { UserContext = UserContext, GepConfiguration = GepConfiguration };

            try
            {
                LogHelper.LogInfo(Log, string.Format("EmailNotificationManager SendNotificationForHoldRequisition Method Started for documentCode={0}", documentCode));

                string questionIDs = commonManager.GetSettingsValueByKey(P2PDocumentType.Requisition, "IncludeCustomAttributesInNotification", UserContext.UserId);
                DataSet objRequisitionDS = GetCommonDao().GetBasicDetailsByIdForNotification(documentCode, UserContext.BuyerPartnerCode, Commenttype, questionIDs);
                DataTable objRequisition = objRequisitionDS.Tables[0];
                DataTable objCommentDT = objRequisitionDS.Tables[1];

                //Custom Attribute Table Builder. Note: customAttributeHtml is being (converted and) passed as a string and not a stringbuilder.
                StringBuilder customAttributeHtml = new StringBuilder();
                if (objRequisitionDS.Tables.Count > 2 && questionIDs != string.Empty)
                {
                    DataTable dtReqCustomAttributes = objRequisitionDS.Tables[2];
                    customAttributeHtml = CustomAttributeBuilder(dtReqCustomAttributes);
                }

                if (objRequisition != null && objRequisition.Rows.Count > 0)
                {

                    List<MailAdressInfo> toMails = new List<MailAdressInfo>();
                    string holdComment = "";
                    if (objCommentDT.Rows.Count > 0)
                    {
                        holdComment = Convert.ToString(objCommentDT.Rows[0]["Comment"]);
                    }
                    //requistion creator
                    var requesterName = objRequisition.Rows[0][SqlConstants.COL_REQUESTER_NAME].ToString();
                    var requesterId = (long)objRequisition.Rows[0][SqlConstants.COL_REQUESTER_ID];
                    var requesterEmailID = objRequisition.Rows[0][SqlConstants.COL_REQUESTER_EMAIL_ID].ToString();
                    toMails.Add(new MailAdressInfo() { ContactCode = requesterId, EmailAddress = requesterEmailID });
                    result = SendFieldValueRequisitionOnHoldandReleaseHold(toMails, objRequisition, requesterName, requesterName, documentCode, customAttributeHtml.ToString(), holdComment, extendedStatus);
                    toMails.Clear();

                    //OBO requester
                    var OBORequester = objRequisition.Rows[0][SqlConstants.COL_ONBEHALFOFNAME].ToString();
                    var OBOContactCode = (long)objRequisition.Rows[0][SqlConstants.COL_ONBEHALFOF];
                    var OBOEmail = objRequisition.Rows[0][SqlConstants.COL_ONBEHALFOFEMAIL].ToString();
                    if (OBOContactCode > 0 && OBOContactCode != requesterId)
                    {
                        toMails.Add(new MailAdressInfo() { ContactCode = OBOContactCode, EmailAddress = OBOEmail });
                        result = SendFieldValueRequisitionOnHoldandReleaseHold(toMails, objRequisition, OBORequester, requesterName, documentCode, customAttributeHtml.ToString(), holdComment, extendedStatus);
                        toMails.Clear();
                    }
                    var teamMembers = GetNewReqDao().GetDocumentStakeholderDetails(documentCode);
                    if (teamMembers.Result.Count >= 0)
                    {
                        for (var i = 0; i < teamMembers.Result.Count; i++)
                        {
                            if (teamMembers.Result[i].StakeholderTypeInfo == StakeholderType.TeamMembers)
                            {
                                toMails.Add(new MailAdressInfo() { ContactCode = teamMembers.Result[i].ContactCode, EmailAddress = teamMembers.Result[i].EmailId });
                                result = SendFieldValueRequisitionOnHoldandReleaseHold(toMails, objRequisition, teamMembers.Result[i].ContactName, requesterName, documentCode, customAttributeHtml.ToString(), holdComment, extendedStatus);
                                toMails.Clear();
                            }
                        }
                    }
                }
            }
            catch (CommunicationException commFaultEx)
            {
                LogHelper.LogError(Log, string.Format("Fault exception occured in EmailNotificationManager SendNotificationForHoldRequisition Method for requisitionId={0}", documentCode), commFaultEx);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, string.Format("Error occured in EmailNotificationManager SendNotificationForHoldRequisition Method for requisitionId={0}", documentCode), ex);
                throw;
            }
            return result;
        }
        public bool SendNotificationForReleaseHoldRequisition(long documentCode, int extendedStatus, long onHeldBy)
        {
            bool result = false;
            long Commenttype = (long)CommentType.Release;
            var commonManager = new RequisitionCommonManager(this.JWTToken)
            { UserContext = UserContext, GepConfiguration = GepConfiguration };

            try
            {
                LogHelper.LogInfo(Log, string.Format("EmailNotificationManager SendNotificationForReleaseHoldRequisition Method Started for documentCode={0}", documentCode));

                string questionIDs = commonManager.GetSettingsValueByKey(P2PDocumentType.Requisition, "IncludeCustomAttributesInNotification", UserContext.UserId);
                DataSet objRequisitionDS = GetCommonDao().GetBasicDetailsByIdForNotification(documentCode, UserContext.BuyerPartnerCode, Commenttype, questionIDs);
                DataTable objRequisition = objRequisitionDS.Tables[0];
                DataTable objCommentDT = objRequisitionDS.Tables[1];

                //Custom Attribute Table Builder. Note: customAttributeHtml is being (converted and) passed as a string and not a stringbuilder.
                StringBuilder customAttributeHtml = new StringBuilder();
                if (objRequisitionDS.Tables.Count > 2 && questionIDs != string.Empty)
                {
                    DataTable dtReqCustomAttributes = objRequisitionDS.Tables[2];
                    customAttributeHtml = CustomAttributeBuilder(dtReqCustomAttributes);
                }

                if (objRequisition != null && objRequisition.Rows.Count > 0)
                {

                    List<MailAdressInfo> toMails = new List<MailAdressInfo>();
                    string releaseHoldComment = "";
                    if (objCommentDT.Rows.Count > 0)
                    {
                        releaseHoldComment = Convert.ToString(objCommentDT.Rows[0]["Comment"]);
                    }
                    //requisition put on hold by
                    var contactDetails = GetNewReqDao().GetContactDetailsByContactCode(onHeldBy);
                    var contactName = contactDetails.FirstName + " " + contactDetails.LastName;
                    var requesterName = objRequisition.Rows[0][SqlConstants.COL_REQUESTER_NAME].ToString();
                    toMails.Add(new MailAdressInfo() { ContactCode = onHeldBy, EmailAddress = contactDetails.EmailAddress});
                    result = SendFieldValueRequisitionOnHoldandReleaseHold(toMails, objRequisition, contactName, requesterName, documentCode, customAttributeHtml.ToString(), releaseHoldComment, extendedStatus);
                    toMails.Clear();
                }
            }
            catch (CommunicationException commFaultEx)
            {
                LogHelper.LogError(Log, string.Format("Fault exception occured in EmailNotificationManager SendNotificationForReleaseHoldRequisition Method for requisitionId={0}", documentCode), commFaultEx);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, string.Format("Error occured in EmailNotificationManager SendNotificationForReleaseHoldRequisition Method for requisitionId={0}", documentCode), ex);
                throw;
            }
            return result;
        }

        private bool SendFieldValueRequisitionOnHoldandReleaseHold(List<MailAdressInfo> toMails, DataTable objRequisition, string userName, string requesterName, long documentCode, string customAttributeHTML, string comment, int extendedStatus)
        {
            var ccMails = new SortedList<long, string>();
            var attachments = new SortedList<long, string>();

            var fieldValues = new SortedList<string, string>
                            {
                                {"[RequisitionName]", objRequisition.Rows[0][SqlConstants.COL_REQUISITION_NAME].ToString()},
                                {"[RequisitionNumber]", objRequisition.Rows[0][SqlConstants.COL_REQUISITION_NUMBER].ToString()},
                                {"[RequisitionTotalAmount]", (Convert.ToDecimal(objRequisition.Rows[0][SqlConstants.COL_OVERALLTOTALAMOUNT].ToString()) > 0 )  ? this.objUtils.FormatNumber(Convert.ToDecimal(objRequisition.Rows[0][SqlConstants.COL_OVERALLTOTALAMOUNT].ToString()),this.cultureInfo):this.objUtils.FormatNumber(Convert.ToDecimal(objRequisition.Rows[0][SqlConstants.COL_REQUISITION_AMOUNT].ToString()),this.cultureInfo)},
                                {"[UserName]",userName},
                                {"[Currency]", GetCurrencySymbol(objRequisition.Rows[0][SqlConstants.COL_CURRENCY].ToString())},
                                {"[HeldBy]", UserContext.UserName},
                                {"[HeldDate]", DateTime.Now.ToString()},
                                {"[RequisitionAuthor]",requesterName},
                                {"[OBORequestor]",objRequisition.Rows[0][SqlConstants.COL_ONBEHALFOFNAME].ToString()},
                                {"[BuyerCompanyName]",Convert.ToString(objRequisition.Rows[0][SqlConstants.COL_BUYER_COMPANY_NAME],CultureInfo.CurrentCulture)},
                                {"[BuyerCompanyLogo]", MultiRegionConfig.GetConfig(CloudConfig.BlobURL) + "cumuluscontent/logo/" + UserContext.BuyerPartnerCode + "_logo.jpg"},
                                {"[Custom Attributes]", customAttributeHTML },
                                {"[Comment]", comment }
                            };

            #region Export Requisition for Attachments

            var objRequisitionExportManager = new RequisitionExportManager(this.JWTToken)
            {
                UserContext = UserContext,
                GepConfiguration = GepConfiguration
            };

            var fileDetails = objRequisitionExportManager.RequisitionExportById(documentCode, UserContext.ContactCode, 0, ((int)AccessType.Both).ToString());
            if (fileDetails != null && fileDetails.FileId > 0)
                attachments.Add(fileDetails.FileId, string.Empty);

            #endregion Export Requisition for Attachments
            if (extendedStatus == 3)
            {
                return SendNotification("P2P425", toMails, ccMails, fieldValues, attachments, UserContext.BuyerPartnerCode, documentCode, DocumentType.Requisition);
            }
            else
            {
                return SendNotification("P2P426", toMails, ccMails, fieldValues, attachments, UserContext.BuyerPartnerCode, documentCode, DocumentType.Requisition);
            }
        }
    }
}
