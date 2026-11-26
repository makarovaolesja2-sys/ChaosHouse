using System.Threading;
using System.Threading.Tasks;
using ChaosHouse.Core.Persistence.DTOs;

namespace ChaosHouse.Core.Persistence
{
    public interface IWorldStorage
    {
        Task SaveAsync(
            WorldDto world,
            string path,
            CancellationToken token = default);

        Task<WorldDto> LoadAsync(
            string path,
            CancellationToken token = default);
    }
}
