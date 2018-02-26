using System;
using Conductor.Core.Models;

namespace Conductor.Web.Models
{
    public class ResultViewModel
    {
        public string JobName { get; set; }
        public string Key { get; set; }
        public string ContainerName { get; set; }
        public ResultStatus Status { get; set; }
        public DateTimeOffset? StartAt { get; set; }
        public DateTimeOffset? FinishAt { get; set; }
        public string Log { get; set; }
    }
}