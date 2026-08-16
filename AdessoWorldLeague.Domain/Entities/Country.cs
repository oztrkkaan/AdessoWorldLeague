namespace AdessoWorldLeague.Domain.Entities
{
    public class Country
    {
        public Country( int id, string name)
        {
            Id = id;
            Name = name;
        }

        private Country() { }

        public int Id { get; init; }
        public string Name { get; init; } = null!;
        public ICollection<Team>? Teams { get; private set; } = [];
    }
}
