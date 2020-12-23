using Gep.Cumulus.CSM.Config;
using Gep.Cumulus.CSM.Entities;
using GEP.Cumulus.Impex.Manager;
using GEP.Cumulus.Logging;
using GEP.Cumulus.P2P.BusinessEntities;
using GEP.Cumulus.P2P.BusinessEntities.ExportDataSetEntities;
using GEP.Cumulus.P2P.Req.BusinessObjects.Entities;
using GEP.Cumulus.P2P.Req.DataAccessObjects;
using GEP.Cumulus.QuestionBank.Entities;
using GEP.SMART.Configuration;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using FileManagerEntities = GEP.NewP2PEntities.FileManagerEntities;

namespace GEP.Cumulus.P2P.Req.BusinessObjects
{
    public class RequisitionExportManager : ExportManager
    {
        private string strDocumentNumber = string.Empty;
        private static UserExecutionContext _userExecutionContext;
        private static GepConfig _gepConfig;
        private static readonly ILog Log = Logger.GetLog(MethodBase.GetCurrentMethod().DeclaringType);

        public RequisitionExportManager(string jwtToken, UserExecutionContext context = null) : base(jwtToken)
        {
            if (context != null)
            {
                this.UserContext = context;
            }
        }

        private IRequisitionDAO GetReqDao()
        {
            return ReqDAOManager.GetDAO<IRequisitionDAO>(UserContext, GepConfiguration, false);
        }

        private string FillRequisitionTemplate(ExportRequisition objExportRequisition, string strTemplate, bool isForPrint)
        {
            if (objExportRequisition == null) throw new ArgumentNullException("dsRequisition");
            if (strTemplate == null) throw new ArgumentNullException("strTemplatePath");
            Proxy.ProxyPartnerService proxyPartnerService = new Proxy.ProxyPartnerService(UserContext, this.JWTToken);
            this.UserContact = proxyPartnerService.GetContactByContactCode(0, UserContext.ContactCode);

            StringBuilder sbHtmlTemplate = new StringBuilder(strTemplate);
            Export objExport = new Export();
            objExport.DocumentDataSet = objExportRequisition.DocumentDataSet;

            strDocumentNumber = objExport.DocumentDataSet.P2PDocument[0].DocumentNumber;
            P2PDocumentDataSet.P2PItemDataTable p2PItem = objExport.DocumentDataSet.P2PItem;
            decimal totalBasecurrency = 0;
            if (p2PItem.Count > 0)
            {
                foreach (var item in p2PItem)
                {
                    totalBasecurrency = totalBasecurrency + item.TotalBaseItemPrice;
                }
            }
            FillHeaderDetails(objExport, sbHtmlTemplate, P2P.BusinessEntities.P2PDocumentType.Requisition, totalBasecurrency);
            FillRequisitionHeaderDetails(objExport, sbHtmlTemplate);
            FillHeaderComments(objExport, sbHtmlTemplate);
            LogNewRelic("Inside FillRequisitionTemplate at LN - 65", "FillRequisitionTemplate-before-FillCustomFields");
            FillCustomFields(Convert.ToInt64(objExportRequisition.DocumentDataSet.P2PDocument[0].DocumentCode), Convert.ToInt64(objExportRequisition.DocumentDataSet.P2PDocument[0].FormID), sbHtmlTemplate, "Header");
            LogNewRelic("Inside FillRequisitionTemplate at LN - 67", "FillRequisitionTemplate-after-FillCustomFields");
            //FillMaterialLineItemDetails(objExportRequisition, sbHtmlTemplate, P2PDocumentType.Requisition);
            //FillServiceLineItemDetails(objExportRequisition, sbHtmlTemplate, P2PDocumentType.Requisition);
            LogNewRelic("Inside FillRequisitionTemplate at LN - 70", "FillRequisitionTemplate-before-FillLineItemDetails");
            FillLineItemDetails(objExportRequisition, sbHtmlTemplate, P2PDocumentType.Requisition);
            LogNewRelic("Inside FillRequisitionTemplate at LN - 72", "FillRequisitionTemplate-after-FillLineItemDetails");
            return sbHtmlTemplate.ToString();
        }

