namespace ChaosHouse.Core.Simulation;

public sealed record SimulationSnapshot(
    string Id,
    string Name,
    string TypeName,
    int Energy,
    bool InjectFailure);
