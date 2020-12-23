using GEP.Cumulus.Requisition.Tests.DataSource.DAO;
using RequisitionMSTest.DataSource;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace GEP.Cumulus.Requisition.Tests
{
    [ExcludeFromCodeCoverage]
    public static class TestCaseSourceFactory
    {
        public static ExecuteTestCases ExecuteTestCases = null;

        public static ExecuteTestCases SetExecutionFlag()
        {            
            try
            {
                if (ExecuteTestCases == null)
                {
                    TestBOManager manager = new TestBOManager();
                    manager.UserContext = UserContextHelper.GetExecutionContext;
                    manager.GepConfiguration = Helper.InitMultiRegion();

                    ExecuteTestCases = manager.GetExecutionFlag();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return ExecuteTestCases;
        }

        public static List<long> GetDraftRequisitions()
        {
            List<long> result = new List<long>();
            try
            {
                TestBOManager manager = new TestBOManager();               
                manager.UserContext = UserContextHelper.GetExecutionContext;
                manager.GepConfiguration = Helper.InitMultiRegion();

                var requisitions = manager.GetDraftRequisitions();                
                foreach (var req in requisitions)
                {
                    result.Add(req.DocumentCode);
                }                 
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return result;
        }

        public static List<RequisitionData> GetDraftRequisitionsWithDetails()
        {
            List<RequisitionData> result = new List<RequisitionData>();
            try
            {
                TestBOManager manager = new TestBOManager();
                manager.UserContext = UserContextHelper.GetExecutionContext;
                manager.GepConfiguration = Helper.InitMultiRegion();

                result = manager.GetDraftRequisitions();                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return result;
        }

        public static List<RequisitionData> GetRequisitionForFixedItemsWithStandardPurchaseType()
        {
            List<RequisitionData> result = new List<RequisitionData>();
            try
            {
                TestBOManager manager = new TestBOManager();
                manager.UserContext = UserContextHelper.GetExecutionContext;
                manager.GepConfiguration = Helper.InitMultiRegion();

                result = manager.GetRequisitionForFixedItemsWithStandardPurchaseType();               
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return result;
        }

        public static GEP.Cumulus.P2P.BusinessEntities.Requisition GetAllRequisitionDetailsByRequisitionId(long requisitionId, long userId, int typeOfUser, List<long> requisitionLineItemsId = null, Dictionary<string, string> settings = null)
        {
            var businessManager = new TestBOManager()
            {
                UserContext = UserContextHelper.GetExecutionContext,
                GepConfiguration = Helper.InitMultiRegion()
            };

            return businessManager.GetAllRequisitionDetailsByRequisitionId(requisitionId, userId, typeOfUser, requisitionLineItemsId, settings);
        }

        public static long GetRequisitionWithItemsAmountDefined(short purchaseType = 1)
        {
            var businessManager = new TestBOManager()
            {
                UserContext = UserContextHelper.GetExecutionContext,
                GepConfiguration = Helper.InitMultiRegion()
            };

            return businessManager.GetRequisitionWithItemsAmountDefined(purchaseType);
        }

        public static long GetRequisitionWithMultipleItemSources(short purchaseType = 1)
        {
            var businessManager = new TestBOManager()
            {
                UserContext = UserContextHelper.GetExecutionContext,
                GepConfiguration = Helper.InitMultiRegion()
            };

            return businessManager.GetRequisitionWithMultipleItemSources(purchaseType);
        }

        public static long GetRequisitionWithManualItemsOnly(short purchaseType = 1)
        {
            var businessManager = new TestBOManager()
            {
                UserContext = UserContextHelper.GetExecutionContext,
                GepConfiguration = Helper.InitMultiRegion()
            };

            return businessManager.GetRequisitionWithManualItemsOnly(purchaseType);
        }

        public static long GetRequisitionWithoutManualItems(short purchaseType = 1)
        {
            var businessManager = new TestBOManager()
            {
                UserContext = UserContextHelper.GetExecutionContext,
                GepConfiguration = Helper.InitMultiRegion()
            };

            return businessManager.GetRequisitionWithoutManualItems(purchaseType);
        }

        public static long GetRequisitionWithoutItems(short purchaseType = 1)
        {
            var businessManager = new TestBOManager()
            {
                UserContext = UserContextHelper.GetExecutionContext,
                GepConfiguration = Helper.InitMultiRegion()
            };

            return businessManager.GetRequisitionWithoutItems(purchaseType);
        }

        public static long GetRequisitionWithAnyFixedItems(short purchaseType = 1)
        {
            var businessManager = new TestBOManager()
            {
                UserContext = UserContextHelper.GetExecutionContext,
                GepConfiguration = Helper.InitMultiRegion()
            };

            return businessManager.GetRequisitionWithAnyFixedItems(purchaseType);
        }

        public static long GetRequisitionWithMaterialItemsOnly()
        {
            var businessManager = new TestBOManager()
            {
                UserContext = UserContextHelper.GetExecutionContext,
                GepConfiguration = Helper.InitMultiRegion()
            };

            return businessManager.GetRequisitionWithMaterialItemsOnly();
        }

        public static long GetRequisitionWithServiceItemsOnly()
        {
            var businessManager = new TestBOManager()
            {
                UserContext = UserContextHelper.GetExecutionContext,
                GepConfiguration = Helper.InitMultiRegion()
            };

            return businessManager.GetRequisitionWithServiceItemsOnly();
        }

        public static long GetRequisitionWithoutFixedItems(short purchaseType = 1)
        {
            var businessManager = new TestBOManager()
            {
                UserContext = UserContextHelper.GetExecutionContext,
                GepConfiguration = Helper.InitMultiRegion()
            };

            return businessManager.GetRequisitionWithoutFixedItems(purchaseType);
        }
        public static RequisitionData downloadExcelTemplate(short type, long documentCode = 0)
        {
            var businessManager = new TestBOManager()
            {
                UserContext = UserContextHelper.GetExecutionContext,
                GepConfiguration = Helper.InitMultiRegion()
            };

            return businessManager.GetRequisitions_ExcelTemplate(type, documentCode);
        }
        public static RequisitionData GetRequisitionsTaxDetails(long documentCode = 0)
        {
            var businessManager = new TestBOManager()
            {
                UserContext = UserContextHelper.GetExecutionContext,
                GepConfiguration = Helper.InitMultiRegion()
            };

            return businessManager.GetRequisitions_ExcelTemplate(4, documentCode);
        }
        public static RequisitionData UpdateTaxDetails(long documentCode = 0,decimal taxAmount=0)
        {
            var businessManager = new TestBOManager()
            {
                UserContext = UserContextHelper.GetExecutionContext,
                GepConfiguration = Helper.InitMultiRegion()
            };

            return businessManager.GetRequisitions_ExcelTemplate(3, documentCode, taxAmount);
        }
        public static RequisitionData GetRequisitions_WithOutTaxCodes( long documentCode = 0)
        {
            var businessManager = new TestBOManager()
            {
                UserContext = UserContextHelper.GetExecutionContext,
                GepConfiguration = Helper.InitMultiRegion()
            };

            return businessManager.GetRequisitions_ExcelTemplate(1,documentCode);
        }
        public static RequisitionData GetRequisitions_WithTaxCodes(long documentCode = 0)
        {
            var businessManager = new TestBOManager()
            {
                UserContext = UserContextHelper.GetExecutionContext,
                GepConfiguration = Helper.InitMultiRegion()
            };

            return businessManager.GetRequisitions_ExcelTemplate(2, documentCode);
        }

        public static Dictionary<string, long> GetSinglePartnerAndContactCode() {

            var businessManager = new TestBOManager()
            {
                UserContext = UserContextHelper.GetExecutionContext,
                GepConfiguration = Helper.InitMultiRegion()
            };

            return businessManager.GetSinglePartnerAndContactCode();
        }

        public static IDictionary<long, byte> GetRequisitionWithItemType()
        {
            IDictionary<long, byte> result = new Dictionary<long, byte>();
            try
            {
                TestBOManager manager = new TestBOManager();
                manager.UserContext = UserContextHelper.GetExecutionContext;
                manager.GepConfiguration = Helper.InitMultiRegion();

                result = manager.GetRequisitionWithItemType();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return result;
       }

        public static long GetLongValueFromDataSet(string sqlQueryKey,string lookupKey)
        {
            var businessManager = new TestBOManager()
            {
                UserContext = UserContextHelper.GetExecutionContext,
                GepConfiguration = Helper.InitMultiRegion()
            };

            return businessManager.GetLongValueFromDataSet(sqlQueryKey, lookupKey);
        }

        
        public static string GetstringValueFromDataSet(string sqlQueryKey, string lookupKey)
        {
            var businessManager = new TestBOManager()
            {
                UserContext = UserContextHelper.GetExecutionContext,
                GepConfiguration = Helper.InitMultiRegion()
            };

            return businessManager.GetstringValueFromDataSet(sqlQueryKey, lookupKey);
        }

        public static long GetRequisitionsForRiskScore(int riskType, int? riskScore = null)
        {
            var businessManager = new TestBOManager()
            {
                UserContext = UserContextHelper.GetExecutionContext,
                GepConfiguration = Helper.InitMultiRegion()
            };

            return businessManager.GetRequisitionsForRiskScore((short)riskType, riskScore);
        }

        public static DataSet GetUserMappedORGEntities(int preferenceLOBType)
        {
            var businessManager = new TestBOManager()
            {
                UserContext = UserContextHelper.GetExecutionContext,
                GepConfiguration = Helper.InitMultiRegion()
            };

            return businessManager.GetUserMappedORGEntities(preferenceLOBType);
        }

        public static List<long> GetRequisitionItemIdsBasedonRequisitionId(long documentCode)
        {
            var businessManager = new TestBOManager()
            {
                UserContext = UserContextHelper.GetExecutionContext,
                GepConfiguration = Helper.InitMultiRegion()
            };

            return businessManager.GetRequisitionItemIdsBasedonRequisitionId(documentCode);
        }
    public static string GetContractNumber()
    {
      string result = "";
      try
      {
        TestBOManager manager = new TestBOManager();
        manager.UserContext = UserContextHelper.GetExecutionContext;
        manager.GepConfiguration = Helper.InitMultiRegion();

        result = manager.GetContractNumber();
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
      }
      return result;
    }
  }
}
