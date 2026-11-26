using System.Text.Json.Serialization;

namespace ChaosHouse.Core.Persistence.DTOs
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    [JsonDerivedType(typeof(GhostDto), "ghost")]
    [JsonDerivedType(typeof(InventorDto), "inventor")]
    [JsonDerivedType(typeof(TalkingFridgeDto), "fridge")]
    public class HouseMemberDto
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string TypeName { get; set; } = string.Empty;

        public int Energy { get; set; }
    }
}

