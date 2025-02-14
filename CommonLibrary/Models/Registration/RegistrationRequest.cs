namespace CommonLibrary.Models.Registration;

public record RegistrationRequest
{
    // public int? NodeId { get; init; }
    public required string Name { get; init; }
    public required int Port { get; init; }
    public required long LastSavedChangeId { get; init; }
}