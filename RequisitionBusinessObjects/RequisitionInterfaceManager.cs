using GEP.Cumulus.P2P.Req.BusinessObjects.Entities;
using Gep.Cumulus.CSM.Entities;
using Gep.Cumulus.CSM.Extensions;
using Gep.Cumulus.Partner.Entities;
using GEP.Cumulus.Documents.Entities;
using GEP.Cumulus.Logging;
using GEP.Cumulus.P2P.BusinessEntities;
using GEP.Cumulus.P2P.BusinessObjects.Proxy;
using GEP.Cumulus.P2P.Common;
using GEP.Cumulus.P2P.DataAccessObjects.SQLServer;
using GEP.Cumulus.P2P.Req.BusinessObjects;
using GEP.Cumulus.P2P.Req.BusinessObjects.Proxy;
using GEP.Cumulus.P2P.Req.DataAccessObjects;
using GEP.Cumulus.SmartCatalog.BusinessEntities.NewCatalog;
using GEP.Cumulus.Web.Utils;
using GEP.Cumulus.Web.Utils.Helpers;
using GEP.Smart.Platform.SearchCoreIntegretor.Entities;
using GEP.SMART.Configuration;
using log4net;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Caching;
using System.ServiceModel;
using System.Text;
using static GEP.Cumulus.P2P.BusinessObjects.Proxy.ProxyCatalogService;
using QuestionBankEntities = GEP.Cumulus.QuestionBank.Entities;
using FileManagerEntities = GEP.NewP2PEntities.FileManagerEntities;

namespace GEP.Cumulus.P2P.BusinessObjects
{
    [ExcludeFromCodeCoverage]
    public class RequisitionInterfaceManager : RequisitionBaseBO
    {
        #region Private Variables & Methods
        private static readonly ILog Log = Logger.GetLog(MethodBase.GetCurrentMethod().DeclaringType);
        private HttpWebRequest req = null;
        private string requestId;

        // Passing default value as null to avoid any build issues, need to change later
        public RequisitionInterfaceManager(string jwtToken): base(jwtToken)
        {

        }

        internal ItemSourceType getRequisitionSourceTypes(byte ItemSource)
        {
            if (ItemSource == (byte)GEP.Cumulus.SmartCatalog.BusinessEntities.CatalogSource.Hosted)
                return ItemSourceType.Hosted;
            else if (ItemSource == (byte)GEP.Cumulus.SmartCatalog.BusinessEntities.CatalogSource.Punchout)
                return ItemSourceType.Punchout;
            else if (ItemSource == (byte)GEP.Cumulus.SmartCatalog.BusinessEntities.CatalogSource.ItemMaster)
                return ItemSourceType.Internal;
            else if (ItemSource == (byte)GEP.Cumulus.SmartCatalog.BusinessEntities.CatalogSource.Manual)
                return ItemSourceType.Manual;
            else if (ItemSource == (byte)GEP.Cumulus.SmartCatalog.BusinessEntities.CatalogSource.HostedAndItemMaster)
                return ItemSourceType.HostedAndInternal;

            return ItemSourceType.Other;
        }

        public SearchResult GetLineItemsFromCatalogAPI(Requisition objRequisition, RequisitionItem item, ItemSearchInput searchInput, bool IsCaptureCatalogApiResponse = false, bool CallWebAPIOnRequisition = false)
        {
            string filePath = string.Empty, fileName = string.Empty, fileUri = string.Empty, fileContent = string.Empty;
            OperationContextScope objOperationContextScope = null;
            ICatalogServiceChannel catalogServiceChannel = null;

            try
            {
                string requestId = Guid.NewGuid().ToString().Replace("-", "");

                List<long> accessEntities = new List<long>();
                if (objRequisition.DocumentAdditionalEntitiesInfoList != null && objRequisition.DocumentAdditionalEntitiesInfoList.Count > 0)
                {
                    foreach (var addEntity in objRequisition.DocumentAdditionalEntitiesInfoList)
                    {
                        accessEntities.Add(addEntity.EntityDetailCode);
                    }
                }

                searchInput.AccessEntities = accessEntities;
                searchInput.UOM = item.UOM;
                searchInput.Quantity = item.Quantity;
                searchInput.Size = 10;
                searchInput.isIMEnable = null;
                searchInput.SortList = new List<SortFieldEnum>();
                searchInput.SortList.Add(SortFieldEnum.ContractNumber);
                if (searchInput.NeedByDate == null || searchInput.NeedByDate == DateTime.MinValue)
                {
                    if (item.ItemType == ItemType.Material)
                        searchInput.CurrentDate = item.DateNeeded;
                    else if (item.ItemType == ItemType.Service)
                        searchInput.CurrentDate = item.StartDate;
                }
                else
                {
                    searchInput.CurrentDate = null;
                }
                if (IsCaptureCatalogApiResponse)
                {
                    filePath = "Interfaces/ServiceLogs/CatalogAPI/" + DateTime.Now.ToString("yyyyMMdd");

                    fileName = requestId + "_" + objRequisition.DocumentNumber + "_" + item.ItemLineNumber + "_Request" + ".txt";
                    fileUri = UserContext.BuyerPartnerCode + "/" + filePath + "/" + fileName;

                    Dictionary<string, object> dictContent = new Dictionary<string, object>();
                    dictContent.Add("DocumentNumber", objRequisition.DocumentNumber);
                    dictContent.Add("ItemNumber", item.ItemLineNumber);
                    dictContent.Add("RequestObject", searchInput);
                    fileContent = dictContent.ToJSON();

                    UploadFileToBlobStorage(UserContext, "buyersqlconn", fileUri, fileContent);

                    LogHelper.LogInfo(Log, requestId + "_" + objRequisition.DocumentNumber + "_" + item.ItemLineNumber + "_Request: " + fileContent);
                }

                SearchResult searchResult = new SearchResult();

                if (CallWebAPIOnRequisition)
                {
                    ItemFilterInputRequest itemFilterInputRequest = new ItemFilterInputRequest()
                    {
                        BIN = searchInput.BIN,
                        ContactCode = searchInput.ContactCode,
                        AccessEntities = searchInput.AccessEntities,
                        Size = 10,
                        UOM = searchInput.UOM,
                        Quantity = searchInput.Quantity,
                        isIMEnable = searchInput.isIMEnable,
                        SortList = searchInput.SortList,
                        CurrentDate = searchInput.CurrentDate,
                        SIN = searchInput.SIN,
                        ContractNumber = searchInput.ContractNumber,
                        SupplierCode = searchInput.SupplierCode,
                        IsMRP = searchInput.IsMRP,
                        isCheckContractLimit = searchInput.isCheckContractLimit,
                        isCheckLeadTimeExpiry = searchInput.isCheckLeadTimeExpiry,
                        DocumentDate = searchInput.DocumentDate,
                        NeedByDate=searchInput.NeedByDate
                    };

                    var requestHeaders = new GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.RequestHeaders();
                    requestHeaders.Set(this.UserContext, this.JWTToken);
                    string appName = System.Environment.GetEnvironmentVariable("NewRelic.AppName") ?? "Requisition";
                    string useCase = "Interface-Req-SetSourceType";
                    var serviceURL = MultiRegionConfig.GetConfig(CloudConfig.AppURL) + "api/GetLineItemsFilter?oloc=559";
                    var webAPI = new GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.WebAPI(requestHeaders, appName, useCase);
                    var apiResult = webAPI.ExecutePost(serviceURL, itemFilterInputRequest);
                    var apiresponse = JsonConvert.DeserializeObject<SearchResult>(apiResult);
                    if (apiresponse != null)
                    {
                        searchResult = apiresponse;
                    }
                }
                else
                {
                    ProxyCatalogService proxyCatalogService = new ProxyCatalogService(UserContext, this.JWTToken);
                    var _catalogServiceEndpoint = MultiRegionConfig.GetConfig(CloudConfig.SmartNewCatalogServiceURL);
                    MessageHeader<string> objMhgAuth = new MessageHeader<string>(this.JWTToken);
                    System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");

                    catalogServiceChannel = proxyCatalogService.ConfigureChannel<ICatalogServiceChannel>(_catalogServiceEndpoint, UserContext, ref objOperationContextScope);

                    System.ServiceModel.OperationContext.Current.OutgoingMessageHeaders.Add(authorization);


                    if (catalogServiceChannel != null)
                    {
                        searchResult = catalogServiceChannel.GetLineItems(searchInput);
                        GEPServiceManager.DisposeService(catalogServiceChannel, objOperationContextScope);
                    }

                }

                if (IsCaptureCatalogApiResponse)
                {
                    filePath = "Interfaces/ServiceLogs/CatalogAPI/" + DateTime.Now.ToString("yyyyMMdd");

                    fileName = requestId + "_" + objRequisition.DocumentNumber + "_" + item.ItemLineNumber + "_Response" + ".txt";
                    fileUri = UserContext.BuyerPartnerCode + "/" + filePath + "/" + fileName;

                    Dictionary<string, object> dictContent = new Dictionary<string, object>();
                    dictContent.Add("DocumentNumber", objRequisition.DocumentNumber);
                    dictContent.Add("ItemNumber", item.ItemLineNumber);
                    dictContent.Add("ResponseObject", searchResult);
                    fileContent = dictContent.ToJSON();

                    UploadFileToBlobStorage(UserContext, "buyersqlconn", fileUri, fileContent);
                    LogHelper.LogInfo(Log, requestId + "_" + objRequisition.DocumentNumber + "_" + item.ItemLineNumber + "_Response: " + fileContent);
                }

                return searchResult;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occurred in SaveRequisitionFromInterface method while calling catalog service", ex);
            }
            finally
            {
                GEPServiceManager.DisposeService(catalogServiceChannel, objOperationContextScope);
            }
            return null;
        }

        public DataSearchResultWrapper GetAllChildContractsByContractNumberAPI(string contractNumber, string Documentnumber, long itemlineNumber, long OrgBU)
        {
            string filePath = string.Empty;
            string fileName = string.Empty;
            string fileUri = string.Empty;
            string fileContent = string.Empty;

            WorkspaceRestInput workspaceRestInput = new WorkspaceRestInput();
            workspaceRestInput.SearchKeyword = "";
            workspaceRestInput.Filters = new string[] { "moduleScope:contract", "pageNumber:1", "noOfRecords:10", "isSeeAllResult:true" };

            var lstAdvanceFilterInput = new List<GEP.Smart.Platform.SearchCoreIntegretor.Entities.AdvanceFilterInput>();
            lstAdvanceFilterInput.Add(new AdvanceFilterInput()
            {
                SearchKey = "DM_Status",
                Value = Convert.ToString(Convert.ToInt32(DocumentStatus.Live)) + "," + Convert.ToString(Convert.ToInt32(DocumentStatus.Execute)),
                IsCustAttr = false,
                FieldType = "Autosuggest"
            });
            lstAdvanceFilterInput.Add(new AdvanceFilterInput()
            {
                SearchKey = "ContractNumber",
                Value = contractNumber,
                IsCustAttr = false,
                FieldType = "Plain"
            });
            lstAdvanceFilterInput.Add(new AdvanceFilterInput()
            {
                SearchKey = "DM_ORGNew",
                Value = OrgBU.ToString(),
                IsCustAttr = false,
                FieldType = "Autosuggest"
            });

            workspaceRestInput.AdvanceSearchInput = lstAdvanceFilterInput;

            var requestHeaders = new GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.RequestHeaders();
            requestHeaders.Set(this.UserContext, this.JWTToken);
            string appName = System.Environment.GetEnvironmentVariable("NewRelic.AppName") ?? "Requisition";
            string useCase = "Interface-Req-SetSourceType";
            var serviceURL = MultiRegionConfig.GetConfig(CloudConfig.AppURL) + "api/GetAllChildContractsByContractNumber?oloc=276";
            var webAPI = new GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.WebAPI(requestHeaders, appName, useCase);

            #region Save Contract Request

            filePath = "Interfaces/ServiceLogs/GetAllChildContractsByContractNumberAPI/" + DateTime.Now.ToString("yyyyMMdd");
            fileName = Documentnumber + "_" + itemlineNumber + "_Request" + ".txt";

            fileUri = UserContext.BuyerPartnerCode + "/" + filePath + "/" + fileName;

            Dictionary<string, object> dictContent = new Dictionary<string, object>();
            dictContent.Add("url", serviceURL);
            dictContent.Add("Header", requestHeaders);
            dictContent.Add("DocumentNumber", Documentnumber);
            dictContent.Add("ItemNumber", itemlineNumber);
            dictContent.Add("RequestObject", workspaceRestInput);
            fileContent = dictContent.ToJSON();

            UploadFileToBlobStorage(UserContext, "buyersqlconn", fileUri, fileContent);
            LogHelper.LogInfo(Log, Documentnumber + "_" + itemlineNumber + "_Request: " + fileContent);

            #endregion

            var apiResult = webAPI.ExecutePost(serviceURL, workspaceRestInput);
            var apiresponse = new DataSearchResultWrapper();
            if (apiResult != null)
            {
                apiresponse = JsonConvert.DeserializeObject<DataSearchResultWrapper>(apiResult);
            }

            #region Save Contract Response

            fileName = Documentnumber + "_" + itemlineNumber + "_Response" + ".txt";
            fileUri = UserContext.BuyerPartnerCode + "/" + filePath + "/" + fileName;

            dictContent = new Dictionary<string, object>();
            dictContent.Add("DocumentNumber", Documentnumber);
            dictContent.Add("ItemNumber", itemlineNumber);
            dictContent.Add("ResponseObject", apiresponse);
            fileContent = dictContent.ToJSON();

            UploadFileToBlobStorage(UserContext, "buyersqlconn", fileUri, fileContent);
            LogHelper.LogInfo(Log, Documentnumber + "_" + itemlineNumber + "_Response: " + fileContent);

            #endregion 

            return apiresponse;
        }

        #endregion

        #region Inbound

        public long UpdateRequisitionFromInterface(P2PDocument obj, Requisition objExistingRequisition, List<ContactORGMapping> lstBUDetails, ref RequisitionCommonManager objCommon, int partnerInterfaceId, int accessControlId, bool useDocumentLOB = false, bool isDocumentNameAsNumber = false)
        {
            long changeRequisitionId = 0;
            try
            {
                string documentName = !isDocumentNameAsNumber ? !string.IsNullOrEmpty(obj.DocumentName) ? obj.DocumentName : objExistingRequisition.DocumentName : objExistingRequisition.DocumentName;

                changeRequisitionId = SaveChangeRequisitionRequestFromInterface(RequisitionSource.ChangeRequisition, objExistingRequisition.DocumentCode, documentName,
                        objExistingRequisition.DocumentNumber, DocumentSourceType.Interface, ((Requisition)obj).RevisionNumber, true);

                obj.DocumentCode = obj.DocumentId = changeRequisitionId;

                changeRequisitionId = UpdateRequistionDetailsFromInterface(changeRequisitionId, obj, objExistingRequisition, objCommon, lstBUDetails, partnerInterfaceId, accessControlId, useDocumentLOB);

                //Push to Event Hub that the Requisition has been updated through Interface
                var oldRequisitionId = objExistingRequisition.DocumentCode;
                var p2pDocumentManger = new RequisitionDocumentManager(this.JWTToken)
                {
                    UserContext = this.UserContext,
                    GepConfiguration = this.GepConfiguration
                };
                object eventData = new { message = "SaveRequisitionFromInterface", data = new { documentCode = changeRequisitionId } };

                p2pDocumentManger.BroadcastPusher(oldRequisitionId, DocumentType.Requisition, "DataStale", eventData);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occurred in UpdateRequisitionFromInterface method in RequisitionInterfaceManager documentnumber :- " + obj?.DocumentNumber, ex);
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw();
                throw;
            }
            return changeRequisitionId;
        }
        public long UpdateRequistionDetailsFromInterface(long newChangeRequistionId, P2PDocument obj, Requisition objExistingRequisition, RequisitionCommonManager objCommon, List<ContactORGMapping> lstBUDetails, int partnerInterfaceId, int accessControlId, bool useDocumentLOB = false)
        {
            RequisitionManager reqManager = new RequisitionManager (this.JWTToken){ UserContext = this.UserContext, GepConfiguration = this.GepConfiguration };
            #region Variables

            bool includeTaxInSplit = false, useTaxMaster = false, allowOverridingOrgEntityFromCatalog = true, setDefaultSplit = false,
                isHeaderShipToValid = false, isLiShipToValid = false, IsHeaderEntityAccessControlEntity = false, isOperationalBudgetEnabled = false, IsDeriveItemDetailEnable = false;
            bool IsOrderingLocationMandatory = false, IsDefaultOrderingLocation = false, isIncludePriceDetails = false, IsCaptureCatalogApiResponse = false;
            int headerEntityId = 0, noOfDaysForDateRequested = 0, splitCounters = 1, DocumentStatus = 0, IsDeltaAmount = 0, IsClientCodeBasedonLinkLocation = 0, CallGetLineItemsAPIOnRequisition = 0; ;
            string headerEntityName = string.Empty, settingValue = string.Empty, headerDeliverto = string.Empty;
            long lobEntityDetailCode = 0, headerOrgEntityDetailCode = 0;
            decimal? ReqTotal_ = 0;
            StringBuilder strErrors = new StringBuilder();

            List<OrganizationStructureService.GeneralLedger> lstGL = new List<OrganizationStructureService.GeneralLedger>();
            List<OrganizationStructureService.OrgEntity> lstOrgEntity = new List<OrganizationStructureService.OrgEntity>();
            List<string> lstGLCodes = new List<string>();
            List<string> lstOrgCodes = new List<string>();
            List<KeyValuePair<string, long>> lstKvpCategoryDetails = new List<KeyValuePair<string, long>>();
            List<KeyValuePair<long, long>> lstKvpCategoryUnspsc = new List<KeyValuePair<long, long>>();
            List<SplitAccountingFields> splitFieldDetails = new List<SplitAccountingFields>();
            List<KeyValuePair<string, long>> lstDeriveditemDetails = new List<KeyValuePair<string, long>>();

            List<DocumentSplitItemEntity> lstEntityDeails = new List<DocumentSplitItemEntity>();
            List<DocumentSplitItemEntity> defaultAccDetails = null;
            DataSet result = null;

            DataTable dtResult = new DataTable { Locale = CultureInfo.InvariantCulture };
            var dtItems = new DataTable { Locale = CultureInfo.InvariantCulture };
            dtItems.Columns.Add("BuyerItemNumber", typeof(string));
            dtItems.Columns.Add("PartnerCode", typeof(decimal));
            dtItems.Columns.Add("UOM", typeof(string));

            var objdt = new DataTable { Locale = CultureInfo.InvariantCulture };
            objdt.Columns.Add("ItemNumber", typeof(string));
            objdt.Columns.Add("CatalogItemID", typeof(long));
            objdt.Columns.Add("ItemMasterItemId", typeof(long));
            objdt.Columns.Add("tardocumentType", typeof(int));

            RequisitionDocumentManager  objDocBO = new RequisitionDocumentManager(this.JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
            BZRequisition objBZRequisition = new BZRequisition();
            Requisition objRequisition = (Requisition)obj;
            objBZRequisition.Requisition = objRequisition;


            Int32 EntityMappedToBillToLocation = 0, EntityMappedToShipToLocation = 0;
            int maxPrecessionforTotal, maxPrecessionForTaxesAndCharges;

            List<KeyValuePair<Level, long>> lstCustomAttrFormId = new List<KeyValuePair<Level, long>>();
            List<QuestionBankEntities.QuestionResponse> lstQuestionsResponse = new List<QuestionBankEntities.QuestionResponse>();
            List<QuestionBankEntities.Question> headerQuestionSet = new List<QuestionBankEntities.Question>();
            List<QuestionBankEntities.Question> itemQuestionSet = new List<QuestionBankEntities.Question>();
            List<QuestionBankEntities.Question> splitQuestionSet = new List<QuestionBankEntities.Question>();
            #endregion
            try
            {

                #region Set the Setting
                Requisition objREQ;
                objREQ = (Requisition)objDocBO.GetBasicDetailsById(P2PDocumentType.Requisition, newChangeRequistionId, UserContext.ContactCode);
                objRequisition.DocumentNumber = objREQ.DocumentNumber;
                objRequisition.DocumentName = objREQ.DocumentName;
                objRequisition.TotalAmount = objREQ.TotalAmount;

                lobEntityDetailCode = objREQ.EntityDetailCode.FirstOrDefault();

                var interfaceSettings = objCommon.GetSettingsFromSettingsComponent(P2PDocumentType.Interfaces, UserContext.ContactCode, (int)SubAppCodes.Interfaces);
                var p2pSettings = objCommon.GetSettingsFromSettingsComponent(P2PDocumentType.None, UserContext.ContactCode, (int)SubAppCodes.P2P);
                var p2pSettingsSpendControlType = objCommon.GetSettingsFromSettingsComponent(P2PDocumentType.None, UserContext.ContactCode, (int)SubAppCodes.P2P, "", lobEntityDetailCode);
                var RequisitionSettings = objCommon.GetSettingsFromSettingsComponent(P2PDocumentType.Requisition, UserContext.ContactCode, (int)SubAppCodes.P2P);
                var _catalogSettings = objCommon.GetSettingsFromSettingsComponent(P2PDocumentType.Catalog, UserContext.ContactCode, (int)SubAppCodes.P2P, "", lobEntityDetailCode);

                includeTaxInSplit = Convert.ToBoolean(objCommon.GetSettingsValueByKey(interfaceSettings, "IncludeTaxInSplit"), NumberFormatInfo.InvariantInfo);
                useTaxMaster = Convert.ToBoolean(objCommon.GetSettingsValueByKey(interfaceSettings, "UseTaxMaster"), NumberFormatInfo.InvariantInfo);

                settingValue = objCommon.GetSettingsValueByKey(interfaceSettings, "NoOfDaysForDateRequested");
                noOfDaysForDateRequested = string.IsNullOrEmpty(settingValue) ? 0 : Convert.ToInt16(settingValue);


                settingValue = objCommon.GetSettingsValueByKey(p2pSettings, "AllowDeliverToFreeText");
                bool deliverToFreeText = !string.IsNullOrEmpty(settingValue) ? Convert.ToBoolean(settingValue) : false;

                settingValue = objCommon.GetSettingsValueByKey(interfaceSettings, "AllowOverridingOrgEntityFromCatalog");
                allowOverridingOrgEntityFromCatalog = !string.IsNullOrEmpty(settingValue) ? Convert.ToBoolean(settingValue) : true;

                settingValue = objCommon.GetSettingsValueByKey(p2pSettings, "AllowNegativeValues");
                var allowNegativeValues = !string.IsNullOrEmpty(settingValue) ? Convert.ToInt16(settingValue) : 0;

                settingValue = objCommon.GetSettingsValueByKey(p2pSettings, "EntityMappedToBillToLocation");
                EntityMappedToBillToLocation = string.IsNullOrEmpty(settingValue) ? 0 : Convert.ToInt32(settingValue);

                settingValue = objCommon.GetSettingsValueByKey(p2pSettings, "EntityMappedToShipToLocation");
                EntityMappedToShipToLocation = string.IsNullOrEmpty(settingValue) ? 0 : Convert.ToInt32(settingValue);

                settingValue = objCommon.GetSettingsValueByKey(p2pSettings, "IsOperationalBudgetEnabled");
                isOperationalBudgetEnabled = string.IsNullOrEmpty(settingValue) ? false : Convert.ToBoolean(settingValue);

                settingValue = objCommon.GetSettingsValueByKey(interfaceSettings, "IsClientCodeBasedonLinkLocation");
                IsClientCodeBasedonLinkLocation = string.IsNullOrEmpty(settingValue) ? 0 : Convert.ToInt16(settingValue);

                settingValue = objCommon.GetSettingsValueByKey(interfaceSettings, "IsDeltaAmount");
                IsDeltaAmount = string.IsNullOrEmpty(settingValue) ? 0 : Convert.ToInt16(settingValue);

                settingValue = objCommon.GetSettingsValueByKey(interfaceSettings, "DeriveHeaderEntities");
                bool DeriveHeaderEntities = !string.IsNullOrEmpty(settingValue) ? Convert.ToBoolean(settingValue) : false;

                settingValue = objCommon.GetSettingsValueByKey(interfaceSettings, "DeriveItemDetails");
                IsDeriveItemDetailEnable = !string.IsNullOrEmpty(settingValue) ? Convert.ToBoolean(settingValue) : false;

                settingValue = objCommon.GetSettingsValueByKey(RequisitionSettings, "IsOrderingLocationMandatory");
                IsOrderingLocationMandatory = !string.IsNullOrEmpty(settingValue) ? Convert.ToBoolean(settingValue) : false;

                settingValue = objCommon.GetSettingsValueByKey(p2pSettings, "IsDefaultOrderingLocation");
                IsDefaultOrderingLocation = !string.IsNullOrEmpty(settingValue) ? Convert.ToBoolean(settingValue) : false;

                settingValue = objCommon.GetSettingsValueByKey(_catalogSettings, "IncludePriceDetails");
                isIncludePriceDetails = !string.IsNullOrEmpty(settingValue) ? Convert.ToBoolean(settingValue) : false;

                settingValue = objCommon.GetSettingsValueByKey(interfaceSettings, "CallGetLineItemsAPIOnRequisition");
                CallGetLineItemsAPIOnRequisition = string.IsNullOrEmpty(settingValue) ? 0 : Convert.ToInt16(settingValue);

                settingValue = objCommon.GetSettingsValueByKey(interfaceSettings, "IsCaptureCatalogApiResponse");
                IsCaptureCatalogApiResponse = !string.IsNullOrEmpty(settingValue) ? Convert.ToBoolean(settingValue) : false;

                var precessionValue = convertStringToInt(objCommon.GetSettingsValueByKey(P2PDocumentType.None, "MaxPrecessionValue", UserContext.ContactCode, (int)SubAppCodes.P2P, "", lobEntityDetailCode));
                maxPrecessionforTotal = convertStringToInt(objCommon.GetSettingsValueByKey(P2PDocumentType.None, "MaxPrecessionValueforTotal", UserContext.ContactCode, (int)SubAppCodes.P2P, "", lobEntityDetailCode));
                maxPrecessionForTaxesAndCharges = convertStringToInt(objCommon.GetSettingsValueByKey(P2PDocumentType.None, "MaxPrecessionValueForTaxesAndCharges", UserContext.ContactCode, (int)SubAppCodes.P2P, "", lobEntityDetailCode));
                bool allowTaxCodewithAmount = Convert.ToBoolean(objCommon.GetSettingsValueByKey(P2PDocumentType.Invoice, "AllowTaxCodewithAmount", UserContext.ContactCode, (int)SubAppCodes.P2P, "", lobEntityDetailCode));
                string supplierStatusForValidation = objCommon.GetSettingsValueByKey(P2PDocumentType.Invoice, "SupplierStatusForValidation", UserContext.ContactCode, (int)SubAppCodes.P2P, "", lobEntityDetailCode);

                IsHeaderEntityAccessControlEntity = SettingsHelper.GetAccessControlSettingsForDocument(UserContext, (int)DocumentType.Requisition);

                settingValue = objCommon.GetSettingsValueByKey(p2pSettings, "PartnerStatuses");
                string PartnerStatuscodes = !string.IsNullOrEmpty(settingValue) ? Convert.ToString(settingValue) : string.Empty;

                settingValue = objCommon.GetSettingsValueByKey(interfaceSettings, "DerivePartnerFromLocationCode");
                bool DerivePartnerFromLocationCode = !string.IsNullOrEmpty(settingValue) ? Convert.ToBoolean(settingValue) : false;

                settingValue = objCommon.GetSettingsValueByKey(p2pSettingsSpendControlType, "SpendControlType");
                var SpendControlType = string.IsNullOrEmpty(settingValue) ? 0 : Convert.ToInt16(settingValue);

                #endregion

                objRequisition.Precision = (Int16)precessionValue;

                List<RequisitionItem> RequisitionItems = GetReqDao().GetLineItemBasicDetails(objExistingRequisition.DocumentCode, ItemType.None, 0, 1000, "", "", 0, 1, "", precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges).Cast<RequisitionItem>().ToList();
                objExistingRequisition.RequisitionItems = RequisitionItems;

                bool isContainHeaderEntity = false;
                if (objRequisition.DocumentAdditionalEntitiesInfoList != null && objRequisition.DocumentAdditionalEntitiesInfoList.Count > 0)
                    isContainHeaderEntity = true;

                if (objRequisition.DocumentBUList != null && objRequisition.DocumentBUList.Count > 0)
                    SetRequisitionAdditionalEntityFromInterface_New(objRequisition, out headerEntityId, out headerEntityName, objRequisition.DocumentLOBDetails.FirstOrDefault().EntityCode);


                /*Validation Part Starts*/
                SettingDetails objSettings = objCommon.GetSettingsFromSettingsComponent(P2PDocumentType.Interfaces, UserContext.ContactCode, (int)SubAppCodes.Interfaces);
                var lstErrors = new List<string>();
                var dctSetting = objSettings.lstSettings.Distinct().ToDictionary(sett => sett.FieldName, sett => sett.FieldValue);

                var Error = ValidateShipToBillToFromInterface(objCommon, objRequisition, dctSetting, out isHeaderShipToValid, out isLiShipToValid, objRequisition.DocumentLOBDetails[0].EntityDetailCode);

                #region 'Bill To Location'

                /* Logic To Save Adhoc BillToLocation */
                if (objRequisition.BilltoLocation != null && objRequisition.BilltoLocation.BilltoLocationId != 0)
                {
                    if (objRequisition.DocumentLOBDetails != null && objRequisition.DocumentLOBDetails.Any() && !string.IsNullOrEmpty(objRequisition.DocumentLOBDetails.FirstOrDefault().EntityCode))
                    {
                        objRequisition.BilltoLocation.LOBEntityCode = objRequisition.DocumentLOBDetails.FirstOrDefault().EntityCode;
                    }
                    if (objRequisition.DocumentAdditionalEntitiesInfoList != null && objRequisition.DocumentAdditionalEntitiesInfoList.Any())
                    {
                        var _lstOrgEntity = from listOrgEntityCodes in objRequisition.DocumentAdditionalEntitiesInfoList.Where(_enityId => _enityId.EntityId == EntityMappedToBillToLocation)
                                            select listOrgEntityCodes;

                        if (_lstOrgEntity != null && _lstOrgEntity.Count() > 0)
                        {
                            List<DocumentAdditionalEntityInfo> lstDocumentAdditionalEntityInfo = new List<DocumentAdditionalEntityInfo>();

                            lstDocumentAdditionalEntityInfo = _lstOrgEntity.Cast<DocumentAdditionalEntityInfo>().ToList();

                            objRequisition.BilltoLocation.lstOrgEntity = lstDocumentAdditionalEntityInfo.ConvertAll(x => new P2P.BusinessEntities.BZOrgEntity()
                            {
                                EntityCode = x.EntityCode,
                                EntityType = x.EntityDisplayName,
                                IsDefault = false
                            });
                        }
                    }
                    objRequisition.BilltoLocation = objCommon.SaveBillToLocation(objRequisition.BilltoLocation);// INDEXING IN RESPECTIVE MANAGER OR BELOW ORDER INDEX
                }
                #endregion 'Bill To Location'

                #region Deliver To Location           
                if (deliverToFreeText == false)
                {
                    if (objRequisition.DelivertoLocation != null && !string.IsNullOrWhiteSpace(objRequisition.DelivertoLocation.DelivertoLocationNumber))
                    {
                        var deliverTo = objCommon.GetDeliverToLocationByNumber(objRequisition.DelivertoLocation.DelivertoLocationNumber);
                        if (!object.ReferenceEquals(deliverTo, null) && deliverTo.DelivertoLocationId > 0)
                        {
                            objRequisition.DelivertoLocation.DelivertoLocationId = deliverTo.DelivertoLocationId;
                        }
                    }
                }
                headerDeliverto = objRequisition.DelivertoLocation != null ? objRequisition.DelivertoLocation.DeliverTo : "";
                #endregion

                #region 'Save Ship To Location'
                LogHelper.LogInfo(Log, "UpdateRequisitionFromInterface : AddUpdateShipToDetail Method Started  : " + obj.DocumentNumber);
                if (objRequisition.ShiptoLocation != null && !isLiShipToValid && isHeaderShipToValid && objRequisition.ShiptoLocation.ShiptoLocationId <= 0)
                {
                    if (objRequisition.DocumentLOBDetails != null && objRequisition.DocumentLOBDetails.Any() && !string.IsNullOrEmpty(objRequisition.DocumentLOBDetails.FirstOrDefault().EntityCode))
                    {
                        objRequisition.ShiptoLocation.LOBEntityCode = objRequisition.DocumentLOBDetails.FirstOrDefault().EntityCode;
                    }
                    if (objRequisition.DocumentAdditionalEntitiesInfoList != null && objRequisition.DocumentAdditionalEntitiesInfoList.Any())
                    {
                        var _lstOrgEntity = from listOrgEntityCodes in objRequisition.DocumentAdditionalEntitiesInfoList.Where(_enityId => _enityId.EntityId == EntityMappedToShipToLocation)
                                            select listOrgEntityCodes;

                        if (_lstOrgEntity != null && _lstOrgEntity.Count() > 0)
                        {
                            List<DocumentAdditionalEntityInfo> lstDocumentAdditionalEntityInfo = new List<DocumentAdditionalEntityInfo>();

                            lstDocumentAdditionalEntityInfo = _lstOrgEntity.Cast<DocumentAdditionalEntityInfo>().ToList();

                            objRequisition.ShiptoLocation.lstOrgEntity = lstDocumentAdditionalEntityInfo.ConvertAll(x => new P2P.BusinessEntities.BZOrgEntity()
                            {
                                EntityCode = x.EntityCode,
                                EntityType = x.EntityDisplayName,
                                IsDefault = false
                            });
                        }
                    }
                    int _newShipToLocationId = objCommon.AddUpdateShipToDetail_New(objRequisition.ShiptoLocation, ref objCommon, true);
                    if (_newShipToLocationId != objRequisition.ShiptoLocation.ShiptoLocationId)
                    {
                        objRequisition.ShiptoLocation.ShiptoLocationId = _newShipToLocationId;
                    }
                }
                #endregion 'Ship To Location'

                #region "finding BU"
                if (useDocumentLOB && accessControlId > 0)
                {
                    string LobEntityCode = "";
                    if (objRequisition.DocumentLOBDetails != null && objRequisition.DocumentLOBDetails.Any() && !string.IsNullOrEmpty(objRequisition.DocumentLOBDetails.FirstOrDefault().EntityCode))
                    {
                        LobEntityCode = objRequisition.DocumentLOBDetails.FirstOrDefault().EntityCode;
                    }
                    SetRequisitionAdditionalEntityFromInterface_New(objRequisition, out headerEntityId, out headerEntityName, LobEntityCode);

                    if (objRequisition.DocumentAdditionalEntitiesInfoList != null && objRequisition.DocumentAdditionalEntitiesInfoList.Where(a => a.EntityId == accessControlId) != null
                        && objRequisition.DocumentAdditionalEntitiesInfoList.Where(a => a.EntityId == accessControlId).Any())
                    {
                        var entity = objRequisition.DocumentAdditionalEntitiesInfoList.Where(a => a.EntityId == accessControlId).FirstOrDefault();
                        objRequisition.DocumentBUList.Clear();
                        objRequisition.DocumentBUList.Add(new DocumentBU() { BusinessUnitCode = entity.EntityDetailCode, BusinessUnitName = entity.EntityDisplayName });
                        objRequisition.BusinessUnitId = entity.EntityDetailCode;
                    }
                }
                else if (objRequisition.DocumentAdditionalEntitiesInfoList != null)
                {
                    if (lstBUDetails.Where(data => data.EntityCode == objRequisition.DocumentAdditionalEntitiesInfoList.First().EntityCode) != null &&
                        lstBUDetails.Where(data => data.EntityCode == objRequisition.DocumentAdditionalEntitiesInfoList.First().EntityCode).Count() > 0)
                    {
                        objRequisition.BusinessUnitId = lstBUDetails.Where(data => data.EntityCode == objRequisition.DocumentAdditionalEntitiesInfoList.First().EntityCode).FirstOrDefault().OrgEntityCode;  // need to check           
                    }
                    else { objRequisition.BusinessUnitId = lstBUDetails != null
                                              ? lstBUDetails.Where(data => data.IsDefault) != null && lstBUDetails.Where(data => data.IsDefault).Any() ? lstBUDetails.Where(data => data.IsDefault).FirstOrDefault().OrgEntityCode
                                              : lstBUDetails.Where(data => data.EntityId == accessControlId) != null && lstBUDetails.Where(data => data.EntityId == accessControlId).Any() ? lstBUDetails.Where(data => data.EntityId == accessControlId).FirstOrDefault().OrgEntityCode
                                              : 0 : 0;
                    }
                }
                else
                {
                    objRequisition.BusinessUnitId = lstBUDetails != null
                                                 ? lstBUDetails.Where(data => data.IsDefault) != null && lstBUDetails.Where(data => data.IsDefault).Any() ? lstBUDetails.Where(data => data.IsDefault).FirstOrDefault().OrgEntityCode
                                                 : lstBUDetails.Where(data => data.EntityId == accessControlId) != null && lstBUDetails.Where(data => data.EntityId == accessControlId).Any() ? lstBUDetails.Where(data => data.EntityId == accessControlId).FirstOrDefault().OrgEntityCode
                                                 : 0 : 0;
                }

                #endregion "finding BU"

                #region For ApprovalPending Document
                if (IsHeaderEntityAccessControlEntity)
                {
                    if (objRequisition.DocumentAdditionalEntitiesInfoList != null && objRequisition.DocumentAdditionalEntitiesInfoList.Count() > 0)
                    {
                        headerOrgEntityDetailCode = objRequisition.DocumentAdditionalEntitiesInfoList.Where(_EntityID => _EntityID.EntityId == accessControlId).FirstOrDefault().EntityDetailCode;
                    }
                }
                else
                {
                    headerOrgEntityDetailCode = 0;
                }
                #endregion

                if (!String.IsNullOrEmpty(objBZRequisition.Requisition.RequesterPASCode))
                {
                    objRequisition.RequesterId = objCommon.GetContactCodeByClientContactCodeOrEmail(objBZRequisition.Requisition.RequesterPASCode, "");
                }
                /* NEED TO CHECK */
                #region 'Validate Catalog Line Items and splits.'

                lstEntityDeails = objRequisition.RequisitionItems.Where(itm => itm.ItemSplitsDetail != null).SelectMany(itmSplt => itmSplt.ItemSplitsDetail).SelectMany(splt => splt.DocumentSplitItemEntities).ToList();

                if (lstEntityDeails != null && lstEntityDeails.Count == 0)
                {
                    if (objRequisition.DocumentAdditionalEntitiesInfoList != null && objRequisition.DocumentAdditionalEntitiesInfoList.Any())
                    {
                        defaultAccDetails = objDocBO.GetDocumentDefaultAccountingDetails(P2PDocumentType.Requisition, LevelType.ItemLevel, UserContext.ContactCode, 0, objRequisition.DocumentAdditionalEntitiesInfoList.ToList(), null, false, 0, lobEntityDetailCode);
                        setDefaultSplit = true;
                    }
                }
                else
                {
                    string EntityCode = string.Empty;
                    EntityCode = objRequisition.DocumentAdditionalEntitiesInfoList != null && objRequisition.DocumentAdditionalEntitiesInfoList.Any() ? objRequisition.DocumentAdditionalEntitiesInfoList.FirstOrDefault().EntityCode : "";
                    result = GetReqInterfaceDao().GetSplitsDetails(objRequisition.RequisitionItems, objRequisition.CreatedBy, lobEntityDetailCode, EntityCode);
                    if (DeriveHeaderEntities)
                    {
                        objCommon.UpdateSplitDeatilsBasedOnHeaderEntity(objRequisition.DocumentAdditionalEntitiesInfoList.ToList(), result);
                    }
                    List<RequisitionItem> nonSplitItem = objRequisition.RequisitionItems.Where(a => a.ItemSplitsDetail != null && a.ItemSplitsDetail.Count == 0).ToList();

                    if (result != null && result.Tables["SplitEntity"].Rows.Count > 0)
                    {
                        foreach (var itm in objRequisition.RequisitionItems)
                        {
                            splitCounters = 1;
                            foreach (RequisitionSplitItems itemSplitDetail in itm.ItemSplitsDetail)
                            {
                                itemSplitDetail.DocumentSplitItemId = 0;
                                itemSplitDetail.UiId = splitCounters;
                                itemSplitDetail.DocumentItemId = itm.DocumentItemId;
                                itemSplitDetail.SplitType = SplitType.Percentage;

                                var splititems = result.Tables[0].AsEnumerable().Where(row => Convert.ToInt64(row["ItemLineNumber"]) == itm.ItemLineNumber && splitCounters == Convert.ToInt64(row["Uids"]));

                                if (splititems != null && splititems.Any())
                                {
                                    List<DocumentSplitItemEntity> obJ = new List<DocumentSplitItemEntity>();
                                    foreach (DataRow split in splititems)
                                    {

                                        obJ.Add(
                                            new DocumentSplitItemEntity()
                                            {
                                                SplitAccountingFieldId = (int)split["SplitAccountingFieldConfigId"],
                                                UiId = (int)split["Uids"],
                                                SplitAccountingFieldValue = Convert.ToString(split["EntityDetailCode"]),
                                                EntityTypeId = (int)split["EntityTypeId"],
                                                EntityCode = Convert.ToString(split["EntityCode"]),

                                            });

                                    }

                                    itemSplitDetail.DocumentSplitItemEntities = obJ;

                                }
                                splitCounters++;
                            }
                        }
                    }
                    if (nonSplitItem.Count > 0 && objRequisition.DocumentAdditionalEntitiesInfoList != null && objRequisition.DocumentAdditionalEntitiesInfoList.Any())
                    {
                        splitCounters = 1;
                        defaultAccDetails = objDocBO.GetDocumentDefaultAccountingDetails(P2PDocumentType.Requisition, LevelType.ItemLevel, UserContext.ContactCode, 0, objRequisition.DocumentAdditionalEntitiesInfoList.ToList(), null, false, 0, lobEntityDetailCode);
                        //defaultAccDetails.ForEach(splt => splt.UiId = splitCounters);
                        foreach (var itm in nonSplitItem)
                        {
                            defaultAccDetails.ForEach(splt => splt.UiId = splitCounters);
                            itm.ItemSplitsDetail = new List<RequisitionSplitItems>()
                                                            {
                                                                new RequisitionSplitItems(){
                                                                    DocumentSplitItemEntities = defaultAccDetails,
                                                                    SplitType = SplitType.Percentage,
                                                                    DocumentItemId = itm.DocumentItemId,
                                                                    UiId = splitCounters
                                                                }
                                                            };
                            splitCounters++;


                        }
                    }
                }
                #endregion 'Validate Catalog Line Items.'
                /* NEED TO CHECK */

                /* NEED TO CHECK */
                #region 'Setting for Requisition Status'

                int _tempDocumentStatus = 0;
                if (objRequisition.DocumentStatusInfo == Documents.Entities.DocumentStatus.None)
                {
                    settingValue = objCommon.GetSettingsValueByKey(interfaceSettings, "RequisitionStatus");
                    DocumentStatus = string.IsNullOrEmpty(settingValue) ? 1 : Convert.ToInt32(settingValue);
                    if (DocumentStatus > 1)
                    {
                        _tempDocumentStatus = DocumentStatus;
                        objRequisition.DocumentStatusInfo = (Documents.Entities.DocumentStatus)(Convert.ToInt32(DocumentStatus));
                    }
                    else
                        objRequisition.DocumentStatusInfo = Documents.Entities.DocumentStatus.Draft;
                }
                else
                {
                    if (objRequisition.DocumentStatusInfo == Documents.Entities.DocumentStatus.ApprovalPending || objRequisition.DocumentStatusInfo == Documents.Entities.DocumentStatus.Approved)
                    {
                        _tempDocumentStatus = Convert.ToInt32(objRequisition.DocumentStatusInfo);
                        objRequisition.DocumentStatusInfo = Documents.Entities.DocumentStatus.Draft;
                    }
                }
                #endregion 'Setting for Requisition Status'
                /* NEED TO CHECK */

                #region Items
                if (objRequisition.RequisitionItems != null && objRequisition.RequisitionItems.Count > 0)
                {
                    List<KeyValuePair<string, decimal>> lstKvpPartnerDetails = new List<KeyValuePair<string, decimal>>();
                    List<PartnerLocation> orderingLoc = new List<PartnerLocation>();
                    foreach (var item in objRequisition.RequisitionItems)
                    {
                        #region Set Partner Code
                        if (lstKvpPartnerDetails.Where(data => data.Key == item.ClientPartnerCode).Any())
                            item.PartnerCode = lstKvpPartnerDetails.Where(data => data.Key == item.ClientPartnerCode).FirstOrDefault().Value;
                        else
                        {
                            if (IsClientCodeBasedonLinkLocation == 1)
                            {
                                PartnerLinkedLocationMapping linklocation = new PartnerLinkedLocationMapping();
                                linklocation = objCommon.GetLinkedLocationBySourceSystemValue(item.ClientPartnerCode);
                                item.PartnerCode = linklocation.PartnerCode;
                                item.OrderLocationId = linklocation.LocationId;
                                item.RemitToLocationId = linklocation.LinkedLocationId; //to uncomment once web make this changes

                            }
                            else
                            {
                                string SourcesystemName = objRequisition.SourceSystemInfo != null ? objRequisition.SourceSystemInfo.SourceSystemName : string.Empty;
                                if (DerivePartnerFromLocationCode)
                                {
                                    DataTable dtPartnerDetails = new DataTable();
                                    dtPartnerDetails = GetCommonDao().GetPartnerandContactCodeDetails(item.ClientPartnerCode, "", "", this.UserContext.BuyerPartnerCode, "", item.ClientPartnerCode, "",
                                                        SourcesystemName, true, PartnerStatuscodes);
                                    if (dtPartnerDetails != null && dtPartnerDetails.Rows != null && dtPartnerDetails.Rows.Count > 0)
                                    {
                                        item.PartnerCode = Convert.ToInt64(dtPartnerDetails.Rows[0]["PartnerCode"]);
                                        item.OrderLocationId = Convert.ToInt64(dtPartnerDetails.Rows[0]["LocationCode"]);
                                    }
                                }
                                else
                                {
                                    item.PartnerCode = objCommon.GetPartnerCodeByClientPartnerCode(item.ClientPartnerCode, SourcesystemName);

                                    if (item.PartnerCode > 0)
                                        lstKvpPartnerDetails.Add(new KeyValuePair<string, decimal>(item.ClientPartnerCode, item.PartnerCode));
                                }
                            }
                        }

                        #endregion Set Partner Code

                        if (IsOrderingLocationMandatory)
                        {
                            long partnercode = 0;
                            string headerEntities = string.Empty;
                            if (item.PartnerCode > 0)
                                partnercode = Convert.ToInt64(item.PartnerCode);
                            if (objRequisition.DocumentAdditionalEntitiesInfoList != null && objRequisition.DocumentAdditionalEntitiesInfoList.Count > 0)
                                headerEntities = string.Join(",", objRequisition.DocumentAdditionalEntitiesInfoList.Select(entity => entity.EntityDetailCode.ToString()));
                            item.OrderLocationId = GetReqDao().GetOrderLocationIdByClientLocationCode(item.OrderLocationName, partnercode, headerEntities, IsDefaultOrderingLocation);
                        }

                        if (!string.IsNullOrWhiteSpace(item.ItemNumber))
                        {
                            var dr = dtItems.NewRow();
                            dr["BuyerItemNumber"] = item.ItemNumber;
                            dr["PartnerCode"] = item.PartnerCode;
                            dr["UOM"] = item.UOM;
                            dtItems.Rows.Add(dr);
                        }

                        #region Set Category Id based on Unspsc Id
                        if (item.Unspsc > 0)
                        {
                            if (lstKvpCategoryUnspsc.Where(data => data.Key == item.Unspsc).Any())
                            {
                                item.CategoryId = lstKvpCategoryUnspsc.Where(data => data.Key == item.Unspsc).FirstOrDefault().Value;
                            }
                            else
                            {
                                item.CategoryId = objCommon.GetPASCodeFromUNSPSCId(item.Unspsc);
                                if (item.CategoryId > 0)
                                {
                                    lstKvpCategoryUnspsc.Add(new KeyValuePair<long, long>(item.Unspsc, item.CategoryId));
                                }
                            }
                        }
                        #endregion
                        else
                        {
                            #region Set Category Id based on Client Category Id
                            if (!string.IsNullOrEmpty(item.ClientCategoryId))
                            {
                                if (lstKvpCategoryDetails.Where(data => data.Key == item.ClientCategoryId).Any())
                                    item.CategoryId = lstKvpCategoryDetails.Where(data => data.Key == item.ClientCategoryId).FirstOrDefault().Value;
                                else
                                {
                                    var requestHeaders = new GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.RequestHeaders();
                                    requestHeaders.Set(this.UserContext, this.JWTToken);
                                    string appName = Environment.GetEnvironmentVariable("NewRelic.AppName") ?? "Requisition"; 
                                    string useCase = "RequisitionInterfaceManager-UpdateRequisitionDetailsFromInterface";
                                    var serviceURL = MultiRegionConfig.GetConfig(CloudConfig.APIMBaseURL) + GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.ServiceURLs.CategoryServiceURL + "GetCategoryCodeForClientCategoryCode?categoryCode=" + item.ClientCategoryId;

                                    var webAPI = new GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.WebAPI(requestHeaders, appName, useCase);
                                    var response = webAPI.ExecuteGet(serviceURL);
                                    item.CategoryId = Convert.ToInt64(response);

                                    if (item.CategoryId > 0)
                                        lstKvpCategoryDetails.Add(new KeyValuePair<string, long>(item.ClientCategoryId, item.CategoryId));
                                }
                            }
                            #endregion
                        }

                        if (isIncludePriceDetails)
                        {
                            settingValue = objCommon.GetSettingsValueByKey(interfaceSettings, "CallWebAPIOnRequisition");
                            bool CallWebAPIOnRequisition = !string.IsNullOrEmpty(settingValue) ? Convert.ToBoolean(settingValue) : false;

                            if (CallGetLineItemsAPIOnRequisition == 1)
                                SetSourceTypeForPetronas(item, objRequisition, IsCaptureCatalogApiResponse, CallWebAPIOnRequisition);
                        }
                    }
                }
                #endregion Items

                #region catalog validation

                if (dtItems.Rows != null && dtItems.Rows.Count > 0 && CallGetLineItemsAPIOnRequisition != 1)
                {
                    dtResult = objDocBO.ValidateInternalCatalogItems(P2PDocumentType.Requisition, dtItems);

                    if (dtResult.Rows != null && dtResult.Rows.Count > 0)
                    {
                        foreach (DataRow dtRow in dtResult.Rows)
                        {
                            if (dtRow["ItemStatus"].ToString() == "F")
                                strErrors.AppendLine(',' + dtRow["ItemNumberWithDesc"].ToString());
                        }
                    }

                }
                dtItems.Dispose();
                #endregion catalog validation

                #region DateNeeded, DateRequested
                foreach (var item in objRequisition.RequisitionItems)
                {

                    if (!string.IsNullOrWhiteSpace(item.ItemNumber))
                    {
                        var matchingRow = dtResult.AsEnumerable().Where(dr => Convert.ToString(dr["ItemNumberWithDesc"]).Equals(item.ItemNumber, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                        if (matchingRow != null)
                        {
                            if (item.CategoryId <= 0 && item.Unspsc <= 0)
                            {
                                item.CategoryId = Convert.ToInt64(matchingRow["CategoryId"].ToString());
                                if (item.CategoryId <= 0)
                                    item.Unspsc = Convert.ToInt32(matchingRow["Unspsc"].ToString());
                            }
                            item.CatalogItemId = Convert.ToInt64(matchingRow["CatalogItemId"].ToString());
                            item.SourceType = (ItemSourceType)Convert.ToInt16(matchingRow["CatalogSource"].ToString());
                            item.IsTaxExempt = Convert.ToBoolean(matchingRow["IsTaxExempt"].ToString());
                        }
                    }
                    #region Derive Item Detail from Catalog/Item Master if Setting (IsDeriveItemDetailEnable) is ON

                    if (IsDeriveItemDetailEnable == true && CallGetLineItemsAPIOnRequisition != 1)
                    {
                        if (item.ItemNumber != null && item.ItemNumber != "" && item.PartnerSourceSystemValue != null && item.PartnerSourceSystemValue != ""
                            && item.UOM != null && item.UOM != "")
                        {
                            DataTable dt = objDocBO.DeriveItemDetails(item.ItemNumber, item.PartnerSourceSystemValue, item.UOM);

                            if (dt.Rows != null && dt.Rows.Count > 0)
                            {
                                foreach (DataRow dtRow in dt.Rows)
                                {
                                    if (string.IsNullOrEmpty(item.Description))
                                    {
                                        item.Description = dtRow["ItemDescription"].ToString();
                                    }

                                    if (string.IsNullOrEmpty(item.ShortName))
                                    {
                                        item.ShortName = dtRow["ShortName"].ToString();
                                    }

                                    if (string.IsNullOrEmpty(item.SupplierItemNumber))
                                    {
                                        item.SupplierItemNumber = dtRow["SupplierItemNumber"].ToString();
                                        item.SupplierPartId = dtRow["SupplierItemNumber"].ToString();
                                    }

                                    if (item.UnitPrice == null)
                                    {
                                        if (dtRow.Table.Columns.Contains("FromUOM"))
                                        {
                                            if (item.UOM == dtRow["FromUOM"].ToString())
                                            {
                                                decimal FromConversionFactor;

                                                if (dtRow["FromConversionFactor"].ToString() != null && dtRow["FromConversionFactor"].ToString() != "")
                                                {
                                                    FromConversionFactor = Convert.ToDecimal(dtRow["FromConversionFactor"].ToString());

                                                    if (dtRow["UnitPrice"].ToString() != null && dtRow["UnitPrice"].ToString() != "")
                                                    {
                                                        item.UnitPrice = Convert.ToDecimal(dtRow["UnitPrice"].ToString()) / FromConversionFactor;
                                                    }
                                                }

                                            }

                                            else if (item.UOM == dtRow["ToUOM"].ToString())
                                            {
                                                decimal ConversionFactor;

                                                if (dtRow["ConversionFactor"].ToString() != null && dtRow["ConversionFactor"].ToString() != "")
                                                {
                                                    ConversionFactor = Convert.ToDecimal(dtRow["ConversionFactor"].ToString());

                                                    if (dtRow["UnitPrice"].ToString() != null && dtRow["UnitPrice"].ToString() != "")
                                                    {
                                                        item.UnitPrice = Convert.ToDecimal(dtRow["UnitPrice"].ToString()) / ConversionFactor;
                                                    }
                                                }
                                            }
                                        }

                                        else
                                        {
                                            if (dtRow["UnitPrice"].ToString() != null && dtRow["UnitPrice"].ToString() != "")
                                                item.UnitPrice = Convert.ToDecimal(dtRow["UnitPrice"].ToString());
                                        }
                                    }

                                    if (item.DateNeeded == null || item.DateNeeded == DateTime.MinValue)
                                    {
                                        int LeadTime = 0;

                                        if (dtRow.Table.Columns.Contains("LeadTime"))
                                        {
                                            if (dtRow["LeadTime"].ToString() != null && dtRow["LeadTime"].ToString() != "")
                                                LeadTime = Convert.ToInt32(dtRow["LeadTime"].ToString());

                                            if (LeadTime > 0)
                                                item.DateNeeded = DateTime.Now.AddDays(LeadTime);
                                        }
                                    }

                                    if (item.CategoryId <= 0)
                                    {
                                        if (dtRow.Table.Columns.Contains("CategoryID"))
                                        {
                                            if (dtRow["CategoryID"].ToString() != null && dtRow["CategoryID"].ToString() != "")
                                                item.CategoryId = Convert.ToInt64((dtRow["CategoryID"].ToString()));
                                        }
                                    }

                                    if (item.ManufacturerName == null || item.ManufacturerName == "")
                                        item.ManufacturerName = dtRow["ManufacturerName"].ToString();

                                    if (item.ManufacturerPartNumber == null || item.ManufacturerPartNumber == "")
                                        item.ManufacturerPartNumber = dtRow["ManufacturerPartNumber"].ToString();

                                    if (dtRow.Table.Columns.Contains("CatalogItemID") && dtRow.Table.Columns.Contains("ItemId"))
                                    {
                                        if (!string.IsNullOrEmpty(dtRow["CatalogItemID"].ToString()) && !string.IsNullOrEmpty(dtRow["ItemId"].ToString()))
                                        {
                                            var objdr = objdt.NewRow();
                                            objdr["ItemNumber"] = item.ItemNumber;
                                            objdr["CatalogItemID"] = Convert.ToInt64(dtRow["CatalogItemID"].ToString());
                                            objdr["ItemMasterItemId"] = Convert.ToInt64(dtRow["ItemId"].ToString());
                                            objdr["tardocumentType"] = 7;
                                            objdt.Rows.Add(objdr);
                                        }
                                    }

                                    else if (dtRow.Table.Columns.Contains("ItemId"))
                                    {
                                        if (dtRow["ItemId"].ToString() != null && dtRow["ItemId"].ToString() != "")
                                        {
                                            var objdr = objdt.NewRow();
                                            objdr["ItemNumber"] = item.ItemNumber;
                                            objdr["CatalogItemID"] = 0;
                                            objdr["ItemMasterItemId"] = Convert.ToInt64(dtRow["ItemId"].ToString());
                                            objdr["tardocumentType"] = 7;
                                            objdt.Rows.Add(objdr);
                                        }
                                    }

                                    else if (dtRow.Table.Columns.Contains("CatalogItemID"))
                                    {
                                        if (dtRow["CatalogItemID"].ToString() != null && dtRow["CatalogItemID"].ToString() != "")
                                        {
                                            var objdr = objdt.NewRow();
                                            objdr["ItemNumber"] = item.ItemNumber;
                                            objdr["CatalogItemID"] = Convert.ToInt64(dtRow["CatalogItemID"].ToString());
                                            objdr["ItemMasterItemId"] = 0;
                                            objdr["tardocumentType"] = 7;
                                            objdt.Rows.Add(objdr);
                                        }
                                    }
                                }
                            }
                            if (item.ShortName == null && item.Description == null)
                            {
                                item.Description = "";
                                item.ShortName = "";
                            }
                        }

                        if (item.ShortName == null && item.Description == null)
                        {
                            item.Description = "";
                            item.ShortName = "";
                        }
                    }

                    else
                    {
                        if (item.Description == null)
                        {
                            item.Description = "";
                            item.ShortName = "";
                        }
                    }

                    #endregion
                    if ((useTaxMaster && Convert.ToDecimal(item.Tax) > 0) || item.IsTaxExempt)
                        item.Tax = 0;
                    item.ItemType = item.ItemType == ItemType.None ? ItemType.Material : item.ItemType;

                    item.CreatedBy = this.UserContext.ContactCode;
                    item.DateRequested = (item.DateRequested == null || item.DateRequested == DateTime.MinValue) ? DateTime.Now : item.DateRequested;

                    item.DateNeeded = (item.DateNeeded == null || item.DateNeeded == DateTime.MinValue) ? item.DateRequested.Value.AddDays(noOfDaysForDateRequested) : item.DateNeeded;

                    item.DateRequested = objDocBO.TimeStampCheck(Convert.ToDateTime(item.DateRequested));

                    item.DateNeeded = objDocBO.TimeStampCheck(Convert.ToDateTime(item.DateNeeded));

                    if (item.StartDate.HasValue)
                    {
                        item.StartDate = objDocBO.TimeStampCheck(Convert.ToDateTime(item.StartDate));
                    }
                    if (item.EndDate.HasValue)
                    {
                        item.EndDate = objDocBO.TimeStampCheck(Convert.ToDateTime(item.EndDate));
                    }


                }
                #endregion

                #region OnBeHalf Of
                if (objRequisition.RequesterId > 0)
                    objRequisition.OnBehalfOf = objRequisition.RequesterId;
                else
                    objRequisition.OnBehalfOf = this.UserContext.ContactCode;
                #endregion

                //NEED TO CHECK
                #region Source System
                if (objRequisition.SourceSystemInfo == null)
                {
                    objRequisition.SourceSystemInfo = new SourceSystemInfo
                    {
                        SourceSystemId = partnerInterfaceId
                    };
                }
                #endregion

                #region Save HeaderLevel Comments
                objCommon.SaveCommentsFromInterface(objRequisition.Comments, P2PDocumentType.Requisition, objRequisition.CreatedBy, objRequisition.DocumentCode, 1, objRequisition.DocumentNumber);
                #endregion

                #region Set Currency Code
                if (objRequisition.Currency == null || objRequisition.Currency == "")
                {
                    var cur = objRequisition.RequisitionItems.Where(items => items.Currency != "");
                    string currency = string.Empty;
                    if (cur.Count() > 0)
                    {
                        currency = cur.FirstOrDefault().Currency;
                    }

                    if (currency == "")
                    {
                        objRequisition.Currency = "USD";
                        foreach (var item in objRequisition.RequisitionItems)
                        {
                            item.Currency = objRequisition.Currency;
                        }
                    }
                    else
                    {
                        objRequisition.Currency = currency;
                        foreach (var item in objRequisition.RequisitionItems)
                        {
                            item.Currency = currency;
                        }
                    }
                    
                }
                else
                {
                    //Assign to ItemS only, Document curr
                    foreach (var item in objRequisition.RequisitionItems)
                    {
                        item.Currency = objRequisition.Currency;
                    }
                }
                #endregion

                var ItemTotal = objRequisition.RequisitionItems.Sum(itm => (itm.Quantity * itm.UnitPrice)).Value;


                #region Set PurchaseType
                var existingReq = GetReqInterfaceDao().GetRequisitionHeaderDetailsByIdForInterface(objExistingRequisition.DocumentCode);
                List<PurchaseType> lstPurchaseType = objCommon.GetPurchaseTypes();
                if (lstPurchaseType != null && lstPurchaseType.Any())
                {
                    if (existingReq != null && existingReq.Requisition != null && !string.IsNullOrEmpty(existingReq.Requisition.PurchaseTypeDescription))
                    {
                        objRequisition.PurchaseType = lstPurchaseType.Where(p => p.Description == existingReq.Requisition.PurchaseTypeDescription) != null && lstPurchaseType.Where(p => p.Description == existingReq.Requisition.PurchaseTypeDescription).Any() ?
                            lstPurchaseType.Where(p => p.Description == existingReq.Requisition.PurchaseTypeDescription).FirstOrDefault().PurchaseTypeId :
                            lstPurchaseType.Where(_id => _id.IsDefault == true).Count() > 0 ? lstPurchaseType.Where(_id => _id.IsDefault == true).FirstOrDefault().PurchaseTypeId : 0;
                    }
                    else
                        objRequisition.PurchaseType = lstPurchaseType.Where(_id => _id.IsDefault == true).Count() > 0 ? lstPurchaseType.Where(_id => _id.IsDefault == true).FirstOrDefault().PurchaseTypeId : 0;
                }
                #endregion

                //Saving Line Items
                objRequisition.DocumentCode = objDocBO.Save(P2PDocumentType.Requisition, objRequisition, false, 0, false, true, true);

                #region Delete HeaderLevel Sublines
                /*Delete Sublines For Line and HeaderLevel*/
                List<ItemCharge> templstItemCharges = new List<ItemCharge>();
                templstItemCharges = objCommon.GetItemChargesByDocumentCode((int)P2PDocumentType.Requisition, objRequisition.DocumentCode, 0);
                GetReqInterfaceDao().DeleteChargeAndSplitsItemsByItemChargeId(templstItemCharges, objRequisition.DocumentCode);

                #endregion // For Header ChargeItems

                #region save entity code mentioned in NewRequisition
                if (isContainHeaderEntity)
                    GetReqDao().SaveDocumentAdditionalEntityInfo(objRequisition.DocumentId, objRequisition.DocumentAdditionalEntitiesInfoList);
                #endregion

                #region Save Header Additional Fields

                SaveHeaderAdditionalAttributeFieldFromInterface(objRequisition.DocumentCode, objRequisition.PurchaseTypeDescription, objRequisition.lstAdditionalFieldAttributues);

                #endregion

                #region "get line item from flipped Requisition"

                //Requisition items flipped from old Requisition which may or may not present in change Requisition came through interface.
                List<RequisitionItem> ExistingItems = GetReqDao().GetLineItemBasicDetails(objRequisition.DocumentCode, ItemType.None, 0, 1000, "", "", 0, 1, "", precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges).Cast<RequisitionItem>().ToList();

                //Requisition items came through change Requisition received through Interface
                List<RequisitionItem> newItems = objRequisition.RequisitionItems.OrderBy(a => a.ItemLineNumber).Cast<RequisitionItem>().ToList();


                #endregion

                #region Delete RequisitionItems 

                var DelReqItemsList = from listp2pitems in ExistingItems.Where(itm => itm.DocumentId == newChangeRequistionId)
                                      where !newItems.Any(x => (x.ItemLineNumber == listp2pitems.ItemLineNumber))
                                      select listp2pitems;

                List<RequisitionItem> DeleteRequisitionItemList = DelReqItemsList.ToList();

                if (DeleteRequisitionItemList.Count > 0)
                {
                    for (int i = 0; i < DeleteRequisitionItemList.Count; i++)
                    {
                        objDocBO.DeleteLineItemByIds(P2PDocumentType.Requisition, Convert.ToString(DeleteRequisitionItemList[i].DocumentItemId), precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges);
                    }
                }

                #endregion

                #region Update Matched and Save New Items
                foreach (RequisitionItem RequisitionItem in newItems)
                {
                    //find out matching old item and assign new values to it and save.
                    var matchedItems = newItems.Join(ExistingItems, newItem => newItem.ItemLineNumber,
                                                        olditm => olditm.ItemLineNumber,
                                                        (newItem, olditm) => olditm).Where(line => line.ItemLineNumber == RequisitionItem.ItemLineNumber).FirstOrDefault();

                    var newRequisitionItem = new RequisitionItem();
                    newRequisitionItem = RequisitionItem;
                    newRequisitionItem.DocumentId = objRequisition.DocumentCode;
                    newRequisitionItem.StartDate = (newRequisitionItem.StartDate == null || (newRequisitionItem.StartDate != null && newRequisitionItem.StartDate == DateTime.MinValue)) ? DateTime.Now : newRequisitionItem.StartDate;
                    newRequisitionItem.EndDate = (newRequisitionItem.EndDate == null || (newRequisitionItem.EndDate != null && newRequisitionItem.EndDate == DateTime.MinValue)) ? DateTime.Now : newRequisitionItem.EndDate;
                    newRequisitionItem.CreatedBy = newRequisitionItem.ModifiedBy = objRequisition.CreatedBy;
                    newRequisitionItem.ItemType = newRequisitionItem.ItemType == ItemType.None ? ItemType.Material : newRequisitionItem.ItemType;
                    newRequisitionItem.ItemExtendedType = newRequisitionItem.ItemExtendedType == ItemExtendedType.None ? ItemExtendedType.Material : newRequisitionItem.ItemExtendedType;

                    if (matchedItems != null)
                    {
                        newRequisitionItem.P2PLineItemId = matchedItems.P2PLineItemId;
                        newRequisitionItem.DocumentItemId = matchedItems.DocumentItemId;
                        newRequisitionItem.DocumentItemShippingDetails = (newRequisitionItem.DocumentItemShippingDetails != null && newRequisitionItem.DocumentItemShippingDetails.Count() > 0) ? newRequisitionItem.DocumentItemShippingDetails : null;
                        newRequisitionItem.ItemType = matchedItems.ItemType;
                        newRequisitionItem.ItemExtendedType = matchedItems.ItemExtendedType;
                        newRequisitionItem.Unspsc = newRequisitionItem.Unspsc == 0 ? matchedItems.Unspsc : newRequisitionItem.Unspsc;
                        newRequisitionItem.CategoryId = newRequisitionItem.CategoryId == 0 ? matchedItems.CategoryId : newRequisitionItem.CategoryId;
                    }

                    if (newRequisitionItem != null && newRequisitionItem.Quantity == 0 && newRequisitionItem.UnitPrice == 0 && newRequisitionItem.DocumentItemId > 0)
                        objDocBO.CancelLineItem(P2PDocumentType.Requisition, newRequisitionItem.DocumentItemId);
                    else if (newRequisitionItem.Quantity != 0)
                    {
                        newRequisitionItem.DocumentItemId = objDocBO.SaveItem(P2PDocumentType.Requisition, newRequisitionItem, newRequisitionItem.IsTaxExempt, false, useTaxMaster, false);

                        ReqTotal_ += (newRequisitionItem.Quantity * newRequisitionItem.UnitPrice == null ? 0 : newRequisitionItem.UnitPrice) + newRequisitionItem.ShippingCharges == null ? 0 : newRequisitionItem.ShippingCharges + newRequisitionItem.Tax == null ? 0 : newRequisitionItem.Tax + newRequisitionItem.AdditionalCharges == null ? 0 : newRequisitionItem.AdditionalCharges;

                        if (newRequisitionItem.DocumentItemId > 0)
                        {
                            #region Partner Details
                            if (matchedItems != null)
                            {
                                newRequisitionItem.ManufacturerName = !string.IsNullOrEmpty(newRequisitionItem.ManufacturerName) ? newRequisitionItem.ManufacturerName : matchedItems.ManufacturerName;
                                newRequisitionItem.ManufacturerPartNumber = !string.IsNullOrEmpty(newRequisitionItem.ManufacturerPartNumber) ? newRequisitionItem.ManufacturerPartNumber : matchedItems.ManufacturerPartNumber;
                                newRequisitionItem.ManufacturerSupplierCode = !string.IsNullOrEmpty(newRequisitionItem.ManufacturerSupplierCode) ? newRequisitionItem.ManufacturerSupplierCode : matchedItems.ManufacturerSupplierCode;
                                newRequisitionItem.ManufacturerModel = !string.IsNullOrEmpty(newRequisitionItem.ManufacturerModel) ? newRequisitionItem.ManufacturerModel : matchedItems.ManufacturerModel;
                            }
                            objDocBO.SaveItemPartnerDetails(P2PDocumentType.Requisition, newRequisitionItem, precessionValue);
                            #endregion

                            #region Item Shipping Details
                            var lstDocumentItemShippingDetail = objDocBO.GetShippingSplitDetailsByLiId(P2PDocumentType.Requisition, newRequisitionItem.DocumentItemId).ToList();
                            var documentItemShippingId = 0l;
                            if (lstDocumentItemShippingDetail != null && lstDocumentItemShippingDetail.Any())
                                documentItemShippingId = lstDocumentItemShippingDetail.FirstOrDefault().DocumentItemShippingId;

                            if (newRequisitionItem.DocumentItemShippingDetails != null && newRequisitionItem.DocumentItemShippingDetails.Count > 0)
                            {
                                foreach (var shippingDetail in newRequisitionItem.DocumentItemShippingDetails)
                                {
                                    shippingDetail.DocumentItemId = newRequisitionItem.DocumentItemId;
                                    shippingDetail.Quantity = newRequisitionItem.Quantity;
                                    shippingDetail.DocumentItemShippingId = documentItemShippingId;

                                    #region 'Logic to decide whether New ShipToLoc to be created if ShiptoLoc doesn't exists'
                                    int shiptolocationId = 0;
                                    if (isLiShipToValid && shippingDetail.ShiptoLocation != null && shippingDetail.ShiptoLocation.ShiptoLocationId <= 0)
                                    {
                                        if (objRequisition.DocumentLOBDetails != null && objRequisition.DocumentLOBDetails.Any() && !string.IsNullOrEmpty(objRequisition.DocumentLOBDetails.FirstOrDefault().EntityCode))
                                        {
                                            shippingDetail.ShiptoLocation.LOBEntityCode = objRequisition.DocumentLOBDetails.FirstOrDefault().EntityCode;
                                        }
                                        if (objRequisition.DocumentAdditionalEntitiesInfoList != null && objRequisition.DocumentAdditionalEntitiesInfoList.Any())
                                        {
                                            var _lstOrgEntity = from listOrgEntityCodes in objRequisition.DocumentAdditionalEntitiesInfoList.Where(_enityId => _enityId.EntityId == EntityMappedToShipToLocation)
                                                                select listOrgEntityCodes;

                                            if (_lstOrgEntity != null && _lstOrgEntity.Count() > 0)
                                            {
                                                List<DocumentAdditionalEntityInfo> lstDocumentAdditionalEntityInfo = new List<DocumentAdditionalEntityInfo>();

                                                lstDocumentAdditionalEntityInfo = _lstOrgEntity.Cast<DocumentAdditionalEntityInfo>().ToList();

                                                shippingDetail.ShiptoLocation.lstOrgEntity = lstDocumentAdditionalEntityInfo.ConvertAll(x => new P2P.BusinessEntities.BZOrgEntity()
                                                {
                                                    EntityCode = x.EntityCode,
                                                    EntityType = x.EntityDisplayName,
                                                    IsDefault = false
                                                });
                                            }
                                        }
                                        shiptolocationId = objCommon.AddUpdateShipToDetail_New(shippingDetail.ShiptoLocation, ref objCommon, true);
                                        if (shiptolocationId != 0)
                                            shippingDetail.ShiptoLocation.ShiptoLocationId = shiptolocationId;
                                    }
                                    else if (!isLiShipToValid && isHeaderShipToValid)
                                        shiptolocationId = shippingDetail.ShiptoLocation.ShiptoLocationId = (objRequisition.ShiptoLocation == null ? 0 : objRequisition.ShiptoLocation.ShiptoLocationId);
                                    else
                                    {
                                        shiptolocationId = shippingDetail.ShiptoLocation.ShiptoLocationId;
                                    }
                                    shippingDetail.ShiptoLocation.ShiptoLocationId = shiptolocationId;
                                    #endregion

                                    #region Get Deliver to Location details
                                    int _delivertolocationid = 0;
                                    if (deliverToFreeText == false)
                                    {
                                        if (shippingDetail.DelivertoLocation != null && !string.IsNullOrWhiteSpace(shippingDetail.DelivertoLocation.DelivertoLocationNumber))
                                        {
                                            _delivertolocationid = shippingDetail.DelivertoLocation.DelivertoLocationId;
                                        }
                                        else if (lstDocumentItemShippingDetail.Any() && lstDocumentItemShippingDetail[0].DelivertoLocation != null)
                                        {
                                            _delivertolocationid = lstDocumentItemShippingDetail[0].DelivertoLocation.DelivertoLocationId;
                                            shippingDetail.DelivertoLocation.DeliverTo = lstDocumentItemShippingDetail[0].DelivertoLocation.DeliverTo;
                                        }
                                        if (shippingDetail != null)
                                            shippingDetail.DeliverTo = shippingDetail.DelivertoLocation.DeliverTo;
                                    }
                                    else if (!string.IsNullOrWhiteSpace(RequisitionItem.DocumentItemShippingDetails[0].DelivertoLocation.DeliverTo))
                                    {
                                        shippingDetail.DelivertoLocation.DeliverTo = RequisitionItem.DocumentItemShippingDetails[0].DelivertoLocation.DeliverTo;
                                    }
                                    else
                                    {
                                        shippingDetail.DelivertoLocation.DeliverTo = headerDeliverto;
                                    }
                                    shippingDetail.DelivertoLocation.DelivertoLocationId = _delivertolocationid;
                                    #endregion

                                    LogHelper.LogInfo(Log, "UpdateRequisitionFromInterface : SaveItemShippingDetails Method Started  : " + objRequisition.DocumentNumber);
                                    objDocBO.SaveItemShippingDetails(P2PDocumentType.Requisition,
                                                             shippingDetail.DocumentItemShippingId,
                                                            shippingDetail.DocumentItemId,
                                                            shippingDetail.ShippingMethod,
                                                            shippingDetail.ShiptoLocation.ShiptoLocationId,
                                                            shippingDetail.DelivertoLocation.DelivertoLocationId,
                                                            shippingDetail.Quantity,
                                                            newRequisitionItem.Quantity,
                                                            UserContext.ContactCode,
                                                            shippingDetail.DelivertoLocation != null ? shippingDetail.DelivertoLocation.DeliverTo : string.Empty,
                                                            precessionValue,
                                                            maxPrecessionforTotal,
                                                            maxPrecessionForTaxesAndCharges,
                                                            useTaxMaster
                                                            );


                                }
                            }
                            else if (isHeaderShipToValid && (objRequisition.ShiptoLocation == null ? 0 : objRequisition.ShiptoLocation.ShiptoLocationId) > 0)
                            {
                                #region IF Shipping and Deliver Missing at Line Level 
                                newRequisitionItem.DocumentItemShippingDetails = new List<DocumentItemShippingDetail>()
                            {
                                new DocumentItemShippingDetail()
                                {
                                    ShiptoLocation = new ShiptoLocation()
                                        {
                                            ShiptoLocationId = objRequisition.ShiptoLocation.ShiptoLocationId
                                        },
                                        DelivertoLocation = new DelivertoLocation()
                                        {
                                            DelivertoLocationId = objRequisition.DelivertoLocation != null ? objRequisition.DelivertoLocation.DelivertoLocationId : 0,
                                            DeliverTo = objRequisition.DelivertoLocation !=null ? objRequisition.DelivertoLocation.DeliverTo : string.Empty
                                        },
                                        DocumentItemShippingId  = documentItemShippingId,
                                        Quantity = RequisitionItem.Quantity
                                }
                             };
                                #endregion


                                objDocBO.SaveItemShippingDetails(P2PDocumentType.Requisition,
                                                            newRequisitionItem.DocumentItemShippingDetails != null && newRequisitionItem.DocumentItemShippingDetails.Any() ?
                                                            newRequisitionItem.DocumentItemShippingDetails[0].DocumentItemShippingId : 0,
                                                            newRequisitionItem.DocumentItemId,
                                                            "",
                                                            newRequisitionItem.DocumentItemShippingDetails[0].ShiptoLocation != null ? newRequisitionItem.DocumentItemShippingDetails[0].ShiptoLocation.ShiptoLocationId : 0,
                                                            newRequisitionItem.DocumentItemShippingDetails[0].DelivertoLocation != null ? newRequisitionItem.DocumentItemShippingDetails[0].DelivertoLocation.DelivertoLocationId : 0,
                                                            newRequisitionItem.Quantity,
                                                            RequisitionItem.Quantity,
                                                            UserContext.ContactCode,
                                                            objRequisition.DelivertoLocation != null ? objRequisition.DelivertoLocation.DeliverTo : string.Empty,
                                                            precessionValue,
                                                            maxPrecessionforTotal,
                                                            maxPrecessionForTaxesAndCharges,
                                                            useTaxMaster
                                                            );//INDEXING NOT NEEDED

                            }

                            #endregion

                            #region Save line level comment

                            objCommon.SaveCommentsFromInterface(newRequisitionItem.Comments, P2PDocumentType.Requisition, objRequisition.CreatedBy, newRequisitionItem.DocumentItemId, 2, objRequisition.DocumentNumber);

                            #endregion

                            #region Additional Details
                            if (matchedItems != null)
                            {
                                newRequisitionItem.AdditionalCharges = newRequisitionItem.AdditionalCharges <= 0 ? matchedItems.AdditionalCharges : newRequisitionItem.AdditionalCharges;
                                newRequisitionItem.ShippingCharges = newRequisitionItem.ShippingCharges <= 0 ? matchedItems.ShippingCharges : newRequisitionItem.ShippingCharges;
                            }
                            newRequisitionItem.DateNeeded = (newRequisitionItem.DateNeeded == null || (newRequisitionItem.DateNeeded != null && newRequisitionItem.DateNeeded == DateTime.MinValue)) ? DateTime.Now : newRequisitionItem.DateNeeded;
                            newRequisitionItem.DateRequested = (newRequisitionItem.DateRequested == null || (newRequisitionItem.DateRequested != null && newRequisitionItem.DateRequested == DateTime.MinValue)) ? DateTime.Now : newRequisitionItem.DateRequested;

                            GetReqDao().SaveItemAdditionDetails(newRequisitionItem, precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges);
                            #endregion

                            #region Accounting Details
                            if (matchedItems != null)
                                GetReqInterfaceDao().DeleteSplitsByItemId(newRequisitionItem.DocumentItemId, newRequisitionItem.DocumentId);
                            List<DocumentSplitItemEntity> lstDocumentSplitItemEntities = new List<DocumentSplitItemEntity>();
                            if (newRequisitionItem.ItemSplitsDetail != null && newRequisitionItem.ItemSplitsDetail.Count > 0)
                            {
                                foreach (RequisitionSplitItems itmSplt in newRequisitionItem.ItemSplitsDetail)
                                    lstDocumentSplitItemEntities.AddRange(itmSplt.DocumentSplitItemEntities);
                                newRequisitionItem.ItemSplitsDetail.ForEach(itm => itm.DocumentItemId = newRequisitionItem.DocumentItemId);

                                List<DocumentSplitItemEntity> DocumentSplitItemEntities = new List<DocumentSplitItemEntity>();
                                string isEnableADRRulesSetting = objCommon.GetSettingsValueByKey(p2pSettings, "IsEnableADRRules");
                                bool isEnableADRRules = string.IsNullOrEmpty(isEnableADRRulesSetting) ? false : Convert.ToBoolean(isEnableADRRulesSetting);
                                string enableADRForRequisitionSetting = objCommon.GetSettingsValueByKey(interfaceSettings, "EnableADRForRequisition");
                                bool enableADRForRequisition = string.IsNullOrEmpty(enableADRForRequisitionSetting) ? false : Convert.ToBoolean(enableADRForRequisitionSetting);
                                List<ADRSplit> adrDocumentSplitItemEntities;

                                LogHelper.LogInfo(Log, "UpdateRequisitionFromInterface : SaveRequisitionAccountingDetails Method Started  : " + objRequisition.DocumentNumber);
                                reqManager.SaveRequisitionAccountingDetails(newRequisitionItem.ItemSplitsDetail, lstDocumentSplitItemEntities, newRequisitionItem.Quantity, precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges, useTaxMaster);

                                if (isEnableADRRules && enableADRForRequisition)
                                {
                                    var splits = reqManager.GetRequisitionAccountingDetailsByItemId(newRequisitionItem.DocumentItemId, 1, 1, (int)newRequisitionItem.ItemType, objRequisition.EntityDetailCode != null && objRequisition.EntityDetailCode.Count > 0 ? objRequisition.EntityDetailCode.FirstOrDefault() : 0);
                                    var splitConfig = objDocBO.GetAllSplitAccountingFields(P2PDocumentType.Requisition, LevelType.ItemLevel, 0, lobEntityDetailCode);
                                    List<ADRSplit> lstAdrSplit = new List<ADRSplit>();

                                    if (splits != null && splits.Any() && splitConfig != null && splitConfig.Any())
                                    {
                                        List<SplitAccountingFields> splitAcc = new List<SplitAccountingFields>();
                                        foreach (var s in splits.FirstOrDefault().DocumentSplitItemEntities)
                                        {
                                            SplitAccountingFields splitAccountingField = new SplitAccountingFields(splitConfig.Where(k => k.EntityTypeId == s.EntityTypeId).FirstOrDefault());

                                            splitAccountingField.EntityCode = s.EntityCode;
                                            splitAccountingField.EntityDetailCode = !string.IsNullOrEmpty(s.SplitAccountingFieldValue) ? Convert.ToInt64(s.SplitAccountingFieldValue) : 0;
                                            splitAcc.Add(splitAccountingField);
                                        }

                                        ADRSplit adrAplit = new ADRSplit();
                                        adrAplit.Splits = splitAcc;
                                        adrAplit.Identifier = newRequisitionItem.DocumentItemId;
                                        lstAdrSplit.Add(adrAplit);
                                    }

                                    if (splits != null && splits.Any() && lstAdrSplit.Any())
                                    {
                                        foreach (var split in splits)
                                        {
                                            bool isSplitsUpdated = false;
                                            adrDocumentSplitItemEntities = objDocBO.GetDocumentDefaultAccountingDetailsForLineItems(P2PDocumentType.Requisition, objRequisition.CreatedBy,
                                                objRequisition.DocumentCode, null,
                                                lstAdrSplit, false, null, objRequisition.EntityDetailCode != null && objRequisition.EntityDetailCode.Count > 0 ? objRequisition.EntityDetailCode.FirstOrDefault() : 0, ADRIdentifier.DocumentItemId);
                                            if (adrDocumentSplitItemEntities != null && adrDocumentSplitItemEntities.Any() &&
                                                adrDocumentSplitItemEntities.FirstOrDefault().Splits != null && adrDocumentSplitItemEntities.FirstOrDefault().Splits.Any())
                                            {
                                                foreach (var documentSplitItemEntity in adrDocumentSplitItemEntities.FirstOrDefault().Splits)
                                                {
                                                    if (!string.IsNullOrEmpty(documentSplitItemEntity.EntityCode) && documentSplitItemEntity.EntityDetailCode > 0 && documentSplitItemEntity.Title != "Requester")
                                                    {
                                                        isSplitsUpdated = true;
                                                        var matchingEntities = split.DocumentSplitItemEntities.Where(entity => entity.EntityTypeId == documentSplitItemEntity.EntityTypeId);
                                                        if (matchingEntities != null && matchingEntities.Any())
                                                        {
                                                            matchingEntities.FirstOrDefault().EntityCode = documentSplitItemEntity.EntityCode;
                                                            matchingEntities.FirstOrDefault().SplitAccountingFieldValue = Convert.ToString(documentSplitItemEntity.EntityDetailCode);
                                                            matchingEntities.FirstOrDefault().DocumentSplitItemId = split.DocumentSplitItemId;
                                                        }
                                                    }
                                                }
                                            }
                                            if (isSplitsUpdated)
                                                reqManager.SaveRequisitionAccountingDetails(splits, split.DocumentSplitItemEntities, newRequisitionItem.Quantity, precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges, useTaxMaster);
                                        }
                                    }
                                }



                            }
                            #endregion

                            #region Item Other Details
                            objDocBO.SaveItemOtherDetails(P2PDocumentType.Requisition, newRequisitionItem, allowTaxCodewithAmount, supplierStatusForValidation);

                            #endregion

                            #region Save Default Split Accounting
                            if (setDefaultSplit)
                                GetReqDao().SaveDefaultAccountingDetails(objRequisition.DocumentCode, defaultAccDetails, precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges, true);
                            #endregion

                            #region Save LineItem Level Sub line Order

                            List<ItemCharge> templstLineLevelCharges = new List<ItemCharge>();
                            templstLineLevelCharges = objCommon.GetItemChargesByDocumentCode((int)P2PDocumentType.Requisition, newRequisitionItem.DocumentId, newRequisitionItem.DocumentItemId);
                            GetReqInterfaceDao().DeleteChargeAndSplitsItemsByItemChargeId(templstLineLevelCharges, newRequisitionItem.DocumentId); // For LineLevel Charge Items

                            if (newRequisitionItem.lstLineItemCharges != null && newRequisitionItem.lstLineItemCharges.Any())
                            {
                                #region Change Requsition Contains Sublines
                                List<ChargeMaster> lstChargeMaster = new List<ChargeMaster>();
                                List<ItemCharge> lstlinelevelItemCharge = new List<ItemCharge>();
                                lstChargeMaster = objCommon.GetAllChargeName(lobEntityDetailCode);

                                if (lstChargeMaster != null && lstChargeMaster.Any())
                                {
                                    foreach (var _item in newRequisitionItem.lstLineItemCharges)
                                    {
                                        if (_item.ChargeDetails != null)
                                        {
                                            /*Filter ChangeName and Assign the values : ChargeMasterID and ChargeName*/
                                            var _lstChargeMaster = lstChargeMaster.Where(_ChargeName => _ChargeName.ChargeName.Equals(_item.ChargeDetails.ChargeName, StringComparison.InvariantCultureIgnoreCase));
                                            if (_lstChargeMaster != null && _lstChargeMaster.Count() == 1)
                                            {
                                                _item.ChargeDetails.ChargeMasterId = _lstChargeMaster.First().ChargeMasterId;
                                                _item.ChargeDetails.ChargeName = _lstChargeMaster.First().ChargeName;

                                                /*Get ChargeMasterBased on ChargeMasterId*/
                                                var objChargeMaster = objCommon.GetChargeMasterDetailsByChargeMasterId(_item.ChargeDetails.ChargeMasterId);
                                                if (objChargeMaster != null)
                                                {
                                                    _item.ChargeDetails.ChargeMasterId = objChargeMaster.ChargeMasterId;
                                                    _item.ChargeDetails.ChargeName = objChargeMaster.ChargeName;
                                                    _item.ChargeDetails.ChargeDescription = objChargeMaster.ChargeDescription;
                                                    _item.ChargeDetails.ChargeTypeCode = objChargeMaster.ChargeTypeCode;
                                                    _item.ChargeDetails.CalculationBasisId = objChargeMaster.CalculationBasisId;
                                                    _item.ChargeDetails.TolerancePercentage = objChargeMaster.TolerancePercentage;
                                                    _item.ChargeDetails.ChargeTypeName = objChargeMaster.ChargeTypeName;

                                                    _item.DocumentCode = newRequisitionItem.DocumentId;//Change RequisitionID , 
                                                    _item.P2PLineItemID = newRequisitionItem.P2PLineItemId;// P2PLineItemID
                                                    _item.ItemTypeID = (int)ItemType.Charge;
                                                    _item.CalculationValue = (decimal)0;//                        
                                                    _item.IsHeaderLevelCharge = false;/* Since It's a LineItem Level*/
                                                    _item.CreatedBy = objRequisition.CreatedBy;
                                                    _item.DocumentItemId = newRequisitionItem.DocumentItemId;


                                                }
                                                lstlinelevelItemCharge.Add(_item);
                                            }
                                        }
                                    }
                                    objCommon.SaveReqAllItemCharges(lstlinelevelItemCharge); //Save All Requisition LineLevel Charge Data
                                }
                                #endregion
                            }

                            #endregion

                            #region save Additional Field Details

                            SaveAdditionalAttributeFieldFromInterface(objRequisition.DocumentCode, objRequisition.PurchaseTypeDescription, newRequisitionItem);

                            #endregion

                        }
                    }
                }
                #endregion

                #region Save or Update Line Item Tax

                GetReqDao().InsertUpdateLineitemTaxes(objRequisition);

                #endregion

                //if (CallGetLineItemsAPIOnRequisition == 1)
                GetReqInterfaceDao().SaveRequisitionItemAdditionalDetailsFromInterface(objRequisition.RequisitionItems,SpendControlType);

                #region Saving Catalog Item Accounting Details
                var catalogSettings = objCommon.GetSettingsFromSettingsComponent(P2PDocumentType.Catalog, UserContext.ContactCode, (int)SubAppCodes.P2P);
                settingValue = objCommon.GetSettingsValueByKey(catalogSettings, "AllowOrgEntityInCatalogItems");
                var allowOrgEntityFromCatalog = string.IsNullOrEmpty(settingValue) ? string.Empty : Convert.ToBoolean(settingValue).ToString().ToLower();

                settingValue = objCommon.GetSettingsValueByKey(catalogSettings, "CorporationEntityId");
                var corporationEntityId = string.IsNullOrEmpty(settingValue) ? 0 : Convert.ToInt32(settingValue);

                settingValue = objCommon.GetSettingsValueByKey(catalogSettings, "ExpenseCodeEntityId");
                var expenseCodeEntityId = string.IsNullOrEmpty(settingValue) ? 0 : Convert.ToInt32(settingValue);
                if (allowOrgEntityFromCatalog == "true" && allowOverridingOrgEntityFromCatalog)
                    GetReqDao().UpdateCatalogOrgEntitiesToRequisition(objRequisition.DocumentCode, corporationEntityId, expenseCodeEntityId);
                #endregion

                #region Save Header Level Sub line Requisition

                if (objRequisition.lstItemCharge != null && objRequisition.lstItemCharge.Count() > 0)
                {
                    #region With Subline in CXML
                    List<ChargeMaster> lstChargeMaster = new List<ChargeMaster>();
                    lstChargeMaster = objCommon.GetAllChargeName(lobEntityDetailCode);
                    List<ItemCharge> lstHeaderlevelItemCharge = new List<ItemCharge>();
                    if (lstChargeMaster != null && lstChargeMaster.Any())
                    {
                        foreach (var item in objRequisition.lstItemCharge)
                        {
                            if (item.ChargeDetails != null)
                            {
                                /*Filter ChangeName and Assign the values : ChargeMasterID and ChargeName*/
                                var _lstChargeMaster = lstChargeMaster.Where(_ChargeName => _ChargeName.ChargeName.Equals(item.ChargeDetails.ChargeName, StringComparison.InvariantCultureIgnoreCase));
                                if (_lstChargeMaster != null && _lstChargeMaster.Count() == 1)
                                {
                                    item.ChargeDetails.ChargeMasterId = _lstChargeMaster.First().ChargeMasterId;
                                    item.ChargeDetails.ChargeName = _lstChargeMaster.First().ChargeName;

                                    /*Get ChargeMasterBased on ChargeMasterId*/
                                    var objChargeMaster = objCommon.GetChargeMasterDetailsByChargeMasterId(item.ChargeDetails.ChargeMasterId);
                                    if (objChargeMaster != null)
                                    {
                                        item.ChargeDetails.ChargeMasterId = objChargeMaster.ChargeMasterId;
                                        item.ChargeDetails.ChargeName = objChargeMaster.ChargeName;
                                        item.ChargeDetails.ChargeDescription = objChargeMaster.ChargeDescription;
                                        item.ChargeDetails.ChargeTypeCode = objChargeMaster.ChargeTypeCode;
                                        item.ChargeDetails.CalculationBasisId = objChargeMaster.CalculationBasisId;
                                        item.ChargeDetails.TolerancePercentage = objChargeMaster.TolerancePercentage;
                                        item.ChargeDetails.ChargeTypeName = objChargeMaster.ChargeTypeName;

                                        item.DocumentCode = objRequisition.DocumentCode;//Change requisition
                                        item.ItemTypeID = (int)ItemType.Charge;
                                        item.CalculationValue = 0;//For Header Sub line Charges                        
                                        item.IsHeaderLevelCharge = true;/* Since It's a Header Level*/
                                        item.CreatedBy = objRequisition.CreatedBy;
                                    }
                                    lstHeaderlevelItemCharge.Add(item);
                                }
                            }
                        }
                        objCommon.SaveReqAllItemCharges(lstHeaderlevelItemCharge); //Save All Requisition Header Charge Data
                    }
                    #endregion
                }
                #endregion

                #region Prorate Tax
                if (((Convert.ToDecimal(objRequisition.Tax) > 0 && !useTaxMaster)
                    || Convert.ToDecimal(objRequisition.Shipping) > 0
                    || Convert.ToDecimal(objRequisition.AdditionalCharges) > 0))
                    reqManager.ProrateHeaderTaxAndShipping(objRequisition);
                #endregion

                #region Custom Attribute Derivation if setting (IsDeriveItemDetailEnable) is ON
                if (IsDeriveItemDetailEnable == true)
                {
                    List<string> lstCustomAttributeQuestionText = new List<string>();

                    foreach (var item in objRequisition.RequisitionItems)
                    {
                        if (item.CustomAttributes == null)
                        {
                            if (objdt.Rows != null && objdt.Rows.Count > 0)
                            {
                                foreach (DataRow dt in objdt.Rows)
                                {
                                    if (dt.Table.Columns.Contains("ItemNumber") && !string.IsNullOrEmpty(dt["ItemNumber"].ToString()))
                                    {
                                        if (item.ItemNumber == dt["ItemNumber"].ToString())
                                        {
                                            if (dt.Table.Columns.Contains("CatalogItemID") && dt.Table.Columns.Contains("ItemMasterItemId") && dt.Table.Columns.Contains("tardocumentType"))
                                            {
                                                if (!string.IsNullOrEmpty(dt["CatalogItemID"].ToString()) && !string.IsNullOrEmpty(dt["ItemMasterItemId"].ToString()) && !string.IsNullOrEmpty(dt["tardocumentType"].ToString()))
                                                    objDocBO.SaveFlippedQuestionResponses(Convert.ToInt64(dt["CatalogItemID"].ToString()), Convert.ToInt64(dt["ItemMasterItemId"].ToString()), item.DocumentItemId, Convert.ToInt32(dt["tardocumentType"].ToString()), lstCustomAttributeQuestionText);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        else if (item.CustomAttributes != null)
                        {
                            foreach (Questionnaire ques in item.CustomAttributes)
                            {
                                lstCustomAttributeQuestionText.Add(ques.QuestionnaireTitle);
                            }
                            if (objdt.Rows != null && objdt.Rows.Count > 0)
                            {
                                foreach (DataRow dt in objdt.Rows)
                                {
                                    if (dt.Table.Columns.Contains("ItemNumber") && !string.IsNullOrEmpty(dt["ItemNumber"].ToString()))
                                    {
                                        if (item.ItemNumber == dt["ItemNumber"].ToString())
                                        {
                                            if (dt.Table.Columns.Contains("CatalogItemID") && dt.Table.Columns.Contains("ItemMasterItemId") && dt.Table.Columns.Contains("tardocumentType"))
                                            {
                                                if (!string.IsNullOrEmpty(dt["CatalogItemID"].ToString()) && !string.IsNullOrEmpty(dt["ItemMasterItemId"].ToString()) && !string.IsNullOrEmpty(dt["tardocumentType"].ToString()))
                                                    objDocBO.SaveFlippedQuestionResponses(Convert.ToInt64(dt["CatalogItemID"].ToString()), Convert.ToInt64(dt["ItemMasterItemId"].ToString()), item.DocumentItemId, Convert.ToInt32(dt["tardocumentType"].ToString()), lstCustomAttributeQuestionText);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion

                #region Custom Attributes
                if (objRequisition.DocumentCode > 0)
                {

                    lstCustomAttrFormId = objCommon.GetCustomAttrFormId((int)DocumentType.Requisition, new List<Level> { Level.Header, Level.Item, Level.Distribution }, 0, 0, 0, 0, objRequisition.DocumentCode);

                    var splitItemIdDetails = GetCommonDao().GetSplitItemIds(objRequisition.DocumentCode, (int)DocumentType.Requisition);

                    if (lstCustomAttrFormId.Count > 0 && lstCustomAttrFormId.Any())
                    {
                        objRequisition.CustomAttrFormId = lstCustomAttrFormId.Where(custAttr => custAttr.Key == Level.Header).Select(custAttr => custAttr.Value).FirstOrDefault<long>();
                        objRequisition.CustomAttrFormIdForItem = lstCustomAttrFormId.Where(custAttr => custAttr.Key == Level.Item).Select(custAttr => custAttr.Value).FirstOrDefault<long>();
                        objRequisition.CustomAttrFormIdForSplit = lstCustomAttrFormId.Where(custAttr => custAttr.Key == Level.Distribution).Select(custAttr => custAttr.Value).FirstOrDefault<long>();

                        // Custom Attributes -- Get Question Set for Header, Item and Split
                        if (objRequisition.CustomAttrFormId > 0)
                            headerQuestionSet = GetCommonDao().GetQuestionSetByFormCode(objRequisition.CustomAttrFormId);

                        if (objRequisition.CustomAttrFormIdForItem > 0)
                            itemQuestionSet = GetCommonDao().GetQuestionSetByFormCode(objRequisition.CustomAttrFormIdForItem);

                        if (objRequisition.CustomAttrFormIdForSplit > 0)
                            splitQuestionSet = GetCommonDao().GetQuestionSetByFormCode(objRequisition.CustomAttrFormIdForSplit);


                        // Custom Attributes -- Fill Questions Response List -- for Header
                        if (headerQuestionSet != null && headerQuestionSet.Any() && objRequisition.CustomAttributes != null && objRequisition.CustomAttributes.Any())
                            objCommon.FillQuestionsResponseList(lstQuestionsResponse, objRequisition.CustomAttributes, headerQuestionSet.Select(question => question.QuestionSetCode).ToList(), objRequisition.DocumentCode);

                        //  Custom Attributes -- Fill Questions Response List -- for Item
                        if (objRequisition.RequisitionItems != null && objRequisition.RequisitionItems.Count > 0 && itemQuestionSet != null && itemQuestionSet.Any())
                        {
                            foreach (RequisitionItem item in objRequisition.RequisitionItems)
                            {
                                if (item.CustomAttributes != null && item.CustomAttributes.Any())
                                    objCommon.FillQuestionsResponseList(lstQuestionsResponse, item.CustomAttributes, itemQuestionSet.Select(question => question.QuestionSetCode).ToList(), item.DocumentItemId);

                                //  Custom Attributes -- Fill Questions Response List -- For Split
                                var splitItemIdList = splitItemIdDetails.Where(splitItem => splitItem.Value == item.DocumentItemId).Select(splitItem => splitItem.Key).ToList<long>();

                                if (item.ItemSplitsDetail != null && item.ItemSplitsDetail.Count > 0 && splitItemIdList != null && splitItemIdList.Any())
                                {
                                    int splitCount = 0;

                                    foreach (RequisitionSplitItems itmSplt in item.ItemSplitsDetail)
                                    {
                                        if (splitCount < splitItemIdList.Count)
                                            itmSplt.DocumentSplitItemId = splitItemIdList[splitCount++];
                                        else
                                            break;

                                        if (itmSplt.CustomAttributes != null)
                                            objCommon.FillQuestionsResponseList(lstQuestionsResponse, itmSplt.CustomAttributes, splitQuestionSet.Select(question => question.QuestionSetCode).ToList(), itmSplt.DocumentSplitItemId);
                                    }
                                }
                            }

                        }
                        // Custom Attributes -- Save Questions Response List -- For Header, Itemm and Split
                        if (lstQuestionsResponse != null && lstQuestionsResponse.Any())
                        {
                            var requestHeaders = new GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.RequestHeaders();
                            requestHeaders.Set(this.UserContext, this.JWTToken);
                            string appName = Environment.GetEnvironmentVariable("NewRelic.AppName") ?? "Requisition"; 
                            string useCase = "RequisitionInterfaceManager-UpdateRequistionDetailsFromInterface";
                            var serviceURL = MultiRegionConfig.GetConfig(CloudConfig.APIMBaseURL) + GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.ServiceURLs.QuestionBankServiceURL + "savequestionresponselist";

                            var saveQuestionResponseListRequest = new Dictionary<string, object>
                            {
                                {"QuestionResponseList", lstQuestionsResponse}
                            };

                            var webAPI = new GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.WebAPI(requestHeaders, appName, useCase);
                            webAPI.ExecutePost(serviceURL, saveQuestionResponseListRequest);
                        }
                    }

                }
                #endregion

                GetReqDao().CalculateAndUpdateSplitDetails(objRequisition.DocumentCode);//INDEXING NOT NEEDED

                GetReqInterfaceDao().SaveRequisitionAdditionalDetailsFromInterface(objRequisition.DocumentCode);

                #region Update Document Status

                /* Reset to CXML Status */
                objRequisition.DocumentStatusInfo = (Documents.Entities.DocumentStatus)_tempDocumentStatus;

                if (objRequisition.DocumentStatusInfo == Documents.Entities.DocumentStatus.Withdrawn)
                {
                    #region WithDraw Document
                    ProxyWorkFlowRestService objProxyWorkFlowRestService = new ProxyWorkFlowRestService(this.UserContext, this.JWTToken);
                    objProxyWorkFlowRestService.WithDrawDocumentByDocumentCode((int)DocumentType.Requisition, objRequisition.DocumentCode, objRequisition.CreatedBy);
                    #endregion
                }
                else if (objRequisition.DocumentStatusInfo == Documents.Entities.DocumentStatus.ApprovalPending)
                {
                    ReqTotal_ = ReqTotal_ == null ? 0 : ReqTotal_;


                    if (IsDeltaAmount == 1 && ReqTotal_ > 0)
                        ReqTotal_ = (ReqTotal_) - (objRequisition.TotalAmount);

                    reqManager.SentRequisitionForApproval(objRequisition.CreatedBy, objRequisition.DocumentCode, (decimal)ReqTotal_, (int)P2PDocumentType.Requisition, objRequisition.Currency, string.Empty, isOperationalBudgetEnabled, headerOrgEntityDetailCode);
                }
                else if (objRequisition.DocumentStatusInfo == Documents.Entities.DocumentStatus.Approved)
                {
                    objDocBO.UpdateDocumentStatus(P2PDocumentType.Requisition, objRequisition.DocumentCode, Documents.Entities.DocumentStatus.Approved, 0, false, POTransmissionMode.None, "", "REQ", objRequisition.SourceSystemInfo.SourceSystemId);

                    #region Consolidating designated approved REQs into one or multiple purchase order based on key entities(Purchase Type)
                    reqManager.PushingDataToEventHub(objRequisition.DocumentCode);
                    #endregion Consolidating designated approved REQs into one or multiple purchase order based on key entities(Purchase Type)

                    #region 'Auto creating  Order'                  
                    List<long> lstDocumentCode = new List<long>();
                    var requestHeaders = new GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.RequestHeaders();
                    requestHeaders.Set(this.UserContext, this.JWTToken);
                    string appName = Environment.GetEnvironmentVariable("NewRelic.AppName") ?? "Requisition"; 
                    string useCase = "RequisitionInterfaceManager-UpdateRequistionDetailsFromInterface";
                    var serviceURL = MultiRegionConfig.GetConfig(CloudConfig.APIMBaseURL) + GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.ServiceURLs.OrderServiceURL + "GetSettingsAndAutoCreateOrder?preDocumentId="+ objRequisition.DocumentCode;
                    var webAPI = new GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.WebAPI(requestHeaders, appName, useCase);
                    var apiResult = webAPI.ExecuteGet(serviceURL);
                    var apiresponse = JsonConvert.DeserializeObject<List<long>>(apiResult);
                    if (apiresponse != null && apiresponse.Any())
                    {
                        lstDocumentCode = apiresponse;
                    }
                    //lstDocumentCode = objproxyOrder.GetSettingsAndAutoCreateOrder(objRequisition.DocumentCode);
                    if (lstDocumentCode != null && lstDocumentCode.Any())
                    {
                        AddIntoSearchIndexerQueueing(lstDocumentCode, (int)DocumentType.PO);
                    }
                    #endregion 'Auto creating  Order'               
                }
                else if (objRequisition.DocumentStatusInfo == Documents.Entities.DocumentStatus.ReviewPending)
                {
                    objDocBO.UpdateDocumentStatus(P2PDocumentType.Requisition, objRequisition.DocumentCode, Documents.Entities.DocumentStatus.ReviewPending, 0, false, POTransmissionMode.None, "", "REQ", objRequisition.SourceSystemInfo.SourceSystemId);

                    Dictionary<string, string> resultObj = new Dictionary<string, string>();
                    resultObj = objDocBO.SendDocumentForReview(objRequisition.DocumentCode, objRequisition.RequesterId, (int)DocumentType.Requisition);

                    if (resultObj != null && resultObj.ContainsKey("SendForReviewResult") && resultObj["SendForReviewResult"] == "NoReviewSetup")
                    {
                        reqManager.SentRequisitionForApproval(objRequisition.CreatedBy, objRequisition.DocumentCode, (decimal)ReqTotal_, (int)P2PDocumentType.Requisition, objRequisition.Currency, string.Empty, isOperationalBudgetEnabled, headerOrgEntityDetailCode);
                    }
                }
                #endregion

                dtResult.Dispose();

                LogHelper.LogInfo(Log, "UpdateRequisitionFromInterface Method Ended.");

                AddIntoSearchIndexerQueueing(objRequisition.DocumentCode, (int)DocumentType.Requisition);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured while UpdateRequistionDetailsFromInterface DocumentNumber " + obj.DocumentNumber, ex);
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw();
                throw;
            }
            return objRequisition.DocumentCode;
        }

        private void SaveAdditionalAttributeFieldFromInterface(long documentCode, string purchaseType, RequisitionItem requisitionItem)
        {
            string settingValue = string.Empty;
            bool ShowAdditionalAttributes = false;

            RequisitionCommonManager objCommon = new RequisitionCommonManager (this.JWTToken) { UserContext = this.UserContext, GepConfiguration = this.GepConfiguration };

            settingValue = objCommon.GetSettingsValueByKey(P2PDocumentType.None, "ShowAdditionalAttributes", UserContext.ContactCode, (int)SubAppCodes.P2P);
            ShowAdditionalAttributes = !string.IsNullOrEmpty(settingValue) ? Convert.ToBoolean(settingValue) : false;

            if (ShowAdditionalAttributes && requisitionItem.lstAdditionalFieldAttributues != null && requisitionItem.lstAdditionalFieldAttributues.Count > 0)
                GetReqInterfaceDao().SaveAdditionalFieldAttributes(documentCode, requisitionItem.DocumentItemId, requisitionItem.lstAdditionalFieldAttributues, purchaseType);
        }

        private void SaveHeaderAdditionalAttributeFieldFromInterface(long documentCode, string purchaseType, List<GEP.Cumulus.P2P.BusinessEntities.P2PAdditionalFieldAtrribute> p2PAdditionalFieldAtrributes)
        {
            string settingValue = string.Empty;
            bool ShowAdditionalAttributes = false;

            RequisitionCommonManager objCommon = new RequisitionCommonManager(this.JWTToken) { UserContext = this.UserContext, GepConfiguration = this.GepConfiguration };

            settingValue = objCommon.GetSettingsValueByKey(P2PDocumentType.None, "ShowAdditionalAttributes", UserContext.ContactCode, (int)SubAppCodes.P2P);
            ShowAdditionalAttributes = !string.IsNullOrEmpty(settingValue) ? Convert.ToBoolean(settingValue) : false;

            if (ShowAdditionalAttributes && p2PAdditionalFieldAtrributes != null && p2PAdditionalFieldAtrributes.Count > 0)
                GetReqInterfaceDao().SaveHeaderAdditionalFieldAttributes(documentCode, p2PAdditionalFieldAtrributes, purchaseType);
        }

        public long SaveChangeRequisitionRequestFromInterface(RequisitionSource requisitionSource, long oldRequisitionId, string documentName, string documentNumber, DocumentSourceType documentSourceType, string revisionNumber, bool isCreatedFromInterface)
        {
            #region Variables
            long newRequisitionId = 0;
            int PrecessionValue, Precessiontotal, MaxPrecessionValueForTaxAndCharges;
            var commonManager = new RequisitionCommonManager (this.JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };

            var settingDetails = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.None, UserContext.ContactCode, (int)SubAppCodes.P2P);
            PrecessionValue = Convert.ToInt16(commonManager.GetSettingsValueByKey(settingDetails, "MaxPrecessionValue"));
            Precessiontotal = Convert.ToInt16(commonManager.GetSettingsValueByKey(settingDetails, "MaxPrecessionValueforTotal"));
            MaxPrecessionValueForTaxAndCharges = Convert.ToInt16(commonManager.GetSettingsValueByKey(settingDetails, "MaxPrecessionValueForTaxesAndCharges"));
            #endregion Variables

            try
            {
                if (oldRequisitionId > 0)
                {
                    #region Set Revision and DocumentNumber
                    string ReqRevisionNumber = GetReqDao().GetRequisitionRevisionNumberByDocumentCode(oldRequisitionId);
                    string currentRevNo = ReqRevisionNumber == "" ? "000" : ReqRevisionNumber;

                    if (documentSourceType == DocumentSourceType.Interface && !string.IsNullOrWhiteSpace(revisionNumber))
                    {
                        revisionNumber = Convert.ToInt32(revisionNumber).ToString("000", CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        int incrementVersion = Convert.ToInt32(currentRevNo, CultureInfo.InvariantCulture) + 1;
                        revisionNumber = incrementVersion.ToString("000", CultureInfo.InvariantCulture);
                    }

                    if (currentRevNo != "000" && !string.IsNullOrEmpty(documentNumber))
                    {
                        var docNumSplit = documentNumber.Split('-');
                        if (docNumSplit.Length > 1)
                            documentNumber = string.Join("-", docNumSplit, 0, docNumSplit.Length - 1);
                    }
                    documentNumber = documentNumber + "-" + revisionNumber;
                    #endregion Set Revision and DocumentNumber

                    newRequisitionId = GetReqDao().SaveChangeRequisitionRequest((int)requisitionSource, oldRequisitionId, documentName, documentNumber, documentSourceType, revisionNumber, isCreatedFromInterface, false, PrecessionValue, Precessiontotal, MaxPrecessionValueForTaxAndCharges);

                    AddIntoSearchIndexerQueueing(oldRequisitionId, (int)DocumentType.Requisition);

                    AddIntoSearchIndexerQueueing(newRequisitionId, (int)DocumentType.Requisition);
                }

            }
            catch(Exception ex)
            {
                LogHelper.LogError(Log, "Error occured while SaveChangeRequisitionRequestFromInterface DocumentNumber " + documentNumber, ex);
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw();
                throw;
            }
            finally
            { }

            return newRequisitionId;
        }

        public long SaveP2PDocumentfromInterface(P2PDocumentType docType, P2PDocument obj, int partnerInterfaceId = 0)
        {
            ProxyPartnerService proxyPartnerService = new ProxyPartnerService(UserContext, this.JWTToken);
            long documentId = 0;
            long contactcode = 0;
            UserContext objUserContext = null;
            UserLOBMapping objUserLOBMapping = null;
            int accessControlEntityId = 0;
            try
            {
                LogHelper.LogInfo(Log, "RequisitionManager : SaveP2PDocumentfromInterface Method Started for document : " + docType.ToString() + obj.DocumentNumber);
                obj.DocumentSourceTypeInfo = DocumentSourceType.Interface;

                if (obj.SourceSystemInfo == null)
                    obj.SourceSystemInfo = new SourceSystemInfo();
                if (!ReferenceEquals(obj, null))
                {
                    RequisitionCommonManager objCommon = new RequisitionCommonManager (this.JWTToken);
                    objCommon.UserContext = this.UserContext;
                    objCommon.GepConfiguration = this.GepConfiguration;

                    string UseOldMethod = string.Empty;
                    var newpathsetting = objCommon.GetSettingsFromSettingsComponent(P2PDocumentType.Interfaces, UserContext.ContactCode, (int)SubAppCodes.Interfaces);
                    UseOldMethod = objCommon.GetSettingsValueByKey(newpathsetting, "OldMethod");
                    bool UseNewPath = !string.IsNullOrEmpty(UseOldMethod) ? Convert.ToBoolean(UseOldMethod) : true;
                    if (UseNewPath)
                    {
                        long doccode = SaveP2PDocumentfromInterface_New(docType, obj, partnerInterfaceId);
                        return doccode;
                    }

                    List<ContactORGMapping> lstBUDetails = new List<ContactORGMapping>();
                    UserExecutionContext userExecutionContext = this.UserContext;

                    if ((docType == P2PDocumentType.Order && ((Order)obj).OrderSource != OrderSource.ChangeRequest
                                                              && ((Order)obj).OrderSource != OrderSource.None) || docType == P2PDocumentType.Requisition)
                    {
                        LogHelper.LogInfo(Log, "Fetching document creator's details  : " + docType.ToString() + "   " + obj.DocumentNumber);

                        #region 'Set Contact Code'
                        // Get ContactCode based on Client User Id or Username sent through Interface

                        contactcode = objCommon.GetContactCodeByClientContactCodeOrEmail(obj.ClientContactCode, obj.CreatedByName);

                        // If Contact Code not found throw validation exception
                        if (contactcode <= 0)
                            throw new Exception("ValidationException : Invalid Client ContactCode in the document");

                        obj.CreatedBy = obj.ModifiedBy = this.UserContext.ContactCode = userExecutionContext.ContactCode = contactcode;

                        #endregion

                        #region Set LOB Details

                        var partnerHelper = new Req.BusinessObjects.RESTAPIHelper.PartnerHelper(this.UserContext, JWTToken);
                        objUserContext = partnerHelper.GetUserContextDetailsByContactCode(obj.CreatedBy);

                        if (docType == P2PDocumentType.Requisition)
                            objUserLOBMapping = objUserContext.GetDefaultBelongingUserLOBMapping() ?? new UserLOBMapping();
                        else if (docType == P2PDocumentType.Order)
                            objUserLOBMapping = objUserContext.GetDefaultServingUserLOBMapping() ?? new UserLOBMapping();

                        if (objUserLOBMapping != null)
                        {
                            accessControlEntityId = UserContext.GetAccessControlEntityId(objUserLOBMapping.EntityDetailCode);
                            obj.EntityId = objUserLOBMapping.EntityId;
                            obj.EntityDetailCode = new List<long>() { objUserLOBMapping.EntityDetailCode };
                        }
                        #endregion

                        LogHelper.LogInfo(Log, "Fetching BU details : " + docType.ToString() + obj.DocumentNumber);
                        #region'Set BU'
                        if (accessControlEntityId > 0)
                        {
                            ContactORGMapping cOrgMap = new ContactORGMapping()
                            {
                                ContactCode = userExecutionContext.ContactCode,
                                CompanyName = userExecutionContext.CompanyName,
                                ClientID = userExecutionContext.ClientID,
                                CultureCode = userExecutionContext.Culture,
                                EntityId = accessControlEntityId
                            };
                            // Get list of BU based on Contact Code
                            lstBUDetails = proxyPartnerService.GetContactORGMapping(cOrgMap).OrderByDescending(orgDtl => orgDtl.IsDefault).ToList();
                            bool LimitAccessControlEntity = false;
                            var settingDetails = objCommon.GetSettingsFromSettingsComponent(P2PDocumentType.None, UserContext.ContactCode, 107);
                            LimitAccessControlEntity = Convert.ToBoolean(objCommon.GetSettingsValueByKey(settingDetails, "LimitAccessControlEntity").ToLower());
                            if (lstBUDetails != null && lstBUDetails.Any())
                            {
                                if (LimitAccessControlEntity)
                                {
                                    var lstDefaultBU = lstBUDetails.Where(data => data.IsDefault).FirstOrDefault();
                                    DocumentBU objDocumentBU = new DocumentBU();
                                    objDocumentBU.BusinessUnitCode = lstDefaultBU.OrgEntityCode;
                                    objDocumentBU.BusinessUnitName = lstDefaultBU.EntityDescription;
                                    obj.DocumentBUList.Add(objDocumentBU);
                                }
                                else
                                {
                                    lstBUDetails.ForEach(data => { if (data.OrgEntityCode > 0) obj.DocumentBUList.Add(new DocumentBU() { BusinessUnitCode = data.OrgEntityCode, BusinessUnitName = data.EntityDescription }); });
                                }
                            }
                        }
                        #endregion
                    }

                    #region 'Save Requisition'
                    if (docType == P2PDocumentType.Requisition)
                    {
                        LogHelper.LogInfo(Log, "SaveRequisitionFromInterface Method Started : " + docType.ToString() + "   " + obj.DocumentNumber);
                        documentId = SaveRequisitionFromInterface(obj, lstBUDetails, ref objCommon, partnerInterfaceId, accessControlEntityId);//indexing in respective manager
                    }
                    #endregion 'save requisition'
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured while SaveP2PDocumentfromInterface DocumentNumber " + obj.DocumentNumber, ex);
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw();
                throw;
            }
            finally
            {
                //DisposeService(objPartnerServiceChannel);
            }
            return documentId;
        }

        public long SaveP2PDocumentfromInterface_New(P2PDocumentType docType, P2PDocument obj, int partnerInterfaceId = 0)
        {
            RequisitionManager reqManager = new RequisitionManager (this.JWTToken) { UserContext = this.UserContext, GepConfiguration = this.GepConfiguration };
            //IPartnerChannel objPartnerServiceChannel = null;
            ProxyPartnerService proxyPartnerService = new ProxyPartnerService(UserContext, this.JWTToken);
            long documentId = 0, contactcode = 0, partnerCode = 0, PartnerContactCode = 0, Locationid = 0, PartnerReconMatchTypeId = 0;
            int VATId = 0;
            UserLOBMapping objUserLOBMapping = null;
            int accessControlEntityId = 0;
            int headerEntityId;
            string headerEntityName, sourceSystemName = string.Empty;
            string settingValue = string.Empty, ClientPartnerCode = string.Empty;
            List<UserLOBMapping> userLOBMappingCollection = new List<UserLOBMapping>();
            bool isDocumentNameAsNumber = false;
            bool useDocumentLOB = false;

            RequisitionCommonManager objCommon = new RequisitionCommonManager (this.JWTToken) { UserContext = this.UserContext, GepConfiguration = this.GepConfiguration };
            var interfaceSettings = objCommon.GetSettingsFromSettingsComponent(P2PDocumentType.Interfaces, UserContext.ContactCode, (int)SubAppCodes.Interfaces);
            settingValue = objCommon.GetSettingsValueByKey(interfaceSettings, "DeriveHeaderEntities");
            bool DeriveHeaderEntities = !string.IsNullOrEmpty(settingValue) ? Convert.ToBoolean(settingValue) : false;

            settingValue = objCommon.GetSettingsValueByKey(interfaceSettings, "DerivePartnerFromLocationCode");
            bool DerivePartnerFromLocationCode = !string.IsNullOrEmpty(settingValue) ? Convert.ToBoolean(settingValue) : false;

            settingValue = objCommon.GetSettingsValueByKey(interfaceSettings, "isDocumentNameAsNumber");
            isDocumentNameAsNumber = !string.IsNullOrEmpty(settingValue) ? Convert.ToBoolean(settingValue) : false;

            settingValue = objCommon.GetSettingsValueByKey(interfaceSettings, "UseDocumentLOB");
            useDocumentLOB = !string.IsNullOrEmpty(settingValue) ? Convert.ToBoolean(settingValue) : false;

            var p2pSettings = objCommon.GetSettingsFromSettingsComponent(P2PDocumentType.None, UserContext.ContactCode, (int)SubAppCodes.P2P);
            settingValue = objCommon.GetSettingsValueByKey(p2pSettings, "PartnerStatuses");
            string PartnerStatuscodes = !string.IsNullOrEmpty(settingValue) ? Convert.ToString(settingValue) : string.Empty;

            try
            {
                LogHelper.LogInfo(Log, "SaveP2PDocumentfromInterface Method Started for document : " + docType.ToString() + obj.DocumentNumber);
                obj.DocumentSourceTypeInfo = DocumentSourceType.Interface;
                if (!ReferenceEquals(obj, null))
                {

                    objCommon.UserContext = this.UserContext;
                    objCommon.GepConfiguration = this.GepConfiguration;

                    List<ContactORGMapping> lstBUDetails = new List<ContactORGMapping>();

                    if (partnerInterfaceId == 0 && obj.SourceSystemInfo != null && !string.IsNullOrEmpty(obj.SourceSystemInfo.SourceSystemName))
                    {
                        var lstSourceSystem = objCommon.GetAllSourceSystemInfo();
                        SourceSystemInfo sourceSystem = new SourceSystemInfo();

                        if (lstSourceSystem != null && lstSourceSystem.Any())
                            sourceSystem = lstSourceSystem.Where(srcSys => obj.SourceSystemInfo.SourceSystemName.Equals(srcSys.SourceSystemName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

                        if (sourceSystem != null)
                            obj.SourceSystemInfo.SourceSystemId = sourceSystem.SourceSystemId;
                    }
                    else
                        obj.SourceSystemInfo = new SourceSystemInfo();

                    if ((docType == P2PDocumentType.Order && ((Order)obj).OrderSource != OrderSource.ChangeRequest
                                                              && ((Order)obj).OrderSource != OrderSource.None) || docType == P2PDocumentType.Requisition)
                    {
                        LogHelper.LogInfo(Log, "Fetching document creator's details  : " + docType.ToString() + "   " + obj.DocumentNumber);

                        if (docType == P2PDocumentType.Requisition && obj.Operation.Equals("delete", StringComparison.InvariantCultureIgnoreCase))
                        {
                            Requisition ExistingRequisition = null;
                            var docStatus = "";
                            docStatus = ((int)DocumentStatus.Draft) + "," + ((int)DocumentStatus.Withdrawn) + "," + ((int)DocumentStatus.ApprovalPending) + "," + ((int)DocumentStatus.Approved) + "," + ((int)DocumentStatus.Rejected);

                            ExistingRequisition = (Requisition)GetReqDao().GetDocumentDetailByDocumentNumber(obj.DocumentNumber, docStatus, "", obj.EntityDetailCode != null && obj.EntityDetailCode.Count > 0 ? obj.EntityDetailCode[0] : -1, partnerCode);

                            if ((ExistingRequisition.DocumentStatusInfo == Documents.Entities.DocumentStatus.ApprovalPending) || (ExistingRequisition.DocumentStatusInfo == Documents.Entities.DocumentStatus.Approved))
                            {
                                #region WithDraw Document
                                ProxyWorkFlowRestService objProxyWorkFlowRestService = new ProxyWorkFlowRestService(this.UserContext, this.JWTToken);
                                objProxyWorkFlowRestService.WithDrawDocumentByDocumentCode((int)DocumentType.Requisition, ExistingRequisition.DocumentCode, ExistingRequisition.CreatedBy);
                                #endregion
                            }
                            reqManager.DeleteDocumentByDocumentCode(P2PDocumentType.Requisition, ExistingRequisition.DocumentCode);
                            return ExistingRequisition.DocumentCode;

                        }

                        #region 'Set partnerCode, Contact Code,PartnerContactCode,Locationid, PartnerReconMatchTypeId, VAT'

                        sourceSystemName = obj.SourceSystemInfo != null ? obj.SourceSystemInfo.SourceSystemName : string.Empty;

                        DataTable dtPartnerDetails = new DataTable();
                        if (docType == P2PDocumentType.Requisition)
                        {
                            dtPartnerDetails = GetCommonDao().GetPartnerandContactCodeDetails(((Requisition)obj).RequisitionItems[0].ClientPartnerCode, obj.ClientContactCode, obj.CreatedByName, this.UserContext.BuyerPartnerCode, "", "", "", sourceSystemName, false, PartnerStatuscodes);
                        }
                        else
                        {
                            ClientPartnerCode = (((Order)obj).OrderItems.Count > 0) ? ((Order)obj).OrderItems[0].ClientPartnerCode : GetCommonDao().Getclientpartnercode(obj.DocumentNumber);
                            dtPartnerDetails = GetCommonDao().GetPartnerandContactCodeDetails(ClientPartnerCode, obj.ClientContactCode, obj.CreatedByName, this.UserContext.BuyerPartnerCode, ((Order)obj).PartnerContactEmailAddress, ((Order)obj).ClientLocationCode, ((Order)obj).BuyerVATNumber, sourceSystemName, DerivePartnerFromLocationCode, PartnerStatuscodes);
                        }

                        if (dtPartnerDetails != null && dtPartnerDetails.Rows != null && dtPartnerDetails.Rows.Count > 0)
                        {
                            partnerCode = Convert.ToInt64(dtPartnerDetails.Rows[0]["PartnerCode"]);
                            contactcode = Convert.ToInt64(dtPartnerDetails.Rows[0]["ContactCode"]);
                            PartnerContactCode = Convert.ToInt64(dtPartnerDetails.Rows[0]["PartnerContactCode"]);
                            Locationid = Convert.ToInt64(dtPartnerDetails.Rows[0]["LocationCode"]);
                            PartnerReconMatchTypeId = Convert.ToInt32(dtPartnerDetails.Rows[0][SqlConstants.COL_PARTNERRECONMATCHTYPEID]);
                            VATId = Convert.ToInt32(dtPartnerDetails.Rows[0][SqlConstants.COL_IDENTIFICATIONID]);
                        }


                        obj.CreatedBy = obj.ModifiedBy = this.UserContext.ContactCode = this.UserContext.ContactCode = contactcode;

                        #endregion

                        #region Set LOB Details
                        proxyPartnerService = new ProxyPartnerService(this.UserContext, this.JWTToken);
                        userLOBMappingCollection = proxyPartnerService.GetUserLOBDetailByContactCode(obj.CreatedBy);

                        List<OrganizationStructure.Entities.LOBAccessControlDetail> lobAccessControlDetail = null;

                        var requestHeaders = new GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.RequestHeaders();
                        requestHeaders.Set(this.UserContext, this.JWTToken);
                        string appName = Environment.GetEnvironmentVariable("NewRelic.AppName") ?? "Requisition"; 
                        string useCase = "RequisitionInterfaceManager-SaveP2PDocumentfromInterface_New";
                        var serviceURL = MultiRegionConfig.GetConfig(CloudConfig.APIMBaseURL) + GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.ServiceURLs.OrganizationServiceURL + "GetLOBWithAccessControlEntityDetail";

                        var webAPI = new GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.WebAPI(requestHeaders, appName, useCase);
                        var result = webAPI.ExecuteGet(serviceURL);

                        var apiresponse = JsonConvert.DeserializeObject<Dictionary<string, List<OrganizationStructure.Entities.LOBAccessControlDetail>>>(result);
                        if (apiresponse != null && apiresponse.Any())
                        {
                            lobAccessControlDetail = apiresponse.First().Value;
                        }

                        if (docType == P2PDocumentType.Requisition)
                        {
                            if (obj.DocumentLOBDetails != null && obj.DocumentLOBDetails.Any() && !string.IsNullOrEmpty(obj.DocumentLOBDetails.FirstOrDefault().EntityCode))
                            {
                                if (useDocumentLOB && lobAccessControlDetail != null && lobAccessControlDetail.Any())
                                {
                                    var lobDetail = lobAccessControlDetail.Where(entity => entity.LOBCode == obj.DocumentLOBDetails.FirstOrDefault().EntityCode);
                                    if (lobDetail != null && lobDetail.Any())
                                    {
                                        objUserLOBMapping = new UserLOBMapping() { EntityId = lobDetail.FirstOrDefault().EntityId, EntityDetailCode = lobDetail.FirstOrDefault().LOBId };
                                    }
                                }
                                else
                                {
                                    string LobEntityCode = obj.DocumentLOBDetails.FirstOrDefault().EntityCode;
                                    objUserLOBMapping = userLOBMappingCollection.Where(a => a.EntityCode == LobEntityCode && a.PreferenceLobType == PreferenceLOBType.Serve).FirstOrDefault();
                                }
                            }
                            else if (obj.DocumentAdditionalEntitiesInfoList != null && obj.DocumentAdditionalEntitiesInfoList.Any())
                            {
                                if (DeriveHeaderEntities == true)
                                {
                                    var lstEntityCodes = obj.DocumentAdditionalEntitiesInfoList.Select(entity => entity.EntityCode).ToList<string>();
                                    var dsSplitConfig = GetSQLP2PDocumentDAO().GetSplitAccountingFieldsWithDefaultValuesForInterface(DocumentType.Requisition, LevelType.HeaderLevel, 1, lstEntityCodes);
                                    SetDocumentAdditionalEntityWithDefaultEntitesFromInterface(obj, dsSplitConfig.Tables[0]);
                                }
                                else
                                {
                                    SetRequisitionAdditionalEntityFromInterface_New((Requisition)obj, out headerEntityId, out headerEntityName);
                                }

                                if (useDocumentLOB && lobAccessControlDetail != null && lobAccessControlDetail.Any())
                                {
                                    var lobDetail = lobAccessControlDetail.Where(entity => entity.LOBCode == obj.DocumentLOBDetails.FirstOrDefault().EntityCode);
                                    if (lobDetail != null && lobDetail.Any())
                                    {
                                        objUserLOBMapping = new UserLOBMapping() { EntityId = lobDetail.FirstOrDefault().EntityId, EntityDetailCode = lobDetail.FirstOrDefault().LOBId };
                                    }
                                }
                                else
                                {
                                    long lobEntityDetailCode = obj.DocumentLOBDetails.FirstOrDefault().EntityDetailCode;
                                    objUserLOBMapping = userLOBMappingCollection.Where(a => a.EntityDetailCode == lobEntityDetailCode && a.PreferenceLobType == PreferenceLOBType.Serve).FirstOrDefault();
                                }
                            }
                            else
                            {
                                var userLOBMapping = userLOBMappingCollection.FirstOrDefault(p => p.PreferenceLobType == PreferenceLOBType.Belong);
                                if (ReferenceEquals(userLOBMapping, null))
                                    objUserLOBMapping = new UserLOBMapping() { EntityId = 0, EntityDetailCode = 0 };
                                else
                                    objUserLOBMapping = userLOBMapping;
                            }
                        }
                        #endregion

                        #region Set Access control id
                        if (objUserLOBMapping != null)
                        {
                            accessControlEntityId = UserContext.GetAccessControlEntityId(objUserLOBMapping.EntityDetailCode);
                            obj.EntityId = objUserLOBMapping.EntityId;
                            obj.EntityDetailCode = new List<long>() { objUserLOBMapping.EntityDetailCode };
                        }
                        #endregion

                        #region'Set BU'
                        LogHelper.LogInfo(Log, "Fetching BU details : " + docType.ToString() + obj.DocumentNumber);
                        if (accessControlEntityId > 0)
                        {

                            lstBUDetails = objCommon.GetContactORGMapping(accessControlEntityId, this.UserContext.ContactCode);// objPartnerServiceChannel.GetContactORGMapping(cOrgMap).OrderByDescending(orgDtl => orgDtl.IsDefault).ToList();
                            bool LimitAccessControlEntity = false;
                            var settingDetails = objCommon.GetSettingsFromSettingsComponent(P2PDocumentType.None, UserContext.ContactCode, 107);
                            LimitAccessControlEntity = Convert.ToBoolean(objCommon.GetSettingsValueByKey(settingDetails, "LimitAccessControlEntity").ToLower());
                            if (lstBUDetails != null && lstBUDetails.Any())
                            {
                                if (LimitAccessControlEntity)
                                {
                                    //var lstDefaultBU = lstBUDetails.Where(data => data.IsDefault).FirstOrDefault();
                                    var lstDefaultBU = lstBUDetails.Where(data => data.IsDefault) != null && lstBUDetails.Where(data => data.IsDefault).Any() ? lstBUDetails.Where(data => data.IsDefault).FirstOrDefault()
                                             : lstBUDetails.Where(data => data.EntityId == accessControlEntityId) != null && lstBUDetails.Where(data => data.EntityId == accessControlEntityId).Any() ? lstBUDetails.Where(data => data.EntityId == accessControlEntityId).FirstOrDefault()
                                             : lstBUDetails.FirstOrDefault();
                                    DocumentBU objDocumentBU = new DocumentBU();
                                    objDocumentBU.BusinessUnitCode = lstDefaultBU.OrgEntityCode;
                                    objDocumentBU.BusinessUnitName = lstDefaultBU.EntityDescription;
                                    obj.DocumentBUList.Add(objDocumentBU);
                                }
                                else
                                {
                                    lstBUDetails.ForEach(data => { if (data.OrgEntityCode > 0) obj.DocumentBUList.Add(new DocumentBU() { BusinessUnitCode = data.OrgEntityCode, BusinessUnitName = data.EntityDescription }); });
                                }
                            }
                        }

                        #endregion

                    }

                    #region 'Save Requisition'
                    if (docType == P2PDocumentType.Requisition)
                    {

                        RequisitionManager objReqManager = new RequisitionManager(this.JWTToken);
                        objReqManager.GepConfiguration = this.GepConfiguration;
                        objReqManager.UserContext = this.UserContext;
                        Requisition ExistingRequisition = null;
                        var docStatus = "";

                        if (!obj.Operation.Equals("new", StringComparison.InvariantCultureIgnoreCase))
                        {
                            docStatus = ((int)DocumentStatus.Draft) + "," + ((int)DocumentStatus.Withdrawn) + "," + ((int)DocumentStatus.ApprovalPending) + "," + ((int)DocumentStatus.Approved) + "," + ((int)DocumentStatus.Rejected);

                            ExistingRequisition = (Requisition)GetReqDao().GetDocumentDetailByDocumentNumber(obj.DocumentNumber, docStatus, "", obj.EntityDetailCode != null ? obj.EntityDetailCode[0] : 0, partnerCode);
                            if (ReferenceEquals(ExistingRequisition, null))
                            {
                                obj.Operation = "new";
                            }
                        }
                        if (!obj.Operation.Equals("Delete", StringComparison.InvariantCultureIgnoreCase))
                        {
                            if (obj.Operation.Equals("new", StringComparison.InvariantCultureIgnoreCase))
                            {
                                LogHelper.LogInfo(Log, "SaveRequisitionFromInterface Method Started : " + docType.ToString() + "   " + obj.DocumentNumber);
                                documentId = SaveRequisitionFromInterface(obj, lstBUDetails, ref objCommon, partnerInterfaceId, accessControlEntityId);//indexing in respective manager
                            }
                            else
                            {
                                LogHelper.LogInfo(Log, "UpdateRequisitionFromInterface Method Started : " + docType.ToString() + "   " + obj.DocumentNumber);

                                if (obj.DocumentStatusInfo == Documents.Entities.DocumentStatus.Approved || obj.DocumentStatusInfo == Documents.Entities.DocumentStatus.ApprovalPending)
                                {
                                    #region WithDraw Document
                                    ProxyWorkFlowRestService objProxyWorkFlowRestService = new ProxyWorkFlowRestService(this.UserContext, this.JWTToken);
                                    objProxyWorkFlowRestService.WithDrawDocumentByDocumentCode((int)DocumentType.Requisition, ExistingRequisition.DocumentCode, obj.CreatedBy);
                                    #endregion

                                }
                                documentId = UpdateRequisitionFromInterface(obj, ExistingRequisition, lstBUDetails, ref objCommon, partnerInterfaceId, accessControlEntityId, useDocumentLOB, isDocumentNameAsNumber);//indexing in respective manager
                            }
                        }

                    }
                    #endregion 'save requisition'

                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured while SaveP2PDocumentfromInterface_New DocumentNumber " + obj.DocumentNumber, ex);
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw();
                throw;
            }
            finally
            {
                // DisposeService(objPartnerServiceChannel);
            }
            return documentId;
        }

        public void SetDocumentAdditionalEntityWithDefaultEntitesFromInterface(P2PDocument document, DataTable dtEntityDetails)
        {
            document.DocumentLOBDetails = new List<DocumentLOBDetails>();

            foreach (DataRow dr in dtEntityDetails.Rows)
            {
                var docEntityDetail = document.DocumentAdditionalEntitiesInfoList.Where(entity => entity.EntityCode == Convert.ToString(dr[SqlConstants.COL_ENTITY_CODE])).FirstOrDefault();

                if (docEntityDetail != null)
                {
                    docEntityDetail.EntityDetailCode = Convert.ToInt64(dr[SqlConstants.COL_ENTITYDETAILCODE]);
                    docEntityDetail.EntityId = Convert.ToInt32(dr[SqlConstants.COL_ENTITYID]);
                    docEntityDetail.EntityDisplayName = Convert.ToString(dr[SqlConstants.COL_ENTITYDISPLAYNAME]);

                }
                else
                {
                    document.DocumentAdditionalEntitiesInfoList.Add(new DocumentAdditionalEntityInfo
                    {
                        EntityCode = Convert.ToString(dr[SqlConstants.COL_ENTITY_CODE]),
                        EntityDetailCode = Convert.ToInt64(dr[SqlConstants.COL_ENTITYDETAILCODE]),
                        EntityId = Convert.ToInt32(dr[SqlConstants.COL_ENTITYID]),
                        EntityDisplayName = Convert.ToString(dr[SqlConstants.COL_ENTITYDISPLAYNAME])
                    });
                }

                if (!(document.DocumentLOBDetails.Where(lob => lob.EntityCode == Convert.ToString(dr[SqlConstants.COL_LOBENTITYCODE]) && lob.EntityDetailCode == Convert.ToInt64(dr[SqlConstants.COL_LOBENTITYDETAILCODE])).Any()))
                    document.DocumentLOBDetails.Add(new DocumentLOBDetails
                    {
                        EntityCode = Convert.ToString(dr[SqlConstants.COL_LOBENTITYCODE]),
                        EntityDetailCode = Convert.ToInt64(dr[SqlConstants.COL_LOBENTITYDETAILCODE])
                    });
            }
            List<DocumentAdditionalEntityInfo> emptyDeatils = document.DocumentAdditionalEntitiesInfoList.Where(a => string.IsNullOrEmpty(a.EntityCode)).ToList();
            foreach (var item in emptyDeatils)
            {
                document.DocumentAdditionalEntitiesInfoList.Remove(item);
            }

        }

        public long SaveRequisitionFromInterface(P2PDocument obj, List<ContactORGMapping> lstBUDetails, ref RequisitionCommonManager objCommon, int partnerInterfaceId, int accessControlId)
        {
            RequisitionManager reqManager = new RequisitionManager(this.JWTToken) { UserContext = this.UserContext, GepConfiguration = this.GepConfiguration };
            NewRequisitionManager reqnewManager = new NewRequisitionManager(this.JWTToken) { UserContext = this.UserContext, GepConfiguration = this.GepConfiguration };
            LogHelper.LogInfo(Log, "SaveRequisitionFromInterface Method Started.");

            #region Variables

            bool includeTaxInSplit = false, useTaxMaster = false, allowOverridingOrgEntityFromCatalog = true, setDefaultSplit = false, isHeaderShipToValid = false, isLiShipToValid = false, IsHeaderEntityAccessControlEntity = false, isOperationalBudgetEnabled = false, isDocumentNameAsNumber = false, IsDeriveItemDetailEnable = false;
            bool isIncludePriceDetails = false, IsOrderingLocationMandatory = false, IsDefaultOrderingLocation = false;
            int headerEntityId = 0, delivertolocationid = 0, noOfDaysForDateRequested = 0, splitCounters = 1, DocumentStatus = 0, IsClientCodeBasedonLinkLocation = 0, CallGetLineItemsAPIOnRequisition = 0;
            string headerEntityName = string.Empty, settingValue = string.Empty, EntityCode = string.Empty, headerDeliverto = string.Empty;
            long lobEntityDetailCode = 0, headerOrgEntityDetailCode = 0;
            StringBuilder strErrors = new StringBuilder();
            bool deriveItemAdditionalFieldsFromCatalog = false;
            bool IsCaptureCatalogApiResponse = false;

            List<OrganizationStructureService.GeneralLedger> lstGL = new List<OrganizationStructureService.GeneralLedger>();
            List<OrganizationStructureService.OrgEntity> lstOrgEntity = new List<OrganizationStructureService.OrgEntity>();
            List<string> lstGLCodes = new List<string>();
            List<string> lstOrgCodes = new List<string>();
            //List<DocumentSplitItemEntity> lstOrgEntities = null, lstGLEntityDeails = null;
            List<KeyValuePair<string, long>> lstKvpCategoryDetails = new List<KeyValuePair<string, long>>();
            List<KeyValuePair<long, long>> lstKvpCategoryUnspsc = new List<KeyValuePair<long, long>>();
            List<SplitAccountingFields> splitFieldDetails = new List<SplitAccountingFields>();

            List<DocumentSplitItemEntity> lstEntityDeails = new List<DocumentSplitItemEntity>();
            List<DocumentSplitItemEntity> defaultAccDetails = null;
            DataSet result = null;

            DataTable dtResult = new DataTable { Locale = CultureInfo.InvariantCulture };
            var dtItems = new DataTable { Locale = CultureInfo.InvariantCulture };
            dtItems.Columns.Add("BuyerItemNumber", typeof(string));
            dtItems.Columns.Add("PartnerCode", typeof(decimal));
            dtItems.Columns.Add("UOM", typeof(string));

            var objdt = new DataTable { Locale = CultureInfo.InvariantCulture };
            objdt.Columns.Add("ItemNumber", typeof(string));
            objdt.Columns.Add("CatalogItemID", typeof(long));
            objdt.Columns.Add("ItemMasterItemId", typeof(long));
            objdt.Columns.Add("tardocumentType", typeof(int));

            var dtItemsDetails = new DataTable { Locale = CultureInfo.InvariantCulture };
            dtItemsDetails.Columns.Add("ItemLineNumber", typeof(long));
            dtItemsDetails.Columns.Add("BuyerItemNumber", typeof(string));
            dtItemsDetails.Columns.Add("SupplierItemNumber", typeof(string));
            dtItemsDetails.Columns.Add("UOM", typeof(string));
            dtItemsDetails.Columns.Add("ContractNumber", typeof(string));
            dtItemsDetails.Columns.Add("PartnerCode", typeof(long));

            DataTable dtValiadtionDetails = new DataTable();

            var commonManager = new RequisitionCommonManager (this.JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
            RequisitionDocumentManager  objDocBO = new RequisitionDocumentManager(this.JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
            BZRequisition objBZRequisition = new BZRequisition();
            Requisition objRequisition = (Requisition)obj;
            objBZRequisition.Requisition = objRequisition;


            Int32 EntityMappedToBillToLocation = 0, EntityMappedToShipToLocation = 0;
            int maxPrecessionforTotal, maxPrecessionForTaxesAndCharges;

            List<KeyValuePair<Level, long>> lstCustomAttrFormId = new List<KeyValuePair<Level, long>>();
            List<QuestionBankEntities.QuestionResponse> lstQuestionsResponse = new List<QuestionBankEntities.QuestionResponse>();
            List<QuestionBankEntities.Question> headerQuestionSet = new List<QuestionBankEntities.Question>();
            List<QuestionBankEntities.Question> itemQuestionSet = new List<QuestionBankEntities.Question>();
            List<QuestionBankEntities.Question> splitQuestionSet = new List<QuestionBankEntities.Question>();
            List<KeyValuePair<string, long>> lstDeriveditemDetails = new List<KeyValuePair<string, long>>();
            #endregion
            try
            {
                #region Set LOB Details & AccessControl EntityId
                lobEntityDetailCode = objRequisition.EntityDetailCode != null ? objRequisition.EntityDetailCode.FirstOrDefault() : 0;
                #endregion

                #region Set the Setting
                var interfaceSettings = objCommon.GetSettingsFromSettingsComponent(P2PDocumentType.Interfaces, UserContext.ContactCode, (int)SubAppCodes.Interfaces);
                var p2pSettings = objCommon.GetSettingsFromSettingsComponent(P2PDocumentType.None, UserContext.ContactCode, (int)SubAppCodes.P2P, "", lobEntityDetailCode);
                var RequisitionSettings = objCommon.GetSettingsFromSettingsComponent(P2PDocumentType.Requisition, UserContext.ContactCode, (int)SubAppCodes.P2P, "", lobEntityDetailCode);
                var invoiceSettings = objCommon.GetSettingsFromSettingsComponent(P2PDocumentType.Invoice, UserContext.ContactCode, (int)SubAppCodes.P2P, "", lobEntityDetailCode);
                var _catalogSettings = objCommon.GetSettingsFromSettingsComponent(P2PDocumentType.Catalog, UserContext.ContactCode, (int)SubAppCodes.P2P, "", lobEntityDetailCode);


                includeTaxInSplit = Convert.ToBoolean(objCommon.GetSettingsValueByKey(interfaceSettings, "IncludeTaxInSplit"), NumberFormatInfo.InvariantInfo);
                useTaxMaster = Convert.ToBoolean(objCommon.GetSettingsValueByKey(interfaceSettings, "UseTaxMaster"), NumberFormatInfo.InvariantInfo);

                settingValue = objCommon.GetSettingsValueByKey(interfaceSettings, "NoOfDaysForDateRequested");
                noOfDaysForDateRequested = string.IsNullOrEmpty(settingValue) ? 0 : Convert.ToInt16(settingValue);

                settingValue = objCommon.GetSettingsValueByKey(interfaceSettings, "isDocumentNameAsNumber");
                isDocumentNameAsNumber = !string.IsNullOrEmpty(settingValue) ? Convert.ToBoolean(settingValue) : false;

                settingValue = objCommon.GetSettingsValueByKey(p2pSettings, "AllowDeliverToFreeText");
                bool deliverToFreeText = !string.IsNullOrEmpty(settingValue) ? Convert.ToBoolean(settingValue) : false;

                settingValue = objCommon.GetSettingsValueByKey(interfaceSettings, "AllowOverridingOrgEntityFromCatalog");
                allowOverridingOrgEntityFromCatalog = !string.IsNullOrEmpty(settingValue) ? Convert.ToBoolean(settingValue) : true;

                settingValue = objCommon.GetSettingsValueByKey(p2pSettings, "AllowNegativeValues");
                var allowNegativeValues = !string.IsNullOrEmpty(settingValue) ? Convert.ToInt16(settingValue) : 0;

                settingValue = objCommon.GetSettingsValueByKey(p2pSettings, "EntityMappedToBillToLocation");
                EntityMappedToBillToLocation = string.IsNullOrEmpty(settingValue) ? 0 : Convert.ToInt32(settingValue);

                settingValue = objCommon.GetSettingsValueByKey(p2pSettings, "EntityMappedToShipToLocation");
                EntityMappedToShipToLocation = string.IsNullOrEmpty(settingValue) ? 0 : Convert.ToInt32(settingValue);

                settingValue = objCommon.GetSettingsValueByKey(interfaceSettings, "DeriveHeaderEntities");
                bool DeriveHeaderEntities = !string.IsNullOrEmpty(settingValue) ? Convert.ToBoolean(settingValue) : false;

                var precessionValue = convertStringToInt(objCommon.GetSettingsValueByKey(p2pSettings, "MaxPrecessionValue"));

                maxPrecessionforTotal = convertStringToInt(objCommon.GetSettingsValueByKey(p2pSettings, "MaxPrecessionValueforTotal"));
                maxPrecessionForTaxesAndCharges = convertStringToInt(objCommon.GetSettingsValueByKey(p2pSettings, "MaxPrecessionValueForTaxesAndCharges"));

                bool allowTaxCodewithAmount = Convert.ToBoolean(objCommon.GetSettingsValueByKey(invoiceSettings, "AllowTaxCodewithAmount"));
                string supplierStatusForValidation = objCommon.GetSettingsValueByKey(invoiceSettings, "SupplierStatusForValidation");

                settingValue = objCommon.GetSettingsValueByKey(p2pSettings, "IsOperationalBudgetEnabled");
                isOperationalBudgetEnabled = string.IsNullOrEmpty(settingValue) ? false : Convert.ToBoolean(settingValue);
                settingValue = objCommon.GetSettingsValueByKey(interfaceSettings, "IsClientCodeBasedonLinkLocation");
                IsClientCodeBasedonLinkLocation = string.IsNullOrEmpty(settingValue) ? 0 : Convert.ToInt16(settingValue);
                IsHeaderEntityAccessControlEntity = SettingsHelper.GetAccessControlSettingsForDocument(UserContext, (int)DocumentType.Requisition);

                settingValue = objCommon.GetSettingsValueByKey(interfaceSettings, "DeriveItemDetails");
                IsDeriveItemDetailEnable = !string.IsNullOrEmpty(settingValue) ? Convert.ToBoolean(settingValue) : false;

                settingValue = objCommon.GetSettingsValueByKey(interfaceSettings, "UseDocumentLOB");
                bool useDocumentLOB = !string.IsNullOrEmpty(settingValue) ? Convert.ToBoolean(settingValue) : false;

                settingValue = objCommon.GetSettingsValueByKey(p2pSettings, "PartnerStatuses");
                string PartnerStatuscodes = !string.IsNullOrEmpty(settingValue) ? Convert.ToString(settingValue) : string.Empty;

                settingValue = objCommon.GetSettingsValueByKey(_catalogSettings, "IncludePriceDetails");
                isIncludePriceDetails = !string.IsNullOrEmpty(settingValue) ? Convert.ToBoolean(settingValue) : false;

                settingValue = objCommon.GetSettingsValueByKey(interfaceSettings, "CallGetLineItemsAPIOnRequisition");
                CallGetLineItemsAPIOnRequisition = string.IsNullOrEmpty(settingValue) ? 0 : Convert.ToInt16(settingValue);

                settingValue = objCommon.GetSettingsValueByKey(interfaceSettings, "DerivePartnerFromLocationCode");
                bool DerivePartnerFromLocationCode = !string.IsNullOrEmpty(settingValue) ? Convert.ToBoolean(settingValue) : false;

                settingValue = objCommon.GetSettingsValueByKey(interfaceSettings, "AllowRequisitionErrorProcessing");
                bool AllowRequisitionErrorProcessing = !string.IsNullOrEmpty(settingValue) ? Convert.ToBoolean(settingValue) : false;

                settingValue = objCommon.GetSettingsValueByKey(P2PDocumentType.Requisition, "IsOrderingLocationMandatory", UserContext.ContactCode, (int)SubAppCodes.P2P);
                IsOrderingLocationMandatory = !string.IsNullOrEmpty(settingValue) ? Convert.ToBoolean(settingValue) : false;

                settingValue = objCommon.GetSettingsValueByKey(P2PDocumentType.None, "IsDefaultOrderingLocation", UserContext.ContactCode, (int)SubAppCodes.P2P);
                IsDefaultOrderingLocation = !string.IsNullOrEmpty(settingValue) ? Convert.ToBoolean(settingValue) : false;

                settingValue = objCommon.GetSettingsValueByKey(interfaceSettings, "DeriveItemAdditionalFieldsFromCatalog");
                deriveItemAdditionalFieldsFromCatalog = !string.IsNullOrEmpty(settingValue) ? Convert.ToBoolean(settingValue) : false;

                settingValue = objCommon.GetSettingsValueByKey(interfaceSettings, "IsCaptureCatalogApiResponse");
                IsCaptureCatalogApiResponse = !string.IsNullOrEmpty(settingValue) ? Convert.ToBoolean(settingValue) : false;

                settingValue = objCommon.GetSettingsValueByKey(p2pSettings, "SpendControlType");
                var SpendControlType = string.IsNullOrEmpty(settingValue) ? 0 : Convert.ToInt16(settingValue);

                settingValue = objCommon.GetSettingsValueByKey(p2pSettings, "OrderingLocationbyHeaderEntities");
                var OrderingLocationbyHeaderEntities = !string.IsNullOrEmpty(settingValue) ? Convert.ToString(settingValue) : string.Empty;

                settingValue = objCommon.GetSettingsValueByKey(p2pSettings, "ShipFromLocationByHeaderEntity");
                var ShipFromLocationByHeaderEntity = !string.IsNullOrEmpty(settingValue) ? Convert.ToString(settingValue) : string.Empty;
                #endregion

                objRequisition.Precision = (Int16)precessionValue;

                if (objRequisition.DocumentBUList != null && objRequisition.DocumentBUList.Count > 0)
                    SetRequisitionAdditionalEntityFromInterface_New(objRequisition, out headerEntityId, out headerEntityName, objRequisition.DocumentLOBDetails.FirstOrDefault().EntityCode);



                #region "finding BU"
                //if (string.IsNullOrWhiteSpace(objRequisition.BusinessUnitName))
                if (useDocumentLOB && accessControlId > 0)
                {
                    string LobEntityCode = "";
                    if (objRequisition.DocumentLOBDetails != null && objRequisition.DocumentLOBDetails.Any() && !string.IsNullOrEmpty(objRequisition.DocumentLOBDetails.FirstOrDefault().EntityCode))
                    {
                        LobEntityCode = objRequisition.DocumentLOBDetails.FirstOrDefault().EntityCode;
                    }
                    SetRequisitionAdditionalEntityFromInterface_New(objRequisition, out headerEntityId, out headerEntityName, LobEntityCode);

                    if (objRequisition.DocumentAdditionalEntitiesInfoList != null && objRequisition.DocumentAdditionalEntitiesInfoList.Where(a => a.EntityId == accessControlId) != null
                        && objRequisition.DocumentAdditionalEntitiesInfoList.Where(a => a.EntityId == accessControlId).Any())
                    {
                        var entity = objRequisition.DocumentAdditionalEntitiesInfoList.Where(a => a.EntityId == accessControlId).FirstOrDefault();
                        objRequisition.DocumentBUList.Clear();
                        objRequisition.DocumentBUList.Add(new DocumentBU() { BusinessUnitCode = entity.EntityDetailCode, BusinessUnitName = entity.EntityDisplayName });
                        objRequisition.BusinessUnitId = entity.EntityDetailCode;
                    }
                }
                else if (objRequisition.DocumentAdditionalEntitiesInfoList != null)
                {
                    if (lstBUDetails.Where(data => data.EntityCode == objRequisition.DocumentAdditionalEntitiesInfoList.First().EntityCode) != null &&
                        lstBUDetails.Where(data => data.EntityCode == objRequisition.DocumentAdditionalEntitiesInfoList.First().EntityCode).Count() > 0)
                    {
                        objRequisition.BusinessUnitId = lstBUDetails.Where(data => data.EntityCode == objRequisition.DocumentAdditionalEntitiesInfoList.First().EntityCode).FirstOrDefault().OrgEntityCode;  // need to check           
                    }
                    else {
                        objRequisition.BusinessUnitId = lstBUDetails != null
                                                 ? lstBUDetails.Where(data => data.IsDefault) != null && lstBUDetails.Where(data => data.IsDefault).Any() ? lstBUDetails.Where(data => data.IsDefault).FirstOrDefault().OrgEntityCode
                                                 : lstBUDetails.Where(data => data.EntityId == accessControlId) != null && lstBUDetails.Where(data => data.EntityId == accessControlId).Any() ? lstBUDetails.Where(data => data.EntityId == accessControlId).FirstOrDefault().OrgEntityCode
                                                 : 0 : 0;
                    }
                }
                else
                {
                    objRequisition.BusinessUnitId = lstBUDetails != null
                                                    ? lstBUDetails.Where(data => data.IsDefault) != null && lstBUDetails.Where(data => data.IsDefault).Any() ? lstBUDetails.Where(data => data.IsDefault).FirstOrDefault().OrgEntityCode
                                                    : lstBUDetails.Where(data => data.EntityId == accessControlId) != null && lstBUDetails.Where(data => data.EntityId == accessControlId).Any() ? lstBUDetails.Where(data => data.EntityId == accessControlId).FirstOrDefault().OrgEntityCode
                                                    : 0 : 0;
                }

                #endregion "finding BU"

                if (IsHeaderEntityAccessControlEntity)
                {
                    if (objRequisition.DocumentAdditionalEntitiesInfoList != null && objRequisition.DocumentAdditionalEntitiesInfoList.Count() > 0)
                    {
                        headerOrgEntityDetailCode = objRequisition.DocumentAdditionalEntitiesInfoList.Where(_EntityID => _EntityID.EntityId == accessControlId).FirstOrDefault().EntityDetailCode;
                    }
                }
                else
                {
                    headerOrgEntityDetailCode = 0;
                }

                if (!String.IsNullOrEmpty(objBZRequisition.Requisition.RequesterPASCode))
                {
                    objRequisition.RequesterId = objCommon.GetContactCodeByClientContactCodeOrEmail(objBZRequisition.Requisition.RequesterPASCode, "");
                }

                #region 'Validate Catalog Line Items and splits.'

                lstEntityDeails = objRequisition.RequisitionItems.Where(itm => itm.ItemSplitsDetail != null).SelectMany(itmSplt => itmSplt.ItemSplitsDetail).SelectMany(splt => splt.DocumentSplitItemEntities).ToList();

                if (lstEntityDeails != null && lstEntityDeails.Count == 0)
                {
                    if (objRequisition.DocumentAdditionalEntitiesInfoList != null && objRequisition.DocumentAdditionalEntitiesInfoList.Any())
                    {
                        defaultAccDetails = objDocBO.GetDocumentDefaultAccountingDetails(P2PDocumentType.Requisition, LevelType.ItemLevel, UserContext.ContactCode, 0, objRequisition.DocumentAdditionalEntitiesInfoList.ToList(), null, false, 0, lobEntityDetailCode);
                        setDefaultSplit = true;
                    }
                }
                else
                {
                    EntityCode = objRequisition.DocumentAdditionalEntitiesInfoList != null && objRequisition.DocumentAdditionalEntitiesInfoList.Any() ? objRequisition.DocumentAdditionalEntitiesInfoList.FirstOrDefault().EntityCode : "";
                    result = GetReqInterfaceDao().GetSplitsDetails(objRequisition.RequisitionItems, objRequisition.CreatedBy, lobEntityDetailCode, EntityCode);
                    if (DeriveHeaderEntities)
                    {
                        commonManager.UpdateSplitDeatilsBasedOnHeaderEntity(objRequisition.DocumentAdditionalEntitiesInfoList.ToList(), result);
                    }
                    List<RequisitionItem> nonSplitItem = objRequisition.RequisitionItems.Where(a => a.ItemSplitsDetail != null && a.ItemSplitsDetail.Count == 0).ToList();

                    if (result != null && result.Tables["SplitEntity"].Rows.Count > 0)
                    {
                        foreach (var itm in objRequisition.RequisitionItems)
                        {
                            splitCounters = 1;
                            foreach (RequisitionSplitItems itemSplitDetail in itm.ItemSplitsDetail)
                            {
                                itemSplitDetail.DocumentSplitItemId = 0;
                                itemSplitDetail.UiId = splitCounters;
                                itemSplitDetail.DocumentItemId = itm.DocumentItemId;
                                itemSplitDetail.SplitType = SplitType.Percentage;

                                var splititems = result.Tables[0].AsEnumerable().Where(row => Convert.ToInt64(row["ItemLineNumber"]) == itm.ItemLineNumber && splitCounters == Convert.ToInt64(row["Uids"]));

                                if (splititems != null && splititems.Any())
                                {
                                    List<DocumentSplitItemEntity> obJ = new List<DocumentSplitItemEntity>();
                                    foreach (DataRow split in splititems)
                                    {

                                        obJ.Add(
                                            new DocumentSplitItemEntity()
                                            {
                                                SplitAccountingFieldId = (int)split["SplitAccountingFieldConfigId"],
                                                UiId = (int)split["Uids"],
                                                SplitAccountingFieldValue = Convert.ToString(split["EntityDetailCode"]),
                                                EntityTypeId = (int)split["EntityTypeId"],
                                                EntityCode = Convert.ToString(split["EntityCode"]),

                                            });

                                    }

                                    itemSplitDetail.DocumentSplitItemEntities = obJ;

                                }
                                splitCounters++;
                            }

                        }
                    }
                    if (nonSplitItem.Count > 0 && objRequisition.DocumentAdditionalEntitiesInfoList != null && objRequisition.DocumentAdditionalEntitiesInfoList.Any())
                    {
                        splitCounters = 1;
                        defaultAccDetails = objDocBO.GetDocumentDefaultAccountingDetails(P2PDocumentType.Requisition, LevelType.ItemLevel, UserContext.ContactCode, 0, objRequisition.DocumentAdditionalEntitiesInfoList.ToList(), null, false, 0, lobEntityDetailCode);
                        //defaultAccDetails.ForEach(splt => splt.UiId = splitCounters);
                        foreach (var itm in nonSplitItem)
                        {
                            defaultAccDetails.ForEach(splt => splt.UiId = splitCounters);
                            itm.ItemSplitsDetail = new List<RequisitionSplitItems>()
                                                            {
                                                                new RequisitionSplitItems(){
                                                                    DocumentSplitItemEntities = defaultAccDetails,
                                                                    SplitType = SplitType.Percentage,
                                                                    DocumentItemId = itm.DocumentItemId,
                                                                    UiId=splitCounters
                                                                }
                                                            };
                            splitCounters++;


                        }
                    }
                }
                #endregion 'Validate Catalog Line Items.'

                #region 'Setting for Requisition Status'

                int _tempDocumentStatus = 0;
                if (objRequisition.DocumentStatusInfo == Documents.Entities.DocumentStatus.None)
                {
                    settingValue = objCommon.GetSettingsValueByKey(interfaceSettings, "RequisitionStatus");
                    DocumentStatus = string.IsNullOrEmpty(settingValue) ? 1 : Convert.ToInt32(settingValue);
                    if (DocumentStatus > 1)
                    {
                        _tempDocumentStatus = DocumentStatus;
                        objRequisition.DocumentStatusInfo = (Documents.Entities.DocumentStatus)(DocumentStatus);
                    }
                    else
                        objRequisition.DocumentStatusInfo = Documents.Entities.DocumentStatus.Draft;
                }
                else
                {
                    if (objRequisition.DocumentStatusInfo == Documents.Entities.DocumentStatus.ApprovalPending || objRequisition.DocumentStatusInfo == Documents.Entities.DocumentStatus.Approved)
                    {
                        _tempDocumentStatus = Convert.ToInt32(objRequisition.DocumentStatusInfo);
                        objRequisition.DocumentStatusInfo = Documents.Entities.DocumentStatus.Draft;
                    }
                }
                #endregion 'Setting for Requisition Status'

                if (objRequisition.RequisitionItems != null && objRequisition.RequisitionItems.Count > 0)
                {
                    List<KeyValuePair<string, decimal>> lstKvpPartnerDetails = new List<KeyValuePair<string, decimal>>();
                    List<PartnerLocation> orderingLoc = new List<PartnerLocation>();
                    foreach (var item in objRequisition.RequisitionItems)
                    {
                        orderingLoc.Clear();
                        // 'Set Partner Code'
                        if (lstKvpPartnerDetails.Where(data => data.Key == item.ClientPartnerCode).Any())
                            item.PartnerCode = lstKvpPartnerDetails.Where(data => data.Key == item.ClientPartnerCode).FirstOrDefault().Value;
                        else
                        {
                            if (IsClientCodeBasedonLinkLocation == 1)
                            {
                                PartnerLinkedLocationMapping linklocation = new PartnerLinkedLocationMapping();
                                linklocation = objCommon.GetLinkedLocationBySourceSystemValue(item.ClientPartnerCode);
                                item.PartnerCode = linklocation.PartnerCode;
                                item.OrderLocationId = linklocation.LocationId;
                                item.RemitToLocationId = linklocation.LinkedLocationId; //to uncomment once web make this changes

                            }
                            else
                            {
                                string SourcesystemName = objRequisition.SourceSystemInfo != null ? objRequisition.SourceSystemInfo.SourceSystemName : string.Empty;
                                if (DerivePartnerFromLocationCode)
                                {
                                    DataTable dtPartnerDetails = new DataTable();
                                    dtPartnerDetails = GetCommonDao().GetPartnerandContactCodeDetails(item.ClientPartnerCode, "", "", this.UserContext.BuyerPartnerCode, "", item.ClientPartnerCode, "",
                                                        SourcesystemName, true, PartnerStatuscodes);
                                    if (dtPartnerDetails != null && dtPartnerDetails.Rows != null && dtPartnerDetails.Rows.Count > 0)
                                    {
                                        item.PartnerCode = Convert.ToInt64(dtPartnerDetails.Rows[0]["PartnerCode"]);
                                        item.OrderLocationId = Convert.ToInt64(dtPartnerDetails.Rows[0]["LocationCode"]);
                                    }
                                }
                                else
                                {
                                    item.PartnerCode = objCommon.GetPartnerCodeByClientPartnerCode(item.ClientPartnerCode, SourcesystemName);

                                    if (item.PartnerCode > 0)
                                        lstKvpPartnerDetails.Add(new KeyValuePair<string, decimal>(item.ClientPartnerCode, item.PartnerCode));
                                }
                            }
                        }

                        if (IsOrderingLocationMandatory)
                        {
                            long partnercode = 0;
                            string headerEntities = string.Empty;
                            if (item.PartnerCode > 0)
                                partnercode = Convert.ToInt64(item.PartnerCode);
                            if (objRequisition.DocumentAdditionalEntitiesInfoList != null && objRequisition.DocumentAdditionalEntitiesInfoList.Count > 0)
                                headerEntities = string.Join(",", objRequisition.DocumentAdditionalEntitiesInfoList.Select(entity => entity.EntityDetailCode.ToString()));
                            item.OrderLocationId = GetReqDao().GetOrderLocationIdByClientLocationCode(item.OrderLocationName, partnercode, headerEntities, IsDefaultOrderingLocation);
                        }

                        if (!string.IsNullOrWhiteSpace(item.ItemNumber))
                        {
                            var dr = dtItems.NewRow();
                            dr["BuyerItemNumber"] = item.ItemNumber;
                            dr["PartnerCode"] = item.PartnerCode;
                            dr["UOM"] = item.UOM;
                            dtItems.Rows.Add(dr);
                        }

                        #region Set Category Id based on Unspsc Id
                        if (item.Unspsc > 0)
                        {
                            if (lstKvpCategoryUnspsc.Where(data => data.Key == item.Unspsc).Any())
                            {
                                item.CategoryId = lstKvpCategoryUnspsc.Where(data => data.Key == item.Unspsc).FirstOrDefault().Value;
                            }
                            else
                            {
                                item.CategoryId = objCommon.GetPASCodeFromUNSPSCId(item.Unspsc);
                                if (item.CategoryId > 0)
                                {
                                    lstKvpCategoryUnspsc.Add(new KeyValuePair<long, long>(item.Unspsc, item.CategoryId));
                                }
                            }
                        }
                        #endregion
                        else
                        {
                            #region Set Category Id based on Client Category Id
                            if (!string.IsNullOrEmpty(item.ClientCategoryId))
                            {
                                if (lstKvpCategoryDetails.Where(data => data.Key == item.ClientCategoryId).Any())
                                    item.CategoryId = lstKvpCategoryDetails.Where(data => data.Key == item.ClientCategoryId).FirstOrDefault().Value;
                                else
                                {
                                    var requestHeaders = new GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.RequestHeaders();
                                    requestHeaders.Set(this.UserContext, this.JWTToken);
                                    string appName = Environment.GetEnvironmentVariable("NewRelic.AppName") ?? "Requisition"; 
                                    string useCase = "RequisitionInterfaceManager-SaveRequisitionFromInterface";
                                    var serviceURL = MultiRegionConfig.GetConfig(CloudConfig.APIMBaseURL) + GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.ServiceURLs.CategoryServiceURL + "GetCategoryCodeForClientCategoryCode?categoryCode=" + item.ClientCategoryId;

                                    var webAPI = new GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.WebAPI(requestHeaders, appName, useCase);
                                    var response = webAPI.ExecuteGet(serviceURL);
                                    item.CategoryId = Convert.ToInt64(response);

                                    if (item.CategoryId > 0)
                                        lstKvpCategoryDetails.Add(new KeyValuePair<string, long>(item.ClientCategoryId, item.CategoryId));
                                }
                            }
                            #endregion
                        }

                        var dr1 = dtItemsDetails.NewRow();
                        dr1["ItemLineNumber"] = item.ItemLineNumber;
                        dr1["BuyerItemNumber"] = item.ItemNumber;
                        dr1["SupplierItemNumber"] = item.SupplierPartId;
                        dr1["UOM"] = item.UOM;
                        dr1["ContractNumber"] = item.ExtContractRef;
                        dr1["PartnerCode"] = Convert.ToInt64(item.PartnerCode);
                        dtItemsDetails.Rows.Add(dr1);
                    }
                }

                #region catalog validation

                if (dtItems.Rows != null && dtItems.Rows.Count > 0 && CallGetLineItemsAPIOnRequisition != 1)
                {
                    dtResult = objDocBO.ValidateInternalCatalogItems(P2PDocumentType.Requisition, dtItems);

                    if (dtResult.Rows != null && dtResult.Rows.Count > 0)
                    {
                        foreach (DataRow dtRow in dtResult.Rows)
                        {
                            if (dtRow["ItemStatus"].ToString() == "F")
                                strErrors.AppendLine(',' + dtRow["ItemNumberWithDesc"].ToString());
                        }
                    }

                }
                dtItems.Dispose();
                #endregion

                #region Validation for Excepton Handling

                if (AllowRequisitionErrorProcessing)
                {
                    if (dtItemsDetails.Rows != null && dtItemsDetails.Rows.Count > 0)
                    {
                        dtValiadtionDetails = ValidateReqItemsForExceptionHandling(dtItemsDetails);
                    }
                }

                #endregion Validation for Excepton Handling

                foreach (var item in objRequisition.RequisitionItems)
                {

                    if (!string.IsNullOrWhiteSpace(item.ItemNumber))
                    {
                        var matchingRow = dtResult.AsEnumerable().Where(dr => Convert.ToString(dr["ItemNumberWithDesc"]).Equals(item.ItemNumber, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                        if (matchingRow != null)
                        {
                            if (item.CategoryId <= 0 && item.Unspsc <= 0)
                            {
                                item.CategoryId = Convert.ToInt64(matchingRow["CategoryId"].ToString());
                                if (item.CategoryId <= 0)
                                    item.Unspsc = Convert.ToInt32(matchingRow["Unspsc"].ToString());
                            }
                            item.CatalogItemId = Convert.ToInt64(matchingRow["CatalogItemId"].ToString());
                            item.SourceType = (ItemSourceType)Convert.ToInt16(matchingRow["CatalogSource"].ToString());
                            item.IsTaxExempt = Convert.ToBoolean(matchingRow["IsTaxExempt"].ToString());
                        }
                    }
                    #region Derive Item Detail from Catalog/Item Master if Setting (IsDeriveItemDetailEnable) is ON

                    if (IsDeriveItemDetailEnable == true && CallGetLineItemsAPIOnRequisition != 1)
                    {
                        if (item.ItemNumber != null && item.ItemNumber != "" && item.PartnerSourceSystemValue != null && item.PartnerSourceSystemValue != ""
                            && item.UOM != null && item.UOM != "")
                        {
                            DataTable dt = objDocBO.DeriveItemDetails(item.ItemNumber, item.PartnerSourceSystemValue, item.UOM);

                            if (dt.Rows != null && dt.Rows.Count > 0)
                            {
                                foreach (DataRow dtRow in dt.Rows)
                                {
                                    if (string.IsNullOrEmpty(item.Description))
                                    {
                                        item.Description = dtRow["ItemDescription"].ToString();
                                    }

                                    if (string.IsNullOrEmpty(item.ShortName))
                                    {
                                        item.ShortName = dtRow["ShortName"].ToString();
                                    }

                                    if (string.IsNullOrEmpty(item.SupplierItemNumber))
                                    {
                                        item.SupplierItemNumber = dtRow["SupplierItemNumber"].ToString();
                                        item.SupplierPartId = dtRow["SupplierItemNumber"].ToString();
                                    }

                                    if (item.UnitPrice == null)
                                    {
                                        if (dtRow.Table.Columns.Contains("FromUOM"))
                                        {
                                            if (item.UOM == dtRow["FromUOM"].ToString())
                                            {
                                                decimal FromConversionFactor;

                                                if (dtRow["FromConversionFactor"].ToString() != null && dtRow["FromConversionFactor"].ToString() != "")
                                                {
                                                    FromConversionFactor = Convert.ToDecimal(dtRow["FromConversionFactor"].ToString());

                                                    if (dtRow["UnitPrice"].ToString() != null && dtRow["UnitPrice"].ToString() != "")
                                                    {
                                                        item.UnitPrice = Convert.ToDecimal(dtRow["UnitPrice"].ToString()) / FromConversionFactor;
                                                    }
                                                }

                                            }

                                            else if (item.UOM == dtRow["ToUOM"].ToString())
                                            {
                                                decimal ConversionFactor;

                                                if (dtRow["ConversionFactor"].ToString() != null && dtRow["ConversionFactor"].ToString() != "")
                                                {
                                                    ConversionFactor = Convert.ToDecimal(dtRow["ConversionFactor"].ToString());

                                                    if (dtRow["UnitPrice"].ToString() != null && dtRow["UnitPrice"].ToString() != "")
                                                    {
                                                        item.UnitPrice = Convert.ToDecimal(dtRow["UnitPrice"].ToString()) / ConversionFactor;
                                                    }
                                                }
                                            }
                                        }

                                        else
                                        {
                                            if (dtRow["UnitPrice"].ToString() != null && dtRow["UnitPrice"].ToString() != "")
                                                item.UnitPrice = Convert.ToDecimal(dtRow["UnitPrice"].ToString());
                                        }
                                    }

                                    if (item.DateNeeded == null || item.DateNeeded == DateTime.MinValue)
                                    {
                                        int LeadTime = 0;

                                        if (dtRow.Table.Columns.Contains("LeadTime"))
                                        {
                                            if (dtRow["LeadTime"].ToString() != null && dtRow["LeadTime"].ToString() != "")
                                                LeadTime = Convert.ToInt32(dtRow["LeadTime"].ToString());

                                            if (LeadTime > 0)
                                                item.DateNeeded = DateTime.Now.AddDays(LeadTime);
                                        }
                                    }

                                    if (dtRow.Table.Columns.Contains("CategoryID"))
                                    {
                                        if (dtRow["CategoryID"].ToString() != null && dtRow["CategoryID"].ToString() != "")
                                            item.CategoryId = Convert.ToInt64((dtRow["CategoryID"].ToString()));
                                    }

                                    if (item.ManufacturerName == null || item.ManufacturerName == "")
                                        item.ManufacturerName = dtRow["ManufacturerName"].ToString();

                                    if (item.ManufacturerPartNumber == null || item.ManufacturerPartNumber == "")
                                        item.ManufacturerPartNumber = dtRow["ManufacturerPartNumber"].ToString();

                                    if (dtRow.Table.Columns.Contains("CatalogItemID") && dtRow.Table.Columns.Contains("ItemId"))
                                    {
                                        if (!string.IsNullOrEmpty(dtRow["CatalogItemID"].ToString()) && !string.IsNullOrEmpty(dtRow["ItemId"].ToString()))
                                        {
                                            var objdr = objdt.NewRow();
                                            objdr["ItemNumber"] = item.ItemNumber;
                                            objdr["CatalogItemID"] = Convert.ToInt64(dtRow["CatalogItemID"].ToString());
                                            objdr["ItemMasterItemId"] = Convert.ToInt64(dtRow["ItemId"].ToString());
                                            objdr["tardocumentType"] = 7;
                                            objdt.Rows.Add(objdr);
                                        }
                                    }

                                    else if (dtRow.Table.Columns.Contains("ItemId"))
                                    {
                                        if (dtRow["ItemId"].ToString() != null && dtRow["ItemId"].ToString() != "")
                                        {
                                            var objdr = objdt.NewRow();
                                            objdr["ItemNumber"] = item.ItemNumber;
                                            objdr["CatalogItemID"] = 0;
                                            objdr["ItemMasterItemId"] = Convert.ToInt64(dtRow["ItemId"].ToString());
                                            objdr["tardocumentType"] = 7;
                                            objdt.Rows.Add(objdr);
                                        }
                                    }

                                    else if (dtRow.Table.Columns.Contains("CatalogItemID"))
                                    {
                                        if (dtRow["CatalogItemID"].ToString() != null && dtRow["CatalogItemID"].ToString() != "")
                                        {
                                            var objdr = objdt.NewRow();
                                            objdr["ItemNumber"] = item.ItemNumber;
                                            objdr["CatalogItemID"] = Convert.ToInt64(dtRow["CatalogItemID"].ToString());
                                            objdr["ItemMasterItemId"] = 0;
                                            objdr["tardocumentType"] = 7;
                                            objdt.Rows.Add(objdr);
                                        }
                                    }
                                }
                            }

                            if (item.ShortName == null && item.Description == null)
                            {
                                item.Description = "";
                                item.ShortName = "";
                            }
                        }

                        if (item.ShortName == null && item.Description == null)
                        {
                            item.Description = "";
                            item.ShortName = "";
                        }
                    }

                    else
                    {
                        if (item.Description == null)
                        {
                            item.Description = "";
                            item.ShortName = "";
                        }

                    }

                    #endregion
                    if ((useTaxMaster && Convert.ToDecimal(item.Tax) > 0) || item.IsTaxExempt)
                        item.Tax = 0;
                    item.ItemType = item.ItemType == ItemType.None ? ItemType.Material : item.ItemType;

                    item.CreatedBy = this.UserContext.ContactCode;
                    if (!String.IsNullOrEmpty(item.ClientContactCode))
                    {
                        item.BuyerContactCode = objCommon.GetContactCodeByClientContactCodeOrEmail(item.ClientContactCode, "");
                    }
                    item.DateRequested = (item.DateRequested == null || item.DateRequested == DateTime.MinValue) ? DateTime.Now : item.DateRequested;

                    if (!AllowRequisitionErrorProcessing)
                    {
                        item.DateNeeded = (item.DateNeeded == null || item.DateNeeded == DateTime.MinValue) ? item.DateRequested.Value.AddDays(noOfDaysForDateRequested) : item.DateNeeded;
                        item.DateNeeded = objDocBO.TimeStampCheck(Convert.ToDateTime(item.DateNeeded));
                    }
                    item.DateRequested = objDocBO.TimeStampCheck(Convert.ToDateTime(item.DateRequested));

                    if (item.StartDate.HasValue)
                    {
                        item.StartDate = objDocBO.TimeStampCheck(Convert.ToDateTime(item.StartDate));
                    }
                    if (item.EndDate.HasValue)
                    {
                        item.EndDate = objDocBO.TimeStampCheck(Convert.ToDateTime(item.EndDate));
                    }

                    // Make invalid data blank
                    if (AllowRequisitionErrorProcessing && dtValiadtionDetails != null && dtValiadtionDetails.Rows.Count > 0)
                    {
                        var matchingRow = dtValiadtionDetails.AsEnumerable().Where(dr => Convert.ToInt64(dr["ItemLineNumber"]) == item.ItemLineNumber).FirstOrDefault();
                        if (matchingRow != null)
                        {
                            if (Convert.ToInt32(matchingRow["IsValidContractNumber"].ToString()) == 0)
                                item.ExtContractRef = "";
                            if (Convert.ToInt32(matchingRow["IsValidBIN"].ToString()) == 0)
                                item.ItemNumber = "";
                        }
                    }

                    #region Set unit price based on setting "IncludePriceDetails"

                   if (isIncludePriceDetails)
                    {
                        settingValue = objCommon.GetSettingsValueByKey(interfaceSettings, "CallWebAPIOnRequisition");
                        bool CallWebAPIOnRequisition = !string.IsNullOrEmpty(settingValue) ? Convert.ToBoolean(settingValue) : false;

                        if (CallGetLineItemsAPIOnRequisition == 0)
                        {
                            if (item.CustomAttributes == null)
                                item.CustomAttributes = new List<Questionnaire>();

                            if (item.lstAdditionalFieldAttributues == null)
                                item.lstAdditionalFieldAttributues = new List<BusinessEntities.P2PAdditionalFieldAtrribute>();

                            SetSourceTypeForChevron(item, objRequisition, IsCaptureCatalogApiResponse, deriveItemAdditionalFieldsFromCatalog, CallWebAPIOnRequisition);
                        }

                        if (CallGetLineItemsAPIOnRequisition == 1)
                            SetSourceTypeForPetronas(item, objRequisition, IsCaptureCatalogApiResponse, CallWebAPIOnRequisition);
                    }
                    #endregion Set unit price based on setting "IncludePriceDetails"
                }

                var taxExcmptCount = objRequisition.RequisitionItems.Where(itm => itm.IsTaxExempt == true);
                if ((taxExcmptCount != null && taxExcmptCount.Count() == objRequisition.RequisitionItems.Count) || useTaxMaster)
                    objRequisition.Tax = 0;

                /*Validation Part Starts*/
                var lstErrors = new List<string>();
                var dctSetting = interfaceSettings.lstSettings.Distinct().ToDictionary(sett => sett.FieldName, sett => sett.FieldValue);

                //var Error = ValidateInterfaceShiptoForPO_New(objCommon, objOrder, dctSetting, out isHeaderShipToValid, out isLiShipToValid);
                var Error = ValidateShipToBillToFromInterface(objCommon, objRequisition, dctSetting, out isHeaderShipToValid, out isLiShipToValid, lobEntityDetailCode);

                /* Validation Part Ends*/

                LogHelper.LogInfo(Log, "SaveRequisitionFromInterface : SaveBillToLocation Method Started  : " + objRequisition.DocumentNumber);

                objRequisition.CreatedOn = objDocBO.TimeStampCheck(objRequisition.CreatedOn);

                #region 'Bill To Location'

                /* Logic To Save Adhoc BillToLocation */
                if (objRequisition.BilltoLocation !=null && objRequisition.BilltoLocation.BilltoLocationId != 0)
                {
                    if (objRequisition.DocumentLOBDetails != null && objRequisition.DocumentLOBDetails.Any() && !string.IsNullOrEmpty(objRequisition.DocumentLOBDetails.FirstOrDefault().EntityCode))
                    {
                        objRequisition.BilltoLocation.LOBEntityCode = objRequisition.DocumentLOBDetails.FirstOrDefault().EntityCode;
                    }
                    if (objRequisition.DocumentAdditionalEntitiesInfoList != null && objRequisition.DocumentAdditionalEntitiesInfoList.Any())
                    {
                        var _lstOrgEntity = from listOrgEntityCodes in objRequisition.DocumentAdditionalEntitiesInfoList.Where(_enityId => _enityId.EntityId == EntityMappedToBillToLocation)
                                            select listOrgEntityCodes;

                        if (_lstOrgEntity != null && _lstOrgEntity.Count() > 0)
                        {
                            List<DocumentAdditionalEntityInfo> lstDocumentAdditionalEntityInfo = new List<DocumentAdditionalEntityInfo>();

                            lstDocumentAdditionalEntityInfo = _lstOrgEntity.Cast<DocumentAdditionalEntityInfo>().ToList();
                            
                            objRequisition.BilltoLocation.lstOrgEntity = lstDocumentAdditionalEntityInfo.ConvertAll(x => new P2P.BusinessEntities.BZOrgEntity()
                            {
                                EntityCode = x.EntityCode,
                                EntityType = x.EntityDisplayName,
                                IsDefault = false
                            });
                        }
                    }
                    objRequisition.BilltoLocation = objCommon.SaveBillToLocation(objRequisition.BilltoLocation);// INDEXING IN RESPECTIVE MANAGER OR BELOW ORDER INDEX
                }
                #endregion 'Bill To Location'

                #region Deliver To Location           
                if (deliverToFreeText == false)
                {
                    if (objRequisition.DelivertoLocation != null && !string.IsNullOrWhiteSpace(objRequisition.DelivertoLocation.DelivertoLocationNumber))
                    {
                        var deliverTo = objCommon.GetDeliverToLocationByNumber(objRequisition.DelivertoLocation.DelivertoLocationNumber);
                        if (!object.ReferenceEquals(deliverTo, null) && deliverTo.DelivertoLocationId > 0)
                        {
                            objRequisition.DelivertoLocation.DelivertoLocationId = deliverTo.DelivertoLocationId;
                        }
                    }
                }
                headerDeliverto = objRequisition.DelivertoLocation != null ? objRequisition.DelivertoLocation.DeliverTo : "";
                #endregion

                #region 'Save Ship To Location'
                LogHelper.LogInfo(Log, "SaveRequisitionFromInterface : AddUpdateShipToDetail Method Started  : " + obj.DocumentNumber);
                if (objRequisition.ShiptoLocation != null && !isLiShipToValid && isHeaderShipToValid && objRequisition.ShiptoLocation.ShiptoLocationId <= 0)
                {
                    if (objRequisition.DocumentLOBDetails != null && objRequisition.DocumentLOBDetails.Any() && !string.IsNullOrEmpty(objRequisition.DocumentLOBDetails.FirstOrDefault().EntityCode))
                    {
                        objRequisition.ShiptoLocation.LOBEntityCode = objRequisition.DocumentLOBDetails.FirstOrDefault().EntityCode;
                    }
                    if (objRequisition.DocumentAdditionalEntitiesInfoList != null && objRequisition.DocumentAdditionalEntitiesInfoList.Any())
                    {
                        var _lstOrgEntity = from listOrgEntityCodes in objRequisition.DocumentAdditionalEntitiesInfoList.Where(_enityId => _enityId.EntityId == EntityMappedToShipToLocation)
                                            select listOrgEntityCodes;

                        if (_lstOrgEntity != null && _lstOrgEntity.Count() > 0)
                        {
                            List<DocumentAdditionalEntityInfo> lstDocumentAdditionalEntityInfo = new List<DocumentAdditionalEntityInfo>();

                            lstDocumentAdditionalEntityInfo = _lstOrgEntity.Cast<DocumentAdditionalEntityInfo>().ToList();

                            objRequisition.ShiptoLocation.lstOrgEntity = lstDocumentAdditionalEntityInfo.ConvertAll(x => new P2P.BusinessEntities.BZOrgEntity()
                            {
                                EntityCode = x.EntityCode,
                                EntityType = x.EntityDisplayName,
                                IsDefault = false
                            });
                        }
                    }
                    objCommon.AddUpdateShipToDetail_New(objRequisition.ShiptoLocation, ref objCommon, true);
                }
                #endregion 'Ship To Location'

                if (objRequisition.RequesterId > 0)
                    objRequisition.OnBehalfOf = objRequisition.RequesterId;
                else
                    objRequisition.OnBehalfOf = this.UserContext.ContactCode;

                #region Source System
                if (objRequisition.SourceSystemInfo == null)
                {
                    objRequisition.SourceSystemInfo = new SourceSystemInfo
                    {
                        SourceSystemId = partnerInterfaceId
                    };
                }
                #endregion

                #region line level ship to location and Deliver To Location

                foreach (var item in objRequisition.RequisitionItems)
                {
                    if (item.DocumentItemShippingDetails != null && item.DocumentItemShippingDetails.Count > 0)
                    {
                        foreach (var shippingDetail in item.DocumentItemShippingDetails)
                        {
                            #region 'Logic to decide whether New ShipToLoc to be created if ShiptoLoc doesn't exists'

                            int shiptolocationId = 0;
                            if (isLiShipToValid && shippingDetail.ShiptoLocation != null && shippingDetail.ShiptoLocation.ShiptoLocationId <= 0)
                            {
                                /*Header LOBEntityCode and OrgEntity to be set at Item level Also*/
                                if (objRequisition.DocumentLOBDetails != null && objRequisition.DocumentLOBDetails.Any() && !string.IsNullOrEmpty(objRequisition.DocumentLOBDetails.FirstOrDefault().EntityCode))
                                {
                                    shippingDetail.ShiptoLocation.LOBEntityCode = objRequisition.DocumentLOBDetails.FirstOrDefault().EntityCode;
                                }
                                if (objRequisition.DocumentAdditionalEntitiesInfoList != null && objRequisition.DocumentAdditionalEntitiesInfoList.Any())
                                {
                                    var _lstOrgEntity = from listOrgEntityCodes in objRequisition.DocumentAdditionalEntitiesInfoList.Where(_enityId => _enityId.EntityId == EntityMappedToShipToLocation)
                                                        select listOrgEntityCodes;

                                    if (_lstOrgEntity != null && _lstOrgEntity.Count() > 0)
                                    {
                                        List<DocumentAdditionalEntityInfo> lstDocumentAdditionalEntityInfo = new List<DocumentAdditionalEntityInfo>();

                                        lstDocumentAdditionalEntityInfo = _lstOrgEntity.Cast<DocumentAdditionalEntityInfo>().ToList();

                                        shippingDetail.ShiptoLocation.lstOrgEntity = lstDocumentAdditionalEntityInfo.ConvertAll(x => new P2P.BusinessEntities.BZOrgEntity()
                                        {
                                            EntityCode = x.EntityCode,
                                            EntityType = x.EntityDisplayName,
                                            IsDefault = false
                                        });
                                    }
                                }

                                LogHelper.LogInfo(Log, "SaveRequisitionFromInterface : line item level AddUpdateShipToDetail  Method Started  : " + obj.DocumentNumber);
                                shiptolocationId = objCommon.AddUpdateShipToDetail_New(shippingDetail.ShiptoLocation, ref objCommon, true);
                                if (shiptolocationId != 0)
                                    shippingDetail.ShiptoLocation.ShiptoLocationId = shiptolocationId;
                            }
                            else if (!isLiShipToValid && isHeaderShipToValid)
                                shiptolocationId = shippingDetail.ShiptoLocation.ShiptoLocationId = objRequisition.ShiptoLocation.ShiptoLocationId;
                            else
                                shiptolocationId = shippingDetail.ShiptoLocation.ShiptoLocationId;

                            #endregion


                            #region Get Deliver to Location details

                            if (deliverToFreeText == false)
                            {
                                if (shippingDetail.DelivertoLocation != null && !string.IsNullOrWhiteSpace(shippingDetail.DelivertoLocation.DelivertoLocationNumber))
                                {
                                    var deliverTo = objCommon.GetDeliverToLocationByNumber(shippingDetail.DelivertoLocation.DelivertoLocationNumber);
                                    if (!object.ReferenceEquals(deliverTo, null) && deliverTo.DelivertoLocationId > 0)
                                        delivertolocationid = deliverTo.DelivertoLocationId;
                                }
                                if (shippingDetail != null)
                                    shippingDetail.DeliverTo = string.Empty;
                            }
                            else if (!string.IsNullOrEmpty(item.DocumentItemShippingDetails[0].DelivertoLocation.DeliverTo))
                            {
                                shippingDetail.DelivertoLocation.DeliverTo = item.DocumentItemShippingDetails[0].DelivertoLocation.DeliverTo;
                            }
                            else if (shippingDetail.DelivertoLocation != null && objRequisition.DelivertoLocation != null)
                                shippingDetail.DelivertoLocation.DeliverTo = headerDeliverto;
                            #endregion
                        }
                    }
                    else if (isHeaderShipToValid && objRequisition.ShiptoLocation.ShiptoLocationId > 0)
                    {
                        item.DocumentItemShippingDetails = new List<DocumentItemShippingDetail>()
                    {
                        new DocumentItemShippingDetail(){ ShiptoLocation = new ShiptoLocation()
                        {
                            ShiptoLocationId = objRequisition.ShiptoLocation.ShiptoLocationId

                        },
                        DelivertoLocation = new DelivertoLocation()
                        {
                            DelivertoLocationId = objRequisition.DelivertoLocation != null ? objRequisition.DelivertoLocation.DelivertoLocationId : 0,
                            DeliverTo = objRequisition.DelivertoLocation != null ? objRequisition.DelivertoLocation.DeliverTo : string.Empty
                        },
                        ShippingMethod="",
                        Quantity= item.Quantity
                    }};
                    }
                }
                #endregion

                #region Set Currency Code
                if (objRequisition.Currency == null || objRequisition.Currency == "")
                {
                    var cur = objRequisition.RequisitionItems.Where(items => items.Currency != "");
                    string currency = string.Empty;
                    if (cur.Count() > 0)
                    {
                        currency = cur.FirstOrDefault().Currency;
                    }

                    if (currency == "")
                    {
                        objRequisition.Currency = "USD";
                        foreach (var item in objRequisition.RequisitionItems)
                        {
                            item.Currency = objRequisition.Currency;
                        }
                    }
                    else
                    {
                        objRequisition.Currency = currency;
                        foreach (var item in objRequisition.RequisitionItems)
                        {
                            item.Currency = currency;
                        }
                    }

                }
                else
                {
                    //Assign to ItemS only, Document curr
                    foreach (var item in objRequisition.RequisitionItems)
                    {
                        item.Currency = objRequisition.Currency;
                    }
                }
                #endregion

                var ItemTotal = objRequisition.RequisitionItems.Sum(itm => (itm.Quantity * itm.UnitPrice)).Value;

                //Saving Line Items
                if (isDocumentNameAsNumber)
                    objRequisition.DocumentName = objRequisition.DocumentNumber;


                #region Set PurchaseType
                List<PurchaseType> lstPurchaseType = commonManager.GetPurchaseTypes();
                if (lstPurchaseType != null && lstPurchaseType.Any())
                {
                    if (!string.IsNullOrEmpty(objRequisition.PurchaseTypeDescription))
                    {
                        objRequisition.PurchaseType = lstPurchaseType.Where(p => p.Description == objRequisition.PurchaseTypeDescription) != null && lstPurchaseType.Where(p => p.Description == objRequisition.PurchaseTypeDescription).Any() ?
                            lstPurchaseType.Where(p => p.Description == objRequisition.PurchaseTypeDescription).FirstOrDefault().PurchaseTypeId :
                            lstPurchaseType.Where(_id => _id.IsDefault == true).Count() > 0 ? lstPurchaseType.Where(_id => _id.IsDefault == true).FirstOrDefault().PurchaseTypeId : 0;
                    }
                    else
                        objRequisition.PurchaseType = lstPurchaseType.Where(_id => _id.IsDefault == true).Count() > 0 ? lstPurchaseType.Where(_id => _id.IsDefault == true).FirstOrDefault().PurchaseTypeId : 0;
                }
                #endregion

                objRequisition.DocumentCode = objDocBO.Save(P2PDocumentType.Requisition, objRequisition, false, 0, false, true, true, false);



                if (objRequisition.DocumentCode > 0)
                {
                    #region Save Header Level Comments

                    objCommon.SaveCommentsFromInterface(objRequisition.Comments, P2PDocumentType.Requisition, objRequisition.CreatedBy, objRequisition.DocumentCode, 1, objRequisition.DocumentNumber);

                    #endregion

                    #region Save Notes And Attachments 
                    if (objRequisition.ListNotesOrAttachments != null && objRequisition.ListNotesOrAttachments.Count > 0)
                    {

                        foreach (NotesOrAttachments notesOrAttachment in objRequisition.ListNotesOrAttachments)
                        {
                            notesOrAttachment.DocumentCode = objRequisition.DocumentCode;
                            notesOrAttachment.CreatedBy = objRequisition.CreatedBy;
                            notesOrAttachment.ModifiedBy = objRequisition.CreatedBy;
                            if (notesOrAttachment.FilePath != null && notesOrAttachment.FilePath.Any())
                            {
                                var objFileDetails = commonManager.UploadAndSaveFile(notesOrAttachment.FilePath, notesOrAttachment.NoteOrAttachmentName);
                                notesOrAttachment.FileId = objFileDetails.FileId;

                            }

                            var reqNotesOrAttachment = objDocBO.MapNotesOrAttachments(notesOrAttachment);
                            objDocBO.SaveNotesAndAttachments(P2PDocumentType.Requisition, reqNotesOrAttachment);

                        }
                    }
                    #endregion

                    #region Save Header Additional Fields

                    SaveHeaderAdditionalAttributeFieldFromInterface(objRequisition.DocumentCode, objRequisition.PurchaseTypeDescription, objRequisition.lstAdditionalFieldAttributues);

                    #endregion

                    #region 'saving line items'
                    List<P2PItem> RequisitionItems = objRequisition.RequisitionItems.OrderBy(a => a.ItemLineNumber).Cast<P2PItem>().ToList();
                    RequisitionItems.Select(a => a.DocumentId = objRequisition.DocumentCode).ToList();
                    List<RequisitionItem> lstRequisitionItems = RequisitionItems.Cast<RequisitionItem>().ToList();

                    var ReqTotal_ = objRequisition.RequisitionItems.Sum(itm => ((itm.Quantity * (itm.UnitPrice == null ? 0 : itm.UnitPrice)) + itm.ShippingCharges == null ? 0 : itm.ShippingCharges + itm.Tax == null ? 0 : itm.Tax + itm.AdditionalCharges == null ? 0 : itm.AdditionalCharges)).Value;

                    foreach (var item in lstRequisitionItems)
                    {
                        item.DocumentItemId = objDocBO.SaveItem(P2PDocumentType.Requisition, item, item.IsTaxExempt, false, useTaxMaster, false);
                        #region Fetch and set values for Contract and supplier Information from Contract API

                        if (SpendControlType == 1 && !string.IsNullOrEmpty(item.ExtContractRef))
                        {
                            var oldBU = objRequisition.BusinessUnitId;

                            if (objRequisition.DocumentAdditionalEntitiesInfoList != null && accessControlId > 0 &&
                                objRequisition.DocumentAdditionalEntitiesInfoList.Any(bu => bu.EntityId == accessControlId))
                            {
                                var entity = objRequisition.DocumentAdditionalEntitiesInfoList.Where(a => a.EntityId == accessControlId).FirstOrDefault();
                                objRequisition.BusinessUnitId = entity.EntityDetailCode;
                            }

                            List<long> OrgBUIds = objRequisition.DocumentBUList?.Select(b => b.BusinessUnitCode).ToList();
                            DataSearchResultWrapper ds = new DataSearchResultWrapper();
                            ds = GetAllChildContractsByContractNumberAPI(item.ExtContractRef, objRequisition.DocumentNumber, item.ItemLineNumber, objRequisition.BusinessUnitId);


                            objRequisition.BusinessUnitId = oldBU;

                            if (ds != null && ds.DataSearchResult != null && ds.DataSearchResult.TotalRecords > 0)
                            {
                                var childContract = ds.DataSearchResult.Value.Where(c => !String.IsNullOrEmpty(c.ParentDocumentNumber)).ToList();
                                var childDocumentNumber = childContract != null && childContract.Count > 0 ? childContract[0].DocumentDetails.DocumentNumber : "";

                                if (!String.IsNullOrEmpty(childDocumentNumber) && childContract[0].DocumentSearchOutput != null && childContract[0].DocumentSearchOutput.DocumentAdditionalFieldList != null)
                                {
                                    #region Set Partner Details for contract
                                    var lstchildPartnerCode = childContract[0].DocumentSearchOutput.DocumentAdditionalFieldList.Where(c => c.FieldName == "PartnerCode").ToList();
                                    var childContractPartnerCode = lstchildPartnerCode != null && lstchildPartnerCode.Count > 0 ? Convert.ToInt64(lstchildPartnerCode[0].FieldValue) : 0;

                                    item.SpendControlDocumentNumber = childDocumentNumber;
                                    item.PartnerCode = childContractPartnerCode > 0 ? childContractPartnerCode : item.PartnerCode;

                                    //var orgids = objRequisition.DocumentBUList;

                                    //List<long> OrgBUIds = new List<long>();
                                    List<long> NonOrgBUIds = new List<long>();

                                    //foreach (DocumentBU bu in orgids)
                                    //{
                                    //    OrgBUIds.Add(bu.BusinessUnitCode);
                                    //}


                                    //var NonOrgEntities = objRequisition.DocumentAdditionalEntitiesInfoList.Where(c => c.EntityId == Convert.ToInt64(OrderingLocationbyHeaderEntities)).ToList();
                                    //foreach (DocumentAdditionalEntityInfo d in NonOrgEntities)
                                    //{
                                    //    NonOrgBUIds.Add(d.EntityDetailCode);
                                    //}


                                    if (childContractPartnerCode > 0)
                                    {
                                        //set ordering location id
                                        if (!string.IsNullOrEmpty(OrderingLocationbyHeaderEntities))
                                        {

                                            var nonORGentityid = OrderingLocationbyHeaderEntities.Split('|');
                                            if (nonORGentityid.Count() > 0)
                                            {
                                                foreach (string sorgEntityId in nonORGentityid)
                                                {
                                                    var NonOrgEntities = objRequisition.DocumentAdditionalEntitiesInfoList.Where(c => c.EntityId == Convert.ToInt64(sorgEntityId)).ToList();
                                                    foreach (DocumentAdditionalEntityInfo d in NonOrgEntities)
                                                    {
                                                        NonOrgBUIds.Add(d.EntityDetailCode);
                                                    }
                                                }
                                            }
                                            var orderingLocationsList = reqnewManager.GetOrderingLocationsWithNonOrgEntities(childContractPartnerCode, 2, 0, OrgBUIds, NonOrgBUIds);
                                            item.OrderLocationId = orderingLocationsList != null && orderingLocationsList.Count > 0 ? orderingLocationsList[0].LocationId : item.OrderLocationId;
                                        }

                                        //To be replaced with api call later
                                        //else
                                        //{
                                        //    List<long> lstAccessControlOrgEntityDetailCodes = new List<long>();
                                        //    ProxyPartnerService proxyPartner = new ProxyPartnerService(UserContext, this.JWTToken);
                                        //    var orderingLocations = proxyPartner.GetAllPartnerLocationsByLocationType(childContractPartnerCode, 2, lstAccessControlOrgEntityDetailCodes, "", 0, 0, "");
                                        //    item.OrderLocationId = orderingLocations != null && orderingLocations.Count > 0 ? orderingLocations[0].LocationId : item.OrderLocationId;
                                        //}

                                        //set ship from Location Id
                                        if (!string.IsNullOrEmpty(ShipFromLocationByHeaderEntity))
                                        {
                                            var orderingLocationsList = reqnewManager.GetOrderingLocationsWithNonOrgEntities(childContractPartnerCode, 5, 0, OrgBUIds, NonOrgBUIds);
                                            item.ShipFromLocationId = orderingLocationsList != null && orderingLocationsList.Count > 0 ? orderingLocationsList[0].LocationId : item.ShipFromLocationId;
                                        }
                                        //Supplier Contact

                                        var SupplierContact = commonManager.GetPartnerContactsByPartnerCodeandOrderingLocation(childContractPartnerCode, item.OrderingLocationId);
                                        item.PartnerContactId = SupplierContact != null && SupplierContact.Count > 0 ? SupplierContact[0].ContactCode : item.PartnerContactId;

                                    }
                                    #endregion
                                }
                            }
                        }
                        #endregion
                        if (item.DocumentItemId > 0)
                        {
                            #region Save Notes And Attachments 
                            if (item.ListNotesOrAttachments != null && item.ListNotesOrAttachments.Count > 0)
                            {
                                foreach (NotesOrAttachments notesOrAttachment in item.ListNotesOrAttachments)
                                {
                                    notesOrAttachment.DocumentCode = objRequisition.DocumentCode;
                                    notesOrAttachment.LineItemId = item.DocumentItemId;
                                    notesOrAttachment.CreatedBy = objRequisition.CreatedBy;
                                    notesOrAttachment.ModifiedBy = objRequisition.CreatedBy;
                                    if (notesOrAttachment.FilePath != null && notesOrAttachment.FilePath.Any())
                                    {
                                        var objFileDetails = commonManager.UploadAndSaveFile(notesOrAttachment.FilePath, notesOrAttachment.NoteOrAttachmentName);
                                        notesOrAttachment.FileId = objFileDetails.FileId;
                                    }
                                    var reqNotesOrAttachment = objDocBO.MapNotesOrAttachments(notesOrAttachment);
                                    objDocBO.SaveNotesAndAttachments(P2PDocumentType.Requisition, reqNotesOrAttachment);
                                }
                            }
                            #endregion

                            #region Partner Details
                            objDocBO.SaveItemPartnerDetails(P2PDocumentType.Requisition, item, precessionValue);
                            #endregion

                            #region Item Shipping Details
                            if (item.DocumentItemShippingDetails != null && item.DocumentItemShippingDetails.Count > 0)
                            {
                                foreach (var shippingDetail in item.DocumentItemShippingDetails)
                                {
                                    shippingDetail.DocumentItemId = item.DocumentItemId;
                                    shippingDetail.Quantity = item.Quantity;


                                    LogHelper.LogInfo(Log, "SaveRequisitionFromInterface : SaveItemShippingDetails Method Started  : " + objRequisition.DocumentNumber);
                                    objDocBO.SaveItemShippingDetails(P2PDocumentType.Requisition,
                                                            0,
                                                            shippingDetail.DocumentItemId,
                                                            shippingDetail.ShippingMethod,
                                                            shippingDetail.ShiptoLocation.ShiptoLocationId,
                                                            shippingDetail.DelivertoLocation.DelivertoLocationId,
                                                            shippingDetail.Quantity,
                                                            item.Quantity,
                                                            UserContext.ContactCode,
                                                            shippingDetail.DelivertoLocation != null ? shippingDetail.DelivertoLocation.DeliverTo : string.Empty,
                                                            precessionValue,
                                                            maxPrecessionforTotal,
                                                            maxPrecessionForTaxesAndCharges,
                                                            useTaxMaster
                                                            );


                                }
                            }
                            else if (isHeaderShipToValid && objRequisition.ShiptoLocation.ShiptoLocationId > 0)
                            {
                                objDocBO.SaveItemShippingDetails(P2PDocumentType.Requisition,
                                                            0,
                                                            item.DocumentItemId,
                                                            "",
                                                            objRequisition.ShiptoLocation.ShiptoLocationId,
                                                            objRequisition.DelivertoLocation != null ? objRequisition.DelivertoLocation.DelivertoLocationId : 0,
                                                            item.Quantity,
                                                            item.Quantity,
                                                            UserContext.ContactCode,
                                                            objRequisition.DelivertoLocation != null ? objRequisition.DelivertoLocation.DeliverTo : string.Empty,
                                                            precessionValue,
                                                            maxPrecessionforTotal,
                                                            maxPrecessionForTaxesAndCharges,
                                                            useTaxMaster
                                                            );//INDEXING NOT NEEDED

                            }
                            #endregion

                            #region Save line level comment
                            objCommon.SaveCommentsFromInterface(item.Comments, P2PDocumentType.Requisition, objRequisition.CreatedBy, item.DocumentItemId, 2, objRequisition.DocumentNumber);
                            #endregion

                            #region Save LineItem Level Sub line Requisition
                            List<ChargeMaster> lstChargeMaster = new List<ChargeMaster>();
                            List<ItemCharge> lstlinelevelItemCharge = new List<ItemCharge>();
                            if (item.lstLineItemCharges != null && item.lstLineItemCharges.Any())
                            {
                                // if (lstChargeMaster.Count > 0)
                                lstChargeMaster = objCommon.GetAllChargeName(lobEntityDetailCode);

                                if (lstChargeMaster != null && lstChargeMaster.Any())
                                {
                                    foreach (var _item in item.lstLineItemCharges)
                                    {
                                        if (_item.ChargeDetails != null)
                                        {
                                            /*Filter ChangeName and Assign the values : ChargeMasterID and ChargeName*/
                                            var _lstChargeMaster = lstChargeMaster.Where(_ChargeName => _ChargeName.ChargeName.Equals(_item.ChargeDetails.ChargeName, StringComparison.InvariantCultureIgnoreCase));
                                            if (_lstChargeMaster != null && _lstChargeMaster.Count() == 1)
                                            {
                                                _item.ChargeDetails.ChargeMasterId = _lstChargeMaster.First().ChargeMasterId;
                                                _item.ChargeDetails.ChargeName = _lstChargeMaster.First().ChargeName;

                                                /*Get ChargeMasterBased on ChargeMasterId*/
                                                var objChargeMaster = objCommon.GetChargeMasterDetailsByChargeMasterId(_item.ChargeDetails.ChargeMasterId);
                                                if (objChargeMaster != null)
                                                {
                                                    _item.ChargeDetails.ChargeMasterId = objChargeMaster.ChargeMasterId;
                                                    _item.ChargeDetails.ChargeName = objChargeMaster.ChargeName;
                                                    _item.ChargeDetails.ChargeDescription = objChargeMaster.ChargeDescription;
                                                    _item.ChargeDetails.ChargeTypeCode = objChargeMaster.ChargeTypeCode;
                                                    _item.ChargeDetails.CalculationBasisId = objChargeMaster.CalculationBasisId;
                                                    _item.ChargeDetails.TolerancePercentage = objChargeMaster.TolerancePercentage;
                                                    _item.ChargeDetails.ChargeTypeName = objChargeMaster.ChargeTypeName;

                                                    _item.DocumentCode = item.DocumentId;//RequisitionID
                                                    _item.P2PLineItemID = item.P2PLineItemId;// P2PLineItemID
                                                    _item.ItemTypeID = (int)ItemType.Charge;
                                                    _item.CalculationValue = 0;//                        
                                                    _item.IsHeaderLevelCharge = false;/* Since It's a LineItem Level*/
                                                    _item.CreatedBy = objRequisition.CreatedBy;
                                                    _item.DocumentItemId = item.DocumentItemId;
                                                }
                                                lstlinelevelItemCharge.Add(_item);
                                            }
                                        }
                                    }
                                    objCommon.SaveReqAllItemCharges(lstlinelevelItemCharge); //Save All Requisition LineLevel Charge Data
                                }
                            }
                            #endregion

                            //#region Additional Details
                            GetReqDao().SaveItemAdditionDetails(item, precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges);
                            //#endregion

                            #region Item Other Details
                            objDocBO.SaveItemOtherDetails(P2PDocumentType.Requisition, item, allowTaxCodewithAmount, supplierStatusForValidation);

                            #endregion

                            #region Accounting Details
                            if (item.ItemSplitsDetail != null && item.ItemSplitsDetail.Count > 0)
                            {
                                List<DocumentSplitItemEntity> DocumentSplitItemEntities = new List<DocumentSplitItemEntity>();
                                string isEnableADRRulesSetting = commonManager.GetSettingsValueByKey(p2pSettings, "IsEnableADRRules");
                                bool isEnableADRRules = string.IsNullOrEmpty(isEnableADRRulesSetting) ? false : Convert.ToBoolean(isEnableADRRulesSetting);
                                string enableADRForRequisitionSetting = commonManager.GetSettingsValueByKey(interfaceSettings, "EnableADRForRequisition");
                                bool enableADRForRequisition = string.IsNullOrEmpty(enableADRForRequisitionSetting) ? false : Convert.ToBoolean(enableADRForRequisitionSetting);
                                List<ADRSplit> adrDocumentSplitItemEntities;
                                foreach (RequisitionSplitItems itmSplt in item.ItemSplitsDetail)
                                {
                                    DocumentSplitItemEntities.AddRange(itmSplt.DocumentSplitItemEntities);
                                    itmSplt.Quantity = item.ItemExtendedType == ItemExtendedType.Fixed ? Convert.ToDecimal(item.UnitPrice) : itmSplt.Quantity;
                                }

                                //item.ItemSplitsDetail.ForEach(itm => { itm.DocumentItemId = item.DocumentItemId; itm.Tax = item.Tax; });
                                item.ItemSplitsDetail.ForEach(itm => itm.DocumentItemId = item.DocumentItemId);
                                LogHelper.LogInfo(Log, "SaveRequisitionFromInterface : SaveRequisitionAccountingDetails Method Started  : " + objRequisition.DocumentNumber);
                                reqManager.SaveRequisitionAccountingDetails(item.ItemSplitsDetail, DocumentSplitItemEntities, item.Quantity, precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges, useTaxMaster);

                                if (isEnableADRRules && enableADRForRequisition)
                                {
                                    var splits = reqManager.GetRequisitionAccountingDetailsByItemId(item.DocumentItemId, 1, 1, (int)item.ItemType, objRequisition.EntityDetailCode != null && objRequisition.EntityDetailCode.Count > 0 ? objRequisition.EntityDetailCode.FirstOrDefault() : 0);
                                    var splitConfig = objDocBO.GetAllSplitAccountingFields(P2PDocumentType.Requisition, LevelType.ItemLevel, 0, lobEntityDetailCode);
                                    List<ADRSplit> lstAdrSplit = new List<ADRSplit>();

                                    if (splits != null && splits.Any() && splitConfig != null && splitConfig.Any())
                                    {
                                        List<SplitAccountingFields> splitAcc = new List<SplitAccountingFields>();
                                        foreach (var s in splits.FirstOrDefault().DocumentSplitItemEntities)
                                        {
                                            SplitAccountingFields splitAccountingField = new SplitAccountingFields(splitConfig.Where(k => k.EntityTypeId == s.EntityTypeId).FirstOrDefault());

                                            splitAccountingField.EntityCode = s.EntityCode;
                                            splitAccountingField.EntityDetailCode = !string.IsNullOrEmpty(s.SplitAccountingFieldValue) ? Convert.ToInt64(s.SplitAccountingFieldValue) : 0;
                                            splitAcc.Add(splitAccountingField);
                                        }

                                        ADRSplit adrAplit = new ADRSplit();
                                        adrAplit.Splits = splitAcc;
                                        adrAplit.Identifier = item.DocumentItemId;
                                        lstAdrSplit.Add(adrAplit);
                                    }

                                    if (splits != null && splits.Any() && lstAdrSplit.Any())
                                    {
                                        foreach (var split in splits)
                                        {
                                            bool isSplitsUpdated = false;
                                            adrDocumentSplitItemEntities = objDocBO.GetDocumentDefaultAccountingDetailsForLineItems(P2PDocumentType.Requisition, objRequisition.CreatedBy,
                                                objRequisition.DocumentCode, null,
                                                lstAdrSplit, false, null, objRequisition.EntityDetailCode != null && objRequisition.EntityDetailCode.Count > 0 ? objRequisition.EntityDetailCode.FirstOrDefault() : 0, ADRIdentifier.DocumentItemId);
                                            if (adrDocumentSplitItemEntities != null && adrDocumentSplitItemEntities.Any() &&
                                                adrDocumentSplitItemEntities.FirstOrDefault().Splits != null && adrDocumentSplitItemEntities.FirstOrDefault().Splits.Any())
                                            {
                                                foreach (var documentSplitItemEntity in adrDocumentSplitItemEntities.FirstOrDefault().Splits)
                                                {
                                                    if (!string.IsNullOrEmpty(documentSplitItemEntity.EntityCode) && documentSplitItemEntity.EntityDetailCode > 0 && documentSplitItemEntity.Title != "Requester")
                                                    {
                                                        isSplitsUpdated = true;
                                                        var matchingEntities = split.DocumentSplitItemEntities.Where(entity => entity.EntityTypeId == documentSplitItemEntity.EntityTypeId);
                                                        if (matchingEntities != null && matchingEntities.Any())
                                                        {
                                                            matchingEntities.FirstOrDefault().EntityCode = documentSplitItemEntity.EntityCode;
                                                            matchingEntities.FirstOrDefault().SplitAccountingFieldValue = Convert.ToString(documentSplitItemEntity.EntityDetailCode);
                                                            matchingEntities.FirstOrDefault().DocumentSplitItemId = split.DocumentSplitItemId;
                                                        }
                                                    }
                                                }
                                            }
                                            if (isSplitsUpdated)
                                                reqManager.SaveRequisitionAccountingDetails(splits, split.DocumentSplitItemEntities, item.Quantity, precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges, useTaxMaster);
                                        }
                                    }
                                }
                            }
                            #endregion

                            #region Save Default Split Accounting
                            if (setDefaultSplit)
                                GetReqDao().SaveDefaultAccountingDetails(objRequisition.DocumentCode, defaultAccDetails, precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges, true);
                            #endregion

                            #region Save Contract Number

                            if (!string.IsNullOrEmpty(item.ExtContractRef))
                            {
                                reqManager.SaveContractInformation(item.DocumentItemId, item.ExtContractRef);
                            }

                            #endregion Save Contract Number

                            #region Save Additional Field Details

                            SaveAdditionalAttributeFieldFromInterface(objRequisition.DocumentCode, objRequisition.PurchaseTypeDescription, item);

                            #endregion

                        }
                    }
                    #endregion 'saving line items'

                    //if (CallGetLineItemsAPIOnRequisition == 1)
                    GetReqInterfaceDao().SaveRequisitionItemAdditionalDetailsFromInterface(objRequisition.RequisitionItems,SpendControlType);

                    GetReqDao().InsertUpdateLineitemTaxes(objRequisition);

                    #region Saving Catalog Item Accounting Details
                    var catalogSettings = objCommon.GetSettingsFromSettingsComponent(P2PDocumentType.Catalog, UserContext.ContactCode, (int)SubAppCodes.P2P, "", lobEntityDetailCode);
                    settingValue = objCommon.GetSettingsValueByKey(catalogSettings, "AllowOrgEntityInCatalogItems");
                    var allowOrgEntityFromCatalog = string.IsNullOrEmpty(settingValue) ? string.Empty : Convert.ToBoolean(settingValue).ToString().ToLower();

                    settingValue = objCommon.GetSettingsValueByKey(catalogSettings, "CorporationEntityId");
                    var corporationEntityId = string.IsNullOrEmpty(settingValue) ? 0 : Convert.ToInt32(settingValue);

                    settingValue = objCommon.GetSettingsValueByKey(catalogSettings, "ExpenseCodeEntityId");
                    var expenseCodeEntityId = string.IsNullOrEmpty(settingValue) ? 0 : Convert.ToInt32(settingValue);
                    if (allowOrgEntityFromCatalog == "true" && allowOverridingOrgEntityFromCatalog)
                        GetReqDao().UpdateCatalogOrgEntitiesToRequisition(objRequisition.DocumentCode, corporationEntityId, expenseCodeEntityId);
                    #endregion

                    #region Prorate Tax
                    if (((Convert.ToDecimal(objRequisition.Tax) > 0 && !useTaxMaster)
                        || Convert.ToDecimal(objRequisition.Shipping) > 0
                        || Convert.ToDecimal(objRequisition.AdditionalCharges) > 0))
                        reqManager.ProrateHeaderTaxAndShipping(objRequisition);
                    #endregion

                    #region Custom Attribute Derivation if setting (IsDeriveItemDetailEnable) is ON
                    if (IsDeriveItemDetailEnable == true)
                    {
                        List<string> lstCustomAttributeQuestionText = new List<string>();

                        foreach (var item in objRequisition.RequisitionItems)
                        {
                            if (item.CustomAttributes == null)
                            {
                                if (objdt.Rows != null && objdt.Rows.Count > 0)
                                {
                                    foreach (DataRow dt in objdt.Rows)
                                    {
                                        if (dt.Table.Columns.Contains("ItemNumber") && !string.IsNullOrEmpty(dt["ItemNumber"].ToString()))
                                        {
                                            if (item.ItemNumber == dt["ItemNumber"].ToString())
                                            {
                                                if (dt.Table.Columns.Contains("CatalogItemID") && dt.Table.Columns.Contains("ItemMasterItemId") && dt.Table.Columns.Contains("tardocumentType"))
                                                {
                                                    if (!string.IsNullOrEmpty(dt["CatalogItemID"].ToString()) && !string.IsNullOrEmpty(dt["ItemMasterItemId"].ToString()) && !string.IsNullOrEmpty(dt["tardocumentType"].ToString()))
                                                        objDocBO.SaveFlippedQuestionResponses(Convert.ToInt64(dt["CatalogItemID"].ToString()), Convert.ToInt64(dt["ItemMasterItemId"].ToString()), item.DocumentItemId, Convert.ToInt32(dt["tardocumentType"].ToString()), lstCustomAttributeQuestionText);
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            else if (item.CustomAttributes != null)
                            {
                                foreach (Questionnaire ques in item.CustomAttributes)
                                {
                                    lstCustomAttributeQuestionText.Add(ques.QuestionnaireTitle);
                                }
                                if (objdt.Rows != null && objdt.Rows.Count > 0)
                                {
                                    foreach (DataRow dt in objdt.Rows)
                                    {
                                        if (dt.Table.Columns.Contains("ItemNumber") && !string.IsNullOrEmpty(dt["ItemNumber"].ToString()))
                                        {
                                            if (item.ItemNumber == dt["ItemNumber"].ToString())
                                            {
                                                if (dt.Table.Columns.Contains("CatalogItemID") && dt.Table.Columns.Contains("ItemMasterItemId") && dt.Table.Columns.Contains("tardocumentType"))
                                                {
                                                    if (!string.IsNullOrEmpty(dt["CatalogItemID"].ToString()) && !string.IsNullOrEmpty(dt["ItemMasterItemId"].ToString()) && !string.IsNullOrEmpty(dt["tardocumentType"].ToString()))
                                                        objDocBO.SaveFlippedQuestionResponses(Convert.ToInt64(dt["CatalogItemID"].ToString()), Convert.ToInt64(dt["ItemMasterItemId"].ToString()), item.DocumentItemId, Convert.ToInt32(dt["tardocumentType"].ToString()), lstCustomAttributeQuestionText);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    #endregion

                    #region Custom Attributes                                      
                    lstCustomAttrFormId = objCommon.GetCustomAttrFormId((int)DocumentType.Requisition, new List<Level> { Level.Header, Level.Item, Level.Distribution }, 0, 0, 0, 0, objRequisition.DocumentCode);

                    var splitItemIdDetails = GetCommonDao().GetSplitItemIds(objRequisition.DocumentCode, (int)DocumentType.Requisition);

                    if (lstCustomAttrFormId.Count > 0 && lstCustomAttrFormId.Any())
                    {
                        objRequisition.CustomAttrFormId = lstCustomAttrFormId.Where(custAttr => custAttr.Key == Level.Header).Select(custAttr => custAttr.Value).FirstOrDefault<long>();
                        objRequisition.CustomAttrFormIdForItem = lstCustomAttrFormId.Where(custAttr => custAttr.Key == Level.Item).Select(custAttr => custAttr.Value).FirstOrDefault<long>();
                        objRequisition.CustomAttrFormIdForSplit = lstCustomAttrFormId.Where(custAttr => custAttr.Key == Level.Distribution).Select(custAttr => custAttr.Value).FirstOrDefault<long>();

                        // Custom Attributes -- Get Question Set for Header, Item and Split
                        if (objRequisition.CustomAttrFormId > 0)
                            headerQuestionSet = GetCommonDao().GetQuestionSetByFormCode(objRequisition.CustomAttrFormId);

                        if (objRequisition.CustomAttrFormIdForItem > 0)
                            itemQuestionSet = GetCommonDao().GetQuestionSetByFormCode(objRequisition.CustomAttrFormIdForItem);

                        if (objRequisition.CustomAttrFormIdForSplit > 0)
                            splitQuestionSet = GetCommonDao().GetQuestionSetByFormCode(objRequisition.CustomAttrFormIdForSplit);


                        // Custom Attributes -- Fill Questions Response List -- for Header
                        if (headerQuestionSet != null && headerQuestionSet.Any() && objRequisition.CustomAttributes != null && objRequisition.CustomAttributes.Any())
                            objCommon.FillQuestionsResponseList(lstQuestionsResponse, objRequisition.CustomAttributes, headerQuestionSet.Select(question => question.QuestionSetCode).ToList(), objRequisition.DocumentCode);

                        //  Custom Attributes -- Fill Questions Response List -- for Item
                        if (objRequisition.RequisitionItems != null && objRequisition.RequisitionItems.Count > 0 && itemQuestionSet != null && itemQuestionSet.Any())
                        {
                            foreach (RequisitionItem item in objRequisition.RequisitionItems)
                            {
                                if (item.CustomAttributes != null && item.CustomAttributes.Any())
                                    objCommon.FillQuestionsResponseList(lstQuestionsResponse, item.CustomAttributes, itemQuestionSet.Select(question => question.QuestionSetCode).ToList(), item.DocumentItemId);

                                //  Custom Attributes -- Fill Questions Response List -- For Split
                                var splitItemIdList = splitItemIdDetails.Where(splitItem => splitItem.Value == item.DocumentItemId).Select(splitItem => splitItem.Key).ToList<long>();

                                if (item.ItemSplitsDetail != null && item.ItemSplitsDetail.Count > 0 && splitItemIdList != null && splitItemIdList.Any())
                                {
                                    int splitCount = 0;

                                    foreach (RequisitionSplitItems itmSplt in item.ItemSplitsDetail)
                                    {
                                        if (splitCount < splitItemIdList.Count)
                                            itmSplt.DocumentSplitItemId = splitItemIdList[splitCount++];
                                        else
                                            break;

                                        if (itmSplt.CustomAttributes != null)
                                            objCommon.FillQuestionsResponseList(lstQuestionsResponse, itmSplt.CustomAttributes, splitQuestionSet.Select(question => question.QuestionSetCode).ToList(), itmSplt.DocumentSplitItemId);
                                    }
                                }
                            }
                        }

                        // Custom Attributes -- Save Questions Response List -- For Header, Itemm and Split
                        if (lstQuestionsResponse != null && lstQuestionsResponse.Any())
                        {
                            var requestHeaders = new GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.RequestHeaders();
                            requestHeaders.Set(this.UserContext, this.JWTToken);
                            string appName = Environment.GetEnvironmentVariable("NewRelic.AppName") ?? "Requisition"; 
                            string useCase = "RequisitionInterfaceManager-SaveRequisitionFromInterface";
                            var serviceURL = MultiRegionConfig.GetConfig(CloudConfig.APIMBaseURL) + GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.ServiceURLs.QuestionBankServiceURL + "savequestionresponselist";

                            var saveQuestionResponseListRequest = new Dictionary<string, object>
                            {
                                {"QuestionResponseList", lstQuestionsResponse}
                            };

                            var webAPI = new GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.WebAPI(requestHeaders, appName, useCase);
                            webAPI.ExecutePost(serviceURL, saveQuestionResponseListRequest);
                        }
                    }
                    #endregion

                    #region Save Header Level Sub line Requisition

                    if (objRequisition.lstItemCharge != null && objRequisition.lstItemCharge.Any())
                    {
                        List<ChargeMaster> lstChargeMasterDetail = new List<ChargeMaster>();
                        List<ItemCharge> lstHeaderItemCharge = new List<ItemCharge>();
                        lstChargeMasterDetail = objCommon.GetAllChargeName(lobEntityDetailCode);

                        if (lstChargeMasterDetail != null && lstChargeMasterDetail.Any())
                        {
                            foreach (var item in objRequisition.lstItemCharge)
                            {
                                if (item.ChargeDetails != null)
                                {
                                    /*Filter ChangeName and Assign the values : ChargeMasterID and ChargeName*/
                                    var _lstChargeMaster = lstChargeMasterDetail.Where(_ChargeName => _ChargeName.ChargeName.Equals(item.ChargeDetails.ChargeName, StringComparison.InvariantCultureIgnoreCase));
                                    if (_lstChargeMaster != null && _lstChargeMaster.Count() == 1)
                                    {
                                        item.ChargeDetails.ChargeMasterId = _lstChargeMaster.First().ChargeMasterId;
                                        item.ChargeDetails.ChargeName = _lstChargeMaster.First().ChargeName;

                                        /*Get ChargeMasterBased on ChargeMasterId*/
                                        var objChargeMaster = objCommon.GetChargeMasterDetailsByChargeMasterId(item.ChargeDetails.ChargeMasterId);
                                        if (objChargeMaster != null)
                                        {
                                            item.ChargeDetails.ChargeMasterId = objChargeMaster.ChargeMasterId;
                                            item.ChargeDetails.ChargeName = objChargeMaster.ChargeName;
                                            item.ChargeDetails.ChargeDescription = objChargeMaster.ChargeDescription;
                                            item.ChargeDetails.ChargeTypeCode = objChargeMaster.ChargeTypeCode;
                                            item.ChargeDetails.CalculationBasisId = objChargeMaster.CalculationBasisId;
                                            item.ChargeDetails.TolerancePercentage = objChargeMaster.TolerancePercentage;
                                            item.ChargeDetails.ChargeTypeName = objChargeMaster.ChargeTypeName;

                                            item.DocumentCode = objRequisition.DocumentCode;//'RequisitionID
                                            item.ItemTypeID = (int)ItemType.Charge;
                                            item.CalculationValue = 0;//For Header Sub line Charges                        
                                            item.IsHeaderLevelCharge = true;/* Since It's a Header Level*/
                                            item.CreatedBy = objRequisition.CreatedBy;
                                        }
                                        lstHeaderItemCharge.Add(item);
                                    }
                                }
                            }
                            objCommon.SaveReqAllItemCharges(lstHeaderItemCharge); //Save All Requisition Header Charge Data
                        }
                    }
                    #endregion

                    GetReqDao().CalculateAndUpdateSplitDetails(objRequisition.DocumentCode);//INDEXING NOT NEEDED

                    GetReqInterfaceDao().SaveRequisitionAdditionalDetailsFromInterface(objRequisition.DocumentCode);

                    /* Reset to CXML Status*/

                    objRequisition.DocumentStatusInfo = (Documents.Entities.DocumentStatus)_tempDocumentStatus;

                    #region Update Document Status

                    if (objRequisition.DocumentStatusInfo == Documents.Entities.DocumentStatus.Withdrawn)
                    {
                        #region WithDraw Document
                        ProxyWorkFlowRestService objProxyWorkFlowRestService = new ProxyWorkFlowRestService(this.UserContext, this.JWTToken);
                        objProxyWorkFlowRestService.WithDrawDocumentByDocumentCode((int)DocumentType.Requisition, objRequisition.DocumentCode, objRequisition.CreatedBy);
                        #endregion
                    }
                    else if (objRequisition.DocumentStatusInfo == Documents.Entities.DocumentStatus.ApprovalPending)
                    {

                        reqManager.SentRequisitionForApproval(objRequisition.CreatedBy, objRequisition.DocumentCode, ReqTotal_, (int)P2PDocumentType.Requisition, objRequisition.Currency, string.Empty, isOperationalBudgetEnabled, headerOrgEntityDetailCode);
                        //objDocBO.UpdateDocumentStatus(P2PDocumentType.Requisition, objRequisition.DocumentCode, Documents.Entities.DocumentStatus.ApprovalPending, (decimal)0, false, POTransmissionMode.None, "", "REQ", objRequisition.SourceSystemInfo.SourceSystemId);
                    }
                    else if (objRequisition.DocumentStatusInfo == Documents.Entities.DocumentStatus.Approved)
                    {
                        objDocBO.UpdateDocumentStatus(P2PDocumentType.Requisition, objRequisition.DocumentCode, Documents.Entities.DocumentStatus.Approved, 0, false, POTransmissionMode.None, "", "REQ", objRequisition.SourceSystemInfo.SourceSystemId);

                        #region Consolidating designated approved REQs into one or multiple purchase order based on key entities(Purchase Type)
                        reqManager.PushingDataToEventHub(objRequisition.DocumentCode);
                        #endregion Consolidating designated approved REQs into one or multiple purchase order based on key entities(Purchase Type)

                        #region 'Auto creating  Order'    

                        settingValue = objCommon.GetSettingsValueByKey(p2pSettings, "IsAutoSourcing");
                        bool isAutoSourceON = !string.IsNullOrEmpty(settingValue) ? Convert.ToBoolean(settingValue) : false;

                        var requestHeaders = new GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.RequestHeaders();
                        requestHeaders.Set(this.UserContext, this.JWTToken);
                        string appName = Environment.GetEnvironmentVariable("NewRelic.AppName") ?? "Requisition"; 
                        string useCase = "RequisitionInterfaceManager-UpdateRequistionDetailsFromInterface";

                        if (isAutoSourceON)
                        {
                            LogHelper.LogInfo(Log, "AutoCreateWorkBenchOrder Method Started.Document id:" + objRequisition.DocumentCode.ToString());

                            var serviceURL = MultiRegionConfig.GetConfig(CloudConfig.APIMBaseURL) + GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.ServiceURLs.OrderServiceURL + "AutoCreateWorkBenchOrder";
                            var webAPI = new GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.WebAPI(requestHeaders, appName, useCase);
                            var jsonRequest = "{\"preDocumentId\":\""+ objRequisition.DocumentCode + "\",\"processFlag\":2,\"isautosubmit\":0}";
                            var apiResult = webAPI.ExecutePost(serviceURL, jsonRequest);

                            //objOrderManager.AutoCreateWorkBenchOrder(objRequisition.DocumentCode,2,false);
                            //objproxyOrder.AutoCreateWorkBenchOrder(objRequisition.DocumentCode, 2, false);
                            LogHelper.LogInfo(Log, "AutoCreateWorkBenchOrder Method Ended.Document id: " + objRequisition.DocumentCode.ToString());
                        }
                        else
                        {
                            List<long> lstDocumentCode = new List<long>();
                            //lstDocumentCode = objOrderManager.GetSettingsAndAutoCreateOrder(objRequisition.DocumentCode);
                            var serviceURL = MultiRegionConfig.GetConfig(CloudConfig.APIMBaseURL) + GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.ServiceURLs.OrderServiceURL + "GetSettingsAndAutoCreateOrder?preDocumentId=" + objRequisition.DocumentCode;
                            var webAPI = new GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.WebAPI(requestHeaders, appName, useCase);
                            var apiResult = webAPI.ExecuteGet(serviceURL);
                            var apiresponse = JsonConvert.DeserializeObject<List<long>>(apiResult);
                            if (apiresponse != null && apiresponse.Any())
                            {
                                lstDocumentCode = apiresponse;
                            }
                            //lstDocumentCode = objproxyOrder.GetSettingsAndAutoCreateOrder(objRequisition.DocumentCode);
                            if (lstDocumentCode != null && lstDocumentCode.Any())
                            {
                                AddIntoSearchIndexerQueueing(lstDocumentCode, (int)DocumentType.PO);
                            }
                        }
                        #endregion 'Auto creating  Order'

                    }
                    else if (objRequisition.DocumentStatusInfo == Documents.Entities.DocumentStatus.ReviewPending)
                    {
                        objDocBO.UpdateDocumentStatus(P2PDocumentType.Requisition, objRequisition.DocumentCode, Documents.Entities.DocumentStatus.ReviewPending, 0, false, POTransmissionMode.None, "", "REQ", objRequisition.SourceSystemInfo.SourceSystemId);

                        Dictionary<string, string> resultObj = new Dictionary<string, string>();
                        resultObj = objDocBO.SendDocumentForReview(objRequisition.DocumentCode, objRequisition.RequesterId, (int)DocumentType.Requisition);

                        if (resultObj != null && resultObj.ContainsKey("SendForReviewResult") && resultObj["SendForReviewResult"] == "NoReviewSetup")
                        {
                            reqManager.SentRequisitionForApproval(objRequisition.CreatedBy, objRequisition.DocumentCode, ReqTotal_, (int)P2PDocumentType.Requisition, objRequisition.Currency, string.Empty, isOperationalBudgetEnabled, headerOrgEntityDetailCode);
                        }
                    }


                    #endregion
                }

                dtResult.Dispose();

                LogHelper.LogInfo(Log, "SaveRequisitionFromInterface Method Ended.");

                AddIntoSearchIndexerQueueing(objRequisition.DocumentCode, (int)DocumentType.Requisition);

                return objRequisition.DocumentCode;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured while SaveRequisitionFromInterface DocumentNumber " + objRequisition.DocumentNumber, ex);
                if (objRequisition != null && objRequisition.DocumentCode > 0)
                {
                    if (ex.Message.StartsWith("Validation", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (objDocBO != null)
                            objDocBO.DeleteDocumentByDocumentCode(P2PDocumentType.Requisition, objRequisition.DocumentCode);
                    }
                }
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw();
                throw;
            }
        }

        private void SetSourceTypeForChevron(RequisitionItem item, Requisition objRequisition, bool IsCaptureCatalogApiResponse, bool deriveItemAdditionalFieldsFromCatalog, bool CallWebAPIOnRequisition = false)
        {

            ItemSearchInput searchInput = new ItemSearchInput();

            #region if line item has only BIN
            if (!string.IsNullOrEmpty(item.ItemNumber) && string.IsNullOrEmpty(item.SupplierPartId))
            {
                searchInput.BIN = item.ItemNumber;
                searchInput.ContractNumber = "";    //dont send ContractNumber with BIN
                searchInput.SupplierCode = 0;       //dont send suppliercode with BIN
                SearchResult searchResult = GetLineItemsFromCatalogAPI(objRequisition, item, searchInput, IsCaptureCatalogApiResponse, CallWebAPIOnRequisition);
                if (searchResult != null && searchResult.Items != null && searchResult.Items.Count > 0)
                {
                    item.SourceType = getRequisitionSourceTypes(searchResult.Items[0].CatalogType);
          item.AllowFlexiblePrice = searchResult.Items[0].STD_FlexiblePrice;
                    #region Set custom attributes
          if (item.CustomAttributes == null)
                        item.CustomAttributes = new List<Questionnaire>();

                    if (searchResult.Items[0].CustomAttributes != null)
                    {
                        var qaCodes = searchResult.Items[0].CustomAttributes.Where(f => f.QuestionText == "QA Codes").ToList();
                        var poText = searchResult.Items[0].CustomAttributes.Where(f => f.QuestionText == "PO Text").ToList(); ;

                        if (qaCodes != null && qaCodes.Count > 0)
                        {
                            var question = qaCodes.FirstOrDefault();

                            if (item.CustomAttributes.Exists(f => f.QuestionnaireTitle == "QA Codes"))
                            {
                                foreach (var customAttribute in item.CustomAttributes)
                                {
                                    if (customAttribute.QuestionnaireTitle == "QA Codes")
                                    {
                                        if (customAttribute.QuestionnaireResponseValues != null && customAttribute.QuestionnaireResponseValues.Count > 0)
                                            customAttribute.QuestionnaireResponseValues[0].ResponseValue = question.QuestionResponse;
                                        else
                                            customAttribute.QuestionnaireResponseValues.Add(new QuestionnaireResponse() { ResponseValue = question.QuestionResponse });
                                    }
                                }
                            }
                            else
                            {
                                List<QuestionnaireResponse> questionResponse = new List<QuestionnaireResponse>();
                                questionResponse.Add(new QuestionnaireResponse() { QuestionId = question.QuestionId, ResponseValue = question.QuestionResponse });
                                Questionnaire newQuestion = new Questionnaire()
                                {
                                    QuestionnaireTitle = question.QuestionText,
                                    QuestionnaireResponseValues = questionResponse
                                };
                                item.CustomAttributes.Add(newQuestion);
                            }
                        }

                        if (poText != null && poText.Count > 0)
                        {
                            var question = poText.FirstOrDefault();

                            if (item.CustomAttributes.Exists(f => f.QuestionnaireTitle == "PO Text"))
                            {
                                foreach (var customAttribute in item.CustomAttributes)
                                {
                                    if (customAttribute.QuestionnaireTitle == "PO Text")
                                    {
                                        if (customAttribute.QuestionnaireResponseValues != null && customAttribute.QuestionnaireResponseValues.Count > 0)
                                            customAttribute.QuestionnaireResponseValues[0].ResponseValue = question.QuestionResponse;
                                        else
                                            customAttribute.QuestionnaireResponseValues.Add(new QuestionnaireResponse() { ResponseValue = question.QuestionResponse });
                                    }
                                }
                            }
                            else
                            {
                                List<QuestionnaireResponse> questionResponse = new List<QuestionnaireResponse>();
                                questionResponse.Add(new QuestionnaireResponse() { QuestionId = question.QuestionId, ResponseValue = question.QuestionResponse });
                                Questionnaire newQuestion = new Questionnaire()
                                {
                                    QuestionnaireTitle = question.QuestionText,
                                    QuestionnaireResponseValues = questionResponse
                                };
                                item.CustomAttributes.Add(newQuestion);
                            }
                        }
                    }
                    #endregion Set custom attributes

                    if (item.lstAdditionalFieldAttributues == null)
                        item.lstAdditionalFieldAttributues = new List<BusinessEntities.P2PAdditionalFieldAtrribute>();

                    SetAdditionalFieldAttributeFromCatalog(item.lstAdditionalFieldAttributues, searchResult, deriveItemAdditionalFieldsFromCatalog);
                }
            }
            #endregion

            #region if line item has only SIN
            else if (string.IsNullOrEmpty(item.ItemNumber) && !string.IsNullOrEmpty(item.SupplierPartId) && item.PartnerCode > 0) //and partnercode>0 call only when partnercode>0
            {
                searchInput.SIN = item.SupplierPartId;
                searchInput.ContractNumber = item.ExtContractRef;
                searchInput.SupplierCode = Convert.ToInt64(item.PartnerCode);
                SearchResult searchResult = GetLineItemsFromCatalogAPI(objRequisition, item, searchInput, IsCaptureCatalogApiResponse, CallWebAPIOnRequisition);
                if (searchResult != null && searchResult.Items != null && searchResult.Items.Count > 0)
                {
                    if ((searchResult.Items[0].EffectivePrice != null && searchResult.Items[0].EffectivePrice > 0) || (searchResult.Items[0].UnitPrice != null && searchResult.Items[0].UnitPrice > 0))
                    {
                        if (!searchResult.Items[0].STD_FlexiblePrice)
                        {
                            if (searchResult.Items[0].EffectivePrice != null && searchResult.Items[0].EffectivePrice > 0)
                                item.UnitPrice = searchResult.Items[0].EffectivePrice;
                            else
                                item.UnitPrice = searchResult.Items[0].UnitPrice;
                        }

                        item.CategoryId = searchResult.Items[0].CategoryId;
                        item.ExtContractRef = searchResult.Items[0].ContractNumber;
                        item.Currency = searchResult.Items[0].Currency;
                        item.UOM = searchResult.Items[0].DefaultUOMCode;
                        item.Description = searchResult.Items[0].Description;
                        item.ManufacturerName = searchResult.Items[0].Manufacturer;
                        item.ManufacturerModel = searchResult.Items[0].ManufacturerModelNumber;
                        item.ManufacturerPartNumber = searchResult.Items[0].ManufacturerPartNumber;
                        item.PartnerCode = searchResult.Items[0].PartnerCode;
                        item.SupplierItemNumber = item.SupplierPartId = searchResult.Items[0].PartnerItemNumber;
                        item.CatalogItemId = searchResult.Items[0].Id;
                        item.SourceType = getRequisitionSourceTypes(searchResult.Items[0].CatalogType);
                        item.AllowFlexiblePrice = searchResult.Items[0].STD_FlexiblePrice;
                    }

                    if (item.lstAdditionalFieldAttributues == null)
                        item.lstAdditionalFieldAttributues = new List<BusinessEntities.P2PAdditionalFieldAtrribute>();

                    SetAdditionalFieldAttributeFromCatalog(item.lstAdditionalFieldAttributues, searchResult, deriveItemAdditionalFieldsFromCatalog);
                }
                //write else if(not null and empty contract number) call API without contract number and overwrite the fields based on response
                else if (!string.IsNullOrEmpty(item.ExtContractRef))
                {
                    searchInput.SIN = item.SupplierPartId;
                    searchInput.ContractNumber = "";
                    searchInput.SupplierCode = Convert.ToInt64(item.PartnerCode);
                    searchResult = GetLineItemsFromCatalogAPI(objRequisition, item, searchInput, IsCaptureCatalogApiResponse, CallWebAPIOnRequisition);
                    if (searchResult != null && searchResult.Items != null && searchResult.Items.Count > 0)
                    {
                        if ((searchResult.Items[0].EffectivePrice != null && searchResult.Items[0].EffectivePrice > 0) || (searchResult.Items[0].UnitPrice != null && searchResult.Items[0].UnitPrice > 0))
                        {
                            if (!searchResult.Items[0].STD_FlexiblePrice)
                            {
                                if (searchResult.Items[0].EffectivePrice != null && searchResult.Items[0].EffectivePrice > 0)
                                    item.UnitPrice = searchResult.Items[0].EffectivePrice;
                                else
                                    item.UnitPrice = searchResult.Items[0].UnitPrice;
                            }

                            item.CategoryId = searchResult.Items[0].CategoryId;
                            item.ExtContractRef = searchResult.Items[0].ContractNumber;
                            item.Currency = searchResult.Items[0].Currency;
                            item.UOM = searchResult.Items[0].DefaultUOMCode;
                            item.Description = searchResult.Items[0].Description;
                            item.ManufacturerName = searchResult.Items[0].Manufacturer;
                            item.ManufacturerModel = searchResult.Items[0].ManufacturerModelNumber;
                            item.ManufacturerPartNumber = searchResult.Items[0].ManufacturerPartNumber;
                            item.PartnerCode = searchResult.Items[0].PartnerCode;
                            item.SupplierItemNumber = item.SupplierPartId = searchResult.Items[0].PartnerItemNumber;
                            item.CatalogItemId = searchResult.Items[0].Id;
                            item.SourceType = getRequisitionSourceTypes(searchResult.Items[0].CatalogType);
                            item.AllowFlexiblePrice = searchResult.Items[0].STD_FlexiblePrice;
            }

                        if (item.lstAdditionalFieldAttributues == null)
                            item.lstAdditionalFieldAttributues = new List<BusinessEntities.P2PAdditionalFieldAtrribute>();

                        SetAdditionalFieldAttributeFromCatalog(item.lstAdditionalFieldAttributues, searchResult, deriveItemAdditionalFieldsFromCatalog);
                    }
                }
            }
            #endregion

            #region if line item has both BIN and SIN
            else if (!string.IsNullOrEmpty(item.ItemNumber) && !string.IsNullOrEmpty(item.SupplierPartId))
            {
                SearchResult searchResult = new SearchResult();
                // if partnercode > 0 call only when partnercode>0
                #region call only when partnercode > 0
                if (item.PartnerCode > 0)
                {
                    // call catalog API with SIN first
                    searchInput.SIN = item.SupplierPartId;
                    searchInput.ContractNumber = item.ExtContractRef;
                    searchInput.SupplierCode = Convert.ToInt64(item.PartnerCode);

                    searchResult = GetLineItemsFromCatalogAPI(objRequisition, item, searchInput, IsCaptureCatalogApiResponse, CallWebAPIOnRequisition);
                    if (searchResult != null && searchResult.Items != null && searchResult.Items.Count > 0)
                    {
                        if ((searchResult.Items[0].EffectivePrice != null && searchResult.Items[0].EffectivePrice > 0) || (searchResult.Items[0].UnitPrice != null && searchResult.Items[0].UnitPrice > 0))
                        {
                            if (!searchResult.Items[0].STD_FlexiblePrice)
                            {
                                if (searchResult.Items[0].EffectivePrice != null && searchResult.Items[0].EffectivePrice > 0)
                                    item.UnitPrice = searchResult.Items[0].EffectivePrice;
                                else
                                    item.UnitPrice = searchResult.Items[0].UnitPrice;
                            }

                            item.CategoryId = searchResult.Items[0].CategoryId;
                            item.ExtContractRef = searchResult.Items[0].ContractNumber;
                            item.Currency = searchResult.Items[0].Currency;
                            item.UOM = searchResult.Items[0].DefaultUOMCode;
                            item.Description = searchResult.Items[0].Description;
                            item.ManufacturerName = searchResult.Items[0].Manufacturer;
                            item.ManufacturerModel = searchResult.Items[0].ManufacturerModelNumber;
                            item.ManufacturerPartNumber = searchResult.Items[0].ManufacturerPartNumber;
                            item.PartnerCode = searchResult.Items[0].PartnerCode;
                            item.SupplierItemNumber = item.SupplierPartId = searchResult.Items[0].PartnerItemNumber;
                            item.CatalogItemId = searchResult.Items[0].Id;
                            item.SourceType = getRequisitionSourceTypes(searchResult.Items[0].CatalogType);
              item.AllowFlexiblePrice = searchResult.Items[0].STD_FlexiblePrice;

              // call catalog API with BIN
              searchInput.BIN = item.ItemNumber;
                            searchInput.SIN = "";
                            searchInput.ContractNumber = "";
                            searchInput.SupplierCode = 0;   //dont send suppliercode with BIN
                            searchResult = GetLineItemsFromCatalogAPI(objRequisition, item, searchInput, IsCaptureCatalogApiResponse, CallWebAPIOnRequisition);
                            if (searchResult != null && searchResult.Items != null && searchResult.Items.Count > 0)
                            {
                                #region Set custom attributes
                                if (item.CustomAttributes == null)
                                    item.CustomAttributes = new List<Questionnaire>();

                                if (searchResult.Items[0].CustomAttributes != null)
                                {
                                    var qaCodes = searchResult.Items[0].CustomAttributes.Where(f => f.QuestionText == "QA Codes").ToList();
                                    var poText = searchResult.Items[0].CustomAttributes.Where(f => f.QuestionText == "PO Text").ToList(); ;

                                    if (qaCodes != null && qaCodes.Count > 0)
                                    {
                                        var question = qaCodes.FirstOrDefault();

                                        if (item.CustomAttributes.Exists(f => f.QuestionnaireTitle == "QA Codes"))
                                        {
                                            foreach (var customAttribute in item.CustomAttributes)
                                            {
                                                if (customAttribute.QuestionnaireTitle == "QA Codes")
                                                {
                                                    if (customAttribute.QuestionnaireResponseValues != null && customAttribute.QuestionnaireResponseValues.Count > 0)
                                                        customAttribute.QuestionnaireResponseValues[0].ResponseValue = question.QuestionResponse;
                                                    else
                                                        customAttribute.QuestionnaireResponseValues.Add(new QuestionnaireResponse() { ResponseValue = question.QuestionResponse });
                                                }
                                            }
                                        }
                                        else
                                        {
                                            List<QuestionnaireResponse> questionResponse = new List<QuestionnaireResponse>();
                                            questionResponse.Add(new QuestionnaireResponse() { QuestionId = question.QuestionId, ResponseValue = question.QuestionResponse });
                                            Questionnaire newQuestion = new Questionnaire()
                                            {
                                                QuestionnaireTitle = question.QuestionText,
                                                QuestionnaireResponseValues = questionResponse
                                            };
                                            item.CustomAttributes.Add(newQuestion);
                                        }
                                    }

                                    if (poText != null && poText.Count > 0)
                                    {
                                        var question = poText.FirstOrDefault();

                                        if (item.CustomAttributes.Exists(f => f.QuestionnaireTitle == "PO Text"))
                                        {
                                            foreach (var customAttribute in item.CustomAttributes)
                                            {
                                                if (customAttribute.QuestionnaireTitle == "PO Text")
                                                {
                                                    if (customAttribute.QuestionnaireResponseValues != null && customAttribute.QuestionnaireResponseValues.Count > 0)
                                                        customAttribute.QuestionnaireResponseValues[0].ResponseValue = question.QuestionResponse;
                                                    else
                                                        customAttribute.QuestionnaireResponseValues.Add(new QuestionnaireResponse() { ResponseValue = question.QuestionResponse });
                                                }
                                            }
                                        }
                                        else
                                        {
                                            List<QuestionnaireResponse> questionResponse = new List<QuestionnaireResponse>();
                                            questionResponse.Add(new QuestionnaireResponse() { QuestionId = question.QuestionId, ResponseValue = question.QuestionResponse });
                                            Questionnaire newQuestion = new Questionnaire()
                                            {
                                                QuestionnaireTitle = question.QuestionText,
                                                QuestionnaireResponseValues = questionResponse
                                            };
                                            item.CustomAttributes.Add(newQuestion);
                                        }
                                    }
                                }
                                #endregion Set custom attributes
                            }
                        }

                        if (item.lstAdditionalFieldAttributues == null)
                            item.lstAdditionalFieldAttributues = new List<BusinessEntities.P2PAdditionalFieldAtrribute>();

                        SetAdditionalFieldAttributeFromCatalog(item.lstAdditionalFieldAttributues, searchResult, deriveItemAdditionalFieldsFromCatalog);
                    }
                    //add else if here to call API with SIN without contract number only when contractnumber is not blank
                    else if (!string.IsNullOrEmpty(item.ExtContractRef))
                    {
                        searchInput.SIN = item.SupplierPartId;
                        searchInput.ContractNumber = "";
                        searchInput.SupplierCode = Convert.ToInt64(item.PartnerCode);

                        searchResult = GetLineItemsFromCatalogAPI(objRequisition, item, searchInput, IsCaptureCatalogApiResponse, CallWebAPIOnRequisition);
                        if (searchResult != null && searchResult.Items != null && searchResult.Items.Count > 0)
                        {
                            if ((searchResult.Items[0].EffectivePrice != null && searchResult.Items[0].EffectivePrice > 0) || (searchResult.Items[0].UnitPrice != null && searchResult.Items[0].UnitPrice > 0))
                            {
                                if (!searchResult.Items[0].STD_FlexiblePrice)
                                {
                                    if (searchResult.Items[0].EffectivePrice != null && searchResult.Items[0].EffectivePrice > 0)
                                        item.UnitPrice = searchResult.Items[0].EffectivePrice;
                                    else
                                        item.UnitPrice = searchResult.Items[0].UnitPrice;
                                }

                                item.CategoryId = searchResult.Items[0].CategoryId;
                                item.ExtContractRef = searchResult.Items[0].ContractNumber;
                                item.Currency = searchResult.Items[0].Currency;
                                item.UOM = searchResult.Items[0].DefaultUOMCode;
                                item.Description = searchResult.Items[0].Description;
                                item.ManufacturerName = searchResult.Items[0].Manufacturer;
                                item.ManufacturerModel = searchResult.Items[0].ManufacturerModelNumber;
                                item.ManufacturerPartNumber = searchResult.Items[0].ManufacturerPartNumber;
                                item.PartnerCode = searchResult.Items[0].PartnerCode;
                                item.SupplierItemNumber = item.SupplierPartId = searchResult.Items[0].PartnerItemNumber;
                                item.CatalogItemId = searchResult.Items[0].Id;
                                item.SourceType = getRequisitionSourceTypes(searchResult.Items[0].CatalogType);
                item.AllowFlexiblePrice = searchResult.Items[0].STD_FlexiblePrice;

                // call catalog API with BIN
                searchInput.BIN = item.ItemNumber;
                                searchInput.SIN = "";
                                searchInput.ContractNumber = "";
                                searchInput.SupplierCode = 0;   //dont send suppliercode with BIN
                                searchResult = GetLineItemsFromCatalogAPI(objRequisition, item, searchInput, IsCaptureCatalogApiResponse, CallWebAPIOnRequisition);
                                if (searchResult != null && searchResult.Items != null && searchResult.Items.Count > 0)
                                {
                                    #region Set custom attributes
                                    if (item.CustomAttributes == null)
                                        item.CustomAttributes = new List<Questionnaire>();

                                    if (searchResult.Items[0].CustomAttributes != null)
                                    {
                                        var qaCodes = searchResult.Items[0].CustomAttributes.Where(f => f.QuestionText == "QA Codes").ToList();
                                        var poText = searchResult.Items[0].CustomAttributes.Where(f => f.QuestionText == "PO Text").ToList(); ;

                                        if (qaCodes != null && qaCodes.Count > 0)
                                        {
                                            var question = qaCodes.FirstOrDefault();

                                            if (item.CustomAttributes.Exists(f => f.QuestionnaireTitle == "QA Codes"))
                                            {
                                                foreach (var customAttribute in item.CustomAttributes)
                                                {
                                                    if (customAttribute.QuestionnaireTitle == "QA Codes")
                                                    {
                                                        if (customAttribute.QuestionnaireResponseValues != null && customAttribute.QuestionnaireResponseValues.Count > 0)
                                                            customAttribute.QuestionnaireResponseValues[0].ResponseValue = question.QuestionResponse;
                                                        else
                                                            customAttribute.QuestionnaireResponseValues.Add(new QuestionnaireResponse() { ResponseValue = question.QuestionResponse });
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                List<QuestionnaireResponse> questionResponse = new List<QuestionnaireResponse>();
                                                questionResponse.Add(new QuestionnaireResponse() { QuestionId = question.QuestionId, ResponseValue = question.QuestionResponse });
                                                Questionnaire newQuestion = new Questionnaire()
                                                {
                                                    QuestionnaireTitle = question.QuestionText,
                                                    QuestionnaireResponseValues = questionResponse
                                                };
                                                item.CustomAttributes.Add(newQuestion);
                                            }
                                        }

                                        if (poText != null && poText.Count > 0)
                                        {
                                            var question = poText.FirstOrDefault();

                                            if (item.CustomAttributes.Exists(f => f.QuestionnaireTitle == "PO Text"))
                                            {
                                                foreach (var customAttribute in item.CustomAttributes)
                                                {
                                                    if (customAttribute.QuestionnaireTitle == "PO Text")
                                                    {
                                                        if (customAttribute.QuestionnaireResponseValues != null && customAttribute.QuestionnaireResponseValues.Count > 0)
                                                            customAttribute.QuestionnaireResponseValues[0].ResponseValue = question.QuestionResponse;
                                                        else
                                                            customAttribute.QuestionnaireResponseValues.Add(new QuestionnaireResponse() { ResponseValue = question.QuestionResponse });
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                List<QuestionnaireResponse> questionResponse = new List<QuestionnaireResponse>();
                                                questionResponse.Add(new QuestionnaireResponse() { QuestionId = question.QuestionId, ResponseValue = question.QuestionResponse });
                                                Questionnaire newQuestion = new Questionnaire()
                                                {
                                                    QuestionnaireTitle = question.QuestionText,
                                                    QuestionnaireResponseValues = questionResponse
                                                };
                                                item.CustomAttributes.Add(newQuestion);
                                            }
                                        }
                                    }
                                    #endregion Set custom attributes
                                }
                            }

                            if (item.lstAdditionalFieldAttributues == null)
                                item.lstAdditionalFieldAttributues = new List<BusinessEntities.P2PAdditionalFieldAtrribute>();

                            SetAdditionalFieldAttributeFromCatalog(item.lstAdditionalFieldAttributues, searchResult, deriveItemAdditionalFieldsFromCatalog);
                        }
                        // when partnercode > 0 but no response from API with and without contract number
                        else
                        {
                            // call catalog API with BIN
                            searchInput.BIN = item.ItemNumber;
                            searchInput.SIN = "";
                            searchInput.ContractNumber = "";
                            searchInput.SupplierCode = 0; //dont send suppliercode for BIN
                            searchResult = GetLineItemsFromCatalogAPI(objRequisition, item, searchInput, IsCaptureCatalogApiResponse, CallWebAPIOnRequisition);
                            if (searchResult != null)
                            {
                                if (searchResult.Items != null && searchResult.Items.Count > 0)
                                {
                                    item.SourceType = getRequisitionSourceTypes(searchResult.Items[0].CatalogType);
                  item.AllowFlexiblePrice = searchResult.Items[0].STD_FlexiblePrice;

                  #region Set custom attributes
                  if (item.CustomAttributes == null)
                                        item.CustomAttributes = new List<Questionnaire>();

                                    if (searchResult.Items[0].CustomAttributes != null)
                                    {
                                        var qaCodes = searchResult.Items[0].CustomAttributes.Where(f => f.QuestionText == "QA Codes").ToList();
                                        var poText = searchResult.Items[0].CustomAttributes.Where(f => f.QuestionText == "PO Text").ToList(); ;

                                        if (qaCodes != null && qaCodes.Count > 0)
                                        {
                                            var question = qaCodes.FirstOrDefault();

                                            if (item.CustomAttributes.Exists(f => f.QuestionnaireTitle == "QA Codes"))
                                            {
                                                foreach (var customAttribute in item.CustomAttributes)
                                                {
                                                    if (customAttribute.QuestionnaireTitle == "QA Codes")
                                                    {
                                                        if (customAttribute.QuestionnaireResponseValues != null && customAttribute.QuestionnaireResponseValues.Count > 0)
                                                            customAttribute.QuestionnaireResponseValues[0].ResponseValue = question.QuestionResponse;
                                                        else
                                                            customAttribute.QuestionnaireResponseValues.Add(new QuestionnaireResponse() { ResponseValue = question.QuestionResponse });
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                List<QuestionnaireResponse> questionResponse = new List<QuestionnaireResponse>();
                                                questionResponse.Add(new QuestionnaireResponse() { QuestionId = question.QuestionId, ResponseValue = question.QuestionResponse });
                                                Questionnaire newQuestion = new Questionnaire()
                                                {
                                                    QuestionnaireTitle = question.QuestionText,
                                                    QuestionnaireResponseValues = questionResponse
                                                };
                                                item.CustomAttributes.Add(newQuestion);
                                            }
                                        }

                                        if (poText != null && poText.Count > 0)
                                        {
                                            var question = poText.FirstOrDefault();

                                            if (item.CustomAttributes.Exists(f => f.QuestionnaireTitle == "PO Text"))
                                            {
                                                foreach (var customAttribute in item.CustomAttributes)
                                                {
                                                    if (customAttribute.QuestionnaireTitle == "PO Text")
                                                    {
                                                        if (customAttribute.QuestionnaireResponseValues != null && customAttribute.QuestionnaireResponseValues.Count > 0)
                                                            customAttribute.QuestionnaireResponseValues[0].ResponseValue = question.QuestionResponse;
                                                        else
                                                            customAttribute.QuestionnaireResponseValues.Add(new QuestionnaireResponse() { ResponseValue = question.QuestionResponse });
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                List<QuestionnaireResponse> questionResponse = new List<QuestionnaireResponse>();
                                                questionResponse.Add(new QuestionnaireResponse() { QuestionId = question.QuestionId, ResponseValue = question.QuestionResponse });
                                                Questionnaire newQuestion = new Questionnaire()
                                                {
                                                    QuestionnaireTitle = question.QuestionText,
                                                    QuestionnaireResponseValues = questionResponse
                                                };
                                                item.CustomAttributes.Add(newQuestion);
                                            }
                                        }
                                    }
                                    #endregion Set custom attributes

                                    if (item.lstAdditionalFieldAttributues == null)
                                        item.lstAdditionalFieldAttributues = new List<BusinessEntities.P2PAdditionalFieldAtrribute>();

                                    SetAdditionalFieldAttributeFromCatalog(item.lstAdditionalFieldAttributues, searchResult, deriveItemAdditionalFieldsFromCatalog);
                                }
                            }
                        }
                    }
                    // if contract number is blank and SIN is invalid
                    #region if contract number is blank and SIN is invalid
                    else
                    {
                        // call catalog API with BIN
                        searchInput.BIN = item.ItemNumber;
                        searchInput.SIN = "";
                        searchInput.ContractNumber = "";
                        searchInput.SupplierCode = 0; //dont send suppliercode for BIN
                        searchResult = GetLineItemsFromCatalogAPI(objRequisition, item, searchInput, IsCaptureCatalogApiResponse, CallWebAPIOnRequisition);
                        if (searchResult != null && searchResult.Items != null && searchResult.Items.Count > 0)
                        {
                            item.SourceType = getRequisitionSourceTypes(searchResult.Items[0].CatalogType);
              item.AllowFlexiblePrice = searchResult.Items[0].STD_FlexiblePrice;

              #region Set custom attributes
              if (item.CustomAttributes == null)
                                item.CustomAttributes = new List<Questionnaire>();

                            if (searchResult.Items[0].CustomAttributes != null)
                            {
                                var qaCodes = searchResult.Items[0].CustomAttributes.Where(f => f.QuestionText == "QA Codes").ToList();
                                var poText = searchResult.Items[0].CustomAttributes.Where(f => f.QuestionText == "PO Text").ToList(); ;

                                if (qaCodes != null && qaCodes.Count > 0)
                                {
                                    var question = qaCodes.FirstOrDefault();

                                    if (item.CustomAttributes.Exists(f => f.QuestionnaireTitle == "QA Codes"))
                                    {
                                        foreach (var customAttribute in item.CustomAttributes)
                                        {
                                            if (customAttribute.QuestionnaireTitle == "QA Codes")
                                            {
                                                if (customAttribute.QuestionnaireResponseValues != null && customAttribute.QuestionnaireResponseValues.Count > 0)
                                                    customAttribute.QuestionnaireResponseValues[0].ResponseValue = question.QuestionResponse;
                                                else
                                                    customAttribute.QuestionnaireResponseValues.Add(new QuestionnaireResponse() { ResponseValue = question.QuestionResponse });
                                            }
                                        }
                                    }
                                    else
                                    {
                                        List<QuestionnaireResponse> questionResponse = new List<QuestionnaireResponse>();
                                        questionResponse.Add(new QuestionnaireResponse() { QuestionId = question.QuestionId, ResponseValue = question.QuestionResponse });
                                        Questionnaire newQuestion = new Questionnaire()
                                        {
                                            QuestionnaireTitle = question.QuestionText,
                                            QuestionnaireResponseValues = questionResponse
                                        };
                                        item.CustomAttributes.Add(newQuestion);
                                    }
                                }

                                if (poText != null && poText.Count > 0)
                                {
                                    var question = poText.FirstOrDefault();

                                    if (item.CustomAttributes.Exists(f => f.QuestionnaireTitle == "PO Text"))
                                    {
                                        foreach (var customAttribute in item.CustomAttributes)
                                        {
                                            if (customAttribute.QuestionnaireTitle == "PO Text")
                                            {
                                                if (customAttribute.QuestionnaireResponseValues != null && customAttribute.QuestionnaireResponseValues.Count > 0)
                                                    customAttribute.QuestionnaireResponseValues[0].ResponseValue = question.QuestionResponse;
                                                else
                                                    customAttribute.QuestionnaireResponseValues.Add(new QuestionnaireResponse() { ResponseValue = question.QuestionResponse });
                                            }
                                        }
                                    }
                                    else
                                    {
                                        List<QuestionnaireResponse> questionResponse = new List<QuestionnaireResponse>();
                                        questionResponse.Add(new QuestionnaireResponse() { QuestionId = question.QuestionId, ResponseValue = question.QuestionResponse });
                                        Questionnaire newQuestion = new Questionnaire()
                                        {
                                            QuestionnaireTitle = question.QuestionText,
                                            QuestionnaireResponseValues = questionResponse
                                        };
                                        item.CustomAttributes.Add(newQuestion);
                                    }
                                }
                            }
                            #endregion Set custom attributes

                            if (item.lstAdditionalFieldAttributues == null)
                                item.lstAdditionalFieldAttributues = new List<BusinessEntities.P2PAdditionalFieldAtrribute>();

                            SetAdditionalFieldAttributeFromCatalog(item.lstAdditionalFieldAttributues, searchResult, deriveItemAdditionalFieldsFromCatalog);
                        }
                    }
                    #endregion
                }
                #endregion
                else
                {
                    // call catalog API with BIN
                    searchInput.BIN = item.ItemNumber;
                    searchInput.SIN = "";
                    searchInput.ContractNumber = "";
                    searchInput.SupplierCode = 0;   //dont send suppliercode for BIN
                    searchResult = GetLineItemsFromCatalogAPI(objRequisition, item, searchInput, IsCaptureCatalogApiResponse, CallWebAPIOnRequisition);
                    if (searchResult != null && searchResult.Items != null && searchResult.Items.Count > 0)
                    {
                        item.SourceType = getRequisitionSourceTypes(searchResult.Items[0].CatalogType);
                        item.AllowFlexiblePrice = searchResult.Items[0].STD_FlexiblePrice;
            #region Set custom attributes
            if (item.CustomAttributes == null)
                            item.CustomAttributes = new List<Questionnaire>();

                        if (searchResult.Items[0].CustomAttributes != null)
                        {
                            var qaCodes = searchResult.Items[0].CustomAttributes.Where(f => f.QuestionText == "QA Codes").ToList();
                            var poText = searchResult.Items[0].CustomAttributes.Where(f => f.QuestionText == "PO Text").ToList(); ;

                            if (qaCodes != null && qaCodes.Count > 0)
                            {
                                var question = qaCodes.FirstOrDefault();

                                if (item.CustomAttributes.Exists(f => f.QuestionnaireTitle == "QA Codes"))
                                {
                                    foreach (var customAttribute in item.CustomAttributes)
                                    {
                                        if (customAttribute.QuestionnaireTitle == "QA Codes")
                                        {
                                            if (customAttribute.QuestionnaireResponseValues != null && customAttribute.QuestionnaireResponseValues.Count > 0)
                                                customAttribute.QuestionnaireResponseValues[0].ResponseValue = question.QuestionResponse;
                                            else
                                                customAttribute.QuestionnaireResponseValues.Add(new QuestionnaireResponse() { ResponseValue = question.QuestionResponse });
                                        }
                                    }
                                }
                                else
                                {
                                    List<QuestionnaireResponse> questionResponse = new List<QuestionnaireResponse>();
                                    questionResponse.Add(new QuestionnaireResponse() { QuestionId = question.QuestionId, ResponseValue = question.QuestionResponse });
                                    Questionnaire newQuestion = new Questionnaire()
                                    {
                                        QuestionnaireTitle = question.QuestionText,
                                        QuestionnaireResponseValues = questionResponse
                                    };
                                    item.CustomAttributes.Add(newQuestion);
                                }
                            }

                            if (poText != null && poText.Count > 0)
                            {
                                var question = poText.FirstOrDefault();

                                if (item.CustomAttributes.Exists(f => f.QuestionnaireTitle == "PO Text"))
                                {
                                    foreach (var customAttribute in item.CustomAttributes)
                                    {
                                        if (customAttribute.QuestionnaireTitle == "PO Text")
                                        {
                                            if (customAttribute.QuestionnaireResponseValues != null && customAttribute.QuestionnaireResponseValues.Count > 0)
                                                customAttribute.QuestionnaireResponseValues[0].ResponseValue = question.QuestionResponse;
                                            else
                                                customAttribute.QuestionnaireResponseValues.Add(new QuestionnaireResponse() { ResponseValue = question.QuestionResponse });
                                        }
                                    }
                                }
                                else
                                {
                                    List<QuestionnaireResponse> questionResponse = new List<QuestionnaireResponse>();
                                    questionResponse.Add(new QuestionnaireResponse() { QuestionId = question.QuestionId, ResponseValue = question.QuestionResponse });
                                    Questionnaire newQuestion = new Questionnaire()
                                    {
                                        QuestionnaireTitle = question.QuestionText,
                                        QuestionnaireResponseValues = questionResponse
                                    };
                                    item.CustomAttributes.Add(newQuestion);
                                }
                            }
                        }
                        #endregion Set custom attributes

                        if (item.lstAdditionalFieldAttributues == null)
                            item.lstAdditionalFieldAttributues = new List<BusinessEntities.P2PAdditionalFieldAtrribute>();

                        SetAdditionalFieldAttributeFromCatalog(item.lstAdditionalFieldAttributues, searchResult, deriveItemAdditionalFieldsFromCatalog);
                    }
                }
            }
            #endregion if line item has both BIN and SIN

            //add API call when SourceType<>Hosted and Contract =blank and partnercode>0 call without BIN SIN and set contract number=contract number from API
            #region Set contract number if blank by passing partner code without BIN and SIN
            if (item.SourceType != ItemSourceType.Hosted && string.IsNullOrEmpty(item.ExtContractRef) && item.PartnerCode > 0)
            {
                searchInput.BIN = "";
                searchInput.SIN = "";
                searchInput.ContractNumber = "";
                searchInput.SupplierCode = Convert.ToInt64(item.PartnerCode);
                SearchResult searchResult = GetLineItemsFromCatalogAPI(objRequisition, item, searchInput, IsCaptureCatalogApiResponse, CallWebAPIOnRequisition);
                if (searchResult != null && searchResult.Items != null && searchResult.Items.Count > 0)
                {
                    item.ExtContractRef = searchResult.Items[0].ContractNumber;
                }
            }
            #endregion Set contract number if blank by passing partner code without BIN and SIN
        }

        private void SetSourceTypeForPetronas(RequisitionItem item, Requisition objRequisition, bool IsCaptureCatalogApiResponse, bool CallWebAPIOnRequisition = false)
        {
            ItemSearchInput searchInput = new ItemSearchInput();

            #region Send all the fields required in catalog API present at Line level Item
            searchInput.SIN = item.SupplierPartId;
            searchInput.ContractNumber = string.IsNullOrEmpty(item.ExtContractRef) ? "" : item.ExtContractRef;
            searchInput.SupplierCode = Convert.ToInt64(item.PartnerCode) == 0 ? 0 : Convert.ToInt64(item.PartnerCode);
            searchInput.BIN = item.ItemNumber;
            searchInput.DocumentDate = objRequisition.CreatedOn;
            searchInput.NeedByDate = item.DateNeeded;
            
            if (objRequisition.BuyingChannel == "BC2")
            {
                searchInput.IsMRP = true;
                searchInput.isCheckContractLimit = true;
                searchInput.isCheckLeadTimeExpiry = true;
            }

            if (objRequisition.BuyingChannel == "BC3")
            {
                if (item.SourceType == ItemSourceType.HostedAndInternal || item.SourceType == ItemSourceType.Hosted)
                {
                    searchInput.IsMRP = false;
                    searchInput.isCheckContractLimit = true;
                    searchInput.isCheckLeadTimeExpiry = true;
                }
            }

            if (objRequisition.BuyingChannel == "BC2" || (objRequisition.BuyingChannel == "BC3" && (item.SourceType == ItemSourceType.HostedAndInternal || item.SourceType == ItemSourceType.Hosted)))
            {
                SearchResult searchResult = GetLineItemsFromCatalogAPI(objRequisition, item, searchInput, IsCaptureCatalogApiResponse, CallWebAPIOnRequisition);
                if (searchResult == null || (searchResult.Items != null && searchResult.Items.Count == 0))
                {
                    searchInput.IsMRP = null;
                    searchInput.SupplierCode = 0;
                    searchInput.isCheckContractLimit = null;
                    searchInput.isCheckLeadTimeExpiry = null;
                    searchInput.DocumentDate = null;
                    searchInput.NeedByDate = null;
                    searchInput.RestrictMultipleItems = true;
                    searchResult = GetLineItemsFromCatalogAPI(objRequisition, item, searchInput, IsCaptureCatalogApiResponse, CallWebAPIOnRequisition);
                    if (searchResult != null && searchResult.Items != null && searchResult.Items.Count == 1)
                    {
                        item.InternalPlantMemo = searchResult.Items[0].InternalPlantMemo;
                        item.Itemspecification = searchResult.Items[0].Specification;
                    }
                }
                else
                {
                    if (searchResult != null && searchResult.Items != null && searchResult.Items.Count == 1)
                    {

                        long orderLocationId = searchResult.Items[0].OrderingLocationID ?? 0;
                        item.OrderLocationId = orderLocationId > 0 ? orderLocationId : item.OrderLocationId;
                        item.ExtContractRef = searchResult.Items[0].ContractNumber;
                        item.IncoTermId = searchResult.Items[0].IncoTermId;
                        item.InternalPlantMemo = searchResult.Items[0].InternalPlantMemo;
                        item.Itemspecification = searchResult.Items[0].Specification;
                        if (searchResult.Items[0].EffectivePrice != null && searchResult.Items[0].EffectivePrice > 0)
                            item.UnitPrice = searchResult.Items[0].EffectivePrice;
                        else
                            item.UnitPrice = searchResult.Items[0].UnitPrice;
                        item.UOM = searchResult.Items[0].DefaultUOMCode;
                        item.SourceType = getRequisitionSourceTypes(searchResult.Items[0].CatalogType);
                        item.IncoTermLocation = searchResult.Items[0].Point;
                        item.IncoTermCode = searchResult.Items[0].IncoTermCode;
                        item.PartnerCode = item.PartnerCode == 0 ? searchResult.Items[0].PartnerCode : item.PartnerCode;
                        item.CatalogItemId = searchResult.Items[0].Id;
                        item.SupplierPartId = searchResult.Items[0].PartnerItemNumber;
                        item.DateNeeded = searchResult.Items[0].NeedByDate;
                    }
                }
            }
            #endregion
        }

        private void SetAdditionalFieldAttributeFromCatalog(List<BusinessEntities.P2PAdditionalFieldAtrribute> additionalFieldAtrributes, SearchResult searchResult, bool deriveItemAdditionalFieldsFromCatalog)
        {
            if (deriveItemAdditionalFieldsFromCatalog && searchResult != null && searchResult.Items != null && searchResult.Items.Count > 0)
            {
                if (searchResult.Items[0].AdditionalFieldAtrributeList != null && searchResult.Items[0].AdditionalFieldAtrributeList.Count > 0)
                {
                    foreach (var item in searchResult.Items[0].AdditionalFieldAtrributeList)
                    {
                        if (!additionalFieldAtrributes.Any(f => f != null && !string.IsNullOrEmpty(f.AdditionalFieldName) && !string.IsNullOrEmpty(item.AdditionalFieldName) && f.AdditionalFieldName.ToLower().Trim() == item.AdditionalFieldName.ToLower().Trim()))
                        {
                            var fieldAttribute = new BusinessEntities.P2PAdditionalFieldAtrribute();
                            fieldAttribute.AdditionalFieldID = item.AdditionalFieldID;
                            fieldAttribute.AdditionalFieldName = item.AdditionalFieldName;
                            if (item.lstP2PAdditionalFieldsValues != null && item.lstP2PAdditionalFieldsValues.Count > 0)
                            {
                                fieldAttribute.AdditionalFieldValue = item.lstP2PAdditionalFieldsValues[0].AdditionalFieldDisplayName;
                                fieldAttribute.AdditionalFieldCode = item.lstP2PAdditionalFieldsValues[0].AdditionalFieldCode;
                            }
                            additionalFieldAtrributes.Add(fieldAttribute);
                        }
                    }
                }
                    if ((searchResult.Items[0].AdditionalFieldAtrributeList != null && searchResult.Items[0].AdditionalFieldAtrributeList.Exists(f => f.AdditionalFieldName == "QA Code"))||(searchResult.Items[0].CustomAttributes != null && searchResult.Items[0].CustomAttributes.Exists(f => f.QuestionText == "QA Codes" ||  f.QuestionText == "QA Code")))
                    {
                        var QaCodeAdditional= searchResult.Items[0].AdditionalFieldAtrributeList.Where(f => f.AdditionalFieldName == "QA Code").ToList().FirstOrDefault();
                       
                        var QaCodeCustomAttribute = searchResult.Items[0].CustomAttributes.Where(f => f.QuestionText == "QA Code" ||  f.QuestionText == "QA Codes").ToList().FirstOrDefault();

                        var PMIFlagFieldAttribute= new BusinessEntities.P2PAdditionalFieldAtrribute();

                               PMIFlagFieldAttribute.AdditionalFieldName = "PMI Flag";

                        if ((QaCodeAdditional!= null && QaCodeAdditional.lstP2PAdditionalFieldsValues != null && QaCodeAdditional.lstP2PAdditionalFieldsValues.Count > 0) || !string.IsNullOrEmpty(QaCodeCustomAttribute.QuestionResponse))

                        {
                            PMIFlagFieldAttribute.AdditionalFieldValue = "Yes";
                        }
                        else
                        {
                            PMIFlagFieldAttribute.AdditionalFieldValue = "No";
                        }
                        additionalFieldAtrributes.Add(PMIFlagFieldAttribute);


                    }
                    else
                    {
                        var PMIFlagFieldAttribute = new BusinessEntities.P2PAdditionalFieldAtrribute();
                        PMIFlagFieldAttribute.AdditionalFieldName = "PMIFlag";
                        PMIFlagFieldAttribute.AdditionalFieldValue = "No";
                        additionalFieldAtrributes.Add(PMIFlagFieldAttribute);
                    }
                //}
            }
        }

        public void SetRequisitionAdditionalEntityFromInterface_New(Requisition objRequisition, out int headerEntityId, out string headerEntityName, string LobEntityCode = "")
        {
            #region Set Document Additional Entities Details
            long lobEntityDetailCode = 0;
            if (objRequisition.EntityDetailCode != null && objRequisition.EntityDetailCode.Any())
                lobEntityDetailCode = objRequisition.EntityDetailCode.FirstOrDefault();
            List<SplitAccountingFields> splitConfiguration = GetSQLP2PDocumentDAO().GetAllSplitAccountingFields(P2PDocumentType.Requisition, LevelType.HeaderLevel, 0, lobEntityDetailCode);
            headerEntityId = 0;
            headerEntityName = string.Empty;
            int i = 0;
            objRequisition.DocumentLOBDetails = new List<DocumentLOBDetails>();
            ProxyOrganizationStructureService proxyOrganizationService = new ProxyOrganizationStructureService(this.UserContext, this.JWTToken);
            foreach (SplitAccountingFields split in splitConfiguration)
            {
                if (split != null)
                {
                    headerEntityId = split.EntityTypeId;
                    headerEntityName = !string.IsNullOrEmpty(split.Title) ? split.Title : string.Empty;
                }
                if (objRequisition.DocumentAdditionalEntitiesInfoList != null && objRequisition.DocumentAdditionalEntitiesInfoList.Any())
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(objRequisition.DocumentAdditionalEntitiesInfoList[i].EntityCode) && !string.IsNullOrEmpty(objRequisition.DocumentAdditionalEntitiesInfoList[i].EntityType))
                        {
                            var entityDetail = proxyOrganizationService.GetOrgEntityByEntityCode(objRequisition.DocumentAdditionalEntitiesInfoList[i].EntityCode, objRequisition.DocumentAdditionalEntitiesInfoList[i].EntityType, LobEntityCode);
                            if (entityDetail != null && entityDetail.OrgEntityCode > 0)
                            {
                                objRequisition.DocumentAdditionalEntitiesInfoList[i].EntityDetailCode = entityDetail.OrgEntityCode;
                                objRequisition.DocumentAdditionalEntitiesInfoList[i].EntityId = entityDetail.objEntityType.EntityId;
                                objRequisition.DocumentAdditionalEntitiesInfoList[i].EntityDisplayName = entityDetail.objEntityType.DisplayName;
                                objRequisition.DocumentLOBDetails.Add(new DocumentLOBDetails
                                {
                                    EntityCode = entityDetail.LOBEntityCode,
                                    EntityDetailCode = entityDetail.LOBEntityDetailCode
                                });
                            }
                        }
                        else if (!string.IsNullOrEmpty(objRequisition.DocumentAdditionalEntitiesInfoList[i].EntityCode))
                        {
                            var entityDetail = proxyOrganizationService.GetOrgEntityByEntityCode(objRequisition.DocumentAdditionalEntitiesInfoList[i].EntityCode, headerEntityName, LobEntityCode);
                            if (entityDetail != null && entityDetail.OrgEntityCode > 0)
                            {
                                objRequisition.DocumentAdditionalEntitiesInfoList[i].EntityDetailCode = entityDetail.OrgEntityCode;
                                objRequisition.DocumentAdditionalEntitiesInfoList[i].EntityId = headerEntityId;
                                objRequisition.DocumentAdditionalEntitiesInfoList[i].EntityDisplayName = headerEntityName;
                                objRequisition.DocumentLOBDetails.Add(new DocumentLOBDetails
                                {
                                    EntityCode = entityDetail.LOBEntityCode,
                                    EntityDetailCode = entityDetail.LOBEntityDetailCode
                                });
                            }
                        }
                        else
                        {
                            objRequisition.DocumentAdditionalEntitiesInfoList[i].EntityDetailCode = 0;
                            objRequisition.DocumentAdditionalEntitiesInfoList[i].EntityId = headerEntityId;
                        }

                    }
                    finally
                    {

                    }
                }
                i++;
            }
            #endregion
        }

        public Int64 SaveReqCartItemsFromInterface(Int64 UserId, Int64 PartnerCode, List<BusinessEntities.P2PItem> lstP2PItem, int precessionValue, int PartnerConfigurationId, string DocumentCode, Int64 PunchoutCartReqId, decimal Tax = 0, decimal Shipping = 0, decimal AdditionalCharges = 0)
        {
            int DocumentLineItemShippingID = 0;
            UserContext.ContactCode = UserId;
            var commonManager = new RequisitionCommonManager(this.JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
            long LOBId = GetCommonDao().GetLOBByDocumentCode(Convert.ToInt64(DocumentCode));
            var p2pSettings = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.None, UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBId);
            int precessionValueDefaultAccounting = convertStringToInt(commonManager.GetSettingsValueByKey(p2pSettings, "MaxPrecessionValue"));
            int maxPrecessionforTotal = convertStringToInt(commonManager.GetSettingsValueByKey(p2pSettings, "MaxPrecessionValueforTotal"));
            int maxPrecessionForTaxesAndCharges = convertStringToInt(commonManager.GetSettingsValueByKey(p2pSettings, "MaxPrecessionValueForTaxesAndCharges"));
            RequisitionDocumentManager  objP2PDocumentManager = new RequisitionDocumentManager(this.JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
            List<P2PItem> lstp2pItemFromDB = new List<P2PItem>();

            /*Get Existing RequisitionItems Based on RequisitonID*/
            lstp2pItemFromDB = (List<P2PItem>)objP2PDocumentManager.GetLineItemBasicDetails(P2PDocumentType.Requisition, Convert.ToInt64(DocumentCode), ItemType.None, 0, int.MaxValue, string.Empty, string.Empty);

            /* Get ShipTo and deliverTo From Header */
            Requisition objRequisition = (Requisition)GetReqDao().GetBasicDetailsById(Convert.ToInt64(DocumentCode), UserContext.ContactCode);

            DataSet dsProratedItems = new DataSet();
            if (Tax > 0 || Shipping > 0 || AdditionalCharges > 0)
            {
                dsProratedItems = GetReqInterfaceDao().ProprateLineItemTaxandShipping(lstP2PItem, Tax, Shipping, AdditionalCharges, PunchoutCartReqId, precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges);
            }

            #region Update and Save RequisitionItems

            foreach (var item in lstP2PItem)
            {
                var existingItem = lstp2pItemFromDB.Where(itm => ((itm.SupplierPartId == item.SupplierPartId) && (itm.SupplierPartAuxiliaryId == item.SupplierPartAuxiliaryId)) && itm.PunchoutCartReqId == PunchoutCartReqId);

                if (existingItem != null && existingItem.Any()) /*To Update Existing RequisitionItems */
                {
                    existingItem.First().Currency = item.Currency;
                    existingItem.First().UOM = item.UOM;
                    existingItem.First().Description = item.Description;
                    existingItem.First().ManufacturerName = item.ManufacturerName;
                    existingItem.First().ManufacturerPartNumber = item.ManufacturerPartNumber;
                    existingItem.First().SupplierPartId = item.SupplierPartId;
                    existingItem.First().Quantity = item.Quantity;
                    existingItem.First().SupplierPartAuxiliaryId = item.SupplierPartAuxiliaryId;
                    existingItem.First().Unspsc = item.Unspsc;
                    existingItem.First().UnitPrice = item.UnitPrice;
                    existingItem.First().StartDate = existingItem.First().StartDate == DateTime.MinValue ? null : existingItem.First().StartDate;
                    existingItem.First().EndDate = existingItem.First().EndDate == DateTime.MinValue ? null : existingItem.First().EndDate;
                    existingItem.First().ModifiedBy = UserId;

                    if (dsProratedItems.Tables.Count > 0)
                    {
                        if (dsProratedItems.Tables[0] != null && dsProratedItems.Tables[0].Rows.Count > 0)
                        {

                            DataTable result = dsProratedItems.Tables[0].AsEnumerable()
                                                .Where(_item => Convert.ToString(_item.Field<string>("SupplierPartId")) == existingItem.First().SupplierPartId && _item.Field<string>("SupplierPartAuxiliaryId") == existingItem.First().SupplierPartAuxiliaryId)
                                                .CopyToDataTable();
                            if (result != null && result.Rows.Count > 0)
                            {
                                existingItem.First().Tax = (string.IsNullOrEmpty(Convert.ToString(result.Rows[0]["Tax"])) ? 0 : Convert.ToDecimal(result.Rows[0]["Tax"]));
                                existingItem.First().ShippingCharges = (string.IsNullOrEmpty(Convert.ToString(result.Rows[0]["ShippingCharges"])) ? 0 : Convert.ToDecimal(result.Rows[0]["ShippingCharges"]));
                                existingItem.First().AdditionalCharges = (string.IsNullOrEmpty(Convert.ToString(result.Rows[0]["AdditionalCharges"])) ? 0 : Convert.ToDecimal(result.Rows[0]["AdditionalCharges"]));
                            }
                        }
                    }
                    else
                    {
                        existingItem.First().Tax = (item.Tax == null ? 0 : item.Tax);
                        existingItem.First().ShippingCharges = (item.ShippingCharges == null ? 0 : item.ShippingCharges);
                        existingItem.First().AdditionalCharges = (item.AdditionalCharges == null ? 0 : item.AdditionalCharges);
                    }

                    List<P2PItem> objp2pItem = existingItem.ToList();

                    GetReqDao().SaveItem(objp2pItem[0], precessionValue, false, "", maxPrecessionforTotal, maxPrecessionForTaxesAndCharges);
                    item.DocumentItemId = GetReqDao().SaveItemAdditionDetails(objp2pItem[0], precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges); //Date requested/needed/ Shipping
                }
                else if (existingItem.Count() == 0) /*To Add New RequisitionItems */
                {
                    item.PunchoutCartReqId = PunchoutCartReqId;
                    item.PartnerCode = PartnerCode;
                    item.PartnerConfigurationId = PartnerConfigurationId;
                    item.DocumentId = Convert.ToInt64(DocumentCode);
                    item.CreatedBy = UserId;
                    item.DateRequested = DateTime.Now;
                    item.DateNeeded = DateTime.Now.AddDays(10);

                    item.SourceType = ItemSourceType.Punchout;
                    item.ItemExtendedType = ItemExtendedType.Material;

                    if (dsProratedItems.Tables.Count > 0)
                    {
                        if (dsProratedItems.Tables[0] != null && dsProratedItems.Tables[0].Rows.Count > 0)
                        {
                            DataTable result = dsProratedItems.Tables[0].AsEnumerable()
                                               .Where(_item => Convert.ToString(_item.Field<string>("SupplierPartId")) == item.SupplierPartId && _item.Field<string>("SupplierPartAuxiliaryId") == item.SupplierPartAuxiliaryId)
                                               .CopyToDataTable();
                            if (result != null && result.Rows.Count > 0)
                            {
                                item.Tax = (string.IsNullOrEmpty(Convert.ToString(result.Rows[0]["Tax"])) ? 0 : Convert.ToDecimal(result.Rows[0]["Tax"]));
                                item.ShippingCharges = (string.IsNullOrEmpty(Convert.ToString(result.Rows[0]["ShippingCharges"])) ? 0 : Convert.ToDecimal(result.Rows[0]["ShippingCharges"]));
                                item.AdditionalCharges = (string.IsNullOrEmpty(Convert.ToString(result.Rows[0]["AdditionalCharges"])) ? 0 : Convert.ToDecimal(result.Rows[0]["AdditionalCharges"]));
                            }
                        }
                    }
                    item.DocumentItemId = GetReqDao().SaveItem(item, precessionValue, false, "", maxPrecessionforTotal, maxPrecessionForTaxesAndCharges); //basic details
                    item.DocumentItemId = GetReqDao().SaveItemAdditionDetails(item, precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges); //Date requested/needed
                    GetReqDao().SaveItemPartnerDetails(item, precessionValue);//To Save Partner Details

                    GetReqDao().SaveItemShippingDetails(DocumentLineItemShippingID, item.DocumentItemId, string.Empty, objRequisition.ShiptoLocation.ShiptoLocationId, objRequisition.DelivertoLocation.DelivertoLocationId, item.Quantity, 0, UserContext.ContactCode, precessionValueDefaultAccounting, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges);

                    //save header level entities ends here
                    List<DocumentAdditionalEntityInfo> documentAdditionalEntityInfoList = new List<DocumentAdditionalEntityInfo>();
                    documentAdditionalEntityInfoList = null;
                    var documentSplitItemEntities = objP2PDocumentManager.GetDocumentDefaultAccountingDetails(P2PDocumentType.Requisition, LevelType.ItemLevel, UserContext.ContactCode, Convert.ToInt64(DocumentCode), documentAdditionalEntityInfoList);
                    GetReqDao().SaveDefaultAccountingDetails(Convert.ToInt64(DocumentCode), documentSplitItemEntities, precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges, true);/* Insert Items Split Details to show Error for GLCode */


                }
            }
            #endregion

            #region Delete RequisitionItems 
            var DelReqItemsList = from listp2pitems in lstp2pItemFromDB.Where(itm => itm.PunchoutCartReqId == PunchoutCartReqId)
                                  where !lstP2PItem.Any(x => (x.SupplierPartId == listp2pitems.SupplierPartId) && (x.SupplierPartAuxiliaryId == listp2pitems.SupplierPartAuxiliaryId))
                                  select listp2pitems;

            List<P2PItem> DeleteP2PItemList = DelReqItemsList.ToList();

            if (DeleteP2PItemList.Count > 0)
            {
                for (int i = 0; i < DeleteP2PItemList.Count; i++)
                {
                    objP2PDocumentManager.DeleteLineItemByIds(P2PDocumentType.Requisition, Convert.ToString(DeleteP2PItemList[i].DocumentItemId), precessionValueDefaultAccounting, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges);
                }
            }
            #endregion

            return Convert.ToInt64(DocumentCode);

        }

        public bool UpdateLineStatusForRequisitionFromInterface(List<RequisitionLineStatusUpdateDetails> reqDetails)
        {
            var objMessageHeader = new MessageHeader<UserExecutionContext>(this.UserContext);
            System.ServiceModel.Channels.MessageHeader messageHeader = objMessageHeader.GetUntypedHeader("GepCustomHeader", "Gep.Cumulus");

            long RequisitionId = 0;
            try
            {
                if (reqDetails != null && reqDetails.Any())
                {
                    foreach (var req in reqDetails)
                    {
                        RequisitionId = req.RequisitionId;

                        if (GetReqInterfaceDao().UpdateLineStatusForRequisitionFromInterface(RequisitionId, req.AllLineStatus, req.IsUpdateAllItems, req.Items, req.StockReservationNumber))
                        {
                            RequisitionEmailNotificationManager emailNotificationManager = new RequisitionEmailNotificationManager (UserContext = UserContext, GepConfiguration = GepConfiguration, this.JWTToken);
                            emailNotificationManager.SendNotificationForLineStatusUpdate(req);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occurred in UpdateLineStatusForRequisitionFromInterface." + RequisitionId, ex);
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw();
                throw;
            }
            return true;
        }

        #endregion

        #region Outbound

        public List<RequisitionItem> GetLineItemBasicDetailsForInterface(long documentCode)
        {
            return GetReqInterfaceDao().GetLineItemBasicDetailsForInterface(documentCode);
        }

        public List<long> GetRequisitionListForInterfaces(string docType, int docCount, int sourceSystemId)
        {
            return GetReqInterfaceDao().GetRequisitionListForInterfaces(docType, docCount, sourceSystemId);
        }

        public DataSet ValidateInterfaceLineStatus(long buyerPartnerCode, DataTable dtRequisitionDetail)
        {
            return GetReqInterfaceDao().ValidateInterfaceLineStatus(buyerPartnerCode, dtRequisitionDetail);
        }

        public List<string> ValidateInterfaceRequisition(Requisition objRequisition)
        {
            var lstErrors = new List<string>();
            RequisitionCommonManager objP2PCommonManager = new RequisitionCommonManager (this.JWTToken){ UserContext = UserContext, GepConfiguration = GepConfiguration };
            try
            {
                SettingDetails objSettings = objP2PCommonManager.GetSettingsFromSettingsComponent(P2PDocumentType.Interfaces, UserContext.ContactCode, (int)SubAppCodes.Interfaces);
                bool isHeaderShipToValid;
                bool isLiShipToValid;
                bool IsOrderingLocationMandatory = false, IsDefaultOrderingLocation = false;

                var dctSetting = objSettings.lstSettings.Distinct().ToDictionary(sett => sett.FieldName, sett => sett.FieldValue);

                var p2pSettings = objP2PCommonManager.GetSettingsFromSettingsComponent(P2PDocumentType.None, UserContext.ContactCode, (int)SubAppCodes.P2P);
                var RequisitionSettings = objP2PCommonManager.GetSettingsFromSettingsComponent(P2PDocumentType.Requisition, UserContext.ContactCode, (int)SubAppCodes.P2P);

                string settingValue = objP2PCommonManager.GetSettingsValueByKey(RequisitionSettings, "IsOrderingLocationMandatory");
                IsOrderingLocationMandatory = !string.IsNullOrEmpty(settingValue) ? Convert.ToBoolean(settingValue) : false;

                settingValue = objP2PCommonManager.GetSettingsValueByKey(p2pSettings, "IsDefaultOrderingLocation");
                IsDefaultOrderingLocation = !string.IsNullOrEmpty(settingValue) ? Convert.ToBoolean(settingValue) : false;

                lstErrors = ValidateShipToBillToFromInterface(objP2PCommonManager, objRequisition, dctSetting, out isHeaderShipToValid, out isLiShipToValid, 0);

                if (dctSetting != null && dctSetting.ContainsKey("AllowRequisitionErrorProcessing") && Convert.ToBoolean(dctSetting["AllowRequisitionErrorProcessing"]))
                    lstErrors.AddRange(GetReqInterfaceDao().ValidateErrorBasedInterfaceRequisition(objRequisition, dctSetting));
                else
                {
                    lstErrors.AddRange(ValidateItemDetailsToBeDerivedFromInterface(objRequisition, dctSetting));
                    lstErrors.AddRange(GetReqInterfaceDao().ValidateInterfaceRequisition(objRequisition, dctSetting, IsOrderingLocationMandatory, IsDefaultOrderingLocation));
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occurred in ValidateInterfaceRequisition method in RequisitionInterfaceManager documentnumber :- " + objRequisition?.DocumentNumber, ex);
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw();
                throw;
            }
            return lstErrors;
        }

        private List<string> ValidateShipToBillToFromInterface(RequisitionCommonManager objP2PCommonManager, Requisition objRequisition, Dictionary<string, string> dctSetting, out bool isHeaderShipToValid, out bool isLiShipToValid, long LobentitydetailCode)
        {
            List<string> lsterror = new List<string>();
            long lobEntityDetailCode = objRequisition.EntityDetailCode != null ? objRequisition.EntityDetailCode.FirstOrDefault() : 0;
            try
            {
                int shipToLocSetting = Convert.ToInt32(dctSetting["IsNewShipToLocCreationRequired"], NumberFormatInfo.InvariantInfo);
                int billToLocSetting = Convert.ToInt32(dctSetting["IsNewBillToLocCreationRequired"], NumberFormatInfo.InvariantInfo);
                var settingValue = objP2PCommonManager.GetSettingsValueByKey(P2PDocumentType.None, "AllowDeliverToFreeText", UserContext.ContactCode, (int)SubAppCodes.P2P, "", lobEntityDetailCode);
                bool deliverToFreeText = !string.IsNullOrEmpty(settingValue) ? Convert.ToBoolean(settingValue) : false;

                string strDefaultBillToLocation = dctSetting["DefaultBillToLocation"];
                bool IsDefaultBillToLocation = !string.IsNullOrEmpty(strDefaultBillToLocation) ? Convert.ToBoolean(strDefaultBillToLocation) : false;
                var strEntityMappedToBillToLocation = objP2PCommonManager.GetSettingsValueByKey(P2PDocumentType.None, "EntityMappedToBillToLocation", UserContext.ContactCode, (int)SubAppCodes.P2P, "", lobEntityDetailCode);
                int EntityMappedToBillToLocation = string.IsNullOrEmpty(strEntityMappedToBillToLocation) ? 0 : Convert.ToInt32(strEntityMappedToBillToLocation);

                long entitydetailcode = 0;
                var selectedEntity = objRequisition.DocumentAdditionalEntitiesInfoList != null ? objRequisition.DocumentAdditionalEntitiesInfoList.Where(a => a.EntityId == EntityMappedToBillToLocation) : null;
                if (selectedEntity != null && selectedEntity.Count() > 0)
                {
                    entitydetailcode = selectedEntity.FirstOrDefault().EntityDetailCode;
                }

                isHeaderShipToValid = false;
                isLiShipToValid = false;
                DataSet result = null;
                StringBuilder lstErrors = new StringBuilder();

                if (objRequisition != null && objRequisition.DocumentStatusInfo != DocumentStatus.Cancelled)
                {
                    result = GetReqInterfaceDao().ValidateShipToBillToFromInterface(objRequisition, Convert.ToBoolean(shipToLocSetting), Convert.ToBoolean(billToLocSetting), deliverToFreeText, LobentitydetailCode, IsDefaultBillToLocation, entitydetailcode);

                    #region Header ship to
                    if (objRequisition.ShiptoLocation != null && result.Tables["HeaderShiptoDetails"].Rows.Count > 0)
                    {
                        if (string.IsNullOrEmpty(result.Tables["HeaderShiptoDetails"].Rows[0]["ErrorDetails"].ToString()))
                        {
                            if ((int)result.Tables[0].Rows[0]["shiptolocid"] > 0)
                            {
                                objRequisition.ShiptoLocation.ShiptoLocationId = (int)result.Tables["HeaderShiptoDetails"].Rows[0]["shiptolocid"];
                                isHeaderShipToValid = true;
                            }
                        }


                        if (objRequisition.ShiptoLocation.ShiptoLocationId == 0 &&
                               Convert.ToInt32(shipToLocSetting, NumberFormatInfo.InvariantInfo) == 1)
                        {   //lstErrors.Add("There was an error in processing the order as it has invalid Ship to Location details at header level");
                            if (string.IsNullOrEmpty(result.Tables["HeaderShiptoDetails"].Rows[0]["ErrorDetails"].ToString()) == true)
                                isHeaderShipToValid = true;

                        }
                    }
                    #endregion

                    #region Item ShipTo
                    string strLiError = string.Empty;
                    if (objRequisition.RequisitionItems != null)
                    {

                        int emptyItemShipTo = 0;
                        foreach (DataRow dr in result.Tables["LineitemShiptoDetails"].Rows)
                        {
                            int liShipToLocationId = 0;
                            RequisitionItem lstRequisitionItem = objRequisition.RequisitionItems.Where(itm => itm.ItemLineNumber == Convert.ToInt64(dr["ItemLineNumber"])).FirstOrDefault();

                            if (lstRequisitionItem.DocumentItemShippingDetails != null &&
                                    lstRequisitionItem.DocumentItemShippingDetails.Any() &&
                                    lstRequisitionItem.DocumentItemShippingDetails.FirstOrDefault().ShiptoLocation != null)
                            {
                                if ((int)dr["shiptolocid"] > 0)
                                {
                                    liShipToLocationId = (int)dr["shiptolocid"];

                                    lstRequisitionItem.DocumentItemShippingDetails[0].ShiptoLocation.ShiptoLocationId = liShipToLocationId;

                                }
                                if (liShipToLocationId == 0 && Convert.ToInt32(shipToLocSetting, NumberFormatInfo.InvariantInfo) == 1)
                                {
                                    if (!string.IsNullOrEmpty(dr["ErrorDetails"].ToString()))
                                    {
                                        strLiError += dr["ErrorDetails"] + " " + dr["ItemLineNumber"] + ",";
                                    }

                                }
                                else if (liShipToLocationId == 0)
                                    strLiError += dr["ErrorDetails"] + " " + dr["ItemLineNumber"] + ",";
                            }
                            else
                            {
                                emptyItemShipTo += 1;
                                strLiError += dr["ErrorDetails"] + " " + dr["ItemLineNumber"] + ",";
                            }

                        }
                        strLiError = strLiError.Trim(',');
                        if (!isHeaderShipToValid && emptyItemShipTo == objRequisition.RequisitionItems.Count)  // to be removed
                        {
                            if (result.Tables["HeaderShiptoDetails"].Rows.Count <= 0)
                                lstErrors.Append("Invalid Ship to Location number or mandatory fields are missing in the Header.");  // to be removed
                            else
                                lstErrors.Append(result.Tables["HeaderShiptoDetails"].Rows[0]["ErrorDetails"].ToString());
                        }
                        else if (!isHeaderShipToValid && !string.IsNullOrWhiteSpace(strLiError) && strLiError.Length > 0)
                        {
                            lstErrors.Append(strLiError + ".");
                        }
                        else if (string.IsNullOrWhiteSpace(strLiError) && result.Tables["LineitemShiptoDetails"].Rows.Count > 0)
                            isLiShipToValid = true;
                        else if (!isHeaderShipToValid && !isLiShipToValid)
                            lstErrors.Append("There is an error in ship to location " + result.Tables["HeaderShiptoDetails"].Rows[0]["ErrorDetails"].ToString());



                    }
                    #endregion

                    #region Bill To

                    if (objRequisition.BilltoLocation != null)
                    {
                        if (!string.IsNullOrEmpty(objRequisition.BilltoLocation.BilltoLocationNumber))
                        {
                            if ((int)result.Tables["HeaderBilltoDetails"].Rows[0]["Billtolocid"] > 0)
                            {
                                objRequisition.BilltoLocation.BilltoLocationId = (int)result.Tables["HeaderBilltoDetails"].Rows[0]["Billtolocid"];

                            }
                            else if (objRequisition.BilltoLocation.BilltoLocationId == 0 && Convert.ToInt32(billToLocSetting, NumberFormatInfo.InvariantInfo) == 0)
                                lstErrors.Append(result.Tables["HeaderBilltoDetails"].Rows[0]["ErrorDetails"].ToString() + ".");

                            if (objRequisition.BilltoLocation.BilltoLocationId == 0 && Convert.ToInt32(billToLocSetting, NumberFormatInfo.InvariantInfo) == 1)
                            {
                                if (!string.IsNullOrEmpty(result.Tables["HeaderBilltoDetails"].Rows[0]["ErrorDetails"].ToString()))
                                    lstErrors.Append(result.Tables["HeaderBilltoDetails"].Rows[0]["ErrorDetails"].ToString() + ".");
                            }
                        }
                        else if (IsDefaultBillToLocation)
                        {
                            if ((int)result.Tables["HeaderBilltoDetails"].Rows[0]["Billtolocid"] > 0)
                            {
                                objRequisition.BilltoLocation.BilltoLocationId = (int)result.Tables["HeaderBilltoDetails"].Rows[0]["Billtolocid"];
                            }
                        }
                    }

                    #endregion

                    #region Header deliver to
                    if (deliverToFreeText == false)
                    {
                        if (objRequisition.DelivertoLocation != null && !string.IsNullOrWhiteSpace(objRequisition.DelivertoLocation.DelivertoLocationNumber))
                        {
                            if (result.Tables["HeaderDelivertoDetails"].Rows.Count > 0)
                            {
                                if ((int)result.Tables["HeaderDelivertoDetails"].Rows[0]["Delivertolocid"] > 0)
                                {
                                    objRequisition.DelivertoLocation.DelivertoLocationId = (int)result.Tables["HeaderDelivertoDetails"].Rows[0]["Delivertolocid"];
                                }
                                else
                                    lstErrors.Append(result.Tables["HeaderDelivertoDetails"].Rows[0]["ErrorDetails"].ToString() + ".");
                            }
                        }
                    }
                    #endregion

                    #region Item deliver to
                    int liDeliverToLocationId = 0;

                    if (result.Tables["LineLevelDelivertoDetails"].Rows.Count > 0)
                    {
                        foreach (DataRow dr in result.Tables[4].Rows)
                        {
                            RequisitionItem lstRequisitionItem = objRequisition.RequisitionItems.Where(itm => itm.ItemLineNumber == Convert.ToInt64(dr["ItemLineNumber"])).FirstOrDefault();
                            if ((int)dr["Delivertolocid"] > 0)
                            {
                                liDeliverToLocationId = (int)dr["Delivertolocid"];
                                lstRequisitionItem.DocumentItemShippingDetails[0].DelivertoLocation.DelivertoLocationId = liDeliverToLocationId;
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(dr["DelivertoLocationNumber"].ToString()))
                                {
                                    lstErrors.Append(dr["ErrorDetails"].ToString() + " " + (int)dr["ItemLineNumber"] + ".");
                                }
                            }
                        }
                    }

                    #endregion


                }
                if (lstErrors.Length > 0)
                    lsterror.Add(lstErrors.ToString());
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occurred in ValidateShipToBillToFromInterface method in RequisitionInterfaceManager documentnumber :- " + objRequisition?.DocumentNumber, ex);
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw();
                throw;
            }
            return lsterror;

        }

        private List<string> ValidateItemDetailsToBeDerivedFromInterface(Requisition objRequisition, Dictionary<string, string> dctSetting)
        {
            bool IsDeriveItemDetailEnable = false;

            if (dctSetting.ContainsKey("DeriveItemDetails"))
            {

                if (dctSetting["DeriveItemDetails"] != null && dctSetting["DeriveItemDetails"] != "")
                    IsDeriveItemDetailEnable = Convert.ToBoolean(dctSetting["DeriveItemDetails"]);
            }

            List<string> lsterror = new List<string>();
            DataSet result = null;

            if (IsDeriveItemDetailEnable == true)
            {
                if (objRequisition.RequisitionItems != null)
                {
                    foreach (var reqitem in objRequisition.RequisitionItems)
                    {
                        if (reqitem.ItemNumber != null && reqitem.ItemNumber != ""
                            && reqitem.PartnerSourceSystemValue != null && reqitem.PartnerSourceSystemValue != ""
                            && reqitem.UOM != null && reqitem.UOM != "")

                            result = GetReqInterfaceDao().ValidateItemDetailsToBeDerivedFromInterface(reqitem.ItemNumber, reqitem.PartnerSourceSystemValue, reqitem.UOM);

                        if (result != null && result.Tables != null && result.Tables.Count > 0)
                        {
                            if (result.Tables["ErrorDetail"].Rows[0]["ErrorMessage"].ToString() != "")
                                lsterror.Add(result.Tables["ErrorDetail"].Rows[0]["ErrorMessage"].ToString());
                        }
                    }
                }
            }

            return lsterror;
        }

        public DataTable ValidateReqItemsForExceptionHandling(DataTable dtItemDetails)
        {
            return GetReqInterfaceDao().ValidateReqItemsForExceptionHandling(dtItemDetails);
        }
        public void UpdateDocumentStatusFromInterface(long documentCode, int docStatus, int docExtendedStatus, int sourceSystemId,string errorDescription)
        {
            try
            {
                RequisitionDocumentManager  objDocBO = new RequisitionDocumentManager(this.JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
                if (docStatus > 0)
                    objDocBO.UpdateDocumentStatus(P2PDocumentType.Requisition, documentCode, (DocumentStatus)docStatus, 0);

                if (docExtendedStatus > 0)
                    objDocBO.UpdateDocumentInterfaceStatus(P2PDocumentType.Requisition, documentCode, (BusinessEntities.InterfaceStatus)docExtendedStatus, true, sourceSystemId, errorDescription: errorDescription);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log,
                    "Error Occured in UpdateDocumentStatusFromInterface method in RequisitionInterfaceManager documentcode :- " + documentCode, ex);
                var errorMessage = " Error Details: \n Message : " + ex.Message;
                errorMessage = errorMessage + " \n Stack Trace : " + Convert.ToString(ex.StackTrace);
                errorMessage = errorMessage + " \n Source : " + Convert.ToString(ex.Source);
                errorMessage = errorMessage + " \n Inner Exception : " +
                               (ex.InnerException != null ? ex.InnerException.Message : string.Empty);
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw();
                throw new Exception(errorMessage, ex);
            }
        }
        #endregion

        #region Upload file to blob storage

        public void UploadFileToBlobStorage(UserExecutionContext userExecutionContext, string containerName, string blobUri, string strContent)
        {
            CloudBlockBlob blob = GetBlobStorageDetails(userExecutionContext, containerName, blobUri, "INT");
            blob.UploadText(strContent);
        }

        private CloudBlockBlob GetBlobStorageDetails(UserExecutionContext userExecutionContext, string containerName, string blobUri, string storageType)
        {
            CloudStorageAccount storageAccount = GetBlobStorageAccount(userExecutionContext, storageType);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
            return container.GetBlockBlobReference(blobUri);
        }

        public CloudStorageAccount GetBlobStorageAccount(UserExecutionContext userExecutionContext, string storageType)
        {
            var objAzureTableStorageConfiguration = new FileManagerEntities.AzureTableStorageConfiguration();

            ObjectCache cache = MemoryCache.Default;
            if (cache.Contains("BlobStorageAccountNameForInterface_" + storageType + "_" + userExecutionContext.BuyerPartnerCode))
                objAzureTableStorageConfiguration.StorageAccountName = cache.Get("BlobStorageAccountNameForInterface_" + storageType + "_" + userExecutionContext.BuyerPartnerCode) as string;

            if (cache.Contains("BlobStorageAccountKeyForInterface_" + storageType + "_" + userExecutionContext.BuyerPartnerCode))
                objAzureTableStorageConfiguration.StorageAccountKey = cache.Get("BlobStorageAccountKeyForInterface_" + storageType + "_" + userExecutionContext.BuyerPartnerCode) as string;

            if (string.IsNullOrEmpty(objAzureTableStorageConfiguration.StorageAccountName) || string.IsNullOrEmpty(objAzureTableStorageConfiguration.StorageAccountKey))
            {
                try
                {
                    var fileManagerApi = new FileManagerApi(userExecutionContext, this.JWTToken);
                     
                    objAzureTableStorageConfiguration = fileManagerApi.GetTableStorageConfiguration(userExecutionContext.BuyerPartnerCode, storageType);
                }
                catch (Exception ex)
                {
                    throw new Exception("In Method: GetBlobAccountDetails. Exception Message: " + ex.Message);
                }

                CacheItemPolicy cacheItemPolicy = new CacheItemPolicy();
                cacheItemPolicy.AbsoluteExpiration = DateTime.Now.AddHours(24.0);
                cache.Add("BlobStorageAccountNameForInterface_" + storageType + "_" + userExecutionContext.BuyerPartnerCode, objAzureTableStorageConfiguration.StorageAccountName, cacheItemPolicy);
                cache.Add("BlobStorageAccountKeyForInterface_" + storageType + "_" + userExecutionContext.BuyerPartnerCode, objAzureTableStorageConfiguration.StorageAccountKey, cacheItemPolicy);
            }

            StorageCredentials storageCredentials = new StorageCredentials(objAzureTableStorageConfiguration.StorageAccountName, objAzureTableStorageConfiguration.StorageAccountKey);
            CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, true);
            return storageAccount;
        }

        #endregion

        public NewP2PEntities.Requisition GetRequistionDetailsForInterfaces(Int64 documentCode)
        {
            CommentsGroup commentGroup = null;
            List<Comments> lstComments = new List<Comments>();
            NewRequisitionManager reqManager = new NewRequisitionManager(this.JWTToken) { UserContext = this.UserContext, GepConfiguration = this.GepConfiguration };
            RequisitionCommonManager commonManager = new RequisitionCommonManager(this.JWTToken) { UserContext = this.UserContext, GepConfiguration = this.GepConfiguration };
            var objReq = reqManager.GetRequisitionDisplayDetails(documentCode);
            objReq = reqManager.GetDefaultEntities(objReq);

            #region "Header Custom Attributes"
            var headerQuestSetCodeList = objReq.CustomAttrQuestionSetCodesForHeader.Select(questSetCode => questSetCode.id).ToList<long>();

            if (headerQuestSetCodeList != null && headerQuestSetCodeList.Any())
            {
                var headerCustomAttributes = new List<Questionnaire>();
                commonManager.GetQuestionWithResponse(headerQuestSetCodeList, headerCustomAttributes, documentCode, false);
                objReq.CustomAttributesWithQuestions = JsonConvert.DeserializeObject<List<NewP2PEntities.Questionnaires>>(JsonConvert.SerializeObject(headerCustomAttributes));
            }
            #endregion "Header Custom Attributes" 


            foreach (var objRequisitionItem in objReq.items)
            {
                #region "Custom Attributes"
                var itemQuestSetCodeList = objReq.CustomAttrQuestionSetCodesForItem.Select(questSetCode => questSetCode.id).ToList<long>();

                if (itemQuestSetCodeList != null && itemQuestSetCodeList.Any())
                {
                    var CustomAttributes = new List<Questionnaire>();
                    commonManager.GetQuestionWithResponse(itemQuestSetCodeList, CustomAttributes, objRequisitionItem.id, false);
                    objRequisitionItem.CustomAttributesWithQuestions = JsonConvert.DeserializeObject<List<NewP2PEntities.Questionnaires>>(JsonConvert.SerializeObject(CustomAttributes));
                }
                #endregion "Custom Attributes" 

                objRequisitionItem.ProcurementStatusName = GetProcurementStatusName(objRequisitionItem.ProcurementStatus);
                objRequisitionItem.ItemTypeName = GetItemTypeName(objRequisitionItem.type.id);
                objRequisitionItem.ItemExtendedTypeName = GetItemExtendedTypeName(objRequisitionItem.type.id);

                #region "remove empty splits"
                foreach (var split in objRequisitionItem.splits)
                {
                    split.SplitEntities = split.SplitEntities.Where(entity => entity.entityCode != "").ToList();
                }
                #endregion "remove empty splits" 

            }
            DataSet PartnerSourceSystemDetails= reqManager.GetPartnerSourceSystemDetailsByReqId(documentCode);
            
                if (PartnerSourceSystemDetails.Tables[0].Rows != null && PartnerSourceSystemDetails.Tables[0].Rows.Count > 0)
                {
                    foreach (var objRequisitionItem in objReq.items)
                    {
                        objRequisitionItem.PartnerSourceSystem = new List<NewP2PEntities.NameAndValue>();
                        DataRow[] drrow = PartnerSourceSystemDetails.Tables[0].Select("RequisitionItemID = " + objRequisitionItem.id.ToString());

                        if (drrow != null && drrow.Count() > 0)
                        {
                            foreach (DataRow dr in drrow)
                            {                            
                                objRequisitionItem.PartnerSourceSystem.Add(
                                    new NewP2PEntities.NameAndValue{
                                            name = dr["SourceSystemName"].ToString(),
                                            value = dr["SourceSystemValue"].ToString()
                                        });
                            }
                        }
                    }
                }
           
            var precessionValue = convertStringToInt(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "MaxPrecessionValue", UserContext.ContactCode, (int)SubAppCodes.P2P, "", objReq.documentLOB.entityDetailCode));
            objReq.MaxPrecessionValue = precessionValue;
            var commentAccessType = "1,2,4";
            commentGroup = commonManager.GetComments(documentCode, P2PDocumentType.Requisition, commentAccessType, 1, 0, true);

            StringBuilder objStringBuilder = new StringBuilder();
            if (commentGroup != null && commentGroup.Comment != null && commentGroup.Comment.Any())
            {
                lstComments = commentGroup.Comment.Where(comments => !comments.CommentType.Equals(((int)CommentType.Reject).ToString()) && comments.CreatedBy != 0)
                                                  .ToList();

                lstComments.ForEach(comment => { objStringBuilder.Append(!string.IsNullOrEmpty(comment.CommentText) ? (comment.CommentText + "\n") : string.Empty); });
                commentGroup.Comment.FirstOrDefault().CommentText = objStringBuilder.ToString();
                objReq.HeaderComments = new List<Comments>();
                if (commentGroup.Comment != null && commentGroup.Comment.Any())
                    objReq.HeaderComments.Add(commentGroup.Comment.FirstOrDefault());
            }

            #region "Comments"

            foreach (var objRequisitionItem in objReq.items)
            {
                commentGroup = commonManager.GetComments(objRequisitionItem.id, P2PDocumentType.Requisition, commentAccessType, 2, 0, true);
 
                if (commentGroup != null && commentGroup.Comment != null && commentGroup.Comment.Any())
                {
                    objStringBuilder.Clear();
                    lstComments = commentGroup.Comment.Where(comments => !comments.CommentType.Equals(((int)CommentType.Reject).ToString()) && comments.CreatedBy != 0)
                                                        .ToList();
                    lstComments.ForEach(comment => { objStringBuilder.Append(!string.IsNullOrEmpty(comment.CommentText) ? (comment.CommentText + "\n") : string.Empty); });
                    commentGroup.Comment.FirstOrDefault().CommentText = objStringBuilder.ToString();
                    objRequisitionItem.LineComments = new List<Comments>();
                    if (commentGroup.Comment != null && commentGroup.Comment.Any())
                        objRequisitionItem.LineComments.Add(commentGroup.Comment.FirstOrDefault());
                }
            }

            #endregion "Comments"

            return objReq;
        }
        private string GetItemExtendedTypeName(long typeId)
        {
            switch (typeId)
            {
                case 0:
                    return "None";
                case 1:
                    return "Material";
                case 2:
                    return "Fixed";
                case 3:
                    return "Variable";
                case 4:
                    return "SERVICEACTIVITY";
                case 5:
                    return "MILESTONE";
                case 6:
                    return "PROGRESS";
                case 7:
                    return "ADVANCE";
                case 8:
                    return "CHARGE";
                case 9:
                    return "CONTINGENTWORKER";
                case 10:
                    return "EXPENSES";
                default:
                    return "";
            }

        }

        private string GetItemTypeName(long typeId)
        {
            switch (typeId)
            {
                case 0:
                    return "None";
                case 1:
                    return "Material";
                case 2:
                case 3:
                    return "Service";
               
                default:
                    return "";
            }

        }
        private string GetProcurementStatusName(int procurementStatus)
        {
            if (procurementStatus == 0) return "None";
            return Enum.GetName(typeof(DocumentStatus), procurementStatus);
        }
    }
}
