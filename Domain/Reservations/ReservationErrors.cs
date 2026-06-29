using SharedKernel;

namespace Domain.Reservations
{
    public static class ReservationErrors
    {
        public static readonly Error NotEnoughTickets =
            Error.Conflict(
                "Reservations.NotEnoughTickets",
                "There are not enough tickets available.");

        public static readonly Error MaxFiveTickets =
            Error.Conflict(
                "Reservations.MaxFiveTickets",
                "A maximum of 5 tickets can be reserved when the event starts in less than 24 hours.");

        public static readonly Error InvalidQuantity =
            Error.Problem(
                "Reservations.InvalidQuantity",
                "The quantity must be greater than zero.");

        public static readonly Error InvalidEmail =
            Error.Problem(
                "Reservations.InvalidEmail",
                "The buyer email is invalid.");

        public static Error NotFound(Guid reservationId) =>
            Error.NotFound(
                "Reservations.NotFound",
                $"Reservation '{reservationId}' was not found.");

        public static readonly Error AlreadyConfirmed =
            Error.Conflict(
                "Reservations.AlreadyConfirmed",
                "The reservation has already been confirmed.");

        public static readonly Error AlreadyCancelled =
            Error.Conflict(
                "Reservations.AlreadyCancelled",
                "The reservation has been cancelled and cannot be confirmed.");

        public static readonly Error PendingPayment =
            Error.Conflict(
                "Reservations.PendingPayment",
                "Pending payment reservations cannot be cancelled.");


        public static readonly Error LateReservation =
            Error.Conflict(
                "Reservations.LateReservation",
                "Reservations are not allowed less than one hour before the event.");

        public static readonly Error MaxTenTickets =
            Error.Conflict(
                "Reservations.MaxTenTickets",
                "Events with a price greater than $100 allow a maximum of 10 tickets per transaction.");
    }
}