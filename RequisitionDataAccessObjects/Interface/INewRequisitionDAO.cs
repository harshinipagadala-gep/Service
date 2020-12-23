using Gep.Cumulus.CSM.Entities;
using Gep.Cumulus.Partner.Entities;
using GEP.Cumulus.P2P.BusinessEntities;
using GEP.Cumulus.QuestionBank.Entities;
using GEP.NewP2PEntities;
using GEP.NewPlatformEntities;
using RequisitionEntities;
using System;
using System.Collections.Generic;
using System.Data;

namespace GEP.Cumulus.P2P.Req.DataAccessObjects
{
    /// <summary>
    /// 2.0 Requisition DAO contract.
    /// </summary>
    public interface INewRequisitionDAO : P2P.DataAccessObjects.Usability.INewP2PDocumentDAO
    {
        /// <summary>
        /// Saves requisition header.
        /// </summary>
        /// <param name="req">Requisition.</param>
        /// <returns>Result.</returns>
        SaveResult SaveRequisitionHeader(NewP2PEntities.Requisition req, List<DocumentBU> documentBUs);

        /// <summary>
        /// Gets requisition in displayable format.
        /// </summary>
        /// <param name="id">Requisition id.</param>
        /// <returns>Requisition for display.</returns>
        NewP2PEntities.Requisition GetRequisitionDisplayDetails(Int64 id, List<long> reqLineItemIds = null, bool enableCategoryAutoSuggest = false);

        /// <summary>
        /// gets complete req obj
        /// </summary>
        /// <param name="documentCode"></param>
        /// <param name="documentTypeCode"></param>
        /// <returns></returns>
        NewP2PEntities.Requisition GetAutoSaveDocument(long documentCode, int documentTypeCode);

        /// <summary>
        /// save complete req obj
        /// </summary>
        /// <param name="objectData"></param>
        /// <param name="documentCode"></param>
        /// <param name="documentTypeCode"></param>
        /// <param name="lastModifiedBy"></param>
        /// <returns></returns>
        SaveResult AutoSaveDocument(NewP2PEntities.Requisition objectData, Int64 documentCode, int documentTypeCode, Int64 lastModifiedBy);

        /// <summary>
        /// GetUserConfigurations
        /// </summary>
        /// <param name="contactCode">contactCode</param>
        /// <param name="documentType">documentType</param>
        /// <returns></returns>
        List<P2PUserConfiguration> GetUserConfigurations(long contactCode, int documentType);

        /// <summary>
        /// SaveUserConfigurations
        /// </summary>
        /// <param name="userConfig">userConfig</param>
        /// <returns></returns>
        SaveResult SaveUserConfigurations(P2PUserConfiguration userConfig);
        /// <summary>
        /// GetContractItems
        /// </summary>
        /// <param name="documentNumber">documentNumber</param>
        /// <returns></returns>
        List<IdAndName> GetContractItemsByContractNumber(string documentNumber, string term, int itemType);

        /// <summary>
        /// Validate Contract Number
        /// </summary>
        /// <param name="contractNumber">contractNumber</param>
        /// <returns></returns>
        bool ValidateContractNumber(string contractNumber);

        /// <summary>
        /// Validate Contract Item Id
        /// </summary>
        /// <param name="contractNumber">contractNumber</param>
        /// <param name="ContractItemId">ContractItemId</param>
        /// <returns></returns>
        bool ValidateContractItemId(string contractNumber, long ContractItemId);

        List<SavedViewDetails> GetSavedViewsForReqWorkBench(long contactCode, Int16 documentTypeCode, long LobId);
        // <summary>
        /// InsertUpdateSavedViewsForReqWorkBench
        /// </summary>
        /// <param name="reqLineID">reqLineID</param>
        /// <returns></returns>
        long InsertUpdateSavedViewsForReqWorkBench(SavedViewDetails objSavedView);
        // <summary>
        /// DeleteSavedViewsForReqWorkBench
        /// </summary>
        /// <param name="reqLineID">reqLineID</param>
        /// <returns></returns>
        bool DeleteSavedViewsForReqWorkBench(long savedViewId);
        // <summary>
        /// AssignBuyerToRequisitionItems
        /// </summary>
        /// <param name="buyerContactCode">buyerContactCode</param>
        /// <param name="requisitionItemIds">requisitionItemIds</param>
        /// <returns>bool</returns>
        bool AssignBuyerToRequisitionItems(long buyerContactCode, string requisitionItemIds);

