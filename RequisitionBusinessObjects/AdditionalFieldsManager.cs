using GEP.Cumulus.Logging;
using GEP.Cumulus.P2P.BusinessEntities;
using GEP.Cumulus.P2P.Req.DataAccessObjects;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
namespace GEP.Cumulus.P2P.Req.BusinessObjects
{
    [ExcludeFromCodeCoverage]
    public class AdditionalFieldsManager : Gep.Cumulus.CSM.BaseBusinessObjects.BaseBO
    {
        private static readonly ILog Log = Logger.GetLog(MethodBase.GetCurrentMethod().DeclaringType);

        private IAdditionalFieldsDAO GetAdditionalFieldsDao()
        {
            return AdditionalFieldsDAOManager.GetDAO<IAdditionalFieldsDAO>(UserContext, GepConfiguration);
        }
        public List<PurchaseTypeFeatureMapping> GetAdditionalFieldsByDocumentType(int DocumentTypeCode, long LOBEntityDetailCode, Int16 PurchaseType = 0, Int16 LeveType = 0, bool isAllFieldsConfigRequired = false, string CultureCode = "")
        {
            try
            {
                return GetAdditionalFieldsDao().GetAdditionalFieldsByDocumentType(DocumentTypeCode, LOBEntityDetailCode, PurchaseType, LeveType, isAllFieldsConfigRequired,CultureCode);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error Occured in GetAdditionalFieldsByDocumentType method", ex);
                throw ex;
            }
        }

        public List<GEP.Cumulus.P2P.NewBusinessEntities.P2P.Common.P2PAdditionalFieldResult> GetAdditionalFieldMasterData(int AdditionalFieldId, long LOBEntityDetailCode, long AdditionalParentFieldDetailCode = 0, string SearchText = "", int pageIndex = 1, int pageSize = 10)
        {
            try
            {
                return GetAdditionalFieldsDao().GetAdditionalFieldMasterData(AdditionalFieldId, LOBEntityDetailCode, AdditionalParentFieldDetailCode, SearchText, pageIndex, pageSize);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error Occured in GetAdditionalFieldMasterData method", ex);
                throw ex;
            }
        }

        public List<GEP.Cumulus.P2P.NewBusinessEntities.P2P.Common.P2PAdditionalFieldsData> GetAdditionalFieldsValues(string AdditionalFieldIds, long LOBEntityDetailCode = 0, int DecumentTypeCode = 0, int FeatureId = 0, Int16 LevelType = 0)
        {
            try
            {
                return GetAdditionalFieldsDao().GetAdditionalFieldsValues(AdditionalFieldIds, LOBEntityDetailCode, DecumentTypeCode , FeatureId, LevelType);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error Occured in GetAdditionalFieldsValues method", ex);
                throw ex;
            }
        }

        public List<GEP.Cumulus.P2P.NewBusinessEntities.P2P.Common.P2PAdditionalFieldsData> GetAdditionalFieldsMasterDataByFieldControlType(int DocumentTypeCode, long LOBEntityDetailCode = 0, string FieldControlType = "4")
        {
            try
            {
                return GetAdditionalFieldsDao().GetAdditionalFieldsMasterDataByFieldControlType(DocumentTypeCode, LOBEntityDetailCode, FieldControlType);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error Occured in GetAdditionalFieldsMasterDataByFieldControlType method", ex);
                throw ex;
            }
        }

        public List<GEP.Cumulus.P2P.NewBusinessEntities.P2P.Common.P2PAdditionalFieldResult> GetAdditionalFieldData(int AdditionalFieldId, string AdditionalParentFieldDetailCodes = "0", string SearchText = "", int pageIndex = 1, int pageSize = 10, int DecumentTypeCode = 0, int FeatureId = 0, Int16 LevelType = 0)
        {
            try
            {
                return GetAdditionalFieldsDao().GetAdditionalFieldData(AdditionalFieldId, AdditionalParentFieldDetailCodes, SearchText, pageIndex, pageSize, DecumentTypeCode ,FeatureId ,LevelType);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error Occured in GetAdditionalFieldData method", ex);
                throw ex;
            }
        }

    public List<GEP.Cumulus.P2P.NewBusinessEntities.P2P.Common.AdditionalFieldDefaultValuesResponse> GetAllChildAdditionalFieldsWithDefaultValues(GEP.Cumulus.P2P.NewBusinessEntities.P2P.Common.AdditionalFieldDefaultValuesRequest additionalFieldDefaultValuesRequest)
    {
      try
      {
        return GetAdditionalFieldsDao().GetAllChildAdditionalFieldsWithDefaultValues(additionalFieldDefaultValuesRequest);
      }
      catch (Exception ex)
      {
        LogHelper.LogError(Log, "Error Occured in GetAllChildAdditionalFieldsWithDefaultValues method", ex);
        throw ex;
      }
    }

        public List<PurchaseTypeFeatureMapping> GetAdditionalFieldsBasedOnDocumentType(int DocumentTypeCode, string LOBEntityDetailCodes, Int16 PurchaseType = 0, Int16 LeveType = 0, bool isAllFieldsConfigRequired = false, string CultureCode = "en-US")
        {
            try
            {
                return GetAdditionalFieldsDao().GetAdditionalFieldsBasedOnDocumentType(DocumentTypeCode, LOBEntityDetailCodes, PurchaseType, LeveType, isAllFieldsConfigRequired, CultureCode);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error Occured in GetAdditionalFieldsBasedOnDocumentType method", ex);
                throw ex;
            }
        }

    public List<AdditionalFieldsDocumentSourceResponse> GetSourceDocumentAdditionalFieldsConfig(List<AdditionalFieldsDocumentSourceInput> additionalFieldsDocumentSourceInputs, string cultureCode = "en-US")
    {
      try
      {
        return GetAdditionalFieldsDao().GetSourceDocumentAdditionalFieldsConfig(additionalFieldsDocumentSourceInputs,cultureCode);
      }
      catch (Exception ex)
      {
        LogHelper.LogError(Log, "Error Occured in GetSourceDocumentAdditionalFieldsConfig method", ex);
        throw ex;
      }
    }

  }
}
