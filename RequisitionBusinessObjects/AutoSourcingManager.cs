using Gep.Cumulus.CSM.Entities;
using Gep.Cumulus.CSM.Extensions;
using Gep.Cumulus.Partner.Entities;
using GEP.Cumulus.DocumentIntegration.Entities;
using GEP.Cumulus.Documents.DataAccessObjects.SQLServer;
using GEP.Cumulus.Documents.Entities;
using GEP.Cumulus.Indexer.SearchIndexerEntities.Entities;
using GEP.Cumulus.Item.Entities;
using GEP.Cumulus.Logging;
using GEP.Cumulus.P2P.BusinessEntities;
using GEP.Cumulus.P2P.BusinessObjects;
using GEP.Cumulus.P2P.PO.BusinessObjects;
using GEP.Cumulus.P2P.Req.DataAccessObjects;
using GEP.SMART.Configuration;
using GEP.SMART.Storage.AzureSQL;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web.UI.WebControls.Expressions;//
using ItemType = GEP.Cumulus.P2P.BusinessEntities.ItemType;
using PayloadHelper = GEP.SMART.SearchCore.PayloadHelpers.Helpers.TemplateHelpers.Concrete.PayloadHelper;
using POProxy = GEP.Cumulus.P2P.Req.BusinessObjects.Proxy;
using Requisition = GEP.Cumulus.P2P.BusinessEntities.Requisition;

