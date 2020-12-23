using Gep.Cumulus.CSM.Entities;
using Gep.Cumulus.ExceptionManager;
using GEP.Cumulus.Logging;
using GEP.Cumulus.QuestionBank.Entities;
using GEP.Cumulus.QuestionBank.ServiceContracts;
using GEP.Cumulus.Web.Utils;
using GEP.SMART.Configuration;
using log4net;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.ServiceModel;

namespace GEP.Cumulus.P2P.Req.BusinessObjects.Proxy
{
    [ExcludeFromCodeCoverage]
    public class ProxySurveyComponent : RequisitionBaseProxy
    {
        public GepServiceFactory GepServices = GepServiceManager.GetInstance;        
        private IQuestionBankServiceChannel objQuestionBankServiceChannel = null;
        private static readonly ILog log = Logger.GetLog(MethodBase.GetCurrentMethod().DeclaringType);
        private OperationContextScope scope = null;

        private string appName = System.Environment.GetEnvironmentVariable("NewRelic.AppName") ?? "Requisition";

        private UserExecutionContext UserExecutionContext { get; set; }
        public ProxySurveyComponent(UserExecutionContext userExecutionContext, string jwtToken): base (userExecutionContext, jwtToken)
        {
            this.UserExecutionContext = GetUserExecutionContext();
        }
        public interface IQuestionBankServiceChannel : IQuestionBank, IClientChannel
        {
        }

        public List<Question> GetMandatoryQuestionWithNoResponse(string questionSetCodes, string questionIds, long assessorId, long assesseeId, long objectInstanceId, AssessorUserType assessorType, string strCompanyName = "", bool validateScore = false)
        {
            List<Question> lstQuestion = new List<Question>();

            try
            {
                objQuestionBankServiceChannel = ConfigureChannel<IQuestionBankServiceChannel>(CloudConfig.QuestionBankServiceURL, UserExecutionContext, ref scope);

                MessageHeader<string> objMhgAuth = new MessageHeader<string>(GetToken());
                System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
                OperationContext.Current.OutgoingMessageHeaders.Add(authorization);

                lstQuestion = objQuestionBankServiceChannel.GetMandatoryQuestionWithNoResponse(questionSetCodes, questionIds, assessorId, assesseeId, objectInstanceId, assessorType, strCompanyName, validateScore);

            }
            catch (GEPCustomException ex)
            {
                LogHelper.LogError(log, "Error occured in GetMandatoryQuestionWithNoResponse Method", ex);
            }
            finally
            {
                GEPServiceManager.DisposeService(objQuestionBankServiceChannel, scope);
            }



            return lstQuestion;
        }

        public List<Question> GetQuestionWithResponsesByQuestionSetPaging(Question objQuestion, long assessorId, long assesseeId, long objectInstanceId, AssessorUserType assessorType, bool bloadQuestionScoreResponse)
        {
            List<Question> lstQuestion = new List<Question>();

            try
            {
                objQuestionBankServiceChannel = ConfigureChannel<IQuestionBankServiceChannel>(CloudConfig.QuestionBankServiceURL, UserExecutionContext, ref scope);

                MessageHeader<string> objMhgAuth = new MessageHeader<string>(GetToken());
                System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
                OperationContext.Current.OutgoingMessageHeaders.Add(authorization);

                lstQuestion = objQuestionBankServiceChannel.GetQuestionWithResponsesByQuestionSetPaging(objQuestion, assesseeId, assesseeId, objectInstanceId, assessorType, bloadQuestionScoreResponse);

            }
            catch (GEPCustomException ex)
            {
                LogHelper.LogError(log, "Error occured in GetPageFieldsWithModifiers Method in CSMProxy", ex);
            }
            finally
            {
                GEPServiceManager.DisposeService(objQuestionBankServiceChannel, scope);
            }



            return lstQuestion;
        }

        public void SaveQuestionsResponse(List<QuestionResponse> lstQuestionsResponse, long DocumentCode, int DocTypeCode)
        {
            try
            {
                objQuestionBankServiceChannel = ConfigureChannel<IQuestionBankServiceChannel>(CloudConfig.QuestionBankServiceURL, UserExecutionContext, ref scope);

                MessageHeader<string> objMhgAuth = new MessageHeader<string>(GetToken());
                System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
                OperationContext.Current.OutgoingMessageHeaders.Add(authorization);

                objQuestionBankServiceChannel.SaveQuestionsResponse(lstQuestionsResponse, DocumentCode, DocTypeCode);

            }
            catch (GEPCustomException ex)
            {
                LogHelper.LogError(log, "Error occured in SaveQuestionsResponse Method in ProxySurveyComponent", ex);
            }
            finally
            {
                GEPServiceManager.DisposeService(objQuestionBankServiceChannel, scope);
            }
        }

        public List<CustomDBLookUpQuestionData> GetCustomDBLookUpQuestionData(DBLookUpFieldConfig DBLookUpFieldConfig)
        {
            List<CustomDBLookUpQuestionData> lstCustomDBLookUpQuestionData = null;

            try
            {
                objQuestionBankServiceChannel = ConfigureChannel<IQuestionBankServiceChannel>(CloudConfig.QuestionBankServiceURL, UserExecutionContext, ref scope);

                MessageHeader<string> objMhgAuth = new MessageHeader<string>(GetToken());
                System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
                OperationContext.Current.OutgoingMessageHeaders.Add(authorization);

                lstCustomDBLookUpQuestionData = objQuestionBankServiceChannel.GetCustomDBLookUpQuestionData(DBLookUpFieldConfig);

            }
            catch (GEPCustomException ex)
            {
                LogHelper.LogError(log, "Error occured in GetCustomDBLookUpQuestionData Method in ProxySurveyComponent", ex);
            }
            finally
            {
                GEPServiceManager.DisposeService(objQuestionBankServiceChannel, scope);
            }
            return lstCustomDBLookUpQuestionData;
        }
    }
}
