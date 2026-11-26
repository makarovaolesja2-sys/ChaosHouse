using System.Collections.Generic;

namespace ChaosHouse.Core.Persistence.DTOs
{
    public class WorldDto
    {
        public List<HouseMemberDto> Members { get; set; } = new();

        public List<GroupDto> Groups { get; set; } = new();
    }
}
