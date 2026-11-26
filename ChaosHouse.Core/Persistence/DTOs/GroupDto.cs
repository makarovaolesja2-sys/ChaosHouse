using System.Collections.Generic;

namespace ChaosHouse.Core.Persistence.DTOs
{
    public class GroupDto
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public List<string> MemberIds { get; set; } = new();
    }
}
