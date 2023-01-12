namespace SharedCode
{
    internal static class Constants
    {
        public const string Path = "/etc/data/";
        public const string PessimisticLockFileName = Path + "PessimisticLockFileName.lock";
        public const string OptimisticLockFileName = Path + "OptimisticLockFileName.lock";
        public const int Iterations = 100;
        public const int TaskPauseInMilliseconds = 25;
        public const int PodWaitToAcquireLockInMilliseconds = 50;
        public const string AcquireOptimisticLock = nameof(AcquireOptimisticLock);
        public const string AcquirePesimisticLock = nameof(AcquirePesimisticLock);
    }
}
