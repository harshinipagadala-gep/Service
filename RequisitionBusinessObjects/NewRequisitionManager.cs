using Gep.Cumulus.CSM.Entities;
using Gep.Cumulus.ExceptionManager;
using Gep.Cumulus.Partner.Entities;
using GEP.Cumulus.Caching;
using GEP.Cumulus.DocumentIntegration.Entities;
using GEP.Cumulus.Documents.Entities;
using GEP.Cumulus.Logging;
using GEP.Cumulus.OrganizationStructure.Entities;
using GEP.Cumulus.P2P.BusinessEntities;
using GEP.Cumulus.P2P.BusinessObjects;
using GEP.Cumulus.P2P.BusinessObjects.Proxy;
using GEP.Cumulus.P2P.Common;
using GEP.Cumulus.P2P.Req.BusinessObjects.Entities;
using GEP.Cumulus.P2P.Req.BusinessObjects.FactoryClasses;
using GEP.Cumulus.P2P.Req.BusinessObjects.Proxy;
using GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper;
using GEP.Cumulus.QuestionBank.Entities;
using GEP.Cumulus.Web.Utils.Helpers;
using GEP.NewP2PEntities;
using GEP.NewPlatformEntities;
using GEP.SMART.Configuration;
using log4net;
using NewRelic.Api.Agent;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using CatalogItemSearch = GEP.Cumulus.SmartCatalog.BusinessEntities.NewCatalog;
using FileManagerEntities = GEP.NewP2PEntities.FileManagerEntities;
using P2P1 = GEP.Cumulus.P2P.BusinessEntities;
using SMARTFaultException = Gep.Cumulus.ExceptionManager;

namespace GEP.Cumulus.P2P.Req.BusinessObjects
{
    /// <summary>
    /// 2.0 Requisition manager.
    /// </summary>
    public class NewRequisitionManager : RequisitionBaseBO
    {
        public NewRequisitionManager(string jwtToken, UserExecutionContext context = null) : base(jwtToken)
        {
            if (context != null)
            {
                this.UserContext = context;
            }
        }
        private static int REQ = 7;
        private const string SENT_FOR_APPROVAl = "Send for Approval";
        private static readonly ILog Log = Logger.GetLog(MethodBase.GetCurrentMethod().DeclaringType);
        private const string REQ_SUBMIT_ERROR = "P2P_Req_Submit_Error";

        private UserContext GetUserContext(long contactCode)
        {
            var partnerHelper = new RESTAPIHelper.PartnerHelper(this.UserContext, JWTToken);
            return partnerHelper.GetUserContextDetailsByContactCode(contactCode);
        }

        #region Private methods.
        /// <summary>
        /// Disposes service channel.
        /// </summary>
        /// <param name="objServiceChannel">Service channel object.</param>
        private void DisposeService(ICommunicationObject objServiceChannel)
        {
            if (objServiceChannel != null)
            {
                if (objServiceChannel.State == CommunicationState.Faulted)
                    objServiceChannel.Abort();
                else
                    objServiceChannel.Close();
            }
        }

        /// <summary>
        /// Gets number for a new requisition.
        /// </summary>
        /// <returns>Requisition number</returns>
        private String GetNewReqNumber()
        {
            long LOBEntityDetailCode = UserContext.BelongingEntityDetailCode;
            var executionHelper = new Req.BusinessObjects.RESTAPIHelper.ExecutionHelper(this.UserContext, this.GepConfiguration, this.JWTToken);
            if (LOBEntityDetailCode <= 0)
            {
                var partnerHelper = new RESTAPIHelper.PartnerHelper(this.UserContext, JWTToken);
                UserContext userContext = partnerHelper.GetUserContextDetailsByContactCode(this.UserContext.ContactCode);

                LOBEntityDetailCode = userContext.GetDefaultBelongingUserLOBMapping().EntityDetailCode;
            }

            return GetEntityNumberForRequisition("REQ", LOBEntityDetailCode);
        }

        /// <summary>
        /// This function is for testing the MotleyService calls, it is not used anywhere for now
        /// </summary>
        /// <param name="LOBEntityDetailCode"></param>
        /// <returns></returns>
        private string GetEntityNumberForRequisition(string documentType, long LOBEntityDetailCode = 0, long EntityDetailcode = 0, int PurchaseTypeID = 0)
        {
            var requestHeaders = new RESTAPIHelper.RequestHeaders();
            string requisitionNumber;
            string appName = Environment.GetEnvironmentVariable("NewRelic.AppName") ?? "Requisition";
            string useCase = "NewRequisitionManager-GetEntityNumberForRequisition";
            string serviceURL = MultiRegionConfig.GetConfig(CloudConfig.APIMBaseURL) + RESTAPIHelper.ServiceURLs.MotleyServiceURL + "GetEntityNumber";
            Dictionary<string, object> body = new Dictionary<string, object>();
            body.Add("entityType", documentType);
            body.Add("lobId", LOBEntityDetailCode);
            body.Add("entityDetailCode", EntityDetailcode);
            body.Add("purchaseTypeID", PurchaseTypeID);

            requestHeaders.Set(UserContext, JWTToken);
            var webAPI = new GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.WebAPI(requestHeaders, appName, useCase);
            requisitionNumber = webAPI.ExecutePost(serviceURL, body);
            return requisitionNumber.Replace("\"", string.Empty);
        }
        #endregion

        #region Public methods.
        /// <summary>
        /// Saves requisition header.
        /// </summary>
        /// <param name="req">Requisition.</param>
        /// <returns>Result.</returns>
        public SaveResult SaveRequisitionHeader(NewP2PEntities.Requisition req)
        {
            if (req.id <= 0)
                req.number = GetNewReqNumber();

            return GetNewReqDao().SaveRequisitionHeader(req, GetBUDetailList(req.HeaderSplitAccountingFields, req.documentCode, req.obo != null ? req.obo.id : 0));
        }

