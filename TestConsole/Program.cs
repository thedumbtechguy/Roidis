using Roidis;
using Roidis.Attribute;
using Roidis.Service.Converter;
using Roidis.Service.KeyGenerator;
using Roidis.Service.Parser;
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

            var proxy = roid.From<MyObject>();

            while (key != "C")
            {
                Console.WriteLine("Enter your name: ");
                var name = Console.ReadLine();

                Console.WriteLine("Enter your gender: ");
                var gender = Console.ReadLine();

                Console.WriteLine("Enter your password: ");
                var sensitive = Console.ReadLine();


                var x = new MyObject()
                {
                    Id = "",
                    Name = name,
                    Sex = gender,
                    Sensitive = sensitive
                };
                x = proxy.Save(x).Result;
                proxy.Increment(x.Id, a => a.Age, new Random().Next(10)).Wait();
                proxy.Decrement(x.Id, a => a.Age, new Random().Next(10)).Wait();

                x = proxy.Fetch(key).Result;


                Console.WriteLine($"Total Count: {proxy.CountAll().Result}");
                Console.WriteLine($"Total Males: {proxy.CountAllWhere(a => a.Sex == "male").Result}");
                Console.WriteLine($"Total Females: {proxy.CountAllWhere(a => a.Sex == "female").Result}");

                #region collapsed
                //proxy.FetchAll().Subscribe(item => Console.WriteLine(item.Id));
                //proxy.FetchAllWhere(a => a.IntProp != 0).Subscribe(item => Console.WriteLine(item.Id));

                //Console.WriteLine(proxy.FetchAllWhere(a => a.Enum1 == Enum1.Enum1Value1));
                //Console.WriteLine(proxy.FetchAllWhere(a => a.Enum1 != Enum1.Enum1Value1));
                //Console.WriteLine(proxy.FetchAllWhere(a => a.BoolProp));
                //Console.WriteLine(proxy.FetchAllWhere(a => !a.BoolProp));
                //Console.WriteLine(proxy.FetchAllWhere(a => a.BoolProp == false && a.GuidProp == Guid.Empty));
                //Console.WriteLine(proxy.FetchAllWhere(a => a.DateTimeOffsetProp == new DateTimeOffset()));

                //Console.WriteLine(proxy.FetchAllWhere(a => !a.BoolProp));
                //Console.WriteLine(proxy.FetchAllWhere(a => a.BoolProp && a.BoolProp));
                //Console.WriteLine(proxy.FetchAllWhere(a => a.BoolProp || a.BoolProp));
                //Console.WriteLine(proxy.FetchAllWhere(a => !a.BoolProp && !a.BoolProp));
                //Console.WriteLine(proxy.FetchAllWhere(a => !a.BoolProp && a.BoolProp));
                //Console.WriteLine(proxy.FetchAllWhere(a => !a.BoolProp || !a.BoolProp));
                //Console.WriteLine(proxy.FetchAllWhere(a => a.BoolProp || !a.BoolProp));
                //Console.WriteLine(proxy.FetchAllWhere(a => a.BoolProp == true));
                //Console.WriteLine(proxy.FetchAllWhere(a => a.BoolProp != false));
                //Console.WriteLine(proxy.FetchAllWhere(a => a.BoolProp == true && a.BoolProp != false));
                //Console.WriteLine(proxy.FetchAllWhere(a => a.IntProp == 1));
                //Console.WriteLine(proxy.FetchAllWhere(a => a.LongProp == 1));
                //Console.WriteLine(proxy.FetchAllWhere(a => a.DecimalProp == 1));
                //Console.WriteLine(proxy.FetchAllWhere(a => a.StringProp == ""));
                //Console.WriteLine(proxy.FetchAllWhere(a => a.DateTimeOffsetProp == new DateTimeOffset()));
                //Console.WriteLine(proxy.FetchAllWhere(a => a.GuidProp == Guid.Empty));
                //Console.WriteLine(proxy.FetchAllWhere(a => a.StringProp == null));
                //Console.WriteLine(proxy.FetchAllWhere(a => a.StringProp == ""));
                //Console.WriteLine(proxy.FetchAllWhere(a => a.DateTimeOffsetProp == DateTimeOffset.Now));
                //Console.WriteLine(proxy.FetchAllWhere(a => a.Enum1 == Enum1.Enum1Value1));
                //Console.WriteLine(proxy.FetchAllWhere(a => a.Enum1 == Enum1.Enum1Value1 && a.DateTimeOffsetProp == DateTimeOffset.Now));
                //Console.WriteLine(proxy.FetchAllWhere(a => a.ByteArrayProp == new byte[0]));
                //Console.WriteLine(proxy.FetchAllWhere(a => a.ByteArrayProp == new byte[0] || a.GuidProp == Guid.Empty));
                #endregion

                key = Console.ReadKey().Key.ToString();
                Console.WriteLine(key);
            }


            Console.WriteLine("=============");
            Console.WriteLine("");

            Console.WriteLine("All Ids: ");
            proxy.FetchAll().Subscribe(item => Console.Write(item.Id));
            Console.WriteLine(".");

            Console.WriteLine("Male Ids: ");
            proxy.FetchAllWhere(a => a.Sex == "male").Subscribe(item => Console.Write(item.Id));
            Console.WriteLine(".");

            Console.WriteLine("Female Ids: ");
            proxy.FetchAllWhere(a => a.Sex == "female").Subscribe(item => Console.Write(item.Id));
            Console.WriteLine(".");
            Console.WriteLine("=============");


            //proxy.FetchAllWhere(a => a.IntProp != 0).Subscribe(item => Console.WriteLine(item.Id));

            Console.WriteLine("Exiting...");
            Console.ReadLine();
        }

        [RoidPrefix("Database")]
        public class MyObject
        {
            [RoidKey]
            public string Id { get; set; }

            [RoidRequired]
            public string Name { get; set; }
            
            public int Age { get; set; }

            [RoidIndex]
            [RoidFieldName("Gender")]
            public string Sex { get; set; }
            
            [RoidIgnore(OnCreate:false)]
            public string Sensitive { get; set; }
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

            [RoidFieldName("CustomFieldName")]
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