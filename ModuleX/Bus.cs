using Common.Enums;
using Common.Events;
using Common.Interfaces;
using Common.Models;
using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace ModuleX;

internal class Bus : IEventBus
{
    private readonly Subject<LogEvent> _logStream = new();
    private readonly Subject<ProgressEvent> _progressStream = new();
    private readonly ConcurrentDictionary<string, Subject<object>> _topics = new();

    public void Publish<T>(T @event)
    {
        if (@event is LogEvent log) _logStream.OnNext(log);
        else if (@event is ProgressEvent prog) _progressStream.OnNext(prog);
    }

    public IObservable<IList<LogEvent>> BufferedLogs =>
        _logStream.Buffer(TimeSpan.FromSeconds(2), 50)
            .Where(x => x.Count > 0);

    public IObservable<ProgressEvent> GetProgressStream(ModuleIdentity id) =>
        _progressStream.Where(p => p.Id == id)
            .Sample(TimeSpan.FromMilliseconds(50))
                .DistinctUntilChanged();

    public void PublishTopic(string topic, object data) =>
        _topics.GetOrAdd(topic, nw => new Subject<object>()).OnNext(data);

    public IDisposable SubscribeTopic(string topic, Action<object> handler) =>
        _topics.GetOrAdd(topic, nw => new Subject<object>()).Subscribe(handler);
}