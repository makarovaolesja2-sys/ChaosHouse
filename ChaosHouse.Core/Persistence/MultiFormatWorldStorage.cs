using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ChaosHouse.Core.Persistence.DTOs;

namespace ChaosHouse.Core.Persistence
{
    public class MultiFormatWorldStorage : IWorldStorage
    {
        private readonly JsonWorldStorage _jsonStorage = new();
        private readonly XmlWorldStorage _xmlStorage = new();
        private readonly LocalDbWorldStorage _localDbStorage = new();

        private IWorldStorage GetStorage(string path)
        {
            string extension = Path.GetExtension(path).ToLowerInvariant();

            return extension switch
            {
                ".json" => _jsonStorage,
                ".xml" => _xmlStorage,
                ".db" => _localDbStorage,
                _ => throw new NotSupportedException(
                    "Toetatud failivormingud on JSON, XML ja LocalDB.")
            };
        }

        public Task SaveAsync(
            WorldDto world,
            string path,
            CancellationToken token = default)
        {
            return GetStorage(path).SaveAsync(world, path, token);
        }

        public Task<WorldDto> LoadAsync(
            string path,
            CancellationToken token = default)
        {
            return GetStorage(path).LoadAsync(path, token);
        }
    }
}
