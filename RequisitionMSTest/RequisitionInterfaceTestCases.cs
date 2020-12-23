using Gep.Cumulus.Partner.Entities;
using GEP.Cumulus.Documents.Entities;
using GEP.Cumulus.P2P.BusinessEntities;
using GEP.Cumulus.P2P.BusinessObjects;
using GEP.Cumulus.P2P.Req.BusinessObjects;
using GEP.Cumulus.P2P.Req.DataAccessObjects;
using GEP.Cumulus.Requisition.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RequisitionMSTest.DataSource;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
//using NUnit.Framework;

namespace RequisitionMSTest
{
    [TestClass]
    public class RequisitionInterfaceTestCases : TestHelper
    {
        RequisitionInterfaceManager reqInterfaceManager;
        RequisitionManager reqManager;
        SQLRequisitionDAO sqlRequisitionDAO;
        RequisitionDocumentManager requisitionDocumentManager;
        NewRequisitionManager newReqManager;

        string jwtToken = Helper.JWTToken;

        Dictionary<string, long> partnerAndContactCodeDict;
        public RequisitionInterfaceTestCases()
        {
            if (CheckToExecute)
            {
                // For test case, need to change this code to fill with real JWT token for testing                
                reqInterfaceManager = new RequisitionInterfaceManager(jwtToken);
                reqInterfaceManager.UserContext = UserContextHelper.GetExecutionContext;
                reqInterfaceManager.GepConfiguration = Helper.InitMultiRegion();

                reqManager = new RequisitionManager(jwtToken);
                reqManager.UserContext = UserContextHelper.GetExecutionContext;
                reqManager.GepConfiguration = Helper.InitMultiRegion();

                requisitionDocumentManager = new RequisitionDocumentManager(jwtToken);
                requisitionDocumentManager.UserContext = UserContextHelper.GetExecutionContext;
                requisitionDocumentManager.GepConfiguration = Helper.InitMultiRegion();

                newReqManager = new NewRequisitionManager(jwtToken);
                newReqManager.UserContext = UserContextHelper.GetExecutionContext;
                newReqManager.GepConfiguration = Helper.InitMultiRegion();

                sqlRequisitionDAO = new SQLRequisitionDAO();
                sqlRequisitionDAO.UserContext = UserContextHelper.GetExecutionContext;
                sqlRequisitionDAO.GepConfiguration = Helper.InitMultiRegion();
                partnerAndContactCodeDict = TestCaseSourceFactory.GetSinglePartnerAndContactCode();
            }
        }

