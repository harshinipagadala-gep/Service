using Gep.Cumulus.CSM.Entities;
using Gep.Cumulus.Partner.Entities;
using GEP.Cumulus.Documents.Entities;
using GEP.Cumulus.Logging;
using GEP.Cumulus.P2P.BusinessEntities;
using GEP.Cumulus.P2P.BusinessObjects;
using GEP.Cumulus.P2P.BusinessObjects.Proxy;
using GEP.Cumulus.P2P.Req.BusinessObjects.Proxy;
using GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper;
using GEP.Cumulus.Portal.Entities;
using GEP.Platform.FileManagerHelper;
using GEP.SMART.Configuration;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using ExcelLib = Aspose.Cells;
using FileManagerEntities = GEP.NewP2PEntities.FileManagerEntities;
using QuestionBankEntities = GEP.Cumulus.QuestionBank.Entities;

namespace GEP.Cumulus.P2P.Req.BusinessObjects.FactoryClasses
{
    public abstract class RequisitionTemplateGenerator : RequisitionBaseBO
    {
        internal static readonly ILog Log = Logger.GetLog(MethodBase.GetCurrentMethod().DeclaringType);
        public readonly string EXCELSHEET_REQUISITIONLINES = "Requisition Lines";
        private readonly string EXCELSHEET_MASTER = "Field Values - Master";
        public readonly string EXCELSHEET_ACCOUNTING = "Accounting Splits";
        public readonly string EXCELSHEET_TAXCODES = "Tax";
        private readonly string EXCELSHEET_ACCOUNTING_MASTER = "Accounting Master";
        private readonly string REQUISITION_EXCEL_UPLOAD_TEMPLATEURI = "cumuluscontent/P2P/";

        internal FileManagerApi fileManagerApi;
        internal NewRequisitionManager RequisitionManger = null;
        private string fileName = string.Empty;
        protected dynamic objFileDetails = null;
        private System.IO.MemoryStream objMemoryStream = null;
        private ExcelLib.License license = null;
        protected ExcelLib.Workbook workbook = null;
        private Int64 documentCode = 0;
        protected string documentNumber = string.Empty;
        protected RequisitionCommonManager commonManager = null;
        //InvoiceManager invoiceManager = null;
        protected long LOBEntityDetailCode = 0;
        protected ExcelLib.Worksheet wsRequisitionLines = null;
        protected ExcelLib.Worksheet wsTaxCodes = null;
        protected ExcelLib.Worksheet wsAccountingSplits = null;
        private ExcelLib.Worksheet wsAccountingMaster = null;
        internal ProxyPartnerService proxyPartnerService = null;
        protected List<SplitAccountingFields> lsDocumentHeaderEntitites = new List<SplitAccountingFields>();
        private int structureId = 0;
        private ExcelLib.Worksheet wsFieldValueMaster = null;
        protected List<SplitAccountingFields> lstSplitAccountingFields = null;
        protected List<KeyValuePair<int, string>> lstAccountingDataFromUpload = new List<KeyValuePair<int, string>>();
        protected RequisitionDocumentManager objDocBO = null;
        protected List<SplitAccountingFields> dataListSplitAccountingFields = null;
        protected string orgEntityCode = string.Empty;
        protected string orgAndNonOrgEntityCode = string.Empty;
        protected string nonOrgEntityCode = string.Empty;
        protected Hashtable htItemsTabAccountingColumnIndex = new Hashtable();
        protected Hashtable htAccountingTabAccountingColumnIndex = new Hashtable();
        protected int SplitTypeColumnIndex = 0;
        protected int SplitValueColumnIndex = 0;
        protected string blobPath = MultiRegionConfig.GetConfig(CloudConfig.BlobURL).Trim('/');
        protected string asposeLicencePathUrl = MultiRegionConfig.GetConfig(CloudConfig.AsposeLicensePath);
        protected List<KeyValue> lstUOMs = null;
        protected List<KeyValue> lstCurrency = null;
        protected List<KeyValue> lstSuppliers = null;
        protected List<KeyValue> lstShipToLocations = null;
        protected List<PASMasterData> lstAllCategories = null;
        protected List<ContactInfo> lstSuppliersOrderContacts = null;
        protected List<PartnerLocation> lstSuppliersOrderLocations = null;
        protected int precessionValue = 0;
        protected int minPrecessionValue = 0;
        protected int maxPrecessionforTotal = 0;
        protected int maxPrecessionForTaxesAndCharges = 0;
        protected string populateDefaultNeedByDate = string.Empty;
        protected int populateDefaultNeedByDateByDays = 0;
        protected bool IsValidateContractNumber = false;
        protected RequisitionManager objReqManager = null;
        protected bool IsPeriodbasedonNeedbyDate = false;
        protected long SourceSystemEntityId = 0;
        protected long SourceSystemEntityDetailCode = 0;
        protected ICollection<string> lstShippingMethods = null;
        protected List<PartnerLocation> lstShipFomLocations = null;
        protected List<TaxMaster> lstTaxes = null;
        internal ProxyPortalService proxyPortalService = null;
        protected int RequisitionTaxLinesRowIndex = 0;
        protected bool AllowTaxExempt = false;
        protected bool DefaultMaterialTaxExempt = false;
        protected bool DefaultServiceTaxExempt = false;
        protected bool OverrideUnitPriceFromCatalogForExcelUploadOfHostedItems = false;
        protected bool isPartnerBasedOnOrderingLoc = false;
        protected bool IsDefaultOrderingLocation = false;
        protected bool MapDispatchModetoOrderingLocation = false;
        protected bool EnableWebAPIForGetLineItems = true;
        protected bool IsExcludeMasterDataFromExcelTemplate = false;
        protected RequisitionExcelTemplateHandler actionType;
        protected bool EnableGetLineItemsBulkAPI = false;
        protected bool EnableAdvancePaymentForREQ = false;
        protected bool EnableAdvancePayment = false;
        protected bool EnableContractBulkAPI = false;
        protected bool ShowTaxJurisdictionForShipTo = false;
        protected string SupplierItemNumberFieldType = "1";
        protected int maxPrecessionForQuantity = 0;
        protected int maxPrecessionForUnitPrice = 0;
        protected int maxPrecessionForShippingOrFreight = 0;
        protected int maxPrecessionForOtherCharges = 0;
        protected string OverrideUnitPriceForExcelUploadItemSource = "";
        protected bool enableMultipleLevelCategoryForExcelUpload = false;
        protected string orderingLocationbyHeaderEntities = string.Empty;
        protected bool AllowMultiCurrencyInRequisition = false;

        public RequisitionTemplateGenerator(UserExecutionContext userContext, Int64 documentCode, string documentNumber, NewRequisitionManager ReqManger, string jwtToken, RequisitionExcelTemplateHandler actionType) : base(jwtToken)
        {
            this.UserContext = userContext;
            this.documentCode = documentCode;
            this.documentNumber = documentNumber;
            this.actionType = actionType;
            RequisitionManger = ReqManger;
            InitializeProxy();
            var OrderingLocationbyHeaderEntities = commonManager.GetSettingsValueByKey(P2PDocumentType.None, "OrderingLocationbyHeaderEntities", this.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBEntityDetailCode);
            orderingLocationbyHeaderEntities = !string.IsNullOrEmpty(OrderingLocationbyHeaderEntities) ? OrderingLocationbyHeaderEntities : string.Empty;
            GetRequisitionEntityDetailsByDocumentCode();
            GetDocumentSourceSystemEntity();
            GetLOBAndStructureId();
            GetAccountingFields();
            string ExcludeMasterDataFromExcelTemplate = commonManager.GetSettingsValueByKey(P2PDocumentType.Requisition, "ExcludeMasterDataFromExcelTemplate", this.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBEntityDetailCode);
            IsExcludeMasterDataFromExcelTemplate = string.IsNullOrEmpty(ExcludeMasterDataFromExcelTemplate) ? false : Convert.ToBoolean(ExcludeMasterDataFromExcelTemplate);
            string EnableMultipleLevelCategoryForExcelUpload = commonManager.GetSettingsValueByKey(P2PDocumentType.Requisition, "EnableMultipleLevelCategoryForExcelUpload", this.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBEntityDetailCode);
            enableMultipleLevelCategoryForExcelUpload = string.IsNullOrEmpty(EnableMultipleLevelCategoryForExcelUpload) ? false : Convert.ToBoolean(EnableMultipleLevelCategoryForExcelUpload);
            if (!(IsExcludeMasterDataFromExcelTemplate &&
                (actionType == RequisitionExcelTemplateHandler.DownloadTemplate
                || actionType == RequisitionExcelTemplateHandler.ExportTemplate
                || actionType == RequisitionExcelTemplateHandler.UploadTemplate)))
            {
                GetAllSplitAccountingCodesByDocumentType();
            }

        }


