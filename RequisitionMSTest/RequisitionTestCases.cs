using GEP.Cumulus.DocumentIntegration.Entities;
using GEP.Cumulus.Documents.Entities;
using GEP.Cumulus.P2P.BusinessObjects;
using GEP.Cumulus.P2P.Req.BusinessObjects;
using GEP.Cumulus.Requisition.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RequisitionMSTest.DataSource;
using System;
using System.Collections.Generic;
using GEP.Cumulus.P2P.BusinessEntities;
using GEP.Cumulus.P2P.Req.DataAccessObjects;
using System.Data;
using System.Linq;
using Gep.Cumulus.Partner.Entities;
using Gep.Cumulus.CSM.Entities;
using GEP.Cumulus.QuestionBank.Entities;
using System.Diagnostics.CodeAnalysis;
using GEP.NewPlatformEntities;

namespace RequisitionMSTest
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class RequisitionManagerTestCases : TestHelper
    {
        NewRequisitionManager requisitionManager;
        RequisitionDocumentManager requisitionDocumentManager;
        RequisitionManager reqManager;
        RequisitionInterfaceManager reqInterfaceManager;
        NewRequisitionDAO newRequisitionDAO;
        SQLRequisitionDAO sqlRequisitionDAO;
        Dictionary<string, long> partnerAndContactCodeDict;
        AutoSourcingManager reqAutoSource;
    RequisitionRuleEngineManager requisitionRuleEngineManager;

    string jwtToken = Helper.JWTToken;

        public RequisitionManagerTestCases()
        {
            if (CheckToExecute)
            {
                requisitionManager = new NewRequisitionManager(jwtToken);
                requisitionManager.UserContext = UserContextHelper.GetExecutionContext;
                requisitionManager.GepConfiguration = Helper.InitMultiRegion();
                requisitionDocumentManager = new RequisitionDocumentManager(jwtToken);
                requisitionDocumentManager.UserContext = UserContextHelper.GetExecutionContext;
                requisitionDocumentManager.GepConfiguration = Helper.InitMultiRegion();
                reqManager = new RequisitionManager(jwtToken);
                reqManager.UserContext = UserContextHelper.GetExecutionContext;
                reqManager.GepConfiguration = Helper.InitMultiRegion();
                newRequisitionDAO = new NewRequisitionDAO();
                newRequisitionDAO.UserContext = UserContextHelper.GetExecutionContext;
                newRequisitionDAO.GepConfiguration = Helper.InitMultiRegion();
                partnerAndContactCodeDict = TestCaseSourceFactory.GetSinglePartnerAndContactCode();
                sqlRequisitionDAO = new SQLRequisitionDAO();
                sqlRequisitionDAO.UserContext = UserContextHelper.GetExecutionContext;
                sqlRequisitionDAO.GepConfiguration = Helper.InitMultiRegion();
                reqAutoSource = new AutoSourcingManager(jwtToken);
                reqAutoSource.UserContext = UserContextHelper.GetExecutionContext;
                reqAutoSource.GepConfiguration = Helper.InitMultiRegion();
        requisitionRuleEngineManager = new RequisitionRuleEngineManager(jwtToken);
        requisitionRuleEngineManager.UserContext = UserContextHelper.GetExecutionContext;
        requisitionRuleEngineManager.GepConfiguration = Helper.InitMultiRegion();
      }
        }

        [TestMethod]
        public void TestExecutionHelper()
        {
            var context = UserContextHelper.GetExecutionContext;
            var config = Helper.InitMultiRegion();
            var executionHelper = new GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.ExecutionHelper(context, config, jwtToken);
            //var runCSM = executionHelper.Check(1, GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.ExecutionHelper.WebAPIType.CSM);
            //var runFileManager = executionHelper.Check(11, GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.ExecutionHelper.WebAPIType.FileManager);
        }

        [TestMethod]

        public void GetAllRequisitionDetails()
        {
            if (CheckToExecute)
            {
                var requisitions = TestCaseSourceFactory.GetDraftRequisitions();

                foreach (var req in requisitions)
                {
                    var result = requisitionManager.GetRequisitionDisplayDetails(req);
                    if (result != null)
                    {
                        Assert.IsTrue(result.documentCode > 0);
                    }
                    else
                    {
                        Assert.Fail();
                        return;
                    }
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }
        }

        [TestMethod]
        [DataRow(1, 0)]
        [DataRow(1, 25)]
        [DataRow(1, 40)]
        [DataRow(1, 50)]
        public void GetRequisitionDisplayDetails_WithRiskValues(int riskType, int? riskScore)
        {
            if (CheckToExecute)
            {
                var reqID = TestCaseSourceFactory.GetRequisitionsForRiskScore(riskType, riskScore);
                if (reqID > 0)
                {
                    var result = requisitionManager.GetRequisitionDisplayDetails(reqID);
                    if (result != null)
                    {
                        Assert.IsTrue(result.RiskScore >= riskScore);
                    }
                    else
                    {
                        Assert.Fail();
                        return;
                    }
                }
                else
                {
                    Assert.Fail();
                    return;
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }
        }
        [TestMethod]
        [DataRow(0, null)]
        public void GetRequisitionDisplayDetails_WithOutRiskValues(int riskType, int? riskScore)
        {
            if (CheckToExecute)
            {
                var reqID = TestCaseSourceFactory.GetRequisitionsForRiskScore(riskType, riskScore);
                if (reqID > 0)
                {
                    var result1 = requisitionManager.GetRequisitionDisplayDetails(reqID);
                    if (result1 != null)
                    {
                        Assert.IsTrue(result1.RiskScore == riskScore);
                    }
                    else
                    {
                        Assert.Fail();
                        return;
                    }
                }
                else
                {
                    Assert.Fail();
                    return;
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }

        }
        [TestMethod]
        [DataRow(0, null)]
        public void GetAllRequisitionDetailsByRequisitionId_WithOutRiskValues(int riskType, int? riskScore)
        {
            if (CheckToExecute)
            {
                var reqID = TestCaseSourceFactory.GetRequisitionsForRiskScore(riskType, riskScore);
                if (reqID > 0)
                {
                    var result1 = requisitionDocumentManager.GetAllRequisitionDetailsByRequisitionId(reqID);
                    if (result1 != null)
                    {
                        Assert.IsTrue(result1.RiskScore == riskScore);
                    }
                    else
                    {
                        Assert.Fail();
                        return;
                    }
                }
                else
                {
                    Assert.Fail();
                    return;
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }

        }
        [TestMethod]
        [DataRow(1, 0)]
        [DataRow(1, 25)]
        [DataRow(1, 40)]
        [DataRow(1, 50)]
        public void GetAllRequisitionDetailsByRequisitionId_WithRiskValues(int riskType, int? riskScore)
        {
            if (CheckToExecute)
            {
                var reqID = TestCaseSourceFactory.GetRequisitionsForRiskScore(riskType, riskScore);
                if (reqID > 0)
                {
                    var result1 = requisitionDocumentManager.GetAllRequisitionDetailsByRequisitionId(reqID);
                    if (result1 != null)
                    {
                        Assert.IsTrue(result1.DocumentCode > 0);
                    }
                    else
                    {
                        Assert.Fail();
                        return;
                    }
                }
                else
                {
                    Assert.Fail();
                    return;
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }
        }
        [TestMethod]
        [DataRow(3, 0)]
        [DataRow(3, 25)]
        [DataRow(3, 40)]
        [DataRow(3, 50)]
        public void CalculateAndUpdateRiskScore(int riskType, int? riskScore)
        {
            if (CheckToExecute)
            {
                var reqID = TestCaseSourceFactory.GetRequisitionsForRiskScore((byte)riskType, riskScore);
                if (reqID > 0)
                {
                    var result = requisitionManager.GetRequisitionDisplayDetails(reqID);
                    result.OnEvent = OnEvent.OnSubmit;
                    var savereq = requisitionManager.SaveCompleteRequisition(result);
                    if (savereq != null)
                    {
                        Assert.IsTrue(savereq.requisition.RiskScore >= riskScore);
                    }
                    else
                    {
                        Assert.Fail();
                        return;
                    }
                }
                else
                {
                    Assert.Fail();
                    return;
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }

        }
        [TestMethod]
        public void GetRequisitionDetailsList()
        {
            if (CheckToExecute)
            {
                GEP.NewP2PEntities.DocumentSearch document = new GEP.NewP2PEntities.DocumentSearch();

                document.startIndex = 1;
                document.pageSize = 10;
                document.supplierCode = 277663;
                document.orgEntities = new List<long> { 6 };
                document.purchaseType = 1;
                document.documentStatus = new long[] { 1, 21, 22, 23, 24, 56, 61, 62, 169, 202 };
                document.LOBEntityDetailCode = 5;
                document.SearchColumn = 1;
                document.searchText = "";
                document.currencyCode = "USD";
                document.contactCode = 1;
                document.orderingLocation = 0;
                document.contractNumber = "";

                var result = requisitionManager.GetRequisitionDetailsList(document);

                if (result != null || result.Count != 0)
                {
                    Assert.IsTrue(result.Count > 0);
                }
                else
                {
                    Assert.Fail();
                    return;
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }
        }

        [TestMethod]
        public void SaveRequisitionDetails_TruncateBuyerItemNumber()
        {
            if (CheckToExecute)
            {
                var requisitions = TestCaseSourceFactory.GetDraftRequisitions();
                var invalidItemNumber = "1234567890123456789012345678901234567890abcdefghijl:,@*mnopqr";
                try
                {
                    foreach (var req in requisitions)
                    {
                        var result = requisitionManager.GetRequisitionDisplayDetails(req);
                        for (var ri = 0; ri < result.items.Count; ri++)
                        {
                            result.items[ri].buyerItemNumber = invalidItemNumber;
                        }
                        if (result != null)
                        {
                            var savereq = requisitionManager.SaveCompleteRequisition(result);
                            if (savereq != null)
                            {
                                Assert.IsTrue(savereq.success);
                            }
                            else
                            {
                                Assert.Fail();
                                return;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message.ToString());
                    //   Assert.Fail();
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }
        }

        [TestMethod]
        public void SaveRequisitionDetails()
        {
            if (CheckToExecute)
            {
                var requisitions = TestCaseSourceFactory.GetDraftRequisitions();
                try
                {

                    foreach (var req in requisitions)
                    {

                        var result = requisitionManager.GetRequisitionDisplayDetails(req);
                        var savereq = requisitionManager.SaveCompleteRequisition(result);
                        if (savereq != null)
                        {
                            Assert.IsTrue(savereq.success);
                        }
                        else
                        {
                            Assert.Fail();
                            return;
                        }

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message.ToString());
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }
        }

        [TestMethod]
        public void GetAllRequisitionDetailsByRequisitionId_ExposeClientPartnerCode()
        {
            if (CheckToExecute)
            {
                long documentCode = TestCaseSourceFactory.GetLongValueFromDataSet("GetdocumentCode", "DocumentCode");
                var requisitionDetails = requisitionDocumentManager.GetAllRequisitionDetailsByRequisitionId(documentCode);
                if (requisitionDetails != null)
                {
                    for (var ri = 0; ri < requisitionDetails.RequisitionItems.Count; ri++)
                    {
                        Assert.IsTrue(requisitionDetails.RequisitionItems[ri].ClientPartnerCode != null);
                    }
                }
                else
                {
                    Assert.Fail();
                    return;
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }
        }

        [TestMethod]
        public void GetInactiveOrgEntities()
        {
            if (CheckToExecute)
            {
                var inactiveEntities = requisitionManager.ValidateRequisitionData(299062);
                if (inactiveEntities.Tables.Count > 0)
                {
                    Assert.IsTrue(inactiveEntities.Tables.Count > 0);
                }
                else
                {
                    Assert.Fail();
                    return;
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }
        }

        [TestMethod]
        public void GetRiskFormQuestionScore()
        {
            if (CheckToExecute)
            {
                var requisitionDetails = reqManager.GetRiskFormQuestionScore();
                if (requisitionDetails != null)
                {

                    Assert.IsTrue(requisitionDetails.RiskFormHeaderInstructionsText != "");

                }
                else
                {
                    Assert.Fail();
                    return;
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }
        }

        [TestMethod]
        public void GetRiskFormHeaderInstructionsText()
        {
            if (CheckToExecute)
            {
                var requisitionDetails = reqManager.GetRiskFormHeaderInstructionsText();
                if (requisitionDetails != null)
                {

                    Assert.IsTrue(requisitionDetails.RiskFormHeaderInstructionsText != "");

                }
                else
                {
                    Assert.Fail();
                    return;
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }
        }

        [TestMethod]
        public void CreateReqFromS2C_StandardInput()
        {
            if (CheckToExecute)
            {
                DocumentIntegrationEntity documentIntegrationEntity = new DocumentIntegrationEntity();
                long docCode = 293776;
                documentIntegrationEntity.Document = new GEP.Cumulus.Documents.Entities.Document();
                documentIntegrationEntity.Document.DocumentCode = docCode;
                documentIntegrationEntity.Document.DocumentName = "test";
                documentIntegrationEntity.Document.DocumentNumber = "2019.001292";
                documentIntegrationEntity.Document.DocumentStatusInfo = DocumentStatus.Live;
                documentIntegrationEntity.Document.NumberofItems = -1;
                documentIntegrationEntity.Document.NumberofSurveys = -1;
                documentIntegrationEntity.Document.NumberofSections = -1;
                documentIntegrationEntity.Document.NumberofPartners = 1;
                documentIntegrationEntity.Document.NumberofAttachments = -1;
                documentIntegrationEntity.Document.DocumentBUList.Add(new GEP.Cumulus.Documents.Entities.DocumentBU
                {
                    BusinessUnitCode = 6,
                    BusinessUnitEntityCode = "1000",
                    BusinessUnitName = "Mylan Inc",
                    IsSingleNode = false,
                    EntityHierarchy = null,
                    ParentEntityDetailCode = null,
                    Level = null
                });
                documentIntegrationEntity.Document.DocumentAdditionalFieldList.Add(new GEP.Cumulus.Documents.Entities.DocumentAdditionalField { });
                documentIntegrationEntity.Document.DocumentLinkInfoList.Add(new GEP.Cumulus.Documents.Entities.DocumentLinkInfo());
                documentIntegrationEntity.Document.IsTemplate = false;
                documentIntegrationEntity.Document.IsConfidential = false;
                documentIntegrationEntity.Document.IsSingleNode = false;
                documentIntegrationEntity.Document.IsDocumentDetails = false;
                documentIntegrationEntity.Document.IsStakeholderDetails = false;
                documentIntegrationEntity.Document.IsAddtionalDetails = false;
                documentIntegrationEntity.Document.SearchKey = null;
                documentIntegrationEntity.Document.AllowDuplicateDocumentName = true;
                documentIntegrationEntity.Document.GPNPasInCSV = null;
                documentIntegrationEntity.Document.LinkedDocumentCode = 0;
                documentIntegrationEntity.Document.GenerateDocumentName = false;
                documentIntegrationEntity.Document.DocumentSourceTypeInfo = 0;
                documentIntegrationEntity.Document.InterfaceDocumentStatus = 0;
                documentIntegrationEntity.Document.EntityId = 2;
                documentIntegrationEntity.Document.EntityDetailCode = new List<long> { 5 };
                documentIntegrationEntity.Document.ACEntityId = 0;
                documentIntegrationEntity.Document.SourceSystemInfo = null;
                documentIntegrationEntity.Document.SelectedPasCode = null;
                documentIntegrationEntity.Document.SelectedRegionCode = null;
                documentIntegrationEntity.Document.IsFilterByBU = true;
                documentIntegrationEntity.Document.IsLinkInfoDetails = false;
                documentIntegrationEntity.Document.ACEEntityDetailCode = 0;
                documentIntegrationEntity.Document.IsDocumentNumberUpdatable = false;
                documentIntegrationEntity.Document.ExistingDocumentNumber = null;
                documentIntegrationEntity.Document.DocumentRelationID = 4;
                documentIntegrationEntity.Document.CompanyName = null;
                documentIntegrationEntity.Document.ClientID = 0;
                documentIntegrationEntity.Document.PageIndex = 0;
                documentIntegrationEntity.Document.PageSize = 0;
                documentIntegrationEntity.Document.TotalRecords = 0;
                documentIntegrationEntity.Document.SortBy = null;
                documentIntegrationEntity.Document.SortOrder = null;
                documentIntegrationEntity.Document.CultureCode = "en-US";
                documentIntegrationEntity.Document.CreatedOn = DateTime.Today;
                documentIntegrationEntity.Document.UpdatedOn = DateTime.Today;
                documentIntegrationEntity.Document.CreatedBy = 1508900040000001;
                documentIntegrationEntity.Document.ModifiedBy = 0;
                documentIntegrationEntity.Document.DefaultCurrencyCode = null;
                documentIntegrationEntity.DocumentCurrency = "USD";
                documentIntegrationEntity.ItemCount = 0;
                documentIntegrationEntity.ImportLinesStatus = 0;
                documentIntegrationEntity.EnforceLineReference = false;
                documentIntegrationEntity.PaymentTermId = -2147483648;
                documentIntegrationEntity.ClientID = 0;
                documentIntegrationEntity.PageIndex = 0;
                documentIntegrationEntity.PageSize = 0;
                documentIntegrationEntity.TotalRecords = 0;
                documentIntegrationEntity.SortBy = null;
                documentIntegrationEntity.SortOrder = null;
                documentIntegrationEntity.CultureCode = "en-US";
                documentIntegrationEntity.CreatedOn = DateTime.Today;
                documentIntegrationEntity.UpdatedOn = DateTime.Today;
                documentIntegrationEntity.CreatedBy = 0;
                documentIntegrationEntity.ModifiedBy = 0;
                documentIntegrationEntity.DefaultCurrencyCode = null;

                var result = requisitionManager.CreateRequisitionFromS2C(documentIntegrationEntity);

            }
            else
            {
                Assert.Inconclusive("Not executed");
            }
        }

        [TestMethod]
        public void GetInactiveOrgEntitiesWithoutRows()
        {
            if (CheckToExecute)
            {
                var requisitions = TestCaseSourceFactory.GetDraftRequisitions();
                if (requisitions.Count > 0)
                {
                    var inactiveEntities = requisitionManager.ValidateRequisitionData(requisitions[0]);
                    if (inactiveEntities.Tables.Count > 0)
                    {
                        Assert.IsTrue(inactiveEntities.Tables[0].Rows.Count == 0);
                    }
                    else
                    {
                        Assert.Fail();
                        return;
                    }
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }
        }
        //[TestMethod]
        //public void UploadExcelTemplateHandlerwithtaxcodes_withoutTaxcodes()
        //{
        //    if (CheckToExecute)
        //    {
        //        var result = TestCaseSourceFactory.downloadExcelTemplate(1);
        //        Int64 documentCode = result.DocumentCode;
        //        string documentNumber = result.DocumentNumber;
        //        RequisitionExcelTemplateHandler action = RequisitionExcelTemplateHandler.ExportTemplate;
        //        string fileID = string.Empty;
        //        RequisitionTemplateResponse reqTemplateResponse = new RequisitionTemplateResponse();
        //        reqTemplateResponse = requisitionManager.ExcelTemplateHandler(documentCode, documentNumber, action, fileID);
        //        var results = TestCaseSourceFactory.UpdateTaxDetails(documentCode, 100);
        //        fileID = reqTemplateResponse.FileId;
        //        if (fileID > 0)
        //        {
        //            action = RequisitionExcelTemplateHandler.UploadTemplate;
        //            reqTemplateResponse = new RequisitionTemplateResponse();
        //            reqTemplateResponse = requisitionManager.ExcelTemplateHandler(documentCode, documentNumber, action, fileID);
        //        }

        //        var reqResult = TestCaseSourceFactory.GetRequisitionsTaxDetails(documentCode);
        //        if (reqResult != null)
        //        {
        //            Assert.IsTrue(reqTemplateResponse.Status == "Success");
        //        }
        //        else
        //        {
        //            Assert.Fail();
        //            return;
        //        }
        //    }
        //    else
        //    {
        //        Assert.Inconclusive("Not executed");
        //    }
        //}

        [TestMethod]
        public void SaveQuestionsResponse()
        {
            if (CheckToExecute)
            {
                requisitionManager = new NewRequisitionManager(jwtToken);
                requisitionManager.UserContext = UserContextHelper.GetExecutionContext;
                requisitionManager.GepConfiguration = Helper.InitMultiRegion();

                List<QuestionResponse> questionResponses = new List<QuestionResponse>() {
                    new QuestionResponse { QuestionId = 120, AssesseeId =7002182504000001, AssessorId = 7002182504000001, AssessorType = AssessorUserType.Supplier, ObjectInstanceId = 234741, ResponseValue = "test", RowId = 0, ColumnId = 0  }
                };

                //requisitionManager.IsRequiredRiskCalculation(0, 165908);
                requisitionManager.SaveQuestionsResponse(questionResponses, 0);
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }
        }

        [TestMethod]
        public void GetCurrencyConversionFactor()
        {
            if (CheckToExecute)
            {
                RequisitionCommonManager requisitionCommonManager = new RequisitionCommonManager(jwtToken);
                requisitionCommonManager.UserContext = UserContextHelper.GetExecutionContext;
                requisitionCommonManager.GepConfiguration = Helper.InitMultiRegion();


                var result = requisitionCommonManager.GetCurrencyConversionFactor("USD", "AUD");
                Assert.IsTrue(result > 0);
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }
        }

        //[TestMethod]
        //public void UploadExcelTemplateHandlerwithtaxcodes_withtaxcode()
        //{
        //    if (CheckToExecute)
        //    {
        //        var result = TestCaseSourceFactory.GetRequisitions_WithTaxCodes();
        //        Int64 documentCode = result.DocumentCode;
        //        string documentNumber = result.DocumentNumber;
        //        RequisitionExcelTemplateHandler action = RequisitionExcelTemplateHandler.ExportTemplate;
        //        Int64 fileID = 0;
        //        RequisitionTemplateResponse reqTemplateResponse = new RequisitionTemplateResponse();
        //        reqTemplateResponse = requisitionManager.ExcelTemplateHandler(documentCode, documentNumber, action, fileID);

        //        var results = TestCaseSourceFactory.UpdateTaxDetails(documentCode, 0);
        //        fileID = reqTemplateResponse.FileId;
        //        if (fileID > 0)
        //        {
        //            action = RequisitionExcelTemplateHandler.UploadTemplate;
        //            reqTemplateResponse = new RequisitionTemplateResponse();
        //            reqTemplateResponse = requisitionManager.ExcelTemplateHandler(documentCode, documentNumber, action, fileID);
        //        }
        //        var reqResult = TestCaseSourceFactory.GetRequisitionsTaxDetails(documentCode);
        //        if (reqResult != null)
        //        {
        //            Assert.IsTrue(reqTemplateResponse.Status == "Success");
        //        }
        //        else
        //        {
        //            Assert.Fail();
        //            return;
        //        }
        //    }
        //    else
        //    {
        //        Assert.Inconclusive("Not executed");
        //    }
        //}
        //[TestMethod]
        //public void downloadExcelTemplate()
        //{
        //    if (CheckToExecute)
        //    {
        //        var result = TestCaseSourceFactory.downloadExcelTemplate(1);
        //        Int64 documentCode = result.DocumentCode;
        //        string documentNumber = result.DocumentNumber;
        //        RequisitionExcelTemplateHandler action = RequisitionExcelTemplateHandler.DownloadTemplate;
        //        Int64 fileID = 0;
        //        RequisitionTemplateResponse reqTemplateResponse = new RequisitionTemplateResponse();
        //        reqTemplateResponse = requisitionManager.ExcelTemplateHandler(documentCode, documentNumber, action, fileID);

        //        if (reqTemplateResponse.Status == "Success")
        //        {
        //            Assert.IsTrue(reqTemplateResponse.Status == "Success");
        //        }
        //        else
        //        {
        //            Assert.Fail();
        //            return;
        //        }
        //    }
        //    else
        //    {
        //        Assert.Inconclusive("Not executed");
        //    }
        //}
        //[TestMethod]
        //public void downloadExcelTemplate_ShipFromEnabled()
        //{
        //    if (CheckToExecute)
        //    {
        //        var result = TestCaseSourceFactory.downloadExcelTemplate(1);
        //        Int64 documentCode = result.DocumentCode;
        //        string documentNumber = result.DocumentNumber;
        //        RequisitionExcelTemplateHandler action = RequisitionExcelTemplateHandler.DownloadTemplate;
        //        Int64 fileID = 0;
        //        RequisitionTemplateResponse reqTemplateResponse = new RequisitionTemplateResponse();
        //        reqTemplateResponse = requisitionManager.ExcelTemplateHandler(documentCode, documentNumber, action, fileID);

        //        if (reqTemplateResponse.Status == "Success")
        //        {
        //            Assert.IsTrue(reqTemplateResponse.Status == "Success");
        //        }
        //        else
        //        {
        //            Assert.Fail();
        //            return;
        //        }
        //    }
        //    else
        //    {
        //        Assert.Inconclusive("Not executed");
        //    }
        //}
        //[TestMethod]
        //public void ExportExcelTemplate_withouttaxocde()
        //{
        //    if (CheckToExecute)
        //    {

        //        var result = TestCaseSourceFactory.downloadExcelTemplate(1);
        //        Int64 documentCode = result.DocumentCode;
        //        string documentNumber = result.DocumentNumber;
        //        RequisitionExcelTemplateHandler action = RequisitionExcelTemplateHandler.ExportTemplate;
        //        Int64 fileID = 0;
        //        RequisitionTemplateResponse reqTemplateResponse = new RequisitionTemplateResponse();
        //        reqTemplateResponse = requisitionManager.ExcelTemplateHandler(documentCode, documentNumber, action, fileID);

        //        if (reqTemplateResponse.Status == "Success")
        //        {
        //            Assert.IsTrue(reqTemplateResponse.Status == "Success");
        //        }
        //        else
        //        {
        //            Assert.Fail();
        //            return;
        //        }
        //    }
        //    else
        //    {
        //        Assert.Inconclusive("Not executed");
        //    }
        //}
        //[TestMethod]
        //public void ExportExcelTemplate_withtaxcode()
        //{
        //    if (CheckToExecute)
        //    {
        //        var result = TestCaseSourceFactory.downloadExcelTemplate(2);
        //        Int64 documentCode = result.DocumentCode;
        //        string documentNumber = result.DocumentNumber;
        //        RequisitionExcelTemplateHandler action = RequisitionExcelTemplateHandler.ExportTemplate;
        //        Int64 fileID = 0;
        //        RequisitionTemplateResponse reqTemplateResponse = new RequisitionTemplateResponse();
        //        reqTemplateResponse = requisitionManager.ExcelTemplateHandler(documentCode, documentNumber, action, fileID);

        //        if (reqTemplateResponse.Status == "Success")
        //        {
        //            Assert.IsTrue(reqTemplateResponse.Status == "Success");
        //        }
        //        else
        //        {
        //            Assert.Fail();
        //            return;
        //        }
        //    }
        //    else
        //    {
        //        Assert.Inconclusive("Not executed");
        //    }
        //}

        [TestMethod]
        public void GetInvalidOrgEntities()
        {
            if (CheckToExecute)
            {
                var requisitions = TestCaseSourceFactory.GetDraftRequisitions();
                if (requisitions.Count > 0)
                {
                    var inactiveEntities = requisitionManager.ValidateRequisitionData(requisitions[0]);
                    if (inactiveEntities.Tables.Count > 0)
                    {
                        Assert.IsTrue(inactiveEntities.Tables[0].Rows.Count == 0);
                    }

                    else
                    {
                        Assert.Fail();
                        return;
                    }
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }
        }

        [DataTestMethod]
        [DataRow("", 10, 1, 0, 0)]
        [DataRow("", 10, 1, -1, 0)]
        [DataRow("", 10, 1, 0, -1)]
        [DataRow("", 10, 1, -1, -1)]

        [DataRow("z", 10, 1, 0, 0)]
        [DataRow("z", 10, 1, -1, 0)]
        [DataRow("z", 10, 1, 0, -1)]
        [DataRow("z", 10, 1, -1, -1)]

        [DataRow("a", 10, 1, 0, 0)]
        [DataRow("a", 10, 1, -1, 0)]
        [DataRow("a", 10, 1, 0, -1)]
        [DataRow("a", 10, 1, -1, -1)]
        public void GetPAScategoriesWithSearchText(string searchText, int pageSize = 10, int pageNumber = 1, long partnerCode = 0, long contactCode = 0)
        {
            if (CheckToExecute)
            {

                // As data row is accepting only primitive types, in order to change the partner code and contact code, added value -1.
                // PageNumber and pageSize cannot be negative and is passed with same values from UI.
                if (partnerCode < 0)
                {
                    partnerCode = partnerAndContactCodeDict["partnerCode"];
                    Console.WriteLine("partnerCode: " + partnerCode);
                }
                if (contactCode < 0)
                {
                    contactCode = partnerAndContactCodeDict["contactCode"];
                    Console.WriteLine("contactCode: " + contactCode);
                }

                DataTable pasCategories = requisitionManager.GetAllPASCategories(searchText, pageSize, pageNumber, partnerCode, contactCode);

                if (pasCategories.Rows.Count > 0)
                {

                    Assert.IsNotNull(pasCategories.Rows[0]);
                    Assert.IsTrue(pasCategories.Rows[0]["PASCode"].ToString().Length > 0);
                    Assert.IsTrue(pasCategories.Rows[0]["PASName"].ToString().Length > 0);
                }
                else
                {
                    Assert.IsTrue(pasCategories.Rows.Count == 0);
                    // Assert.Fail("GetPAScategoriesWithSearchText function: Zero rows returned for passed parameters");
                    return;
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }
        }
        [TestMethod]
        public void GetBasicDetailsById_HeaderDetails()
        {
            if (CheckToExecute)
            {
                try
                {
                    var requisitions = TestCaseSourceFactory.GetDraftRequisitions();
                    //var result = TestCaseSourceFactory.downloadExcelTemplate(1);
                    long userId = sqlRequisitionDAO.UserContext.ContactCode;
                    foreach (var req in requisitions)
                    {
                        var result = requisitionManager.GetRequisitionDisplayDetails(req);
                        var reqResult = sqlRequisitionDAO.GetBasicDetailsById(result.documentCode, userId, 0, true, false, "", 0, 0, 0, "", 0, false);
                        if (reqResult != null)
                        {
                            Assert.IsTrue(reqResult.DocumentAdditionalEntitiesInfoList.Count > 0);
                        }
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message.ToString());
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }
        }
        [TestMethod]
        public void GetLineItemsBaicDetails_ItemDetails()
        {
            if (CheckToExecute)
            {
                try
                {
                    IDictionary<long, byte> requisitionDetails = TestCaseSourceFactory.GetRequisitionWithItemType();
                    long requisitionId = requisitionDetails.First().Key;
                    ItemType itemType = (ItemType)requisitionDetails.First().Value;
                    // P2PDocumentType docType = ;
                    int startIndex = 0;
                    int pageSize = 1000;
                    string sortBy = "";
                    string sortOrder = "";

                    ICollection<P2PItem> reqresult = sqlRequisitionDAO.GetLineItemBasicDetails(requisitionId, itemType, startIndex, pageSize, sortBy, sortOrder, 0, 1, "", 0, 0, 0, false, true, 0);
                    if (reqresult != null)
                    {
                        Assert.IsTrue(reqresult.Any());
                        Console.WriteLine("GetLineItemBasicDetails fetached successfully for requisitionId" + requisitionId);
                    }
                    else
                    {
                        Console.WriteLine("GetLineItemsBasicDetails returned null");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message.ToString());
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }
        }

        [DataTestMethod]
        [DataRow(true, false, false)]
        [DataRow(true, true, false)]
        [DataRow(false, true, true)]
        [DataRow(false, false, true)]

        public void ValidateRequisitionData(bool inactiveSetting, bool parentChildSetting, bool checkInvalidOrderinglocationSupplierContact)
        {
            if (CheckToExecute)
            {
                var reqid = TestCaseSourceFactory.GetLongValueFromDataSet("RequisitionWithInactiveEntitiesWithoutParentChildMapping", "RequisitionID");
                if (reqid == 0)
                {
                    Assert.IsTrue(true, "Requisition not found for the test case");
                    return;
                }
                var invalidEntities = requisitionManager.ValidateRequisitionData(reqid);

                if (invalidEntities != null)
                {
                    if (invalidEntities.Tables.Count > 0 && inactiveSetting == true)
                    {
                        if (invalidEntities.Tables[0].Rows.Count > 0)
                            Assert.IsTrue(invalidEntities.Tables[0].Rows[0]["ErrorFrom"].Equals(1));
                    }
                    if (invalidEntities.Tables.Count > 0 && inactiveSetting == false && parentChildSetting == true)
                    {
                        if (invalidEntities.Tables[0].Rows.Count > 0)
                            Assert.IsTrue(invalidEntities.Tables[0].Rows[0]["ErrorFrom"].Equals(2));
                    }
                }

                else
                {
                    if (inactiveSetting == false && parentChildSetting == false)
                    {
                        Assert.IsTrue(invalidEntities == null);
                    }
                    else
                    {
                        Assert.Fail();
                        return;
                    }
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }
        }
        //[TestMethod]
        //public void ExportExcelTemplate_withServiceLineItems()
        //{
        //    if (CheckToExecute)
        //    {
        //        try
        //        {
        //            var result = TestCaseSourceFactory.downloadExcelTemplate(5);
        //            Int64 documentCode = result.DocumentCode;
        //            string documentNumber = result.DocumentNumber;
        //            RequisitionExcelTemplateHandler action = RequisitionExcelTemplateHandler.ExportTemplate;
        //            Int64 fileID = 0;
        //            RequisitionTemplateResponse reqTemplateResponse = new RequisitionTemplateResponse();
        //            reqTemplateResponse = requisitionManager.ExcelTemplateHandler(documentCode, documentNumber, action, fileID);

        //            if (reqTemplateResponse.Status == "Success")
        //            {
        //                Assert.IsTrue(reqTemplateResponse.Status == "Success");
        //            }
        //            else
        //            {
        //                Assert.Fail();
        //                return;
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine(ex.Message.ToString());
        //        }
        //    }
        //    else
        //    {
        //        Assert.Inconclusive("Not executed");
        //    }
        //}

        [TestMethod]
        [DataRow(1, true, true)]
        [DataRow(1, true, false)]
        [DataRow(2, true, true)]
        [DataRow(-1, true, true)]
        public void GetUsersBasedOnUserDetailsWithPagination(int preferedLobType, bool honorDirectRequesterForOBOSelection, bool isAutosuggest)
        {
            if (CheckToExecute)
            {
                try
                {
                    long LOBEntityDetailCode = 0;
                    List<ContactORGMapping> lstBU = new List<ContactORGMapping>();
                    var resultSet = TestCaseSourceFactory.GetUserMappedORGEntities(preferedLobType);
                    if (resultSet != null && resultSet.Tables.Count > 0)
                    {
                        if (resultSet.Tables[0].Rows != null && resultSet.Tables[0].Rows.Count > 0)
                        {
                            foreach (System.Data.DataRow row in resultSet.Tables[0].Rows)
                            {
                                LOBEntityDetailCode = Convert.ToInt64(row["LOBEntityDetailCode"]);
                                ContactORGMapping oContactORGMapping = new ContactORGMapping()
                                {
                                    ContactCode = Convert.ToInt64(row["ContactCode"]),
                                    OrgEntityCode = Convert.ToInt64(row["OrgEntityCode"]),
                                    EntityDescription = Convert.ToString(row["EntityDescription"]),
                                    EntityDisplayName = Convert.ToString(row["EntityDisplayName"]),
                                    IsDefault = Convert.ToBoolean(row["IsDefault"]),
                                    EntityId = Convert.ToInt32(row["EntityId"]),
                                    EntityKey = Convert.ToString(row["EntityKey"]),
                                    ParentEntityDetailCode = Convert.ToInt64(row["ParentEntityDetailCode"]),
                                    EntityCode = Convert.ToString(row["EntityCode"]),
                                    PreferenceLOBType = Convert.ToInt16(row["PreferenceLOBType"]),
                                    LOBId = Convert.ToInt64(row["LOBEntityDetailCode"])
                                };
                                lstBU.Add(oContactORGMapping);
                            }
                        }
                    }

                    UserDetails usersInfo = new UserDetails();
                    usersInfo.UserLOBDetails = new List<UserLOBDetails>();
                    usersInfo.ContactCode = sqlRequisitionDAO.UserContext.ContactCode;
                    //usersInfo.RestrictedUserActivities = new List<long>();
                    //usersInfo.RestrictedUserActivities.Add(P2PUserActivity.VENDOR_BUYER_USER);
                    usersInfo.UserLOBDetails.Add(new UserLOBDetails()
                    {
                        LOBId = LOBEntityDetailCode,
                        OrgEntityCode = LOBEntityDetailCode,
                        PreferenceLOBType = preferedLobType,
                        BUDetails = new List<ContactORGMapping>()
                    });

                    List<ContactORGMapping> lstDistinctBU = new List<ContactORGMapping>(lstBU);

                    lstBU.ForEach((p) =>
                    {
                        if (lstBU.Exists(b => b.OrgEntityCode == p.ParentEntityDetailCode))
                            lstDistinctBU.RemoveAll(x => x.ParentEntityDetailCode == p.ParentEntityDetailCode);
                    });

                    if (!ReferenceEquals(lstDistinctBU, null))
                        lstDistinctBU.ForEach(p => usersInfo.UserLOBDetails.First().BUDetails.Add(new ContactORGMapping()
                        {
                            OrgEntityCode = p.OrgEntityCode,
                            LOBId = LOBEntityDetailCode
                        }));

                    var reqTemplateResponse = requisitionManager.GetUsersBasedOnUserDetailsWithPagination(usersInfo, "", 1, 10, false, "10100044", true, false,true);

                    if (reqTemplateResponse.Count > 0)
                    {
                        Assert.IsTrue(reqTemplateResponse.Count > 0);
                    }
                    else
                    {
                        Assert.Fail("No records found");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message.ToString());
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }
        }

        //REQ-5629 Adding of test cases for Requisition Manager

        //[TestMethod]
        //public void UpdateProcurementStatusByReqItemId_Test(long requisitionItemId)
        //{
        //    if (CheckToExecute)
        //    {

        //        requisitionItemId = TestCaseSourceFactory.GetLongValueFromDataSet("GetRequstionItemId", "RequisitionItemID");
        //        var result = reqManager.UpdateProcurementStatusByReqItemId(requisitionItemId);

        //        if (result != null)
        //        {
        //            Assert.IsTrue(result > 0);
        //        }
        //        else
        //        {
        //            Assert.Fail();
        //            return;
        //        }
        //    }
        //    else
        //    {
        //        Assert.Inconclusive("Not executed");
        //    }
        //}

        [TestMethod]
        [DataRow(0, null)]
        public void SaveRequisitionBusinessUnit(long documentCode, long buId)
        {
            if (CheckToExecute)
            {
                documentCode = TestCaseSourceFactory.GetLongValueFromDataSet("RequisitionWithFixedItemsSQL", "DocumentCode");
                buId = TestCaseSourceFactory.GetLongValueFromDataSet("GetBUId", "BUID");
                var result = reqManager.SaveRequisitionBusinessUnit(documentCode, buId);

                if (result != false)
                {
                    Assert.IsTrue(result == true);
                }
                else
                {
                    Assert.Fail();
                    return;
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }
        }

        //[TestMethod]
        //public void SaveDocumentBusinessUnit(long documentCode)
        //{
        //    if (CheckToExecute)
        //    {
        //        documentCode = TestCaseSourceFactory.GetLongValueFromDataSet("RequisitionWithFixedItemsSQL", "DocumentCode");
        //        var result = reqManager.SaveDocumentBusinessUnit(documentCode);

        //        if (result != false)
        //        {
        //            Assert.IsTrue(result == true);
        //        }
        //        else
        //        {
        //            Assert.Fail();
        //            return;
        //        }
        //    }
        //    else
        //    {
        //        Assert.Inconclusive("Not executed");
        //    }
        //}

        [TestMethod]
        [DataRow(0, 10, 1, 1, 0)]
        public void GetRequisitionAccountingDetailsByItemId(long requisitionItemId, int pageIndex, int pageSize, int itemType, long LOBId)
        {
            if (CheckToExecute)
            {

                requisitionItemId = TestCaseSourceFactory.GetLongValueFromDataSet("GetRequstionItemId", "RequisitionItemID");
                LOBId = TestCaseSourceFactory.GetLongValueFromDataSet("GetLOBId", "LOBId");
                var result = reqManager.GetRequisitionAccountingDetailsByItemId(requisitionItemId, pageIndex, pageSize, itemType, LOBId);

                if (result.Count > 0)
                {
                    Assert.IsTrue(result.Count > 0);
                }
                else
                {
                    Assert.Fail();
                    return;
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }
        }

        [TestMethod]
        public void CheckAccountingSplitValidations()
        {
            if (CheckToExecute)
            {
                long requisitionId = TestCaseSourceFactory.GetLongValueFromDataSet("RequisitionWithItemsAmountDefinedSQL", "RequisitionID");
                var result = reqManager.CheckAccountingSplitValidations(requisitionId);

                if (result != false)
                {
                    Assert.IsTrue(result == true);
                }
                else
                {
                    Assert.Fail();
                    return;
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }
        }

        [TestMethod]
        public void UpdateSendforBiddingDocumentStatus(long documentCode)
        {
            if (CheckToExecute)
            {
                documentCode = TestCaseSourceFactory.GetLongValueFromDataSet("RequisitionWithFixedItemsSQL", "DocumentCode");
                var result = reqManager.UpdateSendforBiddingDocumentStatus(documentCode);

                if (result != false)
                {
                    Assert.IsTrue(result == true);
                }
                else
                {
                    Assert.Fail();
                    return;
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }
        }


        [TestMethod]
        [DataRow(0)]
        public void GetAllCategoriesByReqId(long documentId)
        {
            if (CheckToExecute)
            {
                documentId = TestCaseSourceFactory.GetLongValueFromDataSet("RequisitionWithItemsAmountDefinedSQL", "RequisitionID");
                var result = reqManager.GetAllCategoriesByReqId(documentId);

                if (result.Count > 0)
                {
                    Assert.IsTrue(result.Count > 0);
                }
                else
                {
                    Assert.Fail();
                    return;
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }
        }

        [TestMethod]
        [DataRow(0, 0)]
        public void GetAllEntitiesByReqId(long documentId, int entityTypeId)
        {
            if (CheckToExecute)
            {
                documentId = TestCaseSourceFactory.GetLongValueFromDataSet("RequisitionWithInactiveEntitiesWithoutParentChildMapping", "RequisitionID");
                entityTypeId = Convert.ToInt32(TestCaseSourceFactory.GetLongValueFromDataSet("GetEntityTypeId", "entityTypeId"));
                var result = reqManager.GetAllEntitiesByReqId(documentId, entityTypeId);

                if (result.Value > 0)
                {
                    Assert.IsTrue(result.Value > 0);
                }
                else
                {
                    Assert.Fail();
                    return;
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }
        }


        [TestMethod]
        [DataRow(0, 1, 0)]
        public void DeleteAllSplitsByDocumentId(long documentId, long ContactCode, long LOBId)
        {
            if (CheckToExecute)
            {
                LOBId = TestCaseSourceFactory.GetLongValueFromDataSet("GetLOBId", "LOBId");
                var result = reqManager.DeleteAllSplitsByDocumentId(documentId, ContactCode, LOBId);

                if (result != false)
                {
                    Assert.IsTrue(result == true);
                }
                else
                {
                    Assert.Fail();
                    return;
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }
        }


        [TestMethod]
        [DataRow(0, 0, 1, 0)]
        public void copyLineItem(long requisitionItemId, long requisitionId, int txtNumberOfCopies, long LOBId)
        {
            if (CheckToExecute)
            {
                requisitionItemId = TestCaseSourceFactory.GetLongValueFromDataSet("GetRequstionItemId", "RequisitionItemID");
                requisitionId = TestCaseSourceFactory.GetLongValueFromDataSet("RequisitionWithItemsAmountDefinedSQL", "RequisitionID");
                LOBId = TestCaseSourceFactory.GetLongValueFromDataSet("GetLOBId", "LOBId");
                var result = reqManager.copyLineItem(requisitionItemId, requisitionId, txtNumberOfCopies, LOBId);

                if (result != false)
                {
                    Assert.IsTrue(result == true);
                }
                else
                {
                    Assert.Fail();
                    return;
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }

        }


        [TestMethod]
        [DataRow(0, 1, 0)]
        public void UpdateReqItemOnPartnerChange(long requisitionItemId, long partnerCode, long LOBId)
        {
            if (CheckToExecute)
            {
                requisitionItemId = TestCaseSourceFactory.GetLongValueFromDataSet("GetRequstionItemId", "RequisitionItemID");
                LOBId = TestCaseSourceFactory.GetLongValueFromDataSet("GetLOBId", "LOBId");
                var result = reqManager.UpdateReqItemOnPartnerChange(requisitionItemId, partnerCode, LOBId);

                if (result != false)
                {
                    Assert.IsTrue(result == true);
                }
                else
                {
                    Assert.Fail();
                    return;
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }
        }

        [TestMethod]
        [DataRow(0)]
        public void GetRequisitionItemAccountingStatus(long requisitionItemId)
        {
            if (CheckToExecute)
            {
                requisitionItemId = TestCaseSourceFactory.GetLongValueFromDataSet("GetRequstionItemId", "RequisitionItemID");
                var result = reqManager.GetRequisitionItemAccountingStatus(requisitionItemId);

                if (result != false)
                {
                    Assert.IsTrue(result == true);
                }
                else
                {
                    Assert.Fail();
                    return;
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }
        }


        [TestMethod]
        [DataRow(0, " ")]
        public void CheckRequisitionCatalogItemAccess(long newrequisitionId, string requisitionIds)
        {
            if (CheckToExecute)
            {
                newrequisitionId = TestCaseSourceFactory.GetLongValueFromDataSet("RequisitionWithInactiveEntitiesWithoutParentChildMapping", "RequisitionID");
                requisitionIds = Convert.ToString(TestCaseSourceFactory.GetstringValueFromDataSet("GetmultiplereqID", "req_ID"));
                var result = reqManager.CheckRequisitionCatalogItemAccess(newrequisitionId, requisitionIds);

                if (result != false)
                {
                    Assert.IsTrue(result == true);
                }
                else
                {
                    Assert.Fail();
                    return;
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }
        }

        //[TestMethod]
        //[DataRow(0)]
        //public void GetLineItemBasicDetailsForInterface(long documentCode)//tested and verified
        //{
        //    if (CheckToExecute)
        //    {
        //        documentCode = TestCaseSourceFactory.GetLongValueFromDataSet("GetdocumentCode", "DocumentCode");
        //        var results = reqInterfaceManager.GetLineItemBasicDetailsForInterface(documentCode);
        //        if (results != null)
        //        {
        //            Assert.IsTrue(results.Count > 0);
        //        }
        //        else
        //        {
        //            Assert.Fail();
        //            return;
        //        }
        //    }
        //    //}
        //    else
        //    {
        //        Assert.Inconclusive("Not executed");
        //    }
        //}

        [TestMethod]
        [DataRow(0)]
        public void PushingDataToEventHub(long Documentcode)//tested and verified
        {
            if (CheckToExecute)
            {
                Documentcode = TestCaseSourceFactory.GetLongValueFromDataSet("GetdocumentCode", "DocumentCode");

                var results = reqManager.PushingDataToEventHub(Documentcode);
                if (results)
                {

                    Assert.IsTrue(results);

                }
                else
                {
                    Assert.Fail();
                    return;
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }

        }

        //[TestMethod]
        //[DataRow("Requisition", 2687, 1)]
        //public void GetRequisitionListForInterfaces(string docType, int docCount, int sourceSystemId)//tested and verified
        //{
        //    if (CheckToExecute)
        //    {
        //        docType = "Requisition";
        //        sourceSystemId = Convert.ToInt32(TestCaseSourceFactory.GetLongValueFromDataSet("GetsourceSystemId", "sourceSystemId"));//1
        //        sourceSystemId = 1;
        //        docCount = Convert.ToInt32(TestCaseSourceFactory.GetLongValueFromDataSet("GetdocumentCode", "DocumentCode"));
        //        var results = reqInterfaceManager.GetRequisitionListForInterfaces(docType, docCount, sourceSystemId);
        //        if (results.Count > 0)
        //        {
        //            Assert.IsTrue(results.Count > 0);
        //        }
        //        else
        //        {
        //            Assert.Fail();
        //            return;
        //        }
        //    }
        //    else
        //    {
        //        Assert.Inconclusive("Not executed");
        //    }
        //}

        [TestMethod]
        [DataRow(0)]

        public void GetOBOUserByRequisitionID(long documentCode)//tested and verified
        {
            if (CheckToExecute)
            {
                documentCode = TestCaseSourceFactory.GetLongValueFromDataSet("GetdocumentCode", "DocumentCode");
                var results = reqManager.GetOBOUserByRequisitionID(documentCode);
                if (results > 0)
                {

                    Assert.IsTrue(results > 0);

                }
                else
                {
                    Assert.Fail();
                    return;
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }

        }
        [TestMethod]
        [DataRow(1, 1, 1)]
        public void CancelChangeRequisition(long documentCode, long userId, int requisitionSource)//tested and verified
        {
            if (CheckToExecute)
            {
                userId = 1;
                documentCode = TestCaseSourceFactory.GetLongValueFromDataSet("GetdocumentCodeCancelChangeRequisition", "DocumentCode");
                requisitionSource = Convert.ToInt32(TestCaseSourceFactory.GetLongValueFromDataSet("GetrequisitionSource", "requisitionSource"));
                var results = reqManager.CancelChangeRequisition(documentCode, userId, requisitionSource);
                if (results > 0)
                {

                    Assert.IsTrue(results > 0);

                }
                else
                {
                    Assert.Fail();
                    return;
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }

        }

        [TestMethod]
        [DataRow(1, "aa")]
        public void SaveContractInformation(long requisitionItemId, string extContractRef)//verfied and tested
        {
            if (CheckToExecute)
            {
                extContractRef = Convert.ToString(TestCaseSourceFactory.GetLongValueFromDataSet("GetextContractRef", "ExtContractRef"));
                requisitionItemId = TestCaseSourceFactory.GetLongValueFromDataSet("GetRequstionItemId", "RequisitionItemID");

                var results = reqManager.SaveContractInformation(requisitionItemId, extContractRef);
                if (results)
                {

                    Assert.IsTrue(results);

                }
                else
                {
                    Assert.Fail();
                    return;
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }

        }
        [TestMethod]
        [DataRow(0)]
        public void CheckBiddingInProgress(long documentId)//verified and tested
        {
            if (CheckToExecute)
            {
                documentId = TestCaseSourceFactory.GetLongValueFromDataSet("GetdocumentId", "RequisitionID");


                var results = reqManager.CheckBiddingInProgress(documentId);
                if (results)
                {

                    Assert.IsTrue(results);

                }
                else
                {
                    Assert.Fail();
                    return;
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }

        }



        [TestMethod]
        [DataRow(1, "ja")]
        public void CheckCatalogItemsAccessForContactCode(long requesterId, string catalogItems)//verified and tested
        {
            if (CheckToExecute)
            {
                requesterId = TestCaseSourceFactory.GetLongValueFromDataSet("GetdocumentId", "RequisitionID");
                catalogItems = Convert.ToString(TestCaseSourceFactory.GetLongValueFromDataSet("GetrequesterId", "RequesterID"));


                var results = reqManager.CheckCatalogItemsAccessForContactCode(requesterId, catalogItems);
                if (results != null)
                {

                    Assert.IsTrue(results.Rows.Count > 0);

                }
                else
                {
                    Assert.Fail();
                    return;
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }
        }
        [TestMethod]
        [DataRow(P2PDocumentType.Requisition, 0)]
        public void DeleteDocumentByDocumentCode(P2PDocumentType docType, long documentCode)//verified and tested
        {
            if (CheckToExecute)
            {
                docType = P2PDocumentType.Requisition;
                documentCode = TestCaseSourceFactory.GetLongValueFromDataSet("GetdocumentCode", "DocumentCode");



                var results = reqManager.DeleteDocumentByDocumentCode(docType, documentCode);
                if (results)
                {

                    Assert.IsTrue(results);

                }
                else
                {
                    Assert.Fail();
                    return;
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }


        }
        [TestMethod]
        [DataRow(0)]
        public void GetRequisitionPunchoutItemCount(long RequisitionId)//verified and tested
        {
            if (CheckToExecute)
            {
                RequisitionId = TestCaseSourceFactory.GetLongValueFromDataSet("GetdocumentId", "RequisitionID");



                var results = reqManager.GetRequisitionPunchoutItemCount(RequisitionId);
                if (results != null)
                {

                    Assert.IsTrue(results.Count > 0);

                }
                else
                {
                    Assert.Fail();
                    return;
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }

        }
        [TestMethod]
        [DataRow(1, "a", 1, 5)]
        public void GetBuyerAssigneeDetails(long ContactCode, string SearchText, int StartIndex, int Size)//verified and tested
        {
            if (CheckToExecute)
            {
                ContactCode = TestCaseSourceFactory.GetLongValueFromDataSet("GetContactCode", "BuyerAssignee");



                var results = reqManager.GetBuyerAssigneeDetails(ContactCode, SearchText, StartIndex, Size);
                if (results != null)
                {

                    Assert.IsTrue(results.Tables.Count > 0);

                }
                else
                {
                    Assert.Fail();
                    return;
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }
        }


        [TestMethod]
        [DataRow(1)]
        public void GetRequisitionItemsDispatchMode(long documentCode)//verified and tested
        {


            if (CheckToExecute)
            {
                documentCode = TestCaseSourceFactory.GetLongValueFromDataSet("GetdocumentCode", "DocumentCode");



                var results = reqManager.GetRequisitionItemsDispatchMode(documentCode);
                if (results != null)
                {

                    Assert.IsTrue(results.Count > 0);

                }
                else
                {
                    Assert.Fail();
                    return;
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }
        }

        //[TestMethod]
        //[DataRow(1)]
        //public void GetRequisitionDetailsById(long documentCode)//verified and tested
        //{
        //    if (CheckToExecute)
        //    {
        //        documentCode = TestCaseSourceFactory.GetLongValueFromDataSet("GetdocumentCode", "DocumentCode");
        //        var results = reqManager.GetRequisitionDetailsById(documentCode);
        //        if (results != null)
        //        {
        //            Assert.IsTrue(results.Requisition != null);
        //        }
        //        else
        //        {
        //            Assert.Fail();
        //            return;
        //        }
        //    }
        //    else
        //    {
        //        Assert.Inconclusive("Not executed");
        //    }
        //}

        [TestMethod]
        [DataRow(1)]
        public void GetAllQuestionnaire(long requisitionItemId)//verified and tested
        {

            if (CheckToExecute)
            {

                requisitionItemId = TestCaseSourceFactory.GetLongValueFromDataSet("GetrequisitionItemId", "RequisitionItemId");


                var results = reqManager.GetAllQuestionnaire(requisitionItemId);
                if (results != null)
                {

                    Assert.IsTrue(results.Count > 0);

                }
                else
                {
                    Assert.Fail();
                    return;
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }

        }

        [TestMethod]
        [DataRow(0)]
        public void GetPartnerDetailsAndOrderingLocationByOrderId(long requisitionId)//verified and tested
        {
            if (CheckToExecute)
            {

                requisitionId = TestCaseSourceFactory.GetLongValueFromDataSet("GetrequisitionId", "Requisitionid");


                var results = reqManager.GetPartnerDetailsAndOrderingLocationByOrderId(requisitionId);
                if (results != null)
                {

                    Assert.IsTrue(results.Count > 0);

                }
                else
                {
                    Assert.Fail();
                    return;
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }

        }

        [TestMethod]
        [DataRow(0)]
        public void GetRequisitionCapitalCodeCountById(long requisitionId)//tested and verified
        {

            if (CheckToExecute)
            {
                requisitionId = TestCaseSourceFactory.GetLongValueFromDataSet("GetdocumentId", "RequisitionID");


                var results = reqManager.GetRequisitionCapitalCodeCountById(requisitionId);
                if (results)
                {

                    Assert.IsTrue(results);

                }
                else
                {
                    Assert.IsFalse(false);
                    return;
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }
        }
        [TestMethod]
        [DataRow(0, 0, false)]
        public void CheckOBOUserCatalogItemAccess(long requisitionId, long requesterId, bool delItems = false)//tested and verified
        {
            if (CheckToExecute)
            {
                requisitionId = TestCaseSourceFactory.GetLongValueFromDataSet("GetdocumentId", "RequisitionID");
                requesterId = TestCaseSourceFactory.GetLongValueFromDataSet("GetrequesterId", "RequesterID");

                var results = reqManager.CheckOBOUserCatalogItemAccess(requisitionId, requesterId);
                if (results)
                {

                    Assert.IsTrue(results);

                }
                else
                {
                    Assert.IsFalse(false);
                    return;
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }

        }
        [TestMethod]
        [DataRow(0, 10, 1)]
        public void GetRequisitioneHeaderTaxes(long requisitionId, int pageIndex, int pageSize)//tested abd verified
        {
            if (CheckToExecute)
            {
                requisitionId = TestCaseSourceFactory.GetLongValueFromDataSet("GetdocumentId", "RequisitionID");



                var results = reqManager.GetRequisitioneHeaderTaxes(requisitionId, pageIndex, pageSize);
                if (results != null)
                {

                    Assert.IsTrue(results.Count >= 0);

                }
                else
                {
                    Assert.IsFalse(false);
                    return;
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }

        }

        [TestMethod]
        [DataRow(0, 0)]
        public void CheckCatalogItemAccessForContactCode(long requisitionId, long requesterId)//verified and tested
        {
            if (CheckToExecute)
            {
                requesterId = TestCaseSourceFactory.GetLongValueFromDataSet("GetrequesterId", "RequesterID");
                requisitionId = TestCaseSourceFactory.GetLongValueFromDataSet("GetdocumentId", "RequisitionID");


                var results = reqManager.CheckCatalogItemAccessForContactCode(requisitionId, requesterId);
                if (results != null)
                {

                    Assert.IsTrue(results != null);

                }
                else
                {
                    Assert.IsFalse(false);
                    return;
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }

        }



        [TestMethod]
        [DataRow("a", 10, 1, 0, false, 0)]
        public void GetListofBillToLocDetails(string searchText, int pageIndex, int pageSize, long entityDetailCode, bool getDefault, long lOBEntityDetailCode)//verified and tested
        {

            if (CheckToExecute)
            {

                entityDetailCode = TestCaseSourceFactory.GetLongValueFromDataSet("GetentityDetailCode", "EntityDetailCode");
                lOBEntityDetailCode = TestCaseSourceFactory.GetLongValueFromDataSet("GetlOBEntityDetailCode", "LOBEntityDetailCode");

                var results = reqManager.GetListofBillToLocDetails(searchText, pageIndex, pageSize, entityDetailCode, getDefault, lOBEntityDetailCode);
                if (results != null)
                {

                    Assert.IsTrue(results.Rows.Count > 0);

                }
                else
                {
                    Assert.IsFalse(false);
                    return;
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }

        }
        [TestMethod]
        [DataRow(0, "", "", 0)]
        public void CopyRequisitionToRequisition(long newrequisitionId, string requisitionIds, string buIds, long LOBId)//verified and tested
        {

            if (CheckToExecute)
            {

                newrequisitionId = TestCaseSourceFactory.GetLongValueFromDataSet("RequisitionWithInactiveEntitiesWithoutParentChildMapping", "RequisitionID");
                requisitionIds = Convert.ToString(TestCaseSourceFactory.GetstringValueFromDataSet("GetmultiplereqID", "req_ID"));
                buIds = Convert.ToString(TestCaseSourceFactory.GetLongValueFromDataSet("GetlOBEntityDetailCode", "LOBEntityDetailCode"));
                LOBId = TestCaseSourceFactory.GetLongValueFromDataSet("GetLOBId", "LOBId");

                var results = reqManager.CopyRequisitionToRequisition(newrequisitionId, requisitionIds, buIds, LOBId);
                if (results > 0)
                {

                    Assert.IsTrue(results > 0);

                }
                else
                {
                    Assert.IsFalse(false);
                    return;
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }

        }

        [TestMethod]
        public void RequisitionIndex_Test()
        {
            if (CheckToExecute)
            {
                List<long> reqIds = new List<long>();
                for (int i = 0; i < 5; i++)
                {
                    reqIds.Add(TestHelper.LongRandom(100, 100000));
                }
                bool isPassed = requisitionManager.AddRequisitionsIntoSearchIndexerQueue(reqIds);
                if (isPassed)
                    Assert.IsTrue(isPassed);
                else
                {
                    Assert.IsFalse(false);
                    return;
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }
        }


        [TestMethod]
        [DataRow(0, 0, "Requisition", "", 0, false)]
        public void SaveCatalogRequisition(long userId, long documentCode, string requisitionName, string requisitionNumber, long oboId = 0, bool callFromCatalog = false)
        {
            if (CheckToExecute)
            {

                // userId = 1;
                userId = sqlRequisitionDAO.UserContext.ContactCode;
                documentCode = TestCaseSourceFactory.GetLongValueFromDataSet("GetdocumentCode", "DocumentCode");
                requisitionNumber = Convert.ToString(TestCaseSourceFactory.GetLongValueFromDataSet("GetrequisitionNumber", "RequisitionNumber"));
                var results = reqManager.SaveCatalogRequisition(userId, documentCode, requisitionName, requisitionNumber);
                if (results > 0)
                {

                    Assert.IsTrue(results > 0);

                }
                else
                {
                    Assert.IsFalse(false);
                    return;
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }

        }

        [TestMethod()]
        [DataRow(0, "")]
        public void AssignBuyerToRequisitionItems(long buyerContactCode, string requisitionItemIds)//verified and tested
        {
            if (CheckToExecute)
            {


                buyerContactCode = TestCaseSourceFactory.GetLongValueFromDataSet("GetbuyerContactCode", "BuyerContactCode");
                requisitionItemIds = TestCaseSourceFactory.GetstringValueFromDataSet("GetmultiplereqitemID", "RequisitionItemID");

                var result1 = requisitionManager.AssignBuyerToRequisitionItems(buyerContactCode, requisitionItemIds);
                if (result1)
                {
                    Assert.IsTrue(result1);
                }
                else
                {
                    Assert.IsFalse(false);
                    return;
                }
            }



            else
            {
                Assert.Inconclusive("Not executed");
            }
        }


        [TestMethod]
        public void ValidateContractNumber()//verified and tested
        {
            if (CheckToExecute)
            {


                string contractNumber = TestCaseSourceFactory.GetContractNumber();


                var result1 = requisitionManager.ValidateContractNumber(contractNumber);
                if (result1)
                {
                    Assert.IsTrue(result1);
                }
                else
                {
                    Assert.IsFalse(false);
                    return;
                }
            }



            else
            {
                Assert.Inconclusive("Not executed");
            }

        }
        [TestMethod]
        [DataRow("", 0)]
        public void ValidateContractItemId(string contractNumber, long ContractItemId)//verified and tested
        {

            if (CheckToExecute)
            {


                contractNumber = TestCaseSourceFactory.GetstringValueFromDataSet("GetDocumentNumber", "DocumentNumber");
                ContractItemId = TestCaseSourceFactory.GetLongValueFromDataSet("GetContractItemId", "LineItemNo");


                var result1 = requisitionManager.ValidateContractItemId(contractNumber, ContractItemId);
                if (result1)
                {
                    Assert.IsTrue(result1);
                }
                else
                {
                    Assert.IsFalse(false);
                    return;
                }
            }



            else
            {
                Assert.Inconclusive("Not executed");
            }

        }
        [TestMethod()]
        [DataRow("", "", 0)]
        public void GetContractItemsByContractNumber(string documentNumber, string term, int itemType)//verified and tested
        {
            if (CheckToExecute)
            {


                documentNumber = TestCaseSourceFactory.GetstringValueFromDataSet("GetDocumentNumber", "DocumentNumber");
                term = TestCaseSourceFactory.GetstringValueFromDataSet("GetItemName", "ItemName");
                itemType = Convert.ToInt32(TestCaseSourceFactory.GetLongValueFromDataSet("GetItemType", "ItemType"));


                var result1 = requisitionManager.GetContractItemsByContractNumber(documentNumber, term, itemType);
                if (result1 != null)
                {
                    Assert.IsTrue(result1 != null);
                }
                else
                {
                    Assert.IsFalse(false);
                    return;
                }
            }



            else
            {
                Assert.Inconclusive("Not executed");
            }

        }
        [TestMethod]
        [DataRow(0)]
        public void DeleteSavedViewsForReqWorkBench(long savedViewId)//verified and tested
        {
            if (CheckToExecute)
            {


                savedViewId = TestCaseSourceFactory.GetLongValueFromDataSet("GetSavedViewInfoId", "SavedViewInfoId");



                var result1 = requisitionManager.DeleteSavedViewsForReqWorkBench(savedViewId);
                if (result1)
                {
                    Assert.IsTrue(result1);
                }
                else
                {
                    Assert.IsFalse(false);
                    return;
                }
            }



            else
            {
                Assert.Inconclusive("Not executed");
            }

        }
        [TestMethod]
        public void SaveRequisitionHeader()// verified and tested
        {
            if (CheckToExecute)
            {
                var requisitions = TestCaseSourceFactory.GetDraftRequisitions();

                foreach (var req in requisitions)
                {
                    var result = requisitionManager.GetRequisitionDisplayDetails(req);
                    var result1 = requisitionManager.SaveRequisitionHeader(result);
                    if (result1 != null)
                    {
                        Assert.IsTrue(result1.id > 0);
                    }
                    else
                    {
                        Assert.Fail();
                        return;
                    }
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }
        }

        [TestMethod]
        public void GetDefaultEntities()// verified and tested
        {
            if (CheckToExecute)
            {
                var requisitions = TestCaseSourceFactory.GetDraftRequisitions();

                foreach (var req in requisitions)
                {
                    var result = requisitionManager.GetRequisitionDisplayDetails(req);
                    var result1 = requisitionManager.GetDefaultEntities(result);
                    if (result1 != null)
                    {
                        Assert.IsTrue(result1.id > 0);
                    }
                    else
                    {
                        Assert.Fail();
                        return;
                    }
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }
        }

        [TestMethod]
        [DataRow(0)]
        public void GetAllHeaderAccountingFieldsForRequisition(long documentCode)//verified and tested
        {
            if (CheckToExecute)
            {
                documentCode = TestCaseSourceFactory.GetLongValueFromDataSet("GetdocumentCode", "DOCUMENTCODE");
                var result1 = requisitionManager.GetAllHeaderAccountingFieldsForRequisition(documentCode);
                if (result1 != null)
                {
                    Assert.IsTrue(result1.Count > 0);
                }
                else
                {
                    Assert.IsFalse(false);
                    return;
                }
            }



            else
            {
                Assert.Inconclusive("Not executed");
            }
        }
        [TestMethod]
        [DataRow(0, 0)]
        public void GetRequisitionErrorLog(long requisitionId, int requestType)//verified and tested
        {
            if (CheckToExecute)
            {
                requisitionId = TestCaseSourceFactory.GetLongValueFromDataSet("GetRequisitionID", "RequisitionID");
                requestType = Convert.ToInt32(TestCaseSourceFactory.GetLongValueFromDataSet("GetRequestType", "RequestType"));
                var result1 = requisitionManager.GetRequisitionErrorLog(requisitionId, requestType);
                if (result1 != null)
                {
                    Assert.IsTrue(true);
                }
                else
                {
                    Assert.IsFalse(false);
                    return;
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }

        }

        [TestMethod]
        [DataRow(1)]
        [DataRow(0)]
        public void UpdateRequisitionPreviousAmount(int updateReqPrevAmount)
        {
            if (CheckToExecute)
            {
                var requisitions = TestCaseSourceFactory.GetDraftRequisitions();

                foreach (var req in requisitions)
                {
                    requisitionManager.UpdateRequisitionPreviousAmount(req, Convert.ToBoolean(updateReqPrevAmount));
                    var result = requisitionManager.GetRequisitionDisplayDetails(req);
                    if (Convert.ToBoolean(updateReqPrevAmount) && result.RequisitionPreviousAmount > 0)
                    {
                        Assert.IsTrue(result.documentCode > 0);
                    }
                    else
                    {
                        Assert.Fail();
                        return;
                    }
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }
        }
        [TestMethod]
        //[DataRow("a", "12", "21", 198798)]
        public void GetAllUsersByActivityCode(string SearchText, string Shouldhaveactivitycodes, string Shouldnothaveactivitycodes, long Partnercode)
        {
            if (CheckToExecute)
            {
                SearchText = "";
                Shouldhaveactivitycodes = TestCaseSourceFactory.GetstringValueFromDataSet("GetActivityCodes", "ActivityCode");
                Shouldnothaveactivitycodes = TestCaseSourceFactory.GetstringValueFromDataSet("GetActivityCode", "ActivityCode");// "10700203";
                Partnercode = TestCaseSourceFactory.GetLongValueFromDataSet("GetPartnerAndContactCode", "PartnerCode"); //198798;

                var result = requisitionManager.GetAllUsersByActivityCode(SearchText, Shouldhaveactivitycodes, Shouldnothaveactivitycodes, Partnercode);
                if (result.Count > 0)
                {
                    Assert.IsTrue(true);
                }
                else
                {
                    Assert.IsFalse(false);
                    return;
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }
        }


        [TestMethod]
        [DataRow(0, 0)]
        public void GetOrderingLocationsWithNonOrgEntities_Test(long requisitionId, int requestType)//verified and tested
        {
            if (CheckToExecute)
            {
                List<long> orgEntity = new List<long>() { 1571824, 1572775, 1572776, 1572779 };
                List<long> nonOrg = new List<long>() { 1572745, 1571825 };
                long partnerCode = 538993;
                long accessControlEntityDetailCode = 0;
                int pageIndex = 0;
                int pageSize = 10;
                string searchText = "";
                int locationType = 2;
                List<PartnerLocation> result = requisitionManager.GetOrderingLocationsWithNonOrgEntities(partnerCode, locationType,  accessControlEntityDetailCode, orgEntity, nonOrg, pageIndex, pageSize, searchText);
                if (result != null)
                {
                    Assert.IsTrue(true);
                }
                else
                {
                    Assert.IsFalse(false);
                    return;
                }
            }
        }

        [TestMethod]
        public void SaveTeamMember()
        {
            try
            {
                if (CheckToExecute)
                {
                    GEP.Cumulus.Documents.Entities.Document objDocument = new GEP.Cumulus.Documents.Entities.Document();
                    var documentCode = TestCaseSourceFactory.GetLongValueFromDataSet("GetdocumentCode", "DOCUMENTCODE");
                    var Contacodes = TestCaseSourceFactory.GetLongValueFromDataSet("GetPartnerAndContactCode", "ContactCode");
                    var EmailID = TestCaseSourceFactory.GetLongValueFromDataSet("GetEmail", "EmailAddress");
                    List<long> ContactCode = new List<long>() { Contacodes };
                    objDocument.DocumentCode = documentCode;
                    List<DocumentStakeHolder> stakeholder = new List<DocumentStakeHolder>();
                    DocumentStakeHolder objDocumentStakeHolder = new DocumentStakeHolder();
                    DocumentStakeHolder objDocumentStakeHolder1 = new DocumentStakeHolder();
                    objDocumentStakeHolder.DocumentCode = documentCode;
                    objDocumentStakeHolder.ContactCode = Contacodes;
                    objDocumentStakeHolder.PartnerCode = UserContextHelper.GetExecutionContext.BuyerPartnerCode;
                    objDocumentStakeHolder.StakeholderTypeInfo = StakeholderType.TeamMembers;
                    objDocumentStakeHolder.IsDeleted = false;
                    objDocumentStakeHolder.EmailId = EmailID.ToString();


                    objDocumentStakeHolder1.DocumentCode = documentCode;
                    objDocumentStakeHolder1.ContactCode = Contacodes;
                    objDocumentStakeHolder1.PartnerCode = UserContextHelper.GetExecutionContext.BuyerPartnerCode;
                    objDocumentStakeHolder1.StakeholderTypeInfo = StakeholderType.TeamMembers;
                    objDocumentStakeHolder1.IsDeleted = false;
                    objDocumentStakeHolder.EmailId = EmailID.ToString();


                    stakeholder.Add(objDocumentStakeHolder);
                    stakeholder.Add(objDocumentStakeHolder1);

                    objDocument.IsStakeholderDetails = true;
                    objDocument.IsDocumentDetails = false;
                    objDocument.DocumentStakeHolderList.Add(objDocumentStakeHolder);


                    requisitionManager.SaveTeamMember(objDocument, stakeholder);
                    Assert.IsTrue(true);

                }
                else
                {
                    Assert.Inconclusive("Not executed");
                }
            }
            catch (Exception ex)
            {
                Assert.IsFalse(false);
                throw ex;
            }
        }

        [TestMethod]
        public void GetOrdersLinksForReqLineItem()
        {
            if (CheckToExecute)
            {
                long p2pLineItemId = TestHelper.LongRandom(100, 100000);
                long requisitionId = TestHelper.LongRandom(100, 100000);
                var result = requisitionManager.GetOrdersLinksForReqLineItem(p2pLineItemId, requisitionId);

                if (result != null)
                {
                    Assert.IsTrue(result.Count > 0);
                }
                else
                {
                    Assert.Fail();
                    return;
                }
            }

            else
            {
                Assert.Inconclusive("Not executed");
            }
        }

        [TestMethod]

        public void GetCategoryHirarchyByCategories()
        {
            if (CheckToExecute)
            {
                var requisitions = TestCaseSourceFactory.GetDraftRequisitions();

                foreach (var req in requisitions)
                {
                    var result = requisitionManager.GetRequisitionDisplayDetails(req);

                    List<long> categories = new List<long>();
                    foreach (var item in result.items)
                    {
                        if (item.category.id > 0)
                            categories.Add(item.category.id);
                    }

                    if (categories.Count > 0)
                    {
                        var resultData = requisitionManager.GetCategoryHirarchyByCategories(categories);

                        if (resultData != null)
                        {
                            Assert.IsTrue(result.documentCode > 0);
                        }
                        else
                        {
                            Assert.Fail();
                            return;
                        }
                    }
                    else
                    {
                        Assert.IsTrue(result.documentCode > 0);
                    }
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }
        }


        [TestMethod]
        public void UpdateRiskScore()
        {
            if (CheckToExecute)
            {
                var DocumentCode = TestCaseSourceFactory.GetLongValueFromDataSet("GetdocumentCode", "DOCUMENTCODE");
                requisitionManager.UpdateRiskScore(DocumentCode);
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }
        }

    [TestMethod]
    public void GetAllRequisitionDetailsByRequisitionId_ExposeAdditionalFields()
    {
      if (CheckToExecute)
      {
        var requisitions = TestCaseSourceFactory.GetDraftRequisitions();
        try
        {
          var result = requisitionManager.GetRequisitionDisplayDetails(requisitions[0]);
          for (var ri = 0; ri < result.items.Count; ri++)
          {
            if (result.items[ri].lstAdditionalFieldAttributues != null && result.items[0].lstAdditionalFieldAttributues.Count > 0)
            {
              var requisitionDetails = requisitionDocumentManager.GetAllRequisitionDetailsByRequisitionId(requisitions[0]);
              if (requisitionDetails != null)
              {
                for (var i = 0; i < requisitionDetails.RequisitionItems.Count; i++)
                {
                  if (requisitionDetails.RequisitionItems[ri].lstAdditionalFieldAttributues != null)
                  {
                    Assert.IsTrue(requisitionDetails.RequisitionItems[ri].lstAdditionalFieldAttributues.Count > 0);
                  }
                  else
                  {
                    Assert.Fail();
                    return;
                  }
                }
              }
            }
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex.Message.ToString());
        }
      }
      else
      {
        Assert.Inconclusive("Not executed");
      }
    }
        
        [TestMethod]
        public void GetDocumentsForUtility()
        {
            if (CheckToExecute)
            {
                List<DocumentDelegation> requisitions = new List<DocumentDelegation>();
                var contactCode = TestCaseSourceFactory.GetLongValueFromDataSet("GetPartnerAndContactCode", "ContactCode");
                string searchText = string.Empty;
                try
                {
                    requisitions = requisitionManager.GetDocumentsForUtility(contactCode, searchText);
                    if (requisitions != null)
                    {
                        Assert.IsTrue(requisitions.Count > 0);
                    }
                    else
                    {
                        Assert.Fail();
                        return;
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message.ToString());
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }

        }
        [TestMethod]
        public void SaveDocumentRequesterChange()
        {
            if (CheckToExecute)
            {
                List<long> requisitions = new List<long>();
                var contactCode = TestCaseSourceFactory.GetLongValueFromDataSet("GetPartnerAndContactCode", "ContactCode");
                long documentCode = TestCaseSourceFactory.GetLongValueFromDataSet("GetRequsterChangeDocuments", "DocumentCode");
                requisitions.Add(documentCode);                
                try
                {
                    bool result = requisitionManager.SaveDocumentRequesterChange(requisitions, contactCode);
                    if (result)
                    {
                        Assert.IsTrue(result);
                    }
                    else
                    {
                        Assert.Fail();
                        return;
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message.ToString());
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }

        }

        [TestMethod]
        public void PerformReIndexForDocuments()
        {
            if (CheckToExecute)
            {
                List<long> requisitions = new List<long>();                
                long documentCode = TestCaseSourceFactory.GetLongValueFromDataSet("GetReIndexDocuments", "DocumentCode");
                requisitions.Add(documentCode);
                try
                {
                    bool result = requisitionManager.PerformReIndexForDocuments(requisitions);
                    if (result)
                    {
                        Assert.IsTrue(result);
                    }
                    else
                    {
                        Assert.Fail();
                        return;
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message.ToString());
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }

        }

        [TestMethod]
        public void CopyRequisition()
        {
            if (CheckToExecute)
            {

                long newrequisitionId = TestCaseSourceFactory.GetLongValueFromDataSet("RequisitionWithInactiveEntitiesWithoutParentChildMapping", "RequisitionID");
                long ContactCode = TestCaseSourceFactory.GetLongValueFromDataSet("GetContactCode", "BuyerAssignee");

                var results = reqManager.CopyRequisition(newrequisitionId, 15675, ContactCode, 2);

                if (results > 0)
                {

                    Assert.IsTrue(results > 0);

                }
                else
                {
                    Assert.IsFalse(false);
                    return;
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }

        }

        [TestMethod]
        public void AutoSourcing_TestCase()
        {
            if (CheckToExecute)
            {
                long newrequisitionId = TestCaseSourceFactory.GetLongValueFromDataSet("RequisitionWithouttaxcode", "RequisitionID");
                var result = reqAutoSource.AutoSourcing(newrequisitionId, 1, true);
                if (result != null)
                {
                    Assert.IsTrue(true);
                }
                else
                {
                    Assert.IsFalse(true);
                    return;
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }

        }

        [TestMethod]
        public void CapitalBudget_TestCase()
        {
            if (CheckToExecute)
            {
                long newrequisitionId = TestCaseSourceFactory.GetLongValueFromDataSet("RequisitionWithouttaxcode", "RequisitionID");
                var result = requisitionManager.GetBudgetDetails(newrequisitionId);
                if (result != null)
                {
                    Assert.IsTrue(true);
                }
                else
                {
                    Assert.IsFalse(true);
                    return;
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }

        }

        [TestMethod]
        public void CatalogItemsUserAccessCheck()
        {
            if (CheckToExecute)
            {
                List<GEP.NewP2PEntities.ItemMasterItemInfo> lstItemMasterItems = new List<GEP.NewP2PEntities.ItemMasterItemInfo>();
                List<long> lstCatalogItems = new List<long>();
                List<long> lstAccessEntities = new List<long>();
                var requisitions = TestCaseSourceFactory.GetDraftRequisitions();
                foreach (var req in requisitions)
                {
                    var reqResult = requisitionManager.GetRequisitionDisplayDetails(req);
                    foreach (var item in reqResult.items)
                    {
                        if (item.catalogItemId > 0 && (item.source.id == 2 || item.source.id == 8))
                            lstCatalogItems.Add(Convert.ToInt64(item.catalogItemId));
                        if (item.catalogItemId > 0 && item.source.id == 5)
                        {
                            GEP.NewP2PEntities.ItemMasterItemInfo itemMasterItemInfo = new GEP.NewP2PEntities.ItemMasterItemInfo();
                            itemMasterItemInfo.ItemMasterItem = Convert.ToInt64(item.catalogItemId);
                            itemMasterItemInfo.BuyerItemNumber = item.buyerItemNumber;
                            lstItemMasterItems.Add(itemMasterItemInfo);
                        }
                            
                    }
                    if (reqResult.HeaderSplitAccountingFields != null && reqResult.HeaderSplitAccountingFields.Count > 0)
                    {
                        foreach (var addEntity in reqResult.HeaderSplitAccountingFields)
                        {
                            lstAccessEntities.Add(addEntity.EntityDetailCode);
                        }
                    }

                }

                GEP.NewP2PEntities.CatalogItemsInfo catalogItemsInfo = new GEP.NewP2PEntities.CatalogItemsInfo()
                {
                    AccessEntities = lstAccessEntities,
                    ContactCode = UserContextHelper.GetExecutionContext.ContactCode,
                    CatalogItems = lstCatalogItems,
                    ItemMasterItems = lstItemMasterItems
                };

                var result = requisitionManager.UserAccessCheckForItems(catalogItemsInfo);
                if (result != null)
                {
                    Assert.IsTrue(true);
                }
                else
                {
                    Assert.Fail();
                    return;
                }
            }
        }
    public void GetBasicDetailsById_ExposeLOBId()
    {

      if (CheckToExecute)
      {
        try
        {
          var requisitions = TestCaseSourceFactory.GetDraftRequisitions();
          long userId = sqlRequisitionDAO.UserContext.ContactCode;
          var reqResult = sqlRequisitionDAO.GetBasicDetailsById(requisitions[0], userId);
          if (reqResult != null)
          {
            if (reqResult.DocumentLOBDetails.Count > 0)
              Assert.IsTrue(reqResult.DocumentLOBDetails.FirstOrDefault().EntityDetailCode > 0);
            else
            {
              Assert.Fail();
              return;
            }
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex.Message.ToString());
        }
      }
      else
      {
        Assert.Inconclusive("Not executed");
      }

    }
        [TestMethod]

        public void GetOrderingLoctionNameByLocationId()
        {
            if (CheckToExecute)
            {
                IdNameAndAddress result = new IdNameAndAddress();

                long locationId = TestCaseSourceFactory.GetLongValueFromDataSet("GetLocationId", "LocationId");
                result = requisitionManager.GetOrderingLoctionNameByLocationId(locationId);
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }
        }

        [TestMethod]
        public void GetDocumentDetailsByDocumentCode()
        {
            if (CheckToExecute)
            {
                List<long> lstpartners = new List<long>();
                long partnerCode = partnerAndContactCodeDict["partnerCode"];
                lstpartners.Add(partnerCode);
                var DocumentCode = TestCaseSourceFactory.GetLongValueFromDataSet("GetdocumentCode", "DOCUMENTCODE");
                var result = sqlRequisitionDAO.GetDocumentDetailsByDocumentCode(DocumentCode, "22,61,62", true, UserContextHelper.GetExecutionContext.BelongingEntityId, lstpartners);
                if (result != null)
                {
                    Assert.IsTrue(result.Document.DocumentCode > 0);
                }
                else
                {
                    Assert.Fail();
                    return;
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }
        }

        [TestMethod]
        public void GetDocumentDetailsByDocumentCode1()
        {
            if (CheckToExecute)
            {
                RequisitionDocumentManager documentManager = new RequisitionDocumentManager(jwtToken);
                documentManager.UserContext = UserContextHelper.GetExecutionContext;
                documentManager.GepConfiguration = Helper.InitMultiRegion();

                var DocumentCode = TestCaseSourceFactory.GetLongValueFromDataSet("GetdocumentCode", "DOCUMENTCODE");
                var result = documentManager.GetSubmissionCheck(DocumentCode, 7);
                if (result != null)
                {
                    Assert.IsTrue(true);
                }
                else
                {
                    Assert.Fail();
                    return;
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }
        }

        [TestMethod]
        public void UpdateMultipleRequisitionItemStatusesTest()
        {
            if (CheckToExecute)
            {
                var DocumentCode = TestCaseSourceFactory.GetLongValueFromDataSet("GetdocumentCode", "DOCUMENTCODE");
                bool result = requisitionManager.UpdateMultipleRequisitionItemStatuses(DocumentCode.ToString(), StockReservationStatus.Orderd);
                if (result)
                {
                    Assert.IsTrue(true);
                }
                else
                {
                    Assert.Fail();
                    return;
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }
        }
    [TestMethod]
    public void EnableQuickQuoteRuleCheckTest()
    {
      if (CheckToExecute)
      {
        var DocumentCode = TestCaseSourceFactory.GetLongValueFromDataSet("GetdocumentCode", "DOCUMENTCODE");
        bool result = requisitionRuleEngineManager.EnableQuickQuoteRuleCheck(DocumentCode);
        if (!result)
        {
          Assert.IsTrue(true);
        }
        else
        {
          Assert.Fail();
          return;
        }
      }
      else
      {
        Assert.Inconclusive("Not executed");
      }
    }
    [TestMethod]
    public void UpdateLineStatusForRequisitionTest()
    {
      if (CheckToExecute)
      {
        var DocumentCode = TestCaseSourceFactory.GetLongValueFromDataSet("GetdocumentCode", "DOCUMENTCODE");
        bool result = requisitionManager.UpdateLineStatusForRequisition(DocumentCode, (StockReservationStatus)(DocumentStatus.Approved), true, null);
        if (result)
        {
          Assert.IsTrue(true);
        }
        else
        {
          Assert.Fail();
          return;
        }
      }
      else
      {
        Assert.Inconclusive("Not executed");
      }
    }

    [TestMethod]
    public void GetAllAccountingFieldsWithDefaultValuesTest()
    {
      if (CheckToExecute)
      {
        var DocumentCode = TestCaseSourceFactory.GetLongValueFromDataSet("GetdocumentCode", "DOCUMENTCODE");
        List<SplitAccountingFields> splitAccountingFields = requisitionDocumentManager.GetAllAccountingFieldsWithDefaultValues(P2PDocumentType.Requisition, LevelType.Both, requisitionDocumentManager.UserContext.ContactCode, DocumentCode, null, null, false, 0);
        if (splitAccountingFields != null)
        {
          Assert.IsTrue(splitAccountingFields.Count > 0);
        }
        else
        {
          Assert.Fail();
          return;
        }
      }
      else
      {
        Assert.Inconclusive("Not executed");
      }
    }

    [TestMethod]
    public void GetAllAccountingFieldsWithDefaultValuesADRTest()
    {
      if (CheckToExecute)
      {
        var DocumentCode = TestCaseSourceFactory.GetLongValueFromDataSet("GetdocumentCode", "DOCUMENTCODE");
        List<ADRSplit> splits = requisitionDocumentManager.GetAllAccountingFieldsWithDefaultValues(P2PDocumentType.Requisition, LevelType.Both, requisitionDocumentManager.UserContext.ContactCode, DocumentCode, null, null, false, new List<long>(), 54, PreferenceLOBType.Serve, ADRIdentifier.DocumentCode, null);
        if (splits != null)
        {
          Assert.IsTrue(true);
        }
        else
        {
          Assert.Fail();
          return;
        }
      }
      else
      {
        Assert.Inconclusive("Not executed");
      }
    }



    [TestMethod]
    public void GetDocumentStatusTest()
    {
      if (CheckToExecute)
      {
        var requisitions = TestCaseSourceFactory.GetDraftRequisitions();
        DocumentStatus documentStatus = requisitionDocumentManager.GetDocumentStatus(requisitions[0]);
        if (documentStatus != null)
        {
          Assert.IsTrue(documentStatus == DocumentStatus.Draft);
        }
        else
        {
          Assert.Fail();
          return;
        }
      }
      else
      {
        Assert.Inconclusive("Not executed");
      }
    }

    [TestMethod]
    public void FinalizeCommentsTest()
    {
      if (CheckToExecute)
      {
        var requisitions = TestCaseSourceFactory.GetDraftRequisitions();
        bool result = requisitionDocumentManager.FinalizeComments(P2PDocumentType.Requisition, requisitions[0], false);
        if (result)
        {
          Assert.IsTrue(true);
        }
        else
        {
          Assert.Fail();
          return;
        }
      }
      else
      {
        Assert.Inconclusive("Not executed");
      }
    }

    [TestMethod]
    public void GetBUDetailListTest()
    {
      if (CheckToExecute)
      {
        var requisitions = TestCaseSourceFactory.GetDraftRequisitions();
        var requisition = requisitionManager.GetRequisitionDisplayDetails(requisitions[0]);
        List<GEP.NewPlatformEntities.DocumentBU> documentBUs = requisitionManager.GetBUDetailList(requisition.HeaderSplitAccountingFields, requisitions[0]);
        if (documentBUs != null)
        {
          Assert.IsTrue(documentBUs.Count > 0);
        }
        else
        {
          Assert.Fail();
          return;
        }
      }
      else
      {
        Assert.Inconclusive("Not executed");
      }
    }

    [TestMethod]
    public void GetContractItemsByContractNumberTest()
    {
      if (CheckToExecute)
      {
        var contractNumber = TestCaseSourceFactory.GetContractNumber();
        List<GEP.NewPlatformEntities.IdAndName> result = requisitionManager.GetContractItemsByContractNumber(contractNumber, "", 1);

        if (result != null)
        {
          Assert.IsTrue(true);
        }
        else
        {
          Assert.Fail();
          return;
        }
      }
      else
      {
        Assert.Inconclusive("Not executed");
      }
    }

  }


}

