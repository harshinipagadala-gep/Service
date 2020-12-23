using Gep.Cumulus.CSM.BaseService;
using Gep.Cumulus.CSM.Config;
using Gep.Cumulus.CSM.Extensions;
using Gep.Cumulus.Partner.Entities;
using GEP.Cumulus.Logging;
using GEP.Cumulus.P2P.Req.BusinessObjects;
using GEP.Cumulus.Web.Utils;
using GEP.NewP2PEntities;
using GEP.NewPlatformEntities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.ServiceModel;

namespace GEP.Cumulus.P2P.Req.Service
{
    [ExcludeFromCodeCoverage]
    /// <summary>
    /// 2.0 Requisition service.
    /// </summary>
    [ServiceBehavior(IncludeExceptionDetailInFaults = true, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class NewRequisitionService : GepService, GEP.Cumulus.P2P.Req.ServiceContracts.INewRequisitionService
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="config">Configuration.</param>

        private static readonly log4net.ILog Log = Logger.GetLog(MethodBase.GetCurrentMethod().DeclaringType);

        public string JWTToken { get; set; }

        public NewRequisitionService(GepConfig config) : base(config)
        {

        }

        /// <summary>
        /// Saves requisition header.
        /// </summary>
        /// <param name="req">Requisition.</param>
        /// <returns>Result.</returns>
        public SaveResult SaveRequisitionHeader(GEP.NewP2PEntities.Requisition req)
        {
            return GetNewRequisitionManager().SaveRequisitionHeader(req);
        }

        /// <summary>
        /// Gets details of requisition for display.
        /// </summary>
        /// <param name="id">Id.</param>
        /// <returns>Requisition.</returns>
        public GEP.NewP2PEntities.Requisition GetRequisitionDisplayDetails(Int64 id, GEP.NewP2PEntities.Requisition requisition = null)
        {
            GEP.NewP2PEntities.Requisition result = null;
            try
            {
                result = GetNewRequisitionManager().GetRequisitionDisplayDetails(id, null, requisition);
                result = GetNewRequisitionManager().GetDefaultEntities(result);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, string.Format("Error occured in New Requisition Service GetRequisitionDisplayDetails Method DocumentCodes = ,", id.ToJSON()), ex);
                UtilsManager.ThrowHttpException(ex.Message);
                throw;
            }
            return result;
        }

        public GEP.NewP2PEntities.Requisition GetRequisitionDisplayDetailsForADR(Int64 id, GEP.NewP2PEntities.Requisition requisition = null)
        {
            var result = GetNewRequisitionManager().GetRequisitionDisplayDetails(id, null, requisition);
            return result;
        }
        /// <summary>
        /// saves the complete Req object including(header,Item,Split)
        /// </summary>
        /// <param name="req">req obj.</param>
        /// <returns>Result.</returns>
        public SaveResult SaveCompleteRequisition(GEP.NewP2PEntities.Requisition req)
        {
            return GetNewRequisitionManager().SaveCompleteRequisition(req);
        }

        public SaveResult AutoSaveDocument(GEP.NewP2PEntities.Requisition objectData, Int64 documentCode, int documentTypeCode, Int64 lastModifiedBy)
        {
            return GetNewRequisitionManager().AutoSaveDocument(objectData, documentCode, documentTypeCode, lastModifiedBy);
        }
        public int SaveBuyerAssignee(long[] DocumentCodes, long BuyerAssigneeValue, long PreviousBuyerAssignee)
        {
            try
            {
                return GetNewRequisitionManager().SaveBuyerAssignee(DocumentCodes, BuyerAssigneeValue, PreviousBuyerAssignee);
            }
            catch (Exception ex)
            {


                LogHelper.LogError(Log, string.Format("Error occured in Requisition SaveBuyerAssignee Method DocumentCodes = {0} ,BuyerAssigneeValue = {1},  PreviousBuyerAssignee = {2},", DocumentCodes.ToJSON(), BuyerAssigneeValue, PreviousBuyerAssignee), ex);
                UtilsManager.ThrowHttpException(ex.Message);


                throw;
            }

        }
        public GEP.NewP2PEntities.Requisition GetAutoSaveDocument(Int64 documentCode, int documentTypeCode)
        {
            GEP.NewP2PEntities.Requisition req = null;
            try
            {
                req = GetNewRequisitionManager().GetAutoSaveDocument(documentCode, documentTypeCode);

            }
            catch (Exception)
            {

            }
            return req;
        }

        /// <summary>
        /// GetUserConfigurations
        /// </summary>
        /// <param name="contactCode">contactCode</param>
        /// <param name="documentType">documentType</param>
        /// <returns></returns>
        public List<P2PUserConfiguration> GetUserConfigurations(long contactCode, int documentType)
        {
            return GetNewRequisitionManager().GetUserConfigurations(contactCode, documentType);
        }

        /// <summary>
        /// SaveUserConfigurations
        /// </summary>
        /// <param name="userConfig">userConfig</param>
        /// <returns></returns>
        public SaveResult SaveUserConfigurations(P2PUserConfiguration userConfig)
        {
            return GetNewRequisitionManager().SaveUserConfigurations(userConfig);
        }
        /// <summary>
        /// GetContractItems
        /// </summary>
        /// <param name="documentNumber">documentNumber</param>
        /// <returns></returns>
        public List<IdAndName> GetContractItemsByContractNumber(string documentNumber, string term, int itemType)
        {
            return GetNewRequisitionManager().GetContractItemsByContractNumber(documentNumber, term, itemType);
        }
        /// <summary>
        /// ValidateContractNumber
        /// </summary>
        /// <param name="contractNumber">contractNumber</param>
        /// <returns></returns>
        public bool ValidateContractNumber(string contractNumber)
        {
            return GetNewRequisitionManager().ValidateContractNumber(contractNumber);
        }

        public List<SavedViewDetails> GetSavedViewsForReqWorkBench(long LobId)
        {
            return GetNewRequisitionManager().GetSavedViewsForReqWorkBench(LobId);
        }
        public long InsertUpdateSavedViewsForReqWorkBench(SavedViewDetails objSavedView)
        {
            return GetNewRequisitionManager().InsertUpdateSavedViewsForReqWorkBench(objSavedView);
        }
        public bool DeleteSavedViewsForReqWorkBench(long savedViewId)
        {
            return GetNewRequisitionManager().DeleteSavedViewsForReqWorkBench(savedViewId);
        }
        public List<long> AutoCreateWorkBenchOrder(long RequisitionId, int processFlag, bool isautosubmit)
        {
            var context = this.GetCallerContext();

            var helper = new RequisitionServiceHelper(context);
            this.JWTToken = helper.GetToken();

            var executionHelper = new BusinessObjects.RESTAPIHelper.ExecutionHelper(context, this.Config, this.JWTToken);
            if (executionHelper.Check(11, BusinessObjects.RESTAPIHelper.ExecutionHelper.WebAPIType.Order))
            {
                var orderHelper = new BusinessObjects.RESTAPIHelper.OrderHelper(context, JWTToken);
                return orderHelper.AutoCreateWorkBenchOrder(RequisitionId, processFlag, isautosubmit);
            }
            else
            {
                Proxy.ProxyOrderService proxyOrderService = new Proxy.ProxyOrderService(this.GetCallerContext(), this.JWTToken);
                return proxyOrderService.AutoCreateWorkBenchOrder(RequisitionId, processFlag, isautosubmit);
            }
        }
        public bool AssignBuyerToRequisitionItems(long buyerContactCode, string requisitionItemIds)
        {
            return GetNewRequisitionManager().AssignBuyerToRequisitionItems(buyerContactCode, requisitionItemIds);
        }
        public List<BuyerInfo> GetAssignBuyersList(string organizationEntityIds, string documentCodes)
        {
            return GetNewRequisitionManager().GetAssignBuyersList(organizationEntityIds, documentCodes);
        }
        public long CreateOrderFromRequisitionItems(List<long> listItemIds, string supplierCode = "")
        {
            var context = this.GetCallerContext();

            var helper = new RequisitionServiceHelper(context);
            this.JWTToken = helper.GetToken();

            var executionHelper = new BusinessObjects.RESTAPIHelper.ExecutionHelper(context, this.Config, this.JWTToken);
            if (executionHelper.Check(12, BusinessObjects.RESTAPIHelper.ExecutionHelper.WebAPIType.Order))
            {
                var orderHelper = new BusinessObjects.RESTAPIHelper.OrderHelper(context, JWTToken);
                return orderHelper.CreateOrderFromRequisitionItems(listItemIds, supplierCode);
            }
            else
            {
                Proxy.ProxyOrderService proxyOrderService = new Proxy.ProxyOrderService(this.GetCallerContext(), this.JWTToken);
                return proxyOrderService.CreateOrderFromRequisitionItems(listItemIds, supplierCode);
            }

        }
        public List<DocumentInfo> GetOrdersListForWorkBench(string listItemIds)
        {
            var context = this.GetCallerContext();

            var helper = new RequisitionServiceHelper(context);
            this.JWTToken = helper.GetToken();

            var executionHelper = new BusinessObjects.RESTAPIHelper.ExecutionHelper(context, this.Config, this.JWTToken);
            if (executionHelper.Check(13, BusinessObjects.RESTAPIHelper.ExecutionHelper.WebAPIType.Order))
            {
                var orderHelper = new BusinessObjects.RESTAPIHelper.OrderHelper(context, JWTToken);
                return orderHelper.GetOrdersListForWorkBench(listItemIds);
            }
            else
            {
                Proxy.ProxyOrderService proxyOrderService = new Proxy.ProxyOrderService(this.GetCallerContext(), this.JWTToken);
                return proxyOrderService.GetOrdersListForWorkBench(listItemIds);
            }
        }

        public bool SaveReqItemstoExistingPO(List<long> listItemIds, long OrderId)
        {
            var context = this.GetCallerContext();

            var helper = new RequisitionServiceHelper(context);
            this.JWTToken = helper.GetToken();

            var executionHelper = new BusinessObjects.RESTAPIHelper.ExecutionHelper(context, this.Config, this.JWTToken);
            if (executionHelper.Check(14, BusinessObjects.RESTAPIHelper.ExecutionHelper.WebAPIType.Order))
            {
                var orderHelper = new BusinessObjects.RESTAPIHelper.OrderHelper(context, JWTToken);
                return orderHelper.SaveReqItemstoExistingPO(listItemIds, OrderId);
            }
            else
            {
                Proxy.ProxyOrderService proxyOrderService = new Proxy.ProxyOrderService(this.GetCallerContext(), this.JWTToken);
                return proxyOrderService.SaveReqItemstoExistingPO(listItemIds, OrderId);
            }
        }

        public List<KeyValuePair<string, string>> ValidateReqWorkbenchItems(string reqItemIds, byte validationType)
        {
            return GetNewRequisitionManager().ValidateReqWorkbenchItems(reqItemIds, validationType);
        }

        public List<GEP.NewP2PEntities.ASLData> ValidateAndGetASLbyReqItems(GEP.NewP2PEntities.ListInputData lstSupplierdetails)
        {
            var context = this.GetCallerContext();

            var helper = new RequisitionServiceHelper(context);
            this.JWTToken = helper.GetToken();

            var executionHelper = new BusinessObjects.RESTAPIHelper.ExecutionHelper(context, this.Config, this.JWTToken);
            if (executionHelper.Check(15, BusinessObjects.RESTAPIHelper.ExecutionHelper.WebAPIType.Order))
            {
                var orderHelper = new BusinessObjects.RESTAPIHelper.OrderHelper(context, JWTToken);
                return orderHelper.ValidateAndGetASLbyReqItems(lstSupplierdetails);
            }
            else
            {
                Proxy.ProxyOrderService proxyOrderService = new Proxy.ProxyOrderService(this.GetCallerContext(), this.JWTToken);
                return proxyOrderService.ValidateAndGetASLbyReqItems(lstSupplierdetails);
            }
        }

        public GEP.Cumulus.P2P.BusinessEntities.RequisitionTemplateResponse ExcelTemplateHandler(Int64 documentCode, string documentNumber, GEP.Cumulus.P2P.BusinessEntities.RequisitionExcelTemplateHandler action, Int64 fileID = 0)
        {
            return GetNewRequisitionManager().ExcelTemplateHandlerFromService(documentCode, documentNumber, action, fileID);
        }

        public List<NewP2PEntities.RequisitionItem> ValidateItemsOnBuChange(List<NewP2PEntities.RequisitionItem> reqItems, string buList, long LOBValue, string SourceType = "1")
        {
            return GetNewRequisitionManager().ValidateItemsOnBuChange(reqItems, buList, LOBValue, SourceType);
        }

        public Dictionary<string, string> SendRequisitionForReview(long documentCode, long contactCode, int documentTypeId, bool isBypassOperationalBudget = false)
        {
            return GetRequisitionDocumentManager().SendDocumentForReview(documentCode, contactCode, documentTypeId, isBypassOperationalBudget: isBypassOperationalBudget);
        }

        public Dictionary<string, string> AcceptOrRejectReview(long documentCode, bool isApproved, int documentTypeId, long LOBId = 0)
        {
            return GetNewRequisitionManager().AcceptOrRejectReview(documentCode, isApproved, documentTypeId, LOBId);
        }

        public Dictionary<string, string> AcceptOrRejectReviewValidations(long documentCode)
        {
            return GetNewRequisitionManager().AcceptOrRejectReviewValidations(documentCode);
        }

        /// <summary>
        /// Get Requisition Details List
        /// </summary>
        /// <param name="documentSearch">documentSearch</param>
        /// <returns>Requisition Object</returns>
        public List<NewP2PEntities.Requisition> GetRequisitionDetailsList(DocumentSearch documentSearch)
        {
            return GetNewRequisitionManager().GetRequisitionDetailsList(documentSearch);
        }

        public List<P2P.BusinessEntities.Questionnaire> GetAllQuestionnaireByFormCodes(long headerFormCode, long lineFormCode, List<P2P.BusinessEntities.Questionnaire> lstQuestionSetCodes)
        {

            return GetNewRequisitionManager().GetAllQuestionnaireByFormCodes(headerFormCode, lineFormCode, lstQuestionSetCodes);

        }

        public void saveBudgetoryStatus(DataTable validationResult, long documentCode)
        {
            GetNewRequisitionManager().saveBudgetoryStatus(validationResult, documentCode);
        }

        public void SendEmailToOrderContact(long documentCode, long BuyerAssigneeValue, long PrevOrderContact)
        {
            GetNewRequisitionManager().SendEmailToOrderContact(documentCode, BuyerAssigneeValue, PrevOrderContact);
        }

        public List<P2P.BusinessEntities.SplitAccountingFields> GetAllAccountingFieldsWithDefaultValuesFromCache(P2P.BusinessEntities.P2PDocumentType docType, P2P.BusinessEntities.LevelType levelType, long ContactCode = 0, long docmentCode = 0, List<P2P.BusinessEntities.DocumentAdditionalEntityInfo> lstHeaderEntityDetails = null, List<P2P.BusinessEntities.SplitAccountingFields> lstSplitAccountingFields = null, bool populateDefaultSplitValue = false, long documentItemId = 0, long lOBEntityDetailCode = 0, PreferenceLOBType preferenceLOBType = PreferenceLOBType.Serve, Document document = null)
        {
            return GetNewRequisitionManager().GetAllAccountingFieldsWithDefaultValuesFromCache(docType, levelType, ContactCode, docmentCode, lstHeaderEntityDetails, lstSplitAccountingFields, populateDefaultSplitValue, documentItemId, lOBEntityDetailCode, preferenceLOBType, document);
        }

        public bool UpdateLineStatusForRequisition(long RequisitionId, P2P.BusinessEntities.StockReservationStatus LineStatus, bool IsUpdateAllItems, List<P2P.BusinessEntities.LineStatusRequisition> Items)
        {
            return GetNewRequisitionManager().UpdateLineStatusForRequisition(RequisitionId, LineStatus, IsUpdateAllItems, Items);
        }

        public P2P.BusinessEntities.CustomAttributesFormData GetCustomAttrFormId(int docType, long LOBEntityDetailCode)
        {
            return GetNewRequisitionManager().GetCustomAttrFormId(docType, LOBEntityDetailCode);
        }

        public List<IdAndName> GetQuestionnaireByFormCode(List<P2P.BusinessEntities.Questionnaire> lstQuestionnair, long formCode, bool filterOnUser = true)
        {
            return GetNewRequisitionManager().GetQuestionnaireByFormCode(lstQuestionnair, formCode, filterOnUser);
        }

        public DataSet ValidateRequisitionData(long documentCode)
        {
            try
            {
                return GetNewRequisitionManager().ValidateRequisitionData(documentCode);

            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, string.Format("Error occured in Requisition ValidateRequisitionData Method documentCode", documentCode.ToJSON()), ex);
                UtilsManager.ThrowHttpException(ex.Message);

                throw ex;
            }
        }


