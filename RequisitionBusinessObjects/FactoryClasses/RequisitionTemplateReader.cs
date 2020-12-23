using Gep.Cumulus.CSM.Entities;
using GEP.Cumulus.Impex.FileHelper;
using GEP.Cumulus.Logging;
using GEP.Cumulus.P2P.BusinessEntities;
using GEP.Cumulus.P2P.Req.BusinessObjects.Entities;
using GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper;
using GEP.Platform.FileManagerHelper;
using GEP.SMART.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using ExcelLib = Aspose.Cells;
using FileManagerEntities = GEP.NewP2PEntities.FileManagerEntities;

namespace GEP.Cumulus.P2P.Req.BusinessObjects.FactoryClasses
{
    public class RequisitionTemplateReader : RequisitionTemplateGenerator
    {
        private System.IO.MemoryStream objMemoryStream = null;
        private ExcelLib.License license = null;
        private string fileId = string.Empty;
        protected DataTable dtRequisitionLines = new DataTable();
        protected DataTable dtRequisitionAccounting = new DataTable();
        protected DataTable dtTaxCodes = new DataTable();
        protected DataTable dtTaxCodesList = new DataTable();
        public List<RequisitionItem> lstRequisitionItems = new List<RequisitionItem>();
        protected string documentNumber = string.Empty;
        protected int reqRowCount = 0;
        protected int acctRowCount = 0;
        protected int taxRowCount = 0;
        public List<Taxes> lstDuplicateTaxes = new List<Taxes>();
        public RequisitionTemplateReader(UserExecutionContext userContext, Int64 documentCode, string documentNumber, NewRequisitionManager ReqManger, string fileId, string jwtToken)
            : base(userContext, documentCode, documentNumber, ReqManger, jwtToken,RequisitionExcelTemplateHandler.UploadTemplate)
        {
            this.fileId = fileId;
            ReadExcelTemplate();
            if (IsExcludeMasterDataFromExcelTemplate)
                GetValidSplitAccountingCodes();
        }

