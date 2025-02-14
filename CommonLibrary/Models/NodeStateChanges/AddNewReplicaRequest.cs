namespace CommonLibrary.Models.NodeStateChanges;

public record AddNewReplicaRequest
{
    public required int Id { get; init; }
    public required string Address { get; init; }
    public required long LastSavedChangeId { get; init; }
}