        public DataTable GetAllPASCategories(string searchText, int pageSize = 10, int pageNumber = 1, long partnerCode = 0, long contactCode = 0)
        {
            return GetNewRequisitionManager().GetAllPASCategories(searchText, pageSize, pageNumber, partnerCode, contactCode);
        }

        public NewP2PEntities.Requisition GetRequisitionDetailsFromCatalog(Int64 documentCode)
        {
            try
            {
                return GetNewRequisitionManager().GetRequisitionDetailsFromCatalog(documentCode);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, string.Format("Error occured in Requisition GetRequisitionDetailsFromCatalog service Method", documentCode.ToJSON()), ex);
                UtilsManager.ThrowHttpException(ex.Message);

                throw ex;
            }
        }

        public List<UserDetails> GetUsersBasedOnUserDetailsWithPagination(UserDetails usersInfo, string searchText, int pageIndex, int pageSize, bool includeCurrentUser, string activityCodes, bool honorDirectRequesterForOBOSelection, bool isAutosuggest, bool isCheckCreateReqActivityForOBO)
        {
            try
            {
                return GetNewRequisitionManager().GetUsersBasedOnUserDetailsWithPagination(usersInfo, searchText, pageIndex, pageSize, includeCurrentUser, activityCodes, honorDirectRequesterForOBOSelection, isAutosuggest, isCheckCreateReqActivityForOBO);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, string.Format("Error occured in New Requisition GetUsersBasedOnUserDetailsWithPagination service Method", usersInfo.ToJSON()), ex);
                UtilsManager.ThrowHttpException(ex.Message);

                throw ex;
            }
        }

        public Documents.Entities.DocumentLOBDetails GetDocumentLOB(Int64 documentCode)
        {
            try
            {
                return GetNewRequisitionManager().GetDocumentLOBByDocumentCode(documentCode);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, string.Format("Error occured in Requisition GetDocumentLOB service Method", documentCode.ToJSON()), ex);
                UtilsManager.ThrowHttpException(ex.Message);

                throw ex;
            }
        }
        /// <summary>
        /////REQ-5561-As a user, If a VAB changes the Req amount beyond a certain delta, the original requester should review the Requisition again
        /// </summary>
        /// <param name="requisitionId,requisitionPreviousAmount">update requisition Previous Amount</param>
        /// <returns>nothing </returns>
        public void UpdateRequisitionPreviousAmount(long requisitionId, bool updateReqPrevAmount)
        {
            try
            {
                GetNewRequisitionManager().UpdateRequisitionPreviousAmount(requisitionId, updateReqPrevAmount);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, string.Format("Error occured in New Requisition UpdateRequisitionPreviousAmount service Method", requisitionId), ex);
                UtilsManager.ThrowHttpException(ex.Message);

                throw ex;
            }
        }

        /// <summary>
        /////REQ-5620 Team Member functionality for getting all users
        /// </summary>
        /// <param name="SearchText">SearchText</param>
        /// <returns>UserDetails Object</returns>
        public List<UserDetails> GetAllUsersByActivityCode(string SearchText, string Shouldhaveactivitycodes, string Shouldnothaveactivitycodes, long Partnercode)
        {
            try
            {
                return GetNewRequisitionManager().GetAllUsersByActivityCode(SearchText, Shouldhaveactivitycodes, Shouldnothaveactivitycodes, Partnercode);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in GetAllUsersByActivityCode params: " + SearchText + " , " + Shouldhaveactivitycodes + " , " + Shouldnothaveactivitycodes, ex);

                throw ex;
            }
        }

        public List<KeyValuePair<long, string>> GetCategoryHirarchyByCategories(List<long> categories)
        {
            try
            {
                return GetNewRequisitionManager().GetCategoryHirarchyByCategories(categories);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in GetCategoryHirarchyByCategories ", ex);

                throw ex;
            }
        }

        public bool EnableQuickQuoteRuleCheck(long documentCode)
        {
            return GetRequisitionRuleEngineManager().EnableQuickQuoteRuleCheck(documentCode);
        }

        public void ResetRequisitionItemFlipType(long requisitionId)
        {
            try
            {
                GetNewRequisitionManager().ResetRequisitionItemFlipType(requisitionId);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, string.Format("Error occured in New Requisition ResetRequisitionItemFlipType service Method", requisitionId), ex);
                UtilsManager.ThrowHttpException(ex.Message);

                throw ex;
            }
        }

        #region Get Managers
        private NewRequisitionManager GetNewRequisitionManager()
        {
            var context = this.GetCallerContext();
            var helper = new RequisitionServiceHelper(context);
            this.JWTToken = helper.GetToken();

            var bo = new NewRequisitionManager(JWTToken);
            bo.UserContext = context;
            bo.GepConfiguration = this.Config;
            return bo;
        }

        private RequisitionDocumentManager GetRequisitionDocumentManager()
        {
            var context = this.GetCallerContext();
            var helper = new RequisitionServiceHelper(context);
            this.JWTToken = helper.GetToken();

            var bo = new RequisitionDocumentManager(JWTToken);
            bo.UserContext = context;
            bo.GepConfiguration = this.Config;
            return bo;
        }

        private RequisitionRuleEngineManager GetRequisitionRuleEngineManager()
        {
            var context = this.GetCallerContext();
            var helper = new RequisitionServiceHelper(context);
            this.JWTToken = helper.GetToken();

            var bo = new RequisitionRuleEngineManager(JWTToken);
            bo.UserContext = context;
            bo.GepConfiguration = this.Config;
            return bo;
        }
        #endregion
    }
}
