using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PayID.API
{
    public class DynamicSerializer : IBsonSerializer
    {
        #region Implementation of IBsonSerializer

        public object Deserialize(BsonReader bsonReader, Type nominalType, IBsonSerializationOptions options)
        {
            return Deserialize(bsonReader, nominalType, null, options);
        }

        public object Deserialize(BsonReader bsonReader, Type nominalType, Type actualType,
          IBsonSerializationOptions options)
        {
            if (bsonReader.GetCurrentBsonType() != BsonType.Document) throw new Exception("Not document");
            var bsonDocument = BsonSerializer.Deserialize(bsonReader, typeof(BsonDocument), options) as BsonDocument;
            return JsonConvert.DeserializeObject<dynamic>(bsonDocument.ToJson());
        }

        public IBsonSerializationOptions GetDefaultSerializationOptions()
        {
            return new DocumentSerializationOptions();
        }

        public void Serialize(BsonWriter bsonWriter, Type nominalType, object value, IBsonSerializationOptions options)
        {
            var json = (value == null) ? "{}" : JsonConvert.SerializeObject(value);
            BsonDocument document = BsonDocument.Parse(json);
            BsonSerializer.Serialize(bsonWriter, typeof(BsonDocument), document, options);
        }

        #endregion
    }
}