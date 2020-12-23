using Gep.Cumulus.CSM.Entities;
using Gep.Cumulus.Partner.Entities;
using GEP.Cumulus.Logging;
using GEP.Cumulus.P2P.BusinessEntities;
using GEP.Cumulus.P2P.BusinessObjects;
using GEP.Cumulus.P2P.BusinessObjects.Proxy;
using GEP.Cumulus.P2P.Req.BusinessObjects.Entities;
using GEP.Cumulus.P2P.Req.BusinessObjects.Proxy;
using GEP.Cumulus.P2P.Req.DataAccessObjects;
using GEP.Cumulus.SmartCatalog.BusinessEntities.NewCatalog;
using GEP.NewPlatformEntities;
using GEP.SMART.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ExcelLib = Aspose.Cells;
using FileManagerEntities = GEP.NewP2PEntities.FileManagerEntities;

namespace GEP.Cumulus.P2P.Req.BusinessObjects.FactoryClasses
{
    public class RequisitionTemplateUploader : RequisitionTemplateReader
    {
        private long documentCode = 0;
        private NewRequisitionManager RequisitionManger = null;
        public RequisitionResponse ReqResponse = null;
        private FileManagerEntities.ReqUploadFileLog ReqUploadLog = null;
        private int iCol_REQ_MSG = 0;
        private int iCol_ACCT_MSG = 0;
        private int iCol_TAX_MSG = 0;
        private UserExecutionContext userContext;
        private Dictionary<int, List<SplitAccountingFields>> AccountingDataList = new Dictionary<int, List<SplitAccountingFields>>();
        private Dictionary<int, List<SplitAccountingFields>> validAccountingDataList = new Dictionary<int, List<SplitAccountingFields>>();
        private FileManagerEntities.ReqTemplateFileResponse requistionTemplateRespone = null;
        private long oboUserContactCode = 0;
        private List<CalendarPeriod> lstPeriodData = null;
        private Requisition objReqDetails = null;
        private List<string> lstAvailableLines = new List<string>();
        private List<string> lstUserActivities;
        public const string Allow_Requestors_To_Edit_Catalogprice_And_Quantityrestrictions = "10700064";
        public const string ALLOW_BUYERS_TO_EDIT_CATALOG_PRICE_AND_QUANTITY_RESTRICTIONS = "10700065";
        protected long LOBEntityDetailCode = 0;
        protected long EntityDetailCode = 0;
        protected int EntityTypeId = 0;
        protected RequisitionCommonManager commonManager = null;
        private DataSet partnerDocInterfaceDic = new DataSet();
        private Dictionary<long, int> partnerInterfaceDictionary = new Dictionary<long, int>();
        private Dictionary<long, List<ContactInfo>> partnerContactDictionary = new Dictionary<long, List<ContactInfo>>();
        private Dictionary<long, List<ContactInfo>> partnerContactDict = new Dictionary<long, List<ContactInfo>>();
        List<CatalogItemDetails> lstCatalogItemDetails = new List<CatalogItemDetails>();
        List<ResponseItem> responseItemsList = new List<ResponseItem>();
        Newtonsoft.Json.Linq.JToken[] ContractData;
        private Dictionary<long, string> taxJurisdictionDictionary = new Dictionary<long, string>();
        List<PartnerCurrencies> partnerCurrencies = new List<PartnerCurrencies>();
        List<CurrencyExchageRate> lstCurrencyExchageRate = new List<CurrencyExchageRate>();
        private IRequisitionDAO GetReqDao()
        {
            return DataAccessObjects.ReqDAOManager.GetDAO<IRequisitionDAO>(UserContext, GepConfiguration, false);
        }

        public RequisitionTemplateUploader(UserExecutionContext userContext, Int64 documentCode, string documentNumber, NewRequisitionManager ReqManger, string fileId, FileManagerEntities.ReqTemplateFileResponse reqTemplateResponse, FileManagerEntities.ReqUploadFileLog requisitionUploadLog, string jwtToken)
: base(userContext, documentCode, documentNumber, ReqManger, fileId, jwtToken)
        {

            this.userContext = userContext;
            {
                UserContext = this.userContext;
                GepConfiguration = ReqManger.GepConfiguration;
            }
            this.documentCode = documentCode;
            this.requistionTemplateRespone = reqTemplateResponse;
            this.requistionTemplateRespone.FileId = fileId;
            this.ReqUploadLog = requisitionUploadLog;


            RequisitionManger = ReqManger;
            LoadSettings();

            GetRequisitionDetails();

            LoadValidationMasterData();

            ValidateAllLineItems();

            //SaveLineItems();
            SaveBulkLineItems();

            GenerateErrorResultFile();

        }

        private void ValidateAllLineItems()
        {
            if (EnableGetLineItemsBulkAPI)
            {
                GetCatalogItemsData();
            }
            if (EnableContractBulkAPI && IsValidateContractNumber)
            {
                GetContractRelatedData();
            }
            if (AllowMultiCurrencyInRequisition)
            {
                GetGetPartnerCurrencies();
            }
            List<string> lstLineNumbersToDelete = new List<string>();
            ReqResponse = new RequisitionResponse();
            ReqResponse.lstRequisitionItemErrorResponse = new List<RequisitionItemErrorResponse>();
            //---tax code implementation for line level
            bool TaxValidate = Taxcodelinelevelchecking();
            GetTransmissionModeBasedOnSetting(lstRequisitionItems);
            if (ShowTaxJurisdictionForShipTo)
                GetTaxJurisdictionBasedOnShipTo();
            foreach (var item in lstRequisitionItems)
            {
                if (item.OperationType == RequisitionOperation.Delete)
                {
                    lstLineNumbersToDelete.Add(item.ItemLineNumber.ToString());
                    // Do not validate in case of delete item
                }
                else
                {
                    RequisitionItemErrorResponse requisitionItemErrorResponse = new RequisitionItemErrorResponse();
                    requisitionItemErrorResponse.LineNumber = string.Empty;
                    requisitionItemErrorResponse.FieldMapping = new List<RequisitionExcelItemColumn>();
                    requisitionItemErrorResponse.SingleSplitAccountingErrors = new List<RequisitionSplitItemAccountingErrors>();
                    requisitionItemErrorResponse.RequisitionMultipleItemSplitMapping = new List<RequisitionItemSplitErrorResponse>();
                    //Validate Line Item
                    Validate(item, requisitionItemErrorResponse);
                    if (item.IsValid)
                    {
                        if (item.OperationType == RequisitionOperation.Update)
                        {
                            //Get the Tax Details for Item to refresh the split
                            if (objReqDetails != null)
                            {
                                if (objReqDetails.RequisitionItems != null && objReqDetails.RequisitionItems.Count() > 0)
                                {
                                    var objItemList = objReqDetails.RequisitionItems.Where(x => x.ItemLineNumber == item.ItemLineNumber);
                                    if (objItemList != null && objItemList.Count() > 0)
                                    {
                                        var objItem = objItemList.FirstOrDefault();
                                        item.DocumentItemId = objItemList.FirstOrDefault().DocumentItemId;
                                        item.Tax = objItemList.FirstOrDefault().Tax;
                                    }
                                }
                            }
                        }
                        //calculte item level tax based on ship to 
                        if (TaxValidate)
                        {
                            item.Tax = 0;
                            if (item.Taxes.Count == 0 && !item.IsTaxExempt)
                                GetTaxCodeRelatedToShiptTO(item, objReqDetails, EntityDetailCode, EntityTypeId);
                            else
                            {
                                List<Taxes> objtax = new List<Taxes>();
                                decimal totalTaxPercentage = 0;
                                if (item.Taxes.Count > 0)
                                {
                                    for (int i = 0; i < item.Taxes.Count; i++)
                                    {
                                        totalTaxPercentage = totalTaxPercentage + Convert.ToDecimal(item.Taxes[i].TaxPercentage);
                                        objtax.Add(new Taxes { TaxCode = item.Taxes[i].TaxCode, TaxPercentage = item.Taxes[i].TaxPercentage, TaxValue = item.Taxes[i].TaxValue, TaxDescription = item.Taxes[i].TaxDescription, TaxId = item.Taxes[i].TaxId });
                                    }
                                    item.Tax = ((totalTaxPercentage * item.Quantity * ((item.UnitPrice))) / 100);
                                    item.Tax = Math.Round((decimal)(item.Tax), maxPrecessionForTaxesAndCharges);
                                    item.taxItems = objtax;
                                }
                            }
                        }
                        //Calculate the Split Totals
                        CalculateSplitItemTotal(item);
                    }
                }
            }
            //Delete Line Items
            if (lstLineNumbersToDelete.Count > 0)
                RequisitionManger.DeleteLineItems(documentCode, String.Join(",", lstLineNumbersToDelete.ToArray()));
        }
        private void GetContractRelatedData()
        {
            try
            {
                List<string> ContractList = lstRequisitionItems.Where(reqitem => !string.IsNullOrEmpty(reqitem.ContractNo)).Select(reqitem => reqitem.ContractNo).Distinct().ToList();
                ContractData = GetContractData(ContractList);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error Occured in GetContractRelatedData method", ex);
                throw ex;
            }
        }
        private void GetGetPartnerCurrencies()
        {
            try
            {

                long PartnerCode = 0;
                List<long> lstPartners = new List<long>();
                foreach (var item in lstRequisitionItems)
                {
                    if (lstSuppliers != null && lstSuppliers.Count() > 0 && !string.IsNullOrEmpty(item.PartnerName))
                    {
                        var supplierList = lstSuppliers.Where(x => x.Name.Equals(item.PartnerName.Trim()) || x.Value.Equals(item.PartnerName.Trim()));
                        if (supplierList != null && supplierList.Any())
                            PartnerCode = (supplierList != null) ? supplierList.FirstOrDefault().Id : 0;
                        lstPartners.Add(PartnerCode);
                    }
                }
                if (lstPartners != null && lstPartners.Count > 0)
                {
                    var Partnercodes = String.Join(",", lstPartners.Distinct());
                    partnerCurrencies = GetPartnerCurrencies(Partnercodes);
                }

            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error Occured in GetGetPartnerCurrencies method", ex);
                throw ex;
            }
        }

        private void GetCatalogItemsData()
        {
            long PartnerCode = 0;
            string Currency = "";
            string UOM = "";
            try
            {

                ItemSearchBulkInputRequest itemSearchBulkInputRequest = new ItemSearchBulkInputRequest();
                List<ItemInput> itemInput = new List<ItemInput>();
                List<OrgEntity> orgEntities = new List<OrgEntity>();
                if (lsDocumentHeaderEntitites != null && lsDocumentHeaderEntitites.Count > 0)
                {
                    foreach (var addEntity in lsDocumentHeaderEntitites)
                        orgEntities.Add(new OrgEntity { EntityDetailCode = addEntity.EntityDetailCode });
                }

                foreach (var item in lstRequisitionItems)
                {
                    bool IsSupplierItemNumberFieldTypeFreeText = false;
                    if (!string.IsNullOrEmpty(item.SupplierItemNumber) && string.IsNullOrEmpty(item.ItemNumber) && SupplierItemNumberFieldType == "0")
                    {
                        IsSupplierItemNumberFieldTypeFreeText = true;
                    }

                    if (((item.ItemNumber != null && item.ItemNumber.Length > 0) || (item.SupplierItemNumber != null && item.SupplierItemNumber.Length > 0)) && !IsSupplierItemNumberFieldTypeFreeText)
                    {

                        if (lstSuppliers != null && lstSuppliers.Count() > 0 && !string.IsNullOrEmpty(item.PartnerName))
                        {
                            var supplierList = lstSuppliers.Where(x => x.Name.Equals(item.PartnerName.Trim()) || x.Value.Equals(item.PartnerName.Trim()));
                            if (supplierList != null && supplierList.Any())
                                PartnerCode = (supplierList != null) ? supplierList.FirstOrDefault().Id : 0;
                        }

                        if (string.IsNullOrEmpty(item.Currency) && objReqDetails != null)
                        {
                            item.Currency = objReqDetails.Currency;
                        }
                        else
                        {
                            var currencyList = lstCurrency.Where(x => x.Name.Equals(item.Currency.Trim()) || x.Value.Equals(item.Currency.Trim()));
                            if (currencyList != null && currencyList.Any())
                                Currency = (currencyList != null) ? currencyList.FirstOrDefault().Value : "USD";
                        }
                        if (lstUOMs != null && lstUOMs.Count() > 0 && !string.IsNullOrEmpty(item.UOM))
                        {
                            var uomList = lstUOMs.Where(x => x.Name.Equals(item.UOM.Trim()) || x.Value.Equals(item.UOM.Trim()));
                            if (uomList != null && uomList.Any())
                                UOM = (uomList != null) ? uomList.FirstOrDefault().Value : "EA";
                        }
                        DateTime? CurrentDate = null;
                        if (item.ItemType == ItemType.Material)
                            CurrentDate = item.DateNeeded;
                        else if (item.ItemType == ItemType.Service)
                            CurrentDate = item.StartDate;

                        itemInput.Add(new ItemInput
                        {
                            BIN = item.ItemNumber,
                            SIN = item.SupplierItemNumber,
                            SupplierCode = PartnerCode,
                            AccessEntities = orgEntities,
                            ContactCode = userContext.ContactCode,
                            CurrencyCode = Currency,
                            UOM = UOM,
                            Quantity = item.Quantity,
                            ApplicableDate = CurrentDate
                        }
                        );
                    }

                }


                itemSearchBulkInputRequest.ItemInputList = itemInput;
                itemSearchBulkInputRequest.IsIdSearch = false;
                if (itemInput != null && itemInput.Count > 0)
                {
                    SearchResult searchResultData = AccessSearchItems(itemSearchBulkInputRequest);
                    if (searchResultData != null && searchResultData.ResponseItems != null && searchResultData.ResponseItems.Count > 0)
                    {
                        responseItemsList = searchResultData.ResponseItems;
                    }
                }


            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error Occured in GetCatalogItemsData method", ex);
                throw ex;
            }
        }
        public void SaveBulkLineItems()
        {
            var validReqItems = lstRequisitionItems.Where(x => x.IsValid == true);
            if (validReqItems != null && validReqItems.Count() > 0)
            {
                bool status = RequisitionManger.SaveBulkRequisitionItems(documentCode, validReqItems.ToList<RequisitionItem>(), precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges, (objReqDetails != null ? objReqDetails.PurchaseType : 0));
                if (!status)
                    requistionTemplateRespone.Status = "Failed";
                else
                {
                    var currentPrincipal = System.Threading.Thread.CurrentPrincipal;
                    Task.Factory.StartNew((userContext) =>
                    {
                        System.Threading.Thread.CurrentPrincipal = currentPrincipal;
                        AddIntoSearchIndexerQueueing(documentCode, 7, false);
                    }, this.userContext);
                }
            }
        }

