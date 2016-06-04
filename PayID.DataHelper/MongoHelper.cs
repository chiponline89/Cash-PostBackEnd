using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayID.DataHelper
{
    public class MongoHelper
    {
        public  string MongoServer, MongoDatabase;
        public  MongoDatabase _database;

        public MongoHelper()
        {

        }

        public MongoHelper(string _server, string _db)
        {
            MongoServer = _server;
            MongoDatabase = _db;
        }

       MongoDatabase Database
        {
            get
            {
                try
                {
                    if (_database == null)
                    {
                        MongoClient client = new MongoClient(MongoServer);
                        MongoServer server = client.GetServer();
                        server.Connect();
                        _database = server.GetDatabase(MongoDatabase);
                    }
                    return _database;
                }
                catch
                {
                    return null;
                }
            }
        }

        public  bool Delete(string objectName, dynamic _id)
        {
            FindAndRemoveArgs arg = new FindAndRemoveArgs();
            arg.Query = Query.EQ("_id", _id);
            FindAndModifyResult result = Database.GetCollection(objectName).FindAndRemove(arg);
            return result.Ok;
        }

        private  BsonDocument GetDocument(string collectionName, IMongoQuery query)
        {
            var doc = Database.GetCollection(collectionName).Find(query).FirstOrDefault();
            return doc;
        }

        public dynamic GetDynamic(string objectName, IMongoQuery query)
        {
            BsonDocument doc = GetDocument(objectName, query);
            if (doc == null)
                return null;
            return doc.ToDynamic();
        }
        public  DynamicObj Get(string objectName, IMongoQuery query)
        {
            BsonDocument doc = GetDocument(objectName, query);
            if (doc == null)
                return null;
            return new DynamicObj(doc);
        }
        private  List<BsonDocument> ListDocument(string collectionName, IMongoQuery query)
        {
            var doc = Database.GetCollection(collectionName).Find(query).ToList();
            return doc;
        }
        public BsonDocument[] ListPaggingBSon(string objectName, IMongoQuery query, IMongoSortBy sort, int pageSize, int pageNum, out long totalSize)
        {
            int skip = (pageNum - 1) * pageSize;
            var list = Database.GetCollection(objectName).Find(query).SetSortOrder(sort).Skip(skip).Take(pageSize);
            totalSize = Database.GetCollection(objectName).Find(query).Count();
            return list.ToArray();
        }
        public long TotalbyStatus(string objectName, IMongoQuery query)
        {          
            return Database.GetCollection(objectName).Find(query).Count();            
        }
        public  dynamic[] ListPagging(string objectName, IMongoQuery query, IMongoSortBy sort, int pageSize, int pageNum, out long totalSize)
        {
            BsonDocument[] list = ListPaggingBSon(objectName, query, sort, pageSize, pageNum, out totalSize);
            List<DynamicObj> lstObj = new List<DynamicObj>();
            foreach (BsonDocument doc in list)
            {
                lstObj.Add(new DynamicObj(doc));
            }
            return lstObj.ToArray();
        }
        public dynamic[] ListDynamic(string objectName, IMongoQuery query)
        {
            List<BsonDocument> list = ListDocument(objectName, query);
            List<dynamic> lstObj = new List<dynamic>();
            foreach (BsonDocument doc in list)
            {
                lstObj.Add(doc.ToDynamic());
            }
            return lstObj.ToArray();
        }
        public  DynamicObj[] List(string objectName, IMongoQuery query)
        {

            List<BsonDocument> list = ListDocument(objectName, query);
            List<DynamicObj> lstObj = new List<DynamicObj>();
            foreach (BsonDocument doc in list)
            {
                lstObj.Add(new DynamicObj(doc));
            }
            return lstObj.ToArray();
        }

        private  bool SaveDocument(string collectionName, BsonDocument document)
        {
            WriteConcernResult result = Database.GetCollection(collectionName).Save(document);
            return result.Ok;
        }

        public bool SaveDynamic(string objectName, dynamic obj)
        {
            DateTime _date = DateTime.Now;
            dynamic dyna = obj;
            dyna.system_last_updated_time = _date;
            dyna.system_time_key = JObject.Parse(@"{
                date : " + _date.ToString("yyyyMMdd") + ",time : '" + _date.ToString("HHmmss") +"'}"
            );
            BsonDocument doc = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(obj.ToString());
            return SaveDocument(objectName, doc);
        }
        public  bool Save(string objectName, DynamicObj obj)
        {
            DateTime _date = DateTime.Now;
            dynamic dyna = obj;
            dyna.system_last_updated_time = _date;
            dynamic objTimeKey = new DynamicObj();
            objTimeKey.date = long.Parse(_date.ToString("yyyyMMdd"));
            objTimeKey.time = _date.ToString("HHmmss");
            dyna.system_time_key = objTimeKey;
         
            return SaveDocument(objectName, dyna.ToBsonDocument());
        }

        private  bool InsertDocument(string collectionName, BsonDocument document)
        {
            WriteConcernResult result = Database.GetCollection(collectionName).Insert(document);
            return result.Ok;
        }
        public bool InsertDynamic(string objectName, dynamic obj)
        {
            DateTime _date = DateTime.Now;
            dynamic dyna = obj;
            dyna.system_last_updated_time = _date;
            dyna.system_created_time = _date;
            dyna.last_update_time_key = new
            {
                date =  long.Parse(_date.ToString("yyyyMMdd")),
                time =  long.Parse(_date.ToString("HHmmss"))
            };
            BsonDocument doc = BsonDocument.Parse(JsonConvert.SerializeObject(dyna));
            return InsertDocument(objectName,doc);
        }
        public  bool Insert(string objectName, DynamicObj obj)
        {
            DateTime _date = DateTime.Now;
            dynamic dyna = obj;
            dyna.system_last_updated_time = _date;
            dyna.system_created_time = _date;
            dynamic objTimeKey = new DynamicObj();
            objTimeKey.date = long.Parse(_date.ToString("yyyyMMdd"));
            objTimeKey.time = long.Parse(_date.ToString("HHmmss"));
            dyna.system_time_key = objTimeKey;
            return InsertDocument(objectName, dyna.ToBsonDocument());
        }
        public  bool UpdateObject(string objectName, IMongoQuery query, IMongoUpdate update)
        {
            WriteConcernResult result = Database.GetCollection(objectName).Update(query, update);
            return result.Ok;
        }

        public  long GetNextSquence(string SequenceName)
        {
            try
            {
                MongoCollection sequenceCollection = Database.GetCollection("counters");
                FindAndModifyArgs args = new FindAndModifyArgs();
                args.Query = Query.EQ("_id", SequenceName);
                args.Update = MongoDB.Driver.Builders.Update.Inc("seq", 1);
                FindAndModifyResult result = sequenceCollection.FindAndModify(args);
                return result.ModifiedDocument.GetElement("seq").Value.ToInt64();
            }
            catch
            {
                dynamic bs = new DynamicObj();
                bs._id = SequenceName;
                bs.seq = 2;
                Insert("counters", bs);
                return 1;
            }
        }
        private  void CreateDocument(string collectionName, BsonDocument document, IMongoQuery query)
        {
            var old = GetDocument(collectionName, query);
            if (old == null)
                InsertDocument(collectionName, document);
        }

        public  void Create(string objectName, DynamicObj obj, IMongoQuery query)
        {
            CreateDocument(objectName, obj.ToBsonDocument(), query);
        }
        public  long Count(string objectName, IMongoQuery query)
        {
            return Database.GetCollection(objectName).Find(query).Count();
        }
    }
}
