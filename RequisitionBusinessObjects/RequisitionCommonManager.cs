using Gep.Cumulus.CSM.Entities;
using Gep.Cumulus.CSM.Entities.SurveyCustomAttribute;
using Gep.Cumulus.CSM.Extensions;
using Gep.Cumulus.Partner.Entities;
using GEP.Cumulus.Caching;
using GEP.Cumulus.Documents.Entities;
using GEP.Cumulus.Logging;
using GEP.Cumulus.OrganizationStructure.Entities;
using GEP.Cumulus.P2P.BusinessEntities;
using GEP.Cumulus.P2P.BusinessObjects;
using GEP.Cumulus.P2P.BusinessObjects.Proxy;
using GEP.Cumulus.P2P.Req.BusinessObjects.Entities;
using GEP.Cumulus.P2P.Req.BusinessObjects.Proxy;
using GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper;
using GEP.Cumulus.P2P.Req.DataAccessObjects;
using GEP.Cumulus.QuestionBank.Entities;
using GEP.Platform.FileManagerHelper;
using GEP.SMART.Configuration;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using static GEP.Cumulus.P2P.BusinessObjects.EmailNotificationManager;
using PASEntities = GEP.Cumulus.P2P.Req.BusinessObjects.Entities;
using FileManagerEntities = GEP.NewP2PEntities.FileManagerEntities;

namespace GEP.Cumulus.P2P.Req.BusinessObjects
{
    public class RequisitionCommonManager : RequisitionBaseBO
    {
        private const string VAB_USER = "10700203";
        private static readonly ILog Log = Logger.GetLog(MethodBase.GetCurrentMethod().DeclaringType);

        public RequisitionCommonManager(string jwtToken, UserExecutionContext context = null) : base(jwtToken)
        {
            if (context != null)
            {
                this.UserContext = context;
            }
        }

        public bool UpdateBaseCurrency(long contactCode, long documentCode, decimal documentAmount, int documentTypeId, string toCurrency, decimal conversionFactor)
        {
            bool AllowMultiCurrencyInRequisition = false;
            try
            {
                string maxPrecessionValue = GetSettingsValueByKey(P2PDocumentType.None, "MaxPrecessionValue", UserContext.ContactCode);
                long LOBId = GetCommonDao().GetLOBByDocumentCode(documentCode);               
                var AllowMultiCurrencyInRequisitionSetting = GetSettingsValueByKey(P2PDocumentType.Requisition, "AllowMultiCurrencyInRequisition", this.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBId);
                AllowMultiCurrencyInRequisition = !string.IsNullOrEmpty(AllowMultiCurrencyInRequisitionSetting) ? Convert.ToBoolean(AllowMultiCurrencyInRequisitionSetting) : false;
                var precessionValue = Convert.ToInt16(string.IsNullOrEmpty(maxPrecessionValue) ? "0" : maxPrecessionValue);
                documentAmount = Math.Round(documentAmount, precessionValue);

            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occurred in UpdateBaseCurrency GetReqCommonDao is null", ex.InnerException);

            }
            if (AllowMultiCurrencyInRequisition)
                return false;
            else
            return GetRequisitionCommonDAO().UpdateBaseCurrency(contactCode, documentCode, documentAmount, documentTypeId, toCurrency, conversionFactor);
        }

        public decimal GetCurrencyConversionFactor(string fromCurrency, string toCurrency)
        {
            var conversionFactor = new MultiCurrencyExchange();
            if (fromCurrency != toCurrency)
            {
                CurrencyExchageRate objCurrencyExchageRate = new CurrencyExchageRate();
                objCurrencyExchageRate.FromCurrencyCode = fromCurrency;
                objCurrencyExchageRate.ToCurrencyCode = toCurrency;
                objCurrencyExchageRate.EffectiveDate = DateTime.Now;

                var requestHeaders = new GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.RequestHeaders();
                requestHeaders.Set(this.UserContext, this.JWTToken);
                string appName = Environment.GetEnvironmentVariable("NewRelic.AppName") ?? "Requisition";
                string useCase = "RequisitionCommonManager-GetCurrencyConversionFactor";
                var serviceURL = MultiRegionConfig.GetConfig(CloudConfig.APIMBaseURL) + GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.ServiceURLs.CurrencyServiceURL + "GetDefaultCurrencyExchangeRateByConversionDate";

                var webAPI = new GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.WebAPI(requestHeaders, appName, useCase);
                var response = webAPI.ExecutePost(serviceURL, objCurrencyExchageRate);

                decimal exchangeRate = 1m;
                JObject resJson = JObject.Parse(response);
                if (resJson.SelectToken("CurrencyExchageRates[0].ExchangeRate") != null)
                {
                    exchangeRate = Convert.ToDecimal(resJson.SelectToken("CurrencyExchageRates[0].ExchangeRate"));
                }
                return exchangeRate;
            }
            else
            {
                return 1m;
            }
        }

        public string GetBaseCurrency()
        {
            CSMHelper objCSMWebAPI = new CSMHelper(this.UserContext, this.JWTToken);
            var lstCurrency = objCSMWebAPI.GetAllCurrency();
            string baseCurrency = lstCurrency.Where(x => x.IsDefault == true).Select(s => s.Value).SingleOrDefault();
            return baseCurrency;
        }