        public void SaveLineItems()
        {
            if (lstRequisitionItems.Count() > 0)
            {
                List<string> lstLineNumbersToDelete = new List<string>();
                long ReqLineItemShippingID = 0;
                //Error Logging
                ReqResponse = new RequisitionResponse();
                ReqResponse.lstRequisitionItemErrorResponse = new List<RequisitionItemErrorResponse>();

                foreach (var item in lstRequisitionItems)
                {
                    ReqLineItemShippingID = 0;
                    if (item.OperationType == RequisitionOperation.Delete)
                    {
                        lstLineNumbersToDelete.Add(item.ItemLineNumber.ToString());
                        // Do not validate in case of delete item

                    }
                    else if (item.OperationType == RequisitionOperation.Update || item.OperationType == RequisitionOperation.Add)
                    {
                        bool canSave = true;
                        RequisitionItemErrorResponse requisitionItemErrorResponse = new RequisitionItemErrorResponse();
                        requisitionItemErrorResponse.LineNumber = string.Empty;
                        requisitionItemErrorResponse.FieldMapping = new List<RequisitionExcelItemColumn>();
                        requisitionItemErrorResponse.SingleSplitAccountingErrors = new List<RequisitionSplitItemAccountingErrors>();
                        requisitionItemErrorResponse.RequisitionMultipleItemSplitMapping = new List<RequisitionItemSplitErrorResponse>();

                        canSave = Validate(item, requisitionItemErrorResponse);
                        if (canSave)
                        {
                            if (item.OperationType == RequisitionOperation.Update)
                            {
                                //Get the DocuementItemId
                                if (objReqDetails != null)
                                {
                                    if (objReqDetails.RequisitionItems != null && objReqDetails.RequisitionItems.Count() > 0)
                                    {
                                        var objItemList = objReqDetails.RequisitionItems.Where(x => x.ItemLineNumber == item.ItemLineNumber);
                                        if (objItemList != null && objItemList.Count() > 0)
                                        {
                                            var objItem = objItemList.FirstOrDefault();
                                            item.DocumentItemId = objItemList.FirstOrDefault().DocumentItemId;
                                            item.Tax = objItemList.FirstOrDefault().Tax;
                                            if (objItem.DocumentItemShippingDetails != null && objItem.DocumentItemShippingDetails.Count() > 0)
                                            {
                                                ReqLineItemShippingID = objItem.DocumentItemShippingDetails.FirstOrDefault().DocumentItemShippingId;
                                                if (ReqLineItemShippingID < 0)
                                                    ReqLineItemShippingID = 0;
                                            }

                                        }
                                    }
                                }
                                // First delete the existing line item from database    
                            }
                            item.DocumentId = documentCode;

                            item.DocumentItemId = objDocBO.SaveItem(P2PDocumentType.Requisition, item, true, false, false, false);

                            if (item.DocumentItemId > 0)
                            {
                                #region Partner Details
                                objDocBO.SaveItemPartnerDetails(P2PDocumentType.Requisition, item, precessionValue);
                                #endregion

                                #region Item Shipping Details
                                if (item.DocumentItemShippingDetails != null && item.DocumentItemShippingDetails.Count > 0)
                                {
                                    foreach (var shippingDetail in item.DocumentItemShippingDetails)
                                    {
                                        shippingDetail.DocumentItemId = item.DocumentItemId;
                                        shippingDetail.Quantity = item.Quantity;
                                        shippingDetail.ShiptoLocation.ShiptoLocationId = item.ShipToLocationId;

                                        objDocBO.SaveItemShippingDetails(P2PDocumentType.Requisition,
                                                          ReqLineItemShippingID,
                                                          shippingDetail.DocumentItemId,
                                                          string.Empty,
                                                          shippingDetail.ShiptoLocation.ShiptoLocationId,
                                                          0,
                                                          shippingDetail.Quantity,
                                                          item.Quantity,
                                                          UserContext.ContactCode,
                                                          string.Empty,
                                                          precessionValue,
                                                          maxPrecessionforTotal,
                                                          maxPrecessionForTaxesAndCharges,
                                                          false
                                                          );

                                    }
                                }

                                #endregion

                                #region Item Additional Details
                                GetReqDao().SaveItemAdditionDetails(item, precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges);
                                #endregion

                                #region Update Contract No
                                string contractNumber = (item.ContractNo == null) ? item.BlanketNumber : item.ContractNo;
                                if (!String.IsNullOrEmpty(contractNumber))
                                    objReqManager.SaveContractInformation(item.DocumentItemId, contractNumber);
                                #endregion

                                // Save the item data
                                CalculateSplitItemTotal(item);

                                #region Accounting Details
                                if (item.ItemSplitsDetail != null && item.ItemSplitsDetail.Count > 0)
                                {
                                    //Delete Exisiting Split Item
                                    GetReqDao().DeleteSplitItemsByItemId(item.DocumentItemId);

                                    List<DocumentSplitItemEntity> DocumentSplitItemEntities = new List<DocumentSplitItemEntity>();
                                    foreach (RequisitionSplitItems itmSplt in item.ItemSplitsDetail)
                                    {
                                        DocumentSplitItemEntities.AddRange(itmSplt.DocumentSplitItemEntities);
                                        item.ItemSplitsDetail.ForEach(itm => itm.DocumentItemId = item.DocumentItemId);
                                        GetReqDao().SaveRequisitionAccountingDetails(new List<RequisitionSplitItems>() { itmSplt }, itmSplt.DocumentSplitItemEntities, item.Quantity, precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges, false);
                                    }
                                }
                                #endregion

                                #region Item Other Details
                                objDocBO.SaveItemOtherDetails(P2PDocumentType.Requisition, item, false, "");

                                #endregion
                            }

                            // Insert the line item in the database (both update and save will do so

                            //Update Period By Need By Date
                            if (item.DocumentItemId > 0 && IsPeriodbasedonNeedbyDate)
                            {
                                UpdatePeriodbyNeedbyDate(item.DocumentItemId);
                            }

                        }
                        else
                        {
                            // Don't save the item data into database, place errors in the separate process
                        }
                    }
                }
                if (lstLineNumbersToDelete.Count > 0)
                    RequisitionManger.DeleteLineItems(documentCode, String.Join(",", lstLineNumbersToDelete.ToArray()));

            }
        }
        private bool Validate(RequisitionItem item, RequisitionItemErrorResponse requisitionItemErrorResponse)
        {
            var ItemValidationResult = ValidateLineItem(item, requisitionItemErrorResponse);
            var SplitAccountingValidationResult = ValidateItemSplitAccountingDetails(item, requisitionItemErrorResponse);
            var TaxValidationResult = ValidateTaxCodes(item, requisitionItemErrorResponse);
            if (ItemValidationResult && SplitAccountingValidationResult)
            {
                item.IsValid = true;
                if (TaxValidationResult)
                {
                    requisitionItemErrorResponse.LineNumber = item.LineReferenceNumber;
                    ReqResponse.lstRequisitionItemErrorResponse.Add(requisitionItemErrorResponse);
                }
                return true;
            }
            else
            {
                requisitionItemErrorResponse.LineNumber = item.LineReferenceNumber;
                ReqResponse.lstRequisitionItemErrorResponse.Add(requisitionItemErrorResponse);
                return false;
            }
        }

