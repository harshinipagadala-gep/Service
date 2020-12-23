using Gep.Cumulus.CSM.Entities;
using Gep.Cumulus.CSM.Extensions;
//using GEP.Cumulus.Dab.DBStorage;
using Gep.Cumulus.ExceptionManager;
using Gep.Cumulus.Partner.Entities;
using GEP.Cumulus.DocumentIntegration.Entities;
using GEP.Cumulus.Documents.DataAccessObjects.SQLServer;
using GEP.Cumulus.Documents.Entities;
using GEP.Cumulus.Logging;
using GEP.Cumulus.P2P.BusinessEntities;
using GEP.Cumulus.P2P.BusinessEntities.ExportDataSetEntities;
using GEP.Cumulus.P2P.Req.DataAccessObjects;
using GEP.Cumulus.P2P.Req.DataAccessObjects.SQLServer;
using log4net;
using Microsoft.Practices.EnterpriseLibrary.Data;
using REDataModel;
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
    public class SQLRequisitionDAO : SQLDocumentDAO, IRequisitionDAO
    {
        private static readonly ILog Log = Logger.GetLog(MethodBase.GetCurrentMethod().DeclaringType);

        //private ReliableSqlDatabase SqlHelper(string companyName = "")
        //{
        //    return new ReliableSqlDatabase(GepConfiguration.GetConnectionString(companyName));
        //}

        public bool UpdateApprovalStatusById(long documentId, Documents.Entities.DocumentStatus approvalStatus)
        {
            SqlConnection _sqlCon = null;
            try
            {
                LogHelper.LogInfo(Log, "UpdateApprovalStatusById Method Started");

                var sqlHelper = ContextSqlConn;
                _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                _sqlCon.Open();

                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("In UpdateApprovalStatusById Method with parameter: documentId=" + documentId +
                                                                                                ",approvalStatus=" + approvalStatus.ToString(CultureInfo.InvariantCulture)));


                var result = Convert.ToBoolean(sqlHelper.ExecuteScalar(ReqSqlConstants.USP_P2P_REQ_UPDATEREQUSITIONSTATUSBYID,
                                                                 new object[] { documentId, approvalStatus }), NumberFormatInfo.InvariantInfo);
                return result;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in UpdateApprovalStatusById method.", ex);

                var objCustomFault = new CustomFault(ex.Message, "UpdateDocumentApprovalStatusById", "UpdateDocumentApprovalStatusById",
                                                     "Requsition", ExceptionType.ApplicationException,
                                                     documentId.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while Updating Requisition approver status :" +
                                                      documentId.ToString(CultureInfo.InvariantCulture));
                // return false;
            }
            finally
            {
                if (!ReferenceEquals(_sqlCon, null) && _sqlCon.State != ConnectionState.Closed)
                {
                    _sqlCon.Close();
                    _sqlCon.Dispose();
                }

                if (Log.IsInfoEnabled)
                {
                    Log.Info("UpdateApprovalStatusById Method Ended");
                }
            }
        }

        public ICollection<P2PDocument> GetP2PLandingPage(int pageIndex, int pageSize, string sortBy, string sortOrder, string activities)
        {
            throw new NotImplementedException();
        }

        public List<P2PItem> GetAllManualLineItemsById(long documentCode)
        {
            throw new NotImplementedException();
        }

        public ICollection<KeyValuePair<long, string>> GetPreDocumentNumberByDocumentID(long orderId)
        {
            throw new NotImplementedException();
        }

        public bool SaveApproverDetails(long documentId, int approverId, Documents.Entities.DocumentStatus approvalStatus, string approveUri,
                                                string rejectUri, string instanceId)
        {
            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;
            try
            {
                var sqlHelper = ContextSqlConn;
                _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();
                var result = Convert.ToBoolean(sqlHelper.ExecuteScalar(_sqlTrans, ReqSqlConstants.USP_P2P_REQ_SAVEREQUISITIONAPPROVERDETAILS,
                                                                 new object[] { documentId, approverId, approvalStatus,
                                                                     approveUri, rejectUri, HtmlEncode(instanceId) }), NumberFormatInfo.InvariantInfo);
                if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
                    _sqlTrans.Commit();
                return result;
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
                var objCustomFault = new CustomFault(ex.Message, "SaveDocumentApproverDetails", "SaveDocumentApproverDetails",
                                                     "Requsition", ExceptionType.ApplicationException,
                                                     documentId.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while Saving Requisition approver Details :" +
                                                      documentId.ToString(CultureInfo.InvariantCulture));
                // return false;
            }
            finally
            {
                if (!ReferenceEquals(_sqlCon, null) && _sqlCon.State != ConnectionState.Closed)
                {
                    _sqlCon.Close();
                    _sqlCon.Dispose();
                }
            }
        }

        public bool SaveTrackStatusDetails(long requisitionId, string instanceId, long approverId, string approverName, string approverType, string approveUri,
                                    string rejectUri, DateTime statusDate, RequisitionTrackStatus approvalStatus, bool isDeleted)
        {
            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;
            try
            {
                var sqlHelper = ContextSqlConn;
                _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();
                var result = Convert.ToBoolean(sqlHelper.ExecuteScalar(_sqlTrans, ReqSqlConstants.USP_P2P_REQ_SAVEREQUISITIONTRACKSTATUSDETAILS,
                                                                 new object[] { requisitionId, HtmlEncode(instanceId), approverId, HtmlEncode(approverName),
                                                                     approverType, approveUri,rejectUri, statusDate, approvalStatus, isDeleted }), NumberFormatInfo.InvariantInfo);
                if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
                    _sqlTrans.Commit();
                return result;
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
                var objCustomFault = new CustomFault(ex.Message, "SaveTrackStatusDetails", "SaveTrackStatusDetails",
                                                     "Requsition", ExceptionType.ApplicationException,
                                                     requisitionId.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while Saving Requisition Teack Status Details :" +
                                                      requisitionId.ToString(CultureInfo.InvariantCulture));
                // return false;
                //throw ex;

            }
            finally
            {
                if (!ReferenceEquals(_sqlCon, null) && _sqlCon.State != ConnectionState.Closed)
                {
                    _sqlCon.Close();
                    _sqlCon.Dispose();
                }
            }
        }

        public P2PDocument GetBasicDetailsById(long documentCode, long userId, int typeOfUser = 0, bool filterByBU = true, bool isFunctionalAdmin = false, string documentStatuses = "", int maxPrecessionValue = 0, int maxPrecessionValueTotal = 0, int maxPrecessionValueForTaxAndCharges = 0, string accessType = "0", int ACEntityId = 0, bool byPassAccessRight = false)
        {
            RefCountingDataReader objRefCountingDataReader = null;
            try
            {
                LogHelper.LogInfo(Log, "GetBasicDetailsById Method Started for id=" + documentCode);

                var sqlHelper = ContextSqlConn;

                if (documentCode > 0)
                {
                    Document objDocument = new Document();
                    objDocument.DocumentCode = documentCode;
                    objDocument.DocumentTypeInfo = DocumentType.Requisition;
                    objDocument.IsDocumentDetails = true;
                    objDocument.IsAddtionalDetails = false;
                    objDocument.IsStakeholderDetails = true;
                    objDocument.CompanyName = UserContext.ClientName;
                    objDocument.IsFilterByBU = filterByBU;
                    objDocument.ACEntityId = ACEntityId;

                    SQLDocumentDAO sqlDocumentDAO = new SQLDocumentDAO();

                    sqlDocumentDAO.ReliableSqlDatabase = sqlHelper;
                    sqlDocumentDAO.UserContext = UserContext;
                    sqlDocumentDAO.GepConfiguration = GepConfiguration;

                    objDocument = sqlDocumentDAO.GetDocumentDetailsById(objDocument, false, isFunctionalAdmin, documentStatuses);

                    if (Log.IsDebugEnabled)
                        Log.Debug(string.Concat("In GetBasicDetailsById Method sp usp_P2P_REQ_GetRequisitionBasicDetailsById with parameter: documentCode=" + documentCode, ",userId=" + userId));

                    objRefCountingDataReader =
                     (RefCountingDataReader)
                     sqlHelper.ExecuteReader(ReqSqlConstants.USP_P2P_REQ_GETREQUISITIONBASICDETAILSBYID,
                                                                      new object[] { documentCode, userId, typeOfUser, maxPrecessionValueTotal, maxPrecessionValueForTaxAndCharges });

                    if (objRefCountingDataReader != null)
                    {
                        var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;
                        var objRequisition = new Requisition();
                        objRequisition.DocumentAdditionalEntitiesInfoList = new Collection<DocumentAdditionalEntityInfo>();
                        while (sqlDr.Read())
                        {
                            objRequisition.DocumentCode = objDocument.DocumentCode;
                            objRequisition.DocumentName = objDocument.DocumentName;
                            objRequisition.CreatedOn = objDocument.CreatedOn;
                            objRequisition.CreatedBy = objDocument.CreatedBy;
                            objRequisition.UpdatedOn = objDocument.UpdatedOn;
                            objRequisition.DocumentNumber = objDocument.DocumentNumber;
                            objRequisition.DocumentTypeInfo = DocumentType.Requisition;
                            objRequisition.DocumentStatusInfo = objDocument.DocumentStatusInfo;
                            objRequisition.DocumentSourceTypeInfo = objDocument.DocumentSourceTypeInfo;
                            objRequisition.DocumentId = GetLongValue(sqlDr, ReqSqlConstants.COL_REQUISITION_ID);
                            objRequisition.RequesterId = GetLongValue(sqlDr, ReqSqlConstants.COL_REQUESTER_ID);
                            objRequisition.BusinessUnitId = GetLongValue(sqlDr, ReqSqlConstants.COL_BUID);
                            objRequisition.BusinessUnitName = GetStringValue(sqlDr, ReqSqlConstants.COL_BUSINESSUNITNAME);
                            objRequisition.Currency = GetStringValue(sqlDr, ReqSqlConstants.COL_CURRENCY);
                            objRequisition.Tax = ReqDAOManager.GetNullableDecimalValue(sqlDr, ReqSqlConstants.COL_TAX);
                            objRequisition.Shipping = ReqDAOManager.GetNullableDecimalValue(sqlDr, ReqSqlConstants.COL_SHIPPING);
                            objRequisition.AdditionalCharges = ReqDAOManager.GetNullableDecimalValue(sqlDr, ReqSqlConstants.COL_ADDITIONAL_CHARGES);
                            objRequisition.TotalAmount = ReqDAOManager.GetNullableDecimalValue(sqlDr, ReqSqlConstants.COL_REQUISITION_AMOUNT);
                            objRequisition.ItemTotalAmount = ReqDAOManager.GetNullableDecimalValue(sqlDr, ReqSqlConstants.COL_ITEM_TOTAL);
                            objRequisition.RequesterName = GetStringValue(sqlDr, ReqSqlConstants.COL_REQUESTER_NAME);
                            objRequisition.ApproveURL = GetStringValue(sqlDr, ReqSqlConstants.COL_APPROVE_URL);
                            objRequisition.RejectURL = GetStringValue(sqlDr, ReqSqlConstants.COL_REJECT_URL);
                            objRequisition.MaterialItemCount = GetIntValue(sqlDr, ReqSqlConstants.COL_MATERIAL_ITEM_COUNT);
                            objRequisition.ServiceItemCount = GetIntValue(sqlDr, ReqSqlConstants.COL_SERVICE_ITEM_COUNT);
                            objRequisition.AdvanceItemCount = GetIntValue(sqlDr, ReqSqlConstants.COL_ADVANCE_ITEM_COUNT);
                            objRequisition.NextLineNumber = GetLongValue(sqlDr, ReqSqlConstants.COL_NEXTLINENUMBER);
                            objRequisition.RequisitionSource = (RequisitionSource)GetTinyIntValue(sqlDr, ReqSqlConstants.COL_REQUISITION_SOURCE);
                            objRequisition.ParentDocumentItemtotal = GetDecimalValue(sqlDr, ReqSqlConstants.COL_PARENTDOCUMENTITEMTOTAL);
                            objRequisition.ShiptoLocation = new ShiptoLocation
                            {
                                ShiptoLocationId = GetIntValue(sqlDr, ReqSqlConstants.COL_SHIPTOLOC_ID),
                                ShiptoLocationName = GetStringValue(sqlDr, ReqSqlConstants.COL_SHIPTOLOC_NAME),
                                Address = new P2PAddress
                                {
                                    AddressLine1 = GetStringValue(sqlDr, ReqSqlConstants.COL_ADDRESS1),
                                    AddressLine2 = GetStringValue(sqlDr, ReqSqlConstants.COL_ADDRESS2),
                                    AddressLine3 = GetStringValue(sqlDr, ReqSqlConstants.COL_ADDRESS3),
                                    City = GetStringValue(sqlDr, ReqSqlConstants.COL_CITY),
                                    State = GetStringValue(sqlDr, ReqSqlConstants.COL_STATE),
                                    Zip = GetStringValue(sqlDr, ReqSqlConstants.COL_ZIP),
                                    CountryCode = GetStringValue(sqlDr, ReqSqlConstants.COL_COUNTRYCODE),
                                    CountryName = GetStringValue(sqlDr, ReqSqlConstants.COL_COUNTRYNAME),
                                    StateCode = GetStringValue(sqlDr, ReqSqlConstants.COL_STATECODE)
                                },
                            };
                            objRequisition.BilltoLocation = new BilltoLocation
                            {
                                BilltoLocationId = GetIntValue(sqlDr, ReqSqlConstants.COL_BILLTOLOC_ID),
                                BilltoLocationName = GetStringValue(sqlDr, ReqSqlConstants.COL_BILLTOLOC_NAME),
                                BilltoLocationNumber = GetStringValue(sqlDr, ReqSqlConstants.COL_BILLTOLOC_NUMBER),
                                Address =
                                {
                                    AddressLine1 = GetStringValue(sqlDr, ReqSqlConstants.BILLTO_COL_ADDRESS1),
                                    AddressLine2 = GetStringValue(sqlDr, ReqSqlConstants.BILLTO_COL_ADDRESS2),
                                    AddressLine3 = GetStringValue(sqlDr, ReqSqlConstants.BILLTO_COL_ADDRESS3),
                                    City = GetStringValue(sqlDr, ReqSqlConstants.BILLTO_COL_CITY),
                                    State = GetStringValue(sqlDr, ReqSqlConstants.BILLTO_COL_STATE),
                                    CountryName = GetStringValue(sqlDr, ReqSqlConstants.BILLTO_COL_COUNTRYNAME),
                                    Zip = GetStringValue(sqlDr, ReqSqlConstants.BILLTO_COL_ZIP),
                                    EmailAddress = GetStringValue(sqlDr, ReqSqlConstants.BILLTO_COL_EmailAddress),
                                    FaxNo=GetStringValue(sqlDr,ReqSqlConstants.BILLTO_COL_FAXNO)
                                }
                            };
                            objRequisition.DelivertoLocation = new DelivertoLocation
                            {
                                DelivertoLocationId = GetIntValue(sqlDr, ReqSqlConstants.COL_DELIVERTOLOCATION_ID),
                                DelivertoLocationName = GetStringValue(sqlDr, ReqSqlConstants.COL_DELIVERTOLOC_NAME),
                                DeliverTo = GetStringValue(sqlDr, ReqSqlConstants.COL_DELIVERTO)
                            };
                            objRequisition.OnBehalfOf = GetLongValue(sqlDr, ReqSqlConstants.COL_ONBEHALFOF);
                            objRequisition.OnBehalfOfName = GetStringValue(sqlDr, ReqSqlConstants.COL_ONBEHALFOFNAME);
                            objDocument.DocumentBUList.ToList().ForEach(data => { objRequisition.DocumentBUList.Add(data); });
                            objRequisition.DocumentLOBDetails = new List<DocumentLOBDetails>();
                            objDocument.DocumentLOBDetails.ToList().ForEach(data => { objRequisition.DocumentLOBDetails.Add(data); });
                            objRequisition.IsCatalogItemsExists = GetBoolValue(sqlDr, ReqSqlConstants.COL_IS_CATALOG_ITEMS_EXISTS);
                            objRequisition.WorkOrderNumber = GetStringValue(sqlDr, ReqSqlConstants.COL_WORKORDERNO);
                            objRequisition.ERPOrderType = GetIntValue(sqlDr, ReqSqlConstants.COL_ERPORDERTYPE);
                            // CommentCount is passed as 0 from SP as Count is handled in RequisitionDocumentManager
                            objRequisition.CommentCount = GetIntValue(sqlDr, ReqSqlConstants.COL_COMMENTCOUNT);
                            objRequisition.CapitalCode = GetStringValue(sqlDr, ReqSqlConstants.COL_CAPITALCODE);
                            objRequisition.IsUrgent = GetBoolValue(sqlDr, ReqSqlConstants.COL_ISURGENT);
                            objRequisition.EntityId = objDocument.EntityId;
                            objRequisition.EntityDetailCode = objDocument.EntityDetailCode;
                            objRequisition.ProgramId = GetLongValue(sqlDr, ReqSqlConstants.COL_PROGRAM_ID);
                            objRequisition.CustomAttrFormId = GetLongValue(sqlDr, ReqSqlConstants.COL_FORMID);
                            objRequisition.CustomAttrFormIdForItem = GetLongValue(sqlDr, ReqSqlConstants.COL_ITEMFORMID);
                            objRequisition.CustomAttrFormIdForSplit = GetLongValue(sqlDr, ReqSqlConstants.COL_SPLITFORMID);
                            objRequisition.PurchaseType = GetTinyIntValue(sqlDr, ReqSqlConstants.COL_PURCHASETYPE);
                            objRequisition.PurchaseTypeDescription = GetStringValue(sqlDr, ReqSqlConstants.COL_PURCHASETYPEDESC);
                            objRequisition.SourceSystemInfo = new SourceSystemInfo()
                            {
                                SourceSystemId = GetIntValue(sqlDr, ReqSqlConstants.COL_SOURCESYSTEMID),
                                SourceSystemName = GetStringValue(sqlDr, ReqSqlConstants.COL_SOURCESYSTEMNAME)
                            };
                            objRequisition.ItemPASCodes = GetStringValue(sqlDr, ReqSqlConstants.COL_ItemPASCodes);
                            objRequisition.TotalAmountChange = GetDecimalValue(sqlDr, ReqSqlConstants.COL_REQUISITIONTOTALCHANGE);
                            objRequisition.Billable = ReqDAOManager.GetNullableBooleanValue(sqlDr, ReqSqlConstants.COL_BILLABLE);
                            objRequisition.CostApprover = GetLongValue(sqlDr, ReqSqlConstants.COL_CostApprover);
                        }
                        if (sqlDr.NextResult())
                        {
                            objRequisition.lstLOBEntityConfiguration = new List<LOBEntityConfiguration>();
                            while (sqlDr.Read())
                            {
                                LOBEntityConfiguration obj = new LOBEntityConfiguration();
                                obj.DivisionEntityCode = GetLongValue(sqlDr, ReqSqlConstants.COL_DivisionEntityCode);
                                obj.EntityId = GetIntValue(sqlDr, ReqSqlConstants.COL_EntityId);
                                obj.IdentificationTypeID = GetIntValue(sqlDr, ReqSqlConstants.COL_IDENTIFICATIONTYPEID);
                                objRequisition.lstLOBEntityConfiguration.Add(obj);
                            }
                        }
                        if (sqlDr.NextResult())
                        {
                            while (sqlDr.Read())
                            {
                                objRequisition.BiddingValidation = new BiddingValidation
                                {
                                    NoofSendForBidddingItems = GetIntValue(sqlDr, ReqSqlConstants.COL_NOOFSENDFORBIDDDINGITEMS),
                                    NoofPartiallyOrderedItems = GetIntValue(sqlDr, ReqSqlConstants.COL_NOOFPARTIALLYORDEREDITEMS),
                                    NoofFullyOrderedItems = GetIntValue(sqlDr, ReqSqlConstants.COL_NOOFFULLYORDEREDITEMS),
                                    NoofUnOrderedItems = GetIntValue(sqlDr, ReqSqlConstants.COL_NOOFUNORDEREDITEMS),
                                    NoofNonSupportedItems = GetIntValue(sqlDr, ReqSqlConstants.COL_NOOFNONSUPPORTEDITEMS),
                                };
                            }
                        }
                        if (sqlDr.NextResult())
                        {
                            while (sqlDr.Read())
                            {
                                objRequisition.IsOverallLimitAllowed = GetBoolValue(sqlDr, ReqSqlConstants.COL_ISOVERALLLIMITALLOWED);
                            }
                        }

                        if (sqlDr.NextResult())
                        {
                            objRequisition.BilltoLocation.lstBillLocation = new List<RegistrationDetails>();
                            while (sqlDr.Read())
                            {
                                RegistrationDetails obj = new RegistrationDetails();
                                obj.CompanyIdentification = GetStringValue(sqlDr, ReqSqlConstants.COL_COMPANYIDENTIFICATION);
                                obj.CompanyIdentificationDisplayName = GetStringValue(sqlDr, ReqSqlConstants.COL_COMPANYIDENTIFICATIONDISPLAYNAME);
                                objRequisition.BilltoLocation.lstBillLocation.Add(obj);
                            }
                        }

                        if (sqlDr.NextResult())
                        {
                            objRequisition.ShiptoLocation.lstShipLocation = new List<RegistrationDetails>();
                            while (sqlDr.Read())
                            {
                                RegistrationDetails obj = new RegistrationDetails();
                                obj.CompanyIdentification = GetStringValue(sqlDr, ReqSqlConstants.COL_COMPANYIDENTIFICATION);
                                obj.CompanyIdentificationDisplayName = GetStringValue(sqlDr, ReqSqlConstants.COL_COMPANYIDENTIFICATIONDISPLAYNAME);
                                objRequisition.ShiptoLocation.lstShipLocation.Add(obj);
                            }
                        }

                        List<DocumentStakeHolder> stakeholderlist = objDocument.DocumentStakeHolderList.Where(e => (e.StakeholderTypeInfo == StakeholderType.Approver && (e.ContactCode == userId || e.ProxyContactCode == userId))).ToList();
                        stakeholderlist.ToList().ForEach(data => { objRequisition.DocumentStakeHolderList.Add(data); });

                        objRefCountingDataReader = (RefCountingDataReader)sqlHelper.ExecuteReader(ReqSqlConstants.USP_P2P_REQ_GETREQUISITIONENTITYDETAILSBYID, new object[] { documentCode });
                        if (objRefCountingDataReader != null)
                        {
                            sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;
                            while (sqlDr.Read())
                            {
                                objRequisition.DocumentAdditionalEntitiesInfoList.Add(new DocumentAdditionalEntityInfo
                                {
                                    EntityDetailCode = GetLongValue(sqlDr, ReqSqlConstants.COL_SPLIT_ITEM_ENTITYDETAILCODE),
                                    EntityId = GetIntValue(sqlDr, ReqSqlConstants.COL_ENTITY_ID),
                                    FieldName = GetStringValue(sqlDr, ReqSqlConstants.COL_FIELD_NAME),
                                    EntityDisplayName = GetStringValue(sqlDr, ReqSqlConstants.COL_ENTITY_DISPLAY_NAME),
                                    EntityType = GetStringValue(sqlDr, ReqSqlConstants.COL_ENTITY_TYPE)
                                });
                            }
                        }

                        return objRequisition;
                    }
                }
                else
                {
                    if (Log.IsWarnEnabled)
                        Log.Warn("In GetBasicDetailsById Method Parameter: id is not greater than 0.");
                }
            }
            finally
            {
                if (!ReferenceEquals(objRefCountingDataReader, null) && !objRefCountingDataReader.IsClosed)
                {
                    objRefCountingDataReader.Close();
                    objRefCountingDataReader.Dispose();
                }

                LogHelper.LogInfo(Log, "GetBasicDetailsById Method Ended for id=" + documentCode);
            }

            return new Requisition();
        }

        public P2PDocument GetOrderBasicDetailsByIdforItems(long id, long userId, int typeOfUser = 0)
        {
            throw new NotImplementedException();
        }

        public List<P2PDocumentValidationInfo> GetValidationDetailsById(long documentCode, bool isPartnerMandatoryInRequisition = false, bool IsOrderingLocationMandatory = false, int populateDefaultNeedByDateByDays = 0, string PartnerStatuses = "2,7", string restrictedPartnerRelationshipTypes = "", bool EnablePastDateDocProcess = false)
        {
            RefCountingDataReader objRefCountingDataReader = null;
            List<P2PDocumentValidationInfo> lstValidationInfo = new List<P2PDocumentValidationInfo>();
            try
            {
                LogHelper.LogInfo(Log, "GetValidationDetailsById Method Started for id=" + documentCode);

                var sqlHelper = ContextSqlConn;
                objRefCountingDataReader = (RefCountingDataReader)sqlHelper.ExecuteReader(ReqSqlConstants.USP_P2P_REQ_GETVALIDATIONERRORCODEBYID, new object[] { documentCode, isPartnerMandatoryInRequisition, IsOrderingLocationMandatory, populateDefaultNeedByDateByDays, PartnerStatuses, restrictedPartnerRelationshipTypes, EnablePastDateDocProcess });
                if (objRefCountingDataReader != null)
                {
                    var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;
                    while (sqlDr.Read())
                    {
                        lstValidationInfo.Add(new P2PDocumentValidationInfo
                        {
                            TabIndexId = GetIntValue(sqlDr, ReqSqlConstants.COL_TAB_ID),
                            ErrorCodes = GetStringValue(sqlDr, ReqSqlConstants.COL_ERRORCODE)
                        });
                    }
                }
            }
            finally
            {
                if (!ReferenceEquals(objRefCountingDataReader, null) && !objRefCountingDataReader.IsClosed)
                {
                    objRefCountingDataReader.Close();
                    objRefCountingDataReader.Dispose();
                }

                LogHelper.LogInfo(Log, "GetValidationDetailsById Method Ended for id=" + documentCode);
            }
            return lstValidationInfo;
        }
        public Document GetDocumentDetailsById(long documentCode)
        {
            Document objDocument = new Document();
            try
            {
                LogHelper.LogInfo(Log, "GetDocumentDetailsById Method Started for id=" + documentCode);

                var sqlHelper = ContextSqlConn;

                if (documentCode > 0)
                {

                    objDocument.DocumentCode = documentCode;
                    objDocument.DocumentTypeInfo = DocumentType.Requisition;
                    objDocument.IsDocumentDetails = true;
                    objDocument.IsAddtionalDetails = false;
                    objDocument.IsStakeholderDetails = false;
                    objDocument.CompanyName = UserContext.ClientName;


                    SQLDocumentDAO sqlDocumentDAO = new SQLDocumentDAO();

                    sqlDocumentDAO.ReliableSqlDatabase = sqlHelper;
                    sqlDocumentDAO.UserContext = UserContext;
                    sqlDocumentDAO.GepConfiguration = GepConfiguration;

                    objDocument = sqlDocumentDAO.GetDocumentDetailsById(objDocument, true);
                    objDocument.CreatedBy = UserContext.ContactCode;//GetCreatorById(documentCode);
                }
            }
            finally
            {

                LogHelper.LogInfo(Log, "GetBasicDetailsById Method Ended for id=" + documentCode);
            }

            return objDocument;
        }

        public long GetCreatorById(long documentCode)
        {
            RefCountingDataReader objRefCountingDataReader = null;
            long OnBehalfOf = 0;
            try
            {
                LogHelper.LogInfo(Log, "Requisition GetCreatorById Method Started for lineItemId=" + documentCode);

                if (documentCode > 0)
                {
                    if (Log.IsDebugEnabled)
                        Log.Debug(string.Format(CultureInfo.InvariantCulture, "Requisition GetCreatorById sp usp_P2P_PO_GetOBOUserByDocumentCode with parameter: documentCode={0} was called.", documentCode));

                    objRefCountingDataReader = (RefCountingDataReader)
                    ContextSqlConn.ExecuteReader(ReqSqlConstants.USP_P2P_REQ_GETOBOUSERBYDOCUMENTCODE, documentCode);
                    if (objRefCountingDataReader != null)
                    {
                        var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;
                        if (sqlDr.Read())
                        {
                            OnBehalfOf = GetLongValue(sqlDr, ReqSqlConstants.COL_ONBEHALFOF);
                        }
                        return OnBehalfOf;
                    }
                }
                else
                {
                    if (Log.IsWarnEnabled)
                        Log.Warn("In Requisition GetCreatorById method documentCode parameter is less than or equal to 0.");
                }
            }
            finally
            {
                if (!ReferenceEquals(objRefCountingDataReader, null) && !objRefCountingDataReader.IsClosed)
                {
                    objRefCountingDataReader.Close();
                    objRefCountingDataReader.Dispose();
                }

                LogHelper.LogInfo(Log, "Requisition GetCreatorById Method Ended for documentCode=" + documentCode);
            }

            return OnBehalfOf;
        }


        public ICollection<P2PItem> GetLineItemBasicDetails(long id, ItemType itemType, int startIndex, int pageSize, string sortBy, string sortOrder, int typeOfUser = 0, int searchInField = 1, string searchFor = "", int MaxPrecessionValue = 0, int MaxPrecessionValueTotal = 0, int MaxPrecessionValueForTaxAndCharges = 0, bool isOrderBasedCreditMemo = false, bool CommentsCountRequired = true, long parentDocumentCode = 0)
        {
            RefCountingDataReader objRefCountingDataReader = null;
            try
            {
                LogHelper.LogInfo(Log, "GetLineItemBasicDetails Method Started for id=" + id);

                if (id > 0)
                {
                    int IsRequestedDocumentTypeRFX = 0; // 1-for RFP , Others 0
                    objRefCountingDataReader =
                    (RefCountingDataReader)
                    ContextSqlConn.ExecuteReader(ReqSqlConstants.USP_P2P_REQ_GETREQUISITIONLINEITEMS,
                                                                     new object[] { id, itemType, startIndex, pageSize, sortBy, sortOrder, UserContext.ContactCode, typeOfUser, IsRequestedDocumentTypeRFX });

                    if (Log.IsDebugEnabled)
                        Log.Debug(string.Concat("In GetLineItemBasicDetails Method with parameter: id=" + id, ",itemType=" + itemType,
                                                                                                          ",startIndex" + startIndex, ",pageSize=" + pageSize,
                                                                                                          ",sortBy" + sortBy, ",sortOrder=" + sortOrder, ",userId=" + UserContext.ContactCode, ",typeOfUser=" + typeOfUser, NumberFormatInfo.InvariantInfo));

                    if (objRefCountingDataReader != null)
                    {

                        var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;
                        var lstRequisitionItems = new List<P2PItem>();
                        while (sqlDr.Read())
                        {
                            var objRequisitionItem = new RequisitionItem
                            {
                                DocumentItemId = GetLongValue(sqlDr, ReqSqlConstants.COL_REQUISITION_ITEM_ID),
                                P2PLineItemId = GetLongValue(sqlDr, ReqSqlConstants.COL_P2P_LINE_ITEM_ID),
                                DocumentId = GetLongValue(sqlDr, ReqSqlConstants.COL_REQUISITION_ID),
                                ShortName = GetStringValue(sqlDr, ReqSqlConstants.COL_SHORT_NAME),
                                Description = GetStringValue(sqlDr, ReqSqlConstants.COL_DESCRIPTION),
                                UnitPrice = ReqDAOManager.GetNullableDecimalValue(sqlDr, ReqSqlConstants.COL_UNIT_PRICE),
                                Quantity = GetDecimalValue(sqlDr, ReqSqlConstants.COL_QUANTITY),
                                UOM = GetStringValue(sqlDr, ReqSqlConstants.COL_UOM),
                                UOMDesc = GetStringValue(sqlDr, ReqSqlConstants.COL_UOM_DESC),
                                DateRequested = GetDateTimeValue(sqlDr, ReqSqlConstants.COL_DATE_REQUESTED),
                                DateNeeded = GetDateTimeValue(sqlDr, ReqSqlConstants.COL_DATE_NEEDED),
                                PartnerCode = GetDecimalValue(sqlDr, ReqSqlConstants.COL_PARTNER_CODE),
                                PartnerName = GetStringValue(sqlDr, ReqSqlConstants.COL_PARTNER_NAME),
                                CategoryId = GetLongValue(sqlDr, ReqSqlConstants.COL_CATEGORY_ID),
                                CategoryName = GetStringValue(sqlDr, ReqSqlConstants.COL_CATEGORY_NAME),
                                ManufacturerName = GetStringValue(sqlDr, ReqSqlConstants.COL_MANUFACTURER_NAME),
                                ManufacturerPartNumber = GetStringValue(sqlDr, ReqSqlConstants.COL_MANUFACTURER_PART_NUMBER),
                                ItemType = (ItemType)GetTinyIntValue(sqlDr, ReqSqlConstants.COL_ITEM_TYPE_ID),
                                Tax = ReqDAOManager.GetNullableDecimalValue(sqlDr, ReqSqlConstants.COL_LINE_ITEM_TAX),
                                ShippingCharges = ReqDAOManager.GetNullableDecimalValue(sqlDr, ReqSqlConstants.COL_SHIPPING_CHARGES),
                                AdditionalCharges = ReqDAOManager.GetNullableDecimalValue(sqlDr, ReqSqlConstants.COL_ADDITIONAL_CHARGES),
                                ItemTotalAmount = (ReqDAOManager.GetNullableDecimalValue(sqlDr, ReqSqlConstants.COL_UNIT_PRICE) * GetDecimalValue(sqlDr, ReqSqlConstants.COL_QUANTITY)),
                                StartDate = GetDateTimeValue(sqlDr, ReqSqlConstants.COL_START_DATE),
                                EndDate = GetDateTimeValue(sqlDr, ReqSqlConstants.COL_END_DATE),
                                TotalRecords = GetIntValue(sqlDr, ReqSqlConstants.COL_TOTAL_RECORDS),
                                SourceType = (ItemSourceType)GetIntValue(sqlDr, ReqSqlConstants.COL_SOURCE_TYPE),
                                MinimumOrderQuantity = GetDecimalValue(sqlDr, ReqSqlConstants.COL_MIN_ORDER_QUANTITY),
                                MaximumOrderQuantity = GetDecimalValue(sqlDr, ReqSqlConstants.COL_MAX_ORDER_QUANTITY),
                                Banding = GetIntValue(sqlDr, ReqSqlConstants.COL_BANDING),
                                //  ItemApprovalStatus = (DocumentStatus)GetIntValue(sqlDr, ReqSqlConstants.COL_REQUISITION_STATUS),//Status from Requisition Table.
                                ItemCode = GetLongValue(sqlDr, ReqSqlConstants.COL_ITEM_CODE),
                                RequisitionStatus = (DocumentStatus)GetIntValue(sqlDr, ReqSqlConstants.COL_DOCUMENT_STATUS),
                                AllowDecimalsForUom = GetBoolValue(sqlDr, ReqSqlConstants.COL_UOM_ALLOWDECIMAL),
                                Currency = GetStringValue(sqlDr, ReqSqlConstants.COL_CURRENCY),
                                IsTaxExempt = GetBoolValue(sqlDr, ReqSqlConstants.COL_ISTAXEXEMPT),
                                ItemExtendedType = (ItemExtendedType)GetTinyIntValue(sqlDr, ReqSqlConstants.COL_ITEM_EXTENDED_TYPE),
                                Efforts = ReqDAOManager.GetNullableDecimalValue(sqlDr, ReqSqlConstants.COL_EFFORTS),
                                SupplierPartId = GetStringValue(sqlDr, ReqSqlConstants.COL_SUPPLIERPARTID),
                                ItemNumber = GetStringValue(sqlDr, ReqSqlConstants.COL_ITEMNUMBER),
                                CatalogItemId = GetLongValue(sqlDr, ReqSqlConstants.COL_CATALOGITEMID),
                                Billable = GetStringValue(sqlDr, ReqSqlConstants.COL_BILLABLE),
                                CommentCount = GetIntValue(sqlDr, ReqSqlConstants.COL_COMMENTCOUNT),
                                Capitalized = GetStringValue(sqlDr, ReqSqlConstants.COL_CAPITALIZED),
                                AccountNumber = GetNullableLongValue(sqlDr, ReqSqlConstants.COL_ACCOUNTNUMBER),
                                ItemLineNumber = GetLongValue(sqlDr, ReqSqlConstants.COL_LINENUMBER),
                                OrderedQuantity = GetDecimalValue(sqlDr, ReqSqlConstants.COL_ORDEREDQUANTITY),
                                IsContracted = GetBoolValue(sqlDr, ReqSqlConstants.COL_ISCONTRACTED),
                                SupplierPartAuxiliaryId = GetStringValue(sqlDr, ReqSqlConstants.COL_SUPPLIERAUXILIARYPARTID),
                                AllowEditAndInspect = (Byte)GetTinyIntValue(sqlDr, ReqSqlConstants.COL_ALLOWEDITANDINSPECT),
                                PartnerConfigurationId = GetLongValue(sqlDr, ReqSqlConstants.COL_PARTNERCONFIGURATIONID),
                                PunchoutCartReqId = GetLongValue(sqlDr, ReqSqlConstants.COL_PUNCHOUTCARTREQID),
                                ItemAdvanceAmount = GetDecimalValue(sqlDr, ReqSqlConstants.COL_ITEMADVANCEAMOUNT),
                                RecoupmentPercentage = GetDecimalValue(sqlDr, ReqSqlConstants.COL_RECOUPMENTPERCENTAGE),
                                ItemStatus = (DocumentStatus)GetTinyIntValue(sqlDr, ReqSqlConstants.COL_ITEM_STATUS),
                                OverallItemLimit = GetDecimalValue(sqlDr, ReqSqlConstants.COL_OVERALLITEMLIMIT),
                                IsOverallLimitAllowed = GetBoolValue(sqlDr, ReqSqlConstants.COL_ISOVERALLLIMITALLOWED),
                                ManufacturerModel = GetStringValue(sqlDr, ReqSqlConstants.COL_MANUFACTURER_MODEL),
                                CreatedOn = GetDateTimeValue(sqlDr, ReqSqlConstants.COL_DATE_CREATED),
                                OrderLocationId = GetLongValue(sqlDr, ReqSqlConstants.COL_ORDER_LOCATIONID),
                                OrderLocationName = GetStringValue(sqlDr, ReqSqlConstants.COL_ORDERLOCATIONNAME),
                                InternalPlantMemo = GetStringValue(sqlDr, ReqSqlConstants.COL_INTERNALPLANTMEMO),
                                Itemspecification = GetStringValue(sqlDr, ReqSqlConstants.COL_ITEMSPECIFICATION)
                            };
                            lstRequisitionItems.Add(objRequisitionItem);
                            objRequisitionItem.Taxes = new List<Taxes>();

                            RefCountingDataReader objNewRefCountingDataReader = null;
                            try
                            {

                                objNewRefCountingDataReader = (RefCountingDataReader)ContextSqlConn.ExecuteReader(ReqSqlConstants.USP_P2P_REQ_GETLINEITEMTAXDETAILS,
                                                                    new object[] { objRequisitionItem.DocumentItemId });

                                if (objNewRefCountingDataReader != null)
                                {
                                    var sqlTaxDr = (SqlDataReader)objNewRefCountingDataReader.InnerReader;
                                    while (sqlTaxDr.Read())
                                    {
                                        var objTaxesAndCharge = new Taxes
                                        {
                                            TaxId = GetIntValue(sqlTaxDr, ReqSqlConstants.COL_TAXID),
                                            TaxDescription = GetStringValue(sqlTaxDr, ReqSqlConstants.COL_TAX_DESC),
                                            TaxType = (TaxType)GetTinyIntValue(sqlTaxDr, ReqSqlConstants.COL_TAX_TYPE),
                                            TaxMode = (SplitType)GetTinyIntValue(sqlTaxDr, ReqSqlConstants.COL_TAX_MODE),
                                            TaxValue = GetDecimalValue(sqlTaxDr, ReqSqlConstants.COL_TAX_VALUE),
                                            TaxCode = GetStringValue(sqlTaxDr, ReqSqlConstants.COL_TAX_CODE)
                                        };
                                        objRequisitionItem.Taxes.Add(objTaxesAndCharge);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                LogHelper.LogError(Log, "GetLineItemBasicDetails and GetLineItemTax Method for Item id= " + objRequisitionItem.DocumentItemId, ex);
                                throw ex;
                            }
                            finally
                            {
                                if (!ReferenceEquals(objNewRefCountingDataReader, null) && !objNewRefCountingDataReader.IsClosed)
                                {
                                    objNewRefCountingDataReader.Close();
                                    objNewRefCountingDataReader.Dispose();
                                }
                            }
                        }
                        //REQ-4822: MobileApp Gets Line Items Splits 
                        if (lstRequisitionItems.Any())
                        {
                            List<long> requisitionItemIds = lstRequisitionItems.Select(x => x.DocumentItemId).ToList();
                            List<RequisitionSplitItems> requisitionSplitItems = GetAllRequisitionAccountingDetails(id, requisitionItemIds);
                            foreach (RequisitionItem ri in lstRequisitionItems)
                            {
                                var reqSplitItems = requisitionSplitItems.Where(x => x.DocumentItemId == ri.DocumentItemId).ToList();
                                ri.ItemSplitsDetail = reqSplitItems;
                            }
                        }
                        return lstRequisitionItems;
                    }
                }
                else
                {
                    if (Log.IsWarnEnabled)
                        Log.Warn("In GetLineItemBasicDetails Method Parameter: id is not greater than 0.");
                }
            }
            catch(Exception ex)
            {
                LogHelper.LogError(Log, "Error occured while GetLineItemBasicDetails for documentcode=" + id, ex);
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

                LogHelper.LogInfo(Log, "GetLineItemBasicDetails Method Ended for id=" + id);
            }

            return new List<P2PItem>();
        }

        public string GenerateDefaultName(long userId, string documentNumberFormat, long preDocumentId)
        {
            SqlConnection objSqlCon = null;
            try
            {
                LogHelper.LogInfo(Log, "GenerateDefaultName Method Started for userId=" + userId + ",documentNumberFormat" + documentNumberFormat);

                var sqlHelper = ContextSqlConn;
                objSqlCon = (SqlConnection)sqlHelper.CreateConnection();
                objSqlCon.Open();

                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("In GenerateDefaultName Method sp usp_P2P_REQ_GenerateDefaultRequisitionName with parameter: userId=" + userId, " was called."));

                string requisitionName = sqlHelper.ExecuteScalar(ReqSqlConstants.USP_P2P_REQ_GENERATEDEFAULTREQUISITIONNAME, userId, documentNumberFormat).ToString();

                return requisitionName;
            }
            catch
            {
                throw;
            }
            finally
            {
                if (!ReferenceEquals(objSqlCon, null) && objSqlCon.State != ConnectionState.Closed)
                {
                    objSqlCon.Close();
                    objSqlCon.Dispose();
                }

                LogHelper.LogInfo(Log, "GenerateDefaultName Method Ended for userId=" + userId);
            }
        }

        public long Save(P2PDocument objDoc, int MaxPrecessionValue, int MaxPrecessionValueTotal, int MaxPrecessionValueForTaxAndCharges, Dictionary<string, object> lstParam = null)
        {
            var objRequisition = (Requisition)objDoc;
            var objDocument = new Document();
            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;
            try
            {
                LogHelper.LogInfo(Log, "Requisition Save Method Started for DocumentId = " + objRequisition.DocumentId);

                objDocument.DocumentTypeInfo = DocumentType.Requisition;
                objDocument.DocumentSourceTypeInfo = objDoc.DocumentSourceTypeInfo;
                objDocument.DocumentCode = objRequisition.DocumentCode;
                objDocument.DocumentName = objRequisition.DocumentName;
                objDocument.DocumentNumber = objRequisition.DocumentNumber;
                objDocument.ExistingDocumentNumber = objRequisition.ExistingDocumentNumber;
                objDocument.IsDocumentNumberUpdatable = objRequisition.IsDocumentNumberUpdatable;
                if (objRequisition.DocumentStatusInfo == DocumentStatus.None)
                    objDocument.DocumentStatusInfo = DocumentStatus.Draft;
                else objDocument.DocumentStatusInfo = objRequisition.DocumentStatusInfo;
                if (objRequisition.RequisitionItems != null)
                {
                    objDocument.NumberofItems = objRequisition.RequisitionItems.Count(e => e.IsDeleted == false);
                    objDocument.NumberofPartners = objRequisition.RequisitionItems.Select(reqItem => reqItem.PartnerCode).Distinct().Count();
                }
                objDocument.CompanyName = UserContext.CompanyName;

                objRequisition.DocumentBUList.ToList().ForEach(data => { if (data.BusinessUnitCode > 0) objDocument.DocumentBUList.Add(data); });

                if (objRequisition.DocumentCode > 0)
                {
                    objDocument.ModifiedBy = objRequisition.ModifiedBy;
                    objDocument.UpdatedOn = objRequisition.UpdatedOn;
                    objDocument.CreatedBy = objRequisition.CreatedBy;
                    objDocument.CreatedOn = DateTime.Now;
                }
                else
                {
                    objDocument.CreatedBy = objRequisition.CreatedBy;
                    objDocument.CreatedOn = objRequisition.CreatedOn;
                }
                objDocument.IsDocumentDetails = true;
                objDocument.IsAddtionalDetails = false;
                objDocument.EntityId = objRequisition.EntityId;
                objDocument.EntityDetailCode = objRequisition.EntityDetailCode;

                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("Requisition Save SaveDocumentDetails with parameter: objRequisition=" + objRequisition.DocumentId, " was called."));

                var sqlHelper = ContextSqlConn;
                _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();


                SQLDocumentDAO sqlDocumentDAO = new SQLDocumentDAO();
                sqlDocumentDAO.SqlTransaction = _sqlTrans;
                sqlDocumentDAO.ReliableSqlDatabase = sqlHelper;
                sqlDocumentDAO.UserContext = UserContext;
                sqlDocumentDAO.GepConfiguration = GepConfiguration;


                objRequisition.DocumentCode = sqlDocumentDAO.SaveDocumentDetails(objDocument);

                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("Requisition Save sp usp_P2P_REQ_SaveRequisition with parameter: objRequisition=" + objRequisition.DocumentId, " was called."));

                var result = Convert.ToInt64(sqlHelper.ExecuteScalar(_sqlTrans, ReqSqlConstants.USP_P2P_REQ_SAVEREQUISITION,
                                                                       objRequisition.DocumentId,
                                                                       objRequisition.DocumentCode,
                                                                       objRequisition.CreatedBy,
                                                                       HtmlEncode(objRequisition.Currency),
                                                                       object.ReferenceEquals(objRequisition.BusinessUnitId, null) ? 0 : objRequisition.BusinessUnitId,
                                                                       object.ReferenceEquals(objRequisition.ShiptoLocation, null) ? 0 : objRequisition.ShiptoLocation.ShiptoLocationId,
                                                                       object.ReferenceEquals(objRequisition.BilltoLocation, null) ? 0 : objRequisition.BilltoLocation.BilltoLocationId,
                                                                       objRequisition.OnBehalfOf,
                                                                       objRequisition.WorkOrderNumber,
                                                                       objRequisition.ERPOrderType,
                                                                       objRequisition.RequisitionSource,
                                                                       object.ReferenceEquals(objRequisition.DelivertoLocation, null) ? 0 : objRequisition.DelivertoLocation.DelivertoLocationId,
                                                                       objRequisition.IsUrgent,
                                                                       objRequisition.CapitalCode,
                                                                       object.ReferenceEquals(objRequisition.DelivertoLocation, null) ? string.Empty : objRequisition.DelivertoLocation.DeliverTo,
                                                                       objRequisition.ProgramId,
                                                                       object.ReferenceEquals(objRequisition.SourceSystemInfo, null) ? 0 : objRequisition.SourceSystemInfo.SourceSystemId,
                                                                       objRequisition.PurchaseType,
                                                                       objRequisition.PurchaseTypeDescription,
                                                                       objRequisition.TotalAmount,
                                                                       (object)objRequisition.Billable ?? DBNull.Value
                                                                      ), NumberFormatInfo.InvariantInfo);

                if (objRequisition.ItemExtendedTypeIds != null && objRequisition.ItemExtendedTypeIds != "" && result > 0)
                {
                    var documentCode = Convert.ToInt64(sqlHelper.ExecuteScalar(_sqlTrans, ReqSqlConstants.USP_P2P_REQ_UPDATELINETYPEBYPURCHASETYPE, objRequisition.DocumentCode, objRequisition.ItemExtendedTypeIds, MaxPrecessionValue, MaxPrecessionValueTotal, MaxPrecessionValueForTaxAndCharges), NumberFormatInfo.InvariantInfo);
                }
                SaveRequisitionAdditionalDetails(objRequisition.DocumentCode, sqlDocumentDAO);


                if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
                    _sqlTrans.Commit();
                return result;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in Requisition Save Method  for DocumentNumber = " + objRequisition.DocumentNumber, ex);
                if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State != ConnectionState.Closed)
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
                LogHelper.LogInfo(Log, "Requisition Save Method Ended for DocumentId = " + objRequisition.DocumentId);
            }
        }

        public bool SaveDocumentAdditionalEntityInfo(long documentId, Collection<DocumentAdditionalEntityInfo> lstEntityInfo)
        {
            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;
            try
            {
                LogHelper.LogInfo(Log, "Requisition SaveDocumentAdditionalEntityInfo Method Started for DocumentItemId = " + documentId);

                var sqlHelper = ContextSqlConn;
                _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();

                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("Requisition SaveDocumentAdditionalEntityInfo sp usp_P2P_REQ_SaveReqAdditionalEntityDetails with parameter: lstEntityInfo=" + lstEntityInfo.ToJSON(), " was called."));

                bool result = false;
                DataTable dtReqItemEntities = null;
                dtReqItemEntities = P2P.DataAccessObjects.DAOHelper.ConvertToDataTable(lstEntityInfo, GetDocumentAdditionalEntityTable);

                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_SAVEREQADDITIONALENTITYDETAILS))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.Add(new SqlParameter("@documentId", SqlDbType.BigInt) { Value = documentId });
                    objSqlCommand.Parameters.Add(new SqlParameter("@tvp_P2P_DocumentAdditionalEntity", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_P2P_DOCUMENTADDITIONALENTITY,
                        Value = dtReqItemEntities
                    });

                    result = Convert.ToBoolean(sqlHelper.ExecuteScalar(objSqlCommand, _sqlTrans), NumberFormatInfo.InvariantInfo);
                }

                if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
                    _sqlTrans.Commit();
                return result;
            }
            catch(Exception ex)
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

                LogHelper.LogInfo(Log, "Requisition SaveDocumentAdditionalEntityInfo Method Ended for DocumentItemId = " + documentId);
            }
        }

        private static DocumentAdditionalField CreateAdditionalInfo(long documentCode, string fieldName, string fieldValue,
                                                                   FieldType fieldType)
        {
            var objDocumentAdditionalField = new DocumentAdditionalField
            {
                DocumentCode = documentCode,
                FieldName = fieldName,
                FieldValue = (fieldName == "TotalValue" ? (Convert.ToDecimal(fieldValue) == -1 ? string.Empty : fieldValue) : fieldValue),
                FieldType = fieldType,
                IsDeleted = false
            };

            return objDocumentAdditionalField;
        }

        //private DataTable SetRequisitionTaxandShippingTable(List<P2PItem> objP2PItems)
        //{
        //    DataTable dtP2pItems = new DataTable() { Locale = CultureInfo.InvariantCulture, TableName = ReqSqlConstants.TVP_P2P_LINEITEMSTAXANDSHIPPING };
        //    dtP2pItems.Columns.Add("Tax", typeof(decimal));
        //    dtP2pItems.Columns.Add("Shipping", typeof(decimal));
        //    dtP2pItems.Columns.Add("AdditionalCharges", typeof(decimal));
        //    dtP2pItems.Columns.Add("SupplierPartAuxiliaryId", typeof(string));
        //    dtP2pItems.Columns.Add("SupplierPartId", typeof(string));
        //    dtP2pItems.Columns.Add("LineNumber", typeof(long));
        //    dtP2pItems.Columns.Add("Quantity", typeof(decimal));
        //    dtP2pItems.Columns.Add("UnitPrice", typeof(decimal));

        //    if (objP2PItems != null && objP2PItems.Any())
        //    {
        //        foreach (var item in objP2PItems)
        //        {
        //            if (item != null)
        //            {
        //                DataRow dr = dtP2pItems.NewRow();
        //                dr["Tax"] = item.Tax == null ? 0 : item.Tax;
        //                dr["Shipping"] = item.ShippingCharges == null ? 0 : item.ShippingCharges;
        //                dr["AdditionalCharges"] = item.AdditionalCharges == null ? 0 : item.AdditionalCharges;
        //                dr["SupplierPartAuxiliaryId"] = string.IsNullOrEmpty(item.SupplierPartAuxiliaryId) ? null : item.SupplierPartAuxiliaryId;
        //                dr["SupplierPartId"] = string.IsNullOrEmpty(item.SupplierPartId) ? null : item.SupplierPartId;
        //                dr["LineNumber"] = item.ItemLineNumber;
        //                dr["Quantity"] = item.Quantity;
        //                dr["UnitPrice"] = item.UnitPrice;

        //                dtP2pItems.Rows.Add(dr);
        //            }
        //        }
        //    }
        //    return dtP2pItems;
        //}

        //public DataSet ProprateLineItemTaxandShipping(List<P2PItem> objItems, decimal Tax, decimal shipping, decimal AdditionalCharges, Int64 PunchoutCartReqId, int precessionValue = 0, int maxPrecessionforTotal = 0, int maxPrecessionForTaxesAndCharges = 0)
        //{
        //    DataSet dsResult = new DataSet();


        //    SqlConnection _sqlCon = null;
        //    SqlTransaction _sqlTrans = null;
        //    try
        //    {
        //        LogHelper.LogInfo(Log, "Requisition ProprateLineItemTaxandShipping Method Started ");

        //        var sqlHelper = ContextSqlConn;
        //        _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
        //        _sqlCon.Open();
        //        _sqlTrans = _sqlCon.BeginTransaction();

        //        if (Log.IsDebugEnabled)
        //            Log.Debug(string.Concat("Requisition ProprateLineItemTaxandShipping sp usp_P2P_REQ_ProrateLineItemTaxandShipping with parameter: P2pItems=" + objItems, " was called."));

        //        using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_PRORATELINEITEMTAXANDSHIPPING))
        //        {
        //            objSqlCommand.CommandType = CommandType.StoredProcedure;
        //            objSqlCommand.Parameters.Add(new SqlParameter("@Tax", SqlDbType.Decimal) { Value = Tax });
        //            objSqlCommand.Parameters.Add(new SqlParameter("@shipping", SqlDbType.Decimal) { Value = shipping });
        //            objSqlCommand.Parameters.Add(new SqlParameter("@AdditionalCharges", SqlDbType.Decimal) { Value = AdditionalCharges });
        //            objSqlCommand.Parameters.Add(new SqlParameter("@PunchoutCartReqId", SqlDbType.BigInt) { Value = PunchoutCartReqId });
        //            objSqlCommand.Parameters.Add(new SqlParameter("@precessionValue", SqlDbType.BigInt) { Value = precessionValue });
        //            objSqlCommand.Parameters.Add(new SqlParameter("@maxPrecessionforTotal", SqlDbType.BigInt) { Value = maxPrecessionforTotal });
        //            objSqlCommand.Parameters.Add(new SqlParameter("@maxPrecessionForTaxesAndCharges", SqlDbType.BigInt) { Value = maxPrecessionForTaxesAndCharges });
        //            objSqlCommand.Parameters.Add(new SqlParameter("@tvp_P2P_LineItemsTaxandShipping", SqlDbType.Structured)
        //            {
        //                TypeName = ReqSqlConstants.TVP_P2P_LINEITEMSTAXANDSHIPPING,
        //                Value = SetRequisitionTaxandShippingTable(objItems)
        //            });

        //            dsResult = sqlHelper.ExecuteDataSet(objSqlCommand, _sqlTrans);
        //        }

        //        if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
        //            _sqlTrans.Commit();
        //        return dsResult;
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

        //        LogHelper.LogInfo(Log, "Requisition Method Ends for ProprateLineItemTaxandShipping ");
        //    }

        //    return dsResult;
        //}
        public long SaveItem(P2PItem objItems, int precessionValue = 0, bool flipParentDocItemDetails = false, string shippingMethod = "", int maxPrecessionforTotal = 0, int maxPrecessionForTaxesAndCharges = 0)
        {
            var objRequisitionItem = (RequisitionItem)objItems;
            long reqItemId = 0;
            long documentCode = 0;
            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;
            SqlDataReader sqlDr = null;
            try
            {
                LogHelper.LogInfo(Log,
                                  "Requisition SaveItem Method Started for DocumentItemId = " +
                                  objRequisitionItem.DocumentItemId);

                var sqlHelper = ContextSqlConn;
                _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();

                if ((short)objRequisitionItem.ItemType == 3) //(short)objRequisitionItem.ItemType == 3
                {
                    if (Log.IsDebugEnabled)
                        Log.Debug(
                            string.Concat(
                                    "Requisition SaveItem sp usp_P2P_REQ_SaveAdvancedPaymentItem with parameter: objItems=" +
                                    objRequisitionItem.ToJSON(), " was called."));

                    sqlDr = (SqlDataReader)sqlHelper.ExecuteReader(_sqlTrans, ReqSqlConstants.USP_P2P_REQ_SAVEADVANCEDPAYMENTITEM,
                                        objRequisitionItem.DocumentId,
                                        objRequisitionItem.DocumentItemId,
                                        HtmlEncode(objRequisitionItem.Description),
                                        (short)objRequisitionItem.ItemType,
                                        UserContext.ContactCode,
                                        objRequisitionItem.PartnerCode,
                                        HtmlEncode(objRequisitionItem.Currency),
                                        objRequisitionItem.ItemAdvanceAmount,
                                        objRequisitionItem.DateNeeded,
                                        objRequisitionItem.RecoupmentPercentage,
                                        objRequisitionItem.ItemLineNumber,
                                        0
                                        );
                }
                else
                {

                    if (Log.IsDebugEnabled)
                        Log.Debug(
                            string.Concat(
                            "Requisition SaveItem sp usp_P2P_REQ_SaveRequisitionItem with parameter: objItems=" +
                            objRequisitionItem.ToJSON(), " was called."));

                    sqlDr = (SqlDataReader)sqlHelper.ExecuteReader(_sqlTrans, ReqSqlConstants.USP_P2P_REQ_SAVEREQUISITIONITEM,
                                                                          objRequisitionItem.DocumentId,
                                                                          objRequisitionItem.DocumentItemId,
                                                                          HtmlEncode(objRequisitionItem.Description),
                                                                          objRequisitionItem.UnitPrice != null ? Math.Round(Convert.ToDecimal(objRequisitionItem.UnitPrice, CultureInfo.CurrentCulture), precessionValue) : objRequisitionItem.UnitPrice,
                                                                          (short)objRequisitionItem.ItemType,
                                                                           Math.Round(Convert.ToDecimal(objRequisitionItem.Quantity, CultureInfo.CurrentCulture), precessionValue),
                                                                          HtmlEncode(objRequisitionItem.UOM),
                                                                          objRequisitionItem.Tax != null ? Math.Round(Convert.ToDecimal(objRequisitionItem.Tax, CultureInfo.CurrentCulture), precessionValue) : objRequisitionItem.Tax,
                                                                          objRequisitionItem.IsTaxExempt,
                                                                          objRequisitionItem.StartDate,
                                                                          objRequisitionItem.EndDate,
                                                                          objRequisitionItem.DateNeeded,
                                                                          objRequisitionItem.DateRequested,
                                                                          UserContext.ContactCode,
                                                                          objRequisitionItem.ItemCode,
                                                                          HtmlEncode(objRequisitionItem.Currency),
                                                                          (short)objRequisitionItem.SourceType,
                                                                          HtmlEncode(objRequisitionItem.ItemNumber),
                                                                          objRequisitionItem.PartnerCode,
                                                                          objRequisitionItem.SupplierPartId,
                                                                          objRequisitionItem.SupplierPartAuxiliaryId,
                                                                          (short)objRequisitionItem.ItemExtendedType,
                                                                           objRequisitionItem.Efforts != null ? Math.Round(Convert.ToDecimal(objRequisitionItem.Efforts, CultureInfo.CurrentCulture), precessionValue) : objRequisitionItem.Efforts,
                                                                           precessionValue,
                                                                           objRequisitionItem.CatalogItemId,
                                                                           objRequisitionItem.ItemLineNumber,
                                                                           objRequisitionItem.TrasmissionMode,
                                                                           objRequisitionItem.TransmissionValue,
                                                                           objRequisitionItem.PunchoutCartReqId,
                                                                           objRequisitionItem.PartnerConfigurationId,
                                                                           Convert.ToInt64(objRequisitionItem.Unspsc),
                                                                           maxPrecessionforTotal,
                                                                           maxPrecessionForTaxesAndCharges,
                                                                           objItems.OverallItemLimit,
                                                                           objRequisitionItem.BuyerContactCode,
                                                                           objRequisitionItem.OrderLocationId,
                                                                       objRequisitionItem.RemitToLocationId);
                }
                if (sqlDr != null)
                {

                    if (sqlDr.Read())
                    {
                        reqItemId = GetLongValue(sqlDr, ReqSqlConstants.COL_REQUISITION_ITEM_ID);
                        documentCode = GetLongValue(sqlDr, ReqSqlConstants.COL_DOCUMENT_CODE);
                        objItems.CategoryId = objItems.CategoryId == 0 && GetLongValue(sqlDr, ReqSqlConstants.COL_CATEGORY_ID) > 0 ?
                                               GetLongValue(sqlDr, ReqSqlConstants.COL_CATEGORY_ID) : objItems.CategoryId;
                        objItems.DateNeeded = !GetDateTimeValue(sqlDr, ReqSqlConstants.COL_DATE_NEEDED).Equals(DateTime.MinValue) ?
                            GetDateTimeValue(sqlDr, ReqSqlConstants.COL_DATE_NEEDED) : objItems.DateNeeded;

                    }
                    sqlDr.Close();
                }

                #region Save Document Additional Info

                if (reqItemId > 0 && documentCode > 0 && UserContext.Product != GEPSuite.eInterface)
                {

                    SQLDocumentDAO sqlDocumentDAO = new SQLDocumentDAO();
                    sqlDocumentDAO.SqlTransaction = _sqlTrans;
                    sqlDocumentDAO.ReliableSqlDatabase = sqlHelper;
                    sqlDocumentDAO.UserContext = UserContext;
                    sqlDocumentDAO.GepConfiguration = GepConfiguration;
                    if ((short)objRequisitionItem.ItemType != 3)//(short)objRequisitionItem.ItemType != 3
                        SaveRequisitionAdditionalDetails(documentCode, sqlDocumentDAO);
                }

                #endregion

                if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
                    _sqlTrans.Commit();
                return reqItemId;
            }
            catch(Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in Requisition SaveItem Method " , ex);
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
                if (!ReferenceEquals(sqlDr, null) && !sqlDr.IsClosed)
                {
                    sqlDr.Close();
                    sqlDr.Dispose();
                }
                if (!ReferenceEquals(_sqlCon, null) && _sqlCon.State != ConnectionState.Closed)
                {
                    _sqlCon.Close();
                    _sqlCon.Dispose();
                }

                LogHelper.LogInfo(Log,
                                  "Requisition SaveItem Method Ended for DocumentItemId = " +
                                  objRequisitionItem.DocumentItemId);
            }
        }


        public long SaveItemAdditionDetails(P2PItem objItems, int precessionValue, int maxPrecessionforTotal = 0, int maxPrecessionForTaxesAndCharges = 0)
        {
            var objRequisitionItem = (RequisitionItem)objItems;
            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;
            SqlDataReader sqlDr = null;
            try
            {
                LogHelper.LogInfo(Log, "Requisition SaveItemAdditionDetails Method Started for DocumentItemId = " + objRequisitionItem.DocumentItemId);

                //var sqlHelper = SqlHelper(UserContext.ClientName);
                var sqlHelper = ContextSqlConn;
                _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();

                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("Requisition SaveItemAdditionDetails sp usp_P2P_REQ_SaveReqItemsAdditionalDetails with parameter: objItems=" + objRequisitionItem.ToJSON(), " was called."));


                sqlDr = (SqlDataReader)sqlHelper.ExecuteReader(_sqlTrans, ReqSqlConstants.USP_P2P_REQ_SAVEREQITEMADDITIONALDETAILS,
                                                                       objRequisitionItem.DocumentItemId,
                                                                       objRequisitionItem.DocumentId,
                                                                       objRequisitionItem.AdditionalCharges != null ? Math.Round(Convert.ToDecimal(objRequisitionItem.AdditionalCharges, CultureInfo.CurrentCulture), precessionValue) : objRequisitionItem.AdditionalCharges,
                                                                       objRequisitionItem.ShippingCharges != null ? Math.Round(Convert.ToDecimal(objRequisitionItem.ShippingCharges, CultureInfo.CurrentCulture), precessionValue) : objRequisitionItem.ShippingCharges,
                                                                       objRequisitionItem.DateNeeded,
                                                                       objRequisitionItem.DateRequested,
                                                                       UserContext.ContactCode,
                                                                       precessionValue,
                                                                       maxPrecessionforTotal,
                                                                       maxPrecessionForTaxesAndCharges
                                                                     );

                long reqItemId = 0;
                long documentCode = 0;
                if (sqlDr != null)
                {

                    if (sqlDr.Read())
                    {
                        reqItemId = GetLongValue(sqlDr, ReqSqlConstants.COL_REQUISITION_ITEM_ID);
                        documentCode = GetLongValue(sqlDr, ReqSqlConstants.COL_DOCUMENT_CODE);
                    }
                    sqlDr.Close();
                }

                #region Save Document Additional Info

                if (reqItemId > 0 && documentCode > 0 && UserContext.Product != GEPSuite.eInterface)
                {

                    SQLDocumentDAO sqlDocumentDAO = new SQLDocumentDAO();
                    sqlDocumentDAO.SqlTransaction = _sqlTrans;
                    sqlDocumentDAO.ReliableSqlDatabase = sqlHelper;
                    sqlDocumentDAO.UserContext = UserContext;
                    sqlDocumentDAO.GepConfiguration = GepConfiguration;


                    SaveRequisitionAdditionalDetails(documentCode, sqlDocumentDAO);
                }

                #endregion

                if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
                    _sqlTrans.Commit();
                return reqItemId;
            }
            catch(Exception ex)
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
                if (!ReferenceEquals(sqlDr, null) && !sqlDr.IsClosed)
                {
                    sqlDr.Close();
                    sqlDr.Dispose();
                }

                LogHelper.LogInfo(Log, "Requisition SaveItemAdditionDetails Method Ended for DocumentItemId = " + objRequisitionItem.DocumentItemId);
            }
        }

        public long SaveItemPartnerDetails(P2PItem objItems, int precessionValue)
        {
            var objRequisitionItem = (RequisitionItem)objItems;
            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;
            try
            {
                LogHelper.LogInfo(Log, "Requisition SaveItemPartnerDetails Method Started for DocumentItemId=" + objRequisitionItem.DocumentItemId);

                //var sqlHelper = SqlHelper(UserContext.ClientName);
                _sqlCon = (SqlConnection)ContextSqlConn.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();

                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("Requisition SaveItemPartnerDetails sp usp_P2P_REQ_SaveRequisitionItemPartners with parameter: objItems=" + objItems.ToJSON(), " was called."));

                var result = Convert.ToInt64(ContextSqlConn.ExecuteScalar(_sqlTrans, ReqSqlConstants.USP_P2P_REQ_SAVEREQUISITIONITEMPARTNERS,
                                                                       objRequisitionItem.DocumentItemId,
                                                                       objRequisitionItem.PartnerCode,
                                                                       HtmlEncode(objRequisitionItem.ManufacturerName),
                                                                       HtmlEncode(objRequisitionItem.ManufacturerPartNumber),
                                                                       objRequisitionItem.ModifiedBy,
                                                                       objRequisitionItem.Quantity,
                                                                       objRequisitionItem.OrderLocationId,
                                                                       objRequisitionItem.PartnerContactId,
                                                                       objRequisitionItem.TrasmissionMode,
                                                                       objRequisitionItem.TransmissionValue,
                                                                       objRequisitionItem.ManufacturerModel
                                                                      ), NumberFormatInfo.InvariantInfo);
                if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
                    _sqlTrans.Commit();
                return result;
            }
            catch
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
                throw;
            }
            finally
            {
                if (!ReferenceEquals(_sqlCon, null) && _sqlCon.State != ConnectionState.Closed)
                {
                    _sqlCon.Close();
                    _sqlCon.Dispose();
                }

                LogHelper.LogInfo(Log, "Requisition SaveItemPartnerDetails Method Ended for DocumentItemId=" + objRequisitionItem.DocumentItemId);
            }

        }

        public long SaveItemShippingDetails(long documentLineItemShippingId, long documentItemId, string shippingMethod, int shiptoLocationId, int delivertoLocationId, decimal quantity, decimal totalQuantity, long userid, int maxPrecessionValue, int maxPrecessionValueTotal, int maxPrecessionValueForTaxAndCharges, string deliverTo = "")
        {
            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;
            SqlDataReader sqlDr = null;
            try
            {
                LogHelper.LogInfo(Log, "Requisition SaveItemShippingDetails Method Started for documentLineItemShippingId=" + documentLineItemShippingId);

                //var sqlHelper = SqlHelper(UserContext.ClientName);
                var sqlHelper = ContextSqlConn;
                _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();

                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("Requisition SaveItemShippingDetails sp usp_P2P_REQ_SaveRequisitionItemShippingDetails with parameter:", " documentLineItemShippingId=" + documentLineItemShippingId +
                                                         ",documentItemId = " + documentItemId +
                                                         ",shippingMethod = " + shippingMethod +
                                                         ",shiptoLocationId = " + shiptoLocationId +
                                                         ",delivertoLocationId = " + delivertoLocationId +
                                                         ",quantity = " + quantity +
                                                         ",totalQuantity = " + totalQuantity +
                                                         ",userid = " + userid +
                                                         ",deliverTo = " + deliverTo +
                                                         " was called."));

                sqlDr = (SqlDataReader)sqlHelper.ExecuteReader(_sqlTrans, ReqSqlConstants.USP_P2P_REQ_SAVEREQUISITIONITEMSHIPPING,
                                                                       documentLineItemShippingId,
                                                                       documentItemId,
                                                                       shippingMethod,
                                                                       shiptoLocationId,
                                                                       delivertoLocationId,
                                                                       quantity,
                                                                       totalQuantity,
                                                                       userid,
                                                                       deliverTo
                                                                    );

                long reqItemShippingId = 0;
                long documentCode = 0;
                if (sqlDr != null)
                {

                    if (sqlDr.Read())
                    {
                        reqItemShippingId = GetLongValue(sqlDr, ReqSqlConstants.COL_REQITEM_SHIPPING_ID);
                        documentCode = GetLongValue(sqlDr, ReqSqlConstants.COL_DOCUMENT_CODE);
                    }
                    sqlDr.Close();
                }

                #region Save Document Additional Info

                if (reqItemShippingId > 0 && documentCode > 0)
                {

                    SQLDocumentDAO sqlDocumentDAO = new SQLDocumentDAO();
                    sqlDocumentDAO.SqlTransaction = _sqlTrans;
                    sqlDocumentDAO.ReliableSqlDatabase = sqlHelper;
                    sqlDocumentDAO.UserContext = UserContext;
                    sqlDocumentDAO.GepConfiguration = GepConfiguration;


                    SaveRequisitionAdditionalDetails(documentCode, sqlDocumentDAO);
                }

                #endregion

                if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
                    _sqlTrans.Commit();
                return reqItemShippingId;
            }
            catch
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

                throw;

            }
            finally
            {
                if (!ReferenceEquals(_sqlCon, null) && _sqlCon.State != ConnectionState.Closed)
                {
                    _sqlCon.Close();
                    _sqlCon.Dispose();
                }
                if (!ReferenceEquals(sqlDr, null) && !sqlDr.IsClosed)
                {
                    sqlDr.Close();
                    sqlDr.Dispose();
                }
                LogHelper.LogInfo(Log, "Requisition SaveItemShippingDetails Method Ended for documentLineItemShippingId=" + documentLineItemShippingId);
            }
        }

        public long SaveShippingSplitDetailsByItemIdTuned(long documentCode, long documentLineItemShippingId, long documentItemId, string shippingMethod, int shiptoLocationId, int delivertoLocationId, decimal quantity, decimal totalQuantity, long userid, string deliverTo)
        {
            throw new NotImplementedException();
        }
        public List<LOBEntityConfiguration> GetLobEntityFOBConfigurationByIdentificationTypeId(long lobEntityDetailCode, int identificationTypeID = 0)
        {
            throw new NotImplementedException();
        }
        public long SaveItemOtherDetails(P2PItem objItems, bool allowTaxCodewithAmount, string supplierStatusForValidation)
        {
            var objRequisitionItem = (RequisitionItem)objItems;
            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;
            SqlDataReader sqlDr = null;
            try
            {
                LogHelper.LogInfo(Log, "Requisition SaveItemOtherDetails Method Started for DocumentItemId=" + objRequisitionItem.DocumentItemId);
                var sqlHelper = ContextSqlConn;
                //var sqlHelper = SqlHelper(UserContext.ClientName);
                _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();

                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("Requisition SaveItemOtherDetails sp usp_P2P_REQ_SaveRequisitionItemOthers with parameter:", " DocumentItemId=" + objRequisitionItem.DocumentItemId +
                                                         ",CategoryId = " + objRequisitionItem.CategoryId +
                                                         ",ModifiedBy = " + objRequisitionItem.ModifiedBy +
                                                         ",Quantity = " + objRequisitionItem.Quantity +
                                                         " was called."));

                sqlDr = (SqlDataReader)sqlHelper.ExecuteReader(_sqlTrans, ReqSqlConstants.USP_P2P_REQ_SAVEREQUISITIONITEMOTHER,
                                                                      objRequisitionItem.DocumentItemId,
                                                                      objRequisitionItem.CategoryId,
                                                                      objRequisitionItem.ModifiedBy,
                                                                      objRequisitionItem.Quantity,
                                                                     objRequisitionItem.IsProcurable,
                                                                     objRequisitionItem.Unspsc,
                                                                     objRequisitionItem.Billable,
                                                                     objRequisitionItem.Capitalized,
                                                                     objRequisitionItem.CapitalCode
                                                                    );


                long reqItemId = 0;
                long documentCode = 0;
                if (sqlDr != null)
                {

                    if (sqlDr.Read())
                    {
                        reqItemId = GetLongValue(sqlDr, ReqSqlConstants.COL_REQUISITION_ITEM_ID);
                        documentCode = GetLongValue(sqlDr, ReqSqlConstants.COL_DOCUMENT_CODE);
                        ((RequisitionItem)objItems).DocumentId = documentCode;
                    }
                    sqlDr.Close();
                }


                #region Save Document Additional Info

                if (reqItemId > 0 && documentCode > 0 && UserContext.Product != GEPSuite.eInterface)
                {

                    SQLDocumentDAO sqlDocumentDAO = new SQLDocumentDAO();
                    sqlDocumentDAO.SqlTransaction = _sqlTrans;
                    sqlDocumentDAO.ReliableSqlDatabase = sqlHelper;
                    sqlDocumentDAO.UserContext = UserContext;
                    sqlDocumentDAO.GepConfiguration = GepConfiguration;


                    SaveRequisitionAdditionalDetails(documentCode, sqlDocumentDAO);
                }

                #endregion


                if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
                    _sqlTrans.Commit();
                return reqItemId;
            }
            catch
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


                throw;
            }
            finally
            {
                if (!ReferenceEquals(sqlDr, null) && !sqlDr.IsClosed)
                {
                    sqlDr.Close();
                    sqlDr.Dispose();
                }
                if (!ReferenceEquals(_sqlCon, null) && _sqlCon.State != ConnectionState.Closed)
                {
                    _sqlCon.Close();
                    _sqlCon.Dispose();
                }
                LogHelper.LogInfo(Log, "Requisition SaveItemOtherDetails Method Ended for DocumentItemId=" + objRequisitionItem.DocumentItemId);
            }
        }

        public void UpdateSearchIndexKey(long reqId, string searchIndexKey)
        {
            SqlConnection _sqlCon = null;
            try
            {
                LogHelper.LogInfo(Log, "UpdateSearchIndexKey Method Started for reqId=" + reqId + ", searchIndexKey=" + searchIndexKey);

                //var sqlHelper = SqlHelper(UserContext.ClientName);
                _sqlCon = (SqlConnection)ContextSqlConn.CreateConnection();
                _sqlCon.Open();

                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("UpdateSearchIndexKey sp usp_P2P_REQ_UpdateRequisitionSearchIndexKey with parameter: ", " reqId=" + reqId +
                                                         ",searchIndexKey = " + searchIndexKey +
                                                         " was called."));


                ContextSqlConn.ExecuteNonQuery(ReqSqlConstants.USP_P2P_REQ_UPDATEREQUISITIONSEARCHINDEXKEY,
                                                                     reqId, HtmlEncode(searchIndexKey)
                                                                      );

            }
            catch
            {
                throw;
            }
            finally
            {
                if (!ReferenceEquals(_sqlCon, null) && _sqlCon.State != ConnectionState.Closed)
                {
                    _sqlCon.Close();
                    _sqlCon.Dispose();
                }
                LogHelper.LogInfo(Log, "UpdateSearchIndexKey Method Ended for reqId=" + reqId + ", searchIndexKey=" + searchIndexKey);

            }
        }

        public ICollection<KeyValuePair<decimal, string>> GetAllPartnersById(long documentId, string documentIds, string BUIds = "")
        {
            RefCountingDataReader objRefCountingDataReader = null;

            try
            {
                LogHelper.LogInfo(Log, "Requisition GetAllPartnersById Method Started for documentId=" + documentId + documentIds);
                if (documentId <= 0)
                    if (Log.IsWarnEnabled)
                        Log.Warn("In Requisition GetAllPartnersById method documentId parameter is less then or equal to 0.");

                if (documentId > 0 || documentIds != "")
                {
                    if (Log.IsDebugEnabled)
                        Log.Debug(string.Concat("Requisition GetAllPartnersById sp usp_P2P_REQ_GetAllPartnersById with parameter:", " documentId=" + documentId +
                                                             " was called."));

                    objRefCountingDataReader =
                     (RefCountingDataReader)
                     ContextSqlConn.ExecuteReader(ReqSqlConstants.USP_P2P_REQ_GETALLPARTNERSBYID,
                                                                      new object[] { documentId, documentIds, BUIds });
                    if (objRefCountingDataReader != null)
                    {
                        var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;
                        var lstKvpPartnerDetails = new List<KeyValuePair<decimal, string>>();
                        while (sqlDr.Read())
                            lstKvpPartnerDetails.Add(new KeyValuePair<decimal, string>(GetDecimalValue(sqlDr, ReqSqlConstants.COL_PARTNER_CODE), GetStringValue(sqlDr, ReqSqlConstants.COL_PARTNER_NAME)));
                        return lstKvpPartnerDetails;
                    }
                }
            }
            finally
            {
                if (!ReferenceEquals(objRefCountingDataReader, null) && !objRefCountingDataReader.IsClosed)
                {
                    objRefCountingDataReader.Close();
                    objRefCountingDataReader.Dispose();
                }
                LogHelper.LogInfo(Log, "Requisition GetAllPartnersById Method Ended for documentId=" + documentId);
            }

            return new List<KeyValuePair<decimal, string>>();
        }

        public long SaveDocumentFromPreDocument(long preDocumentId, string documentName, string documentNumber, decimal partnerCode, long userId, Collection<DocumentBU> lstBU, P2PDocumentType docType, Document preDocDetails, int matchType = 0, DocumentSourceType documentSourceType = DocumentSourceType.None, int p2pDocumentSource = 0, int p2pDocumentOrigin = 0, Dictionary<string, string> dcSettings = null, long orderContactFromReq = 1, long orderingLocationId = 0, bool GetPartnerSpecificPaymentTerm = false, ItemType itemType = ItemType.None, bool isCreatedFromCO = false, bool byPassAccessRight = false, bool isFunctionalAdmin = false, string documentStatuses = "", int invoiceType = 1, int MaxPrecessionValue = 0, int MaxPrecessionValueTotal = 0, int MaxPrecessionValueForTaxAndCharges = 0, int sourceSystemId = 0, bool IsSupplierCurrency = false, bool poinvoicecretedbybuyer = false, long orderId = 0, string currencyCode = "", bool isRedirectToSmart2 = false)
        {
            throw new NotImplementedException();
        }

        public long SaveDocumentFromPreDocument(long preDocumentId, long documentCode, decimal partnerCode, long userId)
        {
            throw new NotImplementedException();
        }

        public ICollection<P2PItem> GetAllLineItemsByDocumentId(long requisitionId, ItemType itemType, int pageIndex, int pageSize, int typeOfUser = 0, int MaxPrecessionValue = 0, int MaxPrecessionValueTotal = 0, int MaxPrecessionValueForTaxAndCharges = 0)
        {
            RefCountingDataReader objRefCountingDataReader = null;
            var lstRequisitionItems = new List<P2PItem>();
            try
            {
                LogHelper.LogInfo(Log, "Requisition GetAllLineItemsByDocumentId Method Started for requisitionId=" + requisitionId);

                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("Requisition GetAllLineItemsByDocumentId sp usp_P2P_REQ_GetAllLineItemsById with parameter: ", " requisitionId=" + requisitionId +
                                                         ",pageIndex = " + pageIndex +
                                                         ",pageSize = " + pageSize +
                                                         " was called."));

                objRefCountingDataReader =
                 (RefCountingDataReader)
                 ContextSqlConn.ExecuteReader(ReqSqlConstants.USP_P2P_REQ_GETALLLINEITEMSBYID,
                                                                  new object[] { requisitionId, pageIndex, pageSize, (short)itemType });
                if (objRefCountingDataReader != null)
                {
                    var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;

                    while (sqlDr.Read())
                    {
                        var objRequisitionItem = new RequisitionItem
                        {
                            DocumentItemId = GetLongValue(sqlDr, ReqSqlConstants.COL_REQUISITION_ITEM_ID),
                            Description = GetStringValue(sqlDr, ReqSqlConstants.COL_DESCRIPTION),
                            UnitPrice = ReqDAOManager.GetNullableDecimalValue(sqlDr, ReqSqlConstants.COL_UNIT_PRICE),
                            Quantity = GetDecimalValue(sqlDr, ReqSqlConstants.COL_QUANTITY),
                            SourceType = (ItemSourceType)GetIntValue(sqlDr, ReqSqlConstants.COL_SOURCE_TYPE),
                            Tax = ReqDAOManager.GetNullableDecimalValue(sqlDr, ReqSqlConstants.COL_TAX),
                            AdditionalCharges = ReqDAOManager.GetNullableDecimalValue(sqlDr, ReqSqlConstants.COL_ADDITIONAL_CHARGES),
                            ShippingCharges = ReqDAOManager.GetNullableDecimalValue(sqlDr, ReqSqlConstants.COL_SHIPPING_CHARGES),
                            Banding = GetIntValue(sqlDr, ReqSqlConstants.COL_BANDING),
                            MaximumOrderQuantity = GetDecimalValue(sqlDr, ReqSqlConstants.COL_MAX_ORDER_QUANTITY),
                            MinimumOrderQuantity = GetDecimalValue(sqlDr, ReqSqlConstants.COL_MIN_ORDER_QUANTITY),
                            UOM = GetStringValue(sqlDr, ReqSqlConstants.COL_UOM),
                            Currency = GetStringValue(sqlDr, ReqSqlConstants.COL_CURRENCY),
                            TotalRecords = GetIntValue(sqlDr, ReqSqlConstants.COL_TOTAL_RECORDS),
                            //  ItemApprovalStatus = (DocumentStatus)GetIntValue(sqlDr, ReqSqlConstants.COL_APPROVAL_STATUS),
                            ItemCode = GetLongValue(sqlDr, ReqSqlConstants.COL_ITEM_CODE),
                            ShortName = GetStringValue(sqlDr, ReqSqlConstants.COL_SHORT_NAME),
                            RequisitionStatus = (DocumentStatus)GetIntValue(sqlDr, ReqSqlConstants.COL_DOCUMENT_STATUS),
                            AllowDecimalsForUom = GetBoolValue(sqlDr, ReqSqlConstants.COL_UOM_ALLOWDECIMAL),
                            AccountingStatus = GetBoolValue(sqlDr, ReqSqlConstants.COL_ACCOUNTINGSTATUS),
                            SplitType = (SplitType)GetTinyIntValue(sqlDr, ReqSqlConstants.COL_SPLIT_TYPE),
                            ItemImages = new List<FileUploadDetails>{
                                                new FileUploadDetails() { FileId = GetLongValue(sqlDr, ReqSqlConstants.COL_ItemImageFileId),
                                                 FileName = GetStringValue(sqlDr, ReqSqlConstants.COL_ItemImageFileName),
                                                 FileUri = GetStringValue(sqlDr, ReqSqlConstants.COL_ItemImageUri) }
                                 },
                            CategoryId = GetLongValue(sqlDr, ReqSqlConstants.COL_CATEGORY_ID),
                            ItemType = (ItemType)GetTinyIntValue(sqlDr, ReqSqlConstants.COL_ITEM_TYPE_ID),
                            ItemExtendedType = (ItemExtendedType)GetTinyIntValue(sqlDr, ReqSqlConstants.COL_ITEM_EXTENDED_TYPE),
                            Efforts = ReqDAOManager.GetNullableDecimalValue(sqlDr, ReqSqlConstants.COL_EFFORTS),
                            StartDate = GetDateTimeValue(sqlDr, ReqSqlConstants.COL_START_DATE),
                            EndDate = GetDateTimeValue(sqlDr, ReqSqlConstants.COL_END_DATE),
                            Billable = GetStringValue(sqlDr, ReqSqlConstants.COL_BILLABLE),
                            CatalogItemId = GetLongValue(sqlDr, ReqSqlConstants.COL_CATALOGITEMID),
                            ContractNo = GetStringValue(sqlDr, ReqSqlConstants.COL_CONTRACTNO),
                            PartnerCode = GetDecimalValue(sqlDr, ReqSqlConstants.COL_PARTNER_CODE),
                            PartnerName = GetStringValue(sqlDr, ReqSqlConstants.COL_PARTNER_NAME),
                            OrderLocationId = GetLongValue(sqlDr, ReqSqlConstants.COL_ORDERLOCATIONID),
                            OrderLocationName = GetStringValue(sqlDr, ReqSqlConstants.COL_ORDERLOCATIONNAME),
                            ProcurementStatus = GetTinyIntValue(sqlDr, ReqSqlConstants.COL_PROCUREMENTSTATUS),
                            Capitalized = GetStringValue(sqlDr, ReqSqlConstants.COL_CAPITALIZED),
                            CapitalCode = GetStringValue(sqlDr, ReqSqlConstants.COL_CAPITALCODE),
                            ExtContractRef = GetStringValue(sqlDr, ReqSqlConstants.COL_EXTCONTRACTREF),
                            AccountNumber = GetNullableLongValue(sqlDr, ReqSqlConstants.COL_ACCOUNTNUMBER),
                            IsContracted = GetBoolValue(sqlDr, ReqSqlConstants.COL_ISCONTRACTED),
                            IsBlanket = GetBoolValue(sqlDr, ReqSqlConstants.COL_ISBLANKET),
                            IsQuestionnaireError = GetBoolValue(sqlDr, ReqSqlConstants.COL_ISQUESTIONNAIREERROR),
                            TransmissionValue = GetStringValue(sqlDr, ReqSqlConstants.COL_TRANSMISSIONVALUE),
                            TrasmissionMode = GetIntValue(sqlDr, ReqSqlConstants.COL_TRASMISSIONMODE),
                            CreatedOn = GetDateTimeValue(sqlDr, ReqSqlConstants.COL_CREATEDON)
                        };
                        var objDocumentItemShippingDetails = new List<DocumentItemShippingDetail>();
                        objDocumentItemShippingDetails.Add(new DocumentItemShippingDetail
                        {
                            ShiptoLocation = new ShiptoLocation
                            {
                                ShiptoLocationId = GetIntValue(sqlDr, ReqSqlConstants.COL_SHIPTOLOC_ID)
                            }
                        });
                        objRequisitionItem.DocumentItemShippingDetails = objDocumentItemShippingDetails;
                        lstRequisitionItems.Add(objRequisitionItem);
                    }
                }
            }
            finally
            {
                if (!ReferenceEquals(objRefCountingDataReader, null) && !objRefCountingDataReader.IsClosed)
                {
                    objRefCountingDataReader.Close();
                    objRefCountingDataReader.Dispose();
                }
                LogHelper.LogInfo(Log, "Requisition GetAllLineItemsByDocumentId Method Ended for requisitionId=" + requisitionId);
            }
            return lstRequisitionItems;
        }

        public ICollection<DocumentTrackStatusDetail> GetTrackDetailsofDocumentById(long requisitionId)
        {
            RefCountingDataReader objRefCountingDataReader = null;
            var lstRequisitionTrackItems = new List<DocumentTrackStatusDetail>();
            try
            {
                LogHelper.LogInfo(Log, "Requisition GetTrackDetailsofDocumentById Method Started for requisitionId=" + requisitionId);

                if (Log.IsDebugEnabled)
                    Log.Debug(string.Format(CultureInfo.InvariantCulture, "Requisition GetTrackDetailsofDocumentById sp usp_P2P_REQ_GetTrackStatusDetailsByID with parameter: requisitionId={0} was called.", requisitionId));


                objRefCountingDataReader =
                    (RefCountingDataReader)
                 ContextSqlConn.ExecuteReader(ReqSqlConstants.USP_P2P_REQ_GETTRACKSTATUSDETAILBYID,
                                                                  new object[] { requisitionId });
                if (objRefCountingDataReader != null)
                {
                    var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;

                    while (sqlDr.Read())
                    {
                        var objRequisitionItem = new DocumentTrackStatusDetail
                        {
                            RequisitionId = GetLongValue(sqlDr, ReqSqlConstants.COL_REQUISITION_ID),
                            InstanceID = GetStringValue(sqlDr, ReqSqlConstants.COL_REQUISITION_INSTANCEID),
                            ApproverID = GetLongValue(sqlDr, ReqSqlConstants.COL_APPROVER_ID),
                            ApproverName = GetStringValue(sqlDr, ReqSqlConstants.COL_APPROVER_NAME),
                            ApproverType = GetStringValue(sqlDr, ReqSqlConstants.COL_APPROVER_TYPE),
                            StatusDate = GetDateTimeValue(sqlDr, ReqSqlConstants.COL_TRACK_DATE),
                            ApprovalTrackStatus = (RequisitionTrackStatus)GetIntValue(sqlDr, ReqSqlConstants.COL_TRACK_STATUS),
                            IsDeleted = GetBoolValue(sqlDr, ReqSqlConstants.COL_TRACK_ISDELETED)

                        };
                        lstRequisitionTrackItems.Add(objRequisitionItem);
                    }
                }
            }
            finally
            {
                if (!ReferenceEquals(objRefCountingDataReader, null) && !objRefCountingDataReader.IsClosed)
                {
                    objRefCountingDataReader.Close();
                    objRefCountingDataReader.Dispose();
                }
                LogHelper.LogInfo(Log, "Requisition GetTrackDetailsofDocumentById Method Ended for requisitionId=" + requisitionId);
            }
            return lstRequisitionTrackItems;
        }

        public bool DeleteLineItemByIds(string lineItemIds, bool isAdvanced, int precessionValue, int maxPrecessionValueforTotal, int maxPrecessionValueForTaxesAndCharges, bool IsSupplierCurrency = false)
        {
            long documentCode;
            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;
            try
            {
                LogHelper.LogInfo(Log, "Requisition DeleteLineItemByIds Method Started for lineItemIds=" + lineItemIds);

                //ReliableSqlDatabase sqlHelper = SqlHelper(UserContext.ClientName);
                _sqlCon = (SqlConnection)ContextSqlConn.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();

                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("Requisition DeleteLineItemByIds sp usp_P2P_REQ_DeleteLineItemById with parameter: ", " lineItemIds=" + lineItemIds + " was called."));

                documentCode = Convert.ToInt64(ContextSqlConn.ExecuteScalar(_sqlTrans, ReqSqlConstants.USP_P2P_REQ_DELETELINEITEMBYID, lineItemIds, (long)UserContext.UserId, isAdvanced, precessionValue, maxPrecessionValueforTotal, maxPrecessionValueForTaxesAndCharges), CultureInfo.InvariantCulture);

                if (documentCode > 0)
                {


                    SQLDocumentDAO sqlDocumentDAO = new SQLDocumentDAO();
                    sqlDocumentDAO.SqlTransaction = _sqlTrans;
                    sqlDocumentDAO.ReliableSqlDatabase = ContextSqlConn;
                    sqlDocumentDAO.UserContext = UserContext;
                    sqlDocumentDAO.GepConfiguration = GepConfiguration;


                    SaveRequisitionAdditionalDetails(documentCode, sqlDocumentDAO);
                    if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
                    {
                        _sqlTrans.Commit();

                        AddIntoSearchIndexerQueueing(documentCode, (int)DocumentType.Requisition, UserContext, GepConfiguration);
                    }
                }
                else
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
                }
            }
            catch
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

                throw;
            }
            finally
            {
                if (!ReferenceEquals(_sqlCon, null) && _sqlCon.State != ConnectionState.Closed)
                {
                    _sqlCon.Close();
                    _sqlCon.Dispose();
                }
                LogHelper.LogInfo(Log, "Requisition DeleteLineItemByIds Method Ended for lineItemIds=" + (String.IsNullOrEmpty(lineItemIds) ? "" : lineItemIds));
            }
            return documentCode > 0;
        }

        public long UpdateItemQuantity(long lineItemId, decimal quantity, int precessionValue, int maxPrecessionforTotal, int maxPrecessionForTaxesAndCharges)
        {
            long documentCode;
            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;
            try
            {
                LogHelper.LogInfo(Log, "Requisition UpdateItemQuantity Method Started for lineItemId=" + lineItemId);

                //ReliableSqlDatabase sqlHelper = SqlHelper(UserContext.ClientName);
                _sqlCon = (SqlConnection)ContextSqlConn.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();

                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("Requisition UpdateItemQuantity sp usp_P2P_REQ_UpdateItemQuantity with parameter: ", " lineItemId=" + lineItemId +
                                                         ",quantity = " + quantity +
                                                         " was called."));

                documentCode = Convert.ToInt64(ContextSqlConn.ExecuteScalar(_sqlTrans, ReqSqlConstants.USP_P2P_REQ_UPDATEITEMQUANTITY, lineItemId, quantity, UserContext.ContactCode, precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges), CultureInfo.InvariantCulture);

                if (documentCode > 0)
                {

                    SQLDocumentDAO sqlDocumentDAO = new SQLDocumentDAO();
                    sqlDocumentDAO.SqlTransaction = _sqlTrans;
                    sqlDocumentDAO.ReliableSqlDatabase = ContextSqlConn;
                    sqlDocumentDAO.UserContext = UserContext;
                    sqlDocumentDAO.GepConfiguration = GepConfiguration;


                    SaveRequisitionAdditionalDetails(documentCode, sqlDocumentDAO);

                    if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
                        _sqlTrans.Commit();
                }
                else
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
                }
            }
            catch
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

                throw;
            }
            finally
            {
                if (!ReferenceEquals(_sqlCon, null) && _sqlCon.State != ConnectionState.Closed)
                {
                    _sqlCon.Close();
                    _sqlCon.Dispose();
                }
                LogHelper.LogInfo(Log, "Requisition UpdateItemQuantity Method Ended for lineItemId=" + lineItemId);
            }
            return documentCode;
        }

        public P2PItem GetPartnerDetailsByLiId(long lineItemId)
        {
            RefCountingDataReader objRefCountingDataReader = null;
            var objRequisitionItem = new RequisitionItem();
            try
            {
                LogHelper.LogInfo(Log, "Requisition GetPartnerDetailsByLiId Method Started for lineItemId=" + lineItemId);

                if (lineItemId > 0)
                {
                    if (Log.IsDebugEnabled)
                        Log.Debug(string.Format(CultureInfo.InvariantCulture, "Requisition GetPartnerDetailsByLiId sp usp_P2P_REQ_GetPartnerDetailsByLiId with parameter: lineItemId={0} was called.", lineItemId));


                    objRefCountingDataReader = (RefCountingDataReader)
                    ContextSqlConn.ExecuteReader(ReqSqlConstants.USP_P2P_REQ_GETPARTNERDETAILSBYLIID, lineItemId);
                    if (objRefCountingDataReader != null)
                    {
                        var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;

                        while (sqlDr.Read())
                        {
                            objRequisitionItem.PartnerName = GetStringValue(sqlDr, ReqSqlConstants.COL_PARTNER_NAME);
                            objRequisitionItem.ClientPartnerCode = GetStringValue(sqlDr, ReqSqlConstants.COL_CLIENT_PARTNERCODE);
                            objRequisitionItem.PartnerCode = GetDecimalValue(sqlDr, ReqSqlConstants.COL_PARTNER_CODE);
                            objRequisitionItem.ManufacturerName = GetStringValue(sqlDr, ReqSqlConstants.COL_MANUFACTURER_NAME);
                            objRequisitionItem.ManufacturerPartNumber = GetStringValue(sqlDr, ReqSqlConstants.COL_MANUFACTURER_PART_NUMBER);
                            objRequisitionItem.isDefault = GetBoolValue(sqlDr, ReqSqlConstants.COL_ISDEFAULT);
                            objRequisitionItem.OrderLocationName = GetStringValue(sqlDr, ReqSqlConstants.COL_ORDERLOCATIONNAME);
                            objRequisitionItem.OrderLocationId = GetLongValue(sqlDr, ReqSqlConstants.COL_ORDERLOCATIONID);
                            objRequisitionItem.PartnerContactName = GetStringValue(sqlDr, ReqSqlConstants.COL_PARTNERCONTACTNAME);
                            objRequisitionItem.PartnerContactId = GetLongValue(sqlDr, ReqSqlConstants.COL_PARTNERCONTACTID);
                            objRequisitionItem.EmailId = GetStringValue(sqlDr, ReqSqlConstants.COL_EMAIL_ID);
                            objRequisitionItem.PhoneNo = GetStringValue(sqlDr, ReqSqlConstants.COL_PHONE_NO);
                            objRequisitionItem.TrasmissionMode = GetIntValue(sqlDr, ReqSqlConstants.COL_TRASMISSIONMODE);
                            objRequisitionItem.TransmissionValue = GetStringValue(sqlDr, ReqSqlConstants.COL_TRANSMISSIONVALUE);
                        }
                    }
                }
                else
                {
                    if (Log.IsWarnEnabled)
                        Log.Warn("In Requisition GetPartnerDetailsByLiId method lineItemId parameter is less than or equal to 0.");
                }
            }
            finally
            {
                if (!ReferenceEquals(objRefCountingDataReader, null) && !objRefCountingDataReader.IsClosed)
                {
                    objRefCountingDataReader.Close();
                    objRefCountingDataReader.Dispose();
                }

                LogHelper.LogInfo(Log, "Requisition GetPartnerDetailsByLiId Method Ended for lineItemId=" + lineItemId);
            }
            return objRequisitionItem;
        }

        public ICollection<DocumentItemShippingDetail> GetShippingSplitDetailsByLiId(long lineItemId)
        {
            RefCountingDataReader objRefCountingDataReader = null;
            var lstDocumentItemShippingDetail = new List<DocumentItemShippingDetail>();

            try
            {
                LogHelper.LogInfo(Log, "Requisition GetShippingSplitDetailsByLiId Method Started for lineItemId=" + lineItemId);
                if (lineItemId > 0)
                {
                    if (Log.IsDebugEnabled)
                        Log.Debug(string.Format(CultureInfo.InvariantCulture, "Requisition GetShippingSplitDetailsByLiId sp usp_P2P_REQ_GetShippingSplitDetailsByLiId with parameter: lineItemId={0} was called.", lineItemId));


                    objRefCountingDataReader = (RefCountingDataReader)
                    ContextSqlConn.ExecuteReader(ReqSqlConstants.USP_P2P_REQ_GETSHIPPINGSPLITDETAILSBYLIID, lineItemId);
                    if (objRefCountingDataReader != null)
                    {
                        var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;
                        var objDocumentItemShippingDetail = new DocumentItemShippingDetail();
                        while (sqlDr.Read())
                        {

                            //{
                            objDocumentItemShippingDetail.DocumentItemShippingId = GetLongValue(sqlDr, ReqSqlConstants.COL_REQITEM_SHIPPING_ID);
                            objDocumentItemShippingDetail.Quantity = GetDecimalValue(sqlDr, ReqSqlConstants.COL_QUANTITY);
                            objDocumentItemShippingDetail.ShippingMethod = GetStringValue(sqlDr, ReqSqlConstants.COL_SHIPPINGMETHOD);
                            objDocumentItemShippingDetail.ShiptoLocation = new ShiptoLocation
                            {
                                ShiptoLocationId = GetIntValue(sqlDr, ReqSqlConstants.COL_SHIPTOLOC_ID),
                                ShiptoLocationName = GetStringValue(sqlDr, ReqSqlConstants.COL_SHIPTOLOC_NAME),
                                ShiptoLocationNumber = GetStringValue(sqlDr, ReqSqlConstants.COL_SHIPTOLOC_NUMBER),
                                Address = new P2PAddress
                                {

                                    AddressLine1 = GetStringValue(sqlDr, ReqSqlConstants.COL_ADDRESS1),
                                    AddressLine2 = GetStringValue(sqlDr, ReqSqlConstants.COL_ADDRESS2),
                                    AddressLine3 = GetStringValue(sqlDr, ReqSqlConstants.COL_ADDRESS3),
                                    City = GetStringValue(sqlDr, ReqSqlConstants.COL_CITY),
                                    State = GetStringValue(sqlDr, ReqSqlConstants.COL_STATE),
                                    StateCode = GetStringValue(sqlDr, ReqSqlConstants.COL_STATECODE),
                                    Zip = GetStringValue(sqlDr, ReqSqlConstants.COL_ZIP),
                                    CountryCode = GetStringValue(sqlDr, ReqSqlConstants.COL_COUNTRYCODE),
                                    CountryName = GetStringValue(sqlDr, ReqSqlConstants.COL_SHIPTO_COUNTRYNAME)
                                }
                            };
                            objDocumentItemShippingDetail.DelivertoLocation = new DelivertoLocation
                            {
                                DelivertoLocationId = GetIntValue(sqlDr, ReqSqlConstants.COL_DELIVERTOLOC_ID),
                                DelivertoLocationName = GetStringValue(sqlDr, ReqSqlConstants.COL_DELIVERTOLOC_NAME),
                                DeliverTo = GetStringValue(sqlDr, ReqSqlConstants.COL_DELIVERTO),
                                Address = new P2PAddress
                                {
                                    AddressLine1 = GetStringValue(sqlDr, ReqSqlConstants.DEL_COL_ADDRESS1),
                                    AddressLine2 = GetStringValue(sqlDr, ReqSqlConstants.DEL_COL_ADDRESS2),
                                    AddressLine3 = GetStringValue(sqlDr, ReqSqlConstants.DEL_COL_ADDRESS3),
                                    City = GetStringValue(sqlDr, ReqSqlConstants.DEL_COL_CITY),
                                    State = GetStringValue(sqlDr, ReqSqlConstants.DEL_COL_STATE),
                                    Zip = GetStringValue(sqlDr, ReqSqlConstants.DEL_COL_ZIP)
                                }
                            };
                            //}; 

                            if (sqlDr.NextResult() && sqlDr.HasRows)
                            {
                                objDocumentItemShippingDetail.ShiptoLocation.lstShipLocation = new List<RegistrationDetails>();
                                while (sqlDr.Read())
                                {
                                    RegistrationDetails obj = new RegistrationDetails();
                                    obj.CompanyIdentification = GetStringValue(sqlDr, ReqSqlConstants.COL_COMPANYIDENTIFICATION);
                                    obj.CompanyIdentificationDisplayName = GetStringValue(sqlDr, ReqSqlConstants.COL_COMPANYIDENTIFICATIONDISPLAYNAME);
                                    objDocumentItemShippingDetail.ShiptoLocation.lstShipLocation.Add(obj);
                                }
                            }

                            lstDocumentItemShippingDetail.Add(objDocumentItemShippingDetail);
                        }
                    }
                }
                else
                {
                    if (Log.IsWarnEnabled)
                        Log.Warn("In Requisition GetShippingSplitDetailsByLiId method lineItemId parameter is less than or equal to 0.");
                }
            }
            finally
            {
                if (!ReferenceEquals(objRefCountingDataReader, null) && !objRefCountingDataReader.IsClosed)
                {
                    objRefCountingDataReader.Close();
                    objRefCountingDataReader.Dispose();
                }
                LogHelper.LogInfo(Log, "Requisition GetShippingSplitDetailsByLiId Method Ended for lineItemId=" + lineItemId);
            }
            return lstDocumentItemShippingDetail;
        }

        public P2PItem GetOtherItemDetailsByLiId(long lineItemId)
        {
            RefCountingDataReader objRefCountingDataReader = null;
            var objRequisitionItem = new RequisitionItem();
            try
            {
                LogHelper.LogInfo(Log, "Requisition GetOtherItemDetailsByLiId Method Started for lineItemId=" + lineItemId);

                if (lineItemId > 0)
                {
                    if (Log.IsDebugEnabled)
                        Log.Debug(string.Concat("Requisition GetOtherItemDetailsByLiId sp usp_P2P_REQ_GetShippingSplitDetailsByLiId with parameter: ", " lineItemId=" + lineItemId +
                                                             " was called."));

                    objRefCountingDataReader = (RefCountingDataReader)
                    ContextSqlConn.ExecuteReader(ReqSqlConstants.USP_P2P_REQ_GETOTHERITEMDETAILSBYLIID, lineItemId);
                    if (objRefCountingDataReader != null)
                    {
                        var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;

                        while (sqlDr.Read())
                        {
                            objRequisitionItem.CategoryName = GetStringValue(sqlDr, ReqSqlConstants.COL_CATEGORY_NAME);
                            objRequisitionItem.CategoryId = GetLongValue(sqlDr, ReqSqlConstants.COL_CATEGORY_ID);
                            objRequisitionItem.ProcurementStatus = GetIntValue(sqlDr, ReqSqlConstants.COL_PROCUREMENT_STATUS);
                            objRequisitionItem.IsProcurable = GetIntValue(sqlDr, ReqSqlConstants.COL_IS_PROCURABLE);
                            objRequisitionItem.Billable = GetStringValue(sqlDr, ReqSqlConstants.COL_BILLABLE);
                            objRequisitionItem.InventoryType = GetNullableBoolValue(sqlDr, ReqSqlConstants.COL_INVENTORYTYPE);
                            objRequisitionItem.Capitalized = GetStringValue(sqlDr, ReqSqlConstants.COL_CAPITALIZED);
                            objRequisitionItem.CapitalCode = GetStringValue(sqlDr, ReqSqlConstants.COL_CAPITALCODE);
                            objRequisitionItem.DateNeeded = GetDateTimeValue(sqlDr, ReqSqlConstants.COL_DATE_NEEDED);
                        }
                    }
                }
                else
                {
                    if (Log.IsWarnEnabled)
                        Log.Warn("In Requisition GetOtherItemDetailsByLiId method lineItemId parameter is less than or equal to 0.");
                }
            }
            finally
            {
                if (!ReferenceEquals(objRefCountingDataReader, null) && !objRefCountingDataReader.IsClosed)
                {
                    objRefCountingDataReader.Close();
                    objRefCountingDataReader.Dispose();
                }
                LogHelper.LogInfo(Log, "Requisition GetOtherItemDetailsByLiId Method Ended for lineItemId=" + lineItemId);
            }
            return objRequisitionItem;
        }

        public ICollection<P2PDocument> GetAllPreDocumentForLeftPanel(decimal partnerCode, long documentId, long userId, int pageIndex, int pageSize, string currencyCode, string searchText, long orgEntityDetailCode, long orderingLocationId = 0, int purchaseTypeId = 1, bool IsRequisitionTypeEnabled = false, bool IsCentralizedReceiver = false, bool IsDesktopReceiver = false, bool EnableMaterialReceiving = false, bool IsServiceReceivingByRequester = false, bool AllowAccessControlOnReceipts = false, bool CheckIsDirect = false, List<UserLOBMapping> AllServingUserLOBMapping = null)
        {
            throw new NotImplementedException();
        }

        public ICollection<P2PDocument> GetAllDocumentForLeftPanel(decimal partnerCode, long documentId, long userId, int pageIndex, int pageSize, string currencyCode, long orgEntityDetailCode, bool isHeaderEntityBU, int purchaseTypeId)
        {
            RefCountingDataReader objRefCountingDataReader = null;
            try
            {
                LogHelper.LogInfo(Log, "Requisition GetAllDocumentForLeftPanel Method Started for partnerCode=" + partnerCode);

                if (Log.IsDebugEnabled)
                    Log.Debug(string.Format(CultureInfo.InvariantCulture, "Requisition GetAllDocumentForLeftPanel SP: usp_P2P_REQ_GetAllRequisitionForLeftPanel with parameter: partnerCode={0},pageIndex = {1},pageSize = {2} was called.", partnerCode, pageIndex, pageSize, currencyCode));
                objRefCountingDataReader =
                 (RefCountingDataReader)
                 ContextSqlConn.ExecuteReader(ReqSqlConstants.USP_P2P_REQ_GETALLREQUISITIONFORLEFTPANEL,
                                                                  new object[] { documentId, userId, pageIndex, pageSize, currencyCode, orgEntityDetailCode, isHeaderEntityBU, purchaseTypeId });
                if (objRefCountingDataReader != null)
                {
                    var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;
                    var lstRequisitions = new List<P2PDocument>();
                    while (sqlDr.Read())
                    {
                        var objRequisition = new Requisition
                        {
                            DocumentId = GetLongValue(sqlDr, ReqSqlConstants.COL_REQUISITION_ID),
                            DocumentName = GetStringValue(sqlDr, ReqSqlConstants.COL_DOCUMENT_NAME),
                            IsUrgent = GetBoolValue(sqlDr, ReqSqlConstants.COL_ISURGENT),
                            IsPunchoutItemExists = GetBoolValue(sqlDr, ReqSqlConstants.COL_IS_PUNCHOUT_ITEMS_EXISTS)
                        };
                        lstRequisitions.Add(objRequisition);
                    }
                    return lstRequisitions;
                }
            }

            finally
            {
                if (!ReferenceEquals(objRefCountingDataReader, null) && !objRefCountingDataReader.IsClosed)
                {
                    objRefCountingDataReader.Close();
                    objRefCountingDataReader.Dispose();
                }
                LogHelper.LogInfo(Log, "Requisition GetAllDocumentForLeftPanel Method Ended for partnerCode=" + partnerCode);
            }

            return new List<P2PDocument>();
        }

        public int GetRequisitionItemsCountByPartnerCode(long requisitionId, decimal partnerCode)
        {
            var itemsCount = 0;

            try
            {
                LogHelper.LogInfo(Log, "Requisition GetRequisitionItemsCountByPartnerCode Method Started for requisitionId=" + requisitionId);

                if (requisitionId > 0)
                {
                    if (Log.IsDebugEnabled)
                        Log.Debug(string.Format(CultureInfo.InvariantCulture, "Requisition GetRequisitionItemsCountByPartnerCode sp usp_P2P_GetRequisitionItemsCountByPartnerCode with parameter: requisitionId={0} and partnerCode = {1}  was called.", requisitionId, partnerCode));


                    itemsCount = (int)ContextSqlConn.ExecuteScalar(ReqSqlConstants.USP_P2P_GETREQUISITIONITEMSCOUNTBYPARTNERCODE, requisitionId, partnerCode);

                }
                else
                {
                    if (Log.IsWarnEnabled)
                        Log.Warn("In Requisition GetRequisitionItemsCountByPartnerCode method requisitionId parameter is less than or equal to 0.");
                }
            }
            finally
            {
                LogHelper.LogInfo(Log, "Requisition GetRequisitionItemsCountByPartnerCode Method Ended for requisitionId=" + requisitionId);
            }
            return itemsCount;
        }

        public ICollection<P2PItem> GetAllLineItemsByDocumentIdForLeftPanel(long documentId, ItemType itemType, int pageIndex, int pageSize, AccountingSplitMode accountingSplitMode = AccountingSplitMode.SplitByItemTotal, int typeOfUser = 0)
        {
            throw new NotImplementedException();
        }

        public bool UpdateDocumentStatus(long requisitionId, DocumentStatus documentStatus, decimal partnerCode, int maxPrecessionValue = 0, int maxPrecessionValueTotal = 0, int maxPrecessionValueForTaxAndCharges = 0, bool IsSupplierCurrency = false, bool isBuyerInvoiceVisibleToBuyer = false)
        {
            bool result = false;

            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;
            try
            {

                LogHelper.LogInfo(Log, "Requisition UpdateDocumentStatus Method Started for requisitionId=" + requisitionId);

                var sqlHelper = ContextSqlConn;
                _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();

                SQLDocumentDAO sqlDocumentDAO = new SQLDocumentDAO();
                sqlDocumentDAO.SqlTransaction = _sqlTrans;
                sqlDocumentDAO.ReliableSqlDatabase = sqlHelper;
                sqlDocumentDAO.UserContext = UserContext;
                sqlDocumentDAO.GepConfiguration = GepConfiguration;

                result = sqlDocumentDAO.UpdateDocumentStatus(requisitionId, documentStatus);

                SaveRequisitionAdditionalDetails(requisitionId, sqlDocumentDAO);

                if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
                    _sqlTrans.Commit();
            }
            catch(Exception ex)
            {
                LogHelper.LogError(Log, "Error occurred in UpdateDocumentStatus method in SQLRequisitionDAO documentcode :- " + requisitionId, ex);
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
                LogHelper.LogInfo(Log, "Requisition UpdateDocumentStatus Method Ended for requisitionId=" + requisitionId);
            }
            return result;
        }

        public long SaveCatalogRequisition(long userId, Document document, int defaultShipToLocationId, string shippingMethod, List<KeyValuePair<string, string>> lstSettingValue, long oboId = 0)
        {
            //Initializing the Logger.
            LogHelper.LogInfo(Log, "SaveCatalogRequisition Method Started.");
            SqlConnection objSqlCon = null;
            SqlTransaction objSqlTrans = null;
            var objDocument = new Document();
            long result = 0;
            try
            {
                var sqlHelper = ContextSqlConn;
                objSqlCon = (SqlConnection)sqlHelper.CreateConnection();
                objSqlCon.Open();
                objSqlTrans = objSqlCon.BeginTransaction();

                objDocument.DocumentTypeInfo = DocumentType.Requisition;
                objDocument.DocumentCode = document.DocumentCode;
                objDocument.DocumentName = document.DocumentName;
                objDocument.DocumentNumber = document.DocumentNumber;


                SQLDocumentDAO sqlDocumentDAO = new SQLDocumentDAO();
                sqlDocumentDAO.SqlTransaction = objSqlTrans;
                sqlDocumentDAO.ReliableSqlDatabase = sqlHelper;
                sqlDocumentDAO.UserContext = UserContext;
                sqlDocumentDAO.GepConfiguration = GepConfiguration;
                //1. Save Document


                if (document.DocumentCode == 0)
                {
                    objDocument.DocumentStatusInfo = DocumentStatus.Draft;
                    objDocument.CreatedBy = userId;
                    objDocument.CreatedOn = DateTime.Now;
                    objDocument.CompanyName = UserContext.CompanyName;
                    objDocument.IsDocumentDetails = true;
                    objDocument.EntityId = document.EntityId;
                    objDocument.EntityDetailCode = document.EntityDetailCode;
                    document.DocumentBUList.ToList().ForEach(data => { if (data.BusinessUnitCode > 0) objDocument.DocumentBUList.Add(data); });

                    if (Log.IsDebugEnabled)
                        Log.Debug(string.Concat("Requisition Save SaveDocumentDetails with parameter: requisitionNumber=" + document.DocumentNumber, " was called."));

                    document.DocumentCode = sqlDocumentDAO.SaveDocumentDetails(objDocument);
                }


                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("In SaveCatalogRequisition Method of class",
                        " SP: usp_P2P_SaveCatalogRequisition,  with parameters: documentCode = ", document.DocumentCode + ", requisitionName = " + document.DocumentName + ", requisitionNumber = " + document.DocumentNumber));
                DataTable dtRequisitionSettingValues = ConvertSettingsToTableTypes(lstSettingValue);

                using (var objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_SAVECATALOGREQUISITION, objSqlCon))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;

                    objSqlCommand.Parameters.AddWithValue("@userId", UserContext.ContactCode);
                    objSqlCommand.Parameters.AddWithValue("@documentCode", document.DocumentCode);
                    objSqlCommand.Parameters.AddWithValue("@requisitionName", HtmlEncode(document.DocumentName));
                    objSqlCommand.Parameters.AddWithValue("@requisitionNumber", document.DocumentNumber);
                    objSqlCommand.Parameters.AddWithValue("@defaultShipToLocationId", defaultShipToLocationId > 0 ? defaultShipToLocationId : 0);
                    objSqlCommand.Parameters.AddWithValue("@oboId", oboId);
                    objSqlCommand.Parameters.AddWithValue("@shippingMethod", shippingMethod);
                    objSqlCommand.Parameters.Add(new SqlParameter("@tvp_P2P_SettingValues", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_P2P_SETTINGVALUES,
                        Value = dtRequisitionSettingValues
                    });
                    result = Convert.ToInt64(sqlHelper.ExecuteScalar(objSqlCommand, objSqlTrans), NumberFormatInfo.InvariantInfo);

                }

                if (result > 0)
                {
                    if (Log.IsDebugEnabled)
                        Log.Debug(string.Concat("Requisition Save SaveDocumentDetails with parameter: requisitionNumber=" + document.DocumentNumber, " was called."));
                    //Get Requisition Additional info data  and then fill with Additional info data and save document with additional data

                    SaveRequisitionAdditionalDetails(document.DocumentCode, sqlDocumentDAO);
                    if (!ReferenceEquals(objSqlTrans, null) && !ReferenceEquals(objSqlCon, null) && objSqlCon.State == ConnectionState.Open)
                        objSqlTrans.Commit();
                }
                else
                {

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
                    }

                }


            }
            catch
            {

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
                }

                throw;
            }
            finally
            {
                if (!ReferenceEquals(objSqlCon, null) && objSqlCon.State != ConnectionState.Closed)
                {
                    objSqlCon.Close();
                    objSqlCon.Dispose();
                }

                //Logger Ended.
                LogHelper.LogInfo(Log, "SaveCatalogRequisition Method Ended");
            }
            return result;
        }

        //public void SaveRequisitionAdditionalDetailsFromInterface(long documentCode)
        //{
        //    SqlConnection objSqlCon = null;
        //    SqlTransaction objSqlTrans = null;

        //    try
        //    {
        //        var sqlHelper = ContextSqlConn;
        //        objSqlCon = (SqlConnection)sqlHelper.CreateConnection();
        //        objSqlCon.Open();
        //        objSqlTrans = objSqlCon.BeginTransaction();

        //        SQLDocumentDAO sqlDocumentDAO = new SQLDocumentDAO();
        //        sqlDocumentDAO.SqlTransaction = objSqlTrans;
        //        sqlDocumentDAO.ReliableSqlDatabase = sqlHelper;
        //        sqlDocumentDAO.UserContext = UserContext;
        //        sqlDocumentDAO.GepConfiguration = GepConfiguration;

        //        SaveRequisitionAdditionalDetails(documentCode, sqlDocumentDAO);

        //        if (!ReferenceEquals(objSqlTrans, null) && !ReferenceEquals(objSqlCon, null) && objSqlCon.State == ConnectionState.Open)
        //            objSqlTrans.Commit();
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

        //            throw;
        //        }
        //    }
        //    finally
        //    {
        //        if (!ReferenceEquals(objSqlCon, null) && objSqlCon.State != ConnectionState.Closed)
        //        {
        //            objSqlCon.Close();
        //            objSqlCon.Dispose();
        //        }
        //    }
        //}

        public void SaveRequisitionAdditionalDetails(long documentCode, SQLDocumentDAO sqlDocumentDAO)
        {
            var objDocument = new Document()
            {
                DocumentCode = documentCode,
                IsAddtionalDetails = true,
                IsDocumentDetails = false,
                IsStakeholderDetails = false,
                IsTemplate = false,
                DocumentTypeInfo = DocumentType.Requisition
            };

            GetRequisitionAdditionalDetails(objDocument, sqlDocumentDAO.SqlTransaction);

            sqlDocumentDAO.SaveDocumentDetails(objDocument);

        }


        private void GetRequisitionAdditionalDetails(Document objDocument, SqlTransaction objSqlTransaction)
        {
            SqlDataReader sqlDr = null;
            try
            {
                sqlDr = (SqlDataReader)ContextSqlConn.ExecuteReader(objSqlTransaction,
                                                      ReqSqlConstants.USP_P2P_GETREQUISITIONADDITIONALDETAILS,
                                                      objDocument.DocumentCode);
                if (sqlDr != null)
                {


                    while (sqlDr.Read())
                    {
                        objDocument.DocumentAdditionalFieldList.Add(CreateAdditionalInfo(objDocument.DocumentCode,
                                                                                         GetStringValue(sqlDr,
                                                                                                        "AdditionalDataXSD"),
                                                                                         GetStringValue(sqlDr,
                                                                                                        "AdditionalDataValue"),
                                                                                         (FieldType)
                                                                                         GetIntValue(sqlDr, "XSDType")));
                    }

                }
            }
            catch (Exception ex)
            {
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw();

                throw;
            }
            finally
            {
                if (!ReferenceEquals(sqlDr, null) && !sqlDr.IsClosed)
                {
                    sqlDr.Close();
                    sqlDr.Dispose();
                }
            }

        }


        public bool AddPreDocumentToDocument(long documentId, string preDocumentIds, decimal partnerCode, long userId, int precessionValue, Dictionary<string, string> dcSettings = null, long orderingLocationId = 0)
        {
            throw new NotImplementedException();
        }



        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "GEP.Cumulus.Logging.LogHelper.LogInfo(log4net.ILog,System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object)")]
        //public long GetDocumentCodeByDocumentId(long requisitionId)
        //{
        //    long documentCode = 0;

        //    try
        //    {
        //        LogHelper.LogInfo(Log, string.Format("Requisition GetDocumentCodeByDocumentId Method Started for requisitionId={0}", requisitionId));

        //        if (requisitionId > 0)
        //        {
        //            if (Log.IsDebugEnabled)
        //                Log.Debug(string.Format("Requisition GetDocumentCodeByDocumentId sp usp_P2P_REQ_GetDocumentCodeByDocumentId with parameter: requisitionId={0}  was called.", requisitionId));


        //            documentCode = (long)ContextSqlConn.ExecuteScalar(ReqSqlConstants.USP_P2P_REQ_GETDOCUMENTCODE_BY_DOCUMENTID, requisitionId);

        //        }
        //        else
        //        {
        //            if (Log.IsWarnEnabled)
        //                Log.Warn("In Requisition GetDocumentCodeByDocumentId method requisitionId parameter is less than or equal to 0.");
        //        }
        //    }
        //    finally
        //    {
        //        LogHelper.LogInfo(Log, "Requisition GetDocumentCodeByDocumentId Method Ended for requisitionId=" + requisitionId);
        //    }
        //    return documentCode;
        //}

        public bool validateDocumentBeforeNextAction(long documentId)
        {
            bool result = false;
            SqlConnection _sqlCon = null;
            try
            {
                LogHelper.LogInfo(Log, "validate Document BeforeNextAction Method Started for documentId = " + documentId);
                //var sqlHelper = SqlHelper(UserContext.ClientName);
                _sqlCon = (SqlConnection)ContextSqlConn.CreateConnection();
                _sqlCon.Open();

                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("In validate Document BeforeNextAction Method",
                                             "SP: usp_P2P_Req_ValidateDocumentBeforeNextAction with parameter: documentId=" + documentId, " was called."));
                result = Convert.ToBoolean(ContextSqlConn.ExecuteScalar(ReqSqlConstants.USP_P2P_REQ_VALIDATEDOCUMENTBEFORENEXTACTION,
                                                                      documentId
                                                                     ), NumberFormatInfo.InvariantInfo);
                return result;
            }
            catch (Exception sqlEx)
            {
                LogHelper.LogError(Log, "Error occured in Req validateDocumentBeforeNextAction Method for documentId=" + documentId, sqlEx);
                throw;
            }
            finally
            {
                if (!ReferenceEquals(_sqlCon, null) && _sqlCon.State != ConnectionState.Closed)
                {
                    _sqlCon.Close();
                    _sqlCon.Dispose();
                }
                LogHelper.LogInfo(Log, "Req validateDocumentBeforeNextAction Method Ended for orderId = " + documentId);
            }
        }

        public ICollection<string> ValidateDocumentByDocumentCode(long documentCode, bool allowTaxCodewithAmount, string supplierStatusForValidation, bool returnResourceErrorMsgKey = false, int populateDefaultNeedByDateByDays = 0, int ReceivedDateLimit = -1, bool EnablePastDateDocProcess = false, bool isDayLightSavingON = false)
        {
            RefCountingDataReader objRefCountingDataReader = null;
            List<string> lstValidationResult = new List<string>();
            try
            {
                LogHelper.LogInfo(Log, "Requisition ValidateDocumentByDocumentCode Method Started for documentId=" + documentCode);
                if (documentCode > 0)
                {
                    if (Log.IsDebugEnabled)
                        Log.Debug(string.Concat("Requisition ValidateDocumentByDocumentCode, sp usp_P2P_ValidateByDocumentCode with parameter: documentCode=", documentCode, " was called."));


                    objRefCountingDataReader = (RefCountingDataReader)
                    ContextSqlConn.ExecuteReader(ReqSqlConstants.USP_P2P_VALIDATE_BY_DOCUMENT_CODE, documentCode, populateDefaultNeedByDateByDays, EnablePastDateDocProcess);
                    if (objRefCountingDataReader != null)
                    {
                        var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;

                        while (sqlDr.Read())
                        {
                            lstValidationResult.Add(GetStringValue(sqlDr, ReqSqlConstants.COL_ERROR_MESSAGE));
                        }
                    }
                }
                else
                {
                    lstValidationResult.Add("Invalid Document Code");
                    if (Log.IsWarnEnabled)
                        Log.Warn("In Requisition ValidateDocumentByDocumentCode method documentCode parameter is less than or equal to 0.");
                }
            }
            catch (Exception ex)
            {
                lstValidationResult.Add(ex.Message);
                throw;
            }
            finally
            {
                if (!ReferenceEquals(objRefCountingDataReader, null) && !objRefCountingDataReader.IsClosed)
                {
                    objRefCountingDataReader.Close();
                    objRefCountingDataReader.Dispose();
                }
                LogHelper.LogInfo(Log, "Requisition ValidateDocumentByDocumentCode Method Ended for documentCode=" + documentCode);
            }
            return lstValidationResult;
        }
        public int GetDocumentExtendedStatus(long documentCode)
        {
            int Exendedstatus = 0;
            RefCountingDataReader objRefCountingDataReader = null;
            List<string> lstValidationResult = new List<string>();
            try
            {
                if (documentCode > 0)
                {
                    objRefCountingDataReader = (RefCountingDataReader)
                    ContextSqlConn.ExecuteReader(ReqSqlConstants.USP_P2P_REQ_GETDOCUMENTEXTENDEDSTATUS,documentCode);
                    if (objRefCountingDataReader != null)
                    {
                        var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;

                        while (sqlDr.Read())
                        {
                            Exendedstatus=GetTinyIntValue(sqlDr, ReqSqlConstants.COL_EXTENDEDSTATUS);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error Occured in GetDocumentExtendedStatus method="+ documentCode, ex);               
                throw ex;
            }
            finally
            {
                if (!ReferenceEquals(objRefCountingDataReader, null) && !objRefCountingDataReader.IsClosed)
                {
                    objRefCountingDataReader.Close();
                    objRefCountingDataReader.Dispose();
                }

            }
            return Exendedstatus;
        }

        public long UpdateProcurementStatusByReqItemId(long requistionItemId)
        {
            bool result = false;
            long DocumentCode = 0;
            var ProcFlag = 0;
            int DocStatus = 0;
            // long statusUpdateFalg = 0;
            RefCountingDataReader objRefCountingDataReader = null;
            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;
            try
            {

                LogHelper.LogInfo(Log, "Requisition UpdateProcurementStatusByReqItemId Method Started for requistionItemId=" + requistionItemId);

                var sqlHelper = ContextSqlConn;
                _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();
                // _sqlTrans = _sqlCon.BeginTransaction();

                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("Requisition UpdateProcurementStatusByReqItemId sp usp_P2P_REQ_UpdateProcurementStatusByReqItemId with parameter: ", " requistionItemId=" + requistionItemId +
                                                         " was called."));
                if (requistionItemId > 0)
                {

                    objRefCountingDataReader =
                (RefCountingDataReader)
                sqlHelper.ExecuteReader(ReqSqlConstants.USP_P2P_UPDATEPROCUREMENTSTATUSBYREQITEMID,
                                                                         requistionItemId,
                                                                         (long)UserContext.UserId
                                                                          );



                    if (objRefCountingDataReader != null)
                    {
                        var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;
                        while (sqlDr.Read())
                        {
                            DocumentCode = (long)(sqlDr["DocumentCode"]);
                            ProcFlag = Convert.ToInt32(sqlDr["ProcFlag"], NumberFormatInfo.InvariantInfo);
                            DocStatus = (int)(sqlDr["DocStatus"]);
                        }
                    }




                    SQLDocumentDAO sqlDocumentDAO = new SQLDocumentDAO();
                    sqlDocumentDAO.SqlTransaction = _sqlTrans;
                    sqlDocumentDAO.ReliableSqlDatabase = sqlHelper;
                    sqlDocumentDAO.UserContext = UserContext;
                    sqlDocumentDAO.GepConfiguration = GepConfiguration;
                    //1. Save Document


                    if (DocStatus == 22)
                    {
                        if (ProcFlag == 1)
                        {
                            result = sqlDocumentDAO.UpdateDocumentStatus(DocumentCode, DocumentStatus.NonProcurable);
                            if (result == true)
                            {
                                DocStatus = 69;
                            }

                        }
                    }
                    else if (DocStatus == 69)
                    {
                        if (ProcFlag == 0)
                        {
                            result = sqlDocumentDAO.UpdateDocumentStatus(DocumentCode, DocumentStatus.Approved);
                            if (result == true)
                            {
                                DocStatus = 22;
                            }

                        }
                    }

                    SaveRequisitionAdditionalDetails(DocumentCode, sqlDocumentDAO);
                }
                if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
                    _sqlTrans.Commit();
            }
            catch
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


                throw;
            }
            finally
            {
                if (!ReferenceEquals(objRefCountingDataReader, null) && !objRefCountingDataReader.IsClosed)
                {
                    objRefCountingDataReader.Close();
                    objRefCountingDataReader.Dispose();
                }

                if (!ReferenceEquals(_sqlCon, null) && _sqlCon.State != ConnectionState.Closed)
                {
                    _sqlCon.Close();
                    _sqlCon.Dispose();
                }
                LogHelper.LogInfo(Log, "Requisition UpdateProcurementStatusByReqItemId Method Ended for requistionItemId=" + requistionItemId);
            }
            return DocStatus;


        }

        public Dictionary<string, string> GetRequisitionDetailsForExternalWorkFlowProcess(long requisitionId)
        {
            RefCountingDataReader objRefCountingDataReader = null;
            List<NewP2PEntities.DocumentInfo> lstOrders = new List<NewP2PEntities.DocumentInfo>();
            var result = new Dictionary<string, string>();
            try
            {
                LogHelper.LogInfo(Log, "GetRequisitionDetailsForExternalWorkFlowProcess in SQL Requisition DAO started for  requisitionId = " + requisitionId);
                var sqlHelper = ContextSqlConn;
                objRefCountingDataReader = (RefCountingDataReader)sqlHelper.ExecuteReader(ReqSqlConstants.USP_P2P_REQ_GETREQUISITIONDETAILSFOREXTERNALWORKFLOWPROCESS, new object[] {
                    requisitionId});
                if (objRefCountingDataReader != null)
                {
                    var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;
                    while (sqlDr.Read())
                    {
                        result.Add("ContactCode",  GetLongValue(sqlDr, ReqSqlConstants.COL_CONTACTCODE).ToString());
                        result.Add("FromCurrency", GetStringValue(sqlDr, ReqSqlConstants.COL_FROMCURRENCY));
                        result.Add("ToCurrency", GetStringValue(sqlDr, ReqSqlConstants.COL_ToCurrencyFlip));
                        result.Add("TotalAmount", sqlDr.IsDBNull(3) ? "0" :GetDecimalValue(sqlDr, ReqSqlConstants.COL_TOTAL_AMOUNT).ToString());
                    }
                }
            }
            finally
            {
                if (!ReferenceEquals(objRefCountingDataReader, null) && !objRefCountingDataReader.IsClosed)
                {
                    objRefCountingDataReader.Close();
                    objRefCountingDataReader.Dispose();
                }

                LogHelper.LogInfo(Log, "GetRequisitionDetailsForExternalWorkFlowProcess Method Ended for id="+requisitionId+" result="+result);
            }
            return result;
           
        }

        public bool UpdateRequisitionExtendedStatus(long documentCode,string ErrorMsg, int updatededExtendedStatus)
        {
            SqlConnection sqlCon = null;
            SqlTransaction sqlTrans = null;
            bool result;
            try
            {
                LogHelper.LogInfo(Log, "In Requisition UpdateRequisitionExtendedStatus Method Started for documentCode=" + documentCode);
                sqlCon = (SqlConnection)ContextSqlConn.CreateConnection();
                sqlCon.Open();
                sqlTrans = sqlCon.BeginTransaction();

                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("Requisition UpdateRequisitionExtendedStatus sp USP_P2P_REQ_UPDATEREQUISITIONEXTENDEDSTATUS with parameter: documentCode=" + documentCode + ", updatededExtendedStatus =" + updatededExtendedStatus + " was called."));

                result = Convert.ToBoolean(ContextSqlConn.ExecuteNonQuery(sqlTrans, ReqSqlConstants.USP_P2P_REQ_UPDATEREQUISITIONEXTENDEDSTATUS, documentCode,updatededExtendedStatus, ErrorMsg), NumberFormatInfo.InvariantInfo);
                if (!ReferenceEquals(sqlTrans, null) && !ReferenceEquals(sqlCon, null) && sqlCon.State == ConnectionState.Open)
                {
                    if (result)
                    {
                        sqlTrans.Commit();
                    }
                    else
                    {
                        try
                        {
                            sqlTrans.Rollback();
                        }
                        catch (InvalidOperationException error)
                        {
                            if (Log.IsInfoEnabled) Log.Info(error.Message);
                        }

                    }
                }
            }
            catch(Exception ex)
            {
                if (!ReferenceEquals(sqlTrans, null) && !ReferenceEquals(sqlCon, null) && sqlCon.State == ConnectionState.Open)
                {
                    try
                    {
                        sqlTrans.Rollback();
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
                if (!ReferenceEquals(sqlCon, null) && sqlCon.State != ConnectionState.Closed)
                {
                    sqlCon.Close();
                    sqlCon.Dispose();
                }
                LogHelper.LogInfo(Log, "Requisition UpdateRequisitionExtendedStatus DAO Method Ended for documentCode=" + documentCode);
            }
            return result;
        }



        public bool SaveRequisitionBusinessUnit(long documentCode, long buId)
        {
            SqlConnection sqlCon = null;
            SqlTransaction sqlTrans = null;
            bool result;
            try
            {
                LogHelper.LogInfo(Log, "In Requisition SaveRequisitionBusinessUnit Method Started for documentCode=" + documentCode);

                //ReliableSqlDatabase sqlHelper = SqlHelper(UserContext.ClientName);
                sqlCon = (SqlConnection)ContextSqlConn.CreateConnection();
                sqlCon.Open();
                sqlTrans = sqlCon.BeginTransaction();

                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("Requisition SaveRequisitionBusinessUnit sp usp_P2P_REQ_SaveBusinessUnit with parameter: documentCode=" + documentCode + ", buId =" + buId + " was called."));

                result = Convert.ToBoolean(ContextSqlConn.ExecuteNonQuery(sqlTrans, ReqSqlConstants.USP_P2P_REQ_SAVEBUSINESSUNIT, documentCode, buId), NumberFormatInfo.InvariantInfo);
                if (!ReferenceEquals(sqlTrans, null) && !ReferenceEquals(sqlCon, null) && sqlCon.State == ConnectionState.Open)
                {
                    if (result)
                    {
                        sqlTrans.Commit();
                    }
                    else
                    {
                        try
                        {
                            sqlTrans.Rollback();
                        }
                        catch (InvalidOperationException error)
                        {
                            if (Log.IsInfoEnabled) Log.Info(error.Message);
                        }

                    }
                }
            }
            catch
            {
                if (!ReferenceEquals(sqlTrans, null) && !ReferenceEquals(sqlCon, null) && sqlCon.State == ConnectionState.Open)
                {
                    try
                    {
                        sqlTrans.Rollback();
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
                if (!ReferenceEquals(sqlCon, null) && sqlCon.State != ConnectionState.Closed)
                {
                    sqlCon.Close();
                    sqlCon.Dispose();
                }
                LogHelper.LogInfo(Log, "Requisition SaveRequisitionBusinessUnit Method Ended for documentCode=" + documentCode);
            }
            return result;
        }

        public bool SaveDocumentBusinessUnit(long documentCode)
        {
            SqlConnection sqlCon = null;
            SqlTransaction sqlTrans = null;
            RefCountingDataReader objRefCountingDataReader = null;
            try
            {
                LogHelper.LogInfo(Log, "In Requisition SaveDocumentBusinessUnit Method Started for documentCode=" + documentCode);

                var sqlHelper = ContextSqlConn;
                sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                sqlCon.Open();
                sqlTrans = sqlCon.BeginTransaction();
                Collection<DocumentBU> lstBU = new Collection<DocumentBU>();

                objRefCountingDataReader = (RefCountingDataReader)
                   ContextSqlConn.ExecuteReader(ReqSqlConstants.USP_P2P_REQ_GETBUSINESSUNIT, documentCode);
                if (objRefCountingDataReader != null)
                {
                    var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;
                    while (sqlDr.Read())
                    {
                        if (GetLongValue(sqlDr, ReqSqlConstants.COL_BUID) > 0)
                            lstBU.Add(new DocumentBU() { BusinessUnitCode = GetLongValue(sqlDr, ReqSqlConstants.COL_BUID) });
                    }
                }

                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("Requisition SaveDocumentDetails was called."));



                var result = true;
                if (lstBU.Any(data => data.BusinessUnitCode > 0))
                {
                    SQLDocumentDAO sqlDocumentDAO = new SQLDocumentDAO();
                    sqlDocumentDAO.SqlTransaction = sqlTrans;
                    sqlDocumentDAO.ReliableSqlDatabase = sqlHelper;
                    sqlDocumentDAO.UserContext = UserContext;
                    sqlDocumentDAO.GepConfiguration = GepConfiguration;
                    result = sqlDocumentDAO.SaveDocumnetBU(documentCode, lstBU, (int)DocumentType.Requisition);
                }
                if (!ReferenceEquals(sqlTrans, null) && !ReferenceEquals(sqlCon, null) && sqlCon.State == ConnectionState.Open)
                {
                    if (result)
                    {
                        sqlTrans.Commit();
                    }
                    else
                    {
                        try
                        {
                            sqlTrans.Rollback();
                        }
                        catch (InvalidOperationException error)
                        {
                            if (Log.IsInfoEnabled) Log.Info(error.Message);
                        }

                    }
                }

                return result;
            }
            catch
            {
                if (!ReferenceEquals(sqlTrans, null) && !ReferenceEquals(sqlCon, null) && sqlCon.State == ConnectionState.Open)
                {
                    try
                    {
                        sqlTrans.Rollback();
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
                if (!ReferenceEquals(sqlCon, null) && sqlCon.State != ConnectionState.Closed)
                {
                    sqlCon.Close();
                    sqlCon.Dispose();
                }
                if (!ReferenceEquals(objRefCountingDataReader, null) && !objRefCountingDataReader.IsClosed)
                {
                    objRefCountingDataReader.Close();
                    objRefCountingDataReader.Dispose();
                }
                LogHelper.LogInfo(Log, "Requisition SaveDocumentBusinessUnit Method Ended for documentCode=" + documentCode);
            }

            return false;
        }

        public long CancelLineItem(long itemId, DocumentStatus status, int itemType, int maxPrecessionValue = 0, int maxPrecessionValueTotal = 0, int maxPrecessionValueForTaxAndCharges = 0)
        {
            throw new NotImplementedException();
        }

        public string AddTemplateItemInReq(long documentCode, string templateIds, List<KeyValuePair<long, decimal>> items, long pasCode, int shiptoLocationId, int itemType, int inventorySource, string buIds, string shippingMethod, int precessionValue, int maxPrecessionforTotal, int maxPrecessionForTaxesAndCharges, string populateDefaultNeedByDate, int populateDefaultNeedByDateByDays)
        {
            SqlConnection sqlCon = null;
            SqlTransaction sqlTrans = null;
            bool result = false;
            long contactCode = 0;
            string strReqItemIds = "";
            try
            {
                LogHelper.LogInfo(Log, "In Requisition AddTemplateItemInReq Method Started for documentCode=" + documentCode);

                contactCode = UserContext.ContactCode;
                if (items != null)
                {
                    if (Log.IsDebugEnabled)
                        Log.Debug(string.Concat("Requisition SaveRequisitionBusinessUnit sp usp_P2P_REQ_SaveBusinessUnit with parameter: documentCode=" + documentCode,
                                                ", pasCode = " + pasCode + ",contactCode = " + contactCode + ",itemType = " + itemType + " was called."));

                    var sqlHelper = ContextSqlConn;
                    sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                    sqlCon.Open();
                    sqlTrans = sqlCon.BeginTransaction();
                    DataSet sqlDataSet = null;
                    using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_ADDTEMPLATEITEMSINREQ))
                    {
                        objSqlCommand.CommandType = CommandType.StoredProcedure;
                        objSqlCommand.Parameters.Add(new SqlParameter("@DocumnetCode", SqlDbType.BigInt) { Value = documentCode });
                        objSqlCommand.Parameters.Add(new SqlParameter("@PasCode", SqlDbType.BigInt) { Value = pasCode });
                        objSqlCommand.Parameters.Add(new SqlParameter("@ShiptoLocationId", SqlDbType.BigInt) { Value = shiptoLocationId });
                        objSqlCommand.Parameters.Add(new SqlParameter("@ContactCode", SqlDbType.BigInt) { Value = contactCode });
                        objSqlCommand.Parameters.Add(new SqlParameter("@TemplateIds", SqlDbType.VarChar) { Value = templateIds });
                        objSqlCommand.Parameters.Add(new SqlParameter("@ItemType", SqlDbType.Int) { Value = itemType });
                        objSqlCommand.Parameters.Add(new SqlParameter("@InventorySource", SqlDbType.Int) { Value = inventorySource });
                        objSqlCommand.Parameters.Add(new SqlParameter("@TemplateItems", SqlDbType.Structured)
                        {
                            TypeName = "tvp_P2P_TemplateItem",
                            Value = P2P.DataAccessObjects.DAOHelper.ConvertToDataTable(items, GetTemplateItemProperties)
                        });
                        objSqlCommand.Parameters.Add(new SqlParameter("@BUId", SqlDbType.VarChar) { Value = buIds });
                        objSqlCommand.Parameters.Add(new SqlParameter("@shippingMethod", SqlDbType.VarChar) { Value = shippingMethod });
                        objSqlCommand.Parameters.Add(new SqlParameter("@MaxPrecessionValue", SqlDbType.Int) { Value = precessionValue });
                        objSqlCommand.Parameters.Add(new SqlParameter("@MaxPrecessionValueTotal", SqlDbType.Int) { Value = maxPrecessionforTotal });
                        objSqlCommand.Parameters.Add(new SqlParameter("@MaxPrecessionValueForTaxAndCharges", SqlDbType.Int) { Value = maxPrecessionForTaxesAndCharges });
                        objSqlCommand.Parameters.Add(new SqlParameter("@PopulateDefaultNeedByDate", SqlDbType.VarChar) { Value = populateDefaultNeedByDate });
                        objSqlCommand.Parameters.Add(new SqlParameter("@PopulateDefaultNeedByDateByDays", SqlDbType.Int) { Value = populateDefaultNeedByDateByDays });
                        sqlDataSet = sqlHelper.ExecuteDataSet(objSqlCommand, sqlTrans);
                    }

                    #region Save Document Additional Info
                    if (sqlDataSet != null && sqlDataSet.Tables.Count > 0)
                    {
                        if (sqlDataSet.Tables[0].Rows.Count > 0)
                        {
                            foreach (DataRow dr in sqlDataSet.Tables[0].Rows)
                            {
                                strReqItemIds += Convert.ToString((long)dr[ReqSqlConstants.COL_REQUISITION_ITEM_ID], CultureInfo.InvariantCulture) + ',';
                            }
                            if (strReqItemIds.EndsWith(","))
                                strReqItemIds = strReqItemIds.Substring(0, strReqItemIds.Length - 1);
                            result = true;
                        }
                    }

                    if (result)
                    {

                        SQLDocumentDAO sqlDocumentDAO = new SQLDocumentDAO();
                        sqlDocumentDAO.SqlTransaction = sqlTrans;
                        sqlDocumentDAO.ReliableSqlDatabase = sqlHelper;
                        sqlDocumentDAO.UserContext = UserContext;
                        sqlDocumentDAO.GepConfiguration = GepConfiguration;


                        SaveRequisitionAdditionalDetails(documentCode, sqlDocumentDAO);
                    }

                    #endregion

                }
                if (!ReferenceEquals(sqlTrans, null) && !ReferenceEquals(sqlCon, null) && sqlCon.State == ConnectionState.Open)
                    sqlTrans.Commit();
            }
            catch
            {
                if (!ReferenceEquals(sqlTrans, null) && !ReferenceEquals(sqlCon, null) && sqlCon.State == ConnectionState.Open)
                {
                    try
                    {
                        sqlTrans.Rollback();
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
                if (!ReferenceEquals(sqlCon, null) && sqlCon.State != ConnectionState.Closed)
                {
                    sqlCon.Close();
                    sqlCon.Dispose();
                }
                LogHelper.LogInfo(Log, "Requisition AddTemplateItemInReq Method Ended for documentCode=" + documentCode);
            }
            return strReqItemIds;
        }

        private List<KeyValuePair<Type, string>> GetTemplateItemProperties()
        {
            var lstTemplateItemProperties = new List<KeyValuePair<Type, string>>();
            lstTemplateItemProperties.Add(new KeyValuePair<Type, string>(typeof(long), "Key"));
            lstTemplateItemProperties.Add(new KeyValuePair<Type, string>(typeof(decimal), "Value"));
            return lstTemplateItemProperties;
        }

        public bool SaveDocumentBusinessUnit(long documentCode, Collection<DocumentBU> lstDocumentBu, int docType)
        {
            throw new NotImplementedException();
        }

        public bool UpdateDocumentStatusInBulk(Collection<DocumentInformation> lstDocumentInfo)
        {
            throw new NotImplementedException();
        }

        public List<SplitAccountingFields> GetAllSplitAccountingFields(P2PDocumentType docType, LevelType levelType, int structureId, long lobId, long AEEntityDetailCode)
        {
            throw new NotImplementedException();
        }
        public List<SplitAccountingFields> GetAllSplitAccountingFieldsWithDefaultValues(P2PDocumentType docType, long parentEntityDetailCode, LevelType levelType)
        {

            RefCountingDataReader objRefCountingDataReader = null;
            var lstsplitAccountingFields = new List<SplitAccountingFields>();

            try
            {
                LogHelper.LogInfo(Log, "In  GetAllSplitAccountingFieldsWithDefaultValues Method Started.");

                switch (docType)
                {
                    case P2PDocumentType.Requisition:

                        objRefCountingDataReader =
                            (RefCountingDataReader)
                            ContextSqlConn.ExecuteReader(
                                ReqSqlConstants.USP_P2P_GETALLSPLITACCOUNTINGDEFAULTVALUES, (int)DocumentType.Requisition, parentEntityDetailCode, levelType);
                        break;
                }

                if (objRefCountingDataReader != null)
                {
                    var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;

                    while (sqlDr.Read())
                    {
                        var splitAccountingFields = new SplitAccountingFields();
                        splitAccountingFields.SplitAccountingFieldId = GetIntValue(sqlDr,
                                                                                   ReqSqlConstants.
                                                                                       COL_SPLITACCOUNTINGFIELD_CONFIGID);
                        splitAccountingFields.FieldName = GetStringValue(sqlDr, ReqSqlConstants.COL_FIELD_NAME);
                        splitAccountingFields.FieldControls = (FieldControls)GetTinyIntValue(sqlDr, ReqSqlConstants.COL_FIELD_CONTROL_TYPE);
                        splitAccountingFields.FieldOrder = GetIntValue(sqlDr, ReqSqlConstants.COL_FIELD_ORDER);
                        splitAccountingFields.ParentSplitAccountingFieldId = GetIntValue(sqlDr, ReqSqlConstants.COL_PARENT_FIELD_CONFIG_ID);
                        splitAccountingFields.IsMandatory = GetBoolValue(sqlDr, ReqSqlConstants.COL_ISMANDATORY);
                        splitAccountingFields.EntityTypeId = GetIntValue(sqlDr, ReqSqlConstants.COL_ENTITY_TYPE_ID);
                        splitAccountingFields.Title = GetStringValue(sqlDr, ReqSqlConstants.COL_TITLE);
                        splitAccountingFields.CodeCombinationOrder = GetTinyIntValue(sqlDr, ReqSqlConstants.COL_CODE_COMBINATION_ORDER);
                        splitAccountingFields.DisplayName = GetStringValue(sqlDr, ReqSqlConstants.COL_SPLIT_ITEM_DISPLAYNAME);
                        splitAccountingFields.EntityCode = GetStringValue(sqlDr, ReqSqlConstants.COL_ENTITY_CODE);
                        splitAccountingFields.ParentEntityDetailCode = GetIntValue(sqlDr, ReqSqlConstants.COL_SPLIT_ITEM_PARENNTENTITYDETAILCODE);
                        splitAccountingFields.EntityDetailCode = GetIntValue(sqlDr, ReqSqlConstants.COL_SPLIT_ITEM_ENTITYDETAILCODE);
                        lstsplitAccountingFields.Add(splitAccountingFields);
                    }
                }

            }
            finally
            {
                if (!ReferenceEquals(objRefCountingDataReader, null) && !objRefCountingDataReader.IsClosed)
                {
                    objRefCountingDataReader.Close();
                    objRefCountingDataReader.Dispose();
                }
                LogHelper.LogInfo(Log, " GetAllSplitAccountingFieldsWithDefaultValues Method completed.");
            }

            return lstsplitAccountingFields;
        }

        public List<RequisitionSplitItems> GetRequisitionAccountingDetailsByItemId(long requisitionItemId, int pageIndex, int pageSize, int itemType, int precessionValue = 0, int precissionTotal = 0, int precessionValueForTaxAndCharges = 0)
        {
            var requisitionSplitItems = new List<RequisitionSplitItems>();
            try
            {
                LogHelper.LogInfo(Log, "Requisition GetRequisitionAccountingDetailsByItemId Method Started.");
                var splitItemsDataSet = new DataSet();
                if (itemType != 3)
                {

                    splitItemsDataSet =
                   ContextSqlConn.ExecuteDataSet(ReqSqlConstants.USP_P2P_REQ_GETREQUISITIONACCOUNTINGDETAILSBYITEMID,
                                                 new object[] { requisitionItemId, pageIndex, pageSize, LevelType.ItemLevel, precessionValue, precissionTotal, precessionValueForTaxAndCharges });
                }
                else
                {
                    splitItemsDataSet =
                      ContextSqlConn.ExecuteDataSet(ReqSqlConstants.USP_P2P_REQ_GETREQUISITIONADVPAYMENTACCOUNTINGDETAILSBYITEMID,

                                                    new object[] { requisitionItemId, pageIndex, pageSize, LevelType.ItemLevel });
                }

                if (splitItemsDataSet != null && splitItemsDataSet.Tables.Count > 0)
                {
                    if (splitItemsDataSet.Tables[0].Rows.Count > 0)
                    {
                        foreach (DataRow dr in splitItemsDataSet.Tables[0].Rows)
                        {
                            var objRequisitionSplitItems = new RequisitionSplitItems
                            {
                                DocumentItemId = (long)dr[ReqSqlConstants.COL_REQUISITION_ITEM_ID],
                                DocumentSplitItemId = (long)dr[ReqSqlConstants.COL_REQUISITION_SPLIT_ITEM_ID],
                                Percentage = Convert.ToDecimal(dr[ReqSqlConstants.COL_PERCENTAGE], CultureInfo.InvariantCulture),
                                Quantity = Convert.ToDecimal(dr[ReqSqlConstants.COL_QUANTITY], CultureInfo.InvariantCulture),
                                Tax = dr[ReqSqlConstants.COL_TAX] != DBNull.Value ? Convert.ToDecimal(dr[ReqSqlConstants.COL_TAX], CultureInfo.InvariantCulture) : (decimal?)null,
                                ShippingCharges = dr[ReqSqlConstants.COL_SHIPPING_CHARGES] != DBNull.Value ? Convert.ToDecimal(dr[ReqSqlConstants.COL_SHIPPING_CHARGES], CultureInfo.InvariantCulture) : (decimal?)null,
                                AdditionalCharges = dr[ReqSqlConstants.COL_ADDITIONAL_CHARGES] != DBNull.Value ? Convert.ToDecimal(dr[ReqSqlConstants.COL_ADDITIONAL_CHARGES], CultureInfo.InvariantCulture) : (decimal?)null,
                                SplitItemTotal = dr[ReqSqlConstants.COL_SPLIT_ITEM_TOTAL] != DBNull.Value ? Convert.ToDecimal(dr[ReqSqlConstants.COL_SPLIT_ITEM_TOTAL], CultureInfo.InvariantCulture) : (decimal?)null,
                                ErrorCode = Convert.ToString(dr[ReqSqlConstants.COL_ERROR_CODE], CultureInfo.InvariantCulture),
                                TotalRecords = Convert.ToInt32(dr[ReqSqlConstants.COL_TOTAL_RECORDS], CultureInfo.InvariantCulture),
                                SplitType = (SplitType)Convert.ToInt32(dr[ReqSqlConstants.COL_SPLIT_TYPE], CultureInfo.InvariantCulture)
                            };

                            var drSplits =
                                splitItemsDataSet.Tables[1].Select("RequisitionSplitItemId =" +
                                                                   objRequisitionSplitItems.DocumentSplitItemId.ToString
                                                                       (CultureInfo.InvariantCulture));

                            objRequisitionSplitItems.DocumentSplitItemEntities = new List<DocumentSplitItemEntity>();
                            foreach (var drSplit in drSplits)
                            {
                                var documentSplitItemEntity = new DocumentSplitItemEntity();
                                documentSplitItemEntity.SplitAccountingFieldId = (int)drSplit[ReqSqlConstants.COL_SPLIT_ACCOUNTING_FIELD_CONFIG_ID];
                                documentSplitItemEntity.SplitAccountingFieldValue = drSplit[ReqSqlConstants.COL_SPLIT_ACCOUNTING_FIELD_VALUE].ToString();
                                documentSplitItemEntity.EntityDisplayName = drSplit[ReqSqlConstants.COL_ENTITY_DISPLAY_NAME].ToString();
                                documentSplitItemEntity.EntityTypeId = (int)drSplit[ReqSqlConstants.COL_ENTITY_TYPE_ID];
                                documentSplitItemEntity.EntityCode = drSplit[ReqSqlConstants.COL_ENTITY_CODE].ToString();
                                documentSplitItemEntity.EntityType = drSplit[ReqSqlConstants.COL_ENTITY_TYPE].ToString();
                                documentSplitItemEntity.CodeCombinationOrder = Convert.ToInt16(drSplit[ReqSqlConstants.COL_CODE_COMBINATION_ORDER], CultureInfo.InvariantCulture);
                                documentSplitItemEntity.ParentEntityDetailCode = (long)drSplit[ReqSqlConstants.COL_SPLIT_ITEM_PARENNTENTITYDETAILCODE];
                                objRequisitionSplitItems.DocumentSplitItemEntities.Add(documentSplitItemEntity);

                            }
                            requisitionSplitItems.Add(objRequisitionSplitItems);
                        }
                    }
                }

            }
            finally
            {
                LogHelper.LogInfo(Log, "Requisition GetRequisitionAccountingDetailsByItemId Method completed.");
            }

            return requisitionSplitItems;
        }

        public bool SaveItemStatus(long documentCode, DocumentStatus itemStatus)
        {
            throw new NotImplementedException();
        }
        public long SaveRequisitionAccountingDetails(List<RequisitionSplitItems> requisitionSplitItems, List<DocumentSplitItemEntity> requisitionSplitItemEntities, decimal lineItemQuantity, int precessionValue, int maxPrecessionforTotal, int maxPrecessionForTaxesAndCharges, bool UseTaxMaster = true, bool updateTaxes = true)

        {
            long validationStatus = 0;
            long documentCode = 0;
            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;
            try
            {
                LogHelper.LogInfo(Log, "Requisition SaveRequisitionAccountingDetails Method Started for DocumentItemId = " + requisitionSplitItems[0].DocumentItemId);

                var sqlHelper = ContextSqlConn;
                _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();

                DataTable dtReqItems;
                DataTable dtReqItemEntities = null;
                dtReqItems = P2P.DataAccessObjects.DAOHelper.ConvertToDataTable(requisitionSplitItems, GetRequisitionSplitItemTable);
                dtReqItemEntities = P2P.DataAccessObjects.DAOHelper.ConvertToDataTable(requisitionSplitItemEntities,
                                                       GetRequisitionSplitItemEntitiesTable);
                //dtReqItemEntities = ConvertToDataTable(requisitionSplitItems.SelectMany(item => item.DocumentSplitItemEntities).ToList<DocumentSplitItemEntity>(), GetRequisitionSplitItemEntitiesTable);
                //string result = string.Empty;
                using (var objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_SAVEREQUISITIONACCOUNTINGDETAILS, _sqlCon))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.Add(new SqlParameter("@tvp_P2P_SplitItems", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_P2P_SPLITITEMS,
                        Value = dtReqItems
                    });
                    objSqlCommand.Parameters.Add(new SqlParameter("@tvp_P2P_SplitItemsEntities", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_P2P_SPLITITEMSENTITIES,
                        Value = dtReqItemEntities
                    });
                    objSqlCommand.Parameters.AddWithValue("@UserId", UserContext.ContactCode);
                    objSqlCommand.Parameters.AddWithValue("@lineItemQuantity", lineItemQuantity);
                    objSqlCommand.Parameters.AddWithValue("@precessionValue", precessionValue);
                    objSqlCommand.Parameters.AddWithValue("@precissionTotal", maxPrecessionforTotal);
                    objSqlCommand.Parameters.AddWithValue("@precessionValueForTaxAndCharges", maxPrecessionForTaxesAndCharges);
                    objSqlCommand.Parameters.AddWithValue("@TaxBasedOnShipTo", UseTaxMaster);
                    objSqlCommand.Parameters.AddWithValue("@UpdateTaxes", updateTaxes);
                    var sqlDr = (SqlDataReader)sqlHelper.ExecuteReader(objSqlCommand, _sqlTrans);
                    if (sqlDr != null)
                    {
                        while (sqlDr.Read())
                        {
                            validationStatus = GetLongValue(sqlDr, ReqSqlConstants.COL_ACCOUNTING_STATUS);
                            documentCode = GetLongValue(sqlDr, ReqSqlConstants.COL_REQUISITION_ID);
                        }
                        sqlDr.Close();
                    }
                }
                if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
                {
                    _sqlTrans.Commit();
                    if (this.UserContext.Product != GEPSuite.eInterface && documentCode > 0)
                        AddIntoSearchIndexerQueueing(documentCode, (int)DocumentType.Requisition, UserContext, GepConfiguration);


                }
            }
            catch
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
                throw;
            }
            finally
            {
                if (!ReferenceEquals(_sqlCon, null) && _sqlCon.State != ConnectionState.Closed)
                {
                    _sqlCon.Close();
                    _sqlCon.Dispose();
                }
                LogHelper.LogInfo(Log, "Requisition SaveRequisitionAccountingDetails Method Ended for DocumentItemId = " + requisitionSplitItems[0].DocumentItemId);
            }

            return validationStatus;

        }

        private List<KeyValuePair<Type, string>> GetRequisitionSplitItemTable()
        {
            var lstMatchItemProperties = new List<KeyValuePair<Type, string>>();
            lstMatchItemProperties.Add(new KeyValuePair<Type, string>(typeof(long), "DocumentSplitItemId"));
            lstMatchItemProperties.Add(new KeyValuePair<Type, string>(typeof(long), "DocumentItemId"));
            lstMatchItemProperties.Add(new KeyValuePair<Type, string>(typeof(short), "SplitType"));
            lstMatchItemProperties.Add(new KeyValuePair<Type, string>(typeof(decimal), "Percentage"));
            lstMatchItemProperties.Add(new KeyValuePair<Type, string>(typeof(decimal), "Quantity"));
            lstMatchItemProperties.Add(new KeyValuePair<Type, string>(typeof(decimal), "Tax"));
            lstMatchItemProperties.Add(new KeyValuePair<Type, string>(typeof(decimal), "ShippingCharges"));
            lstMatchItemProperties.Add(new KeyValuePair<Type, string>(typeof(decimal), "AdditionalCharges"));
            lstMatchItemProperties.Add(new KeyValuePair<Type, string>(typeof(decimal), "SplitItemTotal"));
            lstMatchItemProperties.Add(new KeyValuePair<Type, string>(typeof(bool), "IsDeleted"));
            lstMatchItemProperties.Add(new KeyValuePair<Type, string>(typeof(string), "ErrorCode"));
            lstMatchItemProperties.Add(new KeyValuePair<Type, string>(typeof(int), "UiId"));
            lstMatchItemProperties.Add(new KeyValuePair<Type, string>(typeof(decimal), "OverallLimitSplitItem"));
            return lstMatchItemProperties;
        }

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

        private List<KeyValuePair<Type, string>> GetDocumentAdditionalEntityTable()
        {
            var lstDocumentAdditionalEntity = new List<KeyValuePair<Type, string>>();
            lstDocumentAdditionalEntity.Add(new KeyValuePair<Type, string>(typeof(Int32), "EntityId"));
            lstDocumentAdditionalEntity.Add(new KeyValuePair<Type, string>(typeof(long), "EntityDetailCode"));
            return lstDocumentAdditionalEntity;
        }

        public bool CheckAccountingSplitValidations(long requisitionId)
        {
            bool result = false;
            SqlConnection _sqlCon = null;
            try
            {
                LogHelper.LogInfo(Log, "Requisition CheckAccountingSplitValidations Method Started for requisitionId = " + requisitionId);

                var sqlHelper = ContextSqlConn;
                _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                _sqlCon.Open();

                result = Convert.ToBoolean(sqlHelper.ExecuteScalar(ReqSqlConstants.USP_P2P_REQ_CHECK_REQUISITION_ACCOUNTING_SPLIT_VALIDATIONS,
                                                                  new object[] { requisitionId }), NumberFormatInfo.InvariantInfo);

            }
            catch
            {
                throw;
            }
            finally
            {
                if (!ReferenceEquals(_sqlCon, null) && _sqlCon.State != ConnectionState.Closed)
                {
                    _sqlCon.Close();
                    _sqlCon.Dispose();
                }
                LogHelper.LogInfo(Log, "Requisition CheckAccountingSplitValidations Method Ended for requisitionId = " + requisitionId);
            }

            return result;

        }

        //private static DataTable ConvertReqSplitItemstoDataTable(IEnumerable<RequisitionSplitItems> requisitionSplitItems, long userId)
        //{
        //    if(requisitionSplitItems != null)
        //    {
        //        var dt = new DataTable {Locale = CultureInfo.InvariantCulture};
        //        dt.Columns.Add("RequisitionSplitItemId", typeof(long));
        //        dt.Columns.Add("RequisitionItemId", typeof(long));
        //        dt.Columns.Add("Quantity", typeof(decimal));
        //        dt.Columns.Add("Tax", typeof(decimal));
        //        dt.Columns.Add("ShippingCharges", typeof(decimal));
        //        dt.Columns.Add("AdditionalCharges", typeof(decimal));
        //        dt.Columns.Add("SplitItemTotal", typeof(decimal));
        //        dt.Columns.Add("IsDeleted", typeof(bool));
        //        dt.Columns.Add("SplitType", typeof(short));
        //        dt.Columns.Add("SplitErrorCode", typeof(int));
        //        dt.Columns.Add("UserId", typeof(long));
        //        dt.Columns.Add("CodeCombination", typeof (string));
        //        dt.Columns.Add("UiId", typeof (int));

        //        foreach (var item in requisitionSplitItems)
        //        {
        //            var dr = dt.NewRow();
        //            dr["RequisitionSplitItemId"] = item.DocumentSplitItemId;
        //            dr["RequisitionItemId"] = item.DocumentItemId;
        //            dr["Quantity"] = item.Quantity;
        //            dr["Tax"] = item.DocumentSplitItemId;
        //            dr["ShippingCharges"] = item.ShippingCharges;
        //            dr["AdditionalCharges"] = item.AdditionalCharges;
        //            dr["SplitItemTotal"] = item.SplitItemTotal;
        //            dr["IsDeleted"] = item.IsDeleted;
        //            dr["SplitType"] = item.SplitType;
        //            dr["SplitErrorCode"] = item.ErrorCode;
        //            dr["UserId"] = userId;
        //            dr["CodeCombination"] = item.CodeCombination;
        //            dr["UiId"] = item.UiId;

        //            dt.Rows.Add(dr);
        //        }

        //        return dt;
        //    }

        //    return null;
        //}

        //private static DataTable ConvertSplitItemEntitiestoDataTable(IReadOnlyList<RequisitionSplitItems> requisitionSplitItems)
        //{
        //    if (requisitionSplitItems.Any())
        //    {
        //        var dt = new DataTable {Locale = CultureInfo.InvariantCulture};

        //        dt.Columns.Add("RequisitionSplitItemEntityId", typeof(long));
        //        dt.Columns.Add("RequisitionSplitItemId", typeof(long));
        //        dt.Columns.Add("SplitAccountingFieldConfigId", typeof(int));
        //        dt.Columns.Add("SplitAccountingFieldValue", typeof(string));
        //        dt.Columns.Add("EntityCode", typeof(string));
        //        dt.Columns.Add("UiId", typeof (int));

        //        for (var i = 0; i < requisitionSplitItems.Count(); i++)
        //        {
        //            foreach (var items in requisitionSplitItems[i].DocumentSplitItemEntities)
        //            {
        //                var dr = dt.NewRow();
        //                dr["RequisitionSplitItemEntityId"] = items.DocumentSplitItemEntityId;
        //                dr["RequisitionSplitItemId"] = items.DocumentSplitItemId;
        //                dr["SplitAccountingFieldConfigId"] = items.SplitAccountingFieldId;
        //                dr["SplitAccountingFieldValue"] = items.SplitAccountingFieldValue;
        //                dr["EntityCode"] = items.EntityCode;
        //                dr["UiId"] = items.UiId;

        //                dt.Rows.Add(dr);
        //            }
        //        }

        //        return dt;
        //    }
        //    return null;
        //}

        public bool DeleteDocumentByDocumentCode(long documentCode)
        {
            SqlConnection sqlCon = null;
            SqlTransaction sqlTrans = null;
            LogHelper.LogInfo(Log, string.Concat("In Requisition DeleteDocumentById Method Started for documentCode=", documentCode));

            sqlCon = (SqlConnection)ContextSqlConn.CreateConnection();
            sqlCon.Open();
            sqlTrans = sqlCon.BeginTransaction();

            bool isDeleted;

            SQLDocumentDAO sqlDocumentDAO = new SQLDocumentDAO();
            sqlDocumentDAO.SqlTransaction = sqlTrans;
            sqlDocumentDAO.ReliableSqlDatabase = ContextSqlConn;
            sqlDocumentDAO.UserContext = UserContext;
            sqlDocumentDAO.GepConfiguration = GepConfiguration;

            try
            {
                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("Base DeleteDocumentById method called with parameter: documentCode=", documentCode));

                isDeleted = sqlDocumentDAO.DeleteDocumentById(documentCode);

                if (isDeleted)
                {
                    if (Log.IsDebugEnabled)
                        Log.Debug(string.Concat("Requisition DeleteDocumentById sp usp_DeleteRequisitionByDocumentCode with parameter: documentCode=", documentCode));

                    isDeleted = Convert.ToBoolean(ContextSqlConn.ExecuteNonQuery(sqlTrans, ReqSqlConstants.USP_P2P_DELETE_REQUISITIONBY_DOCUMENTCODE, documentCode), NumberFormatInfo.InvariantInfo);

                    if (isDeleted)
                    {
                        if (!ReferenceEquals(sqlTrans, null) && !ReferenceEquals(sqlCon, null) && sqlCon.State == ConnectionState.Open)
                            sqlTrans.Commit();
                    }
                    else
                    {
                        if (!ReferenceEquals(sqlTrans, null) && !ReferenceEquals(sqlCon, null) && sqlCon.State == ConnectionState.Open)
                        {
                            try
                            {
                                sqlTrans.Rollback();
                            }
                            catch (InvalidOperationException error)
                            {
                                if (Log.IsInfoEnabled) Log.Info(error.Message);
                            }
                        }
                    }
                }
            }
            catch
            {
                if (!ReferenceEquals(sqlTrans, null) && !ReferenceEquals(sqlCon, null) && sqlCon.State == ConnectionState.Open)
                {
                    try
                    {
                        sqlTrans.Rollback();
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
                if (!ReferenceEquals(sqlCon, null) && sqlCon.State != ConnectionState.Closed)
                {
                    sqlCon.Close();
                    sqlCon.Dispose();
                }
                LogHelper.LogInfo(Log, string.Concat("Requisition DeleteDocumentById Method Ended for documentCode=", documentCode));
            }

            return isDeleted;
        }

        //public long GetDocumentIdByDocumentCode(long documentCode)
        //{
        //    try
        //    {
        //        LogHelper.LogInfo(Log, "Requisition getDocumentIdByDocumentCode Method Started for DocumentCode = " + documentCode);
        //        _sqlCon = (SqlConnection)ContextSqlConn.CreateConnection();
        //        _sqlCon.Open();
        //        _sqlTrans = _sqlCon.BeginTransaction();

        //        if (Log.IsDebugEnabled)
        //            Log.Debug(string.Concat("In Requisition getDocumentIdByDocumentCode Method",
        //                                     "SP: USP_P2P_REQ_GETDOCUMENTIDBYDOCUMENTCODE with parameter: DocumentCode=" + documentCode + " was called."));
        //        var result = Convert.ToInt64(ContextSqlConn.ExecuteScalar(_sqlTrans, ReqSqlConstants.USP_P2P_REQ_GETDOCUMENTIDBYDOCUMENTCODE, documentCode), NumberFormatInfo.InvariantInfo);
        //        _sqlTrans.Commit();
        //        return result;
        //    }
        //    catch (Exception)
        //    {
        //        if (!ReferenceEquals(_sqlTrans, null) )
        //            _sqlTrans.Rollback();
        //        throw;
        //    }
        //    finally
        //    {
        //        if (!ReferenceEquals(_sqlCon, null)  && _sqlCon.State != ConnectionState.Closed)
        //            _sqlCon.Close();
        //        LogHelper.LogInfo(Log, "Requisition getDocumentIdByDocumentCode Method Ended for DocumentCode = " + documentCode);
        //    }
        //}
        public bool CopyRequisitionToRequisition(long newrequisitionId, string requisitionIds, string buIds, int precissionValue = 0, int precissionTotal = 0, int precessionValueForTaxAndCharges = 0,bool showTaxJurisdictionForShipTo = false, List<KeyValuePair<long, decimal>> catlogItems = null, List<KeyValuePair<long, decimal>> itemMasteritems = null, bool enableGetLineItemsBulkAPI = false, List<CurrencyExchageRate> lstCurrencyExchageRates=null)
        {
            var status = false;
            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;
            try
            {
                LogHelper.LogInfo(Log, "Requisition CopyRequisitiontoRequisition Method Started for Copy requisitionIds=" + requisitionIds + " to requisitionId" + newrequisitionId);

                var sqlHelper = ContextSqlConn;
                _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();

                DataTable dtCatalogLineDetails = null;
                if (catlogItems != null)
                    dtCatalogLineDetails = ConvertCatalogKeyValueToDataTable(catlogItems);
                DataTable dtItemMasterLineDetails = null;
                if (itemMasteritems != null)
                    dtItemMasterLineDetails = ConvertCatalogKeyValueToDataTable(itemMasteritems);

                DataTable dtCurrencyExchageRates = null;
                if (lstCurrencyExchageRates != null)
                    dtCurrencyExchageRates = ConvertCurrencyExchangesToDataTable(lstCurrencyExchageRates);

                if (newrequisitionId > 0)
                {
                    using (var objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_COPYREQUISITIONTOREQUISITION, _sqlCon))
                    {
                        objSqlCommand.CommandType = CommandType.StoredProcedure;
                        objSqlCommand.CommandTimeout = 150;
                        objSqlCommand.Parameters.AddWithValue("@newrequisitionId", newrequisitionId);
                        objSqlCommand.Parameters.AddWithValue("@oldrequisitionIds", requisitionIds);
                        objSqlCommand.Parameters.AddWithValue("@UserId", UserContext.ContactCode);
                        objSqlCommand.Parameters.AddWithValue("@BUId", buIds);
                        objSqlCommand.Parameters.AddWithValue("@precissionValue", precissionValue);
                        objSqlCommand.Parameters.AddWithValue("@precissionTotal", precissionTotal);
                        objSqlCommand.Parameters.AddWithValue("@precessionValueForTaxAndCharges", precessionValueForTaxAndCharges);
                        objSqlCommand.Parameters.AddWithValue("@showTaxJurisdictionForShipTo", showTaxJurisdictionForShipTo);
                        objSqlCommand.Parameters.Add(new SqlParameter("@CatalogLineItems", SqlDbType.Structured)
                        {
                            TypeName = ReqSqlConstants.TVP_CatalogLineitem,
                            Value = dtCatalogLineDetails
                        });
                        objSqlCommand.Parameters.Add(new SqlParameter("@ItemMasterLineItems", SqlDbType.Structured)
                        {
                            TypeName = ReqSqlConstants.TVP_CatalogLineitem,
                            Value = dtItemMasterLineDetails
                        });
                        objSqlCommand.Parameters.AddWithValue("@enableGetLineItemsBulkAPI", enableGetLineItemsBulkAPI);
                        objSqlCommand.Parameters.Add(new SqlParameter("@CurrencyExchangeRates", SqlDbType.Structured)
                        {
                            TypeName = ReqSqlConstants.tvp_CurrencyExchangeRates,
                            Value = dtCurrencyExchageRates
                        });

                        status = (ContextSqlConn.ExecuteScalar(objSqlCommand), CultureInfo.InvariantCulture).ToString() == "1";
                    }
                    if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
                        _sqlTrans.Commit();
                }
                else
                {
                    if (Log.IsWarnEnabled)
                        Log.Warn("In Requisition CopyRequisitiontoRequisition method requisitionId parameter is less than or equal to 0.");
                }
            }
            catch
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
                throw;
            }
            finally
            {
                if (!ReferenceEquals(_sqlCon, null) && _sqlCon.State != ConnectionState.Closed)
                {
                    _sqlCon.Close();
                    _sqlCon.Dispose();
                }
                LogHelper.LogInfo(Log, "Requisition CopyRequisitiontoRequisition Method Ended for Copy requisitionId=" + requisitionIds + " to requisitionId" + newrequisitionId);
            }
            return status;
        }

        public DocumentIntegration.Entities.DocumentIntegrationEntity GetDocumentDetailsByDocumentCode(long documentCode, string documentStatuses, bool isFunctionalAdmin = false, int ACEntityId = 0, List<long> partners = null, List<DocumentIntegration.Entities.IntegrationTimelines> timelines = null, List<long> teammemberList = null)
        {
            DocumentIntegration.Entities.DocumentIntegrationEntity objDocumentIntegrationEntity = new DocumentIntegration.Entities.DocumentIntegrationEntity();
            RefCountingDataReader objrequisitionItemDataReader = null;
            if (documentCode > 0)
            {
                try
                {
                    LogHelper.LogInfo(Log, "GetDocumentIntegrationDetailsByDocumentCode Method Started for documentCode=" + documentCode);

                    DocumentType IsRequestedDocumentTypeRFX = DocumentType.RFP; //  IsRequestedDocType  - 1 for RFX , O for Other
                    var sqlHelper = ContextSqlConn;
                    Document objDocument = new Document();
                    objDocument.DocumentCode = documentCode;
                    objDocument.DocumentTypeInfo = DocumentType.Requisition;
                    objDocument.IsDocumentDetails = true;
                    objDocument.IsAddtionalDetails = true;
                    objDocument.IsStakeholderDetails = true;
                    objDocument.CompanyName = UserContext.ClientName;
                    objDocument.IsFilterByBU = true;
                    objDocument.ACEntityId = ACEntityId;


                    SQLDocumentDAO sqlDocumentDAO = new SQLDocumentDAO();

                    sqlDocumentDAO.ReliableSqlDatabase = sqlHelper;
                    sqlDocumentDAO.UserContext = UserContext;
                    sqlDocumentDAO.GepConfiguration = GepConfiguration;

                    objDocument.LinkedDocumentTypeCode = DocumentType.None;
                    objDocumentIntegrationEntity.Document = sqlDocumentDAO.GetDocumentDetailsById(objDocument, false, isFunctionalAdmin, documentStatuses);

                    objDocumentIntegrationEntity.DocumentStakeHolders = objDocumentIntegrationEntity.Document.DocumentStakeHolderList.ToList();
                    objDocumentIntegrationEntity.DocumentItems = new List<GEP.Cumulus.Item.Entities.LineItem>();

                    LogHelper.LogInfo(Log, "Requisition GetRequisitionItemsByDocumentCode Method Started.");

                    objrequisitionItemDataReader =
                 (RefCountingDataReader)
                 ContextSqlConn.ExecuteReader(ReqSqlConstants.USP_P2P_REQ_GETREQUISITIONLINEITEMS,
                                                                      new object[] { documentCode, 0, 0, 50, "", "", 0, 0, IsRequestedDocumentTypeRFX });




                    if (objrequisitionItemDataReader != null)
                    {
                        var sqlDr = (SqlDataReader)objrequisitionItemDataReader.InnerReader;
                        while (sqlDr.Read())
                        {
                            var objRequisitionLineItems = new GEP.Cumulus.Item.Entities.LineItem
                            {
                                CategoryName = GetStringValue(sqlDr, ReqSqlConstants.COL_CATEGORY_NAME),
                                CurrencyCode = GetStringValue(sqlDr, ReqSqlConstants.COL_CURRENCY),
                                DocumentCode = documentCode,
                                DocumentType = DocumentType.Requisition,
                                ItemCode = GetLongValue(sqlDr, ReqSqlConstants.COL_ITEM_CODE),
                                ItemDescription = GetStringValue(sqlDr, ReqSqlConstants.COL_DESCRIPTION),
                                ItemName = GetStringValue(sqlDr, ReqSqlConstants.COL_SHORT_NAME),
                                ItemType = (GEP.Cumulus.Item.Entities.ItemType)GetTinyIntValue(sqlDr, ReqSqlConstants.COL_ITEM_TYPE_ID),
                                PartnerCode = (long)GetDecimalValue(sqlDr, ReqSqlConstants.COL_PARTNER_CODE),
                                PASCode = GetLongValue(sqlDr, ReqSqlConstants.COL_CATEGORY_ID),
                                Quantity = GetDecimalValue(sqlDr, ReqSqlConstants.COL_QUANTITY),
                                SubAppCode = SubAppCodes.P2P,
                                TaxAmount = sqlDr[ReqSqlConstants.COL_TAX] != DBNull.Value ? GetDecimalValue(sqlDr, ReqSqlConstants.COL_TAX) : 0,
                                TotalPrice = GetDecimalValue(sqlDr, ReqSqlConstants.COL_QUANTITY) * (sqlDr[ReqSqlConstants.COL_UNIT_PRICE] != DBNull.Value ? Convert.ToDecimal(sqlDr[ReqSqlConstants.COL_UNIT_PRICE], CultureInfo.InvariantCulture) : 0),
                                UnitPrice = sqlDr[ReqSqlConstants.COL_UNIT_PRICE] != DBNull.Value ? GetDecimalValue(sqlDr, ReqSqlConstants.COL_UNIT_PRICE) : 0,
                                UOMCode = GetStringValue(sqlDr, ReqSqlConstants.COL_UOM),
                                UOMDescription = GetStringValue(sqlDr, ReqSqlConstants.COL_UOM_DESCRIPTION),
                                LineNumber = Convert.ToString(GetLongValue(sqlDr, ReqSqlConstants.COL_LINENUMBER)),
                                P2PLineItemId = GetLongValue(sqlDr, ReqSqlConstants.COL_P2P_LINE_ITEM_ID),
                                ItemNumber = GetStringValue(sqlDr, ReqSqlConstants.COL_ITEMNUMBER),
                                IncoTermId = GetIntValue(sqlDr, ReqSqlConstants.COL_INCOTERMID),
                                IncoTermCode = GetStringValue(sqlDr, ReqSqlConstants.COL_INCOTERMCODE),
                                IncoTermLocation = GetStringValue(sqlDr, ReqSqlConstants.COL_INCOTERMLOCATION),
                                IncoTermDescription = GetStringValue(sqlDr, ReqSqlConstants.COL_INCOTERMDESCRIPTION),
                                ItemSpecification = GetStringValue(sqlDr, ReqSqlConstants.COL_ITEMSPECIFICATION),
                                InternalPlantMemo = GetStringValue(sqlDr, ReqSqlConstants.COL_INTERNALPLANTMEMO),
                                ItemStatus = (DocumentStatus)GetTinyIntValue(sqlDr, ReqSqlConstants.COL_ITEMSTATUS),
                                ItemId=GetLongValue(sqlDr,ReqSqlConstants.COL_REQUISITION_ITEM_ID),
                                SupplierPartNumber=GetStringValue(sqlDr,ReqSqlConstants.COL_SUPPLIERPARTID),
                                ClientPASCode = GetStringValue(sqlDr, ReqSqlConstants.COL_CLIENTPASCODE),
                                ConversionFactor = sqlDr[ReqSqlConstants.COL_CONVERSIONFACTOR] != DBNull.Value ? GetDecimalValue(sqlDr, ReqSqlConstants.COL_CONVERSIONFACTOR) : 1
                            };

                            objDocumentIntegrationEntity.DocumentItems.Add(objRequisitionLineItems);
                        }
                        try
                        {
                            if (objDocumentIntegrationEntity.DocumentItems != null && objDocumentIntegrationEntity.DocumentItems.Any())
                            {
                                List<long> lstP2PLineitemIds = objDocumentIntegrationEntity.DocumentItems.Select(item => item.P2PLineItemId).Distinct().ToList();
                                bool IsDocumentDeleted = false; //True when rfx is deleted or cancelled else false
                                var result = UpdateRequisitionLineStatusonRFXCreateorUpdate(documentCode, lstP2PLineitemIds, DocumentType.Requisition, IsDocumentDeleted);
                            }
                        }
                        catch (ArgumentNullException argEx)
                        {
                            LogHelper.LogError(Log, "ArgumentNullException UpdateRequisitionLineStatusonRFXCreateorUpdate getting Exception", argEx);
                            LogHelper.LogInfo(Log, "ArgumentNullException Error while calling UpdateRequisitionLineStatusonRFXCreateorUpdate from GetDocumentDetailsByDocumentCode Method for requisitionId" + documentCode);
                        }
                        catch (Exception Excep)
                        {
                            LogHelper.LogError(Log, "Error while calling UpdateRequisitionLineStatusonRFXCreateorUpdate getting Exception", Excep);
                            LogHelper.LogInfo(Log, "Error while calling UpdateRequisitionLineStatusonRFXCreateorUpdate from GetDocumentDetailsByDocumentCode Method for requisitionId" + documentCode);
                        }

                        if (partners != null && partners.Count() > 0)
                        {
                            foreach (var partnerCode in partners)
                            {
                                if (!objDocumentIntegrationEntity.DocumentStakeHolders.Any(e => e.PartnerCode == partnerCode))
                                {
                                    var documentStakeHolder = new DocumentStakeHolder
                                    {
                                        PartnerCode = partnerCode,
                                        StakeholderTypeInfo = StakeholderType.SupplierPrimaryContact
                                    };

                                    objDocumentIntegrationEntity.DocumentStakeHolders.Add(documentStakeHolder);
                                }
                            }

                        }
                        else
                        {
                            var lstPartnerCodes = objDocumentIntegrationEntity.DocumentItems.Where(e => e.PartnerCode != 0).Select(item => item.PartnerCode).Distinct();
                            foreach (var partnerCode in lstPartnerCodes)
                            {
                                var documentStakeHolder = new DocumentStakeHolder
                                {
                                    PartnerCode = partnerCode,
                                    StakeholderTypeInfo = StakeholderType.SupplierPrimaryContact
                                };
                                objDocumentIntegrationEntity.DocumentStakeHolders.Add(documentStakeHolder);
                            }
                        }

                        if (teammemberList != null && teammemberList.Count > 0)
                        {
                            List<DocumentStakeHolder> lstStakeHoldersList = new List<DocumentStakeHolder>();
                            foreach (var teammember in teammemberList.Distinct().ToList())
                            {
                                DocumentStakeHolder documentStakeHolder = new DocumentStakeHolder();
                                documentStakeHolder.ContactCode = teammember;
                                documentStakeHolder.StakeholderTypeInfo = StakeholderType.TeamMembers;
                                documentStakeHolder.DocumentStakeholderId = 2;//1-View,2-Co-Author,4-Evaluator,5-Approver                                                                                 
                                lstStakeHoldersList.Add(documentStakeHolder);

                            }
                            if (objDocumentIntegrationEntity.DocumentStakeHolders!=null && objDocumentIntegrationEntity.DocumentStakeHolders.Count > 0)
                                objDocumentIntegrationEntity.DocumentStakeHolders.AddRange(lstStakeHoldersList.Distinct().ToList());
                            else 
                                objDocumentIntegrationEntity.DocumentStakeHolders = lstStakeHoldersList.Distinct().ToList();
                        }

                        var pasList =
                            objDocumentIntegrationEntity.DocumentItems.Select(
                                item => new DocumentPas { PasCode = item.PASCode, PasName = item.CategoryName }).Distinct();
                        foreach (var pasEntity in pasList)
                        {
                            var documentPas = new DocumentPas
                            {
                                PasCode = pasEntity.PasCode,
                                PasName = pasEntity.PasName
                            };
                            objDocumentIntegrationEntity.Document.DocumentPASList.Add(documentPas);
                        }
                        if (timelines != null)
                        {
                            objDocumentIntegrationEntity.Timelines = timelines;
                            objDocumentIntegrationEntity.IntegrationTemplateType = DocumentIntegration.Entities.IntegrationTemplateType.AutoPublish;
                        }
                        else
                        {
                            objDocumentIntegrationEntity.IntegrationTemplateType = DocumentIntegration.Entities.IntegrationTemplateType.Draft;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.LogError(Log, "Requisition GetDocumentDetailsByDocumentCode getting Exception", ex);
                    throw;
                }
                finally
                {
                    if (!ReferenceEquals(objrequisitionItemDataReader, null) && !objrequisitionItemDataReader.IsClosed)
                    {
                        objrequisitionItemDataReader.Close();
                        objrequisitionItemDataReader.Dispose();
                    }

                    LogHelper.LogInfo(Log, "Requisition GetRequisitionAccountingDetailsByItemId Method completed.");
                }
            }
            return objDocumentIntegrationEntity;

        }

        public P2PDocument GetDocumentDetailByDocumentNumber(string documentNumber, string docStatus, string rejectionStatus, long LOBentityDetailsCode, long Partnercode)
        {
            RefCountingDataReader objRefCountingDataReader = null;
            try
            {
                LogHelper.LogInfo(Log, "Requisition GetDocumentDetailByDocumentNumber Method Started for documentNumber=" + documentNumber);

                if (Log.IsDebugEnabled)
                    Log.Debug(string.Format(CultureInfo.InvariantCulture, "Requisition GetDocumentDetailByDocumentNumber usp_P2P_REQ_GetDocumentDetailsByNumber with parameter: documentNumber={0} was called.", documentNumber));

                objRefCountingDataReader =
                  (RefCountingDataReader)
                  ContextSqlConn.ExecuteReader(ReqSqlConstants.USP_P2P_REQ_GETDOCUMENTBASICDETAILSDOCUMENTNUMBER, new object[] { documentNumber, LOBentityDetailsCode });

                if (objRefCountingDataReader != null)
                {
                    var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;
                    if (sqlDr.Read())
                    {
                        var req = new Requisition
                        {
                            DocumentName = GetStringValue(sqlDr, ReqSqlConstants.COL_DOCUMENT_NAME),
                            DocumentCode = GetLongValue(sqlDr, ReqSqlConstants.COL_DOCUMENT_CODE),
                            DocumentStatusInfo = (DocumentStatus)GetIntValue(sqlDr, ReqSqlConstants.COL_DOCUMENT_STATUS),
                            DocumentNumber = GetStringValue(sqlDr, ReqSqlConstants.COL_DOCUMENT_NUMBER)
                        };

                        return req;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occurred in GetDocumentDetailByDocumentNumber method in SQLRequisitionDAO documentNumber :- " + documentNumber, ex);
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
                LogHelper.LogInfo(Log, "Requisition GetDocumentDetailByDocumentNumber Method Ended for documentNumber=" + documentNumber);
            }
        }

        public bool UpdateDocumentInterfaceStatus(long documentId, BusinessEntities.InterfaceStatus interfaceStatus, bool modifyAdditionalInfo, int sourceSystemId,int maxPrecisionValue = 0, int maxPrecisionValueForTaxesAndCharges = 0, int maxPrecisionValueForTotal = 0, string DocType = "")
        {

            LogHelper.LogInfo(Log, "Requisition UpdateDocumentInterfaceStatus Method Started for documentId = " + documentId);
            SqlConnection objSqlCon = null;
            SqlTransaction objSqlTrans = null;
            try
            {
                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("In UpdateDocumentInterfaceStatus Method.",
                                            " SP: usp_P2P_REQ_UpdateInterfaceStatus, with parameters: documentId = " + documentId
                                             + ",interfaceStatus =" + interfaceStatus
                                             + ", sourceSystemId =" + sourceSystemId));

                objSqlCon = (SqlConnection)ContextSqlConn.CreateConnection();
                objSqlCon.Open();

                var result = Convert.ToBoolean(ContextSqlConn.ExecuteNonQuery(ReqSqlConstants.USP_P2P_REQ_UPDATEINTERFACESTATUS, documentId, interfaceStatus, sourceSystemId), CultureInfo.InvariantCulture);
                if (result && modifyAdditionalInfo)
                {
                    objSqlTrans = objSqlCon.BeginTransaction();
                    SQLDocumentDAO sqlDocumentDAO = new SQLDocumentDAO();
                    sqlDocumentDAO.SqlTransaction = objSqlTrans;
                    sqlDocumentDAO.ReliableSqlDatabase = ContextSqlConn;
                    sqlDocumentDAO.UserContext = UserContext;
                    sqlDocumentDAO.GepConfiguration = GepConfiguration;

                    SaveRequisitionAdditionalDetails(documentId, sqlDocumentDAO);
                    if (!ReferenceEquals(objSqlTrans, null) && !ReferenceEquals(objSqlCon, null) && objSqlCon.State == ConnectionState.Open)
                        objSqlTrans.Commit();
                }
                return result;
            }
            catch(Exception ex)
            {
                LogHelper.LogError(Log , "Error occurred in UpdateDocumentInterfaceStatus method in SQLRequisitionDAO documentcode :- " + documentId, ex);

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
                }
                throw;
            }
            finally
            {
                if (!ReferenceEquals(objSqlCon, null) && objSqlCon.State != ConnectionState.Closed)
                {
                    objSqlCon.Close();
                    objSqlCon.Dispose();
                }
                LogHelper.LogInfo(Log, "Requisition UpdateDocumentInterfaceStatus Method Ended for documentId=" + documentId);
            }
        }

        public bool UpdateDocumentInterfaceStatusForRequisition(long documentId, BusinessEntities.InterfaceStatus interfaceStatus, int sourceSystemId, string errorDescription = "")
        {

            LogHelper.LogInfo(Log, "Requisition UpdateDocumentInterfaceStatusForRequisition Method Started for documentId = " + documentId);
            SqlConnection objSqlCon = null;
            SqlTransaction objSqlTrans = null;
            try
            {
                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("In UpdateDocumentInterfaceStatusForRequisition Method.",
                                            " SP: usp_P2P_REQ_UpdateInterfaceStatus, with parameters: documentId = " + documentId
                                             + ",interfaceStatus =" + interfaceStatus
                                             + ", sourceSystemId =" + sourceSystemId));

                objSqlCon = (SqlConnection)ContextSqlConn.CreateConnection();
                objSqlCon.Open();
                var result = Convert.ToBoolean(ContextSqlConn.ExecuteNonQuery(ReqSqlConstants.USP_P2P_REQ_UPDATEINTERFACESTATUS, documentId, interfaceStatus, sourceSystemId, errorDescription), CultureInfo.InvariantCulture);
                 AddIntoSearchIndexerQueueing(documentId, (int)DocumentType.Requisition, UserContext, GepConfiguration);
                return result;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occurred in UpdateDocumentInterfaceStatusForRequisition method in SQLRequisitionDAO documentcode :- " + documentId, ex);

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
                }
                throw;
            }
            finally
            {
                if (!ReferenceEquals(objSqlCon, null) && objSqlCon.State != ConnectionState.Closed)
                {
                    objSqlCon.Close();
                    objSqlCon.Dispose();
                }
                LogHelper.LogInfo(Log, "Requisition UpdateDocumentInterfaceStatusForRequisition Method Ended for documentId=" + documentId);
            }
        }
        public bool SaveDefaultAccountingDetails(long requisitionId, List<DocumentSplitItemEntity> requisitionSplitItemEntities, int precessionValue, int maxPrecessionforTotal, int maxPrecessionForTaxesAndCharges, bool saveDefaultGL = false)
        {

            bool flag = false;
            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;
            try
            {
                LogHelper.LogInfo(Log, "Requisition SaveDefaltAccountingSplit Method Started for requisitionId = " + requisitionId);

                var sqlHelper = ContextSqlConn;
                _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();

                DataTable dtReqItemEntities = null;
                dtReqItemEntities = P2P.DataAccessObjects.DAOHelper.ConvertToDataTable(requisitionSplitItemEntities,
                                                       GetRequisitionSplitItemEntitiesTable);
                using (var objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_SAVEREQUISITION_DEFAULT_ACCOUNTINGDETAILS, _sqlCon))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.CommandTimeout = 150;
                    objSqlCommand.Parameters.AddWithValue("@RequisitionId", requisitionId);
                    objSqlCommand.Parameters.Add(new SqlParameter("@tvp_P2P_SplitItemsEntities", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_P2P_SPLITITEMSENTITIES,
                        Value = dtReqItemEntities
                    });
                    objSqlCommand.Parameters.AddWithValue("@UserId", UserContext.ContactCode);
                    objSqlCommand.Parameters.AddWithValue("@SaveDefaultGL", saveDefaultGL);
                    objSqlCommand.Parameters.AddWithValue("@precessionValue", precessionValue);
                    objSqlCommand.Parameters.AddWithValue("@MaxPrecissionTotal", maxPrecessionforTotal);
                    objSqlCommand.Parameters.AddWithValue("@MaxPrecessionValueForTaxAndCharges", maxPrecessionForTaxesAndCharges);


                    flag = Convert.ToBoolean(ContextSqlConn.ExecuteScalar(objSqlCommand), CultureInfo.InvariantCulture);
                }
                if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
                    _sqlTrans.Commit();

            }
            catch(Exception ex)
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
                LogHelper.LogInfo(Log, "Requisition SaveDefaltAccountingSplit Method Ended for requisitionId = " + requisitionId);
            }

            return flag;
        }

        public bool UpdateSendforBiddingDocumentStatus(long documentCode)
        {
            bool result = false;
            SQLDocumentDAO sqlDocumentDAO = new SQLDocumentDAO();

            sqlDocumentDAO.ReliableSqlDatabase = ContextSqlConn;
            sqlDocumentDAO.UserContext = UserContext;
            sqlDocumentDAO.GepConfiguration = GepConfiguration;
            result = sqlDocumentDAO.UpdateDocumentStatus(documentCode, DocumentStatus.SentForBidding);
            return result;
        }

        public List<long> GetAllCategoriesByReqId(long documentId)
        {
            List<long> lstCategories = new List<long>();
            LogHelper.LogInfo(Log, "GetAllCategoriesByReqId Method Started.");
            SqlConnection objSqlCon = null;
            RefCountingDataReader objRefCountingDataReader = null;
            try
            {
                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("In GetAllCategoriesByReqId Method.",
                                            " SP: usp_P2P_REQ_GetAllCategoriesByReqId,  with parameters: documentCode = " + documentId));
                objSqlCon = (SqlConnection)ContextSqlConn.CreateConnection();
                objSqlCon.Open();
                objRefCountingDataReader =
                   (RefCountingDataReader)
                   ContextSqlConn.ExecuteReader(ReqSqlConstants.USP_P2P_REQ_GETALLCATEGORIESBYREQID,
                                      documentId);
                var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;

                while (sqlDr.Read())
                {
                    lstCategories.Add(GetLongValue(sqlDr, ReqSqlConstants.COL_CATEGORY_ID));
                }


                return lstCategories;
            }
            catch
            {
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
                LogHelper.LogInfo(Log, "GetAllCategoriesByReqId Method Ended.");
            }
        }

        public DataSet GetAllEntitiesByReqId(long documentId, int entityTypeId)
        {
            DataSet lstReqEntities = new DataSet();
            LogHelper.LogInfo(Log, "GetAllEntitiesByReqId Method Started.");
            SqlConnection objSqlCon = null;
            try
            {
                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("In GetAllEntitiesByReqId Method.",
                                            " SP: usp_P2P_REQ_GetRequisitionEntities,  with parameters: reqId = " + documentId
                                            + ",entityTypeId =" + entityTypeId));
                objSqlCon = (SqlConnection)ContextSqlConn.CreateConnection();
                objSqlCon.Open();
                lstReqEntities =
                    ContextSqlConn.ExecuteDataSet(ReqSqlConstants.USP_P2P_REQ_GETREQUISITION_ENTITIES,
                                       documentId, entityTypeId);

                return lstReqEntities;
            }
            catch
            {
                throw;
            }
            finally
            {
                if (!ReferenceEquals(objSqlCon, null) && objSqlCon.State != ConnectionState.Closed)
                {
                    objSqlCon.Close();
                    objSqlCon.Dispose();
                }
                LogHelper.LogInfo(Log, "GetAllEntitiesByReqId Method Ended.");
            }
        }

        public DataTable CalculateLineItemTax(long lineItemId, long entityId, int entityTypeId, int orgEntityId, long splitId, decimal totalAmount, bool? taxExempt = null, int MaxPrecessionValueForTaxAndCharges = 0)
        {
            var objCalculatedTaxTbl = new DataTable();

            try
            {
                LogHelper.LogInfo(Log, "In P2PDocumentDAO CalculateLineItemTax Method Started.");

                if (Log.IsDebugEnabled)
                    Log.Debug(
                        string.Concat(
                            "In CalculateLineItemTax Method in P2PDocumentDAO with parameter: lineItemId= " + lineItemId +
                                            ",entityId= " + entityId + ",entityTypeId= " + entityTypeId + ",entityTypeId= " +
                            entityTypeId +
                            ", orgEntityId= " + orgEntityId, CultureInfo.InvariantCulture));

                if (lineItemId > 0)// && entityId > 0 && entityTypeId > 0 && orgEntityId > 0)
                {
                    var objDataset = ContextSqlConn.ExecuteDataSet(ReqSqlConstants.USP_P2P_REQ_CALCULATELINEITEMTAX,
                                                                       DocumentType.Requisition, lineItemId, entityId, entityTypeId, orgEntityId, splitId, totalAmount, taxExempt, MaxPrecessionValueForTaxAndCharges);

                    if (objDataset != null && objDataset.Tables.Count > 0)
                    {
                        objCalculatedTaxTbl = objDataset.Tables[0];
                    }
                }
            }
            finally
            {
                LogHelper.LogInfo(Log, "P2PDocumentDAO CalculateLineItemTax Method completed.");
            }

            return objCalculatedTaxTbl;

        }

        public bool UpdateTaxOnHeaderShipTo(long requisitionId, long orgEntityId, int precessionValue, int maxPrecessionValueTotal, int maxPrecessionValueForTaxAndCharges, bool callFromCatalog = false, string requisitionIds = "")
        {
            bool flag = false;
            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;
            try
            {
                LogHelper.LogInfo(Log, "Requisition UpdateTaxOnHeaderShipTo Method Started for requisitionId = " + requisitionId);

                var sqlHelper = ContextSqlConn;
                _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();

                using (var objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_UPDATETAXONHEADERSHIPTO, _sqlCon))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.CommandTimeout = 150;
                    objSqlCommand.Parameters.AddWithValue("@requisitionId", requisitionId);
                    objSqlCommand.Parameters.AddWithValue("@orgEntityId", orgEntityId);
                    objSqlCommand.Parameters.AddWithValue("@userId", UserContext.ContactCode);
                    objSqlCommand.Parameters.AddWithValue("@precessionValue", precessionValue);
                    objSqlCommand.Parameters.AddWithValue("@maxPrecessionValueTotal", maxPrecessionValueTotal);
                    objSqlCommand.Parameters.AddWithValue("@maxPrecessionValueForTaxAndCharges", maxPrecessionValueForTaxAndCharges);
                    objSqlCommand.Parameters.AddWithValue("@callFromCatalog", callFromCatalog);
                    objSqlCommand.Parameters.AddWithValue("@requisitionIds", requisitionIds);
                    flag = Convert.ToBoolean(ContextSqlConn.ExecuteScalar(objSqlCommand, _sqlTrans), CultureInfo.InvariantCulture);
                }

                SQLDocumentDAO sqlDocumentDAO = new SQLDocumentDAO();
                sqlDocumentDAO.SqlTransaction = _sqlTrans;
                sqlDocumentDAO.ReliableSqlDatabase = sqlHelper;
                sqlDocumentDAO.UserContext = UserContext;
                sqlDocumentDAO.GepConfiguration = GepConfiguration;

                SaveRequisitionAdditionalDetails(requisitionId, sqlDocumentDAO);

                if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
                    _sqlTrans.Commit();

            }
            catch
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
                throw;
            }
            finally
            {
                if (!ReferenceEquals(_sqlCon, null) && _sqlCon.State != ConnectionState.Closed)
                {
                    _sqlCon.Close();
                    _sqlCon.Dispose();
                }
                LogHelper.LogInfo(Log, "Requisition UpdateTaxOnHeaderShipTo Method Ended for requisitionId = " + requisitionId);
            }

            return flag;
        }

        public bool ProrateLineItemTax(long LineItemId, int OrgEntityType, int precessionValue, int maxPrecessionforTotal, int maxPrecessionForTaxesAndCharges)
        {
            bool flag = false;
            long requistionId = 0;
            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;
            try
            {
                LogHelper.LogInfo(Log, "Requisition ProrateLineItemTax Method Started for requisitionItemId = " + LineItemId);

                var sqlHelper = ContextSqlConn;
                _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();
                SQLDocumentDAO sqlDocumentDAO = new SQLDocumentDAO();
                sqlDocumentDAO.SqlTransaction = _sqlTrans;
                sqlDocumentDAO.ReliableSqlDatabase = sqlHelper;
                sqlDocumentDAO.UserContext = UserContext;
                sqlDocumentDAO.GepConfiguration = GepConfiguration;

                requistionId = Convert.ToInt64(sqlHelper.ExecuteScalar(_sqlTrans, ReqSqlConstants.USP_P2P_REQ_PRORATE_LINEITEM_TAX, LineItemId, OrgEntityType, UserContext.ContactCode, precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges), NumberFormatInfo.InvariantInfo);

                if (requistionId > 0)
                {
                    SaveRequisitionAdditionalDetails(requistionId, sqlDocumentDAO);
                    flag = true;
                }

                if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
                    _sqlTrans.Commit();

            }
            catch
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
                throw;
            }
            finally
            {
                if (!ReferenceEquals(_sqlCon, null) && _sqlCon.State != ConnectionState.Closed)
                {
                    _sqlCon.Close();
                    _sqlCon.Dispose();
                }
                LogHelper.LogInfo(Log, "Requisition ProrateLineItemTax Method Ended for requisitionItemId = " + LineItemId);
            }

            return flag;
        }

        public bool ProrateShippingAndFreight(long requisitionId, decimal? shippingCharges, int precessionValue, int maxPrecessionforTotal, int maxPrecessionForTaxesAndCharges, bool isRestrictChangeRequisition = false)
        {
            bool flag = false;
            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;
            try
            {
                LogHelper.LogInfo(Log, "Requisition ProrateShippingAndFreight Method Started for requisitionId = " + requisitionId);

                var sqlHelper = ContextSqlConn;
                _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();

                SQLDocumentDAO sqlDocumentDAO = new SQLDocumentDAO();
                sqlDocumentDAO.SqlTransaction = _sqlTrans;
                sqlDocumentDAO.ReliableSqlDatabase = sqlHelper;
                sqlDocumentDAO.UserContext = UserContext;
                sqlDocumentDAO.GepConfiguration = GepConfiguration;

                flag = Convert.ToBoolean(sqlHelper.ExecuteScalar(_sqlTrans, ReqSqlConstants.USP_P2P_REQ_PRORATESHIPPINGANDFREIGHT, requisitionId, shippingCharges, precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges, isRestrictChangeRequisition), NumberFormatInfo.InvariantInfo);
                SaveRequisitionAdditionalDetails(requisitionId, sqlDocumentDAO);
                if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
                    _sqlTrans.Commit();

            }
            catch
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
                throw;
            }
            finally
            {
                if (!ReferenceEquals(_sqlCon, null) && _sqlCon.State != ConnectionState.Closed)
                {
                    _sqlCon.Close();
                    _sqlCon.Dispose();
                }
                LogHelper.LogInfo(Log, "Requisition ProrateShippingAndFreight Method Ended for requisitionId = " + requisitionId);
            }

            return flag;
        }

        public bool SaveLineItemTaxes(long documentItemId, int precessionValue = 0, int maxPrecessionforTotal = 0, int maxPrecessionForTaxesAndCharges = 0)
        {
            SqlConnection objSqlCon = null;
            SqlTransaction objSqlTrans = null;
            try
            {
                LogHelper.LogInfo(Log, "Requisition SaveLineItemTaxes Method Started for documentItemId=" + documentItemId + ", precessionValue=" + precessionValue);
                var sqlHelper = ContextSqlConn;
                objSqlCon = (SqlConnection)sqlHelper.CreateConnection();
                objSqlCon.Open();
                objSqlTrans = objSqlCon.BeginTransaction();
                SQLDocumentDAO sqlDocumentDAO = new SQLDocumentDAO();
                sqlDocumentDAO.SqlTransaction = objSqlTrans;
                sqlDocumentDAO.ReliableSqlDatabase = sqlHelper;
                sqlDocumentDAO.UserContext = UserContext;
                sqlDocumentDAO.GepConfiguration = GepConfiguration;

                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("In Requisition SaveLineItemTaxes Method SP: usp_P2P_REQ_CalculateAndUpdateLineItemTax with parameter: documentItemId=" + documentItemId, " precessionValue=" + precessionValue, " was called."));
                var flag = false;
                var documentCode = Convert.ToInt64(ContextSqlConn.ExecuteScalar(objSqlTrans, ReqSqlConstants.USP_P2P_REQ_CALCULATE_AND_UPDATELINEITEMTAX, documentItemId, precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges), NumberFormatInfo.InvariantInfo);
                if (documentCode > 0)
                {
                    SaveRequisitionAdditionalDetails(documentCode, sqlDocumentDAO);
                    flag = true;
                }
                if (!ReferenceEquals(objSqlTrans, null) && !ReferenceEquals(objSqlCon, null) && objSqlCon.State == ConnectionState.Open)
                    objSqlTrans.Commit();

                return flag;
            }
            catch
            {
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
                }
                throw;
            }
            finally
            {
                if (!ReferenceEquals(objSqlCon, null) && objSqlCon.State != ConnectionState.Closed)
                {
                    objSqlCon.Close();
                    objSqlCon.Dispose();
                }
                LogHelper.LogInfo(Log, "Requisition SaveLineItemTaxes Method Ended for documentItemId=" + documentItemId + ", precessionValue=" + precessionValue);
            }
        }

        public bool DeleteAllSplitsByDocumentId(long documentId)
        {
            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;
            try
            {
                LogHelper.LogInfo(Log, "Requisition DeleteAllSplitsByDocumentId Method Started for DocumentItemId=" + documentId);

                _sqlCon = (SqlConnection)ContextSqlConn.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();

                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("Requisition DeleteAllSplitsByDocumentId sp usp_P2P_REQ_DeleteAllSplitsByDocumentId with parameter: objItems=" + documentId, " was called."));

                var result = Convert.ToBoolean(ContextSqlConn.ExecuteScalar(_sqlTrans, ReqSqlConstants.USP_P2P_REQ_DELETEALLSPLITSBYDOCUMENTID,
                                                                       documentId
                                                                      ), NumberFormatInfo.InvariantInfo);
                if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
                    _sqlTrans.Commit();
                return result;
            }
            catch
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
                throw;
            }
            finally
            {
                if (!ReferenceEquals(_sqlCon, null) && _sqlCon.State != ConnectionState.Closed)
                {
                    _sqlCon.Close();
                    _sqlCon.Dispose();
                }

                LogHelper.LogInfo(Log, "Requisition DeleteAllSplitsByDocumentId Method Ended for DocumentItemId=" + documentId);
            }
        }

        public DataSet GetBasicDetailsByIdForNotification(long documentCode, long buyerPartnerCode, long CommentType = 1)
        {
            try
            {
                LogHelper.LogInfo(Log, "GetBGetBasicDetailsByIdForNotificationasicDetailsById Method Started for id=" + documentCode + ", buyerPartnerCode=" + buyerPartnerCode);

                var sqlHelper = ContextSqlConn;
                if (documentCode > 0)
                {
                    if (Log.IsDebugEnabled)
                        Log.Debug(string.Concat("In GetBasicDetailsByIdForNotification Method sp USP_P2P_REQ_GETREQUISITIONBYIDFORNOTIFICATION with parameter: documentCode=" + documentCode, ", buyerPartnerCode=" + buyerPartnerCode));

                    var objDSRequisition = sqlHelper.ExecuteDataSet(ReqSqlConstants.USP_P2P_REQ_GETREQUISITIONBYIDFORNOTIFICATION,
                                                                      new object[] { documentCode, buyerPartnerCode, CommentType });

                    return objDSRequisition;
                }
                else
                {
                    if (Log.IsWarnEnabled)
                        Log.Warn("In GetBasicDetailsByIdForNotification Method Parameter: id is not greater than 0.");
                }
            }
            finally
            {

                LogHelper.LogInfo(Log, "GetBasicDetailsByIdForNotification Method Ended for id=" + documentCode);
            }

            return new DataSet();
        }

        public long SaveAccountingApplyToAll(long requisitionId, List<DocumentSplitItemEntity> requisitionSplitItemEntities, int precessionValue, int maxPrecessionValueforTotal, int maxPrecessionValueForTaxesAndCharges, string allowOrgEntityInCatalogItems, long expenseCodeEntityId, bool allowTaxCodewithAmount, string supplierStatusForValidation)
        {
            long documentId = 0;
            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;
            try
            {
                LogHelper.LogInfo(Log, "Requisition SaveRequisitionAccountingApplyToAll Method Started for RequisitionId = " + requisitionId);

                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("In SaveRequisitionAccountingApplyToAll Method.",
                                            " SP: usp_P2P_REQ_SaveRequisitionAccountingApplyToAll,  with parameters: requisitionId = " + requisitionId));

                var sqlHelper = ContextSqlConn;
                _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();

                DataTable dtReqItemEntities = null;
                dtReqItemEntities = P2P.DataAccessObjects.DAOHelper.ConvertToDataTable(requisitionSplitItemEntities,
                                       GetRequisitionSplitItemEntitiesTable);
                using (var objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_SAVEREQUISITIONACCOUNTINGAPPLYTOALL, _sqlCon))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.Add(new SqlParameter("@tvp_P2P_SplitItemsEntities", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_P2P_SPLITITEMSENTITIES,
                        Value = dtReqItemEntities
                    });
                    objSqlCommand.Parameters.AddWithValue("@userId", UserContext.ContactCode);
                    objSqlCommand.Parameters.AddWithValue("@requisitionId", requisitionId);
                    objSqlCommand.Parameters.AddWithValue("@allowOrgEntityInCatalogItems", allowOrgEntityInCatalogItems);
                    objSqlCommand.Parameters.AddWithValue("@expenseCodeEntityId", expenseCodeEntityId);
                    documentId = Convert.ToInt64(ContextSqlConn.ExecuteScalar(objSqlCommand), CultureInfo.InvariantCulture);
                }
                if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
                    _sqlTrans.Commit();
            }
            catch
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
                throw;
            }
            finally
            {
                if (_sqlCon != null && _sqlCon.State != ConnectionState.Closed)
                {
                    _sqlCon.Close();
                    _sqlCon.Dispose();
                }
                LogHelper.LogInfo(Log, "Requisition SaveRequisitionAccountingApplyToAll Method Ended for RequisitionId = " + requisitionId);
            }

            return documentId;
        }

        public void ProrateHeaderTaxAndShipping(Requisition objRequisition)
        {
            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;

            LogHelper.LogInfo(Log, "Order ProrateHeaderTaxAndShipping Method Started for Order ID =" + objRequisition.DocumentCode
                                                + " Tax " + Convert.ToDecimal(objRequisition.Tax)
                                                + " Shipping  " + Convert.ToDecimal(objRequisition.Shipping)
                                                + " Precision " + objRequisition.Precision);
            if (Log.IsDebugEnabled)
                Log.Debug(string.Format(CultureInfo.InvariantCulture, "Order ProrateHeaderTaxAndShipping sp usp_P2P_PO_ProrateHeaderTaxAndShipping with parameter: documentId={0} was called."
                                                                      , objRequisition.DocumentCode));
            try
            {

                var sqlHelper = ContextSqlConn;
                _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();

                sqlHelper.ExecuteNonQuery(ReqSqlConstants.USP_P2P_REQ_PRORATEHEADERTAXANDSHIPPING,
                                                                        new object[] { objRequisition.DocumentCode,
                                                                                    objRequisition.Precision ,
                                                                                    objRequisition.Tax,
                                                                                    objRequisition.Shipping,
                                                                                    objRequisition.AdditionalCharges
                                                                                    });

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
                LogHelper.LogInfo(Log, "Error Occurred in ProrateHeaderTaxAndShipping Method ended for Order ID =" + objRequisition.DocumentCode
                                       + " Tax " + Convert.ToDecimal(objRequisition.Tax)
                                       + " Shipping  " + Convert.ToDecimal(objRequisition.Shipping)
                                       + " Precision " + objRequisition.Precision);
                throw ex;
            }
            finally
            {
                if (_sqlCon != null && _sqlCon.State != ConnectionState.Closed)
                {
                    _sqlCon.Close();
                    _sqlCon.Dispose();
                }

                LogHelper.LogInfo(Log, "Order ProrateHeaderTaxAndShipping Method ended for Order ID =" + objRequisition.DocumentCode
                                        + " Tax " + Convert.ToDecimal(objRequisition.Tax)
                                        + " Shipping  " + Convert.ToDecimal(objRequisition.Shipping)
                                        + " Precision " + objRequisition.Precision);
            }
        }

        public List<DocumentAdditionalEntityInfo> GetAllDocumentAdditionalEntityInfo(long documentCode)
        {
            LogHelper.LogInfo(Log, "Requisition GetAllDocumentAdditionalEntityInfo Method Started");
            var lstDocumentAdditionalEntityInfo = new Collection<DocumentAdditionalEntityInfo>();
            RefCountingDataReader objRefCountingDataReader = null;
            try
            {
                var sqlHelper = ContextSqlConn;

                objRefCountingDataReader = (RefCountingDataReader)sqlHelper.ExecuteReader(ReqSqlConstants.USP_P2P_REQ_GETREQUISITIONENTITYDETAILSBYID, new object[] { documentCode });
                if (objRefCountingDataReader != null)
                {
                    var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;
                    while (sqlDr.Read())
                    {
                        lstDocumentAdditionalEntityInfo.Add(new DocumentAdditionalEntityInfo
                        {
                            EntityDetailCode = GetLongValue(sqlDr, ReqSqlConstants.COL_SPLIT_ITEM_ENTITYDETAILCODE),
                            EntityId = GetIntValue(sqlDr, ReqSqlConstants.COL_ENTITY_ID),
                            EntityDisplayName = GetStringValue(sqlDr, ReqSqlConstants.COL_ENTITY_DISPLAY_NAME),
                            IsAccountingEntity = GetBoolValue(sqlDr, ReqSqlConstants.COL_IS_ACCOUNTING_ENTITY)
                        });
                    }
                }
            }
            finally
            {
                if (!ReferenceEquals(objRefCountingDataReader, null) && !objRefCountingDataReader.IsClosed)
                {
                    objRefCountingDataReader.Close();
                    objRefCountingDataReader.Dispose();
                }
                LogHelper.LogInfo(Log, "GetAllDocumentAdditionalEntityInfo Method Ended for id=" + documentCode);
            }
            return lstDocumentAdditionalEntityInfo.ToList();
        }

        /// <summary>
        /// Makes copies of passed line item.
        /// </summary>
        /// <param name="requisitionItemId">Requisition item id</param>
        /// <param name="requisitionId">Requisition id</param>
        /// <param name="txtNumberOfCopies">Number of copies to be made</param>
        /// <returns>True if success</returns>
        public bool copyLineItem(long requisitionItemId, long requisitionId, int txtNumberOfCopies, int MaxPrecessionValue = 0, int MaxPrecessionValueTotal = 0, int MaxPrecessionValueForTaxAndCharges = 0)
        {
            bool result = false;
            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;
            try
            {
                LogHelper.LogInfo(Log, String.Format("Error Occurred in copyLineItem Method Started for RequisitionitemID ={0},  requisitionId ={1}, txtNumberOfCopies ={2}", requisitionItemId, requisitionId, txtNumberOfCopies));

                var sqlHelper = ContextSqlConn;
                _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();

                result = Convert.ToBoolean(sqlHelper.ExecuteScalar(ReqSqlConstants.USP_P2P_REQ_COPYLINEITEM,
                                                                  new object[] { requisitionItemId, requisitionId, txtNumberOfCopies, MaxPrecessionValue, MaxPrecessionValueTotal, MaxPrecessionValueForTaxAndCharges }), NumberFormatInfo.InvariantInfo);

                #region Save Document Additional Info

                if (requisitionItemId > 0 && requisitionId > 0)
                {
                    SQLDocumentDAO sqlDocumentDAO = new SQLDocumentDAO();
                    sqlDocumentDAO.SqlTransaction = _sqlTrans;
                    sqlDocumentDAO.ReliableSqlDatabase = sqlHelper;
                    sqlDocumentDAO.UserContext = UserContext;
                    sqlDocumentDAO.GepConfiguration = GepConfiguration;

                    SaveRequisitionAdditionalDetails(requisitionId, sqlDocumentDAO);
                }

                #endregion
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
                LogHelper.LogInfo(Log, String.Format("Error Occurred in copyLineItem Method ended for RequisitionitemID ={0},  requisitionId ={1}, txtNumberOfCopies ={2}", requisitionItemId, requisitionId, txtNumberOfCopies));
                throw ex;
            }
            finally
            {
                if (!ReferenceEquals(_sqlCon, null) && _sqlCon.State != ConnectionState.Closed)
                {
                    _sqlCon.Close();
                    _sqlCon.Dispose();
                }
                LogHelper.LogInfo(Log, String.Format("Error Occurred in copyLineItem Method ended for RequisitionitemID ={0},  requisitionId ={1}, txtNumberOfCopies ={2}", requisitionItemId, requisitionId, txtNumberOfCopies));
            }

            return result;

        }

        public bool UpdateReqItemOnPartnerChange(long requisitionItemId, long partnerCode, int precessionValue, int maxPrecessionforTotal, int maxPrecessionForTaxesAndCharges)
        {
            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;
            long documentCode = 0;
            LogHelper.LogInfo(Log, String.Format("Requisition UpdateReqItemOnPartnerChange Method Started for RequisitionItemId ={0}, partnerCode={1}", requisitionItemId, partnerCode));
            if (Log.IsDebugEnabled)
                Log.Debug(string.Format(CultureInfo.InvariantCulture, "Requisition UpdateReqItemOnPartnerChange sp usp_P2P_REQ_UpdateReqItemOnPartnerChange with parameter: requisitionItemId={0}, partnerCode={1}  was called."
                                                                      , requisitionItemId, partnerCode));
            try
            {

                var sqlHelper = ContextSqlConn;
                _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();

                documentCode = Convert.ToInt64(sqlHelper.ExecuteScalar(_sqlTrans, ReqSqlConstants.USP_P2P_REQ_UPDATEREQITEMONPARTNERCHANGE,
                                                                        new object[] { requisitionItemId, partnerCode, precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges }));
                #region Save Document Additional Info
                if (requisitionItemId > 0 && documentCode > 0)
                {
                    SQLDocumentDAO sqlDocumentDAO = new SQLDocumentDAO();
                    sqlDocumentDAO.SqlTransaction = _sqlTrans;
                    sqlDocumentDAO.ReliableSqlDatabase = sqlHelper;
                    sqlDocumentDAO.UserContext = UserContext;
                    sqlDocumentDAO.GepConfiguration = GepConfiguration;
                    SaveRequisitionAdditionalDetails(documentCode, sqlDocumentDAO);
                }
                #endregion

                if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
                    _sqlTrans.Commit();
                return true;
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

                LogHelper.LogInfo(Log, String.Format("Error Occurred in UpdateReqItemOnPartnerChange Method ended for requisitionItemId ={1}" + requisitionItemId
                                       + " partnerCode " + partnerCode));
                throw ex;
            }
            finally
            {
                if (_sqlCon != null && _sqlCon.State != ConnectionState.Closed)
                {
                    _sqlCon.Close();
                    _sqlCon.Dispose();
                }

                LogHelper.LogInfo(Log, String.Format("Requisition UpdateReqItemOnPartnerChange Method ended for requisitionItemId ={0}, partnerCode ={1}",
                    requisitionItemId, partnerCode));
            }
        }

        public bool GetRequisitionItemAccountingStatus(long reqItemId)
        {
            var result = false;

            try
            {
                LogHelper.LogInfo(Log, "Requisition GetRequisitionItemAccountingStatus Method Started for reqItemId=" + reqItemId);
                if (reqItemId > 0)
                {
                    if (Log.IsDebugEnabled)
                        Log.Debug(string.Format(CultureInfo.InvariantCulture, "Requisition GetRequisitionItemAccountingStatus sp usp_P2P_REQ_GetRequisitionItemAccountingStatus with parameter: reqItemId={0} was called.", reqItemId));


                    result = Convert.ToBoolean(
                    ContextSqlConn.ExecuteScalar(ReqSqlConstants.USP_P2P_REQ_GETREQUISITIONITEMACCOUNTINGSTATUS, reqItemId));
                }
                else
                {
                    if (Log.IsWarnEnabled)
                        Log.Warn("In Requisition GetRequisitionItemAccountingStatus method reqItemId parameter is less than or equal to 0.");
                }
            }
            finally
            {
                LogHelper.LogInfo(Log, "Requisition GetRequisitionItemAccountingStatus Method Ended for reqItemId=" + reqItemId);
            }
            return result;
        }

        public List<PartnerInfo> GetPreferredPartnerByReqItemId(long DocumentItemId, int pageIndex, int pageSize, string partnerName, out long partnerCode)
        {
            RefCountingDataReader objRefCountingDataReader = null;
            partnerCode = 0;
            List<PartnerInfo> lstPartnerInfo = new List<PartnerInfo>();
            PartnerInfo objPartnerInfo = null;
            try
            {
                LogHelper.LogInfo(Log, String.Format("GetValidationDetailsById Method Started for DocumentItemId={0}, pageIndex={1}, pageSize={2}, partnerName={3}"
                , DocumentItemId, pageIndex, pageSize, partnerName));

                var sqlHelper = ContextSqlConn;
                objRefCountingDataReader = (RefCountingDataReader)sqlHelper.ExecuteReader(ReqSqlConstants.USP_P2P_REQ_GETPREFERREDPARTNERBYREQITEMID,
                    new object[] { DocumentItemId, partnerName, pageIndex, pageSize, UserContext.ContactCode });
                if (objRefCountingDataReader != null)
                {
                    var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;
                    while (sqlDr.Read())
                    {
                        if (GetBoolValue(sqlDr, ReqSqlConstants.COL_ISDEFAULT))
                        {
                            partnerCode = GetLongValue(sqlDr, ReqSqlConstants.COL_PARTNER_CODE);

                        }
                        objPartnerInfo = new PartnerInfo()
                        {
                            PartnerCode = GetLongValue(sqlDr, ReqSqlConstants.COL_PARTNER_CODE),
                            PartnerName = GetStringValue(sqlDr, ReqSqlConstants.COL_LEGALCOMPANYNAME),
                            ClientPartnerCode = GetStringValue(sqlDr, ReqSqlConstants.COL_CLIENT_PARTNERCODE)
                        };
                        lstPartnerInfo.Add(objPartnerInfo);
                    }
                }
            }
            finally
            {
                if (!ReferenceEquals(objRefCountingDataReader, null) && !objRefCountingDataReader.IsClosed)
                {
                    objRefCountingDataReader.Close();
                    objRefCountingDataReader.Dispose();
                }

                LogHelper.LogInfo(Log, String.Format("GetValidationDetailsById Method Started for DocumentItemId={0}, pageIndex={1}, pageSize={2}, partnerName={3}"
                , DocumentItemId, pageIndex, pageSize, partnerName));
            }
            return lstPartnerInfo;
        }

        public List<ItemPartnerInfo> GetPreferredPartnerByCatalogItemId(long DocumentItemId, int pageIndex, int pageSize, string partnerName, string currencyCode, long entityDetailCode, out long partnerCode, string buList = "")
        {
            RefCountingDataReader objRefCountingDataReader = null;
            partnerCode = 0;
            List<ItemPartnerInfo> lstPartnerInfo = new List<ItemPartnerInfo>();
            ItemPartnerInfo objPartnerInfo = null;
            try
            {
                LogHelper.LogInfo(Log, String.Format("GetValidationDetailsById Method Started for DocumentItemId={0}, pageIndex={1}, pageSize={2}, partnerName={3}, buList={4}"
                , DocumentItemId, pageIndex, pageSize, partnerName, buList));

                var sqlHelper = ContextSqlConn;
                objRefCountingDataReader = (RefCountingDataReader)sqlHelper.ExecuteReader(ReqSqlConstants.USP_P2P_REQ_GETPREFERREDPARTNERBYCATALOGITEMID,
                    new object[] { DocumentItemId, partnerName, pageIndex, pageSize, currencyCode, entityDetailCode, buList });
                if (objRefCountingDataReader != null)
                {
                    var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;
                    while (sqlDr.Read())
                    {
                        if (GetBoolValue(sqlDr, ReqSqlConstants.COL_ISDEFAULT))
                        {
                            partnerCode = GetLongValue(sqlDr, ReqSqlConstants.COL_PARTNER_CODE);

                        }
                        objPartnerInfo = new ItemPartnerInfo()
                        {
                            PartnerCode = GetLongValue(sqlDr, ReqSqlConstants.COL_PARTNER_CODE),
                            PartnerName = GetStringValue(sqlDr, ReqSqlConstants.COL_LEGALCOMPANYNAME),
                            ClientPartnerCode = GetStringValue(sqlDr, ReqSqlConstants.COL_CLIENT_PARTNERCODE),
                            UnitPrice = (!string.IsNullOrWhiteSpace(sqlDr[ReqSqlConstants.COL_UNIT_PRICE].ToString())) ? (Convert.ToDecimal(sqlDr[ReqSqlConstants.COL_UNIT_PRICE])) : default(decimal?),
                            UOM = GetStringValue(sqlDr, ReqSqlConstants.COL_UOM),
                            IsTaxExempt = Convert.ToBoolean(sqlDr[ReqSqlConstants.COL_ISTAXEXEMPT], CultureInfo.InvariantCulture),
                            ExtContractRef = GetStringValue(sqlDr, ReqSqlConstants.COL_EXTCONTRACTREF),
                            UOMName = GetStringValue(sqlDr, ReqSqlConstants.COL_UOM_DESCRIPTION)
                        };
                        lstPartnerInfo.Add(objPartnerInfo);
                    }
                }
            }
            finally
            {
                if (!ReferenceEquals(objRefCountingDataReader, null) && !objRefCountingDataReader.IsClosed)
                {
                    objRefCountingDataReader.Close();
                    objRefCountingDataReader.Dispose();
                }

                LogHelper.LogInfo(Log, String.Format("GetValidationDetailsById Method Started for DocumentItemId={0}, pageIndex={1}, pageSize={2}, partnerName={3}, buList={4}"
                , DocumentItemId, pageIndex, pageSize, partnerName, buList));
            }
            return lstPartnerInfo;
        }

        /// <summary>
        /// Get all the line item ids for a given documentcode
        /// </summary>
        /// <param name="documentCode">document code</param>
        /// <returns>Returns the list of itemids in a table format.</returns>
        public DataTable GetAllItemsByDocumentCode(long documentCode)
        {
            var objItemsTbl = new DataTable();

            try
            {
                LogHelper.LogInfo(Log, "In RequisitionDAO GetAllItemsByDocumentCode Method Started.");

                if (Log.IsDebugEnabled)
                    Log.Debug(string.Format(CultureInfo.InvariantCulture, "GetAllItemsByDocumentCode sp usp_P2P_REQ_GetAllItemIdsByReqId with parameter: documentCode={0} was called.", documentCode));

                var objDataset = ContextSqlConn.ExecuteDataSet(ReqSqlConstants.USP_P2P_REQ_GETALLITEMIDSBYREQID, documentCode);

                if (objDataset != null && objDataset.Tables.Count > 0)
                {
                    objItemsTbl = objDataset.Tables[0];
                }
            }
            finally
            {
                LogHelper.LogInfo(Log, "RequisitionDAO GetAllItemsByDocumentCode Method completed.");
            }
            return objItemsTbl;
        }

        /// <summary>
        /// Checking requisition has catalogitems or not
        /// </summary>
        /// <param name="newrequisitionId">new requisitionIds</param>
        /// <param name="requisitionIds">old requisitionids</param>
        /// <returns>true or false</returns>
        public bool CheckRequisitionCatalogItemAccess(long newrequisitionId, string requisitionIds)
        {
            var status = false;
            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;
            try
            {
                LogHelper.LogInfo(Log, "Requisition CheckRequisitionCatalogItemAccess Method Started for Copy requisitionIds=" + requisitionIds + " to requisitionId" + newrequisitionId);

                var sqlHelper = ContextSqlConn;
                _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();

                if (newrequisitionId > 0)
                {
                    if (Log.IsDebugEnabled)
                        Log.Debug(string.Format(CultureInfo.InvariantCulture, "Requisition CheckRequisitionCatalogItemAccess sp CheckRequisitionCatalogItemAccess with parameter: newrequisitionId={0} and requisitionIds = {1}  was called.", newrequisitionId, requisitionIds));

                    if (ContextSqlConn.ExecuteScalar(_sqlTrans, ReqSqlConstants.USP_P2P_REQ_REQUISITIONCATALOGITEMACCESS, newrequisitionId, requisitionIds, UserContext.ContactCode).ToString() == "1")
                    {
                        status = true;
                    }
                    if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
                        _sqlTrans.Commit();
                }
                else
                {
                    if (Log.IsWarnEnabled)
                        Log.Warn("In Requisition CheckRequisitionCatalogItemAccess method requisitionId parameter is less than or equal to 0.");
                }
            }
            catch
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
                throw;
            }
            finally
            {
                if (!ReferenceEquals(_sqlCon, null) && _sqlCon.State != ConnectionState.Closed)
                {
                    _sqlCon.Close();
                    _sqlCon.Dispose();
                }
                LogHelper.LogInfo(Log, "Requisition CheckRequisitionCatalogItemAccess Method Ended for Copy requisitionId=" + requisitionIds + " to requisitionId" + newrequisitionId);
            }
            return status;
        }

        public Document GetDocumentDetailsByDocumentId(Document objDocument, bool byPassAccessRights = false, bool isFunctionalAdmin = false)
        {
            throw new NotImplementedException();
        }
        public bool CheckOBOUserCatalogItemAccess(long requisitionId, long requesterId, int maxPrecessionValue, int maxPrecessionValueTotal, int maxPrecessionValueForTaxAndCharges, bool delItems = false)
        {
            var status = false;
            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;
            try
            {
                LogHelper.LogInfo(Log, "Requisition CheckOBOUserCatalogItemAccess Method Started ");

                var sqlHelper = ContextSqlConn;
                _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();

                if (requisitionId > 0)
                {
                    if (Log.IsDebugEnabled)
                        Log.Debug(string.Format(CultureInfo.InvariantCulture, "Requisition CheckOBOUserCatalogItemAccess sp CheckRequisitionCatalogItemAccess with parameter: newrequisitionId={0} and requesterId = {1}  was called.", requisitionId, requesterId));

                    if (ContextSqlConn.ExecuteScalar(_sqlTrans, ReqSqlConstants.USP_P2P_REQ_REQUISITIONOBOUSERCATALOGITEMACCESS, requisitionId, requesterId, delItems, maxPrecessionValue, maxPrecessionValueTotal, maxPrecessionValueForTaxAndCharges).ToString() == "1")
                    {
                        status = true;
                    }
                    if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
                        _sqlTrans.Commit();
                }
                else
                {
                    if (Log.IsWarnEnabled)
                        Log.Warn("In Requisition CheckOBOUserCatalogItemAccess method requisitionId parameter is less than or equal to 0.");
                }
            }
            catch
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
                throw;
            }
            finally
            {
                if (!ReferenceEquals(_sqlCon, null) && _sqlCon.State != ConnectionState.Closed)
                {
                    _sqlCon.Close();
                    _sqlCon.Dispose();
                }
                LogHelper.LogInfo(Log, "Requisition CheckOBOUserCatalogItemAccess Method Ended ");
            }
            return status;
        }

        /// <summary>
        /// To add and delete taxes on requisition
        /// </summary>
        /// <param name="requisitionItemId"></param>
        /// <param name="lstTaxes"></param>
        /// <returns></returns>
        public bool UpdateTaxOnLineItem(long requisitionItemId, ICollection<Taxes> lstTaxes, int precessionValue, int maxPrecessionValueForTotal = 0, int maxPrecessionValueForTaxAndCharges = 0)
        {
            var result = false;
            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;
            try
            {
                LogHelper.LogInfo(Log, "Requisition UpdateTaxOnLineItem Method Started ");

                var sqlHelper = ContextSqlConn;
                _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();

                if (requisitionItemId > 0)
                {
                    if (Log.IsDebugEnabled)
                        Log.Debug(string.Format(CultureInfo.InvariantCulture, "Requisition UpdateTaxOnLineItem sp usp_P2P_REQ_UpdateTaxOnLineItem with parameter: requisitionItemId={0},precessionValue={1} was called.",
                            requisitionItemId, precessionValue));

                    DataTable dtTaxes = null;
                    dtTaxes = P2P.DataAccessObjects.DAOHelper.ConvertToDataTable(lstTaxes, P2P.DataAccessObjects.SQLServer.SQLCommonDAO.GetTaxes);
                    long documentCode = 0;
                    using (var objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_UPDATETAXONLINEITEM, _sqlCon))
                    {
                        objSqlCommand.CommandType = CommandType.StoredProcedure;
                        objSqlCommand.Parameters.AddWithValue("@LineItemId", requisitionItemId);
                        objSqlCommand.Parameters.Add(new SqlParameter("@tvp_PTL_TaxMaster", SqlDbType.Structured)
                        {
                            TypeName = ReqSqlConstants.TVP_PTL_TAXMASTER,
                            Value = dtTaxes
                        });
                        objSqlCommand.Parameters.AddWithValue("@UserId", UserContext.ContactCode);
                        objSqlCommand.Parameters.AddWithValue("@precessionValue", precessionValue);
                        objSqlCommand.Parameters.AddWithValue("@precisiontotal", maxPrecessionValueForTotal);
                        objSqlCommand.Parameters.AddWithValue("@MaxPrecessionValueForTaxAndCharges", maxPrecessionValueForTaxAndCharges);
                        documentCode = Convert.ToInt64(ContextSqlConn.ExecuteScalar(objSqlCommand, _sqlTrans), CultureInfo.InvariantCulture);
                    }

                    #region Save Document Additional Info
                    if (requisitionItemId > 0 && documentCode > 0)
                    {
                        result = true;
                        SQLDocumentDAO sqlDocumentDAO = new SQLDocumentDAO();
                        sqlDocumentDAO.SqlTransaction = _sqlTrans;
                        sqlDocumentDAO.ReliableSqlDatabase = sqlHelper;
                        sqlDocumentDAO.UserContext = UserContext;
                        sqlDocumentDAO.GepConfiguration = GepConfiguration;
                        SaveRequisitionAdditionalDetails(documentCode, sqlDocumentDAO);
                    }
                    #endregion
                    if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
                        _sqlTrans.Commit();
                }
                else
                {
                    if (Log.IsWarnEnabled)
                        Log.Warn("In Requisition UpdateTaxOnLineItem method requisitionId parameter is less than or equal to 0.");
                }
            }
            catch
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
                throw;
            }
            finally
            {
                if (!ReferenceEquals(_sqlCon, null) && _sqlCon.State != ConnectionState.Closed)
                {
                    _sqlCon.Close();
                    _sqlCon.Dispose();
                }
                LogHelper.LogInfo(Log, "Requisition UpdateTaxOnLineItem Method Ended ");
            }
            return result;
        }

        public long UpdateCatalogOrgEntitiesToRequisition(long requistionId, long corporationEntityId, long expenseCodeEntityId)
        {
            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;
            long documentCode = 0;
            try
            {
                LogHelper.LogInfo(Log, "Requisition UpdateCatalogOrgEntitiesToRequisition Method Started ");

                var sqlHelper = ContextSqlConn;
                _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();

                if (requistionId > 0)
                {
                    if (Log.IsDebugEnabled)
                        Log.Debug(string.Format(CultureInfo.InvariantCulture, "Requisition UpdateCatalogOrgEntitiesToRequisition sp usp_P2P_REQ_UpdateOrgEntitiesFromCatalog " +
                                                          "with parameter: requistionId={0},corporationEntityId={1}, expenseCodeEntityId={2} was called.",
                                                                    requistionId, corporationEntityId, expenseCodeEntityId));

                    using (var objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_UPDATEORGENTITIESFROMCATALOG, _sqlCon))
                    {
                        objSqlCommand.CommandType = CommandType.StoredProcedure;
                        objSqlCommand.Parameters.AddWithValue("@requistionId", requistionId);
                        objSqlCommand.Parameters.AddWithValue("@corporationEntityId", corporationEntityId);
                        objSqlCommand.Parameters.AddWithValue("@expenseCodeEntityId", expenseCodeEntityId);
                        objSqlCommand.Parameters.AddWithValue("@UserId", UserContext.ContactCode);
                        documentCode = Convert.ToInt64(ContextSqlConn.ExecuteScalar(objSqlCommand, _sqlTrans), CultureInfo.InvariantCulture);
                    }
                    if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
                        _sqlTrans.Commit();
                }
            }
            catch(Exception ex)
            {
                LogHelper.LogError(Log, "Error occured while Requisition UpdateCatalogOrgEntitiesToRequisition documentcode :- "+ requistionId, ex);
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
                LogHelper.LogInfo(Log, "Requisition UpdateCatalogOrgEntitiesToRequisition Method Ended ");
            }
            return documentCode;
        }

        public long UpdateCatalogOrgEntitiesByItemId(string documentItemIds, long corporationEntityId, long expenseCodeEntityId, long documentId)
        {
            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;
            long documentItemCode = 0;
            try
            {
                LogHelper.LogInfo(Log, "Requisition UpdateCatalogOrgEntitiesByItemId Method Started ");

                var sqlHelper = ContextSqlConn;
                _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();

                if (!string.IsNullOrEmpty(documentItemIds))
                {
                    if (Log.IsDebugEnabled)
                        Log.Debug(string.Format(CultureInfo.InvariantCulture, "Requisition UpdateCatalogOrgEntitiesByItemId sp usp_P2P_REQ_UpdateOrgEntitiesFromCatalogByItemId " +
                                                          "with parameter: requistionItemId={0},corporationEntityId={1}, expenseCodeEntityId={2} was called.",
                                                                    documentItemIds, corporationEntityId, expenseCodeEntityId));

                    using (var objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_UPDATEORGENTITIESFROMCATALOGBYITEMID, _sqlCon))
                    {
                        objSqlCommand.CommandType = CommandType.StoredProcedure;
                        objSqlCommand.CommandTimeout = 150;
                        objSqlCommand.Parameters.AddWithValue("@requistionItemIds", documentItemIds);
                        objSqlCommand.Parameters.AddWithValue("@corporationEntityId", corporationEntityId);
                        objSqlCommand.Parameters.AddWithValue("@expenseCodeEntityId", expenseCodeEntityId);
                        objSqlCommand.Parameters.AddWithValue("@UserId", UserContext.ContactCode);
                        documentItemCode = Convert.ToInt64(ContextSqlConn.ExecuteScalar(objSqlCommand, _sqlTrans), CultureInfo.InvariantCulture);
                    }
                    if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
                        _sqlTrans.Commit();
                }
            }
            catch
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
                throw;
            }
            finally
            {
                if (!ReferenceEquals(_sqlCon, null) && _sqlCon.State != ConnectionState.Closed)
                {
                    _sqlCon.Close();
                    _sqlCon.Dispose();
                }
                LogHelper.LogInfo(Log, "Requisition UpdateCatalogOrgEntitiesByItemId Method Ended ");
            }
            return documentItemCode;
        }

        public bool UpdateBillToLocation(long documentId, long entityDetailCode, long LOBEntityDetailCode)
        {
            bool flag = false;
            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;
            try
            {
                LogHelper.LogInfo(Log, "Requisition UpdateBillToLocation Method Started for documentId = " + documentId);

                var sqlHelper = ContextSqlConn;
                _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();

                flag = Convert.ToBoolean(sqlHelper.ExecuteScalar(_sqlTrans, ReqSqlConstants.USP_P2P_REQ_UPDATEBILLTOLOCATION, documentId, entityDetailCode, LOBEntityDetailCode));
                if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
                    _sqlTrans.Commit();

            }
            catch
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
                throw;
            }
            finally
            {
                if (!ReferenceEquals(_sqlCon, null) && _sqlCon.State != ConnectionState.Closed)
                {
                    _sqlCon.Close();
                    _sqlCon.Dispose();
                }
                LogHelper.LogInfo(Log, "Requisition UpdateBillToLocation Method Ended for documentId = " + documentId);
            }

            return flag;
        }

        public bool ProrateTax(long requisitionId, decimal? Tax, int maxPrecessionValue, int maxPrecessionforTotal, int maxPrecessionForTaxesAndCharges, bool IsTaxUserEdited = false, bool isRestrictChangeRequisition = false)
        {
            bool flag = false;
            SqlConnection _sqlCon = null;
            try
            {
                LogHelper.LogInfo(Log, String.Concat("Requisition ProrateTax Method Started for requisitionId = ", requisitionId));

                var sqlHelper = ContextSqlConn;
                _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                _sqlCon.Open();

                flag = Convert.ToBoolean(ContextSqlConn.ExecuteScalar(ReqSqlConstants.USP_P2P_REQ_PRORATETAX, requisitionId, Tax, maxPrecessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges, IsTaxUserEdited, isRestrictChangeRequisition), NumberFormatInfo.InvariantInfo);
            }
            catch
            {
                throw;
            }
            finally
            {
                if (!ReferenceEquals(_sqlCon, null) && _sqlCon.State != ConnectionState.Closed)
                {
                    _sqlCon.Close();
                    _sqlCon.Dispose();
                }
                LogHelper.LogInfo(Log, String.Concat("Requisition ProrateTax Method Ended for requisitionId = ", requisitionId));
            }

            return flag;
        }

        /// <summary>
        /// Deleting Requisition Line Items based on OrgEntity change
        /// </summary>
        /// <param name="requisitionId">RequisitionID</param>
        /// <param name="orgEntityDetailCode">Previous OrgEntityDetailCode</param>
        /// <returns> return true/false</returns>
        public bool DeleteLineItemsByOrgEntity(long requisitionId, long orgEntityDetailCode, int maxPrecessionValue, int maxPrecessionValueTotal, int maxPrecessionValueForTaxAndCharges)
        {
            bool result;
            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;
            try
            {
                LogHelper.LogInfo(Log, "Requisition DeleteLineItemsByOrgEntity Method Started for RequisitionId = " + requisitionId);

                _sqlCon = (SqlConnection)ContextSqlConn.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();

                if (Log.IsDebugEnabled)
                    Log.Debug(string.Format("Requisition DeleteLineItemsByOrgEntity sp usp_P2P_REQ_DeleteLineItemsByOrgEntityCode with parameters: RequistionId = {0}, OrgEntityDetailCode = {1} was called.", requisitionId, orgEntityDetailCode));

                result = Convert.ToBoolean(ContextSqlConn.ExecuteScalar(_sqlTrans, ReqSqlConstants.USP_P2P_REQ_DELETELINEITEMSBYORGENTITYCODE, requisitionId, orgEntityDetailCode, (long)UserContext.UserId, maxPrecessionValue, maxPrecessionValueTotal, maxPrecessionValueForTaxAndCharges), CultureInfo.InvariantCulture);

                if (requisitionId > 0)
                {
                    var sqlDocumentDAO = new SQLDocumentDAO
                    {
                        SqlTransaction = _sqlTrans,
                        ReliableSqlDatabase = ContextSqlConn,
                        UserContext = UserContext,
                        GepConfiguration = GepConfiguration
                    };

                    SaveRequisitionAdditionalDetails(requisitionId, sqlDocumentDAO);
                    if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
                        _sqlTrans.Commit();
                }
                else
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
                }
            }
            catch
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

                throw;
            }
            finally
            {
                if (!ReferenceEquals(_sqlCon, null) && _sqlCon.State != ConnectionState.Closed)
                {
                    _sqlCon.Close();
                    _sqlCon.Dispose();
                }
                LogHelper.LogInfo(Log, "Requisition DeleteLineItemsByOrgEntity Method Ended for RequisitionId=" + requisitionId);
            }
            return result;
        }

        /// <summary>
        /// Deleting Requisition Line Items based on BU change
        /// </summary>
        /// <param name="requisitionId"></param>
        /// <param name="buList"></param>
        /// <param name="maxPrecessionValue"></param>
        /// <param name="maxPrecessionValueTotal"></param>
        /// <param name="maxPrecessionValueForTaxAndCharges"></param>
        /// <param name="lobId">Line Items are not currently mapped to LOB</param>
        /// <returns>return true/false</returns>
        public bool DeleteLineItemsBasedOnBUChange(long requisitionId, string buList, int maxPrecessionValue, int maxPrecessionValueTotal, int maxPrecessionValueForTaxAndCharges, long LOBId = 0)
        {
            bool result;
            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;
            try
            {
                LogHelper.LogInfo(Log, "Requisition DeleteLineItemsBasedOnBUChange Method Started for RequisitionId = " + requisitionId);

                _sqlCon = (SqlConnection)ContextSqlConn.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();

                if (Log.IsDebugEnabled)
                    Log.Debug(string.Format("Requisition DeleteLineItemsBasedOnBUChange sp usp_P2P_REQ_DeleteLineItemsOnBUChange with parameters: RequistionId = {0} was called.", requisitionId));

                result = Convert.ToBoolean(ContextSqlConn.ExecuteScalar(_sqlTrans, ReqSqlConstants.USP_P2P_REQ_DELETELINEITEMSONBUCHANGE, requisitionId, (long)UserContext.UserId, buList, maxPrecessionValue, maxPrecessionValueTotal, maxPrecessionValueForTaxAndCharges), CultureInfo.InvariantCulture);

                if (requisitionId > 0)
                {
                    var sqlDocumentDAO = new SQLDocumentDAO
                    {
                        SqlTransaction = _sqlTrans,
                        ReliableSqlDatabase = ContextSqlConn,
                        UserContext = UserContext,
                        GepConfiguration = GepConfiguration
                    };

                    SaveRequisitionAdditionalDetails(requisitionId, sqlDocumentDAO);
                    if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
                        _sqlTrans.Commit();
                }
                else
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
                }
            }
            catch
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

                throw;
            }
            finally
            {
                if (!ReferenceEquals(_sqlCon, null) && _sqlCon.State != ConnectionState.Closed)
                {
                    _sqlCon.Close();
                    _sqlCon.Dispose();
                }
                LogHelper.LogInfo(Log, "Requisition DeleteLineItemsBasedOnBUChange Method Ended for RequisitionId=" + requisitionId);
            }
            return result;
        }

        /// <summary>
        /// Get All Requisition Details By RequisitionId
        /// </summary>
        /// <param name="requisitionId"> requisitionId</param>
        /// <param name="userId">userId</param>
        /// <param name="typeOfUser">typeOfUser</param>
        /// <returns>All the Requisition details along with line items and accounting details</returns>
        public Requisition GetAllRequisitionDetailsByRequisitionId(long requisitionId, long userId, int typeOfUser, List<long> reqLineItemIds = null, Dictionary<string, string> settings = null)
        {
            RefCountingDataReader objRefCountingDataReader = null;

            try
            {
                LogHelper.LogInfo(Log, "GetBasicDetailsById Method Started for id=" + requisitionId);

                var sqlHelper = ContextSqlConn;
                DataSet objDs = null;

                //Read CatalogItemSources setting for return all item sources considered as "Catalog"
                var catalogItemSources = settings != null && settings.ContainsKey("CatalogItemSources") && !(string.IsNullOrEmpty(settings["CatalogItemSources"])) ? settings["CatalogItemSources"].Split(',').Select(i => Convert.ToInt32(i)).ToList() : null;

                if (requisitionId > 0)
                {
                    if (Log.IsDebugEnabled)
                        Log.Debug(string.Format("In GetAllRequisitionDetailsByRequisitionId Method, sp: usp_P2P_REQ_GetAllRequisitionDetailsByRequisitionId with parameter: RequisitionId= {0}, userId= {1}", requisitionId, userId));

                    DataTable dtReqItemId = new DataTable();
                    dtReqItemId.Columns.Add("Id", typeof(long));
                    if (reqLineItemIds != null && reqLineItemIds.Any())
                    {
                        foreach (long item in reqLineItemIds)
                        {
                            DataRow dr = dtReqItemId.NewRow();
                            dr["Id"] = item != 0 ? item : 0;
                            dtReqItemId.Rows.Add(dr);
                        }
                    }
                    else
                    {
                        DataRow dr = dtReqItemId.NewRow();
                        dr["Id"] = 0;
                        dtReqItemId.Rows.Add(dr);
                    }

                    using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_GETALLREQUISITIONDETAILSBYREQUISITIONID))
                    {
                        objSqlCommand.CommandType = CommandType.StoredProcedure;
                        objSqlCommand.CommandTimeout = 150;
                        objSqlCommand.Parameters.Add(new SqlParameter("@requisitonId", requisitionId));
                        objSqlCommand.Parameters.Add(new SqlParameter("@userId ", userId));
                        objSqlCommand.Parameters.Add(new SqlParameter("@typeOfUser ", typeOfUser));
                        objSqlCommand.Parameters.Add(new SqlParameter("@tvp_P2P_REQ_ReqItemIds", SqlDbType.Structured)
                        {
                            TypeName = ReqSqlConstants.TVP_LONG,
                            Value = dtReqItemId
                        });

                        objDs = sqlHelper.ExecuteDataSet(objSqlCommand);


                    }

                    // DataSet objDs = sqlHelper.ExecuteDataSet(ReqSqlConstants.USP_P2P_REQ_GETALLREQUISITIONDETAILSBYREQUISITIONID,
                    //                                                new object[] { requisitionId, userId, typeOfUser });

                    if (objDs != null && objDs.Tables.Count > 0)
                    {
                        var objDocument = new Document
                        {
                            DocumentCode = requisitionId,
                            DocumentTypeInfo = DocumentType.Requisition,
                            IsDocumentDetails = true,
                            IsAddtionalDetails = false,
                            IsStakeholderDetails = true,
                            CompanyName = UserContext.ClientName
                        };


                        var sqlDocumentDAO = new SQLDocumentDAO
                        {
                            ReliableSqlDatabase = sqlHelper,
                            UserContext = UserContext,
                            GepConfiguration = GepConfiguration
                        };

                        objDocument = sqlDocumentDAO.GetDocumentDetailsById(objDocument, true);

                        foreach (DataRow dr in objDs.Tables[0].Rows)
                        {
                            var objRequisition = new Requisition
                            {
                                DocumentCode = objDocument.DocumentCode,
                                DocumentName = objDocument.DocumentName,
                                CreatedOn = objDocument.CreatedOn,
                                CreatedBy = objDocument.CreatedBy,
                                UpdatedOn = objDocument.UpdatedOn,
                                DocumentType = P2PDocumentType.Requisition,
                                DocumentNumber = objDocument.DocumentNumber,
                                DocumentStatusInfo = objDocument.DocumentStatusInfo,
                                DocumentSourceTypeInfo = objDocument.DocumentSourceTypeInfo,
                                DocumentId = (long)dr[ReqSqlConstants.COL_REQUISITION_ID],
                                ParentDocumentCode = (long)dr[ReqSqlConstants.COL_PARENTDOCUMENTCODE],
                                RequesterId = (long)dr[ReqSqlConstants.COL_REQUESTER_ID],
                                BusinessUnitId = (long)dr[ReqSqlConstants.COL_BUID],
                                BusinessUnitName = Convert.ToString(dr[ReqSqlConstants.COL_BUSINESSUNITNAME], CultureInfo.InvariantCulture),
                                Currency = Convert.ToString(dr[ReqSqlConstants.COL_CURRENCY], CultureInfo.InvariantCulture),
                                Tax = (!string.IsNullOrWhiteSpace(dr[ReqSqlConstants.COL_TAX].ToString())) ? (Convert.ToDecimal(dr[ReqSqlConstants.COL_TAX])) : default(decimal?),
                                Shipping = (!string.IsNullOrWhiteSpace(dr[ReqSqlConstants.COL_SHIPPING].ToString())) ? (Convert.ToDecimal(dr[ReqSqlConstants.COL_SHIPPING])) : default(decimal?),
                                AdditionalCharges = (!string.IsNullOrWhiteSpace(dr[ReqSqlConstants.COL_ADDITIONAL_CHARGES].ToString())) ? (Convert.ToDecimal(dr[ReqSqlConstants.COL_ADDITIONAL_CHARGES])) : default(decimal?),
                                TotalAmount = (!string.IsNullOrWhiteSpace(dr[ReqSqlConstants.COL_REQUISITION_AMOUNT].ToString())) ? (Convert.ToDecimal(dr[ReqSqlConstants.COL_REQUISITION_AMOUNT])) : default(decimal?),
                                ItemTotalAmount = (!string.IsNullOrWhiteSpace(dr[ReqSqlConstants.COL_ITEM_TOTAL].ToString())) ? (Convert.ToDecimal(dr[ReqSqlConstants.COL_ITEM_TOTAL])) : default(decimal?),
                                RequesterName = Convert.ToString(dr[ReqSqlConstants.COL_REQUESTER_NAME], CultureInfo.InvariantCulture),
                                ApproveURL = Convert.ToString(dr[ReqSqlConstants.COL_APPROVE_URL], CultureInfo.InvariantCulture),
                                RejectURL = Convert.ToString(dr[ReqSqlConstants.COL_REJECT_URL], CultureInfo.InvariantCulture),
                                MaterialItemCount = Convert.ToInt32(dr[ReqSqlConstants.COL_MATERIAL_ITEM_COUNT], CultureInfo.InvariantCulture),
                                ServiceItemCount = Convert.ToInt32(dr[ReqSqlConstants.COL_SERVICE_ITEM_COUNT], CultureInfo.InvariantCulture),
                                AdvanceItemCount = Convert.ToInt32(dr[ReqSqlConstants.COL_ADVANCE_ITEM_COUNT], CultureInfo.InvariantCulture),
                                RequisitionSource = (RequisitionSource)Convert.ToInt16(dr[ReqSqlConstants.COL_REQUISITION_SOURCE]),
                                IsOBOManager = Convert.ToBoolean(dr[ReqSqlConstants.COL_ISOBOMANAGER], CultureInfo.InvariantCulture),
                                OnBehalfOf = (long)dr[ReqSqlConstants.COL_ONBEHALFOF] == (long)dr[ReqSqlConstants.COL_REQUESTER_ID] ? 0 : (long)dr[ReqSqlConstants.COL_ONBEHALFOF],
                                OnBehalfOfName = (long)dr[ReqSqlConstants.COL_ONBEHALFOF] == (long)dr[ReqSqlConstants.COL_REQUESTER_ID] ? "" : Convert.ToString(dr[ReqSqlConstants.COL_ONBEHALFOFNAME], CultureInfo.InvariantCulture),
                                CostApprover = Convert.ToInt64(dr[ReqSqlConstants.COL_CostApprover], CultureInfo.InvariantCulture),
                                IsCatalogItemsExists = Convert.ToBoolean(dr[ReqSqlConstants.COL_IS_CATALOG_ITEMS_EXISTS], CultureInfo.InvariantCulture),
                                WorkOrderNumber = Convert.ToString(dr[ReqSqlConstants.COL_WORKORDERNO], CultureInfo.InvariantCulture),
                                ERPOrderType = Convert.ToInt32(dr[ReqSqlConstants.COL_ERPORDERTYPE], CultureInfo.InvariantCulture),
                                CapitalCode = Convert.ToString(dr[ReqSqlConstants.COL_CAPITALCODE], CultureInfo.InvariantCulture),
                                ShiptoLocation = new ShiptoLocation()
                                {
                                    ShiptoLocationId = Convert.ToInt32(dr[ReqSqlConstants.COL_SHIPTOLOC_ID], CultureInfo.InvariantCulture),
                                    IsAdhoc = Convert.ToBoolean(dr[ReqSqlConstants.COL_SHIPTOLOC_ISADHOC], CultureInfo.InvariantCulture)
                                },
                                BilltoLocation = new BilltoLocation
                                {
                                    BilltoLocationId = Convert.ToInt32(dr[ReqSqlConstants.COL_BILLTOLOC_ID], CultureInfo.InvariantCulture)
                                },
                                DelivertoLocation = new DelivertoLocation
                                {
                                    DelivertoLocationId = Convert.ToInt32(dr[ReqSqlConstants.COL_DELIVERTOLOCATION_ID], CultureInfo.InvariantCulture)
                                },
                                EntitySumList = new List<EntitySumCalculation>(),
                                PurchaseType = Convert.ToInt32(dr[ReqSqlConstants.COL_PURCHASETYPE], CultureInfo.InvariantCulture),
                                PurchaseTypeDescription = Convert.ToString(dr[ReqSqlConstants.COL_PURCHASE_DESCRIPTION], CultureInfo.InvariantCulture),
                                CustomAttrFormId = Convert.ToInt64(dr[ReqSqlConstants.COL_FORMID], CultureInfo.InvariantCulture),
                                CustomAttrFormIdForItem = Convert.ToInt64(dr[ReqSqlConstants.COL_ITEMFORMID], CultureInfo.InvariantCulture),
                                CustomAttrFormIdForSplit = Convert.ToInt64(dr[ReqSqlConstants.COL_SPLITFORMID], CultureInfo.InvariantCulture),
                                EntityId = objDocument.EntityId,
                                EntityDetailCode = objDocument.EntityDetailCode,
                                UserDefinedApproversCount = Convert.ToInt32(dr[ReqSqlConstants.COL_USERDEFINEDAPPROVALCOUNT], CultureInfo.InvariantCulture),
                                TotalAmountChange = Convert.ToDecimal(dr[ReqSqlConstants.COL_REQUISITIONTOTALCHANGE], CultureInfo.InvariantCulture),
                                DefaultCurrencyCode = Convert.ToString(dr[ReqSqlConstants.COL_DEFAULTCURRENCYCODE], CultureInfo.InvariantCulture),
                                Rebillable = !Convert.IsDBNull(dr[ReqSqlConstants.COL_BILLABLE]) ? Convert.ToString(dr[ReqSqlConstants.COL_BILLABLE]) : String.Empty,
                                CountryCode = Convert.ToString(dr[ReqSqlConstants.COL_COUNTRYCODE], CultureInfo.InvariantCulture),
                                HeaderTaxCodeCount = Convert.ToInt32(dr[ReqSqlConstants.COL_HEADERTAXCODECOUNT], CultureInfo.InvariantCulture),
                                TaxCodes = Convert.ToString(dr[ReqSqlConstants.COL_TAXCODES], CultureInfo.InvariantCulture),
                                LOBEntity = Convert.ToInt32(dr[ReqSqlConstants.COL_LOBENTITYDETAILCODE], CultureInfo.InvariantCulture),
                                POSignatoryCode = Convert.ToInt64(dr[ReqSqlConstants.COL_POSIGNATORYCODE], CultureInfo.InvariantCulture),
                                POSignatoryName = Convert.ToString(dr[ReqSqlConstants.COL_POSIGNATORYNAME], CultureInfo.InvariantCulture),
                                BudgetoryStatus = Convert.ToInt32(dr[ReqSqlConstants.COL_BUDGETORYSTATUS], CultureInfo.InvariantCulture),
                                IsUrgent = Convert.ToBoolean(dr[ReqSqlConstants.COL_ISURGENT], CultureInfo.InvariantCulture),
                                RequisitionAmount = Convert.ToDecimal(dr[ReqSqlConstants.COL_REQUISITION_AMOUNT], CultureInfo.InvariantCulture),
                                RiskScore = !Convert.IsDBNull(dr[ReqSqlConstants.COL_RISKSCORE]) ? Convert.ToDecimal(dr[ReqSqlConstants.COL_RISKSCORE], CultureInfo.InvariantCulture) : ((decimal?)null),
                                PartnerCount = dr.Table.Columns.Contains(ReqSqlConstants.COL_PARTNERCOUNT) ? Convert.ToInt32(dr[ReqSqlConstants.COL_PARTNERCOUNT], CultureInfo.InvariantCulture) : 0,
                                RequisitionPreviousAmount = Convert.ToDecimal(dr[ReqSqlConstants.COL_REQUISITIONPREVIOUSAMOUNT], CultureInfo.InvariantCulture),
                                IsAdhocShipToLocation= dr.Table.Columns.Contains(ReqSqlConstants.COL_ISADHOCSHIPTOLOCATION) ?  Convert.ToBoolean(dr[ReqSqlConstants.COL_ISADHOCSHIPTOLOCATION], CultureInfo.InvariantCulture):false,
                                IsAdvanceRequsition = dr.Table.Columns.Contains(ReqSqlConstants.COL_ISADVANCEREQUISITION) ? Convert.ToBoolean(dr[ReqSqlConstants.COL_ISADVANCEREQUISITION], CultureInfo.InvariantCulture) : false,
                                InterfaceStatusId = Convert.ToInt32(dr[ReqSqlConstants.COL_INTERFACE_STATUS_ID], CultureInfo.InvariantCulture),
                                InterfaceStatusDesc = Convert.ToString((InterfaceStatus)(Convert.ToInt32(dr[ReqSqlConstants.COL_INTERFACE_STATUS_ID], CultureInfo.InvariantCulture)))                                
                            };
                            objDocument.DocumentBUList.ToList().ForEach(data => { objRequisition.DocumentBUList.Add(data); });

                            List<DocumentStakeHolder> stakeholderlist = objDocument.DocumentStakeHolderList.Where(e => (e.StakeholderTypeInfo == StakeholderType.Approver && (e.ContactCode == userId || e.ProxyContactCode == userId))).ToList();
                            stakeholderlist.ToList().ForEach(data => { objRequisition.DocumentStakeHolderList.Add(data); });

                            //HEADER LEVEL ENTITY DETAILS
                            objRequisition.DocumentAdditionalEntitiesInfoList = new Collection<DocumentAdditionalEntityInfo>();
                            foreach (DataRow entityRow in objDs.Tables[1].Rows)
                            {
                                objRequisition.DocumentAdditionalEntitiesInfoList.Add(new DocumentAdditionalEntityInfo
                                {
                                    EntityDetailCode = (long)entityRow[ReqSqlConstants.COL_SPLIT_ITEM_ENTITYDETAILCODE],
                                    EntityId = Convert.ToInt32(entityRow[ReqSqlConstants.COL_ENTITY_ID], CultureInfo.InvariantCulture),
                                    EntityDisplayName = Convert.ToString(entityRow[ReqSqlConstants.COL_ENTITY_DISPLAY_NAME], CultureInfo.InvariantCulture),
                                    EntityCode = Convert.ToString(entityRow[ReqSqlConstants.COL_ENTITY_CODE], CultureInfo.InvariantCulture),
                                    FieldOrder = Convert.ToInt32(entityRow[ReqSqlConstants.COL_FIELD_ORDER], CultureInfo.InvariantCulture)
                                });
                            }

                            objRequisition.SourceSystemInfo = new SourceSystemInfo()
                            {
                                SourceSystemId = Convert.ToInt32(dr[ReqSqlConstants.COL_SOURCESYSTEMID], CultureInfo.InvariantCulture)
                            };
                            objRequisition.RequisitionSourceSystemId = Convert.ToInt32(dr[ReqSqlConstants.COL_SOURCESYSTEMID], CultureInfo.InvariantCulture);

                            //REQUISITION LINE ITEM DETAILS
                            objRequisition.RequisitionItems = new List<RequisitionItem>();
                            foreach (DataRow lstItems in objDs.Tables[2].Rows)
                            {
                                var objRequisitionItem = new RequisitionItem
                                {
                                    DocumentItemId = (long)lstItems[ReqSqlConstants.COL_REQUISITION_ITEM_ID],
                                    P2PLineItemId = (long)lstItems[ReqSqlConstants.COL_P2P_LINE_ITEM_ID],
                                    DocumentId = (long)lstItems[ReqSqlConstants.COL_REQUISITION_ID],
                                    ShortName = Convert.ToString(lstItems[ReqSqlConstants.COL_SHORT_NAME], CultureInfo.InvariantCulture),
                                    Description = Convert.ToString(lstItems[ReqSqlConstants.COL_DESCRIPTION], CultureInfo.InvariantCulture),
                                    UnitPrice = (!string.IsNullOrWhiteSpace(lstItems[ReqSqlConstants.COL_UNIT_PRICE].ToString())) ? (Convert.ToDecimal(lstItems[ReqSqlConstants.COL_UNIT_PRICE])) : default(decimal?),
                                    Quantity = Convert.ToDecimal(lstItems[ReqSqlConstants.COL_QUANTITY], CultureInfo.InvariantCulture),
                                    UOM = Convert.ToString(lstItems[ReqSqlConstants.COL_UOM], CultureInfo.InvariantCulture),
                                    UOMDesc = Convert.ToString(lstItems[ReqSqlConstants.COL_UOM_DESC], CultureInfo.InvariantCulture),
                                    DateRequested = Convert.IsDBNull(lstItems[ReqSqlConstants.COL_DATE_REQUESTED]) ? DateTime.MinValue : Convert.ToDateTime(lstItems[ReqSqlConstants.COL_DATE_REQUESTED], CultureInfo.InvariantCulture),
                                    DateNeeded = Convert.IsDBNull(lstItems[ReqSqlConstants.COL_DATE_NEEDED]) ? DateTime.MinValue : Convert.ToDateTime(lstItems[ReqSqlConstants.COL_DATE_NEEDED], CultureInfo.InvariantCulture),
                                    PartnerCode = Convert.ToDecimal(lstItems[ReqSqlConstants.COL_PARTNER_CODE], CultureInfo.InvariantCulture),
                                    ClientPartnerCode = Convert.ToString(lstItems[ReqSqlConstants.COL_CLIENT_PARTNERCODE], CultureInfo.InvariantCulture),
                                    CategoryId = (long)lstItems[ReqSqlConstants.COL_CATEGORY_ID],
                                    ManufacturerName = Convert.ToString(lstItems[ReqSqlConstants.COL_MANUFACTURER_NAME], CultureInfo.InvariantCulture),
                                    ManufacturerPartNumber = Convert.ToString(lstItems[ReqSqlConstants.COL_MANUFACTURER_PART_NUMBER], CultureInfo.InvariantCulture),
                                    ManufacturerModel = Convert.ToString(lstItems[ReqSqlConstants.COL_MANUFACTURER_MODEL], CultureInfo.InvariantCulture),
                                    ItemType = (ItemType)Convert.ToInt16(lstItems[ReqSqlConstants.COL_ITEM_TYPE_ID], CultureInfo.InvariantCulture),
                                    Tax = (!string.IsNullOrWhiteSpace(lstItems[ReqSqlConstants.COL_LINE_ITEM_TAX].ToString())) ? (Convert.ToDecimal(lstItems[ReqSqlConstants.COL_LINE_ITEM_TAX])) : default(decimal?),
                                    ShippingCharges = (!string.IsNullOrWhiteSpace(lstItems[ReqSqlConstants.COL_SHIPPING_CHARGES].ToString())) ? (Convert.ToDecimal(lstItems[ReqSqlConstants.COL_SHIPPING_CHARGES])) : default(decimal?),
                                    AdditionalCharges = (!string.IsNullOrWhiteSpace(lstItems[ReqSqlConstants.COL_ADDITIONAL_CHARGES].ToString())) ? (Convert.ToDecimal(lstItems[ReqSqlConstants.COL_ADDITIONAL_CHARGES])) : default(decimal?),
                                    StartDate = Convert.IsDBNull(lstItems[ReqSqlConstants.COL_START_DATE]) ? DateTime.MinValue : Convert.ToDateTime(lstItems[ReqSqlConstants.COL_START_DATE], CultureInfo.InvariantCulture),
                                    EndDate = Convert.IsDBNull(lstItems[ReqSqlConstants.COL_END_DATE]) ? DateTime.MinValue : Convert.ToDateTime(lstItems[ReqSqlConstants.COL_END_DATE], CultureInfo.InvariantCulture),
                                    TotalRecords = Convert.ToInt32(lstItems[ReqSqlConstants.COL_TOTAL_RECORDS], CultureInfo.InvariantCulture),
                                    SourceType = (ItemSourceType)Convert.ToInt16(lstItems[ReqSqlConstants.COL_SOURCE_TYPE], CultureInfo.InvariantCulture),
                                    MinimumOrderQuantity = Convert.ToDecimal(lstItems[ReqSqlConstants.COL_MIN_ORDER_QUANTITY], CultureInfo.InvariantCulture),
                                    MaximumOrderQuantity = Convert.ToDecimal(lstItems[ReqSqlConstants.COL_MAX_ORDER_QUANTITY], CultureInfo.InvariantCulture),
                                    Banding = Convert.ToInt32(lstItems[ReqSqlConstants.COL_BANDING], CultureInfo.InvariantCulture),
                                    ItemCode = (long)lstItems[ReqSqlConstants.COL_ITEM_CODE],
                                    RequisitionStatus = objDocument.DocumentStatusInfo,
                                    AllowDecimalsForUom = Convert.ToBoolean(lstItems[ReqSqlConstants.COL_UOM_ALLOWDECIMAL], CultureInfo.InvariantCulture),
                                    Currency = Convert.ToString(lstItems[ReqSqlConstants.COL_CURRENCY], CultureInfo.InvariantCulture),
                                    IsTaxExempt = Convert.ToBoolean(lstItems[ReqSqlConstants.COL_ISTAXEXEMPT], CultureInfo.InvariantCulture),
                                    ItemExtendedType = (ItemExtendedType)Convert.ToInt16(lstItems[ReqSqlConstants.COL_ITEM_EXTENDED_TYPE], CultureInfo.InvariantCulture),
                                    Efforts = (!string.IsNullOrWhiteSpace(lstItems[ReqSqlConstants.COL_EFFORTS].ToString())) ? (Convert.ToDecimal(lstItems[ReqSqlConstants.COL_EFFORTS])) : default(decimal?),
                                    SupplierPartId = Convert.ToString(lstItems[ReqSqlConstants.COL_SUPPLIERPARTID], CultureInfo.InvariantCulture),
                                    ItemNumber = Convert.ToString(lstItems[ReqSqlConstants.COL_ITEMNUMBER], CultureInfo.InvariantCulture),
                                    CatalogItemId = (long)lstItems[ReqSqlConstants.COL_CATALOGITEMID],
                                    Billable = Convert.ToString(lstItems[ReqSqlConstants.COL_BILLABLE], CultureInfo.InvariantCulture),
                                    Capitalized = Convert.ToString(lstItems[ReqSqlConstants.COL_CAPITALIZED], CultureInfo.InvariantCulture),
                                    CapitalCode = Convert.ToString(lstItems[ReqSqlConstants.COL_CAPITALCODE], CultureInfo.InvariantCulture),
                                    ContractNo = Convert.ToString(lstItems[ReqSqlConstants.COL_CONTRACTNO], CultureInfo.InvariantCulture),
                                    ItemLineNumber = Convert.ToInt32(lstItems[ReqSqlConstants.COL_ITEMLINENUMBER], CultureInfo.InvariantCulture),
                                    BuyerContactCode = (long)lstItems[ReqSqlConstants.COL_BUYERCONTACTCODE],
                                    OrderLocationId = (long)lstItems[ReqSqlConstants.COL_ORDER_LOCATIONID],
                                    RemitToLocationId = (long)lstItems[ReqSqlConstants.COL_INVOICE_REMITTOLOCATIONID],
                                    OrderLocationAddressCode = (long)lstItems[ReqSqlConstants.COL_ORDERTOLOCATION_CODE],
                                    ShipFromLocationAddressCode = (long)lstItems[ReqSqlConstants.COL_SHIPFROMLOCATIONCODE],
                                    ShipFromLocationId = (long)lstItems[ReqSqlConstants.COL_SHIPFROMLOCATIONID],
                                    RemitToLocationAddressCode = (long)lstItems[ReqSqlConstants.COL_REMITTOLOCATION_CODE],
                                    SourceSystemId = (long)lstItems[ReqSqlConstants.COL_SOURCESYSTEMID],
                                    PartnerSourceSystemValue = Convert.ToString(lstItems[ReqSqlConstants.COL_PARTNERSOURCESYSTEMVALUE], CultureInfo.InvariantCulture),
                                    ContractItemId = (long)lstItems[ReqSqlConstants.COL_CONTRACTITEMID],
                                    PartnerReconMatchTypeId = Convert.ToInt32(lstItems[ReqSqlConstants.COL_PARTNERRECONMATCHTYPEID]),

                                    TaxCodes = Convert.ToString(lstItems[ReqSqlConstants.COL_TAXCODES], CultureInfo.InvariantCulture),
                                    CountryCode = Convert.ToString(lstItems[ReqSqlConstants.COL_COUNTRYCODE], CultureInfo.InvariantCulture),
                                    LineTaxCodeCount = Convert.ToInt32(lstItems[ReqSqlConstants.COL_LINETAXCODECOUNT], CultureInfo.InvariantCulture),
                                    ShipToLocationId = Convert.ToInt32(lstItems[ReqSqlConstants.COL_SHIPTOLOCATION_ID], CultureInfo.InvariantCulture),
                                    OverallItemLimit = Convert.ToDecimal(lstItems[ReqSqlConstants.COL_OVERALLITEMLIMIT]),
                                    IsProcurable = Convert.ToInt32(lstItems[ReqSqlConstants.COL_IS_PROCURABLE], CultureInfo.InvariantCulture),
                                    InventoryType = Convert.IsDBNull(lstItems[ReqSqlConstants.COL_INVENTORYTYPE]) ? false : Convert.ToBoolean(lstItems[ReqSqlConstants.COL_INVENTORYTYPE]),  // Set default to false, non-stockable                           
                                    MatchType = (MatchType)Convert.ToInt16((lstItems[ReqSqlConstants.COL_MATCHTYPE])),//this is for Expose Match type in Rule engine 
                                    PriceTypeId = Convert.ToInt64((lstItems[ReqSqlConstants.COL_PRICETYPEID])),
                                    JobTitleId = Convert.ToInt64((lstItems[ReqSqlConstants.COL_JOBTITLEID])),
                                    ContingentWorkerId = Convert.ToInt64((lstItems[ReqSqlConstants.COL_CONTINGENTWORKERID])),
                                    Margin = Convert.ToDecimal(lstItems[ReqSqlConstants.COL_MARGIN]),
                                    BaseRate = Convert.ToDecimal(lstItems[ReqSqlConstants.COL_MARGIN]),
                                    ReportingManagerId = Convert.ToInt64((lstItems[ReqSqlConstants.COL_REPORTINGMANAGERID])),
                                    ProcurementStatus = Convert.ToInt16((lstItems[ReqSqlConstants.COL_PROCUREMENTSTATUS])),
                                    ItemStatus = (DocumentStatus)Convert.ToInt16((lstItems[ReqSqlConstants.COL_ITEM_STATUS])),
                                    RFXFlipType = Convert.ToInt16(lstItems[ReqSqlConstants.COL_RFXFlipType], CultureInfo.InvariantCulture),
                                    SpendControlDocumentCode = Convert.ToInt64((lstItems[ReqSqlConstants.COL_SPENDCONTROLDOCUMENTCODE])),
                                    AdvanceAmount = (!string.IsNullOrWhiteSpace(lstItems[ReqSqlConstants.COL_ADVANCEAMOUNT].ToString())) ? (Convert.ToDecimal(lstItems[ReqSqlConstants.COL_ADVANCEAMOUNT])) : default(decimal?),
                                    IsPreferredSupplier = Convert.IsDBNull(lstItems[ReqSqlConstants.COL_ISPREFERREDSUPPLIER]) ? false : Convert.ToBoolean(lstItems[ReqSqlConstants.COL_ISPREFERREDSUPPLIER]),  // Set default to false, non-stockable                           
                                  AllowFlexiblePrice = Convert.ToBoolean(lstItems[ReqSqlConstants.COL_ALLOWFLEXIBLEPRICE], CultureInfo.InvariantCulture),
                                };
                                if (objRequisitionItem.ItemType == ItemType.Material)
                                    objRequisitionItem.ItemTotalAmount = objRequisitionItem.UnitPrice * objRequisitionItem.Quantity + objRequisitionItem.Tax + objRequisitionItem.AdditionalCharges + objRequisitionItem.ShippingCharges;
                                else
                                    objRequisitionItem.ItemTotalAmount = objRequisitionItem.UnitPrice * objRequisitionItem.Efforts + objRequisitionItem.Tax + objRequisitionItem.AdditionalCharges;

                                //Accounting Details
                                var drSplitsItems = objDs.Tables[3].Select("RequisitionItemId =" +
                                                        objRequisitionItem.DocumentItemId.ToString(CultureInfo.InvariantCulture));
                                objRequisitionItem.ItemSplitsDetail = new List<RequisitionSplitItems>();

                                //Verify if the current item it's considered as "Catalog"
                                var isACatalogItem = (catalogItemSources != null && catalogItemSources.Count(c => c == (int)objRequisitionItem.SourceType) > 0);

                                foreach (var split in drSplitsItems)
                                {
                                    var objRequisitionSplitItems = new RequisitionSplitItems
                                    {
                                        DocumentItemId = (long)split[ReqSqlConstants.COL_REQUISITION_ITEM_ID],
                                        DocumentSplitItemId = (long)split[ReqSqlConstants.COL_REQUISITION_SPLIT_ITEM_ID],
                                        Percentage = Convert.ToDecimal(split[ReqSqlConstants.COL_PERCENTAGE], CultureInfo.InvariantCulture),
                                        Quantity = Convert.ToDecimal(split[ReqSqlConstants.COL_QUANTITY], CultureInfo.InvariantCulture),
                                        Tax = split[ReqSqlConstants.COL_TAX] != DBNull.Value ? Convert.ToDecimal(split[ReqSqlConstants.COL_TAX], CultureInfo.InvariantCulture) : (decimal?)null,
                                        ShippingCharges = split[ReqSqlConstants.COL_SHIPPING_CHARGES] != DBNull.Value ? Convert.ToDecimal(split[ReqSqlConstants.COL_SHIPPING_CHARGES], CultureInfo.InvariantCulture) : (decimal?)null,
                                        AdditionalCharges = split[ReqSqlConstants.COL_ADDITIONAL_CHARGES] != DBNull.Value ? Convert.ToDecimal(split[ReqSqlConstants.COL_ADDITIONAL_CHARGES], CultureInfo.InvariantCulture) : (decimal?)null,
                                        SplitItemTotal = split[ReqSqlConstants.COL_SPLIT_ITEM_TOTAL] != DBNull.Value ? Convert.ToDecimal(split[ReqSqlConstants.COL_SPLIT_ITEM_TOTAL], CultureInfo.InvariantCulture) : (decimal?)null,
                                        OverallLimitSplitItem = split[ReqSqlConstants.COL_OVERALLLIMITSPLITITEM] != DBNull.Value ? Convert.ToDecimal(split[ReqSqlConstants.COL_OVERALLLIMITSPLITITEM], CultureInfo.InvariantCulture) : 0,
                                        ErrorCode = Convert.ToString(split[ReqSqlConstants.COL_ERROR_CODE], CultureInfo.InvariantCulture),
                                        TotalRecords = Convert.ToInt32(split[ReqSqlConstants.COL_TOTAL_RECORDS], CultureInfo.InvariantCulture),
                                        SplitType = (SplitType)Convert.ToInt32(split[ReqSqlConstants.COL_SPLIT_TYPE], CultureInfo.InvariantCulture)
                                    };
                                    var drSplits = objDs.Tables[4].Select("RequisitionSplitItemId =" + objRequisitionSplitItems.DocumentSplitItemId.ToString(CultureInfo.InvariantCulture));
                                    objRequisitionSplitItems.DocumentSplitItemEntities = new List<DocumentSplitItemEntity>();
                                    foreach (var drSplit in drSplits)
                                    {
                                        var documentSplitItemEntity = new DocumentSplitItemEntity
                                        {
                                            SplitAccountingFieldId = (int)drSplit[ReqSqlConstants.COL_SPLIT_ACCOUNTING_FIELD_CONFIG_ID],
                                            SplitAccountingFieldValue = drSplit[ReqSqlConstants.COL_SPLIT_ACCOUNTING_FIELD_VALUE].ToString(),
                                            EntityDisplayName = drSplit[ReqSqlConstants.COL_ENTITY_DISPLAY_NAME].ToString(),
                                            EntityTypeId = (int)drSplit[ReqSqlConstants.COL_ENTITY_TYPE_ID],
                                            EntityCode = drSplit[ReqSqlConstants.COL_ENTITY_CODE].ToString(),
                                            EntityCodeId = drSplit[ReqSqlConstants.COL_ENTITY_TYPE_ID].ToString() + "_" + drSplit[ReqSqlConstants.COL_ENTITY_CODE].ToString(),
                                            CodeCombinationOrder = Convert.ToInt16(drSplit[ReqSqlConstants.COL_CODE_COMBINATION_ORDER], CultureInfo.InvariantCulture),
                                            ParentEntityDetailCode = (long)drSplit[ReqSqlConstants.COL_SPLIT_ITEM_PARENNTENTITYDETAILCODE],
                                            EntityType = Convert.ToString(drSplit[ReqSqlConstants.COL_ENTITY_TYPE]),
                                            StructureId = Convert.ToInt16(drSplit[ReqSqlConstants.COL_STRUCTUREID]),
                                            FieldName = drSplit[ReqSqlConstants.COL_FIELD_NAME].ToString()
                                        };
                                        objRequisitionSplitItems.DocumentSplitItemEntities.Add(documentSplitItemEntity);

                                        if (!string.IsNullOrEmpty(documentSplitItemEntity.EntityCode))
                                        {
                                            bool found = false;
                                            // var overallExists = objRequisitionItem.OverallItemLimit > 0 ? true : false;
                                            var overallExists = objRequisitionItem.ItemExtendedType == ItemExtendedType.Fixed ? true : false;

                                            decimal catalogItemSplitAmount = 0M;
                                            decimal nonCatalogItemSplitAmount = 0M;

                                            if (catalogItemSources != null)
                                            {
                                                catalogItemSplitAmount = isACatalogItem ? (objRequisitionSplitItems.OverallLimitSplitItem > objRequisitionSplitItems.SplitItemTotal.Value ? objRequisitionSplitItems.OverallLimitSplitItem : objRequisitionSplitItems.SplitItemTotal.Value) : Convert.ToDecimal(0);
                                                nonCatalogItemSplitAmount = !isACatalogItem ? (objRequisitionSplitItems.OverallLimitSplitItem > objRequisitionSplitItems.SplitItemTotal.Value ? objRequisitionSplitItems.OverallLimitSplitItem : objRequisitionSplitItems.SplitItemTotal.Value) : Convert.ToDecimal(0);
                                            }

                                            foreach (EntitySumCalculation obj in objRequisition.EntitySumList)
                                            {
                                                if (obj.EntityCode == documentSplitItemEntity.EntityCode)
                                                {
                                                    found = true;
                                                    obj.TotalAmount += objRequisitionSplitItems.SplitItemTotal.Value;
                                                    obj.OverallLimitSplitTotal += (overallExists) ? objRequisitionSplitItems.OverallLimitSplitItem : Convert.ToDecimal(objRequisitionSplitItems.SplitItemTotal ?? 0);
                                                    obj.CatalogTotalAmount += catalogItemSplitAmount;
                                                    obj.NonCatalogTotalAmount += nonCatalogItemSplitAmount;

                                                    break;
                                                }
                                            }
                                            if (!found)
                                            {
                                                objRequisition.EntitySumList.Add(
                                                    new EntitySumCalculation()
                                                    {
                                                        EntityCode = documentSplitItemEntity.EntityCode,
                                                        EntityTypeId = documentSplitItemEntity.EntityTypeId,
                                                        TotalAmount = objRequisitionSplitItems.SplitItemTotal.Value,
                                                        OverallLimitSplitTotal = (overallExists) ? objRequisitionSplitItems.OverallLimitSplitItem : Convert.ToDecimal(objRequisitionSplitItems.SplitItemTotal ?? 0),
                                                        CatalogTotalAmount = catalogItemSplitAmount,
                                                        NonCatalogTotalAmount = nonCatalogItemSplitAmount
                                                    }
                                                );
                                            }
                                        }
                                    }
                                    objRequisitionItem.ItemSplitsDetail.Add(objRequisitionSplitItems);
                                }

                                //Notes And Attachments
                                var drNotesandAttachments = objDs.Tables[5].Select("RequisitionItemID =" +
                                                        objRequisitionItem.DocumentItemId.ToString(CultureInfo.InvariantCulture));
                                objRequisitionItem.ListNotesOrAttachments = new List<NotesOrAttachments>();
                                if (drNotesandAttachments.Length > 0)
                                    GetNotesAndAttachmentsFromDataRow(objRequisition, objRequisitionItem, drNotesandAttachments);

                                //TaxCode Details                                
                                if (objDs.Tables[6] != null && objDs.Tables[6].Rows.Count > 0)
                                {
                                    objRequisitionItem.Taxes = new List<Taxes>();
                                    var drTaxCodeDetails = objDs.Tables[6].Select("RequisitionItemId =" +
                                                            objRequisitionItem.DocumentItemId.ToString(CultureInfo.InvariantCulture));

                                    if (drTaxCodeDetails != null && drTaxCodeDetails.Length > 0)
                                    {
                                        foreach (var sqlTaxDr in drTaxCodeDetails)
                                        {
                                            var objTaxesAndCharge = new Taxes
                                            {
                                                TaxId = Convert.ToInt32(sqlTaxDr[ReqSqlConstants.COL_TAXID]),
                                                TaxDescription = Convert.ToString(sqlTaxDr[ReqSqlConstants.COL_TAX_DESC]),
                                                TaxType = (TaxType)(Convert.ToInt16(sqlTaxDr[ReqSqlConstants.COL_TAX_TYPE])),
                                                TaxMode = (SplitType)(Convert.ToInt16(sqlTaxDr[ReqSqlConstants.COL_TAX_MODE])),
                                                TaxValue = Convert.ToDecimal(sqlTaxDr[ReqSqlConstants.COL_TAX_VALUE]),
                                                TaxCode = Convert.ToString(sqlTaxDr[ReqSqlConstants.COL_TAX_CODE])
                                            };
                                            objRequisitionItem.Taxes.Add(objTaxesAndCharge);
                                        }
                                    }
                                }
                                if (objDs.Tables.Count > 7 && objDs.Tables[7].Rows != null && objDs.Tables[7].Rows.Count > 0)
                                {
                                    objRequisitionItem.lstAdditionalFieldAttributues = new List<P2PAdditionalFieldAtrribute>();
                                    var drAdditionalFieldDetails = objDs.Tables[7].Select("RequisitionItemId =" +
                                                                             objRequisitionItem.DocumentItemId.ToString(CultureInfo.InvariantCulture));
                                    if (drAdditionalFieldDetails != null && drAdditionalFieldDetails.Length > 0)
                                    {
                                        foreach (var drAdditionalField in drAdditionalFieldDetails)
                                        {
                                            var objAdditionalFields = new P2PAdditionalFieldAtrribute
                                            {
                                                AdditionalFieldCode = Convert.ToString(drAdditionalField[ReqSqlConstants.COL_ADDITIONALFIELDCODE]),
                                                AdditionalFieldValue = Convert.ToString(drAdditionalField[ReqSqlConstants.COL_ADDITIONALFIELDVALUE]),
                                                AdditionalFieldID = Convert.ToInt32(drAdditionalField[ReqSqlConstants.COL_ADDITIONALFIELDID]),
                                                AdditionalFieldDetailCode = Convert.ToInt64(drAdditionalField[ReqSqlConstants.COL_ADDITIONALFIELDDETAILCODE]),
                                                AdditionalFieldName = Convert.ToString(drAdditionalField[ReqSqlConstants.COL_ADDITIONALFIELDNAME]),
                                                FeatureId = Convert.ToInt32(drAdditionalField[ReqSqlConstants.COL_FEATUREID])
                                            };
                                            objRequisitionItem.lstAdditionalFieldAttributues.Add(objAdditionalFields);
                                        }
                                    }
                                }
                                objRequisition.RequisitionItems.Add(objRequisitionItem);
                            }

                            return objRequisition;
                        }
                    }
                }
            }
            finally
            {
                if (!ReferenceEquals(objRefCountingDataReader, null) && !objRefCountingDataReader.IsClosed)
                {
                    objRefCountingDataReader.Close();
                    objRefCountingDataReader.Dispose();
                }

                LogHelper.LogInfo(Log, "GetAllRequisitionDetailsByRequisitionId Method Ended for id=" + requisitionId);
            }

            return new Requisition();
        }

        private static void GetNotesAndAttachmentsFromDataRow(Requisition objRequisition, RequisitionItem objRequisitionItem, DataRow[] drNotesandAttachments)
        {
            try
            {
                foreach (var drNotes in drNotesandAttachments)
                {
                    var NotesOrAttachments = new NotesOrAttachments
                    {
                        NotesOrAttachmentId = (long)drNotes[ReqSqlConstants.COL_NOTES_ATTACH_ID],
                        DocumentCode = objRequisition.DocumentCode,
                        LineItemId = (long)drNotes[ReqSqlConstants.COL_NOTES_ATTACH_ITEMID],
                        FileId = (long)drNotes[ReqSqlConstants.COL_ItemImageFileId],
                        NoteOrAttachmentName = Convert.ToString(drNotes[ReqSqlConstants.COL_NOTES_ATTACH_NAME]),
                        NoteOrAttachmentDescription = Convert.ToString(drNotes[ReqSqlConstants.COL_NOTES_ATTACH_DESC]),
                        NoteOrAttachmentType = (NoteOrAttachmentType)Convert.ToInt16(drNotes[ReqSqlConstants.COL_NOTES_ATTACH_TYPE], CultureInfo.InvariantCulture),
                        AccessTypeId = (NoteOrAttachmentAccessType)Convert.ToInt16(drNotes[ReqSqlConstants.COL_NOTES_ATTACH_ACCESSTYPE], CultureInfo.InvariantCulture),
                        SourceType = (SourceType)Convert.ToInt16(drNotes[ReqSqlConstants.COL_SOURCE_TYPE], CultureInfo.InvariantCulture),
                        IsEditable = Convert.ToBoolean(drNotes[ReqSqlConstants.COL_ISEDITABLE], CultureInfo.InvariantCulture),
                        CategoryTypeId = Convert.ToInt32(drNotes[ReqSqlConstants.COL_NOTES_ATTACH_CATEGORYTYPEID], CultureInfo.InvariantCulture),
                        CreatedBy = (long)drNotes[ReqSqlConstants.COL_CREATED_BY],
                        DateCreated = Convert.IsDBNull(drNotes[ReqSqlConstants.COL_DATE_CREATED]) ? DateTime.MinValue : Convert.ToDateTime(drNotes[ReqSqlConstants.COL_DATE_CREATED], CultureInfo.InvariantCulture),
                        ModifiedBy = (long)drNotes[ReqSqlConstants.COL_MODIFIED_BY],
                        ModifiedDate = Convert.IsDBNull(drNotes[ReqSqlConstants.COL_NOTES_ATTACH_MODIFIEDDATE]) ? DateTime.MinValue : Convert.ToDateTime(drNotes[ReqSqlConstants.COL_NOTES_ATTACH_MODIFIEDDATE], CultureInfo.InvariantCulture),
                        P2PLineItemID = objRequisitionItem.P2PLineItemId
                    };
                    objRequisitionItem.ListNotesOrAttachments.Add(NotesOrAttachments);
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Requisition GetNotesAndAttachmentsFromDataRow getting Exception", ex);
            }
        }

        public List<P2PItemCheckedStatus> GetAllDocumentItemsCheckedStatus(long documentId)
        {
            throw new NotImplementedException();
        }

        public ICollection<P2PDocument> GetApprovedRejectedDocumentsForUser(long contactcode, DocumentType docType, int approvalStatus, int pageIndex, int pageSize, string sortBy, string sortOrder)
        {
            throw new NotImplementedException();
        }

        public bool GetRequisitionCapitalCodeCountById(long documentId)
        {
            var result = false;

            try
            {
                LogHelper.LogInfo(Log, "Requisition GetRequisitionCapitalCodeCountById Method Started for reqId=" + documentId);
                if (documentId > 0)
                {
                    if (Log.IsDebugEnabled)
                        Log.Debug(string.Format(CultureInfo.InvariantCulture, "Requisition GetRequisitionCapitalCodeCountById sp usp_P2P_REQ_GetRequisitionItemAccountingStatus with parameter: reqId={0} was called.", documentId));


                    int Count = Convert.ToInt32(ContextSqlConn.ExecuteScalar(ReqSqlConstants.USP_P2P_REQ_GETREQUISITIONCAPITALCODECOUNTBYID, documentId));
                    if (Count > 0)
                    {
                        result = true;
                    }
                }
                else
                {
                    if (Log.IsWarnEnabled)
                        Log.Warn("In Requisition GetRequisitionCapitalCodeCountById method reqId parameter is less than or equal to 0.");
                }
            }
            finally
            {
                LogHelper.LogInfo(Log, "Requisition GetRequisitionCapitalCodeCountById Method Ended for reqId=" + documentId);
            }
            return result;
        }
        //private DataTable ConvertRequisitionLineLevelChargeItemsToTableType(List<RequisitionItem> objRequisitionItem)
        //{
        //    DataTable dtLineLevelChargeItems = new DataTable() { Locale = CultureInfo.InvariantCulture, TableName = ReqSqlConstants.TVP_P2P_REQUISITIONLINELEVELCHARGEITEM };
        //    dtLineLevelChargeItems.Columns.Add("ChargeName", typeof(string));
        //    dtLineLevelChargeItems.Columns.Add("ChargeAmount", typeof(string));
        //    dtLineLevelChargeItems.Columns.Add("LineNumber", typeof(string));
        //    dtLineLevelChargeItems.Columns.Add("IsAllowance", typeof(bool));

        //    if (objRequisitionItem != null && objRequisitionItem.Count() > 0)
        //    {
        //        foreach (var RequisitionLineLevelChargeItem in objRequisitionItem)
        //        {
        //            if (RequisitionLineLevelChargeItem.lstLineItemCharges != null && RequisitionLineLevelChargeItem.lstLineItemCharges.Any())
        //            {
        //                foreach (var chargeItem in RequisitionLineLevelChargeItem.lstLineItemCharges)
        //                {
        //                    DataRow dr = dtLineLevelChargeItems.NewRow();
        //                    dr["ChargeName"] = chargeItem.ChargeDetails != null ? chargeItem.ChargeDetails.ChargeName : string.Empty;
        //                    dr["ChargeAmount"] = chargeItem.ChargeAmount;
        //                    dr["LineNumber"] = Convert.ToString(RequisitionLineLevelChargeItem.ItemLineNumber);
        //                    dr["IsAllowance"] = chargeItem.ChargeDetails != null ? chargeItem.ChargeDetails.IsAllowance : false;

        //                    dtLineLevelChargeItems.Rows.Add(dr);
        //                }
        //            }
        //        }
        //    }
        //    return dtLineLevelChargeItems;
        //}
        //private DataTable ConvertRequisitionHeaderChargeItemsToTableType(Requisition objRequisition)
        //{
        //    DataTable dtHeaderChargeItems = new DataTable() { Locale = CultureInfo.InvariantCulture, TableName = ReqSqlConstants.TVP_P2P_REQUISITIONHEADERCHARGEITEM };
        //    dtHeaderChargeItems.Columns.Add("ChargeName", typeof(string));
        //    dtHeaderChargeItems.Columns.Add("ChargeAmount", typeof(string));
        //    dtHeaderChargeItems.Columns.Add("LineNumber", typeof(string));
        //    dtHeaderChargeItems.Columns.Add("IsAllowance", typeof(bool));

        //    if (objRequisition.lstItemCharge != null && objRequisition.lstItemCharge.Count() > 0)
        //    {
        //        foreach (var RequisitionHeaderChargeItem in objRequisition.lstItemCharge)
        //        {
        //            if (RequisitionHeaderChargeItem != null)
        //            {
        //                DataRow dr = dtHeaderChargeItems.NewRow();
        //                dr["ChargeName"] = RequisitionHeaderChargeItem.ChargeDetails != null ? RequisitionHeaderChargeItem.ChargeDetails.ChargeName : string.Empty;
        //                dr["ChargeAmount"] = RequisitionHeaderChargeItem.ChargeAmount;
        //                dr["LineNumber"] = Convert.ToString(RequisitionHeaderChargeItem.LineNumber);
        //                dr["IsAllowance"] = RequisitionHeaderChargeItem.ChargeDetails != null ? RequisitionHeaderChargeItem.ChargeDetails.IsAllowance : false;

        //                dtHeaderChargeItems.Rows.Add(dr);
        //            }
        //        }
        //    }
        //    return dtHeaderChargeItems;
        //}
        //public List<string> ValidateInterfaceRequisition(Requisition objRequisition, Dictionary<string, string> dctSettings, bool IsOrderingLocationMandatory = false, bool IsDefaultOrderingLocation = false)
        //{
        //    LogHelper.LogInfo(Log, "Order ValidateInterfaceOrder Method Started");
        //    SqlConnection objSqlCon = null;
        //    RefCountingDataReader objRefCountingDataReader = null;
        //    var lstErrors = new List<string>();

        //    string headerEntities = string.Empty;
        //    if (objRequisition.DocumentAdditionalEntitiesInfoList != null && objRequisition.DocumentAdditionalEntitiesInfoList.Count > 0)
        //    {
        //        headerEntities = objRequisition.DocumentAdditionalEntitiesInfoList[0].EntityCode;
        //    }

        //    try
        //    {
        //        if (Log.IsDebugEnabled)
        //            Log.Debug(string.Concat("In ValidateInterfaceOrder Method.",
        //                                    "SP: USP_P2P_REQ_VALIDATEINTERFACEDOCUMENT, with parameters: orderId = " + objRequisition.DocumentCode));

        //        var sqlHelper = ContextSqlConn;
        //        objSqlCon = (SqlConnection)sqlHelper.CreateConnection();
        //        objSqlCon.Open();
        //        using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_VALIDATEINTERFACEDOCUMENT))
        //        {
        //            objSqlCommand.CommandType = CommandType.StoredProcedure;
        //            objSqlCommand.Parameters.AddWithValue("@RequisitionNumber", objRequisition.DocumentNumber);
        //            objSqlCommand.Parameters.AddWithValue("@CurrencyCode", objRequisition.Currency ?? string.Empty);
        //            objSqlCommand.Parameters.AddWithValue("@BuyerUserId", objRequisition.ClientContactCode ?? string.Empty);
        //            objSqlCommand.Parameters.AddWithValue("@IncludeTaxInSplit", Convert.ToBoolean(dctSettings["IncludeTaxInSplit"]));
        //            objSqlCommand.Parameters.AddWithValue("@RequisitionStatus", objRequisition.DocumentStatusInfo);
        //            objSqlCommand.Parameters.AddWithValue("@ShippingCharge", objRequisition.Shipping == null ? 0 : objRequisition.Shipping);
        //            objSqlCommand.Parameters.AddWithValue("@EntityCode", objRequisition.DocumentAdditionalEntitiesInfoList != null && objRequisition.DocumentAdditionalEntitiesInfoList.Any() ? objRequisition.DocumentAdditionalEntitiesInfoList.FirstOrDefault().EntityCode : "");
        //            objSqlCommand.Parameters.AddWithValue("@SourceSystemId", !ReferenceEquals(objRequisition.SourceSystemInfo, null) ? objRequisition.SourceSystemInfo.SourceSystemId : 0);
        //            objSqlCommand.Parameters.AddWithValue("@FOBCode", objRequisition.FOBCode ?? string.Empty);
        //            objSqlCommand.Parameters.AddWithValue("@FOBLocationCode", objRequisition.FOBLocationCode ?? string.Empty);
        //            objSqlCommand.Parameters.AddWithValue("@CarriersCode", objRequisition.CarriersCode ?? string.Empty);
        //            objSqlCommand.Parameters.AddWithValue("@TransitTypeCode", objRequisition.TransitTypeCode ?? string.Empty);
        //            objSqlCommand.Parameters.AddWithValue("@FreightTermsCode", objRequisition.FreightTermsCode ?? string.Empty);
        //            objSqlCommand.Parameters.AddWithValue("@LOBValue", objRequisition.DocumentLOBDetails != null ? (objRequisition.DocumentLOBDetails.FirstOrDefault().EntityCode != null ? objRequisition.DocumentLOBDetails.FirstOrDefault().EntityCode : string.Empty) : string.Empty);
        //            objSqlCommand.Parameters.AddWithValue("@BilltoLocationId", objRequisition.BilltoLocation != null ? objRequisition.BilltoLocation.BilltoLocationId : 0);
        //            objSqlCommand.Parameters.AddWithValue("@ShiptoLocationId", objRequisition.ShiptoLocation != null ? objRequisition.ShiptoLocation.ShiptoLocationId : 0);
        //            objSqlCommand.Parameters.AddWithValue("@EntityMappedToBillToLocation", Convert.ToInt64(dctSettings.ContainsKey("EntityMappedToBillToLocation") == true ? dctSettings["EntityMappedToBillToLocation"] : Convert.ToString(0)));
        //            objSqlCommand.Parameters.AddWithValue("@EntityMappedToShippingMethods", Convert.ToInt64(dctSettings.ContainsKey("EntityMappedToShippingMethods") == true ? dctSettings["EntityMappedToShippingMethods"] : Convert.ToString(0)));
        //            objSqlCommand.Parameters.AddWithValue("@EntityMappedToShipToLocation", Convert.ToInt64(dctSettings.ContainsKey("EntityMappedToShipToLocation") == true ? dctSettings["EntityMappedToShipToLocation"] : Convert.ToString(0)));
        //            objSqlCommand.Parameters.AddWithValue("@Operation", objRequisition.Operation ?? string.Empty);
        //            objSqlCommand.Parameters.AddWithValue("@SourceSystemName", !ReferenceEquals(objRequisition.SourceSystemInfo, null) ? objRequisition.SourceSystemInfo.SourceSystemName : string.Empty);

        //            SqlParameter objSqlParameter = new SqlParameter("@RequisitionItems", SqlDbType.Structured)
        //            {
        //                TypeName = ReqSqlConstants.TVP_P2P_REQUISITIONITEM,
        //                Value = ConvertRequisitionItemsToTableType(objRequisition.RequisitionItems, objRequisition.CreatedOn)
        //            };
        //            objSqlCommand.Parameters.Add(objSqlParameter);

        //            objSqlParameter = new SqlParameter("@SplitItems", SqlDbType.Structured)
        //            {
        //                TypeName = ReqSqlConstants.TVP_P2P_INTERFACESPLITITEMS,
        //                Value = ConvertRequisitionSplitsToTableType(objRequisition.RequisitionItems)
        //            };
        //            objSqlCommand.Parameters.Add(objSqlParameter);

        //            objSqlParameter = new SqlParameter("@tvp_P2P_CustomAttributes", SqlDbType.Structured)
        //            {
        //                TypeName = ReqSqlConstants.TVP_P2P_CUSTOMATTRIBUTES,
        //                Value = dctSettings.ContainsKey("CustomFieldsEnabled") && Convert.ToBoolean(dctSettings["CustomFieldsEnabled"]) == true ?
        //                 ConvertToCustomAttributesDataTable(objRequisition) : P2P.DataAccessObjects.SQLServer.SQLCommonDAO.GetCustomAttributesDataTable()
        //            };
        //            objSqlCommand.Parameters.Add(objSqlParameter);
        //            objSqlParameter = new SqlParameter("@Tvp_P2P_RequisitionHeaderChargeItem", SqlDbType.Structured)
        //            {
        //                TypeName = ReqSqlConstants.TVP_P2P_REQUISITIONHEADERCHARGEITEM,
        //                Value = ConvertRequisitionHeaderChargeItemsToTableType(objRequisition)
        //            };
        //            objSqlCommand.Parameters.Add(objSqlParameter);

        //            objSqlParameter = new SqlParameter("@Tvp_P2P_RequisitionLineLevelChargeItem", SqlDbType.Structured)
        //            {
        //                TypeName = ReqSqlConstants.TVP_P2P_REQUISITIONLINELEVELCHARGEITEM,
        //                Value = ConvertRequisitionLineLevelChargeItemsToTableType(objRequisition.RequisitionItems)
        //            };
        //            objSqlCommand.Parameters.Add(objSqlParameter);

        //            objSqlCommand.Parameters.AddWithValue("@RequesterId", objRequisition.RequesterPASCode ?? string.Empty);
        //            objSqlCommand.Parameters.AddWithValue("@PurchaseTypeDescription", objRequisition.PurchaseTypeDescription ?? string.Empty);
        //            objSqlCommand.Parameters.AddWithValue("@AllowItemNoFreeText", Convert.ToInt64(dctSettings.ContainsKey("AllowBuyerItemNoFreeText") == true ? dctSettings["AllowBuyerItemNoFreeText"] : Convert.ToString(0)));
        //            objSqlCommand.Parameters.AddWithValue("@IsClientCodeBasedonLinkLocation", Convert.ToInt64(dctSettings.ContainsKey("IsClientCodeBasedonLinkLocation") == true ? dctSettings["IsClientCodeBasedonLinkLocation"] : Convert.ToString(0)));
        //            objSqlCommand.Parameters.AddWithValue("@ItemMasterEnabled", Convert.ToInt64(dctSettings.ContainsKey("ItemMasterEnabled") == true ? dctSettings["ItemMasterEnabled"] : Convert.ToString(0)));
        //            objSqlCommand.Parameters.AddWithValue("@DeriveHeaderEntities", dctSettings.ContainsKey("DeriveHeaderEntities") == true ? dctSettings["DeriveHeaderEntities"] : string.Empty);
        //            objSqlCommand.Parameters.AddWithValue("@IsDeriveAccountingBu", Convert.ToBoolean(dctSettings.ContainsKey("IsDeriveAccountingBu") == true ? dctSettings["IsDeriveAccountingBu"] : Convert.ToString(false)));
        //            objSqlCommand.Parameters.AddWithValue("@IsDeriveItemDetailEnable", Convert.ToBoolean(dctSettings.ContainsKey("DeriveItemDetails") == true ? dctSettings["DeriveItemDetails"] : Convert.ToString(false)));
        //            objSqlCommand.Parameters.AddWithValue("@UseDocumentLOB", Convert.ToBoolean(dctSettings.ContainsKey("UseDocumentLOB") == true ? dctSettings["UseDocumentLOB"] : Convert.ToString(false)));
        //            objSqlCommand.Parameters.AddWithValue("@DerivePartnerFromLocationCode", Convert.ToBoolean(dctSettings.ContainsKey("DerivePartnerFromLocationCode") == true ? dctSettings["DerivePartnerFromLocationCode"] : Convert.ToString(false)));
        //            objSqlCommand.Parameters.AddWithValue("@AllowReqForRfxAndOrder", Convert.ToBoolean(dctSettings.ContainsKey("AllowReqForRfxAndOrder") == true ? dctSettings["AllowReqForRfxAndOrder"] : Convert.ToString(false)));
        //            objSqlCommand.Parameters.AddWithValue("@IsOrderingLocationMandatory", IsOrderingLocationMandatory);
        //            objSqlCommand.Parameters.AddWithValue("@IsDefaultOrderingLocation", IsDefaultOrderingLocation);
        //            objSqlCommand.Parameters.AddWithValue("@reqHeaderEntities", headerEntities);
        //            objSqlParameter = new SqlParameter("@RequisitionItemAdditionalFields", SqlDbType.Structured)
        //            {
        //                TypeName = ReqSqlConstants.TVP_P2P_BZ_REQUISITIONITEMADDITIONALFIELDS,
        //                Value = ConvertRequisitionAdditionalFieldAtrributeToTableType(objRequisition.RequisitionItems)
        //            };
        //            objSqlCommand.Parameters.Add(objSqlParameter);

        //            objRefCountingDataReader = (RefCountingDataReader)sqlHelper.ExecuteReader(objSqlCommand);
        //            if (objRefCountingDataReader != null)
        //            {
        //                var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;
        //                while (sqlDr.Read())
        //                    lstErrors.Add(GetStringValue(sqlDr, ReqSqlConstants.COL_ERROR_MESSAGE));
        //            }
        //        }
        //    }
        //    finally
        //    {
        //        if (!ReferenceEquals(objRefCountingDataReader, null) && !objRefCountingDataReader.IsClosed)
        //        {
        //            objRefCountingDataReader.Close();
        //            objRefCountingDataReader.Dispose();
        //        }
        //        if (!ReferenceEquals(objSqlCon, null) && objSqlCon.State != ConnectionState.Closed)
        //        {
        //            objSqlCon.Close();
        //            objSqlCon.Dispose();
        //        }
        //        LogHelper.LogInfo(Log, "Reqisition ValidateInterfaceReqisition Method Ended");
        //    }
        //    return lstErrors;
        //}

        //private DataTable ConvertRequisitionItemsToTableType(List<RequisitionItem> lstRequisitionItem, DateTime DateRequested)
        //{
        //    DataTable dtRequisitionItem = new DataTable() { Locale = CultureInfo.InvariantCulture, TableName = ReqSqlConstants.TVP_P2P_REQUISITIONITEM };
        //    dtRequisitionItem.Columns.Add("ItemLineNumber", typeof(int));
        //    dtRequisitionItem.Columns.Add("ItemNumber", typeof(string));
        //    dtRequisitionItem.Columns.Add("UOM", typeof(string));
        //    dtRequisitionItem.Columns.Add("ClientCategoryId", typeof(string));
        //    dtRequisitionItem.Columns.Add("Currency", typeof(string));
        //    dtRequisitionItem.Columns.Add("ClientPartnerCode", typeof(string));
        //    dtRequisitionItem.Columns.Add("ShippingMethod", typeof(string));
        //    dtRequisitionItem.Columns.Add("Quantity", typeof(decimal));
        //    dtRequisitionItem.Columns.Add("UnitPrice", typeof(decimal));
        //    dtRequisitionItem.Columns.Add("Tax", typeof(decimal));
        //    dtRequisitionItem.Columns.Add("AdditionalCharges", typeof(decimal));
        //    dtRequisitionItem.Columns.Add("ShippingCharges", typeof(decimal));
        //    dtRequisitionItem.Columns.Add("ItemType", typeof(int));
        //    dtRequisitionItem.Columns.Add("ShipToLocationId", typeof(int));
        //    dtRequisitionItem.Columns.Add("DateRequested", typeof(DateTime));
        //    dtRequisitionItem.Columns.Add("DateNeeded", typeof(DateTime));
        //    dtRequisitionItem.Columns.Add("ClientContactCode", typeof(string));
        //    dtRequisitionItem.Columns.Add("OrderingLocationCode", typeof(string));
        //    dtRequisitionItem.Columns.Add("Unspsc", typeof(int));
        //    if (lstRequisitionItem != null)
        //    {
        //        foreach (var reqisitionItem in lstRequisitionItem)
        //        {
        //            DataRow dr = dtRequisitionItem.NewRow();
        //            dr["ItemLineNumber"] = reqisitionItem.ItemLineNumber;
        //            dr["ItemNumber"] = reqisitionItem.ItemNumber ?? string.Empty;
        //            dr["UOM"] = reqisitionItem.UOM;
        //            dr["ClientCategoryId"] = reqisitionItem.ClientCategoryId;
        //            dr["Currency"] = reqisitionItem.Currency;
        //            dr["ClientPartnerCode"] = reqisitionItem.ClientPartnerCode;
        //            dr["ShippingMethod"] = reqisitionItem.DocumentItemShippingDetails != null && reqisitionItem.DocumentItemShippingDetails.Any() ? reqisitionItem.DocumentItemShippingDetails.FirstOrDefault().ShippingMethod : string.Empty;
        //            dr["Quantity"] = reqisitionItem.Quantity;
        //            dr["UnitPrice"] = reqisitionItem.UnitPrice == null ? DBNull.Value : (object)reqisitionItem.UnitPrice;
        //            dr["Tax"] = reqisitionItem.Tax == null ? DBNull.Value : (object)reqisitionItem.Tax;
        //            dr["AdditionalCharges"] = reqisitionItem.AdditionalCharges == null ? DBNull.Value : (object)reqisitionItem.AdditionalCharges;
        //            dr["ShippingCharges"] = reqisitionItem.ShippingCharges == null ? DBNull.Value : (object)reqisitionItem.ShippingCharges;
        //            dr["ItemType"] = reqisitionItem.ItemType;
        //            dr["ShipToLocationId"] = reqisitionItem.DocumentItemShippingDetails != null && reqisitionItem.DocumentItemShippingDetails.Count() > 0 ? (reqisitionItem.DocumentItemShippingDetails[0].ShiptoLocation != null ? reqisitionItem.DocumentItemShippingDetails[0].ShiptoLocation.ShiptoLocationId : 0) : 0;
        //            dr["DateRequested"] = DateRequested == null || DateRequested == DateTime.MinValue ? DBNull.Value : (object)DateRequested;
        //            dr["DateNeeded"] = reqisitionItem.DateNeeded == null || reqisitionItem.DateNeeded == DateTime.MinValue ? DBNull.Value : (object)reqisitionItem.DateNeeded;
        //            dr["ClientContactCode"] = reqisitionItem.ClientContactCode;
        //            dr["OrderingLocationCode"] = !string.IsNullOrEmpty(reqisitionItem.OrderLocationName) ? reqisitionItem.OrderLocationName : string.Empty;
        //            dr["Unspsc"] = reqisitionItem.Unspsc;
        //            dtRequisitionItem.Rows.Add(dr);
        //        }
        //    }
        //    return dtRequisitionItem;
        //}

        //private DataTable ConvertRequisitionSplitsToTableType(List<RequisitionItem> lstRequisitionItem)
        //{
        //    DataTable dtSplitItem = new DataTable() { Locale = CultureInfo.InvariantCulture, TableName = ReqSqlConstants.TVP_P2P_INTERFACESPLITITEMS };
        //    dtSplitItem.Columns.Add("ItemLineNumber", typeof(int));
        //    dtSplitItem.Columns.Add("EntityCode", typeof(string));
        //    dtSplitItem.Columns.Add("EntityType", typeof(string));
        //    dtSplitItem.Columns.Add("SplitItemTotal", typeof(decimal));
        //    dtSplitItem.Columns.Add("Uids", typeof(int));
        //    if (lstRequisitionItem != null)
        //    {
        //        foreach (var reqisitionItem in lstRequisitionItem)
        //        {
        //            if (reqisitionItem != null && reqisitionItem.ItemSplitsDetail != null)
        //            {
        //                foreach (var splitItem in reqisitionItem.ItemSplitsDetail)
        //                {
        //                    if (splitItem != null)
        //                    {
        //                        foreach (var splitEntity in splitItem.DocumentSplitItemEntities)
        //                        {
        //                            if (splitEntity.EntityType != null && splitEntity.EntityCode != null)
        //                            {
        //                                DataRow dr = dtSplitItem.NewRow();
        //                                dr["ItemLineNumber"] = reqisitionItem.ItemLineNumber;
        //                                dr["EntityCode"] = splitEntity.EntityCode;
        //                                dr["EntityType"] = splitEntity.EntityType;
        //                                dr["SplitItemTotal"] = splitItem.SplitItemTotal == null ? DBNull.Value : (object)splitItem.SplitItemTotal;
        //                                dr["Uids"] = splitItem.UiId > 0 ? splitItem.UiId : 0;
        //                                dtSplitItem.Rows.Add(dr);
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    return dtSplitItem;
        //}
        public bool SaveContractInformation(long requisitionItemId, string extContractRef)
        {
            long RequisitionId = 0;
            bool flag = false;
            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;
            try
            {
                LogHelper.LogInfo(Log, "Requisition SaveContractInformation Method Started for requisitionItemId = " + requisitionItemId);

                var sqlHelper = ContextSqlConn;
                _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();

                using (var objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_SAVECONTRACTINFORMATION, _sqlCon))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.AddWithValue("@RequisitionItemId", requisitionItemId);
                    objSqlCommand.Parameters.AddWithValue("@ExtContractRef", extContractRef);
                    RequisitionId = Convert.ToInt64(ContextSqlConn.ExecuteScalar(objSqlCommand), CultureInfo.InvariantCulture);
                }
                if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
                {
                    _sqlTrans.Commit();

                    if (this.UserContext.Product != GEPSuite.eInterface && RequisitionId > 0)
                    {
                        flag = true;
                        AddIntoSearchIndexerQueueing(RequisitionId, (int)DocumentType.Requisition, UserContext, GepConfiguration);
                    }
                }
            }
            catch
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
                throw;
            }
            finally
            {
                if (!ReferenceEquals(_sqlCon, null) && _sqlCon.State != ConnectionState.Closed)
                {
                    _sqlCon.Close();
                    _sqlCon.Dispose();
                }
                LogHelper.LogInfo(Log, "Requisition SaveContractInformation Method Ended for requisitionItemId = " + requisitionItemId);
            }

            return flag;
        }

        public ICollection<KeyValuePair<long, long>> GetAllPartnerCodeAndOrderinglocationId(long documentId)
        {
            RefCountingDataReader objRefCountingDataReader = null;

            try
            {
                LogHelper.LogInfo(Log, "Requisition GetAllPartnerCodeAndOrderinglocationId Method Started for documentId=" + documentId);
                if (documentId <= 0)
                    if (Log.IsWarnEnabled)
                        Log.Warn("In Requisition GetAllPartnerCodeAndOrderinglocationId method documentId parameter is less then or equal to 0.");

                if (documentId > 0)
                {
                    if (Log.IsDebugEnabled)
                        Log.Debug(string.Concat("Requisition GetAllPartnerCodeAndOrderinglocationId sp usp_P2P_REQ_GetAllPartnersById with parameter:", " documentId=" + documentId +
                                                             " was called."));

                    objRefCountingDataReader =
                     (RefCountingDataReader)
                     ContextSqlConn.ExecuteReader(ReqSqlConstants.USP_P2P_REQ_GETALLPARTNERCODEANDORDERINGLOCATIONID,
                                                                      new object[] { documentId });
                    if (objRefCountingDataReader != null)
                    {
                        var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;
                        var lstKvpPartnerDetails = new List<KeyValuePair<long, long>>();
                        while (sqlDr.Read())
                            lstKvpPartnerDetails.Add(new KeyValuePair<long, long>(GetLongValue(sqlDr, ReqSqlConstants.COL_PARTNER_CODE), GetLongValue(sqlDr, ReqSqlConstants.COL_ORDERLOCATIONID)));
                        return lstKvpPartnerDetails;
                    }
                }
            }
            finally
            {
                if (!ReferenceEquals(objRefCountingDataReader, null) && !objRefCountingDataReader.IsClosed)
                {
                    objRefCountingDataReader.Close();
                    objRefCountingDataReader.Dispose();
                }
                LogHelper.LogInfo(Log, "Requisition GetAllPartnerCodeAndOrderinglocationId Method Ended for documentId=" + documentId);
            }

            return new List<KeyValuePair<long, long>>();
        }



        public ICollection<NewP2PEntities.PartnerSpendControlDocumentMapping> GetAllPartnerCodeOrderinglocationIdNadSpendControlItemId(long documentId)
        {
            RefCountingDataReader objRefCountingDataReader = null;

            try
            {
                LogHelper.LogInfo(Log, "Requisition GetAllPartnerCodeOrderinglocationIdNadSpendControlItemId Method Started for documentId=" + documentId);
                if (documentId <= 0)
                    if (Log.IsWarnEnabled)
                        Log.Warn("In Requisition GetAllPartnerCodeOrderinglocationIdNadSpendControlItemId method documentId parameter is less then or equal to 0.");

                if (documentId > 0)
                {
                    if (Log.IsDebugEnabled)
                        Log.Debug(string.Concat("Requisition GetAllPartnerCodeOrderinglocationIdNadSpendControlItemId sp usp_P2P_REQ_GetAllPartnerCodeOrderinglocationIdAndSpendControlItemId with parameter:", " documentId=" + documentId +
                                                             " was called."));

                    objRefCountingDataReader =
                     (RefCountingDataReader)
                     ContextSqlConn.ExecuteReader(ReqSqlConstants.USP_P2P_REQ_GETALLPARTNERCODEORDERINGLOCATIONIDANDSPENDCONTROLITEMID,
                                                                      new object[] { documentId });
                    if (objRefCountingDataReader != null)
                    {
                        var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;
                        var lstPartnerSpendControlDetails = new List<NewP2PEntities.PartnerSpendControlDocumentMapping>();
                        while (sqlDr.Read())
                            lstPartnerSpendControlDetails.Add(new NewP2PEntities.PartnerSpendControlDocumentMapping()
                            {
                                PartnerCode = GetLongValue(sqlDr, ReqSqlConstants.COL_PARTNER_CODE),
                                OrderLocationId = GetLongValue(sqlDr, ReqSqlConstants.COL_ORDERLOCATIONID),
                                SpendControlDocumentCode = GetLongValue(sqlDr, ReqSqlConstants.COL_SPENDCONTROLDOCUMENTCODE)
                            });
                        return lstPartnerSpendControlDetails;
                    }
                }
            }
            finally
            {
                if (!ReferenceEquals(objRefCountingDataReader, null) && !objRefCountingDataReader.IsClosed)
                {
                    objRefCountingDataReader.Close();
                    objRefCountingDataReader.Dispose();
                }
                LogHelper.LogInfo(Log, "Requisition GetAllPartnerCodeOrderinglocationIdNadSpendControlItemId Method Ended for documentId=" + documentId);
            }

            return new List<NewP2PEntities.PartnerSpendControlDocumentMapping>();
        }

        public List<KeyValuePair<long, int>> GetListErrorCodesByOrderIds(List<long> lstDocumentCode, bool isOrderingLocationMandatory)
        {
            RefCountingDataReader objRefCountingDataReader = null;

            try
            {
                LogHelper.LogInfo(Log, "Requisition GetListErrorCodesByOrderIds Method Started for ");
                if (lstDocumentCode.Count() <= 0)
                    if (Log.IsWarnEnabled)
                        Log.Warn("In Requisition GetListErrorCodesByOrderIds method documentId parameter is less then or equal to 0.");

                if (lstDocumentCode.Count() > 0)
                {
                    if (Log.IsDebugEnabled)
                        Log.Debug(string.Concat("Requisition GetListErrorCodesByOrderIds sp usp_P2P_REQ_GetAllPartnersById with was called."));
                    DataTable dtDocumentCode = new DataTable();
                    dtDocumentCode.Columns.Add("DocumentCode");

                    lstDocumentCode.ForEach(x => dtDocumentCode.Rows.Add(x));

                    using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_GETLISTERRORCODESBYORDERIDS))
                    {
                        objSqlCommand.CommandType = CommandType.StoredProcedure;
                        objSqlCommand.Parameters.Add(new SqlParameter("@tvp_P2P_DocumentCode", SqlDbType.Structured)
                        {
                            TypeName = ReqSqlConstants.TVP_P2P_DOCUMENTCODE,
                            Value = dtDocumentCode
                        });
                        objSqlCommand.Parameters.Add(new SqlParameter("@isOrderingLocationMandatory", isOrderingLocationMandatory));
                        objRefCountingDataReader = (RefCountingDataReader)ContextSqlConn.ExecuteReader(objSqlCommand);
                    }

                    if (objRefCountingDataReader != null)
                    {
                        var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;
                        var lstKvpPartnerDetails = new List<KeyValuePair<long, int>>();
                        while (sqlDr.Read())
                            lstKvpPartnerDetails.Add(new KeyValuePair<long, int>(GetLongValue(sqlDr, ReqSqlConstants.COL_DOCUMENT_CODE), GetIntValue(sqlDr, ReqSqlConstants.COL_ERRORCODE)));
                        return lstKvpPartnerDetails;
                    }
                }
            }
            finally
            {
                if (!ReferenceEquals(objRefCountingDataReader, null) && !objRefCountingDataReader.IsClosed)
                {
                    objRefCountingDataReader.Close();
                    objRefCountingDataReader.Dispose();
                }
                LogHelper.LogInfo(Log, "Requisition GetListErrorCodesByOrderIds Method Ended for documentId");
            }

            return new List<KeyValuePair<long, int>>();
        }

        private List<KeyValuePair<Type, string>> GetDocumentCodeTable()
        {
            var lstDocumentCodeTable = new List<KeyValuePair<Type, string>>();
            lstDocumentCodeTable.Add(new KeyValuePair<Type, string>(typeof(long), "DocumentCode"));
            return lstDocumentCodeTable;
        }

        public List<PartnerDetails> GetPartnerDetailsAndOrderingLocationByOrderId(long RequisitionId)
        {
            RefCountingDataReader objRefCountingDataReader = null;
            List<PartnerDetails> lstpartner = new List<PartnerDetails>();
            try
            {
                LogHelper.LogInfo(Log, "Requisition GetPartnerDetailsAndOrderingLocationByOrderId Method Started for documentId=" + RequisitionId);

                if (Log.IsWarnEnabled)
                    Log.Warn("In Requisition GetPartnerDetailsAndOrderingLocationByOrderId method documentId parameter is less then or equal to 0.");


                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("Requisition GetPartnerDetailsAndOrderingLocationByOrderId sp usp_P2P_REQ_GetAllPartnersById with parameter:", " documentId=" + RequisitionId +
                                                         " was called."));


                objRefCountingDataReader =
                 (RefCountingDataReader)
                 ContextSqlConn.ExecuteReader(ReqSqlConstants.USP_P2P_REQ_GETALLPARTNERCODEANDORDERINGLOCATION,
                                                                  new object[] { RequisitionId });
                if (objRefCountingDataReader != null)
                {
                    var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;

                    while (sqlDr.Read())
                    {
                        var objPartnerDetails = new PartnerDetails();
                        objPartnerDetails.PartnerLocations = new List<PartnerLocation>();
                        objPartnerDetails.PartnerCode = GetLongValue(sqlDr, ReqSqlConstants.COL_PARTNER_CODE);
                        objPartnerDetails.LegalCompanyName = GetStringValue(sqlDr, ReqSqlConstants.COL_PARTNER_NAME);
                        objPartnerDetails.DefaultCurrencyCode = GetStringValue(sqlDr, ReqSqlConstants.COL_CURRENCY);
                        objPartnerDetails.PartnerLocations.Add(new PartnerLocation
                        {
                            LocationId = GetLongValue(sqlDr, ReqSqlConstants.COL_LOCATIONID),
                            LocationName = GetStringValue(sqlDr, ReqSqlConstants.OL_COL_LocationName)
                        });
                        lstpartner.Add(objPartnerDetails);
                    }
                    return lstpartner;
                }
            }
            finally
            {
                if (!ReferenceEquals(objRefCountingDataReader, null) && !objRefCountingDataReader.IsClosed)
                {
                    objRefCountingDataReader.Close();
                    objRefCountingDataReader.Dispose();
                }
                LogHelper.LogInfo(Log, "Requisition GetListErrorCodesByOrderIds Method Ended for documentId=" + RequisitionId);
            }

            return new List<PartnerDetails>();
        }

        public bool CheckHasRules(BusinessCase businessCase, int documentTypeId)
        {
            throw new NotImplementedException();
        }

        public ExportRequisition RequisitionExportById(long requisitionId, long contactCode, int userType, string accessType, bool accessTypeComment, bool isAcc = false, int maxPrecessionValue = 0, int maxPrecessionValueForTotal = 0, int maxPrecessionValueForTaxAndCharges = 0, string enableDispatchMode = null)
        {
            var dsRequisitionExport = new ExportRequisition();
            dsRequisitionExport.DocumentDataSet = new P2PDocumentDataSet();
            dsRequisitionExport.RequisitionDataSet = new RequisitionDataSet();

            dsRequisitionExport.DocumentDataSet.Tables.Add(new DataTable(ReqSqlConstants.REQUISITIONITEMADDITIONALFIELDDETAILS));
            dsRequisitionExport.DocumentDataSet.Tables[ReqSqlConstants.REQUISITIONITEMADDITIONALFIELDDETAILS].Columns.Add(ReqSqlConstants.COL_REQUISITION_ITEM_ID, typeof(long));
            dsRequisitionExport.DocumentDataSet.Tables[ReqSqlConstants.REQUISITIONITEMADDITIONALFIELDDETAILS].Columns.Add(ReqSqlConstants.COL_ADDITIONALFIELDID, typeof(int));
            dsRequisitionExport.DocumentDataSet.Tables[ReqSqlConstants.REQUISITIONITEMADDITIONALFIELDDETAILS].Columns.Add(ReqSqlConstants.COL_ADDITIONALFIELDNAME, typeof(string));
            dsRequisitionExport.DocumentDataSet.Tables[ReqSqlConstants.REQUISITIONITEMADDITIONALFIELDDETAILS].Columns.Add(ReqSqlConstants.COL_ADDITIONALFIELDVALUE, typeof(string));
            dsRequisitionExport.DocumentDataSet.Tables[ReqSqlConstants.REQUISITIONITEMADDITIONALFIELDDETAILS].Columns.Add(ReqSqlConstants.COL_ADDITIONALFIELDCODE, typeof(string));
            dsRequisitionExport.DocumentDataSet.Tables[ReqSqlConstants.REQUISITIONITEMADDITIONALFIELDDETAILS].Columns.Add(ReqSqlConstants.COL_ADDITIONALFIELDDETAILCODE, typeof(long));
            dsRequisitionExport.DocumentDataSet.Tables[ReqSqlConstants.REQUISITIONITEMADDITIONALFIELDDETAILS].Columns.Add(ReqSqlConstants.COL_FEATUREID, typeof(int));
            dsRequisitionExport.DocumentDataSet.Tables[ReqSqlConstants.REQUISITIONITEMADDITIONALFIELDDETAILS].Columns.Add(ReqSqlConstants.COL_DATA_DISPLAY_STYLE, typeof(int));
            dsRequisitionExport.DocumentDataSet.Tables[ReqSqlConstants.REQUISITIONITEMADDITIONALFIELDDETAILS].Columns.Add(ReqSqlConstants.COL_FIELDCONTROLTYPE, typeof(int));
            string cultureCode = UserContext.Culture;

            var objParams = new object[11];
            objParams[0] = requisitionId;
            objParams[1] = contactCode;
            objParams[2] = userType;
            objParams[3] = accessType;
            objParams[4] = accessTypeComment;
            objParams[5] = isAcc;
            objParams[6] = maxPrecessionValue;
            objParams[7] = maxPrecessionValueForTotal;
            objParams[8] = maxPrecessionValueForTaxAndCharges;
            objParams[9] = enableDispatchMode;
            objParams[10] = cultureCode;

            var tableNames = new string[7];
            tableNames[0] = dsRequisitionExport.DocumentDataSet.Tables[0].TableName;
            tableNames[1] = dsRequisitionExport.DocumentDataSet.Tables[1].TableName;
            tableNames[2] = dsRequisitionExport.DocumentDataSet.Tables[7].TableName;
            tableNames[3] = dsRequisitionExport.DocumentDataSet.Tables[2].TableName;
            tableNames[4] = dsRequisitionExport.DocumentDataSet.Tables[3].TableName;
            tableNames[5] = dsRequisitionExport.DocumentDataSet.Tables[9].TableName; //accounting details
            tableNames[6] = dsRequisitionExport.DocumentDataSet.Tables[10].TableName; //Additional field details


            ContextSqlConn.LoadDataSet(ReqSqlConstants.USP_P2P_PO_GETREQUISITIONDETAILSFOREXPORTPDFBYID, dsRequisitionExport.DocumentDataSet, tableNames,
                                   objParams);

            return dsRequisitionExport;
        }

        #region Purchase Request Details
        /// <summary>
        /// Get List of Purchase Request Form Questionnaire sets
        /// </summary>
        /// <param name="requisitionItemId">requisitionItemId</param>
        /// <returns>set of Questionnaire in List</returns>
        public List<Questionnaire> GetAllQuestionnaire(long requisitionItemId)
        {
            RefCountingDataReader objRefCountingDataReader = null;
            List<Questionnaire> lstQuestionnaire = new List<Questionnaire>();
            try
            {
                LogHelper.LogInfo(Log, "GetAllQuestionnaire Method Started.");

                var sqlHelper = ContextSqlConn;
                objRefCountingDataReader = (RefCountingDataReader)sqlHelper.ExecuteReader(ReqSqlConstants.USP_P2P_REQ_GETALLQUESTIONNAIRE, new object[] { requisitionItemId, UserContext.Culture });
                if (objRefCountingDataReader != null)
                {
                    var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;
                    while (sqlDr.Read())
                    {
                        Questionnaire objnewQuestionnaire = new Questionnaire()
                        {
                            QuestionnaireCode = GetLongValue(sqlDr, ReqSqlConstants.COL_QUESTIONNAIRECODE),
                            QuestionnaireTitle = GetStringValue(sqlDr, ReqSqlConstants.COL_QUESTIONNAIRETITLE),
                            QuestionnaireOrder = GetIntValue(sqlDr, ReqSqlConstants.COL_QUESTIONNAIREORDER),
                            IsSupplierVisible = GetBoolValue(sqlDr, ReqSqlConstants.COL_ISSUPPLIERVISIBLE),
                            IsInformative = GetBoolValue(sqlDr, ReqSqlConstants.COL_ISINFORMATIVE),
                            Weightage = GetDoubleValue(sqlDr, ReqSqlConstants.COL_WEIGHTAGE)
                        };
                        lstQuestionnaire.Add(objnewQuestionnaire);
                    }
                }
                return lstQuestionnaire;
            }
            finally
            {
                if (!ReferenceEquals(objRefCountingDataReader, null) && !objRefCountingDataReader.IsClosed)
                {
                    objRefCountingDataReader.Close();
                    objRefCountingDataReader.Dispose();
                }

                LogHelper.LogInfo(Log, "GetAllQuestionnaire Method Ended.");
            }
        }
        #endregion
        public DataTable GetListofShipToLocDetails(string searchText, int pageIndex, int pageSize, bool getByID, int shipToLocID, long lOBEntityDetailCode, long entityDetailCode)
        {
            SqlCommand objSqlCommand = null;
            try
            {
                objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_GetListOfShipToLocations);
                objSqlCommand.CommandType = CommandType.StoredProcedure;
                objSqlCommand.Parameters.Add(new SqlParameter("@searchText", searchText));
                objSqlCommand.Parameters.Add(new SqlParameter("@pageIndex", pageIndex));
                objSqlCommand.Parameters.Add(new SqlParameter("@pageSize", pageSize));
                objSqlCommand.Parameters.Add(new SqlParameter("@byID", getByID));
                objSqlCommand.Parameters.Add(new SqlParameter("@shipToLocID", shipToLocID));
                objSqlCommand.Parameters.Add(new SqlParameter("@LOBEntityDetailCode", lOBEntityDetailCode));
                objSqlCommand.Parameters.Add(new SqlParameter("@EntityDetailCode", entityDetailCode));
                return ContextSqlConn.ExecuteDataSet(objSqlCommand).Tables[0];
            }
            finally
            {
                objSqlCommand.Dispose();
            }
        }



        public bool SaveAdvancePaymentDefaultAccountingDetails(long requisitionId, List<DocumentSplitItemEntity> requisitionSplitItemEntities, bool saveDefaultGL = false)
        {

            bool flag = false;
            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;
            try
            {
                LogHelper.LogInfo(Log, "Requisition SaveAdvancePaymentDefaultAccountingDetails Method Started for requisitionId = " + requisitionId);

                var sqlHelper = ContextSqlConn;
                _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();

                DataTable dtReqItemEntities = null;
                dtReqItemEntities = P2P.DataAccessObjects.DAOHelper.ConvertToDataTable(requisitionSplitItemEntities,
                                                       GetRequisitionSplitItemEntitiesTable);
                using (var objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_SAVE_REQ_ADVANCEDPAYMENT_DEFAULT_ACCOUNTINGDETAILS, _sqlCon))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.CommandTimeout = 150;
                    objSqlCommand.Parameters.AddWithValue("@RequisitionId", requisitionId);
                    objSqlCommand.Parameters.Add(new SqlParameter("@tvp_P2P_SplitItemsEntities", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_P2P_SPLITITEMSENTITIES,
                        Value = dtReqItemEntities
                    });
                    objSqlCommand.Parameters.AddWithValue("@UserId", UserContext.ContactCode);
                    objSqlCommand.Parameters.AddWithValue("@SaveDefaultGL", saveDefaultGL);
                    flag = Convert.ToBoolean(ContextSqlConn.ExecuteScalar(objSqlCommand), CultureInfo.InvariantCulture);
                }
                if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
                    _sqlTrans.Commit();

            }
            catch
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
                throw;
            }
            finally
            {
                if (!ReferenceEquals(_sqlCon, null) && _sqlCon.State != ConnectionState.Closed)
                {
                    _sqlCon.Close();
                    _sqlCon.Dispose();
                }
                LogHelper.LogInfo(Log, "Requisition SaveAdvancePaymentDefaultAccountingDetails Method Ended for requisitionId = " + requisitionId);
            }

            return flag;
        }
        public DataTable GetListofBillToLocDetails(string searchText, int pageIndex, int pageSize, long entityDetailCode, bool getDefault, long lOBEntityDetailCode = 0)
        {
            SqlConnection objSqlCon = null;
            DataTable dtBillToLocations = null;

            try
            {
                objSqlCon = (SqlConnection)ContextSqlConn.CreateConnection();
                objSqlCon.Open();
                using (var objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_GetListOfBillToLocations))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.Add(new SqlParameter("@searchText", searchText));
                    objSqlCommand.Parameters.Add(new SqlParameter("@pageIndex", pageIndex));
                    objSqlCommand.Parameters.Add(new SqlParameter("@pageSize", pageSize));
                    objSqlCommand.Parameters.Add(new SqlParameter("@entitydetailcode", entityDetailCode));
                    objSqlCommand.Parameters.Add(new SqlParameter("@getDefault", getDefault));
                    objSqlCommand.Parameters.Add(new SqlParameter("@lOBEntityDetailCode", lOBEntityDetailCode));
                    dtBillToLocations = ContextSqlConn.ExecuteDataSet(objSqlCommand).Tables[0];
                }
                return dtBillToLocations;
            }
            finally
            {
                if (!ReferenceEquals(objSqlCon, null) && objSqlCon.State != ConnectionState.Closed)
                {
                    objSqlCon.Close();
                    objSqlCon.Dispose();
                }
                dtBillToLocations.Dispose();
            }
        }

        public DataTable CheckCatalogItemAccessForContactCode(long requisitionId, long requesterId)
        {
            SqlConnection objSqlCon = null;
            DataTable dtCatalogItemIDs = new DataTable();
            try
            {
                objSqlCon = (SqlConnection)ContextSqlConn.CreateConnection();
                objSqlCon.Open();
                using (var objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_GetListOfCatalogItemIdsNotAllowedAccess))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.Add(new SqlParameter("@requisitionId", requisitionId));
                    objSqlCommand.Parameters.Add(new SqlParameter("@requesterId", requesterId));
                    dtCatalogItemIDs = ContextSqlConn.ExecuteDataSet(objSqlCommand).Tables[0];
                }
                return dtCatalogItemIDs;
            }
            finally
            {
                if (!ReferenceEquals(objSqlCon, null) && objSqlCon.State != ConnectionState.Closed)
                {
                    objSqlCon.Close();
                    objSqlCon.Dispose();
                }
                dtCatalogItemIDs.Dispose();
            }
        }
        private DataTable ConvertSettingsToTableTypes(List<KeyValuePair<string, string>> lstSettingValue)
        {

            DataTable dtOrderSettingValue = new DataTable() { Locale = CultureInfo.InvariantCulture, TableName = ReqSqlConstants.TVP_P2P_SETTINGVALUES };
            dtOrderSettingValue.Columns.Add("SettingKeyName", typeof(string));
            dtOrderSettingValue.Columns.Add("SettingKeyValue", typeof(string));

            foreach (var obj in lstSettingValue)
            {
                DataRow dr = dtOrderSettingValue.NewRow();
                dr["SettingKeyName"] = obj.Key;
                dr["SettingKeyValue"] = obj.Value;
                dtOrderSettingValue.Rows.Add(dr);
            }

            return dtOrderSettingValue;
        }
        public P2PDocument GetDocumentAdditionalEntityDetailsById(long documentCode)
        {
            RefCountingDataReader objRefCountingDataReader = null;
            SqlDataReader sqlDr = null;
            Requisition objRequisition = new Requisition();
            objRequisition.DocumentAdditionalEntitiesInfoList = new Collection<DocumentAdditionalEntityInfo>();
            long LOBEntityDetailCode = 0;
            try
            {
                objRefCountingDataReader = (RefCountingDataReader)ContextSqlConn.ExecuteReader(ReqSqlConstants.USP_P2P_REQ_GETREQUISITIONENTITYDETAILSBYID, new object[] { documentCode });

                if (objRefCountingDataReader != null)
                {
                    sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;
                    while (sqlDr.Read())
                    {
                        objRequisition.DocumentAdditionalEntitiesInfoList.Add(new DocumentAdditionalEntityInfo
                        {
                            EntityDetailCode = GetLongValue(sqlDr, ReqSqlConstants.COL_SPLIT_ITEM_ENTITYDETAILCODE),
                            EntityId = GetIntValue(sqlDr, ReqSqlConstants.COL_ENTITY_ID),
                            EntityDisplayName = GetStringValue(sqlDr, ReqSqlConstants.COL_ENTITY_DISPLAY_NAME),
                            EntityCode = GetStringValue(sqlDr, ReqSqlConstants.COL_ENTITY_CODE),
                            EntityType = GetStringValue(sqlDr, ReqSqlConstants.COL_ENTITY_TYPE)
                        });
                        LOBEntityDetailCode = GetLongValue(sqlDr, ReqSqlConstants.COL_LOBENTITYDETAILCODE);
                    }
                    objRequisition.EntityDetailCode = new List<long>() { LOBEntityDetailCode };
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured while Requisition Save Method  for DocumentNumber = " + objRequisition.DocumentNumber, ex);
                throw;
            }
            finally
            {
                if (!ReferenceEquals(sqlDr, null) && !sqlDr.IsClosed)
                {
                    sqlDr.Close();
                    sqlDr.Dispose();
                }
                if (!ReferenceEquals(objRefCountingDataReader, null) && !objRefCountingDataReader.IsClosed)
                {
                    objRefCountingDataReader.Close();
                    objRefCountingDataReader.Dispose();
                }
                LogHelper.LogInfo(Log, "GetDocumentAdditionalEntityDetailsById Method Ended for documentCode=" + documentCode);
            }
            return objRequisition;
        }
        public long SaveItem(List<P2PItem> objItems, int precessionValue, bool IsInterface = false, int MaxPrecessionValueTotal = 0, int MaxPrecessionValueForTaxAndCharges = 0, bool addLinesFromRequisition = false)
        {
            throw new NotImplementedException();
        }

        public long SaveItemOtherDetails(List<P2PItem> lstOrderItems)
        {
            throw new NotImplementedException();
        }
        //public BZRequisition GetRequisitionHeaderDetailsByIdForInterface(long reqId, bool deliverToFreeText = false)
        //{
        //    BZRequisition objBZRequisition = new BZRequisition();
        //    objBZRequisition.Requisition = new Requisition();
        //    var objRequisition = objBZRequisition.Requisition;

        //    try
        //    {
        //        LogHelper.LogInfo(Log, "GetRequisitionHeaderDetailsByIdForInterface Method Started for Requisition Id=" + reqId + " and deliverToFreeText=" + deliverToFreeText);
        //        if (reqId > 0)
        //        {
        //            if (Log.IsDebugEnabled)
        //                Log.Debug(string.Concat("In GetRequisitionHeaderDetailsByIdForInterface Method",
        //                                         "SP: usp_P2P_REQ_GetHeaderDetailsByIdForInterface with parameter: RequisitionId=" + reqId + " and deliverToFreeText=" + deliverToFreeText));

        //            var resultDataSet =
        //             ContextSqlConn.ExecuteDataSet(ReqSqlConstants.USP_P2P_REQ_GETREQHEADERDETAILSBYID_FOR_INTERFACE,
        //                                                                  new object[] { reqId, deliverToFreeText });

        //            if (resultDataSet.Tables.Count > 0 && resultDataSet.Tables[0].Rows.Count > 0)
        //            {
        //                #region Basic
        //                DataRow requisitionHeaderRow = resultDataSet.Tables[0].Rows[0];
        //                objRequisition.DocumentId = Convert.ToInt64(requisitionHeaderRow[ReqSqlConstants.COL_REQUISITION_ID]);
        //                objRequisition.DocumentCode = Convert.ToInt64(requisitionHeaderRow[ReqSqlConstants.COL_DOCUMENT_CODE]);
        //                objRequisition.DocumentName = Convert.ToString(requisitionHeaderRow[ReqSqlConstants.COL_DOCUMENT_NAME]);
        //                objRequisition.DocumentNumber = Convert.ToString(requisitionHeaderRow[ReqSqlConstants.COL_DOCUMENT_NUMBER]);
        //                objRequisition.ClientContactCode = Convert.ToString(requisitionHeaderRow[ReqSqlConstants.COL_CONTACT_CODE]);
        //                objRequisition.DocumentStatusInfo = (DocumentStatus)Convert.ToInt32(requisitionHeaderRow[ReqSqlConstants.COL_DOCUMENT_STATUS]);
        //                objRequisition.DocumentTypeInfo = (DocumentType)Convert.ToInt32(requisitionHeaderRow[ReqSqlConstants.COL_DOCUMENT_TYPE]);
        //                objRequisition.DocumentSourceTypeInfo = (DocumentSourceType)Convert.ToInt16(requisitionHeaderRow[ReqSqlConstants.COL_DOCUMENT_SOURCE]);
        //                objRequisition.CreatedOn = Convert.ToDateTime(requisitionHeaderRow[ReqSqlConstants.COL_DATE_CREATED]);
        //                objRequisition.UpdatedOn = Convert.ToDateTime(requisitionHeaderRow[ReqSqlConstants.COL_DATE_MODIFIED]);
        //                objRequisition.IsDeleted = Convert.ToBoolean(requisitionHeaderRow[ReqSqlConstants.COL_IS_DELETED]);
        //                objRequisition.ModifiedBy = Convert.ToInt64(requisitionHeaderRow[ReqSqlConstants.COL_CREATED_BY]);
        //                objRequisition.CreatedBy = Convert.ToInt64(requisitionHeaderRow[ReqSqlConstants.COL_CREATED_BY]);
        //                objRequisition.Currency = Convert.ToString(requisitionHeaderRow[ReqSqlConstants.COL_CURRENCY]);
        //                objRequisition.Tax = !Convert.IsDBNull(requisitionHeaderRow[ReqSqlConstants.COL_TAX]) ? Convert.ToDecimal(requisitionHeaderRow[ReqSqlConstants.COL_TAX]) : default(decimal?);
        //                objRequisition.Shipping = !Convert.IsDBNull(requisitionHeaderRow[ReqSqlConstants.COL_SHIPPING]) ? Convert.ToDecimal(requisitionHeaderRow[ReqSqlConstants.COL_SHIPPING]) : default(decimal?);
        //                objRequisition.AdditionalCharges = !Convert.IsDBNull(requisitionHeaderRow[ReqSqlConstants.COL_ADDITIONAL_CHARGES]) ? Convert.ToDecimal(requisitionHeaderRow[ReqSqlConstants.COL_ADDITIONAL_CHARGES]) : default(decimal?);
        //                objRequisition.ItemTotalAmount = !Convert.IsDBNull(requisitionHeaderRow[ReqSqlConstants.COL_ITEMTOTAL]) ? Convert.ToDecimal(requisitionHeaderRow[ReqSqlConstants.COL_ITEMTOTAL]) : default(decimal?);
        //                objRequisition.TotalAmount = !Convert.IsDBNull(requisitionHeaderRow[ReqSqlConstants.COL_REQUISITION_AMOUNT]) ? Convert.ToDecimal(requisitionHeaderRow[ReqSqlConstants.COL_REQUISITION_AMOUNT]) : default(decimal?);
        //                objRequisition.RequisitionSource = (RequisitionSource)Convert.ToInt32(requisitionHeaderRow[ReqSqlConstants.COL_REQUISITION_SOURCE]);
        //                objRequisition.WorkOrderNumber = Convert.ToString(requisitionHeaderRow[ReqSqlConstants.COL_WORKORDERNO]);
        //                objRequisition.ERPOrderTypeName = !Convert.IsDBNull(requisitionHeaderRow[ReqSqlConstants.COL_ERPORDERTYPE]) ? Convert.ToString(requisitionHeaderRow[ReqSqlConstants.COL_ERPORDERTYPE]) : string.Empty;
        //                objRequisition.ShiptoLocation = new ShiptoLocation
        //                {
        //                    ShiptoLocationId = Convert.ToInt32(requisitionHeaderRow[ReqSqlConstants.COL_SHIPTOLOC_ID])
        //                };
        //                objRequisition.CreatedByName = Convert.ToString(requisitionHeaderRow[ReqSqlConstants.COL_REQUESTER_NAME]);
        //                objRequisition.DelivertoLocation = new DelivertoLocation()
        //                {
        //                    DelivertoLocationId = Convert.ToInt32(requisitionHeaderRow[ReqSqlConstants.COL_DELIVERTOLOC_ID]),
        //                    DeliverTo = Convert.ToString(requisitionHeaderRow[ReqSqlConstants.COL_DELIVERTO])
        //                };
        //                objRequisition.SourceSystemInfo = new SourceSystemInfo
        //                {
        //                    SourceSystemId = Convert.ToInt32(requisitionHeaderRow[ReqSqlConstants.COL_SOURCESYSTEMID]),
        //                    SourceSystemName = Convert.ToString(requisitionHeaderRow[ReqSqlConstants.COL_SOURCESYSTEMNAME])
        //                };
        //                objRequisition.CustomAttrFormIdForItem = Convert.ToInt64(requisitionHeaderRow[ReqSqlConstants.COL_ITEMFORMID]);
        //                objRequisition.CustomAttrFormId = Convert.ToInt64(requisitionHeaderRow[ReqSqlConstants.COL_FORMID]);
        //                objRequisition.CustomAttrFormIdForSplit = Convert.ToInt64(requisitionHeaderRow[ReqSqlConstants.COL_SPLITFORMID]);
        //                objRequisition.PurchaseTypeDescription = Convert.ToString(requisitionHeaderRow[ReqSqlConstants.COL_PURCHASETYPE]);
        //                objRequisition.Billable = !Convert.IsDBNull(requisitionHeaderRow[ReqSqlConstants.COL_BILLABLE]) ? Convert.ToBoolean(requisitionHeaderRow[ReqSqlConstants.COL_BILLABLE]) : (bool?)null;
        //                objRequisition.IsUrgent = Convert.ToBoolean(requisitionHeaderRow[ReqSqlConstants.COL_ISURGENT]);
        //                #endregion

        //                #region Document BU
        //                if (resultDataSet.Tables.Count > 1 && resultDataSet.Tables[1].Rows.Count > 0)
        //                {
        //                    DocumentBU objDocumentBU = null;

        //                    foreach (DataRow docBUDataRow in resultDataSet.Tables[1].Rows)
        //                    {
        //                        objDocumentBU = new DocumentBU();
        //                        objDocumentBU.BusinessUnitCode = Convert.ToInt64(docBUDataRow[ReqSqlConstants.COL_BUCODE]);
        //                        objDocumentBU.BusinessUnitName = Convert.ToString(docBUDataRow[ReqSqlConstants.COL_BUSINESSUNITNAME]);
        //                        objRequisition.DocumentBUList.Add(objDocumentBU);
        //                    }
        //                }
        //                #endregion

        //                #region Document LOB
        //                if (resultDataSet.Tables.Count > 2 && resultDataSet.Tables[2].Rows.Count > 0)
        //                {
        //                    objRequisition.EntityDetailCode = new List<long>();
        //                    objRequisition.DocumentLOBDetails = new List<DocumentLOBDetails>();
        //                    foreach (DataRow requisitionLOBDataRow in resultDataSet.Tables[2].Rows)
        //                    {
        //                        objRequisition.EntityId = Convert.ToInt32(requisitionLOBDataRow[ReqSqlConstants.COL_ENTITY_ID]);
        //                        objRequisition.EntityDetailCode.Add(Convert.ToInt64(requisitionLOBDataRow[ReqSqlConstants.COL_ENTITYDETAILCODE]));
        //                        objRequisition.DocumentLOBDetails.Add(new DocumentLOBDetails() { EntityCode = Convert.ToString(requisitionLOBDataRow[ReqSqlConstants.COL_ENTITY_CODE]) });
        //                    }
        //                }
        //                #endregion

        //                #region "Header Level Entity Details"
        //                if (resultDataSet.Tables.Count > 3 && resultDataSet.Tables[3].Rows.Count > 0)
        //                {
        //                    objRequisition.DocumentAdditionalEntitiesInfoList = new Collection<DocumentAdditionalEntityInfo>();

        //                    foreach (DataRow requisitionHeaderEntityDataRow in resultDataSet.Tables[3].Rows)
        //                    {
        //                        DocumentAdditionalEntityInfo docAddEntityInfo = new DocumentAdditionalEntityInfo()
        //                        {
        //                            EntityDetailCode = Convert.ToInt64(requisitionHeaderEntityDataRow[ReqSqlConstants.COL_SPLIT_ITEM_ENTITYDETAILCODE]),
        //                            EntityId = Convert.ToInt32(requisitionHeaderEntityDataRow[ReqSqlConstants.COL_ENTITY_ID]),
        //                            EntityDisplayName = Convert.ToString(requisitionHeaderEntityDataRow[ReqSqlConstants.COL_ENTITY_DISPLAY_NAME]),
        //                            EntityCode = Convert.ToString(requisitionHeaderEntityDataRow[ReqSqlConstants.COL_ENTITY_CODE])
        //                        };

        //                        objRequisition.DocumentAdditionalEntitiesInfoList.Add(docAddEntityInfo);
        //                    }
        //                }
        //                #endregion "Header Level Entity Details"

        //                #region "Ship To Location Details"
        //                if (resultDataSet.Tables.Count > 4 && resultDataSet.Tables[4].Rows.Count > 0)
        //                {
        //                    DataRow reqShipToLocatioRow = resultDataSet.Tables[4].Rows[0];

        //                    objRequisition.ShiptoLocation = new ShiptoLocation()
        //                    {
        //                        ShiptoLocationId = Convert.ToInt32(reqShipToLocatioRow[ReqSqlConstants.COL_SHIPTOLOC_ID]),
        //                        ShiptoLocationName = Convert.ToString(reqShipToLocatioRow[ReqSqlConstants.COL_SHIPTOLOC_NAME]),
        //                        ShiptoLocationNumber = Convert.ToString(reqShipToLocatioRow[ReqSqlConstants.COL_SHIPTOLOC_NUMBER]),
        //                        Address = new P2PAddress()
        //                        {
        //                            AddressLine1 = Convert.ToString(reqShipToLocatioRow[ReqSqlConstants.COL_ADDRESS1]),
        //                            AddressLine2 = Convert.ToString(reqShipToLocatioRow[ReqSqlConstants.COL_ADDRESS2]),
        //                            City = Convert.ToString(reqShipToLocatioRow[ReqSqlConstants.COL_CITY]),
        //                            State = Convert.ToString(reqShipToLocatioRow[ReqSqlConstants.COL_STATE]),
        //                            Zip = Convert.ToString(reqShipToLocatioRow[ReqSqlConstants.COL_ZIP]),
        //                            AddressId = Convert.ToInt64(reqShipToLocatioRow[ReqSqlConstants.COL_ADDRESSID]),
        //                            StateCode = Convert.ToString(reqShipToLocatioRow[ReqSqlConstants.COL_STATECODE]),
        //                            CountryCode = Convert.ToString(reqShipToLocatioRow[ReqSqlConstants.COL_COUNTRYCODE]),
        //                            Country = Convert.ToString(reqShipToLocatioRow[ReqSqlConstants.COL_COUNTRYCODE])
        //                        },
        //                        IsAdhoc = Convert.ToBoolean(reqShipToLocatioRow[ReqSqlConstants.COL_SHIPTOLOC_ISADHOC]),
        //                        AllowForFutureReference = Convert.ToBoolean(reqShipToLocatioRow[ReqSqlConstants.COL_SHIPTOLOC_ALLOWFORFUTUREREFERENCE]),
        //                        ContactPerson = Convert.ToString(reqShipToLocatioRow[ReqSqlConstants.COL_SHIPTOLOC_CONTACTPERSON]),
        //                    };
        //                }
        //                #endregion

        //                #region "Deliver To Location Details"
        //                if (resultDataSet.Tables.Count > 6 && resultDataSet.Tables[6].Rows.Count > 0)
        //                {
        //                    DataRow reqDeliverToLocatioRow = resultDataSet.Tables[6].Rows[0];

        //                    objRequisition.DelivertoLocation = new DelivertoLocation()
        //                    {
        //                        DelivertoLocationId = Convert.ToInt32(reqDeliverToLocatioRow[ReqSqlConstants.COL_DELIVERTOLOC_ID]),
        //                        DelivertoLocationName = Convert.ToString(reqDeliverToLocatioRow[ReqSqlConstants.COL_DELIVERTOLOC_NAME]),
        //                        DelivertoLocationNumber = Convert.ToString(reqDeliverToLocatioRow[ReqSqlConstants.COL_DELIVERTOLOC_NUMBER]),
        //                        Address = new P2PAddress()
        //                        {
        //                            AddressLine1 = Convert.ToString(reqDeliverToLocatioRow[ReqSqlConstants.COL_ADDRESS1]),
        //                            AddressLine2 = Convert.ToString(reqDeliverToLocatioRow[ReqSqlConstants.COL_ADDRESS2]),
        //                            City = Convert.ToString(reqDeliverToLocatioRow[ReqSqlConstants.COL_CITY]),
        //                            State = Convert.ToString(reqDeliverToLocatioRow[ReqSqlConstants.COL_STATE]),
        //                            Zip = Convert.ToString(reqDeliverToLocatioRow[ReqSqlConstants.COL_ZIP]),
        //                            AddressId = Convert.ToInt64(reqDeliverToLocatioRow[ReqSqlConstants.COL_ADDRESSID]),
        //                            StateCode = Convert.ToString(reqDeliverToLocatioRow[ReqSqlConstants.COL_STATECODE]),
        //                            CountryCode = Convert.ToString(reqDeliverToLocatioRow[ReqSqlConstants.COL_COUNTRYCODE])
        //                        }
        //                    };
        //                }
        //                #endregion

        //                #region "Bill To Location Details"
        //                if (resultDataSet.Tables.Count > 7 && resultDataSet.Tables[7].Rows.Count > 0)
        //                {
        //                    DataRow reqBillToLocatioRow = resultDataSet.Tables[7].Rows[0];

        //                    objRequisition.BilltoLocation = new BilltoLocation()
        //                    {
        //                        BilltoLocationId = Convert.ToInt32(reqBillToLocatioRow[ReqSqlConstants.COL_BILLTOLOC_ID]),
        //                        BilltoLocationName = Convert.ToString(reqBillToLocatioRow[ReqSqlConstants.COL_BILLTOLOC_NAME]),
        //                        BilltoLocationNumber = Convert.ToString(reqBillToLocatioRow[ReqSqlConstants.COL_BILLTOLOC_NUMBER]),
        //                        Address = new P2PAddress()
        //                        {
        //                            AddressLine1 = Convert.ToString(reqBillToLocatioRow[ReqSqlConstants.COL_ADDRESS1]),
        //                            AddressLine2 = Convert.ToString(reqBillToLocatioRow[ReqSqlConstants.COL_ADDRESS2]),
        //                            City = Convert.ToString(reqBillToLocatioRow[ReqSqlConstants.COL_CITY]),
        //                            State = Convert.ToString(reqBillToLocatioRow[ReqSqlConstants.COL_STATE]),
        //                            Zip = Convert.ToString(reqBillToLocatioRow[ReqSqlConstants.COL_ZIP]),
        //                            AddressId = Convert.ToInt64(reqBillToLocatioRow[ReqSqlConstants.COL_ADDRESSID]),
        //                            StateCode = Convert.ToString(reqBillToLocatioRow[ReqSqlConstants.COL_STATECODE]),
        //                            CountryCode = Convert.ToString(reqBillToLocatioRow[ReqSqlConstants.COL_COUNTRYCODE]),
        //                            FaxNo = Convert.ToString(reqBillToLocatioRow[ReqSqlConstants.BILLTO_COL_FAXNO]),
        //                            EmailAddress = Convert.ToString(reqBillToLocatioRow[ReqSqlConstants.BILLTO_COL_EmailAddress])
        //                        },
        //                        IsDefault = !Convert.IsDBNull(reqBillToLocatioRow[ReqSqlConstants.COL_ISDEFAULT]) ? Convert.ToBoolean(reqBillToLocatioRow[ReqSqlConstants.COL_ISDEFAULT]) : default(Boolean),
        //                        IsDeleted = Convert.ToBoolean(reqBillToLocatioRow[ReqSqlConstants.COL_IS_DELETED]),
        //                    };
        //                }
        //                #endregion

        //                #region "Buyer Contact Details"
        //                if (resultDataSet.Tables.Count > 9 && resultDataSet.Tables[9].Rows.Count > 0)
        //                {
        //                    DataRow reqBuyerContactRow = resultDataSet.Tables[9].Rows[0];

        //                    objBZRequisition.BuyerContact = new P2PContact()
        //                    {
        //                        FirstName = Convert.ToString(reqBuyerContactRow[ReqSqlConstants.COL_FIRSTNAME]),
        //                        LastName = Convert.ToString(reqBuyerContactRow[ReqSqlConstants.COL_LASTNAME]),
        //                        ContactCode = Convert.ToInt64(reqBuyerContactRow[ReqSqlConstants.COL_CONTACT_CODE]),
        //                        EmailAddress = Convert.ToString(reqBuyerContactRow[ReqSqlConstants.COL_EMAIL_ID]),
        //                        Address = new Address()
        //                        {
        //                            ExtenstionNo1 = Convert.ToString(reqBuyerContactRow[ReqSqlConstants.COL_EXTENSION_NO1]),
        //                            ExtenstionNo2 = Convert.ToString(reqBuyerContactRow[ReqSqlConstants.COL_EXTENSION_NO2]),
        //                            MobileNo = Convert.ToString(reqBuyerContactRow[ReqSqlConstants.COL_MOBILE_NO]),
        //                            PhoneNo1 = Convert.ToString(reqBuyerContactRow[ReqSqlConstants.COL_PHONE_NO1]),
        //                            PhoneNo2 = Convert.ToString(reqBuyerContactRow[ReqSqlConstants.COL_PHONE_NO2])
        //                        }
        //                    };
        //                    objBZRequisition.BuyerContact.UserType = P2PUserType.Buyer;
        //                }
        //                #endregion

        //                return objBZRequisition;
        //            }
        //        }
        //        else
        //        {
        //            if (Log.IsWarnEnabled)
        //                Log.Warn("In GetRequisitionHeaderDetailsByIdForInterface Method Parameter: RequisitionId should be greater than 0.");
        //        }
        //    }
        //    finally
        //    {
        //        LogHelper.LogInfo(Log, "GetRequisitionHeaderDetailsByIdForInterface Method Ended for RequisitionId = " + reqId + " and deliverToFreeText=" + deliverToFreeText);
        //    }
        //    return new BZRequisition();
        //}

        //public List<RequisitionItem> GetLineItemBasicDetailsForInterface(long documentCode)
        //{
        //    Decimal tempDecimal;
        //    List<RequisitionItem> objReqItemList = new List<RequisitionItem>();
        //    try
        //    {
        //        LogHelper.LogInfo(Log, "Requisition GetLineItemBasicDetailsForInterface Method Started for id=" + documentCode);
        //        if (documentCode > 0)
        //        {
        //            if (Log.IsDebugEnabled)
        //                Log.Debug(string.Concat("In Requisition GetLineItemBasicDetails Method",
        //                                         "SP: usp_P2P_REQ_GetLineItemsForInterface with parameter: documentCode=" + documentCode + " was called."));

        //            var reqItemsDataSet = ContextSqlConn.ExecuteDataSet(ReqSqlConstants.USP_P2P_REQ_GETREQLINEITEMS_FOR_INTERFACE,
        //                                                                 new object[] { documentCode });


        //            if (reqItemsDataSet.Tables.Count > 0 && reqItemsDataSet.Tables[0].Rows.Count > 0)
        //            {
        //                foreach (DataRow reqItemDataRow in reqItemsDataSet.Tables[0].Rows)
        //                {
        //                    var objRequisitionItem = new RequisitionItem();

        //                    #region Basic Line Level Details
        //                    objRequisitionItem.DocumentItemId = Convert.ToInt64(reqItemDataRow[ReqSqlConstants.COL_REQUISITION_ITEM_ID]);
        //                    objRequisitionItem.P2PLineItemId = Convert.ToInt64(reqItemDataRow[ReqSqlConstants.COL_P2P_LINE_ITEM_ID]);
        //                    objRequisitionItem.DocumentId = Convert.ToInt64(reqItemDataRow[ReqSqlConstants.COL_REQUISITION_ID]);
        //                    objRequisitionItem.ShortName = Convert.ToString(reqItemDataRow[ReqSqlConstants.COL_SHORT_NAME]);
        //                    objRequisitionItem.Description = Convert.ToString(reqItemDataRow[ReqSqlConstants.COL_DESCRIPTION]);
        //                    objRequisitionItem.UnitPrice = Decimal.TryParse(reqItemDataRow[ReqSqlConstants.COL_UNIT_PRICE].ToString(), out tempDecimal) ? tempDecimal : default(decimal?);
        //                    objRequisitionItem.Quantity = Convert.ToDecimal(reqItemDataRow[ReqSqlConstants.COL_QUANTITY]);
        //                    objRequisitionItem.UOM = Convert.ToString(reqItemDataRow[ReqSqlConstants.COL_UOM]);
        //                    objRequisitionItem.DateRequested = !Convert.IsDBNull(reqItemDataRow[ReqSqlConstants.COL_DATE_REQUESTED]) ? Convert.ToDateTime(reqItemDataRow[ReqSqlConstants.COL_DATE_REQUESTED]) : default(DateTime?);
        //                    objRequisitionItem.DateNeeded = !Convert.IsDBNull(reqItemDataRow[ReqSqlConstants.COL_DATE_NEEDED]) ? Convert.ToDateTime(reqItemDataRow[ReqSqlConstants.COL_DATE_NEEDED]) : default(DateTime?);
        //                    objRequisitionItem.CategoryId = Convert.ToInt64(reqItemDataRow[ReqSqlConstants.COL_CATEGORY_ID]);
        //                    objRequisitionItem.ItemType = (ItemType)Convert.ToInt16(reqItemDataRow[ReqSqlConstants.COL_ITEM_TYPE_ID]);
        //                    objRequisitionItem.CreatedBy = Convert.ToInt64(reqItemDataRow[ReqSqlConstants.COL_CREATED_BY]);
        //                    objRequisitionItem.ItemStatus = (DocumentStatus)Convert.ToInt16(reqItemDataRow[ReqSqlConstants.COL_ITEM_STATUS]);
        //                    objRequisitionItem.Currency = Convert.ToString(reqItemDataRow[ReqSqlConstants.COL_CURRENCY]);
        //                    objRequisitionItem.StartDate = !Convert.IsDBNull(reqItemDataRow[ReqSqlConstants.COL_START_DATE]) ? Convert.ToDateTime(reqItemDataRow[ReqSqlConstants.COL_START_DATE]) : default(DateTime?);
        //                    objRequisitionItem.EndDate = !Convert.IsDBNull(reqItemDataRow[ReqSqlConstants.COL_END_DATE]) ? Convert.ToDateTime(reqItemDataRow[ReqSqlConstants.COL_END_DATE]) : default(DateTime?);
        //                    objRequisitionItem.AdditionalCharges = Decimal.TryParse(reqItemDataRow[ReqSqlConstants.COL_ADDITIONAL_CHARGES].ToString(), out tempDecimal) ? tempDecimal : default(decimal?);
        //                    objRequisitionItem.ShippingCharges = Decimal.TryParse(reqItemDataRow[ReqSqlConstants.COL_SHIPPING_CHARGES].ToString(), out tempDecimal) ? tempDecimal : default(decimal?);
        //                    objRequisitionItem.Tax = Decimal.TryParse(reqItemDataRow[ReqSqlConstants.COL_LINE_ITEM_TAX].ToString(), out tempDecimal) ? tempDecimal : default(decimal?);
        //                    objRequisitionItem.SourceType = (ItemSourceType)Convert.ToInt16(reqItemDataRow[ReqSqlConstants.COL_SOURCE_TYPE]);
        //                    objRequisitionItem.ItemCode = Convert.ToInt64(reqItemDataRow[ReqSqlConstants.COL_ITEM_CODE]);
        //                    objRequisitionItem.CatalogItemId = !Convert.IsDBNull(reqItemDataRow[ReqSqlConstants.COL_CATALOGITEMID]) ? Convert.ToInt64(reqItemDataRow[ReqSqlConstants.COL_CATALOGITEMID]) : default(Int64);
        //                    objRequisitionItem.SupplierPartId = Convert.ToString(reqItemDataRow[ReqSqlConstants.COL_SUPPLIERPARTID]);
        //                    objRequisitionItem.SupplierPartAuxiliaryId = Convert.ToString(reqItemDataRow[ReqSqlConstants.COL_SUPPLIERAUXILIARYPARTID]);
        //                    objRequisitionItem.ItemExtendedType = (ItemExtendedType)Convert.ToInt16(reqItemDataRow[ReqSqlConstants.COL_ITEM_EXTENDED_TYPE]);
        //                    objRequisitionItem.ItemNumber = Convert.ToString(reqItemDataRow[ReqSqlConstants.COL_ITEMNUMBER]);
        //                    objRequisitionItem.ItemLineNumber = Convert.ToInt64(reqItemDataRow[ReqSqlConstants.COL_ITEMLINENUMBER]);
        //                    objRequisitionItem.ItemTotalAmount = (Decimal.TryParse(reqItemDataRow[ReqSqlConstants.COL_UNIT_PRICE].ToString(), out tempDecimal) ? tempDecimal : default(decimal?)) * Convert.ToDecimal(reqItemDataRow[ReqSqlConstants.COL_QUANTITY]);
        //                    objRequisitionItem.PartnerCode = !Convert.IsDBNull(reqItemDataRow[ReqSqlConstants.COL_PARTNER_CODE]) ? Convert.ToDecimal(reqItemDataRow[ReqSqlConstants.COL_PARTNER_CODE]) : default(decimal);
        //                    objRequisitionItem.PartnerName = Convert.ToString(reqItemDataRow[ReqSqlConstants.COL_PARTNER_NAME]);
        //                    objRequisitionItem.CategoryName = Convert.ToString(reqItemDataRow[ReqSqlConstants.COL_CATEGORY_NAME]);
        //                    objRequisitionItem.RecoupmentPercentage = Decimal.TryParse(reqItemDataRow[ReqSqlConstants.COL_RECOUPMENTPERCENTAGE].ToString(), out tempDecimal) ? tempDecimal : default(decimal);
        //                    objRequisitionItem.ManufacturerName = Convert.ToString(reqItemDataRow[ReqSqlConstants.COL_MANUFACTURER_NAME]);
        //                    objRequisitionItem.ManufacturerPartNumber = Convert.ToString(reqItemDataRow[ReqSqlConstants.COL_MANUFACTURER_PART_NUMBER]);
        //                    objRequisitionItem.ClientPartnerCode = Convert.ToString(reqItemDataRow[ReqSqlConstants.COL_CLIENT_PARTNERCODE]);
        //                    objRequisitionItem.InventoryType = Convert.IsDBNull(reqItemDataRow[ReqSqlConstants.COL_INVENTORYTYPE]) ? false : Convert.ToBoolean(reqItemDataRow[ReqSqlConstants.COL_INVENTORYTYPE]);
        //                    #endregion Basic Line Level Details

        //                    objReqItemList.Add(objRequisitionItem);
        //                }
        //            }
        //            return objReqItemList;
        //        }
        //        else
        //        {
        //            if (Log.IsWarnEnabled)
        //                Log.Warn("In Requisition GetLineItemBasicDetailsForInterface Method Parameter: Document Code should be greater than 0.");
        //        }
        //    }
        //    finally
        //    {
        //        LogHelper.LogInfo(Log, "Requisition GetLineItemBasicDetailsForInterface Method Ended for id=" + documentCode);
        //    }
        //    return new List<RequisitionItem>();
        //}

        //public DataSet GetSplitsDetails(List<RequisitionItem> RequisitionItem, long ContactCode, long lobEntityDetailCode, string EntityCode = null)
        //{
        //    DataSet UpdatedResult = new DataSet();
        //    SqlConnection objSqlCon = null;
        //    var lstErrors = new List<string>();
        //    try
        //    {
        //        if (Log.IsDebugEnabled)
        //            Log.Debug("In SaveRequisition Method  GetSplitsDetails Method Started ");

        //        var sqlHelper = ContextSqlConn;
        //        objSqlCon = (SqlConnection)sqlHelper.CreateConnection();
        //        objSqlCon.Open();

        //        using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_GETSPLITDETAILS))
        //        {
        //            objSqlCommand.CommandType = CommandType.StoredProcedure;
        //            SqlParameter objSqlParameter = new SqlParameter("@SplitItems", SqlDbType.Structured)
        //            {
        //                TypeName = ReqSqlConstants.TVP_P2P_INTERFACESPLITITEMS,
        //                Value = ConvertREQSplitsToTableType(RequisitionItem)
        //            };
        //            objSqlCommand.Parameters.Add(objSqlParameter);
        //            objSqlCommand.Parameters.AddWithValue("@ContactCode", Convert.ToString(ContactCode));
        //            objSqlCommand.Parameters.AddWithValue("LOBEntityDetailCode", lobEntityDetailCode);
        //            objSqlCommand.Parameters.AddWithValue("@EntityCode", EntityCode);

        //            UpdatedResult = sqlHelper.ExecuteDataSet(objSqlCommand);
        //            UpdatedResult.Tables[0].TableName = "SplitEntity";
        //        }


        //    }
        //    catch
        //    {

        //        throw;
        //    }
        //    finally
        //    {
        //        LogHelper.LogInfo(Log, " GetSplitsDetails Method Ended");

        //        if (objSqlCon != null && objSqlCon.State == ConnectionState.Open)
        //            objSqlCon.Close();

        //    }
        //    return UpdatedResult;

        //}

        //private DataTable ConvertREQSplitsToTableType(List<RequisitionItem> lstRequisitionItem)
        //{
        //    int splitCounters = 1;
        //    DataTable dtSplitItem = new DataTable() { Locale = CultureInfo.InvariantCulture, TableName = ReqSqlConstants.TVP_P2P_INTERFACESPLITITEMS };
        //    dtSplitItem.Columns.Add("ItemLineNumber", typeof(long));
        //    dtSplitItem.Columns.Add("EntityCode", typeof(string));
        //    dtSplitItem.Columns.Add("EntityType", typeof(string));
        //    dtSplitItem.Columns.Add("SplitItemTotal", typeof(decimal));
        //    dtSplitItem.Columns.Add("Uids", typeof(int));

        //    if (lstRequisitionItem != null)
        //    {
        //        foreach (var RequisitionItem in lstRequisitionItem)
        //        {
        //            if (RequisitionItem != null && RequisitionItem.ItemSplitsDetail != null)
        //            {
        //                splitCounters = 1;

        //                foreach (var splitItem in RequisitionItem.ItemSplitsDetail)
        //                {
        //                    if (splitItem != null)
        //                    {
        //                        foreach (var splitEntity in splitItem.DocumentSplitItemEntities)
        //                        {
        //                            if (splitEntity.EntityType != null && splitEntity.EntityCode != null)
        //                            {
        //                                DataRow dr = dtSplitItem.NewRow();
        //                                dr["ItemLineNumber"] = RequisitionItem.ItemLineNumber;
        //                                dr["EntityCode"] = splitEntity.EntityCode;
        //                                dr["EntityType"] = splitEntity.EntityType;
        //                                dr["SplitItemTotal"] = splitItem.SplitItemTotal == null ? DBNull.Value : (object)splitItem.SplitItemTotal;
        //                                dr["Uids"] = splitCounters;
        //                                dtSplitItem.Rows.Add(dr);
        //                            }
        //                        }
        //                    }
        //                    splitCounters++;
        //                }
        //            }
        //        }
        //    }
        //    return dtSplitItem;
        //}

        //public DataSet ValidateShipToBillToFromInterface(Requisition objRequisition, bool shipToLocSetting, bool billToLocSetting, bool deliverToFreeText, long LobentitydetailCode,
        //        bool IsDefaultBillToLocation, long entityDetailCode)
        //{
        //    DataSet UpdatedResult = new DataSet();
        //    SqlConnection objSqlCon = null;
        //    var lstErrors = new List<string>();
        //    try
        //    {
        //        if (Log.IsDebugEnabled)
        //            Log.Debug(string.Concat("In ValidateShipToBillToFromInterface Method.",
        //                                    "SP: usp_P2P_ValidateInterfaceDocument, with parameters: orderId = " + objRequisition.DocumentCode));
        //        string entitycode = objRequisition.DocumentAdditionalEntitiesInfoList != null ? objRequisition.DocumentAdditionalEntitiesInfoList.Where(a => !string.IsNullOrWhiteSpace(a.EntityCode)).FirstOrDefault().EntityCode : "";
        //        var sqlHelper = ContextSqlConn;
        //        objSqlCon = (SqlConnection)sqlHelper.CreateConnection();
        //        objSqlCon.Open();

        //        using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_VALIDATESHIPTOBILLTOFROMINTERFACE))
        //        {
        //            objSqlCommand.CommandType = CommandType.StoredProcedure;
        //            SqlParameter objSqlParameter = new SqlParameter("@BillToHeader", SqlDbType.Structured)
        //            {
        //                TypeName = ReqSqlConstants.TVP_P2P_HEADERBILLTO,
        //                Value = ConvertRequisitionBillToLocToTableType(objRequisition.BilltoLocation)
        //            };
        //            objSqlCommand.Parameters.Add(objSqlParameter);
        //            objSqlParameter = new SqlParameter("@HeaderShipTo", SqlDbType.Structured)
        //            {
        //                TypeName = ReqSqlConstants.TVP_P2P_HEADERSHIPTO,
        //                Value = P2P.DataAccessObjects.DAOTVPHelper.ConvertOrderShipToLocToTableType(objRequisition.ShiptoLocation)
        //            };

        //            objSqlCommand.Parameters.Add(objSqlParameter);
        //            objSqlParameter = new SqlParameter("@LinelevelShipTo", SqlDbType.Structured)
        //            {
        //                TypeName = ReqSqlConstants.TVP_P2P_LINELEVELSHIPTO,
        //                Value = ConvertRequisitionLineItemShipToLocToTableType(objRequisition.RequisitionItems)
        //            };

        //            objSqlCommand.Parameters.Add(objSqlParameter);
        //            objSqlParameter = new SqlParameter("@DeliverToHeader", SqlDbType.Structured)
        //            {
        //                TypeName = ReqSqlConstants.TVP_P2P_HEADERDELIVERTO,
        //                Value = ConvertRequisitionDeliverToLocToTableType(objRequisition.DelivertoLocation)
        //            };

        //            objSqlCommand.Parameters.Add(objSqlParameter);
        //            objSqlParameter = new SqlParameter("@linelevelDeliverTo", SqlDbType.Structured)
        //            {
        //                TypeName = ReqSqlConstants.TVP_P2P_LINELEVELDELIVERTO,
        //                Value = ConvertRequisitionLineLevelDeliverToLocToTableType(objRequisition.RequisitionItems)
        //            };

        //            objSqlCommand.Parameters.Add(objSqlParameter);
        //            objSqlCommand.Parameters.AddWithValue("@AllowshiptoFreeText", shipToLocSetting);
        //            objSqlCommand.Parameters.AddWithValue("@AllowBilltoFreeText", billToLocSetting);
        //            objSqlCommand.Parameters.AddWithValue("@AllowDelivertoFreeText", deliverToFreeText);
        //            objSqlCommand.Parameters.AddWithValue("@LobEntityDetailCode", LobentitydetailCode);
        //            objSqlCommand.Parameters.AddWithValue("@LobEntityCode", objRequisition.DocumentLOBDetails != null ? objRequisition.DocumentLOBDetails[0].EntityCode : "");
        //            objSqlCommand.Parameters.AddWithValue("@EntityCode", entitycode);
        //            objSqlCommand.Parameters.AddWithValue("@DocumentSource", "Requisition");
        //            objSqlCommand.Parameters.AddWithValue("@DefaultBillToLocation", IsDefaultBillToLocation);
        //            objSqlCommand.Parameters.AddWithValue("@EntityDetailCodeMappedToBillToLocation", entityDetailCode);

        //            UpdatedResult = sqlHelper.ExecuteDataSet(objSqlCommand);
        //            UpdatedResult.Tables[0].TableName = "HeaderShiptoDetails";
        //            UpdatedResult.Tables[1].TableName = "LineitemShiptoDetails";
        //            UpdatedResult.Tables[2].TableName = "HeaderBilltoDetails";
        //            UpdatedResult.Tables[3].TableName = "HeaderDelivertoDetails";
        //            UpdatedResult.Tables[4].TableName = "LineLevelDelivertoDetails";
        //        }


        //    }
        //    catch
        //    {

        //        throw;
        //    }
        //    finally
        //    {
        //        LogHelper.LogInfo(Log, "Requisition ValidateShipToBillToFromInterface Method Ended for orderId=" + objRequisition.DocumentNumber);

        //        if (objSqlCon != null && objSqlCon.State == ConnectionState.Open)
        //            objSqlCon.Close();

        //    }


        //    return UpdatedResult;

        //}

        //public DataSet ValidateItemDetailsToBeDerivedFromInterface(string itemNumber, string partnerSourceSystemValue, string uom)
        //{
        //    DataSet ds = new DataSet();
        //    SqlConnection objSqlCon = null;

        //    try
        //    {
        //        if (Log.IsDebugEnabled)
        //            Log.Debug(string.Concat("In ValidateItemDetailsToBeDerivedFromInterface Method.",
        //                                    "SP: ValidateItemDetailsToBeDerivedFromInterface"));
        //        var sqlHelper = ContextSqlConn;
        //        objSqlCon = (SqlConnection)sqlHelper.CreateConnection();
        //        objSqlCon.Open();

        //        using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_VALIDATEITEMDETAILSTOBEDERIVEDFROMINTERFACE))
        //        {
        //            objSqlCommand.CommandType = CommandType.StoredProcedure;

        //            objSqlCommand.Parameters.AddWithValue("@buyerItemNumber", itemNumber);
        //            objSqlCommand.Parameters.AddWithValue("@partnerSourceSystemValue", partnerSourceSystemValue);
        //            objSqlCommand.Parameters.AddWithValue("@UOM", uom);
        //            ds = sqlHelper.ExecuteDataSet(objSqlCommand);

        //            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
        //                ds.Tables[0].TableName = "ErrorDetail";
        //        }
        //    }
        //    catch
        //    {

        //        throw;
        //    }
        //    finally
        //    {
        //        LogHelper.LogInfo(Log, "Requisition ValidateItemDetailsToBeDerivedFromInterface Method Ended");

        //        if (objSqlCon != null && objSqlCon.State == ConnectionState.Open)
        //            objSqlCon.Close();

        //    }

        //    return ds;
        //}

        //private DataTable ConvertRequisitionBillToLocToTableType(BilltoLocation Billtolocation)
        //{
        //    DataTable dtRequisitionBillTo = new DataTable() { Locale = CultureInfo.InvariantCulture, TableName = ReqSqlConstants.TVP_P2P_HEADERBILLTO };
        //    dtRequisitionBillTo.Columns.Add("BilltoLocationNumber", typeof(string));
        //    dtRequisitionBillTo.Columns.Add("AddressLine1", typeof(string));
        //    dtRequisitionBillTo.Columns.Add("states", typeof(string));
        //    dtRequisitionBillTo.Columns.Add("CountryCode", typeof(string));
        //    dtRequisitionBillTo.Columns.Add("BilltoLocationName", typeof(string));
        //    dtRequisitionBillTo.Columns.Add("Zip", typeof(string));
        //    dtRequisitionBillTo.Columns.Add("City", typeof(string));
        //    dtRequisitionBillTo.Columns.Add("Billtolocid", typeof(int));

        //    if (Billtolocation != null)
        //    {

        //        DataRow dr = dtRequisitionBillTo.NewRow();
        //        dr["BilltoLocationNumber"] = Billtolocation.BilltoLocationNumber == null ? "" : Billtolocation.BilltoLocationNumber;
        //        dr["AddressLine1"] = Billtolocation.Address == null ? "" : Billtolocation.Address.AddressLine1 == null ? "" : Billtolocation.Address.AddressLine1;
        //        dr["states"] = Billtolocation.Address == null ? "" : Billtolocation.Address.State == null ? "" : Billtolocation.Address.State;
        //        dr["CountryCode"] = Billtolocation.Address == null ? "" : Billtolocation.Address.CountryCode == null ? "" : Billtolocation.Address.CountryCode;
        //        dr["BilltoLocationName"] = Billtolocation.Address == null ? "" : Billtolocation.BilltoLocationName == null ? "" : Billtolocation.BilltoLocationName;
        //        dr["Zip"] = Billtolocation.Address == null ? "" : Billtolocation.Address.Zip == null ? "" : Billtolocation.Address.Zip;
        //        dr["City"] = Billtolocation.Address == null ? "" : Billtolocation.Address.City == null ? "" : Billtolocation.Address.City;
        //        dr["Billtolocid"] = Billtolocation.BilltoLocationId == null ? 0 : Billtolocation.BilltoLocationId;
        //        dtRequisitionBillTo.Rows.Add(dr);

        //    }
        //    return dtRequisitionBillTo;
        //}
        //private DataTable ConvertRequisitionLineItemShipToLocToTableType(List<RequisitionItem> lstRequisitionItem)
        //{
        //    DataTable dtRequisitionLineShipTo = new DataTable() { Locale = CultureInfo.InvariantCulture, TableName = ReqSqlConstants.TVP_P2P_LINELEVELSHIPTO };
        //    dtRequisitionLineShipTo.Columns.Add("ItemLineNumber", typeof(int));
        //    dtRequisitionLineShipTo.Columns.Add("ShiptoLocationNumber", typeof(string));
        //    dtRequisitionLineShipTo.Columns.Add("AddressLine1", typeof(string));
        //    dtRequisitionLineShipTo.Columns.Add("states", typeof(string));
        //    dtRequisitionLineShipTo.Columns.Add("CountryCode", typeof(string));
        //    dtRequisitionLineShipTo.Columns.Add("ShiptoLocationName", typeof(string));
        //    dtRequisitionLineShipTo.Columns.Add("Zip", typeof(string));
        //    dtRequisitionLineShipTo.Columns.Add("City", typeof(string));
        //    dtRequisitionLineShipTo.Columns.Add("Shiptolocid", typeof(int));
        //    dtRequisitionLineShipTo.Columns.Add("UnitPrice", typeof(decimal));
        //    dtRequisitionLineShipTo.Columns.Add("Quantity", typeof(decimal));

        //    if (lstRequisitionItem != null)
        //    {
        //        foreach (var doc in lstRequisitionItem)
        //        {
        //            ShiptoLocation ShiptoLocation = doc.DocumentItemShippingDetails.Select(x => x.ShiptoLocation).FirstOrDefault();
        //            DataRow dr = dtRequisitionLineShipTo.NewRow();
        //            dr["ItemLineNumber"] = doc.ItemLineNumber;
        //            if (ShiptoLocation != null)
        //            {
        //                dr["ShiptoLocationNumber"] = ShiptoLocation.ShiptoLocationNumber == null ? "" : ShiptoLocation.ShiptoLocationNumber;
        //                dr["AddressLine1"] = ShiptoLocation.Address == null ? "" : ShiptoLocation.Address.AddressLine1 == null ? "" : ShiptoLocation.Address.AddressLine1;
        //                dr["states"] = ShiptoLocation.Address == null ? "" : ShiptoLocation.Address.State == null ? "" : ShiptoLocation.Address.State;
        //                dr["CountryCode"] = ShiptoLocation.Address == null ? "" : ShiptoLocation.Address.CountryCode == null ? "" : ShiptoLocation.Address.CountryCode;
        //                dr["ShiptoLocationName"] = ShiptoLocation.Address == null ? "" : ShiptoLocation.ShiptoLocationName == null ? "" : ShiptoLocation.ShiptoLocationName;
        //                dr["Zip"] = ShiptoLocation.Address == null ? "" : ShiptoLocation.Address.Zip == null ? "" : ShiptoLocation.Address.Zip;
        //                dr["City"] = ShiptoLocation.Address == null ? "" : ShiptoLocation.Address.City == null ? "" : ShiptoLocation.Address.City;
        //                dr["Shiptolocid"] = ShiptoLocation.ShiptoLocationId == null ? 0 : ShiptoLocation.ShiptoLocationId;
        //            }
        //            /*For Line Level ShipTo Validation to get skip for Cancelled Items*/
        //            dr["UnitPrice"] = doc.UnitPrice != null ? doc.UnitPrice : 0; ;
        //            dr["Quantity"] = doc.Quantity;

        //            dtRequisitionLineShipTo.Rows.Add(dr);
        //        }
        //    }
        //    return dtRequisitionLineShipTo;
        //}
        //private DataTable ConvertRequisitionDeliverToLocToTableType(DelivertoLocation DelivertoLocation)
        //{
        //    DataTable dtRequisitiondeliverTo = new DataTable() { Locale = CultureInfo.InvariantCulture, TableName = ReqSqlConstants.TVP_P2P_HEADERDELIVERTO };
        //    dtRequisitiondeliverTo.Columns.Add("DelivertoLocationNumber", typeof(string));
        //    dtRequisitiondeliverTo.Columns.Add("AddressLine1", typeof(string));
        //    dtRequisitiondeliverTo.Columns.Add("AddressLine2", typeof(string));
        //    dtRequisitiondeliverTo.Columns.Add("AddressLine3", typeof(string));
        //    dtRequisitiondeliverTo.Columns.Add("Country", typeof(string));
        //    dtRequisitiondeliverTo.Columns.Add("StateCode", typeof(string));
        //    dtRequisitiondeliverTo.Columns.Add("StateName", typeof(string));
        //    dtRequisitiondeliverTo.Columns.Add("FaxNo", typeof(string));
        //    dtRequisitiondeliverTo.Columns.Add("EmailAddress", typeof(string));
        //    dtRequisitiondeliverTo.Columns.Add("CountryName", typeof(string));
        //    dtRequisitiondeliverTo.Columns.Add("ISDCountryCode", typeof(string));
        //    dtRequisitiondeliverTo.Columns.Add("AreaCode", typeof(string));
        //    dtRequisitiondeliverTo.Columns.Add("states", typeof(string));
        //    dtRequisitiondeliverTo.Columns.Add("CountryCode", typeof(string));
        //    dtRequisitiondeliverTo.Columns.Add("DelivertoLocationName", typeof(string));
        //    dtRequisitiondeliverTo.Columns.Add("Zip", typeof(string));
        //    dtRequisitiondeliverTo.Columns.Add("City", typeof(string));
        //    dtRequisitiondeliverTo.Columns.Add("Delivertolocid", typeof(int));

        //    if (DelivertoLocation != null && !string.IsNullOrEmpty(DelivertoLocation.DelivertoLocationNumber))
        //    {


        //        DataRow dr = dtRequisitiondeliverTo.NewRow();
        //        dr["DelivertoLocationNumber"] = DelivertoLocation.DelivertoLocationNumber == null ? "" : DelivertoLocation.DelivertoLocationNumber;
        //        dr["AddressLine1"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.Address.AddressLine1 == null ? "" : DelivertoLocation.Address.AddressLine1;
        //        dr["AddressLine2"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.Address.AddressLine2 == null ? "" : DelivertoLocation.Address.AddressLine2;
        //        dr["AddressLine3"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.Address.AddressLine3 == null ? "" : DelivertoLocation.Address.AddressLine3;
        //        dr["Country"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.Address.Country == null ? "" : DelivertoLocation.Address.Country;
        //        dr["StateCode"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.Address.State == null ? "" : DelivertoLocation.Address.State;
        //        dr["StateName"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.Address.StateName == null ? "" : DelivertoLocation.Address.StateName;
        //        dr["FaxNo"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.Address.FaxNo == null ? "" : DelivertoLocation.Address.FaxNo;
        //        dr["EmailAddress"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.Address.EmailAddress == null ? "" : DelivertoLocation.Address.EmailAddress;
        //        dr["CountryName"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.Address.CountryName == null ? "" : DelivertoLocation.Address.CountryName;
        //        dr["ISDCountryCode"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.Address.ISDCountryCode == null ? "" : DelivertoLocation.Address.ISDCountryCode;
        //        dr["AreaCode"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.Address.AreaCode == null ? "" : DelivertoLocation.Address.AreaCode;
        //        dr["states"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.Address.State == null ? "" : DelivertoLocation.Address.State;
        //        dr["CountryCode"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.Address.CountryCode == null ? "" : DelivertoLocation.Address.CountryCode;
        //        dr["DelivertoLocationName"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.DelivertoLocationName == null ? "" : DelivertoLocation.DelivertoLocationName;
        //        dr["Zip"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.Address.Zip == null ? "" : DelivertoLocation.Address.Zip;
        //        dr["City"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.Address.City == null ? "" : DelivertoLocation.Address.City;
        //        dr["Delivertolocid"] = DelivertoLocation.DelivertoLocationId == 0 ? 0 : DelivertoLocation.DelivertoLocationId;
        //        dtRequisitiondeliverTo.Rows.Add(dr);

        //    }
        //    return dtRequisitiondeliverTo;
        //}
        //private DataTable ConvertRequisitionLineLevelDeliverToLocToTableType(List<RequisitionItem> lstRequisitionItem)
        //{
        //    DataTable dtRequisitionLineItemdeliverTo = new DataTable() { Locale = CultureInfo.InvariantCulture, TableName = ReqSqlConstants.TVP_P2P_LINELEVELDELIVERTO };
        //    dtRequisitionLineItemdeliverTo.Columns.Add("ItemLineNumber", typeof(int));
        //    dtRequisitionLineItemdeliverTo.Columns.Add("DelivertoLocationNumber", typeof(string));
        //    dtRequisitionLineItemdeliverTo.Columns.Add("AddressLine1", typeof(string));
        //    dtRequisitionLineItemdeliverTo.Columns.Add("AddressLine2", typeof(string));
        //    dtRequisitionLineItemdeliverTo.Columns.Add("AddressLine3", typeof(string));
        //    dtRequisitionLineItemdeliverTo.Columns.Add("Country", typeof(string));
        //    dtRequisitionLineItemdeliverTo.Columns.Add("StateCode", typeof(string));
        //    dtRequisitionLineItemdeliverTo.Columns.Add("StateName", typeof(string));
        //    dtRequisitionLineItemdeliverTo.Columns.Add("FaxNo", typeof(string));
        //    dtRequisitionLineItemdeliverTo.Columns.Add("EmailAddress", typeof(string));
        //    dtRequisitionLineItemdeliverTo.Columns.Add("CountryName", typeof(string));
        //    dtRequisitionLineItemdeliverTo.Columns.Add("ISDCountryCode", typeof(string));
        //    dtRequisitionLineItemdeliverTo.Columns.Add("AreaCode", typeof(string));
        //    dtRequisitionLineItemdeliverTo.Columns.Add("states", typeof(string));
        //    dtRequisitionLineItemdeliverTo.Columns.Add("CountryCode", typeof(string));
        //    dtRequisitionLineItemdeliverTo.Columns.Add("DelivertoLocationName", typeof(string));
        //    dtRequisitionLineItemdeliverTo.Columns.Add("Zip", typeof(string));
        //    dtRequisitionLineItemdeliverTo.Columns.Add("City", typeof(string));
        //    dtRequisitionLineItemdeliverTo.Columns.Add("Delivertolocid", typeof(int));


        //    if (lstRequisitionItem != null)
        //    {
        //        foreach (var doc in lstRequisitionItem)
        //        {
        //            DelivertoLocation DelivertoLocation = doc.DocumentItemShippingDetails.Select(x => x.DelivertoLocation).FirstOrDefault();
        //            DataRow dr = dtRequisitionLineItemdeliverTo.NewRow();
        //            dr["ItemLineNumber"] = doc.ItemLineNumber;
        //            if (DelivertoLocation != null && !string.IsNullOrEmpty(DelivertoLocation.DelivertoLocationNumber))
        //            {

        //                dr["DelivertoLocationNumber"] = DelivertoLocation.DelivertoLocationNumber == null ? "" : DelivertoLocation.DelivertoLocationNumber;
        //                dr["AddressLine1"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.Address.AddressLine1 == null ? "" : DelivertoLocation.Address.AddressLine1;
        //                dr["AddressLine2"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.Address.AddressLine2 == null ? "" : DelivertoLocation.Address.AddressLine2;
        //                dr["AddressLine3"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.Address.AddressLine3 == null ? "" : DelivertoLocation.Address.AddressLine3;
        //                dr["Country"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.Address.Country == null ? "" : DelivertoLocation.Address.Country;
        //                dr["StateCode"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.Address.State == null ? "" : DelivertoLocation.Address.State;
        //                dr["StateName"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.Address.StateName == null ? "" : DelivertoLocation.Address.StateName;
        //                dr["FaxNo"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.Address.FaxNo == null ? "" : DelivertoLocation.Address.FaxNo;
        //                dr["EmailAddress"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.Address.EmailAddress == null ? "" : DelivertoLocation.Address.EmailAddress;
        //                dr["CountryName"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.Address.CountryName == null ? "" : DelivertoLocation.Address.CountryName;
        //                dr["ISDCountryCode"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.Address.ISDCountryCode == null ? "" : DelivertoLocation.Address.ISDCountryCode;
        //                dr["AreaCode"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.Address.AreaCode == null ? "" : DelivertoLocation.Address.AreaCode;
        //                dr["states"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.Address.State == null ? "" : DelivertoLocation.Address.State;
        //                dr["CountryCode"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.Address.CountryCode == null ? "" : DelivertoLocation.Address.CountryCode;
        //                dr["DelivertoLocationName"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.DelivertoLocationName == null ? "" : DelivertoLocation.DelivertoLocationName;
        //                dr["Zip"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.Address.Zip == null ? "" : DelivertoLocation.Address.Zip;
        //                dr["City"] = DelivertoLocation.Address == null ? "" : DelivertoLocation.Address.City == null ? "" : DelivertoLocation.Address.City;
        //                dr["Delivertolocid"] = DelivertoLocation.DelivertoLocationId == 0 ? 0 : DelivertoLocation.DelivertoLocationId;

        //            }
        //            dtRequisitionLineItemdeliverTo.Rows.Add(dr);
        //        }
        //    }
        //    return dtRequisitionLineItemdeliverTo;
        //}


        //public DataTable ConvertToCustomAttributesDataTable(Requisition objRequisition)
        //{
        //    DataTable dtCustomAttributes = new DataTable();
        //    try
        //    {
        //        dtCustomAttributes = P2P.DataAccessObjects.SQLServer.SQLCommonDAO.GetCustomAttributesDataTable();

        //        P2P.DataAccessObjects.SQLServer.SQLCommonDAO sqlCommonDAO = new P2P.DataAccessObjects.SQLServer.SQLCommonDAO();

        //        sqlCommonDAO.FillCustomAttributeDataTable(dtCustomAttributes, objRequisition.CustomAttributes, Level.Header);

        //        if (objRequisition.RequisitionItems != null && objRequisition.RequisitionItems.Count > 0)
        //        {
        //            foreach (RequisitionItem reqItem in objRequisition.RequisitionItems)
        //            {
        //                sqlCommonDAO.FillCustomAttributeDataTable(dtCustomAttributes, reqItem.CustomAttributes, Level.Item, reqItem.ItemLineNumber);

        //                if (reqItem.ItemSplitsDetail != null && reqItem.ItemSplitsDetail.Count > 0)
        //                {
        //                    int splitNum = 0;
        //                    foreach (RequisitionSplitItems reqSplit in reqItem.ItemSplitsDetail)
        //                        sqlCommonDAO.FillCustomAttributeDataTable(dtCustomAttributes, reqSplit.CustomAttributes, Level.Distribution, reqItem.ItemLineNumber, ++splitNum);
        //                }
        //            }
        //        }
        //    }
        //    catch { }

        //    return dtCustomAttributes;
        //}

        public void CalculateAndUpdateSplitDetails(long RequisitionId)
        {
            SqlConnection _sqlCon = null;
            try
            {
                LogHelper.LogInfo(Log, "Requisition CalculateAndUpdateSplitDetails Method Started for RequisitionId=" + RequisitionId);
                _sqlCon = (SqlConnection)ContextSqlConn.CreateConnection();
                _sqlCon.Open();
                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("In CalculateAndUpdateSplitDetails Method",
                                             "SP: usp_P2P_REQ_CalculateInterfaceItemSplitDetails with parameter: RequisitionId=" + RequisitionId, " was called."));
                ContextSqlConn.ExecuteNonQuery(ReqSqlConstants.USP_P2P_REQ_CALCULATEINTERFACEITEMSPLITDETAILS, RequisitionId);
            }
            catch (Exception sqlEx)
            {
                LogHelper.LogError(Log, "Error occured in Requisition CalculateAndUpdateSplitDetails Method for RequisitionId=" + RequisitionId, sqlEx);
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
                LogHelper.LogInfo(Log, "Requisition CalculateAndUpdateSplitDetails Method Ended for RequisitionId=" + RequisitionId);
            }
        }

        public bool InsertUpdateLineitemTaxes(Requisition objRequisition)
        {
            LogHelper.LogInfo(Log, "Requisition Insert Update Line item Taxes Method Started");
            SqlConnection objSqlCon = null;

            var lstErrors = new List<string>();
            try
            {
                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("In InsertUpdateLineitemTaxes Method.",
                                            "SP: usp_P2P_REQ_InsertUpdateLineItemTaxes, with parameters: orderId = " + objRequisition.DocumentCode));

                var sqlHelper = ContextSqlConn;
                objSqlCon = (SqlConnection)sqlHelper.CreateConnection();
                objSqlCon.Open();

                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_INSERTUPDATELINEITEMTAXES))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    SqlParameter objSqlParameter = new SqlParameter("@tvp_P2P_RequisitionItemTaxes", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_P2P_REQUISITIONITEMTAXES,
                        Value = FillLineItemTaxes(objRequisition, false)
                    };

                    objSqlCommand.Parameters.Add(objSqlParameter);

                    sqlHelper.ExecuteNonQuery(objSqlCommand);
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogInfo(Log, "Error in SQLRequisitionDAO.InsertUpdateLineitemTaxes \n  " + ex.Message);
                return false;
            }
            finally
            {
                if (!ReferenceEquals(objSqlCon, null) && objSqlCon.State != ConnectionState.Closed)
                {
                    objSqlCon.Close();
                    objSqlCon.Dispose();
                }
                LogHelper.LogInfo(Log, "Requisition InsertUpdateLineitemTaxes Method Ended");
            }
        }
        private DataTable FillLineItemTaxes(Requisition objRequisition, bool isValidate = true)
        {
            DataTable dtLineItemTaxes = new DataTable();
            try
            {
                dtLineItemTaxes = new DataTable() { TableName = "tvp_P2P_RequisitionItemTaxes" };
                dtLineItemTaxes.Columns.Add("RequisitionId", typeof(long));
                dtLineItemTaxes.Columns.Add("RequisitionItemId", typeof(long));
                dtLineItemTaxes.Columns.Add("LineNumber", typeof(int));
                dtLineItemTaxes.Columns.Add("TaxId", typeof(int));
                dtLineItemTaxes.Columns.Add("TaxType", typeof(short));
                dtLineItemTaxes.Columns.Add("TaxMode", typeof(short));
                dtLineItemTaxes.Columns.Add("TaxDescription", typeof(string));
                dtLineItemTaxes.Columns.Add("TaxCode", typeof(string));
                dtLineItemTaxes.Columns.Add("TaxValue", typeof(decimal));


                foreach (RequisitionItem objItem in objRequisition.RequisitionItems)
                {
                    if (objItem.Taxes != null && objItem.Taxes.Any())
                    {
                        foreach (var taxes in objItem.Taxes)
                        {
                            var dr = dtLineItemTaxes.NewRow();
                            dr["RequisitionId"] = objRequisition.DocumentCode;
                            dr["RequisitionItemId"] = objItem.DocumentItemId;
                            dr["LineNumber"] = objItem.ItemLineNumber;
                            dr["TaxId"] = 0;
                            dr["TaxType"] = (Int16)taxes.TaxType;
                            dr["TaxMode"] = (Int16)taxes.TaxMode;
                            dr["TaxDescription"] = taxes.TaxDescription;
                            dr["TaxCode"] = taxes.TaxCode;
                            dr["TaxValue"] = taxes.TaxValue;
                            dtLineItemTaxes.Rows.Add(dr);
                        }
                    }
                    else if (!isValidate)
                    {
                        var dr = dtLineItemTaxes.NewRow();
                        dr["RequisitionId"] = objRequisition.DocumentCode;
                        dr["RequisitionItemId"] = objItem.DocumentItemId;
                        dr["LineNumber"] = objItem.ItemLineNumber;
                        dr["TaxId"] = 0;
                        dr["TaxType"] = 0;
                        dr["TaxMode"] = 0;
                        dr["TaxDescription"] = "";
                        dr["TaxCode"] = "";
                        dr["TaxValue"] = 0;
                        dtLineItemTaxes.Rows.Add(dr);

                    }
                }
            }
            catch { }

            return dtLineItemTaxes;
        }
        public DataTable CheckCatalogItemsAccessForContactCode(long requesterId, string catalogItems)
        {
            SqlConnection objSqlCon = null;
            DataTable dtCatalogItemIDs = new DataTable();
            try
            {
                objSqlCon = (SqlConnection)ContextSqlConn.CreateConnection();
                objSqlCon.Open();
                using (var objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_GetListOfCatalogItemIdsNotAllowedAccessLatestNow))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.Add(new SqlParameter("@requesterId", requesterId));
                    objSqlCommand.Parameters.Add(new SqlParameter("@catalogItems", catalogItems));
                    dtCatalogItemIDs = ContextSqlConn.ExecuteDataSet(objSqlCommand).Tables[0];
                }
                return dtCatalogItemIDs;
            }
            finally
            {
                if (!ReferenceEquals(objSqlCon, null) && objSqlCon.State != ConnectionState.Closed)
                {
                    objSqlCon.Close();
                    objSqlCon.Dispose();
                }
                dtCatalogItemIDs.Dispose();
            }
        }
        //public bool DeleteSplitsByItemId(long RequisitionItemId, long documentId)
        //{
        //    SqlConnection _sqlCon = null;
        //    try
        //    {
        //        LogHelper.LogInfo(Log, "Requisition DeleteSplitsByItemId Method Started for orderItemId=" + RequisitionItemId);
        //        _sqlCon = (SqlConnection)ContextSqlConn.CreateConnection();
        //        _sqlCon.Open();
        //        if (Log.IsDebugEnabled)
        //            Log.Debug(string.Concat("In DeleteSplitsByItemId Method",
        //                                     "SP: usp_P2P_REQ_DeleteSplitsByItemId with parameter: orderItemId=" + RequisitionItemId, " was called."));
        //        var boolResult = Convert.ToBoolean(ContextSqlConn.ExecuteScalar(ReqSqlConstants.USP_P2P_REQ_DELETESPLITSBYITEMID,
        //                                                                RequisitionItemId
        //                                                              ), NumberFormatInfo.InvariantInfo);
        //        return boolResult;
        //    }
        //    catch (Exception sqlEx)
        //    {
        //        LogHelper.LogError(Log, "Error occured in Requisition DeleteSplitsByItemId Method for RequisitionItemId=" + RequisitionItemId, sqlEx);
        //        throw;
        //    }
        //    finally
        //    {
        //        if (!ReferenceEquals(_sqlCon, null) && _sqlCon.State != ConnectionState.Closed)
        //        {
        //            _sqlCon.Close();
        //            _sqlCon.Dispose();
        //        }
        //        LogHelper.LogInfo(Log, "Requisition DeleteSplitsByItemId Method Ended for RequisitionItemId=" + RequisitionItemId);
        //    }
        //}
        public bool UpdateRequisitionLineStatusonRFXCreateorUpdate(long documentCode, List<long> p2pLineItemId, DocumentType docType, bool IsDocumentDeleted = false)
        {
            SqlConnection sqlCon = null;
            SqlTransaction sqlTrans = null;
            bool result = false;
            long contactCode = 0;
            List<long> docCodes = new List<long>();

            try
            {
                LogHelper.LogInfo(Log, "In Requisition UpdateRequisitionLineStatusonRFXCreateorUpdate Method Started for documentCode=" + documentCode);
                contactCode = UserContext.ContactCode;
                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("Requisition UpdateRequisitionLineStatusonRFXCreateorUpdate sp usp_P2P_REQ_UpdateReqnLineStatusonRFXCreateorUpdate with parameter: documentCode=" + documentCode + " and P2PLineItemIds list was called."));
                var sqlHelper = ContextSqlConn;
                sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                sqlCon.Open();
                sqlTrans = sqlCon.BeginTransaction();
                using (var objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_UPDATEREQLINESTATUSONRFXCREATEORUPDATE))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.Add(new SqlParameter("@documentCode ", documentCode));
                    objSqlCommand.Parameters.Add(new SqlParameter("@tvp_P2P_Ids", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_P2P_IDS,
                        Value = ConvertIdsToTable(p2pLineItemId)
                    });
                    objSqlCommand.Parameters.Add(new SqlParameter("@requestedDocumentType ", docType));
                    objSqlCommand.Parameters.Add(new SqlParameter("@IsDocumentDeleted ", IsDocumentDeleted));
                    //result = Convert.ToBoolean(sqlHelper.ExecuteScalar(objSqlCommand, sqlTrans), NumberFormatInfo.InvariantInfo);
                    var dsReqIds = sqlHelper.ExecuteDataSet(objSqlCommand, sqlTrans);

                    if (dsReqIds != null && dsReqIds.Tables.Count > 0)
                    {
                        foreach (DataRow item in dsReqIds.Tables[0].Rows)
                        {
                            string data = item[0].ToString();
                            long value = 0;
                            if (long.TryParse(data, out value))
                            {
                                if (value > 0)
                                {
                                    docCodes.Add(value);
                                }
                            }
                            else
                            {
                                result = Convert.ToBoolean(data);
                            }
                        }
                    }
                    if (dsReqIds != null)
                        result = true;
                }
                if (!ReferenceEquals(sqlTrans, null) && !ReferenceEquals(sqlCon, null) && sqlCon.State == ConnectionState.Open)
                    sqlTrans.Commit();

                if (docCodes.Count > 0)
                {
                    // Updated for multiple documents
                    AddIntoSearchIndexerQueueing(docCodes, (int)DocumentType.Requisition, UserContext, GepConfiguration);
                }
                else
                {
                    // Old way
                    AddIntoSearchIndexerQueueing(documentCode, (int)DocumentType.Requisition, UserContext, GepConfiguration);
                }

                return result;
            }
            catch
            {
                if (!ReferenceEquals(sqlTrans, null) && !ReferenceEquals(sqlCon, null) && sqlCon.State == ConnectionState.Open)
                {
                    try
                    {
                        sqlTrans.Rollback();
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
                if (!ReferenceEquals(sqlCon, null) && sqlCon.State != ConnectionState.Closed)
                {
                    sqlCon.Close();
                    sqlCon.Dispose();
                }
                LogHelper.LogInfo(Log, "Requisition UpdateRequisitionLineStatusonRFXCreateorUpdate Method Ended for DocumentCode = " + documentCode);
            }
        }

        private static DataTable ConvertIdsToTable(List<long> Ids)
        {
            DataTable dtIds = new DataTable();
            dtIds.Columns.Add("Id", typeof(long));
            foreach (var id in Ids)
            {
                DataRow dr = dtIds.NewRow();
                dr["Id"] = id;
                dtIds.Rows.Add(dr);
            }
            return dtIds;
        }
        public bool SaveChargeDefaultAccountingDetails(long requisitionId, List<DocumentSplitItemEntity> requisitionSplitItemEntities, int codeCombinationFieldId, string codeCombinationFieldValue, bool saveDefaultGL = false)
        {
            bool flag = false;
            SqlConnection _sqlCon = null;
            //SqlTransaction _sqlTrans = null;
            try
            {
                LogHelper.LogInfo(Log, "requisition SaveChargeDefaultAccountingDetails Method Started for requisitionId = " + requisitionId);

                var sqlHelper = ContextSqlConn;
                _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                _sqlCon.Open();
                //_sqlTrans = _sqlCon.BeginTransaction();

                DataTable dtOrdItemEntities = null;
                dtOrdItemEntities = P2P.DataAccessObjects.DAOHelper.ConvertToDataTable(requisitionSplitItemEntities,
                                                       GetRequisitionSplitItemEntitiesTable);
                using (System.Transactions.TransactionScope obj = new System.Transactions.TransactionScope())
                {
                    using (var objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_SAVEREQUISITIONCHARGEDEFAULTACCOUNTING, _sqlCon))
                    {
                        objSqlCommand.CommandType = CommandType.StoredProcedure;
                        objSqlCommand.CommandTimeout = 150;
                        objSqlCommand.Parameters.AddWithValue("@RequisitionId", requisitionId);
                        objSqlCommand.Parameters.Add(new SqlParameter("@tvp_P2P_SplitItemsEntities", SqlDbType.Structured)
                        {
                            TypeName = ReqSqlConstants.TVP_P2P_SPLITITEMSENTITIES,
                            Value = dtOrdItemEntities
                        });
                        objSqlCommand.Parameters.AddWithValue("@UserId", UserContext.ContactCode);
                        objSqlCommand.Parameters.AddWithValue("@SaveDefaultGL", saveDefaultGL);
                        flag = Convert.ToBoolean(ContextSqlConn.ExecuteScalar(objSqlCommand), CultureInfo.InvariantCulture);
                    }
                    obj.Complete();
                }

            }
            catch
            {

            }
            finally
            {
                _sqlCon.Close();
                _sqlCon.Dispose();
                LogHelper.LogInfo(Log, "requisition SaveChargeDefaultAccountingDetails Method Ended for requisitionId = " + requisitionId);
            }

            return flag;
        }

        public ICollection<P2PItem> GetRequisitionItemsDispatchMode(long documentCode)
        {
            RefCountingDataReader objRefCountingDataReader = null;
            var requisitionItems = new List<P2PItem>();
            try
            {
                LogHelper.LogInfo(Log, "Requisition GetRequisitionItemsDispatchMode Method Started for documentCode=" + documentCode);

                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("Requisition GetRequisitionItemsDispatchMode sp usp_P2P_REQ_GetAllLineItemsById with parameter: ", " documentCode=" + documentCode));

                objRefCountingDataReader =
                 (RefCountingDataReader)
                 ContextSqlConn.ExecuteReader(ReqSqlConstants.USP_P2P_REQ_GETITEMDISPATCHMODE, new object[] { documentCode });
                if (objRefCountingDataReader != null)
                {
                    var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;
                    while (sqlDr.Read())
                    {
                        var objRequisitionItem = new RequisitionItem
                        {
                            PartnerCode = GetDecimalValue(sqlDr, ReqSqlConstants.COL_PARTNER_CODE),
                            OrderLocationId = GetLongValue(sqlDr, ReqSqlConstants.COL_ORDERLOCATIONID),
                            TransmissionValue = GetIntValue(sqlDr, ReqSqlConstants.COL_TRANSMISSIONVALUE).ToString(),
                            TrasmissionMode = GetIntValue(sqlDr, ReqSqlConstants.COL_TRASMISSIONMODE),
                            OrderLocationName = GetStringValue(sqlDr, ReqSqlConstants.COL_ORDERLOCATIONNAME),
                            PartnerName = GetStringValue(sqlDr, ReqSqlConstants.COL_PARTNER_NAME)
                        };
                        requisitionItems.Add(objRequisitionItem);
                    }
                }
            }
            finally
            {
                if (!ReferenceEquals(objRefCountingDataReader, null) && !objRefCountingDataReader.IsClosed)
                {
                    objRefCountingDataReader.Close();
                    objRefCountingDataReader.Dispose();
                }
                LogHelper.LogInfo(Log, "Requisition GetRequisitionItemsDispatchMode Method Ended for documentCode=" + documentCode);
            }
            return requisitionItems;
        }


        public List<Taxes> GetRequisitioneHeaderTaxes(long requisitionId, int pageIndex, int pageSize)
        {
            RefCountingDataReader objRefCountingDataReader = null;
            List<Taxes> listTaxes = new List<Taxes>();
            try
            {
                LogHelper.LogInfo(Log, "Requisition GetRequisitioneHeaderTaxes Method Started for reqisitionId=" + requisitionId);

                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("In Requisition GetRequisitioneHeaderTaxes Method",
                                             "SP: USP_P2P_INV_GETREQUISITIONHEADERTAXES with parameter: reqisitionId=" + requisitionId));
                var sqlHelper = ContextSqlConn;
                objRefCountingDataReader = (RefCountingDataReader)sqlHelper.ExecuteReader(ReqSqlConstants.USP_P2P_INV_GETREQUISITIONHEADERTAXES, new object[] { requisitionId, pageIndex, pageSize });
                if (objRefCountingDataReader != null)
                {
                    var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;
                    while (sqlDr.Read())
                    {
                        var objTaxesAndCharge = new Taxes
                        {
                            TaxId = GetIntValue(sqlDr, ReqSqlConstants.COL_TAXID),
                            TaxDescription = GetStringValue(sqlDr, ReqSqlConstants.COL_TAX_DESC),
                            TaxType = (TaxType)GetTinyIntValue(sqlDr, ReqSqlConstants.COL_TAX_TYPE),
                            TaxMode = (SplitType)GetTinyIntValue(sqlDr, ReqSqlConstants.COL_TAX_MODE),
                            TaxValue = GetDecimalValue(sqlDr, ReqSqlConstants.COL_TAX_VALUE),
                            TaxCode = GetStringValue(sqlDr, ReqSqlConstants.COL_TAX_CODE),
                            IsAccrueTax = GetBoolValue(sqlDr, ReqSqlConstants.COL_TAX_ISACCRUETAX),
                            //IsInterfaceTax = GetBoolValue(sqlDr, ReqSqlConstants.COL_TAX_ISINTERFACETAX),
                            TaxPercentage = GetDecimalValue(sqlDr, ReqSqlConstants.COL_TAX_PERCENTAGE),
                            //IsFLippedFromOrder = GetBoolValue(sqlDr, ReqSqlConstants.COL_TAX_ISFLIPPEDFROMORDER)
                        };
                        listTaxes.Add(objTaxesAndCharge);
                    }
                }
            }
            catch (Exception error)
            {
                if (Log.IsInfoEnabled) Log.Info(error.Message);
                throw error;
            }
            finally
            {
                if (!ReferenceEquals(objRefCountingDataReader, null) && !objRefCountingDataReader.IsClosed)
                {
                    objRefCountingDataReader.Close();
                    objRefCountingDataReader.Dispose();
                }
                LogHelper.LogInfo(Log, "Invoice GetInvoiceHeaderTaxes Method Ended for invoiceId=" + requisitionId);
            }
            return listTaxes;
        }

        public bool UpdateRequisitionHeaderTaxes(ICollection<Taxes> taxes, long requisitionId, int precessionValue, int precessionValueForTotal, int precessionValueForTaxesAndCharges, bool updateLineTax = false, int accuredTax = 1)
        {
            bool result = false;
            long documentItemId = 0;
            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;
            try
            {
                LogHelper.LogInfo(Log, "Requisition UpdateRequisitionHeaderTaxes Method Started for requisitionId = " + requisitionId);

                var sqlHelper = ContextSqlConn;
                _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();
                DataTable dtRequisitioneHeaderTaxes;
                foreach (Taxes tax in taxes)
                {
                    if (tax.IsInterfaceTax != true)
                    {
                        tax.IsInterfaceTax = false;
                    }
                }
                dtRequisitioneHeaderTaxes = P2P.DataAccessObjects.DAOHelper.ConvertToDataTable(taxes, GetrequisitionItemTaxTable);

                using (var objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_INV_UPDATEREQUISITIONHEADERTAXES, _sqlCon))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.AddWithValue("@RequisitionId", requisitionId);
                    objSqlCommand.Parameters.Add(new SqlParameter("@tvp_P2P_RequisitionTaxes", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_P2P_REQUISITIONTAXES,
                        Value = dtRequisitioneHeaderTaxes
                    });
                    objSqlCommand.Parameters.AddWithValue("@userid", UserContext.ContactCode);
                    objSqlCommand.Parameters.AddWithValue("@updateLineTax", updateLineTax);
                    objSqlCommand.Parameters.AddWithValue("@accuredTax", accuredTax);

                    objSqlCommand.Parameters.AddWithValue("@MaxPrecessionValue", precessionValue);
                    objSqlCommand.Parameters.AddWithValue("@MaxPrecessionValueTotal", precessionValueForTotal);
                    objSqlCommand.Parameters.AddWithValue("@MaxPrecessionValueForTaxAndCharges", precessionValueForTaxesAndCharges);

                    documentItemId = Convert.ToInt64(ContextSqlConn.ExecuteScalar(objSqlCommand), CultureInfo.InvariantCulture);
                    //var sqlDr = (SqlDataReader)sqlHelper.ExecuteReader(objSqlCommand, _sqlTrans);                    
                }
                //long invoiceId = 0;
                //if (updateLineTax)
                //    invoiceId = Convert.ToInt64(ContextSqlConn.ExecuteScalar(_sqlTrans, ReqSqlConstants.USP_P2P_INV_CALCULATE_AND_UPDATELINEITEMTAX, invoiceItemId, precessionValue), CultureInfo.InvariantCulture);
                if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
                    _sqlTrans.Commit();
                if (requisitionId > 0)
                    AddIntoSearchIndexerQueueing(requisitionId, (int)DocumentType.Invoice, UserContext, GepConfiguration);

                result = true;
            }
            catch
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
                throw;
            }
            finally
            {
                if (!ReferenceEquals(_sqlCon, null) && _sqlCon.State != ConnectionState.Closed)
                {
                    _sqlCon.Close();
                    _sqlCon.Dispose();
                }
                LogHelper.LogInfo(Log, "Requisition UpdateRequisitionHeaderTaxes Method Ended for requisitionId = " + requisitionId);
            }
            return result;
        }

        private List<KeyValuePair<Type, string>> GetrequisitionItemTaxTable()
        {
            var lstMatchItemProperties = new List<KeyValuePair<Type, string>>();
            lstMatchItemProperties.Add(new KeyValuePair<Type, string>(typeof(int), "TaxId"));
            lstMatchItemProperties.Add(new KeyValuePair<Type, string>(typeof(decimal), "TaxValue"));
            lstMatchItemProperties.Add(new KeyValuePair<Type, string>(typeof(int), "IsAccrueTax"));
            lstMatchItemProperties.Add(new KeyValuePair<Type, string>(typeof(int), "IsDeleted"));
            lstMatchItemProperties.Add(new KeyValuePair<Type, string>(typeof(int), "DocumentItemId"));
            lstMatchItemProperties.Add(new KeyValuePair<Type, string>(typeof(string), "TaxDescription"));
            lstMatchItemProperties.Add(new KeyValuePair<Type, string>(typeof(string), "TaxCode"));
            lstMatchItemProperties.Add(new KeyValuePair<Type, string>(typeof(int), "TaxMode"));
            //lstMatchItemProperties.Add(new KeyValuePair<Type, string>(typeof(int), "IsInterfaceTax"));
            //lstMatchItemProperties.Add(new KeyValuePair<Type, string>(typeof(bool), "IsFLippedFromOrder"));
            return lstMatchItemProperties;
        }



        public int CopyRequisition(long SourceRequisitionId, long DestinationRequisitionId, long ContactCode, int precessionValue, int precissionTotal, int precessionValueForTaxAndCharges,out int eventPerformed, bool isAccountChecked = false, bool isCommentsChecked = false, bool isNotesAndAttachmentChecked = false, bool isAddNonCatalogItems = true, bool isCheckReqUpdate = false, bool IsCopyEntireReq = false, bool isNewNotesAndAttachmentChecked = false,string contractDocumentStatuses="",bool showTaxJurisdictionForShipTo=false, List<KeyValuePair<long, decimal>> catlogItems=null, List<KeyValuePair<long, decimal>> itemMasteritems = null,List<CurrencyExchageRate> lstCurrencyExchageRates=null)
        {
            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;
            try
            {
                List<string> events = new List<string>();
                int documentId = 0;
                LogHelper.LogInfo(Log, "SQLRequisitionDAO CopyRequisition Method Started for Old RequisitionID = " + SourceRequisitionId.ToString() + " New CopyRequisitionID = " + DestinationRequisitionId.ToString());

                _sqlCon = (SqlConnection)ContextSqlConn.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();

                SQLDocumentDAO sqlDocumentDAO = new SQLDocumentDAO();
                sqlDocumentDAO.SqlTransaction = _sqlTrans;
                sqlDocumentDAO.ReliableSqlDatabase = ContextSqlConn;
                sqlDocumentDAO.UserContext = UserContext;
                sqlDocumentDAO.GepConfiguration = GepConfiguration;

                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("In Requisition SQLRequisitionDAO CopyRequisition Method", "SP: USP_P2P_COPYREQUISITIONEXISTINGTONEW"));

                DataTable dtCatalogLineDetails = null;
                if (itemMasteritems != null)
                    dtCatalogLineDetails = ConvertCatalogKeyValueToDataTable(catlogItems);

                DataTable dtItemMasterLineDetails = null;
                if (itemMasteritems != null)
                    dtItemMasterLineDetails = ConvertCatalogKeyValueToDataTable(itemMasteritems);

                DataTable dtCurrencyExchageRates = null;
                if (lstCurrencyExchageRates != null)
                    dtCurrencyExchageRates = ConvertCurrencyExchangesToDataTable(lstCurrencyExchageRates);

                using (var objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_COPYREQUISITIONEXISTINGTONEW, _sqlCon))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.AddWithValue("@oldRequisitionId", SourceRequisitionId);
                    objSqlCommand.Parameters.AddWithValue("@newRequisitionId", DestinationRequisitionId);
                    objSqlCommand.Parameters.AddWithValue("@contactCode", ContactCode);
                    objSqlCommand.Parameters.AddWithValue("@precessionValue", precessionValue);
                    objSqlCommand.Parameters.AddWithValue("@precissionTotal", precissionTotal);
                    objSqlCommand.Parameters.AddWithValue("@precessionValueForTaxAndCharges", precessionValueForTaxAndCharges);
                    objSqlCommand.Parameters.AddWithValue("@isAccountChecked", isAccountChecked);
                    objSqlCommand.Parameters.AddWithValue("@isCommentsChecked", isCommentsChecked);
                    objSqlCommand.Parameters.AddWithValue("@isNotesAndAttachmentChecked", isNotesAndAttachmentChecked);
                    objSqlCommand.Parameters.AddWithValue("@isAddNonCatalogItems", isAddNonCatalogItems);
                    objSqlCommand.Parameters.AddWithValue("@isCheckReqUpdate", isCheckReqUpdate);
                    objSqlCommand.Parameters.AddWithValue("@IsCopyEntireReq", IsCopyEntireReq);
                    objSqlCommand.Parameters.AddWithValue("@isNewNotesAndAttachmentChecked", isNewNotesAndAttachmentChecked);
                    objSqlCommand.Parameters.AddWithValue("@ContractDocumentStatuses", contractDocumentStatuses);
                    objSqlCommand.Parameters.AddWithValue("@showTaxJurisdictionForShipTo", showTaxJurisdictionForShipTo);
                    objSqlCommand.Parameters.Add(new SqlParameter("@CatalogLineItems", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_CatalogLineitem,
                        Value = dtCatalogLineDetails
                    });
                    objSqlCommand.Parameters.Add(new SqlParameter("@ItemMasterLineItems", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_CatalogLineitem,
                        Value = dtItemMasterLineDetails
                    });

                    objSqlCommand.Parameters.Add(new SqlParameter("@CurrencyExchangeRates", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.tvp_CurrencyExchangeRates,
                        Value = dtCurrencyExchageRates
                    });
                    var dsReqdata = ContextSqlConn.ExecuteDataSet(objSqlCommand);
                    if (dsReqdata != null && dsReqdata.Tables.Count > 0)
                    {
                        if (dsReqdata.Tables[0].Rows.Count > 0)
                        {
                            foreach (DataRow dr in dsReqdata.Tables[0].Rows)
                            {
                                string EventName = Convert.ToString(dr["EventName"]);
                                if (EventName==("Delete") || EventName==("update"))
                                    events.Add(EventName);
                                if (EventName==("newRequisitionId"))
                                    documentId=Convert.ToInt32(dr["EventCount"].ToString());
                            }
                        }
                    }
                    //0 no error
                    //1 items deleted
                    //2 items price updated
                    //3 item deleted and price updated
                    if (events.Count > 0)
                    {
                        eventPerformed = ActionPerformed(events);
                    }
                    else { eventPerformed = 0; }

                }



                //SaveOrderAdditionalInfo(DestinationOrderId, sqlDocumentDAO);

                SaveRequisitionAdditionalDetails(DestinationRequisitionId, sqlDocumentDAO);

                if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
                    _sqlTrans.Commit();

                return documentId;
            }
            catch (Exception)
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
                throw;
            }
            finally
            {
                if (!ReferenceEquals(_sqlCon, null) && _sqlCon.State != ConnectionState.Closed)
                {
                    _sqlCon.Close();
                    _sqlCon.Dispose();
                }
                LogHelper.LogInfo(Log, "SQLRequisitionDAO CopyRequisition Method Ended");
            }
        }
        public bool CheckBiddingInProgress(long documentId)
        {
            SqlConnection _sqlCon = null;
            try
            {
                LogHelper.LogInfo(Log, "Requisition CheckBiddingInProgress Method Started for documentId=" + documentId);
                _sqlCon = (SqlConnection)ContextSqlConn.CreateConnection();
                _sqlCon.Open();
                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("In CheckBiddingInProgress Method",
                                             "SP: usp_P2P_REQ_CheckBiddingInProgress with parameter: documentId=" + documentId, " was called."));
                var boolResult = Convert.ToBoolean(ContextSqlConn.ExecuteScalar(ReqSqlConstants.USP_P2P_REQ_CheckBiddingInProgress,
                                                                        documentId
                                                                      ), NumberFormatInfo.InvariantInfo);
                return boolResult;
            }
            catch (Exception sqlEx)
            {
                LogHelper.LogError(Log, "Error occured in Requisition CheckBiddingInProgress Method for documentId=" + documentId, sqlEx);
                throw;
            }
            finally
            {
                if (!ReferenceEquals(_sqlCon, null) && _sqlCon.State != ConnectionState.Closed)
                {
                    _sqlCon.Close();
                    _sqlCon.Dispose();
                }
                LogHelper.LogInfo(Log, "Requisition CheckBiddingInProgress Method Ended for documentId=" + documentId);
            }
        }
        public long SaveChangeRequisitionRequest(int requisitionSource, long prevDocumentCode, string requisitionName, string requisitionNumber, DocumentSourceType documentSourceType = DocumentSourceType.None, string revisionNumber = "", bool isCreatedFromInterface = false, bool byPassAccesRights = false, int PrecessionValue = 0, int Precessiontotal = 0, int MaxPrecessionValueForTaxAndCharges = 0, bool isFunctionalAdmin = false, bool documentActive = false)
        {
            LogHelper.LogInfo(Log, "SaveChangeRequisitionRequest Method Started.");
            SqlConnection objSqlCon = null;
            SqlTransaction objSqlTrans = null;
            long newDocumentCode = 0;

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

                if (prevDocumentCode > 0)
                {
                    var objDocument = new Document
                    {
                        DocumentCode = prevDocumentCode,
                        DocumentTypeInfo = DocumentType.Requisition,
                        IsDocumentDetails = true,
                        IsAddtionalDetails = true,
                        IsStakeholderDetails = true,
                        CompanyName = UserContext.ClientName
                    };
                    objDocument = sqlDocumentDAO.GetDocumentDetailsById(objDocument, byPassAccesRights);

                    objDocument.DocumentSourceTypeInfo = documentSourceType;
                    objDocument.DocumentName = requisitionName;
                    objDocument.DocumentNumber = requisitionNumber;
                    objDocument.DocumentCode = 0;
                    objDocument.DocumentStatusInfo = DocumentStatus.Draft;
                    objDocument.CreatedOn = DateTime.UtcNow;

                    objDocument.IsAddtionalDetails = true;
                    objDocument.DocumentAdditionalFieldList.Add(CreateAdditionalInfo(newDocumentCode, "RevisionNumber", revisionNumber, FieldType.Numeric));

                    //Generating New Document Code
                    var returnDocumentCode = sqlDocumentDAO.SaveDocumentDetails(objDocument);
                    newDocumentCode = returnDocumentCode;
                }

                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("In SaveChangeRequisitionRequest Method of class",
                                            " SP: usp_P2P_REQ_ChangeRequisitionRequest,  with parameters: requisitionSource = ",
                                            requisitionSource + ", documentCode = " + prevDocumentCode + ", NewDocumentCode = " + newDocumentCode + ", userId=" + UserContext.ContactCode + ", revisionNumber=" + revisionNumber));
                if (newDocumentCode > 0)
                {
                    var result = Convert.ToInt64(sqlHelper.ExecuteScalar(objSqlTrans, ReqSqlConstants.USP_P2P_REQ_CHANGEREQUISITIONREQUEST, requisitionSource, prevDocumentCode,
                                                                            newDocumentCode, UserContext.ContactCode, revisionNumber, isCreatedFromInterface,
                                                                                PrecessionValue, Precessiontotal, MaxPrecessionValueForTaxAndCharges), NumberFormatInfo.InvariantInfo);
                    if (result > 0)
                    {
                        SaveRequisitionAdditionalDetails(newDocumentCode, sqlDocumentDAO);

                        if (Log.IsDebugEnabled)
                            Log.Debug(string.Concat("In SaveChangeRequisitionRequest Method SP: usp_P2P_REQ_UpdateRfxAndPoMapping with parameter: documentCode = " + newDocumentCode + ", OldDocumentCode = " + prevDocumentCode + " was called."));

                        if (!documentActive)
                            sqlHelper.ExecuteNonQuery(objSqlTrans, ReqSqlConstants.USP_P2P_REQ_UPDATERFXANDPOMAPPING, newDocumentCode, prevDocumentCode);

                        if (!ReferenceEquals(objSqlTrans, null) && !ReferenceEquals(objSqlCon, null) && objSqlCon.State == ConnectionState.Open)
                            objSqlTrans.Commit();

                        sqlDocumentDAO = new SQLDocumentDAO();
                        sqlDocumentDAO.ReliableSqlDatabase = sqlHelper;
                        sqlDocumentDAO.UserContext = UserContext;
                        sqlDocumentDAO.GepConfiguration = GepConfiguration;
                        if (requisitionSource == (Int64)RequisitionSource.ChangeRequisition)
                        {
                            //var objHideOldReqfromBuyer = new DocumentHideDetails
                            //{
                            //    DocumentCode = prevDocumentCode,
                            //    IsBuyerVisible = false,
                            //    IsSupplierVisible = true,
                            //    DocumentTypeCode = (int)DocumentType.Requisition
                            //};
                            //sqlDocumentDAO.SaveDocumentHideDetails(objHideOldReqfromBuyer);

                            var objHideNewReqfromSupplier = new DocumentHideDetails
                            {
                                DocumentCode = newDocumentCode,
                                IsBuyerVisible = !documentActive,
                                IsSupplierVisible = false,
                                DocumentTypeCode = (int)DocumentType.Requisition
                            };
                            sqlDocumentDAO.SaveDocumentHideDetails(objHideNewReqfromSupplier);
                        }
                    }
                    else
                    {
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
                        }
                    }
                }
                else
                {
                    if (Log.IsDebugEnabled)
                        Log.Debug("The method SaveChangeRequisitionRequest is rolled back");

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
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured while SaveChangeRequisitionRequestFromInterface DocumentNumber " + requisitionNumber, ex);
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
                }
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
                LogHelper.LogInfo(Log, "SaveChangeRequisitionRequest Method Ended");
            }
            return newDocumentCode;
        }
        public string GetRequisitionRevisionNumberByDocumentCode(long documentCode)
        {
            SqlConnection objSqlCon = null;
            try
            {
                LogHelper.LogInfo(Log, "Requisition GetRequisitionRevisionNumberByDocumentCode Method Started for documentCode=" + documentCode);
                var sqlHelper = ContextSqlConn;
                objSqlCon = (SqlConnection)sqlHelper.CreateConnection();
                objSqlCon.Open();

                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("In Requisition GetRequisitionRevisionNumberByDocumentCode Method SP: usp_P2P_REQ_GetRevisionNumberByDocumentCode with parameter: documentCode=" + documentCode, " was called."));
                string requisitionRevisionNumber = sqlHelper.ExecuteScalar(ReqSqlConstants.USP_P2P_REQ_GETREVISIONNUMBERBYDOCUMENTCODE, documentCode).ToString();

                return requisitionRevisionNumber;
            }
            catch(Exception ex)
            {
                LogHelper.LogError(Log, "Error occured while GetRequisitionRevisionNumberByDocumentCode documentCode " + documentCode, ex);
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
                LogHelper.LogInfo(Log, "Requisition GetRequisitionRevisionNumberByDocumentCode Method Ended for documentCode=" + documentCode);
            }
        }

        public long CancelChangeRequisition(long documentCode, long userId, int requisitionSource)
        {
            //Initializing the Logger.
            LogHelper.LogInfo(Log, "CancelChangeRequisition Method Started.");
            SqlConnection objSqlCon = null;
            SqlTransaction objSqlTrans = null;
            long result = 0;
            try
            {
                if (documentCode > 0)
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
                    if (Log.IsDebugEnabled)
                        Log.Debug(
                            string.Concat("Cancel Change Requisition with parameter: documentCode=" + documentCode + " and userId =" + userId,
                                          " was called."));

                    result = Convert.ToInt64(sqlHelper.ExecuteScalar(objSqlTrans, ReqSqlConstants.USP_P2P_REQ_REVOKECHANGEREQUISITION,
                                                               documentCode), NumberFormatInfo.InvariantInfo);

                    if (Log.IsDebugEnabled)
                        Log.Debug(
                            string.Concat("Change Requisition delete with parameter: documentCode=" + documentCode + " and userId =" + userId,
                                          " was called."));

                    bool isDisable = false;
                    bool isenable = false;
                    var objHideNewPOfromBuyer = new DocumentHideDetails
                    {
                        DocumentCode = documentCode,
                        IsBuyerVisible = false,
                        IsSupplierVisible = false,
                        DocumentTypeCode = (int)DocumentType.Requisition
                    };
                    isDisable = sqlDocumentDAO.SaveDocumentHideDetails(objHideNewPOfromBuyer);
                    var objEnableOldPOfromBuyer = new DocumentHideDetails
                    {
                        DocumentCode = result,
                        IsBuyerVisible = true,
                        IsSupplierVisible = true,
                        DocumentTypeCode = (int)DocumentType.Requisition
                    };
                    isenable = sqlDocumentDAO.SaveDocumentHideDetails(objEnableOldPOfromBuyer);

                    bool isDeleted = sqlDocumentDAO.DeleteDocumentById(documentCode);

                    SaveRequisitionAdditionalDetails(documentCode, sqlDocumentDAO);

                    SaveRequisitionAdditionalDetails(result, sqlDocumentDAO);

                    if (isDeleted && result > 0 && isenable && isDisable)
                    {
                        if (!ReferenceEquals(objSqlTrans, null) && !ReferenceEquals(objSqlCon, null) && objSqlCon.State == ConnectionState.Open)
                            objSqlTrans.Commit();
                    }
                    else
                    {
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
                        }
                    }
                }
            }
            catch
            {
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
                }

                throw;
            }
            finally
            {
                if (!ReferenceEquals(objSqlCon, null) && objSqlCon.State != ConnectionState.Closed)
                {
                    objSqlCon.Close();
                    objSqlCon.Dispose();
                }
                //Logger Ended.
                LogHelper.LogInfo(Log, "CancelChangeRequisition Method Ended");
            }
            return result;
        }
        public bool UpdateRequisitionItemAutoSourceProcessFlag(string itemIds, int status)
        {
            SqlConnection sqlCon = null;
            SqlTransaction sqlTrans = null;
            bool result = false;
            long contactCode = 0;
            try
            {
                LogHelper.LogInfo(Log, "In Requisition UpdateRequisitionItemAutoSourceProcessFlag Method Started for itemIds=" + itemIds);
                contactCode = UserContext.ContactCode;
                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("Requisition UpdateRequisitionItemAutoSourceProcessFlag sp USP_P2P_REQ_UpdateRequisitionItemAutoSourceProcessFlag with parameter: itemIds=" + itemIds + " and P2PLineItemIds list was called."));
                var sqlHelper = ContextSqlConn;
                sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                sqlCon.Open();
                sqlTrans = sqlCon.BeginTransaction();
                using (var objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_UPDATEREQUISITIONITEMAUTOSOURCEPROCESSFLAG))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.Add(new SqlParameter("@itemIds", itemIds));
                    objSqlCommand.Parameters.Add(new SqlParameter("@status", status));
                    result = Convert.ToBoolean(sqlHelper.ExecuteScalar(objSqlCommand, sqlTrans), NumberFormatInfo.InvariantInfo);
                }
                if (!ReferenceEquals(sqlTrans, null) && !ReferenceEquals(sqlCon, null) && sqlCon.State == ConnectionState.Open)
                    sqlTrans.Commit();
                return result;
            }
            catch
            {
                if (!ReferenceEquals(sqlTrans, null) && !ReferenceEquals(sqlCon, null) && sqlCon.State == ConnectionState.Open)
                {
                    try
                    {
                        sqlTrans.Rollback();
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
                if (!ReferenceEquals(sqlCon, null) && sqlCon.State != ConnectionState.Closed)
                {
                    sqlCon.Close();
                    sqlCon.Dispose();
                }
                LogHelper.LogInfo(Log, "Requisition UpdateRequisitionItemAutoSourceProcessFlag Method Ended for itemIds = " + itemIds);
            }
        }

        public bool SaveNotesAndAttachments(NotesOrAttachments notesAndAttachments)
        {
            SqlConnection sqlCon = null;
            SqlTransaction sqlTrans = null;
            bool result;
            try
            {
                LogHelper.LogInfo(Log, String.Format("Saved Notes And Attchments={0}", notesAndAttachments));

                sqlCon = (SqlConnection)ContextSqlConn.CreateConnection();
                sqlCon.Open();
                sqlTrans = sqlCon.BeginTransaction();
                result = Convert.ToBoolean(ContextSqlConn.ExecuteNonQuery(ReqSqlConstants.USP_P2P_REQ_SAVEREQUISITIONNOTESORATTACHMENTS,
                                    notesAndAttachments.NotesOrAttachmentId
                                    , notesAndAttachments.DocumentCode
                                    , notesAndAttachments.LineItemId
                                    , notesAndAttachments.FileId
                                    , notesAndAttachments.NoteOrAttachmentName
                                    , notesAndAttachments.NoteOrAttachmentDescription
                                    , (int)notesAndAttachments.NoteOrAttachmentType
                                    , (int)notesAndAttachments.AccessTypeId
                                    , (int)notesAndAttachments.SourceType
                                    , notesAndAttachments.IsEditable
                                    , notesAndAttachments.CategoryTypeId
                                    , notesAndAttachments.CreatedBy
                                    , notesAndAttachments.ModifiedBy
                                    , notesAndAttachments.CategoryTypeName), NumberFormatInfo.InvariantInfo);
                if (!ReferenceEquals(sqlTrans, null) && !ReferenceEquals(sqlCon, null) && sqlCon.State == ConnectionState.Open)
                {
                    if (result)
                    {
                        sqlTrans.Commit();
                    }
                    else
                    {
                        try
                        {
                            sqlTrans.Rollback();
                        }
                        catch (InvalidOperationException ex)
                        {
                            LogHelper.LogInfo(Log, String.Format("SaveNotesAndAttachments Method Started for SaveNotesAndAttachments", notesAndAttachments));

                            var objCustomFault = new CustomFault(ex.Message, "SaveNotesAndAttachments", "SaveNotesAndAttachments",
                                                                 "Common", ExceptionType.ApplicationException,
                                                                 notesAndAttachments.NoteOrAttachmentName, false);
                            throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                                  "Error while SaveNotesAndAttachments " + ex.Message + " Stack Trace: " + ex.StackTrace + "Inner Exception: " + ex.InnerException);
                        }
                    }
                }
            }
            catch
            {
                if (!ReferenceEquals(sqlTrans, null) && !ReferenceEquals(sqlCon, null) && sqlCon.State == ConnectionState.Open)
                {
                    try
                    {
                        sqlTrans.Rollback();
                    }
                    catch (InvalidOperationException ex)
                    {
                        LogHelper.LogError(Log, "Error occured in SaveNotesAndAttachments method.", ex);

                        var objCustomFault = new CustomFault(ex.Message, "SaveNotesAndAttachments", "SaveNotesAndAttachments",
                                                             "Common", ExceptionType.ApplicationException,
                                                             notesAndAttachments.NoteOrAttachmentName, false);
                        throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                              "Error while SaveNotesAndAttachments " + ex.Message + " Stack Trace: " + ex.StackTrace + "Inner Exception: " + ex.InnerException);
                    }
                }
                throw;
            }
            finally
            {
                if (!ReferenceEquals(sqlCon, null) && sqlCon.State != ConnectionState.Closed)
                {
                    sqlCon.Close();
                    sqlCon.Dispose();
                }
            }
            return result;

        }
        public List<NotesOrAttachments> GetNotesAndAttachments(byte level, long id, byte accessTypeId)
        {
            RefCountingDataReader objRefCountingDataReader = null;
            List<NotesOrAttachments> lstNotesOrAttachments = new List<NotesOrAttachments>();
            try
            {
                LogHelper.LogInfo(Log, "GetNotesAndAttachments Method Started for id=" + id);
                var sqlHelper = ContextSqlConn;
                objRefCountingDataReader = (RefCountingDataReader)sqlHelper.ExecuteReader(ReqSqlConstants.USP_P2P_REQ_GETREQUISITIONNOTESORATTACHMENTS, new object[] { id, level, accessTypeId });
                if (objRefCountingDataReader != null)
                {
                    var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;
                    while (sqlDr.Read())
                    {
                        lstNotesOrAttachments.Add(new NotesOrAttachments
                        {
                            NotesOrAttachmentId = GetLongValue(sqlDr, ReqSqlConstants.COL_NOTES_ATTACH_ID),
                            DocumentCode = GetLongValue(sqlDr, ReqSqlConstants.COL_NOTES_ATTACH_REQID),
                            LineItemId = GetLongValue(sqlDr, ReqSqlConstants.COL_NOTES_ATTACH_ITEMID),
                            FileId = GetLongValue(sqlDr, ReqSqlConstants.COL_ItemImageFileId),
                            NoteOrAttachmentName = GetStringValue(sqlDr, ReqSqlConstants.COL_NOTES_ATTACH_NAME),
                            NoteOrAttachmentDescription = GetStringValue(sqlDr, ReqSqlConstants.COL_NOTES_ATTACH_DESC),
                            NoteOrAttachmentType = (NoteOrAttachmentType)GetTinyIntValue(sqlDr, ReqSqlConstants.COL_NOTES_ATTACH_TYPE),
                            AccessTypeId = (NoteOrAttachmentAccessType)GetTinyIntValue(sqlDr, ReqSqlConstants.COL_NOTES_ATTACH_ACCESSTYPE),
                            SourceType = (SourceType)GetTinyIntValue(sqlDr, ReqSqlConstants.COL_SOURCE_TYPE),
                            IsEditable = GetBoolValue(sqlDr, ReqSqlConstants.COL_ISEDITABLE),
                            CategoryTypeId = GetIntValue(sqlDr, ReqSqlConstants.COL_NOTES_ATTACH_CATEGORYTYPEID),
                            CreatedBy = GetLongValue(sqlDr, ReqSqlConstants.COL_CREATED_BY),
                            CreatorName = GetStringValue(sqlDr, ReqSqlConstants.COL_CREATED_BY_NAME),
                            DateCreated = GetDateTimeValue(sqlDr, ReqSqlConstants.COL_DATE_CREATED),
                            ModifiedBy = GetLongValue(sqlDr, ReqSqlConstants.COL_MODIFIED_BY),
                            ModifiedDate = GetDateTimeValue(sqlDr, ReqSqlConstants.COL_NOTES_ATTACH_MODIFIEDDATE),
                            FileSize = GetDecimalValue(sqlDr, ReqSqlConstants.COL_NOTES_ATTACH_FILESIZE)
                        });
                    }
                }
            }
            finally
            {
                if (!ReferenceEquals(objRefCountingDataReader, null) && !objRefCountingDataReader.IsClosed)
                {
                    objRefCountingDataReader.Close();
                    objRefCountingDataReader.Dispose();
                }

                LogHelper.LogInfo(Log, "GetNotesAndAttachments Method Ended for id=" + id);
            }
            return lstNotesOrAttachments;
        }
        public bool DeleteNotesAndAttachments(byte level, List<long> notesOrAttachmentsIds)
        {
            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;
            try
            {
                LogHelper.LogInfo(Log, "DeleteNotesAndAttachments = " + level);

                var sqlHelper = ContextSqlConn;
                _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();

                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("Requisition DeleteNotesAndAttachments: notesOrAttachmentsIds=" + notesOrAttachmentsIds.ToJSON(), " was called."));

                bool result = false;
                DataTable dtnotesOrAttachments = new DataTable();
                dtnotesOrAttachments.Columns.Add("NotesOrAttachmentsId", typeof(long));

                if (notesOrAttachmentsIds != null && notesOrAttachmentsIds.Any())
                {
                    foreach (long item in notesOrAttachmentsIds)
                    {
                        if (item != null)
                        {
                            DataRow dr = dtnotesOrAttachments.NewRow();
                            dr["NotesOrAttachmentsId"] = item;
                            dtnotesOrAttachments.Rows.Add(dr);
                        }
                    }
                }

                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_DELETEREQUISITIONNOTESORATTACHMENTS))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.Add(new SqlParameter("@level", SqlDbType.TinyInt) { Value = level });
                    objSqlCommand.Parameters.Add(new SqlParameter("@tvp_p2p_NotesOrAttachmentsIds", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_P2P_NOTESORATTACHMENTS,
                        Value = dtnotesOrAttachments
                    });

                    result = Convert.ToBoolean(sqlHelper.ExecuteNonQuery(objSqlCommand, _sqlTrans), NumberFormatInfo.InvariantInfo);
                }

                if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
                    _sqlTrans.Commit();
                return result;
            }
            catch
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
                throw;
            }
            finally
            {
                if (!ReferenceEquals(_sqlCon, null) && _sqlCon.State != ConnectionState.Closed)
                {
                    _sqlCon.Close();
                    _sqlCon.Dispose();
                }

                LogHelper.LogInfo(Log, "Requisition DeleteNotesAndAttachments Method Ended for level = " + level);
            }
        }
        public Requisition GetRequisitionDetailsByReqItems(List<long> reqItemIds)
        {
            DataSet objDs = null;
            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;
            Requisition objRequisition = null;
            try
            {
                LogHelper.LogInfo(Log, "GetRequisitionDetailsByReqItems = " + reqItemIds.ToJSON());

                var sqlHelper = ContextSqlConn;
                _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();

                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("Requisition GetRequisitionDetailsByReqItems: reqItemIds=" + reqItemIds.ToJSON(), " was called."));

                DataTable dtReqItemId = new DataTable();
                dtReqItemId.Columns.Add("Id", typeof(long));
                if (dtReqItemId != null && reqItemIds.Any())
                {
                    foreach (long item in reqItemIds)
                    {
                        DataRow dr = dtReqItemId.NewRow();
                        dr["Id"] = item;
                        dtReqItemId.Rows.Add(dr);
                    }
                }

                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_GETLINEITEMSFORWORKBENCH))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.Add(new SqlParameter("@tvp_P2P_Ids", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_P2P_IDS,
                        Value = dtReqItemId
                    });
                    objDs = sqlHelper.ExecuteDataSet(objSqlCommand, _sqlTrans);
                }
                bool header = true;
                if (objDs != null && objDs.Tables.Count > 0)
                {

                    foreach (DataRow lstItems in objDs.Tables[0].Rows)
                    {

                        if (header)
                        {
                            objRequisition = new Requisition
                            {
                                Currency = Convert.ToString(lstItems[ReqSqlConstants.COL_CURRENCY], CultureInfo.InvariantCulture),
                                ShiptoLocation = new ShiptoLocation()
                                {
                                    ShiptoLocationId = lstItems[ReqSqlConstants.COL_SHIPTOLOC_ID] != null ? Convert.ToInt32(lstItems[ReqSqlConstants.COL_SHIPTOLOC_ID], CultureInfo.InvariantCulture) : 0
                                },
                                DelivertoLocation = new DelivertoLocation
                                {
                                    DelivertoLocationId = lstItems[ReqSqlConstants.COL_DELIVERTOLOCATION_ID] != null ? Convert.ToInt32(lstItems[ReqSqlConstants.COL_DELIVERTOLOCATION_ID], CultureInfo.InvariantCulture) : 0,
                                    DeliverTo = lstItems.Table.Columns.Contains(ReqSqlConstants.COL_DELIVERTO) ? (lstItems[ReqSqlConstants.COL_DELIVERTO] != null ? Convert.ToString(lstItems[ReqSqlConstants.COL_DELIVERTO]) : "") : ""
                                },
                                BilltoLocation = new BilltoLocation
                                {
                                    BilltoLocationId = lstItems[ReqSqlConstants.COL_BILLTOLOCATIONID] != null ? Convert.ToInt32(lstItems[ReqSqlConstants.COL_BILLTOLOCATIONID], CultureInfo.InvariantCulture) : 0
                                },
                                ProgramId = Convert.ToInt64(lstItems[ReqSqlConstants.COL_PROGRAM_ID], CultureInfo.InvariantCulture),
                                EntitySumList = new List<EntitySumCalculation>(),
                                PurchaseType = Convert.ToInt32(lstItems[ReqSqlConstants.COL_PURCHASETYPE], CultureInfo.InvariantCulture),
                                DocumentType = P2PDocumentType.Requisition,
                                BusinessUnitId = Convert.ToInt64(lstItems[ReqSqlConstants.COL_BUID] == DBNull.Value ? 0 : lstItems[ReqSqlConstants.COL_BUID]),
                                POSignatoryCode = Convert.ToInt64(lstItems[ReqSqlConstants.COL_POSIGNATORYCODE], CultureInfo.InvariantCulture),
                                POSignatoryName = Convert.ToString(lstItems[ReqSqlConstants.COL_POSIGNATORYNAME], CultureInfo.InvariantCulture),
                                WorkOrderNumber = Convert.ToString(lstItems[ReqSqlConstants.COL_WORKORDERNO], CultureInfo.InvariantCulture),
                                ERPOrderType = Convert.ToInt32(lstItems[ReqSqlConstants.COL_ERPORDERTYPE], CultureInfo.InvariantCulture),
                                BudgetId = Convert.ToInt32(lstItems[ReqSqlConstants.COL_BUDGETID], CultureInfo.InvariantCulture),
                                CostApprover = Convert.ToInt64(lstItems[ReqSqlConstants.COL_CostApprover], CultureInfo.InvariantCulture)
                            };
                            objRequisition.DocumentLOBDetails = new List<DocumentLOBDetails>();
                            if (objDs.Tables[3].Rows != null && objDs.Tables[3].Rows.Count > 0)
                            {
                                foreach (DataRow drLOB in objDs.Tables[3].Rows)
                                {
                                    DocumentLOBDetails objLOB = new DocumentLOBDetails
                                    {
                                        EntityDetailCode = Convert.ToInt64(drLOB[ReqSqlConstants.COL_ENTITYDETAILCODE]),
                                        EntityId = Convert.ToInt16(drLOB[ReqSqlConstants.COL_EntityId]),
                                        EntityCode = Convert.ToString(drLOB[ReqSqlConstants.COL_ENTITY_CODE]),
                                        EntityDisplayName = Convert.ToString(drLOB[ReqSqlConstants.COL_ENTITYNAME])
                                    };
                                    objRequisition.DocumentLOBDetails.Add(objLOB);
                                }
                            }
                            header = false;
                            var obj = GetDocumentAdditionalEntityDetailsById((long)lstItems[ReqSqlConstants.COL_REQUISITION_ID]);
                            objRequisition.DocumentAdditionalEntitiesInfoList = obj.DocumentAdditionalEntitiesInfoList;
                            objRequisition.EntityDetailCode = obj.EntityDetailCode;
                            objRequisition.CustomAttrFormId = Convert.ToInt64(lstItems[ReqSqlConstants.COL_FORMID], CultureInfo.InvariantCulture);
                            objRequisition.CustomAttrFormIdForItem = Convert.ToInt64(lstItems[ReqSqlConstants.COL_ITEMFORMID], CultureInfo.InvariantCulture);
                            objRequisition.CustomAttrFormIdForSplit = Convert.ToInt64(lstItems[ReqSqlConstants.COL_SPLITFORMID], CultureInfo.InvariantCulture);
                            objRequisition.RequisitionItems = new List<RequisitionItem>();
                        }

                        //REQUISITION LINE ITEM DETAILS
                        var objRequisitionItem = new RequisitionItem
                        {
                            DocumentItemId = (long)lstItems[ReqSqlConstants.COL_REQUISITION_ITEM_ID],
                            P2PLineItemId = (long)lstItems[ReqSqlConstants.COL_P2P_LINE_ITEM_ID],
                            DocumentId = (long)lstItems[ReqSqlConstants.COL_REQUISITION_ID],
                            ShortName = Convert.ToString(lstItems[ReqSqlConstants.COL_SHORT_NAME], CultureInfo.InvariantCulture),
                            Description = Convert.ToString(lstItems[ReqSqlConstants.COL_DESCRIPTION], CultureInfo.InvariantCulture),
                            UnitPrice = (!string.IsNullOrWhiteSpace(lstItems[ReqSqlConstants.COL_UNIT_PRICE].ToString())) ? (Convert.ToDecimal(lstItems[ReqSqlConstants.COL_UNIT_PRICE])) : default(decimal?),
                            Quantity = Convert.ToDecimal(lstItems[ReqSqlConstants.COL_QUANTITY], CultureInfo.InvariantCulture),
                            UOM = Convert.ToString(lstItems[ReqSqlConstants.COL_UOM], CultureInfo.InvariantCulture),
                            UOMDesc = Convert.ToString(lstItems[ReqSqlConstants.COL_UOM_DESC], CultureInfo.InvariantCulture),
                            DateRequested = Convert.IsDBNull(lstItems[ReqSqlConstants.COL_DATE_REQUESTED]) ? DateTime.MinValue : Convert.ToDateTime(lstItems[ReqSqlConstants.COL_DATE_REQUESTED], CultureInfo.InvariantCulture),
                            DateNeeded = Convert.IsDBNull(lstItems[ReqSqlConstants.COL_DATE_NEEDED]) ? DateTime.MinValue : Convert.ToDateTime(lstItems[ReqSqlConstants.COL_DATE_NEEDED], CultureInfo.InvariantCulture),
                            PartnerCode = Convert.ToDecimal(lstItems[ReqSqlConstants.COL_PARTNER_CODE], CultureInfo.InvariantCulture),
                            CategoryId = (long)lstItems[ReqSqlConstants.COL_CATEGORY_ID],
                            ManufacturerName = Convert.ToString(lstItems[ReqSqlConstants.COL_MANUFACTURER_NAME], CultureInfo.InvariantCulture),
                            ManufacturerPartNumber = Convert.ToString(lstItems[ReqSqlConstants.COL_MANUFACTURER_PART_NUMBER], CultureInfo.InvariantCulture),
                            ManufacturerModel = Convert.ToString(lstItems[ReqSqlConstants.COL_MANUFACTURER_MODEL], CultureInfo.InvariantCulture),
                            ItemType = (ItemType)Convert.ToInt16(lstItems[ReqSqlConstants.COL_ITEM_TYPE_ID], CultureInfo.InvariantCulture),
                            Tax = (!string.IsNullOrWhiteSpace(lstItems[ReqSqlConstants.COL_LINE_ITEM_TAX].ToString())) ? (Convert.ToDecimal(lstItems[ReqSqlConstants.COL_LINE_ITEM_TAX])) : default(decimal?),
                            ShippingCharges = (!string.IsNullOrWhiteSpace(lstItems[ReqSqlConstants.COL_SHIPPING_CHARGES].ToString())) ? (Convert.ToDecimal(lstItems[ReqSqlConstants.COL_SHIPPING_CHARGES])) : default(decimal?),
                            AdditionalCharges = (!string.IsNullOrWhiteSpace(lstItems[ReqSqlConstants.COL_ADDITIONAL_CHARGES].ToString())) ? (Convert.ToDecimal(lstItems[ReqSqlConstants.COL_ADDITIONAL_CHARGES])) : default(decimal?),
                            StartDate = Convert.IsDBNull(lstItems[ReqSqlConstants.COL_START_DATE]) ? DateTime.MinValue : Convert.ToDateTime(lstItems[ReqSqlConstants.COL_START_DATE], CultureInfo.InvariantCulture),
                            EndDate = Convert.IsDBNull(lstItems[ReqSqlConstants.COL_END_DATE]) ? DateTime.MinValue : Convert.ToDateTime(lstItems[ReqSqlConstants.COL_END_DATE], CultureInfo.InvariantCulture),
                            TotalRecords = Convert.ToInt32(lstItems[ReqSqlConstants.COL_TOTAL_RECORDS], CultureInfo.InvariantCulture),
                            SourceType = (ItemSourceType)Convert.ToInt16(lstItems[ReqSqlConstants.COL_SOURCE_TYPE], CultureInfo.InvariantCulture),
                            MinimumOrderQuantity = Convert.ToDecimal(lstItems[ReqSqlConstants.COL_MIN_ORDER_QUANTITY], CultureInfo.InvariantCulture),
                            MaximumOrderQuantity = Convert.ToDecimal(lstItems[ReqSqlConstants.COL_MAX_ORDER_QUANTITY], CultureInfo.InvariantCulture),
                            Banding = Convert.ToInt32(lstItems[ReqSqlConstants.COL_BANDING], CultureInfo.InvariantCulture),
                            ItemCode = (long)lstItems[ReqSqlConstants.COL_ITEM_CODE],
                            RequisitionStatus = (DocumentStatus)Convert.ToInt16(lstItems[ReqSqlConstants.COL_DOCUMENT_STATUS], CultureInfo.InvariantCulture),
                            AllowDecimalsForUom = Convert.ToBoolean(lstItems[ReqSqlConstants.COL_UOM_ALLOWDECIMAL], CultureInfo.InvariantCulture),
                            Currency = lstItems.Table.Columns.Contains(ReqSqlConstants.COL_ITEMCURRENCY) ? Convert.ToString(lstItems[ReqSqlConstants.COL_ITEMCURRENCY], CultureInfo.InvariantCulture) : string.Empty,
                            IsTaxExempt = Convert.ToBoolean(lstItems[ReqSqlConstants.COL_ISTAXEXEMPT], CultureInfo.InvariantCulture),
                            ItemExtendedType = (ItemExtendedType)Convert.ToInt16(lstItems[ReqSqlConstants.COL_ITEM_EXTENDED_TYPE], CultureInfo.InvariantCulture),
                            Efforts = (!string.IsNullOrWhiteSpace(lstItems[ReqSqlConstants.COL_EFFORTS].ToString())) ? (Convert.ToDecimal(lstItems[ReqSqlConstants.COL_EFFORTS])) : default(decimal?),
                            SupplierPartId = Convert.ToString(lstItems[ReqSqlConstants.COL_SUPPLIERPARTID], CultureInfo.InvariantCulture),
                            SupplierPartAuxiliaryId = Convert.ToString(lstItems[ReqSqlConstants.COL_SUPPLIERAUXILIARYPARTID], CultureInfo.InvariantCulture),
                            ItemNumber = Convert.ToString(lstItems[ReqSqlConstants.COL_ITEMNUMBER], CultureInfo.InvariantCulture),
                            CatalogItemId = (long)lstItems[ReqSqlConstants.COL_CATALOGITEMID],
                            Billable = Convert.ToString(lstItems[ReqSqlConstants.COL_BILLABLE], CultureInfo.InvariantCulture),
                            Capitalized = Convert.ToString(lstItems[ReqSqlConstants.COL_CAPITALIZED], CultureInfo.InvariantCulture),
                            CapitalCode = Convert.ToString(lstItems[ReqSqlConstants.COL_CAPITALCODE], CultureInfo.InvariantCulture),
                            ContractNo = Convert.ToString(lstItems[ReqSqlConstants.COL_CONTRACTNO], CultureInfo.InvariantCulture),
                            ItemLineNumber = Convert.ToInt32(lstItems[ReqSqlConstants.COL_ITEMLINENUMBER], CultureInfo.InvariantCulture),
                            OrderLocationId = (long)lstItems[ReqSqlConstants.COL_ORDER_LOCATIONID],
                            RemitToLocationId = (long)lstItems[ReqSqlConstants.COL_INVOICE_REMITTOLOCATIONID],
                            SourceSystemId = (long)lstItems[ReqSqlConstants.COL_SOURCESYSTEMID],
                            PartnerSourceSystemValue = Convert.ToString(lstItems[ReqSqlConstants.COL_PARTNERSOURCESYSTEMVALUE], CultureInfo.InvariantCulture),
                            PartnerReconMatchTypeId = Convert.ToInt32(lstItems[ReqSqlConstants.COL_PARTNERRECONMATCHTYPEID]),
                            leadTime = (long)lstItems[ReqSqlConstants.COL_LEADTIME],
                            IsERSEnabled = Convert.ToBoolean(lstItems[ReqSqlConstants.COL_ISERSENABLED], CultureInfo.InvariantCulture),
                            Itemspecification = Convert.ToString(lstItems[ReqSqlConstants.COL_ITEMSPECIFICATION], CultureInfo.InvariantCulture),
                            InternalPlantMemo = Convert.ToString(lstItems[ReqSqlConstants.COL_INTERNALPLANTMEMO], CultureInfo.InvariantCulture),
                            TaxJurisdiction = Convert.ToString(lstItems[ReqSqlConstants.COL_TAXJURISDICTION], CultureInfo.InvariantCulture),
                            IncoTermCode = Convert.ToString(lstItems[ReqSqlConstants.COL_INCOTERMCODE], CultureInfo.InvariantCulture),
                            IncoTermLocation = Convert.ToString(lstItems[ReqSqlConstants.COL_INCOTERMLOCATION], CultureInfo.InvariantCulture),
                            IncoTermId = Convert.ToInt32(lstItems[ReqSqlConstants.COL_INCOTERMID], CultureInfo.InvariantCulture),
                            SpendControlDocumentCode = Convert.ToInt64(lstItems[ReqSqlConstants.COL_SPENDCONTROLDOCUMENTCODE]),
                            SpendControlDocumentNumber = Convert.ToString(lstItems[ReqSqlConstants.COL_SPENDCONTROLDOCUMENTNUMBER], CultureInfo.InvariantCulture),
                            SpendControlDocumentItemId = (long)lstItems[ReqSqlConstants.COL_SPENDCONTROLDOCUMENTITEMID],
                            AdvanceReleaseDate = Convert.IsDBNull(lstItems[ReqSqlConstants.COL_ADVANCERELEASEDATE]) ? DateTime.MinValue : Convert.ToDateTime(lstItems[ReqSqlConstants.COL_ADVANCERELEASEDATE], CultureInfo.InvariantCulture),
                            AdvanceAmount = (!string.IsNullOrWhiteSpace(lstItems[ReqSqlConstants.COL_ADVANCEAMOUNT].ToString())) ? (Convert.ToDecimal(lstItems[ReqSqlConstants.COL_ADVANCEAMOUNT])) : default(decimal?),
                            AdvancePercentage = Convert.ToDecimal(lstItems[ReqSqlConstants.COL_ADVANCEPERCENTAGE], CultureInfo.InvariantCulture),
                            AllowAdvances = Convert.ToBoolean(lstItems[ReqSqlConstants.COL_ALLOWADVANCES], CultureInfo.InvariantCulture),
                            DelivertoStr = Convert.ToString(lstItems[ReqSqlConstants.COL_DELIVERTO], CultureInfo.InvariantCulture),
                            IsProcurable = Convert.ToInt32(lstItems[ReqSqlConstants.COL_IS_PROCURABLE], CultureInfo.InvariantCulture),
                            SplitType = (SplitType)Convert.ToInt32(lstItems[ReqSqlConstants.COL_SPLIT_TYPE], CultureInfo.InvariantCulture),
                            ShipFromLocationId = (long)lstItems[ReqSqlConstants.COL_SHIPFROMLOCATIONID],
                            MatchType = (MatchType)Convert.ToInt16(lstItems[ReqSqlConstants.COL_MATCHTYPE], CultureInfo.InvariantCulture),
                            InventoryType = Convert.IsDBNull(lstItems[ReqSqlConstants.COL_INVENTORYTYPE]) ? false : Convert.ToBoolean(lstItems[ReqSqlConstants.COL_INVENTORYTYPE]),
                            LineStatus = (StockReservationStatus)Convert.ToInt32(lstItems[ReqSqlConstants.COL_LineStatus], CultureInfo.InvariantCulture),
                            PriceTypeId = Convert.ToInt64(lstItems[ReqSqlConstants.COL_PRICETYPEID]),
                            JobTitleId = Convert.ToInt64(lstItems[ReqSqlConstants.COL_JOBTITLEID]),
                            ContingentWorkerId = Convert.ToInt64(lstItems[ReqSqlConstants.COL_CONTINGENTWORKERID]),
                            Margin = Convert.ToDecimal(lstItems[ReqSqlConstants.COL_MARGIN], CultureInfo.InvariantCulture),
                            BaseRate = Convert.ToDecimal(lstItems[ReqSqlConstants.COL_BASERATE], CultureInfo.InvariantCulture),
                            ReportingManagerId = Convert.ToInt64(lstItems[ReqSqlConstants.COL_REPORTINGMANAGERID]),
                            ContractLineRef = Convert.ToString(lstItems[ReqSqlConstants.COL_ContractLineReference], CultureInfo.InvariantCulture),
                            SmartFormId = (long)lstItems[ReqSqlConstants.COL_SMARTFORMID],
                            IsTaxUserEdited = Convert.ToBoolean(lstItems[ReqSqlConstants.COL_IsTaxUserEdited]),
                            PaymentTermId = Convert.ToInt32(lstItems[ReqSqlConstants.COL_PAYMENTTERMID], CultureInfo.InvariantCulture),
                            PartnerConfigurationId = (long)lstItems[ReqSqlConstants.COL_PARTNERCONFIGURATIONID],
                            TypeOfItem = Convert.ToInt32(lstItems[ReqSqlConstants.COL_TYPEOFITEM], CultureInfo.InvariantCulture),
                            ItemStatus = (DocumentStatus)Convert.ToInt16(lstItems[ReqSqlConstants.COL_ITEM_STATUS], CultureInfo.InvariantCulture),
                            ProcurementStatus = Convert.ToInt16(lstItems[ReqSqlConstants.COL_PROCUREMENTSTATUS], CultureInfo.InvariantCulture),
                            ConversionFactor = Convert.ToDecimal(lstItems[ReqSqlConstants.COL_CONVERSIONFACTOR], CultureInfo.InvariantCulture),
                            ContractItemId = (long)lstItems[ReqSqlConstants.COL_CONTRACTITEMID],
                            OverallItemLimit = Convert.ToDecimal(lstItems[ReqSqlConstants.COL_OVERALLITEMLIMIT], CultureInfo.InvariantCulture),
                            BuyerContactCode = (long)lstItems[ReqSqlConstants.COL_BUYERCONTACTCODE],
                            PartnerContactId = (long)lstItems[ReqSqlConstants.COL_PARTNER_CONTACTID],
                            TransmissionValue = Convert.ToString(lstItems[ReqSqlConstants.COL_REQUISITION_TRNVALUE], CultureInfo.InvariantCulture),
                            TrasmissionMode = Convert.ToInt32(lstItems[ReqSqlConstants.COL_REQUISITION_TRNMODE], CultureInfo.InvariantCulture),
                          AllowFlexiblePrice = Convert.ToBoolean(lstItems[ReqSqlConstants.COL_ALLOWFLEXIBLEPRICE], CultureInfo.InvariantCulture),
                        };
                        if (objRequisitionItem.ItemType == ItemType.Material)
                            objRequisitionItem.ItemTotalAmount = objRequisitionItem.UnitPrice * objRequisitionItem.Quantity + objRequisitionItem.Tax + objRequisitionItem.AdditionalCharges + objRequisitionItem.ShippingCharges;
                        else
                            objRequisitionItem.ItemTotalAmount = objRequisitionItem.UnitPrice * objRequisitionItem.Efforts + objRequisitionItem.Tax + objRequisitionItem.AdditionalCharges;

                        //Accounting Details
                        var drSplitsItems = objDs.Tables[1].Select("RequisitionItemId =" +
                                                objRequisitionItem.DocumentItemId.ToString(CultureInfo.InvariantCulture));
                        objRequisitionItem.ItemSplitsDetail = new List<RequisitionSplitItems>();
                        foreach (var split in drSplitsItems)
                        {
                            var objRequisitionSplitItems = new RequisitionSplitItems
                            {
                                DocumentItemId = (long)split[ReqSqlConstants.COL_REQUISITION_ITEM_ID],
                                DocumentSplitItemId = (long)split[ReqSqlConstants.COL_REQUISITION_SPLIT_ITEM_ID],
                                Percentage = Convert.ToDecimal(split[ReqSqlConstants.COL_PERCENTAGE], CultureInfo.InvariantCulture),
                                Quantity = Convert.ToDecimal(split[ReqSqlConstants.COL_QUANTITY], CultureInfo.InvariantCulture),
                                Tax = split[ReqSqlConstants.COL_TAX] != DBNull.Value ? Convert.ToDecimal(split[ReqSqlConstants.COL_TAX], CultureInfo.InvariantCulture) : (decimal?)null,
                                ShippingCharges = split[ReqSqlConstants.COL_SHIPPING_CHARGES] != DBNull.Value ? Convert.ToDecimal(split[ReqSqlConstants.COL_SHIPPING_CHARGES], CultureInfo.InvariantCulture) : (decimal?)null,
                                AdditionalCharges = split[ReqSqlConstants.COL_ADDITIONAL_CHARGES] != DBNull.Value ? Convert.ToDecimal(split[ReqSqlConstants.COL_ADDITIONAL_CHARGES], CultureInfo.InvariantCulture) : (decimal?)null,
                                SplitItemTotal = split[ReqSqlConstants.COL_SPLIT_ITEM_TOTAL] != DBNull.Value ? Convert.ToDecimal(split[ReqSqlConstants.COL_SPLIT_ITEM_TOTAL], CultureInfo.InvariantCulture) : (decimal?)null,
                                ErrorCode = Convert.ToString(split[ReqSqlConstants.COL_ERROR_CODE], CultureInfo.InvariantCulture),
                                TotalRecords = Convert.ToInt32(split[ReqSqlConstants.COL_TOTAL_RECORDS], CultureInfo.InvariantCulture),
                                SplitType = (SplitType)Convert.ToInt32(split[ReqSqlConstants.COL_SPLIT_TYPE], CultureInfo.InvariantCulture)
                            };
                            var drSplits = objDs.Tables[2].Select("RequisitionSplitItemId =" + objRequisitionSplitItems.DocumentSplitItemId.ToString(CultureInfo.InvariantCulture));
                            objRequisitionSplitItems.DocumentSplitItemEntities = new List<DocumentSplitItemEntity>();
                            foreach (var drSplit in drSplits)
                            {
                                var documentSplitItemEntity = new DocumentSplitItemEntity
                                {
                                    SplitAccountingFieldId = (int)drSplit[ReqSqlConstants.COL_SPLIT_ACCOUNTING_FIELD_CONFIG_ID],
                                    SplitAccountingFieldValue = drSplit[ReqSqlConstants.COL_SPLIT_ACCOUNTING_FIELD_VALUE].ToString(),
                                    EntityDisplayName = drSplit[ReqSqlConstants.COL_ENTITY_DISPLAY_NAME]?.ToString(),
                                    EntityTypeId = (int)drSplit[ReqSqlConstants.COL_ENTITY_TYPE_ID],
                                    EntityCode = drSplit[ReqSqlConstants.COL_ENTITY_CODE]?.ToString(),
                                    CodeCombinationOrder = Convert.ToInt16(drSplit[ReqSqlConstants.COL_CODE_COMBINATION_ORDER], CultureInfo.InvariantCulture),
                                    ParentEntityDetailCode = Convert.ToInt64(drSplit[ReqSqlConstants.COL_SPLIT_ITEM_PARENNTENTITYDETAILCODE]),
                                    EntityType = drSplit[ReqSqlConstants.COL_ENTITY_TYPE]?.ToString()
                                };
                                objRequisitionSplitItems.DocumentSplitItemEntities.Add(documentSplitItemEntity);

                                if (!string.IsNullOrEmpty(documentSplitItemEntity.EntityCode))
                                {
                                    bool found = false;
                                    foreach (EntitySumCalculation obj in objRequisition.EntitySumList)
                                    {
                                        if (obj.EntityCode == documentSplitItemEntity.EntityCode)
                                        {
                                            found = true;
                                            obj.TotalAmount += objRequisitionSplitItems.SplitItemTotal.Value;
                                            break;
                                        }
                                    }
                                    if (!found)
                                    {
                                        objRequisition.EntitySumList.Add(
                                            new EntitySumCalculation()
                                            {
                                                EntityCode = documentSplitItemEntity.EntityCode,
                                                EntityTypeId = documentSplitItemEntity.EntityTypeId,
                                                TotalAmount = objRequisitionSplitItems.SplitItemTotal.Value
                                            }
                                        );
                                    }
                                }
                            }
                            objRequisitionItem.ItemSplitsDetail.Add(objRequisitionSplitItems);
                        }
                        //Shipping Details
                        var drShippingDetails = objDs.Tables[5].Select("RequisitionItemId =" +
                                                                      objRequisitionItem.DocumentItemId.ToString(CultureInfo.InvariantCulture));
                        objRequisitionItem.DocumentItemShippingDetails = new List<DocumentItemShippingDetail>();
                        foreach (var drShipping in drShippingDetails)
                        {
                            var objDocumentItemShippingDetail = new DocumentItemShippingDetail
                            {
                                DocumentItemShippingId = 0,
                                Quantity = Convert.ToDecimal(drShipping[ReqSqlConstants.COL_QUANTITY], CultureInfo.InvariantCulture),
                                ShippingMethod = Convert.ToString(drShipping[ReqSqlConstants.COL_SHIPPINGMETHOD]),
                                ShiptoLocation = new ShiptoLocation
                                {
                                    ShiptoLocationId = Convert.ToInt32(drShipping[ReqSqlConstants.COL_SHIPTOLOC_ID], CultureInfo.InvariantCulture),
                                    ShiptoLocationName = Convert.ToString(drShipping[ReqSqlConstants.COL_SHIPTOLOC_NAME])

                                },
                                DelivertoLocation = new DelivertoLocation
                                {
                                    DelivertoLocationId = Convert.ToInt32(drShipping[ReqSqlConstants.COL_DELIVERTOLOC_ID], CultureInfo.InvariantCulture),
                                    DelivertoLocationName = Convert.ToString(drShipping[ReqSqlConstants.COL_DELIVERTOLOC_NAME]),
                                    DeliverTo = Convert.ToString(drShipping[ReqSqlConstants.COL_DELIVERTO])

                                }
                            };
                            objRequisitionItem.DocumentItemShippingDetails.Add(objDocumentItemShippingDetail);
                        }
                        //Notes And Attachments
                        var drNotesandAttachments = objDs.Tables[4].Select("RequisitionItemId =" +
                                                objRequisitionItem.DocumentItemId.ToString(CultureInfo.InvariantCulture));
                        objRequisitionItem.ListNotesOrAttachments = new List<NotesOrAttachments>();
                        foreach (var drNotes in drNotesandAttachments)
                        {
                            var NotesOrAttachments = new NotesOrAttachments
                            {
                                NotesOrAttachmentId = (long)drNotes[ReqSqlConstants.COL_NOTES_ATTACH_ID],
                                DocumentCode = 0,
                                LineItemId = 0,
                                FileId = (long)drNotes[ReqSqlConstants.COL_ItemImageFileId],
                                NoteOrAttachmentName = Convert.ToString(drNotes[ReqSqlConstants.COL_NOTES_ATTACH_NAME]),
                                NoteOrAttachmentDescription = Convert.ToString(drNotes[ReqSqlConstants.COL_NOTES_ATTACH_DESC]),
                                NoteOrAttachmentType = (NoteOrAttachmentType)Convert.ToInt16(drNotes[ReqSqlConstants.COL_NOTES_ATTACH_TYPE], CultureInfo.InvariantCulture),
                                AccessTypeId = (NoteOrAttachmentAccessType)Convert.ToInt16(drNotes[ReqSqlConstants.COL_NOTES_ATTACH_ACCESSTYPE], CultureInfo.InvariantCulture),
                                SourceType = (SourceType)Convert.ToInt16(drNotes[ReqSqlConstants.COL_SOURCE_TYPE], CultureInfo.InvariantCulture),
                                IsEditable = Convert.ToBoolean(drNotes[ReqSqlConstants.COL_ISEDITABLE], CultureInfo.InvariantCulture),
                                CategoryTypeId = Convert.ToInt32(drNotes[ReqSqlConstants.COL_NOTES_ATTACH_CATEGORYTYPEID], CultureInfo.InvariantCulture),
                                CreatedBy = (long)drNotes[ReqSqlConstants.COL_CREATED_BY],
                                DateCreated = Convert.IsDBNull(drNotes[ReqSqlConstants.COL_DATE_CREATED]) ? DateTime.MinValue : Convert.ToDateTime(drNotes[ReqSqlConstants.COL_DATE_CREATED], CultureInfo.InvariantCulture),
                                ModifiedBy = (long)drNotes[ReqSqlConstants.COL_MODIFIED_BY],
                                ModifiedDate = Convert.IsDBNull(drNotes[ReqSqlConstants.COL_NOTES_ATTACH_MODIFIEDDATE]) ? DateTime.MinValue : Convert.ToDateTime(drNotes[ReqSqlConstants.COL_NOTES_ATTACH_MODIFIEDDATE], CultureInfo.InvariantCulture),
                                P2PLineItemID = objRequisitionItem.P2PLineItemId
                            };
                            objRequisitionItem.ListNotesOrAttachments.Add(NotesOrAttachments);
                        }
                        objRequisition.RequisitionItems.Add(objRequisitionItem);
                    }
                    if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
                        _sqlTrans.Commit();
                }
                return objRequisition;
            }
            catch
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
                throw;
            }
            finally
            {
                if (!ReferenceEquals(_sqlCon, null) && _sqlCon.State != ConnectionState.Closed)
                {
                    _sqlCon.Close();
                    _sqlCon.Dispose();
                }

                LogHelper.LogInfo(Log, "Requisition GetRequisitionDetailsByReqItems Method Ended");
            }
        }
        public bool UpdateRequisitionBuyerContactCode(List<KeyValuePair<long, long>> lstReqItemsToUpdate)
        {
            LogHelper.LogInfo(Log, "UpdateRequisitionBuyerContactCode Method Started");
            SqlConnection objSqlCon = null;
            bool result = false;

            var lstErrors = new List<string>();
            try
            {
                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("In UpdateRequisitionBuyerContactCode Method.",
                                            "SP: USP_P2P_REQ_UpdateBuyerToRequisitionItems, with parameters: lstReqItemsToUpdate = " + lstReqItemsToUpdate.ToJSON()));

                var sqlHelper = ContextSqlConn;
                objSqlCon = (SqlConnection)sqlHelper.CreateConnection();
                objSqlCon.Open();

                DataTable dtReqItems = new DataTable();
                dtReqItems.Columns.Add("ItemId", typeof(long));
                dtReqItems.Columns.Add("BuyerContactCode", typeof(long));
                foreach (var item in lstReqItemsToUpdate)
                {
                    DataRow dr = dtReqItems.NewRow();
                    dr["ItemId"] = item.Key;
                    dr["BuyerContactCode"] = item.Value;
                    dtReqItems.Rows.Add(dr);

                }
                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_UPDATEBUYERTOREQUISITIONITEMS))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    SqlParameter objSqlParameter = new SqlParameter("@tvp_P2P_RequisitionItemBuyerUpdate", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_P2P_REQUISITIONITEMBUYERUPDATE,
                        Value = dtReqItems
                    };

                    objSqlCommand.Parameters.Add(objSqlParameter);
                    result = Convert.ToBoolean(sqlHelper.ExecuteNonQuery(objSqlCommand));
                    return result;
                }
            }
            finally
            {
                if (!ReferenceEquals(objSqlCon, null) && objSqlCon.State != ConnectionState.Closed)
                {
                    objSqlCon.Close();
                    objSqlCon.Dispose();
                }
                LogHelper.LogInfo(Log, "Requisition UpdateRequisitionBuyerContactCode Method Ended");
            }

            return result;
        }
        public List<NewP2PEntities.DocumentInfo> GetOrdersListForWorkBench(string reqItemIds)
        {
            RefCountingDataReader objRefCountingDataReader = null;
            List<NewP2PEntities.DocumentInfo> lstOrders = new List<NewP2PEntities.DocumentInfo>();
            try
            {
                LogHelper.LogInfo(Log, "GetOrdersListForWorkBench Method Started for id=" + reqItemIds);

                var sqlHelper = ContextSqlConn;
                objRefCountingDataReader = (RefCountingDataReader)sqlHelper.ExecuteReader(ReqSqlConstants.USP_P2P_REQ_GETPOITEMSFORWORKBENCH, new object[] {
                    reqItemIds});
                if (objRefCountingDataReader != null)
                {
                    var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;
                    while (sqlDr.Read())
                    {
                        lstOrders.Add(new NewP2PEntities.DocumentInfo
                        {
                            DocumentCode = GetLongValue(sqlDr, ReqSqlConstants.COL_DOCUMENTCODE),
                            DocumentName = GetStringValue(sqlDr, ReqSqlConstants.COL_DOCUMENT_NAME),
                            DocumentNumber = GetStringValue(sqlDr, ReqSqlConstants.COL_DOCUMENT_NUMBER),
                            SupplierCode = GetDecimalValue(sqlDr, ReqSqlConstants.COL_PARTNER_CODE),
                            SupplierName = GetStringValue(sqlDr, ReqSqlConstants.COL_PARTNER_NAME),
                            CurrencyCode = GetStringValue(sqlDr, ReqSqlConstants.COL_CURRENCY),
                            BUDetails = GetStringValue(sqlDr, ReqSqlConstants.COL_DOCUMENTBUSE),
                            BusinessUnitName = GetStringValue(sqlDr, ReqSqlConstants.COL_BU_BusinessUnitName),
                            Selected = GetStringValue(sqlDr, ReqSqlConstants.COL_SELECTED),
                            Included = GetStringValue(sqlDr, ReqSqlConstants.COL_INCLUDED),
                            Excluded = GetStringValue(sqlDr, ReqSqlConstants.COL_EXCLUDED),
                            RemitToLocationId = GetLongValue(sqlDr, ReqSqlConstants.COL_REMITTOLOCATIONID),
                            OrderToLocationId = GetLongValue(sqlDr, ReqSqlConstants.COL_ORDERTOLOCATIONID),
                            selectedItems = GetStringValue(sqlDr, ReqSqlConstants.COL_SELECTEDITEMS),
                            PurchaseTypeDesc = GetStringValue(sqlDr, ReqSqlConstants.COL_PURCHASETYPEDESC),
                            ProgramDesc = GetStringValue(sqlDr, ReqSqlConstants.COL_PROGRAMDESC)
                        });

                    }
                }
            }
            finally
            {
                if (!ReferenceEquals(objRefCountingDataReader, null) && !objRefCountingDataReader.IsClosed)
                {
                    objRefCountingDataReader.Close();
                    objRefCountingDataReader.Dispose();
                }

                LogHelper.LogInfo(Log, "GetOrdersListForWorkBench Method Ended for id=" + reqItemIds);
            }
            return lstOrders;

        }
        //private DataTable ConvertChargeItemIdToTableType(List<ItemCharge> lstItemCharge)
        //{
        //    DataTable dtChargeItems = new DataTable() { Locale = CultureInfo.InvariantCulture, TableName = ReqSqlConstants.TVP_CHARGEITEMID };
        //    dtChargeItems.Columns.Add("ItemChargeId", typeof(long));

        //    if (lstItemCharge != null && lstItemCharge.Any())
        //    {
        //        foreach (var ChargeItem in lstItemCharge)
        //        {
        //            if (ChargeItem != null)
        //            {
        //                DataRow dr = dtChargeItems.NewRow();
        //                dr["ItemChargeId"] = ChargeItem.ItemChargeId;
        //                dtChargeItems.Rows.Add(dr);
        //            }
        //        }
        //    }
        //    return dtChargeItems;
        //}
        //public bool DeleteChargeAndSplitsItemsByItemChargeId(List<ItemCharge> lstItemCharge, long ChangeRequisitionItemId)
        //{
        //    SqlConnection _sqlCon = null;
        //    SqlTransaction _sqlTrans = null;
        //    Boolean result = false;
        //    try
        //    {
        //        LogHelper.LogInfo(Log, "Requisition DeleteChargeAndSplitsItemsByItemChargeId Method Started ");

        //        var sqlHelper = ContextSqlConn;
        //        _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
        //        _sqlCon.Open();
        //        _sqlTrans = _sqlCon.BeginTransaction();

        //        if (ChangeRequisitionItemId > 0)
        //        {
        //            if (Log.IsDebugEnabled)
        //                Log.Debug(string.Format(CultureInfo.InvariantCulture, "Requisition DeleteChargeAndSplitsItemsByItemChargeId sp USP_P2P_REQ_DELETECHARGEANDSPLITSITEMSBYITEMCHARGEID"));

        //            using (var objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_DELETECHARGEANDSPLITSITEMSBYITEMCHARGEID, _sqlCon))
        //            {
        //                objSqlCommand.CommandType = CommandType.StoredProcedure;
        //                objSqlCommand.Parameters.Add(new SqlParameter("@tvp_ChargeItemId", SqlDbType.Structured)
        //                {
        //                    TypeName = "tvp_ChargeItemId",
        //                    Value = ConvertChargeItemIdToTableType(lstItemCharge)
        //                });
        //                result = Convert.ToBoolean(ContextSqlConn.ExecuteScalar(objSqlCommand, _sqlTrans), CultureInfo.InvariantCulture);
        //            }

        //            if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
        //                _sqlTrans.Commit();
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
        //        LogHelper.LogInfo(Log, "Requisition DeleteChargeAndSplitsItemsByItemChargeId Method Ended ");
        //    }
        //    return result;
        //}
        public List<ItemCharge> GetLineItemCharges(List<long> reqItemIds, long documentCode)
        {
            List<ItemCharge> lstItemCharge = new List<ItemCharge>();
            try
            {
                DataSet dsResult = new DataSet();
                SqlConnection _sqlCon = null;
                SqlTransaction _sqlTrans = null;
                try
                {
                    LogHelper.LogInfo(Log, "Requisition GetLineItemCharges Method Started ");

                    var sqlHelper = ContextSqlConn;
                    _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                    _sqlCon.Open();
                    _sqlTrans = _sqlCon.BeginTransaction();

                    if (Log.IsDebugEnabled)
                        Log.Debug(string.Concat("Requisition GetLineItemCharges  with parameter: documentCode=" + documentCode, " was called."));

                    DataTable dtReqItemId = new DataTable();
                    dtReqItemId.Columns.Add("Id", typeof(long));
                    if (dtReqItemId != null && reqItemIds.Any())
                    {
                        foreach (long item in reqItemIds)
                        {
                            DataRow dr = dtReqItemId.NewRow();
                            dr["Id"] = item;
                            dtReqItemId.Rows.Add(dr);
                        }
                    }

                    using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_GETLINEITEMCHARGES))
                    {
                        objSqlCommand.CommandType = CommandType.StoredProcedure;
                        objSqlCommand.Parameters.Add(new SqlParameter("@documentCode ", documentCode));
                        objSqlCommand.Parameters.Add(new SqlParameter("@tvp_P2P_Ids", SqlDbType.Structured)
                        {
                            TypeName = ReqSqlConstants.TVP_P2P_IDS,
                            Value = dtReqItemId
                        });

                        dsResult = sqlHelper.ExecuteDataSet(objSqlCommand, _sqlTrans);

                        if (dsResult.Tables.Count > 0 && dsResult.Tables[0].Rows.Count > 0)
                        {
                            foreach (DataRow HeaderItemChargeDataRow in dsResult.Tables[0].Rows)
                            {
                                ItemCharge objItemCharge = new ItemCharge();
                                objItemCharge.ItemChargeId = !Convert.IsDBNull(HeaderItemChargeDataRow[ReqSqlConstants.COL_ITEMCHARGEID]) ? Convert.ToInt64(HeaderItemChargeDataRow[ReqSqlConstants.COL_ITEMCHARGEID]) : default(int);
                                objItemCharge.LineNumber = !Convert.IsDBNull(HeaderItemChargeDataRow[ReqSqlConstants.COL_LINENUMBER]) ? Convert.ToInt64(HeaderItemChargeDataRow[ReqSqlConstants.COL_LINENUMBER]) : default(Int64);
                                objItemCharge.CalculationValue = !Convert.IsDBNull(HeaderItemChargeDataRow[ReqSqlConstants.COL_CALCULATIONVALUE]) ? Convert.ToDecimal(HeaderItemChargeDataRow[ReqSqlConstants.COL_CALCULATIONVALUE]) : default(decimal);
                                objItemCharge.AdditionInfo = !Convert.IsDBNull(HeaderItemChargeDataRow[ReqSqlConstants.COL_ADDITIONALINFO]) ? Convert.ToString(HeaderItemChargeDataRow[ReqSqlConstants.COL_ADDITIONALINFO]) : default(string);
                                objItemCharge.ChargeAmount = !Convert.IsDBNull(HeaderItemChargeDataRow[ReqSqlConstants.COL_CHARGEAMOUNT]) ? Convert.ToDecimal(HeaderItemChargeDataRow[ReqSqlConstants.COL_CHARGEAMOUNT]) : default(decimal);
                                objItemCharge.P2PLineItemID = !Convert.IsDBNull(HeaderItemChargeDataRow[ReqSqlConstants.COL_P2P_LINE_ITEM_ID]) ? Convert.ToInt64(HeaderItemChargeDataRow[ReqSqlConstants.COL_P2P_LINE_ITEM_ID]) : default(Int64);

                                objItemCharge.ChargeDetails = new ChargeMaster
                                {
                                    ChargeMasterId = !Convert.IsDBNull(HeaderItemChargeDataRow[ReqSqlConstants.COL_CHARGEMASTERID]) ? Convert.ToInt64(HeaderItemChargeDataRow[ReqSqlConstants.COL_CHARGEMASTERID]) : default(Int64),
                                    ChargeName = !Convert.IsDBNull(HeaderItemChargeDataRow[ReqSqlConstants.COL_CHARGENAME]) ? Convert.ToString(HeaderItemChargeDataRow[ReqSqlConstants.COL_CHARGENAME]) : default(string),
                                    ChargeDescription = !Convert.IsDBNull(HeaderItemChargeDataRow[ReqSqlConstants.COL_CHARGEDESCRIPTION]) ? Convert.ToString(HeaderItemChargeDataRow[ReqSqlConstants.COL_CHARGEDESCRIPTION]) : default(string),
                                    CalculationBasisId = !Convert.IsDBNull(HeaderItemChargeDataRow[ReqSqlConstants.COL_CALCULATIONBASISID]) ? Convert.ToInt32(HeaderItemChargeDataRow[ReqSqlConstants.COL_CALCULATIONBASISID]) : default(int),
                                    CalculationBasis = !Convert.IsDBNull(HeaderItemChargeDataRow[ReqSqlConstants.COL_NAME]) ? Convert.ToString(HeaderItemChargeDataRow[ReqSqlConstants.COL_NAME]) : default(string),
                                    IsIncludeForRetainage = !Convert.IsDBNull(HeaderItemChargeDataRow[ReqSqlConstants.COL_ISINCLUDEFORRETAINAGE]) ? Convert.ToBoolean(HeaderItemChargeDataRow[ReqSqlConstants.COL_ISINCLUDEFORRETAINAGE]) : default(Boolean),
                                    IsIncludeForTax = !Convert.IsDBNull(HeaderItemChargeDataRow[ReqSqlConstants.COL_ISINCLUDEFORTAX]) ? Convert.ToBoolean(HeaderItemChargeDataRow[ReqSqlConstants.COL_ISINCLUDEFORTAX]) : default(Boolean),
                                    TolerancePercentage = !Convert.IsDBNull(HeaderItemChargeDataRow[ReqSqlConstants.COL_TOLERANCEPERCENTAGE]) ? Convert.ToDecimal(HeaderItemChargeDataRow[ReqSqlConstants.COL_TOLERANCEPERCENTAGE]) : default(decimal),
                                    IsAllowance = !Convert.IsDBNull(HeaderItemChargeDataRow[ReqSqlConstants.COL_ISALLOWANCE]) ? Convert.ToBoolean(HeaderItemChargeDataRow[ReqSqlConstants.COL_ISALLOWANCE]) : default(Boolean),
                                    IsEditableOnInvoice = !Convert.IsDBNull(HeaderItemChargeDataRow[ReqSqlConstants.COL_ISEDITABLEONINVOICE]) ? Convert.ToBoolean(HeaderItemChargeDataRow[ReqSqlConstants.COL_ISEDITABLEONINVOICE]) : default(Boolean),
                                    ChargeTypeName = !Convert.IsDBNull(HeaderItemChargeDataRow[ReqSqlConstants.COL_CHARGETYPENAME]) ? Convert.ToString(HeaderItemChargeDataRow[ReqSqlConstants.COL_CHARGETYPENAME]) : default(string),
                                    ChargeTypeCode = !Convert.IsDBNull(HeaderItemChargeDataRow[ReqSqlConstants.COL_CHARGETYPECODE]) ? Convert.ToInt32(HeaderItemChargeDataRow[ReqSqlConstants.COL_CHARGETYPECODE]) : default(int)
                                };

                                objItemCharge.ItemSplitsDetail = new List<OrderSplitItems>();
                                var drSplititems = dsResult.Tables[1].Select("RequisitionItemID =" +
                                                                           objItemCharge.ItemChargeId.ToString
                                                                               (CultureInfo.InvariantCulture));
                                objItemCharge.ItemChargeId = 0;
                                foreach (DataRow split in drSplititems)
                                {
                                    var objRequisitionSplitItems = new OrderSplitItems
                                    {
                                        DocumentItemId = Convert.ToInt64(split[ReqSqlConstants.COL_REQUISITION_ITEM_ID], CultureInfo.InvariantCulture),
                                        DocumentSplitItemId = Convert.ToInt64(split[ReqSqlConstants.COL_REQUISITION_SPLIT_ITEM_ID], CultureInfo.InvariantCulture),
                                        Percentage = Convert.ToDecimal(split[ReqSqlConstants.COL_PERCENTAGE], CultureInfo.InvariantCulture),
                                        Quantity = Convert.ToDecimal(split[ReqSqlConstants.COL_QUANTITY], CultureInfo.InvariantCulture),
                                        Tax = Convert.ToDecimal(split[ReqSqlConstants.COL_TAX], CultureInfo.InvariantCulture),
                                        ShippingCharges = Convert.ToDecimal(split[ReqSqlConstants.COL_SHIPPING_CHARGES], CultureInfo.InvariantCulture),
                                        AdditionalCharges = Convert.ToDecimal(split[ReqSqlConstants.COL_ADDITIONAL_CHARGES], CultureInfo.InvariantCulture),
                                        SplitItemTotal = Convert.ToDecimal(split[ReqSqlConstants.COL_SPLIT_ITEM_TOTAL], CultureInfo.InvariantCulture),
                                        ErrorCode = Convert.ToString(split[ReqSqlConstants.COL_ERROR_CODE], CultureInfo.InvariantCulture),
                                        TotalRecords = Convert.ToInt32(split[ReqSqlConstants.COL_TOTAL_RECORDS], CultureInfo.InvariantCulture),
                                        SplitType = (SplitType)Convert.ToInt32(split[ReqSqlConstants.COL_SPLIT_TYPE], CultureInfo.InvariantCulture)

                                    };

                                    var drSplits = dsResult.Tables[2].Select("RequisitionSplitItemId =" +
                                                                           objRequisitionSplitItems.DocumentSplitItemId.ToString
                                                                               (CultureInfo.InvariantCulture));

                                    objRequisitionSplitItems.DocumentSplitItemId = 0;
                                    objRequisitionSplitItems.DocumentSplitItemEntities = new List<DocumentSplitItemEntity>();
                                    foreach (var drSplit in drSplits)
                                    {
                                        var documentSplitItemEntity = new DocumentSplitItemEntity();
                                        documentSplitItemEntity.DocumentSplitItemId = objRequisitionSplitItems.DocumentSplitItemId;
                                        documentSplitItemEntity.SplitAccountingFieldId = (int)drSplit[ReqSqlConstants.COL_SPLIT_ACCOUNTING_FIELD_CONFIG_ID];
                                        documentSplitItemEntity.SplitAccountingFieldValue = drSplit[ReqSqlConstants.COL_SPLIT_ACCOUNTING_FIELD_VALUE].ToString();
                                        documentSplitItemEntity.EntityDisplayName = drSplit[ReqSqlConstants.COL_ENTITY_DISPLAY_NAME].ToString();
                                        documentSplitItemEntity.EntityTypeId = (int)drSplit[ReqSqlConstants.COL_ENTITY_TYPE_ID];
                                        documentSplitItemEntity.EntityCode = drSplit[ReqSqlConstants.COL_ENTITY_CODE].ToString();
                                        documentSplitItemEntity.CodeCombinationOrder = Convert.ToInt16(drSplit[ReqSqlConstants.COL_CODE_COMBINATION_ORDER], CultureInfo.InvariantCulture);
                                        documentSplitItemEntity.ParentEntityDetailCode = drSplit[ReqSqlConstants.COL_SPLIT_ITEM_PARENNTENTITYDETAILCODE] == null && Convert.ToString(drSplit[ReqSqlConstants.COL_SPLIT_ITEM_PARENNTENTITYDETAILCODE]) == "" ? 0 : (long)drSplit[ReqSqlConstants.COL_SPLIT_ITEM_PARENNTENTITYDETAILCODE];
                                        documentSplitItemEntity.EntityType = Convert.ToString(drSplit[ReqSqlConstants.COL_ENTITY_TYPE]);

                                        objRequisitionSplitItems.DocumentSplitItemEntities.Add(documentSplitItemEntity);
                                    }
                                    objItemCharge.ItemSplitsDetail.Add(objRequisitionSplitItems);
                                }
                                lstItemCharge.Add(objItemCharge);
                            }
                        }
                    }

                    if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
                        _sqlTrans.Commit();
                }
                catch
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
                    throw;
                }
                finally
                {
                    if (!ReferenceEquals(_sqlCon, null) && _sqlCon.State != ConnectionState.Closed)
                    {
                        _sqlCon.Close();
                        _sqlCon.Dispose();
                    }

                    LogHelper.LogInfo(Log, "Requisition Method Ends for GetLineItemCharges ");
                }
            }
            finally
            {
                LogHelper.LogInfo(Log, "Requisition Method Ends for GetLineItemCharges ");
            }
            return lstItemCharge;

        }

        public DataSet GetSplitAccountingFieldsWithDefaultValuesForInterface(DocumentType docType, LevelType levelType, int structureId, List<string> lstEntityCodes, string lobEntityCode = "")
        {
            throw new NotImplementedException();
        }

        public DataSet GetBuyerAssigneeDetails(long ContactCode, string SearchText, int StartIndex, int Size)
        {
            DataSet dsResult = new DataSet();
            try
            {
                SqlConnection _sqlCon = null;

                try
                {
                    LogHelper.LogInfo(Log, "Requisition GetBuyerAssigneeDetails Method Started ");
                    var sqlHelper = ContextSqlConn;
                    _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                    _sqlCon.Open();

                    if (Log.IsDebugEnabled)
                        Log.Debug(string.Concat("Requisition GetBuyerAssigneeDetails  with parameters:Search Text=" + SearchText + " ContactCode=" + ContactCode, " was called."));

                    dsResult = sqlHelper.ExecuteDataSet(ReqSqlConstants.USP_P2P_GETBUYERASSIGNEE_DETAILS, new object[] { ContactCode, SearchText, Size });
                }
                catch
                {
                    throw;
                }
                finally
                {
                    if (!ReferenceEquals(_sqlCon, null) && _sqlCon.State != ConnectionState.Closed)
                    {
                        _sqlCon.Close();
                        _sqlCon.Dispose();
                    }
                    LogHelper.LogInfo(Log, "Requisition Method Ends for GetBuyerAssigneeDetails ");
                }
            }
            finally
            {
                LogHelper.LogInfo(Log, "Requisition Method Ends for GetBuyerAssigneeDetails ");
            }
            return dsResult;
        }

        public void DeleteSplitItemsByItemId(long ItemId)
        {
            LogHelper.LogInfo(Log, "Invoice DeleteSplitItemsByItemId Method Started");
            SqlConnection objSqlCon = null;

            var lstErrors = new List<string>();
            try
            {
                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("In DeleteSplitItemsByItemId Method.",
                                            "SP: Usp_P2P_REQ_DeleteSplitItemsByItemId, with parameters: RequisitionItemId = " + ItemId));

                var sqlHelper = ContextSqlConn;
                objSqlCon = (SqlConnection)sqlHelper.CreateConnection();
                objSqlCon.Open();
                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_DELETESPLITITEMSBYITEMID))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.AddWithValue("@RequisitionItemId", ItemId);

                    sqlHelper.ExecuteNonQuery(objSqlCommand);

                }
            }
            finally
            {
                if (!ReferenceEquals(objSqlCon, null) && objSqlCon.State != ConnectionState.Closed)
                {
                    objSqlCon.Close();
                    objSqlCon.Dispose();
                }
                LogHelper.LogInfo(Log, "Invoice DeleteSplitItemsByItemId Method Ended");
            }
        }

        public Dictionary<string, int> GetRequisitionPunchoutItemCount(long RequisitionId)
        {
            RefCountingDataReader objRefCountingDataReader = null;
            Dictionary<string, int> RequisitionItemCount = new Dictionary<string, int>();

            try
            {
                LogHelper.LogInfo(Log, "GetRequisitionPunchoutItemCount Method Started for RequisitionId=" + RequisitionId);
                var sqlHelper = ContextSqlConn;
                objRefCountingDataReader = (RefCountingDataReader)sqlHelper.ExecuteReader(ReqSqlConstants.USP_P2P_GET_REQUISITION_ITEM_COUNT, new object[] { RequisitionId });
                if (objRefCountingDataReader != null)
                {
                    var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;
                    while (sqlDr.Read())
                    {
                        RequisitionItemCount.Add("TotalItemCount", GetIntValue(sqlDr, ReqSqlConstants.COL_TOTAL_ITEMS_COUNT));
                        RequisitionItemCount.Add("TotalPunchoutItemCount", GetIntValue(sqlDr, ReqSqlConstants.COL_TOTAL_PUNCHOUT_ITEMS_COUNT));
                    }
                }
            }
            finally
            {
                if (!ReferenceEquals(objRefCountingDataReader, null) && !objRefCountingDataReader.IsClosed)
                {
                    objRefCountingDataReader.Close();
                    objRefCountingDataReader.Dispose();
                }
                LogHelper.LogInfo(Log, "GetRequisitionPunchoutItemCount Method Started for RequisitionId=" + RequisitionId);
            }
            return RequisitionItemCount;
        }

        public bool SaveDefaultAccountingDetailsforADR(long documentId, List<ADRSplit> adrsplits, int precessionValue, int maxPrecessionforTotal, int maxPrecessionForTaxesAndCharges, bool saveDefaultGL)
        {
            bool flag = false;
            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;
            try
            {
                LogHelper.LogInfo(Log, "Requisition SaveDefaltAccountingSplit Method Started for requisitionId = " + documentId);

                var sqlHelper = ContextSqlConn;
                _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();
                DataTable dtReqItemEntities = null;
                DataTable newdtReqItemEntities = null;
                foreach (var item in adrsplits)
                {
                    dtReqItemEntities = P2P.DataAccessObjects.DAOHelper.ConvertToDataTable(item.docSplitEntities, GetRequisitionSplitItemEntitiesTable);
                    dtReqItemEntities.Columns.Add("DocumentItemId", typeof(string));
                    foreach (DataRow dr in dtReqItemEntities.Rows)
                    {
                        dr["DocumentItemId"] = item.Identifier.ToString();
                    }
                    if (newdtReqItemEntities == null)
                    {
                        newdtReqItemEntities = dtReqItemEntities.Copy();
                    }
                    else
                    {
                        foreach (DataRow dr in dtReqItemEntities.Rows)
                        {
                            newdtReqItemEntities.Rows.Add(dr.ItemArray);
                        }
                    }
                }
                using (var objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_SAVEREQUISITION_DEFAULT_ACCOUNTINGDETAILSADR, _sqlCon))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.CommandTimeout = 150;
                    objSqlCommand.Parameters.AddWithValue("@RequisitionId", documentId);
                    objSqlCommand.Parameters.Add(new SqlParameter("@tvp_P2P_SplitItemsEntities", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_P2P_ADRSPLITITEMSENTITIES,
                        Value = newdtReqItemEntities
                    });
                    objSqlCommand.Parameters.AddWithValue("@UserId", UserContext.ContactCode);
                    objSqlCommand.Parameters.AddWithValue("@precessionValue", precessionValue);
                    objSqlCommand.Parameters.AddWithValue("@MaxPrecissionTotal", maxPrecessionforTotal);
                    objSqlCommand.Parameters.AddWithValue("@MaxPrecessionValueForTaxAndCharges", maxPrecessionForTaxesAndCharges);


                    flag = Convert.ToBoolean(ContextSqlConn.ExecuteScalar(objSqlCommand), CultureInfo.InvariantCulture);
                }
                if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
                    _sqlTrans.Commit();

            }
            catch
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
                throw;
            }
            finally
            {
                if (!ReferenceEquals(_sqlCon, null) && _sqlCon.State != ConnectionState.Closed)
                {
                    _sqlCon.Close();
                    _sqlCon.Dispose();
                }
                LogHelper.LogInfo(Log, "Requisition SaveDefaltAccountingSplit Method Ended for requisitionId = " + documentId);
            }

            return flag;
        }

        public long SyncChangeRequisition(long documentCode)
        {
            //Initializing the Logger.
            LogHelper.LogInfo(Log, "SyncChangeRequisition Method Started.");
            SqlConnection objSqlCon = null;
            SqlTransaction objSqlTrans = null;
            long result = 0;
            try
            {
                if (documentCode > 0)
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
                    if (Log.IsDebugEnabled)
                        Log.Debug(
                            string.Concat("Sync Change Requisition with parameter: documentCode=" + documentCode,
                                          " was called."));

                    result = Convert.ToInt64(sqlHelper.ExecuteScalar(objSqlTrans, ReqSqlConstants.USP_P2P_REQ_SYNCCHANGEREQUISITION,
                                                               documentCode), NumberFormatInfo.InvariantInfo);
                    if (result == 0)
                    {
                        if (!ReferenceEquals(objSqlTrans, null) && !ReferenceEquals(objSqlCon, null) && objSqlCon.State == ConnectionState.Open)
                            objSqlTrans.Commit();
                        return result;
                    }

                    if (Log.IsDebugEnabled)
                        Log.Debug(
                            string.Concat("Change Requisition delete with parameter: documentCode=" + documentCode,
                                          " was called."));

                    bool isDisable = false;
                    bool isenable = false;
                    var objHideNewPOfromBuyer = new DocumentHideDetails
                    {
                        DocumentCode = documentCode,
                        IsBuyerVisible = true,
                        IsSupplierVisible = true,
                        DocumentTypeCode = (int)DocumentType.Requisition
                    };
                    isDisable = sqlDocumentDAO.SaveDocumentHideDetails(objHideNewPOfromBuyer);
                    var objEnableOldPOfromBuyer = new DocumentHideDetails
                    {
                        DocumentCode = result,
                        IsBuyerVisible = false,
                        IsSupplierVisible = false,
                        DocumentTypeCode = (int)DocumentType.Requisition
                    };
                    isenable = sqlDocumentDAO.SaveDocumentHideDetails(objEnableOldPOfromBuyer);

                    bool isDeleted = sqlDocumentDAO.DeleteDocumentById(result);

                    if (isDeleted && result > 0 && isenable && isDisable)
                    {
                        if (!ReferenceEquals(objSqlTrans, null) && !ReferenceEquals(objSqlCon, null) && objSqlCon.State == ConnectionState.Open)
                            objSqlTrans.Commit();
                    }
                    else
                    {
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
                        }
                    }
                }
            }
            catch
            {
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
                }

                throw;
            }
            finally
            {
                if (!ReferenceEquals(objSqlCon, null) && objSqlCon.State != ConnectionState.Closed)
                {
                    objSqlCon.Close();
                    objSqlCon.Dispose();
                }
                //Logger Ended.
                LogHelper.LogInfo(Log, "SyncChangeRequisition Method Ended");
            }
            return result;
        }
        public long WithdrawChangeRequisition(long documentCode)
        {
            SqlConnection sqlCon = null;
            SqlTransaction sqlTrans = null;
            long result = 0;
            try
            {
                LogHelper.LogInfo(Log, "In Requisition WithdrawChangeRequisition Method Started for documentCode=" + documentCode);
                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("Requisition WithdrawChangeRequisition sp USP_P2P_REQ_WithDrawChangeRequisition with parameter: documentCode=" + documentCode));
                var sqlHelper = ContextSqlConn;
                sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                sqlCon.Open();
                sqlTrans = sqlCon.BeginTransaction();
                using (var objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_WITHDRAWCHANGEREQUISITION))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.Add(new SqlParameter("@documentCode", documentCode));
                    result = Convert.ToInt64(sqlHelper.ExecuteScalar(objSqlCommand, sqlTrans), NumberFormatInfo.InvariantInfo);
                }

                if (result == 0)
                {
                    if (!ReferenceEquals(sqlTrans, null) && !ReferenceEquals(sqlCon, null) && sqlCon.State == ConnectionState.Open)
                        sqlTrans.Commit();
                    return result;
                }

                if (Log.IsDebugEnabled)
                    Log.Debug(
                        string.Concat("Change Requisition delete with parameter: documentCode=" + documentCode,
                                      " was called."));

                bool isDisable = false;
                bool isenable = false;
                SQLDocumentDAO sqlDocumentDAO = new SQLDocumentDAO();
                sqlDocumentDAO.SqlTransaction = sqlTrans;
                sqlDocumentDAO.ReliableSqlDatabase = sqlHelper;
                sqlDocumentDAO.UserContext = UserContext;
                sqlDocumentDAO.GepConfiguration = GepConfiguration;
                var objHideNewPOfromBuyer = new DocumentHideDetails
                {
                    DocumentCode = documentCode,
                    IsBuyerVisible = false,
                    IsSupplierVisible = false,
                    DocumentTypeCode = (int)DocumentType.Requisition
                };
                isDisable = sqlDocumentDAO.SaveDocumentHideDetails(objHideNewPOfromBuyer);
                var objEnableOldPOfromBuyer = new DocumentHideDetails
                {
                    DocumentCode = result,
                    IsBuyerVisible = true,
                    IsSupplierVisible = true,
                    DocumentTypeCode = (int)DocumentType.Requisition
                };
                isenable = sqlDocumentDAO.SaveDocumentHideDetails(objEnableOldPOfromBuyer);
                if (!ReferenceEquals(sqlTrans, null) && !ReferenceEquals(sqlCon, null) && sqlCon.State == ConnectionState.Open)
                    sqlTrans.Commit();
                return result;
            }
            catch
            {
                if (!ReferenceEquals(sqlTrans, null) && !ReferenceEquals(sqlCon, null) && sqlCon.State == ConnectionState.Open)
                {
                    try
                    {
                        sqlTrans.Rollback();
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
                if (!ReferenceEquals(sqlCon, null) && sqlCon.State != ConnectionState.Closed)
                {
                    sqlCon.Close();
                    sqlCon.Dispose();
                }
                LogHelper.LogInfo(Log, "Requisition WithdrawChangeRequisition Method Ended for itemIds = " + documentCode);
            }
        }

        public bool GetReqSplitItemsEntityChangeFlag(long documentCode, string approvalEntityTypeIds, ref Requisition objReq)
        {
            bool result = false;
            try
            {
                LogHelper.LogInfo(Log, "GetReqSplitItemsEntityChangeFlag Method Started for DocumentCode = " + documentCode);
                if (Log.IsDebugEnabled)
                    Log.Debug(string.Format(CultureInfo.InvariantCulture, "GetReqSplitItemsEntityChangeFlag SP usp_P2P_GetSplitItemsEntityChange"));

                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_GETSPLITITEMSENTITYCHANGE))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.AddWithValue("@documentCode", documentCode);
                    objSqlCommand.Parameters.AddWithValue("@entityTypeIds", approvalEntityTypeIds);
                    DataSet ds = ContextSqlConn.ExecuteDataSet(objSqlCommand);

                    if (ds.Tables.Count > 0)
                    {
                        result = true;

                        DataTable dtChangedSplitItems = ds.Tables[0];
                        if (objReq.RequisitionItems != null)
                        {
                            foreach (var item in objReq.RequisitionItems)
                            {
                                if (item.ItemSplitsDetail != null)
                                {
                                    foreach (var splitItem in item.ItemSplitsDetail)
                                    {
                                        splitItem.IsSplitEntityCodeChanged = false;
                                        if (dtChangedSplitItems != null && dtChangedSplitItems.Rows.Count > 0 && dtChangedSplitItems.AsEnumerable().Any(x => x.Field<long>(ReqSqlConstants.COL_SPLITITEMID) == splitItem.DocumentSplitItemId))
                                        {
                                            splitItem.IsSplitEntityCodeChanged = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in GetReqSplitItemsEntityChangeFlag method", ex);
                throw ex;
            }
            finally
            {
                LogHelper.LogInfo(Log, "GetReqSplitItemsEntityChangeFlag Method Completed for DocumentCode = " + documentCode);
            }
            return result;
        }
        /// <summary>
        /// used to check whether the document can be able to withdraw or not.
        /// this applies to only requisition when its status is approved and contains any item that are of stock type.
        /// </summary>
        /// <param name="documentId"></param>
        /// <returns></returns>
        public bool CheckWithDrawApprovedRequisition(long documentId)
        {
            SqlConnection _sqlCon = null;
            try
            {
                LogHelper.LogInfo(Log, "CheckWithDrawApprovedRequisition Method Started");

                var sqlHelper = ContextSqlConn;
                _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                _sqlCon.Open();

                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("In CheckWithDrawApprovedRequisition Method with parameter: documentId=" + documentId));


                var result = Convert.ToBoolean(sqlHelper.ExecuteScalar(ReqSqlConstants.USP_P2P_REQ_CheckWithDrawApprovedRequisition,
                                                                 new object[] { documentId }), NumberFormatInfo.InvariantInfo);
                return result;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in CheckWithDrawApprovedRequisition method.", ex);
                throw ex;

                // return false;
            }
            finally
            {
                if (!ReferenceEquals(_sqlCon, null) && _sqlCon.State != ConnectionState.Closed)
                {
                    _sqlCon.Close();
                    _sqlCon.Dispose();
                }

                if (Log.IsInfoEnabled)
                {
                    Log.Info("CheckWithDrawApprovedRequisition Method Ended");
                }
            }
        }

        public void SaveDocumentLinkInfo(Collection<DocumentLinkInfo> documentLinkInfoList)
        {

            base.SaveDocumentLinkedInfo(documentLinkInfoList);


        }

        //public List<long> GetRequisitionListForInterfaces(string docType, int docCount, int sourceSystemId)
        //{
        //    RefCountingDataReader objRefCountingDataReader = null;
        //    List<long> lstRequisitions = new List<long>();
        //    try
        //    {
        //        LogHelper.LogInfo(Log, "Requisition GetRequisitionListForInterfaces Method Started");

        //        objRefCountingDataReader =
        //            (RefCountingDataReader)
        //            ContextSqlConn.ExecuteReader(ReqSqlConstants.USP_P2P_REQ_GETREQUISIONSFORINTERFACE,
        //                                                            new object[] { docType, docCount, sourceSystemId });
        //        if (objRefCountingDataReader != null)
        //        {
        //            var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;

        //            while (sqlDr.Read())
        //            {
        //                lstRequisitions.Add(GetLongValue(sqlDr, ReqSqlConstants.COL_DOCUMENT_CODE));
        //            }
        //        }
        //    }
        //    finally
        //    {
        //        if (!ReferenceEquals(objRefCountingDataReader, null) && !objRefCountingDataReader.IsClosed)
        //        {
        //            objRefCountingDataReader.Close();
        //            objRefCountingDataReader.Dispose();
        //        }

        //        LogHelper.LogInfo(Log, "Requisition GetRequisitionListForInterfaces Method Started");
        //    }
        //    return lstRequisitions;
        //}

        //public DataSet ValidateInterfaceLineStatus(long buyerPartnerCode, DataTable dtRequisitionDetail)
        //{
        //    DataSet dsRequisitionDetail = new DataSet();
        //    DataTable objDtTableTypeCollection = new DataTable() { Locale = CultureInfo.InvariantCulture };
        //    LogHelper.LogInfo(Log, "Requisition ValidateInterfaceLineStatus Method Started");
        //    SqlConnection objSqlCon = null;
        //    SqlTransaction objSqlTrans = null;
        //    ReliableSqlDatabase sqlHelper;
        //    if (ReferenceEquals(null, dtRequisitionDetail))
        //    {
        //        if (Log.IsWarnEnabled)
        //            Log.Warn(string.Concat("In ValidateInterfaceLineStatus Method of class ", typeof(SQLRequestDAO).ToString(),
        //                                    "Parameter:  requisitionLineDetail is null"));
        //        throw new ArgumentNullException("requisitionLineDetail");
        //    }
        //    try
        //    {
        //        sqlHelper = ContextSqlConn;
        //        objSqlCon = (SqlConnection)sqlHelper.CreateConnection();

        //        using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_VALIDATEINTERFACELINESTATUS))
        //        {
        //            objSqlCommand.CommandType = CommandType.StoredProcedure;
        //            objSqlCommand.Parameters.Add(new SqlParameter("@tvp_P2P_RequisitionLineItem", SqlDbType.Structured)
        //            {
        //                TypeName = ReqSqlConstants.TVP_P2P_REQUISITIONLINEITEM,
        //                Value = dtRequisitionDetail
        //            });

        //            objSqlCon.Open();
        //            objSqlTrans = objSqlCon.BeginTransaction();
        //            dsRequisitionDetail = sqlHelper.ExecuteDataSet(objSqlCommand, objSqlTrans);

        //            if (!ReferenceEquals(objSqlTrans, null))
        //                objSqlTrans.Commit();
        //            objSqlCommand.Parameters.Clear();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.LogError(Log, "Error occured in ValidateInterfaceLineStatus method.", ex);
        //        throw ex;
        //    }
        //    finally
        //    {
        //        if (!ReferenceEquals(objSqlCon, null) && objSqlCon.State != ConnectionState.Closed)
        //        {
        //            objSqlCon.Close();
        //            objSqlCon.Dispose();
        //        }
        //    }
        //    return dsRequisitionDetail;
        //}

        public RequisitionLineStatusUpdateDetails UpdateRequisitionNotificationDetails(long requisitionId)
        {
            RequisitionLineStatusUpdateDetails objRequisitionLineStatusUpdateDetails = new RequisitionLineStatusUpdateDetails();
            try
            {
                DataSet dsResult = new DataSet();
                SqlConnection _sqlCon = null;
                SqlTransaction _sqlTrans = null;
                try
                {
                    LogHelper.LogInfo(Log, "Requisition UpdateRequisitionNotificationDetails Method Started ");

                    var sqlHelper = ContextSqlConn;
                    _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                    _sqlCon.Open();
                    _sqlTrans = _sqlCon.BeginTransaction();

                    if (Log.IsDebugEnabled)
                        Log.Debug(string.Concat("Requisition UpdateRequisitionNotificationDetails  with parameter: documentCode=" + requisitionId, " was called."));


                    using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_GETNOTIFICATIONDETAILSFORINTERFACE))
                    {
                        objSqlCommand.CommandType = CommandType.StoredProcedure;
                        objSqlCommand.Parameters.Add(new SqlParameter("@requisitionId ", requisitionId));

                        dsResult = sqlHelper.ExecuteDataSet(objSqlCommand, _sqlTrans);

                        if (dsResult.Tables.Count > 0 && dsResult.Tables[0].Rows.Count > 0)
                        {
                            foreach (DataRow HeaderRequisitionDataRow in dsResult.Tables[0].Rows)
                            {
                                objRequisitionLineStatusUpdateDetails.RequisitionNumber = !Convert.IsDBNull(HeaderRequisitionDataRow[ReqSqlConstants.COL_REQUISITION_NUMBER]) ? Convert.ToString(HeaderRequisitionDataRow[ReqSqlConstants.COL_REQUISITION_NUMBER]) : default(string);
                                objRequisitionLineStatusUpdateDetails.RequesterFirstName = !Convert.IsDBNull(HeaderRequisitionDataRow[ReqSqlConstants.COL_REQUESTER_FIRSTNAME]) ? Convert.ToString(HeaderRequisitionDataRow[ReqSqlConstants.COL_REQUESTER_FIRSTNAME]) : default(string);
                                objRequisitionLineStatusUpdateDetails.RequesterLastName = !Convert.IsDBNull(HeaderRequisitionDataRow[ReqSqlConstants.COL_REQUESTER_LASTNAME]) ? Convert.ToString(HeaderRequisitionDataRow[ReqSqlConstants.COL_REQUESTER_LASTNAME]) : default(string);
                                objRequisitionLineStatusUpdateDetails.RequesterEmailAddress = !Convert.IsDBNull(HeaderRequisitionDataRow[ReqSqlConstants.COL_REQUESTER_EMAILADDRESS]) ? Convert.ToString(HeaderRequisitionDataRow[ReqSqlConstants.COL_REQUESTER_EMAILADDRESS]) : default(string);
                                objRequisitionLineStatusUpdateDetails.RequesterContactCode = !Convert.IsDBNull(HeaderRequisitionDataRow[ReqSqlConstants.COL_REQUESTER_CONTACTCODE]) ? Convert.ToInt64(HeaderRequisitionDataRow[ReqSqlConstants.COL_REQUESTER_CONTACTCODE]) : default(Int64);
                                objRequisitionLineStatusUpdateDetails.OBUFirstName = !Convert.IsDBNull(HeaderRequisitionDataRow[ReqSqlConstants.COL_OBU_FIRSTNAME]) ? Convert.ToString(HeaderRequisitionDataRow[ReqSqlConstants.COL_OBU_FIRSTNAME]) : default(string);
                                objRequisitionLineStatusUpdateDetails.OBULastName = !Convert.IsDBNull(HeaderRequisitionDataRow[ReqSqlConstants.COL_OBU_LAST_NAME]) ? Convert.ToString(HeaderRequisitionDataRow[ReqSqlConstants.COL_OBU_LAST_NAME]) : default(string);
                                objRequisitionLineStatusUpdateDetails.OBUEmailAddress = !Convert.IsDBNull(HeaderRequisitionDataRow[ReqSqlConstants.COL_OBU_EMAILADDRESS]) ? Convert.ToString(HeaderRequisitionDataRow[ReqSqlConstants.COL_OBU_EMAILADDRESS]) : default(string);
                                objRequisitionLineStatusUpdateDetails.OBUContactCode = !Convert.IsDBNull(HeaderRequisitionDataRow[ReqSqlConstants.COL_OBU_CONTACTCODE]) ? Convert.ToInt64(HeaderRequisitionDataRow[ReqSqlConstants.COL_OBU_CONTACTCODE]) : default(Int64);
                                objRequisitionLineStatusUpdateDetails.RequisitionId = requisitionId;
                            }
                        }
                        if (dsResult.Tables.Count > 0 && dsResult.Tables[1].Rows.Count > 0)
                        {
                            objRequisitionLineStatusUpdateDetails.Items = new List<LineStatusRequisition>();
                            foreach (DataRow items in dsResult.Tables[1].Rows)
                            {
                                {
                                    var documentitems = new LineStatusRequisition();
                                    documentitems.LineNumber = Convert.ToInt64(items[ReqSqlConstants.COL_REQ_ITEM_LINENUMBER], CultureInfo.InvariantCulture);
                                    documentitems.LineStatus = (StockReservationStatus)(items[ReqSqlConstants.COL_LineStatus]);

                                    objRequisitionLineStatusUpdateDetails.Items.Add(documentitems);

                                };

                            }
                        }
                    }

                    if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
                        _sqlTrans.Commit();
                }
                catch
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
                    throw;
                }
                finally
                {
                    if (!ReferenceEquals(_sqlCon, null) && _sqlCon.State != ConnectionState.Closed)
                    {
                        _sqlCon.Close();
                        _sqlCon.Dispose();
                    }

                    LogHelper.LogInfo(Log, "Requisition Method Ends for UpdateRequisitionNotificationDetails ");
                }
            }
            finally
            {
                LogHelper.LogInfo(Log, "Requisition Method Ends for UpdateRequisitionNotificationDetails ");
            }
            return objRequisitionLineStatusUpdateDetails;

        }

        public bool UpdateRequisitionStatusForStockReservation(long RequisitionID)
        {
            LogHelper.LogInfo(Log, "Requisition UpdateRequisitionStatusForStockReservation Method Started for documentId = " + RequisitionID);
            SqlConnection objSqlCon = null;
            SqlTransaction objSqlTrans = null;
            try
            {
                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("In UpdateRequisitionStatusForStockReservation Method.",
                                            " SP: usp_P2P_REQ_UpdateRequsitionStatusFromInterface, with parameters: documentId = " + RequisitionID
                                             ));

                objSqlCon = (SqlConnection)ContextSqlConn.CreateConnection();
                objSqlCon.Open();

                var result = Convert.ToBoolean(ContextSqlConn.ExecuteNonQuery(ReqSqlConstants.USP_P2P_REQ_UPDATEREQUISITIONSTATUSFROMINTERFACE, RequisitionID));
                return result;
            }
            catch
            {
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
                }
                throw;
            }
            finally
            {
                if (!ReferenceEquals(objSqlCon, null) && objSqlCon.State != ConnectionState.Closed)
                {
                    objSqlCon.Close();
                    objSqlCon.Dispose();
                }
                LogHelper.LogInfo(Log, "Requisition UpdateRequisitionStatusForStockReservation Method Ended for documentId=" + RequisitionID);
            }
        }

        // Below method is copied from PO DAO
        public P2PDocument OrderGetDocumentAdditionalEntityDetailsById(long documentCode)
        {
            RefCountingDataReader objRefCountingDataReader = null;
            SqlDataReader sqlDr = null;
            Order objOrder = new Order();
            objOrder.DocumentAdditionalEntitiesInfoList = new Collection<DocumentAdditionalEntityInfo>();
            long LOBEntityDetailCode = 0;
            try
            {
                objRefCountingDataReader = (RefCountingDataReader)ContextSqlConn.ExecuteReader(ReqSqlConstants.USP_P2P_PO_GETORDERENTITYDETAILSBYID, new object[] { documentCode });

                if (objRefCountingDataReader != null)
                {
                    sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;
                    while (sqlDr.Read())
                    {
                        objOrder.DocumentAdditionalEntitiesInfoList.Add(new DocumentAdditionalEntityInfo
                        {
                            EntityDetailCode = GetLongValue(sqlDr, ReqSqlConstants.COL_SPLIT_ITEM_ENTITYDETAILCODE),
                            EntityId = GetIntValue(sqlDr, ReqSqlConstants.COL_ENTITY_ID),
                            EntityDisplayName = GetStringValue(sqlDr, ReqSqlConstants.COL_ENTITY_DISPLAY_NAME),
                            EntityCode = GetStringValue(sqlDr, ReqSqlConstants.COL_ENTITY_CODE)
                        });
                        LOBEntityDetailCode = GetLongValue(sqlDr, ReqSqlConstants.COL_LOBENTITYDETAILCODE);
                    }
                    objOrder.EntityDetailCode = new List<long>() { LOBEntityDetailCode };
                }
            }
            finally
            {
                if (!ReferenceEquals(sqlDr, null) && !sqlDr.IsClosed)
                {
                    sqlDr.Close();
                    sqlDr.Dispose();
                }
                if (!ReferenceEquals(objRefCountingDataReader, null) && !objRefCountingDataReader.IsClosed)
                {
                    objRefCountingDataReader.Close();
                    objRefCountingDataReader.Dispose();
                }
                LogHelper.LogInfo(Log, "GetDocumentAdditionalEntityDetailsById Method Ended for documentCode=" + documentCode);
            }
            return objOrder;
        }

        public RequisitionItemCustomAttributes GetCutsomAttributesForLines(List<long> reqLineItemIds, int sourceDocType, int targetDocType, string level)
        {
            RequisitionItemCustomAttributes reqItemCustomAttributes = new RequisitionItemCustomAttributes();

            RefCountingDataReader objRefCountingDataReader = null;
            SqlConnection objSqlCon = null;
            DataSet ds = new DataSet();

            try
            {

                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("In SQLRequsitionDAO GetCutsomAttributesForLines",
                                             "SP: usp_P2P_GetQuestionResponseByQSetCode"));
                objSqlCon = (SqlConnection)ContextSqlConn.CreateConnection();
                objSqlCon.Open();

                DataTable dtReqItemId = new DataTable();
                dtReqItemId.Columns.Add("Id", typeof(long));
                if (reqLineItemIds != null && reqLineItemIds.Any())
                {
                    foreach (long item in reqLineItemIds)
                    {
                        DataRow dr = dtReqItemId.NewRow();
                        dr["Id"] = item != 0 ? item : 0;
                        dtReqItemId.Rows.Add(dr);
                    }
                }
                else
                {
                    DataRow dr = dtReqItemId.NewRow();
                    dr["Id"] = 0;
                    dtReqItemId.Rows.Add(dr);
                }


                using (var objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_GETCUTSOMATTRIBUTESFORLINES))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.Add(new SqlParameter("@tvp_Long", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_LONG,
                        Value = dtReqItemId
                    });
                    objSqlCommand.Parameters.Add(new SqlParameter("@Level", level));
                    objSqlCommand.Parameters.Add(new SqlParameter("@sourceDocType", sourceDocType));
                    objSqlCommand.Parameters.Add(new SqlParameter("@targetDocType", targetDocType));
                    ds = ContextSqlConn.ExecuteDataSet(objSqlCommand);
                }

                reqItemCustomAttributes.cutsomAttributes = new List<QuestionBank.Entities.Question>();
                QuestionBank.Entities.Question objquestion = new QuestionBank.Entities.Question();
                if (ds?.Tables.Count > 0)
                {
                    foreach (DataRow question in ds.Tables[0]?.Rows)
                    {
                        var qQuestionId = Convert.ToInt64(question[ReqSqlConstants.COL_QUESTIONID]);
                        var questionTypeId = Convert.ToInt32(question[ReqSqlConstants.COL_QUESTIONTYPEID]);
                        objquestion = new QuestionBank.Entities.Question();
                        switch (questionTypeId)
                        {
                            case 6:
                            case 7:
                            case 8:
                            case 9:
                            case 29:
                                QuestionBank.Entities.MultipleChoiceQuestion objChoiceNewQuestion = new QuestionBank.Entities.MultipleChoiceQuestion();
                                foreach (DataRow item1 in ds.Tables[2]?.Rows)
                                {
                                    long lQuestionId = Convert.ToInt64(item1[ReqSqlConstants.COL_QUESTIONID]);
                                    if ((qQuestionId > 0 && lQuestionId > 0) && qQuestionId == lQuestionId)
                                    {
                                        objChoiceNewQuestion.QuestionId = lQuestionId;

                                        if (Convert.ToBoolean(item1[ReqSqlConstants.COL_HASCONDITIONALQUESTION]))
                                            objChoiceNewQuestion.HasConditionalQuestion = true;

                                        objChoiceNewQuestion.RowChoices.Add(new QuestionBank.Entities.QuestionRowChoice
                                        {
                                            QuestionId = lQuestionId,
                                            RowId = Convert.ToInt64(item1[ReqSqlConstants.COL_ROWID]),
                                            RowText = Convert.ToString(item1[ReqSqlConstants.COL_ROWTEXT]),
                                            RowDescription = Convert.ToString(item1[ReqSqlConstants.COL_ROWDESCRIPTION]),
                                            ChildQuestionSetCode = Convert.ToInt64(item1[ReqSqlConstants.COL_CHILDQUESTIONSETCODE]),
                                            IsDefault = Convert.ToBoolean(item1[ReqSqlConstants.COL_ISDEFAULT])
                                        });
                                    }
                                }
                                objquestion = objChoiceNewQuestion;
                                reqItemCustomAttributes.cutsomAttributes.Add(objquestion);
                                break;
                            case 10:
                                QuestionBank.Entities.DateTimeQuestion objDateTimeQuestion = new QuestionBank.Entities.DateTimeQuestion();
                                foreach (DataRow item3 in ds.Tables[4]?.Rows)
                                {
                                    long lQId = Convert.ToInt64(item3[ReqSqlConstants.COL_QUESTIONID]);
                                    if ((qQuestionId > 0 && lQId > 0) && qQuestionId == lQId)
                                    {
                                        objDateTimeQuestion.DateTimeType = (QuestionBank.Entities.DateTimeType)(Convert.ToInt64(item3[ReqSqlConstants.COL_DATETIMETYPE]));
                                        objDateTimeQuestion.DateTimeFormat = (QuestionBank.Entities.DateTimeFormat)(Convert.ToInt64(item3[ReqSqlConstants.COL_DATETIMEFORMAT]));
                                        objDateTimeQuestion.QuestionId = lQId;
                                    }
                                }

                                objquestion = objDateTimeQuestion;
                                reqItemCustomAttributes.cutsomAttributes.Add(objquestion);
                                break;
                            case 3:
                            case 4:
                            case 5:
                            case 11:
                            case 15:
                            case 16:
                                QuestionBank.Entities.MultipleMatrixQuestion objMatrixNewQuestion = new QuestionBank.Entities.MultipleMatrixQuestion();
                                foreach (DataRow item5 in ds.Tables[3]?.Rows)
                                {
                                    long lQId = Convert.ToInt64(item5[ReqSqlConstants.COL_QUESTIONID]);
                                    if ((qQuestionId > 0 && lQId > 0) && qQuestionId == lQId)
                                    {
                                        objMatrixNewQuestion.QuestionId = lQId;
                                        objMatrixNewQuestion.ColumnChoices.Add(new QuestionBank.Entities.QuestionColumnChoice
                                        {
                                            QuestionId = lQId,
                                            ColumnId = Convert.ToInt64(item5[ReqSqlConstants.COL_COLUMNID]),
                                            ColumnText = Convert.ToString(item5[ReqSqlConstants.COL_COLUMNTEXT]),
                                            ColumnDescription = Convert.ToString(item5[ReqSqlConstants.COL_COLUMNDESCRIPTION]),
                                            ChildQuestionSetCode = Convert.ToInt64(item5[ReqSqlConstants.COL_CHILDQUESTIONSETCODE]),
                                            ColumnType = (QuestionBank.Entities.ColumnType)Convert.ToInt32(item5[ReqSqlConstants.COL_COLUMNTYPE])
                                        });
                                    }
                                }
                                //Column Choice List
                                if (ds.Tables[11] != null && ds.Tables[11].Rows.Count > 0)
                                {
                                    foreach (DataRow drColumnChoice in ds.Tables[11]?.Rows)
                                    {
                                        QuestionBank.Entities.QuestionColumnChoice objColChoice = objMatrixNewQuestion.ColumnChoices.First(colchoice => colchoice.ColumnId == Convert.ToInt64(drColumnChoice[ReqSqlConstants.COL_COLUMN_ID], System.Globalization.CultureInfo.InvariantCulture));
                                        objColChoice.ListMatrixCellChoices.Add(new QuestionBank.Entities.MatrixCellChoice
                                        {
                                            CellChoiceId = Convert.ToInt64(drColumnChoice[ReqSqlConstants.COL_CELL_CHOICE_ID]),
                                            ColumnId = Convert.ToInt64(drColumnChoice[ReqSqlConstants.COL_COLUMN_ID]),
                                            RowId = Convert.ToInt64(drColumnChoice[ReqSqlConstants.COL_ROW_ID]),
                                            ChoiceValue = Convert.ToString(drColumnChoice[ReqSqlConstants.COL_CHOICE_VALUE]),
                                            ChoiceScore = Convert.ToDouble(drColumnChoice[ReqSqlConstants.COL_CHOICE_SCORE], CultureInfo.InvariantCulture),// GetDoubleValue(dr,Constants.COL_CHOICE_SCORE)
                                            IsDefault = Convert.ToBoolean(drColumnChoice[ReqSqlConstants.COL_IS_DEFAULT])
                                        });
                                    }
                                }

                                objquestion = objMatrixNewQuestion;
                                reqItemCustomAttributes.cutsomAttributes.Add(objquestion);
                                break;
                            case 1:
                            case 2:
                            case 17:
                            case 28:
                                objquestion = new QuestionBank.Entities.Question();
                                objquestion.QuestionId = qQuestionId;
                                reqItemCustomAttributes.cutsomAttributes.Add(objquestion);
                                break;
                            case 19:
                            case 20:
                            case 21:
                            case 22:
                            case 23:
                            case 24:
                            case 25:
                            case 26:
                            case 27:
                                QuestionBank.Entities.DBLookUpQuestion objNewQuestion = new QuestionBank.Entities.DBLookUpQuestion();
                                foreach (DataRow item8 in ds.Tables[5]?.Rows)
                                {
                                    long lQuestionId = Convert.ToInt64(item8[ReqSqlConstants.COL_QUESTIONID]);
                                    if ((qQuestionId > 0 && lQuestionId > 0) && qQuestionId == lQuestionId)
                                    {
                                        objNewQuestion.AllowSingleSelect = Convert.ToBoolean(item8[ReqSqlConstants.COL_ALLOW_SINGLE_SELECT]);
                                        objNewQuestion.QuestionId = lQuestionId;
                                        objNewQuestion.DBLookUpFieldConfig = new QuestionBank.Entities.DBLookUpFieldConfig()
                                        {
                                            FieldTypeId = Convert.ToInt16(item8[ReqSqlConstants.COL_FIELD_TYPE_ID]),
                                            FieldGetSPName = Convert.ToString(item8[ReqSqlConstants.COL_FIELD_GET_SP_NAME]),
                                            FieldTypeName = Convert.ToString(item8[ReqSqlConstants.COL_FIELD_TYPE_NAME]),
                                            LocalizationSufix = Convert.ToString(item8[ReqSqlConstants.COL_LOCALIZATION_SUFIX]),
                                            IsAutosuggest = Convert.ToBoolean(item8[ReqSqlConstants.COL_ISAUTOSUGGEST])//,
                                                                                                                       //CustomDbConditionalQuestionList = GetCustomDBConditionalQuestions(dr)
                                        };
                                        if (questionTypeId == 27)
                                        {
                                            foreach (DataRow item9 in ds.Tables[6]?.Rows)
                                            {
                                                long lQId = Convert.ToInt64(item9[ReqSqlConstants.COL_QUESTIONID]);
                                                if ((lQId > 0 && lQuestionId > 0) && lQId == lQuestionId)
                                                {
                                                    objNewQuestion.DBLookUpFieldConfig.CustomDbConditionalQuestionList.Add(new QuestionBank.Entities.CustomDbConditionalQuestion
                                                    {
                                                        ConditionalId = Convert.ToInt64(item9[ReqSqlConstants.COL_CONDITIONAL_ID]),
                                                        RowId = Convert.ToInt64(item9[ReqSqlConstants.COL_ROWID]),
                                                        ChildQuestionSetCode = Convert.ToInt64(item9[ReqSqlConstants.COL_CHILDQUESTIONSETCODE], System.Globalization.CultureInfo.InvariantCulture)
                                                    });
                                                }
                                            }
                                        }
                                    }
                                }
                                reqItemCustomAttributes.cutsomAttributes.Add(objNewQuestion);
                                break;

                            case 14:
                                QuestionBank.Entities.AttachmentQuestion objAttachmentQuestion = new QuestionBank.Entities.AttachmentQuestion();

                                foreach (DataRow item11 in ds.Tables[7]?.Rows)
                                {
                                    long lQuestionId = Convert.ToInt64(item11[ReqSqlConstants.COL_QUESTIONID]);
                                    if ((qQuestionId > 0 && lQuestionId > 0) && qQuestionId == lQuestionId)
                                    {
                                        objAttachmentQuestion.QuestionId = lQuestionId;
                                        objAttachmentQuestion.MaxFileSizeInMB = Convert.ToInt32(item11[ReqSqlConstants.COL_MAX_FILE_SIZE_IN_MB]);
                                        objAttachmentQuestion.FileTypeFilter = Convert.ToString(item11[ReqSqlConstants.COL_FILE_TYPE_FILTER]);
                                    }
                                }

                                foreach (DataRow item12 in ds.Tables[8]?.Rows)
                                {
                                    var qObjectInstanceId = Convert.ToInt64(item12[ReqSqlConstants.COL_OBJECTINSTANCEID]);
                                    long lQuestionId = Convert.ToInt64(item12[ReqSqlConstants.COL_QUESTIONID]);
                                    if (qQuestionId > 0 && lQuestionId > 0 && qQuestionId == lQuestionId)
                                    {
                                        objAttachmentQuestion.ListQuestionAttachment.Add(new QuestionBank.Entities.QuestionAttachment
                                        {
                                            QuestionId = lQuestionId,
                                            AttachmentId = Convert.ToInt32(item12[ReqSqlConstants.COL_IS_ATTACHMENTID]),
                                            FileName = Convert.ToString(item12[ReqSqlConstants.COL_FILENAME]),
                                            IsDeleted = Convert.ToBoolean(item12[ReqSqlConstants.COL_ISDELETE])

                                        });
                                    }
                                }
                                reqItemCustomAttributes.cutsomAttributes.Add(objAttachmentQuestion);
                                break;

                            case 18:
                                QuestionBank.Entities.TextBoxQuestion objTextQuestion = new QuestionBank.Entities.TextBoxQuestion();
                                foreach (DataRow item14 in ds.Tables[10]?.Rows)
                                {
                                    long lQuestionId = Convert.ToInt64(item14[ReqSqlConstants.COL_QUESTIONID]);
                                    if ((qQuestionId > 0 && lQuestionId > 0) && qQuestionId == lQuestionId)
                                    {
                                        objTextQuestion.QuestionId = lQuestionId;
                                        objTextQuestion.DoesValidateText = Convert.ToInt32(item14[ReqSqlConstants.COL_DOES_VALIDATE_TEXT]);
                                        objTextQuestion.TextFormatType = (QuestionBank.Entities.TextFormatType)Convert.ToInt32(item14[ReqSqlConstants.COL_TEXT_FORMAT_TYPE]);
                                        objTextQuestion.TextFormatRangeMin = Convert.ToDouble(item14[ReqSqlConstants.COL_TEXT_FORMAT_RANGE_MIN]);
                                        objTextQuestion.TextFormatRangeMax = Convert.ToDouble(item14[ReqSqlConstants.COL_TEXT_FORMAT_RANGE_MAX]);
                                        objTextQuestion.TextInvalidFormatErrorMessage = Convert.ToString(item14[ReqSqlConstants.COL_TEXT_INVALID_FORMAT_ERROR_MESSAGE]);
                                        objTextQuestion.Formula = Convert.ToString(item14[ReqSqlConstants.COL_TEXT_FORMULA]);
                                    }
                                }
                                reqItemCustomAttributes.cutsomAttributes.Add(objTextQuestion);
                                break;
                        }
                    }

                    foreach (DataRow item in ds.Tables[0]?.Rows)
                    {
                        var qObjectInstanceId = Convert.ToInt64(item[ReqSqlConstants.COL_OBJECTINSTANCEID]);
                        var lQuestionId = Convert.ToInt64(item[ReqSqlConstants.COL_QUESTIONID]);
                        foreach (QuestionBank.Entities.Question question in reqItemCustomAttributes.cutsomAttributes)
                        {
                            if ((question.QuestionId > 0 && lQuestionId > 0) && question.QuestionId == lQuestionId)
                            {
                                question.QuestionId = lQuestionId;
                                question.QuestionSetCode = Convert.ToInt64(item[ReqSqlConstants.COL_QUESTIONNAIRECODE]);
                                question.QuestionSortOrder = Convert.ToInt32(item[ReqSqlConstants.COL_SORTORDER]);
                                question.QuestionDescription = Convert.ToString(item[ReqSqlConstants.COL_QUESTIONNAIREDESCRIPTION]);
                                question.QuestionText = Convert.ToString(item[ReqSqlConstants.COL_QUESTIONTEXT]);
                                question.QuestionTypeInfo = new QuestionBank.Entities.QuestionType { QuestionTypeId = Convert.ToInt32(item[ReqSqlConstants.COL_QUESTIONTYPEID]) };
                                question.IsMandatory = Convert.ToBoolean(item[ReqSqlConstants.COL_ISMANDATORY]);
                                question.IsAllowAttachment = Convert.ToBoolean(item[ReqSqlConstants.COL_IS_ALLOWATTACHMENT]);
                                question.IsInformative = Convert.ToBoolean(item[ReqSqlConstants.COL_ISINFORMATIVE]);

                            }
                        }
                    }

                    reqItemCustomAttributes.ListQuestionResponse = new List<QuestionBank.Entities.Question>();
                    foreach (long itemsId in reqLineItemIds)
                    {
                        QuestionBank.Entities.Question question = new QuestionBank.Entities.Question();

                        foreach (DataRow item7 in ds.Tables[1]?.Rows)
                        {
                            long lQuestionId = Convert.ToInt64(item7[ReqSqlConstants.COL_QUESTIONID]);
                            long lObjectInstanceId = Convert.ToInt64(item7[ReqSqlConstants.COL_OBJECTINSTANCEID]);
                            question.NoOfAttachment = 0;
                            if (lObjectInstanceId == itemsId)
                            {
                                if (Convert.ToInt32(item7[ReqSqlConstants.COL_NUMBER_OF_ATTACHMENTS]) > 0)
                                    question.NoOfAttachment = Convert.ToInt32(item7[ReqSqlConstants.COL_NUMBER_OF_ATTACHMENTS]);
                                question.ListQuestionResponses.Add(new QuestionBank.Entities.QuestionResponse
                                {
                                    QuestionId = lQuestionId,
                                    ResponseValue = Convert.ToString(item7[ReqSqlConstants.COL_RESPONSEVALUE]),
                                    ObjectInstanceId = lObjectInstanceId,
                                    RowId = Convert.ToInt32(item7[ReqSqlConstants.COL_ROWID]),
                                    ColumnId = Convert.ToInt32(item7[ReqSqlConstants.COL_COLUMNID]),
                                    AssesseeId = -1,
                                    AssessorId = -1,
                                    AssessorType = QuestionBank.Entities.AssessorUserType.Buyer
                                });
                            }
                        }
                        reqItemCustomAttributes.ListQuestionResponse.Add(question);

                        QuestionBank.Entities.AttachmentQuestion objAttachmentQuestion = new QuestionBank.Entities.AttachmentQuestion();

                        foreach (DataRow item13 in ds.Tables[9]?.Rows)
                        {
                            var qObjectInstanceId = Convert.ToInt64(item13[ReqSqlConstants.COL_OBJECTINSTANCEID]);
                            long lQuestionId = Convert.ToInt64(item13[ReqSqlConstants.COL_QUESTIONID]);
                            if (qObjectInstanceId == itemsId)
                            {
                                objAttachmentQuestion.ListQuestionResponseAttachment.Add(new QuestionBank.Entities.QuestionResponseAttachment
                                {
                                    QuestionId = lQuestionId,
                                    AttachmentId = Convert.ToInt32(item13[ReqSqlConstants.COL_IS_ATTACHMENTID]),
                                    FileName = Convert.ToString(item13[ReqSqlConstants.COL_FILENAME]),
                                    IsDeleted = Convert.ToBoolean(item13[ReqSqlConstants.COL_ISDELETE]),
                                    UploadedDate = Convert.ToDateTime(item13[ReqSqlConstants.COL_UPLOADEDDATE]),
                                    ObjectInstanceId = Convert.ToInt32(item13[ReqSqlConstants.COL_OBJECTINSTANCEID]),
                                    AssessorType = (QuestionBank.Entities.AssessorUserType)Convert.ToInt32(item13[ReqSqlConstants.COL_ASSESSORTYPE]),
                                    AssesseeId = Convert.ToInt32(item13[ReqSqlConstants.COL_ASSESSEEID]),
                                    AssessorId = Convert.ToInt32(item13[ReqSqlConstants.COL_ASSESSORID])
                                });
                                reqItemCustomAttributes.ListQuestionResponse.Add(objAttachmentQuestion);
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in GetCutsomAttributesForLines Method in SQLReuquisitionDAO", ex);
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
            }
            return reqItemCustomAttributes;
        }
        private List<KeyValuePair<Type, string>> GetItemIdsTable()
        {
            var lstItemIds = new List<KeyValuePair<Type, string>>();
            lstItemIds.Add(new KeyValuePair<Type, string>(typeof(long), "Id"));
            return lstItemIds;
        }
        public RiskFormDetails GetRiskFormQuestionScore()
        {
            RiskFormDetails objRiskFormDetails = new RiskFormDetails();
            var objRiskFormQuestionScore = new List<RiskFormQuestionScore>();

            try
            {
                LogHelper.LogInfo(Log, "In SQLRequisitionDAO GetRiskFormQuestionScore Method Started.");


                var objDataset = ContextSqlConn.ExecuteDataSet(ReqSqlConstants.USP_P2P_REQ_GETRISKFORMQUESTIONSCORE);

                if (objDataset != null && objDataset.Tables.Count > 0)
                {

                    foreach (DataRow items in objDataset.Tables[0].Rows)
                    {
                        var riskFormQuestionScore = new RiskFormQuestionScore();
                        riskFormQuestionScore.QuestionId = Convert.ToInt32(items[ReqSqlConstants.COL_QUESTIONID]);
                        riskFormQuestionScore.RowId = Convert.ToInt32(items[ReqSqlConstants.COL_ROWID]);
                        riskFormQuestionScore.Response = Convert.ToString(items[ReqSqlConstants.COL_RESPONSE]);
                        riskFormQuestionScore.Section = Convert.ToByte(items[ReqSqlConstants.COL_SECTION]);
                        riskFormQuestionScore.ResponseWeight = Convert.ToByte(items[ReqSqlConstants.COL_RESPONSEWEIGHT]);
                        objRiskFormQuestionScore.Add(riskFormQuestionScore);
                    }
                    objRiskFormDetails.lstRiskFormQuestionScore = objRiskFormQuestionScore;
                }
            }
            finally
            {
                LogHelper.LogInfo(Log, "SQLRequisitionDAO GetRiskFormQuestionScore Method completed.");
            }

            return objRiskFormDetails;

        }

        public RiskFormDetails GetRiskFormHeaderInstructionsText()
        {
            RiskFormDetails objRiskFormDetails = new RiskFormDetails();
            var objRiskFormQuestionScore = new List<RiskFormQuestionScore>();

            try
            {
                LogHelper.LogInfo(Log, "In SQLRequisitionDAO GetRiskFormHeaderInstructionsText Method Started.");

                var objDataset = ContextSqlConn.ExecuteDataSet(ReqSqlConstants.USP_P2P_REQ_GETRISKFORMHEADERINSTRUCTIONSTEXT);

                if (objDataset != null && objDataset.Tables.Count > 0 && objDataset.Tables[0].Rows.Count > 0)
                {
                    objRiskFormDetails.RiskFormHeaderInstructionsText = Convert.ToString(objDataset.Tables[0].Rows[0][ReqSqlConstants.COL_RISKFORMHEADERINSTRUCTIONSTEXT]);
                }
            }
            finally
            {
                LogHelper.LogInfo(Log, "SQLRequisitionDAO GetRiskFormHeaderInstructionsText Method completed.");
            }

            return objRiskFormDetails;

        }

        //public List<string> ValidateErrorBasedInterfaceRequisition(Requisition objRequisition, Dictionary<string, string> dctSettings)
        //{
        //    LogHelper.LogInfo(Log, "Requisition ValidateErrorBasedInterfaceRequisition Method Started");
        //    SqlConnection objSqlCon = null;
        //    RefCountingDataReader objRefCountingDataReader = null;
        //    var lstErrors = new List<string>();

        //    try
        //    {
        //        if (Log.IsDebugEnabled)
        //            Log.Debug(string.Concat("In ValidateErrorBasedInterfaceRequisition Method.",
        //                                    "SP: usp_P2P_REQ_ValidateErrorBasedInterfaceDocument, with parameters: requisitionId = " + objRequisition.DocumentCode));

        //        var sqlHelper = ContextSqlConn;
        //        objSqlCon = (SqlConnection)sqlHelper.CreateConnection();
        //        objSqlCon.Open();
        //        using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_VALIDATEERRORBASEDINTERFACEDOCUMENT))
        //        {
        //            objSqlCommand.CommandType = CommandType.StoredProcedure;
        //            objSqlCommand.Parameters.AddWithValue("@RequisitionNumber", objRequisition.DocumentNumber);
        //            objSqlCommand.Parameters.AddWithValue("@CurrencyCode", objRequisition.Currency ?? string.Empty);
        //            objSqlCommand.Parameters.AddWithValue("@BuyerUserId", objRequisition.ClientContactCode ?? string.Empty);
        //            objSqlCommand.Parameters.AddWithValue("@IncludeTaxInSplit", Convert.ToBoolean(dctSettings["IncludeTaxInSplit"]));
        //            objSqlCommand.Parameters.AddWithValue("@RequisitionStatus", objRequisition.DocumentStatusInfo);
        //            objSqlCommand.Parameters.AddWithValue("@ShippingCharge", objRequisition.Shipping == null ? 0 : objRequisition.Shipping);
        //            objSqlCommand.Parameters.AddWithValue("@EntityCode", objRequisition.DocumentAdditionalEntitiesInfoList != null && objRequisition.DocumentAdditionalEntitiesInfoList.Any() ? objRequisition.DocumentAdditionalEntitiesInfoList.FirstOrDefault().EntityCode : "");
        //            objSqlCommand.Parameters.AddWithValue("@SourceSystemId", !ReferenceEquals(objRequisition.SourceSystemInfo, null) ? objRequisition.SourceSystemInfo.SourceSystemId : 0);
        //            objSqlCommand.Parameters.AddWithValue("@FOBCode", objRequisition.FOBCode ?? string.Empty);
        //            objSqlCommand.Parameters.AddWithValue("@FOBLocationCode", objRequisition.FOBLocationCode ?? string.Empty);
        //            objSqlCommand.Parameters.AddWithValue("@CarriersCode", objRequisition.CarriersCode ?? string.Empty);
        //            objSqlCommand.Parameters.AddWithValue("@TransitTypeCode", objRequisition.TransitTypeCode ?? string.Empty);
        //            objSqlCommand.Parameters.AddWithValue("@FreightTermsCode", objRequisition.FreightTermsCode ?? string.Empty);
        //            objSqlCommand.Parameters.AddWithValue("@LOBValue", objRequisition.DocumentLOBDetails != null ? (objRequisition.DocumentLOBDetails.FirstOrDefault().EntityCode != null ? objRequisition.DocumentLOBDetails.FirstOrDefault().EntityCode : string.Empty) : string.Empty);
        //            objSqlCommand.Parameters.AddWithValue("@BilltoLocationId", objRequisition.BilltoLocation != null ? objRequisition.BilltoLocation.BilltoLocationId : 0);
        //            objSqlCommand.Parameters.AddWithValue("@ShiptoLocationId", objRequisition.ShiptoLocation != null ? objRequisition.ShiptoLocation.ShiptoLocationId : 0);
        //            objSqlCommand.Parameters.AddWithValue("@EntityMappedToBillToLocation", Convert.ToInt64(dctSettings.ContainsKey("EntityMappedToBillToLocation") == true ? dctSettings["EntityMappedToBillToLocation"] : Convert.ToString(0)));
        //            objSqlCommand.Parameters.AddWithValue("@EntityMappedToShippingMethods", Convert.ToInt64(dctSettings.ContainsKey("EntityMappedToShippingMethods") == true ? dctSettings["EntityMappedToShippingMethods"] : Convert.ToString(0)));
        //            objSqlCommand.Parameters.AddWithValue("@EntityMappedToShipToLocation", Convert.ToInt64(dctSettings.ContainsKey("EntityMappedToShipToLocation") == true ? dctSettings["EntityMappedToShipToLocation"] : Convert.ToString(0)));
        //            objSqlCommand.Parameters.AddWithValue("@Operation", objRequisition.Operation ?? string.Empty);
        //            objSqlCommand.Parameters.AddWithValue("@SourceSystemName", !ReferenceEquals(objRequisition.SourceSystemInfo, null) ? objRequisition.SourceSystemInfo.SourceSystemName : string.Empty);

        //            SqlParameter objSqlParameter = new SqlParameter("@RequisitionItems", SqlDbType.Structured)
        //            {
        //                TypeName = ReqSqlConstants.TVP_P2P_REQUISITIONITEM,
        //                Value = ConvertRequisitionItemsToTableType(objRequisition.RequisitionItems, objRequisition.CreatedOn)
        //            };
        //            objSqlCommand.Parameters.Add(objSqlParameter);

        //            objSqlParameter = new SqlParameter("@SplitItems", SqlDbType.Structured)
        //            {
        //                TypeName = ReqSqlConstants.TVP_P2P_INTERFACESPLITITEMS,
        //                Value = ConvertRequisitionSplitsToTableType(objRequisition.RequisitionItems)
        //            };
        //            objSqlCommand.Parameters.Add(objSqlParameter);

        //            objSqlParameter = new SqlParameter("@tvp_P2P_CustomAttributes", SqlDbType.Structured)
        //            {
        //                TypeName = ReqSqlConstants.TVP_P2P_CUSTOMATTRIBUTES,
        //                Value = dctSettings.ContainsKey("CustomFieldsEnabled") && Convert.ToBoolean(dctSettings["CustomFieldsEnabled"]) == true ?
        //                 ConvertToCustomAttributesDataTable(objRequisition) : P2P.DataAccessObjects.SQLServer.SQLCommonDAO.GetCustomAttributesDataTable()
        //            };
        //            objSqlCommand.Parameters.Add(objSqlParameter);
        //            objSqlParameter = new SqlParameter("@Tvp_P2P_RequisitionHeaderChargeItem", SqlDbType.Structured)
        //            {
        //                TypeName = ReqSqlConstants.TVP_P2P_REQUISITIONHEADERCHARGEITEM,
        //                Value = ConvertRequisitionHeaderChargeItemsToTableType(objRequisition)
        //            };
        //            objSqlCommand.Parameters.Add(objSqlParameter);

        //            objSqlParameter = new SqlParameter("@Tvp_P2P_RequisitionLineLevelChargeItem", SqlDbType.Structured)
        //            {
        //                TypeName = ReqSqlConstants.TVP_P2P_REQUISITIONLINELEVELCHARGEITEM,
        //                Value = ConvertRequisitionLineLevelChargeItemsToTableType(objRequisition.RequisitionItems)
        //            };
        //            objSqlCommand.Parameters.Add(objSqlParameter);

        //            objSqlCommand.Parameters.AddWithValue("@RequesterId", objRequisition.RequesterPASCode ?? string.Empty);
        //            objSqlCommand.Parameters.AddWithValue("@PurchaseTypeDescription", objRequisition.PurchaseTypeDescription ?? string.Empty);
        //            objSqlCommand.Parameters.AddWithValue("@AllowItemNoFreeText", Convert.ToInt64(dctSettings.ContainsKey("AllowBuyerItemNoFreeText") == true ? dctSettings["AllowBuyerItemNoFreeText"] : Convert.ToString(0)));
        //            objSqlCommand.Parameters.AddWithValue("@IsClientCodeBasedonLinkLocation", Convert.ToInt64(dctSettings.ContainsKey("IsClientCodeBasedonLinkLocation") == true ? dctSettings["IsClientCodeBasedonLinkLocation"] : Convert.ToString(0)));
        //            objSqlCommand.Parameters.AddWithValue("@ItemMasterEnabled", Convert.ToInt64(dctSettings.ContainsKey("ItemMasterEnabled") == true ? dctSettings["ItemMasterEnabled"] : Convert.ToString(0)));
        //            objSqlCommand.Parameters.AddWithValue("@DeriveHeaderEntities", dctSettings.ContainsKey("DeriveHeaderEntities") == true ? dctSettings["DeriveHeaderEntities"] : string.Empty);
        //            objSqlCommand.Parameters.AddWithValue("@IsDeriveAccountingBu", Convert.ToBoolean(dctSettings.ContainsKey("IsDeriveAccountingBu") == true ? dctSettings["IsDeriveAccountingBu"] : Convert.ToString(false)));
        //            objSqlCommand.Parameters.AddWithValue("@IsDeriveItemDetailEnable", Convert.ToBoolean(dctSettings.ContainsKey("DeriveItemDetails") == true ? dctSettings["DeriveItemDetails"] : Convert.ToString(false)));
        //            objSqlCommand.Parameters.AddWithValue("@UseDocumentLOB", Convert.ToBoolean(dctSettings.ContainsKey("UseDocumentLOB") == true ? dctSettings["UseDocumentLOB"] : Convert.ToString(false)));
        //            objSqlCommand.Parameters.AddWithValue("@DerivePartnerFromLocationCode", Convert.ToBoolean(dctSettings.ContainsKey("DerivePartnerFromLocationCode") == true ? dctSettings["DerivePartnerFromLocationCode"] : Convert.ToString(false)));
        //            objSqlCommand.Parameters.AddWithValue("@AllowReqForRfxAndOrder", Convert.ToBoolean(dctSettings.ContainsKey("AllowReqForRfxAndOrder") == true ? dctSettings["AllowReqForRfxAndOrder"] : Convert.ToString(false)));
        //            objSqlParameter = new SqlParameter("@RequisitionItemAdditionalFields", SqlDbType.Structured)
        //            {
        //                TypeName = ReqSqlConstants.TVP_P2P_BZ_REQUISITIONITEMADDITIONALFIELDS,
        //                Value = ConvertRequisitionAdditionalFieldAtrributeToTableType(objRequisition.RequisitionItems)
        //            };
        //            objSqlCommand.Parameters.Add(objSqlParameter);

        //            objRefCountingDataReader = (RefCountingDataReader)sqlHelper.ExecuteReader(objSqlCommand);
        //            if (objRefCountingDataReader != null)
        //            {
        //                var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;
        //                while (sqlDr.Read())
        //                    lstErrors.Add(GetStringValue(sqlDr, ReqSqlConstants.COL_ERROR_MESSAGE));

        //            }
        //            objRiskFormDetails.lstRiskFormQuestionScore = objRiskFormQuestionScore;
        //        }
        //    }
        //    finally
        //    {
        //        LogHelper.LogInfo(Log, "SQLRequisitionDAO GetRiskFormQuestionScore Method completed.");
        //    }

        //    return objRiskFormDetails;

        //}        

        //public List<string> ValidateErrorBasedInterfaceRequisition(Requisition objRequisition, Dictionary<string, string> dctSettings)
        //{
        //    LogHelper.LogInfo(Log, "Requisition ValidateErrorBasedInterfaceRequisition Method Started");
        //    SqlConnection objSqlCon = null;
        //    RefCountingDataReader objRefCountingDataReader = null;
        //    var lstErrors = new List<string>();

        //    try
        //    {
        //        if (Log.IsDebugEnabled)
        //            Log.Debug(string.Concat("In ValidateErrorBasedInterfaceRequisition Method.",
        //                                    "SP: usp_P2P_REQ_ValidateErrorBasedInterfaceDocument, with parameters: requisitionId = " + objRequisition.DocumentCode));

        //        var sqlHelper = ContextSqlConn;
        //        objSqlCon = (SqlConnection)sqlHelper.CreateConnection();
        //        objSqlCon.Open();
        //        using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_VALIDATEERRORBASEDINTERFACEDOCUMENT))
        //        {
        //            objSqlCommand.CommandType = CommandType.StoredProcedure;
        //            objSqlCommand.Parameters.AddWithValue("@RequisitionNumber", objRequisition.DocumentNumber);
        //            objSqlCommand.Parameters.AddWithValue("@CurrencyCode", objRequisition.Currency ?? string.Empty);
        //            objSqlCommand.Parameters.AddWithValue("@BuyerUserId", objRequisition.ClientContactCode ?? string.Empty);
        //            objSqlCommand.Parameters.AddWithValue("@IncludeTaxInSplit", Convert.ToBoolean(dctSettings["IncludeTaxInSplit"]));
        //            objSqlCommand.Parameters.AddWithValue("@RequisitionStatus", objRequisition.DocumentStatusInfo);
        //            objSqlCommand.Parameters.AddWithValue("@ShippingCharge", objRequisition.Shipping == null ? 0 : objRequisition.Shipping);
        //            objSqlCommand.Parameters.AddWithValue("@EntityCode", objRequisition.DocumentAdditionalEntitiesInfoList != null && objRequisition.DocumentAdditionalEntitiesInfoList.Any() ? objRequisition.DocumentAdditionalEntitiesInfoList.FirstOrDefault().EntityCode : "");
        //            objSqlCommand.Parameters.AddWithValue("@SourceSystemId", !ReferenceEquals(objRequisition.SourceSystemInfo, null) ? objRequisition.SourceSystemInfo.SourceSystemId : 0);
        //            objSqlCommand.Parameters.AddWithValue("@FOBCode", objRequisition.FOBCode ?? string.Empty);
        //            objSqlCommand.Parameters.AddWithValue("@FOBLocationCode", objRequisition.FOBLocationCode ?? string.Empty);
        //            objSqlCommand.Parameters.AddWithValue("@CarriersCode", objRequisition.CarriersCode ?? string.Empty);
        //            objSqlCommand.Parameters.AddWithValue("@TransitTypeCode", objRequisition.TransitTypeCode ?? string.Empty);
        //            objSqlCommand.Parameters.AddWithValue("@FreightTermsCode", objRequisition.FreightTermsCode ?? string.Empty);
        //            objSqlCommand.Parameters.AddWithValue("@LOBValue", objRequisition.DocumentLOBDetails != null ? (objRequisition.DocumentLOBDetails.FirstOrDefault().EntityCode != null ? objRequisition.DocumentLOBDetails.FirstOrDefault().EntityCode : string.Empty) : string.Empty);
        //            objSqlCommand.Parameters.AddWithValue("@BilltoLocationId", objRequisition.BilltoLocation != null ? objRequisition.BilltoLocation.BilltoLocationId : 0);
        //            objSqlCommand.Parameters.AddWithValue("@ShiptoLocationId", objRequisition.ShiptoLocation != null ? objRequisition.ShiptoLocation.ShiptoLocationId : 0);
        //            objSqlCommand.Parameters.AddWithValue("@EntityMappedToBillToLocation", Convert.ToInt64(dctSettings.ContainsKey("EntityMappedToBillToLocation") == true ? dctSettings["EntityMappedToBillToLocation"] : Convert.ToString(0)));
        //            objSqlCommand.Parameters.AddWithValue("@EntityMappedToShippingMethods", Convert.ToInt64(dctSettings.ContainsKey("EntityMappedToShippingMethods") == true ? dctSettings["EntityMappedToShippingMethods"] : Convert.ToString(0)));
        //            objSqlCommand.Parameters.AddWithValue("@EntityMappedToShipToLocation", Convert.ToInt64(dctSettings.ContainsKey("EntityMappedToShipToLocation") == true ? dctSettings["EntityMappedToShipToLocation"] : Convert.ToString(0)));
        //            objSqlCommand.Parameters.AddWithValue("@Operation", objRequisition.Operation ?? string.Empty);
        //            objSqlCommand.Parameters.AddWithValue("@SourceSystemName", !ReferenceEquals(objRequisition.SourceSystemInfo, null) ? objRequisition.SourceSystemInfo.SourceSystemName : string.Empty);

        //            SqlParameter objSqlParameter = new SqlParameter("@RequisitionItems", SqlDbType.Structured)
        //            {
        //                TypeName = ReqSqlConstants.TVP_P2P_REQUISITIONITEM,
        //                Value = ConvertRequisitionItemsToTableType(objRequisition.RequisitionItems, objRequisition.CreatedOn)
        //            };
        //            objSqlCommand.Parameters.Add(objSqlParameter);

        //            objSqlParameter = new SqlParameter("@SplitItems", SqlDbType.Structured)
        //            {
        //                TypeName = ReqSqlConstants.TVP_P2P_INTERFACESPLITITEMS,
        //                Value = ConvertRequisitionSplitsToTableType(objRequisition.RequisitionItems)
        //            };
        //            objSqlCommand.Parameters.Add(objSqlParameter);

        //            objSqlParameter = new SqlParameter("@tvp_P2P_CustomAttributes", SqlDbType.Structured)
        //            {
        //                TypeName = ReqSqlConstants.TVP_P2P_CUSTOMATTRIBUTES,
        //                Value = dctSettings.ContainsKey("CustomFieldsEnabled") && Convert.ToBoolean(dctSettings["CustomFieldsEnabled"]) == true ?
        //                 ConvertToCustomAttributesDataTable(objRequisition) : P2P.DataAccessObjects.SQLServer.SQLCommonDAO.GetCustomAttributesDataTable()
        //            };
        //            objSqlCommand.Parameters.Add(objSqlParameter);
        //            objSqlParameter = new SqlParameter("@Tvp_P2P_RequisitionHeaderChargeItem", SqlDbType.Structured)
        //            {
        //                TypeName = ReqSqlConstants.TVP_P2P_REQUISITIONHEADERCHARGEITEM,
        //                Value = ConvertRequisitionHeaderChargeItemsToTableType(objRequisition)
        //            };
        //            objSqlCommand.Parameters.Add(objSqlParameter);

        //            objSqlParameter = new SqlParameter("@Tvp_P2P_RequisitionLineLevelChargeItem", SqlDbType.Structured)
        //            {
        //                TypeName = ReqSqlConstants.TVP_P2P_REQUISITIONLINELEVELCHARGEITEM,
        //                Value = ConvertRequisitionLineLevelChargeItemsToTableType(objRequisition.RequisitionItems)
        //            };
        //            objSqlCommand.Parameters.Add(objSqlParameter);

        //            objSqlCommand.Parameters.AddWithValue("@RequesterId", objRequisition.RequesterPASCode ?? string.Empty);
        //            objSqlCommand.Parameters.AddWithValue("@PurchaseTypeDescription", objRequisition.PurchaseTypeDescription ?? string.Empty);
        //            objSqlCommand.Parameters.AddWithValue("@AllowItemNoFreeText", Convert.ToInt64(dctSettings.ContainsKey("AllowBuyerItemNoFreeText") == true ? dctSettings["AllowBuyerItemNoFreeText"] : Convert.ToString(0)));
        //            objSqlCommand.Parameters.AddWithValue("@IsClientCodeBasedonLinkLocation", Convert.ToInt64(dctSettings.ContainsKey("IsClientCodeBasedonLinkLocation") == true ? dctSettings["IsClientCodeBasedonLinkLocation"] : Convert.ToString(0)));
        //            objSqlCommand.Parameters.AddWithValue("@ItemMasterEnabled", Convert.ToInt64(dctSettings.ContainsKey("ItemMasterEnabled") == true ? dctSettings["ItemMasterEnabled"] : Convert.ToString(0)));
        //            objSqlCommand.Parameters.AddWithValue("@DeriveHeaderEntities", dctSettings.ContainsKey("DeriveHeaderEntities") == true ? dctSettings["DeriveHeaderEntities"] : string.Empty);
        //            objSqlCommand.Parameters.AddWithValue("@IsDeriveAccountingBu", Convert.ToBoolean(dctSettings.ContainsKey("IsDeriveAccountingBu") == true ? dctSettings["IsDeriveAccountingBu"] : Convert.ToString(false)));
        //            objSqlCommand.Parameters.AddWithValue("@IsDeriveItemDetailEnable", Convert.ToBoolean(dctSettings.ContainsKey("DeriveItemDetails") == true ? dctSettings["DeriveItemDetails"] : Convert.ToString(false)));
        //            objSqlCommand.Parameters.AddWithValue("@UseDocumentLOB", Convert.ToBoolean(dctSettings.ContainsKey("UseDocumentLOB") == true ? dctSettings["UseDocumentLOB"] : Convert.ToString(false)));
        //            objSqlCommand.Parameters.AddWithValue("@DerivePartnerFromLocationCode", Convert.ToBoolean(dctSettings.ContainsKey("DerivePartnerFromLocationCode") == true ? dctSettings["DerivePartnerFromLocationCode"] : Convert.ToString(false)));
        //            objSqlCommand.Parameters.AddWithValue("@AllowReqForRfxAndOrder", Convert.ToBoolean(dctSettings.ContainsKey("AllowReqForRfxAndOrder") == true ? dctSettings["AllowReqForRfxAndOrder"] : Convert.ToString(false)));

        //            objRefCountingDataReader = (RefCountingDataReader)sqlHelper.ExecuteReader(objSqlCommand);
        //            if (objRefCountingDataReader != null)
        //            {
        //                var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;
        //                while (sqlDr.Read())
        //                    lstErrors.Add(GetStringValue(sqlDr, ReqSqlConstants.COL_ERROR_MESSAGE));
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.LogError(Log, "Error occured in ValidateErrorBasedInterfaceRequisition method.", ex);

        //        var objCustomFault = new CustomFault(ex.Message, "ValidateErrorBasedInterfaceRequisition", "ValidateErrorBasedInterfaceRequisition",
        //                                             "Requsition", ExceptionType.ApplicationException,
        //                                             string.Empty, false);
        //        throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
        //                                              "Error while Updating Requisition approver status :" +
        //                                              string.Empty);
        //    }
        //    finally
        //    {
        //        if (!ReferenceEquals(objRefCountingDataReader, null) && !objRefCountingDataReader.IsClosed)
        //        {
        //            objRefCountingDataReader.Close();
        //            objRefCountingDataReader.Dispose();
        //        }
        //        if (!ReferenceEquals(objSqlCon, null) && objSqlCon.State != ConnectionState.Closed)
        //        {
        //            objSqlCon.Close();
        //            objSqlCon.Dispose();
        //        }
        //        LogHelper.LogInfo(Log, "Reqisition ValidateInterfaceReqisition Method Ended");
        //    }
        //    return lstErrors;
        //}
        //REQ-4822: MobileApp: Gets all split details of Req Item
        public List<RequisitionSplitItems> GetAllRequisitionAccountingDetails(long requisitionId, List<long> requisitionItemIds)
        {
            List<RequisitionSplitItems> requisitionSplitItems = new List<RequisitionSplitItems>();
            SqlConnection objSqlConnection = null;

            try
            {
                LogHelper.LogInfo(Log, "Requisition GetAllRequisitionAccountingDetails Method Started");
                DataSet splitItemsDataSet = null;

                objSqlConnection = new SqlConnection(ContextSqlConn.ConnectionString);
                objSqlConnection.Open();

                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_GETALLREQUISITIONACCOUNTINGDETAILS))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.AddWithValue("@id", requisitionId);
                    SqlParameter objSqlParameter = new SqlParameter("@tvp_P2P_REQ_ReqItemIds", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_LONG,
                        Value = ConvertIdsToTable(requisitionItemIds)
                    };
                    objSqlCommand.Parameters.Add(objSqlParameter);
                    splitItemsDataSet = ContextSqlConn.ExecuteDataSet(objSqlCommand);
                }

                if (splitItemsDataSet != null && splitItemsDataSet.Tables.Count > 0)
                {
                    if (splitItemsDataSet.Tables[0].Rows.Count > 0)
                    {
                        foreach (DataRow dr in splitItemsDataSet.Tables[0].Rows)
                        {
                            var objReqSplitItems = new RequisitionSplitItems
                            {
                                DocumentItemId = (long)dr[ReqSqlConstants.COL_REQUISITION_ITEM_ID],
                                DocumentSplitItemId = (long)dr[ReqSqlConstants.COL_REQUISITION_SPLIT_ITEM_ID],
                                Percentage = Convert.ToDecimal(dr[ReqSqlConstants.COL_PERCENTAGE], CultureInfo.InvariantCulture),
                                Quantity = Convert.ToDecimal(dr[ReqSqlConstants.COL_QUANTITY], CultureInfo.InvariantCulture),
                                SplitItemTotal = Convert.ToDecimal(dr[ReqSqlConstants.COL_SPLIT_ITEM_TOTAL], CultureInfo.InvariantCulture),
                                ErrorCode = Convert.ToString(dr[ReqSqlConstants.COL_ERROR_CODE], CultureInfo.InvariantCulture),
                                SplitType = (SplitType)Convert.ToInt32(dr[ReqSqlConstants.COL_SPLIT_TYPE], CultureInfo.InvariantCulture)
                            };

                            var drItemSplits = splitItemsDataSet.Tables[1].Select("RequisitionSplitItemId = " + objReqSplitItems.DocumentSplitItemId.ToString(CultureInfo.InvariantCulture));

                            objReqSplitItems.DocumentSplitItemEntities = new List<DocumentSplitItemEntity>();
                            foreach (var drSplit in drItemSplits)
                            {
                                var reqSplitItemEntity = new DocumentSplitItemEntity();
                                reqSplitItemEntity.SplitAccountingFieldId = (int)drSplit[ReqSqlConstants.COL_FIELDCONFIGID];
                                reqSplitItemEntity.SplitAccountingFieldValue = drSplit[ReqSqlConstants.COL_SPLIT_ACCOUNTING_FIELD_VALUE].ToString();
                                reqSplitItemEntity.EntityTypeId = (int)drSplit[ReqSqlConstants.COL_ENTITY_TYPE_ID];
                                reqSplitItemEntity.EntityCode = drSplit[ReqSqlConstants.COL_ENTITY_CODE].ToString();
                                reqSplitItemEntity.Title = Convert.ToString(drSplit[ReqSqlConstants.COL_TITLE]);
                                reqSplitItemEntity.FieldName = Convert.ToString(drSplit[ReqSqlConstants.COL_FIELD_NAME]);
                                reqSplitItemEntity.DocumentSplitItemId = Convert.ToInt64(drSplit[ReqSqlConstants.COL_REQUISITION_SPLIT_ITEM_ID]);
                                reqSplitItemEntity.DocumentSplitItemEntityId = Convert.ToInt64(drSplit[ReqSqlConstants.COL_REQSPLITITEMENTITYID]);

                                if (reqSplitItemEntity.Title.ToUpper().Equals("REQUESTER"))
                                {
                                    reqSplitItemEntity.EntityDisplayName = drSplit[ReqSqlConstants.COL_REQUESTER_NAME].ToString();
                                }
                                else
                                {
                                    reqSplitItemEntity.EntityDisplayName = drSplit[ReqSqlConstants.COL_ENTITY_DISPLAY_NAME].ToString();
                                }
                                objReqSplitItems.DocumentSplitItemEntities.Add(reqSplitItemEntity);
                            }
                            requisitionSplitItems.Add(objReqSplitItems);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in SQLRequisitionDAO GetAllRequisitionAccountingDetails Method for requisitionId = " + requisitionId, ex);
                throw ex;
            }
            finally
            {
                if (!ReferenceEquals(objSqlConnection, null) && objSqlConnection.State != ConnectionState.Closed)
                {
                    objSqlConnection.Close();
                    objSqlConnection.Dispose();
                }
                LogHelper.LogInfo(Log, "Requisition GetAllRequisitionAccountingDetails method completed.");
            }
            return requisitionSplitItems;
        }

        //public DataTable ValidateReqItemsForExceptionHandling(DataTable dtItemDetails)
        //{
        //    LogHelper.LogInfo(Log, "Requisition ValidateReqItemsForExceptionHandling Method Started");
        //    SqlConnection _sqlCon = null;
        //    SqlTransaction _sqlTrans = null;
        //    DataTable dt = new DataTable();
        //    try
        //    {
        //        var sqlHelper = ContextSqlConn;
        //        _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
        //        _sqlCon.Open();
        //        _sqlTrans = _sqlCon.BeginTransaction();

        //        if (Log.IsDebugEnabled)
        //            Log.Debug(string.Concat("Requisition ProprateLineItemTaxandShipping sp usp_P2P_REQ_ProrateLineItemTaxandShipping with parameter: P2pItems=" + dtItemDetails, " was called."));

        //        using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_VALIDATEREQITEMSFOREXCEPTIONHANDLING))
        //        {
        //            objSqlCommand.CommandType = CommandType.StoredProcedure;
        //            SqlParameter objSqlParameter = new SqlParameter("@tblReqItemDetails", SqlDbType.Structured)
        //            {
        //                TypeName = ReqSqlConstants.TVP_P2P_REQUISITIONITEMDETAILS,
        //                Value = dtItemDetails
        //            };
        //            objSqlCommand.Parameters.Add(objSqlParameter);
        //            dt = sqlHelper.ExecuteDataSet(objSqlCommand, _sqlTrans).Tables[0];
        //        }

        //        if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
        //            _sqlTrans.Commit();
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

        //        LogHelper.LogInfo(Log, "Requisition ValidateReqItemsForExceptionHandling Method Ended");
        //    }
        //    return dt;
        //}

        public Requisition GetRequisitionPartialDetailsById(long requisitionId)
        {
            Requisition requisition = new Requisition();
            SqlConnection objSqlConnection = null;

            try
            {
                LogHelper.LogInfo(Log, "Requisition GetRequisitionPartialDetailsById Method Started");
                DataSet reqDataSet = null;

                objSqlConnection = new SqlConnection(ContextSqlConn.ConnectionString);
                objSqlConnection.Open();

                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_GETREQUISITIONPARTIALDETAILSBYID))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.AddWithValue("@id", requisitionId);
                    reqDataSet = ContextSqlConn.ExecuteDataSet(objSqlCommand);
                }

                if (reqDataSet != null && reqDataSet.Tables.Count > 0)
                {
                    if (reqDataSet.Tables[0].Rows.Count > 0)
                    {
                        foreach (DataRow dr in reqDataSet.Tables[0].Rows)
                        {
                            requisition.DocumentCode = (long)dr[ReqSqlConstants.COL_DOCUMENTCODE];
                            requisition.DocumentNumber = (string)dr[ReqSqlConstants.COL_DOCUMENT_NUMBER];
                            requisition.DocumentStatusInfo = (DocumentStatus)Convert.ToInt32(dr[ReqSqlConstants.COL_DOCUMENT_STATUS]);

                        }
                    }
                    requisition.DocumentLOBDetails = new List<DocumentLOBDetails>();

                    if (reqDataSet.Tables[1].Rows != null && reqDataSet.Tables[1].Rows.Count > 0)
                    {
                        //var drLobDetails = reqDataSet.Tables[1].Select("DocumentCode = " + requisition.DocumentCode.ToString(CultureInfo.InvariantCulture));
                        foreach (DataRow drLOB in reqDataSet.Tables[1].Rows)
                        {
                            DocumentLOBDetails objLOB = new DocumentLOBDetails
                            {
                                EntityDetailCode = Convert.ToInt64(drLOB[ReqSqlConstants.COL_ENTITYDETAILCODE]),
                                EntityId = Convert.ToInt16(drLOB[ReqSqlConstants.COL_EntityId]),
                                EntityCode = Convert.ToString(drLOB[ReqSqlConstants.COL_ENTITY_CODE]),
                                EntityDisplayName = Convert.ToString(drLOB[ReqSqlConstants.COL_ENTITYNAME])
                            };
                            requisition.DocumentLOBDetails.Add(objLOB);
                        }
                    }

                    requisition.RequisitionItems = new List<RequisitionItem>();
                    if (reqDataSet.Tables[2].Rows != null && reqDataSet.Tables[2].Rows.Count > 0)
                    {
                        //var drItem = reqDataSet.Tables[2].Select("DocumentCode = " + requisition.DocumentCode.ToString(CultureInfo.InvariantCulture));
                        foreach (DataRow drSplit in reqDataSet.Tables[2].Rows)
                        {
                            var reqItem = new RequisitionItem();
                            reqItem.ItemLineNumber = (long)drSplit[ReqSqlConstants.COL_LINENUMBER];
                            reqItem.ItemStatus = (DocumentStatus)Convert.ToInt16(drSplit[ReqSqlConstants.COL_ITEM_STATUS]);
                            requisition.RequisitionItems.Add(reqItem);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in SQLRequisitionDAO GetRequisitionPartialDetailsById Method for requisitionId = " + requisitionId, ex);
                throw ex;
            }
            finally
            {
                if (!ReferenceEquals(objSqlConnection, null) && objSqlConnection.State != ConnectionState.Closed)
                {
                    objSqlConnection.Close();
                    objSqlConnection.Dispose();
                }
                LogHelper.LogInfo(Log, "Requisition GetRequisitionPartialDetailsById method completed.");
            }
            return requisition;
        }

        public long GetOrderLocationIdByClientLocationCode(string ClientLocationCode, long PartnerCode, string headerEntities, bool IsDefaultOrderingLocation)
        {
            long OrderLocationId = 0;
            SqlConnection objSqlConnection = null;

            try
            {
                LogHelper.LogInfo(Log, "Requisition GetOrderLocationIdByClientLocationCode Method Started");
                DataSet reqDataSet = null;

                objSqlConnection = new SqlConnection(ContextSqlConn.ConnectionString);
                objSqlConnection.Open();

                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_INT_GETORDERLOCATIONID))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.AddWithValue("@ClientLocationCode", ClientLocationCode);
                    objSqlCommand.Parameters.AddWithValue("@PartnerCode", PartnerCode);
                    objSqlCommand.Parameters.AddWithValue("@csvHeaderEntities", headerEntities);
                    objSqlCommand.Parameters.AddWithValue("@IsDefaultOrderingLocation", IsDefaultOrderingLocation);

                    reqDataSet = ContextSqlConn.ExecuteDataSet(objSqlCommand);
                }

                if (reqDataSet != null && reqDataSet.Tables.Count > 0)
                {
                    if (reqDataSet.Tables[0].Rows.Count > 0)
                    {
                        foreach (DataRow dr in reqDataSet.Tables[0].Rows)
                        {
                            OrderLocationId = (long)dr["OrderLocationId"];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in SQLRequisitionDAO GetOrderLocationIdByClientLocationCode Method for requisitionId = " + ClientLocationCode, ex);
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw();
                throw ex;
            }
            finally
            {
                if (!ReferenceEquals(objSqlConnection, null) && objSqlConnection.State != ConnectionState.Closed)
                {
                    objSqlConnection.Close();
                    objSqlConnection.Dispose();
                }
                LogHelper.LogInfo(Log, "Requisition GetOrderLocationIdByClientLocationCode method completed.");
            }
            return OrderLocationId;
        }
        public BusinessEntities.Requisition GetRequisitionDetailsForCapitalBudget(long requisitionId)
        {
            RefCountingDataReader objRefCountingDataReader = null;

            try
            {
                LogHelper.LogInfo(Log, "GetRequisitionDetailsForCapitalBudget Method Started for id=" + requisitionId);

                var sqlHelper = ContextSqlConn;
                DataSet objDs = null;

                //Read CatalogItemSources setting for return all item sources considered as "Catalog"

                if (requisitionId > 0)
                {
                    if (Log.IsDebugEnabled)
                        Log.Debug(string.Format("In GetRequisitionDetailsForCapitalBudget Method, sp: usp_P2P_REQ_GetRequisitionDetailsForCapitalBudget with parameter: RequisitionId= {0}", requisitionId));


                    using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_GETREQUISITIONDETAILSFORCAPITALBUDGET))
                    {
                        objSqlCommand.CommandType = CommandType.StoredProcedure;
                        objSqlCommand.CommandTimeout = 150;
                        objSqlCommand.Parameters.Add(new SqlParameter("@requisitonId", requisitionId));
                        objDs = sqlHelper.ExecuteDataSet(objSqlCommand);


                    }

                    if (objDs != null && objDs.Tables.Count > 0)
                    {


                        foreach (DataRow dr in objDs.Tables[0].Rows)
                        {
                            var objRequisition = new BusinessEntities.Requisition
                            {
                                DocumentCode = requisitionId,
                                DocumentType = P2PDocumentType.Requisition,
                                DocumentId = (long)dr[ReqSqlConstants.COL_REQUISITION_ID],
                                DocumentNumber = Convert.ToString(dr[ReqSqlConstants.COL_DOCUMENTNUMBER], CultureInfo.InvariantCulture), 
                                CreatedOn = Convert.IsDBNull(dr[ReqSqlConstants.COL_DOCUMENTDATE]) ? DateTime.MinValue : Convert.ToDateTime(dr[ReqSqlConstants.COL_DOCUMENTDATE], CultureInfo.InvariantCulture),
                                ParentDocumentCode = (long)dr[ReqSqlConstants.COL_PARENTDOCUMENTCODE],
                                RequesterId = (long)dr[ReqSqlConstants.COL_REQUESTER_ID],
                                Currency = Convert.ToString(dr[ReqSqlConstants.COL_CURRENCY], CultureInfo.InvariantCulture),
                                TotalAmount = (!string.IsNullOrWhiteSpace(dr[ReqSqlConstants.COL_REQUISITION_AMOUNT].ToString())) ? (Convert.ToDecimal(dr[ReqSqlConstants.COL_REQUISITION_AMOUNT])) : default(decimal?),
                                ItemTotalAmount = (!string.IsNullOrWhiteSpace(dr[ReqSqlConstants.COL_ITEM_TOTAL].ToString())) ? (Convert.ToDecimal(dr[ReqSqlConstants.COL_ITEM_TOTAL])) : default(decimal?),
                                RequisitionSource = (RequisitionSource)Convert.ToInt16(dr[ReqSqlConstants.COL_REQUISITION_SOURCE]),
                                OnBehalfOf = (long)dr[ReqSqlConstants.COL_ONBEHALFOF] == (long)dr[ReqSqlConstants.COL_REQUESTER_ID] ? 0 : (long)dr[ReqSqlConstants.COL_ONBEHALFOF],
                                ShiptoLocation = new ShiptoLocation()
                                {
                                    ShiptoLocationId = Convert.ToInt32(dr[ReqSqlConstants.COL_SHIPTOLOC_ID], CultureInfo.InvariantCulture)
                                },
                                BilltoLocation = new BilltoLocation
                                {
                                    BilltoLocationId = Convert.ToInt32(dr[ReqSqlConstants.COL_BILLTOLOC_ID], CultureInfo.InvariantCulture)
                                },
                                DelivertoLocation = new DelivertoLocation
                                {
                                    DelivertoLocationId = Convert.ToInt32(dr[ReqSqlConstants.COL_DELIVERTOLOCATION_ID], CultureInfo.InvariantCulture)
                                },
                                PurchaseType = Convert.ToInt32(dr[ReqSqlConstants.COL_PURCHASETYPE], CultureInfo.InvariantCulture),
                                TotalAmountChange = Convert.ToDecimal(dr[ReqSqlConstants.COL_REQUISITIONTOTALCHANGE], CultureInfo.InvariantCulture),
                                RequisitionAmount = Convert.ToDecimal(dr[ReqSqlConstants.COL_REQUISITION_AMOUNT], CultureInfo.InvariantCulture),
                                DocumentStatusInfo = (DocumentStatus)Convert.ToInt32(dr[ReqSqlConstants.COL_DOCUMENT_STATUS]),
                            };


                            //HEADER LEVEL ENTITY DETAILS
                            objRequisition.DocumentAdditionalEntitiesInfoList = new Collection<DocumentAdditionalEntityInfo>();
                            foreach (DataRow entityRow in objDs.Tables[1].Rows)
                            {
                                objRequisition.DocumentAdditionalEntitiesInfoList.Add(new BusinessEntities.DocumentAdditionalEntityInfo
                                {
                                    EntityDetailCode = (long)entityRow[ReqSqlConstants.COL_SPLIT_ITEM_ENTITYDETAILCODE],
                                    EntityId = Convert.ToInt32(entityRow[ReqSqlConstants.COL_ENTITY_ID], CultureInfo.InvariantCulture),
                                    EntityDisplayName = Convert.ToString(entityRow[ReqSqlConstants.COL_ENTITY_DISPLAY_NAME], CultureInfo.InvariantCulture),
                                    EntityCode = Convert.ToString(entityRow[ReqSqlConstants.COL_ENTITY_CODE], CultureInfo.InvariantCulture)
                                });
                            }



                            //REQUISITION LINE ITEM DETAILS
                            objRequisition.RequisitionItems = new List<BusinessEntities.RequisitionItem>();
                            foreach (DataRow lstItems in objDs.Tables[2].Rows)
                            {
                                var objRequisitionItem = new BusinessEntities.RequisitionItem
                                {
                                    DocumentItemId = (long)lstItems[ReqSqlConstants.COL_REQUISITION_ITEM_ID],
                                    P2PLineItemId = (long)lstItems[ReqSqlConstants.COL_P2P_LINE_ITEM_ID],
                                    DocumentId = (long)lstItems[ReqSqlConstants.COL_REQUISITION_ID],
                                    ShortName = Convert.ToString(lstItems[ReqSqlConstants.COL_SHORT_NAME], CultureInfo.InvariantCulture),
                                    Description = Convert.ToString(lstItems[ReqSqlConstants.COL_DESCRIPTION], CultureInfo.InvariantCulture),
                                    UnitPrice = (!string.IsNullOrWhiteSpace(lstItems[ReqSqlConstants.COL_UNIT_PRICE].ToString())) ? (Convert.ToDecimal(lstItems[ReqSqlConstants.COL_UNIT_PRICE])) : default(decimal?),
                                    Quantity = Convert.ToDecimal(lstItems[ReqSqlConstants.COL_QUANTITY], CultureInfo.InvariantCulture),
                                    UOM = Convert.ToString(lstItems[ReqSqlConstants.COL_UOM], CultureInfo.InvariantCulture),
                                    DateRequested = Convert.IsDBNull(lstItems[ReqSqlConstants.COL_DATE_REQUESTED]) ? DateTime.MinValue : Convert.ToDateTime(lstItems[ReqSqlConstants.COL_DATE_REQUESTED], CultureInfo.InvariantCulture),
                                    DateNeeded = Convert.IsDBNull(lstItems[ReqSqlConstants.COL_DATE_NEEDED]) ? DateTime.MinValue : Convert.ToDateTime(lstItems[ReqSqlConstants.COL_DATE_NEEDED], CultureInfo.InvariantCulture),
                                    PartnerCode = Convert.ToDecimal(lstItems[ReqSqlConstants.COL_PARTNER_CODE], CultureInfo.InvariantCulture),
                                    CategoryId = (long)lstItems[ReqSqlConstants.COL_CATEGORY_ID],
                                    ItemType = (ItemType)Convert.ToInt16(lstItems[ReqSqlConstants.COL_ITEM_TYPE_ID], CultureInfo.InvariantCulture),
                                    Tax = (!string.IsNullOrWhiteSpace(lstItems[ReqSqlConstants.COL_LINE_ITEM_TAX].ToString())) ? (Convert.ToDecimal(lstItems[ReqSqlConstants.COL_LINE_ITEM_TAX])) : default(decimal?),
                                    ShippingCharges = (!string.IsNullOrWhiteSpace(lstItems[ReqSqlConstants.COL_SHIPPING_CHARGES].ToString())) ? (Convert.ToDecimal(lstItems[ReqSqlConstants.COL_SHIPPING_CHARGES])) : default(decimal?),
                                    AdditionalCharges = (!string.IsNullOrWhiteSpace(lstItems[ReqSqlConstants.COL_ADDITIONAL_CHARGES].ToString())) ? (Convert.ToDecimal(lstItems[ReqSqlConstants.COL_ADDITIONAL_CHARGES])) : default(decimal?),
                                    StartDate = Convert.IsDBNull(lstItems[ReqSqlConstants.COL_START_DATE]) ? DateTime.MinValue : Convert.ToDateTime(lstItems[ReqSqlConstants.COL_START_DATE], CultureInfo.InvariantCulture),
                                    EndDate = Convert.IsDBNull(lstItems[ReqSqlConstants.COL_END_DATE]) ? DateTime.MinValue : Convert.ToDateTime(lstItems[ReqSqlConstants.COL_END_DATE], CultureInfo.InvariantCulture),
                                    Currency = Convert.ToString(lstItems[ReqSqlConstants.COL_CURRENCY], CultureInfo.InvariantCulture),
                                    ItemExtendedType = (ItemExtendedType)Convert.ToInt16(lstItems[ReqSqlConstants.COL_ITEM_EXTENDED_TYPE], CultureInfo.InvariantCulture),
                                    Efforts = (!string.IsNullOrWhiteSpace(lstItems[ReqSqlConstants.COL_EFFORTS].ToString())) ? (Convert.ToDecimal(lstItems[ReqSqlConstants.COL_EFFORTS])) : default(decimal?),
                                    ItemNumber = Convert.ToString(lstItems[ReqSqlConstants.COL_ITEMNUMBER], CultureInfo.InvariantCulture),
                                    ItemLineNumber = Convert.ToInt32(lstItems[ReqSqlConstants.COL_ITEMLINENUMBER], CultureInfo.InvariantCulture),
                                    ShipToLocationId = Convert.ToInt32(lstItems[ReqSqlConstants.COL_SHIPTOLOCATION_ID], CultureInfo.InvariantCulture)

                                };
                                if (objRequisitionItem.ItemType == ItemType.Material)
                                    objRequisitionItem.ItemTotalAmount = objRequisitionItem.UnitPrice * objRequisitionItem.Quantity + objRequisitionItem.Tax + objRequisitionItem.AdditionalCharges + objRequisitionItem.ShippingCharges;
                                else
                                    objRequisitionItem.ItemTotalAmount = objRequisitionItem.UnitPrice * objRequisitionItem.Efforts + objRequisitionItem.Tax + objRequisitionItem.AdditionalCharges;





                                //Accounting Details
                                var drSplitsItems = objDs.Tables[3].Select("RequisitionItemId =" +
                                                        objRequisitionItem.DocumentItemId.ToString(CultureInfo.InvariantCulture));
                                objRequisitionItem.ItemSplitsDetail = new List<RequisitionSplitItems>();

                                //Verify if the current item it's considered as "Catalog"

                                foreach (var split in drSplitsItems)
                                {
                                    var objRequisitionSplitItems = new RequisitionSplitItems
                                    {
                                        DocumentItemId = (long)split[ReqSqlConstants.COL_REQUISITION_ITEM_ID],
                                        DocumentSplitItemId = (long)split[ReqSqlConstants.COL_REQUISITION_SPLIT_ITEM_ID],
                                        Percentage = Convert.ToDecimal(split[ReqSqlConstants.COL_PERCENTAGE], CultureInfo.InvariantCulture),
                                        Quantity = Convert.ToDecimal(split[ReqSqlConstants.COL_QUANTITY], CultureInfo.InvariantCulture),
                                        SplitItemTotal = split[ReqSqlConstants.COL_SPLIT_ITEM_TOTAL] != DBNull.Value ? Convert.ToDecimal(split[ReqSqlConstants.COL_SPLIT_ITEM_TOTAL], CultureInfo.InvariantCulture) : (decimal?)null,
                                        SplitType = (SplitType)Convert.ToInt32(split[ReqSqlConstants.COL_SPLIT_TYPE], CultureInfo.InvariantCulture)
                                    };
                                    var drSplits = objDs.Tables[4].Select("RequisitionSplitItemId =" + objRequisitionSplitItems.DocumentSplitItemId.ToString(CultureInfo.InvariantCulture));
                                    objRequisitionSplitItems.DocumentSplitItemEntities = new List<DocumentSplitItemEntity>();
                                    foreach (var drSplit in drSplits)
                                    {
                                        var documentSplitItemEntity = new DocumentSplitItemEntity
                                        {
                                            SplitAccountingFieldId = (int)drSplit[ReqSqlConstants.COL_SPLIT_ACCOUNTING_FIELD_CONFIG_ID],
                                            SplitAccountingFieldValue = drSplit[ReqSqlConstants.COL_SPLIT_ACCOUNTING_FIELD_VALUE].ToString(),
                                            EntityTypeId = (int)drSplit[ReqSqlConstants.COL_ENTITY_TYPE_ID],
                                            EntityCode = drSplit[ReqSqlConstants.COL_ENTITY_CODE].ToString(),
                                            EntityCodeId = drSplit[ReqSqlConstants.COL_ENTITY_TYPE_ID].ToString() + "_" + drSplit[ReqSqlConstants.COL_ENTITY_CODE].ToString(),
                                            FieldName = drSplit[ReqSqlConstants.COL_FIELD_NAME].ToString()
                                        };
                                        objRequisitionSplitItems.DocumentSplitItemEntities.Add(documentSplitItemEntity);

                                    }
                                    objRequisitionItem.ItemSplitsDetail.Add(objRequisitionSplitItems);
                                }
                                objRequisition.RequisitionItems.Add(objRequisitionItem);

                            }

                            return objRequisition;
                        }
                    }
                }
            }
            finally
            {
                if (!ReferenceEquals(objRefCountingDataReader, null) && !objRefCountingDataReader.IsClosed)
                {
                    objRefCountingDataReader.Close();
                    objRefCountingDataReader.Dispose();
                }

                LogHelper.LogInfo(Log, "GetRequisitionDetailsForCapitalBudget Method Ended for id=" + requisitionId);
            }
            return new BusinessEntities.Requisition();
        }

        //private DataTable ConvertRequisitionAdditionalFieldAtrributeToTableType(List<RequisitionItem> objRequisitionItem)
        //{
        //    DataTable dtAdditionalFieldAttributes = new DataTable() { Locale = CultureInfo.InvariantCulture, TableName = ReqSqlConstants.TVP_P2P_BZ_REQUISITIONITEMADDITIONALFIELDS };
        //    dtAdditionalFieldAttributes.Columns.Add("RequisitionID", typeof(int));
        //    dtAdditionalFieldAttributes.Columns.Add("RequisitionItemID", typeof(int));
        //    dtAdditionalFieldAttributes.Columns.Add("AdditionalFieldID", typeof(int));
        //    dtAdditionalFieldAttributes.Columns.Add("AdditionalFieldValue", typeof(string));
        //    dtAdditionalFieldAttributes.Columns.Add("AdditionalFieldCode", typeof(string));
        //    dtAdditionalFieldAttributes.Columns.Add("CreatedBy", typeof(long));
        //    dtAdditionalFieldAttributes.Columns.Add("FeatureId", typeof(int));
        //    dtAdditionalFieldAttributes.Columns.Add("AdditionalFieldDetailCode", typeof(string));
        //    dtAdditionalFieldAttributes.Columns.Add("ItemLineNumber", typeof(int));

        //    if (objRequisitionItem != null && objRequisitionItem.Count() > 0)
        //    {
        //        foreach (var RequisitionAdditionalFieldAttribute in objRequisitionItem)
        //        {
        //            if (RequisitionAdditionalFieldAttribute.lstAdditionalFieldAttributues != null && RequisitionAdditionalFieldAttribute.lstAdditionalFieldAttributues.Any())
        //            {
        //                foreach (var additionalFieldAtrribute in RequisitionAdditionalFieldAttribute.lstAdditionalFieldAttributues)
        //                {
        //                    DataRow dr = dtAdditionalFieldAttributes.NewRow();
        //                    dr["RequisitionID"] = 0;
        //                    dr["RequisitionItemID"] = 0;
        //                    dr["AdditionalFieldID"] = 0;
        //                    dr["AdditionalFieldValue"] = Convert.ToString(additionalFieldAtrribute.AdditionalFieldName);
        //                    dr["AdditionalFieldCode"] = Convert.ToString(additionalFieldAtrribute.AdditionalFieldCode);
        //                    dr["CreatedBy"] = 0;
        //                    dr["FeatureId"] = 0;
        //                    dr["AdditionalFieldDetailCode"] = 0;
        //                    dr["ItemLineNumber"] = RequisitionAdditionalFieldAttribute.ItemLineNumber;

        //                    dtAdditionalFieldAttributes.Rows.Add(dr);
        //                }
        //            }
        //        }
        //    }
        //    return dtAdditionalFieldAttributes;
        //}


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

        //public void SaveAdditionalFieldAttributes(long documentID, long documentItemID, List<P2PAdditionalFieldAtrribute> lstAdditionalFieldAttributues, string PurchaseTypeDescription)
        //{
        //    Requisition requisition = new Requisition();
        //    SqlConnection objSqlConnection = null;
        //    bool flag = false;

        //    try
        //    {
        //        LogHelper.LogInfo(Log, "Requisition SaveAdditionalFieldAttributes Method Started");
        //        DataSet reqDataSet = null;

        //        objSqlConnection = new SqlConnection(ContextSqlConn.ConnectionString);
        //        objSqlConnection.Open();

        //        using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_SAVEADDITIONALFIELDATTRIBUTEFROMINTERFACE))
        //        {
        //            objSqlCommand.CommandType = CommandType.StoredProcedure;
        //            objSqlCommand.Parameters.AddWithValue("@id", documentID);
        //            objSqlCommand.Parameters.AddWithValue("@documentItemID", documentItemID);
        //            SqlParameter objSqlParameter = new SqlParameter("@RequisitionItemAdditionalFields", SqlDbType.Structured)
        //            {
        //                TypeName = ReqSqlConstants.TVP_P2P_BZ_REQUISITIONITEMADDITIONALFIELDS,
        //                Value = ConvertAdditionalFieldAtrributeToTableType(documentID,documentItemID,lstAdditionalFieldAttributues)
        //            };
        //            objSqlCommand.Parameters.Add(objSqlParameter);
        //            objSqlCommand.Parameters.AddWithValue("@PurchaseType", PurchaseTypeDescription);

        //            flag = Convert.ToBoolean(ContextSqlConn.ExecuteScalar(objSqlCommand), CultureInfo.InvariantCulture);
        //        }


        //    }

        //    catch (Exception ex)
        //    {
        //        LogHelper.LogError(Log, "Error occured in SQLRequisitionDAO SaveAdditionalFieldAttributes Method for requisitionId = " + documentID, ex);
        //        throw ex;
        //    }
        //    finally
        //    {
        //        if (!ReferenceEquals(objSqlConnection, null) && objSqlConnection.State != ConnectionState.Closed)
        //        {
        //            objSqlConnection.Close();
        //            objSqlConnection.Dispose();
        //        }
        //        LogHelper.LogInfo(Log, "Requisition SaveAdditionalFieldAttributes method completed.");
        //    }

        //}

        //private DataTable ConvertAdditionalFieldAtrributeToTableType(long documentID, long documentItemID, List<P2PAdditionalFieldAtrribute> p2PAdditionalFieldAtrributes)
        //{
        //    DataTable dtAdditionalFieldAttributes = new DataTable() { Locale = CultureInfo.InvariantCulture, TableName = ReqSqlConstants.TVP_P2P_BZ_REQUISITIONITEMADDITIONALFIELDS };
        //    dtAdditionalFieldAttributes.Columns.Add("RequisitionID", typeof(int));
        //    dtAdditionalFieldAttributes.Columns.Add("RequisitionItemID", typeof(int));
        //    dtAdditionalFieldAttributes.Columns.Add("AdditionalFieldID", typeof(int));
        //    dtAdditionalFieldAttributes.Columns.Add("AdditionalFieldValue", typeof(string));
        //    dtAdditionalFieldAttributes.Columns.Add("AdditionalFieldName", typeof(string));
        //    dtAdditionalFieldAttributes.Columns.Add("AdditionalFieldCode", typeof(string));
        //    dtAdditionalFieldAttributes.Columns.Add("CreatedBy", typeof(long));
        //    dtAdditionalFieldAttributes.Columns.Add("FeatureId", typeof(int));
        //    dtAdditionalFieldAttributes.Columns.Add("AdditionalFieldDetailCode", typeof(long));
        //    dtAdditionalFieldAttributes.Columns.Add("ItemLineNumber", typeof(int));

        //    if (p2PAdditionalFieldAtrributes != null && p2PAdditionalFieldAtrributes.Any())
        //    {
        //        foreach (var additionalFieldAtrribute in p2PAdditionalFieldAtrributes)
        //        {
        //            DataRow dr = dtAdditionalFieldAttributes.NewRow();
        //            dr["RequisitionID"] = documentID;
        //            dr["RequisitionItemID"] = documentItemID;
        //            dr["AdditionalFieldID"] = 0;
        //            dr["AdditionalFieldValue"] = Convert.ToString(additionalFieldAtrribute.AdditionalFieldValue);
        //            dr["AdditionalFieldName"] = Convert.ToString(additionalFieldAtrribute.AdditionalFieldName);
        //            dr["AdditionalFieldCode"] = Convert.ToString(additionalFieldAtrribute.AdditionalFieldCode);
        //            dr["CreatedBy"] = 0;
        //            dr["FeatureId"] = 0;
        //            dr["AdditionalFieldDetailCode"] = 0;
        //            dr["ItemLineNumber"] = 0;

        //            dtAdditionalFieldAttributes.Rows.Add(dr);
        //        }
        //   }
        //   return dtAdditionalFieldAttributes;
        //}

        public DocumentIntegrationEntity GetDocumentDetailsBySelectedReqWorkbenchItems(DataTable dtReqItemIds, List<long> partners = null, List<DocumentIntegration.Entities.IntegrationTimelines> timelines = null, List<long> teammemberList = null)
        {
            DocumentIntegrationEntity objDocumentIntegrationEntity = new DocumentIntegration.Entities.DocumentIntegrationEntity();
            RefCountingDataReader objrequisitionItemDataReader = null;
            if (dtReqItemIds != null)
            {
                try
                {
                    LogHelper.LogInfo(Log, "GetDocumentDetailsBySelectedReqWorkbenchItems Method Started");
                    var sqlHelper = ContextSqlConn;
                    Document objDocument = new Document();
                    objDocument.DocumentCode = 0;
                    objDocument.DocumentName = "RfxDocument";
                    objDocument.DocumentTypeInfo = DocumentType.Requisition;
                    objDocumentIntegrationEntity.Document = objDocument;
                    objDocumentIntegrationEntity.DocumentItems = new List<GEP.Cumulus.Item.Entities.LineItem>();

                    LogHelper.LogInfo(Log, "Requisition GetDocumentDetailsBySelectedReqWorkbenchItems Method Started.");

                    var objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_GetDocumentDetailsBySelectedReqWorkbenchItems);
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.Add(new SqlParameter("@tvp_P2P_REQ_ReqItemIds", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_LONG,
                        Value = dtReqItemIds
                    });
                    objrequisitionItemDataReader = (RefCountingDataReader)ContextSqlConn.ExecuteReader(objSqlCommand);

                    if (objrequisitionItemDataReader != null)
                    {
                        var sqlDr = (SqlDataReader)objrequisitionItemDataReader.InnerReader;

                        DocumentPas objDocumentPas = null;
                        while (sqlDr.Read())
                        {
                            objDocumentPas = new DocumentPas();
                            objDocumentPas.PasCode = GetLongValue(sqlDr, ReqSqlConstants.COL_PASCODE);
                            objDocumentPas.PasName = GetStringValue(sqlDr, ReqSqlConstants.COL_PASNAME);
                            objDocumentPas.ClientPASCode = GetStringValue(sqlDr, ReqSqlConstants.COL_CLIENT_PASCODE);
                            objDocumentIntegrationEntity.Document.DocumentPASList.Add(objDocumentPas);
                        }

                        if (sqlDr.NextResult())
                        {
                            DocumentBU objDocumentBU = null;
                            while (sqlDr.Read())
                            {
                                objDocumentBU = new DocumentBU();
                                objDocumentBU.BusinessUnitCode = GetLongValue(sqlDr, ReqSqlConstants.COL_BU_BusinessUnitCode);
                                objDocumentBU.BusinessUnitName = GetStringValue(sqlDr, ReqSqlConstants.COL_BU_BusinessUnitName);
                                objDocumentBU.BusinessUnitEntityCode = GetStringValue(sqlDr, ReqSqlConstants.COL_BU_BusinessUnitEntityCode);
                                objDocumentIntegrationEntity.Document.DocumentBUList.Add(objDocumentBU);
                            }
                        }

                        if (sqlDr.NextResult())
                        {
                            while (sqlDr.Read())
                            {
                                var objRequisitionLineItems = new GEP.Cumulus.Item.Entities.LineItem
                                {

                                    ItemId = GetLongValue(sqlDr, ReqSqlConstants.COL_ITEM_ID),
                                    IncoTermId = GetIntValue(sqlDr, ReqSqlConstants.COL_INCOTERMID),
                                    IncoTermCode = GetStringValue(sqlDr, ReqSqlConstants.COL_INCOTERMCODE),
                                    IncoTermLocation = GetStringValue(sqlDr, ReqSqlConstants.COL_INCOTERMLOCATION),
                                    DocumentNumber = GetStringValue(sqlDr, ReqSqlConstants.COL_DOCUMENT_NUMBER),
                                    ItemCode = GetLongValue(sqlDr, ReqSqlConstants.COL_ITEM_CODE),
                                    ItemName = GetStringValue(sqlDr, ReqSqlConstants.COL_SHORT_NAME),
                                    PartnerCode = (long)GetDecimalValue(sqlDr, ReqSqlConstants.COL_PARTNER_CODE),
                                    ItemDescription = GetStringValue(sqlDr, ReqSqlConstants.COL_DESCRIPTION),
                                    UOMCode = GetStringValue(sqlDr, ReqSqlConstants.COL_UOM),
                                    UnitPrice = sqlDr[ReqSqlConstants.COL_UNIT_PRICE] != DBNull.Value ? GetDecimalValue(sqlDr, ReqSqlConstants.COL_UNIT_PRICE) : 0,
                                    Quantity = GetDecimalValue(sqlDr, ReqSqlConstants.COL_QUANTITY),
                                    DocumentCode = GetLongValue(sqlDr, ReqSqlConstants.COL_REQUISITION_ID),
                                    ItemType = (GEP.Cumulus.Item.Entities.ItemType)GetTinyIntValue(sqlDr, ReqSqlConstants.COL_ITEM_TYPE_ID),
                                    UOMDescription = GetStringValue(sqlDr, ReqSqlConstants.COL_UOM_DESCRIPTION),
                                    PartnerName = GetStringValue(sqlDr, ReqSqlConstants.COL_PARTNER_NAME),
                                    LineNumber = Convert.ToString(GetLongValue(sqlDr, ReqSqlConstants.COL_LINENUMBER)),
                                    P2PLineItemId = GetLongValue(sqlDr, ReqSqlConstants.COL_P2P_LINE_ITEM_ID),
                                    ItemNumber = GetStringValue(sqlDr, ReqSqlConstants.COL_ITEMNUMBER),
                                    CurrencyCode = GetStringValue(sqlDr, ReqSqlConstants.COL_CURRENCY),
                                    ItemSpecification = GetStringValue(sqlDr, ReqSqlConstants.COL_ITEMSPECIFICATION),
                                    InternalPlantMemo = GetStringValue(sqlDr, ReqSqlConstants.COL_INTERNALPLANTMEMO),
                                    SupplierPartNumber=GetStringValue(sqlDr,ReqSqlConstants.COL_SUPPLIERPARTID),
                                    PASCode = GetLongValue(sqlDr, ReqSqlConstants.COL_CATEGORY_ID),
                                    CategoryName = GetStringValue(sqlDr, ReqSqlConstants.COL_CATEGORY_NAME),
                                    ClientPASCode =  GetStringValue(sqlDr, ReqSqlConstants.COL_CLIENTPASCODE)
                                };
                                
                                objDocumentIntegrationEntity.DocumentItems.Add(objRequisitionLineItems);
                            }
                        }

                    }

                    if (partners != null)
                    {
                        List<DocumentStakeHolder> stakeHoldersList = new List<DocumentStakeHolder>();
                        foreach (long partnerCode in partners)
                        {
                            if (objDocumentIntegrationEntity.DocumentStakeHolders != null)
                            {
                                if (!objDocumentIntegrationEntity.DocumentStakeHolders.Any(e => e.PartnerCode == partnerCode))
                                {
                                    DocumentStakeHolder documentStakeHolder = new DocumentStakeHolder
                                    {
                                        PartnerCode = partnerCode,
                                        StakeholderTypeInfo = StakeholderType.SupplierPrimaryContact
                                    };
                                    stakeHoldersList.Add(documentStakeHolder);
                                }
                            }
                            else
                            {
                                DocumentStakeHolder documentStakeHolder = new DocumentStakeHolder
                                {
                                    PartnerCode = partnerCode,
                                    StakeholderTypeInfo = StakeholderType.SupplierPrimaryContact
                                };
                                stakeHoldersList.Add(documentStakeHolder);
                            }
                        }
                        objDocumentIntegrationEntity.DocumentStakeHolders = stakeHoldersList;
                    }

                    if (teammemberList != null && teammemberList.Count > 0)
                    {
                        List<DocumentStakeHolder> lstStakeHoldersList = new List<DocumentStakeHolder>();
                        foreach (var teammember in teammemberList.Distinct().ToList())
                        {
                            DocumentStakeHolder documentStakeHolder = new DocumentStakeHolder();
                            documentStakeHolder.ContactCode = teammember;
                            documentStakeHolder.StakeholderTypeInfo = StakeholderType.TeamMembers;
                            documentStakeHolder.DocumentStakeholderId = 2;//1-View,2-Co-Author,4-Evaluator,5-Approver                                                                                 
                            lstStakeHoldersList.Add(documentStakeHolder);

                        }
                        if (objDocumentIntegrationEntity.DocumentStakeHolders != null && objDocumentIntegrationEntity.DocumentStakeHolders.Count > 0)
                            objDocumentIntegrationEntity.DocumentStakeHolders.AddRange(lstStakeHoldersList.Distinct().ToList());
                        else
                            objDocumentIntegrationEntity.DocumentStakeHolders = lstStakeHoldersList.Distinct().ToList();


                    }

                    if (timelines != null)
                    {
                        objDocumentIntegrationEntity.Timelines = timelines;
                        objDocumentIntegrationEntity.IntegrationTemplateType = DocumentIntegration.Entities.IntegrationTemplateType.AutoPublish;
                    }
                    else
                    {
                        objDocumentIntegrationEntity.IntegrationTemplateType = DocumentIntegration.Entities.IntegrationTemplateType.Draft;
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.LogError(Log, "Requisition GetDocumentDetailsBySelectedReqWorkbenchItems getting Exception", ex);
                    throw;
                }
                finally
                {
                    if (!ReferenceEquals(objrequisitionItemDataReader, null) && !objrequisitionItemDataReader.IsClosed)
                    {
                        objrequisitionItemDataReader.Close();
                        objrequisitionItemDataReader.Dispose();
                    }

                    LogHelper.LogInfo(Log, "Requisition GetDocumentDetailsBySelectedReqWorkbenchItems Method completed.");
                }
            }
            return objDocumentIntegrationEntity;
        }

        //public bool UpdateLineStatusForRequisitionFromInterface(long RequisitionId, BusinessEntities.StockReservationStatus LineStatus, bool IsUpdateAllItems, List<BusinessEntities.LineStatusRequisition> Items, string StockReservationNumber)
        //{
        //    SqlConnection _sqlCon = null;
        //    SqlTransaction _sqlTrans = null;
        //    try
        //    {
        //        LogHelper.LogInfo(Log, "Requisition UpdateLineStatusForRequisitionFromInterface Method Started for DocumentItemId = " + RequisitionId);
        //        var sqlHelper = ContextSqlConn;
        //        _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
        //        _sqlCon.Open();
        //        _sqlTrans = _sqlCon.BeginTransaction();
        //        if (Log.IsDebugEnabled)
        //            Log.Debug(string.Concat("Requisition UpdateLineStatusForRequisitionFromInterface sp usp_P2P_REQ_UpdateRequsitionItemStatusFromInterface with parameter: ;=" + RequisitionId + " was called."));
        //        bool result = false;
        //        DataTable dtLineStatus = new DataTable();
        //        dtLineStatus.Columns.Add("LineNumber", typeof(long));
        //        dtLineStatus.Columns.Add("LineStatus", typeof(long));
        //        dtLineStatus.Columns.Add("ItemType", typeof(long));
        //        dtLineStatus.Columns.Add("ReservationNumber", typeof(string));

        //        if (Items != null && Items.Any())
        //        {
        //            foreach (var item in Items)
        //            {
        //                DataRow dr = dtLineStatus.NewRow();
        //                dr["LineNumber"] = item.LineNumber != 0 ? Convert.ToInt64(item.LineNumber) : 0;
        //                dr["LineStatus"] = Convert.ToInt32(item.LineStatus);
        //                dr["ItemType"] = Convert.ToInt32(item.ItemType);
        //                dr["ReservationNumber"] = Convert.ToString(item.ReservationNumber);
        //                dtLineStatus.Rows.Add(dr);
        //            }
        //        }
        //        using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_UPDATEINTERFACEREQUSITIONITEMSTATUS))
        //        {
        //            objSqlCommand.CommandType = CommandType.StoredProcedure;
        //            objSqlCommand.Parameters.Add(new SqlParameter("@RequisitionId", RequisitionId));
        //            objSqlCommand.Parameters.Add(new SqlParameter("@ForAllItemsStatus", (int)LineStatus));
        //            objSqlCommand.Parameters.Add(new SqlParameter("@UpdateAllItems", IsUpdateAllItems));
        //            objSqlCommand.Parameters.Add(new SqlParameter("@tvpItemStatus", SqlDbType.Structured)
        //            {
        //                TypeName = ReqSqlConstants.tvp_Item_ItemStatus,
        //                Value = dtLineStatus
        //            });
        //            objSqlCommand.Parameters.Add(new SqlParameter("@StockReservationNumber", StockReservationNumber));

        //            result = Convert.ToBoolean(sqlHelper.ExecuteNonQuery(objSqlCommand, _sqlTrans), NumberFormatInfo.InvariantInfo);
        //            _sqlTrans.Commit();
        //        }

        //        if (result && RequisitionId > 0)
        //            AddIntoSearchIndexerQueueing(RequisitionId, REQ, UserContext, GepConfiguration);

        //        return result;
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

        //        LogHelper.LogInfo(Log, "Requisition UpdateLineStatusForRequisitionFromInterface Method Ended for DocumentItemId = " + RequisitionId);
        //    }
        //}

        public GEP.Cumulus.SmartCatalog.BusinessEntities.NewCatalog.ItemSearchInput GetCatalogLineDetails(long documentId)
        {
            SqlConnection sqlConnection = null;
            var sqlHelper = ContextSqlConn;
            DataSet objDs = null;
            //List<CatalogLineDetail> lstcatalogLineDetails = new List<CatalogLineDetail>();
            try
            {
                LogHelper.LogInfo(Log, "Requisition GetCatalogLineDetails Method Started for documentId=" + documentId);

                sqlConnection = (SqlConnection)sqlHelper.CreateConnection();
                sqlConnection.Open();

                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_REQ_GetPriceDetailOfCatalogLineItems))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.CommandTimeout = 0;
                    objSqlCommand.Parameters.Add(new SqlParameter("@requisitionId", documentId));
                    objDs = sqlHelper.ExecuteDataSet(objSqlCommand);


                }
                List<GEP.Cumulus.SmartCatalog.BusinessEntities.NewCatalog.CatalogItemAdditionalInfo> lstCatalogItemAdditionalInfoList = new List<GEP.Cumulus.SmartCatalog.BusinessEntities.NewCatalog.CatalogItemAdditionalInfo>();
                GEP.Cumulus.SmartCatalog.BusinessEntities.NewCatalog.ItemSearchInput objcatalogLineDetails = new GEP.Cumulus.SmartCatalog.BusinessEntities.NewCatalog.ItemSearchInput();
                List<long> AccessEntities = new List<long>();
                if (objDs != null && objDs.Tables.Count > 0)
                {
                    if (objDs.Tables[0].Rows != null && objDs.Tables[0].Rows.Count > 0)
                    {


                        objcatalogLineDetails.Size = objDs.Tables[0].Rows.Count;
                        foreach (DataRow dr in objDs.Tables[1].Rows)
                        {

                            long accessEntities = dr[ReqSqlConstants.COL_ENTITYDETAILCODE] != DBNull.Value ? Convert.ToInt64(dr[ReqSqlConstants.COL_ENTITYDETAILCODE]) : 0;
                            AccessEntities.Add(accessEntities);
                        }
                        objcatalogLineDetails.AccessEntities = AccessEntities;

                        if (objDs.Tables[1].Rows != null && objDs.Tables[1].Rows.Count > 0)
                        {
                            foreach (DataRow dr in objDs.Tables[0].Rows)
                            {
                                GEP.Cumulus.SmartCatalog.BusinessEntities.NewCatalog.CatalogItemAdditionalInfo catalogItemAdditionalInfoList = new GEP.Cumulus.SmartCatalog.BusinessEntities.NewCatalog.CatalogItemAdditionalInfo();
                                catalogItemAdditionalInfoList.CatalogItemId = dr[ReqSqlConstants.COL_CATALOGITEMID] != DBNull.Value ? Convert.ToInt64(dr[ReqSqlConstants.COL_CATALOGITEMID]) : 0;
                                catalogItemAdditionalInfoList.UOM = dr[ReqSqlConstants.COL_UOM] != DBNull.Value ? Convert.ToString(dr[ReqSqlConstants.COL_UOM]) : string.Empty;
                                catalogItemAdditionalInfoList.Quantity = dr[ReqSqlConstants.COL_QUANTITY] != DBNull.Value ? Convert.ToDecimal(dr[ReqSqlConstants.COL_QUANTITY]) : 0;
                                catalogItemAdditionalInfoList.Currency = dr[ReqSqlConstants.COL_CURRENCY] != DBNull.Value ? Convert.ToString(dr[ReqSqlConstants.COL_CURRENCY]) : string.Empty;
                                catalogItemAdditionalInfoList.StartDate = null;
                                if (Convert.ToInt16(dr[ReqSqlConstants.COL_ITEMTYPEID]) == Convert.ToInt16(ItemType.Material))
                                {
                                    if (dr[ReqSqlConstants.COL_DATE_NEEDED] is DBNull)
                                    {
                                        catalogItemAdditionalInfoList.StartDate = null;
                                    }
                                    else
                                    {
                                        catalogItemAdditionalInfoList.StartDate = Convert.ToDateTime(dr[ReqSqlConstants.COL_DATE_NEEDED]);
                                    }

                                }
                                if (Convert.ToInt16(dr[ReqSqlConstants.COL_ITEMTYPEID]) == Convert.ToInt16(ItemType.Service))
                                {
                                    if (dr[ReqSqlConstants.COL_START_DATE] is DBNull)
                                    {
                                        catalogItemAdditionalInfoList.StartDate = null;
                                    }
                                    else
                                    {
                                        catalogItemAdditionalInfoList.StartDate = Convert.ToDateTime(dr[ReqSqlConstants.COL_START_DATE]);
                                    }

                                }

                                catalogItemAdditionalInfoList.AccessEntities = AccessEntities;
                                lstCatalogItemAdditionalInfoList.Add(catalogItemAdditionalInfoList);
                            }
                        }

                    }



                }
                objcatalogLineDetails.CatalogItemAdditionalInfoList = lstCatalogItemAdditionalInfoList;
                return objcatalogLineDetails;
            }
            catch (Exception ex)
            {
                LogHelper.LogInfo(Log, "Error Occurred in GetAllUsersByActivityCode Method.");
                throw ex;

            }
            finally
            {
                if (!ReferenceEquals(sqlConnection, null) && sqlConnection.State != ConnectionState.Closed)
                {
                    sqlConnection.Close();
                    sqlConnection.Dispose();
                }
            }
        }

    public GEP.Cumulus.SmartCatalog.BusinessEntities.NewCatalog.ItemBulkInputRequest GetCatalogLineDetailsForWebAPI(long documentId)
    {
      SqlConnection sqlConnection = null;
      var sqlHelper = ContextSqlConn;
      DataSet objDs = null;
      try
      {
        LogHelper.LogInfo(Log, "Requisition GetCatalogLineDetails Method Started for documentId=" + documentId);

        sqlConnection = (SqlConnection)sqlHelper.CreateConnection();
        sqlConnection.Open();

        using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_REQ_GetPriceDetailOfCatalogLineItems))
        {
          objSqlCommand.CommandType = CommandType.StoredProcedure;
          objSqlCommand.CommandTimeout = 0;
          objSqlCommand.Parameters.Add(new SqlParameter("@requisitionId", documentId));
          objDs = sqlHelper.ExecuteDataSet(objSqlCommand);
        }
        List<GEP.Cumulus.SmartCatalog.BusinessEntities.NewCatalog.CatalogItemAdditionalInfo> lstCatalogItemAdditionalInfoList = new List<GEP.Cumulus.SmartCatalog.BusinessEntities.NewCatalog.CatalogItemAdditionalInfo>();
        GEP.Cumulus.SmartCatalog.BusinessEntities.NewCatalog.ItemBulkInputRequest objcatalogLineDetails = new GEP.Cumulus.SmartCatalog.BusinessEntities.NewCatalog.ItemBulkInputRequest();
        List<long> AccessEntities = new List<long>();
        if (objDs != null && objDs.Tables.Count > 0)
        {
          if (objDs.Tables[0].Rows != null && objDs.Tables[0].Rows.Count > 0)
          {
            objcatalogLineDetails.Size = objDs.Tables[0].Rows.Count;
            foreach (DataRow dr in objDs.Tables[1].Rows)
            {
              long accessEntities = dr[ReqSqlConstants.COL_ENTITYDETAILCODE] != DBNull.Value ? Convert.ToInt64(dr[ReqSqlConstants.COL_ENTITYDETAILCODE]) : 0;
              AccessEntities.Add(accessEntities);
            }
            objcatalogLineDetails.AccessEntities = AccessEntities;

            if (objDs.Tables[1].Rows != null && objDs.Tables[1].Rows.Count > 0)
            {
              foreach (DataRow dr in objDs.Tables[0].Rows)
              {
                GEP.Cumulus.SmartCatalog.BusinessEntities.NewCatalog.CatalogItemAdditionalInfo catalogItemAdditionalInfoList = new GEP.Cumulus.SmartCatalog.BusinessEntities.NewCatalog.CatalogItemAdditionalInfo();
                catalogItemAdditionalInfoList.CatalogItemId = dr[ReqSqlConstants.COL_CATALOGITEMID] != DBNull.Value ? Convert.ToInt64(dr[ReqSqlConstants.COL_CATALOGITEMID]) : 0;
                catalogItemAdditionalInfoList.UOM = dr[ReqSqlConstants.COL_UOM] != DBNull.Value ? Convert.ToString(dr[ReqSqlConstants.COL_UOM]) : string.Empty;
                catalogItemAdditionalInfoList.Quantity = dr[ReqSqlConstants.COL_QUANTITY] != DBNull.Value ? Convert.ToDecimal(dr[ReqSqlConstants.COL_QUANTITY]) : 0;
                catalogItemAdditionalInfoList.Currency = dr[ReqSqlConstants.COL_CURRENCY] != DBNull.Value ? Convert.ToString(dr[ReqSqlConstants.COL_CURRENCY]) : string.Empty;
                catalogItemAdditionalInfoList.StartDate = null;
                if (Convert.ToInt16(dr[ReqSqlConstants.COL_ITEMTYPEID]) == Convert.ToInt16(ItemType.Material))
                {
                  if (dr[ReqSqlConstants.COL_DATE_NEEDED] is DBNull)
                  {
                    catalogItemAdditionalInfoList.StartDate = null;
                  }
                  else
                  {
                    catalogItemAdditionalInfoList.StartDate = Convert.ToDateTime(dr[ReqSqlConstants.COL_DATE_NEEDED]);
                  }
                }
                if (Convert.ToInt16(dr[ReqSqlConstants.COL_ITEMTYPEID]) == Convert.ToInt16(ItemType.Service))
                {
                  if (dr[ReqSqlConstants.COL_START_DATE] is DBNull)
                  {
                    catalogItemAdditionalInfoList.StartDate = null;
                  }
                  else
                  {
                    catalogItemAdditionalInfoList.StartDate = Convert.ToDateTime(dr[ReqSqlConstants.COL_START_DATE]);
                  }
                }
                catalogItemAdditionalInfoList.AccessEntities = AccessEntities;
                lstCatalogItemAdditionalInfoList.Add(catalogItemAdditionalInfoList);
              }
            }
          }
        }
        objcatalogLineDetails.CatalogItemAdditionalInfoList = lstCatalogItemAdditionalInfoList;
        return objcatalogLineDetails;
      }
      catch (Exception ex)
      {
        LogHelper.LogInfo(Log, "Error Occurred in GetCatalogLineDetailsForWebAPI Method.");
        throw ex;

      }
      finally
      {
        if (!ReferenceEquals(sqlConnection, null) && sqlConnection.State != ConnectionState.Closed)
        {
          sqlConnection.Close();
          sqlConnection.Dispose();
        }
      }
    }

    public int UpdateCatalogLineDetails(List<GEP.Cumulus.SmartCatalog.BusinessEntities.NewCatalog.CatalogItem> Items, long documentId)
        {
            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;
            try
            {
                LogHelper.LogInfo(Log, "Requisition SaveDocumentAdditionalEntityInfo Method Started for DocumentItemId = ");

                var sqlHelper = ContextSqlConn;
                _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();
                DataSet dsReqdata = null;
                int eventPerformed = 0;

                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("Requisition SaveDocumentAdditionalEntityInfo sp usp_P2P_REQ_SaveReqAdditionalEntityDetails with parameter: lstEntityInfo="));


                DataTable dtCatalogLineDetails = null;
                dtCatalogLineDetails = ConvertCatalogDetailsToDataTable(Items);                              
                List<string> events = new List<string>();

                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_REQ_UPDATEPRICEDETAILOFCATALOGLINEITEMS))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.Add(new SqlParameter("@requisitionid", SqlDbType.BigInt) { Value = documentId });
                    objSqlCommand.Parameters.Add(new SqlParameter("@CatalogLineItems", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_CatalogLineitem,
                        Value = dtCatalogLineDetails
                    });


                    dsReqdata = sqlHelper.ExecuteDataSet(objSqlCommand);
                }
                if (dsReqdata != null && dsReqdata.Tables.Count > 0)
                {
                    if (dsReqdata.Tables[0].Rows.Count > 0)
                    {
                        foreach (DataRow dr in dsReqdata.Tables[0].Rows)
                        {
                            string EventName = Convert.ToString(dr["EventName"]);
                            events.Add(EventName);
                        }
                    }
                }
                //0 no error
                //1 items deleted
                //2 items price updated
                //3 item deleted and price updated
                if (events.Count > 0)
                {
                    eventPerformed = ActionPerformed(events);
                }
                else { eventPerformed = 0; }




                if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
                    _sqlTrans.Commit();
                return eventPerformed;
            }
            catch (Exception)
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
                throw;
            }
            finally
            {
                if (!ReferenceEquals(_sqlCon, null) && _sqlCon.State != ConnectionState.Closed)
                {
                    _sqlCon.Close();
                    _sqlCon.Dispose();
                }

                LogHelper.LogInfo(Log, "Requisition SaveDocumentAdditionalEntityInfo Method Ended for DocumentItemId");
            }
        }

        public DataTable ConvertCatalogDetailsToDataTable(List<GEP.Cumulus.SmartCatalog.BusinessEntities.NewCatalog.CatalogItem> Items)
        {
            DataTable dtCatalogItem = new DataTable() { Locale = CultureInfo.InvariantCulture, TableName = ReqSqlConstants.TVP_CatalogLineitem };
            dtCatalogItem.Columns.Add("CatalogItemId", typeof(Int64));
            dtCatalogItem.Columns.Add("UnitPrice");

            if (Items != null)
            {
                foreach (var objCatalogItems in Items)
                {
                        DataRow dr = dtCatalogItem.NewRow();
                        if (objCatalogItems.EffectivePrice == null)
                        {
                            dr["UnitPrice"] = objCatalogItems.UnitPrice;
                        }
                        else
                        {
                            dr["UnitPrice"] = objCatalogItems.EffectivePrice;
                        }                      
                            dr["CatalogItemId"] = objCatalogItems.Id;                        
                        dtCatalogItem.Rows.Add(dr);
                    
                }
            }
            return dtCatalogItem;
        }

        public int ActionPerformed(List<string> events)
        {


            if (events.Contains("Delete") && events.Contains("update"))
            {
                return 3;
            }
            else if (events.Contains("Delete"))
            {
                return 1;
            }
            else
            {
                return 2;
            }



        }
        public GEP.Cumulus.SmartCatalog.BusinessEntities.NewCatalog.ItemSearchBulkInput GetCatalogLineDetailsForBulkWebAPI(long documentId, string requisitionIds = "")
        {
            SqlConnection sqlConnection = null;
            var sqlHelper = ContextSqlConn;
            DataSet objDs = null;
            try
            {
                LogHelper.LogInfo(Log, "Requisition GetCatalogLineDetailsForBulkWebAPI Method Started for documentId=" + documentId);

                sqlConnection = (SqlConnection)sqlHelper.CreateConnection();
                sqlConnection.Open();

                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_REQ_GETCATALOGLINEITEMDETAILS))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.CommandTimeout = 0;
                    objSqlCommand.Parameters.Add(new SqlParameter("@requisitionId", documentId));
                    objSqlCommand.Parameters.Add(new SqlParameter("@sourceRequisitionIds", requisitionIds));
                    objDs = sqlHelper.ExecuteDataSet(objSqlCommand);
                }
                List<GEP.Cumulus.SmartCatalog.BusinessEntities.NewCatalog.ItemInput> lstCatalogItemInputList = new List<GEP.Cumulus.SmartCatalog.BusinessEntities.NewCatalog.ItemInput>();
                GEP.Cumulus.SmartCatalog.BusinessEntities.NewCatalog.ItemSearchBulkInput objcatalogLineDetails = new GEP.Cumulus.SmartCatalog.BusinessEntities.NewCatalog.ItemSearchBulkInput();
                List<GEP.Cumulus.SmartCatalog.BusinessEntities.NewCatalog.OrgEntity> AccessEntities = new List<GEP.Cumulus.SmartCatalog.BusinessEntities.NewCatalog.OrgEntity>();
                if (objDs != null && objDs.Tables.Count > 0)
                {
                    if (objDs.Tables[0].Rows != null && objDs.Tables[0].Rows.Count > 0)
                    {
                        objcatalogLineDetails.Size = objDs.Tables[0].Rows.Count;
                        foreach (DataRow dr in objDs.Tables[1].Rows)
                        {
                            long accessEntities = dr[ReqSqlConstants.COL_ENTITYDETAILCODE] != DBNull.Value ? Convert.ToInt64(dr[ReqSqlConstants.COL_ENTITYDETAILCODE]) : 0;
                            AccessEntities.Add(new GEP.Cumulus.SmartCatalog.BusinessEntities.NewCatalog.OrgEntity { EntityDetailCode = accessEntities });
                        }
                        objcatalogLineDetails.AccessEntities = AccessEntities;

                        if (objDs.Tables[1].Rows != null && objDs.Tables[1].Rows.Count > 0)
                        {
                            foreach (DataRow dr in objDs.Tables[0].Rows)
                            {
                                GEP.Cumulus.SmartCatalog.BusinessEntities.NewCatalog.ItemInput catalogItemInputList = new GEP.Cumulus.SmartCatalog.BusinessEntities.NewCatalog.ItemInput();
                                catalogItemInputList.CatalogItemId = dr[ReqSqlConstants.COL_CATALOGITEMID] != DBNull.Value ? Convert.ToInt64(dr[ReqSqlConstants.COL_CATALOGITEMID]) : 0;
                                catalogItemInputList.UOM = dr[ReqSqlConstants.COL_UOM] != DBNull.Value ? Convert.ToString(dr[ReqSqlConstants.COL_UOM]) : string.Empty;
                                catalogItemInputList.Quantity = dr[ReqSqlConstants.COL_QUANTITY] != DBNull.Value ? Convert.ToDecimal(dr[ReqSqlConstants.COL_QUANTITY]) : 0;
                                catalogItemInputList.CurrencyCode = dr[ReqSqlConstants.COL_CURRENCY] != DBNull.Value ? Convert.ToString(dr[ReqSqlConstants.COL_CURRENCY]) : string.Empty;
                                catalogItemInputList.BIN = dr[ReqSqlConstants.COL_ITEMNUMBER] != DBNull.Value ? Convert.ToString(dr[ReqSqlConstants.COL_ITEMNUMBER]) : string.Empty; ;
                                catalogItemInputList.SIN = dr[ReqSqlConstants.COL_SUPPLIERPARTID] != DBNull.Value ? Convert.ToString(dr[ReqSqlConstants.COL_SUPPLIERPARTID]) : string.Empty; ;
                                catalogItemInputList.SupplierCode = dr[ReqSqlConstants.COL_PartnerCode] != DBNull.Value ? Convert.ToInt64(dr[ReqSqlConstants.COL_PartnerCode]) : 0;
                                catalogItemInputList.ContactCode = dr[ReqSqlConstants.COL_ONBEHALFOF] != DBNull.Value ? Convert.ToInt64(dr[ReqSqlConstants.COL_ONBEHALFOF]) : 0;
                                catalogItemInputList.IMId = dr[ReqSqlConstants.COL_ITEMMASTERID] != DBNull.Value ? Convert.ToInt64(dr[ReqSqlConstants.COL_ITEMMASTERID]) : 0;
                                catalogItemInputList.AccessEntities = AccessEntities;
                                lstCatalogItemInputList.Add(catalogItemInputList);
                            }
                        }
                    }
                }
                objcatalogLineDetails.ItemInputList = lstCatalogItemInputList;
                return objcatalogLineDetails;
            }
            catch (Exception ex)
            {
                LogHelper.LogInfo(Log, "Error Occurred in GetCatalogLineDetailsForBulkWebAPI Method.");
                throw ex;

            }
            finally
            {
                if (!ReferenceEquals(sqlConnection, null) && sqlConnection.State != ConnectionState.Closed)
                {
                    sqlConnection.Close();
                    sqlConnection.Dispose();
                }
            }
        }

        public DataTable ConvertCatalogKeyValueToDataTable(List<KeyValuePair<long, decimal>> Items)
        {
            DataTable dtCatalogItem = new DataTable() { Locale = CultureInfo.InvariantCulture, TableName = ReqSqlConstants.TVP_CatalogLineitem };
            dtCatalogItem.Columns.Add("CatalogItemId", typeof(Int64));
            dtCatalogItem.Columns.Add("UnitPrice");

            if (Items != null)
            {
                foreach (var objCatalogItems in Items) 
                {
                   
                        DataRow dr = dtCatalogItem.NewRow();                      
                        dr["UnitPrice"] = objCatalogItems.Value;                        
                        dr["CatalogItemId"] = objCatalogItems.Key;
                        dtCatalogItem.Rows.Add(dr);
                    
                }
            }
            return dtCatalogItem;
        }

        public DataTable ConvertCurrencyExchangesToDataTable(List<CurrencyExchageRate> currencyExchageRates)
        {
            DataTable dtCurrencyExchangeRate = new DataTable() { Locale = CultureInfo.InvariantCulture, TableName = ReqSqlConstants.tvp_CurrencyExchangeRates };
            try
            {
                dtCurrencyExchangeRate.Columns.Add("FromCurrency", typeof(string));
                dtCurrencyExchangeRate.Columns.Add("ToCurrency", typeof(string));
                dtCurrencyExchangeRate.Columns.Add("ExchangeRate", typeof(decimal));
                if (currencyExchageRates != null && currencyExchageRates.Count > 0)
                {
                    foreach (var currencyExchageRate in currencyExchageRates)
                    {
                        DataRow dr = dtCurrencyExchangeRate.NewRow();
                        dr["FromCurrency"] = currencyExchageRate.FromCurrencyCode;
                        dr["ToCurrency"] = currencyExchageRate.ToCurrencyCode;
                        dr["ExchangeRate"] = currencyExchageRate.ExchangeRate;
                        dtCurrencyExchangeRate.Rows.Add(dr);

                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in ConvertCurrencyExchangesToDataTable method ", ex);
                throw;
            }
            return dtCurrencyExchangeRate;
        }

        public bool UpdateRequsitionItemStatus(List<KeyValuePair<long, long>> requisitionItemStatus)
        {
            SqlConnection sqlCon = null;
            SqlTransaction sqlTrans = null;
            bool result = false;
            long contactCode = 0;
            try
            {
                LogHelper.LogInfo(Log, "In Requisition RollBackRequsitionItemStatus Method Started ");
                contactCode = UserContext.ContactCode;
                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("Requisition RollBackRequsitionItemStatus sp USP_P2P_REQ_ROLLBACKREQUSITIONITEMSTATUS "));
                var sqlHelper = ContextSqlConn;
                sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                sqlCon.Open();
                sqlTrans = sqlCon.BeginTransaction();
                DataTable requisitionItemStatusDt = new DataTable() { Locale = CultureInfo.InvariantCulture, TableName = ReqSqlConstants.TVP_P2P_KEYVALUEIDNAME };
                requisitionItemStatusDt.Columns.Add("Id", typeof(long));
                requisitionItemStatusDt.Columns.Add("Value", typeof(long));

                if (requisitionItemStatus != null)
                {
                    foreach (var item in requisitionItemStatus)
                    {
                        DataRow drItemRow = requisitionItemStatusDt.NewRow();
                        drItemRow["Id"] = item.Key;
                        drItemRow["Value"] = item.Value;
                        requisitionItemStatusDt.Rows.Add(drItemRow);
                    }
                }

                using (var objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_UpdateREQUSITIONITEMSTATUS))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.Add(new SqlParameter("@requisitionItemStatus", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_P2P_KEYVALUE,
                        Value = requisitionItemStatusDt
                    });
                    result = Convert.ToBoolean(sqlHelper.ExecuteScalar(objSqlCommand, sqlTrans), NumberFormatInfo.InvariantInfo);
                }
                if (!ReferenceEquals(sqlTrans, null) && !ReferenceEquals(sqlCon, null) && sqlCon.State == ConnectionState.Open)
                {
                    sqlTrans.Commit();

                }

                return result;
            }
            catch
            {
                if (!ReferenceEquals(sqlTrans, null) && !ReferenceEquals(sqlCon, null) && sqlCon.State == ConnectionState.Open)
                {
                    try
                    {
                        sqlTrans.Rollback();
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
                if (!ReferenceEquals(sqlCon, null) && sqlCon.State != ConnectionState.Closed)
                {
                    sqlCon.Close();
                    sqlCon.Dispose();
                }
                LogHelper.LogInfo(Log, "Requisition RollBackRequsitionItemStatus Method Ended ");
            }
        }

        public Dictionary<long, long> GetRequisitionByRequisitionItems(DataTable dtReqItemIds)
        {
            Dictionary<long, long> reqitems = new Dictionary<long, long>();
            RefCountingDataReader objrequisitionItemDataReader = null;
            try
            {
                if (dtReqItemIds != null)
                {
                    var sqlHelper = ContextSqlConn;
                    var objSqlCommand = new SqlCommand(ReqSqlConstants.usp_P2P_REQ_GetRequisitionID);
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.Add(new SqlParameter("@tvp_P2P_REQ_ReqItemIds", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_LONG,
                        Value = dtReqItemIds
                    });
                    objrequisitionItemDataReader = (RefCountingDataReader)ContextSqlConn.ExecuteReader(objSqlCommand);

                    if (objrequisitionItemDataReader != null)
                    {
                        var sqlDr = (SqlDataReader)objrequisitionItemDataReader.InnerReader;
                        while (sqlDr.Read())
                        {
                            long RequistionID = GetLongValue(sqlDr, ReqSqlConstants.COL_REQUISITION_ID);
                            long RequistionItemID = GetLongValue(sqlDr, ReqSqlConstants.COL_REQUISITION_ITEM_ID);
                            if (!reqitems.ContainsKey(RequistionItemID))
                            {
                                reqitems.Add(RequistionItemID, RequistionID);
                            }
                        }
                    }

                }

                return reqitems;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Requisition GetRequisitionByRequisitionItems getting Exception", ex);
                throw;
            }
            finally
            {
                if (!ReferenceEquals(objrequisitionItemDataReader, null) && !objrequisitionItemDataReader.IsClosed)
                {
                    objrequisitionItemDataReader.Close();
                    objrequisitionItemDataReader.Dispose();
                }


            }

        }
        public long GetContactsManagerMapping(long contactCode)
        {
            LogHelper.LogInfo(Log, "GetContactsManagerMapping Method Started");
            long result = 0;
            SqlConnection objSqlCon = null;
            try
            {
                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("In GetContactsManagerMapping Method with parameter: ContactCode = "
                                                , contactCode,
                                                " Stored Procedure to be executed is ", ReqSqlConstants.USP_P2P_REQ_GETACTIVECONTACTSMANAGERMAPPING));

                objSqlCon = (SqlConnection)ContextSqlConn.CreateConnection();
                objSqlCon.Open();
                DataSet dataset = ContextSqlConn.ExecuteDataSet(ReqSqlConstants.USP_P2P_REQ_GETACTIVECONTACTSMANAGERMAPPING, contactCode);

                if (dataset.Tables.Count > 0)
                {
                    foreach (DataRow dr in dataset.Tables[0].Rows)
                    {
                        result = (long)dr[ReqSqlConstants.COL_MANAGERCONTACTCODE];
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in GetContactsManagerMapping method. ContactCode : " + contactCode, ex);
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
            return result;
        }

        public List<CurrencyExchageRate> GetRequisitionCurrency(string RequisitionIds)
        {
            List<CurrencyExchageRate> lstCurrencyExchageRates = new List<CurrencyExchageRate>();
            RefCountingDataReader objrequisitionItemDataReader = null;
            try
            {
                var sqlHelper = ContextSqlConn;
                var objSqlCommand = new SqlCommand(ReqSqlConstants.usp_P2P_REQ_GetRequisitionCurrency);
                objSqlCommand.CommandType = CommandType.StoredProcedure;
                objSqlCommand.Parameters.AddWithValue("@RequisitionIDs", RequisitionIds);
                objrequisitionItemDataReader = (RefCountingDataReader)ContextSqlConn.ExecuteReader(objSqlCommand);

                if (objrequisitionItemDataReader != null)
                {
                    var sqlDr = (SqlDataReader)objrequisitionItemDataReader.InnerReader;
                    while (sqlDr.Read())
                    {
                        CurrencyExchageRate currencyExchageRate = new CurrencyExchageRate();
                        currencyExchageRate.FromCurrencyCode = GetStringValue(sqlDr, ReqSqlConstants.COL_FROMCURRENCY);
                        currencyExchageRate.ToCurrencyCode = GetStringValue(sqlDr, ReqSqlConstants.COL_ToCurrencyFlip);
                        lstCurrencyExchageRates.Add(currencyExchageRate);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in GetRequisitionCurrency Method" , ex);
                throw;
            }
            finally
            {
                if (!ReferenceEquals(objrequisitionItemDataReader, null) && !objrequisitionItemDataReader.IsClosed)
                {
                    objrequisitionItemDataReader.Close();
                    objrequisitionItemDataReader.Dispose();
                }
            }
            return lstCurrencyExchageRates;
        }
        public bool UpdateExtendedStatusforHoldRequisition(long documentCode, int extendedStatus, long onHeldBy)
        {
            SqlConnection sqlCon = null;
            SqlTransaction sqlTrans = null;
            bool result;
            try
            {
                LogHelper.LogInfo(Log, "In Requisition UpdateExtendedStatusforHoldRequisition Method Started for documentCode=" + documentCode);
                sqlCon = (SqlConnection)ContextSqlConn.CreateConnection();
                sqlCon.Open();
                sqlTrans = sqlCon.BeginTransaction();

                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("Requisition UpdateExtendedStatusforHoldRequisition sp USP_P2P_REQ_UPDATEEXTENDEDSTATUSFORHOLDREQUISITION with parameter: documentCode=" + documentCode + ", updatededExtendedStatus =" + extendedStatus + " was called."));

                result = Convert.ToBoolean(ContextSqlConn.ExecuteNonQuery(sqlTrans, ReqSqlConstants.USP_P2P_REQ_UPDATEEXTENDEDSTATUSFORHOLDREQUISITION, documentCode, extendedStatus, onHeldBy), NumberFormatInfo.InvariantInfo);
                if (!ReferenceEquals(sqlTrans, null) && !ReferenceEquals(sqlCon, null) && sqlCon.State == ConnectionState.Open)
                {
                    if (result)
                    {
                        sqlTrans.Commit();
                    }
                    else
                    {
                        try
                        {
                            sqlTrans.Rollback();
                        }
                        catch (InvalidOperationException error)
                        {
                            if (Log.IsInfoEnabled) Log.Info(error.Message);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                if (!ReferenceEquals(sqlTrans, null) && !ReferenceEquals(sqlCon, null) && sqlCon.State == ConnectionState.Open)
                {
                    try
                    {
                        sqlTrans.Rollback();
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
                if (!ReferenceEquals(sqlCon, null) && sqlCon.State != ConnectionState.Closed)
                {
                    sqlCon.Close();
                    sqlCon.Dispose();
                }
                LogHelper.LogInfo(Log, "Requisition UpdateExtendedStatusforHoldRequisition DAO Method Ended for documentCode=" + documentCode);
            }
            return result;
        }

        public List<NewP2PEntities.RequisitionPartnerEntities> GetPartnerDetailsAndOrderingLocationById(long RequisitionId, int SpendControlType = 0)
        {
            RefCountingDataReader objRefCountingDataReader = null;
            List<NewP2PEntities.RequisitionPartnerEntities> lstpartner = new List<NewP2PEntities.RequisitionPartnerEntities>();
            try
            {
                LogHelper.LogInfo(Log, "Requisition GetPartnerDetailsAndOrderingLocationById Method Started for documentId=" + RequisitionId);

                if (Log.IsWarnEnabled)
                    Log.Warn("In Requisition GetPartnerDetailsAndOrderingLocationById method documentId parameter is less then or equal to 0.");


                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("Requisition GetPartnerDetailsAndOrderingLocationById sp usp_P2P_REQ_GetAllPartnersById with parameter:", " documentId=" + RequisitionId +
                                                         " was called."));


                objRefCountingDataReader =
                 (RefCountingDataReader)
                 ContextSqlConn.ExecuteReader(ReqSqlConstants.USP_P2P_REQ_GETALLPARTNERCODEANDORDERINGLOCATION,
                                                                  new object[] { RequisitionId, SpendControlType });
                if (objRefCountingDataReader != null)
                {
                    var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;

                    while (sqlDr.Read())
                    {
                        var objPartnerEntities = new NewP2PEntities.RequisitionPartnerEntities();
                        objPartnerEntities.PartnerLocations = new List<PartnerLocation>();
                        objPartnerEntities.PartnerCode = GetLongValue(sqlDr, ReqSqlConstants.COL_PARTNER_CODE);
                        objPartnerEntities.LegalCompanyName = GetStringValue(sqlDr, ReqSqlConstants.COL_PARTNER_NAME);
                        objPartnerEntities.DefaultCurrencyCode = GetStringValue(sqlDr, ReqSqlConstants.COL_CURRENCY);
                        objPartnerEntities.PartnerLocations.Add(new PartnerLocation
                        {
                            LocationId = GetLongValue(sqlDr, ReqSqlConstants.COL_LOCATIONID),
                            LocationName = GetStringValue(sqlDr, ReqSqlConstants.OL_COL_LocationName)
                        });

                        objPartnerEntities.SpendDocumentContractNumber = (sqlDr.GetSchemaTable().Select("ColumnName = '" + ReqSqlConstants.COL_SPENDCONTROLDOCUMENTNUMBER + "'").Any())
                            ? ((!string.IsNullOrWhiteSpace(GetStringValue(sqlDr, ReqSqlConstants.COL_SPENDCONTROLDOCUMENTNUMBER))) ? GetStringValue(sqlDr, ReqSqlConstants.COL_SPENDCONTROLDOCUMENTNUMBER) : string.Empty)
                            : string.Empty;
                        lstpartner.Add(objPartnerEntities);
                    }
                    return lstpartner;
                }
            }
            finally
            {
                if (!ReferenceEquals(objRefCountingDataReader, null) && !objRefCountingDataReader.IsClosed)
                {
                    objRefCountingDataReader.Close();
                    objRefCountingDataReader.Dispose();
                }
                LogHelper.LogInfo(Log, "Requisition GetListErrorCodesByOrderIds Method Ended for documentId=" + RequisitionId);
            }

            return new List<NewP2PEntities.RequisitionPartnerEntities>();
        }

    } 
}

