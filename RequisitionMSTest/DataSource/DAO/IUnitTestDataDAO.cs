using Gep.Cumulus.CSM.BaseDataAccessObjects;
using System.Collections.Generic;
using System.Data;

namespace GEP.Cumulus.Requisition.Tests.DataSource.DAO
{
    public interface IUnitTestDataDAO : IBaseDAO
    {
        List<RequisitionData> GetRequisitions();
        List<RequisitionData> GetRequisitionForFixedItemsWithStandardPurchaseType();
        long GetRequisitionWithItemsAmountDefined(short purchaseType);
        long GetRequisitionWithMultipleItemSources(short purchaseType);
        long GetRequisitionWithManualItemsOnly(short purchaseType);
        long GetRequisitionWithoutManualItems(short purchaseType);
        long GetRequisitionWithoutItems(short purchaseType);
        GEP.Cumulus.P2P.BusinessEntities.Requisition GetAllRequisitionDetailsByRequisitionId(long requisitionId, long userId, int typeOfUser, List<long> reqLineItemIds = null, Dictionary<string, string> settings = null);
        RequisitionData GetRequisitions_ExcelTemplate(short type, long documentCode = 0, decimal taxAmount = 0);
        long GetRequisitionWithAnyFixedItems(short purchaseType);
        long GetRequisitionWithMaterialItemsOnly();
        long GetRequisitionWithServiceItemsOnly();
        long GetRequisitionWithoutFixedItems(short purchaseType);
        Dictionary<string, long> GetSinglePartnerAndContactCode();
        IDictionary<long, byte> GetRequisitionWithItemType();
        long GetLongValueFromDataSet(string sqlQueryKey, string lookupKey);
        long GetRequisitionsForRiskScore(short riskType, int? riskScore = null);
        ExecuteTestCases GetExecutionFlag();
        DataSet GetUserMappedORGEntities(int preferenceLOBType);

        string GetstringValueFromDataSet(string sqlQueryKey, string lookupKey);

        List<long> GetRequisitionItemIdsBasedonRequisitionId(long documentCode);
    string GetContractNumber();
  }

}
