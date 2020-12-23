using System.Diagnostics.CodeAnalysis;

namespace GEP.Cumulus.P2P.Req.BusinessObjects
{
    [ExcludeFromCodeCoverage]
    public class KeyValue
    {    
        public long Id { get; set; }
        
        public string Value { get; set; }
        
        public string Name { get; set; }
        
        public bool IsDefault { get; set; }
    }
  
}
