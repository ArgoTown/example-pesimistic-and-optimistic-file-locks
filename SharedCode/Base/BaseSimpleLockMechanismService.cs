using System.Net;

namespace SharedCode
{
    internal abstract class BaseSimpleLockMechanismService : BackgroundService, IDisposable
    {
        FileStream? fileStream = null;
        readonly string _lockType;
        bool _disposed;

        public BaseSimpleLockMechanismService(string lockType)
        {
            _lockType = lockType;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (_lockType.Equals(Constants.AcquirePesimisticLock))
                    {
                        // Try to aquire exclusive lock on a file, for other process this will throw error and catch section will receive it
                        fileStream = new FileStream(Constants.PesimisticLockFileName, FileMode.OpenOrCreate, FileAccess.Read, FileShare.None, bufferSize: 1, FileOptions.DeleteOnClose);
                        Console.WriteLine($"File created {Constants.PesimisticLockFileName}, {Dns.GetHostName()} starts the work");

                        for (int iteration = 0; iteration < Constants.Iterations; iteration++)
                        {
                            Console.WriteLine($"Pod with host name {Dns.GetHostName()} is iterating sequence {iteration} out of {Constants.Iterations} with locktype {_lockType}");
                        }

                        fileStream.Dispose();
                        File.Delete(Constants.PesimisticLockFileName);

                        // Add a delay to give ability for other pod to acquire the lock
                        await Task.Delay(Constants.PodWaitToAcquireLockInMilliseconds, stoppingToken);
                    }

                    if (_lockType.Equals(Constants.AcquireOptimisticLock))
                    {
                        if (!File.Exists(Constants.OptimisticLockFileName))
                        {
                            // Tries to create a file and continues to work
                            await File.Create(Constants.OptimisticLockFileName).DisposeAsync();
                            Console.WriteLine($"File created {Constants.OptimisticLockFileName}, {Dns.GetHostName()} starts the work");

                            for (int iteration = 0; iteration < Constants.Iterations; iteration++)
                            {
                                Console.WriteLine($"Pod with host name {Dns.GetHostName()} is iterating sequence {iteration} out of {Constants.Iterations} with locktype {_lockType}");
                            }

                            File.Delete(Constants.OptimisticLockFileName);
                        }
                        else
                        {
                            // Other pod sees that file exists, not throws hard error and can continue to work on other tasks
                            Console.WriteLine($"Optimistic lock is used, so host name {Dns.GetHostName()} skips the work");
                        }

                        await Task.Delay(Constants.PodWaitToAcquireLockInMilliseconds, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    await Task.Delay(Constants.PodWaitToAcquireLockInMilliseconds, stoppingToken);
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
            if (_disposed)
            {
                return;
            }

            if (_lockType.Equals(Constants.AcquirePesimisticLock))
            {
                fileStream?.Close();
                fileStream?.Dispose();
                File.Delete(Constants.PesimisticLockFileName);
            }

            if (_lockType.Equals(Constants.AcquireOptimisticLock))
            {
                File.Delete(Constants.OptimisticLockFileName);
            }

            _disposed = true;
        }
    }
}