        // <summary>
        /// GetAssignBuyersList
        /// </summary>
        /// <param name="organizationEntityIds">organizationEntityIds</param>
        /// <param name="documentCodes">documentCodes</param>
        /// <returns>List<BuyerInfo></returns>
        List<BuyerInfo> GetAssignBuyersList(string organizationEntityIds, string documentCodes);
        void DeleteReqItemCharges(List<long> reqItemIds, long documentCode, long P2PLineItemId, bool IsHeaderLevelCharge, int MaxPrecessionValue, int MaxPrecessionValueForTaxesAndCharges, int MaxPrecessionValueforTotal);
        List<KeyValuePair<string, string>> ValidateReqWorkbenchItems(string reqItemIds, byte validationType, bool allowOneShipToLoation, bool showRemito, bool allowDeliverToFreeText);

        List<SplitAccountingFields> GetAllHeaderAccountingFieldsForRequisition(long docmentCode);
        bool UpdateLineStatusForRequisition(long RequisitionId, BusinessEntities.StockReservationStatus LineStatus, bool IsUpdateAllItems, List<BusinessEntities.LineStatusRequisition> Items);

        NewP2PEntities.Requisition GetRequisitionLineItemsByRequisitionId(long id);

        int SaveBuyerAssignee(long[] DocumentCode, long BuyerAssigneeValue);
        long SaveRequisitionUploadLog(RequisitionUploadLog reqUploadDetail);

        RequisitionResponse GetRequisitionUploadErrorResponse(long requisitionid, int requestType);
        void saveBudgetoryStatus(DataTable validationResult, long documentCode);
        void DeleteLineItems(long requisitionid, string commaSeperatedLineNos);

        bool SaveRequisitionAccountingDetails(List<P2P_RequisitionSplitItems> requisitionSplitItems, List<P2P_RequisitionSplitItemEntities> requisitionSplitItemEntities, decimal lineItemQuantity, int precessionValue, int maxPrecessionforTotal, int maxPrecessionForTaxesAndCharges, bool UseTaxMaster = true, bool updateTaxes = true);

        RequisitionUploadLog GetRequisitionUploadError(long requisitionid, int requestType);
        List<SplitAccountingFields> GetAllSplitAccountingCodesByDocumentType(LevelType levelType, long LobEntityDetailCode, int structureId);

        bool SaveBulkRequisitionItems(long requisitionId, List<GEP.Cumulus.P2P.BusinessEntities.RequisitionItem> lstReqItems, int maxPrecessionValue, int maxPrecessionValueForTotal, int maxPrecessionValueForTaxAndCharges, bool isCallFromWeb = false, int purchaseType = 0);
        SaveResult SaveAllRequisitionDetails(NewP2PEntities.Requisition req, int precessionValue, int maxPrecessionforTotal, int maxPrecessionForTaxesAndCharges, List<DocumentBU> documentBUs);

        List<NewP2PEntities.RequisitionItem> ValidateItemsOnBuChange(List<NewP2PEntities.RequisitionItem> objLstReqItems, string buList, long LOBId, string SourceType = "1");

        //bool UpdateLineStatusForRequisitionFromInterface(long RequisitionId, BusinessEntities.StockReservationStatus LineStatus, bool IsUpdateAllItems, List<BusinessEntities.LineStatusRequisition> Items, string StockReservationNumber);
        bool SaveSRFQuestionsforReqItemId(List<long> QuestionId, long documentCode);

        List<NewP2PEntities.Requisition> GetRequisitionDetailsList(DocumentSearch documentSearch);

        void CalculateAndUpdateRiskScore(long documentCode, bool isRiskFormMandatory);

        DataSet ValidateRequisitionData(long documentCode, DataTable dataTable);
        DataTable GetAllPASCategories(string searchText, int pageSize = 10, int pageNumber = 1, long partnerCode = 0, long contactCode = 0, int categorySelectionLevel = 0);

        NewP2PEntities.Requisition GetRequisitionDetailsFromCatalog(Int64 documentCode);

        int GetGroupIdOfVABUser(long contactCode);
        DataSet GetPartnersDocumetInterfaceInfo(List<long> partnerCodes);

