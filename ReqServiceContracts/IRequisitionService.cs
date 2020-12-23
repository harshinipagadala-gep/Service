using Gep.Cumulus.CSM.Entities;
using Gep.Cumulus.Partner.Entities;
using GEP.Cumulus.DocumentIntegration.Entities;
using GEP.Cumulus.Documents.Entities;
using GEP.Cumulus.P2P.BusinessEntities;
using GEP.NewP2PEntities.FileManagerEntities;
using System;
using System.Collections.Generic;
using System.Data;
using System.ServiceModel;

namespace GEP.Cumulus.P2P.Req.ServiceContracts
{
    [ServiceContract]
    public interface IRequisitionService
    {
        [OperationContract]
        Requisition GetRequisitionBasicDetailsById(long requisitionId, long userId, int typeOfUser = 0);

        [OperationContract]
        Requisition GetRequisitionBasicAndValidationDetailsById(long requisitionId, long userId, int typeOfUser = 0, bool filterByBU = true, bool isFunctionalAdmin = false);

        [OperationContract]
        List<P2PDocumentValidationInfo> GetRequisitionValidationDetailsById(long requisitionId, bool isOnSubmit = false);

        [OperationContract]
        ICollection<RequisitionItem> GetRequisitionLineItemBasicDetails(long requisitionId, ItemType itemType, int startIndex, int pageSize, string sortBy, string sortOrder, int typeOfUser = 0);

        [OperationContract]
        string GenerateDefaultRequisitionName(long userId, long preDocumentId, long LOBEntityDetailCode);

        [OperationContract]
        long SaveRequisition(Requisition objRequisition, bool isShipToChanged);

        [OperationContract]
        long SaveRequisitionItem(RequisitionItem objRequisitionItem, bool isFunctionalAdmin = false);

        [OperationContract]
        long SaveReqItemWithAdditionalDetails(RequisitionItem objRequisitionItem, bool isTaxExempt, long LOBEntityDetailCode, bool allowPeriodUpdate = true, bool isFunctionalAdmin = false);

        [OperationContract]
        long SaveRequisitionItemPartnerDetails(RequisitionItem objRequisitionItem);

        [OperationContract]
        long SaveRequisitionItemShippingDetails(long reqLineItemShippingId, long requisitionItemId, string shippingMethod, int shiptoLocationId, int delivertoLocationId, decimal quantity, decimal totalQuantity, long userid, int precessionValue, int maxPrecessionforTotal, int maxPrecessionForTaxesAndCharges, bool prorateLineItemTax = true, string deliverTo = "");

        [OperationContract]
        long SaveRequisitionItemOtherDetails(RequisitionItem objRequisitionItem, bool allowTaxCodewithAmount, string supplierStatusForValidation);

        [OperationContract]
        void AddRequisitionInfoToPortal(Requisition objRequisition);

        [OperationContract]
        [ServiceKnownType(typeof(Documents.Entities.DocumentStatus))]
        bool UpdateRequisitionApprovalStatusById(long requisitionId, Documents.Entities.DocumentStatus approvalStatus);

        [OperationContract]
        [ServiceKnownType(typeof(Documents.Entities.DocumentStatus))]
        bool SaveRequisitionApproverDetails(long requisitionId, int approverId, Documents.Entities.DocumentStatus approvalStatus, string approveUri, string rejectUri, string instanceId);

        [OperationContract]
        ICollection<KeyValuePair<decimal, string>> GetAllPartnersOfRequisitionById(long requisitionId, string documentIds = "", string buIds = "");

        [OperationContract]
        ICollection<DocumentTrackStatusDetail> GetTrackDetailsofRequisitionById(long requisitionId);

        [OperationContract]
        ICollection<RequisitionItem> GetAllLineItemsByRequisitionId(long requisitionId, ItemType itemType, int pageIndex, int pageSize, int typeOfUser = 0);

        [OperationContract]
        bool DeleteLineItemByIds(string lineItemIds, int precessionValue, int maxPrecessionValueforTotal, int maxPrecessionValueForTaxesAndCharges, bool isAdvanced);

        [OperationContract]
        bool UpdateItemQuantity(long lineItemId, decimal quantity, int itemSource, int banding, decimal maxOrderQuantity, decimal minOrderQuantity);
        //bool SaveRequisitionApproverDetails(int requisitionId, int approverId, ApprovalStatus approvalStatus, string approveUri, string rejectUri, string instanceId);

        [OperationContract]
        [ServiceKnownType(typeof(RequisitionTrackStatus))]
        bool SaveRequisitionTrackStatusDetails(long requisitionId, string instanceId, long approverId, string approverName, string approverType, string approveUri, string rejectUri, DateTime statusDate, RequisitionTrackStatus approvalStatus, bool isDeleted);

