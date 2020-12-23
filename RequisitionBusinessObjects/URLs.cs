using System.Diagnostics.CodeAnalysis;

namespace GEP.Cumulus.P2P.Req.BusinessObjects
{
    [ExcludeFromCodeCoverage]
    public static class URLs
    {
        /// <summary>
        /// API URL for Notification service
        /// </summary>
        public static string NotificationServiceURL
        {
            get { return "notificationrest/api/MailSender/"; }
        }

        /// <summary>
        /// Endpoint name for Send Email Notification Message
        /// </summary>
        public static string PostSendEmailNotificationMessage
        {
            get { return "SendEmailNotificationMessage"; }
        }

        /// <summary>
        /// Endpoint name for Log Email Notification Request
        /// </summary>
        public static string PostLogEmailNotificationRequest
        {
            get { return "LogEmailNotificationRequest"; }
        }


        public static string GetFileDetailsByFileId
        {
            get { return "/api/FileManager/GetFileDetailsByFileId/"; }
        }

        public static string GetFileUriByFileId
        {
            get { return "/api/FileManager/GetFileUriByFileId/"; }
        }

        public static string GetTableStorageConfiguration
        {
            get { return "/api/FileManager/GetTableStorageConfiguration"; }
        }

        public static string DownloadFileByFileUri
        {
            get { return "/api/FileManager/DownloadFileByFileUri?fileUri="; }
        }

        public static string UploadFileToTargetBlob
        {
            get { return "/api/FileManager/UploadFileToTargetBlob"; }
        }

        public static string GetCurrencyAutoSuggest
        {
            get { return "/currency/api/Currency/GetCurrencyAutoSuggest"; }
        }
        public static string GetMasterEntities
        {
            get { return "Budget/api/Budget/GetMasterEntities"; }
        }
        public static string ReleaseBudget
        {
            get { return "Budget/api/Consumption/ReleaseBudget"; }
        }
        public static string ConsumeBudget
        {
            get { return "Budget/api/Consumption/ConsumeBudget"; }
        }
        public static string CheckBudget
        {
            get { return "Budget/api/Consumption/CheckBudget"; }
        }
        public static string GetBudgetDetailsByAllocationIds
        {
            get { return "Budget/api/Budget/GetBudgetDetailsByAllocationIds"; }
        }
        public static string FillOrderFromRequisition
        {
            get { return "order/Api/OrderAPIService/FillOrderFromRequisition"; }
        }

        public static string OrderProcessing
        {
            get { return "order/Api/OrderAPIService/OrderProcessing"; }
        }
        public static string TaxCalculationForAutoSubmit
        {
            get { return "order/Api/OrderAPIService/TaxCalculationForAutoSubmit"; }
        }
        public static string AutoSendOrderForApproval
        {
            get { return "order/Api/OrderAPIService/AutoSendOrderForApproval"; }
        }
        public static string AutoSendOrdertoPartner
        {
            get { return "order/Api/OrderAPIService/AutoSendOrdertoPartner"; }
        }

        public static string ValidateDocumentBeforeNextAction
        {
            get { return "order/Api/OrderAPIService/ValidateDocumentBeforeNextAction"; }
        }
        public static string GetOrderBasicDetailsById
        {
            get { return "order/Api/OrderAPIService/GetOrderBasicDetailsById"; }
        }
        public static string ValidateDocumentFields
        {
            get { return "order/Api/OrderAPIService/ValidateDocumentFields"; }
        }
        public static string UpdateContractUtilization
        {
            get { return "order/Api/OrderAPIService/DocumentUpdateContractUtilization"; }
        }
        public static string AutoCreateWorkBenchOrderSettings
        {
            get { return "order/Api/OrderAPIService/AutoCreateWorkBenchOrderSettings"; }
        }
        public static string SendNotificationToReqForAutoOrderCreation
        {
            get { return "order/Api/OrderAPIService/SendNotificationToReqForAutoOrderCreation"; }
        }
        public static string UpdateConsumingDocumentStatusInBudget
        {
            get { return "Budget/api/ConsumingDocument/UpdateConsumingDocumentStatusInBudget"; }
        }

