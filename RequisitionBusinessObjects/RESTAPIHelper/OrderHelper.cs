using GEP.Cumulus.Documents.Entities;
using GEP.Cumulus.P2P.BusinessEntities;
using GEP.SMART.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper
{
    [ExcludeFromCodeCoverage]
    public class OrderHelper
    {
        private readonly string serviceURL;
        private readonly string reqserviceURL;
        private string appName = Environment.GetEnvironmentVariable("NewRelic.AppName") ?? "Requisition"; 
        private string useCase = "NewRequisitionManager-OrderHelper";
        GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.WebAPI webAPI;

        public Gep.Cumulus.CSM.Entities.UserExecutionContext UserContext { get; set; }
        Req.BusinessObjects.RESTAPIHelper.RequestHeaders requestHeaders;

        public OrderHelper(Gep.Cumulus.CSM.Entities.UserExecutionContext userExecutionContext, string jwtToken)
        {
            serviceURL = string.Concat(MultiRegionConfig.GetConfig(CloudConfig.APIMBaseURL), ServiceURLs.OrderServiceURL);
            reqserviceURL = string.Concat(MultiRegionConfig.GetConfig(CloudConfig.APIMBaseURL), ServiceURLs.OrderReqServiceURL);
            this.UserContext = userExecutionContext;
            requestHeaders = new Req.BusinessObjects.RESTAPIHelper.RequestHeaders();
            requestHeaders.Set(UserContext, jwtToken);
            webAPI = new GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.WebAPI(requestHeaders, appName, useCase);
        }

        public List<long> AutoCreateWorkBenchOrder(long preDocumentId, int processFlag, bool isautosubmit)
        {
            List<long> response = null;
            var Requestobj = new Dictionary<string, object>();
            Requestobj.Add("preDocumentId", preDocumentId);
            Requestobj.Add("processFlag", processFlag);
            Requestobj.Add("isautosubmit", isautosubmit);
            try
            {
                var result = webAPI.ExecutePost(serviceURL + "AutoCreateWorkBenchOrder", Requestobj);
                response = JsonConvert.DeserializeObject<List<long>>(result);
            }
            catch (Exception httpException)
            {
                throw httpException;
            }
            return response;
        }
        public List<long> GetSettingsAndAutoCreateOrder(long preDocumentId)
        {
            List<long> response = null;
            try
            {
                var result = webAPI.ExecuteGet(serviceURL + "GetSettingsAndAutoCreateOrder?preDocumentId=" + preDocumentId);
                response = JsonConvert.DeserializeObject<List<long>>(result);
            }
            catch (Exception httpException)
            {
                throw httpException;
            }
            return response;
        }
        public long CreateOrderFromRequisitionItems(List<long> listItemIds, string supplierCode = "")
        {
            long response = 0;
            var Requestobj = new Dictionary<string, object>();
            Requestobj.Add("listItemIds", listItemIds);
            Requestobj.Add("supplierCode", supplierCode);
            try
            {
                var result = webAPI.ExecutePost(reqserviceURL + "CreateOrderFromRequisitionItems", Requestobj);
                response = JsonConvert.DeserializeObject<long>(result);
            }
            catch (Exception httpException)
            {
                throw httpException;
            }
            return response;
        }
        public List<NewP2PEntities.DocumentInfo> GetOrdersListForWorkBench(string listItemIds)
        {
            var ordersList = new List<NewP2PEntities.DocumentInfo>();
            var Requestobj = new Dictionary<string, string>();
            Requestobj.Add("listItemIds", listItemIds);
            try
            {
                var result = webAPI.ExecutePost(reqserviceURL + "GetOrdersListForWorkBench", Requestobj);
                ordersList = JsonConvert.DeserializeObject<List<NewP2PEntities.DocumentInfo>>(result);
            }
            catch (Exception httpException)
            {
                throw httpException;
            }
            return ordersList;
        }
        public bool SaveReqItemstoExistingPO(List<long> listItemIds, long OrderId)
        {
            bool response = false;
            var Requestobj = new Dictionary<string, object>();
            Requestobj.Add("listItemIds", listItemIds);
            Requestobj.Add("OrderId", OrderId);
            try
            {
                var result = webAPI.ExecutePost(reqserviceURL + "SaveReqItemstoExistingPO", Requestobj);
                response = JsonConvert.DeserializeObject<bool>(result);
            }
            catch (Exception httpException)
            {
                throw httpException;
            }
            return response;
        }
        public List<GEP.NewP2PEntities.ASLData> ValidateAndGetASLbyReqItems(GEP.NewP2PEntities.ListInputData lstSupplierdetails)
        {
            var response = new List<GEP.NewP2PEntities.ASLData>();
            var Requestobj = new Dictionary<string, object>();
            Requestobj.Add("supplierdetails", lstSupplierdetails);
            try
            {
                var result = webAPI.ExecutePost(reqserviceURL + "ValidateAndGetASLbyReqItems", Requestobj);
                response = JsonConvert.DeserializeObject<List<GEP.NewP2PEntities.ASLData>>(result);
            }
            catch (Exception httpException)
            {
                throw httpException;
            }
            return response;
        }
        public long SaveOrderFromRequisition(P2PDocumentType documentType, long requisitionId, decimal partnerCode, long ContactCode, List<DocumentBU> lstBU, long orderingLocation = 0, bool isFunctionalAdmin = false, List<long> p2pLineItemIds = null)
        {
            long response = 0;
            var Requestobj = new Dictionary<string, object>();
            Requestobj.Add("docType", documentType);
            Requestobj.Add("requisitionId", requisitionId);
            Requestobj.Add("partnerCode", partnerCode);
            Requestobj.Add("userId", ContactCode);
            Requestobj.Add("lstBU", lstBU);
            Requestobj.Add("orderingLocation", orderingLocation);
            Requestobj.Add("isFunctionalAdmin", isFunctionalAdmin);
            Requestobj.Add("p2pLineItemIds", p2pLineItemIds);
            try
            {
                var result = webAPI.ExecutePost(serviceURL + "SaveOrderFromRequisition", Requestobj, 120000);
                response = JsonConvert.DeserializeObject<long>(result);
            }
            catch (Exception httpException)
            {
                throw httpException;
            }
            return response;
        }
    }
}
