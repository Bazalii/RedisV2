using System.Text.Json.Serialization;

namespace RedisV2.Database.Domain.Models.Core.ChangeTracking;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(CollectionCreation), "collection-creation")]
[JsonDerivedType(typeof(CollectionDeletion), "collection-deletion")]
[JsonDerivedType(typeof(CollectionFlush), "collection-flush")]
[JsonDerivedType(typeof(ElementUpsert), "element-upsert")]
[JsonDerivedType(typeof(ElementDeletion), "element-deletion")]
public interface IDatabaseChange
{
    string CollectionName { get; }
    DateTimeOffset ChangeTime { get; }
    long Id { get; }
}