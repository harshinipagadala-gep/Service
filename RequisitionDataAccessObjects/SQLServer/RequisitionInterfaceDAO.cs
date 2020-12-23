using Gep.Cumulus.CSM.Entities;
using Gep.Cumulus.ExceptionManager;
using GEP.Cumulus.Documents.DataAccessObjects.SQLServer;
using GEP.Cumulus.Documents.Entities;
using GEP.Cumulus.Logging;
using GEP.Cumulus.P2P.BusinessEntities;
using GEP.Cumulus.P2P.Req.DataAccessObjects.SQLServer;
using GEP.SMART.Storage.AzureSQL;
using log4net;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using SMARTFaultException = Gep.Cumulus.ExceptionManager;

namespace GEP.Cumulus.P2P.Req.DataAccessObjects
{
    [ExcludeFromCodeCoverage]
    public class RequisitionInterfaceDAO : SQLDocumentDAO, IRequisitionInterfaceDAO
    {
        private static readonly ILog Log = Logger.GetLog(MethodBase.GetCurrentMethod().DeclaringType);
        private const Int32 REQ = 7;
        private SQLRequisitionDAO requisitionDAO;

        public DataSet ValidateInterfaceLineStatus(long buyerPartnerCode, DataTable dtRequisitionDetail)
        {
            DataSet dsRequisitionDetail = new DataSet();
            DataTable objDtTableTypeCollection = new DataTable() { Locale = CultureInfo.InvariantCulture };
            LogHelper.LogInfo(Log, "Requisition ValidateInterfaceLineStatus Method Started");
            SqlConnection objSqlCon = null;
            SqlTransaction objSqlTrans = null;
            ReliableSqlDatabase sqlHelper;
            if (ReferenceEquals(null, dtRequisitionDetail))
            {
                if (Log.IsWarnEnabled)
                    Log.Warn(string.Concat("In ValidateInterfaceLineStatus Method of class ", typeof(SQLRequestDAO).ToString(),
                                            "Parameter:  requisitionLineDetail is null"));
                throw new ArgumentNullException("requisitionLineDetail");
            }
            try
            {
                sqlHelper = ContextSqlConn;
                objSqlCon = (SqlConnection)sqlHelper.CreateConnection();

                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_VALIDATEINTERFACELINESTATUS))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.Add(new SqlParameter("@tvp_P2P_RequisitionLineItem", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_P2P_REQUISITIONLINEITEM,
                        Value = dtRequisitionDetail
                    });

                    objSqlCon.Open();
                    objSqlTrans = objSqlCon.BeginTransaction();
                    dsRequisitionDetail = sqlHelper.ExecuteDataSet(objSqlCommand, objSqlTrans);

