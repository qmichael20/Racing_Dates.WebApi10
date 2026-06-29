using SharedKernel;

namespace Domain.Events
{
    public static class EventErrors
    {
        public static Error NotFound(Guid eventId) =>
            Error.NotFound(
                "Events.NotFound",
                $"The event with id '{eventId}' was not found.");

        public static readonly Error InvalidTitle =
            Error.Problem(
                "Events.InvalidTitle",
                "The title must contain between 5 and 100 characters.");

        public static readonly Error InvalidDescription =
            Error.Problem(
                "Events.InvalidDescription",
                "The description must contain between 10 and 500 characters.");

        public static readonly Error InvalidCapacity =
            Error.Problem(
                "Events.InvalidCapacity",
                "The event capacity cannot exceed the capacity of the selected venue.");

        public static readonly Error InvalidStartDate =
            Error.Problem(
                "Events.InvalidStartDate",
                "The start date must be in the future.");

        public static readonly Error InvalidEndDate =
            Error.Problem(
                "Events.InvalidEndDate",
                "The end date must be after the start date.");

        public static readonly Error InvalidTicketPrice =
            Error.Problem(
                "Events.InvalidTicketPrice",
                "The ticket price must be greater than zero.");

        public static Error EventAlreadyCompleted(Guid eventId) =>
            Error.Conflict(
                "Events.EventAlreadyCompleted",
                $"The event with id '{eventId}' has already been completed.");

        public static readonly Error OverlappingVenue =
            Error.Conflict(
                "Events.OverlappingVenue",
                "There is already an active event scheduled at this venue during the selected time.");

        public static readonly Error WeekendNightRestriction =
            Error.Conflict(
                "Events.WeekendNightRestriction",
                "Weekend events cannot start after 22:00.");
    }
}