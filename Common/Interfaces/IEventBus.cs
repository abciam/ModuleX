namespace Common.Interfaces;

public interface IEventBus
{
    void Publish<T>(T @event);
    void PublishTopic(string topic, object data);
    IDisposable SubscribeTopic(string topic, Action<object> handler);
}