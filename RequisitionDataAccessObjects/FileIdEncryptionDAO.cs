using Gep.Cumulus.CSM.Entities;
using Gep.Cumulus.Encryption;
using GEP.SMART.Configuration;
using System;
using System.Diagnostics.CodeAnalysis;

namespace GEP.Cumulus.P2P.Req.DataAccessObjects
{
    [ExcludeFromCodeCoverage]
    public class FileIdEncryptionDAO
    {

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
                throw new Exception($"Error while fetching AES keys from MultiRegion.", ex);
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
                throw new Exception($"Error while encrypting the file id - {fileId}. Error Message = {ex.Message}", ex);
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
                throw new Exception($"Error while fetching AES keys from MultiRegion.", ex);
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
                throw new Exception($"Error while decrypting the file id - {fileId}. Error Message = {ex.Message}", ex);
            }

            try
            {
                long decryptedFileId = Convert.ToInt64(decryptedStringFileId);
                return decryptedFileId;
            }
            catch (Exception ex)
            {
                throw new Exception($"File Id is not in correct format and could not be converted to Integer - {fileId}", ex);
            }
        };
    }
}