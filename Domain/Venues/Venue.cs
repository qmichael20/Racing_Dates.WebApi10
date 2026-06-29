namespace Domain.Venues
{
    public class Venue
    {
        public Guid Id { get; private set; }

        public string Name { get; private set; }

        public int Capacity { get; private set; }

        public string Ciudad { get; private set; }
    }
}
