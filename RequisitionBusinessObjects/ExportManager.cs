using Gep.Cumulus.CSM.BaseDataAccessObjects;
using Gep.Cumulus.CSM.Entities;
using Gep.Cumulus.Partner.Entities;
using GEP.Cumulus.Documents.DataAccessObjects.SQLServer;
using GEP.Cumulus.Documents.Entities;
using GEP.Cumulus.Logging;
using GEP.Cumulus.P2P.BusinessEntities;
using GEP.Cumulus.P2P.BusinessEntities.ExportDataSetEntities;
using GEP.Cumulus.P2P.BusinessObjects;
using GEP.Cumulus.P2P.DataAccessObjects;
using GEP.Cumulus.P2P.DataAccessObjects.SQLServer;
using GEP.Cumulus.P2P.Req.BusinessObjects.Proxy;
using GEP.Cumulus.QuestionBank.Entities;
using GEP.SMART.Configuration;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using GEP.Platform.FileManagerHelper;
using GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper;
using GEP.Cumulus.P2P.Req.BusinessObjects.Entities;
using FileManagerEntities = GEP.NewP2PEntities.FileManagerEntities;

namespace GEP.Cumulus.P2P.Req.BusinessObjects
{
    public class ExportManager : RequisitionBaseBO
    {
        private static readonly ILog Log = Logger.GetLog(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly string oldFonts = "Times New Roman";
        private string stgBillableFieldAvailable;
        private bool stgAllowLineNumber;
        private bool stgAllowManufacturersCodeAndModel;
        private int maxPrecTot;
        private int maxPrecTax;
        private int maxPrec;
        private int srNo = 1;
        private int srNoMaterial = 1;
        private int srNoServicenew = 1;
        private int srNoAdvancenew = 1;
        private bool stgShowStanProcDetails;
        private BlockingCollection<KeyValuePair<long, StringBuilder>> tempMaterialList = new BlockingCollection<KeyValuePair<long, StringBuilder>>();
        private BlockingCollection<KeyValuePair<long, StringBuilder>> tempServiceList = new BlockingCollection<KeyValuePair<long, StringBuilder>>();
        private BlockingCollection<KeyValuePair<long, StringBuilder>> tempList = new BlockingCollection<KeyValuePair<long, StringBuilder>>();
        private BlockingCollection<KeyValuePair<long, StringBuilder>> tempAdvanceList = new BlockingCollection<KeyValuePair<long, StringBuilder>>();
        private BlockingCollection<KeyValuePair<long, StringBuilder>> tempContingentWorkerList = new BlockingCollection<KeyValuePair<long, StringBuilder>>();

        public ExportManager(string jwtToken) : base(jwtToken)
        {

        }
        public Contact UserContact { get; set; }

        public string GetLogoUrl()
        {
            return MultiRegionConfig.GetConfig(CloudConfig.BlobURL) + "cumuluscontent/logo/" + UserContext.BuyerPartnerCode + "_logo.jpg";
        }

        public byte[] UpdateTemplateFonts(byte[] templateHTML, long lobId = 0)
        {
            string html = System.Text.Encoding.UTF8.GetString(templateHTML);

            //get from setting later
            string fonts = GetClientFonts(lobId);
            if (!String.IsNullOrEmpty(fonts))
            {
                html = html.Replace(oldFonts, fonts);
            }

            return Encoding.UTF8.GetBytes(html);
        }

        protected int GetClientHeaderFooterSize(long lobID = 0)
        {
            RequisitionCommonManager commonManager = new RequisitionCommonManager (this.JWTToken){ UserContext = UserContext, GepConfiguration = GepConfiguration };
            string setting = Convert.ToString(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "ClientSettingForExport", UserContext.UserId, (int)SubAppCodes.P2P, "", lobID));
            try
            {
                if (String.IsNullOrEmpty(setting))
                {
                    return 0;
                }
                else if (setting.Split('|').Length > 1)
                {
                    return Convert.ToInt32(setting.Split('|')[0]);
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occurred in GetClientHeaderFooterSize method for ClientSettingForExport = " + setting, ex);
                return 0;
            }

        }

        protected int GetClientHeaderSize(long lobID = 0)
        {
            RequisitionCommonManager commonManager = new RequisitionCommonManager (this.JWTToken){ UserContext = UserContext, GepConfiguration = GepConfiguration };
            string setting = Convert.ToString(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "ClientSettingForExport", UserContext.UserId, (int)SubAppCodes.P2P, "", lobID));
            try
            {
                if (String.IsNullOrEmpty(setting))
                {
                    return 0;
                }
                else if (setting.Split('|').Length > 2)
                {
                    return Convert.ToInt32(setting.Split('|')[2]); ;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occurred in GetClientHeaderFooterSize method for ClientSettingForExport = " + setting, ex);
                return 0;
            }
        }

        protected int GetFooterPageIndexValue(long lobID = 0)
        {
            RequisitionCommonManager commonManager = new RequisitionCommonManager (this.JWTToken){ UserContext = UserContext, GepConfiguration = GepConfiguration };
            string setting = Convert.ToString(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "ClientSettingForExport", UserContext.UserId, (int)SubAppCodes.P2P, "", lobID));
            try
            {
                if (String.IsNullOrEmpty(setting))
                {
                    return 0;
                }
                else if (setting.Split('|').Length > 3)
                {
                    return Convert.ToInt32(setting.Split('|')[3]);
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occurred in GetClientHeaderFooterSize method for ClientSettingForExport = " + setting, ex);
                return 0;
            }
        }

        private string GetClientFonts(long lobId = 0)
        {
            RequisitionCommonManager commonManager = new RequisitionCommonManager (this.JWTToken){ UserContext = UserContext, GepConfiguration = GepConfiguration };
            string setting = Convert.ToString(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "ClientSettingForExport", UserContext.UserId, (int)SubAppCodes.P2P, "", lobId));
            try
            {
                if (String.IsNullOrEmpty(setting))
                {
                    return "";
                }
                else if (setting.Split('|').Length > 1)
                {
                    return setting.Split('|')[1];
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occurred in GetClientHeaderFooterSize method for GetClientFonts = " + setting, ex);
                return string.Empty;
            }

        }

        public int GetPrecisionVal()
        {
            var commonManager = new RequisitionCommonManager (this.JWTToken){ UserContext = UserContext, GepConfiguration = GepConfiguration };
            return Convert.ToInt32(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "MaxPrecessionValue", UserContext.UserId));

        }

        public string CreateAddress(string partnerName, string addressLine1, string addressLine2, string addressLine3, string city, string state, string country, string zipCode)
        {
            var primaryAddress = string.IsNullOrEmpty(partnerName)
                                     ? string.Empty
                                     : ", " + partnerName.Trim();
            primaryAddress += string.IsNullOrEmpty(addressLine1)
                                     ? string.Empty
                                     : ", " + addressLine1.Trim();
            primaryAddress += string.IsNullOrEmpty(addressLine2)
                                  ? string.Empty
                                  : ", " + addressLine2.Trim();
            primaryAddress += string.IsNullOrEmpty(addressLine3)
                                  ? string.Empty
                                  : ", " + addressLine3.Trim();
            primaryAddress += string.IsNullOrEmpty(city)
                                  ? string.Empty
                                  : ", " + city.Trim();
            primaryAddress += string.IsNullOrEmpty(state)
                                  ? string.Empty
                                  : ", " + state;
            primaryAddress += string.IsNullOrEmpty(country)
                                  ? string.Empty
                                  : ", " + country;
            primaryAddress += string.IsNullOrEmpty(zipCode)
                                  ? string.Empty
                                  : ", " + zipCode;
            primaryAddress.TrimEnd(',');
            if (primaryAddress != string.Empty)
                primaryAddress = primaryAddress.Substring(1);

            return primaryAddress;
        }

        public string EmailAndFaxFormat(string emailAddress, string faxNo)
        {
            var primaryAddress = string.IsNullOrEmpty(emailAddress)
                                     ? string.Empty
                                     : emailAddress.Trim();
            primaryAddress += !string.IsNullOrEmpty(primaryAddress) ? (string.IsNullOrEmpty(faxNo) ? string.Empty : "/ " + faxNo.Trim())
                                     : faxNo.Trim();
            return primaryAddress;
        }

        public string ConvertToString(decimal value)
        {
            return FormatNumber(value, GetPrecisionVal());
        }


        public string ConvertToNegativeString(decimal value)
        {
            return Math.Round(value, GetPrecisionVal()).ToString();
        }

        public string ConvertToStringForQuantity(decimal value)
        {
            return FormatNumber(value, GetPrecisionVal());
        }

        public string FormatNumber(decimal value, int maxPrecisionVal)
        {
            CultureInfo cultureInfo = new CultureInfo(UserContact != null && UserContact.Address != null && UserContact.Address.CountryInfo != null
                && UserContact.Address.CountryInfo.CountryCultureInfo != null ? UserContact.Address.CountryInfo.CountryCultureInfo : this.UserContext.Culture.Trim());
            cultureInfo.NumberFormat.CurrencySymbol = "";//not to print with currency symbol

            string result = string.Empty;

            if (value > 0)
            {
                decimal number = Math.Round(value, maxPrecisionVal);
                string roundTripped = number.ToString().TrimEnd('0');

                if (!roundTripped.Contains('.') || roundTripped.Contains('.') && roundTripped.Split('.')[1].Length <= 1)
                    result = number.ToString("c2", cultureInfo);
                else
                    result = number.ToString(string.Format("c{0}", roundTripped.Split('.')[1].Length), cultureInfo);
            }
            else
            {
                result = Convert.ToDecimal(0).ToString("c" + 2, cultureInfo);
            }

            return result.Trim();
        }



        public string GetSectionHtml(string strHtml, string strSectionStartTag, string strSectionEndTag)
        {
            var sectionStartIndex = strHtml.IndexOf(strSectionStartTag, StringComparison.Ordinal);
            var sectionEndIndex = strHtml.IndexOf(strSectionEndTag, StringComparison.Ordinal);
            var strSectionHtml = string.Empty;
            if (sectionStartIndex >= 0 && sectionEndIndex >= 0)
            {
                var sectionEndLength = sectionEndIndex + strSectionEndTag.Length - sectionStartIndex;
                strSectionHtml = strHtml.Substring(sectionStartIndex, sectionEndLength);
            }
            return strSectionHtml;
        }

        public string GetDocumentStatusById(int statusId, P2PDocumentType docType = P2PDocumentType.None)
        {
            switch (statusId)
            {
                case 0: return "None";
                case 1: return "Draft";
                case 21: return "Approval Pending";
                case 22: return "Approved";
                case 23: return "Rejected";
                case 24: return "Withdrawn";
                case 25: return "Supplier Acknowledged";
                case 26: return "Acknowledgement Pending";
                case 27: return "Partially Accepted";
                case 28: return "Accepted";
                case 41: return "Sent To Supplier";
                case 42: return "Sent To Buyer";
                case 43: return "Sent For Acceptance";
                case 44: return "Published";
                case 53: return "Sent To Requester";
                case 54: return "Sent To Receiver";
                case 55: return "Sent Back";
                case 56: return "Sent For Bidding";
                case 61: return "Partially Ordered";
                case 62: return "Ordered";
                case 63: return "Partial Receipt";
                case 64: return "Full Receipt";
                case 65: return "Return Receipt";
                case 66: return "Excess Receipt";
                case 67: return "Exception";
                case 68: return "Matched";
                case 69: return "Non Procurable";
                case 70: return "Invoiced Status";
                case 77: return "Matched with Tolerance";
                case 101: return "Ready For Payment";
                case 102: return "Sent For Payment";
                case 103: return "Payment Failure";
                case 104: return "Invoice Paid";
                case 105: return "Invoice Paid With Remittance";
                case 121: return docType == P2PDocumentType.Invoice ? "Returned" : "Cancelled";
                case 122: return "Deleted";
                case 141: return "Sending In progress";
                case 142: return "Sending To Supplier Failed";
                case 151: return "Send For Processing";
                case 169: return "Send For Approval Failed";
                case 170: return "Send For Processing Failed";
                case 171: return docType == P2PDocumentType.Invoice ? "Cancelled" : "Internally Cancelled";
                case 176: return "Reprocessing";
                case 254: return "Partially Sourced";
                default: return "None";
            }
        }

        public string GetVisibilityComments(string val)
        {
            string[] accessType = val.Split(',');
            string shareText = "";
            foreach (var str in accessType)
            {
                switch (Convert.ToInt32(str))
                {
                    case 1:
                        if (shareText == "")
                            shareText = "Internal Users";
                        else
                            shareText = shareText + ", " + "Internal Users";
                        break;
                    case 2:
                        if (shareText == "")
                            shareText = "Approvers only";
                        else
                            shareText = shareText + ", " + "Approvers only";
                        break;
                    case 3:
                        if (shareText == "")
                            shareText = "Supplier";
                        else
                            shareText = shareText + ", " + "Supplier";
                        break;
                    case 4:
                        if (shareText == "")
                            shareText = "Internal Users and Supplier";
                        else
                            shareText = shareText + ", " + "Internal Users and Supplier";
                        break;
                    case 5:
                        if (shareText == "")
                            shareText = "CustomAccess";
                        else
                            shareText = shareText + ", " + "CustomAccess";
                        break;
                    case 6:
                        if (shareText == "")
                            shareText = "Buyers only";
                        else
                            shareText = shareText + ", " + "Buyers only";
                        break;
                    case 7:
                        if (shareText == "")
                            shareText = "Requesters only";
                        else
                            shareText = shareText + ", " + "Requesters only";
                        break;
                    case 8:
                        if (shareText == "")
                            shareText = "Payable Users only";
                        else
                            shareText = shareText + ", " + "Payable Users only";
                        break;
                }
            }
            return shareText;
        }

        public string GetDate(string date, DateFormatType dateType, TimeZoneInfo timeZoneInfo = null)
        {
            string dateTime = string.Empty;
            if (date != string.Empty && date != "")
            {
                CultureInfo cultureInfo = new CultureInfo(UserContact != null && UserContact.Address != null && UserContact.Address.CountryInfo != null
                && UserContact.Address.CountryInfo.CountryCultureInfo != null ? UserContact.Address.CountryInfo.CountryCultureInfo : this.UserContext.Culture.Trim());

                switch (dateType)
                {
                    case DateFormatType.commentdate: // commentdate
                        if (timeZoneInfo != null)
                            dateTime = TimeZoneInfo.ConvertTime(Convert.ToDateTime(date), timeZoneInfo).ToString("MMMM dd, yyyy h:mm:ss tt", new CultureInfo(this.UserContext.Culture.Trim()));
                        else
                            dateTime = Convert.ToDateTime(date).ToString("MMMM dd, yyyy h:mm:ss tt", new CultureInfo(this.UserContext.Culture.Trim())) + " UTC";
                        break;
                    case DateFormatType.formattedUtcDate: // formatted Utc Date
                        if (timeZoneInfo != null)
                        {
                            DateTime dateTimeObj;
                            dateTimeObj = Convert.ToDateTime(date).AddHours(timeZoneInfo.BaseUtcOffset.TotalHours);
                            dateTime = dateTimeObj.ToUniversalTime().ToString("MMM dd, yyyy h:mm tt", new CultureInfo(this.UserContext.Culture.Trim()));
                        }
                        else
                        {
                            dateTime = Convert.ToDateTime(date).ToString("MMM dd, yyyy h:mm tt", new CultureInfo(this.UserContext.Culture.Trim())) + " UTC";
                        }
                        break;
                    case DateFormatType.CustomDate:
                        if (timeZoneInfo != null)
                            dateTime = TimeZoneInfo.ConvertTime(Convert.ToDateTime(date), timeZoneInfo).ToString("MM/dd/yyyy");
                        else
                            dateTime = Convert.ToDateTime(date).ToString("MM/dd/yyyy");
                        break;
                    case DateFormatType.CustomDateDateFirst:
                        if (timeZoneInfo != null)
                            dateTime = TimeZoneInfo.ConvertTime(Convert.ToDateTime(date), timeZoneInfo).ToString("dd/MM/yyyy");
                        else
                            dateTime = Convert.ToDateTime(date).ToString("dd/MM/yyyy");
                        break;
                    case DateFormatType.CustomDateMonthFirst:
                        if (timeZoneInfo != null)
                            dateTime = TimeZoneInfo.ConvertTime(Convert.ToDateTime(date), timeZoneInfo).ToString("MM-dd-yyyy");
                        else
                            dateTime = Convert.ToDateTime(date).ToString("MM-dd-yyyy");
                        break;
                    default:
                        if (timeZoneInfo != null)
                            dateTime = TimeZoneInfo.ConvertTime(Convert.ToDateTime(date), timeZoneInfo).ToString("d", cultureInfo);
                        else
                            dateTime = Convert.ToDateTime(date).ToString("d", cultureInfo);
                        break;
                }
            }
            return dateTime;
        }


        public byte[] GetLicenceBytes()
        {
            byte[] licence = null;
            string asposeLicencePathUrl = null;
            try
            {
                asposeLicencePathUrl = AppDomain.CurrentDomain.BaseDirectory + "/AsposeTotal/Aspose.Total.lic";
                licence = File.ReadAllBytes(asposeLicencePathUrl);
            }
            catch (CommunicationException ex)
            {
                // Log Exception here
                LogHelper.LogError(Log, String.Format("Error occurred in GetLicenceBytes method in ExportManager AsposeLicensePath='{0}'", asposeLicencePathUrl), ex);
                throw;
            }
            catch (Exception ex)
            {
                // Log Exception here
                LogHelper.LogError(Log, String.Format("Error occurred in GetLicenceBytes method in ExportManager AsposeLicensePath='{0}'", asposeLicencePathUrl), ex);
                throw;
            }
            return licence;
        }

        public void DisposeService(ICommunicationObject objServiceChannel)
        {
            if (objServiceChannel != null)
            {
                if (objServiceChannel.State == CommunicationState.Faulted)
                    objServiceChannel.Abort();
                else
                    objServiceChannel.Close();

            }
            objServiceChannel = null;
        }
       
        public FileManagerEntities.FileDetails SaveMemoryStreamToFile(byte[] fileData, string fileNamePrefix, string fileNameExtn = "pdf")
        {            
            LogNewRelic("Inside SaveMemoryStreamToFile at LN - 506", "SaveMemoryStreamToFile-before-ExecutionHelper");
            var objFiledetails = new FileManagerEntities.FileDetails();

            LogNewRelic("Inside SaveMemoryStreamToFile at LN - 511", "SaveMemoryStreamToFile-before-UploadByteArraytoTemporaryBlobAsync");
            string tempBlobFileUri = FileOperations.UploadByteArraytoTemporaryBlobAsync(fileData, MultiRegionConfig.GetConfig(CloudConfig.CloudStorageConn), MultiRegionConfig.GetConfig(CloudConfig.TempFileUploadContainerName)).GetAwaiter().GetResult();
            var requestHeaders = new GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.RequestHeaders();
            requestHeaders.Set(this.UserContext, this.JWTToken);
            string appName = Environment.GetEnvironmentVariable("NewRelic.AppName") ?? "Requisition";
            string useCase = "ExportManager-SaveMemoryStreamToFile";
            var serviceURL = MultiRegionConfig.GetConfig(CloudConfig.APIMBaseURL) + "/FileManager/api/V2/FileManager/MoveFileToTargetBlob";

            var uploadFileToTargetBlobRequestModel = new FileManagerEntities.MoveFileToTargetBlobRequest()
            {
                FileName = String.Format("{0}.{1}", fileNamePrefix.Trim(), fileNameExtn),
                FileContentType = String.Format("application/{0}", fileNameExtn),
                FileValidationSettings = new FileManagerEntities.MoveFileToTargetBlobFileValidationSettings()
                {
                    FileValidationContainerTypeId = 1,
                    FileValidationSettingsScope = 1
                },
                TemporaryBlobFileUri = tempBlobFileUri
            };
            LogNewRelic("Inside SaveMemoryStreamToFile at LN - 530", "SaveMemoryStreamToFile-before-uploadFileToTargetBlobRequestModel");
            var webAPI = new GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.WebAPI(requestHeaders, appName, useCase);
            var result = webAPI.ExecutePost(serviceURL, uploadFileToTargetBlobRequestModel);
            FileManagerEntities.FileUploadResponseModel fileUploadResponse = JsonConvert.DeserializeObject<FileManagerEntities.FileUploadResponseModel>(result);
            objFiledetails.FileId = fileUploadResponse.FileId;
            objFiledetails.FileExtension = fileUploadResponse.FileExtension;
            objFiledetails.FileCreatedBy = fileUploadResponse.FileCreatedBy;
            objFiledetails.CompanyName = UserContext.CompanyName;
            objFiledetails.CreatedBy = UserContext.ContactCode;
            objFiledetails.DateCreated = DateTime.Now;
            objFiledetails.FileContainerType = FileManagerEntities.FileEnums.FileContainerType.Applications;
            objFiledetails.FileContentType = String.Format("application/{0}", fileNameExtn);
            objFiledetails.FileName = String.Format("{0}.{1}", fileNamePrefix.Trim(), fileNameExtn);
            objFiledetails.FileSizeInBytes = fileData.Length;
            LogNewRelic("Inside SaveMemoryStreamToFile at LN - 545", "SaveMemoryStreamToFile-after-uploadFileToTargetBlobRequestModel");

            return objFiledetails;
        }

        public string CreateRequesterDetails(string requesterName, string requesterEmailId, string RequesterPhone)
        {
            var requesterDetails = string.IsNullOrEmpty(requesterName)
                                     ? string.Empty
                                     : ", " + requesterName.Trim();
            requesterDetails += string.IsNullOrEmpty(requesterEmailId)
                                     ? string.Empty
                                     : ", " + requesterEmailId.Trim();
            requesterDetails += string.IsNullOrEmpty(RequesterPhone)
                                  ? string.Empty
                                  : ", " + RequesterPhone.Trim();
            requesterDetails.TrimEnd(',');
            if (requesterDetails != string.Empty)
                requesterDetails = requesterDetails.Substring(1);

            return requesterDetails;
        }

