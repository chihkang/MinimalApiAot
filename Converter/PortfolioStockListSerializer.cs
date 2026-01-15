namespace MinimalApiAot.Converter;

public sealed class PortfolioStockListSerializer : SerializerBase<List<PortfolioStock>>
{
    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, List<PortfolioStock> value)
    {
        if (value is null)
        {
            context.Writer.WriteNull();
            return;
        }

        var itemSerializer = BsonSerializer.LookupSerializer<PortfolioStock>();
        context.Writer.WriteStartArray();
        foreach (var item in value)
        {
            itemSerializer.Serialize(context, item);
        }
        context.Writer.WriteEndArray();
    }

    public override List<PortfolioStock> Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var bsonType = context.Reader.GetCurrentBsonType();
        if (bsonType == BsonType.Null)
        {
            context.Reader.ReadNull();
            return new List<PortfolioStock>();
        }

        var itemSerializer = BsonSerializer.LookupSerializer<PortfolioStock>();
        var list = new List<PortfolioStock>();

        context.Reader.ReadStartArray();
        while (context.Reader.ReadBsonType() != BsonType.EndOfDocument)
        {
            var item = itemSerializer.Deserialize(context);
            list.Add(item);
        }
        context.Reader.ReadEndArray();

        return list;
    }
}