        private bool ValidateLineItem(RequisitionItem item, RequisitionItemErrorResponse requisitionItemErrorResponse)
        {
            bool result = true;
            bool IsOrderingLocationFilled = false;
            bool hasPrecisionError = false;
            StringBuilder errorList = new StringBuilder();
            if (item != null)
            {
                if (item.OperationType == RequisitionOperation.None)
                {
                    errorList.AppendLine("Operation is Mandatory");
                    requisitionItemErrorResponse.FieldMapping.Add(RequisitionExcelItemColumn.Operation);
                    result = false;
                }
                if (item.ItemLineNumber <= 0)
                {
                    errorList.AppendLine("Line Reference Number is required. Enter the valid line reference number");
                    requisitionItemErrorResponse.FieldMapping.Add(RequisitionExcelItemColumn.LineReferenceNumber);
                    result = false;
                }
                //Line Number Existence Valdiation
                if (item.OperationType == RequisitionOperation.Add && item.ItemLineNumber > 0)
                {
                    if (ValidateLineNumberExisitence(item.ItemLineNumber))
                    {
                        //Line Number 1 always exists in Requsiition as we save the Requisition document before uploading excel 
                        //so we need to delete exisiting line number 1 and insert new one through excel                    
                        Boolean isNewItem = false;
                        if (objReqDetails != null)
                        {
                            if (objReqDetails.RequisitionItems != null && objReqDetails.RequisitionItems.Count() == 1)
                            {
                                var objItem = objReqDetails.RequisitionItems.Where(x => x.ItemLineNumber == item.ItemLineNumber).FirstOrDefault();
                                if (objItem != null)
                                {
                                    if (objItem.Description == "" && objItem.SupplierPartId == "")
                                    {
                                        isNewItem = true;
                                    }
                                }
                            }
                        }
                        if (item.ItemLineNumber == 1 && isNewItem)
                        {
                            item.OperationType = RequisitionOperation.Update;
                        }
                        else
                        {
                            errorList.AppendLine("Line Number already exists.");
                            requisitionItemErrorResponse.FieldMapping.Add(RequisitionExcelItemColumn.LineReferenceNumber);
                            result = false;
                        }
                    }
                }
                if (item.OperationType == RequisitionOperation.Update && item.ItemLineNumber > 0)
                {
                    if (!ValidateLineNumberExisitence(item.ItemLineNumber))
                    {
                        errorList.AppendLine("Line Number does not exists.Please add the line in Add Mode");
                        requisitionItemErrorResponse.FieldMapping.Add(RequisitionExcelItemColumn.LineReferenceNumber);
                        result = false;
                    }
                }
                if (item.ItemType == ItemType.None)
                {
                    errorList.AppendLine("Item Type is Mandatory");
                    requisitionItemErrorResponse.FieldMapping.Add(RequisitionExcelItemColumn.ItemType);
                    result = false;
                }

                if (EnableContractBulkAPI && (!string.IsNullOrEmpty(item.ContractNo) || !string.IsNullOrEmpty(item.BlanketNumber)) && IsValidateContractNumber)
                {
                    string contractNumber = (item.ContractNo == null) ? item.BlanketNumber : item.ContractNo;
                    if (!string.IsNullOrEmpty(item.ContractNo))
                    {
                        bool isValidContract = false;
                        foreach (var value in ContractData)
                        {
                            if (Convert.ToString(value["DocumentSearchOutput"]["DocumentNumber"]).Equals(contractNumber, StringComparison.InvariantCultureIgnoreCase))
                            {
                                isValidContract = true;
                            }
                        }

                        if (!isValidContract)
                        {
                            requisitionItemErrorResponse.FieldMapping.Add(RequisitionExcelItemColumn.ContractNumber);
                            errorList.AppendLine("Blanket/Contract Number entered is not valid. Enter valid blanket/contract number.");
                            result = false;
                        }
                    }
                    else if (!string.IsNullOrEmpty(item.BlanketNumber))
                    {
                        bool isValid = RequisitionManger.ValidateContractNumber(contractNumber);
                        if (!isValid)
                        {
                            requisitionItemErrorResponse.FieldMapping.Add(RequisitionExcelItemColumn.BlanketNumber);
                            errorList.AppendLine("Blanket/Contract Number entered is not valid. Enter valid blanket/contract number.");
                            result = false;
                        }
                    }

                }
                // Validate Contract Number
                if (!EnableContractBulkAPI && (!string.IsNullOrEmpty(item.ContractNo) || !string.IsNullOrEmpty(item.BlanketNumber)) && IsValidateContractNumber)
                {
                    string contractNumber = (item.ContractNo == null) ? item.BlanketNumber : item.ContractNo;
                    bool isValid = RequisitionManger.ValidateContractNumber(contractNumber);
                    if (!isValid)
                    {
                        if (item.ContractNo == null)
                            requisitionItemErrorResponse.FieldMapping.Add(RequisitionExcelItemColumn.BlanketNumber);
                        else
                            requisitionItemErrorResponse.FieldMapping.Add(RequisitionExcelItemColumn.ContractNumber);
                        errorList.AppendLine("Blanket/Contract Number entered is not valid. Enter valid blanket/contract number.");
                        result = false;
                    }

                }

                // Validate Blanket Number
                if (string.IsNullOrEmpty(item.ContractNo) && string.IsNullOrEmpty(item.BlanketNumber) && !string.IsNullOrEmpty(item.ContractItemId.ToString()) && item.ContractItemId > 0)
                {
                    requisitionItemErrorResponse.FieldMapping.Add(RequisitionExcelItemColumn.BlanketNumber);
                    errorList.AppendLine("Contract/Blanket number is required when you enter the blanket line reference number.");
                    result = false;
                }

                // Validate Blanket Line Number
                if ((!string.IsNullOrEmpty(item.BlanketNumber) || !string.IsNullOrEmpty(item.ContractNo)) && !string.IsNullOrEmpty(item.ContractItemId.ToString()) && item.ContractItemId > 0)
                {
                    string contractNumber = (item.ContractNo == null) ? item.BlanketNumber : item.ContractNo;
                    bool isValid = RequisitionManger.ValidateContractItemId(contractNumber, item.ContractItemId);
                    if (!isValid)
                    {
                        requisitionItemErrorResponse.FieldMapping.Add(RequisitionExcelItemColumn.ContractItemLineRef);
                        errorList.AppendLine("Contract Line Number Reference entered is not valid.");
                        result = false;
                    }
                }

                if (item.Quantity == 0 && item.ItemExtendedType != ItemExtendedType.Fixed)
                {
                    errorList.AppendLine("Quantity cannot be 0");
                    requisitionItemErrorResponse.FieldMapping.Add(RequisitionExcelItemColumn.QuantityEffort);
                    result = false;
                }
                //Unit Price
                //if (!item.UnitPrice.HasValue)
                //{
                //    errorList.AppendLine("Unit Price is mandatory");
                //    requisitionItemErrorResponse.FieldMapping.Add(RequisitionExcelItemColumn.UnitPrice);
                //    result = false;
                //}
                //if (item.UnitPrice.HasValue && item.UnitPrice.Value == 0)
                //{
                //    errorList.AppendLine("Unit Price should be greater than 0");
                //    requisitionItemErrorResponse.FieldMapping.Add(RequisitionExcelItemColumn.UnitPrice);
                //    result = false;
                //}

                if (item.ItemType == ItemType.Service)
                {
                    if (item.ItemExtendedType == ItemExtendedType.Fixed)
                    {
                        if (item.Quantity > 1)
                        {
                            errorList.AppendLine("Fixed Service item Quantity should be 1");
                            requisitionItemErrorResponse.FieldMapping.Add(RequisitionExcelItemColumn.QuantityEffort);
                            result = false;
                        }
                    }
                    if (item.ShippingCharges.HasValue && item.ShippingCharges.Value > 0)
                    {
                        errorList.AppendLine("Service line item cannot have shipping charges");
                        requisitionItemErrorResponse.FieldMapping.Add(RequisitionExcelItemColumn.ShippingAndFreight);
                        result = false;
                    }
                    if (!item.StartDate.HasValue)
                    {
                        errorList.AppendLine("Service Start Date  is a Mandatory");
                        requisitionItemErrorResponse.FieldMapping.Add(RequisitionExcelItemColumn.ServiceStartDate);
                        result = false;
                    }
                    else if (item.StartDate == DateTime.MinValue)
                    {
                        errorList.AppendLine("Invalid service start date");
                        requisitionItemErrorResponse.FieldMapping.Add(RequisitionExcelItemColumn.ServiceStartDate);
                        result = false;

                    }

                    if (!item.EndDate.HasValue)
                    {
                        errorList.AppendLine("Service End Date  is a Mandatory");
                        requisitionItemErrorResponse.FieldMapping.Add(RequisitionExcelItemColumn.ServiceEndDate);
                        result = false;
                    }
                    else if (item.EndDate == DateTime.MinValue)
                    {
                        errorList.AppendLine("Invalid service end date");
                        requisitionItemErrorResponse.FieldMapping.Add(RequisitionExcelItemColumn.ServiceEndDate);
                        result = false;
                    }

                    if (item.StartDate.HasValue && item.EndDate.HasValue && item.StartDate.Value > item.EndDate.Value)
                    {
                        errorList.AppendLine("Service Start date cannot be greater than Service end date");
                        requisitionItemErrorResponse.FieldMapping.Add(RequisitionExcelItemColumn.ServiceEndDate);
                        result = false;
                    }
                }

                //Need By Date Auto Pupulation
                if (item.DateNeeded == null)
                {
                    if (populateDefaultNeedByDate.ToUpper().Equals("TRUE"))
                    {
                        item.DateNeeded = DateTime.Now.AddDays(populateDefaultNeedByDateByDays);
                    }
                }
                else if (item.DateNeeded == DateTime.MinValue)
                {
                    errorList.AppendLine("Invalid Need by Date");
                    requisitionItemErrorResponse.FieldMapping.Add(RequisitionExcelItemColumn.NeedByDate);
                    result = false;
                }
                else
                {
                    if (item.DateNeeded < DateTime.Today)
                    {
                        errorList.AppendLine("Need By Date Must be greater than or equal to Current Date");
                        requisitionItemErrorResponse.FieldMapping.Add(RequisitionExcelItemColumn.NeedByDate);
                        result = false;
                    }
                }


                //Ship to Location Validation
                //if (lstShipToLocations != null && lstShipToLocations.Count() > 0)
                //{
                ShiptoLocation objShipTo = new ShiptoLocation();
                if (item.DocumentItemShippingDetails != null && item.DocumentItemShippingDetails.Count() > 0)
                    objShipTo = item.DocumentItemShippingDetails[0].ShiptoLocation;
                if (objShipTo != null)
                {
                    if (!string.IsNullOrEmpty(objShipTo.ShiptoLocationName))
                    {
                        if (lstShipToLocations != null && lstShipToLocations.Count() > 0)
                        {
                            var shipToLocationList = lstShipToLocations.Where(x => x.Name.Equals(objShipTo.ShiptoLocationName.Trim()) || x.Value.Equals(objShipTo.ShiptoLocationName.Trim()));
                            if (shipToLocationList != null && !shipToLocationList.Any())
                            {
                                errorList.AppendLine("Invalid Ship to Location");
                                requisitionItemErrorResponse.FieldMapping.Add(RequisitionExcelItemColumn.ShipToLocation);
                                result = false;
                            }
                            else
                            {
                                item.ShipToLocationId = (shipToLocationList != null) ? Convert.ToInt32(shipToLocationList.FirstOrDefault().Value) : 0;
                            }
                        }
                    }
                    else
                    {
                        //If Ship to Location is not defined. Set Header LEvel Ship To Location to the Line
                        if (objReqDetails != null && objReqDetails.ShiptoLocation != null)
                        {
                            item.ShipToLocationId = objReqDetails.ShiptoLocation.ShiptoLocationId;
                        }
                    }
                }
                //}
                if (item.ShipToLocationId > 0 && ShowTaxJurisdictionForShipTo)
                    setTaxJurisdictionBasedOnShipTo(ref item);

                if (lstShippingMethods != null && lstShippingMethods.Count() > 0)
                {
                    item.ShippingMethod = lstShippingMethods.FirstOrDefault();
                }


                //UOM Validation
                if (lstUOMs != null && lstUOMs.Count() > 0)
                {
                    if (string.IsNullOrEmpty(item.UOM))
                    {
                        errorList.AppendLine("Unit of Measure is Mandatory.");
                        requisitionItemErrorResponse.FieldMapping.Add(RequisitionExcelItemColumn.UnitofMeasure);
                        result = false;
                    }
                    else
                    {
                        var uomList = lstUOMs.Where(x => x.Name.Equals(item.UOM.Trim()) || x.Value.Equals(item.UOM.Trim()));
                        if (uomList != null && !uomList.Any())
                        {
                            errorList.AppendLine("Invalid Unit of Measure");
                            requisitionItemErrorResponse.FieldMapping.Add(RequisitionExcelItemColumn.UnitofMeasure);
                            result = false;
                        }
                        else
                        {
                            item.UOM = (uomList != null) ? uomList.FirstOrDefault().Value : "EA";
                        }
                    }
                }
                //Currency Validation

                if (!AllowMultiCurrencyInRequisition)
                {
                    if (string.IsNullOrEmpty(item.Currency) && objReqDetails != null)
                    {
                        item.Currency = objReqDetails.Currency;
                    }
                    else
                    {
                        if (objReqDetails != null)
                        {
                            if (!item.Currency.Trim().Equals(objReqDetails.Currency))
                            {
                                errorList.AppendLine("Line Item Currency must be same as Requisition Currency");
                                requisitionItemErrorResponse.FieldMapping.Add(RequisitionExcelItemColumn.Currency);
                                result = false;
                            }
                        }
                        else
                        {
                            if (lstCurrency != null && lstCurrency.Count() > 0)
                            {
                                var currencyList = lstCurrency.Where(x => x.Name.Equals(item.Currency.Trim()) || x.Value.Equals(item.Currency.Trim()));
                                if (currencyList != null && !currencyList.Any())
                                {
                                    errorList.AppendLine("Invalid Currency");
                                    requisitionItemErrorResponse.FieldMapping.Add(RequisitionExcelItemColumn.Currency);
                                    result = false;
                                }
                            }
                        }
                    }
                }

                if (AllowMultiCurrencyInRequisition)
                {
                    bool isValidCurrencyForItem = true;
                    if (string.IsNullOrEmpty(item.Currency) && objReqDetails != null)
                    {
                        errorList.AppendLine("Currency is Mandatory");
                        requisitionItemErrorResponse.FieldMapping.Add(RequisitionExcelItemColumn.Currency);
                        result = false;
                        isValidCurrencyForItem = false;
                    }
                    else if (lstCurrency != null && lstCurrency.Count() > 0)
                    {
                        var currencyList = lstCurrency.Where(x => x.Name.Equals(item.Currency.Trim()) || x.Value.Equals(item.Currency.Trim()));
                        if (currencyList != null && !currencyList.Any())
                        {
                            errorList.AppendLine("Invalid Currency");
                            requisitionItemErrorResponse.FieldMapping.Add(RequisitionExcelItemColumn.Currency);
                            result = false;
                            isValidCurrencyForItem = false;
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(item.PartnerName) && lstSuppliers != null && lstSuppliers.Count > 0 && partnerCurrencies != null && partnerCurrencies.Count > 0)
                            {
                                var supplierList = lstSuppliers.Where(x => x.Name.Equals(item.PartnerName.Trim()) || x.Value.Equals(item.PartnerName.Trim()));
                                if (supplierList != null && supplierList.Any())
                                {
                                    item.PartnerCode = (supplierList != null) ? supplierList.FirstOrDefault().Id : 0;
                                    var partnersList = partnerCurrencies.Where(x => x.PartnerCode == item.PartnerCode).ToList();
                                    if (partnersList != null && partnersList.Any())
                                    {
                                        var lstSupplierCurrencies = partnersList.SelectMany(x => x.Currency.Where(y => y.CurrencyCode == item.Currency).ToList());
                                        var partnerCurrencyExist = partnersList.SelectMany(x => x.Currency).ToList();
                                        if (partnerCurrencyExist != null && partnerCurrencyExist.Any() && lstSupplierCurrencies != null && !lstSupplierCurrencies.Any())
                                        {
                                            errorList.AppendLine("Please Enter Supplier Supported Currency");
                                            requisitionItemErrorResponse.FieldMapping.Add(RequisitionExcelItemColumn.Currency);
                                            result = false;
                                            isValidCurrencyForItem = false;
                                        }
                                    }
                                }
                            }

                        }
                    }

                    if (isValidCurrencyForItem)
                    {
                        var currencyList = lstCurrency.Where(x => x.Name.Equals(item.Currency.Trim()) || x.Value.Equals(item.Currency.Trim()));
                        if (currencyList != null && currencyList.Count() > 0 && objReqDetails != null && objReqDetails.Currency != null)
                        {
                            var lstCurrencyConversionFactor = lstCurrencyExchageRate.Where(x => x.FromCurrencyCode == item.Currency && x.ToCurrencyCode == objReqDetails.Currency);
                            if (lstCurrencyConversionFactor != null && lstCurrencyConversionFactor.Count() > 0)
                            {
                                item.ConversionFactor = lstCurrencyConversionFactor.FirstOrDefault().ExchangeRate;
                            }
                            else
                            {
                                item.Currency = currencyList.FirstOrDefault().Value;
                                commonManager = new RequisitionCommonManager(this.JWTToken) { UserContext = this.userContext, GepConfiguration = RequisitionManger.GepConfiguration };
                                decimal ExchangeRate = commonManager.GetCurrencyConversionRate(item.Currency, objReqDetails.Currency);
                                item.ConversionFactor = ExchangeRate;
                                CurrencyExchageRate currencyExchageRate = new CurrencyExchageRate();
                                currencyExchageRate.FromCurrencyCode = item.Currency;
                                currencyExchageRate.ToCurrencyCode = objReqDetails.Currency;
                                currencyExchageRate.ExchangeRate = ExchangeRate;
                                lstCurrencyExchageRate.Add(currencyExchageRate);
                            }
                        }
                    }

                }

                //Supplier Validation
                if (lstSuppliers != null && lstSuppliers.Count() > 0)
                {
                    if (!string.IsNullOrEmpty(item.PartnerName))
                    {
                        var supplierList = lstSuppliers.Where(x => x.Name.Equals(item.PartnerName.Trim()) || x.Value.Equals(item.PartnerName.Trim()));
                        if (supplierList != null && !supplierList.Any())
                        {
                            errorList.AppendLine("Supplier client code/name entered is not valid. Enter valid supplier details.");
                            requisitionItemErrorResponse.FieldMapping.Add(RequisitionExcelItemColumn.Supplier);
                            result = false;
                        }
                        else
                        {
                            item.PartnerCode = (supplierList != null) ? supplierList.FirstOrDefault().Id : 0;
                        }
                    }
                }

                //Supplier Ordering Location Validation
                if (lstSuppliersOrderLocations != null && lstSuppliersOrderLocations.Count() > 0)
                {
                    if (!string.IsNullOrEmpty(item.OrderLocationName) && item.PartnerCode > 0)
                    {
                        var orderLocList = lstSuppliersOrderLocations.Where(x => (x.LocationName.Equals(item.OrderLocationName.Trim()) || x.ClientLocationCode.Equals(item.OrderLocationName.Trim())) && x.PartnerCode == (long)item.PartnerCode);
                        if (orderLocList != null && !orderLocList.Any())
                        {
                            errorList.AppendLine("Supplier ordering location code/name entered is not valid. Enter valid supplier details.");
                            requisitionItemErrorResponse.FieldMapping.Add(RequisitionExcelItemColumn.SupplierOrderingLocation);
                            result = false;
                        }
                        else
                        {
                            item.OrderLocationId = (orderLocList != null) ? orderLocList.FirstOrDefault().LocationId : 0;
                            IsOrderingLocationFilled = true;
                        }
                    }
                    else
                        item.OrderLocationId = 0;
                }
                // Shipfromlocations validations
                if (lstShipFomLocations != null && lstShipFomLocations.Count() > 0 && item.ItemType == ItemType.Material)
                {
                    if (!string.IsNullOrEmpty(item.ShipFromLocationName) && item.PartnerCode > 0)
                    {
                        var shipfromlst = lstShipFomLocations.Where(x => (x.LocationName.Equals(item.ShipFromLocationName.Trim()) || x.ClientLocationCode.Equals(item.ShipFromLocationName.Trim())) && x.PartnerCode == (long)item.PartnerCode);
                        if (shipfromlst != null && !shipfromlst.Any())
                        {
                            errorList.AppendLine("Shipfromlocation code/name entered is not valid. Enter valid supplier details.");
                            requisitionItemErrorResponse.FieldMapping.Add(RequisitionExcelItemColumn.ShipFromLocation);
                            result = false;
                        }
                        else
                        {
                            item.ShipFromLocationId = (shipfromlst != null) ? shipfromlst.FirstOrDefault().LocationId : 0;
                        }
                    }
                    else
                        item.ShipFromLocationId = 0;
                }
                //Supplier Order Contact Validation
                if (lstSuppliersOrderContacts != null && lstSuppliersOrderContacts.Count() > 0)
                {
                    if (!string.IsNullOrEmpty(item.PartnerContactName) && item.PartnerCode > 0)
                    {
                        var orderContactList = lstSuppliersOrderContacts.Where(x => (x.Name.Equals(item.PartnerContactName.Trim()) || x.EmailID.Equals(item.PartnerContactName.Trim())) && x.PartnerCode == (long)item.PartnerCode);
                        if (orderContactList != null && !orderContactList.Any())
                        {
                            errorList.AppendLine("Supplier contact code/name entered is not valid. Enter valid supplier details.");
                            requisitionItemErrorResponse.FieldMapping.Add(RequisitionExcelItemColumn.SupplierOrderingContact);
                            result = false;
                        }
                        else
                        {
                            item.PartnerContactId = (orderContactList != null) ? orderContactList.FirstOrDefault().ContactCode : 0;
                        }
                    }
                    else
                        item.PartnerContactId = 0;

                }

                //Validate Unique Line Number
                result = ValidateUniqueLineNumber(item, requisitionItemErrorResponse, errorList);
                long catalogCategoryId = 0;

                if (EnableGetLineItemsBulkAPI && (!string.IsNullOrEmpty(item.SupplierItemNumber) || !string.IsNullOrEmpty(item.ItemNumber)))
                {
                    long PartnerCode;
                    if (!string.IsNullOrEmpty(item.ItemNumber) && string.IsNullOrEmpty(item.SupplierItemNumber))
                        PartnerCode = 0;
                    else
                        PartnerCode = Convert.ToInt64(item.PartnerCode);

                    bool IsSupplierItemNumberFieldTypeFreeText = false;
                    if (!string.IsNullOrEmpty(item.SupplierItemNumber) && string.IsNullOrEmpty(item.ItemNumber) && SupplierItemNumberFieldType == "0")
                    {
                        IsSupplierItemNumberFieldTypeFreeText = true;
                    }

                    IEnumerable<ResponseItem> lstcatalogItem = responseItemsList.Where(lstitem => (
                        ((!string.IsNullOrEmpty(lstitem.BuyerItemNumber) ? lstitem.BuyerItemNumber : "0") ==
                         (!string.IsNullOrEmpty(item.ItemNumber) ? item.ItemNumber : "0"))
                        &&
                        ((!string.IsNullOrEmpty(lstitem.PartnerItemNumber) ? lstitem.PartnerItemNumber : "0") ==
                         (!string.IsNullOrEmpty(item.SupplierItemNumber) ? item.SupplierItemNumber : "0")) && lstitem.PartnerCode == PartnerCode));

                    if (!IsSupplierItemNumberFieldTypeFreeText && lstcatalogItem != null && lstcatalogItem.Count() > 0)
                    {
                        item.CatalogItemId = lstcatalogItem.FirstOrDefault().CatalogItemId;
                        item.ItemMasterId = lstcatalogItem.FirstOrDefault().IMId;
                        item.Description = lstcatalogItem.FirstOrDefault().Description;
                        item.SupplierPartId = item.SupplierItemNumber;
                        byte CatalogType = lstcatalogItem.FirstOrDefault().CatalogType;
                        if (CatalogType == 1)
                            CatalogType = 2;
                        else if (CatalogType == 9)
                            CatalogType = 5;

                        item.SourceType = (ItemSourceType)CatalogType;
                        bool OverrideExcelUnitPrice = GetOverrideExcelUnitPriceBasedOnConfiguration(Convert.ToString((int)item.SourceType));
                        if (!IsOrderingLocationFilled && lstcatalogItem.FirstOrDefault().OrderingLocationID > 0)
                        {
                            item.OrderLocationId = Convert.ToInt64(lstcatalogItem.FirstOrDefault().OrderingLocationID);
                            IsOrderingLocationFilled = true;

                        }
                        if (OverrideUnitPriceFromCatalogForExcelUploadOfHostedItems && lstcatalogItem.FirstOrDefault().UnitPrice != null && OverrideExcelUnitPrice)
                        {
                            decimal? olditemUnitPrice = item.UnitPrice;
                            if (lstcatalogItem.FirstOrDefault().EffectivePrice > 0)
                                item.UnitPrice = lstcatalogItem.FirstOrDefault().EffectivePrice;
                            else
                                item.UnitPrice = lstcatalogItem.FirstOrDefault().UnitPrice;
                            CalculateQuantityForFixeditems(item, olditemUnitPrice);
                        }
                        if (lstcatalogItem.FirstOrDefault().CategoryId > 0)
                        {
                            catalogCategoryId = lstcatalogItem.FirstOrDefault().CategoryId;
                            if (string.IsNullOrEmpty(item.CategoryName))
                            {
                                item.CategoryId = lstcatalogItem.FirstOrDefault().CategoryId;
                                item.CategoryName = lstcatalogItem.FirstOrDefault().Category;
                            }
                        }

                        var allowAdvances = lstcatalogItem.FirstOrDefault().STD_AllowAdvanceFlag.HasValue
                            ? lstcatalogItem.FirstOrDefault().STD_AllowAdvanceFlag.Value
                            : false;

                        if (EnableAdvancePayment && EnableAdvancePaymentForREQ && allowAdvances)
                        {
                            item.AllowAdvances = allowAdvances;

                            var advanceAmount = lstcatalogItem.FirstOrDefault().STD_AdvanceAmount.HasValue
                                ? lstcatalogItem.FirstOrDefault().STD_AdvanceAmount.Value
                                : 0;

                            var percentage = lstcatalogItem.FirstOrDefault().STD_AdvancePercentage.HasValue
                                ? lstcatalogItem.FirstOrDefault().STD_AdvancePercentage.Value
                                : 0;

                            var releaseDate = lstcatalogItem.FirstOrDefault().STD_AdvanceReleaseDate;

                            var unitPrice = lstcatalogItem.FirstOrDefault().UnitPrice.HasValue
                                ? lstcatalogItem.FirstOrDefault().UnitPrice.Value
                                : 0;

                            var quantity = lstcatalogItem.FirstOrDefault().Quantity.val.HasValue
                                ? lstcatalogItem.FirstOrDefault().Quantity.val.Value
                                : 0;

                            var calculatedAmount = unitPrice * quantity;

                            if (advanceAmount > 0 && calculatedAmount > 0)
                            {
                                var caclculatedAdvancePercentage = Math.Round(advanceAmount / calculatedAmount * 100, minPrecessionValue);
                                item.AdvancePercentage = caclculatedAdvancePercentage;
                            }
                            else if (percentage > 0)
                            {
                                var caclculatedAdvanceAmount = Math.Round(percentage * calculatedAmount / 100, minPrecessionValue);
                                item.AdvanceAmount = caclculatedAdvanceAmount;
                            }

                            if (releaseDate.HasValue)
                            {
                                item.AdvanceReleaseDate = releaseDate.Value;
                            }
                        }

                        RequisitionItem catalogItem = new RequisitionItem();
                        catalogItem.CatalogItemId = lstcatalogItem.FirstOrDefault().CatalogItemId;
                        catalogItem.ItemMasterId = lstcatalogItem.FirstOrDefault().IMId;
                        catalogItem.MinimumOrderQuantity = Convert.ToDecimal(lstcatalogItem.FirstOrDefault().Quantity.Minimum);
                        catalogItem.MaximumOrderQuantity = Convert.ToDecimal(lstcatalogItem.FirstOrDefault().Quantity.Maximum);
                        catalogItem.Banding = Convert.ToInt32(lstcatalogItem.FirstOrDefault().Quantity.Banding);
                        result = ValidateItemBandingAndQty(item, catalogItem, errorList, requisitionItemErrorResponse);
                        var supplierOrderingLocationList = lstSuppliersOrderLocations.Where(lstsupplierloc => (lstsupplierloc.PartnerCode == (long)item.PartnerCode));
                        var defaultSupplierOderingLocationlist = supplierOrderingLocationList.Where(lstsupplierloc => (lstsupplierloc.IsDefault == true));
                        if (!IsOrderingLocationFilled && supplierOrderingLocationList != null && supplierOrderingLocationList.Any() && supplierOrderingLocationList.Count() == 1)
                        {
                            item.OrderLocationId = supplierOrderingLocationList.FirstOrDefault().LocationId;
                            IsOrderingLocationFilled = true;
                        }
                        else if (!IsOrderingLocationFilled && !IsDefaultOrderingLocation && supplierOrderingLocationList != null && supplierOrderingLocationList.Count() > 1)
                        {
                            item.OrderLocationId = 0;
                            IsOrderingLocationFilled = true;
                        }
                        else if (!IsOrderingLocationFilled && IsDefaultOrderingLocation && defaultSupplierOderingLocationlist != null && defaultSupplierOderingLocationlist.Count() > 0)
                        {
                            item.OrderLocationId = defaultSupplierOderingLocationlist.FirstOrDefault().LocationId;
                            IsOrderingLocationFilled = true;
                        }
                        else if (!IsOrderingLocationFilled && IsDefaultOrderingLocation && defaultSupplierOderingLocationlist != null && defaultSupplierOderingLocationlist.Count() == 0 && supplierOrderingLocationList != null && supplierOrderingLocationList.Count() > 0)
                        {
                            item.OrderLocationId = supplierOrderingLocationList.FirstOrDefault().LocationId;
                            IsOrderingLocationFilled = true;
                        }


                    }
                    else if ((!string.IsNullOrEmpty(item.SupplierItemNumber) && string.IsNullOrEmpty(item.ItemNumber)) && (SupplierItemNumberFieldType == "2" || SupplierItemNumberFieldType == "0"))
                    {
                        item.SourceType = ItemSourceType.Manual;
                        item.SupplierPartId = item.SupplierItemNumber;
                    }
                    else if ((!string.IsNullOrEmpty(item.SupplierItemNumber) && !string.IsNullOrEmpty(item.ItemNumber) && (SupplierItemNumberFieldType == "2" || SupplierItemNumberFieldType == "0")))
                    {
                        item.SourceType = ItemSourceType.Manual;
                        item.SupplierPartId = item.SupplierItemNumber;
                    }
                    else
                    {
                        errorList.AppendLine("Item cannot be added to the Document as it have invalid entries");
                        result = false;
                    }
                }
                //Validate Buyer Item Number and Supplier Item Number to checkfor Catalog Item
                else if (!EnableGetLineItemsBulkAPI && !string.IsNullOrEmpty(item.SupplierItemNumber))
                {
                    var catalogItem = ValidateItemNumber(string.Empty, item.SupplierItemNumber, (long)item.PartnerCode);
                    if (catalogItem != null && catalogItem.CatalogItemId > 0)
                    {
                        item.CatalogItemId = catalogItem.CatalogItemId;
                        item.Description = catalogItem.Description;
                        item.SupplierPartId = item.SupplierItemNumber;
                        item.SourceType = ItemSourceType.Hosted;
                        string buyerItemNumber = string.IsNullOrEmpty(item.ItemNumber) ? "0" : item.ItemNumber.Trim();
                        string supplierItemNumber = string.IsNullOrEmpty(item.SupplierItemNumber) ? "0" : item.SupplierItemNumber.Trim();

                        var lstCatalogItems = lstCatalogItemDetails.Where(lstitem => (lstitem.BuyerItemNumber == buyerItemNumber && lstitem.SupplierItemNumber == supplierItemNumber && lstitem.SupplierCode == (long)item.PartnerCode));
                        if (!OverrideUnitPriceFromCatalogForExcelUploadOfHostedItems && lstCatalogItems != null && lstCatalogItems.Count() > 0)
                        {
                            if (!IsOrderingLocationFilled && lstCatalogItems.FirstOrDefault().OrderingLocationID > 0)
                            {
                                item.OrderLocationId = lstCatalogItems.FirstOrDefault().OrderingLocationID;
                                IsOrderingLocationFilled = true;
                            }

                            if (lstCatalogItems.FirstOrDefault().CategoryId > 0)
                            {
                                catalogCategoryId = lstCatalogItems.FirstOrDefault().CategoryId;
                                if (string.IsNullOrEmpty(item.CategoryName))
                                {
                                    item.CategoryId = lstCatalogItems.FirstOrDefault().CategoryId;
                                    item.CategoryName = lstCatalogItems.FirstOrDefault().CategoryName;
                                }
                            }

                        }
                        else
                        {
                            List<long> accessEntities = new List<long>();
                            if (lsDocumentHeaderEntitites != null && lsDocumentHeaderEntitites.Count > 0)
                            {
                                foreach (var addEntity in lsDocumentHeaderEntitites)
                                {
                                    accessEntities.Add(addEntity.EntityDetailCode);
                                }
                            }
                            DateTime? CurrentDate = null;
                            if (item.ItemType == ItemType.Material)
                                CurrentDate = item.DateNeeded;
                            else if (item.ItemType == ItemType.Service)
                                CurrentDate = item.StartDate;
                            SearchResult searchResult = new SearchResult();
                            if (EnableWebAPIForGetLineItems)
                            {
                                ItemFilterInputRequest itemFilterInputRequest = new ItemFilterInputRequest()
                                {
                                    CatalogItemId = catalogItem.CatalogItemId,
                                    AccessEntities = accessEntities,
                                    UOM = item.UOM,
                                    SupplierCode = Convert.ToInt64(item.PartnerCode),
                                    Quantity = item.Quantity,
                                    ContactCode = userContext.ContactCode,
                                    Size = 10,
                                    BIN = item.ItemNumber,
                                    SIN = item.SupplierItemNumber,
                                    CurrencyCode = item.Currency,
                                    CurrentDate = CurrentDate
                                };
                                searchResult = GetDataFromGetLineItems(itemFilterInputRequest);
                            }
                            else
                            {
                                ItemSearchInput itemSearchInput = new ItemSearchInput()
                                {
                                    CatalogItemId = catalogItem.CatalogItemId,
                                    AccessEntities = accessEntities,
                                    UOM = item.UOM,
                                    SupplierCode = Convert.ToInt64(item.PartnerCode),
                                    Quantity = item.Quantity,
                                    ContactCode = userContext.ContactCode,
                                    Size = 10,
                                    BIN = item.ItemNumber,
                                    SIN = item.SupplierItemNumber,
                                    CurrencyCode = item.Currency,
                                    CurrentDate = CurrentDate
                                };

                                ProxyCatalogService proxyCatalogService = new ProxyCatalogService(UserContext, this.JWTToken);
                                searchResult = proxyCatalogService.GetLineItemsSearch(itemSearchInput);
                            }
                            if (searchResult != null && searchResult.Items != null && searchResult.Items.Count > 0)
                            {
                                byte CatalogType = searchResult.Items[0].CatalogType;
                                if (CatalogType == 1)
                                    CatalogType = 2;
                                else if (CatalogType == 9)
                                    CatalogType = 5;

                                item.SourceType = (ItemSourceType)CatalogType;

                                bool OverrideExcelUnitPrice = GetOverrideExcelUnitPriceBasedOnConfiguration(Convert.ToString((int)item.SourceType));

                                if (!IsOrderingLocationFilled && searchResult.Items[0].OrderingLocationID > 0)
                                {
                                    item.OrderLocationId = Convert.ToInt64(searchResult.Items[0].OrderingLocationID);
                                    IsOrderingLocationFilled = true;

                                }
                                if (OverrideUnitPriceFromCatalogForExcelUploadOfHostedItems && searchResult.Items[0].UnitPrice != null && OverrideExcelUnitPrice)
                                {
                                    if (searchResult.Items[0].EffectivePrice > 0)
                                        item.UnitPrice = searchResult.Items[0].EffectivePrice;
                                    else
                                        item.UnitPrice = searchResult.Items[0].UnitPrice;

                                    decimal? olditemUnitPrice = item.UnitPrice;
                                    item.UnitPrice = searchResult.Items[0].UnitPrice;
                                    CalculateQuantityForFixeditems(item, olditemUnitPrice);
                                }
                                if (searchResult.Items[0].CategoryId > 0)
                                {
                                    catalogCategoryId = searchResult.Items[0].CategoryId;
                                    if (string.IsNullOrEmpty(item.CategoryName))
                                    {
                                        item.CategoryId = searchResult.Items[0].CategoryId;
                                        item.CategoryName = searchResult.Items[0].Category;
                                    }
                                }
                                item.ItemMasterId = searchResult.Items[0].IMId;

                                CatalogItemDetails catalogItemDetails = new CatalogItemDetails();
                                catalogItemDetails.BuyerItemNumber = string.IsNullOrEmpty(item.ItemNumber) ? "0" : item.ItemNumber.Trim();
                                catalogItemDetails.SupplierItemNumber = string.IsNullOrEmpty(item.SupplierItemNumber) ? "0" : item.SupplierItemNumber.Trim();
                                catalogItemDetails.OrderingLocationID = Convert.ToInt64(searchResult.Items[0].OrderingLocationID);
                                catalogItemDetails.CategoryId = searchResult.Items[0].CategoryId;
                                catalogItemDetails.CategoryName = searchResult.Items[0].Category;
                                catalogItemDetails.SupplierCode = Convert.ToInt64(item.PartnerCode);
                                lstCatalogItemDetails.Add(catalogItemDetails);

                                var allowAdvances = searchResult.Items[0].STD_AllowAdvanceFlag ?? false;

                                if (EnableAdvancePayment && EnableAdvancePaymentForREQ && allowAdvances)
                                {
                                    item.AllowAdvances = allowAdvances;

                                    var advanceAmount = searchResult.Items[0].STD_AdvanceAmount.HasValue
                                        ? searchResult.Items[0].STD_AdvanceAmount.Value
                                        : 0;

                                    var percentage = searchResult.Items[0].STD_AdvancePercentage.HasValue
                                        ? searchResult.Items[0].STD_AdvancePercentage.Value
                                        : 0;

                                    var releaseDate = searchResult.Items[0].STD_AdvanceReleaseDate;

                                    var unitPrice = searchResult.Items[0].UnitPrice.HasValue
                                        ? searchResult.Items[0].UnitPrice.Value
                                        : 0;

                                    var quantity = searchResult.Items[0].Quantity.val.HasValue
                                        ? searchResult.Items[0].Quantity.val.Value
                                        : 0;

                                    var calculatedAmount = unitPrice * quantity;

                                    if (advanceAmount > 0 && calculatedAmount > 0)
                                    {
                                        var caclculatedAdvancePercentage = Math.Round(advanceAmount / calculatedAmount * 100, minPrecessionValue);
                                        item.AdvancePercentage = caclculatedAdvancePercentage;
                                    }
                                    else if (percentage > 0)
                                    {
                                        var caclculatedAdvanceAmount = Math.Round(percentage * calculatedAmount / 100, minPrecessionValue);
                                        item.AdvanceAmount = caclculatedAdvanceAmount;
                                    }

                                    if (releaseDate.HasValue)
                                    {
                                        item.AdvanceReleaseDate = releaseDate.Value;
                                    }
                                }
                            }
                        }
                        //Validate the Banding and Min\Max Order Qty
                        result = ValidateItemBandingAndQty(item, catalogItem, errorList, requisitionItemErrorResponse);
                        var supplierOrderingLocationList = lstSuppliersOrderLocations.Where(lstsupplierloc => (lstsupplierloc.PartnerCode == (long)item.PartnerCode));
                        var defaultSupplierOderingLocationlist = supplierOrderingLocationList.Where(lstsupplierloc => (lstsupplierloc.IsDefault == true));
                        if (!IsOrderingLocationFilled && supplierOrderingLocationList != null && supplierOrderingLocationList.Any() && supplierOrderingLocationList.Count() == 1)
                        {
                            item.OrderLocationId = supplierOrderingLocationList.FirstOrDefault().LocationId;
                            IsOrderingLocationFilled = true;
                        }
                        else if (!IsOrderingLocationFilled && !IsDefaultOrderingLocation && supplierOrderingLocationList != null && supplierOrderingLocationList.Count() > 1)
                        {
                            item.OrderLocationId = 0;
                            IsOrderingLocationFilled = true;
                        }
                        else if (!IsOrderingLocationFilled && IsDefaultOrderingLocation && defaultSupplierOderingLocationlist != null && defaultSupplierOderingLocationlist.Count() > 0)
                        {
                            item.OrderLocationId = defaultSupplierOderingLocationlist.FirstOrDefault().LocationId;
                            IsOrderingLocationFilled = true;
                        }
                        else if (!IsOrderingLocationFilled && IsDefaultOrderingLocation && defaultSupplierOderingLocationlist != null && defaultSupplierOderingLocationlist.Count() == 0 && supplierOrderingLocationList != null && supplierOrderingLocationList.Count() > 0)
                        {
                            item.OrderLocationId = supplierOrderingLocationList.FirstOrDefault().LocationId;
                            IsOrderingLocationFilled = true;
                        }
                    }
                    else if (catalogItem == null && !string.IsNullOrEmpty(item.Description))
                    {
                        item.SupplierPartId = item.SupplierItemNumber;
                        item.SourceType = ItemSourceType.Manual;
                    }
                    else if (catalogItem == null && string.IsNullOrEmpty(item.Description))
                    {
                        //Supplier ITem is not valid and Description is not given for a line Item This will be treated as Invalid Supplier ITem Number
                        errorList.AppendLine("Supplier Item Number specified is not valid. Enter valid Supplier Item Number.");
                        requisitionItemErrorResponse.FieldMapping.Add(RequisitionExcelItemColumn.SupplierItemNumber);
                        result = false;
                    }
                }
                else
                {
                    //For Non Catalog ITems , Description is mandatory
                    if (string.IsNullOrEmpty(item.Description))
                    {
                        errorList.AppendLine("Description is required for Non Catalog Items");
                        requisitionItemErrorResponse.FieldMapping.Add(RequisitionExcelItemColumn.Description);
                        result = false;
                    }
                }
                //Category Validation
                if (lstAllCategories != null && lstAllCategories.Count() > 0)
                {
                    if (string.IsNullOrEmpty(item.CategoryName))
                    {
                        errorList.AppendLine("Category Name is Mandatory.");
                        requisitionItemErrorResponse.FieldMapping.Add(RequisitionExcelItemColumn.Category);
                        result = false;
                    }
                    else
                    {
                        var categoryList = lstAllCategories.Where(x => x.PASName.Equals(item.CategoryName.Trim()) || x.ClientPASCode.Equals(item.CategoryName.Trim()));
                        if (categoryList != null && !categoryList.Any())
                        {
                            errorList.AppendLine("Invalid Category.");
                            requisitionItemErrorResponse.FieldMapping.Add(RequisitionExcelItemColumn.Category);
                            result = false;
                        }
                        else
                        {
                            item.CategoryId = (categoryList != null) ? categoryList.FirstOrDefault().PASCode : 0;
                            if (catalogCategoryId > 0 && catalogCategoryId != item.CategoryId)
                            {
                                errorList.AppendLine("Mismatch in Catalog Item category and Excel template category");
                                requisitionItemErrorResponse.FieldMapping.Add(RequisitionExcelItemColumn.Category);
                                result = false;
                            }
                        }
                    }
                }
                if (item.TrasmissionMode == 0 && (long)item.PartnerCode > 0)
                {
                    if ((MapDispatchModetoOrderingLocation && item.OrderLocationId > 0) || (!MapDispatchModetoOrderingLocation && item.PartnerCode > 0))
                        setTransmissionMode(ref item);
                    if (item.TrasmissionMode == (int)(POTransmissionMode.Email) && partnerDocInterfaceDic.Tables.Count > 0)
                        setReqSupplierContactDetails(isPartnerBasedOnOrderingLoc, ref item);
                }
                if (item.TrasmissionMode == (int)(POTransmissionMode.Email))
                {
                    if (string.IsNullOrEmpty(item.TransmissionValue))
                    {
                        errorList.AppendLine("Email is required for Dispatch Mode Direct Email");
                        requisitionItemErrorResponse.FieldMapping.Add(RequisitionExcelItemColumn.DispatchInfo);
                        result = false;
                    }
                    else
                    {
                        const string pattern = @"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|" + @"([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)" + @"@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$";
                        var regex = new Regex(pattern, RegexOptions.IgnoreCase);
                        if (!regex.IsMatch(item.TransmissionValue))
                        {
                            errorList.AppendLine("Email entered is not valid. Enter valid Email details.");
                            requisitionItemErrorResponse.FieldMapping.Add(RequisitionExcelItemColumn.DispatchInfo);
                            result = false;
                        }
                    }
                }

                //tax Validation
                if (lstTaxes != null && lstTaxes.Count() > 0)
                {
                    if (item.Taxes.Count() > 0)
                    {
                        requisitionItemErrorResponse.RequisitionLineTaxCodes = new List<RequisitionLineTaxCodes>();
                        StringBuilder taxerrorList = new StringBuilder();
                        List<Taxes> lsttaxes = new List<Taxes>();
                        foreach (var objTaxCode in item.Taxes)
                        {
                            var taxObject = lstTaxes.Where(x => x.TaxCode == objTaxCode.TaxCode).FirstOrDefault();

                            if (taxObject == null)
                            {

                                RequisitionLineTaxCodes obj = new RequisitionLineTaxCodes();
                                if (string.IsNullOrEmpty(objTaxCode.TaxCode))
                                    obj.Message = "Tax code is a Mandatory Field.";
                                else
                                    obj.Message = "Invalid Tax code.";
                                obj.TaxCode = objTaxCode.TaxCode == null ? "" : objTaxCode.TaxCode;
                                obj.LineNumber = (int)objTaxCode.LineNumber;
                                requisitionItemErrorResponse.RequisitionLineTaxCodes.Add(obj);
                            }
                            else
                            {
                                objTaxCode.TaxId = taxObject.TaxId;
                                objTaxCode.TaxValue = taxObject.TaxPercentage;
                                objTaxCode.TaxPercentage = taxObject.TaxPercentage;
                                lsttaxes.Add(objTaxCode);
                            }
                        }
                        if (requisitionItemErrorResponse.RequisitionLineTaxCodes.Count > 0)
                            errorList.AppendLine("Invalid Tax codes");
                        item.Taxes = lsttaxes;
                    }
                }

                List<Tuple<decimal, string, int, RequisitionExcelItemColumn>> validateNumerics = new List<Tuple<decimal, string, int, RequisitionExcelItemColumn>>();
                validateNumerics.Add(new Tuple<decimal, string, int, RequisitionExcelItemColumn>((decimal)item.UnitPrice, "Unit Price", maxPrecessionForUnitPrice, RequisitionExcelItemColumn.UnitPrice));
                validateNumerics.Add(new Tuple<decimal, string, int, RequisitionExcelItemColumn>((decimal)item.Quantity, "Quantity", maxPrecessionForQuantity, RequisitionExcelItemColumn.QuantityEffort));
                validateNumerics.Add(new Tuple<decimal, string, int, RequisitionExcelItemColumn>((decimal)item.ShippingCharges, "Shipping and Freight", maxPrecessionForShippingOrFreight, RequisitionExcelItemColumn.ShippingAndFreight));
                validateNumerics.Add(new Tuple<decimal, string, int, RequisitionExcelItemColumn>((decimal)item.AdditionalCharges, "Other Charges", maxPrecessionForOtherCharges, RequisitionExcelItemColumn.OtherCharges));



                foreach (var validateItem in validateNumerics)
                {
                    if (validateItem.Item3 > 0)
                    {
                        hasPrecisionError = UpdateErrorsBasedOnPrecisionValues(validateItem.Item1, validateItem.Item2, validateItem.Item3, errorList);
                        if (hasPrecisionError)
                        {
                            requisitionItemErrorResponse.FieldMapping.Add(validateItem.Item4);
                            result = false;
                        }
                    }
                }
                //Add Error Logs
                if (ReqResponse.lstRequisitionItemErrorResponse != null && errorList.Length > 0)
                {
                    requisitionItemErrorResponse.Message = errorList.ToString();
                }


            }
            return result;
        }

