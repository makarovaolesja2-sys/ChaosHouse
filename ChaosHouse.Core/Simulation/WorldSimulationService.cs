using System.Diagnostics;

namespace ChaosHouse.Core.Simulation;

public sealed class WorldSimulationService
{
    private const int CancellationCheckMask = 0x0FFF;

    public SimulationResult Calculate(
        SimulationSnapshot snapshot,
        int workUnits,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            if (workUnits < 1)
                throw new ArgumentOutOfRangeException(nameof(workUnits), "Töömaht peab olema vähemalt 1.");

            long checksum = CreateSeed(snapshot);
            for (int i = 0; i < workUnits; i++)
            {
                if ((i & CancellationCheckMask) == 0)
                    cancellationToken.ThrowIfCancellationRequested();

                unchecked
                {
                    checksum ^= checksum << 13;
                    checksum ^= checksum >> 7;
                    checksum ^= checksum << 17;
                    checksum += (long)i * (snapshot.Energy + 1);
                }

                if (snapshot.InjectFailure && i == workUnits / 3)
                    throw new InvalidOperationException("Demoviga: selle objekti arvutus katkestati näidisveaga.");
            }

            cancellationToken.ThrowIfCancellationRequested();
            stopwatch.Stop();
            return new SimulationResult(snapshot.Name, SimulationStatus.Success,
                $"Kontrollsumma: {checksum}", stopwatch.Elapsed, checksum);
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();
            return new SimulationResult(snapshot.Name, SimulationStatus.Cancelled,
                "Arvutus tühistati.", stopwatch.Elapsed);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return new SimulationResult(snapshot.Name, SimulationStatus.Error,
                $"{ex.GetType().Name}: {ex.Message}", stopwatch.Elapsed);
        }
    }

    private static long CreateSeed(SimulationSnapshot snapshot)
    {
        unchecked
        {
            long hash = unchecked((long)14695981039346656037UL);
            foreach (char character in snapshot.Id + snapshot.Name + snapshot.TypeName)
                hash = (hash ^ character) * 1099511628211L;
            return hash ^ snapshot.Energy;
        }
    }
}