        public bool CheckFileTypeIsJPEG(string url)
        {
            try
            {
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "HEAD";
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                HttpStatusCode status = response.StatusCode;
                if (status == HttpStatusCode.OK)
                {
                    ImageDocument doc = new ImageDocument();
                    doc.DocContent = new System.Net.WebClient().DownloadData(url);
                    string fileName = Path.GetFileName(url);
                    doc.DocName = fileName;
                    doc.MIMEType = ImageDocument.GetMimeType(doc.DocContent, fileName);
                    if (doc.MIMEType.ToLower() == "image/jpeg")
                        return true;
                    else
                        return false;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void FillHeaderDetails(Export objExport, StringBuilder sbHtml, P2PDocumentType docType = P2PDocumentType.None, decimal totalBasecurrency = 0)
        {
            RequisitionCommonManager commonManager = new RequisitionCommonManager (this.JWTToken){ UserContext = UserContext, GepConfiguration = GepConfiguration };
            SettingDetails objSettings = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.None, 1, 107);
            var AllowOtherCharges = Convert.ToBoolean(commonManager.GetSettingsValueByKey(objSettings, "AllowOtherCharges"), CultureInfo.InvariantCulture);
            var isLineNumberVisible = Convert.ToBoolean(commonManager.GetSettingsValueByKey(objSettings, "IsLineNumberVisible"), CultureInfo.InvariantCulture);
            var partnerPhoneDetails = string.Empty;
            P2PDocumentDataSet ds = objExport.DocumentDataSet;
            if (ds.P2PDocument.Rows.Count > 0 && ds.P2PDocument[0] != null)
            {
                if (sbHtml.ToString().Contains("##ImageSrc##"))
                {
                    string LogoUrl = GetLogoUrl();
                    if (!string.IsNullOrEmpty(LogoUrl))
                        if (CheckFileTypeIsJPEG(LogoUrl))
                            sbHtml.Replace("##ImageSrc##", "<img src='" + LogoUrl + "' /> ");
                        else
                            sbHtml.Replace("##ImageSrc##", "");
                    else
                        sbHtml.Replace("##ImageSrc##", "");
                }

                //if supplier logo not exist then show blank in the pdf/print
                if (sbHtml.ToString().Contains("##PartnerImageSrc##"))
                {
                    if (!string.IsNullOrEmpty(ds.P2PDocument[0].PartnerImageUrl))
                        if (CheckFileTypeIsJPEG(ds.P2PDocument[0].PartnerImageUrl))
                            sbHtml.Replace("##PartnerImageSrc##", "<img src='" + ds.P2PDocument[0].PartnerImageUrl + "' /> ");
                        else
                            sbHtml.Replace("##PartnerImageSrc##", "");
                    else
                        sbHtml.Replace("##PartnerImageSrc##", "");
                }


                if (!String.IsNullOrEmpty(Convert.ToString(ds.P2PDocument[0].RevisionNumber)))
                {
                    if (sbHtml.ToString().Contains("##lblRequisitionType##"))
                    {
                        sbHtml.Replace("##lblRequisitionType##", "CHANGE REQUISITION");
                        sbHtml.Replace("##lbltotalValueHeader##", "Changed Total Value");
                        string deltarow = "<tr><td align = 'left' valign = 'top' style = 'padding: 3px 10px; border: 1px solid #bbb;'><strong> Delta Amount </strong ></td><td align = 'right' valign = 'top' style = 'padding: 3px 10px; border: 1px solid #bbb;'>" + ConvertToString(Math.Round((ds.P2PDocument[0].RequisitionTotalChange), Convert.ToInt32(commonManager.GetSettingsValueByKey(objSettings, "MaxPrecessionValueforTotal")), MidpointRounding.AwayFromZero)) + "</td></tr >";
                        sbHtml.Replace("##lblDeltaAmountHeader##", deltarow);
                    }
                }
                else
                {
                    if (sbHtml.ToString().Contains("##lblRequisitionType##"))
                    {
                        sbHtml.Replace("##lblRequisitionType##", "REQUISITION");
                        sbHtml.Replace("##lbltotalValueHeader##", "Total Value");
                        sbHtml.Replace("##lblDeltaAmountHeader##", "");
                    }
                }

                if (sbHtml.ToString().Contains("##lblBaseCurrency##"))
                    sbHtml.Replace("##lblBaseCurrency##", ds.P2PDocument[0].BaseCurrency);

                sbHtml.Replace("##lblPartnerOrderingLocation##", Convert.ToString(ds.P2PDocument[0].ClientLocationCode) + " :: " + Convert.ToString(ds.P2PDocument[0].LocationName));
                sbHtml.Replace("##lblPartnerOrderingLocationName##", Convert.ToString(ds.P2PDocument[0].LocationName));
                sbHtml.Replace("##lblPartnerNameAndAdderss##",
                                      CreateAddress(BaseDAO.HtmlEncode(ds.P2PDocument[0].CompanyName), BaseDAO.HtmlEncode(ds.P2PDocument[0].PartnerAddressLine1),
                                                    BaseDAO.HtmlEncode(ds.P2PDocument[0].PartnerAddressLine2),
                                                    BaseDAO.HtmlEncode(ds.P2PDocument[0].PartnerAddressLine3),
                                                    BaseDAO.HtmlEncode(ds.P2PDocument[0].PartnerCity),
                                                    BaseDAO.HtmlEncode(ds.P2PDocument[0].PartnerStateName),
                                                    BaseDAO.HtmlEncode(ds.P2PDocument[0].PartnerCountryName),
                                                    BaseDAO.HtmlEncode(ds.P2PDocument[0].PartnerZipCode)));
                if (sbHtml.ToString().Contains("##BuyerCompanyName##"))
                    sbHtml.Replace("##BuyerCompanyName##", string.IsNullOrEmpty(ds.P2PDocument[0].BuyerCompanyName) ? string.Empty : BaseDAO.HtmlEncode(ds.P2PDocument[0].BuyerCompanyName) + ", ");
                sbHtml.Replace("##CompanyAddress##",
                                      CreateAddress("", BaseDAO.HtmlEncode(ds.P2PDocument[0].BuyerAddressLine1),
                                                    BaseDAO.HtmlEncode(ds.P2PDocument[0].BuyerAddressLine2),
                                                    BaseDAO.HtmlEncode(ds.P2PDocument[0].BuyerAddressLine3),
                                                    BaseDAO.HtmlEncode(ds.P2PDocument[0].BuyerCity),
                                                    BaseDAO.HtmlEncode(ds.P2PDocument[0].BuyerStateName),
                                                    BaseDAO.HtmlEncode(ds.P2PDocument[0].BuyerCountryName),
                                                    BaseDAO.HtmlEncode(ds.P2PDocument[0].BuyerZipode)));
                sbHtml.Replace("##lblPartnerAddress##",
                                      CreateAddress(BaseDAO.HtmlEncode(ds.P2PDocument[0].PartnerName),
                                                    BaseDAO.HtmlEncode(ds.P2PDocument[0].PartnerAddressLine1),
                                                    BaseDAO.HtmlEncode(ds.P2PDocument[0].PartnerAddressLine2),
                                                    BaseDAO.HtmlEncode(ds.P2PDocument[0].PartnerAddressLine3),
                                                    BaseDAO.HtmlEncode(ds.P2PDocument[0].PartnerCity),
                                                    BaseDAO.HtmlEncode(ds.P2PDocument[0].PartnerStateName),
                                                    BaseDAO.HtmlEncode(ds.P2PDocument[0].PartnerCountryName),
                                                    BaseDAO.HtmlEncode(ds.P2PDocument[0].PartnerZipCode)));
                sbHtml.Replace("##lblClientPartnerCode##",
                                      BaseDAO.HtmlEncode(ds.P2PDocument[0].ClientPartnerCode));
                sbHtml.Replace("##lblInvoicingMethod##",
                                      BaseDAO.HtmlEncode(ds.P2PDocument[0].InvoicingMethod));
                sbHtml.Replace("##lblPartnerName##",
                                      BaseDAO.HtmlEncode(ds.P2PDocument[0].CompanyName));
                sbHtml.Replace("##lblLegalCompanyName##", BaseDAO.HtmlEncode(ds.P2PDocument[0].LegalCompanyName));
                sbHtml.Replace("##lblSupplierAddress##",
                                      CreateAddress("", BaseDAO.HtmlEncode(ds.P2PDocument[0].PartnerAddressLine1),
                                                    BaseDAO.HtmlEncode(ds.P2PDocument[0].PartnerAddressLine2),
                                                    BaseDAO.HtmlEncode(ds.P2PDocument[0].PartnerAddressLine3),
                                                    BaseDAO.HtmlEncode(ds.P2PDocument[0].PartnerCity),
                                                    BaseDAO.HtmlEncode(ds.P2PDocument[0].PartnerStateName),
                                                    BaseDAO.HtmlEncode(ds.P2PDocument[0].PartnerCountryName),
                                                    BaseDAO.HtmlEncode(ds.P2PDocument[0].PartnerZipCode)));
                sbHtml.Replace("##lblSupplierAddressLine1##", BaseDAO.HtmlEncode(ds.P2PDocument[0].PartnerAddressLine1));
                sbHtml.Replace("##lblSupplierAddressLine2##", BaseDAO.HtmlEncode(ds.P2PDocument[0].PartnerAddressLine2));
                sbHtml.Replace("##lblSupplierAddressLine3##", BaseDAO.HtmlEncode(ds.P2PDocument[0].PartnerAddressLine3));
                sbHtml.Replace("##lblSupplierCity##", BaseDAO.HtmlEncode(ds.P2PDocument[0].PartnerCity));
                sbHtml.Replace("##lblSupplierStateName##", BaseDAO.HtmlEncode(ds.P2PDocument[0].PartnerStateName));
                sbHtml.Replace("##lblSupplierCountryName##", BaseDAO.HtmlEncode(ds.P2PDocument[0].PartnerCountryName));
                sbHtml.Replace("##lblSupplierZipCode##", BaseDAO.HtmlEncode(ds.P2PDocument[0].PartnerZipCode));

                partnerPhoneDetails = BaseDAO.HtmlEncode(ds.P2PDocument[0].PartnerPhone1);
                if (!string.IsNullOrEmpty(ds.P2PDocument[0].PartnerFaxNo))
                {
                    partnerPhoneDetails += ", ";
                    partnerPhoneDetails += ds.P2PDocument[0].PartnerFaxNo;
                }
                sbHtml.Replace("##lblPartnerPhoneFax##",
                                      partnerPhoneDetails);
                sbHtml.Replace("##lblCurrencySymbol##",
                                    Convert.ToString(BaseDAO.HtmlEncode(ds.P2PDocument[0].CurrencyCode)));
                sbHtml.Replace("##lblPartnerContactEmail##",
                                      BaseDAO.HtmlEncode(ds.P2PDocument[0].PartnerEmailId));
                sbHtml.Replace("##lblPartnerContactPhone##",
                                       ds.P2PDocument[0].PartnerPhone1);
                sbHtml.Replace("##lblPartnerContactName##",
                                      BaseDAO.HtmlEncode(ds.P2PDocument[0].PartnerName));
                sbHtml.Replace("##lblPartnerEmail##", GetBuyerContact(ds.P2PDocument[0].PartnerPhone2, BaseDAO.HtmlEncode(ds.P2PDocument[0].PartnerEmailId)));
                sbHtml.Replace("##lblDispatchMode##", BaseDAO.HtmlEncode(ds.P2PDocument[0].DispatchMode));
                sbHtml.Replace("##lblDocumentName##",
                                     BaseDAO.HtmlEncode(BaseDAO.HtmlEncode(ds.P2PDocument[0].DocumentName)));
                sbHtml.Replace("##lblBuyerAssigneeName##",
                                    BaseDAO.HtmlEncode(ds.P2PDocument[0].BuyerAssigneeName));
                sbHtml.Replace("##lblDocumentNumber##",
                                      BaseDAO.HtmlEncode(ds.P2PDocument[0].DocumentNumber));
                sbHtml.Replace("##lblCurrency##", BaseDAO.HtmlEncode(ds.P2PDocument[0].CurrencyCode));
                if (isLineNumberVisible)
                    sbHtml.Replace("##lblSerialNoLineNo##", "Line No.");
                else
                    sbHtml.Replace("##lblSerialNoLineNo##", "Serial No.");

                sbHtml.Replace("##lblBuyerEmail##", GetBuyerContact(ds.P2PDocument[0].BuyerPhone1, BaseDAO.HtmlEncode(ds.P2PDocument[0].BuyerEmailId)));
                if (ds.P2PDocument[0].DocumentTypeInfo == 8)
                {
                    sbHtml.Replace("##lblOrderType##", ds.P2PDocument[0].OrderSourceTypeInfo);
                }
                else
                {
                    sbHtml.Replace("##lblOrderType##", GetOrderType(ds.P2PDocument[0].DocumentSourceTypeInfo));
                }

                if (sbHtml.ToString().Contains("##lblBuyerContact##"))
                    sbHtml.Replace("##lblBuyerContact##", BaseDAO.HtmlEncode(ds.P2PDocument[0].BuyerEmailId));
                //sbHtml.Replace("##lblBuyerContact##", GetBuyerContact(ds.P2PDocument[0].BuyerPhone1, ds.P2PDocument[0].BuyerEmailId));
                if (sbHtml.ToString().Contains("##lblBuyerContactPhone##"))
                    sbHtml.Replace("##lblBuyerContactPhone##", "/ " + ds.P2PDocument[0].BuyerPhone1);
                if (sbHtml.ToString().Contains("##lblBuyerContactPhone2##") && !string.IsNullOrEmpty(ds.P2PDocument[0].BuyerPhone2))
                    sbHtml.Replace("##lblBuyerContactPhone2##", "/ " + ds.P2PDocument[0].BuyerPhone2);

                if (ds.P2PDocument[0].DocumentStatusInfo == 121 && ds.P2PDocument[0].DocumentTypeInfo == 8)
                {
                    sbHtml.Replace("##lblOrderStatus##", String.Concat("<b style = 'color:red'>", ds.P2PDocument[0].DocumentStatusDisplayName.ToUpper(), "</b>"));
                    sbHtml.Replace("##lblItemTotal##", String.Concat("<b style = 'color:red'>", ds.P2PDocument[0].DocumentStatusDisplayName.ToUpper(), "</b>"));
                    sbHtml.Replace("##lblTaxAmount##", String.Concat("<b style = 'color:red'>", ds.P2PDocument[0].DocumentStatusDisplayName.ToUpper(), "</b>"));
                    sbHtml.Replace("##lblShippingCharges##", String.Concat("<b style = 'color:red'>", ds.P2PDocument[0].DocumentStatusDisplayName.ToUpper(), "</b>"));
                    sbHtml.Replace("##lblOtherChargesTotal##", String.Concat("<b style = 'color:red'>", ds.P2PDocument[0].DocumentStatusDisplayName.ToUpper(), "</b>"));
                    if (ds.P2PDocument[0].HasFlexibleCharges != 1)
                    {
                        var hiddenFlexible = GetSectionHtml(sbHtml.ToString(), "##HeaderFlexibleChargeSection##", "##/HeaderFlexibleChargeSection##");
                        if (!string.IsNullOrWhiteSpace(hiddenFlexible))
                            sbHtml.Replace(hiddenFlexible, string.Empty);
                    }
                    else
                    {
                        sbHtml.Replace("##lblTotalChargesTotal##", String.Concat("<b style = 'color:red'>", ds.P2PDocument[0].DocumentStatusDisplayName.ToUpper(), "</b>"));
                        var hiddenDefault = GetSectionHtml(sbHtml.ToString(), "##HeaderDefaultSection##", "##/HeaderDefaultSection##");
                        if (!string.IsNullOrWhiteSpace(hiddenDefault))
                            sbHtml.Replace(hiddenDefault, string.Empty);
                    }
                    sbHtml.Replace("##lblTotal##", String.Concat("<b style = 'color:red'>", ds.P2PDocument[0].DocumentStatusDisplayName.ToUpper(), "</b>"));
                }
                else
                {
                    if (ds.P2PDocument[0].DocumentTypeInfo == 8)
                    {
                        if (ds.P2PDocument[0].DocumentStatusInfo == 23)
                        {
                            sbHtml.Replace("##lblOrderStatus##", String.Concat("<b style = 'color:red'>", ds.P2PDocument[0].DocumentStatusDisplayName.ToUpper(), "</b>"));
                        }
                        else
                        {
                            sbHtml.Replace("##lblOrderStatus##", ds.P2PDocument[0].DocumentStatusDisplayName);
                        }
                    }
                    else
                    {
                        sbHtml.Replace("##lblOrderStatus##", GetDocumentStatusById(ds.P2PDocument[0].DocumentStatusInfo));
                    }

                    if (ds.P2PDocument[0].ItemTotal >= 0)
                        sbHtml.Replace("##lblItemTotal##", ConvertToString(Math.Round(ds.P2PDocument[0].ItemTotal, Convert.ToInt32(commonManager.GetSettingsValueByKey(objSettings, "MaxPrecessionValueforTotal")), MidpointRounding.AwayFromZero)));
                    else
                        sbHtml.Replace("##lblItemTotal##", ConvertToNegativeString(Math.Round(ds.P2PDocument[0].ItemTotal, Convert.ToInt32(commonManager.GetSettingsValueByKey(objSettings, "MaxPrecessionValueforTotal")), MidpointRounding.AwayFromZero)));

                    if (ds.P2PDocument[0].Tax >= 0)
                        sbHtml.Replace("##lblTaxAmount##", ConvertToString(Math.Round(ds.P2PDocument[0].Tax, Convert.ToInt32(commonManager.GetSettingsValueByKey(objSettings, "MaxPrecessionValueForTaxesAndCharges")), MidpointRounding.AwayFromZero)));
                    else
                        sbHtml.Replace("##lblTaxAmount##", ConvertToNegativeString(Math.Round(ds.P2PDocument[0].Tax, Convert.ToInt32(commonManager.GetSettingsValueByKey(objSettings, "MaxPrecessionValueForTaxesAndCharges")), MidpointRounding.AwayFromZero)));

                    if (ds.P2PDocument[0].HasFlexibleCharges != 1)
                    {
                        if (ds.P2PDocument[0].Shipping >= 0)
                        {
                            sbHtml.Replace("##lblShippingCharges##", ConvertToString(Math.Round(ds.P2PDocument[0].Shipping, Convert.ToInt32(commonManager.GetSettingsValueByKey(objSettings, "MaxPrecessionValueforTotal")), MidpointRounding.AwayFromZero)));
                        }
                        else
                        {
                            sbHtml.Replace("##lblShippingCharges##", ConvertToNegativeString(Math.Round(ds.P2PDocument[0].Shipping, Convert.ToInt32(commonManager.GetSettingsValueByKey(objSettings, "MaxPrecessionValueforTotal")), MidpointRounding.AwayFromZero)));
                        }
                        if (AllowOtherCharges)
                        {
                            if (ds.P2PDocument[0].AdditionalCharges >= 0)
                                sbHtml.Replace("##lblOtherChargesTotal##", ConvertToString(Math.Round(ds.P2PDocument[0].AdditionalCharges, Convert.ToInt32(commonManager.GetSettingsValueByKey(objSettings, "MaxPrecessionValueforTotal")), MidpointRounding.AwayFromZero)));
                            else
                                sbHtml.Replace("##lblOtherChargesTotal##", ConvertToNegativeString(Math.Round(ds.P2PDocument[0].AdditionalCharges, Convert.ToInt32(commonManager.GetSettingsValueByKey(objSettings, "MaxPrecessionValueforTotal")), MidpointRounding.AwayFromZero)));
                        }
                        else
                        {
                            sbHtml.Replace("##lblOtherChargesTotal##", "");
                        }
                        var hiddenFlexible = GetSectionHtml(sbHtml.ToString(), "##HeaderFlexibleChargeSection##", "##/HeaderFlexibleChargeSection##");
                        if (!string.IsNullOrWhiteSpace(hiddenFlexible))
                            sbHtml.Replace(hiddenFlexible, string.Empty);

                    }
                    else
                    {
                        if (ds.P2PDocument[0].Shipping >= 0)
                        {
                            sbHtml.Replace("##lblShippingCharges##", ConvertToString(Math.Round(ds.P2PDocument[0].Shipping, Convert.ToInt32(commonManager.GetSettingsValueByKey(objSettings, "MaxPrecessionValueforTotal")), MidpointRounding.AwayFromZero)));
                        }
                        else
                        {
                            sbHtml.Replace("##lblShippingCharges##", ConvertToNegativeString(Math.Round(ds.P2PDocument[0].Shipping, Convert.ToInt32(commonManager.GetSettingsValueByKey(objSettings, "MaxPrecessionValueforTotal")), MidpointRounding.AwayFromZero)));
                        }
                        if (AllowOtherCharges)
                        {
                            if (ds.P2PDocument[0].AdditionalCharges >= 0)
                                sbHtml.Replace("##lblOtherChargesTotal##", ConvertToString(Math.Round(ds.P2PDocument[0].AdditionalCharges, Convert.ToInt32(commonManager.GetSettingsValueByKey(objSettings, "MaxPrecessionValueforTotal")), MidpointRounding.AwayFromZero)));
                            else
                                sbHtml.Replace("##lblOtherChargesTotal##", ConvertToNegativeString(Math.Round(ds.P2PDocument[0].AdditionalCharges, Convert.ToInt32(commonManager.GetSettingsValueByKey(objSettings, "MaxPrecessionValueforTotal")), MidpointRounding.AwayFromZero)));
                        }
                        else
                        {
                            sbHtml.Replace("##lblOtherChargesTotal##", "");
                        }
                        if (ds.P2PDocument[0].AdditionalCharges >= 0)
                        {
                            if (AllowOtherCharges)
                                sbHtml.Replace("##lblTotalChargesTotal##", ConvertToString(Math.Round(ds.P2PDocument[0].AdditionalCharges, Convert.ToInt32(commonManager.GetSettingsValueByKey(objSettings, "MaxPrecessionValueforTotal")), MidpointRounding.AwayFromZero)));
                        }
                        else
                        {
                            if (AllowOtherCharges)
                                sbHtml.Replace("##lblTotalChargesTotal##", ConvertToNegativeString(Math.Round(ds.P2PDocument[0].AdditionalCharges, Convert.ToInt32(commonManager.GetSettingsValueByKey(objSettings, "MaxPrecessionValueforTotal")), MidpointRounding.AwayFromZero)));
                        }
                        var hiddenDefault = GetSectionHtml(sbHtml.ToString(), "##HeaderDefaultSection##", "##/HeaderDefaultSection##");
                        if (!string.IsNullOrWhiteSpace(hiddenDefault))
                            sbHtml.Replace(hiddenDefault, string.Empty);
                    }

                    if (Math.Round((ds.P2PDocument[0].ItemTotal + ds.P2PDocument[0].Tax + ds.P2PDocument[0].Shipping + ds.P2PDocument[0].AdditionalCharges), Convert.ToInt32(commonManager.GetSettingsValueByKey(objSettings, "MaxPrecessionValueforTotal")), MidpointRounding.AwayFromZero) >= 0)
                        sbHtml.Replace("##lblTotal##", ConvertToString(Math.Round((ds.P2PDocument[0].ItemTotal + ds.P2PDocument[0].Tax + ds.P2PDocument[0].Shipping + ds.P2PDocument[0].AdditionalCharges), Convert.ToInt32(commonManager.GetSettingsValueByKey(objSettings, "MaxPrecessionValueforTotal")), MidpointRounding.AwayFromZero)));
                    else
                        sbHtml.Replace("##lblTotal##", ConvertToNegativeString(Math.Round((ds.P2PDocument[0].ItemTotal + ds.P2PDocument[0].Tax + ds.P2PDocument[0].Shipping + ds.P2PDocument[0].AdditionalCharges), Convert.ToInt32(commonManager.GetSettingsValueByKey(objSettings, "MaxPrecessionValueforTotal")), MidpointRounding.AwayFromZero)));

                    if (Math.Round((totalBasecurrency), Convert.ToInt32(commonManager.GetSettingsValueByKey(objSettings, "MaxPrecessionValueforTotal")), MidpointRounding.AwayFromZero) >= 0)
                        sbHtml.Replace("##lblBaseTotal##", ConvertToString(Math.Round((totalBasecurrency), Convert.ToInt32(commonManager.GetSettingsValueByKey(objSettings, "MaxPrecessionValueforTotal")), MidpointRounding.AwayFromZero)));
                    else
                        sbHtml.Replace("##lblBaseTotal##", ConvertToNegativeString(Math.Round((totalBasecurrency), Convert.ToInt32(commonManager.GetSettingsValueByKey(objSettings, "MaxPrecessionValueforTotal")), MidpointRounding.AwayFromZero)));

                }

                if (ds.DocumentAdditionalEntityInfo.Count > 0 && sbHtml.ToString().Contains("##lblDocumentAdditionalEntityInfo##"))
                    sbHtml.Replace("##lblDocumentAdditionalEntityInfo##", ds.DocumentAdditionalEntityInfo[0].EntityDisplayName);
                else
                    sbHtml.Replace("##lblDocumentAdditionalEntityInfo##", string.Empty);

                if (ds.DocumentAdditionalEntityInfo.Count > 0 && sbHtml.ToString().Contains("##lblDocumentAdditionalEntityCode##"))
                    sbHtml.Replace("##lblDocumentAdditionalEntityCode##", ds.DocumentAdditionalEntityInfo[0].EntityCode);
                else
                    sbHtml.Replace("##lblDocumentAdditionalEntityCode##", string.Empty);

                sbHtml.Replace("##lblBillTo##",
                                      CreateAddress(BaseDAO.HtmlEncode(ds.P2PDocument[0].BilltoLocationName),
                                                   BaseDAO.HtmlEncode(ds.P2PDocument[0].BilltoLocationAddressLine1),
                                                    BaseDAO.HtmlEncode(ds.P2PDocument[0].BilltoLocationAddressLine2),
                                                    BaseDAO.HtmlEncode(ds.P2PDocument[0].BilltoLocationAddressLine3),
                                                    BaseDAO.HtmlEncode(ds.P2PDocument[0].BilltoLocationCity),
                                                    BaseDAO.HtmlEncode(ds.P2PDocument[0].BilltoLocationStateName),
                                                    BaseDAO.HtmlEncode(ds.P2PDocument[0].BilltoLocationCountryName),
                                                    BaseDAO.HtmlEncode(ds.P2PDocument[0].BilltoLocationZipCode)));
                if (sbHtml.ToString().Contains("##lblBillToAddress##"))
                {
                    sbHtml.Replace("##lblBillToAddress##",
                                      CreateAddress("", BaseDAO.HtmlEncode(ds.P2PDocument[0].BilltoLocationAddressLine1),
                                                    BaseDAO.HtmlEncode(ds.P2PDocument[0].BilltoLocationAddressLine2),
                                                    BaseDAO.HtmlEncode(ds.P2PDocument[0].BilltoLocationAddressLine3),
                                                    BaseDAO.HtmlEncode(ds.P2PDocument[0].BilltoLocationCity),
                                                    BaseDAO.HtmlEncode(ds.P2PDocument[0].BilltoLocationStateName),
                                                    BaseDAO.HtmlEncode(ds.P2PDocument[0].BilltoLocationCountryName),
                                                    BaseDAO.HtmlEncode(ds.P2PDocument[0].BilltoLocationZipCode)));
                }
                if (sbHtml.ToString().Contains("##lblBilltoLocationName##"))
                {
                    sbHtml.Replace("##lblBilltoLocationName##", BaseDAO.HtmlEncode(ds.P2PDocument[0].BilltoLocationName));
                }
                sbHtml.Replace("##lblBilltoLocationAddressLine1##", BaseDAO.HtmlEncode(ds.P2PDocument[0].BilltoLocationAddressLine1));
                sbHtml.Replace("##lblBilltoLocationAddressLine2##", BaseDAO.HtmlEncode(ds.P2PDocument[0].BilltoLocationAddressLine2));
                sbHtml.Replace("##lblBilltoLocationAddressLine3##", BaseDAO.HtmlEncode(ds.P2PDocument[0].BilltoLocationAddressLine3));
                sbHtml.Replace("##lblBilltoLocationCity##", BaseDAO.HtmlEncode(ds.P2PDocument[0].BilltoLocationCity));
                sbHtml.Replace("##lblBilltoLocationStateName##", BaseDAO.HtmlEncode(ds.P2PDocument[0].BilltoLocationStateName));
                sbHtml.Replace("##lblBilltoLocationCountryName##", BaseDAO.HtmlEncode(ds.P2PDocument[0].BilltoLocationCountryName));
                sbHtml.Replace("##lblBilltoLocationZipCode##", BaseDAO.HtmlEncode(ds.P2PDocument[0].BilltoLocationZipCode));

                sbHtml.Replace("##EmailIdFaxNo##",
                                      EmailAndFaxFormat(ds.P2PDocument[0].BilltoLocationEmailAddress, ds.P2PDocument[0].BilltoLocationFaxNo));

                if (sbHtml.ToString().Contains("##lblShipToLocationNumber##"))
                {
                    sbHtml.Replace("##lblShipToLocationNumber##", (ds.P2PDocument[0].ShipToLocationNumber == null) ? string.Empty : BaseDAO.HtmlEncode(Convert.ToString(ds.P2PDocument[0].ShipToLocationNumber)));
                }

                sbHtml.Replace("##lblShipTo##",
                                  CreateAddress(BaseDAO.HtmlEncode(ds.P2PDocument[0].ShiptoLocationName),
                                                BaseDAO.HtmlEncode(ds.P2PDocument[0].ShiptoLocationAddressLine1),
                                                BaseDAO.HtmlEncode(ds.P2PDocument[0].ShiptoLocationAddressLine2),
                                                BaseDAO.HtmlEncode(ds.P2PDocument[0].ShiptoLocationAddressLine3),
                                                BaseDAO.HtmlEncode(ds.P2PDocument[0].ShiptoLocationCity),
                                                BaseDAO.HtmlEncode(ds.P2PDocument[0].ShiptoLocationStateName),
                                                BaseDAO.HtmlEncode(ds.P2PDocument[0].ShiptoLocationCountryName),
                                                BaseDAO.HtmlEncode(ds.P2PDocument[0].ShiptoLocationZipCode)));
                if (sbHtml.ToString().Contains("##lblShipToAdress##"))
                {
                    sbHtml.Replace("##lblShipToAdress##",
                                     CreateAddress("", BaseDAO.HtmlEncode(ds.P2PDocument[0].ShiptoLocationAddressLine1),
                                                   BaseDAO.HtmlEncode(ds.P2PDocument[0].ShiptoLocationAddressLine2),
                                                   BaseDAO.HtmlEncode(ds.P2PDocument[0].ShiptoLocationAddressLine3),
                                                   BaseDAO.HtmlEncode(ds.P2PDocument[0].ShiptoLocationCity),
                                                   BaseDAO.HtmlEncode(ds.P2PDocument[0].ShiptoLocationStateName),
                                                   BaseDAO.HtmlEncode(ds.P2PDocument[0].ShiptoLocationCountryName),
                                                   BaseDAO.HtmlEncode(ds.P2PDocument[0].ShiptoLocationZipCode)));
                }

                sbHtml.Replace("##lblShiptoLocationName##", BaseDAO.HtmlEncode(ds.P2PDocument[0].ShiptoLocationName));
                sbHtml.Replace("##lblShiptoLocationAddressLine1##", BaseDAO.HtmlEncode(ds.P2PDocument[0].ShiptoLocationAddressLine1));
                sbHtml.Replace("##lblShiptoLocationAddressLine2##", BaseDAO.HtmlEncode(ds.P2PDocument[0].ShiptoLocationAddressLine2));
                sbHtml.Replace("##lblShiptoLocationAddressLine3##", BaseDAO.HtmlEncode(ds.P2PDocument[0].ShiptoLocationAddressLine3));
                sbHtml.Replace("##lblShiptoLocationCity##", BaseDAO.HtmlEncode(ds.P2PDocument[0].ShiptoLocationCity));
                sbHtml.Replace("##lblShiptoLocationStateName##", BaseDAO.HtmlEncode(ds.P2PDocument[0].ShiptoLocationStateName));
                sbHtml.Replace("##lblShiptoLocationCountryName##", BaseDAO.HtmlEncode(ds.P2PDocument[0].ShiptoLocationCountryName));
                sbHtml.Replace("##lblShiptoLocationZipCode##", BaseDAO.HtmlEncode(ds.P2PDocument[0].ShiptoLocationZipCode));

                if (ds.P2PDocument[0].DeliverTo.Trim() != string.Empty)
                {
                    sbHtml.Replace("##lblDeliverTo##", BaseDAO.HtmlEncode(ds.P2PDocument[0].DeliverTo));
                }
                else
                {
                    sbHtml.Replace("##lblDeliverTo##",
                                      CreateAddress(BaseDAO.HtmlEncode(ds.P2PDocument[0].DelivertoLocationName),
                                                    BaseDAO.HtmlEncode(ds.P2PDocument[0].DelivertoLocationAddressLine1),
                                                    BaseDAO.HtmlEncode(ds.P2PDocument[0].DelivertoLocationAddressLine2),
                                                    BaseDAO.HtmlEncode(ds.P2PDocument[0].DelivertoLocationAddressLine3),
                                                    BaseDAO.HtmlEncode(ds.P2PDocument[0].DelivertoLocationCity),
                                                    BaseDAO.HtmlEncode(ds.P2PDocument[0].DelivertoLocationStateName),
                                                    BaseDAO.HtmlEncode(ds.P2PDocument[0].DelivertoLocationCountryName),
                                                    BaseDAO.HtmlEncode(ds.P2PDocument[0].DelivertoLocationZipCode)));
                }
                sbHtml.Replace("##lblDelivertoLocationName##", BaseDAO.HtmlEncode(ds.P2PDocument[0].DelivertoLocationName));
                sbHtml.Replace("##lblDelivertoLocationAddressLine1##", BaseDAO.HtmlEncode(ds.P2PDocument[0].DelivertoLocationAddressLine1));
                sbHtml.Replace("##lblDelivertoLocationAddressLine2##", BaseDAO.HtmlEncode(ds.P2PDocument[0].DelivertoLocationAddressLine2));
                sbHtml.Replace("##lblDelivertoLocationAddressLine3##", BaseDAO.HtmlEncode(ds.P2PDocument[0].DelivertoLocationAddressLine3));
                sbHtml.Replace("##lblDelivertoLocationCity##", BaseDAO.HtmlEncode(ds.P2PDocument[0].DelivertoLocationCity));
                sbHtml.Replace("##lblDelivertoLocationStateName##", BaseDAO.HtmlEncode(ds.P2PDocument[0].DelivertoLocationStateName));
                sbHtml.Replace("##lblDelivertoLocationCountryName##", BaseDAO.HtmlEncode(ds.P2PDocument[0].DelivertoLocationCountryName));
                sbHtml.Replace("##lblDelivertoLocationZipCode##", BaseDAO.HtmlEncode(ds.P2PDocument[0].DelivertoLocationZipCode));

                if (sbHtml.ToString().Contains("##lblDeliveryInstructions##"))
                    sbHtml.Replace("##lblDeliveryInstructions##", BaseDAO.HtmlEncode(ds.P2PDocument[0].DeliveryInstructions));
                if (sbHtml.ToString().Contains("##lblVATRegistrationNumber##"))
                    sbHtml.Replace("##lblVATRegistrationNumber##", BaseDAO.HtmlEncode(ds.P2PDocument[0].VATRegistrationNumber));
                if (sbHtml.ToString().Contains("##lblRegisteredNumber##"))
                    sbHtml.Replace("##lblRegisteredNumber##", BaseDAO.HtmlEncode(ds.P2PDocument[0].RegisteredNumber));

                sbHtml.Replace("##HeaderDefaultSection##", string.Empty);
                sbHtml.Replace("##/HeaderDefaultSection##", string.Empty);
                sbHtml.Replace("##HeaderFlexibleChargeSection##", string.Empty);
                sbHtml.Replace("##/HeaderFlexibleChargeSection##", string.Empty);
                sbHtml.Replace("##lblOnBehalfOF##", BaseDAO.HtmlEncode(ds.P2PDocument[0].OnBehalfOf));
                if (sbHtml.ToString().Contains("##lblCostApprover##"))
                    sbHtml.Replace("##lblCostApprover##", BaseDAO.HtmlEncode(ds.P2PDocument[0].CostApprover));
                sbHtml.Replace("##lblUrgent##", ds.P2PDocument[0].IsUrgent);
                if (sbHtml.ToString().Contains("##lblPurchaseType##"))
                    sbHtml.Replace("##lblPurchaseType##", ds.P2PDocument[0].PurchaseType);
                sbHtml.Replace("##lblHeaderLevelBU##", ds.P2PDocument[0].HeaderLevelBU);
                if (ds.P2PDocument[0].DocumentTypeInfo == 8)
                {
                    sbHtml.Replace("##lblDocumentStatus##", ds.P2PDocument[0].DocumentStatusDisplayName);
                }
                else
                {
                    sbHtml.Replace("##lblDocumentStatus##", GetDocumentStatusById(ds.P2PDocument[0].DocumentStatusInfo, docType));
                }

                if (ds.P2PDocument[0].RequesterName != null && ds.P2PDocument[0].RequesterName != "" && ds.P2PDocument[0].RequesterName != "\"\"")
                {
                    sbHtml.Replace("##lblRequesterDetails##",
                                          CreateRequesterDetails(BaseDAO.HtmlEncode(ds.P2PDocument[0].RequesterName),
                                                                 BaseDAO.HtmlEncode(ds.P2PDocument[0].RequesterEmailId),
                                                                 BaseDAO.HtmlEncode(ds.P2PDocument[0].RequesterPhone)));
                    sbHtml.Replace("##HeaderLevelRequesterSection##", string.Empty);
                    sbHtml.Replace("##/HeaderLevelRequesterSection##", string.Empty);
                }
                else
                {
                    string strRequesterDetailsSectionHtml = string.Empty;
                    strRequesterDetailsSectionHtml = GetSectionHtml(sbHtml.ToString(), "##HeaderLevelRequesterSection##",
                                                                   "##/HeaderLevelRequesterSection##");
                    if (strRequesterDetailsSectionHtml != null && strRequesterDetailsSectionHtml != string.Empty)
                        sbHtml.Replace(strRequesterDetailsSectionHtml, string.Empty);
                }

                if (sbHtml.ToString().Contains("##lblPostalAddressLine1##"))
                    sbHtml.Replace("##lblPostalAddressLine1##", ds.P2PDocument[0].PostalAddressLine1);
                if (sbHtml.ToString().Contains("##lblPostalAddressLine2##"))
                    sbHtml.Replace("##lblPostalAddressLine2##", ds.P2PDocument[0].PostalAddressLine2);
                if (sbHtml.ToString().Contains("##lblPostalAddressLine3##"))
                    sbHtml.Replace("##lblPostalAddressLine3##", ds.P2PDocument[0].PostalAddressLine3);
            }

        }

        /// <summary>
        /// In Order and IR at time of export this method will get call for exporting Header level comments including related documents.
        /// </summary>
        /// <param name="objExport"></param>
        /// <param name="sbMaterialLineItemHtml"></param>                
        public void FillHeaderComments(Export objExport, StringBuilder sbMaterialLineItemHtml)
        {

            string stHeaderGroupSectionHtml = string.Empty;
            string stHeaderCommentsSectionHtml = string.Empty;

            var sbHeaderCommentsHtml = new StringBuilder();
            var sbHeaderGroupHtml = new StringBuilder();
            var lstComments = objExport.DocumentDataSet.HeaderComment.ToList();
            if (lstComments.Count > 0)
            {
                stHeaderGroupSectionHtml = GetSectionHtml(sbMaterialLineItemHtml.ToString(), "^^HeaderGroup^^",
                                                   "^^/HeaderGroup^^");

                StringBuilder sbHeaderGroupSectionHtml = new StringBuilder();
                string strGroupText = string.Empty;
                foreach (var row in lstComments)
                {
                    if (string.IsNullOrEmpty(strGroupText))
                    {
                        sbHeaderGroupSectionHtml = new StringBuilder(stHeaderGroupSectionHtml);
                        stHeaderCommentsSectionHtml = GetSectionHtml(sbHeaderGroupSectionHtml.ToString(), "^^HeaderComments^^",
                                                              "^^/HeaderComments^^");

                        sbHeaderGroupSectionHtml.Replace("##lblGroup##", BaseDAO.HtmlEncode(row.GroupText));
                    }
                    else if (row.GroupText.Equals(strGroupText))
                    {
                        stHeaderCommentsSectionHtml = GetSectionHtml(sbHeaderGroupSectionHtml.ToString(), "^^HeaderComments^^",
                                                              "^^/HeaderComments^^");
                    }
                    else
                    {
                        if (stHeaderCommentsSectionHtml != null && stHeaderCommentsSectionHtml != string.Empty)
                            sbHeaderGroupSectionHtml.Replace(stHeaderCommentsSectionHtml, sbHeaderCommentsHtml.ToString());
                        sbHeaderGroupSectionHtml.Replace("^^HeaderGroup^^", string.Empty);
                        sbHeaderGroupSectionHtml.Replace("^^/HeaderGroup^^", string.Empty);

                        sbHeaderGroupHtml.Append(sbHeaderGroupSectionHtml.ToString());

                        sbHeaderGroupSectionHtml.Length = 0;
                        sbHeaderGroupSectionHtml = new StringBuilder(stHeaderGroupSectionHtml);

                        sbHeaderGroupSectionHtml.Replace("##lblGroup##", row.GroupText);
                        sbHeaderCommentsHtml.Length = 0;
                        stHeaderCommentsSectionHtml = GetSectionHtml(sbHeaderGroupSectionHtml.ToString(), "^^HeaderComments^^",
                                                              "^^/HeaderComments^^");

                    }
                    StringBuilder sbHeaderCommentsSectionHtml = new StringBuilder(stHeaderCommentsSectionHtml);
                    sbHeaderCommentsSectionHtml.Replace("##lblHeaderUserName##", BaseDAO.HtmlEncode(Convert.ToString(row.UserName)));
                    sbHeaderCommentsSectionHtml.Replace("##lblHeaderDateAndTime##", GetDate(row.DateCreated, DateFormatType.commentdate));
                    sbHeaderCommentsSectionHtml.Replace("##lblHeaderVisibility##", GetVisibilityComments(row.Visibility));
                    if (row.CommentText != null && row.CommentText != string.Empty)
                    {
                        sbHeaderCommentsSectionHtml.Replace("##lblHeaderComment##", Convert.ToString(row.CommentText.Replace("\n", "<br>")));
                    }
                    else
                    {
                        sbHeaderCommentsSectionHtml.Replace("##lblHeaderComment##", BaseDAO.HtmlEncode(Convert.ToString(row.CommentText)));
                    }
                    sbHeaderCommentsSectionHtml.Replace("^^HeaderComments^^", string.Empty);
                    sbHeaderCommentsSectionHtml.Replace("^^/HeaderComments^^", string.Empty);
                    sbHeaderCommentsHtml.Append(sbHeaderCommentsSectionHtml);
                    strGroupText = row.GroupText;

                }
                if (stHeaderCommentsSectionHtml != null && stHeaderCommentsSectionHtml != string.Empty)
                    sbHeaderGroupSectionHtml.Replace(stHeaderCommentsSectionHtml, sbHeaderCommentsHtml.ToString());
                sbHeaderGroupSectionHtml.Replace("^^HeaderGroup^^", string.Empty);
                sbHeaderGroupSectionHtml.Replace("^^/HeaderGroup^^", string.Empty);
                sbHeaderGroupHtml.Append(sbHeaderGroupSectionHtml.ToString());
                sbMaterialLineItemHtml.Replace("##HeaderCommentSection##", string.Empty);
                sbMaterialLineItemHtml.Replace("##/HeaderCommentSection##", string.Empty);
            }
            else
            {
                stHeaderGroupSectionHtml = GetSectionHtml(sbMaterialLineItemHtml.ToString(), "##HeaderCommentSection##",
                                                               "##/HeaderCommentSection##");
            }


            if (stHeaderGroupSectionHtml != null && stHeaderGroupSectionHtml != string.Empty)
                sbMaterialLineItemHtml.Replace(stHeaderGroupSectionHtml, sbHeaderGroupHtml.ToString());

        }

        public void FillCustomFields(long objectId, long formId, StringBuilder sbHtml, string level, List<Question> lstQuestion = null, bool isLinelevel = false)
        {
            List<KeyValuePair<string, string>> lstFields = new List<KeyValuePair<string, string>>();
            KeyValuePair<string, string> field;
            string fieldTmpl = String.Empty;
            string fieldSection = String.Empty;
            string fieldTableTmpl = String.Empty;
            string fieldTableSection = String.Empty;
            string fieldTableLableSection = String.Empty;
            string fieldTableRowSection = String.Empty;
            string fieldTableRowValueTmpl = String.Empty;
            string fieldTableSplitSection = String.Empty;
            int spiltCount = 4;
            StringBuilder fields = new StringBuilder();
            StringBuilder fieldInstance;
            StringBuilder filedRowInstance;
            StringBuilder fieldTables;
            StringBuilder fieldTable;
            StringBuilder fieldTableLable;
            StringBuilder fieldTableValue;

            //only get the table key pair value.
            Dictionary<string, Dictionary<string, List<string>>> tableResult = new Dictionary<string, Dictionary<string, List<string>>>();
            fieldSection = GetSectionHtml(sbHtml.ToString(), "##CustomAttrSectionFor" + level + "##", "##/CustomAttrSectionFor" + level + "##");
            fieldTmpl = GetSectionHtml(sbHtml.ToString(), "##CustomAttrTmplFor" + level + "##", "##/CustomAttrTmplFor" + level + "##");
            fieldTableSection = GetSectionHtml(sbHtml.ToString(), "##CustomAttrSectionTableFor" + level + "##", "##/CustomAttrSectionTableFor" + level + "##");
            fieldTableTmpl = GetSectionHtml(sbHtml.ToString(), "##CustomAttrTmplTableFor" + level + "##", "##/CustomAttrTmplTableFor" + level + "##");
            fieldTableLableSection = GetSectionHtml(fieldTableTmpl, "##CustomAtrrTableLabelSection##", "##/CustomAtrrTableLabelSection##");
            fieldTableRowSection = GetSectionHtml(fieldTableTmpl, "##CustomAttrTmplTableRowSection##", "##/CustomAttrTmplTableRowSection##");
            fieldTableRowValueTmpl = GetSectionHtml(fieldTableTmpl, "##CustomAtrrTableValueSection##", "##/CustomAtrrTableValueSection##");
            fieldTableSplitSection = GetSectionHtml(fieldTableTmpl, "##CustomAtrrTableSplitSection##", "##/CustomAtrrTableSplitSection##");

            RequisitionCommonManager commonManager = new RequisitionCommonManager (this.JWTToken){ UserContext = UserContext, GepConfiguration = GepConfiguration };
            if ((isLinelevel && lstQuestion != null && lstQuestion.Count > 0) || !isLinelevel)
                lstFields = commonManager.GetCustomFields(objectId, formId, tableResult, 0, lstQuestion);
            int count = lstFields.Count;
            if (fieldSection != "" && fieldTmpl != "")
            {
                if (count > 0)
                {
                    if (fieldTmpl.Contains("CustomAtrrLabel2"))
                        for (int i = 0; i < (count / 2 + 1); i++)
                        {
                            if (count > 2 * i)
                            {
                                fieldInstance = new StringBuilder(fieldTmpl);
                                field = lstFields[2 * i];
                                fieldInstance.Replace("##CustomAtrrLabel1##", field.Key);
                                fieldInstance.Replace("##CustomAtrrValue1##", field.Value);

                                if (count > 2 * i + 1)
                                {
                                    field = lstFields[2 * i + 1];
                                    fieldInstance.Replace("##CustomAtrrLabel2##", field.Key);
                                    fieldInstance.Replace("##CustomAtrrValue2##", field.Value);
                                }
                                else
                                {
                                    fieldInstance.Replace("##CustomAtrrLabel2##", string.Empty);
                                    fieldInstance.Replace("##CustomAtrrValue2##", string.Empty);
                                }
                                fields.Append(fieldInstance.ToString());
                            }
                        }
                    else
                        for (int i = 0; i < count; i++)
                        {
                            fieldInstance = new StringBuilder(fieldTmpl);
                            field = lstFields[i];
                            fieldInstance.Replace("##CustomAtrrLabel##", field.Key);
                            fieldInstance.Replace("##CustomAtrrValue##", field.Value);
                            fields.Append(fieldInstance.ToString());
                        }
                }
                if (!string.IsNullOrWhiteSpace(fieldSection))
                    sbHtml.Replace(fieldSection, fields.ToString());
                if (!string.IsNullOrWhiteSpace(fieldTmpl))
                    sbHtml.Replace(fieldTmpl, string.Empty);
                sbHtml.Replace("##CustomAttrTmplFor" + level + "##", string.Empty);
                sbHtml.Replace("##/CustomAttrTmplFor" + level + "##", string.Empty);
            }
            try
            {
                if (fieldSection != "" && fieldTableTmpl != "")
                {
                    fieldTables = new StringBuilder();

                    foreach (var tableName in tableResult.Keys)
                    {
                        Dictionary<string, List<string>> currentTable = tableResult[tableName];
                        fieldTable = new StringBuilder(fieldTableTmpl);
                        fieldTable.Replace("##CustomAtrrTableName##", tableName);
                        fieldTable.Replace("##CustomAttrTmplTableFor" + level + "##", string.Empty);
                        fieldTable.Replace("##/CustomAttrTmplTableFor" + level + "##", string.Empty);

                        if (currentTable.Keys.Count > 0)
                        {
                            StringBuilder splitSection = new StringBuilder();

                            for (int splitIndex = 0; splitIndex < Math.Ceiling((decimal)currentTable.Keys.Count / spiltCount); splitIndex++)
                            {
                                StringBuilder splitScetionTmpl = new StringBuilder(fieldTableSplitSection);

                                splitScetionTmpl.Replace("##CustomAtrrTableSplitSection##", string.Empty);
                                splitScetionTmpl.Replace("##/CustomAtrrTableSplitSection##", string.Empty);

                                StringBuilder labelTemplate = new StringBuilder();

                                for (int j = splitIndex * spiltCount; j < currentTable.Keys.Count && j < (splitIndex + 1) * spiltCount; j++)
                                {
                                    fieldTableLable = new StringBuilder(fieldTableLableSection);

                                    fieldTableLable.Replace("##CustomAtrrTableLabelSection##", string.Empty);
                                    fieldTableLable.Replace("##/CustomAtrrTableLabelSection##", string.Empty);

                                    string lable = currentTable.ElementAt(j).Key;

                                    fieldTableLable.Replace("##CustomAtrrTableLabel##", lable);

                                    labelTemplate.Append(fieldTableLable);
                                }

                                if (!string.IsNullOrWhiteSpace(fieldTableLableSection))
                                    splitScetionTmpl.Replace(fieldTableLableSection, labelTemplate.ToString());

                                StringBuilder tableRowFields = new StringBuilder();

                                for (int i = 0; i < currentTable.First().Value.Count; i++)
                                {
                                    if (String.Join("", currentTable.Values.Select(x => x[i])) == string.Empty)
                                    {
                                        continue;
                                    }

                                    filedRowInstance = new StringBuilder(fieldTableRowSection);

                                    filedRowInstance.Replace("##CustomAttrTmplTableRowSection##", string.Empty);

                                    filedRowInstance.Replace("##/CustomAttrTmplTableRowSection##", string.Empty);

                                    StringBuilder rowTemplate = new StringBuilder();

                                    for (int j = splitIndex * spiltCount; j < currentTable.Keys.Count && j < (splitIndex + 1) * spiltCount; j++)
                                    {
                                        fieldTableValue = new StringBuilder(fieldTableRowValueTmpl);

                                        fieldTableValue.Replace("##CustomAtrrTableValueSection##", string.Empty);

                                        fieldTableValue.Replace("##/CustomAtrrTableValueSection##", string.Empty);

                                        string value = currentTable.ElementAt(j).Value[i];

                                        fieldTableValue.Replace("##CustomAtrrTableRowValue##", value + "&nbsp;");

                                        rowTemplate.Append(fieldTableValue);
                                    }

                                    if (!String.IsNullOrWhiteSpace(fieldTableRowValueTmpl))
                                        filedRowInstance.Replace(fieldTableRowValueTmpl, rowTemplate.ToString());

                                    tableRowFields.Append(filedRowInstance);

                                }
                                if (!String.IsNullOrWhiteSpace(fieldTableRowSection))
                                    splitScetionTmpl.Replace(fieldTableRowSection, tableRowFields.ToString());

                                splitSection.Append(splitScetionTmpl);
                            }

                            if (!String.IsNullOrWhiteSpace(fieldTableSplitSection))
                                fieldTable.Replace(fieldTableSplitSection, splitSection.ToString());
                        }

                        fieldTables.Append(fieldTable);
                    }

                    if (!String.IsNullOrWhiteSpace(fieldTableSection))
                        sbHtml.Replace(fieldTableSection, fieldTables.ToString());
                    if (!String.IsNullOrWhiteSpace(fieldTableTmpl))
                        sbHtml.Replace(fieldTableTmpl, string.Empty);

                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void FillLineItemDetails(Export objExport, StringBuilder sbHtml, P2PDocumentType docType, int usertype = 0, bool enableBuyerInvoiceVisibilityToSupplier = false)
        {
            //get necessary settings - start
            RequisitionCommonManager commonManagernew = new RequisitionCommonManager (this.JWTToken){ UserContext = UserContext, GepConfiguration = GepConfiguration };
            SettingDetails objSettingsnew = commonManagernew.GetSettingsFromSettingsComponent(P2PDocumentType.None, 1, 107);
            stgBillableFieldAvailable = Convert.ToBoolean(commonManagernew.GetSettingsValueByKey(objSettingsnew, "BillableFieldAvailable"), CultureInfo.InvariantCulture).ToString().ToUpper();
            maxPrec = Convert.ToInt32(commonManagernew.GetSettingsValueByKey(objSettingsnew, "MaxPrecessionValue"));
            maxPrecTot = Convert.ToInt32(commonManagernew.GetSettingsValueByKey(objSettingsnew, "MaxPrecessionValueforTotal"));
            maxPrecTax = Convert.ToInt32(commonManagernew.GetSettingsValueByKey(objSettingsnew, "MaxPrecessionValueForTaxesAndCharges"));
            stgAllowLineNumber = Convert.ToBoolean(commonManagernew.GetSettingsValueByKey(objSettingsnew, "IsLineNumberVisible"));
            stgShowStanProcDetails = Convert.ToBoolean(commonManagernew.GetSettingsValueByKey(P2PDocumentType.Order, "ShowStanProcDetails", UserContext.ContactCode));

            SettingDetails objOrderSettings = commonManagernew.GetSettingsFromSettingsComponent(P2PDocumentType.Order, 1, 107);
            stgAllowManufacturersCodeAndModel = Convert.ToBoolean(commonManagernew.GetSettingsValueByKey(objOrderSettings, "AllowManufacturersCodeAndModel"));
            //get necessary settings - end

            //variables -start
            P2PDocumentDataSet.P2PItemDataTable p2PItem = objExport.DocumentDataSet.P2PItem;
            string strMaterialLineItemSectionHtml = "", strServiceLineItemSectionHtml = "", strAdvanceLineItemSectionHtml = "", strContingentWorkerLineItemSectionHtml = "";
            string strLineItemSectionHtml = "";
            //if common line item section html is present
            bool isLineItemSectionIsCommon = sbHtml.ToString().Contains("^^LineItem^^");
            List<Task> taskList = new List<Task>();
            //variables -end
            bool IsVABUser = commonManagernew.GetIsVABUserActive();
            //trigger async item bind -start
            if (p2PItem.Count > 0)
            {
                if (isLineItemSectionIsCommon)
                {
                    strLineItemSectionHtml = GetSectionHtml(sbHtml.ToString(), "^^LineItem^^", "^^/LineItem^^");
                    try
                    {
                        List<Tuple<long, long>> lstFormCodeId = new List<Tuple<long, long>>();
                        foreach (var item in p2PItem)
                        {
                            lstFormCodeId.Add(new Tuple<long, long>(item.DocumentItemId, Convert.ToInt64(item.FormID)));
                        }
                        LogNewRelic("Inside FillLineItemDetails at LN - 1396", "FillLineItemDetails-before-GetQuestionResponseByFormCode");
                        RequisitionDocumentManager  objP2PDocumentManager = new RequisitionDocumentManager(this.JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
                        var QuestionDetails = objP2PDocumentManager.GetQuestionResponseByFormCode(lstFormCodeId);
                        LogNewRelic("Inside FillLineItemDetails at LN - 1399", "FillLineItemDetails-after-GetQuestionResponseByFormCode");

                        Parallel.ForEach(p2PItem, row =>
                        {
                            List<Question> lstQuestion = QuestionDetails.Where(x => x.ListQuestionResponses.Where(qr => qr.ObjectInstanceId == row.DocumentItemId).Any()).ToList();
                            BindAllItem(row, docType, objExport, usertype, strLineItemSectionHtml, lstQuestion);
                        });
                    }
                    catch (Exception ex)
                    {
                        LogHelper.LogError(Log, "Error occurred in FillLineItemDetails method while Parallel.ForEach for BindAllItem in ExportManager.", ex);
                        throw;
                    }
                }
                else
                {
                    strMaterialLineItemSectionHtml = GetSectionHtml(sbHtml.ToString(), "^^MaterialLineItem^^", "^^/MaterialLineItem^^");
                    strServiceLineItemSectionHtml = GetSectionHtml(sbHtml.ToString(), "^^ServiceLineItem^^", "^^/ServiceLineItem^^");
                    strAdvanceLineItemSectionHtml = GetSectionHtml(sbHtml.ToString(), "^^AdvanceLineItem^^", "^^/AdvanceLineItem^^");
                    strContingentWorkerLineItemSectionHtml = GetSectionHtml(sbHtml.ToString(), "^^ContingentWorkerLineItem^^", "^^/ContingentWorkerLineItem^^");
                    try
                    {
                        List<Tuple<long, long>> lstFormCodeId = new List<Tuple<long, long>>();
                        foreach (var item in p2PItem)
                        {
                            lstFormCodeId.Add(new Tuple<long, long>(item.DocumentItemId, Convert.ToInt64(item.FormID)));
                        }
                        LogNewRelic("Inside FillLineItemDetails at LN - 1426", "FillLineItemDetails-before-GetQuestionResponseByFormCode");
                        RequisitionDocumentManager  objP2PDocumentManager = new RequisitionDocumentManager (this.JWTToken){ UserContext = UserContext, GepConfiguration = GepConfiguration };
                        var QuestionDetails = objP2PDocumentManager.GetQuestionResponseByFormCode(lstFormCodeId);
                        LogNewRelic("Inside FillLineItemDetails at LN - 1429", "FillLineItemDetails-after-GetQuestionResponseByFormCode");

                        Parallel.ForEach(p2PItem, row =>
                        {
                            List<Question> lstQuestion = QuestionDetails.Where(x => x.ListQuestionResponses.Where(qr => qr.ObjectInstanceId == row.DocumentItemId).Any()).ToList();

                            if (row.ItemTypeID == 1)
                                BindItem(row, docType, objExport, usertype, strMaterialLineItemSectionHtml, lstQuestion, enableBuyerInvoiceVisibilityToSupplier);
                            else if (row.ItemTypeID == 2 || row.ItemTypeID == 9 || row.ItemTypeID == 10)
                            {
                                if (Convert.ToString(row.ItemExtendedType).Trim().ToUpper().Replace(" ", "") == Enum.GetName(typeof(ItemExtendedType), ItemExtendedType.ContingentWorker).Trim().ToUpper() ||
                                Convert.ToString(row.ItemExtendedType).Trim().ToUpper().Replace(" ", "") == Enum.GetName(typeof(ItemExtendedType), ItemExtendedType.Expense).Trim().ToUpper())
                                {
                                    BindContingentWorkerItem(row, docType, objExport, usertype, strContingentWorkerLineItemSectionHtml, lstQuestion, enableBuyerInvoiceVisibilityToSupplier, IsVABUser);
                                }
                                else
                                {
                                    BindServiceItem(row, docType, objExport, usertype, strServiceLineItemSectionHtml, lstQuestion, enableBuyerInvoiceVisibilityToSupplier, IsVABUser);
                                }

                            }
                            else
                                BindAdvanceItem(row, docType, objExport, usertype, strAdvanceLineItemSectionHtml);
                        });
                    }
                    catch (Exception ex)
                    {
                        LogHelper.LogError(Log, "Error occurred in FillLineItemDetails method while Parallel.ForEach in ExportManager.", ex);
                        throw;
                    }
                }
            }
            //trigger async item bind -end

            //do other stuff while waiting -start
            StringBuilder sbLineItemDetailHtml = new StringBuilder();

            if (sbHtml.ToString().Contains("##lblShippingMethod##"))
            {
                if (p2PItem.Where(x => x.ItemTypeID == 1).Any())
                    sbHtml.Replace("##lblShippingMethod##", Convert.ToString(p2PItem.Where(x => x.ItemTypeID == 1).ToList()[0].ShippingMethod));
                sbHtml.Replace("##lblShippingMethod##", string.Empty);
            }
            //do other stuff while waiting -end
            if (isLineItemSectionIsCommon)
            {
                //append the binded all items -start
                if (tempList.Count > 0)
                {
                    var listTempList = tempList.ToList();
                    listTempList = listTempList.OrderBy(x => x.Key).ToList();
                    for (int f = 0; f < tempList.Count(); f++)
                    {
                        sbLineItemDetailHtml.Append(listTempList[f].Value);
                    }
                }
                else
                    strLineItemSectionHtml = GetSectionHtml(sbHtml.ToString(), "##LineItemSection##", "##/LineItemSection##");
                if (strLineItemSectionHtml != null && strLineItemSectionHtml != string.Empty)
                    sbHtml.Replace(strLineItemSectionHtml, sbLineItemDetailHtml.ToString());
                //append the binded all items -end

                sbHtml.Replace("##LineItemSection##", string.Empty);
                sbHtml.Replace("##/LineItemSection##", string.Empty);
            }
            else
            {
                //append the binded material items -start
                if (tempMaterialList.Count > 0)
                {
                    var listTempMaterialList = tempMaterialList.ToList();
                    listTempMaterialList = listTempMaterialList.OrderBy(x => x.Key).ToList();
                    for (int f = 0; f < listTempMaterialList.Count(); f++)
                    {
                        sbLineItemDetailHtml.Append(listTempMaterialList[f].Value);
                    }
                }
                else
                    strMaterialLineItemSectionHtml = GetSectionHtml(sbHtml.ToString(), "##MaterialLineItemSection##", "##/MaterialLineItemSection##");
                if (strMaterialLineItemSectionHtml != null && strMaterialLineItemSectionHtml != string.Empty)
                    sbHtml.Replace(strMaterialLineItemSectionHtml, sbLineItemDetailHtml.ToString());
                //append the binded material items -end

                sbLineItemDetailHtml = new StringBuilder();

                //append the binded service items -start
                if (tempServiceList.Count > 0)
                {
                    var listTempServiceList = tempServiceList.ToList();
                    listTempServiceList = listTempServiceList.OrderBy(x => x.Key).ToList();
                    for (int f = 0; f < listTempServiceList.Count(); f++)
                    {
                        sbLineItemDetailHtml.Append(listTempServiceList[f].Value);
                    }
                }
                else
                    strServiceLineItemSectionHtml = GetSectionHtml(sbHtml.ToString(), "##ServiceLineItemSection##", "##/ServiceLineItemSection##");
                if (strServiceLineItemSectionHtml != null && strServiceLineItemSectionHtml != string.Empty)
                    sbHtml.Replace(strServiceLineItemSectionHtml, sbLineItemDetailHtml.ToString());
                //append the binded service items -end

                //append the binded advance items -start
                sbLineItemDetailHtml = new StringBuilder();
                if (tempAdvanceList.Count > 0)
                {
                    var listTempAdvanceList = tempAdvanceList.ToList();
                    listTempAdvanceList = listTempAdvanceList.OrderBy(x => x.Key).ToList();
                    for (int f = 0; f < tempAdvanceList.Count(); f++)
                    {
                        sbLineItemDetailHtml.Append(listTempAdvanceList[f].Value);
                    }
                }
                else
                    strAdvanceLineItemSectionHtml = GetSectionHtml(sbHtml.ToString(), "##AdvanceLineItemSection##", "##/AdvanceLineItemSection##");
                if (strAdvanceLineItemSectionHtml != null && strAdvanceLineItemSectionHtml != string.Empty)
                    sbHtml.Replace(strAdvanceLineItemSectionHtml, sbLineItemDetailHtml.ToString());
                //append the binded advance items -end

                //append the binded contingent worker items -start
                if (tempContingentWorkerList.Count > 0)
                {
                    var listTempContingentWorkerList = tempContingentWorkerList.ToList();
                    listTempContingentWorkerList = listTempContingentWorkerList.OrderBy(x => x.Key).ToList();
                    for (int f = 0; f < tempContingentWorkerList.Count(); f++)
                    {
                        sbLineItemDetailHtml.Append(listTempContingentWorkerList[f].Value);
                    }
                }
                else
                {
                    strContingentWorkerLineItemSectionHtml = GetSectionHtml(sbHtml.ToString(), "##ContingentWorkerLineItemSection##", "##/ContingentWorkerLineItemSection##");
                }
                if (strContingentWorkerLineItemSectionHtml != null && strContingentWorkerLineItemSectionHtml != string.Empty)
                {
                    sbHtml.Replace(strContingentWorkerLineItemSectionHtml, sbLineItemDetailHtml.ToString());
                }
                //append the binded contingent worker items -end

                sbHtml.Replace("##ServiceLineItemSection##", string.Empty);
                sbHtml.Replace("##/ServiceLineItemSection##", string.Empty);
                sbHtml.Replace("##MaterialLineItemSection##", string.Empty);
                sbHtml.Replace("##/MaterialLineItemSection##", string.Empty);
                sbHtml.Replace("##AdvanceLineItemSection##", string.Empty);
                sbHtml.Replace("##/AdvanceLineItemSection##", string.Empty);
                sbHtml.Replace("##ContingentWorkerLineItemSection##", string.Empty);
                sbHtml.Replace("##/ContingentWorkerLineItemSection##", string.Empty);
            }
        }

        private void BindItem(P2PDocumentDataSet.P2PItemRow row, P2PDocumentType docType, Export objExport, int usertype, string strMaterialLineItemSectionHtml, List<Question> lstquestion = null, bool enableBuyerInvoiceVisibilityToSupplier = false)
        {
            try
            {
                RequisitionCommonManager commonManager = new RequisitionCommonManager (this.JWTToken){ UserContext = UserContext, GepConfiguration = GepConfiguration };
                SettingDetails objSettings = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.None, 1, 107);
                var AllowOtherCharges = Convert.ToBoolean(commonManager.GetSettingsValueByKey(objSettings, "AllowOtherCharges"), CultureInfo.InvariantCulture);
                P2PDocumentDataSet dsDocument = objExport.DocumentDataSet;
                int sNO = srNoMaterial++;

                var sbMaterialLineItemSectionHtml = new StringBuilder(strMaterialLineItemSectionHtml);
                if (stgAllowLineNumber)
                {
                    sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemSerialNo##", Convert.ToString(row.ItemLineNumber));
                }
                else
                {
                    sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemSerialNo##", sNO.ToString(CultureInfo.InvariantCulture));
                }

                sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemNumber##", BaseDAO.HtmlEncode(Convert.ToString(row.ItemNumber)));
                sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemDescription##", BaseDAO.HtmlEncode(row.Description));
                if (row.Quantity >= 0)
                    sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemQuantity##", ConvertToStringForQuantity(Math.Round(row.Quantity, maxPrec)));
                else
                    sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemQuantity##", ConvertToNegativeString(Math.Round(row.Quantity, maxPrec)));

                sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemUOM##", BaseDAO.HtmlEncode(row.UOMDesc));

                if (row.UnitPrice >= 0)
                    sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemUnitPrice##", ConvertToString(Math.Round(row.UnitPrice, maxPrec)));
                else
                    sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemUnitPrice##", ConvertToNegativeString(Math.Round(row.UnitPrice, maxPrec)));

                if (row.Total >= 0)
                {
                    sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemTotal##", ConvertToString(Math.Round((row.Total), maxPrecTot, MidpointRounding.AwayFromZero)));
                    sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemBaseTotal##", ConvertToString(Math.Round((row.TotalBaseItemPrice), maxPrecTot, MidpointRounding.AwayFromZero)));
                }
                else
                {
                    sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemTotal##", ConvertToNegativeString(Math.Round((row.Total), maxPrecTot, MidpointRounding.AwayFromZero)));
                    sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemBaseTotal##", ConvertToNegativeString(Math.Round((row.TotalBaseItemPrice), maxPrecTot, MidpointRounding.AwayFromZero)));
                }

                sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemCategory##", BaseDAO.HtmlEncode(Convert.ToString(row.CategoryName)));
                if (sbMaterialLineItemSectionHtml.ToString().Contains("##lblMaterialLineItemDispatchMode##") && Convert.ToString(row.EnableDispatchMode).ToUpper() == "TRUE")
                {
                    sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemDispatchMode##", BaseDAO.HtmlEncode(Convert.ToString(row.DispatchMode)));
                    sbMaterialLineItemSectionHtml.Replace("##MaterialLineLevelCustomField##", string.Empty);
                    sbMaterialLineItemSectionHtml.Replace("##/MaterialLineLevelCustomField##", string.Empty);
                }
                else
                {
                    string strMaterialLineLevelCustomFieldHtml = string.Empty;
                    strMaterialLineLevelCustomFieldHtml = GetSectionHtml(sbMaterialLineItemSectionHtml.ToString(), "##MaterialLineLevelCustomField##", "##/MaterialLineLevelCustomField##");
                    if (strMaterialLineLevelCustomFieldHtml != null && strMaterialLineLevelCustomFieldHtml != string.Empty)
                        sbMaterialLineItemSectionHtml.Replace(strMaterialLineLevelCustomFieldHtml, string.Empty);
                }
                if (sbMaterialLineItemSectionHtml.ToString().Contains("##lblMaterialLineItemMatchType##"))
                    sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemMatchType##", GetMatchType(row.MatchType));

                if (sbMaterialLineItemSectionHtml.ToString().Contains("##lblMaterialLineItemOrderingLocation##"))
                    sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemOrderingLocation##", row.OrderingLocation + ", " + row.OrderingLocationAddress);

                if (row.Tax >= 0)
                    sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemTaxes##", ConvertToString(Math.Round(row.Tax, maxPrecTax)));
                else
                    sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemTaxes##", ConvertToNegativeString(Math.Round(row.Tax, maxPrecTax)));

                sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemTaxExempt##", row.TaxExempt);
                if (row.HasFlexibleCharges != 1)
                {
                    if (row.ShippingCharges >= 0)
                        sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemShippingCharges##", ConvertToString(Math.Round(row.ShippingCharges, maxPrecTax)));
                    else
                        sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemShippingCharges##", ConvertToNegativeString(Math.Round(row.ShippingCharges, maxPrecTax)));
                    if (AllowOtherCharges)
                    {
                        if (row.AdditionalCharges >= 0)
                            sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemAdditional##", ConvertToString(Math.Round(row.AdditionalCharges, maxPrecTax)));
                        else
                            sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemAdditional##", ConvertToNegativeString(Math.Round(row.AdditionalCharges, maxPrecTax)));
                    }
                    else
                    {
                        sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemAdditional##", "");
                    }
                    var hiddenFlexible = GetSectionHtml(sbMaterialLineItemSectionHtml.ToString(), "##MaterialLineFlexibleChargeSection##", "##/MaterialLineFlexibleChargeSection##");
                    if (!string.IsNullOrWhiteSpace(hiddenFlexible))
                        sbMaterialLineItemSectionHtml.Replace(hiddenFlexible, string.Empty);
                    var hiddenFlexibleProperty = GetSectionHtml(sbMaterialLineItemSectionHtml.ToString(), "##MaterialLineFlexibleChargePropertySection##", "##/MaterialLineFlexibleChargePropertySection##");
                    if (!string.IsNullOrWhiteSpace(hiddenFlexibleProperty))
                        sbMaterialLineItemSectionHtml.Replace(hiddenFlexibleProperty, string.Empty);

                }
                else
                {
                    if (row.AdditionalCharges >= 0)
                    {
                        sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemTotalCharges##", ConvertToString(Math.Round(row.AdditionalCharges, maxPrecTax)));
                    }
                    else
                    {
                        sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemTotalCharges##", ConvertToNegativeString(Math.Round(row.AdditionalCharges, maxPrecTax)));
                    }
                    var hiddenDefault = GetSectionHtml(sbMaterialLineItemSectionHtml.ToString(), "##MaterialLineDefaultSection##", "##/MaterialLineDefaultSection##");
                    if (!string.IsNullOrWhiteSpace(hiddenDefault))
                        sbMaterialLineItemSectionHtml.Replace(hiddenDefault, string.Empty);
                    var hiddenDefaultProperty = GetSectionHtml(sbMaterialLineItemSectionHtml.ToString(), "##MaterialLineDefaultPropertySection##", "##/MaterialLineDefaultPropertySection##");
                    if (!string.IsNullOrWhiteSpace(hiddenDefaultProperty))
                        sbMaterialLineItemSectionHtml.Replace(hiddenDefaultProperty, string.Empty);
                }
                sbMaterialLineItemSectionHtml.Replace("##MaterialLineDefaultSection##", string.Empty);
                sbMaterialLineItemSectionHtml.Replace("##/MaterialLineDefaultSection##", string.Empty);
                sbMaterialLineItemSectionHtml.Replace("##MaterialLineFlexibleChargeSection##", string.Empty);
                sbMaterialLineItemSectionHtml.Replace("##/MaterialLineFlexibleChargeSection##", string.Empty);
                sbMaterialLineItemSectionHtml.Replace("##MaterialLineDefaultPropertySection##", string.Empty);
                sbMaterialLineItemSectionHtml.Replace("##/MaterialLineDefaultPropertySection##", string.Empty);
                sbMaterialLineItemSectionHtml.Replace("##MaterialLineFlexibleChargePropertySection##", string.Empty);
                sbMaterialLineItemSectionHtml.Replace("##/MaterialLineFlexibleChargePropertySection##", string.Empty);
                sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemRequestedDate##", string.IsNullOrWhiteSpace(row.DateRequested) ? string.Empty : GetDate(row.DateRequested, DateFormatType.DefaultDate));
                sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemNeedByDate##", string.IsNullOrWhiteSpace(row.DateNeeded) ? string.Empty : GetDate(row.DateNeeded, DateFormatType.DefaultDate));
                sbMaterialLineItemSectionHtml.Replace("##lblMaterialLinePartnerItemNumber##", BaseDAO.HtmlEncode(Convert.ToString(row.PartnerItemNumber)));
                if (sbMaterialLineItemSectionHtml.ToString().Contains("##lblLineItemShipFromLocationSection##"))
                    sbMaterialLineItemSectionHtml.Replace("##lblLineItemShipFromLocationSection##", BaseDAO.HtmlEncode(Convert.ToString(row.ShipFromLocation)));
                if (sbMaterialLineItemSectionHtml.ToString().Contains("##ItemExtendedType##"))
                    sbMaterialLineItemSectionHtml.Replace("##ItemExtendedType##", row.ItemExtendedType);
                if (sbMaterialLineItemSectionHtml.ToString().Contains("##lblLineItemShipFromAddressSection##"))
                    sbMaterialLineItemSectionHtml.Replace("##lblLineItemShipFromAddressSection##", BaseDAO.HtmlEncode(Convert.ToString(row.ShipFromAddress)));

                if (sbMaterialLineItemSectionHtml.ToString().Contains("##lblMaterialLineItemContractNo##"))
                    sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemContractNo##", Convert.ToString(row.ContractNo));
                if (sbMaterialLineItemSectionHtml.ToString().Contains("##lblMaterialLineItemPartnerName##"))
                    sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemPartnerName##", Convert.ToString(BaseDAO.HtmlEncode(row.PartnerName)));

                if (sbMaterialLineItemSectionHtml.ToString().Contains("##lblMaterialLineItemPartnerContactName##"))
                    sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemPartnerContactName##", Convert.ToString(BaseDAO.HtmlEncode(row.PartnerContactName)));

                if (sbMaterialLineItemSectionHtml.ToString().Contains("##lblMaterialLineItemPartnerContactEmail##"))
                    sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemPartnerContactEmail##", Convert.ToString(BaseDAO.HtmlEncode(row.PartnerContactEmail)));

                if (sbMaterialLineItemSectionHtml.ToString().Contains("##lblMaterialLineItemClientPartnerCode##"))
                    sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemClientPartnerCode##", Convert.ToString(BaseDAO.HtmlEncode(row.ClientPartnerCode)));

                if (sbMaterialLineItemSectionHtml.ToString().Contains("##lblMaterialLineItemRequisitionNumber##"))
                    sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemRequisitionNumber##", (row.RequisitionNumber == null) ? string.Empty : BaseDAO.HtmlEncode(Convert.ToString(row.RequisitionNumber)));

                 if (sbMaterialLineItemSectionHtml.ToString().Contains("##lblAllowFlexiblePrice##"))
                    sbMaterialLineItemSectionHtml.Replace("##lblAllowFlexiblePrice##", (row.FlexiblePrice ? "Yes" : "No"));

                if (sbMaterialLineItemSectionHtml.ToString().Contains("##lblItemCurrencyCode##"))
                    sbMaterialLineItemSectionHtml.Replace("##lblItemCurrencyCode##", Convert.ToString(row.CurrencyCode));

                    if (docType == P2PDocumentType.Requisition)
                {
                    FillOrderStatusDetails(row, sbMaterialLineItemSectionHtml, dsDocument.P2PDocument[0].DocumentStatusInfo);
                }
                if (sbMaterialLineItemSectionHtml.ToString().Contains("##lblMaterialLineItemBillable##") || sbMaterialLineItemSectionHtml.ToString().Contains("##THBillableCommentSection##"))
                {
                    if (stgBillableFieldAvailable == "TRUE")
                        sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemBillable##", (Convert.ToString(BaseDAO.HtmlEncode(row.Billable))).ToLower() == "billable" ? "Yes" : "No");
                    else
                    {
                        var billableH = GetSectionHtml(sbMaterialLineItemSectionHtml.ToString(), "##THBillableCommentSection##", "##/THBillableCommentSection##");
                        if (billableH != null && billableH != string.Empty)
                            sbMaterialLineItemSectionHtml.Replace(billableH, string.Empty);

                        var billableT = GetSectionHtml(sbMaterialLineItemSectionHtml.ToString(), "##TDBillableCommentSection##", "##/TDBillableCommentSection##");
                        if (billableT != null && billableT != string.Empty)
                            sbMaterialLineItemSectionHtml.Replace(billableT, string.Empty);

                    }
                    sbMaterialLineItemSectionHtml.Replace("##THBillableCommentSection##", string.Empty);
                    sbMaterialLineItemSectionHtml.Replace("##/THBillableCommentSection##", string.Empty);
                    sbMaterialLineItemSectionHtml.Replace("##TDBillableCommentSection##", string.Empty);
                    sbMaterialLineItemSectionHtml.Replace("##/TDBillableCommentSection##", string.Empty);
                }
                if (sbMaterialLineItemSectionHtml.ToString().Contains("##lblMaterialLineItemCapitalized##"))
                    sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemCapitalized##", BaseDAO.HtmlEncode(Convert.ToString(row.Capitalized)));
                if (sbMaterialLineItemSectionHtml.ToString().Contains("##MaterialLineItemShippingDetailSection##"))
                    FillMaterialLineItemShippingDetails(row, sbMaterialLineItemSectionHtml);
                // FillMaterialLineItemDeliverToDetails(row, sbMaterialLineItemSectionHtml);
                if (sbMaterialLineItemSectionHtml.ToString().Contains("##MaterialLineItemManufacturerDetailSection##") && (docType == P2PDocumentType.Order || docType == P2PDocumentType.Receipt || docType == P2PDocumentType.Requisition))
                {
                    FillMaterialLineItemManufacturerDetails(row, sbMaterialLineItemSectionHtml, stgAllowManufacturersCodeAndModel);
                }
                else
                {
                    if (sbMaterialLineItemSectionHtml.ToString().Contains("##lblMaterialLineItemManufacturerPartNumber##"))
                        sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemManufacturerPartNumber##", BaseDAO.HtmlEncode(Convert.ToString(row.ManufacturerPartNumber)));

                    if (sbMaterialLineItemSectionHtml.ToString().Contains("##lblMaterialLineItemManufacturerName##"))
                        sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemManufacturerName##", BaseDAO.HtmlEncode(Convert.ToString(row.ManufacturerName)));
                }
                //if (sbMaterialLineItemSectionHtml.ToString().Contains("##MaterialLineItemExceptionSection##") && docType == P2PDocumentType.InvoiceReconciliation)
                //{
                //    ExportIR objExportIR = (ExportIR)objExport;
                //    FillMaterialLineItemExceptions(objExportIR.IRDataSet.IRLineItemException.Where(t => t.IRItemId == row.DocumentItemId).ToList(),
                //    sbMaterialLineItemSectionHtml);
                //}
                if (docType == P2PDocumentType.Receipt)
                {
                    if (sbMaterialLineItemSectionHtml.ToString().Contains("##MaterialLineItemAssetTagDetailSection##"))
                    {
                        ExportReceipt objExportReceipt = (ExportReceipt)objExport;
                        FillReceiptAssettagDetails(objExportReceipt.ReceiptDataSet.AssetTag.Where(t => t.DocumentItemId == row.DocumentItemId).ToList(), sbMaterialLineItemSectionHtml);
                    }
                    sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemOrderedQuantity##", ConvertToStringForQuantity(row.OrderedQuantity));
                    sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemReceivedQuantity##", ConvertToStringForQuantity(row.ReceivedQuantity));
                    sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemAcceptedQuantity##", ConvertToStringForQuantity(row.AcceptedQuantity));
                    sbMaterialLineItemSectionHtml.Replace("##lblMaterialLinePartnerItemNumber##", BaseDAO.HtmlEncode(Convert.ToString(row.PartnerItemNumber)));
                    sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemRetunedQuantity##", ConvertToStringForQuantity(Convert.ToDecimal(row.ReturnedQuantity)));
                    sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemReasonRetuned##", Convert.ToString(row.ReasonForReturn));
                    sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemPReturnedQuantity##", ConvertToStringForQuantity(row.PreviouslyReturnedQuantity));
                    sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemReceivedDate##", string.IsNullOrWhiteSpace(row.ReceivedDate) ? string.Empty : GetDate(row.ReceivedDate, DateFormatType.DefaultDate));
                }
                else if (docType == P2PDocumentType.ServiceConfirmation)
                {
                    if (sbMaterialLineItemSectionHtml.ToString().Contains("##MaterialLineItemAssetTagDetailSection##"))
                    {
                        ExportReceipt objExportReceipt = (ExportReceipt)objExport;
                        FillReceiptAssettagDetails(objExportReceipt.ReceiptDataSet.AssetTag.Where(t => t.DocumentItemId == row.DocumentItemId).ToList(), sbMaterialLineItemSectionHtml);
                    }
                    sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemOrderedQuantity##", ConvertToStringForQuantity(row.OrderedQuantity));
                    sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemReceivedQuantity##", ConvertToStringForQuantity(row.Efforts));
                    sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemAcceptedQuantity##", ConvertToStringForQuantity(row.AcceptedQuantity));
                    //sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemRequesterName##", row.RequesterName);
                }
                //else if (docType == P2PDocumentType.Invoice)
                //{
                //    sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemOrderedQuantity##", ConvertToStringForQuantity(row.OrderedQuantity));
                //    sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemOrderedUnitPrice##", ConvertToString(row.OrderedUnitPrice));
                //    if (usertype == 0 || (usertype == 1 && dsDocument.P2PDocument[0].DocumentStatusInfo != 1 && dsDocument.P2PDocument[0].DocumentStatusInfo != 23 && dsDocument.P2PDocument[0].DocumentStatusInfo != 121 && dsDocument.P2PDocument[0].DocumentStatusInfo != 122))
                //    {
                //        sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemInvoicedAmount##", ConvertToString(row.InvoicedAmount));
                //        sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemRemainingAmount##", ConvertToString(row.RemainingAmount));
                //    }
                //    else
                //    {
                //        sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemInvoicedAmount##", "NA");
                //        sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemRemainingAmount##", "NA");
                //    }
                //    if (sbMaterialLineItemSectionHtml.ToString().Contains("##MaterialLineItemExceptionSection##"))
                //    {
                //        ExportInvoice objExportInvoice = (ExportInvoice)objExport;
                //        FillInvoiceMaterialLineItemExceptions(objExportInvoice.InvoiceDataSet.InvoiceLineItemException.Where(t => t.InvoiceItemId == row.DocumentItemId).ToList(),
                //        sbMaterialLineItemSectionHtml, usertype);
                //    }
                //    else if (sbMaterialLineItemSectionHtml.ToString().Contains("##MaterialLineItemErrorSection##"))
                //    {
                //        ExportInvoice objExportInvoice = (ExportInvoice)objExport;
                //        List<P2PDocumentDataSet.P2PItemRow> objExportInvoiceItem = objExportInvoice.DocumentDataSet.P2PItem.Where(t => t.DocumentItemId == row.DocumentItemId).ToList();
                //        FillInvoiceMaterialLineItemErrors(objExportInvoiceItem, sbMaterialLineItemSectionHtml, usertype);
                //    }
                //}
                else if (docType == P2PDocumentType.Order)
                {
                    sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemReceivingStatus##", Convert.ToBoolean(row.IsCloseForReceiving) ? "Closed" : "Open");
                    sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemInvoicingStatus##", Convert.ToBoolean(row.IsCloseForInvoicing) ? "Closed" : "Open");
                }
                if (row.SourceType == (int)ItemSourceType.PurchaseRequest && (docType == P2PDocumentType.Requisition || docType == P2PDocumentType.Order))
                {
                    // start
                    var questionnaires = dsDocument.Questionnaire.Where(t => t.DocumentItemId == row.DocumentItemId).ToList();
                    if (questionnaires.Count > 0)
                    {
                        var sbItemQuestionnairesHtml = FillQuestionnaireResponceDetails(sbMaterialLineItemSectionHtml, questionnaires);
                        string strItemQuestionnaireSectionHtml = GetSectionHtml(sbMaterialLineItemSectionHtml.ToString(), "^^LineItemQuestionnaire^^",
                                                                       "^^/LineItemQuestionnaire^^");
                        if (!string.IsNullOrWhiteSpace(strItemQuestionnaireSectionHtml))
                            sbMaterialLineItemSectionHtml.Replace(strItemQuestionnaireSectionHtml, sbItemQuestionnairesHtml.ToString());
                        sbMaterialLineItemSectionHtml.Replace("##QuestionnaireSection##", string.Empty);
                        sbMaterialLineItemSectionHtml.Replace("##/QuestionnaireSection##", string.Empty);
                    }
                    //end
                }
                else
                {
                    string strItemQuestionnaireSectionHtml = GetSectionHtml(sbMaterialLineItemSectionHtml.ToString(), "##QuestionnaireSection##",
                                                                   "##/QuestionnaireSection##");
                    if (!string.IsNullOrWhiteSpace(strItemQuestionnaireSectionHtml))
                        sbMaterialLineItemSectionHtml.Replace(strItemQuestionnaireSectionHtml, string.Empty);
                }
                sbMaterialLineItemSectionHtml.Replace("^^MaterialLineItem^^", string.Empty);
                sbMaterialLineItemSectionHtml.Replace("^^/MaterialLineItem^^", string.Empty);
                if (sbMaterialLineItemSectionHtml.ToString().Contains("^^MaterialLineItemGroup^^"))
                    FillMaterialLineItemComments(
                        dsDocument.LineItemComment.Where(t => t.P2PLineItemId == row.P2PLineItemId).ToList(),
                        sbMaterialLineItemSectionHtml);
                if (sbMaterialLineItemSectionHtml.ToString().Contains("^^MaterialLineItemRequesterDetail^^"))
                    FillMaterialLineItemRequesterDetails(
                        dsDocument.RequesterDetails.Where(t => t.DocumentItemId == row.DocumentItemId).ToList(),
                        sbMaterialLineItemSectionHtml);
                long itm = docType == P2PDocumentType.InvoiceReconciliation ? row.InvoiceItemId : row.DocumentItemId;
                FillCustomFields(itm, Convert.ToInt64(row.FormID), sbMaterialLineItemSectionHtml, "Item", lstquestion, true);
                FillAccountingFields(dsDocument.AccountingDetails.Where(x => x.DocumentItemID == row.DocumentItemId).ToList(), sbMaterialLineItemSectionHtml, row.ItemTypeID, docType);
                FillRequisitionItemAdditionalFields(objExport, row.DocumentItemId, sbMaterialLineItemSectionHtml);
                if (stgShowStanProcDetails && dsDocument.P2PDocument[0].DocumentSourceTypeInfo != 6)
                {
                    FillStandardProcedureDetails(Convert.ToInt64(dsDocument.P2PDocument[0].DocumentCode), sbMaterialLineItemSectionHtml, Convert.ToInt64(row.DocumentItemId), 2, dsDocument);
                }
                else
                {
                    var showStanProcDetailsText = GetSectionHtml(sbMaterialLineItemSectionHtml.ToString(), "##MaterialLineSPDetailSection##", "##/MaterialLineSPDetailSection##");
                    if (showStanProcDetailsText != null && showStanProcDetailsText != string.Empty)
                        sbMaterialLineItemSectionHtml.Replace(showStanProcDetailsText, string.Empty);
                }

                if (docType == P2PDocumentType.Requisition)
                {
                    FillStockReservationDetails(sbMaterialLineItemSectionHtml, objSettings, "Material", row);

                    // Add Source Type description as Item Source
                    sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemItemSource##", row.SourceTypeDescription);
                }

                if (docType == P2PDocumentType.Receipt || docType == P2PDocumentType.ReturnNote)
                {
                    if (sbMaterialLineItemSectionHtml.ToString().Contains("##MultipleOrderDetailSection##"))
                        FillReceiptItemOrderDetails(row, sbMaterialLineItemSectionHtml);
                }

                var advancePaymentSection = GetSectionHtml(sbMaterialLineItemSectionHtml.ToString(), "##AdvancePaymentSection##",
                                                               "##/AdvancePaymentSection##");
                if (!string.IsNullOrWhiteSpace(advancePaymentSection))
                {
                    FillAdvancePaymentDetails(row, sbMaterialLineItemSectionHtml, advancePaymentSection, Convert.ToInt64(dsDocument.P2PDocument[0].DocumentCode));
                }

                long num = stgAllowLineNumber ? row.ItemLineNumber : sNO;
                tempMaterialList.Add(new KeyValuePair<long, StringBuilder>(num, sbMaterialLineItemSectionHtml));
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occurred in BindItem method in ExportManager for P2PLineItemId = " + row.P2PLineItemId, ex);
                throw;
            }
        }

        public StringBuilder FillStockReservationDetails(StringBuilder sbgenericLineItemSectionHtml, SettingDetails objSettings, string source, P2PDocumentDataSet.P2PItemRow row)
        {
            RequisitionCommonManager commonManager = new RequisitionCommonManager (this.JWTToken){ UserContext = UserContext, GepConfiguration = GepConfiguration };
            var enableStockReservation = Convert.ToBoolean(commonManager.GetSettingsValueByKey(objSettings, "EnableStockReservationViaExternalInventoryIntegration"), CultureInfo.InvariantCulture);

            FillLineColumnSection(sbgenericLineItemSectionHtml, enableStockReservation, source, "InventoryType", GetInventoryType(row.InventoryType));
            FillLineColumnSection(sbgenericLineItemSectionHtml, enableStockReservation, source, "Procurable", GetProcurableType(row.isProcurable));
            FillLineColumnSection(sbgenericLineItemSectionHtml, enableStockReservation, source, "LineStatus", GetLineStatus(row.LineStatus));
            FillLineColumnSection(sbgenericLineItemSectionHtml, enableStockReservation, source, "StockReservationNumber", row.StockReservationNumber);


            return sbgenericLineItemSectionHtml;

        }

        public void FillReceiptItemOrderDetails(P2PDocumentDataSet.P2PItemRow objMultiPOItem, StringBuilder sbHtml)
        {
            try
            {
                string strMultipleOrderDetailSectionHtml = string.Empty;
                var sbMultipleOrderDetailHtml = new StringBuilder();
                RequisitionCommonManager commonManager = new RequisitionCommonManager (this.JWTToken){ UserContext = UserContext, GepConfiguration = GepConfiguration };
                SettingDetails objSettings = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.Receipt, 1, 107);

                strMultipleOrderDetailSectionHtml = GetSectionHtml(sbHtml.ToString(), "##MultipleOrderDetailSection##", "##/MultipleOrderDetailSection##");

                if (commonManager.GetSettingsValueByKey(objSettings, "EnableMultiPOReceiving").ToUpper() == "YES")
                {
                    var sbMultipleOrderDetailSectionHtml = new StringBuilder(strMultipleOrderDetailSectionHtml);

                    sbMultipleOrderDetailSectionHtml.Replace("##lblOrderDocumentLineNumber##", BaseDAO.HtmlEncode(Convert.ToString(objMultiPOItem.OrderLineNumber)));
                    sbMultipleOrderDetailSectionHtml.Replace("##lblOrderDocumentNumber##", BaseDAO.HtmlEncode(objMultiPOItem.OrderNumber));
                    sbMultipleOrderDetailSectionHtml.Replace("##lblOrderDocumentName##", BaseDAO.HtmlEncode(objMultiPOItem.OrderName));

                    sbMultipleOrderDetailSectionHtml.Replace("##MultipleOrderDetailSection##", string.Empty);
                    sbMultipleOrderDetailSectionHtml.Replace("##/MultipleOrderDetailSection##", string.Empty);
                    sbMultipleOrderDetailHtml.Append(sbMultipleOrderDetailSectionHtml.ToString());

                    if (strMultipleOrderDetailSectionHtml != null && strMultipleOrderDetailSectionHtml != string.Empty)
                        sbHtml.Replace(strMultipleOrderDetailSectionHtml, sbMultipleOrderDetailHtml.ToString());
                }
                else
                {
                    if (strMultipleOrderDetailSectionHtml != null && strMultipleOrderDetailSectionHtml != string.Empty)
                        sbHtml.Replace(strMultipleOrderDetailSectionHtml, string.Empty);
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occurred in FillReceiptItemOrderDetails method in ExportManager.", ex);
                throw;
            }
        }

        public void FillAdvancePaymentDetails(P2PDocumentDataSet.P2PItemRow objReqItem, StringBuilder sbHtml, string advancePaymentSection, long documentCode)
        {
            try
            {
                var EnableAdvancePayment = false;
                RequisitionCommonManager commonManager = new RequisitionCommonManager(this.JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
                long LOBId = GetCommonDao().GetLOBByDocumentCode(documentCode);
                SettingDetails objSettings = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.None, 1, 107, "", LOBId);
                var getEnableAdvancePaymentSettingValue = commonManager.GetSettingsValueByKey(objSettings, "EnableAdvancePayment");
                if (getEnableAdvancePaymentSettingValue.ToUpper() == "YES" || getEnableAdvancePaymentSettingValue.ToUpper() == "TRUE")
                {
                    EnableAdvancePayment = true;
                }
                if (EnableAdvancePayment)
                {
                    sbHtml.Replace("##lblMaterialLineItemAllowAdvances##", objReqItem.AllowAdvances);
                    if (objReqItem.AllowAdvances == "Yes")
                    {
                        sbHtml.Replace("##lblMaterialLineItemAdvanceAmount##", ConvertToString(Math.Round(objReqItem.AdvanceAmount, maxPrec)));
                        sbHtml.Replace("##lblMaterialLineItemAdvancePercentage##", ConvertToString(Math.Round(objReqItem.AdvancePercentage, maxPrec)));
                        sbHtml.Replace("##lblMaterialLineItemAdvanceReleaseDate##", string.IsNullOrWhiteSpace(objReqItem.AdvanceReleaseDate) ? string.Empty : GetDate(objReqItem.AdvanceReleaseDate, DateFormatType.DefaultDate));
                    }
                    else
                    {
                        sbHtml.Replace("##lblMaterialLineItemAdvanceAmount##", string.Empty);
                        sbHtml.Replace("##lblMaterialLineItemAdvancePercentage##", string.Empty);
                        sbHtml.Replace("##lblMaterialLineItemAdvanceReleaseDate##", string.Empty);
                    }
                    sbHtml.Replace("##AdvancePaymentSection##", string.Empty);
                    sbHtml.Replace("##/AdvancePaymentSection##", string.Empty);
                }
                else
                {
                    sbHtml.Replace(advancePaymentSection, string.Empty);
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occurred in FillAdvancePaymentDetails method in ExportManager.", ex);
                throw;
            }
        }

        /// <summary>
        /// format of MatchType in Different Sections
        /// lblServiceLineItemMatchType,lblContingentWorkerLineItemMatchType,lblLineItemMatchType,lblMaterialLineItemMatchType
        /// lblMaterialLineItemTaxes,lblLineItemTaxes,lblMaterialLineItemTaxes
        /// 
        /// format for table header  Sections
        ///   ##MaterialLineLevelLineStatusHeaderSection## ,##/MaterialLineLevelLineStatusHeaderSection##
        ///   
        /// format for table Data  Sections
        /// ##MaterialLineLevelLineStatusDataSection##,##/MaterialLineLevelLineStatusDataSection##
        /// </summary>
        /// <param name="sbgenericLineItemSectionHtml"></param>
        /// <param name="DisplayStatus"></param>
        /// <param name="source"></param>
        /// <param name="columnName"></param>
        /// <param name="columnValue"></param>
        /// <returns></returns>
        public StringBuilder FillLineColumnSection(StringBuilder sbgenericLineItemSectionHtml, bool DisplayStatus, string source, string columnName, string columnValue)
        {

            string linelevelColumnHeadersection = "##" + source + "LineLevel" + columnName + "HeaderSection##";
            string linelevelColumnDataSection = "##" + source + "LineLevel" + columnName + "DataSection##";


            if (!DisplayStatus)
            {
                //--Disabling Columns start
                var hiddenLineLevelColumnHeaderSection = GetSectionHtml(sbgenericLineItemSectionHtml.ToString(), linelevelColumnHeadersection, "##/" + source + "LineLevel" + columnName + "HeaderSection##");
                if (hiddenLineLevelColumnHeaderSection != string.Empty)
                    sbgenericLineItemSectionHtml.Replace(hiddenLineLevelColumnHeaderSection, string.Empty);

                var hiddenLineLevelDataSection = GetSectionHtml(sbgenericLineItemSectionHtml.ToString(), linelevelColumnDataSection, "##/" + source + "LineLevel" + columnName + "DataSection##");
                if (hiddenLineLevelDataSection != string.Empty)
                    sbgenericLineItemSectionHtml.Replace(hiddenLineLevelDataSection, string.Empty);
                //--Disabling Columns --end
            }
            else
            {
                //assigning values to cloumns if displayed
                if (sbgenericLineItemSectionHtml.ToString().Contains("##lbl" + source + "LineItem" + columnName + "##"))
                    sbgenericLineItemSectionHtml.Replace("##lbl" + source + "LineItem" + columnName + "##", columnValue);

            }
            // clearing table header and data sections string on pdf
            sbgenericLineItemSectionHtml.Replace(linelevelColumnHeadersection, string.Empty);
            sbgenericLineItemSectionHtml.Replace("##/" + source + "LineLevel" + columnName + "HeaderSection##", string.Empty);
            sbgenericLineItemSectionHtml.Replace(linelevelColumnDataSection, string.Empty);
            sbgenericLineItemSectionHtml.Replace("##/" + source + "LineLevel" + columnName + "DataSection##", string.Empty);

            return sbgenericLineItemSectionHtml;
        }

        private void BindServiceItem(P2PDocumentDataSet.P2PItemRow row, P2PDocumentType docType, Export objExport, int usertype, string strServiceLineItemSectionHtml, List<Question> lstquestion = null, bool enableBuyerInvoiceVisibilityToSupplier = false, bool IsVABUser = false)
        {
            try
            {
                RequisitionCommonManager commonManager = new RequisitionCommonManager (this.JWTToken){ UserContext = UserContext, GepConfiguration = GepConfiguration };
                SettingDetails objSettings = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.None, 1, 107);
                var AllowOtherCharges = Convert.ToBoolean(commonManager.GetSettingsValueByKey(objSettings, "AllowOtherCharges"), CultureInfo.InvariantCulture);
                P2PDocumentDataSet dsDocument = objExport.DocumentDataSet;
                int sNO = srNoServicenew++;

                var sbServiceLineItemSectionHtml = new StringBuilder(strServiceLineItemSectionHtml);
                if (stgAllowLineNumber)
                {
                    sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemSerialNo##", Convert.ToString(row.ItemLineNumber));
                }
                else
                {
                    sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemSerialNo##", sNO.ToString(CultureInfo.InvariantCulture));
                }
                sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemDescription##", BaseDAO.HtmlEncode(Convert.ToString(row.Description)));
                //sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemNumber##", Convert.ToString(row.ItemNumber));
                if (row.Total >= 0)
                {
                    sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemTotal##", ConvertToString(Math.Round(row.Total, maxPrecTot, MidpointRounding.AwayFromZero)));
                    sbServiceLineItemSectionHtml.Replace("##lblServiceItemTotal##", ConvertToString(Math.Round(row.Total, maxPrecTot, MidpointRounding.AwayFromZero)));
                    sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemBaseTotal##", ConvertToString(Math.Round(row.TotalBaseItemPrice, maxPrecTot, MidpointRounding.AwayFromZero)));
                }
                else
                {
                    sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemTotal##", ConvertToNegativeString(Math.Round(row.Total, maxPrecTot, MidpointRounding.AwayFromZero)));
                    sbServiceLineItemSectionHtml.Replace("##lblServiceItemTotal##", ConvertToNegativeString(Math.Round(row.Total, maxPrecTot, MidpointRounding.AwayFromZero)));
                    sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemBaseTotal##", ConvertToNegativeString(Math.Round(row.TotalBaseItemPrice, maxPrecTot, MidpointRounding.AwayFromZero)));
                }
                if (row.UnitPrice >= 0)
                    sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemUnitPrice##", ConvertToString(Math.Round(row.UnitPrice, maxPrec)));
                else
                    sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemUnitPrice##", ConvertToNegativeString(Math.Round(row.UnitPrice, maxPrec)));

                if (Convert.ToString(row.ItemExtendedType).Trim().ToUpper() == Enum.GetName(typeof(ItemExtendedType), ItemExtendedType.Progress).Trim().ToUpper() || Convert.ToString(row.ItemExtendedType).Trim().ToUpper() == Enum.GetName(typeof(ItemExtendedType), ItemExtendedType.MileStone).Trim().ToUpper())
                {
                    sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemUOM##", "-");
                    sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemNumber##", "-");
                    sbServiceLineItemSectionHtml.Replace("##lblServiceLinePartnerItemNumber##", "-");
                }
                else
                {
                    sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemUOM##", BaseDAO.HtmlEncode(Convert.ToString(row.UOMDesc)));
                    sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemNumber##", Convert.ToString(row.ItemNumber));
                    sbServiceLineItemSectionHtml.Replace("##lblServiceLinePartnerItemNumber##", BaseDAO.HtmlEncode(Convert.ToString(row.PartnerItemNumber)));
                }
                if (Convert.ToString(row.ItemExtendedType).Trim().ToUpper() == Enum.GetName(typeof(ItemExtendedType), ItemExtendedType.Fixed).Trim().ToUpper() || Convert.ToString(row.ItemExtendedType).Trim().ToUpper() == Enum.GetName(typeof(ItemExtendedType), ItemExtendedType.Progress).Trim().ToUpper() || Convert.ToString(row.ItemExtendedType).Trim().ToUpper() == Enum.GetName(typeof(ItemExtendedType), ItemExtendedType.MileStone).Trim().ToUpper())
                {
                    sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemAcceptedQuantity##", @"N/A");
                    sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemEfforts##", "-");
                }
                else
                {
                    if (row.Efforts >= 0)
                    {
                        sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemAcceptedQuantity##", ConvertToStringForQuantity(row.ReceivedQuantity));
                        sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemEfforts##", ConvertToStringForQuantity(row.Efforts));
                    }
                    else
                    {
                        sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemAcceptedQuantity##", ConvertToNegativeString(row.ReceivedQuantity));
                        sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemEfforts##", ConvertToNegativeString(row.Efforts));
                    }
                }
                if (Convert.ToString(row.ItemExtendedType).Trim().ToUpper() == Enum.GetName(typeof(ItemExtendedType), ItemExtendedType.Variable).Trim().ToUpper())
                {
                    sbServiceLineItemSectionHtml.Replace("##lblServiceLineOrderedQuantity##", ConvertToStringForQuantity(row.OrderedQuantity));
                }
                else
                {
                    sbServiceLineItemSectionHtml.Replace("##lblServiceLineOrderedQuantity##", "0.00");
                }
                if (sbServiceLineItemSectionHtml.ToString().Contains("##lblServiceLineItemCategory##"))
                    sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemCategory##", BaseDAO.HtmlEncode(Convert.ToString(row.CategoryName)));

                if (sbServiceLineItemSectionHtml.ToString().Contains("##lblServiceLineItemDispatchMode##") && Convert.ToString(row.EnableDispatchMode).ToUpper() == "TRUE")
                {
                    sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemDispatchMode##", BaseDAO.HtmlEncode(Convert.ToString(row.DispatchMode)));
                    sbServiceLineItemSectionHtml.Replace("##ServiceLineLevelCustomField##", string.Empty);
                    sbServiceLineItemSectionHtml.Replace("##/ServiceLineLevelCustomField##", string.Empty);
                }
                else
                {
                    string strServiceLineLevelCustomFieldHtml = string.Empty;
                    strServiceLineLevelCustomFieldHtml = GetSectionHtml(sbServiceLineItemSectionHtml.ToString(), "##ServiceLineLevelCustomField##",
                                                                   "##/ServiceLineLevelCustomField##");
                    if (strServiceLineLevelCustomFieldHtml != null && strServiceLineLevelCustomFieldHtml != string.Empty)
                        sbServiceLineItemSectionHtml.Replace(strServiceLineLevelCustomFieldHtml, string.Empty);
                }

                if (sbServiceLineItemSectionHtml.ToString().Contains("##lblServiceLineItemMatchType##"))
                    sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemMatchType##", GetMatchType(row.MatchType));
                if (sbServiceLineItemSectionHtml.ToString().Contains("##lblServiceLineItemStartDate##"))
                    sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemStartDate##", string.IsNullOrWhiteSpace(row.StartDate) ? string.Empty : GetDate(row.StartDate, DateFormatType.DefaultDate));
                if (sbServiceLineItemSectionHtml.ToString().Contains("##lblServiceLineItemEndDate##"))
                    sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemEndDate##", string.IsNullOrWhiteSpace(row.EndDate) ? string.Empty : GetDate(row.EndDate, DateFormatType.DefaultDate));
                if (sbServiceLineItemSectionHtml.ToString().Contains("##lblServiceLineItemTax##"))
                    sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemTax##", ConvertToString(Math.Round(row.Tax, maxPrecTax)));
                if (sbServiceLineItemSectionHtml.ToString().Contains("##lblServiceLineItemServiceType##"))
                    sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemServiceType##", BaseDAO.HtmlEncode(Convert.ToString(row.ItemExtendedType)));
                if (sbServiceLineItemSectionHtml.ToString().Contains("##lblServiceLineItemTaxExempt##"))
                    sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemTaxExempt##", Convert.ToString(row.TaxExempt));
                if (sbServiceLineItemSectionHtml.ToString().Contains("##ItemExtendedType##"))
                    sbServiceLineItemSectionHtml.Replace("##ItemExtendedType##", row.ItemExtendedType);
                if (sbServiceLineItemSectionHtml.ToString().Contains("##lblServiceLineItemOrderingLocation##"))
                    sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemOrderingLocation##", row.OrderingLocation + ", " + row.OrderingLocationAddress);
                if ((docType == P2PDocumentType.Requisition || (docType == P2PDocumentType.Order && (!IsVABUser))) && usertype == 0 && sbServiceLineItemSectionHtml.ToString().Contains("##lblServiceLineItemOverallItemLimit##")
                        && row.OverallItemLimit > 0 && (P2PPurchaseType)dsDocument.P2PDocument[0].PurchaseTypeId == P2PPurchaseType.BlanketOrder)
                {
                    sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemOverallItemLimit##", ConvertToString(Math.Round(row.OverallItemLimit, maxPrec)));
                }
                else
                {
                    sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemOverallItemLimit##", string.Empty);
                    var hiddenServiceOverallItemLimit = GetSectionHtml(sbServiceLineItemSectionHtml.ToString(), "##ServiceOverallItemLimitSection##", "##/ServiceOverallItemLimitSection##");
                    if (!string.IsNullOrWhiteSpace(hiddenServiceOverallItemLimit))
                        sbServiceLineItemSectionHtml.Replace(hiddenServiceOverallItemLimit, string.Empty);
                    var hiddenServiceOverallItemLimitProperty = GetSectionHtml(sbServiceLineItemSectionHtml.ToString(), "##ServiceOverallItemLimitPropertySection##", "##/ServiceOverallItemLimitPropertySection##");
                    if (!string.IsNullOrWhiteSpace(hiddenServiceOverallItemLimitProperty))
                        sbServiceLineItemSectionHtml.Replace(hiddenServiceOverallItemLimitProperty, string.Empty);
                }
                sbServiceLineItemSectionHtml.Replace("##ServiceOverallItemLimitSection##", string.Empty);
                sbServiceLineItemSectionHtml.Replace("##/ServiceOverallItemLimitSection##", string.Empty);
                sbServiceLineItemSectionHtml.Replace("##ServiceOverallItemLimitPropertySection##", string.Empty);
                sbServiceLineItemSectionHtml.Replace("##/ServiceOverallItemLimitPropertySection##", string.Empty);
                if (sbServiceLineItemSectionHtml.ToString().Contains("##lblServiceItemCategory##"))
                    sbServiceLineItemSectionHtml.Replace("##lblServiceItemCategory##", BaseDAO.HtmlEncode(Convert.ToString(row.CategoryName)));
                if (row.HasFlexibleCharges != 1)
                {
                    if (sbServiceLineItemSectionHtml.ToString().Contains("##lblServiceLineItemAdditional##"))
                        if (AllowOtherCharges)
                        {
                            if (row.AdditionalCharges >= 0)
                                sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemAdditional##", ConvertToString(row.AdditionalCharges));
                            else
                                sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemAdditional##", ConvertToNegativeString(row.AdditionalCharges));
                        }
                        else
                        {
                            sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemAdditional##", "");
                        }
                    var hiddenFlexible = GetSectionHtml(sbServiceLineItemSectionHtml.ToString(), "##ServiceLineFlexibleChargeSection##", "##/ServiceLineFlexibleChargeSection##");
                    if (!string.IsNullOrWhiteSpace(hiddenFlexible))
                        sbServiceLineItemSectionHtml.Replace(hiddenFlexible, string.Empty);
                    var hiddenFlexibleProperty = GetSectionHtml(sbServiceLineItemSectionHtml.ToString(), "##ServiceLineFlexibleChargePropertySection##", "##/ServiceLineFlexibleChargePropertySection##");
                    if (!string.IsNullOrWhiteSpace(hiddenFlexibleProperty))
                        sbServiceLineItemSectionHtml.Replace(hiddenFlexibleProperty, string.Empty);
                }
                else
                {
                    if (sbServiceLineItemSectionHtml.ToString().Contains("##lblServiceLineItemTotalCharges##"))
                    {
                        if (row.AdditionalCharges >= 0)
                        {
                            sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemTotalCharges##", ConvertToString(Math.Round(row.AdditionalCharges, maxPrecTax)));
                        }
                        else
                        {
                            sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemTotalCharges##", ConvertToNegativeString(Math.Round(row.AdditionalCharges, maxPrecTax)));
                        }
                    }
                    var hiddenDefault = GetSectionHtml(sbServiceLineItemSectionHtml.ToString(), "##ServiceLineDefaultSection##", "##/ServiceLineDefaultSection##");
                    if (!string.IsNullOrWhiteSpace(hiddenDefault))
                        sbServiceLineItemSectionHtml.Replace(hiddenDefault, string.Empty);
                    var hiddenDefaultProperty = GetSectionHtml(sbServiceLineItemSectionHtml.ToString(), "##ServiceLineDefaultPropertySection##", "##/ServiceLineDefaultPropertySection##");
                    if (!string.IsNullOrWhiteSpace(hiddenDefaultProperty))
                        sbServiceLineItemSectionHtml.Replace(hiddenDefaultProperty, string.Empty);
                }
                sbServiceLineItemSectionHtml.Replace("##ServiceLineDefaultSection##", string.Empty);
                sbServiceLineItemSectionHtml.Replace("##/ServiceLineDefaultSection##", string.Empty);
                sbServiceLineItemSectionHtml.Replace("##ServiceLineFlexibleChargeSection##", string.Empty);
                sbServiceLineItemSectionHtml.Replace("##/ServiceLineFlexibleChargeSection##", string.Empty);
                sbServiceLineItemSectionHtml.Replace("##ServiceLineDefaultPropertySection##", string.Empty);
                sbServiceLineItemSectionHtml.Replace("##/ServiceLineDefaultPropertySection##", string.Empty);
                sbServiceLineItemSectionHtml.Replace("##ServiceLineFlexibleChargePropertySection##", string.Empty);
                sbServiceLineItemSectionHtml.Replace("##/ServiceLineFlexibleChargePropertySection##", string.Empty);
                if (sbServiceLineItemSectionHtml.ToString().Contains("##lblServiceLineItemPartnerName##"))
                    sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemPartnerName##", BaseDAO.HtmlEncode(Convert.ToString(row.PartnerName)));

                if (sbServiceLineItemSectionHtml.ToString().Contains("##lblServiceLineItemPartnerContactName##"))
                    sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemPartnerContactName##", Convert.ToString(BaseDAO.HtmlEncode(row.PartnerContactName)));

                if (sbServiceLineItemSectionHtml.ToString().Contains("##lblServiceLineItemPartnerContactEmail##"))
                    sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemPartnerContactEmail##", Convert.ToString(BaseDAO.HtmlEncode(row.PartnerContactEmail)));

                if (sbServiceLineItemSectionHtml.ToString().Contains("##lblServiceLineItemClientPartnerCode##"))
                    sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemClientPartnerCode##", Convert.ToString(BaseDAO.HtmlEncode(row.ClientPartnerCode)));

                if (sbServiceLineItemSectionHtml.ToString().Contains("##lblAllowFlexiblePrice##"))
                    sbServiceLineItemSectionHtml.Replace("##lblAllowFlexiblePrice##", (row.FlexiblePrice ? "Yes" : "No"));

                if (sbServiceLineItemSectionHtml.ToString().Contains("##lblItemCurrencyCode##"))
                    sbServiceLineItemSectionHtml.Replace("##lblItemCurrencyCode##", Convert.ToString(row.CurrencyCode));

                if (sbServiceLineItemSectionHtml.ToString().Contains("##lblMaterialLineItemBillable##"))
                    sbServiceLineItemSectionHtml.Replace("##lblMaterialLineItemBillable##", Convert.ToString(row.Billable).ToLower() == "billable" ? "Yes" : "No");
                if (sbServiceLineItemSectionHtml.ToString().Contains("##lblMaterialLineItemCapitalized##"))
                    sbServiceLineItemSectionHtml.Replace("##lblMaterialLineItemCapitalized##", BaseDAO.HtmlEncode(Convert.ToString(row.Billable)));
                if (sbServiceLineItemSectionHtml.ToString().Contains("##lblServiceLineItemManufacturerPartNumber##"))
                    sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemManufacturerPartNumber##", Convert.ToString(row.ManufacturerPartNumber));
                if (sbServiceLineItemSectionHtml.ToString().Contains("##lblServiceLineItemContractNo##"))
                    sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemContractNo##", Convert.ToString(row.ContractNo));

                if (sbServiceLineItemSectionHtml.ToString().Contains("##lblServiceLineItemRequisitionNumber##"))
                    sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemRequisitionNumber##", (row.RequisitionNumber == null) ? string.Empty : BaseDAO.HtmlEncode(Convert.ToString(row.RequisitionNumber)));

                if (docType == P2PDocumentType.Requisition)
                {
                    FillOrderStatusDetails(row, sbServiceLineItemSectionHtml, dsDocument.P2PDocument[0].DocumentStatusInfo);
                }
                if (docType == P2PDocumentType.Receipt)
                {
                    sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemReceivedQuantity##", ConvertToStringForQuantity(row.ReceivedQuantity));
                    if (Convert.ToString(row.ItemExtendedType).Trim() == Enum.GetName(typeof(ItemExtendedType), ItemExtendedType.Fixed))
                        sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemAcceptedQuantity##", "-");
                    else
                        sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemAcceptedQuantity##", ConvertToStringForQuantity(row.AcceptedQuantity));
                    sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemStatus##", Convert.ToString((DocumentStatus)row.ItemStatus));
                    sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemFulfilledDate##", string.IsNullOrWhiteSpace(row.ReceivedDate) ? string.Empty : GetDate(row.ReceivedDate, DateFormatType.DefaultDate));
                    sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemReceivedAmount##", ConvertToStringForQuantity(row.ReceivedAmount));
                }

                else if (docType == P2PDocumentType.ServiceConfirmation)
                {
                    if (Convert.ToString(row.ItemExtendedType).Trim().ToUpper() == Enum.GetName(typeof(ItemExtendedType), ItemExtendedType.Fixed).Trim().ToUpper() || Convert.ToString(row.ItemExtendedType).Trim().ToUpper() == Enum.GetName(typeof(ItemExtendedType), ItemExtendedType.Variable).Trim().ToUpper())
                    {
                        sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemCompletionTillDate##", ConvertToStringForQuantity(Convert.ToDecimal(row.OrderedQuantity * row.OrderedUnitPrice * Convert.ToDecimal(row.OverallCompletion)) / 100) + " " + Convert.ToString(dsDocument.P2PDocument[0].CurrencyCode));
                        sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemOverallCompletion##", "-");
                        sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemRemainingWork##", "-");
                    }
                    else
                    {
                        sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemCompletionTillDate##", ConvertToStringForQuantity(Convert.ToDecimal(row.OverallCompletion)) + "%");
                        sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemOverallCompletion##", ConvertToStringForQuantity(Convert.ToDecimal(row.OverallCompletion)) + "%");
                        sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemRemainingWork##", ConvertToStringForQuantity(Convert.ToDecimal(row.RemainingWork)) + "%");
                    }
                    sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemStatus##", Convert.ToString((DocumentStatus)row.ItemStatus));
                    //sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemRequesterName##", row.RequesterName);
                }
                if (sbServiceLineItemSectionHtml.ToString().Contains("##ServiceLineItemServiceDetailSection##"))
                {
                    FillServiceLineItemServiceDetails(row, sbServiceLineItemSectionHtml);
                    if (sbServiceLineItemSectionHtml.ToString().Contains("^^ServiceLineItemRequesterDetail^^"))
                        FillServiceLineItemRequesterDetails(
                            dsDocument.RequesterDetails.Where(t => t.DocumentItemId == row.DocumentItemId).ToList(),
                            sbServiceLineItemSectionHtml);
                }
                sbServiceLineItemSectionHtml.Replace("^^ServiceLineItem^^", string.Empty);
                sbServiceLineItemSectionHtml.Replace("^^/ServiceLineItem^^", string.Empty);
                if (sbServiceLineItemSectionHtml.ToString().Contains("^^ServiceLineItemGroup^^"))
                    FillServiceLineItemComments(
                        dsDocument.LineItemComment.Where(t => t.P2PLineItemId == row.P2PLineItemId).ToList(),
                        sbServiceLineItemSectionHtml);

                long itm = docType == P2PDocumentType.InvoiceReconciliation ? row.InvoiceItemId : row.DocumentItemId;
                FillCustomFields(itm, Convert.ToInt64(row.FormID), sbServiceLineItemSectionHtml, "Item", lstquestion, true);
                FillAccountingFields(dsDocument.AccountingDetails.Where(x => x.DocumentItemID == row.DocumentItemId).ToList(), sbServiceLineItemSectionHtml, row.ItemTypeID, docType);
                FillRequisitionItemAdditionalFields(objExport, row.DocumentItemId, sbServiceLineItemSectionHtml);
                //if (sbServiceLineItemSectionHtml.ToString().Contains("##ServiceLineItemExceptionSection##") && docType == P2PDocumentType.InvoiceReconciliation)
                //{
                //    ExportIR objExportIR = (ExportIR)objExport;
                //    FillServiceLineItemExceptions(objExportIR.IRDataSet.IRLineItemException.Where(t => t.IRItemId == row.DocumentItemId).ToList(),
                //        sbServiceLineItemSectionHtml);
                //}
                if (row.SourceType == (int)ItemSourceType.PurchaseRequest && (docType == P2PDocumentType.Requisition || docType == P2PDocumentType.Order))
                {
                    // start
                    var questionnaires = dsDocument.Questionnaire.Where(t => t.DocumentItemId == row.DocumentItemId).ToList();
                    if (questionnaires.Count > 0)
                    {
                        var sbItemQuestionnairesHtml = FillQuestionnaireResponceDetails(sbServiceLineItemSectionHtml, questionnaires);
                        string strItemQuestionnaireSectionHtml = GetSectionHtml(sbServiceLineItemSectionHtml.ToString(), "^^LineItemQuestionnaire^^",
                                                                       "^^/LineItemQuestionnaire^^");
                        if (!string.IsNullOrWhiteSpace(strItemQuestionnaireSectionHtml))
                            sbServiceLineItemSectionHtml.Replace(strItemQuestionnaireSectionHtml, sbItemQuestionnairesHtml.ToString());
                        sbServiceLineItemSectionHtml.Replace("##QuestionnaireSection##", string.Empty);
                        sbServiceLineItemSectionHtml.Replace("##/QuestionnaireSection##", string.Empty);
                    }
                    //end
                }
                else
                {
                    string strItemQuestionnaireSectionHtml = GetSectionHtml(sbServiceLineItemSectionHtml.ToString(), "##QuestionnaireSection##",
                                                                   "##/QuestionnaireSection##");
                    if (!string.IsNullOrWhiteSpace(strItemQuestionnaireSectionHtml))
                        sbServiceLineItemSectionHtml.Replace(strItemQuestionnaireSectionHtml, string.Empty);
                }
                //if (sbServiceLineItemSectionHtml.ToString().Contains("##lblServiceLineItemOrderedUnitPrice##") && docType == P2PDocumentType.Invoice)
                //{
                //    sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemOrderedUnitPrice##", ConvertToString(row.OrderedUnitPrice));
                //    if (usertype == 0 || (usertype == 1 && dsDocument.P2PDocument[0].DocumentStatusInfo != 1 && dsDocument.P2PDocument[0].DocumentStatusInfo != 23 && dsDocument.P2PDocument[0].DocumentStatusInfo != 121 && dsDocument.P2PDocument[0].DocumentStatusInfo != 122))
                //    {
                //        sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemInvoicedAmount##", ConvertToString(row.InvoicedAmount));
                //        sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemRemainingAmount##", ConvertToString(row.RemainingAmount));
                //    }
                //    else
                //    {
                //        sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemInvoicedAmount##", "NA");
                //        sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemRemainingAmount##", "NA");
                //    }
                //    if (sbServiceLineItemSectionHtml.ToString().Contains("##ServiceLineItemExceptionSection##"))
                //    {
                //        ExportInvoice objExportInvoice = (ExportInvoice)objExport;
                //        FillInvoiceServiceLineItemExceptions(objExportInvoice.InvoiceDataSet.InvoiceLineItemException.Where(t => t.InvoiceItemId == row.DocumentItemId).ToList(),
                //            sbServiceLineItemSectionHtml, usertype);
                //    }
                //    else if (sbServiceLineItemSectionHtml.ToString().Contains("##ServiceLineItemErrorSection##"))
                //    {
                //        ExportInvoice objExportInvoice = (ExportInvoice)objExport;
                //        List<P2PDocumentDataSet.P2PItemRow> objExportInvoiceItem = objExportInvoice.DocumentDataSet.P2PItem.Where(t => t.DocumentItemId == row.DocumentItemId).ToList();
                //        FillInvoiceServiceLineItemErrors(objExportInvoiceItem, sbServiceLineItemSectionHtml, usertype);
                //    }
                //}
                if (docType == P2PDocumentType.Order)
                {
                    sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemReceivingStatus##", Convert.ToBoolean(row.IsCloseForReceiving) ? "Closed" : "Open");
                    sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemInvoicingStatus##", Convert.ToBoolean(row.IsCloseForInvoicing) ? "Closed" : "Open");
                }
                if (stgShowStanProcDetails && dsDocument.P2PDocument[0].DocumentSourceTypeInfo != 6)
                {
                    FillStandardProcedureDetails(Convert.ToInt64(dsDocument.P2PDocument[0].DocumentCode), sbServiceLineItemSectionHtml, Convert.ToInt64(row.DocumentItemId), 2, dsDocument, ItemType.Service);
                }
                else
                {
                    var showStanProcDetailsText = GetSectionHtml(sbServiceLineItemSectionHtml.ToString(), "##ServiceLineSPDetailSection##", "##/ServiceLineSPDetailSection##");
                    if (showStanProcDetailsText != null && showStanProcDetailsText != string.Empty)
                        sbServiceLineItemSectionHtml.Replace(showStanProcDetailsText, string.Empty);
                }

                if (docType == P2PDocumentType.Requisition)
                {
                    FillStockReservationDetails(sbServiceLineItemSectionHtml, objSettings, "Service", row);

                    // Add Source Type description as Item Source
                    sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemItemSource##", row.SourceTypeDescription);
                }

                if (docType == P2PDocumentType.Receipt || docType == P2PDocumentType.ReturnNote)
                {
                    if (sbServiceLineItemSectionHtml.ToString().Contains("##MultipleOrderDetailSection##"))
                        FillReceiptItemOrderDetails(row, sbServiceLineItemSectionHtml);
                }

                var advancePaymentSection = GetSectionHtml(sbServiceLineItemSectionHtml.ToString(), "##AdvancePaymentSection##",
                                                               "##/AdvancePaymentSection##");
                if (!string.IsNullOrWhiteSpace(advancePaymentSection))
                {
                    FillAdvancePaymentDetails(row, sbServiceLineItemSectionHtml, advancePaymentSection, Convert.ToInt64(dsDocument.P2PDocument[0].DocumentCode));
                }

                long num = stgAllowLineNumber ? row.ItemLineNumber : sNO;
                tempServiceList.Add(new KeyValuePair<long, StringBuilder>(num, sbServiceLineItemSectionHtml));
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occurred in BindServiceItem method in ExportManager for P2PLineItemId = " + row.P2PLineItemId, ex);
                throw;
            }
        }

        private void BindContingentWorkerItem(P2PDocumentDataSet.P2PItemRow row, P2PDocumentType docType, Export objExport, int usertype, string strContingentWorkerLineItemSectionHtml, List<Question> lstquestion = null, bool enableBuyerInvoiceVisibilityToSupplier = false, bool IsVABUser = false)
        {
            try
            {
                RequisitionCommonManager commonManager = new RequisitionCommonManager (this.JWTToken){ UserContext = UserContext, GepConfiguration = GepConfiguration };
                SettingDetails objSettings = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.None, 1, 107);
                var AllowOtherCharges = Convert.ToBoolean(commonManager.GetSettingsValueByKey(objSettings, "AllowOtherCharges"), CultureInfo.InvariantCulture);
                P2PDocumentDataSet dsDocument = objExport.DocumentDataSet;
                int sNO = srNoServicenew++;

                var sbContingentWorkerLineItemSectionHtml = new StringBuilder(strContingentWorkerLineItemSectionHtml);
                if (stgAllowLineNumber)
                {
                    sbContingentWorkerLineItemSectionHtml.Replace("##lblContingentWorkerLineItemSerialNo##", Convert.ToString(row.ItemLineNumber));
                }
                else
                {
                    sbContingentWorkerLineItemSectionHtml.Replace("##lblContingentWorkerLineItemSerialNo##", sNO.ToString(CultureInfo.InvariantCulture));
                }
                sbContingentWorkerLineItemSectionHtml.Replace("##lblContingentWorkerLineItemDescription##", BaseDAO.HtmlEncode(Convert.ToString(row.Description)));
                //sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemNumber##", Convert.ToString(row.ItemNumber));
                if (row.Total >= 0)
                {
                    sbContingentWorkerLineItemSectionHtml.Replace("##lblContingentWorkerLineItemTotal##", ConvertToString(Math.Round(row.Total, maxPrecTot, MidpointRounding.AwayFromZero)) + " " + Convert.ToString(dsDocument.P2PDocument[0].CurrencyCode));
                    sbContingentWorkerLineItemSectionHtml.Replace("##lblContingentWorkerItemTotal##", ConvertToString(Math.Round(row.Total, maxPrecTot, MidpointRounding.AwayFromZero)));
                }
                else
                {
                    sbContingentWorkerLineItemSectionHtml.Replace("##lblContingentWorkerLineItemTotal##", ConvertToNegativeString(Math.Round(row.Total, maxPrecTot, MidpointRounding.AwayFromZero)) + " " + Convert.ToString(dsDocument.P2PDocument[0].CurrencyCode));
                    sbContingentWorkerLineItemSectionHtml.Replace("##lblContingentWorkerItemTotal##", ConvertToNegativeString(Math.Round(row.Total, maxPrecTot, MidpointRounding.AwayFromZero)));
                }
                if (row.UnitPrice >= 0)
                    sbContingentWorkerLineItemSectionHtml.Replace("##lblContingentWorkerLineItemUnitPrice##", ConvertToString(Math.Round(row.UnitPrice, maxPrec)));
                else
                    sbContingentWorkerLineItemSectionHtml.Replace("##lblContingentWorkerLineItemUnitPrice##", ConvertToNegativeString(Math.Round(row.UnitPrice, maxPrec)));

                if (Convert.ToString(row.ItemExtendedType).Trim().ToUpper() == Enum.GetName(typeof(ItemExtendedType), ItemExtendedType.Progress).Trim().ToUpper() || Convert.ToString(row.ItemExtendedType).Trim().ToUpper() == Enum.GetName(typeof(ItemExtendedType), ItemExtendedType.MileStone).Trim().ToUpper())
                {
                    sbContingentWorkerLineItemSectionHtml.Replace("##lblContingentWorkerLineItemUOM##", "-");
                    sbContingentWorkerLineItemSectionHtml.Replace("##lblContingentWorkerLineItemNumber##", "-");
                    sbContingentWorkerLineItemSectionHtml.Replace("##lblContingentWorkerLinePartnerItemNumber##", "-");
                }
                else
                {
                    sbContingentWorkerLineItemSectionHtml.Replace("##lblContingentWorkerLineItemUOM##", BaseDAO.HtmlEncode(Convert.ToString(row.UOMDesc)));
                    sbContingentWorkerLineItemSectionHtml.Replace("##lblContingentWorkerLineItemNumber##", Convert.ToString(row.ItemNumber));
                    sbContingentWorkerLineItemSectionHtml.Replace("##lblContingentWorkerLinePartnerItemNumber##", BaseDAO.HtmlEncode(Convert.ToString(row.PartnerItemNumber)));
                }
                if (Convert.ToString(row.ItemExtendedType).Trim().ToUpper() == Enum.GetName(typeof(ItemExtendedType), ItemExtendedType.Fixed).Trim().ToUpper() || Convert.ToString(row.ItemExtendedType).Trim().ToUpper() == Enum.GetName(typeof(ItemExtendedType), ItemExtendedType.Progress).Trim().ToUpper() || Convert.ToString(row.ItemExtendedType).Trim().ToUpper() == Enum.GetName(typeof(ItemExtendedType), ItemExtendedType.MileStone).Trim().ToUpper())
                {
                    sbContingentWorkerLineItemSectionHtml.Replace("##lblContingentWorkerLineItemAcceptedQuantity##", @"N/A");
                    sbContingentWorkerLineItemSectionHtml.Replace("##lblContingentWorkerLineItemEfforts##", "-");
                }
                else
                {
                    if (row.Efforts >= 0)
                    {
                        sbContingentWorkerLineItemSectionHtml.Replace("##lblContingentWorkerLineItemAcceptedQuantity##", ConvertToStringForQuantity(row.ReceivedQuantity));
                        sbContingentWorkerLineItemSectionHtml.Replace("##lblContingentWorkerLineItemEfforts##", ConvertToStringForQuantity(row.Efforts));
                    }
                    else
                    {
                        sbContingentWorkerLineItemSectionHtml.Replace("##lblContingentWorkerLineItemAcceptedQuantity##", ConvertToNegativeString(row.ReceivedQuantity));
                        sbContingentWorkerLineItemSectionHtml.Replace("##lblContingentWorkerLineItemEfforts##", ConvertToNegativeString(row.Efforts));
                    }
                }
                if (Convert.ToString(row.ItemExtendedType).Trim().ToUpper() == Enum.GetName(typeof(ItemExtendedType), ItemExtendedType.Variable).Trim().ToUpper())
                {
                    sbContingentWorkerLineItemSectionHtml.Replace("##lblContingentWorkerLineOrderedQuantity##", ConvertToStringForQuantity(row.OrderedQuantity));
                }
                else
                {
                    sbContingentWorkerLineItemSectionHtml.Replace("##lblContingentWorkerLineOrderedQuantity##", "0.00");
                }
                if (sbContingentWorkerLineItemSectionHtml.ToString().Contains("##lblContingentWorkerLineItemCategory##"))
                    sbContingentWorkerLineItemSectionHtml.Replace("##lblContingentWorkerLineItemCategory##", BaseDAO.HtmlEncode(Convert.ToString(row.CategoryName)));

                if (sbContingentWorkerLineItemSectionHtml.ToString().Contains("##lblContingentWorkerLineItemDispatchMode##") && Convert.ToString(row.EnableDispatchMode).ToUpper() == "TRUE")
                {
                    sbContingentWorkerLineItemSectionHtml.Replace("##lblContingentWorkerLineItemDispatchMode##", BaseDAO.HtmlEncode(Convert.ToString(row.DispatchMode)));
                    sbContingentWorkerLineItemSectionHtml.Replace("##ContingentWorkerLineLevelCustomField##", string.Empty);
                    sbContingentWorkerLineItemSectionHtml.Replace("##/ContingentWorkerLineLevelCustomField##", string.Empty);
                }
                else
                {
                    string strContingentWorkerLineLevelCustomFieldHtml = string.Empty;
                    strContingentWorkerLineLevelCustomFieldHtml = GetSectionHtml(sbContingentWorkerLineItemSectionHtml.ToString(), "##ContingentWorkerLineLevelCustomField##",
                                                                   "##/ContingentWorkerLineLevelCustomField##");
                    if (strContingentWorkerLineLevelCustomFieldHtml != null && strContingentWorkerLineLevelCustomFieldHtml != string.Empty)
                        sbContingentWorkerLineItemSectionHtml.Replace(strContingentWorkerLineLevelCustomFieldHtml, string.Empty);
                }

                if (sbContingentWorkerLineItemSectionHtml.ToString().Contains("##lblContingentWorkerLineItemMatchType##"))
                    sbContingentWorkerLineItemSectionHtml.Replace("##lblContingentWorkerLineItemMatchType##", GetMatchType(row.MatchType));
                if (sbContingentWorkerLineItemSectionHtml.ToString().Contains("##lblContingentWorkerLineItemStartDate##"))
                    sbContingentWorkerLineItemSectionHtml.Replace("##lblContingentWorkerLineItemStartDate##", string.IsNullOrWhiteSpace(row.StartDate) ? string.Empty : GetDate(row.StartDate, DateFormatType.DefaultDate));
                if (sbContingentWorkerLineItemSectionHtml.ToString().Contains("##lblContingentWorkerLineItemEndDate##"))
                    sbContingentWorkerLineItemSectionHtml.Replace("##lblContingentWorkerLineItemEndDate##", string.IsNullOrWhiteSpace(row.EndDate) ? string.Empty : GetDate(row.EndDate, DateFormatType.DefaultDate));
                //get Service Request related columns
                if (sbContingentWorkerLineItemSectionHtml.ToString().Contains("##lblContingentWorkerLineItemPriceType##"))
                    sbContingentWorkerLineItemSectionHtml.Replace("##lblContingentWorkerLineItemPriceType##", Convert.ToString(row.PriceType));
                if (sbContingentWorkerLineItemSectionHtml.ToString().Contains("##lblContingentWorkerLineItemContingentWorkerName##"))
                    sbContingentWorkerLineItemSectionHtml.Replace("##lblContingentWorkerLineItemContingentWorkerName##", Convert.ToString(row.ContingentWorkerName));
                if (sbContingentWorkerLineItemSectionHtml.ToString().Contains("##lblContingentWorkerLineItemJobTitle##"))
                    sbContingentWorkerLineItemSectionHtml.Replace("##lblContingentWorkerLineItemJobTitle##", Convert.ToString(row.JobTitle));
                if (sbContingentWorkerLineItemSectionHtml.ToString().Contains("##lblContingentWorkerLineItemMargin##"))
                    sbContingentWorkerLineItemSectionHtml.Replace("##lblContingentWorkerLineItemMargin##", ConvertToString(Math.Round(row.Margin, maxPrec)));
                if (sbContingentWorkerLineItemSectionHtml.ToString().Contains("##lblContingentWorkerLineItemPayRate##"))
                    sbContingentWorkerLineItemSectionHtml.Replace("##lblContingentWorkerLineItemPayRate##", ConvertToString(Math.Round(row.BaseRate, maxPrec)));
                if (sbContingentWorkerLineItemSectionHtml.ToString().Contains("##lblContingentWorkerLineItemReportingManager##"))
                    sbContingentWorkerLineItemSectionHtml.Replace("##lblContingentWorkerLineItemReportingManager##", Convert.ToString(row.ReportingManager));

                if (sbContingentWorkerLineItemSectionHtml.ToString().Contains("##lblContingentWorkerLineItemTax##"))
                    sbContingentWorkerLineItemSectionHtml.Replace("##lblContingentWorkerLineItemTax##", ConvertToString(Math.Round(row.Tax, maxPrecTax)));
                if (sbContingentWorkerLineItemSectionHtml.ToString().Contains("##lblContingentWorkerLineItemServiceType##"))
                    sbContingentWorkerLineItemSectionHtml.Replace("##lblContingentWorkerLineItemServiceType##", BaseDAO.HtmlEncode(Convert.ToString(row.ItemExtendedType)));
                if (sbContingentWorkerLineItemSectionHtml.ToString().Contains("##lblContingentWorkerLineItemTaxExempt##"))
                    sbContingentWorkerLineItemSectionHtml.Replace("##lblContingentWorkerLineItemTaxExempt##", Convert.ToString(row.TaxExempt));
                if (sbContingentWorkerLineItemSectionHtml.ToString().Contains("##ItemExtendedType##"))
                    sbContingentWorkerLineItemSectionHtml.Replace("##ItemExtendedType##", row.ItemExtendedType);
                if (sbContingentWorkerLineItemSectionHtml.ToString().Contains("##lblContingentWorkerLineItemOrderingLocation##"))
                    sbContingentWorkerLineItemSectionHtml.Replace("##lblContingentWorkerLineItemOrderingLocation##", row.OrderingLocation + ", " + row.OrderingLocationAddress);
                if ((docType == P2PDocumentType.Requisition || (docType == P2PDocumentType.Order && (!IsVABUser))) && usertype == 0 && sbContingentWorkerLineItemSectionHtml.ToString().Contains("##lblContingentWorkerLineItemOverallItemLimit##") && row.OverallItemLimit > 0)
                    sbContingentWorkerLineItemSectionHtml.Replace("##lblContingentWorkerLineItemOverallItemLimit##", ConvertToString(Math.Round(row.OverallItemLimit, maxPrec)));
                else
                {
                    sbContingentWorkerLineItemSectionHtml.Replace("##lblContingentWorkerLineItemOverallItemLimit##", string.Empty);
                    var hiddenServiceOverallItemLimit = GetSectionHtml(sbContingentWorkerLineItemSectionHtml.ToString(), "##ContingentWorkerOverallItemLimitSection##", "##/ContingentWorkerOverallItemLimitSection##");
                    if (!string.IsNullOrWhiteSpace(hiddenServiceOverallItemLimit))
                        sbContingentWorkerLineItemSectionHtml.Replace(hiddenServiceOverallItemLimit, string.Empty);
                    var hiddenServiceOverallItemLimitProperty = GetSectionHtml(sbContingentWorkerLineItemSectionHtml.ToString(), "##ContingentWorkerOverallItemLimitPropertySection##", "##/ContingentWorkerOverallItemLimitPropertySection##");
                    if (!string.IsNullOrWhiteSpace(hiddenServiceOverallItemLimitProperty))
                        sbContingentWorkerLineItemSectionHtml.Replace(hiddenServiceOverallItemLimitProperty, string.Empty);
                }
                sbContingentWorkerLineItemSectionHtml.Replace("##ContingentWorkerOverallItemLimitSection##", string.Empty);
                sbContingentWorkerLineItemSectionHtml.Replace("##/ContingentWorkerOverallItemLimitSection##", string.Empty);
                sbContingentWorkerLineItemSectionHtml.Replace("##ContingentWorkerOverallItemLimitPropertySection##", string.Empty);
                sbContingentWorkerLineItemSectionHtml.Replace("##/ContingentWorkerOverallItemLimitPropertySection##", string.Empty);
                if (sbContingentWorkerLineItemSectionHtml.ToString().Contains("##lblContingentWorkerItemCategory##"))
                    sbContingentWorkerLineItemSectionHtml.Replace("##lblContingentWorkerItemCategory##", BaseDAO.HtmlEncode(Convert.ToString(row.CategoryName)));
                if (row.HasFlexibleCharges != 1)
                {
                    if (sbContingentWorkerLineItemSectionHtml.ToString().Contains("##lblContingentWorkerLineItemAdditional##"))
                        if (AllowOtherCharges)
                            sbContingentWorkerLineItemSectionHtml.Replace("##lblContingentWorkerLineItemAdditional##", ConvertToString(row.AdditionalCharges));
                        else
                        {
                            sbContingentWorkerLineItemSectionHtml.Replace("##lblContingentWorkerLineItemAdditional##", "");
                        }
                    var hiddenFlexible = GetSectionHtml(sbContingentWorkerLineItemSectionHtml.ToString(), "##ContingentWorkerLineFlexibleChargeSection##", "##/ContingentWorkerLineFlexibleChargeSection##");
                    if (!string.IsNullOrWhiteSpace(hiddenFlexible))
                        sbContingentWorkerLineItemSectionHtml.Replace(hiddenFlexible, string.Empty);
                    var hiddenFlexibleProperty = GetSectionHtml(sbContingentWorkerLineItemSectionHtml.ToString(), "##ContingentWorkerLineFlexibleChargePropertySection##", "##/ContingentWorkerLineFlexibleChargePropertySection##");
                    if (!string.IsNullOrWhiteSpace(hiddenFlexibleProperty))
                        sbContingentWorkerLineItemSectionHtml.Replace(hiddenFlexibleProperty, string.Empty);
                }
                else
                {
                    if (sbContingentWorkerLineItemSectionHtml.ToString().Contains("##lblContingentWorkerLineItemTotalCharges##"))
                    {
                        if (row.AdditionalCharges >= 0)
                        {
                            sbContingentWorkerLineItemSectionHtml.Replace("##lblContingentWorkerLineItemTotalCharges##", ConvertToString(Math.Round(row.AdditionalCharges, maxPrecTax)));
                        }
                        else
                        {
                            sbContingentWorkerLineItemSectionHtml.Replace("##lblContingentWorkerLineItemTotalCharges##", ConvertToNegativeString(Math.Round(row.AdditionalCharges, maxPrecTax)));
                        }
                    }
                    var hiddenDefault = GetSectionHtml(sbContingentWorkerLineItemSectionHtml.ToString(), "##ContingentWorkerLineDefaultSection##", "##/ContingentWorkerLineDefaultSection##");
                    if (!string.IsNullOrWhiteSpace(hiddenDefault))
                        sbContingentWorkerLineItemSectionHtml.Replace(hiddenDefault, string.Empty);
                    var hiddenDefaultProperty = GetSectionHtml(sbContingentWorkerLineItemSectionHtml.ToString(), "##ContingentWorkerLineDefaultPropertySection##", "##/ContingentWorkerLineDefaultPropertySection##");
                    if (!string.IsNullOrWhiteSpace(hiddenDefaultProperty))
                        sbContingentWorkerLineItemSectionHtml.Replace(hiddenDefaultProperty, string.Empty);
                }
                sbContingentWorkerLineItemSectionHtml.Replace("##ContingentWorkerLineDefaultSection##", string.Empty);
                sbContingentWorkerLineItemSectionHtml.Replace("##/ContingentWorkerLineDefaultSection##", string.Empty);
                sbContingentWorkerLineItemSectionHtml.Replace("##ContingentWorkerLineFlexibleChargeSection##", string.Empty);
                sbContingentWorkerLineItemSectionHtml.Replace("##/ContingentWorkerLineFlexibleChargeSection##", string.Empty);
                sbContingentWorkerLineItemSectionHtml.Replace("##ContingentWorkerLineDefaultPropertySection##", string.Empty);
                sbContingentWorkerLineItemSectionHtml.Replace("##/ContingentWorkerLineDefaultPropertySection##", string.Empty);
                sbContingentWorkerLineItemSectionHtml.Replace("##ContingentWorkerLineFlexibleChargePropertySection##", string.Empty);
                sbContingentWorkerLineItemSectionHtml.Replace("##/ContingentWorkerLineFlexibleChargePropertySection##", string.Empty);
                if (sbContingentWorkerLineItemSectionHtml.ToString().Contains("##lblContingentWorkerLineItemPartnerName##"))
                    sbContingentWorkerLineItemSectionHtml.Replace("##lblContingentWorkerLineItemPartnerName##", BaseDAO.HtmlEncode(Convert.ToString(row.PartnerName)));

                if (sbContingentWorkerLineItemSectionHtml.ToString().Contains("##lblContingentWorkerLineItemPartnerContactName##"))
                    sbContingentWorkerLineItemSectionHtml.Replace("##lblContingentWorkerLineItemPartnerContactName##", Convert.ToString(BaseDAO.HtmlEncode(row.PartnerContactName)));

                if (sbContingentWorkerLineItemSectionHtml.ToString().Contains("##lblContingentWorkerLineItemPartnerContactEmail##"))
                    sbContingentWorkerLineItemSectionHtml.Replace("##lblContingentWorkerLineItemPartnerContactEmail##", Convert.ToString(BaseDAO.HtmlEncode(row.PartnerContactEmail)));

                if (sbContingentWorkerLineItemSectionHtml.ToString().Contains("##lblContingentWorkerLineItemClientPartnerCode##"))
                    sbContingentWorkerLineItemSectionHtml.Replace("##lblContingentWorkerLineItemClientPartnerCode##", Convert.ToString(BaseDAO.HtmlEncode(row.ClientPartnerCode)));

                if (sbContingentWorkerLineItemSectionHtml.ToString().Contains("##lblAllowFlexiblePrice##"))
                    sbContingentWorkerLineItemSectionHtml.Replace("##lblAllowFlexiblePrice##", (row.FlexiblePrice ? "Yes" : "No"));

                if (sbContingentWorkerLineItemSectionHtml.ToString().Contains("##lblItemCurrencyCode##"))
                    sbContingentWorkerLineItemSectionHtml.Replace("##lblItemCurrencyCode##", Convert.ToString(row.CurrencyCode));

                if (sbContingentWorkerLineItemSectionHtml.ToString().Contains("##lblMaterialLineItemBillable##"))
                    sbContingentWorkerLineItemSectionHtml.Replace("##lblMaterialLineItemBillable##", Convert.ToString(row.Billable).ToLower() == "billable" ? "Yes" : "No");
                if (sbContingentWorkerLineItemSectionHtml.ToString().Contains("##lblMaterialLineItemCapitalized##"))
                    sbContingentWorkerLineItemSectionHtml.Replace("##lblMaterialLineItemCapitalized##", BaseDAO.HtmlEncode(Convert.ToString(row.Billable)));
                if (sbContingentWorkerLineItemSectionHtml.ToString().Contains("##lblContingentWorkerLineItemManufacturerPartNumber##"))
                    sbContingentWorkerLineItemSectionHtml.Replace("##lblContingentWorkerLineItemManufacturerPartNumber##", Convert.ToString(row.ManufacturerPartNumber));
                if (sbContingentWorkerLineItemSectionHtml.ToString().Contains("##lblContingentWorkerLineItemContractNo##"))
                    sbContingentWorkerLineItemSectionHtml.Replace("##lblContingentWorkerLineItemContractNo##", Convert.ToString(row.ContractNo));

                if (sbContingentWorkerLineItemSectionHtml.ToString().Contains("##lblContingentWorkerLineItemRequisitionNumber##"))
                    sbContingentWorkerLineItemSectionHtml.Replace("##lblContingentWorkerLineItemRequisitionNumber##", (row.RequisitionNumber == null) ? string.Empty : BaseDAO.HtmlEncode(Convert.ToString(row.RequisitionNumber)));

                if (sbContingentWorkerLineItemSectionHtml.ToString().Contains("##ServiceLineItemServiceDetailSection##"))
                {
                    FillServiceLineItemServiceDetails(row, sbContingentWorkerLineItemSectionHtml);
                    if (sbContingentWorkerLineItemSectionHtml.ToString().Contains("^^ServiceLineItemRequesterDetail^^"))
                        FillServiceLineItemRequesterDetails(
                            dsDocument.RequesterDetails.Where(t => t.DocumentItemId == row.DocumentItemId).ToList(),
                            sbContingentWorkerLineItemSectionHtml);
                }
                sbContingentWorkerLineItemSectionHtml.Replace("^^ContingentWorkerLineItem^^", string.Empty);
                sbContingentWorkerLineItemSectionHtml.Replace("^^/ContingentWorkerLineItem^^", string.Empty);
                if (sbContingentWorkerLineItemSectionHtml.ToString().Contains("^^ServiceLineItemGroup^^"))
                    FillServiceLineItemComments(
                        dsDocument.LineItemComment.Where(t => t.P2PLineItemId == row.P2PLineItemId).ToList(),
                        sbContingentWorkerLineItemSectionHtml);

                long itm = docType == P2PDocumentType.InvoiceReconciliation ? row.InvoiceItemId : row.DocumentItemId;
                FillCustomFields(itm, Convert.ToInt64(row.FormID), sbContingentWorkerLineItemSectionHtml, "Item", lstquestion, true);
                FillAccountingFields(dsDocument.AccountingDetails.Where(x => x.DocumentItemID == row.DocumentItemId).ToList(), sbContingentWorkerLineItemSectionHtml, row.ItemTypeID, docType);
                FillRequisitionItemAdditionalFields(objExport, row.DocumentItemId, sbContingentWorkerLineItemSectionHtml);
                if (row.SourceType == (int)ItemSourceType.PurchaseRequest && (docType == P2PDocumentType.Requisition || docType == P2PDocumentType.Order))
                {
                    // start
                    var questionnaires = dsDocument.Questionnaire.Where(t => t.DocumentItemId == row.DocumentItemId).ToList();
                    if (questionnaires.Count > 0)
                    {
                        var sbItemQuestionnairesHtml = FillQuestionnaireResponceDetails(sbContingentWorkerLineItemSectionHtml, questionnaires);
                        string strItemQuestionnaireSectionHtml = GetSectionHtml(sbContingentWorkerLineItemSectionHtml.ToString(), "^^LineItemQuestionnaire^^",
                                                                       "^^/LineItemQuestionnaire^^");
                        if (!string.IsNullOrWhiteSpace(strItemQuestionnaireSectionHtml))
                            sbContingentWorkerLineItemSectionHtml.Replace(strItemQuestionnaireSectionHtml, sbItemQuestionnairesHtml.ToString());
                        sbContingentWorkerLineItemSectionHtml.Replace("##QuestionnaireSection##", string.Empty);
                        sbContingentWorkerLineItemSectionHtml.Replace("##/QuestionnaireSection##", string.Empty);
                    }
                    //end
                }
                else
                {
                    string strItemQuestionnaireSectionHtml = GetSectionHtml(sbContingentWorkerLineItemSectionHtml.ToString(), "##QuestionnaireSection##",
                                                                   "##/QuestionnaireSection##");
                    if (!string.IsNullOrWhiteSpace(strItemQuestionnaireSectionHtml))
                        sbContingentWorkerLineItemSectionHtml.Replace(strItemQuestionnaireSectionHtml, string.Empty);
                }
                if (docType == P2PDocumentType.Order)
                {
                    sbContingentWorkerLineItemSectionHtml.Replace("##lblContingentWorkerLineItemReceivingStatus##", Convert.ToBoolean(row.IsCloseForReceiving) ? "Closed" : "Open");
                    sbContingentWorkerLineItemSectionHtml.Replace("##lblContingentWorkerLineItemInvoicingStatus##", Convert.ToBoolean(row.IsCloseForInvoicing) ? "Closed" : "Open");
                }
                if (stgShowStanProcDetails && dsDocument.P2PDocument[0].DocumentSourceTypeInfo != 6)
                {
                    FillStandardProcedureDetails(Convert.ToInt64(dsDocument.P2PDocument[0].DocumentCode), sbContingentWorkerLineItemSectionHtml, Convert.ToInt64(row.DocumentItemId), 2, dsDocument, ItemType.Service);
                }
                else
                {
                    var showStanProcDetailsText = GetSectionHtml(sbContingentWorkerLineItemSectionHtml.ToString(), "##ContingentWorkerLineSPDetailSection##", "##/ContingentWorkerLineSPDetailSection##");
                    if (showStanProcDetailsText != null && showStanProcDetailsText != string.Empty)
                        sbContingentWorkerLineItemSectionHtml.Replace(showStanProcDetailsText, string.Empty);
                }

                if (docType == P2PDocumentType.Receipt || docType == P2PDocumentType.ReturnNote)
                {
                    if (sbContingentWorkerLineItemSectionHtml.ToString().Contains("##MultipleOrderDetailSection##"))
                        FillReceiptItemOrderDetails(row, sbContingentWorkerLineItemSectionHtml);
                }

                long num = stgAllowLineNumber ? row.ItemLineNumber : sNO;
                tempContingentWorkerList.Add(new KeyValuePair<long, StringBuilder>(num, sbContingentWorkerLineItemSectionHtml));
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occurred in BindContingentWorkerItem method in ExportManager for P2PLineItemId = " + row.P2PLineItemId, ex);
                throw;
            }
        }

        private void BindAdvanceItem(P2PDocumentDataSet.P2PItemRow row, P2PDocumentType docType, Export objExport, int usertype, string strAdvanceLineItemSectionHtml)
        {
            try
            {
                P2PDocumentDataSet dsDocument = objExport.DocumentDataSet;
                int sNO = srNoAdvancenew++;

                var sbAdvanceLineItemSectionHtml = new StringBuilder(strAdvanceLineItemSectionHtml);
                if (stgAllowLineNumber)
                {
                    sbAdvanceLineItemSectionHtml.Replace("##lblAdvanceLineItemSerialNo##", Convert.ToString(row.ItemLineNumber));
                }
                else
                {
                    sbAdvanceLineItemSectionHtml.Replace("##lblAdvanceLineItemSerialNo##", sNO.ToString(CultureInfo.InvariantCulture));
                }

                sbAdvanceLineItemSectionHtml.Replace("##lblAdvanceLineItemDescription##", BaseDAO.HtmlEncode(row.Description));
                if (sbAdvanceLineItemSectionHtml.ToString().Contains("##lblAdvanceLineItemPartnerName##"))
                    sbAdvanceLineItemSectionHtml.Replace("##lblAdvanceLineItemPartnerName##", BaseDAO.HtmlEncode(Convert.ToString(row.PartnerName)));
                sbAdvanceLineItemSectionHtml.Replace("##lblAdvanceLineItemTotal##", ConvertToNegativeString(Math.Round((row.Total), maxPrecTot, MidpointRounding.AwayFromZero)));

                if (docType == P2PDocumentType.Requisition || docType == P2PDocumentType.Order)
                {
                    if (row.Recoupment >= 0)
                    {
                        sbAdvanceLineItemSectionHtml.Replace("##lblAdvanceLineItemRecoupment##", ConvertToString(Math.Round(row.Recoupment, maxPrecTax)));
                    }
                    else
                    {
                        sbAdvanceLineItemSectionHtml.Replace("##lblAdvanceLineItemRecoupment##", ConvertToNegativeString(Math.Round((row.Recoupment), maxPrecTot, MidpointRounding.AwayFromZero)));
                    }
                    sbAdvanceLineItemSectionHtml.Replace("##lblAdvanceLineItemDueDate##", string.IsNullOrWhiteSpace(row.DateNeeded) ? string.Empty : row.DateNeeded);
                }

                //sbAdvanceLineItemSectionHtml.Replace("##lblAdvanceLineItemAdvanceAmount##", row.UOMDesc);

                //sbAdvanceLineItemSectionHtml.Replace("##MaterialLineDefaultSection##", string.Empty);
                //sbAdvanceLineItemSectionHtml.Replace("##/MaterialLineDefaultSection##", string.Empty);

                //if (sbAdvanceLineItemSectionHtml.ToString().Contains("##AdvanceLineItemExceptionSection##") && docType == P2PDocumentType.InvoiceReconciliation)
                //{
                //    ExportIR objExportIR = (ExportIR)objExport;
                //    FillAdvanceLineItemExceptions(objExportIR.IRDataSet.IRLineItemException.Where(t => t.IRItemId == row.DocumentItemId).ToList(),
                //    sbAdvanceLineItemSectionHtml);
                //}


                //else if (docType == P2PDocumentType.Invoice)
                //{
                //    sbAdvanceLineItemSectionHtml.Replace("##lblMaterialLineItemOrderedQuantity##", ConvertToStringForQuantity(row.OrderedQuantity));
                //    sbAdvanceLineItemSectionHtml.Replace("##lblMaterialLineItemOrderedUnitPrice##", ConvertToString(row.OrderedUnitPrice));
                //    if (usertype == 0 || (usertype == 1 && dsDocument.P2PDocument[0].DocumentStatusInfo != 1 && dsDocument.P2PDocument[0].DocumentStatusInfo != 23 && dsDocument.P2PDocument[0].DocumentStatusInfo != 121 && dsDocument.P2PDocument[0].DocumentStatusInfo != 122))
                //    {
                //        sbAdvanceLineItemSectionHtml.Replace("##lblMaterialLineItemInvoicedAmount##", ConvertToString(row.InvoicedAmount));
                //        sbAdvanceLineItemSectionHtml.Replace("##lblMaterialLineItemRemainingAmount##", ConvertToString(row.RemainingAmount));
                //    }
                //    else
                //    {
                //        sbAdvanceLineItemSectionHtml.Replace("##lblMaterialLineItemInvoicedAmount##", "NA");
                //        sbAdvanceLineItemSectionHtml.Replace("##lblMaterialLineItemRemainingAmount##", "NA");
                //    }
                //    if (sbAdvanceLineItemSectionHtml.ToString().Contains("##AdvanceLineItemExceptionSection##"))
                //    {
                //        ExportInvoice objExportInvoice = (ExportInvoice)objExport;
                //        FillInvoiceAdvanceLineItemExceptions(objExportInvoice.InvoiceDataSet.InvoiceLineItemException.Where(t => t.InvoiceItemId == row.DocumentItemId).ToList(),
                //        sbAdvanceLineItemSectionHtml, usertype);
                //    }
                //    else if (sbAdvanceLineItemSectionHtml.ToString().Contains("##AdvanceLineItemErrorSection##"))
                //    {
                //        ExportInvoice objExportInvoice = (ExportInvoice)objExport;
                //        List<P2PDocumentDataSet.P2PItemRow> objExportInvoiceItem = objExportInvoice.DocumentDataSet.P2PItem.Where(t => t.DocumentItemId == row.DocumentItemId).ToList();
                //        FillInvoiceMaterialLineItemErrors(objExportInvoiceItem, sbAdvanceLineItemSectionHtml, usertype);
                //    }
                //}
                else if (docType == P2PDocumentType.Order)
                {
                    sbAdvanceLineItemSectionHtml.Replace("##lblMaterialLineItemReceivingStatus##", Convert.ToBoolean(row.IsCloseForReceiving) ? "Closed" : "Open");
                    sbAdvanceLineItemSectionHtml.Replace("##lblMaterialLineItemInvoicingStatus##", Convert.ToBoolean(row.IsCloseForInvoicing) ? "Closed" : "Open");
                }
                sbAdvanceLineItemSectionHtml.Replace("^^AdvanceLineItem^^", string.Empty);
                sbAdvanceLineItemSectionHtml.Replace("^^/AdvanceLineItem^^", string.Empty);
                if (sbAdvanceLineItemSectionHtml.ToString().Contains("^AdvanceLineItemGroup^^"))
                    FillAdvanceLineItemComments(
                        dsDocument.LineItemComment.Where(t => t.P2PLineItemId == row.P2PLineItemId).ToList(),
                        sbAdvanceLineItemSectionHtml);

                long itm = docType == P2PDocumentType.InvoiceReconciliation ? row.InvoiceItemId : row.DocumentItemId;
                //FillCustomFields(itm, Convert.ToInt64(row.FormID), sbAdvanceLineItemSectionHtml, "Item");
                FillAccountingFields(dsDocument.AccountingDetails.Where(x => x.DocumentItemID == row.DocumentItemId).ToList(), sbAdvanceLineItemSectionHtml, row.ItemTypeID, docType);
                FillRequisitionItemAdditionalFields(objExport, row.DocumentItemId, sbAdvanceLineItemSectionHtml);
                if (docType == P2PDocumentType.Receipt || docType == P2PDocumentType.ReturnNote)
                {
                    if (sbAdvanceLineItemSectionHtml.ToString().Contains("##MultipleOrderDetailSection##"))
                        FillReceiptItemOrderDetails(row, sbAdvanceLineItemSectionHtml);
                }

                long num = stgAllowLineNumber ? row.ItemLineNumber : sNO;
                tempAdvanceList.Add(new KeyValuePair<long, StringBuilder>(num, sbAdvanceLineItemSectionHtml));
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occurred in BindAdvanceItem method in ExportManager for P2PLineItemId = " + row.P2PLineItemId, ex);
                throw;
            }
        }

        public void FillAccountingFields(List<P2PDocumentDataSet.AccountingDetailsRow> lstSplits, StringBuilder sbHtml, int ItemTypeId, P2PDocumentType docType = 0)
        {
            try
            {
                string accSection = GetSectionHtml(sbHtml.ToString(), "##AccountingSection##", "##/AccountingSection##");
                if (accSection != "")
                {
                    StringBuilder rows = new StringBuilder();
                    if (lstSplits.Count > 0)
                    {
                        int srNo = 1;
                        List<P2PDocumentDataSet.AccountingDetailsRow> lstFields;
                        P2PDocumentDataSet.AccountingDetailsRow field;
                        P2PDocumentDataSet.AccountingDetailsRow firstField;
                        StringBuilder accTmplInstance;
                        StringBuilder fieldNameInstance;
                        StringBuilder fieldValInstance;
                        StringBuilder fieldNamesRow;
                        StringBuilder fieldValuesRow;
                        StringBuilder fields;

                        //get templates and sections --start
                        string accTmpl = GetSectionHtml(accSection, "##AccountingTmpl##", "##/AccountingTmpl##");
                        string fieldSection = GetSectionHtml(accSection, "##FieldSection##", "##/FieldSection##");

                        string fieldNameTmpl = new StringBuilder(GetSectionHtml(accSection, "##AccountingNameTmpl##", "##/AccountingNameTmpl##"))
                            .Replace("##AccountingNameTmpl##", string.Empty)
                            .Replace("##/AccountingNameTmpl##", string.Empty).ToString();
                        string fieldValTmpl = new StringBuilder(GetSectionHtml(accSection, "##AccountingValTmpl##", "##/AccountingValTmpl##"))
                            .Replace("##AccountingValTmpl##", string.Empty)
                            .Replace("##/AccountingValTmpl##", string.Empty).ToString();


                        string lblUp = new StringBuilder(GetSectionHtml(fieldSection, "##UnitPriceLabel##", "##/UnitPriceLabel##"))
                            .Replace("##UnitPriceLabel##", string.Empty)
                            .Replace("##/UnitPriceLabel##", string.Empty).ToString();
                        string lblQty = new StringBuilder(GetSectionHtml(fieldSection, "##QuantityLabel##", "##/QuantityLabel##"))
                            .Replace("##QuantityLabel##", string.Empty)
                            .Replace("##/QuantityLabel##", string.Empty).ToString();
                        string lblTot = new StringBuilder(GetSectionHtml(fieldSection, "##TotalLabel##", "##/TotalLabel##"))
                            .Replace("##TotalLabel##", string.Empty)
                            .Replace("##/TotalLabel##", string.Empty).ToString();


                        string rowStart = new StringBuilder(GetSectionHtml(accTmpl.ToString(), "##AccountingFieldRowStart##", "##/AccountingFieldRowStart##"))
                            .Replace("##AccountingFieldRowStart##", string.Empty)
                            .Replace("##/AccountingFieldRowStart##", string.Empty).ToString();
                        string rowEnd = new StringBuilder(GetSectionHtml(accTmpl.ToString(), "##AccountingFieldRowEnd##", "##/AccountingFieldRowEnd##"))
                            .Replace("##AccountingFieldRowEnd##", string.Empty)
                            .Replace("##/AccountingFieldRowEnd##", string.Empty).ToString();


                        string lblSrNo = new StringBuilder(GetSectionHtml(fieldSection, "##SrNoLabel##", "##/SrNoLabel##"))
                            .Replace("##SrNoLabel##", string.Empty)
                            .Replace("##/SrNoLabel##", string.Empty).ToString();
                        string valSrNo = new StringBuilder(GetSectionHtml(fieldSection, "##SrNoVal##", "##/SrNoVal##"))
                            .Replace("##SrNoVal##", string.Empty)
                            .Replace("##/SrNoVal##", string.Empty).ToString();
                        string empty = new StringBuilder(GetSectionHtml(fieldSection, "##AccEmpty##", "##/AccEmpty##"))
                            .Replace("##AccEmpty##", string.Empty)
                            .Replace("##/AccEmpty##", string.Empty).ToString();
                        //get templates and sections --end

                        int counter = 3;
                        if (string.IsNullOrEmpty(lblUp))
                        {
                            counter--;
                        }
                        if (string.IsNullOrEmpty(lblQty))
                        {
                            counter--;
                        }
                        if (string.IsNullOrEmpty(lblTot))
                        {
                            counter--;
                        }


                        foreach (var splitId in lstSplits.Select(x => x.SplitItemId).Distinct().ToList())
                        {
                            fields = new StringBuilder();
                            accTmplInstance = new StringBuilder(accTmpl)
                                .Replace("##AccountingTmpl##", string.Empty)
                            .Replace("##/AccountingTmpl##", string.Empty);
                            lstFields = lstSplits.Where(x => x.SplitItemId == splitId).OrderBy(x => x.FieldOrder).ToList();


                            firstField = lstFields[0];      //will be used to bind qty, UP, total
                            int fieldCount = lstFields.Count;

                            if (fieldCount + counter > 0)     // fields + (qty, UP, total) hence (count + 3)
                            {
                                for (int i = 0; (i < ((fieldCount + counter) / 5) + 1) && (5 * i != fieldCount + counter); i++) // running loop for 5 fields at a time
                                {
                                    //initiate a row
                                    fieldNamesRow = new StringBuilder();
                                    fieldValuesRow = new StringBuilder();
                                    fields.Append(rowStart);
                                    if (i == 0)
                                    {
                                        fieldNamesRow.Append(lblSrNo);
                                        fieldValuesRow.Append(valSrNo);
                                    }
                                    else
                                    {
                                        fieldNamesRow.Append(empty);
                                        fieldValuesRow.Append(empty);
                                    }

                                    //bind fields in the row
                                    for (int j = 0; j < 5; j++)
                                    {
                                        fieldNameInstance = new StringBuilder(fieldNameTmpl);
                                        fieldValInstance = new StringBuilder(fieldValTmpl);

                                        if (ItemTypeId != 3)
                                        {
                                            if (5 * i + j < fieldCount)
                                            {
                                                field = lstFields[5 * i + j];
                                                fieldNameInstance.Replace("##AccountingLabel##", field.EntityType);
                                                if ((docType == P2PDocumentType.Order && field.EntityTitle == "Requester") || (field.EntityType == "Requester"))
                                                    fieldValInstance.Replace("##AccountingValue##", field.EntityDisplayName + "  " + field.EntityCode);
                                                else if ((docType == P2PDocumentType.Order && field.EntityTitle == "Fund") || (field.EntityType == "Fund"))
                                                    fieldValInstance.Replace("##AccountingValue##", field.EntityDisplayName);
                                                else if (field.EntityDisplayName != "")
                                                    fieldValInstance.Replace("##AccountingValue##",
                                                        (field.EntityCode != "" ? field.EntityCode + " - " : "")
                                                        + field.EntityDisplayName);
                                            }
                                            else if (5 * i + j == fieldCount && lblUp != "")
                                            {
                                                fieldNameInstance.Replace("##AccountingLabel##", lblUp);
                                                fieldValInstance.Replace("##AccountingValue##", ConvertToString(Math.Round((firstField.SplitItemTax), maxPrec)));////SplitItemTax used for Unit Price
                                            }
                                            else if (5 * i + j == fieldCount + 1 && lblQty != "")
                                            {
                                                fieldNameInstance.Replace("##AccountingLabel##", lblQty);
                                                fieldValInstance.Replace("##AccountingValue##", ConvertToString(Math.Round(firstField.Quantity, maxPrec)));
                                            }
                                            else if (5 * i + j == fieldCount + 2 && lblTot != "")
                                            {
                                                fieldNameInstance.Replace("##AccountingLabel##", lblTot);
                                                fieldValInstance.Replace("##AccountingValue##", ConvertToString(Math.Round(firstField.SplitItemTotal, maxPrecTot, MidpointRounding.AwayFromZero)));
                                            }
                                        }

                                        else
                                        {
                                            if (5 * i + j < fieldCount)
                                            {
                                                field = lstFields[5 * i + j];
                                                fieldNameInstance.Replace("##AccountingLabel##", field.EntityType);
                                                if ((docType == P2PDocumentType.Order && field.EntityTitle == "Requester") || (field.EntityType == "Requester"))
                                                    fieldValInstance.Replace("##AccountingValue##", field.EntityDisplayName + "  " + field.EntityCode);
                                                else if ((docType == P2PDocumentType.Order && field.EntityTitle == "Fund") || (field.EntityType == "Fund"))
                                                    fieldValInstance.Replace("##AccountingValue##", field.EntityDisplayName);
                                                else if (field.EntityDisplayName != "")
                                                    fieldValInstance.Replace("##AccountingValue##",
                                                        (field.EntityCode != "" ? field.EntityCode + " - " : "")
                                                        + field.EntityDisplayName);
                                            }
                                        }

                                        fieldNameInstance.Replace("##AccountingLabel##", string.Empty);
                                        fieldValInstance.Replace("##AccountingValue##", string.Empty);
                                        fieldNamesRow.Append(fieldNameInstance.ToString());
                                        fieldValuesRow.Append(fieldValInstance.ToString());

                                    }

                                    //append the binded fields and end the row
                                    fields.Append(fieldNamesRow);
                                    fields.Append(rowEnd);
                                    fields.Append(rowStart);
                                    fields.Append(fieldValuesRow);
                                    fields.Append(rowEnd);
                                }
                            }

                            //append the template instance of the split
                            if (!string.IsNullOrWhiteSpace(fieldSection))
                                accTmplInstance.Replace(fieldSection, fields.ToString());
                            accTmplInstance.Replace("##SrNoAcc##", srNo++.ToString());
                            rows.Append(accTmplInstance);
                        }
                        if (!string.IsNullOrWhiteSpace(accTmpl))
                            sbHtml.Replace(accTmpl, rows.ToString());
                        sbHtml.Replace("##AccountingSection##", string.Empty);
                        sbHtml.Replace("##/AccountingSection##", string.Empty);
                    }
                    else if (!string.IsNullOrWhiteSpace(accSection))
                        sbHtml.Replace(accSection, string.Empty);
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occurred in FillAccountingDetails method for DocumentItemID = " + lstSplits[0].DocumentItemID, ex);
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objMaterialItem"></param>
        /// <param name="sbOrderHtml"></param>
        public StringBuilder FillQuestionnaireResponceDetails(StringBuilder sbMaterialLineItemSectionHtml, List<P2PDocumentDataSet.QuestionnaireRow> questionnaires)
        {
            string strItemQuestionnaireSectionHtml = GetSectionHtml(sbMaterialLineItemSectionHtml.ToString(), "^^LineItemQuestionnaire^^",
                                                                                       "^^/LineItemQuestionnaire^^");
            string strItemQuestionnaireTitleHtml = GetSectionHtml(strItemQuestionnaireSectionHtml.ToString(), "^^LineItemQuestionnaireTitle^^",
                                                            "^^/LineItemQuestionnaireTitle^^");
            string strLineItemQuestionSectionHtml = GetSectionHtml(strItemQuestionnaireSectionHtml.ToString(), "^^LineItemQuestion^^",
                                                            "^^/LineItemQuestion^^");

            var sbItemQuestionnairesHtml = new StringBuilder();
            long QuestionnaireCode = 0;
            foreach (var questionnaire in questionnaires)
            {
                if (QuestionnaireCode != questionnaire.QuestionnaireCode)
                {
                    QuestionnaireCode = questionnaire.QuestionnaireCode;
                    var sbItemQuestionnaireTitleHtml = new StringBuilder(strItemQuestionnaireTitleHtml);
                    sbItemQuestionnaireTitleHtml.Replace("##QuestionnaireTitle##", BaseDAO.HtmlEncode(questionnaire.QuestionnaireTitle));
                    sbItemQuestionnaireTitleHtml.Replace("^^LineItemQuestionnaireTitle^^", string.Empty);
                    sbItemQuestionnaireTitleHtml.Replace("^^/LineItemQuestionnaireTitle^^", string.Empty);
                    sbItemQuestionnairesHtml.Append(sbItemQuestionnaireTitleHtml.ToString());
                }

                var sbItemQuestionSectionHtml = new StringBuilder(strLineItemQuestionSectionHtml);
                sbItemQuestionSectionHtml.Replace("##Question##", questionnaire.SortOrder + "." + BaseDAO.HtmlEncode(questionnaire.QuestionText));
                sbItemQuestionSectionHtml.Replace("##Responce##", questionnaire.ResponseValue);
                sbItemQuestionSectionHtml.Replace("^^LineItemQuestion^^", string.Empty);
                sbItemQuestionSectionHtml.Replace("^^/LineItemQuestion^^", string.Empty);
                sbItemQuestionnairesHtml.Append(sbItemQuestionSectionHtml);

            }
            return sbItemQuestionnairesHtml;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objMaterialItem"></param>
        /// <param name="sbOrderHtml"></param>
        private void FillMaterialLineItemManufacturerDetails(P2PDocumentDataSet.P2PItemRow objMaterialItem, StringBuilder sbOrderHtml, Boolean allowManufacturersCodeAndModel)
        {
            string strMaterialLineItemManufacturerDetailSectionHtml = string.Empty;
            var sbMaterialLineItemManufacturerDetailHtml = new StringBuilder();
            strMaterialLineItemManufacturerDetailSectionHtml = GetSectionHtml(sbOrderHtml.ToString(), "##MaterialLineItemManufacturerDetailSection##",
                                                               "##/MaterialLineItemManufacturerDetailSection##");
            if (objMaterialItem.ManufacturerName != string.Empty || objMaterialItem.ManufacturerPartNumber != string.Empty)
            {
                string strMaterialLineItemManufacturerGroupSectionHtml = string.Empty;
                string strMaterialLineItemManufacturersSectionHtml = string.Empty;
                var sbMaterialLineItemManufacturerGroupHtml = new StringBuilder();
                var sbMaterialLineItemManufacturersHtml = new StringBuilder();
                string[] strManufacturerNames = objMaterialItem.ManufacturerName.ToString().Split('|');
                string[] strManufacturerPartNumbers = objMaterialItem.ManufacturerPartNumber.ToString().Split('|');
                string[] strManufacturerModel = objMaterialItem.ManufacturerModel.ToString().Split('|');
                string[] strManufacturerSuplierCode = objMaterialItem.ManufacturerSupplierCode.ToString().Split('|');

                if (strManufacturerNames.Length > 0)
                {

                    strMaterialLineItemManufacturerGroupSectionHtml = GetSectionHtml(sbOrderHtml.ToString(), "^^MaterialLineItemManufacturerGroup^^",
                                                       "^^/MaterialLineItemManufacturerGroup^^");

                    StringBuilder sbMaterialItemManufacturerGroupSectionHtml = new StringBuilder();

                    for (int i = 0; i < strManufacturerNames.Length; i++)
                    {
                        if (i > 0)
                        {
                            if (strMaterialLineItemManufacturersSectionHtml != null && strMaterialLineItemManufacturersSectionHtml != string.Empty)
                                sbMaterialItemManufacturerGroupSectionHtml.Replace(strMaterialLineItemManufacturersSectionHtml, sbMaterialLineItemManufacturersHtml.ToString());
                        }

                        sbMaterialItemManufacturerGroupSectionHtml.Replace("^^MaterialLineItemManufacturerGroup^^", string.Empty);
                        sbMaterialItemManufacturerGroupSectionHtml.Replace("^^/MaterialLineItemManufacturerGroup^^", string.Empty);

                        sbMaterialLineItemManufacturerGroupHtml.Append(sbMaterialItemManufacturerGroupSectionHtml.ToString());

                        sbMaterialItemManufacturerGroupSectionHtml.Length = 0;
                        sbMaterialItemManufacturerGroupSectionHtml = new StringBuilder(strMaterialLineItemManufacturerGroupSectionHtml);

                        sbMaterialLineItemManufacturersHtml.Length = 0;
                        strMaterialLineItemManufacturersSectionHtml = GetSectionHtml(sbMaterialItemManufacturerGroupSectionHtml.ToString(), "^^MaterialLineItemManufacturers^^",
                                                                  "^^/MaterialLineItemManufacturers^^");

                        StringBuilder sbMaterialItemManufacturersSectionHtml = new StringBuilder(strMaterialLineItemManufacturersSectionHtml);
                        if (sbMaterialItemManufacturersSectionHtml.ToString().Contains("##lblMaterialLineItemManufacturerName##"))
                            sbMaterialItemManufacturersSectionHtml.Replace("##lblMaterialLineItemManufacturerName##", BaseDAO.HtmlEncode(Convert.ToString(strManufacturerNames[i])));

                        if (sbMaterialItemManufacturersSectionHtml.ToString().Contains("##lblMaterialLineItemManufacturerPartNumber##"))
                            sbMaterialItemManufacturersSectionHtml.Replace("##lblMaterialLineItemManufacturerPartNumber##", BaseDAO.HtmlEncode(Convert.ToString(strManufacturerPartNumbers[i])));
                 
                        if (sbMaterialItemManufacturersSectionHtml.ToString().Contains("##lblMaterialLineItemManufacturerSuplierCode##"))
                                sbMaterialItemManufacturersSectionHtml.Replace("##lblMaterialLineItemManufacturerSuplierCode##", BaseDAO.HtmlEncode(Convert.ToString(strManufacturerSuplierCode[i])));

                        if (sbMaterialItemManufacturersSectionHtml.ToString().Contains("##lblMaterialLineItemManufacturerModel##"))
                                sbMaterialItemManufacturersSectionHtml.Replace("##lblMaterialLineItemManufacturerModel##", BaseDAO.HtmlEncode(Convert.ToString(strManufacturerModel[i])));

                        sbMaterialItemManufacturersSectionHtml.Replace("^^MaterialLineItemManufacturers^^", string.Empty);
                        sbMaterialItemManufacturersSectionHtml.Replace("^^/MaterialLineItemManufacturers^^", string.Empty);
                        sbMaterialLineItemManufacturersHtml.Append(sbMaterialItemManufacturersSectionHtml);
                    }
                    if (!string.IsNullOrWhiteSpace(strMaterialLineItemManufacturersSectionHtml))
                        sbMaterialItemManufacturerGroupSectionHtml.Replace(strMaterialLineItemManufacturersSectionHtml, sbMaterialLineItemManufacturersHtml.ToString());
                    sbMaterialItemManufacturerGroupSectionHtml.Replace("^^MaterialLineItemManufacturerGroup^^", string.Empty);
                    sbMaterialItemManufacturerGroupSectionHtml.Replace("^^/MaterialLineItemManufacturerGroup^^", string.Empty);
                    sbMaterialLineItemManufacturerGroupHtml.Append(sbMaterialItemManufacturerGroupSectionHtml.ToString());

                    sbOrderHtml.Replace("##MaterialLineItemManufacturerDetailSection##", string.Empty);
                    sbOrderHtml.Replace("##/MaterialLineItemManufacturerDetailSection##", string.Empty);

                }
                else
                {
                    strMaterialLineItemManufacturerGroupSectionHtml = GetSectionHtml(sbOrderHtml.ToString(), "##MaterialLineItemManufacturerDetailSection##",
                                                                   "##/MaterialLineItemManufacturerDetailSection##");
                }
                if (strMaterialLineItemManufacturerGroupSectionHtml != null && strMaterialLineItemManufacturerGroupSectionHtml != string.Empty)
                    sbOrderHtml.Replace(strMaterialLineItemManufacturerGroupSectionHtml, sbMaterialLineItemManufacturerGroupHtml.ToString());
            }
            else
            {
                if (strMaterialLineItemManufacturerDetailSectionHtml != null && strMaterialLineItemManufacturerDetailSectionHtml != string.Empty)
                    sbOrderHtml.Replace(strMaterialLineItemManufacturerDetailSectionHtml, sbMaterialLineItemManufacturerDetailHtml.ToString());
            }

            if (allowManufacturersCodeAndModel)
            {
                if (sbOrderHtml.ToString().Contains("##headerlblMaterialLineItemManufacturerSuplierCode##"))
                    sbOrderHtml.Replace("##headerlblMaterialLineItemManufacturerSuplierCode##", "Mfr. Supplier Code");

                if (sbOrderHtml.ToString().Contains("##headerlblMaterialLineItemManufacturerModel##"))
                    sbOrderHtml.Replace("##headerlblMaterialLineItemManufacturerModel##", "Mfr. Model");
            }
            else
            {
                if (sbOrderHtml.ToString().Contains("##headerlblMaterialLineItemManufacturerSuplierCode##"))
                    sbOrderHtml.Replace("##headerlblMaterialLineItemManufacturerSuplierCode##", string.Empty);

                if (sbOrderHtml.ToString().Contains("##headerlblMaterialLineItemManufacturerModel##"))
                    sbOrderHtml.Replace("##headerlblMaterialLineItemManufacturerModel##", string.Empty);
            }

        } 

        public void FillMaterialLineItemShippingDetails(P2PDocumentDataSet.P2PItemRow objMaterialItem, StringBuilder sbHtml)
        {
            string strMaterialLineItemShippingDetailSectionHtml = string.Empty;
            var sbMaterialLineItemShippingDetailHtml = new StringBuilder();
            RequisitionCommonManager commonManager = new RequisitionCommonManager (this.JWTToken){ UserContext = UserContext, GepConfiguration = GepConfiguration };
            SettingDetails objSettings = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.None, 1, 107);
            var AllowDeliverToFreeText = Convert.ToBoolean(commonManager.GetSettingsValueByKey(objSettings, "AllowDeliverToFreeText"));

            if ((objMaterialItem.DelivertoLocationName == string.Empty && objMaterialItem.ShiptoLocationID == 0) || objMaterialItem.ShiptoLocationID != 0 || ((objMaterialItem.DelivertoLocationName != string.Empty) || objMaterialItem.DeliverTo.Trim() != string.Empty))
            {

                if (objMaterialItem.ShippingMethod != string.Empty || objMaterialItem.ShiptoLocationName != string.Empty)
                {

                    strMaterialLineItemShippingDetailSectionHtml = GetSectionHtml(sbHtml.ToString(), "##MaterialLineItemShippingDetailSection##",
                                                                   "##/MaterialLineItemShippingDetailSection##");

                    var sbMaterialLineItemSectionHtml = new StringBuilder(strMaterialLineItemShippingDetailSectionHtml);

                    sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemShippingMethod##",
                                                             Convert.ToString(objMaterialItem.ShippingMethod));

                    if (sbMaterialLineItemSectionHtml.ToString().Contains("##lblMaterialLineItemShipToLocationNumber##"))
                    {
                        sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemShipToLocationNumber##", (objMaterialItem.ShipToLocationNumber == null) ? string.Empty : BaseDAO.HtmlEncode(Convert.ToString(objMaterialItem.ShipToLocationNumber)));
                    }

                    sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemShipTo##",
                                                          BaseDAO.HtmlEncode(objMaterialItem.ShiptoLocationName));

                    sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemShippingAddress##",
                                                         CreateAddress("",
                                                                       BaseDAO.HtmlEncode(objMaterialItem.ShiptoLocationAddressLine1),
                                                                       BaseDAO.HtmlEncode(objMaterialItem.ShiptoLocationAddressLine2),
                                                                       BaseDAO.HtmlEncode(objMaterialItem.ShiptoLocationAddressLine3),
                                                                       BaseDAO.HtmlEncode(objMaterialItem.ShiptoLocationCity),
                                                                       BaseDAO.HtmlEncode(objMaterialItem.ShiptoLocationState),
                                                                       BaseDAO.HtmlEncode(objMaterialItem.ShiptoLocationCountry),
                                                                       BaseDAO.HtmlEncode(objMaterialItem.ShiptoLocationZip)));

                    if (objMaterialItem.DeliverTo.Trim() != string.Empty && AllowDeliverToFreeText)
                    {
                        sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemDeliverTo##",
                                                       BaseDAO.HtmlEncode(objMaterialItem.DeliverTo));
                        sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemDeliverToAddress##", string.Empty);
                        sbMaterialLineItemSectionHtml.Replace("##lblDeliverToAddressLabel##", string.Empty);

                    }
                    else
                    {
                        sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemDeliverTo##",
                                                        BaseDAO.HtmlEncode(objMaterialItem.DelivertoLocationName));
                        sbMaterialLineItemSectionHtml.Replace("##lblDeliverToAddressLabel##", "Deliver To Address");
                        sbMaterialLineItemSectionHtml.Replace("##lblMaterialLineItemDeliverToAddress##",
                                                             CreateAddress("",
                                                                           BaseDAO.HtmlEncode(objMaterialItem.DelivertoLocationAddressLine1),
                                                                           BaseDAO.HtmlEncode(objMaterialItem.DelivertoLocationAddressLine2),
                                                                           BaseDAO.HtmlEncode(objMaterialItem.DelivertoLocationAddressLine3),
                                                                           BaseDAO.HtmlEncode(objMaterialItem.DelivertoLocationCity),
                                                                           BaseDAO.HtmlEncode(objMaterialItem.DelivertoLocationStateName),
                                                                           BaseDAO.HtmlEncode(objMaterialItem.DelivertoLocationCountryName),
                                                                           BaseDAO.HtmlEncode(objMaterialItem.DelivertoLocationZipCode)));
                    }

                    sbMaterialLineItemSectionHtml.Replace("##MaterialLineItemShippingDetailSection##", string.Empty);
                    sbMaterialLineItemSectionHtml.Replace("##/MaterialLineItemShippingDetailSection##", string.Empty);
                    sbMaterialLineItemShippingDetailHtml.Append(sbMaterialLineItemSectionHtml.ToString());

                }
                else
                    strMaterialLineItemShippingDetailSectionHtml = GetSectionHtml(sbHtml.ToString(), "##MaterialLineItemShippingDetailSection##",
                                                                   "##/MaterialLineItemShippingDetailSection##");
            }
            else
                strMaterialLineItemShippingDetailSectionHtml = GetSectionHtml(sbHtml.ToString(), "##MaterialLineItemShippingDetailSection##",
                                                               "##/MaterialLineItemShippingDetailSection##");
            if (strMaterialLineItemShippingDetailSectionHtml != null && strMaterialLineItemShippingDetailSectionHtml != string.Empty)
                sbHtml.Replace(strMaterialLineItemShippingDetailSectionHtml, sbMaterialLineItemShippingDetailHtml.ToString());

        }

        public void FillCommonLineItemShippingDetails(P2PDocumentDataSet.P2PItemRow objCommonItem, StringBuilder sbHtml)
        {
            string strCommonLineItemShippingDetailSectionHtml = string.Empty;
            var sbCommonLineItemShippingDetailHtml = new StringBuilder();
            RequisitionCommonManager commonManager = new RequisitionCommonManager (this.JWTToken){ UserContext = UserContext, GepConfiguration = GepConfiguration };
            SettingDetails objSettings = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.None, 1, 107);
            var AllowDeliverToFreeText = Convert.ToBoolean(commonManager.GetSettingsValueByKey(objSettings, "AllowDeliverToFreeText"));

            if ((objCommonItem.DelivertoLocationName == string.Empty && objCommonItem.ShiptoLocationID == 0) || objCommonItem.ShiptoLocationID != 0 || ((objCommonItem.DelivertoLocationName != string.Empty) || objCommonItem.DeliverTo.Trim() != string.Empty))
            {

                if (objCommonItem.ShippingMethod != string.Empty || objCommonItem.ShiptoLocationName != string.Empty || objCommonItem.ShiptoLocationAddressLine1 != string.Empty)
                {

                    strCommonLineItemShippingDetailSectionHtml = GetSectionHtml(sbHtml.ToString(), "##CommonLineItemShippingDetailSection##",
                                                                   "##/CommonLineItemShippingDetailSection##");

                    var sbCommonLineItemSectionHtml = new StringBuilder(strCommonLineItemShippingDetailSectionHtml);

                    sbCommonLineItemSectionHtml.Replace("##lblCommonLineItemShippingMethod##",
                                                             Convert.ToString(objCommonItem.ShippingMethod));
                    sbCommonLineItemSectionHtml.Replace("##lblCommonLineItemShipTo##",
                                                          BaseDAO.HtmlEncode(objCommonItem.ShiptoLocationName));

                    sbCommonLineItemSectionHtml.Replace("##lblCommonLineItemShippingAddress##",
                                                         CreateAddress("",
                                                                       BaseDAO.HtmlEncode(objCommonItem.ShiptoLocationAddressLine1),
                                                                       BaseDAO.HtmlEncode(objCommonItem.ShiptoLocationAddressLine2),
                                                                       BaseDAO.HtmlEncode(objCommonItem.ShiptoLocationAddressLine3),
                                                                       BaseDAO.HtmlEncode(objCommonItem.ShiptoLocationCity),
                                                                       BaseDAO.HtmlEncode(objCommonItem.ShiptoLocationState),
                                                                       BaseDAO.HtmlEncode(objCommonItem.ShiptoLocationCountry),
                                                                       BaseDAO.HtmlEncode(objCommonItem.ShiptoLocationZip)));

                    if (objCommonItem.DeliverTo.Trim() != string.Empty && AllowDeliverToFreeText)
                    {
                        sbCommonLineItemSectionHtml.Replace("##lblCommonLineItemDeliverTo##",
                                                       BaseDAO.HtmlEncode(objCommonItem.DeliverTo));
                        sbCommonLineItemSectionHtml.Replace("##lblCommonLineItemDeliverToAddress##", string.Empty);
                        sbCommonLineItemSectionHtml.Replace("##lblDeliverToAddressLabel##", string.Empty);

                    }
                    else
                    {
                        sbCommonLineItemSectionHtml.Replace("##lblCommonLineItemDeliverTo##",
                                                        BaseDAO.HtmlEncode(objCommonItem.DelivertoLocationName));
                        sbCommonLineItemSectionHtml.Replace("##lblDeliverToAddressLabel##", "Deliver To Address");
                        sbCommonLineItemSectionHtml.Replace("##lblCommonLineItemDeliverToAddress##",
                                                             CreateAddress("",
                                                                           BaseDAO.HtmlEncode(objCommonItem.DelivertoLocationAddressLine1),
                                                                           BaseDAO.HtmlEncode(objCommonItem.DelivertoLocationAddressLine2),
                                                                           BaseDAO.HtmlEncode(objCommonItem.DelivertoLocationAddressLine3),
                                                                           BaseDAO.HtmlEncode(objCommonItem.DelivertoLocationCity),
                                                                           BaseDAO.HtmlEncode(objCommonItem.DelivertoLocationStateName),
                                                                           BaseDAO.HtmlEncode(objCommonItem.DelivertoLocationCountryName),
                                                                           BaseDAO.HtmlEncode(objCommonItem.DelivertoLocationZipCode)));
                    }

                    sbCommonLineItemSectionHtml.Replace("##CommonLineItemShippingDetailSection##", string.Empty);
                    sbCommonLineItemSectionHtml.Replace("##/CommonLineItemShippingDetailSection##", string.Empty);
                    sbCommonLineItemShippingDetailHtml.Append(sbCommonLineItemSectionHtml.ToString());

                }
                else
                    strCommonLineItemShippingDetailSectionHtml = GetSectionHtml(sbHtml.ToString(), "##CommonLineItemShippingDetailSection##",
                                                                   "##/CommonLineItemShippingDetailSection##");
            }
            else
                strCommonLineItemShippingDetailSectionHtml = GetSectionHtml(sbHtml.ToString(), "##CommonLineItemShippingDetailSection##",
                                                               "##/CommonLineItemShippingDetailSection##");
            if (strCommonLineItemShippingDetailSectionHtml != null && strCommonLineItemShippingDetailSectionHtml != string.Empty)
                sbHtml.Replace(strCommonLineItemShippingDetailSectionHtml, sbCommonLineItemShippingDetailHtml.ToString());
        }

        public void FillReceiptAssettagDetails(List<ReceiptDataSet.AssetTagRow> assetTags, StringBuilder sbMaterialLineItemHtml)
        {

            string strMaterialLineItemAssetTagDetailSectionHtml = string.Empty;
            string strMaterialLineItemAssetTagDetailRowSectionHtml = string.Empty;
            var sbMaterialLineItemAssettagDetailHtml = new StringBuilder();
            var sbMaterialLineItemAssettagDetailRowHtml = new StringBuilder();
            if (assetTags.Count > 0)
            {
                strMaterialLineItemAssetTagDetailSectionHtml = GetSectionHtml(sbMaterialLineItemHtml.ToString(), "##MaterialLineItemAssetTagDetailSection##",
                                                                                 "##/MaterialLineItemAssetTagDetailSection##");
                StringBuilder sbMaterialLineItemAssettagDetailSectionHtml = new StringBuilder();
                string strGroupText = string.Empty;
                foreach (var row in assetTags)
                {
                    if (string.IsNullOrEmpty(strGroupText))
                    {
                        sbMaterialLineItemAssettagDetailSectionHtml = new StringBuilder(strMaterialLineItemAssetTagDetailSectionHtml);
                    }
                    strMaterialLineItemAssetTagDetailRowSectionHtml = GetSectionHtml(sbMaterialLineItemAssettagDetailSectionHtml.ToString(), "^^MaterialLineItemAssetTagDetail^^", "^^/MaterialLineItemAssetTagDetail^^");
                    StringBuilder sbMaterialLineItemAssetTagDetailRowSectionHtml = new StringBuilder(strMaterialLineItemAssetTagDetailRowSectionHtml);
                    sbMaterialLineItemAssetTagDetailRowSectionHtml.Replace("##lblSerialNumber##", Convert.ToString(row.SerialNumber));
                    sbMaterialLineItemAssetTagDetailRowSectionHtml.Replace("##lblAssetKey##", Convert.ToString(row.AssetKey));
                    sbMaterialLineItemAssetTagDetailRowSectionHtml.Replace("##lblAssetLocation##", Convert.ToString(row.AssetLocation));
                    sbMaterialLineItemAssetTagDetailRowSectionHtml.Replace("##lblAssetReturn##", Convert.ToString(row.AssetReturn));
                    sbMaterialLineItemAssetTagDetailRowSectionHtml.Replace("^^MaterialLineItemAssetTagDetail^^", string.Empty);
                    sbMaterialLineItemAssetTagDetailRowSectionHtml.Replace("^^/MaterialLineItemAssetTagDetail^^", string.Empty);
                    sbMaterialLineItemAssettagDetailRowHtml.Append(sbMaterialLineItemAssetTagDetailRowSectionHtml);
                    strGroupText = row.AssetKey;
                }
                if (strMaterialLineItemAssetTagDetailRowSectionHtml != null && strMaterialLineItemAssetTagDetailRowSectionHtml != string.Empty)
                    sbMaterialLineItemAssettagDetailSectionHtml.Replace(strMaterialLineItemAssetTagDetailRowSectionHtml, sbMaterialLineItemAssettagDetailRowHtml.ToString());
                sbMaterialLineItemAssettagDetailSectionHtml.Replace("##MaterialLineItemAssetTagDetailSection##", string.Empty);
                sbMaterialLineItemAssettagDetailSectionHtml.Replace("##/MaterialLineItemAssetTagDetailSection##", string.Empty);
                sbMaterialLineItemAssettagDetailHtml.Append(sbMaterialLineItemAssettagDetailSectionHtml);
            }
            else
            {
                strMaterialLineItemAssetTagDetailSectionHtml = GetSectionHtml(sbMaterialLineItemHtml.ToString(), "##MaterialLineItemAssetTagDetailSection##",
                                                               "##/MaterialLineItemAssetTagDetailSection##");

            }
            if (strMaterialLineItemAssetTagDetailSectionHtml != null && strMaterialLineItemAssetTagDetailSectionHtml != string.Empty)
                sbMaterialLineItemHtml.Replace(strMaterialLineItemAssetTagDetailSectionHtml, sbMaterialLineItemAssettagDetailHtml.ToString());
        }

        /// <summary>
        /// In Order and IR at time of export this method will get call for exporting Material Line Item level comments including related documents.
        /// </summary>
        /// <param name="lstLineItemComments"></param>
        /// <param name="sbMaterialLineItemHtml"></param>
        public void FillMaterialLineItemComments(ICollection<P2PDocumentDataSet.LineItemCommentRow> lstLineItemComments, StringBuilder sbMaterialLineItemHtml)
        {

            string strMaterialLineItemGroupSectionHtml = string.Empty;
            string strMaterialLineItemCommentsSectionHtml = string.Empty;
            var sbMaterialLineItemGroupHtml = new StringBuilder();
            var sbMaterialLineItemCommentsHtml = new StringBuilder();
            if (lstLineItemComments.Count > 0)
            {

                strMaterialLineItemGroupSectionHtml = GetSectionHtml(sbMaterialLineItemHtml.ToString(), "^^MaterialLineItemGroup^^",
                                                   "^^/MaterialLineItemGroup^^");

                StringBuilder sbMaterialLineItemGroupSectionHtml = new StringBuilder();

                string strGroupText = string.Empty;
                foreach (var row in lstLineItemComments)
                {
                    if (string.IsNullOrEmpty(strGroupText))
                    {
                        sbMaterialLineItemGroupSectionHtml = new StringBuilder(strMaterialLineItemGroupSectionHtml);
                        strMaterialLineItemCommentsSectionHtml = GetSectionHtml(sbMaterialLineItemGroupSectionHtml.ToString(), "^^MaterialLineItemComments^^",
                                                              "^^/MaterialLineItemComments^^");
                        sbMaterialLineItemGroupSectionHtml.Replace("##lblMaterialLineItemGroup##", row.GroupText);
                    }
                    else if (row.GroupText.Equals(strGroupText))
                    {

                        strMaterialLineItemCommentsSectionHtml = GetSectionHtml(sbMaterialLineItemGroupSectionHtml.ToString(), "^^MaterialLineItemComments^^",
                                                              "^^/MaterialLineItemComments^^");
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(strMaterialLineItemCommentsSectionHtml))
                            sbMaterialLineItemGroupSectionHtml.Replace(strMaterialLineItemCommentsSectionHtml, sbMaterialLineItemCommentsHtml.ToString());
                        sbMaterialLineItemGroupSectionHtml.Replace("^^MaterialLineItemGroup^^", string.Empty);
                        sbMaterialLineItemGroupSectionHtml.Replace("^^/MaterialLineItemGroup^^", string.Empty);

                        sbMaterialLineItemGroupHtml.Append(sbMaterialLineItemGroupSectionHtml.ToString());

                        sbMaterialLineItemGroupSectionHtml.Length = 0;
                        sbMaterialLineItemGroupSectionHtml = new StringBuilder(strMaterialLineItemGroupSectionHtml);

                        sbMaterialLineItemGroupSectionHtml.Replace("##lblMaterialLineItemGroup##", row.GroupText);
                        sbMaterialLineItemCommentsHtml.Length = 0;
                        strMaterialLineItemCommentsSectionHtml = GetSectionHtml(sbMaterialLineItemGroupSectionHtml.ToString(), "^^MaterialLineItemComments^^",
                                                              "^^/MaterialLineItemComments^^");

                    }
                    StringBuilder sbMaterialLineItemCommentsSectionHtml = new StringBuilder(strMaterialLineItemCommentsSectionHtml);
                    sbMaterialLineItemCommentsSectionHtml.Replace("##lblMaterialLineItemUserName##", Convert.ToString(row.UserName));
                    sbMaterialLineItemCommentsSectionHtml.Replace("##lblMaterialLineItemDateAndTime##", GetDate(row.DateCreated, DateFormatType.commentdate));
                    sbMaterialLineItemCommentsSectionHtml.Replace("##lblMaterialLineItemVisibility##", GetVisibilityComments(row.Visibility));
                    if (row.CommentText != null && row.CommentText != string.Empty)
                    {
                        sbMaterialLineItemCommentsSectionHtml.Replace("##lblMaterialLineItemComment##", Convert.ToString(row.CommentText.Replace("\n", "<br>")));
                    }
                    else
                    {
                        sbMaterialLineItemCommentsSectionHtml.Replace("##lblMaterialLineItemComment##", Convert.ToString(row.CommentText));
                    }
                    sbMaterialLineItemCommentsSectionHtml.Replace("^^MaterialLineItemComments^^", string.Empty);
                    sbMaterialLineItemCommentsSectionHtml.Replace("^^/MaterialLineItemComments^^", string.Empty);
                    sbMaterialLineItemCommentsHtml.Append(sbMaterialLineItemCommentsSectionHtml);
                    strGroupText = row.GroupText;
                }
                if (!string.IsNullOrWhiteSpace(strMaterialLineItemCommentsSectionHtml))
                    sbMaterialLineItemGroupSectionHtml.Replace(strMaterialLineItemCommentsSectionHtml, sbMaterialLineItemCommentsHtml.ToString());
                sbMaterialLineItemGroupSectionHtml.Replace("^^MaterialLineItemGroup^^", string.Empty);
                sbMaterialLineItemGroupSectionHtml.Replace("^^/MaterialLineItemGroup^^", string.Empty);
                sbMaterialLineItemGroupHtml.Append(sbMaterialLineItemGroupSectionHtml.ToString());

                sbMaterialLineItemHtml.Replace("##MaterialLineItemCommentSection##", string.Empty);
                sbMaterialLineItemHtml.Replace("##/MaterialLineItemCommentSection##", string.Empty);

            }
            else
            {
                strMaterialLineItemGroupSectionHtml = GetSectionHtml(sbMaterialLineItemHtml.ToString(), "##MaterialLineItemCommentSection##",
                                                               "##/MaterialLineItemCommentSection##");
            }
            if (strMaterialLineItemGroupSectionHtml != null && strMaterialLineItemGroupSectionHtml != string.Empty)
                sbMaterialLineItemHtml.Replace(strMaterialLineItemGroupSectionHtml, sbMaterialLineItemGroupHtml.ToString());

        }

