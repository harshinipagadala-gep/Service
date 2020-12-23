using GEP.Cumulus.Logging;
using GEP.Cumulus.P2P.RestService.Req.App_Start.Proxy;
using GEP.Cumulus.Web.Utils.Helpers;
using GEP.NewP2PEntities;
using GEP.NewPlatformEntities;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.ServiceModel.Activation;


namespace GEP.Cumulus.P2P.Req.RestService
{
    [ExcludeFromCodeCoverage]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class NewRequisitionRestService : GEP.Cumulus.P2P.Req.RestServiceContracts.INewRequisitionRestService
    {
        private static readonly ILog Log = Logger.GetLog(MethodBase.GetCurrentMethod().DeclaringType);

        private string Token
        {
            get
            {
                var token = string.Empty;
                try
                {
                    if (System.ServiceModel.Web.WebOperationContext.Current != null &&
                        System.ServiceModel.Web.WebOperationContext.Current.IncomingRequest != null &&
                        System.ServiceModel.Web.WebOperationContext.Current.IncomingRequest.Headers["Authorization"] != null)
                    {
                        token = System.ServiceModel.Web.WebOperationContext.Current.IncomingRequest.Headers["Authorization"];
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.LogError(Log, "Error occured in Token NewRequisitionRestService :", ex);
                }
                return token;
            }
        }

        public SaveResult SaveRequisitionHeader(Requisition req)
        {
            try
            {
                NewProxyRequisitionService prxyReqService = new NewProxyRequisitionService(P2P.Req.RestService.ExceptionHelper.GetExecutionContext, Token);
                return prxyReqService.SaveRequisitionHeader(req);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error Occured in SaveRequisitionHeader method", ex);
                throw;
            }
        }

        public Requisition GetRequisitionDisplay(Int64 id)
        {
            try
            {
                NewProxyRequisitionService prxyReqService = new NewProxyRequisitionService(P2P.Req.RestService.ExceptionHelper.GetExecutionContext, Token);
                return prxyReqService.GetRequisitionDisplayDetails(id);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error Occured in GetRequisitionDisplay method", ex);
                throw;
            }
        }

        public SaveResult SaveCompleteRequisition(Requisition req)
        {
            try
            {
                SaveResult result = new SaveResult();
                long buyerPartnerCode = P2P.Req.RestService.ExceptionHelper.GetExecutionContext.BuyerPartnerCode;
                NewProxyRequisitionService prxyReqService = new NewProxyRequisitionService(P2P.Req.RestService.ExceptionHelper.GetExecutionContext, Token);
                result = prxyReqService.SaveCompleteRequisition(req);
                if (result.success && req != null)
                    result.edc = UrlEncryptionHelper.EncryptURL("dc=" + result.id + "&bpc=" + buyerPartnerCode);
                return result;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error Occured in SaveCompleteRequisition method", ex);
                throw;
            }
        }

        public SaveResult AutoSaveDocument(Requisition documentData, Int64 documentCode, int documentTypeCode, Int64 lastModifiedBy)
        {
            try
            {
                NewProxyRequisitionService prxyReqService = new NewProxyRequisitionService(P2P.Req.RestService.ExceptionHelper.GetExecutionContext, Token);
                return prxyReqService.AutoSaveDocument(documentData, documentCode, documentTypeCode, lastModifiedBy);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error Occured in AutoSaveDocument method", ex);
                throw;
            }
        }

        public Requisition GetAutoSaveDocument(Int64 id, int docTypeCode)
        {
            try
            {
                NewProxyRequisitionService prxyReqService = new NewProxyRequisitionService(P2P.Req.RestService.ExceptionHelper.GetExecutionContext, Token);
                return prxyReqService.GetAutoSaveDocument(id, docTypeCode);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error Occured in GetAutoSaveDocument method", ex);
                throw;
            }
        }

        public SaveResult SaveUserConfigurations(P2PUserConfiguration userConfig)
        {
            try
            {
                NewProxyRequisitionService prxyReqService = new NewProxyRequisitionService(P2P.Req.RestService.ExceptionHelper.GetExecutionContext, Token);
                return prxyReqService.SaveUserConfigurations(userConfig);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error Occured in SaveUserConfigurations method", ex);
                throw;
            }
        }

        public List<P2PUserConfiguration> GetUserConfigurations(long contactCode, int documentType)
        {
            try
            {
                NewProxyRequisitionService prxyReqService = new NewProxyRequisitionService(P2P.Req.RestService.ExceptionHelper.GetExecutionContext, Token);
                return prxyReqService.GetUserConfigurations(contactCode, documentType);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error Occured in GetUserConfigurations method", ex);
                throw;
            }
        }

        public string getString()
        {
            return "test";
        }

        public List<SavedViewDetails> GetSavedViewsForReqWorkBench(long LobId)
        {
            try
            {
                NewProxyRequisitionService prxyReqService = new NewProxyRequisitionService(P2P.Req.RestService.ExceptionHelper.GetExecutionContext, Token);
                return prxyReqService.GetSavedViewsForReqWorkBench(LobId);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error Occured in GetSavedViewsForReqWorkBench method", ex);
                throw;
            }
        }

        public long InsertUpdateSavedViewsForReqWorkBench(SavedViewDetails objSavedView)
        {
            try
            {
                NewProxyRequisitionService prxyReqService = new NewProxyRequisitionService(P2P.Req.RestService.ExceptionHelper.GetExecutionContext, Token);
                return prxyReqService.InsertUpdateSavedViewsForReqWorkBench(objSavedView);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error Occured in InsertUpdateSavedViewsForReqWorkBench method", ex);
                throw;
            }
        }

        public bool DeleteSavedViewsForReqWorkBench(long savedViewId)
        {
            try
            {
                NewProxyRequisitionService prxyReqService = new NewProxyRequisitionService(P2P.Req.RestService.ExceptionHelper.GetExecutionContext, Token);
                return prxyReqService.DeleteSavedViewsForReqWorkBench(savedViewId);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error Occured in DeleteSavedViewsForReqWorkBench method", ex);
                throw;
            }
        }
        public List<long> AutoCreateWorkBenchOrder(long RequisitionId, int itemscount, bool isautosubmit)
        {
            try
            {
                NewProxyRequisitionService prxyReqService = new NewProxyRequisitionService(P2P.Req.RestService.ExceptionHelper.GetExecutionContext, Token);
                return prxyReqService.AutoCreateWorkBenchOrder(RequisitionId, itemscount, isautosubmit);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error Occured in DeleteSavedViewsForReqWorkBench method", ex);
                throw;
            }
        }
    }
}
