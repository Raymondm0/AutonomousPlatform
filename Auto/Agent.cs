using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Winform_platform.Auto
{
    class Agent
    {

    }

    class Platform_Config
    {
        public static void record_reagent(string json_path, string pos, string reagent)
        {
            try
            {
                string reagent_json = File.ReadAllText(json_path);
                JObject json = JObject.Parse(reagent_json);
                json["Points"][pos]["name"] = reagent;
                File.WriteAllText(json_path, json.ToString(Formatting.Indented));
            }
            catch { }
        }
    }
}