        public void FillMaterialLineItemRequesterDetails(ICollection<P2PDocumentDataSet.RequesterDetailsRow> lstLineItemRequesterDetails, StringBuilder sbMaterialLineItemHtml)
        {
            string strMaterialLineItemRequesterDetailSectionHtml = string.Empty;
            string strMaterialLineItemRequesterDetailRowSectionHtml = string.Empty;
            var sbMaterialLineItemRequesterDetailHtml = new StringBuilder();
            var sbMaterialLineItemRequesterDetailRowHtml = new StringBuilder();
            if (lstLineItemRequesterDetails.Count > 0)
            {
                strMaterialLineItemRequesterDetailSectionHtml = GetSectionHtml(sbMaterialLineItemHtml.ToString(), "##MaterialLineItemRequesterDetailSection##",
                                                                                 "##/MaterialLineItemRequesterDetailSection##");
                StringBuilder sbMaterialLineItemRequesterDetailSectionHtml = new StringBuilder();
                string strGroupText = string.Empty;
                foreach (var row in lstLineItemRequesterDetails)
                {
                    if (string.IsNullOrEmpty(strGroupText))
                    {
                        sbMaterialLineItemRequesterDetailSectionHtml = new StringBuilder(strMaterialLineItemRequesterDetailSectionHtml);
                    }
                    strMaterialLineItemRequesterDetailRowSectionHtml = GetSectionHtml(strMaterialLineItemRequesterDetailSectionHtml.ToString(), "^^MaterialLineItemRequesterDetail^^", "^^/MaterialLineItemRequesterDetail^^");
                    StringBuilder sbMaterialLineItemRequesterDetailRowSectionHtml = new StringBuilder(strMaterialLineItemRequesterDetailRowSectionHtml);
                    sbMaterialLineItemRequesterDetailRowSectionHtml.Replace("##lblMaterialLineItemRequesterName##", Convert.ToString(row.RequesterName));
                    sbMaterialLineItemRequesterDetailRowSectionHtml.Replace("##lblMaterialLineItemRequesterEmailId##", Convert.ToString(row.RequesterEmailId));
                    sbMaterialLineItemRequesterDetailRowSectionHtml.Replace("##lblMaterialLineItemRequesterPhone##", Convert.ToString(row.RequesterPhone));
                    sbMaterialLineItemRequesterDetailRowSectionHtml.Replace("^^MaterialLineItemRequesterDetail^^", string.Empty);
                    sbMaterialLineItemRequesterDetailRowSectionHtml.Replace("^^/MaterialLineItemRequesterDetail^^", string.Empty);
                    sbMaterialLineItemRequesterDetailRowHtml.Append(sbMaterialLineItemRequesterDetailRowSectionHtml);
                    strGroupText = row.RequesterName;
                }
                if (!string.IsNullOrWhiteSpace(strMaterialLineItemRequesterDetailRowSectionHtml))
                    sbMaterialLineItemRequesterDetailSectionHtml.Replace(strMaterialLineItemRequesterDetailRowSectionHtml, sbMaterialLineItemRequesterDetailRowHtml.ToString());
                sbMaterialLineItemRequesterDetailSectionHtml.Replace("##MaterialLineItemRequesterDetailSection##", string.Empty);
                sbMaterialLineItemRequesterDetailSectionHtml.Replace("##/MaterialLineItemRequesterDetailSection##", string.Empty);
                sbMaterialLineItemRequesterDetailHtml.Append(sbMaterialLineItemRequesterDetailSectionHtml);
            }
            else
            {
                strMaterialLineItemRequesterDetailSectionHtml = GetSectionHtml(sbMaterialLineItemHtml.ToString(), "##MaterialLineItemRequesterDetailSection##",
                                                               "##/MaterialLineItemRequesterDetailSection##");

            }
            if (strMaterialLineItemRequesterDetailSectionHtml != null && strMaterialLineItemRequesterDetailSectionHtml != string.Empty)
                sbMaterialLineItemHtml.Replace(strMaterialLineItemRequesterDetailSectionHtml, sbMaterialLineItemRequesterDetailHtml.ToString());
        }

