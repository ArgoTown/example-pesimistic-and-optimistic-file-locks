using System.Net;

namespace SharedCode.Service
{
    internal class OptimisticLockHostedService : BackgroundService
    {
        FileStream? fileStream = null;
        readonly string _lockType;
        bool _disposed;
        int _version;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            EnsureDeletedOptimisticLockFile();

            _version = 0;
            while (!stoppingToken.IsCancellationRequested && _version < 200)
            {
                try
                {
                    var fileNameWithVersion = $"{Constants.OptimisticLockFileName}_{_version}";
                    if (!File.Exists(fileNameWithVersion))
                    {
                        await File.Create(fileNameWithVersion).DisposeAsync();
                        for (int iteration = 0; iteration < Constants.Iterations; iteration++)
                        {
                            // Do the work whilst acquired lock on file level
                            Console.WriteLine($"Optimistic lock acquired for file: {fileNameWithVersion}");
                            Console.WriteLine($"Pod with host name {Dns.GetHostName()} is iterating sequence {iteration} out of {Constants.Iterations} with optimistic lock type and wait time {Constants.TaskPauseInMilliseconds}.");
                            await Task.Delay(Constants.TaskPauseInMilliseconds, stoppingToken);
                        }

                        File.Delete(fileNameWithVersion);
                    }
                    else
                    {
                        Console.WriteLine($"Optimistic lock is used, so host name {Dns.GetHostName()} skips the work and waits for {Constants.PodWaitToAcquireLockInMilliseconds} ms.");
                        await Task.Delay(Constants.PodWaitToAcquireLockInMilliseconds, stoppingToken);
                    }

                    _version++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception happened : {ex.Message}. Pod waiting for {Constants.PodWaitToAcquireLockInMilliseconds} ms. to retry acquiring the lock.");
                    await Task.Delay(Constants.PodWaitToAcquireLockInMilliseconds, stoppingToken);
                }
            }
        }

        static void EnsureDeletedOptimisticLockFile()
        {
            var optimisticLockFiles = Directory
                .GetFiles(Constants.Path)
                .Where(file => file.StartsWith(Constants.OptimisticLockFileName + "_"));

            foreach (var optimisticLockFile in optimisticLockFiles)
            {
                File.Delete(optimisticLockFile);
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

            EnsureDeletedOptimisticLockFile();
            _disposed = true;
        }
    }
}