        List<UserDetails> GetUsersBasedOnUserDetailsWithPagination(UserDetails usersInfo, string searchText,
            int pageIndex, int pageSize, bool includeCurrentUser, string activityCodes,
            bool honorDirectRequesterForOBOSelection, bool isAutosuggest, bool isCheckCreateReqActivityForOBO);

        Documents.Entities.DocumentLOBDetails GetDocumentLOBByDocumentCode(long documentCode);
        bool AddRequisitionsIntoSearchIndexerQueue(List<long> reqIds);
        void UpdateRequisitionPreviousAmount(long requisitionId, bool updateReqPrevAmount);
        List<UserDetails> GetAllUsersByActivityCode(string SearchText, string Shouldhaveactivitycodes, string Shouldnothaveactivitycodes, long Partnercode);
        List<PartnerLocation> GetOrderingLocationsWithNonOrgEntities(long partnerCode, int LocationTypeId, List<long> orgBUIds = null, List<long> nonOrgBUIds = null, string TempRemittoLocationKey = "", int pageIndex = 0, int pageSize = 10, string searchText = "");


        bool SaveTeamMember(GEP.Cumulus.Documents.Entities.Document objDocument);
        DataTable GetOrdersLinksForReqLineItem(long p2pLineItemId, long requisitionId);

        List<KeyValuePair<long, string>> GetRequesterAndAuthorFilter(int FilterFor, int pageSize, string term = "");
        DataTable GetCategoryHirarchyByCategories(List<long> categories);
        List<RequisitionPartnerInfo> GetAllBuyerSuppliersAutoSuggest(long BuyerPartnerCode, string Status, string SearchText, int PageIndex, int PageSize, string OrgEntityCodes, long LOBEntityDetailCode, string RestrictedSupplierRelationTypes, long PASCode, long ContactCode);
        List<PurchaseType> GetPurchaseTypes();

        List<ServiceType> GetPurchaseTypeItemExtendedTypeMapping();
        void UpdateRiskScore(long documentCode);

        int UpdateDocumentBudgetoryStatus(long documentCode, Int16 budgetoryStatus, List<BudgetAllocationDetails> lstbudgetAllocationIds = null);

        bool UpdateRequisitionItemStatusWorkBench(string IsCreatePOorRfx, DataTable dt, DataTable tableReqIds, long rfxId);
        long GetRequisitionItemRFxLink(long reqItemId);
        List<DocumentDelegation> GetDocumentsForUtility(long contactCode, string searchText);
        bool SaveDocumentRequesterChange(List<long> documentRequesterChangeList, long contactCode);
        bool PerformReIndexForDocuments(List<long> documentReIndexList);
        bool SaveReqItemBlanketMapping(List<BlanketItems> blanketUtilized, long documentItemId);
        IdNameAndAddress GetOrderingLoctionNameByLocationId(long locationId);
        List<BlanketDetails> GetBlanketDetailsForReqLineItem(long requisitionItemId);
        void SaveQuestionsResponse(List<QuestionResponse> lstQuestionsResponse, long docId);
        List<PartnerLocation> GetAllSupplierLocationsByOrgEntity(string OrgEntityCode, int PartnerLocationType = 2);

        bool UpdateRequisitionItemFlipType(BusinessEntities.Requisition objRequisition);

        long GetOrgEntityManagers(long orgEntityCode);
        long SaveDocumentDetails(GEP.Cumulus.Documents.Entities.Document document);
        Boolean UpdateRequisitionItemTaxJurisdiction(List<KeyValuePair<long, string>> lstItemTaxJurisdictions);

        List<User> GetUsersBasedOnEntityDetailsCode(string orgEntityCodes, long PartnerCode, int PageIndex, long ContactCode, int PageSize, string SearchText = "", string ActivityCodes = "");
        DataSet GetPartnerSourceSystemDetailsByReqId(long requisitionId);
        bool PushRequisitionToInterface(long requisitionId);

        List<BudgetAllocationDetails> GetBudgetDetails(long requisitionId);
        List<long> GetP2PLineitemIdbasedOnPartnerandCurrencyCode(long requisitionId, long partnerCode, long locationId, string currencyCode);
        long GetLobByEntityDetailCode(long entitydetailcode);
    }
}
