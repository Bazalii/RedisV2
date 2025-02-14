namespace CommonLibrary.Models.Nodes;

public record InconsistentNode
{
    public required int Id { get; init; }
    public required string Address { get; set; }
    public long LastChangeId { get; init; }
}