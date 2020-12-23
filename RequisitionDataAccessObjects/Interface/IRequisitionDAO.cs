using Gep.Cumulus.CSM.Entities;
using Gep.Cumulus.Partner.Entities;
using GEP.Cumulus.DocumentIntegration.Entities;
using GEP.Cumulus.Documents.Entities;
using GEP.Cumulus.P2P.BusinessEntities;
using GEP.Cumulus.SmartCatalog.BusinessEntities.NewCatalog;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;

namespace GEP.Cumulus.P2P.Req.DataAccessObjects
{
    public interface IRequisitionDAO : P2P.DataAccessObjects.IP2PDocumentDAO
    {
        int GetRequisitionItemsCountByPartnerCode(long requisitionId, decimal partnerCode);
        long UpdateProcurementStatusByReqItemId(long requistionItemId);
        bool SaveRequisitionBusinessUnit(long documentCode, long buId);
        bool SaveDocumentBusinessUnit(long documentCode);
        string AddTemplateItemInReq(long documentCode, string templateIds, List<KeyValuePair<long, decimal>> items, long pasCode, int shiptoLocationId, int itemType, int inventorySource, string buIds, string shippingMethod, int precessionValue, int maxPrecessionforTotal, int maxPrecessionForTaxesAndCharges, string populateDefaultNeedByDate, int populateDefaultNeedByDateByDays);
        List<RequisitionSplitItems> GetRequisitionAccountingDetailsByItemId(long requisitionItemId, int pageIndex, int pageSize, int itemType, int precessionValue = 0, int precissionTotal = 0, int precessionValueForTaxAndCharges = 0);
        long SaveRequisitionAccountingDetails(List<RequisitionSplitItems> requisitionSplitItems, List<DocumentSplitItemEntity> requisitionSplitItemEntities, decimal lineItemQuantity, int precessionValue, int maxPrecessionforTotal, int maxPrecessionForTaxesAndCharges, bool UseTaxMaster = true, bool updateTaxes = true);
        //bool SaveDefaltAccountingSplit(long requisitionId, List<DocumentSplitItemEntity> requisitionSplitItemEntities);
        bool CheckAccountingSplitValidations(long requisitionId);
        //long GetDocumentIdByDocumentCode(long documentCode);
        Document GetDocumentDetailsById(long documentCode);
        bool CopyRequisitionToRequisition(long newrequisitionId, string requisitionIds, string buIds, int precissionValue = 0, int precissionTotal = 0, int precessionValueForTaxAndCharges = 0,bool showTaxJurisdictionForShipTo=false, List<KeyValuePair<long, decimal>> catlogItems = null, List<KeyValuePair<long, decimal>> itemMasteritems = null, bool enableGetLineItemsBulkAPI = false, List<CurrencyExchageRate> currencyExchageRates=null);
        DocumentIntegration.Entities.DocumentIntegrationEntity GetDocumentDetailsByDocumentCode(long documentCode, string documentStatuses = "", bool isFunctionalAdmin = false, int ACEntityId = 0, List<long> partners = null, List<DocumentIntegration.Entities.IntegrationTimelines> timelines = null, List<long> teammemberList = null);
        bool UpdateSendforBiddingDocumentStatus(long documentCode);
        List<long> GetAllCategoriesByReqId(long documentId);
        //KeyValuePair<long, decimal> GetAllEntitiesByReqId(long documentId, int entityTypeId);
        DataSet GetAllEntitiesByReqId(long documentId, int entityTypeId);
        //ICollection<TaxesAndCharges> GetAllTaxsAndCharges(long entityId, short entityLevel, int pageIndex, int pageSize);
        bool DeleteAllSplitsByDocumentId(long documentId);
        DataSet GetBasicDetailsByIdForNotification(long documentCode, long buyerPartnerCode, long CommentType = 1);
        long SaveAccountingApplyToAll(long requisitionId, List<DocumentSplitItemEntity> requisitionSplitItemEntities, int precessionValue, int maxPrecessionValueforTotal, int maxPrecessionValueForTaxesAndCharges, string allowOrgEntityInCatalogItems, long expenseCodeEntityId, bool allowTaxCodewithAmount, string supplierStatusForValidation);
        void ProrateHeaderTaxAndShipping(Requisition objRequisition);
        List<DocumentAdditionalEntityInfo> GetAllDocumentAdditionalEntityInfo(long documentCode);
        bool copyLineItem(long requisitionItemId, long requisitionId, int txtNumberOfCopies, int MaxPrecessionValue = 0, int MaxPrecessionValueTotal = 0, int MaxPrecessionValueForTaxAndCharges = 0);
        bool UpdateReqItemOnPartnerChange(long requisitionItemId, long partnerCode, int precessionValue, int maxPrecessionforTotal, int maxPrecessionForTaxesAndCharges);
        bool GetRequisitionItemAccountingStatus(long requisitionItemId);
        List<PartnerInfo> GetPreferredPartnerByReqItemId(long DocumentItemId, int pageIndex, int pageSize, string partnerName, out long partnerCode);
        List<ItemPartnerInfo> GetPreferredPartnerByCatalogItemId(long DocumentItemId, int pageIndex, int pageSize, string partnerName, string currencyCode, long entityDetailCode, out long partnerCode, string buList = "");
        bool CheckRequisitionCatalogItemAccess(long newrequisitionId, string requisitionIds);
        bool CheckOBOUserCatalogItemAccess(long requisitionId, long requesterId, int maxPrecessionValue, int maxPrecessionValueTotal, int maxPrecessionValueForTaxAndCharges, bool delItems = false);
        bool UpdateTaxOnLineItem(long requisitionItemId, ICollection<Taxes> lstTaxes, int precessionValue, int maxPrecessionValueForTotal = 0, int maxPrecessionValueForTaxAndCharges = 0);
        long UpdateCatalogOrgEntitiesToRequisition(long requistionId, long corporationEntityId, long expenseCodeEntityId);
        bool UpdateBillToLocation(long documentId, long entityDetailCode, long LOBEntityDetailCode);
        Requisition GetAllRequisitionDetailsByRequisitionId(long requisitionId, long userId, int typeOfUser, List<long> lineItemIds = null, Dictionary<string, string> settings = null);
        bool GetRequisitionCapitalCodeCountById(long documentId);
        //List<string> ValidateInterfaceRequisition(Requisition objRequisition, Dictionary<string, string> dctSettings, bool IsOrderingLocationMandatory = false, bool IsDefaultOrderingLocation = false);
        bool SaveContractInformation(long requisitionItemId, string extContractRef);
        ICollection<KeyValuePair<long, long>> GetAllPartnerCodeAndOrderinglocationId(long preDocumentId);
        List<KeyValuePair<long, int>> GetListErrorCodesByOrderIds(List<long> lstDocumentCode, bool isOrderingLocationMandatory);
        List<PartnerDetails> GetPartnerDetailsAndOrderingLocationByOrderId(long requisitionId);
        ExportRequisition RequisitionExportById(long requisitionId, long contactCode, int userType, string accessType, bool accessTypeComment, bool isAcc = false, int maxPrecessionValue = 0, int maxPrecessionValueForTotal = 0, int maxPrecessionValueForTaxAndCharges = 0, string enableDispatchMode = null);
        List<Questionnaire> GetAllQuestionnaire(long requisitionItemId);
        DataTable GetListofShipToLocDetails(string searchText, int pageIndex, int pageSize, bool getByID, int shipToLocID, long lOBEntityDetailCode, long entityDetailCode);
        DataTable GetListofBillToLocDetails(string searchText, int pageIndex, int pageSize, long entityDetailCode, bool getDefault, long lOBEntityDetailCode);
        DataTable CheckCatalogItemAccessForContactCode(long requisitionId, long requesterId);
        //BZRequisition GetRequisitionHeaderDetailsByIdForInterface(long requisitionId, bool deliverToFreeText = false);
        //List<RequisitionItem> GetLineItemBasicDetailsForInterface(long documentCode);
        //DataSet GetSplitsDetails(List<RequisitionItem> RequisitionItem, long contactCode, long lobEntityDetailCode, string EntityCode = null);

