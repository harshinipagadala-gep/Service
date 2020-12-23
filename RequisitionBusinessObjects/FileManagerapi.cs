using Gep.Cumulus.CSM.Entities;
using GEP.NewP2PEntities.FileManagerEntities;
using GEP.SMART.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace GEP.Cumulus.P2P.Req.BusinessObjects
{
    [ExcludeFromCodeCoverage]
    internal class FileManagerApi
    {
        private UserExecutionContext UserExecutionContext = null;
        private string BaseUrl = MultiRegionConfig.GetConfig(CloudConfig.SmartBaseURL);
        protected static string blobPath = MultiRegionConfig.GetConfig(CloudConfig.BlobURL).Trim('/');
        private int timeout = 50000;
        private string JWTToken;


        public FileManagerApi(UserExecutionContext UserExecutionContext, string jwtToken)
        {
            this.UserExecutionContext = UserExecutionContext;
            this.JWTToken = jwtToken;
        }
        public DownloadFileDetailsModel GetFileDetailsbyFileId(long fileid)
        {
            DownloadFileDetailsModel fileResult = null;
            try
            {

                string url = BaseUrl + URLs.GetFileDetailsByFileId + fileid + "?oloc=" + FileManagerOLOC.OLOCValue;
                var requestHeaders = new RESTAPIHelper.RequestHeaders();
                requestHeaders.Set(this.UserExecutionContext, this.JWTToken);
                var webapi = new RESTAPIHelper.WebAPI(requestHeaders, "DEV_APAC_Dss_Req_Microservice_NEWREQ", "GetFileDetailsbyFileId");
                var response = webapi.ExecuteGet(url);
                fileResult = JsonConvert.DeserializeObject<DownloadFileDetailsModel>(response);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return fileResult;
        }

        public byte[] DownloadFilebyFileUri(string fileUri)
        {
            byte[] fileResult = null;
            try
            {
                string url = BaseUrl + URLs.DownloadFileByFileUri + fileUri + "&oloc=" + FileManagerOLOC.OLOCValue;
                var requestHeaders = new RESTAPIHelper.RequestHeaders();
                requestHeaders.Set(this.UserExecutionContext, this.JWTToken);
                var webapi = new RESTAPIHelper.WebAPI(requestHeaders, "DEV_APAC_Dss_Req_Microservice_NEWREQ", "DownloadFilebyFileUri");
                var response = webapi.ExecuteGet(url);
                fileResult = Convert.FromBase64String(response.Replace("\"", string.Empty));

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return fileResult;
        }

        public DownloadFileDetailsModel DownloadFileByFileId(long fileId)
        {
            DownloadFileDetailsModel downloadFileDetailsModel = null;
            try
            {
                string url = BaseUrl + URLs.GetFileDetailsByFileId + fileId + "&oloc=" + FileManagerOLOC.OLOCValue;
                var requestHeaders = new RESTAPIHelper.RequestHeaders();
                requestHeaders.Set(this.UserExecutionContext, this.JWTToken);
                var webapi = new RESTAPIHelper.WebAPI(requestHeaders, "DEV_APAC_Dss_Req_Microservice_NEWREQ", "DownloadFileByFileId");
                var response = webapi.ExecuteGet(url);
                downloadFileDetailsModel = JsonConvert.DeserializeObject<DownloadFileDetailsModel>(response);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return downloadFileDetailsModel;
        }

        public FileUploadResponseModel UploadFileToBlobContainer(UploadFileToTargetBlobRequestModel requestModel)
        {
            FileUploadResponseModel fileResult = null;
            try
            {
                string url = BaseUrl + URLs.UploadFileToTargetBlob + "?oloc=" + FileManagerOLOC.OLOCValue;
                var requestHeaders = new RESTAPIHelper.RequestHeaders();
                requestHeaders.Set(this.UserExecutionContext, this.JWTToken);
                var webapi = new RESTAPIHelper.WebAPI(requestHeaders, "DEV_APAC_Dss_Req_Microservice_NEWREQ", "UploadFileToBlobContainer");
                var response = webapi.ExecutePost(url, requestModel);
                fileResult = JsonConvert.DeserializeObject<FileUploadResponseModel>(response);

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return fileResult;
        }

        public FileDetails DownloadFilefromBlob(string pFileName, string pFileUrl)
        {
            string fileUri = blobPath + "/" + pFileUrl + pFileName;
            byte[] bFileArray = DownloadFilebyFileUri(fileUri);
            if (bFileArray != null)
            {
                var objFiledetails = new FileDetails();
                objFiledetails.FileData = bFileArray;
                return objFiledetails;
            }

            return null;
        }

        public string GetFileUriByFileId(long fileid)
        {
            string result = string.Empty;
            try
            {
                string url = BaseUrl + URLs.GetFileUriByFileId + fileid + "?oloc=" + FileManagerOLOC.OLOCValue;
                var requestHeaders = new RESTAPIHelper.RequestHeaders();
                requestHeaders.Set(this.UserExecutionContext, this.JWTToken);
                var webapi = new RESTAPIHelper.WebAPI(requestHeaders, "DEV_APAC_Dss_Req_Microservice_NEWREQ", "GetFileDetailsbyFileId");
                result = webapi.ExecuteGet(url);
                result = result.Replace("\"", string.Empty);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }

        public AzureTableStorageConfiguration GetTableStorageConfiguration(long buyerPartnerCode, string storageType = "WEB")
        {
            AzureTableStorageConfiguration configurationResult = null;
            try
            {
                string url = BaseUrl + URLs.GetTableStorageConfiguration + "?oloc=" + FileManagerOLOC.OLOCValue;
                var requestHeaders = new RESTAPIHelper.RequestHeaders();
                requestHeaders.Set(this.UserExecutionContext, this.JWTToken);
                var requestModel = new Dictionary<string, object>() {
                    { "buyerPartnerCode", buyerPartnerCode },
                    { "storageType", storageType }
                };
                var webapi = new RESTAPIHelper.WebAPI(requestHeaders, "DEV_APAC_Dss_Req_Microservice_NEWREQ", "GetTableStorageConfiguration");
                var response = webapi.ExecutePost(url, requestModel);
                configurationResult = JsonConvert.DeserializeObject<AzureTableStorageConfiguration>(response);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return configurationResult;
        }
    }
}