        public void ReadExcelTemplate()
        {
            byte[] bytLicenceData;
            bytLicenceData = FileOperations.DownloadByteArrayAsync(blobPath + "/" + asposeLicencePathUrl, MultiRegionConfig.GetConfig(CloudConfig.CloudStorageConn)).GetAwaiter().GetResult();

            objMemoryStream = BytesToStream(bytLicenceData);
            if (objMemoryStream != null)
            {
                license = new ExcelLib.License();
                license.SetLicense(objMemoryStream);
            }

            if (!string.IsNullOrEmpty(fileId))
            {
                var requestHeaders = new RESTAPIHelper.RequestHeaders();
                requestHeaders.Set(this.UserContext, this.JWTToken);
                string appName = Environment.GetEnvironmentVariable("NewRelic.AppName") ?? "Requisition";
                string useCase = "RequisitionTemplateReader-ReadExcelTemplate";
                var serviceURL = MultiRegionConfig.GetConfig(CloudConfig.APIMBaseURL) + "/FileManager/api/V3/FileManager/GetFileDetailsbyFileId/" + fileId;

                var webAPI = new WebAPI(requestHeaders, appName, useCase);
                var response = webAPI.ExecuteGet(serviceURL);
                objFileDetails = JsonConvert.DeserializeObject<FileManagerEntities.DownloadFileDetailsModel>(response);

                objFileDetails.FileData = FileOperations.DownloadByteArrayAsync(objFileDetails.AbsoluteFileUri, MultiRegionConfig.GetConfig(CloudConfig.CloudStorageConn)).GetAwaiter().GetResult();
            }

            #region Download Template from BLOB and read data

            if (objFileDetails.FileData != null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    ms.Write(objFileDetails.FileData, 0, objFileDetails.FileData.Length);
                    ms.Position = 0;
                    ExcelHelper objExcelHelper = new ExcelHelper(bytLicenceData, ms);
                    workbook = new ExcelLib.Workbook(ms);
                    workbook.FileFormat = ExcelLib.FileFormatType.Xlsx;

                    wsRequisitionLines = workbook.Worksheets[EXCELSHEET_REQUISITIONLINES];
                    wsAccountingSplits = workbook.Worksheets[EXCELSHEET_ACCOUNTING];
                    wsTaxCodes = workbook.Worksheets[EXCELSHEET_TAXCODES];


                    dtRequisitionLines = objExcelHelper.ReadDataBySheetName(EXCELSHEET_REQUISITIONLINES, (int)RequisitionExcelTemplateRowStartIndex.RequisitionLinesRowStartIndex, null, null, null, true, true, "dtRequisitionLines");
                    dtRequisitionAccounting = objExcelHelper.ReadDataBySheetName(EXCELSHEET_ACCOUNTING, (int)RequisitionExcelTemplateRowStartIndex.RequisitionAccountingSplitRowStartIndex, null, null, null, true, true, "dtRequisitionAccounting");
                    dtTaxCodes = objExcelHelper.ReadDataBySheetName(EXCELSHEET_TAXCODES, (int)RequisitionExcelTemplateRowStartIndex.RequisitionLinesRowTaxCodesStartIndex, null, null, null, true, true, "dtTaxCodes");
                    dtTaxCodesList = objExcelHelper.ReadDataBySheetName(EXCELSHEET_TAXCODES, (int)RequisitionExcelTemplateRowStartIndex.RequisitionLinesRowTaxCodesStartIndex, null, null, null, true, true, "dtTaxCodesList");


                }

                #region ReadLines


                if (dtRequisitionLines != null && dtRequisitionLines.Rows.Count > 0)
                {
                    //Removing extra space and * character
                    foreach (DataColumn dtItemCol in dtRequisitionLines.Columns)
                    {
                        if (!string.IsNullOrEmpty(dtItemCol.ColumnName))
                        {
                            dtItemCol.ColumnName = FormatColumnName(dtItemCol.ColumnName);
                        }
                    }

                    if (dtRequisitionAccounting != null && dtRequisitionAccounting.Rows.Count > 0)
                    {
                        //Removing extra space and * character
                        foreach (DataColumn dtAcctCol in dtRequisitionAccounting.Columns)
                        {
                            if (!string.IsNullOrEmpty(dtAcctCol.ColumnName))
                            {
                                dtAcctCol.ColumnName = FormatColumnName(dtAcctCol.ColumnName);
                            }
                            //Deleting empty rows from table, to minimize extra loops
                            dtRequisitionAccounting.AsEnumerable().Where(rw => string.IsNullOrEmpty(Convert.ToString(rw["LineReferenceNumber"]))).ToList().ForEach(rww => rww.Delete());
                            dtRequisitionAccounting.AcceptChanges();
                        }
                    }

                    if (dtTaxCodes != null && dtTaxCodes.Rows.Count > 0)
                    {
                        //Removing extra space and * character
                        foreach (DataColumn dtTaxCol in dtTaxCodes.Columns)
                        {
                            if (!string.IsNullOrEmpty(dtTaxCol.ColumnName))
                            {
                                dtTaxCol.ColumnName = FormatColumnName(dtTaxCol.ColumnName);
                            }
                        }
                        foreach (DataColumn dtTaxCol in dtTaxCodesList.Columns)
                        {
                            if (!string.IsNullOrEmpty(dtTaxCol.ColumnName))
                            {
                                dtTaxCol.ColumnName = FormatColumnName(dtTaxCol.ColumnName);
                            }
                        }
                        //Deleting empty rows from table, to minimize extra loops
                        dtTaxCodes.AsEnumerable().Where(rw => string.IsNullOrEmpty(Convert.ToString(rw["LineReferenceNumber"]))).ToList().ForEach(rww => rww.Delete());
                        dtTaxCodes.AcceptChanges();

                    }
                    //Deleting empty rows from table, to minimize extra loops
                    dtRequisitionLines.AsEnumerable().Where(rw => string.IsNullOrEmpty(Convert.ToString(rw["Operation"]))).ToList().ForEach(rww => rww.Delete());
                    dtRequisitionLines.AcceptChanges();

                    reqRowCount = dtRequisitionLines.Rows.Count + (int)RequisitionExcelTemplateRowStartIndex.RequisitionLinesRowStartIndex;
                    acctRowCount = dtRequisitionAccounting.Rows.Count + (int)RequisitionExcelTemplateRowStartIndex.RequisitionAccountingSplitRowStartIndex;
                    taxRowCount = dtTaxCodes.Rows.Count + (int)RequisitionExcelTemplateRowStartIndex.RequisitionLinesRowTaxCodesStartIndex;
                    // Loop over and Read
                    foreach (DataRow dr in dtRequisitionLines.Rows)
                    {
                        RequisitionItem objReqItem = new RequisitionItem();
                        objReqItem.OperationType = GetInvoiceFieldValue<RequisitionOperation>(dr, RequisitionExcelItemColumn.Operation);
                        objReqItem.ItemLineNumber = GetInvoiceFieldValue<long>(dr, RequisitionExcelItemColumn.LineReferenceNumber);
                        objReqItem.LineReferenceNumber = GetInvoiceFieldValue<string>(dr, RequisitionExcelItemColumn.LineReferenceNumber);
                        //ship to Location
                        DocumentItemShippingDetail objDocumentItemShippingDetail = new DocumentItemShippingDetail();
                        ShiptoLocation objShiptoLocation = new ShiptoLocation();
                        objShiptoLocation.ShiptoLocationName = GetInvoiceFieldValue<string>(dr, RequisitionExcelItemColumn.ShipToLocation);
                        objDocumentItemShippingDetail.ShiptoLocation = objShiptoLocation;
                        objReqItem.DocumentItemShippingDetails = new List<DocumentItemShippingDetail>() { objDocumentItemShippingDetail };
                        objReqItem.ItemType = GetItemType(GetInvoiceFieldValue<string>(dr, RequisitionExcelItemColumn.ItemType));
                        objReqItem.ItemExtendedType = GetItemExtendedType(GetInvoiceFieldValue<string>(dr, RequisitionExcelItemColumn.ItemType));
                        objReqItem.ItemNumber = GetInvoiceFieldValue<string>(dr, RequisitionExcelItemColumn.ItemNumber);
                        objReqItem.SupplierItemNumber = GetInvoiceFieldValue<string>(dr, RequisitionExcelItemColumn.SupplierItemNumber);
                        objReqItem.ContractNo = GetInvoiceFieldValue<string>(dr, RequisitionExcelItemColumn.ContractNumber);
                        objReqItem.BlanketNumber = GetInvoiceFieldValue<string>(dr, RequisitionExcelItemColumn.BlanketNumber);
                        objReqItem.ContractItemId = GetInvoiceFieldValue<long>(dr, RequisitionExcelItemColumn.ContractItemLineRef);
                        objReqItem.Description = GetInvoiceFieldValue<string>(dr, RequisitionExcelItemColumn.Description);
                        GetManufacturerDetails(GetInvoiceFieldValue<string>(dr, RequisitionExcelItemColumn.ManufacturerDetails), objReqItem);
                        objReqItem.CategoryName = GetInvoiceFieldValue<string>(dr, RequisitionExcelItemColumn.Category);
                        objReqItem.Quantity = GetInvoiceFieldValue<decimal>(dr, RequisitionExcelItemColumn.QuantityEffort);
                        objReqItem.UOM = GetInvoiceFieldValue<string>(dr, RequisitionExcelItemColumn.UnitofMeasure);
                        objReqItem.Currency = GetInvoiceFieldValue<string>(dr, RequisitionExcelItemColumn.Currency);
                        objReqItem.UnitPrice = GetInvoiceFieldValue<decimal>(dr, RequisitionExcelItemColumn.UnitPrice);
                        objReqItem.AdditionalCharges = GetInvoiceFieldValue<decimal>(dr, RequisitionExcelItemColumn.OtherCharges);
                        objReqItem.ShippingCharges = GetInvoiceFieldValue<decimal>(dr, RequisitionExcelItemColumn.ShippingAndFreight);
                        objReqItem.Tax = 0;
                        objReqItem.DateNeeded = GetFormattedDate(GetInvoiceFieldValue<string>(dr, RequisitionExcelItemColumn.NeedByDate));
                        objReqItem.DateNeeded = objReqItem.DateNeeded == DateTime.MaxValue ? (DateTime?)null : TimeZoneInfo.ConvertTimeToUtc(objReqItem.DateNeeded.Value.Date.Add(new TimeSpan(12, 00, 0)));
                        objReqItem.DelivertoLocationID = 0;
                        objReqItem.DelivertoStr = GetInvoiceFieldValue<string>(dr, RequisitionExcelItemColumn.DeliverTo);

                        //Here item is marked as Manual. We will mark item as Hosted if item is successfully validated as Catalog item.
                        objReqItem.SourceType = ItemSourceType.Manual;
                        if (objReqItem.ItemType == ItemType.Service)
                        {
                            objReqItem.StartDate = GetFormattedDate(GetInvoiceFieldValue<string>(dr, RequisitionExcelItemColumn.ServiceStartDate));
                            objReqItem.StartDate = objReqItem.StartDate == DateTime.MaxValue ? (DateTime?)null : TimeZoneInfo.ConvertTimeToUtc(objReqItem.StartDate.Value.Date.Add(new TimeSpan(12, 00, 0)));
                            objReqItem.EndDate = GetFormattedDate(GetInvoiceFieldValue<string>(dr, RequisitionExcelItemColumn.ServiceEndDate));
                            objReqItem.EndDate = objReqItem.EndDate == DateTime.MaxValue ? (DateTime?)null : TimeZoneInfo.ConvertTimeToUtc(objReqItem.EndDate.Value.Date.Add(new TimeSpan(12, 00, 0)));
                        }
                        objReqItem.DateRequested = DateTime.Now;

                        objReqItem.PartnerName = GetInvoiceFieldValue<string>(dr, RequisitionExcelItemColumn.Supplier);
                        objReqItem.OrderLocationName = GetInvoiceFieldValue<string>(dr, RequisitionExcelItemColumn.SupplierOrderingLocation);
                        objReqItem.PartnerContactName = GetInvoiceFieldValue<string>(dr, RequisitionExcelItemColumn.SupplierOrderingContact);
                        objReqItem.ShipFromLocationName = GetInvoiceFieldValue<string>(dr, RequisitionExcelItemColumn.ShipFromLocation);
                        objReqItem.MatchType = GetItemMatchType(GetInvoiceFieldValue<string>(dr, RequisitionExcelItemColumn.MatchType));
                        objReqItem.IsTaxExempt = GetInvoiceFieldValue<string>(dr, RequisitionExcelItemColumn.TaxExempt) == "Yes" ? true : false;
                        objReqItem.TrasmissionMode = (int)GetTrasmissionMode(GetInvoiceFieldValue<string>(dr, RequisitionExcelItemColumn.DispatchMode));
                        if (objReqItem.TrasmissionMode > 0)
                            objReqItem.TransmissionValue = GetInvoiceFieldValue<string>(dr, RequisitionExcelItemColumn.DispatchInfo);
                        //Read Accounting information
                        objReqItem.ItemSplitsDetail = new List<RequisitionSplitItems>();
                        if (dtRequisitionAccounting != null && dtRequisitionAccounting.Rows.Count > 0)
                        {
                            List<DataRow> objAccountingDetails = dtRequisitionAccounting.AsEnumerable().Where(rw => rw.Field<dynamic>(Enum.GetName(typeof(RequisitionExcelItemColumn), RequisitionExcelItemColumn.LineReferenceNumber)).ToString() == objReqItem.ItemLineNumber.ToString()).ToList<DataRow>(); ;
                            if (objAccountingDetails != null && objAccountingDetails.Count() > 0)
                            {
                                foreach (DataRow dtRowAccountingItems in objAccountingDetails)
                                {
                                    RequisitionSplitItems objReqSplitItem = new RequisitionSplitItems();
                                    objReqSplitItem.DocumentItemId = GetInvoiceFieldValue<long>(dtRowAccountingItems, RequisitionAccountingSplitColumn.LineReferenceNumber);
                                    objReqSplitItem.SplitType = GetInvoiceFieldValue<SplitType>(dtRowAccountingItems, RequisitionAccountingSplitColumn.SplitType);
                                    if (objReqSplitItem.SplitType == SplitType.Percentage)
                                        objReqSplitItem.Percentage = GetInvoiceFieldValue<decimal>(dtRowAccountingItems, RequisitionAccountingSplitColumn.SplitValue);
                                    else
                                        objReqSplitItem.Quantity = GetInvoiceFieldValue<decimal>(dtRowAccountingItems, RequisitionAccountingSplitColumn.SplitValue);
                                    //Read Accounting information
                                    FillAccountingFields(dtRequisitionAccounting, dtRowAccountingItems, objReqSplitItem, lstSplitAccountingFields);
                                    objReqItem.ItemSplitsDetail.Add(objReqSplitItem);
                                }
                            }
                            else
                            {
                                RequisitionSplitItems objReqSplitItem = new RequisitionSplitItems();

                                objReqSplitItem.DocumentItemId = objReqItem.ItemLineNumber;
                                objReqSplitItem.SplitType = SplitType.Quantity;
                                objReqSplitItem.Percentage = 100;
                                objReqSplitItem.Quantity = objReqItem.ItemExtendedType == ItemExtendedType.Fixed ? Convert.ToDecimal(objReqItem.UnitPrice) : objReqItem.Quantity;
                                //Read Accounting information
                                FillAccountingFields(dtRequisitionLines, dr, objReqSplitItem, lstSplitAccountingFields);
                                objReqItem.ItemSplitsDetail.Add(objReqSplitItem);
                            }
                        }
                        else
                        {
                            RequisitionSplitItems objReqSplitItem = new RequisitionSplitItems();

                            objReqSplitItem.DocumentItemId = objReqItem.ItemLineNumber;
                            objReqSplitItem.SplitType = SplitType.Quantity;
                            objReqSplitItem.Percentage = 100;
                            objReqSplitItem.Quantity = objReqItem.ItemExtendedType == ItemExtendedType.Fixed ? Convert.ToDecimal(objReqItem.UnitPrice) : objReqItem.Quantity;
                            //Read Accounting information
                            FillAccountingFields(dtRequisitionLines, dr, objReqSplitItem, lstSplitAccountingFields);
                            objReqItem.ItemSplitsDetail.Add(objReqSplitItem);
                        }

                        // tax codes
                        objReqItem.Taxes = new List<Taxes>();
                        if (dtTaxCodes != null && dtTaxCodes.Rows.Count > 0 && !objReqItem.IsTaxExempt)
                        {
                            List<DataRow> objTaxCodes = dtTaxCodes.AsEnumerable().Where(rw => rw.Field<dynamic>(Enum.GetName(typeof(RequisitionLinesRowTaxCodesColumn), RequisitionLinesRowTaxCodesColumn.LineReferenceNumber)).ToString() == objReqItem.ItemLineNumber.ToString()).ToList<DataRow>(); ;
                            if (objTaxCodes != null && objTaxCodes.Count() > 0)
                            {
                                foreach (DataRow dtTaxItems in objTaxCodes)
                                {
                                    Taxes objTaxes = new Taxes();
                                    objTaxes.LineNumber = GetInvoiceFieldValue<long>(dtTaxItems, RequisitionLinesRowTaxCodesColumn.LineReferenceNumber);
                                    objTaxes.TaxCode = GetInvoiceFieldValue<string>(dtTaxItems, RequisitionLinesRowTaxCodesColumn.TaxCode);
                                    objTaxes.TaxDescription = GetInvoiceFieldValue<string>(dtTaxItems, RequisitionLinesRowTaxCodesColumn.TaxDescription);
                                    objReqItem.Taxes.Add(objTaxes);

                                }
                                var grpDupes = objReqItem.Taxes.GroupBy(x => new { x.LineNumber, x.TaxCode, x.TaxDescription })
                                            .Where(g => g.Count() > 1)
                                            .Select(y => new Taxes()
                                            {
                                                LineNumber = y.Key.LineNumber,
                                                TaxCode = y.Key.TaxCode,
                                                TaxDescription = y.Key.TaxDescription

                                            });

                                foreach (var item in grpDupes)
                                {
                                    objReqItem.Taxes.RemoveAll(x => x.TaxCode == item.TaxCode);
                                    lstDuplicateTaxes.Add(item);
                                }
                            }
                        }

                        lstRequisitionItems.Add(objReqItem);

                    }
                }
                #endregion
            }


