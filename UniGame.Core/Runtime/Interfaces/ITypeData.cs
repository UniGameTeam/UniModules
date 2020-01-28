namespace UniGreenModules.UniCore.Runtime.Interfaces
{
    using ObjectPool.Runtime.Interfaces;
    using UniRx;

    public interface ITypeData : 
        IPoolable, 
        IMessageBroker,
        IContextWriter,
        IValueContainerStatus, 
        IReadOnlyData
    {
    }
}