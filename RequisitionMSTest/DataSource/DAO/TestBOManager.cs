using Gep.Cumulus.CSM.BaseBusinessObjects;
using Gep.Cumulus.CSM.BaseDataAccessObjects;
using GEP.Cumulus.P2P.Req.DataAccessObjects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace GEP.Cumulus.Requisition.Tests.DataSource.DAO
{
    [ExcludeFromCodeCoverage]
    public class TestBOManager : BaseBO
    {
        private static IDictionary<Type, object> InitializeDaoList()
        {
            IDictionary<Type, object> daoList = new Dictionary<Type, object>();
            daoList.Add(typeof(IUnitTestDataDAO), new UnitTestDataDAO());
            daoList.Add(typeof(IRequisitionDAO), new SQLRequisitionDAO());

            return daoList;
        }

        protected T GetDao<T>() where T : IBaseDAO
        {
            IDictionary<Type, object> DaoList = InitializeDaoList();
            var dao = (T)DaoList[typeof(T)];
            dao.UserContext = UserContext;
            dao.GepConfiguration = GepConfiguration;
            return dao;
        }

        public List<RequisitionData> GetDraftRequisitions()
        {
            return GetDao<IUnitTestDataDAO>().GetRequisitions();
        }

        public List<RequisitionData> GetRequisitionForFixedItemsWithStandardPurchaseType()
        {
            return GetDao<IUnitTestDataDAO>().GetRequisitionForFixedItemsWithStandardPurchaseType();
        }

        public GEP.Cumulus.P2P.BusinessEntities.Requisition GetAllRequisitionDetailsByRequisitionId(long requisitionId, long userId, int typeOfUser, List<long> requisitionLineItemsId = null, Dictionary<string, string> settings = null)
        {
            return GetDao<IRequisitionDAO>().GetAllRequisitionDetailsByRequisitionId(requisitionId, userId, typeOfUser, requisitionLineItemsId, settings);
            // TODO: Need to fix this test case method
            //return null;
        }

        public long GetRequisitionWithItemsAmountDefined(short purchaseType)
        {
            return GetDao<IUnitTestDataDAO>().GetRequisitionWithItemsAmountDefined(purchaseType);
        }

        public long GetRequisitionWithMultipleItemSources(short purchaseType)
        {
            return GetDao<IUnitTestDataDAO>().GetRequisitionWithMultipleItemSources(purchaseType);
        }

        public long GetRequisitionWithManualItemsOnly(short purchaseType)
        {
            return GetDao<IUnitTestDataDAO>().GetRequisitionWithManualItemsOnly(purchaseType);
        }

        public long GetRequisitionWithoutManualItems(short purchaseType)
        {
            return GetDao<IUnitTestDataDAO>().GetRequisitionWithoutManualItems(purchaseType);
        }

        public long GetRequisitionWithoutItems(short purchaseType)
        {
            return GetDao<IUnitTestDataDAO>().GetRequisitionWithoutItems(purchaseType);
        }

        public long GetRequisitionWithAnyFixedItems(short purchaseType)
        {
            return GetDao<IUnitTestDataDAO>().GetRequisitionWithAnyFixedItems(purchaseType);
        }

        public long GetRequisitionWithMaterialItemsOnly()
        {
            return GetDao<IUnitTestDataDAO>().GetRequisitionWithMaterialItemsOnly();
        }

        public long GetRequisitionWithServiceItemsOnly()
        {
            return GetDao<IUnitTestDataDAO>().GetRequisitionWithServiceItemsOnly();
        }

        public long GetRequisitionWithoutFixedItems(short purchaseType)
        {
            return GetDao<IUnitTestDataDAO>().GetRequisitionWithoutFixedItems(purchaseType);
        }
        public RequisitionData GetRequisitions_ExcelTemplate(short type, long documentCode = 0, decimal taxAmount = 0)
        {
            return GetDao<IUnitTestDataDAO>().GetRequisitions_ExcelTemplate(type, documentCode, taxAmount);
        }

        public Dictionary<string, long> GetSinglePartnerAndContactCode()
        {
            return GetDao<IUnitTestDataDAO>().GetSinglePartnerAndContactCode();
        }

        public IDictionary<long, byte> GetRequisitionWithItemType()
        {
            return GetDao<IUnitTestDataDAO>().GetRequisitionWithItemType();
        }
        public long GetLongValueFromDataSet(string sqlQueryKey, string lookupKey)
        {
            return GetDao<IUnitTestDataDAO>().GetLongValueFromDataSet(sqlQueryKey, lookupKey);
        }

        public string GetstringValueFromDataSet(string sqlQueryKey, string lookupKey)
        {
            return GetDao<IUnitTestDataDAO>().GetstringValueFromDataSet(sqlQueryKey, lookupKey);
        }
        public long GetRequisitionsForRiskScore(short riskType, int? riskScore = null)
        {
            return GetDao<IUnitTestDataDAO>().GetRequisitionsForRiskScore(riskType, riskScore);
        }

        public ExecuteTestCases GetExecutionFlag()
        {
            return GetDao<IUnitTestDataDAO>().GetExecutionFlag();
        }

        public DataSet GetUserMappedORGEntities(int preferenceLOBType)
        {
            return GetDao<IUnitTestDataDAO>().GetUserMappedORGEntities(preferenceLOBType);
        }

        public List<long> GetRequisitionItemIdsBasedonRequisitionId(long documentCode)
        {
            return GetDao<IUnitTestDataDAO>().GetRequisitionItemIdsBasedonRequisitionId(documentCode);
        }
    public string GetContractNumber()
    {
      return GetDao<IUnitTestDataDAO>().GetContractNumber();
    }
  }
}