            #endregion
        }

        private DateTime GetFormattedDate(string strDate)
        {
            //If Date is Defined and Invalid return Min Date
            DateTime date = DateTime.MaxValue;
            if (!string.IsNullOrEmpty(strDate) && !DateTime.TryParse(strDate, new CultureInfo("en-US"), DateTimeStyles.None, out date))
                return DateTime.MinValue;
            else if (string.IsNullOrEmpty(strDate))  //If Date is not Deinfed Return Max Date          
                return DateTime.MaxValue;
            return date;
        }
        private string FormatColumnName(string pUnFormattedColumnName)
        {
            string strFormattedColumnName = string.Empty;

            if (!string.IsNullOrEmpty(pUnFormattedColumnName))
                strFormattedColumnName = pUnFormattedColumnName.Replace("*", string.Empty).Replace(" ", string.Empty).Replace("^", string.Empty).Replace("/", string.Empty).Trim();

            return strFormattedColumnName;
        }

        private void GetManufacturerDetails(string manufacturerDetails, RequisitionItem reqItem)
        {
            reqItem.ManufacturerModel = string.Empty;
            reqItem.ManufacturerName = string.Empty;
            reqItem.ManufacturerPartNumber = string.Empty;
            if (manufacturerDetails != null && !manufacturerDetails.Equals(string.Empty))
            {
                var multipleManufacturer = manufacturerDetails.Split('|').ToArray();
                var iRow = 0;
                foreach (var obj in multipleManufacturer)
                {
                    var arrManufacturerDetails = obj.Split(':').ToArray();
                    if (arrManufacturerDetails.Length > 0)
                    {
                        if (arrManufacturerDetails.Length > 0)
                            reqItem.ManufacturerName = iRow != 0 ? reqItem.ManufacturerName + "|" + arrManufacturerDetails[0] : arrManufacturerDetails[0];
                        if (arrManufacturerDetails.Length > 1)
                            reqItem.ManufacturerPartNumber = iRow != 0 ? reqItem.ManufacturerPartNumber + "|" + arrManufacturerDetails[1] : arrManufacturerDetails[1];
                        if (arrManufacturerDetails.Length > 2)
                            reqItem.ManufacturerModel = iRow != 0 ? reqItem.ManufacturerModel + "|" + arrManufacturerDetails[2] : arrManufacturerDetails[2];
                    }
                    iRow++;
                }
            }

        }