        [OperationContract]
        RequisitionItem GetPartnerDetailsByLiId(long lineItemId);

        [OperationContract]
        ICollection<DocumentItemShippingDetail> GetShippingSplitDetailsByLiId(long lineItemId);

        [OperationContract]
        RequisitionItem GetOtherItemDetailsByLiId(long lineItemId);

        [OperationContract]
        long SaveCatalogRequisition(long userId, long reqId, string requisitionName, string requisitionNumber, long oboId = 0);

        //[OperationContract]
        //long GetDocumentCodeByRequisitionId(long requisitionId);

        [OperationContract]
        ICollection<string> ValidateDocumentByDocumentCode(long documentCode, long LOBEntityDetailCode);

        [OperationContract]
        long UpdateProcurementStatusByReqItemId(long requisitionItemId);

        [OperationContract]
        bool SaveRequisitionBusinessUnit(long documentCode, long buId);

        [OperationContract]
        bool SaveDocumentBusinessUnit(long documentCode);

        [OperationContract]
        ICollection<P2PDocument> GetAllRequisitionsForLeftPanel(long requisitionId, long userId, int pageIndex, int pageSize, string currencyCode, long orgEntityDetailCode, int purchaseTypeId);

        [OperationContract]
        long SaveRequisitionfromInterface(long buyerPartnerCode, int partnerInterfaceId, BZRequisition objRequisition);

        [OperationContract]
        bool AddTemplateItemInReq(long documentCode, string templateIds, List<KeyValuePair<long, decimal>> items, int itemType, string buIds);

        [OperationContract]
        List<SplitAccountingFields> GetAllSplitAccountingFields(P2PDocumentType docType, LevelType levelType, int structureId = 0, long LOBId = 0, long ACEEntityDetailCode = 0);

        [OperationContract]
        List<RequisitionSplitItems> GetRequisitionAccountingDetailsByItemId(long requisitionItemId, int pageIndex,
                                                                            int pageSize, int itemType, long LOBId);

        [OperationContract]
        long SaveRequisitionAccountingDetails(List<RequisitionSplitItems> requisitionSplitItems, List<DocumentSplitItemEntity> requisitionSplitItemEntities, decimal lineItemQuantity, bool updateTaxes, long LOBId);

        [OperationContract]
        bool DeleteRequisitionByDocumentCode(long documentCode);
        [OperationContract]
        bool UpdateLineStatusForRequisition(long RequisitionId, StockReservationStatus LineStatus, bool IsUpdateAllItems, List<P2P.BusinessEntities.LineStatusRequisition> Items);
        [OperationContract]
        bool CheckAccountingSplitValidations(long requisitionId);

        [OperationContract]
        List<long> AutoCreateOrder(long documentCode);

        [OperationContract]
        long CopyRequisitionToRequisition(long newrequisitionId, string requisitionIds, string buIds, long LOBId);

        [OperationContract]
        string GetDocumentDetailsByDocumentCode(long documentCode);


        [OperationContract]
        List<SplitAccountingFields> GetAllSplitAccountingFieldsWithDefaultValues(P2PDocumentType docType, LevelType levelType, long documentCode, long OnBehalfOfId, long documentItemId = 0, long lOBEntityDetailCode = 0, List<DocumentAdditionalEntityInfo> lstHeaderEntityDetails = null, PreferenceLOBType preferenceLOBType = PreferenceLOBType.Belong);

        [OperationContract]
        List<ADRSplit> GetAllSplitAccountingFieldsWithDefaultValuesForADR(P2PDocumentType docType, LevelType levelType, long documentCode, long OnBehalfOfId, List<long> documentItemId = null, long lOBEntityDetailCode = 0, List<DocumentAdditionalEntityInfo> lstHeaderEntityDetails = null, PreferenceLOBType preferenceLOBType = PreferenceLOBType.Belong, ADRIdentifier identifier = ADRIdentifier.None, object document = null);

        [OperationContract]
        void SendNotificationForApprovedRequisition(long requisition, string queryString);

        [OperationContract]
        bool UpdateSendforBiddingDocumentStatus(long documentCode);

        [OperationContract]
        List<long> GetAllCategoriesByReqId(long documentId);

        [OperationContract]
        KeyValuePair<long, decimal> GetAllEntitiesByReqId(long documentId, int entityTypeId);

        [OperationContract]
        bool DeleteAllSplitsByDocumentId(long documentId, long ContactCode, long LOBId);

        [OperationContract]
        void SendNotificationForRejectedRequisition(long requisition, ApproverDetails rejector, List<ApproverDetails> prevApprovers, string queryString);

