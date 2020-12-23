using System.Diagnostics.CodeAnalysis;
using System.ServiceModel;
using OrganizationContract = GEP.Cumulus.OrganizationStructure.ServiceContracts;
using PartnerContract = Gep.Cumulus.Partner.ServiceContracts;

namespace GEP.Cumulus.P2P.Req.BusinessObjects
{
    [ExcludeFromCodeCoverage]
    internal class ServiceInitialization
    {
        public interface IPartnerServiceChannel : PartnerContract.IPartner, IClientChannel
        {
        }
        public interface IOrganizationStructureChannel : OrganizationContract.IOrganizationStructure, IClientChannel
        {
        }
    }
}
