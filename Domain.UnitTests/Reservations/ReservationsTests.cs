using Domain.Enums;
using Domain.Reservations;
using FluentAssertions;
using Xunit;

namespace Domain.UnitTests.Reservations
{
    public class ReservationTests
    {
        private Reservation CreateDefaultPendingReservation()
        {
            return new Reservation(
                id: Guid.NewGuid(),
                eventId: Guid.NewGuid(),
                quantity: 3,
                buyerName: "Juancho Rois",
                buyerEmail: "juancho.rois@mail.com"
            );
        }

        [Fact]
        public void Constructor_Should_InitializeReservationInPendingStatus_WhenCreated()
        {
            // Arrange
            Guid id = Guid.NewGuid();
            Guid eventId = Guid.NewGuid();
            int quantity = 4;
            string buyerName = "Silvestre Dangond";
            string buyerEmail = "silvestre@mail.com";

            // Act
            var reservation = new Reservation(id, eventId, quantity, buyerName, buyerEmail);

            // Assert
            reservation.Id.Should().Be(id);
            reservation.EventId.Should().Be(eventId);
            reservation.Quantity.Should().Be(quantity);
            reservation.BuyerName.Should().Be(buyerName);
            reservation.BuyerEmail.Should().Be(buyerEmail);

            // Reglas de estado inicial por defecto:
            reservation.Status.Should().Be(ReservationStatus.Pending);
            reservation.ReservationCode.Should().BeNull();
            reservation.CancelledAt.Should().BeNull();
            reservation.LostTickets.Should().BeFalse();
        }

        [Fact]
        public void Confirm_Should_UpdateStatusToConfirmed_AndSetReservationCode()
        {
            // Arrange
            var reservation = CreateDefaultPendingReservation();
            string expectedCode = "EV-123456";

            // Act
            reservation.Confirm(expectedCode);

            // Assert
            reservation.Status.Should().Be(ReservationStatus.Confirmed);
            reservation.ReservationCode.Should().Be(expectedCode);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Cancel_Should_UpdateStatusToCancelled_AndSetCancellationDetails(bool expectedLostTickets)
        {
            // Arrange
            var reservation = CreateDefaultPendingReservation();
            DateTime beforeCancellation = DateTime.UtcNow;

            // Act
            reservation.Cancel(lostTickets: expectedLostTickets);

            // Assert
            reservation.Status.Should().Be(ReservationStatus.Cancelled);
            reservation.LostTickets.Should().Be(expectedLostTickets);

            // Validamos que la fecha de cancelación se haya asignado en el momento exacto
            reservation.CancelledAt.Should().NotBeNull();
            reservation.CancelledAt.Value.Should().BeOnOrAfter(beforeCancellation);
            reservation.CancelledAt.Value.Should().BeOnOrBefore(DateTime.UtcNow);
        }
    }
}