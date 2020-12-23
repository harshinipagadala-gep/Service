using Gep.Cumulus.CSM.Entities;
using GEP.Cumulus.P2P.Req.RestService.App_Start;
using GEP.Cumulus.Web.Utils;
using GEP.NewP2PEntities;
using GEP.NewPlatformEntities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.ServiceModel;
namespace GEP.Cumulus.P2P.RestService.Req.App_Start.Proxy
{
    [ExcludeFromCodeCoverage]
    internal class NewProxyRequisitionService
    {
        public GepServiceFactory GepServices = GepServiceManager.GetInstance;
        public string ServiceUrl = UrlHelperExtensions.NewRequisitionServiceUrl.ToString();
        private INewRequisitionServiceChannel objRequisitionServiceChannel = null;
        private OperationContextScope objOperationContextScope = null;
        private UserExecutionContext UserExecutionContext = null;
        private string JWTToken = string.Empty;
        public NewProxyRequisitionService(UserExecutionContext UserExecutionContext, string jwtToken)
        {
            this.UserExecutionContext = UserExecutionContext;
            this.JWTToken = jwtToken;
        }

        private void AddToken(string jwtToken)
        {
            MessageHeader<string> objMhgAuth = new MessageHeader<string>(jwtToken);
            System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
            OperationContext.Current.OutgoingMessageHeaders.Add(authorization);
        }