        //DataSet ValidateShipToBillToFromInterface(Requisition objRequisition, bool shipToLocSetting, bool billToLocSetting, bool deliverToFreeText, long LobentitydetailCode, bool IsDefaultBillToLocation, long entityDetailCode);
        void CalculateAndUpdateSplitDetails(long RequisitionId);
        bool InsertUpdateLineitemTaxes(Requisition objRequisition);
        bool UpdateRequisitionLineStatusonRFXCreateorUpdate(long documentCode, List<long> p2pLineItemId, Gep.Cumulus.CSM.Entities.DocumentType docType, bool IsDocumentDeleted = false);

        DataTable CheckCatalogItemsAccessForContactCode(long requesterId, string catalogItems);
        //bool DeleteSplitsByItemId(long RequisitionItemId, long documentId);
        //DataSet ProprateLineItemTaxandShipping(List<P2PItem> objItems, decimal Tax, decimal shipping, decimal AdditionalCharges, Int64 PunchoutCartReqId, int precessionValue = 0, int maxPrecessionforTotal = 0, int maxPrecessionForTaxesAndCharges = 0);
        ICollection<P2PItem> GetRequisitionItemsDispatchMode(long documentCode);
        bool UpdateRequisitionItemAutoSourceProcessFlag(string itemIds, int status);
        Requisition GetRequisitionDetailsByReqItems(List<long> reqItemIds);
        List<Taxes> GetRequisitioneHeaderTaxes(long requisitionId, int pageIndex, int pageSize);
        bool UpdateRequisitionHeaderTaxes(ICollection<Taxes> taxes, long requisitionId, int precessionValue, int precessionValueForTotal, int precessionValueForTaxesAndCharges, bool updateLineTax = false, int accuredTax = 1);
        bool UpdateRequisitionBuyerContactCode(List<KeyValuePair<long, long>> lstReqItemsToUpdate);
        List<NewP2PEntities.DocumentInfo> GetOrdersListForWorkBench(string reqItemIds);
        List<ItemCharge> GetLineItemCharges(List<long> reqItemIds, long documentCode);

