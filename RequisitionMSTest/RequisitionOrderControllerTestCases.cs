using GEP.Cumulus.Documents.Entities;
using GEP.Cumulus.P2P.BusinessObjects;
using GEP.Cumulus.Requisition.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RequisitionMSTest.DataSource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequisitionMSTest
{
    [TestClass]
    public class RequisitionOrderControllerTestCases : TestHelper
    {
        RequisitionManager requisitionManager;
        public RequisitionOrderControllerTestCases()
        {
            if (CheckToExecute)
            {
                requisitionManager = new RequisitionManager(string.Empty);
                requisitionManager.UserContext = UserContextHelper.GetExecutionContext;
                requisitionManager.GepConfiguration = Helper.InitMultiRegion();


            }
        }


        [TestMethod]

        public void GetAllRequisitionDetailsByRequisitionId()
        {
            if (CheckToExecute)
            {
                long documentCode = TestCaseSourceFactory.GetLongValueFromDataSet("GetApproveddocumentCode", "DocumentCode");
                var result = requisitionManager.GetAllRequisitionDetailsByRequisitionId(documentCode, requisitionManager.UserContext.ContactCode, 0);
                if (result != null)
                {

                    Assert.IsTrue(true);

                }
                else
                {
                    Assert.Fail();
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }

        }
        [TestMethod]
        public void GetAllDocumentAdditionalEntityInfo()
        {
            if (CheckToExecute)
            {
                long documentCode = TestCaseSourceFactory.GetLongValueFromDataSet("GetApproveddocumentCode", "DocumentCode");
                var result = requisitionManager.GetAllDocumentAdditionalEntityInfo(documentCode);
                if (result != null)
                {
                    Assert.IsTrue(result.Count > 0);
                }
                else
                {
                    Assert.Fail();
                }
            }
            else
            {
                Assert.Inconclusive("Not executed");
            }

        }

        [TestMethod]

        public void GetRequisitionDetailsByReqItems()
        {
            if (CheckToExecute)
            {
                List<long> reqItemIds = new List<long>();
                long documentCode = TestCaseSourceFactory.GetLongValueFromDataSet("GetApproveddocumentCode", "DocumentCode");
                reqItemIds = TestCaseSourceFactory.GetRequisitionItemIdsBasedonRequisitionId(documentCode);
                var result = requisitionManager.GetRequisitionDetailsByReqItems(reqItemIds);
                if (result != null)
                {
                    Assert.IsTrue(true);
                }
                else
                {
                    Assert.Fail();
                }
            }
            else
            {
                Assert.Inconclusive("Not Executed");
            };
        }

        [TestMethod]

        public void GetDocumentAdditionalEntityDetailsById()
        {
            if (CheckToExecute)
            {
                long documentCode = TestCaseSourceFactory.GetLongValueFromDataSet("GetApproveddocumentCode", "DocumentCode");
                var result = requisitionManager.GetDocumentAdditionalEntityDetailsById(documentCode);
                if (result != null)
                {
                    Assert.IsTrue(true);
                }
                else
                {
                    Assert.Fail();
                }
            }
            else
            {
                Assert.Inconclusive("Not Executed");
            };
        }

        [TestMethod]
        public void GetLineItemCharges()
        {

            if (CheckToExecute)
            {
                long documentCode = TestCaseSourceFactory.GetLongValueFromDataSet("GetApproveddocumentCode", "DocumentCode");
                List<long> reqItemIds = new List<long>();
                reqItemIds = TestCaseSourceFactory.GetRequisitionItemIdsBasedonRequisitionId(documentCode);
                var result = requisitionManager.GetLineItemCharges(reqItemIds, documentCode);
                if (result != null)
                {
                    Assert.IsTrue(true);
                }
                else
                {
                    Assert.Fail();
                }
            }
            else
            {
                Assert.Inconclusive("Not Executed");
            };
        }
        [TestMethod]
        public void GetOrdersListForWorkBench()
        {
            if (CheckToExecute)
            {
                //Need to take Order's create Document 
                long documentCode = TestCaseSourceFactory.GetLongValueFromDataSet("GetOrderCreatedRequisitionId", "DocumentCode");
                List<long> reqItemIds = new List<long>();
                reqItemIds = TestCaseSourceFactory.GetRequisitionItemIdsBasedonRequisitionId(documentCode);
                string itemIds = String.Join(",", reqItemIds);
                var result = requisitionManager.GetOrdersListForWorkBench(itemIds);
                if (result != null)
                {
                    Assert.IsTrue(true);
                }
                else
                {
                    Assert.Fail();
                }
            }
            else
            {
                Assert.Inconclusive("Not Executed");
            };
        }
        [TestMethod]
        public void UpdateRequisitionItemAutoSourceProcessFlag()
        {
            if (CheckToExecute)
            {
                //Need to check the case
                //Need to take Order's create Document 
                long p2pLineItemId = TestCaseSourceFactory.GetLongValueFromDataSet("GetP2PLineItemId", "P2PLineItemID");
                string itemIds = String.Join(",",p2pLineItemId);
                int documentStatus = Convert.ToInt16(DocumentStatus.Draft);
                var result = requisitionManager.UpdateRequisitionItemAutoSourceProcessFlag(itemIds, documentStatus);
                if (result !=null)
                {
                    Assert.IsTrue(true);
                }
                else
                {
                    Assert.Fail();
                }
            }
            else
            {
                Assert.Inconclusive("Not Executed");
            };

        }

        [TestMethod]
        public void GetAllPartnerCodeOrderinglocationIdNadSpendControlItemId()
        {
            if (CheckToExecute)
            {
                long documentCode = TestCaseSourceFactory.GetLongValueFromDataSet("GetOrderCreatedRequisitionId", "DocumentCode");
                var result = requisitionManager.GetAllPartnerCodeOrderinglocationIdNadSpendControlItemId(documentCode);
                if (result != null)
                {
                    Assert.IsTrue(true);
                }
                else
                {
                    Assert.Fail();
                }
            }
            else
            {
                Assert.Inconclusive("Not Executed");
            };
        }


        [TestMethod]
        public void SynChangeRequisition()
        {
            if (CheckToExecute)
            {
                long documentCode = TestCaseSourceFactory.GetLongValueFromDataSet("GetOrderCreatedRequisitionId", "DocumentCode");
                var result = requisitionManager.SyncChangeRequisition(documentCode);
                if (result !=null)
                {
                    Assert.IsTrue(true);
                }
                else
                {
                    Assert.Fail();
                }
            }
            else
            {
                Assert.Inconclusive("Not Executed");
            };
        }

        [TestMethod]
        public void UpdateRequisitionBuyerContactCode()
        {
            if (CheckToExecute)
            {
                long buyerContactCode = 7002182504000001;
                long documentCode = TestCaseSourceFactory.GetLongValueFromDataSet("GetApproveddocumentCode", "DocumentCode");
                List<long> reqItemIds = TestCaseSourceFactory.GetRequisitionItemIdsBasedonRequisitionId(documentCode);
                List<KeyValuePair<long, long>> lstReqItemsToUpdate = new List<KeyValuePair<long, long>>();
                lstReqItemsToUpdate.Add(new KeyValuePair<long, long>(reqItemIds.FirstOrDefault(), buyerContactCode));
                var result = requisitionManager.UpdateRequisitionBuyerContactCode(lstReqItemsToUpdate);
                if (result)
                {
                    Assert.IsTrue(true);
                }
                else
                {
                    Assert.Fail();
                }
            } else
            {
                Assert.Inconclusive("Not Executed");
            }

        }

        // it is orders test case not required for us  GetListErrorCodesByOrderIds where we are checking the validation from orders table 

        [TestMethod]

        public void GetShippingSplitDetailsByLiId()
        {
            if (CheckToExecute)
            {
                long documentCode = TestCaseSourceFactory.GetLongValueFromDataSet("GetApproveddocumentCode", "DocumentCode");
                List<long> reqItemIds = TestCaseSourceFactory.GetRequisitionItemIdsBasedonRequisitionId(documentCode);
                var result = requisitionManager.GetShippingSplitDetailsByLiId(reqItemIds.FirstOrDefault());
                if (result != null)
                {
                    Assert.IsTrue(true);
                }
                else
                {
                    Assert.Fail();
                }
            }
            else
            {
                Assert.Inconclusive("Not Executed");
            }
        }

        [TestMethod]
        public void GetAllPartnerCodeAndOrderinglocationId()
        {
            if (CheckToExecute)
            {
                long documentCode = TestCaseSourceFactory.GetLongValueFromDataSet("GetApproveddocumentCode", "DocumentCode");
                List<long> reqItemIds = TestCaseSourceFactory.GetRequisitionItemIdsBasedonRequisitionId(documentCode);
                var result = requisitionManager.GetAllPartnerCodeAndOrderinglocationId(reqItemIds.FirstOrDefault());
                if (result != null)
                {
                    Assert.IsTrue(true);
                }
                else
                {
                    Assert.Fail();
                }
            }
            else
            {
                Assert.Inconclusive("Not Executed");
            }
        }

    }
}