        [TestMethod]
        [DataRow(1)]
        public void GetRequisitionDetailsById(long documentCode)//tested and verified
        {
            if (CheckToExecute)
            {
                documentCode = TestCaseSourceFactory.GetLongValueFromDataSet("GetdocumentCode", "DocumentCode");
                var results = reqManager.GetRequisitionDetailsById(documentCode);
                if (results != null)
                {
                    Assert.IsTrue(results.Requisition != null && results.BuyerContact != null);
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
        public void GetLineItemBasicDetailsForInterface(long documentCode)//tested and verified
        {
            if (CheckToExecute)
            {
                documentCode = TestCaseSourceFactory.GetLongValueFromDataSet("GetdocumentCode", "DocumentCode");
                var results = reqInterfaceManager.GetLineItemBasicDetailsForInterface(documentCode);
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
        [DataRow(P2PDocumentType.Requisition, 400768, 0)]
        public void GetRequisitionListForInterfaces(P2PDocumentType docType, int docCount, int sourceSystemId)//tested and verified
        {
            if (CheckToExecute)
            {
                docType = P2PDocumentType.Requisition;
                sourceSystemId = 0;
                docCount = Convert.ToInt32(TestCaseSourceFactory.GetLongValueFromDataSet("GetdocumentCode", "DocumentCode"));
                //sourceSystemId = Convert.ToInt32(TestCaseSourceFactory.GetLongValueFromDataSet("GetsourceSystemId", "sourceSystemId"));//1
                var results = reqInterfaceManager.GetRequisitionListForInterfaces(docType.ToString(), docCount, sourceSystemId);
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
        public void ValidateInterfaceLineStatus()
        {
            if (CheckToExecute)
            {
                var documentCode = TestCaseSourceFactory.GetLongValueFromDataSet("GetdocumentCode", "DocumentCode");
                long buyerPartnerCode = UserContextHelper.GetExecutionContext.BuyerPartnerCode;
                DataTable dtRequisitionDetail = new DataTable() { Locale = CultureInfo.InvariantCulture, TableName = "TVP_P2P_REQUISITIONLINEITEM" };
                dtRequisitionDetail.Columns.Add("RequisitionNumber", typeof(string));
                dtRequisitionDetail.Columns.Add("RequisitionLineNumber", typeof(long));
                dtRequisitionDetail.Columns.Add("RequisitionId", typeof(long));
                dtRequisitionDetail.Columns.Add("StockReservationStatusForLineItem", typeof(string));
                dtRequisitionDetail.Columns.Add("RequisitionStockStatus", typeof(string));
                dtRequisitionDetail.Columns.Add("IsUpdateAllLineItems", typeof(bool));
                dtRequisitionDetail.Columns.Add("ReservationNumber", typeof(string));
                dtRequisitionDetail.Columns.Add("StockReservationNumber", typeof(string));

                DataSet results = reqInterfaceManager.ValidateInterfaceLineStatus(buyerPartnerCode, dtRequisitionDetail);
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
        public void ValidateInterfaceRequisition()
        {
            if (CheckToExecute)
            {
                long documentCode = TestCaseSourceFactory.GetLongValueFromDataSet("GetdocumentCode", "DocumentCode");
                BZRequisition objRequisition = reqManager.GetRequisitionDetailsById(documentCode);

                //This is failing when DefaultBillToLocation is not preset in dctSetting in RequisitionInterfaceManager>ValidateShipToBillToFromInterface method.
                var results = reqInterfaceManager.ValidateInterfaceRequisition(objRequisition.Requisition);

                if (results != null)
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
        public void SaveP2PDocumentfromInterface()
        {
            if (CheckToExecute)
            {
                P2PDocumentType docType = P2PDocumentType.Requisition;
                long documentCode = TestCaseSourceFactory.GetLongValueFromDataSet("GetdocumentCode", "DocumentCode");
                var reqID = TestCaseSourceFactory.GetLongValueFromDataSet("RequisitionWithInactiveEntitiesWithoutParentChildMapping", "RequisitionID");
                if (reqID == 0)
                {
                    Assert.IsTrue(true, "Requisition not found for the test case");
                    return;
                }
                P2PDocument requisitionObj = requisitionDocumentManager.GetAllRequisitionDetailsByRequisitionId(reqID);
                int partnerInterfaceId = 1;
            //Need to re-visit this as the data for the object is Unknown
               requisitionObj.Operation = "save";
               var documentLObDetails = newReqManager.GetDocumentLOBByDocumentCode(reqID);
               requisitionObj.DocumentLOBDetails = new List<DocumentLOBDetails>();
               requisitionObj.DocumentLOBDetails.Add(documentLObDetails);

                var results = reqInterfaceManager.SaveP2PDocumentfromInterface(docType, requisitionObj, partnerInterfaceId);

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
        public void UpdateLineStatusForRequisitionFromInterface()
        {
            if (CheckToExecute)
            {

                P2PDocumentType docType = P2PDocumentType.Requisition;
                long documentCode = TestCaseSourceFactory.GetLongValueFromDataSet("GetdocumentCode", "DocumentCode");
                long RequisitionNumber = TestCaseSourceFactory.GetLongValueFromDataSet("GetrequisitionNumber", "RequisitionNumber");

                //Need to re-visit this as the data for the object is Unknown
                RequisitionLineStatusUpdateDetails obj1 = new RequisitionLineStatusUpdateDetails { StockReservationNumber = "1" };
                List<RequisitionLineStatusUpdateDetails> reqDetails = new List<RequisitionLineStatusUpdateDetails> { obj1 };


                var results = reqInterfaceManager.UpdateLineStatusForRequisitionFromInterface(reqDetails);

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
        //[DataRow(289739, 1)]
        //public void UpdateRequisitionLineStatus(long lRequisitionId, long lBuyerPartnerCode)//tested and verified
        //{
        //    if (CheckToExecute)
        //    {
        //        //lRequisitionId = TestCaseSourceFactory.GetLongValueFromDataSet("RequisitionWithInactiveEntitiesWithoutParentChildMapping", "RequisitionID");
        //        if (lRequisitionId == 0)
        //        {
        //            Assert.IsTrue(true, "Requisition not found for the test case");
        //            return;
        //        }
        //        lBuyerPartnerCode = UserContextHelper.GetExecutionContext.BuyerPartnerCode;

        //        //Returns void!!
        //        reqManager.UpdateRequisitionLineStatus(lRequisitionId, lBuyerPartnerCode);
        //        Assert.IsTrue(true);
        //    }
        //    else
        //    {
        //        Assert.Fail("Not executed");
        //    }
        //}

        [TestMethod]
        public void SaveReqCartItemsFromInterface()
        {
            if (CheckToExecute)
            {
                //Need to re-visit this as the data for the object is unknown
                Int64 iUserId = sqlRequisitionDAO.UserContext.ContactCode;
                Int64 iPartnerCode = 198798;
                List<GEP.Cumulus.P2P.BusinessEntities.P2PItem> lstP2PItems = new List<P2PItem>();
                int iPrecessionValue = 1;
                int iPartnerConfigurationId = 1;
                string sDocumentCode = TestCaseSourceFactory.GetLongValueFromDataSet("GetdocumentCode", "DocumentCode").ToString();
                Int64 iPunchoutCartReqId = 1;
                decimal dTax = 0;
                decimal dShipping = 0;
                decimal dAdditionalCharges = 0;

                var results = reqInterfaceManager.SaveReqCartItemsFromInterface(iUserId, iPartnerCode, lstP2PItems, iPrecessionValue, iPartnerConfigurationId, sDocumentCode, iPunchoutCartReqId, dTax, dShipping, dAdditionalCharges);
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
                Assert.Fail("Not executed");
            }
        }

        [TestMethod]
        public void SaveRequisitionFromInterface()
        {
            if (CheckToExecute)
            {
                //Need to re-visit this as the data for the object is Unknown
                long documentCode = TestCaseSourceFactory.GetLongValueFromDataSet("GetdocumentCode", "DocumentCode");
                var reqID = TestCaseSourceFactory.GetLongValueFromDataSet("RequisitionWithInactiveEntitiesWithoutParentChildMapping", "RequisitionID");
                if (reqID == 0)
                {
                    Assert.IsTrue(true, "Requisition not found for the test case");
                    return;
                }
                RequisitionCommonManager objCommon = new RequisitionCommonManager(jwtToken);
                P2PDocument requisitionObj = requisitionDocumentManager.GetAllRequisitionDetailsByRequisitionId(documentCode);
                List<ContactORGMapping> lstBUDetails = new List<ContactORGMapping>();
                var documentLObDetails = newReqManager.GetDocumentLOBByDocumentCode(documentCode);
                requisitionObj.DocumentLOBDetails = new List<DocumentLOBDetails>();
                requisitionObj.DocumentLOBDetails.Add(documentLObDetails);
                var objShippingDetails = sqlRequisitionDAO.GetShippingSplitDetailsByLiId(reqID);
                objCommon.UserContext = UserContextHelper.GetExecutionContext;
                objCommon.GepConfiguration = Helper.InitMultiRegion();
                int partnerInterfaceId = 1;
                int accessControlEntityId = requisitionDocumentManager.GetAccessControlEntityId(P2PDocumentType.Requisition, UserContextHelper.GetExecutionContext.ContactCode);
                var results = reqInterfaceManager.SaveRequisitionFromInterface(requisitionObj, lstBUDetails, ref objCommon, partnerInterfaceId, accessControlEntityId);
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
                Assert.Fail("not executed");
            }
        }

        [TestMethod]
        public void GetRequistionDetailsForInterfaces()
        {
            if (CheckToExecute)
            {
                long documentCode = 0; 
                var result = reqInterfaceManager.GetRequistionDetailsForInterfaces(documentCode);
                if (result != null)
                {
                    Assert.IsTrue(true);
                }
                else
                {
                    Assert.Fail();
                }
            } else
            {
                Assert.Inconclusive("Not executed");
            }
        }



    }
}
