namespace SharedCode
{
    internal static class Constants
    {
        public const string PesimisticLockFileName = "/etc/data/PesimisticLockFileName.lock";
        public const string OptimisticLockFileName = "/etc/data/OptimisticLockFileName";
        public const int Iterations = 1000;
        public const int TaskPauseInMilliseconds = 300;
        public const int PodWaitToAcquireLockInMilliseconds = 1000;
        public const string AcquireOptimisticLock = nameof(AcquireOptimisticLock);
        public const string AcquirePesimisticLock = nameof(AcquirePesimisticLock);
    }
}
