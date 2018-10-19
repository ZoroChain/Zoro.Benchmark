
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ThinNeo;
using System.Security.Cryptography;

namespace Zoro.Benchmark.Services.Transactions
{
    public class TransferOneBcp : IChainService
    {
        public TransferOneBcp(ILogger<IChainService> logger, IConfigurationRoot config) : base(logger, config)
        {
        }

        public string Name => "转账1Bcp";

        public string ID => "tran Bcp";
        
        public string ContractHash = "0x0920825f8e657a3a10d1aff4ac9b64d80c68a905";

        public string rpcurl = "http://182.254.221.14:10332";

        

        public int GetCount()
        {
            int count = 0;
            string getcounturl = rpcurl + "/?jsonrpc=2.0&id=1&method=getblockcount&params=[]";
            System.Net.WebClient wc = new System.Net.WebClient();
            try
            {
                var info = wc.DownloadString(getcounturl);
                var json = Newtonsoft.Json.Linq.JObject.Parse(info);
                count = (int)json["result"];
            } catch (Exception e){
                count = 0;
            }
            return count;
            
        }

        async public override Task Run(Dictionary<String, Object> args)
        {
            _logger.LogInformation("{0} Thread ID: {1}",
                this.GetType().ToString(), Thread.CurrentThread.ManagedThreadId);

            string wif = args["wif"].ToString();
            List<string> targetAddress = (List<string>)args["targetAddress"];
            decimal sendCount = Decimal.Parse(args["sendCount"].ToString());
            byte[] prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(wif);
            byte[] pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
            string address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);
            Hash160 scripthash = ThinNeo.Helper.GetPublicKeyHashFromAddress(address);

            int n = 0;
            foreach (string toAddress in targetAddress)
            {
                n += 1;
                //拼合约的汇编工具
                byte[] vmscript = null;
                byte[] randombytes = new byte[32];
                using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(randombytes);
                }
                BigInteger randomnum = new BigInteger(randombytes);

                using (ScriptBuilder sb = new ScriptBuilder())
                {
                    sb.EmitPushNumber(randomnum);
                    sb.Emit(ThinNeo.VM.OpCode.DROP);
                    var array = new MyJson.JsonNode_Array();
                    array.AddArrayValue("(addr)" + address);
                    array.AddArrayValue("(addr)" + toAddress);
                    array.AddArrayValue("(int)" + sendCount + "00000000");
                    sb.EmitParamJson(array);
                    sb.EmitPushString("transfer");
                    sb.EmitAppCall(new Hash160(ContractHash));
                    vmscript = sb.ToArray();
                }

                //拼装交易体
                ThinNeo.InvokeTransData extdata = new ThinNeo.InvokeTransData();
                extdata.script = vmscript;
                extdata.gas = 0;
                Transaction tran = new ThinNeo.Transaction();
                List<ThinNeo.TransactionInput> list_inputs = new List<ThinNeo.TransactionInput>();
                tran.inputs = list_inputs.ToArray();
                List<ThinNeo.TransactionOutput> list_outputs = new List<ThinNeo.TransactionOutput>();
                tran.outputs = list_outputs.ToArray();
                tran.version = 1;
                tran.extdata = extdata;
                //附加鉴证
                tran.attributes = new ThinNeo.Attribute[1];
                tran.attributes[0] = new ThinNeo.Attribute();
                tran.attributes[0].usage = ThinNeo.TransactionAttributeUsage.Script;
                // scripthash, 在公钥前后各加了一个字节, 是一个智能合约，将他反编译的话、
                // 就是：PushBytes[pubkey]; CheckSig; 这样两条指令。
                //当你访问你的账户的时候，比如用你的账户给别人转账，就会调用这个合约来验证
                tran.attributes[0].data = scripthash;
                tran.type = ThinNeo.TransactionType.InvocationTransaction;
                byte[] msg = tran.GetMessage();
                string msgstr = ThinNeo.Helper.Bytes2HexString(msg);
                byte[] signdata = ThinNeo.Helper.Sign(msg, prikey);
                tran.AddWitness(signdata, pubkey, address);
                string txid = tran.GetHash().ToString();
                Console.WriteLine(txid);
                byte[] data = tran.GetRawData();
                string rawdata = ThinNeo.Helper.Bytes2HexString(data);

                byte[] postdata;

                try
                {
                    var url = HttpHelper.MakeRpcUrlPost(rpcurl, "sendrawtransaction", out postdata, new MyJson.JsonNode_ValueString(rawdata));

                    var result = await HttpHelper.HttpPost(url, postdata);
                    var blockCount = GetCount();
                    MyJson.JsonNode_Object resJO = (MyJson.JsonNode_Object)MyJson.Parse(result);
                    var timeStamp = string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now);
                    _logger.LogInformation("{0} Transaction Number{1}: Transfer 1BCP From {2} to {3}, TXID={4}, CurrentBlockCount={5}, RPC result={6}",
                        timeStamp, n, address, toAddress, txid, blockCount, resJO.ToString());
                } catch (Exception e){
                    _logger.LogInformation("Transaction Number{0} Failed:", n);
                }
            }
        }
    }
}
