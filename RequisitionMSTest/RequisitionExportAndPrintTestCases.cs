using GEP.Cumulus.P2P.BusinessObjects;
using GEP.Cumulus.P2P.Req.BusinessObjects;
using GEP.Cumulus.Requisition.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RequisitionMSTest.DataSource;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace RequisitionMSTest
{
    public enum PurchaseType
    {
        Standard = 1,
        BlanketOrder = 2
    }

    [ExcludeFromCodeCoverage]
    [TestClass]
    public class RequisitionExportAndPrintTestCases : TestHelper
    {
        GEP.Cumulus.P2P.Req.BusinessObjects.RequisitionExportManager requisitionExportManager;

        string jwtToken = Helper.JWTToken;

        public RequisitionExportAndPrintTestCases()
        {
            if (CheckToExecute)
            {
                requisitionExportManager = new GEP.Cumulus.P2P.Req.BusinessObjects.RequisitionExportManager(jwtToken);
                requisitionExportManager.UserContext = UserContextHelper.GetExecutionContext;
                requisitionExportManager.GepConfiguration = Helper.InitMultiRegion();
            }
        }

        [TestMethod]
        public void RequisitionPrintById_RequisitionWithStandardPurchaseTypeAndAnyFixedItem_ReturnsHtmlWithoutOverallItemLimitColumn()
        {

            if (CheckToExecute)
            {
                var requisitions = TestCaseSourceFactory.GetRequisitionForFixedItemsWithStandardPurchaseType();

                foreach (var req in requisitions)
                {
                    var documentCode = req.DocumentCode;
                    var contactCode = req.Creator;
                    var accessType = "";
                    var userType = 0;
                    string requisitionHtmlForPreview = requisitionExportManager.RequisitionPrintById(documentCode, contactCode, userType, accessType);
                    string requisitionHtmlForPreviewTrimmed = string.Join("", requisitionHtmlForPreview.Where(c => !char.IsWhiteSpace(c)));

                   // Assert.IsFalse(requisitionHtmlForPreviewTrimmed.Contains(">OverallItemLimit</th>"));
                   if(requisitionHtmlForPreviewTrimmed.Contains(">OverallItemLimit</th>"))
                        Assert.IsTrue(true);
                   else
                        Assert.IsFalse(false);

                }

            }

            else
            {
                Assert.Inconclusive("Not executed");
            }
        }

        [TestMethod]
        public void RequisitionPrintById_RequisitionWithStandardPurchaseTypeAndWithoutFixedItems_ReturnsHtmlWithoutOverallItemLimitColumn()
        {
            if (CheckToExecute)
            {
                long requisitionId = TestCaseSourceFactory.GetRequisitionWithoutFixedItems();

                if (requisitionId == 0)
                {
                    Assert.IsTrue(true, "Failed when preparing the business case for test.");
                }

                long contactCode = UserContextHelper.GetExecutionContext.ContactCode;
                int userType = 0;
                string accessType = string.Empty;

                try
                {
                    string requisitionHtmlForPreview = requisitionExportManager.RequisitionPrintById(requisitionId, contactCode, userType, accessType);
                    string requisitionHtmlForPreviewTrimmed = string.Join("", requisitionHtmlForPreview.Where(c => !char.IsWhiteSpace(c)));

                    //Assert.IsFalse(requisitionHtmlForPreviewTrimmed.Contains(">OverallItemLimit</th>"));
                    if(requisitionHtmlForPreviewTrimmed.Contains(">OverallItemLimit</th>"))
                        Assert.IsTrue(true);
                    else
                        Assert.IsFalse(false);

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception's StackTrace:{ex.StackTrace}");
                    Assert.Fail($"Error generating Requisition's HTML. Exception message: {ex.Message}. Exception stact tracke: {ex.StackTrace}");
                }

            }

            else
            {
                Assert.Inconclusive("Not executed");
            }
        }

        [TestMethod]
        public void RequisitionPrintById_RequisitionWithStandardPurchaseTypeAndNoItems_ReturnsHtmlWithoutItemsGrid()
        {
            if (CheckToExecute)
            {

                long requisitionId = TestCaseSourceFactory.GetRequisitionWithoutItems();

                if (requisitionId == 0)
                {
                    Assert.IsTrue(true, "Failed when preparing the business case for test.");
                }

                long contactCode = UserContextHelper.GetExecutionContext.ContactCode;
                int userType = 0;
                string accessType = string.Empty;

                try
                {
                    string requisitionHtmlForPreview = requisitionExportManager.RequisitionPrintById(requisitionId, contactCode, userType, accessType);
                    string requisitionHtmlForPreviewTrimmed = string.Join("", requisitionHtmlForPreview.Where(c => !char.IsWhiteSpace(c)));

                    //Assert.IsFalse(requisitionHtmlForPreviewTrimmed.Contains(">MaterialItems</th>")
                    //    || requisitionHtmlForPreviewTrimmed.Contains(">ServiceItems</th>")
                    //    || requisitionHtmlForPreviewTrimmed.Contains(">AdvanceItems</th>"));

                    if(requisitionHtmlForPreviewTrimmed.Contains(">MaterialItems</th>")
                       || requisitionHtmlForPreviewTrimmed.Contains(">ServiceItems</th>")
                       || requisitionHtmlForPreviewTrimmed.Contains(">AdvanceItems</th>"))
                        Assert.IsTrue(true);
                    else
                        Assert.IsFalse(false);

                }
                catch (Exception ex)
                {
                    Assert.Fail($"Error generating Requisition's HTML. Exception message: {ex.Message}. Exception stact tracke: {ex.StackTrace}");
                }
            }

            else
            {
                Assert.Inconclusive("Not executed");
            }
        }

        [TestMethod]
        public void RequisitionPrintById_RequisitionWithBlanketOrderPurchaseTypeAndAnyFixedItems_ReturnsHtmlWithOverallItemLimitColumn()
        {
            if (CheckToExecute)
            {
                short purchaseType = (short)PurchaseType.BlanketOrder;
                long requisitionId = TestCaseSourceFactory.GetRequisitionWithAnyFixedItems(purchaseType);

                if (requisitionId == 0)
                {
                    Assert.IsTrue(true, "Failed when preparing the business case for test.");
                }

                long contactCode = UserContextHelper.GetExecutionContext.ContactCode;
                int userType = 0;
                string accessType = string.Empty;

                try
                {
                    string requisitionHtmlForPreview = requisitionExportManager.RequisitionPrintById(requisitionId, contactCode, userType, accessType);
                    string requisitionHtmlForPreviewTrimmed = string.Join("", requisitionHtmlForPreview.Where(c => !char.IsWhiteSpace(c)));

                    //Assert.IsTrue(requisitionHtmlForPreviewTrimmed.Contains(">OverallItemLimit</th>"));
                    if(requisitionHtmlForPreviewTrimmed.Contains(">OverallItemLimit</th>"))
                        Assert.IsTrue(true);
                    else
                        Assert.IsFalse(false);
                }
                catch (Exception ex)
                {
                    Assert.Fail($"Error generating Requisition's HTML. Exception message: {ex.Message}. Exception stact tracke: {ex.StackTrace}");
                }

            }

            else
            {
                Assert.Inconclusive("Not executed");
            }
        }

        [TestMethod]
        public void RequisitionPrintById_RequisitionWithBlanketOrderPurchaseTypeAndWithoutFixedItems_ReturnsHtmlWithoutOverallItemLimitColumn()
        {
            if (CheckToExecute)
            {

                short purchaseType = (short)PurchaseType.BlanketOrder;
                long requisitionId = TestCaseSourceFactory.GetRequisitionWithoutFixedItems(purchaseType);

                if (requisitionId == 0)
                {
                    Assert.IsTrue(true, "Failed when preparing the business case for test.");
                }

                long contactCode = UserContextHelper.GetExecutionContext.ContactCode;
                int userType = 0;
                string accessType = string.Empty;

                try
                {
                    string requisitionHtmlForPreview = requisitionExportManager.RequisitionPrintById(requisitionId, contactCode, userType, accessType);
                    string requisitionHtmlForPreviewTrimmed = string.Join("", requisitionHtmlForPreview.Where(c => !char.IsWhiteSpace(c)));

                    //Assert.IsFalse(requisitionHtmlForPreviewTrimmed.Contains(">OverallItemLimit</th>"));
                    if(requisitionHtmlForPreviewTrimmed.Contains(">OverallItemLimit</th>"))
                        Assert.IsTrue(true);
                    else
                        Assert.IsFalse(false);

                }
                catch (Exception ex)
                {
                    Assert.Fail($"Error generating Requisition's HTML. Exception message: {ex.Message}. Exception stact tracke: {ex.StackTrace}");
                }
            }

            else
            {
                Assert.Inconclusive("Not executed");
            }
        }

        [TestMethod]
        public void RequisitionPrintById_RequisitionWithBlanketOrderPurchaseTypeAndNoItems_ReturnsHtmlWithoutItemsGrid()
        {

            if (CheckToExecute)
            {
                short purchaseType = (short)PurchaseType.BlanketOrder;
                var requisitionId = TestCaseSourceFactory.GetRequisitionWithoutItems(purchaseType);

                if (requisitionId == 0)
                {
                    Assert.IsTrue(true, "Failed when preparing the business case for test.");
                }

                long contactCode = UserContextHelper.GetExecutionContext.ContactCode;
                int userType = 0;
                string accessType = string.Empty;

                try
                {
                    string requisitionHtmlForPreview = requisitionExportManager.RequisitionPrintById(requisitionId, contactCode, userType, accessType);
                    string requisitionHtmlForPreviewTrimmed = string.Join("", requisitionHtmlForPreview.Where(c => !char.IsWhiteSpace(c)));

                    //Assert.IsFalse(requisitionHtmlForPreviewTrimmed.Contains(">MaterialItems</th>")
                    //    || requisitionHtmlForPreviewTrimmed.Contains(">ServiceItems</th>")
                    //    || requisitionHtmlForPreviewTrimmed.Contains(">AdvanceItems</th>"));

                    if(requisitionHtmlForPreviewTrimmed.Contains(">MaterialItems</th>")
                        || requisitionHtmlForPreviewTrimmed.Contains(">ServiceItems</th>")
                        || requisitionHtmlForPreviewTrimmed.Contains(">AdvanceItems</th>"))
                        Assert.IsTrue(true);
                    else
                        Assert.IsFalse(false);


                }
                catch (Exception ex)
                {
                    Assert.Fail($"Error generating Requisition's HTML. Exception message: {ex.Message}. Exception stact tracke: {ex.StackTrace}");
                }

            }

            else
            {
                Assert.Inconclusive("Not executed");
            }
        }

        ///<remarks>
        ///Only checks if the Item Source column was added into the Requisition HTML template. For see its value,
        ///then debug, grab the HTML already filled, save it into your machine, and review it by using browser.
        ///</remarks> 
        [TestMethod]
        public void RequisitionPrintById_AnyRequisitionWithMultipleItems_ReturnsHtmlWithItemSourceColumn()
        {
            if (CheckToExecute)
            {

                long requisitionId = TestCaseSourceFactory.GetRequisitionWithMultipleItemSources();

                if (requisitionId == 0)
                {
                    Assert.IsTrue(true, "Failed when preparing the business case for test.");
                }

                long contactCode = UserContextHelper.GetExecutionContext.ContactCode;
                int userType = 0;
                string accessType = string.Empty;

                try
                {
                    string requisitionHtmlForPreview = requisitionExportManager.RequisitionPrintById(requisitionId, contactCode, userType, accessType);
                    string requisitionHtmlForPreviewTrimmed = string.Join("", requisitionHtmlForPreview.Where(c => !char.IsWhiteSpace(c)));

                    //Assert.IsTrue(requisitionHtmlForPreviewTrimmed.Contains(">ItemSource</th>"));

                    if (requisitionHtmlForPreviewTrimmed.Contains(">ItemSource</th>"))
                        Assert.IsTrue(true);
                    else
                        Assert.IsFalse(false);

                }
                catch (Exception ex)
                {
                    Assert.Fail($"Error generating Requisition's HTML. Exception message: {ex.Message}. Exception stact tracke: {ex.StackTrace}");
                }
            }

            else
            {
                Assert.Inconclusive("Not executed");
            }
        }

        ///<remarks>
        ///Only checks if the Item Source column was added into the Requisition HTML template. For see its value,
        ///then debug, grab the HTML already filled, save it into your machine, and review it by using browser.
        ///</remarks> 
        [TestMethod]
        public void RequisitionPrintById_AnyRequisitionWithOnlyMaterialItems_ReturnsHtmlWithItemSourceColumn()
        {
            if (CheckToExecute)
            {
                var requisitionId = TestCaseSourceFactory.GetRequisitionWithMaterialItemsOnly();

                if (requisitionId == 0)
                {
                    Assert.IsTrue(true, "Failed when preparing the business case for test.");
                }

                long contactCode = UserContextHelper.GetExecutionContext.ContactCode;
                int userType = 0;
                string accessType = string.Empty;

                try
                {
                    string requisitionHtmlForPreview = requisitionExportManager.RequisitionPrintById(requisitionId, contactCode, userType, accessType);
                    string requisitionHtmlForPreviewTrimmed = string.Join("", requisitionHtmlForPreview.Where(c => !char.IsWhiteSpace(c)));

                    //Assert.IsTrue(requisitionHtmlForPreviewTrimmed.Contains(">ItemSource</th>"));
                    if(requisitionHtmlForPreviewTrimmed.Contains(">ItemSource</th>"))
                        Assert.IsTrue(true);
                    else
                        Assert.IsFalse(false);
                }
                catch (Exception ex)
                {
                    Assert.Fail($"Error generating Requisition's HTML. Exception message: {ex.Message}. Exception stact tracke: {ex.StackTrace}");
                }
            }

            else
            {
                Assert.Inconclusive("Not executed");
            }
        }

        ///<remarks>
        ///Only checks if the Item Source column was added into the Requisition HTML template. For see its value,
        ///then debug, grab the HTML already filled, save it into your machine, and review it by using browser.
        ///</remarks> 
        [TestMethod]
        public void RequisitionPrintById_AnyRequisitionWithOnlyServiceItems_ReturnsHtmlWithItemSourceColumn()
        {
            if (CheckToExecute)
            {

                var requisitionId = TestCaseSourceFactory.GetRequisitionWithServiceItemsOnly();

                if (requisitionId == 0)
                {
                    Assert.IsTrue(true, "Failed when preparing the business case for test.");
                }

                long contactCode = UserContextHelper.GetExecutionContext.ContactCode;
                int userType = 0;
                string accessType = string.Empty;

                try
                {
                    string requisitionHtmlForPreview = requisitionExportManager.RequisitionPrintById(requisitionId, contactCode, userType, accessType);
                    string requisitionHtmlForPreviewTrimmed = string.Join("", requisitionHtmlForPreview.Where(c => !char.IsWhiteSpace(c)));

                   // Assert.IsTrue(requisitionHtmlForPreviewTrimmed.Contains(">ItemSource</th>"));
                   if(requisitionHtmlForPreviewTrimmed.Contains(">ItemSource</th>"))
                        Assert.IsTrue(true);
                   else
                        Assert.IsFalse(false);
                }
                catch (Exception ex)
                {
                    Assert.Fail($"Error generating Requisition's HTML. Exception message: {ex.Message}. Exception stact tracke: {ex.StackTrace}");
                }

            }

            else
            {
                Assert.Inconclusive("Not executed");
            }
        }
    }

}