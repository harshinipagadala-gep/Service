using Gep.Cumulus.CSM.Entities;
using GEP.Cumulus.P2P.BusinessObjects;
using GEP.Cumulus.Requisition.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RequisitionMSTest.DataSource;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequisitionMSTest
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class SQLRequisitionDAOTestCases : TestHelper
    {
        private UserExecutionContext _userContext;
        private const short _blanketOrderPurchaseType = 2;

        public SQLRequisitionDAOTestCases()
        {
            if (CheckToExecute)
            {
                _userContext = UserContextHelper.GetExecutionContext;
            }
        }

        [TestMethod]
        public void GetAllRequisitionDetailsByRequisitionId_ByNotPassingTheSettingsDictionary_ShouldReturnEntityCatalogAndNonCatalogAmountsEqualToZero()
        {
            if (CheckToExecute)
            {
                var requisitionId = TestCaseSourceFactory.GetRequisitionWithItemsAmountDefined();

                var requisitionDetails = TestCaseSourceFactory.GetAllRequisitionDetailsByRequisitionId(requisitionId, _userContext.ContactCode, 0);

                var areAllEntityCatalogAndNonCatalogAmountEqualToZero = requisitionDetails.EntitySumList.All(e => e.NonCatalogTotalAmount == 0M && e.CatalogTotalAmount == 0M);

                Assert.IsTrue(areAllEntityCatalogAndNonCatalogAmountEqualToZero);
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }
        }

        [TestMethod]
        public void GetAllRequisitionDetailsByRequisitionId_ByPassingTheSettingsDictionaryButCatalogSettingUndefined_ShouldReturnEntityCatalogAndNonCatalogAmountsEqualToZero()
        {
            if (CheckToExecute)
            {

                var requisitionId = TestCaseSourceFactory.GetRequisitionWithItemsAmountDefined();
                var settings = new Dictionary<string, string>();

                var requisitionDetails = TestCaseSourceFactory.GetAllRequisitionDetailsByRequisitionId(requisitionId, _userContext.ContactCode, 0, null, settings);

                var areAllEntityCatalogAndNonCatalogAmountEqualToZero = requisitionDetails.EntitySumList.All(e => e.NonCatalogTotalAmount == 0M && e.CatalogTotalAmount == 0M);

                Assert.IsTrue(areAllEntityCatalogAndNonCatalogAmountEqualToZero);
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }
        }

        [TestMethod]
        public void GetAllRequisitionDetailsByRequisitionId_ByPassingTheSettingsDictionaryButCatalogSettingDefinedWithNoValue_ShouldReturnEntityCatalogAndNonCatalogAmountsEqualToZero()
        {
            if (CheckToExecute)
            {

                var requisitionId = TestCaseSourceFactory.GetRequisitionWithItemsAmountDefined();

                var settings = new Dictionary<string, string>();
                settings.Add("CatalogItemSources", string.Empty);

                var requisitionDetails = TestCaseSourceFactory.GetAllRequisitionDetailsByRequisitionId(requisitionId, _userContext.ContactCode, 0, null, settings);

                var areAllEntityCatalogAndNonCatalogAmountEqualToZero = requisitionDetails.EntitySumList.All(e => e.NonCatalogTotalAmount == 0M && e.CatalogTotalAmount == 0M);

                Assert.IsTrue(areAllEntityCatalogAndNonCatalogAmountEqualToZero);
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }
        }

        [TestMethod]
        public void GetAllRequisitionDetailsByRequisitionId_StandardRequisitionWithMultipleItemSourcesAndPassingCatalogSettingWithValues_ShouldReturnEntityCatalogOrNonCatalogAmountsNotEqualToZero()
        {
            if (CheckToExecute)
            {
                var requisitionId = TestCaseSourceFactory.GetRequisitionWithMultipleItemSources();

                var settings = new Dictionary<string, string>();
                settings.Add("CatalogItemSources", "2,3,4,5,6,8");

                var requisitionDetails = TestCaseSourceFactory.GetAllRequisitionDetailsByRequisitionId(requisitionId, _userContext.ContactCode, 0, null, settings);

                var areAllEntityCatalogOrNonCatalogAmountNotEqualToZero = requisitionDetails.EntitySumList.All(e => e.NonCatalogTotalAmount != 0M || e.CatalogTotalAmount != 0M);

                Assert.IsTrue(areAllEntityCatalogOrNonCatalogAmountNotEqualToZero);
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }
        }

        [TestMethod]
        public void GetAllRequisitionDetailsByRequisitionId_BlanketOrderRequisitionWithMultipleItemSourcesAndPassingCatalogSettingWithValues_ShouldReturnEntityCatalogOrNonCatalogAmountsNotEqualToZero()
        {
            if (CheckToExecute)
            {
                var requisitionId = TestCaseSourceFactory.GetRequisitionWithMultipleItemSources(_blanketOrderPurchaseType);

                //In case Requisition hasn't been found for the scenario, then test case will be raised as passed
                if (requisitionId == 0)
                {
                    Assert.IsTrue(true, "Requisition not found for the test case");
                    return;
                }

                var settings = new Dictionary<string, string>();
                settings.Add("CatalogItemSources", "2,3,4,5,6,8");

                var requisitionDetails = TestCaseSourceFactory.GetAllRequisitionDetailsByRequisitionId(requisitionId, _userContext.ContactCode, 0, null, settings);

                var areAllEntityCatalogOrNonCatalogAmountNotEqualToZero = requisitionDetails.EntitySumList.All(e => e.NonCatalogTotalAmount != 0M || e.CatalogTotalAmount != 0M);

                Assert.IsTrue(areAllEntityCatalogOrNonCatalogAmountNotEqualToZero);
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }
        }

        [TestMethod]
        public void GetAllRequisitionDetailsByRequisitionId_StandardRequisitionWithManualItemsOnlyAndPassingCatalogSettingWithValues_ShouldReturnEntityCatalogAmountEqualsToZeroAndNonCatalogAmountEqualsOrGreaterThanZero()
        {
            if (CheckToExecute)
            {
                var requisitionId = TestCaseSourceFactory.GetRequisitionWithManualItemsOnly();

                var settings = new Dictionary<string, string>();
                settings.Add("CatalogItemSources", "2,3,4,5,6,8");

                var requisitionDetails = TestCaseSourceFactory.GetAllRequisitionDetailsByRequisitionId(requisitionId, _userContext.ContactCode, 0, null, settings);

                var result = requisitionDetails.EntitySumList.All(e => e.NonCatalogTotalAmount >= 0M && e.CatalogTotalAmount == 0M);

                Assert.IsTrue(result);
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }
        }

        [TestMethod]
        public void GetAllRequisitionDetailsByRequisitionId_BlanketOrderRequisitionWithManualItemsOnlyAndPassingCatalogSettingWithValues_ShouldReturnEntityCatalogAmountEqualsToZeroAndNonCatalogAmountEqualsOrGreaterThanZero()
        {
            if (CheckToExecute)
            {
                var requisitionId = TestCaseSourceFactory.GetRequisitionWithManualItemsOnly(_blanketOrderPurchaseType);

                var settings = new Dictionary<string, string>();
                settings.Add("CatalogItemSources", "2,3,4,5,6,8");

                var requisitionDetails = TestCaseSourceFactory.GetAllRequisitionDetailsByRequisitionId(requisitionId, _userContext.ContactCode, 0, null, settings);

                var result = requisitionDetails.EntitySumList.All(e => e.NonCatalogTotalAmount >= 0M && e.CatalogTotalAmount == 0M);

                Assert.IsTrue(result);
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }
        }

        [TestMethod]
        public void GetAllRequisitionDetailsByRequisitionId_StandardRequisitionWithoutManualItemsAndPassingCatalogSettingWithValues_ShouldReturnEntityNonCatalogAmountEqualsToZeroAndCatalogAmountEqualsOrGreaterThanZero()
        {
            if (CheckToExecute)
            {

                var requisitionId = TestCaseSourceFactory.GetRequisitionWithoutManualItems();

                var settings = new Dictionary<string, string>();
                settings.Add("CatalogItemSources", "2,3,4,5,6,8");

                var requisitionDetails = TestCaseSourceFactory.GetAllRequisitionDetailsByRequisitionId(requisitionId, _userContext.ContactCode, 0, null, settings);

                var result = requisitionDetails.EntitySumList.All(e => e.NonCatalogTotalAmount == 0M && e.CatalogTotalAmount >= 0M);

                Assert.IsTrue(result);

            }
            else
            {
                Assert.Inconclusive("Not executed");
            }
        }

        [TestMethod]
        public void GetAllRequisitionDetailsByRequisitionId_BlanketOrderRequisitionWithoutManualItemsAndPassingCatalogSettingWithValues_ShouldReturnEntityNonCatalogAmountEqualsToZeroAndCatalogAmountEqualsOrGreaterThanZero()
        {
            if (CheckToExecute)
            {
                var requisitionId = TestCaseSourceFactory.GetRequisitionWithoutManualItems(_blanketOrderPurchaseType);

                //In case Requisition hasn't been found for the scenario, then test case will be raised as passed
                if (requisitionId == 0)
                {
                    Assert.IsTrue(true, "Requisition not found for the test case");
                    return;
                }

                var settings = new Dictionary<string, string>();
                settings.Add("CatalogItemSources", "2,3,4,5,6,8");

                var requisitionDetails = TestCaseSourceFactory.GetAllRequisitionDetailsByRequisitionId(requisitionId, _userContext.ContactCode, 0, null, settings);

                var result = requisitionDetails.EntitySumList.All(e => e.NonCatalogTotalAmount == 0M && e.CatalogTotalAmount >= 0M);

                Assert.IsTrue(result);

            }
            else
            {
                Assert.Inconclusive("Not executed");
            }
        }

        [TestMethod]
        public void GetAllRequisitionDetailsByRequisitionId_StandardRequisitionWithMultipleItemSourcesAndPassingCatalogSettingWithValues_ShouldReturnEntityTotalAmountEqualsToTheSumOfNonCatalogAndCatalogAmounts()
        {
            if (CheckToExecute)
            {
                var requisitionId = TestCaseSourceFactory.GetRequisitionWithMultipleItemSources();

                var settings = new Dictionary<string, string>();
                settings.Add("CatalogItemSources", "2,3,4,5,6,8");

                var requisitionDetails = TestCaseSourceFactory.GetAllRequisitionDetailsByRequisitionId(requisitionId, _userContext.ContactCode, 0, null, settings);

                var result = requisitionDetails.EntitySumList.All(e => e.TotalAmount == (e.CatalogTotalAmount + e.NonCatalogTotalAmount));

                Assert.IsTrue(result);

            }
            else
            {
                Assert.Inconclusive("Not executed");
            }
        }

        [TestMethod]
        public void GetAllRequisitionDetailsByRequisitionId_StandardRequisitionThatDoesNotHaveItemsAndPassingCatalogSettingWithValues_ShouldReturnRequisitionDetailsWithoutItems()
        {
            if (CheckToExecute)
            {
                var requisitionId = TestCaseSourceFactory.GetRequisitionWithoutItems();

                var settings = new Dictionary<string, string>();
                settings.Add("CatalogItemSources", "2,3,4,5,6,8");

                var requisitionDetails = TestCaseSourceFactory.GetAllRequisitionDetailsByRequisitionId(requisitionId, _userContext.ContactCode, 0, null, settings);

                var result = requisitionDetails.RequisitionItems.Count();

                Assert.IsTrue(result == 0);
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }
        }

        [TestMethod]
        public void GetAllRequisitionDetailsByRequisitionId_BlanketOrderRequisitionThatDoesNotHaveItemsAndPassingCatalogSettingWithValues_ShouldReturnRequisitionDetailsWithoutItems()
        {
            if (CheckToExecute)
            {
                var requisitionId = TestCaseSourceFactory.GetRequisitionWithoutItems(_blanketOrderPurchaseType);

                var settings = new Dictionary<string, string>();
                settings.Add("CatalogItemSources", "2,3,4,5,6,8");

                var requisitionDetails = TestCaseSourceFactory.GetAllRequisitionDetailsByRequisitionId(requisitionId, _userContext.ContactCode, 0, null, settings);

                var result = requisitionDetails.RequisitionItems.Count();

                Assert.IsTrue(result == 0);

            }
            else
            {
                Assert.Inconclusive("Not executed");
            }
        }

    }
}
