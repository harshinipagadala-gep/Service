using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEP.Cumulus.P2P.Req.BusinessObjects.Entities
{
   public class PartnerCurrencies
    {     
            public long PartnerCode { get; set; }
            public List<CurrencyCodeName> Currency;       
    }

    public class CurrencyCodeName
    {
        public string CurrencyName { get; set; }

        public string CurrencyCode { get; set; }
    }
}