        private bool UpdateErrorsBasedOnPrecisionValues(decimal columnValue, string columnName, int maxPrecisionValue, StringBuilder errorList)
        {
            bool hasError = false;
            bool hasZerosAfterPrecision = false;
            int decimalCount = 0;
            string number = Convert.ToString(columnValue);
            var splitArray = number.Split('.');
            if (splitArray.Length > 1)
            {
                decimalCount = number.Substring(number.IndexOf(".") + 1).Length;
                long decimalAfterPrecission = 0;
                if (splitArray[1].Length > maxPrecisionValue)
                    long.TryParse(splitArray[1].Substring(maxPrecisionValue), out decimalAfterPrecission);
                hasZerosAfterPrecision = decimalAfterPrecission <= 0;
            }
            if (decimalCount > maxPrecisionValue && !hasZerosAfterPrecision)
            {
                errorList.AppendLine("Decimal Digits Count in " + columnName + " exceeds the setting value ("
                    + maxPrecisionValue + ")");
                hasError = true;
            }
            return hasError;
        }

        private SearchResult GetDataFromGetLineItems(ItemFilterInputRequest itemFilterInput)
        {
            SearchResult searchResult = new SearchResult();
            string AppURL = MultiRegionConfig.GetConfig(CloudConfig.AppURL);
            string GetLineItemsURl = AppURL + URLs.GetLineItems;
            Req.BusinessObjects.RESTAPIHelper.RequestHeaders requestHeaders = new Req.BusinessObjects.RESTAPIHelper.RequestHeaders();
            string appName = URLs.AppNameForGetLineItems;
            string useCase = URLs.UseCaseForExcelUpload;
            requestHeaders.Set(this.userContext, JWTToken);
            var webAPI = new GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.WebAPI(requestHeaders, appName, useCase);
            var JsonResult = webAPI.ExecutePost(GetLineItemsURl, itemFilterInput);
            searchResult = JsonConvert.DeserializeObject<SearchResult>(JsonResult);

            return searchResult;
        }
        private SearchResult AccessSearchItems(ItemSearchBulkInputRequest itemSearchInput)
        {
            SearchResult searchResult = new SearchResult();
            try
            {
                string AppURL = MultiRegionConfig.GetConfig(CloudConfig.AppURL);
                string AccessSearchItemsURl = AppURL + URLs.AccessSearchItems;
                Req.BusinessObjects.RESTAPIHelper.RequestHeaders requestHeaders = new Req.BusinessObjects.RESTAPIHelper.RequestHeaders();
                string appName = URLs.AppNameForGetLineItems;
                string useCase = URLs.UseCaseForExcelUpload;
                requestHeaders.Set(this.userContext, JWTToken);
                var webAPI = new GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.WebAPI(requestHeaders, appName, useCase);
                var JsonResult = webAPI.ExecutePost(AccessSearchItemsURl, itemSearchInput);
                searchResult = JsonConvert.DeserializeObject<SearchResult>(JsonResult);
                logNewRelicEvents("AccessSearchItems", "Request", JsonConvert.SerializeObject(itemSearchInput));
                logNewRelicEvents("AccessSearchItems", "Response", JsonConvert.SerializeObject(searchResult));
                return searchResult;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error Occured in AccessSearchItems method", ex);
                throw ex;
            }
        }

