using GEP.Cumulus.Caching;
using GEP.Cumulus.Logging;
using GEP.Cumulus.P2P.BusinessEntities;
using GEP.Cumulus.P2P.Req.DataAccessObjects;
using GEP.NewP2PEntities;
using GEP.SMART.Configuration;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Script.Serialization;
using Gep.Cumulus.CSM.Entities;

namespace GEP.Cumulus.P2P.Req.BusinessObjects
{
    public class CapitalBudgetManager : RequisitionBaseBO
    {
        private List<Entity> lstEntities = new List<Entity>();
        private static readonly ILog Log = Logger.GetLog(MethodBase.GetCurrentMethod().DeclaringType);
        private P2P.BusinessEntities.Requisition req = new P2P.BusinessEntities.Requisition();

        public CapitalBudgetManager(string jwtToken, UserExecutionContext context = null) : base(jwtToken)
        {
            if (context != null)
            {
                this.UserContext = context;
            }
        }

        private List<Entity> GetMasterEntities()
        {

            List<Entity> EntityNames = new List<Entity>();
            EntityNames = GEPDataCache.GetFromCacheJSON<List<Entity>>("GetMasterEntities",
                        UserContext.BuyerPartnerCode, UserContext.ContactCode, "en-US");
            if (EntityNames == null)
            {
                string getMasterEntitiesAPIURL = MultiRegionConfig.GetConfig(CloudConfig.APIMBaseURL) + URLs.GetMasterEntities;

                var requestHeaders = new RESTAPIHelper.RequestHeaders();
                requestHeaders.Set(this.UserContext, JWTToken);
                var webapi = new RESTAPIHelper.WebAPI(requestHeaders, "DEV_APAC_Dss_Req_Microservice_NEWREQ", "GetMasterEntities");
                var JsonResult = webapi.ExecuteGet(getMasterEntitiesAPIURL);

                var jsonSerializer = new JavaScriptSerializer();
                EntityNames = jsonSerializer.Deserialize<List<Entity>>(JsonResult);
                GEPDataCache.PutInCacheJSON<List<Entity>>("GetMasterEntities",
                            UserContext.BuyerPartnerCode, UserContext.ContactCode, "en-US", EntityNames);
            }
            return EntityNames;
        }

        public Boolean ReleaseBudget(long documentCode, long parentDocumentCode = 0)
        {
            BudgetRequest BudgetRequest = new BudgetRequest();
            BudgetResponse budgetResponse = new BudgetResponse();
            try
            {
                string getReleaseBudgetAPIURL = MultiRegionConfig.GetConfig(CloudConfig.APIMBaseURL) + URLs.ReleaseBudget;
                BudgetRequest.DocumentCode = documentCode;
                var req = GetRequisitionDetailsForCapitalBudget(documentCode);
                BudgetRequest.ParentDocumentCode = req.ParentDocumentCode;
                BudgetRequest.DocumentType = "Requisition";

                var requestHeaders = new RESTAPIHelper.RequestHeaders();
                requestHeaders.Set(UserContext, JWTToken);
                var webAPI = new RESTAPIHelper.WebAPI(requestHeaders, "DEV_APAC_Dss_Req_Microservice_NEWREQ", "AutoSourcingManager-FillOrderFromRequisition");
                var JsonResult = webAPI.ExecutePost(getReleaseBudgetAPIURL, BudgetRequest);

                var jsonSerializer = new JavaScriptSerializer();
                budgetResponse = jsonSerializer.Deserialize<BudgetResponse>(JsonResult);
                if (!budgetResponse.IsSuccess)
                    UpdateDocumentBudgetoryStatus(documentCode, (byte)NewP2PEntities.CapitalBudgetStatus.ReleaseBudgetFailed);
                return budgetResponse.IsSuccess;
            }
            catch (Exception ex)
            {
                saveNewrelicErrors("ReleaseBudget", "ReleaseBudgetError", ex.Message);
                throw;
            }
            finally
            {
                logNewRelicEvents("ReleaseBudget", "BudgetRequest", JsonConvert.SerializeObject(BudgetRequest));

                logNewRelicEvents("ReleaseBudget", "BudgetResponse", JsonConvert.SerializeObject(budgetResponse));
            }
        }

