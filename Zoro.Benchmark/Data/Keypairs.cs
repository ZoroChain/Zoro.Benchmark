using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Zoro.Benchmark.Data
{
    public enum DATA_FROM
    {
        FILE = 0,
        LIVE = 1
    }
    public class Keypairs : Dictionary<String, String>
    {
        public Keypairs(IConfigurationRoot config, DATA_FROM dataFrom)
        {
            if (dataFrom == DATA_FROM.FILE)
            {
                foreach (var kvp in File.ReadLines(config.GetSection("Data").GetValue<string>(("InputFile")))
                    .Select(line => line.Split(','))
                    .ToDictionary(data => data[0].Replace("'", ""), data => data[1].Replace("'", "")))
                        this.Add(kvp.Key, kvp.Value);
            }
        }
    }
}