        /// <summary>
        /// Gets details of requisition for display.
        /// </summary>
        /// <param name="id">Id.</param>
        /// <returns>Requisition.</returns>
        public NewP2PEntities.Requisition GetRequisitionDisplayDetails(Int64 id, List<long> documentIds = null, GEP.NewP2PEntities.Requisition requisition = null)
        {
            NewP2PEntities.Requisition req = null;
            try
            {
                if (id <= 0)
                {
                    req = GEPDataCache.GetFromCacheJSON<NewP2PEntities.Requisition>("BlankRequisitionDetails",
                        UserContext.BuyerPartnerCode, UserContext.ContactCode, "en-US");
                    if (req != null)
                    {
                        req.createdOn = DateTime.Now;
                        req.lastModifiedOn = DateTime.Now;
                    }
                }
                RequisitionCommonManager commonMamager = new RequisitionCommonManager(this.JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
                var p2pSettings = commonMamager.GetSettingsFromSettingsComponent(P2PDocumentType.None, UserContext.ContactCode, (int)SubAppCodes.P2P, "");
                bool enableCategoryAutoSuggest = commonMamager.GetSettingsValueByKey(p2pSettings, "EnableCategoryAutoSuggest") == string.Empty ? false : Convert.ToBoolean(commonMamager.GetSettingsValueByKey(p2pSettings, "EnableCategoryAutoSuggest"));

                if (req == null)
                {
                    req = GetNewReqDao().GetRequisitionDisplayDetails(id, documentIds, enableCategoryAutoSuggest);
                    if (req?.items != null && req.items.Count > 0)
                    {
                        foreach (var requisitionItem in req.items)
                        {
                            if (!string.IsNullOrEmpty(requisitionItem.ContractReference))
                            {
                                var requestHeaders = new RESTAPIHelper.RequestHeaders();
                                requestHeaders.Set(UserContext, this.JWTToken);
                                var appName = Environment.GetEnvironmentVariable("NewRelic.AppName") ?? "Requisition";
                                const string useCase =
                                    "RequisitionInterfaceManager-UpdateRequisitionDetailsFromInterface";
                                var serviceUrl = MultiRegionConfig.GetConfig(CloudConfig.APIMBaseURL) +
                                                 "CLMRestAPI/api/v0/ContractERP/GetERPNumberByContractNumber?contractNumber=" +
                                                 requisitionItem.ContractReference;

                                var webApi = new RESTAPIHelper.WebAPI(requestHeaders, appName, useCase);
                                var response = webApi.ExecuteGet(serviceUrl);
                                requisitionItem.ContractReference = response;
                            }
                        }
                    }
                    if (id <= 0)
                    {
                        GEPDataCache.PutInCacheJSON<NewP2PEntities.Requisition>("BlankRequisitionDetails",
                            UserContext.BuyerPartnerCode, UserContext.ContactCode, "en-US",
                            req);
                    }
                }
                req.UserConfigurations = GetUserConfigurations(UserContext.ContactCode, REQ);

                SplitAccountingFields selectedheaderentity = (!ReferenceEquals(requisition, null) && requisition.HeaderSplitAccountingFields != null) ?
                                    requisition.HeaderSplitAccountingFields.Find(x => x.EntityTypeId == 0 && x.EntityDetailCode > 0) : null;
                if (selectedheaderentity != null)
                {
                    OrganizationStructure.Entities.OrgSearch objSearch = new OrganizationStructure.Entities.OrgSearch
                    {
                        AssociationTypeInfo = OrganizationStructure.Entities.AssociationType.Both,
                        OrgEntityCode = selectedheaderentity.EntityDetailCode,
                        objEntityType = new OrganizationStructure.Entities.OrgEntityType { },
                    };
                    OrganizationStructure.Entities.OrgSearchResult orgEntity = commonMamager.GetEntitySearchResults(objSearch).ToList().FirstOrDefault();
                    selectedheaderentity.EntityTypeId = orgEntity.EntityId;
                }

                long selectedHeaderentityLOBEntityDetailCode = 0;
                SettingDetails requisitionSettingDetails = commonMamager.GetSettingsFromSettingsComponent(P2PDocumentType.Requisition, UserContext.ContactCode, (int)SubAppCodes.P2P, "");
                bool setDefaultLOBForCatalogRequisition = commonMamager.GetSettingsValueByKey(requisitionSettingDetails, "SetDefaultLOBForCatalogRequisition") == string.Empty ? false : Convert.ToBoolean(commonMamager.GetSettingsValueByKey(requisitionSettingDetails, "SetDefaultLOBForCatalogRequisition"));
                if (setDefaultLOBForCatalogRequisition && selectedheaderentity != null)
                {
                    selectedHeaderentityLOBEntityDetailCode = GetLobByEntityDetailCode(selectedheaderentity.EntityDetailCode);
                }

                if (id <= 0 && req != null)
                {
                    List<SplitAccountingFields> lstHeaderSpltAccFlds = new List<SplitAccountingFields>();
                    List<NewPlatformEntities.DocumentBU> lstDocumentBU = new List<NewPlatformEntities.DocumentBU>();
                    string strDocumentName = string.Empty;
                    string strDocumentNo = string.Empty;
                    string docName = string.Empty;
                    List<Questionnaire> lstQuestionnaire = new List<Questionnaire>();
                    long CustomAttrFormIdForHeader = 0;
                    long CustomAttrFormIdForItem = 0;
                    long CustomAttrFormIdForSplit = 0;
                    long CustomAttrFormIdForRiskAssessment = 0;

                    req.documentBU = null;
                    req.documentLOB = new DocumentLOB();
                    req.OnEvent = OnEvent.None;
                    if (req.documentLOB.entityDetailCode == 0)
                        req.documentLOB.entityDetailCode = GetDocumentLOB(P2PDocumentType.Requisition);
                    if (selectedHeaderentityLOBEntityDetailCode > 0)
                    {
                        req.documentLOB.entityDetailCode = selectedHeaderentityLOBEntityDetailCode;
                    }
                    var data = GetCustomAttrFormId((int)DocumentType.Requisition, req.documentLOB.entityDetailCode);
                    CustomAttrFormIdForHeader = data.CustomAttrFormIdForHeader;
                    CustomAttrFormIdForItem = data.CustomAttrFormIdForItem;
                    CustomAttrFormIdForSplit = data.CustomAttrFormIdForSplit;
                    CustomAttrFormIdForRiskAssessment = data.CustomAttrFormIdForRiskAssessment;
                    lstQuestionnaire = data.lstQuestionnaire;

                    SettingDetails commonSettingDetails = commonMamager.GetSettingsFromSettingsComponent(P2PDocumentType.None, UserContext.ContactCode, (int)SubAppCodes.P2P, "", req.documentLOB.entityDetailCode);
                    var AllowDeliverToFreeTextSetting = commonMamager.GetSettingsValueByKey(commonSettingDetails, "AllowDeliverToFreeText");
                    bool AllowDeliverToFreeText = !string.IsNullOrEmpty(AllowDeliverToFreeTextSetting) ? Convert.ToBoolean(AllowDeliverToFreeTextSetting) : false;
                    if (AllowDeliverToFreeText)
                    {
                        var CustomDeliverToTextInUserProfileSetting = commonMamager.GetSettingsValueByKey(commonSettingDetails, "CustomDeliverToTextInUserProfile");
                        var CustomDeliverToTextInUserProfile = !string.IsNullOrEmpty(CustomDeliverToTextInUserProfileSetting) ? CustomDeliverToTextInUserProfileSetting : "";
                        RequisitionCommonManager reqCommonManager = new RequisitionCommonManager(this.JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
                        string customAttributeValue = reqCommonManager.GetCustomAttributeValuesForUser(UserContext.ContactCode, CustomDeliverToTextInUserProfile);
                        req.deliverToStr = customAttributeValue;
                    }
                    SettingDetails reqSettingDetails = commonMamager.GetSettingsFromSettingsComponent(P2PDocumentType.Requisition, UserContext.ContactCode, (int)SubAppCodes.P2P, "", req.documentLOB.entityDetailCode);
                    var populateDefaultRequisitionName = commonMamager.GetSettingsValueByKey(reqSettingDetails, "PopulateDefaultRequisitionName");
                    var currentPrincipal = System.Threading.Thread.CurrentPrincipal;
                    if (!String.IsNullOrEmpty(populateDefaultRequisitionName))
                    {
                        if (Convert.ToBoolean(populateDefaultRequisitionName))
                        {

                            Task tHeaderEntitiesAndBULst = Task.Factory.StartNew(() =>
                            {
                                System.Threading.Thread.CurrentPrincipal = currentPrincipal;
                                getHeaderEntitiesAndBUList(requisition, out lstHeaderSpltAccFlds, out lstDocumentBU, req, selectedHeaderentityLOBEntityDetailCode);
                            });

                            Task taskDocName = Task.Factory.StartNew((scope) =>
                            {
                                var ctx = (UserExecutionContext)scope;
                                System.Threading.Thread.CurrentPrincipal = currentPrincipal;
                                GenerateDefaultName(P2PDocumentType.Requisition, ctx.ContactCode, 0, 0, out strDocumentName);
                            }, UserContext);
                            Task.WaitAll(taskDocName, tHeaderEntitiesAndBULst);
                        }
                        else
                        {
                            getHeaderEntitiesAndBUList(requisition, out lstHeaderSpltAccFlds, out lstDocumentBU, req, selectedHeaderentityLOBEntityDetailCode);
                        }
                    }
                    else
                    {
                        //if there is no configuration available in basic settings,default considering to show default requisition name
                        Task tHeaderEntitiesAndBULst = Task.Factory.StartNew(() =>
                        {
                            System.Threading.Thread.CurrentPrincipal = currentPrincipal;
                            getHeaderEntitiesAndBUList(requisition, out lstHeaderSpltAccFlds, out lstDocumentBU, req, selectedHeaderentityLOBEntityDetailCode);
                        });
                        Task taskDocName = Task.Factory.StartNew((scope) =>
                        {
                            System.Threading.Thread.CurrentPrincipal = currentPrincipal;
                            var ctx = (UserExecutionContext)scope;
                            GenerateDefaultName(P2PDocumentType.Requisition, ctx.ContactCode, 0, 0, out strDocumentName);
                        }, UserContext);
                        Task.WaitAll(taskDocName, tHeaderEntitiesAndBULst);
                    }
                    //Task taskCustAttrFrmIds = Task.Factory.StartNew(() => GetCustomAttrFormId((int)DocumentType.Requisition, req.documentLOB.entityDetailCode, out CustomAttrFormIdForHeader, out CustomAttrFormIdForItem, out CustomAttrFormIdForSplit, out lstQuestionnaire));

                    //Task.WaitAll(taskDocName, tHeaderEntitiesAndBULst, taskCustAttrFrmIds);

                    req.documentBU = lstDocumentBU;
                    if (req.documentBU.Count > 0)
                        req.businessUnit = new IdAndName { id = req.documentBU[0].buCode };
                    req.name = strDocumentName;
                    //req.number = strDocumentNo;
                    req.HeaderSplitAccountingFields = lstHeaderSpltAccFlds;

                    req.CustomAttrFormIdForHeader = CustomAttrFormIdForHeader;
                    req.CustomAttrFormIdForItem = CustomAttrFormIdForItem;
                    req.CustomAttrFormIdForSplit = CustomAttrFormIdForSplit;
                    req.CustomAttrFormIdForRiskAssessment = CustomAttrFormIdForRiskAssessment;
                    req.CustomAttrQuestionSetCodesForHeader = GetQuestionnaireByFormCode(lstQuestionnaire, req.CustomAttrFormIdForHeader);
                    req.CustomAttrQuestionSetCodesForItem = GetQuestionnaireByFormCode(lstQuestionnaire, req.CustomAttrFormIdForItem);
                    if (CustomAttrFormIdForRiskAssessment > 0)
                        req.CustomAttrQuestionSetCodesForRiskAssessment = GetQuestionnaireByFormCode(lstQuestionnaire, req.CustomAttrFormIdForRiskAssessment);
                }
                else
                {
                    List<SplitAccountingFields> HeaderSplitAccountingFields = new List<SplitAccountingFields>();
                    List<Questionnaire> lstQuestionSetCodes = new List<Questionnaire>();
                    lstQuestionSetCodes = GetAllQuestionnaireByFormCodes(req.CustomAttrFormIdForHeader, req.CustomAttrFormIdForItem, lstQuestionSetCodes, req.CustomAttrFormIdForRiskAssessment);
                    GetAllHeaderAccountingFieldsForRequisition(id, out HeaderSplitAccountingFields);

                    //Task taskHeaderEnt = Task.Factory.StartNew(() => GetAllHeaderAccountingFieldsForRequisition(id, out HeaderSplitAccountingFields));
                    //Task taskQuestionResult = Task.Factory.StartNew(() => GetAllQuestionnaireByFormCodes(req.CustomAttrFormIdForHeader, req.CustomAttrFormIdForItem, out lstQuestionSetCodes));

                    //Task.WaitAll(taskHeaderEnt, taskQuestionResult);
                    req.HeaderSplitAccountingFields = HeaderSplitAccountingFields;
                    req.CustomAttrQuestionSetCodesForHeader = GetQuestionnaireByFormCode(lstQuestionSetCodes, req.CustomAttrFormIdForHeader);
                    req.CustomAttrQuestionSetCodesForItem = GetQuestionnaireByFormCode(lstQuestionSetCodes, req.CustomAttrFormIdForItem);
                    if (req.CustomAttrFormIdForRiskAssessment > 0)
                    {
                        SettingDetails reqSettingDetails = commonMamager.GetSettingsFromSettingsComponent(P2PDocumentType.Requisition, UserContext.ContactCode, (int)SubAppCodes.P2P, "", req.documentLOB.entityDetailCode);
                        bool enableRiskFormForRequisition = Convert.ToBoolean(commonMamager.GetSettingsValueByKey(reqSettingDetails, "EnableRiskFormForRequisition"));

                        req.CustomAttrQuestionSetCodesForRiskAssessment = GetQuestionnaireByFormCode(lstQuestionSetCodes, req.CustomAttrFormIdForRiskAssessment);
                        if (enableRiskFormForRequisition && req.EnableRiskForm && req.CustomAttrQuestionSetCodesForRiskAssessment.Count > 0 && (req.status.id == 1 || req.status.id == 24 || req.status.id == 121 || req.status.id == 169 || req.status.id == 23))
                            req.IsRiskFormMandatory = IsRiskFormMandatory(id, req.CustomAttrFormIdForRiskAssessment, req.CustomAttrQuestionSetCodesForRiskAssessment);
                    }
                }
                if (req.items != null)
                {
                    foreach (NewP2PEntities.RequisitionItem item in req.items)
                    {
                        if (item.CustomAttrQuestionSetCodesForItem != null && item.CustomAttrQuestionSetCodesForItem.Count > 0)
                            item.CustomAttrQuestionSetCodesForItem.AddRange(req.CustomAttrQuestionSetCodesForItem);
                        else
                            item.CustomAttrQuestionSetCodesForItem = req.CustomAttrQuestionSetCodesForItem;
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return req;
        }

        public void GetAllHeaderAccountingFieldsForRequisition(long id, out List<SplitAccountingFields> HeaderSplitAccountingFields)
        {
            HeaderSplitAccountingFields = GetNewReqDao().GetAllHeaderAccountingFieldsForRequisition(id);
        }
        public bool UpdateLineStatusForRequisition(long RequisitionId, P2P.BusinessEntities.StockReservationStatus LineStatus, bool IsUpdateAllItems, List<P2P.BusinessEntities.LineStatusRequisition> Items)
        {
            return GetNewReqDao().UpdateLineStatusForRequisition(RequisitionId, LineStatus, IsUpdateAllItems, Items);
        }


        public NewP2PEntities.Requisition GetDefaultEntities(NewP2PEntities.Requisition req)
        {
            List<DocumentAdditionalEntityInfo> lstHeaderEntityDetails = new List<DocumentAdditionalEntityInfo>();

            req.HeaderSplitAccountingFields.ForEach(e => lstHeaderEntityDetails.Add(new DocumentAdditionalEntityInfo
            {
                EntityCode = e.EntityCode,
                EntityDisplayName = e.EntityDisplayName,
                EntityDetailCode = e.EntityDetailCode,
                EntityId = e.EntityTypeId,
                Level = (int)e.LevelType,
                ParentEntityDetailCode = e.ParentEntityDetailCode,
                ParentEntityId = (int)e.ParentEntityType,
                IsAccountingEntity = e.IsAccountingEntity,
                LOBEntityDetailCode = e.LOBEntityDetailCode
            }));
            long lobId = 0;
            if (req.documentLOB != null && req.documentLOB.entityDetailCode > 0)
                lobId = req.documentLOB.entityDetailCode;
            else
                lobId = UserContext.BelongingEntityDetailCode;

            if (req.documentCode <= 0)
            {
                req.LineSplitAccountingFields = GetAllAccountingFieldsWithDefaultValuesFromCache(P2PDocumentType.Requisition, LevelType.ItemLevel, 0, 0, lstHeaderEntityDetails, null, false, 0, lobId, PreferenceLOBType.Belong);
            }
            else
            {

                RequisitionCommonManager commonMamager = new RequisitionCommonManager(this.JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
                SettingDetails reqSettingDetails = commonMamager.GetSettingsFromSettingsComponent(P2PDocumentType.Requisition, UserContext.ContactCode, (int)SubAppCodes.P2P, "", lobId);
                var splitAccountingFromManagerForExistingDocument = commonMamager.GetSettingsValueByKey(reqSettingDetails, "SplitAccountingFromCacheForExistingDocument");

                if (!string.IsNullOrEmpty(splitAccountingFromManagerForExistingDocument) && splitAccountingFromManagerForExistingDocument.ToLower() == "true")
                {
                    req.LineSplitAccountingFields = GetAllAccountingFieldsWithDefaultValuesFromCache(P2PDocumentType.Requisition, LevelType.ItemLevel, 0, 0, lstHeaderEntityDetails, null, false, 0, lobId, PreferenceLOBType.Belong);

                }
                else
                {

                    RequisitionDocumentManager documentManager = new RequisitionDocumentManager(this.JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
                    var accountingFieldsWithDefaultValues = documentManager.GetAllAccountingFieldsWithDefaultValues(P2PDocumentType.Requisition, LevelType.ItemLevel, 0, req.documentCode, lstHeaderEntityDetails, null, false, null, lobId, PreferenceLOBType.Belong, ADRIdentifier.lineNumber, req);
                    if (accountingFieldsWithDefaultValues != null && accountingFieldsWithDefaultValues.Count() > 0)
                    {
                        var ADRResult = accountingFieldsWithDefaultValues.FirstOrDefault();
                        if (ADRResult != null)
                            req.LineSplitAccountingFields = ADRResult.Splits;
                    }
                }
            }
            return req;
        }
        public void GenerateDefaultName(P2PDocumentType docType, long userId, long preDocumentId, long LOBEntityDetailCode, out string strDocumentName)
        {
            var documentManager = new RequisitionDocumentManager(this.JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
            strDocumentName = documentManager.GenerateDefaultName(docType, userId, preDocumentId, LOBEntityDetailCode, "");
        }

        /* public void GetNewReqNumber_Async(out string strDocumentNo)
         {
             IPartnerChannel objPartnerServiceChannel = null;
             long LOBEntityDetailCode = UserContext.BelongingEntityDetailCode;
             var objEntityNumber = new EntityNumberBO { UserContext = UserContext, GepConfiguration = GepConfiguration };
             if (LOBEntityDetailCode <= 0)
             {
                 objPartnerServiceChannel = GepServiceManager.GetInstance.CreateChannel<IPartnerChannel>(MultiRegionConfig.GetConfig(CloudConfig.PartnerServiceURL));
                 using (new OperationContextScope((objPartnerServiceChannel)))
                 {
                     UserContext userContext = null;
                     var objMhg =
                     new MessageHeader<UserExecutionContext>(UserContext);
                     var untypedmh = objMhg.GetUntypedHeader("GepCustomHeader", "Gep.Cumulus");
                     OperationContext.Current.OutgoingMessageHeaders.Add(untypedmh);
                     userContext = objPartnerServiceChannel.GetUserContextDetailsByContactCode(UserContext.ContactCode);
                     LOBEntityDetailCode = userContext.GetDefaultBelongingUserLOBMapping().EntityDetailCode;
                 }
             }
             strDocumentNo = objEntityNumber.GetEntityNumber("REQ", LOBEntityDetailCode);
      }*/

        public void getHeaderEntitiesAndBUList(NewP2PEntities.Requisition requisition, out List<SplitAccountingFields> lstHeaderSpltAccFlds, out List<NewPlatformEntities.DocumentBU> lstDocumentBU, NewPlatformEntities.Document document = null, long selectedHeaderentityLOBEntityDetailCode = 0)
        {
            var partnerHelper = new RESTAPIHelper.PartnerHelper(this.UserContext, JWTToken);
            string activities = partnerHelper.GetUserActivitiesByContactCode(UserContext.ContactCode, UserContext.BuyerPartnerCode);

            PreferenceLOBType preferenceLOBType = PreferenceLOBType.Belong;
            long LOBEntityDetailCode = UserContext.BelongingEntityDetailCode;
            //selectedHeaderentityLOBEntityDetailCode will be greater than 0 only if requisition is flipped from catalog
            if (selectedHeaderentityLOBEntityDetailCode > 0)
            {
                if (UserContext.BelongingEntityDetailCode == selectedHeaderentityLOBEntityDetailCode)
                {
                    preferenceLOBType = PreferenceLOBType.Belong;
                    LOBEntityDetailCode = UserContext.BelongingEntityDetailCode;
                }
                else 
                {
                    preferenceLOBType = PreferenceLOBType.Serve;
                    LOBEntityDetailCode = selectedHeaderentityLOBEntityDetailCode;
                }
            }
            else if ((activities.Contains("10100044") || activities.Contains("10700094")) && !activities.Contains("10700023") && !activities.Contains("10100043"))//Only direct requster
            {
                preferenceLOBType = PreferenceLOBType.Serve;
                LOBEntityDetailCode = UserContext.ServingEntityDetailCode;
            }

            if (requisition != null && requisition.HeaderSplitAccountingFields != null)
            {
                RequisitionDocumentManager objP2PDocumentManager = new RequisitionDocumentManager(this.JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
                lstHeaderSpltAccFlds = objP2PDocumentManager.GetAllAccountingFieldsWithDefaultValues(P2PDocumentType.Requisition, LevelType.HeaderLevel, 0, 0, null, requisition.HeaderSplitAccountingFields, false, 0, LOBEntityDetailCode, preferenceLOBType);
            }
            else
            {
                lstHeaderSpltAccFlds = GetAllAccountingFieldsWithDefaultValuesFromCache(P2PDocumentType.Requisition, LevelType.HeaderLevel, 0, 0, null, null, false, 0, LOBEntityDetailCode, preferenceLOBType, document);
            }
            lstDocumentBU = GetBUDetailList(lstHeaderSpltAccFlds, requisition != null ? requisition.documentCode : 0, (requisition != null && requisition.obo != null) ? requisition.obo.id : 0);
        }

        public CustomAttributesFormData GetCustomAttrFormId(int docType, long LOBEntityDetailCode)
        {
            CustomAttributesFormData result = new CustomAttributesFormData();

            RequisitionCommonManager commonMamager = new RequisitionCommonManager(this.JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
            var lstCustomAttrFrmIds = commonMamager.GetCustomAttrFormId(docType, new List<Level>() { Level.Header, Level.Item, Level.Distribution, Level.RiskAssessment }, 0, LOBEntityDetailCode);
            long header = 0;
            long line = 0;
            long split = 0;
            long riskAssessment = 0;
            foreach (var lvlForm in lstCustomAttrFrmIds)
            {
                switch (lvlForm.Key)
                {
                    case P2P.BusinessEntities.Level.Header:
                        header = lvlForm.Value;
                        break;
                    case P2P.BusinessEntities.Level.Item:
                        line = lvlForm.Value;
                        break;
                    case P2P.BusinessEntities.Level.Distribution:
                        split = lvlForm.Value;
                        break;
                    case P2P.BusinessEntities.Level.RiskAssessment:
                        riskAssessment = lvlForm.Value;
                        break;
                }
            }

            result.CustomAttrFormIdForHeader = header;
            result.CustomAttrFormIdForItem = line;
            result.CustomAttrFormIdForSplit = split;
            result.CustomAttrFormIdForRiskAssessment = riskAssessment;
            result.lstQuestionnaire = GetAllQuestionnaireByFormCodes(result.CustomAttrFormIdForHeader, result.CustomAttrFormIdForItem, result.CustomAttrFormIdForRiskAssessment);

            return result;
        }

        // Please don't use this method for line level defaulting
        public List<SplitAccountingFields> GetAllAccountingFieldsWithDefaultValuesFromCache(P2PDocumentType docType, LevelType levelType,
                                                                                       long ContactCode = 0, long docmentCode = 0,
                                                                                       List<DocumentAdditionalEntityInfo> lstHeaderEntityDetails = null,
                                                                                       List<SplitAccountingFields> lstSplitAccountingFields = null,
                                                                                       bool populateDefaultSplitValue = false, long documentItemId = 0, long lOBEntityDetailCode = 0,
                                                                                       PreferenceLOBType preferenceLOBType = PreferenceLOBType.Serve, NewPlatformEntities.Document document = null)
        {
            RequisitionDocumentManager documentManager = new RequisitionDocumentManager(this.JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
            if (ContactCode == 0)
                ContactCode = UserContext.ContactCode;
            if (levelType == LevelType.HeaderLevel)
            {
                List<SplitAccountingFields> lstDefaultAccounting = GEPDataCache.GetFromCacheJSON<List<SplitAccountingFields>>(String.Concat("HeaderAccounting", "_", docType, "_", lOBEntityDetailCode, "_", preferenceLOBType), UserContext.BuyerPartnerCode, ContactCode, "en-US");
                if (lstDefaultAccounting == null)
                {
                    try
                    {
                        List<ADRSplit> adrSplits = documentManager.GetAllAccountingFieldsWithDefaultValues(docType, levelType, 0, docmentCode, null, null, false, null, lOBEntityDetailCode, preferenceLOBType, ADRIdentifier.DocumentCode, document);
                        if (adrSplits != null && adrSplits.Count > 0 && adrSplits[0].Splits != null)
                        {
                            lstDefaultAccounting = adrSplits[0].Splits;
                        }

                    }
                    catch (Exception e)
                    {
                        LogHelper.LogError(Log, "Error occurred in GetAllAccountingFieldsWithDefaultValuesFromCache method for header split accounting fields.", e);

                        throw e;
                    }
                    GEPDataCache.PutInCacheJSON<List<SplitAccountingFields>>(String.Concat("HeaderAccounting", "_", docType, "_", lOBEntityDetailCode, "_", preferenceLOBType), UserContext.BuyerPartnerCode, ContactCode, "en-US", lstDefaultAccounting);
                }
                return lstDefaultAccounting;
            }
            else
            {
                List<SplitAccountingFields> lstDefaultAccounting = GEPDataCache.GetFromCacheJSON<List<SplitAccountingFields>>(String.Concat("LineAccounting", "_", docType, "_", lOBEntityDetailCode, "_", preferenceLOBType), UserContext.BuyerPartnerCode, ContactCode, "en-US");
                if (lstDefaultAccounting == null)
                {
                    try
                    {
                        List<ADRSplit> adrSplits = documentManager.GetAllAccountingFieldsWithDefaultValues(docType, levelType, 0, docmentCode, lstHeaderEntityDetails, null, false, null, lOBEntityDetailCode, preferenceLOBType, ADRIdentifier.lineNumber, document);
                        if (adrSplits != null && adrSplits.Count > 0 && adrSplits[0].Splits != null)
                        {
                            lstDefaultAccounting = adrSplits[0].Splits;
                        }
                    }
                    catch (Exception e)
                    {
                        LogHelper.LogError(Log, "Error occurred in GetAllAccountingFieldsWithDefaultValuesFromCache method for Line split accounting fields.", e);

                        throw e;
                    }
                    GEPDataCache.PutInCacheJSON<List<SplitAccountingFields>>(String.Concat("LineAccounting", "_", docType, "_", lOBEntityDetailCode, "_", preferenceLOBType), UserContext.BuyerPartnerCode, ContactCode, "en-US", lstDefaultAccounting);
                }
                return lstDefaultAccounting;
            }
        }

        /// <summary>
        /// SaveBuyerAssignee
        /// </summary>
        /// <param name="DocumentCode"></param>
        /// <param name="BuyerAssigneeValue"></param>
        /// <returns></returns>
        public int SaveBuyerAssignee(long[] DocumentCodes, long BuyerAssigneeValue, long PrevOrderContact)
        {
            int results = 0;
            results = GetNewReqDao().SaveBuyerAssignee(DocumentCodes, BuyerAssigneeValue);
            if (results > 0)
            {
                // Task tbuyerAssignee = Task.Factory.StartNew(() => 

                if (UserContext.ContactCode != BuyerAssigneeValue)
                {
                    DocumentCodes.ToList().ForEach(objDocumentCode =>
                    {
                        SendEmailToOrderContact(objDocumentCode, BuyerAssigneeValue, PrevOrderContact);
                    });

                }
            }
            return results;
        }

        public void SendEmailToOrderContact(long documentCode, long BuyerAssigneeValue, long PrevOrderContact)
        {

            RequisitionEmailNotificationManager emailNotificationManager = new RequisitionEmailNotificationManager(this.UserContext, this.JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
            emailNotificationManager.SendNotificationForBuyerAssignee(documentCode, BuyerAssigneeValue, PrevOrderContact);

        }
        /// <summary>
        /// saves the complete Req object including(header,Item,Split)
        /// </summary>
        /// <param name="req">req object</param>
        /// <returns>Result</returns>
        [Transaction]
        public SaveResult SaveCompleteRequisition(NewP2PEntities.Requisition req)
        {
            RequisitionCommonManager objP2PCommonManager = new RequisitionCommonManager(this.JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
            RequisitionDocumentManager objP2PDocumentManager = new RequisitionDocumentManager(this.JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
            SettingDetails reqSettingDetails = objP2PCommonManager.GetSettingsFromSettingsComponent(P2PDocumentType.Requisition, UserContext.ContactCode, (int)SubAppCodes.P2P, "", req.documentLOB.entityDetailCode);

            SaveResult result = new SaveResult();
            //if (String.IsNullOrEmpty(req.number))
            //    req.number = GetNewReqNumber();

            bool saveRequisition = true;
            bool? IsPeriodbasedonNeedbyDate = Convert.ToBoolean(objP2PCommonManager.GetSettingsValueByKey(reqSettingDetails, "IsPeriodbasedonNeedbyDate"));
            bool enableRiskFormForRequisition = objP2PCommonManager.GetSettingsValueByKey(reqSettingDetails, "EnableRiskFormForRequisition") == string.Empty ? false : Convert.ToBoolean(objP2PCommonManager.GetSettingsValueByKey(reqSettingDetails, "EnableRiskFormForRequisition"));
            var AllowEditofCustAttrbyApproveronSubmittedReqs = objP2PCommonManager.GetSettingsValueByKey(reqSettingDetails, "AllowEditofCustAttrbyApproveronSubmittedReqs") == string.Empty ? false : Convert.ToBoolean(objP2PCommonManager.GetSettingsValueByKey(reqSettingDetails, "AllowEditofCustAttrbyApproveronSubmittedReqs"));
            var AllowEditAccounting = objP2PCommonManager.GetSettingsValueByKey(reqSettingDetails, "AllowEditAccounting") == string.Empty ? false : Convert.ToBoolean(objP2PCommonManager.GetSettingsValueByKey(reqSettingDetails, "AllowEditAccounting"));
            var p2pSettings = objP2PCommonManager.GetSettingsFromSettingsComponent(P2PDocumentType.None, UserContext.ContactCode, (int)SubAppCodes.P2P, "", req.documentLOB.entityDetailCode);

            bool? IsOBEnabled = Convert.ToBoolean(objP2PCommonManager.GetSettingsValueByKey(p2pSettings, "IsOperationalBudgetEnabled"));
            int precessionValue = convertStringToInt(objP2PCommonManager.GetSettingsValueByKey(p2pSettings, "MaxPrecessionValue"));
            int maxPrecessionforTotal = convertStringToInt(objP2PCommonManager.GetSettingsValueByKey(p2pSettings, "MaxPrecessionValueforTotal"));
            int maxPrecessionForTaxesAndCharges = convertStringToInt(objP2PCommonManager.GetSettingsValueByKey(p2pSettings, "MaxPrecessionValueForTaxesAndCharges"));

            if (String.IsNullOrEmpty(req.number))
            {
                //// req.number = GetNewReqNumber();
                string reqNumber = string.Empty;
                string reqName = string.Empty;

                if (req.documentLOB.entityDetailCode == 0)
                    req.documentLOB.entityDetailCode = GetDocumentLOB(P2PDocumentType.Requisition);

                var entityIdMappedToNumberConfiguration = objP2PCommonManager.GetSettingsValueByKey(p2pSettings, "EntityIdMappedToNumberConfiguration") == string.Empty ? 0 : Convert.ToInt32(objP2PCommonManager.GetSettingsValueByKey(p2pSettings, "EntityIdMappedToNumberConfiguration"));
                long EntityDetailcode = 0;
                if (req.HeaderSplitAccountingFields != null && req.HeaderSplitAccountingFields.Count() > 0)
                {
                    var NumberConfigEntity = req.HeaderSplitAccountingFields.FirstOrDefault(p => p.EntityTypeId == entityIdMappedToNumberConfiguration);
                    if (NumberConfigEntity != null)
                        EntityDetailcode = NumberConfigEntity.EntityDetailCode;
                }
                req.number = GetEntityNumberForRequisition("REQ", req.documentLOB.entityDetailCode, EntityDetailcode);
            }

            if (req.id > 0)
            {
                saveRequisition = false;
                Documents.Entities.Document objDocument = GetDocumentDao().GetDocumentBasicDetails(req.id);
                if ((Convert.ToInt16(objDocument.DocumentStatusInfo) == 1 || Convert.ToInt16(objDocument.DocumentStatusInfo) == 24 || Convert.ToInt16(objDocument.DocumentStatusInfo) == 121 ||
                    Convert.ToInt16(objDocument.DocumentStatusInfo) == 169 || Convert.ToInt16(objDocument.DocumentStatusInfo) == 23 || Convert.ToInt16(objDocument.DocumentStatusInfo) == 56 || ((Convert.ToInt16(objDocument.DocumentStatusInfo)) == 202 && (req.status.id == 23 || req.status.id == 28 || req.status.id == 202)) || (Convert.ToInt16(objDocument.DocumentStatusInfo) == 22 && req.status.id == 56)) || (Convert.ToInt16(objDocument.DocumentStatusInfo) == 21 && (Convert.ToInt16(objDocument.DocumentStatusInfo) == req.status.id) && (AllowEditofCustAttrbyApproveronSubmittedReqs || AllowEditAccounting)))
                    saveRequisition = true;
                else
                {
                    result = new SaveResult { success = false, id = req.id, number = req.number, requisition = GetRequisitionDisplayDetails(req.id), message = REQ_SUBMIT_ERROR, messageType = "warning" };
                    return result;
                }
            }

            if (req.id == 0)
            {
                req.createdOn = DateTime.Now;
                req.lastModifiedOn = DateTime.Now;
            }
            if (saveRequisition)
            {

                try
                {
                    var eventAttributes = new Dictionary<string, object>();
                    eventAttributes.Add("RequisitionName", req.name);
                    eventAttributes.Add("ItemCount", req.items.Count);
                    eventAttributes.Add("RequisitionSource", req.RequisitionSource);
                    eventAttributes.Add("DocumentCode", req.documentCode);
                    NewRelic.Api.Agent.NewRelic.RecordCustomEvent("SaveEventBegin", eventAttributes);
                }
                catch (Exception)
                {
                    Dictionary<string, object> msg = new Dictionary<string, object>();
                    NewRelic.Api.Agent.NewRelic.NoticeError("Error Occured on New Relic Instrumentaion", msg);
                }


                List<NewPlatformEntities.DocumentBU> documentBUs = GetBUDetailList(req.HeaderSplitAccountingFields, req.documentCode, req.obo != null ? req.obo.id : 0);
                result = GetNewReqDao().SaveAllRequisitionDetails(req, precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges, documentBUs);

            }
            if (req.id == 0)
            {
                #region My Task Implementation
                var currentPrincipal = System.Threading.Thread.CurrentPrincipal;
                Task.Factory.StartNew((userContext) =>
                {
                    System.Threading.Thread.CurrentPrincipal = currentPrincipal;
                    List<TaskActionDetails> lstTasksAction = new List<TaskActionDetails>();
                    lstTasksAction.Add(TaskManager.CreateActionDetails(ActionKey.SentForApproval, SENT_FOR_APPROVAl));
                    var objTaskManager = new TaskManager() { UserContext = UserContext, GepConfiguration = GepConfiguration };
                    var delegateSaveTaskActionDetails = new TaskManager.InvokeSaveTaskActionDetails(objTaskManager.SaveTaskActionDetails);
                    delegateSaveTaskActionDetails.BeginInvoke(TaskManager.CreateTaskObject(result.id, ((UserExecutionContext)userContext).ContactCode, lstTasksAction, false, false, ((UserExecutionContext)userContext).BuyerPartnerCode, ((UserExecutionContext)userContext).CompanyName), null, null);
                }, this.UserContext);
                #endregion
            }

            if (result.success && (req.OnEvent == OnEvent.OnSubmit || req.OnEvent == OnEvent.OnSubmitFromManageApproval || req.OnEvent == OnEvent.OnApprove)
                    && (req.status.id == 1 || req.status.id == 24 || req.status.id == 121 ||
                    req.status.id == 169 || req.status.id == 23 || req.status.id == 21))
                objP2PCommonManager.ValidateAllAccountingCodeCombination(result.id, DocumentType.Requisition);

            #region Update Period by Need by date
            if (IsOBEnabled.HasValue && IsOBEnabled.Value == true)
            {
                if (req.items != null && req.items.Count > 0)
                {
                    objP2PDocumentManager.UpdateAllPeriodbyNeedbyDate(P2PDocumentType.Requisition, result.id, IsPeriodbasedonNeedbyDate.Value);
                }
            }
            #endregion


            if (result.success && enableRiskFormForRequisition && req.EnableRiskForm && (req.OnEvent == OnEvent.OnSubmit || req.OnEvent == OnEvent.OnSubmitFromManageApproval))
            {
                bool isRiskFormMandatory = IsRequiredRiskCalculation(result.id, req.CustomAttrFormIdForRiskAssessment);
                GetNewReqDao().CalculateAndUpdateRiskScore(result.id, isRiskFormMandatory);
            }

            result.requisition = GetRequisitionDisplayDetails(result.id);
            try
            {
                if (result.success)
                {
                    try
                    {
                        var eventAttributes = new Dictionary<string, object>();
                        eventAttributes.Add("DocumentId", result.requisition.id);
                        NewRelic.Api.Agent.NewRelic.RecordCustomEvent("SaveEventSuccess", eventAttributes);
                    }
                    catch (Exception)
                    {

                        Dictionary<string, object> msg = new Dictionary<string, object>();
                        NewRelic.Api.Agent.NewRelic.NoticeError("Error Occured on New Relic Instrumentaion", msg);
                    }

                    bool chargeexists = false;
                    if (!ReferenceEquals(req.ItemChargesForHeader, null) && req.ItemChargesForHeader.Count > 0)
                    {
                        req.ItemChargesForHeader.Where(w => w.DocumentCode == 0).Select(w => w.DocumentCode = result.id).ToList();
                        SaveReqAllItemCharges(ConvertReqObject(req.ItemChargesForHeader));
                        chargeexists = true;
                    }

                    if (req.items != null && req.items.Count > 0)
                    {
                        foreach (NewP2PEntities.RequisitionItem item in req.items)
                        {
                            if (!ReferenceEquals(item.ItemChargesForSubLine, null) && item.ItemChargesForSubLine.Count > 0)
                            {
                                SaveReqAllItemCharges(ConvertReqObject(item.ItemChargesForSubLine));
                                chargeexists = true;
                            }

                        }
                    }
                    if (chargeexists)
                    {
                        result = new SaveResult { success = result.success, id = result.id, number = result.number, requisition = GetRequisitionDisplayDetails(result.id) };
                    }
                    if (req.id > 0 && (req.status.id == 1 || req.status.id == 23 || req.status.id == 24 || req.status.id == 121 || req.status.id == 202) && req.OnEvent != OnEvent.OnSubmitFromManageApproval)
                    {
                        string strEnablePureAdHocApproval = objP2PCommonManager.GetSettingsValueByKey(p2pSettings, "EnableAdhocApprovalOnly");
                        if (!string.IsNullOrEmpty(strEnablePureAdHocApproval) && !Convert.ToBoolean(strEnablePureAdHocApproval))
                            objP2PDocumentManager.ResetAllApprovers(result.id, 7, "Reset");
                    }
                }
            }
            finally
            {
                RequisitionDocumentManager objP2PDocumentMngr = new RequisitionDocumentManager(this.JWTToken)
                {
                    UserContext = UserContext,
                    GepConfiguration = GepConfiguration
                };
                if (result.success)
                {
                    objP2PDocumentMngr.BroadcastPusher(result.id, Gep.Cumulus.CSM.Entities.DocumentType.Requisition, "DataStale", "SaveCompleteRequisition");

                }
            }
            return result;
        }

        /// <summary>
        /// saves the complete Req object including(header,Item,Split)
        /// </summary>
        /// <param name="req">req object</param>
        /// <returns>Result</returns>
        [Transaction]
        public SaveResult CreateRequisitionFromRequestForm(NewP2PEntities.Requisition req)
        {
            RequisitionCommonManager objP2PCommonManager = new RequisitionCommonManager(this.JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
            RequisitionDocumentManager objP2PDocumentManager = new RequisitionDocumentManager(this.JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
            SettingDetails reqSettingDetails = objP2PCommonManager.GetSettingsFromSettingsComponent(P2PDocumentType.Requisition, UserContext.ContactCode, (int)SubAppCodes.P2P, "", req.documentLOB.entityDetailCode);

            SaveResult result = new SaveResult();
            if (String.IsNullOrEmpty(req.number))
                req.number = GetNewReqNumber();

            bool saveRequisition = true;
            bool? IsPeriodbasedonNeedbyDate = Convert.ToBoolean(objP2PCommonManager.GetSettingsValueByKey(reqSettingDetails, "IsPeriodbasedonNeedbyDate"));
            bool enableRiskFormForRequisition = objP2PCommonManager.GetSettingsValueByKey(reqSettingDetails, "EnableRiskFormForRequisition") == string.Empty ? false : Convert.ToBoolean(objP2PCommonManager.GetSettingsValueByKey(reqSettingDetails, "EnableRiskFormForRequisition"));
            var AllowEditofCustAttrbyApproveronSubmittedReqs = objP2PCommonManager.GetSettingsValueByKey(reqSettingDetails, "AllowEditofCustAttrbyApproveronSubmittedReqs") == string.Empty ? false : Convert.ToBoolean(objP2PCommonManager.GetSettingsValueByKey(reqSettingDetails, "AllowEditofCustAttrbyApproveronSubmittedReqs"));
            var AllowEditAccounting = objP2PCommonManager.GetSettingsValueByKey(reqSettingDetails, "AllowEditAccounting") == string.Empty ? false : Convert.ToBoolean(objP2PCommonManager.GetSettingsValueByKey(reqSettingDetails, "AllowEditAccounting"));
            var p2pSettings = objP2PCommonManager.GetSettingsFromSettingsComponent(P2PDocumentType.None, UserContext.ContactCode, (int)SubAppCodes.P2P, "", req.documentLOB.entityDetailCode);

            bool? IsOBEnabled = Convert.ToBoolean(objP2PCommonManager.GetSettingsValueByKey(p2pSettings, "IsOperationalBudgetEnabled"));
            int precessionValue = convertStringToInt(objP2PCommonManager.GetSettingsValueByKey(p2pSettings, "MaxPrecessionValue"));
            int maxPrecessionforTotal = convertStringToInt(objP2PCommonManager.GetSettingsValueByKey(p2pSettings, "MaxPrecessionValueforTotal"));
            int maxPrecessionForTaxesAndCharges = convertStringToInt(objP2PCommonManager.GetSettingsValueByKey(p2pSettings, "MaxPrecessionValueForTaxesAndCharges"));
            if (req.obo.id == 0)
            {
                req.obo.id = req.createdBy.id > 0 ? req.createdBy.id : UserContext.ContactCode;
            }
            if (req.RequesterID == 0)
            {
                req.RequesterID = UserContext.ContactCode;
            }
            if (String.IsNullOrEmpty(req.number))
            {
                //// req.number = GetNewReqNumber();
                string reqNumber = string.Empty;
                string reqName = string.Empty;

                if (req.documentLOB.entityDetailCode == 0)
                    req.documentLOB.entityDetailCode = GetDocumentLOB(P2PDocumentType.Requisition);

                var entityIdMappedToNumberConfiguration = Convert.ToInt32(objP2PCommonManager.GetSettingsValueByKey(P2PDocumentType.None, "EntityIdMappedToNumberConfiguration", UserContext.UserId, (int)SubAppCodes.P2P, "", req.documentLOB.entityDetailCode));
                var EntityDetailcode = (req.HeaderSplitAccountingFields != null && req.HeaderSplitAccountingFields.Count() > 0) ? req.HeaderSplitAccountingFields.FirstOrDefault(p => p.EntityTypeId == entityIdMappedToNumberConfiguration).EntityDetailCode : 0;
                GetNewDocumentNumber(out reqNumber, out reqName, "REQ", P2PDocumentType.Requisition, req.documentLOB.entityDetailCode, EntityDetailcode);
                req.number = reqNumber;
            }
            if (String.IsNullOrEmpty(req.name))
            {
                string strDocumentName = string.Empty;
                GenerateDefaultName(P2PDocumentType.Requisition, UserContext.ContactCode, 0, 0, out strDocumentName);
                req.name = strDocumentName;
            }

            if (req.id == 0)
            {
                req.createdOn = DateTime.Now;
                req.lastModifiedOn = DateTime.Now;
            }
            if (saveRequisition)
            {

                try
                {
                    var eventAttributes = new Dictionary<string, object>();
                    eventAttributes.Add("RequisitionName", req.name);
                    eventAttributes.Add("ItemCount", req.items.Count);
                    eventAttributes.Add("RequisitionSource", req.RequisitionSource);
                    eventAttributes.Add("DocumentCode", req.documentCode);
                    NewRelic.Api.Agent.NewRelic.RecordCustomEvent("SaveEventBegin", eventAttributes);
                }
                catch (Exception)
                {
                    Dictionary<string, object> msg = new Dictionary<string, object>();
                    NewRelic.Api.Agent.NewRelic.NoticeError("Error Occured on New Relic Instrumentaion", msg);
                }


                List<NewPlatformEntities.DocumentBU> documentBUs = GetBUDetailList(req.HeaderSplitAccountingFields, req.documentCode, req.obo != null ? req.obo.id : 0);
                result = GetNewReqDao().SaveAllRequisitionDetails(req, precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges, documentBUs);

            }

            #region Update Period by Need by date
            if (IsOBEnabled.HasValue && IsOBEnabled.Value == true)
            {
                if (req.items != null && req.items.Count > 0)
                {
                    objP2PDocumentManager.UpdateAllPeriodbyNeedbyDate(P2PDocumentType.Requisition, result.id, IsPeriodbasedonNeedbyDate.Value);
                }
            }
            #endregion

            result.requisition.id = result.id;

            try
            {
                if (result.success)
                {
                    try
                    {
                        var eventAttributes = new Dictionary<string, object>();
                        eventAttributes.Add("DocumentId", result.requisition.id);
                        NewRelic.Api.Agent.NewRelic.RecordCustomEvent("SaveEventSuccess", eventAttributes);
                    }
                    catch (Exception)
                    {
                        Dictionary<string, object> msg = new Dictionary<string, object>();
                        NewRelic.Api.Agent.NewRelic.NoticeError("Error Occured on New Relic Instrumentaion", msg);
                    }

                    bool chargeexists = false;
                    if (!ReferenceEquals(req.ItemChargesForHeader, null) && req.ItemChargesForHeader.Count > 0)
                    {
                        req.ItemChargesForHeader.Where(w => w.DocumentCode == 0).Select(w => w.DocumentCode = result.id).ToList();
                        SaveReqAllItemCharges(ConvertReqObject(req.ItemChargesForHeader));
                        chargeexists = true;
                    }

                    if (req.items != null && req.items.Count > 0)
                    {
                        foreach (NewP2PEntities.RequisitionItem item in req.items)
                        {
                            if (!ReferenceEquals(item.ItemChargesForSubLine, null) && item.ItemChargesForSubLine.Count > 0)
                            {
                                SaveReqAllItemCharges(ConvertReqObject(item.ItemChargesForSubLine));
                                chargeexists = true;
                            }

                        }
                    }

                }
            }
            finally
            {
                RequisitionDocumentManager objP2PDocumentMngr = new RequisitionDocumentManager(this.JWTToken)
                {
                    UserContext = UserContext,
                    GepConfiguration = GepConfiguration
                };
                if (result.success)
                {
                    objP2PDocumentMngr.BroadcastPusher(result.id, Gep.Cumulus.CSM.Entities.DocumentType.Requisition, "DataStale", "SaveCompleteRequisition");

                }
            }
            return result;
        }
        public List<Questionnaire> GetAllQuestionnaireByFormCodes(long headerFormCode, long lineFormCode, List<Questionnaire> lstQuestionSetCodes, long riskFormCode = 0)
        {
            DataTable FormCodes = new DataTable() { Locale = CultureInfo.InvariantCulture, TableName = "tvp_Long" };
            FormCodes.Columns.Add("Id", typeof(long));
            if (headerFormCode > 0)
            {
                DataRow dr = FormCodes.NewRow();
                dr["Id"] = headerFormCode;
                FormCodes.Rows.Add(dr);
            }
            if (lineFormCode > 0)
            {
                DataRow dr = FormCodes.NewRow();
                dr["Id"] = lineFormCode;
                FormCodes.Rows.Add(dr);
            }
            if (riskFormCode > 0)
            {
                DataRow dr = FormCodes.NewRow();
                dr["Id"] = riskFormCode;
                FormCodes.Rows.Add(dr);
            }
            if (FormCodes.Rows.Count > 0)
                lstQuestionSetCodes = GetCommonDao().GetAllQuestionnaireByFormCodes(FormCodes);
            else
                lstQuestionSetCodes = new List<Questionnaire>();

            return lstQuestionSetCodes;
        }

        public List<Questionnaire> GetAllQuestionnaireByFormCodes(long headerFormCode, long lineFormCode, long riskFormCode)
        {
            List<Questionnaire> objResult = new List<Questionnaire>();
            DataTable FormCodes = new DataTable() { Locale = CultureInfo.InvariantCulture, TableName = "tvp_Long" };
            FormCodes.Columns.Add("Id", typeof(long));
            if (headerFormCode > 0)
            {
                DataRow dr = FormCodes.NewRow();
                dr["Id"] = headerFormCode;
                FormCodes.Rows.Add(dr);
            }
            if (lineFormCode > 0 && headerFormCode != lineFormCode)
            {
                DataRow dr = FormCodes.NewRow();
                dr["Id"] = lineFormCode;
                FormCodes.Rows.Add(dr);
            }
            if (riskFormCode > 0)
            {
                DataRow dr = FormCodes.NewRow();
                dr["Id"] = riskFormCode;
                FormCodes.Rows.Add(dr);
            }
            if (FormCodes.Rows.Count > 0)
                objResult = GetCommonDao().GetAllQuestionnaireByFormCodes(FormCodes);

            return objResult;
        }

        public List<IdAndName> GetQuestionnaireByFormCode(List<Questionnaire> lstQuestionnair, long formCode, bool filterOnUser = true)
        {
            List<IdAndName> questionSetCodes = new List<IdAndName>();
            if (!ReferenceEquals(lstQuestionnair, null) && lstQuestionnair.Count > 0)
                foreach (var qSet in lstQuestionnair)
                {
                    if (!filterOnUser || qSet.IsSupplierVisible || !this.UserContext.IsSupplier)
                    {
                        if (qSet.QuestionnaireCode > 0 && qSet.FormCode == formCode)
                        {
                            questionSetCodes.Add(new IdAndName
                            {
                                id = qSet.QuestionnaireCode,
                                name = qSet.QuestionnaireTitle
                            });
                        }
                    }
                }
            return questionSetCodes;
        }

        public List<P2P.BusinessEntities.DocumentSplitItemEntity> getSplitItemEntities(ReqAccountingSplit split, List<P2P1.SplitAccountingFields> splitEntityReq)
        {
            List<P2P.BusinessEntities.DocumentSplitItemEntity> lstSplitEntity = new List<P2P.BusinessEntities.DocumentSplitItemEntity>();

            if (split.requester != null)
                lstSplitEntity.Add(getSplitEntity(split.requester, split.id, splitEntityReq));
            if (split.gLCode != null)
                lstSplitEntity.Add(getSplitEntity(split.gLCode, split.id, splitEntityReq));
            if (split.period != null)
                lstSplitEntity.Add(getSplitEntity(split.period, split.id, splitEntityReq));

            if (split.splitEntity1 != null)
            {
                lstSplitEntity.Add(getSplitEntity(split.splitEntity1, split.id, splitEntityReq));
                if (split.splitEntity2 != null)
                {
                    lstSplitEntity.Add(getSplitEntity(split.splitEntity2, split.id, splitEntityReq));
                    if (split.splitEntity3 != null)
                    {
                        lstSplitEntity.Add(getSplitEntity(split.splitEntity3, split.id, splitEntityReq));
                        if (split.splitEntity4 != null)
                        {
                            lstSplitEntity.Add(getSplitEntity(split.splitEntity4, split.id, splitEntityReq));
                            if (split.splitEntity5 != null)
                            {
                                lstSplitEntity.Add(getSplitEntity(split.splitEntity5, split.id, splitEntityReq));
                                if (split.splitEntity6 != null)
                                {
                                    lstSplitEntity.Add(getSplitEntity(split.splitEntity6, split.id, splitEntityReq));
                                    if (split.splitEntity7 != null)
                                    {
                                        lstSplitEntity.Add(getSplitEntity(split.splitEntity7, split.id, splitEntityReq));
                                        if (split.splitEntity8 != null)
                                        {
                                            lstSplitEntity.Add(getSplitEntity(split.splitEntity8, split.id, splitEntityReq));
                                            if (split.splitEntity9 != null)
                                            {
                                                lstSplitEntity.Add(getSplitEntity(split.splitEntity9, split.id, splitEntityReq));
                                                if (split.splitEntity10 != null)
                                                {
                                                    lstSplitEntity.Add(getSplitEntity(split.splitEntity10, split.id, splitEntityReq));
                                                    if (split.splitEntity11 != null)
                                                    {
                                                        lstSplitEntity.Add(getSplitEntity(split.splitEntity11, split.id, splitEntityReq));
                                                        if (split.splitEntity12 != null)
                                                        {
                                                            lstSplitEntity.Add(getSplitEntity(split.splitEntity12, split.id, splitEntityReq));
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return lstSplitEntity;
        }
        public P2P.BusinessEntities.DocumentSplitItemEntity getSplitEntity(SplitEntity accSplitEnt, long SplitItemId, List<P2P1.SplitAccountingFields> splitEntityReq)
        {
            if (accSplitEnt.title != null)
                accSplitEnt.fieldId = splitEntityReq.Where(x => x.Title == accSplitEnt.title).Select(x => x.SplitAccountingFieldId).FirstOrDefault();
            else
                accSplitEnt.fieldId = splitEntityReq.Where(x => x.EntityTypeId == accSplitEnt.entityType).Select(x => x.SplitAccountingFieldId).FirstOrDefault();

            P2P.BusinessEntities.DocumentSplitItemEntity objSplitEntity = new P2P.BusinessEntities.DocumentSplitItemEntity()
            {
                DocumentSplitItemId = SplitItemId,
                DocumentSplitItemEntityId = accSplitEnt.splitEntityId,
                EntityCode = accSplitEnt.entityCode,
                EntityTypeId = accSplitEnt.entityType,
                EntityDisplayName = accSplitEnt.name,
                SplitAccountingFieldId = (int)accSplitEnt.fieldId,
                SplitAccountingFieldValue = accSplitEnt.code
            };

            return objSplitEntity;
        }
        public List<NewPlatformEntities.DocumentBU> GetBUDetailList(List<SplitAccountingFields> HeaderSplitAccountingFields, long documentCode, long onBehalfof = 0)
        {
            List<NewPlatformEntities.DocumentBU> lstBU = new List<NewPlatformEntities.DocumentBU>();
            List<DocumentAdditionalEntityInfo> documentAdditionalEntityInfolst = new List<DocumentAdditionalEntityInfo>();
            HeaderSplitAccountingFields.ForEach(splits =>
            {
                documentAdditionalEntityInfolst.Add(new DocumentAdditionalEntityInfo
                {
                    EntityId = splits.EntityTypeId,
                    EntityCode = splits.EntityCode,
                    EntityDetailCode = splits.EntityDetailCode,
                    EntityDisplayName = splits.EntityDisplayName
                });
            });
            P2PAccessControlManager objP2PAccessControlManager = new P2PAccessControlManager { UserContext = UserContext, GepConfiguration = GepConfiguration, jwtToken = this.JWTToken };
            var documentBus = objP2PAccessControlManager.getBuDetailsForDocument(P2PDocumentType.Requisition, UserContext, 0, onBehalfof, documentAdditionalEntityInfolst).ToList();
            documentBus.ForEach(docBU =>
            {
                lstBU.Add(new NewPlatformEntities.DocumentBU
                {
                    buCode = docBU.BusinessUnitCode,
                    buName = docBU.BusinessUnitName,
                    documentCode = documentCode
                });
            });
            return lstBU;
        }

        public SaveResult AutoSaveDocument(NewP2PEntities.Requisition objectData, Int64 documentCode, int documentTypeCode, Int64 lastModifiedBy)
        {
            return GetNewReqDao().AutoSaveDocument(objectData, documentCode, documentTypeCode, lastModifiedBy);
        }

        public NewP2PEntities.Requisition GetAutoSaveDocument(Int64 documentCode, int documentTypeCode)
        {
            NewP2PEntities.Requisition req = GetNewReqDao().GetAutoSaveDocument(documentCode, documentTypeCode);
            return req;
        }

        public List<P2PUserConfiguration> GetUserConfigurations(long contactCode, int documentType)
        {
            return GetNewReqDao().GetUserConfigurations(contactCode, documentType);
        }

        public SaveResult SaveUserConfigurations(P2PUserConfiguration userConfig)
        {
            return GetNewReqDao().SaveUserConfigurations(userConfig);
        }

        public List<IdAndName> GetContractItemsByContractNumber(string documentNumber, string term, int itemType)
        {
            return GetNewReqDao().GetContractItemsByContractNumber(documentNumber, term, itemType);
        }
        public bool ValidateContractNumber(string contractNumber)
        {
            return GetNewReqDao().ValidateContractNumber(contractNumber);
        }
        public bool ValidateContractItemId(string contractNumber, long ContractItemId)
        {
            return GetNewReqDao().ValidateContractItemId(contractNumber, ContractItemId);
        }

        public List<SavedViewDetails> GetSavedViewsForReqWorkBench(long LobId)
        {
            return GetNewReqDao().GetSavedViewsForReqWorkBench(UserContext.ContactCode, 7, LobId);
        }
        public long InsertUpdateSavedViewsForReqWorkBench(SavedViewDetails objSavedView)
        {
            return GetNewReqDao().InsertUpdateSavedViewsForReqWorkBench(objSavedView);
        }
        public bool DeleteSavedViewsForReqWorkBench(long savedViewId)
        {
            return GetNewReqDao().DeleteSavedViewsForReqWorkBench(savedViewId);
        }

        #endregion
        public bool AssignBuyerToRequisitionItems(long buyerContactCode, string requisitionItemIds)
        {
            return GetNewReqDao().AssignBuyerToRequisitionItems(buyerContactCode, requisitionItemIds);
        }
        public List<BuyerInfo> GetAssignBuyersList(string organizationEntityIds, string documentCodes)
        {
            ProxyPartnerService proxyPartnerService = new ProxyPartnerService(UserContext, this.JWTToken);
            //IPartnerChannel objPartnerServiceChannel = null;
            List<User> lstUsers = new List<User>();
            List<BuyerInfo> newUserList = new List<BuyerInfo>();
            List<long> lstContact = new List<long>();
            //objPartnerServiceChannel = GepServiceManager.GetInstance.CreateChannel<IPartnerChannel>(CloudConfig.PartnerServiceURL, UserContext, ref _operationScope);
            //using (new OperationContextScope((objPartnerServiceChannel)))
            //{
            //    var objMhg =
            //    new MessageHeader<UserExecutionContext>(UserContext);
            //    var untypedmh = objMhg.GetUntypedHeader("GepCustomHeader", "Gep.Cumulus");
            //    OperationContext.Current.OutgoingMessageHeaders.Add(untypedmh);
            lstUsers = proxyPartnerService.GetUsersBasedOnActivityCode("10700023,10700003", "", UserContext.BuyerPartnerCode, 0, 1000);
            //}
            List<BuyerInfo> lstbuyerInfo = GetNewReqDao().GetAssignBuyersList(organizationEntityIds, documentCodes);
            if (lstUsers.Any())
            {
                lstContact = lstUsers.Select(x => x.ContactCode).ToList();
                newUserList = lstbuyerInfo.Where(u => lstContact.ToList<long>().Contains(Convert.ToInt64(u.ContactCode))).ToList<BuyerInfo>();
            }
            else
                newUserList = lstbuyerInfo;
            return newUserList;
        }
        public List<P2P.BusinessEntities.ItemCharge> ConvertReqObject(List<NewP2PEntities.ItemCharge> ItemChargesForHeader)
        {
            List<P2P.BusinessEntities.ItemCharge> lstItemCharge = new List<P2P.BusinessEntities.ItemCharge>();
            foreach (var item in ItemChargesForHeader)
            {
                P2P.BusinessEntities.ItemCharge obj = new P2P.BusinessEntities.ItemCharge();
                obj.ItemChargeId = item.ItemChargeId;

                obj.DocumentCode = item.DocumentCode;

                obj.P2PLineItemID = item.P2PLineItemID;

                obj.LineNumber = item.LineNumber;

                obj.ItemTypeID = item.ItemTypeID;

                obj.CalculationValue = item.CalculationValue;

                obj.ChargeAmount = item.ChargeAmount;

                obj.AdditionInfo = item.AdditionInfo;

                obj.CreatedBy = item.CreatedBy;

                obj.UpdatedBy = item.UpdatedBy;
                obj.ChargeDetails = new P2P1.ChargeMaster();
                if (item.ChargeDetails != null)
                {

                    obj.ChargeDetails.ChargeMasterId = item.ChargeDetails.ChargeMasterId;
                    obj.ChargeDetails.ChargeName = item.ChargeDetails.ChargeName;
                    obj.ChargeDetails.ChargeDescription = item.ChargeDetails.ChargeDescription;
                    obj.ChargeDetails.ChargeTypeCode = item.ChargeDetails.ChargeTypeCode;
                    obj.ChargeDetails.EDICode = item.ChargeDetails.EDICode;
                    obj.ChargeDetails.IsAllowance = item.ChargeDetails.IsAllowance;
                    obj.ChargeDetails.CalculationBasisId = item.ChargeDetails.CalculationBasisId;
                    obj.ChargeDetails.CalculationBasis = item.ChargeDetails.CalculationBasis;
                    obj.ChargeDetails.EntityDetailCode = item.ChargeDetails.EntityDetailCode;
                    obj.ChargeDetails.GLDetailId = item.ChargeDetails.GLDetailId;
                    obj.ChargeDetails.IsIncludeForTax = item.ChargeDetails.IsIncludeForTax;
                    obj.ChargeDetails.IsEditableOnInvoice = item.ChargeDetails.IsEditableOnInvoice;
                    obj.ChargeDetails.IsIncludeForRetainage = item.ChargeDetails.IsIncludeForRetainage;
                    obj.ChargeDetails.TolerancePercentage = item.ChargeDetails.TolerancePercentage;
                    obj.ChargeDetails.IsEditableOnOrder = item.ChargeDetails.IsEditableOnOrder;
                    obj.ChargeDetails.ChargeTypeName = item.ChargeDetails.ChargeTypeName;
                }


                obj.IsHeaderLevelCharge = item.IsHeaderLevelCharge;

                obj.TotalAmount = item.TotalAmount;

                obj.Tax = item.Tax;

                obj.AdditionalCharges = item.AdditionalCharges;

                obj.TotalAllowance = item.TotalAllowance;

                obj.TotalCharge = item.TotalCharge;

                obj.IsChecked = item.IsChecked;

                obj.ChargeItemCount = item.ChargeItemCount;

                obj.DocumentAdditionalCharges = item.DocumentAdditionalCharges;

                obj.DocumentTax = item.DocumentTax;

                obj.DocumentTotal = item.DocumentTotal;

                obj.MatchStatus = item.MatchStatus;

                obj.IsDeleted = item.IsDeleted;

                List<P2P.BusinessEntities.RequisitionSplitItems> lstSplitItems = new List<P2P.BusinessEntities.RequisitionSplitItems>();
                if (item.Reqsplits != null)
                {
                    RequisitionDocumentManager objDocBO = new RequisitionDocumentManager(this.JWTToken) { UserContext = this.UserContext, GepConfiguration = this.GepConfiguration };
                    var splitEntityReq = objDocBO.GetAllSplitAccountingFields(P2P1.P2PDocumentType.Requisition, P2P1.LevelType.ItemLevel);

                    foreach (var split in item.Reqsplits)
                    {
                        P2P.BusinessEntities.RequisitionSplitItems splitItem = new P2P.BusinessEntities.RequisitionSplitItems()
                        {
                            AdditionalCharges = split.additionalCharges,
                            CreatedBy = split.createdBy != null ? split.createdBy.id : 0,
                            DocumentItemId = split.documentItemId,
                            DocumentSplitItemId = split.id,
                            ErrorCode = split.errorCode,
                            IsDeleted = split.isDeleted,
                            Percentage = split.percentage,
                            Quantity = (Decimal)split.quantity,
                            ShippingCharges = split.shippingCharges,
                            Tax = split.tax,
                            SplitItemTotal = split.splitItemTotal,
                            SplitType = (P2P.BusinessEntities.SplitType)item.splitType,
                            DocumentSplitItemEntities = getSplitItemEntities(split, splitEntityReq)
                        };
                        lstSplitItems.Add(splitItem);
                    }
                }
                obj.ReqItemSplitsDetail = lstSplitItems;

                obj.SplitType = (P2P.BusinessEntities.SplitType)item.splitType;

                lstItemCharge.Add(obj);
            }
            return lstItemCharge;
        }
        public List<KeyValuePair<string, string>> ValidateReqWorkbenchItems(string reqItemIds, byte validationType)
        {
            RequisitionCommonManager commonManager = new RequisitionCommonManager(this.JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
            var objP2PSetting = commonManager.GetSettingsFromSettingsComponent(P2P1.P2PDocumentType.None, UserContext.ContactCode, (int)SubAppCodes.P2P);
            var objSettings = commonManager.GetSettingsFromSettingsComponent(P2P1.P2PDocumentType.Requisition, UserContext.ContactCode, (int)SubAppCodes.P2P);

            bool allowOneShipToLocation = objP2PSetting.lstSettings.FirstOrDefault(p => p.FieldName == "AllowOneShipToLocation").FieldValue.ToString().ToUpper().Equals("TRUE");
            bool showRemitToLocation = objSettings.lstSettings.FirstOrDefault(p => p.FieldName.ToUpper() == "SHOWREMITTOLOCATION") == null ? false : objSettings.lstSettings.FirstOrDefault(p => p.FieldName.ToUpper() == "SHOWREMITTOLOCATION").FieldValue.ToString().ToUpper().Equals("TRUE");
            bool allowDeliverToFreeText = objP2PSetting.lstSettings.FirstOrDefault(p => p.FieldName.ToUpper() == "ALLOWDELIVERTOFREETEXT").FieldValue.ToString().ToUpper().Equals("TRUE");
            return GetNewReqDao().ValidateReqWorkbenchItems(reqItemIds, validationType, allowOneShipToLocation, showRemitToLocation, allowDeliverToFreeText);
        }
        public void SaveReqAllItemCharges(List<P2P.BusinessEntities.ItemCharge> ItemChargesForHeader)
        {
            try
            {
                RequisitionCommonManager objCommon = new RequisitionCommonManager(this.JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
                ItemChargesForHeader = objCommon.SaveReqAllItemCharges(ItemChargesForHeader);

                if (!ReferenceEquals(ItemChargesForHeader, null) && ItemChargesForHeader.Count > 0)
                {
                    List<P2P.BusinessEntities.DocumentSplitItemEntity> DocumentSplitItemEntities = new List<P2P.BusinessEntities.DocumentSplitItemEntity>();
                    List<P2P.BusinessEntities.RequisitionSplitItems> DocumentSplitItems = new List<P2P.BusinessEntities.RequisitionSplitItems>();
                    int uiId = 0;

                    foreach (P2P.BusinessEntities.ItemCharge item in ItemChargesForHeader)
                    {
                        if (item.ReqItemSplitsDetail != null && item.ReqItemSplitsDetail.Count > 0 && item.ItemChargeId > 0)
                        {
                            foreach (P2P.BusinessEntities.RequisitionSplitItems itmSplt in item.ReqItemSplitsDetail)
                            {
                                uiId = uiId + 1;
                                itmSplt.UiId = uiId;
                                itmSplt.DocumentItemId = item.ItemChargeId;
                                DocumentSplitItems.Add(itmSplt);
                                foreach (P2P.BusinessEntities.DocumentSplitItemEntity itmSpltEnt in itmSplt.DocumentSplitItemEntities)
                                {
                                    itmSpltEnt.UiId = uiId;
                                    DocumentSplitItemEntities.Add(itmSpltEnt);
                                }
                            }
                        }
                    }
                    if (!ReferenceEquals(DocumentSplitItems, null) && DocumentSplitItems.Count > 0)
                    {
                        objCommon.SaveRequisitionChargeAccountingDetails(DocumentSplitItems, DocumentSplitItemEntities);
                    }

                }
            }
            finally
            {
            }
        }

        public RequisitionTemplateResponse ExcelTemplateHandlerFromService(Int64 documentCode, string documentNumber, RequisitionExcelTemplateHandler action, long fileID)
        {
            string encryptedFileId = (fileID > 0) ? EncryptionHelper.Encrypt(fileID, this.UserContext.ContactCode) : "";
            var reqTemplateFileResponse = this.ExcelTemplateHandler(documentCode, documentNumber, action, encryptedFileId);
            var requisitionTemplateResponse = new RequisitionTemplateResponse()
            {
                EncryptedDD = reqTemplateFileResponse.EncryptedDD,
                FileId = fileID,
                Message = reqTemplateFileResponse.Message,
                ProcessedFileId = fileID,
                Status = reqTemplateFileResponse.Status,
                StatusCode = reqTemplateFileResponse.StatusCode
            };
            return requisitionTemplateResponse;
        }
        public FileManagerEntities.ReqTemplateFileResponse ExcelTemplateHandler(Int64 documentCode, string documentNumber, RequisitionExcelTemplateHandler action, string fileID = "")
        {
            FileManagerEntities.ReqUploadFileLog requisitionUploadLog = null;
            FileManagerEntities.ReqTemplateFileResponse reqTemplateResponse = new FileManagerEntities.ReqTemplateFileResponse();
            try
            {
                requisitionUploadLog = SaveRequisitionUploadLog(new FileManagerEntities.ReqUploadFileLog() { RequestType = (int)action, RequisitionID = documentCode, Status = "InProgress", UploadedBy = UserContext.UserId, UploadedFileID = fileID }, reqTemplateResponse);
                switch (action)
                {
                    case RequisitionExcelTemplateHandler.DownloadTemplate:
                        RequisitionTemplateDownLoader objDownloader = new RequisitionTemplateDownLoader(UserContext, documentCode, documentNumber, this, reqTemplateResponse, this.JWTToken);
                        break;
                    case RequisitionExcelTemplateHandler.ExportTemplate:
                        RequisitionTemplateExporter objExporter = new RequisitionTemplateExporter(UserContext, documentCode, documentNumber, this, reqTemplateResponse, this.JWTToken);
                        break;
                    case RequisitionExcelTemplateHandler.UploadTemplate:
                        RequisitionTemplateUploader objUploader = new RequisitionTemplateUploader(UserContext, documentCode, documentNumber, this, fileID, reqTemplateResponse, requisitionUploadLog, this.JWTToken);
                        break;
                    case RequisitionExcelTemplateHandler.DownloadErrorLog:
                        var errorLog = GetRequisitionErrorLog(documentCode, (int)RequisitionExcelTemplateHandler.UploadTemplate);
                        reqTemplateResponse.FileId = (errorLog == null) ? "" : errorLog.ProcessedFileID.ToString();
                        break;
                    case RequisitionExcelTemplateHandler.DownloadMasterTemplate:
                        RequisitionMasterTemplateDownloader objMasterTemplate = new RequisitionMasterTemplateDownloader(UserContext, documentCode, documentNumber, this, reqTemplateResponse, this.JWTToken);
                        break;
                }
                if (reqTemplateResponse.Status != null && reqTemplateResponse.Status.Equals("Failed"))
                {
                    reqTemplateResponse.Status = "Failed";
                    requisitionUploadLog.Status = "Failed";
                }
                else
                {
                    reqTemplateResponse.Status = "Success";
                    requisitionUploadLog.Status = "Completed";
                }
                SaveRequisitionUploadLog(requisitionUploadLog, reqTemplateResponse);
            }
            catch (Exception ex)
            {
                reqTemplateResponse.Status = "Failed";
                if (requisitionUploadLog != null)
                {
                    requisitionUploadLog.Error = ex.Message;
                    requisitionUploadLog.Status = "Failed";
                    requisitionUploadLog.ErrorTrace = ex.StackTrace;
                    SaveRequisitionUploadLog(requisitionUploadLog, reqTemplateResponse);
                }
            }
            return reqTemplateResponse;
        }

        public FileManagerEntities.ReqUploadFileLog SaveRequisitionUploadLog(FileManagerEntities.ReqUploadFileLog requisitionUploadLog, FileManagerEntities.ReqTemplateFileResponse reqTemplateResponse)
        {
            requisitionUploadLog.ProcessedFileID = reqTemplateResponse.ProcessedFileId;
            requisitionUploadLog.UploadedFileID = reqTemplateResponse.FileId;

            var reqUploadLog = new RequisitionUploadLog()
            {
                RequisitionUploadLogID = requisitionUploadLog.RequisitionUploadLogID,
                RequestType = requisitionUploadLog.RequestType,
                RequisitionID = requisitionUploadLog.RequisitionID,
                Status = requisitionUploadLog.Status,
                UploadedBy = requisitionUploadLog.UploadedBy,
                ProcessedFileID = string.IsNullOrEmpty(requisitionUploadLog.ProcessedFileID) ? 0 : EncryptionHelper.Decrypt(requisitionUploadLog.ProcessedFileID, this.UserContext.ContactCode),
                UploadedFileID = string.IsNullOrEmpty(requisitionUploadLog.UploadedFileID) ? 0 : EncryptionHelper.Decrypt(requisitionUploadLog.UploadedFileID, this.UserContext.ContactCode),
                Error = requisitionUploadLog.Error,
                ErrorTrace = requisitionUploadLog.ErrorTrace,
                ProcessedXMLResult = requisitionUploadLog.ProcessedXMLResult
            };

            requisitionUploadLog.RequisitionUploadLogID = GetNewReqDao().SaveRequisitionUploadLog(reqUploadLog);
            return requisitionUploadLog;
        }
        public NewP2PEntities.Requisition GetRequisitionLineItemsByRequisitionId(long id)
        {
            return GetNewReqDao().GetRequisitionLineItemsByRequisitionId(id);
        }

        public List<SplitAccountingFields> GetAllHeaderAccountingFieldsForRequisition(long documentCode)
        {
            return GetNewReqDao().GetAllHeaderAccountingFieldsForRequisition(documentCode);
        }
        public void saveBudgetoryStatus(DataTable validationResult, long documentCode)
        {
            GetNewReqDao().saveBudgetoryStatus(validationResult, documentCode);
        }

        public void DeleteLineItems(long requisitionId, string commaSeperatedLineNos)
        {
            GetNewReqDao().DeleteLineItems(requisitionId, commaSeperatedLineNos);
        }

        public FileManagerEntities.ReqUploadFileLog GetRequisitionErrorLog(long requisitionId, int requestType)
        {
            var requisitionUploadLog = GetNewReqDao().GetRequisitionUploadError(requisitionId, requestType);
            var reqUploadFileLog = new FileManagerEntities.ReqUploadFileLog()
            {
                RequisitionUploadLogID = requisitionUploadLog.RequisitionUploadLogID,
                RequestType = requisitionUploadLog.RequestType,
                RequisitionID = requisitionUploadLog.RequisitionID,
                Status = requisitionUploadLog.Status,
                UploadedBy = requisitionUploadLog.UploadedBy,
                ProcessedFileID = (requisitionUploadLog.ProcessedFileID > 0) ? EncryptionHelper.Encrypt(requisitionUploadLog.ProcessedFileID, this.UserContext.ContactCode) : string.Empty,
                UploadedFileID = (requisitionUploadLog.UploadedFileID > 0) ? EncryptionHelper.Encrypt(requisitionUploadLog.UploadedFileID, this.UserContext.ContactCode) : string.Empty,
                Error = requisitionUploadLog.Error,
                ErrorTrace = requisitionUploadLog.ErrorTrace,
                ProcessedXMLResult = requisitionUploadLog.ProcessedXMLResult
            };
            return reqUploadFileLog;
        }

        public List<SplitAccountingFields> GetAllSplitAccountingCodesByDocumentType(LevelType levelType, long LobEntityDetailCode, int structureId)
        {
            return GetNewReqDao().GetAllSplitAccountingCodesByDocumentType(levelType, LobEntityDetailCode, structureId);
        }

        public List<SplitAccountingFields> GetValidSplitAccountingCodes(
                                                            LevelType levelType,
                                                            long LobEntityDetailCode,
                                                            int structureId,
                                                            List<KeyValuePair<int, string>> lstAccountingDataFromUpload,
                                                            long sourceSystemEntityId,
                                                            long sourceSystemEntityDetailCode)
        {
            return GetNewReqDao().GetValidSplitAccountingCodes(
                                                            levelType,
                                                            LobEntityDetailCode,
                                                            structureId,
                                                            lstAccountingDataFromUpload,
                                                            sourceSystemEntityId,
                                                            sourceSystemEntityDetailCode);
        }

        public bool SaveBulkRequisitionItems(long requisitionId, List<GEP.Cumulus.P2P.BusinessEntities.RequisitionItem> lstReqItems, int maxPrecessionValue, int maxPrecessionValueForTotal, int maxPrecessionValueForTaxAndCharges, int purchaseType = 0)
        {
            return GetNewReqDao().SaveBulkRequisitionItems(requisitionId, lstReqItems, maxPrecessionValue, maxPrecessionValueForTotal, maxPrecessionValueForTaxAndCharges, false, purchaseType);
        }
        public List<NewP2PEntities.RequisitionItem> ValidateItemsOnBuChange(List<NewP2PEntities.RequisitionItem> ReqItems, string buList, long LOBValue, string SourceType = "1")
        {
            return GetNewReqDao().ValidateItemsOnBuChange(ReqItems, buList, LOBValue, SourceType);

        }

        public Dictionary<string, string> AcceptOrRejectReview(long documentCode, bool isApproved, int documentTypeId, long LOBId = 0)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            result = AcceptOrRejectReviewValidations(documentCode);

            if (result.Count() == 0)
            {
                RequisitionDocumentManager documentManager = new RequisitionDocumentManager(this.JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
                var retValue = documentManager.AcceptOrRejectReview(documentCode, isApproved, documentTypeId, LOBId);
                result.Add("ReceiveNotificationResult", retValue);
            }

            return result;
        }

        public Dictionary<string, string> AcceptOrRejectReviewValidations(long documentCode)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            long LOBId = GetCommonDao().GetLOBByDocumentCode(documentCode);
            RequisitionDocumentManager p2pDocMgr = new RequisitionDocumentManager(this.JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
            var commonManager = new RequisitionCommonManager(this.JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
            bool isBypassOperationBudgetValidationInReviewPending = false;
            bool? IsOperationalbudgetPopupEnable = Convert.ToBoolean(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "IsOperationalbudgetPopupEnable", commonManager.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBId));
            bool? isOperationalBudgetEnabled = Convert.ToBoolean(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "IsOperationalBudgetEnabled", commonManager.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBId));
            DataTable flagForBudgetSplitAccountingValidation = new DataTable();

            flagForBudgetSplitAccountingValidation.TableName = "Validate Budget Accounting Details";

            if (this.UserContext.Product == GEPSuite.eInterface)
            {
                int populateDefaultNeedByDateByDays = Convert.ToInt16(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "NeedByDateByDays", commonManager.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBId));
                var lstValidationResult = p2pDocMgr.ValidateDocumentByDocumentCode(P2PDocumentType.Requisition, documentCode, LOBId, false, false, populateDefaultNeedByDateByDays);
                if (lstValidationResult.Count != 0)
                {
                    foreach (string element in lstValidationResult)
                    {
                        if (element.Contains("Invalid Needed Date"))
                        {
                            result.Add("InvalidNeededDate", "1");
                            break;
                        }
                    }
                    if (result.Count == 0)
                        result.Add("IsValidateDocumentByDocumentCode", "1");
                }
            }

            if (isBypassOperationBudgetValidationInReviewPending && isOperationalBudgetEnabled.HasValue && isOperationalBudgetEnabled.Value && result.Count() == 0)
            {
                flagForBudgetSplitAccountingValidation = GetOperationalBudgetManager().ValidateBudgetSplitAccounting(documentCode, DocumentType.Requisition, false, LOBId);
                if (IsOperationalbudgetPopupEnable.HasValue && IsOperationalbudgetPopupEnable.Value == true)
                {
                    if (flagForBudgetSplitAccountingValidation.Rows.Count > 0)
                    {
                        System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                        List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
                        Dictionary<string, object> row;
                        foreach (DataRow dr in flagForBudgetSplitAccountingValidation.Rows)
                        {
                            row = new Dictionary<string, object>();
                            foreach (DataColumn col in flagForBudgetSplitAccountingValidation.Columns)
                            {
                                row.Add(col.ColumnName, dr[col]);
                            }
                            rows.Add(row);
                        }
                        string jsonStringResult = serializer.Serialize(rows);
                        result.Add("IsValidateBudgetSplitAccounting", jsonStringResult);
                    }
                }
                else if (IsOperationalbudgetPopupEnable.HasValue && IsOperationalbudgetPopupEnable.Value == false)
                    saveBudgetoryStatus(flagForBudgetSplitAccountingValidation, documentCode);
            }

            return result;
        }

        private string GetDefaultCurrency()
        {
            var objCommon = new RequisitionCommonManager(this.JWTToken) { UserContext = this.UserContext, GepConfiguration = this.GepConfiguration };
            return objCommon.GetDefaultCurrency();
        }

        public DocumentIntegrationResults CreateRequisitionFromS2C(DocumentIntegrationEntity objDocumentIntegrationEntity)
        {
            GetUserExecutionContextDetails();
            DocumentIntegrationResults objDocumentIntegrationResults = new DocumentIntegrationResults();
            try
            {
                var DocumentIntegrationEntityjson = new JavaScriptSerializer().Serialize(objDocumentIntegrationEntity);
                LogHelper.LogError(Log, "Input JSON from contarct:" + DocumentIntegrationEntityjson, null);
                NewP2PEntities.Requisition req = GetRequisitionDisplayDetails(0);
                if (objDocumentIntegrationEntity != null && !(string.IsNullOrEmpty(objDocumentIntegrationEntity.DocumentCurrency)))
                {
                    req.currency = new CodeAndName
                    {
                        code = objDocumentIntegrationEntity.DocumentCurrency,
                        name = objDocumentIntegrationEntity.DocumentCurrency
                    };
                }
                else
                {
                    req.currency = new CodeAndName
                    {
                        code = UserContext.DefaultCurrencyCode,
                        name = UserContext.DefaultCurrencyCode
                    };
                }

                string businessUnitEntityCode = string.Empty;
                List<GEP.NewPlatformEntities.DocumentBU> documentBUList = new List<NewPlatformEntities.DocumentBU>();
                if (objDocumentIntegrationEntity != null && objDocumentIntegrationEntity.Document != null && objDocumentIntegrationEntity.Document.DocumentBUList != null && objDocumentIntegrationEntity.Document.DocumentBUList.Count > 0)
                {
                    foreach (var DocumentBUList in objDocumentIntegrationEntity.Document.DocumentBUList)
                    {
                        GEP.NewPlatformEntities.DocumentBU documentBU = new GEP.NewPlatformEntities.DocumentBU();
                        documentBU.buCode = DocumentBUList.BusinessUnitCode;
                        documentBU.buName = DocumentBUList.BusinessUnitName;
                        businessUnitEntityCode = DocumentBUList.BusinessUnitCode.ToString();
                        documentBUList.Add(documentBU);
                    }
                }
                req.documentBU = documentBUList;
                req.RequisitionSource = (byte)RequisitionSource.ContractRequisition;
                req.ContractNumber = objDocumentIntegrationEntity.Document.DocumentNumber;
                req.Contract = new CodeAndName()
                {
                    code = objDocumentIntegrationEntity.Document.DocumentNumber,
                    name = objDocumentIntegrationEntity.Document.DocumentName
                };
                req.purchaseType = (byte)GetContractPurchaseTypeId();

                ORGMaster_InputParams ORG_InputParams = new ORGMaster_InputParams();
                ORG_InputParams.ContactCode = this.UserContext.ContactCode;
                ORG_InputParams.DocumentCode = objDocumentIntegrationEntity.Document.DocumentCode;
                ORG_InputParams.CompanyName = this.UserContext.CompanyName;
                ORG_InputParams.SetDefault = true;
                ORG_InputParams.PreferenceLOBType = 1;
                ORG_InputParams.LOBEntityDetailCode = this.UserContext.BelongingEntityDetailCode.ToString();
                ORG_InputParams.LOBEntityCodes = this.UserContext.BelongingEntityDetailCode.ToString();
                ORG_InputParams.DisplayNameWithCodes = false;
                ORG_InputParams.selectedNodes = businessUnitEntityCode;
                ORG_InputParams.getComplete = false;
                Contact_ORG_Mapping lstContact_ORG_Mapping = GetContactOrgMappingDetails(ORG_InputParams);

                if (lstContact_ORG_Mapping.SelectedORGList.Count > 0 && req.HeaderSplitAccountingFields.Count > 0)
                {
                    foreach (var data in req.HeaderSplitAccountingFields)
                    {
                        var objHeaderEntityData = lstContact_ORG_Mapping.SelectedORGList[0].EntityDetails.FirstOrDefault(o => o.EntityId == data.EntityTypeId && o.EntityDetailCode != data.EntityDetailCode);
                        if (null != objHeaderEntityData)
                        {
                            data.EntityDetailCode = Convert.ToInt64(objHeaderEntityData.EntityDetailCode);
                            data.ParentEntityDetailCode = Convert.ToInt64(objHeaderEntityData.ParentEntityDetailCode);
                            data.EntityDisplayName = objHeaderEntityData.EntityDisplayName;
                        }
                    }
                }
                Collection<DocumentAdditionalEntityInfo> lstDocHeaderEntities = new Collection<DocumentAdditionalEntityInfo>();

                foreach (var item in req.HeaderSplitAccountingFields)
                {
                    lstDocHeaderEntities.Add(new DocumentAdditionalEntityInfo
                    {
                        EntityCode = item.EntityCode,
                        EntityDetailCode = item.EntityDetailCode,
                        EntityDisplayName = item.DisplayName,
                        EntityId = item.EntityTypeId,
                        ParentEntityDetailCode = item.ParentEntityDetailCode,
                        ParentEntityId = item.ParentSplitAccountingFieldId,
                        LOBEntityDetailCode = item.LOBEntityDetailCode
                    });
                }

                var commonManager = new RequisitionCommonManager(this.JWTToken) { UserContext = this.UserContext, GepConfiguration = this.GepConfiguration };
                var billtoLocation = commonManager.GetBillToLocBasedOnDefaultHeaderEntity(P2PDocumentType.Requisition, lstDocHeaderEntities);
                req.billTo = new IdNameAddressAndContact();
                if (billtoLocation != null)
                {
                    foreach (DataRow dataRow in billtoLocation.Rows)
                    {
                        req.billTo.id = Convert.ToInt64(dataRow["BilltoLocationID"]);
                        req.billTo.name = Convert.ToString(dataRow["BillToLocName"]);
                        req.billTo.address = Convert.ToString(dataRow["BillToAddress"]);
                        req.billTo.contact = Convert.ToString(dataRow["BillToContact"]);
                    }
                }

                var p2pSettings = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.None, UserContext.ContactCode, (int)SubAppCodes.P2P, "", UserContext.BelongingEntityDetailCode);
                long entityDetailCode = 0;
                var settingValue = commonManager.GetSettingsValueByKey(p2pSettings, "EntityMappedToShipToLocation");
                var EntityMappedToShipToLocation = string.IsNullOrEmpty(settingValue) ? 0 : Convert.ToInt32(settingValue);
                if (lstDocHeaderEntities != null)
                {
                    entityDetailCode = lstDocHeaderEntities.Where(x => x.EntityId == EntityMappedToShipToLocation).Select(x => x.EntityDetailCode).FirstOrDefault();
                }
                var shiptolocation = GetContactDefaultShipToLoc(entityDetailCode, this.UserContext.BelongingEntityDetailCode);
                if (shiptolocation != null)
                {
                    req.shipTo = new IdNameAndAddress();
                    req.shipTo.id = shiptolocation.ShiptoLocationId;
                    req.shipTo.name = shiptolocation.ShiptoLocationName;
                    req.shipTo.address = shiptolocation.Address.AddressLine1 + shiptolocation.Address.City + shiptolocation.Address.Country;
                }

                SaveResult result = SaveCompleteRequisition(req);
                Collection<DocumentLinkInfo> lstDocumentLinkInfo = new Collection<DocumentLinkInfo>();
                LogHelper.LogError(Log, " Save CompleteRequisition sucessfull  " + result.id, null);
                string url = String.Empty;
                if (result.success)
                {
                    DocumentLinkInfo objdocumentLinkInfo = new DocumentLinkInfo();
                    objdocumentLinkInfo.DocumentCode = objDocumentIntegrationEntity.Document.DocumentCode;
                    objdocumentLinkInfo.DocumentTypeCode = DocumentType.Contract;
                    objdocumentLinkInfo.LinkedDocumentCode = result.id;
                    objdocumentLinkInfo.LinkedDocumentType = DocumentType.Requisition;
                    objdocumentLinkInfo.DocumentRelationID = 3;
                    objdocumentLinkInfo.CreatedBy = this.UserContext.ContactCode;
                    objdocumentLinkInfo.ModifiedBy = this.UserContext.ContactCode;
                    objdocumentLinkInfo.CreatedOn = DateTime.Now;
                    objdocumentLinkInfo.UpdatedOn = DateTime.Now;
                    objdocumentLinkInfo.IsDeleted = false;
                    lstDocumentLinkInfo.Add(objdocumentLinkInfo);
                    GetReqDao().SaveDocumentLinkInfo(lstDocumentLinkInfo);
                    string encryptedDD = String.Empty;
                    encryptedDD = UrlEncryptionHelper.EncryptURL("dc=" + result.id + "&bpc=" + UserContext.BuyerPartnerCode);
                    url = "P2P/Index?dd=" + encryptedDD + "&isFromS2C=1&oloc=263#/req5";
                    objDocumentIntegrationResults.ReturnUrl = url;
                    objDocumentIntegrationResults.IsSuccess = true;
                }
                else
                {
                    objDocumentIntegrationResults.IsSuccess = false;
                    objDocumentIntegrationResults.ReturnUrl = "";
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in CreateRequisitionFromS2C Method", ex);
                objDocumentIntegrationResults.IsSuccess = false;
                objDocumentIntegrationResults.ReturnUrl = "";
                throw;
            }
            return objDocumentIntegrationResults;
        }

        private int GetContractPurchaseTypeId()
        {
            try
            {
                var commonManager = new RequisitionCommonManager(this.JWTToken) { UserContext = this.UserContext, GepConfiguration = this.GepConfiguration };
                var purchasetypes = commonManager.GetPurchaseTypeItemExtendedTypeMapping();
                int defaultPurchaseTypeId = 1;
                foreach (var purchaseType in purchasetypes)
                {
                    if (purchaseType.IsHeaderContractVisible)
                        return purchaseType.PurchaseTypeId;
                    else if (purchaseType.IsDefault)
                        defaultPurchaseTypeId = purchaseType.PurchaseTypeId;
                }
                return defaultPurchaseTypeId;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in GetContractPurchaseTypeId Method", ex);
                throw ex;
            }
        }

        private ShiptoLocation GetContactDefaultShipToLoc(long entityDetailCode, long LOBEntityDetailCode)
        {
            try
            {
                ShiptoLocation objShiptoLocation = new ShiptoLocation();
                ProxyPartnerService proxyPartnerService = new ProxyPartnerService(UserContext, this.JWTToken);
                var contactPreferences = proxyPartnerService.GetContactPreferenceByContactCode(UserContext.BuyerPartnerCode, UserContext.ContactCode);
                if (contactPreferences != null && contactPreferences.contact != null && contactPreferences.contact.UserInfo != null && contactPreferences.contact.UserInfo.ShipToLocationId > 0)
                {
                    var commonManager = new RequisitionCommonManager(this.JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
                    objShiptoLocation = commonManager.GetShipToLocationById(contactPreferences.contact.UserInfo.ShipToLocationId, entityDetailCode, LOBEntityDetailCode);
                }
                return objShiptoLocation;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in GetContactDefaultShipToLoc Method", ex);
                throw ex;
            }
        }

        private Contact_ORG_Mapping GetContactOrgMappingDetails(ORGMaster_InputParams ORG_InputParams)
        {
            try
            {
                ProxyPartnerService proxyPartnerService = new ProxyPartnerService(UserContext, this.JWTToken);
                var lstContact_ORG_Mapping = proxyPartnerService.GetContactOrgMappingDetails(ORG_InputParams);
                return lstContact_ORG_Mapping;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in GetContactOrgMappingDetails Method", ex);
                throw ex;
            }
        }

        private void GetUserExecutionContextDetails()
        {
            try
            {
                var userdetails = GetUserContext(this.UserContext.ContactCode);
                var belongingLOBDts = userdetails.GetDefaultBelongingUserLOBMapping();
                var servingLOBDtls = userdetails.GetDefaultServingUserLOBMapping();
                this.UserContext.BelongingEntityDetailCode = belongingLOBDts != null ? belongingLOBDts.EntityDetailCode : 0;
                this.UserContext.ServingEntityDetailCode = servingLOBDtls != null ? servingLOBDtls.EntityDetailCode : 0;
                this.UserContext.BelongingEntityId = belongingLOBDts != null ? belongingLOBDts.EntityId : 0;
                this.UserContext.ServingEntityId = servingLOBDtls != null ? servingLOBDtls.EntityId : 0;
                this.UserContext.ShipToLocationId = userdetails.ShipToLocationId;
                this.UserContext.UserId = userdetails.UserId;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in GetUserExecutionContextDetails Method", ex);
                throw ex;
            }
        }

        public List<NewP2PEntities.Requisition> GetRequisitionDetailsList(DocumentSearch documentSearch)
        {
            try
            {
                List<NewP2PEntities.Requisition> lstRequisition = GetNewReqDao().GetRequisitionDetailsList(documentSearch);
                foreach (NewP2PEntities.Requisition req in lstRequisition)
                {
                    req.EncryptedDD = UrlEncryptionHelper.EncryptURL("dc=" + req.documentCode + "&bpc=" + UserContext.BuyerPartnerCode);
                }
                return lstRequisition;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in GetRequisitionDetailsList method in NewRequisitionDAO. Document Status :" + string.Join(",", documentSearch.documentStatus)
                    + " Search Column = " + documentSearch.SearchColumn.ToString() + " LOBEntityDetailCode = " + documentSearch.LOBEntityDetailCode.ToString() +
                    " Currency Code = " + documentSearch.currencyCode.ToString() + " Search Text = " + documentSearch.searchText.ToString(), ex);
                var objCustomFault = new CustomFault(ex.Message, "GetRequisitionDetailsList", "GetRequisitionDetailsList",
                                                      "Requisition Rest Service", ExceptionType.ApplicationException, string.Empty, false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while Get Requisition Details List :" + documentSearch.supplierCode.ToString(CultureInfo.InvariantCulture));
            }
        }

        public DataSet ValidateRequisitionData(long documentCode)
        {
            try
            {
                bool validateOrderinglocationAndSupplierContact = false;
                long LOBId = GetCommonDao().GetLOBByDocumentCode(documentCode);
                RequisitionCommonManager commonManager = new RequisitionCommonManager(this.JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
                var objP2PSetting = commonManager.GetSettingsFromSettingsComponent(P2P1.P2PDocumentType.None, UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBId);
                var objReqSetting = commonManager.GetSettingsFromSettingsComponent(P2P1.P2PDocumentType.Requisition, UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBId);
                var checkEntityActiveStatus = commonManager.GetSettingsValueByKey(objP2PSetting, "CheckEntityActiveStatus");
                var validateEntityParentChildRelationship = commonManager.GetSettingsValueByKey(objP2PSetting, "ValidateEntityParentChildRelationship");
                var CheckSupplierContactAndOrderLocationValidity = commonManager.GetSettingsValueByKey(objP2PSetting, "CheckSupplierContactAndOrderLocationValidity");
                var CheckInactiveRequester = commonManager.GetSettingsValueByKey(objReqSetting, "CheckInactiveRequester");
                bool CheckInactiveRequesters = !string.IsNullOrEmpty(CheckInactiveRequester) ? Convert.ToBoolean(CheckInactiveRequester) : false;
                if (!string.IsNullOrEmpty(CheckSupplierContactAndOrderLocationValidity))
                {
                    if (CheckSupplierContactAndOrderLocationValidity.ToUpper().ToString() == "YES")
                        validateOrderinglocationAndSupplierContact = true;
                    else if (CheckSupplierContactAndOrderLocationValidity.ToUpper().ToString() == "NO")
                        validateOrderinglocationAndSupplierContact = false;
                }
                bool ValidateEntityActiveStatus = !string.IsNullOrEmpty(checkEntityActiveStatus) ? Convert.ToBoolean(checkEntityActiveStatus) : false;
                bool validateEntityParentChildRelationshipStatus = !string.IsNullOrEmpty(validateEntityParentChildRelationship) ? Convert.ToBoolean(validateEntityParentChildRelationship) : false;
                DataTable dataTable = new DataTable();
                dataTable.Columns.Add("SettingName", typeof(string));
                dataTable.Columns.Add("SettingValue", typeof(int));

                ConvertSettingValueToDatatable(dataTable, "checkentityactivestatus", ValidateEntityActiveStatus);
                ConvertSettingValueToDatatable(dataTable, "ValidateEntityParentChildRelationship", validateEntityParentChildRelationshipStatus);
                ConvertSettingValueToDatatable(dataTable, "validateOrderinglocationAndSupplierContact", validateOrderinglocationAndSupplierContact);
                ConvertSettingValueToDatatable(dataTable, "CheckInactiveRequester", CheckInactiveRequesters);

                DataSet objInactiveEntities = GetNewReqDao().ValidateRequisitionData(documentCode, dataTable);
                return objInactiveEntities;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in ValidateRequisitionData", ex);
                throw ex;
            }
        }
        public bool IsRequiredRiskCalculation(long RequisitionId, long CustomAttrFormIdForRiskAssessment)
        {
            bool isRequiredRiskCalculation = false;
            try
            {
                int riskWeight = 0;
                List<Tuple<long, long>> lstFormCodeId = new List<Tuple<long, long>>();

                lstFormCodeId.Add(new Tuple<long, long>(RequisitionId, CustomAttrFormIdForRiskAssessment));
                var response = GetCommonDao().GetQuestionResponse(lstFormCodeId);
                var lstresponse = GetReqDao().GetRiskFormQuestionScore();
                var section1 = lstresponse.lstRiskFormQuestionScore.Where(x => x.Section == 1).ToList();
                if (section1.Count > 0)
                {
                    foreach (var obj in section1)
                    {
                        var queObj = response.Where(x => x.QuestionId == obj.QuestionId && x.ResponseValue == obj.Response).FirstOrDefault();
                        if (queObj != null)
                            riskWeight = riskWeight + obj.ResponseWeight;
                    }
                    if (riskWeight != 5)
                        isRequiredRiskCalculation = true;
                }

            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in IsRequiredRiskCalculation", ex);
                throw ex;
            }
            return isRequiredRiskCalculation;

        }
        public bool IsRiskFormMandatory(long RequisitionId, long CustomAttrFormIdForRiskAssessment, List<IdAndName> CustomAttrQuestionSetCodesForRiskAssessment)
        {
            bool isRiskFormMandatory = false;
            try
            {

                int riskWeight = 0;
                List<Tuple<long, long>> lstFormCodeId = new List<Tuple<long, long>>();

                ProxySurveyComponent proxySurveyComponent = new ProxySurveyComponent(this.UserContext, this.JWTToken);
                List<Question> question = proxySurveyComponent.GetMandatoryQuestionWithNoResponse(String.Join(",", CustomAttrQuestionSetCodesForRiskAssessment.Select(o => o.id)), "", -1, -1, RequisitionId, AssessorUserType.Buyer);
                if (question.Count > 0)
                    return true;

                lstFormCodeId.Add(new Tuple<long, long>(RequisitionId, CustomAttrFormIdForRiskAssessment));
                var response = GetCommonDao().GetQuestionResponse(lstFormCodeId);
                var lstresponse = GetReqDao().GetRiskFormQuestionScore();
                var section1 = lstresponse.lstRiskFormQuestionScore.Where(x => x.Section == 1).ToList();
                if (section1.Count > 0)
                {
                    foreach (var obj in section1)
                    {
                        var queObj = response.Where(x => x.QuestionId == obj.QuestionId && x.ResponseValue == obj.Response).FirstOrDefault();
                        if (queObj != null)
                            riskWeight = riskWeight + obj.ResponseWeight;
                    }
                }
                if (riskWeight != 5)
                {
                    var section2 = lstresponse.lstRiskFormQuestionScore.Where(x => x.Section == 2).ToList();
                    if (section1.Count > 0)
                    {
                        var lstQue = section2.GroupBy(x => x.QuestionId).Select(x => x.FirstOrDefault());
                        foreach (var obj in lstQue)
                        {
                            var queObj = response.Where(x => x.QuestionId == obj.QuestionId && x.ResponseValue != "").FirstOrDefault();
                            if (queObj == null)
                            {
                                isRiskFormMandatory = true;
                                return isRiskFormMandatory;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in IsRiskFormMandatory", ex);
                throw ex;
            }
            return isRiskFormMandatory;

        }

        public DataTable GetAllPASCategories(string searchText, int pageSize = 10, int pageNumber = 1, long partnerCode = 0, long contactCode = 0)
        {
            try
            {
                RequisitionCommonManager commonMamager = new RequisitionCommonManager(this.JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
                var p2pSettings = commonMamager.GetSettingsFromSettingsComponent(P2PDocumentType.None, UserContext.ContactCode, (int)SubAppCodes.P2P, "");
                int categorySelectionLevel = commonMamager.GetSettingsValueByKey(p2pSettings, "CategorySelectionLevel") == string.Empty ? 1 : Convert.ToInt16(commonMamager.GetSettingsValueByKey(p2pSettings, "CategorySelectionLevel"));

                DataTable objInactiveEntities = GetNewReqDao().GetAllPASCategories(searchText, pageSize, pageNumber, partnerCode, contactCode, categorySelectionLevel);
                return objInactiveEntities;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in GetAllPASCategories", ex);
                throw ex;
            }
        }

        public NewP2PEntities.Requisition GetRequisitionDetailsFromCatalog(Int64 documentCode)
        {
            try
            {
                NewP2PEntities.Requisition objReqData = GetNewReqDao().GetRequisitionDetailsFromCatalog(documentCode);
                return objReqData;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in GetRequisitionDetailsFromCatalog in reqmanager", ex);
                throw ex;
            }
        }

        public bool AddRequisitionsIntoSearchIndexerQueue(List<long> reqIds)
        {
            return GetNewReqDao().AddRequisitionsIntoSearchIndexerQueue(reqIds);
        }

        public DataSet GetPartnersDocumetInterfaceInfo(List<long> partnerCodes)
        {
            try
            {
                DataSet objPartnersDocumetInterfaceInfo = GetNewReqDao().GetPartnersDocumetInterfaceInfo(partnerCodes);
                return objPartnersDocumetInterfaceInfo;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in GetPartnersDocumetInterfaceInfo", ex);
                throw ex;
            }
        }

        public List<UserDetails> GetUsersBasedOnUserDetailsWithPagination(UserDetails usersInfo, string searchText, int pageIndex, int pageSize, bool includeCurrentUser, string activityCodes, bool honorDirectRequesterForOBOSelection, bool isAutosuggest, bool isCheckCreateReqActivityForOBO)
        {
            try
            {
                object[] requestParams = {
                    usersInfo,
                    searchText,
                    pageIndex,
                    pageSize,
                    includeCurrentUser,
                    activityCodes,
                    honorDirectRequesterForOBOSelection,
                    isAutosuggest,
                    isCheckCreateReqActivityForOBO
                };

                var relicHelper = new NewRelicHelper();
                relicHelper.logNewRelicEvents("GetOBOUserDetails",
                    "Request", JsonConvert.SerializeObject(requestParams), "NewRequisitionManager");

                return GetNewReqDao().GetUsersBasedOnUserDetailsWithPagination(usersInfo, searchText, pageIndex, pageSize, includeCurrentUser, activityCodes, honorDirectRequesterForOBOSelection, isAutosuggest, isCheckCreateReqActivityForOBO);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in GetUsersBasedOnUserDetailsWithPagination in NewRequisitionManager", ex);
                throw ex;
            }
        }

        public Documents.Entities.DocumentLOBDetails GetDocumentLOBByDocumentCode(long documentCode)
        {
            try
            {
                DocumentLOBDetails documentLOB = GetNewReqDao().GetDocumentLOBByDocumentCode(documentCode);
                return documentLOB;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in GetDocumentLOBByDocumentCode", ex);
                throw ex;
            }
        }

        public void UpdateRequisitionPreviousAmount(long requisitionId, bool updateReqPrevAmount)
        {
            try
            {
                GetNewReqDao().UpdateRequisitionPreviousAmount(requisitionId, updateReqPrevAmount);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in UpdateRequisitionPreviousAmount", ex);
                throw ex;
            }
        }

        public void ResetRequisitionItemFlipType(long requisitionId)
        {
            try
            {
                GetNewReqDao().ResetRequisitionItemFlipType(requisitionId);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in ResetRequisitionItemFlipType", ex);
                throw ex;
            }
        }

        /////REQ-5620 Team Member functionality for getting all users
        public List<UserDetails> GetAllUsersByActivityCode(string SearchText, string Shouldhaveactivitycodes, string Shouldnothaveactivitycodes, long Partnercode)
        {
            try
            {
                return GetNewReqDao().GetAllUsersByActivityCode(SearchText, Shouldhaveactivitycodes, Shouldnothaveactivitycodes, Partnercode);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in GetAllUsersByActivityCode", ex);
                throw ex;
            }
        }

        public List<PartnerLocation> GetOrderingLocationsWithNonOrgEntities(long partnerCode, int locationType, long accessControlEntityDetailCode, List<long> OrgBUIds = null, List<long> NonOrgBUIds = null, int pageIndex = 0, int pageSize = 10, string searchText = "")
        {
            try
            {
                // orderingLocationTypeId = 2, shipFromLocation = 5;
                var accessControlOrgEntityDetailCodesList = new List<long>();
                if (OrgBUIds != null && OrgBUIds.Count > 0)
                {
                    accessControlOrgEntityDetailCodesList.AddRange(OrgBUIds);
                }
                else if (accessControlEntityDetailCode != 0)
                {
                    accessControlOrgEntityDetailCodesList.Add(accessControlEntityDetailCode);
                }
                return GetNewReqDao().GetOrderingLocationsWithNonOrgEntities(partnerCode, locationType, accessControlOrgEntityDetailCodesList, NonOrgBUIds, "", pageIndex, pageSize, searchText);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in GetOrderingLocationsWithNonOrgEntities", ex);
                throw ex;
            }
        }


        public void SaveTeamMember(GEP.Cumulus.Documents.Entities.Document objDocument, List<GEP.Cumulus.Documents.Entities.DocumentStakeHolder> newTeamMembers)
        {
            try
            {


                bool isSuccessful = GetNewReqDao().SaveTeamMember(objDocument);

                if (newTeamMembers.Count > 0 && isSuccessful)
                {

                    RequisitionEmailNotificationManager emailNotificationManager = new RequisitionEmailNotificationManager(UserContext = UserContext, GepConfiguration = GepConfiguration, this.JWTToken);

                    emailNotificationManager.SendNotificationToTeamMembers(objDocument, newTeamMembers);


                }

            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error Occured in SaveTeamMember method", ex);

                throw;
            }
        }

        public List<OrderLinkMap> GetOrdersLinksForReqLineItem(long p2pLineItemId, long requisitionId)
        {
            List<OrderLinkMap> orderLinkList = new List<OrderLinkMap>();
            try
            {
                DataTable orderLinkDataTable = GetNewReqDao().GetOrdersLinksForReqLineItem(p2pLineItemId, requisitionId);
                if (!(orderLinkDataTable is null))
                {
                    foreach (DataRow row in orderLinkDataTable.Rows)
                    {
                        orderLinkList.Add(new OrderLinkMap()
                        {
                            DocumentNumber = row["DocumentNumber"] == null ? "" : row["DocumentNumber"].ToString(),
                            EncryptedUrlParam = UrlEncryptionHelper.EncryptURL("bpc=" + UserContext.BuyerPartnerCode + "&dc=" + (row["DocumentCode"] == null ? 0 : Convert.ToInt64(row["DocumentCode"]))),
                            EncryptedUrlParamBPC = UrlEncryptionHelper.EncryptURL(UserContext.BuyerPartnerCode.ToString()),
                            DocumentName = row["DocumentName"] == null ? "" : row["DocumentName"].ToString(),
                            DocumentStatus = row["DocumentStatus"] == null ? 0 : Convert.ToInt32(row["DocumentStatus"])
                        });
                    }
                }

                return orderLinkList;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error Occured in GetOrdersLinksForReqLineItem method", ex);
                throw;
            }
        }

        public List<KeyValuePair<long, string>> GetRequesterAndAuthorFilter(int FilterFor, int pageSize, string term = "")
        {
            try
            {
                return GetNewReqDao().GetRequesterAndAuthorFilter(FilterFor, pageSize, term);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error Occured in GetRequesterAndAuthorFilter method", ex);
                throw;
            }
        }

        private void saveNewrelicErrors(string eventname, string key, string values)
        {
            var eventObject = new Dictionary<string, object>();
            try
            {

                eventObject.Add(key, values);
                NewRelic.Api.Agent.NewRelic.RecordCustomEvent(eventname, eventObject);
            }
            catch (Exception)
            {
                Dictionary<string, object> msg = new Dictionary<string, object>();
                NewRelic.Api.Agent.NewRelic.NoticeError("Error Occured on New Relic Writing ", msg);
            }
        }
        public List<KeyValuePair<long, string>> GetCategoryHirarchyByCategories(List<long> categories)
        {
            List<KeyValuePair<long, string>> lstCategoryHirarchy = new List<KeyValuePair<long, string>>();
            try
            {
                DataTable dtCategoryHirarchy = GetNewReqDao().GetCategoryHirarchyByCategories(categories);
                if (!(dtCategoryHirarchy is null))
                {
                    foreach (DataRow row in dtCategoryHirarchy.Rows)
                    {
                        lstCategoryHirarchy.Add(new KeyValuePair<long, string>(
                            row["PASCode"] == null ? 0 : Convert.ToInt64(row["PASCode"].ToString()),
                            row["CategoryHierarchy"] == null ? "" : row["CategoryHierarchy"].ToString()
                        ));
                    }
                }
                return lstCategoryHirarchy;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error Occured in GetCategoryHirarchyByCategories method", ex);
                throw;
            }
        }

        private void ConvertSettingValueToDatatable(DataTable dataTable, string settingName, bool settingValue)
        {
            DataRow dr = dataTable.NewRow();
            dr["SettingName"] = settingName;
            dr["SettingValue"] = settingValue;
            dataTable.Rows.Add(dr);

        }

        public List<RequisitionPartnerInfo> GetAllBuyerSuppliersAutoSuggest(long BuyerPartnerCode, string Status, string SearchText, int PageIndex, int PageSize, string OrgEntityCodes, long LOBEntityDetailCode, string RestrictedSupplierRelationTypes, long PASCode, long ContactCode)
        {
            try
            {
                return GetNewReqDao().GetAllBuyerSuppliersAutoSuggest(BuyerPartnerCode, Status, SearchText, PageIndex, PageSize, OrgEntityCodes, LOBEntityDetailCode, RestrictedSupplierRelationTypes, PASCode, ContactCode);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occurred in GetAllBuyerSuppliersAutoSuggest method", ex);
                throw;
            }
        }
        public List<PurchaseType> GetPurchaseTypes()
        {
            return GetNewReqDao().GetPurchaseTypes();
        }
        public List<ServiceType> GetPurchaseTypeItemExtendedTypeMapping()
        {
            return GetNewReqDao().GetPurchaseTypeItemExtendedTypeMapping();
        }


        #region Interface Method(s) --> Moved to RequisitionInterfaceManager.cs

        //public bool UpdateLineStatusForRequisitionFromInterface(List<RequisitionLineStatusUpdateDetails> reqDetails)
        //{
        //    var objMessageHeader = new MessageHeader<UserExecutionContext>(this.UserContext);
        //    System.ServiceModel.Channels.MessageHeader messageHeader = objMessageHeader.GetUntypedHeader("GepCustomHeader", "Gep.Cumulus");

        //    long RequisitionId = 0;
        //    try
        //    {
        //        if (reqDetails != null && reqDetails.Any())
        //        {
        //            foreach (var req in reqDetails)
        //            {
        //                RequisitionId = req.RequisitionId;

        //                if (GetNewReqDao().UpdateLineStatusForRequisitionFromInterface(RequisitionId, req.AllLineStatus, req.IsUpdateAllItems, req.Items, req.StockReservationNumber))
        //                {
        //                    RequisitionEmailNotificationManager emailNotificationManager = new RequisitionEmailNotificationManager(UserContext = UserContext, GepConfiguration = GepConfiguration);
        //                    emailNotificationManager.SendNotificationForLineStatusUpdate(req);
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.LogError(Log, "Error occurred in UpdateLineStatusForRequisitionFromInterface." + RequisitionId, ex);
        //        throw;
        //    }
        //    return true;
        //}

        #endregion
        public void UpdateRiskScore(long DocumentCode)
        {
            try
            {
                GetNewReqDao().UpdateRiskScore(DocumentCode);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error Occured in UpdateRiskScore method", ex);
                throw;
            }
        }



        public bool UpdateRequisitionItemStatusWorkBench(string IsCreatePOorRfx, List<long> items, List<long> reqIds, long rfxId)
        {
            try
            {
                DataTable table = new DataTable();
                table.Columns.Add("Id", typeof(long));
                for (int i = 0; i < items.Count; i++)
                    table.Rows.Add(new object[] { items[i] });

                DataTable tableReqIds = new DataTable();
                tableReqIds.Columns.Add("Id", typeof(long));
                for (int i = 0; i < reqIds.Count; i++)
                    tableReqIds.Rows.Add(new object[] { reqIds[i] });

                return GetNewReqDao().UpdateRequisitionItemStatusWorkBench(IsCreatePOorRfx, table, tableReqIds, rfxId);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error Occured in SaveRequisitionItemRFxMapping method", ex);
                throw;
            }
        }

        public long GetRequisitionItemRFxLink(long reqItemId)
        {
            try
            {
                return GetNewReqDao().GetRequisitionItemRFxLink(reqItemId);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error Occured in GetRequisitionItemRFxLink method", ex);
                throw;
            }
        }
        /// <summary>
        /// Get documents for changing requestor/reindex of requisition.
        /// </summary>
        public List<DocumentDelegation> GetDocumentsForUtility(long contactCode, string searchText)
        {
            try
            {
                List<DocumentDelegation> lstDocument = new List<DocumentDelegation>();

                lstDocument = GetNewReqDao().GetDocumentsForUtility(contactCode, searchText);

                return lstDocument;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in NewRequisitionManager GetDocumentsForUtility Method ", ex);
                var objCustomFault = new CustomFault(ex.Message, "GetDocumentsForUtility", "GetDocumentsForUtility",
                                                     "NewRequisitionManager", ExceptionType.ApplicationException, null, false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while GetDocumentsForUtility");
            }
        }

        /// <summary>
        /// Saves document for changing the requester of requisition.
        /// </summary>

        public bool SaveDocumentRequesterChange(List<long> documentRequesterChangeList, long contactCode)
        {
            try
            {
                bool result = false;
                if (documentRequesterChangeList != null && documentRequesterChangeList.Any())
                {
                    result = GetNewReqDao().SaveDocumentRequesterChange(documentRequesterChangeList, contactCode);
                }
                return result;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in NewRequisitionManager SaveDocumentRequesterChange Method", ex);
                var objCustomFault = new CustomFault(ex.Message, "SaveDocumentRequesterChange", "SaveDocumentRequesterChange",
                                                     "NewRequisitionManager", ExceptionType.ApplicationException, null, false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while SaveDocumentRequesterChange");
            }
        }


        /// <summary>
        /// Reindex  requisition documents
        /// </summary>
        public bool PerformReIndexForDocuments(List<long> documentReIndexList)
        {
            try
            {
                bool result = false;
                if (documentReIndexList != null && documentReIndexList.Any())
                {
                    result = GetNewReqDao().PerformReIndexForDocuments(documentReIndexList);
                }
                return result;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in NewRequisitionManager PerformReIndexForDocuments Method", ex);
                var objCustomFault = new CustomFault(ex.Message, "PerformReIndexForDocuments", "PerformReIndexForDocuments",
                                                     "NewRequisitionManager", ExceptionType.ApplicationException, null, false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while PerformReIndexForDocuments");
            }
        }

        public AccessDeniedCatalogItems UserAccessCheckForItems(CatalogItemsInfo CatalogItemsInfo)
        {
            try
            {
                AccessDeniedCatalogItems accessDeniedCatalogItems = new AccessDeniedCatalogItems();
                RequisitionCommonManager commonMamager = new RequisitionCommonManager(this.JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
                SettingDetails reqSettingDetails = commonMamager.GetSettingsFromSettingsComponent(P2PDocumentType.Requisition, UserContext.ContactCode, (int)SubAppCodes.P2P);
                bool enableWebAPIForGetLineItems = Convert.ToBoolean(commonMamager.GetSettingsValueByKey(reqSettingDetails, "EnableWebAPIForGetLineItems"));
                List<long> lstItemMasterItems = new List<long>();
                List<long> lstCatalogItems = new List<long>();
                lstItemMasterItems = UserAccessCheckForItemMasteritems(CatalogItemsInfo, enableWebAPIForGetLineItems);
                lstCatalogItems = UserAccessCheckForCatalogItems(CatalogItemsInfo, enableWebAPIForGetLineItems);
                accessDeniedCatalogItems.CatalogItems = lstCatalogItems;
                accessDeniedCatalogItems.ItemMasterItems = lstItemMasterItems;
                return accessDeniedCatalogItems;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error Occured in UserAccessCheckForItems method", ex);
                throw ex;
            }

        }
        //Below method will be deleted after catalogitem provided api for bulkitemmasteritems
        public List<long> UserAccessCheckForItemMasteritems(CatalogItemsInfo CatalogItemsInfo, bool enableWebAPIForGetLineItems)
        {
            List<long> lstItemMasterItems = new List<long>();
            ProxyCatalogService proxyCatalogService = new ProxyCatalogService(UserContext, this.JWTToken);
            CatalogItemSearch.SearchResult searchResult = null;
            try
            {
                if (CatalogItemsInfo.ItemMasterItems.Count > 0)
                {
                    foreach (ItemMasterItemInfo ItemMasterItemInfo in CatalogItemsInfo.ItemMasterItems)
                    {
                        long ItemMasteritem = ItemMasterItemInfo.ItemMasterItem;
                        if (enableWebAPIForGetLineItems)
                        {
                            CatalogItemSearch.ItemFilterInputRequest itemFilterInputRequest = new CatalogItemSearch.ItemFilterInputRequest()
                            {
                                BIN = ItemMasterItemInfo.BuyerItemNumber,
                                ContactCode = CatalogItemsInfo.ContactCode,
                                AccessEntities = CatalogItemsInfo.AccessEntities,
                                Size = 10
                            };

                            string GetLineItemsURl = MultiRegionConfig.GetConfig(CloudConfig.AppURL) + "api/GetLineItemsFilter?oloc=559";
                            Req.BusinessObjects.RESTAPIHelper.RequestHeaders requestHeaders = new Req.BusinessObjects.RESTAPIHelper.RequestHeaders();
                            string appName = Environment.GetEnvironmentVariable("NewRelic.AppName") ?? "Requisition";
                            string useCase = "Req-OBOChange";
                            requestHeaders.Set(this.UserContext, JWTToken);
                            var webAPI = new GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.WebAPI(requestHeaders, appName, useCase);
                            var JsonResult = webAPI.ExecutePost(GetLineItemsURl, itemFilterInputRequest);
                            searchResult = JsonConvert.DeserializeObject<CatalogItemSearch.SearchResult>(JsonResult);
                        }
                        else
                        {
                            CatalogItemSearch.ItemSearchInput itemSearchInput = new CatalogItemSearch.ItemSearchInput()
                            {
                                BIN = ItemMasterItemInfo.BuyerItemNumber,
                                ContactCode = CatalogItemsInfo.ContactCode,
                                AccessEntities = CatalogItemsInfo.AccessEntities,
                                Size = 10
                            };
                            searchResult = proxyCatalogService.GetLineItemsSearch(itemSearchInput);
                        }
                        if (searchResult != null && searchResult.Items != null && searchResult.Items.Count == 0)
                        {
                            lstItemMasterItems.Add(ItemMasteritem);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error Occured in UserAccessCheckForItemMasteritems method", ex);
                throw ex;
            }
            return lstItemMasterItems;
        }

        public List<long> UserAccessCheckForCatalogItems(CatalogItemsInfo CatalogItemsInfo, bool enableWebAPIForGetLineItems)
        {
            List<long> lstCatalogItems = new List<long>();
            try
            {

                if (CatalogItemsInfo.CatalogItems.Count > 0)
                {
                    List<CatalogItemSearch.CatalogItemAdditionalInfo> CatalogItemAdditionalInfoList = new List<CatalogItemSearch.CatalogItemAdditionalInfo>();
                    CatalogItemSearch.ItemSearchInput catalogItemSearchInput = new CatalogItemSearch.ItemSearchInput();
                    CatalogItemSearch.ItemBulkInputRequest itemBulkInputRequest = new CatalogItemSearch.ItemBulkInputRequest();
                    CatalogItemSearch.SearchResult searchResult = new CatalogItemSearch.SearchResult();
                    foreach (long CatalogItem in CatalogItemsInfo.CatalogItems)
                    {
                        CatalogItemSearch.CatalogItemAdditionalInfo catalogItemAdditionalInfo = new CatalogItemSearch.CatalogItemAdditionalInfo();
                        catalogItemAdditionalInfo.CatalogItemId = CatalogItem;
                        catalogItemAdditionalInfo.AccessEntities = CatalogItemsInfo.AccessEntities;
                        CatalogItemAdditionalInfoList.Add(catalogItemAdditionalInfo);
                    }
                    if (enableWebAPIForGetLineItems)
                    {
                        itemBulkInputRequest.ContactCode = CatalogItemsInfo.ContactCode;
                        itemBulkInputRequest.Size = CatalogItemsInfo.CatalogItems.Count;
                        itemBulkInputRequest.CatalogItemAdditionalInfoList = CatalogItemAdditionalInfoList;

                        string GetLineItemsURl = MultiRegionConfig.GetConfig(CloudConfig.AppURL) + "api/GetLineItemsBulk?oloc=559";
                        Req.BusinessObjects.RESTAPIHelper.RequestHeaders requestHeaders = new Req.BusinessObjects.RESTAPIHelper.RequestHeaders();
                        string appName = Req.BusinessObjects.URLs.AppNameForGetLineItems;
                        string useCase = Req.BusinessObjects.URLs.UseCaseForOBOChange;
                        requestHeaders.Set(this.UserContext, this.JWTToken);
                        var webAPI = new GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.WebAPI(requestHeaders, appName, useCase);
                        var JsonResult = webAPI.ExecutePost(GetLineItemsURl, itemBulkInputRequest);
                        searchResult = JsonConvert.DeserializeObject<CatalogItemSearch.SearchResult>(JsonResult);
                    }
                    else
                    {
                        catalogItemSearchInput.ContactCode = CatalogItemsInfo.ContactCode;
                        catalogItemSearchInput.Size = CatalogItemsInfo.CatalogItems.Count;
                        catalogItemSearchInput.CatalogItemAdditionalInfoList = CatalogItemAdditionalInfoList;
                        ProxyCatalogService proxyCatalogService = new ProxyCatalogService(UserContext, this.JWTToken);
                        searchResult = proxyCatalogService.GetLineItemsSearch(catalogItemSearchInput);
                    }
                    foreach (long CatalogItem in CatalogItemsInfo.CatalogItems)
                    {
                        var resultData = searchResult.Items.Where(x => x.Id.Equals(CatalogItem)).ToList();
                        if (resultData.Count == 0)
                        {
                            lstCatalogItems.Add(CatalogItem);
                        }
                    }


                }

            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error Occured in UserAccessCheckForCatalogItems method", ex);
                throw ex;
            }
            return lstCatalogItems;
        }
        public IdNameAndAddress GetOrderingLoctionNameByLocationId(long locationId)
        {
            try
            {
                return GetNewReqDao().GetOrderingLoctionNameByLocationId(locationId);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error Occured in GetOrderingLoctionNameByLocationId method", ex);
                throw;
            }
        }
        public void SaveQuestionsResponse(List<QuestionResponse> lstQuestionsResponse, long docId)
        {
            try
            {
                GetNewReqDao().SaveQuestionsResponse(lstQuestionsResponse, docId);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error Occured in SaveQuestionsResponse method for Change Procurement profile scenario", ex);
                throw;
            }
        }

        public List<BlanketDetails> GetBlanketDetailsForReqLineItem(long requisitionItemId)
        {
            List<BlanketDetails> blanketList = new List<BlanketDetails>();
            try
            {
                blanketList = GetNewReqDao().GetBlanketDetailsForReqLineItem(requisitionItemId);
                return blanketList;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error Occured in GetBlanketDetailsForReqLineItem method", ex);
                throw;
            }
        }

        public BlanketDetails MapBlanketDetailsFromContractAPI(string blanketAPIResult, BlanketDetails blanketDetails)
        {
            try
            {
                var blanketDataFromAPI = JObject.Parse(blanketAPIResult);
                var dataSearchResult = blanketDataFromAPI.GetValue("DataSearchResult").SelectToken("Value").ToArray();
                foreach (var value in dataSearchResult)
                {
                    if (Convert.ToString(value["DocumentSearchOutput"]["DocumentNumber"])
                        .Equals(blanketDetails.BlanketDocumentNumber, StringComparison.InvariantCultureIgnoreCase))
                    {
                        blanketDetails.BlanketName = Convert.ToString(value["DocumentSearchOutput"]["DocumentName"]);
                        var expiryProperty = value["DocumentSearchOutput"]["DocumentAdditionalFieldList"].ToList()
                        .Where(x => x["FieldName"].ToString() == "ExpiryOn").FirstOrDefault();
                        if (expiryProperty != null && !string.IsNullOrEmpty(Convert.ToString(expiryProperty["FieldValue"])))
                            blanketDetails.ExpiryDate = Convert.ToDateTime(expiryProperty["FieldValue"]);
                    }
                }
                return blanketDetails;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error Occured in MapBlanketDetailsFromContractAPI method", ex);
                throw;
            }
        }

        public List<PartnerLocation> GetAllSupplierLocationsByOrgEntity(string OrgEntityCode, int PartnerLocationType = 2)
        {
            return GetNewReqDao().GetAllSupplierLocationsByOrgEntity(OrgEntityCode, PartnerLocationType);
        }

        public long GetOrgEntityManagers(long orgEntityCode)
        {
            return GetNewReqDao().GetOrgEntityManagers(orgEntityCode);
        }

        public long SaveDocumentDetails(GEP.Cumulus.Documents.Entities.Document document)
        {
            try
            {
                string EntityType = string.Empty;
                if (document.DocumentNumber == string.Empty)
                {
                    if (document.DocumentTypeInfo == DocumentType.RequestTemplate)
                        EntityType = "RT";
                    else if (document.DocumentTypeInfo == DocumentType.RequestForm)
                        EntityType = "RF";

                    document.DocumentNumber = GetEntityNumberForRequisition(EntityType);
                }
                return GetNewReqDao().SaveDocumentDetails(document);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error Occured in SaveDocumentDetails method ", ex);
                throw;
            }

        }

        public void GetNewDocumentNumber(out string documentNumber, out string documentName, string docTypeName, GEP.Cumulus.P2P.BusinessEntities.P2PDocumentType docType, long LOBEntityDetailCode = 0, long EntityDetailcode = 0)
        {
            if (LOBEntityDetailCode == 0)
            {
                LOBEntityDetailCode = GetDocumentLOB(docType);
            }
            var pdm = new P2PDocumentManager { UserContext = UserContext, GepConfiguration = GepConfiguration, jwtToken = this.JWTToken };
            documentNumber = GetEntityNumberForRequisition(docTypeName, LOBEntityDetailCode, EntityDetailcode);
            documentName = pdm.GenerateDefaultName(docType, UserContext.ContactCode, 0, LOBEntityDetailCode, documentNumber);
        }

        public long GetDocumentLOB(GEP.Cumulus.P2P.BusinessEntities.P2PDocumentType docType)
        {
            long documentLOB = 0;

            var partnerHelper = new RESTAPIHelper.PartnerHelper(this.UserContext, JWTToken);
            UserContext userContext = partnerHelper.GetUserContextDetailsByContactCode(UserContext.ContactCode);

            if (docType == P2PDocumentType.Requisition || docType == P2PDocumentType.PaymentRequest)
            {
                if (UserContext.BelongingEntityDetailCode <= 0)
                    documentLOB = userContext.GetDefaultBelongingUserLOBMapping().EntityDetailCode;
                else
                    documentLOB = UserContext.BelongingEntityDetailCode;
            }
            else
            {
                if (UserContext.ServingEntityDetailCode <= 0)
                    documentLOB = userContext.GetDefaultServingUserLOBMapping().EntityDetailCode;
                else
                    documentLOB = UserContext.ServingEntityDetailCode;
            }
            return documentLOB;
        }


        public Boolean UpdateMultipleRequisitionItemStatuses(string requisitionIds, P2P.BusinessEntities.StockReservationStatus statusTobeUpdated)
        {
            bool result = false;
            List<long> requisitionIdList = requisitionIds.Split(',').Select(long.Parse).ToList();
            RequisitionDocumentManager p2pDocumentManager = new RequisitionDocumentManager(this.JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
            requisitionIdList.ForEach(RequisitionId => p2pDocumentManager.UpdateRequisitionDocumentStatus(RequisitionId, DocumentStatus.Ordered));
            result = GetNewReqDao().updateStatusOfMultipleRequisitionItems(requisitionIds, DocumentStatus.Ordered);
            logNewRelicApp(UserContext.BuyerPartnerCode, requisitionIds, result);
            if (this.UserContext.Product != GEPSuite.eInterface)
                AddIntoSearchIndexerQueueing(requisitionIdList, (int)DocumentType.Requisition);
            return result;
        }

        private void logNewRelicApp(long buyerPartnerCode, string requisitionIds, bool result)
        {
            var eventAttributes = new Dictionary<string, object>();
            eventAttributes.Add("requisitionIds", requisitionIds);
            eventAttributes.Add("buyerPartnerCode", buyerPartnerCode);
            eventAttributes.Add("result", result.ToString());
            NewRelic.Api.Agent.NewRelic.RecordCustomEvent("UpdateMultipleRequisitionItemStatuses", eventAttributes);
        }
        public Boolean UpdateRequisitionItemTaxJurisdiction(List<KeyValuePair<long, string>> lstItemTaxJurisdictions)
        {
            bool result = false;
            result = GetNewReqDao().UpdateRequisitionItemTaxJurisdiction(lstItemTaxJurisdictions);
            return result;
        }

        public List<User> GetUsersBasedOnEntityDetailsCode(string orgEntityCodes, long PartnerCode, int PageIndex, long ContactCode, int PageSize, string SearchText = "", string ActivityCodes = "")
        {
            try
            {
                return GetNewReqDao().GetUsersBasedOnEntityDetailsCode(orgEntityCodes, PartnerCode, PageIndex, ContactCode, PageSize, SearchText, ActivityCodes);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in GetUsersBasedOnEntityDetailsCode in NewRequisitionManager", ex);
                throw ex;
            }
        }
        public DataSet GetPartnerSourceSystemDetailsByReqId(long requisitionId)
        {
            try
            {
                return GetNewReqDao().GetPartnerSourceSystemDetailsByReqId(requisitionId);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in GetPartnerSourceSystemDetailsByReqId in NewRequisitionManager", ex);
                throw ex;
            }
        }

        public bool SendRequisitionForExternalValidation(long documentCode, int extendedStatusId)
        {
            try
            {
                bool result = false;
                result = GetNewReqDao().PushRequisitionToInterface(documentCode);
                var result1 = result && GetReqDao().UpdateRequisitionExtendedStatus(documentCode, String.Empty, extendedStatusId);
                AddIntoSearchIndexerQueueing(documentCode, (int)DocumentType.Requisition);
                return result1;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in SendRequisitionForExternalValidation in NewRequisitionManager", ex);
                throw ex;
            }

        }

        public List<BudgetDetails> GetBudgetDetails(long documentCode)
        {
            try
            {
                List<BudgetAllocationDetails> budgetAllocationDetails = new List<BudgetAllocationDetails>();
                budgetAllocationDetails = GetNewReqDao().GetBudgetDetails(documentCode);
                var AlloctionIds = budgetAllocationDetails.Select(a => a.BudgetEntityAllocationId).Distinct().ToList();
                var budgetDetails = GetBudgetDetailsByAllocationIds(AlloctionIds);
                List<BudgetDetails> BudgetDetails = BudgetDetailResponseMapping(budgetDetails);
                return BudgetDetails;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in SendRequisitionForExternalValidation in NewRequisitionManager", ex);
                throw ex;
            }
        }
        public List<BudgetDetails> BudgetDetailResponseMapping(dynamic BudgetAllocationDetails)
        {
            List<BudgetDetails> budgetDetails = new List<BudgetDetails>();
            foreach (var items in BudgetAllocationDetails)
            {
                BudgetDetails budget = new BudgetDetails();
                budget.AvailableFunds = items["AvailableFunds"];
                budget.BudgetDescription = items["BudgetDescription"];
                budget.BudgetEntityAllocationId = items["BudgetEntityAllocationId"];
                budget.BudgetName = items["BudgetName"];
                budget.CommittedFunds = items["CommittedFunds"];
                budget.ExpensedFunds = items["ExpensedFunds"];
                budget.ObligatedFunds = items["ObligatedFunds"];
                budget.PeriodEndDate = Convert.ToDateTime(items["PeriodEndDate"]);
                budget.PeriodName = items["PeriodName"];
                budget.PeriodStartDate = Convert.ToDateTime(items["PeriodStartDate"]);
                budget.TotalBudget = items["TotalBudget"];
                budgetDetails.Add(budget);
            }
            return budgetDetails;
        }
        private dynamic GetBudgetDetailsByAllocationIds(List<long> BudgetEntityAllocationId)
        {

            string getCheckBudgetAPIURL = MultiRegionConfig.GetConfig(CloudConfig.APIMBaseURL) + URLs.GetBudgetDetailsByAllocationIds;

            var requestHeaders = new RESTAPIHelper.RequestHeaders();
            requestHeaders.Set(UserContext, JWTToken);
            var webAPI = new RESTAPIHelper.WebAPI(requestHeaders, "DEV_APAC_Dss_Req_Microservice_NEWREQ", "CapitalBudgetManager-CheckBudget");
            var JsonResult = webAPI.ExecutePost(getCheckBudgetAPIURL, BudgetEntityAllocationId);

            var jsonSerializer = new JavaScriptSerializer();
            var BudgetCheckResponse = jsonSerializer.Deserialize<object>(JsonResult);
            return BudgetCheckResponse;
        }
        public List<P2P1.TaxJurisdiction> GetTaxJurisdcitionsByShiptoLocationIds(List<long> shipToLocationids)
        {
            try
            {
                List<P2P1.TaxJurisdiction> taxJurisdictionDetails = new List<P2P1.TaxJurisdiction>();
                taxJurisdictionDetails = GetNewReqDao().GetTaxJurisdcitionsByShiptoLocationIds(shipToLocationids);
                return taxJurisdictionDetails;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in GetTaxJurisdcitionsByShiptoLocationIds in NewRequisitionManager", ex);
                throw ex;
            }
        }

        public List<PASMasterData> GetAllLevelCategories(long lOBEntityDetailCode)
        {
            try
            {
                List<PASMasterData> lstAllLevelCategories = new List<PASMasterData>();

                lstAllLevelCategories = GetNewReqDao().GetAllLevelCategories(lOBEntityDetailCode);

                return lstAllLevelCategories;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error Occured in GetAllLevelCategories in NewRequisitionManager", ex);
                throw ex;
            }
        }

        public List<long> GetP2PLineitemIdbasedOnPartnerandCurrencyCode(long requisitionId, long partnerCode, long locationId, string currencyCode)
        {
            List<long> P2PLineitemIds = new List<long>();
            try
            {
                long LOBId = GetCommonDao().GetLOBByDocumentCode(requisitionId);
                RequisitionCommonManager commonManager = new RequisitionCommonManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
                string AllowMultiCurrencyInRequisitionSetting = (commonManager.GetSettingsValueByKey(P2PDocumentType.Requisition, "AllowMultiCurrencyInRequisition", commonManager.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBId));
                bool AllowMultiCurrencyInRequisition = !string.IsNullOrEmpty(AllowMultiCurrencyInRequisitionSetting) ? Convert.ToBoolean(AllowMultiCurrencyInRequisitionSetting) : false;
                if (AllowMultiCurrencyInRequisition)
                    P2PLineitemIds = GetNewReqDao().GetP2PLineitemIdbasedOnPartnerandCurrencyCode(requisitionId, partnerCode, locationId, currencyCode);

            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error Occured in GetP2PLineitemIdbasedOnPartnerandCurrencyCode in NewRequisitionManager", ex);
                throw ex;
            }
            return P2PLineitemIds;
        }

        public void PerformIndexingForRequisition(long documentCode)
        {
            AddIntoSearchIndexerQueueing(documentCode, (int)DocumentType.Requisition);
        }

        public bool UpdateExtendedStatusforHoldRequisition(long requisitionId, int extendedStatus, long onHeldBy)
        {
            return GetReqDao().UpdateExtendedStatusforHoldRequisition(requisitionId, extendedStatus, onHeldBy);

        }

        public long GetLobByEntityDetailCode(long entitydetailcode)
        {
            return GetNewReqDao().GetLobByEntityDetailCode(entitydetailcode);
        }

    }


}