        public bool UpdateDocumentStatusInBudget(long documentCode, string documentStatus)
        {
            bool response = false;
            try
            {
                string getUpdateDocumentStatusInBudgetAPIURL = MultiRegionConfig.GetConfig(CloudConfig.APIMBaseURL) + URLs.UpdateConsumingDocumentStatusInBudget;

                var requestHeaders = new RESTAPIHelper.RequestHeaders();
                requestHeaders.Set(UserContext, JWTToken);
                var webAPI = new RESTAPIHelper.WebAPI(requestHeaders, "DEV_APAC_Dss_Req_Microservice_NEWREQ", "AutoSourcingManager-FillOrderFromRequisition");
                var JsonResult = webAPI.ExecutePost(getUpdateDocumentStatusInBudgetAPIURL, new { DocumentCode = documentCode, DocumentStatus = documentStatus });

                var jsonSerializer = new JavaScriptSerializer();
                response = jsonSerializer.Deserialize<bool>(JsonResult);
                return response;
            }
            catch (Exception ex)
            {
                saveNewrelicErrors("UpdateDocumentStatusInBudget", "UpdateDocumentStatusInBudget", ex.Message);
                throw;
            }
            finally
            {
                logNewRelicEvents("UpdateDocumentStatusInBudget", "BudgetRequest", JsonConvert.SerializeObject(new { DocumentCode = documentCode, DocumentStatus = documentStatus }));

                logNewRelicEvents("UpdateDocumentStatusInBudget", "BudgetResponse", JsonConvert.SerializeObject(new { DocumentCode = documentCode, DocumentStatus = documentStatus }));
            }
        }

        private BudgetCheckResponse ConsumeBudget(BudgetCheckRequest budgetCheckRequest)
        {
            string getConsumeBudgetAPIURL = MultiRegionConfig.GetConfig(CloudConfig.APIMBaseURL) + URLs.ConsumeBudget;

            var requestHeaders = new RESTAPIHelper.RequestHeaders();
            requestHeaders.Set(UserContext, JWTToken);
            var webAPI = new RESTAPIHelper.WebAPI(requestHeaders, "DEV_APAC_Dss_Req_Microservice_NEWREQ", "CapitalBudgetManager-ConsumeBudget");
            var JsonResult = webAPI.ExecutePost(getConsumeBudgetAPIURL, budgetCheckRequest);

            var jsonSerializer = new JavaScriptSerializer();
            BudgetCheckResponse BudgetCheckResponse = jsonSerializer.Deserialize<BudgetCheckResponse>(JsonResult);
            return BudgetCheckResponse;
        }

        private BudgetCheckResponse CheckBudget(BudgetCheckRequest budgetCheckRequest)
        {

            string getCheckBudgetAPIURL = MultiRegionConfig.GetConfig(CloudConfig.APIMBaseURL) + URLs.CheckBudget;

            var requestHeaders = new RESTAPIHelper.RequestHeaders();
            requestHeaders.Set(UserContext, JWTToken);
            var webAPI = new RESTAPIHelper.WebAPI(requestHeaders, "DEV_APAC_Dss_Req_Microservice_NEWREQ", "CapitalBudgetManager-CheckBudget");
            var JsonResult = webAPI.ExecutePost(getCheckBudgetAPIURL, budgetCheckRequest);

            var jsonSerializer = new JavaScriptSerializer();
            BudgetCheckResponse BudgetCheckResponse = jsonSerializer.Deserialize<BudgetCheckResponse>(JsonResult);
            return BudgetCheckResponse;
        }

        public int UpdateDocumentBudgetoryStatus(long documentCode, Int16 budgetoryStatus, List<BudgetAllocationDetails> lstbudgetAllocationIds = null)
        {
            return GetNewReqDao().UpdateDocumentBudgetoryStatus(documentCode, budgetoryStatus, lstbudgetAllocationIds);
        }

