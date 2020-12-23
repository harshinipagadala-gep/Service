using Gep.Cumulus.CSM.Entities;
using Gep.Cumulus.Encryption;
using GEP.Cumulus.Logging;
using GEP.SMART.Configuration;
using log4net;
//using GEP.SMART.Security.ClaimsManagerNet;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GEP.Cumulus.P2P.Req.BusinessObjects
{
    [ExcludeFromCodeCoverage]
    public class EncryptionHelper
    {
        private static readonly ILog Log = Logger.GetLog(MethodBase.GetCurrentMethod().DeclaringType);

        //TODO Remove Once authorization is implemented
        // Code Snippet 1
        private static string GetVector(long contactcode)
        {
            return contactcode.ToString().PadRight(16, '0');
        }

        // Code Snippet 2
        private static string GetVectorForNotLoggedInUser()
        {
            return MultiRegionConfig.GetConfig(CloudConfig.AESIV);
        }

        private static string GetAESKey(long? contactCode = 0)
        {
            //TODO remove Once authorization is implemented

            if (contactCode != null && contactCode > 0)
                return GetVector(Convert.ToInt64(contactCode));
            else
                return GetVectorForNotLoggedInUser();

            //TODO Uncomment Once authorization is implemented
            //    if (SmartClaimsManager.IsAuthenticated())
            //        AESIV = GetVector();
            //    else
            //        AESIV = GetVectorForNotLoggedInUser();

        }

        /// <summary>
        /// This Function is used to encrypted file id using EncryptWithAESandURLEncode from Gep.Cumulus.Encryption.
        /// </summary>
        public static Func<long?, long?, string> Encrypt = (fileId, contactCode) =>
        {
            string AESPrivateKey;
            string AESIV;


            try
            {

                AESPrivateKey = MultiRegionConfig.GetConfig(CloudConfig.AESPrivateKey);
                AESIV = GetAESKey(contactCode);
            }

            catch (Exception ex)
            {                
                LogHelper.LogError(Log, "Error while fetching AES keys from MultiRegion.", ex);
                throw new Exception("Error occurs while encrypting.");
            }

            try
            {
                string encryptedFileId = "";
                if (fileId != null && fileId > 0)
                    encryptedFileId = SmartUrlEncryptionHelper.EncryptWithAESandURLEncode(fileId.ToString(), AESPrivateKey, AESIV);
                else
                    encryptedFileId = "";
                return encryptedFileId;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, $"Error while encrypting the file id - {fileId}. Error Message = {ex.Message}", ex);
                throw new Exception("Error occurs while encrypting fileId.");
            }
        };

        /// <summary>
        /// This Function is used to decrypted file id using DecryptWithAESandURLDecode from Gep.Cumulus.Encryption.
        /// </summary>
        public static Func<string, long?, long> Decrypt = (fileId, contactCode) =>
        {
            string AESPrivateKey;
            string AESIV;
            string decryptedStringFileId;

            try
            {


                AESPrivateKey = MultiRegionConfig.GetConfig(CloudConfig.AESPrivateKey);
                AESIV = GetAESKey(contactCode);

            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, $"Error while fetching AES keys from MultiRegion.", ex);
                throw new Exception("Error occurs while decrypting");
            }

            try
            {
                if (!string.IsNullOrEmpty(fileId))
                    decryptedStringFileId = SmartUrlEncryptionHelper.DecryptWithAESandURLDecode(fileId, AESPrivateKey, AESIV);
                else
                    decryptedStringFileId = "0";
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, $"Error while decrypting the file id - {fileId}. Error Message = {ex.Message}", ex);                
                throw new Exception("Error occurs while decrypting fileId");                
            }

            try
            {
                long decryptedFileId = Convert.ToInt64(decryptedStringFileId);
                return decryptedFileId;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, $"File Id is not in correct format and could not be converted to Integer - {fileId}", ex);
                throw new Exception("Error occurs while decrypting the fileId");
            }
        };
    }
}