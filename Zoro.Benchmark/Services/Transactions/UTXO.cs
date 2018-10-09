using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinNeo;

namespace Zoro.Benchmark.Services.Transactions
{
    public class UTXO
    {
        //txid[n] 是utxo的属性
        public Hash256 txid;
        public int n;

        //asset资产、addr 属于谁，value数额，这都是查出来的
        public string addr;
        public string asset;
        public decimal value;
        public UTXO(string _addr, Hash256 _txid, string _asset, decimal _value, int _n)
        {
            this.addr = _addr;
            this.txid = _txid;
            this.asset = _asset;
            this.value = _value;
            this.n = _n;
        }

        public static async Task<Dictionary<string, List<UTXO>>> GetByAddress(string dumpPath, string address)
        {
            Dictionary<string, List<UTXO>> dir = new Dictionary<string, List<UTXO>>();

            foreach (string filePath in Directory.GetFiles(dumpPath + "/utxo"))
            {
                if (filePath.Contains(address))
                {
                    using (var reader = File.OpenText(filePath))
                    {
                        JObject jObject = JObject.Parse(await reader.ReadToEndAsync());

                        UTXO utxo = new UTXO(jObject["addr"].ToString(),
                            new ThinNeo.Hash256(jObject["txid"].ToString()),
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
                        else
                        {
                            List<UTXO> l = new List<UTXO>();
                            l.Add(utxo);
                            dir[jObject["asset"].ToString()] = l;
                        }
                    }
                }
            }
            return dir;
        }
    }
}
