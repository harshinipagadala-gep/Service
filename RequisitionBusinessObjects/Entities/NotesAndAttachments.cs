using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEP.Cumulus.P2P.Req.BusinessObjects.Entities
{
    public class ReqNotesOrAttachments
    {
        public byte DocumentType { get; set; }
        public string FilePath { get; set; }
        public string CategoryTypeName { get; set; }
        public decimal FileSize { get; set; }
        public DateTime ModifiedDate { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime DateCreated { get; set; }
        public string CreatorName { get; set; }
        public long CreatedBy { get; set; }
        public int CategoryTypeId { get; set; }
        public bool IsEditable { get; set; }
        public GEP.Cumulus.P2P.BusinessEntities.SourceType SourceType { get; set; }
        public GEP.Cumulus.P2P.BusinessEntities.NoteOrAttachmentAccessType AccessTypeId { get; set; }
        public string NoteOrAttachmentTypeName { get; set; }
        public GEP.Cumulus.P2P.BusinessEntities.NoteOrAttachmentType NoteOrAttachmentType { get; set; }
        public string NoteOrAttachmentDescription { get; set; }
        public string NoteOrAttachmentName { get; set; }
        public long? FileId { get; set; }
        public long LineItemId { get; set; }
        public long DocumentCode { get; set; }
        public long NotesOrAttachmentId { get; set; }
        public long P2PLineItemID { get; set; }
        public string FileUri { get; set; }
        public string EncryptedFileId { get; set; }
    }

}
