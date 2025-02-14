using CommonLibrary.Models.Enums;

namespace CommonLibrary.Models.Registration;

public record RegistrationResponse
{
    public NodeRole NodeRole { get; init; }
    public int Id { get; init; }
}