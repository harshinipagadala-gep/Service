using System;
using System.Runtime.Serialization;
namespace GEP.Cumulus.P2P.Req.RestService
{
    [DataContract]
    [Serializable]
    public enum EnableReminderFramework
    {
        [EnumMember(Value = "None")]
        None = 0,
        [EnumMember(Value = "EnableReminderForHR")]
        EnableReminderForHR = 1,
        [EnumMember(Value = "EnableReminderForPool")]
        EnableReminderForPool = 3,
        [EnumMember(Value = "EnableReminderForGroup")]
        EnableReminderForGroup = 9,
        [EnumMember(Value = "EnableReminderForUserDefined")]
        EnableReminderForUserDefined = 11,
        [EnumMember(Value = "EnableReminderForSpecificUser")]
        EnableReminderForSpecificUser = 12,
        [EnumMember(Value = "EnableReminderForGroupReview")]
        EnableReminderForGroupReview = 13,
        [EnumMember(Value = "EnableReminderForAll")]
        EnableReminderForAll = 99,

    }
}