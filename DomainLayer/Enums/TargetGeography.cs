using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace DomainLayer.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TargetGeography
    {
        [EnumMember(Value = "Global")]
        Global,

        [EnumMember(Value = "US")]
        US,

        [EnumMember(Value = "EU")]
        EU,

        [EnumMember(Value = "Asia-Pacific")]
        AsiaPacific,

        [EnumMember(Value = "Latin America")]
        LatinAmerica,

        [EnumMember(Value = "Middle East & Africa")]
        MiddleEastAfrica
    }
}