        private void logNewRelicEvents(string eventname, string key, string values)
        {
            var eventAttributes = new Dictionary<string, object>();
            eventAttributes.Add("eventname", eventname);
            eventAttributes.Add("key", key);
            eventAttributes.Add("values", values);
            NewRelic.Api.Agent.NewRelic.RecordCustomEvent("ExcelUploadEvents", eventAttributes);
        }
        private Newtonsoft.Json.Linq.JToken[] GetContractData(List<string> ContractNumbers)
        {
            try
            {
                string contractNumbersList = string.Join(",", ContractNumbers);
                string contractDocStatus = "71,83";
                string getAllChildContractsByContractNumber = MultiRegionConfig.GetConfig(CloudConfig.APIMBaseURL) + URLs.GetAllChildContractsByContractNumber;
                string InputData = "{\"SearchKeyword\":\"\",\"Isinheritancefilterrequired\":false," +
                    "\"Filters\":[\"moduleScope:Contract\",\"pageNumber:1\",\"noOfRecords:2000\",\"isSeeAllResult:true\"]," +
                    "\"AdvanceSearchInput\":[{\"SearchKey\":\"DM_Status\",\"Value\":\"" + contractDocStatus + "\",\"IsCustAttr\":false,\"FieldType\":\"Autosuggest\"}," +
                    "{\"SearchKey\":\"ContractNumber\",\"Value\":\"" + contractNumbersList + "\",\"IsCustAttr\":false,\"FieldType\":\"Plain\"}]," +
                    "\"oloc\":216}";
                Req.BusinessObjects.RESTAPIHelper.RequestHeaders requestHeaders = new Req.BusinessObjects.RESTAPIHelper.RequestHeaders();
                string appName = URLs.AppName;
                string useCase = URLs.UseCaseForExcelUpload;
                requestHeaders.Set(this.userContext, JWTToken);
                var webAPI = new GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.WebAPI(requestHeaders, appName, useCase);
                var request = JsonConvert.DeserializeObject(InputData);
                var jsonResult = webAPI.ExecutePost(getAllChildContractsByContractNumber, request);
                var contractData = Newtonsoft.Json.Linq.JObject.Parse(jsonResult);
                var dataSearchResult = contractData.GetValue("DataSearchResult").SelectToken("Value").ToArray();
                return dataSearchResult;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error Occured in GetContractData method", ex);
                throw ex;
            }

        }

        private List<PartnerCurrencies> GetPartnerCurrencies(string Partners)
        {
            List<PartnerCurrencies> partnerCurrencies = new List<PartnerCurrencies>();
            try
            {
                string InputData = "{\"partners\":\"" + Partners + "\"}";
                var partnerlist = JsonConvert.DeserializeObject(InputData);
                string AppURL = MultiRegionConfig.GetConfig(CloudConfig.AppURL);
                string GetPartnerCurrencies = AppURL + URLs.GetPartnerCurrencies;
                Req.BusinessObjects.RESTAPIHelper.RequestHeaders requestHeaders = new Req.BusinessObjects.RESTAPIHelper.RequestHeaders();
                string appName = URLs.AppNameForGetLineItems;
                string useCase = URLs.UseCaseForExcelUpload;
                requestHeaders.Set(this.userContext, JWTToken);
                var webAPI = new GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.WebAPI(requestHeaders, appName, useCase);
                var JsonResult = webAPI.ExecutePost(GetPartnerCurrencies, partnerlist);
                partnerCurrencies = JsonConvert.DeserializeObject<List<PartnerCurrencies>>(JsonResult);
                logNewRelicEvents("GetPartnerCurrencies", "Request", JsonConvert.SerializeObject(Partners));
                logNewRelicEvents("GetPartnerCurrencies", "Response", JsonConvert.SerializeObject(partnerCurrencies));
                return partnerCurrencies;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error Occured in AccessSearchItems method", ex);
                throw ex;
            }
        }
        private bool ValidateItemSplitAccountingDetails(RequisitionItem item, RequisitionItemErrorResponse requisitionItemErrorResponse)
        {
            bool result = true;

            //Single Split Accounting
            if (item != null && item.ItemSplitsDetail != null)
            {
                StringBuilder errorList = new StringBuilder();

                bool SplitValidationResult = ValidateSplitItemDetails(item, requisitionItemErrorResponse, errorList);
                bool AccontingVaidationResult = ValidateAccounting(item.ItemSplitsDetail, requisitionItemErrorResponse, errorList, item.LineReferenceNumber);
                if (SplitValidationResult && AccontingVaidationResult)
                    return true;
                else
                    return false;
            }

            return result;
        }

        private bool ValidateTaxCodes(RequisitionItem item, RequisitionItemErrorResponse requisitionItemErrorResponse)
        {
            bool result = false;
            if (wsRequisitionLines != null && requisitionItemErrorResponse != null && requisitionItemErrorResponse.RequisitionLineTaxCodes != null && requisitionItemErrorResponse.RequisitionLineTaxCodes.Count > 0)
            {
                result = true;
            }
            return result;
        }
        public void GenerateErrorResultFile()
        {
            List<RequisitionLineTaxCodes> lst = GetTaxCodeErrors();

            if ((wsRequisitionLines != null && ReqResponse.lstRequisitionItemErrorResponse != null && ReqResponse.lstRequisitionItemErrorResponse.Count > 0) || (lst != null && lst.Count > 0))
            {
                var itemErrorLines = ReqResponse.lstRequisitionItemErrorResponse.Select(x => x.LineNumber).ToArray<string>();
                var lstRequisitionMultipleItemSplitMapping = ReqResponse.lstRequisitionItemErrorResponse.Where(x => x.RequisitionMultipleItemSplitMapping != null).SelectMany(x => x.RequisitionMultipleItemSplitMapping).ToList();
                var splitErrorLines = lstRequisitionMultipleItemSplitMapping.Select(x => x.LineNumber).ToArray<string>();
                var taxcodeError = ReqResponse.lstRequisitionItemErrorResponse.Where(x => x.RequisitionLineTaxCodes != null).SelectMany(x => x.RequisitionLineTaxCodes).ToList();
                iCol_REQ_MSG = GetDataTableColumnIndexByName(dtRequisitionLines, Enum.GetName(typeof(RequisitionExcelItemColumn), RequisitionExcelItemColumn.ErrorMessages));
                iCol_ACCT_MSG = GetDataTableColumnIndexByName(dtRequisitionAccounting, Enum.GetName(typeof(RequisitionAccountingSplitColumn), RequisitionAccountingSplitColumn.ErrorMessages));
                iCol_TAX_MSG = GetDataTableColumnIndexByName(dtTaxCodes, Enum.GetName(typeof(RequisitionLinesRowTaxCodesColumn), RequisitionLinesRowTaxCodesColumn.ErrorMessages));
                if (itemErrorLines != null && itemErrorLines.Count() > 0 || (lst != null && lst.Count > 0))
                {
                    CreateErrorLine("REQ", wsRequisitionLines, itemErrorLines, (int)RequisitionExcelTemplateRowStartIndex.RequisitionLinesRowStartIndex, (int)RequisitionExcelItemColumn.LineReferenceNumber, reqRowCount, ReqResponse.lstRequisitionItemErrorResponse, null);
                    CreateErrorLine("ACCT", wsAccountingSplits, itemErrorLines, (int)RequisitionExcelTemplateRowStartIndex.RequisitionAccountingSplitRowStartIndex, (int)RequisitionAccountingSplitColumn.LineReferenceNumber, acctRowCount, lstRequisitionMultipleItemSplitMapping, ReqResponse.lstRequisitionItemErrorResponse);

                    if (taxcodeError != null && taxcodeError.Count() == 0)
                        taxcodeError = new List<RequisitionLineTaxCodes>();
                    if (taxcodeError != null && lst != null && lst.Count > 0)
                        taxcodeError.AddRange(lst);

                    CreateErrorLineForTax(wsTaxCodes, (int)RequisitionExcelTemplateRowStartIndex.RequisitionLinesRowTaxCodesStartIndex + 1, (int)RequisitionLinesRowTaxCodesColumn.LineReferenceNumber, taxcodeError);

                    objFileDetails.FileId = string.Empty;
                    requistionTemplateRespone.ProcessedFileId = UploadFiletoBlobContainerAndGetFileId();
                    System.Xml.Serialization.XmlSerializer xmlSerializer = new System.Xml.Serialization.XmlSerializer(typeof(RequisitionResponse));
                    var sb = new StringBuilder();
                    xmlSerializer.Serialize(new StringWriter(sb), ReqResponse);
                    this.ReqUploadLog.ProcessedXMLResult = sb.ToString();
                }
            }
            //workbook.Save(@"C:\temp\" + "Download_errorlog.xlsx");
        }

        private void CreateErrorLineForTax(ExcelLib.Worksheet workSheet, int startIndex, int columnNo, List<RequisitionLineTaxCodes> lst)
        {
            for (var iRow = startIndex; iRow <= startIndex + taxRowCount; iRow++)
            {
                var excelRowLine = workSheet.Cells.Rows[iRow];
                var lineNumber = excelRowLine[columnNo].StringValue;
                if (excelRowLine[columnNo].StringValue == "")
                    lineNumber = "0";

                if ((lineNumber == "" || lineNumber == "0") && excelRowLine[columnNo + 1].StringValue != "")
                {
                    for (var icol = 0; icol < dtTaxCodesList.Columns.Count; icol++)
                        FormatColor(excelRowLine[icol], Color.Transparent);

                    excelRowLine[iCol_TAX_MSG].PutValue("Line Reference Number is required. Enter the valid line reference number");
                    FormatColor(excelRowLine[(int)RequisitionLinesRowTaxCodesColumn.LineReferenceNumber], Color.Pink);
                }
                if (lineNumber != "0")
                {
                    var result = lst.Where(x => x.LineNumber.ToString() == lineNumber).FirstOrDefault();
                    if (result != null)
                    {
                        for (var icol = 0; icol < dtTaxCodesList.Columns.Count; icol++)
                            FormatColor(excelRowLine[icol], Color.Transparent);


                        if (excelRowLine[columnNo + 1].StringValue == "" && (string.IsNullOrEmpty(result.TaxCode) || (result.TaxCode == "" && result.LineNumber > 0)))
                        {
                            FormatColor(excelRowLine[(int)RequisitionLinesRowTaxCodesColumn.TaxCode], Color.Pink);
                            excelRowLine[iCol_TAX_MSG].PutValue(result.Message);
                        }

                        DataRow[] results = dtRequisitionLines.Select("LineReferenceNumber ='" + lineNumber + "'");
                        if (results.Count() == 0)
                        {
                            FormatColor(excelRowLine[(int)RequisitionLinesRowTaxCodesColumn.LineReferenceNumber], Color.Pink);
                            excelRowLine[iCol_TAX_MSG].PutValue(result.Message);
                        }
                        if (lstDuplicateTaxes.Count > 0)
                        {
                            var duplicateTaxes = lstDuplicateTaxes.Where(x => x.LineNumber.ToString() == lineNumber && x.TaxCode == excelRowLine[columnNo + 1].StringValue).FirstOrDefault();
                            if (duplicateTaxes != null)
                            {
                                FormatColor(excelRowLine[(int)RequisitionLinesRowTaxCodesColumn.LineReferenceNumber], Color.Pink);
                                FormatColor(excelRowLine[(int)RequisitionLinesRowTaxCodesColumn.TaxCode], Color.Pink);
                                excelRowLine[iCol_TAX_MSG].PutValue(result.Message);
                            }
                        }

                    }

                }
            }
        }

