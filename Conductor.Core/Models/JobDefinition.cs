using System;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace Conductor.Core.Models
{
    public class JobDefinition : TableEntity
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool Enabled { get; set; }
        public string OS { get; set; }
        public double CPU { get; set; }
        public double Memory { get; set; }
        public bool Private { get; set; }
        public string Image { get; set; }
        public string EnvValues { get; set; }
        public JobDefinition()
        {}
        public JobDefinition(string name)
            :base("D", name)
        {
            Name = name;
        }
        public JobType GetJobType()
        {
            return (JobType)Enum.Parse(typeof(JobType), Type);
        }
        public virtual IDictionary<string, string> GetEnvValues()
        {
            return JsonConvert.DeserializeObject<IDictionary<string,string>>(EnvValues);
        }
    }
}