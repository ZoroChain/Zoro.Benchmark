using FindCoin.core;
using FindCoin.thinneo;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FindCoin.Block
{
    class SaveBlock : ISave
    {
        private static SaveBlock instance = null;
        public  static SaveBlock getInstance() {
            if (instance == null) {
                return new SaveBlock();
            }
            return instance;
        }

        public override void Save(JToken jObject, string path)
        {
            JObject result = new JObject();
            result["hash"] = jObject["hash"];
            result["size"] = jObject["size"];
            result["version"] = jObject["version"];
            result["previousblockhash"] = jObject["previousblockhash"];
            result["merkleroot"] = jObject["merkleroot"];
            result["time"] = jObject["time"];
            result["index"] = jObject["index"];
            result["nonce"] = jObject["nonce"];
            result["nextconsensus"] = jObject["nextconsensus"];
            result["script"] = jObject["script"];

            Helper.blockTime = int.Parse(result["time"].ToString());

            File.Delete(path);
            File.WriteAllText(path, result.ToString(), Encoding.UTF8);

            foreach (var tx in jObject["tx"])
            {
                var txPath = "transaction" + Path.DirectorySeparatorChar + result["hash"] + ".txt";
                SaveTransaction.getInstance().Save(tx as JObject, txPath);
            }
        }
    }
}
