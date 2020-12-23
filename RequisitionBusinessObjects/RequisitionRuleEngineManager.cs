using Gep.Cumulus.CSM.Entities;
using GEP.Cumulus.Logging;
using GEP.Cumulus.P2P.BusinessEntities;
using GEP.Cumulus.P2P.Req.BusinessObjects.Entities;
using log4net;
using Newtonsoft.Json;
using REDataModel;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace GEP.Cumulus.P2P.Req.BusinessObjects
{
    [ExcludeFromCodeCoverage]
    public class RequisitionRuleEngineManager : RequisitionBaseBO
    {
        private static readonly ILog Log = Logger.GetLog(MethodBase.GetCurrentMethod().DeclaringType);

        public RequisitionRuleEngineManager(string jwtToken) : base(jwtToken)
        {

        }

        public bool CheckHasRules(BusinessCase businessCase)
        {
            return GetSQLP2PDocumentDAO().CheckHasRules(businessCase, (int)P2PDocumentType.Requisition);
        }

        public bool EnableQuickQuoteRuleCheck(long documentCode)
        {
            bool result = false;
            try
            {
                List<RuleAction> actions = null;
                Requisition objReq = null;

                bool hasRules = CheckHasRules(BusinessCase.RequisitionItemRFXFlipTypeCheck);

                if (hasRules)
                {
                    UserExecutionContext userExecutionContext = UserContext;
                    objReq = GetReqDao().GetAllRequisitionDetailsByRequisitionId(documentCode, UserContext.ContactCode, 0);

                    RequisitionCommonManager commonManager = new RequisitionCommonManager(JWTToken) { GepConfiguration = GepConfiguration, UserContext = UserContext };
                    SettingDetails invSettings = commonManager.GetSettingsFromSettingsComponent(P2PDocumentType.Invoice, UserContext.ContactCode, 107);
                    bool IsREOptimizationEnabled = Convert.ToBoolean(commonManager.GetSettingsValueByKey(invSettings, "IsREOptimizationEnabled"));

                    actions = GetCommonDao().EvaluateRulesForObject(BusinessCase.RequisitionItemRFXFlipTypeCheck, objReq, IsREOptimizationEnabled);
                    Dictionary<string, int> dictRFXFlipType = new Dictionary<string, int>()
                    {
                        { "RFX Not Needed".ToLower(),    0 },
                        { "Quick RFx".ToLower(),    1 },
                        { "Standard RFx".ToLower(), 2 }
                    };

                    actions.ForEach(action =>
                    {
                        dynamic keyResult = JsonConvert.DeserializeObject(action.KeyResult);
                        List<string> keys = keyResult.ToObject<List<string>>();
                        keys.ForEach(strKey =>
                        {
                            int key = Convert.ToInt32(strKey);
                            RequisitionItem reqItem = objReq.RequisitionItems.Where(item => item.ItemLineNumber == key).FirstOrDefault();

                            List<ParameterOutput> parameters = new List<ParameterOutput>();
                            parameters = JsonConvert.DeserializeObject<List<ParameterOutput>>(action.Parameters);

                            if (parameters != null && parameters.Count > 0 && reqItem != null)
                            {
                                ParameterOutput parameterOutput = (from p in parameters where p.Name.Trim().Equals("ErrorAction") select p).FirstOrDefault();
                                string parameterValue = "";

                                if (parameterOutput != null)
                                {
                                    parameterValue = (from p in parameters where p.Name.Trim().Equals("ErrorAction") select p).FirstOrDefault().Value.Trim();
                                    reqItem.RFXFlipType = dictRFXFlipType[parameterValue.ToLower()];  
                                }
                            }

                        });
                    });

                    bool isUpdateSuccess = GetNewReqDao().UpdateRequisitionItemFlipType(objReq);

                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in EnableQuickQuoteRuleCheck Method in RequisitionRuleEngineManager", ex);
            }

            return result;
        }
    }
}