        private void FillRequisitionHeaderDetails(Export objExport, StringBuilder sbRequisitionHtml)
        {
            P2PDocumentDataSet ds = objExport.DocumentDataSet;
            if (ds.P2PDocument.Rows.Count > 0 && ds.P2PDocument[0] != null)
            {
                TimeZoneInfo timeZoneInfo = null;
                Proxy.ProxyPartnerService proxyPartnerService = new Proxy.ProxyPartnerService(UserContext, this.JWTToken);
                var contactInformationCode = proxyPartnerService.GetContactInformationByContactCode(this.UserContext.BuyerPartnerCode, this.UserContext.ContactCode);
                if (contactInformationCode != null)
                {
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(contactInformationCode.contact.UserInfo.TimeZone);
                }

                if (sbRequisitionHtml.ToString().Contains("##lblDateCreated##"))
                {
                    sbRequisitionHtml.Replace("##lblDateCreated##", GetDate(ds.P2PDocument[0].DateCreated, DateFormatType.formattedUtcDate, timeZoneInfo));
                }
            }
        }

        public byte[] GetRequisitionHtml(byte[] htmlTemplate, long RequisitionId, bool isForPrint, long contactCode, int userType, string accessType)
        {
            ExportRequisition objExportRequisition = null;
            try
            {
                bool accessTypeComment = false;
                List<Question> lstFields = new List<Question>();
                var commonManager = new RequisitionCommonManager(this.JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
                var accessTypeCommentOnPDF = commonManager.GetSettingsValueByKey(P2PDocumentType.Requisition, "AccessTypeCommentOnPDF", UserContext.ContactCode);
                if (!string.IsNullOrEmpty(accessTypeCommentOnPDF))
                {
                    accessType = accessTypeCommentOnPDF;
                    accessTypeComment = true;
                }
                string accVisibilty = commonManager.GetSettingsValueByKey(P2PDocumentType.None, "AccountingDetailVisibiltyInExportPDF", UserContext.ContactCode);
                SettingDetails objSettingsnew = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.None, 1, 107);
                int maxPrecessionValue = Convert.ToInt32(commonManager.GetSettingsValueByKey(objSettingsnew, "MaxPrecessionValue"));
                int maxPrecessionValueForTotal = Convert.ToInt32(commonManager.GetSettingsValueByKey(objSettingsnew, "MaxPrecessionValueforTotal"));
                int maxPrecessionValueForTaxAndCharges = Convert.ToInt32(commonManager.GetSettingsValueByKey(objSettingsnew, "MaxPrecessionValueForTaxesAndCharges"));
                string enableDispatchMode = commonManager.GetSettingsValueByKey(P2PDocumentType.Requisition, "IsDispatchModeVisible", UserContext.ContactCode);
                bool isAcc = false;
                if (accVisibilty == "2" || (accVisibilty == "1" && this.UserContext.IsSupplier) || (accVisibilty == "0" && !this.UserContext.IsSupplier))
                    isAcc = true;
                objExportRequisition = GetReqDao().RequisitionExportById(RequisitionId, contactCode, userType, accessType, accessTypeComment, isAcc, maxPrecessionValue, maxPrecessionValueForTotal, maxPrecessionValueForTaxAndCharges, enableDispatchMode);
                string strHtml = System.Text.Encoding.UTF8.GetString(htmlTemplate);
                objExportRequisition.DocumentDataSet.P2PDocument[0].DocumentCode = RequisitionId;
                strHtml = FillRequisitionTemplate(objExportRequisition, strHtml, isForPrint);
                return Encoding.UTF8.GetBytes(strHtml);
            }
            finally
            {

                objExportRequisition = null;
            }
        }
        private bool CheckforfileExist()
        {
            var exists = false;
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(MultiRegionConfig.GetConfig(CloudConfig.BlobURL) + "cumuluscontent/P2P/ExportTemplate/" + UserContext.BuyerPartnerCode + "_RequisitionTemplate.html");
                request.Method = "HEAD";
                request.Timeout = 5000;
                return exists = ((HttpWebResponse)request.GetResponse()).StatusCode == HttpStatusCode.OK;
            }
            catch (WebException)
            {
                return exists = false;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occurred while getting client Requisition templete.", ex);
                throw;
            }
        }

