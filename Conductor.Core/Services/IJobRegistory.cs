namespace Conductor.Core.Services 
{
    public interface IJobRegistory
    {
        void Enqueue(string jobName);
        string Dequeue();
    }
}