        bool CheckBiddingInProgress(long documentId);
        long SaveChangeRequisitionRequest(int requisitionSource, long documentCode, string orderName, string orderNumber, DocumentSourceType documentSourceType, string revisionNumber, bool isCreatedFromInterface, bool byPassAccesRights = false, int PrecessionValue = 0, int Precessiontotal = 0, int MaxPrecessionValueForTaxAndCharges = 0, bool isFunctionalAdmin = false, bool documentActive = false);
        string GetRequisitionRevisionNumberByDocumentCode(long documentCode);


        int CopyRequisition(long SourceRequisitionId, long DestinationRequisitionId, long ContactCode, int PrecessionValue, int precessionValueForTotal, int precessionValueForTaxesAndCharges, out int eventPerformed, bool isAccountChecked = false, bool isCommentsChecked = false, bool isNotesAndAttachmentChecked = false, bool isAddNonCatalogItems = true, bool isCheckReqUpdate = false, bool IsCopyEntireReq = false, bool isNewNotesAndAttachmentChecked = false,string contractDocumentStatuses= "",bool showTaxJurisdictionForShipTo=false, List<KeyValuePair<long, decimal>> catlogItems = null, List<KeyValuePair<long, decimal>> itemMasteritems = null,List<CurrencyExchageRate> currencyExchageRates=null);

        long CancelChangeRequisition(long documentCode, long userId, int requisitionSource);



