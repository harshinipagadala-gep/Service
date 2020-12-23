using Gep.Cumulus.CSM.Entities;
using GEP.Cumulus.DocumentIntegration.Entities;
using GEP.Cumulus.P2P.BusinessEntities;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;


[assembly: System.CLSCompliant(false)]
namespace GEP.Cumulus.P2P.Req.RestServiceContracts
{
    [ServiceContract]
    public interface IRequisitionRestService
    {
        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.Wrapped, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "HealthCheck")]
        string HealthCheck();
        
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, UriTemplate = "UpdateRequisitionApprovalStatusById")]
        bool UpdateRequisitionApprovalStatusById(long requisitionId, string approvalStatus);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, UriTemplate = "SaveRequisitionApproverDetails")]
        bool SaveRequisitionApproverDetails(long requisitionId, int approverId, string approvalStatus, string approveUri, string rejectUri, string instanceId);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, UriTemplate = "GetAllLineItemsByRequisitionId")]
        string GetAllLineItemsByRequisitionId(long requisitionId, ItemType itemType, int pageIndex, int pageSize, int typeOfUser);

        //[OperationContract]
        //[WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, UriTemplate = "DeleteLineItemByIds")]
        //string DeleteLineItemByIds(string lineItemIds);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, UriTemplate = "UpdateItemQuantity")]
        string UpdateItemQuantity(long lineItemId, decimal quantity, int itemSource, int banding, decimal maxOrderQuantity, decimal minOrderQuantity);
        //bool SaveRequisitionApproverDetails(int requisitionId, int approverId, string approvalStatus, string approveUri, string rejectUri, string instanceId);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, UriTemplate = "SaveRequisitionTrackStatusDetails")]
        bool SaveRequisitionTrackStatusDetails(string GetPredictedPathResult);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, UriTemplate = "SaveRequisition")]
        string SaveRequisition(Requisition objRequisition);


        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, UriTemplate = "SaveRequisitionItem")]
        string SaveRequisitionItem(RequisitionItem objRequisitionItem);

        //[OperationContract]
        //[WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, UriTemplate = "SaveReqItemWithAdditionalDetails")]
        //string SaveReqItemWithAdditionalDetails(RequisitionItem objRequisitionItem);

        //[OperationContract]
        //[WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, UriTemplate = "GenerateDefaultRequisitionName")]
        //string GenerateDefaultRequisitionName();

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, UriTemplate = "GetRequisitionBasicDetailsById")]
        string GetRequisitionBasicDetailsById(long requisitionId);

        //[OperationContract]
        //[WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, UriTemplate = "GetRequisitionLineItemBasicDetails")]
        //string GetRequisitionLineItemBasicDetails(long requisitionId, ItemType itemType, int startIndex, int pageSize, string sortBy, string sortOrder, int typeOfUser);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, UriTemplate = "GetAllPartnersOfRequisitionById")]
        string GetAllPartnersOfRequisitionById(long requisitionId);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, UriTemplate = "SaveRequisitionItemPartnerDetails")]
        string SaveRequisitionItemPartnerDetails(RequisitionItem objRequisitionItem);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, UriTemplate = "SaveRequisitionItemShippingDetails")]
        string SaveRequisitionItemShippingDetails(long reqLineItemShippingId, long requisitionItemId, string shippingMethod, int shiptoLocationId, int delivertoLocationId, decimal quantity, decimal totalQuantity, bool prorateLineItemTax);

        //[OperationContract]
        //[WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json, UriTemplate = "SaveRequisitionItemOtherDetails")]
        //string SaveRequisitionItemOtherDetails(RequisitionItem objRequisitionItem);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, UriTemplate = "GetPartnerDetailsByLiId")]
        string GetPartnerDetailsByLiId(long lineItemId);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, UriTemplate = "GetShippingSplitDetailsByLiId")]
        string GetShippingSplitDetailsByLiId(long lineItemId);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, UriTemplate = "GetOtherItemDetailsByLiId")]
        string GetOtherItemDetailsByLiId(long lineItemId);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, UriTemplate = "GetTrackDetailsofRequisitionById")]
        string GetTrackDetailsofRequisitionById(long requisitionId);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, UriTemplate = "SaveRequisitionBusinessUnit")]
        bool SaveRequisitionBusinessUnit(long documentCode);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, UriTemplate = "ValidateDocumentByDocumentCode")]
        string ValidateDocumentByDocumentCode(long documentCode);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, UriTemplate = "GetAllSplitAccountingFields")]
        string GetAllSplitAccountingFields(P2PDocumentType docType, LevelType levelType);

        //[OperationContract]
        //[WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, UriTemplate = "GetRequisitionAccountingDetailsByItemId")]
        //string GetRequisitionAccountingDetailsByItemId(long requisitionItemId, int pageIndex,
        //                                                                    int pageSize);

        //[OperationContract]
        //[WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, UriTemplate = "SaveRequisitionAccountingDetails")]
        //string SaveRequisitionAccountingDetails(List<RequisitionSplitItems> requisitionSplitItems, List<DocumentSplitItemEntity> requisitionSplitItemEntities, decimal lineItemQuantity, bool updateTaxes);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, UriTemplate = "GetAllSplitAccountingControlValues")]
        string GetAllSplitAccountingControlValues(int entityTypeId, int parentEntityCode, string searchText);

        //[OperationContract]
        //[WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, UriTemplate = "GetDocumentCodeByRequisitionId")]
        //string GetDocumentCodeByRequisitionId(long requisitionId);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, UriTemplate = "GetAllQualifiedPartners")]
        string GetAllQualifiedPartners(string searchText, int pageIndex, int pageSize);
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, UriTemplate = "GetDocumentDetailsByDocumentCode")]
        string GetDocumentDetailsByDocumentCode(long documentCode);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, UriTemplate = "GetAllSplitAccountingFieldsWithDefaultValues")]
        string GetAllSplitAccountingFieldsWithDefaultValues(P2PDocumentType docType, LevelType levelType, long documentCode, long OnBehalfOfId);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, UriTemplate = "ApprovalCallBackMethod")]
        void ApprovalCallBackMethod(string eventName, long documentCode, string documentStatus, int wfDocTypeId, long contactCode, string userStatus, string approvalType, string returnEntity, string hierarchyIds = null);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, UriTemplate = "SendNotificationForApprovedRequisition")]
        void SendNotificationForApprovedRequisition(long documentCode);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json, UriTemplate = "GetAllCategoriesByReqId")]
        string GetAllCategoriesByReqId(long documentCode);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json, UriTemplate = "GetAllEntitiesByReqId")]
        string GetAllEntitiesByReqId(long documentCode, int entityTypeId);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, UriTemplate = "SendNotificationForRejectedRequisition")]
        void SendNotificationForRejectedRequisition(long documentCode, ApproverDetails rejector, List<ApproverDetails> prevApprover);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, UriTemplate = "GetSelectedRequisitionBasicDetailsById")]
        string GetSelectedRequisitionBasicDetailsById(long requisitionId);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, UriTemplate = "GetSelectedRequisitionLineItemDetailsById")]
        string GetSelectedRequisitionLineItemDetailsById(long requisitionId, ItemType itemType, int startIndex, int pageSize, string sortBy, string sortOrder, int typeOfUser);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, UriTemplate = "SentRequisitionForApproval")]
        Dictionary<string, string> SentRequisitionForApproval(long contactCode, long documentCode, decimal documentAmount, int documentTypeId, string fromCurrency, string toCurrency, bool isOperationalBudgetEnabled, long headerOrgEntityCode);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, UriTemplate = "AddTemplateItemInReq")]
        bool AddTemplateItemInReq(long documentCode, string templateIds, List<KeyValuePair<long, decimal>> items, int itemType, string buIds);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, UriTemplate = "SaveCatalogRequisition")]
        long SaveCatalogRequisition(long userId, long reqId, string requisitionName, string requisitionNumber, long oboId = 0);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, UriTemplate = "SaveOfflineApprovalDetails")]
        Dictionary<string, string> SaveOfflineApprovalDetails(long contactCode, long documentCode, decimal documentAmount, string fromCurrency, string toCurrency, WorkflowInputEntities workflowEntity, long headerOrgEntityCode);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, UriTemplate = "UpdateRequisitionLineStatusonRFXCreateorUpdate")]
        bool UpdateRequisitionLineStatusonRFXCreateorUpdate(long documentCode, List<long> p2pLineItemId, DocumentType docType, bool IsDocumentDeleted = false);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, UriTemplate = "ReviewCallBackMethod")]
        void ReviewCallBackMethod(string eventName, long documentCode, string documentStatus, int wfDocTypeId, long contactCode, string userStatus, string approvalType, string returnEntity);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, UriTemplate = "SkipCallBackMethod")]
        void SkipCallBackMethod(string eventName, long documentCode, string documentStatus, int wfDocTypeId, long contactCode, string userStatus, string approvalType, string returnEntity, List<ApproverDetails> lstApprovers, int skipType = 0, bool isOffLine = false, long actionarId = 0);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, UriTemplate = "CreateRequisitionFromS2C")]
        DocumentIntegrationResults CreateRequisitionFromS2C(DocumentIntegrationEntity objDocumentIntegrationEntity);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, UriTemplate = "GetRequisitionDisplayDetails")]
        string GetRequisitionDisplayDetails(Int64 id);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, UriTemplate = "GetAllRequisitionDetailsByRequisitionId")]
        string GetAllRequisitionDetailsByRequisitionId(long requisitionId);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, UriTemplate = "GetDocumentSplitAccountingFields")]
        string GetDocumentSplitAccountingFields(int docType, int levelType, long LOBId = 0, int structureId = 0);

        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, UriTemplate = "GetBasicSettingsValueByKeys")]
        string GetBasicSettingsValueByKeys(Dictionary<string, int> keys, long contactCode);

        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, UriTemplate = "PushingRequisitionToEventHub")]
        void PushingRequisitionToEventHub(long requisitionId, string consolidationAttributes, long contactCode, UserExecutionContext userExecutionContext = null);

    }
}
