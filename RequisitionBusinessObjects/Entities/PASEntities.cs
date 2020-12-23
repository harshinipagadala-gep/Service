using Gep.Cumulus.CSM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEP.Cumulus.P2P.Req.BusinessObjects.Entities
{
    public class PASMaster : EntityBase
    {
        public bool IsHidden { get; set; }
        public bool UnSelectable { get; set; }
        public List<PASMaster> ChildNodes { get; set; }
        public int PASLevel { get; set; }
        public long ContactCode { get; set; }
        public bool PASStatus { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }
        public bool IsDirect { get; set; }
        public int UNSPSCKey { get; set; }
        public string ClientPASCode { get; set; }
        public long GPNPASCode { get; set; }
        public int ChildCount { get; set; }
        public int VersionId { get; set; }
        public long MappedPASCode { get; set; }
        public long ParentPASCode { get; set; }
        public long PartnerCode { get; set; }
        public string PASName { get; set; }
        public long PASCode { get; set; }
        public bool IsDefault { get; set; }
        public bool IsSelected { get; set; }
    }
}
