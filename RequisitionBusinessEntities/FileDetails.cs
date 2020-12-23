using System;
using System.Collections.Generic;

namespace GEP.Cumulus.P2P.Req.BusinessEntities
{
    public static class FileEnums
    {
        public enum FileStatus
        {
            None = 0,
            Queued = 1,
            Active = 2,
            InActive = 3,
            InProgress = 4,
            Completed = 5,
            Failed = 6
        }

        // Enum for defining access type for the blob
        public enum FileAccessType
        {
            Private = 0,
            Public = 1,
        }

        // Enum for defining the container base Uri
        public enum FileContainerType
        {
            None = 0,
            Applications = 1,
            Interfaces = 2,
            Internal = 3,
        }
    }

    /// <summary>
    /// This class contain details of FileDetails.
    /// </summary>
    public class FileDetails
    {
        /// <summary>
        /// Gets or sets the file id.
        /// </summary>
        public long FileId { get; set; }
        /// <summary>
        /// Gets or sets the internal file name.
        /// </summary>
        public string InternalFileName { get; set; }
        /// <summary>
        /// Gets or sets the file name.
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// Gets or sets the file extension.
        /// </summary>
        public string FileExtension { get; set; }
        /// <summary>
        /// Gets or sets the file content type.
        /// </summary>
        public string FileContentType { get; set; }
        /// <summary>
        /// Gets or sets the file uri.
        /// </summary>
        public string FileUri { get; set; }
        /// <summary>
        /// Gets or sets the file thumb url.
        /// </summary>
        public string FileThumbUrl { get; set; }
        /// <summary>
        /// Gets or sets the file size in bytes.
        /// </summary>
        public float FileSizeInBytes { get; set; }
        /// <summary>
        /// Gets or sets whether the file is zipped or not.
        /// </summary>
        public bool IsZipped { get; set; }
        /// <summary>
        /// Gets or sets whether the file is encrypted or not.
        /// </summary>
        public bool IsEncrypted { get; set; }
        /// <summary>
        /// Gets or sets a file encryption type.
        /// </summary>
        public string EncryptionType { get; set; }
        /// <summary>
        /// Gets or sets whether a file is deleted or not.
        /// </summary>
        public bool IsDeleted { get; set; }
        /// <summary>
        /// Gets or sets whether a file is searchable.
        /// </summary>
        public bool IsSearchable { get; set; }
        /// <summary>
        /// Gets or sets whether a file is indexed or not.
        /// </summary>
        public bool IsIndexed { get; set; }
        /// <summary>
        /// Gets or sets the file created date.
        /// </summary>
        public DateTime DateCreated { get; set; }
        /// <summary>
        /// Gets or sets the file updated date.
        /// </summary>
        public DateTime DateUpdated { get; set; }
        /// <summary>
        /// Gets or sets the file comment.
        /// </summary>
        public string FileComment { get; set; }
        /// <summary>
        /// Gets or sets the file created by.
        /// </summary>
        public long FileCreatedBy { get; set; }
        /// <summary>
        /// Gets or sets the file created by contact name.
        /// </summary>
        public string FileCreatedByContactName { get; set; }
        /// <summary>
        /// Gets or sets file status.
        /// </summary>
        public FileEnums.FileStatus FileStatus { get; set; }
        /// <summary>
        /// Gets or sets the file data in bytes.
        /// </summary>
        public byte[] FileData { get; set; }
        /// <summary>
        /// Gets or sets the file access type.
        /// </summary>
        public FileEnums.FileAccessType FileAccessType { get; set; }
        /// <summary>
        /// Gets or sets the file container type.
        /// </summary>
        public FileEnums.FileContainerType FileContainerType { get; set; }
        /// <summary>
        /// Gets or sets the source blob name.
        /// </summary>
        public string SourceBlobName { get; set; }
        /// <summary>
        /// Gets or sets whether a file is public or not.
        /// </summary>
        public bool IsPublic { get; set; }
        /// <summary>
        /// Gets or sets is announcement.
        /// </summary>
        public bool IsAnnouncement { get; set; }
        /// <summary>
        /// Gets or sets is thumbnail.
        /// </summary>
        public bool IsThumbnail { get; set; }
        /// <summary>
        /// Gets or sets exclude from encryption.
        /// </summary>
        public bool ExcludeFromEncryption { get; set; }
        /// <summary>
        /// Gets or sets internal file id's.
        /// </summary>
        public string InternalFileIds { get; set; }
        /// <summary>
        /// Gets or sets the contact code.
        /// </summary>
        public long ContactCode { get; set; }
        /// <summary>
        /// Gets or sets the document code.
        /// </summary>
        public long DocumentCode { get; set; }
        /// <summary>
        /// Gets or sets internal file results.
        /// </summary>
        public List<FileResults> InternalFileResults { get; set; }
        /// <summary>
        /// Gets or sets process exception.
        /// </summary>
        public Exception ProcessException { get; set; }
        /// <summary>
        /// Gets or sets version number.
        /// </summary>
        public int? VersionNumber { get; set; }
        /// <summary>
        /// Gets or sets the internal file names for validation.
        /// </summary>
        public ICollection<string> InternalFileNamesForValidation { get; set; }
        /// <summary>
        /// Gets or sets the file configuration details.
        /// </summary>
        public FileConfigurationSetting FileConfiguration { get; set; }
        /// <summary>
        /// Gets or sets the file request details id.
        /// </summary>
        public long FileRequestDetailsId { get; set; }
        /// <summary>
        /// Gets or sets the file created by.
        /// </summary>
        public long CreatedBy { get; set; }
        /// <summary>
        /// Gets or sets the company name.
        /// </summary>
        public string CompanyName { get; set; }
    }

