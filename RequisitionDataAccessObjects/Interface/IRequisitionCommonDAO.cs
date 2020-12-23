using Gep.Cumulus.CSM.BaseDataAccessObjects;
using Gep.Cumulus.Partner.Entities;
using System.Collections.Generic;

namespace GEP.Cumulus.P2P.Req.DataAccessObjects
{
    public interface IRequisitionCommonDAO : IBaseDAO
    {
        bool UpdateBaseCurrency(long contactCode, long documentCode, decimal documentAmount, int documentTypeId, string toCurrency, decimal conversionFactor);
        List<ContactInfo> GetPartnerContactsByPartnerCodeandOrderingLocation(long partnerCode, long orderingLocationId, bool flagToFetchContactsOfAllRoles = false);
    }
}