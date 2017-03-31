using Roidis;
using Roidis.Attribute;
using Roidis.Service.Converter;
using Roidis.Service.KeyGenerator;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var _redisConfig = new ConfigurationOptions()
            {
                ClientName = "PetraTools",
                ConnectTimeout = 100000,
                SyncTimeout = 100000,
                AbortOnConnectFail = true,
                ConnectRetry = 2,
            };
            _redisConfig.EndPoints.Add("192.168.160.131:6379");
            var conn = ConnectionMultiplexer.Connect(_redisConfig);
            conn.PreserveAsyncOrder = false;

            Console.WriteLine("Running!");
            Console.WriteLine("Hit any key to contue. 'c' to quit!");

            var roid = new Roid(conn);

            var key = "";

            var proxy = roid.From<TestObject>();

            while (key != "C")
            {
                //    var x = new TestObject()
                //    {
                //        Id = key,
                //        StringProp = key
                //    };
                //    proxy.Save(x).Wait();
                //    x = proxy.Fetch(key).Result;

                //    proxy.FetchAll().Subscribe(item => Console.WriteLine(item.Id));
                proxy.FetchAllWhere(a => !a.BoolProp && a.GuidProp == Guid.Empty).Subscribe(item => Console.WriteLine(item.Id));

                //proxy.FetchAllWhere(a => !a.BoolProp);
                //proxy.FetchAllWhere(a => a.BoolProp && a.BoolProp);
                //proxy.FetchAllWhere(a => a.BoolProp || a.BoolProp);
                //proxy.FetchAllWhere(a => !a.BoolProp && !a.BoolProp);
                //proxy.FetchAllWhere(a => !a.BoolProp && a.BoolProp);
                //proxy.FetchAllWhere(a => !a.BoolProp || !a.BoolProp);
                //proxy.FetchAllWhere(a => a.BoolProp || !a.BoolProp);
                //proxy.FetchAllWhere(a => a.BoolProp == true);
                //proxy.FetchAllWhere(a => a.BoolProp != false);
                //proxy.FetchAllWhere(a => a.BoolProp == true && a.BoolProp != false);
                //proxy.FetchAllWhere(a => a.IntProp == 1);
                //proxy.FetchAllWhere(a => a.LongProp == 1);
                //proxy.FetchAllWhere(a => a.DecimalProp == 1);
                //proxy.FetchAllWhere(a => a.StringProp == "");
                //proxy.FetchAllWhere(a => a.DateTimeOffsetProp == new DateTimeOffset());
                //proxy.FetchAllWhere(a => a.GuidProp == Guid.Empty);
                //proxy.FetchAllWhere(a => a.StringProp == null);
                //proxy.FetchAllWhere(a => a.StringProp == "");
                //proxy.FetchAllWhere(a => a.DateTimeOffsetProp == DateTimeOffset.Now);
                //proxy.FetchAllWhere(a => a.Enum1 == Enum1.Enum1Value1);
                //proxy.FetchAllWhere(a => a.Enum1 == Enum1.Enum1Value1 && a.DateTimeOffsetProp == DateTimeOffset.Now);
                //proxy.FetchAllWhere(a => a.ByteArrayProp == new byte[0]);
                //proxy.FetchAllWhere(a => a.ByteArrayProp == new byte[0] || a.GuidProp == Guid.Empty);


                key = Console.ReadKey().Key.ToString();
                Console.WriteLine(key);
            }

            Console.WriteLine("Exiting...");
        }

        [RoidPrefix("CustomPrefix")]
        public class TestObject
        {
            [RoidKey]
            public string Id { get; set; }

            [RoidIndex]
            public string StringProp { get; set; }

            [RoidIndex]
            public byte[] ByteArrayProp { get; set; }

            [RoidIndex]
            public int IntProp { get; set; }
            public int? NullableIntProp { get; set; }

            [RoidIndex]
            public double DoubleProp { get; set; }
            public double? NullableDoubleProp { get; set; }

            [RoidIndex]
            public long LongProp { get; set; }
            public long? NullableLongProp { get; set; }

            [RoidIndex]
            public decimal DecimalProp { get; set; }
            public decimal? NullableDecimalProp { get; set; }

            [RoidIndex]
            public bool BoolProp { get; set; }
            public bool? NullableBoolProp { get; set; }

            [RoidIndex]
            public Guid GuidProp { get; set; }
            public Guid? NullableGuidProp { get; set; }

            public DateTime DateTimeProp { get; set; }
            [RoidIndex]
            public DateTime? NullableDateTimeProp { get; set; }

            [RoidIndex]
            public DateTimeOffset DateTimeOffsetProp { get; set; }
            public DateTimeOffset? NullableDateTimeOffsetProp { get; set; }

            [RoidIndex("Enummed")]
            public Enum1 Enum1 { get; set; }
            public Enum2 Enum2 { get; set; }

            public List<object> ListProp { get; set; }
            public object[] ArrayProp { get; set; }

            [RoidRequired]
            public int RequiredField { get; set; }

            [RoidField("CustomFieldName")]
            public int CustomField { get; set; }

        }

        public enum Enum1
        {
            Enum1Value0 = 0,
            Enum1Value1 = 1,
        }

        public enum Enum2
        {
            Enum2Value0,
            Enum2Value1,
        }

    }
}