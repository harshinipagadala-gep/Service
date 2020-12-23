using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using GEP.Smart.Platform.SearchCoreIntegretor.Entities;
using GEP.Cumulus.Contract.Entities;

namespace GEP.Cumulus.P2P.Req.BusinessObjects
{
    [ExcludeFromCodeCoverage]
    [DataContract]
    [Serializable]
    public class CatalogItemDetails
    {
        [DataMember]
        public string BuyerItemNumber { get; set; }
        [DataMember]
        public string SupplierItemNumber { get; set; }
        [DataMember]
        public long SupplierCode { get; set; }
        [DataMember]
        public long OrderingLocationID { get; set; }

        [DataMember]
        public long CategoryId { get; set; }
        [DataMember]
        public string CategoryName { get; set; }
    }
    public class DataSearchResultWrapper
    {
        public DataSearchResult DataSearchResult { get; set; }
    }
}