        public void GenerateTemplate()
        {

            FileManagerDetails();

            if (workbook != null)
            {
                wsRequisitionLines = workbook.Worksheets[EXCELSHEET_REQUISITIONLINES];
                if (wsRequisitionLines != null)
                {
                    var IsShipFromLocationVisible = Convert.ToString(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "IsShipFromLocationVisible", this.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBEntityDetailCode));
                    if (IsShipFromLocationVisible.ToLower() == "true")
                    {
                        int ReqLinesColumnStartIndex = 0;
                        ReqLinesColumnStartIndex = (int)RequisitionExcelItemColumn.ShipFromLocation;
                        wsRequisitionLines.Cells.InsertColumn(ReqLinesColumnStartIndex);
                        wsRequisitionLines.Cells.SetColumnWidth(ReqLinesColumnStartIndex, 30);
                        Aspose.Cells.Cell cellReqLines = wsRequisitionLines.Cells[(int)RequisitionExcelTemplateRowStartIndex.RequisitionLinesRowStartIndex, ReqLinesColumnStartIndex];
                        RemoveExcelCellValidation(SelectCellAreaByColumnIndex(ReqLinesColumnStartIndex), wsRequisitionLines);
                        cellReqLines.PutValue("Ship From Location");
                        if (!(IsExcludeMasterDataFromExcelTemplate &&
                            (actionType == RequisitionExcelTemplateHandler.DownloadTemplate || actionType == RequisitionExcelTemplateHandler.ExportTemplate)))
                        {
                            FillShipFromData();
                        }
                    }
                }

            }

            #region Create Dynamic Accounting Columns in Requisitions Lines and Accounting Splits Tab 
            if (workbook != null)
            {
                wsRequisitionLines = workbook.Worksheets[EXCELSHEET_REQUISITIONLINES];
                wsAccountingSplits = workbook.Worksheets[EXCELSHEET_ACCOUNTING];
                wsAccountingMaster = workbook.Worksheets[EXCELSHEET_ACCOUNTING_MASTER];
                wsFieldValueMaster = workbook.Worksheets[EXCELSHEET_MASTER];
                if (wsRequisitionLines != null && wsAccountingSplits != null)
                {
                    int ReqLinesAccountingColumnStartIndex = 0;
                    int ReqAccountingSplitsColumnStartIndex = 0;
                    int CustomAttributesStartIndex = -2;
                    int currentIndex = 0;

                    //Start Creating Accounting Columns after SupplierOrderingContact column in excel
                    var IsShipFromLocationVisible = Convert.ToString(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "IsShipFromLocationVisible", this.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBEntityDetailCode));
                    if (IsShipFromLocationVisible.ToLower() == "true")
                    {
                        ReqLinesAccountingColumnStartIndex = (int)RequisitionExcelItemColumn.ShipFromLocation;
                    }
                    else
                    {
                        ReqLinesAccountingColumnStartIndex = (int)RequisitionExcelItemColumn.DeliverTo;
                    }

                    //Start Creating Accounting Columns after SplitType column in excel
                    ReqAccountingSplitsColumnStartIndex = (int)RequisitionAccountingSplitColumn.LineReferenceNumber;
                    UpdatelstSplitAccountingFields();

                    if (lstSplitAccountingFields != null && lstSplitAccountingFields.Count > 0)
                    {
                        CustomAttributesStartIndex = ReqLinesAccountingColumnStartIndex;

                        foreach (SplitAccountingFields spltAccountingField in lstSplitAccountingFields)
                        {
                            string columnTitle = string.Empty;
                            //Note :  Excel Row index are zero based index. 
                            //Adding Accounting Entity in Requisition Lines Tab

                            wsRequisitionLines.Cells.InsertColumn(ReqLinesAccountingColumnStartIndex + 1);
                            wsRequisitionLines.Cells.SetColumnWidth(ReqLinesAccountingColumnStartIndex + 1, 30);
                            Aspose.Cells.Cell cellReqLines = wsRequisitionLines.Cells[(int)RequisitionExcelTemplateRowStartIndex.RequisitionLinesRowStartIndex, ReqLinesAccountingColumnStartIndex + 1];
                            RemoveExcelCellValidation(SelectCellAreaByColumnIndex(ReqLinesAccountingColumnStartIndex + 1), wsRequisitionLines);
                            //Adding Accounting Entity in Accounting Splits Tab after Split Type column
                            wsAccountingSplits.Cells.InsertColumn(ReqAccountingSplitsColumnStartIndex + 1);
                            wsAccountingSplits.Cells.SetColumnWidth(ReqAccountingSplitsColumnStartIndex + 1, 30);
                            Aspose.Cells.Cell cellAccounting = wsAccountingSplits.Cells[(int)RequisitionExcelTemplateRowStartIndex.RequisitionAccountingSplitRowStartIndex, ReqAccountingSplitsColumnStartIndex + 1];

                            if (spltAccountingField.IsMandatory)
                                columnTitle = spltAccountingField.Title + "*";
                            else
                                columnTitle = spltAccountingField.Title;

                            //set accounting column format in req lines
                            ExcelLib.Style cellFormat = cellReqLines.GetStyle();
                            ExcelLib.StyleFlag flag = new ExcelLib.StyleFlag();
                            flag.NumberFormat = true;
                            //Set the formating on the as text formating
                            cellFormat.Number = 49;
                            cellReqLines.SetStyle(cellFormat, flag);
                            //SEt column Name in Req Lines                            
                            cellReqLines.PutValue(columnTitle);
                            //Set Column Name in Accounting Split
                            cellAccounting.PutValue(columnTitle);
                            CustomAttributesStartIndex++;
                            if (!htItemsTabAccountingColumnIndex.ContainsKey(spltAccountingField.Title))
                                htItemsTabAccountingColumnIndex.Add(spltAccountingField.Title, (ReqLinesAccountingColumnStartIndex + (lstSplitAccountingFields.Count() - currentIndex)));
                            if (!htAccountingTabAccountingColumnIndex.ContainsKey(spltAccountingField.Title))
                                htAccountingTabAccountingColumnIndex.Add(spltAccountingField.Title, (ReqAccountingSplitsColumnStartIndex + (lstSplitAccountingFields.Count() - currentIndex)));
                            currentIndex++;
                        }
                    }
                    SplitTypeColumnIndex = ReqAccountingSplitsColumnStartIndex + (currentIndex) + 1;
                    SplitValueColumnIndex = SplitTypeColumnIndex + 1;

                    #region Custom Attributes
                    //Create Custom Attributes Dynamically after Accounting Entities
                    //List<QuestionBankEntities.Question> lstCustomAttributes = GetCustomAttributes();
                    //if (lstCustomAttributes.Count > 0)
                    //{
                    //    foreach (QuestionBankEntities.Question objQuestion in lstCustomAttributes)
                    //    {
                    //        string columnTitle = string.Empty;
                    //        wsRequisitionLines.Cells.InsertColumn(CustomAttributesStartIndex + 1);
                    //        wsRequisitionLines.Cells.SetColumnWidth(CustomAttributesStartIndex + 1, 30);
                    //        Aspose.Cells.Cell cellReqLines = wsRequisitionLines.Cells[(int)RequisitionExcelTemplateRowStartIndex.RequisitionLinesRowStartIndex, CustomAttributesStartIndex + 1];
                    //        if (objQuestion.IsMandatory)
                    //            columnTitle = objQuestion.QuestionText + "*";
                    //        else
                    //            columnTitle = objQuestion.QuestionText;
                    //        //SEt column Name in Req Lines                            
                    //        cellReqLines.PutValue(columnTitle);
                    //    }
                    //}
                    #endregion

                    #region Fill Accounting Master Tab
                    if (!(IsExcludeMasterDataFromExcelTemplate &&
                        (actionType == RequisitionExcelTemplateHandler.DownloadTemplate || actionType == RequisitionExcelTemplateHandler.ExportTemplate)))
                    {
                        FillAccountingMasterData(lstSplitAccountingFields);
                    }
                    #endregion

                    #region Fill FieldValue Master Tab
                    if (!(IsExcludeMasterDataFromExcelTemplate &&
                        (actionType == RequisitionExcelTemplateHandler.DownloadTemplate || actionType == RequisitionExcelTemplateHandler.ExportTemplate)))
                    {
                        FillFieldValueMaster();
                    }

                    if (IsExcludeMasterDataFromExcelTemplate &&
                        (actionType == RequisitionExcelTemplateHandler.DownloadTemplate || actionType == RequisitionExcelTemplateHandler.ExportTemplate))
                    {
                        workbook.Worksheets.RemoveAt(EXCELSHEET_ACCOUNTING_MASTER);
                    }
                    #endregion


                }
                ChangeAccountingColumnColorStyle();
            }
            #endregion