        private P2P.BusinessEntities.Requisition GetRequisitionDetailsForCapitalBudget(long documentCode)
        {
            return GetReqDao().GetRequisitionDetailsForCapitalBudget(documentCode);
        }

        private BudgetCheckRequest PrepareBudgetCheckObject(long documentCode)
        {
            logNewRelicEvents("CapitalBudgetEntities", "CapitalBudgetEntities", "Started");
            lstEntities = GetMasterEntities();
            logNewRelicEvents("CapitalBudgetEntities", "CapitalBudgetEntities", JsonConvert.SerializeObject(lstEntities));
            req = GetRequisitionDetailsForCapitalBudget(documentCode);
            BudgetCheckRequest budgetCheckRequest = new BudgetCheckRequest();
            budgetCheckRequest.DocumentCode = documentCode;
            budgetCheckRequest.DocumentStatus = req.DocumentStatusInfo.ToString();
            budgetCheckRequest.DocumentProcessDate = req.CreatedOn;
            budgetCheckRequest.DocumentNumber = req.DocumentNumber;
            budgetCheckRequest.DocumentType = "Requisition";
            budgetCheckRequest.ParentDocumentCode = req.ParentDocumentCode;
            budgetCheckRequest.CurrencyCode = req.Currency;
            budgetCheckRequest.LineInfos = new List<LineInfo>();
            LineInfo lineInfo = new LineInfo();
            BudgetCheckSplit budgetCheckSplit = new BudgetCheckSplit();

            List<LineEntity> LineEntity = new List<LineEntity>();
            List<LineEntity> lstHeaderEntities = prepareBudgetEntities(req, 1, lstEntities);
            foreach (var item in req.RequisitionItems)
            {
                LineEntity = new List<LineEntity>();
                if (lstHeaderEntities.Count > 0)
                    LineEntity.AddRange(lstHeaderEntities);
                lineInfo = new LineInfo();
                lineInfo.LineItemId = item.P2PLineItemId;
                lineInfo.LineNumber = item.ItemLineNumber;
                //lineInfo.ProcessingDate = (DateTime)(item.ItemType == ItemType.Material ? item.DateNeeded : item.StartDate);
                lineInfo.StartDate = (DateTime)(item.ItemType == ItemType.Material ? item.DateNeeded : item.StartDate);
                lineInfo.EndDate = (DateTime)(item.ItemType == ItemType.Material ? item.DateNeeded : item.EndDate);
                lineInfo.Quantity = item.Quantity;
                lineInfo.UnitPrice = (decimal)item.UnitPrice;
                lineInfo.LineTotal = (decimal)item.ItemTotalAmount;
                lineInfo.ItemType = item.ItemType == ItemType.Material ? NewP2PEntities.BudgetItemType.Material : NewP2PEntities.BudgetItemType.Service;
                lineInfo.Splits = new List<BudgetCheckSplit>();
                List<LineEntity> lstLineEntities = prepareBudgetEntities(item, 2, lstEntities);
                if (lstLineEntities.Count > 0)
                    LineEntity.AddRange(lstLineEntities);
                foreach (var split in item.ItemSplitsDetail)
                {
                    budgetCheckSplit = new BudgetCheckSplit();
                    budgetCheckSplit.SplitId = split.DocumentSplitItemId;
                    if (item.ItemExtendedType == ItemExtendedType.Fixed && split.SplitType == SplitType.Percentage)
                    {
                        budgetCheckSplit.SplitQuantity = Math.Round(((item.Quantity * split.Percentage) / 100), 5);
                    }
                    else { budgetCheckSplit.SplitQuantity = split.Quantity; }
                    budgetCheckSplit.SplitAmount = (decimal)split.SplitItemTotal;
                    List<LineEntity> lstsplitEntities = prepareBudgetEntities(split, 3, lstEntities);
                    if (lstsplitEntities.Count > 0)
                        LineEntity.AddRange(lstsplitEntities);
                    budgetCheckSplit.Entities = (LineEntity);
                    lineInfo.Splits.Add(budgetCheckSplit);
                }
                budgetCheckRequest.LineInfos.Add(lineInfo);
            }

            logNewRelicEvents("CapitalBudgetreq", "CapitalBudgetreq", JsonConvert.SerializeObject(budgetCheckRequest));

            return budgetCheckRequest;
        }