        //bool DeleteChargeAndSplitsItemsByItemChargeId(List<ItemCharge> lstItemCharge, long ChangeorderItemId);



        DataSet GetBuyerAssigneeDetails(long ContactCode, string SearchText, int StartIndex, int Size);
        long GetCreatorById(long documentCode);

        void DeleteSplitItemsByItemId(long ItemId);

        //DataSet ValidateItemDetailsToBeDerivedFromInterface(string itemNumber, string partnerSourceSystemValue, string uom);

        Dictionary<string, int> GetRequisitionPunchoutItemCount(long RequisitionId);
        long SyncChangeRequisition(long documentCode);
        long WithdrawChangeRequisition(long documentCode);

        bool GetReqSplitItemsEntityChangeFlag(long documentCode, string approvalEntityTypeIds, ref Requisition objReq);
        void SaveDocumentLinkInfo(Collection<DocumentLinkInfo> documentLinkInfoList);
        bool CheckWithDrawApprovedRequisition(long documentId);

        //List<long> GetRequisitionListForInterfaces(string docType, int docCount, int sourceSystemId);

        //DataSet ValidateInterfaceLineStatus(long buyerPartnerCode, DataTable dtRequisitionDetail);
        RequisitionLineStatusUpdateDetails UpdateRequisitionNotificationDetails(long requisitionId);
        bool UpdateRequisitionStatusForStockReservation(long requisitionId);

        P2PDocument OrderGetDocumentAdditionalEntityDetailsById(long documentCode);
        RequisitionItemCustomAttributes GetCutsomAttributesForLines(List<long> itemIds, int sourceDocType, int targetDocType, string level);
        RiskFormDetails GetRiskFormQuestionScore();
        RiskFormDetails GetRiskFormHeaderInstructionsText();
        //List<string> ValidateErrorBasedInterfaceRequisition(Requisition objRequisition, Dictionary<string, string> dctSettings);

        ICollection<NewP2PEntities.PartnerSpendControlDocumentMapping> GetAllPartnerCodeOrderinglocationIdNadSpendControlItemId(long documentId);

        //DataTable ValidateReqItemsForExceptionHandling(DataTable dtItemDetails);
        //void SaveRequisitionAdditionalDetailsFromInterface(long documentCode);

        Requisition GetRequisitionPartialDetailsById(long documentCode);

        long GetOrderLocationIdByClientLocationCode(string ClientLocationCode, long PartnerCode, string headerEntities, bool IsDefaultOrderingLocation);
        //void SaveAdditionalFieldAttributes(long documentID, long documentItemID, List<P2PAdditionalFieldAtrribute> lstAdditionalFieldAttributues, string PurchaseTypeDescription);
        BusinessEntities.Requisition GetRequisitionDetailsForCapitalBudget(long documentCode);
        DocumentIntegrationEntity GetDocumentDetailsBySelectedReqWorkbenchItems(DataTable dtReqItemIds, List<long> partners = null, List<DocumentIntegration.Entities.IntegrationTimelines> timelines = null, List<long> teammemberList = null);
        ItemSearchInput GetCatalogLineDetails(long documentId);
        ItemBulkInputRequest GetCatalogLineDetailsForWebAPI(long documentId);
        ItemSearchBulkInput GetCatalogLineDetailsForBulkWebAPI(long documentId, string requisitionIds = "");

        int UpdateCatalogLineDetails(List<CatalogItem> Items, long documentId);
        bool UpdateRequsitionItemStatus(List<KeyValuePair<long, long>> requisitionItemStatus);
        Dictionary<long, long> GetRequisitionByRequisitionItems(DataTable dtReqItemIds);
        List<CurrencyExchageRate> GetRequisitionCurrency(string RequisitionId);

        List<NewP2PEntities.RequisitionPartnerEntities> GetPartnerDetailsAndOrderingLocationById(long requisitionId, int spendControlType=0);
    }
}
