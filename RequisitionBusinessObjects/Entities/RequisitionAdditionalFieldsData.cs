using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace GEP.Cumulus.P2P.Req.BusinessObjects.Entities
{
  [DataContract]
  public class RequisitionAdditionalFieldsResponse
  {
    [DataMember]
    public long RequisitionID { get; set; }
    [DataMember]
    public long RequisitionItemID { get; set; }
    [DataMember]
    public int AdditionalFieldID { get; set; }
    [DataMember]
    public string AdditionalFieldDisplayName { get; set; }
    [DataMember]
    public string AdditionalFieldCode { get; set; }
    [DataMember]
    public string AdditionalFieldValue { get; set; }
    [DataMember]
    public long AdditionalFieldDetailCode { get; set; }
    [DataMember]
    public int FeatureId { get; set; }
    [DataMember]
    public byte LevelType { get; set; }
    [DataMember]
    public byte FieldControlType { get; set; }
    [DataMember]
    public long P2PLineItemID { get; set; }
    [DataMember]
    public byte DataDisplayStyle { get; set; }
    [DataMember]
    public string FlipDocumentTypes { get; set; }
    [DataMember]
    public string DocumentSpecification { get; set; }
    [DataMember]
    public int SourceDocumentTypeId { get; set; }
  }
  public class RequisitionAdditionalFieldsRequest
  {
    [DataMember]
    public long RequisitionID { get; set; }
    [DataMember]
    public int LevelType { get; set; }
    [DataMember]
    public int AdditionalFieldID { get; set; }
    [DataMember]
    public string RequisitionItemIDs { get; set; }
    [DataMember]
    public int FlipDocumentType { get; set; }
  }
  }