        private List<LineEntity> prepareBudgetEntities(dynamic objectData, int type, List<Entity> lstMasterEntity)
        {
            List<LineEntity> lstLineEntity = new List<LineEntity>();
            LineEntity lineEntity = new LineEntity();
            switch (type)
            {
                case 1:
                    var lstHeaders = lstMasterEntity.Where(e => e.EntityTypeName == "Header Entity").ToList();
                    if (lstHeaders.Count > 0)
                    {
                        Type typeRequisition = typeof(P2P.BusinessEntities.Requisition);

                        // Loop over properties.
                        foreach (var Entityname in lstHeaders)
                        {
                            foreach (PropertyInfo propertyInfo in typeRequisition.GetProperties())
                            {
                                string name = propertyInfo.Name;
                                if (name.ToLower() == Entityname.Name.ToLower() || name.ToLower().Contains(Entityname.Name.ToLower()))
                                {
                                    var lstcontains = lstLineEntity.Where(e => e.Name == Entityname.Name).ToList();
                                    if (lstcontains.Count == 0)
                                    {
                                        lineEntity = new LineEntity();
                                        object value = propertyInfo.GetValue(objectData, null);
                                        lineEntity.Name = Entityname.Name;
                                        lineEntity.Value = value == null ? "" : value.ToString();
                                        lstLineEntity.Add(lineEntity);
                                    }
                                }
                            }
                        }
                    }
                    break;
                case 2:
                    var lstline = lstMasterEntity.Where(e => e.EntityTypeName == "Line Entity").ToList();
                    if (lstline.Count > 0)
                    {
                        Type typeRequisitionitem = typeof(P2P.BusinessEntities.RequisitionItem);
                        // Loop over properties.
                        foreach (var Entityname in lstline)
                        {
                            foreach (PropertyInfo propertyInfo in typeRequisitionitem.GetProperties())
                            {
                                string name = propertyInfo.Name;
                                if (name.ToLower() == Entityname.Name.ToLower() || name.ToLower().Contains(Entityname.Name.ToLower()))
                                {
                                    var lstcontains = lstLineEntity.Where(e => e.Name == Entityname.Name).ToList();
                                    if (lstcontains.Count == 0)
                                    {
                                        lineEntity = new LineEntity();
                                        object value = propertyInfo.GetValue(objectData, null);
                                        lineEntity.Name = Entityname.Name;
                                        lineEntity.Value = value == null ? "" : value.ToString();
                                        lstLineEntity.Add(lineEntity);
                                    }
                                }
                            }
                        }
                    }
                    break;
                case 3:
                    var lstsplit = lstMasterEntity.Where(e => e.EntityTypeName == "Accounting Entity").ToList();
                    if (lstsplit.Count > 0)
                    {
                        Type typeRequisitionitem = typeof(RequisitionSplitItems);
                        // Loop over properties.
                        foreach (var Entityname in lstsplit)
                        {
                            foreach (var split in objectData.DocumentSplitItemEntities)
                            {

                                if (split.FieldName.ToLower() == Entityname.Name.ToLower())
                                {
                                    var lstsplitcontains = lstLineEntity.Where(e => e.Name == Entityname.Name).ToList();
                                    if (lstsplitcontains.Count == 0)
                                    {
                                        lineEntity = new LineEntity();
                                        lineEntity.Name = Entityname.Name;
                                        lineEntity.Value = split.SplitAccountingFieldValue.ToString();
                                        lstLineEntity.Add(lineEntity);
                                    }
                                }
                            }
                        }
                    }
                    break;
            }
            return lstLineEntity;
        }
        public bool CapitalBudgetValidation(long documentCode, Boolean isReConsume = false)
        {
            logNewRelicEvents("CapitalBudgetValidation", "CapitalBudgetValidation", JsonConvert.SerializeObject(documentCode));
            BudgetCheckResponse budgetCheckResponse = new BudgetCheckResponse();
            try
            {
                var budgetCheckRequest = PrepareBudgetCheckObject(documentCode);
                budgetCheckRequest.IsReConsume = isReConsume;
                if (lstEntities.Count == 0) return true;
                budgetCheckResponse = CheckBudget(budgetCheckRequest);
                logNewRelicEvents("CapitalBudgetValidation", "CapitalBudgetValidationRequest", JsonConvert.SerializeObject(budgetCheckRequest));

                NewP2PEntities.CapitalBudgetStatus status = NewP2PEntities.CapitalBudgetStatus.None;
                if (budgetCheckResponse.IsSuccess)
                {
                    List<BudgetAllocationDetails> lstbudgetAllocationIds = new List<BudgetAllocationDetails>();
                    lstbudgetAllocationIds = mapReqSplititemWithBudgetAllocationID(budgetCheckRequest, budgetCheckResponse);
                    status = (NewP2PEntities.CapitalBudgetStatus)getCapitalBudgetStatus(budgetCheckResponse.DocumentBudgetAction);
                    UpdateDocumentBudgetoryStatus(documentCode, (byte)status, lstbudgetAllocationIds);
                }
                return budgetCheckResponse.IsSuccess && status != NewP2PEntities.CapitalBudgetStatus.Decline ? true : false;
            }
            catch (Exception ex)
            {
                saveNewrelicErrors("CapitalBudgetValidation", "ValidationError", ex.Message);
                throw;
            }
            finally
            {
                logNewRelicEvents("CapitalBudgetValidation", "CapitalBudgetValidationResponse", JsonConvert.SerializeObject(budgetCheckResponse));
            }
        }

