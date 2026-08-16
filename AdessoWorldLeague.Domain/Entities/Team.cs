namespace AdessoWorldLeague.Domain.Entities
{
    public class Team
    {
        public Team(string name, Country country)
        {
            Name = name;
            Country = country;
        }

        private Team() { }

        public int Id { get; init; }
        public string Name { get; init; } = null!;
        public int CountryId { get; init; }
        public Country Country { get; init; } = null!;
        public List<DrawGroup> Groups { get; private set; } = [];
        public List<DrawTeamAssignment> DrawTeamAssignments { get; private set; } = [];
    }
}
