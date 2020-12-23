using Gep.Cumulus.CSM.Entities;
using GEP.Cumulus.P2P.BusinessEntities;
using System;

namespace GEP.Cumulus.P2P.Req.BusinessObjects.FactoryClasses
{
    public class RequisitionTemplateDownLoader : RequisitionTemplateGenerator
    {
        public RequisitionTemplateDownLoader(UserExecutionContext userContext, Int64 documentCode, string documentNumber, NewRequisitionManager ReqManger, GEP.NewP2PEntities.FileManagerEntities.ReqTemplateFileResponse reqTemplateResponse, string jwtToken)
            : base(userContext, documentCode, documentNumber, ReqManger, jwtToken , RequisitionExcelTemplateHandler.DownloadTemplate)
        {
            GenerateTemplate();
            reqTemplateResponse.FileId = UploadFiletoBlobContainerAndGetFileId();
        }
    }
}
