using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace Conductor.Core.Models
{
    public enum ResultStatus
    {
        Creating,
        Running,
        Success,
        Failed,
        Cancel
    }
}