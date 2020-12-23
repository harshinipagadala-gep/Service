using Gep.Cumulus.CSM.Entities;
using Gep.Cumulus.CSM.Extensions;
using Gep.Cumulus.ExceptionManager;
using GEP.Cumulus.Logging;
using GEP.Cumulus.P2P.BusinessObjects;
using GEP.Cumulus.Web.Utils;
using log4net;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Web.Script.Serialization;
using SMARTFaultException = Gep.Cumulus.ExceptionManager;
using WorkFlowRestService = GEP.Cumulus.Workflow.RestServiceContracts;
namespace GEP.Cumulus.P2P.Req.BusinessObjects.Proxy
{
    [ExcludeFromCodeCoverage]
    internal class ProxyWorkFlowRestService : RequisitionBaseProxy
    {
        #region "Variables"
        public GepServiceFactory GepServices = GepServiceManager.GetInstance;
        private HttpWebRequest req = null;
        private UserExecutionContext UserExecutionContext = null;

        private static readonly ILog log = Logger.GetLog(MethodBase.GetCurrentMethod().DeclaringType);
        private string appName = System.Environment.GetEnvironmentVariable("NewRelic.AppName") ?? "Requisition";
        public ProxyWorkFlowRestService(UserExecutionContext userExecutionContext, string jwtToken): base(userExecutionContext, jwtToken)
        {
            this.UserExecutionContext = GetUserExecutionContext();
        }
        public interface IWorkFlowRestServiceChannel : WorkFlowRestService.IWorkflowRestService, IClientChannel
        {

        }
        #endregion
        private void CreateHttpWebRequest(string strURL)
        {
            req = WebRequest.Create(strURL) as HttpWebRequest;
            req.Method = "POST";
            req.ContentType = @"application/json";

            NameValueCollection nameValueCollection = new NameValueCollection();
            //userExecutionContext.UserName = "";
            string userName = UserExecutionContext.UserName;
            string clientName = UserExecutionContext.ClientName;
            UserExecutionContext.UserName = string.Empty;
            UserExecutionContext.ClientName = string.Empty;
            string userContextJson = UserExecutionContext.ToJSON();
            nameValueCollection.Add("UserExecutionContext", userContextJson);
            nameValueCollection.Add("Authorization", this.GetToken());

            req.Headers.Add(nameValueCollection);
            UserExecutionContext.UserName = userName;
            UserExecutionContext.ClientName = clientName;
        }

        private string GetHttpWebResponse(Dictionary<string, object> odict)
        {
            JavaScriptSerializer JSrz = new JavaScriptSerializer();
            var data = JSrz.Serialize(odict);
            var byteData = Encoding.UTF8.GetBytes(data);


            req.ContentLength = byteData.Length;
            using (Stream stream = req.GetRequestStream())
            {
                stream.Write(byteData, 0, byteData.Length);
            }

            string result = null;
            using (HttpWebResponse resp = req.GetResponse() as HttpWebResponse)
            {
                using (StreamReader reader = new StreamReader(resp.GetResponseStream()))
                {
                    result = reader.ReadToEnd();
                }
            }
            return result;
        }
        public bool WithDrawDocumentByDocumentCode(int DocumentTypeID, long DocumentCode, long UserID)
        {
            bool ISSucess = false;
            string result = string.Empty;
            string serviceurl = string.Empty;
            GEPBaseProxy gepBaseProxy = new GEPBaseProxy();            
            try
            {
                serviceurl = UrlHelperExtensions.WorkFlowRestUrl + "/WithdrawDocument";
                CreateHttpWebRequest(serviceurl);
                Dictionary<string, object> odict = new Dictionary<string, object>();
                odict.Add("wfDocTypeId", DocumentTypeID);
                odict.Add("documentCode", DocumentCode);
                odict.Add("userId", UserID);

                result = GetHttpWebResponse(odict);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(log, "Error occured in WithDrawDocumentByDocumentCode Method", ex);
                var objCustomFault = new CustomFault(ex.Message, "WithDrawDocumentByDocumentCode", "WithDrawDocumentByDocumentCode", "AppStartBO", ExceptionType.ApplicationException, "", false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while WithDrawDocumentByDocumentCode");
            }
            finally
            {
            }
            return ISSucess;
        }
    }
}