        private void CreateErrorLine(string typeOfWS, ExcelLib.Worksheet workSheet, string[] errorLines, int startIndex, int columnNo, int noofRows, dynamic lstResponse, dynamic splitMappingFields)
        {

            for (var iRow = noofRows; iRow >= startIndex + 1; iRow--)
            {
                var excelRowLine = workSheet.Cells.Rows[iRow];
                var lineNumber = excelRowLine[columnNo].StringValue;
                if (excelRowLine[columnNo].StringValue == "")
                    lineNumber = "0";
                var rowFound = errorLines.FirstOrDefault(x => x == lineNumber);

                bool insertError = false;
                if (rowFound == null)
                    workSheet.Cells.DeleteRow(iRow);
                else
                {
                    //make all column white
                    if (typeOfWS == "REQ")
                    {
                        for (var icol = 0; icol < dtRequisitionLines.Columns.Count; icol++)
                            FormatColor(excelRowLine[icol], Color.Transparent);
                        FormatReqWorkSheet(lstResponse, excelRowLine, lineNumber);
                    }
                    else if (typeOfWS == "ACCT")
                    {
                        for (var icol = 0; icol < dtRequisitionAccounting.Columns.Count; icol++)
                            FormatColor(excelRowLine[icol], Color.Transparent);

                        if (iRow == startIndex + 1 || lineNumber != workSheet.Cells.Rows[iRow - 1][columnNo].StringValue) //insert error for first line only
                            insertError = true;
                        else
                            insertError = false;


                        FormatAcctWorksheet(lstResponse, excelRowLine, lineNumber, insertError, splitMappingFields);
                    }
                }
            }
        }

        private void FormatAcctWorksheet(List<RequisitionItemSplitErrorResponse> lstResponse, ExcelLib.Row excelRowLine, string lineNumber, bool insertError, List<RequisitionItemErrorResponse> splitMappingFields)
        {
            var responseList = lstResponse.Where(x => x.LineNumber == lineNumber).ToList();
            if (responseList != null && responseList.Count() > 0)
            {
                if (insertError == true)
                    excelRowLine[iCol_ACCT_MSG].PutValue(String.Join(" ", responseList.Select(x => x.Message).ToArray()));
                else
                    excelRowLine[iCol_ACCT_MSG].PutValue("");

                var splitMappingField = splitMappingFields.FirstOrDefault(x => x.LineNumber == lineNumber).SplitItemFieldMapping;
                if (splitMappingField != null && splitMappingField.Count() > 0)
                {
                    foreach (var column in splitMappingField) //static fields
                    {
                        var index = GetDataTableColumnIndexByName(dtRequisitionAccounting, Enum.GetName(typeof(RequisitionAccountingSplitColumn), column));
                        FormatColor(excelRowLine[index], Color.Pink);
                    }
                }
                foreach (var response in responseList)
                    FormatAcctErrForDynamicFields(response.MultipleSplitAccountingErrors, excelRowLine, dtRequisitionAccounting);
            }
        }

        private void FormatReqWorkSheet(List<RequisitionItemErrorResponse> lstResponse, ExcelLib.Row excelRowLine, string lineNumber)
        {
            var response = lstResponse.FirstOrDefault(x => x.LineNumber == lineNumber);
            if (response != null)
            {
                excelRowLine[iCol_REQ_MSG].PutValue(response.Message);
                if (response.FieldMapping != null && response.FieldMapping.Count() > 0)
                {
                    foreach (var column in response.FieldMapping)
                        FormatColor(excelRowLine[(int)column], Color.Pink);
                }

                if (response.SingleSplitAccountingErrors != null && response.SingleSplitAccountingErrors.Count() > 0)
                    FormatAcctErrForDynamicFields(response.SingleSplitAccountingErrors, excelRowLine, dtRequisitionLines);
            }
        }
        private List<RequisitionLineTaxCodes> GetTaxCodeErrors()
        {
            List<RequisitionLineTaxCodes> lst = new List<RequisitionLineTaxCodes>();
            RequisitionLineTaxCodes obj = new RequisitionLineTaxCodes();

            if (dtTaxCodesList != null && dtTaxCodesList.Rows.Count > 0)
            {
                foreach (DataRow row in dtTaxCodesList.Rows)
                {

                    obj = new RequisitionLineTaxCodes();
                    var lineNumber = Convert.ToInt16(row["LineReferenceNumber"] == DBNull.Value ? 0 : row["LineReferenceNumber"]);
                    if (lineNumber == 0 && Convert.ToString(row["TaxCode"] == DBNull.Value ? "" : row["TaxCode"]) != "")
                    {
                        obj.LineNumber = 0;
                        obj.Message = "Line Reference Number is required. Enter the valid line reference number";
                        obj.TaxCode = Convert.ToString(row["TaxCode"]);
                        lst.Add(obj);
                    }
                    else
                    {
                        DataRow[] result = dtRequisitionLines.Select("LineReferenceNumber ='" + lineNumber.ToString() + "'");
                        if (result.Count() == 0 && lineNumber > 0)
                        {
                            obj.LineNumber = lineNumber;
                            obj.Message = "Invalid Line Number";
                            obj.TaxCode = Convert.ToString(row["TaxCode"] == DBNull.Value ? "" : row["TaxCode"]);
                            lst.Add(obj);
                        }
                        else if (lstDuplicateTaxes.Count > 0)
                        {
                            var taxCode = Convert.ToString(row["TaxCode"] == DBNull.Value ? "" : row["TaxCode"]);
                            var duplicateTaxes = lstDuplicateTaxes.Where(x => x.LineNumber == lineNumber && x.TaxCode == taxCode).FirstOrDefault();
                            if (duplicateTaxes != null && lineNumber > 0)
                            {
                                obj.LineNumber = lineNumber;
                                obj.Message = "Duplicate Tax Codes";
                                obj.TaxCode = Convert.ToString(row["TaxCode"] == DBNull.Value ? "" : row["TaxCode"]);
                                lst.Add(obj);
                            }
                        }
                    }

                }
            }
            return lst;
        }
        private void FormatAcctErrForDynamicFields(List<RequisitionSplitItemAccountingErrors> itemSplitAcctErrors, ExcelLib.Row excelRowLine, DataTable dt)
        {
            foreach (var column in itemSplitAcctErrors)
            {
                var index = GetDataTableColumnIndexByName(dt, column.FieldName);
                var cell = excelRowLine[index];
                if (cell.StringValue == column.FieldValue || cell.StringValue == "")
                    FormatColor(cell, Color.Pink);

            }
        }

        private void FormatColor(ExcelLib.Cell cell, Color color)
        {
            var style = cell.GetStyle();
            style.ForegroundColor = color;
            style.Pattern = ExcelLib.BackgroundType.Solid;
            style.SetBorder(ExcelLib.BorderType.RightBorder, ExcelLib.CellBorderType.Thin, Color.Gray);
            style.SetBorder(ExcelLib.BorderType.LeftBorder, ExcelLib.CellBorderType.Thin, Color.Gray);
            style.SetBorder(ExcelLib.BorderType.BottomBorder, ExcelLib.CellBorderType.Thin, Color.Gray);
            style.SetBorder(ExcelLib.BorderType.TopBorder, ExcelLib.CellBorderType.Thin, Color.Gray);
            cell.SetStyle(style);
        }
        private void DeleteAllLines(ExcelLib.Worksheet workSheet, int columnNo, int startIndex)
        {
            var totalRows = workSheet.Cells.GetLastDataRow(columnNo);
            workSheet.Cells.DeleteRows(startIndex + 1, totalRows);
        }

