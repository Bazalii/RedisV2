using CommonLibrary.Models.Nodes;
using CommonLibrary.Models.Registration;

namespace CommonLibrary.Models.NodeStateChanges;

public record MakeNodeLeaderRequest
{
    public required Node[] HealthyReplicas { get; init; }
    public required InconsistentNode[] InconsistentReplicas { get; init; }
}