using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using ChaosHouse.Core.Persistence.DTOs;

namespace ChaosHouse.Core.Persistence
{
    public class LocalDbWorldStorage : IWorldStorage
    {
        private readonly string _connectionString =
            @"Server=(localdb)\MSSQLLocalDB;Database=ChaosHouseDb;Integrated Security=true;";


        public async Task SaveAsync(
            WorldDto world,
            string path,
            CancellationToken token = default)
        {
            ArgumentNullException.ThrowIfNull(world);

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(token);

            using var transaction =
                (SqlTransaction)await connection.BeginTransactionAsync(token);

            try
            {
                using (var deleteCommand = new SqlCommand(
                    @"DELETE FROM dbo.GroupMember;
              DELETE FROM dbo.HouseGroup;
              DELETE FROM dbo.HouseMember;",
                    connection,
                    transaction))
                {
                    await deleteCommand.ExecuteNonQueryAsync(token);
                }

                foreach (HouseMemberDto member in world.Members)
                {
                    token.ThrowIfCancellationRequested();

                    if (member == null)
                        throw new InvalidOperationException(
                            "Salvestatav tegelane puudub.");

                    int? spookinessLevel = null;
                    string? currentGadget = null;
                    bool? isLocked = null;

                    switch (member)
                    {
                        case GhostDto ghost:
                            spookinessLevel = ghost.SpookinessLevel;
                            break;

                        case InventorDto inventor:
                            currentGadget = inventor.CurrentGadget;
                            break;

                        case TalkingFridgeDto fridge:
                            isLocked = fridge.IsLocked;
                            break;

                        default:
                            throw new InvalidOperationException(
                                "Tundmatu tegelase tüüp.");
                    }

                    using var command = new SqlCommand(
                        @"INSERT INTO dbo.HouseMember
                          (Id, Name, TypeName, Energy,
                           SpookinessLevel, CurrentGadget, IsLocked)
                         VALUES
                         (@Id, @Name, @TypeName, @Energy,
                          @SpookinessLevel, @CurrentGadget, @IsLocked);",
                        connection,
                        transaction);

                    command.Parameters.Add("@Id", SqlDbType.NVarChar, 100).Value =
                        member.Id;

                    command.Parameters.Add("@Name", SqlDbType.NVarChar, 200).Value =
                        member.Name;

                    command.Parameters.Add("@TypeName", SqlDbType.NVarChar, 100).Value =
                        member.TypeName;

                    command.Parameters.Add("@Energy", SqlDbType.Int).Value =
                        member.Energy;

                    command.Parameters.Add("@SpookinessLevel", SqlDbType.Int).Value =
                        (object?)spookinessLevel ?? DBNull.Value;

                    command.Parameters.Add("@CurrentGadget", SqlDbType.NVarChar, 200).Value =
                        (object?)currentGadget ?? DBNull.Value;

                    command.Parameters.Add("@IsLocked", SqlDbType.Bit).Value =
                        (object?)isLocked ?? DBNull.Value;

                    await command.ExecuteNonQueryAsync(token);
                }

                foreach (GroupDto group in world.Groups)
                {
                    token.ThrowIfCancellationRequested();

                    using var command = new SqlCommand(
                        @"INSERT INTO dbo.HouseGroup (Id, Name)
                  VALUES (@Id, @Name);",
                        connection,
                        transaction);

                    command.Parameters.Add("@Id", SqlDbType.NVarChar, 100).Value =
                        group.Id;

                    command.Parameters.Add("@Name", SqlDbType.NVarChar, 200).Value =
                        group.Name;

                    await command.ExecuteNonQueryAsync(token);
                }

                foreach (GroupDto group in world.Groups)
                {
                    foreach (string memberId in group.MemberIds)
                    {
                        token.ThrowIfCancellationRequested();

                        using var command = new SqlCommand(
                            @"INSERT INTO dbo.GroupMember (GroupId, MemberId)
                      VALUES (@GroupId, @MemberId);",
                            connection,
                            transaction);

                        command.Parameters.Add("@GroupId", SqlDbType.NVarChar, 100).Value =
                            group.Id;

                        command.Parameters.Add("@MemberId", SqlDbType.NVarChar, 100).Value =
                            memberId;

                        await command.ExecuteNonQueryAsync(token);
                    }
                }

                await transaction.CommitAsync(token);
            }
            catch
            {
                await transaction.RollbackAsync(CancellationToken.None);
                throw;
            }
        }

        public async Task<WorldDto> LoadAsync(
            string path,
            CancellationToken token = default)
        {
            var world = new WorldDto();

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(token);

            using (var command = new SqlCommand(
                @"SELECT Id, Name, TypeName, Energy,
                 SpookinessLevel, CurrentGadget, IsLocked
          FROM dbo.HouseMember;",
                connection))
            {
                using var reader = await command.ExecuteReaderAsync(token);

                while (await reader.ReadAsync(token))
                {
                    string id = reader.GetString(0);
                    string name = reader.GetString(1);
                    string typeName = reader.GetString(2);
                    int energy = reader.GetInt32(3);

                    HouseMemberDto member = typeName switch
                    {
                        "Kummitus" => new GhostDto
                        {
                            Id = id,
                            Name = name,
                            TypeName = typeName,
                            Energy = energy,
                            SpookinessLevel = reader.GetInt32(4)
                        },

                        "Leiutaja" => new InventorDto
                        {
                            Id = id,
                            Name = name,
                            TypeName = typeName,
                            Energy = energy,
                            CurrentGadget = reader.GetString(5)
                        },

                        "Rääkiv külmkapp" => new TalkingFridgeDto
                        {
                            Id = id,
                            Name = name,
                            TypeName = typeName,
                            Energy = energy,
                            IsLocked = reader.GetBoolean(6)
                        },

                        _ => throw new InvalidOperationException(
                            $"Tundmatu tegelase tüüp andmebaasis: {typeName}")
                    };

                    world.Members.Add(member);
                }
            }

            using (var command = new SqlCommand(
                @"SELECT Id, Name
          FROM dbo.HouseGroup;",
                connection))
            {
                using var reader = await command.ExecuteReaderAsync(token);

                while (await reader.ReadAsync(token))
                {
                    world.Groups.Add(new GroupDto
                    {
                        Id = reader.GetString(0),
                        Name = reader.GetString(1)
                    });
                }
            }

            using (var command = new SqlCommand(
                @"SELECT GroupId, MemberId
          FROM dbo.GroupMember;",
                connection))
            {
                using var reader = await command.ExecuteReaderAsync(token);

                while (await reader.ReadAsync(token))
                {
                    string groupId = reader.GetString(0);
                    string memberId = reader.GetString(1);

                    GroupDto? group = world.Groups.FirstOrDefault(
                        g => g.Id == groupId);

                    if (group == null)
                        throw new InvalidOperationException(
                            $"Grupi ID-ga {groupId} ei leitud.");

                    group.MemberIds.Add(memberId);
                }
            }

            return world;
        }

    }
}
