using Domain.Enums;
using Domain.Venues;

namespace Domain.Events
{
    public class Event
    {
        public Guid Id { get; private set; }

        public string Title { get; private set; }

        public string Description { get; private set; }

        public Guid VenueId { get; private set; }

        public Venue? Venue { get; private set; }

        public int MaxCapacity { get; private set; }

        public DateTime StartDate { get; private set; }

        public DateTime EndDate { get; private set; }

        public decimal TicketPrice { get; private set; }

        public EventType EventType { get; private set; }

        public EventState Status { get; private set; }

        protected Event() { }

        public Event(
            Guid id,
            string title,
            string description,
            Guid venueId,
            int maxCapacity,
            DateTime startDate,
            DateTime endDate,
            decimal ticketPrice,
            EventType eventType)
        {
            Id = id;
            Title = title;
            Description = description;
            VenueId = venueId;
            MaxCapacity = maxCapacity;
            StartDate = startDate;
            EndDate = endDate;
            TicketPrice = ticketPrice;
            EventType = eventType;
            Status = EventState.Active;
        }

        public void MarkAsCompleted()
        {
            Status = EventState.Completed;
        }
    }
}