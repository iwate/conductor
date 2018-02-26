namespace Conductor.Core.Services 
{
    public interface IJobRegistry
    {
        void Enqueue(string jobName);
        string Dequeue();
    }
}