using System.Diagnostics.CodeAnalysis;

namespace GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper
{
    [ExcludeFromCodeCoverage]
    public static class ServiceURLs
    {
        public static string CommonServiceURL = "/P2PCommon/Api/Common/";
        public static string DocumentServiceURL = "/P2PDocument/Api/P2PDocument/";
        public static string PartnerServiceURL = "/Partner/Api/Partner/";
        public static string OrderServiceURL = "/Order/Api/OrderAPIService/";
        public static string OrderReqServiceURL = "/Order/Api/OrderReqAPIService/";
        public static string MotleyServiceURL = "/motley/api/v1/motley/";
        public static string OrganizationServiceURL = "/Org/api/v1/org/";
        public static string CurrencyServiceURL = "/currency/api/currency/";
        public static string QuestionBankServiceURL = "/questionbank/api/v1/questionbank/";
        public static string UserManagementServiceURL = "/usermanagement/api/UserManagement/";
        public static string CategoryServiceURL = "/category/api/v1/Category/";
        public static string SettingsServiceURL = "/settings/api/v1/settings/";
        public static string CommentsServiceURL = "/CommentsControl/api/Comments/";
        public static string CommonReferenceServiceURL = "/commonreferencedata/api/commonreferencedata/";
        public static string CustomAttributesURL = "customattributes/api/";
        public const string GetLineItemsFilter = "api/GetLineItemsFilter?oloc=559";
        public static string GetCustomAttributeValuesForUser = "/CustomAttributes/api/GetCustomAttributeValuesForUser?contactCode=";
    }
}
