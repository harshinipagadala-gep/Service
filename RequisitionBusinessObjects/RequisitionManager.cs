using ESIntegratorEntities.Entities;
using Gep.Cumulus.CSM.Entities;
using Gep.Cumulus.CSM.Extensions;
using Gep.Cumulus.Partner.Entities;
using GEP.Cumulus.Documents.Entities;
using GEP.Cumulus.Logging;
using GEP.Cumulus.OrganizationStructure.Entities;
using GEP.Cumulus.P2P.BusinessEntities;
using GEP.Cumulus.P2P.BusinessObjects.PartnerService;
using GEP.Cumulus.P2P.BusinessObjects.Proxy;
using GEP.Cumulus.P2P.Common;
using GEP.Cumulus.P2P.DataAccessObjects.SQLServer;
using GEP.Cumulus.P2P.Req.BusinessObjects;
using GEP.Cumulus.P2P.Req.BusinessObjects.Entities;
using GEP.Cumulus.P2P.Req.BusinessObjects.Proxy;
using GEP.Cumulus.P2P.Req.DataAccessObjects;
using GEP.Cumulus.SmartCatalog.BusinessEntities.NewCatalog;
using GEP.Cumulus.Web.Utils.Helpers;
using GEP.SMART.Configuration;
using log4net;
using Newtonsoft.Json;
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
using System.Web.Script.Serialization;
using QuestionBankEntities = GEP.Cumulus.QuestionBank.Entities;


namespace GEP.Cumulus.P2P.BusinessObjects
{
    public class RequisitionManager : RequisitionBaseBO
    {
        private RequisitionCommonManager reqCommonManager;
        public RequisitionManager(string jwtToken, UserExecutionContext context = null) : base(jwtToken)
        {
            if (context != null)
            {
                this.UserContext = context;                
            }
            reqCommonManager = new RequisitionCommonManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
        }
        private static readonly ILog Log = Logger.GetLog(MethodBase.GetCurrentMethod().DeclaringType);
        private HttpWebRequest req = null;
               
        public long UpdateProcurementStatusByReqItemId(long requisitionItemId)
        {
            return GetReqDao().UpdateProcurementStatusByReqItemId(requisitionItemId);
        }

        public bool SaveRequisitionBusinessUnit(long documentCode, long buId)
        {
            bool returnValue = GetReqDao().SaveRequisitionBusinessUnit(documentCode, buId);
            AddIntoSearchIndexerQueueing(documentCode, (int)DocumentType.Requisition);
            return returnValue;
        }

        public bool SaveDocumentBusinessUnit(long documentCode)
        {
            bool returnValue = GetReqDao().SaveDocumentBusinessUnit(documentCode);
            AddIntoSearchIndexerQueueing(documentCode, (int)DocumentType.Requisition);
            return returnValue;
        }

        public bool AddTemplateItemInReq(long documentCode, string templateIds, List<KeyValuePair<long, decimal>> items, int itemType, string buIds)
        {
            int defaultShipToLocationId = 0;
            long defaultPasCode = 0;
            ProxyPartnerService proxyPartnerService = new ProxyPartnerService(UserContext, this.JWTToken);
            
            try
            {
                if (Log.IsWarnEnabled)
                    Log.Warn(string.Concat("In AddTemplateItemInReq getting default ship to locatiopn " +
                                           "for documentCode=" + documentCode, ", ContactCode=" + UserContext.ContactCode + " was called."));

                var contactPreferences =
                       proxyPartnerService.GetContactPreferenceByContactCode(UserContext.BuyerPartnerCode, UserContext.ContactCode);
                defaultShipToLocationId = contactPreferences.contact.UserInfo.ShipToLocationId;

                if (Log.IsWarnEnabled)
                    Log.Warn(string.Concat("In AddTemplateItemInReq getting default pascode " +
                                           "for documentCode=" + documentCode, ", ContactCode=" + UserContext.ContactCode + " was called."));

                IList<PartnerService.PASMaster> pasList = proxyPartnerService.GetContactPASDefault(UserContext.ContactCode);
                if (pasList != null && pasList.Any())
                    defaultPasCode = pasList.FirstOrDefault().PASCode;
            }
            catch (CommunicationException ex)
            {
                LogHelper.LogError(Log, "Error occured in AddTemplateItemInReq Method in RequisitionManager", ex);
            }
            finally
            {
                //DisposeService(objPartnerServiceChannel);
            }
            var commonManager = new RequisitionCommonManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
            RequisitionDocumentManager  objP2PDocumentManager = new RequisitionDocumentManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };

            // shipping method based on LOb and ACE of Document
            var objRequisition = GetReqDao().OrderGetDocumentAdditionalEntityDetailsById(documentCode);
            long LOBId = objRequisition.EntityDetailCode.FirstOrDefault();

            int inventorySource = Convert.ToInt16(commonManager.GetSettingsValueByKey(P2PDocumentType.Template, "InventorySource", UserContext.ContactCode, 111));

            var noneSettingDetails = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.None, UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBId);

            int precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges;
            precessionValue = convertStringToInt(commonManager.GetSettingsValueByKey(noneSettingDetails, "MaxPrecessionValue"));
            maxPrecessionforTotal = convertStringToInt(commonManager.GetSettingsValueByKey(noneSettingDetails, "MaxPrecessionValueforTotal"));
            maxPrecessionForTaxesAndCharges = convertStringToInt(commonManager.GetSettingsValueByKey(noneSettingDetails, "MaxPrecessionValueForTaxesAndCharges"));
            string populateDefaultNeedByDate = (commonManager.GetSettingsValueByKey(noneSettingDetails, "PopulateDefaultNeedByDate"));
            int populateDefaultNeedByDateByDays = convertStringToInt(commonManager.GetSettingsValueByKey(noneSettingDetails, "NeedByDateByDays"));
            var entityIdMappedToShippingMethods = Convert.ToInt32(commonManager.GetSettingsValueByKey(noneSettingDetails, "EntityMappedToShippingMethods"));

            string shippingMethod = GetCommonDao().GetDefaultShippingMethod((new Requisition()).GetACEEntityDetailCode(objRequisition.DocumentAdditionalEntitiesInfoList, entityIdMappedToShippingMethods), LOBId);


            string strReqItemIds = GetReqDao().AddTemplateItemInReq(documentCode, templateIds, items, defaultPasCode, defaultShipToLocationId, itemType, inventorySource, buIds, shippingMethod, precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges, populateDefaultNeedByDate, populateDefaultNeedByDateByDays);
            if (!string.IsNullOrEmpty(strReqItemIds))
            {
                #region Saving default accounting
                var documentSplitItemEntities = objP2PDocumentManager.GetDocumentDefaultAccountingDetails(P2PDocumentType.Requisition, LevelType.ItemLevel, 0, documentCode);
                GetReqDao().SaveDefaultAccountingDetails(documentCode, documentSplitItemEntities, precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges, true);

                var catalogSettingDetails = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.Catalog, UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBId);

                var allowOrgEntityFromCatalog = Convert.ToBoolean(commonManager.GetSettingsValueByKey(catalogSettingDetails, "AllowOrgEntityInCatalogItems"), CultureInfo.InvariantCulture).ToString().ToUpper();
                var corporationEntityId = Convert.ToInt32(commonManager.GetSettingsValueByKey(catalogSettingDetails, "CorporationEntityId"), CultureInfo.InvariantCulture);
                var expenseCodeEntityId = Convert.ToInt32(commonManager.GetSettingsValueByKey(catalogSettingDetails, "ExpenseCodeEntityId"), CultureInfo.InvariantCulture);

                if (allowOrgEntityFromCatalog == "TRUE")
                {
                    GetReqDao().UpdateCatalogOrgEntitiesByItemId(strReqItemIds, corporationEntityId, expenseCodeEntityId, documentCode);
                }
                GetReqDao().UpdateTaxOnHeaderShipTo(documentCode, 0, precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges);

                AddIntoSearchIndexerQueueing(documentCode, (int)DocumentType.Requisition);