        private bool ValidateAccounting(List<RequisitionSplitItems> lstSplitItems, RequisitionItemErrorResponse requisitionItemErrorResponse, StringBuilder errorListSplits, string lineNumber)
        {
            bool result = true;
            List<RequisitionSplitItemAccountingErrors> lstAccountingErrors = null;
            if (lstSplitItems.Count() == 1)
            {
                requisitionItemErrorResponse.SingleSplitAccountingErrors = new List<RequisitionSplitItemAccountingErrors>();
            }
            else if (lstSplitItems.Count() > 1)
            {
                requisitionItemErrorResponse.RequisitionMultipleItemSplitMapping = new List<RequisitionItemSplitErrorResponse>();

            }

            int splitNumber = 0;
            foreach (RequisitionSplitItems objSplitItem in lstSplitItems)
            {
                StringBuilder errorList = new StringBuilder();
                lstAccountingErrors = new List<RequisitionSplitItemAccountingErrors>();
                List<DocumentSplitItemEntity> lstDocumentSplitItemEntites = objSplitItem.DocumentSplitItemEntities;
                splitNumber++;
                foreach (DocumentSplitItemEntity objDocumentSplitItemEntity in lstDocumentSplitItemEntites)
                {
                    //Check if Header Level Entity or not. If yes default it to Header Level Entity Values
                    if (lsDocumentHeaderEntitites.Any(x => x.Title.ToUpper().Equals(objDocumentSplitItemEntity.EntityDisplayName.ToUpper())))
                    {
                        var headerEntity = lsDocumentHeaderEntitites.Where(x => x.Title.ToUpper().Equals(objDocumentSplitItemEntity.EntityDisplayName.ToUpper()));
                        if (headerEntity != null && headerEntity.Count() > 0)
                        {
                            objDocumentSplitItemEntity.SplitAccountingFieldValue = headerEntity.FirstOrDefault().EntityDetailCode.ToString();
                            objDocumentSplitItemEntity.EntityCode = headerEntity.FirstOrDefault().EntityCode.ToString();
                        }
                        continue;
                    }
                    //Source System Entity defaulting
                    if (objDocumentSplitItemEntity.EntityTypeId == SourceSystemEntityId)
                    {
                        objDocumentSplitItemEntity.SplitAccountingFieldValue = SourceSystemEntityDetailCode.ToString();
                        //Set the Entity Code
                        var dataList = AccountingDataList[objDocumentSplitItemEntity.SplitAccountingFieldId];
                        if (dataList != null && dataList.Count() > 0)
                        {
                            var CodeList = dataList.Where(x => x.EntityDetailCode.Equals(SourceSystemEntityDetailCode));
                            if (CodeList != null && CodeList.Any())
                            {
                                objDocumentSplitItemEntity.EntityCode = CodeList.FirstOrDefault().EntityCode;
                            }
                        }
                        continue;
                    }
                    //Mandatory Field Validations
                    if (objDocumentSplitItemEntity.IsMandatory && (objDocumentSplitItemEntity.EntityCode.Trim().Equals(string.Empty) || objDocumentSplitItemEntity.EntityCode.Trim().Equals("0")))
                    {
                        errorList.AppendLine(objDocumentSplitItemEntity.EntityDisplayName + " is a mandatory Entity in Split Number " + splitNumber.ToString());
                        lstAccountingErrors.Add(new RequisitionSplitItemAccountingErrors { SplitNumber = splitNumber, FieldName = objDocumentSplitItemEntity.EntityDisplayName.Replace(" ", ""), FieldValue = objDocumentSplitItemEntity.EntityCode });
                        result = false;
                    }
                    //Code Validations start Here
                    List<SplitAccountingFields> lstKVP = null;

                    if (IsExcludeMasterDataFromExcelTemplate)
                    {
                        if (lsDocumentHeaderEntitites.Where(x => !x.SplitAccountingFieldId.Equals(objDocumentSplitItemEntity.SplitAccountingFieldId)).Any())
                        {
                            if (!((objDocumentSplitItemEntity.EntityCode.Trim().Equals(string.Empty) || objDocumentSplitItemEntity.EntityCode.Trim().Equals("0"))))
                            {
                                if (AccountingDataList.ContainsKey(objDocumentSplitItemEntity.SplitAccountingFieldId))
                                {
                                    lstKVP = AccountingDataList[objDocumentSplitItemEntity.SplitAccountingFieldId];
                                }
                                if (lstKVP != null && lstKVP.Count() > 0)
                                {
                                    var CodeList = lstKVP.Where(x => x.EntityDisplayName.Trim().Equals(objDocumentSplitItemEntity.EntityCode.Trim()) || x.EntityCode.Trim().Equals(objDocumentSplitItemEntity.EntityCode));
                                    if (CodeList != null && !CodeList.Any())
                                    {
                                        errorList.AppendLine(objDocumentSplitItemEntity.EntityDisplayName + "  has invalid Code Value in Split Number " + splitNumber.ToString());
                                        lstAccountingErrors.Add(new RequisitionSplitItemAccountingErrors { SplitNumber = splitNumber, FieldName = objDocumentSplitItemEntity.EntityDisplayName.Replace(" ", ""), FieldValue = objDocumentSplitItemEntity.EntityCode });
                                        result = false;
                                    }
                                    else
                                    {
                                        objDocumentSplitItemEntity.SplitAccountingFieldValue = (CodeList != null) ? CodeList.FirstOrDefault().EntityDetailCode.ToString() : string.Empty;
                                        objDocumentSplitItemEntity.EntityCode = (CodeList != null) ? CodeList.FirstOrDefault().EntityCode : string.Empty;
                                    }
                                }
                                else
                                {
                                    errorList.AppendLine(objDocumentSplitItemEntity.EntityDisplayName + "  has invalid Code Value in Split Number " + splitNumber.ToString());
                                    lstAccountingErrors.Add(new RequisitionSplitItemAccountingErrors { SplitNumber = splitNumber, FieldName = objDocumentSplitItemEntity.EntityDisplayName.Replace(" ", ""), FieldValue = objDocumentSplitItemEntity.EntityCode });
                                    result = false;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (AccountingDataList.ContainsKey(objDocumentSplitItemEntity.SplitAccountingFieldId))
                        {
                            lstKVP = AccountingDataList[objDocumentSplitItemEntity.SplitAccountingFieldId];
                            if (!((objDocumentSplitItemEntity.EntityCode.Trim().Equals(string.Empty) || objDocumentSplitItemEntity.EntityCode.Trim().Equals("0"))))
                            {
                                if (lstKVP != null && lstKVP.Count() > 0)
                                {
                                    var CodeList = lstKVP.Where(x => x.EntityDisplayName.Trim().Equals(objDocumentSplitItemEntity.EntityCode.Trim()) || x.EntityCode.Trim().Equals(objDocumentSplitItemEntity.EntityCode));
                                    if (CodeList != null && !CodeList.Any())
                                    {
                                        errorList.AppendLine(objDocumentSplitItemEntity.EntityDisplayName + "  has invalid Code Value in Split Number " + splitNumber.ToString());
                                        lstAccountingErrors.Add(new RequisitionSplitItemAccountingErrors { SplitNumber = splitNumber, FieldName = objDocumentSplitItemEntity.EntityDisplayName.Replace(" ", ""), FieldValue = objDocumentSplitItemEntity.EntityCode });
                                        result = false;
                                    }
                                    else
                                    {
                                        objDocumentSplitItemEntity.SplitAccountingFieldValue = (CodeList != null) ? CodeList.FirstOrDefault().EntityDetailCode.ToString() : string.Empty;
                                        objDocumentSplitItemEntity.EntityCode = (CodeList != null) ? CodeList.FirstOrDefault().EntityCode : string.Empty;
                                    }
                                }
                            }
                        }
                    }

                    //Requester Validation
                    if (objDocumentSplitItemEntity.EntityDisplayName.ToUpper().Trim().Equals("REQUESTER"))
                    {
                        //if Requester is Empty. Set Requester as currect user or OBO user
                        if ((objDocumentSplitItemEntity.EntityCode.Trim().Equals(string.Empty) || objDocumentSplitItemEntity.EntityCode.Trim().Equals("0")))
                        {
                            if (oboUserContactCode > 0)
                            {
                                objDocumentSplitItemEntity.SplitAccountingFieldValue = oboUserContactCode.ToString();
                                objDocumentSplitItemEntity.EntityCode = oboUserContactCode.ToString();
                            }
                            else
                            {
                                objDocumentSplitItemEntity.SplitAccountingFieldValue = userContext.ContactCode.ToString();
                                objDocumentSplitItemEntity.EntityCode = userContext.ContactCode.ToString();
                            }
                        }
                    }
                    //if Period Defaulting based on Need By Date is False, Then PEriod Defaulting should be based on Current Date.
                    if (objDocumentSplitItemEntity.EntityDisplayName.ToUpper().Trim().Equals("PERIOD") && !IsPeriodbasedonNeedbyDate)
                    {
                        //if Period is Empty. Set Default Period
                        if (lstPeriodData != null && lstPeriodData.Count() > 0)
                        {
                            var calenderPeriodObj = lstPeriodData.FirstOrDefault();
                            var periodId = !(ReferenceEquals(calenderPeriodObj, null)) ? calenderPeriodObj.PeriodId : 0;
                            objDocumentSplitItemEntity.SplitAccountingFieldValue = (periodId != 0) ? Convert.ToString(periodId) : string.Empty;
                            objDocumentSplitItemEntity.EntityCode = calenderPeriodObj.PeriodCode;
                        }
                    }

                }

                if (lstSplitItems.Count() > 1)
                {
                    RequisitionItemSplitErrorResponse objErrorResponse = new RequisitionItemSplitErrorResponse();
                    if (errorListSplits != null && errorListSplits.Length > 0 && splitNumber == 1)
                        objErrorResponse.Message = errorListSplits.ToString() + "\r\n";

                    objErrorResponse.Message = objErrorResponse.Message + errorList.ToString();

                    objErrorResponse.MultipleSplitAccountingErrors = lstAccountingErrors;
                    objErrorResponse.LineNumber = lineNumber;
                    requisitionItemErrorResponse.RequisitionMultipleItemSplitMapping.Add(objErrorResponse);

                }
                else if (lstSplitItems.Count() == 1)
                {
                    if (errorListSplits != null && errorListSplits.Length > 0)
                        requisitionItemErrorResponse.Message = errorListSplits.ToString() + "\r\n" + errorList.ToString();

                    if (requisitionItemErrorResponse.Message != null && requisitionItemErrorResponse.Message.Length > 0)
                        requisitionItemErrorResponse.Message = requisitionItemErrorResponse.Message + errorList.ToString();
                    else
                        requisitionItemErrorResponse.Message = errorList.ToString();

                    requisitionItemErrorResponse.SingleSplitAccountingErrors = lstAccountingErrors;
                }

            }

            return result;

        }

        private bool ValidateSplitItemDetails(RequisitionItem objReqItem, RequisitionItemErrorResponse requisitionItemErrorResponse, StringBuilder errorList)
        {
            bool result = true;
            requisitionItemErrorResponse.SplitItemFieldMapping = new List<RequisitionAccountingSplitColumn>();

            //Split Type Validation
            if (objReqItem.ItemSplitsDetail.Where(x => x.SplitType == SplitType.None).Distinct().Count() > 0)
            {
                errorList.AppendLine("Split Type is Mandatory");
                requisitionItemErrorResponse.SplitItemFieldMapping.Add(RequisitionAccountingSplitColumn.SplitType);
                result = false;
            }
            else if (objReqItem.ItemSplitsDetail.Select(x => x.SplitType).Distinct().Count() > 1)
            {
                errorList.AppendLine("Split Type should be consistent for all splits");
                requisitionItemErrorResponse.SplitItemFieldMapping.Add(RequisitionAccountingSplitColumn.SplitType);
                result = false;
            }

            // Validate Sum of Percentage should be Equal 100 is Split Type is Percentage

            if (objReqItem.ItemSplitsDetail != null && objReqItem.ItemSplitsDetail.Count > 0 && objReqItem.ItemSplitsDetail[0].SplitType == SplitType.Percentage)
            {
                if (objReqItem.ItemSplitsDetail != null && objReqItem.ItemSplitsDetail.Count() > 0 && objReqItem.ItemSplitsDetail[0].SplitType == SplitType.Percentage
                    && ((objReqItem.ItemSplitsDetail.Sum(x => x.Percentage) > 100) || (objReqItem.ItemSplitsDetail.Sum(x => x.Percentage) < 100)))
                {
                    errorList.AppendLine("Sum of Split Item(s) Percentage should be equal to 100%");
                    requisitionItemErrorResponse.SplitItemFieldMapping.Add(RequisitionAccountingSplitColumn.SplitValue);
                    result = false;
                }
                else
                    objReqItem.SplitType = objReqItem.ItemSplitsDetail[0].SplitType;
            }

            if (objReqItem.ItemSplitsDetail != null && objReqItem.ItemSplitsDetail.Count > 0 && objReqItem.ItemSplitsDetail[0].SplitType == SplitType.Quantity)
            {
                // Validate Sum of Quantity should be Equal to Line Quantity
                if (objReqItem.ItemExtendedType == ItemExtendedType.Fixed)
                {
                    if (objReqItem.UnitPrice != objReqItem.ItemSplitsDetail.Sum(x => x.Quantity))
                    {
                        errorList.AppendLine("Sum of Split Item(s) quantity Should be equal to Unit Price of Fixed Service Line ");
                        requisitionItemErrorResponse.SplitItemFieldMapping.Add(RequisitionAccountingSplitColumn.SplitValue);
                        result = false;
                    }
                }
                else
                {
                    if (objReqItem.Quantity != objReqItem.ItemSplitsDetail.Sum(x => x.Quantity))
                    {
                        errorList.AppendLine("Sum of Split Item(s) quantity is not equal to Line quantity");
                        requisitionItemErrorResponse.SplitItemFieldMapping.Add(RequisitionAccountingSplitColumn.SplitValue);
                        result = false;
                    }
                }
            }

            if (objReqItem.ItemSplitsDetail.Any(t => t.DocumentItemId <= 0) && objReqItem.ItemSplitsDetail.Count > 1)
            {
                errorList.AppendLine("Line Number is a Mandatory Field.");
                requisitionItemErrorResponse.SplitItemFieldMapping.Add(RequisitionAccountingSplitColumn.LineReferenceNumber);
                result = false;
            }
            return result;
        }

        private void LoadValidationMasterData()
        {

            LoadMasterDataList();
            GetPeriodDateForAutoSuggest();
            //ReLoad the Accounting Code Validation List
            if (dataListSplitAccountingFields != null && dataListSplitAccountingFields.Count() > 0)
            {
                AccountingDataList = dataListSplitAccountingFields.GroupBy(x => x.SplitAccountingFieldId).ToDictionary(x => x.Key, x => x.Select(obj => obj).ToList());
            }

        }
        public bool Taxcodelinelevelchecking()
        {
            try
            {
                if (lsDocumentHeaderEntitites != null && lsDocumentHeaderEntitites.Count > 0)
                {
                    LOBEntityDetailCode = lsDocumentHeaderEntitites[0].LOBEntityDetailCode;
                    commonManager = new RequisitionCommonManager(this.JWTToken) { UserContext = this.userContext, GepConfiguration = RequisitionManger.GepConfiguration };
                    int EntityMappedtoTAX = Convert.ToInt32(commonManager.GetSettingsValueByKey(P2PDocumentType.Requisition, "ENTITYTYPE_TAX", this.userContext.UserId, (int)SubAppCodes.P2P, "", LOBEntityDetailCode));
                    if (EntityMappedtoTAX > 0)
                    {
                        var lstEntityDetails = lsDocumentHeaderEntitites.Where(data => data.EntityTypeId == EntityMappedtoTAX);
                        if (lstEntityDetails != null && lstEntityDetails.Any())
                        {
                            EntityDetailCode = lstEntityDetails.FirstOrDefault().EntityDetailCode;
                            EntityTypeId = lstEntityDetails.FirstOrDefault().EntityTypeId;

                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occurred in Taxcodelinelevelchecking : " + ex.Message, ex);
            }
            return false;

        }
        private void GetTaxCodeRelatedToShiptTO(RequisitionItem objReqItem, Requisition objdetails, long EntityDetailCode, int EntityTypeId)
        {
            try
            {
                int PriviousShipto = 0;

                if (AllowTaxExempt)
                {
                    if ((objReqItem.ItemType == ItemType.Service && DefaultServiceTaxExempt) || (objReqItem.ItemType == ItemType.Material && DefaultMaterialTaxExempt))
                    {
                        objReqItem.Tax = 0;
                        objReqItem.taxItems = new List<Taxes>();
                    }
                }
                if (objReqItem.ShipToLocationId > 0 && !AllowTaxExempt)
                {
                    DataTable TaxDetailsBasedOnShipTo = new DataTable();
                    List<Taxes> objtax = new List<Taxes>();
                    decimal totalTaxPercentage = 0;
                    int totalChargeForTaxes = 0;
                    if (PriviousShipto != objReqItem.ShipToLocationId)
                    {
                        objReqItem.Tax = 0;
                        TaxDetailsBasedOnShipTo = new DataTable();
                        objtax = new List<Taxes>();
                        PriviousShipto = objReqItem.ShipToLocationId;
                        TaxDetailsBasedOnShipTo = commonManager.GetTaxItemsByEntityID(PriviousShipto, EntityDetailCode, EntityTypeId);
                        if (TaxDetailsBasedOnShipTo != null && TaxDetailsBasedOnShipTo.Rows != null && TaxDetailsBasedOnShipTo.Rows.Count > 0)
                        {
                            for (int i = 0; i < TaxDetailsBasedOnShipTo.Rows.Count; i++)
                            {
                                totalTaxPercentage = totalTaxPercentage + Convert.ToDecimal(TaxDetailsBasedOnShipTo.Rows[i][4]);
                                objtax.Add(new Taxes { TaxCode = TaxDetailsBasedOnShipTo.Rows[i][3].ToString(), TaxPercentage = Convert.ToDecimal(TaxDetailsBasedOnShipTo.Rows[i][4]), TaxValue = Convert.ToDecimal(TaxDetailsBasedOnShipTo.Rows[i][4]), TaxDescription = TaxDetailsBasedOnShipTo.Rows[i][2].ToString(), TaxId = Convert.ToInt64(TaxDetailsBasedOnShipTo.Rows[i][0].ToString()) });
                            }
                        }
                    }
                    if (TaxDetailsBasedOnShipTo != null && TaxDetailsBasedOnShipTo.Rows != null && TaxDetailsBasedOnShipTo.Rows.Count > 0)
                    {
                        if (objReqItem.ItemChargesForSubLine != null)
                        {
                            foreach (var itemchange in objReqItem.ItemChargesForSubLine)
                            {
                                if (itemchange.ChargeDetails != null && itemchange.ChargeDetails.IsIncludeForTax == true)
                                {
                                    totalChargeForTaxes += Convert.ToInt32(itemchange.ChargeAmount);
                                };
                            }
                        }
                        objReqItem.Tax = ((totalTaxPercentage * objReqItem.Quantity * ((objReqItem.UnitPrice) + totalChargeForTaxes)) / 100);
                        objReqItem.Tax = Math.Round((decimal)(objReqItem.Tax), maxPrecessionForTaxesAndCharges);
                        objReqItem.taxItems = objtax;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occurred in GetTaxCodeRelatedToShiptTO : " + ex.Message, ex);
            }
        }
        private void CalculateSplitItemTotal(RequisitionItem objReqItem)
        {
            if (objReqItem.ItemSplitsDetail != null && objReqItem.ItemSplitsDetail.Count() > 1)
            {
                #region Auto Calculate Percentage / Quantity based on Split Type 
                int i = 0;
                decimal quantitySum = 0;
                decimal temp = decimal.Zero;
                decimal diff = decimal.Zero;
                foreach (RequisitionSplitItems splititem in objReqItem.ItemSplitsDetail)
                {
                    i++;
                    if (objReqItem.SplitType == SplitType.Percentage)
                    {
                        // Calculate quantity for splits
                        if (objReqItem.ItemExtendedType == ItemExtendedType.Fixed)
                        {
                            temp = objReqItem.UnitPrice.HasValue ? objReqItem.UnitPrice.Value / objReqItem.ItemSplitsDetail.Count() : 1;
                        }
                        else
                        {
                            temp = objReqItem.Quantity * splititem.Percentage / 100;
                        }
                        temp = Math.Round(temp, maxPrecessionForTaxesAndCharges);
                        quantitySum = quantitySum + temp;
                        if (i == objReqItem.ItemSplitsDetail.Count)
                        {
                            if (objReqItem.ItemExtendedType == ItemExtendedType.Fixed)
                                diff = objReqItem.UnitPrice.Value - quantitySum;
                            else
                                diff = objReqItem.Quantity - quantitySum;

                            if (diff != 0)
                            {
                                // Quantity is greater than quantitysum add difference to last split
                                temp = temp + diff;
                            }
                        }
                        splititem.Quantity = temp;
                    }
                    else // split type Quantity
                    {
                        splititem.Percentage = splititem.Quantity * 100 / ((objReqItem.ItemExtendedType == ItemExtendedType.Fixed) ? objReqItem.UnitPrice.Value : objReqItem.Quantity);
                    }
                }
                #endregion

                decimal itemTotal = objReqItem.Quantity * (objReqItem.UnitPrice.HasValue ? objReqItem.UnitPrice.Value : 1)
                                    + (objReqItem.Tax.HasValue ? objReqItem.Tax.Value : 0)
                                    + (objReqItem.AdditionalCharges.HasValue ? objReqItem.AdditionalCharges.Value : 0)
                                    + (objReqItem.ShippingCharges.HasValue ? objReqItem.ShippingCharges.Value : 0);

                int j = 0;
                #region Auto Calculate Shipping / Tax / Other Charges based on Percentage
                foreach (RequisitionSplitItems splitItem in objReqItem.ItemSplitsDetail)
                {
                    j++;

                    //Calculate  Shipping Charges For each split.Service line item will not have shipping charges 
                    temp = objReqItem.ShippingCharges.HasValue ? (objReqItem.ShippingCharges.Value * splitItem.Percentage) / 100 : 0;
                    temp = Math.Round(temp, maxPrecessionForTaxesAndCharges);
                    if (j == objReqItem.ItemSplitsDetail.Count)
                    {
                        temp = Math.Round(temp * 100, maxPrecessionForTaxesAndCharges);
                        temp = 0.01M * Math.Ceiling(temp);
                    }
                    splitItem.ShippingCharges = temp;

                    //Calculate Tax For each Split 
                    temp = objReqItem.Tax.HasValue ? (objReqItem.Tax.Value * splitItem.Percentage) / 100 : 0;
                    temp = Math.Round(temp, maxPrecessionForTaxesAndCharges);
                    if (j == objReqItem.ItemSplitsDetail.Count)
                    {
                        temp = Math.Round(temp * 100, maxPrecessionForTaxesAndCharges);
                        temp = 0.01M * Math.Ceiling(temp);
                    }
                    splitItem.Tax = temp;

                    // Calculate other charges 
                    temp = objReqItem.AdditionalCharges.HasValue ? (objReqItem.AdditionalCharges.Value * splitItem.Percentage) / 100 : 0;
                    temp = Math.Round(temp, maxPrecessionForTaxesAndCharges);
                    if (j == objReqItem.ItemSplitsDetail.Count)
                    {
                        temp = Math.Round(temp * 100, maxPrecessionForTaxesAndCharges);
                        temp = 0.01M * Math.Ceiling(temp);
                    }
                    splitItem.AdditionalCharges = temp;

                    //Calculate SplitItemTotal
                    temp = Math.Round((itemTotal * splitItem.Percentage) / 100, maxPrecessionforTotal);
                    if (j == objReqItem.ItemSplitsDetail.Count)
                    {
                        temp = Math.Round(temp * 100, maxPrecessionforTotal);
                        temp = 0.01M * Math.Ceiling(temp);
                    }
                    splitItem.SplitItemTotal = temp;
                }
                #endregion
            }
            else
            {

                //Single Split Calculations
                if (objReqItem.ItemSplitsDetail != null && objReqItem.ItemSplitsDetail.Count() == 1)
                {
                    objReqItem.ItemSplitsDetail[0].Tax = Math.Round((decimal)(objReqItem.Tax), maxPrecessionForTaxesAndCharges);
                    objReqItem.ItemSplitsDetail[0].AdditionalCharges = Math.Round((decimal)(objReqItem.AdditionalCharges), maxPrecessionForTaxesAndCharges);
                    objReqItem.ItemSplitsDetail[0].ShippingCharges = Math.Round((decimal)(objReqItem.ShippingCharges), maxPrecessionForTaxesAndCharges);
                    objReqItem.ItemSplitsDetail[0].Quantity = objReqItem.ItemExtendedType == ItemExtendedType.Fixed ? Convert.ToDecimal(objReqItem.UnitPrice) : objReqItem.Quantity;
                    objReqItem.ItemSplitsDetail[0].SplitItemTotal = Math.Round((decimal)((objReqItem.ItemExtendedType == ItemExtendedType.Fixed ? objReqItem.UnitPrice : objReqItem.UnitPrice * objReqItem.ItemSplitsDetail[0].Quantity) + objReqItem.ItemSplitsDetail[0].AdditionalCharges + objReqItem.ItemSplitsDetail[0].ShippingCharges + objReqItem.ItemSplitsDetail[0].Tax), maxPrecessionforTotal);
                }
            }
        }

        private void CalculateQuantityForFixeditems(RequisitionItem objReqItem, decimal? oldUnitprice)
        {
            if (objReqItem.ItemExtendedType == ItemExtendedType.Fixed && objReqItem.ItemSplitsDetail != null && objReqItem.ItemSplitsDetail.Count() > 1)
            {
                foreach (RequisitionSplitItems splititem in objReqItem.ItemSplitsDetail)
                {
                    if (splititem.SplitType == SplitType.Quantity)
                    {
                        splititem.Quantity = Convert.ToDecimal(splititem.Quantity * objReqItem.UnitPrice.Value / (oldUnitprice.HasValue ? oldUnitprice : 1));
                    }
                }

            }
            else if (objReqItem.ItemExtendedType == ItemExtendedType.Fixed && objReqItem.ItemSplitsDetail != null && objReqItem.ItemSplitsDetail.Count() == 1)
            {
                objReqItem.ItemSplitsDetail[0].Quantity = Convert.ToDecimal(objReqItem.UnitPrice);
            }

        }

        private void UpdatePeriodbyNeedbyDate(long documentItemId)
        {
            objDocBO.UpdatePeriodbyNeedbyDate(P2PDocumentType.Requisition, documentItemId);
        }

        private bool ValidateUniqueLineNumber(RequisitionItem item, RequisitionItemErrorResponse requisitionItemErrorResponse, StringBuilder errorList)
        {
            bool result = true;
            var duplicateLineNumbers = lstRequisitionItems.GroupBy(x => x.ItemLineNumber).ToDictionary(x => x.Key, x => x.Count()).Where(y => y.Value > 1);
            if (duplicateLineNumbers != null && duplicateLineNumbers.Count() > 0)
            {

                if (duplicateLineNumbers.Any((x => x.Key == item.ItemLineNumber && item.ItemLineNumber > 0)))
                {
                    errorList.AppendLine("Line Reference Number is duplicate. Enter the valid line reference number that is unique across all lines");
                    requisitionItemErrorResponse.FieldMapping.Add(RequisitionExcelItemColumn.LineReferenceNumber);
                    result = false;
                }

            }
            return result;
        }

        private long GetOBOUser()
        {
            return objReqManager.GetOBOUserByRequisitionID(documentCode);
        }
        private void GetPeriodDateForAutoSuggest()
        {
            OperationalBudgetManager objBudgetBO = new OperationalBudgetManager() { UserContext = this.userContext, GepConfiguration = RequisitionManger.GepConfiguration };
            lstPeriodData = objBudgetBO.GetPeriodDataForAutoSuggest();
        }
        private bool ValidateItemBandingAndQty(RequisitionItem item, RequisitionItem catalogItem, StringBuilder errStrBuilder, RequisitionItemErrorResponse requisitionItemErrorResponse)
        {
            bool result = errStrBuilder.Length > 0 ? false : true;
            bool flag = false;
            UserExecutionContext userExecutionContext = UserContext;
            var partnerHelper = new RESTAPIHelper.PartnerHelper(this.UserContext, JWTToken);
            lstUserActivities = partnerHelper.GetUserActivitiesByContactCode(userExecutionContext.ContactCode, userExecutionContext.BuyerPartnerCode).Split(',').ToList();

            foreach (var activity in lstUserActivities)
            {
                if (activity == ALLOW_BUYERS_TO_EDIT_CATALOG_PRICE_AND_QUANTITY_RESTRICTIONS || activity == Allow_Requestors_To_Edit_Catalogprice_And_Quantityrestrictions)
                {
                    flag = true;
                }
            }

            if (!flag)
            {
                if (catalogItem.MaximumOrderQuantity > 0 && item.Quantity > catalogItem.MaximumOrderQuantity)
                {
                    errStrBuilder.AppendLine("The quantity should be less than or equal to " + catalogItem.MaximumOrderQuantity.ToString());
                    requisitionItemErrorResponse.FieldMapping.Add(RequisitionExcelItemColumn.QuantityEffort);
                    result = false;
                }
                if (catalogItem.MinimumOrderQuantity > 0 && item.Quantity < catalogItem.MinimumOrderQuantity)
                {
                    errStrBuilder.AppendLine("The quantity should be greater than or equal to " + catalogItem.MinimumOrderQuantity.ToString());
                    requisitionItemErrorResponse.FieldMapping.Add(RequisitionExcelItemColumn.QuantityEffort);
                    result = false;
                }
                if (catalogItem.Banding > 0 && item.Quantity % catalogItem.Banding != 0)
                {
                    errStrBuilder.AppendLine("The quantity should be in multiples of  " + catalogItem.Banding.ToString());
                    requisitionItemErrorResponse.FieldMapping.Add(RequisitionExcelItemColumn.QuantityEffort);
                    result = false;
                }

            }
            return result;
        }

        private bool ValidateLineNumberExisitence(long lineNumber)
        {
            if (lstAvailableLines.Contains(lineNumber.ToString()))
                return true;
            else
                return false;
        }

        private void GetRequisitionDetails()
        {
            objReqDetails = GetRequisitionDetailsForBulkUploadReqLines(documentCode);
            if (objReqDetails != null)
            {
                lstAvailableLines = objReqDetails.LineNumbers.Split(',').ToList<string>();
                oboUserContactCode = objReqDetails.OnBehalfOf;
            }
        }

        private void setTransmissionMode(ref RequisitionItem reqItem)
        {
            /* to get the default dispatch mode  partner  */
            UserExecutionContext userExecutionContext = this.userContext;
            ProxyPartnerService proxy = new ProxyPartnerService(userExecutionContext, this.JWTToken);
            List<PartnerDocumetInterfaceInfo> partnerInterfaceDetails = new List<PartnerDocumetInterfaceInfo>();
            IdAndName defaultDisptachMode = new IdAndName() { id = 1, name = "Portal" };

            long PartnerCode = (long)reqItem.PartnerCode;
            long LocationId = (long)reqItem.OrderLocationId;
            if (partnerInterfaceDictionary.Count > 0)
            {
                if (MapDispatchModetoOrderingLocation)
                {
                    if (partnerInterfaceDictionary.ContainsKey(LocationId))
                        defaultDisptachMode.id = GetTransmissionModeIdByInterFaceTypeId(partnerInterfaceDictionary[LocationId]);
                }
                else
                {
                    if (partnerInterfaceDictionary.ContainsKey(PartnerCode))
                        defaultDisptachMode.id = GetTransmissionModeIdByInterFaceTypeId(partnerInterfaceDictionary[PartnerCode]);
                }

            }
            reqItem.TrasmissionMode = Convert.ToInt32(defaultDisptachMode.id);
        }
        private void setReqSupplierContactDetails(bool isPartnerBasedOnOrderingLoc, ref RequisitionItem reqItem)
        {
            /* to get the default dispatch value for disptachmode as Email  partner  */
            List<ContactInfo> partnerContacts = new List<ContactInfo>();
            long PartnerCode = (long)reqItem.PartnerCode;
            long OrderingLocationId = reqItem.OrderLocationId;

            if (partnerContactDict.ContainsKey(PartnerCode))
            {
                partnerContacts = partnerContactDict[PartnerCode];
            }
            else
            {
                DataTable tblFiltered = new DataTable();
                if (isPartnerBasedOnOrderingLoc && OrderingLocationId > 0)
                {
                    DataView dv = new DataView(partnerDocInterfaceDic.Tables[1]);
                    dv.RowFilter = "PartnerCode = " + PartnerCode + " and locationId=" + OrderingLocationId;
                    tblFiltered = dv.ToTable();
                    if (tblFiltered.Rows.Count == 0)
                    {
                        dv = new DataView(partnerDocInterfaceDic.Tables[1]);
                        dv.RowFilter = "PartnerCode = " + PartnerCode;
                        tblFiltered = dv.ToTable();
                    }
                }
                else
                {
                    DataView dv = new DataView(partnerDocInterfaceDic.Tables[1]);
                    dv.RowFilter = "PartnerCode = " + PartnerCode;
                    tblFiltered = dv.ToTable();

                }

                foreach (DataRow item in tblFiltered.Rows)
                {
                    partnerContacts.Add(new ContactInfo()
                    {
                        ContactCode = Convert.ToInt64(item["ContactCode"]),
                        Name = item["Name"].ToString(),
                        EmailID = item["EmailId"].ToString(),
                        ContactNumber = item["ContactNumber"].ToString(),
                        IsPrimary = Convert.ToBoolean(item["IsPrimary"])

                    });
                }
                partnerContactDict.Add((long)reqItem.PartnerCode, partnerContacts);
            }

            if (isPartnerBasedOnOrderingLoc)
            {
                //  If only one contact set default
                if (partnerContacts.Count == 1)
                {
                    reqItem.TransmissionValue = partnerContacts[0].EmailID;
                }

            }
            else
            {
                // set Primary contact 
                foreach (var partnerContact in partnerContacts)
                {
                    if (partnerContact.IsPrimary)
                    {
                        reqItem.TransmissionValue = partnerContact.EmailID;
                        break;
                    }
                }


            }

        }
        private void setTaxJurisdictionBasedOnShipTo(ref RequisitionItem reqItem)
        {
            long shipToId = (long)reqItem.ShipToLocationId;
            if (taxJurisdictionDictionary.Count > 0)
            {
                if (taxJurisdictionDictionary.ContainsKey(shipToId))
                    reqItem.TaxJurisdiction = taxJurisdictionDictionary[shipToId];
            }
        }
        public void GetTaxJurisdictionBasedOnShipTo()
        {
            List<long> shipToLocationIds = new List<long>();
            List<TaxJurisdiction> taxJurisdictions = new List<TaxJurisdiction>();
            long shipToId = 0;
            foreach (var item in lstRequisitionItems)
            {
                ShiptoLocation objShipTo = new ShiptoLocation();
                if (item.DocumentItemShippingDetails != null && item.DocumentItemShippingDetails.Count() > 0)
                    objShipTo = item.DocumentItemShippingDetails[0].ShiptoLocation;
                if (objShipTo != null)
                {
                    if (!string.IsNullOrEmpty(objShipTo.ShiptoLocationName))
                    {
                        if (lstShipToLocations != null && lstShipToLocations.Count() > 0)
                        {
                            var shipToLocationList = lstShipToLocations.Where(x => x.Name.Equals(objShipTo.ShiptoLocationName.Trim()) || x.Value.Equals(objShipTo.ShiptoLocationName.Trim()));
                            if (shipToLocationList != null && shipToLocationList.Any())
                                shipToId = (shipToLocationList != null) ? Convert.ToInt32(shipToLocationList.FirstOrDefault().Value) : 0;
                        }
                    }
                    else
                    {
                        if (objReqDetails != null && objReqDetails.ShiptoLocation != null)
                            shipToId = objReqDetails.ShiptoLocation.ShiptoLocationId;
                    }
                }
                if (shipToId > 0 && !shipToLocationIds.Contains(shipToId))
                    shipToLocationIds.Add(shipToId);
            }
            if (shipToLocationIds.Count > 0)
            {
                taxJurisdictions = GetTaxJurisdictionsData(shipToLocationIds);
                if (taxJurisdictions.Count > 0)
                {
                    foreach (var response in taxJurisdictions)
                    {
                        if (!taxJurisdictionDictionary.ContainsKey(response.ShiptoLocationId))
                            taxJurisdictionDictionary.Add(response.ShiptoLocationId, response.JurisdictionCode + "-" + response.JurisdictionName);
                    }
                }
            }
        }
        private List<TaxJurisdiction> GetTaxJurisdictionsData(List<long> shipToIds)
        {
            List<TaxJurisdiction> taxJurisdictionsResponse = new List<TaxJurisdiction>();
            string GetTaxJurisdictionsURl = "";
            try
            {
                string AppURL = MultiRegionConfig.GetConfig(CloudConfig.AppURL);
                string input = string.Join(",", shipToIds);
                GetTaxJurisdictionsURl = string.Concat(AppURL + URLs.GetTaxJurisdictions, "&shipToLocationIds=", input.ToString()); ;
                Req.BusinessObjects.RESTAPIHelper.RequestHeaders requestHeaders = new Req.BusinessObjects.RESTAPIHelper.RequestHeaders();
                string appName = Environment.GetEnvironmentVariable("NewRelic.AppName");
                string useCase = "TemplateUploader-GetTaxJurisdictions";
                requestHeaders.Set(this.userContext, JWTToken);
                var webAPI = new GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.WebAPI(requestHeaders, appName, useCase);
                var JsonResult = webAPI.ExecuteGet(GetTaxJurisdictionsURl);
                taxJurisdictionsResponse = JsonConvert.DeserializeObject<List<TaxJurisdiction>>(JsonResult);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, string.Format("Error occured in GetTaxJurisdictionsData Method in RequisitionTemplateUploader for URL = {0}", GetTaxJurisdictionsURl), ex);
                throw ex;
            }

            return taxJurisdictionsResponse;
        }
        public void GetTransmissionModeBasedOnSetting(List<RequisitionItem> reqItems)
        {
            /* to get the default dispatch mode and disptach value select   list of partnerts  */
            List<long> partnerCodes = new List<long>();
            List<PartnerLocation> lstPartnerLocations = new List<PartnerLocation>();
            List<PartnerLocationTransactionInfo> partnerLocationTransactions = new List<PartnerLocationTransactionInfo>();
            foreach (var item in reqItems)
            {
                if (lstSuppliers != null && lstSuppliers.Count() > 0 && !string.IsNullOrEmpty(item.PartnerName))
                {
                    var supplierList = lstSuppliers.Where(x => x.Name.Equals(item.PartnerName.Trim()) || x.Value.Equals(item.PartnerName.Trim()));
                    if (supplierList != null && supplierList.Any())
                    {
                        PartnerLocation partnerLocation = new PartnerLocation();
                        long PartnerCode = (supplierList != null) ? supplierList.FirstOrDefault().Id : 0;
                        if (PartnerCode > 0 && !partnerCodes.Contains(PartnerCode))
                        {
                            partnerCodes.Add(PartnerCode);
                        }
                        if (!lstPartnerLocations.Any(x => x.PartnerCode == PartnerCode) && !MapDispatchModetoOrderingLocation)
                        {
                            partnerLocation.PartnerCode = PartnerCode;
                            lstPartnerLocations.Add(partnerLocation);
                        }
                        if (lstSuppliersOrderLocations != null && lstSuppliersOrderLocations.Count() > 0 && !string.IsNullOrEmpty(item.OrderLocationName) && PartnerCode > 0 && MapDispatchModetoOrderingLocation)
                        {
                            var orderLocList = lstSuppliersOrderLocations.Where(x => (x.LocationName.Equals(item.OrderLocationName.Trim()) || x.ClientLocationCode.Equals(item.OrderLocationName.Trim())) && x.PartnerCode == PartnerCode);
                            if (orderLocList != null && orderLocList.Any())
                            {
                                long OrderLocationId = (orderLocList != null) ? orderLocList.FirstOrDefault().LocationId : 0;
                                if (!lstPartnerLocations.Any(x => x.LocationId == OrderLocationId) && OrderLocationId > 0)
                                {
                                    partnerLocation.LocationId = OrderLocationId;
                                    partnerLocation.PartnerCode = PartnerCode;
                                    lstPartnerLocations.Add(partnerLocation);
                                }
                            }
                        }
                    }
                }
            }
            if (lstPartnerLocations.Count > 0)
            {
                partnerDocInterfaceDic = RequisitionManger.GetPartnersDocumetInterfaceInfo(partnerCodes);
                partnerLocationTransactions = GetTransmissionModeData(lstPartnerLocations, MapDispatchModetoOrderingLocation);
                if (partnerLocationTransactions != null && partnerLocationTransactions.Count > 0)
                {
                    foreach (var response in partnerLocationTransactions)
                    {
                        if (MapDispatchModetoOrderingLocation)
                        {
                            if (!partnerInterfaceDictionary.ContainsKey(response.LocationId))
                                partnerInterfaceDictionary.Add(response.LocationId, response.InterfaceTypeId);
                        }
                        else
                        {
                            if (!partnerInterfaceDictionary.ContainsKey(response.PartnerCode))
                                partnerInterfaceDictionary.Add(response.PartnerCode, response.InterfaceTypeId);
                        }
                    }
                }
            }
        }
        private List<PartnerLocationTransactionInfo> GetTransmissionModeData(List<PartnerLocation> partnerLocations, bool isFromLocation)
        {
            List<PartnerLocationTransactionInfo> transmissionModeDetails = new List<PartnerLocationTransactionInfo>();
            string AppURL = MultiRegionConfig.GetConfig(CloudConfig.AppURL);
            string GetTransmissionModeURl = AppURL + URLs.GetTransmissionMode;
            Dictionary<string, object> inputdict = new Dictionary<string, object>();
            inputdict.Add("lstPartnerLocations", partnerLocations);
            inputdict.Add("fetchFromLocationLevel", isFromLocation);
            Req.BusinessObjects.RESTAPIHelper.RequestHeaders requestHeaders = new Req.BusinessObjects.RESTAPIHelper.RequestHeaders();
            string appName = Environment.GetEnvironmentVariable("NewRelic.AppName") ?? "Requisition";
            string useCase = "TemplateUploader-GetTransmissionMode";
            requestHeaders.Set(this.userContext, JWTToken);
            var webAPI = new GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.WebAPI(requestHeaders, appName, useCase);
            var JsonResult = webAPI.ExecutePost(GetTransmissionModeURl, inputdict);
            transmissionModeDetails = JsonConvert.DeserializeObject<List<PartnerLocationTransactionInfo>>(JsonResult);

            return transmissionModeDetails;
        }
        private int GetTransmissionModeIdByInterFaceTypeId(int id)
        {
            switch (id)
            {
                case 1: return 1;
                case 2: return 2;
                case 3: return 2;
                case 4: return 3;
                case 5: return 4;
                case 6: return 5;
                default: return 0;
            }
        }

        private bool GetOverrideExcelUnitPriceBasedOnConfiguration(string sourceType)
        {
            bool OverrideExcelUnitPrice = false;
            string[] itemSourceArray = OverrideUnitPriceForExcelUploadItemSource.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (itemSourceArray.Length > 0)
                OverrideExcelUnitPrice = itemSourceArray.Contains(sourceType);
            return OverrideExcelUnitPrice;
        }
    }


}
