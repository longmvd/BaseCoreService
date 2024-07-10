using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace BaseCoreService.Common
{
    public class Utils
    {
        public static string GetStringQuery(string queryName, string filename = "Queries/Queries.json")
        {
            var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);

            var text = File.ReadAllText(fullPath);

            var queries = JObject.Parse(text);
            var stringQuery = (string)queries?[queryName];
            return stringQuery;
        }

        private static readonly Random _random = new();

        public static string GenerateSearchParam()
        {
            return $"@{RandomString(6)}";
        }

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
        }
    }
}
