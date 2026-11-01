namespace ChaosHouse.Core.Simulation;

public enum SimulationStatus
{
    Success,
    Error,
    Cancelled
}

public sealed record SimulationResult(
    string ObjectName,
    SimulationStatus Status,
    string Details,
    TimeSpan Duration,
    long? Checksum = null);