        public bool ConsumeCapitalBudget(long documentCode, Boolean isReConsume = false)
        {
            logNewRelicEvents("ConsumeCapitalBudgetRequest", "ConsumeCapitalBudgetRequest", JsonConvert.SerializeObject(documentCode));

            BudgetCheckResponse budgetCheckResponse = new BudgetCheckResponse();
            try
            {
                var budgetCheckRequest = PrepareBudgetCheckObject(documentCode);
                budgetCheckRequest.IsReConsume = isReConsume;
                if (lstEntities.Count == 0) return true;
                budgetCheckResponse = ConsumeBudget(budgetCheckRequest);
                logNewRelicEvents("CapitalBudgetValidation", "ConsumeCapitalBudgetRequest", JsonConvert.SerializeObject(budgetCheckRequest));
                NewP2PEntities.CapitalBudgetStatus status = NewP2PEntities.CapitalBudgetStatus.None;
                List<BudgetAllocationDetails> lstbudgetAllocationIds = new List<BudgetAllocationDetails>();
                lstbudgetAllocationIds = mapReqSplititemWithBudgetAllocationID(budgetCheckRequest, budgetCheckResponse);
                if (budgetCheckResponse.IsSuccess)
                {
                    status = (NewP2PEntities.CapitalBudgetStatus)getCapitalBudgetStatus(budgetCheckResponse.DocumentBudgetAction);
                    UpdateDocumentBudgetoryStatus(documentCode, getCapitalBudgetStatus(budgetCheckResponse.DocumentBudgetAction), lstbudgetAllocationIds);
                }
                else
                {
                    UpdateDocumentBudgetoryStatus(documentCode, (byte)NewP2PEntities.CapitalBudgetStatus.ConsumeBudgetFailed, lstbudgetAllocationIds);
                }

                return budgetCheckResponse.IsSuccess && status != NewP2PEntities.CapitalBudgetStatus.Decline ? true : false;

            }
            catch (Exception ex)
            {
                saveNewrelicErrors("ConsumeCapitalBudget", "ConsumeCapitalBudgetError", ex.Message);
                throw;
            }
            finally
            {
                logNewRelicEvents("ConsumeCapitalBudget", "ConsumeCapitalBudgetResponse", JsonConvert.SerializeObject(budgetCheckResponse));
            }
        }