        [OperationContract]
        void SendNotificationForRequisitionApproval(long documentCode, List<ApproverDetails> lstPendingApprover, List<ApproverDetails> lstPastApprover, string eventName, DocumentStatus documentStatus, string approvalType);

        [OperationContract]
        long SaveRequisitionAccountingApplyToAll(long requisitionId, List<DocumentSplitItemEntity> requisitionSplitItemEntities);


        [OperationContract]
        bool copyLineItem(long requisitionItemId, long requisitionId, int txtNumberOfCopies, long LOBId);

        [OperationContract]
        bool GetRequisitionItemAccountingStatus(long requisitionItemId);

        [OperationContract]
        bool UpdateReqItemOnPartnerChange(long requisitionItemId, long partnerCode, long LOBId);

        [OperationContract]
        List<PartnerInfo> GetPreferredPartnerByReqItemId(long DocumentItemId, int pageIndex, int pageSize, string partnerName, out long partnerCode);

        [OperationContract]
        List<ItemPartnerInfo> GetPreferredPartnerByCatalogItemId(long DocumentItemId, int pageIndex, int pageSize, string partnerName, string currencyCode, long entityDetailCode, out long partnerCode, string buList = "");

        [OperationContract]
        bool FinalizeComments(long documentCode, bool isIndexingRequired = true);

        [OperationContract]
        bool CheckRequisitionCatalogItemAccess(long newrequisitionId, string requisitionIds);

        [OperationContract]
        bool CheckOBOUserCatalogItemAccess(long requisitionId, long requesterId, bool delItems = false);

        [OperationContract]
        bool UpdateTaxOnLineItem(long requisitionItemId, ICollection<Taxes> lstTaxes, long LOBEntityDetailCode);

        [OperationContract]
        bool DeleteRequisitionItemsOnBUChange(long requisitionId, string buList, long LOBId);

        [OperationContract]
        Requisition GetAllRequisitionDetailsByRequisitionId(long requisitionId);

        [OperationContract]
        bool GetRequisitionCapitalCodeCountById(long requisitionId);

        [OperationContract]
        Dictionary<string, string> SentRequisitionForApproval(long contactCode, long documentCode, decimal documentAmount, int documentTypeId, string fromCurrency, string toCurrency, bool isOperationalBudgetEnabled, long headerOrgEntityCode, bool isBypassOperationalBudget = false);

        [OperationContract]
        List<string> ValidateInterfaceRequisition(long buyerPartnerCode, Requisition objRequisition);

        [OperationContract]
        Dictionary<string, string> SaveOfflineApprovalDetails(long contactCode, long documentCode, decimal documentAmount, string fromCurrency, string toCurrency, WorkflowInputEntities workflowEntity, long headerOrgEntityCode);

        [OperationContract]
        bool SaveContractInformation(long requisitionItemId, string extContractRef);

        [OperationContract]
        bool UpdateDocumentStatus(long documentCode, DocumentStatus docuemntStatus, bool isStockRequisition = false);

        [OperationContract]
        List<PartnerDetails> GetPartnerDetailsAndOrderingLocationByOrderId(long RequisitionId);

        [OperationContract]
        FileDetails RequisitionExportById(long requisitionId, long contactCode = 0, int userType = 0, string accessType = "4");

        [OperationContract]
        string GetDownloadPDFURL(long fileId);

        [OperationContract]
        string RequisitionPrintById(long fileId, long contactCode = 0, int userType = 0, string accessType = "4");

        [OperationContract]
        List<Questionnaire> GetAllQuestionnaire(long requisitionItemId);

        [OperationContract]
        Int64 SaveReqCartItemsFromInterface(Int64 UserId, Int64 PartnerCode, Int64 BuyerPartnerCode, List<P2P.BusinessEntities.RequisitionItem> lstP2PItem, int precessionValue, int PartnerConfigurationId, string DocumentCode, Int64 PunchoutCartReqId, decimal Tax = 0, decimal Shipping = 0, decimal AdditionalCharges = 0);

        [OperationContract]
        DataTable GetListofShipToLocDetails(string searchText, int pageIndex, int pageSize, bool getByID, int shipToLocID, long lOBEntityDetailCode, long entityDetailCode);

        [OperationContract]
        DataTable GetListofBillToLocDetails(string searchText, int pageIndex, int pageSize, long entityDetailCode, bool getDefault, long lOBEntityDetailCode = 0);

        [OperationContract]
        DataTable CheckCatalogItemAccessForContactCode(long requisitionId, long requesterId);



        [OperationContract]
        BZRequisition GetRequisitionDetailsById(long buyerPartnerCode, long documentCode);



        [OperationContract]
        DataTable CheckCatalogItemsAccessForContactCode(long requesterId, string catalogItems);