        public FileManagerEntities.FileDetails RequisitionExportById(long RequisitionId, long contactCode, int userType, string accessType)
        {
            MemoryStream msHTMLStream = null;
            var objFiledetails = new FileManagerEntities.FileDetails();
            try
            {
                _userExecutionContext = UserContext;
                _gepConfig = GepConfiguration;
                byte[] htmlTemplate = null;
                LogNewRelic("Inside RequisitionExportById at LN - 162 - documentCode : " + RequisitionId.ToString(), "RequisitionExportById-before-GetRequisitionHtml");
                ExportManager objExportManager = new ExportManager (this.JWTToken){ UserContext = _userExecutionContext, GepConfiguration = _gepConfig };
                ExportTemplate exportTemplate = GetHtmlTemplateToPDFConversion(Gep.Cumulus.CSM.Entities.DocumentType.Requisition, RequisitionId);
                htmlTemplate = objExportManager.UpdateTemplateFonts(Encoding.UTF8.GetBytes(exportTemplate.TemplateHTML));
                htmlTemplate = GetRequisitionHtml(htmlTemplate, RequisitionId, false, contactCode, userType, accessType);

                byte[] bytLicenceData = objExportManager.GetLicenceBytes();
                ImpExManager objImpExManager = new ImpExManager(bytLicenceData);


                msHTMLStream = new MemoryStream();
                msHTMLStream.Write(htmlTemplate, 0, htmlTemplate.Length);
                byte[] pdfRequisition = objImpExManager.ConvertHTMLtoPDF(msHTMLStream, "", true);
                LogNewRelic("Inside RequisitionExportById at LN - 175 - documentCode : " + RequisitionId.ToString(), "RequisitionExportById-before-SaveMemoryStreamToFile");
                objFiledetails = objExportManager.SaveMemoryStreamToFile(pdfRequisition, strDocumentNumber);
            }
            catch (Exception ex)
            {
                LogNewRelic("Inside RequisitionExportById at LN - 181 - documentCode : " + RequisitionId.ToString(), "RequisitionExportById-SaveMemoryStreamToFile + Ex. Message =" + ex.Message);
                LogHelper.LogError(Log, "Error occurred while RequisitionExportById.", ex);
                throw;
            }
            finally
            {
                msHTMLStream = null;
            }
            return objFiledetails;
        }

        public string RequisitionPrintById(long RequisitionId, long contactCode, int userType, string accessType)
        {
            MemoryStream msHTMLStream = null;
            try
            {
                byte[] htmlTemplate = null;
                _userExecutionContext = UserContext;
                _gepConfig = GepConfiguration;
                ExportTemplate exportTemplate = GetHtmlTemplateToPDFConversion(Gep.Cumulus.CSM.Entities.DocumentType.Requisition, RequisitionId);
                ExportManager objExportManager = new ExportManager(this.JWTToken) { UserContext = _userExecutionContext, GepConfiguration = _gepConfig };
                htmlTemplate = objExportManager.UpdateTemplateFonts(Encoding.UTF8.GetBytes(exportTemplate.TemplateHTML));
                htmlTemplate = GetRequisitionHtml(htmlTemplate, RequisitionId, true, contactCode, userType, accessType);
                using (msHTMLStream = new MemoryStream())
                {
                    msHTMLStream.Write(htmlTemplate, 0, htmlTemplate.Length);
                    msHTMLStream.Position = 0;
                    var sr = new StreamReader(msHTMLStream);
                    return sr.ReadToEnd();
                }
            }
            finally
            {
                msHTMLStream = null;
            }
        }
        private void LogNewRelic(string message, string method)
        {
            var eventAttributes = new Dictionary<string, object>();
            eventAttributes.Add("message", message);
            eventAttributes.Add("method", method);
            NewRelic.Api.Agent.NewRelic.RecordCustomEvent("RequisitionExportManager", eventAttributes);
        }
    }
}