        public Cumulus.P2P.NewBusinessEntities.P2P.Common.ExternalApproverDetails GetCapitalBudgetApprovers(Cumulus.P2P.NewBusinessEntities.P2P.Common.ExternalApproverDetails externalApproverDetails, Boolean isReConsume = true)
        {
            BudgetCheckResponse budgetCheckResponse = new BudgetCheckResponse();
            List<long> budgetApprovers = new List<long>();
            bool isCapitalBudgetCheckfailed = false;
            try
            {
                var budgetCheckRequest = PrepareBudgetCheckObject(externalApproverDetails.DocumentCode);
                budgetCheckRequest.IsReConsume = isReConsume;
                if (lstEntities.Count == 0)
                {
                    externalApproverDetails.Status = 400;
                    return externalApproverDetails;
                }
                budgetCheckResponse = ConsumeBudget(budgetCheckRequest);
                NewP2PEntities.CapitalBudgetStatus status = NewP2PEntities.CapitalBudgetStatus.None;
                List<long> lstbudgetAllocationIds = new List<long>();
                List<Cumulus.P2P.NewBusinessEntities.P2P.Common.ExternalApprovers> externalApproversData = new List<Cumulus.P2P.NewBusinessEntities.P2P.Common.ExternalApprovers>();
                if (budgetCheckResponse.IsSuccess)
                {
                    status = (NewP2PEntities.CapitalBudgetStatus)getCapitalBudgetStatus(budgetCheckResponse.DocumentBudgetAction);
                    if (status == CapitalBudgetStatus.ComplexApproval || status == CapitalBudgetStatus.SimpleApproval)
                    {
                        lstbudgetAllocationIds = getBudgetEntityAllocationIds(budgetCheckRequest, budgetCheckResponse);
                        var budgetOwnerDetails = GetBudgetDetailsByAllocationIds(lstbudgetAllocationIds);

                        if (budgetOwnerDetails != null)
                        {
                            var budgetOwnerList = budgetOwnerDetails.FirstOrDefault();
                            var datalist = budgetOwnerList.BudgetOwners.Select(x => new { x.BudgetOwnerContactCode, x.BudgetOwnerName, x.BudgetOwnerId }).Distinct().OrderBy(y => y.BudgetOwnerName).ToList();
                            if (datalist != null)
                            {
                                for (int i = 0; i < datalist.Count(); i++)
                                {
                                    var list = new Cumulus.P2P.NewBusinessEntities.P2P.Common.ExternalApprovers()
                                    {
                                        Profile = new Cumulus.P2P.NewBusinessEntities.P2P.Common.UserProfile
                                        {
                                            ContactCode = datalist[i].BudgetOwnerContactCode,
                                            UserName = datalist[i].BudgetOwnerName
                                        },
                                        SequenceNumber = i + 1,
                                        Status = 2
                                    };
                                    externalApproversData.Add(list);
                                }
                            }
                            else
                            {
                                var list = new Cumulus.P2P.NewBusinessEntities.P2P.Common.ExternalApprovers()
                                {
                                    Profile = new Cumulus.P2P.NewBusinessEntities.P2P.Common.UserProfile
                                    {
                                        ContactCode = 0,
                                        UserName = ""
                                    },
                                    SequenceNumber = 1,
                                    Status = 2
                                };
                                externalApproversData.Add(list);
                            }
                            externalApproverDetails.externalApproversList = externalApproversData;
                            externalApproverDetails.DocumentStatus = GEP.Cumulus.Documents.Entities.DocumentStatus.ApprovalPending;
                        }
                    }
                    else if (status == CapitalBudgetStatus.HierarachyApproval)
                    {                        
                        if (budgetCheckResponse.HierarchyApprovers != null && budgetCheckResponse.HierarchyApprovers.Count > 0)
                        {                           
                            externalApproversData = (from b in budgetCheckResponse.HierarchyApprovers
                                                     select new Cumulus.P2P.NewBusinessEntities.P2P.Common.ExternalApprovers
                                                     {
                                                         Profile = new Cumulus.P2P.NewBusinessEntities.P2P.Common.UserProfile
                                                         {
                                                             ContactCode = b.BudgetOwnerContactCode,
                                                             UserName = b.BudgetOwnerName,
                                                         },
                                                         SequenceNumber = b.SequenceId,
                                                         Status = 2,

                                                     }).ToList();
                            if (externalApproversData == null || externalApproversData.Count == 0)
                            {
                                externalApproversData.Add(new Cumulus.P2P.NewBusinessEntities.P2P.Common.ExternalApprovers()
                                {
                                    Profile = new Cumulus.P2P.NewBusinessEntities.P2P.Common.UserProfile
                                    {
                                        ContactCode = 0,
                                        UserName = ""
                                    },
                                    SequenceNumber = 1,
                                    Status = 2
                                });
                            }
                        }
                        else
                        {
                            externalApproversData.Add(new Cumulus.P2P.NewBusinessEntities.P2P.Common.ExternalApprovers()
                            {
                                Profile = new Cumulus.P2P.NewBusinessEntities.P2P.Common.UserProfile
                                {
                                    ContactCode = 0,
                                    UserName = ""
                                },
                                SequenceNumber = 1,
                                Status = 2
                            });
                        }
                        externalApproverDetails.externalApproversList = externalApproversData;
                        externalApproverDetails.DocumentStatus = GEP.Cumulus.Documents.Entities.DocumentStatus.ApprovalPending;                        
                    }
                }
                if (!budgetCheckResponse.IsSuccess || (status == NewP2PEntities.CapitalBudgetStatus.Decline))
                {
                    isCapitalBudgetCheckfailed = true;
                    externalApproverDetails.Error = "Declined";
                }
                externalApproverDetails.Status = 200;
                if (isCapitalBudgetCheckfailed)
                    externalApproverDetails.Status = 400;
                return externalApproverDetails;
            }
            catch (Exception ex)
            {
                saveNewrelicErrors("GetCapitalBudgetApprovals", "GetCapitalBudgetApprovers", ex.Message);
                externalApproverDetails.Status = 400;
                externalApproverDetails.Error = ex.Message;
                return externalApproverDetails;
            }
        }