        public void FillServiceLineItemServiceDetails(P2PDocumentDataSet.P2PItemRow objServiceItem, StringBuilder sbHtml)
        {

            string strServiceLineItemServiceDetailSectionHtml = string.Empty;
            RequisitionCommonManager commonManager = new RequisitionCommonManager (this.JWTToken){ UserContext = UserContext, GepConfiguration = GepConfiguration };
            SettingDetails objSettings = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.None, 1, 107);
            var AllowDeliverToFreeText = Convert.ToBoolean(commonManager.GetSettingsValueByKey(objSettings, "AllowDeliverToFreeText"));
            var sbServiceLineItemServiceDetailHtml = new StringBuilder();
            if (objServiceItem.ShiptoLocationName != string.Empty || objServiceItem.ShiptoLocationAddressLine1 != string.Empty)
            {
                strServiceLineItemServiceDetailSectionHtml = GetSectionHtml(sbHtml.ToString(), "##ServiceLineItemServiceDetailSection##",
                                                               "##/ServiceLineItemServiceDetailSection##");

                var sbServiceLineItemSectionHtml = new StringBuilder(strServiceLineItemServiceDetailSectionHtml);

                sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemServiceToLocation##", Convert.ToString(objServiceItem.ShiptoLocationName));

                if (sbServiceLineItemSectionHtml.ToString().Contains("##lblServiceLineItemShipToLocationNumber##"))
                {
                    sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemShipToLocationNumber##", (objServiceItem.ShipToLocationNumber == null) ? string.Empty : BaseDAO.HtmlEncode(Convert.ToString(objServiceItem.ShipToLocationNumber)));
                }

                sbServiceLineItemSectionHtml.Replace("##lblServiceLineItemServiceToAddress##",
                                                     CreateAddress("",
                                                                   objServiceItem.ShiptoLocationAddressLine1,
                                                                   objServiceItem.ShiptoLocationAddressLine2,
                                                                   objServiceItem.ShiptoLocationAddressLine3,
                                                                   objServiceItem.ShiptoLocationCity,
                                                                   objServiceItem.ShiptoLocationState,
                                                                   objServiceItem.ShiptoLocationCountry,
                                                                   objServiceItem.ShiptoLocationZip));
                if (objServiceItem.DeliverTo.Trim() != string.Empty && AllowDeliverToFreeText)
                {
                    sbServiceLineItemSectionHtml.Replace("##lblServicelLineItemDeliverTo##",
                                                   objServiceItem.DeliverTo);
                    sbServiceLineItemSectionHtml.Replace("##lblServicelLineItemDeliverToAddress##",
                                                   string.Empty);
                    sbServiceLineItemSectionHtml.Replace("##lblDeliverToAddressLabel##", string.Empty);

                }
                else
                {

                    sbServiceLineItemSectionHtml.Replace("##lblServicelLineItemDeliverTo##",
                                                       objServiceItem.DelivertoLocationName);
                    sbServiceLineItemSectionHtml.Replace("##lblDeliverToAddressLabel##", "Deliver To Address");
                    sbServiceLineItemSectionHtml.Replace("##lblServicelLineItemDeliverToAddress##",
                                                         CreateAddress("",
                                                                       objServiceItem.DelivertoLocationAddressLine1,
                                                                       objServiceItem.DelivertoLocationAddressLine2,
                                                                       objServiceItem.DelivertoLocationAddressLine3,
                                                                       objServiceItem.DelivertoLocationCity,
                                                                       objServiceItem.DelivertoLocationStateName,
                                                                       objServiceItem.DelivertoLocationCountryName,
                                                                       objServiceItem.DelivertoLocationZipCode));
                }

                sbServiceLineItemSectionHtml.Replace("##ServiceLineItemServiceDetailSection##", string.Empty);
                sbServiceLineItemSectionHtml.Replace("##/ServiceLineItemServiceDetailSection##", string.Empty);
                sbServiceLineItemServiceDetailHtml.Append(sbServiceLineItemSectionHtml.ToString());
            }
            else
                strServiceLineItemServiceDetailSectionHtml = GetSectionHtml(sbHtml.ToString(), "##ServiceLineItemServiceDetailSection##",
                                                               "##/ServiceLineItemServiceDetailSection##");
            if (strServiceLineItemServiceDetailSectionHtml != null && strServiceLineItemServiceDetailSectionHtml != string.Empty)
                sbHtml.Replace(strServiceLineItemServiceDetailSectionHtml, sbServiceLineItemServiceDetailHtml.ToString());

        }

