using GEP.Cumulus.DocumentIntegration.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using S2C.Integration.Flip.Nuget;

namespace GEP.Cumulus.P2P.Req.BusinessObjects.Entities
{
  [DataContract]
  public class RfxFlipData
  {
    [DataMember]
    public DocumentIntegrationEntity documentIntegrationEntity;
    [DataMember]
    public List<PriceSheetDIO> priceSheet;
  }
}
