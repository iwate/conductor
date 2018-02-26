///
/// https://github.com/lixar/Lixar.Azure/blob/master/Lixar.Azure/Storage/CloudLock.cs
///
/// LICENSE: https://github.com/lixar/Lixar.Azure/blob/master/LICENSE.md
///
using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Lixar.Azure.Storage
{
    /// <summary>
    /// Implementation of a Cloud Lock that acquires and auto renews a Blob Lease.
    /// </summary>
    public class CloudLock : IDisposable
    {
        /// <summary>
        /// The minimum allowable timespan for acquiring a lease.
        /// </summary>
        public static readonly TimeSpan MinimumAcquireTimeSpan = TimeSpan.FromSeconds(15);

        /// <summary>
        /// The maximum allowable timespan for acquiring a lease.
        /// </summary>
        public static readonly TimeSpan MaximumAcquireTimeSpan = TimeSpan.FromSeconds(60);

        private CancellationTokenSource renewCancellationTokenSource;
        private bool disposed = false;

        public string LeaseId { get; private set; }
        public TimeSpan AcquireTimeSpan { get; private set; }
        public TimeSpan RenewTimeSpan { get; private set; }
        public ICloudBlob Blob { get; private set; }

        public CloudLock(ICloudBlob blob) : this(blob, TimeSpan.FromSeconds(30))
        {
        }

        public CloudLock(ICloudBlob blob, TimeSpan acquireTimeSpan):this(blob, acquireTimeSpan, TimeSpan.FromSeconds(acquireTimeSpan.TotalSeconds / 2))
        {
        }

        public CloudLock(ICloudBlob blob, TimeSpan acquireTimeSpan, TimeSpan renewTimeSpan)
        {
            if (blob == null)
            {
                throw new ArgumentNullException(nameof(blob));
            }

            if (acquireTimeSpan < MinimumAcquireTimeSpan || acquireTimeSpan > MaximumAcquireTimeSpan)
            {
                throw new ArgumentOutOfRangeException(nameof(acquireTimeSpan));
            }

            if (renewTimeSpan >= acquireTimeSpan)
            {
                throw new ArgumentOutOfRangeException(nameof(renewTimeSpan), "Renew TimeSpan must be less than the Acquire TimeSpan.");
            }

            Blob = blob;
            AcquireTimeSpan = acquireTimeSpan;
            RenewTimeSpan = renewTimeSpan;
        }
        public async Task<bool> TryAcquireAsync()
        {
            if (!string.IsNullOrEmpty(LeaseId))
            {
                throw new Exception("Lease has already been acquired.");
            }

            try
            {
                LeaseId = await Blob.AcquireLeaseAsync(AcquireTimeSpan);

                renewCancellationTokenSource = new CancellationTokenSource();

                //Run periodic renew  in the background
                RunPeriodicRenewTask((async () =>
                {
                    await Blob.RenewLeaseAsync(AccessCondition.GenerateLeaseCondition(LeaseId));
                }), RenewTimeSpan, renewCancellationTokenSource.Token);

                return true;
            }
            catch (StorageException storageException)
            {
                if (storageException.RequestInformation.HttpStatusCode == (int) HttpStatusCode.Conflict)
                {
                    return false;
                }
                throw;
            }
        }

        public Task<bool> AcquireAsync(int retryCount, TimeSpan delay)
        {
            return AcquireAsync(retryCount, delay, CancellationToken.None);
        }

        public async Task<bool> AcquireAsync(int retryCount, TimeSpan delay, CancellationToken cancellationToken)
        {
            if (retryCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(retryCount));
            }

            var retryAttempts = 0;
            while (true)
            {
                if (await TryAcquireAsync().ConfigureAwait(false))
                {
                    return true;
                }

                if (retryAttempts >= retryCount)
                {
                    return false;
                }

                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);

                retryAttempts++;
            }
        }

        public async Task ReleaseLeaseAsync()
        {
            try
            {
                if (!string.IsNullOrEmpty(LeaseId))
                {
                    renewCancellationTokenSource.Cancel();
                    Trace.TraceInformation($"Releasing Lease... {LeaseId}");
                    await Blob.ReleaseLeaseAsync(AccessCondition.GenerateLeaseCondition(LeaseId)).ConfigureAwait(false);
                    Trace.TraceInformation($"Released Lease. {LeaseId}");
                    LeaseId = null;
                }
            }
            catch (StorageException ex)
            {
                Trace.TraceError("Error occured trying to Release Lease.", ex);
            }
        }

        private static async Task RunPeriodicRenewTask(Func<Task> func, TimeSpan period, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(period, cancellationToken).ConfigureAwait(false);

                    if (!cancellationToken.IsCancellationRequested)
                    {
                        await func().ContinueWith((task) =>
                        {
                            if (task.IsFaulted)
                            {
                                Exception ex = task.Exception;
                                while (ex is AggregateException && ex.InnerException != null)
                                    ex = ex.InnerException;
                                Trace.TraceError($"Renew failed : {ex.StackTrace} ");
                            }
                            else if (task.IsCanceled)
                            {
                                Trace.TraceInformation("Renew Task was cancelled.");
                            }
                            else
                            {
                                Trace.TraceInformation("Renew completed.");
                            }
                        }, cancellationToken);
                    }
                }
                catch (TaskCanceledException)
                {
                    Trace.TraceInformation("Delay Task was cancelled.");
                    return;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    ReleaseLeaseAsync().Wait();
                }
                disposed = true;
            }
        }

        ~CloudLock()
        {
            Dispose(false);
        }
    }
}