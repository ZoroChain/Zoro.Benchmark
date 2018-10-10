using FindCoin.core;
using FindCoin.thinneo;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FindCoin.Block
{
    class SaveAddress : ISave
    {

        private static SaveAddress instance = null;
        public static SaveAddress getInstance()
        {
            if (instance == null)
            {
                return new SaveAddress();
            }
            return instance;
        }

        public override void Save(JToken jObject, string path)
        {
            foreach (JObject j in jObject) {
                var addressPath = "address" + Path.DirectorySeparatorChar + j["address"] + ".txt";
                if (File.Exists(addressPath))
                {
                    JObject result = JObject.Parse(File.ReadAllText(addressPath, Encoding.UTF8));
                    result["lastuse"] = Helper.blockTime;
                    result["txcount"] = int.Parse(result["txcount"].ToString()) + 1;

                    File.WriteAllText(addressPath, result.ToString(), Encoding.UTF8);
                }
                else
                {
                    if (Directory.Exists("address") == false)
                    {
                        Directory.CreateDirectory("address");
                    }

                    JObject result = new JObject();
                    result["addr"] = j["address"];
                    result["firstuse"] = Helper.blockTime;
                    result["lastuse"] = Helper.blockTime;
                    result["txcount"] = 1;

                    File.Delete(addressPath);
                    File.WriteAllText(addressPath, result.ToString(), Encoding.UTF8);
                }
            }              
        }
    }
}
