using System.Net;

namespace SharedCode
{
    internal abstract class BaseSimpleLockMechanismService : BackgroundService
    {
        FileStream? fileStream = null;
        readonly string _lockType;
        bool _disposed;
        public AsyncLocal<int> Version = new();

        public BaseSimpleLockMechanismService(string lockType)
        {
            _lockType = lockType;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            EnsureDeletedLockFiles();

            Version.Value = 0;
            while (!stoppingToken.IsCancellationRequested && Version.Value < 200)
            {
                try
                {
                    if (_lockType.Equals(Constants.AcquirePesimisticLock))
                    {
                        fileStream = new FileStream(Constants.PessimisticLockFileName, FileMode.OpenOrCreate, FileAccess.Read, FileShare.None, bufferSize: 1, FileOptions.DeleteOnClose);
                        Console.WriteLine($"File created {Constants.PessimisticLockFileName}, {Dns.GetHostName()} starts the work");

                        for (int iteration = 0; iteration < Constants.Iterations; iteration++)
                        {
                            // Do the work whilst acquired lock on file level
                            Console.WriteLine($"Pod with host name {Dns.GetHostName()} is iterating sequence {iteration} out of {Constants.Iterations} with locktype {_lockType} and wait time {Constants.TaskPauseInMilliseconds}");
                            await Task.Delay(Constants.TaskPauseInMilliseconds, stoppingToken);
                        }

                        fileStream.Dispose();
                        File.Delete(Constants.PessimisticLockFileName);
                        await Task.Delay(Constants.PodWaitToAcquireLockInMilliseconds, stoppingToken);
                    }

                    if (_lockType.Equals(Constants.AcquireOptimisticLock))
                    {
                        var fileNameWithVersion = $"{Constants.OptimisticLockFileName}_{Version.Value}";
                        if (!File.Exists(fileNameWithVersion))
                        {
                            await File.Create(fileNameWithVersion).DisposeAsync();
                            for (int iteration = 0; iteration < Constants.Iterations; iteration++)
                            {
                                // Do the work whilst acquired lock on file level
                                Console.WriteLine($"Optimistic lock acquired for file: {fileNameWithVersion}");
                                Console.WriteLine($"Pod with host name {Dns.GetHostName()} is iterating sequence {iteration} out of {Constants.Iterations} with locktype {_lockType} and wait time {Constants.TaskPauseInMilliseconds}");
                                await Task.Delay(Constants.TaskPauseInMilliseconds, stoppingToken);
                            }

                            File.Delete(fileNameWithVersion);
                        }
                        else
                        {
                            Console.WriteLine($"Optimistic lock is used, so host name {Dns.GetHostName()} skips the work and waits for {Constants.PodWaitToAcquireLockInMilliseconds} ms.");
                            await Task.Delay(Constants.PodWaitToAcquireLockInMilliseconds, stoppingToken);
                        }

                        Version.Value++;
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
            if (File.Exists(Constants.PessimisticLockFileName))
            {
                File.Delete(Constants.PessimisticLockFileName);
            }

            EnsureDeletedOptimisticLockFile();
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

            if (_lockType.Equals(Constants.AcquirePesimisticLock))
            {
                fileStream?.Close();
                fileStream?.Dispose();
                File.Delete(Constants.PessimisticLockFileName);
            }

            if (_lockType.Equals(Constants.AcquireOptimisticLock))
            {
                EnsureDeletedOptimisticLockFile();
            }

            _disposed = true;
        }
    }
}
