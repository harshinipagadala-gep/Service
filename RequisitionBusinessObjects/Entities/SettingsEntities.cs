using Gep.Cumulus.CSM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace GEP.Cumulus.P2P.Req.BusinessObjects.Entities
{
    [DataContract]
    public class SettingDetails : EntityBase
    {
        [DataMember]
        public int ContactTypeInfo { get; set; }

        [DataMember]
        public int SettingConfigurationId { get; set; }

        [DataMember]
        public int SettingDetailsId { get; set; }

        [DataMember]
        public long ContactCode { get; set; }

        [DataMember]
        public string ObjectTypes { get; set; }

        [DataMember]
        public List<BasicSettings> lstSettings { get; set; }
    }

    [DataContract]
    public class BasicSettings
    {
        [DataMember]
        public string DefaultValue { get; set; }

        [DataMember]
        public string FieldName { get; set; }

        [DataMember]
        public string FieldValue { get; set; }

        [DataMember]
        public int OwnerTypeInfo { get; set; }
    }

    [DataContract]
    public class GetFeatureSettingsResponse
    {
        [DataMember]
        public int ContactType { get; set; }
        [DataMember]
        public int FeatureConfigurationId { get; set; }
        [DataMember]
        public int FeatureSettingId { get; set; }
        [DataMember]
        public long ContactCode { get; set; }
        [DataMember]
        public List<FeatureSetting> FeatureSettings { get; set; }
    }

    [DataContract]
    public class FeatureSetting
    {
        [DataMember]
        public string DefaultValue { get; set; }
        [DataMember]
        public string FieldName { get; set; }
        [DataMember]
        public string FieldValue { get; set; }
        [DataMember]
        public int OwnerTypeInfo { get; set; }
    }
}
