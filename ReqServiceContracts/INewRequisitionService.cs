using Gep.Cumulus.Partner.Entities;
using GEP.NewP2PEntities;
using GEP.NewPlatformEntities;
using System;
using System.Collections.Generic;
using System.Data;
using System.ServiceModel;

namespace GEP.Cumulus.P2P.Req.ServiceContracts
{
    /// <summary>
    /// Contract for 2.0 Requisition service.
    /// </summary>
    [ServiceContract]
    public interface INewRequisitionService
    {
        /// <summary>
        /// Saves requisition header.
        /// </summary>
        /// <param name="req">Requisition.</param>
        /// <returns>Result.</returns>
        [OperationContract]
        SaveResult SaveRequisitionHeader(Requisition req);

        /// <summary>
        /// Gets details of requisition for display.
        /// </summary>
        /// <param name="id">Id.</param>
        /// <returns>Requisition.</returns>
        [OperationContract]
        Requisition GetRequisitionDisplayDetails(Int64 id, GEP.NewP2PEntities.Requisition requisition = null);

        [OperationContract]
        GEP.NewP2PEntities.Requisition GetRequisitionDisplayDetailsForADR(Int64 id, GEP.NewP2PEntities.Requisition requisition = null);

        /// <summary>
        /// saves the complete Req object including(header,Item,Split)
        /// </summary>
        /// <param name="req">req id</param>
        /// <returns>Result.</returns>
        [OperationContract]
        SaveResult SaveCompleteRequisition(Requisition req);


        /// <summary>
        /// automatically save the data on some time frame
        /// </summary>
        /// <param name="objectData"></param>
        /// <param name="documentCode"></param>
        /// <param name="documentTypeCode"></param>
        /// <param name="lastModifiedBy"></param>
        /// <returns></returns>
        [OperationContract]
        SaveResult AutoSaveDocument(Requisition objectData, Int64 documentCode, int documentTypeCode, Int64 lastModifiedBy);

        /// <summary>
        /// Gets the auto saved data.
        /// </summary>
        /// <param name="documentCode"></param>
        /// <param name="documentTypeCode"></param>
        /// <returns></returns>
        [OperationContract]
        Requisition GetAutoSaveDocument(Int64 documentCode, int documentTypeCode);
        /// <summary>
        /// SaveBuyerAssignee
        /// </summary>
        /// <param name="documentCode"></param>
        /// <param name="documentTypeCode"></param>
        /// <returns></returns>
        [OperationContract]
        int SaveBuyerAssignee(long[] DocumentCode, long BuyerAssigneeValue, long PreviousBuyerAssignee);
        /// <summary>
        /// Gets User Config
        /// </summary>
        /// <param name="contactCode">contactCode</param>
        /// <param name="documentType">documentType</param>
        /// <returns></returns>
        [OperationContract]
        List<P2PUserConfiguration> GetUserConfigurations(long contactCode, int documentType);

        /// <summary>
        /// Save User Config
        /// </summary>
        /// <param name="userConfig">userConfig</param>
        /// <returns></returns>
        [OperationContract]
        SaveResult SaveUserConfigurations(P2PUserConfiguration userConfig);

        /// <summary>
        /// Gets ContractItems
        /// </summary>
        /// <param name="documentNumber">documentNumber</param>
        /// <returns></returns>
        [OperationContract]
        List<IdAndName> GetContractItemsByContractNumber(string documentNumber, string term, int itemType);

        /// <summary>
        /// Gets ContractItems
        /// </summary>
        /// <param name="documentNumber">documentNumber</param>
        /// <returns></returns>
        [OperationContract]
        bool ValidateContractNumber(string contractNumber);

        [OperationContract]
        List<SavedViewDetails> GetSavedViewsForReqWorkBench(long LobId = 0);

        [OperationContract]
        long InsertUpdateSavedViewsForReqWorkBench(SavedViewDetails objSavedView);

        [OperationContract]
        bool DeleteSavedViewsForReqWorkBench(long savedViewId);
        [OperationContract]
        List<long> AutoCreateWorkBenchOrder(long RequisitionId, int processFlag, bool isautosubmit);
        [OperationContract]
        bool AssignBuyerToRequisitionItems(long buyerContactCode, string requisitionItemIds);
        [OperationContract]
        List<BuyerInfo> GetAssignBuyersList(string organizationEntityIds, string documentCodes);
        [OperationContract]
        long CreateOrderFromRequisitionItems(List<long> listItemIds, string supplierCode = "");

        [OperationContract]
        List<DocumentInfo> GetOrdersListForWorkBench(string listItemIds);

        [OperationContract]
        bool SaveReqItemstoExistingPO(List<long> listItemIds, long OrderId);

        [OperationContract]
        List<KeyValuePair<string, string>> ValidateReqWorkbenchItems(string reqItemIds, byte validationType);

