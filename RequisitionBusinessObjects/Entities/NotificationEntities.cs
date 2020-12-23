using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEP.Cumulus.P2P.Req.BusinessObjects.Entities
{
    public class MailAdressInfo
    {
        public long ContactCode { get; set; }
        public string EmailAddress { get; set; }
        public string FaxNumber { get; set; }
    }

    public class EmailAdressInfo
    {
        public EmailAdressInfo(bool isSMARTRegisteredUser, long contactCode, string emailAddress)
        {
            IsSMARTRegisteredUser = isSMARTRegisteredUser;
            ContactCode = contactCode;
            EmailAddress = emailAddress;
        }
        public bool IsSMARTRegisteredUser { get; }
        public long ContactCode { get; }
        public string EmailAddress { get; }
    }

    public class EmailAttachmentInfo
    {
        public EmailAttachmentInfo(long fileId, string filePath)
        {
            FileId = fileId;
            FilePath = filePath;
        }
        public string FilePath { get; }
        public string FileName { get; set; }
        public long FileId { get; }
    }

    public class EmailMessageInfo
    {
        public EmailMessageInfo(string emailNotificationGUID, string eventCode, string referenceType, List<EmailAdressInfo> to)
        {
            RequestGUID = Guid.NewGuid().ToString();
            EmailNotificationGUID = emailNotificationGUID;
            EventCode = eventCode;
            ReferenceType = referenceType;
            To = to;
        }
        public string RequestGUID { get; private set; }
        public string EmailNotificationGUID { get; private set; }
        public string ReferenceType { get; private set; }
        public string EventCode { get; private set; }
        public long NotificationTemplateID { get; set; }
        public long NotificationId { get; set; }
        public long EmailMessageTemplateId { get; set; }
        public IEnumerable<EmailAdressInfo> To { get; private set; }
        public IList<EmailAdressInfo> CC { get; set; }
        public IList<EmailAdressInfo> Bcc { get; set; }
        public SortedList<string, string> EmailMessageTemplateFieldValues { get; set; }
        public List<EmailAttachmentInfo> Attachments { get; set; }
        public string CustomMessageBody { get; set; }
        public bool IsHtmlBody { get; set; }
        public string Subject { get; set; }
        public long ReferenceCode { get; set; }
    }
}
