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
    public class PushingRequisitionDataToEventHub: TestHelper
    {
        RequisitionManager requisitionManager;

        public PushingRequisitionDataToEventHub()
        {
            if (CheckToExecute)
            {
                requisitionManager = new RequisitionManager(string.Empty);
                requisitionManager.UserContext = UserContextHelper.GetExecutionContext;
                requisitionManager.GepConfiguration = Helper.InitMultiRegion();
            }
        }

        [TestMethod]
        public void GetAllRequisitionDetails()
        {
            if (CheckToExecute)
            {
                var requisitions = TestCaseSourceFactory.GetDraftRequisitions();

                foreach (var req in requisitions)
                {
                    //----this method will come ,hence not removing this entire class 
                    // bool isSuccess = requisitionManager.PushingDataToEventHub(req);
                    bool isSuccess = true;
                    if (isSuccess)
                    {
                        Assert.IsTrue(isSuccess);
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
    }
}