        public List<BudgetDocumentDetails> GetBudgetDetailsByAllocationIds(List<long> BudgetEntityAllocationId)
        {
            string getConsumeBudgetAPIURL = MultiRegionConfig.GetConfig(CloudConfig.APIMBaseURL) + URLs.GetBudgetDetailsByAllocationIds;
                
            List<BudgetDocumentDetails> budgetResponse = null;
            try
            {

                var requestHeaders = new RESTAPIHelper.RequestHeaders();
                requestHeaders.Set(UserContext, JWTToken);
                string appName = Environment.GetEnvironmentVariable("NewRelic.AppName") ?? "Requisition";
                var webAPI = new RESTAPIHelper.WebAPI(requestHeaders, appName, "CapitalBudgetManager-GetBudgetDetailsByAllocationIds");
                var JsonResult = webAPI.ExecutePost(getConsumeBudgetAPIURL, BudgetEntityAllocationId);

                var jsonSerializer = new JavaScriptSerializer();
                budgetResponse = jsonSerializer.Deserialize<List<BudgetDocumentDetails>>(JsonResult);
                return budgetResponse;
            }
            catch (Exception ex)
            {
                logNewRelicEvents("ConsumeCapitalBudget", "ConsumeCapitalBudgetResponse", JsonConvert.SerializeObject(budgetResponse));
                //throw;
            }
            return budgetResponse;
        }

