using FluentAssertions;

namespace Events.Tests;

public class EventsTest
{
    record MyEvent(string Operation) : IEvent;

    record MyOtherEvent(string Operation) : IEvent;

    [Fact(DisplayName = "Event filters can be chained together")]
    public void EventFiltersCanBeChained()
    {
        var printing = new PrintingEvents();
        var capturing = new CapturingEvents();
        var json = new JsonEvents();

        var events = EventFilters
            .AddServiceName("MyService")
            .Then(EventFilters.AddEnvironment("Production"))
            .Then(EventFilters.AddTimestamp())
            .Then(printing.And(capturing).And(json));

        events.Emit(new MyEvent("Operation1"));

        capturing
            .Events.First()
            .Should()
            .BeOfType<MetadataEvent>()
            .Which.Metadata.Should()
            .Contain("ServiceName", "MyService")
            .And.Contain("Environment", "Production");
    }

    [Fact(DisplayName = "Event filters can be combined with predicates")]
    public void EventFiltersCanBeCombinedWithPredicates()
    {
        var printing = new PrintingEvents();
        var capturing = new CapturingEvents();
        var json = new JsonEvents();

        var events = EventFilters
            .AddServiceName("MyService")
            .Then(EventFilters.AddEnvironment("Production"))
            .Then(EventFilters.AddTimestamp())
            .Then(EventFilters.Accept(EventFilters.TypeIs<MyEvent>()))
            .Then(printing.And(capturing).And(json));

        events.Emit(new MyEvent("Operation1"));
        events.Emit(new MyOtherEvent("Operation2"));

        capturing.Events.Should().HaveCount(1);

        capturing
            .Events.First()
            .Should()
            .BeOfType<MetadataEvent>()
            .Which.Event.Should()
            .BeOfType<MyEvent>();
    }
}
