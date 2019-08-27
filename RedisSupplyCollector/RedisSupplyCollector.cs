using System;
using System.Collections.Generic;
using System.Linq;
using S2.BlackSwan.SupplyCollector;
using S2.BlackSwan.SupplyCollector.Models;
using StackExchange.Redis;

namespace RedisSupplyCollector
{
    public class RedisSupplyCollector : SupplyCollectorBase
    {
        public string BuildConnectionString(Dictionary<string, string> connectionStringValues)
        {
            return connectionStringValues["host"] + ":" + connectionStringValues["port"];
        }
        public override List<string> CollectSample(DataEntity dataEntity, int sampleSize)
        {
            DataContainer container = dataEntity.Container;
            var redis = ConnectionMultiplexer.Connect(container.ConnectionString);
            IDatabase db = redis.GetDatabase();
            IServer server = redis.GetServer(container.ConnectionString);

            var samples = new List<string>();

            foreach(var key in server.Keys())
            {
                RedisValue value = db.StringGet(key);
                samples.Add(value.ToString());
            }

            return samples;
        }

        public override List<string> DataStoreTypes()
        {
            return new List<string>() { "Redis" };
        }

        public override List<DataCollectionMetrics> GetDataCollectionMetrics(DataContainer container)
        {
            var dataCollectionMetrics = new List<DataCollectionMetrics>();

            var redis = ConnectionMultiplexer.Connect(container.ConnectionString);
            IDatabase db = redis.GetDatabase();
            IServer server = redis.GetServer(container.ConnectionString);

            foreach(var key in server.Keys())
            {
                RedisValue value = db.StringGet(key);

                var metrics = new DataCollectionMetrics();
                metrics.Name = key;
                metrics.RowCount = 1;
                metrics.TotalSpaceKB = value.Length() / 1024;
                metrics.UsedSpaceKB = metrics.TotalSpaceKB;

                dataCollectionMetrics.Add(metrics);
            }

            return dataCollectionMetrics;
        }

        public override (List<DataCollection>, List<DataEntity>) GetSchema(DataContainer container)
        {
            var collections = new List<DataCollection>();
            var entities = new List<DataEntity>();

            var redis = ConnectionMultiplexer.Connect(container.ConnectionString);
            IDatabase db = redis.GetDatabase();
            var endpoints = redis.GetEndPoints();
            IServer server = redis.GetServer(endpoints.First());

            foreach(var key in server.Keys())
            {
                var collectionEntities = new List<DataEntity>();

                DataCollection dataCollection = new DataCollection(container, key);
                dataCollection.Container = container;
                dataCollection.Name = key;

                collections.Add(dataCollection);

                RedisValue value = db.StringGet(key);
                if(value.HasValue)
                {
                    var redisdataType = DataType.String.ToString();
                    entities.Add(new DataEntity(value.ToString(), DataType.String, redisdataType, container, dataCollection));
                }

            }

            return (collections, entities);
        }

        public override bool TestConnection(DataContainer container)
        {
            var connection = ConnectionMultiplexer.Connect(container.ConnectionString);
            return connection.IsConnected;
        }
    }
}
