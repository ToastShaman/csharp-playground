using System.Collections.Immutable;
using System.Text.Json;

namespace Events;

public interface IEvent { }

public interface IEvents
{
    public void Emit(IEvent e);
}

public static class Events
{
    public static IEvents Create(Action<IEvent> record) => new FuncEvents(record);
}

public static class EventsExtensions
{
    public static IEvents And(this IEvents first, IEvents second) =>
        Events.Create(e =>
        {
            first.Emit(e);
            second.Emit(e);
        });

    public static IEvents Then(this IEvents events, IEventFilter filter) => filter.Filter(events);

    public static IEvent AddMetadata(this IEvent e, string key, object value) =>
        (e is MetadataEvent me ? me : new MetadataEvent(e)).Add(key, value);
}

public interface IEventFilter
{
    public IEvents Filter(IEvents events);
}

public static class EventFilter
{
    public static IEventFilter Create(Func<IEvents, IEvents> f) => new FuncEventFilter(f);
}

public static class EventFilters
{
    public static IEventFilter AddServiceName(string serviceName) =>
        AddStatic("ServiceName", serviceName);

    public static IEventFilter AddEnvironment(string environment) =>
        AddStatic("Environment", environment);

    public static IEventFilter AddTimestamp() => AddStatic("Timestamp", DateTime.UtcNow);

    public static IEventFilter AddStatic(string key, object value) =>
        EventFilter.Create(events => Events.Create(e => events.Emit(e.AddMetadata(key, value))));

    public static IEventFilter Accept(Func<IEvent, bool> predicate) =>
        EventFilter.Create(events =>
            Events.Create(e =>
            {
                if (predicate(e))
                {
                    events.Emit(e);
                }
            })
        );

    public static IEventFilter Reject(Func<IEvent, bool> predicate) =>
        EventFilter.Create(events =>
            Events.Create(e =>
            {
                if (!predicate(e))
                {
                    events.Emit(e);
                }
            })
        );

    public static Func<IEvent, bool> TypeIs<T>() => e => e is T;
}

public static class EventFilterExtensions
{
    public static IEventFilter Then(this IEventFilter first, IEventFilter second) =>
        EventFilter.Create(events => second.Filter(first.Filter(events)));

    public static IEvents Then(this IEventFilter filter, IEvents events) =>
        Events.Create(e => filter.Filter(events).Emit(e));
}

public class FuncEvents : IEvents
{
    private readonly Action<IEvent> _record;

    public FuncEvents(Action<IEvent> record)
    {
        ArgumentNullException.ThrowIfNull(record);
        _record = record;
    }

    public void Emit(IEvent e) => _record(e);
}

public class FuncEventFilter : IEventFilter
{
    private readonly Func<IEvents, IEvents> _f;

    public FuncEventFilter(Func<IEvents, IEvents> f)
    {
        ArgumentNullException.ThrowIfNull(f);
        _f = f;
    }

    public IEvents Filter(IEvents events) => _f(events);
}

public record MetadataEvent : IEvent
{
    public IEvent Event { get; }

    public ImmutableDictionary<string, object> Metadata { get; }

    public MetadataEvent(IEvent Event, ImmutableDictionary<string, object> Metadata)
    {
        ArgumentNullException.ThrowIfNull(Event);
        ArgumentNullException.ThrowIfNull(Metadata);
        if (Event is MetadataEvent)
            throw new ArgumentException("Event cannot be a MetadataEvent", nameof(Event));

        this.Event = Event;
        this.Metadata = Metadata;
    }

    public MetadataEvent(IEvent e)
        : this(e, ImmutableDictionary<string, object>.Empty) { }

    public MetadataEvent Add(string key, object value) => new(Event, Metadata.Add(key, value));
};

public class PrintingEvents : IEvents
{
    public void Emit(IEvent e) => Console.WriteLine(e);
}

public class CapturingEvents : IEvents
{
    public List<IEvent> Events { get; private set; } = [];

    public void Emit(IEvent e) => Events.Add(e);
}

public class JsonEvents : IEvents
{
    public void Emit(IEvent e)
    {
        object obj = (e is MetadataEvent me) ? new { Event = (object)me.Event, me.Metadata } : e;
        Console.WriteLine(JsonSerializer.Serialize(obj));
    }
}
