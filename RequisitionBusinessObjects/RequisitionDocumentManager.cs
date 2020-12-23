using GEP.Cumulus.Logging;
using GEP.Cumulus.P2P.BusinessEntities;
using GEP.Cumulus.P2P.BusinessObjects;
using GEP.Cumulus.P2P.Req.DataAccessObjects;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using GEP.Cumulus.Documents.Entities;
using GEP.Cumulus.P2P.DataAccessObjects;
using GEP.Cumulus.P2P.Req.BusinessObjects.Proxy;
using System.Collections.ObjectModel;
using System.Data;
using QuestionBankEntities = GEP.Cumulus.QuestionBank.Entities;
using Gep.Cumulus.CSM.Entities;
using Gep.Cumulus.Partner.Entities;
using System.Globalization;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using GEP.Cumulus.P2P.DataAccessObjects.SQLServer;
using GEP.Cumulus.Documents.DataAccessObjects.SQLServer;
using GEP.Cumulus.Caching;
using REDataModel;
using GEP.SMART.Configuration;
using GEP.Cumulus.P2P.BusinessObjects.Proxy;
using Gep.Cumulus.CSM.Extensions;
using Newtonsoft.Json;
using System.ServiceModel;
using System.Web.Script.Serialization;
using System.Collections.Specialized;
using System.IO;
using GEP.Cumulus.P2P.Common;
using System.Web;
using GEP.Cumulus.DocumentIntegration.Entities;
using GEP.Cumulus.OrganizationStructure.Entities;
using GEP.Cumulus.P2P.Req.BusinessObjects.Entities;
using S2C.Integration.Flip.Nuget;

namespace GEP.Cumulus.P2P.Req.BusinessObjects
{
    public class RequisitionDocumentManager : RequisitionBaseBO
    {
        const string DisableCommentsManagerApiCall = "NO";
        const string EnableCommentsManagerApiCall = "YES";
        const string GetComments = "1";
        const string GetCommentAttachments = "2";
        const string SaveComment = "3";
        const string SaveCommentAttachments = "4";
        const string DeleteComment = "5";
        const string DeleteAttachment = "7";
        const string DisableDeleteComments = "8";

        private static readonly ILog Log = Logger.GetLog(MethodBase.GetCurrentMethod().DeclaringType);
        public static int BUSINESS_UNIT_ENTITY_ID = 7;
        public static string DEFAULT_CULTURECODE = "en-US";
        public static string DEFAULT_CURRENCYCODE = "USD";
        public const string FUNCTIONAL_ADMIN = "11400003";
        public const string VENDOR_BUYER_USER = "10700203";

        private HttpWebRequest httpWebRequest = null;

        public RequisitionDocumentManager(string jwtToken, UserExecutionContext context = null) : base(jwtToken)
        {
            if (context != null)
            {
                this.UserContext = context;
            }
        }

        public P2PDocument GetBasicDetailsById(P2PDocumentType docType, long id, long userId, int typeOfUser = 0, bool filterByBU = true, string accessType = "0")
        {
            P2PDocument objP2PDoc;

            var commonManager = new RequisitionCommonManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
            int ACEntityId = 0;
            long LOBId = GetCommonDao().GetLOBByDocumentCode(id);
            var settingDetails = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.None, UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBId);
            int precessionValue = convertStringToInt(commonManager.GetSettingsValueByKey(settingDetails, "MaxPrecessionValue"));
            int maxPrecessionValueforTotal = convertStringToInt(commonManager.GetSettingsValueByKey(settingDetails, "MaxPrecessionValueforTotal"));
            int maxPrecessionValueForTaxesAndCharges = convertStringToInt(commonManager.GetSettingsValueByKey(settingDetails, "MaxPrecessionValueForTaxesAndCharges"));
            ACEntityId = GetAccessControlEntityId(docType, UserContext.ContactCode);
            string documentStatuses = "";
            bool isFunctionalAdmin = false;
            if (docType == P2PDocumentType.Requisition)
            {
                List<string> lstUserActivities = new List<string>();
                SettingDetails P2PsettingDetails = commonManager.GetSettingsFromSettingsComponent(docType, UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBId);
                documentStatuses = commonManager.GetSettingsValueByKey(P2PsettingDetails, "AccessibleDocumentStatuses");

                var partnerHelper = new RESTAPIHelper.PartnerHelper(this.UserContext, JWTToken);
                lstUserActivities = partnerHelper.GetUserActivitiesByContactCode(UserContext.ContactCode, UserContext.BuyerPartnerCode).Split(',').ToList();

                isFunctionalAdmin = lstUserActivities.Where(p => p == FUNCTIONAL_ADMIN).ToList().Any() ? true : false;
            }
            objP2PDoc = GetReqDao().GetBasicDetailsById(id, userId, typeOfUser, filterByBU, isFunctionalAdmin, documentStatuses, precessionValue, maxPrecessionValueforTotal, maxPrecessionValueForTaxesAndCharges, accessType, ACEntityId);
            // This lstComments gives CommentGroup Count and not the count of the Comments.
            var lstComments = commonManager.GetCommentsForDocuments(id, docType, UserContext.ContactCode, objP2PDoc.DocumentStakeHolderList.ToList(), typeOfUser);
            objP2PDoc.CommentCount = lstComments.Count;

            return objP2PDoc;
        }

        private int convertStringToInt(string value)
        {
            return Convert.ToInt16(string.IsNullOrEmpty(value) ? "0" : value);
        }


        public ICollection<P2PItem> GetLineItemBasicDetails(P2PDocumentType docType, long id, GEP.Cumulus.P2P.BusinessEntities.ItemType itemType,
                                                            int startIndex, int pageSize, string sortBy,
                                                            string sortOrder, int typeOfUser = 0, int searchInField = 1, string searchFor = "", bool isOrderBasedCreditMemo = false, long parentDocumentCode = 0)
        {
            ICollection<P2PItem> result = null;
            try
            {

                var commonManager = new RequisitionCommonManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
                int precessionValue, precessionValueforTotal, precessionValueForTaxesAndCharges;
                bool CommentsCountRequired = true;
                var settingDetails = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.None, UserContext.ContactCode, (int)SubAppCodes.P2P);
                precessionValue = Convert.ToInt16(commonManager.GetSettingsValueByKey(settingDetails, "MaxPrecessionValue"));
                precessionValueforTotal = Convert.ToInt16(commonManager.GetSettingsValueByKey(settingDetails, "MaxPrecessionValueforTotal"));
                precessionValueForTaxesAndCharges = Convert.ToInt16(commonManager.GetSettingsValueByKey(settingDetails, "MaxPrecessionValueForTaxesAndCharges"));
                CommentsCountRequired = Convert.ToBoolean(commonManager.GetSettingsValueByKey(settingDetails, "CommentsCountRequired"));
                result = GetReqDao().GetLineItemBasicDetails(id, itemType, startIndex, pageSize, sortBy, sortOrder, typeOfUser, searchInField, searchFor, precessionValue, precessionValueforTotal, precessionValueForTaxesAndCharges, isOrderBasedCreditMemo, CommentsCountRequired, parentDocumentCode);

            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occurred in GetLineItemBasicDetails method in RequisitionDocumentManager.", ex);
                throw ex;
            }
            return result;
        }


        public string GenerateDefaultName(P2PDocumentType docType, long userId, long preDocumentId, long LOBEntityDetailCode, string orderNumber = "")
        {
            string key = string.Empty;

            switch (docType)
            {
                case P2PDocumentType.Requisition:
                    key = "RequisitionPrefix";
                    break;
                case P2PDocumentType.Order:
                    key = "OrderPrefix";
                    break;
                case P2PDocumentType.Receipt:
                    key = "ReceiptPrefix";
                    break;
                case P2PDocumentType.Invoice:
                    key = "InvoicePrefix";
                    break;
                case P2PDocumentType.InvoiceReconciliation:
                    key = "IRPrefix";
                    break;
                case P2PDocumentType.ReturnNote:
                    key = "ReturnNotePrefix";
                    docType = P2PDocumentType.Receipt;
                    break;
                case P2PDocumentType.CreditMemo:
                    key = "CreditMemoPrefix";
                    docType = P2PDocumentType.CreditMemo;
                    break;
                case P2PDocumentType.Program:
                    key = "ProgramPrefix";
                    docType = P2PDocumentType.Program;
                    break;
                case P2PDocumentType.PaymentRequest:
                    key = "PaymentRequestPrefix";
                    docType = P2PDocumentType.PaymentRequest;
                    break;
                case P2PDocumentType.ServiceConfirmation:
                    key = "ServiceConfirmationPrefix";
                    docType = P2PDocumentType.ServiceConfirmation;
                    break;
                case P2PDocumentType.ASN:
                    key = "ASNPrefix";
                    docType = P2PDocumentType.ASN;
                    break;
            }

            var commonManager = new RequisitionCommonManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };

            SettingDetails settingDetails = commonManager.GetSettingsFromSettingsComponent(docType, userId, (int)SubAppCodes.P2P, "", LOBEntityDetailCode);

            var documentNumberFormat = commonManager.GetSettingsValueByKey(settingDetails, key);
            var orderNameFormat = commonManager.GetSettingsValueByKey(settingDetails, "OrderNameFormat");

            return GetReqDao().GenerateDefaultName(userId, documentNumberFormat, preDocumentId);
        }

        public long Save(P2PDocumentType docType, P2PDocument obj, bool isShipToChanged = false, long preDocumentCode = 0, bool updateLineItemShipToLocation = false, bool isMaterialRecevingToleranceChanged = false, bool isServiceRecevingToleranceChanged = false, bool flipCustomFields = false, bool calledFrom2O = false, Dictionary<string, object> lstParam = null, bool IsTriggerADR = false)
        {
            long docId = 0;
            try
            {
                Dictionary<string, string> dcSettings = new Dictionary<string, string>();
                if (null != obj)
                {
                    Validator<P2PDocument> valDocItem = ValidationFactory.CreateValidator<P2PDocument>("Group1");
                    ValidationResults valResults = valDocItem.Validate(obj);
                    obj.DocumentType = docType;
                    var commonManager = new RequisitionCommonManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
                    var p2pSettings = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.None, UserContext.ContactCode, (int)SubAppCodes.P2P, "", obj.EntityDetailCode.FirstOrDefault());
                    int precessionValue = convertStringToInt(commonManager.GetSettingsValueByKey(p2pSettings, "MaxPrecessionValue"));
                    int maxPrecessionforTotal = convertStringToInt(commonManager.GetSettingsValueByKey(p2pSettings, "MaxPrecessionValueforTotal"));
                    int maxPrecessionForTaxesAndCharges = convertStringToInt(commonManager.GetSettingsValueByKey(p2pSettings, "MaxPrecessionValueForTaxesAndCharges"));
                    var interfaceSettings = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.Interfaces, UserContext.ContactCode, (int)SubAppCodes.Interfaces);

                    var settingDetailnew = commonManager.GetSettingsValueByKey(interfaceSettings, "OrderNumberassigntoname");
                    var OrderNumberassigntoname = !string.IsNullOrEmpty(settingDetailnew) ? settingDetailnew : "2";
                    Boolean isSaveFOBShippingdetails = false;

                    var st = new StringBuilder();
                    if (valResults.IsValid)
                    {
                        Collection<DocumentAdditionalEntityInfo> DocumentAdditionalEntitiesInfoList = new Collection<DocumentAdditionalEntityInfo>();
                        if (obj.DocumentId <= 0 && string.IsNullOrWhiteSpace(obj.DocumentNumber))
                        {
                            var EntityIdMappedToNumberConfiguration = Convert.ToInt32(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "EntityIdMappedToNumberConfiguration", UserContext.UserId, (int)SubAppCodes.P2P, "", obj.EntityDetailCode[0]));
                            var NumberConfigEntity = obj.DocumentAdditionalEntitiesInfoList.Where(data => data.EntityId == EntityIdMappedToNumberConfiguration).FirstOrDefault();
                            long NumberConfigEntityDetailedCode = 0;

                            if (NumberConfigEntity != null)
                                NumberConfigEntityDetailedCode = NumberConfigEntity.EntityDetailCode;
                            obj.DocumentStatusInfo = DocumentStatus.Draft;

                            switch (docType)
                            {
                                case P2PDocumentType.Requisition:
                                    if (obj.DocumentNumber == "" || obj.DocumentNumber == null)
                                        obj.DocumentNumber = GetEntityNumberForRequisition("REQ", obj.EntityDetailCode[0], NumberConfigEntityDetailedCode);
                                    break;
                                case P2PDocumentType.Order:
                                    Order objOrder = (Order)obj;
                                    //ORDER-4965       
                                    bool PONumberingBasedonPurchaseType = false;
                                    var orderSettings = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.Order, UserContext.ContactCode, (int)SubAppCodes.P2P, "", obj.EntityDetailCode.FirstOrDefault());
                                    PONumberingBasedonPurchaseType = Convert.ToBoolean(commonManager.GetSettingsValueByKey(orderSettings, "PONumberingBasedonPurchaseType"));

                                    //ORDER-4965
                                    //ORDER-4965

                                    if (PONumberingBasedonPurchaseType == false)

                                    {
                                        obj.DocumentNumber = GetEntityNumberForRequisition("PO", obj.EntityDetailCode[0], NumberConfigEntityDetailedCode);
                                    }
                                    else
                                    {
                                        obj.DocumentNumber = GetEntityNumberForRequisition("PO", obj.EntityDetailCode[0], NumberConfigEntityDetailedCode, objOrder.PurchaseType);
                                    }

                                    //ORDER-4965
                                    //if OrderNameformat is 2 then append 




                                    if (obj.DocumentSourceTypeInfo == DocumentSourceType.Interface && OrderNumberassigntoname == "1")
                                    {
                                        obj.DocumentName = obj.DocumentNumber;
                                    }
                                    else
                                    {
                                        var settingDetails = commonManager.GetSettingsFromSettingsComponent(docType, obj.CreatedBy, 107);
                                        var orderNameFormat = commonManager.GetSettingsValueByKey(settingDetails, "OrderNameFormat");
                                        if (orderNameFormat == "2")
                                            obj.DocumentName = string.IsNullOrWhiteSpace(obj.DocumentName) ? obj.DocumentNumber : obj.DocumentName + "-" + obj.DocumentNumber;

                                    }
                                    break;
                                case P2PDocumentType.Receipt:
                                    obj.DocumentNumber = GetEntityNumberForRequisition("REC", obj.EntityDetailCode[0]);
                                    break;
                                case P2PDocumentType.InvoiceReconciliation:
                                    obj.DocumentNumber = GetEntityNumberForRequisition("IR", obj.EntityDetailCode[0], NumberConfigEntityDetailedCode);
                                    break;
                                case P2PDocumentType.ReturnNote:
                                    obj.DocumentNumber = GetEntityNumberForRequisition("RET", obj.EntityDetailCode[0]);
                                    break;

                                case P2PDocumentType.Program:
                                    obj.DocumentNumber = GetEntityNumberForRequisition("PGM", obj.EntityDetailCode[0]);
                                    break;
                                case P2PDocumentType.PaymentRequest:
                                    obj.DocumentNumber = GetEntityNumberForRequisition("PR", obj.EntityDetailCode[0], NumberConfigEntityDetailedCode);
                                    break;
                                case P2PDocumentType.ASN:
                                    obj.DocumentNumber = GetEntityNumberForRequisition("ASN", obj.EntityDetailCode[0]);
                                    break;
                                default:
                                    obj.DocumentNumber = string.Empty;
                                    break;
                            }
                        }


                        else if (obj.DocumentId > 0)
                        {
                            Document objDocument = GetDocumentDao().GetDocumentBasicDetails(obj.DocumentId);
                            obj.DocumentStatusInfo = objDocument.DocumentStatusInfo;
                        }

                        if (docType == P2PDocumentType.Order && obj.DocumentSourceTypeInfo == DocumentSourceType.Interface && OrderNumberassigntoname == "1" && !string.IsNullOrEmpty(obj.Operation) && obj.Operation.Equals("new", StringComparison.InvariantCultureIgnoreCase))
                        {
                            obj.DocumentName = obj.DocumentNumber;
                        }
                        else
                        {
                            if (string.IsNullOrWhiteSpace(obj.DocumentName))
                            {
                                if (docType == P2PDocumentType.Requisition)
                                {
                                    var requisitionSettingDetails = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.Requisition, UserContext.ContactCode, (int)SubAppCodes.P2P, "", obj.EntityDetailCode.FirstOrDefault());
                                    var populateDefaultRequisitionName = commonManager.GetSettingsValueByKey(requisitionSettingDetails, "PopulateDefaultRequisitionName");
                                    if (populateDefaultRequisitionName != null)
                                    {
                                        if (Convert.ToBoolean(populateDefaultRequisitionName))
                                        {
                                            obj.DocumentName = GenerateDefaultName(docType, obj.CreatedBy, preDocumentCode, obj.EntityDetailCode.First(), obj.DocumentNumber);
                                        }
                                    }
                                    else
                                    {
                                        //if there is no configuration available in basic settings,default considering to show default requisition name
                                        obj.DocumentName = GenerateDefaultName(docType, obj.CreatedBy, preDocumentCode, obj.EntityDetailCode.First(), obj.DocumentNumber);
                                    }


                                }
                                else
                                    obj.DocumentName = GenerateDefaultName(docType, obj.CreatedBy, preDocumentCode, obj.EntityDetailCode.First(), obj.DocumentNumber);
                            }
                        }

                        UserExecutionContext userExecutionContext = UserContext;
                        if (docType == P2PDocumentType.Order)
                        {
                            // do nothing
                            var temp = 0;
                        }
                        else
                        {
                            if (docType == P2PDocumentType.Requisition || (docType == P2PDocumentType.Invoice && userExecutionContext.IsSupplier == false) || docType == P2PDocumentType.PaymentRequest || docType == P2PDocumentType.ASN)
                            {
                                P2PAccessControlManager objP2PAccessControlManager = new P2PAccessControlManager { UserContext = UserContext, GepConfiguration = GepConfiguration, jwtToken  = this.JWTToken };
                                objP2PAccessControlManager.GetBUDetailList(docType, userExecutionContext, obj);
                            }
                            docId = GetReqDao().Save(obj, precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges, lstParam);
                        }

                        //Getting the Configurations to Delete Line Items when Header Level Org Entity Changed
                        var allowOrgEntityInCatalogItems = false;
                        if (docType != P2PDocumentType.Receipt && docType != P2PDocumentType.PaymentRequest)
                            allowOrgEntityInCatalogItems = Convert.ToBoolean(commonManager.GetSettingsValueByKey(P2PDocumentType.Catalog, "AllowOrgEntityInCatalogItems", (long)UserContext.ContactCode) == string.Empty ? "False" : commonManager.GetSettingsValueByKey(P2PDocumentType.Catalog, "AllowOrgEntityInCatalogItems", (long)UserContext.ContactCode));

                        if (docType == P2PDocumentType.Requisition || docType == P2PDocumentType.Order || docType == P2PDocumentType.PaymentRequest || docType == P2PDocumentType.ASN)
                        {
                            switch (docType)
                            {
                                case P2PDocumentType.Requisition:
                                    DocumentAdditionalEntitiesInfoList = ((Requisition)obj).DocumentAdditionalEntitiesInfoList;
                                    break;

                            }

                            List<DocumentSplitItemEntity> lstDefaultAccounting = null;
                            List<ADRSplit> lstADRSplits = null;
                            if (obj.DocumentId == 0 && (ReferenceEquals(null, DocumentAdditionalEntitiesInfoList) || DocumentAdditionalEntitiesInfoList.Count() == 0))
                            {
                                DocumentAdditionalEntitiesInfoList = SetDocumentAdditionalEntitiesDetails(docType, LevelType.HeaderLevel);
                                if (!CompareInfo.ReferenceEquals(DocumentAdditionalEntitiesInfoList, null) && DocumentAdditionalEntitiesInfoList.Count() > 0)
                                {
                                    var entityId = Convert.ToInt32(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "EntityMappedToBillToLocation", UserContext.UserId));
                                    var entityDetailCode = DocumentAdditionalEntitiesInfoList.Where(data => data.EntityId == entityId).FirstOrDefault().EntityDetailCode;
                                    var flag = false;
                                    if (docType == P2PDocumentType.Requisition)
                                        flag = GetReqDao().UpdateBillToLocation(docId, entityDetailCode, obj.EntityDetailCode[0]);

                                }
                            }
                            else if (obj.DocumentId > 0)
                            {
                                if (isShipToChanged && !IsTriggerADR)
                                {
                                    //to retrive default with new entities
                                    lstDefaultAccounting = GetDocumentDefaultAccountingDetails(docType, LevelType.ItemLevel, obj.CreatedBy, obj.DocumentId,
                                        DocumentAdditionalEntitiesInfoList.ToList());
                                    //DocumentAdditionalEntitiesInfoList.Clear();
                                    foreach (var defaultAccounting in lstDefaultAccounting)
                                    {
                                        if (DocumentAdditionalEntitiesInfoList.Where(p => p.EntityId == defaultAccounting.EntityTypeId).Count() <= 0)
                                        {
                                            DocumentAdditionalEntitiesInfoList.Add(new DocumentAdditionalEntityInfo()
                                            {
                                                EntityCode = defaultAccounting.EntityCode,
                                                EntityDisplayName = defaultAccounting.EntityDisplayName,
                                                EntityId = defaultAccounting.EntityTypeId,
                                                EntityDetailCode = Convert.ToInt64(defaultAccounting.SplitAccountingFieldValue)
                                            });
                                        }
                                    }
                                    //Deleting Line Items based on Header Level Org Entity Change
                                    if (allowOrgEntityInCatalogItems)
                                    {
                                        DeleteLineItemsBasedOnOrgEntity(docType, obj, true);
                                    }
                                }
                            }

                            bool result = true;
                            if (!ReferenceEquals(null, DocumentAdditionalEntitiesInfoList) && (isShipToChanged || (obj.DocumentId == 0 || docType == P2PDocumentType.Program || calledFrom2O || IsTriggerADR))) //Added for handling Program as a document when Program already exist.
                            {
                                result = GetReqDao().SaveDocumentAdditionalEntityInfo(docId, DocumentAdditionalEntitiesInfoList);
                                if (!result && !IsTriggerADR && !calledFrom2O)
                                {
                                    if (lstDefaultAccounting != null)
                                    {
                                        GetReqDao().SaveDefaultAccountingDetails(docId, lstDefaultAccounting, precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges);
                                    }
                                }
                                else if (IsTriggerADR && obj.DocumentId > 0)
                                {
                                    lstADRSplits = GetDocumentDefaultAccountingDetailsForLineItems(docType, 0, docId, DocumentAdditionalEntitiesInfoList.ToList(),
                                        null, false, null, obj.EntityDetailCode.FirstOrDefault(), ADRIdentifier.DocumentItemId);
                                    GetReqDao().SaveDefaultAccountingDetailsforADR(docId, lstADRSplits, precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges, false);
                                }
                            }
                        }
                        if (obj.DocumentCode == 0 && docId > 0 && obj.DocumentSourceTypeInfo == DocumentSourceType.Interface)
                            GetCommonDao().SaveInterfaceDocuments(docId);
                        //Deleting Line Items in Inovice based on Header Level Org Entity Change
                        if (docType == P2PDocumentType.Invoice && obj.DocumentId > 0 && allowOrgEntityInCatalogItems)
                        {
                            DeleteLineItemsBasedOnOrgEntity(docType, obj, true);
                        }

                        if (isShipToChanged)
                        {
                            if (docType == P2PDocumentType.Requisition || docType == P2PDocumentType.Order || docType == P2PDocumentType.Invoice || docType == P2PDocumentType.PaymentRequest)
                                GetReqDao().UpdateTaxOnHeaderShipTo(docId, 0, precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges);
                        }

                        if (obj.DocumentId <= 0 && docId > 0)
                        {
                            obj.DocumentId = docId;
                            #region My Task Implementation
                            switch (docType)
                            {
                                case P2PDocumentType.Requisition:
                                    List<TaskActionDetails> lstTasksAction = new List<TaskActionDetails>();
                                    lstTasksAction.Add(TaskManager.CreateActionDetails(ActionKey.SentForApproval, SqlConstants.SENT_FOR_APPROVAl));
                                    var objTaskManager = new TaskManager() { UserContext = UserContext, GepConfiguration = GepConfiguration, jwtToken = this.JWTToken };
                                    var delegateSaveTaskActionDetails = new TaskManager.InvokeSaveTaskActionDetails(objTaskManager.SaveTaskActionDetails);

                                    delegateSaveTaskActionDetails.BeginInvoke(TaskManager.CreateTaskObject(obj.DocumentId, obj.CreatedBy, lstTasksAction, false, false, UserContext.BuyerPartnerCode, UserContext.CompanyName), null, null);

                                    break;
                                default:
                                    break;
                            }
                            #endregion
                        }
                        else
                            if (Log.IsWarnEnabled)
                            Log.Warn(string.Concat("In Save Method AddInfoToPortal was not called for DocumentId=", obj.DocumentId.ToString(CultureInfo.InvariantCulture), " as DocumentId is greater than 0 OR docId is less than equal to zero"));

                        //if (docType == P2PDocumentType.Invoice)
                        //{
                        //    Task.Factory.StartNew((userContext) => { AddRemoveMyTaskInInvoice(docId); }, this.UserContext);
                        //}

                        if (obj.isFilterEntityChanged || (docType == P2PDocumentType.InvoiceReconciliation && lstParam != null && lstParam.Count > 0 && Convert.ToBoolean(lstParam.Where(x => x.Key == "isFilterEntityChanged").FirstOrDefault().Value)))
                            commonManager.updateUserDefinedApprovalonEntityChange(docId, docType, obj.TotalAmount, obj.Currency);
                    }

                    foreach (ValidationResult item in valResults)
                        st.AppendLine(item.Message);

                    if (st.Length > 0)
                    {
                        if (Log.IsWarnEnabled)
                            Log.Warn(string.Concat("In Save Method validation failed for DocumentId=", obj.DocumentId.ToString(CultureInfo.InvariantCulture), " with following reasons:" + st));
                    }
                }
                else
                {
                    if (Log.IsWarnEnabled)
                        Log.Warn("In Save method obj parameter is null.");
                }
                if (this.UserContext.Product != GEPSuite.eInterface && !calledFrom2O)
                {
                    if (docId > 0)
                    {
                        AddIntoSearchIndexerQueueing(docId, (int)getDocumentType(docType));
                    }
                }

            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occurred in Save method in RequisitionDocumentManager.", ex);
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw();
                throw;
            }

            return docId;
        }

        public long SaveItem(P2PDocumentType docType, P2PItem objItem, bool isTaxExempt = true, bool saveDefaultAccounting = true, bool SaveLineItemTaxes = true, bool ProrateLineItemTax = true, P2PDocument objDocument = null, bool allowPeriodUpdate = true, bool isFunctionalAdmin = false, bool isTriggerADR = false)
        {
            long documentItemId = 0;
            try
            {
                var documentManager = new RequisitionDocumentManager(this.JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
                string shippingMethod = String.Empty;
                var passedDocType = docType;
                long lobEntityDetailCode = 0;

                if (objItem != null)
                {
                    // Validation to be done here
                    var commonManager = new RequisitionCommonManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
                    int precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges, maxPrecessionValueForQuantity;
                    bool allowTaxCodewithAmount;
                    string supplierStatusForValidation;
                    var flipParentItemDetails = false;
                    //if (docType == P2PDocumentType.Invoice && objItem.P2PLineItemId > 0)
                    //{
                    //    var invoiceSettingDetails = commonManager.GetSettingsFromSettingsComponent(docType, UserContext.ContactCode, 107);
                    //    var allowMultipleInvoiceItemMapping = Convert.ToString(commonManager.GetSettingsValueByKey(invoiceSettingDetails, "AllowMultipleInvoiceItemMapping"));
                    //    flipParentItemDetails = allowMultipleInvoiceItemMapping.ToUpper() == "TRUE" ? true : false;
                    //}

                    var entityIdMappedToShippingMethods = Convert.ToInt32(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "EntityMappedToShippingMethods", UserContext.UserId));
                    // shipping method based on LOb and ACE of Document 
                    if (docType == P2PDocumentType.Requisition
                        || docType == P2PDocumentType.Order
                        || docType == P2PDocumentType.Invoice
                        || docType == P2PDocumentType.InvoiceReconciliation
                        || docType == P2PDocumentType.CreditMemo
                        || docType == P2PDocumentType.PaymentRequest)
                    {
                        var obj = GetReqDao().GetDocumentAdditionalEntityDetailsById(objItem.DocumentId);
                        lobEntityDetailCode = obj.EntityDetailCode.FirstOrDefault();
                        var settingDetails = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.None, UserContext.ContactCode, (int)SubAppCodes.P2P, "", obj.EntityDetailCode.FirstOrDefault());
                        precessionValue = convertStringToInt(commonManager.GetSettingsValueByKey(settingDetails, "MaxPrecessionValue"));
                        maxPrecessionValueForQuantity = convertStringToInt(commonManager.GetSettingsValueByKey(settingDetails, "Decimal_MaxPrecessionForQuantity"));
                        maxPrecessionforTotal = convertStringToInt(commonManager.GetSettingsValueByKey(settingDetails, "MaxPrecessionValueforTotal"));
                        maxPrecessionForTaxesAndCharges = convertStringToInt(commonManager.GetSettingsValueByKey(settingDetails, "MaxPrecessionValueForTaxesAndCharges"));
                        shippingMethod = GetCommonDao().GetDefaultShippingMethod((new Order()).GetACEEntityDetailCode(obj.DocumentAdditionalEntitiesInfoList, entityIdMappedToShippingMethods), obj.EntityDetailCode.FirstOrDefault());

                        var invoiceSetting = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.Invoice, UserContext.ContactCode, (int)SubAppCodes.P2P, "", obj.EntityDetailCode.FirstOrDefault());
                        allowTaxCodewithAmount = Convert.ToBoolean(commonManager.GetSettingsValueByKey(invoiceSetting, "AllowTaxCodewithAmount"));
                        supplierStatusForValidation = commonManager.GetSettingsValueByKey(invoiceSetting, "SupplierStatusForValidation");
                    }
                    else
                    {
                        //Document obj = new SQLDocumentDAO() { UserContext = UserContext, GepConfiguration = GepConfiguration }.GetDocumentDetailsById(new Document()
                        //{
                        //    DocumentCode = objItem.DocumentId,
                        //    DocumentName = "",
                        //    DocumentNumber = "",
                        //    IsDocumentDetails = true
                        //}, true);
                        long LOBId = GetCommonDao().GetLOBByDocumentCode(objItem.DocumentId);
                        lobEntityDetailCode = LOBId;
                        var otherSettingDetails = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.None, UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBId);
                        precessionValue = convertStringToInt(commonManager.GetSettingsValueByKey(otherSettingDetails, "MaxPrecessionValue"));
                        maxPrecessionValueForQuantity = convertStringToInt(commonManager.GetSettingsValueByKey(otherSettingDetails, "Decimal_MaxPrecessionForQuantity"));
                        maxPrecessionforTotal = convertStringToInt(commonManager.GetSettingsValueByKey(otherSettingDetails, "MaxPrecessionValueforTotal"));
                        maxPrecessionForTaxesAndCharges = convertStringToInt(commonManager.GetSettingsValueByKey(otherSettingDetails, "MaxPrecessionValueForTaxesAndCharges"));

                        if (docType == P2PDocumentType.Receipt)
                        {
                            maxPrecessionForTaxesAndCharges = maxPrecessionValueForQuantity; // For Receipt we are not using maxPrecessionForTaxesAndCharges
                        }

                        var invoiceSettings = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.Invoice, UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBId);
                        allowTaxCodewithAmount = Convert.ToBoolean(commonManager.GetSettingsValueByKey(invoiceSettings, "AllowTaxCodewithAmount"));
                        supplierStatusForValidation = commonManager.GetSettingsValueByKey(invoiceSettings, "SupplierStatusForValidation");
                    }

                    if (objItem.SourceType == ItemSourceType.Hosted || objItem.SourceType == ItemSourceType.Internal)
                    {
                        if ((short)objItem.ItemType != 3)//should be (short)objItem.ItemType != 3
                        {
                            if (((!ValidateDocumentItem(objItem).Any()) && (!ValidateHostedDocumentItem(objItem).Any())))
                            {
                                documentItemId = GetReqDao().SaveItem(objItem, precessionValue, flipParentItemDetails, shippingMethod, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges);
                            }
                            else
                            {
                                if (Log.IsWarnEnabled)
                                    Log.Warn(string.Concat("In SaveItem ValidateHostedDocumentItem failed for hosted lineitemid=", objItem.DocumentItemId.ToString(CultureInfo.InvariantCulture)));
                            }
                        }
                        else
                        {
                            documentItemId = GetReqDao().SaveItem(objItem, precessionValue, flipParentItemDetails, shippingMethod, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges);
                        }

                    }
                    else
                    {
                        if ((short)objItem.ItemType != 3)//should be (short)objItem.ItemType != 3
                        {
                            if ((!ValidateDocumentItem(objItem).Any()))
                            {
                                documentItemId = GetReqDao().SaveItem(objItem, precessionValue, flipParentItemDetails, shippingMethod, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges);
                            }

                            else
                            {
                                if (Log.IsWarnEnabled)
                                    Log.Warn(string.Concat("In SaveItem ValidateDocumentItem failed for lineitemid=", objItem.DocumentItemId.ToString(CultureInfo.InvariantCulture)));

                            }
                        }
                        else
                        {
                            documentItemId = GetReqDao().SaveItem(objItem, precessionValue, flipParentItemDetails, shippingMethod, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges);
                        }

                    }

                    #region Save default accounting and calculate line Item Tax for New LineItem
                    if (!isTriggerADR)
                    {
                        if ((documentItemId > 0 && (objItem.DocumentItemId == 0 || objItem.DocumentItemId == -1)) || !isTaxExempt)
                        {
                            if (docType != P2PDocumentType.Receipt && saveDefaultAccounting)
                            {
                                long contactCode = 0;
                                bool blnFlipOrderSplits = false;
                                if (docType == P2PDocumentType.Requisition && (short)objItem.ItemType != 3)//(short)objItem.ItemType != 3
                                {
                                    string documentStatuses = "";
                                    SettingDetails settingDetails = commonManager.GetSettingsFromSettingsComponent(docType, UserContext.ContactCode, 107);
                                    documentStatuses = commonManager.GetSettingsValueByKey(settingDetails, "AccessibleDocumentStatuses");
                                    var objRequisition = GetReqDao().GetBasicDetailsById(objItem.DocumentId, UserContext.ContactCode, 0, true, isFunctionalAdmin, documentStatuses, precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges);
                                    if (objRequisition != null)
                                    {
                                        contactCode = ((GEP.Cumulus.P2P.BusinessEntities.Requisition)(objRequisition)).OnBehalfOf;
                                        lobEntityDetailCode = objRequisition.EntityDetailCode.FirstOrDefault();
                                    }
                                }
                                //if (docType == P2PDocumentType.Invoice)
                                //{
                                //    Invoice objInvoice = null;
                                //    if (objDocument != null)
                                //        objInvoice = (Invoice)objDocument;
                                //    else
                                //        objInvoice = (Invoice)GetDao(docType).GetBasicDetailsById(objItem.DocumentId, UserContext.ContactCode, 0, true, false, "", precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges);
                                //    if (objInvoice != null)
                                //    {
                                //        contactCode = objInvoice.OrderContact;
                                //        blnFlipOrderSplits = objInvoice.InvoiceSource == InvoiceSource.ScannedPOInvoice;
                                //    }
                                //}
                                if (!blnFlipOrderSplits)
                                {
                                    List<SplitAccountingFields> lstSplitAccountingFields = new List<SplitAccountingFields>();
                                    if (objItem.CatalogItemId > 0)
                                    {
                                        var settingDetails = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.Catalog, UserContext.ContactCode, 107);

                                        var allowOrgEntityFromCatalog = Convert.ToBoolean(commonManager.GetSettingsValueByKey(settingDetails, "AllowOrgEntityInCatalogItems"), CultureInfo.InvariantCulture).ToString().ToUpper();
                                        var corporationEntityId = Convert.ToInt32(commonManager.GetSettingsValueByKey(settingDetails, "CorporationEntityId"), CultureInfo.InvariantCulture);
                                        var expenseCodeEntityId = Convert.ToInt32(commonManager.GetSettingsValueByKey(settingDetails, "ExpenseCodeEntityId"), CultureInfo.InvariantCulture);

                                        if (allowOrgEntityFromCatalog == "TRUE" && corporationEntityId > 0 && expenseCodeEntityId > 0)
                                        {
                                            DataTable dtORGData = GetCommonDao().GetOrgEntityMappedWithCatalogItem(objItem.CatalogItemId);
                                            if (dtORGData != null && dtORGData.Rows.Count > 0)
                                            {
                                                if (dtORGData.Rows[0][0].ToString() != "")
                                                {
                                                    SplitAccountingFields objCorporation = new SplitAccountingFields();
                                                    objCorporation.EntityDetailCode = Convert.ToInt64(dtORGData.Rows[0][0].ToString(), CultureInfo.InvariantCulture);
                                                    objCorporation.EntityTypeId = corporationEntityId;
                                                    lstSplitAccountingFields.Add(objCorporation);
                                                }
                                                if (dtORGData.Rows[0][1].ToString() != "")
                                                {
                                                    SplitAccountingFields objExpenseCode = new SplitAccountingFields();
                                                    objExpenseCode.EntityDetailCode = Convert.ToInt64(dtORGData.Rows[0][1].ToString(), CultureInfo.InvariantCulture);
                                                    objExpenseCode.EntityTypeId = expenseCodeEntityId;
                                                    lstSplitAccountingFields.Add(objExpenseCode);
                                                }
                                            }
                                        }
                                    }
                                    var documentSplitItemEntities = GetDocumentDefaultAccountingDetails(docType, LevelType.ItemLevel, contactCode, objItem.DocumentId, null, lstSplitAccountingFields, false, documentItemId);
                                    if ((short)objItem.ItemType != 3)
                                    {
                                        GetReqDao().SaveDefaultAccountingDetails(objItem.DocumentId, documentSplitItemEntities, precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges);
                                    }
                                    else
                                    {
                                        GetReqDao().SaveAdvancePaymentDefaultAccountingDetails(objItem.DocumentId, documentSplitItemEntities);
                                    }
                                }
                                //else
                                //{
                                //    //If scanned PO invoice flip split of first item from 
                                //    GetDao<IInvoiceDAO>().SaveAccountingFromPO(objItem.DocumentId, precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges, allowTaxCodewithAmount, supplierStatusForValidation);
                                //}
                            }
                        }
                        if ((docType == P2PDocumentType.Order || docType == P2PDocumentType.Requisition || docType == P2PDocumentType.PaymentRequest || docType == P2PDocumentType.Invoice) && ProrateLineItemTax
                                && (objItem.DocumentItemId > 0 || (objItem.Quantity > 0 && objItem.UnitPrice >= 0)))
                        {
                            var updateTaxOnPunchoutItem = Convert.ToBoolean(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "UpdateTaxOnPunchoutItem", UserContext.ContactCode));

                            if (!(!updateTaxOnPunchoutItem && objItem.PunchoutCartReqId > 0))
                                GetReqDao().ProrateLineItemTax(documentItemId, 0, precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges);
                        }
                    }
                    else
                    {
                        List<ADRSplit> lstADRsplits = GetDocumentDefaultAccountingDetailsForLineItems(docType, 0, objItem.DocumentId, null,
                             null, false, null, lobEntityDetailCode, ADRIdentifier.DocumentItemId);

                        GetReqDao().SaveDefaultAccountingDetailsforADR(objItem.DocumentId, lstADRsplits, precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges, false);
                    }
                    #endregion

                    if (allowPeriodUpdate && (docType == P2PDocumentType.Order || docType == P2PDocumentType.Invoice || docType == P2PDocumentType.PaymentRequest))
                        UpdatePeriodbyNeedbyDate(docType, documentItemId);

                    if (documentItemId > 0 && objItem.DocumentItemId > 0 && SaveLineItemTaxes)
                    {
                        if ((docType == P2PDocumentType.Order || docType == P2PDocumentType.Requisition))
                            GetReqDao().SaveLineItemTaxes(documentItemId, precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges);
                    }
                    //string keyValue = commonManager.GetSettingsValueByKey(P2PDocumentType.PaymentRequest, "IsTaxEditable", commonManager.UserContext.ContactCode, 107);
                    //if (documentItemId > 0 && (docType == P2PDocumentType.PaymentRequest) && SaveLineItemTaxes && !Convert.ToBoolean(keyValue))
                    //{
                    //    GetDao(docType).SaveLineItemTaxes(documentItemId, precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges);
                    //}
                    //if (docType == P2PDocumentType.Receipt)
                    //{
                    //    Task.Factory.StartNew(() => AddReceiptFinalizeTask(objItem.DocumentId, allowTaxCodewithAmount, supplierStatusForValidation));
                    //}
                    //if (docType == P2PDocumentType.Invoice && objItem.DocumentItemId > 0)
                    //{
                    //    if (objDocument == null || objDocument.DocumentStatusInfo == DocumentStatus.Exception)
                    //        GetDao<IInvoiceDAO>().UpdateIRDetailsByInvoiceId(objItem.DocumentId, precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges);
                    //    if (objDocument == null || objDocument.DocumentStatusInfo == DocumentStatus.Error)
                    //    {
                    //        if (((InvoiceItem)objItem).ErrorList != null)
                    //        {
                    //            var invoiceManager = new InvoiceManager { UserContext = UserContext, GepConfiguration = GepConfiguration };
                    //            invoiceManager.ResolveAndUpdateErrorInvoice(objItem.DocumentId, objItem.DocumentItemId, ((InvoiceItem)objItem).ErrorList);
                    //        }
                    //    }
                    //}

                    if (this.UserContext.Product != GEPSuite.eInterface)
                        AddIntoSearchIndexerQueueing(objItem.DocumentId, (int)getDocumentType(passedDocType));
                }
                else
                {
                    if (Log.IsWarnEnabled)
                        Log.Warn("In SaveItem objRequisitionItem is null.");
                }

            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occurred in SaveItem method in RequisitionDocumentManager.", ex);
                throw;
            }
            return documentItemId;
        }


        private static IEnumerable<string> ValidateDocumentItem(P2PItem objItem)
        {
            var lstResults = new List<string>();
            float floatVal;


            if (objItem.UnitPrice.HasValue)
                if (!(float.TryParse(Convert.ToString(objItem.UnitPrice, CultureInfo.InvariantCulture), out floatVal)))
                {
                    lstResults.Add("Please enter a valid Unit Price");
                }

            if (!(float.TryParse(Convert.ToString(objItem.Quantity, CultureInfo.InvariantCulture), out floatVal)))
            {
                lstResults.Add(string.Format(CultureInfo.InvariantCulture, "Please enter a valid Quantity."));
            }

            if (objItem.Tax.HasValue && objItem.UnitPrice > 0)
                if (objItem.Tax.HasValue && objItem.Tax < 0 || !(float.TryParse(Convert.ToString(objItem.Tax, CultureInfo.InvariantCulture), out floatVal)))
                {
                    lstResults.Add(string.Format(CultureInfo.InvariantCulture, "The Tax value can not be negative. Please enter a Tax."));
                }

            return lstResults;
        }


        private static IEnumerable<string> ValidateHostedDocumentItem(P2PItem objItem)
        {
            var lstResults = new List<string>();
            if (objItem.Quantity == 0)
                lstResults.Add("Please enter a valid quantity");
            if (objItem.MaximumOrderQuantity > 0 && objItem.Quantity > objItem.MaximumOrderQuantity)
                lstResults.Add(string.Format(CultureInfo.InvariantCulture, "The quantity cannot be greater than the maximum order quantity. Please enter a quantity less than {0}.",
                                              objItem.MaximumOrderQuantity));
            if (objItem.MinimumOrderQuantity > 0 && objItem.Quantity < objItem.MinimumOrderQuantity)
                lstResults.Add(string.Format(CultureInfo.InvariantCulture, "The quantity cannot be less than the minimum order quantity. Please enter a quantity greater than {0}.",
                                              objItem.MinimumOrderQuantity));
            if (objItem.Banding > 0 && objItem.Quantity % objItem.Banding != 0)
                lstResults.Add(string.Format(CultureInfo.InvariantCulture, "The banding quantity of this item is {0}. Please enter a quantity that is a multiple of the banding quantity.",
                                               objItem.Banding));
            //if (!DAO.CheckDecimalAllowedForUom(objRequisitionItem.UOM) && objRequisitionItem.Quantity % 1 != 0)
            //    lstResults.Add("Please enter a valid quantity");
            return lstResults;
        }

        public long SaveItemPartnerDetails(P2PDocumentType docType, P2PItem objDocItem, int precessionValue = 0)
        {
            long reqItemId = 0;
            try
            {
                if (precessionValue == 0)
                {
                    var commonManager = new RequisitionCommonManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
                    long LOBId = GetCommonDao().GetLOBByDocumentCode(objDocItem.DocumentId);
                    precessionValue = convertStringToInt(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "MaxPrecessionValue", UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBId));
                }
                //DocumentId                    
                if (null != objDocItem)
                {
                    // Validation to be done here

                    reqItemId = GetReqDao().SaveItemPartnerDetails(objDocItem, precessionValue);
                    if (this.UserContext.Product != GEPSuite.eInterface)
                        AddIntoSearchIndexerQueueing(objDocItem.DocumentId, (int)getDocumentType(docType));
                }
                else
                {
                    if (Log.IsWarnEnabled)
                        Log.Warn("In SaveItemPartnerDetails objDocItem is null.");
                }

            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occurred in SaveItemPartnerDetails method in RequisitionDocumentManager.", ex);
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw();
                throw;
            }
            return reqItemId;
        }

        public long SaveItemShippingDetails(P2PDocumentType docType, long documentLineItemShippingId, long documentItemId, string shippingMethod, int shiptoLocationId, int delivertoLocationId, decimal quantity, decimal totalQuantity, long userid, string deliverTo, int precessionValue, int maxPrecessionforTotal, int maxPrecessionForTaxesAndCharges, bool prorateLineItemTax = true)
        {
            long reqItemId = 0;
            try
            {

                if (documentItemId > 0)
                {
                    var lstresult = ValidateSaveItemShippingDetails(shiptoLocationId, quantity,
                                                                    totalQuantity);

                    if (!lstresult.Any())
                    {
                        var commonManager = new RequisitionCommonManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
                        reqItemId = GetReqDao().SaveItemShippingDetails(documentLineItemShippingId, documentItemId, shippingMethod, shiptoLocationId, delivertoLocationId, quantity, totalQuantity, userid, precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges, deliverTo);
                        if (documentItemId > 0 && prorateLineItemTax && (P2PDocumentType.Requisition == docType || P2PDocumentType.Order == docType || P2PDocumentType.PaymentRequest == docType))
                            GetReqDao().ProrateLineItemTax(documentItemId, 0, precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges);

                    }
                    else
                    {
                        if (Log.IsWarnEnabled)
                            Log.Warn("In SaveItemShippingDetails lstresult is not empty.");
                    }
                }
                else
                {
                    if (Log.IsWarnEnabled)
                        Log.Warn("In SaveItemShippingDetails documentItemId is less than equal to 0.");
                }

            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occurred in SaveItemShippingDetails method in RequisitionDocumentManager.", ex);
                throw ex;
            }
            return reqItemId;
        }

        public long SaveItemOtherDetails(P2PDocumentType docType, P2PItem objRequisitionItem, bool allowTaxCodewithAmount, string supplierStatusForValidation)
        {
            long reqItemId = 0;
            try
            {

                if (null != objRequisitionItem)
                {
                    // Validation to be done here
                    reqItemId = GetReqDao().SaveItemOtherDetails(objRequisitionItem, allowTaxCodewithAmount, supplierStatusForValidation);
                    if (this.UserContext.Product != GEPSuite.eInterface)
                        AddIntoSearchIndexerQueueing(objRequisitionItem.DocumentId, (int)getDocumentType(docType));
                }
                else
                {
                    if (Log.IsWarnEnabled)
                        Log.Warn("In SaveItemOtherDetails method objRequisitionItem is null.");
                }

            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occurred in SaveItemOtherDetails method in RequisitionDocumentManager.", ex);
                throw ex;
            }
            return reqItemId;
        }



        public void AddInfoToPortal(P2PDocumentType docType, P2PDocument objDoc)
        {
            if (objDoc != null && objDoc.DocumentId > 0)
            {
                var objProxyPortal = new ProxyPortal();
                objDoc.DocumentType = docType;
                objProxyPortal.UserContext = UserContext;
                var delegateAddDocumentInfo = new ProxyPortal.InvokeAddDocumentInfo(objProxyPortal.SaveDocumentInfo);
                delegateAddDocumentInfo.BeginInvoke(objDoc, null, null);
            }
        }


        public bool DeleteLineItemByIds(P2PDocumentType docType, string lineItemIds, int precessionValue, int maxPrecessionValueforTotal, int maxPrecessionValueForTaxesAndCharges, bool isAdvanced = false)
        {
            bool IsSupplierCurrency = getSupplierCurrencySetting();
            bool result = false;
            try
            {

                if (!String.IsNullOrWhiteSpace(lineItemIds))
                    result = GetReqDao().DeleteLineItemByIds(lineItemIds, isAdvanced, precessionValue, maxPrecessionValueforTotal, maxPrecessionValueForTaxesAndCharges, IsSupplierCurrency);//need documentcode for indexing

                if (Log.IsWarnEnabled)
                {
                    if (lineItemIds == null)
                        Log.Warn("In DeleteLineItemByIds method lineItemIds parameter is blank.");
                }

            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occurred in SaveItemPartnerDetails method in RequisitionDocumentManager.", ex);
                throw ex;
            }
            return result;
        }

        public ICollection<DocumentItemShippingDetail> GetShippingSplitDetailsByLiId(P2PDocumentType docType, long lineItemId)
        {
            return GetReqDao().GetShippingSplitDetailsByLiId(lineItemId);
        }

        private static IEnumerable<string> ValidateSaveItemShippingDetails(int shiptoLocationId, decimal quantity, decimal totalQuantity)
        {
            var lstResults = new List<string>();

            if (quantity == 0)
                lstResults.Add("Invalid quantity");

            if (shiptoLocationId <= 0)
                lstResults.Add("Invalid ShiptoLocationId");

            if (totalQuantity == 0)
                lstResults.Add("Invalid TotalQuantity");
            return lstResults;
        }

        public bool UpdateDocumentStatus(P2PDocumentType docType, long documentId, Documents.Entities.DocumentStatus documentStatus, decimal partnerCode, bool isInterface = false, POTransmissionMode poTransmissionMode = POTransmissionMode.None, string poTransmissionValue = "", string orderSource = "", int SourceSystemId = 0, bool isAutoCreatedOrder = false, bool resent = false, bool EmailBlockingforSupplier = false, bool IsAcknowledgeByBuyer = false, List<Comments> CommentList = null, bool IsBuyerInvoiceVisibleToSupplier = false)
        {
            bool result = false;
            try
            {
                var commonManager = new RequisitionCommonManager(JWTToken) { UserContext = this.UserContext, GepConfiguration = this.GepConfiguration };
                int maxPrecessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges;
                maxPrecessionValue = commonManager.GetPrecisionValue();
                maxPrecessionforTotal = commonManager.GetPrecisionValueforTotal();
                maxPrecessionForTaxesAndCharges = commonManager.GetPrecisionValueForTaxesAndCharges();
                bool IsAutoAcknowledgeDirectOrder = false;
                bool sendMailForNonIntegratedSupplier = false;
                string settingValue = string.Empty;


                settingValue = commonManager.GetSettingsValueByKey(P2PDocumentType.Interfaces, "IsAutoAcknowledgeDirectOrder", UserContext.ContactCode, (int)SubAppCodes.Interfaces);
                IsAutoAcknowledgeDirectOrder = !string.IsNullOrEmpty(settingValue) ? Convert.ToBoolean(settingValue) : false;
                settingValue = string.Empty;
                settingValue = commonManager.GetSettingsValueByKey(P2PDocumentType.Interfaces, "sendMailForNonIntegratedSupplier", UserContext.ContactCode, (int)SubAppCodes.Interfaces);
                sendMailForNonIntegratedSupplier = !string.IsNullOrEmpty(settingValue) ? Convert.ToBoolean(settingValue) : false;
                bool IsSupplierCurrencyList = getSupplierCurrencySetting();
                ProxyPartnerService partnerProxy = new ProxyPartnerService(UserContext, this.JWTToken);

                var objPartnerFinancialDetails = new PartnerFinancialDetails();
                int listPartnerInterfaceInfo = 0;

                result = GetReqDao().UpdateDocumentStatus(documentId, documentStatus, partnerCode);

                if (this.UserContext.Product != GEPSuite.eInterface)
                    if (docType != P2PDocumentType.None)
                        AddIntoSearchIndexerQueueing(documentId, (int)getDocumentType(docType));

                UpdateDocumentStatusInBudget(documentId, documentStatus);

            }
            catch (Exception ex)
            {
                // Log Exception here
                LogHelper.LogError(Log, "Error occurred in UpdateDocumentStatus method in RequisitionDocumentManager documentcode :- " + documentId, ex);
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw();
                throw;
            }
            return result;
        }


        public ICollection<string> ValidateDocumentByDocumentCode(P2PDocumentType docType, long documentCode, long LOBEntityDetailCode, bool returnResourceErrorMsgKey = false, bool isOnSubmit = false, int populateDefaultNeedByDateByDays = 0, bool EnablePastDateDocProcess = false)
        {
            var commonManager = new RequisitionCommonManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
            bool allowTaxCodewithAmount = Convert.ToBoolean(commonManager.GetSettingsValueByKey(P2PDocumentType.Invoice, "AllowTaxCodewithAmount", UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBEntityDetailCode));
            string supplierStatusForValidation = commonManager.GetSettingsValueByKey(P2PDocumentType.Invoice, "SupplierStatusForValidation", UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBEntityDetailCode);
            int receivedDateLimit = -1;
            if ((commonManager.GetSettingsValueByKey(P2PDocumentType.Receipt, "RestrictionForReceivingInPast", UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBEntityDetailCode)) != "")
            {
                receivedDateLimit = convertStringToInt(commonManager.GetSettingsValueByKey(P2PDocumentType.Receipt, "RestrictionForReceivingInPast", UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBEntityDetailCode));
            }
            EnablePastDateDocProcess = Convert.ToBoolean(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "EnablePastDateDocProcess", UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBEntityDetailCode).ToString());
            ICollection<string> returnValue = GetReqDao().ValidateDocumentByDocumentCode(documentCode, allowTaxCodewithAmount, supplierStatusForValidation, returnResourceErrorMsgKey, populateDefaultNeedByDateByDays, receivedDateLimit, EnablePastDateDocProcess);
            DocumentType dcType = getDocumentType(docType);
            if (dcType == DocumentType.Invoice && isOnSubmit)
            {
                var tempI = commonManager.ValidateItemCustomFields((int)dcType, documentCode, true, 0, true, true);
                if (tempI.Count > 0 && tempI[tempI.Count - 1].listQuestionIds.Count > 0)
                    returnValue.Add("Item level Custom Fields are missing.");
                var tempS = commonManager.ValidateSplitCustomFields((int)dcType, documentCode, true, 0, true, true);
                if (tempS.Count > 0 && tempS[tempS.Count - 1].listQuestionIds.Count > 0)
                    returnValue.Add("Split level Custom Fields are missing.");
            }

            //AddIntoSearchIndexerQueueing(documentCode, (int)getDocumentType(docType));
            return returnValue;
        }

        public bool CancelLineItem(P2PDocumentType docType, long itemId, int itemType = 0)
        {
            var commonManager = new RequisitionCommonManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
            int maxPrecessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges;
            maxPrecessionValue = commonManager.GetPrecisionValue();
            maxPrecessionforTotal = commonManager.GetPrecisionValueforTotal();
            maxPrecessionForTaxesAndCharges = commonManager.GetPrecisionValueForTaxesAndCharges();

            long documentCode = 0;

            documentCode = GetReqDao().CancelLineItem(itemId, DocumentStatus.Cancelled, itemType, maxPrecessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges);
            AddIntoSearchIndexerQueueing(documentCode, (int)getDocumentType(docType));

            return documentCode > 0;
        }

        #region 'Interface Methods'

        public DataTable ValidateInternalCatalogItems(P2PDocumentType docType, DataTable dtItems)
        {
            return GetCommonDao().ValidateInternalCatalogItems(dtItems);
        }

        public DataTable DeriveItemDetails(string itemNumber, string partnerSourceSystemValue, string uom)
        {
            return GetCommonDao().DeriveItemDetails(itemNumber, partnerSourceSystemValue, uom);
        }

        public void SaveFlippedQuestionResponses(long CatalogItemID, long ItemMasterItemId, long DocumentItemId, int tardocumentType, List<string> lstCustomAttributeQuestionText)
        {
            GetCommonDao().SaveFlippedQuestionResponses(CatalogItemID, ItemMasterItemId, DocumentItemId, tardocumentType, lstCustomAttributeQuestionText);
        }
        #endregion 'Interface Methods'

        public bool DeleteDocumentByDocumentCode(P2PDocumentType docType, long documentCode)
        {
            bool result = GetReqDao().DeleteDocumentByDocumentCode(documentCode);
            AddIntoSearchIndexerQueueing(documentCode, (int)getDocumentType(docType));
            if (result && docType == P2PDocumentType.Requisition)
            {
                BroadcastPusher(documentCode, DocumentType.Requisition, "DataStale", "DeleteRequisition");
            }

            return result;
        }

        public List<SplitAccountingFields> GetAllSplitAccountingFields(P2PDocumentType docType, LevelType levelType, int structureId = 0, long lobId = 0, long AEEntityDetailCode = 0)
        {
            string strKey = "AccountingFields" + docType.ToString() + levelType + "StructureId" + structureId + "lobId" + lobId + "AEEntityDetailCode" + AEEntityDetailCode;
            List<SplitAccountingFields> splitAccountingFields = GEPDataCache.GetFromCacheJSON<List<SplitAccountingFields>>(strKey, this.UserContext.BuyerPartnerCode, "en-US");
            if (splitAccountingFields == null)
            {
                try
                {
                    splitAccountingFields = GetSQLP2PDocumentDAO().GetAllSplitAccountingFields(docType, levelType, structureId, lobId, AEEntityDetailCode);
                }
                catch (Exception ex)
                {
                    LogHelper.LogError(Log, "Error occured in GetAllSplitAccountingFields Method in ManageCommonController", ex);
                    GEPDataCache.RemoveFromCache(strKey, this.UserContext.BuyerPartnerCode, "en-US");
                    throw;
                }
                GEPDataCache.PutInCacheJSON<List<SplitAccountingFields>>(strKey, this.UserContext.BuyerPartnerCode, "en-US", splitAccountingFields);
            }
            return splitAccountingFields;
        }

        public string SentDocumentForApproval(long contactCode, long documentCode, decimal documentAmount, int documentTypeId, decimal conversionFactor)

        {
            string serviceurl;
            Requisition objReq = null;
            Order objOrder = null;
            ProgramDetails objPG = null;
            bool isApproved = false;
            bool isUrgent = false;
            string result = string.Empty;
            List<OfflineApprovalDetails> offlineRulesList = new List<OfflineApprovalDetails>();

            try
            {
                List<RuleAction> actions = null;
                List<RuleAction> ruleactions = null;
                WorkflowInputEntities WorkflowIEobj = null;
                RequisitionCommonManager commonManager = new RequisitionCommonManager(JWTToken) { GepConfiguration = this.GepConfiguration, UserContext = this.UserContext };

                bool isUrgentApproval = false;
                bool hasRules = false;
                hasRules = GetSQLP2PDocumentDAO().CheckHasRules(BusinessCase.Approval, documentTypeId);

                if (documentTypeId == 7)
                {
                    objReq = (Requisition)GetReqDao().GetAllRequisitionDetailsByRequisitionId(documentCode, this.UserContext.ContactCode, 0);
                    string skippedApprovalSetting = commonManager.GetSettingsValueByKey(P2PDocumentType.Requisition, "EnableSkippedApproval", UserContext.ContactCode, 107, "", objReq.EntityDetailCode.FirstOrDefault());
                    bool isEnableSkippedApproval = false;
                    if (!(string.IsNullOrEmpty(skippedApprovalSetting)))
                    {
                        isEnableSkippedApproval = Convert.ToBoolean(skippedApprovalSetting);
                    }
                    if (objReq.IsUrgent && isEnableSkippedApproval)
                    {
                        isUrgentApproval = true;
                    }

                    if (objReq.RequisitionSource == RequisitionSource.ChangeRequisition)
                    {
                        string approvalEntityTypeIds = commonManager.GetSettingsValueByKey(P2PDocumentType.None, "AccountingEntityRelevantForApproval", UserContext.ContactCode, (int)SubAppCodes.P2P, "", objReq.EntityDetailCode.FirstOrDefault());
                        if (string.IsNullOrWhiteSpace(approvalEntityTypeIds))
                        {
                            approvalEntityTypeIds = "";
                        }
                        objReq.EntitySumDeltaList = GetCommonDao().GetDocumentEntitySumDeltaList(documentCode, approvalEntityTypeIds, objReq.PurchaseType);
                        GetReqDao().GetReqSplitItemsEntityChangeFlag(documentCode, approvalEntityTypeIds, ref objReq);
                    }
                    isUrgent = objReq.IsUrgent;
                    if (objReq.CustomAttrFormId > 0 || objReq.CustomAttrFormIdForItem > 0 || objReq.CustomAttrFormIdForSplit > 0)
                    {
                        objReq.ListQuestionResponse = GetQuestionResponse(documentTypeId, documentCode, objReq.CustomAttrFormId, objReq.CustomAttrFormIdForItem, objReq.CustomAttrFormIdForSplit, ((System.Collections.IEnumerable)objReq.RequisitionItems).Cast<object>().ToList(), true);
                        objReq.RequisitionItems.ForEach(items => items.ListQuestionResponse = objReq.ListQuestionResponse.Where(w => w.ObjectInstanceId == items.DocumentItemId).ToList());
                    }
                    objReq.lstUserQuestionResponse = GetUserQuestionresponse(objReq.LOBEntity, objReq.OnBehalfOf > 0 ? objReq.OnBehalfOf : UserContext.ContactCode);

                    if (objReq.DocumentStatusInfo != DocumentStatus.Approved)
                    {
                        SettingDetails invSettings = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.Invoice, UserContext.ContactCode, 107);
                        bool IsREOptimizationEnabled = Convert.ToBoolean(commonManager.GetSettingsValueByKey(invSettings, "IsREOptimizationEnabled"));

                        actions = GetCommonDao().EvaluateRulesForObject(BusinessCase.Approval, objReq, IsREOptimizationEnabled);
                        if (actions != null && actions.Count > 0 && actions.Where(e => e.Action != ActionType.InduceUserDefinedApproval) != null && actions.Where(e => e.Action != ActionType.InduceUserDefinedApproval).ToList().Count > 0)
                        {
                            ruleactions = UpdateActionParameters(actions.Where(e => e.Action != ActionType.InduceUserDefinedApproval).ToList(), contactCode, objReq, 1);
                        }
                        WorkflowIEobj = new WorkflowInputEntities();

                        if (ruleactions != null && ruleactions.Count > 0 && ruleactions.Where(e => e.Action != ActionType.InduceUserDefinedApproval) != null)
                            WorkflowIEobj.RuleAction = ruleactions.Where(e => e.Action != ActionType.InduceUserDefinedApproval).ToList();

                        bool IsRequisitionDeltaAmountEnabled = false;
                        string EnableDeltaApprovalsOnChangeRequisition = commonManager.GetSettingsValueByKey(P2PDocumentType.Requisition, "EnableDeltaApprovalsOnChangeRequisition", UserContext.ContactCode, (int)SubAppCodes.P2P, "", objReq.EntityDetailCode.FirstOrDefault());
                        if (!String.IsNullOrWhiteSpace(EnableDeltaApprovalsOnChangeRequisition) && EnableDeltaApprovalsOnChangeRequisition.Trim().ToUpper() == "YES")
                            IsRequisitionDeltaAmountEnabled = true;
                        if (objReq.RequisitionSource == RequisitionSource.ChangeRequisition && objReq.TotalAmountChange > 0 && IsRequisitionDeltaAmountEnabled)
                        {
                            documentAmount = objReq.TotalAmountChange;
                        }
                    }
                    else
                        isApproved = true;
                }


                if (actions != null && actions.Count > 0 && actions.Where(a => a.Action == ActionType.UserDefinedApproval).Any())
                {
                    if (documentTypeId == 7)
                        hasRules = InduceUserDefinedApprovals(documentCode, documentTypeId, BusinessCase.Approval, objReq, actions, true, isUrgent);
                    else if (documentTypeId == 8)
                        hasRules = InduceUserDefinedApprovals(documentCode, documentTypeId, BusinessCase.Approval, objOrder, actions, true, isUrgent);

                }
                if (actions != null && actions.Count > 0 && actions.Where(e => e.Action != ActionType.InduceUserDefinedApproval) != null && actions.Where(e => e.Action != ActionType.InduceUserDefinedApproval).ToList().Count > 0)
                {
                    offlineRulesList = GetCommonDao().GetRuleIdsMarkedOfflineForStaticApproval(documentCode, documentTypeId);

                    if (offlineRulesList.Count() > 0)
                    {
                        foreach (RuleAction action in actions.Where(e => e.Action != ActionType.InduceUserDefinedApproval).ToList())
                        {
                            int ruleId = 0;
                            List<RuleParameter> ruleParams = new List<RuleParameter>();
                            ruleId = action.RuleId;
                            if (offlineRulesList.Any(e => e.RuleId == ruleId))
                            {
                                //  updateRules = true;
                                string parameters = action.Parameters;
                                if (parameters.Contains("IsOfflineApproved") && parameters.Contains("OfflineApproverId"))
                                {
                                    //do later
                                }
                                else
                                {
                                    string ruleToBeAdded = ",{\"Id\":0,\"Name\":\"IsOfflineApproved\",\"Value\":\"true\"},{\"Id\":0,\"Name\":\"OfflineApproverId\",\"Value\":\"" + offlineRulesList.Where(e => e.RuleId == ruleId).FirstOrDefault().OfflineApproverId + "\"}]";
                                    parameters = parameters.Remove(parameters.Length - 1);
                                    parameters = string.Concat(parameters, ruleToBeAdded);
                                    action.Parameters = parameters;
                                }
                            }
                        }

                    }
                    if (WorkflowIEobj.RuleAction != null && WorkflowIEobj.RuleAction.Count > 0 && WorkflowIEobj.RuleAction.Where(e => e.Action != ActionType.InduceUserDefinedApproval) != null && WorkflowIEobj.RuleAction.Where(e => e.Action != ActionType.InduceUserDefinedApproval).ToList().Count == offlineRulesList.Count)
                    {
                        WorkflowIEobj.RuleAction.Add(new RuleAction
                        {
                            RuleId = 0,
                            CollectionId = 0,
                            Parameters = "[{\"Id\":0,\"Name\":\" \",\"Value\":\" \"}]",
                            RuleName = "",
                            CollectionName = "",
                            Action = ActionType.AutoApprove,
                            KeyResult = ""
                        });
                    }
                }

                if (!isApproved)
                {
                    serviceurl = MultiRegionConfig.GetConfig(CloudConfig.WorkFlowRestURL) + "/InvokeWorkFlow";
                    CreateHttpWebRequest(serviceurl);

                    Dictionary<string, object> odict = new Dictionary<string, object>();
                    odict.Add("contactCode", contactCode);
                    odict.Add("documentCode", documentCode);

                    odict.Add("documentAmount", documentAmount);
                    odict.Add("documentTypeId", documentTypeId);
                    odict.Add("eventName", "OnSubmit");
                    odict.Add("returnEntity", "");
                    odict.Add("isUrgentApproval", isUrgentApproval);

                    if ((documentTypeId == 7 || documentTypeId == 8 || documentTypeId == 9 || documentTypeId == 10 || documentTypeId == 27 || documentTypeId == 31) && actions != null)
                    {
                        odict.Add("wfInputParameter", WorkflowIEobj);
                    }
                    result = GetHttpWebResponse(odict);
                }
                return result;

            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in SentDocumentForApproval Method in RequisitionDocumentManager", ex);
            }
            return string.Empty;

        }

        public List<QuestionBankEntities.QuestionResponse> GetQuestionResponseDetails(List<Tuple<long, long>> lstFormCodeId)
        {
            var response = GetCommonDao().GetQuestionResponse(lstFormCodeId);
            return response;
        }

        public List<QuestionBankEntities.Question> GetQuestionResponseByFormCode(List<Tuple<long, long>> lstFormCodeId)
        {
            try
            {
                DataSet ds = GetCommonDao().GetQuestionDetailsByFormCode(lstFormCodeId);
                return GetQuestionDetailsByFormCode(ds);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in GetQuestionResponseByFormCode Method in RequisitionDocumentManager", ex);
            }
            return new List<QuestionBankEntities.Question>();
        }

        public List<QuestionBankEntities.Question> GetQuestionDetailsByFormCode(DataSet ds)
        {
            List<QuestionBankEntities.Question> lstquestion = new List<QuestionBankEntities.Question>();
            try
            {
                QuestionBankEntities.Question objquestion = new QuestionBankEntities.Question();
                if (ds?.Tables.Count > 0)
                {
                    foreach (DataRow question in ds.Tables[0]?.Rows)
                    {
                        var qQuestionId = Convert.ToInt64(question[SqlConstants.COL_QUESTIONID]);
                        var qObjectInstanceId = Convert.ToInt64(question[SqlConstants.COL_OBJECTINSTANCEID]);
                        var questionTypeId = Convert.ToInt32(question[SqlConstants.COL_QUESTIONTYPEID]);
                        objquestion = new QuestionBankEntities.Question();
                        switch (questionTypeId)
                        {
                            case 7:
                                QuestionBankEntities.MultipleChoiceQuestion objChoiceNewQuestion = new QuestionBankEntities.MultipleChoiceQuestion();
                                foreach (DataRow item2 in ds.Tables[2]?.Rows)
                                {
                                    long lQuestionId = Convert.ToInt64(item2[SqlConstants.COL_QUESTIONID]);
                                    long lObjectInstanceId = Convert.ToInt64(item2[SqlConstants.COL_OBJECTINSTANCEID]);
                                    if (qObjectInstanceId == lObjectInstanceId && (qQuestionId > 0 && lQuestionId > 0) && qQuestionId == lQuestionId)
                                    {
                                        objChoiceNewQuestion.QuestionId = Convert.ToInt64(item2[SqlConstants.COL_QUESTIONID]);
                                        objChoiceNewQuestion.RowChoices.Add(new QuestionBankEntities.QuestionRowChoice
                                        {
                                            QuestionId = Convert.ToInt64(item2[SqlConstants.COL_QUESTIONID]),
                                            RowId = Convert.ToInt64(item2[SqlConstants.COL_ROWID]),
                                            RowText = Convert.ToString(item2[SqlConstants.COL_ROWTEXT]),
                                            RowDescription = Convert.ToString(item2[SqlConstants.COL_ROWDESCRIPTION]),
                                            ChildQuestionSetCode = Convert.ToInt64(item2[SqlConstants.COL_CHILDQUESTIONSETCODE])
                                        });
                                        objChoiceNewQuestion.ListQuestionResponses.Add(new QuestionBankEntities.QuestionResponse
                                        {
                                            ResponseValue = Convert.ToString(item2[SqlConstants.COL_RESPONSEVALUE]),
                                            ObjectInstanceId = Convert.ToInt64(item2[SqlConstants.COL_OBJECTINSTANCEID]),
                                            RowId = Convert.ToInt64(item2[SqlConstants.COL_ROWID])
                                        });
                                    }
                                }
                                objquestion = objChoiceNewQuestion;
                                lstquestion.Add(objquestion);
                                break;
                            case 10:

                                foreach (DataRow item4 in ds.Tables[4]?.Rows)
                                {
                                    long lQId = Convert.ToInt64(item4[SqlConstants.COL_QUESTIONID]);
                                    long lObjectInstanceId = Convert.ToInt64(item4[SqlConstants.COL_OBJECTINSTANCEID]);
                                    if (qObjectInstanceId == lObjectInstanceId && (qQuestionId > 0 && lQId > 0) && qQuestionId == lQId)
                                    {
                                        QuestionBankEntities.DateTimeQuestion objDateTimeQuestion = new QuestionBankEntities.DateTimeQuestion
                                        {
                                            DateTimeType = (QuestionBankEntities.DateTimeType)(Convert.ToInt64(item4[SqlConstants.COL_DATETIMETYPE])),
                                            DateTimeFormat = (QuestionBankEntities.DateTimeFormat)(Convert.ToInt64(item4[SqlConstants.COL_DATETIMEFORMAT])),
                                            QuestionId = Convert.ToInt64(item4[SqlConstants.COL_QUESTIONID])
                                        };

                                        objDateTimeQuestion.ListQuestionResponses.Add(new QuestionBankEntities.QuestionResponse
                                        {
                                            ResponseValue = Convert.ToString(item4[SqlConstants.COL_RESPONSEVALUE]),
                                            ObjectInstanceId = Convert.ToInt64(item4[SqlConstants.COL_OBJECTINSTANCEID])
                                        });
                                        objquestion = objDateTimeQuestion;
                                        lstquestion.Add(objquestion);
                                    }
                                }
                                break;
                            case 16:
                                QuestionBankEntities.MultipleMatrixQuestion objMatrixNewQuestion = new QuestionBankEntities.MultipleMatrixQuestion();
                                foreach (DataRow item3 in ds.Tables[3]?.Rows)
                                {
                                    long lQId = Convert.ToInt64(item3[SqlConstants.COL_QUESTIONID]);
                                    long lQObjectInstanceId = Convert.ToInt64(item3[SqlConstants.COL_OBJECTINSTANCEID]);
                                    if (qObjectInstanceId == lQObjectInstanceId && (qQuestionId > 0 && lQId > 0) && qQuestionId == lQId)
                                    {
                                        objMatrixNewQuestion.QuestionId = Convert.ToInt64(item3[SqlConstants.COL_QUESTIONID]);
                                        objMatrixNewQuestion.ColumnChoices.Add(new QuestionBankEntities.QuestionColumnChoice
                                        {
                                            QuestionId = Convert.ToInt64(item3[SqlConstants.COL_QUESTIONID]),
                                            ColumnId = Convert.ToInt64(item3[SqlConstants.COL_COLUMNID]),
                                            ColumnText = Convert.ToString(item3[SqlConstants.COL_COLUMNTEXT]),
                                            ColumnDescription = Convert.ToString(item3[SqlConstants.COL_COLUMNDESCRIPTION]),
                                            ChildQuestionSetCode = Convert.ToInt64(item3[SqlConstants.COL_CHILDQUESTIONSETCODE]),
                                            ColumnType = (QuestionBankEntities.ColumnType)Convert.ToInt32(item3[SqlConstants.COL_COLUMNTYPE])
                                        });
                                        objMatrixNewQuestion.ListQuestionResponses.Add(new QuestionBankEntities.QuestionResponse
                                        {
                                            ResponseValue = Convert.ToString(item3[SqlConstants.COL_RESPONSEVALUE]),
                                            ObjectInstanceId = Convert.ToInt64(item3[SqlConstants.COL_OBJECTINSTANCEID]),
                                            ColumnId = Convert.ToInt64(item3[SqlConstants.COL_COLUMNID]),
                                            RowId = Convert.ToInt64(item3[SqlConstants.COL_ROWID])
                                        });
                                    }
                                }
                                //Column Choice List
                                if (ds.Tables[5] != null && ds.Tables[5].Rows.Count > 0)
                                {
                                    foreach (DataRow drColumnChoice in ds.Tables[5]?.Rows)
                                    {
                                        var objColChoices = objMatrixNewQuestion.ColumnChoices.Where(colchoice => colchoice.ColumnId == Convert.ToInt64(drColumnChoice[SqlConstants.COL_COLUMN_ID], System.Globalization.CultureInfo.InvariantCulture));
                                        if (objColChoices != null && objColChoices.Count() > 0)
                                        {
                                            foreach (var objColChoice in objColChoices)
                                            {
                                                objColChoice.ListMatrixCellChoices.Add(new QuestionBankEntities.MatrixCellChoice
                                                {
                                                    CellChoiceId = Convert.ToInt64(drColumnChoice[SqlConstants.COL_CELL_CHOICE_ID]),
                                                    ColumnId = Convert.ToInt64(drColumnChoice[SqlConstants.COL_COLUMN_ID]),
                                                    RowId = Convert.ToInt64(drColumnChoice[SqlConstants.COL_ROW_ID]),
                                                    ChoiceValue = Convert.ToString(drColumnChoice[SqlConstants.COL_CHOICE_VALUE]),
                                                    ChoiceScore = Convert.ToDouble(drColumnChoice[SqlConstants.COL_CHOICE_SCORE], CultureInfo.InvariantCulture),// GetDoubleValue(dr,Constants.COL_CHOICE_SCORE)
                                                    IsDefault = Convert.ToBoolean(drColumnChoice[SqlConstants.COL_IS_DEFAULT])
                                                });
                                            }
                                        }
                                    }
                                }
                                objquestion = objMatrixNewQuestion;
                                lstquestion.Add(objquestion);
                                break;
                            case 1:
                            case 2:
                            case 17:
                            case 28:
                                foreach (DataRow item1 in ds.Tables[1]?.Rows)
                                {
                                    long lQuestionId = Convert.ToInt64(item1[SqlConstants.COL_QUESTIONID]);
                                    long lObjectInstanceId = Convert.ToInt64(item1[SqlConstants.COL_OBJECTINSTANCEID]);
                                    if (qObjectInstanceId == lObjectInstanceId && (qQuestionId > 0 && lQuestionId > 0) && qQuestionId == lQuestionId)
                                    {
                                        objquestion = new QuestionBankEntities.Question();
                                        objquestion.QuestionId = Convert.ToInt64(item1[SqlConstants.COL_QUESTIONID]);
                                        objquestion.ListQuestionResponses.Add(new QuestionBankEntities.QuestionResponse
                                        {
                                            QuestionId = Convert.ToInt64(item1[SqlConstants.COL_QUESTIONID]),
                                            ResponseValue = Convert.ToString(item1[SqlConstants.COL_RESPONSEVALUE]),
                                            ObjectInstanceId = Convert.ToInt64(item1[SqlConstants.COL_OBJECTINSTANCEID]),
                                            AssesseeId = -1,
                                            AssessorId = -1,
                                            AssessorType = QuestionBankEntities.AssessorUserType.Buyer
                                        });
                                        lstquestion.Add(objquestion);
                                    }
                                }
                                break;
                        }
                    }

                    foreach (DataRow item in ds.Tables[0]?.Rows)
                    {
                        var qObjectInstanceId = Convert.ToInt64(item[SqlConstants.COL_OBJECTINSTANCEID]);
                        foreach (QuestionBankEntities.Question question in lstquestion)
                        {
                            var lstQuestion = question.ListQuestionResponses.Where(qr => qr.ObjectInstanceId == qObjectInstanceId).FirstOrDefault();

                            if (lstQuestion != null && (question.QuestionId > 0 && Convert.ToInt64(item[SqlConstants.COL_QUESTIONID]) > 0) && question.QuestionId == Convert.ToInt64(item[SqlConstants.COL_QUESTIONID]))
                            {
                                question.QuestionId = Convert.ToInt64(item[SqlConstants.COL_QUESTIONID]);
                                question.QuestionSetCode = Convert.ToInt64(item[SqlConstants.COL_QUESTIONNAIRECODE]);
                                question.QuestionSortOrder = Convert.ToInt32(item[SqlConstants.COL_SORTORDER]);
                                question.QuestionDescription = Convert.ToString(item[SqlConstants.COL_QUESTIONNAIREDESCRIPTION]);
                                question.QuestionText = Convert.ToString(item[SqlConstants.COL_QUESTIONTEXT]);
                                question.QuestionTypeInfo = new QuestionBankEntities.QuestionType { QuestionTypeId = Convert.ToInt32(item[SqlConstants.COL_QUESTIONTYPEID]) };

                            }
                        }
                    }
                }
                return lstquestion;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in GetQuestionDetailsByFormCode Method in RequisitionDocumentManager", ex);
            }
            finally
            {
            }
            return new List<QuestionBankEntities.Question>();
        }

        public List<QuestionBankEntities.QuestionResponse> GetQuestionResponse(int documentTypeId, long documentCode, long formIdHeader,
            long formIdItem, long formIdSplit, List<Object> objItems, bool isDataValidation = false)
        {
            List<QuestionBankEntities.QuestionResponse> lstQuestionsResponse = new List<QuestionBankEntities.QuestionResponse>();
            List<QuestionBankEntities.Question> headerQuestionSet = new List<QuestionBankEntities.Question>();
            List<QuestionBankEntities.Question> itemQuestionSet = new List<QuestionBankEntities.Question>();
            List<QuestionBankEntities.Question> splitQuestionSet = new List<QuestionBankEntities.Question>();
            RequisitionCommonManager commonManager = new RequisitionCommonManager(JWTToken) { GepConfiguration = this.GepConfiguration, UserContext = this.UserContext };

            #region Custom Attributes                  

            // Custom Attributes -- Get Question Set for Header, Item and Split
            if (formIdHeader > 0)
                headerQuestionSet = GetCommonDao().GetQuestionSetByFormCode(formIdHeader);

            if (formIdItem > 0)
                itemQuestionSet = GetCommonDao().GetQuestionSetByFormCode(formIdItem);

            if (formIdSplit > 0)
                splitQuestionSet = GetCommonDao().GetQuestionSetByFormCode(formIdSplit);


            // Custom Attributes -- Fill Questions Response List -- for Header
            List<Questionnaire> lstCustAttrQuestionnaire = new List<Questionnaire>();
            commonManager.GetQuestionWithResponse(headerQuestionSet.Select(questSetCode => questSetCode.QuestionSetCode).ToList<long>(), lstCustAttrQuestionnaire, documentCode);
            if (headerQuestionSet != null && headerQuestionSet.Any() && lstCustAttrQuestionnaire != null)
                commonManager.FillQuestionsResponseList(lstQuestionsResponse, lstCustAttrQuestionnaire, headerQuestionSet.Select(question => question.QuestionSetCode).ToList(), documentCode, isDataValidation);

            //  Custom Attributes -- Fill Questions Response List -- for Item
            if (objItems != null && objItems.Count > 0 && itemQuestionSet != null && itemQuestionSet.Any())
            {
                if (documentTypeId == 7)
                {
                    foreach (RequisitionItem item in objItems)
                    {
                        if (item.CustomAttributes != null)
                            commonManager.FillQuestionsResponseList(lstQuestionsResponse, item.CustomAttributes, itemQuestionSet.Select(question => question.QuestionSetCode).ToList(), item.DocumentItemId, isDataValidation);
                        else
                        {
                            item.CustomAttributes = new List<Questionnaire>();
                            commonManager.GetQuestionWithResponse(itemQuestionSet.Select(questSetCode => questSetCode.QuestionSetCode).ToList<long>(), item.CustomAttributes, item.DocumentItemId);
                            commonManager.FillQuestionsResponseList(lstQuestionsResponse, item.CustomAttributes, itemQuestionSet.Select(question => question.QuestionSetCode).ToList(), item.DocumentItemId, isDataValidation);
                        }
                        //  Custom Attributes -- Fill Questions Response List -- For Split
                        if (item.ItemSplitsDetail != null && item.ItemSplitsDetail.Count > 0 && splitQuestionSet != null && splitQuestionSet.Any())
                        {
                            foreach (RequisitionSplitItems itmSplt in item.ItemSplitsDetail)
                                if (itmSplt.CustomAttributes != null)
                                    commonManager.FillQuestionsResponseList(lstQuestionsResponse, itmSplt.CustomAttributes, splitQuestionSet.Select(question => question.QuestionSetCode).ToList(), itmSplt.DocumentSplitItemId, isDataValidation);
                                else
                                {
                                    itmSplt.CustomAttributes = new List<Questionnaire>();
                                    commonManager.GetQuestionWithResponse(splitQuestionSet.Select(questSetCode => questSetCode.QuestionSetCode).ToList<long>(), itmSplt.CustomAttributes, itmSplt.DocumentSplitItemId);
                                    commonManager.FillQuestionsResponseList(lstQuestionsResponse, itmSplt.CustomAttributes, splitQuestionSet.Select(question => question.QuestionSetCode).ToList(), itmSplt.DocumentSplitItemId, isDataValidation);
                                }
                        }

                    }
                }
                else if (documentTypeId == 8)
                {
                    foreach (OrderItem item in objItems)
                    {
                        if (item.CustomAttributes != null)
                            commonManager.FillQuestionsResponseList(lstQuestionsResponse, item.CustomAttributes, itemQuestionSet.Select(question => question.QuestionSetCode).ToList(), item.DocumentItemId, isDataValidation);
                        else
                        {
                            item.CustomAttributes = new List<Questionnaire>();
                            commonManager.GetQuestionWithResponse(itemQuestionSet.Select(questSetCode => questSetCode.QuestionSetCode).ToList<long>(), item.CustomAttributes, item.DocumentItemId);
                            commonManager.FillQuestionsResponseList(lstQuestionsResponse, item.CustomAttributes, itemQuestionSet.Select(question => question.QuestionSetCode).ToList(), item.DocumentItemId, isDataValidation);
                        }
                        //  Custom Attributes -- Fill Questions Response List -- For Split
                        if (item.ItemSplitsDetail != null && item.ItemSplitsDetail.Count > 0 && splitQuestionSet != null && splitQuestionSet.Any())
                        {
                            foreach (OrderSplitItems itmSplt in item.ItemSplitsDetail)
                                if (itmSplt.CustomAttributes != null)
                                    commonManager.FillQuestionsResponseList(lstQuestionsResponse, itmSplt.CustomAttributes, splitQuestionSet.Select(question => question.QuestionSetCode).ToList(), itmSplt.DocumentSplitItemId, isDataValidation);
                                else
                                {
                                    itmSplt.CustomAttributes = new List<Questionnaire>();
                                    commonManager.GetQuestionWithResponse(splitQuestionSet.Select(questSetCode => questSetCode.QuestionSetCode).ToList<long>(), itmSplt.CustomAttributes, itmSplt.DocumentSplitItemId);
                                    commonManager.FillQuestionsResponseList(lstQuestionsResponse, itmSplt.CustomAttributes, splitQuestionSet.Select(question => question.QuestionSetCode).ToList(), itmSplt.DocumentSplitItemId, isDataValidation);
                                }
                        }

                    }
                }

            }

            // Custom Attributes -- Save Questions Response List -- For Header, Itemm and Split
            if (lstQuestionsResponse != null && lstQuestionsResponse.Any())
                return lstQuestionsResponse;
            #endregion
            return new List<QuestionBankEntities.QuestionResponse>();
        }


        public List<QuestionBankEntities.Question> GetUserQuestionresponse(long LobId, long OBO)
        {
            RequisitionCommonManager commonManager = new RequisitionCommonManager(JWTToken) { GepConfiguration = this.GepConfiguration, UserContext = this.UserContext };
            List<QuestionBankEntities.Question> lstUserQuestionResponse = new List<QuestionBankEntities.Question>();
            SettingDetails objSettingDetails = GEPDataCache.GetFromCacheJSON<SettingDetails>(String.Concat("REQ_UserManager", "_", LobId, "_", OBO), UserContext.BuyerPartnerCode, "en-US");

            try
            {
                if (objSettingDetails == null)
                {
                    var settingsHelper = new RESTAPIHelper.SettingsHelper(UserContext, JWTToken);
                    var requestBody = new
                    {
                        SubAppCode = (int)SubAppCodes.Portal,
                        ObjectType = "GEP.Cumulus.Portal.UserManager",
                        ContactCode = UserContext.ContactCode,
                        LOBId = LobId
                    };
                    objSettingDetails = settingsHelper.GetFeatureSettings(requestBody);

                    if (objSettingDetails != null && objSettingDetails.lstSettings.Count > 0)
                    {
                        GEPDataCache.PutInCacheJSON<SettingDetails>(String.Concat("REQ_UserManager", "_", LobId, "_", OBO), UserContext.BuyerPartnerCode, "en-US", objSettingDetails);
                    }
                }
            }
            catch
            {
                GEPDataCache.RemoveFromCache(String.Concat("REQ_UserManager", "_", LobId, "_", OBO), UserContext.BuyerPartnerCode, "en-US");
                throw;

            }

            if (objSettingDetails != null && objSettingDetails.lstSettings.Count > 0)
            {
                string UserQuestionSetCode = "";
                var UserQuestionSetdata = objSettingDetails.lstSettings.FirstOrDefault(s => s.FieldName == "QustionSetCode");
                if (null != UserQuestionSetCode)
                    UserQuestionSetCode = UserQuestionSetdata.FieldValue;

                if (!string.IsNullOrEmpty(UserQuestionSetCode))
                    lstUserQuestionResponse = commonManager.GetQuestionWithResponsesByQuestionSetPaging(new QuestionBankEntities.Question() { QuestionSetCode = Convert.ToInt64(UserQuestionSetCode.Split(',')[0]) }, OBO, OBO, OBO, QuestionBankEntities.AssessorUserType.Buyer, false);

            }
            return lstUserQuestionResponse;
        }

        public bool InduceUserDefinedApprovals(long documentCode, int documentTypeId, BusinessCase businessCase, P2PDocument docObj = null, List<RuleAction> actions = null, bool isFromSubmit = false, bool isUrgent = false)
        {
            #region Initialization

            List<RuleAction> ruleActions = new List<RuleAction>();
            Requisition objReq = new Requisition();
            Order objOrder = new Order();

            bool result = false;
            bool HasRuleId = false;
            int wfDocTypeId = documentTypeId;
            decimal documentAmount = 0;
            string currency = "USD";
            P2PDocument docobj = null;
            #endregion
            RequisitionCommonManager commonManager = new RequisitionCommonManager(JWTToken) { GepConfiguration = this.GepConfiguration, UserContext = this.UserContext };
            SettingDetails commonSetting = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.None, UserContext.ContactCode, 107);
            string strEnablePureAdHocApproval = commonManager.GetSettingsValueByKey(commonSetting, "EnableAdhocApprovalOnly");
            if (!string.IsNullOrEmpty(strEnablePureAdHocApproval) && Convert.ToBoolean(strEnablePureAdHocApproval))
                return result;
            else
            {
                SettingDetails invSettings = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.Invoice, UserContext.ContactCode, 107);
                bool IsREOptimizationEnabled = Convert.ToBoolean(commonManager.GetSettingsValueByKey(invSettings, "IsREOptimizationEnabled"));
                HasRuleId = GetCommonDao().CheckHasRuleForUserDefinedApprover(documentCode, documentTypeId);

                if (!HasRuleId)
                {
                    if (docObj == null)
                    {
                        if (!HasRuleId)
                        {
                            string customValidationMessage = string.Empty;

                            if (documentTypeId == 7)
                            {
                                wfDocTypeId = 7;
                                objReq = (Requisition)GetReqDao().GetAllRequisitionDetailsByRequisitionId(documentCode, this.UserContext.ContactCode, 0);
                                if (objReq.RequisitionSource == RequisitionSource.ChangeRequisition)
                                {
                                    string approvalEntityTypeIds = commonManager.GetSettingsValueByKey(commonSetting, "AccountingEntityRelevantForApproval");
                                    if (string.IsNullOrWhiteSpace(approvalEntityTypeIds))
                                    {
                                        approvalEntityTypeIds = "";
                                    }
                                    objReq.EntitySumDeltaList = GetCommonDao().GetDocumentEntitySumDeltaList(documentCode, approvalEntityTypeIds, objReq.PurchaseType);
                                    GetReqDao().GetReqSplitItemsEntityChangeFlag(documentCode, approvalEntityTypeIds, ref objReq);
                                }
                                isUrgent = objReq.IsUrgent;
                                documentAmount = objReq.RequisitionAmount;
                                currency = objReq.Currency;

                                if (objReq.CustomAttrFormId > 0 || objReq.CustomAttrFormIdForItem > 0 || objReq.CustomAttrFormIdForSplit > 0)
                                {
                                    var lstItemIds = objReq.RequisitionItems.Select(item => item.DocumentItemId).ToList();
                                    List<Tuple<long, long>> lstFormCodeId = new List<Tuple<long, long>>();
                                    if (objReq.CustomAttrFormId > 0)
                                        lstFormCodeId.Add(new Tuple<long, long>(documentCode, objReq.CustomAttrFormId));
                                    foreach (var item in objReq.RequisitionItems)
                                    {
                                        if (objReq.CustomAttrFormIdForItem > 0)
                                            lstFormCodeId.Add(new Tuple<long, long>(item.DocumentItemId, objReq.CustomAttrFormIdForItem));

                                        if (objReq.CustomAttrFormIdForSplit > 0)
                                        {
                                            foreach (var split in item.ItemSplitsDetail)
                                            {
                                                lstFormCodeId.Add(new Tuple<long, long>(split.DocumentSplitItemId, objReq.CustomAttrFormIdForSplit));
                                            }
                                        }
                                    }
                                    objReq.ListQuestionResponse = GetQuestionResponseDetails(lstFormCodeId);
                                    objReq.RequisitionItems.ForEach(items => items.ListQuestionResponse = objReq.ListQuestionResponse.Where(w => w.ObjectInstanceId == items.DocumentItemId).ToList());
                                    docobj = objReq;
                                }
                                objReq.lstUserQuestionResponse = GetUserQuestionresponse(objReq.LOBEntity, objReq.OnBehalfOf > 0 ? objReq.OnBehalfOf : UserContext.ContactCode);

                                actions = GetCommonDao().EvaluateRulesForObject(businessCase, objReq, IsREOptimizationEnabled);
                            }
                        }
                    }
                    if (actions != null && actions.Count > 0 && actions.Any(x => x.Action == ActionType.InduceUserDefinedApproval || x.Action == ActionType.UserDefinedApproval))
                    {
                        result = this.SaveUserDefinedApprovers(documentCode, wfDocTypeId, actions.Where(x => x.Action == ActionType.InduceUserDefinedApproval || x.Action == ActionType.UserDefinedApproval).ToList(), isUrgent, docObj == null ? docobj : docObj);
                    }
                }

                else if (isUrgent && isFromSubmit)
                {
                    UpdateUserDefinedApproversForUrgent(documentCode, wfDocTypeId, documentAmount, currency);
                }

                result = HasRuleId ? true : result;

                return result;
            }
        }

        public void UpdateUserDefinedApproversForUrgent(long documentCode, int wfDocTypeId, decimal documentAmount, string currency)
        {
            string instanceId = "";
            try
            {
                string serviceurl;
                serviceurl = UrlHelperExtensions.WorkFlowRestUrl + "/GetDocumentInstanceIdActive";
                CreateHttpWebRequest(serviceurl);

                Dictionary<string, object> odict = new Dictionary<string, object>();
                odict.Add("documentCode", documentCode);
                odict.Add("wfDocTypeId", wfDocTypeId);
                instanceId = JSONHelper.DeserializeObj<string>(GetHttpWebResponse(odict));
                if (instanceId != "")
                {
                    try
                    {
                        List<UserDefinedApproval> lstUserDefinedApproval = this.GetUserDefinedApproversList(documentCode, wfDocTypeId);
                        if (lstUserDefinedApproval != null && lstUserDefinedApproval.Count() > 0)
                        {
                            lstUserDefinedApproval.ForEach(k => k.ApproverDetails = new List<ApproverDetails>());
                            udpateApprovalUrgent(lstUserDefinedApproval);
                            try
                            {
                                this.UpdateUserDefinedApprovers("", 0, documentCode, wfDocTypeId, lstUserDefinedApproval);
                            }
                            catch (Exception ex)
                            {
                                LogHelper.LogError(Log, "Error occured in SaveAllApprovalDetails Method in RequisitionDocumentManager", ex);
                                throw;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.LogError(Log, "Error occured in GetAllApprovalDetails Method in RequisitionDocumentManager", ex);
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in UpdateUserDefinedApproversForUrgent Method in RequisitionDocumentManager", ex);
                throw;
            }
        }

        private void udpateApprovalUrgent(List<UserDefinedApproval> userDefinedApproval)
        {
            var hieraries = userDefinedApproval.OrderBy(x => x.HierarchyId).GroupBy(x => x.HierarchyId);
            if (hieraries.Count() > 0)
            {
                foreach (var hierary in hieraries)
                {
                    hierary.OrderBy(x => x.WorkflowOrder).GroupBy(x => x.WorkflowOrder);
                    if (hierary.Count() > 0)
                    {
                        hierary.Where(x => x.WorkflowOrder < hierary.Max(k => k.WorkflowOrder) && !x.IsSkipped && !x.IsOfflineApproved
                                       && !(x.IsActive && x.IsProcessed)).Select(x =>
                                       {
                                           x.IsSkipped = true;
                                           x.SkipType = Enums.WFSkipType.UrgentSkip;
                                           x.SkipDate = DateTime.Now;
                                           x.SkipActionerId = UserContext.ContactCode;
                                           return x;
                                       }).ToList();
                    }
                }
            }
        }

        

        private bool SaveUserDefinedApprovers(long documentCode, int wfDocTypeId, List<RuleAction> actions, bool isUrgent = false, P2PDocument docObj = null)
        {
            #region Initialization
            List<UserDefinedApproval> lstUserDefinedApprovers = new List<UserDefinedApproval>();
            bool result = false;
            #endregion
            var UserDefinedApprovalActions = actions.First(approval => approval.Action == ActionType.UserDefinedApproval);
            var approvalParam = JsonConvert.DeserializeObject<List<ParameterOutput>>(UserDefinedApprovalActions.Parameters);
            var approvalSources = approvalParam.First(k => k.Name == "ApproverMatrixSource").Value.Split(',').Distinct();
            foreach (var approvalSource in approvalSources)
            {
                switch (approvalSource)
                {
                    case "Rules":
                        GetUserDefinedApproversForRules(documentCode, wfDocTypeId, actions, lstUserDefinedApprovers);
                        break;
                    case "OrganizationManager":
                        GetUserDefinedApproversForOrgManagers(documentCode, wfDocTypeId, docObj, lstUserDefinedApprovers, approvalParam);
                        break;
                }
            }
            if (lstUserDefinedApprovers.Count > 0)
            {
                if (isUrgent)
                {
                    udpateApprovalUrgent(lstUserDefinedApprovers);
                }
                try
                {
                    string serviceurl;
                    serviceurl = UrlHelperExtensions.WorkFlowRestUrl + "/UpdateUserDefinedApprovers";
                    CreateHttpWebRequest(serviceurl, "PATCH");

                    Dictionary<string, object> odict = new Dictionary<string, object>();
                    odict.Add("instanceId", string.Empty);
                    odict.Add("wfOrderId", 0);
                    odict.Add("documentCode", documentCode);
                    odict.Add("wfDocTypeId", wfDocTypeId);
                    odict.Add("userDefinedApproval", lstUserDefinedApprovers);

                    string Result = ((Dictionary<string, object>)JSONHelper.DeserializeObject(GetHttpWebResponse(odict)))["UpdateUserDefinedApproversResult"].ToString();
                    if (Result.ToLower() == "true")
                    {
                        result = true;
                    }
                }
                catch (CommunicationException ex)
                {
                    LogHelper.LogError(Log, "Error occurred in SaveUserDefinedApprovers Method in RequisitionDocumentManager", ex);
                }
                catch (Exception ex)
                {
                    LogHelper.LogError(Log, "Error occured in SaveUserDefinedApprovers Method in RequisitionDocumentManager", ex);
                    throw;
                }
            }
            return result;
        }

        private static void GetUserDefinedApproversForRules(long documentCode, int wfDocTypeId, List<RuleAction> actions, List<UserDefinedApproval> lstUserDefinedApprovers)
        {
            UserDefinedApproval userdefinedapproval = new UserDefinedApproval();
            foreach (RuleAction action in actions.Where(k => k.Action == ActionType.InduceUserDefinedApproval))
            {
                List<ParameterOutput> parameterOutput = new List<ParameterOutput>();
                if (actions != null && actions.Count > 0)
                {
                    parameterOutput = JsonConvert.DeserializeObject<List<ParameterOutput>>(action.Parameters);
                }

                userdefinedapproval = new UserDefinedApproval();

                userdefinedapproval.WFOrderId = 0;
                userdefinedapproval.CollectionId = action.CollectionId;
                userdefinedapproval.CollectionName = action.CollectionName;
                userdefinedapproval.DocumentCode = documentCode;
                userdefinedapproval.IsActive = false;
                userdefinedapproval.IsDeleted = false;
                userdefinedapproval.IsProcessed = false;

                userdefinedapproval.RuleId = action.RuleId;
                userdefinedapproval.RuleName = action.RuleName;
                userdefinedapproval.WFDocTypeId = wfDocTypeId;
                userdefinedapproval.WFSubOrderId = 0;
                userdefinedapproval.WorkflowId = Convert.ToInt32(parameterOutput.Where(e => e.Name.ToLower() == "workflowid").DefaultIfEmpty(new ParameterOutput() { Value = "12" }).First().Value);
                userdefinedapproval.SkipType = Enums.WFSkipType.None;
                userdefinedapproval.IsSkipped = false;
                userdefinedapproval.SkipActionerId = 0;
                userdefinedapproval.PoolType = Convert.ToInt32(parameterOutput.Where(e => e.Name.ToLower() == "pooltype").DefaultIfEmpty(new ParameterOutput() { Value = "0" }).First().Value);
                userdefinedapproval.PoolTypeValue = Convert.ToDecimal(parameterOutput.Where(e => e.Name.ToLower() == "pooltypevalue").DefaultIfEmpty(new ParameterOutput() { Value = "0" }).First().Value);
                userdefinedapproval.RejectionPoolType = Convert.ToInt32(parameterOutput.Where(e => e.Name.ToLower() == "rejectionpooltype").DefaultIfEmpty(new ParameterOutput() { Value = "1" }).First().Value);
                userdefinedapproval.RejectionPoolTypeValue = Convert.ToDecimal(parameterOutput.Where(e => e.Name.ToLower() == "rejectionpooltypevalue").DefaultIfEmpty(new ParameterOutput() { Value = "1" }).First().Value);
                userdefinedapproval.EntityTypeId = Convert.ToInt32(parameterOutput.Where(e => e.Name.ToLower() == "entitytypeid").DefaultIfEmpty(new ParameterOutput() { Value = "7" }).First().Value);
                userdefinedapproval.IsOfflineApproved = Convert.ToBoolean(parameterOutput.Where(e => e.Name.ToLower() == "isofflineapproved").DefaultIfEmpty(new ParameterOutput() { Value = "False" }).First().Value);
                userdefinedapproval.OfflineApproverId = Convert.ToInt64(parameterOutput.Where(e => e.Name.ToLower() == "offlineapproverid").DefaultIfEmpty(new ParameterOutput() { Value = "0" }).First().Value);
                userdefinedapproval.HierarchyId = Convert.ToInt32(parameterOutput.Where(e => e.Name.ToLower() == "hierarchyid").DefaultIfEmpty(new ParameterOutput() { Value = "1" }).First().Value);
                userdefinedapproval.HierarchyName = parameterOutput.Where(e => e.Name.ToLower() == "hierarchyname").DefaultIfEmpty(new ParameterOutput() { Value = string.Empty }).First().Value;
                userdefinedapproval.ParentHierarchyId = Convert.ToInt32(parameterOutput.Where(e => e.Name.ToLower() == "parenthierarchyid").DefaultIfEmpty(new ParameterOutput() { Value = "0" }).First().Value);
                userdefinedapproval.IsVisibilityRestricted = Convert.ToBoolean(parameterOutput.Where(e => e.Name.ToLower() == "isvisibilityrestricted").DefaultIfEmpty(new ParameterOutput() { Value = "False" }).First().Value);

                if (lstUserDefinedApprovers.Any(k => k.HierarchyId == userdefinedapproval.HierarchyId))
                {
                    userdefinedapproval.WorkflowOrder = lstUserDefinedApprovers.Where(k => k.HierarchyId == userdefinedapproval.HierarchyId).Max(k => k.WorkflowOrder) + 1;
                }
                else
                    userdefinedapproval.WorkflowOrder = 0;

                userdefinedapproval.WorkflowSettings = new List<WorkFlowSettings>();

                foreach (var parameter in parameterOutput)
                {
                    userdefinedapproval.WorkflowSettings.Add(new WorkFlowSettings()
                    {
                        SettingName = parameter.Name,
                        SettingValue = parameter.Value
                    });
                }

                lstUserDefinedApprovers.Add(userdefinedapproval);
            }
        }

        private void GetUserDefinedApproversForOrgManagers(long documentCode, int wfDocTypeId, P2PDocument docObj, List<UserDefinedApproval> lstUserDefinedApprovers, List<ParameterOutput> approvalParam)
        {
            RequisitionCommonManager commonManager = new RequisitionCommonManager(JWTToken) { GepConfiguration = this.GepConfiguration, UserContext = this.UserContext };
            var IsApprovalBasedonOverallTotal = "false";
            var isThresholdAmtBasedOnOverallTotal = false;
            bool isDeltaPositive = false;
            bool IsBlanketAsPurchaseType = false;
            UserDefinedApproval userdefinedapproval = new UserDefinedApproval();
            UserDefinedApproval userdefinedLeadapproval = new UserDefinedApproval();

            if (approvalParam.Any(e => e.Name.ToLower() == "structureid"))
            {
                int purchaseType = 1;
                List<string> structureid = approvalParam.First(e => e.Name.ToLower() == "structureid").Value.Split(',').ToList();
                List<OrgManagerStructureMapping> mappingDetails = commonManager.GetOrgManagerStructureMappings(structureid);
                P2PDocument parentdoc = null;
                foreach (var mapping in mappingDetails)
                {
                    List<OrgManagerDetails> lstorgManagerDetails = new List<OrgManagerDetails>();
                    List<OrgManagerDetails> lstLeadApproverDetails = new List<OrgManagerDetails>();
                    List<EntityCominationSumCalculation> entitySum = this.GetEntitySumBasedOnDocumentProperty(docObj, wfDocTypeId, "OrgManager", mapping.EntityMapping.Select(p => p.Key).ToList());
                    List<EntityCominationSumCalculation> totalentitysum = null;
                    if (docObj.DocumentType == P2PDocumentType.Requisition)
                    {
                        Requisition req = (Requisition)docObj;
                        purchaseType = req.PurchaseType;
                        string EnableDeltaApprovalsOnChangeRequisition = commonManager.GetSettingsValueByKey(P2PDocumentType.Requisition, "EnableDeltaApprovalsOnChangeRequisition", UserContext.ContactCode, (int)SubAppCodes.P2P, "", req.EntityDetailCode.FirstOrDefault());
                        if (req.RequisitionSource == RequisitionSource.ChangeRequisition && !String.IsNullOrWhiteSpace(EnableDeltaApprovalsOnChangeRequisition) && EnableDeltaApprovalsOnChangeRequisition.Trim().ToUpper() == "YES")
                        {
                            parentdoc = GetReqDao().GetAllRequisitionDetailsByRequisitionId(req.ParentDocumentCode, this.UserContext.ContactCode, 0);
                        }
                    }
                    if (parentdoc != null)
                    {
                        List<EntityCominationSumCalculation> parententitySum = this.GetEntitySumBasedOnDocumentProperty(parentdoc, wfDocTypeId, "OrgManager", mapping.EntityMapping.Select(p => p.Key).ToList());
                        if (isDeltaPositive)
                            totalentitysum = this.GetEntitySumBasedOnDocumentProperty(docObj, wfDocTypeId, "OrgManager", mapping.EntityMapping.Select(p => p.Key).ToList());
                        foreach (var entity in entitySum)
                        {
                            if (parententitySum.Any(parEnt => JsonConvert.SerializeObject(parEnt.EntityCombinations) == JsonConvert.SerializeObject(entity.EntityCombinations)))
                            {
                                entity.TotalAmount = entity.TotalAmount - parententitySum.First(parEnt => JsonConvert.SerializeObject(parEnt.EntityCombinations) == JsonConvert.SerializeObject(entity.EntityCombinations)).TotalAmount;
                                entity.OverallLimitSplitTotal = entity.OverallLimitSplitTotal - parententitySum.First(parEnt => JsonConvert.SerializeObject(parEnt.EntityCombinations) == JsonConvert.SerializeObject(entity.EntityCombinations)).OverallLimitSplitTotal;
                            }
                        }
                    }

                    switch (docObj.DocumentType)
                    {
                        case P2PDocumentType.Order:
                            isThresholdAmtBasedOnOverallTotal = Convert.ToBoolean(!string.IsNullOrWhiteSpace(IsApprovalBasedonOverallTotal) && IsApprovalBasedonOverallTotal.Trim().ToUpper() == "TRUE" && IsBlanketAsPurchaseType);
                            break;
                        case P2PDocumentType.Requisition:
                            isThresholdAmtBasedOnOverallTotal = (purchaseType == 2) ? true : false;
                            break;
                        default:
                            isThresholdAmtBasedOnOverallTotal = false;
                            break;
                    }

                    foreach (var entity in entitySum.Where(k => k.TotalAmount > 0 || k.OverallLimitSplitTotal > 0))
                    {
                        OrgManagerDetails orgManagerDetails = new OrgManagerDetails();
                        orgManagerDetails.StructureId = mapping.StructureId;
                        orgManagerDetails.StructureName = mapping.StructureName;
                        if (isThresholdAmtBasedOnOverallTotal)
                        {
                            if ((decimal)entity.OverallLimitSplitTotal <= 0)
                                continue;
                            if (totalentitysum != null)
                            {
                                var totalCOoverall = totalentitysum.First(totEnt => JsonConvert.SerializeObject(totEnt.EntityCombinations) == JsonConvert.SerializeObject(entity.EntityCombinations));
                                if ((totalCOoverall != null) && (totalCOoverall.OverallLimitSplitTotal > 0))
                                    orgManagerDetails.ThresholdAmt = (decimal)totalCOoverall.OverallLimitSplitTotal;
                                else
                                    continue;
                            }
                            else
                                orgManagerDetails.ThresholdAmt = (decimal)entity.OverallLimitSplitTotal;
                        }
                        else
                        {
                            if ((decimal)entity.TotalAmount <= 0)
                                continue;
                            if (totalentitysum != null)
                            {
                                var totalCOAmt = totalentitysum.First(totEnt => JsonConvert.SerializeObject(totEnt.EntityCombinations) == JsonConvert.SerializeObject(entity.EntityCombinations));
                                if ((totalCOAmt != null) && (totalCOAmt.TotalAmount > 0))
                                    orgManagerDetails.ThresholdAmt = (decimal)totalCOAmt.TotalAmount;
                                else
                                    continue;
                            }
                            else
                                orgManagerDetails.ThresholdAmt = (decimal)entity.TotalAmount;
                        }
                        orgManagerDetails.HierarchyName = mapping.StructureName + "-";
                        foreach (var a in entity.EntityCombinations)
                        {
                            var mappingId = mapping.EntityMapping.First(k => k.Key == a.EntityTypeId).Value;
                            switch (mappingId)
                            {
                                case 1:
                                    orgManagerDetails.ORGEntityCode1 = a.EntityDetailCode;
                                    break;
                                case 2:
                                    orgManagerDetails.ORGEntityCode2 = a.EntityDetailCode;
                                    break;
                                case 3:
                                    orgManagerDetails.ORGEntityCode3 = a.EntityDetailCode;
                                    break;
                                case 4:
                                    orgManagerDetails.ORGEntityCode4 = a.EntityDetailCode;
                                    break;
                                case 5:
                                    orgManagerDetails.ORGEntityCode5 = a.EntityDetailCode;
                                    break;
                                case 6:
                                    orgManagerDetails.ORGEntityCode6 = a.EntityDetailCode;
                                    break;
                                case 7:
                                    orgManagerDetails.ORGEntityCode7 = a.EntityDetailCode;
                                    break;
                            }
                            orgManagerDetails.HierarchyName = orgManagerDetails.HierarchyName + a.EntityCode + "|";
                        }
                        lstorgManagerDetails.Add(orgManagerDetails);
                    }

                    var lstorgManagerDetailsTemp = lstorgManagerDetails;

                    lstorgManagerDetails = this.GetOrgManagerDetailsBasedOnEntities(lstorgManagerDetails.Where(p => p.ORGEntityCode1 != 0
                    || p.ORGEntityCode2 != 0 || p.ORGEntityCode3 != 0 || p.ORGEntityCode4 != 0 || p.ORGEntityCode5 != 0 || p.ORGEntityCode6 != 0
                    || p.ORGEntityCode7 != 0).ToList());
                    var grouping = lstorgManagerDetails.OrderBy(k => k.HierarchyName).GroupBy(k => k.HierarchyName);

                    long LOBId = GetCommonDao().GetLOBByDocumentCode(documentCode);
                    var validateOrderforInactiveorNoManager = commonManager.GetSettingsValueByKey(P2PDocumentType.None, "ValidateOrderforInactiveorNoManager", UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBId);
                    if (validateOrderforInactiveorNoManager != null && validateOrderforInactiveorNoManager.ToLower() == "true")
                    {
                        lstorgManagerDetails.ForEach(managerDetail =>
                        {
                            if (!managerDetail.IsManagerActive)
                            {
                                managerDetail.ManagerDetails.ForEach(manager =>
                                {
                                    {
                                        if (approvalParam.Any(e => e.Name.ToLower() == "leadapproverid"))
                                        {
                                            string leadApproverId = approvalParam.First(e => e.Name.ToLower() == "leadapproverid").Value;
                                            manager.ApproverId = Convert.ToInt64(leadApproverId);
                                            manager.ApproverType = 2;
                                        }
                                    }
                                });
                            }
                        });

                        List<string> sourceHierarchySplits = new List<string>();
                        List<string> returnedHierarchySplits = new List<string>();
                        lstorgManagerDetailsTemp.ForEach(l =>
                        {
                            List<string> a = l.HierarchyName.Split('-').ToList();
                            a.Remove(a[0]);
                            a.ToList();
                            a.ForEach(s =>
                            {
                                sourceHierarchySplits.Add(s);
                            });
                        });

                        lstorgManagerDetails.ForEach(l =>
                        {
                            List<string> a = l.HierarchyName.Split('-').ToList();
                            a.Remove(a[0]);
                            a.ToList();
                            a.ForEach(s =>
                            {
                                returnedHierarchySplits.Add(s);
                            });
                        });


                        var finalReturnedvals = new HashSet<string>(returnedHierarchySplits).ToList();
                        var finalSourcevals = new HashSet<string>(sourceHierarchySplits).ToList();

                        var unmappedManagerEntitycodes = finalSourcevals.Except(finalReturnedvals);

                        if (unmappedManagerEntitycodes.Count() > 0)
                        {
                            foreach (var ume in unmappedManagerEntitycodes)
                            {
                                var entityCombination = "";
                                foreach (var entity in entitySum.Where(k => k.TotalAmount > 0 || k.OverallLimitSplitTotal > 0))
                                {

                                    entity.EntityCombinations.ForEach(a =>
                                    {
                                        entityCombination = entityCombination + a.EntityCode + "|";
                                    });

                                    if (entityCombination == ume)
                                    {
                                        OrgManagerDetails orgManagerDetails = new OrgManagerDetails();
                                        orgManagerDetails.StructureId = mapping.StructureId;
                                        orgManagerDetails.StructureName = mapping.StructureName;
                                        if (isThresholdAmtBasedOnOverallTotal)
                                        {
                                            if ((decimal)entity.OverallLimitSplitTotal <= 0)
                                                continue;
                                            if (totalentitysum != null)
                                            {
                                                var totalCOoverall = totalentitysum.First(totEnt => JsonConvert.SerializeObject(totEnt.EntityCombinations) == JsonConvert.SerializeObject(entity.EntityCombinations));
                                                if ((totalCOoverall != null) && (totalCOoverall.OverallLimitSplitTotal > 0))
                                                    orgManagerDetails.ThresholdAmt = (decimal)totalCOoverall.OverallLimitSplitTotal;
                                                else
                                                    continue;
                                            }
                                            else
                                                orgManagerDetails.ThresholdAmt = (decimal)entity.OverallLimitSplitTotal;
                                        }
                                        else
                                        {
                                            if ((decimal)entity.TotalAmount <= 0)
                                                continue;
                                            if (totalentitysum != null)
                                            {
                                                var totalCOAmt = totalentitysum.First(totEnt => JsonConvert.SerializeObject(totEnt.EntityCombinations) == JsonConvert.SerializeObject(entity.EntityCombinations));
                                                if ((totalCOAmt != null) && (totalCOAmt.TotalAmount > 0))
                                                    orgManagerDetails.ThresholdAmt = (decimal)totalCOAmt.TotalAmount;
                                                else
                                                    continue;
                                            }
                                            else
                                                orgManagerDetails.ThresholdAmt = (decimal)entity.TotalAmount;
                                        }
                                        orgManagerDetails.HierarchyName = mapping.StructureName + "-";
                                        foreach (var a in entity.EntityCombinations)
                                        {
                                            var mappingId = mapping.EntityMapping.First(k => k.Key == a.EntityTypeId).Value;
                                            switch (mappingId)
                                            {
                                                case 1:
                                                    orgManagerDetails.ORGEntityCode1 = a.EntityDetailCode;
                                                    break;
                                                case 2:
                                                    orgManagerDetails.ORGEntityCode2 = a.EntityDetailCode;
                                                    break;
                                                case 3:
                                                    orgManagerDetails.ORGEntityCode3 = a.EntityDetailCode;
                                                    break;
                                                case 4:
                                                    orgManagerDetails.ORGEntityCode4 = a.EntityDetailCode;
                                                    break;
                                                case 5:
                                                    orgManagerDetails.ORGEntityCode5 = a.EntityDetailCode;
                                                    break;
                                                case 6:
                                                    orgManagerDetails.ORGEntityCode6 = a.EntityDetailCode;
                                                    break;
                                                case 7:
                                                    orgManagerDetails.ORGEntityCode7 = a.EntityDetailCode;
                                                    break;
                                            }
                                            orgManagerDetails.HierarchyName = orgManagerDetails.HierarchyName + a.EntityCode + "|";
                                            if (approvalParam.Any(e => e.Name.ToLower() == "leadapproverid"))
                                            {
                                                var leadApproverId = approvalParam.First(e => e.Name.ToLower() == "leadapproverid").Value;
                                                orgManagerDetails.ManagerDetails = new List<ApproverDetails>() { };
                                                orgManagerDetails.ManagerDetails.Add(new ApproverDetails
                                                {
                                                    ApproverId = Convert.ToInt64(leadApproverId),
                                                    ApproverType = 2
                                                });
                                            }
                                            else
                                            {
                                                orgManagerDetails.ManagerDetails = new List<ApproverDetails>() { }; ;
                                            }

                                        }
                                        if (orgManagerDetails.ManagerDetails.Count > 0)
                                        {
                                            lstorgManagerDetails.Add(orgManagerDetails);
                                        }
                                    }
                                }
                            };
                        }
                    }

                    var considerAuthorityAmtInUserDefinedWF = commonManager.GetSettingsValueByKey(P2PDocumentType.None, "ConsiderAuthorityAmtInUserDefinedWF", UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBId);
                    if (!string.IsNullOrEmpty(considerAuthorityAmtInUserDefinedWF) && considerAuthorityAmtInUserDefinedWF.ToLower() == "true")
                    {
                        lstorgManagerDetailsTemp.ForEach(x =>
                        {
                            var thresholdAmtLst = lstorgManagerDetails.Where(y => y.ORGEntityCode1 == x.ORGEntityCode1 && y.ORGEntityCode2 == x.ORGEntityCode2
                                                 && y.ORGEntityCode3 == x.ORGEntityCode3 && y.ORGEntityCode4 == x.ORGEntityCode4 && y.ORGEntityCode5 == x.ORGEntityCode5
                                                 && y.ORGEntityCode6 == x.ORGEntityCode6 && y.ORGEntityCode7 == x.ORGEntityCode7 && y.HierarchyName == x.HierarchyName
                                                 && y.StructureId == x.StructureId && y.StructureName == x.StructureName)
                                                .Select(s => s.ThresholdAmt).ToList();
                            decimal maxThresholdForCC = thresholdAmtLst.Max();
                            if (x.ThresholdAmt > maxThresholdForCC)
                            {
                                string leadApproverId = approvalParam.First(e => e.Name.ToLower() == "leadapproverid").Value;

                                lstLeadApproverDetails.Add(new OrgManagerDetails
                                {
                                    ORGEntityCode1 = x.ORGEntityCode1,
                                    ORGEntityCode2 = x.ORGEntityCode2,
                                    ORGEntityCode3 = x.ORGEntityCode3,
                                    ORGEntityCode4 = x.ORGEntityCode4,
                                    ORGEntityCode5 = x.ORGEntityCode5,
                                    ORGEntityCode6 = x.ORGEntityCode6,
                                    ORGEntityCode7 = x.ORGEntityCode7,
                                    StructureId = x.StructureId,
                                    StructureName = x.StructureName,
                                    HierarchyName = x.HierarchyName,
                                    IsManagerActive = x.IsManagerActive,
                                    ManagerDetails = new List<ApproverDetails>()
                                    {
                                        new ApproverDetails()
                                        {
                                        ApproverId = Convert.ToInt64(leadApproverId),
                                        ApproverType = 2
                                        }
                                    }
                                });
                            }
                        });
                    }

                    foreach (var hierarchy in grouping)
                    {
                        List<UserDefinedApproval> userDefinedApprovals = new List<UserDefinedApproval>();
                        List<UserDefinedApproval> userDefinedLeadApprovals = new List<UserDefinedApproval>();
                        var maxHierarchyId = lstUserDefinedApprovers.Any() ? lstUserDefinedApprovers.Max(k => k.HierarchyId) + 1 : 0;
                        var workflowOrder = 0;
                        var leadApproverCC = lstLeadApproverDetails.Where(k => k.HierarchyName == hierarchy.Key).Select(e => e.ManagerDetails[0].ApproverId).FirstOrDefault();
                        var hierarchyNameForCC = lstLeadApproverDetails.Where(k => k.HierarchyName == hierarchy.Key).Select(e => e.HierarchyName).FirstOrDefault();
                        var structureNameForCC = lstLeadApproverDetails.Where(k => k.HierarchyName == hierarchy.Key).Select(e => e.StructureName).FirstOrDefault();

                        foreach (var managerDetails in hierarchy.OrderBy(k => k.ThresholdAmt))
                        {
                            userdefinedapproval = new UserDefinedApproval();

                            userdefinedapproval.CollectionId = int.MaxValue;
                            userdefinedapproval.CollectionName = managerDetails.StructureName;
                            userdefinedapproval.DocumentCode = documentCode;
                            userdefinedapproval.IsActive = false;
                            userdefinedapproval.IsDeleted = false;
                            userdefinedapproval.IsProcessed = false;
                            userdefinedapproval.RuleId = int.MaxValue;
                            userdefinedapproval.RuleName = managerDetails.StructureName;
                            userdefinedapproval.WFDocTypeId = wfDocTypeId;
                            userdefinedapproval.WFSubOrderId = 0;
                            userdefinedapproval.WorkflowId = 12;
                            userdefinedapproval.SkipType = Enums.WFSkipType.None;
                            userdefinedapproval.IsSkipped = false;
                            userdefinedapproval.SkipActionerId = 0;
                            userdefinedapproval.PoolType = Convert.ToInt32(approvalParam.Where(e => e.Name.ToLower() == "pooltype").DefaultIfEmpty(new ParameterOutput() { Value = "1" }).First().Value);
                            userdefinedapproval.PoolTypeValue = Convert.ToInt32(approvalParam.Where(e => e.Name.ToLower() == "pooltypevalue").DefaultIfEmpty(new ParameterOutput() { Value = "1" }).First().Value);
                            userdefinedapproval.RejectionPoolType = 0;
                            userdefinedapproval.RejectionPoolTypeValue = 0;
                            userdefinedapproval.EntityTypeId = 0;
                            userdefinedapproval.IsOfflineApproved = false;
                            userdefinedapproval.OfflineApproverId = 0;
                            userdefinedapproval.HierarchyId = maxHierarchyId;
                            userdefinedapproval.HierarchyName = managerDetails.HierarchyName;
                            userdefinedapproval.ParentHierarchyId = 0;
                            userdefinedapproval.IsVisibilityRestricted = false;
                            userdefinedapproval.WorkflowOrder = workflowOrder++;

                            userdefinedapproval.WorkflowSettings = new List<WorkFlowSettings>();

                            userdefinedapproval.WorkflowSettings.Add(new WorkFlowSettings()
                            {
                                SettingName = "SpecificUsers",
                                SettingValue = String.Join(",", managerDetails.ManagerDetails.Select(k => k.ApproverId))
                            });
                            userDefinedApprovals.Add(userdefinedapproval);
                        };
                        lstUserDefinedApprovers.AddRange(userDefinedApprovals);

                        if (!string.IsNullOrEmpty(considerAuthorityAmtInUserDefinedWF) && considerAuthorityAmtInUserDefinedWF.ToLower() == "true" && leadApproverCC > 0)
                        {
                            userdefinedLeadapproval = new UserDefinedApproval();

                            userdefinedLeadapproval.CollectionId = int.MaxValue;
                            userdefinedLeadapproval.CollectionName = structureNameForCC;
                            userdefinedLeadapproval.DocumentCode = documentCode;
                            userdefinedLeadapproval.IsActive = false;
                            userdefinedLeadapproval.IsDeleted = false;
                            userdefinedLeadapproval.IsProcessed = false;
                            userdefinedLeadapproval.RuleId = int.MaxValue;
                            userdefinedLeadapproval.RuleName = structureNameForCC;
                            userdefinedLeadapproval.WFDocTypeId = wfDocTypeId;
                            userdefinedLeadapproval.WFSubOrderId = 0;
                            userdefinedLeadapproval.WorkflowId = 12;
                            userdefinedLeadapproval.SkipType = Enums.WFSkipType.None;
                            userdefinedLeadapproval.IsSkipped = false;
                            userdefinedLeadapproval.SkipActionerId = 0;
                            userdefinedLeadapproval.PoolType = Convert.ToInt32(approvalParam.Where(e => e.Name.ToLower() == "pooltype").DefaultIfEmpty(new ParameterOutput() { Value = "1" }).First().Value);
                            userdefinedLeadapproval.PoolTypeValue = Convert.ToInt32(approvalParam.Where(e => e.Name.ToLower() == "pooltypevalue").DefaultIfEmpty(new ParameterOutput() { Value = "1" }).First().Value);
                            userdefinedLeadapproval.RejectionPoolType = 0;
                            userdefinedLeadapproval.RejectionPoolTypeValue = 0;
                            userdefinedLeadapproval.EntityTypeId = 0;
                            userdefinedLeadapproval.IsOfflineApproved = false;
                            userdefinedLeadapproval.OfflineApproverId = 0;
                            userdefinedLeadapproval.HierarchyId = maxHierarchyId;
                            userdefinedLeadapproval.HierarchyName = hierarchyNameForCC;
                            userdefinedLeadapproval.ParentHierarchyId = 0;
                            userdefinedLeadapproval.IsVisibilityRestricted = false;
                            userdefinedLeadapproval.WorkflowOrder = workflowOrder++;

                            userdefinedLeadapproval.WorkflowSettings = new List<WorkFlowSettings>();

                            userdefinedLeadapproval.WorkflowSettings.Add(new WorkFlowSettings()
                            {
                                SettingName = "SpecificUsers",
                                SettingValue = String.Join(",", leadApproverCC)
                            });

                            if (userdefinedLeadapproval.WorkflowSettings[0].SettingValue != null && userdefinedLeadapproval.WorkflowSettings[0].SettingValue.Length > 0)
                            {
                                userDefinedLeadApprovals.Add(userdefinedLeadapproval);
                            }

                            if (userDefinedLeadApprovals.Count > 0)
                            {
                                lstUserDefinedApprovers.AddRange(userDefinedLeadApprovals);
                            }
                        }
                    }
                }
            }
        }

        public DateTime TimeStampCheck(DateTime timecheck)
        {
            TimeSpan MinValueTime = new TimeSpan(0, 0, 0);
            if (timecheck.TimeOfDay == MinValueTime)
            {
                TimeSpan currenttime = DateTime.Now.TimeOfDay;

                timecheck = timecheck + currenttime;
            }

            return timecheck;

        }
    
        public List<RuleAction> UpdateActionParameters(List<RuleAction> actions, long contactCode, Object doc, Int32 type)
        {
            DocumentAdditionalEntityInfo ent = new DocumentAdditionalEntityInfo();
            foreach (RuleAction a in actions)
            {
                int entityType, min;
                Boolean sox;
                string LOBEntityDetailCode = string.Empty;
                string HeaderEntityCodes = string.Empty;
                string PreferenceLOBType = string.Empty;
                ProxyPartnerService proxyPartnerService = new ProxyPartnerService(UserContext, this.JWTToken);
                List<UserLOBMapping> userLOBMappingCollection = new List<UserLOBMapping>();

                RequisitionCommonManager commonManager = new RequisitionCommonManager(JWTToken) { GepConfiguration = this.GepConfiguration, UserContext = this.UserContext };
                bool isUrgentApproval = false;

                if (a.Action == ActionType.SupervisoryHierUserBased)
                {
                    List<RuleParameter> ps = JsonConvert.DeserializeObject<List<RuleParameter>>(a.Parameters);
                    if (type == 1)
                    {
                        Requisition objReq = (Requisition)doc;
                        string skippedApprovalSetting = commonManager.GetSettingsValueByKey(P2PDocumentType.Requisition, "EnableSkippedApproval", UserContext.ContactCode, 107, "", objReq.EntityDetailCode.FirstOrDefault());
                        bool isEnableSkippedApproval = false;
                        if (!(string.IsNullOrEmpty(skippedApprovalSetting)))
                        {
                            isEnableSkippedApproval = Convert.ToBoolean(skippedApprovalSetting);
                        }
                        if (objReq.IsUrgent && isEnableSkippedApproval)
                        {
                            isUrgentApproval = true;
                        }
                        LOBEntityDetailCode = objReq.EntityDetailCode.FirstOrDefault().ToString();
                        proxyPartnerService = new ProxyPartnerService(this.UserContext, this.JWTToken);
                        userLOBMappingCollection = proxyPartnerService.GetUserLOBDetailByContactCode(objReq.OnBehalfOf == 0 ? objReq.RequesterId : objReq.OnBehalfOf);
                        var results = userLOBMappingCollection.Where(e => (e.EntityDetailCode) == Convert.ToInt64(LOBEntityDetailCode));
                        if (results.Count() > 0 && results.Where(e => e.PreferenceLobType == Gep.Cumulus.Partner.Entities.PreferenceLOBType.Belong).Count() > 0)
                        {
                            PreferenceLOBType = ((int)Gep.Cumulus.Partner.Entities.PreferenceLOBType.Belong).ToString();
                        }
                        else
                        {
                            PreferenceLOBType = ((int)Gep.Cumulus.Partner.Entities.PreferenceLOBType.Serve).ToString();
                        }
                        if (objReq.DocumentAdditionalEntitiesInfoList != null && objReq.DocumentAdditionalEntitiesInfoList.Count > 0)
                        {
                            foreach (var bu in objReq.DocumentAdditionalEntitiesInfoList)
                            {
                                if (string.IsNullOrEmpty(HeaderEntityCodes))
                                {
                                    HeaderEntityCodes = bu.EntityDetailCode.ToString();
                                }
                                else
                                {
                                    HeaderEntityCodes = HeaderEntityCodes + "," + bu.EntityDetailCode.ToString();
                                }
                            }

                        }


                        RuleParameter lobidParameter = new RuleParameter();
                        lobidParameter.Id = 0;
                        lobidParameter.Name = "LOBEntityDetailCode";
                        lobidParameter.Value = LOBEntityDetailCode;

                        RuleParameter documentBUList = new RuleParameter();
                        documentBUList.Id = 0;
                        documentBUList.Name = "EntityDetailCode";
                        documentBUList.Value = HeaderEntityCodes;

                        RuleParameter preferredLOBID = new RuleParameter();
                        preferredLOBID.Id = 0;
                        preferredLOBID.Name = "PreferenceLOBType";
                        preferredLOBID.Value = PreferenceLOBType;

                        RuleParameter isUrgentParameter = new RuleParameter();
                        isUrgentParameter.Id = 0;
                        isUrgentParameter.Name = "IsUrgentApproval";
                        isUrgentParameter.Value = Convert.ToString(isUrgentApproval);

                        ps.Add(lobidParameter);
                        ps.Add(documentBUList);
                        ps.Add(preferredLOBID);
                        ps.Add(isUrgentParameter);
                    }

                    RuleParameter p = ps.Where(e => e.Name.ToString().ToUpper().Trim().Equals("USEHIERARCHYOF")).FirstOrDefault();

                    if (p != null && !string.IsNullOrWhiteSpace(p.Value) && (p.Value.ToUpper().Trim().Equals("3")))
                    {
                        p.Value = Convert.ToString(contactCode);
                        long entityCode = 0;
                        RuleParameter p2 = ps.Where(e => e.Name.ToUpper().Trim().Equals("HIERARCHYORGTYPEID")).FirstOrDefault();
                        if (p2 != null)
                        {

                            Int32.TryParse(p2.Value, out entityType);
                            switch (type)
                            {
                                case 1:
                                    Requisition req = (Requisition)doc;
                                    ent = req.DocumentAdditionalEntitiesInfoList.Where(e => e.EntityId == entityType).FirstOrDefault();
                                    break;
                                case 2:
                                    Order odr = (Order)doc;
                                    ent = odr.DocumentAdditionalEntitiesInfoList.Where(e => e.EntityId == entityType).FirstOrDefault();
                                    break;
                                case 3:
                                    //InvoiceReconciliation ir = (InvoiceReconciliation)doc;
                                    //ent = ir.DocumentAdditionalEntitiesInfoList.Where(e => e.EntityId == entityType).FirstOrDefault();
                                    break;
                                default:
                                    break;
                            }

                            if (ent != null)
                                entityCode = ent.EntityDetailCode;
                        }
                        if (entityCode > 0)
                        {
                            NewRequisitionManager newRequisitionManager = new NewRequisitionManager(this.JWTToken) { GepConfiguration = this.GepConfiguration, UserContext = this.UserContext };
                            long managerCode = newRequisitionManager.GetOrgEntityManagers(entityCode);

                            var isActive = ps.Where(e => e.Name.ToUpper().Trim().Equals("SENDINACTIVEUSERTOLEADAPPROVER")).FirstOrDefault();

                            if (isActive != null && isActive.Value == "true" && managerCode == 0)
                            {
                                a.Action = ActionType.SpecificUser;
                            }
                            else if (managerCode > 0)
                            {
                                p.Value = managerCode.ToString();
                                RuleParameter p3 = ps.Where(e => e.Name.ToUpper().Trim().Equals("FORCESOXCOMPLIANCE")).First();
                                RuleParameter p4 = ps.Where(e => e.Name.ToUpper().Trim().Equals("MINIMUMAPPROVALSREQUIRED")).First();
                                Int32.TryParse(p4.Value, out min);
                                Boolean.TryParse(p3.Value, out sox);
                                if (sox && min < 2 && managerCode == contactCode)
                                {
                                    p4.Value = "2";
                                }
                            }
                            else
                            {
                                p.Value = contactCode.ToString();
                            }
                        }
                    }
                    else if (p != null && !string.IsNullOrWhiteSpace(p.Value) && (p.Value.ToUpper().Trim().Equals("ONBEHALFOF") && type == 1))
                    {
                        if (doc != null)
                        {
                            Requisition req = (Requisition)doc;

                            if (req != null && req.OnBehalfOf > 0 && req.RequesterId != req.OnBehalfOf)
                            {
                                p.Value = Convert.ToString(req.OnBehalfOf);
                            }
                            else
                                p.Value = contactCode.ToString();
                            if (req.IsOBOManager)
                            {
                                RuleParameter p1 = ps.Where(e => e.Name.ToUpper().Trim().Equals("MINIMUMAPPROVALSREQUIRED")).First();
                                if (Convert.ToInt32(p1.Value) < 2)
                                {
                                    p1.Value = "2";
                                }
                            }
                        }
                    }
                    else if (p != null && !string.IsNullOrWhiteSpace(p.Value) && (p.Value.ToUpper().Trim().Equals("COSTAPPROVER") && type == 1))
                    {
                        if (doc != null)
                        {
                            Requisition req = (Requisition)doc;

                            if (req != null && req.CostApprover > 0)
                            {
                                p.Value = Convert.ToString(req.CostApprover);
                            }
                            else
                                p.Value = contactCode.ToString();
                        }
                    }
                    else if (p != null && !string.IsNullOrWhiteSpace(p.Value) && (p.Value.ToUpper().Trim().Equals("ORDERCONTACT") && type == 2))
                    {
                        if (doc != null)
                        {
                            Order order = (Order)doc;

                            if (order != null)
                            {
                                p.Value = Convert.ToString(order.OrderContactCode);
                            }
                            else
                                p.Value = contactCode.ToString();

                        }
                    }
                    else
                    {
                        p.Value = contactCode.ToString();
                    }

                    a.Parameters = JsonConvert.SerializeObject(ps);
                }
                else if (a.Action == ActionType.UserDefinedApproval)
                {
                    List<RuleParameter> ps = JsonConvert.DeserializeObject<List<RuleParameter>>(a.Parameters);
                    RuleParameter p = ps.Where(e => e.Name.ToString().ToUpper().Trim().Equals("ENTITYTYPE")).FirstOrDefault();
                    if (p != null && !string.IsNullOrWhiteSpace(p.Value))
                    {
                        Type t = doc.GetType();
                        PropertyInfo prop = t.GetProperty("DocumentId");
                        p.Value = JsonConvert.SerializeObject(GetCommonDao().GetAllEntityValueAmount(Convert.ToInt64(prop.GetValue(doc, null)), p.Value));
                    }
                    a.Parameters = JsonConvert.SerializeObject(ps);
                }
            }
            return actions;
        }

        public string SaveOfflineApprovalDetails(long contactCode, long documentCode, decimal documentAmount, int documentTypeId, WorkflowInputEntities workflowEntity, string currency)
        {
            #region Debug Logging
            if (Log.IsDebugEnabled)
            {
                Log.Debug(string.Concat("In SentDocumentForApproval Method in RequisitionDocumentManager  ",
                " with parameters: contactCode = " + contactCode + ",documentCode =" + documentCode +
                                                 ",documentAmount = " + documentAmount + ",documentTypeId=" + documentTypeId));
            }
            #endregion
            string serviceurl;
            try
            {
                serviceurl = MultiRegionConfig.GetConfig(CloudConfig.WorkFlowRestURL) + "/InvokeWorkFlow";
                CreateHttpWebRequest(serviceurl);

                Dictionary<string, object> objDict = new Dictionary<string, object>();
                objDict.Add("contactCode", contactCode);
                objDict.Add("documentCode", documentCode);

                objDict.Add("documentAmount", documentAmount);
                objDict.Add("documentTypeId", documentTypeId);
                objDict.Add("eventName", "OnSubmit");
                objDict.Add("returnEntity", "");
                if ((documentTypeId == 7 || documentTypeId == 8 || documentTypeId == 9 || documentTypeId == 10) &&
                    workflowEntity != null && workflowEntity.RuleAction != null)
                {
                    objDict.Add("wfInputParameter", workflowEntity);
                }
                objDict.Add("currency", currency);

                string result = null;

                result = GetHttpWebResponse(objDict);
                return result;

            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in SentDocumentForApproval Method in RequisitionDocumentManager", ex);
                throw;
            }

            return string.Empty;
        }

        private void CreateHttpWebRequest(string strURL, string method = "POST")
        {
            httpWebRequest = WebRequest.Create(strURL) as HttpWebRequest;
            httpWebRequest.Method = method;
            httpWebRequest.ContentType = @"application/json";

            NameValueCollection nameValueCollection = new NameValueCollection();
            //UserContext.UserName = "";
            string userName = UserContext.UserName;
            string clientName = UserContext.ClientName;
            UserContext.UserName = string.Empty;
            UserContext.ClientName = string.Empty;
            string userContextJson = UserContext.ToJSON();
            nameValueCollection.Add("UserExecutionContext", userContextJson);
            nameValueCollection.Add("Authorization", this.JWTToken);
            httpWebRequest.Headers.Add(nameValueCollection);
            UserContext.UserName = userName;
            UserContext.ClientName = clientName;

        }

        private string GetHttpWebResponse(Dictionary<string, object> odict)
        {
            JavaScriptSerializer JSrz = new JavaScriptSerializer();
            var data = JSrz.Serialize(odict);
            var byteData = Encoding.UTF8.GetBytes(data);
            httpWebRequest.ContentLength = byteData.Length;
            using (Stream stream = httpWebRequest.GetRequestStream())
            {
                stream.Write(byteData, 0, byteData.Length);
            }

            string result = null;
            using (HttpWebResponse resp = httpWebRequest.GetResponse() as HttpWebResponse)
            {
                using (StreamReader reader = new StreamReader(resp.GetResponseStream()))
                {
                    result = reader.ReadToEnd();
                }
            }
            return result;
        }

        public bool UpdateDocumentInterfaceStatus(P2PDocumentType docType, long documentId, BusinessEntities.InterfaceStatus interfaceStatus, bool modifyAdditionalInfo, int sourceSystemId, string errorDescription,string DocType = "")
        {
            var commonManager = new RequisitionCommonManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
            bool returnValue;
            try
            {
                int maxPrecisionValue = commonManager.GetPrecisionValue();
                int maxPrecisionValueForTaxesAndCharges = commonManager.GetPrecisionValueForTaxesAndCharges();
                int maxPrecisionValueForTotal = commonManager.GetPrecisionValueforTotal();
                var objInterfaceSettings = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.Interfaces, UserContext.ContactCode, (int)SubAppCodes.Interfaces);
                var settingValue = commonManager.GetSettingsValueByKey(objInterfaceSettings, "DefaultUser");

                bool isRequisitionExtendedStatusTobeUpdated = interfaceStatus == BusinessEntities.InterfaceStatus.AcceptedByERP || interfaceStatus == BusinessEntities.InterfaceStatus.RejectedByERP || interfaceStatus == BusinessEntities.InterfaceStatus.QueuedForERPProcessing;
                bool isRequisitionEligibleForWorkFlowProcess = interfaceStatus == BusinessEntities.InterfaceStatus.AcceptedByERP;

                if ((this.UserContext.Product == GEPSuite.eInterface) && (!string.IsNullOrWhiteSpace(settingValue)))
                {
                    this.UserContext.ContactCode = Convert.ToInt64(settingValue);
                }
                int extendedStatus = GetReqDao().GetDocumentExtendedStatus(documentId);
                //returnValue = GetReqDao().UpdateDocumentInterfaceStatus(documentId, interfaceStatus, modifyAdditionalInfo, sourceSystemId, maxPrecisionValue, maxPrecisionValueForTaxesAndCharges, maxPrecisionValueForTotal, DocType);//indexing in respective manager
                returnValue = GetReqDao().UpdateDocumentInterfaceStatusForRequisition(documentId, interfaceStatus, sourceSystemId,errorDescription:errorDescription);//indexing in respective manager
                if (isRequisitionExtendedStatusTobeUpdated)
                    GetReqDao().UpdateRequisitionExtendedStatus(documentId, "", interfaceStatus == BusinessEntities.InterfaceStatus.QueuedForERPProcessing ? 1:0);
               
                if (isRequisitionEligibleForWorkFlowProcess && extendedStatus==1)
                    ProcessRequitionForWorkFlow(documentId);

                AddIntoSearchIndexerQueueing(documentId, (int)getDocumentType(docType));
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occurred in UpdateDocumentInterfaceStatus method in RequisitionDocumentManager documentcode :- " + documentId+" InterfaceStatus :-"+ interfaceStatus, ex);
                throw;
            }
            return returnValue;
        }

        private void ProcessRequitionForWorkFlow(long documentId)
        {
            try
            {
                RequisitionDocumentManager documentManager = new RequisitionDocumentManager(this.JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
                if (Log.IsDebugEnabled)
                    LogHelper.LogInfo(Log, String.Format("ProcessRequitionForWorkFlow Started for Requisition ID={0},", documentId));
                long requesterContactCode;
                string fromCurrency, toCurrency;
                decimal totalAmount;
                GetRequisitionDetailsForWorkflowProcess(documentId, out requesterContactCode, out fromCurrency, out toCurrency, out totalAmount);
                Dictionary<string,string> result = SendDocumentForReview(documentId, requesterContactCode,7, isBypassOperationalBudget:true);
                if (result.ContainsKey("SendForReviewResult") && result["SendForReviewResult"].Equals("NoReviewSetup"))
                {
                    result = ProcesssRequisitionForApproval(documentId, requesterContactCode, fromCurrency, toCurrency, totalAmount);
                }
                if (!(result is null) && result.Count > 0)
                {
                    UpdateRequisitionExtendedStatus(result, documentId);
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occurred in ProcessRequisitionWorkFlowForExternalvalidation method in RequisitionDocumentManager.", ex);
                throw;
            }
        }

        private void UpdateRequisitionExtendedStatus(Dictionary<string, string> result,long requisitionId)
        {
            
            var updatedExtendedStatusValue = 0;
            RequisitionManager requisitionManager = new RequisitionManager(this.JWTToken){UserContext = this.UserContext,GepConfiguration = this.GepConfiguration};
            var isUpdateSucessfull = requisitionManager.UpdateRequisitionExtendedStatus(requisitionId, "", updatedExtendedStatusValue);
            if (Log.IsDebugEnabled)
            {
                var sentToWorkFlowResponse = "";
                if (result.ContainsKey("SendForReviewResult"))
                    sentToWorkFlowResponse = result?["SendForReviewResult"];
                else if(result.ContainsKey("SendForApprovalResult"))
                    sentToWorkFlowResponse = result?["SendForApprovalResult"];

                    LogHelper.LogInfo(Log, String.Format("ProcessRequitionForWorkFlow Method Completed for sentToWorkFlowResponse={0},requisitionId={1},isUpdateSucessfull={2}", sentToWorkFlowResponse, requisitionId, isUpdateSucessfull));
            }
        }

        private Dictionary<string, string> ProcesssRequisitionForApproval(long documentId, long requesterContactCode, string fromCurrency, string toCurrency, decimal totalAmount)
        {
            Dictionary<string, string> result;
            DocumentStatus documentStatus = GetDocumentStatus(documentId);
            if (documentStatus == DocumentStatus.ApprovalPending || documentStatus == DocumentStatus.Ordered || documentStatus == DocumentStatus.PartiallyOrdered || documentStatus == DocumentStatus.SentForBidding || documentStatus == DocumentStatus.Approved || documentStatus == DocumentStatus.Accepted)
            {
                if (Log.IsDebugEnabled)
                    LogHelper.LogInfo(Log, String.Format("ProcessRequitionForWorkFlow cannot sent To work flow approval for Requisition ID={0}, staus ={1}", documentId, documentStatus));
            }
            var currentPrincipal = System.Threading.Thread.CurrentPrincipal;
            Task.Factory.StartNew(() =>
            {
                System.Threading.Thread.CurrentPrincipal = currentPrincipal;
                FinalizeComments(P2PDocumentType.Requisition, documentId, false);
            });
            RequisitionManager requisitionManager = new RequisitionManager(this.JWTToken)
            {
                UserContext = this.UserContext,
                GepConfiguration = this.GepConfiguration
            };
            result = requisitionManager.SentRequisitionForApproval(requesterContactCode, documentId, totalAmount, 7, fromCurrency, toCurrency, false, 0);
            return result;
        }

        private void GetRequisitionDetailsForWorkflowProcess(long documentId, out long requesterContactCode, out string fromCurrency, out string toCurrency, out decimal totalAmount)
        {
            RequisitionManager requisitionManager = new RequisitionManager(this.JWTToken)
            {
                UserContext = this.UserContext,
                GepConfiguration = this.GepConfiguration
            };
            Dictionary<string, string> results = requisitionManager.GetRequisitionDetailsForExternalWorkFlowProcess(documentId);
            requesterContactCode = Convert.ToInt64(results?["ContactCode"] ?? "0");
            fromCurrency = results?["FromCurrency"] ?? String.Empty;
            toCurrency = results?["ToCurrency"] ?? String.Empty;
            totalAmount = Convert.ToDecimal(results?["TotalAmount"] ?? "0");
        }

        public List<SplitAccountingFields> GetAllAccountingFieldsWithDefaultValues(P2PDocumentType docType, LevelType levelType,
                                                                                         long ContactCode = 0, long docmentCode = 0,
                                                                                         List<DocumentAdditionalEntityInfo> lstHeaderEntityDetails = null,
                                                                                         List<SplitAccountingFields> lstSplitAccountingFields = null,
                                                                                         bool populateDefaultSplitValue = false, long documentItemId = 0, long lOBEntityDetailCode = 0,
                                                                                         PreferenceLOBType preferenceLOBType = PreferenceLOBType.Serve)
        {
            if (ContactCode == 0)
                ContactCode = UserContext.ContactCode;

            if (lstHeaderEntityDetails == null && docmentCode > 0)
            {
                lstHeaderEntityDetails = GetReqDao().GetAllDocumentAdditionalEntityInfo(docmentCode);
            }

            List<Tuple<int, long>> lstDetails = new List<Tuple<int, long>>();
            if (lstHeaderEntityDetails != null && lstHeaderEntityDetails.Count() > 0)
            {
                foreach (var item in lstHeaderEntityDetails)
                {
                    lstDetails.Add(new Tuple<int, long>(item.EntityId, item.EntityDetailCode));
                }
            }
            if (lstSplitAccountingFields != null && lstSplitAccountingFields.Count() > 0)
            {
                foreach (var item in lstSplitAccountingFields)
                {
                    lstDetails.Add(new Tuple<int, long>(item.EntityTypeId, item.EntityDetailCode));
                }
            }
            //int accessControlEntityId = GetAccessControlEntityId(docType, ContactCode);

            List<SplitAccountingFields> result = new List<SplitAccountingFields>();
            var commonManager = new RequisitionCommonManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };

            SettingDetails settingDetails = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.None, UserContext.ContactCode, (int)SubAppCodes.P2P, "", lOBEntityDetailCode);
            bool isEnableOldADRRules = Convert.ToBoolean(commonManager.GetSettingsValueByKey(settingDetails, "IsEnableOldADRRules"));
            bool isEnableADRRules = Convert.ToBoolean(commonManager.GetSettingsValueByKey(settingDetails, "IsEnableADRRules"));

            //Header Level Condition needs to be removed after rules return entities
            if (isEnableOldADRRules || levelType == LevelType.HeaderLevel || levelType == LevelType.Both || docType == P2PDocumentType.CreditMemo)
            {
                result = GetCommonDao().GetAllSplitAccountingFieldsWithDefaultValues(getDocumentType(docType), (levelType == LevelType.Both ? ((isEnableOldADRRules) ? LevelType.ItemLevel : LevelType.HeaderLevel) : levelType), (int)preferenceLOBType, ContactCode, lstDetails, populateDefaultSplitValue, documentItemId, lOBEntityDetailCode);
            }
            if (isEnableADRRules)
            {
                if (levelType == LevelType.Both)
                {
                    if (result.Count() > 0 && lstHeaderEntityDetails.Count() > 0)
                        lstHeaderEntityDetails.ForEach((p) =>
                        {
                            result.ForEach((r) =>
                            {
                                if (p.EntityId == r.EntityTypeId && r.EntityDetailCode > 0)
                                {
                                    p.EntityCode = r.EntityCode;
                                    p.EntityDetailCode = r.EntityDetailCode;
                                    p.EntityDisplayName = r.EntityDisplayName;
                                    p.IsAccountingEntity = r.IsAccountingEntity;
                                    p.Level = (int)r.LevelType;
                                    p.LOBEntityDetailCode = r.LOBEntityDetailCode;
                                    p.ParentEntityDetailCode = r.ParentEntityDetailCode;
                                }
                            });
                        });
                    result.Where(p => !lstHeaderEntityDetails.Exists(h => h.EntityId == p.EntityTypeId)).ToList().ForEach((p) =>
                    {
                        lstHeaderEntityDetails.Add(new DocumentAdditionalEntityInfo()
                        {
                            EntityCode = p.EntityCode,
                            EntityDetailCode = p.EntityDetailCode,
                            EntityDisplayName = p.EntityDisplayName,
                            IsAccountingEntity = p.IsAccountingEntity,
                            Level = (int)p.LevelType,
                            LOBEntityDetailCode = p.LOBEntityDetailCode,
                            ParentEntityDetailCode = p.ParentEntityDetailCode,
                            EntityId = p.EntityTypeId
                        });
                    });
                    result = new List<SplitAccountingFields>();
                }
                result = new ADRManager() { UserContext = UserContext, GepConfiguration = GepConfiguration, jwtToken = this.JWTToken }.GetAllAccountingFieldsWithDefaultValues(getDocumentType(docType), ((levelType == LevelType.Both) ? LevelType.ItemLevel : levelType), ContactCode, docmentCode, lstHeaderEntityDetails, result, populateDefaultSplitValue, documentItemId, lOBEntityDetailCode,
                preferenceLOBType);
            }
            return result ?? new List<SplitAccountingFields>();
        }
        public List<ADRSplit> GetAllAccountingFieldsWithDefaultValues(P2PDocumentType docType, LevelType levelType,
                                                                                       long ContactCode = 0, long docmentCode = 0,
                                                                                       List<DocumentAdditionalEntityInfo> lstHeaderEntityDetails = null,
                                                                                       List<ADRSplit> lstAdrSplits = null,
                                                                                       bool populateDefaultSplitValue = false, List<long> documentItemIds = null, long lOBEntityDetailCode = 0,
                                                                                       PreferenceLOBType preferenceLOBType = PreferenceLOBType.Serve, ADRIdentifier identifier = ADRIdentifier.None, object document = null)
        {
            //Overriding preference Lob Type as getting wrong values from Requisition
            preferenceLOBType = (docType == P2PDocumentType.Requisition || docType == P2PDocumentType.PaymentRequest) ? PreferenceLOBType.Belong : PreferenceLOBType.Serve;
            if (ContactCode == 0)
                ContactCode = UserContext.ContactCode;

            if (lstHeaderEntityDetails == null && docmentCode > 0)
            {
                lstHeaderEntityDetails = GetReqDao().GetAllDocumentAdditionalEntityInfo(docmentCode);
            }

            List<Tuple<int, long>> lstDetails = new List<Tuple<int, long>>();
            if (lstHeaderEntityDetails != null && lstHeaderEntityDetails.Count() > 0)
            {
                foreach (var item in lstHeaderEntityDetails)
                {
                    lstDetails.Add(new Tuple<int, long>(item.EntityId, item.EntityDetailCode));
                }
            }
            if (lstAdrSplits != null && lstAdrSplits.Count() > 0 && lstAdrSplits.First().Splits.Count() > 0)
            {
                foreach (var item in lstAdrSplits.First().Splits)
                {
                    lstDetails.Add(new Tuple<int, long>(item.EntityTypeId, item.EntityDetailCode));
                }
            }

            var commonManager = new RequisitionCommonManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
            SettingDetails settingDetails = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.None, UserContext.ContactCode, 107, "", lOBEntityDetailCode);
            bool isEnableOldADRRules = Convert.ToBoolean(commonManager.GetSettingsValueByKey(settingDetails, "IsEnableOldADRRules"));
            bool isEnableADRRules = Convert.ToBoolean(commonManager.GetSettingsValueByKey(settingDetails, "IsEnableADRRules"));

            //Header Level Condition needs to be removed after rules return entities
            if (isEnableOldADRRules || levelType == LevelType.HeaderLevel)
            {
                long documentItemId = 0;
                if (documentItemIds != null && documentItemIds.Count() > 0)
                    documentItemId = documentItemIds.FirstOrDefault();
                var split = GetCommonDao().GetAllSplitAccountingFieldsWithDefaultValues(getDocumentType(docType), levelType, (int)preferenceLOBType, ContactCode, lstDetails, populateDefaultSplitValue,
                    documentItemId, lOBEntityDetailCode);
                if (lstAdrSplits == null)
                {
                    lstAdrSplits = new List<ADRSplit>();
                    lstAdrSplits.Add(new ADRSplit
                    {
                        Identifier = "",
                        Splits = split
                    });
                }
                else
                {
                    lstAdrSplits.ForEach(p =>
                    {
                        p.Splits = new List<SplitAccountingFields>();
                        split.ForEach(q =>
                        {
                            p.Splits.Add(new SplitAccountingFields(q));
                        });
                    });
                }
            }
            if (isEnableADRRules)
            {
                lstAdrSplits = new ADRManager() { UserContext = UserContext, GepConfiguration = GepConfiguration, jwtToken = this.JWTToken }.GetAllAccountingFieldsWithDefaultValues(getDocumentType(docType), levelType, ContactCode, docmentCode, lstHeaderEntityDetails, lstAdrSplits, populateDefaultSplitValue, documentItemIds, lOBEntityDetailCode,
                preferenceLOBType, identifier, document);
            }
            if (!ReferenceEquals(lstAdrSplits, null))
            {
                lstAdrSplits.ForEach(adrsplit =>
                {
                    adrsplit.Splits.ForEach(split =>
                    {
                        if (split.FieldName.ToUpper(CultureInfo.InvariantCulture) == "REQUESTER" && string.IsNullOrEmpty(split.EntityCode))
                        {
                            split.EntityDetailCode = ContactCode;
                        }
                    });
                });
            }
            return lstAdrSplits ?? new List<ADRSplit>();
        }


        public int GetAccessControlEntityId(P2PDocumentType docType, long contactCode)
        {
            int accessControlEntityId = 0;
            if ((docType == P2PDocumentType.Requisition || docType == P2PDocumentType.Program) && UserContext.BelongingEntityDetailCode > 0)
            {
                accessControlEntityId = UserContext.GetAccessControlEntityId(UserContext.BelongingEntityDetailCode);
            }
            else if ((docType != P2PDocumentType.Requisition && docType != P2PDocumentType.Program) && UserContext.ServingEntityDetailCode > 0)
            {
                accessControlEntityId = UserContext.GetAccessControlEntityId(UserContext.ServingEntityDetailCode);
            }
            else
            {
                Gep.Cumulus.Partner.Entities.UserContext userContext;
                try
                {
                    var partnerHelper = new RESTAPIHelper.PartnerHelper(this.UserContext, JWTToken);
                    userContext = partnerHelper.GetUserContextDetailsByContactCode(contactCode);
                }
                catch (Exception ex)
                {
                    // Log Exception here
                    LogHelper.LogError(Log, "Error occurred in SetDocumentAdditionalEntitiesDetails method in ReceiptManager.", ex);
                    throw;
                }

                if (userContext.UserLOBMapping != null)
                {
                    if ((docType == P2PDocumentType.Requisition) || (docType == P2PDocumentType.Program))
                        accessControlEntityId = UserContext.GetBelongingAccessControlEntityId(userContext);
                    else
                        accessControlEntityId = UserContext.GetServingAccessControlEntityId(userContext);
                }
            }
            return accessControlEntityId;
        }
        public List<DocumentSplitItemEntity> GetDocumentDefaultAccountingDetails(P2PDocumentType docType, LevelType levelType,
                                                                                                 long ContactCode = 0, long docmentCode = 0,
                                                                                                 List<DocumentAdditionalEntityInfo> lstHeaderEntityDetails = null,
                                                                                                 List<SplitAccountingFields> lstSplitAccountingFields = null,
                                                                                 bool populateDefaultSplitValue = false, long documentItemId = 0, long lOBEntityDetailCode = 0)
        {
            var documentSplitItemEntity = new List<SplitAccountingFields>();
            if (ContactCode == 0)
                ContactCode = UserContext.ContactCode;

            //if (docType != P2PDocumentType.Order) 
            PreferenceLOBType preferenceLOBType = (docType == P2PDocumentType.Requisition || docType == P2PDocumentType.PaymentRequest) ? PreferenceLOBType.Belong : PreferenceLOBType.Serve;
            documentSplitItemEntity = GetAllAccountingFieldsWithDefaultValues(docType, levelType, ContactCode, docmentCode, lstHeaderEntityDetails, lstSplitAccountingFields, populateDefaultSplitValue, documentItemId, lOBEntityDetailCode,
                preferenceLOBType);
            //else
            //    documentSplitItemEntity = objP2PDocumentManager.GetAllSplitAccountingFieldsWithDefaultValues(docType, levelType, ContactCode, docmentCode, lstHeaderEntityDetails, lstSplitAccountingFields, populateDefaultSplitValue);

            var documentSplitItemEntities = new List<DocumentSplitItemEntity>();
            DocumentSplitItemEntity objEntity;
            foreach (var objSplitAccountingFields in documentSplitItemEntity)
            {
                objEntity = new DocumentSplitItemEntity();
                objEntity.DocumentSplitItemId = 0;
                objEntity.DocumentSplitItemEntityId = 0;
                objEntity.SplitAccountingFieldId = objSplitAccountingFields.SplitAccountingFieldId;
                if (objSplitAccountingFields.FieldName.ToUpper(CultureInfo.InvariantCulture) == "REQUESTER")
                    objEntity.SplitAccountingFieldValue = Convert.ToString(ContactCode);
                else
                {
                    objEntity.SplitAccountingFieldValue = Convert.ToString(objSplitAccountingFields.EntityDetailCode, CultureInfo.InvariantCulture);

                }
                objEntity.EntityCode = objSplitAccountingFields.EntityCode;
                objEntity.EntityTypeId = objSplitAccountingFields.EntityTypeId;
                objEntity.IsAccountingEntity = objSplitAccountingFields.IsAccountingEntity;
                documentSplitItemEntities.Add(objEntity);
            }
            return documentSplitItemEntities;
        }
        public List<ADRSplit> GetDocumentDefaultAccountingDetailsForLineItems(P2PDocumentType docType,
                                                                                       long ContactCode = 0, long docmentCode = 0,
                                                                                       List<DocumentAdditionalEntityInfo> lstHeaderEntityDetails = null,
                                                                                       List<ADRSplit> lstAdrSplits = null,
                                                                                       bool populateDefaultSplitValue = false, List<long> documentItemIds = null, long lOBEntityDetailCode = 0,
                                                                                       ADRIdentifier identifier = ADRIdentifier.None, object document = null)
        {
            var documentSplitItemEntity = new List<ADRSplit>();

            documentSplitItemEntity = GetAllAccountingFieldsWithDefaultValues(
                       docType, LevelType.ItemLevel, ContactCode, docmentCode, lstHeaderEntityDetails, lstAdrSplits,
                                                                                          populateDefaultSplitValue, documentItemIds, lOBEntityDetailCode,
                                                                                          PreferenceLOBType.Serve, identifier, document);
            return documentSplitItemEntity;
        }



        public bool FinalizeComments(P2PDocumentType docType, long objectId, bool isindexingRequired = true)
        {
            bool returnValue = false;
            try
            {
                var headerObjectType = string.Empty;
                var itemObjectType = string.Empty;
                switch (docType)
                {
                    case P2PDocumentType.Requisition:
                        headerObjectType = "GEP.Cumulus.P2P.Requisition";
                        itemObjectType = "GEP.Cumulus.P2P.Requisition.LineItem";
                        break;
                        //case P2PDocumentType.Order:
                        //    headerObjectType = "GEP.Cumulus.P2P.Order";
                        //    itemObjectType = "GEP.Cumulus.P2P.Order.LineItem";
                        //    break;
                        //case P2PDocumentType.Receipt:
                        //    headerObjectType = "GEP.Cumulus.P2P.Receipt";
                        //    itemObjectType = "GEP.Cumulus.P2P.Receipt.LineItem";
                        //    break;
                        //case P2PDocumentType.Invoice:
                        //    headerObjectType = "GEP.Cumulus.P2P.Invoice";
                        //    itemObjectType = "GEP.Cumulus.P2P.Invoice.LineItem";
                        //    break;
                        //case P2PDocumentType.InvoiceReconciliation:
                        //    headerObjectType = "GEP.Cumulus.P2P.IR";
                        //    itemObjectType = "GEP.Cumulus.P2P.IR.LineItem";
                        //    break;
                        //case P2PDocumentType.ReturnNote:
                        //    headerObjectType = "GEP.Cumulus.P2P.ReturnNote";
                        //    itemObjectType = "GEP.Cumulus.P2P.ReturnNote.LineItem";
                        //    break;
                        //case P2PDocumentType.PaymentRequest:
                        //    headerObjectType = "GEP.Cumulus.P2P.PaymentRequest";
                        //    itemObjectType = "GEP.Cumulus.P2P.PaymentRequest.LineItem";
                        //    break;
                }

                var lstCommentGroup = new List<CommentsGroup>();

                var objCommentsGroupReq = new CommentsGroup()
                {
                    ObjectID = objectId,
                    ObjectType = headerObjectType
                };
                lstCommentGroup.Add(objCommentsGroupReq);
                //MessageHeader<UserExecutionContext> objMhg = new MessageHeader<UserExecutionContext>(UserContext);
                //MessageHeader untypedmh = objMhg.GetUntypedHeader("GepCustomHeader", "Gep.Cumulus");
                //OperationContext.Current.OutgoingMessageHeaders.Add(untypedmh);

                var dtItems = GetReqDao().GetAllItemsByDocumentCode(objectId);
                if (dtItems != null && dtItems.Rows != null && dtItems.Rows.Count > 0)
                {
                    PushDatatoCommentGroup(ref lstCommentGroup, dtItems, itemObjectType);
                }

                var commentHelper = new RESTAPIHelper.CommentHelper(this.UserContext, JWTToken);
                returnValue = commentHelper.FinalizeComments(lstCommentGroup);


                if (isindexingRequired)
                {
                    AddIntoSearchIndexerQueueing(objectId, (int)getDocumentType(docType));//REVIEW
                }

            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in FinalizeComments", ex);
                throw;
            }
            return returnValue;

        }

        private static void PushDatatoCommentGroup(ref List<CommentsGroup> lstCommentGroup, DataTable dtItems, string objectType)
        {
            for (var i = 0; i < dtItems.Rows.Count; i++)
            {
                var objCommentGroup = new CommentsGroup()
                {
                    ObjectID = Convert.ToInt64(dtItems.Rows[i]["ItemId"], CultureInfo.InvariantCulture),
                    ObjectType = objectType
                };
                lstCommentGroup.Add(objCommentGroup);
            }
        }

        public Collection<DocumentAdditionalEntityInfo> SetDocumentAdditionalEntitiesDetails(P2PDocumentType docType, LevelType levelType, ContactORGMapping orgDetail = null, long contactCode = 0)
        {
            contactCode = contactCode > 0 ? contactCode : UserContext.ContactCode;
            string key = "DocumentAdditionalEntitiesDetails" + docType;
            Collection<DocumentAdditionalEntityInfo> lstDocumentAdditionalEntityInfo = GEPDataCache.GetFromCacheJSON<Collection<DocumentAdditionalEntityInfo>>(key, UserContext.BuyerPartnerCode, contactCode, "en-US");
            if (lstDocumentAdditionalEntityInfo == null)
            {
                ContactORGMapping defaultBU = new ContactORGMapping();
                try
                {
                    int accessControlEntityId = 0;
                    long LOBEntityDetailCode = 0;
                    if (contactCode == UserContext.ContactCode)
                    {
                        if ((docType == P2PDocumentType.Requisition) || (docType == P2PDocumentType.Program))
                        {
                            accessControlEntityId = UserContext.BelongingEntityId;
                            LOBEntityDetailCode = UserContext.BelongingEntityDetailCode;
                        }
                    }
                    if (accessControlEntityId <= 0)
                    {
                        Gep.Cumulus.Partner.Entities.UserContext userContext;
                        try
                        {
                            var partnerHelper = new RESTAPIHelper.PartnerHelper(this.UserContext, JWTToken);
                            userContext = partnerHelper.GetUserContextDetailsByContactCode(contactCode);
                        }
                        catch (Exception ex)
                        {
                            // Log Exception here
                            LogHelper.LogError(Log, "Error occurred in SetDocumentAdditionalEntitiesDetails method in ReceiptManager.", ex);
                            throw;
                        }

                        if ((docType == P2PDocumentType.Requisition) || (docType == P2PDocumentType.Program))
                        {
                            accessControlEntityId = UserContext.GetBelongingAccessControlEntityId(userContext);
                            LOBEntityDetailCode = userContext.GetDefaultBelongingUserLOBMapping().EntityDetailCode;
                        }
                        else
                        {
                            if (userContext.TypeOfUser != User.UserType.Supplier)
                            {
                                accessControlEntityId = UserContext.GetServingAccessControlEntityId(userContext);
                                LOBEntityDetailCode = userContext.GetDefaultServingUserLOBMapping().EntityDetailCode;
                            }
                        }
                    }
                    if (orgDetail == null)
                    {
                        var lstBUDetails = GetContactORGMapping(contactCode, accessControlEntityId);
                        defaultBU = lstBUDetails.Where(data => data.IsDefault).FirstOrDefault();
                    }
                    else
                        defaultBU = orgDetail;


                    List<SplitAccountingFields> lstSplitConfiguration = GetAllSplitAccountingFields(docType, levelType);
                    int headerEntityId = 0;

                    if (lstSplitConfiguration != null && lstSplitConfiguration.Count > 0)
                    {
                        foreach (var splitConfiguration in lstSplitConfiguration)
                        {
                            headerEntityId = splitConfiguration.EntityTypeId;
                            if (headerEntityId == accessControlEntityId)
                            {
                                if (ReferenceEquals(lstDocumentAdditionalEntityInfo, null))
                                    lstDocumentAdditionalEntityInfo = new Collection<DocumentAdditionalEntityInfo>();
                                lstDocumentAdditionalEntityInfo.Add(new DocumentAdditionalEntityInfo()
                                {
                                    EntityDetailCode = splitConfiguration.PopulateDefault ? defaultBU.OrgEntityCode : 0,
                                    EntityId = accessControlEntityId,
                                    EntityDisplayName = splitConfiguration.PopulateDefault ? defaultBU.EntityDescription : "",
                                    EntityCode = defaultBU.EntityCode != null ? defaultBU.EntityCode : ""
                                });
                            }
                            else
                            {
                                if (splitConfiguration.PopulateDefault && defaultBU != null)
                                {
                                    ProxyOrganizationStructureService objOrgProxy = new ProxyOrganizationStructureService(this.UserContext, this.JWTToken);
                                    try
                                    {

                                        var objSearch = new OrganizationStructure.Entities.OrgSearch
                                        {
                                            AssociationTypeInfo = OrganizationStructure.Entities.AssociationType.Both,
                                            OrgEntityCode = defaultBU.OrgEntityCode,
                                            objEntityType = new OrganizationStructure.Entities.OrgEntityType { },
                                            LOBEntityDetailCode = LOBEntityDetailCode
                                        };

                                        OrgSearchResult entityDetail;
                                        var executionHelper = new GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.ExecutionHelper(this.UserContext, this.GepConfiguration, this.JWTToken);
                                        if (executionHelper.Check(19, Req.BusinessObjects.RESTAPIHelper.ExecutionHelper.WebAPIType.Common))
                                        {
                                            var csmHelper = new RESTAPIHelper.CSMHelper(UserContext, JWTToken);
                                            var body = new
                                            {
                                                OrgEntityCode = defaultBU.OrgEntityCode,
                                                EntityId = 0,
                                                AssociationTypeInfo = (int)OrganizationStructure.Entities.AssociationType.Both,
                                                LOBEntityDetailCode = LOBEntityDetailCode
                                            };
                                            entityDetail = csmHelper.GetSearchedEntityDetails(body).ToList().FirstOrDefault();
                                        }
                                        else
                                        {
                                            entityDetail = objOrgProxy.GetEntitySearchResults(objSearch).ToList().FirstOrDefault();
                                        }

                                        if (entityDetail != null)
                                        {
                                            OrgSearchResult parentEntityDetail;
                                            if (executionHelper.Check(20, Req.BusinessObjects.RESTAPIHelper.ExecutionHelper.WebAPIType.Common))
                                            {
                                                var csmHelper = new RESTAPIHelper.CSMHelper(UserContext, JWTToken);
                                                var body = new
                                                {
                                                    OrgEntityCode = entityDetail.ParentOrgEntityCode,
                                                    EntityId = 0,
                                                    AssociationTypeInfo = (int)OrganizationStructure.Entities.AssociationType.Both,
                                                    LOBEntityDetailCode = LOBEntityDetailCode
                                                };
                                                parentEntityDetail = csmHelper.GetSearchedEntityDetails(body).ToList().FirstOrDefault();
                                            }
                                            else
                                            {
                                                objSearch.OrgEntityCode = entityDetail.ParentOrgEntityCode;
                                                //Get the Parent Entity Details till it matches the Heder configuration Entity type
                                                parentEntityDetail = objOrgProxy.GetEntitySearchResults(objSearch).ToList().FirstOrDefault();
                                            }
                                            
                                            while (parentEntityDetail != null && parentEntityDetail.EntityId != headerEntityId)
                                            {
                                                if (executionHelper.Check(21, Req.BusinessObjects.RESTAPIHelper.ExecutionHelper.WebAPIType.Common))
                                                {
                                                    var csmHelper = new RESTAPIHelper.CSMHelper(UserContext, JWTToken);
                                                    var body = new
                                                    {
                                                        OrgEntityCode = parentEntityDetail.ParentOrgEntityCode,
                                                        EntityId = 0,
                                                        AssociationTypeInfo = (int)OrganizationStructure.Entities.AssociationType.Both,
                                                        LOBEntityDetailCode = LOBEntityDetailCode
                                                    };
                                                    parentEntityDetail = csmHelper.GetSearchedEntityDetails(body).ToList().FirstOrDefault();
                                                }
                                                else
                                                {
                                                    objSearch.OrgEntityCode = parentEntityDetail.ParentOrgEntityCode;
                                                    parentEntityDetail = objOrgProxy.GetEntitySearchResults(objSearch).ToList().FirstOrDefault();
                                                }
                                            }

                                            //Get Top 1 Heder configuration Entity details if its not available after traversing hierarchy
                                            if (parentEntityDetail == null)
                                            {
                                                long parentEntityDetailCode = 0;
                                                if (!ReferenceEquals(lstDocumentAdditionalEntityInfo, null) && lstDocumentAdditionalEntityInfo.Count() > 0)
                                                    parentEntityDetailCode = lstDocumentAdditionalEntityInfo.Where(p => p.EntityId == splitConfiguration.ParentSplitAccountingFieldId).FirstOrDefault().EntityDetailCode;

                                                if (executionHelper.Check(22, Req.BusinessObjects.RESTAPIHelper.ExecutionHelper.WebAPIType.Common))
                                                {
                                                    var csmHelper = new RESTAPIHelper.CSMHelper(UserContext, JWTToken);
                                                    var body = new
                                                    {
                                                        OrgEntityCode = 0,
                                                        ParentOrgEntityCode = parentEntityDetailCode,
                                                        EntityId = headerEntityId,
                                                        AssociationTypeInfo = (int)OrganizationStructure.Entities.AssociationType.Both,
                                                        LOBEntityDetailCode = LOBEntityDetailCode
                                                    };
                                                    parentEntityDetail = csmHelper.GetSearchedEntityDetails(body).ToList().FirstOrDefault();
                                                }
                                                else
                                                {
                                                    objSearch.OrgEntityCode = 0;
                                                    objSearch.objEntityType = new OrganizationStructure.Entities.OrgEntityType { EntityId = headerEntityId };
                                                    objSearch.ParentOrgEntityCode = parentEntityDetailCode;
                                                    parentEntityDetail = objOrgProxy.GetEntitySearchResults(objSearch).ToList().FirstOrDefault();
                                                }
                                            }

                                            if (parentEntityDetail != null && parentEntityDetail.OrgEntityCode > 0)
                                            {
                                                if (ReferenceEquals(lstDocumentAdditionalEntityInfo, null))
                                                    lstDocumentAdditionalEntityInfo = new Collection<DocumentAdditionalEntityInfo>();
                                                lstDocumentAdditionalEntityInfo.Add(new DocumentAdditionalEntityInfo()
                                                {
                                                    EntityDetailCode = parentEntityDetail.OrgEntityCode,
                                                    EntityId = parentEntityDetail.EntityId,
                                                    EntityDisplayName = parentEntityDetail.DisplayName,
                                                    EntityCode = parentEntityDetail.EntityCode == null ? "" : parentEntityDetail.EntityCode
                                                });
                                            }

                                        }
                                    }
                                    finally { }
                                }
                                else
                                {
                                    if (ReferenceEquals(lstDocumentAdditionalEntityInfo, null))
                                        lstDocumentAdditionalEntityInfo = new Collection<DocumentAdditionalEntityInfo>();
                                    lstDocumentAdditionalEntityInfo.Add(new DocumentAdditionalEntityInfo()
                                    {
                                        EntityDetailCode = 0,
                                        EntityId = splitConfiguration.EntityTypeId,
                                        EntityDisplayName = "",
                                        EntityCode = ""
                                    });
                                }
                            }
                        }
                    }
                    GEPDataCache.PutInCacheJSON<Collection<DocumentAdditionalEntityInfo>>(key, UserContext.BuyerPartnerCode, contactCode, "en-US", lstDocumentAdditionalEntityInfo);
                }
                catch (Exception ex)
                {
                    GEPDataCache.RemoveFromCache(key, contactCode, "en-US");
                    throw;
                }
            }
            return lstDocumentAdditionalEntityInfo;
        }


        private void DeleteLineItemsBasedOnOrgEntity(P2PDocumentType docType, P2PDocument obj, bool allowOrgEntityInCatalogItems)
        {
            try
            {
                var commonManager = new RequisitionCommonManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
                var settingdetails = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.None, UserContext.ContactCode, (int)SubAppCodes.P2P, "", obj.EntityDetailCode.FirstOrDefault());
                int maxPrecessionValue = convertStringToInt(commonManager.GetSettingsValueByKey(settingdetails, "MaxPrecessionValue"));
                int maxPrecessionValueTotal = convertStringToInt(commonManager.GetSettingsValueByKey(settingdetails, "MaxPrecessionValueforTotal"));
                int maxPrecessionValueForTaxAndCharges = convertStringToInt(commonManager.GetSettingsValueByKey(settingdetails, "MaxPrecessionValueForTaxesAndCharges"));

                var headerLevelOrgEntityId =
                    Convert.ToInt32(commonManager.GetSettingsValueByKey(P2PDocumentType.Catalog, "CorporationEntityId",
                                                                        (long)UserContext.ContactCode));
                //Get the Previous Header Level Entity Details
                /// BY: DEV_MohammedRafi_Order5251_P2PCodeBaseSeperation
                List<DocumentAdditionalEntityInfo> lstHeaderEntityDetails = new List<DocumentAdditionalEntityInfo>();

                lstHeaderEntityDetails = GetReqDao().GetAllDocumentAdditionalEntityInfo(obj.DocumentId);

                var documentAdditionalEntitiesInfoList = new Collection<DocumentAdditionalEntityInfo>();

                switch (docType)
                {
                    case P2PDocumentType.Requisition:
                        documentAdditionalEntitiesInfoList = ((Requisition)obj).DocumentAdditionalEntitiesInfoList;
                        break;
                    case P2PDocumentType.Order:
                        documentAdditionalEntitiesInfoList = ((Order)obj).DocumentAdditionalEntitiesInfoList;
                        break;

                }

                if (allowOrgEntityInCatalogItems)
                {
                    if (lstHeaderEntityDetails.Any())
                    {
                        foreach (var objLstHeaderEntityDetails in lstHeaderEntityDetails)
                        {
                            if (objLstHeaderEntityDetails.EntityId == headerLevelOrgEntityId)
                            {
                                if (documentAdditionalEntitiesInfoList != null)
                                    if (documentAdditionalEntitiesInfoList.Any(
                                        documentAdditionalEntityInfo =>
                                        documentAdditionalEntityInfo.EntityId == objLstHeaderEntityDetails.EntityId &&
                                        objLstHeaderEntityDetails.EntityDetailCode !=
                                        documentAdditionalEntityInfo.EntityDetailCode))
                                    {
                                        GetReqDao().DeleteLineItemsByOrgEntity(obj.DocumentId,
                                                                        objLstHeaderEntityDetails.
                                                                            EntityDetailCode, maxPrecessionValue, maxPrecessionValueTotal, maxPrecessionValueForTaxAndCharges);


                                        AddIntoSearchIndexerQueueing(obj.DocumentCode, (int)getDocumentType(docType));
                                    }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log Exception here
                LogHelper.LogError(Log, "Error occurred in DeleteLineItemsBasedOnOrgEntity method in RequisitionDocumentManager.", ex);
                throw;
            }
        }


        public Requisition GetAllRequisitionDetailsByRequisitionId(long requisitionId)
        {
            try
            {
                return (Requisition)GetReqDao().GetAllRequisitionDetailsByRequisitionId(requisitionId, this.UserContext.ContactCode, 0);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in EvaluateRulesForObject Method in RequisitionDocumentManager", ex);
                throw;
            }
        }


        public DocumentType getDocumentType(P2PDocumentType docType)
        {
            DocumentType objDocumentType;
            switch (docType)
            {
                case P2PDocumentType.Catalog: objDocumentType = DocumentType.Catelog; break;
                case P2PDocumentType.CreditMemo: objDocumentType = DocumentType.CreditMemo; break;
                case P2PDocumentType.Invoice: objDocumentType = DocumentType.Invoice; break;
                case P2PDocumentType.InvoiceReconciliation: objDocumentType = DocumentType.InvoiceReconcillation; break;
                case P2PDocumentType.Item: objDocumentType = DocumentType.Item; break;
                case P2PDocumentType.Order: objDocumentType = DocumentType.PO; break;
                case P2PDocumentType.Program: objDocumentType = DocumentType.Program; break;
                case P2PDocumentType.Receipt: objDocumentType = DocumentType.Receipts; break;
                case P2PDocumentType.Requisition: objDocumentType = DocumentType.Requisition; break;
                case P2PDocumentType.ReturnNote: objDocumentType = DocumentType.ReturnNote; break;
                case P2PDocumentType.PaymentRequest: objDocumentType = DocumentType.PaymentRequest; break;
                case P2PDocumentType.ServiceConfirmation: objDocumentType = DocumentType.ServiceConfirmation; break;
                case P2PDocumentType.ASN: objDocumentType = DocumentType.ASN; break;
                case P2PDocumentType.ScannedImage: objDocumentType = DocumentType.ScannedImage; break;
                case P2PDocumentType.BlanketOrder: objDocumentType = DocumentType.Blanket; break;//This Document refers to Contract Blanket Document
                default: objDocumentType = DocumentType.None; break;
            }
            return objDocumentType;
        }


        public void UpdatePeriodbyNeedbyDate(P2PDocumentType docType, long documentItemId)
        {
            var commonManager = new RequisitionCommonManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
            bool? IsPeriodbasedonNeedbyDate = Convert.ToBoolean(commonManager.GetSettingsValueByKey(docType, "IsPeriodbasedonNeedbyDate", commonManager.UserContext.ContactCode, 107));
            bool? IsOBEnabled = Convert.ToBoolean(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "IsOperationalBudgetEnabled", commonManager.UserContext.ContactCode, 107));

            #region  Updates splits "Period" entity based on the dateneeded for material and startdate for services for Orders
            if ((docType == P2PDocumentType.Requisition || docType == P2PDocumentType.Order || docType == P2PDocumentType.Invoice || docType == P2PDocumentType.PaymentRequest) && documentItemId > 0 && (IsPeriodbasedonNeedbyDate.HasValue && IsPeriodbasedonNeedbyDate.Value == true) && (IsOBEnabled.HasValue && IsOBEnabled.Value == true))
            {
                var currentPrincipal = System.Threading.Thread.CurrentPrincipal;
                Task.Factory.StartNew((_userContext) =>
                {
                    System.Threading.Thread.CurrentPrincipal = currentPrincipal;
                    bool result = GetCommonDao().UpdatePeriodonSplitsByNeedbyDate((int)docType, documentItemId, IsPeriodbasedonNeedbyDate.Value);
                }, this.UserContext);
            }
            #endregion
        }


        public void UpdateAllPeriodbyNeedbyDate(P2PDocumentType docType, long documentId, bool IsPeriodbasedonNeedbyDate)
        {
            #region  Updates splits "Period" entity based on the dateneeded for material and startdate for services for Orders
            if ((docType == P2PDocumentType.Requisition || docType == P2PDocumentType.Order || docType == P2PDocumentType.Invoice || docType == P2PDocumentType.PaymentRequest) && documentId > 0)
            {
                GetCommonDao().UpdateAllPeriodonSplitsByNeedbyDate((int)docType, documentId, IsPeriodbasedonNeedbyDate);
            }
            #endregion
        }

        public List<UserDefinedApproval> GetUserDefinedApproversList(long documentCode, int wfDocTypeId)
        {
            string responseResult = string.Empty;
            List<UserDefinedApproval> userDefinedApproval = new List<UserDefinedApproval>();
            try
            {
                string serviceurl;
                serviceurl = UrlHelperExtensions.WorkFlowRestUrl + "/GetUserDefinedApprovers";
                CreateHttpWebRequest(serviceurl);
                Dictionary<string, object> odict = new Dictionary<string, object>();
                odict.Add("instanceId", "");
                odict.Add("wfOrderId", 0);
                odict.Add("documentCode", documentCode);
                odict.Add("wfDocTypeId", wfDocTypeId);
                responseResult = GetHttpWebResponse(odict);

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                userDefinedApproval = serializer.Deserialize<List<UserDefinedApproval>>(responseResult);
            }

            catch (CommunicationException ex)
            {
                LogHelper.LogError(Log, "Error occurred in GetUserDefinedApprovers Method in RequisitionDocumentManager", ex);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in GetUserDefinedApprovers Method in RequisitionDocumentManager", ex);
                throw;
            }
            return userDefinedApproval;
        }

        public List<UserDefinedApproval> UpdateUserDefinedApprovers(string instanceId, long wfOrderId, long documentCode, int wfDocTypeId, List<UserDefinedApproval> userDefinedApproval)
        {
            string responseResult = string.Empty;
            try
            {
                string serviceurl;
                serviceurl = UrlHelperExtensions.WorkFlowRestUrl + "/UpdateUserDefinedApprovers";
                CreateHttpWebRequest(serviceurl, "PATCH");

                Dictionary<string, object> odict = new Dictionary<string, object>();
                odict.Add("instanceId", instanceId);
                odict.Add("wfOrderId", wfOrderId);
                odict.Add("documentCode", documentCode);
                odict.Add("wfDocTypeId", wfDocTypeId);
                odict.Add("userDefinedApproval", userDefinedApproval);
                responseResult = GetHttpWebResponse(odict);

                serviceurl = UrlHelperExtensions.WorkFlowRestUrl + "/GetUserDefinedApprovers";
                CreateHttpWebRequest(serviceurl);

                odict = new Dictionary<string, object>();
                odict.Add("instanceId", instanceId);
                odict.Add("wfOrderId", wfOrderId);
                odict.Add("documentCode", documentCode);
                odict.Add("wfDocTypeId", wfDocTypeId);
                responseResult = GetHttpWebResponse(odict);
                return new JavaScriptSerializer().Deserialize<List<UserDefinedApproval>>(responseResult);
            }
            catch (CommunicationException ex)
            {
                LogHelper.LogError(Log, "Error occurred in UpdateUserDefinedApprovers Method in RequisitionDocumentManager", ex);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in UpdateUserDefinedApprovers Method in RequisitionDocumentManager", ex);
                throw;
            }
            return new List<UserDefinedApproval>();
        }


        public bool ResetAllApprovers(long documentCode, int wfDocTypeId, string approvalStatus)
        {
            string responseResult = string.Empty;
            try
            {
                string serviceurl;
                serviceurl = UrlHelperExtensions.WorkFlowRestUrl + "/ResetAllApprovers";
                CreateHttpWebRequest(serviceurl);

                Dictionary<string, object> odict = new Dictionary<string, object>();
                odict.Add("documentCode", documentCode);
                odict.Add("wfDocTypeId", wfDocTypeId);
                odict.Add("approvalStatus", approvalStatus);
                responseResult = GetHttpWebResponse(odict);
                return new JavaScriptSerializer().Deserialize<bool>(responseResult);
            }
            catch (CommunicationException ex)
            {
                LogHelper.LogError(Log, "Error occurred in ResetAllApprovers Method in RequisitionDocumentManager", ex);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in ResetAllApprovers Method in RequisitionDocumentManager", ex);
                throw;
            }
            return false;
        }

        private bool getSupplierCurrencySetting()
        {
            var commonManager = new RequisitionCommonManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
            string masterSupplierSetting = commonManager.GetSettingsValueByKey(P2PDocumentType.None, "IsRestrictCurrencyList", UserContext.ContactCode);
            bool IsSupplierCurrency = false;
            if (masterSupplierSetting == "Supplier")
            {
                IsSupplierCurrency = true;
            }
            return IsSupplierCurrency;
        }

        public bool SaveNotesAndAttachments(P2PDocumentType docType, Entities.ReqNotesOrAttachments reqNotesAndAttachments)
        {
            var notesAndAttachments = MapReqNotesOrAttachments(reqNotesAndAttachments);
            return GetReqDao().SaveNotesAndAttachments(notesAndAttachments);
        }

        public List<Entities.ReqNotesOrAttachments> GetNotesAndAttachments(byte level, long requisionId)
        {
            var ReqNotesOrAttachments = new List<Entities.ReqNotesOrAttachments>();
            var result = GetReqDao().GetNotesAndAttachments(level, requisionId, (byte)(UserContext.IsSupplier ? 2 : 1));
            foreach (var notesOrAttachment in result)
            {
                ReqNotesOrAttachments.Add(this.MapNotesOrAttachments(notesOrAttachment));
            }

            return ReqNotesOrAttachments;
        }

        public bool DeleteNotesAndAttachments(byte level, List<long> notesOrAttachmentsIds)
        {
            return GetReqDao().DeleteNotesAndAttachments(level, notesOrAttachmentsIds);
        }

        internal NotesOrAttachments MapReqNotesOrAttachments(Entities.ReqNotesOrAttachments reqNotesAndAttachments)
        {
            var notesAndAttachments = new NotesOrAttachments();
            if (reqNotesAndAttachments != null)
            {
                notesAndAttachments = new NotesOrAttachments()
                {
                    AccessTypeId = reqNotesAndAttachments.AccessTypeId,
                    CategoryTypeId = reqNotesAndAttachments.CategoryTypeId,
                    CategoryTypeName = reqNotesAndAttachments.CategoryTypeName,
                    CreatedBy = reqNotesAndAttachments.CreatedBy,
                    CreatorName = reqNotesAndAttachments.CreatorName,
                    DateCreated = reqNotesAndAttachments.DateCreated,
                    DocumentCode = reqNotesAndAttachments.DocumentCode,
                    DocumentType = reqNotesAndAttachments.DocumentType,
                    FileId = string.IsNullOrEmpty(reqNotesAndAttachments.EncryptedFileId) ? 0 : EncryptionHelper.Decrypt(reqNotesAndAttachments.EncryptedFileId, this.UserContext.ContactCode),
                    FilePath = reqNotesAndAttachments.FilePath,
                    FileSize = reqNotesAndAttachments.FileSize,
                    FileUri = reqNotesAndAttachments.FileUri,
                    IsEditable = reqNotesAndAttachments.IsEditable,
                    LineItemId = reqNotesAndAttachments.LineItemId,
                    ModifiedBy = reqNotesAndAttachments.ModifiedBy,
                    ModifiedDate = reqNotesAndAttachments.ModifiedDate,
                    NoteOrAttachmentDescription = reqNotesAndAttachments.NoteOrAttachmentDescription,
                    NoteOrAttachmentName = reqNotesAndAttachments.NoteOrAttachmentName,
                    NoteOrAttachmentType = reqNotesAndAttachments.NoteOrAttachmentType,
                    NoteOrAttachmentTypeName = reqNotesAndAttachments.NoteOrAttachmentTypeName,
                    NotesOrAttachmentId = reqNotesAndAttachments.NotesOrAttachmentId,
                    P2PLineItemID = reqNotesAndAttachments.P2PLineItemID,
                    SourceType = reqNotesAndAttachments.SourceType
                };
            }
            return notesAndAttachments;
        }

        internal Entities.ReqNotesOrAttachments MapNotesOrAttachments(NotesOrAttachments notesAndAttachments)
        {
            var reqNotesAndAttachments = new Entities.ReqNotesOrAttachments();
            if (notesAndAttachments != null)
            {
                reqNotesAndAttachments = new Entities.ReqNotesOrAttachments()
                {
                    AccessTypeId = notesAndAttachments.AccessTypeId,
                    CategoryTypeId = notesAndAttachments.CategoryTypeId,
                    CategoryTypeName = notesAndAttachments.CategoryTypeName,
                    CreatedBy = notesAndAttachments.CreatedBy,
                    CreatorName = notesAndAttachments.CreatorName,
                    DateCreated = notesAndAttachments.DateCreated,
                    DocumentCode = notesAndAttachments.DocumentCode,
                    DocumentType = notesAndAttachments.DocumentType,
                    EncryptedFileId = (notesAndAttachments.FileId != null && notesAndAttachments.FileId > 0) ? EncryptionHelper.Encrypt(notesAndAttachments.FileId, this.UserContext.ContactCode) : string.Empty,
                    FilePath = notesAndAttachments.FilePath,
                    FileSize = notesAndAttachments.FileSize,
                    FileUri = notesAndAttachments.FileUri,
                    IsEditable = notesAndAttachments.IsEditable,
                    LineItemId = notesAndAttachments.LineItemId,
                    ModifiedBy = notesAndAttachments.ModifiedBy,
                    ModifiedDate = notesAndAttachments.ModifiedDate,
                    NoteOrAttachmentDescription = notesAndAttachments.NoteOrAttachmentDescription,
                    NoteOrAttachmentName = notesAndAttachments.NoteOrAttachmentName,
                    NoteOrAttachmentType = notesAndAttachments.NoteOrAttachmentType,
                    NoteOrAttachmentTypeName = notesAndAttachments.NoteOrAttachmentTypeName,
                    NotesOrAttachmentId = notesAndAttachments.NotesOrAttachmentId,
                    P2PLineItemID = notesAndAttachments.P2PLineItemID,
                    SourceType = notesAndAttachments.SourceType
                };
            }
            return reqNotesAndAttachments;
        }

        #region Pub-Pusher Start
        public void BroadcastPusher(long DocumentCode, DocumentType _DocumentType, string eventname, string message, UserExecutionContext _objuserExecutionContext = null)
        {
            PusherPubHelper obj = new PusherPubHelper();
            var userExecutioncontext = _objuserExecutionContext == null ? UserContext : _objuserExecutionContext;
            var currentPrincipal = System.Threading.Thread.CurrentPrincipal;
            Task.Factory.StartNew((self) =>
            {
                System.Threading.Thread.CurrentPrincipal = currentPrincipal;                
                try
                {
                    obj.BroadcastPusher(DocumentCode, _DocumentType, eventname, message, (UserExecutionContext)self);
                }
                catch (Exception ex)
                {
                    LogHelper.LogError(Log, string.Format("BroadcastPusher  :-> DocumentCode={0} , _DocumentType={1}, message{2}, eventname{3}, Stack Trace = {4}, userExecutioncontext ={5}", DocumentCode, _DocumentType, message, eventname, userExecutioncontext.ToJSON(), ex.StackTrace != null ? ex.StackTrace.ToString() : "null"), ex);
                }
            }, userExecutioncontext);
        }

        public void BroadcastPusher(long documentCode, DocumentType documentType, string eventName, object message, UserExecutionContext userExecutionContext = null)
        {
            PusherPubHelper hubPusher = new PusherPubHelper();
            var userExecutioncontext = userExecutionContext == null ? UserContext : userExecutionContext;
            var currentPrincipal = System.Threading.Thread.CurrentPrincipal;
            Task.Factory.StartNew((self) =>
            {
                System.Threading.Thread.CurrentPrincipal = currentPrincipal;
                try
                {
                    hubPusher.BroadcastPusher(documentCode, documentType, eventName, message, (UserExecutionContext)self);
                }
                catch (Exception ex)
                {
                    LogHelper.LogError(Log, string.Format("BroadcastPusher  :-> DocumentCode={0} , _DocumentType={1}, message{2}, eventname{3}, Stack Trace = {4}, userExecutioncontext ={5}", documentCode, documentType, message.ToJSON(), eventName, userExecutioncontext.ToJSON(), ex.StackTrace != null ? ex.StackTrace.ToString() : "null"), ex);
                }
            }, userExecutioncontext);
        }

        #endregion Pub-Pusher End

        public Dictionary<string, string> SendDocumentForReview(long documentCode, long contactCode, int documentTypeId, bool overrideAllowAsyncMethods = false, bool isBypassOperationalBudget = false)
        {

            #region Debug Logging

            if (Log.IsDebugEnabled)
            {
                Log.Debug(string.Concat("In SendDocumentForReview Method in RequisitionDocumentManager  ", " with parameters: documentCode =" + documentCode + "contactCode = " + contactCode + ", documentTypeId=" + documentTypeId));
            }

            #endregion

            string serviceurl;
            Requisition objReq = null;
            Dictionary<string, string> result = new Dictionary<string, string>();

            try
            {

                RequisitionCommonManager commonManager = new RequisitionCommonManager(JWTToken) { GepConfiguration = this.GepConfiguration, UserContext = this.UserContext };
                RequisitionManager requisitionManager = new RequisitionManager(JWTToken) { GepConfiguration = this.GepConfiguration, UserContext = this.UserContext };
                long LOBId = GetCommonDao().GetLOBByDocumentCode(documentCode);
                bool allowAsyncMethods = Convert.ToBoolean(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "AllowAsyncMethods", commonManager.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBId));
                bool hasRules = GetSQLP2PDocumentDAO().CheckHasRules(BusinessCase.Review, documentTypeId);

                if (overrideAllowAsyncMethods) { allowAsyncMethods = false; }

                if (documentTypeId == (int)DocumentType.Requisition)
                {
                    string enableCapitalBudgetSetting = commonManager.GetSettingsValueByKey(P2PDocumentType.None, "EnableCapitalBudget", UserContext.ContactCode, 107, "", LOBId);
                    bool enableCapitalBudget = false;
                    bool consumption = false;
                    var resultConsume = "";
                    if (!string.IsNullOrEmpty(enableCapitalBudgetSetting) && enableCapitalBudgetSetting.ToLower() == "yes")
                        enableCapitalBudget = true;

                    if (enableCapitalBudget)
                    {
                        resultConsume = requisitionManager.ConsumeCapitalBudget(documentCode, LOBId, out consumption);
                        if (consumption && !(string.IsNullOrEmpty(resultConsume)))
                        {
                            result.Add("SendForReviewResult", resultConsume);
                            return result;
                        }
                    }
                    if (hasRules)
                    {
                        result = SendRequisitionForReviewValidations(documentCode, isBypassOperationalBudget);
                        if (result.Count == 0)
                        {
                            var catalogItemSourcesSetting = commonManager.GetSettingsValueByKey(P2PDocumentType.Requisition, "CatalogItemSources", this.UserContext.ContactCode, (int)SubAppCodes.P2P, string.Empty, this.UserContext.GetLOBEntityId());
                            var settings = new Dictionary<string, string>();
                            settings.Add("CatalogItemSources", catalogItemSourcesSetting);

                            objReq = (Requisition)GetReqDao().GetAllRequisitionDetailsByRequisitionId(documentCode, this.UserContext.ContactCode, 0, null, settings);

                            if (objReq.CustomAttrFormId > 0 || objReq.CustomAttrFormIdForItem > 0 || objReq.CustomAttrFormIdForSplit > 0)
                            {
                                objReq.ListQuestionResponse = GetQuestionResponse(documentTypeId, documentCode, objReq.CustomAttrFormId, objReq.CustomAttrFormIdForItem, objReq.CustomAttrFormIdForSplit, ((System.Collections.IEnumerable)objReq.RequisitionItems).Cast<object>().ToList(), true);
                                objReq.RequisitionItems.ForEach(items => items.ListQuestionResponse = objReq.ListQuestionResponse.Where(w => w.ObjectInstanceId == items.DocumentItemId).ToList());
                            }
                            /* commenting this call as the setting is not configured */
                            //objReq.lstUserQuestionResponse = GetUserQuestionresponse(objReq.LOBEntity, objReq.OnBehalfOf > 0 ? objReq.OnBehalfOf : UserContext.ContactCode);


                            SettingDetails invSettings = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.Invoice, UserContext.ContactCode, 107);
                            bool IsREOptimizationEnabled = Convert.ToBoolean(commonManager.GetSettingsValueByKey(invSettings, "IsREOptimizationEnabled"));

                            //ProxyInvoiceService proxyInvoice = new ProxyInvoiceService(UserContext);
                            //List<RuleAction> actions = proxyInvoice.EvaluateRulesForObject(BusinessCase.Review, objReq, IsREOptimizationEnabled);

                            List<RuleAction> actions = GetCommonDao().EvaluateRulesForObject(BusinessCase.Review, objReq, IsREOptimizationEnabled);

                            List<ParameterOutput> parameters = new List<ParameterOutput>();
                            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                            bool enableOBOReview = false;
                            bool hasOBOReviewRule = false;
                            bool hasHRManagerReviewRule = false;

                            if (actions != null && actions.Count > 0)
                            {
                                if (objReq.OnBehalfOf > 0)
                                {
                                    enableOBOReview = true;
                                }
                                foreach (RuleAction action in actions)
                                {
                                    parameters.AddRange(javaScriptSerializer.Deserialize<List<ParameterOutput>>(action.Parameters));
                                }

                                if (parameters.Any(parms => parms.Name == "OBOReviewer"))
                                {
                                    hasOBOReviewRule = true;
                                    actions = updateReviewActionParameters(actions, objReq, enableOBOReview);
                                    if (!enableOBOReview)
                                    {
                                        parameters = new List<ParameterOutput>();
                                        foreach (RuleAction action in actions)
                                        {
                                            parameters.AddRange(javaScriptSerializer.Deserialize<List<ParameterOutput>>(action.Parameters));
                                        }
                                    }
                                }
                                if (parameters.Any(parms => parms.Name == "HRManagerReview"))
                                {   
                                    actions = updateReviewActionParametersForHRManager(actions, objReq, out hasHRManagerReviewRule);
                                    parameters = new List<ParameterOutput>();
                                    foreach (RuleAction action in actions)
                                    {
                                        parameters.AddRange(javaScriptSerializer.Deserialize<List<ParameterOutput>>(action.Parameters));
                                    }
                                }
                                if (parameters.Any(parms => parms.Name == "AdhocReviewer"))
                                {
                                    actions = updateAdhocReviewActionParameters(actions);
                                    parameters = new List<ParameterOutput>();
                                    foreach (RuleAction action in actions)
                                    {
                                        parameters.AddRange(javaScriptSerializer.Deserialize<List<ParameterOutput>>(action.Parameters));
                                    }
                                }

                                if (parameters.Any(parms => parms.Name == "GroupId"))
                                {
                                    List<Contact> groupContacts = new List<Contact>();

                                    List<long> groupIdList = parameters.Where(parms => parms.Name == "GroupId").Select(grpid => grpid.Value).Distinct().Select(long.Parse).ToList();
                                    string groupIds = string.Join(",", groupIdList);

                                    if (!(enableOBOReview && hasOBOReviewRule) && !hasHRManagerReviewRule)
                                    {
                                        ProxyPartnerService proxyPartnerService = new ProxyPartnerService(UserContext, this.JWTToken);

                                        groupContacts = proxyPartnerService.GetContactsForGroup(groupIds);

                                    }

                                    if (groupContacts.Count > 0 || (enableOBOReview && hasOBOReviewRule)|| hasHRManagerReviewRule)
                                    {
                                        if (allowAsyncMethods)
                                        {
                                            var currentPrincipal = System.Threading.Thread.CurrentPrincipal;
                                            Task.Factory.StartNew(() =>
                                            {
                                                System.Threading.Thread.CurrentPrincipal = currentPrincipal;
                                                string instanceId = string.Empty;
                                                try
                                                {
                                                    WorkflowInputEntities workflowInputEntities = new WorkflowInputEntities();
                                                    workflowInputEntities.RuleAction = actions;

                                                    serviceurl = MultiRegionConfig.GetConfig(CloudConfig.WorkFlowRestURL) + "/InvokeWorkFlow";

                                                    CreateHttpWebRequest(serviceurl);

                                                    Dictionary<string, object> odict = new Dictionary<string, object>();
                                                    odict.Add("contactCode", contactCode);
                                                    odict.Add("documentCode", documentCode);
                                                    odict.Add("documentAmount", 0m);
                                                    odict.Add("documentTypeId", 41); /*workflowDocumentTypeId*/
                                                    odict.Add("eventName", "OnSubmit");
                                                    odict.Add("returnEntity", "");
                                                    odict.Add("wfInputParameter", workflowInputEntities);


                                                    instanceId = GetHttpWebResponse(odict);//InvokeWorkFlow method will always return instanceId
                                                }
                                                catch (Exception ex)
                                                {
                                                    LogHelper.LogError(Log, "Error occured in SendRequisitionForReview Method in RequisitionManager Error:" + ex.Message, ex);
                                                    instanceId = string.Empty;
                                                }
                                                finally
                                                {
                                                    NewRequisitionDAO objReqManager = new NewRequisitionDAO() { GepConfiguration = this.GepConfiguration, UserContext = this.UserContext };
                                                    if (!(string.IsNullOrEmpty(instanceId)))
                                                    {
                                                        objReqManager.UpdateLineStatusForRequisition(documentCode, (StockReservationStatus)(DocumentStatus.ReviewPending), true, null);
                                                        GetNewReqDao().UpdateRequisitionPreviousAmount(documentCode, true);
                                                        LogHelper.LogInfo(Log, "UpdateLineStatusForRequisition Method End : " + documentCode.ToString());

                                                    }
                                                    else
                                                    {
                                                        UpdateDocumentStatus(P2PDocumentType.Requisition, documentCode, DocumentStatus.SendForApprovalFailed, 0);
                                                        objReqManager.UpdateLineStatusForRequisition(documentCode, (StockReservationStatus)(DocumentStatus.SendForApprovalFailed), true, null);
                                                        if (enableCapitalBudget && consumption && (string.IsNullOrEmpty(resultConsume)))
                                                            requisitionManager.ReleaseCapitalBudget(documentCode);
                                                    }
                                                }

                                            });
                                            result.Add("SendForReviewResult", "SentForReview");
                                        }
                                        else
                                        {
                                            string instanceId = string.Empty;
                                            try
                                            {
                                                WorkflowInputEntities workflowInputEntities = new WorkflowInputEntities();
                                                workflowInputEntities.RuleAction = actions;

                                                serviceurl = MultiRegionConfig.GetConfig(CloudConfig.WorkFlowRestURL) + "/InvokeWorkFlow";

                                                CreateHttpWebRequest(serviceurl);

                                                Dictionary<string, object> odict = new Dictionary<string, object>();
                                                odict.Add("contactCode", contactCode);
                                                odict.Add("documentCode", documentCode);
                                                odict.Add("documentAmount", 0m);
                                                odict.Add("documentTypeId", 41); /*workflowDocumentTypeId*/
                                                odict.Add("eventName", "OnSubmit");
                                                odict.Add("returnEntity", "");
                                                odict.Add("wfInputParameter", workflowInputEntities);


                                                instanceId = GetHttpWebResponse(odict);//InvokeWorkFlow method will always return instanceId
                                            }
                                            catch (Exception ex)
                                            {
                                                LogHelper.LogError(Log, "Error occured in SendRequisitionForReview Method in RequisitionManager Error:" + ex.Message, ex);
                                                instanceId = string.Empty;
                                            }
                                            finally
                                            {
                                                NewRequisitionDAO objReqManager = new NewRequisitionDAO() { GepConfiguration = this.GepConfiguration, UserContext = this.UserContext };
                                                if (!(string.IsNullOrEmpty(instanceId)))
                                                {
                                                    objReqManager.UpdateLineStatusForRequisition(documentCode, (StockReservationStatus)(DocumentStatus.ReviewPending), true, null);
                                                    GetNewReqDao().UpdateRequisitionPreviousAmount(documentCode, true);
                                                    LogHelper.LogInfo(Log, "UpdateLineStatusForRequisition Method End : " + documentCode.ToString());
                                                    result.Add("SendForReviewResult", "SentForReview");
                                                }
                                                else
                                                {
                                                    result.Add("SendForReviewResult", "ErrorSendingForReview");
                                                    if (enableCapitalBudget && consumption && (string.IsNullOrEmpty(resultConsume)))
                                                        requisitionManager.ReleaseCapitalBudget(documentCode);
                                                }
                                            }
                                        }
                                        return result;
                                    }
                                }
                            }
                        }
                        else
                        {
                            return result;//Return result of budget validation
                        }
                    }
                    result.Add("SendForReviewResult", "NoReviewSetup");
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in SendDocumentForReview Method in RequisitionDocumentManager", ex);
                result.Add("SendForReviewResult", "ErrorSendingForReview");
            }

            return result;

        }

        public List<RuleAction> updateAdhocReviewActionParameters(List<RuleAction> actions)
        {
            int idxOfDeltaAmount = -1;
            for (int idx = 0; idx < actions.Count(); idx++)
            {
                RuleAction a = actions[idx];
                if (a.Action == ActionType.ReviewGroup)
                {
                    List<RuleParameter> ps = JsonConvert.DeserializeObject<List<RuleParameter>>(a.Parameters);
                    if (ps.Any(parms => parms.Name == "DeltaAmount"))
                    {
                        idxOfDeltaAmount = idx;
                    }
                }
            }
            if (idxOfDeltaAmount > -1)
            {
                actions.RemoveAt(idxOfDeltaAmount);
            }
            return actions;
        }

        public List<RuleAction> updateReviewActionParameters(List<RuleAction> actions, Requisition req, bool enableOBOReview = false)
        {
            int idxOfVBAReviewer = -1;
            for (int idx = 0; idx < actions.Count(); idx++)
            {
                RuleAction a = actions[idx];
                if (a.Action == ActionType.ReviewGroup)
                {
                    List<RuleParameter> ps = JsonConvert.DeserializeObject<List<RuleParameter>>(a.Parameters);
                    if (ps.Any(parms => parms.Name == "OBOReviewer"))
                    {
                        RuleParameter pa = ps.Where(e => e.Name.ToString().Trim().Equals("OBOReviewer")).FirstOrDefault();
                        if (pa.Value.ToString().Trim().ToUpper().Equals("TRUE"))
                        {
                            idxOfVBAReviewer = idx;
                            if (enableOBOReview)
                            {
                                RuleParameter p = ps.Where(parms => parms.Name == "GroupContactCode").FirstOrDefault();
                                p.Value = req.OnBehalfOf.ToString();
                                a.Parameters = JsonConvert.SerializeObject(ps);
                            }
                        }
                    }
                }
            }
            if (idxOfVBAReviewer > -1)
            {
                if (!enableOBOReview)
                {
                    actions.RemoveAt(idxOfVBAReviewer);
                }
            }
            return actions;
        }
        public List<RuleAction> updateReviewActionParametersForHRManager(List<RuleAction> actions, Requisition req, out bool hasHRManagerReviewRule)
        {
            hasHRManagerReviewRule = false;
            long result = 0;
            int idxOfHRManagerReviewer = -1;
            result = GetReqDao().GetContactsManagerMapping(req.OnBehalfOf == 0 ? req.RequesterId : req.OnBehalfOf);
            for (int idx = 0; idx < actions.Count(); idx++)
            {
                RuleAction a = actions[idx];
                if (a.Action == ActionType.ReviewGroup)
                {
                    List<RuleParameter> ps = JsonConvert.DeserializeObject<List<RuleParameter>>(a.Parameters);
                    if (ps.Any(parms => parms.Name == "HRManagerReview"))
                    {
                        RuleParameter pa = ps.Where(e => e.Name.ToString().Trim().Equals("HRManagerReview")).FirstOrDefault();
                        if (pa.Value.ToString().Trim().ToUpper().Equals("TRUE"))
                        {
                            idxOfHRManagerReviewer = idx;
                            RuleParameter p = ps.Where(parms => parms.Name == "GroupContactCode").FirstOrDefault();
                            p.Value = result.ToString();
                            a.Parameters = JsonConvert.SerializeObject(ps);
                        }                  
                    }
                }
            }
            if (idxOfHRManagerReviewer > -1)
            {
                if (result == 0)
                {
                    actions.RemoveAt(idxOfHRManagerReviewer);
                }
                else
                {
                    hasHRManagerReviewRule = true; 
                }
            }
            return actions;
        }

        private Dictionary<string, string> SendRequisitionForReviewValidations(long documentCode, bool isBypassOperationalBudget)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            long LOBId = GetCommonDao().GetLOBByDocumentCode(documentCode);
            bool flag = true;
            var commonManager = new RequisitionCommonManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };

            bool? IsOperationalbudgetPopupEnable = Convert.ToBoolean(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "IsOperationalbudgetPopupEnable", commonManager.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBId));
            bool? isOperationalBudgetEnabled = Convert.ToBoolean(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "IsOperationalBudgetEnabled", commonManager.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBId));

            DataTable flagForBudgetSplitAccountingValidation = new DataTable();

            flagForBudgetSplitAccountingValidation.TableName = "Validate Budget Accounting Details";

            if (!flag)
            {
                result.Add("IsCheckAccountingSplitValidations", "1");
            }
            else
            {
                if (this.UserContext.Product == GEPSuite.eInterface)
                {
                    int populateDefaultNeedByDateByDays = Convert.ToInt16(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "NeedByDateByDays", commonManager.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBId));
                    var lstValidationResult = ValidateDocumentByDocumentCode(P2PDocumentType.Requisition, documentCode, LOBId, false, false, populateDefaultNeedByDateByDays);
                    if (lstValidationResult.Count != 0)
                    {
                        foreach (string element in lstValidationResult)
                        {
                            if (element.Contains("Invalid Needed Date"))
                            {
                                result.Add("InvalidNeededDate", "1");
                                break;
                            }
                        }
                        if (result.Count == 0)
                            result.Add("IsValidateDocumentByDocumentCode", "1");
                    }
                }

                if ((!isBypassOperationalBudget) && isOperationalBudgetEnabled.HasValue && isOperationalBudgetEnabled.Value && result.Count() == 0)
                {
                    //bool AllowBudgetValidationOnOverAll = Convert.ToBoolean(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "AllowBudgetValidationOnOverAll", commonManager.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBId));
                    flagForBudgetSplitAccountingValidation = GetOperationalBudgetManager().ValidateBudgetSplitAccounting(documentCode, DocumentType.Requisition, false, LOBId);
                    if (IsOperationalbudgetPopupEnable.HasValue && IsOperationalbudgetPopupEnable.Value == true)
                    {
                        if (flagForBudgetSplitAccountingValidation.Rows.Count > 0)
                        {
                            System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                            List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
                            Dictionary<string, object> row;
                            foreach (DataRow dr in flagForBudgetSplitAccountingValidation.Rows)
                            {
                                row = new Dictionary<string, object>();
                                foreach (DataColumn col in flagForBudgetSplitAccountingValidation.Columns)
                                {
                                    row.Add(col.ColumnName, dr[col]);
                                }
                                rows.Add(row);
                            }
                            string jsonStringResult = serializer.Serialize(rows);
                            result.Add("IsValidateBudgetSplitAccounting", jsonStringResult);
                        }
                    }
                    else if (IsOperationalbudgetPopupEnable.HasValue && IsOperationalbudgetPopupEnable.Value == false)
                        GetNewReqDao().saveBudgetoryStatus(flagForBudgetSplitAccountingValidation, documentCode);
                }
            }

            return result;
        }

        private void AddAdhocReviewDetails(long documentCode, long contactCode, int documentTypeId)
        {
            #region Debug Logging

            if (Log.IsDebugEnabled)
            {
                Log.Debug(string.Concat("In AddAdhocReviewDetails Method in RequisitionDocumentManager  ", " with parameters: documentCode =" + documentCode + "contactCode = " + contactCode + ", documentTypeId=" + documentTypeId));
            }

            #endregion

            Requisition objReq = null;
            Dictionary<string, string> result = new Dictionary<string, string>();

            try
            {
                RequisitionCommonManager commonManager = new RequisitionCommonManager(JWTToken) { GepConfiguration = this.GepConfiguration, UserContext = this.UserContext };
                var catalogItemSourcesSetting = commonManager.GetSettingsValueByKey(P2PDocumentType.Requisition, "CatalogItemSources", this.UserContext.ContactCode, (int)SubAppCodes.P2P, string.Empty, this.UserContext.GetLOBEntityId());
                var settings = new Dictionary<string, string>();
                settings.Add("CatalogItemSources", catalogItemSourcesSetting);

                objReq = (Requisition)GetReqDao().GetAllRequisitionDetailsByRequisitionId(documentCode, this.UserContext.ContactCode, 0, null, settings);

                bool isNotOBO = false;
                if (contactCode != objReq.OnBehalfOf)
                {
                    isNotOBO = true;
                }

                if (documentTypeId == (int)DocumentType.Requisition && isNotOBO)
                {
                    bool hasRules = GetSQLP2PDocumentDAO().CheckHasRules(BusinessCase.Review, documentTypeId);
                    if (hasRules)
                    {
                        if (objReq.CustomAttrFormId > 0 || objReq.CustomAttrFormIdForItem > 0 || objReq.CustomAttrFormIdForSplit > 0)
                        {
                            objReq.ListQuestionResponse = GetQuestionResponse(documentTypeId, documentCode, objReq.CustomAttrFormId, objReq.CustomAttrFormIdForItem, objReq.CustomAttrFormIdForSplit, ((System.Collections.IEnumerable)objReq.RequisitionItems).Cast<object>().ToList(), true);
                            objReq.RequisitionItems.ForEach(items => items.ListQuestionResponse = objReq.ListQuestionResponse.Where(w => w.ObjectInstanceId == items.DocumentItemId).ToList());
                        }


                        SettingDetails invSettings = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.Invoice, UserContext.ContactCode, 107);
                        bool IsREOptimizationEnabled = Convert.ToBoolean(commonManager.GetSettingsValueByKey(invSettings, "IsREOptimizationEnabled"));



                        List<RuleAction> actions = GetCommonDao().EvaluateRulesForObject(BusinessCase.Review, objReq, IsREOptimizationEnabled);

                        List<ParameterOutput> parameters = new List<ParameterOutput>();
                        JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                        if (actions != null && actions.Count > 0)
                        {

                            foreach (RuleAction action in actions)
                            {
                                parameters.AddRange(javaScriptSerializer.Deserialize<List<ParameterOutput>>(action.Parameters));
                            }

                            if (parameters.Any(parms => parms.Name == "AdhocReviewer"))
                            {
                                for (int idx = 0; idx < actions.Count(); idx++)
                                {
                                    RuleAction a = actions[idx];
                                    if (a.Action == ActionType.ReviewGroup)
                                    {
                                        List<RuleParameter> ps = JsonConvert.DeserializeObject<List<RuleParameter>>(a.Parameters);
                                        RuleParameter deltaAmount = ps.Where(parms => parms.Name == "DeltaAmount").FirstOrDefault();
                                        RuleParameter deltaPercentage = ps.Where(parms => parms.Name == "DeltaPercentage").FirstOrDefault();
                                        if (deltaAmount != null || deltaPercentage != null)
                                        {
                                            bool percentage = (((objReq.RequisitionAmount - objReq.RequisitionPreviousAmount) / objReq.RequisitionPreviousAmount) * 100) >= Convert.ToDecimal(deltaPercentage.Value) ? true : false;

                                            if (((objReq.RequisitionAmount - objReq.RequisitionPreviousAmount) >= Convert.ToDecimal(deltaAmount.Value)) || (percentage))
                                            {
                                                AdhocReviewer adhocReviewer = new AdhocReviewer();
                                                adhocReviewer.DocumentId = documentCode;
                                                adhocReviewer.ContactCode = objReq.CreatedBy;
                                                adhocReviewer.Reviewer = (objReq.OnBehalfOf == 0 ? objReq.RequesterId : objReq.OnBehalfOf);
                                                adhocReviewer.WfdoctypeId = 41;
                                                adhocReviewer.RevieweType = (int)ReviewerType.AdhocReviewer;
                                                commonManager.SaveAdhocReview(adhocReviewer);
                                            }
                                        }
                                    }
                                }

                            }

                        }


                    }

                }
                GetNewReqDao().UpdateRequisitionPreviousAmount(documentCode, true);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in AddAdhocReviewDetails Method in RequisitionDocumentManager", ex);
            }


        }

        public string AcceptOrRejectReview(long documentCode, bool isApproved, int documentTypeId, long LOBId = 0)
        {
            var commonManager = new RequisitionCommonManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
            bool allowAsyncMethods = Convert.ToBoolean(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "AllowAsyncMethods", commonManager.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBId));

            if (isApproved)
                AddAdhocReviewDetails(documentCode, commonManager.UserContext.ContactCode, documentTypeId);
            else
                GetNewReqDao().UpdateRequisitionPreviousAmount(documentCode, false);

            if (!allowAsyncMethods)
            {
                #region Debug Logging
                long contactCode = commonManager.UserContext.ContactCode;

                if (Log.IsDebugEnabled)
                {
                    Log.Debug(string.Concat("In SendForApproval Method in ManageCommonController ",
                    " with parameters: documentCode = " + documentCode + ",contactCode =" + contactCode +
                    ",isApproved =" + isApproved + ",documentTypeId =" + documentTypeId));
                }
                #endregion
                string serviceurl;
                //int wfdocumentTypeId = documentTypeId;
                string result = null, resultValue = null;
                try
                {
                    serviceurl = MultiRegionConfig.GetConfig(CloudConfig.WorkFlowRestURL) + "/ReceiveNotification";

                    CreateHttpWebRequest(serviceurl);

                    Dictionary<string, object> odict = new Dictionary<string, object>();
                    odict.Add("contactCode", contactCode);
                    odict.Add("documentCode", documentCode);
                    odict.Add("isApproved", isApproved);
                    odict.Add("documentTypeId", 41);
                    result = GetHttpWebResponse(odict);
                    JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                    Dictionary<string, object> obj = (Dictionary<string, object>)javaScriptSerializer.DeserializeObject(result);
                    resultValue = obj["ReceiveNotificationResult"].ToString();

                    return resultValue;
                }
                catch (CommunicationException commFaultEx)
                {
                    LogHelper.LogError(Log, "Error occured in AcceptOrRejectReview Method in RequisitionDocumentManager", commFaultEx);
                }
                catch (Exception ex)
                {
                    LogHelper.LogError(Log, "Error occured in AcceptOrRejectReview Method in RequisitionDocumentManager", ex);
                    throw;
                }
                finally
                {
                    //if (resultValue != "True")
                    //    SendFailureNotification(documentCode, (DocumentType)documentTypeId, (UserExecutionContext)_userExecutionContext);           
                }
                return resultValue;
            }
            else
            {
                AcceptOrRejectReviewAsync(documentCode, isApproved, documentTypeId, LOBId);
                //Expecting always true value
                return "True";//Json("{\"ReceiveNotificationResult\":\"True\"}", JsonRequestBehavior.AllowGet);
            }
        }

        private string AcceptOrRejectReviewAsync(long documentCode, bool isApproved, int documentTypeId, long LOBId = 0)
        {

            var _userExecutionContext = this.UserContext;
            var commonManager = new RequisitionCommonManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
            long contactCode = commonManager.UserContext.ContactCode;
            var resultValue = "True";
            var currentPrincipal = System.Threading.Thread.CurrentPrincipal;
            Task.Factory.StartNew((self) =>
            {
                System.Threading.Thread.CurrentPrincipal = currentPrincipal;
                string serviceurl;
                string result = null;
                // int wfdocumentTypeId = documentTypeId;
                try
                {

                    FinalizeComments(P2PDocumentType.Requisition, documentCode);
                    serviceurl = MultiRegionConfig.GetConfig(CloudConfig.WorkFlowRestURL) + "/ReceiveNotification";
                    CreateHttpWebRequest(serviceurl, (UserExecutionContext)self);

                    Dictionary<string, object> odict = new Dictionary<string, object>();
                    odict.Add("contactCode", contactCode);
                    odict.Add("documentCode", documentCode);
                    odict.Add("isApproved", isApproved);
                    odict.Add("documentTypeId", 41);
                    result = GetHttpWebResponse(odict);
                    JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                    Dictionary<string, object> obj = (Dictionary<string, object>)javaScriptSerializer.DeserializeObject(result);
                    resultValue = obj["ReceiveNotificationResult"].ToString();

                }
                catch (CommunicationException commFaultEx)
                {
                    LogHelper.LogError(Log, "Error occured in AcceptOrRejectReviewAsync Method in RequisitionDocumentManager", commFaultEx);
                }
                catch (Exception ex)
                {
                    LogHelper.LogError(Log, "Error occured in AcceptOrRejectReviewAsync Method in RequisitionDocumentManager", ex);
                    throw;
                }
                finally
                {
                    if (resultValue != "True")
                    {
                        //SendFailureNotification(documentCode, (DocumentType)documentTypeId, (UserExecutionContext)self);
                    }
                }
            }, _userExecutionContext);

            return resultValue;
        }

        private void CreateHttpWebRequest(string strURL, UserExecutionContext userExecutionContext)
        {
            httpWebRequest = WebRequest.Create(strURL) as HttpWebRequest;
            httpWebRequest.Method = "POST";
            httpWebRequest.ContentType = @"application/json";

            NameValueCollection nameValueCollection = new NameValueCollection();
            userExecutionContext.UserName = "";
            string userContextJson = userExecutionContext.ToJSON();
            nameValueCollection.Add("UserExecutionContext", userContextJson);
            nameValueCollection.Add("Authorization", this.JWTToken);
            httpWebRequest.Headers.Add(nameValueCollection);
        }

        public List<OrgManagerDetails> GetOrgManagerDetailsBasedOnEntities(List<OrgManagerDetails> lstorgManagerDetails)
        {
            List<OrgManagerDetails> managerDetails = new List<OrgManagerDetails>();
            string serviceurl;

            serviceurl = UrlHelperExtensions.WorkFlowRestUrl + "/GetOrgManagerDetailsBasedOnEntities";
            CreateHttpWebRequest(serviceurl, "POST");
            Dictionary<string, object> odict = new Dictionary<string, object>();
            odict.Add("lstorgManagerDetails", lstorgManagerDetails);

            string Result = GetHttpWebResponse(odict);
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            managerDetails = serializer.Deserialize<List<OrgManagerDetails>>(Result);
            return managerDetails;
        }

        private List<EntityCominationSumCalculation> GetEntitySumBasedOnDocumentProperty(P2PDocument obj, int documentType, string ApprovalSource, List<int> EntityIds)
        {
            List<EntityCominationSumCalculation> Entities = new List<EntityCominationSumCalculation>();
            //Category Aggregation and Catalog/Non Catalog Aggregation to be added 
            if (documentType == 7)
            {
                Requisition req = (Requisition)obj;
                if (ApprovalSource == "OrgManager")
                {
                    req.RequisitionItems.ForEach(item =>
                    {
                        item.ItemSplitsDetail.ForEach(Splits =>
                        {
                            List<EntitiesCalculations> entitySumCalculations = new List<EntitiesCalculations>();
                            var overallExists = item.ItemExtendedType == ItemExtendedType.Fixed ? true : false;
                            EntityIds.ForEach(entityId =>
                            {
                                Splits.DocumentSplitItemEntities.ForEach(k =>
                                {
                                    if (k.EntityTypeId == Convert.ToInt32(entityId))
                                    {
                                        EntitiesCalculations entitySumCalculation = new EntitiesCalculations
                                        {
                                            EntityTypeId = Convert.ToInt32(entityId),
                                            EntityCode = k.EntityCode,
                                            EntityDetailCode = Convert.ToInt64(k.SplitAccountingFieldValue)
                                        };
                                        entitySumCalculations.Add(entitySumCalculation);
                                    }
                                });
                            });
                            if (!Entities.Any(k => JsonConvert.SerializeObject(k.EntityCombinations) == JsonConvert.SerializeObject(entitySumCalculations)))
                            {
                                Entities.Add(new EntityCominationSumCalculation
                                {
                                    EntityCombinations = entitySumCalculations,
                                    TotalAmount = Splits.SplitItemTotal,
                                    OverallLimitSplitTotal = (overallExists) ? Splits.OverallLimitSplitItem : Convert.ToDecimal(Splits.SplitItemTotal ?? 0)
                                });
                            }
                            else
                            {
                                Entities.First(k => JsonConvert.SerializeObject(k.EntityCombinations) == JsonConvert.SerializeObject(entitySumCalculations)).TotalAmount += Splits.SplitItemTotal;
                                Entities.First(k => JsonConvert.SerializeObject(k.EntityCombinations) == JsonConvert.SerializeObject(entitySumCalculations)).OverallLimitSplitTotal += (overallExists) ? Splits.OverallLimitSplitItem : Convert.ToDecimal(Splits.SplitItemTotal ?? 0);
                            }
                        });
                    });
                }
            }
            else if (documentType == 8)
            {
                Order order = (Order)obj;
                if (ApprovalSource == "OrgManager")
                {
                    order.OrderItems.Where(items => items.ItemStatus != DocumentStatus.Cancelled).ToList().ForEach(items =>
                    {
                        items.ItemSplitsDetail.ForEach(Splits =>
                        {
                            List<EntitiesCalculations> entitySumCalculations = new List<EntitiesCalculations>();
                            var overallExists = ((items.OverallItemLimit > 0) && (items.ItemExtendedType == ItemExtendedType.Fixed)) ? true : false;
                            EntityIds.ForEach(entityId =>
                            {
                                Splits.DocumentSplitItemEntities.ForEach(k =>
                                {
                                    if (k.EntityTypeId == Convert.ToInt32(entityId))
                                    {
                                        EntitiesCalculations entitySumCalculation = new EntitiesCalculations
                                        {
                                            EntityTypeId = Convert.ToInt32(entityId),
                                            EntityCode = k.EntityCode,
                                            EntityDetailCode = Convert.ToInt64(k.SplitAccountingFieldValue)
                                        };
                                        entitySumCalculations.Add(entitySumCalculation);
                                    }
                                });
                            });
                            if (!Entities.Any(k => JsonConvert.SerializeObject(k.EntityCombinations) == JsonConvert.SerializeObject(entitySumCalculations)))
                            {
                                Entities.Add(new EntityCominationSumCalculation
                                {
                                    EntityCombinations = entitySumCalculations,
                                    TotalAmount = Splits.SplitItemTotal,
                                    OverallLimitSplitTotal = (overallExists) ? Splits.OverallLimitSplitItem : Convert.ToDecimal(Splits.SplitItemTotal ?? 0)
                                });
                            }
                            else
                            {
                                Entities.First(k => JsonConvert.SerializeObject(k.EntityCombinations) == JsonConvert.SerializeObject(entitySumCalculations)).TotalAmount += Splits.SplitItemTotal;
                                Entities.First(k => JsonConvert.SerializeObject(k.EntityCombinations) == JsonConvert.SerializeObject(entitySumCalculations)).OverallLimitSplitTotal += (overallExists) ? Splits.OverallLimitSplitItem : Convert.ToDecimal(Splits.SplitItemTotal ?? 0);
                            }
                        });
                    });
                }
            }
            return Entities;
        }


        public static string EncryptURL(string querystring)
        {
            byte[] base64EncodedBytes = Encoding.UTF8.GetBytes(querystring);
            querystring = HttpServerUtility.UrlTokenEncode(base64EncodedBytes);
            return querystring;
        }

        public static string GetQueryString(long documentCode, long buyerPartnerCode, bool isRedirectToSmart2 = false)
        {
            if (isRedirectToSmart2)
                return EncryptURL("bpc=" + buyerPartnerCode + "&dc=" + documentCode);
            else
                return EncryptURL("bpc=" + buyerPartnerCode + "&dc=" + documentCode) + "&oloc=" + (int)SubAppCodes.P2P;
        }

        public P2PDocument GetBasicAndValidationDetailsById(P2PDocumentType docType, long id, long userId, int typeOfUser, bool filterByBU = true, bool isFunctionalAdmin = false, string accessType = "0")
        {
            P2PDocument objP2PDoc;
            try
            {
                var commonManager = new RequisitionCommonManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };

                long LOBId = GetCommonDao().GetLOBByDocumentCode(id);
                SettingDetails p2pSettingDetails = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.None, UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBId);
                int maxPrecessionValue = convertStringToInt(commonManager.GetSettingsValueByKey(p2pSettingDetails, "MaxPrecessionValue"));
                int maxPrecessionValueforTotal = convertStringToInt(commonManager.GetSettingsValueByKey(p2pSettingDetails, "MaxPrecessionValueforTotal"));
                int maxPrecessionValueForTaxesAndCharges = convertStringToInt(commonManager.GetSettingsValueByKey(p2pSettingDetails, "MaxPrecessionValueForTaxesAndCharges"));

                string documentStatuses = "";
                int ACEntityId = 0;
                if (docType == P2PDocumentType.Requisition)
                {
                    SettingDetails settingDetails = commonManager.GetSettingsFromSettingsComponent(docType, UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBId);
                    documentStatuses = commonManager.GetSettingsValueByKey(settingDetails, "AccessibleDocumentStatuses");
                }
                ACEntityId = GetAccessControlEntityId(docType, UserContext.ContactCode);
                objP2PDoc = GetReqDao().GetBasicDetailsById(id, userId, typeOfUser, filterByBU, isFunctionalAdmin, documentStatuses, maxPrecessionValue, maxPrecessionValueforTotal, maxPrecessionValueForTaxesAndCharges, accessType, ACEntityId);
                objP2PDoc.ValidationInfo = GetValidationDetailsById(docType, id);
                // This lstComments gives CommentGroup Count and not the count of the Comments.
                var lstComments = commonManager.GetCommentsForDocuments(id, docType, UserContext.ContactCode, objP2PDoc.DocumentStakeHolderList.ToList(), typeOfUser);
                objP2PDoc.CommentCount = lstComments.Count;

            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occurred in GetBasicAndValidationDetailsById method in RequisitionDocumentManager.", ex);
                throw ex;
            }
            return objP2PDoc;
        }

        public List<P2PDocumentValidationInfo> GetValidationDetailsById(P2PDocumentType docType, long id, bool isOnSubmit = false)
        {
            List<P2PDocumentValidationInfo> result = new List<P2PDocumentValidationInfo>();
            try
            {

                var commonManager = new RequisitionCommonManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
                long LOBId = GetCommonDao().GetLOBByDocumentCode(id);
                SettingDetails settingDetails = commonManager.GetSettingsFromSettingsComponent(docType, UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBId);

                var isPartnerMandatoryInRequisition = commonManager.GetSettingsValueByKey(settingDetails, "IsPartnerMandatoryInRequisition");
                var isPartnerMandatory = isPartnerMandatoryInRequisition.ToUpper() == "TRUE" ? true : false;
                var isOrderingLocationMandatory = false;
                if (docType == P2PDocumentType.Requisition)
                    isOrderingLocationMandatory = commonManager.GetSettingsValueByKey(settingDetails, "IsOrderingLocationMandatory").ToUpper() == "TRUE" ? true : false;
                var populateDefaultNeedByDateByDays = commonManager.GetSettingsValueByKey(P2PDocumentType.None, "NeedByDateByDays", UserContext.ContactCode);

                string restrictedSupplierRelationshipTypes = "";
                string PartnerStatuses = "";
                if (docType == P2PDocumentType.Requisition || docType == P2PDocumentType.Order)
                {
                    restrictedSupplierRelationshipTypes = commonManager.GetSettingsValueByKey(P2PDocumentType.None, "SupplierRelationshipTypesToBeRestricted", UserContext.ContactCode);
                    PartnerStatuses = commonManager.GetSettingsValueByKey(P2PDocumentType.None, "PartnerStatuses", UserContext.ContactCode);
                }
                bool EnablePastDateDocProcess = Convert.ToBoolean(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "EnablePastDateDocProcess", UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBId));

                result = GetReqDao().GetValidationDetailsById(id, isPartnerMandatory, isOrderingLocationMandatory, Convert.ToInt16(populateDefaultNeedByDateByDays), PartnerStatuses, restrictedSupplierRelationshipTypes, EnablePastDateDocProcess);

                if (isOnSubmit)
                {
                    DocumentType dcType = getDocumentType(docType);
                    var temp = commonManager.ValidateItemCustomFields((int)dcType, id, true);
                    if (temp.Count == 0 || temp[temp.Count - 1].listQuestionIds.Count == 0)
                        temp.AddRange(commonManager.ValidateSplitCustomFields((int)dcType, id, true));
                    if (temp.Count > 0 && temp[temp.Count - 1].listQuestionIds.Count > 0)
                        result.Add(new P2PDocumentValidationInfo() { TabIndexId = 2, ErrorCodes = "73" });
                }

            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occurred in GetValidationDetailsById method in RequisitionDocumentManager.", ex);
                throw ex;
            }
            return result;
        }

        public long SaveItemWithAdditionalDetails(P2PDocumentType docType, P2PItem objItem, long LOBEntityDetailCode, bool isTaxExempt = true, bool saveDefaultAccounting = true, bool saveLineItemTaxes = true, bool prorateLineItemTax = true, bool allowPeriodUpdate = true, bool isFunctionalAdmin = false, int maxPrecessionForTaxesAndCharges = 0)
        {
            long documentItemId = 0;

            if (null != objItem)
            {
                var commonManager = new RequisitionCommonManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
                var settingDetails = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.None, UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBEntityDetailCode);
                int precessionValue = convertStringToInt(commonManager.GetSettingsValueByKey(settingDetails, "MaxPrecessionValue"));
                int precessionValueforTotal = convertStringToInt(commonManager.GetSettingsValueByKey(settingDetails, "MaxPrecessionValueforTotal"));
                int precessionValueForTaxesAndCharges = convertStringToInt(commonManager.GetSettingsValueByKey(settingDetails, "MaxPrecessionValueForTaxesAndCharges"));
                bool enablePastDateDocProcess = Convert.ToBoolean(commonManager.GetSettingsValueByKey(settingDetails, "EnablePastDateDocProcess"));

                objItem.DocumentItemId = SaveItem(docType, objItem, isTaxExempt, saveDefaultAccounting, saveLineItemTaxes, prorateLineItemTax, null, true, isFunctionalAdmin);//indexing in called function

                if ((short)objItem.ItemType != 3) //(short)objItem.ItemType != 3
                {
                    List<string> lstErrors = ValidateDocumentItemWithAdditionalDetails(objItem, enablePastDateDocProcess).ToList();
                    if (lstErrors == null || lstErrors.Count == 0)
                        documentItemId = GetReqDao().SaveItemAdditionDetails(objItem, precessionValue, precessionValueforTotal, precessionValueForTaxesAndCharges);
                    else
                    {
                        string strErrors = string.Empty;
                        foreach (string s in lstErrors)
                            strErrors = strErrors + ',' + s;


                        if (Log.IsWarnEnabled)
                            Log.Warn("In SaveItemWithAdditionalDetails ValidateDocumentItemWithAdditionalDetails failed.");

                        throw new Exception(strErrors.TrimStart(','));
                    }
                    if (docType == P2PDocumentType.Requisition && allowPeriodUpdate)
                        UpdatePeriodbyNeedbyDate(docType, documentItemId);
                }
                else
                    documentItemId = objItem.DocumentItemId;
            }
            else
            {
                if (Log.IsWarnEnabled)
                    Log.Warn("In SaveItemWithAdditionalDetails objItem is null.");
            }
            return documentItemId;
        }

        private static IEnumerable<string> ValidateDocumentItemWithAdditionalDetails(P2PItem objItem, bool enablePastDateDocProcess)
        {
            var lstResults = new List<string>();
            DateTime dateVal;
            var itemType = (int)objItem.ItemType;

            if (objItem.AdditionalCharges < 0)
            {
                lstResults.Add(string.Format(CultureInfo.InvariantCulture, "Other Charges can not be negative. Please enter valid Other Charges."));
            }
            if (itemType == 1)
            {
                if (objItem.ShippingCharges < 0)
                {
                    lstResults.Add(string.Format(CultureInfo.InvariantCulture, "Shipping & Freight charges can not be negative. Please enter a valid Shipping & Freight charge."));
                }
                if (!(DateTime.TryParse(string.Format(CultureInfo.InvariantCulture, "{0:dd-MMM-yyyy}", objItem.DateRequested), out dateVal)))
                {
                    lstResults.Add(string.Format(CultureInfo.InvariantCulture, "Requested Date entered is not a valid date. Please enter a valid Requested Date."));
                }
                if (objItem.DateNeeded == null || !(DateTime.TryParse(string.Format(CultureInfo.InvariantCulture, "{0:dd-MMM-yyyy}", objItem.DateNeeded), out dateVal)) || (objItem.DateNeeded < DateTime.Now.Date) && !enablePastDateDocProcess)
                {
                    lstResults.Add(string.Format(CultureInfo.InvariantCulture, "Need by Date should be greater than current date. Please enter a valid Need by Date."));
                }
            }
            return lstResults;
        }

        public bool UpdateApprovalStatusById(P2PDocumentType docType, long documentId, Documents.Entities.DocumentStatus approvalStatus)
        {
            bool returnValue = GetReqDao().UpdateApprovalStatusById(documentId, approvalStatus);
            AddIntoSearchIndexerQueueing(documentId, (int)getDocumentType(docType));
            return returnValue;
        }

        public ICollection<KeyValuePair<decimal, string>> GetAllPartnersById(P2PDocumentType docType, long documentId, string documentIds = "", string BUIds = "")
        {
            ICollection<KeyValuePair<decimal, string>> result = null;
            try
            {
                result = GetReqDao().GetAllPartnersById(documentId, documentIds, BUIds);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occurred in GetAllPartnersById method in RequisitionDocumentManager.", ex);
                throw ex;
            }
            return result;
        }

        public ICollection<DocumentTrackStatusDetail> GetTrackDetailsofDocumentById(P2PDocumentType docType, long documentId)
        {
            if (documentId > 0)
            {
                return GetReqDao().GetTrackDetailsofDocumentById(documentId);
            }
            else
            {
                if (Log.IsWarnEnabled)
                    Log.Warn("In GetTrackDetailsofDocumentById method documentId parameter is less then or equal to 0.");
                return new List<DocumentTrackStatusDetail>();
            }
        }

        public ICollection<P2PItem> GetAllLineItemsByDocumentId(P2PDocumentType docType, ItemType itemType, long documentId, int pageIndex, int pageSize, int typeOfUser = 0)
        {
            if (documentId > 0)
            {
                var commonManager = new RequisitionCommonManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
                long LOBId = GetCommonDao().GetLOBByDocumentCode(documentId);

                // Test
                var settingDetails = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.None, this.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBId);
                var MaxPrecessionValue = convertStringToInt(commonManager.GetSettingsValueByKey(settingDetails, "MaxPrecessionValue"));
                int MaxPrecessionValueTotal = convertStringToInt(commonManager.GetSettingsValueByKey(settingDetails, "MaxPrecessionValueforTotal"));
                int MaxPrecessionValueForTaxAndCharges = convertStringToInt(commonManager.GetSettingsValueByKey(settingDetails, "MaxPrecessionValueForTaxesAndCharges"));

                var interfaceSettings = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.Interfaces, this.UserContext.ContactCode, (int)SubAppCodes.Interfaces);
                var settingValue = commonManager.GetSettingsValueByKey(interfaceSettings, "UseSequenceNumberAsInvoiceLineNumber");
                var useSequenceNumberAsInvoiceLineNumber = !string.IsNullOrEmpty(settingValue) ? Convert.ToBoolean(settingValue) : false;

                if (docType == P2PDocumentType.Invoice)
                {
                    return new SQLCommonDAO() { UserContext = UserContext, GepConfiguration = GepConfiguration }.GetAllLineItemsByInvoiceId(documentId, itemType, pageIndex, pageSize, typeOfUser, MaxPrecessionValue, MaxPrecessionValueTotal, MaxPrecessionValueForTaxAndCharges, useSequenceNumberAsInvoiceLineNumber);
                }
                else
                {
                    return GetReqDao().GetAllLineItemsByDocumentId(documentId, itemType, pageIndex, pageSize, typeOfUser, MaxPrecessionValue, MaxPrecessionValueTotal, MaxPrecessionValueForTaxAndCharges);
                }
            }
            else
            {
                if (Log.IsWarnEnabled)
                    Log.Warn("In GetAllLineItemsByDocumentId method documentId parameter is less then or equal to 0.");
                return new List<P2PItem>();
            }
        }

        public bool UpdateItemQuantity(P2PDocumentType docType, long lineItemId, decimal quantity, int itemSource, int banding, decimal maxOrderQuantity, decimal minOrderQuantity)
        {
            var commonManager = new RequisitionCommonManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
            var settingDetails = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.None, UserContext.ContactCode, (int)SubAppCodes.P2P, "", 0);
            int maxPrecessionValue = convertStringToInt(commonManager.GetSettingsValueByKey(settingDetails, "MaxPrecessionValue"));
            int maxPrecessionforTotal = convertStringToInt(commonManager.GetSettingsValueByKey(settingDetails, "MaxPrecessionValueforTotal"));
            int maxPrecessionForTaxesAndCharges = convertStringToInt(commonManager.GetSettingsValueByKey(settingDetails, "MaxPrecessionValueForTaxesAndCharges"));

            long documentCode = 0;
            if (lineItemId > 0)
            {
                var lstValidationResults = ValidateItemQuantity(quantity, itemSource, banding, maxOrderQuantity, minOrderQuantity);

                if (lstValidationResults != null && lstValidationResults.Any())
                {
                    if (Log.IsWarnEnabled)
                        Log.Warn(
                            string.Concat("In UpdateItemQuantity method Validation failed for following reasons: " +
                                          lstValidationResults.First()));
                    return false;
                }

                documentCode = GetReqDao().UpdateItemQuantity(lineItemId, quantity, maxPrecessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges);
                AddIntoSearchIndexerQueueing(documentCode, (int)getDocumentType(docType));
            }

            return documentCode > 0;
        }

        private static IEnumerable<string> ValidateItemQuantity(decimal quantity, int itemSource, int banding, decimal maxOrderQuantity, decimal minOrderQuantity)
        {
            var lstResults = new List<string>();
            if (itemSource == 2)
            {
                if (quantity <= 0)
                    lstResults.Add("Please enter a valid quantity");
                if (maxOrderQuantity > 0 && quantity > maxOrderQuantity)
                    lstResults.Add(string.Format(CultureInfo.InvariantCulture, "The quantity cannot be greater than maximum order quantity. Please enter a quantity less than {0}.",
                                                  maxOrderQuantity));
                if (minOrderQuantity > 0 && quantity < minOrderQuantity)
                    lstResults.Add(string.Format(CultureInfo.InvariantCulture, "The quantity cannot be less than minimum order quantity. Please enter a quantity greater than {0}.",
                                                  minOrderQuantity));
                if (banding > 0 && quantity % banding != 0)
                    lstResults.Add(string.Format(CultureInfo.InvariantCulture, "The banding quantity of this item is {0}. Please enter a quantity that is a multiple of the banding quantity.",
                                                  banding));
            }
            return lstResults;
        }

        public bool SaveApproverDetails(P2PDocumentType docType, long documentId, int approverId,
                                      Documents.Entities.DocumentStatus approvalStatus, string approveUrls, string rejectUrls,
                                      string instranceId)
        {
            bool returnValue = GetReqDao().SaveApproverDetails(documentId, approverId, approvalStatus, approveUrls, rejectUrls, instranceId);
            AddIntoSearchIndexerQueueing(documentId, (int)getDocumentType(docType));
            return returnValue;
        }

        public bool SaveTrackStatusDetails(P2PDocumentType docType, long requisitionId, string instanceId,
                                        long approverId, string approverName, string approverType, string approveUrls,
                                        string rejectUrls, DateTime statusDate, RequisitionTrackStatus approvalStatus, bool isDeleted)
        {
            return GetReqDao().SaveTrackStatusDetails(requisitionId, instanceId, approverId, approverName, approverType, approveUrls,
                                                           rejectUrls, statusDate, approvalStatus, isDeleted);

        }

        public P2PItem GetPartnerDetailsByLiId(P2PDocumentType docType, long lineItemId)
        {
            return GetReqDao().GetPartnerDetailsByLiId(lineItemId);
        }

        public bool validateDocumentBeforeNextAction(P2PDocumentType docType, long documentId)
        {
            return GetReqDao().validateDocumentBeforeNextAction(documentId);
        }

        public void SyncChangeRequisition(long documentCode)
        {
            var commonManager = new RequisitionCommonManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
            SettingDetails reqSettingDetails = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.Requisition, UserContext.ContactCode, 107);
            var setting = commonManager.GetSettingsValueByKey(reqSettingDetails, "IsRestrictChangeRequisition");
            bool isRestrictChangeRequisition = setting == "" ? true : Convert.ToBoolean(commonManager.GetSettingsValueByKey(reqSettingDetails, "IsRestrictChangeRequisition"));
            if (!isRestrictChangeRequisition)
            {
                var parentDocument = GetReqDao().SyncChangeRequisition(documentCode);
                if (parentDocument > 0)
                {
                    var currentPrincipal = System.Threading.Thread.CurrentPrincipal;
                    Task.Factory.StartNew((_userContext) =>
                    {
                        System.Threading.Thread.CurrentPrincipal = currentPrincipal;
                        this.UserContext = (UserExecutionContext)_userContext;

                        if (this.UserContext.Product != GEPSuite.eInterface)
                            AddIntoSearchIndexerQueueing(documentCode, (int)DocumentType.Requisition);
                        if (parentDocument > 0)
                        {
                            if (this.UserContext.Product != GEPSuite.eInterface)
                                AddIntoSearchIndexerQueueing(parentDocument, (int)DocumentType.Requisition);
                        }

                    }, this.UserContext);
                }
            }
        }

        public bool DeleteLineItemsBasedOnBUChange(P2PDocumentType docType, long DocumentId, string buList = "", long LOBId = 0)
        {
            bool result;
            try
            {
                var commonManager = new RequisitionCommonManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
                if (LOBId == 0)
                {
                    LOBId = GetCommonDao().GetLOBByDocumentCode(DocumentId);
                }
                var settingdetails = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.None, UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBId);
                int maxPrecessionValue = convertStringToInt(commonManager.GetSettingsValueByKey(settingdetails, "MaxPrecessionValue"));
                int maxPrecessionValueTotal = convertStringToInt(commonManager.GetSettingsValueByKey(settingdetails, "MaxPrecessionValueforTotal"));
                int maxPrecessionValueForTaxAndCharges = convertStringToInt(commonManager.GetSettingsValueByKey(settingdetails, "MaxPrecessionValueForTaxesAndCharges"));

                result = GetReqDao().DeleteLineItemsBasedOnBUChange(DocumentId, buList, maxPrecessionValue, maxPrecessionValueTotal, maxPrecessionValueForTaxAndCharges, LOBId);//indexing in respective manager + below

                AddIntoSearchIndexerQueueing(DocumentId, (int)getDocumentType(docType));
            }
            catch (Exception ex)
            {
                // Log Exception here
                LogHelper.LogError(Log, "Error occurred in DeleteLineItemsBasedOnBUChange method in RequisitionDocumentManager.", ex);
                throw;
            }
            return result;
        }

        public ICollection<P2PDocument> GetAllDocumentForLeftPanel(P2PDocumentType docType, decimal partnerCode, long documentId, long userId, int pageIndex, int pageSize, string currencyCode, long orgEntityDetailCode = 0, int purchaseTypeId = 1)
        {
            bool isHeaderEntityBU = GEP.Cumulus.Web.Utils.Helpers.SettingsHelper.GetAccessControlSettingsForDocument(this.UserContext, (int)DocumentType.Requisition);
            return GetReqDao().GetAllDocumentForLeftPanel(partnerCode, documentId, userId, pageIndex, pageSize, currencyCode, orgEntityDetailCode, isHeaderEntityBU, purchaseTypeId);
        }

        public DocumentStatus GetDocumentStatus(long documentCode)
        {
            Document objDocument = GetDocumentDao().GetDocumentBasicDetails(documentCode);
            return objDocument.DocumentStatusInfo;
        }

        public P2PItem GetOtherItemDetailsByLiId(P2PDocumentType docType, long lineItemId)
        {
            return GetReqDao().GetOtherItemDetailsByLiId(lineItemId);
        }

        public long SaveAccountingApplyToAll(P2PDocumentType documentType, long documentCode, List<DocumentSplitItemEntity> documentSplitItemEntities)
        {
            var commonManager = new RequisitionCommonManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
            long LOBId = GetCommonDao().GetLOBByDocumentCode(documentCode);
            var nonesettingDetails = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.None, UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBId);
            int precessionValue = convertStringToInt(commonManager.GetSettingsValueByKey(nonesettingDetails, "MaxPrecessionValue"));
            int maxPrecessionValueforTotal = convertStringToInt(commonManager.GetSettingsValueByKey(nonesettingDetails, "MaxPrecessionValueforTotal"));
            int maxPrecessionValueForTaxesAndCharges = convertStringToInt(commonManager.GetSettingsValueByKey(nonesettingDetails, "MaxPrecessionValueForTaxesAndCharges"));

            var catalogSettingDetails = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.Catalog, UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBId);
            string allowOrgEntityInCatalogItems = commonManager.GetSettingsValueByKey(catalogSettingDetails, "AllowOrgEntityInCatalogItems");
            int expenseCodeEntityId = convertStringToInt(commonManager.GetSettingsValueByKey(catalogSettingDetails, "ExpenseCodeEntityId"));

            var invoiceSettingDetails = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.Invoice, UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBId);
            bool allowTaxCodewithAmount = Convert.ToBoolean(commonManager.GetSettingsValueByKey(invoiceSettingDetails, "AllowTaxCodewithAmount"));
            string supplierStatusForValidation = commonManager.GetSettingsValueByKey(invoiceSettingDetails, "SupplierStatusForValidation");

            long returnValue;

            returnValue = GetReqDao().SaveAccountingApplyToAll(documentCode, documentSplitItemEntities, precessionValue, maxPrecessionValueforTotal, maxPrecessionValueForTaxesAndCharges, allowOrgEntityInCatalogItems, expenseCodeEntityId, allowTaxCodewithAmount, supplierStatusForValidation);//indexing in respective manager + below

            AddIntoSearchIndexerQueueing(documentCode, (int)getDocumentType(documentType));
            return returnValue;
        }

        public string GetDownloadPDFURL(long fileId)
        {
            string strFileURI = string.Empty;
            try
            {
                if (fileId > 0)
                {
                    FileManagerApi fileManagerApi = new FileManagerApi(this.UserContext, this.JWTToken);
                    strFileURI = fileManagerApi.GetFileUriByFileId(fileId);
                }
            }
            catch (Exception ex)
            {
                // Log Exception here
                LogHelper.LogError(Log, "Error occurred in GetDownloadPDFURL method in RequisitionDocumentManager.", ex);
                throw;
            }
            return strFileURI;
        }

        #region WebAPI function calls
        public bool CheckWithDrawApprovedRequisition(long documentId)
        {
            return GetReqDao().CheckWithDrawApprovedRequisition(documentId);

        }

        public DocumentIntegrationEntity GetDocumentDetailsByDocumentIdCode(long id, bool isFunctionalAdmin = false, List<long> partners = null, List<GEP.Cumulus.DocumentIntegration.Entities.IntegrationTimelines> timelines = null,bool sendTeammembersForQuickQuote = false,bool sendAdditionalColumnsForSendForBidding=false)
        {
            try
            {
                RequisitionCommonManager commonManager = new RequisitionCommonManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
                string documentStatuses = commonManager.GetSettingsValueByKey(P2PDocumentType.Requisition, "AccessibleDocumentStatuses", UserContext.ContactCode, (int)SubAppCodes.P2P);
                int ACEntityId = GetAccessControlEntityId(P2PDocumentType.Requisition, UserContext.ContactCode);
                List<long> teammemberList = new List<long>();
                if(sendTeammembersForQuickQuote)
                teammemberList = GetTeammemberList(id);
                DocumentIntegrationEntity documentIntegrationEntity = GetReqDao().GetDocumentDetailsByDocumentCode(id, documentStatuses, isFunctionalAdmin, ACEntityId, partners, timelines, teammemberList);
                DocumentIntegrationEntity objdocumentIntegrationEntity = UpdatePriceWithBaseCurrency(documentIntegrationEntity, id, false, true);
                return objdocumentIntegrationEntity;
      }
      catch (Exception ex)
            {
                LogHelper.LogError(Log, string.Format("Error occured on GetDocumentDetailsByDocumentIdCode in RequisitionDocumentManager for document Code = {0} , InnerExceptionMessage = {1}", id, ex.InnerException != null ? ex.InnerException.Message : "null"), ex);
                throw;
            }
        }

        public DocumentIntegrationEntity UpdatePriceWithBaseCurrency(DocumentIntegrationEntity documentIntegrationEntity, long documentcode = 0, bool isCallFromWorkbench = false, bool isCallFromRequisition = false)
        {
            RequisitionCommonManager commonManager = new RequisitionCommonManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
            try
            {
                long LOBEntityDetailCode = 0;
                if (isCallFromRequisition)
                    LOBEntityDetailCode = GetCommonDao().GetLOBByDocumentCode(documentcode);
                else
                    LOBEntityDetailCode = this.UserContext.BelongingEntityDetailCode;

                bool AllowMultiCurrencyInRequisition = false;                
                var AllowMultiCurrencyInRequisitionSetting = commonManager.GetSettingsValueByKey(P2PDocumentType.Requisition, "AllowMultiCurrencyInRequisition", this.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBEntityDetailCode);
                AllowMultiCurrencyInRequisition = !string.IsNullOrEmpty(AllowMultiCurrencyInRequisitionSetting) ? Convert.ToBoolean(AllowMultiCurrencyInRequisitionSetting) : false;
                if (AllowMultiCurrencyInRequisition)
                {
                    int DecimalMaxPrecessionForUnitPrice = 0;
                    int maxPrecessionValueforTotal = 0;
                    int MaxPrecessionValueForTaxesAndCharges = 0;
                    var Decimal_MaxPrecessionForUnitPrice = commonManager.GetSettingsValueByKey(P2PDocumentType.None, "Decimal_MaxPrecessionForUnitPrice", this.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBEntityDetailCode);
                    DecimalMaxPrecessionForUnitPrice = !string.IsNullOrEmpty(Decimal_MaxPrecessionForUnitPrice) ? Convert.ToInt32(Decimal_MaxPrecessionForUnitPrice) : 0;
                    var MaxPrecessionValueTotal = commonManager.GetSettingsValueByKey(P2PDocumentType.None, "MaxPrecessionValueforTotal", this.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBEntityDetailCode);
                    maxPrecessionValueforTotal = !string.IsNullOrEmpty(MaxPrecessionValueTotal) ? Convert.ToInt32(MaxPrecessionValueTotal) : 0;
                    var MaxPrecessionValue_TaxesAndCharges = commonManager.GetSettingsValueByKey(P2PDocumentType.None, "MaxPrecessionValueForTaxesAndCharges", this.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBEntityDetailCode);
                    MaxPrecessionValueForTaxesAndCharges = !string.IsNullOrEmpty(MaxPrecessionValue_TaxesAndCharges) ? Convert.ToInt32(MaxPrecessionValue_TaxesAndCharges) : 0;
                    if (documentIntegrationEntity.DocumentItems != null && documentIntegrationEntity.DocumentItems.Count > 0)
                    {
                        foreach (var item in documentIntegrationEntity.DocumentItems)
                        {
                            item.UnitPrice = Math.Round((item.UnitPrice * item.ConversionFactor), DecimalMaxPrecessionForUnitPrice);
                            item.TaxAmount = Math.Round((item.TaxAmount * item.ConversionFactor), MaxPrecessionValueForTaxesAndCharges);
                            item.TotalPrice = Math.Round((item.TotalPrice * item.ConversionFactor), maxPrecessionValueforTotal);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error Occured in UpdatePriceWithBaseCurrency method", ex);
                throw;
            }
            return documentIntegrationEntity;
        }

        public List<PriceSheetDIO> GetRfxPriceSheetObject(DocumentIntegrationEntity integrationEntity,long id, bool sendAdditionalColumnsForSendForBidding = false, bool sendAdditionalFieldsForRfx = false,string itemIds="")
    {
      List<PriceSheetDIO> lstpriceSheets = new List<PriceSheetDIO>();
   
      try
      {
         RequisitionCommonManager commonManager = new RequisitionCommonManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
         var settingDetails = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.None, UserContext.ContactCode, (int)SubAppCodes.P2P);
         int precessionValueForQuantity = convertStringToInt(commonManager.GetSettingsValueByKey(settingDetails, "Decimal_MaxPrecessionForQuantity"));
         int precessionValueForUnitPrice = convertStringToInt(commonManager.GetSettingsValueByKey(settingDetails, "Decimal_MaxPrecessionForUnitPrice"));
         List<Item.Entities.LineItem> materialItems = integrationEntity.DocumentItems.Where(a => (int)a.ItemType == (int)ItemType.Material).ToList();
        if (materialItems != null && materialItems.Count > 0)
        {
          PriceSheetDIO priceSheetDIO = FillPriceSheetObjectForFlip(materialItems, sendAdditionalColumnsForSendForBidding, id, precessionValueForQuantity, precessionValueForUnitPrice, sendAdditionalFieldsForRfx, itemIds);
          priceSheetDIO.ItemType = Item.Entities.ItemType.Material;
          priceSheetDIO.name = "";
          lstpriceSheets.Add(priceSheetDIO);
        }
        List<Item.Entities.LineItem> nonmaterialItems = integrationEntity.DocumentItems.Where(a => (int)a.ItemType != (int)ItemType.Material).ToList();
        if (nonmaterialItems != null && nonmaterialItems.Count > 0)
        {
          PriceSheetDIO priceSheetDIO = FillPriceSheetObjectForFlip(nonmaterialItems, sendAdditionalColumnsForSendForBidding, id, precessionValueForQuantity, precessionValueForUnitPrice, sendAdditionalFieldsForRfx, itemIds);
          priceSheetDIO.ItemType = nonmaterialItems[0].ItemType ;
          priceSheetDIO.name = "";
          lstpriceSheets.Add(priceSheetDIO);
        }
      }
      catch (Exception ex)
      {
        LogHelper.LogError(Log, string.Format("Error occured on GetRfxPriceSheetObject in RequisitionDocumentManager for document Code = {0} , InnerExceptionMessage = {1}", id, ex.InnerException != null ? ex.InnerException.Message : "null"), ex);
        throw;
      }
      return lstpriceSheets;
    }
    public List<RequisitionAdditionalFieldsResponse> GetRequisitionAdditionalFieldsData(long reqId,string itemIds="")
    {
      List<RequisitionAdditionalFieldsResponse> additionalFieldsData = new List<RequisitionAdditionalFieldsResponse>();
      try
      {
        RequisitionAdditionalFieldsRequest fieldsRequest = new RequisitionAdditionalFieldsRequest()
        {
          RequisitionID = reqId,
          RequisitionItemIDs = itemIds,
          LevelType=(int)LevelType.ItemLevel,
          FlipDocumentType = (int)DocumentType.RFP
        };
        var requestHeaders = new GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.RequestHeaders();
        requestHeaders.Set(UserContext, this.JWTToken);
        string appName = Environment.GetEnvironmentVariable("NewRelic.AppName") ?? "Requisition";
        string useCase = URLs.UseCaseForGetAdditionalFields;
        var webAPI = new GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.WebAPI(requestHeaders, appName, useCase);
        string GetAdditionalFieldsDataURL = MultiRegionConfig.GetConfig(CloudConfig.AppURL) + URLs.GetRequisitionAdditionalFieldsData;
        var JsonResult = webAPI.ExecutePost(GetAdditionalFieldsDataURL, fieldsRequest);
        additionalFieldsData = JsonConvert.DeserializeObject<List<RequisitionAdditionalFieldsResponse>>(JsonResult);
      }
      catch (Exception ex)
      {
        LogHelper.LogError(Log, string.Format("Error occured on GetRequisitionAdditionalFieldsData in RequisitionDocumentManager for document Code = {0} ,itemIds={1} InnerExceptionMessage = {2}", reqId, itemIds, ex.InnerException != null ? ex.InnerException.Message : "null"), ex);
        throw;
      }
      return additionalFieldsData;
    }

        public PriceSheetDIO FillPriceSheetObjectForFlip(List<Item.Entities.LineItem> lineItems, bool sendAdditionalColumnsForSendForBidding, long reqId, int precessionValueForQuantity, int precessionValueForUnitPrice,bool sendAdditionalFieldsForRfx, string itemIds = "")
        {
            PriceSheetDIO priceSheetObj = new PriceSheetDIO();
            try
            {
                LineItemDIO lineItem = new LineItemDIO();

                List<RequisitionAdditionalFieldsResponse> additionalFields = new List<RequisitionAdditionalFieldsResponse>();
                if(sendAdditionalFieldsForRfx)
                additionalFields = GetRequisitionAdditionalFieldsData(reqId, itemIds);

                List<RowData> lstrowData = FillColumnData(lineItems, additionalFields, sendAdditionalColumnsForSendForBidding, sendAdditionalFieldsForRfx);
                lineItem.RowData = lstrowData;
                priceSheetObj.LineItem = lineItem;

                List<ColSchema> lstcolSchemas = FillColumnSchema(additionalFields, sendAdditionalColumnsForSendForBidding, precessionValueForQuantity, precessionValueForUnitPrice, sendAdditionalFieldsForRfx);
                priceSheetObj.colSchema = lstcolSchemas;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, string.Format("Error occured on FillPriceSheetObjectForFlip in RequisitionDocumentManager for document Code = {0} ,itemIds={1} InnerExceptionMessage = {2}", reqId, itemIds, ex.InnerException != null ? ex.InnerException.Message : "null"), ex);
                throw;
            }

            return priceSheetObj;
        }

    public List<RowData> FillColumnData(List<Item.Entities.LineItem> lineItems, List<RequisitionAdditionalFieldsResponse> additionalFields,bool sendAdditionalColumnsForSendForBidding,bool sendAdditionalFieldsForRfx)
    {
      List<RowData> lstrowData = new List<RowData>();
      try
      {
        lineItems.ForEach(item =>
        {
          List<ColumnData> lstcolumnData = new List<ColumnData>();
          lstcolumnData.Add(new ColumnData()
          {
            ColumnName = URLs.COL_ITEM_NAME,
            ColumnValue = item.ItemDescription
          });
          lstcolumnData.Add(new ColumnData()
          {
            ColumnName = URLs.COL_ITEM_NUMBER,
            ColumnValue = string.IsNullOrEmpty(item.ItemNumber) ? "" : item.ItemNumber
          });
           lstcolumnData.Add(new ColumnData()
            {
                ColumnName = URLs.COL_UNIT,
                ColumnValue = item.UOMCode +":" + item.UOMDescription
            });
            lstcolumnData.Add(new ColumnData()
          {
            ColumnName = URLs.COL_VOLUME,
            ColumnValue = Convert.ToString(item.Quantity)
          });
          lstcolumnData.Add(new ColumnData()
          {
            ColumnName = URLs.COL_UNITPRICE,
            ColumnValue = Convert.ToString(item.UnitPrice)
          });
          lstcolumnData.Add(new ColumnData()
          {
            ColumnName = URLs.COL_LINE_NUMBER,
            ColumnValue = Convert.ToString(item.LineNumber)
          });
          lstcolumnData.Add(new ColumnData()
          {
            ColumnName = URLs.COL_INCOTERMS_CODE,
            ColumnValue = Convert.ToString(item.IncoTermCode)
          });
          lstcolumnData.Add(new ColumnData()
          {
            ColumnName = URLs.COL_INCOTERMS_LOCATION,
            ColumnValue = Convert.ToString(item.IncoTermLocation)
          });
          lstcolumnData.Add(new ColumnData()
          {
            ColumnName = URLs.COL_DOCUMENT_CODE,
            ColumnValue = Convert.ToString(item.DocumentCode)
          });
          lstcolumnData.Add(new ColumnData()
          {
            ColumnName = URLs.COL_INCOTERM_ID,
            ColumnValue = Convert.ToString(item.IncoTermId)
          });
          lstcolumnData.Add(new ColumnData()
          {
            ColumnName = URLs.COL_P2PLINEITEMID,
            ColumnValue = Convert.ToString(item.P2PLineItemId)
          });
          lstcolumnData.Add(new ColumnData()
          {
            ColumnName = URLs.COL_CURRENCY_CODE,
            ColumnValue = Convert.ToString(item.CurrencyCode)
          });
          lstcolumnData.Add(new ColumnData()
          {
            ColumnName = URLs.COL_TOTAL_PRICE,
            ColumnValue = Convert.ToString(item.TotalPrice)
          });
          lstcolumnData.Add(new ColumnData()
          {
            ColumnName = URLs.COL_TAX_AMOUNT,
            ColumnValue = Convert.ToString(item.TaxAmount)
          });

          if (sendAdditionalColumnsForSendForBidding)
          {
          lstcolumnData.Add(new ColumnData()
          {
            ColumnName = URLs.COL_ITEM_SPECIFICATION,
            ColumnValue = Convert.ToString(item.ItemSpecification)
          });
          lstcolumnData.Add(new ColumnData()
            {
              ColumnName = URLs.COL_SUPPLIERITEMNUMBER,
              ColumnValue = Convert.ToString(item.SupplierPartNumber)
            });
              lstcolumnData.Add(new ColumnData()
            {
              ColumnName = URLs.COL_Category,
              ColumnValue = Convert.ToString(item.CategoryName)
            });
            lstcolumnData.Add(new ColumnData()
            {
              ColumnName = URLs.COL_Category_ID,
              ColumnValue = Convert.ToString(item.PASCode)
            });
             lstcolumnData.Add(new ColumnData()
             {
                ColumnName = URLs.COL_CLIENT_PASCODE,
                 ColumnValue = Convert.ToString(item.ClientPASCode)
              });
              
              lstcolumnData.Add(new ColumnData()
              {
                 ColumnName = URLs.COL_ITEM_DESCRIPTION,
                 ColumnValue = Convert.ToString(item.ItemDescription)
              });

          }
          if (sendAdditionalFieldsForRfx && additionalFields!=null && additionalFields.Count>0)
          {
            List<RequisitionAdditionalFieldsResponse> itemAdditionalFields = additionalFields.Where(a => a.RequisitionItemID == item.ItemId).ToList();
            if (itemAdditionalFields != null && itemAdditionalFields.Count > 0)
            {
              var additionalFieldNameAndValues = itemAdditionalFields.GroupBy(field => new { field.AdditionalFieldID, field.AdditionalFieldDisplayName })
                                                .Select(grp => new { grp.Key.AdditionalFieldDisplayName, AdditionalFieldValue = string.Join(",", grp.ToList().Select(x => GetFormattedAdditionalFieldValue(x))) })
                                                .ToList();
              additionalFieldNameAndValues.ForEach(additionalField =>
              {
                lstcolumnData.Add(new ColumnData()
                {
                  ColumnName = additionalField.AdditionalFieldDisplayName,
                  ColumnValue = additionalField.AdditionalFieldValue
                });
              });
            }
          }
          RowData rowData = new RowData()
          {
          RowNumber = Convert.ToInt32(item.LineNumber),
            Row = lstcolumnData
          };
          lstrowData.Add(rowData);
        });
      }
      catch (Exception ex)
      {
        LogHelper.LogError(Log, string.Format("Error occured on FillColumnData in RequisitionDocumentManager for  InnerExceptionMessage = {0}", ex.InnerException != null ? ex.InnerException.Message : "null"), ex);
        throw;
      }
      return lstrowData;
    }

    public string GetFormattedAdditionalFieldValue(RequisitionAdditionalFieldsResponse additionalField)
    {
      int Displaystyle = additionalField.DataDisplayStyle;
      switch (Displaystyle)
      {
        case (int)AdditionalFieldDataDisplayStyle.CodeDescription:
          return !string.IsNullOrEmpty(additionalField.AdditionalFieldCode) ? (additionalField.AdditionalFieldCode + "-" + additionalField.AdditionalFieldValue) : additionalField.AdditionalFieldValue;
        case (int)AdditionalFieldDataDisplayStyle.DescriptionCode:
          return additionalField.AdditionalFieldValue + "-" + additionalField.AdditionalFieldCode;
        case (int)AdditionalFieldDataDisplayStyle.Code:
          return additionalField.AdditionalFieldCode;
        case (int)AdditionalFieldDataDisplayStyle.Description:
          return additionalField.AdditionalFieldValue;
        default:
          return "";
      }
    }
     public List<ColSchema> FillColumnSchema(List<RequisitionAdditionalFieldsResponse> additionalFields, bool sendAdditionalColumnsForSendForBidding, int precessionValueForQuantity, int precessionValueForUnitPrice,bool sendAdditionalFieldsForRfx)
      {
      List<ColSchema> lstcolSchemas = new List<ColSchema>();
      try
      {
        var Itemname = new ColSchema()
        {
          name = URLs.COL_ITEM_NAME,
          type = URLs.COL_TEXT_TYPE,
          mandatory = true,
          allowSupplierInput = false,
          isVisibleToSupplier = true,
          isColumnEditable = true,
          mapTo = URLs.COL_ITEM_NAME,
          isColumnVisible=true,
          settings = new Settings()
          {
            hasDecimal = false,
            decimalPoints = 0
          }
        };
        lstcolSchemas.Add(Itemname);
        var Itemnumber = new ColSchema()
        {
          name = URLs.COL_ITEM_NUMBER,
          type = URLs.COL_TEXT_TYPE,
          mandatory = false,
          allowSupplierInput = false,
          isVisibleToSupplier = true,
          isColumnEditable = false,
          mapTo = URLs.COL_ITEM_NUMBER,
          isColumnVisible=true,
          settings = new Settings()
          {
            hasDecimal = false,
            decimalPoints = 0
          }
        };
        lstcolSchemas.Add(Itemnumber);
        var Unit = new ColSchema()
        {
          name = URLs.COL_UNIT,
          type = URLs.COL_DROPDOWN_TYPE,
          mandatory = true,
          allowSupplierInput = false,
          isVisibleToSupplier = true,
          isColumnEditable = true,
          mapTo = URLs.COL_UNIT,
          isColumnVisible=true,
          settings = new Settings()
          {
            hasDecimal = false,
            decimalPoints = 0
          }
        };
        lstcolSchemas.Add(Unit);
        var Baselinepriceperunit = new ColSchema()
        {
          name = URLs.COL_UNITPRICE,
          type = URLs.COL_CURRENCY_TYPE,
          mandatory = true,
          allowSupplierInput = false,
          isVisibleToSupplier = false,
          isColumnEditable = true,
          mapTo = URLs.COL_UNITPRICE,
          isColumnVisible=true,
          settings = new Settings()
          {
            hasDecimal = true,
            decimalPoints = precessionValueForUnitPrice
          }
        };
        lstcolSchemas.Add(Baselinepriceperunit);
        var Volume = new ColSchema()
        {
          name = URLs.COL_VOLUME,
          type = URLs.COL_NUMERIC,
          mandatory = true,
          allowSupplierInput = false,
          isVisibleToSupplier = true,
          isColumnEditable = true,
          mapTo = URLs.COL_VOLUME,
          isColumnVisible=true,
          settings = new Settings()
          {
            hasDecimal = true,
            decimalPoints = precessionValueForQuantity
          }
        };
        lstcolSchemas.Add(Volume);
        var Linenumber = new ColSchema()
        {
          name = URLs.COL_LINE_NUMBER,
          type = URLs.COL_TEXT_TYPE,
          mandatory = true,
          allowSupplierInput = false,
          isVisibleToSupplier = true,
          isColumnEditable = true,
          mapTo = "",
          isColumnVisible=true,
          settings = new Settings()
          {
            hasDecimal = false,
            decimalPoints = 0
          }
        };
        lstcolSchemas.Add(Linenumber);
        var IncotermsCode = new ColSchema()
        {
          name = URLs.COL_INCOTERMS_CODE,
          type = URLs.COL_DROPDOWN_TYPE,
          mandatory = false,
          allowSupplierInput = false,
          isVisibleToSupplier = true,
          isColumnEditable = true,
          mapTo = URLs.COL_INCOTERMS_CODE,
          isColumnVisible=true,
          settings = new Settings()
          {
            hasDecimal = false,
            decimalPoints = 0,
            dropDownType = URLs.COL_Incoterms
          }
        };
        lstcolSchemas.Add(IncotermsCode);
        var IncotermsLocation = new ColSchema()
        {
          name = URLs.COL_INCOTERMS_LOCATION,
          type = URLs.COL_TEXT_TYPE,
          mandatory = false,
          allowSupplierInput = false,
          isVisibleToSupplier = true,
          isColumnEditable = true,
          mapTo = URLs.COL_INCOTERMS_LOCATION,
          isColumnVisible=true,
          settings = new Settings()
          {
            hasDecimal = false,
            decimalPoints = 0
          }
        };
        lstcolSchemas.Add(IncotermsLocation);
        var DocumentCode = new ColSchema()
        {
          name = URLs.COL_DOCUMENT_CODE,
          type = URLs.COL_NUMERIC,
          mandatory = false,
          allowSupplierInput = false,
          isVisibleToSupplier = false,
          isColumnEditable = false,
          mapTo = "",
          isColumnVisible=false,
          settings = new Settings()
          {
            hasDecimal = false,
            decimalPoints = 0
          }
        };
        lstcolSchemas.Add(DocumentCode);

        var IncotermId = new ColSchema()
        {
          name = URLs.COL_INCOTERM_ID,
          type = URLs.COL_NUMERIC,
          mandatory = false,
          allowSupplierInput = false,
          isVisibleToSupplier = false,
          isColumnEditable = false,
          mapTo = "",
          isColumnVisible=false,
          settings = new Settings()
          {
            hasDecimal = false,
            decimalPoints = 0
          }
        };
        lstcolSchemas.Add(IncotermId);

        var P2PLineItemId = new ColSchema()
        {
          name = URLs.COL_P2PLINEITEMID,
          type = URLs.COL_NUMERIC,
          mandatory = false,
          allowSupplierInput = false,
          isVisibleToSupplier = false,
          isColumnEditable = false,
          mapTo = "",
          isColumnVisible=false,
          settings = new Settings()
          {
            hasDecimal = false,
            decimalPoints = 0
          }
        };
        lstcolSchemas.Add(P2PLineItemId);
        var CurrencyCode = new ColSchema()
        {
          name = URLs.COL_CURRENCY_CODE,
          type = URLs.COL_TEXT_TYPE,
          mandatory = false,
          allowSupplierInput = false,
          isVisibleToSupplier = false,
          isColumnEditable = false,
          mapTo = "",
          isColumnVisible=false,
          settings = new Settings()
          {
            hasDecimal = false,
            decimalPoints = 0
          }
        };
        lstcolSchemas.Add(CurrencyCode);
        var TotalPrice = new ColSchema()
        {
          name = URLs.COL_TOTAL_PRICE,
          type = URLs.COL_CURRENCY_TYPE,
          mandatory = false,
          allowSupplierInput = false,
          isVisibleToSupplier = false,
          isColumnEditable = false,
          mapTo = "",
          isColumnVisible=false,
          settings = new Settings()
          {
            hasDecimal = false,
            decimalPoints = 0
          }
        };
        lstcolSchemas.Add(TotalPrice);
        var TaxAmount = new ColSchema()
        {
          name = URLs.COL_TAX_AMOUNT,
          type = URLs.COL_NUMERIC,
          mandatory = false,
          allowSupplierInput = false,
          isVisibleToSupplier = false,
          isColumnEditable = false,
          mapTo = "",
          isColumnVisible=false,
          settings = new Settings()
          {
            hasDecimal = false,
            decimalPoints = 0
          }
        };
        lstcolSchemas.Add(TaxAmount);


        if (sendAdditionalColumnsForSendForBidding)
        {
          var SupplierItemNumber = new ColSchema()
          {
            name = URLs.COL_SUPPLIERITEMNUMBER,
            type = URLs.COL_TEXT_TYPE,
            mandatory = false,
            allowSupplierInput = true,
            isVisibleToSupplier = true,
            isColumnEditable = true,
            mapTo = URLs.COL_SUPPLIERITEMNUMBER,
            isColumnVisible=true,
            settings = new Settings()
            {
              hasDecimal = false,
              decimalPoints = 0
            }
          };
          lstcolSchemas.Add(SupplierItemNumber);
          var ItemSpecification = new ColSchema()
          {
            name = URLs.COL_ITEM_SPECIFICATION,
            type = URLs.COL_EXTENDED_TEXT,
            mandatory = false,
            allowSupplierInput = false,
            isVisibleToSupplier = true,
            isColumnEditable = true,
            mapTo = URLs.COL_ITEM_SPECIFICATION,
            isColumnVisible=true,
            settings = new Settings()
            {
              hasDecimal = false,
              decimalPoints = 0
            }
          };
          lstcolSchemas.Add(ItemSpecification);
          lstcolSchemas.Add( new ColSchema()
          {
            name = URLs.COL_Category,
            type = URLs.COL_TEXT_TYPE,
            mandatory = false,
            allowSupplierInput = false,
            isVisibleToSupplier = false,
            isColumnEditable = false,
            mapTo = URLs.COL_Category,
            isColumnVisible = true
          });
          lstcolSchemas.Add(new ColSchema()
          {
            name = URLs.COL_Category_ID,
            type = URLs.COL_TEXT_TYPE,
            mandatory = false,
            allowSupplierInput = false,
            isVisibleToSupplier = true,
            isColumnEditable = false,
            mapTo = URLs.COL_Category_ID,
            isColumnVisible = false
          });
          
            lstcolSchemas.Add(new ColSchema()
            {
              name = URLs.COL_CLIENT_PASCODE,
              type = URLs.COL_TEXT_TYPE,
              mandatory = false,
              allowSupplierInput = false,
              isVisibleToSupplier = true,
              isColumnEditable = false,
              mapTo = URLs.COL_CLIENT_PASCODE,
              isColumnVisible = false
              });
              
              lstcolSchemas.Add(new ColSchema()
             {
               name = URLs.COL_ITEM_DESCRIPTION,
               type = URLs.COL_EXTENDED_TEXT,
               mandatory = false,
               allowSupplierInput = false,
               isVisibleToSupplier = true,
               isColumnEditable = false,
               mapTo = URLs.COL_ITEM_DESCRIPTION,
               isColumnVisible = true,
                settings = new Settings()
                {
                    hasDecimal = false,
                    decimalPoints = 0
                }
            });
              
        }

        if (sendAdditionalFieldsForRfx && additionalFields != null && additionalFields.Count > 0)
        {
          List<RequisitionAdditionalFieldsResponse> groupedAdditionalfields = additionalFields.GroupBy(field => new { field.RequisitionItemID}).FirstOrDefault().ToList();
          List<RequisitionAdditionalFieldsResponse> distinctAdditionalfields = groupedAdditionalfields.GroupBy(t => t.AdditionalFieldID).Select(grp => grp.First()).ToList();
          groupedAdditionalfields.ForEach(field =>
          {
            string docSpecification = field.DocumentSpecification;
            var jsonDocSpecification = JsonConvert.DeserializeObject<dynamic>(docSpecification);
            lstcolSchemas.Add(new ColSchema()
            {
              name = field.AdditionalFieldDisplayName,
              type = URLs.COL_TEXT_TYPE,
              mandatory = false,
              allowSupplierInput = (jsonDocSpecification!=null && jsonDocSpecification.allowSupplierInputOnRfx!=null)? Convert.ToBoolean(jsonDocSpecification.allowSupplierInputOnRfx):false,
              isVisibleToSupplier = (jsonDocSpecification != null && jsonDocSpecification.hideToSupplierOnRfx!=null) ? !Convert.ToBoolean(jsonDocSpecification.hideToSupplierOnRfx) : true,
              isColumnEditable = false,
              mapTo = field.AdditionalFieldDisplayName,
              isColumnVisible = true
            });
          });
        }
      }
      catch (Exception ex)
      {
        LogHelper.LogError(Log, string.Format("Error occured on FillColumnSchema in RequisitionDocumentManager for InnerExceptionMessage = {0}", ex.InnerException != null ? ex.InnerException.Message : "null"), ex);
        throw;
      }
      return lstcolSchemas;
    }


      public List<long> GetTeammemberList(long documentCode, List<long> reqLineItemIds = null)
        {

            List<long> conatcodes = new List<long>();
            try
            {
                List<RuleAction> actions = null;
                Requisition objReq = null;
                bool hasRules = false;
                hasRules = GetSQLP2PDocumentDAO().CheckHasRules(BusinessCase.RequisitionItemRFXFlipTypeCheck, (int)P2PDocumentType.Requisition);
                if (hasRules)
                {
                    UserExecutionContext userExecutionContext = UserContext;
                    objReq = GetReqDao().GetAllRequisitionDetailsByRequisitionId(documentCode, UserContext.ContactCode, 0, reqLineItemIds);
                    RequisitionCommonManager commonManager = new RequisitionCommonManager(JWTToken) { GepConfiguration = GepConfiguration, UserContext = UserContext };
                    SettingDetails invSettings = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.Invoice, UserContext.ContactCode, 107);
                    bool IsREOptimizationEnabled = Convert.ToBoolean(commonManager.GetSettingsValueByKey(invSettings, "IsREOptimizationEnabled"));
                    actions = GetCommonDao().EvaluateRulesForObject(BusinessCase.RequisitionItemRFXFlipTypeCheck, objReq, IsREOptimizationEnabled);
                    if (actions!=null && actions.Count > 0)
                    {
                        foreach (var action in actions)
                        {
                            dynamic keyResult = JsonConvert.DeserializeObject(action.KeyResult);
                            List<ParameterOutput> parameters = new List<ParameterOutput>();
                            parameters = JsonConvert.DeserializeObject<List<ParameterOutput>>(action.Parameters);
                            if (parameters.Any(parms => parms.Name.ToUpper() == "GROUPID"))
                            {
                                List<Contact> groupContacts = new List<Contact>();
                                List<long> groupIdList = parameters.Where(parms => parms.Name.ToUpper() == "GROUPID").Select(grpid => grpid.Value).Distinct().Select(long.Parse).ToList();
                                string groupIds = string.Join(",", groupIdList);
                                if (groupIdList!=null && groupIdList.Count > 0)
                                {
                                    ProxyPartnerService proxyPartnerService = new ProxyPartnerService(UserContext, this.JWTToken);
                                    groupContacts = proxyPartnerService.GetContactsForGroup(groupIds);
                                    if (groupContacts!=null && groupContacts.Count > 0)
                                    {
                                        List<long> contactList = groupContacts.Select(e => e.ContactCode).ToList<long>();
                                        if (contactList!=null && contactList.Count > 0)
                                            conatcodes.AddRange(contactList);
                                    }

                                }
                            }
                        }
                    }
                }
                return conatcodes;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in GetTeammemberList Method in RequisitionDocumentManager", ex);
                throw ex;
            }

        }

        public RuleValidations GetApprovalCheck(long documentCode, int documentTypeId)
        {
            RuleValidations objRuleValidations = new RuleValidations();
            string result = string.Empty;
            Requisition objReq = null;
            Order objOrder = new Order();

            CreditMemo objcreditMemo = new CreditMemo();

            List<RuleAction> actions = null;
            bool hasRules = false;
            ProxyPartnerService proxyPartnerService = new ProxyPartnerService(UserContext, this.JWTToken);
            List<UserDefinedApproval> userDefinedApproval = new List<UserDefinedApproval>();
            ContactAccountingInfo contactAccountingInfo = new ContactAccountingInfo();
            List<User> contactManagerInfo = new List<User>();
            Decimal? TotalAmount = 0.0000m;
            String DefaultCurrencyCode = "";
            hasRules = GetSQLP2PDocumentDAO().CheckHasRules(BusinessCase.ApprovalCheck, documentTypeId);

            int documentSubTypeid = 0;
            RequisitionCommonManager commonManager = new RequisitionCommonManager(JWTToken) { GepConfiguration = this.GepConfiguration, UserContext = this.UserContext };
            SettingDetails invSettings = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.Invoice, UserContext.ContactCode, 107);
            bool IsREOptimizationEnabled = Convert.ToBoolean(commonManager.GetSettingsValueByKey(invSettings, "IsREOptimizationEnabled"));

            if (documentTypeId == (int)DocumentType.Requisition)
            {
                if (hasRules)
                {
                    documentSubTypeid = 7;
                    objReq = (Requisition)GetReqDao().GetAllRequisitionDetailsByRequisitionId(documentCode, this.UserContext.ContactCode, 0);
                    //TotalAmount = objReq.TotalAmount;
                    DefaultCurrencyCode = objReq.DefaultCurrencyCode;
                    // The below overall limit is coming based on higher value between item level total vs overall limit
                    // Calculation in procedure : (UnitPrice x Quantity) + Tax + AdditionalCharges > OverallItemLimit
                    var TotalAndOverallLimit = objReq.RequisitionItems.Sum(x => x.OverallItemLimit);
                    // Set the higher amount for checking the rules
                    TotalAmount = TotalAndOverallLimit > objReq.TotalAmount ? TotalAndOverallLimit : objReq.TotalAmount;
                    //ProxyInvoiceService proxyInvoice = new ProxyInvoiceService(UserContext);
                    //actions = proxyInvoice.EvaluateRulesForObject(BusinessCase.ApprovalCheck, objReq, IsREOptimizationEnabled);

                    SettingDetails p2pSettings = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.None, UserContext.ContactCode, (int)SubAppCodes.P2P, "", objReq.EntityDetailCode == null ? 0 : objReq.EntityDetailCode.FirstOrDefault());
                    var costObjectEntitiesForValidation = commonManager.GetSettingsValueByKey(p2pSettings, "CostObjectEntitiesForValidation");
                    if (!string.IsNullOrEmpty(costObjectEntitiesForValidation))
                        FillCostObjectEntitiesForValidation(objReq, costObjectEntitiesForValidation);

                    actions = GetCommonDao().EvaluateRulesForObject(BusinessCase.ApprovalCheck, objReq, IsREOptimizationEnabled);
                }
            }
            if (actions != null && actions.Count > 0)
            {
                userDefinedApproval = this.GetUserDefinedApproversList(documentCode, documentSubTypeid);
                foreach (var action in actions)
                {
                    List<ParameterOutput> parameters = new List<ParameterOutput>();
                    if (action.Action == ActionType.ApprovalCheck)
                    {
                        // result = string.Concat(result, action.KeyResult);
                        parameters = JsonConvert.DeserializeObject<List<ParameterOutput>>(action.Parameters);
                        if (parameters != null && parameters.Count > 0)
                        {
                            var dpaCheckforLastApprover = (from p in parameters where p.Name.ToUpper().Trim().Equals("DPACHECKFORLASTAPPROVER") select p).FirstOrDefault();
                            var dpaCheckforCurrentApprover = (from p in parameters where p.Name.ToUpper().Trim().Equals("DPACHECKFORCURRENTAPPROVER") select p).FirstOrDefault();
                            var orgManagerCheckforLastApprover = (from p in parameters where p.Name.ToUpper().Trim().Equals("ORGMANAGERLIMITCHECKFORLASTAPPROVER") select p).FirstOrDefault();
                            var orgManagerCheckforCurrentApprover = (from p in parameters where p.Name.ToUpper().Trim().Equals("ORGMANAGERLIMITCHECKFORCURRENTAPPROVER") select p).FirstOrDefault();

                            if ((dpaCheckforLastApprover != null || dpaCheckforCurrentApprover != null) && userDefinedApproval.Count > 0)
                            {
                                contactAccountingInfo = proxyPartnerService.GetContactAccountingInfoByContactCode(UserContext.ContactCode);
                                if (TotalAmount < contactAccountingInfo.AuthorizeAmount)
                                {
                                    List<ApproverDetails> approverDetails = new List<ApproverDetails>();
                                    if (dpaCheckforLastApprover != null)
                                    {
                                        if (userDefinedApproval.Where(e => e.WorkflowOrder == userDefinedApproval.Max(x => x.WorkflowOrder) && e.IsProcessed == false && e.IsActive == true).Any())
                                            approverDetails = userDefinedApproval.Where(e => e.WorkflowOrder == userDefinedApproval.Max(x => x.WorkflowOrder) && e.IsProcessed == false && e.IsActive == true).FirstOrDefault().ApproverDetails;
                                    }
                                    else if (dpaCheckforCurrentApprover != null)
                                    {
                                        if (userDefinedApproval.Where(e => e.IsProcessed == false && e.IsActive == true).Any())
                                            approverDetails = userDefinedApproval.Where(e => e.IsProcessed == false && e.IsActive == true).OrderBy(x => x.WorkflowOrder).FirstOrDefault().ApproverDetails;
                                    }
                                    if (approverDetails.Any(e => e.ApproverId == contactAccountingInfo.ContactCode || e.ProxyApproverId == contactAccountingInfo.ContactCode))
                                    {
                                        if (documentTypeId == (int)DocumentType.PO)
                                        {
                                            string parameterValue = "";
                                            if (dpaCheckforLastApprover != null)
                                                parameterValue = (from p in parameters where p.Name.ToUpper().Trim().Equals("DPACHECKFORLASTAPPROVER") select p).FirstOrDefault().Value.Trim();
                                            else if (dpaCheckforCurrentApprover != null)
                                                parameterValue = (from p in parameters where p.Name.ToUpper().Trim().Equals("DPACHECKFORCURRENTAPPROVER") select p).FirstOrDefault().Value.Trim();
                                            if (parameterValue.Trim().ToUpper().Equals("DELTAAMOUNT"))
                                            {
                                                if (contactAccountingInfo.AuthorizeAmount < objOrder.OrderPrevOrderItemTotalAmountDiff)
                                                {
                                                    objRuleValidations.Action = actions.FirstOrDefault().Action;
                                                    result = (from p in parameters where p.Name.ToUpper().Trim().Equals("ERRORMESSAGE") select p).FirstOrDefault().Value.Trim();
                                                }
                                            }
                                            else if (contactAccountingInfo.AuthorizeAmount < objOrder.TotalAmount)
                                            {
                                                objRuleValidations.Action = actions.FirstOrDefault().Action;
                                                result = (from p in parameters where p.Name.ToUpper().Trim().Equals("ERRORMESSAGE") select p).FirstOrDefault().Value.Trim();
                                            }
                                        }
                                        else if (documentTypeId == (int)DocumentType.Requisition && contactAccountingInfo.AuthorizeAmount < objReq.TotalAmount)
                                        {
                                            objRuleValidations.Action = actions.FirstOrDefault().Action;
                                            result = (from p in parameters where p.Name.ToUpper().Trim().Equals("ERRORMESSAGE") select p).FirstOrDefault().Value.Trim();
                                        }
                                        //else if (documentTypeId == (int)DocumentType.InvoiceReconcillation && contactAccountingInfo.AuthorizeAmount < objInvoiceReconciliation.TotalAmount)
                                        //{
                                        //    objRuleValidations.Action = actions.FirstOrDefault().Action;
                                        //    result = (from p in parameters where p.Name.ToUpper().Trim().Equals("ERRORMESSAGE") select p).FirstOrDefault().Value.Trim();
                                        //}
                                        //else if (documentTypeId == (int)DocumentType.PaymentRequest && contactAccountingInfo.AuthorizeAmount < objPaymentRequest.TotalAmount)
                                        //{
                                        //    objRuleValidations.Action = actions.FirstOrDefault().Action;
                                        //    result = (from p in parameters where p.Name.ToUpper().Trim().Equals("ERRORMESSAGE") select p).FirstOrDefault().Value.Trim();
                                        //}
                                    }
                                    break;
                                }
                            }
                            else if ((orgManagerCheckforLastApprover != null || orgManagerCheckforCurrentApprover != null) && userDefinedApproval.Count > 0)
                            {
                                List<ApproverDetails> approverDetails = new List<ApproverDetails>();
                                var objCommonManager = new RequisitionCommonManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
                                var tempFliterEntityTypeId = objCommonManager.GetSettingsValueByKey(P2PDocumentType.None, "ApprovalFliterEntityTypeId", UserContext.ContactCode, (int)SubAppCodes.P2P, "");
                                var FliterEntityTypeId = String.IsNullOrEmpty(tempFliterEntityTypeId) == true ? "7" : Convert.ToString(tempFliterEntityTypeId);
                                List<SplitAccountingFields> lstEntities = objCommonManager.GetAllAdditionalFields(documentTypeId, documentCode, FliterEntityTypeId, LevelType.Both);
                                var orgEntities = String.Join(",", lstEntities.Select(d => d.EntityDetailCode).ToList());
                                string parameterValue = "";
                                if (orgManagerCheckforLastApprover != null)
                                    parameterValue = (from p in parameters where p.Name.ToUpper().Trim().Equals("ORGMANAGERLIMITCHECKFORLASTAPPROVER") select p).FirstOrDefault().Value.Trim();

                                else if (orgManagerCheckforCurrentApprover != null)
                                    parameterValue = (from p in parameters where p.Name.ToUpper().Trim().Equals("ORGMANAGERLIMITCHECKFORCURRENTAPPROVER") select p).FirstOrDefault().Value.Trim();
                                if (documentTypeId == (int)DocumentType.PO)
                                {
                                    if (parameterValue.Trim().ToUpper().Equals("DELTAAMOUNT"))
                                        TotalAmount = objOrder.OrderPrevOrderItemTotalAmountDiff;
                                }
                                contactManagerInfo = proxyPartnerService.GetSpecificUserApproversBasedOnActivityCode(String.Empty, documentSubTypeid, 0, 0, orgEntities, TotalAmount != null ? Convert.ToDecimal(TotalAmount) : 0, String.Empty, DefaultCurrencyCode);
                                if (orgManagerCheckforLastApprover != null)
                                {
                                    if (userDefinedApproval.Where(e => e.WorkflowOrder == userDefinedApproval.Max(x => x.WorkflowOrder) && e.IsProcessed == false && e.IsActive == true).Any())
                                        approverDetails = userDefinedApproval.Where(e => e.WorkflowOrder == userDefinedApproval.Max(x => x.WorkflowOrder) && e.IsProcessed == false && e.IsActive == true).FirstOrDefault().ApproverDetails;
                                }
                                else if (orgManagerCheckforCurrentApprover != null)
                                {
                                    if (userDefinedApproval.Where(e => e.IsProcessed == false && e.IsActive == true).Any())
                                        approverDetails = userDefinedApproval.Where(e => e.IsProcessed == false && e.IsActive == true).OrderBy(x => x.WorkflowOrder).FirstOrDefault().ApproverDetails;
                                }
                                if (approverDetails.Any(e => e.ApproverId == UserContext.ContactCode))
                                {
                                    var Approvers = approverDetails.Where(y => contactManagerInfo.Any(z => ((z.ContactCode == y.ApproverId || z.ContactCode == y.ProxyApproverId) && z.ContactCode == UserContext.ContactCode)));
                                    if (!Approvers.Any())
                                    {
                                        objRuleValidations.Action = actions.FirstOrDefault().Action;
                                        result = (from p in parameters where p.Name.ToUpper().Trim().Equals("ERRORMESSAGE") select p).FirstOrDefault().Value.Trim();
                                    }
                                }
                                break;
                            }
                            var parameter = (from p in parameters where p.Name.ToUpper().Trim().Equals("ERRORACTION") select p).FirstOrDefault();
                            if (parameter != null)
                            {
                                objRuleValidations.Action = actions.FirstOrDefault().Action;
                                result = string.Format("{0}\t{1}\n", result,
                                    (from p in parameters where p.Name.ToUpper().Trim().Equals("ERRORMESSAGE") select p).FirstOrDefault().Value.Trim());
                            }

                        }
                    }
                }
                objRuleValidations.ValidationMessage = result;
            }
            return objRuleValidations;
        }

        private void FillCostObjectEntitiesForValidation(Requisition req, string costObjectEntitiesForValidation)
        {
            string[] splitEntities = costObjectEntitiesForValidation.Split(',');

            req.RequisitionItems.ForEach(item =>
            {
                List<string> lstCostObjectValidationEntities = new List<string>();
                string data = "";
                item.ItemSplitsDetail.ForEach(Splits =>
                {
                    data = "";
                    foreach (var entityId in splitEntities)
                    {
                        Splits.DocumentSplitItemEntities.ForEach(k =>
                        {
                            if (k.EntityTypeId == Convert.ToInt32(entityId))
                            {
                                data = (!string.IsNullOrEmpty(data) ? data + "_" : data) + k.EntityTypeId + (!string.IsNullOrEmpty(k.EntityCode) ? "_X" : "");
                            }
                        });
                    }
                    if (data != "")
                    {
                        Splits.CostObjectEntityCombination = data;
                    }

                });
            });

        }

        public RuleValidations GetSubmissionCheck(long documentCode, int documentTypeId, BusinessCase businessCase = BusinessCase.SubmissionCheck)
        {
            RuleValidations objRuleValidations = new RuleValidations();

            try
            {
                string result = string.Empty;
                Requisition objReq = null;
                Order objOrder = null;
                CreditMemo objcreditMemo = null;
                List<RuleAction> actions = null;

                //Check whether rules exist or not
                bool hasRules = false;
                string customValidationMessage = string.Empty;
                var enableCapitalBudget = string.Empty;
                RequisitionCommonManager commonManager = new RequisitionCommonManager(JWTToken) { GepConfiguration = this.GepConfiguration, UserContext = this.UserContext };
                SettingDetails invSettings = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.Invoice, UserContext.ContactCode, 107);
                bool IsREOptimizationEnabled = Convert.ToBoolean(commonManager.GetSettingsValueByKey(invSettings, "IsREOptimizationEnabled"));
                NewP2PEntities.ConsumeCapitalBudget capitalBudgetConsumption = NewP2PEntities.ConsumeCapitalBudget.None;

                if (documentTypeId == 7)
                {
                    objReq = (Requisition)GetReqDao().GetAllRequisitionDetailsByRequisitionId(documentCode, this.UserContext.ContactCode, 0);
                    SettingDetails p2pSettings = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.None, UserContext.ContactCode, 107, "", objReq.LOBEntity);
                    enableCapitalBudget = commonManager.GetSettingsValueByKey(p2pSettings, "EnableCapitalBudget");
                    SettingDetails reqSettings = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.Requisition, UserContext.ContactCode, 107, "", objReq.LOBEntity);
                    var capitalBudgetConsumptionSetting = commonManager.GetSettingsValueByKey(reqSettings, "CapitalBudgetConsumption");
                    capitalBudgetConsumption = capitalBudgetConsumptionSetting == "" ? NewP2PEntities.ConsumeCapitalBudget.None : (NewP2PEntities.ConsumeCapitalBudget)Convert.ToByte(capitalBudgetConsumptionSetting);

                    var costObjectEntitiesForValidation = commonManager.GetSettingsValueByKey(p2pSettings, "CostObjectEntitiesForValidation");
                    if (!string.IsNullOrEmpty(costObjectEntitiesForValidation))
                        FillCostObjectEntitiesForValidation(objReq, costObjectEntitiesForValidation);
                    if (objReq.CustomAttrFormId > 0 || objReq.CustomAttrFormIdForItem > 0 || objReq.CustomAttrFormIdForSplit > 0)
                    {
                        //objReq.ListQuestionResponse = GetQuestionResponse(documentTypeId, documentCode, objReq.CustomAttrFormId, objReq.CustomAttrFormIdForItem, objReq.CustomAttrFormIdForSplit, ((System.Collections.IEnumerable)objReq.RequisitionItems).Cast<object>().ToList(), true);

                        var lstItemIds = objReq.RequisitionItems.Select(item => item.DocumentItemId).ToList();
                        List<Tuple<long, long>> lstHeader = new List<Tuple<long, long>>();
                        List<Tuple<long, long>> lstItems = new List<Tuple<long, long>>();
                        if (objReq.CustomAttrFormId > 0)
                            lstHeader.Add(new Tuple<long, long>(documentCode, objReq.CustomAttrFormId));
                        foreach (var item in objReq.RequisitionItems)
                        {
                            if (objReq.CustomAttrFormIdForItem > 0)
                                lstItems.Add(new Tuple<long, long>(item.DocumentItemId, objReq.CustomAttrFormIdForItem));

                        }
                        List<QuestionBankEntities.QuestionResponse> resultCustomAttr = objReq.CustomAttrFormIdForItem > 0 ? GetQuestionResponseDetails(lstItems) : new List<QuestionBankEntities.QuestionResponse>();
                        objReq.ListQuestionResponse = objReq.CustomAttrFormId > 0 ? GetQuestionResponseDetails(lstHeader) : new List<QuestionBankEntities.QuestionResponse>();
                        objReq.RequisitionItems.ForEach(items => items.ListQuestionResponse = resultCustomAttr.Where(w => w.ObjectInstanceId == items.DocumentItemId).ToList());

                    }
                    objReq.lstUserQuestionResponse = GetUserQuestionresponse(objReq.LOBEntity, objReq.OnBehalfOf > 0 ? objReq.OnBehalfOf : UserContext.ContactCode);

                    actions = GetCommonDao().EvaluateRulesForObject(BusinessCase.SubmissionCheck, objReq, IsREOptimizationEnabled);
                }
                objRuleValidations.ValidationMessage = string.Concat(result, customValidationMessage);
                if (actions != null && actions.Count > 0)
                {
                    if (actions[0].Action == ActionType.AutoReject)
                    {
                        List<ParameterOutput> parameters =
                            JsonConvert.DeserializeObject<List<ParameterOutput>>(actions[0].Parameters);
                        if (parameters != null && parameters.Count > 0)
                        {
                            var parameter = (from p in parameters where p.Name.ToUpper().Trim().Equals("ERRORACTION") select p).FirstOrDefault();
                            if (parameter != null)
                            {
                                result = (from p in parameters where p.Name.ToUpper().Trim().Equals("ERRORMESSAGE") select p).FirstOrDefault().Value.Trim();
                            }
                        }
                        objRuleValidations.Action = actions[0].Action;
                    }
                    foreach (var action in actions)
                    {
                        if (action.Action == ActionType.Validate)
                        {
                            result = string.Concat(result, action.KeyResult);
                            List<ParameterOutput> parameters =
                              JsonConvert.DeserializeObject<List<ParameterOutput>>(action.Parameters);
                            if (parameters != null && parameters.Count > 0)
                            {
                                var parameter = (from p in parameters where p.Name.ToUpper().Trim().Equals("ERRORACTION") select p).FirstOrDefault();
                                if (parameter != null)
                                {
                                    result = string.Format("{0}\t{1}\n", result,
                                        (from p in parameters where p.Name.ToUpper().Trim().Equals("ERRORMESSAGE") select p).FirstOrDefault().Value.Trim());
                                }
                            }
                        }
                    }
                    if (actions[0].Action != ActionType.AutoReject && result != string.Empty)
                        objRuleValidations.Action = ActionType.Validate;
                    objRuleValidations.ValidationMessage = string.Concat(result, customValidationMessage);
                }
                if (!string.IsNullOrEmpty(enableCapitalBudget) && enableCapitalBudget.ToLower() == "yes" && documentTypeId == (byte)DocumentType.Requisition && string.IsNullOrEmpty(objRuleValidations.ValidationMessage))
                {
                    CapitalBudgetManager capitalBudgetManager = new CapitalBudgetManager(this.JWTToken) { GepConfiguration = this.GepConfiguration, UserContext = this.UserContext };
                    bool IsReConsume = false;
                    if (((capitalBudgetConsumption == NewP2PEntities.ConsumeCapitalBudget.ALL
                        || capitalBudgetConsumption == NewP2PEntities.ConsumeCapitalBudget.OnSubmit
                        || capitalBudgetConsumption == NewP2PEntities.ConsumeCapitalBudget.OnSubmitLastReviewerAccept
                        || capitalBudgetConsumption == NewP2PEntities.ConsumeCapitalBudget.OnSubmitLastApproverApprove
                        ) && (objReq.DocumentStatusInfo == DocumentStatus.ReviewPending || objReq.DocumentStatusInfo == DocumentStatus.ApprovalPending)))
                        IsReConsume = true;
                    if (((capitalBudgetConsumption == NewP2PEntities.ConsumeCapitalBudget.LastReviewerAccept
                        || capitalBudgetConsumption == NewP2PEntities.ConsumeCapitalBudget.LastReviewerAcceptLastApproverApprove
                        ) && objReq.DocumentStatusInfo == DocumentStatus.ApprovalPending))
                        IsReConsume = true;

                    var validateCapitalBudget = capitalBudgetManager.CapitalBudgetValidation(documentCode, IsReConsume);
                    if (!validateCapitalBudget)
                    {
                        objRuleValidations.Action = ActionType.Validate;
                        objRuleValidations.ValidationMessage = "P2P_Req_CapitalBudgetValidation";
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, string.Format("Error occured in GetSubmissionCheck  Method in RequisitionDocumentManager Buyer partner code = {0} , Contact Code = {1},  ExceptionMessage = {2}, ParameterValues = {3} ,  Stack Trace = {4}.",
                                this.UserContext.BuyerPartnerCode, this.UserContext.ContactCode, ex.Message != null ? ex.Message : "null", documentCode, ex.StackTrace != null ? ex.StackTrace.ToString() : "null"), ex);
            }


            return objRuleValidations;
        }

        public void WithdrawChangeRequisition(long documentCode, DocumentStatus documentStatus)
        {
            var commonManager = new RequisitionCommonManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
            SettingDetails reqSettingDetails = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.Requisition, UserContext.ContactCode, 107);
            SettingDetails commonSettingDetails = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.None, UserContext.ContactCode, 107);
            var setting = commonManager.GetSettingsValueByKey(reqSettingDetails, "IsRestrictChangeRequisition");
            var enablestockresSetting = commonManager.GetSettingsValueByKey(commonSettingDetails, "EnableStockReservationViaExternalInventoryIntegration");
            bool isRestrictChangeRequisition = setting == "" ? true : Convert.ToBoolean(commonManager.GetSettingsValueByKey(reqSettingDetails, "IsRestrictChangeRequisition"));
            bool isStockReservationEnabled = enablestockresSetting == "" ? false : Convert.ToBoolean(commonManager.GetSettingsValueByKey(commonSettingDetails, "EnableStockReservationViaExternalInventoryIntegration"));
            if (!isRestrictChangeRequisition)
            {
                var parentDocument = GetReqDao().WithdrawChangeRequisition(documentCode);
                if (parentDocument > 0)
                {
                    var currentPrincipal = System.Threading.Thread.CurrentPrincipal;
                    Task.Factory.StartNew((_userContext) =>
                    {
                        System.Threading.Thread.CurrentPrincipal = currentPrincipal;
                        this.UserContext = (UserExecutionContext)_userContext;

                        if (this.UserContext.Product != GEPSuite.eInterface)
                            AddIntoSearchIndexerQueueing(documentCode, (int)DocumentType.Requisition);
                        if (parentDocument > 0)
                        {
                            if (this.UserContext.Product != GEPSuite.eInterface)
                                AddIntoSearchIndexerQueueing(parentDocument, (int)DocumentType.Requisition);
                        }

                    }, this.UserContext);
                }
            }
            if (isStockReservationEnabled == true)
            {
                NewRequisitionDAO objReqManager = new NewRequisitionDAO() { GepConfiguration = this.GepConfiguration, UserContext = this.UserContext };
                objReqManager.UpdateLineStatusForRequisition(documentCode, (StockReservationStatus)(DocumentStatus.Withdrawn), true, null);
            }


            RequisitionEmailNotificationManager emailNotificationManager = new RequisitionEmailNotificationManager(UserContext, GepConfiguration, this.JWTToken);
            emailNotificationManager.SendNotificationForWithdrawRequisition(documentCode, documentStatus);
        }
        #endregion

        public bool UpdateRequisitionDocumentStatus(long documentId, Documents.Entities.DocumentStatus documentStatus)
        {
            bool result = false;
            try
            {
                result = GetReqDao().UpdateDocumentStatus(documentId, documentStatus, 0);
                UpdateDocumentStatusInBudget(documentId, documentStatus);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occurred in UpdateRequisitionDocumentStatus method in RequisitionDocumentManager.", ex);
                throw;
            }
            return result;
        }

        private string GetEntityNumberForRequisition(string documentType, long LOBEntityDetailCode = 0, long EntityDetailcode = 0, int PurchaseTypeID = 0)
        {
            var requestHeaders = new RESTAPIHelper.RequestHeaders();
            string requisitionNumber;
            string appName = Environment.GetEnvironmentVariable("NewRelic.AppName") ?? "Requisition"; 
            string useCase = "RequisitionDocumentManager-GetEntityNumberForRequisition";
            string serviceURL = MultiRegionConfig.GetConfig(CloudConfig.APIMBaseURL) + RESTAPIHelper.ServiceURLs.MotleyServiceURL + "GetEntityNumber";
            Dictionary<string, object> body = new Dictionary<string, object>();
            body.Add("entityType", documentType);
            body.Add("lobId", LOBEntityDetailCode);
            body.Add("entityDetailCode", EntityDetailcode);
            body.Add("purchaseTypeID", PurchaseTypeID);

            requestHeaders.Set(UserContext, JWTToken);
            var webAPI = new GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.WebAPI(requestHeaders, appName, useCase);
            requisitionNumber = webAPI.ExecutePost(serviceURL, body);
            return requisitionNumber.Replace("\"", string.Empty);
        }

        private void UpdateDocumentStatusInBudget(long documentId, DocumentStatus documentStatus)
        {
            var currentPrincipal = System.Threading.Thread.CurrentPrincipal;
            Task.Factory.StartNew((_userContext) =>
            {
                System.Threading.Thread.CurrentPrincipal = currentPrincipal;
                try
                {
                    UserExecutionContext ctx = (UserExecutionContext)_userContext;
                    CapitalBudgetManager capitalBudgetManager = new CapitalBudgetManager(JWTToken) { GepConfiguration = GepConfiguration, UserContext = ctx };
                    RequisitionCommonManager commonManager = new RequisitionCommonManager(JWTToken) { GepConfiguration = GepConfiguration, UserContext = ctx };
                    long LOBId = GetCommonDao().GetLOBByDocumentCode(documentId);
                    string enableCapitalBudgetSetting = commonManager.GetSettingsValueByKey(P2PDocumentType.None, "EnableCapitalBudget", ctx.ContactCode, 107, "", LOBId);
                    bool enableCapitalBudget = false;
                    if (!string.IsNullOrEmpty(enableCapitalBudgetSetting) && enableCapitalBudgetSetting.ToLower() == "yes")
                        enableCapitalBudget = true;
                    if (enableCapitalBudget)
                    {
                        capitalBudgetManager.UpdateDocumentStatusInBudget(documentId, documentStatus.ToString());

                    }
                }
                catch (Exception ex)
                {
                    LogHelper.LogError(Log, "Error occurred in UpdateDocumentStatusInBudget method in RequisitionDocumentManager.", ex);
                }
            }, this.UserContext);
        }

        public Dictionary<string, string> CheckAuthorityAmountforRequester(long contactCode, decimal documentAmount, string fromCurrency, string toCurrency)
        {
            Dictionary<string, string> result = new Dictionary<string, string>(); ;
            try
            {
                ProxyPartnerService proxyPartnerService = new ProxyPartnerService(UserContext, this.JWTToken);
                decimal ConversionFactor;
                var commonManager = new RequisitionCommonManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
                ConversionFactor = commonManager.GetCurrencyConversionFactor(fromCurrency, toCurrency);
                documentAmount = documentAmount * ConversionFactor;
                ContactAccountingInfo contactAccountingInfo = new ContactAccountingInfo();
                contactAccountingInfo = proxyPartnerService.GetContactAccountingInfoByContactCode(contactCode);
                if (documentAmount > contactAccountingInfo.AuthorizeAmount)
                {
                    result.Add("CheckAuthorityAmountResult", "False");
                }
                else
                {
                    result.Add("CheckAuthorityAmountResult", "True");
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occurred in CheckAuthorityAmountforRequester method in RequisitionDocumentManager.", ex);
                result.Add("CheckAuthorityAmountResult", "CheckAuthorityAmountResultFailed");
            }
            return result;
        }

        public bool SaveTaskActionDetails(TaskInformation objTaskInformation)
        {
            try
            {
                if (objTaskInformation != null && objTaskInformation.DocumentCode > 0)
                    return GetSQLP2PDocumentDAO().SaveTaskDetails(objTaskInformation);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in RequisitionDocumentManager for SaveTaskActionDetails Method of TaskManager for documentCode = " + objTaskInformation.DocumentCode, ex);
                return false;
            }
            return true;
        }
    }
}
