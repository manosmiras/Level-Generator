using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
public class DesignElementConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return (objectType == typeof(DesignElement));
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var jo = JObject.Load(reader);
        if (jo["FooBarBuzz"].Value<string>() == "A")
            return jo.ToObject<LevelPiece>(serializer);

        if (jo["FooBarBuzz"].Value<string>() == "B")
            return jo.ToObject<Trap>(serializer);

        return null;
    }

    public override bool CanWrite => false;

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}