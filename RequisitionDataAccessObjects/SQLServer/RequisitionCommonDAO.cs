using GEP.Cumulus.Documents.DataAccessObjects.SQLServer;
using GEP.Cumulus.Logging;
using GEP.Cumulus.P2P.BusinessEntities;
using GEP.Cumulus.P2P.Req.DataAccessObjects.SQLServer;
using log4net;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Collections.Generic;
using Gep.Cumulus.Partner.Entities;

namespace GEP.Cumulus.P2P.Req.DataAccessObjects
{
    [ExcludeFromCodeCoverage]
    public class RequisitionCommonDAO : SQLDocumentDAO, IRequisitionCommonDAO
    {
        private static readonly ILog Log = Logger.GetLog(MethodBase.GetCurrentMethod().DeclaringType);

        public bool UpdateBaseCurrency(long contactCode, long documentCode, decimal documentAmount, int documentTypeId, string toCurrency, decimal conversionFactor)
        {
            bool result = false;
            SqlConnection _sqlCon = null;
            try
            {
                LogHelper.LogInfo(Log, "UpdateBaseCurrency Method Started for documentCode=" + documentCode);
                _sqlCon = (SqlConnection)ContextSqlConn.CreateConnection();

                _sqlCon.Open();
                if (Log.IsDebugEnabled)
                    Log.Debug(string.Format(CultureInfo.InvariantCulture, "CrrencyConversion sp USP_P2P_REQ_UPDATEBASECURRENCY with parameter: documentCode={0} was called.", documentCode));

                result = Convert.ToBoolean(ContextSqlConn.ExecuteScalar(ReqSqlConstants.USP_P2P_REQ_UPDATEBASECURRENCY,
                                                                    new object[] { documentCode, documentAmount, documentTypeId, HtmlEncode(toCurrency), conversionFactor }));
                AddIntoSearchIndexerQueueing(documentCode, documentTypeId, UserContext, GepConfiguration);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in UpdateBaseCurrency method.", ex);
            }
            finally
            {
                if (!ReferenceEquals(_sqlCon, null) && _sqlCon.State != ConnectionState.Closed)
                {
                    _sqlCon.Close();
                    _sqlCon.Dispose();
                }
                LogHelper.LogInfo(Log, "UpdateBaseCurrency Method Ended for documentCode=" + documentCode);
            }
            return result;

        }

        public ItemCharge DeleteItemChargeByItemChargeId(int documentTypeCode, long itemChargeId, int MaxPrecessionValue, int MaxPrecessionValueForTaxesAndCharges, int MaxPrecessionValueforTotal)
        {            
            SqlConnection objSqlCon = null;
            SqlTransaction _sqlTrans = null;
            var sqlHelper = ContextSqlConn;
            ItemCharge objCharge = new ItemCharge();
            RefCountingDataReader dr = null;
            SqlDataReader sqlDr = null;

            try
            {
                objSqlCon = (SqlConnection)ContextSqlConn.CreateConnection();

                string spName = ReqSqlConstants.USP_P2P_DELETEREQUISITIONITEMCHARGEBYID;

                objSqlCon.Open();
                _sqlTrans = objSqlCon.BeginTransaction();
                LogHelper.LogInfo(Log, "DeleteItemChargeByItemChargeId Method Started For itemChargeId = " + itemChargeId.ToString());
                using (var objSqlCommand = new SqlCommand(spName))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.Add(new SqlParameter("@ItemChargeId", itemChargeId));
                    objSqlCommand.Parameters.Add(new SqlParameter("@MaxPrecessionValue", MaxPrecessionValue));
                    objSqlCommand.Parameters.Add(new SqlParameter("@MaxPrecessionValueForTaxAndCharges ", MaxPrecessionValueForTaxesAndCharges));
                    objSqlCommand.Parameters.Add(new SqlParameter("@MaxPrecessionValueTotal ", MaxPrecessionValueforTotal));
                    dr = (RefCountingDataReader)ContextSqlConn.ExecuteReader(objSqlCommand);
                    sqlDr = (SqlDataReader)dr.InnerReader;

                    while (sqlDr.Read())
                    {
                        objCharge.DocumentCode = GetLongValue(sqlDr, ReqSqlConstants.COL_DOCUMENTCODE);
                        objCharge.ItemChargeId = GetLongValue(sqlDr, ReqSqlConstants.COL_ITEMCHARGEID);
                        objCharge.AdditionalCharges = GetDecimalValue(sqlDr, ReqSqlConstants.COL_ADDITIONAL_CHARGES);
                        objCharge.Tax = GetDecimalValue(sqlDr, ReqSqlConstants.COL_TAX);
                        objCharge.TotalCharge = GetDecimalValue(sqlDr, ReqSqlConstants.COL_TOTALCHARGE);
                        objCharge.TotalAllowance = GetDecimalValue(sqlDr, ReqSqlConstants.COL_TOTALALLOWANCE);
                        objCharge.DocumentTotal = GetDecimalValue(sqlDr, ReqSqlConstants.COL_DOCUMENTTOTAL);                       
                    }                    
                }
                if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(objSqlCon, null) && objSqlCon.State == ConnectionState.Open)
                    _sqlTrans.Commit();
            }
            catch (Exception objEx)
            {
                if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(objSqlCon, null) && objSqlCon.State == ConnectionState.Open)
                {
                    try
                    {
                        _sqlTrans.Rollback();
                    }
                    catch (InvalidOperationException error)
                    {
                        if (Log.IsInfoEnabled) Log.Info(error.Message);
                    }
                }
                
