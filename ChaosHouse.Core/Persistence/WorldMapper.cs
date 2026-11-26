using ChaosHouse.Core.Groups;
using ChaosHouse.Core.Persistence.DTOs;
using System;
using System.Collections.Generic;
using ChaosHouse.Core.Groups;

namespace ChaosHouse.Core.Persistence
{
    public static class WorldMapper
    {
        public static WorldDto ToDto(
            IEnumerable<HouseMember> members,
            IEnumerable<HouseGroup> groups)
        {
            ArgumentNullException.ThrowIfNull(members);
            ArgumentNullException.ThrowIfNull(groups);

            var world = new WorldDto();

            foreach (var member in members)
            {
                HouseMemberDto dto = member switch
                {
                    Ghost ghost => new GhostDto
                    {
                        SpookinessLevel = ghost.SpookinessLevel
                    },

                    Inventor inventor => new InventorDto
                    {
                        CurrentGadget = inventor.CurrentGadget
                    },

                    TalkingFridge fridge => new TalkingFridgeDto
                    {
                        IsLocked = fridge.IsLocked
                    },

                    _ => throw new InvalidOperationException(
                        $"Tundmatu tegelase tüüp: {member.GetType().Name}")
                };

                dto.Id = member.Id;
                dto.Name = member.Name;
                dto.TypeName = member.TypeName;
                dto.Energy = member.Energy;

                world.Members.Add(dto);
            }

            foreach (var group in groups)
            {
                var groupDto = new GroupDto
                {
                    Id = group.Id,
                    Name = group.Name
                };

                foreach (var member in group.Members)
                {
                    groupDto.MemberIds.Add(member.Id);
                }

                world.Groups.Add(groupDto);
            }

            return world;
        }

        public static HouseMember FromDto(HouseMemberDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            if (string.IsNullOrWhiteSpace(dto.Id))
                throw new InvalidOperationException("Tegelase ID puudub.");

            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new InvalidOperationException("Tegelase nimi puudub.");

            if (dto.Energy < 0 || dto.Energy > 100)
                throw new InvalidOperationException(
                    "Tegelase energia peab olema vahemikus 0–100.");

            if (string.IsNullOrWhiteSpace(dto.TypeName))
                throw new InvalidOperationException(
                    "Tegelase tüüp puudub.");

            return dto switch
            {
                GhostDto ghost
                    when dto.TypeName == "Kummitus"
                    && ghost.SpookinessLevel >= 0
                    && ghost.SpookinessLevel <= 100 =>
                        new Ghost(
                            ghost.Id,
                            ghost.Name,
                            ghost.Energy,
                            ghost.SpookinessLevel),

                InventorDto inventor
                    when dto.TypeName == "Leiutaja"
                    && !string.IsNullOrWhiteSpace(inventor.CurrentGadget) =>
                        new Inventor(
                            inventor.Id,
                            inventor.Name,
                            inventor.Energy,
                            inventor.CurrentGadget),

                TalkingFridgeDto fridge
                    when dto.TypeName == "Rääkiv külmkapp" =>
                        new TalkingFridge(
                            fridge.Id,
                            fridge.Name,
                            fridge.Energy,
                            fridge.IsLocked),

                _ => throw new InvalidOperationException(
                    $"Salvestatud tegelase tüüp või andmed on vigased: {dto.TypeName}")
            };
        }
    }
}