        public short getCapitalBudgetStatus(string BudgetStatus)
        {
            if (BudgetStatus == null) return 0;
            foreach (var iColor in Enum.GetValues(typeof(NewP2PEntities.CapitalBudgetStatus)))
            {
                if (iColor.ToString() == BudgetStatus.Replace(" ", ""))
                {
                    return (byte)(NewP2PEntities.CapitalBudgetStatus)Enum.Parse(typeof(NewP2PEntities.CapitalBudgetStatus), BudgetStatus.Replace(" ", ""));

                }
            }
            return 0;
        }
        private void saveNewrelicErrors(string eventname, string key, string values)
        {

            LogHelper.LogError(Log, eventname + " - " + key + " - " + values, new Exception());

            //var eventObject = new Dictionary<string, object>();
            //try
            //{

            //    eventObject.Add(key, values);
            //    NewRelic.Api.Agent.NewRelic.RecordCustomEvent(eventname, eventObject);
            //}
            //catch (Exception)
            //{
            //    NewRelic.Api.Agent.NewRelic.NoticeError("Error Occured on New Relic Writing ", null);
            //}
        }

        private void logNewRelicEvents(string eventname, string key, string values)
        {
            var eventAttributes = new Dictionary<string, object>();
            eventAttributes.Add("eventname", eventname);
            eventAttributes.Add("key", key);
            eventAttributes.Add("values", values);
            NewRelic.Api.Agent.NewRelic.RecordCustomEvent("CapitalBudgetManagerEvents", eventAttributes);
        }

        public List<BudgetAllocationDetails> mapReqSplititemWithBudgetAllocationID(BudgetCheckRequest budgetCheckRequest, BudgetCheckResponse budgetCheckResponse)
        {
            List<BudgetAllocationDetails> budgetAllocationDetails = new List<BudgetAllocationDetails>();
            List<KeyValuePair<long, long>> lstbudgetAllocationIds = new List<KeyValuePair<long, long>>();
            foreach (var request in budgetCheckRequest.LineInfos)
            {
                var response = budgetCheckResponse.LineDetails.Where(x => x.LineItemId == request.LineItemId).FirstOrDefault();
                if (response != null)
                {
                    for (int i = 0; i < request.Splits.Count; i++)
                    {
                        foreach (var details in response.Splits[i].AllocationDetails)
                        {
                            foreach (var items in details.BudgetOwners)
                            {
                                BudgetAllocationDetails budgetAllocation = new BudgetAllocationDetails();
                                budgetAllocation.BudgetEntityAllocationId = details.BudgetEntityAllocationId;
                                budgetAllocation.BudgetId = details.BudgetId;
                                budgetAllocation.RequisitionSplititemId = request.Splits[i].SplitId;
                                budgetAllocation.RequisitionId = budgetCheckRequest.DocumentCode;
                                budgetAllocation.BudgetOwnerContactcode = items;
                                budgetAllocationDetails.Add(budgetAllocation);
                            }
                        }
                        //lstbudgetAllocationIds.Add(new KeyValuePair<long, long>(request.Splits[i].SplitId, response.Splits[i].AllocationDetails.Select(a=>a.BudgetEntityAllocationId).FirstOrDefault()));
                    }
                }
            }
            return budgetAllocationDetails;
        }

        private List<long> getBudgetEntityAllocationIds(BudgetCheckRequest budgetCheckRequest, BudgetCheckResponse budgetCheckResponse)
        {
            List<long> lstbudgetAllocationIds = new List<long>();
            foreach (var request in budgetCheckRequest.LineInfos)
            {
                var response = budgetCheckResponse.LineDetails.Where(x => x.LineItemId == request.LineItemId).FirstOrDefault();
                if (response != null)
                {
                    for (int i = 0; i < request.Splits.Count; i++)
                    {
                        foreach (var details in response.Splits[i].AllocationDetails)
                        {
                            lstbudgetAllocationIds.Add(details.BudgetEntityAllocationId);
                        }
                    }
                }
            }
            return lstbudgetAllocationIds.Distinct().ToList();
        }
    }
}
