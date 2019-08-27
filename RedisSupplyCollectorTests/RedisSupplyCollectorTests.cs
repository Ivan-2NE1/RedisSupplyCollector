using System;
using Xunit;
using System.Collections.Generic;
using S2.BlackSwan.SupplyCollector.Models;

namespace RedisSupplyCollectorTests
{
    public class RedisSupplyCollectorTests
    {
        public readonly RedisSupplyCollector.RedisSupplyCollector _instance;
        public readonly DataContainer _container;
        
        public RedisSupplyCollectorTests()
        {
            _instance = new RedisSupplyCollector.RedisSupplyCollector();
            var connectionStringValues = new Dictionary<string, string>();
            connectionStringValues["host"] = "localhost";
            connectionStringValues["port"] = "6379";

            _container = new DataContainer()
            {
                ConnectionString = _instance.BuildConnectionString(connectionStringValues)
            };
        }

        [Fact]
        public void DataStoreTypesTest()
        {
            var result = _instance.DataStoreTypes();
            Assert.Contains("Redis", result);
        }

        [Fact]
        public void TestConnectionTest()
        {
            var result = _instance.TestConnection(_container);
            Assert.True(result);
        }

        [Fact]
        public void GetSchemaTest()
        {
            var (tables, elements) = _instance.GetSchema(_container);
            Assert.Equal(3, tables.Count);
            Assert.Equal(120, elements.Count);
            foreach(DataEntity element in elements)
            {
                Assert.NotEqual(string.Empty, element.DbDataType);
            }
        }

    }
}
