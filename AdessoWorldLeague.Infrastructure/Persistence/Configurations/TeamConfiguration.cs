using AdessoWorldLeague.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdessoWorldLeague.Infrastructure.Persistence.Configurations;

public class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Name);

        builder.HasData(
        // Türkiye - CountryId: 1
        new { Id = 1, Name = "Adesso İstanbul", CountryId = 1 },
        new { Id = 2, Name = "Adesso Ankara", CountryId = 1 },
        new { Id = 3, Name = "Adesso İzmir", CountryId = 1 },
        new { Id = 4, Name = "Adesso Antalya", CountryId = 1 },

        // Almanya - CountryId: 2
        new { Id = 5, Name = "Adesso Berlin", CountryId = 2 },
        new { Id = 6, Name = "Adesso Frankfurt", CountryId = 2 },
        new { Id = 7, Name = "Adesso Münih", CountryId = 2 },
        new { Id = 8, Name = "Adesso Dortmund", CountryId = 2 },

        // Belçika - CountryId: 3
        new { Id = 9, Name = "Adesso Brüksel", CountryId = 3 },
        new { Id = 10, Name = "Adesso Brugge", CountryId = 3 },
        new { Id = 11, Name = "Adesso Anvers", CountryId = 3 },
        new { Id = 12, Name = "Adesso Gent", CountryId = 3 },

        // Fransa - CountryId: 4
        new { Id = 13, Name = "Adesso Paris", CountryId = 4 },
        new { Id = 14, Name = "Adesso Marsilya", CountryId = 4 },
        new { Id = 15, Name = "Adesso Nice", CountryId = 4 },
        new { Id = 16, Name = "Adesso Lyon", CountryId = 4 },

        // Hollanda - CountryId: 5
        new { Id = 17, Name = "Adesso Amsterdam", CountryId = 5 },
        new { Id = 18, Name = "Adesso Rotterdam", CountryId = 5 },
        new { Id = 19, Name = "Adesso Lahey", CountryId = 5 },
        new { Id = 20, Name = "Adesso Eindhoven", CountryId = 5 },

        // Portekiz - CountryId: 6
        new { Id = 21, Name = "Adesso Lisbon", CountryId = 6 },
        new { Id = 22, Name = "Adesso Porto", CountryId = 6 },
        new { Id = 23, Name = "Adesso Braga", CountryId = 6 },
        new { Id = 24, Name = "Adesso Coimbra", CountryId = 6 },

        // İtalya - CountryId: 7
        new { Id = 25, Name = "Adesso Roma", CountryId = 7 },
        new { Id = 26, Name = "Adesso Milano", CountryId = 7 },
        new { Id = 27, Name = "Adesso Venedik", CountryId = 7 },
        new { Id = 28, Name = "Adesso Napoli", CountryId = 7 },

        // İspanya - CountryId: 8
        new { Id = 29, Name = "Adesso Madrid", CountryId = 8 },
        new { Id = 30, Name = "Adesso Barselona", CountryId = 8 },
        new { Id = 31, Name = "Adesso Sevilla", CountryId = 8 },
        new { Id = 32, Name = "Adesso Valencia", CountryId = 8 }
    );
    }
}
