using FindCoin.core;
using FindCoin.thinneo;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FindCoin.Block
{
    class SaveAddressTransaction:ISave
    {
        private static SaveAddressTransaction instance = null;
        public static SaveAddressTransaction getInstance()
        {
            if (instance == null)
            {
                return new SaveAddressTransaction();
            }
            return instance;
        }

        public override void Save(JToken jObject, string path)
        {         
            if (Directory.Exists("addressTransaction") == false)
            {
                Directory.CreateDirectory("addressTransaction");
            }

            JObject result = new JObject();
            result["txid"] = jObject["txid"];
            result["blockindex"] = Helper.blockHeight;
            result["blocktime"] = Helper.blockTime;

            File.Delete(path);
            File.WriteAllText(path, result.ToString(), Encoding.UTF8);
        }
    }
}