                LogHelper.LogError(Log, "Error occurred while Deleting Item Charge for documentTypeCode = " + documentTypeCode + " ,ItemChargeID = " + itemChargeId, objEx);
                throw new Exception("Error occurred while Deleting Charge for ItemChargeID = " + itemChargeId);
            }
            finally
            {
                if (!ReferenceEquals(sqlDr, null) && !sqlDr.IsClosed)
                {
                    sqlDr.Close();
                    sqlDr.Dispose();
                }

                if (!ReferenceEquals(dr, null) && !dr.IsClosed)
                {
                    dr.Close();
                    dr.Dispose();
                }
                if (!ReferenceEquals(objSqlCon, null) && objSqlCon.State != ConnectionState.Closed)
                {
                    objSqlCon.Close();
                    objSqlCon.Dispose();
                }
            }
            return objCharge;
        }

        public List<GEP.NewP2PEntities.ASLValidationResponse> ValidateASL(long orderId, long PartnerCode, long OrderLocationID, long orgEntityDetailCode)
        {
            RefCountingDataReader objRefCountingDataReader = null;
            List<GEP.NewP2PEntities.ASLValidationResponse> lstItems = new List<GEP.NewP2PEntities.ASLValidationResponse>();
            LogHelper.LogInfo(Log, "ValidateASL Method Started");
            try
            {                
                objRefCountingDataReader =
                 (RefCountingDataReader)
                 ContextSqlConn.ExecuteReader(ReqSqlConstants.USP_P2P_VALIDATEASL, orderId, PartnerCode, OrderLocationID, orgEntityDetailCode);
                if (objRefCountingDataReader != null)
                {
                    var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;
                    while (sqlDr.Read())
                    {
                        lstItems.Add(new GEP.NewP2PEntities.ASLValidationResponse
                        {
                            ItemNumber = GetStringValue(sqlDr, ReqSqlConstants.COL_ITEMNUMBER),
                            OrderItemId = GetLongValue(sqlDr, ReqSqlConstants.COL_ORDER_ITEM_ID),
                            ErrorCode = GetTinyIntValue(sqlDr, ReqSqlConstants.COL_ERRORCODE),
                        });
                    }
                    return lstItems;
                }
            }
            finally
            {
                if (!ReferenceEquals(objRefCountingDataReader, null) && !objRefCountingDataReader.IsClosed)
                {
                    objRefCountingDataReader.Close();
                    objRefCountingDataReader.Dispose();
                }                
            }
            return lstItems;
        }

        public List<TaxIntegrationType> GetTaxintegrationByMappedEntity()
        {
            RefCountingDataReader objRefCountingDataReader = null;
            var taxIntegrationType = new List<TaxIntegrationType>();

            try
            {
                var sqlHelper = ContextSqlConn;
                objRefCountingDataReader = (RefCountingDataReader)sqlHelper.ExecuteReader(ReqSqlConstants.USP_P2P_GETTAXINTEGRATIONBYMAPPEDENTITY);
                if (objRefCountingDataReader != null)
                {
                    var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;
                    while (sqlDr.Read())
                    {
                        taxIntegrationType.Add(new TaxIntegrationType
                        {
                            EntityDetailCode = GetLongValue(sqlDr, ReqSqlConstants.COL_ENTITY_DETAIL_CODE),
                            TaxIntegration = GetIntValue(sqlDr, ReqSqlConstants.COL_TAXINTEGRATION),
                            EntityId = GetIntValue(sqlDr, ReqSqlConstants.COL_ENTITYID),
                            LOBEntityDetailCode = GetLongValue(sqlDr, ReqSqlConstants.COL_LOBENTITYDETAILCODE),
                            Division = GetIntValue(sqlDr, ReqSqlConstants.COL_DIVISION),
                            Country = GetStringValue(sqlDr, ReqSqlConstants.COL_COUNTRY),
                            ExternalIntegrationConfig = GetStringValue(sqlDr, ReqSqlConstants.COL_ExternalIntegrationConfig),
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occurred in GetTaxintegrationByMappedEntity Method", ex);
                throw new Exception("Error occurred in GetTaxintegrationByMappedEntity Method");
            }
            finally
            {
                if (!ReferenceEquals(objRefCountingDataReader, null) && !objRefCountingDataReader.IsClosed)
                {
                    objRefCountingDataReader.Close();
                    objRefCountingDataReader.Dispose();
                }                
            }
            return taxIntegrationType;
        }

         public List<ContactInfo> GetPartnerContactsByPartnerCodeandOrderingLocation(long partnerCode, long orderingLocationId, bool flagToFetchContactsOfAllRoles = false)
        {
            RefCountingDataReader objRefCountingDataReader = null;
            List<ContactInfo> lstAllContactInfo = new List<ContactInfo>();
            try
            {
                if (flagToFetchContactsOfAllRoles)
                {
                    objRefCountingDataReader = (RefCountingDataReader)ContextSqlConn.ExecuteReader(ReqSqlConstants.USP_P2P_REQ_GETPARTNERCONTACTSOFROLESBYLOCATIONID, partnerCode, orderingLocationId);
                    if (objRefCountingDataReader != null)
                    {
                        var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;

                        while (sqlDr.Read())
                        {
                            lstAllContactInfo.Add(new ContactInfo
                            {
                                ContactCode = GetLongValue(sqlDr, ReqSqlConstants.COL_CONTACTCODE),
                                EmailID = GetStringValue(sqlDr, ReqSqlConstants.COL_EMAIL_ID),
                                Name = GetStringValue(sqlDr, ReqSqlConstants.COL_NAME),
                                ContactNumber = GetStringValue(sqlDr, ReqSqlConstants.COL_CONTACT_NUMBER),
                                //Assigning IsDefault property to IsPrimary of ContactInfo
                                IsPrimary = Convert.ToBoolean(sqlDr[ReqSqlConstants.COL_ISDEFAULT]),
                                ContactDefaultRoleName = GetStringValue(sqlDr, ReqSqlConstants.COL_CONTACT_DEFAULT_ROLENAME),
                            });
                        }
                        return lstAllContactInfo;
                    }
                }
                else
                {
                    objRefCountingDataReader = (RefCountingDataReader)ContextSqlConn.ExecuteReader(ReqSqlConstants.USP_P2P_GETPARTERCONTACTSBYLOCATIONID, partnerCode, orderingLocationId);
                    if (objRefCountingDataReader != null)
                    {
                        var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;
                        while (sqlDr.Read())
                        {
                            lstAllContactInfo.Add(new ContactInfo
                            {
                                ContactCode = GetLongValue(sqlDr, ReqSqlConstants.COL_CONTACTCODE),
                                EmailID = GetStringValue(sqlDr, ReqSqlConstants.COL_EMAIL_ID),
                                Name = GetStringValue(sqlDr, ReqSqlConstants.COL_NAME),
                                ContactNumber = GetStringValue(sqlDr, ReqSqlConstants.COL_CONTACT_NUMBER),
                            });
                        }
                        return lstAllContactInfo;
                    }
                }

            }
            catch (Exception ex)
            {
                 LogHelper.LogError(Log, "Error occurred in GetPartnerContactsByPartnerCodeandOrderingLocation Method", ex);
                    throw new Exception("Error occurred in GetPartnerContactsByPartnerCodeandOrderingLocation Method");
            }
            finally
            {
                if (!ReferenceEquals(objRefCountingDataReader, null) && !objRefCountingDataReader.IsClosed)
                {
                    objRefCountingDataReader.Close();
                    objRefCountingDataReader.Dispose();
                }
            }
            return new List<ContactInfo>();
        }
    }
}