        public static string CurrencyServiceURL = "currency/api/Currency/";
        public static string GetCurrencyAutoSuggestFunction = "GetCurrencyAutoSuggest";
        public static string GetCurrencyExchangeRateByConversionDate = "GetCurrencyExchangeRateByConversionDate";
    public const string GetTransmissionMode = "api/Req/GetTransmissionModeBasedOnSetting?oloc=266";
    #region GetLineItemsWebAPI
    public const string GetLineItems = "api/GetLineItemsFilter?oloc=559";
    public const string GetLineItemsBulk = "api/GetLineItemsBulk?oloc=559";
    public const string GetLineItemAccess = "api/AccessSearchItems?oloc=290";
    public const string UseCaseForGetLineItems = "Req-CopyRequisition";
    public const string AppNameForGetLineItems = "Requisition";
    public const string UseCaseForOBOChange = "Req-OBOChange";
    public const string UseCaseForExcelUpload = "Req-ExcelUpload";
    public const string AccessSearchItems = "api/AccessSearchItems?oloc=290";
    public const string UseCaseForGetAdditionalFields = "RequisitionDocumentManager-GetRequisitionAdditionalFieldsData";
    public const string GetRequisitionAdditionalFieldsData = "api/Req/GetRequisitionAdditionalFieldsData?oloc=264";
    public const string GetPartnerCurrencies = "api/Req/GetPartnerCurrencies?oloc=264";
        #endregion
        public const string AppName = "Requisition";
        public const string GetAllChildContractsByContractNumber = "/contracts/api/GetAllChildContractsByContractNumber";
    public const string GetTaxJurisdictions = "api/Req/GetTaxJurisdcitionsByShiptoLocationIds?oloc=264";
    #region EnableCommentsManagerApiCall
    public const string DisableCommentsManagerApiCall = "NO";
        public const string EnableCommentsManagerApiCall = "YES";
        public const string GetComments = "1";
        public const string GetCommentAttachments = "2";
        public const string SaveComment = "3";
        public const string SaveCommentAttachments = "4";
        public const string DeleteComment = "5";
        public const string FinalizeComments = "6";
        public const string DeleteAttachment = "7";
        public const string DisableDeleteComments = "8";
    #endregion

    #region RFxFlipConstants

    internal const string COL_ITEM_NAME = "Item name";
    internal const string COL_ITEM_NUMBER = "Item number";
    internal const string COL_UNIT = "Unit";
    internal const string COL_UNITPRICE = "Baseline price per unit";
    internal const string COL_VOLUME = "Volume";
    internal const string COL_LINE_NUMBER = "Line number";
    internal const string COL_INCOTERMS_CODE = "Incoterms Code";
    internal const string COL_INCOTERMS_LOCATION = "Incoterms Location";
    internal const string COL_DOCUMENT_CODE = "DocumentCode";
    internal const string COL_INCOTERM_ID = "IncotermId";
    internal const string COL_P2PLINEITEMID = "P2PLineItemId";
    internal const string COL_CURRENCY_CODE = "CurrencyCode";
    internal const string COL_TOTAL_PRICE = "Total Price";
    internal const string COL_TAX_AMOUNT = "TaxAmount";
    internal const string COL_SUPPLIERITEMNUMBER = "Supplier Item Number";
    internal const string COL_ITEM_DESCRIPTION = "Item Description";
    internal const string COL_ITEM_SPECIFICATION = "Item Specification";
    internal const string COL_Category = "Category";
    internal const string COL_Category_ID = "Category Id";
    internal const string COL_CLIENT_PASCODE = "ClientPasCode";    

    internal const string COL_TEXT_TYPE = "Text";
    internal const string COL_DROPDOWN_TYPE = "Drop Down";
    internal const string COL_CURRENCY_TYPE = "Currency";
    internal const string COL_NUMERIC = "Numeric";
    internal const string COL_Incoterms = "Incoterms";
    internal const string COL_EXTENDED_TEXT = "Extended Text (12K)";    

        #endregion
    }
}