        public void FillServiceLineItemRequesterDetails(ICollection<P2PDocumentDataSet.RequesterDetailsRow> lstLineItemRequesterDetails, StringBuilder sbServiceLineItemHtml)
        {
            string strServiceLineItemRequesterDetailSectionHtml = string.Empty;
            string strServiceLineItemRequesterDetailRowSectionHtml = string.Empty;
            var sbServiceLineItemRequesterDetailHtml = new StringBuilder();
            var sbServiceLineItemRequesterDetailRowHtml = new StringBuilder();
            if (lstLineItemRequesterDetails.Count > 0)
            {
                strServiceLineItemRequesterDetailSectionHtml = GetSectionHtml(sbServiceLineItemHtml.ToString(), "##ServiceLineItemRequesterDetailSection##",
                                                                                 "##/ServiceLineItemRequesterDetailSection##");
                StringBuilder sbServiceLineItemRequesterDetailSectionHtml = new StringBuilder();
                string strGroupText = string.Empty;
                foreach (var row in lstLineItemRequesterDetails)
                {
                    if (string.IsNullOrEmpty(strGroupText))
                    {
                        sbServiceLineItemRequesterDetailSectionHtml = new StringBuilder(strServiceLineItemRequesterDetailSectionHtml);
                    }
                    strServiceLineItemRequesterDetailRowSectionHtml = GetSectionHtml(strServiceLineItemRequesterDetailSectionHtml.ToString(), "^^ServiceLineItemRequesterDetail^^", "^^/ServiceLineItemRequesterDetail^^");
                    StringBuilder sbServiceLineItemRequesterDetailRowSectionHtml = new StringBuilder(strServiceLineItemRequesterDetailRowSectionHtml);
                    sbServiceLineItemRequesterDetailRowSectionHtml.Replace("##lblServiceLineItemRequesterName##", Convert.ToString(row.RequesterName));
                    sbServiceLineItemRequesterDetailRowSectionHtml.Replace("##lblServiceLineItemRequesterEmailId##", Convert.ToString(row.RequesterEmailId));
                    sbServiceLineItemRequesterDetailRowSectionHtml.Replace("##lblServiceLineItemRequesterPhone##", Convert.ToString(row.RequesterPhone));
                    sbServiceLineItemRequesterDetailRowSectionHtml.Replace("^^ServiceLineItemRequesterDetail^^", string.Empty);
                    sbServiceLineItemRequesterDetailRowSectionHtml.Replace("^^/ServiceLineItemRequesterDetail^^", string.Empty);
                    sbServiceLineItemRequesterDetailRowHtml.Append(sbServiceLineItemRequesterDetailRowSectionHtml);
                    strGroupText = row.RequesterName;
                }

                if (!string.IsNullOrWhiteSpace(strServiceLineItemRequesterDetailRowSectionHtml))
                    sbServiceLineItemRequesterDetailSectionHtml.Replace(strServiceLineItemRequesterDetailRowSectionHtml, sbServiceLineItemRequesterDetailRowHtml.ToString());
                sbServiceLineItemRequesterDetailSectionHtml.Replace("##ServiceLineItemRequesterDetailSection##", string.Empty);
                sbServiceLineItemRequesterDetailSectionHtml.Replace("##/ServiceLineItemRequesterDetailSection##", string.Empty);
                sbServiceLineItemRequesterDetailHtml.Append(sbServiceLineItemRequesterDetailSectionHtml);
            }
            else
            {
                strServiceLineItemRequesterDetailSectionHtml = GetSectionHtml(sbServiceLineItemHtml.ToString(), "##ServiceLineItemRequesterDetailSection##",
                                                               "##/ServiceLineItemRequesterDetailSection##");

            }
            if (strServiceLineItemRequesterDetailSectionHtml != null && strServiceLineItemRequesterDetailSectionHtml != string.Empty)
                sbServiceLineItemHtml.Replace(strServiceLineItemRequesterDetailSectionHtml, sbServiceLineItemRequesterDetailHtml.ToString());
        }


         