        [OperationContract]
        bool UpdateRequisitionLineStatusonRFXCreateorUpdate(long documentCode, List<long> p2pLineItemId, DocumentType docType, bool IsDocumentDeleted = false);

        [OperationContract]
        List<Taxes> GetRequisitioneHeaderTaxes(long requisitionId, int pageIndex, int pageSize);

        [OperationContract]
        bool UpdateRequisitionHeaderTaxes(ICollection<Taxes> taxes, long requisitionId, bool updateLineTax = false, int accuredTax = 1);

        [OperationContract]
        ICollection<RequisitionItem> GetRequisitionItemsDispatchMode(long documentCode);


        [OperationContract]
        int CopyRequisition(long SourceRequisitionId, long DestinationRequisitionId, long ContactCode, int PrecessionValue, bool isAccountChecked = false, bool isCommentsChecked = false, bool isNotesAndAttachmentChecked = false, bool isAddNonCatalogItems = true, bool isCheckReqUpdate = false, bool IsCopyEntireReq = false, bool isNewNotesAndAttachmentChecked = false);

        [OperationContract]
        bool CheckBiddingInProgress(long documentId);
        [OperationContract]
        long SaveChangeRequisitionRequest(int requisitionSource, long documentCode, string documentName, string documentNumber, bool isFunctionalAdmin = false, bool documentActive = false);


        [OperationContract]
        long CancelChangeRequisition(long documentCode, long userId, int requisitionSource);

        [OperationContract]
        List<long> AutoCreateWorkBenchOrder(long preDocumentId, int processFlag, bool isautosubmit);

        [OperationContract]
        DataSet GetBuyerAssigneeDetails(long ContactCode, string SearchText, int StartIndex, int Size);
        [OperationContract]
        bool validateDocumentBeforeNextAction(long DocumentId);

        [OperationContract]
        Dictionary<string, int> GetRequisitionPunchoutItemCount(long RequisitionId);

        [OperationContract]
        void SyncChangeRequisition(long RequisitionId);

        [OperationContract]
        void SendNotificationForRequisitionReview(long documentCode, List<ReviewerDetails> lstPendingReviewers, List<ReviewerDetails> lstPastReviewer, string eventName, DocumentStatus documentStatus, string reviewType);

        [OperationContract]
        void SendNotificationForReviewRejectedRequisition(long requisition, ReviewerDetails rejector, List<ReviewerDetails> prevReviewers, string queryString);

        [OperationContract]
        void SendReviewedRequisitionForApproval(Requisition requisition);

        [OperationContract]
        void SendNotificationForReviewAcceptedRequisition(long requisitionId, ReviewerDetails acceptor, string queryString);

        [OperationContract]
        bool SendNotificationForSkipOrOffLineRequisitionApproval(long documentCode, List<ApproverDetails> lstApprovers, int skipType = 0, bool isOffLine = false, long actionarId = 0);

        [OperationContract]
        DocumentIntegrationResults CreateRequisitionFromS2C(DocumentIntegrationEntity objDocumentIntegrationEntity);

        [OperationContract]
        List<long> GetRequisitionListForInterfaces(string docType, int docCount, int sourceSystemId);

        [OperationContract]
        DataSet ValidateInterfaceLineStatus(long buyerPartnerCode, DataTable dtRequisitionDetail);

        [OperationContract]
        void UpdateLineStatusForRequisitionFromInterface(List<RequisitionLineStatusUpdateDetails> reqDetails);

        [OperationContract]
        void UpdateRequisitionLineStatus(long requisitionId, long buyerPartnerCode);

        [OperationContract]
        bool PushingDataToEventHub(long Documentcode);

        [OperationContract]
        RequisitionItemCustomAttributes GetCutsomAttributesForLines(List<long> itemIds, int sourceDocType, int targetDocType, string level);

        [OperationContract]
        List<RequisitionItem> GetLineItemBasicDetailsForInterface(long documentCode);
        [OperationContract]
        RiskFormDetails GetRiskFormQuestionScore();
        [OperationContract]
        RiskFormDetails GetRiskFormHeaderInstructionsText();

        [OperationContract]
        void SendNotificationForSkipApproval(long documentCode, List<ApproverDetails> lstSkippedApprovers, List<ApproverDetails> lstFinalApprovers);

        [OperationContract]
        bool ConsumeReleaseCapitalBudget(long requisitionId, DocumentStatus documentStatus, bool isReConsume);

        [OperationContract]
        Boolean ReleaseBudget(long documentCode, long parentDocumentCode = 0);

        [OperationContract]
        Boolean CapitalBudgetValidation(long documentCode, Boolean isReConsume = false);

        [OperationContract]
        Requisition TestJWT(long requisitionId, long userId, int typeOfUser = 0);

    }
}