        public SaveResult SaveRequisitionHeader(Requisition req)
        {
            try
            {
                objRequisitionServiceChannel = P2P.Req.RestService.App_Start.ServiceHelper.ConfigureChannel<INewRequisitionServiceChannel>(ServiceUrl, UserExecutionContext, ref objOperationContextScope);
                using (objOperationContextScope)
                {
                    AddToken(JWTToken);
                    return objRequisitionServiceChannel.SaveRequisitionHeader(req);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                GEPServiceManager.DisposeService(objRequisitionServiceChannel, objOperationContextScope);
            }
        }

        public Requisition GetRequisitionDisplayDetails(Int64 id)
        {
            try
            {
                objRequisitionServiceChannel = P2P.Req.RestService.App_Start.ServiceHelper.ConfigureChannel<INewRequisitionServiceChannel>(ServiceUrl, UserExecutionContext, ref objOperationContextScope);
                AddToken(JWTToken);
                return objRequisitionServiceChannel.GetRequisitionDisplayDetails(id, null);

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                GEPServiceManager.DisposeService(objRequisitionServiceChannel, objOperationContextScope);
            }
        }

        public SaveResult SaveCompleteRequisition(Requisition req)
        {
            try
            {
                objRequisitionServiceChannel = P2P.Req.RestService.App_Start.ServiceHelper.ConfigureChannel<INewRequisitionServiceChannel>(ServiceUrl, UserExecutionContext, ref objOperationContextScope);
                AddToken(JWTToken);
                return objRequisitionServiceChannel.SaveCompleteRequisition(req);

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                GEPServiceManager.DisposeService(objRequisitionServiceChannel, objOperationContextScope);
            }
        }

        public SaveResult AutoSaveDocument(Requisition objectData, Int64 documentCode, int documentTypeCode, Int64 lastModifiedBy)
        {
            try
            {
                objRequisitionServiceChannel = P2P.Req.RestService.App_Start.ServiceHelper.ConfigureChannel<INewRequisitionServiceChannel>(ServiceUrl, UserExecutionContext, ref objOperationContextScope);
                AddToken(JWTToken);
                return objRequisitionServiceChannel.AutoSaveDocument(objectData, documentCode, documentTypeCode, lastModifiedBy);

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                GEPServiceManager.DisposeService(objRequisitionServiceChannel, objOperationContextScope);
            }
        }

        public Requisition GetAutoSaveDocument(Int64 id, int docTypeCode)
        {
            try
            {
                objRequisitionServiceChannel = P2P.Req.RestService.App_Start.ServiceHelper.ConfigureChannel<INewRequisitionServiceChannel>(ServiceUrl, UserExecutionContext, ref objOperationContextScope);
                AddToken(JWTToken);
                return objRequisitionServiceChannel.GetAutoSaveDocument(id, docTypeCode);

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                GEPServiceManager.DisposeService(objRequisitionServiceChannel, objOperationContextScope);
            }
        }

        public SaveResult SaveUserConfigurations(P2PUserConfiguration userConfig)
        {
            try
            {
                objRequisitionServiceChannel = P2P.Req.RestService.App_Start.ServiceHelper.ConfigureChannel<INewRequisitionServiceChannel>(ServiceUrl, UserExecutionContext, ref objOperationContextScope);
                AddToken(JWTToken);
                return objRequisitionServiceChannel.SaveUserConfigurations(userConfig);

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                GEPServiceManager.DisposeService(objRequisitionServiceChannel, objOperationContextScope);
            }
        }

        public List<P2PUserConfiguration> GetUserConfigurations(long contactCode, int documentType)
        {
            try
            {
                objRequisitionServiceChannel = P2P.Req.RestService.App_Start.ServiceHelper.ConfigureChannel<INewRequisitionServiceChannel>(ServiceUrl, UserExecutionContext, ref objOperationContextScope);
                AddToken(JWTToken);
                return objRequisitionServiceChannel.GetUserConfigurations(contactCode, documentType);

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                GEPServiceManager.DisposeService(objRequisitionServiceChannel, objOperationContextScope);
            }
        }
        #region Saved Views For ReqWorkbench

        public List<SavedViewDetails> GetSavedViewsForReqWorkBench(long LobId)
        {
            List<SavedViewDetails> lstSaveViewDetails = new List<SavedViewDetails>();
            try
            {
                objRequisitionServiceChannel = P2P.Req.RestService.App_Start.ServiceHelper.ConfigureChannel<INewRequisitionServiceChannel>(ServiceUrl, UserExecutionContext, ref objOperationContextScope);
                AddToken(JWTToken);
                return objRequisitionServiceChannel.GetSavedViewsForReqWorkBench(LobId);

            }
            catch (Exception ex)
            {
                //return lstSaveViewDetails;
                throw ex;
            }
            finally
            {
                GEPServiceManager.DisposeService(objRequisitionServiceChannel, objOperationContextScope);
            }
        }

        public long InsertUpdateSavedViewsForReqWorkBench(SavedViewDetails objSavedView)
        {
            try
            {
                objRequisitionServiceChannel = P2P.Req.RestService.App_Start.ServiceHelper.ConfigureChannel<INewRequisitionServiceChannel>(ServiceUrl, UserExecutionContext, ref objOperationContextScope);
                AddToken(JWTToken);
                return objRequisitionServiceChannel.InsertUpdateSavedViewsForReqWorkBench(objSavedView);

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                GEPServiceManager.DisposeService(objRequisitionServiceChannel, objOperationContextScope);
            }
        }

        public bool DeleteSavedViewsForReqWorkBench(long savedViewId)
        {
            try
            {
                objRequisitionServiceChannel = P2P.Req.RestService.App_Start.ServiceHelper.ConfigureChannel<INewRequisitionServiceChannel>(ServiceUrl, UserExecutionContext, ref objOperationContextScope);
                AddToken(JWTToken);
                return objRequisitionServiceChannel.DeleteSavedViewsForReqWorkBench(savedViewId);

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                GEPServiceManager.DisposeService(objRequisitionServiceChannel, objOperationContextScope);
            }
        }
        #endregion
        public List<long> AutoCreateWorkBenchOrder(long RequisitionId, int itemscount, bool isautosubmit)
        {
            try
            {
                objRequisitionServiceChannel = P2P.Req.RestService.App_Start.ServiceHelper.ConfigureChannel<INewRequisitionServiceChannel>(ServiceUrl, UserExecutionContext, ref objOperationContextScope);
                AddToken(JWTToken);
                return objRequisitionServiceChannel.AutoCreateWorkBenchOrder(RequisitionId, itemscount, isautosubmit);

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                GEPServiceManager.DisposeService(objRequisitionServiceChannel, objOperationContextScope);
            }
        }

        public Documents.Entities.DocumentLOBDetails GetDocumentLOB(long documentCode)
        {
            try
            {
                objRequisitionServiceChannel = P2P.Req.RestService.App_Start.ServiceHelper.ConfigureChannel<INewRequisitionServiceChannel>(ServiceUrl, UserExecutionContext, ref objOperationContextScope);
                AddToken(JWTToken);
                return objRequisitionServiceChannel.GetDocumentLOB(documentCode);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                GEPServiceManager.DisposeService(objRequisitionServiceChannel, objOperationContextScope);
            }
        }

        public bool EnableQuickQuoteRuleCheck(long documentCode)
        {
            try
            {
                objRequisitionServiceChannel = P2P.Req.RestService.App_Start.ServiceHelper.ConfigureChannel<INewRequisitionServiceChannel>(ServiceUrl, UserExecutionContext, ref objOperationContextScope);
                AddToken(JWTToken);
                return objRequisitionServiceChannel.EnableQuickQuoteRuleCheck(documentCode);

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                GEPServiceManager.DisposeService(objRequisitionServiceChannel, objOperationContextScope);
            }
        }

        public void ResetRequisitionItemFlipType(long requisitionId)
        {
            try
            {
                objRequisitionServiceChannel = P2P.Req.RestService.App_Start.ServiceHelper.ConfigureChannel<INewRequisitionServiceChannel>(ServiceUrl, UserExecutionContext, ref objOperationContextScope);
                AddToken(JWTToken);
                objRequisitionServiceChannel.ResetRequisitionItemFlipType(requisitionId);

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                GEPServiceManager.DisposeService(objRequisitionServiceChannel, objOperationContextScope);
            }
        }

    }
}
