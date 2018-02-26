using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace Conductor.Core.Models
{
    public class JobResult : TableEntity
    {
        public string Status { get; set; }
        public string ContainerName { get; set; }
        public DateTimeOffset? StartAt { get; set; }
        public DateTimeOffset? FinishAt { get; set; }
        public string LogUri { get; set; }

        public JobResult()
        {}
        public JobResult(string jobName)
            : this (jobName, DateTimeOffset.Now)
        {}
        public JobResult(string jobName, DateTimeOffset date)
            : base (jobName, (long.MaxValue - date.Ticks).ToString("d19"))
        {
        }

        public ResultStatus GetResultStatus()
        {
            return (ResultStatus)Enum.Parse(typeof(ResultStatus), Status);
        }
    }
}