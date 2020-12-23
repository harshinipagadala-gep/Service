using GEP.Cumulus.P2P.BusinessObjects.Usability;
using GEP.Cumulus.P2P.Req.BusinessObjects;
using GEP.Cumulus.Requisition.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RequisitionMSTest.DataSource;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace RequisitionMSTest
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class RequisitionItemSourceTestCases : TestHelper
    {
        NewRequisitionManager requisitionManager;
        List<ItemSourceLocalizationKey> itemSources;

        string jwtToken = Helper.JWTToken;

        public RequisitionItemSourceTestCases()
        {
            if (CheckToExecute)
            {
                requisitionManager = new NewRequisitionManager(jwtToken);
                requisitionManager.UserContext = UserContextHelper.GetExecutionContext;
                requisitionManager.GepConfiguration = Helper.InitMultiRegion();

                itemSources = FillItemSourcesList();
            }
        }

        [TestMethod]
        public void GetRequisitionDisplayDetails_RequisitionWithMultipleItemsAndMultipleItemSource_ReturnsRequisitionWithItemsAndItsSource()
        {
            if (CheckToExecute)
            {
                long requisitionId = TestCaseSourceFactory.GetRequisitionWithMultipleItemSources();

                var requisition = requisitionManager.GetRequisitionDisplayDetails(requisitionId);
                var requisitionItemsSources = requisition.items.
                        GroupBy(ri => new { ri.source.id, ri.source.name }).
                        Select(ris => new ItemSourceLocalizationKey { SourceId = (int)ris.Key.id, LocalizationKey = ris.Key.name }).ToList();
                var itemSourcesVerification = VerifyItemSources(requisitionItemsSources, itemSources);

                if (!itemSourcesVerification.IsValid)
                {
                    var invalidItemSources = string.Join(", ", itemSourcesVerification.ItemSources.Select(i => i.ToString()));

                    Assert.Fail($"Invalid item sources: {invalidItemSources}");
                }

                Assert.IsNull(itemSourcesVerification.ItemSources);

            }

            else
            {
                Assert.Inconclusive("Not executed");
            }
        }

        [TestMethod]
        public void GetRequisitionDisplayDetails_RequisitionWithNoItems_ReturnsRequisitionWithoutItems()
        {
            if (CheckToExecute)
            {
                long requisitionId = TestCaseSourceFactory.GetRequisitionWithoutItems();

                var requisition = requisitionManager.GetRequisitionDisplayDetails(requisitionId);

                Assert.IsNull(requisition.items);
            }

            else
            {
                Assert.Inconclusive("Not executed");
            }
        }

        [TestMethod]
        public void GetRequisitionDisplayDetails_RequisitionWithOnlyNonCatalogItems_ReturnsRequisitionWithNonCatalogItemsAndItsCorrespondingSource()
        {
            if (CheckToExecute)
            {
                long requisitionId = TestCaseSourceFactory.GetRequisitionWithManualItemsOnly();
                var nonCatalogItemSources = itemSources.Where(i => i.SourceId == 1).ToList();//assuming that Manual items are non catalog

                var requisition = requisitionManager.GetRequisitionDisplayDetails(requisitionId);
                var requisitionItemsSources = requisition.items.
                        GroupBy(ri => new { ri.source.id, ri.source.name }).
                        Select(ris => new ItemSourceLocalizationKey { SourceId = (int)ris.Key.id, LocalizationKey = ris.Key.name }).ToList();
                var itemSourcesVerification = VerifyItemSources(requisitionItemsSources, nonCatalogItemSources);

                if (!itemSourcesVerification.IsValid)
                {
                    var invalidItemSources = string.Join(", ", itemSourcesVerification.ItemSources.Select(i => i.ToString()));

                    Assert.Fail($"Invalid item sources: {invalidItemSources}");
                }

                Assert.IsNull(itemSourcesVerification.ItemSources);
            }

            else
            {
                Assert.Inconclusive("Not executed");
            }
        }

        [TestMethod]
        public void GetRequisitionDisplayDetails_RequisitionWithCatalogItems_ReturnsRequisitionWithCatalogItemsAndItsCorrespondingSource()
        {
            if (CheckToExecute)
            {
                long requisitionId = TestCaseSourceFactory.GetRequisitionWithoutManualItems();
                var catalogItemSources = itemSources.Where(i => i.SourceId != 0 && i.SourceId != 1).ToList();

                var requisition = requisitionManager.GetRequisitionDisplayDetails(requisitionId);
                var requisitionItemsSources = requisition.items.
                        GroupBy(ri => new { ri.source.id, ri.source.name }).
                        Select(ris => new ItemSourceLocalizationKey { SourceId = (int)ris.Key.id, LocalizationKey = ris.Key.name }).ToList();
                var itemSourcesVerification = VerifyItemSources(requisitionItemsSources, catalogItemSources);

                if (!itemSourcesVerification.IsValid)
                {
                    var invalidItemSources = string.Join(", ", itemSourcesVerification.ItemSources.Select(i => i.ToString()));

                    Assert.Fail($"Invalid item sources: {invalidItemSources}");
                }

                Assert.IsNull(itemSourcesVerification.ItemSources);
            }

            else
            {
                Assert.Inconclusive("Not executed");
            }
        }

        /// <summary>
        /// Model class for hold SourceID (Item Source) and its Localization key.
        /// </summary>
        public class ItemSourceLocalizationKey
        {
            public int SourceId;
            public string LocalizationKey;

            public override string ToString()
            {
                return $"(SourceID: {SourceId}, LocalizationKey: {LocalizationKey})";
            }
        }

        /// <summary>
        /// Wraps the VerifyItemSources method response.
        /// </summary>
        public class ItemSourceVerification
        {
            public bool IsValid;
            public List<ItemSourceLocalizationKey> ItemSources;
        }

        /// <summary>
        /// Verify that the item source binding it's doing properly.
        /// </summary>
        /// <remarks>
        /// It extracts all the item sources that appears on the <paramref name="testCaseItemSources"/> list 
        /// but not in the <paramref name="mockedItemSources"/>. So in case there's an 
        /// item source missing, returns an object that contains a flag for response status, and a list that contains missing item sources.
        /// </remarks>
        /// <param name="testCaseItemSources">list of item sources (from test case)</param>
        /// <param name="mockedItemSources">list of item sources (mocked)</param>
        /// <returns>Returns an ItemSourceVerification instance that contains flag for response status, and a list of missing item sources.</returns>
        public ItemSourceVerification VerifyItemSources(List<ItemSourceLocalizationKey> testCaseItemSources, List<ItemSourceLocalizationKey> mockedItemSources)
        {
           
                //Workaround for EXCEPT set operator
                var dismatchingItemSources = testCaseItemSources.Where(tis => !mockedItemSources.Exists(mis => mis.SourceId == tis.SourceId && mis.LocalizationKey == tis.LocalizationKey)).ToList();

                return dismatchingItemSources.Count == 0 ?
                    new ItemSourceVerification() { IsValid = true, ItemSources = null } : new ItemSourceVerification() { IsValid = false, ItemSources = dismatchingItemSources };

           
        }

        /// <summary>
        /// Mocks the Item Sources for GetRequisitionDisplayDetails test cases.
        /// </summary>
        /// <returns>Returns a list of all Item Sources.</returns>
        public List<ItemSourceLocalizationKey> FillItemSourcesList()
        {
            #region Initialize SourceType List with static data
            return new[]
            {
                    new ItemSourceLocalizationKey() { SourceId = 0, LocalizationKey = "P2P_REQ_Other"},
                    new ItemSourceLocalizationKey() { SourceId = 1, LocalizationKey = "P2P_REQ_Manual"},
                    new ItemSourceLocalizationKey() { SourceId = 2, LocalizationKey = "P2P_REQ_Hosted"},
                    new ItemSourceLocalizationKey() { SourceId = 3, LocalizationKey = "P2P_REQ_Punchout"},
                    new ItemSourceLocalizationKey() { SourceId = 4, LocalizationKey = "P2P_REQ_Template"},
                    new ItemSourceLocalizationKey() { SourceId = 5, LocalizationKey = "P2P_REQ_Internal"},
                    new ItemSourceLocalizationKey() { SourceId = 6, LocalizationKey = "P2P_REQ_PurchaseRequest"},
                    new ItemSourceLocalizationKey() { SourceId = 8, LocalizationKey = "P2P_REQ_HostedAndInternal"}
                }.ToList();
            #endregion
        }
    }

}