        private ItemExtendedType GetItemExtendedType(string pItemType)
        {
            ItemExtendedType iType = ItemExtendedType.None;

            if (!string.IsNullOrEmpty(pItemType))
            {
                switch (pItemType.Trim())
                {
                    case "Material":
                        iType = ItemExtendedType.Material;
                        break;
                    case "Fixed Service":
                        iType = ItemExtendedType.Fixed;
                        break;
                    case "Variable Service":
                        iType = ItemExtendedType.Variable;
                        break;
                }
            }
            return iType;
        }

        private ItemType GetItemType(string pItemType)
        {
            ItemType iType = ItemType.None;

            if (!string.IsNullOrEmpty(pItemType))
            {
                switch (pItemType.Trim())
                {
                    case "Material":
                        iType = ItemType.Material;
                        break;
                    case "Fixed Service":
                    case "Variable Service":
                        iType = ItemType.Service;
                        break;
                }
            }
            return iType;
        }

        private MatchType GetItemMatchType(string pItemType)
        {
            MatchType iType = MatchType.None;

            if (!string.IsNullOrEmpty(pItemType))
            {
                switch (pItemType.Trim())
                {

                    case "2-Way":
                        iType = MatchType.TwoWayMatch;
                        break;
                    case "3-Way":
                        iType = MatchType.ThreeWayMatch;
                        break;
                }
            }
            return iType;
        }
        private POTransmissionMode GetTrasmissionMode(string pItemType)
        {
            POTransmissionMode transmissionMode = POTransmissionMode.None;

            if (!string.IsNullOrEmpty(pItemType))
            {
                switch (pItemType.Trim())
                {
                    case "Portal":
                        transmissionMode = POTransmissionMode.Portal;
                        break;
                    case "Direct Email":
                        transmissionMode = POTransmissionMode.Email;
                        break;
                    case "EDI/cXML":
                        transmissionMode = POTransmissionMode.CxmlEDI;
                        break;
                    case "Call & Submit":
                        transmissionMode = POTransmissionMode.CallAndSubmit;
                        break;
                }
            }
            return transmissionMode;
        }
        private void FillAccountingFields(DataTable dtAccountingDetails, DataRow drAccoutingRow, RequisitionSplitItems objReqSplitItem, List<SplitAccountingFields> lstSplitAccountingFields)
        {
            objReqSplitItem.DocumentSplitItemEntities = new List<DocumentSplitItemEntity>();

            foreach (var SplitAccountingFields in lstSplitAccountingFields)
            {
                DocumentSplitItemEntity objDocumentSplitItemEntity = new DocumentSplitItemEntity();

                //PERIOD\REQUESTER\Header Level Entities are not available in Excel so need to handle separately
                //Source System Should also be Auto Defaulted 
                if ((SplitAccountingFields.Title.ToUpper().Equals("PERIOD")
                    || SplitAccountingFields.Title.ToUpper().Equals("REQUESTER"))
                    || lsDocumentHeaderEntitites.Any(x => x.Title.ToUpper().Equals(SplitAccountingFields.Title.ToUpper()))
                    || SplitAccountingFields.EntityTypeId == SourceSystemEntityId)
                {
                    objDocumentSplitItemEntity.EntityDisplayName = SplitAccountingFields.Title;
                    objDocumentSplitItemEntity.SplitAccountingFieldId = SplitAccountingFields.SplitAccountingFieldId;
                    objDocumentSplitItemEntity.EntityCode = "0";
                    objDocumentSplitItemEntity.EntityTypeId = SplitAccountingFields.EntityTypeId;
                    objReqSplitItem.DocumentSplitItemEntities.Add(objDocumentSplitItemEntity);
                }
                else
                {
                    if (dtAccountingDetails.Columns.Contains(SplitAccountingFields.Title.Replace(" ", string.Empty)))
                    {
                        string AccoungingFieldValue = drAccoutingRow[SplitAccountingFields.Title.Replace(" ", string.Empty)].ToString();
                        objDocumentSplitItemEntity.IsMandatory = SplitAccountingFields.IsMandatory;
                        if (!string.IsNullOrEmpty(AccoungingFieldValue))
                        {
                            objDocumentSplitItemEntity.EntityDisplayName = SplitAccountingFields.Title;
                            objDocumentSplitItemEntity.SplitAccountingFieldId = SplitAccountingFields.SplitAccountingFieldId;
                            objDocumentSplitItemEntity.EntityCode = AccoungingFieldValue;
                            objDocumentSplitItemEntity.EntityTypeId = SplitAccountingFields.EntityTypeId;

                            if (!lstAccountingDataFromUpload.Where(x => x.Key.Equals(objDocumentSplitItemEntity.SplitAccountingFieldId) && x.Value.Equals(objDocumentSplitItemEntity.EntityCode)).Any())
                                lstAccountingDataFromUpload.Add(new KeyValuePair<int, string>(objDocumentSplitItemEntity.SplitAccountingFieldId, objDocumentSplitItemEntity.EntityCode));
                        }
                        else
                        {
                            objDocumentSplitItemEntity.EntityDisplayName = SplitAccountingFields.Title;
                            objDocumentSplitItemEntity.SplitAccountingFieldId = SplitAccountingFields.SplitAccountingFieldId;
                            objDocumentSplitItemEntity.EntityCode = string.Empty;
                            objDocumentSplitItemEntity.SplitAccountingFieldValue = "0";
                            objDocumentSplitItemEntity.EntityTypeId = SplitAccountingFields.EntityTypeId;
                        }
                        objReqSplitItem.DocumentSplitItemEntities.Add(objDocumentSplitItemEntity);
                    }
                }
            }
        }
    }


}





