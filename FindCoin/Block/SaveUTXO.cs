using FindCoin.core;
using FindCoin.thinneo;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FindCoin.Block
{
    class SaveUTXO:ISave
    {
        private static SaveUTXO instance = null;
        public static SaveUTXO getInstance()
        {
            if (instance == null)
            {
                return new SaveUTXO();
            }
            return instance;
        }

        public override void Save(JToken jObject, string path)
        {
            foreach (JObject vout in jObject["vout"]) {
                JObject result = new JObject();
                result["addr"] = vout["address"];
                result["txid"] = jObject["txid"];
                result["n"] = vout["n"];
                result["asset"] = vout["asset"];
                result["value"] = vout["value"];
                result["createHeight"] = Helper.blockHeight;
                result["used"] = 0;
                result["useHeight"] = "";
                result["claimed"] = "";

                var utxoPath = "utxo" + Path.DirectorySeparatorChar + result["txid"] + "_" + result["n"] + "_" + result["addr"] + ".txt";
                File.Delete(utxoPath);
                File.WriteAllText(utxoPath, result.ToString(), Encoding.UTF8);
            }
	        var curdir = Directory.GetCurrentDirectory();
            foreach (JObject vin in jObject["vin"])
            { 
                var inPath = "utxo" + Path.DirectorySeparatorChar + vin["txid"] + "_" + vin["vout"] + "*.txt";
		        foreach (string filePath in Directory.GetFiles(curdir + inPath))
		        {
                    if (File.Exists(filePath))
	    		        ChangeUTXO(filePath);
		        }
            }
        }

        public void ChangeUTXO(string path) {
            JObject result = JObject.Parse(File.ReadAllText(path, Encoding.UTF8));
            result["used"] = 1;
            result["useHeight"] = Helper.blockHeight;
            File.WriteAllText(path, result.ToString(), Encoding.UTF8);
        }

        public Dictionary<string, List<Utxo>> getUTXO(string address) {
            Dictionary<string, List<Utxo>> dir = new Dictionary<string, List<Utxo>>();
            var path = Directory.GetCurrentDirectory();
            
            foreach (string filePath in Directory.GetFiles(path + "/utxo")) {
                if (filePath.Contains(address)) {
                    JObject jObject = JObject.Parse(File.ReadAllText(filePath));
                    Utxo utxo = new Utxo(jObject["addr"].ToString(),
                        new Hash256((jObject["txid"].ToString())),
                        jObject["asset"].ToString(),
                        decimal.Parse(jObject["value"].ToString()),
                        int.Parse(jObject["n"].ToString()));
                    if (dir.ContainsKey(jObject["asset"].ToString()))
                    {
                        if (jObject["used"].ToString() == "0")
                        {
                            dir[jObject["asset"].ToString()].Add(utxo);
                        }
                    }
                    else {
                        List<Utxo> l = new List<Utxo>();
                        l.Add(utxo);
                        dir[jObject["asset"].ToString()] = l;
                    }                   
                }
            }
            return dir;
        }
    }

    class Utxo {
        public Hash256 txid;
        public int n;

        public string addr;
        public string asset;
        public decimal value;
        public Utxo(string _addr, Hash256 _txid, string _asset, decimal _value, int _n)
        {
            this.addr = _addr;
            this.txid = _txid;
            this.asset = _asset;
            this.value = _value;
            this.n = _n;
        }
    }
}
