namespace SharedCode.Service
{
    internal class OptimisticLockHostedService : BaseSimpleLockMechanismService
    {
        public OptimisticLockHostedService() : base(Constants.AcquireOptimisticLock) {}
    }
}
