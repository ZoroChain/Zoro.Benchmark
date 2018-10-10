using FindCoin.helper;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace FindCoin.core
{
    /// <summary>
    /// 合约任务
    /// 
    /// <para>
    /// 对合约处理的任务都需要继承该类
    /// </para>
    /// 
    /// </summary>
    abstract class ContractTask : IContractTask, ITask, INetType
    {
        /* 任务名称 */
        private string taskname;
        public string name()
        {
            return taskname;
        }

        protected ContractTask(string taskname)
        {
            this.taskname = taskname;
        }

        /// <summary>
        /// 初始化配置
        /// 
        /// </summary>
        /// <param name="config"></param>
        public void Init(JObject config)
        {
            //url = config["Url"].ToString();
            //startNetType = config["startNetType"].ToString();
            //try
            //{
            //    initConfig(config);
            //    LogHelper.printLog("InitTask success:" + name() + "_" + networkType());
            //}
            //catch (Exception ex)
            //{
            //    LogHelper.printLog("InitTask failed:" + name() + "_" + networkType() + ",exMsg:" + ex.Message);
            //}
        }
        public abstract void initConfig(JObject config);

        /// <summary>
        /// 启动任务
        /// 
        /// </summary>
        public void Start()
        {
            LogHelper.initThread(taskname);
            try
            {
                startTask();
            }
            catch (Exception ex)
            {
                LogHelper.printEx(ex);
            }
        }
        public abstract void startTask();

        private string url;
        public string getUrl() {         
            // return "http://182.254.221.14:10332";
            return "http://115.159.68.43:10332";
            if (networkType() == "testnet")
            {
                return url + ":20332/";
            }
            else {
                return url + ":10332/";
            }
        }
        /*启动网络类型*/
        private string startNetType;
        public string networkType()
        {
            return startNetType;
        }
    }
}
