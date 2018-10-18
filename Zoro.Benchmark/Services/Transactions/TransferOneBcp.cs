
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ThinNeo;

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

            
            foreach (string toAddress in targetAddress)
            {
                //拼合约的汇编工具
                byte[] vmscript = null;
                using (ScriptBuilder sb = new ScriptBuilder())
                {
                    var array = new MyJson.JsonNode_Array();
                    array.AddArrayValue("(addr)" + address);
                    array.AddArrayValue("(addr)" + toAddress);
                    array.AddArrayValue("(int)" + sendCount);
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
                var url = HttpHelper.MakeRpcUrlPost("http://182.254.221.14:10332", "sendrawtransaction", out postdata, new MyJson.JsonNode_ValueString(rawdata));
                var result = await HttpHelper.HttpPost(url, postdata);
                MyJson.JsonNode_Object resJO = (MyJson.JsonNode_Object)MyJson.Parse(result);
                Console.WriteLine(resJO.ToString());
                Console.WriteLine(address + " " + toAddress + "  ");

                _logger.LogInformation("Transaxtion response: {0} {1}", txid, resJO.ToString());
            }
        }
    }
}