        [OperationContract]
        List<GEP.NewP2PEntities.ASLData> ValidateAndGetASLbyReqItems(GEP.NewP2PEntities.ListInputData lstSupplierdetails);

        [OperationContract]
        GEP.Cumulus.P2P.BusinessEntities.RequisitionTemplateResponse ExcelTemplateHandler(Int64 documentCode, string documentNumber, GEP.Cumulus.P2P.BusinessEntities.RequisitionExcelTemplateHandler action, Int64 fileID = 0);

        [OperationContract]
        List<NewP2PEntities.RequisitionItem> ValidateItemsOnBuChange(List<NewP2PEntities.RequisitionItem> reqItems, string buList, long LOBValue, string SourceType = "1");

        [OperationContract]
        Dictionary<string, string> SendRequisitionForReview(long documentCode, long contactCode, int documentTypeId, bool isBypassOperationalBudget = false);

        [OperationContract]
        Dictionary<string, string> AcceptOrRejectReview(long documentCode, bool isApproved, int documentTypeId, long LOBId = 0);

        [OperationContract]
        Dictionary<string, string> AcceptOrRejectReviewValidations(long documentCode);

        [OperationContract]
        List<NewP2PEntities.Requisition> GetRequisitionDetailsList(DocumentSearch documentSearch);

        [OperationContract]
        List<P2P.BusinessEntities.Questionnaire> GetAllQuestionnaireByFormCodes(long headerFormCode, long lineFormCode, List<GEP.Cumulus.P2P.BusinessEntities.Questionnaire> lstQuestionSetCodes);

        [OperationContract]
        void saveBudgetoryStatus(DataTable validationResult, long documentCode);

        [OperationContract]
        void SendEmailToOrderContact(long documentCode, long BuyerAssigneeValue, long PrevOrderContact);

        [OperationContract]
        List<GEP.Cumulus.P2P.BusinessEntities.SplitAccountingFields> GetAllAccountingFieldsWithDefaultValuesFromCache(GEP.Cumulus.P2P.BusinessEntities.P2PDocumentType docType, GEP.Cumulus.P2P.BusinessEntities.LevelType levelType,
                                                                                       long ContactCode = 0, long docmentCode = 0,
                                                                                       List<GEP.Cumulus.P2P.BusinessEntities.DocumentAdditionalEntityInfo> lstHeaderEntityDetails = null,
                                                                                       List<GEP.Cumulus.P2P.BusinessEntities.SplitAccountingFields> lstSplitAccountingFields = null,
                                                                                       bool populateDefaultSplitValue = false, long documentItemId = 0, long lOBEntityDetailCode = 0,
                                                                                       Gep.Cumulus.Partner.Entities.PreferenceLOBType preferenceLOBType = Gep.Cumulus.Partner.Entities.PreferenceLOBType.Serve, NewPlatformEntities.Document document = null);

        [OperationContract]
        bool UpdateLineStatusForRequisition(long RequisitionId, P2P.BusinessEntities.StockReservationStatus LineStatus, bool IsUpdateAllItems, List<P2P.BusinessEntities.LineStatusRequisition> Items);

        [OperationContract]
        P2P.BusinessEntities.CustomAttributesFormData GetCustomAttrFormId(int docType, long LOBEntityDetailCode);

        [OperationContract]
        List<IdAndName> GetQuestionnaireByFormCode(List<GEP.Cumulus.P2P.BusinessEntities.Questionnaire> lstQuestionnair, long formCode, bool filterOnUser = true);

        [OperationContract]
        DataSet ValidateRequisitionData(long documentCode);

        [OperationContract]
        DataTable GetAllPASCategories(string searchText, int pageSize = 10, int pageNumber = 1, long partnerCode = 0, long contactCode = 0);

        [OperationContract]
        NewP2PEntities.Requisition GetRequisitionDetailsFromCatalog(Int64 documentCode);

        [OperationContract]
        List<UserDetails> GetUsersBasedOnUserDetailsWithPagination(UserDetails usersInfo, string searchText,
            int pageIndex, int pageSize, bool includeCurrentUser, string activityCodes,
            bool honorDirectRequesterForOBOSelection, bool isAutosuggest, bool isCheckCreateReqActivityForOBO);

        [OperationContract]
        Documents.Entities.DocumentLOBDetails GetDocumentLOB(Int64 documentCode);


        [OperationContract]
        void UpdateRequisitionPreviousAmount(long requisitionId, bool updateReqPrevAmount);


        [OperationContract]
        List<UserDetails> GetAllUsersByActivityCode(string SearchText, string Shouldhaveactivitycodes, string Shouldnothaveactivitycodes, long Partnercode);

        [OperationContract]
        List<KeyValuePair<long, string>> GetCategoryHirarchyByCategories(List<long> categories);

        [OperationContract]
        bool EnableQuickQuoteRuleCheck(long documentCode);

        [OperationContract]
        void ResetRequisitionItemFlipType(long requisitionId);

    }
}