            //workbook.Save("C:\\Manoj\\Excel Upload\\RequisitionTemplate.xlsx");
        }

        private void WriteFileDataToWorkbook(byte[] fileData)
        {
            if (fileData != null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    ms.Write(fileData, 0, fileData.Length);
                    ms.Position = 0;
                    workbook = new ExcelLib.Workbook(ms);
                    workbook.FileFormat = ExcelLib.FileFormatType.Xlsx;
                }
            }
        }

        public MemoryStream BytesToStream(byte[] inputBytes)
        {
            MemoryStream objMemoryStream = null;

            if (inputBytes != null && inputBytes.Length > 0)
            {
                objMemoryStream = new MemoryStream();
                objMemoryStream.Write(inputBytes, 0, inputBytes.Length);
                objMemoryStream.Position = 0;
            }
            return objMemoryStream;
        }
        public string UploadFiletoBlobContainerAndGetFileId()
        {
            FileManagerEntities.FileUploadResponseModel fileUpload = null;
            byte[] bExcelArray = null;
            string result = "";
            if (workbook != null)
            {
                //create MEmory Stream
                MemoryStream objMemoryStreamExcel = new MemoryStream();
                ExcelLib.SaveFormat objSaveFormat = ExcelLib.SaveFormat.Xlsx;

                //Save into Byte Array
                workbook.Save(objMemoryStreamExcel, objSaveFormat);
                objMemoryStreamExcel.Seek(0, SeekOrigin.Begin);
                bExcelArray = objMemoryStreamExcel.ToArray();
                string excelFileName = string.Empty;
                if (IsExcludeMasterDataFromExcelTemplate)
                {
                    if (actionType == RequisitionExcelTemplateHandler.DownloadMasterTemplate)
                    {
                        excelFileName = documentNumber + " _LineUpload_MasterTemplate" + ".xlsx";
                    }
                    else
                    {
                        excelFileName = documentNumber + " _LineUpload_TransactionalTemplate" + ".xlsx";
                    }
                }
                else
                {
                    excelFileName = documentNumber + " _LineUpload_Template" + ".xlsx";

                }

                string tempBlobFileUri = FileOperations.UploadByteArraytoTemporaryBlobAsync(bExcelArray,
                                                                                            MultiRegionConfig.GetConfig(CloudConfig.CloudStorageConn),
                                                                                            MultiRegionConfig.GetConfig(CloudConfig.TempFileUploadContainerName)).GetAwaiter().GetResult();

                var requestHeaders = new RequestHeaders();
                requestHeaders.Set(this.UserContext, this.JWTToken);
                string appName = Environment.GetEnvironmentVariable("NewRelic.AppName") ?? "Requisition";
                string useCase = "RequisitionTemplateGenerator-UploadFiletoBlobContainerAndGetFileId";
                var serviceURL = MultiRegionConfig.GetConfig(CloudConfig.APIMBaseURL) + "/FileManager/api/V2/FileManager/MoveFileToTargetBlob";

                var uploadFileToTargetBlobRequestModel = new FileManagerEntities.MoveFileToTargetBlobRequest()
                {
                    FileName = excelFileName,
                    FileContentType = FileExtenstion.Excelx.ToLower(),
                    FileValidationSettings = new FileManagerEntities.MoveFileToTargetBlobFileValidationSettings()
                    {
                        FileValidationSettingsScope = (int)FileManagerEntities.FileValidationSettingsScope.Global,
                        FileValidationContainerTypeId = (int)FileManagerEntities.FileEnums.FileContainerType.Applications,
                    },
                    TemporaryBlobFileUri = tempBlobFileUri
                };
                var webAPI = new WebAPI(requestHeaders, appName, useCase);
                var resonse = webAPI.ExecutePost(serviceURL, uploadFileToTargetBlobRequestModel);
                fileUpload = JsonConvert.DeserializeObject<FileManagerEntities.FileUploadResponseModel>(resonse);
                result = fileUpload?.EncryptedFileId;
            }
            return result;
        }
        private List<QuestionBankEntities.Question> GetCustomAttributes()
        {
            long CustomAttrFormIdForItem = 0;
            List<QuestionBankEntities.Question> itemQuestionSet = new List<QuestionBankEntities.Question>();
            List<Questionnaire> CustomAttributes = new List<Questionnaire>();
            List<QuestionBankEntities.Question> lstCustomAttributes = new List<QuestionBankEntities.Question>();

            //Get Custom Attrbutes based on documentCode
            //RequisitionCommonManager commonManager = new RequisitionCommonManager { UserContext = this.UserContext, GepConfiguration = RequisitionManger.GepConfiguration };
            var CustomAttrFormsIds = commonManager.GetCustomAttrFormId((int)Gep.Cumulus.CSM.Entities.DocumentType.Requisition, new List<GEP.Cumulus.P2P.BusinessEntities.Level>() { GEP.Cumulus.P2P.BusinessEntities.Level.Header, GEP.Cumulus.P2P.BusinessEntities.Level.Item, GEP.Cumulus.P2P.BusinessEntities.Level.Distribution }, 0, 0, 0, 0, documentCode, 0);
            foreach (var lvlForm in CustomAttrFormsIds)
            {
                switch (lvlForm.Key)
                {
                    case P2P.BusinessEntities.Level.Item:
                        CustomAttrFormIdForItem = lvlForm.Value;
                        break;
                }
            }
            if (CustomAttrFormIdForItem > 0)
            {
                itemQuestionSet = commonManager.GetQuestionSetByFormCode(CustomAttrFormIdForItem);

                //Get Questions
                if (itemQuestionSet != null && itemQuestionSet.Count() > 0)
                {
                    var itemQuestSetCodeList = itemQuestionSet.Select(questSetCode => questSetCode.QuestionSetCode).ToList<long>();
                    foreach (long questionSetCode in itemQuestSetCodeList)
                    {
                        var surveyQuestions = commonManager.GetQuestionWithResponsesByQuestionSetPaging(new QuestionBankEntities.Question() { QuestionSetCode = questionSetCode }, -1, -1, documentCode, QuestionBankEntities.AssessorUserType.Buyer, false);
                        lstCustomAttributes.AddRange(surveyQuestions);
                    }
                    //commonManager.GetQuestionWithResponse(itemQuestSetCodeList, CustomAttributes, documentCode);
                    //commonManager.FillQuestionsResponseList(lstQuestionsResponse, CustomAttributes, itemQuestSetCodeList, documentCode);
                }
            }
            return lstCustomAttributes;
        }
        private void FillAccountingMasterData(List<SplitAccountingFields> lstSplitAccountingFields)
        {
            if (wsAccountingMaster != null)
            {
                int maxDataColumn = 1;
                if (lstSplitAccountingFields != null && lstSplitAccountingFields.Count > 0)
                {
                    var orderedSplitAccountingFields = lstSplitAccountingFields.OrderBy(x => x.FieldOrder);
                    //Set the user context 
                    {
                        UserContext = this.UserContext;
                        GepConfiguration = RequisitionManger.GepConfiguration;
                    }
                    if (dataListSplitAccountingFields != null && dataListSplitAccountingFields.Count() > 0)
                    {
                        foreach (SplitAccountingFields spltAccountingField in orderedSplitAccountingFields)
                        {
                            wsAccountingMaster.Cells.InsertColumn(maxDataColumn);
                            wsAccountingMaster.Cells.InsertColumn(maxDataColumn + 1);
                            wsAccountingMaster.Cells.Merge((int)RequisitionExcelTemplateRowStartIndex.RequisitionAccountingMasterRowStartIndex - 2, maxDataColumn, 1, 2);
                            wsAccountingMaster.Cells.SetRowHeight((int)RequisitionExcelTemplateRowStartIndex.RequisitionAccountingMasterRowStartIndex - 2, 24.75);
                            Aspose.Cells.Cell titleCell = wsAccountingMaster.Cells[(int)RequisitionExcelTemplateRowStartIndex.RequisitionAccountingMasterRowStartIndex - 2, maxDataColumn];
                            titleCell.PutValue(spltAccountingField.Title);

                            ExcelLib.Style titleStyle = titleCell.GetStyle();
                            titleStyle.HorizontalAlignment = ExcelLib.TextAlignmentType.Center;
                            titleStyle.Font.Color = Color.White;
                            titleStyle.Font.IsBold = true;
                            titleStyle.Font.Size = 14;
                            titleStyle.Pattern = ExcelLib.BackgroundType.Solid;
                            titleStyle.ForegroundColor = Color.FromArgb(255, 192, 0);
                            titleCell.SetStyle(titleStyle);
                            titleCell.GetMergedRange().SetOutlineBorder(ExcelLib.BorderType.TopBorder, ExcelLib.CellBorderType.Thin, Color.Gray);
                            titleCell.GetMergedRange().SetOutlineBorder(ExcelLib.BorderType.BottomBorder, ExcelLib.CellBorderType.Thin, Color.Gray);
                            titleCell.GetMergedRange().SetOutlineBorder(ExcelLib.BorderType.LeftBorder, ExcelLib.CellBorderType.Thin, Color.Gray);
                            titleCell.GetMergedRange().SetOutlineBorder(ExcelLib.BorderType.RightBorder, ExcelLib.CellBorderType.Thin, Color.Gray);

                            Aspose.Cells.Cell codeCell = wsAccountingMaster.Cells[(int)RequisitionExcelTemplateRowStartIndex.RequisitionAccountingMasterRowStartIndex - 1, maxDataColumn];
                            codeCell.PutValue("Code");
                            ExcelLib.Style codeStyle = codeCell.GetStyle();
                            codeStyle.Font.Color = Color.White;
                            codeStyle.Font.IsBold = true;
                            codeStyle.Font.IsItalic = true;
                            codeStyle.Pattern = ExcelLib.BackgroundType.Solid;
                            codeStyle.ForegroundColor = Color.FromArgb(255, 192, 0);
                            codeStyle.SetBorder(ExcelLib.BorderType.TopBorder, ExcelLib.CellBorderType.Thin, Color.Gray);
                            codeStyle.SetBorder(ExcelLib.BorderType.BottomBorder, ExcelLib.CellBorderType.Thin, Color.Gray);
                            codeStyle.SetBorder(ExcelLib.BorderType.LeftBorder, ExcelLib.CellBorderType.Thin, Color.Gray);
                            codeStyle.SetBorder(ExcelLib.BorderType.RightBorder, ExcelLib.CellBorderType.Thin, Color.Gray);
                            codeCell.SetStyle(codeStyle);
                            Aspose.Cells.Cell nameCell = wsAccountingMaster.Cells[(int)RequisitionExcelTemplateRowStartIndex.RequisitionAccountingMasterRowStartIndex - 1, maxDataColumn + 1];
                            nameCell.PutValue("Name");
                            ExcelLib.Style nameStyle = nameCell.GetStyle();
                            nameStyle.Font.Color = Color.White;
                            nameStyle.Font.IsBold = true;
                            nameStyle.Font.IsItalic = true;
                            nameStyle.Pattern = ExcelLib.BackgroundType.Solid;
                            nameStyle.ForegroundColor = Color.FromArgb(255, 192, 0);
                            nameStyle.SetBorder(ExcelLib.BorderType.TopBorder, ExcelLib.CellBorderType.Thin, Color.Gray);
                            nameStyle.SetBorder(ExcelLib.BorderType.BottomBorder, ExcelLib.CellBorderType.Thin, Color.Gray);
                            nameStyle.SetBorder(ExcelLib.BorderType.LeftBorder, ExcelLib.CellBorderType.Thin, Color.Gray);
                            nameStyle.SetBorder(ExcelLib.BorderType.RightBorder, ExcelLib.CellBorderType.Thin, Color.Gray);
                            nameCell.SetStyle(nameStyle);
                            List<SplitAccountingFields> dataListOfSameField = dataListSplitAccountingFields.Where(p => p.SplitAccountingFieldId == spltAccountingField.SplitAccountingFieldId).ToList();
                            if (dataListOfSameField != null && dataListOfSameField.Count() > 0)
                            {
                                wsAccountingMaster.Cells.ImportCustomObjects(dataListOfSameField, new string[] { "EntityCode", "EntityDisplayName" }, false, (int)RequisitionExcelTemplateRowStartIndex.RequisitionAccountingMasterRowStartIndex, maxDataColumn, dataListOfSameField.Count, false, string.Empty, false);
                                wsAccountingMaster.AutoFitColumn(maxDataColumn);
                                wsAccountingMaster.Cells.GetColumnWidth(maxDataColumn);
                                wsAccountingMaster.AutoFitColumn(maxDataColumn + 1);
                                maxDataColumn += 2;
                            }
                        }
                    }
                }
            }

        }
        private void FillFieldValueMaster()
        {
            LoadMasterDataList();
            if (wsFieldValueMaster != null)
            {
                //write UOMS
                if (lstUOMs != null && lstUOMs.Count() > 0)
                {
                    lstUOMs = lstUOMs.OrderBy(x => x.Name).ToList<KeyValue>();
                    wsFieldValueMaster.Cells.ImportCustomObjects(lstUOMs, new string[] { "Value", "Name" }, false, (int)RequisitionExcelTemplateRowStartIndex.RequisitionFieldValueMasterRowStartIndex, (int)RequisitionFieldValueMasterColumn.UOMCode + 1, lstUOMs.Count, false, string.Empty, false);
                }
                //write Currecny
                if (lstCurrency != null && lstCurrency.Count() > 0)
                {
                    lstCurrency = lstCurrency.OrderBy(x => x.Name).ToList<KeyValue>();
                    wsFieldValueMaster.Cells.ImportCustomObjects(lstCurrency, new string[] { "Value" }, false, (int)RequisitionExcelTemplateRowStartIndex.RequisitionFieldValueMasterRowStartIndex, (int)RequisitionFieldValueMasterColumn.Curreny + 1, lstCurrency.Count, false, string.Empty, false);
                }
                //write Ship To Location
                if (lstShipToLocations != null && lstShipToLocations.Count() > 0)
                {
                    lstShipToLocations = lstShipToLocations.OrderBy(x => x.Name).ToList<KeyValue>();
                    wsFieldValueMaster.Cells.ImportCustomObjects(lstShipToLocations, new string[] { "Value", "Name" }, false, (int)RequisitionExcelTemplateRowStartIndex.RequisitionFieldValueMasterRowStartIndex, (int)RequisitionFieldValueMasterColumn.ShipToLocationCode + 1, lstShipToLocations.Count, false, string.Empty, false);
                }
                //write Suppliers
                if (lstSuppliers != null && lstSuppliers.Count() > 0)
                {
                    lstSuppliers = lstSuppliers.OrderBy(x => x.Name).ToList<KeyValue>();
                    wsFieldValueMaster.Cells.ImportCustomObjects(lstSuppliers, new string[] { "Value", "Name" }, false, (int)RequisitionExcelTemplateRowStartIndex.RequisitionFieldValueMasterRowStartIndex, (int)RequisitionFieldValueMasterColumn.SupplierCode + 1, lstSuppliers.Count, false, string.Empty, false);
                }
                //write Categories
                if (lstAllCategories != null && lstAllCategories.Count() > 0)
                {
                    //List<KeyValue> lstCategories = new List<KeyValue>();
                    //lstAllCategories.ForEach(x => { lstCategories.Add(new KeyValue { Value = x.ClientPASCode, Name = x.PASName }); });
                    wsFieldValueMaster.Cells.ImportCustomObjects(lstAllCategories, new string[] { "ClientPASCode", "PASName" }, false, (int)RequisitionExcelTemplateRowStartIndex.RequisitionFieldValueMasterRowStartIndex, (int)RequisitionFieldValueMasterColumn.CategoryCode + 1, lstAllCategories.Count, false, string.Empty, false);
                }

                //write Supplier Ordering Contact
                if (lstSuppliersOrderContacts != null && lstSuppliersOrderContacts.Count() > 0)
                {
                    lstSuppliersOrderContacts = lstSuppliersOrderContacts.OrderBy(x => x.PartnerName).ToList();
                    wsFieldValueMaster.Cells.ImportCustomObjects(lstSuppliersOrderContacts, new string[] { "PartnerName", "Name", "EmailID" }, false, (int)RequisitionExcelTemplateRowStartIndex.RequisitionFieldValueMasterRowStartIndex, (int)RequisitionFieldValueMasterColumn.OrderContactSupplierName + 1, lstSuppliersOrderContacts.Count, false, string.Empty, false);
                }
                //write Supplier Ordering Location
                if (lstSuppliersOrderLocations != null && lstSuppliersOrderLocations.Count() > 0)
                {
                    lstSuppliersOrderLocations = lstSuppliersOrderLocations.OrderBy(x => x.PartnerName).ToList();
                    wsFieldValueMaster.Cells.ImportCustomObjects(lstSuppliersOrderLocations, new string[] { "PartnerName", "ClientLocationCode", "LocationName" }, false, (int)RequisitionExcelTemplateRowStartIndex.RequisitionFieldValueMasterRowStartIndex, (int)RequisitionFieldValueMasterColumn.OrderLocationSupplierName + 1, lstSuppliersOrderLocations.Count, false, string.Empty, false);
                }

                wsTaxCodes = workbook.Worksheets[EXCELSHEET_TAXCODES];
                if (wsTaxCodes != null)
                {
                    if (lstTaxes != null && lstTaxes.Count() > 0)
                    {
                        wsFieldValueMaster.Cells.ImportCustomObjects(lstTaxes, new string[] { "TaxCode", "TaxDescription", "TaxPercentage" }, false, (int)RequisitionExcelTemplateRowStartIndex.RequisitionFieldValueMasterRowStartIndex, (int)RequisitionFieldValueMasterColumn.TaxCodes + 1, lstTaxes.Count, false, string.Empty, false);
                        ExcelLib.Range rngTaxCodes = wsFieldValueMaster.Cells.CreateRange((int)RequisitionExcelTemplateRowStartIndex.RequisitionFieldValueMasterRowStartIndex, (int)RequisitionFieldValueMasterColumn.TaxCodes + 1, lstTaxes.Count, 1);
                        ListValidation(wsFieldValueMaster, rngTaxCodes, SelectCellAreaByColumnIndex(1), wsTaxCodes, 18);
                        wsTaxCodes.Cells.SetColumnWidth(3, 35);
                    }
                }

                if (lstTaxes != null && lstTaxes.Count() > 0 && actionType == RequisitionExcelTemplateHandler.DownloadMasterTemplate)
                {
                    wsFieldValueMaster.Cells.ImportCustomObjects(lstTaxes, new string[] { "TaxCode", "TaxDescription", "TaxPercentage" }, false, (int)RequisitionExcelTemplateRowStartIndex.RequisitionFieldValueMasterRowStartIndex, (int)RequisitionFieldValueMasterColumn.TaxCodes + 1, lstTaxes.Count, false, string.Empty, false);

                }

            }
        }

        private ExcelLib.CellArea SelectCellAreaByColumnIndex(int ExcelColumnIndex)
        {
            ExcelLib.CellArea cellAreaValidation;
            cellAreaValidation.StartRow = 4;
            cellAreaValidation.EndRow = 6500;
            cellAreaValidation.StartColumn = ExcelColumnIndex;
            cellAreaValidation.EndColumn = ExcelColumnIndex;

            return cellAreaValidation;
        }
        private void RemoveExcelCellValidation(ExcelLib.CellArea pCellArea, ExcelLib.Worksheet pWorkSheet)
        {

            // Get the validations collection.
            ExcelLib.ValidationCollection validations = pWorkSheet.Validations;
            // Create a new validation to the validations list.
            ExcelLib.Validation validation = validations[validations.Add()];
            // Set the validation type.
            validation.Type = Aspose.Cells.ValidationType.AnyValue;

            //Add cell area
            validation.AddArea(pCellArea);
        }
        private void ListValidation(ExcelLib.Worksheet wsMasterData, ExcelLib.Range pRange, ExcelLib.CellArea pCellArea, ExcelLib.Worksheet pWorkSheet, int pExcelMasterDataFieldName, bool pShowError = true)
        {
            // Create a range in the second worksheet.
            ExcelLib.Range range = pRange; //Operation

            // Name the range.
            range.Name = "MyRangeTaxCode";

            // Get the validations collection.
            ExcelLib.ValidationCollection validations = pWorkSheet.Validations;

            // Create a new validation to the validations list.
            ExcelLib.Validation validation = validations[validations.Add()];

            // Set the validation type.
            validation.Type = Aspose.Cells.ValidationType.List;

            // Set the operator.
            validation.Operator = ExcelLib.OperatorType.None;

            // Set the in cell drop down.
            validation.InCellDropDown = true;

            // Set the formula1.
            validation.Formula1 = "=" + range.Name;

            // Enable it to show error.
            validation.ShowError = pShowError;

            // Set the alert type severity level.
            validation.AlertStyle = ExcelLib.ValidationAlertType.Information;

            if (validation.ShowError)
            {
                // Set the alert type severity level.
                validation.AlertStyle = ExcelLib.ValidationAlertType.Stop;

                // Set the error title.
                validation.ErrorTitle = "Error";

                // Set the error message.
                validation.ErrorMessage = "Please select a Tax Code from the list";
            }

            //Add cell area
            validation.AddArea(pCellArea);

        }

        public void FillShipFromData()
        {
            wsFieldValueMaster = workbook.Worksheets[EXCELSHEET_MASTER];
            wsTaxCodes = workbook.Worksheets[EXCELSHEET_TAXCODES];
            int maxDataColumn = (int)RequisitionFieldValueMasterColumn.DispatchMode;
            lstShipFomLocations = GetAllShipFromLocations();
            if (wsFieldValueMaster != null)
            {
                if (lstShipFomLocations != null && lstShipFomLocations.Count() > 0)
                {
                    int MaxDateAddingColumn = maxDataColumn + 4;
                    // wsFieldValueMaster.Cells.InsertColumn(maxDataColumn);
                    wsFieldValueMaster.Cells.InsertColumn(MaxDateAddingColumn);
                    wsFieldValueMaster.Cells.InsertColumn(MaxDateAddingColumn + 1);
                    wsFieldValueMaster.Cells.Merge((int)RequisitionExcelTemplateRowStartIndex.RequisitionFieldValueMasterRowStartIndex - 2, MaxDateAddingColumn, 1, 3);
                    wsFieldValueMaster.Cells.SetRowHeight((int)RequisitionExcelTemplateRowStartIndex.RequisitionFieldValueMasterRowStartIndex - 2, 24.75);
                    Aspose.Cells.Cell titleCell = wsFieldValueMaster.Cells[(int)RequisitionExcelTemplateRowStartIndex.RequisitionFieldValueMasterRowStartIndex - 2, MaxDateAddingColumn];
                    titleCell.PutValue("Ship From Location");
                    ExcelLib.Style titleStyle = titleCell.GetStyle();
                    titleStyle.HorizontalAlignment = ExcelLib.TextAlignmentType.Center;
                    titleStyle.Font.Color = Color.White;
                    titleStyle.Font.IsBold = true;
                    titleStyle.Font.Size = 14;
                    titleStyle.Pattern = ExcelLib.BackgroundType.Solid;
                    titleStyle.ForegroundColor = Color.FromArgb(255, 192, 0);
                    titleCell.SetStyle(titleStyle);
                    titleCell.GetMergedRange().SetOutlineBorder(ExcelLib.BorderType.TopBorder, ExcelLib.CellBorderType.Thin, Color.Gray);
                    titleCell.GetMergedRange().SetOutlineBorder(ExcelLib.BorderType.BottomBorder, ExcelLib.CellBorderType.Thin, Color.Gray);
                    titleCell.GetMergedRange().SetOutlineBorder(ExcelLib.BorderType.LeftBorder, ExcelLib.CellBorderType.Thin, Color.Gray);
                    titleCell.GetMergedRange().SetOutlineBorder(ExcelLib.BorderType.RightBorder, ExcelLib.CellBorderType.Thin, Color.Gray);
                    // Partner Name 
                    Aspose.Cells.Cell nameCell = wsFieldValueMaster.Cells[(int)RequisitionExcelTemplateRowStartIndex.RequisitionFieldValueMasterRowStartIndex - 1, MaxDateAddingColumn];
                    nameCell.PutValue("Supplier Name");
                    ExcelLib.Style nameStyle = nameCell.GetStyle();
                    nameStyle.Font.Color = Color.White;
                    nameStyle.Font.IsBold = true;
                    nameStyle.Font.IsItalic = true;
                    nameStyle.Pattern = ExcelLib.BackgroundType.Solid;
                    nameStyle.ForegroundColor = Color.FromArgb(255, 192, 0);
                    nameStyle.SetBorder(ExcelLib.BorderType.TopBorder, ExcelLib.CellBorderType.Thin, Color.Gray);
                    nameStyle.SetBorder(ExcelLib.BorderType.BottomBorder, ExcelLib.CellBorderType.Thin, Color.Gray);
                    nameStyle.SetBorder(ExcelLib.BorderType.LeftBorder, ExcelLib.CellBorderType.Thin, Color.Gray);
                    nameStyle.SetBorder(ExcelLib.BorderType.RightBorder, ExcelLib.CellBorderType.Thin, Color.Gray);
                    nameCell.SetStyle(nameStyle);
                    // Clientlocation code 
                    Aspose.Cells.Cell codeCell = wsFieldValueMaster.Cells[(int)RequisitionExcelTemplateRowStartIndex.RequisitionFieldValueMasterRowStartIndex - 1, MaxDateAddingColumn + 1];
                    codeCell.PutValue("Location Code");
                    ExcelLib.Style codeStyle = codeCell.GetStyle();
                    codeStyle.Font.Color = Color.White;
                    codeStyle.Font.IsBold = true;
                    codeStyle.Font.IsItalic = true;
                    codeStyle.Pattern = ExcelLib.BackgroundType.Solid;
                    codeStyle.ForegroundColor = Color.FromArgb(255, 192, 0);
                    codeStyle.SetBorder(ExcelLib.BorderType.TopBorder, ExcelLib.CellBorderType.Thin, Color.Gray);
                    codeStyle.SetBorder(ExcelLib.BorderType.BottomBorder, ExcelLib.CellBorderType.Thin, Color.Gray);
                    codeStyle.SetBorder(ExcelLib.BorderType.LeftBorder, ExcelLib.CellBorderType.Thin, Color.Gray);
                    codeStyle.SetBorder(ExcelLib.BorderType.RightBorder, ExcelLib.CellBorderType.Thin, Color.Gray);
                    codeCell.SetStyle(codeStyle);
                    // Location Name 
                    Aspose.Cells.Cell codeNameCell = wsFieldValueMaster.Cells[(int)RequisitionExcelTemplateRowStartIndex.RequisitionFieldValueMasterRowStartIndex - 1, MaxDateAddingColumn + 2];
                    codeNameCell.PutValue("Location Name");
                    ExcelLib.Style codeNameStyle = codeNameCell.GetStyle();
                    codeNameStyle.Font.Color = Color.White;
                    codeNameStyle.Font.IsBold = true;
                    codeNameStyle.Font.IsItalic = true;
                    codeNameStyle.Pattern = ExcelLib.BackgroundType.Solid;
                    codeNameStyle.ForegroundColor = Color.FromArgb(255, 192, 0);
                    codeNameStyle.SetBorder(ExcelLib.BorderType.TopBorder, ExcelLib.CellBorderType.Thin, Color.Gray);
                    codeNameStyle.SetBorder(ExcelLib.BorderType.BottomBorder, ExcelLib.CellBorderType.Thin, Color.Gray);
                    codeNameStyle.SetBorder(ExcelLib.BorderType.LeftBorder, ExcelLib.CellBorderType.Thin, Color.Gray);
                    codeNameStyle.SetBorder(ExcelLib.BorderType.RightBorder, ExcelLib.CellBorderType.Thin, Color.Gray);
                    codeNameCell.SetStyle(codeNameStyle);
                    // Importing the data in the field 
                    foreach (PartnerLocation lst in lstShipFomLocations)
                    {
                        wsFieldValueMaster.Cells.ImportCustomObjects(lstShipFomLocations, new string[] { "PartnerName", "ClientLocationCode", "LocationName" }, false, (int)RequisitionExcelTemplateRowStartIndex.RequisitionFieldValueMasterRowStartIndex, MaxDateAddingColumn, lstShipFomLocations.Count, false, string.Empty, false);
                        wsFieldValueMaster.AutoFitColumn(MaxDateAddingColumn);
                        wsFieldValueMaster.Cells.GetColumnWidth(MaxDateAddingColumn);
                        wsFieldValueMaster.AutoFitColumn(MaxDateAddingColumn + 1);
                        wsFieldValueMaster.Cells.GetColumnWidth(MaxDateAddingColumn + 1);
                        wsFieldValueMaster.AutoFitColumn(MaxDateAddingColumn + 2);
                        wsFieldValueMaster.Cells.GetColumnWidth(MaxDateAddingColumn + 2);
                    }
                }
            }
        }


        public List<KeyValue> GetAllSuppliersMappedToDocumentBU()
        {
            //Defaulting for NON PO
            List<RequisitionPartnerInfo> lstPartnerMappedtoBUInfo = new List<RequisitionPartnerInfo>();
            List<KeyValue> lstSuppliers = new List<KeyValue>();
            string partnerStatuses = "";
            partnerStatuses = commonManager.GetSettingsValueByKey(P2PDocumentType.None, "PartnerStatuses", this.UserContext.ContactCode);
            lstPartnerMappedtoBUInfo = RequisitionManger.GetAllBuyerSuppliersAutoSuggest(UserContext.BuyerPartnerCode, partnerStatuses, "", 1, 100000, orgEntityCode, LOBEntityDetailCode, "", 0, this.UserContext.ContactCode);
            if (lstPartnerMappedtoBUInfo != null && lstPartnerMappedtoBUInfo.Count() > 0)
                lstPartnerMappedtoBUInfo.ForEach(supplier => { lstSuppliers.Add(new KeyValue { Value = supplier.ClientPartnerCode, Name = supplier.PartnerName, Id = supplier.PartnerCode }); });
            return lstSuppliers;
        }
        private void GetRequisitionEntityDetailsByDocumentCode()
        {
            lsDocumentHeaderEntitites = RequisitionManger.GetAllHeaderAccountingFieldsForRequisition(documentCode);
            List<DocumentAdditionalEntityInfo> lstDocumentAdditionalEntityInfo = new List<DocumentAdditionalEntityInfo>();
            if (lsDocumentHeaderEntitites != null && lsDocumentHeaderEntitites.Count() > 0)
            {
                lsDocumentHeaderEntitites.ForEach(header =>
                {
                    lstDocumentAdditionalEntityInfo.Add(new DocumentAdditionalEntityInfo
                    {
                        EntityId = header.EntityTypeId,
                        EntityCode = header.EntityCode,
                        EntityDetailCode = header.EntityDetailCode
                    });
                });
            }
            var lstBU = RequisitionManger.GetBUDetailList(lsDocumentHeaderEntitites, documentCode);
            orgEntityCode = string.Join(",", lstBU.Select(x => x.buCode.ToString()).ToArray());
            //addiing the Non Org Entities to get the supplier details for both ORG and Non ORG Entities//
            if (!string.IsNullOrEmpty(orderingLocationbyHeaderEntities))
            {
                List<long> nonOrgBUCodes = new List<long>();
                foreach (var item in lsDocumentHeaderEntitites)
                {
                    if (!item.IsAccountingEntity && orderingLocationbyHeaderEntities.Split('|').Contains(item.EntityTypeId.ToString()))
                    {
                        nonOrgBUCodes.Add(item.EntityDetailCode);
                    }
                }
                nonOrgEntityCode = string.Join(",", nonOrgBUCodes.ToArray());
                orgAndNonOrgEntityCode = string.Concat(orgEntityCode, ",", nonOrgEntityCode);

            }
            else
            {
                orgAndNonOrgEntityCode = orgEntityCode;
            }
        }
        private void GetLOBAndStructureId()
        {
            if (lsDocumentHeaderEntitites != null && lsDocumentHeaderEntitites.Count() > 0)
            {
                structureId = lsDocumentHeaderEntitites[0].StructureId;
                LOBEntityDetailCode = lsDocumentHeaderEntitites[0].LOBEntityDetailCode;
            }
        }

        public List<KeyValue> GetAllShiptToLocations()
        {
            //Defaulting for NON PO
            var EntityMappedtoShiptoLocation = Convert.ToInt32(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "EntityMappedToShipToLocation", this.UserContext.UserId, (int)SubAppCodes.P2P, "", LOBEntityDetailCode));
            List<KeyValue> kvpShipToLocations = new List<KeyValue>();
            if (EntityMappedtoShiptoLocation > 0 && lsDocumentHeaderEntitites.Count() > 0)
            {
                var lstEntityDetails = lsDocumentHeaderEntitites.Where(data => data.EntityTypeId == EntityMappedtoShiptoLocation);
                if (lstEntityDetails != null && lstEntityDetails.Any())
                {
                    ICollection<KeyValuePair<int, string>> lstShipToLocation = commonManager.GetAllShipToLocations(string.Empty, 0, 10000, LOBEntityDetailCode, lstEntityDetails.FirstOrDefault().EntityDetailCode);
                    if (lstShipToLocation != null && lstShipToLocation.Count() > 0)
                    {
                        foreach (KeyValuePair<int, string> kvp in lstShipToLocation)
                        {
                            kvpShipToLocations.Add(new KeyValue { Value = kvp.Key.ToString(), Name = kvp.Value });
                        }
                    }
                }
            }
            return kvpShipToLocations;

        }

        public ICollection<string> GetAllShippingMethods()
        {
            //Defaulting for NON PO
            var EntityMappedToShippingMethods = Convert.ToInt32(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "EntityMappedToShippingMethods", this.UserContext.UserId, (int)SubAppCodes.P2P, "", LOBEntityDetailCode));
            if (EntityMappedToShippingMethods > 0 && lsDocumentHeaderEntitites.Count() > 0)
            {
                var lstEntityDetails = lsDocumentHeaderEntitites.Where(data => data.EntityTypeId == EntityMappedToShippingMethods);
                if (lstEntityDetails != null && lstEntityDetails.Any())
                {
                    return commonManager.GetAllShippingMethods("", 0, 100, lstEntityDetails.FirstOrDefault().EntityDetailCode, LOBEntityDetailCode);

                }
            }
            return null;

        }

        public List<PASMasterData> GetAllCategories()
        {
            return commonManager.GetAllCategoriesByLOB(LOBEntityDetailCode);
        }

        public void GetAccountingFields()
        {
            lstSplitAccountingFields = objDocBO.GetAllSplitAccountingFields(P2PDocumentType.Requisition, LevelType.ItemLevel, structureId);
        }
        protected void GetAllSplitAccountingCodesByDocumentType()
        {
            dataListSplitAccountingFields = RequisitionManger.GetAllSplitAccountingCodesByDocumentType(LevelType.ItemLevel, LOBEntityDetailCode, structureId);
            //Remove Header Level Entities from the List
            if (dataListSplitAccountingFields != null && dataListSplitAccountingFields.Count() > 0 && lsDocumentHeaderEntitites != null && lsDocumentHeaderEntitites.Count() > 0)
            {
                dataListSplitAccountingFields = dataListSplitAccountingFields.Where(x => !lsDocumentHeaderEntitites.Any(y => y.Title.ToUpper().Equals(x.Title.ToUpper()))).OrderByDescending(x => x.FieldOrder).ToList<SplitAccountingFields>();
            }
        }

        protected void GetValidSplitAccountingCodes()
        {
            dataListSplitAccountingFields = RequisitionManger.GetValidSplitAccountingCodes(
                                                                LevelType.ItemLevel,
                                                                LOBEntityDetailCode,
                                                                structureId,
                                                                lstAccountingDataFromUpload,
                                                                SourceSystemEntityId,
                                                                SourceSystemEntityDetailCode);
            //Remove Header Level Entities from the List
            if (dataListSplitAccountingFields != null && dataListSplitAccountingFields.Count() > 0 && lsDocumentHeaderEntitites != null && lsDocumentHeaderEntitites.Count() > 0)
            {
                dataListSplitAccountingFields = dataListSplitAccountingFields
                    .Where(x => !lsDocumentHeaderEntitites
                    .Any(y => y.Title.ToUpper().Equals(x.Title.ToUpper())))
                    .OrderByDescending(x => x.FieldOrder)
                    .ToList<SplitAccountingFields>();
            }
        }

        private List<ContactInfo> GetAllSupplierByLOBAndOrgEntity()
        {
            return commonManager.GetAllSupplierByLOBAndOrgEntity(LOBEntityDetailCode, orgAndNonOrgEntityCode);
        }

        private List<PartnerLocation> GetAllSupplierLocationsByOrgEntity()
        {
            return RequisitionManger.GetAllSupplierLocationsByOrgEntity(orgAndNonOrgEntityCode);
        }

        //private void SetSupplierMappedOrgEntity()
        //{
        //    var PartnerMappedOrgEntityId = Convert.ToInt32(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "PartnerMappedOrgEntityId", UserContext.UserId, (int)SubAppCodes.P2P, "", LOBEntityDetailCode));;
        //    if (PartnerMappedOrgEntityId > 0 && lsDocumentHeaderEntitites.Count() > 0)
        //    {
        //        var lstEntityDetails = lsDocumentHeaderEntitites.Where(data => data.EntityTypeId == PartnerMappedOrgEntityId);
        //        if (lstEntityDetails != null && lstEntityDetails.Any())
        //        {
        //            orgEntityCode = lstEntityDetails.FirstOrDefault().EntityDetailCode;
        //        }
        //    }

        //}
        public T GetInvoiceFieldValue<T>(DataRow pDataRow, Enum pExcelColumns)
        {
            object objValue = string.Empty;

            var enumType = pExcelColumns.GetType();

            if (typeof(T) == typeof(SplitType))
            {
                objValue = GetSplitType(Convert.ToString(pDataRow[Enum.GetName(enumType, pExcelColumns)]));
            }
            else if (typeof(T) == typeof(RequisitionOperation))
            {
                objValue = GetRequisitionOperation(Convert.ToString(pDataRow[Enum.GetName(enumType, pExcelColumns)]));
            }
            else if (pDataRow != null && pDataRow.Table.Columns.Contains(Enum.GetName(enumType, pExcelColumns)))
            {
                objValue = Parse<T>(Convert.ToString(pDataRow[Enum.GetName(enumType, pExcelColumns)]));
            }
            else
            {
                objValue = Parse<T>(string.Empty);
            }
            return (T)objValue;
        }

        private static T Parse<T>(string source)
        {
            try
            {
                if (!String.IsNullOrEmpty(source))
                {
                    if (typeof(T).IsSubclassOf(typeof(Enum)))
                    {
                        return (T)Enum.Parse(typeof(T), source.Trim(), true);
                    }
                    else
                    {
                        return (T)Convert.ChangeType(source.Trim(), typeof(T));
                    }
                }
                else
                {
                    return default(T);
                }
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        private SplitType GetSplitType(string splitType)
        {
            SplitType iSplitType = SplitType.None;

            if (!string.IsNullOrEmpty(splitType))
            {
                switch (splitType.Trim())
                {
                    case "None":
                        iSplitType = SplitType.None;
                        break;
                    case "Percentage":
                        iSplitType = SplitType.Percentage;
                        break;
                    case "Quantity":
                        iSplitType = SplitType.Quantity;
                        break;
                }
            }
            else
                iSplitType = SplitType.None;
            return iSplitType;
        }

        private RequisitionOperation GetRequisitionOperation(string requisitionOperation)
        {
            RequisitionOperation iSplitType = RequisitionOperation.None;

            if (!string.IsNullOrEmpty(requisitionOperation))
            {
                switch (requisitionOperation.Trim())
                {
                    case "None":
                        iSplitType = RequisitionOperation.None;
                        break;
                    case "Add":
                        iSplitType = RequisitionOperation.Add;
                        break;
                    case "Update":
                        iSplitType = RequisitionOperation.Update;
                        break;
                    case "Delete":
                        iSplitType = RequisitionOperation.Delete;
                        break;
                }
            }
            return iSplitType;
        }


        public int GetDataTableColumnIndexByName(DataTable pDataTable, string pDataTableColumnName)
        {
            int iColumnIndex = 0;

            if (!string.IsNullOrEmpty(pDataTableColumnName) && pDataTable.Columns.Contains(pDataTableColumnName.Trim()))
            {
                iColumnIndex = pDataTable.Columns[pDataTableColumnName.Trim()].Ordinal;
            }
            return iColumnIndex;
        }

        protected void LoadMasterDataList()
        {
            lstUOMs = commonManager.GetAllUOMList();
            CSMHelper objCSMWebAPI = new CSMHelper(this.UserContext, this.JWTToken);
            lstCurrency = objCSMWebAPI.GetAllCurrency();
            lstSuppliers = GetAllSuppliersMappedToDocumentBU();
            lstShipFomLocations = GetAllShipFromLocations();
            lstShipToLocations = GetAllShiptToLocations();
            if (enableMultipleLevelCategoryForExcelUpload)
            {
                lstAllCategories = GetAllLevelCategories();
            }
            else
            {
                lstAllCategories = GetAllCategories();
            }
            lstSuppliersOrderContacts = GetAllSupplierByLOBAndOrgEntity();
            lstSuppliersOrderLocations = GetAllSupplierLocationsByOrgEntity();
            lstShippingMethods = GetAllShippingMethods();
            lstTaxes = GetFilteredTaxMasterDetails();
        }

        protected void LoadSettings()
        {
            precessionValue = convertStringToInt(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "MaxPrecessionValue", this.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBEntityDetailCode));
            maxPrecessionforTotal = convertStringToInt(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "MaxPrecessionValueforTotal", this.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBEntityDetailCode));
            maxPrecessionForTaxesAndCharges = convertStringToInt(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "MaxPrecessionValueForTaxesAndCharges", this.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBEntityDetailCode));
            populateDefaultNeedByDate = (commonManager.GetSettingsValueByKey(P2PDocumentType.None, "PopulateDefaultNeedByDate", this.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBEntityDetailCode));
            populateDefaultNeedByDateByDays = convertStringToInt(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "NeedByDateByDays", this.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBEntityDetailCode));
            IsValidateContractNumber = Convert.ToBoolean((commonManager.GetSettingsValueByKey(P2PDocumentType.None, "IsValidateContractNumber", this.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBEntityDetailCode)));
            bool? IsPeriodbasedonNeedbyDate = Convert.ToBoolean(commonManager.GetSettingsValueByKey(P2PDocumentType.Requisition, "IsPeriodbasedonNeedbyDate", this.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBEntityDetailCode));
            AllowTaxExempt = Convert.ToBoolean(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "AllowTaxExempt", this.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBEntityDetailCode));
            DefaultMaterialTaxExempt = Convert.ToBoolean(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "DefaultMaterialTaxExempt", this.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBEntityDetailCode));
            DefaultServiceTaxExempt = Convert.ToBoolean(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "DefaultServiceTaxExempt", this.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBEntityDetailCode));
            var OverrideUnitPriceFromCatalogForExcelUpload = commonManager.GetSettingsValueByKey(P2PDocumentType.Requisition, "OverrideUnitPriceFromCatalogForExcelUploadOfHostedItems", this.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBEntityDetailCode);
            if (!string.IsNullOrEmpty(OverrideUnitPriceFromCatalogForExcelUpload))
                OverrideUnitPriceFromCatalogForExcelUploadOfHostedItems = Convert.ToBoolean(OverrideUnitPriceFromCatalogForExcelUpload);
            var objPartnerBasedOnOrderingLoc = commonManager.GetSettingsValueByKey(P2PDocumentType.None, "IsPartnerContactBasedOnOrderingLocations", this.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBEntityDetailCode);
            if (!string.IsNullOrEmpty(objPartnerBasedOnOrderingLoc))
                isPartnerBasedOnOrderingLoc = Convert.ToBoolean(objPartnerBasedOnOrderingLoc);
            var IsDefaultOrderingLocationValue = commonManager.GetSettingsValueByKey(P2PDocumentType.None, "IsDefaultOrderingLocation", this.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBEntityDetailCode);
            IsDefaultOrderingLocation = !string.IsNullOrEmpty(IsDefaultOrderingLocationValue) ? Convert.ToBoolean(IsDefaultOrderingLocationValue) : false;
            var MapDispatchModetoOrderingLocationValue = commonManager.GetSettingsValueByKey(P2PDocumentType.None, "MapDispatchModeToOrderingLocation", this.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBEntityDetailCode);
            MapDispatchModetoOrderingLocation = !string.IsNullOrEmpty(MapDispatchModetoOrderingLocationValue) ? Convert.ToBoolean(MapDispatchModetoOrderingLocationValue) : false;
            var EnableWebAPIForGetLineItemsValue = commonManager.GetSettingsValueByKey(P2PDocumentType.Requisition, "EnableWebAPIForGetLineItems", this.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBEntityDetailCode);
            EnableWebAPIForGetLineItems = !string.IsNullOrEmpty(EnableWebAPIForGetLineItemsValue) ? Convert.ToBoolean(EnableWebAPIForGetLineItemsValue) : true;
            var EnableGetLineItemsBulkAPIForREQ = commonManager.GetSettingsValueByKey(P2PDocumentType.Requisition, "EnableGetLineItemsBulkAPI", this.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBEntityDetailCode);
            EnableGetLineItemsBulkAPI = !string.IsNullOrEmpty(EnableGetLineItemsBulkAPIForREQ) ? Convert.ToBoolean(EnableGetLineItemsBulkAPIForREQ) : false;

            var EnableAdvancePaymentForREQSetting = commonManager.GetSettingsValueByKey(P2PDocumentType.Requisition, "EnableAdvancePaymentForREQ", this.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBEntityDetailCode);
            EnableAdvancePaymentForREQ = !string.IsNullOrEmpty(EnableAdvancePaymentForREQSetting) ? Convert.ToBoolean(EnableAdvancePaymentForREQSetting) : false;

            var EnableAdvancePaymentSetting = commonManager.GetSettingsValueByKey(P2PDocumentType.None, "EnableAdvancePayment", this.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBEntityDetailCode);
            EnableAdvancePayment = !string.IsNullOrEmpty(EnableAdvancePaymentSetting) ? Convert.ToBoolean(EnableAdvancePaymentSetting) : false;

            minPrecessionValue = convertStringToInt(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "MinPrecessionValue", this.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBEntityDetailCode));

            var EnableContractBulkAPIForREQ = commonManager.GetSettingsValueByKey(P2PDocumentType.Requisition, "EnableContractBulkAPI", this.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBEntityDetailCode);
            EnableContractBulkAPI = !string.IsNullOrEmpty(EnableContractBulkAPIForREQ) ? Convert.ToBoolean(EnableContractBulkAPIForREQ) : false;
            var ShowTaxJurisdictionForShipToValue = commonManager.GetSettingsValueByKey(P2PDocumentType.None, "ShowTaxJurisdictionForShipTo", this.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBEntityDetailCode);
            ShowTaxJurisdictionForShipTo = !string.IsNullOrEmpty(ShowTaxJurisdictionForShipToValue) ? ShowTaxJurisdictionForShipToValue.ToLower() == "yes" ? true : false : false;
            var SupplierItemNumberFieldTypeForREQ = commonManager.GetSettingsValueByKey(P2PDocumentType.Requisition, "SupplierItemNumberFieldType", this.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBEntityDetailCode);
            SupplierItemNumberFieldType = !string.IsNullOrEmpty(SupplierItemNumberFieldTypeForREQ) ? SupplierItemNumberFieldTypeForREQ : "1";
            maxPrecessionForQuantity = convertStringToInt(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "Decimal_MaxPrecessionForQuantity", this.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBEntityDetailCode));
            maxPrecessionForUnitPrice = convertStringToInt(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "Decimal_MaxPrecessionForUnitPrice", this.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBEntityDetailCode));
            maxPrecessionForShippingOrFreight = convertStringToInt(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "Decimal_MaxPrecessionForShippingOrFreight", this.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBEntityDetailCode));
            maxPrecessionForOtherCharges = convertStringToInt(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "Decimal_MaxPrecessionForOtherCharges", this.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBEntityDetailCode));
            OverrideUnitPriceForExcelUploadItemSource = commonManager.GetSettingsValueByKey(P2PDocumentType.Requisition, "OverrideUnitPriceForExcelUploadItemSource", this.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBEntityDetailCode);
            var AllowMultiCurrencyInRequisitionSetting = commonManager.GetSettingsValueByKey(P2PDocumentType.Requisition, "AllowMultiCurrencyInRequisition", this.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBEntityDetailCode);
            AllowMultiCurrencyInRequisition = !string.IsNullOrEmpty(AllowMultiCurrencyInRequisitionSetting) ? Convert.ToBoolean(AllowMultiCurrencyInRequisitionSetting) : false;
        }

        protected void InitializeProxy()
        {
            commonManager = new RequisitionCommonManager(this.JWTToken) { UserContext = this.UserContext, GepConfiguration = RequisitionManger.GepConfiguration };
            //invoiceManager = new InvoiceManager { UserContext = this.UserContext, GepConfiguration = RequisitionManger.GepConfiguration };
            objDocBO = new RequisitionDocumentManager(this.JWTToken) { UserContext = this.UserContext, GepConfiguration = RequisitionManger.GepConfiguration };
            objReqManager = new RequisitionManager(this.JWTToken) { UserContext = this.UserContext, GepConfiguration = RequisitionManger.GepConfiguration };
            proxyPartnerService = new ProxyPartnerService(UserContext, this.JWTToken);
            fileManagerApi = new FileManagerApi(this.UserContext, this.JWTToken);
            proxyPortalService = new ProxyPortalService(this.UserContext, this.JWTToken);
        }
        protected Requisition GetRequisitionDetailsForBulkUploadReqLines(long requisitionId)
        {
            return commonManager.GetRequisitionDetailsForBulkUploadReqLines(requisitionId);
        }

        protected RequisitionItem ValidateItemNumber(string BuyerItemNumber, string SupplierItemNumber, long PartnerCode)
        {
            return commonManager.ValidateItemNumber(BuyerItemNumber, SupplierItemNumber, PartnerCode);
        }
        protected void GetDocumentSourceSystemEntity()
        {
            DocumentLOBDetails objDocumentLOBDetails = commonManager.GetDocumentLOB(documentCode);
            if (objDocumentLOBDetails != null && objDocumentLOBDetails.EntityId > 0)
            {
                SourceSystemEntityId = objDocumentLOBDetails.EntityId;
                SourceSystemEntityDetailCode = objDocumentLOBDetails.EntityDetailCode;
            }
        }
        protected void ChangeAccountingColumnColorStyle()
        {
            foreach (var kv in htItemsTabAccountingColumnIndex.Values)
            {
                Aspose.Cells.Cell titleCell = wsRequisitionLines.Cells[(int)RequisitionExcelTemplateRowStartIndex.RequisitionLinesRowStartIndex, (int)kv];
                ExcelLib.Style titleStyle = titleCell.GetStyle();
                titleStyle.HorizontalAlignment = ExcelLib.TextAlignmentType.Left;
                titleStyle.Font.Color = Color.White;
                titleStyle.Pattern = ExcelLib.BackgroundType.Solid;
                titleStyle.ForegroundColor = Color.FromArgb(255, 100, 0);
                titleCell.SetStyle(titleStyle);
            }

        }
        private List<PartnerLocation> GetAllShipFromLocations()
        {
            int shipFromlocationType = Convert.ToInt16(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "ShipFromLocationType", this.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBEntityDetailCode));
            return commonManager.GetAllSupplierLocationsByLOBAndOrgEntity(LOBEntityDetailCode, orgAndNonOrgEntityCode, shipFromlocationType);
        }
        public List<TaxMaster> GetFilteredTaxMasterDetails()
        {

            return proxyPortalService.GetFilteredTaxMasterDetails().ToList();
        }

        public void FileManagerDetails()
        {
            #region FileManager Methods

            string fileName = IsExcludeMasterDataFromExcelTemplate && actionType == RequisitionExcelTemplateHandler.DownloadMasterTemplate ? "Excel Requisition Master Template.xlsx" : "Excel Requisition Template.xlsx";

            byte[] bytLicenceData;
            bytLicenceData = FileOperations.DownloadByteArrayAsync(blobPath + "/" + asposeLicencePathUrl, MultiRegionConfig.GetConfig(CloudConfig.CloudStorageConn)).GetAwaiter().GetResult();

            objMemoryStream = BytesToStream(bytLicenceData);
            if (objMemoryStream != null)
            {
                license = new ExcelLib.License();
                license.SetLicense(objMemoryStream);
            }
            #endregion

            #region Download Requisition Template from BLOB

            string fileUri = blobPath + "/" + REQUISITION_EXCEL_UPLOAD_TEMPLATEURI + fileName;
            byte[] bFileArray = FileOperations.DownloadByteArrayAsync(fileUri, MultiRegionConfig.GetConfig(CloudConfig.CloudStorageConn)).GetAwaiter().GetResult();
            if (bFileArray != null)
            {
                objFileDetails = new FileManagerEntities.FileDetails();
                objFileDetails.FileData = bFileArray;
            }
            WriteFileDataToWorkbook(objFileDetails.FileData);

            #endregion
        }

        public void UpdatelstSplitAccountingFields()
        {
            if (lstSplitAccountingFields != null && lstSplitAccountingFields.Count() > 0)
                lstSplitAccountingFields = lstSplitAccountingFields.Where(y => y.FieldControls != FieldControls.CustomAttributes && !y.Title.ToUpper().Equals("REQUESTER") && !y.Title.ToUpper().Equals("PERIOD")).OrderByDescending(x => x.FieldOrder).ToList<SplitAccountingFields>();

            //Remove the Source System Entity from List
            if (SourceSystemEntityId > 0)
            {
                lstSplitAccountingFields = lstSplitAccountingFields.Where(x => x.EntityTypeId != SourceSystemEntityId).ToList();
            }
            //Remove Header Level Entities from the List
            if (lstSplitAccountingFields != null && lstSplitAccountingFields.Count() > 0 && lsDocumentHeaderEntitites != null && lsDocumentHeaderEntitites.Count() > 0)
            {
                lstSplitAccountingFields = lstSplitAccountingFields.Where(x => !lsDocumentHeaderEntitites.Any(y => y.Title.ToUpper().Equals(x.Title.ToUpper()))).OrderByDescending(x => x.FieldOrder).ToList<SplitAccountingFields>();
            }
        }

        public void GenerateMasterTemplate()
        {
            FileManagerDetails();
            wsAccountingMaster = workbook.Worksheets[EXCELSHEET_ACCOUNTING_MASTER];
            wsFieldValueMaster = workbook.Worksheets[EXCELSHEET_MASTER];
            UpdatelstSplitAccountingFields();
            FillAccountingMasterData(lstSplitAccountingFields);
            FillFieldValueMaster();
            FillShipFromData();
        }

        public List<PASMasterData> GetAllLevelCategories()
        {
            return RequisitionManger.GetAllLevelCategories(LOBEntityDetailCode);
        }
    }
}
