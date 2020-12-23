using GEP.SMART.Configuration;
using System;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace GEP.Cumulus.P2P.Req.RestService.App_Start
{
    [ExcludeFromCodeCoverage]
    public static class UrlHelperExtensions
    {
        private const string UrlSeparator = "/";

        private static string _p2pScriptVersion = MultiRegionConfig.GetConfig(CloudConfig.ScriptVersion);

        private static string GetContentURL()
        {
            var contentURL = GetBlobUrl();
            if (!string.IsNullOrEmpty(contentURL) && !contentURL.EndsWith("/"))
                contentURL = string.Concat(contentURL, "/");
            return contentURL;
        }
        public static Uri ContentBlobUrl(string contentkey)
        {
            var blobUrl = new StringBuilder();
            blobUrl.Append(GetBlobUrl());
            blobUrl.Append(UrlSeparator);
            blobUrl.Append(contentkey);
            var blobUri = new Uri(blobUrl.ToString());

            return blobUri;
        }

        private static string GetBlobUrl()
        {
            var blobUrl = string.Empty;

            try
            {
                blobUrl = MultiRegionConfig.GetConfig(CloudConfig.GEPContentURL);
            }
            catch (Exception)
            {
                blobUrl = string.Empty;
            }

            if (string.IsNullOrEmpty(blobUrl))
                blobUrl = ConfigurationManager.AppSettings["GePContentUrl"];

            return blobUrl;
        }


        //public static string P2PBlobUrl(this System.Web.Mvc.UrlHelper helper, string contentSrc)
        //{
        //    return string.Concat(_gepContentUrl, contentSrc, "?ver=", _p2pScriptVersion);
        //}


        public static Uri P2PDocumentServiceUrl
        {
            get
            {
                return new Uri(MultiRegionConfig.GetConfig(CloudConfig.P2PDocumentServiceURL));
            }
        }

        public static Uri P2PCommonServiceUrl
        {
            get
            {
                return new Uri(MultiRegionConfig.GetConfig(CloudConfig.P2PCommonServiceURL));
            }
        }

        public static Uri RequisitionServiceUrl
        {
            get
            {
                return new Uri(MultiRegionConfig.GetConfig(CloudConfig.RequisitionServiceURL));
            }
        }

        // This property requires for proxy classes and will remove RequisitionServiceUrl Once all methods move to proxy classes. 
        public static string RequisitionServiceStringUrl
        {
            get
            {
                return MultiRegionConfig.GetConfig(CloudConfig.RequisitionServiceURL);
            }
        }

        public static Uri OrderServiceUrl
        {
            get
            {
                return new Uri(MultiRegionConfig.GetConfig(CloudConfig.OrderServiceURL));
            }
        }

        public static Uri OperationalBudgetServiceUrl
        {
            get
            {
                return new Uri(MultiRegionConfig.GetConfig(CloudConfig.OperationalBudgetServiceURL));
            }
        }


        // This property requires for proxy classes and will remove OrderServiceUrl Once all methods move to proxy classes. 
        public static string OrderServiceStringUrl
        {
            get
            {
                return MultiRegionConfig.GetConfig(CloudConfig.OrderServiceURL);
            }
        }

        public static Uri ItemServiceUrl
        {
            get
            {
                return new Uri(MultiRegionConfig.GetConfig(CloudConfig.ItemServiceURL));
            }
        }

        public static string TemplateServiceUrl
        {
            get
            {
                return MultiRegionConfig.GetConfig(CloudConfig.TemplateServiceURL);
            }
        }

        public static Uri WorkFlowRestUrl
        {
            get
            {
                return new Uri(MultiRegionConfig.GetConfig(CloudConfig.WorkFlowRestURL));
            }
        }

        public static string PortalDashBoardUrl
        {
            get
            {
                return MultiRegionConfig.GetConfig(CloudConfig.PortalDashBoardURL);
            }
        }

        public static string PartnerServiceUrl
        {
            get
            {
                return MultiRegionConfig.GetConfig(CloudConfig.PartnerServiceURL);
            }
        }
        public static int BuyerPartnerCode
        {
            get
            {
                return Convert.ToInt32(MultiRegionConfig.GetConfig(CloudConfig.BuyerPartnerCode), CultureInfo.InvariantCulture);
            }
        }

        public static Uri InvoiceServiceUrl
        {
            get
            {
                return new Uri(MultiRegionConfig.GetConfig(CloudConfig.InvoiceServiceURL));
            }
        }

        // This property requires for proxy classes and will remove InvoiceServiceUrl Once all methods move to proxy classes. 
        public static string InvoiceServiceStringUrl
        {
            get
            {
                return MultiRegionConfig.GetConfig(CloudConfig.InvoiceServiceURL);
            }
        }

        public static Uri ReceiptServiceUrl
        {
            get
            {
                return new Uri(MultiRegionConfig.GetConfig(CloudConfig.ReceiptServiceURL));
            }
        }

        // This property requires for proxy classes and will remove ReceiptServiceUrl Once all methods move to proxy classes. 
        public static string ReceiptServiceStringUrl
        {
            get
            {
                return MultiRegionConfig.GetConfig(CloudConfig.ReceiptServiceURL);
            }
        }

        public static string P2PCommonServiceStringUrl
        {
            get
            {
                return MultiRegionConfig.GetConfig(CloudConfig.P2PCommonServiceURL);
            }
        }

        public static Uri DocumentUrl
        {
            get
            {
                return new Uri(MultiRegionConfig.GetConfig(CloudConfig.P2PDocumentURL));
            }
        }

        public static Uri SettingsServiceUrl
        {
            get
            {
                return new Uri(MultiRegionConfig.GetConfig(CloudConfig.SettingServiceURL));
            }
        }

        public static Uri RFXUrl
        {
            get
            {
                return new Uri(!string.IsNullOrEmpty(MultiRegionConfig.GetConfig(CloudConfig.RFXURL)) ? MultiRegionConfig.GetConfig(CloudConfig.RFXURL).Replace('|', '&') : MultiRegionConfig.GetConfig(CloudConfig.RFXURL));
            }
        }

        public static Uri OrganizationServiceUrl
        {
            get
            {
                return new Uri(MultiRegionConfig.GetConfig(CloudConfig.OrganizationServiceURL));
            }
        }

        public static Uri IRServiceUrl
        {
            get
            {
                return new Uri(MultiRegionConfig.GetConfig(CloudConfig.IRServiceURL));
            }
        }

        // This property requires for proxy classes and will remove IRServiceUrl Once all methods move to proxy classes. 
        public static string IRServiceStringUrl
        {
            get
            {
                return MultiRegionConfig.GetConfig(CloudConfig.IRServiceURL);
            }
        }

        public static string P2PDocumentServiceStringUrl
        {
            get
            {
                return MultiRegionConfig.GetConfig(CloudConfig.P2PDocumentServiceURL);
            }
        }

        public static Uri SmartCatalogRestUrl
        {
            get
            {
                return new Uri(MultiRegionConfig.GetConfig(CloudConfig.SmartCatalogRestURL));
            }
        }
        public static Uri DownloadManagerServiceUrl
        {
            get
            {
                return new Uri(MultiRegionConfig.GetConfig(CloudConfig.DownloadManagerServiceURL));
            }
        }

        public static Uri ScannedImageServiceUrl
        {
            get
            {
                return new Uri(MultiRegionConfig.GetConfig(CloudConfig.ScannedImageServiceURL));
            }
        }

        public static Uri CatalogUrl
        {
            get
            {
                return new Uri(MultiRegionConfig.GetConfig(CloudConfig.CatalogURL));
            }
        }

        public static string GePContentUrl
        {
            get
            {
                return MultiRegionConfig.GetConfig(CloudConfig.GEPContentURL).TrimEnd();
            }
        }

        public static string BidandBuySourcingURL
        {
            get
            {
                return MultiRegionConfig.GetConfig(CloudConfig.BidandBuySourcingURL);
            }
        }

        public static string FlipTOOrderRestURL
        {
            get
            {
                return MultiRegionConfig.GetConfig(CloudConfig.FlipTOOrderRestURL);
            }
        }

        public static Uri PortalServiceUrl
        {
            get
            {
                return new Uri(MultiRegionConfig.GetConfig(CloudConfig.PortalServiceURL));
            }
        }

        public static Uri CSMServiceUrl
        {
            get
            {
                return new Uri(MultiRegionConfig.GetConfig(CloudConfig.CSMServiceURL));
            }
        }

        public static Uri OrderRestServiceEndPoint
        {
            get
            {
                return new Uri(MultiRegionConfig.GetConfig(CloudConfig.OrderRestServiceURL));
            }
        }

        public static string PortalURL
        {
            get
            {
                return MultiRegionConfig.GetConfig(CloudConfig.SmartBaseURL).TrimEnd('/');
            }
        }
        public static string StsUrl
        {
            get
            {
                return MultiRegionConfig.GetConfig(CloudConfig.STSURL);
            }
        }
        public static string IdpUrl
        {
            get
            {
                return MultiRegionConfig.GetConfig(CloudConfig.IDPURL);
            }
        }

        public static Uri P2PDocumentRestServiceEndPoint
        {
            get
            {
                return new Uri(MultiRegionConfig.GetConfig(CloudConfig.P2PDocumentRestServiceURL));
            }
        }

        public static Uri IRRestUrl
        {
            get
            {
                return new Uri(MultiRegionConfig.GetConfig(CloudConfig.IRRestURL));
            }
        }

        public static Uri CreditMemoServiceUrl
        {
            get
            {
                return new Uri(MultiRegionConfig.GetConfig(CloudConfig.CreditMemoServiceURL));
            }
        }

        public static Uri DynamicDiscountServiceUrl
        {
            get
            {
                return new Uri(MultiRegionConfig.GetConfig(CloudConfig.DynamicDiscountServiceURL));
            }
        }

        public static Uri ProgramServiceUrl
        {
            get
            {
                return new Uri(MultiRegionConfig.GetConfig(CloudConfig.ProgramServiceURL));
            }
        }
        public static Uri PaymentRequestServiceUrl
        {
            get
            {
                return new Uri(MultiRegionConfig.GetConfig(CloudConfig.PaymentRequestServiceURL));
            }
        }
        public static Uri NewRequisitionServiceUrl
        {
            get
            {
                return new Uri(MultiRegionConfig.GetConfig(CloudConfig.SmartRequisitionServiceURL));
            }
        }
        public static Uri NewOrderServiceUrl
        {
            get
            {
                return new Uri(MultiRegionConfig.GetConfig(CloudConfig.SmartOrderServiceURL));
            }
        }
        public static Uri InvoiceV2ServiceUrl
        {
            get
            {
                return new Uri(MultiRegionConfig.GetConfig(CloudConfig.InvoiceV2ServiceURL));
            }
        }
        public static Uri ServiceConfirmationServiceUrl
        {
            get
            {
                return new Uri(MultiRegionConfig.GetConfig(CloudConfig.ServiceConfirmationServiceURL));
            }
        }
        public static Uri ASNServiceUrl
        {
            get
            {
                return new Uri(MultiRegionConfig.GetConfig(CloudConfig.ASNServiceURL));
            }
        }
        public static Uri ASNRESTServiceUrl
        {
            get
            {
                return new Uri(MultiRegionConfig.GetConfig(CloudConfig.ASNRESTServiceURL));
            }
        }
    }
}
