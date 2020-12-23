using GEP.Cumulus.Logging;
using GEP.Cumulus.P2P.BusinessEntities;
using GEP.Cumulus.P2P.Req.DataAccessObjects.SQLServer;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;

namespace GEP.Cumulus.P2P.Req.DataAccessObjects
{
    [ExcludeFromCodeCoverage]
    public class AdditionalFieldsDAO : GEP.Cumulus.P2P.DataAccessObjects.Usability.NewP2PDocumentDAO, IAdditionalFieldsDAO
    {
        private static readonly ILog Log = Logger.GetLog(MethodBase.GetCurrentMethod().DeclaringType);

        public List<PurchaseTypeFeatureMapping> GetAdditionalFieldsByDocumentType(int DocumentTypeCode, long LOBEntityDetailCode, Int16 PurchaseType = 0, Int16 LeveType = 0, bool isAllFieldsConfigRequired = false, string CultureCode = "")
        {
            List<PurchaseTypeFeatureMapping> lstpurchaseTypeFeatureMappings = new List<PurchaseTypeFeatureMapping>();
            try
            {
                DataSet ResultData = FillDataset(ReqSqlConstants.usp_P2P_REQ_GetPurchaseTypeFeatures,
                                                 new SqlParameter[] {
                                                 new SqlParameter("@DocumentType",SqlDbType.Int),
                                                 new SqlParameter("@LOBEntityDetailCode",SqlDbType.BigInt),
                                                 new SqlParameter("@PurchaseTypeId",SqlDbType.SmallInt),
                                                 new SqlParameter("@LevelType",SqlDbType.SmallInt),
                                                 new SqlParameter("@isAllFieldsConfigRequired",SqlDbType.Bit),
                                                 new SqlParameter("@CultureCode",SqlDbType.VarChar)
                                                 },
                                                new object[] { DocumentTypeCode, LOBEntityDetailCode, PurchaseType, LeveType, isAllFieldsConfigRequired , CultureCode }
                                               );

                if (ResultData.Tables.Count > 0 && ResultData.Tables[0].Rows.Count > 0)
                {
                    lstpurchaseTypeFeatureMappings = BindAdditionalFieldsByDocumentTypeData(ResultData);
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogInfo(Log, "Error Occurred in GetAdditionalFieldsByDocumentType Method.");
                throw ex;
            }
            return lstpurchaseTypeFeatureMappings;
        }

        public List<PurchaseTypeFeatureMapping> BindAdditionalFieldsByDocumentTypeData(DataSet ResultData)
        {
            DataTable UpdatedResult = ResultData.Tables[0];
            DataTable ChildAdditionalFieldsData= ResultData.Tables[1];
            List<PurchaseTypeFeatureMapping> lstpurchaseTypeFeatureMappings = new List<PurchaseTypeFeatureMapping>();
            try
            {
                DataTable dtPurchaseTypes = UpdatedResult.DefaultView.ToTable(true, ReqSqlConstants.COL_PURCHASETYPEID);
                DataTable dtFeatures = UpdatedResult.DefaultView.ToTable(true, ReqSqlConstants.COL_FEATUREID, ReqSqlConstants.COL_FEATUREDESCRIPTION, ReqSqlConstants.COL_PURCHASETYPEID);
                foreach (DataRow purchetype in dtPurchaseTypes.Rows)
                {
                    PurchaseTypeFeatureMapping purchaseTypeFeatureMapping = new PurchaseTypeFeatureMapping();
                    purchaseTypeFeatureMapping.lstFeaturesFieldMapping = new List<FeatureFieldMapping>();
                    purchaseTypeFeatureMapping.PurchaseTypeId = ConvertToInt32(purchetype, ReqSqlConstants.COL_PURCHASETYPEID);
                    lstpurchaseTypeFeatureMappings.Add(purchaseTypeFeatureMapping);
                    foreach (DataRow feature in dtFeatures.Rows)
                    {
                        if (ConvertToInt32(feature, ReqSqlConstants.COL_PURCHASETYPEID) == ConvertToInt32(purchetype, ReqSqlConstants.COL_PURCHASETYPEID))
                        {
                            FeatureFieldMapping featureFieldMapping = new FeatureFieldMapping();
                            featureFieldMapping.lstP2PAdditionalFields = new List<P2PAdditionalFieldConfig>();
                            featureFieldMapping.FeatureId = ConvertToInt32(feature, ReqSqlConstants.COL_FEATUREID);
                            featureFieldMapping.FeatureDescription = ConvertToString(feature, ReqSqlConstants.COL_FEATUREDESCRIPTION);
                            purchaseTypeFeatureMapping.lstFeaturesFieldMapping.Add(featureFieldMapping);
                            foreach (DataRow row in UpdatedResult.Rows)
                            {
                                if ((ConvertToInt32(row, ReqSqlConstants.COL_PURCHASETYPEID) == ConvertToInt32(purchetype, ReqSqlConstants.COL_PURCHASETYPEID)) && (ConvertToInt32(row, ReqSqlConstants.COL_FEATUREID) == ConvertToInt32(feature, ReqSqlConstants.COL_FEATUREID)))
                                {
                                    P2PAdditionalFieldConfig additionalFieldConfig = new P2PAdditionalFieldConfig();
                                    additionalFieldConfig.AdditionalFieldID = ConvertToInt32(row, ReqSqlConstants.COL_ADDITIONALFIELDID);
                                    additionalFieldConfig.AdditionalFieldDisplayName = ConvertToString(row, ReqSqlConstants.COL_ADDITIONALFIELDNAME);
                                    additionalFieldConfig.AdditionalFieldTranslatedName = ConvertToString(row, ReqSqlConstants.COL_ADDITIONALFIELDTRANSLATEDNAME);
                                    additionalFieldConfig.DocumentSpecification = ConvertToString(row, ReqSqlConstants.COL_DocumentSpecification);
                                    additionalFieldConfig.FlipDocumentTypes = ConvertToString(row, ReqSqlConstants.COL_FLIPDOCUMENTTYPES);
                                    additionalFieldConfig.PopulateDefault = ConvertToBoolean(row, ReqSqlConstants.COL_POPULATE_DEFAULT);
                                    additionalFieldConfig.FieldControlType = ConvertToInt16(row, ReqSqlConstants.COL_FIELDCONTROLTYPE);
                                    additionalFieldConfig.FieldOrder = ConvertToInt32(row, ReqSqlConstants.COL_FIELDORDER);
                                    additionalFieldConfig.ParentAdditionalFieldId = ConvertToInt32(row, ReqSqlConstants.COL_PARENTADDITIONALFIELDID);
                                    additionalFieldConfig.LevelType = ConvertToInt16(row, ReqSqlConstants.COL_LEVELTYPE);
                                    additionalFieldConfig.IsMappedToOrgEntity = ConvertToBoolean(row, ReqSqlConstants.COL_ISMAPPEDTOORGENTITY);
                                    additionalFieldConfig.EnableShowLookup = ConvertToBoolean(row, ReqSqlConstants.COL_ENABLESHOW_LOOKUP);
                                    additionalFieldConfig.IsVisibleOnExportPDF = ConvertToBoolean(row, ReqSqlConstants.COL_ISVISIBLEONEXPORTPDF);
                                    additionalFieldConfig.DataDisplayStyle = ConvertToInt16(row, ReqSqlConstants.COL_DataDisplayStyle);
                                    List<ChildAdditionalFieldsConfig> lstchildAdditionalFieldConfig = new List<ChildAdditionalFieldsConfig>();
                                    foreach (DataRow ChildAdditionalFieldsrow in ChildAdditionalFieldsData.Rows)
                                    {
                                        if ((ConvertToInt32(row, ReqSqlConstants.COL_ADDITIONALFIELDID) == ConvertToInt32(ChildAdditionalFieldsrow, ReqSqlConstants.COL_ParentlFieldId)) && (ConvertToInt32(row, ReqSqlConstants.COL_PURCHASETYPEID) == ConvertToInt32(ChildAdditionalFieldsrow, ReqSqlConstants.COL_PURCHASETYPEID)) && (ConvertToInt32(row, ReqSqlConstants.COL_FEATUREID) == ConvertToInt32(ChildAdditionalFieldsrow, ReqSqlConstants.COL_FEATUREID)))
                                        {                                           
                                            ChildAdditionalFieldsConfig childAdditionalFieldConfig = new ChildAdditionalFieldsConfig();
                                            childAdditionalFieldConfig.AdditionalFieldID = ConvertToInt32(ChildAdditionalFieldsrow, ReqSqlConstants.COL_ADDITIONALFIELDID);
                                            childAdditionalFieldConfig.AdditionalFieldDisplayName = ConvertToString(ChildAdditionalFieldsrow, ReqSqlConstants.COL_ADDITIONALFIELDNAME);
                                            childAdditionalFieldConfig.AdditionalFieldTranslatedName = ConvertToString(ChildAdditionalFieldsrow, ReqSqlConstants.COL_ADDITIONALFIELDTRANSLATEDNAME);
                                            childAdditionalFieldConfig.DocumentSpecification = ConvertToString(ChildAdditionalFieldsrow, ReqSqlConstants.COL_DocumentSpecification);
                                            childAdditionalFieldConfig.FlipDocumentTypes = ConvertToString(ChildAdditionalFieldsrow, ReqSqlConstants.COL_FLIPDOCUMENTTYPES);
                                            childAdditionalFieldConfig.PopulateDefault = ConvertToBoolean(ChildAdditionalFieldsrow, ReqSqlConstants.COL_POPULATE_DEFAULT);
                                            childAdditionalFieldConfig.FieldControlType = ConvertToInt16(ChildAdditionalFieldsrow, ReqSqlConstants.COL_FIELDCONTROLTYPE);
                                            childAdditionalFieldConfig.FieldOrder = ConvertToInt32(ChildAdditionalFieldsrow, ReqSqlConstants.COL_FIELDORDER);
                                            childAdditionalFieldConfig.ParentAdditionalFieldId = ConvertToInt32(ChildAdditionalFieldsrow, ReqSqlConstants.COL_PARENTADDITIONALFIELDID);
                                            childAdditionalFieldConfig.LevelType = ConvertToInt16(ChildAdditionalFieldsrow, ReqSqlConstants.COL_LEVELTYPE);
                                            childAdditionalFieldConfig.IsMappedToOrgEntity = ConvertToBoolean(ChildAdditionalFieldsrow, ReqSqlConstants.COL_ISMAPPEDTOORGENTITY);
                                            childAdditionalFieldConfig.EnableShowLookup = ConvertToBoolean(ChildAdditionalFieldsrow, ReqSqlConstants.COL_ENABLESHOW_LOOKUP);
                                            childAdditionalFieldConfig.IsVisibleOnExportPDF = ConvertToBoolean(ChildAdditionalFieldsrow, ReqSqlConstants.COL_ISVISIBLEONEXPORTPDF);
                                            childAdditionalFieldConfig.DataDisplayStyle = ConvertToInt16(ChildAdditionalFieldsrow, ReqSqlConstants.COL_DataDisplayStyle);
                                            childAdditionalFieldConfig.FieldSpecification = ConvertToString(ChildAdditionalFieldsrow, ReqSqlConstants.COL_FieldSpecification);
                                            childAdditionalFieldConfig.AdditionalParentFieldDetailCode = ConvertToInt64(ChildAdditionalFieldsrow, ReqSqlConstants.COL_ADDITIONALPARENTFIELDDETAILCODE);
                                            childAdditionalFieldConfig.IsChildAdditionalField = ConvertToBoolean(ChildAdditionalFieldsrow, ReqSqlConstants.COL_IsChildAdditionalField);
                                            
                                            lstchildAdditionalFieldConfig.Add(childAdditionalFieldConfig);
                                        }

                                    }
                                       additionalFieldConfig.lstChildAdditionalFields = lstchildAdditionalFieldConfig;

                                        featureFieldMapping.lstP2PAdditionalFields.Add(additionalFieldConfig);
                                }
                            }

                        }

                    }
                }

            }

            catch (Exception ex)
            {
                LogHelper.LogInfo(Log, "Error Occurred in BindAdditionalFieldsByDocumentTypeData Method.");
                throw ex;
            }
            return lstpurchaseTypeFeatureMappings;
        }

        public List<GEP.Cumulus.P2P.NewBusinessEntities.P2P.Common.P2PAdditionalFieldResult> GetAdditionalFieldMasterData(int AdditionalFieldId, long LOBEntityDetailCode, long AdditionalParentFieldDetailCode = 0, string SearchText = "", int pageIndex = 1, int pageSize = 10)
        {
            List<GEP.Cumulus.P2P.NewBusinessEntities.P2P.Common.P2PAdditionalFieldResult> lstadditionalFieldResult = new List<GEP.Cumulus.P2P.NewBusinessEntities.P2P.Common.P2PAdditionalFieldResult>();
            try
            {
                DataSet ResultData = FillDataset(ReqSqlConstants.USP_P2P_GETALLADDITIONALFIELDVALUES,
                                                 new SqlParameter[] {
                                                 new SqlParameter("@AdditionalFieldId",SqlDbType.Int),
                                                 new SqlParameter("@LOBID",SqlDbType.BigInt),
                                                 new SqlParameter("@AdditionalParentFieldDetailCode",SqlDbType.BigInt),
                                                 new SqlParameter("@SearchText",SqlDbType.NVarChar),
                                                 new SqlParameter("@pageIndex",SqlDbType.Int),
                                                 new SqlParameter("@pageSize",SqlDbType.Int)
                                                 },
                                                new object[] { AdditionalFieldId, LOBEntityDetailCode, AdditionalParentFieldDetailCode, SearchText, pageIndex, pageSize }
                                               );

                if (ResultData.Tables.Count > 0 && ResultData.Tables[0].Rows.Count > 0)
                {
                    lstadditionalFieldResult = BindAdditionalFieldMasterData(ResultData.Tables[0]);
                }

            }
            catch (Exception ex)
            {
                LogHelper.LogInfo(Log, "Error Occurred in GetAdditionalFieldMasterData Method.");
                throw ex;
            }

            return lstadditionalFieldResult;
        }

        public List<GEP.Cumulus.P2P.NewBusinessEntities.P2P.Common.P2PAdditionalFieldResult> BindAdditionalFieldMasterData(DataTable UpdatedResult)
        {
            List<GEP.Cumulus.P2P.NewBusinessEntities.P2P.Common.P2PAdditionalFieldResult> lstadditionalFieldResult = new List<GEP.Cumulus.P2P.NewBusinessEntities.P2P.Common.P2PAdditionalFieldResult>();
            try
            {
                foreach (DataRow row in UpdatedResult.Rows)
                {
                    GEP.Cumulus.P2P.NewBusinessEntities.P2P.Common.P2PAdditionalFieldResult additionalFieldResult = new GEP.Cumulus.P2P.NewBusinessEntities.P2P.Common.P2PAdditionalFieldResult();
                    additionalFieldResult.AdditionalFieldDetailCode = ConvertToInt64(row, ReqSqlConstants.COL_ADDITIONALFIELDDETAILCODE);
                    additionalFieldResult.AdditionalFieldDisplayName = ConvertToString(row, ReqSqlConstants.COL_ADDITIONALFIELDNAME);
                    additionalFieldResult.AdditionalFieldCode = ConvertToString(row, ReqSqlConstants.COL_ADDITIONALFIELDCODE);
                    additionalFieldResult.AdditionalParentFieldDetailCode = ConvertToInt64(row, ReqSqlConstants.COL_ADDITIONALPARENTFIELDDETAILCODE);
                    additionalFieldResult.AdditionalParentFieldDisplayName = ConvertToString(row, ReqSqlConstants.COL_ADDITIONALPARENTFIELDDISPLAYNAME);
                    additionalFieldResult.TotalRecords = ConvertToInt32(row, ReqSqlConstants.COL_TOTALCOUNT);
                    additionalFieldResult.AdditionalFieldID = ConvertToInt32(row, ReqSqlConstants.COL_ADDITIONALFIELDID);
                    additionalFieldResult.IsDefault = ConvertToBoolean(row, ReqSqlConstants.COL_IsDefault);
                    additionalFieldResult.ParentAdditionalFieldId = ConvertToInt32(row, ReqSqlConstants.COL_PARENTADDITIONALFIELDID);
                    additionalFieldResult.AdditionalParentFieldCode = ConvertToString(row, ReqSqlConstants.COL_ADDITIONALPARENTFIELDCODE);
                    lstadditionalFieldResult.Add(additionalFieldResult);
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogInfo(Log, "Error Occurred in BindAdditionalFieldMasterData Method.");
                throw ex;
            }
            return lstadditionalFieldResult;
        }

        public List<GEP.Cumulus.P2P.NewBusinessEntities.P2P.Common.P2PAdditionalFieldsData> GetAdditionalFieldsValues(string AdditionalFieldIds, long LOBEntityDetailCode = 0, int DecumentTypeCode = 0, int FeatureId = 0, Int16 LevelType = 0)
        {
            List<GEP.Cumulus.P2P.NewBusinessEntities.P2P.Common.P2PAdditionalFieldsData> lstP2PAdditionalFieldsData = new List<GEP.Cumulus.P2P.NewBusinessEntities.P2P.Common.P2PAdditionalFieldsData>();
            try
            {
                DataSet ResultData = FillDataset(ReqSqlConstants.USP_P2P_GETADDITIONALFIELDSMASTERDATA,
                                                 new SqlParameter[] {
                                                 new SqlParameter("@AdditionalFieldId",SqlDbType.NVarChar),
                                                 new SqlParameter("@LOBID",SqlDbType.BigInt),
                                                 new SqlParameter("@DecumentTypeCode",SqlDbType.Int),
                                                 new SqlParameter("@FeatureId",SqlDbType.Int),
                                                 new SqlParameter("@LevelType",SqlDbType.TinyInt)
                                                 },
                                                new object[] { AdditionalFieldIds, LOBEntityDetailCode , DecumentTypeCode , FeatureId , LevelType }
                                               );

                if (ResultData.Tables.Count > 0 && ResultData.Tables[0].Rows.Count > 0)
                {
                    lstP2PAdditionalFieldsData = BindAdditionalFieldsValues(ResultData.Tables[0],true);
                }

            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error Occured in GetAdditionalFieldsValues method", ex);
                throw;
            }

            return lstP2PAdditionalFieldsData;
        }


        public List<GEP.Cumulus.P2P.NewBusinessEntities.P2P.Common.P2PAdditionalFieldsData> GetAdditionalFieldsMasterDataByFieldControlType(int DocumentTypeCode, long LOBEntityDetailCode = 0, string FieldControlType = "4")
        {
            List<GEP.Cumulus.P2P.NewBusinessEntities.P2P.Common.P2PAdditionalFieldsData> lstP2PAdditionalFieldsData = new List<GEP.Cumulus.P2P.NewBusinessEntities.P2P.Common.P2PAdditionalFieldsData>();
            try
            {

                DataSet ResultData = FillDataset(ReqSqlConstants.USP_P2P_GETADDITIONALFIELDSDATABYFIELDCONTROLTYPE,
                                                 new SqlParameter[] {
                                                 new SqlParameter("@DocumentType",SqlDbType.Int),
                                                 new SqlParameter("@LOBID",SqlDbType.BigInt),
                                                 new SqlParameter("@FieldControlType",SqlDbType.VarChar)
                                                 },
                                                new object[] { DocumentTypeCode, LOBEntityDetailCode, FieldControlType }
                                               );

                if (ResultData.Tables.Count > 0 && ResultData.Tables[0].Rows.Count > 0)
                {
                    lstP2PAdditionalFieldsData = BindAdditionalFieldsValues(ResultData.Tables[0]);
                }

            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error Occured in GetAdditionalFieldsMasterDataByFieldControlType method", ex);
                throw;
            }


            return lstP2PAdditionalFieldsData;
        }

        public List<GEP.Cumulus.P2P.NewBusinessEntities.P2P.Common.P2PAdditionalFieldsData> BindAdditionalFieldsValues(DataTable UpdatedResult,bool isRequiredFormattedStyleNames=false)
        {
            List<GEP.Cumulus.P2P.NewBusinessEntities.P2P.Common.P2PAdditionalFieldsData> lstP2PAdditionalFieldsData = new List<GEP.Cumulus.P2P.NewBusinessEntities.P2P.Common.P2PAdditionalFieldsData>();
            try
            {

                DataTable dtAdditionalFieldIDs = UpdatedResult.DefaultView.ToTable(true, ReqSqlConstants.COL_ADDITIONALFIELDID, ReqSqlConstants.COL_ADDITIONALFIELD_NAME);
                foreach (DataRow AdditionalFieldID in dtAdditionalFieldIDs.Rows)
                {

                    GEP.Cumulus.P2P.NewBusinessEntities.P2P.Common.P2PAdditionalFieldsData P2PAdditionalFieldsData = new GEP.Cumulus.P2P.NewBusinessEntities.P2P.Common.P2PAdditionalFieldsData();
                    P2PAdditionalFieldsData.lstP2PAdditionalFieldsValues = new List<GEP.Cumulus.P2P.NewBusinessEntities.P2P.Common.P2PAdditionalFieldsValues>();
                    P2PAdditionalFieldsData.AdditionalFieldID = ConvertToInt32(AdditionalFieldID, ReqSqlConstants.COL_ADDITIONALFIELDID);
                    P2PAdditionalFieldsData.AdditionalFieldName = ConvertToString(AdditionalFieldID, ReqSqlConstants.COL_ADDITIONALFIELD_NAME);
                    lstP2PAdditionalFieldsData.Add(P2PAdditionalFieldsData);
                    foreach (DataRow row in UpdatedResult.Rows)
                    {
                        if ((ConvertToInt32(row, ReqSqlConstants.COL_ADDITIONALFIELDID) == ConvertToInt32(AdditionalFieldID, ReqSqlConstants.COL_ADDITIONALFIELDID)))
                        {
                            GEP.Cumulus.P2P.NewBusinessEntities.P2P.Common.P2PAdditionalFieldsValues P2PAdditionalFieldsValues = new GEP.Cumulus.P2P.NewBusinessEntities.P2P.Common.P2PAdditionalFieldsValues();
                            P2PAdditionalFieldsValues.AdditionalFieldDetailCode = ConvertToInt64(row, ReqSqlConstants.COL_ADDITIONALFIELDDETAILCODE);
                            P2PAdditionalFieldsValues.AdditionalFieldCode = ConvertToString(row, ReqSqlConstants.COL_ADDITIONALFIELDCODE);
                            P2PAdditionalFieldsValues.AdditionalFieldDisplayName = ConvertToString(row, ReqSqlConstants.COL_ADDITIONALFIELDNAME);
                            P2PAdditionalFieldsValues.IsDefault = ConvertToBoolean(row, ReqSqlConstants.COL_IsDefault);
                            P2PAdditionalFieldsValues.ParentAdditionalFieldId = ConvertToInt32(row, ReqSqlConstants.COL_PARENTADDITIONALFIELDID);
                            P2PAdditionalFieldsValues.AdditionalParentFieldDetailCode = ConvertToInt64(row, ReqSqlConstants.COL_ADDITIONALPARENTFIELDDETAILCODE);
                            P2PAdditionalFieldsValues.AdditionalParentFieldCode = ConvertToString(row, ReqSqlConstants.COL_ADDITIONALPARENTFIELDCODE);
                            P2PAdditionalFieldsValues.AdditionalParentFieldDisplayName = ConvertToString(row, ReqSqlConstants.COL_ADDITIONALPARENTFIELDDISPLAYNAME);
                            if (isRequiredFormattedStyleNames)
                            {
                                Int16 Displaystyle = ConvertToInt16(row, ReqSqlConstants.COL_DataDisplayStyle);
                                if((Int16)(AdditionalFieldDataDisplayStyle.CodeDescription)==Displaystyle)
                                    P2PAdditionalFieldsValues.AdditionalFieldFormattedName = ConvertToString(row, ReqSqlConstants.COL_ADDITIONALFIELDCODE) + "-" + ConvertToString(row, ReqSqlConstants.COL_ADDITIONALFIELDNAME);

                               else if ((Int16)(AdditionalFieldDataDisplayStyle.DescriptionCode) == Displaystyle)
                                    P2PAdditionalFieldsValues.AdditionalFieldFormattedName = ConvertToString(row, ReqSqlConstants.COL_ADDITIONALFIELDNAME) + "-"+ConvertToString(row, ReqSqlConstants.COL_ADDITIONALFIELDCODE);

                                else if ((Int16)(AdditionalFieldDataDisplayStyle.Code) == Displaystyle)
                                    P2PAdditionalFieldsValues.AdditionalFieldFormattedName = ConvertToString(row, ReqSqlConstants.COL_ADDITIONALFIELDCODE);

                                else if ((Int16)(AdditionalFieldDataDisplayStyle.Description) == Displaystyle)
                                    P2PAdditionalFieldsValues.AdditionalFieldFormattedName = ConvertToString(row, ReqSqlConstants.COL_ADDITIONALFIELDNAME);

                                else if ((Int16)(AdditionalFieldDataDisplayStyle.None) == Displaystyle)
                                    P2PAdditionalFieldsValues.AdditionalFieldFormattedName = "";

                            }
                            P2PAdditionalFieldsData.lstP2PAdditionalFieldsValues.Add(P2PAdditionalFieldsValues);
                        }

                    }

                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error Occured in BindAdditionalFieldsValues method", ex);
                throw ex;
            }
            return lstP2PAdditionalFieldsData;
        }


        public DataSet FillDataset(string SProc, SqlParameter[] Params, object[] Values)
        {
            var sqlHelper = ContextSqlConn;
            SqlConnection sqlCon = (SqlConnection)sqlHelper.CreateConnection();
            sqlCon.Open();
            try
            {
                using (SqlDataAdapter myAdapter = new SqlDataAdapter())
                {
                    myAdapter.SelectCommand = new SqlCommand(SProc, sqlCon);
                    myAdapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                    // assign all parameters with its values
                    for (int input = 0; input < Params.Length; input++)
                    {
                        myAdapter.SelectCommand.Parameters.Add(Params[input]).Value = Values[input];
                    }
                    DataSet myDataSet = new DataSet();
                    myAdapter.Fill(myDataSet);
                    return myDataSet;
                }

            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error Occured in FillDataset method", ex);
                throw ex;
            }
            finally
            {
                if (!ReferenceEquals(sqlCon, null) && sqlCon.State != ConnectionState.Closed)
                {
                    sqlCon.Close();
                    sqlCon.Dispose();
                }

            }
        }

        public List<GEP.Cumulus.P2P.NewBusinessEntities.P2P.Common.P2PAdditionalFieldResult> GetAdditionalFieldData(int AdditionalFieldId, string AdditionalParentFieldDetailCodes = "0", string SearchText = "", int pageIndex = 1, int pageSize = 10, int DecumentTypeCode = 0, int FeatureId = 0, Int16 LevelType = 0)
        {
            List<GEP.Cumulus.P2P.NewBusinessEntities.P2P.Common.P2PAdditionalFieldResult> lstadditionalFieldResult = new List<GEP.Cumulus.P2P.NewBusinessEntities.P2P.Common.P2PAdditionalFieldResult>();
            try
            {
                DataSet ResultData = FillDataset(ReqSqlConstants.USP_P2P_GETALLADDITIONALFIELDDATA,
                                                 new SqlParameter[] {
                                                 new SqlParameter("@AdditionalFieldId",SqlDbType.Int),
                                                 new SqlParameter("@AdditionalParentFieldDetailCodes",SqlDbType.VarChar),
                                                 new SqlParameter("@SearchText",SqlDbType.NVarChar),
                                                 new SqlParameter("@pageIndex",SqlDbType.Int),
                                                 new SqlParameter("@pageSize",SqlDbType.Int),
                                                 new SqlParameter("@DecumentTypeCode",SqlDbType.Int),
                                                 new SqlParameter("@FeatureId",SqlDbType.Int),
                                                 new SqlParameter("@LevelType",SqlDbType.TinyInt)
                                                 },
                                                new object[] { AdditionalFieldId, AdditionalParentFieldDetailCodes, SearchText, pageIndex, pageSize, DecumentTypeCode, FeatureId, LevelType }
                                               );

                if (ResultData.Tables.Count > 0 && ResultData.Tables[0].Rows.Count > 0)
                {
                    lstadditionalFieldResult = BindAdditionalFieldData(ResultData.Tables[0]);
                }

            }
            catch (Exception ex)
            {
                LogHelper.LogInfo(Log, "Error Occurred in GetAdditionalFieldData Method.");
                throw ex;
            }

            return lstadditionalFieldResult;
        }

        public List<GEP.Cumulus.P2P.NewBusinessEntities.P2P.Common.P2PAdditionalFieldResult> BindAdditionalFieldData(DataTable UpdatedResult)
        {
            List<GEP.Cumulus.P2P.NewBusinessEntities.P2P.Common.P2PAdditionalFieldResult> lstadditionalFieldResult = new List<GEP.Cumulus.P2P.NewBusinessEntities.P2P.Common.P2PAdditionalFieldResult>();
            try
            {
                foreach (DataRow row in UpdatedResult.Rows)
                {
                    GEP.Cumulus.P2P.NewBusinessEntities.P2P.Common.P2PAdditionalFieldResult additionalFieldResult = new GEP.Cumulus.P2P.NewBusinessEntities.P2P.Common.P2PAdditionalFieldResult();
                    additionalFieldResult.AdditionalFieldDetailCode = ConvertToInt64(row, ReqSqlConstants.COL_ADDITIONALFIELDDETAILCODE);
                    additionalFieldResult.AdditionalFieldDisplayName = ConvertToString(row, ReqSqlConstants.COL_ADDITIONALFIELDNAME);
                    additionalFieldResult.AdditionalFieldCode = ConvertToString(row, ReqSqlConstants.COL_ADDITIONALFIELDCODE);
                    additionalFieldResult.TotalRecords = ConvertToInt32(row, ReqSqlConstants.COL_TOTALCOUNT);
                    additionalFieldResult.AdditionalFieldID = ConvertToInt32(row, ReqSqlConstants.COL_ADDITIONALFIELDID);
                    additionalFieldResult.IsDefault = ConvertToBoolean(row, ReqSqlConstants.COL_IsDefault);
                    additionalFieldResult.DataDisplayStyle = ConvertToInt16(row, ReqSqlConstants.COL_DataDisplayStyle);
                    lstadditionalFieldResult.Add(additionalFieldResult);
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogInfo(Log, "Error Occurred in BindAdditionalFieldData Method.");
                throw ex;
            }
            return lstadditionalFieldResult;
        }
    public List<GEP.Cumulus.P2P.NewBusinessEntities.P2P.Common.AdditionalFieldDefaultValuesResponse> GetAllChildAdditionalFieldsWithDefaultValues(NewBusinessEntities.P2P.Common.AdditionalFieldDefaultValuesRequest additionalFieldDefaultValuesRequest)
    {
      DataSet result = new DataSet();
      SqlConnection _sqlCon = null;
      SqlTransaction _sqlTrans = null;
      List<GEP.Cumulus.P2P.NewBusinessEntities.P2P.Common.AdditionalFieldDefaultValuesResponse> lstadditionalFieldResult = new List<GEP.Cumulus.P2P.NewBusinessEntities.P2P.Common.AdditionalFieldDefaultValuesResponse>();
      try
      {
        LogHelper.LogInfo(Log, "GetAllChildAdditionalFieldsWithDefaultValues Method Started ");
        var sqlHelper = ContextSqlConn;
        _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
        _sqlCon.Open();
        _sqlTrans = _sqlCon.BeginTransaction();
        
        DataTable dtParentDetails = new DataTable() { Locale = CultureInfo.InvariantCulture, TableName = ReqSqlConstants.TVP_ADDITIONALFIELDPARENTDETAILS };
        dtParentDetails.Columns.Add(ReqSqlConstants.COL_ADDITIONALFIELDTYPEID, typeof(int));
        dtParentDetails.Columns.Add(ReqSqlConstants.COL_ADDITIONALFIELDDETAILCODE, typeof(long));
        dtParentDetails.Columns.Add(ReqSqlConstants.COL_ISORGPARENT, typeof(bool));
        if (additionalFieldDefaultValuesRequest != null && additionalFieldDefaultValuesRequest.AdditionalFieldParentDetails != null)
        {
          foreach (var value in additionalFieldDefaultValuesRequest.AdditionalFieldParentDetails)
          {
            DataRow dr = dtParentDetails.NewRow();
            dr[ReqSqlConstants.COL_ADDITIONALFIELDTYPEID] = value.AdditionalFieldId;
            dr[ReqSqlConstants.COL_ADDITIONALFIELDDETAILCODE] = value.AdditionalFieldDetailCode;
            dr[ReqSqlConstants.COL_ISORGPARENT] = value.IsOrgParent;
            dtParentDetails.Rows.Add(dr);
          }
        }
        using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_GETALLCHILDADDITIONALFIELDWITHDEFAULTVALUES))
        {
          objSqlCommand.CommandType = CommandType.StoredProcedure;
          objSqlCommand.Parameters.AddWithValue("@DocumentType", additionalFieldDefaultValuesRequest.DocumentType);

          objSqlCommand.Parameters.AddWithValue("@LevelType", additionalFieldDefaultValuesRequest.LevelType);
          objSqlCommand.Parameters.AddWithValue("@FeatureId", additionalFieldDefaultValuesRequest.FeatureId);
          SqlParameter objSqlParameter = new SqlParameter("@tvp_AdditionalFieldParentDetails", SqlDbType.Structured)
          {
            TypeName = ReqSqlConstants.TVP_ADDITIONALFIELDPARENTDETAILS,
            Value = dtParentDetails
          };

          objSqlCommand.Parameters.Add(objSqlParameter);
          

          result = sqlHelper.ExecuteDataSet(objSqlCommand);
          if (result.Tables.Count > 0 && result.Tables[0].Rows.Count > 0)
          {
            lstadditionalFieldResult = BindAdditionalFieldChildData(result.Tables[0]);
          }
        }
      }
      catch (Exception ex)
      {
        LogHelper.LogInfo(Log, "Error Occurred in GetAllChildAdditionalFieldsWithDefaultValues Method.");
        throw ex;
      }
      finally
      {
        if (!ReferenceEquals(_sqlCon, null) && _sqlCon.State != ConnectionState.Closed)
        {
          _sqlCon.Close();
          _sqlCon.Dispose();
        }
      }

      return lstadditionalFieldResult;
    }
    public List<GEP.Cumulus.P2P.NewBusinessEntities.P2P.Common.AdditionalFieldDefaultValuesResponse> BindAdditionalFieldChildData(DataTable UpdatedResult)
    {
      List<GEP.Cumulus.P2P.NewBusinessEntities.P2P.Common.AdditionalFieldDefaultValuesResponse> lstadditionalFieldResult = new List<GEP.Cumulus.P2P.NewBusinessEntities.P2P.Common.AdditionalFieldDefaultValuesResponse>();
      try
      {
        foreach (DataRow row in UpdatedResult.Rows)
        {
          GEP.Cumulus.P2P.NewBusinessEntities.P2P.Common.AdditionalFieldDefaultValuesResponse additionalFieldResult = new GEP.Cumulus.P2P.NewBusinessEntities.P2P.Common.AdditionalFieldDefaultValuesResponse();
          additionalFieldResult.ParentAdditionalFieldId = ConvertToInt32(row, ReqSqlConstants.COL_PARENTADDITIONALFIELDTYPEID);
          additionalFieldResult.ParentAdditionalFieldDetailCode = ConvertToInt64(row, ReqSqlConstants.COL_PARENTADDITIONALFIELDDETAILCODE);
          additionalFieldResult.ChildAdditionalFieldId = ConvertToInt32(row, ReqSqlConstants.COL_CHILDFIELDID);
          additionalFieldResult.ChildAdditionalFieldDetailCode = ConvertToInt64(row, ReqSqlConstants.COL_CHILDFIELDDETAILCODE);
          additionalFieldResult.ChildAdditionalFieldCode = ConvertToString(row, ReqSqlConstants.COL_CHILDADDITIONALFIELDCODE);
          additionalFieldResult.ChildAdditionalFieldDisplayName = ConvertToString(row, ReqSqlConstants.COL_CHILDADDITIONALFIELDDISPLAYNAME);
          additionalFieldResult.ChildFieldControlType = ConvertToInt16(row, ReqSqlConstants.COL_CHILDFIELDCONTROLTYPE);
          lstadditionalFieldResult.Add(additionalFieldResult);
        }
      }
      catch (Exception ex)
      {
        LogHelper.LogInfo(Log, "Error Occurred in BindAdditionalFieldChildData Method.");
        throw ex;
      }
      return lstadditionalFieldResult;
    }

        public List<PurchaseTypeFeatureMapping> GetAdditionalFieldsBasedOnDocumentType(int DocumentTypeCode, string LOBEntityDetailCodes, Int16 PurchaseType = 0, Int16 LeveType = 0, bool isAllFieldsConfigRequired = false, string CultureCode = "en-US")
        {
            List<PurchaseTypeFeatureMapping> lstpurchaseTypeFeatureMappings = new List<PurchaseTypeFeatureMapping>();
            try
            {
                DataSet ResultData = FillDataset(ReqSqlConstants.usp_P2P_GetAdditionalFieldsByDocumentType,
                                                 new SqlParameter[] {
                                                 new SqlParameter("@DocumentType",SqlDbType.Int),
                                                 new SqlParameter("@LOBEntityDetailCodes",SqlDbType.VarChar),
                                                 new SqlParameter("@PurchaseTypeId",SqlDbType.SmallInt),
                                                 new SqlParameter("@LevelType",SqlDbType.SmallInt),
                                                 new SqlParameter("@isAllFieldsConfigRequired",SqlDbType.Bit),
                                                 new SqlParameter("@CultureCode",SqlDbType.VarChar)
                                                 },
                                                new object[] { DocumentTypeCode, LOBEntityDetailCodes, PurchaseType, LeveType, isAllFieldsConfigRequired, CultureCode }
                                               );

                if (ResultData.Tables.Count > 0 && ResultData.Tables[0].Rows.Count > 0)
                {
                    lstpurchaseTypeFeatureMappings = BindAdditionalFieldsBasedOnDocumentTypeData(ResultData);
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogInfo(Log, "Error Occurred in GetAdditionalFieldsByDocumentType Method.");
                throw ex;
            }
            return lstpurchaseTypeFeatureMappings;
        }

        public List<PurchaseTypeFeatureMapping> BindAdditionalFieldsBasedOnDocumentTypeData(DataSet ResultData)
        {
            DataTable UpdatedResult = ResultData.Tables[0];
            DataTable ChildAdditionalFieldsData = ResultData.Tables[1];
            List<PurchaseTypeFeatureMapping> lstpurchaseTypeFeatureMappings = new List<PurchaseTypeFeatureMapping>();
            try
            {
                DataTable dtPurchaseTypes = UpdatedResult.DefaultView.ToTable(true, ReqSqlConstants.COL_PURCHASETYPEID);
                DataTable dtFeatures = UpdatedResult.DefaultView.ToTable(true, ReqSqlConstants.COL_FEATUREID, ReqSqlConstants.COL_FEATUREDESCRIPTION, ReqSqlConstants.COL_PURCHASETYPEID, ReqSqlConstants.COL_LOBEntityDetailCode);
                foreach (DataRow purchetype in dtPurchaseTypes.Rows)
                {
                    PurchaseTypeFeatureMapping purchaseTypeFeatureMapping = new PurchaseTypeFeatureMapping();
                    purchaseTypeFeatureMapping.lstFeaturesFieldMapping = new List<FeatureFieldMapping>();
                    purchaseTypeFeatureMapping.PurchaseTypeId = ConvertToInt32(purchetype, ReqSqlConstants.COL_PURCHASETYPEID);
                    lstpurchaseTypeFeatureMappings.Add(purchaseTypeFeatureMapping);
                    foreach (DataRow feature in dtFeatures.Rows)
                    {
                        if (ConvertToInt32(feature, ReqSqlConstants.COL_PURCHASETYPEID) == ConvertToInt32(purchetype, ReqSqlConstants.COL_PURCHASETYPEID))
                        {
                            FeatureFieldMapping featureFieldMapping = new FeatureFieldMapping();
                            featureFieldMapping.lstP2PAdditionalFields = new List<P2PAdditionalFieldConfig>();
                            featureFieldMapping.FeatureId = ConvertToInt32(feature, ReqSqlConstants.COL_FEATUREID);
                            featureFieldMapping.FeatureDescription = ConvertToString(feature, ReqSqlConstants.COL_FEATUREDESCRIPTION);
                           featureFieldMapping.LOBEntityDetailCode = ConvertToInt64(feature, ReqSqlConstants.COL_LOBEntityDetailCode);
                            purchaseTypeFeatureMapping.lstFeaturesFieldMapping.Add(featureFieldMapping);
                            foreach (DataRow row in UpdatedResult.Rows)
                            {
                                if ((ConvertToInt32(row, ReqSqlConstants.COL_PURCHASETYPEID) == ConvertToInt32(purchetype, ReqSqlConstants.COL_PURCHASETYPEID)) && (ConvertToInt32(row, ReqSqlConstants.COL_FEATUREID) == ConvertToInt32(feature, ReqSqlConstants.COL_FEATUREID)) && (ConvertToInt64(row, ReqSqlConstants.COL_LOBEntityDetailCode) == ConvertToInt64(feature, ReqSqlConstants.COL_LOBEntityDetailCode)))
                                {
                                    P2PAdditionalFieldConfig additionalFieldConfig = new P2PAdditionalFieldConfig();
                                    additionalFieldConfig.AdditionalFieldID = ConvertToInt32(row, ReqSqlConstants.COL_ADDITIONALFIELDID);
                                    additionalFieldConfig.AdditionalFieldDisplayName = ConvertToString(row, ReqSqlConstants.COL_ADDITIONALFIELDNAME);
                                    additionalFieldConfig.AdditionalFieldTranslatedName = ConvertToString(row, ReqSqlConstants.COL_ADDITIONALFIELDTRANSLATEDNAME);
                                    additionalFieldConfig.DocumentSpecification = ConvertToString(row, ReqSqlConstants.COL_DocumentSpecification);
                                    additionalFieldConfig.FlipDocumentTypes = ConvertToString(row, ReqSqlConstants.COL_FLIPDOCUMENTTYPES);
                                    additionalFieldConfig.PopulateDefault = ConvertToBoolean(row, ReqSqlConstants.COL_POPULATE_DEFAULT);
                                    additionalFieldConfig.FieldControlType = ConvertToInt16(row, ReqSqlConstants.COL_FIELDCONTROLTYPE);
                                    additionalFieldConfig.FieldOrder = ConvertToInt32(row, ReqSqlConstants.COL_FIELDORDER);
                                    additionalFieldConfig.ParentAdditionalFieldId = ConvertToInt32(row, ReqSqlConstants.COL_PARENTADDITIONALFIELDID);
                                    additionalFieldConfig.LevelType = ConvertToInt16(row, ReqSqlConstants.COL_LEVELTYPE);
                                    additionalFieldConfig.IsMappedToOrgEntity = ConvertToBoolean(row, ReqSqlConstants.COL_ISMAPPEDTOORGENTITY);
                                    additionalFieldConfig.EnableShowLookup = ConvertToBoolean(row, ReqSqlConstants.COL_ENABLESHOW_LOOKUP);
                                    additionalFieldConfig.IsVisibleOnExportPDF = ConvertToBoolean(row, ReqSqlConstants.COL_ISVISIBLEONEXPORTPDF);
                                    additionalFieldConfig.DataDisplayStyle = ConvertToInt16(row, ReqSqlConstants.COL_DataDisplayStyle);
                                    List<ChildAdditionalFieldsConfig> lstchildAdditionalFieldConfig = new List<ChildAdditionalFieldsConfig>();
                                    foreach (DataRow ChildAdditionalFieldsrow in ChildAdditionalFieldsData.Rows)
                                    {
                                        if ((ConvertToInt32(row, ReqSqlConstants.COL_ADDITIONALFIELDID) == ConvertToInt32(ChildAdditionalFieldsrow, ReqSqlConstants.COL_ParentlFieldId)) && (ConvertToInt32(row, ReqSqlConstants.COL_PURCHASETYPEID) == ConvertToInt32(ChildAdditionalFieldsrow, ReqSqlConstants.COL_PURCHASETYPEID)) && (ConvertToInt32(row, ReqSqlConstants.COL_FEATUREID) == ConvertToInt32(ChildAdditionalFieldsrow, ReqSqlConstants.COL_FEATUREID)) && (ConvertToInt64(row, ReqSqlConstants.COL_LOBEntityDetailCode) == ConvertToInt64(feature, ReqSqlConstants.COL_LOBEntityDetailCode)))
                                        {
                                            ChildAdditionalFieldsConfig childAdditionalFieldConfig = new ChildAdditionalFieldsConfig();
                                            childAdditionalFieldConfig.AdditionalFieldID = ConvertToInt32(ChildAdditionalFieldsrow, ReqSqlConstants.COL_ADDITIONALFIELDID);
                                            childAdditionalFieldConfig.AdditionalFieldDisplayName = ConvertToString(ChildAdditionalFieldsrow, ReqSqlConstants.COL_ADDITIONALFIELDNAME);
                                            childAdditionalFieldConfig.AdditionalFieldTranslatedName = ConvertToString(ChildAdditionalFieldsrow, ReqSqlConstants.COL_ADDITIONALFIELDTRANSLATEDNAME);
                                            childAdditionalFieldConfig.DocumentSpecification = ConvertToString(ChildAdditionalFieldsrow, ReqSqlConstants.COL_DocumentSpecification);
                                            childAdditionalFieldConfig.FlipDocumentTypes = ConvertToString(ChildAdditionalFieldsrow, ReqSqlConstants.COL_FLIPDOCUMENTTYPES);
                                            childAdditionalFieldConfig.PopulateDefault = ConvertToBoolean(ChildAdditionalFieldsrow, ReqSqlConstants.COL_POPULATE_DEFAULT);
                                            childAdditionalFieldConfig.FieldControlType = ConvertToInt16(ChildAdditionalFieldsrow, ReqSqlConstants.COL_FIELDCONTROLTYPE);
                                            childAdditionalFieldConfig.FieldOrder = ConvertToInt32(ChildAdditionalFieldsrow, ReqSqlConstants.COL_FIELDORDER);
                                            childAdditionalFieldConfig.ParentAdditionalFieldId = ConvertToInt32(ChildAdditionalFieldsrow, ReqSqlConstants.COL_PARENTADDITIONALFIELDID);
                                            childAdditionalFieldConfig.LevelType = ConvertToInt16(ChildAdditionalFieldsrow, ReqSqlConstants.COL_LEVELTYPE);
                                            childAdditionalFieldConfig.IsMappedToOrgEntity = ConvertToBoolean(ChildAdditionalFieldsrow, ReqSqlConstants.COL_ISMAPPEDTOORGENTITY);
                                            childAdditionalFieldConfig.EnableShowLookup = ConvertToBoolean(ChildAdditionalFieldsrow, ReqSqlConstants.COL_ENABLESHOW_LOOKUP);
                                            childAdditionalFieldConfig.IsVisibleOnExportPDF = ConvertToBoolean(ChildAdditionalFieldsrow, ReqSqlConstants.COL_ISVISIBLEONEXPORTPDF);
                                            childAdditionalFieldConfig.DataDisplayStyle = ConvertToInt16(ChildAdditionalFieldsrow, ReqSqlConstants.COL_DataDisplayStyle);
                                            childAdditionalFieldConfig.FieldSpecification = ConvertToString(ChildAdditionalFieldsrow, ReqSqlConstants.COL_FieldSpecification);
                                            childAdditionalFieldConfig.AdditionalParentFieldDetailCode = ConvertToInt64(ChildAdditionalFieldsrow, ReqSqlConstants.COL_ADDITIONALPARENTFIELDDETAILCODE);
                                            childAdditionalFieldConfig.IsChildAdditionalField = ConvertToBoolean(ChildAdditionalFieldsrow, ReqSqlConstants.COL_IsChildAdditionalField);

                                            lstchildAdditionalFieldConfig.Add(childAdditionalFieldConfig);
                                        }

                                    }
                                    additionalFieldConfig.lstChildAdditionalFields = lstchildAdditionalFieldConfig;

                                    featureFieldMapping.lstP2PAdditionalFields.Add(additionalFieldConfig);
                                }
                            }

                        }

                    }
                }

            }

            catch (Exception ex)
            {
                LogHelper.LogInfo(Log, "Error Occurred in BindAdditionalFieldsByDocumentTypeData Method.");
                throw ex;
            }
            return lstpurchaseTypeFeatureMappings;
        }

    public List<AdditionalFieldsDocumentSourceResponse> BindSourceAdditionalFieldsData(DataSet ResultData)
    {
      DataTable UpdatedResult = ResultData.Tables[0];
      DataTable ChildAdditionalFieldsData = ResultData.Tables[1];
      DataTable dtFeatures = UpdatedResult.DefaultView.ToTable(true, ReqSqlConstants.COL_SOURCEDOCUMENTTYPEID, ReqSqlConstants.COL_FEATUREID, ReqSqlConstants.COL_LEVELTYPE);
      List<AdditionalFieldsDocumentSourceResponse> lstadditionalFieldsDocumentSource = new List<AdditionalFieldsDocumentSourceResponse>();
      foreach (DataRow feature in dtFeatures.Rows)
      {
        AdditionalFieldsDocumentSourceResponse additionalFieldsDocumentSource = new AdditionalFieldsDocumentSourceResponse();
        additionalFieldsDocumentSource.SourceDocumentTypeId = ConvertToInt32(feature, ReqSqlConstants.COL_SOURCEDOCUMENTTYPEID);
         additionalFieldsDocumentSource.FeatureId = ConvertToInt16(feature, ReqSqlConstants.COL_FEATUREID);
        additionalFieldsDocumentSource.LevelType = ConvertToInt16(feature, ReqSqlConstants.COL_LEVELTYPE);
        additionalFieldsDocumentSource.lstP2PAdditionalFields = new List<P2PAdditionalFieldConfig>();
        foreach (DataRow row in UpdatedResult.Rows)
        {
          if ((ConvertToInt32(row, ReqSqlConstants.COL_SOURCEDOCUMENTTYPEID) == ConvertToInt32(feature, ReqSqlConstants.COL_SOURCEDOCUMENTTYPEID)) && (ConvertToInt32(row, ReqSqlConstants.COL_FEATUREID) == ConvertToInt32(feature, ReqSqlConstants.COL_FEATUREID)) && (ConvertToInt64(row, ReqSqlConstants.COL_LEVELTYPE) == ConvertToInt64(feature, ReqSqlConstants.COL_LEVELTYPE))) { 
          P2PAdditionalFieldConfig additionalFieldConfig = new P2PAdditionalFieldConfig();
          additionalFieldConfig.AdditionalFieldID = ConvertToInt32(row, ReqSqlConstants.COL_ADDITIONALFIELDID);
          additionalFieldConfig.AdditionalFieldDisplayName = ConvertToString(row, ReqSqlConstants.COL_ADDITIONALFIELDNAME);
          additionalFieldConfig.AdditionalFieldTranslatedName = ConvertToString(row, ReqSqlConstants.COL_ADDITIONALFIELDTRANSLATEDNAME);
          additionalFieldConfig.DocumentSpecification = ConvertToString(row, ReqSqlConstants.COL_DocumentSpecification);
          additionalFieldConfig.FlipDocumentTypes = ConvertToString(row, ReqSqlConstants.COL_FLIPDOCUMENTTYPES);
          additionalFieldConfig.PopulateDefault = ConvertToBoolean(row, ReqSqlConstants.COL_POPULATE_DEFAULT);
          additionalFieldConfig.FieldControlType = ConvertToInt16(row, ReqSqlConstants.COL_FIELDCONTROLTYPE);
          additionalFieldConfig.FieldOrder = ConvertToInt32(row, ReqSqlConstants.COL_FIELDORDER);
          additionalFieldConfig.ParentAdditionalFieldId = ConvertToInt32(row, ReqSqlConstants.COL_PARENTADDITIONALFIELDID);
          additionalFieldConfig.LevelType = ConvertToInt16(row, ReqSqlConstants.COL_LEVELTYPE);
          additionalFieldConfig.IsMappedToOrgEntity = ConvertToBoolean(row, ReqSqlConstants.COL_ISMAPPEDTOORGENTITY);
          additionalFieldConfig.EnableShowLookup = ConvertToBoolean(row, ReqSqlConstants.COL_ENABLESHOW_LOOKUP);
          additionalFieldConfig.IsVisibleOnExportPDF = ConvertToBoolean(row, ReqSqlConstants.COL_ISVISIBLEONEXPORTPDF);
          additionalFieldConfig.DataDisplayStyle = ConvertToInt16(row, ReqSqlConstants.COL_DataDisplayStyle);

          List<ChildAdditionalFieldsConfig> lstchildAdditionalFieldConfig = new List<ChildAdditionalFieldsConfig>();
          foreach (DataRow ChildAdditionalFieldsrow in ChildAdditionalFieldsData.Rows)
          {
            if ((ConvertToInt32(row, ReqSqlConstants.COL_ADDITIONALFIELDID) == ConvertToInt32(ChildAdditionalFieldsrow, ReqSqlConstants.COL_ParentlFieldId)) && (ConvertToInt32(row, ReqSqlConstants.COL_FEATUREID) == ConvertToInt32(ChildAdditionalFieldsrow, ReqSqlConstants.COL_FEATUREID)))
            {
              ChildAdditionalFieldsConfig childAdditionalFieldConfig = new ChildAdditionalFieldsConfig();
              childAdditionalFieldConfig.AdditionalFieldID = ConvertToInt32(ChildAdditionalFieldsrow, ReqSqlConstants.COL_ADDITIONALFIELDID);
              childAdditionalFieldConfig.AdditionalFieldDisplayName = ConvertToString(ChildAdditionalFieldsrow, ReqSqlConstants.COL_ADDITIONALFIELDNAME);
              childAdditionalFieldConfig.AdditionalFieldTranslatedName = ConvertToString(ChildAdditionalFieldsrow, ReqSqlConstants.COL_ADDITIONALFIELDTRANSLATEDNAME);
              childAdditionalFieldConfig.DocumentSpecification = ConvertToString(ChildAdditionalFieldsrow, ReqSqlConstants.COL_DocumentSpecification);
              childAdditionalFieldConfig.FlipDocumentTypes = ConvertToString(ChildAdditionalFieldsrow, ReqSqlConstants.COL_FLIPDOCUMENTTYPES);
              childAdditionalFieldConfig.PopulateDefault = ConvertToBoolean(ChildAdditionalFieldsrow, ReqSqlConstants.COL_POPULATE_DEFAULT);
              childAdditionalFieldConfig.FieldControlType = ConvertToInt16(ChildAdditionalFieldsrow, ReqSqlConstants.COL_FIELDCONTROLTYPE);
              childAdditionalFieldConfig.FieldOrder = ConvertToInt32(ChildAdditionalFieldsrow, ReqSqlConstants.COL_FIELDORDER);
              childAdditionalFieldConfig.ParentAdditionalFieldId = ConvertToInt32(ChildAdditionalFieldsrow, ReqSqlConstants.COL_PARENTADDITIONALFIELDID);
              childAdditionalFieldConfig.LevelType = ConvertToInt16(ChildAdditionalFieldsrow, ReqSqlConstants.COL_LEVELTYPE);
              childAdditionalFieldConfig.IsMappedToOrgEntity = ConvertToBoolean(ChildAdditionalFieldsrow, ReqSqlConstants.COL_ISMAPPEDTOORGENTITY);
              childAdditionalFieldConfig.EnableShowLookup = ConvertToBoolean(ChildAdditionalFieldsrow, ReqSqlConstants.COL_ENABLESHOW_LOOKUP);
              childAdditionalFieldConfig.IsVisibleOnExportPDF = ConvertToBoolean(ChildAdditionalFieldsrow, ReqSqlConstants.COL_ISVISIBLEONEXPORTPDF);
              childAdditionalFieldConfig.DataDisplayStyle = ConvertToInt16(ChildAdditionalFieldsrow, ReqSqlConstants.COL_DataDisplayStyle);
              childAdditionalFieldConfig.FieldSpecification = ConvertToString(ChildAdditionalFieldsrow, ReqSqlConstants.COL_FieldSpecification);
              childAdditionalFieldConfig.AdditionalParentFieldDetailCode = ConvertToInt64(ChildAdditionalFieldsrow, ReqSqlConstants.COL_ADDITIONALPARENTFIELDDETAILCODE);
              childAdditionalFieldConfig.IsChildAdditionalField = ConvertToBoolean(ChildAdditionalFieldsrow, ReqSqlConstants.COL_IsChildAdditionalField);

              lstchildAdditionalFieldConfig.Add(childAdditionalFieldConfig);
            }
          }
          additionalFieldConfig.lstChildAdditionalFields = lstchildAdditionalFieldConfig;
            additionalFieldsDocumentSource.lstP2PAdditionalFields.Add(additionalFieldConfig);
          }
        }
        lstadditionalFieldsDocumentSource.Add(additionalFieldsDocumentSource);
      }
      return lstadditionalFieldsDocumentSource;
    }

    public List<AdditionalFieldsDocumentSourceResponse> GetSourceDocumentAdditionalFieldsConfig(List<AdditionalFieldsDocumentSourceInput> additionalFieldsDocumentSourceInputs, string cultureCode="en-US")
    {
      DataSet result = new DataSet();
      SqlConnection _sqlCon = null;
      SqlTransaction _sqlTrans = null;
      List<AdditionalFieldsDocumentSourceResponse> additionalFieldsDocumentSourceResponses = new List<AdditionalFieldsDocumentSourceResponse>();
      try
      {
        LogHelper.LogInfo(Log, "GetSourceDocumentAdditionalFieldsConfig Method Started ");
        var sqlHelper = ContextSqlConn;
        _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
        _sqlCon.Open();
        _sqlTrans = _sqlCon.BeginTransaction();

        DataTable dtSourceDetails = new DataTable() { Locale = CultureInfo.InvariantCulture, TableName = ReqSqlConstants.TVP_ADDITIONALFIELDSOURCEDOCUMENTDETAILS };
        dtSourceDetails.Columns.Add(ReqSqlConstants.COL_SOURCEDOCUMENTTYPEID, typeof(int));
        dtSourceDetails.Columns.Add(ReqSqlConstants.COL_FEATUREID, typeof(int));
        dtSourceDetails.Columns.Add(ReqSqlConstants.COL_LEVELTYPE, typeof(int));
        dtSourceDetails.Columns.Add(ReqSqlConstants.COL_ADDITIONALFIELDIDS, typeof(string));
        if (additionalFieldsDocumentSourceInputs != null && additionalFieldsDocumentSourceInputs.Count>0)
        {
          foreach (var value in additionalFieldsDocumentSourceInputs)
          {
            DataRow dr = dtSourceDetails.NewRow();
            dr[ReqSqlConstants.COL_SOURCEDOCUMENTTYPEID] = value.SourceDocumentTypeId;
            dr[ReqSqlConstants.COL_FEATUREID] = value.FeatureId;
            dr[ReqSqlConstants.COL_LEVELTYPE] = value.LevelType;
            dr[ReqSqlConstants.COL_ADDITIONALFIELDIDS] = value.AdditionalFieldIds;
            dtSourceDetails.Rows.Add(dr);
          }
        }
        using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_GETSOURCEDOCUMENTADDITIONALFIELDSCONFIG))
        {
          objSqlCommand.CommandType = CommandType.StoredProcedure;
          objSqlCommand.Parameters.AddWithValue("@CultureCode", cultureCode);

          SqlParameter objSqlParameter = new SqlParameter("@tvp_AdditionalFieldSourceDocumentDetails", SqlDbType.Structured)
          {
            TypeName = ReqSqlConstants.TVP_ADDITIONALFIELDSOURCEDOCUMENTDETAILS,
            Value = dtSourceDetails
          };

          objSqlCommand.Parameters.Add(objSqlParameter);

          result = sqlHelper.ExecuteDataSet(objSqlCommand);
          if (result.Tables.Count > 0 && result.Tables[0].Rows.Count > 0)
          {
            additionalFieldsDocumentSourceResponses = BindSourceAdditionalFieldsData(result);
          }
        }
      }
      catch (Exception ex)
      {
        LogHelper.LogInfo(Log, "Error Occurred in GetSourceDocumentAdditionalFieldsConfig Method.");
        throw ex;
      }
      finally
      {
        if (!ReferenceEquals(_sqlCon, null) && _sqlCon.State != ConnectionState.Closed)
        {
          _sqlCon.Close();
          _sqlCon.Dispose();
        }
      }
      return additionalFieldsDocumentSourceResponses;
    }

  }
}
