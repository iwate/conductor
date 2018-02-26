using System;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Conductor.Core.Models
{
    public class CronJobDefinition : JobDefinition
    {
        public string Cron { get; set; }
        public CronJobDefinition()
        {}
        public CronJobDefinition(string name)
            :base(name)
        {
            Name = name;
            Type = JobType.Cron.ToString();
        }
    }
}