                #endregion
                return true;
            }
            return false;
        }


        public long SaveCatalogRequisition(long userId, long documentCode, string requisitionName, string requisitionNumber, long oboId = 0, bool callFromCatalog = false)
        {
            int defaultShipToLocationId = 0;
            UserExecutionContext userExecutionContext = UserContext;
            UserContext userContext = null;
            UserContext oboUserContext = null;
            try
            {
                if (oboId <= 0 || documentCode <= 0)
                {
                    var partnerHelper = new Req.BusinessObjects.RESTAPIHelper.PartnerHelper(UserContext, JWTToken);
                    userContext = partnerHelper.GetUserContextDetailsByContactCode(userId);
                }

                if (oboId > 0)
                {
                    var partnerHelper = new Req.BusinessObjects.RESTAPIHelper.PartnerHelper(UserContext, JWTToken);
                    oboUserContext = partnerHelper.GetUserContextDetailsByContactCode(oboId);
                }

                if (!ReferenceEquals(userContext, null))
                    defaultShipToLocationId = userContext.ShipToLocationId;

                if (oboId > 0 && !ReferenceEquals(oboUserContext, null))
                    defaultShipToLocationId = oboUserContext.ShipToLocationId;
            }
            catch (CommunicationException ex)
            {
                LogHelper.LogError(Log, "Error occured in SaveCatalogRequisition Method in RequisitionManager", ex);
            }

            long LOBEntityDetailCode = userContext.GetDefaultBelongingUserLOBMapping().EntityDetailCode;

            RequisitionDocumentManager  objP2PDocumentManager = new RequisitionDocumentManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
            var commonManager = new RequisitionCommonManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };

            if (documentCode <= 0)
            {
                requisitionNumber = GetEntityNumberForRequisition("REQ", LOBEntityDetailCode);
                requisitionName = objP2PDocumentManager.GenerateDefaultName(P2PDocumentType.Requisition, userId, 0, LOBEntityDetailCode, requisitionNumber);
            }
            var objRequisition = GetReqDao().GetDocumentAdditionalEntityDetailsById(documentCode);

            if (documentCode > 0)
                LOBEntityDetailCode = objRequisition.EntityDetailCode.FirstOrDefault();

            var noneSettingDetails = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.None, UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBEntityDetailCode);

            int precessionValue = convertStringToInt(commonManager.GetSettingsValueByKey(noneSettingDetails, "MaxPrecessionValue"));
            int maxPrecessionforTotal = convertStringToInt(commonManager.GetSettingsValueByKey(noneSettingDetails, "MaxPrecessionValueforTotal"));
            int maxPrecessionForTaxesAndCharges = convertStringToInt(commonManager.GetSettingsValueByKey(noneSettingDetails, "MaxPrecessionValueForTaxesAndCharges"));

            var entityIdMappedToShippingMethods = Convert.ToInt32(commonManager.GetSettingsValueByKey(noneSettingDetails, "EntityMappedToShippingMethods"));

            // Get default shipping methods based on ACE and LOB
            string shippingMethod = GetCommonDao().GetDefaultShippingMethod((new Requisition()).GetACEEntityDetailCode(objRequisition.DocumentAdditionalEntitiesInfoList, entityIdMappedToShippingMethods), objRequisition.EntityDetailCode.FirstOrDefault());

            Requisition reqObj = new Requisition()
            {
                DocumentCode = documentCode,
                DocumentName = requisitionName,
                DocumentNumber = requisitionNumber,
                EntityId = documentCode <= 0 ? userContext.GetDefaultBelongingUserLOBMapping().EntityId : 0,
                EntityDetailCode = new List<long>() { LOBEntityDetailCode }
            };
            if (documentCode <= 0)
            {
                UserLOBMapping defaultLOB = new UserLOBMapping();
                PreferenceLOBType preferenceLOBType = PreferenceLOBType.Belong;
                defaultLOB = userContext.GetDefaultBelongingUserLOBMapping();
                Collection<DocumentAdditionalEntityInfo> lstAdditionalEntities = new Collection<DocumentAdditionalEntityInfo>();
                List<SplitAccountingFields> splitAccountingFields = null;
                splitAccountingFields = objP2PDocumentManager.GetAllAccountingFieldsWithDefaultValues(P2PDocumentType.Requisition, LevelType.HeaderLevel,
                            UserContext.ContactCode, documentCode, null, null, false, 0, defaultLOB.EntityDetailCode, preferenceLOBType);

                foreach (var item in splitAccountingFields)
                {
                    lstAdditionalEntities.Add(new BusinessEntities.DocumentAdditionalEntityInfo
                    {
                        EntityCode = item.EntityCode,
                        EntityDetailCode = item.EntityDetailCode,
                        EntityDisplayName = item.DisplayName,
                        EntityId = item.EntityTypeId,
                        ParentEntityDetailCode = item.ParentEntityDetailCode
                    });
                }
                reqObj.DocumentAdditionalEntitiesInfoList = lstAdditionalEntities;
                P2PAccessControlManager objP2PAccessControlManager = new P2PAccessControlManager { UserContext = UserContext, GepConfiguration = GepConfiguration, jwtToken = this.JWTToken };
                objP2PAccessControlManager.GetBUDetailList(P2PDocumentType.Requisition, UserContext, reqObj);
            }
            List<KeyValuePair<string, string>> lstSettingValue = new List<KeyValuePair<string, string>>();
            lstSettingValue.Add(new KeyValuePair<string, string>("MaxPrecessionValue", precessionValue.ToString()));
            lstSettingValue.Add(new KeyValuePair<string, string>("MaxPrecessionValueforTotal", maxPrecessionforTotal.ToString()));
            lstSettingValue.Add(new KeyValuePair<string, string>("MaxPrecessionValueForTaxesAndCharges", maxPrecessionForTaxesAndCharges.ToString()));
            lstSettingValue.Add(new KeyValuePair<string, string>("PopulateDefaultNeedByDate", commonManager.GetSettingsValueByKey(noneSettingDetails, "PopulateDefaultNeedByDate")));
            lstSettingValue.Add(new KeyValuePair<string, string>("PopulateDefaultNeedByDateByDays", commonManager.GetSettingsValueByKey(noneSettingDetails, "NeedByDateByDays")));
            lstSettingValue.Add(new KeyValuePair<string, string>("IsOrderingLocationVisible", commonManager.GetSettingsValueByKey(P2PDocumentType.Requisition, "IsOrderingLocationVisible", UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBEntityDetailCode)));


            var documentId = GetReqDao().SaveCatalogRequisition(userId, reqObj, defaultShipToLocationId, shippingMethod, lstSettingValue, oboId);
            //To Add Search Indexing on Document
            if (documentId > 0)
            {
                //save header level entities starts here
                Collection<DocumentAdditionalEntityInfo> documentAdditionalEntityInfo = documentAdditionalEntityInfo = new Collection<DocumentAdditionalEntityInfo>();
                if (documentCode <= 0)
                {
                    var splitAccountingFields = objP2PDocumentManager.GetAllAccountingFieldsWithDefaultValues(P2PDocumentType.Requisition, LevelType.HeaderLevel,
                        userId, 0, null, null, false, 0, LOBEntityDetailCode, PreferenceLOBType.Belong);
                    if (!CompareInfo.ReferenceEquals(splitAccountingFields, null) && splitAccountingFields.Count() > 0)
                    {
                        var entityId = Convert.ToInt32(commonManager.GetSettingsValueByKey(noneSettingDetails, "EntityMappedToBillToLocation"));
                        var entityDetailCode = splitAccountingFields.Where(data => data.EntityTypeId == entityId).FirstOrDefault().EntityDetailCode;
                        var flag = false;
                        flag = GetReqDao().UpdateBillToLocation(documentId, entityDetailCode, LOBEntityDetailCode);
                    }
                    splitAccountingFields.ForEach(p => documentAdditionalEntityInfo.Add(new DocumentAdditionalEntityInfo()
                    {
                        EntityCode = p.EntityCode,
                        EntityDetailCode = p.EntityDetailCode,
                        EntityDisplayName = p.DisplayName,
                        EntityId = p.EntityTypeId,
                        ParentEntityDetailCode = p.ParentEntityDetailCode,
                        IsAccountingEntity = p.IsAccountingEntity,
                        LOBEntityDetailCode = p.LOBEntityDetailCode
                    }));
                    GetReqDao().SaveDocumentAdditionalEntityInfo(documentId, documentAdditionalEntityInfo);
                }

                //save header level entities ends here
                List<DocumentAdditionalEntityInfo> documentAdditionalEntityInfoList = new List<DocumentAdditionalEntityInfo>();
                if (documentAdditionalEntityInfo.Count <= 0)
                    documentAdditionalEntityInfoList = null;
                else
                    documentAdditionalEntityInfoList = documentAdditionalEntityInfo.Cast<DocumentAdditionalEntityInfo>().ToList();

                if (!Convert.ToBoolean(commonManager.GetSettingsValueByKey(noneSettingDetails, "IsEnableADRRules")))
                {
                    var documentSplitItemEntities = objP2PDocumentManager.GetDocumentDefaultAccountingDetails(P2PDocumentType.Requisition, LevelType.ItemLevel, oboId > 0 ? oboId : UserContext.ContactCode, documentId, documentAdditionalEntityInfoList);
                    GetReqDao().SaveDefaultAccountingDetails(documentId, documentSplitItemEntities, precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges, true);
                }
                else
                {
                    List<ADRSplit> documentSplitItemEntities = objP2PDocumentManager.GetDocumentDefaultAccountingDetailsForLineItems(P2PDocumentType.Requisition,
                        userContext.ContactCode, documentId, documentAdditionalEntityInfoList, null, false, null, LOBEntityDetailCode,
                        ADRIdentifier.DocumentItemId, null);
                    GetReqDao().SaveDefaultAccountingDetailsforADR(documentId, documentSplitItemEntities, precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges, true);
                }
                // addinig parameter to not update tax on punchout catalog item which comes with taxes
                GetReqDao().UpdateTaxOnHeaderShipTo(documentId, 0, precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges, true);
                var currentPrincipal = System.Threading.Thread.CurrentPrincipal;
                Task.Factory.StartNew((scope) =>
                {
                    System.Threading.Thread.CurrentPrincipal = currentPrincipal;
                    if (documentCode <= 0)
                    {
                        ((P2PDocumentManager)scope).AddInfoToPortal(P2PDocumentType.Requisition, new Requisition
                        {
                            DocumentId = documentCode,
                            DocumentName = requisitionName,
                            DocumentNumber = requisitionNumber,
                            CreatedBy = userId
                        });
                    }
                }, objP2PDocumentManager);
                #region My Task Implementation
                Task.Factory.StartNew((scope) =>
                {
                    System.Threading.Thread.CurrentPrincipal = currentPrincipal;
                    var ctx = (UserExecutionContext)UserContext;
                    
                    List<TaskActionDetails> lstTasksAction = new List<TaskActionDetails>();
                    lstTasksAction.Add(TaskManager.CreateActionDetails(ActionKey.SentForApproval, SqlConstants.SENT_FOR_APPROVAl));
                    var objTaskManager = new TaskManager() { UserContext = ctx, GepConfiguration = GepConfiguration };
                    var delegateSaveTaskActionDetails = new TaskManager.InvokeSaveTaskActionDetails(objTaskManager.SaveTaskActionDetails);
                    delegateSaveTaskActionDetails.BeginInvoke(TaskManager.CreateTaskObject(documentId, userId, lstTasksAction, false, false, ctx.BuyerPartnerCode, ctx.CompanyName), null, null);

                }, UserContext);
                #endregion

                #region Copying OrgEntities from Catalog to Requisition based on settings
                var catalogSettingDetails = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.Catalog, UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBEntityDetailCode);

                var allowOrgEntityFromCatalog = Convert.ToBoolean(commonManager.GetSettingsValueByKey(catalogSettingDetails, "AllowOrgEntityInCatalogItems"), CultureInfo.InvariantCulture).ToString().ToUpper();

                if (allowOrgEntityFromCatalog == "TRUE")
                {
                    var corporationEntityId = Convert.ToInt32(commonManager.GetSettingsValueByKey(catalogSettingDetails, "CorporationEntityId"), CultureInfo.InvariantCulture);
                    var expenseCodeEntityId = Convert.ToInt32(commonManager.GetSettingsValueByKey(catalogSettingDetails, "ExpenseCodeEntityId"), CultureInfo.InvariantCulture);
                    GetReqDao().UpdateCatalogOrgEntitiesToRequisition(documentId, corporationEntityId, expenseCodeEntityId);
                }
                AddIntoSearchIndexerQueueing(documentId, (int)DocumentType.Requisition, true);
                #endregion
            }
            //To Add Search Indexing on Document
            return documentId;
        }

        /// <summary>
        /// This function is for testing the MotleyService calls, it is not used anywhere for now
        /// </summary>
        /// <param name="LOBEntityDetailCode"></param>
        /// <returns></returns>
        private string GetEntityNumberForRequisition(string documentType, long LOBEntityDetailCode)
        {
            Req.BusinessObjects.RESTAPIHelper.RequestHeaders requestHeaders = new Req.BusinessObjects.RESTAPIHelper.RequestHeaders();
            string requisitionNumber;            
            string appName = Environment.GetEnvironmentVariable("NewRelic.AppName") ?? "Requisition"; 
            string useCase = "RequisitionManager-GetEntityNumberForRequisition";
            string serviceURL = MultiRegionConfig.GetConfig(CloudConfig.APIMBaseURL) + GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.ServiceURLs.MotleyServiceURL + "GetEntityNumber";
            Dictionary<string, object> body = new Dictionary<string, object>();
            body.Add("entityType", documentType);
            body.Add("lobId", LOBEntityDetailCode);
            body.Add("entityDetailCode", 0);
            body.Add("purchaseTypeID", 0);

            requestHeaders.Set(UserContext, this.JWTToken);
            var webAPI = new GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.WebAPI(requestHeaders, appName, useCase);
            requisitionNumber = webAPI.ExecutePost(serviceURL, body);
            return requisitionNumber.Replace("\"", string.Empty); 
        }

        public List<RequisitionSplitItems> GetRequisitionAccountingDetailsByItemId(long requisitionItemId, int pageIndex, int pageSize, int itemType, long LOBId)
        {
            var commonManager = new RequisitionCommonManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
            var p2pSettings = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.None, UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBId);
            int precessionValue = convertStringToInt(commonManager.GetSettingsValueByKey(p2pSettings, "MaxPrecessionValue"));
            int maxPrecessionforTotal = convertStringToInt(commonManager.GetSettingsValueByKey(p2pSettings, "MaxPrecessionValueforTotal"));
            int maxPrecessionForTaxesAndCharges = convertStringToInt(commonManager.GetSettingsValueByKey(p2pSettings, "MaxPrecessionValueForTaxesAndCharges"));
            return GetReqDao().GetRequisitionAccountingDetailsByItemId(requisitionItemId, pageIndex,
                                                                                     pageSize, itemType, precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges);
        }

        public long SaveRequisitionAccountingDetails(List<RequisitionSplitItems> requisitionSplitItems, List<DocumentSplitItemEntity> requisitionSplitItemEntities, decimal lineItemQuantity, bool updateTaxes, long LOBId)
        {
            var commonManager = new RequisitionCommonManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
            var p2pSettings = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.None, UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBId);
            int precessionValue = convertStringToInt(commonManager.GetSettingsValueByKey(p2pSettings, "MaxPrecessionValue"));
            int maxPrecessionforTotal = convertStringToInt(commonManager.GetSettingsValueByKey(p2pSettings, "MaxPrecessionValueforTotal"));
            int maxPrecessionForTaxesAndCharges = convertStringToInt(commonManager.GetSettingsValueByKey(p2pSettings, "MaxPrecessionValueForTaxesAndCharges"));
            var updateTaxOnPunchoutItem = Convert.ToBoolean(commonManager.GetSettingsValueByKey(p2pSettings, "UpdateTaxOnPunchoutItem"));
            var useTaxMaster = true;

            if (!updateTaxOnPunchoutItem)
            {
                useTaxMaster = false;
                updateTaxes = false;
            }

            return SaveRequisitionAccountingDetails(requisitionSplitItems, requisitionSplitItemEntities, lineItemQuantity, precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges, useTaxMaster, updateTaxes);//need documentcode for indexing
        }

        public long SaveRequisitionAccountingDetails(List<RequisitionSplitItems> requisitionSplitItems, List<DocumentSplitItemEntity> requisitionSplitItemEntities, decimal lineItemQuantity, int precessionValue, int maxPrecessionforTotal, int maxPrecessionForTaxesAndCharges, bool useTaxMaster = true, bool updateTaxes = true)
        {
            return GetReqDao().SaveRequisitionAccountingDetails(requisitionSplitItems, requisitionSplitItemEntities, lineItemQuantity, precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges, useTaxMaster, updateTaxes);
        }

        public bool CheckAccountingSplitValidations(long requisitionId)
        {
            return GetReqDao().CheckAccountingSplitValidations(requisitionId);
        }

        public long CopyRequisitionToRequisition(long newrequisitionId, string requisitionIds, string buIds, long LOBId)
        {
            if (newrequisitionId == 0)
            {
                long LOBEntityDetailCode = UserContext.BelongingEntityDetailCode;
                int LOBEntityId = UserContext.BelongingEntityId;
                if (LOBEntityId <= 0)
                {
                    UserContext userContext = null;
                    try
                    {
                        var partnerHelper = new Req.BusinessObjects.RESTAPIHelper.PartnerHelper(UserContext, JWTToken);
                        userContext = partnerHelper.GetUserContextDetailsByContactCode(UserContext.ContactCode);
                    }
                    catch (CommunicationException ex)
                    {
                        LogHelper.LogError(Log, "Error occured in SaveCatalogRequisition Method in RequisitionManager", ex);
                    }
                    finally
                    {
                        //DisposeService(objPartnerServiceChannel);
                    }
                    LOBEntityId = userContext.GetDefaultBelongingUserLOBMapping().EntityId;
                    LOBEntityDetailCode = userContext.GetDefaultBelongingUserLOBMapping().EntityDetailCode;
                }
                var document = new Requisition
                {
                    CompanyName = UserContext.ClientName,
                    Currency = "USD",
                    CreatedBy = UserContext.ContactCode,
                    RequisitionItems = new List<RequisitionItem>(),
                    EntityId = LOBEntityId,
                    EntityDetailCode = new List<long>() { LOBEntityDetailCode }
                };

                RequisitionDocumentManager  manager = new RequisitionDocumentManager(JWTToken);
                manager.GepConfiguration = GepConfiguration;
                manager.UserContext = UserContext;

                newrequisitionId = manager.Save(P2PDocumentType.Requisition, document);

            }
            var commonManager = new RequisitionCommonManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };

            int precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges;
            var p2pSettings = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.None, UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBId);
            precessionValue = convertStringToInt(commonManager.GetSettingsValueByKey(p2pSettings, "MaxPrecessionValue"));
            maxPrecessionforTotal = convertStringToInt(commonManager.GetSettingsValueByKey(p2pSettings, "MaxPrecessionValueforTotal"));
            maxPrecessionForTaxesAndCharges = convertStringToInt(commonManager.GetSettingsValueByKey(p2pSettings, "MaxPrecessionValueForTaxesAndCharges"));
            var showTaxJurisdictionForShipToValue= commonManager.GetSettingsValueByKey(p2pSettings, "ShowTaxJurisdictionForShipTo");
            string showTaxJurisdictionForShipTo = (showTaxJurisdictionForShipToValue == string.Empty ? "No" : showTaxJurisdictionForShipToValue);
            string enableGetLineItemsBulkAPISetting = (commonManager.GetSettingsValueByKey(P2PDocumentType.Requisition, "EnableGetLineItemsBulkAPI", commonManager.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBId));
            bool enableGetLineItemsBulkAPI = string.IsNullOrEmpty(enableGetLineItemsBulkAPISetting) ? false : true;
            string AllowMultiCurrencyInRequisitionSetting = (commonManager.GetSettingsValueByKey(P2PDocumentType.Requisition, "AllowMultiCurrencyInRequisition", commonManager.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBId));
            bool AllowMultiCurrencyInRequisition = !string.IsNullOrEmpty(AllowMultiCurrencyInRequisitionSetting) ? Convert.ToBoolean(AllowMultiCurrencyInRequisitionSetting) : false;

            ItemSearchInput itemSearchInput = new ItemSearchInput();
            ItemSearchBulkInput itemSearchBulkInput = new ItemSearchBulkInput();
            ItemBulkInputRequest itemBulkInputRequest = new ItemBulkInputRequest();
            List<KeyValuePair<long, decimal>> catlogItems = new List<KeyValuePair<long, decimal>>();
            List<KeyValuePair<long, decimal>> itemMasteritems = new List<KeyValuePair<long, decimal>>();
            List<CurrencyExchageRate> lstCurrencyExchageRates = new List<CurrencyExchageRate>();
            bool isCatalogItemValid = true; 
            try
            {
                if (AllowMultiCurrencyInRequisition)
                {
                    List<CurrencyExchageRate> currencyExchageRates = GetReqDao().GetRequisitionCurrency(Convert.ToString(requisitionIds));
                    if (currencyExchageRates != null && currencyExchageRates.Count > 0)
                    {
                        foreach (var currencyExchageRate in currencyExchageRates)
                        {
                            CurrencyExchageRate objCurrencyExchageRate = new CurrencyExchageRate();
                            decimal ExchangeRate = commonManager.GetCurrencyConversionRate(currencyExchageRate.FromCurrencyCode, currencyExchageRate.ToCurrencyCode);
                            objCurrencyExchageRate.FromCurrencyCode = currencyExchageRate.FromCurrencyCode;
                            objCurrencyExchageRate.ToCurrencyCode = currencyExchageRate.ToCurrencyCode;
                            objCurrencyExchageRate.ExchangeRate = ExchangeRate;
                            lstCurrencyExchageRates.Add(objCurrencyExchageRate);
                        }
                    }

                }

                if (enableGetLineItemsBulkAPI)
                {
                    itemSearchBulkInput = GetCatalogLineDetailsForBulkWebAPI(newrequisitionId, requisitionIds);
                    if (itemSearchBulkInput.ItemInputList != null && itemSearchBulkInput.ItemInputList.Count > 0)
                    {
                        itemSearchBulkInput.IsIdSearch = true;
                        SearchResult searchResult = new SearchResult();
                        var requestHeaders = new GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.RequestHeaders();
                        string appName = Req.BusinessObjects.URLs.AppNameForGetLineItems;
                        string useCase = GEP.Cumulus.P2P.Req.BusinessObjects.URLs.UseCaseForGetLineItems;
                        requestHeaders.Set(UserContext, this.JWTToken);
                        var webAPI = new GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.WebAPI(requestHeaders, appName, useCase);
                        string GetLineItemsBulkUrl = MultiRegionConfig.GetConfig(CloudConfig.AppURL) + GEP.Cumulus.P2P.Req.BusinessObjects.URLs.GetLineItemAccess;
                        var test1 = JsonConvert.SerializeObject(itemSearchBulkInput);

                        var JsonResult = webAPI.ExecutePost(GetLineItemsBulkUrl, itemSearchBulkInput);
                        searchResult = JsonConvert.DeserializeObject<SearchResult>(JsonResult);
                        List<ResponseItem> Items = searchResult.ResponseItems;
                        foreach (var objCatalogItems in Items)
                        {
                            decimal unitPrice = 0;
                            if ((objCatalogItems.CatalogItemId > 0) || (objCatalogItems.IMId > 0))
                            {
                                if (objCatalogItems.EffectivePrice ==0)
                                {
                                    unitPrice = (decimal)objCatalogItems.UnitPrice;
                                }
                                else
                                {
                                    unitPrice = (decimal)objCatalogItems.EffectivePrice;
                                }
                                if (objCatalogItems.CatalogItemId > 0)
                                {
                                    if (!catlogItems.Contains(new KeyValuePair<long, decimal>(objCatalogItems.CatalogItemId, unitPrice)))
                                        catlogItems.Add(new KeyValuePair<long, decimal>(objCatalogItems.CatalogItemId, unitPrice));
                                }
                                else
                                {
                                    if (!itemMasteritems.Contains(new KeyValuePair<long, decimal>(objCatalogItems.IMId, unitPrice)))
                                        itemMasteritems.Add(new KeyValuePair<long, decimal>(objCatalogItems.IMId, unitPrice));
                                }
                            }
                        }

                        if (itemSearchBulkInput.ItemInputList.Count != Items.Count)
                        {
                            isCatalogItemValid = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occurred in GetCatalogLineDetailsForBulkWebAPI method in RequistionManager.", ex);
                throw ex;
            }
            GetReqDao().CopyRequisitionToRequisition(newrequisitionId, requisitionIds, buIds, precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges, showTaxJurisdictionForShipTo.ToLower()=="yes"?true:false, catlogItems, itemMasteritems, enableGetLineItemsBulkAPI, lstCurrencyExchageRates);

            GetReqDao().UpdateTaxOnHeaderShipTo(newrequisitionId, 0, precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges, false, requisitionIds);

            #region Saving default accounting
            RequisitionDocumentManager  objP2PDocumentManager = new RequisitionDocumentManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
            var documentSplitItemEntities = objP2PDocumentManager.GetDocumentDefaultAccountingDetails(P2PDocumentType.Requisition, LevelType.ItemLevel, 0, newrequisitionId);
            GetReqDao().SaveDefaultAccountingDetails(newrequisitionId, documentSplitItemEntities, precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges, true);
            #endregion

            AddIntoSearchIndexerQueueing(newrequisitionId, (int)DocumentType.Requisition);
            if (enableGetLineItemsBulkAPI && !isCatalogItemValid)
            {
                newrequisitionId = -1;
            }
            return newrequisitionId;
        }

        public DocumentIntegration.Entities.DocumentIntegrationEntity GetDocumentDetailsByDocumentCode(long documentCode)
        {
            var commonManager = new RequisitionCommonManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
            string documentStatuses = commonManager.GetSettingsValueByKey(P2PDocumentType.Requisition, "AccessibleDocumentStatuses", UserContext.ContactCode, (int)SubAppCodes.P2P);
            return GetReqDao().GetDocumentDetailsByDocumentCode(documentCode, documentStatuses);
        }

        public bool UpdateSendforBiddingDocumentStatus(long documentCode)
        {
            List<TaskActionDetails> lstTasksAction = new List<TaskActionDetails>();
            lstTasksAction.Add(TaskManager.CreateActionDetails(ActionKey.SentForApproval, SqlConstants.SENT_FOR_APPROVAl));
            var objTaskManager = new TaskManager() { UserContext = UserContext, GepConfiguration = GepConfiguration };
            var delegateSaveTaskActionDetails = new TaskManager.InvokeSaveTaskActionDetails(objTaskManager.SaveTaskActionDetails);
            delegateSaveTaskActionDetails.BeginInvoke(TaskManager.CreateTaskObject(documentCode, UserContext.ContactCode, lstTasksAction, true, false, UserContext.BuyerPartnerCode, UserContext.CompanyName), null, null);
            bool returnValue = GetReqDao().UpdateSendforBiddingDocumentStatus(documentCode);

            AddIntoSearchIndexerQueueing(documentCode, (int)DocumentType.Requisition);

            return returnValue;
        }

        public List<long> GetAllCategoriesByReqId(long documentId)
        {
            return GetReqDao().GetAllCategoriesByReqId(documentId);
        }

        public KeyValuePair<long, decimal> GetAllEntitiesByReqId(long documentId, int entityTypeId)
        {
            DataSet reqEntities = new DataSet();
            KeyValuePair<long, decimal> lstReqEntities = new KeyValuePair<long, decimal>();
            reqEntities = GetReqDao().GetAllEntitiesByReqId(documentId, entityTypeId);

            if (reqEntities.Tables.Count > 0)
            {

                string fromCurrency = string.Empty;
                string toCurrency = string.Empty;
                decimal conversionRate = 1;
                decimal convertedAmt = 0;
                fromCurrency = Convert.ToString(reqEntities.Tables[0].Rows[0]["FromCurrency"]);
                toCurrency = Convert.ToString(reqEntities.Tables[0].Rows[0]["ToCurrency"]);

                if (fromCurrency != toCurrency && toCurrency != "")
                {
                    RequisitionCommonManager objCommonManager = new RequisitionCommonManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
                    conversionRate = objCommonManager.GetCurrencyConversionFactor(fromCurrency, toCurrency);
                }
                if (reqEntities.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow row in reqEntities.Tables[1].Rows)
                    {
                        if (fromCurrency != toCurrency && toCurrency != "")
                            convertedAmt = Convert.ToDecimal(row["Amount"]) * conversionRate;
                        else
                            convertedAmt = Convert.ToDecimal(row["Amount"]);
                        lstReqEntities = new KeyValuePair<long, decimal>(Convert.ToInt64(row["ReqEntityId"]), convertedAmt);
                    }
                }
            }

            return lstReqEntities;
        }

        public bool DeleteAllSplitsByDocumentId(long documentId, long ContactCode, long LOBId)
        {
            bool result;
            result = GetReqDao().DeleteAllSplitsByDocumentId(documentId);

            #region Saving default accounting
            if (result)
            {
                int precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges;

                var commonManager = new RequisitionCommonManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
                precessionValue = convertStringToInt(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "MaxPrecessionValue", UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBId));
                maxPrecessionforTotal = convertStringToInt(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "MaxPrecessionValueforTotal", UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBId));
                maxPrecessionForTaxesAndCharges = convertStringToInt(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "MaxPrecessionValueForTaxesAndCharges", UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBId));

                RequisitionDocumentManager  objP2PDocumentManager = new RequisitionDocumentManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
                var documentSplitItemEntities = objP2PDocumentManager.GetDocumentDefaultAccountingDetails(P2PDocumentType.Requisition, LevelType.ItemLevel, ContactCode, documentId);
                GetReqDao().SaveDefaultAccountingDetails(documentId, documentSplitItemEntities, precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges, true);
                AddIntoSearchIndexerQueueing(documentId, (int)DocumentType.Requisition);
                return true;
            }
            #endregion

            return false;
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
        }

        //internal ItemSourceType getRequisitionSourceTypes(byte ItemSource)
        //{
        //    if (ItemSource == (byte)GEP.Cumulus.SmartCatalog.BusinessEntities.CatalogSource.Hosted)
        //        return ItemSourceType.Hosted;
        //    else if (ItemSource == (byte)GEP.Cumulus.SmartCatalog.BusinessEntities.CatalogSource.Punchout)
        //        return ItemSourceType.Punchout;
        //    else if (ItemSource == (byte)GEP.Cumulus.SmartCatalog.BusinessEntities.CatalogSource.ItemMaster)
        //        return ItemSourceType.Internal;
        //    else if (ItemSource == (byte)GEP.Cumulus.SmartCatalog.BusinessEntities.CatalogSource.Manual)
        //        return ItemSourceType.Manual;
        //    else if (ItemSource == (byte)GEP.Cumulus.SmartCatalog.BusinessEntities.CatalogSource.HostedAndItemMaster)
        //        return ItemSourceType.HostedAndInternal;

        //    return ItemSourceType.Other;
        //}

        private List<RequisitionSplitItems> ValidateAndUpdateSplits(RequisitionItem objItem, int precessionValue, decimal ItemTotal,
                                                                    List<SplitAccountingFields> splitEntity, bool includeTax,
                                                                    List<OrganizationStructureService.GeneralLedger> lstGL,
                                                                    List<OrganizationStructureService.OrgEntity> lstOrgEntity,
                                                                    List<DocumentAdditionalEntityInfo> lstHeaderEntityDetails,
                                                                    List<SplitAccountingFields> lstSplitAccountingFields,
                                                                    bool populateDefaultSplitValue, int allowNegativeValues,
                                                                    ref RequisitionCommonManager objCommon)
        {
            RequisitionDocumentManager  objDocMgr = new RequisitionDocumentManager(JWTToken);
            objDocMgr.GepConfiguration = this.GepConfiguration;
            objDocMgr.UserContext = this.UserContext;
            int splitCounter = 1;
            decimal Totalquantity = 0;
            StringBuilder strError = new StringBuilder();
            if (objItem.ItemSplitsDetail != null && objItem.ItemSplitsDetail.Count > 0)
            {
                foreach (RequisitionSplitItems itemSplitDetail in objItem.ItemSplitsDetail)
                {
                    var positiveSplitItemTotal = Math.Abs(itemSplitDetail.SplitItemTotal ?? 0);
                    var positiveUnitPrice = Math.Abs(objItem.UnitPrice ?? 0);
                    if (objItem.UnitPrice > 0 && itemSplitDetail.SplitItemTotal < 0)
                        strError.AppendLine("Split total for Item No: " + objItem.ItemLineNumber + " can not be negative.");
                    if (allowNegativeValues > 0 && objItem.UnitPrice < 0 && itemSplitDetail.SplitItemTotal > 0)
                        strError.AppendLine("Invalid Split total for Item No: " + objItem.ItemLineNumber + " .");

                    itemSplitDetail.Quantity = Math.Round(Convert.ToDecimal((((positiveSplitItemTotal / ((Convert.ToDecimal(objItem.Quantity) * Convert.ToDecimal(positiveUnitPrice))
                                                                                + ((includeTax == true) ? (Convert.ToDecimal(objItem.Tax)
                                                                                + Convert.ToDecimal(objItem.ShippingCharges)
                                                                                + Convert.ToDecimal(objItem.AdditionalCharges)) : 0)
                                                                                )) * 100) * objItem.Quantity) / 100), precessionValue);

                    itemSplitDetail.DocumentSplitItemId = 0;
                    itemSplitDetail.UiId = splitCounter;
                    Totalquantity += itemSplitDetail.Quantity;
                    if (splitCounter == objItem.ItemSplitsDetail.Count)
                    {
                        var qtyDiff = objItem.Quantity - Totalquantity;
                        if (qtyDiff > 0)
                            itemSplitDetail.Quantity += qtyDiff;
                    }
                    itemSplitDetail.DocumentItemId = objItem.DocumentItemId;
                    itemSplitDetail.SplitType = SplitType.Percentage;
                    itemSplitDetail.Percentage = Math.Round(Convert.ToDecimal((positiveSplitItemTotal / ((objItem.Quantity * positiveUnitPrice)
                                                                                + ((includeTax == true) ? (Convert.ToDecimal(objItem.Tax)
                                                                                + Convert.ToDecimal(objItem.ShippingCharges)
                                                                                + Convert.ToDecimal(objItem.AdditionalCharges)) : 0)
                                                                                )) * 100), precessionValue);

                    itemSplitDetail.Tax = Math.Round(Convert.ToDecimal((itemSplitDetail.Percentage * objItem.Tax) / 100), precessionValue);
                    itemSplitDetail.ShippingCharges = Math.Round(Convert.ToDecimal((itemSplitDetail.Percentage * objItem.ShippingCharges) / 100), precessionValue);
                    itemSplitDetail.AdditionalCharges = Math.Round(Convert.ToDecimal((itemSplitDetail.Percentage * objItem.AdditionalCharges) / 100), precessionValue);
                    foreach (var entity in splitEntity)
                    {
                        var matchingEntity = itemSplitDetail.DocumentSplitItemEntities.Where(splt =>
                                              splt.EntityType.Equals((string.IsNullOrWhiteSpace(entity.Title) ? "" : entity.Title), StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

                        if (entity.IsMandatory && matchingEntity == null && !entity.Title.Equals("Requester", StringComparison.InvariantCultureIgnoreCase))
                        {
                            strError.AppendLine("Line Item number : " + objItem.ItemLineNumber + " missing value for mandatory split entity  : " + entity.Title + ". ");
                        }

                        else if (matchingEntity != null && entity.EntityTypeId > 0)
                        {
                            var entityDetails = lstOrgEntity.Where(orgEntity =>
                                               orgEntity.EntityCode.Equals(matchingEntity.EntityCode, StringComparison.InvariantCultureIgnoreCase) && orgEntity.objEntityType.EntityId == entity.EntityTypeId).FirstOrDefault();

                            if (entityDetails != null && entityDetails.OrgEntityCode > 0)
                            {
                                matchingEntity.SplitAccountingFieldId = entity.SplitAccountingFieldId;
                                matchingEntity.UiId = splitCounter;
                                matchingEntity.SplitAccountingFieldValue = Convert.ToString(entityDetails.OrgEntityCode);
                                matchingEntity.EntityTypeId = entityDetails.objEntityType.EntityId;
                                matchingEntity.EntityCode = entityDetails.EntityCode;

                            }

                            else
                                strError.AppendLine("Line number " + +objItem.ItemLineNumber + " has an invalid Accounting Details value for field (" + entity.Title + "). ");
                        }
                        else if (entity.Title.Equals("Requester", StringComparison.InvariantCultureIgnoreCase) && entity.EntityTypeId == 0)
                        {
                            itemSplitDetail.DocumentSplitItemEntities.Add(
                            new DocumentSplitItemEntity()
                            {
                                SplitAccountingFieldId = entity.SplitAccountingFieldId,
                                UiId = splitCounter,
                                SplitAccountingFieldValue = Convert.ToString(this.UserContext.ContactCode),
                                EntityDisplayName = (string.IsNullOrWhiteSpace(entity.DisplayName) ? "" : entity.DisplayName),
                                EntityType = string.IsNullOrWhiteSpace(entity.FieldName) ? "" : entity.Title,
                                EntityCode = string.IsNullOrWhiteSpace(entity.EntityCode) ? "" : entity.EntityCode
                            });
                        }
                        else if (entity.Title.Equals("GL Code", StringComparison.InvariantCultureIgnoreCase) && entity.EntityTypeId == 0)
                        {
                            if (matchingEntity == null)
                            {
                                itemSplitDetail.DocumentSplitItemEntities.Add(
                                    new DocumentSplitItemEntity()
                                    {
                                        SplitAccountingFieldId = entity.SplitAccountingFieldId,
                                        UiId = splitCounter,
                                        SplitAccountingFieldValue = Convert.ToString(entity.EntityDetailCode),
                                        EntityDisplayName = (string.IsNullOrWhiteSpace(entity.DisplayName) ? "" : entity.DisplayName),
                                        EntityType = string.IsNullOrWhiteSpace(entity.FieldName) ? "" : entity.Title,
                                        EntityCode = string.IsNullOrWhiteSpace(entity.EntityCode) ? "" : entity.EntityCode
                                    });
                            }

                            else
                            {
                                OrganizationStructureService.GeneralLedger glDetails;
                                glDetails = lstGL.Where(gllst => gllst.GeneralLedgerCode.Equals(matchingEntity.EntityCode, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

                                if (glDetails == null || (glDetails != null && glDetails.GeneralLedgerId <= 0))
                                    strError.AppendLine("Line number " + +objItem.ItemLineNumber + " has an invalid Accounting Details value for field (" + entity.Title + "). ");

                                else
                                {
                                    matchingEntity.SplitAccountingFieldId = entity.SplitAccountingFieldId;
                                    matchingEntity.UiId = splitCounter;
                                    matchingEntity.SplitAccountingFieldValue = Convert.ToString(glDetails.GeneralLedgerId);
                                    matchingEntity.EntityTypeId = entity.EntityTypeId;
                                    matchingEntity.EntityCode = glDetails.GeneralLedgerCode;
                                }
                            }
                        }
                        else if (!string.IsNullOrWhiteSpace(entity.Title))
                        {
                            itemSplitDetail.DocumentSplitItemEntities.Add(
                                new DocumentSplitItemEntity()
                                {
                                    SplitAccountingFieldId = entity.SplitAccountingFieldId,
                                    UiId = splitCounter,
                                    SplitAccountingFieldValue = Convert.ToString(entity.EntityDetailCode),
                                    EntityDisplayName = (string.IsNullOrWhiteSpace(entity.DisplayName) ? "" : entity.DisplayName),
                                    EntityType = string.IsNullOrWhiteSpace(entity.FieldName) ? "" : entity.Title,
                                    EntityCode = string.IsNullOrWhiteSpace(entity.EntityCode) ? "" : entity.EntityCode
                                });
                        }

                    }
                    splitCounter++;
                }
            }

            else
            {
                //Need to remove this call and use splitEntity
                var defaultAccDetails = objDocMgr.GetDocumentDefaultAccountingDetails(P2PDocumentType.Requisition, LevelType.ItemLevel,
                                                                                      UserContext.ContactCode, 0,
                                                                                      lstHeaderEntityDetails, lstSplitAccountingFields,
                                                                                      populateDefaultSplitValue);
                //Need to remove this call and use splitEntity

                objItem.ItemSplitsDetail = new List<RequisitionSplitItems>();
                defaultAccDetails.ForEach(splt => splt.UiId = splitCounter);
                objItem.ItemSplitsDetail = new List<RequisitionSplitItems>()
                                                            {
                                                                new RequisitionSplitItems(){
                                                                    DocumentSplitItemEntities=defaultAccDetails,
                                                                    Quantity= objItem.Quantity,
                                                                    Tax = objItem.Tax,
                                                                    SplitType= SplitType.Percentage,
                                                                    DocumentItemId = objItem.DocumentItemId,
                                                                    UiId=splitCounter,
                                                                    Percentage=100,
                                                                    SplitItemTotal = (objItem.Quantity * objItem.UnitPrice) + (objItem.Tax !=null  && objItem.Tax> 0 ? objItem.Tax : 0)
                                                                                     + (objItem.AdditionalCharges !=null  && objItem.AdditionalCharges> 0 ? objItem.AdditionalCharges : 0)
                                                                                     + (objItem.ShippingCharges !=null  && objItem.ShippingCharges> 0 ? objItem.ShippingCharges:0)
                                                                }
                                                            };
            }

            if (!string.IsNullOrWhiteSpace(strError.ToString()))
                throw new Exception(strError.ToString());

            return objItem.ItemSplitsDetail;

        }
        //public void SetRequisitionAdditionalEntityFromInterface_New(Requisition objRequisition, out int headerEntityId, out string headerEntityName, string LobEntityCode = "")
        //{
        //    #region Set Document Additional Entities Details
        //    long lobEntityDetailCode = 0;
        //    if (objRequisition.EntityDetailCode != null && objRequisition.EntityDetailCode.Any())
        //        lobEntityDetailCode = objRequisition.EntityDetailCode.FirstOrDefault();
        //    List<SplitAccountingFields> splitConfiguration = GetDao<IP2PDocumentDAO>().GetAllSplitAccountingFields(P2PDocumentType.Requisition, LevelType.HeaderLevel, 0, lobEntityDetailCode);
        //    headerEntityId = 0;
        //    headerEntityName = string.Empty;
        //    int i = 0;
        //    objRequisition.DocumentLOBDetails = new List<DocumentLOBDetails>();
        //    ProxyOrganizationStructureService proxyOrganizationService = new ProxyOrganizationStructureService(this.UserContext);
        //    foreach (SplitAccountingFields split in splitConfiguration)
        //    {
        //        if (split != null)
        //        {
        //            headerEntityId = split.EntityTypeId;
        //            headerEntityName = !string.IsNullOrEmpty(split.Title) ? split.Title : string.Empty;
        //        }
        //        if (objRequisition.DocumentAdditionalEntitiesInfoList != null && objRequisition.DocumentAdditionalEntitiesInfoList.Any())
        //        {
        //            try
        //            {
        //                if (!string.IsNullOrEmpty(objRequisition.DocumentAdditionalEntitiesInfoList[i].EntityCode))
        //                {
        //                    var entityDetail = proxyOrganizationService.GetOrgEntityByEntityCode(objRequisition.DocumentAdditionalEntitiesInfoList[i].EntityCode, headerEntityName, LobEntityCode);
        //                    if (entityDetail != null && entityDetail.OrgEntityCode > 0)
        //                    {
        //                        objRequisition.DocumentAdditionalEntitiesInfoList[i].EntityDetailCode = entityDetail.OrgEntityCode;
        //                        objRequisition.DocumentAdditionalEntitiesInfoList[i].EntityId = headerEntityId;
        //                        objRequisition.DocumentAdditionalEntitiesInfoList[i].EntityDisplayName = headerEntityName;
        //                        objRequisition.DocumentLOBDetails.Add(new DocumentLOBDetails
        //                        {
        //                            EntityCode = entityDetail.LOBEntityCode,
        //                            EntityDetailCode = entityDetail.LOBEntityDetailCode
        //                        });
        //                    }
        //                }
        //                else
        //                {
        //                    objRequisition.DocumentAdditionalEntitiesInfoList[i].EntityDetailCode = 0;
        //                    objRequisition.DocumentAdditionalEntitiesInfoList[i].EntityId = headerEntityId;
        //                }

        //            }
        //            finally
        //            {

        //            }
        //        }
        //        i++;
        //    }
        //    #endregion
        //}

        private void SetRequisitionAdditionalEntityFromInterface(Requisition objRequisition, out int headerEntityId, out string headerEntityName, StringBuilder strErrors, int accessControlId)
        {
            #region Set Document Additional Entities Details
            List<SplitAccountingFields> splitConfiguration = GetSQLP2PDocumentDAO().GetAllSplitAccountingFields(P2PDocumentType.Requisition, LevelType.HeaderLevel, 0);


            headerEntityId = 0;
            headerEntityName = string.Empty;
            var headerEntity = string.Empty;
            if (splitConfiguration != null)
            {
                headerEntityId = splitConfiguration.FirstOrDefault().EntityTypeId;
                headerEntityName = splitConfiguration.FirstOrDefault().Title;
            }
            if (headerEntityId == accessControlId && objRequisition.DocumentAdditionalEntitiesInfoList == null)
            {
                objRequisition.DocumentAdditionalEntitiesInfoList = new Collection<DocumentAdditionalEntityInfo>();
                objRequisition.DocumentAdditionalEntitiesInfoList.Add(new DocumentAdditionalEntityInfo()
                {
                    EntityDetailCode = objRequisition.BusinessUnitId,
                    EntityId = accessControlId
                });
            }
            else
            {
                ProxyOrganizationStructureService objOrgProxy = new ProxyOrganizationStructureService(this.UserContext, this.JWTToken);
                try
                {
                    if (objRequisition.DocumentAdditionalEntitiesInfoList != null && objRequisition.DocumentAdditionalEntitiesInfoList.Any())
                    {
                        var entityDetail = objOrgProxy.GetOrgEntityByEntityCode(objRequisition.DocumentAdditionalEntitiesInfoList.FirstOrDefault().EntityCode, headerEntityName);
                        if (entityDetail != null && entityDetail.OrgEntityCode > 0)
                        {
                            objRequisition.DocumentAdditionalEntitiesInfoList.FirstOrDefault().EntityDetailCode = entityDetail.OrgEntityCode;
                            objRequisition.DocumentAdditionalEntitiesInfoList.FirstOrDefault().EntityId = headerEntityId;
                        }
                        else
                            strErrors.AppendLine("Invalid " + headerEntityName + " (" + objRequisition.DocumentAdditionalEntitiesInfoList.FirstOrDefault().EntityCode + "). ");
                    }
                    else
                    {
                        var objSearch = new OrganizationStructure.Entities.OrgSearch
                        {
                            AssociationTypeInfo = OrganizationStructure.Entities.AssociationType.Both,
                            OrgEntityCode = objRequisition.BusinessUnitId,
                            objEntityType = new OrganizationStructure.Entities.OrgEntityType { },
                            LOBEntityDetailCode = objRequisition.EntityDetailCode.FirstOrDefault()
                        };

                        OrgSearchResult entityDetail;
                        var executionHelper = new GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.ExecutionHelper(this.UserContext, this.GepConfiguration, this.JWTToken);
                        if (executionHelper.Check(23, Req.BusinessObjects.RESTAPIHelper.ExecutionHelper.WebAPIType.Common))
                        {
                            var csmHelper = new Req.BusinessObjects.RESTAPIHelper.CSMHelper(UserContext, JWTToken);
                            var body = new
                            {
                                OrgEntityCode = objRequisition.BusinessUnitId,
                                EntityId = 0,
                                AssociationTypeInfo = (int)OrganizationStructure.Entities.AssociationType.Both,
                                LOBEntityDetailCode = objRequisition.EntityDetailCode.FirstOrDefault()
                            };
                            entityDetail = csmHelper.GetSearchedEntityDetails(body).ToList().FirstOrDefault();
                        }
                        else
                        {
                            entityDetail = objOrgProxy.GetEntitySearchResults(objSearch).ToList().FirstOrDefault();
                        }
                        if (entityDetail != null)
                        {
                            OrgSearchResult parentEntityDetail;
                            if (executionHelper.Check(24, Req.BusinessObjects.RESTAPIHelper.ExecutionHelper.WebAPIType.Common))
                            {
                                var csmHelper = new Req.BusinessObjects.RESTAPIHelper.CSMHelper(UserContext, JWTToken);
                                var body = new
                                {
                                    OrgEntityCode = entityDetail.ParentOrgEntityCode,
                                    EntityId = 0,
                                    AssociationTypeInfo = (int)OrganizationStructure.Entities.AssociationType.Both,
                                    LOBEntityDetailCode = objRequisition.EntityDetailCode.FirstOrDefault()
                                };
                                parentEntityDetail = csmHelper.GetSearchedEntityDetails(body).ToList().FirstOrDefault();
                            }
                            else
                            {
                                objSearch.OrgEntityCode = entityDetail.ParentOrgEntityCode;
                                //Get the Parent Entity Details till it matches the Heder configuration Entity type
                                parentEntityDetail = objOrgProxy.GetEntitySearchResults(objSearch).ToList().FirstOrDefault();
                            }

                            while (parentEntityDetail != null && parentEntityDetail.EntityId != headerEntityId)
                            {
                                if (executionHelper.Check(25, Req.BusinessObjects.RESTAPIHelper.ExecutionHelper.WebAPIType.Common))
                                {
                                    var csmHelper = new Req.BusinessObjects.RESTAPIHelper.CSMHelper(UserContext, JWTToken);
                                    var body = new
                                    {
                                        OrgEntityCode = parentEntityDetail.ParentOrgEntityCode,
                                        EntityId = 0,
                                        AssociationTypeInfo = (int)OrganizationStructure.Entities.AssociationType.Both,
                                        LOBEntityDetailCode = objRequisition.EntityDetailCode.FirstOrDefault()
                                    };
                                    parentEntityDetail = csmHelper.GetSearchedEntityDetails(body).ToList().FirstOrDefault();
                                }
                                else
                                {
                                    objSearch.OrgEntityCode = parentEntityDetail.ParentOrgEntityCode;
                                    parentEntityDetail = objOrgProxy.GetEntitySearchResults(objSearch).ToList().FirstOrDefault();
                                }
                            }

                            //Get Top 1 Heder configuration Entity details if its not available after traversing hierarchy
                            if (parentEntityDetail == null)
                            {
                                if (executionHelper.Check(26, Req.BusinessObjects.RESTAPIHelper.ExecutionHelper.WebAPIType.Common))
                                {
                                    var csmHelper = new Req.BusinessObjects.RESTAPIHelper.CSMHelper(UserContext, JWTToken);
                                    var body = new
                                    {
                                        OrgEntityCode = 0,
                                        EntityId = headerEntityId,
                                        AssociationTypeInfo = (int)OrganizationStructure.Entities.AssociationType.Both,
                                        LOBEntityDetailCode = objRequisition.EntityDetailCode.FirstOrDefault()
                                    };
                                    parentEntityDetail = csmHelper.GetSearchedEntityDetails(body).ToList().FirstOrDefault();
                                }
                                else
                                {
                                    objSearch.OrgEntityCode = 0;
                                    objSearch.objEntityType = new OrganizationStructure.Entities.OrgEntityType { EntityId = headerEntityId };
                                    parentEntityDetail = objOrgProxy.GetEntitySearchResults(objSearch).ToList().FirstOrDefault();
                                }
                            }

                            if (parentEntityDetail != null && parentEntityDetail.OrgEntityCode > 0)
                            {
                                objRequisition.DocumentAdditionalEntitiesInfoList = new Collection<DocumentAdditionalEntityInfo>();
                                objRequisition.DocumentAdditionalEntitiesInfoList.Add(new DocumentAdditionalEntityInfo() { EntityDetailCode = parentEntityDetail.OrgEntityCode, EntityId = parentEntityDetail.EntityId });
                            }
                        }
                    }
                }
                finally
                {
                }
            }
            #endregion
        }

        internal void ProrateHeaderTaxAndShipping(Requisition ObjReq)
        {
            GetReqDao().ProrateHeaderTaxAndShipping(ObjReq);
            if (this.UserContext.Product != GEPSuite.eInterface)
                AddIntoSearchIndexerQueueing(ObjReq.DocumentCode, (int)DocumentType.Requisition);
        }

        public bool copyLineItem(long requisitionItemId, long requisitionId, int txtNumberOfCopies, long LOBId)
        {
            var commonManager = new RequisitionCommonManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
            RequisitionDocumentManager  objP2PDocumentManager = new RequisitionDocumentManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
            int MaxPrecessionValue, MaxPrecessionValueTotal, MaxPrecessionValueForTaxAndCharges;
            var p2pSettings = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.None, UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBId);
            MaxPrecessionValue = convertStringToInt(commonManager.GetSettingsValueByKey(p2pSettings, "MaxPrecessionValue"));
            MaxPrecessionValueTotal = convertStringToInt(commonManager.GetSettingsValueByKey(p2pSettings, "MaxPrecessionValueforTotal"));
            MaxPrecessionValueForTaxAndCharges = convertStringToInt(commonManager.GetSettingsValueByKey(p2pSettings, "MaxPrecessionValueForTaxesAndCharges"));

            bool returnValue = GetReqDao().copyLineItem(requisitionItemId, requisitionId, txtNumberOfCopies, MaxPrecessionValue, MaxPrecessionValueTotal, MaxPrecessionValueForTaxAndCharges);
            AddIntoSearchIndexerQueueing(requisitionId, (int)DocumentType.Requisition);
            return returnValue;
        }

        public bool UpdateReqItemOnPartnerChange(long requisitionItemId, long partnerCode, long LOBId)//need documentcode for indexing
        {
            var commonManager = new RequisitionCommonManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
            var p2pSettings = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.None, UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBId);
            var precessionValue = convertStringToInt(commonManager.GetSettingsValueByKey(p2pSettings, "MaxPrecessionValue"));
            int maxPrecessionforTotal = convertStringToInt(commonManager.GetSettingsValueByKey(p2pSettings, "MaxPrecessionValueforTotal"));
            int maxPrecessionForTaxesAndCharges = convertStringToInt(commonManager.GetSettingsValueByKey(p2pSettings, "MaxPrecessionValueForTaxesAndCharges"));

            var result = GetReqDao().UpdateReqItemOnPartnerChange(requisitionItemId, partnerCode, precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges);
            if (result)
            {
                result = GetReqDao().SaveLineItemTaxes(requisitionItemId, precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges);
            }
            return result;
        }

        public bool GetRequisitionItemAccountingStatus(long requisitionItemId)
        {
            return GetReqDao().GetRequisitionItemAccountingStatus(requisitionItemId);
        }

        public List<PartnerInfo> GetPreferredPartnerByReqItemId(long DocumentItemId, int pageIndex, int pageSize, string partnerName, out long partnerCode)
        {
            return GetReqDao().GetPreferredPartnerByReqItemId(DocumentItemId, pageIndex, pageSize, partnerName, out partnerCode);
        }

        public List<ItemPartnerInfo> GetPreferredPartnerByCatalogItemId(long DocumentItemId, int pageIndex, int pageSize, string partnerName, string currencyCode, long entityDetailCode, out long partnerCode, string buList = "")
        {
            return GetReqDao().GetPreferredPartnerByCatalogItemId(DocumentItemId, pageIndex, pageSize, partnerName, currencyCode, entityDetailCode, out partnerCode, buList);
        }
        public bool CheckRequisitionCatalogItemAccess(long newrequisitionId, string requisitionIds)
        {
            return GetReqDao().CheckRequisitionCatalogItemAccess(newrequisitionId, requisitionIds);
        }
        public bool CheckOBOUserCatalogItemAccess(long requisitionId, long requesterId, bool delItems = false)
        {
            if (requisitionId == 0)
            {
                return false;
            }

            var commonManager = new RequisitionCommonManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };

            long LOBId = GetCommonDao().GetLOBByDocumentCode(requisitionId);
            var p2pSettings = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.None, UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBId);
            int maxPrecessionValue = convertStringToInt(commonManager.GetSettingsValueByKey(p2pSettings, "MaxPrecessionValue"));
            int maxPrecessionValueTotal = convertStringToInt(commonManager.GetSettingsValueByKey(p2pSettings, "MaxPrecessionValueforTotal"));
            int maxPrecessionValueForTaxAndCharges = convertStringToInt(commonManager.GetSettingsValueByKey(p2pSettings, "MaxPrecessionValueForTaxesAndCharges"));

            return GetReqDao().CheckOBOUserCatalogItemAccess(requisitionId, requesterId, maxPrecessionValue, maxPrecessionValueTotal, maxPrecessionValueForTaxAndCharges, delItems);
        }

        public bool UpdateTaxOnLineItem(long requisitionItemId, ICollection<Taxes> lstTaxes, long LOBEntityDetailCode)//need documentcode for indexing
        {
            var commonManager = new RequisitionCommonManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
            var p2pSettings = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.None, UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBEntityDetailCode);
            int precessionValue = convertStringToInt(commonManager.GetSettingsValueByKey(p2pSettings, "MaxPrecessionValue"));
            int precessionValueForTotal = convertStringToInt(commonManager.GetSettingsValueByKey(p2pSettings, "MaxPrecessionValueforTotal"));
            int precessionValueForTaxAndCharges = convertStringToInt(commonManager.GetSettingsValueByKey(p2pSettings, "MaxPrecessionValueForTaxesAndCharges"));
            var result = GetReqDao().UpdateTaxOnLineItem(requisitionItemId, lstTaxes, precessionValue, precessionValueForTotal, precessionValueForTaxAndCharges);
            return result;
        }

        public bool GetRequisitionCapitalCodeCountById(long requisitionId)
        {
            return GetReqDao().GetRequisitionCapitalCodeCountById(requisitionId);
        }
        public Dictionary<string, string> SentRequisitionForApproval(long contactCode, long documentCode, decimal documentAmount, int documentTypeId, string fromCurrency, string toCurrency,
                                                                     bool isOperationalBudgetEnabled, long headerOrgEntityCode, bool overrideAllowAsyncMethods = false, bool isBypassOperationalBudget = false)
        {

            long LOBId = GetCommonDao().GetLOBByDocumentCode(documentCode);

            RequisitionDocumentManager  p2pDocMgr = new RequisitionDocumentManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
            var commonManager = new RequisitionCommonManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
            var newReqManager = new GEP.Cumulus.P2P.Req.BusinessObjects.NewRequisitionManager(this.JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
            bool? IsOperationalbudgetPopupEnable = Convert.ToBoolean(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "IsOperationalbudgetPopupEnable", commonManager.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBId));

            string results = string.Empty;
            Dictionary<string, string> result = new Dictionary<string, string>();
            toCurrency = toCurrency == "" ? GetDefaultCurrency() : toCurrency;

            bool flag = true;
            if (this.UserContext.Product == GEPSuite.eInterface)
            {
                flag = this.CheckAccountingSplitValidations(documentCode);
            }
            DataTable flagForBudgetSplitAccountingValidation = new DataTable();
            flagForBudgetSplitAccountingValidation.TableName = "Validate Budget Accounting Details";
            if (!flag)
            {
                result.Add("IsCheckAccountingSplitValidations", "1");

            }
            else
            {
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

                if ((!isBypassOperationalBudget) && isOperationalBudgetEnabled && result.Count() == 0)
                {
                    //bool AllowBudgetValidationOnOverAll = Convert.ToBoolean(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "AllowBudgetValidationOnOverAll", commonManager.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBId));
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
                        newReqManager.saveBudgetoryStatus(flagForBudgetSplitAccountingValidation, documentCode);
                }
            }

            if (result.Count() == 0)
            {

                var AllowAsyncMethods = Convert.ToBoolean(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "AllowAsyncMethods", UserContext.UserId, (int)SubAppCodes.P2P, "", LOBId));
                var documentManager = new RequisitionDocumentManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
                documentManager.UpdateDocumentStatus(P2PDocumentType.Requisition, documentCode, DocumentStatus.ApprovalPending, 0);

                if (overrideAllowAsyncMethods) { AllowAsyncMethods = false; }

                if (AllowAsyncMethods)
                {
                    var currentPrincipal = System.Threading.Thread.CurrentPrincipal;
                    Task.Factory.StartNew((_userContext) =>
                    {
                        System.Threading.Thread.CurrentPrincipal = currentPrincipal;
                        var ctx = (UserExecutionContext)_userContext;
                        string strReturnVal = string.Empty;
                        try
                        {
                            strReturnVal = SendRequisitionForApproval(contactCode, documentCode, documentAmount, documentTypeId, fromCurrency, toCurrency, isOperationalBudgetEnabled, headerOrgEntityCode, _userContext, LOBId);
                        }
                        catch (Exception ex)
                        {
                            LogHelper.LogError(Log, "Error occured in SendRequisitionForApproval Method in RequisitionManager Error:" + ex.Message, ex);
                            strReturnVal = string.Empty;
                        }
                        finally
                        {
                            if (string.IsNullOrEmpty(strReturnVal.Trim()))
                            {
                                //Set DocumentStatus as Failed
                                var objDocManager = new RequisitionDocumentManager(JWTToken) { UserContext = ctx, GepConfiguration = GepConfiguration };
                                documentManager.UpdateDocumentStatus(P2PDocumentType.Requisition, documentCode, DocumentStatus.SendForApprovalFailed, 0);

                                //Set DocumentStatus as Failed
                                var objEmailNotificationManager = new RequisitionEmailNotificationManager(ctx, this.JWTToken)
                                {
                                    UserContext = ctx,
                                    GepConfiguration = GepConfiguration
                                };

                                newReqManager.UpdateLineStatusForRequisition(documentCode, (StockReservationStatus)(DocumentStatus.SendForApprovalFailed), true, null);
                                objEmailNotificationManager.SendFailureNotificaiton(documentCode, DocumentType.Requisition, FailureAction.Approval);
                            }
                            else
                            {
                                newReqManager.UpdateLineStatusForRequisition(documentCode, (StockReservationStatus)(DocumentStatus.ApprovalPending), true, null);
                            }
                        }
                    }, this.UserContext);
                    result.Add("SendForApprovalResult", "Success");
                }
                else
                {
                    object _userContext = this.UserContext;
                    string resultVal = SendRequisitionForApproval(contactCode, documentCode, documentAmount, documentTypeId, fromCurrency, toCurrency, isOperationalBudgetEnabled, headerOrgEntityCode, _userContext, LOBId);
                    result.Add("SendForApprovalResult", resultVal);
                    if (!(string.IsNullOrEmpty(resultVal)))
                        newReqManager.UpdateLineStatusForRequisition(documentCode, (StockReservationStatus)(DocumentStatus.ApprovalPending), true, null);
                }

            }
            return result;
        }
        private string SendRequisitionForApproval(long contactCode, long documentCode, decimal documentAmount, int documentTypeId, string fromCurrency, string toCurrency, bool isOperationalBudgetEnabled, long headerOrgEntityCode, object _userContext, long LOBId = 0)
        {
            var objP2PCommonManager = new RequisitionCommonManager(JWTToken) { UserContext = (UserExecutionContext)_userContext, GepConfiguration = GepConfiguration };

            bool isHeaderLevelEntityBU = SettingsHelper.GetAccessControlSettingsForDocument((UserExecutionContext)_userContext, (int)DocumentType.Requisition);
            if (!isHeaderLevelEntityBU)
            {
                UserContext userContext = null;
                try
                {
                    var userExecutionContext = (UserExecutionContext)_userContext;
                    var partnerHelper = new Req.BusinessObjects.RESTAPIHelper.PartnerHelper(UserContext, JWTToken);
                    userContext = partnerHelper.GetUserContextDetailsByContactCode(contactCode);
                }
                catch (Exception ex)
                {
                    // Log Exception here
                    LogHelper.LogError(Log, "Error occurred in GetDefaultAssetBULocation method in ReceiptManager.", ex);
                    throw;
                }
                finally
                {
                    //DisposeService(objPartnerServiceChannel);
                }
                var lstBUDetails = GetContactORGMapping(contactCode, ((UserExecutionContext)_userContext).GetBelongingAccessControlEntityId(userContext));
                SaveRequisitionBusinessUnit(documentCode, lstBUDetails.Where(data => data.IsDefault).FirstOrDefault().OrgEntityCode);
            }
            else
            {
                SaveRequisitionBusinessUnit(documentCode, headerOrgEntityCode);
            }

            RequisitionDocumentManager  objP2pDocMgr = new RequisitionDocumentManager(JWTToken) { UserContext = (UserExecutionContext)_userContext, GepConfiguration = GepConfiguration };
            decimal ConversionFactor;
            RequisitionCommonManager objReqCommonManager = new RequisitionCommonManager(JWTToken) { UserContext = (UserExecutionContext)_userContext, GepConfiguration = GepConfiguration };
            ConversionFactor = objReqCommonManager.GetCurrencyConversionFactor(fromCurrency, toCurrency);
            documentAmount = documentAmount * ConversionFactor;
            objReqCommonManager.UpdateBaseCurrency(contactCode, documentCode, documentAmount, (int)DocumentType.Requisition, toCurrency, ConversionFactor);
            return objP2pDocMgr.SentDocumentForApproval(contactCode, documentCode, documentAmount, (int)DocumentType.Requisition, ConversionFactor);
        }

        private string SaveOfflineApprovalDetails(long contactCode, long documentCode, decimal documentAmount, string fromCurrency, string toCurrency, WorkflowInputEntities workflowEntity, long headerOrgEntityCode, object _userContext)
        {

            bool isHeaderLevelEntityBU = SettingsHelper.GetAccessControlSettingsForDocument((UserExecutionContext)_userContext, (int)DocumentType.Requisition);
            if (!isHeaderLevelEntityBU)
            {
                Gep.Cumulus.Partner.Entities.UserContext userContext = null;
                try
                {
                    var userExecutionContext = (UserExecutionContext)_userContext;
                    var partnerHelper = new Req.BusinessObjects.RESTAPIHelper.PartnerHelper(UserContext, JWTToken);
                    userContext = partnerHelper.GetUserContextDetailsByContactCode(contactCode);
                }
                catch (Exception ex)
                {
                    // Log Exception here
                    LogHelper.LogError(Log, "Error occurred in SaveOfflineApprovalDetails method in RequisitionManager.", ex);
                    throw;
                }
                finally
                {
                    //DisposeService(objPartnerServiceChannel);
                }
                var lstBUDetails = GetContactORGMapping(contactCode, ((UserExecutionContext)_userContext).GetBelongingAccessControlEntityId(userContext));
                SaveRequisitionBusinessUnit(documentCode, lstBUDetails.Where(data => data.IsDefault).FirstOrDefault().OrgEntityCode);
            }
            else
            {
                SaveRequisitionBusinessUnit(documentCode, headerOrgEntityCode);
            }

            RequisitionDocumentManager  objP2pDocMgr = new RequisitionDocumentManager(JWTToken) { UserContext = (UserExecutionContext)_userContext, GepConfiguration = GepConfiguration };
            decimal ConversionFactor;
            var commonManager = new RequisitionCommonManager(JWTToken) { UserContext = (UserExecutionContext)_userContext, GepConfiguration = GepConfiguration };
            
            ConversionFactor = commonManager.GetCurrencyConversionFactor(fromCurrency, toCurrency);
            documentAmount = documentAmount * ConversionFactor;
            commonManager.UpdateBaseCurrency(contactCode, documentCode, documentAmount, (int)DocumentType.Requisition, toCurrency, ConversionFactor);
            return objP2pDocMgr.SaveOfflineApprovalDetails(contactCode, documentCode, documentAmount, (int)DocumentType.Requisition, workflowEntity, toCurrency);
        }

        public Dictionary<string, string> SaveOfflineApprovalDetails(long contactCode, long documentCode, decimal documentAmount, string fromCurrency, string toCurrency, WorkflowInputEntities workflowEntity, long headerOrgEntityCode)
        {

            long LOBId = GetCommonDao().GetLOBByDocumentCode(documentCode);
            RequisitionDocumentManager  p2pDocMgr = new RequisitionDocumentManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
            var commonManager = new RequisitionCommonManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
            string results = string.Empty;
            Dictionary<string, string> result = new Dictionary<string, string>();
            DataTable flagForBudgetSplitAccountingValidation = new DataTable();
            flagForBudgetSplitAccountingValidation.TableName = "Validate Budget Accounting Details";
            bool isOperationalBudgetEnabled = Convert.ToBoolean(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "IsOperationalBudgetEnabled", commonManager.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBId));
            toCurrency = toCurrency == "" ? GetDefaultCurrency() : toCurrency;

            var lstValidationResult = p2pDocMgr.ValidateDocumentByDocumentCode(P2PDocumentType.Requisition, documentCode, LOBId);
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

            if (isOperationalBudgetEnabled && result.Count() == 0)
            {
                //bool AllowBudgetValidationOnOverAll = Convert.ToBoolean(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "AllowBudgetValidationOnOverAll", commonManager.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBId));
                flagForBudgetSplitAccountingValidation = GetOperationalBudgetManager().ValidateBudgetSplitAccounting(documentCode, DocumentType.Requisition, false, LOBId);
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

            if (result.Count() == 0)
            {
                var AllowAsyncMethods = Convert.ToBoolean(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "AllowAsyncMethods", UserContext.UserId, (int)SubAppCodes.P2P, "", LOBId));
                if (AllowAsyncMethods)
                {
                    // Log Exception here
                    var currentPrincipal = System.Threading.Thread.CurrentPrincipal;
                    Task.Factory.StartNew((_userContext) =>
                    {
                        System.Threading.Thread.CurrentPrincipal = currentPrincipal;
                        SaveOfflineApprovalDetails(contactCode, documentCode, documentAmount, fromCurrency, toCurrency, workflowEntity, headerOrgEntityCode, _userContext);
                    }, this.UserContext);
                    result.Add("SendForApprovalResult", "Success");
                }
                else
                {
                    object _userContext = this.UserContext;
                    string resultVal = SaveOfflineApprovalDetails(contactCode, documentCode, documentAmount, fromCurrency, toCurrency, workflowEntity, headerOrgEntityCode, _userContext);
                    result.Add("SendForApprovalResult", resultVal);
                }
            }
            return result;
        }


        private string GetDefaultCurrency()
        {
            var objCommon = new RequisitionCommonManager(JWTToken) { UserContext = this.UserContext, GepConfiguration = this.GepConfiguration };
            return objCommon.GetDefaultCurrency();
        }

        

        public bool SaveContractInformation(long requisitionItemId, string extContractRef)
        {
            var result = GetReqDao().SaveContractInformation(requisitionItemId, extContractRef);
            return result;
        }

        public List<NewP2PEntities.RequisitionPartnerEntities> GetPartnerDetailsAndOrderingLocationById(long requisitionId)
        {
            long LOBId = GetCommonDao().GetLOBByDocumentCode(requisitionId);
            var commonManager = new RequisitionCommonManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
            int spendControlType = convertStringToInt(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "SpendControlType", commonManager.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBId));
            return GetReqDao().GetPartnerDetailsAndOrderingLocationById(requisitionId, spendControlType);

        }

        public List<PartnerDetails> GetPartnerDetailsAndOrderingLocationByOrderId(long requisitionId)
        {
            return GetReqDao().GetPartnerDetailsAndOrderingLocationByOrderId(requisitionId);

        }


        /// <summary>
        /// Get List of Purchase Request Form Questionnaire sets
        /// </summary>
        /// <param name="requisitionItemId">requisitionItemId</param>
        /// <returns>set of Questionnaire in List</returns>
        public List<Questionnaire> GetAllQuestionnaire(long requisitionItemId)
        {
            return GetReqDao().GetAllQuestionnaire(requisitionItemId);
        }


        

        public DataTable GetListofShipToLocDetails(string searchText, int pageIndex, int pageSize, bool getByID, int shipToLocID, long lOBEntityDetailCode, long entityDetailCode)
        {
            return GetReqDao().GetListofShipToLocDetails(searchText, pageIndex, pageSize, getByID, shipToLocID, lOBEntityDetailCode, entityDetailCode);
        }

        public DataTable GetListofBillToLocDetails(string searchText, int pageIndex, int pageSize, long entityDetailCode, bool getDefault, long lOBEntityDetailCode)
        {
            return GetReqDao().GetListofBillToLocDetails(searchText, pageIndex, pageSize, entityDetailCode, getDefault, lOBEntityDetailCode);
        }

        public DataTable CheckCatalogItemAccessForContactCode(long requisitionId, long requesterId)
        {
            return GetReqDao().CheckCatalogItemAccessForContactCode(requisitionId, requesterId);
        }
        /// <summary>
        /// Get Requisition Details for Outbound interface.
        /// </summary>
        /// <param name="buyerPartnerCode">Buyer Partner code to  get Requisition detail for specific client.</param>
        /// <param name="documentCode">Document code for Requisition</param>       
        /// <returns>BZRequisition containing All Requisitions details/</returns>
        public BZRequisition GetRequisitionDetailsById(long documentCode)
        {
            #region Variable Declaration
            BZRequisition objBZRequisition = new BZRequisition();
            objBZRequisition.Requisition = new Requisition();
            objBZRequisition.Requisition.RequisitionItems = new List<RequisitionItem>();

            var obj = GetReqDao().GetDocumentAdditionalEntityDetailsById(documentCode);

            var commonManager = new RequisitionCommonManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
            CommentsGroup commentGroup = null;
            List<Comments> lstComments = new List<Comments>();
            List<QuestionBankEntities.Question> headerQuestionSet = new List<QuestionBankEntities.Question>();
            List<QuestionBankEntities.Question> itemQuestionSet = new List<QuestionBankEntities.Question>();
            List<QuestionBankEntities.Question> splitQuestionSet = new List<QuestionBankEntities.Question>();
            #endregion

            #region Set the Setting
            var p2pSettings = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.None, UserContext.ContactCode, (int)SubAppCodes.P2P);
            int precessionValue = convertStringToInt(commonManager.GetSettingsValueByKey(p2pSettings, "MaxPrecessionValue"));
            int precissionTotal = convertStringToInt(commonManager.GetSettingsValueByKey(p2pSettings, "MaxPrecessionValueforTotal"));
            int precessionValueForTaxAndCharges = convertStringToInt(commonManager.GetSettingsValueByKey(p2pSettings, "MaxPrecessionValueForTaxesAndCharges"));
            string settingValue = string.Empty;
            var interfaceSettings = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.Interfaces, UserContext.ContactCode, (int)SubAppCodes.Interfaces);


            settingValue = commonManager.GetSettingsValueByKey(p2pSettings, "AllowDeliverToFreeText");
            bool deliverToFreeText = !string.IsNullOrEmpty(settingValue) ? Convert.ToBoolean(settingValue) : false;

            settingValue = commonManager.GetSettingsValueByKey(interfaceSettings, "ExcludeTaxAndChargesFromSplit");
            var excludeTaxAndChargesFromSplit = !string.IsNullOrEmpty(settingValue) ? Convert.ToBoolean(settingValue) : false;

            #endregion

            #region Get Requisition Header Details
            objBZRequisition = GetReqInterfaceDao().GetRequisitionHeaderDetailsByIdForInterface(documentCode, deliverToFreeText);

            if (ReferenceEquals(objBZRequisition.Requisition, null))
                return objBZRequisition;

            #endregion

            #region "Custom Attributes"
            if (objBZRequisition.Requisition.CustomAttrFormId > 0)
            {
                headerQuestionSet = GetCommonDao().GetQuestionSetByFormCode(objBZRequisition.Requisition.CustomAttrFormId);
                var headerQuestSetCodeList = headerQuestionSet.Select(questSetCode => questSetCode.QuestionSetCode).ToList<long>();

                if (headerQuestSetCodeList != null && headerQuestSetCodeList.Any())
                {
                    objBZRequisition.Requisition.CustomAttributes = new List<Questionnaire>();
                    commonManager.GetQuestionWithResponse(headerQuestSetCodeList, objBZRequisition.Requisition.CustomAttributes, objBZRequisition.Requisition.DocumentCode, false);
                }
            }
            if (objBZRequisition.Requisition.CustomAttrFormIdForItem > 0)
                itemQuestionSet = GetCommonDao().GetQuestionSetByFormCode(objBZRequisition.Requisition.CustomAttrFormIdForItem);
            if (objBZRequisition.Requisition.CustomAttrFormIdForSplit > 0)
                splitQuestionSet = GetCommonDao().GetQuestionSetByFormCode(objBZRequisition.Requisition.CustomAttrFormIdForSplit);

            #endregion "Custom Attributes"        

            #region Get Requisition Item Level Details

            objBZRequisition.Requisition.RequisitionItems = GetReqInterfaceDao().GetLineItemBasicDetailsForInterface(documentCode);

            #endregion

            #region 'Get Header Level Comments'

            var objRequisition = objBZRequisition.Requisition;
            var commentAccessType = "1,2,4";
            commentGroup = commonManager.GetComments(documentCode, P2PDocumentType.Requisition, commentAccessType, 1, 0, true);

            StringBuilder objStringBuilder = new StringBuilder();
            if (commentGroup != null && commentGroup.Comment != null && commentGroup.Comment.Any())
            {
                lstComments = commentGroup.Comment.Where(comments => !comments.CommentType.Equals(((int)CommentType.Reject).ToString()) && comments.CreatedBy != 0)
                                                  .ToList();

                lstComments.ForEach(comment => { objStringBuilder.Append(!string.IsNullOrEmpty(comment.CommentText) ? (comment.CommentText + "\n") : string.Empty); });
                commentGroup.Comment.FirstOrDefault().CommentText = objStringBuilder.ToString();
                objBZRequisition.Requisition.Comments = new List<Comments>();
                if (commentGroup.Comment != null && commentGroup.Comment.Any())
                    objBZRequisition.Requisition.Comments.Add(commentGroup.Comment.FirstOrDefault());
            }

            #endregion

            #region 'Get Item Level Comments / Custom Attributes / Split Details'
            foreach (RequisitionItem objRequisitionItem in objBZRequisition.Requisition.RequisitionItems)
            {
                #region "Comments"

                commentGroup = commonManager.GetComments(objRequisitionItem.DocumentItemId, P2PDocumentType.Requisition, commentAccessType, 2, 0, true);
                if (commentGroup != null && commentGroup.Comment != null && commentGroup.Comment.Any())
                {
                    objStringBuilder.Clear();
                    lstComments = commentGroup.Comment.Where(comments => !comments.CommentType.Equals(((int)CommentType.Reject).ToString()) && comments.CreatedBy != 0)
                                                     .ToList();
                    lstComments.ForEach(comment => { objStringBuilder.Append(!string.IsNullOrEmpty(comment.CommentText) ? (comment.CommentText + "\n") : string.Empty); });
                    commentGroup.Comment.FirstOrDefault().CommentText = objStringBuilder.ToString();
                    objRequisitionItem.Comments = new List<Comments>();
                    if (commentGroup.Comment != null && commentGroup.Comment.Any())
                        objRequisitionItem.Comments.Add(commentGroup.Comment.FirstOrDefault());
                }

                #endregion "Comments"

                #region "Custom Attributes"
                var itemQuestSetCodeList = itemQuestionSet.Select(questSetCode => questSetCode.QuestionSetCode).ToList<long>();

                if (itemQuestSetCodeList != null && itemQuestSetCodeList.Any())
                {
                    objRequisitionItem.CustomAttributes = new List<Questionnaire>();
                    commonManager.GetQuestionWithResponse(itemQuestSetCodeList, objRequisitionItem.CustomAttributes, objRequisitionItem.DocumentItemId, false);
                }
                #endregion "Custom Attributes"

                #region 'Get Shipping detail'
                var shippingDetails = (List<DocumentItemShippingDetail>)GetReqDao().GetShippingSplitDetailsByLiId(objRequisitionItem.DocumentItemId);
                if (shippingDetails != null)
                {
                    foreach (DocumentItemShippingDetail objDocumentItemShippingDetail in shippingDetails)
                    {
                        objDocumentItemShippingDetail.ShiptoLocation.ShiptoLocationNumber = objDocumentItemShippingDetail.ShiptoLocation.ShiptoLocationNumber;
                        objDocumentItemShippingDetail.ShiptoLocation.Address.StateCode = objDocumentItemShippingDetail.ShiptoLocation.Address.StateCode;
                        objDocumentItemShippingDetail.ShiptoLocation.Address.Country = objDocumentItemShippingDetail.ShiptoLocation.Address.CountryCode;
                        objDocumentItemShippingDetail.DelivertoLocation.DeliverTo = objDocumentItemShippingDetail.DelivertoLocation.DeliverTo;
                    }
                    objRequisitionItem.DocumentItemShippingDetails = shippingDetails;
                }
                #endregion

                #region "Split Details"
                var splitDetails = GetReqDao().GetRequisitionAccountingDetailsByItemId(objRequisitionItem.DocumentItemId, 0, 1000, (int)objRequisitionItem.ItemType, precessionValue, precissionTotal, precessionValueForTaxAndCharges);
                if (splitDetails != null && splitDetails.Count() > 0)
                {
                    foreach (RequisitionSplitItems reqSplit in splitDetails)
                    {
                        var isSingleAccSegmentRequired = Convert.ToInt32(commonManager.GetSettingsValueByKey(P2PDocumentType.Interfaces, "IsSingleAccSegmentRequired", 0, (int)SubAppCodes.Interfaces));

                        List<DocumentSplitItemEntity> lstSplitEntities = new List<DocumentSplitItemEntity>();
                        DocumentSplitItemEntity splitEntity = new DocumentSplitItemEntity();

                        if (isSingleAccSegmentRequired == 1)
                        {
                            string entityCodes = string.Empty;

                            foreach (DocumentSplitItemEntity oldSplitEntity in reqSplit.DocumentSplitItemEntities)
                            {
                                if (!string.IsNullOrWhiteSpace(oldSplitEntity.EntityCode))
                                    entityCodes = entityCodes + oldSplitEntity.EntityCode + ".";
                            }

                            entityCodes = entityCodes.Trim('.');
                            splitEntity.EntityCode = entityCodes;
                            splitEntity.EntityDisplayName = string.Empty;
                            splitEntity.EntityType = string.Empty;
                            lstSplitEntities.Add(splitEntity);

                            reqSplit.DocumentSplitItemEntities = lstSplitEntities;
                        }
                        else
                        {
                            foreach (DocumentSplitItemEntity oldSplitEntity in reqSplit.DocumentSplitItemEntities)
                            {
                                if (!string.IsNullOrWhiteSpace(oldSplitEntity.EntityCode))
                                    lstSplitEntities.Add(oldSplitEntity);
                            }

                            reqSplit.DocumentSplitItemEntities = lstSplitEntities;
                        }
                        if (excludeTaxAndChargesFromSplit)
                            reqSplit.SplitItemTotal = reqSplit.SplitItemTotal - (reqSplit.AdditionalCharges ?? 0) - (reqSplit.ShippingCharges ?? 0) - (reqSplit.Tax ?? 0);

                        #region "Custom Attributes"
                        var splitQuestSetCodeList = splitQuestionSet.Select(questSetCode => questSetCode.QuestionSetCode).ToList<long>();

                        if (splitQuestSetCodeList != null && splitQuestSetCodeList.Any())
                        {
                            reqSplit.CustomAttributes = new List<Questionnaire>();
                            commonManager.GetQuestionWithResponse(splitQuestSetCodeList, reqSplit.CustomAttributes, reqSplit.DocumentSplitItemId, false);
                        }
                        #endregion "Custom Attributes"
                    }

                    objRequisitionItem.ItemSplitsDetail = splitDetails;
                }
                #endregion "Split Details"
            }
            #endregion

            return objBZRequisition;
        }

        private List<string> ValidateShipToBillToFromInterface(RequisitionCommonManager objP2PCommonManager, Requisition objRequisition, Dictionary<string, string> dctSetting, out bool isHeaderShipToValid, out bool isLiShipToValid, long LobentitydetailCode)
        {
            long lobEntityDetailCode = objRequisition.EntityDetailCode != null ? objRequisition.EntityDetailCode.FirstOrDefault() : 0;

            int shipToLocSetting = Convert.ToInt32(dctSetting["IsNewShipToLocCreationRequired"], NumberFormatInfo.InvariantInfo);
            int billToLocSetting = Convert.ToInt32(dctSetting["IsNewBillToLocCreationRequired"], NumberFormatInfo.InvariantInfo);
            var settingValue = objP2PCommonManager.GetSettingsValueByKey(P2PDocumentType.None, "AllowDeliverToFreeText", UserContext.ContactCode, (int)SubAppCodes.P2P, "", lobEntityDetailCode);
            bool deliverToFreeText = !string.IsNullOrEmpty(settingValue) ? Convert.ToBoolean(settingValue) : false;

            string strDefaultBillToLocation = dctSetting["DefaultBillToLocation"];
            bool IsDefaultBillToLocation = !string.IsNullOrEmpty(strDefaultBillToLocation) ? Convert.ToBoolean(strDefaultBillToLocation) : false;
            var strEntityMappedToBillToLocation = objP2PCommonManager.GetSettingsValueByKey(P2PDocumentType.None, "EntityMappedToBillToLocation", UserContext.ContactCode, (int)SubAppCodes.P2P, "", lobEntityDetailCode);
            int EntityMappedToBillToLocation = string.IsNullOrEmpty(strEntityMappedToBillToLocation) ? 0 : Convert.ToInt32(strEntityMappedToBillToLocation);

            long entitydetailcode = 0;
            var selectedEntity = objRequisition.DocumentAdditionalEntitiesInfoList != null ? objRequisition.DocumentAdditionalEntitiesInfoList.Where(a => a.EntityId == EntityMappedToBillToLocation) : null;
            if (selectedEntity != null && selectedEntity.Count() > 0)
            {
                entitydetailcode = selectedEntity.FirstOrDefault().EntityDetailCode;
            }

            isHeaderShipToValid = false;
            isLiShipToValid = false;
            DataSet result = null;
            StringBuilder lstErrors = new StringBuilder();

            if (objRequisition != null && objRequisition.DocumentStatusInfo != DocumentStatus.Cancelled)
            {
                result = GetReqInterfaceDao().ValidateShipToBillToFromInterface(objRequisition, Convert.ToBoolean(shipToLocSetting), Convert.ToBoolean(billToLocSetting), deliverToFreeText, LobentitydetailCode, IsDefaultBillToLocation, entitydetailcode);

                #region Header ship to
                if (objRequisition.ShiptoLocation != null && result.Tables["HeaderShiptoDetails"].Rows.Count > 0)
                {
                    if (string.IsNullOrEmpty(result.Tables["HeaderShiptoDetails"].Rows[0]["ErrorDetails"].ToString()))
                    {
                        if ((int)result.Tables[0].Rows[0]["shiptolocid"] > 0)
                        {
                            objRequisition.ShiptoLocation.ShiptoLocationId = (int)result.Tables["HeaderShiptoDetails"].Rows[0]["shiptolocid"];
                            isHeaderShipToValid = true;
                        }
                    }


                    if (objRequisition.ShiptoLocation.ShiptoLocationId == 0 &&
                           Convert.ToInt32(shipToLocSetting, NumberFormatInfo.InvariantInfo) == 1)
                    {   //lstErrors.Add("There was an error in processing the order as it has invalid Ship to Location details at header level");
                        if (string.IsNullOrEmpty(result.Tables["HeaderShiptoDetails"].Rows[0]["ErrorDetails"].ToString()) == true)
                            isHeaderShipToValid = true;

                    }
                }
                #endregion

                #region Item ShipTo
                string strLiError = string.Empty;
                if (objRequisition.RequisitionItems != null)
                {

                    int emptyItemShipTo = 0;
                    foreach (DataRow dr in result.Tables["LineitemShiptoDetails"].Rows)
                    {
                        int liShipToLocationId = 0;
                        RequisitionItem lstRequisitionItem = objRequisition.RequisitionItems.Where(itm => itm.ItemLineNumber == Convert.ToInt64(dr["ItemLineNumber"])).FirstOrDefault();

                        if (lstRequisitionItem.DocumentItemShippingDetails != null &&
                                lstRequisitionItem.DocumentItemShippingDetails.Any() &&
                                lstRequisitionItem.DocumentItemShippingDetails.FirstOrDefault().ShiptoLocation != null)
                        {
                            if ((int)dr["shiptolocid"] > 0)
                            {
                                liShipToLocationId = (int)dr["shiptolocid"];

                                lstRequisitionItem.DocumentItemShippingDetails[0].ShiptoLocation.ShiptoLocationId = liShipToLocationId;

                            }
                            if (liShipToLocationId == 0 && Convert.ToInt32(shipToLocSetting, NumberFormatInfo.InvariantInfo) == 1)
                            {
                                if (!string.IsNullOrEmpty(dr["ErrorDetails"].ToString()))
                                {
                                    strLiError += dr["ErrorDetails"] + " " + dr["ItemLineNumber"] + ",";
                                }

                            }
                            else if (liShipToLocationId == 0)
                                strLiError += dr["ErrorDetails"] + " " + dr["ItemLineNumber"] + ",";
                        }
                        else
                        {
                            emptyItemShipTo += 1;
                            strLiError += dr["ErrorDetails"] + " " + dr["ItemLineNumber"] + ",";
                        }

                    }
                    strLiError = strLiError.Trim(',');
                    if (!isHeaderShipToValid && emptyItemShipTo == objRequisition.RequisitionItems.Count)  // to be removed
                    {
                        if (result.Tables["HeaderShiptoDetails"].Rows.Count <= 0)
                            lstErrors.Append("Invalid Ship to Location number or mandatory fields are missing in the Header.");  // to be removed
                        else
                            lstErrors.Append(result.Tables["HeaderShiptoDetails"].Rows[0]["ErrorDetails"].ToString());
                    }
                    else if (!isHeaderShipToValid && !string.IsNullOrWhiteSpace(strLiError) && strLiError.Length > 0)
                    {
                        lstErrors.Append(strLiError + ".");
                    }
                    else if (string.IsNullOrWhiteSpace(strLiError) && result.Tables["LineitemShiptoDetails"].Rows.Count > 0)
                        isLiShipToValid = true;
                    else if (!isHeaderShipToValid && !isLiShipToValid)
                        lstErrors.Append("There is an error in ship to location " + result.Tables["HeaderShiptoDetails"].Rows[0]["ErrorDetails"].ToString());



                }
                #endregion

                #region Bill To

                if (objRequisition.BilltoLocation != null)
                {
                    if (!string.IsNullOrEmpty(objRequisition.BilltoLocation.BilltoLocationNumber))
                    {
                        if ((int)result.Tables["HeaderBilltoDetails"].Rows[0]["Billtolocid"] > 0)
                        {
                            objRequisition.BilltoLocation.BilltoLocationId = (int)result.Tables["HeaderBilltoDetails"].Rows[0]["Billtolocid"];

                        }
                        else if (objRequisition.BilltoLocation.BilltoLocationId == 0 && Convert.ToInt32(billToLocSetting, NumberFormatInfo.InvariantInfo) == 0)
                            lstErrors.Append(result.Tables["HeaderBilltoDetails"].Rows[0]["ErrorDetails"].ToString() + ".");

                        if (objRequisition.BilltoLocation.BilltoLocationId == 0 && Convert.ToInt32(billToLocSetting, NumberFormatInfo.InvariantInfo) == 1)
                        {
                            if (!string.IsNullOrEmpty(result.Tables["HeaderBilltoDetails"].Rows[0]["ErrorDetails"].ToString()))
                                lstErrors.Append(result.Tables["HeaderBilltoDetails"].Rows[0]["ErrorDetails"].ToString() + ".");
                        }
                    }
                    else if (IsDefaultBillToLocation)
                    {
                        if ((int)result.Tables["HeaderBilltoDetails"].Rows[0]["Billtolocid"] > 0)
                        {
                            objRequisition.BilltoLocation.BilltoLocationId = (int)result.Tables["HeaderBilltoDetails"].Rows[0]["Billtolocid"];
                        }
                    }
                }

                #endregion

                #region Header deliver to
                if (deliverToFreeText == false)
                {
                    if (objRequisition.DelivertoLocation != null && !string.IsNullOrWhiteSpace(objRequisition.DelivertoLocation.DelivertoLocationNumber))
                    {
                        if (result.Tables["HeaderDelivertoDetails"].Rows.Count > 0)
                        {
                            if ((int)result.Tables["HeaderDelivertoDetails"].Rows[0]["Delivertolocid"] > 0)
                            {
                                objRequisition.DelivertoLocation.DelivertoLocationId = (int)result.Tables["HeaderDelivertoDetails"].Rows[0]["Delivertolocid"];
                            }
                            else
                                lstErrors.Append(result.Tables["HeaderDelivertoDetails"].Rows[0]["ErrorDetails"].ToString() + ".");
                        }
                    }
                }
                #endregion

                #region Item deliver to
                int liDeliverToLocationId = 0;

                if (result.Tables["LineLevelDelivertoDetails"].Rows.Count > 0)
                {
                    foreach (DataRow dr in result.Tables[4].Rows)
                    {
                        RequisitionItem lstRequisitionItem = objRequisition.RequisitionItems.Where(itm => itm.ItemLineNumber == Convert.ToInt64(dr["ItemLineNumber"])).FirstOrDefault();
                        if ((int)dr["Delivertolocid"] > 0)
                        {
                            liDeliverToLocationId = (int)dr["Delivertolocid"];
                            lstRequisitionItem.DocumentItemShippingDetails[0].DelivertoLocation.DelivertoLocationId = liDeliverToLocationId;
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(dr["DelivertoLocationNumber"].ToString()))
                            {
                                lstErrors.Append(dr["ErrorDetails"].ToString() + " " + (int)dr["ItemLineNumber"] + ".");
                            }
                        }
                    }
                }

                #endregion


            }
            List<string> lsterror = new List<string>();
            if (lstErrors.Length > 0)
                lsterror.Add(lstErrors.ToString());
            return lsterror;

        }

        public DataTable CheckCatalogItemsAccessForContactCode(long requesterId, string catalogItems)
        {
            return GetReqDao().CheckCatalogItemsAccessForContactCode(requesterId, catalogItems);
        }

         
        public bool UpdateRequisitionLineStatusonRFXCreateorUpdate(long documentCode, List<long> p2pLineItemId, DocumentType docType, bool IsDocumentDeleted = false)
        {
            try
            {
                RequisitionDocumentManager objP2PDocumentManager = new RequisitionDocumentManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
                objP2PDocumentManager.BroadcastPusher(documentCode, Gep.Cumulus.CSM.Entities.DocumentType.Requisition, "DataStale", "SendLinesForBidding");
                bool returnValue = GetReqDao().UpdateRequisitionLineStatusonRFXCreateorUpdate(documentCode, p2pLineItemId, docType, IsDocumentDeleted);
                if (IsDocumentDeleted)
                {
                    AddIntoSearchIndexerQueueing(documentCode, (int)DocumentType.Requisition);
                }
                return returnValue;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occurred in UpdateRequisitionLineStatusonRFXCreateorUpdate method in RequistionManager.", ex);
                throw ex;
            }
        }
        public ICollection<P2PItem> GetRequisitionItemsDispatchMode(long documentCode)
        {
            if (documentCode > 0)
            {
                return GetReqDao().GetRequisitionItemsDispatchMode(documentCode);
            }
            else
            {
                if (Log.IsWarnEnabled)
                    Log.Warn("In GetRequisitionItemsDispatchMode method documentCode parameter is less then or equal to 0.");
                return new List<P2PItem>();
            }
        }

        public List<Taxes> GetRequisitioneHeaderTaxes(long requisitionId, int pageIndex, int pageSize)
        {
            return GetReqDao().GetRequisitioneHeaderTaxes(requisitionId, pageIndex, pageSize);
        }

        public bool UpdateRequisitionHeaderTaxes(ICollection<Taxes> taxes, long requisitionId, bool updateLineTax = false, int accuredTax = 1)
        {
            var commonManager = new RequisitionCommonManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
            var precessionValue = commonManager.GetPrecisionValue();
            var precessionValueForTotal = commonManager.GetPrecisionValueforTotal();
            var precessionValueForTaxesAndCharges = commonManager.GetPrecisionValueForTaxesAndCharges();

            return GetReqDao().UpdateRequisitionHeaderTaxes(taxes, requisitionId, precessionValue, precessionValueForTotal, precessionValueForTaxesAndCharges, updateLineTax, accuredTax);
        }


        public int CopyRequisition(long SourceRequisitionId, long DestinationRequisitionId, long ContactCode, int PrecessionValue, bool isAccountChecked = false, bool isCommentsChecked = false, bool isNotesAndAttachmentChecked = false, bool isAddNonCatalogItems = true, bool isCheckReqUpdate = false, bool IsCopyEntireReq = false, bool isNewNotesAndAttachmentChecked = false)
        {
            var commonManager = new RequisitionCommonManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
            RequisitionDocumentManager  objP2PDocumentManager = new RequisitionDocumentManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
            var precessionValue = commonManager.GetPrecisionValue();
            var precessionValueForTotal = commonManager.GetPrecisionValueforTotal();
            var precessionValueForTaxesAndCharges = commonManager.GetPrecisionValueForTaxesAndCharges();
            int eventPerformed = 0;
            long LOBId = GetCommonDao().GetLOBByDocumentCode(SourceRequisitionId);
            string contractDocumentStatuses = (commonManager.GetSettingsValueByKey(P2PDocumentType.None, "ContractDocumentStatuses", commonManager.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBId));
            string showTaxJurisdictionForShipToValue = (commonManager.GetSettingsValueByKey(P2PDocumentType.None, "ShowTaxJurisdictionForShipTo", commonManager.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBId));
            string showTaxJurisdictionForShipTo= (showTaxJurisdictionForShipToValue == string.Empty ? "No":showTaxJurisdictionForShipToValue);
      string enableWebAPIForGetLineItemsSetting = (commonManager.GetSettingsValueByKey(P2PDocumentType.Requisition, "EnableWebAPIForGetLineItems", commonManager.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBId));
      bool enableWebAPIForGetLineItems = string.IsNullOrEmpty(enableWebAPIForGetLineItemsSetting) || enableWebAPIForGetLineItemsSetting.ToLower()!="true"? false : true;
            string enableGetLineItemsBulkAPISetting = (commonManager.GetSettingsValueByKey(P2PDocumentType.Requisition, "EnableGetLineItemsBulkAPI", commonManager.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBId));
            bool enableGetLineItemsBulkAPI = string.IsNullOrEmpty(enableGetLineItemsBulkAPISetting) || enableGetLineItemsBulkAPISetting.ToLower() != "true"? false : true;
            string AllowMultiCurrencyInRequisitionSetting = (commonManager.GetSettingsValueByKey(P2PDocumentType.Requisition, "AllowMultiCurrencyInRequisition", commonManager.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBId));
            bool AllowMultiCurrencyInRequisition = !string.IsNullOrEmpty(AllowMultiCurrencyInRequisitionSetting) ? Convert.ToBoolean(AllowMultiCurrencyInRequisitionSetting) : false;
            try
            {
                ItemSearchInput itemSearchInput = new ItemSearchInput();
                ItemSearchBulkInput itemSearchBulkInput = new ItemSearchBulkInput();
                ItemBulkInputRequest itemBulkInputRequest = new ItemBulkInputRequest();
                List<KeyValuePair<long, decimal>> catlogItems = new List<KeyValuePair<long, decimal>>();
                List<KeyValuePair<long, decimal>> itemMasteritems = new List<KeyValuePair<long, decimal>>();
                List<CurrencyExchageRate> lstCurrencyExchageRates = new List<CurrencyExchageRate>();
                if (enableGetLineItemsBulkAPI)
                {
                    itemSearchBulkInput = GetCatalogLineDetailsForBulkWebAPI(SourceRequisitionId);
                    if (itemSearchBulkInput.ItemInputList != null && itemSearchBulkInput.ItemInputList.Count > 0)
                    {
                        itemSearchBulkInput.IsIdSearch = true;
                        SearchResult searchResult = new SearchResult();
                        var requestHeaders = new GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.RequestHeaders();
                        string appName = Req.BusinessObjects.URLs.AppNameForGetLineItems;
                        string useCase = GEP.Cumulus.P2P.Req.BusinessObjects.URLs.UseCaseForGetLineItems;
                        requestHeaders.Set(UserContext, this.JWTToken);
                        var webAPI = new GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.WebAPI(requestHeaders, appName, useCase);
                        string GetLineItemsBulkUrl = MultiRegionConfig.GetConfig(CloudConfig.AppURL) + GEP.Cumulus.P2P.Req.BusinessObjects.URLs.GetLineItemAccess;
                        LogNewRelicAppForPayload(UserContext.BuyerPartnerCode, GetLineItemsBulkUrl, JsonConvert.SerializeObject(itemSearchBulkInput), "enableGetLineItemsBulkAPI");
                        var JsonResult = webAPI.ExecutePost(GetLineItemsBulkUrl, itemSearchBulkInput);
                        searchResult = JsonConvert.DeserializeObject<SearchResult>(JsonResult);
                        List<ResponseItem> Items = searchResult.ResponseItems;
                        foreach (var objCatalogItems in Items)
                        {
                            decimal unitPrice = 0;
                            if ((objCatalogItems.CatalogItemId > 0) || (objCatalogItems.IMId > 0))
                            {
                                if (objCatalogItems.EffectivePrice ==0)
                                {
                                    unitPrice = (decimal)objCatalogItems.UnitPrice;
                                }
                                else
                                {
                                    unitPrice = (decimal)objCatalogItems.EffectivePrice;
                                }
                                if (objCatalogItems.CatalogItemId > 0)
                                {
                                    if (!catlogItems.Contains(new KeyValuePair<long, decimal>(objCatalogItems.CatalogItemId, unitPrice)))
                                        catlogItems.Add(new KeyValuePair<long, decimal>(objCatalogItems.CatalogItemId, unitPrice));
                                }
                                else
                                {
                                    if (!itemMasteritems.Contains(new KeyValuePair<long, decimal>(objCatalogItems.IMId, unitPrice)))
                                        itemMasteritems.Add(new KeyValuePair<long, decimal>(objCatalogItems.IMId, unitPrice));
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (enableWebAPIForGetLineItems)
                    {
                        itemBulkInputRequest = GetCatalogLineDetailsForWebAPI(SourceRequisitionId);
                        if (itemBulkInputRequest.CatalogItemAdditionalInfoList != null && itemBulkInputRequest.CatalogItemAdditionalInfoList.Count > 0)
                        {
                            itemBulkInputRequest.ContactCode = UserContext.ContactCode;
                            SearchResult searchResult = new SearchResult();
                            var requestHeaders = new GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.RequestHeaders();
                            string appName = Req.BusinessObjects.URLs.AppNameForGetLineItems;
                            string useCase = GEP.Cumulus.P2P.Req.BusinessObjects.URLs.UseCaseForGetLineItems;
                            requestHeaders.Set(UserContext, this.JWTToken);
                            var webAPI = new GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.WebAPI(requestHeaders, appName, useCase);
                            string GetLineItemsBulkUrl = MultiRegionConfig.GetConfig(CloudConfig.AppURL) + GEP.Cumulus.P2P.Req.BusinessObjects.URLs.GetLineItemsBulk;
                            LogNewRelicAppForPayload(UserContext.BuyerPartnerCode, GetLineItemsBulkUrl, JsonConvert.SerializeObject(itemBulkInputRequest), "enableWebAPIForGetLineItems");
                            var JsonResult = webAPI.ExecutePost(GetLineItemsBulkUrl, itemBulkInputRequest);
                            searchResult = JsonConvert.DeserializeObject<SearchResult>(JsonResult);
                            List<CatalogItem> Items = searchResult.Items;
                            foreach (var objCatalogItems in Items)
                            {
                                decimal unitPrice = 0;
                                if ((objCatalogItems.Id > 0))
                                {
                                    if (objCatalogItems.EffectivePrice == null)
                                    {
                                        unitPrice = (decimal)objCatalogItems.UnitPrice;
                                    }
                                    else
                                    {
                                        unitPrice = (decimal)objCatalogItems.EffectivePrice;
                                    }
                                    if (objCatalogItems.CatalogItemId > 0)
                                    {
                                        if (!catlogItems.Contains(new KeyValuePair<long, decimal>(objCatalogItems.Id, unitPrice)))
                                                catlogItems.Add(new KeyValuePair<long, decimal>(objCatalogItems.Id, unitPrice));
                                    }

                                }
                            }
                        }
                    }
                    else if (!enableWebAPIForGetLineItems)
                    {
                        itemSearchInput = GetCatalogLineDetails(SourceRequisitionId);
                        if (itemSearchInput.CatalogItemAdditionalInfoList != null && itemSearchInput.CatalogItemAdditionalInfoList.Count > 0)
                        {
                            itemSearchInput.ContactCode = UserContext.ContactCode;
                            ProxyCatalogService proxyCatalogService = new ProxyCatalogService(UserContext, this.JWTToken);
                            SearchResult searchResult = proxyCatalogService.GetLineItemsSearch(itemSearchInput);
                            List<CatalogItem> Items = searchResult.Items;
                            foreach (var objCatalogItems in Items)
                            {
                                decimal unitPrice = 0;
                                if ((objCatalogItems.Id > 0))
                                {
                                    if (objCatalogItems.EffectivePrice == null)
                                    {
                                        unitPrice = (decimal)objCatalogItems.UnitPrice;
                                    }
                                    else
                                    {
                                        unitPrice = (decimal)objCatalogItems.EffectivePrice;
                                    }
                                    if (!catlogItems.Contains(new KeyValuePair<long, decimal>(objCatalogItems.Id, unitPrice)))
                                        catlogItems.Add(new KeyValuePair<long, decimal>(objCatalogItems.Id, unitPrice));

                                }
                            }
                        }
                    }
                }
                if(AllowMultiCurrencyInRequisition)
                {
                    List<CurrencyExchageRate> currencyExchageRates = GetReqDao().GetRequisitionCurrency(SourceRequisitionId.ToString());
                    if(currencyExchageRates!=null && currencyExchageRates.Count>0)
                    {
                        foreach(var currencyExchageRate in currencyExchageRates)
                        {
                            CurrencyExchageRate objCurrencyExchageRate = new CurrencyExchageRate();
                            decimal ExchangeRate = commonManager.GetCurrencyConversionRate(currencyExchageRate.FromCurrencyCode,currencyExchageRate.ToCurrencyCode);
                            objCurrencyExchageRate.FromCurrencyCode = currencyExchageRate.FromCurrencyCode;
                            objCurrencyExchageRate.ToCurrencyCode = currencyExchageRate.ToCurrencyCode;
                            objCurrencyExchageRate.ExchangeRate = ExchangeRate;
                            lstCurrencyExchageRates.Add(objCurrencyExchageRate);
                        }
                    }

                }
                int documentId = GetReqDao().CopyRequisition(SourceRequisitionId, DestinationRequisitionId, ContactCode, PrecessionValue, precessionValueForTotal, precessionValueForTaxesAndCharges, out eventPerformed, isAccountChecked, isCommentsChecked, isNotesAndAttachmentChecked, isAddNonCatalogItems, isCheckReqUpdate, IsCopyEntireReq, isNewNotesAndAttachmentChecked, contractDocumentStatuses, showTaxJurisdictionForShipTo.ToLower()=="yes"?true:false, catlogItems,itemMasteritems, lstCurrencyExchageRates);

                if (documentId > 0)
                {
                    #region Saving default accounting
                    if (!IsCopyEntireReq || (IsCopyEntireReq && !isAccountChecked))
                    {
                        var documentSplitItemEntities = objP2PDocumentManager.GetDocumentDefaultAccountingDetails(P2PDocumentType.Requisition, LevelType.ItemLevel, UserContext.ContactCode, documentId);

                        GetReqDao().SaveDefaultAccountingDetails(documentId, documentSplitItemEntities, precessionValue, precessionValueForTotal, precessionValueForTaxesAndCharges, true);
                    }
          #endregion

                    
                    AddIntoSearchIndexerQueueing(documentId, (int)DocumentType.Requisition);
                }
                return eventPerformed;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occurred in CopyRequisition method in RequistionManager.", ex);
                throw ex;

            }

        }


        public bool CheckBiddingInProgress(long documentId)
        {
            return GetReqDao().CheckBiddingInProgress(documentId);
        }

        public long CancelChangeRequisition(long documentCode, long userId, int requisitionSource)
        {
            long returnValue = GetReqDao().CancelChangeRequisition(documentCode, userId, requisitionSource);
            if (this.UserContext.Product != GEPSuite.eInterface)
                AddIntoSearchIndexerQueueing(documentCode, (int)DocumentType.Requisition);
            AddIntoSearchIndexerQueueing(returnValue, (int)DocumentType.Requisition);

            return returnValue;
        }
        public DataSet GetBuyerAssigneeDetails(long ContactCode, string SearchText, int StartIndex, int Size)
        {
            return GetReqDao().GetBuyerAssigneeDetails(ContactCode, SearchText, StartIndex, Size);
        }

        public long GetOBOUserByRequisitionID(long documentCode)
        {
            return GetReqDao().GetCreatorById(documentCode);
        }
        public Dictionary<string, int> GetRequisitionPunchoutItemCount(long RequisitionId)
        {
            return GetReqDao().GetRequisitionPunchoutItemCount(RequisitionId);
        }

        public void SendReviewedRequisitionForApproval(Requisition requisition)
        {
            var currentPrincipal = System.Threading.Thread.CurrentPrincipal;
            Task.Factory.StartNew((_userContext) =>
            {
                System.Threading.Thread.CurrentPrincipal = currentPrincipal;
                string strReturnVal = string.Empty;
                string toCurrency = GetDefaultCurrency();
                string fromCurrency = string.IsNullOrEmpty(requisition.Currency) ? GetDefaultCurrency() : requisition.Currency;
                UserExecutionContext ctx = (UserExecutionContext)_userContext;
                RequisitionCommonManager commonManager = new RequisitionCommonManager(JWTToken) { UserContext = ctx, GepConfiguration = GepConfiguration };
                RequisitionDocumentManager documentManager = new RequisitionDocumentManager(JWTToken) { UserContext = ctx, GepConfiguration = GepConfiguration };
                documentManager.UpdateDocumentStatus(P2PDocumentType.Requisition, requisition.DocumentCode, DocumentStatus.ApprovalPending, 0);
                long LOBId = GetCommonDao().GetLOBByDocumentCode(requisition.DocumentCode);
                bool isOperationalBudgetEnabled = Convert.ToBoolean(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "IsOperationalBudgetEnabled", commonManager.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBId));

                try
                {
                    var approverContactCode = requisition.OnBehalfOf > 0 ? requisition.OnBehalfOf : ctx.ContactCode;
                    strReturnVal = SendRequisitionForApproval(approverContactCode, requisition.DocumentCode, (decimal)requisition.TotalAmount, (int)DocumentType.Requisition, fromCurrency, toCurrency, isOperationalBudgetEnabled, 0, _userContext, LOBId);
                }
                catch (Exception ex)
                {
                    LogHelper.LogError(Log, "Error occured in SendReviewedRequisitionForApproval Method in RequisitionManager Error:" + ex.Message, ex);
                    strReturnVal = string.Empty;
                }
                finally
                {
                    if (string.IsNullOrEmpty(strReturnVal.Trim()))
                    {
                        //Set DocumentStatus as Failed
                        var objDocManager = new RequisitionDocumentManager(JWTToken) { UserContext = ctx, GepConfiguration = GepConfiguration };
                        documentManager.UpdateDocumentStatus(P2PDocumentType.Requisition, requisition.DocumentCode, DocumentStatus.SendForApprovalFailed, 0);

                        // Send notification
                        var objEmailNotificationManager = new RequisitionEmailNotificationManager(ctx, this.JWTToken)
                        {
                            UserContext = ctx,
                            GepConfiguration = GepConfiguration
                        };

                        objEmailNotificationManager.SendFailureNotificaiton(requisition.DocumentCode, DocumentType.Requisition, FailureAction.Approval);
                    }
                }
            }, this.UserContext);
        }

        //public List<long> GetRequisitionListForInterfaces(string docType, int docCount, int sourceSystemId)
        //{
        //    return GetReqDao().GetRequisitionListForInterfaces(docType, docCount, sourceSystemId);
        //}

        //public DataSet ValidateInterfaceLineStatus(long buyerPartnerCode, DataTable dtRequisitionDetail)
        //{
        //    return GetReqDao().ValidateInterfaceLineStatus(buyerPartnerCode, dtRequisitionDetail);
        //}

        public void UpdateRequisitionLineStatus(long requisitionId, long buyerPartnerCode)
        {
            bool result;
            RequisitionLineStatusUpdateDetails reqDetails = new RequisitionLineStatusUpdateDetails();
            result = GetReqDao().UpdateRequisitionStatusForStockReservation(requisitionId);

            if (result && requisitionId > 0)
                AddIntoSearchIndexerQueueing(requisitionId, (int)DocumentType.Requisition);

            if (result)
            {
                reqDetails = GetReqDao().UpdateRequisitionNotificationDetails(requisitionId);
                var emailNotificationManager = new RequisitionEmailNotificationManager(this.UserContext, this.JWTToken)
                {
                    UserContext = this.UserContext,
                    GepConfiguration = this.GepConfiguration
                };
                emailNotificationManager.SendNotificationForLineStatusUpdate(reqDetails);
            }
        }

        public bool PushingDataToEventHub(long Documentcode)
        {
            try
            {
                RequisitionCommonManager commonManager = new RequisitionCommonManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };

                string attributesToBeUsedForPurchaseOrderConsolidation = (commonManager.GetSettingsValueByKey(P2PDocumentType.Requisition, "AttributesToBeUsedForPurchaseOrderConsolidation", UserContext.ContactCode, (int)SubAppCodes.P2P, ""));
                if (!(string.IsNullOrEmpty(attributesToBeUsedForPurchaseOrderConsolidation)))
                {
                    PushingRequisitionToEventHub(Documentcode, attributesToBeUsedForPurchaseOrderConsolidation, UserContext.ContactCode);
                }
                return true;
            }
            catch (Exception ex)
            {
                // Log Exception here
                LogHelper.LogError(Log, "Error occurred in PushingDataToEventHub method in RequisitionManager.", ex);
                throw;
            }
        }

        public void CreateHttpWebRequest(string strURL, UserExecutionContext userExecutionContext)
        {
            req = WebRequest.Create(strURL) as HttpWebRequest;
            req.Method = "POST";
            req.ContentType = @"application/json";

            NameValueCollection nameValueCollection = new NameValueCollection();
            userExecutionContext.UserName = "";
            string userContextJson = userExecutionContext.ToJSON();
            nameValueCollection.Add("UserExecutionContext", userContextJson);
            string strSubscriptionKey = MultiRegionConfig.GetConfig(CloudConfig.APIMSubscriptionKey);
            nameValueCollection.Add("Ocp-Apim-Subscription-Key", strSubscriptionKey);
            nameValueCollection.Add("RegionId", MultiRegionConfig.GetConfig(CloudConfig.PrimaryRegion));
            nameValueCollection.Add("BPC", userExecutionContext.BuyerPartnerCode.ToString());
            nameValueCollection.Add("Authorization", this.JWTToken);
            req.Headers.Add(nameValueCollection);
        }

        private string GetHttpWebResponse(EventRequest odict)
        {
            JavaScriptSerializer JSrz = new JavaScriptSerializer();
            var data = JSrz.Serialize(odict);
            var byteData = Encoding.UTF8.GetBytes(data);


            req.ContentLength = byteData.Length;
            using (Stream stream = req.GetRequestStream())
            {
                stream.Write(byteData, 0, byteData.Length);
            }

            string result = null;
            using (HttpWebResponse resp = req.GetResponse() as HttpWebResponse)
            {
                using (StreamReader reader = new StreamReader(resp.GetResponseStream()))
                {
                    result = reader.ReadToEnd();
                }
            }
            return result;
        }

        public RequisitionItemCustomAttributes GetCutsomAttributesForLines(List<long> itemIds, int sourceDocType, int targetDocType, string level)
        {
            return GetReqDao().GetCutsomAttributesForLines(itemIds, sourceDocType, targetDocType, level);
        }

        public long SaveChangeRequisitionRequest(int requisitionSource, long documentCode, string documentName, string documentNumber, DocumentSourceType documentSourceType = DocumentSourceType.None, string revisionNumber = "", bool isCreatedFromInterface = false, bool byPassAccesRights = false, bool isFunctionalAdmin = false, bool documentActive = false)
        {
            long newDocumentCode = 0;
            var commonManager = new RequisitionCommonManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
            int PrecessionValue, Precessiontotal, MaxPrecessionValueForTaxAndCharges;
            var settingDetails = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.None, UserContext.ContactCode, (int)SubAppCodes.P2P);
            PrecessionValue = Convert.ToInt16(commonManager.GetSettingsValueByKey(settingDetails, "MaxPrecessionValue"));
            Precessiontotal = Convert.ToInt16(commonManager.GetSettingsValueByKey(settingDetails, "MaxPrecessionValueforTotal"));
            MaxPrecessionValueForTaxAndCharges = Convert.ToInt16(commonManager.GetSettingsValueByKey(settingDetails, "MaxPrecessionValueForTaxesAndCharges"));
            try
            {
                if (documentCode > 0)
                {
                    string ReqRevisionNumber = GetReqDao().GetRequisitionRevisionNumberByDocumentCode(documentCode);
                    if (requisitionSource == (int)RequisitionSource.ChangeRequisition)
                    {
                        string currentRevNo = ReqRevisionNumber == "" ? "000" : ReqRevisionNumber;

                        if (documentSourceType == DocumentSourceType.Interface && !string.IsNullOrWhiteSpace(revisionNumber))
                        {
                            revisionNumber = Convert.ToInt32(revisionNumber).ToString("000", CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            int incrementVersion = Convert.ToInt32(currentRevNo, CultureInfo.InvariantCulture) + 1;
                            revisionNumber = incrementVersion.ToString("000", CultureInfo.InvariantCulture);
                        }
                        if (currentRevNo != "000" && !string.IsNullOrEmpty(documentNumber))
                        {
                            var docNumSplit = documentNumber.Split('-');
                            if (docNumSplit.Length > 1)
                                documentNumber = string.Join("-", docNumSplit, 0, docNumSplit.Length - 1);
                        }
                        documentNumber = documentNumber + "-" + revisionNumber;
                    }
                    else
                    {
                        if (revisionNumber == "")
                            revisionNumber = ReqRevisionNumber;
                    }

                    newDocumentCode = GetReqDao().SaveChangeRequisitionRequest(requisitionSource, documentCode, documentName, documentNumber, documentSourceType, revisionNumber, isCreatedFromInterface, byPassAccesRights, PrecessionValue, Precessiontotal, MaxPrecessionValueForTaxAndCharges, isFunctionalAdmin, documentActive);
                    if (newDocumentCode > 0)
                    {
                        
                        List<Level> levels = new List<Level>() { Level.Header, Level.Item, Level.Distribution };
                        commonManager.FlipCustomFields(documentCode, (int)DocumentType.Requisition, newDocumentCode, (int)DocumentType.Requisition, levels);

                        var RequisitionDocumentManager  = new RequisitionDocumentManager(JWTToken)
                        {
                            UserContext = this.UserContext,
                            GepConfiguration = this.GepConfiguration
                        };
                        object changeRequisitionEventData = new
                        {
                            message = "ChangeRequisition",
                            data = new { documentCode = newDocumentCode }
                        };

                        RequisitionDocumentManager.BroadcastPusher(documentCode, DocumentType.Requisition, "DataStale", changeRequisitionEventData);
                    }

                    var currentPrincipal = System.Threading.Thread.CurrentPrincipal;
                    Task.Factory.StartNew((_userContext) =>
                    {
                        System.Threading.Thread.CurrentPrincipal = currentPrincipal;
                        this.UserContext = (UserExecutionContext)_userContext;

                        if (this.UserContext.Product != GEPSuite.eInterface)
                            AddIntoSearchIndexerQueueing(documentCode, (int)DocumentType.Requisition);
                        if (newDocumentCode > 0)
                        {
                            if (this.UserContext.Product != GEPSuite.eInterface)
                                AddIntoSearchIndexerQueueing(newDocumentCode, (int)DocumentType.Requisition);
                        }

                    }, this.UserContext);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally { }

            return newDocumentCode;
        }

        public void PushingRequisitionToEventHub(long requisitionId, string consolidationAttributes, long contactCode)
        {
            UserExecutionContext userExecutionContext = this.UserContext;
            var newReqManager = new GEP.Cumulus.P2P.Req.BusinessObjects.NewRequisitionManager(this.JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
            try
            {
                LogHelper.LogError(Log, string.Concat("PushingRequisitionToEventHub : PushingRequisitionToEventHub in RequisitionManager Started At Requisition : " + requisitionId), new Exception { });
                var objReq = newReqManager.GetRequisitionDisplayDetails(requisitionId);
                objReq = newReqManager.GetDefaultEntities(objReq);

                string[] purchaseTypeDetails = consolidationAttributes.Split(',');
                if (purchaseTypeDetails.Contains(objReq.purchaseTypeDesc))
                {
                    EventRequest objRequest = new EventRequest();
                    objRequest.MessageHeader.BuyerPartnerCode = userExecutionContext.BuyerPartnerCode;
                    objRequest.MessageHeader.MessageSenderContactCode = contactCode;
                    objRequest.MessageHeader.MessageType = "Requisition";
                    objRequest.MessageHeader.MessageSubType = "Requisition";
                    objRequest.MessageHeader.ProductVersion = "2.0";
                    objRequest.MessageHeader.JWToken = "Token1";
                    objRequest.MessageHeader.SourceSystem = "InterFace";
                    objRequest.AdditionalParams.Add("UserContext", userExecutionContext);
                    objRequest.MessageRequest = Convert.ToString(objReq.ToJSON());
                    objRequest.MessageHeader.MessageId = Guid.NewGuid();
                    objRequest.RegionId = MultiRegionConfig.GetConfig(CloudConfig.PrimaryRegion);

                    var serviceurl = MultiRegionConfig.GetConfig(CloudConfig.APIMBaseURL) + MultiRegionConfig.GetConfig(CloudConfig.ESIntegratorURL);
                    CreateHttpWebRequest(serviceurl, userExecutionContext);
                    var result = GetHttpWebResponse(objRequest);
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, string.Concat("Exception Program PushingRequisitionToEventHub with parameters in RequistionRestService :",
                                                    ", consolidationAttributes = " + consolidationAttributes + ", documentCode " + requisitionId + " Contact Code :" + userExecutionContext.ContactCode), ex);
                throw;
            }
        }
        public RiskFormDetails GetRiskFormQuestionScore()
        {
            return GetReqDao().GetRiskFormQuestionScore();
        }
        public RiskFormDetails GetRiskFormHeaderInstructionsText()
        {
            return GetReqDao().GetRiskFormHeaderInstructionsText();
        }
        #region Interface functions        
        
        public bool DeleteDocumentByDocumentCode(P2PDocumentType docType, long documentCode)
        {
            bool result = GetReqDao().DeleteDocumentByDocumentCode(documentCode);
            AddIntoSearchIndexerQueueing(documentCode, (int)DocumentType.Requisition);
            return result;
        }
        #endregion

        //public DataTable ValidateReqItemsForExceptionHandling(DataTable dtItemDetails)
        //{
        //    return GetReqDao().ValidateReqItemsForExceptionHandling(dtItemDetails);
        //}
        public Requisition GetRequisitionPartialDetailsById(long documentCode)
        {
            return GetReqDao().GetRequisitionPartialDetailsById(documentCode);
        }

        public DocumentIntegration.Entities.DocumentIntegrationEntity GetDocumentDetailsBySelectedReqWorkbenchItems(string reqItemIds, List<long> partners = null, List<DocumentIntegration.Entities.IntegrationTimelines> timelines = null, bool sendTeammembersForQuickQuote = false, bool sendAdditionalColumnsForSendForBidding = false)

        {
            try
            {
                DataTable table = new DataTable();
                table.Columns.Add("Id", typeof(long));
                string[] items = reqItemIds.Split(',');

                for (int i = 0; i < items.Length; i++)
                {
                    table.Rows.Add(new object[] { items[i] });
                }
                List<long> teammemberList = new List<long>();
                Dictionary<long, long> reqItems = new Dictionary<long, long>();
                if (sendTeammembersForQuickQuote && table != null && table.Rows.Count > 0)
                {
                    reqItems = GetReqDao().GetRequisitionByRequisitionItems(table);
                    List<long> ReqIds = reqItems.Values.Distinct().ToList();
                    foreach (var reqid in ReqIds)
                    {
                        List<long> reqitemsIds = reqItems.Where(val => val.Value == reqid).Select(key => key.Key).ToList();
                        List<long> contactListList = GetTeammemberList(reqid, reqitemsIds);
                        if (contactListList != null && contactListList.Count > 0)
                            teammemberList.AddRange(contactListList);
                    }
                }

                DocumentIntegration.Entities.DocumentIntegrationEntity integrationEntity = (DocumentIntegration.Entities.DocumentIntegrationEntity)GetReqDao().GetDocumentDetailsBySelectedReqWorkbenchItems(table, partners, timelines, teammemberList);
                return integrationEntity;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error Occured in GetDocumentDetailsBySelectedReqWorkbenchItems method", ex);
                throw;
            }
        }

        public Requisition GetAllRequisitionDetailsByRequisitionId(long requisitionId, long userId, int typeOfUser, List<long> reqLineItemIds = null, Dictionary<string, string> settings = null)
        {
            return GetReqDao().GetAllRequisitionDetailsByRequisitionId(requisitionId, userId, typeOfUser, reqLineItemIds, settings);
        }

        public bool UpdateRequisitionBuyerContactCode(List<KeyValuePair<long, long>> lstReqItemsToUpdate)
        {
            return GetReqDao().UpdateRequisitionBuyerContactCode(lstReqItemsToUpdate);
        }

        public List<DocumentAdditionalEntityInfo> GetAllDocumentAdditionalEntityInfo(long documentCode)
        {
            return GetReqDao().GetAllDocumentAdditionalEntityInfo(documentCode);
        }

        public Requisition GetRequisitionDetailsByReqItems(List<long> reqItemIds)
        {
            return GetReqDao().GetRequisitionDetailsByReqItems(reqItemIds);
        }

        public List<KeyValuePair<long, int>> GetListErrorCodesByOrderIds(List<long> lstDocumentCode, bool isOrderingLocationMandatory)
        {
            return GetReqDao().GetListErrorCodesByOrderIds(lstDocumentCode, isOrderingLocationMandatory);
        }

        public P2PDocument GetDocumentAdditionalEntityDetailsById(long documentCode)
        {
            return GetReqDao().GetDocumentAdditionalEntityDetailsById(documentCode);
        }

        public List<ItemCharge> GetLineItemCharges(List<long> reqItemIds, long documentCode)
        {
            return GetReqDao().GetLineItemCharges(reqItemIds, documentCode);
        }

        public List<NewP2PEntities.DocumentInfo> GetOrdersListForWorkBench(string reqItemIds)
        {
            return GetReqDao().GetOrdersListForWorkBench(reqItemIds);
        }

        public ICollection<DocumentItemShippingDetail> GetShippingSplitDetailsByLiId(long lineItemId)
        {
            return GetReqDao().GetShippingSplitDetailsByLiId(lineItemId);
        }
        public bool UpdateRequisitionItemAutoSourceProcessFlag(string itemIds, int status)
        {
            return GetReqDao().UpdateRequisitionItemAutoSourceProcessFlag(itemIds, status);
        }

        public ICollection<NewP2PEntities.PartnerSpendControlDocumentMapping> GetAllPartnerCodeOrderinglocationIdNadSpendControlItemId(long documentId)
        {
            return GetReqDao().GetAllPartnerCodeOrderinglocationIdNadSpendControlItemId(documentId);
        }

        public ICollection<KeyValuePair<long, long>> GetAllPartnerCodeAndOrderinglocationId(long documentId)
        {
            return GetReqDao().GetAllPartnerCodeAndOrderinglocationId(documentId);
        }


        public long SyncChangeRequisition(long documentCode)
        {
            return GetReqDao().SyncChangeRequisition(documentCode);
        }
        public ItemSearchInput GetCatalogLineDetails(long documentId)
        {
            return GetReqDao().GetCatalogLineDetails(documentId);

        }
        public ItemBulkInputRequest GetCatalogLineDetailsForWebAPI(long documentId)
        {
          return GetReqDao().GetCatalogLineDetailsForWebAPI(documentId);

        }
        public ItemSearchBulkInput GetCatalogLineDetailsForBulkWebAPI(long documentId, string requisitionIds = "")
        {
            return GetReqDao().GetCatalogLineDetailsForBulkWebAPI(documentId, requisitionIds);

        }
        public int UpdateCatalogLineDetails(List<CatalogItem> Items, long documentId)
        {
            return GetReqDao().UpdateCatalogLineDetails(Items, documentId);

        }
        public string ConsumeCapitalBudget(long documentCode,long LOBId,out bool consumption)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            consumption = false;
            var commonManager = new RequisitionCommonManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
            string enableCapitalBudgetConsumptionSetting = commonManager.GetSettingsValueByKey(P2PDocumentType.Requisition, "CapitalBudgetConsumption", UserContext.ContactCode, 107, "", LOBId);
            NewP2PEntities.ConsumeCapitalBudget capitalBudgetConsumption = NewP2PEntities.ConsumeCapitalBudget.None;
            if (!(string.IsNullOrEmpty(enableCapitalBudgetConsumptionSetting)))
            {
                capitalBudgetConsumption = (NewP2PEntities.ConsumeCapitalBudget)Convert.ToInt16(enableCapitalBudgetConsumptionSetting);
            }
            if (capitalBudgetConsumption != NewP2PEntities.ConsumeCapitalBudget.None)
            {
                Document objDocument = GetDocumentDao().GetDocumentBasicDetails(documentCode);
                var documentStatus = objDocument.DocumentStatusInfo;


                if ((documentStatus == DocumentStatus.Draft || documentStatus == DocumentStatus.Withdrawn || documentStatus == DocumentStatus.Rejected) && (capitalBudgetConsumption == NewP2PEntities.ConsumeCapitalBudget.ALL
                    || capitalBudgetConsumption == NewP2PEntities.ConsumeCapitalBudget.OnSubmit
                    || capitalBudgetConsumption == NewP2PEntities.ConsumeCapitalBudget.OnSubmitLastApproverApprove
                    || capitalBudgetConsumption == NewP2PEntities.ConsumeCapitalBudget.OnSubmitLastReviewerAccept))
                {
                    CapitalBudgetManager capitalBudgetManager = new CapitalBudgetManager(this.JWTToken) { GepConfiguration = this.GepConfiguration, UserContext = this.UserContext };
                    var validateCapitalBudget = capitalBudgetManager.ConsumeCapitalBudget(documentCode, false);
                    consumption = true;
                    if (!validateCapitalBudget)
                        return "P2P_Req_CapitalBudgetValidation";

                }
            }
            return "";
        }

        public bool ReleaseCapitalBudget(long documentCode)
        {
            CapitalBudgetManager capitalBudgetManager = new CapitalBudgetManager(this.JWTToken) { GepConfiguration = this.GepConfiguration, UserContext = this.UserContext };
            return capitalBudgetManager.ReleaseBudget(documentCode,0);
        }
        public bool UpdateRequsitionItemStatus(KeyValuePair<long, long> requisitionStatus, List<KeyValuePair<long, long>> requisitionItemStatus)
        {
            var documentManager = new RequisitionDocumentManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
            documentManager.UpdateDocumentStatus(P2PDocumentType.Requisition, requisitionStatus.Key, (DocumentStatus)(requisitionStatus.Value), 0);
            var result= GetReqDao().UpdateRequsitionItemStatus(requisitionItemStatus);
            AddIntoSearchIndexerQueueing(requisitionStatus.Key, (int)DocumentType.Requisition);
            return result;
        }

        public Dictionary<string, string> GetRequisitionDetailsForExternalWorkFlowProcess(long requisitionId)
        {
            Dictionary<string, string> result = GetReqDao().GetRequisitionDetailsForExternalWorkFlowProcess(requisitionId);
            return result;
        }

        public bool UpdateRequisitionExtendedStatus(long requisitionId,string ErrorMsg, int updatededExtendedStatus)
        {
            return GetReqDao().UpdateRequisitionExtendedStatus(requisitionId, ErrorMsg, updatededExtendedStatus);
            
        }
        public List<long> GetTeammemberList(long documentCode, List<long> reqLineItemIds = null)
        {
            var documentManager = new RequisitionDocumentManager(JWTToken) { UserContext = UserContext, GepConfiguration = GepConfiguration };
            List<long> teamemeberList = documentManager.GetTeammemberList(documentCode, reqLineItemIds);
            return teamemeberList;
        }
        
        private void LogNewRelicAppForPayload(long buyerPartnerCode, string url, string data, string useCase)
        {
            var eventAttributes = new Dictionary<string, object>();
            eventAttributes.Add("URL", url);
            eventAttributes.Add("BuyerPartnerCode", buyerPartnerCode.ToString());
            eventAttributes.Add("Payload", data);
            eventAttributes.Add("UseCase", useCase);
            NewRelic.Api.Agent.NewRelic.RecordCustomEvent("RequisitionManager_Payloads", eventAttributes);
        }

    }
}