    /// <summary>
    /// This class contains detail of file results class.
    /// </summary>
    public class FileResults
    {
        /// <summary>
        /// Gets or sets record name.
        /// </summary>
        public string RecordName { get; set; }
        /// <summary>
        /// Gets or sets record status.
        /// </summary>
        public string RecordStatus { get; set; }
        /// <summary>
        /// Gets or sets message.
        /// </summary>
        public string Message { get; set; }
    }

    /// <summary>
    /// This class contain details of FileConfigurationSetting.
    /// </summary>
    public class FileConfigurationSetting
    {
        /// <summary>
        /// Gets or sets the maximum size for attachment.
        /// </summary>
        public long MaxSizeForAttachment { get; set; }
        /// <summary>
        /// Gets or sets the maximum file number for attachment.
        /// </summary>
        public int MaxFileNoForAttachment { get; set; }
        /// <summary>
        /// Gets or sets the file extension for attachment.
        /// </summary>
        public string FileExtForAttachment { get; set; }
        /// <summary>
        /// Gets or sets whether the file extension is allowed or not.
        /// </summary>
        public bool IsAllowedExtension { get; set; }
    }

    public class AzureTableStorageConfiguration
    {
        //
        // Summary:
        //     Buyer Partner Code of a domain.
        public long BuyerPartnerCode { get; set; }
        //
        // Summary:
        //     Azure Storage Account Name.
        public string StorageAccountName { get; set; }
        //
        // Summary:
        //     Azure Storage Account Key.
        public string StorageAccountKey { get; set; }
    }

    public class DownloadFileDetailsModel
    {
        /// <summary>
        /// Unique File Id assigned to every new file.
        /// </summary>
        public long FileId { get; set; }
        /// <summary>
        /// Encrypted File Name.
        /// </summary>
        public string InternalFileName { get; set; }
        /// <summary>
        /// Name of the File.
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// Extension of the File.
        /// </summary>
        public string FileExtension { get; set; }
        /// <summary>
        /// Content Type of the File.
        /// </summary>
        public string FileContentType { get; set; }
        /// <summary>
        /// URI of the file in Blob Storage.
        /// </summary>
        public string FileUri { get; set; }
        /// <summary>
        /// Size of the File in Bytes.
        /// </summary>
        public float FileSizeInBytes { get; set; }
        /// <summary>
        /// Determines whether the file is deleted.
        /// </summary>
        public bool IsDeleted { get; set; }
        /// <summary>
        /// Determines whether the file is Searchable.
        /// </summary>
        public bool IsSearchable { get; set; }
        /// <summary>
        /// File created date.
        /// </summary>
        public DateTime DateCreated { get; set; }
        /// <summary>
        /// File updated date.
        /// </summary>
        public DateTime DateUpdated { get; set; }
        /// <summary>
        /// File Comments, if any.
        /// </summary>
        public string FileComment { get; set; }
        /// <summary>
        /// Uploaded file status.
        /// </summary>
        public FileEnums.FileStatus FileStatus { get; set; }
        /// <summary>
        /// File data in Byte Array.
        /// </summary>
        public byte[] FileData { get; set; }
        /// <summary>
        /// File created by.
        /// </summary>
        public long CreatedBy { get; set; }
    }

