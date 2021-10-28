using Newtonsoft.Json;
using System;
using System.IO;

namespace KAgent.Config
{
    internal class Settings
    {
        public String ip { get; set; }
        public int port { get; set; }
        public String id { get; set; }
        public String pw { get; set; }
        public String DatabaseName { get; set; }

        public string apiuri { get; set; } = "http://pooq.kbsmedia.co.kr/api/mcms";
        public string apiuri_dev { get; set; } = "http://kpooq.kbsmedia.co.kr/api/api.php";
        public string api_smr_uri { get; set; } = "http://smrapi.kbsmedia.co.kr/api/api.php";

        public String configFileName = "config.json";

        public static Settings instance = null;

        public static Settings GetInstance()
        {
            if (instance == null)
            {
                return new Settings();
            }
            return instance;
        }

        public void Save()
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.NullValueHandling = NullValueHandling.Ignore;

            using (StreamWriter sw = new StreamWriter(this.configFileName))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, this);
            }
        }
    }
}