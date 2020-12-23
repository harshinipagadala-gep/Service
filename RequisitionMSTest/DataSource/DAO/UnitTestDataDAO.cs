using Gep.Cumulus.CSM.Config;
using Gep.Cumulus.CSM.Entities;
using GEP.Cumulus.Documents.DataAccessObjects.SQLServer;
using GEP.Cumulus.P2P.BusinessEntities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace GEP.Cumulus.Requisition.Tests.DataSource.DAO
{
    [ExcludeFromCodeCoverage]
    public class UnitTestDataDAO : SQLDocumentDAO, IUnitTestDataDAO
    {
        
        public UnitTestDataDAO(UserExecutionContext userContext, GepConfig gepConfiguration)
        {
            UserContext = userContext;
            GepConfiguration = gepConfiguration;
        }

        public UnitTestDataDAO()
        {

        }

        public List<RequisitionData> GetRequisitions()
        {
            List<RequisitionData> result = new List<RequisitionData>();
            try
            {
                long Creator = UserContext.ContactCode;
                string sql = ConfigurationManager.AppSettings["DraftRequisitionSQL"].ToString();
                var data = ContextSqlConn.ExecuteDataSet(System.Data.CommandType.Text, sql);
                // DocumentCode, DocumentStatus, Creator
                if (data != null && data.Tables.Count > 0)
                {
                    if (data.Tables[0].Rows != null && data.Tables[0].Rows.Count > 0)
                    {
                        foreach (System.Data.DataRow row in data.Tables[0].Rows)
                        {
                            result.Add(Fill(row));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception - " + ex.Message);
            }
            return result;
        }

        public ExecuteTestCases GetExecutionFlag()
        {
            ExecuteTestCases result = null;
            try
            {
                long Creator = UserContext.ContactCode;
                string sql = "SELECT EXECUTETESTCASES, SCHEDULE FROM REQUISITIONTESTEXECUTION";
                var data = ContextSqlConn.ExecuteDataSet(System.Data.CommandType.Text, sql);                
                if (data != null && data.Tables.Count > 0)
                {
                    if (data.Tables[0].Rows != null && data.Tables[0].Rows.Count > 0)
                    {
                        foreach (System.Data.DataRow row in data.Tables[0].Rows)
                        {
                            var execute = Convert.ToBoolean(row["EXECUTETESTCASES"]);
                            var executiontime = Convert.ToDateTime(row["SCHEDULE"]);

                            result = new ExecuteTestCases(execute, executiontime);
                            break;
                        }
                    }
                }
            }            

            catch (Exception ex)
            {
                Console.WriteLine("Exception - " + ex.Message);
            }

            

            return result;
        }

        public List<RequisitionData> GetRequisitionForFixedItemsWithStandardPurchaseType()
        {
            List<RequisitionData> result = new List<RequisitionData>();
            try
            {
                long Creator = UserContext.ContactCode;
                string sql = "SELECT TOP 1 DOCUMENTCODE, DOCUMENTSTATUS, CREATOR FROM DM_DOCUMENTS INNER JOIN dbo.P2P_Requisition On DocumentCode = P2P_Requisition.RequisitionID Inner JOIN P2P_RequisitionItems On P2P_Requisition.RequisitionID = P2P_RequisitionItems.RequisitionID Where P2P_RequisitionItems.ItemTypeID = 2 AND P2P_RequisitionItems.IsDeleted = 0 AND P2P_Requisition.PurchaseType = 1 Order by P2P_Requisition.RequisitionID DESC";
                var data = ContextSqlConn.ExecuteDataSet(System.Data.CommandType.Text, sql);
                // DocumentCode, DocumentStatus, Creator
                if (data != null && data.Tables.Count > 0)
                {
                    if (data.Tables[0].Rows != null && data.Tables[0].Rows.Count > 0)
                    {
                        foreach (System.Data.DataRow row in data.Tables[0].Rows)
                        {
                            result.Add(Fill(row));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception - " + ex.Message);
            }
            return result;
        }


        private RequisitionData Fill(System.Data.DataRow row)
        {
            return new RequisitionData()
            {
                DocumentCode = Convert.ToInt64(row["DocumentCode"]),
                DocumentStatus = Convert.ToInt32(row["DocumentStatus"]),
                Creator = Convert.ToInt64(row["Creator"])
            };
        }

        /// <summary>
        /// Get a requisition with items that their amount is greater than zero.
        /// </summary>
        /// <returns>Requisition Id.</returns>
        public long GetRequisitionWithItemsAmountDefined(short purchaseType)
        {
            long requisitionId = 0;

            try
            {
                var sqlStatement = new StringBuilder(ConfigurationManager.AppSettings["RequisitionWithItemsAmountDefinedSQL"]);
                sqlStatement.Replace("@PurchaseType", purchaseType.ToString());

                var resultSet = ContextSqlConn.ExecuteDataSet(System.Data.CommandType.Text, sqlStatement.ToString());

                if (resultSet != null && resultSet.Tables.Count > 0)
                {
                    if (resultSet.Tables[0].Rows != null && resultSet.Tables[0].Rows.Count > 0)
                    {
                        foreach (System.Data.DataRow row in resultSet.Tables[0].Rows)
                        {
                            requisitionId = Convert.ToInt64(row["RequisitionID"]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception - " + ex.Message);
            }

            return requisitionId;
        }

        /// <summary>
        /// Get requisition that has multiple item sources (Hosted, Punchout, Manual, and so on).
        /// </summary>
        /// <returns>Requisition Id.</returns>
        public long GetRequisitionWithMultipleItemSources(short purchaseType)
        {
            long requisitionId = 0;

            try
            {
                var sqlStatement = new StringBuilder(ConfigurationManager.AppSettings["RequisitionWithMultipleItemSourcesSQL"]);
                sqlStatement.Replace("@PurchaseType", purchaseType.ToString());

                var resultSet = ContextSqlConn.ExecuteDataSet(System.Data.CommandType.Text, sqlStatement.ToString());

                if (resultSet != null && resultSet.Tables.Count > 0)
                {
                    if (resultSet.Tables[0].Rows != null && resultSet.Tables[0].Rows.Count > 0)
                    {
                        foreach (System.Data.DataRow row in resultSet.Tables[0].Rows)
                        {
                            requisitionId = Convert.ToInt64(row["RequisitionID"]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception - " + ex.Message);
            }

            return requisitionId;
        }

        /// <summary>
        /// Get requisition with manual items only.
        /// </summary>
        /// <returns>Requisition Id.</returns>
        public long GetRequisitionWithManualItemsOnly(short purchaseType)
        {
            long requisitionId = 0;

            try
            {
                var sqlStatement = new StringBuilder(ConfigurationManager.AppSettings["RequisitionWithManualItemsOnlySQL"]);
                sqlStatement.Replace("@PurchaseType", purchaseType.ToString());

                var resultSet = ContextSqlConn.ExecuteDataSet(System.Data.CommandType.Text, sqlStatement.ToString());

                if (resultSet != null && resultSet.Tables.Count > 0)
                {
                    if (resultSet.Tables[0].Rows != null && resultSet.Tables[0].Rows.Count > 0)
                    {
                        foreach (System.Data.DataRow row in resultSet.Tables[0].Rows)
                        {
                            requisitionId = Convert.ToInt64(row["RequisitionID"]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception - " + ex.Message);
            }

            return requisitionId;
        }

        /// <summary>
        /// Get requisition without manual items.
        /// </summary>
        /// <returns>Requisition Id.</returns>
        public long GetRequisitionWithoutManualItems(short purchaseType)
        {
            long requisitionId = 0;

            try
            {
                var sqlStatement = new StringBuilder(ConfigurationManager.AppSettings["RequisitionWithoutManualItemsSQL"]);
                sqlStatement.Replace("@PurchaseType", purchaseType.ToString());

                var resultSet = ContextSqlConn.ExecuteDataSet(System.Data.CommandType.Text, sqlStatement.ToString());

                if (resultSet != null && resultSet.Tables.Count > 0)
                {
                    if (resultSet.Tables[0].Rows != null && resultSet.Tables[0].Rows.Count > 0)
                    {
                        foreach (System.Data.DataRow row in resultSet.Tables[0].Rows)
                        {
                            requisitionId = Convert.ToInt64(row["RequisitionID"]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception - " + ex.Message);
            }

            return requisitionId;
        }

        /// <summary>
        /// Get requisition that doesn't have items.
        /// </summary>
        /// <returns>Requisition Id</returns>
        public long GetRequisitionWithoutItems(short purchaseType)
        {
            long requisitionId = 0;

            try
            {
                var sqlStatement = new StringBuilder(ConfigurationManager.AppSettings["RequisitionWithoutItemsSQL"]);
                sqlStatement.Replace("@PurchaseType", purchaseType.ToString());

                var resultSet = ContextSqlConn.ExecuteDataSet(System.Data.CommandType.Text, sqlStatement.ToString());

                if (resultSet != null && resultSet.Tables.Count > 0)
                {
                    if (resultSet.Tables[0].Rows != null && resultSet.Tables[0].Rows.Count > 0)
                    {
                        foreach (System.Data.DataRow row in resultSet.Tables[0].Rows)
                        {
                            requisitionId = Convert.ToInt64(row["RequisitionID"]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception - " + ex.Message);
            }

            return requisitionId;
        }

        public P2P.BusinessEntities.Requisition GetAllRequisitionDetailsByRequisitionId(long requisitionId, long userId, int typeOfUser, List<long> reqLineItemIds = null, Dictionary<string, string> settings = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get a Requisition that at least has a Fixed item.
        /// </summary>
        /// <param name="purchaseType">requisition purchase type (Standard, Blanket)</param>
        /// <returns>requisition id (document code)</returns>
        public long GetRequisitionWithAnyFixedItems(short purchaseType)
        {
            long requisitionId = 0;

            try
            {
                var sqlStatement = new StringBuilder(ConfigurationManager.AppSettings["RequisitionWithFixedItemsSQL"]);
                sqlStatement.Replace("@PurchaseType", purchaseType.ToString());

                var resultSet = ContextSqlConn.ExecuteDataSet(System.Data.CommandType.Text, sqlStatement.ToString());

                if (resultSet != null && resultSet.Tables.Count > 0)
                {
                    if (resultSet.Tables[0].Rows != null && resultSet.Tables[0].Rows.Count > 0)
                    {
                        foreach (System.Data.DataRow row in resultSet.Tables[0].Rows)
                        {
                            requisitionId = Convert.ToInt64(row["DocumentCode"]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception - " + ex.Message);
            }

            return requisitionId;
        }

        /// <summary>
        /// Get a Requisition that only has Material items.
        /// </summary>
        /// <returns>requisition id (document code)</returns>
        public long GetRequisitionWithMaterialItemsOnly()
        {
            long requisitionId = 0;

            try
            {
                var sqlStatement = new StringBuilder(ConfigurationManager.AppSettings["RequisitionWithMaterialItemsOnlySQL"]);

                var resultSet = ContextSqlConn.ExecuteDataSet(System.Data.CommandType.Text, sqlStatement.ToString());

                if (resultSet != null && resultSet.Tables.Count > 0)
                {
                    if (resultSet.Tables[0].Rows != null && resultSet.Tables[0].Rows.Count > 0)
                    {
                        foreach (System.Data.DataRow row in resultSet.Tables[0].Rows)
                        {
                            requisitionId = Convert.ToInt64(row["DocumentCode"]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception - " + ex.Message);
            }

            return requisitionId;
        }

        /// <summary>
        /// Get a Requisition that only has Service items.
        /// </summary>
        /// <returns>requisition id (document code)</returns>
        public long GetRequisitionWithServiceItemsOnly()
        {
            long requisitionId = 0;

            try
            {
                var sqlStatement = new StringBuilder(ConfigurationManager.AppSettings["RequisitionWithServiceItemsOnlySQL"]);

                var resultSet = ContextSqlConn.ExecuteDataSet(System.Data.CommandType.Text, sqlStatement.ToString());

                if (resultSet != null && resultSet.Tables.Count > 0)
                {
                    if (resultSet.Tables[0].Rows != null && resultSet.Tables[0].Rows.Count > 0)
                    {
                        foreach (System.Data.DataRow row in resultSet.Tables[0].Rows)
                        {
                            requisitionId = Convert.ToInt64(row["DocumentCode"]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception - " + ex.Message);
            }

            return requisitionId;
        }

        /// <summary>
        /// Get Requisition without fixed items.
        /// </summary>
        /// <param name="purchaseType">the requisition purchase type (Standard, Blanket Order)</param>
        /// <returns>the requisition id (document code)</returns>
        public long GetRequisitionWithoutFixedItems(short purchaseType)
        {
            long requisitionId = 0;

            try
            {
                var sqlStatement = new StringBuilder(ConfigurationManager.AppSettings["RequisitionWithoutFixedItemsSQL"]);
                sqlStatement.Replace("@PurchaseType", purchaseType.ToString());

                var resultSet = ContextSqlConn.ExecuteDataSet(System.Data.CommandType.Text, sqlStatement.ToString());

                if (resultSet != null && resultSet.Tables.Count > 0)
                {
                    if (resultSet.Tables[0].Rows != null && resultSet.Tables[0].Rows.Count > 0)
                    {
                        foreach (System.Data.DataRow row in resultSet.Tables[0].Rows)
                        {
                            requisitionId = Convert.ToInt64(row["DocumentCode"]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception - " + ex.Message);
            }

            return requisitionId;
        }


        public Dictionary<string, long> GetSinglePartnerAndContactCode()
        {
            Dictionary<string, long> resultDict = new Dictionary<string, long>();
            resultDict.Add("contactCode", 0);
            resultDict.Add("partnerCode", 0);
            try
            {
                string query = ConfigurationManager.AppSettings["GetPartnerAndContactCode"];
                var resultSet = ContextSqlConn.ExecuteDataSet(System.Data.CommandType.Text, query);

                if (resultSet != null && resultSet.Tables.Count > 0 && resultSet.Tables[0].Rows != null && resultSet.Tables[0].Rows.Count > 0)
                {
                    resultDict["contactCode"] = Convert.ToInt64(resultSet.Tables[0].Rows[0]["ContactCode"]);
                    resultDict["partnerCode"] = Convert.ToInt64(resultSet.Tables[0].Rows[0]["PartnerCode"]);
                }
            }
            catch (Exception ex) {
                Console.WriteLine("N-UNIT: Exception in method GetSinglePartnerAndContactCode: " + ex.Message);
            }

            return resultDict;

        }
        public RequisitionData GetRequisitions_ExcelTemplate(short type,long documentCode=0, decimal taxAmount = 0)
        {
            RequisitionData req = new RequisitionData();

            try
            {
                string query = "";
                switch(type)
                {
                    case 1:
                        query = ConfigurationManager.AppSettings["RequisitionWithouttaxcode"];
                        break;
                    case 2:
                        query = ConfigurationManager.AppSettings["RequisitionWithtaxcode"];
                        break;
                    case 3:
                        query = ConfigurationManager.AppSettings["UpdateTaxestoZero"];
                        break;
                    case 4:
                        query = ConfigurationManager.AppSettings["GetTaxes"];
                        break;
                    case 5:
                        query = ConfigurationManager.AppSettings["RequisitionWithFixedOrVariableLineItems"];
                        break;
                }
                var sqlStatement = new StringBuilder(query);
                sqlStatement.Replace("@RequisitionID", documentCode.ToString());
                sqlStatement.Replace("@taxAmount", taxAmount.ToString());

                switch (type)
                {
                    case 1:
                    case 2:
                    var resultSet = ContextSqlConn.ExecuteDataSet(System.Data.CommandType.Text, sqlStatement.ToString());

                    if (resultSet != null && resultSet.Tables.Count > 0)
                    {
                        if (resultSet.Tables[0].Rows != null && resultSet.Tables[0].Rows.Count > 0)
                        {
                            foreach (System.Data.DataRow row in resultSet.Tables[0].Rows)
                            {
                                req.DocumentCode = Convert.ToInt64(row["DocumentCode"]);
                                req.DocumentNumber = Convert.ToString(row["DocumentNumber"]);
                            }
                        }
                    }
                        break;
                    case 3:
                     resultSet = ContextSqlConn.ExecuteDataSet(System.Data.CommandType.Text, sqlStatement.ToString());
                        break;
                    case 4:
                         resultSet = ContextSqlConn.ExecuteDataSet(System.Data.CommandType.Text, sqlStatement.ToString());

                        if (resultSet != null && resultSet.Tables.Count > 0)
                        {
                            if (resultSet.Tables[0].Rows != null && resultSet.Tables[0].Rows.Count > 0)
                            {
                                foreach (System.Data.DataRow row in resultSet.Tables[0].Rows)
                                {
                                    req.Tax = Convert.ToDecimal(row["Tax"]);
                                    
                                }
                            }
                        }
                        break;
                    case 5:
                        resultSet = ContextSqlConn.ExecuteDataSet(System.Data.CommandType.Text, sqlStatement.ToString());
                        if(resultSet != null && resultSet.Tables.Count > 0)
                        {
                            if(resultSet.Tables[0].Rows != null && resultSet.Tables[0].Rows.Count > 0)
                            {
                                foreach(System.Data.DataRow row in resultSet.Tables[0].Rows)
                                {
                                    req.DocumentCode = Convert.ToInt64(row["DocumentCode"]);
                                    req.DocumentNumber = Convert.ToString(row["DocumentNumber"]);
                                }

                            }
                        }
                        break;
            }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception - " + ex.Message);
            }

            return req;
        }

        public IDictionary<long, byte> GetRequisitionWithItemType()
        {
            IDictionary<long, byte> resultDict = new Dictionary<long, byte>();
            try
            {
                string query = ConfigurationManager.AppSettings["GetRequisitionWithItemType"].ToString();
                var reqresult = ContextSqlConn.ExecuteReader(System.Data.CommandType.Text, query);
                if (reqresult.Read())
                {
                    long key = (long)reqresult["DocumentCode"];
                    byte value = (byte)reqresult["ItemTypeId"];
                    resultDict.Add(key, value);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("N-UNIT: Exception in method GetRequisitionWithItemType: " + ex.Message);
            }
            return resultDict;
       }
        public long GetLongValueFromDataSet(string sqlQueryKey, string lookupKey)
        {
            long requisitionId = 0;

            try
            {
                 var sqlStatement = new StringBuilder(ConfigurationManager.AppSettings[sqlQueryKey]);

                 var resultSet = ContextSqlConn.ExecuteDataSet(System.Data.CommandType.Text, sqlStatement.ToString());

                 if (resultSet != null && resultSet.Tables.Count > 0)
                 {
                    if (resultSet.Tables[0].Rows != null && resultSet.Tables[0].Rows.Count > 0)
                    {
                      foreach (System.Data.DataRow row in resultSet.Tables[0].Rows)
                      {
                        requisitionId = Convert.ToInt64(row[lookupKey]);
                      }
                    }
                 }
            }
            catch (Exception ex)
            {
                  Console.WriteLine("Exception - " + ex.Message);
            }

            return requisitionId;
        }

        public string GetstringValueFromDataSet(string sqlQueryKey, string lookupKey)
        {
            string requisitionId = "";

            try
            {
                var sqlStatement = new StringBuilder(ConfigurationManager.AppSettings[sqlQueryKey]);

                var resultSet = ContextSqlConn.ExecuteDataSet(System.Data.CommandType.Text, sqlStatement.ToString());

                if (resultSet != null && resultSet.Tables.Count > 0)
                {
                    if (resultSet.Tables[0].Rows != null && resultSet.Tables[0].Rows.Count > 0)
                    {
                        foreach (System.Data.DataRow row in resultSet.Tables[0].Rows)
                        {
                            requisitionId = row[lookupKey].ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception - " + ex.Message);
            }

            return requisitionId;
        }
        public long GetRequisitionsForRiskScore(short riskType ,int? riskScore=null)
        {
            long requisitionId = 0;

            try
            {
                var sqlStatement = new StringBuilder(ConfigurationManager.AppSettings["RequisitionWithOutRiskScore"]);
                
                if(riskScore!=null && riskType>=1)
                {
                    sqlStatement = new StringBuilder(ConfigurationManager.AppSettings["RequisitionWithRiskScore"]);
                    sqlStatement.Replace("@RiskScore", riskScore.ToString());
                }
                var resultSet = ContextSqlConn.ExecuteDataSet(System.Data.CommandType.Text, sqlStatement.ToString());

                if (resultSet != null && resultSet.Tables.Count > 0)
                {
                    if (resultSet.Tables[0].Rows != null && resultSet.Tables[0].Rows.Count > 0)
                    {
                        foreach (System.Data.DataRow row in resultSet.Tables[0].Rows)
                        {
                            requisitionId = Convert.ToInt64(row["DocumentCode"]);
                        }
                    }
                }
                if (requisitionId>0 && riskType == 3)
                {
                    sqlStatement = new StringBuilder(ConfigurationManager.AppSettings["UpdateDocumentStatus"]);
                    sqlStatement.Replace("@RequisitionID", requisitionId.ToString());
                    resultSet = ContextSqlConn.ExecuteDataSet(System.Data.CommandType.Text, sqlStatement.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception - " + ex.Message);
            }

            return requisitionId;
        }

        public DataSet GetUserMappedORGEntities(int preferenceLOBType)
        {
            long lobEntitityDetailCode = 0;
            DataSet resultSet = null;

            try
            {
                var sqlStatement = new StringBuilder(ConfigurationManager.AppSettings["GetUserLOB"]);
                sqlStatement.Replace("@ContactCode", ConfigurationManager.AppSettings["ContactCode"]);
                sqlStatement.Replace("@PreferenceLOBType", preferenceLOBType.ToString());

                resultSet = ContextSqlConn.ExecuteDataSet(System.Data.CommandType.Text, sqlStatement.ToString());

                if (resultSet != null && resultSet.Tables.Count > 0)
                {
                    if (resultSet.Tables[0].Rows != null && resultSet.Tables[0].Rows.Count > 0)
                    {
                        foreach (System.Data.DataRow row in resultSet.Tables[0].Rows)
                        {
                            lobEntitityDetailCode = Convert.ToInt64(row["EntityDetailCode"]);
                        }
                    }
                }
                if (lobEntitityDetailCode > 0)
                {
                    resultSet = ContextSqlConn.ExecuteDataSet("usp_PRN_GetSelectedContactORGMapping", ConfigurationManager.AppSettings["ContactCode"], preferenceLOBType, lobEntitityDetailCode);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception - " + ex.Message);
            }

            return resultSet;
        }

        public List<long> GetRequisitionItemIdsBasedonRequisitionId(long documentCode)
        {
            DataSet resultSet = null;
            List<long> reqitemIds = new List<long>();
            try
            {

                var sqlStatement = new StringBuilder(ConfigurationManager.AppSettings["GetReqItemIds"]);
                sqlStatement.Replace("@RequisitionID", documentCode.ToString());
                resultSet = ContextSqlConn.ExecuteDataSet(System.Data.CommandType.Text, sqlStatement.ToString());
                if (resultSet != null && resultSet.Tables.Count > 0)
                {
                    if (resultSet.Tables[0].Rows != null && resultSet.Tables[0].Rows.Count > 0)
                    {
                        foreach (System.Data.DataRow row in resultSet.Tables[0].Rows)
                        {
                            reqitemIds.Add(Convert.ToInt64(row["RequisitionItemId"]));
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception -" + ex.Message);
            }
            return reqitemIds;
        }
    public string GetContractNumber()
    {
      string contractNumber = "";
      try
      {
        string sql = ConfigurationManager.AppSettings["GetextContractRef"].ToString();
        var data = ContextSqlConn.ExecuteDataSet(System.Data.CommandType.Text, sql);
        if (data != null && data.Tables.Count > 0)
        {
          if (data.Tables[0].Rows != null && data.Tables[0].Rows.Count > 0)
          {
            foreach (System.Data.DataRow row in data.Tables[0].Rows)
            {
              contractNumber = row["ExtContractRef"].ToString();
            }
          }
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine("Exception - " + ex.Message);
      }
      return contractNumber;
    }
  }
}