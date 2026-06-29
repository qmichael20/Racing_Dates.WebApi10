using SharedKernel;

namespace Domain.Venues
{
    public static class VenueErrors
    {
        public static Error NotFound(Guid venueId) =>
            Error.NotFound(
                "Venues.NotFound",
                $"The venue with id '{venueId}' was not found.");

        public static readonly Error InvalidCapacity =
            Error.Problem(
                "Venues.InvalidCapacity",
                "The venue capacity must be greater than zero.");

        public static readonly Error InvalidName =
            Error.Problem(
                "Venues.InvalidName",
                "The venue name is invalid.");

        public static readonly Error AlreadyExists =
            Error.Conflict(
                "Venues.AlreadyExists",
                "A venue with the same name already exists.");
    }
}