using GEP.Cumulus.P2P.Req.BusinessObjects;
using GEP.Cumulus.Requisition.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RequisitionMSTest.DataSource;
using System;
using System.Collections.Generic;
using GEP.Cumulus.P2P.BusinessEntities;
using System.Diagnostics.CodeAnalysis;

namespace RequisitionMSTest
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class AdditionalFieldsTestCases : TestHelper
    {
        AdditionalFieldsManager additionalFieldsManager;

        public AdditionalFieldsTestCases()
        {
            additionalFieldsManager = new AdditionalFieldsManager();
            additionalFieldsManager.UserContext = UserContextHelper.GetExecutionContext;
            additionalFieldsManager.GepConfiguration = Helper.InitMultiRegion();
        }

        [TestMethod]
        public void GetAdditionalFieldsMasterDataByFieldControlType()
        {
            if (CheckToExecute)
            {
                var result = additionalFieldsManager.GetAdditionalFieldsMasterDataByFieldControlType(7, 0);

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
        public void GetPurchaseTypeFeatureMapping()
        {
            if (CheckToExecute)
            {
                List<PurchaseTypeFeatureMapping> res = additionalFieldsManager.GetAdditionalFieldsByDocumentType(7, 1571824, 0, 0, true);
                if (res != null && res.Count > 0)
                {
                    List<long> purchaseTypes = new List<long>();
                    foreach (var a in res)
                    {
                        purchaseTypes.Add(a.PurchaseTypeId);
                    }
                    if (purchaseTypes != null)
                    {
                        Assert.IsTrue(purchaseTypes.Count > 0);
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
