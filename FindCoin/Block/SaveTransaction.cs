using FindCoin.core;
using FindCoin.thinneo;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FindCoin.Block
{
    class SaveTransaction : ISave
    {
        private static SaveTransaction instance = null;
        public static SaveTransaction getInstance()
        {
            if (instance == null)
            {
                return new SaveTransaction();
            }
            return instance;
        }

        public override void Save(JToken jObject, string path)
        {
            JObject result = new JObject();
            result["txid"] = jObject["txid"];
            result["size"] = jObject["size"];
            result["type"] = jObject["type"];
            result["version"] = jObject["version"];
            result["attributes"] = jObject["attributes"];
            result["vin"] = jObject["vin"];
            result["vout"] = jObject["vout"];
            result["sys_fee"] = jObject["sys_fee"];
            result["net_fee"] = jObject["net_fee"];
            result["scripts"] = jObject["scripts"];
            result["nonce"] = jObject["nonce"];
            result["blockindex"] = Helper.blockHeight;

            File.Delete(path);
            // File.WriteAllText(path, result.ToString(), Encoding.UTF8);

            // SaveAddress.getInstance().Save(result["vout"], null);

            SaveUTXO.getInstance().Save(result, null);

            var addressTransactionPath = "addressTransaction" + Path.DirectorySeparatorChar + result["txid"] + ".txt";
            // SaveAddressTransaction.getInstance().Save(result, addressTransactionPath);

            if (result["type"].ToString() == "RegisterTransaction")
            {
                if (Directory.Exists("asset") == false)
                {
                    Directory.CreateDirectory("asset");
                }
                var assetPath = "asset" + Path.DirectorySeparatorChar + result["txid"] + ".txt";
                // saveAsset(jObject, assetPath);
            }
            else if (result["type"].ToString() == "InvocationTransaction") {
                // SaveNotify.getInstance().Save(result, null);
            }
        }

        private void saveAsset(JToken jObject, string path)
        {
            JObject result = new JObject();
            result["version"] = jObject["version"];
            result["id"] = jObject["txid"];
            result["type"] = jObject["asset"]["type"];
            result["name"] = jObject["asset"]["name"];
            result["amount"] = jObject["asset"]["amount"];
            result["available"] = 1;
            result["precision"] = jObject["asset"]["precision"];
            result["owner"] = jObject["asset"]["owner"];
            result["admin"] = jObject["asset"]["admin"];
            result["issuer"] = 1;
            result["expiration"] = 0;
            result["frozen"] = 0;

            File.Delete(path);
            File.WriteAllText(path, result.ToString(), Encoding.UTF8);
        }
    }
}
