using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace DomainLayer.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ResearchDepth
    {
        [EnumMember(Value = "quick")]
        Quick,

        [EnumMember(Value = "standard")]
        Standard,

        [EnumMember(Value = "comprehensive")]
        Comprehensive
    }
}
