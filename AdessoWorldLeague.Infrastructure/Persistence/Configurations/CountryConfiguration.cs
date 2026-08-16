using AdessoWorldLeague.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdessoWorldLeague.Infrastructure.Persistence.Configurations
{
    public class CountryConfiguration : IEntityTypeConfiguration<Country>
    {
        public void Configure(EntityTypeBuilder<Country> builder)
        {
            builder.HasKey(m => m.Id);
           
            builder.Property(x => x.Name)
             .IsRequired()
             .HasMaxLength(100);

            builder.HasMany(e => e.Teams)
               .WithOne(e => e.Country)
               .HasForeignKey(e => e.CountryId)
               .OnDelete(DeleteBehavior.Restrict);

            builder.HasData(new Country(1, "Türkiye"), 
                            new Country(2, "Almanya"), 
                            new Country(3, "Belçika"), 
                            new Country(4, "Fransa"), 
                            new Country(5, "Hollanda"), 
                            new Country(6, "Portekiz"), 
                            new Country(7, "İtalya"),
                            new Country(8, "İspanya"));
        }
    }
}
