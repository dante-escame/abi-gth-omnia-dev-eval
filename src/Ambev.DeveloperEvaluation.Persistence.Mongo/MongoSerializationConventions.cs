using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Ambev.DeveloperEvaluation.Persistence.Mongo;

public static class MongoSerializationConventions
{
    private static readonly bool Registered = RegisterSerializers();

    public static void Register() => _ = Registered;

    private static bool RegisterSerializers()
    {
        BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
        BsonSerializer.RegisterSerializer(new DecimalSerializer(BsonType.Decimal128));
        BsonSerializer.RegisterSerializer(new NullableSerializer<decimal>(new DecimalSerializer(BsonType.Decimal128)));

        return true;
    }
}
