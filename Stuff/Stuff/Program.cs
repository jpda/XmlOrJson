using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace Stuff
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var o = new Ok();

            var count = 100000;
            var times = 5;
            Console.WriteLine("How many things?");
            var tryCount = Console.ReadLine();
            if (!string.IsNullOrEmpty(tryCount))
            {
                count = int.Parse(tryCount);
            }

            Console.WriteLine("How many times?");
            var tryTimes = Console.ReadLine();
            if (!string.IsNullOrEmpty(tryTimes))
            {
                int.TryParse(tryTimes, out times);
            }

            o.DoStuff(count, times);
            Console.ReadLine();
        }
    }

    public class Ok
    {
        public void DoStuff(int objectcount, int intervals = 1)
        {
            Log.Write($"Generating {objectcount} objects and running {intervals} times...");
            for (var i = 0; i < intervals; i++)
            {
                //Log.Write($"Generating {objectcount} objects...");
                var d = GenFakeData<SerializeMe>(objectcount);
                //Log.Write($"Generated {d.Count} objects");
                var sampleData = new SerializeAllThe { Things = d };
                Serialize(sampleData);
                Console.WriteLine("---------------");
            }
        }

        private void Test()
        {
            //var serializers = new List<Func<>>() { };
        }

        private static void Serialize(SerializeAllThe things)
        {
            var xml = new XmlSerializer(typeof(SerializeAllThe));
            var dcs = new DataContractSerializer(typeof(SerializeAllThe));
            var json = JsonSerializer.Create();
            var sw = new Stopwatch();

            using (var s = new MemoryStream())
            using (var j = new MemoryStream())
            using (var x = new MemoryStream())
            {
                sw.Restart();
                dcs.WriteObject(s, things);
                sw.Stop();
                Log.Write($"XML-DCS: {sw.ElapsedMilliseconds}ms ({s.Length / sw.ElapsedMilliseconds}b/s)");

                sw.Restart();
                xml.Serialize(x, things);
                sw.Stop();
                Log.Write($"XML-SER: {sw.ElapsedMilliseconds}ms ({x.Length / sw.ElapsedMilliseconds}b/s)");

                sw.Restart();
                //json.Serialize(new JsonTextWriter(new StreamWriter(j)), things);

                var jd = JsonConvert.SerializeObject(things);
                sw.Stop();
                //Log.Write($"WARN: Not using stream");
                Log.Write($"jsonnet: {sw.ElapsedMilliseconds}ms ({jd.Length / sw.ElapsedMilliseconds}b/s)");
                

                Deserialize(s, x, jd);
            }
        }

        private static void Deserialize(Stream xml, Stream xmls, string json)
        {
            Console.WriteLine("Deserializing...");
            var sw = new Stopwatch();

            var xmlser = new XmlSerializer(typeof(SerializeAllThe));
            var dcs = new DataContractSerializer(typeof(SerializeAllThe));
            var jsons = JsonSerializer.Create();

            xml.Position = 0;
            sw.Start();
            dcs.ReadObject(xml);
            sw.Stop();
            Log.Write($"XML-DCS: {sw.ElapsedMilliseconds}ms");

            xmls.Position = 0;
            sw.Start();
            xmlser.Deserialize(xmls);
            sw.Stop();
            Log.Write($"XML-SER: {sw.ElapsedMilliseconds}ms");

            //json.Position = 0;
            sw.Restart();
            //jsons.Deserialize(new JsonTextReader(new StreamReader(json)));
            JsonConvert.DeserializeObject(json);
            sw.Stop();
            Log.Write($"jsonnet: {sw.ElapsedMilliseconds}ms");
        }

        private static List<T> GenFakeData<T>(int count = 100) where T : new()
        {
            var fields = typeof(T).GetFields();
            var things = new List<T>();
            Parallel.For(0, count, i =>
            {
                var r = new Random(i * DateTime.UtcNow.Millisecond);
                var o = new T();
                foreach (var f in fields)
                {
                    switch (f.FieldType.ToString())
                    {
                        case "System.Guid":
                            {
                                f.SetValue(o, Guid.NewGuid());
                                continue;
                            }
                        case "System.Int32":
                            {
                                f.SetValue(o, r.Next());
                                continue;
                            }
                        case "System.DateTime":
                            {
                                f.SetValue(o, DateTime.UtcNow.AddDays(r.Next(1000)));
                                continue;
                            }
                        //other types here
                        //...
                        //snip
                        default:
                            {
                                f.SetValue(o, $"words n stuff {DateTime.UtcNow.AddDays(r.Next(2425)).ToString("o")}");
                                break;
                            }
                    }
                }
                lock (things)
                {
                    things.Add(o);
                }
            });
            return things;
        }
    }

    public static class Log
    {
        public static void Write(string message)
        {
            Console.WriteLine($"{DateTime.UtcNow.ToString("o")}: {message}");
        }
    }

    public class SerializeAllThe
    {
        public List<SerializeMe> Things;
    }
    
    public class SerializeMe
    {
        public Guid Field0;
        public string Field1;
        public int Field2;
        public DateTime Field3;
        public string Field4;
        public string Field5;
        public string Field6;
        public int Field7;
        public DateTime Field8;
        public string Field9;
    }
}
