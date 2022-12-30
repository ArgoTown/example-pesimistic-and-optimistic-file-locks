namespace SharedCode.Service
{
    internal class PessimisticLockHostedService : BaseSimpleLockMechanismService
    {
        public PessimisticLockHostedService() : base(Constants.AcquirePesimisticLock) { }
    }
}