                    if (!ReferenceEquals(objSqlTrans, null))
                        objSqlTrans.Commit();
                    objSqlCommand.Parameters.Clear();
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in ValidateInterfaceLineStatus method.", ex);
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw();
                throw;
            }
            finally
            {
                if (!ReferenceEquals(objSqlCon, null) && objSqlCon.State != ConnectionState.Closed)
                {
                    objSqlCon.Close();
                    objSqlCon.Dispose();
                }
            }
            return dsRequisitionDetail;
        }

        public bool UpdateLineStatusForRequisitionFromInterface(long RequisitionId, BusinessEntities.StockReservationStatus LineStatus, bool IsUpdateAllItems, List<BusinessEntities.LineStatusRequisition> Items, string StockReservationNumber)
        {
            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;
            try
            {
                LogHelper.LogInfo(Log, "Requisition UpdateLineStatusForRequisitionFromInterface Method Started for DocumentItemId = " + RequisitionId);
                var sqlHelper = ContextSqlConn;
                _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();
                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("Requisition UpdateLineStatusForRequisitionFromInterface sp usp_P2P_REQ_UpdateRequsitionItemStatusFromInterface with parameter: ;=" + RequisitionId + " was called."));
                bool result = false;
                DataTable dtLineStatus = new DataTable();
                dtLineStatus.Columns.Add("LineNumber", typeof(long));
                dtLineStatus.Columns.Add("LineStatus", typeof(long));
                dtLineStatus.Columns.Add("ItemType", typeof(long));
                dtLineStatus.Columns.Add("ReservationNumber", typeof(string));

                if (Items != null && Items.Any())
                {
                    foreach (var item in Items)
                    {
                        DataRow dr = dtLineStatus.NewRow();
                        dr["LineNumber"] = item.LineNumber != 0 ? Convert.ToInt64(item.LineNumber) : 0;
                        dr["LineStatus"] = Convert.ToInt32(item.LineStatus);
                        dr["ItemType"] = Convert.ToInt32(item.ItemType);
                        dr["ReservationNumber"] = Convert.ToString(item.ReservationNumber);
                        dtLineStatus.Rows.Add(dr);
                    }
                }
                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_UPDATEINTERFACEREQUSITIONITEMSTATUS))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.Add(new SqlParameter("@RequisitionId", RequisitionId));
                    objSqlCommand.Parameters.Add(new SqlParameter("@ForAllItemsStatus", (int)LineStatus));
                    objSqlCommand.Parameters.Add(new SqlParameter("@UpdateAllItems", IsUpdateAllItems));
                    objSqlCommand.Parameters.Add(new SqlParameter("@tvpItemStatus", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.tvp_Item_ItemStatus,
                        Value = dtLineStatus
                    });
                    objSqlCommand.Parameters.Add(new SqlParameter("@StockReservationNumber", StockReservationNumber));

                    result = Convert.ToBoolean(sqlHelper.ExecuteNonQuery(objSqlCommand, _sqlTrans), NumberFormatInfo.InvariantInfo);
                    _sqlTrans.Commit();
                }

                if (result && RequisitionId > 0)
                    AddIntoSearchIndexerQueueing(RequisitionId, REQ, UserContext, GepConfiguration);

                return result;
            }
            catch(Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in Requisition UpdateLineStatusForRequisitionFromInterface Method for documentcode = " + RequisitionId, ex);
                if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
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
                throw;
            }
            finally
            {
                if (!ReferenceEquals(_sqlCon, null) && _sqlCon.State != ConnectionState.Closed)
                {
                    _sqlCon.Close();
                    _sqlCon.Dispose();
                }

                LogHelper.LogInfo(Log, "Requisition UpdateLineStatusForRequisitionFromInterface Method Ended for DocumentItemId = " + RequisitionId);
            }
        }
        public List<long> GetRequisitionListForInterfaces(string docType, int docCount, int sourceSystemId)
        {
            RefCountingDataReader objRefCountingDataReader = null;
            List<long> lstRequisitions = new List<long>();
            try
            {
                LogHelper.LogInfo(Log, "Requisition GetRequisitionListForInterfaces Method Started");

                objRefCountingDataReader =
                    (RefCountingDataReader)
                    ContextSqlConn.ExecuteReader(ReqSqlConstants.USP_P2P_REQ_GETREQUISIONSFORINTERFACE,
                                                                    new object[] { docType, docCount, sourceSystemId });
                if (objRefCountingDataReader != null)
                {
                    var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;

                    while (sqlDr.Read())
                    {
                        lstRequisitions.Add(GetLongValue(sqlDr, ReqSqlConstants.COL_DOCUMENT_CODE));
                    }
                }
            }
            catch(Exception ex)
            {
                LogHelper.LogError(Log, "Error Occured in Requisition GetRequisitionListForInterfaces ", ex);
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw();
                throw;
            }
            finally
            {
                if (!ReferenceEquals(objRefCountingDataReader, null) && !objRefCountingDataReader.IsClosed)
                {
                    objRefCountingDataReader.Close();
                    objRefCountingDataReader.Dispose();
                }

                LogHelper.LogInfo(Log, "Requisition GetRequisitionListForInterfaces Method Started");
            }
            return lstRequisitions;
        }
        public List<string> ValidateInterfaceRequisition(Requisition objRequisition, Dictionary<string, string> dctSettings, bool IsOrderingLocationMandatory = false, bool IsDefaultOrderingLocation = false)
        {
            LogHelper.LogInfo(Log, "Order ValidateInterfaceOrder Method Started");
            SqlConnection objSqlCon = null;
            RefCountingDataReader objRefCountingDataReader = null;
            var lstErrors = new List<string>();

            try
            {
                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("In ValidateInterfaceOrder Method.",
                                            "SP: USP_P2P_REQ_VALIDATEINTERFACEDOCUMENT, with parameters: orderId = " + objRequisition.DocumentCode));

                var sqlHelper = ContextSqlConn;
                objSqlCon = (SqlConnection)sqlHelper.CreateConnection();
                objSqlCon.Open();
                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_VALIDATEINTERFACEDOCUMENT))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.AddWithValue("@RequisitionNumber", objRequisition.DocumentNumber);
                    objSqlCommand.Parameters.AddWithValue("@CurrencyCode", objRequisition.Currency ?? string.Empty);
                    objSqlCommand.Parameters.AddWithValue("@BuyerUserId", objRequisition.ClientContactCode ?? string.Empty);
                    objSqlCommand.Parameters.AddWithValue("@IncludeTaxInSplit", Convert.ToBoolean(dctSettings["IncludeTaxInSplit"]));
                    objSqlCommand.Parameters.AddWithValue("@RequisitionStatus", objRequisition.DocumentStatusInfo);
                    objSqlCommand.Parameters.AddWithValue("@ShippingCharge", objRequisition.Shipping == null ? 0 : objRequisition.Shipping);
                    objSqlCommand.Parameters.AddWithValue("@SourceSystemId", !ReferenceEquals(objRequisition.SourceSystemInfo, null) ? objRequisition.SourceSystemInfo.SourceSystemId : 0);
                    objSqlCommand.Parameters.AddWithValue("@FOBCode", objRequisition.FOBCode ?? string.Empty);
                    objSqlCommand.Parameters.AddWithValue("@FOBLocationCode", objRequisition.FOBLocationCode ?? string.Empty);
                    objSqlCommand.Parameters.AddWithValue("@CarriersCode", objRequisition.CarriersCode ?? string.Empty);
                    objSqlCommand.Parameters.AddWithValue("@TransitTypeCode", objRequisition.TransitTypeCode ?? string.Empty);
                    objSqlCommand.Parameters.AddWithValue("@FreightTermsCode", objRequisition.FreightTermsCode ?? string.Empty);
                    objSqlCommand.Parameters.AddWithValue("@LOBValue", objRequisition.DocumentLOBDetails != null ? (objRequisition.DocumentLOBDetails.FirstOrDefault().EntityCode != null ? objRequisition.DocumentLOBDetails.FirstOrDefault().EntityCode : string.Empty) : string.Empty);
                    objSqlCommand.Parameters.AddWithValue("@BilltoLocationId", objRequisition.BilltoLocation != null ? objRequisition.BilltoLocation.BilltoLocationId : 0);
                    objSqlCommand.Parameters.AddWithValue("@ShiptoLocationId", objRequisition.ShiptoLocation != null ? objRequisition.ShiptoLocation.ShiptoLocationId : 0);
                    objSqlCommand.Parameters.AddWithValue("@EntityMappedToBillToLocation", Convert.ToInt64(dctSettings.ContainsKey("EntityMappedToBillToLocation") == true ? dctSettings["EntityMappedToBillToLocation"] : Convert.ToString(0)));
                    objSqlCommand.Parameters.AddWithValue("@EntityMappedToShippingMethods", Convert.ToInt64(dctSettings.ContainsKey("EntityMappedToShippingMethods") == true ? dctSettings["EntityMappedToShippingMethods"] : Convert.ToString(0)));
                    objSqlCommand.Parameters.AddWithValue("@EntityMappedToShipToLocation", Convert.ToInt64(dctSettings.ContainsKey("EntityMappedToShipToLocation") == true ? dctSettings["EntityMappedToShipToLocation"] : Convert.ToString(0)));
                    objSqlCommand.Parameters.AddWithValue("@Operation", objRequisition.Operation ?? string.Empty);
                    objSqlCommand.Parameters.AddWithValue("@SourceSystemName", !ReferenceEquals(objRequisition.SourceSystemInfo, null) ? objRequisition.SourceSystemInfo.SourceSystemName : string.Empty);

                    SqlParameter objSqlParameter = new SqlParameter("@RequisitionItems", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_P2P_REQUISITIONITEM,
                        Value = ConvertRequisitionItemsToTableType(objRequisition.RequisitionItems, objRequisition.CreatedOn)
                    };
                    objSqlCommand.Parameters.Add(objSqlParameter);

                    objSqlParameter = new SqlParameter("@SplitItems", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_P2P_INTERFACESPLITITEMS,
                        Value = ConvertRequisitionSplitsToTableType(objRequisition.RequisitionItems)
                    };
                    objSqlCommand.Parameters.Add(objSqlParameter);

                    objSqlParameter = new SqlParameter("@tvp_P2P_CustomAttributes", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_P2P_CUSTOMATTRIBUTES,
                        Value = dctSettings.ContainsKey("CustomFieldsEnabled") && Convert.ToBoolean(dctSettings["CustomFieldsEnabled"]) == true ?
                         ConvertToCustomAttributesDataTable(objRequisition) : P2P.DataAccessObjects.SQLServer.SQLCommonDAO.GetCustomAttributesDataTable()
                    };
                    objSqlCommand.Parameters.Add(objSqlParameter);
                    objSqlParameter = new SqlParameter("@Tvp_P2P_RequisitionHeaderChargeItem", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_P2P_REQUISITIONHEADERCHARGEITEM,
                        Value = ConvertRequisitionHeaderChargeItemsToTableType(objRequisition)
                    };
                    objSqlCommand.Parameters.Add(objSqlParameter);

                    objSqlParameter = new SqlParameter("@Tvp_P2P_RequisitionLineLevelChargeItem", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_P2P_REQUISITIONLINELEVELCHARGEITEM,
                        Value = ConvertRequisitionLineLevelChargeItemsToTableType(objRequisition.RequisitionItems)
                    };
                    objSqlCommand.Parameters.Add(objSqlParameter);

                    objSqlCommand.Parameters.AddWithValue("@RequesterId", objRequisition.RequesterPASCode ?? string.Empty);
                    objSqlCommand.Parameters.AddWithValue("@PurchaseTypeDescription", objRequisition.PurchaseTypeDescription ?? string.Empty);
                    objSqlCommand.Parameters.AddWithValue("@AllowItemNoFreeText", Convert.ToInt64(dctSettings.ContainsKey("AllowBuyerItemNoFreeText") == true ? dctSettings["AllowBuyerItemNoFreeText"] : Convert.ToString(0)));
                    objSqlCommand.Parameters.AddWithValue("@IsClientCodeBasedonLinkLocation", Convert.ToInt64(dctSettings.ContainsKey("IsClientCodeBasedonLinkLocation") == true ? dctSettings["IsClientCodeBasedonLinkLocation"] : Convert.ToString(0)));
                    objSqlCommand.Parameters.AddWithValue("@ItemMasterEnabled", Convert.ToInt64(dctSettings.ContainsKey("ItemMasterEnabled") == true ? dctSettings["ItemMasterEnabled"] : Convert.ToString(0)));
                    objSqlCommand.Parameters.AddWithValue("@DeriveHeaderEntities", dctSettings.ContainsKey("DeriveHeaderEntities") == true ? dctSettings["DeriveHeaderEntities"] : string.Empty);
                    objSqlCommand.Parameters.AddWithValue("@IsDeriveAccountingBu", Convert.ToBoolean(dctSettings.ContainsKey("IsDeriveAccountingBu") == true ? dctSettings["IsDeriveAccountingBu"] : Convert.ToString(false)));
                    objSqlCommand.Parameters.AddWithValue("@IsDeriveItemDetailEnable", Convert.ToBoolean(dctSettings.ContainsKey("DeriveItemDetails") == true ? dctSettings["DeriveItemDetails"] : Convert.ToString(false)));
                    objSqlCommand.Parameters.AddWithValue("@UseDocumentLOB", Convert.ToBoolean(dctSettings.ContainsKey("UseDocumentLOB") == true ? dctSettings["UseDocumentLOB"] : Convert.ToString(false)));
                    objSqlCommand.Parameters.AddWithValue("@DerivePartnerFromLocationCode", Convert.ToBoolean(dctSettings.ContainsKey("DerivePartnerFromLocationCode") == true ? dctSettings["DerivePartnerFromLocationCode"] : Convert.ToString(false)));
                    objSqlCommand.Parameters.AddWithValue("@AllowReqForRfxAndOrder", Convert.ToBoolean(dctSettings.ContainsKey("AllowReqForRfxAndOrder") == true ? dctSettings["AllowReqForRfxAndOrder"] : Convert.ToString(false)));
                    objSqlCommand.Parameters.AddWithValue("@IsOrderingLocationMandatory", IsOrderingLocationMandatory);
                    objSqlCommand.Parameters.AddWithValue("@IsDefaultOrderingLocation", IsDefaultOrderingLocation);
                    objSqlParameter = new SqlParameter("@RequisitionItemAdditionalFields", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_P2P_BZ_REQUISITIONITEMADDITIONALFIELDS,
                        Value = ConvertRequisitionAdditionalFieldAtrributeToTableType(objRequisition.RequisitionItems)
                    };
                    objSqlCommand.Parameters.Add(objSqlParameter);
                    objSqlParameter = new SqlParameter("@RequisitionHeaderAdditionalFields", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_P2P_BZ_REQUISITIONITEMADDITIONALFIELDS,
                        Value = ConvertAdditionalFieldAtrributeToTableType(0, 0, objRequisition.lstAdditionalFieldAttributues)
                    };
                    objSqlCommand.Parameters.Add(objSqlParameter);
                    objSqlParameter = new SqlParameter("@HeaderEntities", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_P2P_DOCUMENTADDITIONALENTITYINFO,
                        Value = ConvertHeaderEntitiesToTableType(objRequisition.DocumentAdditionalEntitiesInfoList)
                    };
                    objSqlCommand.Parameters.Add(objSqlParameter);

                    objRefCountingDataReader = (RefCountingDataReader)sqlHelper.ExecuteReader(objSqlCommand);
                    if (objRefCountingDataReader != null)
                    {
                        var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;
                        while (sqlDr.Read())
                            lstErrors.Add(GetStringValue(sqlDr, ReqSqlConstants.COL_ERROR_MESSAGE));
                    }
                }
            }
            catch(Exception ex)
            {
                LogHelper.LogError(Log, "Error Occured in Requisition ValidateInterfaceRequisition ", ex);
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw();
                throw;
            }
            finally
            {
                if (!ReferenceEquals(objRefCountingDataReader, null) && !objRefCountingDataReader.IsClosed)
                {
                    objRefCountingDataReader.Close();
                    objRefCountingDataReader.Dispose();
                }
                if (!ReferenceEquals(objSqlCon, null) && objSqlCon.State != ConnectionState.Closed)
                {
                    objSqlCon.Close();
                    objSqlCon.Dispose();
                }
                LogHelper.LogInfo(Log, "Reqisition ValidateInterfaceReqisition Method Ended");
            }
            return lstErrors;
        }

        private DataTable ConvertHeaderEntitiesToTableType(Collection<DocumentAdditionalEntityInfo> HeaderEntity)
        {

            DataTable dtHeaderEntities = new DataTable() { Locale = CultureInfo.InvariantCulture, TableName = ReqSqlConstants.TVP_P2P_DOCUMENTADDITIONALENTITYINFO };
            dtHeaderEntities.Columns.Add("EntityId", typeof(int));
            dtHeaderEntities.Columns.Add("EntityCode", typeof(string));
            dtHeaderEntities.Columns.Add("EntityType", typeof(string));
            dtHeaderEntities.Columns.Add("EntityDetailCode", typeof(long));
            dtHeaderEntities.Columns.Add("EntityDisplayName", typeof(string));
            dtHeaderEntities.Columns.Add("LOBEntityCode", typeof(string));
            dtHeaderEntities.Columns.Add("LOBEntityDetailCode", typeof(long));

            if (HeaderEntity != null && HeaderEntity.Any())
            {
                if (HeaderEntity[0].EntityCode.Contains("|"))    //Validate Req from cXML
                {
                    foreach (var entity in HeaderEntity[0].EntityCode.Split('|'))
                    {
                        DataRow dr = dtHeaderEntities.NewRow();
                        dr["EntityId"] = 0;
                        dr["EntityCode"] = entity;
                        dr["EntityType"] = "";
                        dr["EntityDetailCode"] = 0;
                        dr["EntityDisplayName"] = "";
                        dr["LOBEntityCode"] = "";
                        dr["LOBEntityDetailCode"] = 0;
                        dtHeaderEntities.Rows.Add(dr);
                    }
                }
                else
                {
                    foreach (var entity in HeaderEntity)    //Validate Req from JSON
                    {
                        DataRow dr = dtHeaderEntities.NewRow();
                        dr["EntityId"] = entity.EntityId;
                        dr["EntityCode"] = entity.EntityCode;
                        dr["EntityType"] = entity.EntityType;
                        dr["EntityDetailCode"] = 0;
                        dr["EntityDisplayName"] = entity.EntityDisplayName;
                        dr["LOBEntityCode"] = entity.FieldName;
                        dr["LOBEntityDetailCode"] = 0;
                        dtHeaderEntities.Rows.Add(dr);
                    }
                }
            }
            return dtHeaderEntities;
        }

        public void SaveAdditionalFieldAttributes(long documentID, long documentItemID, List<P2PAdditionalFieldAtrribute> lstAdditionalFieldAttributues, string PurchaseTypeDescription)
        {
            Requisition requisition = new Requisition();
            SqlConnection objSqlConnection = null;
            bool flag = false;

            try
            {
                LogHelper.LogInfo(Log, "Requisition SaveAdditionalFieldAttributes Method Started");

                objSqlConnection = new SqlConnection(ContextSqlConn.ConnectionString);
                objSqlConnection.Open();

                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_SAVEADDITIONALFIELDATTRIBUTEFROMINTERFACE))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.AddWithValue("@id", documentID);
                    objSqlCommand.Parameters.AddWithValue("@documentItemID", documentItemID);
                    SqlParameter objSqlParameter = new SqlParameter("@RequisitionItemAdditionalFields", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_P2P_BZ_REQUISITIONITEMADDITIONALFIELDS,
                        Value = ConvertAdditionalFieldAtrributeToTableType(documentID, documentItemID, lstAdditionalFieldAttributues)
                    };
                    objSqlCommand.Parameters.Add(objSqlParameter);
                    objSqlCommand.Parameters.AddWithValue("@PurchaseType", PurchaseTypeDescription ?? string.Empty);

                    flag = Convert.ToBoolean(ContextSqlConn.ExecuteScalar(objSqlCommand), CultureInfo.InvariantCulture);
                }


            }

            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in SQLRequisitionDAO SaveAdditionalFieldAttributes Method for requisitionId = " + documentID, ex);
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw();
                throw;
            }
            finally
            {
                if (!ReferenceEquals(objSqlConnection, null) && objSqlConnection.State != ConnectionState.Closed)
                {
                    objSqlConnection.Close();
                    objSqlConnection.Dispose();
                }
                LogHelper.LogInfo(Log, "Requisition SaveAdditionalFieldAttributes method completed.");
            }

        }

        public void SaveHeaderAdditionalFieldAttributes(long documentID, List<P2PAdditionalFieldAtrribute> lstAdditionalFieldAttributues, string PurchaseTypeDescription)
        {
            Requisition requisition = new Requisition();
            SqlConnection objSqlConnection = null;
            bool flag = false;

            try
            {
                LogHelper.LogInfo(Log, "Requisition SaveHeaderAdditionalFieldAttributes Method Started");

                objSqlConnection = new SqlConnection(ContextSqlConn.ConnectionString);
                objSqlConnection.Open();

                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_SAVEHEADERADDITIONALFIELDATTRIBUTEFROMINTERFACE))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.AddWithValue("@id", documentID);
                    SqlParameter objSqlParameter = new SqlParameter("@RequisitionHeaderAdditionalFields", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_P2P_BZ_REQUISITIONHEADERADDITIONALFIELDS,
                        Value = ConvertHeaderAdditionalFieldAtrributeToTableType(documentID,lstAdditionalFieldAttributues)
                    };
                    objSqlCommand.Parameters.Add(objSqlParameter);
                    objSqlCommand.Parameters.AddWithValue("@PurchaseType", PurchaseTypeDescription ?? string.Empty);

                    flag = Convert.ToBoolean(ContextSqlConn.ExecuteScalar(objSqlCommand), CultureInfo.InvariantCulture);
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in SQLRequisitionDAO SaveHeaderAdditionalFieldAttributes Method for requisitionId = " + documentID, ex);
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw();
                throw;
            }
            finally
            {
                if (!ReferenceEquals(objSqlConnection, null) && objSqlConnection.State != ConnectionState.Closed)
                {
                    objSqlConnection.Close();
                    objSqlConnection.Dispose();
                }
                LogHelper.LogInfo(Log, "Requisition SaveHeaderAdditionalFieldAttributes method completed.");
            }

        }

        private DataTable ConvertRequisitionAdditionalFieldAtrributeToTableType(List<RequisitionItem> objRequisitionItem)
        {
            DataTable dtAdditionalFieldAttributes = new DataTable() { Locale = CultureInfo.InvariantCulture, TableName = ReqSqlConstants.TVP_P2P_BZ_REQUISITIONITEMADDITIONALFIELDS };
            dtAdditionalFieldAttributes.Columns.Add("RequisitionID", typeof(int));
            dtAdditionalFieldAttributes.Columns.Add("RequisitionItemID", typeof(int));
            dtAdditionalFieldAttributes.Columns.Add("AdditionalFieldID", typeof(int));
            dtAdditionalFieldAttributes.Columns.Add("AdditionalFieldValue", typeof(string));
            dtAdditionalFieldAttributes.Columns.Add("AdditionalFieldName", typeof(string));
            dtAdditionalFieldAttributes.Columns.Add("AdditionalFieldCode", typeof(string));
            dtAdditionalFieldAttributes.Columns.Add("CreatedBy", typeof(long));
            dtAdditionalFieldAttributes.Columns.Add("FeatureId", typeof(int));
            dtAdditionalFieldAttributes.Columns.Add("AdditionalFieldDetailCode", typeof(string));
            dtAdditionalFieldAttributes.Columns.Add("ItemLineNumber", typeof(int));

            if (objRequisitionItem != null && objRequisitionItem.Count() > 0)
            {
                foreach (var RequisitionAdditionalFieldAttribute in objRequisitionItem)
                {
                    if (RequisitionAdditionalFieldAttribute.lstAdditionalFieldAttributues != null && RequisitionAdditionalFieldAttribute.lstAdditionalFieldAttributues.Any())
                    {
                        foreach (var additionalFieldAtrribute in RequisitionAdditionalFieldAttribute.lstAdditionalFieldAttributues)
                        {
                            DataRow dr = dtAdditionalFieldAttributes.NewRow();
                            dr["RequisitionID"] = 0;
                            dr["RequisitionItemID"] = 0;
                            dr["AdditionalFieldID"] = 0;
                            dr["AdditionalFieldValue"] = Convert.ToString(additionalFieldAtrribute.AdditionalFieldValue);
                            dr["AdditionalFieldName"] = Convert.ToString(additionalFieldAtrribute.AdditionalFieldName);
                            dr["AdditionalFieldCode"] = Convert.ToString(additionalFieldAtrribute.AdditionalFieldCode);
                            dr["CreatedBy"] = 0;
                            dr["FeatureId"] = 0;
                            dr["AdditionalFieldDetailCode"] = 0;
                            dr["ItemLineNumber"] = RequisitionAdditionalFieldAttribute.ItemLineNumber;

                            dtAdditionalFieldAttributes.Rows.Add(dr);
                        }
                    }
                }
            }
            return dtAdditionalFieldAttributes;
        }

        private DataTable ConvertAdditionalFieldAtrributeToTableType(long documentID, long documentItemID, List<P2PAdditionalFieldAtrribute> p2PAdditionalFieldAtrributes)
        {
            DataTable dtAdditionalFieldAttributes = new DataTable() { Locale = CultureInfo.InvariantCulture, TableName = ReqSqlConstants.TVP_P2P_BZ_REQUISITIONITEMADDITIONALFIELDS };
            dtAdditionalFieldAttributes.Columns.Add("RequisitionID", typeof(int));
            dtAdditionalFieldAttributes.Columns.Add("RequisitionItemID", typeof(int));
            dtAdditionalFieldAttributes.Columns.Add("AdditionalFieldID", typeof(int));
            dtAdditionalFieldAttributes.Columns.Add("AdditionalFieldValue", typeof(string));
            dtAdditionalFieldAttributes.Columns.Add("AdditionalFieldName", typeof(string));
            dtAdditionalFieldAttributes.Columns.Add("AdditionalFieldCode", typeof(string));
            dtAdditionalFieldAttributes.Columns.Add("CreatedBy", typeof(long));
            dtAdditionalFieldAttributes.Columns.Add("FeatureId", typeof(int));
            dtAdditionalFieldAttributes.Columns.Add("AdditionalFieldDetailCode", typeof(long));
            dtAdditionalFieldAttributes.Columns.Add("ItemLineNumber", typeof(int));

            if (p2PAdditionalFieldAtrributes != null && p2PAdditionalFieldAtrributes.Any())
            {
                foreach (var additionalFieldAtrribute in p2PAdditionalFieldAtrributes)
                {
                    DataRow dr = dtAdditionalFieldAttributes.NewRow();
                    dr["RequisitionID"] = documentID;
                    dr["RequisitionItemID"] = documentItemID;
                    dr["AdditionalFieldID"] = 0;
                    dr["AdditionalFieldValue"] = Convert.ToString(additionalFieldAtrribute.AdditionalFieldValue);
                    dr["AdditionalFieldName"] = Convert.ToString(additionalFieldAtrribute.AdditionalFieldName);
                    dr["AdditionalFieldCode"] = Convert.ToString(additionalFieldAtrribute.AdditionalFieldCode);
                    dr["CreatedBy"] = 0;
                    dr["FeatureId"] = 0;
                    dr["AdditionalFieldDetailCode"] = 0;
                    dr["ItemLineNumber"] = 0;

                    dtAdditionalFieldAttributes.Rows.Add(dr);
                }
            }
            return dtAdditionalFieldAttributes;
        }
        private DataTable ConvertHeaderAdditionalFieldAtrributeToTableType(long documentID, List<P2PAdditionalFieldAtrribute> p2PAdditionalFieldAtrributes)
        {
            DataTable dtAdditionalFieldAttributes = new DataTable() { Locale = CultureInfo.InvariantCulture, TableName = ReqSqlConstants.TVP_P2P_BZ_REQUISITIONHEADERADDITIONALFIELDS };
            dtAdditionalFieldAttributes.Columns.Add("FeatureId", typeof(int));
            dtAdditionalFieldAttributes.Columns.Add("AdditionalFieldID", typeof(int));
            dtAdditionalFieldAttributes.Columns.Add("AdditionalFieldValue", typeof(string));
            dtAdditionalFieldAttributes.Columns.Add("AdditionalFieldName", typeof(string));
            dtAdditionalFieldAttributes.Columns.Add("AdditionalFieldCode", typeof(string));
            dtAdditionalFieldAttributes.Columns.Add("AdditionalFieldDetailCode", typeof(long));
            dtAdditionalFieldAttributes.Columns.Add("CreatedBy", typeof(long));

            if (p2PAdditionalFieldAtrributes != null && p2PAdditionalFieldAtrributes.Any())
            {
                foreach (var additionalFieldAtrribute in p2PAdditionalFieldAtrributes)
                {
                    DataRow dr = dtAdditionalFieldAttributes.NewRow();
                    dr["FeatureId"] = 0;
                    dr["AdditionalFieldID"] = 0;
                    dr["AdditionalFieldValue"] = Convert.ToString(additionalFieldAtrribute.AdditionalFieldValue);
                    dr["AdditionalFieldName"] = Convert.ToString(additionalFieldAtrribute.AdditionalFieldName);
                    dr["AdditionalFieldCode"] = Convert.ToString(additionalFieldAtrribute.AdditionalFieldCode);
                    dr["AdditionalFieldDetailCode"] = 0;
                    dr["CreatedBy"] = 0;

                    dtAdditionalFieldAttributes.Rows.Add(dr);
                }
            }
            return dtAdditionalFieldAttributes;
        }


        private DataTable ConvertRequisitionLineLevelChargeItemsToTableType(List<RequisitionItem> objRequisitionItem)
        {
            DataTable dtLineLevelChargeItems = new DataTable() { Locale = CultureInfo.InvariantCulture, TableName = ReqSqlConstants.TVP_P2P_REQUISITIONLINELEVELCHARGEITEM };
            dtLineLevelChargeItems.Columns.Add("ChargeName", typeof(string));
            dtLineLevelChargeItems.Columns.Add("ChargeAmount", typeof(string));
            dtLineLevelChargeItems.Columns.Add("LineNumber", typeof(string));
            dtLineLevelChargeItems.Columns.Add("IsAllowance", typeof(bool));

            if (objRequisitionItem != null && objRequisitionItem.Count() > 0)
            {
                foreach (var RequisitionLineLevelChargeItem in objRequisitionItem)
                {
                    if (RequisitionLineLevelChargeItem.lstLineItemCharges != null && RequisitionLineLevelChargeItem.lstLineItemCharges.Any())
                    {
                        foreach (var chargeItem in RequisitionLineLevelChargeItem.lstLineItemCharges)
                        {
                            DataRow dr = dtLineLevelChargeItems.NewRow();
                            dr["ChargeName"] = chargeItem.ChargeDetails != null ? chargeItem.ChargeDetails.ChargeName : string.Empty;
                            dr["ChargeAmount"] = chargeItem.ChargeAmount;
                            dr["LineNumber"] = Convert.ToString(RequisitionLineLevelChargeItem.ItemLineNumber);
                            dr["IsAllowance"] = chargeItem.ChargeDetails != null ? chargeItem.ChargeDetails.IsAllowance : false;

                            dtLineLevelChargeItems.Rows.Add(dr);
                        }
                    }
                }
            }
            return dtLineLevelChargeItems;
        }

        private DataTable ConvertRequisitionHeaderChargeItemsToTableType(Requisition objRequisition)
        {
            DataTable dtHeaderChargeItems = new DataTable() { Locale = CultureInfo.InvariantCulture, TableName = ReqSqlConstants.TVP_P2P_REQUISITIONHEADERCHARGEITEM };
            dtHeaderChargeItems.Columns.Add("ChargeName", typeof(string));
            dtHeaderChargeItems.Columns.Add("ChargeAmount", typeof(string));
            dtHeaderChargeItems.Columns.Add("LineNumber", typeof(string));
            dtHeaderChargeItems.Columns.Add("IsAllowance", typeof(bool));

            if (objRequisition.lstItemCharge != null && objRequisition.lstItemCharge.Count() > 0)
            {
                foreach (var RequisitionHeaderChargeItem in objRequisition.lstItemCharge)
                {
                    if (RequisitionHeaderChargeItem != null)
                    {
                        DataRow dr = dtHeaderChargeItems.NewRow();
                        dr["ChargeName"] = RequisitionHeaderChargeItem.ChargeDetails != null ? RequisitionHeaderChargeItem.ChargeDetails.ChargeName : string.Empty;
                        dr["ChargeAmount"] = RequisitionHeaderChargeItem.ChargeAmount;
                        dr["LineNumber"] = Convert.ToString(RequisitionHeaderChargeItem.LineNumber);
                        dr["IsAllowance"] = RequisitionHeaderChargeItem.ChargeDetails != null ? RequisitionHeaderChargeItem.ChargeDetails.IsAllowance : false;

                        dtHeaderChargeItems.Rows.Add(dr);
                    }
                }
            }
            return dtHeaderChargeItems;
        }

        private DataTable ConvertRequisitionItemsToTableType(List<RequisitionItem> lstRequisitionItem, DateTime DateRequested)
        {
            DataTable dtRequisitionItem = new DataTable() { Locale = CultureInfo.InvariantCulture, TableName = ReqSqlConstants.TVP_P2P_REQUISITIONITEM };
            dtRequisitionItem.Columns.Add("ItemLineNumber", typeof(int));
            dtRequisitionItem.Columns.Add("ItemNumber", typeof(string));
            dtRequisitionItem.Columns.Add("UOM", typeof(string));
            dtRequisitionItem.Columns.Add("ClientCategoryId", typeof(string));
            dtRequisitionItem.Columns.Add("Currency", typeof(string));
            dtRequisitionItem.Columns.Add("ClientPartnerCode", typeof(string));
            dtRequisitionItem.Columns.Add("ShippingMethod", typeof(string));
            dtRequisitionItem.Columns.Add("Quantity", typeof(decimal));
            dtRequisitionItem.Columns.Add("UnitPrice", typeof(decimal));
            dtRequisitionItem.Columns.Add("Tax", typeof(decimal));
            dtRequisitionItem.Columns.Add("AdditionalCharges", typeof(decimal));
            dtRequisitionItem.Columns.Add("ShippingCharges", typeof(decimal));
            dtRequisitionItem.Columns.Add("ItemType", typeof(int));
            dtRequisitionItem.Columns.Add("ShipToLocationId", typeof(int));
            dtRequisitionItem.Columns.Add("DateRequested", typeof(DateTime));
            dtRequisitionItem.Columns.Add("DateNeeded", typeof(DateTime));
            dtRequisitionItem.Columns.Add("ClientContactCode", typeof(string));
            dtRequisitionItem.Columns.Add("OrderingLocationCode", typeof(string));
            dtRequisitionItem.Columns.Add("Unspsc", typeof(int));
            if (lstRequisitionItem != null)
            {
                foreach (var reqisitionItem in lstRequisitionItem)
                {
                    DataRow dr = dtRequisitionItem.NewRow();
                    dr["ItemLineNumber"] = reqisitionItem.ItemLineNumber;
                    dr["ItemNumber"] = reqisitionItem.ItemNumber ?? string.Empty;
                    dr["UOM"] = reqisitionItem.UOM;
                    dr["ClientCategoryId"] = reqisitionItem.ClientCategoryId;
                    dr["Currency"] = reqisitionItem.Currency;
                    dr["ClientPartnerCode"] = reqisitionItem.ClientPartnerCode;
                    dr["ShippingMethod"] = reqisitionItem.DocumentItemShippingDetails != null && reqisitionItem.DocumentItemShippingDetails.Any() ? reqisitionItem.DocumentItemShippingDetails.FirstOrDefault().ShippingMethod : string.Empty;
                    dr["Quantity"] = reqisitionItem.Quantity;
                    dr["UnitPrice"] = reqisitionItem.UnitPrice == null ? DBNull.Value : (object)reqisitionItem.UnitPrice;
                    dr["Tax"] = reqisitionItem.Tax == null ? DBNull.Value : (object)reqisitionItem.Tax;
                    dr["AdditionalCharges"] = reqisitionItem.AdditionalCharges == null ? DBNull.Value : (object)reqisitionItem.AdditionalCharges;
                    dr["ShippingCharges"] = reqisitionItem.ShippingCharges == null ? DBNull.Value : (object)reqisitionItem.ShippingCharges;
                    dr["ItemType"] = reqisitionItem.ItemType;
                    dr["ShipToLocationId"] = reqisitionItem.DocumentItemShippingDetails != null && reqisitionItem.DocumentItemShippingDetails.Count() > 0 ? (reqisitionItem.DocumentItemShippingDetails[0].ShiptoLocation != null ? reqisitionItem.DocumentItemShippingDetails[0].ShiptoLocation.ShiptoLocationId : 0) : 0;
                    dr["DateRequested"] = DateRequested == null || DateRequested == DateTime.MinValue ? DBNull.Value : (object)DateRequested;
                    dr["DateNeeded"] = reqisitionItem.DateNeeded == null || reqisitionItem.DateNeeded == DateTime.MinValue ? DBNull.Value : (object)reqisitionItem.DateNeeded;
                    dr["ClientContactCode"] = reqisitionItem.ClientContactCode;
                    dr["OrderingLocationCode"] = !string.IsNullOrEmpty(reqisitionItem.OrderLocationName) ? reqisitionItem.OrderLocationName : string.Empty;
                    dr["Unspsc"] = reqisitionItem.Unspsc;
                    dtRequisitionItem.Rows.Add(dr);
                }
            }
            return dtRequisitionItem;
        }

        private DataTable ConvertRequisitionSplitsToTableType(List<RequisitionItem> lstRequisitionItem)
        {
            DataTable dtSplitItem = new DataTable() { Locale = CultureInfo.InvariantCulture, TableName = ReqSqlConstants.TVP_P2P_INTERFACESPLITITEMS };
            dtSplitItem.Columns.Add("ItemLineNumber", typeof(int));
            dtSplitItem.Columns.Add("EntityCode", typeof(string));
            dtSplitItem.Columns.Add("EntityType", typeof(string));
            dtSplitItem.Columns.Add("SplitItemTotal", typeof(decimal));
            dtSplitItem.Columns.Add("Uids", typeof(int));
            if (lstRequisitionItem != null)
            {
                foreach (var reqisitionItem in lstRequisitionItem)
                {
                    if (reqisitionItem != null && reqisitionItem.ItemSplitsDetail != null)
                    {
                        foreach (var splitItem in reqisitionItem.ItemSplitsDetail)
                        {
                            if (splitItem != null)
                            {
                                foreach (var splitEntity in splitItem.DocumentSplitItemEntities)
                                {
                                    if (splitEntity.EntityType != null && splitEntity.EntityCode != null)
                                    {
                                        DataRow dr = dtSplitItem.NewRow();
                                        dr["ItemLineNumber"] = reqisitionItem.ItemLineNumber;
                                        dr["EntityCode"] = splitEntity.EntityCode;
                                        dr["EntityType"] = splitEntity.EntityType;
                                        dr["SplitItemTotal"] = splitItem.SplitItemTotal == null ? DBNull.Value : (object)splitItem.SplitItemTotal;
                                        dr["Uids"] = splitItem.UiId > 0 ? splitItem.UiId : 0;
                                        dtSplitItem.Rows.Add(dr);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return dtSplitItem;
        }

        public DataTable ConvertToCustomAttributesDataTable(Requisition objRequisition)
        {
            DataTable dtCustomAttributes = new DataTable();
            try
            {
                dtCustomAttributes = P2P.DataAccessObjects.SQLServer.SQLCommonDAO.GetCustomAttributesDataTable();

                P2P.DataAccessObjects.SQLServer.SQLCommonDAO sqlCommonDAO = new P2P.DataAccessObjects.SQLServer.SQLCommonDAO();

                sqlCommonDAO.FillCustomAttributeDataTable(dtCustomAttributes, objRequisition.CustomAttributes, Level.Header);

                if (objRequisition.RequisitionItems != null && objRequisition.RequisitionItems.Count > 0)
                {
                    foreach (RequisitionItem reqItem in objRequisition.RequisitionItems)
                    {
                        sqlCommonDAO.FillCustomAttributeDataTable(dtCustomAttributes, reqItem.CustomAttributes, Level.Item, reqItem.ItemLineNumber);

                        if (reqItem.ItemSplitsDetail != null && reqItem.ItemSplitsDetail.Count > 0)
                        {
                            int splitNum = 0;
                            foreach (RequisitionSplitItems reqSplit in reqItem.ItemSplitsDetail)
                                sqlCommonDAO.FillCustomAttributeDataTable(dtCustomAttributes, reqSplit.CustomAttributes, Level.Distribution, reqItem.ItemLineNumber, ++splitNum);
                        }
                    }
                }
            }
            catch { }

            return dtCustomAttributes;
        }

        //public void ProrateHeaderTaxAndShipping(Requisition objRequisition)
        //{
        //    SqlConnection _sqlCon = null;
        //    SqlTransaction _sqlTrans = null;

        //    LogHelper.LogInfo(Log, "Order ProrateHeaderTaxAndShipping Method Started for Order ID =" + objRequisition.DocumentCode
        //                                        + " Tax " + Convert.ToDecimal(objRequisition.Tax)
        //                                        + " Shipping  " + Convert.ToDecimal(objRequisition.Shipping)
        //                                        + " Precision " + objRequisition.Precision);
        //    if (Log.IsDebugEnabled)
        //        Log.Debug(string.Format(CultureInfo.InvariantCulture, "Order ProrateHeaderTaxAndShipping sp usp_P2P_PO_ProrateHeaderTaxAndShipping with parameter: documentId={0} was called."
        //                                                              , objRequisition.DocumentCode));
        //    try
        //    {

        //        var sqlHelper = ContextSqlConn;
        //        _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
        //        _sqlCon.Open();
        //        _sqlTrans = _sqlCon.BeginTransaction();

        //        sqlHelper.ExecuteNonQuery(ReqSqlConstants.USP_P2P_REQ_PRORATEHEADERTAXANDSHIPPING,
        //                                                                new object[] { objRequisition.DocumentCode,
        //                                                                            objRequisition.Precision ,
        //                                                                            objRequisition.Tax,
        //                                                                            objRequisition.Shipping,
        //                                                                            objRequisition.AdditionalCharges
        //                                                                            });

        //        if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
        //            _sqlTrans.Commit();
        //    }
        //    catch (Exception ex)
        //    {
        //        if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
        //        {
        //            try
        //            {
        //                _sqlTrans.Rollback();

        //            }
        //            catch (InvalidOperationException error)
        //            {
        //                if (Log.IsInfoEnabled) Log.Info(error.Message);
        //            }
        //        }
        //        LogHelper.LogInfo(Log, "Error Occurred in ProrateHeaderTaxAndShipping Method ended for Order ID =" + objRequisition.DocumentCode
        //                               + " Tax " + Convert.ToDecimal(objRequisition.Tax)
        //                               + " Shipping  " + Convert.ToDecimal(objRequisition.Shipping)
        //                               + " Precision " + objRequisition.Precision);
        //        throw ex;
        //    }
        //    finally
        //    {
        //        if (_sqlCon != null && _sqlCon.State != ConnectionState.Closed)
        //        {
        //            _sqlCon.Close();
        //            _sqlCon.Dispose();
        //        }

        //        LogHelper.LogInfo(Log, "Order ProrateHeaderTaxAndShipping Method ended for Order ID =" + objRequisition.DocumentCode
        //                                + " Tax " + Convert.ToDecimal(objRequisition.Tax)
        //                                + " Shipping  " + Convert.ToDecimal(objRequisition.Shipping)
        //                                + " Precision " + objRequisition.Precision);
        //    }
        //}
        public DataTable ValidateReqItemsForExceptionHandling(DataTable dtItemDetails)
        {
            LogHelper.LogInfo(Log, "Requisition ValidateReqItemsForExceptionHandling Method Started");
            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;
            DataTable dt = new DataTable();
            try
            {
                var sqlHelper = ContextSqlConn;
                _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();

                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("Requisition ProprateLineItemTaxandShipping sp usp_P2P_REQ_ProrateLineItemTaxandShipping with parameter: P2pItems=" + dtItemDetails, " was called."));

                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_VALIDATEREQITEMSFOREXCEPTIONHANDLING))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    SqlParameter objSqlParameter = new SqlParameter("@tblReqItemDetails", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_P2P_REQUISITIONITEMDETAILS,
                        Value = dtItemDetails
                    };
                    objSqlCommand.Parameters.Add(objSqlParameter);
                    dt = sqlHelper.ExecuteDataSet(objSqlCommand, _sqlTrans).Tables[0];
                }

                if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
                    _sqlTrans.Commit();
            }
            catch (Exception ex)
            {
                if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
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
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw();
                throw;
            }
            finally
            {
                if (!ReferenceEquals(_sqlCon, null) && _sqlCon.State != ConnectionState.Closed)
                {
                    _sqlCon.Close();
                    _sqlCon.Dispose();
                }

                LogHelper.LogInfo(Log, "Requisition ValidateReqItemsForExceptionHandling Method Ended");
            }
            return dt;
        }
        public DataSet ValidateItemDetailsToBeDerivedFromInterface(string itemNumber, string partnerSourceSystemValue, string uom)
        {
            DataSet ds = new DataSet();
            SqlConnection objSqlCon = null;

            try
            {
                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("In ValidateItemDetailsToBeDerivedFromInterface Method.",
                                            "SP: ValidateItemDetailsToBeDerivedFromInterface"));
                var sqlHelper = ContextSqlConn;
                objSqlCon = (SqlConnection)sqlHelper.CreateConnection();
                objSqlCon.Open();

                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_VALIDATEITEMDETAILSTOBEDERIVEDFROMINTERFACE))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;

                    objSqlCommand.Parameters.AddWithValue("@buyerItemNumber", itemNumber);
                    objSqlCommand.Parameters.AddWithValue("@partnerSourceSystemValue", partnerSourceSystemValue);
                    objSqlCommand.Parameters.AddWithValue("@UOM", uom);
                    ds = sqlHelper.ExecuteDataSet(objSqlCommand);

                    if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                        ds.Tables[0].TableName = "ErrorDetail";
                }
            }
            catch(Exception ex)
            {
                LogHelper.LogError(Log, "Error Occured in Requisition ValidateItemDetailsToBeDerivedFromInterface",ex);
                throw;
            }
            finally
            {
                LogHelper.LogInfo(Log, "Requisition ValidateItemDetailsToBeDerivedFromInterface Method Ended");

                if (objSqlCon != null && objSqlCon.State == ConnectionState.Open)
                    objSqlCon.Close();

            }

            return ds;
        }
        public DataSet ValidateShipToBillToFromInterface(Requisition objRequisition, bool shipToLocSetting, bool billToLocSetting, bool deliverToFreeText, long LobentitydetailCode,
                bool IsDefaultBillToLocation, long entityDetailCode)
        {
            DataSet UpdatedResult = new DataSet();
            SqlConnection objSqlCon = null;
            var lstErrors = new List<string>();
            try
            {
                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("In ValidateShipToBillToFromInterface Method.",
                                            "SP: usp_P2P_ValidateInterfaceDocument, with parameters: orderId = " + objRequisition.DocumentCode));
                string entitycode = objRequisition.DocumentAdditionalEntitiesInfoList != null ? objRequisition.DocumentAdditionalEntitiesInfoList.Where(a => !string.IsNullOrWhiteSpace(a.EntityCode)).FirstOrDefault().EntityCode : "";
                var sqlHelper = ContextSqlConn;
                objSqlCon = (SqlConnection)sqlHelper.CreateConnection();
                objSqlCon.Open();

                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_VALIDATESHIPTOBILLTOFROMINTERFACE))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    SqlParameter objSqlParameter = new SqlParameter("@BillToHeader", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_P2P_HEADERBILLTO,
                        Value = ConvertRequisitionBillToLocToTableType(objRequisition.BilltoLocation)
                    };
                    objSqlCommand.Parameters.Add(objSqlParameter);
                    objSqlParameter = new SqlParameter("@HeaderShipTo", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_P2P_HEADERSHIPTO,
                        Value = P2P.DataAccessObjects.DAOTVPHelper.ConvertOrderShipToLocToTableType(objRequisition.ShiptoLocation)
                    };

                    objSqlCommand.Parameters.Add(objSqlParameter);
                    objSqlParameter = new SqlParameter("@LinelevelShipTo", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_P2P_LINELEVELSHIPTO,
                        Value = ConvertRequisitionLineItemShipToLocToTableType(objRequisition.RequisitionItems)
                    };

                    objSqlCommand.Parameters.Add(objSqlParameter);
                    objSqlParameter = new SqlParameter("@DeliverToHeader", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_P2P_HEADERDELIVERTO,
                        Value = ConvertRequisitionDeliverToLocToTableType(objRequisition.DelivertoLocation)
                    };

                    objSqlCommand.Parameters.Add(objSqlParameter);
                    objSqlParameter = new SqlParameter("@linelevelDeliverTo", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_P2P_LINELEVELDELIVERTO,
                        Value = ConvertRequisitionLineLevelDeliverToLocToTableType(objRequisition.RequisitionItems)
                    };

                    objSqlCommand.Parameters.Add(objSqlParameter);
                    objSqlCommand.Parameters.AddWithValue("@AllowshiptoFreeText", shipToLocSetting);
                    objSqlCommand.Parameters.AddWithValue("@AllowBilltoFreeText", billToLocSetting);
                    objSqlCommand.Parameters.AddWithValue("@AllowDelivertoFreeText", deliverToFreeText);
                    objSqlCommand.Parameters.AddWithValue("@LobEntityDetailCode", LobentitydetailCode);
                    objSqlCommand.Parameters.AddWithValue("@LobEntityCode", objRequisition.DocumentLOBDetails != null ? objRequisition.DocumentLOBDetails[0].EntityCode : "");
                    objSqlCommand.Parameters.AddWithValue("@EntityCode", entitycode);
                    objSqlCommand.Parameters.AddWithValue("@DocumentSource", "Requisition");
                    objSqlCommand.Parameters.AddWithValue("@DefaultBillToLocation", IsDefaultBillToLocation);
                    objSqlCommand.Parameters.AddWithValue("@EntityDetailCodeMappedToBillToLocation", entityDetailCode);

                    UpdatedResult = sqlHelper.ExecuteDataSet(objSqlCommand);
                    UpdatedResult.Tables[0].TableName = "HeaderShiptoDetails";
                    UpdatedResult.Tables[1].TableName = "LineitemShiptoDetails";
                    UpdatedResult.Tables[2].TableName = "HeaderBilltoDetails";
                    UpdatedResult.Tables[3].TableName = "HeaderDelivertoDetails";
                    UpdatedResult.Tables[4].TableName = "LineLevelDelivertoDetails";
                }


            }
            catch(Exception ex)
            {
                LogHelper.LogError(Log, "Requisition ValidateShipToBillToFromInterface Method Ended for orderId=" + objRequisition?.DocumentNumber, ex);
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw();
                throw;
            }
            finally
            {
                LogHelper.LogInfo(Log, "Requisition ValidateShipToBillToFromInterface Method Ended for orderId=" + objRequisition.DocumentNumber);

                if (objSqlCon != null && objSqlCon.State == ConnectionState.Open)
                    objSqlCon.Close();

            }


            return UpdatedResult;

        }

        private DataTable ConvertRequisitionBillToLocToTableType(BilltoLocation Billtolocation)
        {
            DataTable dtRequisitionBillTo = new DataTable() { Locale = CultureInfo.InvariantCulture, TableName = ReqSqlConstants.TVP_P2P_HEADERBILLTO };
            dtRequisitionBillTo.Columns.Add("BilltoLocationNumber", typeof(string));
            dtRequisitionBillTo.Columns.Add("AddressLine1", typeof(string));
            dtRequisitionBillTo.Columns.Add("states", typeof(string));
            dtRequisitionBillTo.Columns.Add("CountryCode", typeof(string));
            dtRequisitionBillTo.Columns.Add("BilltoLocationName", typeof(string));
            dtRequisitionBillTo.Columns.Add("Zip", typeof(string));
            dtRequisitionBillTo.Columns.Add("City", typeof(string));
            dtRequisitionBillTo.Columns.Add("Billtolocid", typeof(int));

            if (Billtolocation != null)
            {

                DataRow dr = dtRequisitionBillTo.NewRow();
                dr["BilltoLocationNumber"] = Billtolocation.BilltoLocationNumber == null ? "" : Billtolocation.BilltoLocationNumber;
                dr["AddressLine1"] = Billtolocation.Address == null ? "" : Billtolocation.Address.AddressLine1 == null ? "" : Billtolocation.Address.AddressLine1;
                dr["states"] = Billtolocation.Address == null ? "" : Billtolocation.Address.State == null ? "" : Billtolocation.Address.State;
                dr["CountryCode"] = Billtolocation.Address == null ? "" : Billtolocation.Address.CountryCode == null ? "" : Billtolocation.Address.CountryCode;
                dr["BilltoLocationName"] = Billtolocation.Address == null ? "" : Billtolocation.BilltoLocationName == null ? "" : Billtolocation.BilltoLocationName;
                dr["Zip"] = Billtolocation.Address == null ? "" : Billtolocation.Address.Zip == null ? "" : Billtolocation.Address.Zip;
                dr["City"] = Billtolocation.Address == null ? "" : Billtolocation.Address.City == null ? "" : Billtolocation.Address.City;
                dr["Billtolocid"] = Billtolocation.BilltoLocationId == null ? 0 : Billtolocation.BilltoLocationId;
                dtRequisitionBillTo.Rows.Add(dr);

            }
            return dtRequisitionBillTo;
        }

        private DataTable ConvertRequisitionLineItemShipToLocToTableType(List<RequisitionItem> lstRequisitionItem)
        {
            DataTable dtRequisitionLineShipTo = new DataTable() { Locale = CultureInfo.InvariantCulture, TableName = ReqSqlConstants.TVP_P2P_LINELEVELSHIPTO };
            dtRequisitionLineShipTo.Columns.Add("ItemLineNumber", typeof(int));
            dtRequisitionLineShipTo.Columns.Add("ShiptoLocationNumber", typeof(string));
            dtRequisitionLineShipTo.Columns.Add("AddressLine1", typeof(string));
            dtRequisitionLineShipTo.Columns.Add("states", typeof(string));
            dtRequisitionLineShipTo.Columns.Add("CountryCode", typeof(string));
            dtRequisitionLineShipTo.Columns.Add("ShiptoLocationName", typeof(string));
            dtRequisitionLineShipTo.Columns.Add("Zip", typeof(string));
            dtRequisitionLineShipTo.Columns.Add("City", typeof(string));
            dtRequisitionLineShipTo.Columns.Add("Shiptolocid", typeof(int));
            dtRequisitionLineShipTo.Columns.Add("UnitPrice", typeof(decimal));
            dtRequisitionLineShipTo.Columns.Add("Quantity", typeof(decimal));

            if (lstRequisitionItem != null)
            {
                foreach (var doc in lstRequisitionItem)
                {
                    ShiptoLocation ShiptoLocation = doc.DocumentItemShippingDetails.Select(x => x.ShiptoLocation).FirstOrDefault();
                    DataRow dr = dtRequisitionLineShipTo.NewRow();
                    dr["ItemLineNumber"] = doc.ItemLineNumber;
                    if (ShiptoLocation != null)
                    {
                        dr["ShiptoLocationNumber"] = ShiptoLocation.ShiptoLocationNumber == null ? "" : ShiptoLocation.ShiptoLocationNumber;
                        dr["AddressLine1"] = ShiptoLocation.Address == null ? "" : ShiptoLocation.Address.AddressLine1 == null ? "" : ShiptoLocation.Address.AddressLine1;
                        dr["states"] = ShiptoLocation.Address == null ? "" : ShiptoLocation.Address.State == null ? "" : ShiptoLocation.Address.State;
                        dr["CountryCode"] = ShiptoLocation.Address == null ? "" : ShiptoLocation.Address.CountryCode == null ? "" : ShiptoLocation.Address.CountryCode;
                        dr["ShiptoLocationName"] = ShiptoLocation.Address == null ? "" : ShiptoLocation.ShiptoLocationName == null ? "" : ShiptoLocation.ShiptoLocationName;
                        dr["Zip"] = ShiptoLocation.Address == null ? "" : ShiptoLocation.Address.Zip == null ? "" : ShiptoLocation.Address.Zip;
                        dr["City"] = ShiptoLocation.Address == null ? "" : ShiptoLocation.Address.City == null ? "" : ShiptoLocation.Address.City;
                        dr["Shiptolocid"] = ShiptoLocation.ShiptoLocationId == null ? 0 : ShiptoLocation.ShiptoLocationId;
                    }
                    /*For Line Level ShipTo Validation to get skip for Cancelled Items*/
                    dr["UnitPrice"] = doc.UnitPrice != null ? doc.UnitPrice : 0; ;
                    dr["Quantity"] = doc.Quantity;

                    dtRequisitionLineShipTo.Rows.Add(dr);
                }
            }
            return dtRequisitionLineShipTo;
        }
        private DataTable ConvertRequisitionDeliverToLocToTableType(DelivertoLocation DelivertoLocation)
        {
            DataTable dtRequisitiondeliverTo = new DataTable() { Locale = CultureInfo.InvariantCulture, TableName = ReqSqlConstants.TVP_P2P_HEADERDELIVERTO };
            dtRequisitiondeliverTo.Columns.Add("DelivertoLocationNumber", typeof(string));
            dtRequisitiondeliverTo.Columns.Add("AddressLine1", typeof(string));
            dtRequisitiondeliverTo.Columns.Add("AddressLine2", typeof(string));
            dtRequisitiondeliverTo.Columns.Add("AddressLine3", typeof(string));
            dtRequisitiondeliverTo.Columns.Add("Country", typeof(string));
            dtRequisitiondeliverTo.Columns.Add("StateCode", typeof(string));
            dtRequisitiondeliverTo.Columns.Add("StateName", typeof(string));
            dtRequisitiondeliverTo.Columns.Add("FaxNo", typeof(string));
            dtRequisitiondeliverTo.Columns.Add("EmailAddress", typeof(string));
            dtRequisitiondeliverTo.Columns.Add("CountryName", typeof(string));
            dtRequisitiondeliverTo.Columns.Add("ISDCountryCode", typeof(string));
            dtRequisitiondeliverTo.Columns.Add("AreaCode", typeof(string));
            dtRequisitiondeliverTo.Columns.Add("states", typeof(string));
            dtRequisitiondeliverTo.Columns.Add("CountryCode", typeof(string));
            dtRequisitiondeliverTo.Columns.Add("DelivertoLocationName", typeof(string));
            dtRequisitiondeliverTo.Columns.Add("Zip", typeof(string));
            dtRequisitiondeliverTo.Columns.Add("City", typeof(string));
            dtRequisitiondeliverTo.Columns.Add("Delivertolocid", typeof(int));

            if (DelivertoLocation != null && !string.IsNullOrEmpty(DelivertoLocation.DelivertoLocationNumber))
            {


                DataRow dr = dtRequisitiondeliverTo.NewRow();
                dr["DelivertoLocationNumber"] = DelivertoLocation.DelivertoLocationNumber == null ? "" : DelivertoLocation.DelivertoLocationNumber;
                dr["AddressLine1"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.Address.AddressLine1 == null ? "" : DelivertoLocation.Address.AddressLine1;
                dr["AddressLine2"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.Address.AddressLine2 == null ? "" : DelivertoLocation.Address.AddressLine2;
                dr["AddressLine3"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.Address.AddressLine3 == null ? "" : DelivertoLocation.Address.AddressLine3;
                dr["Country"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.Address.Country == null ? "" : DelivertoLocation.Address.Country;
                dr["StateCode"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.Address.State == null ? "" : DelivertoLocation.Address.State;
                dr["StateName"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.Address.StateName == null ? "" : DelivertoLocation.Address.StateName;
                dr["FaxNo"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.Address.FaxNo == null ? "" : DelivertoLocation.Address.FaxNo;
                dr["EmailAddress"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.Address.EmailAddress == null ? "" : DelivertoLocation.Address.EmailAddress;
                dr["CountryName"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.Address.CountryName == null ? "" : DelivertoLocation.Address.CountryName;
                dr["ISDCountryCode"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.Address.ISDCountryCode == null ? "" : DelivertoLocation.Address.ISDCountryCode;
                dr["AreaCode"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.Address.AreaCode == null ? "" : DelivertoLocation.Address.AreaCode;
                dr["states"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.Address.State == null ? "" : DelivertoLocation.Address.State;
                dr["CountryCode"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.Address.CountryCode == null ? "" : DelivertoLocation.Address.CountryCode;
                dr["DelivertoLocationName"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.DelivertoLocationName == null ? "" : DelivertoLocation.DelivertoLocationName;
                dr["Zip"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.Address.Zip == null ? "" : DelivertoLocation.Address.Zip;
                dr["City"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.Address.City == null ? "" : DelivertoLocation.Address.City;
                dr["Delivertolocid"] = DelivertoLocation.DelivertoLocationId == 0 ? 0 : DelivertoLocation.DelivertoLocationId;
                dtRequisitiondeliverTo.Rows.Add(dr);

            }
            return dtRequisitiondeliverTo;
        }
        private DataTable ConvertRequisitionLineLevelDeliverToLocToTableType(List<RequisitionItem> lstRequisitionItem)
        {
            DataTable dtRequisitionLineItemdeliverTo = new DataTable() { Locale = CultureInfo.InvariantCulture, TableName = ReqSqlConstants.TVP_P2P_LINELEVELDELIVERTO };
            dtRequisitionLineItemdeliverTo.Columns.Add("ItemLineNumber", typeof(int));
            dtRequisitionLineItemdeliverTo.Columns.Add("DelivertoLocationNumber", typeof(string));
            dtRequisitionLineItemdeliverTo.Columns.Add("AddressLine1", typeof(string));
            dtRequisitionLineItemdeliverTo.Columns.Add("AddressLine2", typeof(string));
            dtRequisitionLineItemdeliverTo.Columns.Add("AddressLine3", typeof(string));
            dtRequisitionLineItemdeliverTo.Columns.Add("Country", typeof(string));
            dtRequisitionLineItemdeliverTo.Columns.Add("StateCode", typeof(string));
            dtRequisitionLineItemdeliverTo.Columns.Add("StateName", typeof(string));
            dtRequisitionLineItemdeliverTo.Columns.Add("FaxNo", typeof(string));
            dtRequisitionLineItemdeliverTo.Columns.Add("EmailAddress", typeof(string));
            dtRequisitionLineItemdeliverTo.Columns.Add("CountryName", typeof(string));
            dtRequisitionLineItemdeliverTo.Columns.Add("ISDCountryCode", typeof(string));
            dtRequisitionLineItemdeliverTo.Columns.Add("AreaCode", typeof(string));
            dtRequisitionLineItemdeliverTo.Columns.Add("states", typeof(string));
            dtRequisitionLineItemdeliverTo.Columns.Add("CountryCode", typeof(string));
            dtRequisitionLineItemdeliverTo.Columns.Add("DelivertoLocationName", typeof(string));
            dtRequisitionLineItemdeliverTo.Columns.Add("Zip", typeof(string));
            dtRequisitionLineItemdeliverTo.Columns.Add("City", typeof(string));
            dtRequisitionLineItemdeliverTo.Columns.Add("Delivertolocid", typeof(int));


            if (lstRequisitionItem != null)
            {
                foreach (var doc in lstRequisitionItem)
                {
                    DelivertoLocation DelivertoLocation = doc.DocumentItemShippingDetails.Select(x => x.DelivertoLocation).FirstOrDefault();
                    DataRow dr = dtRequisitionLineItemdeliverTo.NewRow();
                    dr["ItemLineNumber"] = doc.ItemLineNumber;
                    if (DelivertoLocation != null && !string.IsNullOrEmpty(DelivertoLocation.DelivertoLocationNumber))
                    {

                        dr["DelivertoLocationNumber"] = DelivertoLocation.DelivertoLocationNumber == null ? "" : DelivertoLocation.DelivertoLocationNumber;
                        dr["AddressLine1"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.Address.AddressLine1 == null ? "" : DelivertoLocation.Address.AddressLine1;
                        dr["AddressLine2"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.Address.AddressLine2 == null ? "" : DelivertoLocation.Address.AddressLine2;
                        dr["AddressLine3"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.Address.AddressLine3 == null ? "" : DelivertoLocation.Address.AddressLine3;
                        dr["Country"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.Address.Country == null ? "" : DelivertoLocation.Address.Country;
                        dr["StateCode"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.Address.State == null ? "" : DelivertoLocation.Address.State;
                        dr["StateName"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.Address.StateName == null ? "" : DelivertoLocation.Address.StateName;
                        dr["FaxNo"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.Address.FaxNo == null ? "" : DelivertoLocation.Address.FaxNo;
                        dr["EmailAddress"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.Address.EmailAddress == null ? "" : DelivertoLocation.Address.EmailAddress;
                        dr["CountryName"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.Address.CountryName == null ? "" : DelivertoLocation.Address.CountryName;
                        dr["ISDCountryCode"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.Address.ISDCountryCode == null ? "" : DelivertoLocation.Address.ISDCountryCode;
                        dr["AreaCode"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.Address.AreaCode == null ? "" : DelivertoLocation.Address.AreaCode;
                        dr["states"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.Address.State == null ? "" : DelivertoLocation.Address.State;
                        dr["CountryCode"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.Address.CountryCode == null ? "" : DelivertoLocation.Address.CountryCode;
                        dr["DelivertoLocationName"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.DelivertoLocationName == null ? "" : DelivertoLocation.DelivertoLocationName;
                        dr["Zip"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.Address.Zip == null ? "" : DelivertoLocation.Address.Zip;
                        dr["City"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.Address.City == null ? "" : DelivertoLocation.Address.City;
                        dr["Delivertolocid"] = DelivertoLocation.DelivertoLocationId == 0 ? 0 : DelivertoLocation.DelivertoLocationId;

                    }
                    dtRequisitionLineItemdeliverTo.Rows.Add(dr);
                }
            }
            return dtRequisitionLineItemdeliverTo;
        }


        public List<RequisitionItem> GetLineItemBasicDetailsForInterface(long documentCode)
        {
            Decimal tempDecimal;
            List<RequisitionItem> objReqItemList = new List<RequisitionItem>();
            try
            {
                LogHelper.LogInfo(Log, "Requisition GetLineItemBasicDetailsForInterface Method Started for id=" + documentCode);
                if (documentCode > 0)
                {
                    if (Log.IsDebugEnabled)
                        Log.Debug(string.Concat("In Requisition GetLineItemBasicDetails Method",
                                                 "SP: usp_P2P_REQ_GetLineItemsForInterface with parameter: documentCode=" + documentCode + " was called."));

                    var reqItemsDataSet = ContextSqlConn.ExecuteDataSet(ReqSqlConstants.USP_P2P_REQ_GETREQLINEITEMS_FOR_INTERFACE,
                                                                         new object[] { documentCode });


                    if (reqItemsDataSet.Tables.Count > 0 && reqItemsDataSet.Tables[0].Rows.Count > 0)
                    {
                        foreach (DataRow reqItemDataRow in reqItemsDataSet.Tables[0].Rows)
                        {
                            var objRequisitionItem = new RequisitionItem();

                            #region Basic Line Level Details
                            objRequisitionItem.DocumentItemId = Convert.ToInt64(reqItemDataRow[ReqSqlConstants.COL_REQUISITION_ITEM_ID]);
                            objRequisitionItem.P2PLineItemId = Convert.ToInt64(reqItemDataRow[ReqSqlConstants.COL_P2P_LINE_ITEM_ID]);
                            objRequisitionItem.DocumentId = Convert.ToInt64(reqItemDataRow[ReqSqlConstants.COL_REQUISITION_ID]);
                            objRequisitionItem.ShortName = Convert.ToString(reqItemDataRow[ReqSqlConstants.COL_SHORT_NAME]);
                            objRequisitionItem.Description = Convert.ToString(reqItemDataRow[ReqSqlConstants.COL_DESCRIPTION]);
                            objRequisitionItem.UnitPrice = Decimal.TryParse(reqItemDataRow[ReqSqlConstants.COL_UNIT_PRICE].ToString(), out tempDecimal) ? tempDecimal : default(decimal?);
                            objRequisitionItem.Quantity = Convert.ToDecimal(reqItemDataRow[ReqSqlConstants.COL_QUANTITY]);
                            objRequisitionItem.UOM = Convert.ToString(reqItemDataRow[ReqSqlConstants.COL_UOM]);
                            objRequisitionItem.DateRequested = !Convert.IsDBNull(reqItemDataRow[ReqSqlConstants.COL_DATE_REQUESTED]) ? Convert.ToDateTime(reqItemDataRow[ReqSqlConstants.COL_DATE_REQUESTED]) : default(DateTime?);
                            objRequisitionItem.DateNeeded = !Convert.IsDBNull(reqItemDataRow[ReqSqlConstants.COL_DATE_NEEDED]) ? Convert.ToDateTime(reqItemDataRow[ReqSqlConstants.COL_DATE_NEEDED]) : default(DateTime?);
                            objRequisitionItem.CategoryId = Convert.ToInt64(reqItemDataRow[ReqSqlConstants.COL_CATEGORY_ID]);
                            objRequisitionItem.ItemType = (ItemType)Convert.ToInt16(reqItemDataRow[ReqSqlConstants.COL_ITEM_TYPE_ID]);
                            objRequisitionItem.CreatedBy = Convert.ToInt64(reqItemDataRow[ReqSqlConstants.COL_CREATED_BY]);
                            objRequisitionItem.ItemStatus = (DocumentStatus)Convert.ToInt16(reqItemDataRow[ReqSqlConstants.COL_ITEM_STATUS]);
                            objRequisitionItem.Currency = Convert.ToString(reqItemDataRow[ReqSqlConstants.COL_CURRENCY]);
                            objRequisitionItem.StartDate = !Convert.IsDBNull(reqItemDataRow[ReqSqlConstants.COL_START_DATE]) ? Convert.ToDateTime(reqItemDataRow[ReqSqlConstants.COL_START_DATE]) : default(DateTime?);
                            objRequisitionItem.EndDate = !Convert.IsDBNull(reqItemDataRow[ReqSqlConstants.COL_END_DATE]) ? Convert.ToDateTime(reqItemDataRow[ReqSqlConstants.COL_END_DATE]) : default(DateTime?);
                            objRequisitionItem.AdditionalCharges = Decimal.TryParse(reqItemDataRow[ReqSqlConstants.COL_ADDITIONAL_CHARGES].ToString(), out tempDecimal) ? tempDecimal : default(decimal?);
                            objRequisitionItem.ShippingCharges = Decimal.TryParse(reqItemDataRow[ReqSqlConstants.COL_SHIPPING_CHARGES].ToString(), out tempDecimal) ? tempDecimal : default(decimal?);
                            objRequisitionItem.Tax = Decimal.TryParse(reqItemDataRow[ReqSqlConstants.COL_LINE_ITEM_TAX].ToString(), out tempDecimal) ? tempDecimal : default(decimal?);
                            objRequisitionItem.SourceType = (ItemSourceType)Convert.ToInt16(reqItemDataRow[ReqSqlConstants.COL_SOURCE_TYPE]);
                            objRequisitionItem.ItemCode = Convert.ToInt64(reqItemDataRow[ReqSqlConstants.COL_ITEM_CODE]);
                            objRequisitionItem.CatalogItemId = !Convert.IsDBNull(reqItemDataRow[ReqSqlConstants.COL_CATALOGITEMID]) ? Convert.ToInt64(reqItemDataRow[ReqSqlConstants.COL_CATALOGITEMID]) : default(Int64);
                            objRequisitionItem.SupplierPartId = Convert.ToString(reqItemDataRow[ReqSqlConstants.COL_SUPPLIERPARTID]);
                            objRequisitionItem.SupplierPartAuxiliaryId = Convert.ToString(reqItemDataRow[ReqSqlConstants.COL_SUPPLIERAUXILIARYPARTID]);
                            objRequisitionItem.ItemExtendedType = (ItemExtendedType)Convert.ToInt16(reqItemDataRow[ReqSqlConstants.COL_ITEM_EXTENDED_TYPE]);
                            objRequisitionItem.ItemNumber = Convert.ToString(reqItemDataRow[ReqSqlConstants.COL_ITEMNUMBER]);
                            objRequisitionItem.ItemLineNumber = Convert.ToInt64(reqItemDataRow[ReqSqlConstants.COL_ITEMLINENUMBER]);
                            objRequisitionItem.ItemTotalAmount = (Decimal.TryParse(reqItemDataRow[ReqSqlConstants.COL_UNIT_PRICE].ToString(), out tempDecimal) ? tempDecimal : default(decimal?)) * Convert.ToDecimal(reqItemDataRow[ReqSqlConstants.COL_QUANTITY]);
                            objRequisitionItem.PartnerCode = !Convert.IsDBNull(reqItemDataRow[ReqSqlConstants.COL_PARTNER_CODE]) ? Convert.ToDecimal(reqItemDataRow[ReqSqlConstants.COL_PARTNER_CODE]) : default(decimal);
                            objRequisitionItem.PartnerName = Convert.ToString(reqItemDataRow[ReqSqlConstants.COL_PARTNER_NAME]);
                            objRequisitionItem.CategoryName = Convert.ToString(reqItemDataRow[ReqSqlConstants.COL_CATEGORY_NAME]);
                            objRequisitionItem.RecoupmentPercentage = Decimal.TryParse(reqItemDataRow[ReqSqlConstants.COL_RECOUPMENTPERCENTAGE].ToString(), out tempDecimal) ? tempDecimal : default(decimal);
                            objRequisitionItem.ManufacturerName = Convert.ToString(reqItemDataRow[ReqSqlConstants.COL_MANUFACTURER_NAME]);
                            objRequisitionItem.ManufacturerPartNumber = Convert.ToString(reqItemDataRow[ReqSqlConstants.COL_MANUFACTURER_PART_NUMBER]);
                            objRequisitionItem.ClientPartnerCode = Convert.ToString(reqItemDataRow[ReqSqlConstants.COL_CLIENT_PARTNERCODE]);
                            objRequisitionItem.InventoryType = Convert.IsDBNull(reqItemDataRow[ReqSqlConstants.COL_INVENTORYTYPE]) ? false : Convert.ToBoolean(reqItemDataRow[ReqSqlConstants.COL_INVENTORYTYPE]);
                            #endregion Basic Line Level Details

                            objReqItemList.Add(objRequisitionItem);
                        }
                    }
                    return objReqItemList;
                }
                else
                {
                    if (Log.IsWarnEnabled)
                        Log.Warn("In Requisition GetLineItemBasicDetailsForInterface Method Parameter: Document Code should be greater than 0.");
                }
            }
            finally
            {
                LogHelper.LogInfo(Log, "Requisition GetLineItemBasicDetailsForInterface Method Ended for id=" + documentCode);
            }
            return new List<RequisitionItem>();
        }
        public List<string> ValidateErrorBasedInterfaceRequisition(Requisition objRequisition, Dictionary<string, string> dctSettings)
        {
            LogHelper.LogInfo(Log, "Requisition ValidateErrorBasedInterfaceRequisition Method Started");
            SqlConnection objSqlCon = null;
            RefCountingDataReader objRefCountingDataReader = null;
            var lstErrors = new List<string>();

            try
            {
                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("In ValidateErrorBasedInterfaceRequisition Method.",
                                            "SP: usp_P2P_REQ_ValidateErrorBasedInterfaceDocument, with parameters: requisitionId = " + objRequisition.DocumentCode));

                var sqlHelper = ContextSqlConn;
                objSqlCon = (SqlConnection)sqlHelper.CreateConnection();
                objSqlCon.Open();
                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_VALIDATEERRORBASEDINTERFACEDOCUMENT))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.AddWithValue("@RequisitionNumber", objRequisition.DocumentNumber);
                    objSqlCommand.Parameters.AddWithValue("@CurrencyCode", objRequisition.Currency ?? string.Empty);
                    objSqlCommand.Parameters.AddWithValue("@BuyerUserId", objRequisition.ClientContactCode ?? string.Empty);
                    objSqlCommand.Parameters.AddWithValue("@IncludeTaxInSplit", Convert.ToBoolean(dctSettings["IncludeTaxInSplit"]));
                    objSqlCommand.Parameters.AddWithValue("@RequisitionStatus", objRequisition.DocumentStatusInfo);
                    objSqlCommand.Parameters.AddWithValue("@ShippingCharge", objRequisition.Shipping == null ? 0 : objRequisition.Shipping);
                    objSqlCommand.Parameters.AddWithValue("@SourceSystemId", !ReferenceEquals(objRequisition.SourceSystemInfo, null) ? objRequisition.SourceSystemInfo.SourceSystemId : 0);
                    objSqlCommand.Parameters.AddWithValue("@FOBCode", objRequisition.FOBCode ?? string.Empty);
                    objSqlCommand.Parameters.AddWithValue("@FOBLocationCode", objRequisition.FOBLocationCode ?? string.Empty);
                    objSqlCommand.Parameters.AddWithValue("@CarriersCode", objRequisition.CarriersCode ?? string.Empty);
                    objSqlCommand.Parameters.AddWithValue("@TransitTypeCode", objRequisition.TransitTypeCode ?? string.Empty);
                    objSqlCommand.Parameters.AddWithValue("@FreightTermsCode", objRequisition.FreightTermsCode ?? string.Empty);
                    objSqlCommand.Parameters.AddWithValue("@LOBValue", objRequisition.DocumentLOBDetails != null ? (objRequisition.DocumentLOBDetails.FirstOrDefault().EntityCode != null ? objRequisition.DocumentLOBDetails.FirstOrDefault().EntityCode : string.Empty) : string.Empty);
                    objSqlCommand.Parameters.AddWithValue("@BilltoLocationId", objRequisition.BilltoLocation != null ? objRequisition.BilltoLocation.BilltoLocationId : 0);
                    objSqlCommand.Parameters.AddWithValue("@ShiptoLocationId", objRequisition.ShiptoLocation != null ? objRequisition.ShiptoLocation.ShiptoLocationId : 0);
                    objSqlCommand.Parameters.AddWithValue("@EntityMappedToBillToLocation", Convert.ToInt64(dctSettings.ContainsKey("EntityMappedToBillToLocation") == true ? dctSettings["EntityMappedToBillToLocation"] : Convert.ToString(0)));
                    objSqlCommand.Parameters.AddWithValue("@EntityMappedToShippingMethods", Convert.ToInt64(dctSettings.ContainsKey("EntityMappedToShippingMethods") == true ? dctSettings["EntityMappedToShippingMethods"] : Convert.ToString(0)));
                    objSqlCommand.Parameters.AddWithValue("@EntityMappedToShipToLocation", Convert.ToInt64(dctSettings.ContainsKey("EntityMappedToShipToLocation") == true ? dctSettings["EntityMappedToShipToLocation"] : Convert.ToString(0)));
                    objSqlCommand.Parameters.AddWithValue("@Operation", objRequisition.Operation ?? string.Empty);
                    objSqlCommand.Parameters.AddWithValue("@SourceSystemName", !ReferenceEquals(objRequisition.SourceSystemInfo, null) ? objRequisition.SourceSystemInfo.SourceSystemName : string.Empty);

                    SqlParameter objSqlParameter = new SqlParameter("@RequisitionItems", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_P2P_REQUISITIONITEM,
                        Value = ConvertRequisitionItemsToTableType(objRequisition.RequisitionItems, objRequisition.CreatedOn)
                    };
                    objSqlCommand.Parameters.Add(objSqlParameter);

                    objSqlParameter = new SqlParameter("@SplitItems", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_P2P_INTERFACESPLITITEMS,
                        Value = ConvertRequisitionSplitsToTableType(objRequisition.RequisitionItems)
                    };
                    objSqlCommand.Parameters.Add(objSqlParameter);

                    objSqlParameter = new SqlParameter("@tvp_P2P_CustomAttributes", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_P2P_CUSTOMATTRIBUTES,
                        Value = dctSettings.ContainsKey("CustomFieldsEnabled") && Convert.ToBoolean(dctSettings["CustomFieldsEnabled"]) == true ?
                         ConvertToCustomAttributesDataTable(objRequisition) : P2P.DataAccessObjects.SQLServer.SQLCommonDAO.GetCustomAttributesDataTable()
                    };
                    objSqlCommand.Parameters.Add(objSqlParameter);
                    objSqlParameter = new SqlParameter("@Tvp_P2P_RequisitionHeaderChargeItem", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_P2P_REQUISITIONHEADERCHARGEITEM,
                        Value = ConvertRequisitionHeaderChargeItemsToTableType(objRequisition)
                    };
                    objSqlCommand.Parameters.Add(objSqlParameter);

                    objSqlParameter = new SqlParameter("@Tvp_P2P_RequisitionLineLevelChargeItem", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_P2P_REQUISITIONLINELEVELCHARGEITEM,
                        Value = ConvertRequisitionLineLevelChargeItemsToTableType(objRequisition.RequisitionItems)
                    };
                    objSqlCommand.Parameters.Add(objSqlParameter);

                    objSqlCommand.Parameters.AddWithValue("@RequesterId", objRequisition.RequesterPASCode ?? string.Empty);
                    objSqlCommand.Parameters.AddWithValue("@PurchaseTypeDescription", objRequisition.PurchaseTypeDescription ?? string.Empty);
                    objSqlCommand.Parameters.AddWithValue("@AllowItemNoFreeText", Convert.ToInt64(dctSettings.ContainsKey("AllowBuyerItemNoFreeText") == true ? dctSettings["AllowBuyerItemNoFreeText"] : Convert.ToString(0)));
                    objSqlCommand.Parameters.AddWithValue("@IsClientCodeBasedonLinkLocation", Convert.ToInt64(dctSettings.ContainsKey("IsClientCodeBasedonLinkLocation") == true ? dctSettings["IsClientCodeBasedonLinkLocation"] : Convert.ToString(0)));
                    objSqlCommand.Parameters.AddWithValue("@ItemMasterEnabled", Convert.ToInt64(dctSettings.ContainsKey("ItemMasterEnabled") == true ? dctSettings["ItemMasterEnabled"] : Convert.ToString(0)));
                    objSqlCommand.Parameters.AddWithValue("@DeriveHeaderEntities", dctSettings.ContainsKey("DeriveHeaderEntities") == true ? dctSettings["DeriveHeaderEntities"] : string.Empty);
                    objSqlCommand.Parameters.AddWithValue("@IsDeriveAccountingBu", Convert.ToBoolean(dctSettings.ContainsKey("IsDeriveAccountingBu") == true ? dctSettings["IsDeriveAccountingBu"] : Convert.ToString(false)));
                    objSqlCommand.Parameters.AddWithValue("@IsDeriveItemDetailEnable", Convert.ToBoolean(dctSettings.ContainsKey("DeriveItemDetails") == true ? dctSettings["DeriveItemDetails"] : Convert.ToString(false)));
                    objSqlCommand.Parameters.AddWithValue("@UseDocumentLOB", Convert.ToBoolean(dctSettings.ContainsKey("UseDocumentLOB") == true ? dctSettings["UseDocumentLOB"] : Convert.ToString(false)));
                    objSqlCommand.Parameters.AddWithValue("@DerivePartnerFromLocationCode", Convert.ToBoolean(dctSettings.ContainsKey("DerivePartnerFromLocationCode") == true ? dctSettings["DerivePartnerFromLocationCode"] : Convert.ToString(false)));
                    objSqlCommand.Parameters.AddWithValue("@AllowReqForRfxAndOrder", Convert.ToBoolean(dctSettings.ContainsKey("AllowReqForRfxAndOrder") == true ? dctSettings["AllowReqForRfxAndOrder"] : Convert.ToString(false)));
                    objSqlParameter = new SqlParameter("@RequisitionItemAdditionalFields", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_P2P_BZ_REQUISITIONITEMADDITIONALFIELDS,
                        Value = ConvertRequisitionAdditionalFieldAtrributeToTableType(objRequisition.RequisitionItems)
                    };
                    objSqlCommand.Parameters.Add(objSqlParameter);
                    objSqlParameter = new SqlParameter("@RequisitionHeaderAdditionalFields", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_P2P_BZ_REQUISITIONITEMADDITIONALFIELDS,
                        Value = ConvertAdditionalFieldAtrributeToTableType(0,0,objRequisition.lstAdditionalFieldAttributues)
                    };
                    objSqlCommand.Parameters.Add(objSqlParameter);
                    objSqlParameter = new SqlParameter("@HeaderEntities", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_P2P_DOCUMENTADDITIONALENTITYINFO,
                        Value = ConvertHeaderEntitiesToTableType(objRequisition.DocumentAdditionalEntitiesInfoList)
                    };
                    objSqlCommand.Parameters.Add(objSqlParameter);

                    objRefCountingDataReader = (RefCountingDataReader)sqlHelper.ExecuteReader(objSqlCommand);
                    if (objRefCountingDataReader != null)
                    {
                        var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;
                        while (sqlDr.Read())
                            lstErrors.Add(GetStringValue(sqlDr, ReqSqlConstants.COL_ERROR_MESSAGE));
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in ValidateErrorBasedInterfaceRequisition method.", ex);

                var objCustomFault = new CustomFault(ex.Message, "ValidateErrorBasedInterfaceRequisition", "ValidateErrorBasedInterfaceRequisition",
                                                     "Requsition", ExceptionType.ApplicationException,
                                                     string.Empty, false);
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw();
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while Updating Requisition approver status :" +
                                                      string.Empty);
            }
            finally
            {
                if (!ReferenceEquals(objRefCountingDataReader, null) && !objRefCountingDataReader.IsClosed)
                {
                    objRefCountingDataReader.Close();
                    objRefCountingDataReader.Dispose();
                }
                if (!ReferenceEquals(objSqlCon, null) && objSqlCon.State != ConnectionState.Closed)
                {
                    objSqlCon.Close();
                    objSqlCon.Dispose();
                }
                LogHelper.LogInfo(Log, "Reqisition ValidateInterfaceReqisition Method Ended");
            }
            return lstErrors;
        }
        public DataSet GetSplitsDetails(List<RequisitionItem> RequisitionItem, long ContactCode, long lobEntityDetailCode, string EntityCode = null)
        {
            DataSet UpdatedResult = new DataSet();
            SqlConnection objSqlCon = null;
            var lstErrors = new List<string>();
            try
            {
                if (Log.IsDebugEnabled)
                    Log.Debug("In SaveRequisition Method  GetSplitsDetails Method Started ");

                var sqlHelper = ContextSqlConn;
                objSqlCon = (SqlConnection)sqlHelper.CreateConnection();
                objSqlCon.Open();

                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_GETSPLITDETAILS))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    SqlParameter objSqlParameter = new SqlParameter("@SplitItems", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_P2P_INTERFACESPLITITEMS,
                        Value = ConvertREQSplitsToTableType(RequisitionItem)
                    };
                    objSqlCommand.Parameters.Add(objSqlParameter);
                    objSqlCommand.Parameters.AddWithValue("@ContactCode", Convert.ToString(ContactCode));
                    objSqlCommand.Parameters.AddWithValue("LOBEntityDetailCode", lobEntityDetailCode);
                    objSqlCommand.Parameters.AddWithValue("@EntityCode", EntityCode);

                    UpdatedResult = sqlHelper.ExecuteDataSet(objSqlCommand);
                    UpdatedResult.Tables[0].TableName = "SplitEntity";
                }


            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in Requisition GetSplitsDetails Method" , ex);
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw();
                throw;
            }
            finally
            {
                LogHelper.LogInfo(Log, " GetSplitsDetails Method Ended");

                if (objSqlCon != null && objSqlCon.State == ConnectionState.Open)
                    objSqlCon.Close();

            }
            return UpdatedResult;

        }
        private DataTable ConvertREQSplitsToTableType(List<RequisitionItem> lstRequisitionItem)
        {
            int splitCounters = 1;
            DataTable dtSplitItem = new DataTable() { Locale = CultureInfo.InvariantCulture, TableName = ReqSqlConstants.TVP_P2P_INTERFACESPLITITEMS };
            dtSplitItem.Columns.Add("ItemLineNumber", typeof(long));
            dtSplitItem.Columns.Add("EntityCode", typeof(string));
            dtSplitItem.Columns.Add("EntityType", typeof(string));
            dtSplitItem.Columns.Add("SplitItemTotal", typeof(decimal));
            dtSplitItem.Columns.Add("Uids", typeof(int));

            if (lstRequisitionItem != null)
            {
                foreach (var RequisitionItem in lstRequisitionItem)
                {
                    if (RequisitionItem != null && RequisitionItem.ItemSplitsDetail != null)
                    {
                        splitCounters = 1;

                        foreach (var splitItem in RequisitionItem.ItemSplitsDetail)
                        {
                            if (splitItem != null)
                            {
                                foreach (var splitEntity in splitItem.DocumentSplitItemEntities)
                                {
                                    if (splitEntity.EntityType != null && splitEntity.EntityCode != null)
                                    {
                                        DataRow dr = dtSplitItem.NewRow();
                                        dr["ItemLineNumber"] = RequisitionItem.ItemLineNumber;
                                        dr["EntityCode"] = splitEntity.EntityCode;
                                        dr["EntityType"] = splitEntity.EntityType;
                                        dr["SplitItemTotal"] = splitItem.SplitItemTotal == null ? DBNull.Value : (object)splitItem.SplitItemTotal;
                                        dr["Uids"] = splitCounters;
                                        dtSplitItem.Rows.Add(dr);
                                    }
                                }
                            }
                            splitCounters++;
                        }
                    }
                }
            }
            return dtSplitItem;
        }

        public BZRequisition GetRequisitionHeaderDetailsByIdForInterface(long reqId, bool deliverToFreeText = false)
        {
            BZRequisition objBZRequisition = new BZRequisition();
            objBZRequisition.Requisition = new Requisition();
            var objRequisition = objBZRequisition.Requisition;

            try
            {
                LogHelper.LogInfo(Log, "GetRequisitionHeaderDetailsByIdForInterface Method Started for Requisition Id=" + reqId + " and deliverToFreeText=" + deliverToFreeText);
                if (reqId > 0)
                {
                    if (Log.IsDebugEnabled)
                        Log.Debug(string.Concat("In GetRequisitionHeaderDetailsByIdForInterface Method",
                                                 "SP: usp_P2P_REQ_GetHeaderDetailsByIdForInterface with parameter: RequisitionId=" + reqId + " and deliverToFreeText=" + deliverToFreeText));

                    var resultDataSet =
                     ContextSqlConn.ExecuteDataSet(ReqSqlConstants.USP_P2P_REQ_GETREQHEADERDETAILSBYID_FOR_INTERFACE,
                                                                          new object[] { reqId, deliverToFreeText });

                    if (resultDataSet.Tables.Count > 0 && resultDataSet.Tables[0].Rows.Count > 0)
                    {
                        #region Basic
                        DataRow requisitionHeaderRow = resultDataSet.Tables[0].Rows[0];
                        objRequisition.DocumentId = Convert.ToInt64(requisitionHeaderRow[ReqSqlConstants.COL_REQUISITION_ID]);
                        objRequisition.DocumentCode = Convert.ToInt64(requisitionHeaderRow[ReqSqlConstants.COL_DOCUMENT_CODE]);
                        objRequisition.DocumentName = Convert.ToString(requisitionHeaderRow[ReqSqlConstants.COL_DOCUMENT_NAME]);
                        objRequisition.DocumentNumber = Convert.ToString(requisitionHeaderRow[ReqSqlConstants.COL_DOCUMENT_NUMBER]);
                        objRequisition.ClientContactCode = Convert.ToString(requisitionHeaderRow[ReqSqlConstants.COL_CONTACT_CODE]);
                        objRequisition.DocumentStatusInfo = (DocumentStatus)Convert.ToInt32(requisitionHeaderRow[ReqSqlConstants.COL_DOCUMENT_STATUS]);
                        objRequisition.DocumentTypeInfo = (DocumentType)Convert.ToInt32(requisitionHeaderRow[ReqSqlConstants.COL_DOCUMENT_TYPE]);
                        objRequisition.DocumentSourceTypeInfo = (DocumentSourceType)Convert.ToInt16(requisitionHeaderRow[ReqSqlConstants.COL_DOCUMENT_SOURCE]);
                        objRequisition.CreatedOn = Convert.ToDateTime(requisitionHeaderRow[ReqSqlConstants.COL_DATE_CREATED]);
                        objRequisition.UpdatedOn = Convert.ToDateTime(requisitionHeaderRow[ReqSqlConstants.COL_DATE_MODIFIED]);
                        objRequisition.IsDeleted = Convert.ToBoolean(requisitionHeaderRow[ReqSqlConstants.COL_IS_DELETED]);
                        objRequisition.ModifiedBy = Convert.ToInt64(requisitionHeaderRow[ReqSqlConstants.COL_CREATED_BY]);
                        objRequisition.CreatedBy = Convert.ToInt64(requisitionHeaderRow[ReqSqlConstants.COL_CREATED_BY]);
                        objRequisition.Currency = Convert.ToString(requisitionHeaderRow[ReqSqlConstants.COL_CURRENCY]);
                        objRequisition.Tax = !Convert.IsDBNull(requisitionHeaderRow[ReqSqlConstants.COL_TAX]) ? Convert.ToDecimal(requisitionHeaderRow[ReqSqlConstants.COL_TAX]) : default(decimal?);
                        objRequisition.Shipping = !Convert.IsDBNull(requisitionHeaderRow[ReqSqlConstants.COL_SHIPPING]) ? Convert.ToDecimal(requisitionHeaderRow[ReqSqlConstants.COL_SHIPPING]) : default(decimal?);
                        objRequisition.AdditionalCharges = !Convert.IsDBNull(requisitionHeaderRow[ReqSqlConstants.COL_ADDITIONAL_CHARGES]) ? Convert.ToDecimal(requisitionHeaderRow[ReqSqlConstants.COL_ADDITIONAL_CHARGES]) : default(decimal?);
                        objRequisition.ItemTotalAmount = !Convert.IsDBNull(requisitionHeaderRow[ReqSqlConstants.COL_ITEMTOTAL]) ? Convert.ToDecimal(requisitionHeaderRow[ReqSqlConstants.COL_ITEMTOTAL]) : default(decimal?);
                        objRequisition.TotalAmount = !Convert.IsDBNull(requisitionHeaderRow[ReqSqlConstants.COL_REQUISITION_AMOUNT]) ? Convert.ToDecimal(requisitionHeaderRow[ReqSqlConstants.COL_REQUISITION_AMOUNT]) : default(decimal?);
                        objRequisition.RequisitionSource = (RequisitionSource)Convert.ToInt32(requisitionHeaderRow[ReqSqlConstants.COL_REQUISITION_SOURCE]);
                        objRequisition.WorkOrderNumber = Convert.ToString(requisitionHeaderRow[ReqSqlConstants.COL_WORKORDERNO]);
                        objRequisition.ERPOrderTypeName = !Convert.IsDBNull(requisitionHeaderRow[ReqSqlConstants.COL_ERPORDERTYPE]) ? Convert.ToString(requisitionHeaderRow[ReqSqlConstants.COL_ERPORDERTYPE]) : string.Empty;
                        objRequisition.ShiptoLocation = new ShiptoLocation
                        {
                            ShiptoLocationId = Convert.ToInt32(requisitionHeaderRow[ReqSqlConstants.COL_SHIPTOLOC_ID])
                        };
                        objRequisition.CreatedByName = Convert.ToString(requisitionHeaderRow[ReqSqlConstants.COL_REQUESTER_NAME]);
                        objRequisition.DelivertoLocation = new DelivertoLocation()
                        {
                            DelivertoLocationId = Convert.ToInt32(requisitionHeaderRow[ReqSqlConstants.COL_DELIVERTOLOC_ID]),
                            DeliverTo = Convert.ToString(requisitionHeaderRow[ReqSqlConstants.COL_DELIVERTO])
                        };
                        objRequisition.SourceSystemInfo = new SourceSystemInfo
                        {
                            SourceSystemId = Convert.ToInt32(requisitionHeaderRow[ReqSqlConstants.COL_SOURCESYSTEMID]),
                            SourceSystemName = Convert.ToString(requisitionHeaderRow[ReqSqlConstants.COL_SOURCESYSTEMNAME])
                        };
                        objRequisition.CustomAttrFormIdForItem = Convert.ToInt64(requisitionHeaderRow[ReqSqlConstants.COL_ITEMFORMID]);
                        objRequisition.CustomAttrFormId = Convert.ToInt64(requisitionHeaderRow[ReqSqlConstants.COL_FORMID]);
                        objRequisition.CustomAttrFormIdForSplit = Convert.ToInt64(requisitionHeaderRow[ReqSqlConstants.COL_SPLITFORMID]);
                        objRequisition.PurchaseTypeDescription = Convert.ToString(requisitionHeaderRow[ReqSqlConstants.COL_PURCHASETYPE]);
                        objRequisition.Billable = !Convert.IsDBNull(requisitionHeaderRow[ReqSqlConstants.COL_BILLABLE]) ? Convert.ToBoolean(requisitionHeaderRow[ReqSqlConstants.COL_BILLABLE]) : (bool?)null;
                        objRequisition.IsUrgent = Convert.ToBoolean(requisitionHeaderRow[ReqSqlConstants.COL_ISURGENT]);
                        #endregion

                        #region Document BU
                        if (resultDataSet.Tables.Count > 1 && resultDataSet.Tables[1].Rows.Count > 0)
                        {
                            DocumentBU objDocumentBU = null;

                            foreach (DataRow docBUDataRow in resultDataSet.Tables[1].Rows)
                            {
                                objDocumentBU = new DocumentBU();
                                objDocumentBU.BusinessUnitCode = Convert.ToInt64(docBUDataRow[ReqSqlConstants.COL_BUCODE]);
                                objDocumentBU.BusinessUnitName = Convert.ToString(docBUDataRow[ReqSqlConstants.COL_BUSINESSUNITNAME]);
                                objRequisition.DocumentBUList.Add(objDocumentBU);
                            }
                        }
                        #endregion

                        #region Document LOB
                        if (resultDataSet.Tables.Count > 2 && resultDataSet.Tables[2].Rows.Count > 0)
                        {
                            objRequisition.EntityDetailCode = new List<long>();
                            objRequisition.DocumentLOBDetails = new List<DocumentLOBDetails>();
                            foreach (DataRow requisitionLOBDataRow in resultDataSet.Tables[2].Rows)
                            {
                                objRequisition.EntityId = Convert.ToInt32(requisitionLOBDataRow[ReqSqlConstants.COL_ENTITY_ID]);
                                objRequisition.EntityDetailCode.Add(Convert.ToInt64(requisitionLOBDataRow[ReqSqlConstants.COL_ENTITYDETAILCODE]));
                                objRequisition.DocumentLOBDetails.Add(new DocumentLOBDetails() { EntityCode = Convert.ToString(requisitionLOBDataRow[ReqSqlConstants.COL_ENTITY_CODE]) });
                            }
                        }
                        #endregion

                        #region "Header Level Entity Details"
                        if (resultDataSet.Tables.Count > 3 && resultDataSet.Tables[3].Rows.Count > 0)
                        {
                            objRequisition.DocumentAdditionalEntitiesInfoList = new Collection<DocumentAdditionalEntityInfo>();

                            foreach (DataRow requisitionHeaderEntityDataRow in resultDataSet.Tables[3].Rows)
                            {
                                DocumentAdditionalEntityInfo docAddEntityInfo = new DocumentAdditionalEntityInfo()
                                {
                                    EntityDetailCode = Convert.ToInt64(requisitionHeaderEntityDataRow[ReqSqlConstants.COL_SPLIT_ITEM_ENTITYDETAILCODE]),
                                    EntityId = Convert.ToInt32(requisitionHeaderEntityDataRow[ReqSqlConstants.COL_ENTITY_ID]),
                                    EntityDisplayName = Convert.ToString(requisitionHeaderEntityDataRow[ReqSqlConstants.COL_ENTITY_DISPLAY_NAME]),
                                    EntityCode = Convert.ToString(requisitionHeaderEntityDataRow[ReqSqlConstants.COL_ENTITY_CODE])
                                };

                                objRequisition.DocumentAdditionalEntitiesInfoList.Add(docAddEntityInfo);
                            }
                        }
                        #endregion "Header Level Entity Details"

                        #region "Ship To Location Details"
                        if (resultDataSet.Tables.Count > 4 && resultDataSet.Tables[4].Rows.Count > 0)
                        {
                            DataRow reqShipToLocatioRow = resultDataSet.Tables[4].Rows[0];

                            objRequisition.ShiptoLocation = new ShiptoLocation()
                            {
                                ShiptoLocationId = Convert.ToInt32(reqShipToLocatioRow[ReqSqlConstants.COL_SHIPTOLOC_ID]),
                                ShiptoLocationName = Convert.ToString(reqShipToLocatioRow[ReqSqlConstants.COL_SHIPTOLOC_NAME]),
                                ShiptoLocationNumber = Convert.ToString(reqShipToLocatioRow[ReqSqlConstants.COL_SHIPTOLOC_NUMBER]),
                                Address = new P2PAddress()
                                {
                                    AddressLine1 = Convert.ToString(reqShipToLocatioRow[ReqSqlConstants.COL_ADDRESS1]),
                                    AddressLine2 = Convert.ToString(reqShipToLocatioRow[ReqSqlConstants.COL_ADDRESS2]),
                                    City = Convert.ToString(reqShipToLocatioRow[ReqSqlConstants.COL_CITY]),
                                    State = Convert.ToString(reqShipToLocatioRow[ReqSqlConstants.COL_STATE]),
                                    Zip = Convert.ToString(reqShipToLocatioRow[ReqSqlConstants.COL_ZIP]),
                                    AddressId = Convert.ToInt64(reqShipToLocatioRow[ReqSqlConstants.COL_ADDRESSID]),
                                    StateCode = Convert.ToString(reqShipToLocatioRow[ReqSqlConstants.COL_STATECODE]),
                                    CountryCode = Convert.ToString(reqShipToLocatioRow[ReqSqlConstants.COL_COUNTRYCODE]),
                                    Country = Convert.ToString(reqShipToLocatioRow[ReqSqlConstants.COL_COUNTRYCODE])
                                },
                                IsAdhoc = Convert.ToBoolean(reqShipToLocatioRow[ReqSqlConstants.COL_SHIPTOLOC_ISADHOC]),
                                AllowForFutureReference = Convert.ToBoolean(reqShipToLocatioRow[ReqSqlConstants.COL_SHIPTOLOC_ALLOWFORFUTUREREFERENCE]),
                                ContactPerson = Convert.ToString(reqShipToLocatioRow[ReqSqlConstants.COL_SHIPTOLOC_CONTACTPERSON]),
                            };
                        }
                        #endregion

                        #region "Deliver To Location Details"
                        if (resultDataSet.Tables.Count > 6 && resultDataSet.Tables[6].Rows.Count > 0)
                        {
                            DataRow reqDeliverToLocatioRow = resultDataSet.Tables[6].Rows[0];

                            objRequisition.DelivertoLocation = new DelivertoLocation()
                            {
                                DelivertoLocationId = Convert.ToInt32(reqDeliverToLocatioRow[ReqSqlConstants.COL_DELIVERTOLOC_ID]),
                                DelivertoLocationName = Convert.ToString(reqDeliverToLocatioRow[ReqSqlConstants.COL_DELIVERTOLOC_NAME]),
                                DelivertoLocationNumber = Convert.ToString(reqDeliverToLocatioRow[ReqSqlConstants.COL_DELIVERTOLOC_NUMBER]),
                                Address = new P2PAddress()
                                {
                                    AddressLine1 = Convert.ToString(reqDeliverToLocatioRow[ReqSqlConstants.COL_ADDRESS1]),
                                    AddressLine2 = Convert.ToString(reqDeliverToLocatioRow[ReqSqlConstants.COL_ADDRESS2]),
                                    City = Convert.ToString(reqDeliverToLocatioRow[ReqSqlConstants.COL_CITY]),
                                    State = Convert.ToString(reqDeliverToLocatioRow[ReqSqlConstants.COL_STATE]),
                                    Zip = Convert.ToString(reqDeliverToLocatioRow[ReqSqlConstants.COL_ZIP]),
                                    AddressId = Convert.ToInt64(reqDeliverToLocatioRow[ReqSqlConstants.COL_ADDRESSID]),
                                    StateCode = Convert.ToString(reqDeliverToLocatioRow[ReqSqlConstants.COL_STATECODE]),
                                    CountryCode = Convert.ToString(reqDeliverToLocatioRow[ReqSqlConstants.COL_COUNTRYCODE])
                                }
                            };
                        }
                        #endregion

                        #region "Bill To Location Details"
                        if (resultDataSet.Tables.Count > 7 && resultDataSet.Tables[7].Rows.Count > 0)
                        {
                            DataRow reqBillToLocatioRow = resultDataSet.Tables[7].Rows[0];

                            objRequisition.BilltoLocation = new BilltoLocation()
                            {
                                BilltoLocationId = Convert.ToInt32(reqBillToLocatioRow[ReqSqlConstants.COL_BILLTOLOC_ID]),
                                BilltoLocationName = Convert.ToString(reqBillToLocatioRow[ReqSqlConstants.COL_BILLTOLOC_NAME]),
                                BilltoLocationNumber = Convert.ToString(reqBillToLocatioRow[ReqSqlConstants.COL_BILLTOLOC_NUMBER]),
                                Address = new P2PAddress()
                                {
                                    AddressLine1 = Convert.ToString(reqBillToLocatioRow[ReqSqlConstants.COL_ADDRESS1]),
                                    AddressLine2 = Convert.ToString(reqBillToLocatioRow[ReqSqlConstants.COL_ADDRESS2]),
                                    City = Convert.ToString(reqBillToLocatioRow[ReqSqlConstants.COL_CITY]),
                                    State = Convert.ToString(reqBillToLocatioRow[ReqSqlConstants.COL_STATE]),
                                    Zip = Convert.ToString(reqBillToLocatioRow[ReqSqlConstants.COL_ZIP]),
                                    AddressId = Convert.ToInt64(reqBillToLocatioRow[ReqSqlConstants.COL_ADDRESSID]),
                                    StateCode = Convert.ToString(reqBillToLocatioRow[ReqSqlConstants.COL_STATECODE]),
                                    CountryCode = Convert.ToString(reqBillToLocatioRow[ReqSqlConstants.COL_COUNTRYCODE]),
                                    FaxNo = Convert.ToString(reqBillToLocatioRow[ReqSqlConstants.BILLTO_COL_FAXNO]),
                                    EmailAddress = Convert.ToString(reqBillToLocatioRow[ReqSqlConstants.BILLTO_COL_EmailAddress])
                                },
                                IsDefault = !Convert.IsDBNull(reqBillToLocatioRow[ReqSqlConstants.COL_ISDEFAULT]) ? Convert.ToBoolean(reqBillToLocatioRow[ReqSqlConstants.COL_ISDEFAULT]) : default(Boolean),
                                IsDeleted = Convert.ToBoolean(reqBillToLocatioRow[ReqSqlConstants.COL_IS_DELETED]),
                            };
                        }
                        #endregion

                        #region "Buyer Contact Details"
                        if (resultDataSet.Tables.Count > 9 && resultDataSet.Tables[9].Rows.Count > 0)
                        {
                            DataRow reqBuyerContactRow = resultDataSet.Tables[9].Rows[0];

                            objBZRequisition.BuyerContact = new P2PContact()
                            {
                                FirstName = Convert.ToString(reqBuyerContactRow[ReqSqlConstants.COL_FIRSTNAME]),
                                LastName = Convert.ToString(reqBuyerContactRow[ReqSqlConstants.COL_LASTNAME]),
                                ContactCode = Convert.ToInt64(reqBuyerContactRow[ReqSqlConstants.COL_CONTACT_CODE]),
                                EmailAddress = Convert.ToString(reqBuyerContactRow[ReqSqlConstants.COL_EMAIL_ID]),
                                Address = new Address()
                                {
                                    ExtenstionNo1 = Convert.ToString(reqBuyerContactRow[ReqSqlConstants.COL_EXTENSION_NO1]),
                                    ExtenstionNo2 = Convert.ToString(reqBuyerContactRow[ReqSqlConstants.COL_EXTENSION_NO2]),
                                    MobileNo = Convert.ToString(reqBuyerContactRow[ReqSqlConstants.COL_MOBILE_NO]),
                                    PhoneNo1 = Convert.ToString(reqBuyerContactRow[ReqSqlConstants.COL_PHONE_NO1]),
                                    PhoneNo2 = Convert.ToString(reqBuyerContactRow[ReqSqlConstants.COL_PHONE_NO2])
                                }
                            };
                            objBZRequisition.BuyerContact.UserType = P2PUserType.Buyer;
                        }
                        #endregion

                        return objBZRequisition;
                    }
                }
                else
                {
                    if (Log.IsWarnEnabled)
                        Log.Warn("In GetRequisitionHeaderDetailsByIdForInterface Method Parameter: RequisitionId should be greater than 0.");
                }
            }
            finally
            {
                LogHelper.LogInfo(Log, "GetRequisitionHeaderDetailsByIdForInterface Method Ended for RequisitionId = " + reqId + " and deliverToFreeText=" + deliverToFreeText);
            }
            return new BZRequisition();
        }
        public bool DeleteChargeAndSplitsItemsByItemChargeId(List<ItemCharge> lstItemCharge, long ChangeRequisitionItemId)
        {
            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;
            Boolean result = false;
            try
            {
                LogHelper.LogInfo(Log, "Requisition DeleteChargeAndSplitsItemsByItemChargeId Method Started ");

                var sqlHelper = ContextSqlConn;
                _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();

                if (ChangeRequisitionItemId > 0)
                {
                    if (Log.IsDebugEnabled)
                        Log.Debug(string.Format(CultureInfo.InvariantCulture, "Requisition DeleteChargeAndSplitsItemsByItemChargeId sp USP_P2P_REQ_DELETECHARGEANDSPLITSITEMSBYITEMCHARGEID"));

                    using (var objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_DELETECHARGEANDSPLITSITEMSBYITEMCHARGEID, _sqlCon))
                    {
                        objSqlCommand.CommandType = CommandType.StoredProcedure;
                        objSqlCommand.Parameters.Add(new SqlParameter("@tvp_ChargeItemId", SqlDbType.Structured)
                        {
                            TypeName = "tvp_ChargeItemId",
                            Value = ConvertChargeItemIdToTableType(lstItemCharge)
                        });
                        result = Convert.ToBoolean(ContextSqlConn.ExecuteScalar(objSqlCommand, _sqlTrans), CultureInfo.InvariantCulture);
                    }

                    if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
                        _sqlTrans.Commit();
                }
            }
            catch(Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in Requisition DeleteChargeAndSplitsItemsByItemChargeId ",ex);
                if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
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
                throw;
            }
            finally
            {
                if (!ReferenceEquals(_sqlCon, null) && _sqlCon.State != ConnectionState.Closed)
                {
                    _sqlCon.Close();
                    _sqlCon.Dispose();
                }
                LogHelper.LogInfo(Log, "Requisition DeleteChargeAndSplitsItemsByItemChargeId Method Ended ");
            }
            return result;
        }
        private DataTable ConvertChargeItemIdToTableType(List<ItemCharge> lstItemCharge)
        {
            DataTable dtChargeItems = new DataTable() { Locale = CultureInfo.InvariantCulture, TableName = ReqSqlConstants.TVP_CHARGEITEMID };
            dtChargeItems.Columns.Add("ItemChargeId", typeof(long));

            if (lstItemCharge != null && lstItemCharge.Any())
            {
                foreach (var ChargeItem in lstItemCharge)
                {
                    if (ChargeItem != null)
                    {
                        DataRow dr = dtChargeItems.NewRow();
                        dr["ItemChargeId"] = ChargeItem.ItemChargeId;
                        dtChargeItems.Rows.Add(dr);
                    }
                }
            }
            return dtChargeItems;
        }
        public bool DeleteSplitsByItemId(long RequisitionItemId, long documentId)
        {
            SqlConnection _sqlCon = null;
            try
            {
                LogHelper.LogInfo(Log, "Requisition DeleteSplitsByItemId Method Started for orderItemId=" + RequisitionItemId);
                _sqlCon = (SqlConnection)ContextSqlConn.CreateConnection();
                _sqlCon.Open();
                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("In DeleteSplitsByItemId Method",
                                             "SP: usp_P2P_REQ_DeleteSplitsByItemId with parameter: orderItemId=" + RequisitionItemId, " was called."));
                var boolResult = Convert.ToBoolean(ContextSqlConn.ExecuteScalar(ReqSqlConstants.USP_P2P_REQ_DELETESPLITSBYITEMID,
                                                                        RequisitionItemId
                                                                      ), NumberFormatInfo.InvariantInfo);
                return boolResult;
            }
            catch (Exception sqlEx)
            {
                LogHelper.LogError(Log, "Error occured in Requisition DeleteSplitsByItemId Method for RequisitionItemId=" + RequisitionItemId, sqlEx);
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(sqlEx).Throw();
                throw;
            }
            finally
            {
                if (!ReferenceEquals(_sqlCon, null) && _sqlCon.State != ConnectionState.Closed)
                {
                    _sqlCon.Close();
                    _sqlCon.Dispose();
                }
                LogHelper.LogInfo(Log, "Requisition DeleteSplitsByItemId Method Ended for RequisitionItemId=" + RequisitionItemId);
            }
        }
        //public long SaveRequisitionAccountingDetails(List<RequisitionSplitItems> requisitionSplitItems, List<DocumentSplitItemEntity> requisitionSplitItemEntities, decimal lineItemQuantity, int precessionValue, int maxPrecessionforTotal, int maxPrecessionForTaxesAndCharges, bool UseTaxMaster = true, bool updateTaxes = true)

        //{
        //    long validationStatus = 0;
        //    long documentCode = 0;
        //    SqlConnection _sqlCon = null;
        //    SqlTransaction _sqlTrans = null;
        //    try
        //    {
        //        LogHelper.LogInfo(Log, "Requisition SaveRequisitionAccountingDetails Method Started for DocumentItemId = " + requisitionSplitItems[0].DocumentItemId);

        //        var sqlHelper = ContextSqlConn;
        //        _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
        //        _sqlCon.Open();
        //        _sqlTrans = _sqlCon.BeginTransaction();

        //        DataTable dtReqItems;
        //        DataTable dtReqItemEntities = null;
        //        dtReqItems = P2P.DataAccessObjects.DAOHelper.ConvertToDataTable(requisitionSplitItems, GetRequisitionSplitItemTable);
        //        dtReqItemEntities = P2P.DataAccessObjects.DAOHelper.ConvertToDataTable(requisitionSplitItemEntities,
        //                                               GetRequisitionSplitItemEntitiesTable);
        //        //dtReqItemEntities = ConvertToDataTable(requisitionSplitItems.SelectMany(item => item.DocumentSplitItemEntities).ToList<DocumentSplitItemEntity>(), GetRequisitionSplitItemEntitiesTable);
        //        //string result = string.Empty;
        //        using (var objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_SAVEREQUISITIONACCOUNTINGDETAILS, _sqlCon))
        //        {
        //            objSqlCommand.CommandType = CommandType.StoredProcedure;
        //            objSqlCommand.Parameters.Add(new SqlParameter("@tvp_P2P_SplitItems", SqlDbType.Structured)
        //            {
        //                TypeName = ReqSqlConstants.TVP_P2P_SPLITITEMS,
        //                Value = dtReqItems
        //            });
        //            objSqlCommand.Parameters.Add(new SqlParameter("@tvp_P2P_SplitItemsEntities", SqlDbType.Structured)
        //            {
        //                TypeName = ReqSqlConstants.TVP_P2P_SPLITITEMSENTITIES,
        //                Value = dtReqItemEntities
        //            });
        //            objSqlCommand.Parameters.AddWithValue("@UserId", UserContext.ContactCode);
        //            objSqlCommand.Parameters.AddWithValue("@lineItemQuantity", lineItemQuantity);
        //            objSqlCommand.Parameters.AddWithValue("@precessionValue", precessionValue);
        //            objSqlCommand.Parameters.AddWithValue("@precissionTotal", maxPrecessionforTotal);
        //            objSqlCommand.Parameters.AddWithValue("@precessionValueForTaxAndCharges", maxPrecessionForTaxesAndCharges);
        //            objSqlCommand.Parameters.AddWithValue("@TaxBasedOnShipTo", UseTaxMaster);
        //            objSqlCommand.Parameters.AddWithValue("@UpdateTaxes", updateTaxes);
        //            var sqlDr = (SqlDataReader)sqlHelper.ExecuteReader(objSqlCommand, _sqlTrans);
        //            if (sqlDr != null)
        //            {
        //                while (sqlDr.Read())
        //                {
        //                    validationStatus = GetLongValue(sqlDr, ReqSqlConstants.COL_ACCOUNTING_STATUS);
        //                    documentCode = GetLongValue(sqlDr, ReqSqlConstants.COL_REQUISITION_ID);
        //                }
        //                sqlDr.Close();
        //            }
        //        }
        //        if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
        //        {
        //            _sqlTrans.Commit();
        //            if (this.UserContext.Product != GEPSuite.eInterface && documentCode > 0)
        //                AddIntoSearchIndexerQueueing(documentCode, (int)DocumentType.Requisition, UserContext, GepConfiguration);


        //        }
        //    }
        //    catch
        //    {
        //        if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
        //        {
        //            try
        //            {
        //                _sqlTrans.Rollback();
        //            }
        //            catch (InvalidOperationException error)
        //            {
        //                if (Log.IsInfoEnabled) Log.Info(error.Message);
        //            }
        //        }
        //        throw;
        //    }
        //    finally
        //    {
        //        if (!ReferenceEquals(_sqlCon, null) && _sqlCon.State != ConnectionState.Closed)
        //        {
        //            _sqlCon.Close();
        //            _sqlCon.Dispose();
        //        }
        //        LogHelper.LogInfo(Log, "Requisition SaveRequisitionAccountingDetails Method Ended for DocumentItemId = " + requisitionSplitItems[0].DocumentItemId);
        //    }

        //    return validationStatus;

        //}

        private List<KeyValuePair<Type, string>> GetRequisitionSplitItemEntitiesTable()
        {
            var lstMatchItemProperties = new List<KeyValuePair<Type, string>>();
            lstMatchItemProperties.Add(new KeyValuePair<Type, string>(typeof(long), "DocumentSplitItemEntityId"));
            lstMatchItemProperties.Add(new KeyValuePair<Type, string>(typeof(long), "DocumentSplitItemId"));
            lstMatchItemProperties.Add(new KeyValuePair<Type, string>(typeof(int), "SplitAccountingFieldId"));
            lstMatchItemProperties.Add(new KeyValuePair<Type, string>(typeof(string), "SplitAccountingFieldValue"));
            lstMatchItemProperties.Add(new KeyValuePair<Type, string>(typeof(string), "EntityCode"));
            lstMatchItemProperties.Add(new KeyValuePair<Type, string>(typeof(int), "UiId"));
            return lstMatchItemProperties;
        }

        //private List<KeyValuePair<Type, string>> GetRequisitionSplitItemTable()
        //{
        //    var lstMatchItemProperties = new List<KeyValuePair<Type, string>>();
        //    lstMatchItemProperties.Add(new KeyValuePair<Type, string>(typeof(long), "DocumentSplitItemId"));
        //    lstMatchItemProperties.Add(new KeyValuePair<Type, string>(typeof(long), "DocumentItemId"));
        //    lstMatchItemProperties.Add(new KeyValuePair<Type, string>(typeof(short), "SplitType"));
        //    lstMatchItemProperties.Add(new KeyValuePair<Type, string>(typeof(decimal), "Percentage"));
        //    lstMatchItemProperties.Add(new KeyValuePair<Type, string>(typeof(decimal), "Quantity"));
        //    lstMatchItemProperties.Add(new KeyValuePair<Type, string>(typeof(decimal), "Tax"));
        //    lstMatchItemProperties.Add(new KeyValuePair<Type, string>(typeof(decimal), "ShippingCharges"));
        //    lstMatchItemProperties.Add(new KeyValuePair<Type, string>(typeof(decimal), "AdditionalCharges"));
        //    lstMatchItemProperties.Add(new KeyValuePair<Type, string>(typeof(decimal), "SplitItemTotal"));
        //    lstMatchItemProperties.Add(new KeyValuePair<Type, string>(typeof(bool), "IsDeleted"));
        //    lstMatchItemProperties.Add(new KeyValuePair<Type, string>(typeof(string), "ErrorCode"));
        //    lstMatchItemProperties.Add(new KeyValuePair<Type, string>(typeof(int), "UiId"));
        //    lstMatchItemProperties.Add(new KeyValuePair<Type, string>(typeof(decimal), "OverallLimitSplitItem"));
        //    return lstMatchItemProperties;
        //}

        public void SaveRequisitionAdditionalDetailsFromInterface(long documentCode)
        {
            SqlConnection objSqlCon = null;
            SqlTransaction objSqlTrans = null;

            try
            {
                var sqlHelper = ContextSqlConn;
                objSqlCon = (SqlConnection)sqlHelper.CreateConnection();
                objSqlCon.Open();
                objSqlTrans = objSqlCon.BeginTransaction();

                SQLDocumentDAO sqlDocumentDAO = new SQLDocumentDAO();
                sqlDocumentDAO.SqlTransaction = objSqlTrans;
                sqlDocumentDAO.ReliableSqlDatabase = sqlHelper;
                sqlDocumentDAO.UserContext = UserContext;
                sqlDocumentDAO.GepConfiguration = GepConfiguration;

                requisitionDAO = new SQLRequisitionDAO { UserContext = UserContext, GepConfiguration = GepConfiguration };
                requisitionDAO.SaveRequisitionAdditionalDetails(documentCode, sqlDocumentDAO);

                if (!ReferenceEquals(objSqlTrans, null) && !ReferenceEquals(objSqlCon, null) && objSqlCon.State == ConnectionState.Open)
                    objSqlTrans.Commit();
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in Requisition SaveRequisitionAdditionalDetailsFromInterface Method", ex);
                if (!ReferenceEquals(objSqlTrans, null) && !ReferenceEquals(objSqlCon, null) && objSqlCon.State == ConnectionState.Open)
                {
                    try
                    {
                        objSqlTrans.Rollback();
                    }
                    catch (InvalidOperationException error)
                    {
                        if (Log.IsInfoEnabled) Log.Info(error.Message);
                    }
                    System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw();
                    throw;
                }
            }
            finally
            {
                if (!ReferenceEquals(objSqlCon, null) && objSqlCon.State != ConnectionState.Closed)
                {
                    objSqlCon.Close();
                    objSqlCon.Dispose();
                }
            }
        }
        public DataSet ProprateLineItemTaxandShipping(List<P2PItem> objItems, decimal Tax, decimal shipping, decimal AdditionalCharges, Int64 PunchoutCartReqId, int precessionValue = 0, int maxPrecessionforTotal = 0, int maxPrecessionForTaxesAndCharges = 0)
        {
            DataSet dsResult = new DataSet();


            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;
            try
            {
                LogHelper.LogInfo(Log, "Requisition ProprateLineItemTaxandShipping Method Started ");

                var sqlHelper = ContextSqlConn;
                _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();

                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("Requisition ProprateLineItemTaxandShipping sp usp_P2P_REQ_ProrateLineItemTaxandShipping with parameter: P2pItems=" + objItems, " was called."));

                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_PRORATELINEITEMTAXANDSHIPPING))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.Add(new SqlParameter("@Tax", SqlDbType.Decimal) { Value = Tax });
                    objSqlCommand.Parameters.Add(new SqlParameter("@shipping", SqlDbType.Decimal) { Value = shipping });
                    objSqlCommand.Parameters.Add(new SqlParameter("@AdditionalCharges", SqlDbType.Decimal) { Value = AdditionalCharges });
                    objSqlCommand.Parameters.Add(new SqlParameter("@PunchoutCartReqId", SqlDbType.BigInt) { Value = PunchoutCartReqId });
                    objSqlCommand.Parameters.Add(new SqlParameter("@precessionValue", SqlDbType.BigInt) { Value = precessionValue });
                    objSqlCommand.Parameters.Add(new SqlParameter("@maxPrecessionforTotal", SqlDbType.BigInt) { Value = maxPrecessionforTotal });
                    objSqlCommand.Parameters.Add(new SqlParameter("@maxPrecessionForTaxesAndCharges", SqlDbType.BigInt) { Value = maxPrecessionForTaxesAndCharges });
                    objSqlCommand.Parameters.Add(new SqlParameter("@tvp_P2P_LineItemsTaxandShipping", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_P2P_LINEITEMSTAXANDSHIPPING,
                        Value = SetRequisitionTaxandShippingTable(objItems)
                    });

                    dsResult = sqlHelper.ExecuteDataSet(objSqlCommand, _sqlTrans);
                }

                if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
                    _sqlTrans.Commit();
                return dsResult;
            }
            catch(Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in Requisition ProprateLineItemTaxandShipping Method", ex);
                if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
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
                throw;
            }
            finally
            {
                if (!ReferenceEquals(_sqlCon, null) && _sqlCon.State != ConnectionState.Closed)
                {
                    _sqlCon.Close();
                    _sqlCon.Dispose();
                }

                LogHelper.LogInfo(Log, "Requisition Method Ends for ProprateLineItemTaxandShipping ");
            }

            return dsResult;
        }
        private DataTable SetRequisitionTaxandShippingTable(List<P2PItem> objP2PItems)
        {
            DataTable dtP2pItems = new DataTable() { Locale = CultureInfo.InvariantCulture, TableName = ReqSqlConstants.TVP_P2P_LINEITEMSTAXANDSHIPPING };
            dtP2pItems.Columns.Add("Tax", typeof(decimal));
            dtP2pItems.Columns.Add("Shipping", typeof(decimal));
            dtP2pItems.Columns.Add("AdditionalCharges", typeof(decimal));
            dtP2pItems.Columns.Add("SupplierPartAuxiliaryId", typeof(string));
            dtP2pItems.Columns.Add("SupplierPartId", typeof(string));
            dtP2pItems.Columns.Add("LineNumber", typeof(long));
            dtP2pItems.Columns.Add("Quantity", typeof(decimal));
            dtP2pItems.Columns.Add("UnitPrice", typeof(decimal));

            if (objP2PItems != null && objP2PItems.Any())
            {
                foreach (var item in objP2PItems)
                {
                    if (item != null)
                    {
                        DataRow dr = dtP2pItems.NewRow();
                        dr["Tax"] = item.Tax == null ? 0 : item.Tax;
                        dr["Shipping"] = item.ShippingCharges == null ? 0 : item.ShippingCharges;
                        dr["AdditionalCharges"] = item.AdditionalCharges == null ? 0 : item.AdditionalCharges;
                        dr["SupplierPartAuxiliaryId"] = string.IsNullOrEmpty(item.SupplierPartAuxiliaryId) ? null : item.SupplierPartAuxiliaryId;
                        dr["SupplierPartId"] = string.IsNullOrEmpty(item.SupplierPartId) ? null : item.SupplierPartId;
                        dr["LineNumber"] = item.ItemLineNumber;
                        dr["Quantity"] = item.Quantity;
                        dr["UnitPrice"] = item.UnitPrice;

                        dtP2pItems.Rows.Add(dr);
                    }
                }
            }
            return dtP2pItems;
        }

        private DataTable ConvertIncoTermDetailIntoDataTable(List<RequisitionItem> requisitionItems,int SpendControlType=0)
        {
            DataTable dtRequisitionItem = new DataTable() { Locale = CultureInfo.InvariantCulture, TableName = ReqSqlConstants.TVP_P2P_REQUISITIONITEMINCOTERMDETAILS };
            dtRequisitionItem.Columns.Add("RequisitionItemID", typeof(Int64));
            dtRequisitionItem.Columns.Add("IncoTermID", typeof(int));
            dtRequisitionItem.Columns.Add("IncoTermCode", typeof(string));
            dtRequisitionItem.Columns.Add("IncoTermLocation", typeof(string));
            dtRequisitionItem.Columns.Add("CatalogItemID", typeof(long));
            dtRequisitionItem.Columns.Add("ExtContractRef", typeof(string));
            dtRequisitionItem.Columns.Add("Itemspecification", typeof(string));
            dtRequisitionItem.Columns.Add("InternalPlantMemo", typeof(string));
            dtRequisitionItem.Columns.Add("ShipFromLocationId", typeof(long));
            dtRequisitionItem.Columns.Add("AllowFlexiblePrice", typeof(long));


      if (requisitionItems != null)
            {
                foreach (var objRequisitionItems in requisitionItems)
                {
                    DataRow dr = dtRequisitionItem.NewRow();
                    dr["RequisitionItemID"] = objRequisitionItems.DocumentItemId;
                    dr["IncoTermID"] = objRequisitionItems.IncoTermId;
                    dr["IncoTermCode"] = objRequisitionItems.IncoTermCode;
                    dr["IncoTermLocation"] = objRequisitionItems.IncoTermLocation;
                    dr["CatalogItemID"] = objRequisitionItems.CatalogItemId;
                    dr["ExtContractRef"] = !(string.IsNullOrEmpty(objRequisitionItems.SpendControlDocumentNumber)) ? objRequisitionItems.SpendControlDocumentNumber : objRequisitionItems.ExtContractRef;
                    dr["Itemspecification"] = objRequisitionItems.Itemspecification;
                    dr["InternalPlantMemo"] = objRequisitionItems.InternalPlantMemo;
                    dr["ShipFromLocationId"] = objRequisitionItems.ShipFromLocationId;
                    dr["AllowFlexiblePrice"] = objRequisitionItems.AllowFlexiblePrice;

          dtRequisitionItem.Rows.Add(dr);

                }
            }
            return dtRequisitionItem;
        }

        public void SaveRequisitionItemAdditionalDetailsFromInterface(List<RequisitionItem> requisitionItems,int SpendControlType=0)
        {
            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;
            try
            {
                LogHelper.LogInfo(Log, "Requisition SaveRequisitionItemIncoTermDetailsFromInterface Method Started for DocumentItemId = ");

                var sqlHelper = ContextSqlConn;
                _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();

                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("Requisition SaveRequisitionItemIncoTermDetailsFromInterface sp usp_P2P_REQ_SaveReqAdditionalEntityDetails with parameter: lstEntityInfo="));


                DataTable dtRequisitionItem = null;
                dtRequisitionItem = ConvertIncoTermDetailIntoDataTable(requisitionItems);

                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_SAVEREQUISITIONITEMSINCOTERMDETAILS))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.Add(new SqlParameter("@TVP_P2P_RequisitionItemIncoTermDetails", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_P2P_REQUISITIONITEMINCOTERMDETAILS,
                        Value = dtRequisitionItem
                    });
                    objSqlCommand.Parameters.Add(new SqlParameter("@SpendControlType", SpendControlType));

                    ContextSqlConn.ExecuteNonQuery(objSqlCommand);
                }

            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in SQLRequisitionDAO SaveRequisitionItemIncoTermDetailsFromInterface Method  " , ex);
                if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
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
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw();
                throw;
            }
            finally
            {
                if (!ReferenceEquals(_sqlCon, null) && _sqlCon.State != ConnectionState.Closed)
                {
                    _sqlCon.Close();
                    _sqlCon.Dispose();
                }

                LogHelper.LogInfo(Log, "Requisition SaveRequisitionItemIncoTermDetailsFromInterface Method Ended for DocumentItemId");
            }
        }

        //public bool UpdateRequisitionStatusForStockReservation(long RequisitionID)
        //{
        //    LogHelper.LogInfo(Log, "Requisition UpdateRequisitionStatusForStockReservation Method Started for documentId = " + RequisitionID);
        //    SqlConnection objSqlCon = null;
        //    SqlTransaction objSqlTrans = null;
        //    try
        //    {
        //        if (Log.IsDebugEnabled)
        //            Log.Debug(string.Concat("In UpdateRequisitionStatusForStockReservation Method.",
        //                                    " SP: usp_P2P_REQ_UpdateRequsitionStatusFromInterface, with parameters: documentId = " + RequisitionID
        //                                        ));

        //        objSqlCon = (SqlConnection)ContextSqlConn.CreateConnection();
        //        objSqlCon.Open();

        //        var result = Convert.ToBoolean(ContextSqlConn.ExecuteNonQuery(ReqSqlConstants.USP_P2P_REQ_UPDATEREQUISITIONSTATUSFROMINTERFACE, RequisitionID));
        //        return result;
        //    }
        //    catch
        //    {
        //        if (!ReferenceEquals(objSqlTrans, null) && !ReferenceEquals(objSqlCon, null) && objSqlCon.State == ConnectionState.Open)
        //        {
        //            try
        //            {
        //                objSqlTrans.Rollback();
        //            }
        //            catch (InvalidOperationException error)
        //            {
        //                if (Log.IsInfoEnabled) Log.Info(error.Message);
        //            }
        //        }
        //        throw;
        //    }
        //    finally
        //    {
        //        if (!ReferenceEquals(objSqlCon, null) && objSqlCon.State != ConnectionState.Closed)
        //        {
        //            objSqlCon.Close();
        //            objSqlCon.Dispose();
        //        }
        //        LogHelper.LogInfo(Log, "Requisition UpdateRequisitionStatusForStockReservation Method Ended for documentId=" + RequisitionID);
        //    }
        //}
        //public RequisitionLineStatusUpdateDetails UpdateRequisitionNotificationDetails(long requisitionId)
        //{
        //    RequisitionLineStatusUpdateDetails objRequisitionLineStatusUpdateDetails = new RequisitionLineStatusUpdateDetails();
        //    try
        //    {
        //        DataSet dsResult = new DataSet();
        //        SqlConnection _sqlCon = null;
        //        SqlTransaction _sqlTrans = null;
        //        try
        //        {
        //            LogHelper.LogInfo(Log, "Requisition UpdateRequisitionNotificationDetails Method Started ");

        //            var sqlHelper = ContextSqlConn;
        //            _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
        //            _sqlCon.Open();
        //            _sqlTrans = _sqlCon.BeginTransaction();

        //            if (Log.IsDebugEnabled)
        //                Log.Debug(string.Concat("Requisition UpdateRequisitionNotificationDetails  with parameter: documentCode=" + requisitionId, " was called."));


        //            using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_GETNOTIFICATIONDETAILSFORINTERFACE))
        //            {
        //                objSqlCommand.CommandType = CommandType.StoredProcedure;
        //                objSqlCommand.Parameters.Add(new SqlParameter("@requisitionId ", requisitionId));

        //                dsResult = sqlHelper.ExecuteDataSet(objSqlCommand, _sqlTrans);

        //                if (dsResult.Tables.Count > 0 && dsResult.Tables[0].Rows.Count > 0)
        //                {
        //                    foreach (DataRow HeaderRequisitionDataRow in dsResult.Tables[0].Rows)
        //                    {
        //                        objRequisitionLineStatusUpdateDetails.RequisitionNumber = !Convert.IsDBNull(HeaderRequisitionDataRow[ReqSqlConstants.COL_REQUISITION_NUMBER]) ? Convert.ToString(HeaderRequisitionDataRow[ReqSqlConstants.COL_REQUISITION_NUMBER]) : default(string);
        //                        objRequisitionLineStatusUpdateDetails.RequesterFirstName = !Convert.IsDBNull(HeaderRequisitionDataRow[ReqSqlConstants.COL_REQUESTER_FIRSTNAME]) ? Convert.ToString(HeaderRequisitionDataRow[ReqSqlConstants.COL_REQUESTER_FIRSTNAME]) : default(string);
        //                        objRequisitionLineStatusUpdateDetails.RequesterLastName = !Convert.IsDBNull(HeaderRequisitionDataRow[ReqSqlConstants.COL_REQUESTER_LASTNAME]) ? Convert.ToString(HeaderRequisitionDataRow[ReqSqlConstants.COL_REQUESTER_LASTNAME]) : default(string);
        //                        objRequisitionLineStatusUpdateDetails.RequesterEmailAddress = !Convert.IsDBNull(HeaderRequisitionDataRow[ReqSqlConstants.COL_REQUESTER_EMAILADDRESS]) ? Convert.ToString(HeaderRequisitionDataRow[ReqSqlConstants.COL_REQUESTER_EMAILADDRESS]) : default(string);
        //                        objRequisitionLineStatusUpdateDetails.RequesterContactCode = !Convert.IsDBNull(HeaderRequisitionDataRow[ReqSqlConstants.COL_REQUESTER_CONTACTCODE]) ? Convert.ToInt64(HeaderRequisitionDataRow[ReqSqlConstants.COL_REQUESTER_CONTACTCODE]) : default(Int64);
        //                        objRequisitionLineStatusUpdateDetails.OBUFirstName = !Convert.IsDBNull(HeaderRequisitionDataRow[ReqSqlConstants.COL_OBU_FIRSTNAME]) ? Convert.ToString(HeaderRequisitionDataRow[ReqSqlConstants.COL_OBU_FIRSTNAME]) : default(string);
        //                        objRequisitionLineStatusUpdateDetails.OBULastName = !Convert.IsDBNull(HeaderRequisitionDataRow[ReqSqlConstants.COL_OBU_LAST_NAME]) ? Convert.ToString(HeaderRequisitionDataRow[ReqSqlConstants.COL_OBU_LAST_NAME]) : default(string);
        //                        objRequisitionLineStatusUpdateDetails.OBUEmailAddress = !Convert.IsDBNull(HeaderRequisitionDataRow[ReqSqlConstants.COL_OBU_EMAILADDRESS]) ? Convert.ToString(HeaderRequisitionDataRow[ReqSqlConstants.COL_OBU_EMAILADDRESS]) : default(string);
        //                        objRequisitionLineStatusUpdateDetails.OBUContactCode = !Convert.IsDBNull(HeaderRequisitionDataRow[ReqSqlConstants.COL_OBU_CONTACTCODE]) ? Convert.ToInt64(HeaderRequisitionDataRow[ReqSqlConstants.COL_OBU_CONTACTCODE]) : default(Int64);
        //                        objRequisitionLineStatusUpdateDetails.RequisitionId = requisitionId;
        //                    }
        //                }
        //                if (dsResult.Tables.Count > 0 && dsResult.Tables[1].Rows.Count > 0)
        //                {
        //                    objRequisitionLineStatusUpdateDetails.Items = new List<LineStatusRequisition>();
        //                    foreach (DataRow items in dsResult.Tables[1].Rows)
        //                    {
        //                        {
        //                            var documentitems = new LineStatusRequisition();
        //                            documentitems.LineNumber = Convert.ToInt64(items[ReqSqlConstants.COL_REQ_ITEM_LINENUMBER], CultureInfo.InvariantCulture);
        //                            documentitems.LineStatus = (StockReservationStatus)(items[ReqSqlConstants.COL_LineStatus]);

        //                            objRequisitionLineStatusUpdateDetails.Items.Add(documentitems);

        //                        };

        //                    }
        //                }
        //            }

        //            if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
        //                _sqlTrans.Commit();
        //        }
        //        catch
        //        {
        //            if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
        //            {
        //                try
        //                {
        //                    _sqlTrans.Rollback();
        //                }
        //                catch (InvalidOperationException error)
        //                {
        //                    if (Log.IsInfoEnabled) Log.Info(error.Message);
        //                }
        //            }
        //            throw;
        //        }
        //        finally
        //        {
        //            if (!ReferenceEquals(_sqlCon, null) && _sqlCon.State != ConnectionState.Closed)
        //            {
        //                _sqlCon.Close();
        //                _sqlCon.Dispose();
        //            }

        //            LogHelper.LogInfo(Log, "Requisition Method Ends for UpdateRequisitionNotificationDetails ");
        //        }
        //    }
        //    finally
        //    {
        //        LogHelper.LogInfo(Log, "Requisition Method Ends for UpdateRequisitionNotificationDetails ");
        //    }
        //    return objRequisitionLineStatusUpdateDetails;

        //}

    }
}
