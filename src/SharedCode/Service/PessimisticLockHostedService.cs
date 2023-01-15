using System.Net;

namespace SharedCode.Service
{
    internal class PessimisticLockHostedService : BackgroundService
    {
        FileStream? fileStream = null;
        bool _disposed;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            EnsureDeletedLockFiles();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    fileStream = new FileStream(Constants.PessimisticLockFileName, FileMode.OpenOrCreate, FileAccess.Read, FileShare.None, bufferSize: 1, FileOptions.DeleteOnClose);
                    Console.WriteLine($"File created {Constants.PessimisticLockFileName}, {Dns.GetHostName()} starts the work.");

                    for (int iteration = 0; iteration < Constants.Iterations; iteration++)
                    {
                        // Do the work whilst acquired lock on file level
                        Console.WriteLine($"Pod with host name {Dns.GetHostName()} is iterating sequence {iteration} out of {Constants.Iterations} with pessimistic lock type and wait time {Constants.TaskPauseInMilliseconds}.");
                        await Task.Delay(Constants.TaskPauseInMilliseconds, stoppingToken);
                    }

                    fileStream.Dispose();
                    File.Delete(Constants.PessimisticLockFileName);
                    await Task.Delay(Constants.PodWaitToAcquireLockInMilliseconds, stoppingToken);
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

            fileStream?.Close();
            fileStream?.Dispose();
            File.Delete(Constants.PessimisticLockFileName);

            _disposed = true;
        }
    }
}
