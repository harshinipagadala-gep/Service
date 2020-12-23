using GEP.Cumulus.P2P.BusinessEntities;
using System;
using System.Collections.Generic;

namespace GEP.Cumulus.P2P.Req.DataAccessObjects
{
    public interface IAdditionalFieldsDAO : P2P.DataAccessObjects.Usability.INewP2PDocumentDAO
    {
        List<PurchaseTypeFeatureMapping> GetAdditionalFieldsByDocumentType(int DocumentTypeCode, long LOBEntityDetailCode, Int16 PurchaseType = 0, Int16 LeveType = 0, bool isAllFieldsConfigRequired = false, string CultureCode = "");
        List<GEP.Cumulus.P2P.NewBusinessEntities.P2P.Common.P2PAdditionalFieldResult> GetAdditionalFieldMasterData(int AdditionalFieldId, long LOBEntityDetailCode, long AdditionalParentFieldDetailCode = 0, string SearchText = "", int pageIndex = 1, int pageSize = 10);
        List<GEP.Cumulus.P2P.NewBusinessEntities.P2P.Common.P2PAdditionalFieldsData> GetAdditionalFieldsValues(string AdditionalFieldIds, long LOBEntityDetailCode = 0, int DecumentTypeCode = 0, int FeatureId = 0, Int16 LevelType= 0);
        List<GEP.Cumulus.P2P.NewBusinessEntities.P2P.Common.P2PAdditionalFieldsData> GetAdditionalFieldsMasterDataByFieldControlType(int DocumentTypeCode, long LOBEntityDetailCode = 0, string FieldControlType = "4");        
        List<GEP.Cumulus.P2P.NewBusinessEntities.P2P.Common.AdditionalFieldDefaultValuesResponse> GetAllChildAdditionalFieldsWithDefaultValues(NewBusinessEntities.P2P.Common.AdditionalFieldDefaultValuesRequest additionalFieldDefaultValuesRequest);
        List<GEP.Cumulus.P2P.NewBusinessEntities.P2P.Common.P2PAdditionalFieldResult> GetAdditionalFieldData(int AdditionalFieldId, string AdditionalParentFieldDetailCodes = "0", string SearchText = "", int pageIndex = 1, int pageSize = 10, int DecumentTypeCode = 0, int FeatureId = 0, Int16 LevelType = 0);
        List<PurchaseTypeFeatureMapping> GetAdditionalFieldsBasedOnDocumentType(int DocumentTypeCode, string LOBEntityDetailCodes, Int16 PurchaseType = 0, Int16 LeveType = 0, bool isAllFieldsConfigRequired = false, string CultureCode = "en-US");
    List<AdditionalFieldsDocumentSourceResponse> GetSourceDocumentAdditionalFieldsConfig(List<AdditionalFieldsDocumentSourceInput> additionalFieldsDocumentSourceInputs, string cultureCode = "en-US");
  }
}
