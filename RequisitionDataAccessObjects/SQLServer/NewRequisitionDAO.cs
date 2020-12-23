using Gep.Cumulus.CSM.Entities;
using Gep.Cumulus.CSM.Extensions;
using Gep.Cumulus.ExceptionManager;
using Gep.Cumulus.Partner.Entities;
using GEP.Cumulus.Documents.DataAccessObjects.SQLServer;
using GEP.Cumulus.Documents.Entities;
using GEP.Cumulus.Logging;
using GEP.Cumulus.P2P.BusinessEntities;
using GEP.Cumulus.P2P.Req.DataAccessObjects.SQLServer;
using GEP.Cumulus.QuestionBank.Entities;
using GEP.NewP2PEntities;
using GEP.NewPlatformEntities;
using GEP.SMART.Storage.AzureSQL;
using log4net;
using Microsoft.Practices.EnterpriseLibrary.Data;
using RequisitionEntities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SMARTFaultException = Gep.Cumulus.ExceptionManager;
namespace GEP.Cumulus.P2P.Req.DataAccessObjects
{
    [ExcludeFromCodeCoverage]
    /// <summary>
    /// 2.0 Requisition DAO.
    /// </summary>
    public class NewRequisitionDAO : GEP.Cumulus.P2P.DataAccessObjects.Usability.NewP2PDocumentDAO, INewRequisitionDAO, IDisposable
    {
        #region Private variables
        private const Int32 REQ = 7;
        private const Int32 DRAFTSTATUS = 1;
        private const Int16 LINE = 2;

        private const String REQUISITION = "P2P_Requisition";
        private const String REQ_OBJECT_TYPE = "GEP.Cumulus.P2P.Requisition";
        private const String REQ_LINE_OBJECT_TYPE = "GEP.Cumulus.P2P.Requisition.LineItem";

        private String _Conn = @"metadata=res://*/RequisitionModel.csdl|res://*/RequisitionModel.ssdl|res://*/RequisitionModel.msl;provider=System.Data.SqlClient;provider connection string=""data source={serverName};initial catalog={databaseName};user id={username};password={password};Connection Timeout=180;MultipleActiveResultSets=True;App=EntityFramework""";
        private RequisitionEntities.RequisitionEntities _Context;
        private P2P_Requisition dbReq = null;
        private List<DM_DocumentBU> dbBU = null;
        private DM_DocumentLOBMapping dbLOB = null;
        private List<P2P_SplitAccountingFieldConfigurations> dbSplitAccFieldConfig = null;
        private static readonly ILog Log = Logger.GetLog(MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Private methods
        /// <summary>
        /// Returns new requisition.
        /// </summary>
        /// <returns>New requisition.</returns>
        private void FillFormId(DataRowCollection CustomAttributeFormsIds, ref NewP2PEntities.Requisition req)
        {
            foreach (DataRow Dr in CustomAttributeFormsIds)
            {
                long FormId = 0;
                if (Dr[ReqSqlConstants.COL_LEVEL].ToString() == ((int)GEP.Cumulus.P2P.BusinessEntities.Level.Header).ToString())
                {
                    long.TryParse(Dr[ReqSqlConstants.COL_FORMCODE].ToString(), out FormId);
                    req.CustomAttrFormIdForHeader = FormId;
                }
                else if (Dr[ReqSqlConstants.COL_LEVEL].ToString() == ((int)GEP.Cumulus.P2P.BusinessEntities.Level.Item).ToString())
                {
                    long.TryParse(Dr[ReqSqlConstants.COL_FORMCODE].ToString(), out FormId);
                    req.CustomAttrFormIdForItem = FormId;
                }
                else if (Dr[ReqSqlConstants.COL_LEVEL].ToString() == ((int)GEP.Cumulus.P2P.BusinessEntities.Level.RiskAssessment).ToString())
                {
                    long.TryParse(Dr[ReqSqlConstants.COL_FORMCODE].ToString(), out FormId);
                    req.CustomAttrFormIdForRiskAssessment = FormId;
                }
            }
        }

        /// <summary>
        /// Returns new requisition.
        /// </summary>
        /// <returns>New requisition.</returns>
        private NewP2PEntities.Requisition GetNewRequisitionDisplayDetails()
        {
            return new NewP2PEntities.Requisition
            {
                createdOn = DateTime.Now,
                lastModifiedOn = DateTime.Now,
                items = new List<NewP2PEntities.RequisitionItem>(),
                notes = new List<Note>(),
                number = "",
                createdBy = new IdAndName
                {
                    id = UserContext.ContactCode,
                    name = UserContext.UserName
                },
                lastModifiedBy = new IdAndName
                {
                    id = UserContext.ContactCode,
                    name = UserContext.UserName
                },
                obo = new IdAndName
                {
                    id = UserContext.ContactCode,
                    name = UserContext.UserName
                },
                source = new IdAndName
                {

                },
                status = new IdAndName
                {
                    id = DRAFTSTATUS,
                    name = DRAFT
                },
                type = new IdAndName
                {
                    id = REQ,
                    name = REQUISITION
                },
                RequisitionSource = (byte)RequisitionSource.ManualRequisition,
                RequesterID = UserContext.ContactCode,
                deliverToStr = "",
                OnEvent = OnEvent.None
            };
        }

        /// <summary>
        /// Dispose connection.
        /// </summary>
        private void DisposeConnection()
        {
            if (_Context != null)
            {
                _Context.Dispose();
            }
        }

        /// <summary>
        /// Initializes connection.
        /// </summary>
        /// <param name="connStr">Connection string.</param>
        private void InitializeConnection(String connStr)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connStr);
            _Conn = _Conn.Replace("{serverName}", builder.DataSource).Replace("{databaseName}", builder.InitialCatalog).Replace("{username}", builder.UserID).Replace("{password}", builder.Password);
            _Context = new RequisitionEntities.RequisitionEntities(_Conn);
            _Context.Configuration.AutoDetectChangesEnabled = true;
        }

        /// <summary>
        /// Gets Requisition from DB for updating or returns a new one.
        /// </summary>
        /// <param name="loadEagerly">Flag for eager loading.</param>
        /// <param name="id">Requisition Id.</param>
        private void InitializeDBReq(Boolean loadEagerly, Int64 id)
        {
            if (id > 0)
            {
                if (loadEagerly)
                {
                    dbReq = (from r in _Context.P2P_Requisition
                             .Include(r => r.DM_Documents)
                             .Include(r => r.DM_Documents.DM_DocumentAdditionalFields)
                             .Include(r => r.DM_Documents.DM_DocumentBU)
                             .Include(r => r.DM_Documents.DM_DocumentLOBMapping)
                             .Include(r => r.P2P_RequisitionEntityDetails)
                             .Include(r => r.P2P_RequisitionItems)
                             .Include(r => r.P2P_RequisitionItems.Select(ri => ri.P2P_RequisitionSplitItems))
                             .Include(r => r.P2P_RequisitionItems.Select(ri => ri.P2P_RequisitionTaxes))
                             .Include(r => r.P2P_RequisitionItems.Select(ri => ri.P2P_ReqLineItemShippingDetails))
                             .Include(r => r.P2P_RequisitionItems.Select(ri => ri.P2P_RequisitionSplitItems.Select(rsi => rsi.P2P_RequisitionSplitItemEntities)))
                             where r.RequisitionID == id
                             select r).FirstOrDefault();
                }
                else
                {
                    dbReq = (from r in _Context.P2P_Requisition
                             .Include(r => r.DM_Documents)
                             .Include(r => r.P2P_RequisitionEntityDetails)
                             where r.RequisitionID == id
                             select r).FirstOrDefault();
                }
            }
            else
            {
                dbReq = new P2P_Requisition
                {
                    RequisitionID = -1,
                    RequesterID = UserContext.ContactCode,
                    DM_Documents = new DM_Documents(),
                    P2P_RequisitionEntityDetails = new List<P2P_RequisitionEntityDetails>()
                };
                _Context.Entry(dbReq).State = System.Data.EntityState.Added;
            }

            dbSplitAccFieldConfig = (from c in _Context.P2P_SplitAccountingFieldConfigurations
                                     where c.DocumentType == REQ && c.IsDeleted == false && c.LevelType == LINE
                                     select c).ToList();
        }

        /// <summary>
        /// Updates fields of P2P_Requisition.
        /// </summary>
        /// <param name="req">Requisition object.</param>
        private void UpdateP2P_Requisition(NewP2PEntities.Requisition req)
        {
            dbReq.AdditionalCharges = req.otherCharges;
            dbReq.ItemTotal = req.itemTotal;
            dbReq.RequisitionAmount = req.total;
            dbReq.Shipping = req.shipping;
            dbReq.Tax = req.tax;
            dbReq.BilltoLocationID = req.billTo == null ? 0 : Convert.ToInt32(req.billTo.id);
            dbReq.CurrencyCode = req.currency == null ? null : req.currency.name;
            dbReq.DeliverTo = req.deliverToStr;
            dbReq.IsUrgent = req.isUrgent;
            dbReq.OnBehalfOf = req.obo == null && req.createdBy == null ? 0 : req.obo == null ? req.createdBy.id : req.obo.id;
            dbReq.WorkOrderNo = req.workOrder;
            dbReq.ProgramId = req.programId;
            dbReq.BaseCurrency = req.currency == null ? "USD" : req.currency.name;
            dbReq.CurrencyCode = req.currency == null ? "USD" : req.currency.name;
            dbReq.BudgetId = req.BudgetId;
            if (req.businessUnit == null || req.businessUnit.id <= 0)
                dbReq.BUID = null;
            else
                dbReq.BUID = req.businessUnit.id;

            if (req.deliverTo == null || req.deliverTo.id <= 0)
                dbReq.DelivertoLocationID = null;
            else
                dbReq.DelivertoLocationID = Convert.ToInt32(req.deliverTo.id);

            if (req.shipTo == null || req.shipTo.id <= 0)
                dbReq.ShiptoLocationID = null;
            else
                dbReq.ShiptoLocationID = Convert.ToInt32(req.shipTo.id);

            if (req.erpOrderType == null)
                dbReq.ERPOrderType = null;
            else
                dbReq.ERPOrderType = Convert.ToInt32(req.erpOrderType.id);
            if (req.pOSignatory != null)
                dbReq.POSignatoryCode = req.pOSignatory.id;

            //TODO Rohit:When Purchase Item type is introducted bring it from the Front End
            //Till then Hardcoding with 1
            dbReq.PurchaseType = req.purchaseType;
            dbReq.BudgetoryStatus = req.budgetoryStatus;

            dbReq.RequisitionSource = req.RequisitionSource;
            dbReq.RequisitionTotalChange = req.RequisitionTotalChange;
            dbReq.ParentDocumentCode = req.ParentDocumentCode;
            dbReq.RevisionNumber = req.RevisionNumber == null ? "" : req.RevisionNumber;
        }

        /// <summary>
        /// Updates fields of DM_Documents.
        /// </summary>
        /// <param name="req">Requisition object.</param>
        private void UpdateDM_Documents(NewP2PEntities.Requisition req)
        {
            if (req.id <= 0)
            {
                if (dbReq.DM_Documents == null)
                    dbReq.DM_Documents = new DM_Documents();

                dbReq.DM_Documents.DocumentCode = -1;
                dbReq.DM_Documents.Creator = req.createdBy.id;
                dbReq.DM_Documents.DocumentNumber = req.number;
                dbReq.DM_Documents.DocumentTypeCode = REQ;
                dbReq.DM_Documents.DateCreated = req.createdOn;
                dbReq.DM_Documents.IsConfidential = false;
                dbReq.DM_Documents.IsTemplate = false;
                dbReq.DM_Documents.IsBuyerVisible = true;
                dbReq.DM_Documents.IsDeleted = false;
                dbReq.DM_Documents.SearchKey = String.Empty;
                dbReq.DM_Documents.IsSupplierVisible = true;
                dbReq.DM_Documents.Updated_TimeStamp = Encoding.ASCII.GetBytes(DateTime.Now.ToString("yyyyMMdd"));
                dbReq.DM_Documents.DocumentSourceType = req.source != null ? Convert.ToByte(req.source.id) : Convert.ToByte(1);
                dbReq.DM_Documents.NumberofSurveys = 0;
                dbReq.DM_Documents.NumberofSections = 0;
                dbReq.DM_Documents.NumberofPartners = 0;
                dbReq.DM_Documents.NumberofAttachments = 0;
                dbReq.DM_Documents.NumberofPas = 0;
                dbReq.DM_Documents.NumberofRegion = 0;
                dbReq.DM_Documents.NumberofBU = 0;
                _Context.Entry(dbReq.DM_Documents).State = System.Data.EntityState.Added;
            }
            dbReq.DM_Documents.DateModified = DateTime.UtcNow;
            dbReq.DM_Documents.DocumentName = req.name;
            dbReq.DM_Documents.NumberofItems = req.items == null ? 0 : req.items.Count();
            dbReq.DM_Documents.DocumentStatus = req.status == null ? DRAFTSTATUS : Convert.ToInt32(req.status.id);
        }
        /// <summary>
        /// Updates Document bu.
        /// </summary>
        /// <param name="req"></param>
        private void UpdateDM_DocumentBU(List<NewPlatformEntities.DocumentBU> documentBUs)
        {
            if (documentBUs == null)
                return;

            long reqId = documentBUs.Any() ? documentBUs.FirstOrDefault().documentCode : 0;
            List<DM_DocumentBU> dbBU = (from dbu in dbReq.DM_Documents.DM_DocumentBU where dbu.DocumentCode == reqId select dbu).ToList();
            List<long> clientBU = documentBUs.Select(x => x.buCode).ToList();
            List<long> dBU = dbBU != null ? dbBU.Select(x => x.BUCode).ToList() : new List<long>();

            if (dbBU.Count() > 0)
                foreach (DM_DocumentBU bu in dbBU)
                {
                    if (!clientBU.Contains(bu.BUCode))
                        _Context.Entry(bu).State = System.Data.EntityState.Deleted;
                }

            foreach (NewPlatformEntities.DocumentBU bu in documentBUs)
            {
                if (!dBU.Contains(bu.buCode) && bu.buCode > 0)
                {
                    DM_DocumentBU dbu = new DM_DocumentBU
                    {
                        DocumentCode = reqId <= 0 ? -1 : reqId,
                        BUCode = bu.buCode,
                        Updated_TimeStamp = Encoding.ASCII.GetBytes(DateTime.Now.ToString("yyyyMMdd"))
                    };
                    _Context.Entry(dbu).State = System.Data.EntityState.Added;
                }
            }
        }

        /// <summary>
        /// update Document LOB details
        /// </summary>
        /// <param name="req"></param>
        private void UpdateDM_DocumentLOB(NewP2PEntities.Requisition req)
        {
            if (req.id <= 0)
            {
                DM_DocumentLOBMapping dbLOB = new DM_DocumentLOBMapping
                {
                    DocumentCode = -1,
                    EntityDetailCode = req.documentLOB.entityDetailCode,
                    EntityId = (int)req.documentLOB.entityId,
                    Updated_TimeStamp = Encoding.ASCII.GetBytes(DateTime.Now.ToString("yyyyMMdd"))
                };
                _Context.Entry(dbLOB).State = System.Data.EntityState.Added;
            }
            else
            {
                DM_DocumentLOBMapping dbLOB = (from lob in dbReq.DM_Documents.DM_DocumentLOBMapping where lob.DocumentCode == req.id select lob).FirstOrDefault();
                if (dbLOB != null)
                {
                    dbLOB.DocumentCode = req.id;
                    dbLOB.EntityDetailCode = req.documentLOB.entityDetailCode;
                    dbLOB.EntityId = (int)req.documentLOB.entityId;
                    dbLOB.Updated_TimeStamp = Encoding.ASCII.GetBytes(DateTime.Now.ToString("yyyyMMdd"));
                }
            }
        }

        /// <summary>
        /// Updates document additional fields.
        /// </summary>
        /// <param name="req">Requisition.</param>
        private void UpdateDM_DocumentAdditionalFields(NewP2PEntities.Requisition req)
        {
            List<NewP2PEntities.RequisitionItem> items = null;
            if (req.items != null && req.items.Count > 0)
                items = req.items.Where(x => x.isDeleted == false).ToList();

            if (dbReq.DM_Documents.DM_DocumentAdditionalFields == null)
                dbReq.DM_Documents.DM_DocumentAdditionalFields = new List<DM_DocumentAdditionalFields>();

            DM_DocumentAdditionalFields isUrgent = (from daf in dbReq.DM_Documents.DM_DocumentAdditionalFields where daf.DocumentCode == req.id && daf.FieldName == "IsUrgent" select daf).FirstOrDefault();
            if (isUrgent == null)
            {
                isUrgent = new DM_DocumentAdditionalFields
                {
                    DocumentCode = req.id == 0 ? -1 : req.id,
                    FieldName = "IsUrgent",
                    FieldType = 1
                };

                dbReq.DM_Documents.DM_DocumentAdditionalFields.Add(isUrgent);
                _Context.Entry(isUrgent).State = System.Data.EntityState.Added;
            }

            isUrgent.FieldValue = req.isUrgent ? "1" : "0";
            DM_DocumentAdditionalFields itemCount = (from daf in dbReq.DM_Documents.DM_DocumentAdditionalFields where daf.DocumentCode == req.id && daf.FieldName == "ItemCount" select daf).FirstOrDefault();
            if (itemCount == null)
            {
                itemCount = new DM_DocumentAdditionalFields
                {
                    DocumentCode = req.id == 0 ? -1 : req.id,
                    FieldName = "ItemCount",
                    FieldType = 1
                };

                dbReq.DM_Documents.DM_DocumentAdditionalFields.Add(itemCount);
                _Context.Entry(itemCount).State = System.Data.EntityState.Added;
            }

            itemCount.FieldValue = items == null ? "0" : items.Count.ToString();
            Int16 nonPCount = 0;
            if (items != null && items.Count > 0)
                foreach (NewP2PEntities.RequisitionItem i in items)
                    if (i.isProcurable != null && i.isProcurable.id == 1)  // 0 - Procurable, 1- From Inventory
                        nonPCount++;

            DM_DocumentAdditionalFields nonProcurableCount = (from daf in dbReq.DM_Documents.DM_DocumentAdditionalFields where daf.DocumentCode == req.id && daf.FieldName == "NonProcurableCount" select daf).FirstOrDefault();
            if (nonProcurableCount == null)
            {
                nonProcurableCount = new DM_DocumentAdditionalFields
                {
                    DocumentCode = req.id == 0 ? -1 : req.id,
                    FieldName = "NonProcurableCount",
                    FieldType = 1
                };

                dbReq.DM_Documents.DM_DocumentAdditionalFields.Add(nonProcurableCount);
                _Context.Entry(nonProcurableCount).State = System.Data.EntityState.Added;
            }

            nonProcurableCount.FieldValue = nonPCount.ToString();
            DM_DocumentAdditionalFields reqType = (from daf in dbReq.DM_Documents.DM_DocumentAdditionalFields where daf.DocumentCode == req.id && daf.FieldName == "ReqType" select daf).FirstOrDefault();
            if (reqType == null)
            {
                reqType = new DM_DocumentAdditionalFields
                {
                    DocumentCode = req.id == 0 ? -1 : req.id,
                    FieldName = "ReqType",
                    FieldType = 1
                };

                dbReq.DM_Documents.DM_DocumentAdditionalFields.Add(reqType);
                _Context.Entry(reqType).State = System.Data.EntityState.Added;
            }

            reqType.FieldValue = req.source == null || req.source.id <= 0 ? "1" : req.source.id.ToString();
            DM_DocumentAdditionalFields totalValue = (from daf in dbReq.DM_Documents.DM_DocumentAdditionalFields where daf.DocumentCode == req.id && daf.FieldName == "TotalValue" select daf).FirstOrDefault();
            if (totalValue == null)
            {
                totalValue = new DM_DocumentAdditionalFields
                {
                    DocumentCode = req.id == 0 ? -1 : req.id,
                    FieldName = "TotalValue",
                    FieldType = 1
                };

                dbReq.DM_Documents.DM_DocumentAdditionalFields.Add(totalValue);
                _Context.Entry(totalValue).State = System.Data.EntityState.Added;
            }

            double? totalValueDoc = 0;
            if (items != null && items.Count > 0)
                foreach (NewP2PEntities.RequisitionItem i in items)
                    totalValueDoc = totalValueDoc + ((i.unitPrice == null ? 0 : i.unitPrice) * i.quantity) + (i.taxes == null ? 0 : i.taxes) + (i.shippingCharges == null ? 0 : i.shippingCharges) + (i.otherCharges == null ? 0 : i.otherCharges);
            totalValue.FieldValue = totalValueDoc == null ? "0" : "" + totalValueDoc;

            DM_DocumentAdditionalFields totalValueCurrencyCode = (from daf in dbReq.DM_Documents.DM_DocumentAdditionalFields where daf.DocumentCode == req.id && daf.FieldName == "TotalValueCurrencyCode" select daf).FirstOrDefault();
            if (totalValueCurrencyCode == null)
            {
                totalValueCurrencyCode = new DM_DocumentAdditionalFields
                {
                    DocumentCode = req.id == 0 ? -1 : req.id,
                    FieldName = "TotalValueCurrencyCode",
                    FieldType = 3
                };

                dbReq.DM_Documents.DM_DocumentAdditionalFields.Add(totalValueCurrencyCode);
                _Context.Entry(totalValueCurrencyCode).State = System.Data.EntityState.Added;
            }
            totalValueCurrencyCode.FieldValue = req.currency == null ? "USD" : req.currency.name;
        }

        /// <summary>
        /// Updates fields of P2P_RequisitionEntityDetails.
        /// </summary>
        /// <param name="req">Requisition object.</param>
        private void UpdateP2P_RequisitionEntityDetails(NewP2PEntities.Requisition req)
        {
            // need to have DocumentCode before saving the data.
            if (req.HeaderSplitAccountingFields.Any())
            {
                // header entities present in DB and not in in REQ object.
                List<P2P_RequisitionEntityDetails> dbReqEntToBeDeleted = (from e in dbReq.P2P_RequisitionEntityDetails
                                                                          where e.RequisitionID == req.documentCode && !req.HeaderSplitAccountingFields.Any(p => p.EntityTypeId == e.EntityId)
                                                                          select e).ToList();

                foreach (P2P_RequisitionEntityDetails split in dbReqEntToBeDeleted)
                    _Context.Entry(split).State = System.Data.EntityState.Deleted;

                foreach (var headersplitField in req.HeaderSplitAccountingFields)
                {
                    P2P_RequisitionEntityDetails dbReqEnt = (from e in dbReq.P2P_RequisitionEntityDetails
                                                             where e.RequisitionID == req.documentCode && e.EntityId == headersplitField.EntityTypeId
                                                             select e).FirstOrDefault();
                    if (dbReqEnt == null)
                    {
                        dbReqEnt = new P2P_RequisitionEntityDetails
                        {
                            EntityDetailCode = headersplitField.EntityDetailCode,
                            EntityId = headersplitField.EntityTypeId,
                            RequisitionID = req.id
                        };
                        dbReq.P2P_RequisitionEntityDetails.Add(dbReqEnt);
                        _Context.Entry(dbReqEnt).State = System.Data.EntityState.Added;
                    }
                    else
                        dbReqEnt.EntityDetailCode = headersplitField.EntityDetailCode;
                }
            }
        }

        /// <summary>
        /// Saves header of a requisition.
        /// </summary>
        /// <param name="req">Requisition with filled header</param>
        /// <returns>Outcome of save</returns>
        private Boolean SaveReqHeader(NewP2PEntities.Requisition req, List<NewPlatformEntities.DocumentBU> documentBUs)
        {
            if (req == null || dbReq == null || dbReq.DM_Documents == null || dbReq.P2P_RequisitionEntityDetails == null)
                return false;

            UpdateP2P_Requisition(req);
            UpdateDM_Documents(req);
            UpdateDM_DocumentBU(documentBUs);
            UpdateDM_DocumentLOB(req);
            UpdateP2P_RequisitionEntityDetails(req);
            UpdateDM_DocumentAdditionalFields(req);
            return true;
        }


        /// <summary>
        /// Fills requisition header information.
        /// </summary>
        /// <param name="dr">Data row.</param>
        /// <param name="req">Requisition reference.</param>
        private void FillRequisitionHeader(DataRow dr, DataRowCollection drcBU, DataRow drLOB, ref NewP2PEntities.Requisition req)
        {
            String billToEmail = ConvertToString(dr, ReqSqlConstants.COL_BILLTOEMAIL);
            String billToFax = ConvertToString(dr, ReqSqlConstants.COL_BILLTOFAX);
            String billToContact = String.Empty;
            if (String.IsNullOrEmpty(billToEmail))
                billToContact = billToFax;
            else if (String.IsNullOrEmpty(billToFax))
                billToContact = billToEmail;
            else
                billToContact = billToEmail + SPACE + SLASH + SPACE + billToFax;

            req.billTo = new IdNameAddressAndContact
            {
                id = ConvertToInt64(dr, ReqSqlConstants.COL_BILLTOID),
                name = BuildLocation("", ConvertToString(dr, ReqSqlConstants.COL_BILLTOLOC_NAME)),
                contact = billToContact,
                address = BuildAddress(ConvertToString(dr, ReqSqlConstants.COL_BILLTOADDRESS1), ConvertToString(dr, ReqSqlConstants.COL_BILLTOADDRESS2),
                    ConvertToString(dr, ReqSqlConstants.COL_BILLTOADDRESS3), ConvertToString(dr, ReqSqlConstants.BILLTO_COL_CITY),
                    ConvertToString(dr, ReqSqlConstants.BILLTO_COL_STATE), ConvertToString(dr, ReqSqlConstants.COL_BILLTOCOUNTRY),
                    ConvertToString(dr, ReqSqlConstants.BILLTO_COL_ZIP)),
                number = BuildLocation("", ConvertToString(dr, ReqSqlConstants.COL_BILLTOLOC_NUMBER))
            };
            req.businessUnit = new IdAndName
            {
                id = ConvertToInt64(dr, ReqSqlConstants.COL_BUSINESSUNITID),
                name = ConvertToString(dr, ReqSqlConstants.COL_BUSINESSUNITNAME)
            };
            req.createdBy = new IdAndName
            {
                id = ConvertToInt64(dr, ReqSqlConstants.COL_CREATEDBYID),
                name = BuildName(ConvertToString(dr, ReqSqlConstants.COL_CREATEDBYFNAME), ConvertToString(dr, ReqSqlConstants.COL_CREATEDBYLNAME))
            };
            req.CreaterClientContactCode = ConvertToString(dr, ReqSqlConstants.COL_CREATERCLIENTCONTACTCODE);
            req.CreaterEmailAddress = ConvertToString(dr, ReqSqlConstants.COL_CREATEREMAILADDRESS);
            req.createdOn = ConvertToDateTime(dr, ReqSqlConstants.COL_CREATED_ON);
            req.currency = new CodeAndName
            {
                code = ConvertToString(dr, ReqSqlConstants.COL_CURRENCYSYMBOL),
                name = ConvertToString(dr, ReqSqlConstants.COL_CURRENCY)
            };
            req.BaseCurrency = dr.Table.Columns.Contains(ReqSqlConstants.COL_BASE_CURRENCYCODE) ? ConvertToString(dr, ReqSqlConstants.COL_BASE_CURRENCYCODE) : ConvertToString(dr, ReqSqlConstants.COL_CURRENCY);
            req.deliverTo = new IdNameAndAddress
            {
                id = ConvertToInt64(dr, ReqSqlConstants.COL_DELIVERTOID),
                name = BuildLocation("", ConvertToString(dr, ReqSqlConstants.COL_DELIVERTOLOC_NAME)),
                address = BuildAddress(ConvertToString(dr, ReqSqlConstants.COL_DELTOADDRESS1), ConvertToString(dr, ReqSqlConstants.COL_DELTOADDRESS2),
                ConvertToString(dr, ReqSqlConstants.COL_DELTOADDRESS3), ConvertToString(dr, ReqSqlConstants.DEL_COL_CITY),
                ConvertToString(dr, ReqSqlConstants.DEL_COL_STATE), ConvertToString(dr, ReqSqlConstants.DEL_COL_COUNTRY),
                ConvertToString(dr, ReqSqlConstants.DEL_COL_ZIP)),
                number = BuildLocation("", ConvertToString(dr, ReqSqlConstants.COL_DELIVERTOLOC_NUMBER))
            };
            req.deliverToStr = ConvertToString(dr, ReqSqlConstants.COL_DELIVERTOSTR);
            req.documentCode = ConvertToInt64(dr, ReqSqlConstants.COL_DOCUMENTCODE);
            Int64 erp = ConvertToInt64(dr, ReqSqlConstants.COL_ERPORDERTYPEID);
            req.erpOrderType = new IdAndName
            {
                id = erp,
                name = ConvertToString(dr, ReqSqlConstants.COL_ERPORDERTYPE) ?? String.Empty
            };
            req.id = ConvertToInt64(dr, ReqSqlConstants.COL_ID);
            req.isUrgent = ConvertToBoolean(dr, ReqSqlConstants.COL_ISURGENT);
            req.lastModifiedOn = ConvertToNullableDateTime(dr, ReqSqlConstants.COL_LASTMODIFIEDON);
            req.name = ConvertToString(dr, ReqSqlConstants.COL_NAME);
            req.number = ConvertToString(dr, ReqSqlConstants.COL_NUMBER);
            req.obo = new IdAndName
            {
                id = ConvertToInt64(dr, ReqSqlConstants.COL_OBOID),
                name = BuildName(ConvertToString(dr, ReqSqlConstants.COL_OBO_FNAME), ConvertToString(dr, ReqSqlConstants.COL_OBO_LNAME))
            };
            req.OBOClientContactCode = ConvertToString(dr, ReqSqlConstants.COL_OBOCLIENTCONTACTCODE);
            req.shipTo = new IdNameAndAddress
            {
                id = ConvertToInt64(dr, ReqSqlConstants.COL_SHIPTOID),
                name = BuildLocation("", ConvertToString(dr, ReqSqlConstants.COL_SHIPTOLOC_NAME)),
                nameandnumber = ConvertToString(dr, ReqSqlConstants.COL_SHIPTOLOC_NUMBER) + "-" + ConvertToString(dr, ReqSqlConstants.COL_SHIPTOLOC_NAME),
                address = String.IsNullOrEmpty(ConvertToString(dr, ReqSqlConstants.COL_LATITUDE)) ? BuildAddress(ConvertToString(dr, ReqSqlConstants.COL_SHIPTOADDRESS1), ConvertToString(dr, ReqSqlConstants.COL_SHIPTOADDRESS2),
                    ConvertToString(dr, ReqSqlConstants.COL_SHIPTOADDRESS3), ConvertToString(dr, ReqSqlConstants.COL_SHIPTOCITY),
                    ConvertToString(dr, ReqSqlConstants.COL_SHIPTOSTATE), ConvertToString(dr, ReqSqlConstants.COL_SHIPTOCOUNTRY),
                    ConvertToString(dr, ReqSqlConstants.COL_SHIPTOZIP)) : ConvertToString(dr, ReqSqlConstants.COL_LATITUDE) + ", " + ConvertToString(dr, ReqSqlConstants.COL_LONGITUDE)

            };
            req.ShiptoLocationDetails = new LocationDetails
            {
                id = ConvertToInt64(dr, ReqSqlConstants.COL_SHIPTOID),
                LocationName = BuildLocation("", ConvertToString(dr, ReqSqlConstants.COL_SHIPTOLOC_NAME)),
                LocationNumber = ConvertToString(dr, ReqSqlConstants.COL_SHIPTOLOC_NUMBER),
                AddressLine1 = String.IsNullOrEmpty(ConvertToString(dr, ReqSqlConstants.COL_LATITUDE)) ? ConvertToString(dr, ReqSqlConstants.COL_SHIPTOADDRESS1) : ConvertToString(dr, ReqSqlConstants.COL_LATITUDE) + ", " + ConvertToString(dr, ReqSqlConstants.COL_LONGITUDE),
                AddressLine2 = ConvertToString(dr, ReqSqlConstants.COL_SHIPTOADDRESS2),
                AddressLine3 = ConvertToString(dr, ReqSqlConstants.COL_SHIPTOADDRESS3),
                City = ConvertToString(dr, ReqSqlConstants.COL_SHIPTOCITY),
                StateCode = ConvertToString(dr, ReqSqlConstants.COL_SHIPTOSTATE),
                Country = ConvertToString(dr, ReqSqlConstants.COL_SHIPTOCOUNTRY),
                CountryCode = ConvertToString(dr, ReqSqlConstants.COL_SHIPTOCOUNTRYCODE),
                Zip = ConvertToString(dr, ReqSqlConstants.COL_SHIPTOZIP)
            };
            req.buyerAssigneeName = new IdAndName
            {
                id = ConvertToInt64(dr, ReqSqlConstants.COL_BUYERASSIGNEE),
                name = ConvertToString(dr, ReqSqlConstants.COL_BUYERASSIGNEENAME)
            };
            Int64 sourceId = ConvertToInt64(dr, ReqSqlConstants.COL_SOURCEID);
            req.source = new IdAndName
            {
                id = sourceId,
                name = sourceId == 1 ? MANUAL : (sourceId == 2 ? CATALOG : NONE)
            };
            Int64 statusId = ConvertToInt64(dr, ReqSqlConstants.COL_OB_STATUSID);
            req.status = new IdAndName
            {
                id = statusId,
                name = GetStatusName(statusId)
            };
            req.type = new IdAndName
            {
                id = REQ,
                name = REQUISITION
            };
            req.workOrder = ConvertToString(dr, ReqSqlConstants.COL_WORKORDER);
            req.BudgetId = ConvertToInt16(dr, ReqSqlConstants.COL_BUDGETID);

            if (drcBU.Count > 0)
            {
                req.documentBU = new List<NewPlatformEntities.DocumentBU>();
                foreach (DataRow drc in drcBU)
                {
                    NewPlatformEntities.DocumentBU bu = new NewPlatformEntities.DocumentBU
                    {
                        buCode = ConvertToInt64(drc, ReqSqlConstants.COL_BUCODE)
                    };
                    req.documentBU.Add(bu);
                }
            }

            req.documentLOB = new DocumentLOB
            {
                entityDetailCode = ConvertToInt64(drLOB, ReqSqlConstants.COL_ENTITYDETAILCODE),
                entityId = ConvertToInt64(drLOB, ReqSqlConstants.COL_EntityId),
                entityCode = String.IsNullOrEmpty(ConvertToString(drLOB, ReqSqlConstants.COL_ENTITY_CODE)) ? String.Empty : ConvertToString(drLOB, ReqSqlConstants.COL_ENTITY_CODE),
                entityDisplayName = String.IsNullOrEmpty(ConvertToString(drLOB, ReqSqlConstants.COL_ENTITYNAME)) ? String.Empty : ConvertToString(drLOB, ReqSqlConstants.COL_ENTITYNAME)

            };

            req.purchaseType = ConvertToByte(dr, ReqSqlConstants.COL_PURCHASETYPE);
            req.purchaseTypeDesc = ConvertToString(dr, ReqSqlConstants.COL_PURCHASETYPEDESC);

            //for posignatorycode
            req.pOSignatory = new IdAndName
            {
                id = ConvertToInt64(dr, ReqSqlConstants.COL_POSIGNATORYCODE),
                name = ConvertToString(dr, ReqSqlConstants.COL_POSIGNATORYNAME) ?? String.Empty

            };

            req.sourceSystem = new IdAndName
            {
                id = ConvertToInt64(dr, ReqSqlConstants.COL_SOURCESYSTEMID),
                name = ConvertToString(dr, ReqSqlConstants.COL_SOURCESYSTEMNAME)
            };
            req.Contract = new CodeAndName
            {
                code = ConvertToString(dr, ReqSqlConstants.COL_CONTRACTNUMBER),
                name = ConvertToString(dr, ReqSqlConstants.COL_CONTRACTNAME)
            };
            req.EnforceLineReference = ConvertToBoolean(dr, ReqSqlConstants.COL_ISENFORCELINEREFERENCE);
            req.ContractExpiryDate = ConvertToNullableDateTime(dr, ReqSqlConstants.COL_CONTRACTENDDATE);
            req.ContractValue = ConvertToNullableDouble(dr, ReqSqlConstants.COL_CONTRACTVALUE);
            req.OnEvent = OnEvent.None;
            req.budgetoryStatus = ConvertToInt16(dr, ReqSqlConstants.COL_BUDGETORYSTATUS);
            req.RequisitionSource = ConvertToByte(dr, ReqSqlConstants.COL_REQUISITION_SOURCE);
            req.RequisitionTotalChange = ConvertToDecimal(dr, ReqSqlConstants.COL_REQUISITIONTOTALCHANGE);
            req.ParentDocumentCode = ConvertToInt64(dr, ReqSqlConstants.COL_PARENTDOCUMENTCODE);
            req.RevisionNumber = ConvertToString(dr, ReqSqlConstants.COL_REVISIONNUMBER);
            req.ChangeRequisitionDocumentCode = ConvertToInt64(dr, ReqSqlConstants.COL_CHANGEREQUISITIONDOCUMENTCODE);
            req.total = ConvertToDecimal(dr, ReqSqlConstants.COL_REQUISITION_AMOUNT);
            req.RequesterID = ConvertToInt64(dr, ReqSqlConstants.COL_REQUESTERID);
            req.IsStockRequisition = ConvertToBoolean(dr, ReqSqlConstants.COL_ISSTOCKREQUISITION);
            req.RiskScore = !Convert.IsDBNull(dr[ReqSqlConstants.COL_RISKSCORE]) ? Convert.ToDecimal(dr[ReqSqlConstants.COL_RISKSCORE], CultureInfo.InvariantCulture) : ((decimal?)null);
            req.RiskFormCategory = ConvertToString(dr, ReqSqlConstants.COL_RISKCATEGORY);
            req.RequisitionPreviousAmount = ConvertToDecimal(dr, ReqSqlConstants.COL_REQUISITIONPREVIOUSAMOUNT);
            req.EnableRiskForm = ConvertToBoolean(dr, ReqSqlConstants.COL_ENABLERISKFORM);
            req.ProcurementProfileId = ConvertToInt64(dr, ReqSqlConstants.COL_ProcurementProfileId);
            req.TaxJurisdiction = ConvertToString(dr, ReqSqlConstants.COL_TAXJURISDICTION);
            req.isAdhocShipToLocation = ConvertToBoolean(dr, ReqSqlConstants.COL_SHIPTOLOC_ISADHOC);
            req.IsAdvanceRequsition = ConvertToBoolean(dr, ReqSqlConstants.COL_ISADVANCEREQUISITION);
            req.ExtendedStatus = new IdAndName
            {
                id = ConvertToInt32(dr, ReqSqlConstants.COL_ExtendedStatus),
                name = "",
            };
            req.ERPRequisitionNumber = ConvertToString(dr, ReqSqlConstants.COL_ERPRequisitionNumber);
        }

        /// <summary>
        /// Returns requisition items.
        /// </summary>
        /// <param name="rows">Data rows.</param>
        /// <returns>Requisition items.</returns>
        private List<NewP2PEntities.RequisitionItem> GetRequisitionItems(DataRowCollection rows)
        {
            List<NewP2PEntities.RequisitionItem> items = new List<NewP2PEntities.RequisitionItem>();
            foreach (DataRow dr in rows)
            {
                NewP2PEntities.RequisitionItem item = new NewP2PEntities.RequisitionItem();
                item.buyerItemNumber = ConvertToString(dr, ReqSqlConstants.COL_BUYERITEMNUMBER);
                item.catalogItemId = ConvertToNullableInt64(dr, ReqSqlConstants.COL_CATALOGITEMID);
                item.category = new IdAndName
                {
                    id = ConvertToInt64(dr, ReqSqlConstants.COL_CATEGORY_ID),
                    name = ConvertToString(dr, ReqSqlConstants.COL_CATEGORY_NAME)
                };
                item.ClientCategoryId = ConvertToString(dr, ReqSqlConstants.COL_CLIENTCATEGORYID);
                item.Unspsc = ConvertToString(dr, ReqSqlConstants.COL_UNSPSCKEY);
                item.contractExpiryDate = ConvertToNullableDateTime(dr, ReqSqlConstants.COL_CONTRACTENDDATE);
                item.contractNumber = ConvertToString(dr, ReqSqlConstants.COL_CONTRACTNUMBER);
                item.contractValue = ConvertToNullableDouble(dr, ReqSqlConstants.COL_CONTRACTVALUE);
                item.contractName = ConvertToString(dr, ReqSqlConstants.COL_CONTRACTNAME);
                item.ContractStatus = ConvertToInt32(dr, ReqSqlConstants.COL_CONTRACTSTATUS);
                item.createdBy = new IdAndName
                {
                    id = ConvertToInt64(dr, ReqSqlConstants.COL_CREATEDBYID),
                    name = BuildName(ConvertToString(dr, ReqSqlConstants.COL_CREATEDBYFNAME), ConvertToString(dr, ReqSqlConstants.COL_CREATEDBYLNAME))
                };
                item.createdOn = ConvertToDateTime(dr, ReqSqlConstants.COL_CREATED_ON);
                item.deliverTo = new IdNameAndAddress
                {
                    id = ConvertToInt64(dr, ReqSqlConstants.COL_DELIVERTOLOCATION_ID),
                    name = BuildLocation(ConvertToString(dr, ReqSqlConstants.COL_DELIVERTOLOC_NUMBER), ConvertToString(dr, ReqSqlConstants.COL_DELIVERTOLOC_NAME)),
                    address = BuildAddress(ConvertToString(dr, ReqSqlConstants.COL_DELTOADDRESS1), ConvertToString(dr, ReqSqlConstants.COL_DELTOADDRESS2),
                        ConvertToString(dr, ReqSqlConstants.COL_DELTOADDRESS3), ConvertToString(dr, ReqSqlConstants.DEL_COL_CITY),
                        ConvertToString(dr, ReqSqlConstants.DEL_COL_STATE), ConvertToString(dr, ReqSqlConstants.DEL_COL_COUNTRY),
                        ConvertToString(dr, ReqSqlConstants.DEL_COL_ZIP)),
                    number = BuildLocation("", ConvertToString(dr, ReqSqlConstants.COL_DELIVERTOLOC_NUMBER))
                };
                item.deliverToStr = ConvertToString(dr, ReqSqlConstants.COL_DELIVERTOSTR);
                item.description = ConvertToString(dr, ReqSqlConstants.COL_DESCRIPTION);
                item.documentCode = ConvertToInt64(dr, ReqSqlConstants.COL_DOCUMENT_CODE);
                item.endDate = ConvertToNullableDateTime(dr, ReqSqlConstants.COL_DYNAMICDISCOUNT_ENDDATE);
                item.id = dr.Table.Columns.Contains(ReqSqlConstants.COL_ID) ? ConvertToInt64(dr, ReqSqlConstants.COL_ID) : dr.Table.Columns.Contains(ReqSqlConstants.COL_REQUISITION_ITEM_ID) ? ConvertToInt64(dr, ReqSqlConstants.COL_REQUISITION_ITEM_ID) : 0;
                item.inventoryType = ConvertToNullableBoolean(dr, ReqSqlConstants.COL_INVENTORYTYPE);
                Int64 procurable = ConvertToInt64(dr, ReqSqlConstants.COL_IS_PROCURABLE);
                item.isProcurable = new IdAndName
                {
                    id = procurable,
                    name = procurable > 0 ? INVENTORY : PROCURABLE
                };
                item.isTaxExempt = ConvertToBoolean(dr, ReqSqlConstants.COL_ISTAXEXEMPT);
                item.lastModifiedBy = new IdAndName
                {
                    id = dr.Table.Columns.Contains(ReqSqlConstants.COL_LASTMODIFIEDBYID) ? ConvertToInt64(dr, ReqSqlConstants.COL_LASTMODIFIEDBYID) : 0,
                    name = BuildName(dr.Table.Columns.Contains(ReqSqlConstants.COL_LASTMODIFIEDBYFNAME) ? ConvertToString(dr, ReqSqlConstants.COL_LASTMODIFIEDBYFNAME) : string.Empty, dr.Table.Columns.Contains(ReqSqlConstants.COL_LASTMODIFIEDBYLNAME) ? ConvertToString(dr, ReqSqlConstants.COL_LASTMODIFIEDBYLNAME) : string.Empty)
                };
                item.lastModifiedOn = ConvertToDateTime(dr, ReqSqlConstants.COL_LASTMODIFIEDON);
                item.lineNumber = ConvertToInt64(dr, ReqSqlConstants.COL_LINENUMBER);
                item.lineReferenceNumber = ConvertToInt64(dr, ReqSqlConstants.COL_LINENUMBER);
                item.manufacturer = ConvertToString(dr, ReqSqlConstants.COL_MANUFACTURER);
                item.manufacturerPartNumber = ConvertToString(dr, ReqSqlConstants.COL_MANUFACTURER_PART_NUMBER);
                item.ManufacturerModel = ConvertToString(dr, ReqSqlConstants.COL_MANUFACTURER_MODEL);
                item.name = ConvertToString(dr, ReqSqlConstants.COL_NAME);
                item.needByDate = ConvertToNullableDateTime(dr, ReqSqlConstants.COL_NEEDBYDATE);
                item.otherCharges = ConvertToNullableDouble(dr, ReqSqlConstants.COL_OTHERCHARGES);
                item.p2PLineItemId = ConvertToInt64(dr, ReqSqlConstants.COL_P2P_LINE_ITEM_ID);
                item.clientPartnerCode = dr.Table.Columns.Contains(ReqSqlConstants.COL_CLIENT_PARTNERCODE) ? ConvertToString(dr, ReqSqlConstants.COL_CLIENT_PARTNERCODE) : string.Empty;
                item.partner = new IdAndName
                {
                    id = dr.Table.Columns.Contains(ReqSqlConstants.COL_PARTNER_CODE) ? ConvertToInt64(dr, ReqSqlConstants.COL_PARTNER_CODE) : 0,
                    name = dr.Table.Columns.Contains(ReqSqlConstants.COL_PARTNER_NAME) ? ConvertToString(dr, ReqSqlConstants.COL_PARTNER_NAME) : string.Empty
                };
                item.IsDefaultPartner = ConvertToBoolean(dr, ReqSqlConstants.COL_ISDEFAULT);
                item.partnerCode = dr.Table.Columns.Contains(ReqSqlConstants.COL_PARTNERINTERFACECODE) ? ConvertToString(dr, ReqSqlConstants.COL_PARTNERINTERFACECODE) : string.Empty;
                item.partnerItemNumber = ConvertToString(dr, ReqSqlConstants.COL_PARTNERITEMNUMBER);
                item.quantity = ConvertToDouble(dr, ReqSqlConstants.COL_QUANTITY);
                item.requestedDate = ConvertToNullableDateTime(dr, ReqSqlConstants.COL_REQUESTEDDATE);
                item.shippingCharges = ConvertToNullableDouble(dr, ReqSqlConstants.COL_SHIPPING_CHARGES);
                item.shippingMethod = ConvertToString(dr, ReqSqlConstants.COL_SHIPPINGMETHOD);
                item.shipTo = new IdNameAndAddress
                {
                    id = ConvertToInt64(dr, ReqSqlConstants.COL_SHIPTOLOC_ID),
                    name = BuildLocation("", ConvertToString(dr, ReqSqlConstants.COL_SHIPTOLOC_NAME)),
                    nameandnumber = ConvertToString(dr, ReqSqlConstants.COL_SHIPTOLOC_NUMBER) + "-" + ConvertToString(dr, ReqSqlConstants.COL_SHIPTOLOC_NAME),
                    address = String.IsNullOrEmpty(ConvertToString(dr, ReqSqlConstants.COL_LATITUDE)) ? BuildAddress(ConvertToString(dr, ReqSqlConstants.COL_SHIPTOADDRESS1), ConvertToString(dr, ReqSqlConstants.COL_SHIPTOADDRESS2),
                        ConvertToString(dr, ReqSqlConstants.COL_SHIPTOADDRESS3), ConvertToString(dr, ReqSqlConstants.COL_SHIPTOCITY),
                        ConvertToString(dr, ReqSqlConstants.COL_SHIPTOSTATE), ConvertToString(dr, ReqSqlConstants.COL_SHIPTOCOUNTRY),
                        ConvertToString(dr, ReqSqlConstants.COL_SHIPTOZIP)) : ConvertToString(dr, ReqSqlConstants.COL_LATITUDE) + ", " + ConvertToString(dr, ReqSqlConstants.COL_LONGITUDE)
                };
                item.ShiptoLocationDetails = new LocationDetails
                {
                    id = ConvertToInt64(dr, ReqSqlConstants.COL_SHIPTOLOC_ID),
                    LocationName = BuildLocation("", ConvertToString(dr, ReqSqlConstants.COL_SHIPTOLOC_NAME)),
                    LocationNumber = ConvertToString(dr, ReqSqlConstants.COL_SHIPTOLOC_NUMBER),
                    AddressLine1 = String.IsNullOrEmpty(ConvertToString(dr, ReqSqlConstants.COL_LATITUDE)) ? ConvertToString(dr, ReqSqlConstants.COL_SHIPTOADDRESS1) : ConvertToString(dr, ReqSqlConstants.COL_LATITUDE) + ", " + ConvertToString(dr, ReqSqlConstants.COL_LONGITUDE),
                    AddressLine2 = ConvertToString(dr, ReqSqlConstants.COL_SHIPTOADDRESS2),
                    AddressLine3 = ConvertToString(dr, ReqSqlConstants.COL_SHIPTOADDRESS3),
                    City = ConvertToString(dr, ReqSqlConstants.COL_SHIPTOCITY),
                    StateCode = ConvertToString(dr, ReqSqlConstants.COL_SHIPTOSTATE),
                    Country = ConvertToString(dr, ReqSqlConstants.COL_SHIPTOCOUNTRY),
                    CountryCode = ConvertToString(dr, ReqSqlConstants.COL_SHIPTOCOUNTRYCODE),
                    Zip = ConvertToString(dr, ReqSqlConstants.COL_SHIPTOZIP)
                };
                Int64 src = ConvertToInt64(dr, ReqSqlConstants.COL_SOURCEID);
                item.source = new IdAndName
                {
                    id = src,
                    name = BindItemSource(src)
                };
                item.splitType = ConvertToInt16(dr, ReqSqlConstants.COL_SPLIT_TYPE);
                item.startDate = ConvertToNullableDateTime(dr, ReqSqlConstants.COL_START_DATE);
                item.status = ConvertToByte(dr, ReqSqlConstants.COL_ITEM_STATUS);
                item.supplierPartAuxiliaryId = ConvertToString(dr, ReqSqlConstants.COL_SUPPLIERAUXILIARYPARTID);
                item.taxes = ConvertToNullableDouble(dr, ReqSqlConstants.COL_TAX);
                Int64 typeId = ConvertToInt64(dr, ReqSqlConstants.COL_ITEM_TYPE_ID);
                string typeName = GetItemExtendedType(typeId);
                item.type = new IdAndName
                {
                    id = typeId,
                    name = typeName
                };
                item.unitPrice = ConvertToNullableDouble(dr, ReqSqlConstants.COL_UNIT_PRICE);
                item.uom = new CodeAndName
                {
                    code = ConvertToString(dr, ReqSqlConstants.COL_UOM_CODE),
                    name = ConvertToString(dr, ReqSqlConstants.COL_UOM_DESCRIPTION)
                };
                item.AllowDecimal = ConvertToBoolean(dr, ReqSqlConstants.COL_UOM_ALLOWDECIMAL);
                item.orderedQuantity = dr.Table.Columns.Contains(ReqSqlConstants.COL_ORDEREDQUANTITY) ? ConvertToNullableDouble(dr, ReqSqlConstants.COL_ORDEREDQUANTITY) : null;
                item.OrderedAmount = dr.Table.Columns.Contains(ReqSqlConstants.COL_ORDEREDAMOUNT) ? ConvertToNullableDouble(dr, ReqSqlConstants.COL_ORDEREDAMOUNT) : null;
                if (item.orderedQuantity > 0)
                {
                    if ((typeId == 1) || (typeId == 3))
                    {
                        item.OrderingStatus = item.orderedQuantity >= item.quantity ? item.quantity.ToString() : item.orderedQuantity.ToString();
                    }
                    else
                    {
                        item.OrderingStatus = item.OrderedAmount >= item.unitPrice ? item.unitPrice.ToString() : item.OrderedAmount.ToString();
                    }
                }
                else
                {
                    item.OrderingStatus = "0";
                }
                item.ContractItems = new IdAndName
                {
                    id = ConvertToInt32(dr, ReqSqlConstants.COL_CONTRACTITEMID),
                    name = ConvertToString(dr, ReqSqlConstants.COL_CONTRACTITEMDESCRIPTION)
                };

                item.orderingLocation = new IdCodeAndName
                {
                    id = ConvertToInt64(dr, ReqSqlConstants.COL_ORDERLOCATIONID),
                    code = ConvertToString(dr, ReqSqlConstants.COL_ORDERLOCATIONCODE),
                    name = ConvertToString(dr, ReqSqlConstants.COL_ORDERLOCATIONNAME)

                };

                item.orderingLocationAdress = BuildAddress(ConvertToString(dr, ReqSqlConstants.COL_ORDLOCADDRESS1), ConvertToString(dr, ReqSqlConstants.COL_ORDLOCADDRESS2),
                     ConvertToString(dr, ReqSqlConstants.COL_ORDLOCADDRESS3), ConvertToString(dr, ReqSqlConstants.COL_ORDLOCCITY),
                     ConvertToString(dr, ReqSqlConstants.COL_ORDLOCSTATE), ConvertToString(dr, ReqSqlConstants.COL_ORDLOCCOUNTRY),
                     ConvertToString(dr, ReqSqlConstants.COL_ORDLOCZIP));

                item.ShipFromLocation = new IdNameAndAddress
                {
                    id = ConvertToInt64(dr, ReqSqlConstants.COL_SHIPFROMLOCATIONID),
                    nameandnumber = ConvertToString(dr, ReqSqlConstants.COL_SHIPFROMLOCATIONCODE),
                    name = ConvertToString(dr, ReqSqlConstants.COL_SHIPFROMLOCATIONNAME),
                    address = BuildAddress(ConvertToString(dr, ReqSqlConstants.COL_SHIPFROMLOCADDRESS1), ConvertToString(dr, ReqSqlConstants.COL_SHIPFROMLOCADDRESS2),
                   ConvertToString(dr, ReqSqlConstants.COL_SHIPFROMLOCADDRESS3), ConvertToString(dr, ReqSqlConstants.COL_SHIPFROMLOCCITY),
                   ConvertToString(dr, ReqSqlConstants.COL_SHIPFROMLOCSTATE), ConvertToString(dr, ReqSqlConstants.COL_SHIPFROMLOCCOUNTRY),
                   ConvertToString(dr, ReqSqlConstants.COL_SHIPFROMLOCZIP))

                };


                item.partnerContact = new IdNameAndEmail
                {
                    id = ConvertToInt64(dr, ReqSqlConstants.COL_PARTNER_CONTACTID),
                    name = ConvertToString(dr, ReqSqlConstants.COL_PARTNERCONTACTNAME)
                };

                item.partnerContactEmail = ConvertToString(dr, ReqSqlConstants.COL_PARTNER_CONTACT_EMAIL);
                item.partnerContactNumber = ConvertToString(dr, ReqSqlConstants.COL_PARTNER_CONTACT_NUMBER);

                item.TrasmissionMode = ConvertToInt32(dr, ReqSqlConstants.COL_REQUISITION_TRNMODE);
                item.TransmissionValue = ConvertToString(dr, ReqSqlConstants.COL_REQUISITION_TRNVALUE);

                item.supplierDispatchMode = new IdAndName
                {
                    id = ConvertToInt32(dr, ReqSqlConstants.COL_REQUISITION_TRNMODE),
                    name = GetTranmissionModeById(ConvertToInt32(dr, ReqSqlConstants.COL_REQUISITION_TRNMODE))
                };

                item.AccountingStatus = ConvertToBoolean(dr, ReqSqlConstants.COL_ACCOUNTING_STATUS);
                item.OverallItemLimit = ConvertToDecimal(dr, ReqSqlConstants.COL_OVERALLITEMLIMIT);
                item.ProcurementStatus = ConvertToByte(dr, ReqSqlConstants.COL_PROCUREMENTSTATUS);

                item.lineSourceSystem = new IdAndName
                {
                    id = dr.Table.Columns.Contains(ReqSqlConstants.COL_SOURCESYSTEMID) ? ConvertToInt64(dr, ReqSqlConstants.COL_SOURCESYSTEMID) : 0,
                    name = dr.Table.Columns.Contains(ReqSqlConstants.COL_SOURCESYSTEMNAME) ? ConvertToString(dr, ReqSqlConstants.COL_SOURCESYSTEMNAME) : string.Empty
                };
                item.matching = new IdAndName()
                {
                    id = ConvertToInt16(dr, ReqSqlConstants.COL_MATCHTYPE),
                    name = ((NewPlatformEntities.MatchType)ConvertToInt16(dr, ReqSqlConstants.COL_MATCHTYPE)).ToString()
                };
                item.matching.name = item.matching.id == 0 ? "" : item.matching.name;
                item.LineStatus = ConvertToInt32(dr, ReqSqlConstants.COL_LineStatus);
                item.minQuantity = ConvertToNullableDouble(dr, ReqSqlConstants.COL_MIN_ORDER_QUANTITY);
                item.maxQuantity = ConvertToNullableDouble(dr, ReqSqlConstants.COL_MAX_ORDER_QUANTITY);
                item.banding = ConvertToNullableDouble(dr, ReqSqlConstants.COL_BANDING);

                item.IsAddedFromRequistion = ConvertToBoolean(dr, ReqSqlConstants.COL_ISADDEDFROMREQUISTION);
                item.PriceTypeId = ConvertToInt64(dr, ReqSqlConstants.COL_PRICETYPEID);
                item.PriceType = new IdAndName
                {
                    id = item.PriceTypeId,
                    name = ConvertToString(dr, ReqSqlConstants.COL_PRICETYPENAME)
                };
                item.JobTitleId = ConvertToInt64(dr, ReqSqlConstants.COL_JOBTITLEID);
                item.JobTitle = new IdAndName
                {
                    id = item.JobTitleId,
                    name = ConvertToString(dr, ReqSqlConstants.COL_JOBTITLENAME)

                };
                item.ContingentWorkerId = ConvertToInt64(dr, ReqSqlConstants.COL_CONTINGENTWORKERID);
                item.ContingentWorker = new IdAndName
                {
                    id = item.ContingentWorkerId,
                    name = ConvertToString(dr, ReqSqlConstants.COL_CONTINGENTWORKERNAME)

                };
                item.Margin = ConvertToDecimal(dr, ReqSqlConstants.COL_MARGIN);
                item.BaseRate = ConvertToDecimal(dr, ReqSqlConstants.COL_BASERATE);
                item.ReportingManagerId = ConvertToInt64(dr, ReqSqlConstants.COL_REPORTINGMANAGERID);
                item.ReportingManager = new IdAndName()
                {
                    id = item.ReportingManagerId,
                    name = ConvertToString(dr, ReqSqlConstants.COL_REPORTINGMANAGERFIRSTNAME),
                };
                item.StockReservationNumber = ConvertToString(dr, ReqSqlConstants.COL_STOCKRESERVATIONNUMBER);
                item.SmartFormId = ConvertToInt64(dr, ReqSqlConstants.COL_SMARTFORMID);
                item.partnerReconMatchTypeId = ConvertToInt32(dr, ReqSqlConstants.COL_PARTNERRECONMATCHTYPEID);
                item.remitToLocation = new IdNameAndAddress()
                {
                    id = ConvertToInt64(dr, ReqSqlConstants.COL_INVOICE_REMITTOLOCATIONID),
                };
                item.SpendControlDocumentCode = dr.Table.Columns.Contains(ReqSqlConstants.COL_SPENDCONTROLDOCUMENTCODE) ? ConvertToInt64(dr, ReqSqlConstants.COL_SPENDCONTROLDOCUMENTCODE) : 0;
                item.SpendControlDocumentItemId = dr.Table.Columns.Contains(ReqSqlConstants.COL_SPENDCONTROLDOCUMENTITEMID) ? ConvertToInt64(dr, ReqSqlConstants.COL_SPENDCONTROLDOCUMENTITEMID) : 0;
                item.SpendControlDocumentNumber = dr.Table.Columns.Contains(ReqSqlConstants.COL_SPENDCONTROLDOCUMENTNUMBER) ? ConvertToString(dr, ReqSqlConstants.COL_SPENDCONTROLDOCUMENTNUMBER) : string.Empty;
                item.SpendControlDocumentName = dr.Table.Columns.Contains(ReqSqlConstants.COL_SPENDCONTROLDOCUMENTNAME) ? ConvertToString(dr, ReqSqlConstants.COL_SPENDCONTROLDOCUMENTNAME) : string.Empty;
                item.SpendControlDocumentItemReferenceNumber = dr.Table.Columns.Contains(ReqSqlConstants.COL_SPENDCONTROLDOCUMENTITEMREFERENCENUMBER) ? ConvertToString(dr, ReqSqlConstants.COL_SPENDCONTROLDOCUMENTITEMREFERENCENUMBER) : string.Empty;
                item.paymentTerms = new IdAndName()
                {
                    id = ConvertToInt32(dr, ReqSqlConstants.COL_PAYMENTTERMID),
                    name = ConvertToString(dr, ReqSqlConstants.COL_PAYMENTTERMNAME)
                };

                item.CategoryHierarchy = dr.Table.Columns.Contains(ReqSqlConstants.COL_CATEGORYHIERARCHY) ? ConvertToString(dr, ReqSqlConstants.COL_CATEGORYHIERARCHY) : string.Empty;

                item.incoTermCode = new IdAndName()
                {
                    id = ConvertToInt32(dr, ReqSqlConstants.COL_INCOTERMID),
                    name = ConvertToString(dr, ReqSqlConstants.COL_INCOTERMCODE)
                };

                item.incoTermLocation = ConvertToString(dr, ReqSqlConstants.COL_INCOTERMLOCATION);
                item.ConversionFactor = (decimal)(dr.Table.Columns.Contains(ReqSqlConstants.COL_CONVERSIONFACTOR) ? ConvertToDouble(dr, ReqSqlConstants.COL_CONVERSIONFACTOR) : 0);
                item.TaxJurisdiction = dr.Table.Columns.Contains(ReqSqlConstants.COL_TAXJURISDICTION) ? ConvertToString(dr, ReqSqlConstants.COL_TAXJURISDICTION) : string.Empty;
                item.IsAdhocShipToLocation = dr.Table.Columns.Contains(ReqSqlConstants.COL_ISADHOCSHIPTOLOCATION) ? ConvertToBoolean(dr, ReqSqlConstants.COL_ISADHOCSHIPTOLOCATION) : false;
                item.Itemspecification = dr.Table.Columns.Contains(ReqSqlConstants.COL_ITEMSPECIFICATION) ? ConvertToString(dr, ReqSqlConstants.COL_ITEMSPECIFICATION) : string.Empty;
                item.InternalPlantMemo = dr.Table.Columns.Contains(ReqSqlConstants.COL_INTERNALPLANTMEMO) ? ConvertToString(dr, ReqSqlConstants.COL_INTERNALPLANTMEMO) : string.Empty;
                item.AllowAdvances = dr.Table.Columns.Contains(ReqSqlConstants.COL_ALLOWADVANCES) ? ConvertToBoolean(dr, ReqSqlConstants.COL_ALLOWADVANCES) : false;
                item.AdvancePercentage = (decimal)(dr.Table.Columns.Contains(ReqSqlConstants.COL_ADVANCEPERCENTAGE) ? ConvertToDouble(dr, ReqSqlConstants.COL_ADVANCEPERCENTAGE) : 0);
                item.AdvanceAmount = (decimal)(dr.Table.Columns.Contains(ReqSqlConstants.COL_ADVANCEAMOUNT) ? ConvertToDouble(dr, ReqSqlConstants.COL_ADVANCEAMOUNT) : 0);
                item.AdvanceReleaseDate = ConvertToNullableDateTime(dr, ReqSqlConstants.COL_ADVANCERELEASEDATE);
                item.ItemMasterId = ConvertToNullableInt64(dr, ReqSqlConstants.COL_ITEMMASTERID);
                item.IsPreferredSupplier = dr.Table.Columns.Contains(ReqSqlConstants.COL_ISPREFERREDSUPPLIER) ? ConvertToBoolean(dr, ReqSqlConstants.COL_ISPREFERREDSUPPLIER) : false;
                item.AllowFlexiblePrice = dr.Table.Columns.Contains(ReqSqlConstants.COL_ALLOWFLEXIBLEPRICE) ? ConvertToBoolean(dr, ReqSqlConstants.COL_ALLOWFLEXIBLEPRICE) : false;
                item.ContractReference = dr.Table.Columns.Contains(ReqSqlConstants.COL_CONTRACTREFERENCE)
                    ? ConvertToString(dr, ReqSqlConstants.COL_CONTRACTREFERENCE)
                    : string.Empty;
                items.Add(item);

            }
            return items;
        }

        private string GetItemExtendedType(long typeId)
        {
            switch (typeId)
            {
                case 0:
                    return "None";
                case 1:
                    return MATERIAL;
                case 2:
                    return FIXED_SERVICE;
                case 3:
                    return VARIABLE_SERVICE;
                case 4:
                    return SERVICE_ACTIVITY;
                case 5:
                    return MILESTONE;
                case 6:
                    return PROGRESS;
                case 7:
                    return ADVANCE;
                case 8:
                    return CHARGE;
                case 9:
                    return CONTINGENTWORKER;
                case 10:
                    return EXPENSES;
                default:
                    return "";
            }

        }
        private string GetTranmissionModeById(int id)
        {
            switch (id)
            {
                case 1: return "Portal";
                case 2: return "EDI/cXML";
                case 3: return "Direct Email";
                case 4: return "Call & Submit";
                case 5: return "Fax";
                default: return "";
            }

        }

        /// <summary>
        /// Fills tax details in requisition items.
        /// </summary>
        /// <param name="rows">Data rows.</param>
        /// <param name="req">Requisition reference.</param>
        private void FillTaxesInRequisitionItems(DataRowCollection rows, ref NewP2PEntities.Requisition req)
        {
            List<Tax> taxes = new List<Tax>();
            foreach (DataRow dr in rows)
            {
                Tax tax = new Tax();
                tax.code = ConvertToString(dr, ReqSqlConstants.COL_TAX_CODE);
                tax.description = ConvertToString(dr, ReqSqlConstants.COL_TAX_DESC);
                tax.id = ConvertToInt64(dr, ReqSqlConstants.COL_ID);
                tax.itemId = ConvertToInt64(dr, ReqSqlConstants.COL_ITEM_ID);
                tax.percent = ConvertToDecimal(dr, ReqSqlConstants.COL_TAXPERCENT);
                tax.taxId = ConvertToInt32(dr, ReqSqlConstants.COL_TAXID);
                Int64 taxTypeId = ConvertToInt64(dr, ReqSqlConstants.COL_TAXTYPEID);
                tax.type = new IdAndName
                {
                    id = taxTypeId,
                    name = taxTypeId == 1 ? FEDERAL : taxTypeId == 2 ? STATE : taxTypeId == 3 ? DISTRICT : taxTypeId == 4 ? CITY : taxTypeId == 5 ? COUNTY : String.Empty
                };
                taxes.Add(tax);
            }

            foreach (NewP2PEntities.RequisitionItem item in req.items)
            {
                item.taxItems = new List<Tax>();
                item.taxPercentage = 0;
                taxes.ForEach(x => { if (x.itemId == item.id) { item.taxPercentage += x.percent; item.taxItems.Add(x); } });
            }
        }

        /// <summary>
        /// Gets all splits in requisition.
        /// </summary>
        /// <param name="rows">Data rows.</param>
        /// <returns>All splits.</returns>
        private List<ReqAccountingSplit> GetAllRequisitionSplits(DataRowCollection rows)
        {
            List<ReqAccountingSplit> splits = new List<ReqAccountingSplit>();
            foreach (DataRow dr in rows)
            {
                ReqAccountingSplit split = new ReqAccountingSplit();
                split.createdBy = new IdAndName
                {
                    id = ConvertToInt64(dr, ReqSqlConstants.COL_CREATEDBYID),
                    name = BuildName(ConvertToString(dr, ReqSqlConstants.COL_CREATEDBYFNAME), ConvertToString(dr, ReqSqlConstants.COL_CREATEDBYLNAME))
                };
                split.createdOn = ConvertToDateTime(dr, ReqSqlConstants.COL_CREATED_ON);
                split.documentCode = ConvertToInt64(dr, ReqSqlConstants.COL_REQUISITION_ID);
                split.documentItemId = ConvertToInt64(dr, ReqSqlConstants.COL_REQUISITION_ITEM_ID);
                split.id = ConvertToInt64(dr, ReqSqlConstants.COL_REQUISITION_SPLIT_ITEM_ID);
                split.lastModifiedBy = new IdAndName
                {
                    id = ConvertToInt64(dr, ReqSqlConstants.COL_LASTMODIFIEDBYID),
                    name = BuildName(ConvertToString(dr, ReqSqlConstants.COL_LASTMODIFIEDBYFNAME), ConvertToString(dr, ReqSqlConstants.COL_LASTMODIFIEDBYLNAME))
                };
                split.lastModifiedOn = ConvertToDateTime(dr, ReqSqlConstants.COL_LASTMODIFIEDON);
                split.quantity = ConvertToDouble(dr, ReqSqlConstants.COL_QUANTITY);
                split.splitItemTotal = ConvertToDecimal(dr, ReqSqlConstants.COL_SPLIT_ITEM_TOTAL);
                split.errorCode = ConvertToString(dr, ReqSqlConstants.COL_ERRORCODE);
                split.percentage = ConvertToDecimal(dr, ReqSqlConstants.COL_PERCENTAGE);
                split.shippingCharges = dr.Table.Columns.Contains(ReqSqlConstants.COL_SHIPPING_CHARGES) ? ConvertToDecimal(dr, ReqSqlConstants.COL_SHIPPING_CHARGES) : 0;
                split.additionalCharges = dr.Table.Columns.Contains(ReqSqlConstants.COL_ADDITIONAL_CHARGES) ? ConvertToDecimal(dr, ReqSqlConstants.COL_ADDITIONAL_CHARGES) : 0;
                split.tax = dr.Table.Columns.Contains(ReqSqlConstants.COL_TAX) ? ConvertToDecimal(dr, ReqSqlConstants.COL_TAX) : 0;
                split.OverallLimitSplitItem = dr.Table.Columns.Contains(ReqSqlConstants.COL_OVERALLLIMITSPLITITEM) ? ConvertToDecimal(dr, ReqSqlConstants.COL_OVERALLLIMITSPLITITEM) : 0;
                split.isADREnabel = true;
                splits.Add(split);
            }

            return splits;
        }

        /// <summary>
        /// Gets all DB split entities.
        /// </summary>
        /// <param name="rows">Data rows.</param>
        /// <returns>DB split entities.</returns>
        private List<DBSplitEntity> GetAllRequisitionSplitEntities(DataRowCollection rows)
        {
            List<DBSplitEntity> splitEntities = new List<DBSplitEntity>();
            foreach (DataRow dr in rows)
            {
                DBSplitEntity splitEntity = new DBSplitEntity();
                splitEntity.code = ConvertToString(dr, ReqSqlConstants.COL_SPLIT_ACCOUNTING_FIELD_VALUE);
                splitEntity.entityCode = ConvertToString(dr, ReqSqlConstants.COL_ENTITY_CODE);
                splitEntity.title = ConvertToString(dr, ReqSqlConstants.COL_TITLE);
                if (splitEntity.title.ToUpper() == REQUESTER)
                    splitEntity.name = ConvertToString(dr, ReqSqlConstants.COL_REQUESTER_NAME);
                else
                    splitEntity.name = ConvertToString(dr, ReqSqlConstants.COL_ENTITY_DISPLAY_NAME);
                splitEntity.entityType = ConvertToInt32(dr, ReqSqlConstants.COL_ENTITY_TYPE_ID);
                splitEntity.fieldId = ConvertToInt64(dr, ReqSqlConstants.COL_FIELDCONFIGID);
                splitEntity.splitEntityId = ConvertToInt64(dr, ReqSqlConstants.COL_REQSPLITITEMENTITYID);
                splitEntity.glName = ConvertToString(dr, ReqSqlConstants.COL_GLNAME);
                splitEntity.splitItemId = ConvertToInt64(dr, ReqSqlConstants.COL_REQUISITION_SPLIT_ITEM_ID);
                splitEntity.FieldControlType = ConvertToInt64(dr, ReqSqlConstants.COL_FIELD_CONTROL_TYPE);
                splitEntity.FieldName = ConvertToString(dr, ReqSqlConstants.COL_FIELD_NAME);
                splitEntity.AutoSuggestURLId = ConvertToInt32(dr, ReqSqlConstants.COL_AUTO_SUGGEST_URL_ID);
                splitEntities.Add(splitEntity);
            }

            return splitEntities;
        }

        /// <summary>
        /// Fills accounting entities in splits.
        /// </summary>
        /// <param name="splitEntities">Split entities.</param>
        /// <param name="requester">Requester.</param>
        /// <param name="splits">Splits reference.</param>
        private void FillEntitiesInRequisitionSplits(List<DBSplitEntity> splitEntities, IdAndName requester, ref List<ReqAccountingSplit> splits)
        {
            foreach (ReqAccountingSplit split in splits)
            {
                split.SplitEntities = new List<DBSplitEntity>();
                List<DBSplitEntity> splitEnts = (from se in splitEntities where se.splitItemId == split.id select se).ToList();
                //Below code is commented beacuse we are implementing dynamic splitentities - REQ - 4496
                //Int16 counter = 1;
                foreach (DBSplitEntity splitEnt in splitEnts)
                {
                    if (splitEnt.title.ToUpper() == REQUESTER)
                        split.requester = new SplitEntity
                        {
                            code = splitEnt.code == String.Empty ? (requester == null ? String.Empty : requester.id.ToString()) : splitEnt.code,
                            name = splitEnt.name == String.Empty ? (requester == null ? String.Empty : requester.name) : splitEnt.name,
                            fieldId = splitEnt.fieldId,
                            splitEntityId = splitEnt.splitEntityId,
                            FieldControlType = splitEnt.FieldControlType,
                            FieldName = splitEnt.FieldName
                        };
                    else
                    {
                        SplitEntity newEnt = new SplitEntity
                        {
                            code = splitEnt.code,
                            entityCode = splitEnt.entityCode,
                            name = splitEnt.name,
                            entityType = splitEnt.entityType,
                            fieldId = splitEnt.fieldId,
                            splitEntityId = splitEnt.splitEntityId,
                            title = splitEnt.title,
                            FieldControlType = splitEnt.FieldControlType,
                            FieldName = splitEnt.FieldName
                        };
                        if (splitEnt.title.ToUpper() == GL_CODE && splitEnt.AutoSuggestURLId == 2)
                            split.gLCode = newEnt;
                        else if (splitEnt.title.ToUpper() == PERIOD && splitEnt.entityType == 0)
                        {
                            split.period = newEnt;
                        }
                        else
                        {
                            split.SplitEntities.Add(splitEnt);
                            /* Below code is commented beacuse we are implementing dynamic splitentities-REQ-4496
                            if (counter == 1)
                                split.splitEntity1 = newEnt;
                            else if (counter == 2)
                                split.splitEntity2 = newEnt;
                            else if (counter == 3)
                                split.splitEntity3 = newEnt;
                            else if (counter == 4)
                                split.splitEntity4 = newEnt;
                            else if (counter == 5)
                                split.splitEntity5 = newEnt;
                            else if (counter == 6)
                                split.splitEntity6 = newEnt;
                            else if (counter == 7)
                                split.splitEntity7 = newEnt;
                            else if (counter == 8)
                                split.splitEntity8 = newEnt;
                            else if (counter == 9)
                                split.splitEntity9 = newEnt;
                            else if (counter == 10)
                                split.splitEntity10 = newEnt;
                            else if (counter == 11)
                                split.splitEntity11 = newEnt;
                            else if (counter == 12)
                                split.splitEntity12 = newEnt;
                            else if (counter == 13)
                                split.splitEntity13 = newEnt;
                            else if (counter == 14)
                                split.splitEntity14 = newEnt;
                            else if (counter == 15)
                                split.splitEntity15 = newEnt;
                            else if (counter == 16)
                                split.splitEntity16 = newEnt;
                            else if (counter == 17)
                                split.splitEntity17 = newEnt;
                            else if (counter == 18)
                                split.splitEntity18 = newEnt;
                            else if (counter == 19)
                                split.splitEntity19 = newEnt;
                            else if (counter == 20)
                                split.splitEntity20 = newEnt;

                            counter++;
                            */
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets all notes related to a requisition
        /// </summary>
        /// <param name="rows">Data rows.</param>
        /// <returns>List of DB notes.</returns>
        private List<DBNote> GetAllNotesInRequisition(DataRowCollection rows)
        {
            List<DBNote> notes = new List<DBNote>();
            foreach (DataRow dr in rows)
            {
                DBNote note = new DBNote();
                note.accessType = ConvertToInt32(dr, ReqSqlConstants.COL_ACCESSTYPE);
                note.createdBy = new IdAndName
                {
                    id = ConvertToInt64(dr, ReqSqlConstants.COL_CREATEDBYID),
                    name = BuildName(ConvertToString(dr, ReqSqlConstants.COL_CREATEDBYFNAME), ConvertToString(dr, ReqSqlConstants.COL_CREATEDBYLNAME))
                };
                note.createdOn = ConvertToNullableDateTime(dr, ReqSqlConstants.COL_CREATED_ON);
                note.id = ConvertToInt32(dr, ReqSqlConstants.COL_ID);
                note.objectId = ConvertToInt64(dr, ReqSqlConstants.COL_OBJECTID);
                note.objectType = ConvertToString(dr, ReqSqlConstants.COL_OBJECTTYPE);
                note.text = ConvertToString(dr, ReqSqlConstants.COL_COMMENT);
                notes.Add(note);
            }

            return notes;
        }

        /// <summary>
        /// Gets all attachments associated to Requisition.
        /// </summary>
        /// <param name="rows">Data rows.</param>
        /// <returns>List of attachments.</returns>
        private List<DBAttachment> GetAllAttachmentsInRequisition(DataRowCollection rows)
        {
            List<DBAttachment> attachments = new List<DBAttachment>();
            foreach (DataRow dr in rows)
            {
                DBAttachment attachment = new DBAttachment();
                attachment.commentId = ConvertToInt32(dr, ReqSqlConstants.COL_COMMENTID);
                attachment.FileName = ConvertToString(dr, ReqSqlConstants.COL_ItemImageFileName);
                attachment.FileURL = ConvertToString(dr, ReqSqlConstants.COL_FILEURL);
                attachment.fileId = ConvertToInt64(dr, ReqSqlConstants.COL_ItemImageFileId);
                attachments.Add(attachment);
            }

            return attachments;
        }

        /// <summary>
        /// Fills attachments in notes.
        /// </summary>
        /// <param name="attachments">List of attachments.</param>
        /// <param name="notes">Notes reference.</param>
        private void FillAttachmentsInNotes(List<DBAttachment> attachments, ref List<DBNote> notes)
        {
            foreach (DBNote note in notes)
                note.attachmentURLs = (from a in attachments
                                       where a.commentId == note.id
                                       select new URLandName
                                       {
                                           name = a.FileName,
                                           url = a.FileURL,
                                           fileId = a.fileId,
                                           encryptedFileId = FileIdEncryptionDAO.Encrypt(a.fileId, this.UserContext.ContactCode)
                                       }).ToList();
        }

        /// <summary>
        /// Returns text for access type.
        /// </summary>
        /// <param name="accessType">Access type.</param>
        /// <returns>String</returns>
        private string GetCommentAccessTypeStr(Int32 accessType)
        {
            switch (accessType)
            {
                case 1:
                    return INTERNALUSERS;
                case 2:
                    return APPROVERS;
                case 3:
                    return SUPPLIER;
                case 4:
                    return INTERNALUSERSANDPARTNERS;
                case 5:
                    return CUSTOM;
                case 6:
                    return BUYERS;
                case 7:
                    return REQUESTERS;
                case 8:
                    return PAYABLEUSERS;
                default:
                    return INTERNALUSERS;
            }
        }

        /// <summary>
        /// Fills notes in requisition.
        /// </summary>
        /// <param name="notes">Notes.</param>
        /// <param name="req">Requisition reference.</param>
        private void FillNotesInRequisition(List<DBNote> notes, ref NewP2PEntities.Requisition req)
        {
            Int64 reqId = req.id;
            long status = req.status.id;
            req.notes = (from n in notes
                         where n.objectId == reqId && n.objectType == REQ_OBJECT_TYPE
                         select new Note
                         {
                             id = n.id,
                             accessType = new IdAndName
                             {
                                 id = n.accessType,
                                 name = GetCommentAccessTypeStr(n.accessType)
                             },
                             attachmentURLs = n.attachmentURLs,
                             createdBy = n.createdBy,
                             createdOn = n.createdOn,
                             text = n.text,
                             IsDeleteEnable =
                                (status != (long)GEP.Cumulus.Documents.Entities.DocumentStatus.Draft &&
                                status != (long)GEP.Cumulus.Documents.Entities.DocumentStatus.Rejected &&
                                status != (long)GEP.Cumulus.Documents.Entities.DocumentStatus.Withdrawn)
                                ? false : true
                         }).ToList();
            if (req.items != null)
            {
                foreach (NewP2PEntities.RequisitionItem item in req.items)
                {
                    item.notes = (from n in notes
                                  where n.objectId == item.id && n.objectType == REQ_LINE_OBJECT_TYPE
                                  select new Note
                                  {
                                      id = n.id,
                                      accessType = new IdAndName
                                      {
                                          id = n.accessType,
                                          name = GetCommentAccessTypeStr(n.accessType)
                                      },
                                      attachmentURLs = n.attachmentURLs,
                                      createdBy = n.createdBy,
                                      createdOn = n.createdOn,
                                      text = n.text,
                                      IsDeleteEnable =
                                        (status != (long)GEP.Cumulus.Documents.Entities.DocumentStatus.Draft &&
                                        status != (long)GEP.Cumulus.Documents.Entities.DocumentStatus.Rejected &&
                                        status != (long)GEP.Cumulus.Documents.Entities.DocumentStatus.Withdrawn)
                                        ? false : true
                                  }).ToList();
                }
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Saves header of a requisition.
        /// </summary>
        /// <param name="req">Requisition with filled header</param>
        /// <returns>Outcome of save</returns>
        public SaveResult SaveRequisitionHeader(NewP2PEntities.Requisition req, List<NewPlatformEntities.DocumentBU> documentBUs)
        {
            if (req == null)
                return new SaveResult { success = false, message = "Requisition is empty." };

            var sqlHelper = ContextSqlConn;
            InitializeConnection(sqlHelper.ConnectionString);

            InitializeDBReq(false, req.id);
            if (!SaveReqHeader(req, documentBUs))
                return new SaveResult { success = false, message = "Requisition is not valid." };

            _Context.SaveChanges();
            DisposeConnection();
            if (dbReq == null || dbReq.DM_Documents == null || dbReq.RequisitionID <= 0 || String.IsNullOrEmpty(dbReq.DM_Documents.DocumentNumber))
                return new SaveResult { success = false, message = "Error while saving." };

            if (req.id > 0)
                AddIntoSearchIndexerQueueing(req.id, REQ, UserContext, GepConfiguration);

            return new SaveResult { success = true, id = dbReq.RequisitionID, number = dbReq.DM_Documents.DocumentNumber };
        }

        public async Task<Dictionary<int, List<QuestionResponse>>> GetAllCustomAttribsForDoc(long id)
        {
            Dictionary<int, List<QuestionResponse>> dict = new Dictionary<int, List<QuestionResponse>>();
            List<QuestionResponse> lstQueResponse = new List<QuestionResponse>();
            List<QuestionResponse> lstlineQueResponse = new List<QuestionResponse>();
            List<QuestionResponse> lstspiltQueResponse = new List<QuestionResponse>();
            if (id > 0)
            {
                var customAttribDataSet = ContextSqlConn.ExecuteDataSet(ReqSqlConstants.USP_P2P_GETALLCUSTOMATTRIBSBYDOCUMENTCODE, new object[] { id, REQ });
                if (customAttribDataSet != null && customAttribDataSet.Tables.Count > 0 && customAttribDataSet.Tables[0].Rows.Count > 0)
                {
                    int level = 0;
                    foreach (DataRow dr in customAttribDataSet.Tables[0].Rows)
                    {
                        QuestionResponse que = new QuestionResponse();

                        que.AssesseeId = ConvertToInt64(dr, ReqSqlConstants.COL_ASSESSEEID);
                        que.AssessorId = ConvertToInt64(dr, ReqSqlConstants.COL_ASSESSORID);
                        que.AssessorType = (AssessorUserType)dr[ReqSqlConstants.COL_ASSESSORTYPE];
                        que.ColumnId = ConvertToInt64(dr, ReqSqlConstants.COL_COLUMNID);
                        que.CreatedOn = ConvertToDateTime(dr, ReqSqlConstants.COL_DATE_CREATED);
                        que.IsDeleted = ConvertToBoolean(dr, ReqSqlConstants.COL_IS_DELETED);
                        que.ObjectInstanceId = ConvertToInt64(dr, ReqSqlConstants.COL_OBJECTINSTANCEID);
                        que.QuestionId = ConvertToInt64(dr, ReqSqlConstants.COL_QUESTIONID);
                        que.ResponseId = ConvertToInt64(dr, ReqSqlConstants.COL_RESPONSEID);
                        que.ResponseValue = ConvertToString(dr, ReqSqlConstants.COL_RESPONSEVALUE);
                        que.UserComments = ConvertToString(dr, ReqSqlConstants.COL_USERCOMMENTS);
                        que.UpdatedOn = ConvertToDateTime(dr, ReqSqlConstants.COL_DATE_MODIFIED);
                        que.RowId = ConvertToInt64(dr, ReqSqlConstants.COL_ROWID);
                        level = ConvertToInt16(dr, ReqSqlConstants.COL_LEVEL);
                        switch (level)
                        {
                            case 1:
                                lstQueResponse.Add(que);
                                break;
                            case 2:
                                lstlineQueResponse.Add(que);
                                break;
                            case 3:
                                lstspiltQueResponse.Add(que);
                                break;
                        }

                    }
                    dict.Add(1, lstQueResponse);
                    dict.Add(2, lstlineQueResponse);
                    dict.Add(3, lstspiltQueResponse);
                }
            }

            return await Task.FromResult<Dictionary<int, List<QuestionResponse>>>(dict);

        }

        /// <summary>
        /// Gets requisition in displayable format.
        /// </summary>
        /// <param name="id">Requisition id.</param>
        /// <returns>Requisition for display.</returns>
        public NewP2PEntities.Requisition GetRequisitionDisplayDetails(Int64 id, List<long> reqLineItemIds = null, bool enableCategoryAutoSuggest = false)
        {
            var task1 = GetAllCustomAttribsForDoc(id);
            var task2 = GetRequisitionetails(id, reqLineItemIds, enableCategoryAutoSuggest);
            var task3 = GetUserConfigurationsDetails(UserContext.ContactCode, REQ);
            var task4 = GetNotesAndAttachments(id, 1);
            var task5 = GetCategoryType();
            var task6 = GetDocumentStakeholderDetails(id);
            List<Task> TaskList = new List<Task>();
            TaskList.Add(task1);
            TaskList.Add(task2);
            TaskList.Add(task3);
            TaskList.Add(task4);
            TaskList.Add(task5);
            TaskList.Add(task6);
            Task.WhenAll(TaskList);

            var req = task2.Result;
            var cAttrib = task1.Result;
            req.UserConfigurations = task3.Result;
            req.lstNotesOrAttachments = task4.Result;
            req.lstCategoryType = task5.Result;
            req.HeaderCustomAttribs = new List<QuestionResponse>();
            req.documentStakeHolderList = task6.Result;
            foreach (var key in cAttrib.Keys)
            {
                if (cAttrib[key].Count() > 0)
                {
                    foreach (QuestionResponse qr in cAttrib[key])
                    {
                        if (qr.ObjectInstanceId == req.id && key == 1)
                            req.HeaderCustomAttribs.Add(qr);
                        else
                        {
                            req.items.ForEach(x =>
                            {
                                if (x.id == qr.ObjectInstanceId && key == 2)
                                {
                                    x.ItemCustomAttribs = x.ItemCustomAttribs == null ? new List<QuestionResponse>() : x.ItemCustomAttribs;
                                    x.ItemCustomAttribs.Add(qr);
                                }
                                else if (x.splits != null && x.splits.Count() > 0)
                                {
                                    x.splits.ForEach(sp =>
                                    {
                                        if (sp.id == qr.ObjectInstanceId && key == 3)
                                        {
                                            sp.SplitCustomAttribs = sp.SplitCustomAttribs == null ? new List<QuestionResponse>() : sp.SplitCustomAttribs;
                                            sp.SplitCustomAttribs.Add(qr);
                                        }
                                    });
                                }
                            });
                        }
                    }
                }
            }

            return req;
        }

        public async Task<List<BusinessEntities.NotesOrAttachments>> GetNotesAndAttachments(long id, byte level)
        {
            RefCountingDataReader objRefCountingDataReader = null;
            List<BusinessEntities.NotesOrAttachments> lstNotesOrAttachments = new List<BusinessEntities.NotesOrAttachments>();


            LogHelper.LogInfo(Log, "GetNotesAndAttachments Method Started for id=" + id);
            var sqlHelper = ContextSqlConn;
            objRefCountingDataReader = (RefCountingDataReader)sqlHelper.ExecuteReader(ReqSqlConstants.USP_P2P_REQ_GETREQUISITIONNOTESORATTACHMENTS, new object[] { id, level, (byte)(UserContext.IsSupplier ? 2 : 1) });
            if (objRefCountingDataReader != null)
            {
                var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;
                while (sqlDr.Read())
                {
                    lstNotesOrAttachments.Add(new BusinessEntities.NotesOrAttachments
                    {
                        NotesOrAttachmentId = GetLongValue(sqlDr, ReqSqlConstants.COL_NOTES_ATTACH_ID),
                        DocumentCode = GetLongValue(sqlDr, ReqSqlConstants.COL_NOTES_ATTACH_REQID),
                        LineItemId = GetLongValue(sqlDr, ReqSqlConstants.COL_NOTES_ATTACH_ITEMID),
                        FileId = GetLongValue(sqlDr, ReqSqlConstants.COL_ItemImageFileId),
                        NoteOrAttachmentName = GetStringValue(sqlDr, ReqSqlConstants.COL_NOTES_ATTACH_NAME),
                        NoteOrAttachmentDescription = GetStringValue(sqlDr, ReqSqlConstants.COL_NOTES_ATTACH_DESC),
                        NoteOrAttachmentType = (BusinessEntities.NoteOrAttachmentType)GetTinyIntValue(sqlDr, ReqSqlConstants.COL_NOTES_ATTACH_TYPE),
                        AccessTypeId = (BusinessEntities.NoteOrAttachmentAccessType)GetTinyIntValue(sqlDr, ReqSqlConstants.COL_NOTES_ATTACH_ACCESSTYPE),
                        SourceType = (BusinessEntities.SourceType)GetTinyIntValue(sqlDr, ReqSqlConstants.COL_SOURCE_TYPE),
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

                if (!ReferenceEquals(objRefCountingDataReader, null) && !objRefCountingDataReader.IsClosed)
                {
                    objRefCountingDataReader.Close();
                    objRefCountingDataReader.Dispose();
                }

                LogHelper.LogInfo(Log, "GetNotesAndAttachments Method Ended for id=" + id);
            }

            return await Task.FromResult<List<BusinessEntities.NotesOrAttachments>>(lstNotesOrAttachments);
        }


        public async Task<List<KeyValuePair<string, int>>> GetCategoryType()
        {
            List<KeyValuePair<string, int>> lstCategoryTypes = new List<KeyValuePair<string, int>>();

            LogHelper.LogInfo(Log, "Notes Attachments GetCategoryTypes Method Started");
            RefCountingDataReader objRefCountingDataReader = null;

            SqlDataReader sqlDr = null;
            if (Log.IsDebugEnabled)
                Log.Debug(string.Format(CultureInfo.InvariantCulture, "GetCategoryTypes sp USP_P2P_PO_GETGLCODE was called."));
            try
            {
                objRefCountingDataReader =
                    (RefCountingDataReader)
                    ContextSqlConn.ExecuteReader(ReqSqlConstants.USP_P2P_GETCATEGORYTYPES);

                if (objRefCountingDataReader != null)
                {
                    sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;
                    while (sqlDr.Read())
                    {
                        lstCategoryTypes.Add(new KeyValuePair<string, int>(GetStringValue(sqlDr, ReqSqlConstants.COL_NOTES_ATTACH_CAT_DESC), GetIntValue(sqlDr, ReqSqlConstants.COL_NOTES_ATTACH_CAT_ID)));
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogInfo(Log, "Error Occurred in GetCategoryTypes Method ended for Order");
                throw ex;
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

                LogHelper.LogInfo(Log, "GetCategoryTypes Method ended for Order");
            }

            return await Task.FromResult<List<KeyValuePair<string, int>>>(lstCategoryTypes);
        }

        public async Task<NewP2PEntities.Requisition> GetRequisitionetails(long id, List<long> reqLineItemIds = null, bool enableCategoryAutoSuggest = false)
        {
            NewP2PEntities.Requisition req = new NewP2PEntities.Requisition();
            SqlConnection objSqlCon = null;
            try
            {

                if (id <= 0)
                {
                    req = GetNewRequisitionDisplayDetails();
                }
                else
                {
                    DataSet requisitionDataSet = null;
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
                    var sqlHelper = ContextSqlConn;
                    objSqlCon = (SqlConnection)sqlHelper.CreateConnection();
                    using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_GETALLDISPLAYDETAILS))
                    {
                        objSqlCommand.CommandType = CommandType.StoredProcedure;
                        objSqlCommand.Parameters.Add(new SqlParameter("@id", id));
                        objSqlCommand.Parameters.Add(new SqlParameter("@tvp_P2P_REQ_ReqItemIds", SqlDbType.Structured)
                        {
                            TypeName = ReqSqlConstants.TVP_LONG,
                            Value = dtReqItemId
                        });
                        objSqlCommand.Parameters.Add(new SqlParameter("@enableCategoryAutoSuggest", enableCategoryAutoSuggest));


                        requisitionDataSet = sqlHelper.ExecuteDataSet(objSqlCommand);
                    }
                    //var requisitionDataSet = ContextSqlConn.ExecuteDataSet(ReqSqlConstants.USP_P2P_REQ_GETALLDISPLAYDETAILS, new object[] { id });
                    if (requisitionDataSet != null && requisitionDataSet.Tables.Count > 0)
                    {
                        if (requisitionDataSet.Tables[0].Rows != null && requisitionDataSet.Tables[0].Rows.Count > 0)
                        {
                            FillRequisitionHeader(requisitionDataSet.Tables[0].Rows[0], requisitionDataSet.Tables[8].Rows, requisitionDataSet.Tables[9].Rows[0], ref req);

                            if (requisitionDataSet.Tables.Count > 2 && requisitionDataSet.Tables[2].Rows != null && requisitionDataSet.Tables[2].Rows.Count > 0)
                            {
                                req.items = GetRequisitionItems(requisitionDataSet.Tables[2].Rows);
                                if (requisitionDataSet.Tables.Count > 3 && requisitionDataSet.Tables[3].Rows != null && requisitionDataSet.Tables[3].Rows.Count > 0)
                                    FillTaxesInRequisitionItems(requisitionDataSet.Tables[3].Rows, ref req);

                                if (requisitionDataSet.Tables.Count > 4 && requisitionDataSet.Tables[4].Rows != null && requisitionDataSet.Tables[4].Rows.Count > 0)
                                {
                                    List<ReqAccountingSplit> splits = GetAllRequisitionSplits(requisitionDataSet.Tables[4].Rows);
                                    if (requisitionDataSet.Tables.Count > 5 && requisitionDataSet.Tables[5].Rows != null && requisitionDataSet.Tables[5].Rows.Count > 0)
                                    {
                                        FillEntitiesInRequisitionSplits(GetAllRequisitionSplitEntities(requisitionDataSet.Tables[5].Rows), req.createdBy, ref splits);
                                    }

                                    foreach (NewP2PEntities.RequisitionItem item in req.items)
                                    {
                                        item.RequisitionSource = req.RequisitionSource;
                                        item.splits = (from s in splits where s.documentItemId == item.id select s).ToList();
                                        foreach (ReqAccountingSplit split in item.splits)
                                        {
                                            split.RequisitionSource = req.RequisitionSource;
                                            split.IsAddedFromRequistion = item.IsAddedFromRequistion;
                                        }
                                    }
                                }
                            }

                            if (requisitionDataSet.Tables.Count > 6 && requisitionDataSet.Tables[6].Rows != null && requisitionDataSet.Tables[6].Rows.Count > 0)
                            {
                                List<DBNote> notes = GetAllNotesInRequisition(requisitionDataSet.Tables[6].Rows);
                                if (requisitionDataSet.Tables.Count > 7 && requisitionDataSet.Tables[7].Rows != null && requisitionDataSet.Tables[7].Rows.Count > 0)
                                    FillAttachmentsInNotes(GetAllAttachmentsInRequisition(requisitionDataSet.Tables[7].Rows), ref notes);

                                FillNotesInRequisition(notes, ref req);
                            }

                            if (requisitionDataSet.Tables.Count > 10 && requisitionDataSet.Tables[10].Rows != null && requisitionDataSet.Tables[10].Rows.Count > 0)
                                FillItemCharges(requisitionDataSet.Tables[10].Rows, ref req);

                            if (requisitionDataSet.Tables.Count > 11 && requisitionDataSet.Tables[11].Rows != null && requisitionDataSet.Tables[11].Rows.Count > 0)
                                FillItemChargesSplits(requisitionDataSet, ref req);

                            if (requisitionDataSet.Tables.Count > 13 && requisitionDataSet.Tables[13].Rows != null && requisitionDataSet.Tables[13].Rows.Count > 0)
                                FillDefaultChargeMasterDetails(requisitionDataSet.Tables[13].Rows, ref req);

                            if (requisitionDataSet.Tables.Count > 14 && requisitionDataSet.Tables[14].Rows != null && requisitionDataSet.Tables[14].Rows.Count > 0)
                                FillFormId(requisitionDataSet.Tables[14].Rows, ref req);

                            if (requisitionDataSet.Tables.Count > 15 && requisitionDataSet.Tables[15].Rows != null && requisitionDataSet.Tables[15].Rows.Count > 0)
                                FillSRFCustomAttrQuestionSetCodesForItem(requisitionDataSet.Tables[15].Rows, ref req);

                            if (requisitionDataSet.Tables.Count > 16 && requisitionDataSet.Tables[16].Rows != null && requisitionDataSet.Tables[16].Rows.Count > 0)
                                FillAdditionalFieldsData(requisitionDataSet.Tables[16].Rows, ref req);
                            if (requisitionDataSet.Tables.Count > 17 && requisitionDataSet.Tables[17].Rows != null && requisitionDataSet.Tables[17].Rows.Count > 0)
                                FillHeaderAdditionalFieldsData(requisitionDataSet.Tables[17].Rows, ref req);
                        }
                    }
                }
            }
            finally
            {
                if (objSqlCon != null && objSqlCon.State != ConnectionState.Closed)
                {
                    objSqlCon.Close();
                }
            }
            return await Task.FromResult<NewP2PEntities.Requisition>(req);
        }

        public void FillAdditionalFieldsData(DataRowCollection AdditionalFieldData, ref NewP2PEntities.Requisition req)
        {

            try
            {
                if (AdditionalFieldData != null && AdditionalFieldData.Count > 0)
                {
                    foreach (NewP2PEntities.RequisitionItem requisitionItem in req.items)
                    {

                        var lstrequisitionAdditionalItems = (from DataRow dr in AdditionalFieldData
                                                             where Convert.ToInt64(dr[ReqSqlConstants.COL_REQUISITION_ITEM_ID]) == Convert.ToInt64(requisitionItem.id)
                                                             select new
                                                             GEP.Cumulus.P2P.NewBusinessEntities.P2P.Common.P2PAdditionalFieldAtrribute
                                                             {
                                                                 AdditionalFieldID = Convert.ToInt32(dr[ReqSqlConstants.COL_ADDITIONALFIELDID]),
                                                                 AdditionalFieldCode = Convert.ToString(dr[ReqSqlConstants.COL_ADDITIONALFIELDCODE]),
                                                                 AdditionalFieldValue = Convert.ToString(dr[ReqSqlConstants.COL_ADDITIONALFIELDVALUE]),
                                                                 FeatureId = Convert.ToInt32(dr[ReqSqlConstants.COL_FEATUREID]),
                                                                 AdditionalFieldDetailCode = Convert.ToInt64(dr[ReqSqlConstants.COL_ADDITIONALFIELDDETAILCODE]),
                                                                 SourceDocumentTypeId = Convert.ToInt32(dr[ReqSqlConstants.COL_SOURCEDOCUMENTTYPEID])
                                                             }
                                               ).ToList();

                        requisitionItem.lstAdditionalFieldAttributues = lstrequisitionAdditionalItems;
                    }


                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void FillHeaderAdditionalFieldsData(DataRowCollection AdditionalFieldData, ref NewP2PEntities.Requisition req)
        {
            try
            {
                if (AdditionalFieldData != null && AdditionalFieldData.Count > 0)
                {
                    long reqid = req.id;
                    var lstrequisitionAdditionalItems = (from DataRow dr in AdditionalFieldData
                                                         where ConvertToInt64(dr, ReqSqlConstants.COL_REQUISITION_ID) == reqid
                                                         select new
                                                         GEP.Cumulus.P2P.BusinessEntities.P2PAdditionalFieldAtrribute
                                                         {
                                                             AdditionalFieldID = ConvertToInt32(dr, ReqSqlConstants.COL_ADDITIONALFIELDID),
                                                             AdditionalFieldCode = ConvertToString(dr, ReqSqlConstants.COL_ADDITIONALFIELDCODE),
                                                             AdditionalFieldValue = ConvertToString(dr, ReqSqlConstants.COL_ADDITIONALFIELDVALUE),
                                                             FeatureId = ConvertToInt32(dr, ReqSqlConstants.COL_FEATUREID),
                                                             AdditionalFieldDetailCode = ConvertToInt64(dr, ReqSqlConstants.COL_ADDITIONALFIELDDETAILCODE),
                                                           SourceDocumentTypeId = Convert.ToInt32(dr[ReqSqlConstants.COL_SOURCEDOCUMENTTYPEID])
                                                         }
                                           ).ToList();

                    req.lstAdditionalFieldAttributues = lstrequisitionAdditionalItems;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void FillSRFCustomAttrQuestionSetCodesForItem(DataRowCollection CustomAttrFormIdForItem, ref NewP2PEntities.Requisition req)
        {

            IdAndName CustomAttrQuestionSetCodeForItem = new IdAndName();
            if (CustomAttrFormIdForItem.Count > 0 && CustomAttrFormIdForItem != null)
            {
                foreach (var requisitionItem in req.items)
                {
                    var lstSRFQuestions = (from DataRow dr in CustomAttrFormIdForItem
                                           where (Int64)dr[ReqSqlConstants.COL_REQUISITION_ITEM_ID] == requisitionItem.id
                                           select new IdAndName { id = ConvertToInt64(dr, ReqSqlConstants.COL_QUESTIONNAIRECODE), name = ConvertToString(dr, ReqSqlConstants.COL_QUESTIONNAIRETITLE) })
                                          .ToList();
                    requisitionItem.CustomAttrQuestionSetCodesForItem = lstSRFQuestions;
                }
            }

        }
        public void SaveQuestionsResponse(List<QuestionResponse> lstQuestionsResponse, long docId)
        {
            LogHelper.LogInfo(Log, "SaveQuestionsResponse Method started");


            if (lstQuestionsResponse == null)
                return;
            SqlConnection objSqlCon = null;
            SqlTransaction objSqlTrans = null;
            try
            {
                Log.Debug(string.Concat("In SaveQuestionsResponse Method with parameter: lstQuestionsResponse.Count = ", lstQuestionsResponse));


                using (DataTable dtQuestionsResponse = new DataTable("QuestionsResponse"))
                {
                    dtQuestionsResponse.Locale = System.Globalization.CultureInfo.InvariantCulture;
                    dtQuestionsResponse.Columns.Add("QuestionId", typeof(long));
                    dtQuestionsResponse.Columns.Add("AssessorId", typeof(long));
                    dtQuestionsResponse.Columns.Add("AssesseeId", typeof(long));
                    dtQuestionsResponse.Columns.Add("AssessorType", typeof(int));
                    dtQuestionsResponse.Columns.Add("ObjectInstanceId", typeof(long));
                    dtQuestionsResponse.Columns.Add("RowId", typeof(long));
                    dtQuestionsResponse.Columns.Add("ColumnId", typeof(long));
                    dtQuestionsResponse.Columns.Add("ResponseValue", typeof(string));
                    dtQuestionsResponse.Columns.Add("UserComments", typeof(string));
                    dtQuestionsResponse.Columns.Add("IsDeleted", typeof(bool));
                    dtQuestionsResponse.Columns.Add("ModifiedBy", typeof(long));
                    //DataRow drEntityAssociationDetails;
                    string strCompanyName = "";
                    lstQuestionsResponse.ForEach(objQuesResp =>
                    {
                        dtQuestionsResponse.Rows.Add(objQuesResp.QuestionId, objQuesResp.AssessorId,
                                                   objQuesResp.AssesseeId, (int)objQuesResp.AssessorType,
                                                   objQuesResp.ObjectInstanceId, objQuesResp.RowId,
                                                   objQuesResp.ColumnId, objQuesResp.ResponseValue,
                                                   objQuesResp.UserComments, objQuesResp.IsDeleted, objQuesResp.ModifiedBy);
                        strCompanyName = objQuesResp.CompanyName;
                    });

                    ReliableSqlDatabase sqlHelper = null;

                    sqlHelper = ContextSqlConn;
                    objSqlCon = (SqlConnection)sqlHelper.CreateConnection();
                    objSqlCon.Open();
                    objSqlTrans = objSqlCon.BeginTransaction();

                    using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_SAVECUSTOMATTRIBRESPONSE))
                    {
                        objSqlCommand.CommandType = CommandType.StoredProcedure;
                        objSqlCommand.Parameters.Add(new SqlParameter() { ParameterName = "@QuestionResponses", SqlDbType = SqlDbType.Structured, Value = dtQuestionsResponse });
                        objSqlCommand.Parameters.Add(new SqlParameter() { ParameterName = "@documentCode", SqlDbType = SqlDbType.BigInt, Value = docId });
                        objSqlCommand.CommandTimeout = 0;

                        sqlHelper.ExecuteNonQuery(objSqlCommand, objSqlTrans);
                        if (objSqlTrans != null)
                            objSqlTrans.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                if (objSqlTrans != null)
                {
                    objSqlTrans.Rollback();
                }
                LogHelper.LogError(Log, "Error occured in SaveQuestionsResponse method. List<QuestionResponse> : " + lstQuestionsResponse, ex);
                CustomFault objCustomFault = new CustomFault("Error while Saving Questions Responses " + ex.Message, "SaveQuestionsResponse", "SaveQuestionsResponse", "SaveQuestionsResponse", ExceptionType.ApplicationException, string.Empty, false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while Saving Questions Responses" + ex.Message);
            }
            finally
            {
                if (objSqlCon != null && objSqlCon.State != ConnectionState.Closed)
                {
                    objSqlCon.Close();
                }
                Log.Info("SaveQuestionsResponse Method ended");
            }

        }
        public int SaveBuyerAssignee(long[] DocumentCodes, long BuyerAssigneeValue)
        {
            LogHelper.LogInfo(Log, "SaveBuyerAssignee Method started");
            if (DocumentCodes.Length == 0)
                return 0;
            SqlConnection objSqlCon = null;
            SqlTransaction objSqlTrans = null;
            try
            {
                using (DataTable dtDocumentCodes = new DataTable("DocumentCodes"))
                {
                    dtDocumentCodes.Locale = System.Globalization.CultureInfo.InvariantCulture;
                    dtDocumentCodes.Columns.Add("DocumentCode", typeof(long));
                    var lstDocumentCodes = DocumentCodes.ToList();
                    lstDocumentCodes.ForEach(documentcode =>
                    {
                        dtDocumentCodes.Rows.Add(documentcode);

                    });

                    Log.Debug(string.Concat("In SaveBuyerAssignee Method with parameter: DocumentCodes = ", DocumentCodes));
                    var sqlHelper = ContextSqlConn;
                    objSqlCon = (SqlConnection)sqlHelper.CreateConnection();

                    using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_SAVEBUYERASSIGNEE))
                    {
                        objSqlCommand.CommandType = CommandType.StoredProcedure;
                        objSqlCommand.Parameters.Add(new SqlParameter() { ParameterName = "@DocumentCodes", SqlDbType = SqlDbType.Structured, Value = dtDocumentCodes });
                        objSqlCommand.Parameters.Add(new SqlParameter() { ParameterName = "@BuyerAssignee", SqlDbType = SqlDbType.BigInt, Value = BuyerAssigneeValue });
                        objSqlCommand.Parameters.Add(new SqlParameter() { ParameterName = "@CreatedBy", SqlDbType = SqlDbType.BigInt, Value = UserContext.ContactCode });
                        objSqlCommand.CommandTimeout = 0;
                        objSqlCon.Open();
                        objSqlTrans = objSqlCon.BeginTransaction();
                        var Result = sqlHelper.ExecuteNonQuery(objSqlCommand, objSqlTrans);
                        objSqlTrans.Commit();

                        AddIntoSearchIndexerQueueing(lstDocumentCodes, REQ, UserContext, GepConfiguration);
                        return Result;
                    }
                }
            }
            catch (Exception ex)
            {
                if (objSqlTrans != null)
                {
                    objSqlTrans.Rollback();
                }
                LogHelper.LogError(Log, "Error occured in SaveBuyerAssignee method. with parameter: DocumentCodes = " + DocumentCodes.ToJSON(), ex);
                CustomFault objCustomFault = new CustomFault("Error while SaveBuyerAssignee" + ex.Message, "SaveBuyerAssignee", "SaveBuyerAssignee", "SaveBuyerAssignee",

ExceptionType.ApplicationException, string.Empty, false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while SaveBuyerAssignee" + ex.Message);
            }
            finally
            {
                if (objSqlCon != null && objSqlCon.State != ConnectionState.Closed)
                {
                    objSqlCon.Close();
                }
                Log.Info("SaveBuyerAssignee Method ended");
            }

        }
        /// <summary>
        /// save complete req json obj
        /// </summary>
        /// <param name="ObjectData"></param>
        /// <param name="DocumentCode"></param>
        /// <param name="DocumentTypeCode"></param>
        /// <param name="LastModifiedBy"></param>
        /// <returns></returns>
        public SaveResult AutoSaveDocument(NewP2PEntities.Requisition ObjectData, Int64 DocumentCode, int DocumentTypeCode, Int64 LastModifiedBy)
        {

            SqlConnection sqlCon = null;
            SqlTransaction sqlTrans = null;
            bool result;
            try
            {
                LogHelper.LogInfo(Log, String.Format("AutoSaveObject Method Started for DocumentCode={0}, DocumentTypeCode={1}, LastModifiedBy={2}", DocumentCode, DocumentTypeCode, LastModifiedBy));

                sqlCon = (SqlConnection)ContextSqlConn.CreateConnection();
                sqlCon.Open();
                sqlTrans = sqlCon.BeginTransaction();
                result = Convert.ToBoolean(ContextSqlConn.ExecuteNonQuery(sqlTrans, ReqSqlConstants.usp_DM_AutoSaveDocument, JSONHelper.ToJSON(ObjectData), DocumentCode, DocumentTypeCode, LastModifiedBy, DateTime.Now));
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
            }
            return new SaveResult { success = result };
        }

        /// <summary>
        /// gets complete req json data
        /// </summary>
        /// <param name="id"></param>
        /// <param name="DocTypeCode"></param>
        /// <returns></returns>
        public NewP2PEntities.Requisition GetAutoSaveDocument(Int64 id, int DocTypeCode)
        {
            SqlConnection sqlCon = null;
            SqlTransaction sqlTrans = null;
            NewP2PEntities.Requisition req = null;
            RefCountingDataReader objRefCountingDataReader = null;
            var sqlHelper = ContextSqlConn;
            try
            {
                LogHelper.LogInfo(Log, String.Format("GetAutoSaveObject Method Started for id={0}, DocTypeCode={1}", id, DocTypeCode));

                sqlCon = (SqlConnection)ContextSqlConn.CreateConnection();
                sqlCon.Open();
                sqlTrans = sqlCon.BeginTransaction();
                objRefCountingDataReader = (RefCountingDataReader)sqlHelper.ExecuteReader(ReqSqlConstants.usp_DM_GetAutoSaveDocument,
                    new object[] { id, DocTypeCode });

                if (objRefCountingDataReader != null)
                {
                    try
                    {
                        var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;
                        while (sqlDr.Read())
                        {
                            req = Gep.Cumulus.CSM.Extensions.JSONHelper.DeserializeObj<NewP2PEntities.Requisition>(GetStringValue(sqlDr, "DocumentData"));
                            req.lastModifiedOn = GetDateTimeValue(sqlDr, "DateModified");
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
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
            }
            return req;
        }

        /// <summary>
        /// Get User's saved Configurations for header & Grid
        /// </summary>
        /// <param name="contactCode">contactCode</param>
        /// <param name="documentType">documentType</param>
        /// <returns></returns>
        public async Task<List<P2PUserConfiguration>> GetUserConfigurationsDetails(long contactCode, int documentType)
        {
            List<P2PUserConfiguration> lstUserConfig = new List<P2PUserConfiguration>();

            P2PUserConfiguration userConfig = null;
            RefCountingDataReader objRefCountingDataReader = null;
            try
            {
                LogHelper.LogInfo(Log, String.Format("GetUserConfigurations Method Started for contactCode={0}, documentType={1}", contactCode, documentType));

                objRefCountingDataReader = (RefCountingDataReader)ContextSqlConn.ExecuteReader(ReqSqlConstants.USP_P2P_GETUSERCONFIGURATIONS,
                    new object[] { contactCode, documentType });

                if (objRefCountingDataReader != null)
                {
                    try
                    {
                        var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;
                        while (sqlDr.Read())
                        {
                            userConfig = new P2PUserConfiguration();
                            userConfig.ContactCode = GetLongValue(sqlDr, ReqSqlConstants.COL_CONTACTCODE);
                            userConfig.DocumentType = GetIntValue(sqlDr, ReqSqlConstants.COL_DOCUMENT_TYPE);
                            userConfig.ConfigType = (ConfigType)GetIntValue(sqlDr, ReqSqlConstants.COL_CONFIGTYPE);
                            userConfig.ConfigDetails = GetStringValue(sqlDr, ReqSqlConstants.COL_CONFIGDETAILS);
                            lstUserConfig.Add(userConfig);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.LogError(Log, "Error occured in GetUserConfigurations method.", ex);

                        var objCustomFault = new CustomFault(ex.Message, "GetUserConfigurations", "GetUserConfigurations",
                                                                "Common", ExceptionType.ApplicationException,
                                                                contactCode.ToString(CultureInfo.InvariantCulture), false);
                        throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                                "Error while GetUserConfigurations " + ex.Message + " Stack Trace: " + ex.StackTrace + "Inner Exception: " + ex.InnerException);
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
            }

            return await Task.FromResult<List<P2PUserConfiguration>>(lstUserConfig);
        }

        /// <summary>
        /// Get User's saved Configurations for header & Grid
        /// </summary>
        /// <param name="contactCode">contactCode</param>
        /// <param name="documentType">documentType</param>
        /// <returns></returns>
        public List<P2PUserConfiguration> GetUserConfigurations(long contactCode, int documentType)
        {
            List<P2PUserConfiguration> lstUserConfig = new List<P2PUserConfiguration>();
            SqlConnection sqlCon = null;
            P2PUserConfiguration userConfig = null;
            RefCountingDataReader objRefCountingDataReader = null;
            var sqlHelper = ContextSqlConn;
            try
            {
                LogHelper.LogInfo(Log, String.Format("GetUserConfigurations Method Started for contactCode={0}, documentType={1}", contactCode, documentType));

                sqlCon = (SqlConnection)ContextSqlConn.CreateConnection();
                sqlCon.Open();
                objRefCountingDataReader = (RefCountingDataReader)sqlHelper.ExecuteReader(ReqSqlConstants.USP_P2P_GETUSERCONFIGURATIONS,
                    new object[] { contactCode, documentType });

                if (objRefCountingDataReader != null)
                {
                    try
                    {
                        var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;
                        while (sqlDr.Read())
                        {
                            userConfig = new P2PUserConfiguration();
                            userConfig.ContactCode = GetLongValue(sqlDr, ReqSqlConstants.COL_CONTACTCODE);
                            userConfig.DocumentType = GetIntValue(sqlDr, ReqSqlConstants.COL_DOCUMENT_TYPE);
                            userConfig.ConfigType = (ConfigType)GetIntValue(sqlDr, ReqSqlConstants.COL_CONFIGTYPE);
                            userConfig.ConfigDetails = GetStringValue(sqlDr, ReqSqlConstants.COL_CONFIGDETAILS);
                            lstUserConfig.Add(userConfig);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.LogError(Log, "Error occured in GetUserConfigurations method.", ex);

                        var objCustomFault = new CustomFault(ex.Message, "GetUserConfigurations", "GetUserConfigurations",
                                                             "Common", ExceptionType.ApplicationException,
                                                             contactCode.ToString(CultureInfo.InvariantCulture), false);
                        throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                              "Error while GetUserConfigurations " + ex.Message + " Stack Trace: " + ex.StackTrace + "Inner Exception: " + ex.InnerException);
                    }
                }
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
            }
            return lstUserConfig;
        }


        /// <summary>
        /// Save User Configuration for header & grid
        /// </summary>
        /// <param name="userConfig">userConfig</param>
        /// <returns></returns>
        public SaveResult SaveUserConfigurations(P2PUserConfiguration userConfig)
        {
            SqlConnection sqlCon = null;
            SqlTransaction sqlTrans = null;
            bool result;
            try
            {
                LogHelper.LogInfo(Log, String.Format("SaveUserConfigurations Method Started for userConfig={0}", userConfig));

                sqlCon = (SqlConnection)ContextSqlConn.CreateConnection();
                sqlCon.Open();
                sqlTrans = sqlCon.BeginTransaction();
                result = Convert.ToBoolean(ContextSqlConn.ExecuteNonQuery(sqlTrans, ReqSqlConstants.USP_P2P_SAVEUSERCONFIGURATIONS, userConfig.ContactCode, userConfig.DocumentType, userConfig.ConfigType, userConfig.ConfigDetails));
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
                            LogHelper.LogError(Log, "Error occured in SaveUserConfigurations method.", ex);

                            var objCustomFault = new CustomFault(ex.Message, "SaveUserConfigurations", "SaveUserConfigurations",
                                                                 "Common", ExceptionType.ApplicationException,
                                                                 userConfig.ContactCode.ToString(CultureInfo.InvariantCulture), false);
                            throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                                  "Error while SaveUserConfigurations " + ex.Message + " Stack Trace: " + ex.StackTrace + "Inner Exception: " + ex.InnerException);
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
                        LogHelper.LogError(Log, "Error occured in SaveUserConfigurations method.", ex);

                        var objCustomFault = new CustomFault(ex.Message, "SaveUserConfigurations", "SaveUserConfigurations",
                                                             "Common", ExceptionType.ApplicationException,
                                                             userConfig.ContactCode.ToString(CultureInfo.InvariantCulture), false);
                        throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                              "Error while SaveUserConfigurations " + ex.Message + " Stack Trace: " + ex.StackTrace + "Inner Exception: " + ex.InnerException);
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
            return new SaveResult { success = result };
        }
        /// <summary>
        /// Get Contract Items by documentNumber
        /// </summary>
        /// <param name="documentNumber">documentNumber</param>
        /// <returns></returns>
        public List<IdAndName> GetContractItemsByContractNumber(string documentNumber, string term, int itemType)
        {
            List<IdAndName> lstIdAndName = new List<IdAndName>();
            SqlConnection sqlCon = null;
            IdAndName ContractDetails = null;
            RefCountingDataReader objRefCountingDataReader = null;
            var sqlHelper = ContextSqlConn;
            try
            {
                LogHelper.LogInfo(Log, String.Format("GetContractItemsByContractNumber Method Started for documentNumber={0}", documentNumber));

                sqlCon = (SqlConnection)ContextSqlConn.CreateConnection();
                sqlCon.Open();
                objRefCountingDataReader = (RefCountingDataReader)sqlHelper.ExecuteReader(ReqSqlConstants.USP_P2P_GetContractItemsByContractNumber,
                    new object[] { documentNumber, term, itemType });

                if (objRefCountingDataReader != null)
                {
                    try
                    {
                        var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;
                        while (sqlDr.Read())
                        {
                            ContractDetails = new IdAndName();
                            ContractDetails.id = GetLongValue(sqlDr, ReqSqlConstants.COL_ID);
                            ContractDetails.name = GetStringValue(sqlDr, ReqSqlConstants.COL_NAME);
                            lstIdAndName.Add(ContractDetails);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.LogError(Log, "Error occured in GetContractItemsByContractNumber method.", ex);

                        var objCustomFault = new CustomFault(ex.Message, "GetContractItemsByContractNumber", "GetContractItemsByContractNumber",
                                                             "Common", ExceptionType.ApplicationException,
                                                             documentNumber, false);
                        throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                              "Error while GetContractItemsByContractNumber " + ex.Message + " Stack Trace: " + ex.StackTrace + "Inner Exception: " + ex.InnerException);
                    }
                }
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
            }
            return lstIdAndName;
        }

        /// <summary>
        /// Validate Contract Number
        /// </summary>
        /// <param name="documentNumber">ContractNumber</param>
        /// <returns></returns>
        public bool ValidateContractNumber(string contractNumber)
        {
            SqlConnection sqlCon = null;
            SqlTransaction sqlTrans = null;
            bool result;
            try
            {
                LogHelper.LogInfo(Log, String.Format("Validate Contract Number={0}", contractNumber));

                sqlCon = (SqlConnection)ContextSqlConn.CreateConnection();
                sqlCon.Open();
                sqlTrans = sqlCon.BeginTransaction();
                // result = Convert.ToBoolean(ContextSqlConn.ExecuteNonQuery(sqlTrans, ReqSqlConstants.USP_P2P_REQ_ValidateContractNumber, contractNumber));
                result = Convert.ToBoolean(ContextSqlConn.ExecuteScalar(ReqSqlConstants.USP_P2P_REQ_ValidateContractNumber, contractNumber), NumberFormatInfo.InvariantInfo);
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
                            LogHelper.LogError(Log, "Error occured in ValidateContractNumber method.", ex);

                            var objCustomFault = new CustomFault(ex.Message, "ValidateContractNumber", "ValidateContractNumber",
                                                                 "Common", ExceptionType.ApplicationException,
                                                                 contractNumber, false);
                            throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                                  "Error while ValidateContractNumber " + ex.Message + " Stack Trace: " + ex.StackTrace + "Inner Exception: " + ex.InnerException);
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
                        LogHelper.LogError(Log, "Error occured in ValidateContractNumber method.", ex);

                        var objCustomFault = new CustomFault(ex.Message, "ValidateContractNumber", "ValidateContractNumber",
                                                             "Common", ExceptionType.ApplicationException,
                                                             contractNumber, false);
                        throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                              "Error while ValidateContractNumber " + ex.Message + " Stack Trace: " + ex.StackTrace + "Inner Exception: " + ex.InnerException);
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

        /// <summary>
        /// Validate Contract Item Id
        /// </summary>
        /// <param name="contractNumber">contractNumber</param>
        /// <param name="ContractItemId">ContractItemId</param>
        /// <returns></returns>
        public bool ValidateContractItemId(string contractNumber, long ContractItemId)
        {
            SqlConnection sqlCon = null;
            bool result;
            try
            {
                LogHelper.LogInfo(Log, String.Format("Validate Contract Item Id={0}", ContractItemId));

                sqlCon = (SqlConnection)ContextSqlConn.CreateConnection();
                sqlCon.Open();
                result = Convert.ToBoolean(ContextSqlConn.ExecuteScalar(ReqSqlConstants.USP_P2P_REQ_VALIDATEBLANKETITEMNUMBER, contractNumber, ContractItemId), NumberFormatInfo.InvariantInfo);

            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in ValidateContractItemId method.", ex);

                var objCustomFault = new CustomFault(ex.Message, "ValidateContractItemId", "ValidateContractItemId",
                                                     "Common", ExceptionType.ApplicationException,
                                                     contractNumber, false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while ValidateContractItemId " + ex.Message + " Stack Trace: " + ex.StackTrace + "Inner Exception: " + ex.InnerException);
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

        #endregion
        public List<SavedViewDetails> GetSavedViewsForReqWorkBench(long contactCode, Int16 documentTypeCode, long LobId)
        {
            List<SavedViewDetails> lstsavedViewDetails = new List<SavedViewDetails>();
            SqlConnection sqlCon = null;
            RefCountingDataReader objRefCountingDataReader = null;
            var sqlHelper = ContextSqlConn;
            try
            {
                LogHelper.LogInfo(Log, String.Format("GetSavedViewsForReqWorkBench Method Started for contractCode={0},documentTypeCode={1}", contactCode, documentTypeCode));

                sqlCon = (SqlConnection)ContextSqlConn.CreateConnection();
                sqlCon.Open();
                objRefCountingDataReader = (RefCountingDataReader)sqlHelper.ExecuteReader(ReqSqlConstants.USP_P2P_REQ_GETSAVEDVIEWS_WORKBENCH,
                    new object[] { contactCode, documentTypeCode, LobId });

                if (objRefCountingDataReader != null)
                {
                    try
                    {
                        var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;
                        while (sqlDr.Read())
                        {
                            lstsavedViewDetails.Add(new SavedViewDetails
                            {
                                ViewId = GetLongValue(sqlDr, ReqSqlConstants.COL_REQ_SAVEDVIEW_ID),
                                ContactCode = GetLongValue(sqlDr, ReqSqlConstants.COL_CONTACT_CODE),
                                ViewName = GetStringValue(sqlDr, ReqSqlConstants.COL_REQ_SAVEDVIEW_NAME),
                                ColumnList = GetStringValue(sqlDr, ReqSqlConstants.COL_REQ_SAVEDVIEW_COLUMNLIST),
                                Filters = GetStringValue(sqlDr, ReqSqlConstants.COL_REQ_SAVEDVIEW_FILTERS),
                                SortColumn = GetStringValue(sqlDr, ReqSqlConstants.COL_REQ_SAVEDVIEW_SORTCOLUMN),
                                SortOrder = GetStringValue(sqlDr, ReqSqlConstants.COL_REQ_SAVEDVIEW_SORTORDER),
                                GroupColumn = GetStringValue(sqlDr, ReqSqlConstants.COL_REQ_SAVEDVIEW_GROUPCOLUMN),
                                IsDefaultView = GetBoolValue(sqlDr, ReqSqlConstants.COL_REQ_SAVEDVIEW_ISDEFAULTVIEW),
                                IsSystemDefault = GetBoolValue(sqlDr, ReqSqlConstants.COL_REQ_SAVEDVIEW_ISSYSTEMDEFAULT),
                                DocumentTypeCode = documentTypeCode,
                                LobId = GetLongValue(sqlDr, ReqSqlConstants.COL_REQ_SAVEDVIEW_LOBID)
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.LogError(Log, "Error occured in GetSavedViewsForReqWorkBench method.", ex);

                        var objCustomFault = new CustomFault(ex.Message, "GetSavedViewsForReqWorkBench", "GetSavedViewsForReqWorkBench",
                                                             "Common", ExceptionType.ApplicationException,
                                                             "", false);
                        throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                              "Error while GetSavedViewsForReqWorkBench " + ex.Message + " Stack Trace: " + ex.StackTrace + "Inner Exception: " + ex.InnerException);
                    }
                }
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
            }
            return lstsavedViewDetails;
        }
        public long InsertUpdateSavedViewsForReqWorkBench(SavedViewDetails objSavedView)
        {
            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;
            long SavedViewInfoId = 0;
            SqlDataReader sqlDr = null;
            try
            {
                LogHelper.LogInfo(Log, String.Format("Insert Update Saved Views For Req WorkBench={0}", objSavedView));
                var sqlHelper = ContextSqlConn;
                _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();

                sqlDr = (SqlDataReader)sqlHelper.ExecuteReader(_sqlTrans, ReqSqlConstants.USP_P2P_REQ_INSERTUPDATESAVEDVIEWS_WORKBENCH,
                                    objSavedView.ViewId
                                    , objSavedView.ContactCode == 1 ? UserContext.ContactCode : objSavedView.ContactCode
                                    , objSavedView.DocumentTypeCode
                                    , objSavedView.ViewName
                                    , objSavedView.ColumnList
                                    , objSavedView.Filters
                                    , objSavedView.SortColumn
                                    , objSavedView.SortOrder
                                    , objSavedView.GroupColumn
                                    , objSavedView.IsDefaultView
                                    , objSavedView.LobId);

                if (sqlDr != null)
                {
                    if (sqlDr.Read())
                    {
                        SavedViewInfoId = GetLongValue(sqlDr, ReqSqlConstants.COL_REQ_SAVEDVIEW_ID);
                    }
                    sqlDr.Close();
                }
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
                    catch (InvalidOperationException ex)
                    {
                        LogHelper.LogError(Log, "Error occured in InsertUpdateSavedViewsForReqWorkBench method.", ex);

                        var objCustomFault = new CustomFault(ex.Message, "InsertUpdateSavedViewsForReqWorkBench", "InsertUpdateSavedViewsForReqWorkBench",
                                                             "Common", ExceptionType.ApplicationException,
                                                             objSavedView.ViewName, false);
                        throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                              "Error while ValidateContractNumber " + ex.Message + " Stack Trace: " + ex.StackTrace + "Inner Exception: " + ex.InnerException);
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
            }
            return SavedViewInfoId;
        }

        public bool PushRequisitionToInterface(long requisitionId)
        {
            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;
            bool result = false;
            try
            {
                LogHelper.LogInfo(Log, String.Format("PushRequisitionToInterface DAO requisitionId={0}", requisitionId));
                long lobEntityDetailCode = GetDocumentLOBByDocumentCode(requisitionId)?.EntityDetailCode ?? 0;
                var sqlHelper = ContextSqlConn;
                _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();

                result = Convert.ToBoolean(sqlHelper.ExecuteNonQuery(_sqlTrans, ReqSqlConstants.USP_P2P_PUSHREQUISITIONTOINTERFACE, requisitionId, lobEntityDetailCode), NumberFormatInfo.InvariantInfo);
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
                    catch (InvalidOperationException ex)
                    {
                        LogHelper.LogError(Log, "Error occured in PushRequisitionToInterface method.", ex);
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
            }
            return result;

        }

        public bool DeleteSavedViewsForReqWorkBench(long savedViewId)
        {
            SqlConnection sqlCon = null;
            SqlTransaction sqlTrans = null;
            bool result;
            try
            {
                LogHelper.LogInfo(Log, String.Format("Delete Saved Views For Req WorkBench={0}", savedViewId));

                sqlCon = (SqlConnection)ContextSqlConn.CreateConnection();
                sqlCon.Open();
                sqlTrans = sqlCon.BeginTransaction();
                result = Convert.ToBoolean(ContextSqlConn.ExecuteNonQuery(ReqSqlConstants.USP_P2P_REQ_DELETEAVEDVIEWS_WORKBENCH, savedViewId), NumberFormatInfo.InvariantInfo);
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
                            LogHelper.LogInfo(Log, String.Format("DeleteSavedViewsForReqWorkBench Method Started for savedViews", savedViewId));

                            var objCustomFault = new CustomFault(ex.Message, "DeleteSavedViewsForReqWorkBench", "DeleteSavedViewsForReqWorkBench",
                                                                 "Common", ExceptionType.ApplicationException,
                                                                 savedViewId.ToString(), false);
                            throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                                  "Error while DeleteSavedViewsForReqWorkBench " + ex.Message + " Stack Trace: " + ex.StackTrace + "Inner Exception: " + ex.InnerException);
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
                        LogHelper.LogError(Log, "Error occured in DeleteSavedViewsForReqWorkBench method.", ex);

                        var objCustomFault = new CustomFault(ex.Message, "DeleteSavedViewsForReqWorkBench", "DeleteSavedViewsForReqWorkBench",
                                                             "Common", ExceptionType.ApplicationException,
                                                             savedViewId.ToString(), false);
                        throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                              "Error while DeleteSavedViewsForReqWorkBench " + ex.Message + " Stack Trace: " + ex.StackTrace + "Inner Exception: " + ex.InnerException);
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
        public bool AssignBuyerToRequisitionItems(long buyerContactCode, string requisitionItemIds)
        {
            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;
            bool result = false;
            try
            {
                LogHelper.LogInfo(Log, String.Format("Assign Buyer To Requisition Items,buyerContactCode={0},requisitionItemIds={1}", buyerContactCode, requisitionItemIds));
                var sqlHelper = ContextSqlConn;
                _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();

                result = Convert.ToBoolean(sqlHelper.ExecuteNonQuery(_sqlTrans, ReqSqlConstants.USP_P2P_REQ_ASSIGNBUYERTOREQUISITIONITEMS,
                                  buyerContactCode, requisitionItemIds), NumberFormatInfo.InvariantInfo);
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
                    catch (InvalidOperationException ex)
                    {
                        LogHelper.LogError(Log, "Error occured in AssignBuyerToRequisitionItems method.", ex);

                        var objCustomFault = new CustomFault(ex.Message, "AssignBuyerToRequisitionItems", "AssignBuyerToRequisitionItems",
                                                             "Common", ExceptionType.ApplicationException,
                                                             requisitionItemIds + " " + buyerContactCode.ToString(), false);
                        throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                              "Error while AssignBuyerToRequisitionItems " + ex.Message + " Stack Trace: " + ex.StackTrace + "Inner Exception: " + ex.InnerException);
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
            }
            return result;
        }

        public List<BuyerInfo> GetAssignBuyersList(string organizationEntityIds, string documentCodes)
        {
            List<BuyerInfo> lstBuyerInfo = new List<BuyerInfo>();
            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;
            try
            {
                LogHelper.LogInfo(Log, "Order GetAssignBuyersList Method Started for organizationEntityIds :" + organizationEntityIds + "documentCodes :" + documentCodes);

                var sqlHelper = ContextSqlConn;
                _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();

                List<long> lstdoccodes = documentCodes.Split(',').Select(long.Parse).ToList();
                List<long> lstorgentityids = organizationEntityIds.Split(',').Select(long.Parse).ToList();

                DataTable dtorganizationEntityId = new DataTable() { Locale = CultureInfo.InvariantCulture, TableName = ReqSqlConstants.TVP_ORG_ENTITYIDS };
                dtorganizationEntityId.Columns.Add("EntityCode", typeof(long));
                foreach (var organizationEntityId in lstorgentityids)
                {
                    DataRow dr = dtorganizationEntityId.NewRow();
                    dr["EntityCode"] = organizationEntityId;
                    dtorganizationEntityId.Rows.Add(dr);
                }

                DataTable dtdocumentCodes = new DataTable() { Locale = CultureInfo.InvariantCulture, TableName = ReqSqlConstants.TVP_P2P_DOCUMENTCODE };
                dtdocumentCodes.Columns.Add("DocumentCode", typeof(long));
                foreach (var documentCode in lstdoccodes)
                {
                    DataRow dr = dtdocumentCodes.NewRow();
                    dr["DocumentCode"] = documentCode;
                    dtdocumentCodes.Rows.Add(dr);
                }


                using (var objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_GETLISTOFASSIGNBUYERS, _sqlCon))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.Add(new SqlParameter("@tvpOrgEntity", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_ORG_ENTITYIDS,
                        Value = dtorganizationEntityId
                    });
                    objSqlCommand.Parameters.Add(new SqlParameter("@tvp_P2P_DocumentCode", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_P2P_DOCUMENTCODE,
                        Value = dtdocumentCodes
                    });
                    var sqlDr = (SqlDataReader)sqlHelper.ExecuteReader(objSqlCommand, _sqlTrans);
                    if (sqlDr != null)
                    {
                        while (sqlDr.Read())
                        {
                            //BuyerInfo obj = new BuyerInfo();
                            //obj.ContactCode = GetStringValue(sqlDr, ReqSqlConstants.COL_CONTACT_CODE);
                            //obj.BuyerName = GetStringValue(sqlDr, ReqSqlConstants.COL_USERNAME);
                            //obj.DocumentCode = GetStringValue(sqlDr, ReqSqlConstants.COL_DOCUMENT_CODE);
                            //obj.OrgEntityCode = GetStringValue(sqlDr, ReqSqlConstants.COL_ORG_ENTITYCODE);
                            //lstBuyerInfo.Add(obj);

                            lstBuyerInfo.Add(new BuyerInfo()
                            {
                                ContactCode = GetStringValue(sqlDr, ReqSqlConstants.COL_CONTACT_CODE),
                                BuyerName = GetStringValue(sqlDr, ReqSqlConstants.COL_USERNAME),
                                DocumentCode = GetStringValue(sqlDr, ReqSqlConstants.COL_DOCUMENT_CODE),
                                OrgEntityCode = GetStringValue(sqlDr, ReqSqlConstants.COL_ORG_ENTITYCODE)
                            });
                        }
                        sqlDr.Close();
                    }
                }
                if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
                    _sqlTrans.Commit();
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
                LogHelper.LogInfo(Log, "Order GetAssignBuyersList Method Ended for organizationEntityIds :" + organizationEntityIds + "documentCodes :" + documentCodes);
            }

            return lstBuyerInfo;

        }
        /// <summary>
        /// Validate Contract Number
        /// </summary>
        /// <param name="documentNumber">ContractNumber</param>
        /// <returns></returns>
        public List<KeyValuePair<string, string>> ValidateReqWorkbenchItems(string reqItemIds, byte validationType, bool allowOneShipToLoation, bool showRemito, bool allowDeliverToFreeText)
        {
            DataSet requisitionDataSet = new DataSet();
            List<KeyValuePair<string, string>> lsterrors = new List<KeyValuePair<string, string>>();
            try
            {
                LogHelper.LogInfo(Log, String.Format("ValidateReqWorkbenchItems={0}", reqItemIds));

                requisitionDataSet = ContextSqlConn.ExecuteDataSet(ReqSqlConstants.USP_P2P_REQ_VALIDATEREQWORKBENCHITEMS, new object[] { reqItemIds, validationType, allowOneShipToLoation, showRemito, allowDeliverToFreeText });
                if (requisitionDataSet.Tables[0].Rows != null && requisitionDataSet.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dr in requisitionDataSet.Tables[0].Rows)
                    {
                        lsterrors.Add(new KeyValuePair<string, string>(ConvertToString(dr, ReqSqlConstants.COL_REQUISITION_ITEM_ID), ConvertToString(dr, ReqSqlConstants.COL_ERRORSTRING)));
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in ValidateReqWorkbenchItems method.", ex);

                var objCustomFault = new CustomFault(ex.Message, "ValidateReqWorkbenchItems", "ValidateReqWorkbenchItems",
                                                     "Common", ExceptionType.ApplicationException,
                                                     reqItemIds, false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while ValidateReqWorkbenchItems " + ex.Message + " Stack Trace: " + ex.StackTrace + "Inner Exception: " + ex.InnerException);
                throw;
            }

            return lsterrors;
        }

        private void FillItemCharges(DataRowCollection rows, ref NewP2PEntities.Requisition Req)
        {

            List<NewP2PEntities.ItemCharge> ItemCharges = new List<NewP2PEntities.ItemCharge>();

            foreach (DataRow sqlDr in rows)
            {
                NewP2PEntities.ItemCharge objItemCharge = new NewP2PEntities.ItemCharge();
                objItemCharge.ItemChargeId = ConvertToInt64(sqlDr, ReqSqlConstants.COL_ITEMCHARGEID);
                objItemCharge.LineNumber = ConvertToInt64(sqlDr, ReqSqlConstants.COL_LINENUMBER);
                objItemCharge.CalculationValue = ConvertToDecimal(sqlDr, ReqSqlConstants.COL_CALCULATIONVALUE);
                objItemCharge.AdditionInfo = ConvertToString(sqlDr, ReqSqlConstants.COL_ADDITIONALINFO);
                objItemCharge.ChargeAmount = ConvertToDecimal(sqlDr, ReqSqlConstants.COL_CHARGEAMOUNT);
                objItemCharge.P2PLineItemID = ConvertToInt64(sqlDr, ReqSqlConstants.COL_P2P_LINE_ITEM_ID);
                objItemCharge.IsHeaderLevelCharge = ConvertToBoolean(sqlDr, ReqSqlConstants.COL_ISHEADERLEVELCHARGE);
                objItemCharge.DocumentCode = ConvertToInt64(sqlDr, ReqSqlConstants.COL_DOCUMENTCODE);
                objItemCharge.CreatedBy = ConvertToInt64(sqlDr, ReqSqlConstants.COL_CREATEDBY);
                objItemCharge.ChargeDetails = new NewP2PEntities.ChargeMaster
                {
                    ChargeMasterId = ConvertToInt64(sqlDr, ReqSqlConstants.COL_CHARGEMASTERID),
                    ChargeName = ConvertToString(sqlDr, ReqSqlConstants.COL_CHARGENAME),
                    ChargeDescription = ConvertToString(sqlDr, ReqSqlConstants.COL_CHARGEDESCRIPTION),
                    CalculationBasisId = ConvertToInt16(sqlDr, ReqSqlConstants.COL_CALCULATIONBASISID),
                    CalculationBasis = ConvertToString(sqlDr, ReqSqlConstants.COL_NAME),
                    IsIncludeForRetainage = ConvertToBoolean(sqlDr, ReqSqlConstants.COL_ISINCLUDEFORRETAINAGE),
                    IsIncludeForTax = ConvertToBoolean(sqlDr, ReqSqlConstants.COL_ISINCLUDEFORTAX),
                    TolerancePercentage = ConvertToDecimal(sqlDr, ReqSqlConstants.COL_TOLERANCEPERCENTAGE),
                    IsAllowance = ConvertToBoolean(sqlDr, ReqSqlConstants.COL_ISALLOWANCE),
                    IsEditableOnInvoice = ConvertToBoolean(sqlDr, ReqSqlConstants.COL_ISEDITABLEONINVOICE),
                    ChargeTypeName = ConvertToString(sqlDr, ReqSqlConstants.COL_CHARGETYPENAME),
                    ChargeTypeCode = ConvertToInt32(sqlDr, ReqSqlConstants.COL_CHARGETYPECODE)
                };
                objItemCharge.IsDeleted = false;
                ItemCharges.Add(objItemCharge);
            }
            Req.ItemChargesForHeader = (from itemcharges in ItemCharges where itemcharges.IsHeaderLevelCharge == true select itemcharges).ToList();
            if (!ReferenceEquals(Req.items, null) && Req.items.Count > 0)
            {
                foreach (NewP2PEntities.RequisitionItem item in Req.items)
                    item.ItemChargesForSubLine = (from itemcharges in ItemCharges where itemcharges.IsHeaderLevelCharge == false && itemcharges.P2PLineItemID == item.p2PLineItemId select itemcharges).ToList();
            }
        }
        private void FillItemChargesSplits(DataSet requisitionDataSet, ref NewP2PEntities.Requisition Req)
        {
            if (requisitionDataSet.Tables.Count > 11 && requisitionDataSet.Tables[11].Rows != null && requisitionDataSet.Tables[11].Rows.Count > 0)
            {
                List<ReqAccountingSplit> chargeSplits = GetAllRequisitionSplits(requisitionDataSet.Tables[11].Rows);
                if (requisitionDataSet.Tables.Count > 12 && requisitionDataSet.Tables[12].Rows != null && requisitionDataSet.Tables[12].Rows.Count > 0)
                    FillEntitiesInReqSplits(GetAllRequisitionSplitEntities(requisitionDataSet.Tables[12].Rows), Req.createdBy, ref chargeSplits);

                if (!ReferenceEquals(Req.ItemChargesForHeader, null) && Req.ItemChargesForHeader.Count > 0)
                {
                    foreach (NewP2PEntities.ItemCharge itemCharge in Req.ItemChargesForHeader)
                        itemCharge.Reqsplits = (from s in chargeSplits where s.documentItemId == itemCharge.ItemChargeId select s).ToList();
                }
                if (!ReferenceEquals(Req.items, null) && Req.items.Count > 0)
                {
                    foreach (NewP2PEntities.RequisitionItem item in Req.items)
                    {
                        if (!ReferenceEquals(item.ItemChargesForSubLine, null) && item.ItemChargesForSubLine.Count > 0)
                        {
                            foreach (NewP2PEntities.ItemCharge itemCharge in item.ItemChargesForSubLine)
                                itemCharge.Reqsplits = (from s in chargeSplits where s.documentItemId == itemCharge.ItemChargeId select s).ToList();
                        }
                    }
                }
            }
        }
        private void FillEntitiesInReqSplits(List<DBSplitEntity> splitEntities, IdAndName requester, ref List<ReqAccountingSplit> splits)
        {
            foreach (ReqAccountingSplit split in splits)
            {
                split.SplitEntities = new List<DBSplitEntity>();
                List<DBSplitEntity> splitEnts = (from se in splitEntities where se.splitItemId == split.id select se).ToList();
                //Below code is commented because we are implementing Dynamic spliit entities(REQ-4496)
                // Int16 counter = 1;
                foreach (DBSplitEntity splitEnt in splitEnts)
                {
                    if (splitEnt.title.ToUpper() == REQUESTER)
                        split.requester = new SplitEntity
                        {
                            code = splitEnt.code == String.Empty ? (requester == null ? String.Empty : requester.id.ToString()) : splitEnt.code,
                            name = splitEnt.name == String.Empty ? (requester == null ? String.Empty : requester.name) : splitEnt.name,
                            fieldId = splitEnt.fieldId,
                            splitEntityId = splitEnt.splitEntityId
                        };
                    else
                    {
                        SplitEntity newEnt = new SplitEntity
                        {
                            code = splitEnt.code,
                            entityCode = splitEnt.entityCode,
                            name = splitEnt.name,
                            entityType = splitEnt.entityType,
                            fieldId = splitEnt.fieldId,
                            splitEntityId = splitEnt.splitEntityId,
                            title = splitEnt.title
                        };
                        if (splitEnt.title.ToUpper() == GL_CODE)
                            split.gLCode = newEnt;
                        else if (splitEnt.title.ToUpper() == PERIOD && splitEnt.entityType == 0)
                        {
                            split.period = newEnt;
                        }
                        else
                        {
                            split.SplitEntities.Add(splitEnt);

                            /*Below code is commented because we are implementing Dynamic spliit entities(REQ-4496)
                           if (counter == 1)
                               split.splitEntity1 = newEnt;
                           else if (counter == 2)
                               split.splitEntity2 = newEnt;
                           else if (counter == 3)
                               split.splitEntity3 = newEnt;
                           else if (counter == 4)
                               split.splitEntity4 = newEnt;
                           else if (counter == 5)
                               split.splitEntity5 = newEnt;
                           else if (counter == 6)
                               split.splitEntity6 = newEnt;
                           else if (counter == 7)
                               split.splitEntity7 = newEnt;
                           else if (counter == 8)
                               split.splitEntity8 = newEnt;
                           else if (counter == 9)
                               split.splitEntity9 = newEnt;
                           else if (counter == 10)
                               split.splitEntity10 = newEnt;
                           else if (counter == 11)
                               split.splitEntity11 = newEnt;
                           else if (counter == 12)
                               split.splitEntity12 = newEnt;
                           else if (counter == 13)
                               split.splitEntity13 = newEnt;
                           else if (counter == 14)
                               split.splitEntity14 = newEnt;
                           else if (counter == 15)
                               split.splitEntity15 = newEnt;
                           else if (counter == 16)
                               split.splitEntity16 = newEnt;
                           else if (counter == 17)
                               split.splitEntity17 = newEnt;
                           else if (counter == 18)
                               split.splitEntity18 = newEnt;
                           else if (counter == 19)
                               split.splitEntity19 = newEnt;
                           else if (counter == 20)
                               split.splitEntity20 = newEnt;

                           counter++;
                           */
                        }
                    }
                }
            }
        }
        public void DeleteReqItemCharges(List<long> reqItemIds, long documentCode, long P2PLineItemId, bool IsHeaderLevelCharge, int MaxPrecessionValue, int MaxPrecessionValueForTaxesAndCharges, int MaxPrecessionValueforTotal)
        {
            SqlConnection objSqlCon = null;
            SqlTransaction _sqlTrans = null;
            var sqlHelper = ContextSqlConn;
            RefCountingDataReader dr = null;
            NewP2PEntities.ItemCharge objCharge = new NewP2PEntities.ItemCharge();
            try
            {
                objSqlCon = (SqlConnection)ContextSqlConn.CreateConnection();

                string spName = string.Empty;
                spName = ReqSqlConstants.USP_P2P_DELETEREQUISITIONTEMCHARGE;
                DataTable dtReqItemId = new DataTable();
                dtReqItemId.Columns.Add("Id", typeof(long));
                if (dtReqItemId != null && reqItemIds.Any())
                {
                    foreach (long item in reqItemIds)
                    {
                        DataRow drow = dtReqItemId.NewRow();
                        drow["Id"] = item;
                        dtReqItemId.Rows.Add(drow);
                    }
                }
                objSqlCon.Open();
                _sqlTrans = objSqlCon.BeginTransaction();
                LogHelper.LogInfo(Log, "DeleteItemChargeByItemChargeId Method Started For documentCode = " + documentCode.ToString());
                using (var objSqlCommand = new SqlCommand(spName))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.Add(new SqlParameter("@tvp_P2P_Ids", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_P2P_IDS,
                        Value = dtReqItemId
                    });
                    objSqlCommand.Parameters.Add(new SqlParameter("@documentCode", documentCode));
                    objSqlCommand.Parameters.Add(new SqlParameter("@P2PLineItemId", P2PLineItemId));
                    objSqlCommand.Parameters.Add(new SqlParameter("@IsHeaderLevelCharge", IsHeaderLevelCharge));
                    objSqlCommand.Parameters.Add(new SqlParameter("@MaxPrecessionValue", MaxPrecessionValue));
                    objSqlCommand.Parameters.Add(new SqlParameter("@MaxPrecessionValueForTaxAndCharges ", MaxPrecessionValueForTaxesAndCharges));
                    objSqlCommand.Parameters.Add(new SqlParameter("@MaxPrecessionValueTotal ", MaxPrecessionValueforTotal));
                    dr = (RefCountingDataReader)ContextSqlConn.ExecuteReader(objSqlCommand);
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

                LogHelper.LogError(Log, "Error occurred while Deleting Item Charge for documentCode" + documentCode, objEx);
                throw new Exception("Error occurred while Deleting Charge for documentCode = " + documentCode);
            }
            finally
            {
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
        }
        private void FillDefaultChargeMasterDetails(DataRowCollection rows, ref NewP2PEntities.Requisition Req)
        {
            NewP2PEntities.ChargeMaster objChargeMaster = new NewP2PEntities.ChargeMaster();
            foreach (DataRow sqlDr in rows)
            {
                objChargeMaster.ChargeMasterId = ConvertToInt64(sqlDr, ReqSqlConstants.COL_CHARGEMASTERID);
                objChargeMaster.ChargeName = ConvertToString(sqlDr, ReqSqlConstants.COL_CHARGENAME);
                objChargeMaster.ChargeDescription = ConvertToString(sqlDr, ReqSqlConstants.COL_CHARGEDESCRIPTION);
                objChargeMaster.ChargeTypeCode = ConvertToInt32(sqlDr, ReqSqlConstants.COL_CHARGETYPECODE);
                objChargeMaster.IsAllowance = ConvertToBoolean(sqlDr, ReqSqlConstants.COL_ISALLOWANCE);
                objChargeMaster.CalculationBasisId = ConvertToInt16(sqlDr, ReqSqlConstants.COL_CALCULATIONBASISID);
                objChargeMaster.IsIncludeForTax = ConvertToBoolean(sqlDr, ReqSqlConstants.COL_ISINCLUDEFORTAX);
                objChargeMaster.IsEditableOnInvoice = ConvertToBoolean(sqlDr, ReqSqlConstants.COL_ISEDITABLEONINVOICE);
                objChargeMaster.IsIncludeForRetainage = ConvertToBoolean(sqlDr, ReqSqlConstants.COL_ISINCLUDEFORRETAINAGE);
                objChargeMaster.TolerancePercentage = ConvertToDecimal(sqlDr, ReqSqlConstants.COL_TOLERANCEPERCENTAGE);
                objChargeMaster.ChargeTypeName = ConvertToString(sqlDr, ReqSqlConstants.COL_CHARGETYPENAME);
            }
            Req.DefaultChargeMasterDetails = objChargeMaster;

        }

        public List<SplitAccountingFields> GetAllHeaderAccountingFieldsForRequisition(long documentCode)
        {
            List<SplitAccountingFields> lstSplitAccountingFields = new List<SplitAccountingFields>();
            RefCountingDataReader objRefCountingDataReader = null;
            try
            {
                LogHelper.LogInfo(Log, "GetAllHeaderAccountingFieldsForRequisition Method Started for id=" + documentCode);
                var sqlHelper = ContextSqlConn;
                objRefCountingDataReader = (RefCountingDataReader)sqlHelper.ExecuteReader(ReqSqlConstants.USP_P2P_REQ_GETREQUISITIONENTITYDETAILSBYID, new object[] { documentCode });
                if (objRefCountingDataReader != null)
                {
                    var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;
                    while (sqlDr.Read())
                    {
                        lstSplitAccountingFields.Add(new SplitAccountingFields
                        {
                            SplitAccountingFieldId = GetIntValue(sqlDr, ReqSqlConstants.COL_SPLIT_ACCOUNTING_FIELD_CONFIG_ID),
                            FieldName = GetStringValue(sqlDr, ReqSqlConstants.COL_FIELD_NAME),
                            FieldControls = (FieldControls)GetTinyIntValue(sqlDr, ReqSqlConstants.COL_FIELD_CONTROL_TYPE),
                            FieldOrder = GetIntValue(sqlDr, ReqSqlConstants.COL_FIELD_ORDER),
                            ParentFieldConfigId = GetIntValue(sqlDr, ReqSqlConstants.COL_PARENT_FIELD_CONFIG_ID),
                            IsMandatory = GetBoolValue(sqlDr, ReqSqlConstants.COL_ISMANDATORY),
                            EntityTypeId = GetIntValue(sqlDr, ReqSqlConstants.COL_ENTITYID),
                            Title = GetStringValue(sqlDr, ReqSqlConstants.COL_TITLE),
                            DocumentType = (DocumentType)GetIntValue(sqlDr, ReqSqlConstants.COL_DOCUMENT_TYPE),
                            CodeCombinationOrder = GetTinyIntValue(sqlDr, ReqSqlConstants.COL_CODE_COMBINATION_ORDER),
                            DisplayName = GetStringValue(sqlDr, ReqSqlConstants.COL_ENTITY_DISPLAY_NAME),
                            EntityCode = GetStringValue(sqlDr, ReqSqlConstants.COL_ENTITY_CODE),
                            EntityDetailCode = GetLongValue(sqlDr, ReqSqlConstants.COL_ENTITY_DETAIL_CODE),
                            ParentEntityDetailCode = GetLongValue(sqlDr, ReqSqlConstants.COL_PARENT_ENTITY_DETAIL_CODE),
                            ParentEntityCode = GetStringValue(sqlDr, ReqSqlConstants.COL_PARENTENTITYCODE),
                            LevelType = (LevelType)GetTinyIntValue(sqlDr, ReqSqlConstants.COL_LEVEL_TYPE),
                            ParentEntityType = (ParentEntityType)GetTinyIntValue(sqlDr, ReqSqlConstants.COL_PARENT_ENTITY_TYPE),
                            MappingType = (BusinessEntities.MappingType)GetTinyIntValue(sqlDr, ReqSqlConstants.COL_MAPPING_TYPE),
                            PopulateDefault = GetBoolValue(sqlDr, ReqSqlConstants.COL_POPULATE_DEFAULT),
                            CatalogItemControlType = (FieldControls)GetTinyIntValue(sqlDr, ReqSqlConstants.COL_CATALOG_ITEM_CONTROL_TYPE),
                            DataDisplayStyle = (DataDisplayStyle)GetTinyIntValue(sqlDr, ReqSqlConstants.COL_DATA_DISPLAY_STYLE),
                            AutoSuggestURLId = GetIntValue(sqlDr, ReqSqlConstants.COL_AUTO_SUGGEST_URL_ID),
                            StructureId = GetIntValue(sqlDr, ReqSqlConstants.COL_STRUCTURE_ID),
                            LOBEntityDetailCode = GetLongValue(sqlDr, ReqSqlConstants.COL_LOBENTITYDETAILCODE),
                            EntityDisplayName = GetStringValue(sqlDr, ReqSqlConstants.COL_ENTITY_DISPLAY_NAMES),
                            IsAccountingEntity = GetBoolValue(sqlDr, ReqSqlConstants.COL_IS_ACCOUNTING_ENTITY),
                            IsActive = GetBoolValue(sqlDr, ReqSqlConstants.COL_ISACTIVE),
                            ParentSplitAccountingFieldId = GetIntValue(sqlDr, ReqSqlConstants.COL_PARENT_FIELD_CONFIG_ID),
                            EnableShowLookup = GetBoolValue(sqlDr, ReqSqlConstants.COL_ENABLESHOWLOOKUP)
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
            }
            return lstSplitAccountingFields;
        }

        /// <summary>
        /// This method is used for generating Excel
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public NewP2PEntities.Requisition GetRequisitionLineItemsByRequisitionId(long id)
        {
            NewP2PEntities.Requisition req = new NewP2PEntities.Requisition();

            var requisitionDataSet = ContextSqlConn.ExecuteDataSet(ReqSqlConstants.USP_P2P_REQ_GETREQUISITIONLINEITEMSBYREQUISITIONID, new object[] { id });
            if (requisitionDataSet != null && requisitionDataSet.Tables.Count > 0)
            {
                if (requisitionDataSet.Tables[0].Rows != null && requisitionDataSet.Tables[0].Rows.Count > 0)
                {
                    req.items = GetRequisitionLineItemsByRequisitionDataSet(requisitionDataSet.Tables[0].Rows, (requisitionDataSet.Tables.Count > 3 && requisitionDataSet.Tables[3].Rows.Count > 0 ? requisitionDataSet.Tables[3] : null));

                    if (requisitionDataSet.Tables.Count > 1 && requisitionDataSet.Tables[1].Rows.Count > 0)
                    {
                        List<ReqAccountingSplit> splits = GetAllRequisitionSplitsFromDataSet(requisitionDataSet.Tables[1].Rows);

                        if (requisitionDataSet.Tables.Count > 2 && requisitionDataSet.Tables[2].Rows != null && requisitionDataSet.Tables[2].Rows.Count > 0)
                            FillEntitiesInRequisitionSplits(GetAllRequisitionSplitEntitiesFromDataSet(requisitionDataSet.Tables[2].Rows), req.createdBy, ref splits);

                        foreach (NewP2PEntities.RequisitionItem item in req.items)
                            item.splits = (from s in splits where s.documentItemId == item.id select s).ToList();
                    }
                }
            }
            return req;
        }
        public long SaveRequisitionUploadLog(RequisitionUploadLog reqUploadDetail)
        {
            LogHelper.LogInfo(Log, "SaveRequisitionUploadLog Method Started");
            SqlConnection objSqlCon = null;
            RefCountingDataReader objRefCountingDataReader = null;
            try
            {

                var sqlHelper = ContextSqlConn;
                objSqlCon = (SqlConnection)sqlHelper.CreateConnection();
                objSqlCon.Open();

                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_SAVEREQUISITIONUPLOADLOG))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;

                    objSqlCommand.Parameters.AddWithValue("@RequisitionUploadLogID", reqUploadDetail.RequisitionUploadLogID);
                    objSqlCommand.Parameters.AddWithValue("@RequisitionId", reqUploadDetail.RequisitionID);
                    objSqlCommand.Parameters.AddWithValue("@RequestType", reqUploadDetail.RequestType);
                    objSqlCommand.Parameters.AddWithValue("@UploadedFileID", reqUploadDetail.UploadedFileID);
                    objSqlCommand.Parameters.AddWithValue("@ProcessedFileID", reqUploadDetail.ProcessedFileID);
                    objSqlCommand.Parameters.AddWithValue("@Status", reqUploadDetail.Status);
                    objSqlCommand.Parameters.AddWithValue("@Error", reqUploadDetail.Error ?? String.Empty);
                    objSqlCommand.Parameters.AddWithValue("@ErrorTrace", reqUploadDetail.ErrorTrace ?? String.Empty);
                    objSqlCommand.Parameters.AddWithValue("@ProcessedXMLResult", reqUploadDetail.ProcessedXMLResult ?? String.Empty);
                    objSqlCommand.Parameters.AddWithValue("@UploadedBy", reqUploadDetail.UploadedBy);

                    objRefCountingDataReader = (RefCountingDataReader)sqlHelper.ExecuteReader(objSqlCommand);
                    var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;
                    if (sqlDr != null)
                    {
                        if (sqlDr.Read())
                        {
                            reqUploadDetail.RequisitionUploadLogID = GetLongValue(sqlDr, ReqSqlConstants.COL_REQ_UPLOADDETAIL_ID);
                        }
                        sqlDr.Close();
                    }
                    return reqUploadDetail.RequisitionUploadLogID;
                }
            }
            catch (Exception sqlEx)
            {
                LogHelper.LogError(Log, "Error occured in SaveRequisitionUploadLog Method for RequisitionID" + reqUploadDetail.RequisitionID, sqlEx);
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
                LogHelper.LogInfo(Log, "SaveRequisitionUploadLog Method Ended");
            }

            return 0;
        }

        public RequisitionUploadLog GetRequisitionUploadError(long requisitionid, int requestType)
        {
            RefCountingDataReader objRefCountingDataReader = null;
            RequisitionUploadLog requisitionUploadLog = null;
            try
            {
                LogHelper.LogInfo(Log, "GetRequisitionUploadError Method Started for id=" + requisitionid);
                var sqlHelper = ContextSqlConn;
                objRefCountingDataReader = (RefCountingDataReader)sqlHelper.ExecuteReader(ReqSqlConstants.USP_P2P_REQ_GETREQUISITIONUPLOADLOG, new object[] { requisitionid, requestType });
                if (objRefCountingDataReader != null)
                {
                    var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;
                    while (sqlDr.Read())
                    {
                        requisitionUploadLog = new RequisitionUploadLog();
                        requisitionUploadLog.RequisitionUploadLogID = GetLongValue(sqlDr, ReqSqlConstants.COL_REQ_UPLOADDETAIL_ID);
                        requisitionUploadLog.RequisitionID = GetLongValue(sqlDr, ReqSqlConstants.COL_REQUISITION_ID);
                        requisitionUploadLog.RequestType = GetTinyIntValue(sqlDr, ReqSqlConstants.COL_REQ_REQUESTTYPE);
                        requisitionUploadLog.UploadedFileID = GetLongValue(sqlDr, ReqSqlConstants.COL_UPLOADEDFILEID);
                        requisitionUploadLog.ProcessedFileID = GetLongValue(sqlDr, ReqSqlConstants.COL_PROCESSEDFILEID);
                        requisitionUploadLog.Status = GetStringValue(sqlDr, ReqSqlConstants.COL_STATUS);
                        requisitionUploadLog.Error = GetStringValue(sqlDr, ReqSqlConstants.COL_REQ_ERROR);
                        requisitionUploadLog.ErrorTrace = GetStringValue(sqlDr, ReqSqlConstants.COL_REQ_ERRORTRACE);
                        requisitionUploadLog.ProcessedXMLResult = GetStringValue(sqlDr, ReqSqlConstants.COL_REQ_PROCESSEDXMLRESULT);
                    }
                }
            }
            catch (Exception sqlEx)
            {
                LogHelper.LogError(Log, "Error occured in GetRequisitionUploadError Method for RequisitionID" + requisitionid, sqlEx);
                throw;
            }
            finally
            {
                if (!ReferenceEquals(objRefCountingDataReader, null) && !objRefCountingDataReader.IsClosed)
                {
                    objRefCountingDataReader.Close();
                    objRefCountingDataReader.Dispose();
                }
                LogHelper.LogInfo(Log, "GetRequisitionUploadError Method Ended");
            }
            return requisitionUploadLog;
        }
        public RequisitionResponse GetRequisitionUploadErrorResponse(long requisitionid, int requestType)
        {
            RefCountingDataReader objRefCountingDataReader = null;
            RequisitionResponse requisitionResponse = null;
            try
            {
                LogHelper.LogInfo(Log, "GetRequisitionUploadErrorResponse Method Started for id=" + requisitionid);
                var sqlHelper = ContextSqlConn;
                objRefCountingDataReader = (RefCountingDataReader)sqlHelper.ExecuteReader(ReqSqlConstants.USP_P2P_REQ_GETREQUISITIONUPLOADLOG, new object[] { requisitionid, requestType });
                if (objRefCountingDataReader != null)
                {
                    var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;
                    while (sqlDr.Read())
                    {
                        var processedXmlResult = GetStringValue(sqlDr, ReqSqlConstants.COL_REQ_PROCESSEDXMLRESULT);
                        System.Xml.Serialization.XmlSerializer xmlSerializer = new System.Xml.Serialization.XmlSerializer(typeof(RequisitionResponse));
                        requisitionResponse = (RequisitionResponse)xmlSerializer.Deserialize(new StringReader(processedXmlResult));
                    }
                }
            }
            catch (Exception sqlEx)
            {
                LogHelper.LogError(Log, "Error occured in GetRequisitionUploadErrorResponse Method for RequisitionID" + requisitionid, sqlEx);
                throw;
            }
            finally
            {
                if (!ReferenceEquals(objRefCountingDataReader, null) && !objRefCountingDataReader.IsClosed)
                {
                    objRefCountingDataReader.Close();
                    objRefCountingDataReader.Dispose();
                }
                LogHelper.LogInfo(Log, "GetRequisitionUploadErrorResponse Method Ended");
            }
            return requisitionResponse;
        }

        public void DeleteLineItems(long requisitionid, string commaSeperatedLineNos)
        {
            LogHelper.LogInfo(Log, "DeleteLineItems Method Started");
            SqlConnection objSqlCon = null;
            try
            {

                var sqlHelper = ContextSqlConn;
                objSqlCon = (SqlConnection)sqlHelper.CreateConnection();
                objSqlCon.Open();

                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_DELETELINEITEMS))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Connection = objSqlCon;
                    objSqlCommand.Parameters.AddWithValue("@RequisitionID", requisitionid);
                    objSqlCommand.Parameters.AddWithValue("@LineNos", commaSeperatedLineNos);
                    objSqlCommand.ExecuteNonQuery();

                }
            }
            catch (Exception sqlEx)
            {
                LogHelper.LogError(Log, "Error occured in DeleteLineItems Method for RequisitionID" + requisitionid, sqlEx);
                throw;
            }
            finally
            {
                if (!ReferenceEquals(objSqlCon, null) && objSqlCon.State != ConnectionState.Closed)
                {
                    objSqlCon.Close();
                    objSqlCon.Dispose();
                }
                LogHelper.LogInfo(Log, "DeleteLineItems Method Ended");
            }
        }

        /// <summary>
        /// Returns requisition items for excel download.
        /// </summary>
        /// <param name="rows">Data rows.</param>
        /// <returns>Requisition items.</returns>
        private List<NewP2PEntities.RequisitionItem> GetRequisitionLineItemsByRequisitionDataSet(DataRowCollection rows, DataTable taxRows = null)
        {
            List<NewP2PEntities.RequisitionItem> items = new List<NewP2PEntities.RequisitionItem>();
            foreach (DataRow dr in rows)
            {
                NewP2PEntities.RequisitionItem item = new NewP2PEntities.RequisitionItem();
                item.lineNumber = ConvertToInt64(dr, ReqSqlConstants.COL_LINENUMBER);
                item.buyerItemNumber = ConvertToString(dr, ReqSqlConstants.COL_BUYERITEMNUMBER);
                item.contractNumber = ConvertToString(dr, ReqSqlConstants.COL_CONTRACTNUMBER);
                item.ContractItemId = ConvertToInt64(dr, ReqSqlConstants.COL_CONTRACTITEMID);
                item.description = ConvertToString(dr, ReqSqlConstants.COL_DESCRIPTION);
                item.startDate = ConvertToNullableDateTime(dr, ReqSqlConstants.COL_START_DATE);
                item.endDate = ConvertToNullableDateTime(dr, ReqSqlConstants.COL_END_DATE);
                item.id = ConvertToInt64(dr, ReqSqlConstants.COL_ID);
                item.needByDate = ConvertToNullableDateTime(dr, ReqSqlConstants.COL_NEEDBYDATE);
                item.otherCharges = ConvertToNullableDouble(dr, ReqSqlConstants.COL_OTHERCHARGES);
                item.quantity = ConvertToDouble(dr, ReqSqlConstants.COL_QUANTITY);
                item.shippingCharges = ConvertToNullableDouble(dr, ReqSqlConstants.COL_SHIPPING_CHARGES);
                item.shippingMethod = ConvertToString(dr, ReqSqlConstants.COL_SHIPPINGMETHOD);
                item.splitType = ConvertToInt16(dr, ReqSqlConstants.COL_SPLIT_TYPE);
                item.unitPrice = ConvertToNullableDouble(dr, ReqSqlConstants.COL_UNIT_PRICE);
                item.category = new IdAndName
                {
                    id = ConvertToInt64(dr, ReqSqlConstants.COL_CATEGORY_ID),
                    name = ConvertToString(dr, ReqSqlConstants.COL_CATEGORY_NAME)
                };

                item.partner = new IdAndName
                {
                    id = ConvertToInt64(dr, ReqSqlConstants.COL_PARTNER_CODE),
                    name = ConvertToString(dr, ReqSqlConstants.COL_PARTNER_NAME)
                };
                item.isProcurable = new IdAndName
                {
                    id = ConvertToInt64(dr, ReqSqlConstants.COL_IS_PROCURABLE),
                    name = ConvertToInt16(dr, ReqSqlConstants.COL_IS_PROCURABLE) > 0 ? INVENTORY : PROCURABLE
                };

                item.currencyCode = ConvertToString(dr, ReqSqlConstants.COL_CURRENCY);
                item.clientPartnerCode = ConvertToString(dr, ReqSqlConstants.COL_CLIENT_PARTNERCODE);

                item.shipTo = new IdNameAndAddress
                {
                    id = ConvertToInt64(dr, ReqSqlConstants.COL_SHIPTOLOC_ID),
                    name = BuildLocation("", ConvertToString(dr, ReqSqlConstants.COL_SHIPTOLOC_NAME))
                };

                item.uom = new CodeAndName
                {
                    code = ConvertToString(dr, ReqSqlConstants.COL_UOM_CODE),
                    name = ConvertToString(dr, ReqSqlConstants.COL_UOM_DESCRIPTION)
                };

                item.partnerContact = new IdNameAndEmail
                {
                    name = ConvertToString(dr, ReqSqlConstants.COL_PARTNERCONTACTNAME)
                };

                item.partnerContactEmail = ConvertToString(dr, ReqSqlConstants.COL_PARTNER_CONTACT_EMAIL);

                item.orderingLocation = new IdCodeAndName
                {
                    code = ConvertToString(dr, ReqSqlConstants.COL_CLIENTLOCATIONCODE),
                    name = ConvertToString(dr, ReqSqlConstants.COL_ORDERLOCATIONNAME)
                };
                item.ShipFromLocation = new IdNameAndAddress
                {
                    name = ConvertToString(dr, ReqSqlConstants.COL_SHIPFROMLOCATIONNAME),
                    address = ConvertToString(dr, ReqSqlConstants.COL_SHIPFROMLOCATIONCODE)
                };
                item.manufacturer = ConvertToString(dr, ReqSqlConstants.COL_MANUFACTURER);
                item.ManufacturerModel = ConvertToString(dr, ReqSqlConstants.COL_MANUFACTURER_MODEL);
                item.manufacturerPartNumber = ConvertToString(dr, ReqSqlConstants.COL_MANUFACTURER_PART_NUMBER);
                item.type = new IdAndName
                {
                    id = ConvertToInt64(dr, ReqSqlConstants.COL_ITEM_TYPE_ID),
                    name = ConvertToString(dr, ReqSqlConstants.COL_ITEMTYPE)
                };
                item.matching = new IdAndName()
                {
                    id = ConvertToInt16(dr, ReqSqlConstants.COL_MATCHTYPE),
                    name = ((NewPlatformEntities.MatchType)ConvertToInt16(dr, ReqSqlConstants.COL_MATCHTYPE)).ToString()
                };
                item.matching.name = item.matching.id == 0 ? "" : item.matching.name;
                item.partnerItemNumber = ConvertToString(dr, ReqSqlConstants.COL_SUPPLIERPARTID);
                item.inventoryType = ConvertToBoolean(dr, ReqSqlConstants.COL_INVENTORYTYPE);
                item.isTaxExempt = ConvertToBoolean(dr, ReqSqlConstants.COL_ISTAXEXEMPT);
                item.TrasmissionMode = ConvertToInt16(dr, ReqSqlConstants.COL_TRASMISSIONMODE);
                item.TransmissionValue = ConvertToString(dr, ReqSqlConstants.COL_TRANSMISSIONVALUE);
                item.deliverToStr = ConvertToString(dr, ReqSqlConstants.COL_DELIVERTO);
                item.taxItems = new List<Tax>();
                if (taxRows != null)
                {
                    var lstrows = taxRows.AsEnumerable().Where(row => row.Field<long>("RequisitionItemId") == item.id);

                    if (lstrows.Any())
                    {
                        var tblFiltered = lstrows.CopyToDataTable<DataRow>();

                        foreach (var taxes in lstrows)
                        {
                            Tax tax = new Tax();
                            tax.code = taxes["TaxCode"].ToString();
                            tax.description = taxes["TaxDescription"].ToString();
                            tax.value = Convert.ToDecimal(taxes["TaxValue"]);
                            tax.type = new IdAndName { id = Convert.ToInt16(taxes["TaxTypeId"]) };
                            item.taxItems.Add(tax);
                        }
                    }
                }
                items.Add(item);

            }
            return items;
        }

        /// <summary>
        /// Gets all splits in requisition for excel download.
        /// </summary>
        /// <param name="rows">Data rows.</param>
        /// <returns>All splits.</returns>
        private List<ReqAccountingSplit> GetAllRequisitionSplitsFromDataSet(DataRowCollection rows)
        {
            List<ReqAccountingSplit> splits = new List<ReqAccountingSplit>();
            foreach (DataRow dr in rows)
            {
                ReqAccountingSplit split = new ReqAccountingSplit();
                split.id = ConvertToInt64(dr, ReqSqlConstants.COL_REQUISITION_SPLIT_ITEM_ID);
                split.quantity = ConvertToDouble(dr, ReqSqlConstants.COL_QUANTITY);
                split.percentage = ConvertToDecimal(dr, ReqSqlConstants.COL_PERCENTAGE);
                split.documentItemId = ConvertToInt64(dr, ReqSqlConstants.COL_REQUISITION_ITEM_ID);
                split.SplitType = ConvertToInt32(dr, ReqSqlConstants.COL_SPLIT_TYPE);
                splits.Add(split);
            }

            return splits;
        }

        /// <summary>
        /// Gets all DB split entities.
        /// </summary>
        /// <param name="rows">Data rows.</param>
        /// <returns>DB split entities.</returns>
        private List<DBSplitEntity> GetAllRequisitionSplitEntitiesFromDataSet(DataRowCollection rows)
        {
            List<DBSplitEntity> splitEntities = new List<DBSplitEntity>();
            foreach (DataRow dr in rows)
            {
                DBSplitEntity splitEntity = new DBSplitEntity();
                splitEntity.entityType = ConvertToInt32(dr, ReqSqlConstants.COL_ENTITY_TYPE_ID);
                splitEntity.splitItemId = ConvertToInt64(dr, ReqSqlConstants.COL_REQUISITION_SPLIT_ITEM_ID);
                splitEntity.title = ConvertToString(dr, ReqSqlConstants.COL_TITLE);
                splitEntity.entityCode = ConvertToString(dr, ReqSqlConstants.COL_ENTITY_CODE);
                splitEntity.code = ConvertToString(dr, ReqSqlConstants.COL_SPLIT_ACCOUNTING_FIELD_VALUE);
                // splitEntity.glName = ConvertToString(dr, ReqSqlConstants.COL_GLNAME);
                if (splitEntity.title.ToUpper() == REQUESTER)
                    splitEntity.name = ConvertToString(dr, ReqSqlConstants.COL_REQUESTER_NAME);
                else if (splitEntity.title.ToUpper() == GL_CODE)
                    splitEntity.name = ConvertToString(dr, ReqSqlConstants.COL_GLNAME);
                else
                    splitEntity.name = ConvertToString(dr, ReqSqlConstants.COL_ENTITY_DISPLAY_NAME);

                splitEntities.Add(splitEntity);
            }

            return splitEntities;
        }


        public void saveBudgetoryStatus(DataTable validationResult, long documentCode)
        {
            var sqlHelper = ContextSqlConn;
            InitializeConnection(sqlHelper.ConnectionString);
            InitializeDBReq(false, documentCode);
            int budgetoryStatus = 1;
            List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
            Dictionary<string, object> row;
            foreach (DataRow dr in validationResult.Rows)
            {
                row = new Dictionary<string, object>();
                foreach (DataColumn col in validationResult.Columns)
                {
                    row.Add(col.ColumnName, dr[col]);
                }
                rows.Add(row);
            }
            if (rows.Count > 0)
            {
                foreach (Dictionary<string, object> r in rows)
                {
                    if (r.Where(x => x.Key == "Status").Select(c => c.Value).ToList().FirstOrDefault().Equals("2")
                        || r.Where(x => x.Key == "Status").Select(c => c.Value).ToList().FirstOrDefault().Equals("4"))
                        budgetoryStatus = 4;
                }
            }
            dbReq.BudgetoryStatus = budgetoryStatus;
            _Context.SaveChanges();
            DisposeConnection();
        }

        public List<SplitAccountingFields> GetValidSplitAccountingCodes(
                                                LevelType levelType,
                                                long LobEntityDetailCode,
                                                int structureId,
                                                List<KeyValuePair<int, string>> lstAccountingDataFromUpload,
                                                long sourceSystemEntityId,
                                                long sourceSystemEntityDetailCode)
        {
            //Murthy
            LogHelper.LogInfo(Log, "Requisition GetValidSplitAccountingCodes Method Started");
            SqlConnection objSqlCon = null;
            RefCountingDataReader objRefCountingDataReader = null;
            List<SplitAccountingFields> lstSplitAccountingFields = null;
            try
            {
                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("In GetValidSplitAccountingCodes Method.",
                                             "SP: usp_P2P_REQ_GetValidSplitAccountingCodes"));

                List<DocumentSplitItemEntity> dsEntities = new List<DocumentSplitItemEntity>();
                foreach (var entity in lstAccountingDataFromUpload)
                {
                    DocumentSplitItemEntity dsEntity = new DocumentSplitItemEntity();
                    dsEntity.SplitAccountingFieldId = entity.Key;
                    dsEntity.EntityCode = entity.Value;

                    dsEntities.Add(dsEntity);
                }

                DataTable dtReqItemEntities = GEP.Cumulus.P2P.DataAccessObjects.DAOHelper.ConvertToDataTable(dsEntities,
                                                       GetRequisitionSplitItemEntitiesTable);

                var sqlHelper = ContextSqlConn;
                objSqlCon = (SqlConnection)sqlHelper.CreateConnection();
                objSqlCon.Open();
                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_GETVALIDSPLITACCOUNTINGCODES))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.AddWithValue("@LevelType", (int)levelType);
                    objSqlCommand.Parameters.AddWithValue("@LOBEntityDetailCode", LobEntityDetailCode);
                    objSqlCommand.Parameters.AddWithValue("@structureID", structureId);
                    objSqlCommand.Parameters.Add(new SqlParameter("@tvp_P2P_SplitItemsEntities", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_P2P_SPLITITEMSENTITIES,
                        Value = dtReqItemEntities
                    });
                    objSqlCommand.Parameters.AddWithValue("@SourceSystemEntityId", sourceSystemEntityId);
                    objSqlCommand.Parameters.AddWithValue("@SourceSystemEntityDetailCode", sourceSystemEntityDetailCode);
                    objRefCountingDataReader = (RefCountingDataReader)sqlHelper.ExecuteReader(objSqlCommand);
                }

                if (objRefCountingDataReader != null)
                {
                    lstSplitAccountingFields = new List<SplitAccountingFields>();
                    SplitAccountingFields splitAccountingField = new SplitAccountingFields();
                    var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;
                    while (sqlDr.Read())
                    {
                        splitAccountingField = new SplitAccountingFields();
                        splitAccountingField.SplitAccountingFieldId = GetIntValue(sqlDr, ReqSqlConstants.COL_SPLIT_ACCOUNTING_FIELD_CONFIG_ID);
                        splitAccountingField.Title = GetStringValue(sqlDr, ReqSqlConstants.COL_TITLE);
                        splitAccountingField.EntityDetailCode = GetLongValue(sqlDr, ReqSqlConstants.COL_SPLIT_ITEM_ENTITYDETAILCODE);
                        splitAccountingField.EntityCode = GetStringValue(sqlDr, ReqSqlConstants.COL_ENTITY_CODE);
                        splitAccountingField.EntityDisplayName = GetStringValue(sqlDr, ReqSqlConstants.COL_LEGAL_ENTITY_DISPLAYNAME);
                        splitAccountingField.EntityTypeId = GetIntValue(sqlDr, ReqSqlConstants.COL_ENTITY_TYPE_ID);
                        // TODO : Fill all  the data
                        lstSplitAccountingFields.Add(splitAccountingField);
                    }
                }
                return lstSplitAccountingFields;
            }
            catch (Exception ex)
            {
                Log.Debug("Exception occured in GetValidSplitAccountingCodes method of Requisition" + ex.Message);
                return null;
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
                LogHelper.LogInfo(Log, "Requisition GetValidSplitAccountingCodes Method Ended");
            }
        }

        public List<SplitAccountingFields> GetAllSplitAccountingCodesByDocumentType(LevelType levelType, long LobEntityDetailCode, int structureId)
        {
            LogHelper.LogInfo(Log, "Requisition GetAllSplitAccountingCodesByDocumentType Method Started");
            SqlConnection objSqlCon = null;
            RefCountingDataReader objRefCountingDataReader = null;
            List<SplitAccountingFields> lstSplitAccountingFields = null;
            try
            {
                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("In GetAllSplitAccountingCodesByDocumentType Method.",
                                             "SP: USP_P2P_REQ_GETALLSPLITACCOUNTINGCODESBYDOCUMENTTYPE"));

                var sqlHelper = ContextSqlConn;
                objSqlCon = (SqlConnection)sqlHelper.CreateConnection();
                objSqlCon.Open();
                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_GETALLSPLITACCOUNTINGCODESBYDOCUMENTTYPE))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.AddWithValue("@LevelType", (int)levelType);
                    objSqlCommand.Parameters.AddWithValue("@LOBEntityDetailCode", LobEntityDetailCode);
                    objSqlCommand.Parameters.AddWithValue("@structureID", structureId);
                    objRefCountingDataReader = (RefCountingDataReader)sqlHelper.ExecuteReader(objSqlCommand);
                }

                if (objRefCountingDataReader != null)
                {
                    lstSplitAccountingFields = new List<SplitAccountingFields>();
                    var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;
                    while (sqlDr.Read())
                    {
                        SplitAccountingFields splitAccountingField = new SplitAccountingFields();
                        splitAccountingField.SplitAccountingFieldId = GetIntValue(sqlDr, ReqSqlConstants.COL_SPLIT_ACCOUNTING_FIELD_CONFIG_ID);
                        splitAccountingField.Title = GetStringValue(sqlDr, ReqSqlConstants.COL_TITLE);
                        splitAccountingField.EntityDetailCode = GetLongValue(sqlDr, ReqSqlConstants.COL_SPLIT_ITEM_ENTITYDETAILCODE);
                        splitAccountingField.EntityCode = GetStringValue(sqlDr, ReqSqlConstants.COL_ENTITY_CODE);
                        splitAccountingField.EntityDisplayName = GetStringValue(sqlDr, ReqSqlConstants.COL_LEGAL_ENTITY_DISPLAYNAME);
                        splitAccountingField.EntityTypeId = GetIntValue(sqlDr, ReqSqlConstants.COL_ENTITY_TYPE_ID);
                        // TODO : Fill all  the data
                        lstSplitAccountingFields.Add(splitAccountingField);
                    }
                    //Get GLCode
                    if (sqlDr.NextResult())
                    {
                        while (sqlDr.Read())
                        {
                            SplitAccountingFields splitAccountingField = new SplitAccountingFields();
                            splitAccountingField.SplitAccountingFieldId = GetIntValue(sqlDr, ReqSqlConstants.COL_SPLIT_ACCOUNTING_FIELD_CONFIG_ID);
                            splitAccountingField.Title = GetStringValue(sqlDr, ReqSqlConstants.COL_TITLE);
                            splitAccountingField.EntityDetailCode = GetLongValue(sqlDr, ReqSqlConstants.COL_SPLIT_ITEM_ENTITYDETAILCODE);
                            splitAccountingField.EntityCode = GetStringValue(sqlDr, ReqSqlConstants.COL_ENTITY_CODE);
                            splitAccountingField.EntityDisplayName = GetStringValue(sqlDr, ReqSqlConstants.COL_LEGAL_ENTITY_DISPLAYNAME);

                            lstSplitAccountingFields.Add(splitAccountingField);
                        }
                    }
                }
                return lstSplitAccountingFields;
            }
            catch (Exception ex)
            {
                Log.Debug("Exception occured in GetAllSplitAccountingCodesByDocumentType method of Requisition" + ex.Message);
                return null;
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
                LogHelper.LogInfo(Log, "Requisition GetAllSplitAccountingCodesByDocumentType Method Ended");
            }
        }

        #region Bulk Upload Req Excel Lines
        public bool SaveBulkRequisitionItems(long requisitionId, List<GEP.Cumulus.P2P.BusinessEntities.RequisitionItem> lstReqItems, int maxPrecessionValue, int maxPrecessionValueForTotal, int maxPrecessionValueForTaxAndCharges, bool isCallFromWeb = false, int purchaseType = 0)
        {
            LogHelper.LogInfo(Log, "SaveBulkRequisitionItems Method Started");
            bool result = true;
            SqlConnection objSqlCon = null;
            try
            {
                var sqlHelper = ContextSqlConn;
                objSqlCon = (SqlConnection)sqlHelper.CreateConnection();
                objSqlCon.Open();

                //Convert List to DataTable
                DataTable dtReqItems = new DataTable();
                DataTable dtReqSplitItems = new DataTable();
                DataTable dtReqSplitItemEntities = new DataTable();
                DataTable dtReqTaxes = new DataTable();
                CreateSQLBulkCopyDataTable(requisitionId, lstReqItems, dtReqItems, dtReqSplitItems, dtReqSplitItemEntities, dtReqTaxes, isCallFromWeb, purchaseType);

                //Clean the Data from Staging Tables
                DeleteRequisitionUploadStagingData(requisitionId);

                using (SqlBulkCopy bcp = new SqlBulkCopy(objSqlCon))
                {
                    bcp.BulkCopyTimeout = 0;
                    bcp.BatchSize = 10000;

                    if (dtReqItems.Rows.Count > 0)
                    {
                        bcp.DestinationTableName = "P2P_RequisitionItems_Staging";

                        foreach (DataColumn col in dtReqItems.Columns)
                            bcp.ColumnMappings.Add(col.ColumnName, col.ColumnName);

                        bcp.WriteToServer(dtReqItems);
                    }

                    if (dtReqSplitItems.Rows.Count > 0)
                    {
                        bcp.ColumnMappings.Clear();
                        bcp.DestinationTableName = "P2P_RequisitionSplitItems_Staging";

                        foreach (DataColumn col in dtReqSplitItems.Columns)
                            bcp.ColumnMappings.Add(col.ColumnName, col.ColumnName);

                        bcp.WriteToServer(dtReqSplitItems);
                    }

                    if (dtReqSplitItemEntities.Rows.Count > 0)
                    {
                        bcp.ColumnMappings.Clear();
                        bcp.DestinationTableName = "P2P_RequisitionSplitItemEntities_Staging";

                        foreach (DataColumn col in dtReqSplitItemEntities.Columns)
                            bcp.ColumnMappings.Add(col.ColumnName, col.ColumnName);

                        bcp.WriteToServer(dtReqSplitItemEntities);
                    }

                    if (dtReqTaxes.Rows.Count > 0)
                    {
                        bcp.ColumnMappings.Clear();
                        bcp.DestinationTableName = "P2P_RequisitionTaxes_Staging";

                        foreach (DataColumn col in dtReqTaxes.Columns)
                            bcp.ColumnMappings.Add(col.ColumnName, col.ColumnName);

                        bcp.WriteToServer(dtReqTaxes);
                    }
                    //Call the Stored Procedure to Move Data from Staging Tables
                    result = BulkCopyRequisitionLineItems(requisitionId, this.UserContext.ContactCode, maxPrecessionValue, maxPrecessionValueForTotal, maxPrecessionValueForTaxAndCharges, isCallFromWeb);
                }
            }
            catch (Exception sqlEx)
            {
                LogHelper.LogError(Log, "Error occured in SaveBulkRequisitionItems Method ", sqlEx);
                throw;
            }
            finally
            {
                if (!ReferenceEquals(objSqlCon, null) && objSqlCon.State != ConnectionState.Closed)
                {
                    objSqlCon.Close();
                    objSqlCon.Dispose();
                }
                LogHelper.LogInfo(Log, "SaveBulkRequisitionItems Method Ended ");
            }

            return result;
        }
        private void CreateSQLBulkCopyDataTable(long RequisitionId, List<GEP.Cumulus.P2P.BusinessEntities.RequisitionItem> lstReqItems, DataTable dtReqItems, DataTable dtReqSplitItems, DataTable dtReqSplitItemEntities, DataTable dtReqTaxes, bool isCallFromWeb = false, int purchaseType = 0)
        {
            //Create Req Item DataTable
            dtReqItems.Columns.Add("RequisitionID", typeof(System.Int64));
            dtReqItems.Columns.Add("RequisitionItemID", typeof(System.Int64));
            dtReqItems.Columns.Add("LineNumber", typeof(System.Int64));
            dtReqItems.Columns.Add("ShortName", typeof(System.String));
            dtReqItems.Columns.Add("Description", typeof(System.String));
            dtReqItems.Columns.Add("UnitPrice", typeof(System.Decimal));
            dtReqItems.Columns.Add("Quantity", typeof(System.Decimal));
            dtReqItems.Columns.Add("UOM", typeof(System.String));
            dtReqItems.Columns.Add("DateRequested", typeof(System.DateTime));
            dtReqItems.Columns.Add("DateNeeded", typeof(System.DateTime));
            dtReqItems.Columns.Add("PartnerCode", typeof(System.Decimal));
            dtReqItems.Columns.Add("ManufacturerName", typeof(System.String));
            dtReqItems.Columns.Add("ManufacturerPartNumber", typeof(System.String));
            dtReqItems.Columns.Add("CategoryID", typeof(System.Int64));
            dtReqItems.Columns.Add("ItemTypeID", typeof(System.Int32));//could create problem
            dtReqItems.Columns.Add("CurrencyCode", typeof(System.String));
            dtReqItems.Columns.Add("StartDate", typeof(System.DateTime));
            dtReqItems.Columns.Add("EndDate", typeof(System.DateTime));
            dtReqItems.Columns.Add("AdditionalCharges", typeof(System.Decimal));
            dtReqItems.Columns.Add("ShippingCharges", typeof(System.Decimal));
            dtReqItems.Columns.Add("Tax", typeof(System.Decimal));
            dtReqItems.Columns.Add("SplitType", typeof(System.Int32));//could create problem
            dtReqItems.Columns.Add("CatalogItemID", typeof(System.Int64));
            dtReqItems.Columns.Add("SupplierPartId", typeof(System.String));
            dtReqItems.Columns.Add("SupplierPartAuxiliaryId", typeof(System.String));
            dtReqItems.Columns.Add("IsTaxExempt", typeof(System.Int32));
            dtReqItems.Columns.Add("ItemExtendedType", typeof(System.Int32));//could create problem
            dtReqItems.Columns.Add("ItemNumber", typeof(System.String));
            dtReqItems.Columns.Add("ExtContractRef", typeof(System.String));
            dtReqItems.Columns.Add("ContractItemId", typeof(System.Int64));
            dtReqItems.Columns.Add("OrderLocationId", typeof(System.Int64));
            dtReqItems.Columns.Add("ShipFromLocationId", typeof(System.Int64));
            dtReqItems.Columns.Add("PartnerContactId", typeof(System.Int64));
            dtReqItems.Columns.Add("ManufacturerModel", typeof(System.String));
            dtReqItems.Columns.Add("ShipToLocationId", typeof(System.Int32));
            dtReqItems.Columns.Add("SourceType", typeof(System.Int32));
            dtReqItems.Columns.Add("ShippingMethod", typeof(System.String));
            dtReqItems.Columns.Add("IsDeleted", typeof(System.Boolean));
            dtReqItems.Columns.Add("OverallItemLimit", typeof(System.Decimal));
            dtReqItems.Columns.Add("TrasmissionMode", typeof(System.Int32));
            dtReqItems.Columns.Add("TransmissionValue", typeof(System.String));

            dtReqItems.Columns.Add("ProcurementStatus", typeof(System.Int32));
            dtReqItems.Columns.Add("IsProcurable", typeof(System.Int32));
            dtReqItems.Columns.Add("DeliverTo", typeof(System.String));
            dtReqItems.Columns.Add("DelivertoLocationID", typeof(System.Int32));
            dtReqItems.Columns.Add("SupplierUOMCode", typeof(System.String));
            dtReqItems.Columns.Add("ContractLineReference", typeof(System.String));

            //---added by venkatesh for match type
            dtReqItems.Columns.Add("MatchType", typeof(System.Int16));
            dtReqItems.Columns.Add("InventoryType", typeof(System.Boolean));
            //-----adding venkatesh for anadarko stories 
            dtReqItems.Columns.Add("PriceTypeId", typeof(System.Int64));
            dtReqItems.Columns.Add("JobTitleId", typeof(System.Int64));
            dtReqItems.Columns.Add("ContingentWorkerId", typeof(System.Int64));
            dtReqItems.Columns.Add("Margin", typeof(System.Decimal));
            dtReqItems.Columns.Add("BaseRate", typeof(System.Decimal));
            dtReqItems.Columns.Add("ReportingManagerId", typeof(System.Int64));
            dtReqItems.Columns.Add("SmartFormId", typeof(System.Int64));
            dtReqItems.Columns.Add(ReqSqlConstants.COL_SPENDCONTROLDOCUMENTCODE, typeof(System.Int64));
            dtReqItems.Columns.Add(ReqSqlConstants.COL_SPENDCONTROLDOCUMENTITEMID, typeof(System.Int64));
            dtReqItems.Columns.Add(ReqSqlConstants.COL_SPENDCONTROLDOCUMENTNUMBER, typeof(System.String));
            dtReqItems.Columns.Add(ReqSqlConstants.COL_PAYMENTTERMID, typeof(System.Int32));
            //REQ-5515 adding columns for incoterms
            dtReqItems.Columns.Add(ReqSqlConstants.COL_INCOTERMID, typeof(System.Int32));
            dtReqItems.Columns.Add(ReqSqlConstants.COL_INCOTERMCODE, typeof(System.String));
            dtReqItems.Columns.Add(ReqSqlConstants.COL_INCOTERMLOCATION, typeof(System.String));
            dtReqItems.Columns.Add(ReqSqlConstants.COL_CONVERSIONFACTOR, typeof(decimal));
            dtReqItems.Columns.Add(ReqSqlConstants.COL_TAXJURISDICTION, typeof(System.String));
            dtReqItems.Columns.Add(ReqSqlConstants.COL_ITEMSPECIFICATION, typeof(System.String));
            dtReqItems.Columns.Add(ReqSqlConstants.COL_INTERNALPLANTMEMO, typeof(System.String));
            dtReqItems.Columns.Add(ReqSqlConstants.COL_ALLOWADVANCES, typeof(System.Boolean));
            dtReqItems.Columns.Add(ReqSqlConstants.COL_ADVANCEPERCENTAGE, typeof(System.Decimal));
            dtReqItems.Columns.Add(ReqSqlConstants.COL_ADVANCEAMOUNT, typeof(System.Decimal));
            dtReqItems.Columns.Add(ReqSqlConstants.COL_ADVANCERELEASEDATE, typeof(System.DateTime));
            dtReqItems.Columns.Add(ReqSqlConstants.COL_ITEMMASTERID, typeof(System.Int64));
            dtReqItems.Columns.Add(ReqSqlConstants.COL_ISPREFERREDSUPPLIER, typeof(System.Boolean));
            dtReqItems.Columns.Add(ReqSqlConstants.COL_ALLOWFLEXIBLEPRICE, typeof(System.Boolean));
            //Create RequisitionSplitItems Data Table
            dtReqSplitItems.Columns.Add("RequisitionSplitItemId", typeof(System.Int64));//Check the column name in staging table
            dtReqSplitItems.Columns.Add("RequisitionID", typeof(System.Int64));//Check the column name in staging table
            dtReqSplitItems.Columns.Add("LineNumber", typeof(System.Int32));
            dtReqSplitItems.Columns.Add("SplitNumber", typeof(System.Int32));
            dtReqSplitItems.Columns.Add("RequisitionItemId", typeof(System.Int64));
            dtReqSplitItems.Columns.Add("SplitType", typeof(System.Int32));//could create problem
            dtReqSplitItems.Columns.Add("Percentage", typeof(System.Decimal));
            dtReqSplitItems.Columns.Add("Quantity", typeof(System.Decimal));
            dtReqSplitItems.Columns.Add("Tax", typeof(System.Decimal));
            dtReqSplitItems.Columns.Add("ShippingCharges", typeof(System.Decimal));
            dtReqSplitItems.Columns.Add("AdditionalCharges", typeof(System.Decimal));
            dtReqSplitItems.Columns.Add("SplitItemTotal", typeof(System.Decimal));
            dtReqSplitItems.Columns.Add("IsDeleted", typeof(System.Boolean));
            dtReqSplitItems.Columns.Add("OverallLimitSplitItem", typeof(System.Decimal));

            //Create RequisitionSplitItemEntities Data Table
            dtReqSplitItemEntities.Columns.Add("RequisitionSplitItemId", typeof(System.Int64));
            dtReqSplitItemEntities.Columns.Add("RequisitionID", typeof(System.Int64));//Check the column name in staging table
            dtReqSplitItemEntities.Columns.Add("LineNumber", typeof(System.Int64));
            dtReqSplitItemEntities.Columns.Add("SplitNumber", typeof(System.Int32));
            dtReqSplitItemEntities.Columns.Add("SplitAccountingFieldConfigId", typeof(System.Int32));
            dtReqSplitItemEntities.Columns.Add("SplitAccountingFieldValue", typeof(System.String));
            dtReqSplitItemEntities.Columns.Add("EntityCode", typeof(System.String));

            dtReqTaxes.Columns.Add("RequisitionID", typeof(System.Int64));
            dtReqTaxes.Columns.Add("RequisitionItemId", typeof(System.Int64));
            dtReqTaxes.Columns.Add("LineNumber", typeof(System.Int64));
            dtReqTaxes.Columns.Add("TaxId", typeof(System.Int32));
            dtReqTaxes.Columns.Add("TaxDescription", typeof(System.String));
            dtReqTaxes.Columns.Add("TaxType", typeof(System.Byte));
            dtReqTaxes.Columns.Add("TaxMode", typeof(System.Byte));
            dtReqTaxes.Columns.Add("TaxValue", typeof(System.Decimal));
            dtReqTaxes.Columns.Add("IsDeleted", typeof(System.Boolean));
            dtReqTaxes.Columns.Add("IsManual", typeof(System.Boolean));
            dtReqTaxes.Columns.Add("RequisitionTaxId", typeof(System.Int64));
            int itemCount = 5000;
            foreach (GEP.Cumulus.P2P.BusinessEntities.RequisitionItem objItem in lstReqItems)
            {
                if (isCallFromWeb && objItem.DocumentItemId == 0)
                {
                    itemCount = itemCount + 1;
                    objItem.ItemLineNumber = itemCount;
                }
                DataRow drItem = dtReqItems.NewRow();
                drItem["RequisitionID"] = RequisitionId;
                drItem["RequisitionItemID"] = objItem.DocumentItemId;
                drItem["LineNumber"] = objItem.ItemLineNumber;
                drItem["ShortName"] = objItem.ShortName;
                drItem["Description"] = objItem.Description;
                drItem["UnitPrice"] = objItem.UnitPrice;
                drItem["Quantity"] = objItem.Quantity;
                drItem["UOM"] = objItem.UOM;
                drItem["DateRequested"] = objItem.DateRequested ?? DateTime.Now;
                if (objItem.DateNeeded != null)
                {
                    DateTime dt = (DateTime)objItem.DateNeeded;
                    drItem["DateNeeded"] = dt.Date.Add(new TimeSpan(12, 00, 0));
                }
                drItem["PartnerCode"] = objItem.PartnerCode;
                drItem["ManufacturerName"] = objItem.ManufacturerName;
                drItem["ManufacturerPartNumber"] = objItem.ManufacturerPartNumber;
                drItem["CategoryID"] = objItem.CategoryId;
                drItem["ItemTypeID"] = objItem.ItemType == ItemType.Material ? ItemType.Material : ItemType.Service;
                drItem["CurrencyCode"] = objItem.Currency;
                if (objItem.StartDate != null)
                {
                    DateTime startDateDT = (DateTime)objItem.StartDate;
                    drItem["StartDate"] = startDateDT.Date.Add(new TimeSpan(12, 00, 0));
                }
                if (objItem.EndDate != null)
                {
                    DateTime endDateDT = (DateTime)objItem.EndDate;
                    drItem["EndDate"] = endDateDT.Date.Add(new TimeSpan(12, 00, 0));
                }
                drItem["AdditionalCharges"] = objItem.AdditionalCharges;
                drItem["ShippingCharges"] = objItem.ShippingCharges;
                drItem["Tax"] = objItem.Tax;
                drItem["SplitType"] = objItem.SplitType;
                drItem["CatalogItemID"] = objItem.CatalogItemId;
                drItem["SupplierPartId"] = objItem.SupplierPartId == null ? string.Empty : objItem.SupplierPartId;
                drItem["SupplierPartAuxiliaryId"] = objItem.SupplierPartAuxiliaryId == null ? string.Empty : objItem.SupplierPartAuxiliaryId;
                drItem["IsTaxExempt"] = objItem.IsTaxExempt;
                drItem["ItemExtendedType"] = objItem.ItemExtendedType;
                drItem["ItemNumber"] = objItem.ItemNumber == null ? string.Empty : objItem.ItemNumber;
                drItem["ExtContractRef"] = !string.IsNullOrEmpty(objItem.ContractNo) ? objItem.ContractNo : (objItem.ExtContractRef == null ? string.Empty : objItem.ExtContractRef);
                drItem["ContractItemId"] = objItem.ContractItemId;
                drItem["ContractLineReference"] = objItem.ContractLineRef;
                drItem["OrderLocationId"] = objItem.OrderLocationId;
                drItem["ShipFromLocationId"] = objItem.ShipFromLocationId;
                drItem["PartnerContactId"] = objItem.PartnerContactId;
                drItem["ManufacturerModel"] = objItem.ManufacturerModel;
                drItem["ShipToLocationId"] = objItem.ShipToLocationId;
                drItem["SourceType"] = objItem.SourceType;
                drItem["ShippingMethod"] = objItem.ShippingMethod;
                drItem["IsDeleted"] = objItem.IsDeleted;
                drItem["TrasmissionMode"] = objItem.TrasmissionMode;
                drItem["TransmissionValue"] = objItem.TransmissionValue;
                drItem["ProcurementStatus"] = objItem.ProcurementStatus;
                drItem["IsProcurable"] = objItem.IsProcurable;
                drItem["DeliverTo"] = objItem.DelivertoStr;
                drItem["DelivertoLocationID"] = objItem.DelivertoLocationID;
                drItem["SupplierUOMCode"] = objItem.SupplierUOMCode == null ? string.Empty : objItem.SupplierUOMCode;
                //---added by venkatesh for match type
                drItem["MatchType"] = objItem.MatchType;
                drItem["InventoryType"] = objItem.InventoryType.HasValue ? objItem.InventoryType.Value : false;
                //----added by venkatesh for anadarko
                drItem["PriceTypeId"] = objItem.PriceTypeId != 0 ? objItem.PriceTypeId : 0;
                drItem["JobTitleId"] = objItem.JobTitleId != 0 ? objItem.JobTitleId : 0;
                drItem["ContingentWorkerId"] = objItem.ContingentWorkerId != 0 ? objItem.ContingentWorkerId : 0;
                drItem["Margin"] = objItem.Margin != 0 ? objItem.Margin : 0;
                drItem["BaseRate"] = objItem.BaseRate != 0 ? objItem.BaseRate : 0;
                drItem["ReportingManagerId"] = objItem.ReportingManagerId != 0 ? objItem.ReportingManagerId : 0;
                drItem["SmartFormId"] = objItem.SmartFormId != 0 ? objItem.SmartFormId : 0;
                drItem[ReqSqlConstants.COL_SPENDCONTROLDOCUMENTCODE] = objItem.SpendControlDocumentCode;
                drItem[ReqSqlConstants.COL_SPENDCONTROLDOCUMENTITEMID] = objItem.SpendControlDocumentItemId;
                drItem[ReqSqlConstants.COL_SPENDCONTROLDOCUMENTNUMBER] = objItem.SpendControlDocumentNumber;
                drItem[ReqSqlConstants.COL_PAYMENTTERMID] = objItem.PaymentTermId;
                drItem[ReqSqlConstants.COL_INCOTERMID] = objItem.IncoTermId;
                drItem[ReqSqlConstants.COL_INCOTERMCODE] = objItem.IncoTermCode;
                drItem[ReqSqlConstants.COL_INCOTERMLOCATION] = objItem.IncoTermLocation;
                drItem[ReqSqlConstants.COL_CONVERSIONFACTOR] = objItem.ConversionFactor;
                drItem[ReqSqlConstants.COL_TAXJURISDICTION] = objItem.TaxJurisdiction;
                drItem[ReqSqlConstants.COL_ITEMSPECIFICATION] = objItem.Itemspecification;
                drItem[ReqSqlConstants.COL_INTERNALPLANTMEMO] = objItem.InternalPlantMemo;
                // The below item type will be fixed item type, the logic will be changed later based on configuration settings for calculating overall item limit
                if (objItem.OverallItemLimit == 0 && objItem.ItemType == ItemType.Service && purchaseType == 2)
                {
                    var unitP = objItem.UnitPrice.HasValue ? objItem.UnitPrice.Value : 0;
                    var shippingC = objItem.ShippingCharges.HasValue ? objItem.ShippingCharges.Value : 0;
                    var additionalC = objItem.AdditionalCharges.HasValue ? objItem.AdditionalCharges.Value : 0;
                    var taxPrice = objItem.Tax.HasValue ? objItem.Tax.Value : 0;

                    objItem.OverallItemLimit = Convert.ToDecimal((unitP * objItem.Quantity) + shippingC + additionalC + taxPrice);
                }

                drItem["OverallItemLimit"] = objItem.OverallItemLimit;

                drItem[ReqSqlConstants.COL_ALLOWADVANCES] = objItem.AllowAdvances;
                drItem[ReqSqlConstants.COL_ADVANCEPERCENTAGE] = objItem.AdvancePercentage == null ? 0 : objItem.AdvancePercentage;
                drItem[ReqSqlConstants.COL_ADVANCEAMOUNT] = objItem.AdvanceAmount == null ? 0 : objItem.AdvanceAmount;
                if (objItem.AdvanceReleaseDate != null)
                {
                    drItem[ReqSqlConstants.COL_ADVANCERELEASEDATE] = objItem.AdvanceReleaseDate;
                }
                drItem[ReqSqlConstants.COL_ITEMMASTERID] = objItem.ItemMasterId == null ? 0 : objItem.ItemMasterId;
                drItem[ReqSqlConstants.COL_ISPREFERREDSUPPLIER] = objItem.IsPreferredSupplier;
                drItem[ReqSqlConstants.COL_ALLOWFLEXIBLEPRICE] = objItem.AllowFlexiblePrice;

                //Add the line Item in dtReqItems
                dtReqItems.Rows.Add(drItem);

                if (objItem.taxItems != null && objItem.taxItems.Any())
                {
                    foreach (var taxes in objItem.taxItems)
                    {
                        var dr = dtReqTaxes.NewRow();
                        dr["RequisitionId"] = RequisitionId;
                        dr["RequisitionItemId"] = objItem.DocumentItemId;
                        dr["LineNumber"] = objItem.ItemLineNumber;
                        dr["TaxId"] = taxes.TaxId;
                        dr["TaxType"] = taxes.TaxType;
                        dr["TaxMode"] = taxes.TaxMode;
                        dr["TaxDescription"] = taxes.TaxDescription;
                        dr["TaxValue"] = taxes.TaxValue;
                        dr["IsDeleted"] = taxes.IsDeleted;
                        dr["IsManual"] = taxes.IsManual;
                        dr["RequisitionTaxId"] = taxes.DocumentTaxId;
                        dtReqTaxes.Rows.Add(dr);
                    }
                }

                //Read the Split ITems
                if (objItem.ItemSplitsDetail != null && objItem.ItemSplitsDetail.Count > 0)
                {
                    int iItemsplitNumber = 1;

                    foreach (RequisitionSplitItems objSplitItem in objItem.ItemSplitsDetail)
                    {
                        if (objItem.IsDeleted == false || objSplitItem.DocumentSplitItemId > 0)
                        {
                            DataRow drSplitItem = dtReqSplitItems.NewRow();
                            drSplitItem["RequisitionID"] = RequisitionId;
                            drSplitItem["RequisitionSplitItemId"] = objSplitItem.DocumentSplitItemId > 0 ? objSplitItem.DocumentSplitItemId : 0;
                            drSplitItem["LineNumber"] = objItem.ItemLineNumber;
                            drSplitItem["SplitNumber"] = iItemsplitNumber;
                            drSplitItem["RequisitionItemId"] = objItem.DocumentItemId;
                            drSplitItem["SplitType"] = objItem.SplitType;
                            drSplitItem["Percentage"] = objSplitItem.Percentage;
                            drSplitItem["Quantity"] = objSplitItem.Quantity;
                            drSplitItem["Tax"] = objSplitItem.Tax;
                            drSplitItem["ShippingCharges"] = objSplitItem.ShippingCharges;
                            drSplitItem["AdditionalCharges"] = objSplitItem.AdditionalCharges;
                            drSplitItem["SplitItemTotal"] = objSplitItem.SplitItemTotal;
                            drSplitItem["IsDeleted"] = objSplitItem.IsDeleted;
                            drSplitItem["OverallLimitSplitItem"] = objSplitItem.OverallLimitSplitItem;
                            //add into datatable
                            dtReqSplitItems.Rows.Add(drSplitItem);

                            //Read the Split Item Entities
                            if (objSplitItem.DocumentSplitItemEntities != null && objSplitItem.DocumentSplitItemEntities.Count > 0)
                            {
                                foreach (DocumentSplitItemEntity objDocumentSplitItemEntity in objSplitItem.DocumentSplitItemEntities)
                                {
                                    DataRow drSplitItemEntities = dtReqSplitItemEntities.NewRow();
                                    drSplitItemEntities["RequisitionID"] = RequisitionId;
                                    drSplitItemEntities["RequisitionSplitItemId"] = objSplitItem.DocumentSplitItemId > 0 ? objSplitItem.DocumentSplitItemId : 0;
                                    drSplitItemEntities["LineNumber"] = objItem.ItemLineNumber;
                                    drSplitItemEntities["SplitNumber"] = iItemsplitNumber;
                                    drSplitItemEntities["SplitAccountingFieldConfigId"] = objDocumentSplitItemEntity.SplitAccountingFieldId;
                                    drSplitItemEntities["SplitAccountingFieldValue"] = objDocumentSplitItemEntity.SplitAccountingFieldValue;
                                    drSplitItemEntities["EntityCode"] = objDocumentSplitItemEntity.EntityCode;

                                    dtReqSplitItemEntities.Rows.Add(drSplitItemEntities);
                                }
                            }
                            iItemsplitNumber++;
                        }
                    }
                }

            }

        }

        private bool BulkCopyRequisitionLineItems(long requisitionId, long uploadedBy, int maxPrecessionValue, int maxPrecessionValueForTotal, int maxPrecessionValueForTaxAndCharges, bool isCallFromWeb = false)
        {
            bool result = true;
            LogHelper.LogInfo(Log, "Requisition BulkCopyRequisitionLineItems Method Started");
            SqlConnection objSqlCon = null;
            RefCountingDataReader objRefCountingDataReader = null;
            try
            {
                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("In BulkCopyRequisitionLineItems Method.",
                                             "SP: usp_P2P_REQ_BulkCopyRequisitionLineItems"));

                var sqlHelper = ContextSqlConn;

                using (SqlCommand objSqlCommand = new SqlCommand(isCallFromWeb ? ReqSqlConstants.USP_P2P_REQ_WEBBULKCOPYREQUISITIONLINEITEMS : ReqSqlConstants.USP_P2P_REQ_BULKCOPYREQUISITIONLINEITEMS))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.CommandTimeout = 0;
                    objSqlCommand.Parameters.AddWithValue("@requisitionID", requisitionId);
                    objSqlCommand.Parameters.AddWithValue("@uploadedBy", uploadedBy);
                    objSqlCommand.Parameters.AddWithValue("@MaxPrecessionValue", maxPrecessionValue);
                    objSqlCommand.Parameters.AddWithValue("@MaxPrecessionValueForTotal", maxPrecessionValueForTotal);
                    objSqlCommand.Parameters.AddWithValue("@MaxPrecessionValueForTaxAndCharges", maxPrecessionValueForTaxAndCharges);
                    objSqlCommand.Parameters.AddWithValue("@cultureCode", UserContext.Culture);
                    result = Convert.ToBoolean(sqlHelper.ExecuteNonQuery(objSqlCommand));
                }
            }
            catch (Exception ex)
            {
                Log.Error("Exception occured in BulkCopyRequisitionLineItems method of Requisition" + ex.Message);
                result = false;
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
                LogHelper.LogInfo(Log, "Requisition BulkCopyRequisitionLineItems Method Ended");
            }
            //Clean the Staging Tables
            if (result)
                DeleteRequisitionUploadStagingData(requisitionId);

            return result;
        }

        private void DeleteRequisitionUploadStagingData(long requisitionId)
        {
            LogHelper.LogInfo(Log, "Requisition DeleteRequisitionUploadStagingData Method Started");
            SqlConnection objSqlCon = null;
            RefCountingDataReader objRefCountingDataReader = null;
            try
            {
                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("In DeleteRequisitionUploadStagingData Method.",
                                             "SP: usp_P2P_DeleteRequisitionUploadStagingData"));

                var sqlHelper = ContextSqlConn;
                objSqlCon = (SqlConnection)sqlHelper.CreateConnection();
                objSqlCon.Open();
                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_DELETEREQUISITIONUPLOADSTAGINGDATA))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.AddWithValue("@requisitionID", requisitionId);
                    sqlHelper.ExecuteNonQuery(objSqlCommand);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Exception occured in DeleteRequisitionUploadStagingData method of Requisition" + ex.Message);
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
                LogHelper.LogInfo(Log, "Requisition DeleteRequisitionUploadStagingData Method Ended");
            }
        }

        #endregion

        public bool SaveRequisitionAccountingDetails(List<P2P_RequisitionSplitItems> requisitionSplitItems, List<P2P_RequisitionSplitItemEntities> requisitionSplitItemEntities, decimal lineItemQuantity, int precessionValue, int maxPrecessionforTotal, int maxPrecessionForTaxesAndCharges, bool UseTaxMaster = true, bool updateTaxes = true)

        {
            bool result = false;
            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;
            try
            {
                LogHelper.LogInfo(Log, "Requisition SaveRequisitionAccountingDetails Method Started for DocumentItemId = " + requisitionSplitItems[0].RequisitionItemId);

                var sqlHelper = ContextSqlConn;
                _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();

                DataTable dtReqItems;
                DataTable dtReqItemEntities = null;
                List<DocumentSplitItemEntity> dsEntities = new List<DocumentSplitItemEntity>();
                foreach (var entity in requisitionSplitItemEntities)
                {
                    DocumentSplitItemEntity dsEntity = new DocumentSplitItemEntity();
                    dsEntity.DocumentSplitItemEntityId = entity.RequisitionSplitItemEntityId;
                    dsEntity.DocumentSplitItemId = entity.RequisitionSplitItemId;
                    dsEntity.SplitAccountingFieldId = entity.SplitAccountingFieldConfigId;
                    dsEntity.SplitAccountingFieldValue = entity.SplitAccountingFieldValue;
                    dsEntity.EntityCode = entity.EntityCode;
                    dsEntity.UiId = entity.UiId;
                    dsEntities.Add(dsEntity);
                }


                dtReqItems = GEP.Cumulus.P2P.DataAccessObjects.DAOHelper.ConvertToDataTable(requisitionSplitItems, GetRequisitionSplitItemTable);
                dtReqItemEntities = GEP.Cumulus.P2P.DataAccessObjects.DAOHelper.ConvertToDataTable(dsEntities,
                                                       GetRequisitionSplitItemEntitiesTable);

                using (var objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_SAVEREQUISITIONACCOUNTINGDETAILSV2, _sqlCon))
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

                    result = Convert.ToBoolean(sqlHelper.ExecuteNonQuery(objSqlCommand, _sqlTrans));
                }
                if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
                    _sqlTrans.Commit();
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
                LogHelper.LogInfo(Log, "Requisition SaveRequisitionAccountingDetails Method Ended for DocumentItemId = " + requisitionSplitItems[0].RequisitionSplitItemId);
            }

            return result;
        }

        private List<KeyValuePair<Type, string>> GetRequisitionSplitItemTable()
        {
            var lstMatchItemProperties = new List<KeyValuePair<Type, string>>();
            lstMatchItemProperties.Add(new KeyValuePair<Type, string>(typeof(long), "RequisitionSplitItemId"));
            lstMatchItemProperties.Add(new KeyValuePair<Type, string>(typeof(long), "RequisitionItemId"));
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

        public SaveResult SaveAllRequisitionDetails(NewP2PEntities.Requisition objDoc, int precessionValue, int maxPrecessionforTotal, int maxPrecessionForTaxesAndCharges, List<NewPlatformEntities.DocumentBU> documentBUs)
        {

            var objDocument = new GEP.Cumulus.Documents.Entities.Document();

            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;
            try
            {
                LogHelper.LogInfo(Log, "Requisition Save Method Started for DocumentId = " + objDoc.documentCode);

                var sqlHelper = ContextSqlConn;
                _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();

                objDocument.DocumentTypeInfo = DocumentType.Requisition;
                objDocument.DocumentSourceTypeInfo = (Documents.Entities.DocumentSourceType)objDoc.source.id;
                objDocument.DocumentCode = objDoc.documentCode;
                objDocument.DocumentName = !string.IsNullOrEmpty(objDoc.name) ? objDoc.name : " ";
                objDocument.DocumentNumber = objDoc.number;
                objDocument.ExistingDocumentNumber = objDoc.ExistingDocumentNumber;
                objDocument.IsDocumentNumberUpdatable = objDoc.IsDocumentNumberUpdatable;
                if ((DocumentStatus)objDoc.status.id == DocumentStatus.None)
                    objDocument.DocumentStatusInfo = DocumentStatus.Draft;
                else objDocument.DocumentStatusInfo = (DocumentStatus)objDoc.status.id;
                if (objDoc.items != null)
                {
                    objDocument.NumberofItems = objDoc.items.Count(e => e.isDeleted == false);
                    objDocument.NumberofPartners = objDoc.items.Select(reqItem => reqItem.partner.id).Distinct().Count();
                    objDoc.IsAdvanceRequsition = objDoc.items.Any(reqItem => reqItem.AllowAdvances == true);
                }
                // objDocument.CompanyName = UserContext.CompanyName;
                if (documentBUs != null)
                    documentBUs.ToList().ForEach(data => { if (data.buCode > 0) objDocument.DocumentBUList.Add(new Documents.Entities.DocumentBU { BusinessUnitCode = data.buCode }); });

                if (objDoc.documentCode > 0)
                {
                    if (objDoc.lastModifiedBy != null)
                        objDocument.ModifiedBy = objDoc.lastModifiedBy.id;

                    objDocument.UpdatedOn = (DateTime)(objDoc.lastModifiedOn != null ? objDoc.lastModifiedOn : DateTime.Now);
                }
                else
                {
                    objDocument.CreatedBy = objDoc.createdBy.id;
                    objDocument.CreatedOn = DateTime.UtcNow;
                    objDocument.UpdatedOn = DateTime.UtcNow;
                    objDocument.ModifiedBy = objDoc.createdBy.id;
                }
                if (objDocument.DocumentStakeHolderList != null && objDocument.DocumentCode <= 0)
                {

                    DocumentStakeHolder objDocumentStakeHolder = new DocumentStakeHolder();
                    objDocument.IsStakeholderDetails = true;
                    objDocumentStakeHolder.IsDeleted = false;

                    objDocumentStakeHolder.DocumentCode = objDoc.documentCode;
                    objDocumentStakeHolder.ContactCode = objDoc.createdBy.id;
                    objDocumentStakeHolder.PartnerCode = UserContext.BuyerPartnerCode;
                    objDocumentStakeHolder.StakeholderTypeInfo = StakeholderType.Author;
                    objDocument.DocumentStakeHolderList.Add(objDocumentStakeHolder);

                }

                objDocument.IsDocumentDetails = true;
                objDocument.IsAddtionalDetails = false;
                objDocument.EntityId = (int)objDoc.documentLOB.entityId;
                objDocument.ACEEntityDetailCode = objDoc.documentLOB.entityDetailCode;
                if (objDocument.EntityDetailCode == null)
                    objDocument.EntityDetailCode = new List<long>();
                objDocument.EntityDetailCode.Add(objDoc.documentLOB.entityDetailCode);


                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("Requisition Save SaveDocumentDetails with parameter: objRequisition=" + objDoc.documentCode, " was called."));


                SQLDocumentDAO sqlDocumentDAO = new SQLDocumentDAO();
                sqlDocumentDAO.SqlTransaction = _sqlTrans;
                sqlDocumentDAO.ReliableSqlDatabase = sqlHelper;
                sqlDocumentDAO.UserContext = UserContext;
                sqlDocumentDAO.GepConfiguration = GepConfiguration;
                objDoc.documentCode = sqlDocumentDAO.SaveDocumentDetails(objDocument);

                #region SaveRequisitionHeader Details
                if (objDoc.businessUnit == null || objDoc.businessUnit.id <= 0)
                    objDoc.BUId = null;
                else
                    objDoc.BUId = objDoc.businessUnit.id;

                if (objDoc.workOrder == null)
                    objDoc.workOrder = "";
                if (objDoc.deliverTo != null && objDoc.deliverTo.id > 0)
                    objDoc.deliverToId = Convert.ToInt32(objDoc.deliverTo.id);
                else
                    objDoc.deliverToId = 0;
                if (objDoc.shipTo == null || objDoc.shipTo.id <= 0)
                    objDoc.ShiptoLocationID = null;
                else
                    objDoc.ShiptoLocationID = Convert.ToInt32(objDoc.shipTo.id);

                if (objDoc.Contract == null || string.IsNullOrEmpty(objDoc.Contract.code))
                    objDoc.ContractNumber = "";
                else
                    objDoc.ContractNumber = Convert.ToString(objDoc.Contract.code);

                if (objDoc.erpOrderType == null)
                    objDoc.ERPOrderTypeId = 0;
                else
                    objDoc.ERPOrderTypeId = Convert.ToInt32(objDoc.erpOrderType.id);
                if (objDoc.pOSignatory != null)
                    objDoc.POSignatoryId = objDoc.pOSignatory.id;
                else
                    objDoc.POSignatoryId = 0;
                if (objDoc.workOrder == null)
                    objDoc.workOrder = null;

                bool result = false;
                if (objDoc.documentCode > 0)
                {
                    using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_SAVEREQUISITIONDEATILS))
                    {

                        objSqlCommand.CommandType = CommandType.StoredProcedure;

                        objSqlCommand.Parameters.AddWithValue("@requisitionid", objDoc.documentCode);
                        objSqlCommand.Parameters.AddWithValue("@requesterid", objDoc.RequesterID);
                        objSqlCommand.Parameters.AddWithValue("@currencycode", objDoc.currency.name);
                        objSqlCommand.Parameters.AddWithValue("@buId", objDoc.businessUnit != null ? objDoc.businessUnit.id : 0);
                        objSqlCommand.Parameters.AddWithValue("@shiptoLocationID", objDoc.shipTo != null ? objDoc.shipTo.id : 0);
                        objSqlCommand.Parameters.AddWithValue("@billtoLocationID", objDoc.billTo != null ? objDoc.billTo.id : 0);
                        objSqlCommand.Parameters.AddWithValue("@purchaseType", objDoc.purchaseType);
                        objSqlCommand.Parameters.AddWithValue("@OnBehalfOf", objDoc.obo != null ? objDoc.obo.id : (objDoc.createdBy != null ? objDoc.createdBy.id : 0));
                        objSqlCommand.Parameters.AddWithValue("@workOrder", objDoc.workOrder);
                        objSqlCommand.Parameters.AddWithValue("@ERPOrderType", objDoc.ERPOrderTypeId);
                        objSqlCommand.Parameters.AddWithValue("@sourceid", objDoc.RequisitionSource);
                        objSqlCommand.Parameters.AddWithValue("@delivertoid", objDoc.deliverToId);
                        objSqlCommand.Parameters.AddWithValue("@deliverto", !string.IsNullOrEmpty(objDoc.deliverToStr) ? objDoc.deliverToStr : ((objDoc.deliverTo != null && !String.IsNullOrEmpty(objDoc.deliverTo.name)) ? Convert.ToString(objDoc.deliverTo.name) : ""));
                        objSqlCommand.Parameters.AddWithValue("@isurgent", objDoc.isUrgent);
                        objSqlCommand.Parameters.AddWithValue("@programId", objDoc.programId);
                        objSqlCommand.Parameters.AddWithValue("@budgetid", objDoc.BudgetId);
                        objSqlCommand.Parameters.AddWithValue("@itemtotal", objDoc.itemTotal);
                        objSqlCommand.Parameters.AddWithValue("@reqamount", objDoc.total);
                        objSqlCommand.Parameters.AddWithValue("@budgetstorystatus", objDoc.budgetoryStatus);
                        objSqlCommand.Parameters.AddWithValue("@additionalcharges", objDoc.otherCharges);
                        objSqlCommand.Parameters.AddWithValue("@tax", objDoc.tax);
                        objSqlCommand.Parameters.AddWithValue("@posignatory", objDoc.POSignatoryId);
                        objSqlCommand.Parameters.AddWithValue("@shipping", objDoc.shipping);
                        objSqlCommand.Parameters.AddWithValue("@basecurrency", !string.IsNullOrEmpty(objDoc.BaseCurrency) ? objDoc.BaseCurrency : objDoc.currency.name);
                        objSqlCommand.Parameters.AddWithValue("@RequisitionTotalChange", objDoc.RequisitionTotalChange);
                        objSqlCommand.Parameters.AddWithValue("@ParentDocumentCode", objDoc.ParentDocumentCode);
                        objSqlCommand.Parameters.AddWithValue("@RevisionNumber", objDoc.RevisionNumber != null ? objDoc.RevisionNumber : "");
                        objSqlCommand.Parameters.AddWithValue("@BuyerAssignee", objDoc.buyerAssigneeName != null ? objDoc.buyerAssigneeName.id : 0);
                        objSqlCommand.Parameters.AddWithValue("@ContractNumber", objDoc.ContractNumber);
                        objSqlCommand.Parameters.AddWithValue("@ProcurementProfileId", objDoc.ProcurementProfileId);
                        objSqlCommand.Parameters.AddWithValue("@EnableRiskForm", objDoc.EnableRiskForm);
                        objSqlCommand.Parameters.AddWithValue("@conversionAmount", objDoc.ConversionAmount);
                        objSqlCommand.Parameters.AddWithValue("@TaxJurisdiction", objDoc.TaxJurisdiction != null ? objDoc.TaxJurisdiction : "");
                        objSqlCommand.Parameters.AddWithValue("@IsAdvanceRequsition", objDoc.IsAdvanceRequsition);
                        objSqlCommand.Parameters.AddWithValue("@CostApprover", objDoc.CostApprover.HasValue ? objDoc.CostApprover.Value : 0);
                        objSqlCommand.Parameters.AddWithValue("@OnHeldBy", objDoc.OnHeldBy);
                        SqlParameter objSqlParameter = new SqlParameter("@tvp_P2P_DocumentAdditionalEntity", SqlDbType.Structured)
                        {
                            TypeName = ReqSqlConstants.TVP_P2P_DOCUMENTADDITIONALENTITY,
                            Value = ConvertRequisitionEntitiesToTableTypes(objDoc.HeaderSplitAccountingFields)
                        };
                        objSqlCommand.Parameters.Add(objSqlParameter);
                        objSqlCommand.Parameters.AddWithValue("@requestFormId", objDoc.requestFormId);

                        result = Convert.ToBoolean(sqlHelper.ExecuteNonQuery(objSqlCommand, _sqlTrans));
                    }

                    #endregion

                    if (!ReferenceEquals(_sqlTrans, null))
                        _sqlTrans.Commit();
                    if (result)
                    {
                        //Converts 2.0 Items to 1.0 Items;
                        var convertedReqItems = GetConvertedSaveBulkRequisitionItems(objDoc);
                        result = SaveBulkRequisitionItems(objDoc.documentCode, convertedReqItems, precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges, true, objDoc.purchaseType);

                        if (result)
                        {
                            DataTable dtRequisitionItemAdditionalFields = new DataTable("RequisitionItemAdditionalFields");
                            dtRequisitionItemAdditionalFields.Columns.Add(ReqSqlConstants.COL_REQUISITION_ID, typeof(long));
                            dtRequisitionItemAdditionalFields.Columns.Add(ReqSqlConstants.COL_REQUISITION_ITEM_ID, typeof(long));
                            dtRequisitionItemAdditionalFields.Columns.Add(ReqSqlConstants.COL_ADDITIONALFIELDID, typeof(Int32));
                            dtRequisitionItemAdditionalFields.Columns.Add(ReqSqlConstants.COL_ADDITIONALFIELDVALUE, typeof(string));
                            dtRequisitionItemAdditionalFields.Columns.Add(ReqSqlConstants.COL_ADDITIONALFIELDCODE, typeof(string));
                            dtRequisitionItemAdditionalFields.Columns.Add(ReqSqlConstants.COL_CREATEDBYID, typeof(long));
                            dtRequisitionItemAdditionalFields.Columns.Add(ReqSqlConstants.COL_FEATUREID, typeof(Int32));
                            dtRequisitionItemAdditionalFields.Columns.Add(ReqSqlConstants.COL_ADDITIONALFIELDDETAILCODE, typeof(Int64));
                            dtRequisitionItemAdditionalFields.Columns.Add(ReqSqlConstants.COL_IS_DELETED, typeof(bool));
                            dtRequisitionItemAdditionalFields.Columns.Add(ReqSqlConstants.COL_SOURCEDOCUMENTTYPEID, typeof(int));

              List<QuestionResponseAttachment> lstQuestionResponseAttachment = new List<QuestionResponseAttachment>();
                            List<QuestionResponse> lstQuestionsResponse = new List<QuestionResponse>();
                            GEP.Cumulus.P2P.DataAccessObjects.SQLServer.SQLCommonDAO objComDao = new GEP.Cumulus.P2P.DataAccessObjects.SQLServer.SQLCommonDAO { UserContext = UserContext, GepConfiguration = GepConfiguration };
                            var objReq = objComDao.GetRequisitionDetailsForBulkUploadReqLines(objDoc.documentCode);
                            var bulkItems = objReq.RequisitionItems;
                            objDoc = MapBulkReqItems(objDoc, objReq);

                            if (objDoc.HeaderCustomAttribs != null && objDoc.HeaderCustomAttribs.Count > 0)
                            {
                                #region save question and response/customattributes
                                objDoc.HeaderCustomAttribs.ForEach(qResponse =>
                                {
                                    qResponse.ObjectInstanceId = objDoc.documentCode;
                                    lstQuestionsResponse.Add(qResponse);
                                });
                                #endregion
                            }
                            if (objDoc.lstAdditionalFieldAttributues != null && objDoc.lstAdditionalFieldAttributues.Count > 0)
                            {
                                SaveRequisitionHeaderAdditionalFields(objDoc);
                            }

                            List<long> srfquestionIdList = new List<long>();
                            var rItem = objDoc.items.Select(x => x).Where(x => x.isDeleted == false).ToList();
                            for (int i = 0; i < rItem.Count(); i++)
                            {

                                if (objDoc.items[i].ItemCustomAttribs != null)
                                    objDoc.items[i].ItemCustomAttribs.ForEach(x =>
                                    {
                                        x.ObjectInstanceId = rItem[i].id;
                                        if (x.ObjectInstanceId > 0)
                                            lstQuestionsResponse.Add(x);
                                    });
                                if (objDoc.items[i].ItemCustomAttribs != null && objDoc.items[i].SmartFormId != 0 && objDoc.items[i].SmartFormId > 0)
                                {
                                    objDoc.items[i].ItemCustomAttribs.ForEach(x =>
                                    {

                                        srfquestionIdList.Add(x.QuestionId);
                                    });
                                }

                                if (objDoc.items[i].ItemCustomAttrisAttachment != null)
                                    objDoc.items[i].ItemCustomAttrisAttachment.ForEach(x =>
                                    {
                                        x.ObjectInstanceId = rItem[i].id;
                                        if (x.ObjectInstanceId > 0)
                                            lstQuestionResponseAttachment.Add(x);
                                    });

                                if (rItem[i].splits != null)
                                {
                                    var itemSplits = rItem[i].splits.Select(x => x).Where(x => x.isDeleted == false).ToList();
                                    for (int j = 0; j < itemSplits.Count(); j++)
                                    {
                                        if (objDoc.items[i].splits != null && objDoc.items[i].splits[j].SplitCustomAttribs != null)
                                            objDoc.items[i].splits[j].SplitCustomAttribs.ForEach(qRes =>
                                            {
                                                qRes.ObjectInstanceId = itemSplits[j].documentItemId;
                                                if (qRes.ObjectInstanceId > 0)
                                                    lstQuestionsResponse.Add(qRes);
                                            });
                                    }
                                }

                                if (objDoc.items[i].lstAdditionalFieldAttributues != null && objDoc.items[i].lstAdditionalFieldAttributues.Any())
                                {

                                    objDoc.items[i].lstAdditionalFieldAttributues.ForEach(x =>
                                    {
                                        dtRequisitionItemAdditionalFields.Rows.Add(objDoc.documentCode, objDoc.items[i].id, x.AdditionalFieldID, x.AdditionalFieldValue, x.AdditionalFieldCode, objDoc.createdBy.id, x.FeatureId, x.AdditionalFieldDetailCode, x.isDeleted,x.SourceDocumentTypeId);
                                    });

                                }

                            }
                            if (lstQuestionsResponse != null && lstQuestionsResponse.Any() && lstQuestionsResponse.Count > 0)
                                SaveQuestionsResponse(lstQuestionsResponse, objDoc.documentCode);
                            if (srfquestionIdList != null && srfquestionIdList.Count > 0)
                            {
                                SaveSRFQuestionsforReqItemId(srfquestionIdList, objDoc.documentCode);
                            }

                            if (lstQuestionResponseAttachment != null && lstQuestionResponseAttachment.Any() && lstQuestionResponseAttachment.Count > 0)
                            {
                                SaveQuestionResponseAttachment(lstQuestionResponseAttachment);
                            }

                            if (dtRequisitionItemAdditionalFields != null && dtRequisitionItemAdditionalFields.Rows.Count > 0)
                            {
                                SaveRequisitionItemAdditionFileds(dtRequisitionItemAdditionalFields);
                            }
                        }
                    }
                    _sqlTrans = _sqlCon.BeginTransaction();
                    sqlDocumentDAO.SqlTransaction = _sqlTrans;
                    SQLRequisitionDAO objReqDao = new SQLRequisitionDAO { UserContext = UserContext, GepConfiguration = GepConfiguration };
                    if (objDoc.lstNotesOrAttachments != null)
                    {
                        foreach (var NotesAndAttachment in objDoc.lstNotesOrAttachments)
                        {
                            if (NotesAndAttachment.DocumentCode == 0)
                            {
                                NotesAndAttachment.DocumentCode = objDoc.documentCode;
                                objReqDao.SaveNotesAndAttachments(NotesAndAttachment);
                            }
                        }
                    }

                    objReqDao.SaveRequisitionAdditionalDetails(objDoc.documentCode, sqlDocumentDAO);

                    if (objDoc.RequisitionSource == 3)
                        objReqDao.SaveRequisitionAdditionalDetails(objDoc.ParentDocumentCode, sqlDocumentDAO);

                    if (!ReferenceEquals(_sqlTrans, null))
                        _sqlTrans.Commit();

                    if (objDoc.documentCode > 0)
                        AddIntoSearchIndexerQueueing(objDoc.documentCode, REQ, UserContext, GepConfiguration);

                    if (objDoc.RequisitionSource == 3)
                        AddIntoSearchIndexerQueueing(objDoc.ParentDocumentCode, REQ, UserContext, GepConfiguration);
                }
                return new SaveResult { success = result, id = objDoc.documentCode, number = objDoc.number, requisition = new NewP2PEntities.Requisition() };
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
                LogHelper.LogInfo(Log, "Requisition SaveAllRequisitionDetails Method Ended for DocumentItemId = " + objDoc.id);
            }
            return new SaveResult { success = true, id = objDoc.id, number = objDoc.number, requisition = new NewP2PEntities.Requisition() };


        }

        public void SaveRequisitionHeaderAdditionalFields(NewP2PEntities.Requisition objDoc)
        {
            SqlConnection objSqlCon = null;
            SqlTransaction objSqlTrans = null;
            try
            {
                using (DataTable dtHeaderAdditionalFields = new DataTable("RequisitionHeaderAdditionalFields"))
                {
                    dtHeaderAdditionalFields.Columns.Add(ReqSqlConstants.COL_FEATUREID, typeof(Int32));
                    dtHeaderAdditionalFields.Columns.Add(ReqSqlConstants.COL_ADDITIONALFIELDID, typeof(Int32));
                    dtHeaderAdditionalFields.Columns.Add(ReqSqlConstants.COL_ADDITIONALFIELDVALUE, typeof(string));
                    dtHeaderAdditionalFields.Columns.Add(ReqSqlConstants.COL_ADDITIONALFIELDCODE, typeof(string));
                    dtHeaderAdditionalFields.Columns.Add(ReqSqlConstants.COL_ADDITIONALFIELDDETAILCODE, typeof(Int64));
                    dtHeaderAdditionalFields.Columns.Add(ReqSqlConstants.COL_CREATEDBYID, typeof(long));
                    dtHeaderAdditionalFields.Columns.Add(ReqSqlConstants.COL_IS_DELETED, typeof(bool));
          dtHeaderAdditionalFields.Columns.Add(ReqSqlConstants.COL_SOURCEDOCUMENTTYPEID, typeof(int));

          objDoc.lstAdditionalFieldAttributues.ForEach(x =>
                    {
                        dtHeaderAdditionalFields.Rows.Add(x.FeatureId, x.AdditionalFieldID, x.AdditionalFieldValue, x.AdditionalFieldCode, x.AdditionalFieldDetailCode, objDoc.createdBy.id, x.isDeleted, x.SourceDocumentTypeId);
                    });
                    ReliableSqlDatabase sqlHelper = ContextSqlConn;
                    objSqlCon = (SqlConnection)sqlHelper.CreateConnection();
                    using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_SAVEREQUISITIONHEADERADDITIONALFIELDS))
                    {
                        objSqlCommand.CommandType = CommandType.StoredProcedure;
                        objSqlCommand.Parameters.Add(new SqlParameter("@documentCode", SqlDbType.BigInt) { Value = objDoc.documentCode });
                        objSqlCommand.Parameters.Add(new SqlParameter() { ParameterName = "@RequisitionHeaderAdditionalFields", SqlDbType = SqlDbType.Structured, Value = dtHeaderAdditionalFields });

                        objSqlCon.Open();
                        objSqlTrans = objSqlCon.BeginTransaction();

                        sqlHelper.ExecuteNonQuery(objSqlCommand, objSqlTrans);

                        objSqlTrans.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                if (objSqlTrans != null)
                {
                    objSqlTrans.Rollback();
                }
                LogHelper.LogError(Log, "Error occured in SaveRequisitionHeaderAdditionalFields method", ex);
            }
            finally
            {
                if (objSqlCon != null && objSqlCon.State != ConnectionState.Closed)
                {
                    objSqlCon.Close();
                }
            }
        }

        public void SaveRequisitionItemAdditionFileds(DataTable dtRequisitionItemAdditionalFields)
        {
            SqlConnection objSqlCon = null;
            SqlTransaction objSqlTrans = null;
            try
            {
                ReliableSqlDatabase sqlHelper = ContextSqlConn;
                objSqlCon = (SqlConnection)sqlHelper.CreateConnection();

                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_SAVEREQUISITIONITEMFIELDS))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.Add(new SqlParameter() { ParameterName = "@RequisitionItemAdditionalFields", SqlDbType = SqlDbType.Structured, Value = dtRequisitionItemAdditionalFields });

                    objSqlCon.Open();
                    objSqlTrans = objSqlCon.BeginTransaction();

                    sqlHelper.ExecuteNonQuery(objSqlCommand, objSqlTrans);

                    objSqlTrans.Commit();
                }
            }
            catch (Exception ex)
            {
                if (objSqlTrans != null)
                {
                    objSqlTrans.Rollback();
                }
                LogHelper.LogError(Log, "Error occured in SaveRequisitionItemAdditionFileds method", ex);
            }
            finally
            {
                if (objSqlCon != null && objSqlCon.State != ConnectionState.Closed)
                {
                    objSqlCon.Close();
                }
            }
        }

        public void SaveQuestionResponseAttachment(List<QuestionResponseAttachment> lstQuestionResponseAttachment)
        {

            SqlConnection objSqlCon = null;
            SqlTransaction objSqlTrans = null;
            try
            {
                using (DataTable dtQuestionResponseAttachment = new DataTable("QuestionResponseAttachment"))
                {
                    dtQuestionResponseAttachment.Locale = System.Globalization.CultureInfo.InvariantCulture;
                    dtQuestionResponseAttachment.Columns.Add("AttachmentId", typeof(long));
                    dtQuestionResponseAttachment.Columns.Add("QuestionId", typeof(long));
                    dtQuestionResponseAttachment.Columns.Add("AssessorId", typeof(long));
                    dtQuestionResponseAttachment.Columns.Add("AssesseeId", typeof(long));
                    dtQuestionResponseAttachment.Columns.Add("AssessorType", typeof(int));
                    dtQuestionResponseAttachment.Columns.Add("ObjectInstanceId", typeof(long));
                    dtQuestionResponseAttachment.Columns.Add("FileName", typeof(string));
                    dtQuestionResponseAttachment.Columns.Add("IsDeleted", typeof(bool));

                    lstQuestionResponseAttachment.ForEach(objQuesResp =>
                    {
                        dtQuestionResponseAttachment.Rows.Add(objQuesResp.AttachmentId, objQuesResp.QuestionId, objQuesResp.AssessorId,
                                                   objQuesResp.AssesseeId, (int)objQuesResp.AssessorType,
                                                   objQuesResp.ObjectInstanceId, objQuesResp.FileName, objQuesResp.IsDeleted);

                    });


                    ReliableSqlDatabase sqlHelper = ContextSqlConn;
                    objSqlCon = (SqlConnection)sqlHelper.CreateConnection();

                    using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_QB_SAVEQUESTIONRESPONSEATTACHMENT))
                    {
                        objSqlCommand.CommandType = CommandType.StoredProcedure;
                        objSqlCommand.Parameters.Add(new SqlParameter() { ParameterName = "@QuestionResponseAttachment", SqlDbType = SqlDbType.Structured, Value = dtQuestionResponseAttachment });

                        objSqlCon.Open();
                        objSqlTrans = objSqlCon.BeginTransaction();

                        sqlHelper.ExecuteNonQuery(objSqlCommand, objSqlTrans);

                        objSqlTrans.Commit();
                    }


                }
            }
            catch (Exception ex)
            {
                if (objSqlTrans != null)
                {
                    objSqlTrans.Rollback();
                }
                LogHelper.LogError(Log, "Error occured in SaveQuestionResponseAttachment method. List<QuestionResponseAttachment> : " + lstQuestionResponseAttachment.ToJSON(), ex);
                CustomFault objCustomFault = new CustomFault("Error while Save Question Response Attachment ", "SaveQuestionResponseAttachment", "SaveQuestionResponseAttachment", "SaveQuestionsResponse", ExceptionType.ApplicationException, string.Empty, false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while Saving Questions Responses");
            }
            finally
            {
                if (objSqlCon != null && objSqlCon.State != ConnectionState.Closed)
                {
                    objSqlCon.Close();
                }
            }

        }

        private NewP2PEntities.Requisition MapBulkReqItems(NewP2PEntities.Requisition req, BusinessEntities.Requisition objReq)
        {
            //objReq.RequisitionItems[0].DocumentItemId;
            if (req.items != null && objReq.RequisitionItems != null)
                req.items.Select(e =>
                {
                    e.id = objReq.RequisitionItems.Where(s => (e.lineReferenceNumber <= 0 && s.ItemLineNumber == e.lineNumber) || s.P2PLineItemId == e.p2PLineItemId).Select(r => r.DocumentItemId).FirstOrDefault(); return e;
                }).ToList();

            return req;
        }

        private DataTable ConvertRequisitionEntitiesToTableTypes(List<SplitAccountingFields> HeaderSplitAccountingFields)
        {
            DataTable dtRequisitionItem = new DataTable() { Locale = CultureInfo.InvariantCulture, TableName = ReqSqlConstants.TVP_P2P_DOCUMENTADDITIONALENTITY };

            dtRequisitionItem.Columns.Add("EntityId", typeof(Int64));
            dtRequisitionItem.Columns.Add("EntityDetailCode", typeof(long));
            if (HeaderSplitAccountingFields != null)
            {
                foreach (var objRequisitonItem in HeaderSplitAccountingFields)
                {
                    DataRow dr = dtRequisitionItem.NewRow();
                    dr["EntityId"] = objRequisitonItem.EntityTypeId;
                    dr["EntityDetailCode"] = objRequisitonItem.EntityDetailCode;
                    dtRequisitionItem.Rows.Add(dr);
                }
            }
            return dtRequisitionItem;
        }

        private List<GEP.Cumulus.P2P.BusinessEntities.RequisitionItem> GetConvertedSaveBulkRequisitionItems(NewP2PEntities.Requisition req)
        {
            List<GEP.Cumulus.P2P.BusinessEntities.RequisitionItem> lstReqItems = new List<GEP.Cumulus.P2P.BusinessEntities.RequisitionItem>();
            foreach (var item in req.items)
            {
                long lReqItemId = item.id > 10000000 ? 0 : item.id;
                if (lReqItemId > 0 || (lReqItemId == 0 && item.isDeleted == false))
                {
                    GEP.Cumulus.P2P.BusinessEntities.RequisitionItem reqItem = new GEP.Cumulus.P2P.BusinessEntities.RequisitionItem();
                    reqItem.DocumentItemId = lReqItemId;
                    reqItem.DocumentId = req.documentCode;
                    reqItem.ItemLineNumber = item.lineNumber;
                    reqItem.ShortName = item.name;
                    reqItem.Description = item.description;
                    reqItem.UnitPrice = Convert.ToDecimal(item.unitPrice);
                    reqItem.Quantity = Convert.ToDecimal(item.quantity);
                    reqItem.UOM = item.uom != null ? item.uom.code : "";
                    reqItem.DateNeeded = item.needByDate;
                    reqItem.DateRequested = item.requestedDate;
                    reqItem.PartnerCode = item.partner != null ? item.partner.id : 0;
                    reqItem.ManufacturerName = item.manufacturer;
                    reqItem.ManufacturerPartNumber = item.manufacturerPartNumber;
                    reqItem.CategoryId = item.category != null ? item.category.id : 0;
                    reqItem.ItemType = item.type != null ? (ItemType)item.type.id : ItemType.Material;
                    reqItem.Currency = item.currencyCode;
                    reqItem.StartDate = item.startDate;
                    reqItem.EndDate = item.endDate;
                    reqItem.AdditionalCharges = Convert.ToDecimal(item.otherCharges);
                    reqItem.ShippingCharges = Convert.ToDecimal(item.shippingCharges);
                    reqItem.Tax = Convert.ToDecimal(item.taxes);
                    reqItem.SplitType = (SplitType)item.splitType;
                    reqItem.CatalogItemId = item.catalogItemId.HasValue ? item.catalogItemId.Value : reqItem.CatalogItemId;
                    reqItem.SupplierPartAuxiliaryId = item.supplierPartAuxiliaryId == null ? string.Empty : item.supplierPartAuxiliaryId;
                    reqItem.SupplierPartId = item.partnerItemNumber == null ? string.Empty : item.partnerItemNumber;
                    reqItem.IsTaxExempt = item.isTaxExempt;
                    reqItem.ItemExtendedType = item.type != null ? (Cumulus.P2P.BusinessEntities.ItemExtendedType)item.type.id : ItemExtendedType.Material;
                    reqItem.ItemNumber = item.buyerItemNumber == null ? String.Empty : (item.buyerItemNumber.Length > 50 ? item.buyerItemNumber.Substring(0, 50) : item.buyerItemNumber);
                    reqItem.ExtContractRef = item.contractNumber == null ? String.Empty : item.contractNumber;
                    reqItem.ContractName = item.contractName == null ? String.Empty : item.contractName;
                    reqItem.IsDeleted = item.isDeleted;
                    reqItem.TrasmissionMode = item.TrasmissionMode;
                    reqItem.TransmissionValue = item.TransmissionValue;
                    reqItem.OverallItemLimit = item.OverallItemLimit;
                    reqItem.InventoryType = item.inventoryType;
                    reqItem.ProcurementStatus = item.ProcurementStatus != 0 ? item.ProcurementStatus : (item.isProcurable != null ? Convert.ToInt16(item.isProcurable.id) : 0);
                    reqItem.IsProcurable = item.isProcurable != null ? Convert.ToInt16(item.isProcurable.id) : 0;
                    if (item.ContractItems != null)
                    {
                        reqItem.ContractItemId = item.ContractItems.id;
                        reqItem.ContractLineRef = item.ContractItems.name;
                    }
                    if (item.orderingLocation == null)
                    {
                        reqItem.OrderLocationId = 0;
                    }
                    else
                        reqItem.OrderLocationId = item.orderingLocation.id;


                    reqItem.PartnerContactId = item.partnerContact == null ? 0 : item.partnerContact.id;
                    reqItem.ManufacturerModel = item.ManufacturerModel;
                    reqItem.ManufacturerName = item.manufacturer;
                    reqItem.ManufacturerPartNumber = item.manufacturerPartNumber;
                    reqItem.ShipToLocationId = Convert.ToInt32(item.shipTo != null ? item.shipTo.id : 0);
                    reqItem.SourceType = item.source != null ? (ItemSourceType)item.source.id : ItemSourceType.Manual;
                    reqItem.ShippingMethod = item.shippingMethod;
                    reqItem.DelivertoLocationID = item.deliverTo == null ? 0 : Convert.ToInt32(item.deliverTo.id);
                    reqItem.DelivertoStr = item.deliverToStr;
                    reqItem.MatchType = (BusinessEntities.MatchType)(item.matching != null ? item.matching.id : 0);
                    reqItem.PriceTypeId = item.PriceTypeId;
                    reqItem.JobTitleId = item.JobTitleId;
                    reqItem.ContingentWorkerId = item.ContingentWorkerId;
                    reqItem.Margin = item.Margin;
                    reqItem.BaseRate = item.BaseRate;
                    reqItem.ReportingManagerId = item.ReportingManagerId;
                    reqItem.SmartFormId = item.SmartFormId;
                    reqItem.PaymentTermId = Convert.ToInt32(item.paymentTerms != null ? item.paymentTerms.id : 0);
                    if (item.incoTermCode != null)
                    {
                        reqItem.IncoTermId = Convert.ToInt32(item.incoTermCode != null ? item.incoTermCode.id : 0);
                        reqItem.IncoTermCode = item.incoTermCode.name != null ? item.incoTermCode.name : string.Empty;
                        reqItem.IncoTermLocation = item.incoTermLocation != null ? item.incoTermLocation : string.Empty;
                    }
                    if (item.ShipFromLocation == null)
                    {
                        reqItem.ShipFromLocationId = 0;
                    }
                    else
                        reqItem.ShipFromLocationId = item.ShipFromLocation.id;

                    reqItem.SupplierUOMCode = item.supplierUOMCode == null ? string.Empty : item.supplierUOMCode;
                    int iItemsplitNumber = 1;

                    reqItem.taxItems = new List<Taxes>();
                    if (item.taxItems != null && item.taxItems.Count > 0)
                    {
                        foreach (var taxes in item.taxItems)
                        {
                            Taxes tax = new Taxes();
                            tax.TaxCode = taxes.code;
                            tax.TaxDescription = taxes.description;
                            tax.TaxId = taxes.taxId;
                            tax.TaxValue = taxes.value;
                            tax.IsDeleted = false;
                            tax.IsManual = true;
                            tax.TaxValue = taxes.percent;
                            tax.DocumentTaxId = lReqItemId == 0 ? 0 : taxes.id;
                            if (taxes.type != null)
                            {
                                tax.TaxType = (TaxType)taxes.type.id;
                            }
                            reqItem.taxItems.Add(tax);
                        }
                    }
                    reqItem.ItemSplitsDetail = new List<RequisitionSplitItems>();

                    if (item.splits != null)
                    {
                        foreach (var objSplitItem in item.splits)
                        {
                            objSplitItem.id = objSplitItem.id > 10000000 ? 0 : objSplitItem.id;
                            RequisitionSplitItems oldSplitItem = new RequisitionSplitItems();
                            oldSplitItem.SplitNumber = iItemsplitNumber;
                            oldSplitItem.DocumentItemId = objSplitItem.documentItemId;
                            oldSplitItem.SplitType = (SplitType)objSplitItem.SplitType;
                            oldSplitItem.Percentage = objSplitItem.percentage;
                            oldSplitItem.Quantity = (decimal)objSplitItem.quantity;
                            oldSplitItem.Tax = objSplitItem.tax;
                            oldSplitItem.ShippingCharges = objSplitItem.shippingCharges;
                            oldSplitItem.AdditionalCharges = objSplitItem.additionalCharges;
                            oldSplitItem.SplitItemTotal = objSplitItem.splitItemTotal;
                            oldSplitItem.DocumentSplitItemId = objSplitItem.id;
                            oldSplitItem.IsDeleted = objSplitItem.isDeleted;
                            oldSplitItem.OverallLimitSplitItem = (item.OverallItemLimit * objSplitItem.percentage) / 100;
                            //Read the Split Item Entities
                            if (objSplitItem != null)
                            {
                                oldSplitItem.DocumentSplitItemEntities = getSplitItemEntities(objSplitItem);
                            }
                            reqItem.ItemSplitsDetail.Add(oldSplitItem);
                            iItemsplitNumber++;
                        }
                    }
                    reqItem.SpendControlDocumentCode = item.SpendControlDocumentCode;
                    reqItem.SpendControlDocumentItemId = item.SpendControlDocumentItemId;
                    reqItem.SpendControlDocumentNumber = item.SpendControlDocumentNumber;
                    reqItem.ConversionFactor = item.ConversionFactor;
                    reqItem.TaxJurisdiction = item.TaxJurisdiction;
                    reqItem.Itemspecification = item.Itemspecification;
                    reqItem.InternalPlantMemo = item.InternalPlantMemo;
                    reqItem.AllowAdvances = item.AllowAdvances;
                    reqItem.AdvancePercentage = item.AdvancePercentage == null ? 0 : Convert.ToDecimal(item.AdvancePercentage);
                    reqItem.AdvanceAmount = item.AdvanceAmount == null ? 0 : Convert.ToDecimal(item.AdvanceAmount);
                    reqItem.AdvanceReleaseDate = item.AdvanceReleaseDate;
                    reqItem.ItemMasterId = item.ItemMasterId == null ? 0 : Convert.ToInt64(item.ItemMasterId);
                    if (reqItem.PartnerCode <= 0)
                        reqItem.IsPreferredSupplier = false;
                    else
                        reqItem.IsPreferredSupplier = item.IsPreferredSupplier;
                    reqItem.AllowFlexiblePrice = item.AllowFlexiblePrice;
                    lstReqItems.Add(reqItem);

                }
            }
            return lstReqItems;
        }

        private List<Cumulus.P2P.BusinessEntities.DocumentSplitItemEntity> getSplitItemEntities(ReqAccountingSplit split)
        {
            try
            {
                List<Cumulus.P2P.BusinessEntities.DocumentSplitItemEntity> lstSplitEntity = new List<Cumulus.P2P.BusinessEntities.DocumentSplitItemEntity>();

                if (split.requester != null)
                    lstSplitEntity.Add(getSplitEntity(split.requester, split.id));
                if (split.gLCode != null)
                    lstSplitEntity.Add(getSplitEntity(split.gLCode, split.id));
                if (split.period != null)
                    lstSplitEntity.Add(getSplitEntity(split.period, split.id));

                for (int cnt = 0; cnt < split.SplitEntities.Count; cnt++)
                {
                    lstSplitEntity.Add(getSplitEntity(split.SplitEntities[cnt], split.id));
                }

                /*Below Code is commented because we are implementing dynamic splitentities  REQ-4496
                if (split.splitEntity1 != null)
                {
                    lstSplitEntity.Add(getSplitEntity(split.splitEntity1, split.id));
                    if (split.splitEntity2 != null)
                    {
                        lstSplitEntity.Add(getSplitEntity(split.splitEntity2, split.id));
                        if (split.splitEntity3 != null)
                        {
                            lstSplitEntity.Add(getSplitEntity(split.splitEntity3, split.id));
                            if (split.splitEntity4 != null)
                            {
                                lstSplitEntity.Add(getSplitEntity(split.splitEntity4, split.id));
                                if (split.splitEntity5 != null)
                                {
                                    lstSplitEntity.Add(getSplitEntity(split.splitEntity5, split.id));
                                    if (split.splitEntity6 != null)
                                    {
                                        lstSplitEntity.Add(getSplitEntity(split.splitEntity6, split.id));
                                        if (split.splitEntity7 != null)
                                        {
                                            lstSplitEntity.Add(getSplitEntity(split.splitEntity7, split.id));
                                            if (split.splitEntity8 != null)
                                            {
                                                lstSplitEntity.Add(getSplitEntity(split.splitEntity8, split.id));
                                                if (split.splitEntity9 != null)
                                                {
                                                    lstSplitEntity.Add(getSplitEntity(split.splitEntity9, split.id));
                                                    if (split.splitEntity10 != null)
                                                    {
                                                        lstSplitEntity.Add(getSplitEntity(split.splitEntity10, split.id));
                                                        if (split.splitEntity11 != null)
                                                        {
                                                            lstSplitEntity.Add(getSplitEntity(split.splitEntity11, split.id));
                                                            if (split.splitEntity12 != null)
                                                            {
                                                                lstSplitEntity.Add(getSplitEntity(split.splitEntity12, split.id));
                                                                if (split.splitEntity13 != null)
                                                                {
                                                                    lstSplitEntity.Add(getSplitEntity(split.splitEntity13, split.id));
                                                                    if (split.splitEntity14 != null)
                                                                    {
                                                                        lstSplitEntity.Add(getSplitEntity(split.splitEntity14, split.id));
                                                                        if (split.splitEntity15 != null)
                                                                        {
                                                                            lstSplitEntity.Add(getSplitEntity(split.splitEntity15, split.id));
                                                                            if (split.splitEntity16 != null)
                                                                            {
                                                                                lstSplitEntity.Add(getSplitEntity(split.splitEntity16, split.id));
                                                                                if (split.splitEntity17 != null)
                                                                                {
                                                                                    lstSplitEntity.Add(getSplitEntity(split.splitEntity17, split.id));
                                                                                    if (split.splitEntity18 != null)
                                                                                    {
                                                                                        lstSplitEntity.Add(getSplitEntity(split.splitEntity18, split.id));
                                                                                        if (split.splitEntity19 != null)
                                                                                        {
                                                                                            lstSplitEntity.Add(getSplitEntity(split.splitEntity19, split.id));
                                                                                            if (split.splitEntity20 != null)
                                                                                            {
                                                                                                lstSplitEntity.Add(getSplitEntity(split.splitEntity20, split.id));
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                */
                return lstSplitEntity;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error Occured in getSplitItemEntities method", ex);
                throw;
            }
        }

        private Cumulus.P2P.BusinessEntities.DocumentSplitItemEntity getSplitEntity(SplitEntity accSplitEnt, long SplitItemId)
        {
            try
            {
                Cumulus.P2P.BusinessEntities.DocumentSplitItemEntity objSplitEntity = new Cumulus.P2P.BusinessEntities.DocumentSplitItemEntity()
                {
                    DocumentSplitItemId = SplitItemId,
                    DocumentSplitItemEntityId = accSplitEnt.splitEntityId,
                    EntityCode = accSplitEnt.entityCode,
                    EntityTypeId = accSplitEnt.entityType,
                    EntityDisplayName = accSplitEnt.name,
                    SplitAccountingFieldId = (int)accSplitEnt.fieldId,
                    SplitAccountingFieldValue = accSplitEnt.code
                };

                return objSplitEntity;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error Occured in getSplitEntity method", ex);
                throw;
            }

        }
        public bool UpdateLineStatusForRequisition(long RequisitionId, BusinessEntities.StockReservationStatus LineStatus, bool IsUpdateAllItems, List<BusinessEntities.LineStatusRequisition> Items)
        {
            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;
            try
            {
                LogHelper.LogInfo(Log, "Requisition UpdateLineStatusForRequisition Method Started for DocumentItemId = " + RequisitionId);
                var sqlHelper = ContextSqlConn;
                _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();
                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("Requisition UpdateLineStatusForRequisition sp usp_P2P_REQ_UpdateRequsitionItemStatusFromInterface with parameter: ;=" + RequisitionId + " was called."));
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
                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.usp_P2P_REQ_UpdateRequsitionItemStatusFromInterface))
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

                    result = Convert.ToBoolean(sqlHelper.ExecuteNonQuery(objSqlCommand, _sqlTrans), NumberFormatInfo.InvariantInfo);
                    _sqlTrans.Commit();
                }
                if (result && RequisitionId > 0)
                    AddIntoSearchIndexerQueueing(RequisitionId, REQ, UserContext, GepConfiguration);
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

                LogHelper.LogInfo(Log, "Requisition UpdateLineStatusForRequisition Method Ended for DocumentItemId = " + RequisitionId);
            }
        }

        public bool UpdateRequisitionItemFlipType(BusinessEntities.Requisition objRequisition)
        {
            SqlConnection _sqlCon = null;
            long documentCode = objRequisition.DocumentCode;
            List<BusinessEntities.RequisitionItem> items = objRequisition.RequisitionItems;
            try
            {
                LogHelper.LogInfo(Log, "Requisition UpdateRequisitionItemFlipType Method Started for documentCode = " + documentCode);
                ReliableSqlDatabase sqlHelper = ContextSqlConn;
                _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                _sqlCon.Open();
                if (Log.IsDebugEnabled)
                {
                    Log.Debug(string.Concat("Requisition UpdateRequisitionItemFlipType sp USP_P2P_REQ_UpdateRequisitionItemFlipType with parameter: " + items.ToJSON(10, 50000000) + " was called."));
                }

                bool result = false;
                DataTable dtLineStatus = new DataTable();
                dtLineStatus.Columns.Add("RequisitionItemID", typeof(long));
                dtLineStatus.Columns.Add("P2PLineItemID", typeof(long));
                dtLineStatus.Columns.Add("RequisitionID", typeof(long));
                dtLineStatus.Columns.Add("RFXFlipType", typeof(int));

                if (items != null && items.Any())
                {
                    foreach (BusinessEntities.RequisitionItem item in items.Where(i => i.RFXFlipType != 0))
                    {
                        DataRow dr = dtLineStatus.NewRow();
                        dr["RequisitionItemID"] = item.DocumentItemId != 0 ? Convert.ToInt64(item.DocumentItemId) : 0;
                        dr["P2PLineItemID"] = Convert.ToInt32(item.P2PLineItemId);
                        dr["RequisitionID"] = Convert.ToInt32(documentCode);
                        dr["RFXFlipType"] = Convert.ToString(item.RFXFlipType);
                        dtLineStatus.Rows.Add(dr);
                    }
                }
                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_UpdateRequisitionItemFlipType))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.Add(new SqlParameter("@tvp_P2P_RequisitionItemFlipType", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_P2P_REQUISITIONITEMFLIPTYPEUPDATE,
                        Value = dtLineStatus
                    });

                    result = Convert.ToBoolean(sqlHelper.ExecuteNonQuery(objSqlCommand), NumberFormatInfo.InvariantInfo);
                    if (!result)
                    {
                        LogHelper.LogInfo(Log, "Error occured in UpdateRequisitionItemFlipType query. documentCode : " + documentCode);
                    }
                    AddIntoSearchIndexerQueueing(documentCode, REQ, UserContext, GepConfiguration);
                }

                return result;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in UpdateRequisitionItemFlipType method. documentCode : " + documentCode, ex);
                throw ex;
            }
            finally
            {
                if (!ReferenceEquals(_sqlCon, null) && _sqlCon.State != ConnectionState.Closed)
                {
                    _sqlCon.Close();
                    _sqlCon.Dispose();
                }

                LogHelper.LogInfo(Log, "Requisition UpdateRequisitionItemFlipType Method Ended for DocumentCode = " + documentCode);
            }
        }

        public void ResetRequisitionItemFlipType(long requisitionId)
        {
            LogHelper.LogInfo(Log, "ResetRequisitionItemFlipType Method started");
            SqlConnection _sqlCon = null;
            try
            {
                LogHelper.LogInfo(Log, "Requisition ResetRequisitionItemFlipType Method Started for documentCode = " + requisitionId);
                ReliableSqlDatabase sqlHelper = ContextSqlConn;
                _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                _sqlCon.Open();

                if (Log.IsDebugEnabled)
                {
                    Log.Debug(string.Concat("Requisition ResetRequisitionItemFlipType sp USP_P2P_REQ_ResetRequisitionItemFlipType with parameter: " + requisitionId));
                }

                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_ResetRequisitionItemFlipType))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.Add(new SqlParameter() { ParameterName = "@RequisitionId", SqlDbType = SqlDbType.BigInt, Value = requisitionId });
                    objSqlCommand.CommandTimeout = 0;
                    bool result = Convert.ToBoolean(sqlHelper.ExecuteNonQuery(objSqlCommand), NumberFormatInfo.InvariantInfo);
                    if (!result)
                    {
                        LogHelper.LogInfo(Log, "Error occured in ResetRequisitionItemFlipType query. documentCode : " + requisitionId);
                    }
                }

            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in ResetRequisitionItemFlipType method. with parameter: requisitionId = " + requisitionId, ex);
                throw ex;
            }
            finally
            {
                if (!ReferenceEquals(_sqlCon, null) && _sqlCon.State != ConnectionState.Closed)
                {
                    _sqlCon.Close();
                    _sqlCon.Dispose();
                }
                Log.Info("ResetRequisitionItemFlipType Method ended");
            }

        }

        #region Interface Service --> TO BE COMMENTED    
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
        #endregion

        public List<NewP2PEntities.RequisitionItem> ValidateItemsOnBuChange(List<NewP2PEntities.RequisitionItem> objLstReqItems, string buList, long LOBId, string SourceType = "1")
        {
            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;
            DataSet UpdatedResult = new DataSet();
            long lineNumber = 0;
            long partnerCode = 0;
            bool isDeleted = false; int source = 0;
            try
            {
                LogHelper.LogInfo(Log, "ValidateItemsOnBuChange Method Started ");
                var sqlHelper = ContextSqlConn;
                _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();
                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("Order SaveItem SP: Usp_P2P_REQ_ValidateLineItemsOnBuChange with parameter: objReqItems=" + objLstReqItems.ToJSON(10, 50000000), " was called."));

                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_VALIDATELINEITEMSONBUCHANGE))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    SqlParameter objSqlParameter = new SqlParameter("@LineItems", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_P2P_PartnerItems,
                        Value = ConvertItemsToTableTypes(objLstReqItems)
                    };

                    objSqlCommand.Parameters.Add(objSqlParameter);
                    objSqlCommand.Parameters.AddWithValue("@BUId", buList);

                    objSqlCommand.Parameters.AddWithValue("@SourceType", SourceType);

                    UpdatedResult = sqlHelper.ExecuteDataSet(objSqlCommand);
                    UpdatedResult.Tables[0].TableName = "LineDetails";
                    //var sqlDr = (SqlDataReader)sqlHelper.ExecuteReader(objSqlCommand);
                }
                if (UpdatedResult.Tables[0] != null)
                {

                    if (UpdatedResult.Tables[0].Rows.Count > 0)
                    {
                        foreach (DataRow dr in UpdatedResult.Tables["LineDetails"].Rows)
                        {
                            lineNumber = Convert.ToInt64(dr["LineNumber"]);
                            partnerCode = Convert.ToInt64(dr["PartnerCode"]);
                            isDeleted = Convert.ToBoolean(dr["isDeleted"]);
                            source = Convert.ToInt16(dr["SourceType"]);
                            var items = objLstReqItems.Where(itm => itm.lineNumber == lineNumber);
                            if (items.Any())
                            {
                                if (items.FirstOrDefault().partner != null)
                                    items.FirstOrDefault().partner.id = partnerCode;

                                items.FirstOrDefault().isDeleted = isDeleted;
                            }

                        }
                    }
                }

                if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
                    _sqlTrans.Commit();
                return objLstReqItems;


            }
            catch (Exception ex)
            {
                LogHelper.LogInfo(Log, "Error Occurred in ValidateItemsOnBuChange Method.");
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
            return null;
        }

        private DataTable ConvertItemsToTableTypes(List<NewP2PEntities.RequisitionItem> lstReqItems)
        {


            DataTable dtLineItem = new DataTable() { Locale = CultureInfo.InvariantCulture, TableName = ReqSqlConstants.TVP_P2P_PartnerItems };
            dtLineItem.Columns.Add("LineNumber", typeof(long));
            dtLineItem.Columns.Add("PartnerCode", typeof(decimal));
            dtLineItem.Columns.Add("IsDeleted", typeof(bool));
            dtLineItem.Columns.Add("SourceType", typeof(string));

            if (lstReqItems != null)
            {
                foreach (var doc in lstReqItems)
                {


                    DataRow dr = dtLineItem.NewRow();
                    dr["LineNumber"] = doc.lineNumber;
                    dr["PartnerCode"] = doc.partner != null ? doc.partner.id : 0;
                    dr["IsDeleted"] = doc.isDeleted;
                    dr["SourceType"] = doc.source.id;
                    dtLineItem.Rows.Add(dr);
                }
            }
            return dtLineItem;
        }

        public async Task<List<Documents.Entities.DocumentStakeHolder>> GetDocumentStakeholderDetails(long DocumentCode)
        {
            RefCountingDataReader objRefCountingDataReader = null;
            List<Documents.Entities.DocumentStakeHolder> DocumentStakeHolderList = new List<Documents.Entities.DocumentStakeHolder>();


            LogHelper.LogInfo(Log, "GetDocumentStakeholderDetails Method Started for DocumentCode=" + DocumentCode);
            var sqlHelper = ContextSqlConn;
            objRefCountingDataReader = (RefCountingDataReader)sqlHelper.ExecuteReader(ReqSqlConstants.USP_DM_GETDOCUMENTSTAKEHOLDERDETAILS,
                new object[] { DocumentCode });
            if (objRefCountingDataReader != null)
            {
                var RefCountingDataReader = (SqlDataReader)objRefCountingDataReader.InnerReader;
                Documents.Entities.DocumentStakeHolder objDocumentStakeHolder = null;
                while (RefCountingDataReader.Read())
                {
                    objDocumentStakeHolder = new Documents.Entities.DocumentStakeHolder();
                    objDocumentStakeHolder.DocumentStakeholderId = GetLongValue(RefCountingDataReader, ReqSqlConstants.COL_DOCUMENT_STAKEHOLDER_ID);
                    objDocumentStakeHolder.PartnerCode = GetLongValue(RefCountingDataReader, ReqSqlConstants.COL_PARTNER_CODE);
                    objDocumentStakeHolder.ContactCode = GetLongValue(RefCountingDataReader, ReqSqlConstants.COL_CONTACT_CODE);
                    objDocumentStakeHolder.StakeholderTypeInfo = (GEP.Cumulus.Documents.Entities.StakeholderType)GetIntValue(RefCountingDataReader, ReqSqlConstants.COL_STAKEHOLDER_TYPE_INFO);
                    objDocumentStakeHolder.PartnerName = GetStringValue(RefCountingDataReader, ReqSqlConstants.COL_PARTNER_NAME);
                    objDocumentStakeHolder.ContactName = GetStringValue(RefCountingDataReader, ReqSqlConstants.COL_CONTACT_NAME);
                    objDocumentStakeHolder.StakeholderDocumentStatus = (GEP.Cumulus.Documents.Entities.DocumentStatus)GetIntValue(RefCountingDataReader, ReqSqlConstants.COL_STAKEHOLDER_DOCUMENT_STATUS);
                    objDocumentStakeHolder.ProxyContactCode = GetLongValue(RefCountingDataReader, ReqSqlConstants.COL_PROXY_CONTACT_CODE);
                    objDocumentStakeHolder.EmailId = GetStringValue(RefCountingDataReader, ReqSqlConstants.COL_EMAIL_ID);
                    DocumentStakeHolderList.Add(objDocumentStakeHolder);
                }
            }
            if (!ReferenceEquals(objRefCountingDataReader, null) && !objRefCountingDataReader.IsClosed)
            {
                objRefCountingDataReader.Close();
                objRefCountingDataReader.Dispose();
            }

            LogHelper.LogInfo(Log, "GetDocumentStakeholderDetails Method Ended for DocumentCode=" + DocumentCode);

            return await Task.FromResult<List<Documents.Entities.DocumentStakeHolder>>(DocumentStakeHolderList);
        }
        public Contact GetContactDetailsByContactCode(long contactCode)
        {
            RefCountingDataReader objRefCountingDataReader = null;
            Contact contactDetails = new Contact();
            try
            {
                objRefCountingDataReader =
                    (RefCountingDataReader)
                    ContextSqlConn.ExecuteReader(ReqSqlConstants.USP_P2P_GETCONTACTDETAILSBYCONTACTCODE, contactCode);
                if (objRefCountingDataReader != null)
                {
                    SqlDataReader sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;
                    while (sqlDr.Read())
                    {
                        contactDetails.ContactCode = GetLongValue(sqlDr, ReqSqlConstants.COL_CONTACT_CODE);
                        contactDetails.EmailAddress = GetStringValue(sqlDr, ReqSqlConstants.COL_EMAIL_ID);
                        contactDetails.FirstName = GetStringValue(sqlDr, ReqSqlConstants.COL_FIRSTNAME);
                        contactDetails.LastName = GetStringValue(sqlDr, ReqSqlConstants.COL_LASTNAME);
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
            }

            return contactDetails;
        }

        public bool SaveSRFQuestionsforReqItemId(List<long> QuestionId, long documentCode)
        {
            LogHelper.LogInfo(Log, "In SaveSRFQuestionsforReqItemId Method ");
            bool result = false;
            SqlConnection objSqlCon = null;
            SqlTransaction objSqlTrans = null;
            try
            {
                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("In SaveSRFQuestionsforReqItemId Method ",
                                                " with parameter: DocumentCode = ", documentCode));
                var sqlHelper = ContextSqlConn;
                objSqlCon = (SqlConnection)sqlHelper.CreateConnection();
                objSqlCon.Open();
                objSqlTrans = objSqlCon.BeginTransaction();

                DataTable dtQuestionId = new DataTable();
                dtQuestionId.Columns.Add("SRFQuestionId", typeof(long));
                if (QuestionId != null && QuestionId.Count > 0)
                {
                    foreach (long QId in QuestionId)
                    {
                        DataRow dr = dtQuestionId.NewRow();
                        dr["SRFQuestionId"] = QId;
                        dtQuestionId.Rows.Add(dr);

                    }
                }
                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_SAVESRFQUESTIONSFORREQITEMID))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.Add(new SqlParameter("@documentCode", SqlDbType.BigInt) { Value = @documentCode });
                    objSqlCommand.Parameters.Add(new SqlParameter("@QuestionIds", SqlDbType.Structured)

                    {
                        TypeName = ReqSqlConstants.TVP_LONG,
                        Value = dtQuestionId
                    });
                    result = Convert.ToBoolean(sqlHelper.ExecuteNonQuery(objSqlCommand, objSqlTrans));
                    objSqlTrans.Commit();
                }
                return result;
            }
            catch (Exception ex)
            {
                if (objSqlTrans != null)
                {
                    objSqlTrans.Rollback();
                }
                LogHelper.LogError(Log, "Error occured in SaveSRFQuestionsforReqItemId method. documentCode : " + documentCode, ex);
                throw ex;
            }
            finally
            {
                if (objSqlCon != null && objSqlCon.State != ConnectionState.Closed)
                {
                    objSqlCon.Close();
                }
                LogHelper.LogInfo(Log, "SaveSRFQuestionsforReqItemId Method Ended");
            }
        }

        public List<NewP2PEntities.Requisition> GetRequisitionDetailsList(DocumentSearch documentSearch)
        {
            List<NewP2PEntities.Requisition> requisitions = new List<NewP2PEntities.Requisition>();
            SqlConnection objSqlCon = null;
            try
            {
                if (documentSearch != null)
                {
                    DataSet requisitionDataSet = null;
                    DataTable dtOrgEntities = new DataTable();
                    dtOrgEntities.Columns.Add("Id", typeof(long));
                    if (documentSearch.orgEntities != null && documentSearch.orgEntities.Any())
                    {
                        foreach (long entity in documentSearch.orgEntities)
                        {
                            DataRow dr = dtOrgEntities.NewRow();
                            dr["Id"] = entity != 0 ? entity : 0;
                            dtOrgEntities.Rows.Add(dr);
                        }
                    }
                    else
                    {
                        DataRow dr = dtOrgEntities.NewRow();
                        dr["Id"] = 0;
                        dtOrgEntities.Rows.Add(dr);
                    }

                    var sqlHelper = ContextSqlConn;
                    objSqlCon = (SqlConnection)sqlHelper.CreateConnection();
                    using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_GETALLREQUISITIONSITEMSINFO))
                    {
                        objSqlCommand.CommandType = CommandType.StoredProcedure;
                        objSqlCommand.Parameters.Add(new SqlParameter("@SearchText", documentSearch.searchText));
                        objSqlCommand.Parameters.Add(new SqlParameter("@SearchColumn", documentSearch.SearchColumn));
                        objSqlCommand.Parameters.Add(new SqlParameter("@ContactCode", documentSearch.contactCode));
                        objSqlCommand.Parameters.Add(new SqlParameter("@SupplierCode", documentSearch.supplierCode));
                        objSqlCommand.Parameters.Add(new SqlParameter("@OrderingLocation", documentSearch.orderingLocation));
                        objSqlCommand.Parameters.Add(new SqlParameter("@PurchaseType", documentSearch.purchaseType));
                        objSqlCommand.Parameters.Add(new SqlParameter("@CurrecyCode", documentSearch.currencyCode));
                        objSqlCommand.Parameters.Add(new SqlParameter("@RequisitionStatuses", string.Join(",", documentSearch.documentStatus)));
                        objSqlCommand.Parameters.Add(new SqlParameter("@ContractNumber", documentSearch.contractNumber));
                        objSqlCommand.Parameters.Add(new SqlParameter("@LOBEntityDetailCode", documentSearch.LOBEntityDetailCode));
                        objSqlCommand.Parameters.Add(new SqlParameter("@PageNumber", documentSearch.startIndex));
                        objSqlCommand.Parameters.Add(new SqlParameter("@PageSize", documentSearch.pageSize));
                        objSqlCommand.Parameters.Add(new SqlParameter("@tvp_OrgEntities", SqlDbType.Structured)
                        {
                            TypeName = ReqSqlConstants.TVP_LONG,
                            Value = dtOrgEntities
                        });
                        objSqlCommand.CommandTimeout = 0;
                        requisitionDataSet = sqlHelper.ExecuteDataSet(objSqlCommand);
                    }

                    if (requisitionDataSet != null && requisitionDataSet.Tables.Count > 0)
                    {
                        if (requisitionDataSet.Tables[0].Rows != null && requisitionDataSet.Tables[0].Rows.Count > 0)
                        {
                            //Filling Requisition Header Details
                            FillRequisitionDetails(requisitionDataSet.Tables[0], ref requisitions);

                            //Fill Requisition Line Details:
                            FillRequisitionLineItems(requisitionDataSet.Tables[0], ref requisitions);
                        }

                        //Filling Taxes
                        if (requisitionDataSet.Tables[1].Rows != null && requisitionDataSet.Tables[1].Rows.Count > 0)
                        {
                            foreach (NewP2PEntities.Requisition objReq in requisitions)
                            {
                                var r = objReq;
                                FillTaxesInRequisitionItems(requisitionDataSet.Tables[1].Rows, ref r);
                            }
                        }

                        //Filling Splits
                        if (requisitionDataSet.Tables[2].Rows != null && requisitionDataSet.Tables[2].Rows.Count > 0)
                        {
                            List<ReqAccountingSplit> splits = GetAllRequisitionSplits(requisitionDataSet.Tables[2].Rows);
                            if (requisitionDataSet.Tables.Count > 2 && requisitionDataSet.Tables[3].Rows != null && requisitionDataSet.Tables[3].Rows.Count > 0)
                            {
                                FillEntitiesInRequisitionSplits(GetAllRequisitionSplitEntities(requisitionDataSet.Tables[3].Rows), null, ref splits);
                            }

                            foreach (NewP2PEntities.Requisition objReq in requisitions)
                            {
                                foreach (NewP2PEntities.RequisitionItem item in objReq.items)
                                {
                                    item.splits = (from s in splits where s.documentItemId == item.id select s).ToList();
                                    foreach (ReqAccountingSplit split in item.splits)
                                    {
                                        split.IsAddedFromRequistion = item.IsAddedFromRequistion;
                                    }
                                }
                            }
                        }

                        //filling AdditionaFields
                        if (requisitionDataSet.Tables.Count > 4 && requisitionDataSet.Tables[4].Rows != null && requisitionDataSet.Tables[4].Rows.Count > 0)
                        {
                            foreach (NewP2PEntities.Requisition objReq in requisitions)
                            {
                                var r = objReq;
                                FillAdditionalFieldsData(requisitionDataSet.Tables[4].Rows, ref r);
                            }

                        }

                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in GetRequisitionDetailsList method in NewRequisitionDAO. Document Status :" + string.Join(",", documentSearch.documentStatus)
                    + " Search Column = " + documentSearch.SearchColumn.ToString() + " LOBEntityDetailCode = " + documentSearch.LOBEntityDetailCode.ToString() +
                    " Currency Code = " + documentSearch.currencyCode.ToString() + " Search Text = " + documentSearch.searchText.ToString(), ex);
                throw ex;
            }
            finally
            {
                if (objSqlCon != null && objSqlCon.State != ConnectionState.Closed)
                {
                    objSqlCon.Close();
                }
                LogHelper.LogInfo(Log, "GetRequisitionDetailsList Method Ended");
            }

            return requisitions.GroupBy(dCode => dCode.documentCode).Select(c => c.First()).ToList();
        }

        private void FillRequisitionDetails(DataTable reqTable, ref List<NewP2PEntities.Requisition> requisitions)
        {
            try
            {
                foreach (DataRow dr in reqTable.Rows)
                {
                    NewP2PEntities.Requisition req = new NewP2PEntities.Requisition();

                    req.name = ConvertToString(dr, ReqSqlConstants.COL_REQUISITION_NAME);
                    req.number = ConvertToString(dr, ReqSqlConstants.COL_REQUISITION_NUMBER);
                    req.documentCode = ConvertToInt64(dr, ReqSqlConstants.COL_DOCUMENTCODE);
                    req.obo = new IdAndName
                    {
                        id = ConvertToInt64(dr, ReqSqlConstants.COL_ONBEHALFOF),
                        name = BuildName(ConvertToString(dr, ReqSqlConstants.COL_OBO_FNAME), ConvertToString(dr, ReqSqlConstants.COL_OBO_LNAME))
                    };
                    req.createdBy = new IdAndName
                    {
                        id = ConvertToInt64(dr, ReqSqlConstants.COL_REQUESTERID),
                        name = BuildName(ConvertToString(dr, ReqSqlConstants.COL_CREATEDBYFNAME), ConvertToString(dr, ReqSqlConstants.COL_CREATEDBYLNAME))
                    };
                    req.purchaseType = ConvertToByte(dr, ReqSqlConstants.COL_PURCHASETYPE);
                    req.TotalRequisitions = ConvertToInt32(dr, ReqSqlConstants.COL_TOTALCOUNT);

                    requisitions.Add(req);
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in FillRequisitionDetails method in NewRequisitionDAO.", ex);
                throw ex;
            }
        }

        private void FillRequisitionLineItems(DataTable reqTable, ref List<NewP2PEntities.Requisition> requisitions)
        {
            try
            {
                List<NewP2PEntities.RequisitionItem> objRequisitionItem = GetRequisitionItems(reqTable.Rows);

                foreach (NewP2PEntities.Requisition req in requisitions)
                {
                    req.items = new List<NewP2PEntities.RequisitionItem>();
                    objRequisitionItem.ForEach(x => { if (x.documentCode == req.documentCode) { req.items.Add(x); } });
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in FillRequisitionLineItems method in NewRequisitionDAO.", ex);
                throw ex;
            }
        }
        public void CalculateAndUpdateRiskScore(long documentCode, bool isRiskFormMandatory)
        {
            SqlConnection objSqlCon = null;
            SqlTransaction objSqlTrans = null;
            try
            {

                ReliableSqlDatabase sqlHelper = null;

                sqlHelper = ContextSqlConn;
                objSqlCon = (SqlConnection)sqlHelper.CreateConnection();
                objSqlCon.Open();
                objSqlTrans = objSqlCon.BeginTransaction();

                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_CALCULATEANDUPDATERISKSCORE))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.Add(new SqlParameter() { ParameterName = "@DocumentCode", SqlDbType = SqlDbType.BigInt, Value = documentCode });
                    objSqlCommand.Parameters.Add(new SqlParameter() { ParameterName = "@isRiskFormMandatory", SqlDbType = SqlDbType.Bit, Value = isRiskFormMandatory });
                    objSqlCommand.CommandTimeout = 0;
                    sqlHelper.ExecuteDataSet(objSqlCommand);
                    if (objSqlTrans != null)
                        objSqlTrans.Commit();
                }

            }
            catch (Exception ex)
            {
                if (objSqlTrans != null)
                {
                    objSqlTrans.Rollback();
                }
                LogHelper.LogError(Log, "Error occured in CalculateAndUpdateRiskScore method. DocumentCode : " + documentCode, ex);
                CustomFault objCustomFault = new CustomFault("Error while Saving Questions Responses " + ex.Message, "CalculateAndUpdateRiskScore", "CalculateAndUpdateRiskScore", "CalculateAndUpdateRiskScore", ExceptionType.ApplicationException, string.Empty, false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while Calculate And Update Risk Score" + ex.Message);
            }
            finally
            {
                if (objSqlCon != null && objSqlCon.State != ConnectionState.Closed)
                {
                    objSqlCon.Close();
                }
                Log.Info("CalculateAndUpdateRiskScore Method ended");
            }
        }

        public DataSet ValidateRequisitionData(long documentCode, DataTable dataTable)
        {

            SqlConnection objSqlCon = null;
            DataSet dsInactiveOrgEntities = null;
            //DataTable dtInactiveOrgEntities = null;
            try
            {

                ReliableSqlDatabase sqlHelper = null;

                sqlHelper = ContextSqlConn;
                objSqlCon = (SqlConnection)sqlHelper.CreateConnection();
                objSqlCon.Open();

                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_ValidateRequisitionData))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.Add(new SqlParameter() { ParameterName = "@RequisitionID", SqlDbType = SqlDbType.BigInt, Value = documentCode });
                    SqlParameter stgValues = new SqlParameter("@ValidatebasicSettingValues", SqlDbType.Structured);
                    stgValues.TypeName = ReqSqlConstants.TVP_Basic_Setting;
                    stgValues.Value = dataTable;
                    objSqlCommand.CommandTimeout = 0;
                    objSqlCommand.Parameters.Add(stgValues);
                    dsInactiveOrgEntities = sqlHelper.ExecuteDataSet(objSqlCommand);
                }
                //if (dsInactiveOrgEntities != null && dsInactiveOrgEntities.Tables.Count > 0)
                //    dtInactiveOrgEntities = dsInactiveOrgEntities.Tables[0];
            }
            finally
            {
                if (objSqlCon != null && objSqlCon.State != ConnectionState.Closed)
                {
                    objSqlCon.Close();
                    objSqlCon.Dispose();
                }
                //if( dtInactiveOrgEntities!=null ){
                //    dtInactiveOrgEntities.Dispose();
                //}
            }
            return dsInactiveOrgEntities;
        }


        public DataTable GetAllPASCategories(string searchText, int pageSize = 10, int pageNumber = 1, long partnerCode = 0, long contactCode = 0, int categorySelectionLevel = 0)
        {

            SqlConnection objSqlCon = null;
            DataTable dtCategoryList = null;

            try
            {
                ReliableSqlDatabase sqlHelper = null;

                sqlHelper = ContextSqlConn;
                objSqlCon = (SqlConnection)sqlHelper.CreateConnection();
                objSqlCon.Open();

                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_GETALLPASCATEGORIES))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.Add(new SqlParameter() { ParameterName = "@SearchText", SqlDbType = SqlDbType.NVarChar, Size = 200, Value = searchText });
                    objSqlCommand.Parameters.Add(new SqlParameter() { ParameterName = "@PageSize", SqlDbType = SqlDbType.Int, Value = pageSize });
                    objSqlCommand.Parameters.Add(new SqlParameter() { ParameterName = "@PageNumber", SqlDbType = SqlDbType.Int, Value = pageNumber });
                    objSqlCommand.Parameters.Add(new SqlParameter() { ParameterName = "@PartnerCode", SqlDbType = SqlDbType.BigInt, Value = partnerCode });
                    objSqlCommand.Parameters.Add(new SqlParameter() { ParameterName = "@ContactCode", SqlDbType = SqlDbType.BigInt, Value = contactCode });
                    objSqlCommand.Parameters.Add(new SqlParameter() { ParameterName = "@CategorySelectionLevel", SqlDbType = SqlDbType.BigInt, Value = categorySelectionLevel });
                    objSqlCommand.CommandTimeout = 0;
                    dtCategoryList = sqlHelper.ExecuteDataSet(objSqlCommand).Tables[0];
                }
                return dtCategoryList;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in GetAllPASCategories method in NewRequisitionDAO.", ex);
                throw ex;
            }
            finally
            {
                if (objSqlCon != null && objSqlCon.State != ConnectionState.Closed)
                {
                    objSqlCon.Close();
                    objSqlCon.Dispose();
                }
                if (dtCategoryList != null)
                {
                    dtCategoryList.Dispose();
                }
            }
        }

        public NewP2PEntities.Requisition GetRequisitionDetailsFromCatalog(Int64 documentCode)
        {
            DataSet requisitionDataSet = null;
            SqlConnection objSqlCon = null;
            NewP2PEntities.Requisition objReqData = new NewP2PEntities.Requisition();
            objReqData.obo = new IdAndName();
            try
            {
                ReliableSqlDatabase sqlHelper = null;
                sqlHelper = ContextSqlConn;
                objSqlCon = (SqlConnection)sqlHelper.CreateConnection();
                objSqlCon.Open();

                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_GetRequisitionDetailsFromCatalog))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.Add(new SqlParameter() { ParameterName = "@RequisitionID", SqlDbType = SqlDbType.BigInt, Value = documentCode });
                    objSqlCommand.CommandTimeout = 0;

                    requisitionDataSet = sqlHelper.ExecuteDataSet(objSqlCommand);

                    if (requisitionDataSet != null && requisitionDataSet.Tables.Count > 0)
                    {
                        if (requisitionDataSet.Tables[0].Rows != null && requisitionDataSet.Tables[0].Rows.Count > 0)
                        {
                            objReqData.number = ConvertToString(requisitionDataSet.Tables[0].Rows[0], ReqSqlConstants.COL_REQUISITION_NUMBER);
                            objReqData.ProcurementProfileId = ConvertToInt64(requisitionDataSet.Tables[0].Rows[0], ReqSqlConstants.COL_ProcurementProfileId);
                            objReqData.obo.id = ConvertToInt64(requisitionDataSet.Tables[0].Rows[0], ReqSqlConstants.COL_ONBEHALFOF);
                            objReqData.CurrencyCode = ConvertToString(requisitionDataSet.Tables[0].Rows[0], ReqSqlConstants.COL_CURRENCY);
                            objReqData.RequesterID = ConvertToInt64(requisitionDataSet.Tables[0].Rows[0], ReqSqlConstants.COL_REQUESTERID);
                            objReqData.BUId = ConvertToInt64(requisitionDataSet.Tables[0].Rows[0], ReqSqlConstants.COL_LOBId);
                        }
                        if (requisitionDataSet.Tables.Count > 1 && requisitionDataSet.Tables[1].Rows != null && requisitionDataSet.Tables[1].Rows.Count > 0)
                        {
                            objReqData.HeaderSplitAccountingFields = new List<SplitAccountingFields>();
                            foreach (DataRow row in requisitionDataSet.Tables[1].Rows)
                            {
                                SplitAccountingFields splitAccountingFields = new SplitAccountingFields();
                                splitAccountingFields.EntityTypeId = ConvertToInt16(row, ReqSqlConstants.COL_ENTITYID);
                                splitAccountingFields.EntityDetailCode = ConvertToInt64(row, ReqSqlConstants.COL_ENTITYDETAILCODE);
                                objReqData.HeaderSplitAccountingFields.Add(splitAccountingFields);
                            }
                        }
                    }
                }
                return objReqData;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in GetRequisitionDetailsFromCatalog method in NewRequisitionDAO.", ex);
                throw ex;
            }
            finally
            {
                if (objSqlCon != null && objSqlCon.State != ConnectionState.Closed)
                {
                    objSqlCon.Close();
                    objSqlCon.Dispose();
                }

            }
        }

        public int GetGroupIdOfVABUser(long contactCode)
        {
            RefCountingDataReader objRefCountingDataReader = null;
            int GroupId = 0;
            try
            {
                LogHelper.LogInfo(Log, "Requisition GetGroupIdOfVABUser Method Started for contactCode=" + contactCode);

                if (Log.IsDebugEnabled)
                    Log.Debug(string.Format(CultureInfo.InvariantCulture, "Requisition GetGroupIdOfVABUser sp usp_P2P_REQ_GetGroupIdOfVABUser with parameter: contactCode={0} was called.", contactCode));

                objRefCountingDataReader = (RefCountingDataReader)
                ContextSqlConn.ExecuteReader(ReqSqlConstants.USP_P2P_REQ_GETGROUPIDOFVABUSER, contactCode);
                if (objRefCountingDataReader != null)
                {
                    var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;
                    if (sqlDr.Read())
                    {
                        GroupId = GetIntValue(sqlDr, ReqSqlConstants.COL_GROUPID);
                    }
                    return GroupId;
                }
            }
            finally
            {
                if (!ReferenceEquals(objRefCountingDataReader, null) && !objRefCountingDataReader.IsClosed)
                {
                    objRefCountingDataReader.Close();
                    objRefCountingDataReader.Dispose();
                }

                LogHelper.LogInfo(Log, "Requisition GetGroupIdOfVABUser Method Ended for contactCode=" + contactCode);
            }

            return GroupId;
        }

        public DataSet GetPartnersDocumetInterfaceInfo(List<long> partnerCodes)
        {

            SqlConnection objSqlCon = null;
            DataSet dtPartnersDetailList = null;

            DataTable dtpartnerCodes = new DataTable();
            dtpartnerCodes.Columns.Add("PartnerCode", typeof(long));
            if (partnerCodes != null && partnerCodes.Any())
            {
                foreach (long PartnerCode in partnerCodes)
                {
                    DataRow dr = dtpartnerCodes.NewRow();
                    dr["PartnerCode"] = PartnerCode;
                    dtpartnerCodes.Rows.Add(dr);
                }
            }


            try
            {
                ReliableSqlDatabase sqlHelper = null;

                sqlHelper = ContextSqlConn;
                objSqlCon = (SqlConnection)sqlHelper.CreateConnection();
                objSqlCon.Open();

                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_REQ_GETPARTNERSDOCUMETINTERFACEINFO))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.Add(new SqlParameter("@tvpPRNPartnerCodes", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_PRN_PARTNERCODE,
                        Value = dtpartnerCodes
                    });
                    objSqlCommand.CommandTimeout = 0;
                    dtPartnersDetailList = sqlHelper.ExecuteDataSet(objSqlCommand);
                }
                return dtPartnersDetailList;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in GetPartnersDocumetInterfaceInfo method in NewRequisitionDAO.", ex);
                throw ex;
            }
            finally
            {
                if (objSqlCon != null && objSqlCon.State != ConnectionState.Closed)
                {
                    objSqlCon.Close();
                    objSqlCon.Dispose();
                }
                if (dtPartnersDetailList != null)
                {
                    dtPartnersDetailList.Dispose();
                }
            }
        }

        public List<UserDetails> GetUsersBasedOnUserDetailsWithPagination(UserDetails usersInfo, string searchText, int pageIndex, int pageSize, bool includeCurrentUser, string activityCodes, bool honorDirectRequesterForOBOSelection, bool isAutosuggest, bool isCheckCreateReqActivityForOBO)
        {

            SqlConnection objSqlCon = null;
            DataSet ds = new DataSet();
            List<UserDetails> usersDetails = new List<UserDetails>();
            try
            {
                ReliableSqlDatabase sqlHelper = ContextSqlConn;
                objSqlCon = (SqlConnection)sqlHelper.CreateConnection();
                objSqlCon.Open();
                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_PRN_GETUSERSBASEDONUSERDETAILSFORPAGINATION, objSqlCon))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    var value = ConvertToContactDetailsDataSet(usersInfo);
                    var restrictedActivities = ConvertToActivitiesDetailsDataSet(usersInfo);
                    if (value.Rows.Count > 0)
                        objSqlCommand.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = "@tvp_ORG_SelectedBUs",
                            SqlDbType = SqlDbType.Structured,
                            TypeName = "tvp_ORG_SelectedBUs",
                            Value = value
                        });
                    if (restrictedActivities.Rows.Count > 0)
                        objSqlCommand.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = "@tvp_RestrictedUsers_ActivityCodes",
                            SqlDbType = SqlDbType.Structured,
                            TypeName = "tvp_RestrictedUsers_ActivityCodes",
                            Value = restrictedActivities
                        });
                    objSqlCommand.Parameters.Add(new SqlParameter()
                    {
                        ParameterName = "@searchText",
                        SqlDbType = SqlDbType.NVarChar,
                        Value = searchText
                    });
                    objSqlCommand.Parameters.Add(new SqlParameter()
                    {
                        ParameterName = "@PageIndex",
                        SqlDbType = SqlDbType.Int,
                        Value = pageIndex
                    });
                    objSqlCommand.Parameters.Add(new SqlParameter()
                    {
                        ParameterName = "@PageSize",
                        SqlDbType = SqlDbType.Int,
                        Value = pageSize
                    });
                    objSqlCommand.Parameters.Add(new SqlParameter()
                    {
                        ParameterName = "@ContactCode",
                        SqlDbType = SqlDbType.BigInt,
                        Value = usersInfo.ContactCode
                    });
                    objSqlCommand.Parameters.Add(new SqlParameter()
                    {
                        ParameterName = "@includeCurrentUser",
                        SqlDbType = SqlDbType.Bit,
                        Value = includeCurrentUser
                    });
                    objSqlCommand.Parameters.Add(new SqlParameter()
                    {
                        ParameterName = "@activityCodes",
                        SqlDbType = SqlDbType.NVarChar,
                        Value = activityCodes
                    });
                    objSqlCommand.Parameters.Add(new SqlParameter()
                    {
                        ParameterName = "@honorDirectRequesterForOBOSelection",
                        SqlDbType = SqlDbType.Bit,
                        Value = honorDirectRequesterForOBOSelection
                    });
                    objSqlCommand.Parameters.Add(new SqlParameter()
                    {
                        ParameterName = "@isAutosuggest",
                        SqlDbType = SqlDbType.Bit,
                        Value = isAutosuggest
                    });
                    objSqlCommand.Parameters.Add(new SqlParameter()
                    {
                        ParameterName = "@isCheckCreateReqActivityForOBO",
                        SqlDbType = SqlDbType.Bit,
                        Value = isCheckCreateReqActivityForOBO
                    });

                    ds = sqlHelper.ExecuteDataSet(objSqlCommand);
                    if (!ReferenceEquals(ds, null) && ds.Tables.Count > 0)
                        foreach (DataRow row in ds.Tables[0].Rows)
                        {
                            // If none of the user Entries are present in the User List Or if there is a new User in list
                            UserDetails userDetails = new UserDetails();
                            userDetails.UserLOBDetails = new List<UserLOBDetails>();
                            userDetails.ContactCode = Convert.ToInt64(row[ReqSqlConstants.COL_CONTACTCODE]);
                            userDetails.FirstName = Convert.ToString(row[ReqSqlConstants.COL_FIRSTNAME]);
                            userDetails.LastName = Convert.ToString(row[ReqSqlConstants.COL_LASTNAME]);
                            userDetails.EmailAddress = Convert.ToString(row[ReqSqlConstants.COL_EMAILADDRESS]);
                            userDetails.TotalRecords = Convert.ToInt32(row[ReqSqlConstants.COL_TOTAL_RECORDS]);
                            //Mapping LOB details for user
                            UserLOBDetails userLOBDetails = new UserLOBDetails();
                            userLOBDetails.BUDetails = new List<ContactORGMapping>();
                            userLOBDetails.ContactCode = Convert.ToInt64(row[ReqSqlConstants.COL_CONTACTCODE]);
                            userLOBDetails.LOBId = Convert.ToInt64(row[ReqSqlConstants.COL_LOBEntityDetailCode]);
                            userLOBDetails.PreferenceLOBType = Convert.ToInt32(row[ReqSqlConstants.COL_PreferenceLOBType]);
                            //Mapping BU details for user
                            ContactORGMapping userBUDetails = new ContactORGMapping();
                            userBUDetails.OrgEntityCode = Convert.ToInt64(row[ReqSqlConstants.COL_BUENTITYDETAILCODE]);
                            //userBUDetails.IsDefault = Convert.ToBoolean(row[ReqSqlConstants.COL_BUISDEFAULT]);
                            userBUDetails.EntityDescription = Convert.ToString(row[ReqSqlConstants.COL_BUENTITYDESCRIPTION]);
                            userBUDetails.EntityCode = Convert.ToString(row[ReqSqlConstants.COL_BUENTITYCODE]);
                            userBUDetails.EntityDisplayName = Convert.ToString(row[ReqSqlConstants.COL_BU_ENTITY_DISPLAY_NAME]);

                            userLOBDetails.BUDetails.Add(userBUDetails);
                            userDetails.UserLOBDetails.Add(userLOBDetails);
                            usersDetails.Add(userDetails);

                        }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in GetUsersBasedOnUserDetailsWithPagination method of NewRequisitionDAO", ex);
                throw ex;
            }
            finally
            {
                if (objSqlCon != null && objSqlCon.State != ConnectionState.Closed)
                {
                    objSqlCon.Close();
                }
                LogHelper.LogInfo(Log, "GetUsersBasedOnUserDetailsWithPagination method ended");
            }

            return usersDetails;
        }
        private DataTable ConvertToContactDetailsDataSet(UserDetails usersInfo)
        {
            DataTable dtUserDetailsMapping = new DataTable();
            dtUserDetailsMapping.Columns.Add("EntityDetailCode", typeof(long));
            dtUserDetailsMapping.Columns.Add("EntityId", typeof(int));
            dtUserDetailsMapping.Columns.Add("EntityDisplayName", typeof(string));
            dtUserDetailsMapping.Columns.Add("EntityCode", typeof(string));
            dtUserDetailsMapping.Columns.Add("EntityDescription", typeof(string));
            dtUserDetailsMapping.Columns.Add("IsActive", typeof(bool));
            dtUserDetailsMapping.Columns.Add("LOBEntityDetailCode", typeof(long));
            dtUserDetailsMapping.Columns.Add("PreferenceLobType", typeof(int));

            if (usersInfo.UserLOBDetails != null)
                foreach (UserLOBDetails userLOBDetails in usersInfo.UserLOBDetails)
                {
                    foreach (ContactORGMapping buDetails in userLOBDetails.BUDetails)
                    {
                        dtUserDetailsMapping.Rows.Add(buDetails.OrgEntityCode, 0, "", "", "", 1, userLOBDetails.OrgEntityCode, userLOBDetails.PreferenceLOBType);
                    }
                }
            return dtUserDetailsMapping;
        }
        public DataTable ConvertToActivitiesDetailsDataSet(UserDetails usersInfo)
        {
            DataTable dtActivities = new DataTable();
            dtActivities.Columns.Add("ActivityCode", typeof(long));
            if (usersInfo.RestrictedUserActivities != null)
            {
                foreach (long ActivityCode in usersInfo.RestrictedUserActivities)
                {
                    dtActivities.Rows.Add(ActivityCode);
                }
            }
            return dtActivities;
        }

        public bool AddRequisitionsIntoSearchIndexerQueue(List<long> reqIds)
        {
            return AddIntoSearchIndexerQueueing(reqIds, (int)DocumentType.Requisition, UserContext, GepConfiguration, true);
        }
        public Documents.Entities.DocumentLOBDetails GetDocumentLOBByDocumentCode(long documentCode)
        {
            DocumentLOBDetails documentLOB = new DocumentLOBDetails();
            SqlConnection sqlCon = null;
            RefCountingDataReader objRefCountingDataReader = null;
            var sqlHelper = ContextSqlConn;
            try
            {
                LogHelper.LogInfo(Log, String.Format("GetDocumentLOBByDocumentCode Method Started for documentCode={0}", documentCode));

                sqlCon = (SqlConnection)ContextSqlConn.CreateConnection();
                sqlCon.Open();
                objRefCountingDataReader = (RefCountingDataReader)sqlHelper.ExecuteReader(ReqSqlConstants.USP_P2P_REQ_GETDOCUMENTLOBBYDOCUMENTCODE,
                    new object[] { documentCode });

                if (objRefCountingDataReader != null)
                {
                    try
                    {
                        var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;
                        while (sqlDr.Read())
                        {
                            documentLOB.EntityId = GetIntValue(sqlDr, ReqSqlConstants.COL_EntityId);
                            documentLOB.EntityDetailCode = GetLongValue(sqlDr, ReqSqlConstants.COL_ENTITYDETAILCODE);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.LogError(Log, "Error occured in GetDocumentLOBByDocumentCode method.", ex);

                        var objCustomFault = new CustomFault(ex.Message, "GetDocumentLOBByDocumentCode", "GetDocumentLOBByDocumentCode",
                                                             "Common", ExceptionType.ApplicationException,
                                                             "", false);
                        throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                              "Error while GetDocumentLOBByDocumentCode " + ex.Message + " Stack Trace: " + ex.StackTrace + "Inner Exception: " + ex.InnerException);
                    }
                }
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
            }
            return documentLOB;
        }

        public void UpdateRequisitionPreviousAmount(long requisitionId, bool updateReqPrevAmount)
        {
            LogHelper.LogInfo(Log, "UpdateRequisitionPreviousAmount Method started");
            SqlConnection objSqlCon = null;
            SqlTransaction objSqlTrans = null;
            try
            {


                Log.Debug(string.Concat("In UpdateRequisitionPreviousAmount Method with parameter: requisitionId = ", requisitionId));
                var sqlHelper = ContextSqlConn;
                objSqlCon = (SqlConnection)sqlHelper.CreateConnection();

                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_UPDATEREQUSITIONPREVIOUSAMOUNT))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.Add(new SqlParameter() { ParameterName = "@RequisitionId", SqlDbType = SqlDbType.BigInt, Value = requisitionId });
                    objSqlCommand.Parameters.Add(new SqlParameter() { ParameterName = "@updateReqPrevAmount", SqlDbType = SqlDbType.Bit, Value = updateReqPrevAmount });
                    objSqlCommand.CommandTimeout = 0;
                    objSqlCon.Open();
                    objSqlTrans = objSqlCon.BeginTransaction();
                    var Result = sqlHelper.ExecuteNonQuery(objSqlCommand, objSqlTrans);
                    objSqlTrans.Commit();
                }

            }
            catch (Exception ex)
            {
                if (objSqlTrans != null)
                {
                    objSqlTrans.Rollback();
                }
                LogHelper.LogError(Log, "Error occured in UpdateRequisitionPreviousAmount method. with parameter: requisitionId = " + requisitionId, ex);
                CustomFault objCustomFault = new CustomFault("Error while UpdateRequisitionPreviousAmount" + ex.Message, "UpdateRequisitionPreviousAmount", "UpdateRequisitionPreviousAmount", "UpdateRequisitionPreviousAmount",

                 ExceptionType.ApplicationException, string.Empty, false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while UpdateRequisitionPreviousAmount" + ex.Message);
            }
            finally
            {
                if (objSqlCon != null && objSqlCon.State != ConnectionState.Closed)
                {
                    objSqlCon.Close();
                }
                Log.Info("UpdateRequisitionPreviousAmount Method ended");
            }

        }

        public List<PartnerLocation> GetOrderingLocationsWithNonOrgEntities(long Partnercode, int LocationTypeId, List<long> OrgBUIds, List<long> NonOrgBUIds, string TempRemittoLocationKey = "", int PageIndex = 0, int PageSize = 10, string SearchText = "")
        {

            SqlConnection sqlConnection = null;
            ReliableDatabase sqlHelper = ContextSqlConn;
            //sqlConnection = (SqlConnection)sqlHelper.CreateConnection();
            RefCountingDataReader refCountingDataReader = null;
            SqlDataReader sqlDataReader = null;
            List<PartnerLocation> PartnerLocationList = new List<PartnerLocation>();

            try
            {
                LogHelper.LogInfo(Log, "GetOrderingLocationsWithNonOrgEntities method in SQLRequisition started for partner code " + Partnercode + " .");
                sqlConnection = (SqlConnection)sqlHelper.CreateConnection();
                sqlConnection.Open();

                using (SqlCommand sqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_GetOrderingLocationsWithNonOrgEntities, sqlConnection))
                {
                    sqlCommand.CommandType = CommandType.StoredProcedure;

                    sqlCommand.Parameters.AddWithValue("@PartnerCode", Partnercode);
                    sqlCommand.Parameters.AddWithValue("@LocationTypeId", LocationTypeId);
                    sqlCommand.Parameters.AddWithValue("@TempRemittoLocationKey", TempRemittoLocationKey);
                    sqlCommand.Parameters.AddWithValue("@pageIndex", PageIndex);
                    sqlCommand.Parameters.AddWithValue("@pageSize", PageSize);
                    sqlCommand.Parameters.AddWithValue("@searchText", SearchText);
                    SqlParameter OrgIds = new SqlParameter("@AccessControlOrgEntityDetailCodes", SqlDbType.Structured);
                    OrgIds.TypeName = ReqSqlConstants.TVP_ORG_ENTITYCODES;
                    OrgIds.Value = ConvertEntityDetailCodesToDatatable(OrgBUIds);
                    SqlParameter NonOrgIds = new SqlParameter("@NonOrgIdEntityDetailCodes", SqlDbType.Structured);
                    NonOrgIds.TypeName = ReqSqlConstants.TVP_ORG_ENTITYCODES;
                    NonOrgIds.Value = ConvertEntityDetailCodesToDatatable(NonOrgBUIds);

                    sqlCommand.Parameters.Add(OrgIds);
                    sqlCommand.Parameters.Add(NonOrgIds);

                    refCountingDataReader = (RefCountingDataReader)sqlHelper.ExecuteReader(sqlCommand);
                    sqlDataReader = (SqlDataReader)refCountingDataReader.InnerReader;

                    while (sqlDataReader.Read())
                    {
                        PartnerLocation objPartnerLocation = new PartnerLocation();

                        objPartnerLocation.PartnerCode = GetLongValue(sqlDataReader, "PartnerCode");
                        objPartnerLocation.LocationId = GetLongValue(sqlDataReader, "LocationId");
                        objPartnerLocation.LocationName = GetStringValue(sqlDataReader, "LocationName");
                        objPartnerLocation.AlternateLocationName = GetStringValue(sqlDataReader, "AlternateLocationName");
                        objPartnerLocation.Address = new Address();
                        objPartnerLocation.Address.AddressCode = GetLongValue(sqlDataReader, "AddressCode");
                        objPartnerLocation.Address.Addressline1 = GetStringValue(sqlDataReader, "Addressline1");
                        objPartnerLocation.Address.Addressline2 = GetStringValue(sqlDataReader, "Addressline2");
                        objPartnerLocation.Address.Addressline3 = GetStringValue(sqlDataReader, "Addressline3");
                        objPartnerLocation.Address.CountryInfo.CountryId = GetIntValue(sqlDataReader, "CountryId");
                        objPartnerLocation.Address.CountryInfo.CountryName = GetStringValue(sqlDataReader, "CountryName");
                        objPartnerLocation.Address.StateInfo.StateCode = GetLongValue(sqlDataReader, "StateCode");
                        objPartnerLocation.Address.StateInfo.StateName = GetStringValue(sqlDataReader, "StateName");
                        objPartnerLocation.Address.StateInfo.CountryId = GetIntValue(sqlDataReader, "StateCountryId");
                        objPartnerLocation.Address.City = GetStringValue(sqlDataReader, "City");
                        objPartnerLocation.Address.ZipCode = GetStringValue(sqlDataReader, "ZipCode");
                        objPartnerLocation.Address.PhoneNo1 = GetStringValue(sqlDataReader, "PhoneNo1");
                        objPartnerLocation.Address.ExtenstionNo1 = GetStringValue(sqlDataReader, "ExtensionNo1");
                        objPartnerLocation.Address.PhoneNo2 = GetStringValue(sqlDataReader, "PhoneNo2");
                        objPartnerLocation.Address.ExtenstionNo2 = GetStringValue(sqlDataReader, "ExtensionNo2");
                        objPartnerLocation.Address.FaxNo = GetStringValue(sqlDataReader, "FaxNo");
                        objPartnerLocation.ClientLocationCode = GetStringValue(sqlDataReader, "ClientLocationCode");
                        objPartnerLocation.Address.StateInfo.StateAbbrevationCode = GetStringValue(sqlDataReader, "StateAbbrevationCode");
                        objPartnerLocation.Address.CountryInfo.CountryCode = GetStringValue(sqlDataReader, "CountryAbbrevationCode");
                        objPartnerLocation.Address.County = GetStringValue(sqlDataReader, "County");
                        objPartnerLocation.IsDefault = GetBoolValue(sqlDataReader, "IsDefault");
                        objPartnerLocation.TotalRecords = GetIntValue(sqlDataReader, "TotalCount");
                        PartnerLocationList.Add(objPartnerLocation);

                    }


                }


            }

            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occurred in GetOrderingLocationsWithNonOrgEntities method in SQLRequisition", ex);
            }

            finally
            {
                if (!ReferenceEquals(sqlDataReader, null) && !sqlDataReader.IsClosed)
                {
                    sqlDataReader.Close();
                    sqlDataReader.Dispose();
                }
                if (!ReferenceEquals(refCountingDataReader, null) && !refCountingDataReader.IsClosed)
                {
                    refCountingDataReader.Close();
                    refCountingDataReader.Dispose();
                }

                if (!ReferenceEquals(sqlConnection, null) && sqlConnection.State != ConnectionState.Closed)
                {
                    sqlConnection.Close();
                    sqlConnection.Dispose();
                }
                LogHelper.LogInfo(Log, "GetOrderingLocationsWithNonOrgEntities method in SQLOrderCommon Ended ");

            }
            return PartnerLocationList;

        }

        private DataTable ConvertEntityDetailCodesToDatatable(List<long> orgids)
        {
            DataTable dataTable = new DataTable() { Locale = CultureInfo.InvariantCulture, TableName = ReqSqlConstants.TVP_ORG_ENTITYCODES };
            dataTable.Columns.Add("EntityDetailCode", typeof(long));

            foreach (long id in orgids)
            {

                DataRow dr = dataTable.NewRow();
                dr["EntityDetailCode"] = id;
                dataTable.Rows.Add(dr);
            }

            return dataTable;

        }

        /////REQ-5620 Team Member functionality for getting all users
        public List<UserDetails> GetAllUsersByActivityCode(string SearchText, string Shouldhaveactivitycodes, string Shouldnothaveactivitycodes, long Partnercode)
        {
            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;
            DataTable UpdatedResult = new DataTable();
            List<UserDetails> objLstReqItems = new List<UserDetails>();

            try
            {
                LogHelper.LogInfo(Log, "GetAllUsersByActivityCode Method Started ");
                var sqlHelper = ContextSqlConn;
                _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();
                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("Order SaveItem SP: USP_REQ_GETTEAMUSERS with parameter:  was called."));

                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_REQ_GETALLUSERSBYACTIVITYCODE))
                {

                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.Add(new SqlParameter() { ParameterName = "@SearchText", SqlDbType = SqlDbType.NVarChar, Size = 200, Value = SearchText });
                    objSqlCommand.Parameters.Add(new SqlParameter() { ParameterName = "@Shouldhaveactivitycodes", SqlDbType = SqlDbType.VarChar, Value = Shouldhaveactivitycodes });
                    objSqlCommand.Parameters.Add(new SqlParameter() { ParameterName = "@Shouldnothaveactivitycodes", SqlDbType = SqlDbType.VarChar, Value = Shouldnothaveactivitycodes });
                    objSqlCommand.Parameters.Add(new SqlParameter() { ParameterName = "@PartnerCode", SqlDbType = SqlDbType.BigInt, Value = Partnercode });
                    objSqlCommand.CommandTimeout = 0;
                    UpdatedResult = sqlHelper.ExecuteDataSet(objSqlCommand).Tables[0];

                }
                if (UpdatedResult.Rows.Count > 0)
                {
                    foreach (DataRow row in UpdatedResult.Rows)
                    {
                        UserDetails userDetails = new UserDetails();
                        userDetails.FirstName = row["FirstName"] == null ? "" : row["FirstName"].ToString();
                        userDetails.LastName = row["LastName"] == null ? "" : row["LastName"].ToString();
                        userDetails.ContactCode = row["ContactCode"] == null ? 0 : Convert.ToInt64(row["ContactCode"]); ;
                        userDetails.EmailAddress = row["EmailAddress"] == null ? "" : row["EmailAddress"].ToString();
                        userDetails.UserName = row["UserName"] == null ? "" : row["UserName"].ToString();
                        objLstReqItems.Add(userDetails);
                    }
                }

                return objLstReqItems;
            }
            catch (Exception ex)
            {
                LogHelper.LogInfo(Log, "Error Occurred in GetAllUsersByActivityCode Method.");
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
        }


        public bool SaveTeamMember(GEP.Cumulus.Documents.Entities.Document objDocument)
        {

            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;
            long reqdocumentcode = 0;
            bool isSuccessful = false;
            try
            {

                var sqlHelper = ContextSqlConn;
                _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();
                SQLDocumentDAO sqlDocumentDAO = new SQLDocumentDAO();
                sqlDocumentDAO.SqlTransaction = _sqlTrans;
                sqlDocumentDAO.ReliableSqlDatabase = sqlHelper;
                sqlDocumentDAO.UserContext = UserContext;
                sqlDocumentDAO.GepConfiguration = GepConfiguration;
                reqdocumentcode = sqlDocumentDAO.SaveDocumentDetails(objDocument);
                //var task1 = GetDocumentStakeholderDetails(reqdocumentcode);
                //List<Task> TaskList = new List<Task>();
                //TaskList.Add(task1);
                //Task.WhenAll(TaskList);
                if (reqdocumentcode > 0)
                {
                    isSuccessful = true;
                    AddIntoSearchIndexerQueueing(reqdocumentcode, REQ, UserContext, GepConfiguration);
                }


                //return task1.Result;
                return isSuccessful;

            }
            catch (Exception ex)
            {

                if (_sqlTrans != null)
                {
                    _sqlTrans.Rollback();
                }
                throw ex;
            }
            finally
            {

                if (!ReferenceEquals(_sqlTrans, null))
                    _sqlTrans.Commit();

                if (!ReferenceEquals(_sqlCon, null) && _sqlCon.State != ConnectionState.Closed)
                {
                    _sqlCon.Close();
                    _sqlCon.Dispose();
                }

            }



        }

        public DataTable GetOrdersLinksForReqLineItem(long p2pLineItemId, long requisitionId)
        {

            SqlConnection objSqlCon = null;
            DataTable orderLinkData = null;
            try
            {
                ReliableSqlDatabase sqlHelper = null;

                sqlHelper = ContextSqlConn;
                objSqlCon = (SqlConnection)sqlHelper.CreateConnection();
                objSqlCon.Open();

                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_GETORDERSFORREQUISITIONLINE))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.Add(new SqlParameter() { ParameterName = "@P2PLineItemId", SqlDbType = SqlDbType.BigInt, Value = p2pLineItemId });
                    objSqlCommand.Parameters.Add(new SqlParameter() { ParameterName = "@RequisitionId", SqlDbType = SqlDbType.BigInt, Value = requisitionId });
                    objSqlCommand.CommandTimeout = 0;
                    orderLinkData = sqlHelper.ExecuteDataSet(objSqlCommand).Tables[0];
                }
                return orderLinkData;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in GetOrdersLinksForReqLineItem method in NewRequisitionDAO.", ex);
                throw ex;
            }
            finally
            {
                if (objSqlCon != null && objSqlCon.State != ConnectionState.Closed)
                {
                    objSqlCon.Close();
                    objSqlCon.Dispose();
                }
                if (orderLinkData != null)
                {
                    orderLinkData.Dispose();
                }
            }
        }

        public List<KeyValuePair<long, string>> GetRequesterAndAuthorFilter(int FilterFor, int pageSize, string term = "")
        {

            List<KeyValuePair<long, string>> dtUsersList = new List<KeyValuePair<long, string>>();
            DataSet requisitionUsersDataSet = new DataSet();
            SqlConnection objSqlCon = null;
            RefCountingDataReader objRefCountingDataReader = null;

            try
            {
                var sqlHelper = ContextSqlConn;
                objSqlCon = (SqlConnection)sqlHelper.CreateConnection();
                objSqlCon.Open();
                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_GETAllAuthorsAndRequestersForFilters))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;

                    objSqlCommand.Parameters.AddWithValue("@searchText", term);
                    objSqlCommand.Parameters.AddWithValue("@pageSize", pageSize);
                    objSqlCommand.Parameters.AddWithValue("@filterFor", FilterFor);
                    objSqlCommand.CommandTimeout = 0;
                    requisitionUsersDataSet = sqlHelper.ExecuteDataSet(objSqlCommand);
                }
                if (requisitionUsersDataSet != null && requisitionUsersDataSet.Tables.Count > 0)
                {
                    if (requisitionUsersDataSet.Tables[0].Rows != null && requisitionUsersDataSet.Tables[0].Rows.Count > 0)
                    {
                        foreach (DataRow row in requisitionUsersDataSet.Tables[0].Rows)
                        {
                            dtUsersList.Add(new KeyValuePair<long, string>(ConvertToInt64(row, "ContactCode"), ConvertToString(row, "UserName")));
                        }
                    }
                }
                return dtUsersList;
            }

            catch (Exception ex)
            {
                LogHelper.LogInfo(Log, "Error Occurred in GetRequesterAndAuthorFilter Method.");
                throw ex;

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

                LogHelper.LogInfo(Log, "GetRequesterAndAuthorFilter Method ended");

            }
        }
        public DataTable GetCategoryHirarchyByCategories(List<long> categories)
        {
            DataTable categoryHirarchy = new DataTable();
            SqlConnection sqlCon = null;
            var sqlHelper = ContextSqlConn;
            try
            {

                sqlCon = (SqlConnection)ContextSqlConn.CreateConnection();
                sqlCon.Open();
                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_GETCATEGORYHIRARCHYFORDEFAULTCATEGORY))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    SqlParameter pasCodes = new SqlParameter("@tvpPASCodesTable", SqlDbType.Structured);
                    pasCodes.TypeName = ReqSqlConstants.PASCODESTABLE;
                    pasCodes.Value = ConvertPasCodesToDatatable(categories);
                    objSqlCommand.Parameters.Add(pasCodes);
                    objSqlCommand.CommandTimeout = 0;
                    categoryHirarchy = sqlHelper.ExecuteDataSet(objSqlCommand).Tables[0];
                }
            }
            finally
            {
                if (!ReferenceEquals(sqlCon, null) && sqlCon.State != ConnectionState.Closed)
                {
                    sqlCon.Close();
                    sqlCon.Dispose();
                }

            }
            return categoryHirarchy;
        }
        private DataTable ConvertPasCodesToDatatable(List<long> categories)
        {
            DataTable dataTable = new DataTable() { Locale = CultureInfo.InvariantCulture, TableName = ReqSqlConstants.PASCODESTABLE };
            dataTable.Columns.Add("PASCode", typeof(long));

            foreach (long id in categories)
            {

                DataRow dr = dataTable.NewRow();
                dr["PASCode"] = id;
                dataTable.Rows.Add(dr);
            }

            return dataTable;

        }

        public List<RequisitionPartnerInfo> GetAllBuyerSuppliersAutoSuggest(long BuyerPartnerCode, string Status, string SearchText, int PageIndex, int PageSize, string OrgEntityCodes, long LOBEntityDetailCode, string RestrictedSupplierRelationTypes, long PASCode, long ContactCode)
        {
            LogHelper.LogInfo(Log, "GetAllBuyerSuppliersAutoSuggest Method Started");
            RefCountingDataReader refPASdr = null;
            List<RequisitionPartnerInfo> lstPartnerInfo = null;
            try
            {
                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("In GetAllBuyerSuppliersAutoSuggest Method of class ", typeof(NewRequisitionDAO).ToString(),
                                                " with parameter: BuyerPartnerCode = ", BuyerPartnerCode, " Status = ", Status, " SearchText = ", SearchText,
                                                " PageIndex = ", PageIndex, " PageSize = ", PageSize, " OrgEntityCodes = ", OrgEntityCodes, " ContactCode = ", ContactCode,
                                                " Stored Procedure to be executed is ", ReqSqlConstants.USP_P2P_REQ_GETALLBUYERSUPPLIERSAUTOSUGGEST));

                ReliableSqlDatabase sqlHelper = ContextSqlConn;

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = ReqSqlConstants.USP_P2P_REQ_GETALLBUYERSUPPLIERSAUTOSUGGEST;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter() { ParameterName = "@BuyerPartnerCode", SqlDbType = SqlDbType.BigInt, Value = BuyerPartnerCode });
                    cmd.Parameters.Add(new SqlParameter() { ParameterName = "@Status", SqlDbType = SqlDbType.VarChar, Value = Status });
                    cmd.Parameters.Add(new SqlParameter() { ParameterName = "@SearchText", SqlDbType = SqlDbType.NVarChar, Value = SearchText });
                    cmd.Parameters.Add(new SqlParameter() { ParameterName = "@PageIndex", SqlDbType = SqlDbType.Int, Value = PageIndex });
                    cmd.Parameters.Add(new SqlParameter() { ParameterName = "@PageSize", SqlDbType = SqlDbType.Int, Value = PageSize });
                    cmd.Parameters.Add(new SqlParameter() { ParameterName = "@OrgEntityCodes", SqlDbType = SqlDbType.VarChar, Value = OrgEntityCodes });
                    cmd.Parameters.Add(new SqlParameter() { ParameterName = "@LOBEntityDetailCode", SqlDbType = SqlDbType.BigInt, Value = LOBEntityDetailCode });
                    cmd.Parameters.Add(new SqlParameter() { ParameterName = "@RestrictedSupplierRelationTypes", SqlDbType = SqlDbType.VarChar, Value = RestrictedSupplierRelationTypes });
                    cmd.Parameters.Add(new SqlParameter() { ParameterName = "@PASCode", SqlDbType = SqlDbType.BigInt, Value = PASCode });
                    cmd.Parameters.Add(new SqlParameter() { ParameterName = "@ContactCode", SqlDbType = SqlDbType.BigInt, Value = ContactCode });

                    refPASdr = (RefCountingDataReader)sqlHelper.ExecuteReader(cmd);
                    SqlDataReader SQLDr = (SqlDataReader)refPASdr.InnerReader;

                    lstPartnerInfo = new List<RequisitionPartnerInfo>();

                    while (SQLDr.Read())
                    {
                        RequisitionPartnerInfo objPartnerInfo = new RequisitionPartnerInfo();
                        objPartnerInfo.PartnerCode = GetLongValue(SQLDr, ReqSqlConstants.COL_PARTNER_CODE);
                        objPartnerInfo.NoOfRecords = GetIntValue(SQLDr, ReqSqlConstants.COL_TOTAL_RECORDS);
                        objPartnerInfo.PartnerName = GetStringValue(SQLDr, ReqSqlConstants.COL_LEGALCOMPANYNAME);
                        objPartnerInfo.MatchType = GetIntValue(SQLDr, ReqSqlConstants.COL_PARTNERRECONMATCHTYPEID);
                        objPartnerInfo.ClientPartnerCode = GetStringValue(SQLDr, ReqSqlConstants.COL_CLIENT_PARTNERCODE);
                        objPartnerInfo.Status = GetStringValue(SQLDr, ReqSqlConstants.COL_PARTNERSTATUSDISPLAYNAME);
                        objPartnerInfo.HeadquarterLocation = new Address();
                        objPartnerInfo.HeadquarterLocation.Addressline1 = GetStringValue(SQLDr, ReqSqlConstants.COL_ADDRESS1);
                        objPartnerInfo.HeadquarterLocation.Addressline2 = GetStringValue(SQLDr, ReqSqlConstants.COL_ADDRESS2);
                        objPartnerInfo.HeadquarterLocation.Addressline3 = GetStringValue(SQLDr, ReqSqlConstants.COL_ADDRESS3);
                        objPartnerInfo.HeadquarterLocation.City = GetStringValue(SQLDr, ReqSqlConstants.COL_CITY);
                        objPartnerInfo.HeadquarterLocation.StateInfo.StateName = GetStringValue(SQLDr, ReqSqlConstants.COL_STATENAME);
                        objPartnerInfo.HeadquarterLocation.CountryInfo.CountryName = GetStringValue(SQLDr, ReqSqlConstants.COL_COUNTRYNAME);
                        objPartnerInfo.HeadquarterLocation.ZipCode = GetStringValue(SQLDr, ReqSqlConstants.COL_INV_SHIPTOZIPCODE);
                        lstPartnerInfo.Add(objPartnerInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occurred in GetAllBuyerSuppliersAutoSuggest method. BuyerPartnerCode : " + BuyerPartnerCode + " Status : " + Status + " SearchText :" + SearchText + " PageIndex : " + PageIndex + " PageSize : " + PageSize
                    + " OrgEntityCodes : " + OrgEntityCodes + "LOBEntityDetailCode : " + LOBEntityDetailCode + "ContactCode : " + ContactCode, ex);
                CustomFault objCustomFault = new CustomFault("Error while get GetAllBuyerSuppliersAutoSuggest", "GetAllBuyerSuppliersAutoSuggest", "GetAllBuyerSuppliersAutoSuggest", "PartnerInfo", ExceptionType.ApplicationException, BuyerPartnerCode.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while retrieving GetAllBuyerSuppliersAutoSuggest");
            }
            finally
            {
                if (refPASdr != null && refPASdr.IsClosed == false)
                {
                    refPASdr.Close();
                }
                LogHelper.LogInfo(Log, "GetAllBuyerSuppliersAutoSuggest Method Ended");
            }
            return lstPartnerInfo;
        }
        public List<PurchaseType> GetPurchaseTypes()
        {
            List<PurchaseType> lstPurchaseType = new List<PurchaseType>();
            RefCountingDataReader objRefCountingDataReader = null;
            try
            {
                LogHelper.LogInfo(Log, "Common GetPurchaseTypes");

                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("In Common getPurchaseTypes",
                                             "SP: usp_P2P_REQ_getPurchaseTypes"));
                var sqlHelper = ContextSqlConn;

                objRefCountingDataReader = (RefCountingDataReader)sqlHelper.ExecuteReader(ReqSqlConstants.USP_P2P_REQ_GETPURCHASETYPES);

                if (objRefCountingDataReader != null)
                {
                    var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;

                    while (sqlDr.Read())
                    {
                        lstPurchaseType.Add(new PurchaseType
                        {
                            PurchaseTypeId = GetTinyIntValue(sqlDr, ReqSqlConstants.COL_PURCHASETYPEID),
                            Description = GetStringValue(sqlDr, ReqSqlConstants.COL_DESCRIPTION),
                            IsDefault = GetBoolValue(sqlDr, ReqSqlConstants.COL_ISDEFAULT),
                            IsFlexibleCharge = GetBoolValue(sqlDr, ReqSqlConstants.COL_ISFLEXIBLECHARGE)

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
                LogHelper.LogInfo(Log, "GetPurchaseTypes");
            }
            return lstPurchaseType;
        }

        public List<ServiceType> GetPurchaseTypeItemExtendedTypeMapping()
        {
            List<ServiceType> lstServiceType = new List<ServiceType>();

            RefCountingDataReader objRefCountingDataReader = null;
            try
            {
                LogHelper.LogInfo(Log, "Common GetPurchaseTypeItemExtendedTypeMapping");

                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("In Common GetPurchaseTypeItemExtendedTypeMapping",
                                             "SP: USP_P2P_GetPurchaseTypeItemExtendedTypeMapping"));
                var sqlHelper = ContextSqlConn;

                objRefCountingDataReader = (RefCountingDataReader)sqlHelper.ExecuteReader(ReqSqlConstants.USP_P2P_GETPURCHASETYPEITEMEXTENDEDTYPEMAPPING);

                if (objRefCountingDataReader != null)
                {
                    var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;

                    while (sqlDr.Read())
                    {
                        lstServiceType.Add(new ServiceType
                        {
                            PurchaseTypeId = GetTinyIntValue(sqlDr, ReqSqlConstants.COL_PURCHASETYPEID),
                            ServiceTypeId = GetTinyIntValue(sqlDr, ReqSqlConstants.COL_ITEM_EXTENDED_TYPE),
                            IsDefault = GetBoolValue(sqlDr, ReqSqlConstants.COL_ISDEFAULT),
                            ItemType = GetStringValue(sqlDr, ReqSqlConstants.COL_ITEMTYPE),
                            MatchType = GetIntValue(sqlDr, ReqSqlConstants.COL_MATCHTYPE),
                            IsHeaderContractVisible = GetBoolValue(sqlDr, ReqSqlConstants.COL_ISHEADERCONTRACTVISIBLE),
                            IsOverallLimitEnabled = GetBoolValue(sqlDr, ReqSqlConstants.COL_ISOVERALLLIMITENABLED),
                            FulfillmentDocumentType = GetIntValue(sqlDr, ReqSqlConstants.COL_FULFILLMENTDOCUMENTTYPE)

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
                LogHelper.LogInfo(Log, "GetPurchaseTypeItemExtendedTypeMapping");
            }

            return lstServiceType;
        }

        public void UpdateRiskScore(long documentCode)
        {
            LogHelper.LogInfo(Log, "UpdateRiskScore Method started");
            SqlConnection objSqlCon = null;
            SqlTransaction objSqlTrans = null;
            try
            {
                Log.Debug(string.Concat("In UpdateRiskScore Method with parameter: DocumentCode = ", documentCode));
                var sqlHelper = ContextSqlConn;
                objSqlCon = (SqlConnection)sqlHelper.CreateConnection();

                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_UPDATERISKSCOREANDCATEGORY))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.Add(new SqlParameter() { ParameterName = "@documentCode", SqlDbType = SqlDbType.BigInt, Value = documentCode });
                    objSqlCommand.CommandTimeout = 0;
                    objSqlCon.Open();
                    objSqlTrans = objSqlCon.BeginTransaction();
                    var Result = sqlHelper.ExecuteNonQuery(objSqlCommand, objSqlTrans);
                    objSqlTrans.Commit();
                }
            }
            catch (Exception ex)
            {
                if (objSqlTrans != null)
                {
                    objSqlTrans.Rollback();
                }
                LogHelper.LogError(Log, "Error occured in UpdateRiskScore method. with parameter: DocumentCodes = " + documentCode, ex);
                CustomFault objCustomFault = new CustomFault("Error while UpdateRiskScore" + ex.Message, "UpdateRiskScore", "UpdateRiskScore", "UpdateRiskScore",

                ExceptionType.ApplicationException, string.Empty, false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while UpdateRiskScore" + ex.Message);
            }
            finally
            {
                if (objSqlCon != null && objSqlCon.State != ConnectionState.Closed)
                {
                    objSqlCon.Close();
                }
                Log.Info("UpdateRiskScore Method ended");
            }

        }


        public int UpdateDocumentBudgetoryStatus(long documentCode, Int16 budgetoryStatus, List<BudgetAllocationDetails> lstbudgetAllocationIds = null)
        {
            int Result = 0;
            LogHelper.LogInfo(Log, "UpdateDocumentBudgetoryStatus Method started");
            SqlConnection objSqlCon = null;
            try
            {
                Log.Debug(string.Concat("In UpdateRequisitionPreviousAmount Method with parameter: documentCode = ", documentCode));
                var sqlHelper = ContextSqlConn;
                objSqlCon = (SqlConnection)sqlHelper.CreateConnection();
                DataTable budgetAllocationIds = new DataTable() { Locale = CultureInfo.InvariantCulture, TableName = ReqSqlConstants.TVP_P2P_REQ_BUDGETALLOCATION };
                budgetAllocationIds.Columns.Add("BudgetEntityAllocationId", typeof(long));
                budgetAllocationIds.Columns.Add("BudgetId", typeof(long));
                budgetAllocationIds.Columns.Add("RequisitionSplititemId", typeof(long));
                budgetAllocationIds.Columns.Add("RequisitionId", typeof(long));
                budgetAllocationIds.Columns.Add("BudgetOwnerContactcode", typeof(long));

                if (lstbudgetAllocationIds != null)
                {
                    foreach (var item in lstbudgetAllocationIds)
                    {
                        DataRow dr = budgetAllocationIds.NewRow();
                        dr["BudgetEntityAllocationId"] = item.BudgetEntityAllocationId;
                        dr["BudgetId"] = item.BudgetId;
                        dr["RequisitionSplititemId"] = item.RequisitionSplititemId;
                        dr["RequisitionId"] = item.RequisitionId;
                        dr["BudgetOwnerContactcode"] = item.BudgetOwnerContactcode;
                        budgetAllocationIds.Rows.Add(dr);
                    }
                }
                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_UPDATEDOCUMENTBUDGETORYSTATUS))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.Add(new SqlParameter() { ParameterName = "@documentCode", SqlDbType = SqlDbType.BigInt, Value = documentCode });
                    objSqlCommand.Parameters.Add(new SqlParameter() { ParameterName = "@budgetoryStatus", SqlDbType = SqlDbType.TinyInt, Value = budgetoryStatus });
                    objSqlCommand.Parameters.Add(new SqlParameter("@tvp_P2P_BudgetAllocation", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_P2P_REQ_BUDGETALLOCATION,
                        Value = budgetAllocationIds
                    });
                    objSqlCommand.CommandTimeout = 0;
                    objSqlCon.Open();
                    Result = sqlHelper.ExecuteNonQuery(objSqlCommand);
                }

            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in UpdateDocumentBudgetoryStatus method. with parameter: documentCode = " + documentCode, ex);
                CustomFault objCustomFault = new CustomFault("Error while UpdateDocumentBudgetoryStatus" + ex.Message, "UpdateDocumentBudgetoryStatus", "UpdateDocumentBudgetoryStatus", "UpdateDocumentBudgetoryStatus",

                 ExceptionType.ApplicationException, string.Empty, false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while UpdateDocumentBudgetoryStatus" + ex.Message);
            }
            finally
            {
                if (objSqlCon != null && objSqlCon.State != ConnectionState.Closed)
                {
                    objSqlCon.Close();
                }
                Log.Info("UpdateDocumentBudgetoryStatus Method ended");
            }
            return Result;
        }



        public bool UpdateRequisitionItemStatusWorkBench(string IsCreatePOorRfx, DataTable dt, DataTable tableReqIds, long rfxId)
        {
            bool result = false;
            SqlConnection objSqlCon = null;
            try
            {


                var sqlHelper = ContextSqlConn;
                objSqlCon = (SqlConnection)sqlHelper.CreateConnection();

                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_UPDATEREQUISITIONITEMSTATUSWORKBENCH))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.Add(new SqlParameter() { ParameterName = "@IsCreatePOorRfx", SqlDbType = SqlDbType.VarChar, Value = IsCreatePOorRfx });
                    objSqlCommand.Parameters.Add(new SqlParameter() { ParameterName = "@tvp_P2P_REQ_ReqItemIds", SqlDbType = SqlDbType.Structured, Value = dt });
                    objSqlCommand.Parameters.Add(new SqlParameter() { ParameterName = "@rfxId", SqlDbType = SqlDbType.BigInt, Value = rfxId });
                    objSqlCommand.CommandTimeout = 0;
                    objSqlCon.Open();
                    sqlHelper.ExecuteDataSet(objSqlCommand);

                    result = true;
                    List<long> reqids = tableReqIds.AsEnumerable().Select(r => r.Field<long>("Id")).ToList();
                    AddIntoSearchIndexerQueueing(reqids, REQ, UserContext, GepConfiguration);
                }

            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error Occured in SaveRequisitionItemRFxMapping method", ex);
                throw;
            }
            finally
            {
                if (objSqlCon != null && objSqlCon.State != ConnectionState.Closed)
                {
                    objSqlCon.Close();
                }
            }

            return result;
        }

        public long GetRequisitionItemRFxLink(long reqItemId)
        {
            long result = 0;
            DataTable Result = null;
            LogHelper.LogInfo(Log, "GetRequisitionItemRFxLink Method started");
            SqlConnection objSqlCon = null;
            try
            {
                var sqlHelper = ContextSqlConn;
                objSqlCon = (SqlConnection)sqlHelper.CreateConnection();

                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_GETREQUISITIONITEMRFXLINK))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.Add(new SqlParameter() { ParameterName = "@reqItemId", SqlDbType = SqlDbType.BigInt, Value = reqItemId });
                    objSqlCommand.CommandTimeout = 0;
                    objSqlCon.Open();
                    Result = sqlHelper.ExecuteDataSet(objSqlCommand).Tables[0];

                    if (Result.Rows != null && Result.Rows.Count > 0)
                    {
                        result = Result.Rows[0]["LinkedDocumentCode"] == null ? 0 : Convert.ToInt64(Result.Rows[0]["LinkedDocumentCode"]);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error Occured in GetRequisitionItemRFxLink method", ex);
                throw;
            }
            finally
            {
                if (objSqlCon != null && objSqlCon.State != ConnectionState.Closed)
                {
                    objSqlCon.Close();
                }
            }

            return result;

        }
        public List<DocumentDelegation> GetDocumentsForUtility(long contactCode, string searchText)
        {
            List<DocumentDelegation> lstDocuments = new List<DocumentDelegation>();
            RefCountingDataReader objRefCountingDataReader = null;
            try
            {
                objRefCountingDataReader =
                    (RefCountingDataReader)
                    ContextSqlConn.ExecuteReader(ReqSqlConstants.USP_P2P_REQ_GETALLREQUISITIONSFORUTILITY, contactCode, searchText);
                if (objRefCountingDataReader != null)
                {
                    var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;
                    while (sqlDr.Read())
                    {
                        var document = new DocumentDelegation
                        {
                            DocumentCode = GetLongValue(sqlDr, ReqSqlConstants.COL_DOCUMENT_CODE),
                            DocumentName = GetStringValue(sqlDr, ReqSqlConstants.COL_DOCUMENT_NAME),
                            DocumentNumber = GetStringValue(sqlDr, ReqSqlConstants.COL_DOCUMENT_NUMBER)
                        };
                        lstDocuments.Add(document);
                    }
                    return lstDocuments;
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
            return new List<DocumentDelegation>();
        }


        public bool SaveDocumentRequesterChange(List<long> documentRequesterChangeList, long contactCode)
        {
            bool result = false;
            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;
            LogHelper.LogInfo(Log, "SaveDocumentRequesterChange Method Started");
            try
            {
                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("In SaveDocumentRequesterChange Method.", "SP: USP_P2P_REQ_SAVEREQUISITIONFORREQUESTERCHANGE"));

                var sqlHelper = ContextSqlConn;
                _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();

                DataTable dtdocumentCodes = new DataTable() { Locale = CultureInfo.InvariantCulture, TableName = ReqSqlConstants.TVP_LONG };
                dtdocumentCodes.Columns.Add("Id", typeof(long));
                if (documentRequesterChangeList != null)
                {
                    foreach (var documentCode in documentRequesterChangeList)
                    {
                        DataRow dr = dtdocumentCodes.NewRow();
                        dr["Id"] = documentCode;
                        dtdocumentCodes.Rows.Add(dr);
                    }
                }

                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_SAVEREQUISITIONFORREQUESTERCHANGE))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.Add(new SqlParameter("@requisitionIds", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_LONG,
                        Value = dtdocumentCodes
                    });
                    objSqlCommand.Parameters.AddWithValue("@ContactCode", contactCode);
                    int i = sqlHelper.ExecuteNonQuery(objSqlCommand, _sqlTrans);
                    if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
                        _sqlTrans.Commit();
                    if (i > 0)
                        result = true;
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
                LogHelper.LogInfo(Log, "SaveDocumentRequesterChange Method Ended");
            }
            return result;
        }
        public bool PerformReIndexForDocuments(List<long> documentReIndexList)
        {
            bool result = false;
            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;
            LogHelper.LogInfo(Log, "PerformReIndexForDocuments Method Started");
            try
            {
                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("In PerformReIndexForDocuments Method.", "SP: USP_SEARCH_SAVEINDEXERQUEUEINGDETAILS"));

                var sqlHelper = ContextSqlConn;
                _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();

                DataTable dtDocumentReIndex = new DataTable() { Locale = CultureInfo.InvariantCulture, TableName = ReqSqlConstants.TVP_ES_DOCCODETYPEMAPPING };
                dtDocumentReIndex.Columns.Add("DocumentCode", typeof(long));
                dtDocumentReIndex.Columns.Add("DocumentTypeCode", typeof(int));

                if (dtDocumentReIndex != null)
                {
                    foreach (var document in documentReIndexList)
                    {
                        DataRow dr = dtDocumentReIndex.NewRow();
                        dr["DocumentCode"] = document;
                        dr["DocumentTypeCode"] = ReqSqlConstants.COL_REQ_DOCUMENTTYPECODE;
                        dtDocumentReIndex.Rows.Add(dr);
                    }
                }

                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_SEARCH_SAVEINDEXERQUEUEINGDETAILS))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.Add(new SqlParameter("@tvp_ES_DocCodeTypeMapping", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_ES_DOCCODETYPEMAPPING,
                        Value = dtDocumentReIndex
                    });
                    sqlHelper.ExecuteNonQuery(objSqlCommand, _sqlTrans);
                    if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
                    {
                        _sqlTrans.Commit();
                        result = true;
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
                LogHelper.LogInfo(Log, "PerformReIndexForDocuments Method Ended");
            }
            return result;
        }

        public bool SaveReqItemBlanketMapping(List<BlanketItems> UtilizedBlankets, long reqItemId)
        {
            bool result = false;
            DataTable categoryHirarchy = new DataTable();
            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;
            var sqlHelper = ContextSqlConn;
            try
            {
                _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();

                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_SAVEREQITEMBLANKETMAPPING))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    SqlParameter BlanketMapper = new SqlParameter("@tvp_P2P_RequisitionItem_BlanketMapping", SqlDbType.Structured);
                    BlanketMapper.TypeName = ReqSqlConstants.TVP_P2P_REQ_BLANKETMAPPING;
                    BlanketMapper.Value = ConvertBlanketItemsToTableTypes(UtilizedBlankets, reqItemId);
                    objSqlCommand.Parameters.Add(BlanketMapper);
                    objSqlCommand.CommandTimeout = 0;
                    result = Convert.ToBoolean(sqlHelper.ExecuteNonQuery(objSqlCommand, _sqlTrans));
                }
                if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
                    _sqlTrans.Commit();
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
                LogHelper.LogInfo(Log, "Requisition SaveReqItemBlanketMapping Method Ended for ReqItemId = " + reqItemId);
            }

            return result;
        }

        private DataTable ConvertBlanketItemsToTableTypes(List<BlanketItems> UtilizedBlankets, long reqItemId)
        {
            DataTable dtRequisitionItem = new DataTable() { Locale = CultureInfo.InvariantCulture, TableName = ReqSqlConstants.TVP_P2P_REQ_BLANKETMAPPING };

            dtRequisitionItem.Columns.Add("RequisitionItemId", typeof(long));
            dtRequisitionItem.Columns.Add("BlanketDocumentCode", typeof(long));
            dtRequisitionItem.Columns.Add("BlanketDocumentNumber", typeof(string));
            dtRequisitionItem.Columns.Add("Quantity", typeof(decimal));
            dtRequisitionItem.Columns.Add("BlanketLineItemNo", typeof(Int32));
            dtRequisitionItem.Columns.Add("UnitPrice", typeof(decimal));
            dtRequisitionItem.Columns.Add("BlanketUtilized", typeof(decimal));
            dtRequisitionItem.Columns.Add("BlanketPending", typeof(decimal));
            if (UtilizedBlankets != null)
            {
                foreach (var objRequisitonItem in UtilizedBlankets)
                {
                    DataRow dr = dtRequisitionItem.NewRow();
                    dr["RequisitionItemId"] = reqItemId;
                    dr["BlanketDocumentCode"] = objRequisitonItem.BlanketDocumentCode;
                    dr["BlanketDocumentNumber"] = objRequisitonItem.BlanketDocumentNumber;
                    dr["Quantity"] = objRequisitonItem.Quantity;
                    dr["BlanketLineItemNo"] = objRequisitonItem.BlanketLineItemNo;
                    dr["UnitPrice"] = objRequisitonItem.UnitPrice;
                    dr["BlanketUtilized"] = objRequisitonItem.BlanketUtilized;
                    dr["BlanketPending"] = objRequisitonItem.BlanketPending;
                    dtRequisitionItem.Rows.Add(dr);
                }
            }
            return dtRequisitionItem;
        }

        public IdNameAndAddress GetOrderingLoctionNameByLocationId(long locationId)
        {
            SqlConnection objSqlCon = null;
            SqlDataReader sqlDr = null;
            RefCountingDataReader objRefCountingDataReader = null;
            IdNameAndAddress orderingLocation = new IdNameAndAddress();
            try
            {
                LogHelper.LogInfo(Log, "GetOrderingLoctionNameByLocationId Method Started for locationId=" + locationId + "");

                var sqlHelper = ContextSqlConn;
                objSqlCon = (SqlConnection)sqlHelper.CreateConnection();
                objSqlCon.Open();

                if (Log.IsDebugEnabled)
                    Log.Debug(string.Concat("In GetOrderingLoctionNameByLocationId Method sp usp_P2P_REQ_GetOrderingLocationByLocationID with parameter: locationId=" + locationId, " was called."));
                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_GETORDERINGLOCATIONBYLOCATIONID))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;

                    objSqlCommand.Parameters.AddWithValue("@LocationId", locationId);
                    objSqlCommand.CommandTimeout = 0;
                    objRefCountingDataReader = (RefCountingDataReader)ContextSqlConn.ExecuteReader(objSqlCommand);
                }

                if (objRefCountingDataReader != null)
                {
                    sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;
                    while (sqlDr.Read())
                    {
                        orderingLocation.id = locationId;
                        orderingLocation.address = BuildAddress(GetStringValue(sqlDr, ReqSqlConstants.COL_ORDLOCADDRESS1), GetStringValue(sqlDr, ReqSqlConstants.COL_ORDLOCADDRESS2),
                        GetStringValue(sqlDr, ReqSqlConstants.COL_ORDLOCADDRESS3), GetStringValue(sqlDr, ReqSqlConstants.COL_ORDLOCCITY),
                        GetStringValue(sqlDr, ReqSqlConstants.COL_ORDLOCSTATE), GetStringValue(sqlDr, ReqSqlConstants.COL_ORDLOCCOUNTRY),
                        GetStringValue(sqlDr, ReqSqlConstants.COL_ORDLOCZIP));
                        orderingLocation.name = GetStringValue(sqlDr, ReqSqlConstants.OL_COL_LocationName);
                    }
                }
                return orderingLocation;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in GetOrderingLoctionNameByLocationId method.", ex);

                var objCustomFault = new CustomFault(ex.Message, "GetOrderingLoctionNameByLocationId", "GetOrderingLoctionNameByLocationId",
                                                       "GetOrderingLoctionNameByLocationId", ExceptionType.ApplicationException,
                                                       locationId.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                        "Error while GetOrderingLoctionNameByLocationId " + ex.Message + " Stack Trace: " + ex.StackTrace + "Inner Exception: " + ex.InnerException);
            }
            finally
            {
                if (!ReferenceEquals(objSqlCon, null) && objSqlCon.State != ConnectionState.Closed)
                {
                    objSqlCon.Close();
                    objSqlCon.Dispose();
                }
                LogHelper.LogInfo(Log, "GetOrderingLoctionNameByLocationId Method Ended for locationId=" + locationId);
            }
        }

        public List<BlanketDetails> GetBlanketDetailsForReqLineItem(long requisitionItemId)
        {
            SqlConnection objSqlCon = null;
            List<BlanketDetails> blanketData = new List<BlanketDetails>();
            RefCountingDataReader objRefCountingDataReader = null;
            try
            {
                var sqlHelper = ContextSqlConn;
                objSqlCon = (SqlConnection)sqlHelper.CreateConnection();
                objSqlCon.Open();

                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_GETBLANKETDETAILSBYREQITEMID))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.Add(new SqlParameter()
                    {
                        ParameterName = "@RequisitionItemId",
                        SqlDbType = SqlDbType.BigInt,
                        Value = requisitionItemId
                    });
                    objSqlCommand.CommandTimeout = 0;
                    objRefCountingDataReader = (RefCountingDataReader)sqlHelper.ExecuteReader(objSqlCommand);
                }
                if (objRefCountingDataReader != null)
                {
                    var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;
                    while (sqlDr.Read())
                    {
                        BlanketDetails blanketDetails = new BlanketDetails();
                        blanketDetails.BlanketDocumentNumber = GetStringValue(sqlDr, ReqSqlConstants.BLANKET_DOCUMENT_NUMBER);
                        blanketDetails.AmountConsumed = GetDecimalValue(sqlDr, ReqSqlConstants.BLANKET_AMOUNT_CONSUMED);
                        blanketData.Add(blanketDetails);
                    }
                }
                return blanketData;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in GetBlanketDetailsForReqLineItem method in NewRequisitionDAO.", ex);
                throw ex;
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
        }

        public List<PartnerLocation> GetAllSupplierLocationsByOrgEntity(string OrgEntityCode, int PartnerLocationType = 2)
        {
            RefCountingDataReader objRefCountingDataReader = null;
            List<PartnerLocation> lstPartnerLocations = new List<PartnerLocation>();

            try
            {
                var sqlHelper = ContextSqlConn;
                objRefCountingDataReader = (RefCountingDataReader)sqlHelper.ExecuteReader(ReqSqlConstants.USP_REQ_GETALLSUPPLIERLOCBYLOBAndOrgEntity, new object[] {
                    OrgEntityCode,PartnerLocationType});
                if (objRefCountingDataReader != null)
                {
                    var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;
                    while (sqlDr.Read())
                    {
                        lstPartnerLocations.Add(new PartnerLocation
                        {
                            LocationName = GetStringValue(sqlDr, ReqSqlConstants.OL_COL_LocationName),
                            ClientLocationCode = GetStringValue(sqlDr, ReqSqlConstants.OL_COL_LocationCode),
                            LocationId = GetLongValue(sqlDr, ReqSqlConstants.COL_LOCATIONID),
                            PartnerCode = GetLongValue(sqlDr, ReqSqlConstants.COL_PARTNER_CODE),
                            PartnerName = GetStringValue(sqlDr, ReqSqlConstants.COL_PARTNER_NAME),
                            IsDefault = GetBoolValue(sqlDr, ReqSqlConstants.COL_ISDEFAULT)

                        });
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occurred in GetAllSupplierLocationsByOrgEntity Method", ex);
                throw new Exception("Error occurred in GetAllSupplierLocationsByOrgEntity Method");
            }
            finally
            {
                if (!ReferenceEquals(objRefCountingDataReader, null) && !objRefCountingDataReader.IsClosed)
                {
                    objRefCountingDataReader.Close();
                    objRefCountingDataReader.Dispose();
                }
            }
            return lstPartnerLocations;
        }

        public long GetOrgEntityManagers(long orgEntityCode)
        {
            long ManagerContactCode = 0;
            SqlConnection objSqlCon = null;
            var sqlHelper = ContextSqlConn;
            objSqlCon = (SqlConnection)sqlHelper.CreateConnection();
            objSqlCon.Open();
            try
            {
                DataSet ds = sqlHelper.ExecuteDataSet(ReqSqlConstants.USP_WF_GETMANAGERFORORGENTITY, orgEntityCode, 0);
                if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
                {
                    ManagerContactCode = Convert.ToInt64(ds.Tables[1].Rows[0][ReqSqlConstants.COL_CONTACT_CODE], CultureInfo.InvariantCulture);
                }
                return ManagerContactCode;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occurred in GetOrgEntityManagers method ", ex);
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

        }


        public long SaveDocumentDetails(GEP.Cumulus.Documents.Entities.Document document)
        {

            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;
            SQLDocumentDAO sqlDocumentDAO = new SQLDocumentDAO();
            try
            {
                LogHelper.LogInfo(Log, "SaveDocumentDetails Method Started");
                _sqlCon = (SqlConnection)ContextSqlConn.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();

                sqlDocumentDAO.SqlTransaction = _sqlTrans;
                sqlDocumentDAO.ReliableSqlDatabase = ContextSqlConn;
                sqlDocumentDAO.UserContext = UserContext;
                sqlDocumentDAO.GepConfiguration = GepConfiguration;
                long documentCode = sqlDocumentDAO.SaveDocumentDetails(document);

                if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
                    _sqlTrans.Commit();


                return documentCode;
            }
            catch (Exception ex)
            {
                if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
                {
                    LogHelper.LogError(Log, "Error occured in SaveDocumentDetails Method in NewRequisitionDAO", ex);
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

            }

        }

        public bool updateStatusOfMultipleRequisitionItems(string requisitionIds, DocumentStatus statusTobeUpdated)
        {
            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;
            bool result = false;
            try
            {
                var sqlHelper = ContextSqlConn;
                _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();
                Int32 status = Convert.ToInt32(statusTobeUpdated);
                result = Convert.ToBoolean(sqlHelper.ExecuteNonQuery(_sqlTrans, ReqSqlConstants.USP_P2P_REQ_UPDATEMUTIPLEREQUISITIONITEMSSTATUSES, status, requisitionIds), NumberFormatInfo.InvariantInfo);
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
                    catch (InvalidOperationException exe)
                    {
                        LogHelper.LogError(Log, "Error occured in updateStatusOfMultipleRequisitionItems method.", ex);

                        var objCustomFault = new CustomFault(ex.Message, "updateStatusOfMultipleRequisitionItems", "updateStatusOfMultipleRequisitionItems",
                                                             "Common", ExceptionType.ApplicationException,
                                                             requisitionIds, false);
                        throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                              "Error while updateStatusOfMultipleRequisitionItems " + ex.Message + " Stack Trace: " + ex.StackTrace + "Inner Exception: " + ex.InnerException);
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
            }
            return result;
        }
        public Boolean UpdateRequisitionItemTaxJurisdiction(List<KeyValuePair<long, string>> lstItemTaxJurisdictions)
        {
            SqlConnection _sqlCon = null;
            SqlTransaction _sqlTrans = null;
            bool result = false;
            try
            {
                var sqlHelper = ContextSqlConn;
                _sqlCon = (SqlConnection)sqlHelper.CreateConnection();
                _sqlCon.Open();
                _sqlTrans = _sqlCon.BeginTransaction();
                DataTable itemTaxJurisdiction = new DataTable() { Locale = CultureInfo.InvariantCulture, TableName = ReqSqlConstants.TVP_P2P_KEYVALUEIDNAME };
                itemTaxJurisdiction.Columns.Add("Id", typeof(long));
                itemTaxJurisdiction.Columns.Add("Value", typeof(string));

                if (lstItemTaxJurisdictions != null)
                {
                    foreach (var item in lstItemTaxJurisdictions)
                    {
                        DataRow dr = itemTaxJurisdiction.NewRow();
                        dr["Id"] = item.Key;
                        dr["Value"] = item.Value;
                        itemTaxJurisdiction.Rows.Add(dr);
                    }
                }
                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_UPDATEREQUISITIONITEMTAXJURISDICTION))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.Add(new SqlParameter("@tvp_P2P_KeyValueIdName", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_P2P_KEYVALUEIDNAME,
                        Value = itemTaxJurisdiction
                    });
                    sqlHelper.ExecuteNonQuery(objSqlCommand, _sqlTrans);
                    if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
                    {
                        _sqlTrans.Commit();
                        result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                if (!ReferenceEquals(_sqlTrans, null) && !ReferenceEquals(_sqlCon, null) && _sqlCon.State == ConnectionState.Open)
                {
                    try
                    {
                        _sqlTrans.Rollback();
                    }
                    catch (InvalidOperationException exe)
                    {
                        LogHelper.LogError(Log, "Error occured in UpdateRequisitionItemTaxJurisdiction method.", ex);

                        var objCustomFault = new CustomFault(ex.Message, "UpdateRequisitionItemTaxJurisdiction", "UpdateRequisitionItemTaxJurisdiction",
                                                             "Common", ExceptionType.ApplicationException, "", false);
                        throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                              "Error while updateStatusOfMultipleRequisitionItems " + ex.Message + " Stack Trace: " + ex.StackTrace + "Inner Exception: " + ex.InnerException);
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
            }
            return result;
        }

        public List<User> GetUsersBasedOnEntityDetailsCode(string orgEntityCodes, long PartnerCode, int PageIndex, long ContactCode, int PageSize, string SearchText = "", string ActivityCodes = "")
        {
            SqlConnection objSqlCon = null;
            DataSet ds = new DataSet();
            List<User> lstUser = new List<User>();
            try
            {
                ReliableSqlDatabase sqlHelper = ContextSqlConn;
                objSqlCon = (SqlConnection)sqlHelper.CreateConnection();
                objSqlCon.Open();
                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_GETUSERBASEDONENTITYDETAILSCODE, objSqlCon))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    DataTable tblOrgEntityCodes = CreateOrgEntityTVP(orgEntityCodes);

                    objSqlCommand.Parameters.AddWithValue("@ActivityCodes", "");
                    objSqlCommand.Parameters.AddWithValue("@SearchText", SearchText);
                    objSqlCommand.Parameters.AddWithValue("@PartnerCode", PartnerCode);
                    objSqlCommand.Parameters.AddWithValue("@PageIndex", PageIndex);
                    objSqlCommand.Parameters.AddWithValue("@PageSize", PageSize);
                    objSqlCommand.Parameters.AddWithValue("@ContactCode", ContactCode);
                    objSqlCommand.Parameters.Add(new SqlParameter("@tvpOrgEntity", SqlDbType.Structured)
                    {
                        TypeName = "tvp_ORGEntityCodesList",
                        Value = tblOrgEntityCodes
                    });

                    ds = sqlHelper.ExecuteDataSet(objSqlCommand);
                    if (!ReferenceEquals(ds, null) && ds.Tables.Count > 0)
                    {
                        foreach (DataRow row in ds.Tables[0].Rows)
                        {
                            User user = new User();
                            user.ContactCode = Convert.ToInt64(row[ReqSqlConstants.COL_CONTACTCODE]);
                            user.FirstName = Convert.ToString(row[ReqSqlConstants.COL_FIRSTNAME]);
                            user.LastName = Convert.ToString(row[ReqSqlConstants.COL_LASTNAME]);
                            user.EmailAddress = Convert.ToString(row[ReqSqlConstants.COL_EMAILADDRESS]);
                            user.TotalRecords = Convert.ToInt32(row[ReqSqlConstants.COL_TOTAL_RECORDS]);
                            lstUser.Add(user);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in GetUsersBasedOnEntityDetailsCode method of NewRequisitionDAO", ex);
                throw ex;
            }
            finally
            {
                if (objSqlCon != null && objSqlCon.State != ConnectionState.Closed)
                {
                    objSqlCon.Close();
                }
                LogHelper.LogInfo(Log, "GetUsersBasedOnEntityDetailsCode method ended");
            }

            return lstUser;
        }

        private DataTable CreateOrgEntityTVP(string orgEntityCodes)
        {
            DataTable dtOrgEntityCode = new DataTable();
            dtOrgEntityCode.Columns.Add(ReqSqlConstants.COL_ENTITYCODE, typeof(long));
            List<string> lstEntity = orgEntityCodes.Split(',').Distinct().ToList();
            lstEntity.ForEach((x) =>
            {
                DataRow drEntityCode = dtOrgEntityCode.NewRow();
                drEntityCode[0] = Convert.ToInt64(x.Trim());
                dtOrgEntityCode.Rows.Add(drEntityCode);
            });
            return dtOrgEntityCode;
        }
        public DataSet GetPartnerSourceSystemDetailsByReqId(long requisitionId)
        {

            DataSet requisitionUsersDataSet = new DataSet();
            SqlConnection objSqlCon = null;
            RefCountingDataReader objRefCountingDataReader = null;

            try
            {
                var sqlHelper = ContextSqlConn;
                objSqlCon = (SqlConnection)sqlHelper.CreateConnection();
                objSqlCon.Open();
                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_REQ_GETPARTNERSOURCESYSTEMDETAILSBYREQID))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;

                    objSqlCommand.Parameters.AddWithValue("@requisitionId", requisitionId);
                    objSqlCommand.CommandTimeout = 0;
                    requisitionUsersDataSet = sqlHelper.ExecuteDataSet(objSqlCommand);
                }
                return requisitionUsersDataSet;
            }

            catch (Exception ex)
            {
                LogHelper.LogInfo(Log, "Error Occurred in GetRequesterAndAuthorFilter Method.");
                throw ex;

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

                LogHelper.LogInfo(Log, "GetPartnerSourceSystemDetailsByReqId Method ended");

            }
        }


        public List<BudgetAllocationDetails> GetBudgetDetails(long requisitionId)
        {
            SqlConnection objSqlCon = null;
            DataSet ds = new DataSet();
            List<BudgetAllocationDetails> budgetAllocationDetails = new List<BudgetAllocationDetails>();
            try
            {
                ReliableSqlDatabase sqlHelper = ContextSqlConn;
                objSqlCon = (SqlConnection)sqlHelper.CreateConnection();
                objSqlCon.Open();
                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_GETBUDGETDETAILS, objSqlCon))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.AddWithValue("@documentCode", requisitionId);

                    ds = sqlHelper.ExecuteDataSet(objSqlCommand);
                    if (!ReferenceEquals(ds, null) && ds.Tables.Count > 0)
                    {
                        foreach (DataRow row in ds.Tables[0].Rows)
                        {
                            BudgetAllocationDetails user = new BudgetAllocationDetails();
                            user.RequisitionSplititemId = Convert.ToInt64(row[ReqSqlConstants.REQUISITIONSPLITITEMID]);
                            user.BudgetEntityAllocationId = Convert.ToInt64(row[ReqSqlConstants.BUDGETENTITYALLOCATIONID]);
                            user.BudgetId = Convert.ToInt64(row[ReqSqlConstants.BUDGETID]);
                            user.RequisitionId = Convert.ToInt64(row[ReqSqlConstants.REQUISITIONID]);
                            budgetAllocationDetails.Add(user);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in GetUsersBasedOnEntityDetailsCode method of NewRequisitionDAO", ex);
                throw ex;
            }
            finally
            {
                if (objSqlCon != null && objSqlCon.State != ConnectionState.Closed)
                {
                    objSqlCon.Close();
                }
                LogHelper.LogInfo(Log, "GetUsersBasedOnEntityDetailsCode method ended");
            }

            return budgetAllocationDetails;
        }

        public List<TaxJurisdiction> GetTaxJurisdcitionsByShiptoLocationIds(List<long> shipToLocationIds)
        {
            SqlConnection objSqlCon = null;
            DataSet ds = new DataSet();
            List<TaxJurisdiction> taxJurisdictions = new List<TaxJurisdiction>();
            try
            {
                ReliableSqlDatabase sqlHelper = ContextSqlConn;
                objSqlCon = (SqlConnection)sqlHelper.CreateConnection();
                objSqlCon.Open();
                DataTable dtShipToIds = new DataTable() { Locale = CultureInfo.InvariantCulture, TableName = ReqSqlConstants.TVP_LONG };
                dtShipToIds.Columns.Add("Id", typeof(long));
                foreach (var id in shipToLocationIds)
                {
                    DataRow dr = dtShipToIds.NewRow();
                    dr["Id"] = id;
                    dtShipToIds.Rows.Add(dr);
                }
                using (SqlCommand objSqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_GETTAXJURISDICTIONSBYSHIPTOLOCATIONID, objSqlCon))
                {
                    objSqlCommand.CommandType = CommandType.StoredProcedure;
                    objSqlCommand.Parameters.Add(new SqlParameter("@tvp_shipToLocationIds", SqlDbType.Structured)
                    {
                        TypeName = ReqSqlConstants.TVP_LONG,
                        Value = dtShipToIds
                    });

                    ds = sqlHelper.ExecuteDataSet(objSqlCommand);
                    if (!ReferenceEquals(ds, null) && ds.Tables.Count > 0)
                    {
                        foreach (DataRow row in ds.Tables[0].Rows)
                        {
                            TaxJurisdiction taxJurisdiction = new TaxJurisdiction();
                            taxJurisdiction.ShiptoLocationId = Convert.ToInt64(row[ReqSqlConstants.SHIPTOLOCATIONID]);
                            taxJurisdiction.JurisdictionCode = Convert.ToString(row[ReqSqlConstants.JURISDICTIONCODE]);
                            taxJurisdiction.JurisdictionName = Convert.ToString(row[ReqSqlConstants.JURISDICTIONNAME]);
                            taxJurisdiction.JurisdictionId = Convert.ToInt64(row[ReqSqlConstants.JURISDICTIONID]);
                            taxJurisdictions.Add(taxJurisdiction);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in GetTaxJurisdcitionsByShiptoLocationIds method of NewRequisitionDAO", ex);
                throw ex;
            }
            finally
            {
                if (objSqlCon != null && objSqlCon.State != ConnectionState.Closed)
                {
                    objSqlCon.Close();
                }
                LogHelper.LogInfo(Log, "GetTaxJurisdcitionsByShiptoLocationIds method ended");
            }

            return taxJurisdictions;
        }

        public List<PASMasterData> GetAllLevelCategories(long lOBEntityDetailCode)
        {
            SqlConnection sqlConnection = null;
            DataSet ds = new DataSet();
            List<PASMasterData> lstCategories = new List<PASMasterData>();
            try
            {
                ReliableSqlDatabase sqlHelper = ContextSqlConn;
                sqlConnection = (SqlConnection)sqlHelper.CreateConnection();
                sqlConnection.Open();
                using (SqlCommand sqlCommand = new SqlCommand(ReqSqlConstants.USP_P2P_REQ_GetAllLevelCategories, sqlConnection))
                {
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.Parameters.AddWithValue("@LOBEntityDetailCode", lOBEntityDetailCode);
                    ds = sqlHelper.ExecuteDataSet(sqlCommand);
                    if (!ReferenceEquals(ds, null) && ds.Tables.Count > 0)
                    {
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            PASMasterData pasMasterData = new PASMasterData();
                            pasMasterData.ClientPASCode = ConvertToString(dr, ReqSqlConstants.COL_CLIENT_PASCODE);
                            pasMasterData.PASCode = ConvertToInt64(dr, ReqSqlConstants.COL_PASCODE);
                            pasMasterData.PASName = ConvertToString(dr, ReqSqlConstants.COL_CATEGORYHIERARCHY);
                            lstCategories.Add(pasMasterData);
                        }

                    }

                }

            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in GetAllLevelCategories in NewRequisitionDAO", ex);
            }
            finally
            {
                if (sqlConnection != null && sqlConnection.State != ConnectionState.Closed)
                {
                    sqlConnection.Close();
                    sqlConnection.Dispose();
                }
                LogHelper.LogInfo(Log, "GetUsersBasedOnEntityDetailsCode method ended");
            }

            return lstCategories;
        }

        public List<long> GetP2PLineitemIdbasedOnPartnerandCurrencyCode(long requisitionId, long partnerCode, long locationId, string currencyCode)
        {
            RefCountingDataReader objRefCountingDataReader = null;
            List<long> lstP2PLineItemID = new List<long>();
            try
            {
                objRefCountingDataReader =
                 (RefCountingDataReader)
                 ContextSqlConn.ExecuteReader(ReqSqlConstants.USP_P2P_REQ_GETP2PLINEITEMIDBYPARTENRANDCURRENCYCODE,
                                                                  new object[] { requisitionId, partnerCode, locationId, currencyCode });
                if (objRefCountingDataReader != null)
                {
                    var sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;

                    while (sqlDr.Read())
                    {
                        long P2PLineItemId = GetLongValue(sqlDr, ReqSqlConstants.COL_P2P_LINE_ITEM_ID);
                        lstP2PLineItemID.Add(P2PLineItemId);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in GetP2PLineitemIdbasedOnPartnerandCurrencyCode method.", ex);
            }
            finally
            {
                if (!ReferenceEquals(objRefCountingDataReader, null) && !objRefCountingDataReader.IsClosed)
                {
                    objRefCountingDataReader.Close();
                    objRefCountingDataReader.Dispose();
                }

            }
            return lstP2PLineItemID;
        }

        public long GetLobByEntityDetailCode(long entitydetailcode)
        {
            var lobs = new List<long>();
            RefCountingDataReader objRefCountingDataReader = null;
            SqlDataReader sqlDr = null;
            try
            {
                objRefCountingDataReader = (RefCountingDataReader)ContextSqlConn.ExecuteReader(ReqSqlConstants.USP_P2P_REQ_GETENTITYDETAILSBYSEARCHRESULTS,
                                                                    new object[] { long.Equals(entitydetailcode, (Int64)0) ? DBNull.Value : (object)entitydetailcode });
                sqlDr = (SqlDataReader)objRefCountingDataReader.InnerReader;
                while (sqlDr.Read())
                {
                    var lobEntityDetailCode = GetLongValue(sqlDr, ReqSqlConstants.COL_LOBEntityDetailCode);
                    lobs.Add(lobEntityDetailCode);
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, string.Concat("Error occured in GetLobByEntityDetailCode DAO method for Entitydetailcode:", entitydetailcode.ToString()), ex);
                throw ex;
            }
            finally
            {
                if (!ReferenceEquals(objRefCountingDataReader, null) && !objRefCountingDataReader.IsClosed)
                {
                    objRefCountingDataReader.Close();
                    objRefCountingDataReader.Dispose();
                }
                LogHelper.LogInfo(Log, "GetLobByEntityDetailCode Method completed.");
            }
            return lobs.Any() ? lobs.FirstOrDefault(): 0;
        }
    }

}
