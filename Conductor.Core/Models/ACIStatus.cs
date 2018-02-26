namespace Conductor.Core.Models
{
    public enum ACIStatus
    {
        None,
        Pending,
        Creating,
        Running,
        Succeeded,
        Failed,
        ProvisioningFailed,
        Other    
    }
}