using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FinancesManager.Common
{
    public interface IOptions
    {
        string Name { get; }
    }
    
    public class Configuration
    {
        private static readonly string OptionsPath = $"{Path.GetFullPath("../../../")}/config.json";

        public static T Load<T>() where T: IOptions, new()
        {
            using var streamReader = new StreamReader(OptionsPath);
            using var jsonReader = new JsonTextReader(streamReader);
            var json = JToken.ReadFrom(jsonReader);
            return json[new T().Name].ToObject<T>();
        }
    }
}