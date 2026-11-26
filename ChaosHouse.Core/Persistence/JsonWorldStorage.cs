using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ChaosHouse.Core.Persistence.DTOs;

namespace ChaosHouse.Core.Persistence
{
    public class JsonWorldStorage : IWorldStorage
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        public async Task SaveAsync(
            WorldDto world,
            string path,
            CancellationToken token = default)
        {
            ArgumentNullException.ThrowIfNull(world);

            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException(
                    "Salvestusfaili tee ei tohi olla tühi.",
                    nameof(path));

            await using var stream = new FileStream(
                path,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                4096,
                FileOptions.Asynchronous);

            await JsonSerializer.SerializeAsync(
                stream,
                world,
                Options,
                token);
        }

        public async Task<WorldDto> LoadAsync(
            string path,
            CancellationToken token = default)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException(
                    "Faili tee ei tohi olla tühi.",
                    nameof(path));

            await using var stream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                4096,
                FileOptions.Asynchronous);

            var world = await JsonSerializer.DeserializeAsync<WorldDto>(
                stream,
                Options,
                token);

            return world ?? throw new InvalidDataException(
                "Salvestusfail ei sisalda korrektset maailma.");
        }
    }
}
