using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace FindCoin.core
{
    abstract class ISave
    {
        public abstract void Save(JToken jToken, string path);
    }
}
