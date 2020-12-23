using GEP.NewP2PEntities;
using GEP.NewPlatformEntities;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace GEP.Cumulus.P2P.Req.RestServiceContracts
{
    [ServiceContract]
    public interface INewRequisitionRestService
    {
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, UriTemplate = "SaveRequisitionHeader")]
        SaveResult SaveRequisitionHeader(Requisition req);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, UriTemplate = "GetReqFromSQL")]
        Requisition GetRequisitionDisplay(Int64 id);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, UriTemplate = "SaveCompleteRequisition")]
        SaveResult SaveCompleteRequisition(Requisition req);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, UriTemplate = "GetAutoSaveDocument")]
        Requisition GetAutoSaveDocument(Int64 id, int docTypeCode);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, UriTemplate = "AutoSaveDocument")]
        SaveResult AutoSaveDocument(Requisition documentData, Int64 documentCode, int documentTypeCode, Int64 lastModifiedBy);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, UriTemplate = "GetUserConfigurations")]
        List<P2PUserConfiguration> GetUserConfigurations(long contactCode, int documentType);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, UriTemplate = "SaveUserConfigurations")]
        SaveResult SaveUserConfigurations(P2PUserConfiguration userConfig);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, UriTemplate = "getString")]
        string getString();

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, UriTemplate = "GetSavedViewsForReqWorkBench")]
        List<SavedViewDetails> GetSavedViewsForReqWorkBench(long LobId = 0);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, UriTemplate = "InsertUpdateSavedViewsForReqWorkBench")]
        long InsertUpdateSavedViewsForReqWorkBench(SavedViewDetails savedView);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, UriTemplate = "DeleteSavedViewsForReqWorkBench")]
        bool DeleteSavedViewsForReqWorkBench(long savedViewId);
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, UriTemplate = "AutoCreateWorkBenchOrder")]
        List<long> AutoCreateWorkBenchOrder(long RequisitionId, int itemscount, bool isautosubmit);
    }
}
