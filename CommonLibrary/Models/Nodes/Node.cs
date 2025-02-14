namespace CommonLibrary.Models.Nodes;

public record Node
{
    public required int Id { get; init; }
    public required string Address { get; set; }
}