        public decimal GetCurrencyConversionRate(string fromCurrency, string toCurrency)
        {
            var conversionFactor = new MultiCurrencyExchange();
            decimal exchangeRate = 1m;
            try
            {
                if ((fromCurrency != toCurrency) && !string.IsNullOrEmpty(fromCurrency) && !string.IsNullOrEmpty(toCurrency))
                {
                    CurrencyExchageRate objCurrencyExchageRate = new CurrencyExchageRate();
                    objCurrencyExchageRate.FromCurrencyCode = fromCurrency;
                    objCurrencyExchageRate.ToCurrencyCode = toCurrency;
                    objCurrencyExchageRate.EffectiveDate = DateTime.Now;
                    var requestHeaders = new GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.RequestHeaders();
                    requestHeaders.Set(this.UserContext, this.JWTToken);
                    string appName = Environment.GetEnvironmentVariable("NewRelic.AppName") ?? "Requisition";
                    string useCase = "RequisitionCommonManager-GetCurrencyConversionRate";
                    var serviceURL = MultiRegionConfig.GetConfig(CloudConfig.APIMBaseURL) + GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.ServiceURLs.CurrencyServiceURL + "GetCurrencyExchangeRateByConversionDate";
                    var webAPI = new GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.WebAPI(requestHeaders, appName, useCase);
                    var response = webAPI.ExecutePost(serviceURL, objCurrencyExchageRate);

                    JObject resJson = JObject.Parse(response);
                    if (resJson.SelectToken("ExchangeRate") != null)
                    {
                        exchangeRate = resJson.SelectToken("ExchangeRate").ToObject<decimal>();
                    }
                    return exchangeRate;
                }
                else
                {
                    return 1m;
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in GetCurrencyConversionRate in RequisitionCommonManager", ex);
                throw ex;
            }

        }

        public string GetCustomAttributeValuesForUser(long contactCode, string customAttributeText)
        {
            string result = "";
            try
            {
                if (!string.IsNullOrEmpty(customAttributeText))
                {
                    var requestHeaders = new GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.RequestHeaders();
                    requestHeaders.Set(this.UserContext, this.JWTToken);
                    string appName = Environment.GetEnvironmentVariable("NewRelic.AppName") ?? "Requisition";
                    string useCase = "RequisitionCommonManager-GetCustomAttributeValuesForUser";
                    var serviceURL = MultiRegionConfig.GetConfig(CloudConfig.APIMBaseURL) + GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.ServiceURLs.GetCustomAttributeValuesForUser + contactCode;
                    var webAPI = new GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.WebAPI(requestHeaders, appName, useCase);
                    var response = webAPI.ExecutePost(serviceURL, "");
                    JArray jsonArray = JArray.Parse(response);
                    JObject resJson = jsonArray.Children<JObject>().FirstOrDefault(o => o["QuestionText"] != null && o["QuestionText"].ToString().ToUpper() == customAttributeText.Trim().ToUpper());
                    if (resJson != null && resJson.SelectToken("ResponseValue") != null)
                    {
                        result = resJson.SelectToken("ResponseValue").ToObject<string>();
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in GetCustomAttributeValuesForUser in RequisitionCommonManager", ex);
                throw ex;
            }
            return result;

        }
        public string GetSettingsValueByKey(P2PDocumentType docType, string key, long userId, int subAppCode = (int)SubAppCodes.P2P, string sourceSystemName = "", long LOBId = 0)
        {
            var keyValue = string.Empty;
            var settingDetails = GetSettingsFromSettingsComponent(docType, userId, subAppCode, sourceSystemName, LOBId);

            if (settingDetails != null && settingDetails.lstSettings.Any())
            {
                foreach (var sett in settingDetails.lstSettings)
                {
                    if (sett.FieldName == key)
                    {
                        keyValue = sett.FieldValue;
                        break;
                    }
                }
            }
            return keyValue;
        }

        public string GetSettingsValueByKey(SettingDetails settingDetails, string key)
        {
            if (settingDetails != null && settingDetails.lstSettings.Any())
            {
                foreach (var sett in settingDetails.lstSettings)
                {
                    if (sett.FieldName == key)
                        return sett.FieldValue;
                }
            }
            return string.Empty;
        }

        public SettingDetails GetSettingsFromSettingsComponent(P2PDocumentType docType, long userId, int subAppCode, string sourceSystemName = "", long LOBId = 0)
        {
            var objectType = string.Empty;
            string basicSettingsCacheKey = String.Concat("RequisitionCommonManager_DownstreamBasicSettings", "_", docType, "_", sourceSystemName, "_", LOBId);
            SettingDetails objSettingDetails = GEPDataCache.GetFromCacheJSON<SettingDetails>(basicSettingsCacheKey, UserContext.BuyerPartnerCode, "en-US");
            if (objSettingDetails == null)
            {
                switch (docType)
                {
                    case P2PDocumentType.Requisition:
                        objectType = "GEP.Cumulus.P2P.Requisition";
                        break;
                    case P2PDocumentType.Order:
                        objectType = "GEP.Cumulus.P2P.Order";
                        break;
                    case P2PDocumentType.Receipt:
                        objectType = "GEP.Cumulus.P2P.Receipt";
                        break;
                    case P2PDocumentType.Invoice:
                        objectType = "GEP.Cumulus.P2P.Invoice";
                        break;
                    case P2PDocumentType.InvoiceReconciliation:
                        objectType = "GEP.Cumulus.P2P.InvoiceReconciliation";
                        break;
                    case P2PDocumentType.Interfaces:
                        objectType = "GEP.Cumulus.Interfaces";
                        break;
                    case P2PDocumentType.Catalog:
                        objectType = "GEP.Cumulus.Catalog";
                        break;
                    case P2PDocumentType.ReturnNote:
                        objectType = "GEP.Cumulus.P2P.ReturnNote";
                        break;
                    case P2PDocumentType.Template:
                        objectType = "GEP.Cumulus.Items.Template";
                        break;
                    case P2PDocumentType.CreditMemo:
                        objectType = "GEP.Cumulus.P2P.CreditMemo";
                        break;
                    case P2PDocumentType.Program:
                        objectType = "GEP.Cumulus.P2P.Program";
                        break;
                    case P2PDocumentType.PaymentRequest:
                        objectType = "GEP.Cumulus.P2P.PaymentRequest";
                        break;
                    case P2PDocumentType.ServiceConfirmation:
                        objectType = "GEP.Cumulus.P2P.ServiceConfirmation";
                        break;
                    case P2PDocumentType.ASN:
                        objectType = "GEP.Cumulus.P2P.ASN";
                        break;
                    case P2PDocumentType.Portal:
                        objectType = "GEP.Cumulus.Portal.ClientSpecificSettings";
                        break;
                    default:
                        objectType = "GEP.Cumulus.P2P";
                        break;
                }

                if (!string.IsNullOrEmpty(sourceSystemName))
                    objectType = objectType + "." + sourceSystemName;

                UserExecutionContext userExecutionContext = UserContext;

                try
                {
                    var settingsHelper = new SettingsHelper(UserContext, this.JWTToken);
                    var requestBody = new
                    {
                        SubAppCode = subAppCode,
                        ObjectType = objectType,
                        ContactCode = userId,
                        LOBId = LOBId
                    };
                    objSettingDetails = settingsHelper.GetFeatureSettings(requestBody);

                    if (objSettingDetails.lstSettings != null && objSettingDetails.lstSettings.Count > 0)
                        GEPDataCache.PutInCacheJSON<SettingDetails>(basicSettingsCacheKey, UserContext.BuyerPartnerCode, "en-US", objSettingDetails);
                }
                catch
                {
                    GEPDataCache.RemoveFromCache(basicSettingsCacheKey, UserContext.BuyerPartnerCode, "en-US");
                    throw;
                }
            }
            return objSettingDetails;
        }

        public bool FlipCustomFields(long srcDocCode, int srcDocType, long tgtDocCode, int tgtDocType, List<Level> levels, bool isMapping = true)
        {
            try
            {
                foreach (var item in GetCustomAttrFormId(srcDocType, levels, 0, 0, 0, 0, srcDocCode))
                {
                    if (item.Value == 0)
                        levels.Remove(item.Key);
                }
                List<Tuple<int, int, int, long, long>> lstSrcTgt = new List<Tuple<int, int, int, long, long>>();
                if (levels.Contains(Level.Header))
                    lstSrcTgt.Add(new Tuple<int, int, int, long, long>(srcDocType, tgtDocType, (int)Level.Header, srcDocCode, tgtDocCode));
                if (levels.Contains(Level.Item) || levels.Contains(Level.Distribution))
                    lstSrcTgt.AddRange(GetCommonDao().GetSrcTgtIdsMapping(srcDocCode, srcDocType, tgtDocCode, tgtDocType, levels, isMapping));

                if (lstSrcTgt.Count > 0 && levels.Count > 0)
                {
                    List<FlippedDocumentDetail> lstFlipDetail = new List<FlippedDocumentDetail>();
                    FlippedDocumentDetail flipDetail;
                    foreach (var item in lstSrcTgt)
                    {
                        flipDetail = new FlippedDocumentDetail();
                        flipDetail.SourceDocumentType = item.Item1;
                        flipDetail.TargetDocumentType = item.Item2;
                        flipDetail.Level = item.Item3;
                        flipDetail.SourceObjectId = item.Item4;
                        flipDetail.TargetObjectId = item.Item5;
                        lstFlipDetail.Add(flipDetail);
                    }

                    string appName = Environment.GetEnvironmentVariable("NewRelic.AppName") ?? "Requisition";
                    string useCase = "RequisitionCommonManager-FlipCustomFields";
                    string serviceURL = MultiRegionConfig.GetConfig(CloudConfig.APIMBaseURL) + ServiceURLs.CustomAttributesURL + "FlipCustomAttributeValues";

                    var body = new
                    {
                        FlippedDocumentDetails = lstFlipDetail
                    };

                    var requestHeaders = new RequestHeaders();
                    requestHeaders.Set(UserContext, JWTToken);
                    var webAPI = new WebAPI(requestHeaders, appName, useCase);
                    webAPI.ExecutePost(serviceURL, body);
                }
                return true;
            }
            catch (CommunicationException ex)
            {
                LogHelper.LogError(Log, "Communication Error occurred in FlipCustomFields Method in RequisitionCommonManager for srcDocCode = " + srcDocCode + "tgtDocCode" + tgtDocCode, ex);
                throw;
            }
            catch (Exception e)
            {
                LogHelper.LogError(Log, "Error occurred in FlipCustomFields Method in RequisitionCommonManager for srcDocCode = " + srcDocCode + "tgtDocCode" + tgtDocCode, e);
                throw;
            }
        }

        public List<KeyValuePair<Level, long>> GetCustomAttrFormId(int docType, List<Level> levels, long categoryId = 0, long LOBEntityDetailCode = 0, int entityId = 0, long entityDetailCode = 0, long documentCode = 0, int purchaseTypeId = 0)
        {
            return GetCommonDao().GetCustomAttrFormId(docType, levels, categoryId, LOBEntityDetailCode, entityId, entityDetailCode, documentCode, purchaseTypeId);
        }

        public CommentsGroup GetComments(long documentId, P2PDocumentType docType, string accessType, int level, long contactCode, bool isInterface = false)
        {
            CommentsGroup objCommentsGroup = new CommentsGroup();
            objCommentsGroup.ObjectID = documentId;

            if (docType == P2PDocumentType.Order && level == 1)
                objCommentsGroup.ObjectType = "GEP.Cumulus.P2P.Order";
            else if (docType == P2PDocumentType.Order && level == 2)
                objCommentsGroup.ObjectType = "GEP.Cumulus.P2P.Order.LineItem";
            else if (docType == P2PDocumentType.Invoice && level == 1)
                objCommentsGroup.ObjectType = "GEP.Cumulus.P2P.Invoice";
            else if (docType == P2PDocumentType.Invoice && level == 2)
                objCommentsGroup.ObjectType = "GEP.Cumulus.P2P.Invoice.LineItem";
            else if (docType == P2PDocumentType.Receipt && level == 1)
                objCommentsGroup.ObjectType = "GEP.Cumulus.P2P.Receipt";
            else if (docType == P2PDocumentType.Receipt && level == 2)
                objCommentsGroup.ObjectType = "GEP.Cumulus.P2P.Receipt.LineItem";
            else if (docType == P2PDocumentType.Requisition && level == 1)
                objCommentsGroup.ObjectType = "GEP.Cumulus.P2P.Requisition";
            else if (docType == P2PDocumentType.Requisition && level == 2)
                objCommentsGroup.ObjectType = "GEP.Cumulus.P2P.Requisition.LineItem";
            else if (docType == P2PDocumentType.CreditMemo && level == 1)
                objCommentsGroup.ObjectType = "GEP.Cumulus.P2P.CreditMemo";
            else if (docType == P2PDocumentType.CreditMemo && level == 2)
                objCommentsGroup.ObjectType = "GEP.Cumulus.P2P.CreditMemo.LineItem";
            else if (docType == P2PDocumentType.InvoiceReconciliation && level == 1)
                objCommentsGroup.ObjectType = "GEP.Cumulus.P2P.IR";
            else if (docType == P2PDocumentType.InvoiceReconciliation && level == 2)
                objCommentsGroup.ObjectType = "GEP.Cumulus.P2P.IR.LineItem";

            List<CommentsGroupRequestModel> commentsGroupRequestModel = new List<CommentsGroupRequestModel>();
            commentsGroupRequestModel.Add(new CommentsGroupRequestModel()
            {
                AccessType = accessType,
                ObjectType = objCommentsGroup.ObjectType,
                ObjectID = objCommentsGroup.ObjectID
            });

            var commentHelper = new Req.BusinessObjects.RESTAPIHelper.CommentHelper(this.UserContext, JWTToken);
            var result = commentHelper.GetCommentsWithAttachments(commentsGroupRequestModel);
            var cmtGroup = commentHelper.Map(result);
            if (cmtGroup != null && cmtGroup.Count > 0)
            {
                objCommentsGroup = cmtGroup.FirstOrDefault();
            }

            if (isInterface)
            {
                if (objCommentsGroup != null && objCommentsGroup.Comment != null && objCommentsGroup.Comment.Any())
                    objCommentsGroup.Comment = objCommentsGroup.Comment.OrderBy(comment => comment.CommentID).ToList();
            }

            return objCommentsGroup ?? new CommentsGroup();
        }

        public void FillQuestionsResponseList(List<QuestionResponse> lstQuestionsResponse, List<GEP.Cumulus.P2P.BusinessEntities.Questionnaire> customAttributes, List<long> lstEntityQuestionSetCode, long objectInstanceId, bool isDataValidation = false)
        {
            QuestionResponse questionResponse = new QuestionResponse();

            foreach (var entityQuestionSetCode in lstEntityQuestionSetCode)
            {
                var surveyQuestions = GetQuestionWithResponsesByQuestionSetPaging(new Question() { QuestionSetCode = entityQuestionSetCode }, -1, -1, objectInstanceId, AssessorUserType.Buyer, false);

                if (surveyQuestions != null && surveyQuestions.Any())
                {
                    foreach (GEP.Cumulus.P2P.BusinessEntities.Questionnaire entityQuestionarie in customAttributes)
                    {
                        if (entityQuestionarie.QuestionnaireResponseValues != null && entityQuestionarie.QuestionnaireResponseValues.Any())
                        {
                            foreach (QuestionnaireResponse entityQuestionarieResponse in entityQuestionarie.QuestionnaireResponseValues)
                            {
                                var surveyQuestionObj = surveyQuestions.Where(surveryQuest => surveryQuest.QuestionText.Equals(entityQuestionarie.QuestionnaireTitle, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

                                if (!ReferenceEquals(surveyQuestionObj, null))
                                {
                                    long childQuestionSetCode = 0;
                                    questionResponse = new QuestionResponse();

                                    questionResponse.QuestionId = surveyQuestionObj.QuestionId;
                                    questionResponse.ObjectInstanceId = objectInstanceId;
                                    questionResponse.AssessorId = -1;
                                    questionResponse.AssesseeId = -1;
                                    questionResponse.AssessorType = AssessorUserType.Buyer;
                                    questionResponse.ResponseValue = entityQuestionarieResponse.ResponseValue;

                                    if (surveyQuestionObj.GetType() == typeof(DBLookUpQuestion))
                                        questionResponse.RowId = entityQuestionarieResponse.RowId;

                                    if (surveyQuestionObj.GetType() == typeof(MultipleChoiceQuestion) && !string.IsNullOrEmpty(entityQuestionarieResponse.ResponseValue))
                                    {
                                        var rowChoice = ((MultipleChoiceQuestion)surveyQuestionObj).RowChoices.Where(row => row.RowText == entityQuestionarieResponse.ResponseValue).FirstOrDefault();

                                        if (!ReferenceEquals(rowChoice, null))
                                        {
                                            questionResponse.RowId = rowChoice.RowId;
                                            childQuestionSetCode = rowChoice.ChildQuestionSetCode;

                                            if (
                                                surveyQuestionObj.QuestionTypeInfo.QuestionTypeId == 4 ||         // Matrix Of Radio  Buttons
                                                surveyQuestionObj.QuestionTypeInfo.QuestionTypeId == 6 ||         // Radio Buttons - Single Answer
                                                surveyQuestionObj.QuestionTypeInfo.QuestionTypeId == 8            // Check Boxes - Multiple Answer
                                               )
                                                questionResponse.ResponseValue = "true";
                                        }
                                    }
                                    else if (surveyQuestionObj.GetType() == typeof(MultipleMatrixQuestion) && !string.IsNullOrEmpty(entityQuestionarieResponse.ResponseValue))
                                    {
                                        var colChoice = ((MultipleMatrixQuestion)surveyQuestionObj).ColumnChoices.Where(col => col.ColumnText == entityQuestionarieResponse.ColumnText).FirstOrDefault();

                                        if (!ReferenceEquals(colChoice, null))
                                        {
                                            questionResponse.ColumnId = colChoice.ColumnId;
                                            questionResponse.RowId = entityQuestionarieResponse.RowId;

                                            if (!ReferenceEquals(colChoice.ListMatrixCellChoices, null) && colChoice.ListMatrixCellChoices.Count > 0)
                                            {
                                                var cellChoice = colChoice.ListMatrixCellChoices.Where(cell => cell.ChoiceValue == entityQuestionarieResponse.ResponseValue).FirstOrDefault();

                                                if (ReferenceEquals(cellChoice, null) && this.UserContext.Product == GEPSuite.eInterface)
                                                    cellChoice = colChoice.ListMatrixCellChoices.Where(cell => cell.ChoiceValue.Contains(entityQuestionarieResponse.ResponseValue)).FirstOrDefault();

                                                if (!ReferenceEquals(cellChoice, null))
                                                    questionResponse.ResponseValue = Convert.ToString(cellChoice.CellChoiceId);
                                                else if (this.UserContext.Product == GEPSuite.eInterface)
                                                    questionResponse.ResponseValue = string.Empty;
                                            }
                                        }
                                    }

                                    lstQuestionsResponse.Add(questionResponse);

                                    if (entityQuestionarieResponse.ChildQuestionSets != null && entityQuestionarieResponse.ChildQuestionSets.Count != 0)
                                        FillQuestionsResponseList(lstQuestionsResponse, entityQuestionarieResponse.ChildQuestionSets, new List<long>() { childQuestionSetCode }, objectInstanceId);

                                }
                            }
                        }
                        else if (isDataValidation)
                        {
                            var surveyQuestionObj = surveyQuestions.Where(surveryQuest => surveryQuest.QuestionText == entityQuestionarie.QuestionnaireTitle).FirstOrDefault();

                            if (!ReferenceEquals(surveyQuestionObj, null))
                            {
                                questionResponse = new QuestionResponse();

                                questionResponse.QuestionId = surveyQuestionObj.QuestionId;
                                questionResponse.ObjectInstanceId = objectInstanceId;
                                questionResponse.AssessorId = -1;
                                questionResponse.AssesseeId = -1;
                                questionResponse.AssessorType = AssessorUserType.Buyer;
                                questionResponse.ResponseValue = string.Empty;
                            }

                            lstQuestionsResponse.Add(questionResponse);
                        }

                    }

                }

            }
        }

        public int GetPrecisionValue()
        {
            UserExecutionContext userExecutionContext = UserContext;
            string precesionValue = GetSettingsValueByKey(P2PDocumentType.None, "MaxPrecessionValue", userExecutionContext.ContactCode);
            return Convert.ToInt16(string.IsNullOrEmpty(precesionValue) ? "0" : precesionValue);
        }
        public int GetPrecisionValueforTotal()
        {
            UserExecutionContext userExecutionContext = UserContext;
            string precesionValueforTotal = GetSettingsValueByKey(P2PDocumentType.None, "MaxPrecessionValueforTotal", userExecutionContext.ContactCode);
            return Convert.ToInt16(string.IsNullOrEmpty(precesionValueforTotal) ? "0" : precesionValueforTotal);
        }
        public int GetPrecisionValueForTaxesAndCharges()
        {
            UserExecutionContext userExecutionContext = UserContext;
            string precesionValue = GetSettingsValueByKey(P2PDocumentType.None, "MaxPrecessionValueForTaxesAndCharges", userExecutionContext.ContactCode);
            return Convert.ToInt16(string.IsNullOrEmpty(precesionValue) ? "0" : precesionValue);
        }

        public List<GEP.Cumulus.P2P.BusinessEntities.Questionnaire> GetQuestionWithResponse(List<long> surveyQuestionsetCodes, List<GEP.Cumulus.P2P.BusinessEntities.Questionnaire> lstP2PpQuestionarie, long objectInstanceId, bool getBlankResponses = true)
        {
            foreach (var surveyQuestionsetCode in surveyQuestionsetCodes)
            {
                var surveyQuestionsWithResponse = new List<QuestionBank.Entities.Question>();

                //Get All Questions based on question set code                
                var surveyQuestions = GetQuestionWithResponsesByQuestionSetPaging(new Question() { QuestionSetCode = surveyQuestionsetCode }, -1, -1, objectInstanceId, AssessorUserType.Buyer, false);

                //Filter only those questions having reponses 
                if (surveyQuestions != null && surveyQuestions.Any())
                {
                    if (getBlankResponses)
                        surveyQuestionsWithResponse = surveyQuestions.ToList<QuestionBank.Entities.Question>();
                    else
                        surveyQuestionsWithResponse = surveyQuestions.Where(quest => quest.ListQuestionResponses != null && quest.ListQuestionResponses.Count > 0).ToList<QuestionBank.Entities.Question>();
                }

                if (surveyQuestionsWithResponse != null && surveyQuestionsWithResponse.Any())
                {
                    foreach (var question in surveyQuestionsWithResponse)
                    {

                        GEP.Cumulus.P2P.BusinessEntities.Questionnaire _p2pQuestion = new GEP.Cumulus.P2P.BusinessEntities.Questionnaire();
                        _p2pQuestion.QuestionnaireResponseValues = new List<QuestionnaireResponse>();
                        var lstColumnChoices = new List<QuestionColumnChoice>();

                        _p2pQuestion.QuestionnaireTitle = question.QuestionText;
                        _p2pQuestion.IsSupplierVisible = question.VisibleTo == VisibleTo.PartnerOnly || question.VisibleTo == VisibleTo.Both ? true : false;

                        if (question.GetType() == typeof(MultipleMatrixQuestion) && ((MultipleMatrixQuestion)question).ColumnChoices != null && ((MultipleMatrixQuestion)question).ColumnChoices.Any())
                            lstColumnChoices = ((MultipleMatrixQuestion)question).ColumnChoices;


                        foreach (QuestionBank.Entities.QuestionResponse questResponse in question.ListQuestionResponses)
                        {
                            List<GEP.Cumulus.P2P.BusinessEntities.Questionnaire> childQuestionWithResponse = new List<GEP.Cumulus.P2P.BusinessEntities.Questionnaire>();
                            string rowText = null;
                            long rowId = 0;
                            string columnText = string.Empty;

                            if (question.GetType() == typeof(DBLookUpQuestion))
                            {
                                if (this.UserContext.Product == GEPSuite.eInterface)
                                {
                                    var objDBLookUpQuestion = (DBLookUpQuestion)question;
                                    DBLookUpFieldConfig DBLookUpFieldConfig = new DBLookUpFieldConfig();

                                    int responseId = 0;
                                    Int32.TryParse(questResponse.ResponseValue, out responseId);

                                    if (responseId > 0)
                                    {
                                        DBLookUpFieldConfig.FieldId = Convert.ToInt32(questResponse.ResponseValue);
                                        DBLookUpFieldConfig.IsAutosuggest = true;
                                        DBLookUpFieldConfig.FieldGetSPName = objDBLookUpQuestion.DBLookUpFieldConfig.FieldGetSPName + "ForInterface";
                                        List<CustomDBLookUpQuestionData> lstCustomDBLookUpQuestionData = new List<CustomDBLookUpQuestionData>();
                                        ProxySurveyComponent objProxySurveyComponent = null;
                                        objProxySurveyComponent = new ProxySurveyComponent(UserContext, this.JWTToken);
                                        lstCustomDBLookUpQuestionData = objProxySurveyComponent.GetCustomDBLookUpQuestionData(DBLookUpFieldConfig);

                                        if (lstCustomDBLookUpQuestionData != null && lstCustomDBLookUpQuestionData.Count > 0)
                                        {
                                            questResponse.ResponseValue = lstCustomDBLookUpQuestionData.FirstOrDefault().FieldName;
                                        }
                                    }
                                }

                                rowId = questResponse.RowId;
                            }

                            if (question.GetType() == typeof(MultipleChoiceQuestion))
                            {
                                var rowChoice = ((MultipleChoiceQuestion)question).RowChoices.Where(row => row.RowId == questResponse.RowId).FirstOrDefault();

                                if (!ReferenceEquals(rowChoice, null))
                                {
                                    rowText = rowChoice.RowText;

                                    if (rowChoice.ChildQuestionSetCode > 0)
                                        childQuestionWithResponse = GetQuestionWithResponse(new List<long> { rowChoice.ChildQuestionSetCode }, childQuestionWithResponse, objectInstanceId, getBlankResponses);
                                }
                            }
                            else if (question.GetType() == typeof(MultipleMatrixQuestion) && lstColumnChoices != null && lstColumnChoices.Any())
                            {
                                var colChoice = lstColumnChoices.ToList().Where(col => col.ColumnId == questResponse.ColumnId).FirstOrDefault();

                                if (colChoice != null)
                                {
                                    columnText = colChoice.ColumnText;
                                    int responseId;
                                    if (this.UserContext.Product == GEPSuite.eInterface && colChoice.ColumnType == ColumnType.Dropdown && !string.IsNullOrEmpty(questResponse.ResponseValue) && Int32.TryParse(questResponse.ResponseValue, out responseId))
                                    {
                                        string response = colChoice.ListMatrixCellChoices.Where(c => c.CellChoiceId == Convert.ToInt64(questResponse.ResponseValue)).Select(c => c.ChoiceValue).FirstOrDefault();
                                        questResponse.ResponseValue = response;
                                    }
                                }

                                rowId = questResponse.RowId;
                            }

                            _p2pQuestion.QuestionnaireResponseValues.Add(new QuestionnaireResponse
                            {
                                ChildQuestionSets = childQuestionWithResponse.Count > 0 ? childQuestionWithResponse : null
                               ,
                                ResponseValue = rowText ?? questResponse.ResponseValue
                               ,
                                RowId = rowId
                               ,
                                ColumnText = columnText
                            });
                        }
                        lstP2PpQuestionarie.Add(_p2pQuestion);

                    }
                }
            }
            return lstP2PpQuestionarie;
        }

        public List<Question> GetQuestionWithResponsesByQuestionSetPaging(Question objQuestion, long assessorId, long assesseeId, long objectInstanceId, AssessorUserType assessorType, bool bloadQuestionScoreResponse)
        {
            ProxySurveyComponent objProxySurveyComponent = new ProxySurveyComponent(UserContext, this.JWTToken);

            return objProxySurveyComponent.GetQuestionWithResponsesByQuestionSetPaging(objQuestion, assessorId, assesseeId, objectInstanceId, assessorType, bloadQuestionScoreResponse);
        }

        public void SaveQuestionsResponse(List<QuestionResponse> lstQuestionsResponse, long DocumentCode, int DocTypeCode)
        {
            ProxySurveyComponent objProxySurveyComponent = null;

            objProxySurveyComponent = new ProxySurveyComponent(UserContext, this.JWTToken);

            objProxySurveyComponent.SaveQuestionsResponse(lstQuestionsResponse, DocumentCode, DocTypeCode);
        }

        public FileManagerEntities.FileDetails UploadAndSaveFile(string fileUrl, string fileName = null)
        {
            var fileDetails = new FileManagerEntities.FileDetails();
            var fileManagerApi = new FileManagerApi(this.UserContext, this.JWTToken);

            if (!string.IsNullOrEmpty(fileUrl))
            {
                try
                {
                    byte[] fileData;
                    fileData = FileOperations.DownloadByteArrayAsync(fileUrl, MultiRegionConfig.GetConfig(CloudConfig.CloudStorageConn)).GetAwaiter().GetResult();


                    string tempBlobFileUri = FileOperations.UploadByteArraytoTemporaryBlobAsync(fileData,
                                                                                                MultiRegionConfig.GetConfig(CloudConfig.CloudStorageConn),
                                                                                                MultiRegionConfig.GetConfig(CloudConfig.TempFileUploadContainerName)).GetAwaiter().GetResult();

                    var requestHeaders = new GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.RequestHeaders();
                    requestHeaders.Set(this.UserContext, this.JWTToken);
                    string appName = Environment.GetEnvironmentVariable("NewRelic.AppName") ?? "Requisition";
                    string useCase = "RequisitionCommonManager-UploadAndSaveFile";
                    var serviceURL = MultiRegionConfig.GetConfig(CloudConfig.APIMBaseURL) + "/FileManager/api/V2/FileManager/MoveFileToTargetBlob";

                    var uploadFileToTargetBlobRequestModel = new FileManagerEntities.MoveFileToTargetBlobRequest()
                    {
                        FileName = !string.IsNullOrWhiteSpace(fileName) ? fileName : Path.GetFileName(fileUrl),
                        FileContentType = MimeMapping.GetMimeMapping(Path.GetFileName(fileUrl)),
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

                    fileDetails.FileId = fileUploadResponse.FileId;
                    fileDetails.FileName = fileUploadResponse.FileDisplayName;
                    fileDetails.FileExtension = fileUploadResponse.FileExtension;
                    fileDetails.FileSizeInBytes = fileUploadResponse.FileSizeInBytes;
                    fileDetails.FileCreatedBy = fileUploadResponse.FileCreatedBy;
                    fileDetails.EncryptedFileId = fileUploadResponse.EncryptedFileId;

                }
                catch (CommunicationException ex)
                {
                    LogHelper.LogError(Log, "Error occured in SaveFile Method in RequisitionCommonManager", ex);
                }
            }
            return fileDetails;
        }

        public int InsertShiptoLocation(ShiptoLocation objShipToLocation, long LOBEntityDetailCode, long EntityDetailCode, int EntityId, bool isInterface = false, long contactCode = 0)
        {
            return GetCommonDao().InsertShiptoLocation(objShipToLocation, LOBEntityDetailCode, EntityDetailCode, EntityId, isInterface, contactCode);//need documentcode for indexing
        }

        public int AddUpdateShipToDetail_New(ShiptoLocation shiptoLocation, ref RequisitionCommonManager objCommon, bool isInterface = false)
        {
            StringBuilder strErrors = new StringBuilder();
            bool toInsert = true;

            if (shiptoLocation != null)
            {
                #region 'Logic to decide whether New ShipToLoc to be created if ShiptoLoc doesn't exists'
                if (shiptoLocation.ShiptoLocationId == 0)
                {
                    if (string.IsNullOrWhiteSpace(shiptoLocation.Address.AddressLine1) || string.IsNullOrWhiteSpace(shiptoLocation.Address.State) || string.IsNullOrWhiteSpace(shiptoLocation.Address.CountryCode) || string.IsNullOrWhiteSpace(shiptoLocation.ShiptoLocationName) || string.IsNullOrWhiteSpace(shiptoLocation.Address.Zip) || string.IsNullOrWhiteSpace(shiptoLocation.Address.City))
                        toInsert = false;
                }
                string shipToLocSetting = objCommon.GetSettingsValueByKey(P2PDocumentType.Interfaces, "IsNewShipToLocCreationRequired", 0, (int)SubAppCodes.Interfaces);
                if (Convert.ToInt32(shipToLocSetting, NumberFormatInfo.InvariantInfo) == 1 && toInsert == true)
                {
                    if (shiptoLocation.Address == null)
                        shiptoLocation.Address = new P2PAddress();

                    var csmHelper = new RESTAPIHelper.CSMHelper(UserContext, JWTToken);
                    var body = new
                    {
                        EntityTypeName = new Gep.Cumulus.CSM.Entities.Address().GetType().FullName,
                        SeedIncrement = 1
                    };
                    shiptoLocation.Address.AddressId = csmHelper.GenerateNewEntityCode(body);
                    shiptoLocation.IsAdhoc = Convert.ToBoolean(1);
                    shiptoLocation.AllowForFutureReference = Convert.ToBoolean(0);
                    shiptoLocation.Address.Country = shiptoLocation.Address.CountryCode;
                    shiptoLocation.ShiptoLocationId = objCommon.InsertShiptoLocation(shiptoLocation, 0, 0, 0, isInterface);//indexing in called function
                }

            }

            if (strErrors.Length == 0)
            {
                return shiptoLocation.ShiptoLocationId;
            }
            else
                throw new Exception("ValidationException: " + strErrors.ToString());

            #endregion
        }


        public ChargeMaster GetChargeMasterDetailsByChargeMasterId(long chargeMasterId)
        {
            try
            {
                return GetCommonDao().GetChargeMasterDetailsByChargeMasterId(chargeMasterId);
            }
            catch (CommunicationException ex)
            {
                LogHelper.LogError(Log, "GetChargeMasterDetailsByChargeMasterId for chargeMasterId=" + chargeMasterId + " " + ex.Message, ex);
            }
            return null;
        }

        public List<ItemCharge> SaveReqAllItemCharges(List<ItemCharge> listItemCharges)
        {

            try
            {
                int precessionValue = convertStringToInt(this.GetSettingsValueByKey(P2PDocumentType.None, "MaxPrecessionValue", UserContext.ContactCode, (int)SubAppCodes.P2P, "", 0));
                int precessionValueForTotal = convertStringToInt(this.GetSettingsValueByKey(P2PDocumentType.None, "MaxPrecessionValueforTotal", UserContext.ContactCode, (int)SubAppCodes.P2P, "", 0));
                int precessionValueForTaxesAndCharges = convertStringToInt(this.GetSettingsValueByKey(P2PDocumentType.None, "MaxPrecessionValueForTaxesAndCharges", UserContext.ContactCode, (int)SubAppCodes.P2P, "", 0));
                int codeCombinationFieldId = convertStringToInt(this.GetSettingsValueByKey(P2PDocumentType.None, "CodeCombinationFieldId", UserContext.ContactCode, (int)SubAppCodes.P2P, "", 0));
                string codeCombinationFieldValue = this.GetSettingsValueByKey(P2PDocumentType.None, "CodeCombinationFieldValue", UserContext.ContactCode, (int)SubAppCodes.P2P, "", 0);

                listItemCharges = GetCommonDao().SaveReqAllItemCharges(listItemCharges, precessionValue, precessionValueForTotal, precessionValueForTaxesAndCharges);
                P2PDocumentType docType = P2PDocumentType.Requisition;

                var P2PDocumentManager = new P2PDocumentManager()
                {
                    UserContext = UserContext,
                    GepConfiguration = GepConfiguration,
                    jwtToken = this.JWTToken
                };

                if (P2PDocumentManager != null && listItemCharges[0].IsHeaderLevelCharge)
                {
                    var documentSplitItemEntities = P2PDocumentManager.GetDocumentDefaultAccountingDetails(docType, LevelType.ItemLevel, UserContext.ContactCode, listItemCharges[0].DocumentCode);
                    GetReqDao().SaveChargeDefaultAccountingDetails(listItemCharges[0].DocumentCode, documentSplitItemEntities, codeCombinationFieldId, codeCombinationFieldValue);
                }

            }
            catch (CommunicationException ex)
            {
                LogHelper.LogError(Log, "SaveAllItemCharges for document code=" + listItemCharges[0].DocumentCode + " " + ex.Message, ex);
            }
            return listItemCharges;
        }

        public List<ChargeMaster> GetAllChargeName(long entityDetailCode, bool restrictChargeTypesOnPOInvoiceForSupplier = false, int orderId = 0, int documentItemId = 0, int typeOfUser = 0)
        {
            try
            {
                return GetCommonDao().GetAllChargeName(entityDetailCode, restrictChargeTypesOnPOInvoiceForSupplier, orderId, documentItemId, typeOfUser);
            }
            catch (CommunicationException ex)
            {
                LogHelper.LogError(Log, "GetAllChargeName for EntityDetailCode=" + entityDetailCode +
                     ", restrictChargeTypesOnPOInvoiceForSupplier=" + restrictChargeTypesOnPOInvoiceForSupplier +
                     ", orderId=" + orderId + ", documentItemId=" + documentItemId +
                     ", typeOfUser" + typeOfUser + " " + ex.Message, ex);
            }
            return null;
        }

        public string GetDefaultCurrency()
        {
            #region Debug Logging
            if (Log.IsDebugEnabled)
            {
                Log.Debug(string.Concat("In GetDefaultCurrency Method in RequisitionManager"));
            }
            #endregion

            string toCurrency = string.Empty;
            var oCurrency = new Gep.Cumulus.CSM.Entities.Currency();
            oCurrency.CultureCode = UserContext.Culture;

            try
            {
                var authToken = this.JWTToken;
                var request = HttpClientHelper.BuildRestAPIURLWithAuthToken(URLs.CurrencyServiceURL, URLs.GetCurrencyAutoSuggestFunction, authToken, this.UserContext);

                P2P.BusinessEntities.Currency.CurrencyAutoSuggestFilterModel objCurrency = new P2P.BusinessEntities.Currency.CurrencyAutoSuggestFilterModel("", 1000)
                {
                    CultureCode = this.UserContext.Culture
                };
                using (System.Net.Http.HttpClient httpRequest = new System.Net.Http.HttpClient())
                {
                    var response = HttpClientHelper.ExecutePostAsnycWithModel(httpRequest, request.ServiceURL, objCurrency, request.Headers, 300).Result;
                    if (!string.IsNullOrEmpty(response))
                    {
                        var lstAllCurrencys = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Gep.Cumulus.CSM.Entities.Currency>>(response);
                        lstAllCurrencys = lstAllCurrencys.Where(x => (x.IsActive == true) && (x.IsDefault == true)).ToList();
                        if (lstAllCurrencys.Count > 0)
                            toCurrency = Convert.ToString(lstAllCurrencys[0].CurrencyCode);
                    }
                }
            }
            catch (CommunicationException ex)
            {
                LogHelper.LogError(Log, "Communication Error occured in GetDefaultCurrency Method in RequisitionCommonManager with BuyerPartnerCode=" + UserContext.BuyerPartnerCode, ex);
                throw;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in RequisitionCommonManager GetDefaultCurrency Method with BuyerPartnerCode=" + UserContext.BuyerPartnerCode, ex);
                throw;
            }
            finally
            {

            }
            return toCurrency;
        }

        public void SaveCommentsFromInterface(List<Comments> lstComments, P2PDocumentType documentType,
                                              long contactCode, long objectId,
                                              int commentLevel, string commentGropText, bool isIndexingRequired = false)
        {
            FileManagerEntities.FileDetails file;
            List<long> fileIDs = new List<long>();

            if (lstComments != null && lstComments.Any())
            {
                CommentsGroup objCommentsGroup = new CommentsGroup();
                objCommentsGroup = GetComments(objectId, documentType, "1,2,4", commentLevel, 0);
                foreach (var comment in lstComments)
                {
                    fileIDs.Clear();
                    List<string> encryptedFileIds = new List<string>();
                    var lstComment = new List<Comments>();
                    lstComment.Add(new Comments()
                    {
                        CommentText = comment.CommentText,
                        CommentType = !string.IsNullOrWhiteSpace(comment.CommentType) ? comment.CommentType : "1",
                        AccessType = !string.IsNullOrEmpty(comment.AccessType) ? GetCommentAccessTypeFromInterface(comment.AccessType) : "4",
                        IsSubmitted = true,
                        CreatedBy = contactCode,
                        IsDeleteEnable = true
                    });
                    objCommentsGroup.ObjectID = objectId;
                    objCommentsGroup.CreatedBy = contactCode;
                    objCommentsGroup.GroupText = commentGropText;
                    objCommentsGroup.Comment = lstComment;
                    objCommentsGroup.TotalComments = lstComments.Count;
                    objCommentsGroup = SaveComments(objCommentsGroup, documentType, commentLevel);

                    if (comment.CommentAttachment != null && comment.CommentAttachment.Any())
                    {
                        foreach (var attachment in comment.CommentAttachment)
                        {
                            file = UploadAndSaveFile(attachment.FileUri, attachment.FileName);

                            if (file != null && file.FileId > 0)
                                fileIDs.Add(file.FileId);

                            if (!string.IsNullOrEmpty(file.EncryptedFileId))
                            {
                                encryptedFileIds.Add(file.EncryptedFileId);
                            }
                        }

                        if (objCommentsGroup.Comment != null && objCommentsGroup.Comment.Any() && fileIDs.Any())
                        {
                            var body = new
                            {
                                commentID = objCommentsGroup.Comment.FirstOrDefault().CommentID,
                                fileIds = encryptedFileIds
                            };
                            var commentHelper = new RESTAPIHelper.CommentHelper(this.UserContext, JWTToken);
                            commentHelper.SaveCommentAttachments(body);
                        }
                    }
                }

                if (isIndexingRequired)
                {
                    AddIntoSearchIndexerQueueing(objectId, (int)documentType);
                }
            }
        }

        private string GetCommentAccessTypeFromInterface(string accessType)
        {
            string accessTypeId = "";

            switch (accessType.ToLower())
            {
                case "internalusers":
                    accessTypeId = "1";
                    break;
                case "approvers":
                    accessTypeId = "2";
                    break;
                case "supplier":
                    accessTypeId = "3";
                    break;
                case "internalusersandsupplier":
                    accessTypeId = "4";
                    break;
                case "customaccess":
                    accessTypeId = "5";
                    break;
                case "buyers":
                    accessTypeId = "6";
                    break;
                case "requesters":
                    accessTypeId = "7";
                    break;
                case "payableusers":
                    accessTypeId = "8";
                    break;
                default:
                    accessTypeId = "4";
                    break;
            }

            return accessTypeId;
        }

        public CommentsGroup SaveComments(CommentsGroup objCommentsGroup, P2PDocumentType docType, int level)
        {
            if (docType == P2PDocumentType.Order && level == 1)
                objCommentsGroup.ObjectType = "GEP.Cumulus.P2P.Order";
            else if (docType == P2PDocumentType.Order && level == 2)
                objCommentsGroup.ObjectType = "GEP.Cumulus.P2P.Order.LineItem";
            else if (docType == P2PDocumentType.Invoice && level == 1)
                objCommentsGroup.ObjectType = "GEP.Cumulus.P2P.Invoice";
            else if (docType == P2PDocumentType.Invoice && level == 2)
                objCommentsGroup.ObjectType = "GEP.Cumulus.P2P.Invoice.LineItem";
            else if (docType == P2PDocumentType.Receipt && level == 1)
                objCommentsGroup.ObjectType = "GEP.Cumulus.P2P.Receipt";
            else if (docType == P2PDocumentType.Receipt && level == 2)
                objCommentsGroup.ObjectType = "GEP.Cumulus.P2P.Receipt.LineItem";
            else if (docType == P2PDocumentType.Requisition && level == 1)
                objCommentsGroup.ObjectType = "GEP.Cumulus.P2P.Requisition";
            else if (docType == P2PDocumentType.Requisition && level == 2)
                objCommentsGroup.ObjectType = "GEP.Cumulus.P2P.Requisition.LineItem";
            else if (docType == P2PDocumentType.InvoiceReconciliation && level == 1)
                objCommentsGroup.ObjectType = "GEP.Cumulus.P2P.IR";
            else if (docType == P2PDocumentType.InvoiceReconciliation && level == 2)
                objCommentsGroup.ObjectType = "GEP.Cumulus.P2P.IR.LineItem";
            else if (docType == P2PDocumentType.CreditMemo && level == 1)
                objCommentsGroup.ObjectType = "GEP.Cumulus.P2P.CreditMemo";
            else if (docType == P2PDocumentType.CreditMemo && level == 2)
                objCommentsGroup.ObjectType = "GEP.Cumulus.P2P.CreditMemo.LineItem";
            else if (docType == P2PDocumentType.ReturnNote && level == 1)
                objCommentsGroup.ObjectType = "GEP.Cumulus.P2P.ReturnNote";
            else if (docType == P2PDocumentType.ReturnNote && level == 2)
                objCommentsGroup.ObjectType = "GEP.Cumulus.P2P.ReturnNote.LineItem";

            var commentHelper = new RESTAPIHelper.CommentHelper(this.UserContext, JWTToken);
            return commentHelper.SaveComment(objCommentsGroup);
        }

        public List<ItemCharge> GetItemChargesByDocumentCode(int documentTypeCode, long documentCode, long P2PLineItemId)
        {
            try
            {
                return GetCommonDao().GetItemChargesByDocumentCode(documentTypeCode, documentCode, P2PLineItemId);
            }
            catch (CommunicationException ex)
            {
                LogHelper.LogError(Log, "GetItemChargesByDocumentCode for document code=" + documentCode + " " + ex.Message, ex);
            }
            return null;
        }

        public string GetStringValueForNotificationEmails()
        {
            UserExecutionContext userExecutionContext = UserContext;
            string message = GetSettingsValueByKey(P2PDocumentType.None, "NotificationTemplateMessageString", userExecutionContext.ContactCode);
            return string.IsNullOrEmpty(message) ? "" : message;
        }

        public DataSet GetAccountingDataByDocumentCode(long documentCode, string accountingEntities, int doctype = 0)
        {
            return GetCommonDao().GetAccountingDataByDocumentCode(documentCode, accountingEntities, doctype);
        }

        public List<ServiceType> GetPurchaseTypeItemExtendedTypeMapping()
        {
            return GetCommonDao().GetPurchaseTypeItemExtendedTypeMapping();
        }

        public PartnerLinkedLocationMapping GetLinkedLocationBySourceSystemValue(string SourceSystemValue)
        {

            PartnerLinkedLocationMapping pllm = new PartnerLinkedLocationMapping();
            PartnerLinkedLocationMapping setting = GEPDataCache.GetFromCacheJSON<PartnerLinkedLocationMapping>(String.Concat("InterfaceSettings_Partner_", SourceSystemValue), UserContext.BuyerPartnerCode, "en-US");
            if (setting == null)
            {
                try
                {
                    ProxyPartnerService proxyPartnerService = new ProxyPartnerService(UserContext, this.JWTToken);
                    // Get Partner code from Client Partner Code sent through Interface
                    pllm = proxyPartnerService.GetLinkedLocationBySourceSystemValue(SourceSystemValue);

                    GEPDataCache.PutInCacheJSON<PartnerLinkedLocationMapping>(String.Concat("InterfaceSettings_Partner_", SourceSystemValue), UserContext.BuyerPartnerCode, "en-US", pllm);
                }
                catch
                {
                    GEPDataCache.RemoveFromCache(String.Concat("InterfaceSettings_Partner_", SourceSystemValue), UserContext.BuyerPartnerCode, "en-US");
                    throw;
                }

            }
            else
                pllm = setting;
            return pllm;
        }

        public List<ApproverDetails> GetActionersDetails(long documentCode, int wfDocTypeId)
        {
            List<ApproverDetails> lstApproverDetails = new List<ApproverDetails>();
            HttpWebRequest service = null;
            LogHelper.LogInfo(Log, "GetActionersDetails Method Started for order id =" + documentCode);
            try
            {
                string serviceurl = MultiRegionConfig.GetConfig(CloudConfig.WorkFlowRestURL) + "/GetActionersList";
                CreateHttpWebRequest(serviceurl, ref service);

                Dictionary<string, object> odict = new Dictionary<string, object>();
                odict.Add("documentCode", documentCode);
                odict.Add("wfDocTypeId", wfDocTypeId);
                string result = null;
                result = GetHttpWebResponse(odict, service);

                var jSrz = new JavaScriptSerializer();
                lstApproverDetails = jSrz.Deserialize<List<ApproverDetails>>(result);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in GetActionersDetails Method in RequisitionCommonManager", ex);
                throw;
            }
            return lstApproverDetails;
        }

        public void CreateHttpWebRequest(string strURL, ref HttpWebRequest service)
        {
            service = WebRequest.Create(strURL) as HttpWebRequest;
            service.Method = "POST";
            service.ContentType = @"application/json";

            NameValueCollection nameValueCollection = new NameValueCollection();
            string userName = UserContext.UserName;
            string clientName = UserContext.ClientName;
            UserContext.UserName = string.Empty;
            UserContext.ClientName = string.Empty;
            nameValueCollection.Add("UserExecutionContext", UserContext.ToJSON());
            nameValueCollection.Add("Authorization", this.JWTToken);
            service.Headers.Add(nameValueCollection);
            UserContext.UserName = userName;
            UserContext.ClientName = clientName;
        }

        public string GetHttpWebResponse(Dictionary<string, object> odict, HttpWebRequest service)
        {
            JavaScriptSerializer JSrz = new JavaScriptSerializer();
            var data = JSrz.Serialize(odict);
            var byteData = Encoding.UTF8.GetBytes(data);


            service.ContentLength = byteData.Length;
            using (Stream stream = service.GetRequestStream())
            {
                stream.Write(byteData, 0, byteData.Length);
            }

            string result = null;
            using (HttpWebResponse resp = service.GetResponse() as HttpWebResponse)
            {
                using (StreamReader reader = new StreamReader(resp.GetResponseStream()))
                {
                    result = reader.ReadToEnd();
                }
            }
            return result;
        }

        public ShiptoLocation GetShipToLocationById(int id, long entityDetailCode, long LOBEntityDetailCode)
        {
            if (id > 0)
                return GetCommonDao().GetShipToLocationById(id, entityDetailCode, LOBEntityDetailCode);
            else
                return new ShiptoLocation();
        }

        public List<PASMasterData> GetAllCategoriesByLOB(long LOBEntityDetailCode)
        {
            return GetCommonDao().GetAllCategoriesByLOB(LOBEntityDetailCode);
        }

        public List<ContactInfo> GetAllSupplierByLOBAndOrgEntity(long LOBEntityDetailCode, string OrgEntityCode)
        {
            return GetCommonDao().GetAllSupplierByLOBAndOrgEntity(LOBEntityDetailCode, OrgEntityCode);
        }

        public Requisition GetRequisitionDetailsForBulkUploadReqLines(long requisitionId)
        {
            return GetCommonDao().GetRequisitionDetailsForBulkUploadReqLines(requisitionId);
        }

        public RequisitionItem ValidateItemNumber(string BuyerItemNumber, string SupplierItemNumber, long PartnerCode)
        {
            return GetCommonDao().ValidateItemNumber(BuyerItemNumber, SupplierItemNumber, PartnerCode);
        }

        public List<PartnerLocation> GetAllSupplierLocationsByLOBAndOrgEntity(long LOBEntityDetailCode, string OrgEntityCode, int Partnerlocation = 2)
        {
            return GetCommonDao().GetAllSupplierLocationsByLOBAndOrgEntity(LOBEntityDetailCode, OrgEntityCode, Partnerlocation);
        }

        public List<KeyValue> GetAllUOMList()
        {
            List<KeyValue> lstListP2PInvoiceMasterInfo = new List<KeyValue>();
            KeyValue objKeyValue = new KeyValue();
            var lstUoms = GetCommonDao().GetAllUOMs("", 0, 1000);
            var settingDetails = GetSettingsFromSettingsComponent(P2PDocumentType.None, this.UserContext.ContactCode, 107);
            string defaultUOMCode = "EA";
            string defaultUOMDescription = "each";
            if (settingDetails != null)
            {
                string MaterialDefaultUOM = GetSettingsValueByKey(settingDetails, "DefaultUOMforMaterialItem") != ""
                                         ? Convert.ToString(GetSettingsValueByKey(settingDetails, "DefaultUOMforMaterialItem"),
                                             CultureInfo.InvariantCulture)
                                         : "EA|each";
                defaultUOMCode = MaterialDefaultUOM.Split('|')[0].ToString(CultureInfo.InvariantCulture);
                defaultUOMDescription = MaterialDefaultUOM.Split('|')[1].ToString(CultureInfo.InvariantCulture);
            }
            int i = 1;
            foreach (var uom in lstUoms)
            {
                objKeyValue = new KeyValue();
                objKeyValue.Id = i++;
                objKeyValue.Name = uom.Value;
                objKeyValue.Value = uom.Key;
                objKeyValue.IsDefault = (uom.Value == defaultUOMDescription && uom.Key == defaultUOMCode) ? true : false;
                lstListP2PInvoiceMasterInfo.Add(objKeyValue);
            }
            return lstListP2PInvoiceMasterInfo;
        }

        public List<KeyValuePair<string, string>> GetCustomFields(long objectId, long formId, Dictionary<string, Dictionary<string, List<string>>> tableValue, long categoryId = 0, List<Question> lstQuestion = null)
        {
            List<KeyValuePair<string, string>> quesRespList = new List<KeyValuePair<string, string>>();
            bool isQuestionSetPaging = false;
            try
            {
                if (formId == 0 && categoryId > 0 && lstQuestion == null)
                    formId = GetCustomAttrFormId((int)DocumentType.Catelog, new List<Level>() { Level.Category }, categoryId)[0].Value;

                if (formId > 0 && objectId > 0)
                {
                    if (lstQuestion == null)
                    {
                        isQuestionSetPaging = true;
                        lstQuestion = new List<Question>();
                        GEP.Cumulus.PartnerManagement.BusinessObjects.FormBO frmBO = new PartnerManagement.BusinessObjects.FormBO { UserContext = UserContext, GepConfiguration = GepConfiguration };
                        GEP.Cumulus.PartnerManagement.Entities.Tab objTab = new GEP.Cumulus.PartnerManagement.Entities.Tab();
                        List<GEP.Cumulus.PartnerManagement.Entities.Tab> lstTab = new List<GEP.Cumulus.PartnerManagement.Entities.Tab>();
                        objTab.FormCode = formId;
                        lstTab = frmBO.GetTab(objTab, true);
                        if (lstTab.Count > 0)
                        {
                            foreach (var qSet in lstTab[0].Questionaries)
                            {
                                lstQuestion.AddRange(GetQuestions(objectId, qSet.QuestionnaireCode));
                            }
                        }
                    }
                    quesRespList = FillFields(objectId, lstQuestion, isQuestionSetPaging);
                    try
                    {
                        if (tableValue != null)
                        {
                            var result = FillTableFields(objectId, lstQuestion);

                            if (result != null)
                            {
                                foreach (var x in result)
                                {
                                    tableValue.Add(x.Key, x.Value);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.LogError(Log, "Error occured in FillTableFields Method in RequisitionCommonManager", ex);
                        throw ex;
                    }
                }
            }
            catch (CommunicationException ex)
            {
                LogHelper.LogError(Log, "Error occured in GetCustomFields Method in RequisitionCommonManager for objectId = " + objectId + "formId" + formId + "categoryId" + categoryId, ex);
            }
            return quesRespList;
        }

        private Dictionary<string, Dictionary<string, List<string>>> FillTableFields(long objectId, List<Question> lstQuestion)
        {

            Dictionary<string, Dictionary<string, List<string>>> tableList = new Dictionary<string, Dictionary<string, List<string>>>();

            foreach (var q in lstQuestion)
            {
                if (q.QuestionTypeInfo.QuestionTypeId == 16)
                {
                    int maxRow = 0;

                    foreach (var response in q.ListQuestionResponses)
                    {
                        if (maxRow <= response.RowId)
                        {
                            maxRow = (int)response.RowId;
                        }
                    }
                    var currentTable = new Dictionary<string, List<string>>();

                    foreach (QuestionColumnChoice question in ((MultipleMatrixQuestion)q).ColumnChoices)
                    {
                        if (!currentTable.ContainsKey(question.ColumnText))
                        {
                            currentTable.Add(question.ColumnText, new List<string>());
                            for (int i = 0; i < maxRow; i++)
                            {
                                currentTable[question.ColumnText].Add(String.Empty);
                            }
                        }
                        foreach (var response in q.ListQuestionResponses.Where(x => x.ColumnId == question.ColumnId))
                        {
                            if (question.ColumnType == ColumnType.Dropdown)
                            {
                                currentTable[question.ColumnText][(int)response.RowId - 1] = question.ListMatrixCellChoices.Where(x => x.CellChoiceId == Convert.ToInt32(response.ResponseValue)).Select(y => y.ChoiceValue).FirstOrDefault() ?? String.Empty;
                            }
                            else
                            {
                                currentTable[question.ColumnText][(int)response.RowId - 1] = response.ResponseValue ?? String.Empty;
                            }
                        }
                    }
                    tableList.Add(q.QuestionText, currentTable);
                }
            }

            return tableList;
        }
        private List<KeyValuePair<string, string>> FillFields(long objectId, List<Question> lstQuestion, bool isQuestionSetPaging = true)
        {
            List<KeyValuePair<string, string>> quesRespList = new List<KeyValuePair<string, string>>();
            foreach (var q in lstQuestion)
            {
                if (q.QuestionTypeInfo.QuestionTypeId == 16)
                {
                    continue;
                }
                string value = GetFieldVal(q);
                if (value != "NA")
                    quesRespList.Add(new KeyValuePair<string, string>(q.QuestionText, value));
                if (isQuestionSetPaging)
                    quesRespList.AddRange(FillFields(objectId, GetChildQuestions(q, objectId)));
            }
            return quesRespList;
        }

        private string GetFieldVal(Question ques)
        {
            string result = "";
            if (ques.QuestionTypeInfo.QuestionTypeId != 18)
            {
                switch (ques.QuestionTypeInfo.QuestionTypeId)
                {
                    case 1:
                    case 2:
                    case 17:
                    case 28:
                        if (ques.ListQuestionResponses.Count > 0)
                        {
                            foreach (QuestionResponse objQuestionResponse in ques.ListQuestionResponses)
                            {
                                if (objQuestionResponse.ResponseValue.Split('\n').Length > 0)
                                {
                                    foreach (string strResponse in objQuestionResponse.ResponseValue.Split('\n'))
                                    {
                                        if (strResponse != "")
                                            result = result != "" ? result += "\n" : result;
                                        result += strResponse;
                                    }
                                }
                            }
                        }
                        break;
                    case 6:
                        if (((MultipleChoiceQuestion)ques).RowChoices.Count > 0)
                        {
                            foreach (QuestionRowChoice objQuestionRowChoice in ((MultipleChoiceQuestion)ques).RowChoices)
                            {
                                bool isAnswerSelected = ques.ListQuestionResponses.Exists(qr => qr.RowId == objQuestionRowChoice.RowId);
                                if (isAnswerSelected)
                                {
                                    if (objQuestionRowChoice.RowText != "")
                                        result = result != "" ? result += ", " : result;
                                    result += objQuestionRowChoice.RowText;
                                }
                            }
                        }
                        break;
                    case 7:
                        if (((MultipleChoiceQuestion)ques).ListQuestionResponses.Count > 0)
                        {
                            foreach (QuestionResponse objResp in ((MultipleChoiceQuestion)ques).ListQuestionResponses)
                            {
                                QuestionRowChoice QRChoice = ((MultipleChoiceQuestion)ques).RowChoices.FirstOrDefault(rc => rc.RowId == objResp.RowId);
                                if (QRChoice != null)
                                {
                                    if (QRChoice.RowText != "")
                                        result = result != "" ? result += ", " : result;
                                    result += QRChoice.RowText.Length > 100 ? QRChoice.RowText.Substring(0, 97) + "..." : QRChoice.RowText;
                                }
                            }
                        }
                        break;
                    case 8:
                        if (((MultipleChoiceQuestion)ques).RowChoices.Count > 0)
                        {
                            foreach (QuestionRowChoice objQuestionRowChoice in ((MultipleChoiceQuestion)ques).RowChoices)
                            {
                                bool isAnswerSelected = ques.ListQuestionResponses.Exists(qr => qr.RowId == objQuestionRowChoice.RowId);
                                if (isAnswerSelected)
                                {
                                    if (objQuestionRowChoice.RowText != "")
                                        result = result != "" ? result += ", " : result;
                                    result += objQuestionRowChoice.RowText;
                                }
                            }
                        }
                        break;
                    case 9:
                        if (((MultipleChoiceQuestion)ques).ListQuestionResponses.Count > 0)
                        {
                            foreach (QuestionResponse objResp in ((MultipleChoiceQuestion)ques).ListQuestionResponses)
                            {
                                QuestionRowChoice QRChoice = ((MultipleChoiceQuestion)ques).RowChoices.FirstOrDefault(rc => rc.RowId == objResp.RowId);
                                if (QRChoice != null)
                                {
                                    if (QRChoice.RowText != "")
                                        result = result != "" ? result += ", " : result;
                                    result += QRChoice.RowText;
                                }
                            }
                        }
                        break;
                    case 10:
                        string strDateTimeFormat = "dd MMM yyyy";
                        switch (((DateTimeQuestion)ques).DateTimeFormat)
                        {
                            case DateTimeFormat.DDMMYYYY:
                                strDateTimeFormat = "dd/MM/yy";
                                break;
                            case DateTimeFormat.MMDDYYYY:
                                strDateTimeFormat = "MM/dd/yy";
                                break;
                        }
                        DateTime QuestionRespDt;
                        foreach (QuestionResponse objResp in ((DateTimeQuestion)ques).ListQuestionResponses)
                        {
                            result = result != "" ? result += ", " : result;
                            if (DateTime.TryParse(objResp.ResponseValue, out QuestionRespDt))
                            {
                                result += QuestionRespDt.ToString(strDateTimeFormat);
                            }
                            else
                            {
                                result += objResp.ResponseValue;
                            }
                        }
                        break;
                    case 19:
                    case 20:
                    case 21:
                    case 22:
                    case 23:
                    case 24:
                    case 25:
                    case 26:
                    case 27:
                        result = GenerateDBLookUpQuestion((DBLookUpQuestion)ques);
                        break;
                    case 14:
                        foreach (QuestionResponseAttachment objQuestionResponseAttachement in ques.ListQuestionResponseAttachment)
                        {
                            result = result != "" ? result += ", " : result;
                            result += objQuestionResponseAttachement.FileName;
                        }
                        break;
                    default:
                        result = "NA";
                        break;
                }
            }

            if (result == "")
                result = " - ";
            return result;
        }

        private string GenerateDBLookUpQuestion(DBLookUpQuestion objDBLookUpQuestion)
        {
            string result = "";
            if (objDBLookUpQuestion.ListQuestionResponses.Count > 0)
            {
                QuestionResponse objResp = objDBLookUpQuestion.ListQuestionResponses[0];
                switch (objDBLookUpQuestion.QuestionTypeInfo.QuestionTypeId)
                {
                    case 19:
                    case 20:
                        //category
                        if (objDBLookUpQuestion.QuestionTypeInfo.QuestionTypeId == 19)
                        {
                            var getCategoriesByCategoryCodesRequest = new List<long>();
                            List<PASEntities.PASMaster> pasLst = new List<PASEntities.PASMaster>();
                            foreach (var cat in objResp.ResponseValue.Split(',').ToList())
                            {
                                pasLst.Add(new PASEntities.PASMaster()
                                {
                                    PASCode = Convert.ToInt64(cat),
                                    CompanyName = this.UserContext.CompanyName
                                });
                                getCategoriesByCategoryCodesRequest.Add(Convert.ToInt64(cat));
                            }

                            var csmHelper = new RESTAPIHelper.CSMHelper(UserContext, JWTToken);
                            var body = new
                            {
                                CategoryCodes = getCategoriesByCategoryCodesRequest
                            };
                            pasLst = csmHelper.GetCategoriesByCategoryCodes(body);

                            foreach (var pas in pasLst)
                            {
                                result = result != "" ? result += ", " : result;
                                result += pas.PASName;
                            }
                        }
                        //region
                        //else
                        //{
                        //    UserExecutionContext userExecutionContext = UserContext;
                        //    ProxyCSMService csm = new ProxyCSMService(userExecutionContext);
                        //    var regLst = csm.GetRegionDetailsForRegionIds(objResp.ResponseValue);
                        //    foreach (var reg in regLst)
                        //    {
                        //        result = result != "" ? result += ", " : result;
                        //        result += reg.RegionName;
                        //    }
                        //}
                        break;
                    //Business Unit
                    case 25:
                    //Partner type
                    case 26:
                        ProxyPartnerService proxyPartnerService = new ProxyPartnerService(UserContext, this.JWTToken);
                        if (objDBLookUpQuestion.QuestionTypeInfo.QuestionTypeId == 25)
                        {
                            string[] selectedPartnersArr = objResp.ResponseValue.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            string selectedPartners = string.Empty;
                            for (int i = 0; i < selectedPartnersArr.Length; i++)
                            {
                                long partnerCode = Convert.ToInt64(selectedPartnersArr[i]);
                                if (partnerCode > 0)
                                {
                                    PartnerDetails objPartnerDetails = proxyPartnerService.GetPartnerDetails(partnerCode, P2PDocumentManager.DEFAULT_CULTURECODE);
                                    result = result != "" ? result += ", " : result;
                                    result += objPartnerDetails.LegalCompanyName;
                                }
                            }
                        }
                        else if (objDBLookUpQuestion.QuestionTypeInfo.QuestionTypeId == 26)
                        {
                            long contactCode = Convert.ToInt64(objResp.ResponseValue);
                            if (contactCode > 0)
                            {
                                Contact objContact = proxyPartnerService.GetContactByContactCode(0, contactCode);
                                result = result != "" ? result += ", " : result;
                                result += (objContact.FirstName + " " + objContact.LastName);
                            }
                        }
                        break;

                    case 21:
                        {
                            long OrgEntityCode = Convert.ToInt64(objResp.ResponseValue);
                            if (OrgEntityCode > 0)
                            {
                                UserExecutionContext userExecutionContext = UserContext;
                                ProxyOrganizationStructureService poxyOrganizationStructureService = new ProxyOrganizationStructureService(UserContext, this.JWTToken);
                                //OrganizationStructureService.IOrganizationStructureChannel objOrgServivceChannel = null;
                                //objOrgServivceChannel = GepServiceManager.GetInstance.CreateChannel<OrganizationStructureService.IOrganizationStructureChannel>(MultiRegionConfig.GetConfig(CloudConfig.OrganizationStructureServiceURL));
                                //using (new OperationContextScope((objOrgServivceChannel)))
                                //{
                                //    var objMhg = new MessageHeader<UserExecutionContext>(userExecutionContext);
                                //    var untypedmh = objMhg.GetUntypedHeader("GepCustomHeader", "Gep.Cumulus");
                                //    OperationContext.Current.OutgoingMessageHeaders.Add(untypedmh);

                                var executionHelper = new GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.ExecutionHelper(this.UserContext, this.GepConfiguration, this.JWTToken);
                                if (executionHelper.Check(18, RESTAPIHelper.ExecutionHelper.WebAPIType.Common))
                                {
                                    var requestHeaders = new GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.RequestHeaders();
                                    requestHeaders.Set(this.UserContext, this.JWTToken);
                                    string appName = Environment.GetEnvironmentVariable("NewRelic.AppName") ?? "Requisition";
                                    string useCase = "RequisitionCommonManager-GenerateDBLookUpQuestion";
                                    var serviceURL = MultiRegionConfig.GetConfig(CloudConfig.APIMBaseURL) + GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.ServiceURLs.OrganizationServiceURL + "GetEntityDetails";

                                    var body = new
                                    {
                                        OrgEntityCode = OrgEntityCode,
                                        EntityId = 7
                                    };
                                    var webAPI = new GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.WebAPI(requestHeaders, appName, useCase);
                                    var response = webAPI.ExecutePost(serviceURL, body);

                                    JObject resJson = JObject.Parse(response);
                                    if (resJson.SelectToken("EntityDetailsResponse[0].DisplayName") != null)
                                    {
                                        result = result != "" ? result += ", " : result;
                                        result += Convert.ToString(resJson.SelectToken("EntityDetailsResponse[0].DisplayName"));
                                    }
                                }
                                else
                                {
                                    List<OrganizationStructure.Entities.OrgEntity> OrgEntityList = poxyOrganizationStructureService.GetEntityDetails(
                                                                                                        new OrganizationStructure.Entities.OrgSearch()
                                                                                                        {// will need to change 7 hardcoding if multi BU change occurs in survey component
                                                                                                            OrgEntityCode = OrgEntityCode,
                                                                                                            objEntityType = new OrganizationStructure.Entities.OrgEntityType { EntityId = 7 }
                                                                                                        });
                                    result = result != "" ? result += ", " : result;
                                    result += OrgEntityList[0].objEntityType.DisplayName;
                                }
                                // }
                            }
                        }
                        break;
                    case 27:
                        {
                            long responseId = Convert.ToInt64(objResp.ResponseValue);
                            if (responseId > 0)
                            {
                                UserExecutionContext userExecutionContext = UserContext;
                                ProxySurveyComponent objProxySurveyComponent = null;
                                objProxySurveyComponent = new ProxySurveyComponent(this.UserContext, this.JWTToken);
                                objDBLookUpQuestion.DBLookUpFieldConfig.FieldId = Convert.ToInt32(objResp.ResponseValue);
                                objDBLookUpQuestion.DBLookUpFieldConfig.IsAutosuggest = true;
                                List<CustomDBLookUpQuestionData> lstCustomDBLookUpQuestionData = objProxySurveyComponent.GetCustomDBLookUpQuestionData(objDBLookUpQuestion.DBLookUpFieldConfig);
                                if (lstCustomDBLookUpQuestionData != null && lstCustomDBLookUpQuestionData.Count > 0)
                                {
                                    result = result != "" ? result += ", " : result;
                                    result += lstCustomDBLookUpQuestionData[0].FieldName;
                                }

                            }
                        }
                        break;
                }
            }
            return result;
        }


        private List<Question> GetChildQuestions(Question ques, long objectId)
        {
            List<Question> lstQuestion = new List<Question>();
            switch (ques.QuestionTypeInfo.QuestionTypeId)
            {
                case 6:
                    if (((MultipleChoiceQuestion)ques).RowChoices.Count > 0)
                    {
                        foreach (QuestionRowChoice objQuestionRowChoice in ((MultipleChoiceQuestion)ques).RowChoices)
                        {
                            bool isAnswerSelected = ques.ListQuestionResponses.Exists(qr => qr.RowId == objQuestionRowChoice.RowId);
                            if (isAnswerSelected && objQuestionRowChoice.ChildQuestionSetCode > 0)
                                lstQuestion.AddRange(GetQuestions(objectId, objQuestionRowChoice.ChildQuestionSetCode));
                        }
                    }
                    break;
                case 7:
                    if (((MultipleChoiceQuestion)ques).ListQuestionResponses.Count > 0)
                    {
                        foreach (QuestionResponse objResp in ((MultipleChoiceQuestion)ques).ListQuestionResponses)
                        {
                            QuestionRowChoice QRChoice = ((MultipleChoiceQuestion)ques).RowChoices.FirstOrDefault(rc => rc.RowId == objResp.RowId);
                            if (QRChoice != null)
                            {
                                if (QRChoice.ChildQuestionSetCode > 0)
                                    lstQuestion.AddRange(GetQuestions(objectId, QRChoice.ChildQuestionSetCode));
                            }
                        }
                    }
                    break;
                case 8:
                    if (((MultipleChoiceQuestion)ques).RowChoices.Count > 0)
                    {
                        foreach (QuestionRowChoice objQuestionRowChoice in ((MultipleChoiceQuestion)ques).RowChoices)
                        {
                            bool isAnswerSelected = ques.ListQuestionResponses.Exists(qr => qr.RowId == objQuestionRowChoice.RowId);
                            if (isAnswerSelected && objQuestionRowChoice.ChildQuestionSetCode > 0)
                                lstQuestion.AddRange(GetQuestions(objectId, objQuestionRowChoice.ChildQuestionSetCode));
                        }
                    }
                    break;
                case 9:
                    if (((MultipleChoiceQuestion)ques).ListQuestionResponses.Count > 0)
                    {
                        foreach (QuestionResponse objResp in ((MultipleChoiceQuestion)ques).ListQuestionResponses)
                        {
                            QuestionRowChoice QRChoice = ((MultipleChoiceQuestion)ques).RowChoices.FirstOrDefault(rc => rc.RowId == objResp.RowId);
                            if (QRChoice != null)
                            {
                                if (QRChoice.ChildQuestionSetCode > 0)
                                    lstQuestion.AddRange(GetQuestions(objectId, QRChoice.ChildQuestionSetCode));
                            }
                        }
                    }
                    break;
            }
            return lstQuestion;
        }

        public ICollection<string> GetAllShippingMethods(string searchText, int pageIndex, int pageSize, long ACEEntityDetailCode, long LOBEntityDetailCode)
        {
            return GetCommonDao().GetAllShippingMethods(searchText, pageIndex, pageSize, ACEEntityDetailCode, LOBEntityDetailCode);
        }

        private List<Question> GetQuestions(long objectId, long qSet)
        {
            Question objQuestion = new Question();
            GEP.Cumulus.QuestionBank.BusinessObjects.QuestionResponseBO qBO = new GEP.Cumulus.QuestionBank.BusinessObjects.QuestionResponseBO { UserContext = UserContext, GepConfiguration = GepConfiguration };
            objQuestion.QuestionSetCode = qSet;
            objQuestion.PageIndex = 0;
            objQuestion.PageSize = 0;
            objQuestion.QuestionId = 0;
            objQuestion.CultureCode = "";
            objQuestion.CompanyName = "BuyerSqlConn";
            return qBO.GetQuestionWithResponsesByQuestionSetPaging(objQuestion, -1, -1, objectId, AssessorUserType.Buyer, false);
        }

        public List<P2PCustomValidationByLevel> ValidateItemCustomFields(int docType, long docId, bool checkOnly = false, long formId = 0, bool filterOnUser = true, bool checkedItemsOnly = false)
        {
            List<P2PCustomValidationByLevel> result = new List<P2PCustomValidationByLevel>();
            try
            {
                result = GetCommonDao().ValidateCustomFields(docType, docId, formId, filterOnUser, checkedItemsOnly,
                    ((byte)Level.Item).ToString(), 0, 0, 0, 0, 0, UserContext.IsSupplier, (int)Level.Item, true);
            }
            catch (CommunicationException ex)
            {
                LogHelper.LogError(Log, "Error occured in ValidateItemCustomFields Method in P2PCommonManager for docId = " + docId + "docType" + docType + "checkOnly" + checkOnly, ex);
            }
            return result;
        }

        public List<P2PCustomValidationByLevel> ValidateSplitCustomFields(int docType, long docId, bool checkOnly = false, long formId = 0, bool filterOnUser = true, bool checkedItemsOnly = false)
        {
            List<P2PCustomValidationByLevel> result = new List<P2PCustomValidationByLevel>();
            try
            {
                result = GetCommonDao().ValidateCustomFields(docType, docId, formId, filterOnUser, checkedItemsOnly,
                    ((byte)Level.Item).ToString(), 0, 0, 0, 0, 0, UserContext.IsSupplier, (int)Level.Distribution, false);
            }
            catch (CommunicationException ex)
            {
                LogHelper.LogError(Log, "Error occured in ValidateSplitCustomFields Method in P2PCommonManager for docId = " + docId + "docType" + docType + "checkOnly" + checkOnly, ex);
            }
            return result;
        }

        public List<SplitAccountingFields> GetAllAdditionalFields(int documentType, long documentCode, string entityTypeids, LevelType levelType)
        {
            return GetCommonDao().GetAllAdditionalFields(documentType, documentCode, entityTypeids, levelType);
        }

        public int SaveBilltoLocation(BilltoLocation objBilltoLocation)
        {
            return GetCommonDao().SaveBilltoLocation(objBilltoLocation);
        }

        public List<ShiptoLocation> GetAllShipToLocations()
        {
            return GetCommonDao().GetAllShipToLocations();
        }

        public BilltoLocation SaveBillToLocation(BilltoLocation objBillToLocation)
        {
            if (!string.IsNullOrWhiteSpace(objBillToLocation.BilltoLocationNumber))
            {
                BilltoLocation objBillToLoc = GetBillToLocationByNumber(objBillToLocation.BilltoLocationNumber);

                if (!object.ReferenceEquals(objBillToLocation, null) && objBillToLoc.BilltoLocationId > 0)
                    objBillToLocation.BilltoLocationId = objBillToLoc.BilltoLocationId;
            }
            #region 'Logic to decide whether New Bill to Location to be created if doesn't exists'


            if (objBillToLocation.BilltoLocationId == 0)
            {
                string billToLocSetting = GetSettingsValueByKey(P2PDocumentType.Interfaces, "IsNewBillToLocCreationRequired", 0, (int)SubAppCodes.Interfaces);
                if (!String.IsNullOrWhiteSpace(billToLocSetting) && Convert.ToInt32(billToLocSetting) == 1)
                {
                    var csmHelper = new RESTAPIHelper.CSMHelper(UserContext, JWTToken);
                    var body = new
                    {
                        EntityTypeName = new Address().GetType().FullName,
                        SeedIncrement = 1
                    };
                    objBillToLocation.Address.AddressId = csmHelper.GenerateNewEntityCode(body);
                    objBillToLocation.IsAdhoc = true; // To Avoid Validation which will trigger while saving the Master BillTo
                    objBillToLocation.BilltoLocationId = SaveBilltoLocation(objBillToLocation);//indexing in called function
                }
                else if (objBillToLocation.BilltoLocationId == 0)
                    throw new Exception("Invalid Bill to location.");
            }

            return objBillToLocation;

            #endregion
        }

        public BilltoLocation GetBillToLocationByNumber(string billToLocNumber, bool getDeletedOne = false)
        {
            return GetCommonDao().GetBillToLocationByNumber(billToLocNumber, getDeletedOne);
        }

        public int GetBillToLocationIdByAddress(BilltoLocation objBillToLocation, bool getDeletedOne = false)
        {
            return GetCommonDao().GetBillToLocationIdByAddress(objBillToLocation, getDeletedOne);
        }

        public DelivertoLocation GetDeliverToLocationByNumber(string deliverToNumber)
        {
            return GetCommonDao().GetDeliverToLocationByNumber(deliverToNumber);
        }

        public long GetContactCodeByClientContactCodeOrEmail(string clientContactCode, string createdByName, long partnerCode = 0)
        {
            ProxyPartnerService proxyPartnerService = new ProxyPartnerService(UserContext, this.JWTToken);
            //IPartnerChannel objPartnerServiceChannel = null;
            long contactCode = 0;
            try
            {
                //objPartnerServiceChannel = GepServiceManager.GetInstance.CreateChannel<IPartnerChannel>(MultiRegionConfig.GetConfig(CloudConfig.PartnerServiceURL));
                //using (new OperationContextScope((objPartnerServiceChannel)))
                //{
                //var objMhg =
                //    new MessageHeader<UserExecutionContext>(this.UserContext);
                //    var untypedmh = objMhg.GetUntypedHeader("GepCustomHeader", "Gep.Cumulus");
                //    OperationContext.Current.OutgoingMessageHeaders.Add(untypedmh);

                // Get Partner code from Client Partner Code sent through Interface
                contactCode = proxyPartnerService.GetContactCodeByClientContactCodeOrEmail(clientContactCode, createdByName, partnerCode > 0 ? partnerCode : this.UserContext.BuyerPartnerCode);
                //}
            }
            finally
            {
                //DisposeService(objPartnerServiceChannel);
            }
            return contactCode;
        }

        public long GetPartnerCodeByClientPartnerCode(string clientPartnerCode, string sourceSystemName = "")
        {
            long partnerCode = 0;
            string setting = GEPDataCache.GetFromCacheJSON<string>(String.Concat("InterfaceSettings_Partner_", clientPartnerCode, sourceSystemName), UserContext.BuyerPartnerCode, "en-US");
            if (setting == null)
            {
                try
                {
                    ProxyPartnerService proxyPartnerService = new ProxyPartnerService(UserContext, this.JWTToken);

                    // Get Partner code from Client Partner Code sent through Interface
                    partnerCode = proxyPartnerService.GetPartnerCodeByClientPartnerCode(clientPartnerCode, sourceSystemName);
                    GEPDataCache.PutInCacheJSON<long>(String.Concat("InterfaceSettings_Partner_", clientPartnerCode, sourceSystemName), UserContext.BuyerPartnerCode, "en-US", partnerCode);
                }
                catch
                {
                    GEPDataCache.RemoveFromCache(String.Concat("InterfaceSettings_Partner_", clientPartnerCode, sourceSystemName), UserContext.BuyerPartnerCode, "en-US");
                    throw;
                }

            }
            else
                partnerCode = Convert.ToInt64(setting);
            return partnerCode;
        }

        public void UpdateSplitDeatilsBasedOnHeaderEntity(List<DocumentAdditionalEntityInfo> DocumentAdditionalEntitiesInfoList, DataSet result)
        {
            foreach (var item in DocumentAdditionalEntitiesInfoList)
            {
                for (int intCount = 0; intCount < result.Tables[0].Rows.Count; intCount++)
                {
                    if (item.EntityId.Equals(result.Tables[0].Rows[intCount]["EntityTypeId"]))
                    {
                        result.Tables[0].Rows[intCount]["EntityCode"] = item.EntityCode;
                        result.Tables[0].Rows[intCount]["EntityDetailCode"] = item.EntityDetailCode;

                    }
                }
            }
        }

        public List<ContactORGMapping> GetContactORGMapping(int BUEntityId = 7, long contactcode = 0)
        {
            ProxyPartnerService partnerProxy = new ProxyPartnerService(UserContext, this.JWTToken);
            List<ContactORGMapping> lstBUDetails = new List<ContactORGMapping>();
            try
            {
                ContactORGMapping cOrgMap = new ContactORGMapping()
                {
                    ContactCode = contactcode <= 0 ? this.UserContext.ContactCode : contactcode,
                    CompanyName = this.UserContext.CompanyName,
                    ClientID = this.UserContext.ClientID,
                    CultureCode = this.UserContext.Culture,
                    EntityId = BUEntityId
                };

                // Get list of BU based on Contact Code
                lstBUDetails = partnerProxy.GetContactORGMapping(cOrgMap);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in GetContactORGMapping Method in RequisitionCommonManager", ex);
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw();
                throw;
            }
            return lstBUDetails;
        }

        public List<PurchaseType> GetPurchaseTypes()
        {
            return GetCommonDao().GetPurchaseTypes();
        }

        public long GetPASCodeFromUNSPSCId(int Unspsc)
        {
            try
            {
                return GetCommonDao().GetPASCodeFromUNSPSCId(Unspsc);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in GetPASCodeFromUNSPSCId Method in P2PCommonManager", ex);
                throw;
            }
        }

        public List<SourceSystemInfo> GetAllSourceSystemInfo()
        {
            return (List<SourceSystemInfo>)GetCommonDao().GetAllSourceSystemInfo();
        }

        public List<CommentsGroup> GetCommentsForDocuments(long documentId, P2PDocumentType docType, long contactCode, List<DocumentStakeHolder> docStakeHolders, int userType)
        {
            List<CommentsGroup> lstCommentsGroup = new List<CommentsGroup>();
            CommentsGroup objCommentsGroup = new CommentsGroup();
            Comments objComment = new Comments();
            List<string> lstUserActivities = new List<string>();

            bool isAP = false;
            bool isBuyer = false;
            bool isRequester = false;

            if (contactCode != 0)
            {
                UserExecutionContext userExecutionContext = UserContext;
                var partnerHelper = new RESTAPIHelper.PartnerHelper(this.UserContext, JWTToken);
                lstUserActivities = partnerHelper.GetUserActivitiesByContactCode(contactCode, userExecutionContext.BuyerPartnerCode).Split(',').ToList();

                foreach (var activity in lstUserActivities)
                {
                    if (activity == P2PUserActivity.CREATE_INVOICE || activity == P2PUserActivity.CREATE_INVOICE_BY_FLIPPING_ORDER || activity == P2PUserActivity.CREATE_INVOICE_BY_FLIPPING_PAYMENT)
                        isAP = true;
                    else if (activity == P2PUserActivity.CREATE_REQUISITION)
                        isRequester = true;
                    else if (activity == P2PUserActivity.CREATE_ORDER || activity == P2PUserActivity.FLIP_REQUISITIONS_TO_ORDER)
                        isBuyer = true;
                }
            }

            string accessType = "4";
            if (userType != 1)
            {
                accessType += ",1";
                if (isAP)
                    accessType += ",8";
                if (isBuyer)
                    accessType += ",6";
                if (isRequester)
                    accessType += ",7";
            }

            foreach (var stakeHolder in docStakeHolders)
            {
                if (stakeHolder.StakeholderTypeInfo == StakeholderType.Approver && stakeHolder.ContactCode == UserContext.ContactCode)
                {
                    accessType += ",2";
                    break;
                }
            }

            if (docType == P2PDocumentType.Requisition)
            {
                objCommentsGroup.ObjectID = documentId;
                objCommentsGroup.ObjectType = ObjectType.REQUISITION;
                objComment.AccessType = accessType;
                objCommentsGroup.Comment = new List<Comments>();
                objCommentsGroup.Comment.Add(objComment);
                lstCommentsGroup.Add(objCommentsGroup);
            }
            else
            {
                var relatedDocument = GetRelatedDocumentsForComments(documentId, (int)GetDocumentType(docType));
                foreach (var item in relatedDocument)
                {
                    objCommentsGroup = new CommentsGroup();
                    objCommentsGroup.ObjectID = item.Key;
                    objCommentsGroup.ObjectType = item.Value;
                    objCommentsGroup.Comment = new List<Comments>();
                    Comments objCom = new Comments();
                    objCom.AccessType = accessType;
                    objCommentsGroup.Comment.Add(objCom);
                    lstCommentsGroup.Add(objCommentsGroup);
                }
            }

            List<CommentsGroupRequestModel> commentsGroupRequestModel = new List<CommentsGroupRequestModel>();
            foreach (var item in lstCommentsGroup)
            {
                commentsGroupRequestModel.Add(new CommentsGroupRequestModel()
                {
                    AccessType = accessType,
                    ObjectType = item.ObjectType,
                    ObjectID = item.ObjectID
                });
            }
            var commentHelper = new Req.BusinessObjects.RESTAPIHelper.CommentHelper(this.UserContext, JWTToken);
            var result = commentHelper.GetCommentsWithAttachments(commentsGroupRequestModel);
            lstCommentsGroup = commentHelper.Map(result);

            return lstCommentsGroup;
        }

        public ICollection<KeyValuePair<long, string>> GetRelatedDocumentsForComments(long documentCode, int documentType)
        {
            return GetCommonDao().GetRelatedDocumentsForComments(documentCode, documentType);
        }

        public bool SaveAdhocReview(AdhocReviewer adhocReviewer)
        {
            return GetCommonDao().SaveAdhocReview(adhocReviewer);

        }

        public List<OrgManagerStructureMapping> GetOrgManagerStructureMappings(List<string> Structureid)
        {
            return GetCommonDao().GetOrgManagerStructureMappings(Structureid);
        }

        public void updateUserDefinedApprovalonEntityChange(long objdocumentCode, P2PDocumentType objdocType, decimal? objTotalAmount, string objdocCurrencyCode)
        {
            Dictionary<string, object> stateparams = new Dictionary<string, object>();
            stateparams.Add("documentCode", objdocumentCode);
            stateparams.Add("docType", objdocType);
            stateparams.Add("TotalAmount", objTotalAmount);
            stateparams.Add("GepConfiguration", GepConfiguration);
            stateparams.Add("UserContext", UserContext);
            stateparams.Add("docCurrencyCode", objdocCurrencyCode);
            var currentPrincipal = System.Threading.Thread.CurrentPrincipal;
            Task.Factory.StartNew((state) =>
            {
                System.Threading.Thread.CurrentPrincipal = currentPrincipal;
                Dictionary<string, object> parameters = (Dictionary<string, object>)state;

                long documentCode = (long)parameters["documentCode"];
                P2PDocumentType docType = (P2PDocumentType)parameters["docType"];
                decimal? TotalAmount = (decimal?)parameters["TotalAmount"];
                string docCurrencyCode = (string)parameters["docCurrencyCode"];
                UserExecutionContext _userContext = (UserExecutionContext)parameters["UserContext"];
                Gep.Cumulus.CSM.Config.GepConfig _gepConfiguration = (Gep.Cumulus.CSM.Config.GepConfig)parameters["GepConfiguration"];
                UpdateUserDefinedApproval(documentCode, docType, TotalAmount, docCurrencyCode, _userContext, _gepConfiguration);

            }, stateparams);

        }

        public List<KeyValuePair<string, string>> updateUserDefinedApprovalonEntityChangeSync(long objdocumentCode, P2PDocumentType objdocType, decimal? objTotalAmount, string docCurrencyCode)
        {
            return UpdateUserDefinedApproval(objdocumentCode, objdocType, objTotalAmount, docCurrencyCode, UserContext, GepConfiguration);
        }

        private List<KeyValuePair<string, string>> UpdateUserDefinedApproval(long documentCode, P2PDocumentType docType, decimal? TotalAmount, string docCurrencyCode, UserExecutionContext _userContext, Gep.Cumulus.CSM.Config.GepConfig _gepConfiguration)
        {
            List<String> prevApprovers = new List<string>();
            List<String> updatedApprovers = new List<string>();
            var oCurrency = new Gep.Cumulus.CSM.Entities.Currency();
            string OrgEntities = "";
            int wfDocType = 0;
            DocumentType DocumentType = DocumentType.None;
            List<UserDefinedApproval> userDefinedApproval = new List<UserDefinedApproval>();
            P2PDocumentManager p2pDocumentmanger = new P2PDocumentManager { UserContext = _userContext, GepConfiguration = _gepConfiguration, jwtToken = this.JWTToken };
            try
            {
                switch (docType)
                {
                    case P2PDocumentType.Requisition:
                        wfDocType = (int)Enums.WFDocTypeId.Requisition;
                        DocumentType = DocumentType.Requisition;
                        break;
                    case P2PDocumentType.Order:
                        wfDocType = (int)Enums.WFDocTypeId.PO;
                        DocumentType = DocumentType.PO;
                        break;
                }

                int AllowFilteronApprovers = 0; string FliterEntityTypeId = "";
                var settingDetails = GetSettingsFromSettingsComponent(P2PDocumentType.None, _userContext.ContactCode, 107);
                userDefinedApproval = p2pDocumentmanger.GetUserDefinedApproversList(documentCode, wfDocType);


                string tempAllowFilteronApprovers = GetSettingsValueByKey(settingDetails, "AllowFilteronApprovers");
                AllowFilteronApprovers = String.IsNullOrEmpty(tempAllowFilteronApprovers) == true ? 0 : Convert.ToInt32(tempAllowFilteronApprovers);

                if (AllowFilteronApprovers == 1)
                {
                    string tempFliterEntityTypeId = GetSettingsValueByKey(settingDetails, "ApprovalFliterEntityTypeId");
                    FliterEntityTypeId = String.IsNullOrEmpty(tempFliterEntityTypeId) == true ? "" : Convert.ToString(tempFliterEntityTypeId);

                    if (FliterEntityTypeId != "")
                    {
                        string CurrencyCode = GetDefaultCurrency();
                        if (string.IsNullOrEmpty(docCurrencyCode))
                        {
                            P2PDocument document;
                            document = GetReqDao().GetBasicDetailsById(documentCode, _userContext.ContactCode);

                            TotalAmount = document.TotalAmount;
                            docCurrencyCode = document.Currency;
                        }
                        TotalAmount = TotalAmount * GetCurrencyConversionFactor(docCurrencyCode, CurrencyCode);

                        List<SplitAccountingFields> lstEntities = GetAllAdditionalFields((int)DocumentType, documentCode, FliterEntityTypeId, LevelType.Both);
                        OrgEntities = String.Join(",", lstEntities.Select(d => d.EntityDetailCode).ToList());

                        List<User> contactManagerInfo = new List<User>();
                        contactManagerInfo = new ProxyPartnerService(_userContext, this.JWTToken).GetSpecificUserApproversBasedOnActivityCode("", wfDocType, 0, 0, OrgEntities, (decimal)TotalAmount, "", CurrencyCode);

                        if (userDefinedApproval.Any(e => e.IsActive == false && e.IsDeleted == false && e.IsProcessed == false))
                        {
                            userDefinedApproval = userDefinedApproval.Where(e => e.IsActive == false && e.IsDeleted == false && e.IsProcessed == false && e.WorkflowId == 12).ToList();

                            foreach (var userDefinedLevel in userDefinedApproval)
                            {
                                String[] approverslist = userDefinedLevel.WorkflowSettings.Where(e => e.SettingName == "SpecificUsers").First().SettingValue.Split(',');
                                prevApprovers.AddRange(approverslist.ToList());
                                var updatedSettingValue = String.Join(",", approverslist.Where(d => contactManagerInfo.Any(z => z.ContactCode.ToString() == d)).ToList());
                                if (!string.IsNullOrEmpty(updatedSettingValue))
                                {
                                    userDefinedApproval.Where(e => e.WFSubOrderId == userDefinedLevel.WFSubOrderId).FirstOrDefault().WorkflowSettings.
                                        Where(e => e.SettingName == "SpecificUsers").First().SettingValue = updatedSettingValue;
                                }
                                else
                                {
                                    userDefinedApproval.Where(e => e.WFSubOrderId == userDefinedLevel.WFSubOrderId).FirstOrDefault().IsDeleted = true;
                                }
                            }
                        }
                        List<UserDefinedApproval> updatedApproval = p2pDocumentmanger.UpdateUserDefinedApprovers("", 0, documentCode, wfDocType, userDefinedApproval);
                        if (updatedApproval.Any(e => e.IsActive == false && e.IsDeleted == false && e.IsProcessed == false))
                        {
                            updatedApproval = updatedApproval.Where(e => e.IsActive == false && e.IsDeleted == false && e.IsProcessed == false && e.WorkflowId == 12).ToList();

                            foreach (var updatedApprovalLevel in updatedApproval)
                            {
                                String[] approverslist = updatedApprovalLevel.WorkflowSettings.Where(e => e.SettingName == "SpecificUsers").First().SettingValue.Split(',');
                                updatedApprovers.AddRange(approverslist.ToList());
                            }
                        }
                        string ContactCodes = String.Join(",", prevApprovers.Except(updatedApprovers));
                        List<User> Users = ContactCodes.Any() ? new RESTAPIHelper.PartnerHelper(this.UserContext, JWTToken).GetUserDetailsByContactCodes(ContactCodes, false) : new List<User>();
                        return (Users.Select(d => new KeyValuePair<string, string>("deletedApprover", d.FirstName + " " + d.LastName)).ToList());
                    }
                }

            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in updateUserDefinedApprovalonEntityChange Method in P2PCommonManager", ex);
                throw;
            }
            return new List<KeyValuePair<string, string>>();
        }

        public DocumentType GetDocumentType(P2PDocumentType p2pDocType)
        {
            switch (p2pDocType)
            {
                case P2PDocumentType.Requisition:
                    return DocumentType.Requisition;
                case P2PDocumentType.Order:
                    return DocumentType.PO;
                case P2PDocumentType.Receipt:
                    return DocumentType.Receipts;
                case P2PDocumentType.ReturnNote:
                    return DocumentType.ReturnNote;
                case P2PDocumentType.Invoice:
                    return DocumentType.Invoice;
                case P2PDocumentType.InvoiceReconciliation:
                    return DocumentType.InvoiceReconcillation;
                case P2PDocumentType.PaymentRequest:
                    return DocumentType.PaymentRequest;
                case P2PDocumentType.CreditMemo:
                    return DocumentType.CreditMemo;
                default:
                    return DocumentType.None;

            }
        }

        public List<Question> GetQuestionSetByFormCode(long formCode)
        {
            return GetCommonDao().GetQuestionSetByFormCode(formCode);
        }

        public ICollection<KeyValuePair<int, string>> GetAllShipToLocations(string searchText, int pageIndex, int pageSize, long LOBEntityDetailCode, long entityDetailCode)
        {
            return GetCommonDao().GetAllShipToLocations(searchText, pageIndex, pageSize, LOBEntityDetailCode, entityDetailCode);
        }

        public bool GetIsVABUserActive()
        {
            try
            {
                List<string> lstUserActivities = new List<string>();

                var partnerHelper = new RESTAPIHelper.PartnerHelper(this.UserContext, JWTToken);
                string UserActivities = partnerHelper.GetUserActivitiesByContactCode(UserContext.ContactCode, UserContext.BuyerPartnerCode);

                if (!string.IsNullOrEmpty(UserActivities))
                {
                    lstUserActivities = UserActivities.Split(',').ToList();
                    return lstUserActivities != null && lstUserActivities.Count > 0 ? lstUserActivities.Where(p => p == VAB_USER).ToList().Any() ? true : false : false;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in GetUserActivities Method in ReqCommonManager", ex);
                return false;
            }
        }

        public DocumentLOBDetails GetDocumentLOB(long documentCode)
        {
            try
            {
                return GetCommonDao().GetDocumentLOB(documentCode);
            }
            catch (CommunicationException ex)
            {
                LogHelper.LogError(Log, "GetDocumentLOB for documentCode=" + documentCode + " " + ex.Message, ex);
            }
            return null;
        }

        public DataTable GetTaxItemsByEntityID(int shipToID, long headerEntityID, int headerEntityTypeID)
        {
            try
            {
                return GetCommonDao().GetTaxItemsByEntityID(shipToID, headerEntityID, headerEntityTypeID);
            }
            catch (CommunicationException ex)
            {
                LogHelper.LogError(Log, "GetTaxItemsByEntityID" + ex.Message, ex);
            }
            return null;
        }

        public List<OrgSearchResult> GetEntitySearchResults(OrgSearch objSearch)
        {
            return GetCommonDao().GetEntitySearchResults(objSearch);
        }

        public void ValidateAllAccountingCodeCombination(long documentCode, DocumentType documentType)
        {
            GetCommonDao().ValidateAllAccountingCodeCombination(documentCode, documentType);
        }

        public DataTable GetBillToLocBasedOnDefaultHeaderEntity(P2PDocumentType docType, Collection<DocumentAdditionalEntityInfo> lstDocHeaderEntities)
        {

            var REQSettings = GetSettingsFromSettingsComponent(P2PDocumentType.None, UserContext.ContactCode, (int)SubAppCodes.P2P, "", lstDocHeaderEntities != null && lstDocHeaderEntities.Count > 0 ? lstDocHeaderEntities[0].LOBEntityDetailCode : 0);
            int EntityMappedToBillToLocation = !string.IsNullOrEmpty(GetSettingsValueByKey(REQSettings, "EntityMappedToBillToLocation")) ? Convert.ToInt16(GetSettingsValueByKey(REQSettings, "EntityMappedToBillToLocation")) : 0;
            long defaultentitydetailcode = 0;
            long lOBEntityDetailCode = 0;
            bool hasValidEntity = false;

            if (EntityMappedToBillToLocation > 0)
            {
                for (int i = 0; i < lstDocHeaderEntities.Count; i++)
                {
                    if (lstDocHeaderEntities[i].EntityId.Equals(EntityMappedToBillToLocation))
                    {
                        defaultentitydetailcode = lstDocHeaderEntities[i].EntityDetailCode;
                        lOBEntityDetailCode = lstDocHeaderEntities[i].LOBEntityDetailCode;
                        hasValidEntity = true;
                        break;
                    }
                }
            }
            if (lstDocHeaderEntities.Count > 0)
            {
                defaultentitydetailcode = ((defaultentitydetailcode == 0) ? lstDocHeaderEntities[0].EntityDetailCode : defaultentitydetailcode);
                if (!hasValidEntity)
                {
                    lOBEntityDetailCode = lstDocHeaderEntities[0].LOBEntityDetailCode;
                }
            }
            return GetCommonDao().GetListofBillToLocDetails("", 0, 0, defaultentitydetailcode, true, lOBEntityDetailCode);
        }

        public void SaveRequisitionChargeAccountingDetails(List<RequisitionSplitItems> requisitionChargeSplitItems, List<DocumentSplitItemEntity> requisitionChargeSplitItemEntities)
        {
            try
            {
                int precessionValue = convertStringToInt(this.GetSettingsValueByKey(P2PDocumentType.None, "MaxPrecessionValue", UserContext.ContactCode, (int)SubAppCodes.P2P, "", 0));
                int precessionValueForTotal = convertStringToInt(this.GetSettingsValueByKey(P2PDocumentType.None, "MaxPrecessionValueforTotal", UserContext.ContactCode, (int)SubAppCodes.P2P, "", 0));
                int precessionValueForTaxesAndCharges = convertStringToInt(this.GetSettingsValueByKey(P2PDocumentType.None, "MaxPrecessionValueForTaxesAndCharges", UserContext.ContactCode, (int)SubAppCodes.P2P, "", 0));
                GetCommonDao().SaveRequisitionChargeAccountingDetails(requisitionChargeSplitItems, requisitionChargeSplitItemEntities, precessionValue, precessionValueForTotal, precessionValueForTaxesAndCharges);
            }
            catch (CommunicationException ex)
            {
                LogHelper.LogError(Log, "SaveRequisitionChargeAccountingDetails for document Item Id= " + requisitionChargeSplitItems[0].DocumentItemId + " " + ex.Message, ex);
            }
        }

        #region WebAPI method calls

        public List<DocumentProxyDetails> CheckOriginalApproverNotificationStatus()
        {
            return GetCommonDao().CheckOriginalApproverNotificationStatus();
        }

        public string GetAutoSaveDocument(Int64 id, int DocTypeCode)
        {
            return GetCommonDao().GetAutoSaveDocument(id, DocTypeCode);
        }

        public void RemoveTaskDetailsFormWithdrawn(int wfDocTypeId, long documentCode, long userId)
        {

            P2PDocumentManager objDocManager = new P2PDocumentManager()
            {
                UserContext = UserContext,
                GepConfiguration = GepConfiguration,
                jwtToken = this.JWTToken
            };
            List<ReviewerDetails> reviewersList = new List<ReviewerDetails>();
            List<ApproverDetails> lstApproverDetails = new List<ApproverDetails>();
            if (wfDocTypeId == 41)
            {
                reviewersList = objDocManager.GetReviewersList(documentCode, wfDocTypeId);
                DeleteAllTasksForReviewDocument(documentCode, wfDocTypeId, reviewersList, UserContext);
            }
            else
            {
                lstApproverDetails = GetActionersDetails(documentCode, wfDocTypeId);
                DeleteTaskDetailsForDocument(documentCode, wfDocTypeId, lstApproverDetails);
            }

        }

        public void DeleteAllTasksForReviewDocument(long documentCode, int wfDocTypeId, List<ReviewerDetails> reviewersList, UserExecutionContext userExecutionContext)
        {
            var objTaskManager = new TaskManager() { UserContext = UserContext, GepConfiguration = GepConfiguration };
            TaskInformation taskInformation = new TaskInformation();

            if (reviewersList.Any())
            {
                List<TaskActionDetails> lstTasksAction = new List<TaskActionDetails>();
                lstTasksAction.Add(TaskManager.CreateActionDetails(ActionKey.Accept, "ACCEPT"));
                lstTasksAction.Add(TaskManager.CreateActionDetails(ActionKey.Reject, "REJECT"));

                foreach (ReviewerDetails reviewer in reviewersList)
                {
                    taskInformation = TaskManager.CreateTaskObject(documentCode, reviewer.ReviewerId, lstTasksAction, true, false, userExecutionContext.BuyerPartnerCode, userExecutionContext.CompanyName);

                    objTaskManager.SaveTaskActionDetails(taskInformation);

                    if (reviewer.ProxyReviewerId > 0)
                    {
                        taskInformation = TaskManager.CreateTaskObject(documentCode, reviewer.ProxyReviewerId, lstTasksAction, true, false,
                                                                          userExecutionContext.BuyerPartnerCode, userExecutionContext.CompanyName);
                        objTaskManager.SaveTaskActionDetails(taskInformation);
                    }
                }
            }
        }

        public void DeleteTaskDetailsForDocument(long documentCode, int wfDocTypeId, List<ApproverDetails> lstActioners)
        {
            //var lstActioners = objDocSer.GetActionersDetails(documentCode, wfDocTypeId);
            var lstPendingApprovers = new List<ApproverDetails>();

            if (lstActioners != null && lstActioners.Any())
            {
                lstPendingApprovers =
                lstActioners.Where(X => X.Status == 2).ToList();

                if (lstPendingApprovers.Any())
                {
                    //var taskActionDetails = TaskHelper.CreateActionDetails(actionKey, string.Empty);
                    List<TaskActionDetails> lstTasksAction = new List<TaskActionDetails>();
                    lstTasksAction.Add(TaskManager.CreateActionDetails(ActionKey.Approve, "APPROVE"));
                    lstTasksAction.Add(TaskManager.CreateActionDetails(ActionKey.Reject, "REJECT"));

                    var userExecutionContext = UserContext;
                    var objTaskManager = new TaskManager() { UserContext = UserContext, GepConfiguration = GepConfiguration };
                    foreach (var actioner in lstPendingApprovers)
                    {
                        var taskInformation = TaskManager.CreateTaskObject(documentCode, actioner.ApproverId,
                                                                          lstTasksAction,
                                                                          true, false,
                                                                          userExecutionContext.BuyerPartnerCode,
                                                                          userExecutionContext.CompanyName);
                        objTaskManager.SaveTaskActionDetails(taskInformation);

                        if (actioner.ProxyApproverId > 0)
                        {
                            taskInformation = TaskManager.CreateTaskObject(documentCode, actioner.ProxyApproverId,
                                                                              lstTasksAction,
                                                                              true, false,
                                                                              userExecutionContext.BuyerPartnerCode,
                                                                              userExecutionContext.CompanyName);
                            objTaskManager.SaveTaskActionDetails(taskInformation);
                        }
                    }
                }
            }
        }

        public BilltoLocation GetBillToLocationById(int id)
        {
            if (id > 0)
                return GetCommonDao().GetBillToLocationById(id);

            return new BilltoLocation();
        }

        public P2PItem GetContractDataByCatalogItemId(long catalogItemId, long partnerCode, string contractNo)
        {
            return GetCommonDao().GetContractDataByCatalogItemId(catalogItemId, partnerCode, contractNo);
        }

        public int GetWorkflowDocTypeForDocument(long documentCode, int documentType)
        {
            return GetCommonDao().GetWorkflowDocTypeForDocument(documentCode, documentType);
        }

        public ICollection<Countries> SearchCountries(string searchCountry)
        {
            var csmHelper = new RESTAPIHelper.CSMHelper(UserContext, JWTToken);
            var country = new
            {
                CountryName = searchCountry ?? string.Empty,
                PageIndex = 0,
                PageSize = 10,
                SortOrder = ""
            };
            return csmHelper.SearchCountries(country);
        }

        public List<LOBEntityConfiguration> GetLOBEntityConfigurationByLOBEntityDetailCode(long LOBEntityDetailCode, int IdentificationTypeID = 0)
        {
            try
            {
                return GetCommonDao().GetLOBEntityConfigurationByLOBEntityDetailCode(LOBEntityDetailCode, IdentificationTypeID);
            }
            catch (CommunicationException ex)
            {
                LogHelper.LogError(Log, "GetLOBEntityConfigurationByLOBEntityDetailCode for LOBEntityDetailCode=" + LOBEntityDetailCode + " " + ex.Message, ex);
            }
            return null;
        }

        public void GetLOBEntityConfigurationByLOBEntityDetailCodes(long LOBEntityDetailCode, out List<LOBEntityConfiguration> config, int IdentificationTypeID = 0)
        {
            config = GetCommonDao().GetLOBEntityConfigurationByLOBEntityDetailCode(LOBEntityDetailCode, IdentificationTypeID);
        }

        public bool AutoSaveDocument(string ObjectData, Int64 DocumentCode, int DocumentTypeCode, Int64 LastModifiedBy)
        {
            return GetCommonDao().AutoSaveDocument(ObjectData, DocumentCode, DocumentTypeCode, LastModifiedBy);
        }


        public bool DeleteAutoSaveDocument(long DocumentCode, int DocumentTypeCode)
        {
            return GetCommonDao().DeleteAutoSaveDocument(DocumentCode, DocumentTypeCode);
        }

        public ICollection<States> SearchStates(string searchState, int countryId)
        {
            var csmHelper = new RESTAPIHelper.CSMHelper(UserContext, JWTToken);
            var state = new
            {
                StateName = searchState,
                CountryId = countryId,
                PageIndex = 0,
                PageSize = 10,
                SortOrder = ""
            };
            return csmHelper.SearchStates(state);
        }

        public string GenerateLocationCode(long userId)
        {
            var locationPrefix = GetSettingsValueByKey(P2PDocumentType.None, "LocationPrefix", userId);
            return GetCommonDao().GenerateLocationCode(locationPrefix);
        }

        public List<ContactInfo> GetPartnerContactsByPartnerCodeandOrderingLocation(long partnerCode, long orderingLocationId, bool flagToFetchContactsOfAllRoles = false)
        {
            return GetRequisitionCommonDAO().GetPartnerContactsByPartnerCodeandOrderingLocation(partnerCode, orderingLocationId, flagToFetchContactsOfAllRoles);
        }

        public bool HasAccessToViewEntity(long documentCode, DocumentType documentType)
        {
            List<string> lstUserActivities = new List<string>();
            bool result = false;
            if (documentCode > 0)
            {
                try
                {
                    UserExecutionContext userExecutionContext = UserContext;
                    var partnerHelper = new RESTAPIHelper.PartnerHelper(this.UserContext, JWTToken);
                    lstUserActivities = partnerHelper.GetUserActivitiesByContactCode(userExecutionContext.ContactCode, userExecutionContext.BuyerPartnerCode).Split(',').ToList();

                    bool hasViewAccess = false;
                    switch (documentType)
                    {
                        case DocumentType.Requisition:
                            if (UserActivity.CheckUserActivity(UserActivity.CREATE_REQUISITION, lstUserActivities) || UserActivity.CheckUserActivity(UserActivity.APPROVE_REJECT_REQUISITION, lstUserActivities) || UserActivity.CheckUserActivity(UserActivity.FUNCTIONAL_ADMIN, lstUserActivities) || UserActivity.CheckUserActivity(UserActivity.VIEW_REQUISITION, lstUserActivities))
                                hasViewAccess = true;
                            break;
                        case DocumentType.PO:
                            hasViewAccess = UserActivity.CheckUserActivity(UserActivity.VIEW_ORDER, lstUserActivities);
                            break;
                        case DocumentType.Invoice:
                            if (UserActivity.CheckUserActivity(UserActivity.VIEW_INVOICING_STATUS, lstUserActivities) || UserActivity.CheckUserActivity(UserActivity.CREATE_INVOICE, lstUserActivities) ||
                                UserActivity.CheckUserActivity(UserActivity.CREATE_INVOICE_BY_FLIPPING_ORDER, lstUserActivities) || UserActivity.CheckUserActivity(UserActivity.VIEW_RECEIVED_INVOICE, lstUserActivities) ||
                                UserActivity.CheckUserActivity(UserActivity.SUBMIT_INVOICE_FOR_PAYMENT, lstUserActivities) || UserActivity.CheckUserActivity(UserActivity.EDIT_INVOICE, lstUserActivities) ||
                                UserActivity.CheckUserActivity(UserActivity.REJECT_INVOICE, lstUserActivities))
                                hasViewAccess = true;
                            break;
                        case DocumentType.Receipts:
                            hasViewAccess = UserActivity.CheckUserActivity(UserActivity.VIEW_SEARCH_RECEIPT, lstUserActivities);
                            break;
                        case DocumentType.InvoiceReconcillation:
                            hasViewAccess = true;
                            break;
                        case DocumentType.PaymentRequest:
                            hasViewAccess = true;
                            break;
                        case DocumentType.CreditMemo:
                            hasViewAccess = true;
                            break;
                        case DocumentType.ReturnNote:
                            hasViewAccess = true;
                            break;
                        case DocumentType.ServiceConfirmation:
                            hasViewAccess = true;
                            break;
                        case DocumentType.Timesheet:
                            hasViewAccess = true;
                            break;
                        case DocumentType.RFP:
                            hasViewAccess = true;
                            break;
                        case DocumentType.RFQ:
                            hasViewAccess = true;
                            break;
                        case DocumentType.RFI:
                            hasViewAccess = true;
                            break;
                        case DocumentType.ASN:
                            hasViewAccess = true;
                            break;
                        default:
                            hasViewAccess = false;
                            break;
                    }
                    if (hasViewAccess)
                    {
                        result = HasAccessToViewEntity(documentCode);
                    }
                    else
                    {
                        result = false;
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.LogError(Log, "Error occured in HasAccessToViewEntity Method in P2PCommonManager", ex);
                    throw;
                }

            }
            return result;
        }

        public bool HasAccessToViewEntity(long documentCode)
        {
            return GetCommonDao().HasAccessToViewEntity(documentCode);
        }

        public List<KeyValuePair<string, string>> SaveShiptoLocationList(List<ShiptoLocation> lstShipToLocation, out List<ShiptoLocation> outLstShipToLocation, long LOBEntityDetailCode, long entityDetailCode, int entityId, bool addAddhocState = false, bool isInterface = false, List<PropertyInfo> lstPropertyInfo = null, long contactCode = 0)
        {
            List<KeyValuePair<string, string>> lstSaveLogs = new List<KeyValuePair<string, string>>();
            outLstShipToLocation = new List<ShiptoLocation>();
            foreach (ShiptoLocation shipToLoc in lstShipToLocation)
            {
                string Name = shipToLoc.ShiptoLocationName;
                try
                {
                    SaveShipToLocation(outLstShipToLocation, lstSaveLogs, shipToLoc, LOBEntityDetailCode, entityDetailCode, entityId, addAddhocState, isInterface, lstPropertyInfo, contactCode);
                }
                catch (Exception ex)
                {

                }
            }

            return lstSaveLogs;
        }

        private void SaveShipToLocation(List<ShiptoLocation> outLstShipToLocation, List<KeyValuePair<string, string>> lstSaveLogs, ShiptoLocation shipToLoc, long LOBEntityDetailCode, long entityDetailCode, int entityId, bool addAddhocState = false, bool isInterface = false, List<PropertyInfo> lstPropertyInfo = null, long contactCode = 0)
        {
            ShiptoLocation outShipToLoc = shipToLoc;
            string key = string.Empty;
            if (lstPropertyInfo != null && lstPropertyInfo.Count > 0)
            {
                foreach (var prop in lstPropertyInfo)
                {
                    if (shipToLoc.GetType().GetProperty(prop.Name) != null)
                        key += (prop.GetValue(shipToLoc, null) ?? string.Empty).ToString() + "|";
                }
                key = key.TrimStart('|').TrimEnd('|');
            }
            else
                key = shipToLoc.ShiptoLocationName;

            try
            {
                if (string.IsNullOrEmpty(shipToLoc.ShiptoLocationNumber))
                {
                    //in case client wont send ship to location number, get ship to location by address.
                    shipToLoc.ShiptoLocationId = GetShipToLocationIdByAddress(shipToLoc, true);

                    States objState = null;

                    if (shipToLoc.ShiptoLocationId > 0)
                    {
                        // Update Existing State
                        if (addAddhocState && string.IsNullOrWhiteSpace(shipToLoc.Address.StateCode) && !string.IsNullOrWhiteSpace(shipToLoc.Address.State))
                        {
                            objState = AddStateOther(new States() { StateCode = Convert.ToInt64(shipToLoc.Address.StateCode), StateName = shipToLoc.Address.State, IsActive = true, IsDeleted = false, CultureCode = "en-US" });
                            shipToLoc.Address.StateCode = Convert.ToString(objState.StateCode);
                        }

                        if (!UpdateShiptoLocation(shipToLoc))
                            lstSaveLogs.Add(new KeyValuePair<string, string>(key, "Unable to update ship to location details"));
                    }
                    else
                    {
                        if (shipToLoc.Address.AddressId == 0)
                        {
                            var csmHelper = new RESTAPIHelper.CSMHelper(UserContext, JWTToken);
                            var body = new
                            {
                                EntityTypeName = new Address().GetType().FullName,
                                SeedIncrement = 1
                            };
                            shipToLoc.Address.AddressId = csmHelper.GenerateNewEntityCode(body);
                        }

                        // Add New State
                        if (addAddhocState && string.IsNullOrWhiteSpace(shipToLoc.Address.StateCode) && !string.IsNullOrWhiteSpace(shipToLoc.Address.State))
                        {
                            objState = AddStateOther(new States() { StateName = shipToLoc.Address.State, IsActive = true, IsDeleted = false, CultureCode = "en-US" });
                            shipToLoc.Address.StateCode = Convert.ToString(objState.StateCode);
                        }

                        outShipToLoc.ShiptoLocationId = InsertShiptoLocation(shipToLoc, LOBEntityDetailCode, entityDetailCode, entityId, isInterface, contactCode);
                        if (outShipToLoc.ShiptoLocationId <= 0)
                            lstSaveLogs.Add(new KeyValuePair<string, string>(key, "Unable to add ship to location details"));
                    }
                }
                else
                {
                    ShiptoLocation objShipToLoc = GetShipToLocationByNumber(shipToLoc.ShiptoLocationNumber, true);
                    States objState = null;

                    if (object.ReferenceEquals(objShipToLoc, null) || string.IsNullOrWhiteSpace(objShipToLoc.ShiptoLocationNumber))
                    {
                        // Add New State
                        if (addAddhocState && string.IsNullOrWhiteSpace(shipToLoc.Address.StateCode) && !string.IsNullOrWhiteSpace(shipToLoc.Address.State))
                        {
                            objState = AddStateOther(new States() { StateName = shipToLoc.Address.State, IsActive = true, IsDeleted = false, CultureCode = "en-US" });
                            shipToLoc.Address.StateCode = Convert.ToString(objState.StateCode);
                        }

                        var csmHelper = new RESTAPIHelper.CSMHelper(UserContext, JWTToken);
                        var body = new
                        {
                            EntityTypeName = new Address().GetType().FullName,
                            SeedIncrement = 1
                        };
                        shipToLoc.Address.AddressId = csmHelper.GenerateNewEntityCode(body);
                        outShipToLoc.ShiptoLocationId = InsertShiptoLocation(shipToLoc, LOBEntityDetailCode, entityDetailCode, entityId, isInterface, contactCode);
                        if (outShipToLoc.ShiptoLocationId <= 0)
                            lstSaveLogs.Add(new KeyValuePair<string, string>(key, "Unable to add ship to location details"));
                    }
                    else
                    {
                        outShipToLoc.ShiptoLocationId = objShipToLoc.ShiptoLocationId;

                        // Update Existing State
                        if (addAddhocState && string.IsNullOrWhiteSpace(shipToLoc.Address.StateCode) && !string.IsNullOrWhiteSpace(shipToLoc.Address.State))
                        {
                            long _stateCode = !string.IsNullOrWhiteSpace(objShipToLoc.Address.State) ? 0 : Convert.ToInt64(objShipToLoc.Address.StateCode);
                            objState = AddStateOther(new States() { StateCode = _stateCode, StateName = shipToLoc.Address.State, IsActive = true, IsDeleted = false, CultureCode = "en-US" });
                            shipToLoc.Address.StateCode = Convert.ToString(objState.StateCode);
                        }

                        shipToLoc.Address.AddressId = objShipToLoc.Address.AddressId;
                        if (!UpdateShiptoLocation(shipToLoc))
                            lstSaveLogs.Add(new KeyValuePair<string, string>(key, "Unable to update ship to location details"));
                    }
                }
            }
            catch (Exception ex)
            {
                lstSaveLogs.Add(new KeyValuePair<string, string>(key, ex.Message.Contains("Validation:") ? ex.Message : "Unable to update ship to location details."));
                throw ex;
            }
            outLstShipToLocation.Add(outShipToLoc);
        }

        public ShiptoLocation GetShipToLocationByNumber(string shipToLocNumber, bool getDeletedShipTo = false, bool isInterface = false)
        {
            return GetCommonDao().GetShipToLocationByNumber(shipToLocNumber, getDeletedShipTo, isInterface);
        }

        public int GetShipToLocationIdByAddress(ShiptoLocation objShipToLocation, bool getDeletedShipTo = false)
        {
            return GetCommonDao().GetShipToLocationIdByAddress(objShipToLocation, getDeletedShipTo);
        }

        public bool UpdateShiptoLocation(ShiptoLocation shiptoLocation)
        {
            return GetCommonDao().UpdateShiptoLocation(shiptoLocation);
        }

        public States AddStateOther(States objState)
        {
            States objStateInfo = null;
            if (!ReferenceEquals(objState, null))
            {
                var csmHelper = new RESTAPIHelper.CSMHelper(UserContext, JWTToken);
                var body = new
                {
                    StateCode = objState.StateCode,
                    StateName = objState.StateName,
                    IsActive = objState.IsActive,
                    IsDeleted = objState.IsDeleted,
                    StateAbbrevationCode = objState.StateAbbrevationCode
                };
                objStateInfo = csmHelper.SaveState(body);

            }
            return objStateInfo;
        }

        public List<DocumentProxyDetails> CheckOriginalReviewerNotificationStatus()
        {
            return GetCommonDao().CheckOriginalReviewerNotificationStatus();
        }

        public ItemCharge DeleteItemChargeByItemChargeId(int documentTypeCode, long itemChargeId, long LOBEntityDetailCode)
        {
            ItemCharge objcharge = new ItemCharge();
            int precessionValue = convertStringToInt(this.GetSettingsValueByKey(P2PDocumentType.None, "MaxPrecessionValue", UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBEntityDetailCode));
            int precessionValueForTotal = convertStringToInt(this.GetSettingsValueByKey(P2PDocumentType.None, "MaxPrecessionValueforTotal", UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBEntityDetailCode));
            int precessionValueForTaxesAndCharges = convertStringToInt(this.GetSettingsValueByKey(P2PDocumentType.None, "MaxPrecessionValueForTaxesAndCharges", UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBEntityDetailCode));

            try
            {
                objcharge = GetCommonDao().DeleteItemChargeByItemChargeId(documentTypeCode, itemChargeId, precessionValue, precessionValueForTotal, precessionValueForTaxesAndCharges);
            }
            catch (CommunicationException ex)
            {
                LogHelper.LogError(Log, "DeleteItemChargeByItemChargeId for documentTypeCode = " + documentTypeCode + ",itemChargeId = " + itemChargeId + " " + ex.Message, ex);
            }
            return objcharge;
        } 

        public List<GEP.NewP2PEntities.ASLValidationResponse> ValidateASL(long orderId, long PartnerCode, long OrderLocationID, long orgEntityDetailCode)
        {
            List<GEP.NewP2PEntities.ASLValidationResponse> lst = new List<GEP.NewP2PEntities.ASLValidationResponse>();
            try
            {
                lst = GetCommonDao().ValidateASL(orderId, PartnerCode, OrderLocationID, orgEntityDetailCode);
                return lst;
            }
            catch (CommunicationException ex)
            {
                LogHelper.LogError(Log, "ValidateASL  " + ex.Message, ex);
            }

            return lst;
        }

        public List<TaxIntegrationType> GetTaxintegrationByMappedEntity()
        {
            return GetCommonDao().GetTaxintegrationByMappedEntity();
        }
        #endregion
    }
}

