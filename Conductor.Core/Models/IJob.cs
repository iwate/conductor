using System.Collections.Generic;
using System.Threading;

namespace Conductor.Core.Models
{
    public interface IJob
    {
        string Name { get; }
        void Start(IEnumerable<JobResult> recoveries, CancellationToken cancellationToken);
    }
}