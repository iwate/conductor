## Conductor
Conductor is job scheduler for Azure Container Instance.
It's inspired by AWS Lambda.

### Try it
[![Deploy to Azure](https://azuredeploy.net/deploybutton.png)](https://azuredeploy.net/)

You need create service principal before deploy.   
[https://docs.microsoft.com/cli/azure/create-an-azure-service-principal-azure-cli](https://docs.microsoft.com/cli/azure/create-an-azure-service-principal-azure-cli)

### Motivation
Azure Function is great services for FaaS application in the following points.

 - Fast wakeup (made by simply a process)
 - Shared process (and sahred static values)
 - Durable Function (can make async & scalable app easily)

However, in some case, the above fetures is obstacle.
Then I desire to execute function on a clean environment as possible.

### Feature

1. Queue Job: 
    Run the container when queue message count is greeter than 0.
2. Cron Job: 
    Run the container on scheduled time.

### Pros

1. Docker...

### Cons

1. It takes 2min ~ 10min when os is Windows :( <You must be joking!
    - It takes a few seconds when os is Linux :)
2. Expensive than Azure Function
    - Container Instance
    - WebApp instance for Conductor 

### Impression

You should execute cmd & exe on Azure Function if need executing function on a clean environment as possible.

Process and AppDomain is so great....

### Function Implementation

Container create cost is huge(time & money). You should make Queue job container applications as batch processing as the following.

This sample wait until 10min after last queue message. 

[queuesample](https://github.com/iwate/queuesample)
[DockerHub](https://hub.docker.com/r/iwate/queuesample)

```
static void Main(string[] args)
{
    var connectionString = Environment.GetEnvironmentVariable("ConnectionString");
    if (string.IsNullOrEmpty(connectionString))
    {
        Console.WriteLine("ConnectionString is not found");
        Environment.Exit(1);  
    }
    
    var queueName = Environment.GetEnvironmentVariable("Queue");
    if (string.IsNullOrEmpty(queueName))
    {
        Console.WriteLine("Queue is not found");
        Environment.Exit(1);  
    }
    CloudStorageAccount account;
    if (CloudStorageAccount.TryParse(connectionString, out account) == false)
    {
        Console.WriteLine("ConnectionString is invalid");
        Environment.Exit(1);
    }

    var queue = account.CreateCloudQueueClient().GetQueueReference(queueName);

    queue.CreateIfNotExistsAsync().Wait();

    ExecuteUntilAsync(async() => await ExecuteAllAsync(queue), i=>TimeSpan.FromMinutes(1), 10).Wait();
}

static async Task ExecuteUntilAsync(Func<Task<bool>> action, Func<int, TimeSpan> backoff, int loopMax)
{
    var count = 0;
    while(count < loopMax)
    {
        if (await action())
        {
            count = 0;
            continue;
        }
        await Task.Delay(backoff(++count));
    }
}

static async Task<bool> ExecuteAllAsync(CloudQueue queue)
{
    await queue.FetchAttributesAsync();
    if (queue.ApproximateMessageCount == 0)
        return false;

    for(var i = 0; i < queue.ApproximateMessageCount; i++)
    {
        var message = await queue.GetMessageAsync();
        if (Execute(message))
            await queue.DeleteMessageAsync(message);
    }
    return true;
}

static bool Execute(CloudQueueMessage message)
{
    Console.WriteLine(message.AsString);
    return true;
}
```