    public class FileUploadResponseModel
    {
        /// <summary>
        /// This is the internal Database id representing the file in the table FM_FileDetails
        /// </summary>
        public long FileId { get; set; }
        /// <summary>
        /// This is the File Name which can be used to display on UI. This is the same file name as recieved in the request model. 
        /// This file name will not include the GUID appended at the end before uploading to blob
        /// </summary>
        public string FileDisplayName { get; set; }
        /// <summary>
        /// This is the extension of the file in lower case.
        /// </summary>
        public string FileExtension { get; set; }
        /// <summary>
        /// Size of the uploded file in bytes
        /// </summary>
        public float FileSizeInBytes { get; set; }
        /// <summary>
        /// This is the contact code of the user who requested the file upload.
        /// </summary>
        public long FileCreatedBy { get; set; }


    }

    public class UploadFileToTargetBlobRequestModel
    {
        /// <summary>
        /// File Name indicates the actual file display name of the file to be uploaded.
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// This indicates the name of the root container in which the file should be uploaded. It should be ensured that this is an existing root container 
        /// in blob, access of which has been verified. If a new container name is set to this properity then a new container will be created 
        /// and file will be uploaded in the new container. However the file may not be downloadable from this new container due to access issues. 
        /// eg: buyersqlconn, publiccontainer, workbenchsqlconn etc
        /// </summary>
        public string TargetBlobRootContainerName { get; set; }
        /// <summary>
        /// This is an optional field which can be used to override the default folder name in which the file will be uploaded, within the root container.
        /// If not provided the default folder name is "Attachment"
        /// Example of overridden folder can be "Portal/Announcement"
        /// </summary>
        public string TargetBlobFolderPathWithinRootContainer { get; set; }
        /// <summary>
        /// This is the mime type of the file retrieved from file properties
        /// </summary>
        public string FileContentType { get; set; }
        /// <summary>
        /// Any additional remark about the file to be uploaded.
        /// </summary>
        public string FileComment { get; set; }
        /// <summary>
        /// The file itself converted to byte array
        /// </summary>
        public byte[] FileData { get; set; }
        /// <summary>
        /// This is the container type id which can be refered from the table FM_FileContainerType.
        /// It is used to fetch the corresponding file validation settings from the table FM_FileValidations
        /// </summary>
        public FileValidationSettingsRequestModel FileValidationSettingsRequestModel { get; set; }


    }

    public class MoveFileToTargetBlobFileValidationSettings
    {
        public int FileValidationSettingsScope { get; set; }// = DataAccess.Enum.FileValidationSettingsScope.Global,
        public int FileValidationContainerTypeId { get; set; }
    }
    public class MoveFileToTargetBlobRequest
    {
        public string FileName { get; set; }
        public string FileContentType { get; set; }
        public string TemporaryBlobFileUri { get; set; }
        public MoveFileToTargetBlobFileValidationSettings FileValidationSettings { get; set; }
    }

    public class FileValidationSettingsRequestModel
    {
        /// <summary>
        /// This specifies whether to use 'Global' validation settings defined in FM_FileValidations for validating the file being uploaded 
        /// or to use the product specific file validation settings defined in FM_FileConfigurations.
        /// </summary>
        public FileValidationSettingsScope FileValidationSettingsScope { get; set; }

        /// <summary>
        /// This is the container type id which can be refered from the table FM_FileContainerType.
        /// It is used to fetch the corresponding global scope file validation settings from the table 
        /// FM_FileValidations. This is required to be provided only if FileValidationSettingsScope = Global
        /// </summary>
        public int FileValidationContainerTypeId { get; set; }

        /// <summary>
        /// This is the product code which can be refered from the table 'TODO'.
        /// This along with ObjectType is used to fetch the corresponding product scoped file validation settings 
        /// from the table FM_FileConfigurations. This is required to be provided only if 
        /// FileValidationSettingsScope = ProductSpecific
        /// </summary>
        public int SubAppCode { get; set; }

        /// <summary>
        /// This is used to identify a specific use case or functionality within a product.
        /// This along with SubAppCode is used to fetch the corresponding product scoped file validation settings 
        /// from the table FM_FileConfigurations. This is required to be provided only if 
        /// FileValidationSettingsScope = ProductSpecific
        /// </summary>
        public string ObjectType { get; set; }

    }

    //TODO: Move to its own physical file
    public enum FileValidationSettingsScope
    {
        Global = 1,
        ProductSpecific = 2
    }

    public static class FileManagerOLOC
    {
        public const string OLOCValue = "288";
    }
}