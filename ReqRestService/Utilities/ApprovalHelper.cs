using Gep.Cumulus.CSM.Entities;
using Gep.Cumulus.CSM.Extensions;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;
namespace GEP.Cumulus.P2P.Req.RestService
{
    public class ApprovalHelper
    {
        private HttpWebRequest req = null;
        public UserExecutionContext userExecutionContext { get; set; }


        public void CreateHttpWebRequest(string strURL, string jwtToken)
        {
            req = WebRequest.Create(strURL) as HttpWebRequest;
            req.Method = "POST";
            req.ContentType = @"application/json";

            NameValueCollection nameValueCollection = new NameValueCollection();
            string userName = userExecutionContext.UserName;
            string clientName = userExecutionContext.ClientName;
            userExecutionContext.UserName = string.Empty;
            userExecutionContext.ClientName = string.Empty;
            string userContextJson = userExecutionContext.ToJSON();
            nameValueCollection.Add("UserExecutionContext", userContextJson);            
            nameValueCollection.Add("Authorization", jwtToken);
            req.Headers.Add(nameValueCollection);
            userExecutionContext.UserName = userName;
            userExecutionContext.ClientName = clientName;

        }

        public string GetHttpWebResponse(Dictionary<string, object> odict)
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

    }
}