namespace GEP.Cumulus.P2P.Req.BusinessObjects
{
    [ExcludeFromCodeCoverage]
    public class ContractUtilizationTracker
    {
        public long DocumentCode { get; set; }
        public decimal PendingUtilization { get; set; }
        public decimal Quantity { get; set; }
        public ComplianceType ContractComplianceType { get; set; }
        public long RequisitionItemId { get; set; }
        public long BlanketDocumentCode { get; set; }
        public string BlanketDocumentNumber { get; set; }
        public Int32 BlanketLineItemNo { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal BlanketUtilized { get; set; }
        public decimal BlanketPending { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class AutoSourcingManager : RequisitionBaseBO
    {
        protected RequisitionCommonManager commonManager = null;
        protected P2P.PO.BusinessObjects.OrderCommonManager PocommonManager = null;
        protected long LOBEntityDetailCode = 0;
        protected ReliableSqlDatabase ContextSqlConn;
        private static readonly ILog Log = Logger.GetLog(MethodBase.GetCurrentMethod().DeclaringType);
        private Dictionary<string, object> AutoSourceLogging = new Dictionary<string, object>();       
        private HttpWebRequest req = null;

        public AutoSourcingManager(string jwtToken, UserExecutionContext context = null) : base(jwtToken)
        {
            if (context != null)
            {
                this.UserContext = context;
            }

            commonManager = new RequisitionCommonManager(this.JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
        }

        public List<RequisitionItem> AutoAssignBuyerToReqItems(List<RequisitionItem> reqItems, string HeaderOrgEntities, List<string> BuyerFetchItems)
        {
            List<KeyValuePair<long, long>> lstReqItemsToUpdate = new List<KeyValuePair<long, long>>();
            try
            {
                var responselist = GetLineItemsFromItemMaster(BuyerFetchItems, HeaderOrgEntities, true, DocumentType.Requisition);
                List<KeyValuePair<string, long>> validBuyerlist = GetValidBuyerContactCodes(responselist);

                foreach (var reqItem in reqItems.Where(x => x.BuyerContactCode == 0))
                {
                    reqItem.BuyerContactCode = (from item in validBuyerlist where item.Key == reqItem.ItemNumber select item.Value).FirstOrDefault();
                    if (reqItem.BuyerContactCode != 0)
                        lstReqItemsToUpdate.Add(new KeyValuePair<long, long>(reqItem.DocumentItemId, reqItem.BuyerContactCode));
                }
                bool result = GetReqDao().UpdateRequisitionBuyerContactCode(lstReqItemsToUpdate);
                return reqItems;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occurred in AutoAssignBuyerToReqItems method in AutoSourcingManager.", ex);
                throw ex;
            }
        }

        private List<ItemSearchDetails> GetLineItemsFromItemMaster(List<string> itemNumbers, string headerOrgEntities, bool IsExactWordSearch = false, DocumentType documentType = DocumentType.Requisition, GEP.Cumulus.P2P.BusinessEntities.ItemType itemType = GEP.Cumulus.P2P.BusinessEntities.ItemType.Material, DataSource searchField = DataSource.ItemMaster, int pageSize = 1000, int pageIndex = 0)
        {
            List<ItemSearchDetails> lstItemSearchDetails = new List<ItemSearchDetails>();
            try
            {
                POProxy.ProxyItemService objItemProxy = new POProxy.ProxyItemService(this.UserContext, this.JWTToken);
                ItemSearchInput objItemSearchInput = new ItemSearchInput();
                objItemSearchInput.DocumentType = documentType;
                objItemSearchInput.PageIndex = pageIndex;
                objItemSearchInput.Size = pageSize;
                objItemSearchInput.SearchField = searchField;
                objItemSearchInput.ItemType = (Item.Entities.ItemType)itemType;
                objItemSearchInput.IsExactWordSearch = IsExactWordSearch;
                objItemSearchInput.Text = itemNumbers;
                objItemSearchInput.HeaderOrgEntities = headerOrgEntities;

                lstItemSearchDetails = objItemProxy.GetLineItems(objItemSearchInput);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
            return lstItemSearchDetails;
        }

        public List<long> AutoSourcing(long RequisitionId, int processFlag, bool isautosubmit)
        {
            List<long> lstDocumentCode = new List<long>();
            UserExecutionContext userExecutionContext = UserContext;
            string lstLOBCodes = "";
            AutoSourceLogging.Clear();
            AutoSourceLogging.Add(AUTOSourcingLoggingConstants.Auto_RequisitionCode, RequisitionId.ToString());
            AutoSourcingNewrelicLogging(AUTOSourcingLoggingCustomEventNames.Auto_StartedAutoSourcing, AutoSourceLogging, AUTOSourcingLoggingCatchErrors.Auto_CatchError);
            try
            {
                var objReq = GetReqDao().GetAllRequisitionDetailsByRequisitionId(RequisitionId, userExecutionContext.ContactCode, 0);
                Document document = new SQLDocumentDAO() { UserContext = UserContext, GepConfiguration = GepConfiguration }.GetDocumentDetailsById(new Document()
                {
                    DocumentCode = RequisitionId,
                    DocumentName = "",
                    DocumentNumber = "",
                    IsDocumentDetails = true
                }, true);
                if (objReq.DocumentStatusInfo != DocumentStatus.Approved) return new List<long>();
                if (objReq.RequisitionItems == null)
                    objReq.RequisitionItems = new List<RequisitionItem>();

                AutoSourceLogging.Clear();
                AutoSourceLogging.Add(AUTOSourcingLoggingConstants.Auto_RequisitionCode, RequisitionId.ToString());
                AutoSourcingNewrelicLogging(AUTOSourcingLoggingCustomEventNames.Auto_AutoSourcingSettings, AutoSourceLogging, AUTOSourcingLoggingCatchErrors.Auto_CatchError);


                string HeaderOrgEntities = GetHeaderOrgandSplitEntities(objReq);

                objReq.RequisitionItems = CheckForItemMaster(objReq.RequisitionItems, HeaderOrgEntities);

                bool allowOneShipToLocation = commonManager.GetSettingsValueByKey(P2PDocumentType.None, "AllowOneShipToLocation", this.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBEntityDetailCode).ToUpper().Equals("TRUE");  //AllowOneShipToLocation
                bool showRemitToLocation = false;

                lstLOBCodes = Convert.ToString(document.DocumentLOBDetails.Select(x => x.EntityDetailCode).FirstOrDefault()); //Document LOB

                //Get all blankets
                List<Indexer.SearchIndexerEntities.Entities.EC_ContractDetails> lstBlanketDetails = GetContractsFromElastic(RequisitionId, lstLOBCodes, objReq.Currency);

                //Blanket Utilization
                objReq.RequisitionItems = BlanketSupplierDerivation(objReq, lstBlanketDetails, objReq.EntityId, showRemitToLocation, HeaderOrgEntities);

                foreach (var reqitem in objReq.RequisitionItems)
                {
                    if (reqitem.BuyerContactCode == 0)
                    {
                        reqitem.BuyerContactCode = objReq.RequesterId;
                    }
                }
                //AutoSourcing Logic = ASL   
                var ASLfailedItems = objReq.RequisitionItems.Where(x => x.IsDeleted == true).Select(s => s.P2PLineItemId.ToString()).ToList();
                if (ASLfailedItems.Count > 0)
                {
                    GetReqDao().UpdateRequisitionItemAutoSourceProcessFlag(ASLfailedItems.Aggregate((i, j) => i + "," + j), 1); //requisitonitem have isDeleted=true - process flag set to 1 ;                      
                    AutoSourceLogging.Clear();
                    AutoSourceLogging.Add(AUTOSourcingLoggingConstants.Auto_RequisitionCode, objReq.DocumentId.ToString());
                    AutoSourceLogging.Add(AUTOSourcingLoggingConstants.Auto_RequisitionItemId, string.Join(",", ASLfailedItems.ToList()));
                    AutoSourcingNewrelicLogging(AUTOSourcingLoggingCustomEventNames.Auto_ItemsFailedAutoSourcing, AutoSourceLogging, AUTOSourcingLoggingCatchErrors.Auto_CatchError);
                }
                objReq.RequisitionItems = objReq.RequisitionItems.Where(x => x.IsDeleted == false).ToList();

                if (objReq.RequisitionItems.Count == 0)
                    return new List<long>();
                List<List<RequisitionItem>> groupeditems = GroupingReqItems(objReq, allowOneShipToLocation, showRemitToLocation);

                lstDocumentCode = CreateOrderProcessAfterBlanketDerivation(objReq, groupeditems, lstBlanketDetails, processFlag, HeaderOrgEntities, lstLOBCodes, RequisitionId);

            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occurred in AutoSourcing method in AutoSourcingManager.", ex);
                throw;
            }
            return lstDocumentCode;
        }

        //Need to called by order API
        private List<long> CreateOrderProcessAfterBlanketDerivation(Requisition objReq, List<List<RequisitionItem>> groupeditems, List<EC_ContractDetails> lstBlanketDetails, int processFlag, string HeaderOrgEntities, string lstLOBCodes, long RequisitionId)
        {
            bool showRemitToLocation = false;
            bool IsValidateASL = commonManager.GetSettingsValueByKey(P2PDocumentType.Requisition, "AllowOneShipToLocation", this.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBEntityDetailCode).ToUpper().Equals("TRUE"); //commonManager.GetSettingsValueByKey
            List<Indexer.SearchIndexerEntities.Entities.EC_ContractAdditionalDetails> lstDocumentCode = new List<Indexer.SearchIndexerEntities.Entities.EC_ContractAdditionalDetails>();
            string serviceurl = MultiRegionConfig.GetConfig(CloudConfig.OrderServiceURL);
            RequisitionManager reqManager = new RequisitionManager(this.JWTToken) { UserContext = this.UserContext, GepConfiguration = this.GepConfiguration };
            UserExecutionContext userExecutionContext = this.UserContext;
            PocommonManager = new OrderCommonManager { UserContext = UserContext, GepConfiguration = GepConfiguration };
            try
            {
                foreach (var groupeditem in groupeditems.ToList())
                {
                    foreach (var item in groupeditem.SelectMany(s => s.BlanketUtilized).GroupBy(x => x.BlanketDocumentCode))
                    {
                        List<RequisitionItem> groupeditembyBlanket = groupeditem.Where(s => item.Any(x => x.BlanketDocumentCode == item.Key)).ToList();
                        groupeditembyBlanket.ToList().ForEach(u => u.Quantity = item.Where(x => x.RequisitionItemID == u.DocumentItemId).Select(s => s.Quantity).FirstOrDefault());
                        groupeditembyBlanket.ToList().ForEach(u => u.ContractNo = item.Where(x => x.RequisitionItemID == u.DocumentItemId).Select(s => s.BlanketDocumentNumber).FirstOrDefault().ToString());
                        groupeditembyBlanket.ToList().ForEach(u => u.ContractLineRef = item.Where(x => x.RequisitionItemID == u.DocumentItemId).Select(s => s.BlanketLineItemNo).First().ToString());
                        groupeditembyBlanket.ToList().ForEach(u => u.ContractEndDate = lstBlanketDetails.Where(x => x.DM_Documents.DocumentNumber == u.ContractNo).Select(s => s.DateExpiry).FirstOrDefault());
                        groupeditembyBlanket.ToList().ForEach(u => u.ContractName = lstBlanketDetails.Where(x => x.DM_Documents.DocumentNumber == u.ContractNo).Select(s => s.Name).FirstOrDefault());
                        bool IsAutoSubmitPO;
                        NewP2PEntities.Order order = FillOrderFromRequisition(objReq, groupeditembyBlanket, out IsAutoSubmitPO, lstBlanketDetails, showRemitToLocation, processFlag, PocommonManager, IsValidateASL, HeaderOrgEntities, lstLOBCodes);
                        lstDocumentCode.Add(new Indexer.SearchIndexerEntities.Entities.EC_ContractAdditionalDetails() { DocumentCode = order.documentCode, IsAutoSubmit = IsAutoSubmitPO });
                    }
                }
                AutoSourceLogging.Clear();
                AutoSourceLogging.Add(AUTOSourcingLoggingConstants.Auto_RequisitionCode, objReq.DocumentId.ToString());
                AutoSourceLogging.Add(AUTOSourcingLoggingConstants.Auto_RequisitionItemId, string.Join(",", lstDocumentCode.ToList()));
                AutoSourcingNewrelicLogging(AUTOSourcingLoggingCustomEventNames.Auto_rderAPIInAutoSourcing, AutoSourceLogging, AUTOSourcingLoggingCatchErrors.Auto_CatchError);

                var OrderprocessedID = OrderProcessing(objReq, lstDocumentCode);
                LogHelper.LogInfo(Log, "Error occurred in AutoSourcing method in AutoSourcingManager." + OrderprocessedID.ToJSON());
            }

            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occurred in AutoSourcing method in AutoSourcingManager.", ex);
                throw;
            }
            return (from doc in lstDocumentCode select doc.DocumentCode).Distinct().ToList();
        }

        private List<RequisitionItem> CheckForItemMaster(List<RequisitionItem> requisitionItems, string HeaderOrgEntities)//ItemMaster
        {
            POProxy.ProxyPartnerService proxyPartnerService = new POProxy.ProxyPartnerService(UserContext, this.JWTToken);
            try
            {
                if (AutoSourcingDerivationPrecedence())
                {
                    if (requisitionItems.Where(items => items.BuyerContactCode == 0).ToList().Count > 0)
                        requisitionItems = AutoAssignBuyerToReqItems(requisitionItems, HeaderOrgEntities, requisitionItems.Where(items => items.BuyerContactCode == 0).Select(item => item.ItemNumber).ToList());
                    List<long> buyerActivitychecklist = requisitionItems.Select(b => b.BuyerContactCode).Distinct().ToList();
                    foreach (long buyercode in buyerActivitychecklist)
                    {
                        if (!GetActivityStatus(proxyPartnerService, buyercode, UserContext.BuyerPartnerCode))
                        {
                            requisitionItems.Where(x => x.BuyerContactCode == buyercode).ToList().ForEach(b => b.BuyerContactCode = 0);
                        }
                    }

                    var workBenchProcessItems = requisitionItems.Where(x => x.BuyerContactCode == 0).Select(s => s.P2PLineItemId.ToString()).ToList();
                    if (workBenchProcessItems.Count > 0)
                        GetReqDao().UpdateRequisitionItemAutoSourceProcessFlag(workBenchProcessItems.Aggregate((i, j) => i + "," + j), 1); //TODO:requisitonitem have BuyerContactCode=0 - process flag set to 1 ;

                    requisitionItems = requisitionItems.Where(b => b.BuyerContactCode != 0).ToList();
                }
                foreach (RequisitionItem objItem in requisitionItems)
                {
                    objItem.DocumentItemShippingDetails = (List<DocumentItemShippingDetail>)GetReqDao().GetShippingSplitDetailsByLiId(objItem.DocumentItemId);
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occurred in CheckAutoSourcingDerivationPrecedence method in NewRequisitionManager.", ex);
                throw;
            }

            return requisitionItems;
        }

        private string GetHeaderOrgandSplitEntities(Requisition objReq)
        {
            string HeaderOrgEntities = "";
            if (objReq.DocumentAdditionalEntitiesInfoList != null || objReq.DocumentAdditionalEntitiesInfoList.Count > 0)
            {
                HeaderOrgEntities = objReq.DocumentAdditionalEntitiesInfoList.Count > 0 ? (objReq.DocumentAdditionalEntitiesInfoList.Select(i => i.EntityDetailCode.ToString()).ToList().Aggregate((i, j) => i + "," + j)) : "";
                string SplitEntities = GetSplitLevelFiledValue(objReq);
                HeaderOrgEntities += SplitEntities;
            }
            return HeaderOrgEntities;
        }

        private List<List<RequisitionItem>> GroupingReqItems(Requisition objReq, bool allowOneShipToLocation, bool showRemitToLocation)
        {
            return showRemitToLocation ?
                    allowOneShipToLocation ? objReq.RequisitionItems.GroupBy(x => new { x.Currency, x.PartnerCode, x.ContractNo, x.OrderLocationId, x.RemitToLocationId }).Select(g => g.ToList()).ToList() : objReq.RequisitionItems.GroupBy(x => new { x.Currency, x.PartnerCode, x.ContractNo, x.OrderLocationId, x.RemitToLocationId, x.DocumentItemShippingDetails[0].ShiptoLocation.ShiptoLocationName, x.BlanketNumber }).Select(g => g.ToList()).ToList()
                    : allowOneShipToLocation ? objReq.RequisitionItems.GroupBy(x => new { x.Currency, x.PartnerCode, x.ContractNo }).Select(g => g.ToList()).ToList() : objReq.RequisitionItems.GroupBy(x => new { x.Currency, x.PartnerCode, x.ContractNo, x.DocumentItemShippingDetails[0].ShiptoLocation.ShiptoLocationName, x.BlanketNumber }).Select(g => g.ToList()).ToList();
        }

        private bool GetActivityStatus(POProxy.ProxyPartnerService partnerProxy, long BuyerContactCode, long partnerCode)
        {
            var partnerHelper = new RESTAPIHelper.PartnerHelper(this.UserContext, JWTToken);
            string activitylist = partnerHelper.GetUserActivitiesByContactCode(BuyerContactCode, partnerCode);
            return string.IsNullOrEmpty(activitylist) ? false : (activitylist.Split(',').Contains("10700003") || activitylist.Split(',').Contains("10700079")); //Create Order (or) Flip Requisitions to Order
        }

        private List<KeyValuePair<string, long>> GetValidBuyerContactCodes(List<ItemSearchDetails> ItemMasterBuyerlist)
        {
            List<KeyValuePair<string, long>> validBuyerCodes = new List<KeyValuePair<string, long>>();
            try
            {
                ServiceInitialization.IPartnerServiceChannel objPartnerServiceChannel = GepServiceManager.GetInstance.CreateChannel<ServiceInitialization.IPartnerServiceChannel>(MultiRegionConfig.GetConfig(CloudConfig.PartnerServiceURL));
                UserExecutionContext userExecutionContext = UserContext;
                POProxy.ProxyPartnerService proxyPartnerService = new POProxy.ProxyPartnerService(UserContext, this.JWTToken);
                List<Contact> lstContacts = proxyPartnerService.GetAllContactsByPartnerCode(UserContext.BuyerPartnerCode, 0, 0, "", "", "", "", "", "");

                foreach (var Item in ItemMasterBuyerlist.Where(i => string.IsNullOrEmpty(i.ItemStandardFieldDetails.BuyerName) == false))
                {
                    long contactCode = 0;
                    if (Item.ItemStandardFieldDetails.BuyerName.Contains("@"))
                        contactCode = lstContacts.Where(e => e.IsActive && e.IsDeleted == false && (e.EmailAddress == Item.ItemStandardFieldDetails.BuyerName)).Select(e => e.ContactCode).FirstOrDefault();
                    else
                        contactCode = lstContacts.Where(e => e.IsActive && e.IsDeleted == false && (e.ContactCode == Convert.ToInt64(Item.ItemStandardFieldDetails.BuyerName))).Select(e => e.ContactCode).FirstOrDefault();

                    validBuyerCodes.Add(new KeyValuePair<string, long>(Item.ItemNumber, contactCode));
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occurred in GetValidBuyerContactCodes method in AutoSourcingManager", ex);
            }
            return validBuyerCodes;
        }

        public string GetSplitLevelFiledValue(Requisition objReq)
        {
            commonManager = new RequisitionCommonManager (this.JWTToken){ UserContext = UserContext, GepConfiguration = GepConfiguration };
            string AutoSourcingSplitEntity = commonManager.GetSettingsValueByKey(P2PDocumentType.Requisition, "AutoSourcingSplitEnitity", UserContext.ContactCode);
            var arrAutoSourcingSplitEntity = AutoSourcingSplitEntity.Trim().Split(',');
            string SplitEntities = "";
            if (!(string.IsNullOrEmpty(AutoSourcingSplitEntity)))
            {
                for (var i = 0; i < arrAutoSourcingSplitEntity.Count(); i++)
                {
                    SplitEntities += "," + Convert.ToString(objReq.RequisitionItems[0].ItemSplitsDetail[0].DocumentSplitItemEntities.Where(s => s.EntityTypeId == Convert.ToInt32(arrAutoSourcingSplitEntity[i])).Select(s => s.SplitAccountingFieldValue).FirstOrDefault());
                }
            }

            return SplitEntities;
        }

        public bool AutoSourcingDerivationPrecedence()
        {
            string RequisitionAutoSourcingItemMasterandBlanket = commonManager.GetSettingsValueByKey(P2PDocumentType.Requisition, "AutoSourcingDerivationPrecedence", UserContext.ContactCode);
            var arrRequisitionAutoSourcingItemMasterandBlanket = RequisitionAutoSourcingItemMasterandBlanket.Trim().Split(',');
            var statusForItemMasterandScm = (arrRequisitionAutoSourcingItemMasterandBlanket.Length > 0
                        && arrRequisitionAutoSourcingItemMasterandBlanket.Any(status => status == "ItemMaster"));
            return statusForItemMasterandScm;
        }

        public List<Indexer.SearchIndexerEntities.Entities.EC_ContractDetails> GetContractsFromElastic(long preDocumentId, string lstLOBCodes, string Currency)
        {

            string UsecaseName = "Requisition-Autosourcing";
            string Metadata = "metadata";
            List<Indexer.SearchIndexerEntities.Entities.EC_ContractDetails> allResults = new List<Indexer.SearchIndexerEntities.Entities.EC_ContractDetails>();
            AutoSourceLogging.Clear();
            AutoSourceLogging.Add(AUTOSourcingLoggingConstants.Auto_RequisitionCode, preDocumentId.ToString());
            AutoSourcingNewrelicLogging(AUTOSourcingLoggingCustomEventNames.Auto_AutoSourcingGetContractsFromElastic, AutoSourceLogging, AUTOSourcingLoggingCatchErrors.Auto_CatchError);

            try
            {

                var payload = CreatePayload(preDocumentId, lstLOBCodes, Currency);

                JObject payloadMetadata = new JObject();
                payloadMetadata.Add("IsDacRequired", true);
                payloadMetadata.Add("DocumentTypeCode", 30);
                payloadMetadata.Add("Key", null);
                payloadMetadata.Add("TypeIdentifier", 0);
                payloadMetadata.Add("DACRuleType", null);
                payloadMetadata.Add("IsTemplate", false);

                payload.Add(Metadata, JObject.FromObject(payloadMetadata));

                payload = PayloadHelper.Wrapper(UsecaseName, payload);

                var transactionId = Guid.NewGuid().ToString();

                var respES = GetESData(payload, transactionId, preDocumentId);

                if (!CheckResponse(respES))
                    return allResults;
                if (respES["Data"]["hits"]["total"].Value<long>() > 0)
                {

                    var hits = respES["Data"]["hits"]["hits"];
                    foreach (var i in hits)
                    {
                        Indexer.SearchIndexerEntities.Entities.EC_ContractDetails d = JsonConvert.DeserializeObject<Indexer.SearchIndexerEntities.Entities.EC_ContractDetails>(i["_source"].ToString());
                        allResults.Add(d);
                    }
                    allResults = allResults.Where(x => x.EC_LineItem.Count() > 0).Where(x => x.EC_ContractAdditionalDetails.IsAutoCreate == true).ToList();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return allResults;
        }

        private bool CheckResponse(JObject response)
        {
            return !ReferenceEquals(response["Status"], null) && response["Status"].ToString().ToLower() == "success";
        }

        private JObject CreatePayload(long preDocumentId, string lstLOBCodes, string Currency)
        {
            JObject resp = new JObject();
            try
            {
                JObject advResponse = new JObject();

                long PageIndex = 0;
                long PageSize = 1000;
                //request.GroupId = 1;
                List<string> sourceItems = new List<string>();
                List<string> ExcludedFields = new List<string>();
                ExcludedFields.Add("dM_Documents.dM_DocumentPas");
                ExcludedFields.Add("dM_Documents.dM_DocumentBU");
                ExcludedFields.Add("dM_Documents.dM_DocumentRegion");
                resp.Merge(PayloadHelper.Source(sourceItems, ExcludedFields));
                resp.Merge(PayloadHelper.From(PageIndex));
                resp.Merge(PayloadHelper.Size(PageSize));

                List<JObject> input = new List<JObject>();

                List<object> lstDocType = new List<object>();
                lstDocType.Add(Convert.ToString((int)Gep.Cumulus.Search.Entities.Enums.ModuleScope.Blanket));

                List<object> lstStatus = new List<object>();
                lstStatus.Add(Convert.ToString((int)GEP.Cumulus.Documents.Entities.DocumentStatus.Live));
                lstStatus.Add(Convert.ToString((int)GEP.Cumulus.Documents.Entities.DocumentStatus.Execute));

                List<object> lstEntities = new List<object>();
                lstEntities.Add(lstLOBCodes);

                List<object> lstCurrency = new List<object>();
                lstCurrency.Add(Currency.ToLower());

                input.Add(PayloadHelper.Terms("dM_Documents.documentTypeCode", lstDocType));
                input.Add(PayloadHelper.Terms("dM_Documents.documentStatus", lstStatus));
                input.Add(PayloadHelper.Terms("documentOrgMapping.entityDetailCode", lstEntities));
                input.Add(PayloadHelper.Terms("contractCurrency", lstCurrency));

                //List<JObject> input2 = new List<JObject>();
                //input2.Add(PayloadHelper.Term("expiryDateType", "1"));
                //Dictionary<PayloadHelper.RangeType, dynamic> value = new Dictionary<PayloadHelper.RangeType, dynamic>();
                //value.Add(PayloadHelper.RangeType.GreaterThanEqualTo, DateTime.Now);
                //input2.Add(PayloadHelper.Range("dateExpiry", value));
                //input.Add(PayloadHelper.Any(input2.ToArray()));

                resp.Merge(PayloadHelper.Filter(PayloadHelper.All(input.ToArray())));
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return resp;
        }

        public JObject GetESData(JObject payload, string transactionId, long preDocumentId)
        {
            var result = "";
            try
            {
                var requestHeaders = new RESTAPIHelper.RequestHeaders();
                requestHeaders.Set(UserContext, this.JWTToken);
                var webAPI = new RESTAPIHelper.WebAPI(requestHeaders, "DEV_APAC_Dss_Req_Microservice_NEWREQ", "AutoSourcingManager-FillOrderFromRequisition");
                var url = MultiRegionConfig.GetConfig(MultiRegionConfigTypes.PrimaryRegion, CloudConfig.SearchCoreApiUrl, UserContext.BuyerPartnerCode).FirstOrDefault();
                result = webAPI.ExecutePost(url, payload);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return JObject.Parse(result);
        }

        private List<Indexer.SearchIndexerEntities.Entities.EC_ContractDetails> FilterBlanketsByASLCheck(List<Indexer.SearchIndexerEntities.Entities.EC_ContractDetails> lstMatchedContracts, string headerOrgEntities, List<string> itemNumber, ItemType itemType)
        {
            List<long> supplierList;
            bool isSoleSourceEnabled;
            bool isEnforceASL;
            GetSuppliersFromItemMaster(headerOrgEntities, itemNumber, itemType, out supplierList, out isSoleSourceEnabled, out isEnforceASL);
            if (isSoleSourceEnabled || isEnforceASL)
            {
                lstMatchedContracts = (from filterContracts in lstMatchedContracts where filterContracts.DM_Documents.DM_DocumentStakeholder.Any(s => s.StakeholderType == 3 && supplierList.Contains(s.PartnerCode)) select filterContracts).ToList();
            }
            return lstMatchedContracts;
        }

        private void GetSuppliersFromItemMaster(string headerOrgEntities, List<string> itemNumbers, GEP.Cumulus.P2P.BusinessEntities.ItemType itemType, out List<long> supplierList, out bool isSoleSourceEnabled, out bool isEnforceASL)
        {
            supplierList = new List<long>();
            isSoleSourceEnabled = false;
            isEnforceASL = false;

            var responselist = GetLineItemsFromItemMaster(itemNumbers, headerOrgEntities, true, DocumentType.Requisition, itemType);
            if (responselist.Count > 0)
            {
                var isSoleSupplierExists = responselist.FirstOrDefault().EntityDetail.Any(y => y.ItemSupplierDetailsList.Any(z => z.StatusId == 4));
                isEnforceASL = responselist.FirstOrDefault().ItemStandardFieldDetails.ASL;
                //1- Approved,2-Debarred,3-Preferred,4-Sole Source

                if (isSoleSupplierExists)//sole supplier takes precedence
                {
                    isSoleSourceEnabled = true;
                    var supplierDetails = responselist.FirstOrDefault().EntityDetail.Where(x => x.ItemSupplierDetailsList.Any(s => s.StatusId == 4)).Select(x => x.ItemSupplierDetailsList).FirstOrDefault();
                    supplierList = supplierDetails.Where(s => s.StatusId == 4).Select(x => Convert.ToInt64(x.PartnerCode)).ToList();
                }
                else if (isEnforceASL)
                {
                    isSoleSourceEnabled = false;
                    var supplierDetails = responselist.FirstOrDefault().EntityDetail.Select(x => x.ItemSupplierDetailsList).FirstOrDefault();
                    supplierList = supplierDetails.Select(x => Convert.ToInt64(x.PartnerCode)).ToList();
                }
            }
        }

        public List<RequisitionItem> BlanketSupplierDerivation(Requisition objReq, List<Indexer.SearchIndexerEntities.Entities.EC_ContractDetails> lstBlankets, int legalEntity, bool showRemitToLocation, string HeaderOrgEntities)
        {
            var reqItems = objReq.RequisitionItems;
            var AutoSourcing = new Dictionary<string, object>();
            List<ContractUtilizationTracker> UtilizedBlanket = new List<ContractUtilizationTracker>();
            try
            {
                LogHelper.LogError(Log, "Error occurred in BlanketSupplierDerivation method .", new Exception { });
                var filtredBlanketbasedonlist = new Indexer.SearchIndexerEntities.Entities.EC_ContractDetails();
                if (lstBlankets.Count() > 0)
                {
                    AutoSourceLogging.Clear();
                    AutoSourceLogging.Add(AUTOSourcingLoggingConstants.Auto_RequisitionCode, objReq.DocumentId.ToString());
                    AutoSourceLogging.Add(AUTOSourcingLoggingConstants.Auto_ListOfContacts, string.Join(",", lstBlankets.Select(x => x.DocumentCode).ToList()));
                    AutoSourcingNewrelicLogging(AUTOSourcingLoggingCustomEventNames.Auto_ListOfFetchContacts, AutoSourceLogging, AUTOSourcingLoggingCatchErrors.Auto_CatchError);
                }
                foreach (var reqItem in reqItems)
                {
                    var lstMatchedBlankets = new List<Indexer.SearchIndexerEntities.Entities.EC_ContractDetails>();
                    if (reqItem.ContractNo != "")//this is for if Requisition Has the ContractNo
                    {
                        filtredBlanketbasedonlist = lstBlankets.Where(x => x.DM_Documents.DocumentNumber == reqItem.ContractNo).FirstOrDefault();
                        if (filtredBlanketbasedonlist != null && filtredBlanketbasedonlist.DocumentCode > 0)//if the blanket not valid 
                        {
                            var matchedBlanketitem = GetMatchingBlanketOrContractLineItem(objReq, reqItem, filtredBlanketbasedonlist);// list out associated contracts for given requisition line -- 2530
                            if (matchedBlanketitem != null)
                            {
                                foreach (var item in matchedBlanketitem)
                                {
                                    filtredBlanketbasedonlist.ContractLimit = item.UnitPrice; // for easy filter ,usign header level entity ,and saving our satisfied Line Unit Price .
                                    filtredBlanketbasedonlist.ContractTypeId = item.LineItemNo;   // for easy filter ,usign header level entity ,and saving our satisfied Line LineItemNo .
                                }
                                AutoSourceLogging.Clear();
                                AutoSourceLogging.Add(AUTOSourcingLoggingConstants.Auto_RequisitionCode, objReq.DocumentId.ToString());
                                AutoSourceLogging.Add(AUTOSourcingLoggingConstants.Auto_RequisitionItemId, reqItem.DocumentItemId.ToString());
                                AutoSourceLogging.Add(AUTOSourcingLoggingConstants.Auto_ContractNo, filtredBlanketbasedonlist.DocumentCode.ToString() + " - " + matchedBlanketitem[0].ItemId.ToString());
                                AutoSourcingNewrelicLogging(AUTOSourcingLoggingCustomEventNames.Auto_MatchedReqItems, AutoSourceLogging, AUTOSourcingLoggingCatchErrors.Auto_CatchError);

                                lstMatchedBlankets.Add(filtredBlanketbasedonlist);
                                ApplyPrecedenceLogic(reqItem, lstMatchedBlankets, ref UtilizedBlanket);
                            }
                            else
                            {
                                reqItem.IsDeleted = true;
                            }
                        }
                        else
                        {
                            reqItem.IsDeleted = true;
                        }
                    }
                    else
                    {
                        if (AutoSourcingDerivationPrecedence())
                        {
                            var filteredBlanketsByASLCheck = FilterBlanketsByASLCheck(lstBlankets, HeaderOrgEntities, new List<string> { reqItem.ItemNumber }, reqItem.ItemType);
                            foreach (var doc in filteredBlanketsByASLCheck)
                            {
                                List<Indexer.SearchIndexerEntities.Entities.EC_LineItem> matchedBlanketitem = GetMatchingBlanketOrContractLineItem(objReq, reqItem, doc);// list out associated contracts for given requisition line -- 2530
                                if (matchedBlanketitem != null)
                                {
                                    foreach (var item in matchedBlanketitem)
                                    {
                                        doc.ContractLimit = item.UnitPrice;  // for easy filter ,usign header level entity ,and saving our satisfied Line Unit Price .
                                        doc.ContractTypeId = item.LineItemNo;// for easy filter ,usign header level entity ,and saving our satisfied Line LineItemNo .                                       
                                    }
                                    lstMatchedBlankets.Add(doc);
                                    AutoSourceLogging.Clear();
                                    AutoSourceLogging.Add(AUTOSourcingLoggingConstants.Auto_RequisitionCode, objReq.DocumentId.ToString());
                                    AutoSourceLogging.Add(AUTOSourcingLoggingConstants.Auto_RequisitionItemId, reqItem.DocumentItemId.ToString());
                                    AutoSourceLogging.Add(AUTOSourcingLoggingConstants.Auto_ContractNo, filtredBlanketbasedonlist.DocumentCode.ToString() + " - " + matchedBlanketitem[0].ItemId.ToString());
                                    AutoSourcingNewrelicLogging(AUTOSourcingLoggingCustomEventNames.Auto_MatchedReqItems, AutoSourceLogging, AUTOSourcingLoggingCatchErrors.Auto_CatchError);
                                }
                            }
                        }
                        else
                        {
                            foreach (var doc in lstBlankets)
                            {
                                List<Indexer.SearchIndexerEntities.Entities.EC_LineItem> matchedBlanketitem = GetMatchingBlanketOrContractLineItem(objReq, reqItem, doc);// list out associated contracts for given requisition line -- 2530
                                if (matchedBlanketitem.Count > 0)
                                {
                                    foreach (var item in matchedBlanketitem)
                                    {
                                        doc.ContractLimit = item.UnitPrice;  // for easy filter ,usign header level entity ,and saving our satisfied Line Unit Price .
                                        doc.ContractTypeId = item.LineItemNo;// for easy filter ,usign header level entity ,and saving our satisfied Line LineItemNo .                                       
                                    }
                                    lstMatchedBlankets.Add(doc);
                                    AutoSourceLogging.Clear();
                                    AutoSourceLogging.Add(AUTOSourcingLoggingConstants.Auto_RequisitionCode, objReq.DocumentId.ToString());
                                    AutoSourceLogging.Add(AUTOSourcingLoggingConstants.Auto_RequisitionItemId, reqItem.DocumentItemId.ToString());
                                    AutoSourceLogging.Add(AUTOSourcingLoggingConstants.Auto_ContractNo, filtredBlanketbasedonlist.DocumentCode.ToString() + " - " + matchedBlanketitem[0].ItemId.ToString());
                                    AutoSourcingNewrelicLogging(AUTOSourcingLoggingCustomEventNames.Auto_MatchedReqItems, AutoSourceLogging, AUTOSourcingLoggingCatchErrors.Auto_CatchError);
                                }
                            }
                        }

                        if (lstMatchedBlankets.Count > 0)
                        {
                            ApplyPrecedenceLogic(reqItem, lstMatchedBlankets, ref UtilizedBlanket);
                        }
                        else { reqItem.IsDeleted = true; }

                    }
                }
                return reqItems;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occurred in BlanketSupplierDerivation method in AutoSourcingManager.", ex);
                throw ex;
            }
        }

        private RequisitionItem ApplyPrecedenceLogic(RequisitionItem reqItem, List<Indexer.SearchIndexerEntities.Entities.EC_ContractDetails> lstMatchedBlankets, ref List<ContractUtilizationTracker> UtilizedBlanket)
        {
            var lstBlanketUtilization = new List<ContractUtilization>();
            reqItem.BlanketUtilized = new List<BlanketItems>();

            foreach (var contract in lstMatchedBlankets.ToList())
            {
                List<LineUtilization> lstLineUtilizationValues = new List<LineUtilization>();
                var lstLineUtilization = new LineUtilization
                {
                    LineItemNo = Convert.ToInt32(contract.ContractTypeId),//Here ContractTypeId Means Contract Satisfied Line Item no,By sending this we can expect only 1 Line contract  
                    DocumentNumber = contract.DM_Documents.DocumentNumber,
                    DocumentCode = contract.DM_Documents.DocumentCode,
                    TargetCurrencyCode = contract.ContractCurrency,
                    DateExpiry = contract.DateExpiry
                };
                lstLineUtilizationValues.Add(lstLineUtilization);
                var objContractUtilization = new ContractUtilization
                {
                    DocumentNumber = contract.DM_Documents.DocumentNumber,
                    UtilizedValue = Convert.ToDecimal(contract.ContractUtilizedValue),
                    ListLineUtilization = lstLineUtilizationValues,
                    TargetCurrencyCode = contract.ContractCurrency,
                    DocumentCode = contract.DM_Documents.DocumentCode
                };
                lstBlanketUtilization.Add(objContractUtilization);
            }
            var UtlizationAvailableBlankets = GetBlanketUtilizationByHeaderORLineLevel(lstBlanketUtilization, reqItem, lstMatchedBlankets, ref UtilizedBlanket);
            if (UtlizationAvailableBlankets.Any())
            {
                var ValidBlankets = new List<Indexer.SearchIndexerEntities.Entities.EC_ContractDetails>();
                foreach (var blanket in UtlizationAvailableBlankets)
                {
                    var FillingEachValidBlanket = new Indexer.SearchIndexerEntities.Entities.EC_ContractDetails();
                    FillingEachValidBlanket = (from s in lstMatchedBlankets where s.DocumentCode == blanket.DocumentCode select s).FirstOrDefault();
                    FillingEachValidBlanket.ContractMode = Convert.ToInt32(blanket.PendingUtilization);//filling each  blanket quantity for easy filter,instead of calling again method
                    FillingEachValidBlanket.ComplianceType = Convert.ToInt32(blanket.ContractComplianceType);
                    ValidBlankets.Add(FillingEachValidBlanket);//getting only UtlizationAvailableBlankets
                    reqItem.BlanketUtilized.Add(new BlanketItems
                    {
                        BlanketDocumentCode = blanket.BlanketDocumentCode,
                        Quantity = blanket.Quantity,
                        BlanketComplianceType = (int)blanket.ContractComplianceType,
                        BlanketDocumentNumber = blanket.BlanketDocumentNumber,
                        BlanketLineItemNo = blanket.BlanketLineItemNo,
                        UnitPrice = blanket.UnitPrice,
                        BlanketUtilized = blanket.BlanketUtilized,
                        BlanketPending = blanket.BlanketPending,
                        RequisitionItemID = blanket.RequisitionItemId,
                        SuplierId = blanket.DocumentCode
                    });
                }
                if (ValidBlankets.Count() > 0)
                {
                    AutoSourceLogging.Clear();
                    AutoSourceLogging.Add(AUTOSourcingLoggingConstants.Auto_RequisitionCode, reqItem.DocumentId.ToString());
                    AutoSourceLogging.Add(AUTOSourcingLoggingConstants.Auto_utilizationAviableContracts, string.Join(",", ValidBlankets.Select(x => x.DocumentCode).ToList()));
                    AutoSourcingNewrelicLogging(AUTOSourcingLoggingCustomEventNames.Auto_NotUtilizedContracts, AutoSourceLogging, AUTOSourcingLoggingCatchErrors.Auto_CatchError);
                }
                if (ValidBlankets.Count() > 0 && ValidBlankets[0].DocumentCode > 0)
                {
                    #region old code
                    //ValidBlankets = (from s in ValidBlankets where s.ContractLimit == ValidBlankets.Min(x => x.ContractLimit) select s).ToList();//select lowest price contract          
                    //var filteredBlankets = (from s in ValidBlankets where (s.DateExpiry == ValidBlankets.Min(x => x.DateExpiry) && s.DateExpiry.HasValue) select s).ToList();//select first expiring  contract  
                    //if (!filteredBlankets.Any())
                    //{
                    //    filteredBlankets = (from s in ValidBlankets where !s.DateExpiry.HasValue select s).ToList();//checking if no exipring contracts, select perpectual contracts 
                    //}
                    //var SingleContractAfterFilter = new Indexer.SearchIndexerEntities.Entities.EC_ContractDetails();
                    ////Filter blankets with compliance type - Line Quantity
                    //var contractsWithComplianceTypeLineQuantity = filteredBlankets.Where(c => c.ComplianceType == Convert.ToInt32(ComplianceType.LineValueWithQuantity));
                    //if (contractsWithComplianceTypeLineQuantity != null && contractsWithComplianceTypeLineQuantity.Any())
                    //{
                    //    //if valid blankets of compliance type - Line Quantity are available with sufficient quantity, Minimum pending utilization check is made only on these type of blankets.
                    //    SingleContractAfterFilter = (from s in contractsWithComplianceTypeLineQuantity where s.ContractMode == contractsWithComplianceTypeLineQuantity.Min(x => x.ContractMode) select s).First();//select minimum quanity/value based on complaince type                        
                    //}
                    //else
                    //{
                    //    //if no valid blankets of compliance type-line quantity are available, minumum utilization check is applied on all blankets 
                    //    SingleContractAfterFilter = (from s in filteredBlankets where s.ContractMode == filteredBlankets.Min(x => x.ContractMode) select s).FirstOrDefault();//select minimum quanity/value based on complaince type                        
                    //}
                    //if (SingleContractAfterFilter != null && ValidBlankets[0].DocumentCode > 0)
                    //{
                    #endregion
                    reqItem.ContractNo = ValidBlankets[0].DM_Documents.DocumentNumber;
                    reqItem.PartnerCode = ValidBlankets[0].DM_Documents.DM_DocumentStakeholder != null ? ValidBlankets[0].DM_Documents.DM_DocumentStakeholder.Where(t => t.StakeholderType == 3).Select(s => s.PartnerCode).FirstOrDefault() : 0;
                    reqItem.RemitToLocationId = Convert.ToInt64(ValidBlankets[0].EC_ContractLinkLocationAddressDetails.Where(a => a.LocationTypeId == 1).Select(a => a.LocationId).FirstOrDefault());
                    reqItem.RemitToLocationAddressCode = Convert.ToInt64(ValidBlankets[0].EC_ContractLinkLocationAddressDetails.Where(a => a.LocationTypeId == 1).Select(a => a.AddressCode).FirstOrDefault());
                    reqItem.OrderLocationId = Convert.ToInt64(ValidBlankets[0].EC_ContractLinkLocationAddressDetails.Where(a => a.LocationTypeId == 2).Select(a => a.LocationId).FirstOrDefault());
                    reqItem.OrderLocationAddressCode = Convert.ToInt64(ValidBlankets[0].EC_ContractLinkLocationAddressDetails.Where(a => a.LocationTypeId == 2).Select(a => a.AddressCode).FirstOrDefault());
                    reqItem.ContractItemId = Convert.ToInt32(ValidBlankets[0].ContractTypeId);//here ContractTypeId Means Contract Satisfied Line Item no,By sending this we can expect only 1 Line contract  
                    if (reqItem.BuyerContactCode == 0)
                    {
                        var ContractCode = ValidBlankets[0].DM_Documents.DM_DocumentStakeholder != null ? ValidBlankets[0].DM_Documents.DM_DocumentStakeholder.Where(x => x.StakeholderType == 11).Select(x => x.ContactCode).FirstOrDefault() : null;
                        if (ContractCode != null)
                        {
                            reqItem.BuyerContactCode = Convert.ToInt64(ContractCode);
                        }
                    }
                }
            }
            UtilizedBlanket.AddRange(UtlizationAvailableBlankets);
            GetNewReqDao().SaveReqItemBlanketMapping(reqItem.BlanketUtilized, reqItem.DocumentItemId);
            return reqItem;
        }

        public List<ContractUtilizationTracker> GetBlanketUtilizationByHeaderORLineLevel(List<ContractUtilization> BlanketUtilizationDetails, RequisitionItem reqItem, List<Indexer.SearchIndexerEntities.Entities.EC_ContractDetails> lstMatchedContracts, ref List<ContractUtilizationTracker> UtilizedBlanket)
        {
            var documents = new List<ContractUtilizationTracker>();
            string strcontractUtilizationDetails = "";
            UserExecutionContext userExecutionContext = this.UserContext;
            string serviceurl = MultiRegionConfig.GetConfig(CloudConfig.ContractRestServiceURL);
            try
            {
                if (BlanketUtilizationDetails != null)
                {
                    RequisitionManager reqManager = new RequisitionManager (this.JWTToken){ UserContext = this.UserContext, GepConfiguration = this.GepConfiguration };
                    //var commonManager = new OrderCommonManager { UserContext = UserContext, GepConfiguration = GepConfiguration };
                    var contractUtilizationTracking = Convert.ToInt32(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "ContractUtilizationTracking", UserContext.UserId));
                    var ContractUtilizationTrackingIncludeTaxes = Convert.ToBoolean(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "ContractUtilizationTrackingIncludeTaxes", UserContext.UserId));
                    if (contractUtilizationTracking == 3)
                    {
                        string contractResult = null;
                        serviceurl = serviceurl + "/GetContractUtilization";
                        CreateHttpWebRequest(serviceurl);
                        contractResult = GetHttpWebResponseforList(BlanketUtilizationDetails);
                        strcontractUtilizationDetails = ((Dictionary<string, object>)JSONHelper.DeserializeObject(contractResult))["GetContractUtilizationResult"].ToJSON();
                        List<ContractUtilization> validContractdetails = JSONHelper.DeserializeObj<List<ContractUtilization>>(strcontractUtilizationDetails);
                        validContractdetails.ToList().ForEach(u => u.ListLineUtilization[0].DateExpiry = BlanketUtilizationDetails.Where(x => x.DocumentNumber == u.DocumentNumber).Select(s => s.ListLineUtilization[0].DateExpiry).FirstOrDefault());
                        if (validContractdetails != null && validContractdetails.Any())
                        {
                            validContractdetails = validContractdetails.OrderBy(x => x.ListLineUtilization.Max(s => s.UnitPrice)).ThenBy(x => x.ListLineUtilization.Max(s => s.DateExpiry)).ThenBy(x => x.ListLineUtilization.Max(s => s.Quantity)).ThenByDescending(x => x.ComplianceType).ToList();
                            decimal reqQuantity = reqItem.Quantity;
                            decimal pendingReq = 0, reqAmount = 0;
                            decimal sumOfBlanket = validContractdetails.Select(s => s.ListLineUtilization.Sum(d => d.Quantity)).Sum();

                            if (sumOfBlanket >= reqQuantity)
                            {
                                foreach (var contract in validContractdetails)
                                {

                                    if (contract.UnlimitedUtilization == false)
                                    {

                                        bool? EnforceLineReference = false;

                                        var matchingContract = lstMatchedContracts.Where(x => x.DM_Documents.DocumentNumber == contract.DocumentNumber).FirstOrDefault();
                                        if (matchingContract != null)
                                            EnforceLineReference = matchingContract.EC_ContractAdditionalDetails.EnforceLineReference;

                                        if (contract.ComplianceType == ComplianceType.LineValueWithQuantity && contract.ListLineUtilization != null && contract.ListLineUtilization.Any() && contract.ContractValue > contract.UtilizedValue)// checking Line level By Quantity 
                                        {
                                            if ((UtilizedBlanket != null || UtilizedBlanket.Count > 0) && UtilizedBlanket.Any(s => s.BlanketDocumentCode == contract.ListLineUtilization[0].DocumentCode))
                                            {
                                                contract.ListLineUtilization[0].Quantity = contract.ListLineUtilization[0].Quantity - UtilizedBlanket.Where(s => s.BlanketDocumentCode == contract.ListLineUtilization[0].DocumentCode).Sum(s => s.BlanketUtilized);
                                            }
                                            decimal contractLineQuantity = contract.ListLineUtilization.Min(s => s.Quantity) - contract.ListLineUtilization.Min(s => s.UtilizedQuantityValue);
                                            if (contractLineQuantity > 0)
                                            {
                                                pendingReq = (reqQuantity - contractLineQuantity);
                                                if (pendingReq > 0)
                                                {
                                                    documents.Add(new ContractUtilizationTracker
                                                    {
                                                        DocumentCode = contract.DocumentCode,
                                                        PendingUtilization = pendingReq,
                                                        ContractComplianceType = contract.ComplianceType,
                                                        Quantity = contractLineQuantity,
                                                        RequisitionItemId = reqItem.DocumentItemId,
                                                        BlanketDocumentCode = contract.DocumentCode,
                                                        BlanketDocumentNumber = contract.DocumentNumber,
                                                        BlanketLineItemNo = contract.ListLineUtilization.Select(c => c.LineItemNo).First(),
                                                        UnitPrice = contract.ListLineUtilization.Select(c => c.UnitPrice).First(),
                                                        BlanketUtilized = contract.ListLineUtilization.Select(c => c.Quantity).First(),
                                                        BlanketPending = 0
                                                    });
                                                    reqQuantity = pendingReq;
                                                }
                                                else
                                                {
                                                    documents.Add(new ContractUtilizationTracker
                                                    {
                                                        DocumentCode = contract.DocumentCode,
                                                        PendingUtilization = Math.Abs(pendingReq),
                                                        ContractComplianceType = contract.ComplianceType,
                                                        Quantity = reqQuantity,
                                                        RequisitionItemId = reqItem.DocumentItemId,
                                                        BlanketDocumentCode = contract.DocumentCode,
                                                        BlanketDocumentNumber = contract.DocumentNumber,
                                                        BlanketLineItemNo = contract.ListLineUtilization.Select(c => c.LineItemNo).First(),
                                                        UnitPrice = contract.ListLineUtilization.Select(c => c.UnitPrice).First(),
                                                        BlanketUtilized = reqQuantity,
                                                        BlanketPending = Math.Abs(pendingReq)
                                                    });
                                                    break;
                                                }
                                            }
                                        }

                                        if (contract.ComplianceType == ComplianceType.LineValue && contract.ListLineUtilization != null && contract.ListLineUtilization.Any() //&& PrecedencyCheckingForQuantiy == 0  //checking Line level By Amount //
                                            && contract.ContractValue > contract.UtilizedValue)
                                        {
                                            if ((UtilizedBlanket != null || UtilizedBlanket.Count > 0) && UtilizedBlanket.Any(s => s.BlanketDocumentCode == contract.ListLineUtilization[0].DocumentCode))
                                            {
                                                contract.ListLineUtilization[0].Total = contract.ListLineUtilization[0].Total - UtilizedBlanket.Where(s => s.BlanketDocumentCode == contract.ListLineUtilization[0].DocumentCode).Sum(s => s.BlanketUtilized);
                                            }
                                            if (reqAmount == 0)
                                            {
                                                reqAmount = Convert.ToDecimal(((EnforceLineReference == true ? contract.ListLineUtilization.Min(s => s.UnitPrice) : reqItem.UnitPrice) * reqItem.Quantity) + ((ContractUtilizationTrackingIncludeTaxes == true ? (reqItem.AdditionalCharges + reqItem.ShippingCharges + reqItem.Tax) : 0)));
                                            }
                                            decimal contractLineQuantity = contract.ListLineUtilization.Min(s => s.Quantity);
                                            decimal contractLineValue = contract.ListLineUtilization.Min(s => s.Total) - contract.ListLineUtilization.Min(s => s.UtilizedLineValue);//Why do we need to do this min check?
                                            if (contractLineValue > 0)
                                            {
                                                pendingReq = (reqAmount - contractLineValue);
                                                if (pendingReq > 0)
                                                {
                                                    documents.Add(new ContractUtilizationTracker
                                                    {
                                                        DocumentCode = contract.DocumentCode,
                                                        PendingUtilization = Math.Abs(pendingReq),
                                                        ContractComplianceType = contract.ComplianceType,
                                                        Quantity = contractLineQuantity,
                                                        RequisitionItemId = reqItem.DocumentItemId,
                                                        BlanketDocumentCode = contract.DocumentCode,
                                                        BlanketDocumentNumber = contract.DocumentNumber,
                                                        BlanketLineItemNo = contract.ListLineUtilization.Select(c => c.LineItemNo).First(),
                                                        UnitPrice = contract.ListLineUtilization.Select(c => c.UnitPrice).First(),
                                                        BlanketUtilized = contractLineValue,
                                                        BlanketPending = 0
                                                    });
                                                    reqAmount = pendingReq;
                                                }
                                                else
                                                {
                                                    documents.Add(new ContractUtilizationTracker
                                                    {
                                                        DocumentCode = contract.DocumentCode,
                                                        PendingUtilization = Math.Abs(pendingReq),
                                                        ContractComplianceType = contract.ComplianceType,
                                                        Quantity = contractLineQuantity,
                                                        RequisitionItemId = reqItem.DocumentItemId,
                                                        BlanketDocumentCode = contract.DocumentCode,
                                                        BlanketDocumentNumber = contract.DocumentNumber,
                                                        BlanketLineItemNo = contract.ListLineUtilization.Select(c => c.LineItemNo).First(),
                                                        UnitPrice = contract.ListLineUtilization.Select(c => c.UnitPrice).First(),
                                                        BlanketUtilized = reqAmount,
                                                        BlanketPending = Math.Abs(pendingReq)
                                                    });

                                                    break;
                                                }
                                            }
                                        }
                                        // Checking Header Level//
                                        else if (contract.ComplianceType == ComplianceType.None || contract.ComplianceType == ComplianceType.HeaderValue //&& PrecedencyCheckingForQuantiy == 0 
                                            && contract.ContractValue > contract.UtilizedValue)
                                        {
                                            if ((UtilizedBlanket != null || UtilizedBlanket.Count > 0) && UtilizedBlanket.Any(s => s.BlanketDocumentCode == contract.ListLineUtilization[0].DocumentCode))
                                            {
                                                contract.ContractValue = contract.ContractValue - UtilizedBlanket.Where(s => s.BlanketDocumentCode == contract.ListLineUtilization[0].DocumentCode).Sum(s => s.BlanketUtilized);
                                            }
                                            decimal contractLineQuantity = contract.ListLineUtilization.Min(s => s.Quantity);
                                            if (reqAmount == 0)
                                            {
                                                reqAmount = Convert.ToDecimal(((EnforceLineReference == true ? contract.ListLineUtilization.Min(s => s.UnitPrice) : reqItem.UnitPrice) * reqItem.Quantity) + ((ContractUtilizationTrackingIncludeTaxes == true ? (reqItem.AdditionalCharges + reqItem.ShippingCharges + reqItem.Tax) : 0)));
                                            }
                                            decimal contractLineValue = contract.UtilizedValue + reqAmount;

                                            pendingReq = contractLineValue - contract.ContractValue;
                                            if (pendingReq > 0)
                                            {
                                                documents.Add(new ContractUtilizationTracker
                                                {
                                                    DocumentCode = contract.DocumentCode,
                                                    PendingUtilization = Math.Abs(pendingReq),
                                                    ContractComplianceType = contract.ComplianceType,
                                                    Quantity = contractLineQuantity,
                                                    RequisitionItemId = reqItem.DocumentItemId,
                                                    BlanketDocumentCode = contract.DocumentCode,
                                                    BlanketDocumentNumber = contract.DocumentNumber,
                                                    BlanketLineItemNo = contract.ListLineUtilization.Select(c => c.LineItemNo).First(),
                                                    UnitPrice = contract.ListLineUtilization.Select(c => c.UnitPrice).First(),
                                                    BlanketUtilized = contract.ContractValue,
                                                    BlanketPending = 0
                                                });
                                                reqAmount = pendingReq;
                                            }
                                            else
                                            {
                                                documents.Add(new ContractUtilizationTracker
                                                {
                                                    DocumentCode = contract.DocumentCode,
                                                    PendingUtilization = Math.Abs(pendingReq),
                                                    ContractComplianceType = contract.ComplianceType,
                                                    Quantity = contractLineQuantity,
                                                    RequisitionItemId = reqItem.DocumentItemId,
                                                    BlanketDocumentCode = contract.DocumentCode,
                                                    BlanketDocumentNumber = contract.DocumentNumber,
                                                    BlanketLineItemNo = contract.ListLineUtilization.Select(c => c.LineItemNo).First(),
                                                    UnitPrice = contract.ListLineUtilization.Select(c => c.UnitPrice).First(),
                                                    BlanketUtilized = reqAmount,
                                                    BlanketPending = Math.Abs(pendingReq)
                                                });
                                                break;
                                            }

                                        }
                                        #region commented code
                                        //// checking Line level By Quantity //
                                        //if (contract.ComplianceType == ComplianceType.LineValueWithQuantity && contract.ListLineUtilization != null && contract.ListLineUtilization.Any() && contract.ContractValue > contract.UtilizedValue)
                                        //{
                                        //    decimal contractLineQuantity = contract.ListLineUtilization.Min(s => s.Quantity); //Why do we need to do this min check?
                                        //    decimal contractUtilizedLineQuantity = contract.ListLineUtilization.Min(s => s.UtilizedQuantityValue) + reqQuantity;
                                        //    if ((contractLineQuantity >= contractUtilizedLineQuantity))// && contract.ListLineUtilization != null && contract.ListLineUtilization.Count() > 0)
                                        //    {
                                        //        decimal finalquantity = (contractLineQuantity - contractUtilizedLineQuantity);
                                        //        documents.Add(new P2P.BusinessObjects.ContractUtilizationTracker
                                        //        {
                                        //            DocumentCode = contract.DocumentCode,
                                        //            PendingUtilization = finalquantity,
                                        //            ContractComplianceType = contract.ComplianceType
                                        //        });
                                        //        //PrecedencyCheckingForQuantiy = 1;
                                        //    }
                                        //}
                                        ////checking Line level By Amount //
                                        //else if (contract.ComplianceType == ComplianceType.LineValue && contract.ListLineUtilization != null && contract.ListLineUtilization.Any() //&& PrecedencyCheckingForQuantiy == 0 
                                        //    && contract.ContractValue > contract.UtilizedValue)
                                        //{
                                        //    decimal reqAmount = Convert.ToDecimal(((EnforceLineReference == true ? contract.ListLineUtilization.Min(s => s.UnitPrice) : reqItem.UnitPrice) * reqItem.Quantity) + ((ContractUtilizationTrackingIncludeTaxes == true ? (reqItem.AdditionalCharges + reqItem.ShippingCharges + reqItem.Tax) : 0)));

                                        //    decimal contractLineValue = contract.ListLineUtilization.Min(s => s.Total);//Why do we need to do this min check?
                                        //    decimal contratUtilizedLineValue = contract.ListLineUtilization.Min(s => s.UtilizedLineValue) + reqAmount;
                                        //    if (contractLineValue >= contratUtilizedLineValue)
                                        //    {
                                        //        decimal finalValue = (contractLineValue - contratUtilizedLineValue);
                                        //        documents.Add(new P2P.BusinessObjects.ContractUtilizationTracker
                                        //        {
                                        //            DocumentCode = contract.DocumentCode,
                                        //            PendingUtilization = finalValue,
                                        //            ContractComplianceType = contract.ComplianceType
                                        //        });
                                        //    }
                                        //}
                                        //// Checking Header Level//
                                        //else if (contract.ComplianceType == ComplianceType.None || contract.ComplianceType == ComplianceType.HeaderValue //&& PrecedencyCheckingForQuantiy == 0 
                                        //    && contract.ContractValue > contract.UtilizedValue)
                                        //{
                                        //    decimal reqAmount = Convert.ToDecimal(((EnforceLineReference == true ? contract.ListLineUtilization.Min(s => s.UnitPrice) : reqItem.UnitPrice) * reqItem.Quantity) + ((ContractUtilizationTrackingIncludeTaxes == true ? (reqItem.AdditionalCharges + reqItem.ShippingCharges + reqItem.Tax) : 0)));

                                        //    decimal contractLineValue = contract.UtilizedValue + reqAmount;
                                        //    if (contract.ContractValue >= contractLineValue)
                                        //    {
                                        //        decimal amount = contract.ContractValue - contractLineValue;
                                        //        documents.Add(new P2P.BusinessObjects.ContractUtilizationTracker
                                        //        {
                                        //            DocumentCode = contract.DocumentCode,
                                        //            PendingUtilization = amount,
                                        //            ContractComplianceType = contract.ComplianceType
                                        //        });
                                        //    }
                                        //}
                                        #endregion
                                    }
                                }
                            }
                            if (pendingReq > 0)
                            {
                                reqItem.IsDeleted = true;
                                documents = documents.Where(s => s.RequisitionItemId != reqItem.DocumentItemId).ToList();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log Exception here
                LogHelper.LogError(Log, "Error occurred in GetLineLevelContractUtilization method in OrderDocumentManager.", ex);
                throw;
            }
            return documents;

        }

        //private List<Indexer.SearchIndexerEntities.Entities.EC_ContractDetails> GetValidContracts(List<Indexer.SearchIndexerEntities.Entities.EC_ContractDetails> lstContract)
        //{
        //    return (from doc in lstContract where doc.DM_Documents.DocumentTypeCode == (int)Gep.Cumulus.Search.Entities.Enums.ModuleScope.Contract select doc).ToList();
        //}

        //private List<Indexer.SearchIndexerEntities.Entities.EC_ContractDetails> GetValidBlankets(List<Indexer.SearchIndexerEntities.Entities.EC_ContractDetails> lstBlanket)
        //{
        //    return (from doc in lstBlanket where ((bool)doc.EC_ContractAdditionalDetails.IsAutoCreate) && doc.DM_Documents.DocumentTypeCode == (int)Gep.Cumulus.Search.Entities.Enums.ModuleScope.Blanket select doc).ToList();
        //}

        private List<Indexer.SearchIndexerEntities.Entities.EC_LineItem> GetMatchingBlanketOrContractLineItem(Requisition objReq, RequisitionItem reqItem, Indexer.SearchIndexerEntities.Entities.EC_ContractDetails doc)
        {
            try
            {
                //var commonManager = new OrderCommonManager { UserContext = UserContext, GepConfiguration = GepConfiguration };
                string AutoSourcingSplitEntity = commonManager.GetSettingsValueByKey(P2PDocumentType.Requisition, "AutoSourcingSplitEnitity", UserContext.ContactCode);
                var arrAutoSourcingSplitEntity = AutoSourcingSplitEntity.Trim().Split(',');
                Dictionary<int, int> splitDetails = new Dictionary<int, int>();
                if (!(string.IsNullOrEmpty(AutoSourcingSplitEntity)))
                {
                    foreach (var item in reqItem.ItemSplitsDetail)
                    {
                        for (var i = 0; i < arrAutoSourcingSplitEntity.Count(); i++)
                        {
                            string SplitAccountingFieldValue = Convert.ToString(item.DocumentSplitItemEntities.Where(s => s.EntityTypeId == Convert.ToInt32(arrAutoSourcingSplitEntity[i])).Select(s => s.SplitAccountingFieldValue).FirstOrDefault());//instead of static we have added one config for site level changes 

                            splitDetails.Add(Convert.ToInt32(arrAutoSourcingSplitEntity[i]), Convert.ToInt32(SplitAccountingFieldValue));
                        }

                    }
                }
                bool checkAllHierachyLevels = CheckAllHierachyLevels(objReq, doc, splitDetails);

                return (from c in doc.EC_LineItem
                        where
                       (((reqItem.ContractItemId != 0 && (c.LineItemNo == reqItem.ContractItemId))
                       && ((string.IsNullOrWhiteSpace(reqItem.ItemNumber) && string.IsNullOrWhiteSpace(reqItem.SupplierPartId))
                       || (((!string.IsNullOrWhiteSpace(reqItem.ItemNumber) && (c.ItemNumber == reqItem.ItemNumber))
                       || (!string.IsNullOrWhiteSpace(reqItem.SupplierPartId) && (c.SupplierPartNumber == reqItem.SupplierPartId))))))

                       || (reqItem.ContractItemId == 0 && ((!string.IsNullOrWhiteSpace(reqItem.ItemNumber) && (c.ItemNumber == reqItem.ItemNumber))
                       || (!string.IsNullOrWhiteSpace(reqItem.SupplierPartId) && (c.SupplierPartNumber == reqItem.SupplierPartId)))))
                       && c.UOM == reqItem.UOM && checkAllHierachyLevels
                        select c).ToList<Indexer.SearchIndexerEntities.Entities.EC_LineItem>();
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occurred in BlanketSupplierDerivation method in AutoSourcingManager" + "" + reqItem.ToString(), ex);
                throw ex;
            }
            return null;
            //Checking  valid Blanket Item/Blanket Supllier Item Number/Valid Item Number or not with  Req  Blanket Number/ItemNumber/SupplierPartId
        }

        private bool CheckAllHierachyLevels(Requisition objReq, Indexer.SearchIndexerEntities.Entities.EC_ContractDetails doc, Dictionary<int, int> ReqAllSiteEntitydetailCodes)
        {
            LogHelper.LogError(Log, "Error occurred in AutoSourcing method in AutoSourcingManager start. CheckAllHierachyLevels", new Exception { });
            if (doc.DocumentOrgMapping == null || doc.DocumentOrgMapping.Count == 0)
                return false;
            Dictionary<int, int> getSplitHeaderLevelEntitiesFromConfig = GetSplitHeaderLevelEntitiesFromConfig(objReq, ReqAllSiteEntitydetailCodes);
            for (int i = 0; i < getSplitHeaderLevelEntitiesFromConfig.Count(); i++)
            {
                if (doc.DocumentOrgMapping.Any(s => s.EntityId == getSplitHeaderLevelEntitiesFromConfig.ElementAt(i).Key)) //checking entity Code having in Blanket or not .
                {
                    bool docStatus = true;
                    for (int j = i; j < getSplitHeaderLevelEntitiesFromConfig.Count(); j++) //looping split and header entitis accordingly order sequeance.
                    {

                        if (!(doc.DocumentOrgMapping.Any(s => s.EntityId == getSplitHeaderLevelEntitiesFromConfig.ElementAt(j).Key))) //i am not cheking with enittydetails code due to from elastic search we are getting based on document only so same entitydetails code why we need to check .
                        {
                            docStatus = false;//If even single entity not available then skiping . 
                            break;
                        }
                    }
                    if (docStatus)
                        return true;
                }
            }
            return false;
        }

        public Dictionary<int, int> GetSplitHeaderLevelEntitiesFromConfig(Requisition objReq, Dictionary<int, int> ReqAllSiteEntitydetailCodes)
        {
            //Reading SplitLevel Config data from setting 
            string AutoSourcingSplitEntity = commonManager.GetSettingsValueByKey(P2PDocumentType.Requisition, "AutoSourcingSplitEnitity", UserContext.ContactCode);
            var arrAutoSourcingSplitEntity = AutoSourcingSplitEntity.Trim().Split(',');
            Dictionary<int, int> GetAllSplitLevelAndHeaderLevelDetials = new Dictionary<int, int>();
            if (!(string.IsNullOrEmpty(AutoSourcingSplitEntity)))
            {
                for (var i = 0; i < arrAutoSourcingSplitEntity.Count(); i++)
                {
                    GetAllSplitLevelAndHeaderLevelDetials.Add(Convert.ToInt32(arrAutoSourcingSplitEntity[i]), Convert.ToInt32(objReq.RequisitionItems[0].ItemSplitsDetail[0].DocumentSplitItemEntities.Where(s => s.EntityTypeId == Convert.ToInt32(arrAutoSourcingSplitEntity[i])).Select(s => s.SplitAccountingFieldValue).FirstOrDefault()));
                }
            }
            //Reading hedaer level Config blanket from setting 
            string AutoSourcingHeaderEntity = commonManager.GetSettingsValueByKey(P2PDocumentType.Requisition, "AutoSourcingHeaderEntity", UserContext.ContactCode);
            var arrAutoSourcingHeaderEntity = (AutoSourcingHeaderEntity.Trim().Split(','));
            if (AutoSourcingHeaderEntity != "0" && AutoSourcingHeaderEntity != "")
            {
                for (var i = 0; i < arrAutoSourcingHeaderEntity.Count(); i++)
                {
                    ReqAllSiteEntitydetailCodes.Add(Convert.ToInt32(arrAutoSourcingHeaderEntity[i]), Convert.ToInt32(objReq.DocumentAdditionalEntitiesInfoList.Where(e => e.EntityId == Convert.ToInt32(arrAutoSourcingHeaderEntity[i])).Select(e => e.EntityDetailCode).FirstOrDefault())); //LOB
                }
            }
            return ReqAllSiteEntitydetailCodes;
        }

        private void CreateHttpWebRequest(string strURL, string method = "POST")
        {
            req = WebRequest.Create(strURL) as HttpWebRequest;
            req.Method = method;
            req.ContentType = @"application/json";

            NameValueCollection nameValueCollection = new NameValueCollection();
            //UserContext.UserName = "";
            string userName = UserContext.UserName;
            string clientName = UserContext.ClientName;
            UserContext.UserName = string.Empty;
            UserContext.ClientName = string.Empty;
            string userContextJson = UserContext.ToJSON();
            nameValueCollection.Add("UserExecutionContext", userContextJson);
            nameValueCollection.Add("Authorization", this.JWTToken);
            req.Headers.Add(nameValueCollection);
            UserContext.UserName = userName;
            UserContext.ClientName = clientName;

        }

        private string GetHttpWebResponseforList(List<ContractUtilization> ContractUtilizationDetails)
        {
            System.Web.Script.Serialization.JavaScriptSerializer JSrz = new System.Web.Script.Serialization.JavaScriptSerializer();
            Dictionary<string, object> data = new Dictionary<string, object>();
            // HttpWebRequest req = WebRequest.Create(strURL) as HttpWebRequest;
            data.Add("ContractUtilizationDetails", ContractUtilizationDetails);
            string jsonData = data.ToJSON().ToString();
            var byteData = Encoding.UTF8.GetBytes(jsonData);


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

        public NewP2PEntities.Order FillOrderFromRequisition(Requisition objReq, List<RequisitionItem> groupedReqItems, out bool IsAutoSubmitPO, List<EC_ContractDetails> lstContractDetails, bool showRemitToLocation, int processFlag, OrderCommonManager commonManager, bool IsValidateASL, string HeaderOrgEntities, string lstLOBCodes)
        {
            NewP2PEntities.Order OrderjsonResult = new NewP2PEntities.Order();
            IsAutoSubmitPO = false;
            Dictionary<string, object> body = new Dictionary<string, object>();
            try
            {
                body.Add("objReq", objReq);
                body.Add("groupedReqItems", groupedReqItems);
                body.Add("IsAutoSubmitPO", IsAutoSubmitPO);
                body.Add("lstContractDetails", lstContractDetails);
                body.Add("showRemitToLocation", showRemitToLocation);
                body.Add("processFlag", processFlag);
                body.Add("commonManager", commonManager);
                body.Add("IsValidateASL", IsValidateASL);
                body.Add("HeaderOrgEntities", HeaderOrgEntities);
                body.Add("lstLOBCodes", lstLOBCodes);
                string FillOrderFromRequisitionAPIURL = MultiRegionConfig.GetConfig(CloudConfig.APIMBaseURL) + URLs.FillOrderFromRequisition;

                var requestHeaders = new RESTAPIHelper.RequestHeaders();
                requestHeaders.Set(UserContext, this.JWTToken);
                var webAPI = new RESTAPIHelper.WebAPI(requestHeaders, "DEV_APAC_Dss_Req_Microservice_NEWREQ", "AutoSourcingManager-FillOrderFromRequisition");
                var JsonResult = webAPI.ExecutePost(FillOrderFromRequisitionAPIURL, body);

                OrderjsonResult = JsonConvert.DeserializeObject<NewP2PEntities.Order>(JsonResult);
                return OrderjsonResult;
            }
            catch (Exception ex)
            {

                LogHelper.LogError(Log, "Error occured in UpdateRequisitionItemAutoSourceProcessFlag_WebAPI in AutoSourcingManager method for documentCode(s)=" + body.ToJSON(), ex);
            }
            return OrderjsonResult;
        }

        public List<long> OrderProcessing(Requisition objReq, List<Indexer.SearchIndexerEntities.Entities.EC_ContractAdditionalDetails> lstDocumentCode)
        {
            List<long> OrderjsonResult = new List<long>();
            Dictionary<string, object> body = new Dictionary<string, object>();
            try
            {
                body.Add("objReq", objReq);
                body.Add("lstDocumentCode", lstDocumentCode);
                string OrderProcessingAPIURL = MultiRegionConfig.GetConfig(CloudConfig.APIMBaseURL) + URLs.OrderProcessing;

                var requestHeaders = new RESTAPIHelper.RequestHeaders();
                requestHeaders.Set(UserContext, this.JWTToken);
                var webAPI = new RESTAPIHelper.WebAPI(requestHeaders, "DEV_APAC_Dss_Req_Microservice_NEWREQ", "AutoSourcingManager-OrderProcessing");
                var JsonResult = webAPI.ExecutePost(OrderProcessingAPIURL, body);

                OrderjsonResult = JsonConvert.DeserializeObject<List<long>>(JsonResult);
                return OrderjsonResult;
            }
            catch (Exception ex)
            {

                LogHelper.LogError(Log, "Error occured in UpdateRequisitionItemAutoSourceProcessFlag_WebAPI in AutoSourcingManager method for documentCode(s)=" + body.ToJSON(), ex);
            }
            return OrderjsonResult;
        }

        public void AutoSourcingNewrelicLogging(string eventName, Dictionary<string, object> autosource, string error)
        {
            try
            {
                NewRelic.Api.Agent.NewRelic.RecordCustomEvent(eventName, autosource);
            }
            catch (Exception)
            {
                Dictionary<string, object> msg = new Dictionary<string, object>();
                NewRelic.Api.Agent.NewRelic.NoticeError(error, msg);                
            }
        }
    }
    public static class AUTOSourcingLoggingConstants
    {
        public const string Auto_RequisitionCode = "RequisitionCode";
        public const string Auto_RequisitionItemId = "RequisitionItemId";
        public const string Auto_ListOfContacts = "ListOfContacts";
        public const string Auto_ContractNo = "ContractNo";
        public const string Auto_utilizationAviableContracts = "utilizationAviableContracts";
        public const string Auto_FinalContract = "FinalContract";
        public const string Auto_OrderNumber = "OrderNumber";
    }
    public static class AUTOSourcingLoggingCustomEventNames
    {
        public const string Auto_StartedAutoSourcing = "StartedAutoSourcing";
        public const string Auto_ItemsFailedAutoSourcing = "ItemsFailedAutoSourcing";
        public const string Auto_ListOfFetchContacts = "ListOfFetchContacts";
        public const string Auto_MatchedReqItems = "MatchedReqItems";
        public const string Auto_NotUtilizedContracts = "NotUtilizedContracts";
        public const string Auto_FinalContract = "FinalContract";
        public const string Auto_rderAPIInAutoSourcing = "OrderAPIInAutoSourcing";
        public const string Auto_AutoSourcingGetContractsFromElastic = "AutoSourcingGetContractsFromElastic";
        public const string Auto_AutoSourcingCreatePayload = "AutoSourcingCreatePayload";
        public const string Auto_AutoSourcingGetESData = "AutoSourcingGetESData";
        public const string Auto_AutoSourcingSettings = "AutoSourcingSettings";
    }
    public static class AUTOSourcingLoggingCatchErrors
    {
        public const string Auto_CatchError = "Error occurred in AutoCreateWorkBenchOrder method in AutoSourcingManager start";
    }
}