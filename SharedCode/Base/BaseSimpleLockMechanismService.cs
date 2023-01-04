using System.Net;

namespace SharedCode
{
    internal abstract class BaseSimpleLockMechanismService : BackgroundService
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
            EnsureDeletedLockFiles();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (_lockType.Equals(Constants.AcquirePesimisticLock))
                    {
                        fileStream = new FileStream(Constants.PesimisticLockFileName, FileMode.OpenOrCreate, FileAccess.Read, FileShare.None, bufferSize: 1, FileOptions.DeleteOnClose);
                        Console.WriteLine($"File created {Constants.PesimisticLockFileName}, {Dns.GetHostName()} starts the work");

                        for (int iteration = 0; iteration < Constants.Iterations; iteration++)
                        {
                            // Do the work whilst acquired lock on file level
                            Console.WriteLine($"Pod with host name {Dns.GetHostName()} is iterating sequence {iteration} out of {Constants.Iterations} with locktype {_lockType} and wait time {Constants.TaskPauseInMilliseconds}");
                            await Task.Delay(Constants.TaskPauseInMilliseconds, stoppingToken);
                        }

                        fileStream.Dispose();
                        File.Delete(Constants.PesimisticLockFileName);
                        await Task.Delay(Constants.PodWaitToAcquireLockInMilliseconds, stoppingToken);
                    }

                    if (_lockType.Equals(Constants.AcquireOptimisticLock))
                    {
                        if (!File.Exists(Constants.OptimisticLockFileName))
                        {
                            await File.Create(Constants.OptimisticLockFileName).DisposeAsync();
                            Console.WriteLine($"File created {Constants.OptimisticLockFileName}, {Dns.GetHostName()} starts the work");

                            for (int iteration = 0; iteration < Constants.Iterations; iteration++)
                            {
                                // Do the work whilst acquired lock on file level
                                Console.WriteLine($"Pod with host name {Dns.GetHostName()} is iterating sequence {iteration} out of {Constants.Iterations} with locktype {_lockType} and wait time {Constants.TaskPauseInMilliseconds}");
                                await Task.Delay(Constants.TaskPauseInMilliseconds, stoppingToken);
                            }

                            File.Delete(Constants.OptimisticLockFileName);
                            await Task.Delay(Constants.PodWaitToAcquireLockInMilliseconds, stoppingToken);
                        }
                        else
                        {
                            Console.WriteLine($"Optimistic lock is used, so host name {Dns.GetHostName()} skips the work and waits for {Constants.PodWaitToAcquireLockInMilliseconds} ms.");
                            await Task.Delay(Constants.PodWaitToAcquireLockInMilliseconds, stoppingToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception happened : {ex.Message}. Pod waiting for {Constants.PodWaitToAcquireLockInMilliseconds} ms. to retry acquiring the lock.");
                    await Task.Delay(Constants.PodWaitToAcquireLockInMilliseconds, stoppingToken);
                }
            }
        }

        static void EnsureDeletedLockFiles()
        {
            if (File.Exists(Constants.PesimisticLockFileName))
            {
                File.Delete(Constants.PesimisticLockFileName);
            }
            if (File.Exists(Constants.OptimisticLockFileName))
            {
                File.Delete(Constants.OptimisticLockFileName);
            }
        }

        public override void Dispose()
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