        /// <summary>
        /// In Order and IR at time of export this method will get call for exporting Service Line Item level comments including related documents.
        /// </summary>
        /// <param name="lstLineItemComments"></param>
        /// <param name="sbMaterialLineItemHtml"></param>
        public void FillServiceLineItemComments(ICollection<P2PDocumentDataSet.LineItemCommentRow> lstLineItemComments, StringBuilder sbMaterialLineItemHtml)
        {
            string strServiceLineItemGroupSectionHtml = string.Empty;
            string strServiceLineItemCommentsSectionHtml = string.Empty;
            var sbServiceLineItemGroupHtml = new StringBuilder();
            var sbServiceLineItemCommentsHtml = new StringBuilder();
            if (lstLineItemComments.Count > 0)
            {
                strServiceLineItemGroupSectionHtml = GetSectionHtml(sbMaterialLineItemHtml.ToString(), "^^ServiceLineItemGroup^^",
                                                   "^^/ServiceLineItemGroup^^");

                StringBuilder sbServiceLineItemGroupSectionHtml = new StringBuilder();

                string strGroupText = string.Empty;
                foreach (var row in lstLineItemComments)
                {
                    if (string.IsNullOrEmpty(strGroupText))
                    {
                        sbServiceLineItemGroupSectionHtml = new StringBuilder(strServiceLineItemGroupSectionHtml);
                        strServiceLineItemCommentsSectionHtml = GetSectionHtml(sbServiceLineItemGroupSectionHtml.ToString(), "^^ServiceLineItemComments^^",
                                                              "^^/ServiceLineItemComments^^");

                        sbServiceLineItemGroupSectionHtml.Replace("##lblServiceLineItemGroup##", row.GroupText);
                    }
                    else if (row.GroupText.Equals(strGroupText))
                    {

                        strServiceLineItemCommentsSectionHtml = GetSectionHtml(sbServiceLineItemGroupSectionHtml.ToString(), "^^ServiceLineItemComments^^",
                                                              "^^/ServiceLineItemComments^^");
                    }
                    else
                    {
                        if (strServiceLineItemCommentsSectionHtml != null && strServiceLineItemCommentsSectionHtml != string.Empty)
                            sbServiceLineItemGroupSectionHtml.Replace(strServiceLineItemCommentsSectionHtml, sbServiceLineItemCommentsHtml.ToString());
                        sbServiceLineItemGroupSectionHtml.Replace("^^ServiceLineItemGroup^^", string.Empty);
                        sbServiceLineItemGroupSectionHtml.Replace("^^/ServiceLineItemGroup^^", string.Empty);

                        sbServiceLineItemGroupHtml.Append(sbServiceLineItemGroupSectionHtml.ToString());

                        sbServiceLineItemGroupSectionHtml.Length = 0;
                        sbServiceLineItemGroupSectionHtml = new StringBuilder(strServiceLineItemGroupSectionHtml);

                        sbServiceLineItemGroupSectionHtml.Replace("##lblServiceLineItemGroup##", row.GroupText);
                        sbServiceLineItemCommentsHtml.Length = 0;
                        strServiceLineItemCommentsSectionHtml = GetSectionHtml(sbServiceLineItemGroupSectionHtml.ToString(), "^^ServiceLineItemComments^^",
                                                              "^^/ServiceLineItemComments^^");

                    }
                    StringBuilder sbServiceLineItemCommentsSectionHtml = new StringBuilder(strServiceLineItemCommentsSectionHtml);
                    sbServiceLineItemCommentsSectionHtml.Replace("##lblServiceLineItemUserName##", Convert.ToString(row.UserName));
                    sbServiceLineItemCommentsSectionHtml.Replace("##lblServiceLineItemDateAndTime##", GetDate(row.DateCreated, DateFormatType.commentdate));
                    sbServiceLineItemCommentsSectionHtml.Replace("##lblServiceLineItemVisibility##", GetVisibilityComments(row.Visibility));
                    if (row.CommentText != null && row.CommentText != string.Empty)
                    {
                        sbServiceLineItemCommentsSectionHtml.Replace("##lblServiceLineItemComment##", Convert.ToString(row.CommentText.Replace("\n", "<br>")));
                    }
                    else
                    {
                        sbServiceLineItemCommentsSectionHtml.Replace("##lblServiceLineItemComment##", Convert.ToString(row.CommentText));
                    }
                    sbServiceLineItemCommentsSectionHtml.Replace("^^ServiceLineItemComments^^", string.Empty);
                    sbServiceLineItemCommentsSectionHtml.Replace("^^/ServiceLineItemComments^^", string.Empty);
                    sbServiceLineItemCommentsHtml.Append(sbServiceLineItemCommentsSectionHtml);
                    strGroupText = row.GroupText;
                }
                if (strServiceLineItemCommentsSectionHtml != null && strServiceLineItemCommentsSectionHtml != string.Empty)
                    sbServiceLineItemGroupSectionHtml.Replace(strServiceLineItemCommentsSectionHtml, sbServiceLineItemCommentsHtml.ToString());
                sbServiceLineItemGroupSectionHtml.Replace("^^ServiceLineItemGroup^^", string.Empty);
                sbServiceLineItemGroupSectionHtml.Replace("^^/ServiceLineItemGroup^^", string.Empty);
                sbServiceLineItemGroupHtml.Append(sbServiceLineItemGroupSectionHtml.ToString());

                sbMaterialLineItemHtml.Replace("##ServiceLineItemCommentSection##", string.Empty);
                sbMaterialLineItemHtml.Replace("##/ServiceLineItemCommentSection##", string.Empty);

            }
            else
            {
                strServiceLineItemGroupSectionHtml = GetSectionHtml(sbMaterialLineItemHtml.ToString(), "##ServiceLineItemCommentSection##",
                                                               "##/ServiceLineItemCommentSection##");
            }
            if (strServiceLineItemGroupSectionHtml != null && strServiceLineItemGroupSectionHtml != string.Empty)
                sbMaterialLineItemHtml.Replace(strServiceLineItemGroupSectionHtml, sbServiceLineItemGroupHtml.ToString());
        }

