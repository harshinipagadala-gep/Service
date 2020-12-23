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
    public class RequisitionDetailsTestCases : TestHelper
    {
        NewRequisitionManager requisitionManager;

        string jwtToken = Helper.JWTToken;

        public RequisitionDetailsTestCases()
        {            
            if (CheckToExecute)
            {
                requisitionManager = new NewRequisitionManager(jwtToken);
                requisitionManager.UserContext = UserContextHelper.GetExecutionContext;
                requisitionManager.GepConfiguration = Helper.InitMultiRegion();
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
                document.documentStatus = new long[] { 22, 61 };
                document.documentType = 7;

                var result = requisitionManager.GetRequisitionDetailsList(document);

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

        

    }
}
