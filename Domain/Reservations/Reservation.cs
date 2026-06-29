using Domain.Enums;

namespace Domain.Reservations
{
    public class Reservation
    {
        public Guid Id { get; private set; }

        public Guid EventId { get; private set; }

        public int Quantity { get; private set; }

        public string BuyerName { get; private set; }

        public string BuyerEmail { get; private set; }

        public ReservationStatus Status { get; private set; }

        public string? ReservationCode { get; private set; }

        public DateTime? CancelledAt { get; private set; }

        public bool LostTickets { get; private set; }

        public Reservation(
           Guid id,
           Guid eventId,
           int quantity,
           string buyerName,
           string buyerEmail)
        {
            Id = id;
            EventId = eventId;
            Quantity = quantity;
            BuyerName = buyerName;
            BuyerEmail = buyerEmail;
            Status = ReservationStatus.Pending;
        }

        public void Confirm(string reservationCode)
        {
            Status = ReservationStatus.Confirmed;
            ReservationCode = reservationCode;
        }

        public void Cancel(bool lostTickets)
        {
            Status = ReservationStatus.Cancelled;
            CancelledAt = DateTime.UtcNow;
            LostTickets = lostTickets;
        }
    }
}