        public void FillAdvanceLineItemComments(ICollection<P2PDocumentDataSet.LineItemCommentRow> lstLineItemComments, StringBuilder sbMaterialLineItemHtml)
        {
            string strAdvanceLineItemGroupSectionHtml = string.Empty;
            string strAdvanceLineItemCommentsSectionHtml = string.Empty;
            var sbAdvanceLineItemGroupHtml = new StringBuilder();
            var sbAdvanceLineItemCommentsHtml = new StringBuilder();
            if (lstLineItemComments.Count > 0)
            {
                strAdvanceLineItemGroupSectionHtml = GetSectionHtml(sbMaterialLineItemHtml.ToString(), "^^AdvanceLineItemGroup^^",
                                                   "^^/AdvanceLineItemGroup^^");

                StringBuilder sbAdvanceLineItemGroupSectionHtml = new StringBuilder();

                string strGroupText = string.Empty;
                foreach (var row in lstLineItemComments)
                {
                    if (string.IsNullOrEmpty(strGroupText))
                    {
                        sbAdvanceLineItemGroupSectionHtml = new StringBuilder(strAdvanceLineItemGroupSectionHtml);
                        strAdvanceLineItemCommentsSectionHtml = GetSectionHtml(sbAdvanceLineItemGroupSectionHtml.ToString(), "^^AdvanceLineItemComments^^",
                                                              "^^/AdvanceLineItemComments^^");

                        sbAdvanceLineItemGroupSectionHtml.Replace("##lblAdvanceLineItemGroup##", row.GroupText);
                    }
                    else if (row.GroupText.Equals(strGroupText))
                    {

                        strAdvanceLineItemCommentsSectionHtml = GetSectionHtml(sbAdvanceLineItemGroupSectionHtml.ToString(), "^^AdvanceLineItemComments^^",
                                                              "^^/AdvanceLineItemComments^^");
                    }
                    else
                    {
                        if (strAdvanceLineItemCommentsSectionHtml != null && strAdvanceLineItemCommentsSectionHtml != string.Empty)
                            sbAdvanceLineItemGroupSectionHtml.Replace(strAdvanceLineItemCommentsSectionHtml, sbAdvanceLineItemCommentsHtml.ToString());
                        sbAdvanceLineItemGroupSectionHtml.Replace("^^AdvanceLineItemGroup^^", string.Empty);
                        sbAdvanceLineItemGroupSectionHtml.Replace("^^/AdvanceLineItemGroup^^", string.Empty);

                        sbAdvanceLineItemGroupHtml.Append(sbAdvanceLineItemGroupSectionHtml.ToString());

                        sbAdvanceLineItemGroupSectionHtml.Length = 0;
                        sbAdvanceLineItemGroupSectionHtml = new StringBuilder(strAdvanceLineItemGroupSectionHtml);

                        sbAdvanceLineItemGroupSectionHtml.Replace("##lblAdvanceLineItemGroup##", row.GroupText);
                        sbAdvanceLineItemCommentsHtml.Length = 0;
                        strAdvanceLineItemCommentsSectionHtml = GetSectionHtml(sbAdvanceLineItemGroupSectionHtml.ToString(), "^^AdvanceLineItemComments^^",
                                                              "^^/AdvanceLineItemComments^^");

                    }
                    StringBuilder sbAdvanceLineItemCommentsSectionHtml = new StringBuilder(strAdvanceLineItemCommentsSectionHtml);
                    sbAdvanceLineItemCommentsSectionHtml.Replace("##lblAdvanceLineItemUserName##", Convert.ToString(row.UserName));
                    sbAdvanceLineItemCommentsSectionHtml.Replace("##lblAdvanceLineItemDateAndTime##", GetDate(row.DateCreated, DateFormatType.commentdate));
                    sbAdvanceLineItemCommentsSectionHtml.Replace("##lblAdvanceLineItemVisibility##", GetVisibilityComments(row.Visibility));
                    if (row.CommentText != null && row.CommentText != string.Empty)
                    {
                        sbAdvanceLineItemCommentsSectionHtml.Replace("##lblAdvanceLineItemComment##", Convert.ToString(row.CommentText.Replace("\n", "<br>")));
                    }
                    else
                    {
                        sbAdvanceLineItemCommentsSectionHtml.Replace("##lblAdvanceLineItemComment##", Convert.ToString(row.CommentText));
                    }
                    sbAdvanceLineItemCommentsSectionHtml.Replace("^^AdvanceLineItemComments^^", string.Empty);
                    sbAdvanceLineItemCommentsSectionHtml.Replace("^^/AdvanceLineItemComments^^", string.Empty);
                    sbAdvanceLineItemCommentsHtml.Append(sbAdvanceLineItemCommentsSectionHtml);
                    strGroupText = row.GroupText;
                }
                if (strAdvanceLineItemCommentsSectionHtml != null && strAdvanceLineItemCommentsSectionHtml != string.Empty)
                    sbAdvanceLineItemGroupSectionHtml.Replace(strAdvanceLineItemCommentsSectionHtml, sbAdvanceLineItemCommentsHtml.ToString());
                sbAdvanceLineItemGroupSectionHtml.Replace("^^AdvanceLineItemGroup^^", string.Empty);
                sbAdvanceLineItemGroupSectionHtml.Replace("^^/AdvanceLineItemGroup^^", string.Empty);
                sbAdvanceLineItemGroupHtml.Append(sbAdvanceLineItemGroupSectionHtml.ToString());

                sbMaterialLineItemHtml.Replace("##AdvanceLineItemCommentSection##", string.Empty);
                sbMaterialLineItemHtml.Replace("##/AdvanceLineItemCommentSection##", string.Empty);

            }
            else
            {
                strAdvanceLineItemGroupSectionHtml = GetSectionHtml(sbMaterialLineItemHtml.ToString(), "##AdvanceLineItemCommentSection##",
                                                               "##/AdvanceLineItemCommentSection##");
            }
            if (strAdvanceLineItemGroupSectionHtml != null && strAdvanceLineItemGroupSectionHtml != string.Empty)
                sbMaterialLineItemHtml.Replace(strAdvanceLineItemGroupSectionHtml, sbAdvanceLineItemGroupHtml.ToString());
        }

        public string GetBuyerContact(string Phone, string email)
        {
            var buyerContact = string.IsNullOrEmpty(email)
                                    ? string.Empty
                                    : "/ " + email.Trim();
            buyerContact += string.IsNullOrEmpty(Phone)
                                  ? string.Empty
                                  : "/ " + Phone.Trim();
            buyerContact.TrimEnd('/');
            if (buyerContact != string.Empty)
                buyerContact = buyerContact.Substring(1);
            return buyerContact;

        }

           

        public ExportTemplate GetHtmlTemplateToPDFConversion(Gep.Cumulus.CSM.Entities.DocumentType documentType, long documentCode, bool isNotificationBasedOnReceipientLanguage = false, bool isInterneralVersion = false, string InternalPrintLanguage = "en-US", long lobId = 0)
        {
            ExportTemplate result = null;
            DivisionMapping divisionMapping = null;

            RequisitionCommonManager commonManager = new RequisitionCommonManager (this.JWTToken){ UserContext = UserContext, GepConfiguration = GepConfiguration };

            string templateTypeSetting = commonManager.GetSettingsValueByKey(P2PDocumentType.None, "TemplateTypePreference", UserContext.UserId, (int)SubAppCodes.P2P, "", lobId);
            if (templateTypeSetting == null || templateTypeSetting == string.Empty)
                templateTypeSetting = "1";
            if (isNotificationBasedOnReceipientLanguage)
            {
                templateTypeSetting = "2";
            }
            if (isInterneralVersion)
            {
                templateTypeSetting = "3";
            }

            TemplateTypeReference templateTypeReference = (TemplateTypeReference)Enum.Parse(typeof(TemplateTypeReference), templateTypeSetting);

            if (documentType == Gep.Cumulus.CSM.Entities.DocumentType.Receipts ||
                documentType == Gep.Cumulus.CSM.Entities.DocumentType.ReturnNote ||
                documentType == Gep.Cumulus.CSM.Entities.DocumentType.InvoiceReconcillation ||
                documentType == Gep.Cumulus.CSM.Entities.DocumentType.ServiceConfirmation)
            {
                result = GetHtmlTemplateToPDFConversion(documentType, this.UserContext.Culture, TemplateTypeReference.ByUserLocalCulture.GetHashCode());
            }
            else if (documentType == Gep.Cumulus.CSM.Entities.DocumentType.PO || documentType == Gep.Cumulus.CSM.Entities.DocumentType.Invoice
                || documentType == Gep.Cumulus.CSM.Entities.DocumentType.Requisition
                || documentType == Gep.Cumulus.CSM.Entities.DocumentType.PaymentRequest)
            {
                switch (templateTypeReference)
                {
                    case TemplateTypeReference.ByUserLocalCulture:

                        //Load template by user culture code
                        result = GetHtmlTemplateToPDFConversion(documentType, this.UserContext.Culture, TemplateTypeReference.ByUserLocalCulture.GetHashCode());
                        break;

                    case TemplateTypeReference.BySupplierLocalCulture:

                        //Load template by supplier culture code  
                        List<long> partnerContactCode = GetCommonDao().GetPartnerContactCodeByDocumentCode(documentType.GetHashCode(), documentCode);

                        Proxy.ProxyPartnerService proxyPartnerService = new Proxy.ProxyPartnerService(UserContext, this.JWTToken);

                        Gep.Cumulus.Partner.Entities.Contact partner = proxyPartnerService.GetContactByContactCode(partnerContactCode[0], partnerContactCode[1]);

                        // Get division id, entity id, entity detail id.
                        divisionMapping = GetCommonDao().GetDivisionCodeEntityCodeEntityDetailMappingByDocumentCode(documentCode, documentType.GetHashCode());
                        if (divisionMapping.EntityId == null)
                        {
                            result = GetHtmlTemplateToPDFConversion(documentType, partner.ContactCultureCode, TemplateTypeReference.BySupplierLocalCulture.GetHashCode());
                        }
                        else
                        {
                            result = GetHtmlTemplateToPDFConversion(documentType, partner.ContactCultureCode, divisionMapping.DivisionEntityCode, divisionMapping.EntityId, divisionMapping.EntityDetailCode, TemplateTypeReference.ByDivision_EntityCode_EntityDetailCode.GetHashCode());
                        }
                        break;
                    case TemplateTypeReference.ByDivision_EntityCode_EntityDetailCode:

                        // Get division id, entity id, entity detail id.
                        divisionMapping = GetCommonDao().GetDivisionCodeEntityCodeEntityDetailMappingByDocumentCode(documentCode, documentType.GetHashCode());

                        if (isInterneralVersion && documentType == Gep.Cumulus.CSM.Entities.DocumentType.PO)
                        {
                            // print internal version for order                           
                            if (InternalPrintLanguage != "")
                            {
                                if (divisionMapping != null && divisionMapping.DivisionEntityCode > 0)
                                {
                                    result = GetHtmlTemplateToPDFConversion(documentType, InternalPrintLanguage, divisionMapping.DivisionEntityCode, divisionMapping.EntityId > 0 ? divisionMapping.EntityId : null, divisionMapping.EntityDetailCode > 0 ? divisionMapping.EntityDetailCode : null, TemplateTypeReference.ByDivision_EntityCode_EntityDetailCode.GetHashCode());
                                }
                                else
                                {
                                    result = GetHtmlTemplateToPDFConversion(documentType, InternalPrintLanguage, TemplateTypeReference.ByUserLocalCulture.GetHashCode());
                                }
                            }
                            else
                            {
                                result = GetHtmlTemplateToPDFConversion(documentType, this.UserContext.Culture, TemplateTypeReference.ByUserLocalCulture.GetHashCode());
                            }
                        }
                        else
                        {

                            if (divisionMapping.EntityId == null)
                            {
                                result = GetHtmlTemplateToPDFConversion(documentType, this.UserContext.Culture, TemplateTypeReference.ByUserLocalCulture.GetHashCode());
                            }
                            else
                            {
                                result = GetHtmlTemplateToPDFConversion(documentType, this.UserContext.Culture, divisionMapping.DivisionEntityCode, divisionMapping.EntityId, divisionMapping.EntityDetailCode, TemplateTypeReference.ByDivision_EntityCode_EntityDetailCode.GetHashCode());
                            }
                        }

                        break;

                    case TemplateTypeReference.ByDivision:

                        //Get DivisionId by orderId
                        divisionMapping = GetCommonDao().GetDivisionCodeEntityCodeEntityDetailMappingByDocumentCode(documentCode, documentType.GetHashCode());

                        result = GetHtmlTemplateToPDFConversion(documentType, this.UserContext.Culture, divisionMapping.DivisionEntityCode, null, null, TemplateTypeReference.ByDivision.GetHashCode());
                        break;

                    default:
                        break;
                }
            }
            //   result = UpdateTemplateFonts(result);

            return result;
        }

        /// <summary>
        /// Common method for get htmlTemplate
        /// </summary>
        /// <param name="documentType"></param>
        /// <param name="cultureCode"></param>
        /// <returns></returns>
        private ExportTemplate GetHtmlTemplateToPDFConversion(Gep.Cumulus.CSM.Entities.DocumentType documentType, string cultureCode, long divisionId, int? entityCode, long? entityDetailCode, int templateTypeReferenceId)
        {
            ExportTemplate template;

            try
            {
                template = GetCommonDao().GetDocumentExportTemplate(documentType.GetHashCode(), cultureCode, divisionId, entityCode, entityDetailCode, templateTypeReferenceId);

            }
            catch (Exception ex)
            {
                // Log Exception here
                LogHelper.LogError(Log, "Error occurred in GetHtmlTemplateToPDFConversion method in ExportManager.", ex);
                throw;
            }

            return template;
        }

        /// <summary>
        /// Common method for get htmlTemplate
        /// </summary>
        /// <param name="documentType"></param>
        /// <param name="cultureCode"></param>
        /// <returns></returns>
        private ExportTemplate GetHtmlTemplateToPDFConversion(Gep.Cumulus.CSM.Entities.DocumentType documentType, string cultureCode, int templateTypeReferenceId)
        {
            ExportTemplate template;

            try
            {
                template = GetCommonDao().GetDocumentExportTemplate(documentType.GetHashCode(), cultureCode, 0, null, null, templateTypeReferenceId);
            }
            catch (Exception ex)
            {
                // Log Exception here
                LogHelper.LogError(Log, "Error occurred in GetHtmlTemplateToPDFConversion method in ExportManager.", ex);
                throw;
            }
            return template;
        }
        //public static string GetInvoiceLineItemException(List<InvoiceDataSet.InvoiceLineItemExceptionRow> matchStatus)
        //{
        //    string exception = string.Empty;
        //    foreach (var matchStatusId in matchStatus)
        //    {
        //        if (matchStatusId.ExceptionTypeId == Convert.ToInt32(MatchStatusExceptionType.MatchStatus_ExceptionType_SystemDefined))
        //        {
        //            switch (Convert.ToInt32(matchStatusId.MatchStatus))
        //            {
        //                case 5:
        //                    exception += exception == string.Empty ? "Quantity/Efforts Exception" : ", Quantity/Efforts Exception";
        //                    break;
        //                case 9:
        //                    exception += exception == string.Empty ? "Unit Cost/Rate Exception" : ", Unit Cost/Rate Exception";
        //                    break;
        //                case 12:
        //                    exception += exception == string.Empty ? "Line Item Value Exception" : ", Line Item Value Exception";
        //                    break;
        //                case 15:
        //                    exception += exception == string.Empty ? "Item Total Exception" : ", Item Total Exception";
        //                    break;
        //                case 18:
        //                    exception += exception == string.Empty ? "Tax Exception" : ", Tax Exception";
        //                    break;
        //                case 21:
        //                    exception += exception == string.Empty ? "Shipping Exception" : ", Shipping Exception";
        //                    break;
        //                case 24:
        //                    exception += exception == string.Empty ? "Charges Exception" : ", Charges Exception";
        //                    break;
        //                case 27:
        //                    exception += exception == string.Empty ? "Total Amount Exception" : ", Total Amount Exception";
        //                    break;
        //                case 29:
        //                    exception += exception == string.Empty ? "UOM Exception" : ", UOM Exception";
        //                    break;
        //                case 38:
        //                    exception += exception == string.Empty ? "Non PO Exception" : ", Non PO Exception";
        //                    break;
        //                case 39:
        //                    exception += exception == string.Empty ? "Item mismatch Exception" : ", Item mismatch Exception";
        //                    break;
        //                case 52:
        //                    exception += exception == string.Empty ? "Advance Exception" : ", Advance Exception";
        //                    break;
        //                case 55:
        //                    exception += exception == string.Empty ? "Recoupment Exception" : ", Recoupment Exception";
        //                    break;
        //            }
        //        }
        //        else
        //        {
        //            exception += exception == string.Empty ? matchStatusId.Description.Trim() : ", " + matchStatusId.Description.Trim();
        //        }
        //    }
        //    exception.TrimEnd(',');
        //    return exception;
        //}

        public static string GetOrderType(int val)
        {
            switch (val)
            {
                case 0:
                    return "New Order";
                case 1:
                    return "New Order";
                case 2:
                    return "New Order";
                case 3:
                    return "Confirming Order";
                case 4:
                    return "New Order";
                case 5:
                    return "Change Order";
                case 6:
                    return "Change Request";
                case 7:
                    return "Flip To Order";
                case 8:
                    return "Blanket Order";
                case 9:
                    return "Release Order";
                default:
                    return "";
            }

        }

        public static string GetMatchType(int val)
        {
            switch (val)
            {
                case 0:
                    return "";
                case 1:
                    return "2 Way";
                case 2:
                    return "3 Way";
                case 3:
                    return "4 Way";
                default:
                    return "";
            }

        }

        public static string GetInventoryType(int val)
        {
            switch (val)
            {
                case 0:
                    return "Non Stock";
                case 1:
                    return "Stock";
                default:
                    return "";
            }

        }

        public static string GetProcurableType(int val)
        {
            switch (val)
            {
                case 0:
                    return "Procurable";
                case 1:
                    return "From Inventory";
                default:
                    return "";
            }

        }

        public static string GetLineStatus(int val)
        {

            switch (val)
            {

                case 1:
                    return "Draft";
                case 21:
                    return "Approval Pending";
                case 202:
                    return "Review Pending";
                case 169:
                    return "Send For Approval Failed";
                case 22:
                    return "Ready For Reservation";
                case 7:
                    return "Ready For Order";
                case 5:
                    return "Reservation Pending";
                case 6:
                    return "Stock Reserved";
                case 61:
                    return "Partially Ordered";
                case 62:
                    return "Ordered";
                case 23:
                    return "Rejected";
                case 24:
                    return "Withdrawn";
                case 56:
                    return "Sent For Bidding";
                default:
                    return "";
            }

        }


        public void FillStandardProcedureDetails(long orderId, StringBuilder sbHtml, long orderItemId, int level, P2PDocumentDataSet dssp, ItemType itemtype = ItemType.Material)
        {
            List<P2PDocumentDataSet.StandardsAndProceduresRow> lstFields = new List<P2PDocumentDataSet.StandardsAndProceduresRow>();
            string fieldTmpl = String.Empty;
            string fieldSection = String.Empty;
            string fieldItemTmpl = String.Empty;
            string fieldItemSection = String.Empty;
            StringBuilder fields = new StringBuilder();
            StringBuilder fieldInstance;

            fieldSection = GetSectionHtml(sbHtml.ToString(), "##lblStartSPDetails##", "##/lblStartSPDetails##");
            fieldTmpl = GetSectionHtml(sbHtml.ToString(), "##SPTmplForItem##", "##/SPTmplForItem##");

            if (itemtype == ItemType.Material)
            {
                fieldItemSection = GetSectionHtml(sbHtml.ToString(), "##MaterialLineSPDetail##", "##/MaterialLineSPDetail##");
                fieldItemTmpl = GetSectionHtml(sbHtml.ToString(), "##MaterialLineItemSP##", "##/MaterialLineItemSP##");
            }
            else
            {
                fieldItemSection = GetSectionHtml(sbHtml.ToString(), "##ServiceLineSPDetail##", "##/ServiceLineSPDetail##");
                fieldItemTmpl = GetSectionHtml(sbHtml.ToString(), "##ServiceLineItemSP##", "##/ServiceLineItemSP##");
            }

            

            if (level == 1)
            {

                lstFields = dssp.StandardsAndProcedures.Where(x => (x.DocumentCode == orderId.ToString() && x.LevelType == level.ToString())).ToList();
            }
            else
            {
                lstFields = dssp.StandardsAndProcedures.Where(x => (x.DocumentCode == orderId.ToString() && x.P2PItemId == orderItemId.ToString() && x.LevelType == level.ToString())).ToList();
            }

            int count = lstFields.Count;
            if (count > 0)
            {

                if (level == 1)
                {
                    for (int i = 0; i < count; i++)
                    {
                        fieldInstance = new StringBuilder(fieldTmpl);
                        fieldInstance.Replace("##lblCodeRev##", lstFields[i].Code + " / " + lstFields[i].RevisionNumber);
                        fieldInstance.Replace("##lblName##", lstFields[i].Name);
                        fieldInstance.Replace("##lblFullText##", lstFields[i].FullText);
                        fieldInstance.Replace("##SPTmplForItem##", string.Empty);
                        fieldInstance.Replace("##/SPTmplForItem##", string.Empty);
                        fields.Append(fieldInstance.ToString());
                    }
                    if (!string.IsNullOrWhiteSpace(fieldSection))
                        sbHtml.Replace(fieldSection, fields.ToString());
                    if (!string.IsNullOrWhiteSpace(fieldTmpl))
                        sbHtml.Replace(fieldTmpl, string.Empty);
                    sbHtml.Replace("##lblStartSPDetailsSection##", string.Empty);
                    sbHtml.Replace("##/lblStartSPDetailsSection##", string.Empty);
                }
                else
                {
                    if (itemtype == ItemType.Material)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            fieldInstance = new StringBuilder(fieldItemTmpl);
                            fieldInstance.Replace("##lblMaterialLineItemCodeRev##", lstFields[i].Code + " / " + lstFields[i].RevisionNumber);
                            fieldInstance.Replace("##lblMaterialLineItemName##", lstFields[i].Name);
                            fieldInstance.Replace("##lblMaterialLineItemFullText##", lstFields[i].FullText);
                            fieldInstance.Replace("##MaterialLineItemSP##", string.Empty);
                            fieldInstance.Replace("##/MaterialLineItemSP##", string.Empty);
                            fields.Append(fieldInstance.ToString());
                        }
                        sbHtml.Replace("##MaterialLineSPDetailSection##", string.Empty);
                        sbHtml.Replace("##/MaterialLineSPDetailSection##", string.Empty);
                    }
                    else
                    {
                        for (int i = 0; i < count; i++)
                        {
                            fieldInstance = new StringBuilder(fieldItemTmpl);
                            fieldInstance.Replace("##lblServiceLineItemCodeRev##", lstFields[i].Code + " / " + lstFields[i].RevisionNumber);
                            fieldInstance.Replace("##lblServiceLineItemName##", lstFields[i].Name);
                            fieldInstance.Replace("##lblServiceLineItemFullText##", lstFields[i].FullText);
                            fieldInstance.Replace("##ServiceLineItemSP##", string.Empty);
                            fieldInstance.Replace("##/ServiceLineItemSP##", string.Empty);
                            fields.Append(fieldInstance.ToString());
                        }
                        sbHtml.Replace("##ServiceLineSPDetailSection##", string.Empty);
                        sbHtml.Replace("##/ServiceLineSPDetailSection##", string.Empty);

                    }
                    if (!string.IsNullOrWhiteSpace(fieldItemSection))
                        sbHtml.Replace(fieldItemSection, fields.ToString());
                    if (!string.IsNullOrWhiteSpace(fieldItemTmpl))
                        sbHtml.Replace(fieldItemTmpl, string.Empty);

                }



            }
            else
            {
                if (level == 1)
                {
                    if (fieldTmpl != null && fieldTmpl != string.Empty)
                        sbHtml.Replace(fieldTmpl, string.Empty);
                    if (fieldSection != null && fieldSection != string.Empty)
                        sbHtml.Replace(fieldSection, string.Empty);

                    sbHtml.Replace("##lblStartSPDetails##", string.Empty);
                    sbHtml.Replace("##/lblStartSPDetails##", string.Empty);
                    sbHtml.Replace("##lblStartSPDetailsSection##", string.Empty);
                    sbHtml.Replace("##/lblStartSPDetailsSection##", string.Empty);
                }
                else
                {
                    if (fieldItemTmpl != null && fieldItemTmpl != string.Empty)
                        sbHtml.Replace(fieldItemTmpl, string.Empty);
                    if (fieldItemSection != null && fieldItemSection != string.Empty)
                        sbHtml.Replace(fieldItemSection, string.Empty);

                    sbHtml.Replace("##ServiceLineSPDetail##", string.Empty);
                    sbHtml.Replace("##/ServiceLineSPDetail##", string.Empty);
                    sbHtml.Replace("##MaterialLineSPDetail##", string.Empty);
                    sbHtml.Replace("##/MaterialLineSPDetail##", string.Empty);
                    sbHtml.Replace("##ServiceLineSPDetailSection##", string.Empty);
                    sbHtml.Replace("##/ServiceLineSPDetailSection##", string.Empty);
                    sbHtml.Replace("##MaterialLineSPDetailSection##", string.Empty);
                    sbHtml.Replace("##/MaterialLineSPDetailSection##", string.Empty);
                }
            }

        }
        public FileManagerEntities.FileDetails SaveExcelMemoryStreamToFile(byte[] fileData, string fileNamePrefix)
        {
            var objFiledetails = new FileManagerEntities.FileDetails();
            objFiledetails.CompanyName = UserContext.CompanyName;
            objFiledetails.CreatedBy = UserContext.ContactCode;
            objFiledetails.DateCreated = DateTime.Now;
            objFiledetails.FileContainerType = FileManagerEntities.FileEnums.FileContainerType.Applications;
            objFiledetails.FileContentType = ".xlsx";
            objFiledetails.FileData = fileData;
            DateTime date = DateTime.Now;
            fileNamePrefix = fileNamePrefix.Trim();
            objFiledetails.FileName = fileNamePrefix + ".xlsx";
            objFiledetails.FileSizeInBytes = fileData.Length;

            string tempBlobFileUri = FileOperations.UploadByteArraytoTemporaryBlobAsync(fileData,
                                                                                        MultiRegionConfig.GetConfig(CloudConfig.CloudStorageConn),
                                                                                        MultiRegionConfig.GetConfig(CloudConfig.TempFileUploadContainerName)).GetAwaiter().GetResult();

            var requestHeaders = new RequestHeaders();
            requestHeaders.Set(this.UserContext, this.JWTToken);
            string appName = Environment.GetEnvironmentVariable("NewRelic.AppName") ?? "Requisition";
            string useCase = "ExportManager-SaveExcelMemoryStreamToFile";
            var serviceURL = MultiRegionConfig.GetConfig(CloudConfig.APIMBaseURL) + "/FileManager/api/V2/FileManager/MoveFileToTargetBlob";

            var uploadFileToTargetBlobRequestModel = new FileManagerEntities.MoveFileToTargetBlobRequest()
            {
                FileName = fileNamePrefix + ".xlsx",
                FileContentType = FileExtenstion.Excelx.ToLower(),
                FileValidationSettings = new FileManagerEntities.MoveFileToTargetBlobFileValidationSettings()
                {
                    FileValidationSettingsScope = (int)FileManagerEntities.FileValidationSettingsScope.Global,
                    FileValidationContainerTypeId = (int)FileManagerEntities.FileEnums.FileContainerType.Applications,
                },
                TemporaryBlobFileUri = tempBlobFileUri
            };
            var webAPI = new WebAPI(requestHeaders, appName, useCase);
            var result = webAPI.ExecutePost(serviceURL, uploadFileToTargetBlobRequestModel);
            var fileUploadResponse = JsonConvert.DeserializeObject<FileManagerEntities.FileUploadResponseModel>(result);

            objFiledetails.FileId = fileUploadResponse.FileId;
            objFiledetails.FileName = fileUploadResponse.FileDisplayName;
            objFiledetails.FileExtension = fileUploadResponse.FileExtension;
            objFiledetails.FileSizeInBytes = fileUploadResponse.FileSizeInBytes;
            objFiledetails.FileCreatedBy = fileUploadResponse.FileCreatedBy;

            return objFiledetails;
        }

        private void BindAllItem(P2PDocumentDataSet.P2PItemRow row, P2PDocumentType docType, Export objExport, int usertype, string strLineItemSectionHtml, List<Question> lstquestion = null)
        {
            try
            {
                RequisitionCommonManager commonManager = new RequisitionCommonManager (this.JWTToken){ UserContext = UserContext, GepConfiguration = GepConfiguration };
                SettingDetails objSettings = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.None, 1, 107);
                var AllowOtherCharges = Convert.ToBoolean(commonManager.GetSettingsValueByKey(objSettings, "AllowOtherCharges"), CultureInfo.InvariantCulture);
                P2PDocumentDataSet dsDocument = objExport.DocumentDataSet;
                int sNO = srNo++;

                var sbLineItemSectionHtml = new StringBuilder(strLineItemSectionHtml);
                if (stgAllowLineNumber)
                {
                    sbLineItemSectionHtml.Replace("##lblLineItemSerialNo##", Convert.ToString(row.ItemLineNumber));
                }
                else
                {
                    sbLineItemSectionHtml.Replace("##lblLineItemSerialNo##", sNO.ToString(CultureInfo.InvariantCulture));
                }

                sbLineItemSectionHtml.Replace("##lblLineItemNumber##", Convert.ToString(row.ItemNumber));
                sbLineItemSectionHtml.Replace("##lblLineItemDescription##", row.Description);
                sbLineItemSectionHtml.Replace("##lblLineItemQuantity##", ConvertToStringForQuantity(Math.Round(row.Quantity, maxPrec)));
                sbLineItemSectionHtml.Replace("##lblOverallItemLimit##", ConvertToStringForQuantity(Math.Round(row.OverallItemLimit, maxPrec)));
                sbLineItemSectionHtml.Replace("##lblLineItemUOM##", row.UOMDesc);
                sbLineItemSectionHtml.Replace("##ItemExtendedType##", row.ItemExtendedType);

                if (sbLineItemSectionHtml.ToString().Contains("##lblAllowFlexiblePrice##"))
                    sbLineItemSectionHtml.Replace("##lblAllowFlexiblePrice##", (row.FlexiblePrice ? "Yes" : "No"));

                if (sbLineItemSectionHtml.ToString().Contains("##lblItemCurrencyCode##"))
                    sbLineItemSectionHtml.Replace("##lblItemCurrencyCode##", Convert.ToString(row.CurrencyCode));

                if (sbLineItemSectionHtml.ToString().Contains("##ItemDueDate##"))
                {
                    sbLineItemSectionHtml.Replace("##ItemDueDate##", row.ItemTypeID == 1 ? (string.IsNullOrWhiteSpace(row.DateNeeded) ? string.Empty : GetDate(row.DateNeeded, DateFormatType.CustomDateDateFirst)) : row.ItemTypeID == 2 ? (string.IsNullOrWhiteSpace(row.EndDate) ? string.Empty : GetDate(row.EndDate, DateFormatType.CustomDateDateFirst)) : string.Empty);
                }

                if (row.UnitPrice >= 0)
                    sbLineItemSectionHtml.Replace("##lblLineItemUnitPrice##", ConvertToString(Math.Round(row.UnitPrice, maxPrec)));
                else
                    sbLineItemSectionHtml.Replace("##lblLineItemUnitPrice##", ConvertToNegativeString(Math.Round(row.UnitPrice, maxPrec)));

                if (row.Total >= 0)
                    sbLineItemSectionHtml.Replace("##lblLineItemTotal##", ConvertToString(Math.Round((row.Total), maxPrecTot, MidpointRounding.AwayFromZero)));
                else
                    sbLineItemSectionHtml.Replace("##lblLineItemTotal##", ConvertToNegativeString(Math.Round((row.Total), maxPrecTot, MidpointRounding.AwayFromZero)));

                sbLineItemSectionHtml.Replace("##lblLineItemCategory##", BaseDAO.HtmlEncode(Convert.ToString(row.CategoryName)));
                if (sbLineItemSectionHtml.ToString().Contains("##lblLineItemDispatchMode##") && Convert.ToString(row.EnableDispatchMode).ToUpper() == "TRUE")
                {
                    sbLineItemSectionHtml.Replace("##lblLineItemDispatchMode##", BaseDAO.HtmlEncode(Convert.ToString(row.DispatchMode)));
                }
                else
                {
                    string strLineLevelCustomFieldHtml = string.Empty;
                    strLineLevelCustomFieldHtml = GetSectionHtml(sbLineItemSectionHtml.ToString(), "##LineLevelCustomField##", "##/LineLevelCustomField##");
                    if (strLineLevelCustomFieldHtml != null && strLineLevelCustomFieldHtml != string.Empty)
                        sbLineItemSectionHtml.Replace(strLineLevelCustomFieldHtml, string.Empty);
                }
                if (sbLineItemSectionHtml.ToString().Contains("##lblLineItemMatchType##"))
                    sbLineItemSectionHtml.Replace("##lblLineItemMatchType##", GetMatchType(row.MatchType));

                if (row.Tax >= 0)
                    sbLineItemSectionHtml.Replace("##lblLineItemTaxes##", ConvertToString(Math.Round(row.Tax, maxPrecTax)));
                else
                    sbLineItemSectionHtml.Replace("##lblLineItemTaxes##", ConvertToNegativeString(Math.Round(row.Tax, maxPrecTax)));

                sbLineItemSectionHtml.Replace("##lblLineItemTaxExempt##", row.TaxExempt);
                if (row.HasFlexibleCharges != 1)
                {
                    sbLineItemSectionHtml.Replace("##lblLineItemShippingCharges##", ConvertToString(Math.Round(row.ShippingCharges, maxPrecTax)));
                    if (AllowOtherCharges)
                        sbLineItemSectionHtml.Replace("##lblLineItemAdditional##", ConvertToString(Math.Round(row.AdditionalCharges, maxPrecTax)));
                    else
                    {
                        sbLineItemSectionHtml.Replace("##lblLineItemAdditional##", "");
                    }
                    var hiddenFlexible = GetSectionHtml(sbLineItemSectionHtml.ToString(), "##LineFlexibleChargeSection##", "##/LineFlexibleChargeSection##");
                    if (!string.IsNullOrWhiteSpace(hiddenFlexible))
                        sbLineItemSectionHtml.Replace(hiddenFlexible, string.Empty);
                    var hiddenFlexibleProperty = GetSectionHtml(sbLineItemSectionHtml.ToString(), "##LineFlexibleChargePropertySection##", "##/LineFlexibleChargePropertySection##");
                    if (!string.IsNullOrWhiteSpace(hiddenFlexibleProperty))
                        sbLineItemSectionHtml.Replace(hiddenFlexibleProperty, string.Empty);

                }
                else
                {
                    if (row.AdditionalCharges >= 0)
                    {
                        sbLineItemSectionHtml.Replace("##lblLineItemTotalCharges##", ConvertToString(Math.Round(row.AdditionalCharges, maxPrecTax)));
                    }
                    else
                    {
                        sbLineItemSectionHtml.Replace("##lblLineItemTotalCharges##", ConvertToNegativeString(Math.Round(row.AdditionalCharges, maxPrecTax)));
                    }
                    var hiddenDefault = GetSectionHtml(sbLineItemSectionHtml.ToString(), "##LineDefaultSection##", "##/LineDefaultSection##");
                    if (!string.IsNullOrWhiteSpace(hiddenDefault))
                        sbLineItemSectionHtml.Replace(hiddenDefault, string.Empty);
                    var hiddenDefaultProperty = GetSectionHtml(sbLineItemSectionHtml.ToString(), "##LineDefaultPropertySection##", "##/LineDefaultPropertySection##");
                    if (!string.IsNullOrWhiteSpace(hiddenDefaultProperty))
                        sbLineItemSectionHtml.Replace(hiddenDefaultProperty, string.Empty);
                }
                sbLineItemSectionHtml.Replace("##LineDefaultSection##", string.Empty);
                sbLineItemSectionHtml.Replace("##/LineDefaultSection##", string.Empty);
                sbLineItemSectionHtml.Replace("##LineFlexibleChargeSection##", string.Empty);
                sbLineItemSectionHtml.Replace("##/LineFlexibleChargeSection##", string.Empty);
                sbLineItemSectionHtml.Replace("##LineDefaultPropertySection##", string.Empty);
                sbLineItemSectionHtml.Replace("##/LineDefaultPropertySection##", string.Empty);
                sbLineItemSectionHtml.Replace("##LineFlexibleChargePropertySection##", string.Empty);
                sbLineItemSectionHtml.Replace("##/LineFlexibleChargePropertySection##", string.Empty);
                sbLineItemSectionHtml.Replace("##lblLineItemRequestedDate##", string.IsNullOrWhiteSpace(row.DateRequested) ? string.Empty : GetDate(row.DateRequested, DateFormatType.DefaultDate));
                sbLineItemSectionHtml.Replace("##lblLineItemNeedByDate##", string.IsNullOrWhiteSpace(row.DateNeeded) ? string.Empty : GetDate(row.DateNeeded, DateFormatType.DefaultDate));
                sbLineItemSectionHtml.Replace("##lblLinePartnerItemNumber##", BaseDAO.HtmlEncode(Convert.ToString(row.PartnerItemNumber)));
                if (sbLineItemSectionHtml.ToString().Contains("##lblLineItemContractNo##"))
                    sbLineItemSectionHtml.Replace("##lblLineItemContractNo##", BaseDAO.HtmlEncode(Convert.ToString(row.ContractNo)));
                if (sbLineItemSectionHtml.ToString().Contains("##lblLineItemPartnerName##"))
                    sbLineItemSectionHtml.Replace("##lblLineItemPartnerName##", BaseDAO.HtmlEncode(Convert.ToString(row.PartnerName)));

                if (sbLineItemSectionHtml.ToString().Contains("##lblLineItemRequisitionNumber##"))
                    sbLineItemSectionHtml.Replace("##lblLineItemRequisitionNumber##", (row.RequisitionNumber == null) ? string.Empty : BaseDAO.HtmlEncode(Convert.ToString(row.RequisitionNumber)));

                if (sbLineItemSectionHtml.ToString().Contains("##lblLineItemShipFromLocationSection##"))
                    sbLineItemSectionHtml.Replace("##lblLineItemShipFromLocationSection##", BaseDAO.HtmlEncode(Convert.ToString(row.ShipFromLocation)));

                if (sbLineItemSectionHtml.ToString().Contains("##lblLineItemShipFromAddressSection##"))
                    sbLineItemSectionHtml.Replace("##lblLineItemShipFromAddressSection##", BaseDAO.HtmlEncode(Convert.ToString(row.ShipFromAddress)));

                if (sbLineItemSectionHtml.ToString().Contains("##lblLineItemBillable##") || sbLineItemSectionHtml.ToString().Contains("##THBillableCommentSection##"))
                {
                    if (stgBillableFieldAvailable == "TRUE")
                        sbLineItemSectionHtml.Replace("##lblLineItemBillable##", (Convert.ToString(row.Billable)).ToLower() == "billable" ? "Yes" : "No");
                    else
                    {
                        var billableH = GetSectionHtml(sbLineItemSectionHtml.ToString(), "##THBillableCommentSection##", "##/THBillableCommentSection##");
                        if (billableH != null && billableH != string.Empty)
                            sbLineItemSectionHtml.Replace(billableH, string.Empty);

                        var billableT = GetSectionHtml(sbLineItemSectionHtml.ToString(), "##TDBillableCommentSection##", "##/TDBillableCommentSection##");
                        if (billableT != null && billableT != string.Empty)
                            sbLineItemSectionHtml.Replace(billableT, string.Empty);

                    }
                    sbLineItemSectionHtml.Replace("##THBillableCommentSection##", string.Empty);
                    sbLineItemSectionHtml.Replace("##/THBillableCommentSection##", string.Empty);
                    sbLineItemSectionHtml.Replace("##TDBillableCommentSection##", string.Empty);
                    sbLineItemSectionHtml.Replace("##/TDBillableCommentSection##", string.Empty);
                }
                if (sbLineItemSectionHtml.ToString().Contains("##lblLineItemCapitalized##"))
                    sbLineItemSectionHtml.Replace("##lblLineItemCapitalized##", Convert.ToString(row.Capitalized));
                if (sbLineItemSectionHtml.ToString().Contains("##CommonLineItemShippingDetailSection##"))
                {
                    FillCommonLineItemShippingDetails(row, sbLineItemSectionHtml);
                }
                else
                if (row.ItemTypeID == 1 && sbLineItemSectionHtml.ToString().Contains("##LineItemShippingDetailSection##"))
                {
                    FillMaterialLineItemShippingDetails(row, sbLineItemSectionHtml);
                }
                if (row.ItemTypeID == 1 && sbLineItemSectionHtml.ToString().Contains("##LineItemManufacturerDetailSection##") && (docType == P2PDocumentType.Order || docType == P2PDocumentType.Receipt))
                {
                    FillMaterialLineItemManufacturerDetails(row, sbLineItemSectionHtml, stgAllowManufacturersCodeAndModel);
                }
                else
                {
                    if (sbLineItemSectionHtml.ToString().Contains("##lblLineItemManufacturerPartNumber##"))
                        sbLineItemSectionHtml.Replace("##lblLineItemManufacturerPartNumber##", Convert.ToString(row.ManufacturerPartNumber));
                }
                //if (sbLineItemSectionHtml.ToString().Contains("##LineItemExceptionSection##") && docType == P2PDocumentType.InvoiceReconciliation)
                //{
                //    ExportIR objExportIR = (ExportIR)objExport;
                //    FillMaterialLineItemExceptions(objExportIR.IRDataSet.IRLineItemException.Where(t => t.IRItemId == row.DocumentItemId).ToList(),
                //    sbLineItemSectionHtml);
                //}
                if (docType == P2PDocumentType.Requisition)
                {
                    FillOrderStatusDetails(row, sbLineItemSectionHtml, dsDocument.P2PDocument[0].DocumentStatusInfo);
                }
                sbLineItemSectionHtml.Replace("^^LineItem^^", string.Empty);
                sbLineItemSectionHtml.Replace("^^/LineItem^^", string.Empty);
                if (sbLineItemSectionHtml.ToString().Contains("^^LineItemGroup^^"))
                    FillMaterialLineItemComments(
                        dsDocument.LineItemComment.Where(t => t.P2PLineItemId == row.P2PLineItemId).ToList(),
                        sbLineItemSectionHtml);
                if (sbLineItemSectionHtml.ToString().Contains("^^LineItemRequesterDetail^^"))
                    FillMaterialLineItemRequesterDetails(
                        dsDocument.RequesterDetails.Where(t => t.DocumentItemId == row.DocumentItemId).ToList(),
                        sbLineItemSectionHtml);
                long itm = docType == P2PDocumentType.InvoiceReconciliation ? row.InvoiceItemId : row.DocumentItemId;
                FillCustomFields(itm, Convert.ToInt64(row.FormID), sbLineItemSectionHtml, "Item", lstquestion, true);
                FillAccountingFields(dsDocument.AccountingDetails.Where(x => x.DocumentItemID == row.DocumentItemId).ToList(), sbLineItemSectionHtml, row.ItemTypeID);
                if (stgShowStanProcDetails && dsDocument.P2PDocument[0].DocumentSourceTypeInfo != 6)
                {
                    FillStandardProcedureDetails(Convert.ToInt64(dsDocument.P2PDocument[0].DocumentCode), sbLineItemSectionHtml, Convert.ToInt64(row.DocumentItemId), 2, dsDocument);
                }
                else
                {
                    var showStanProcDetailsText = GetSectionHtml(sbLineItemSectionHtml.ToString(), "##LineSPDetailSection##", "##/LineSPDetailSection##");
                    if (showStanProcDetailsText != null && showStanProcDetailsText != string.Empty)
                        sbLineItemSectionHtml.Replace(showStanProcDetailsText, string.Empty);
                }

                if (docType == P2PDocumentType.Requisition)
                {
                    FillStockReservationDetails(sbLineItemSectionHtml, objSettings, "", row);
                }

                if (docType == P2PDocumentType.Receipt || docType == P2PDocumentType.ReturnNote)
                {
                    if (sbLineItemSectionHtml.ToString().Contains("##MultipleOrderDetailSection##"))
                        FillReceiptItemOrderDetails(row, sbLineItemSectionHtml);
                }
                
                long num = stgAllowLineNumber ? row.ItemLineNumber : sNO;
                tempList.Add(new KeyValuePair<long, StringBuilder>(num, sbLineItemSectionHtml));
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occurred in BindAllItem method in ExportManager for P2PLineItemId = " + row.P2PLineItemId, ex);
                throw;
            }
        }

        public void FillRequisitionItemAdditionalFields(Export objExport, long itemId, StringBuilder sbhtml)
        {
            RequisitionCommonManager commonManager = new RequisitionCommonManager (this.JWTToken){ UserContext = UserContext, GepConfiguration = GepConfiguration };
            SettingDetails objSettings = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.None, 1, 107);
            string showAdditionalAttributes = commonManager.GetSettingsValueByKey(objSettings, "ShowAdditionalAttributes");
            bool isShowAdditionalAttributes = string.IsNullOrEmpty(showAdditionalAttributes) ? false : Convert.ToBoolean(showAdditionalAttributes);
            string reqAdditionalFieldSection = GetSectionHtml(sbhtml.ToString(), "##AdditionalFieldSection##", "##/AdditionalFieldSection##");
            string reqAdditionalFieldItemSection = GetSectionHtml(sbhtml.ToString(), "##AdditionalFieldItemSection##", "##/AdditionalFieldItemSection##");

            if (isShowAdditionalAttributes && !(string.IsNullOrEmpty(reqAdditionalFieldSection) && string.IsNullOrEmpty(reqAdditionalFieldSection)))
            {
                P2PDocumentDataSet dsDocument = objExport.DocumentDataSet;
                List<DataRow> reqAdditionalFieldAtrributesTableRows = null;

                if (dsDocument.Tables["RequisitionItemAdditionalFieldDetails"].Rows.Count > 0)
                {
                    reqAdditionalFieldAtrributesTableRows = dsDocument.Tables["RequisitionItemAdditionalFieldDetails"].Select().Any() && dsDocument.Tables["RequisitionItemAdditionalFieldDetails"].Select().Where(x => x.Field<long>("RequisitionItemID") == itemId).Any() ? dsDocument.Tables["RequisitionItemAdditionalFieldDetails"].Select().Where(x => x.Field<long>("RequisitionItemID") == itemId).ToList() : null;
                }

                if (reqAdditionalFieldAtrributesTableRows != null && reqAdditionalFieldAtrributesTableRows.Count > 0)
                {
                    List<REQAdditionalAttribute> reqAdditionalFieldAttributesData = new List<REQAdditionalAttribute>();
                    StringBuilder reqAdditionalFieldItemSectionBuilder = new StringBuilder(reqAdditionalFieldItemSection);
                    StringBuilder appendReqAdditionalFieldItemSectionBuilder = new StringBuilder(string.Empty);
                    foreach (DataRow dataRow in reqAdditionalFieldAtrributesTableRows)
                    {
                        REQAdditionalAttribute reqAdditionalField = new REQAdditionalAttribute();
                        reqAdditionalField.AdditionalFieldCode = Convert.ToString(dataRow["AdditionalFieldCode"]);
                        reqAdditionalField.AdditionalFieldDetailCode = Convert.ToInt64(dataRow["AdditionalFieldDetailCode"]);
                        reqAdditionalField.AdditionalFieldId = Convert.ToInt32(dataRow["AdditionalFieldID"]);
                        reqAdditionalField.AdditionalFieldDisplayName = Convert.ToString(dataRow["AdditionalFieldDisplayName"]);
                        reqAdditionalField.AdditionalFieldDisplayValue = Convert.ToString(dataRow["AdditionalFieldValue"]);
                        reqAdditionalField.FeatureId = Convert.ToInt32(dataRow["FeatureId"]);
                        reqAdditionalField.DataDisplayStyle = Convert.ToByte(dataRow["DataDisplayStyle"]);
                        reqAdditionalField.FieldControlType = Convert.ToByte(dataRow["FieldControlType"]);
                        reqAdditionalFieldAttributesData.Add(reqAdditionalField);
                    }
                    foreach (REQAdditionalAttribute reqAdditionalFieldAtrribute in reqAdditionalFieldAttributesData)
                    {
                        reqAdditionalFieldItemSectionBuilder.Replace("##ItemAdditionalFieldDisplayName##", reqAdditionalFieldAtrribute.AdditionalFieldDisplayName);
                        if (reqAdditionalFieldAtrribute.FieldControlType == 3)
                        {
                            switch (reqAdditionalFieldAtrribute.DataDisplayStyle)
                            {
                                case 1:
                                    string codeValueFormat = $"{ reqAdditionalFieldAtrribute.AdditionalFieldCode }-{ reqAdditionalFieldAtrribute.AdditionalFieldDisplayValue}";
                                    if (reqAdditionalFieldAtrribute.AdditionalFieldCode == string.Empty && reqAdditionalFieldAtrribute.AdditionalFieldDisplayValue == string.Empty)
                                        codeValueFormat = string.Empty;
                                    reqAdditionalFieldItemSectionBuilder.Replace("##ItemAdditionalFieldDisplayValue##", codeValueFormat);
                                    break;
                                case 2:
                                    string valueCodeFormat = $"{ reqAdditionalFieldAtrribute.AdditionalFieldDisplayValue }-{ reqAdditionalFieldAtrribute.AdditionalFieldCode}";
                                    if (reqAdditionalFieldAtrribute.AdditionalFieldCode == string.Empty && reqAdditionalFieldAtrribute.AdditionalFieldDisplayValue == string.Empty)
                                        valueCodeFormat = string.Empty;
                                    reqAdditionalFieldItemSectionBuilder.Replace("##ItemAdditionalFieldDisplayValue##", valueCodeFormat);
                                    break;
                                case 3:
                                    reqAdditionalFieldItemSectionBuilder.Replace("##ItemAdditionalFieldDisplayValue##", reqAdditionalFieldAtrribute.AdditionalFieldDisplayValue);
                                    break;
                                default:
                                    reqAdditionalFieldItemSectionBuilder.Replace("##ItemAdditionalFieldDisplayValue##", reqAdditionalFieldAtrribute.AdditionalFieldCode);
                                    break;
                            }
                        }
                        else
                        {
                            reqAdditionalFieldItemSectionBuilder.Replace("##ItemAdditionalFieldDisplayValue##", reqAdditionalFieldAtrribute.AdditionalFieldDisplayValue);
                        }

                        appendReqAdditionalFieldItemSectionBuilder.Append(reqAdditionalFieldItemSectionBuilder);

                        reqAdditionalFieldItemSectionBuilder = new StringBuilder(reqAdditionalFieldItemSection);
                    }

                    sbhtml.Replace(reqAdditionalFieldItemSection, appendReqAdditionalFieldItemSectionBuilder.ToString());
                    sbhtml.Replace("##AdditionalFieldItemSection##", string.Empty);
                    sbhtml.Replace("##/AdditionalFieldItemSection##", string.Empty);
                    sbhtml.Replace("##AdditionalFieldSection##", string.Empty);
                    sbhtml.Replace("##/AdditionalFieldSection##", string.Empty);

                }
            }
            if (!string.IsNullOrEmpty(reqAdditionalFieldSection))
            {
                sbhtml.Replace(reqAdditionalFieldSection, string.Empty);
            }
            if (!string.IsNullOrEmpty(reqAdditionalFieldItemSection))
            {
                sbhtml.Replace(reqAdditionalFieldItemSection, string.Empty);
            }
        }
        public void FillOrderStatusDetails(P2PDocumentDataSet.P2PItemRow objItem, StringBuilder sbHtml, int DocumentStatus)
        {
            string strOrderStatusSectionHtml = string.Empty;
            var sbOrderStatusHtml = new StringBuilder();
            if (DocumentStatus != 1 && DocumentStatus != 23 && DocumentStatus != 24 && DocumentStatus != 121)
            {
                // Calculations
                var OrderingStatus = string.Empty;

                if (objItem.OrderedQuantity > 0)
                {
                    if ((objItem.ItemTypeID == 1) || (Convert.ToString(objItem.ItemExtendedType).Trim().ToUpper() == Enum.GetName(typeof(ItemExtendedType), ItemExtendedType.Variable).Trim().ToUpper()) || (objItem.ItemTypeID == 3))
                    {
                        OrderingStatus = objItem.OrderedQuantity >= objItem.Quantity ? objItem.Quantity.ToString() : objItem.OrderedQuantity.ToString();
                    }
                    else
                    {
                        OrderingStatus = objItem.OrderedUnitPrice >= objItem.UnitPrice ? objItem.UnitPrice.ToString() : objItem.OrderedUnitPrice.ToString();
                    }
                }
                else
                {
                    OrderingStatus = "0";
                }
                strOrderStatusSectionHtml = GetSectionHtml(sbHtml.ToString(), "##OrderingStatusSection##",
                                                               "##/OrderingStatusSection##");
                StringBuilder sbOrderStatusSectionHtml = new StringBuilder(strOrderStatusSectionHtml);

                sbOrderStatusSectionHtml.Replace("##lblLineOrderingStatus##", OrderingStatus);
                sbOrderStatusHtml.Append(sbOrderStatusSectionHtml.ToString());
                //end
            }
            else
            {
                strOrderStatusSectionHtml = GetSectionHtml(sbHtml.ToString(), "##OrderingStatusSection##",
                                                               "##/OrderingStatusSection##");
            }
            if (strOrderStatusSectionHtml != null && strOrderStatusSectionHtml != string.Empty)
                sbHtml.Replace(strOrderStatusSectionHtml, sbOrderStatusHtml.ToString());
            sbHtml.Replace("^^OrderingStatusSection^^", string.Empty);
            sbHtml.Replace("^^/OrderingStatusSection^^", string.Empty);
            sbHtml.Replace("##OrderingStatusSection##", string.Empty);
            sbHtml.Replace("##/OrderingStatusSection##", string.Empty);
        }

        private void LogNewRelic(string message, string method)
        {
            var eventAttributes = new Dictionary<string, object>();
            eventAttributes.Add("message", message);
            eventAttributes.Add("method", method);
            NewRelic.Api.Agent.NewRelic.RecordCustomEvent("ExportManager", eventAttributes);
        }
    }

    public class ImageDocument
    {
        public int DocId { get; set; }
        public string DocName { get; set; }
        public string MIMEType { get; set; }
        public byte[] DocContent { get; set; }

        private static readonly byte[] BMP = { 66, 77 };

        private static readonly byte[] GIF = { 71, 73, 70, 56 };

        private static readonly byte[] ICO = { 0, 0, 1, 0 };

        private static readonly byte[] JPG = { 255, 216, 255 };

        private static readonly byte[] PNG = { 137, 80, 78, 71, 13, 10, 26, 10, 0, 0, 0, 13, 73, 72, 68, 82 };

        private static readonly byte[] TIFF = { 73, 73, 42, 0 };

        public static string GetMimeType(byte[] file, string fileName)
        {
            string mime = "application/octet-stream";
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return mime;
            }
            string extension = Path.GetExtension(fileName) == null
                                   ? string.Empty
                                   : Path.GetExtension(fileName).ToUpper();

            if (file.Take(2).SequenceEqual(BMP))
            {
                mime = "image/bmp";
            }
            else if (file.Take(4).SequenceEqual(GIF))
            {
                mime = "image/gif";
            }
            else if (file.Take(4).SequenceEqual(ICO))
            {
                mime = "image/x-icon";
            }
            else if (file.Take(3).SequenceEqual(JPG))
            {
                mime = "image/jpeg";
            }
            else if (file.Take(16).SequenceEqual(PNG))
            {
                mime = "image/png";
            }
            else if (file.Take(4).SequenceEqual(TIFF))
            {
                mime = "image/tiff";
            }
            return mime;
        }
    }
}
