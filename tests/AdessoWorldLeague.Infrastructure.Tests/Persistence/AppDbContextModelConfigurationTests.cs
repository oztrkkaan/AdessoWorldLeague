using AdessoWorldLeague.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace AdessoWorldLeague.Infrastructure.Tests.Persistence;

public class AppDbContextModelConfigurationTests : IClassFixture<SqliteAppDbContextFixture>
{
    private readonly SqliteAppDbContextFixture _fixture;

    public AppDbContextModelConfigurationTests(SqliteAppDbContextFixture fixture) => _fixture = fixture;

    [Theory]
    [InlineData(typeof(Country), nameof(Country.Id))]
    [InlineData(typeof(Team), nameof(Team.Id))]
    [InlineData(typeof(Draw), nameof(Draw.Id))]
    [InlineData(typeof(DrawGroup), nameof(DrawGroup.Id))]
    [InlineData(typeof(DrawTeamAssignment), nameof(DrawTeamAssignment.Id))]
    public void Model_ShouldConfigureExpectedPrimaryKey(Type entityType, string keyPropertyName)
    {
        var primaryKey = GetEntityType(entityType).FindPrimaryKey();

        Assert.NotNull(primaryKey);
        Assert.Equal([keyPropertyName], primaryKey.Properties.Select(p => p.Name));
    }

    [Theory]
    [InlineData(typeof(Country), nameof(Country.Name), 100)]
    [InlineData(typeof(Draw), nameof(Draw.CreatorFullName), 200)]
    [InlineData(typeof(DrawGroup), nameof(DrawGroup.GroupName), 1)]
    public void Model_ShouldConfigureRequiredPropertyWithMaxLength(Type entityType, string propertyName, int maxLength)
    {
        var property = GetEntityType(entityType).FindProperty(propertyName);

        Assert.NotNull(property);
        Assert.False(property.IsNullable);
        Assert.Equal(maxLength, property.GetMaxLength());
    }

    [Fact]
    public void Model_ShouldConfigureUniqueIndexOnDrawIdAndGroupName()
    {
        var index = GetEntityType(typeof(DrawGroup)).GetIndexes()
            .SingleOrDefault(i => i.Properties.Select(p => p.Name)
                .SequenceEqual([nameof(DrawGroup.DrawId), nameof(DrawGroup.GroupName)]));

        Assert.NotNull(index);
        Assert.True(index.IsUnique);
    }

    [Fact]
    public void Model_ShouldRestrictDeleteFromCountryToTeam()
    {
        var foreignKey = GetForeignKey(typeof(Team), nameof(Team.CountryId));

        Assert.Equal(typeof(Country), foreignKey.PrincipalEntityType.ClrType);
        Assert.Equal(DeleteBehavior.Restrict, foreignKey.DeleteBehavior);
        Assert.False(foreignKey.IsRequired == false, "Team.CountryId zorunlu bir yabancı anahtar olmalı.");
    }

    [Fact]
    public void Model_ShouldCascadeDeleteFromDrawToDrawGroup()
    {
        var foreignKey = GetForeignKey(typeof(DrawGroup), nameof(DrawGroup.DrawId));

        Assert.Equal(typeof(Draw), foreignKey.PrincipalEntityType.ClrType);
        Assert.Equal(DeleteBehavior.Cascade, foreignKey.DeleteBehavior);
    }

    [Fact]
    public void Model_ShouldCascadeDeleteFromDrawGroupToDrawTeamAssignment()
    {
        var foreignKey = GetForeignKey(typeof(DrawTeamAssignment), nameof(DrawTeamAssignment.DrawGroupId));

        Assert.Equal(typeof(DrawGroup), foreignKey.PrincipalEntityType.ClrType);
        Assert.Equal(DeleteBehavior.Cascade, foreignKey.DeleteBehavior);
    }

    [Fact]
    public void Model_ShouldRestrictDeleteFromTeamToDrawTeamAssignment()
    {
        var foreignKey = GetForeignKey(typeof(DrawTeamAssignment), nameof(DrawTeamAssignment.TeamId));

        Assert.Equal(typeof(Team), foreignKey.PrincipalEntityType.ClrType);
        Assert.Equal(DeleteBehavior.Restrict, foreignKey.DeleteBehavior);
    }

    [Fact]
    public void Model_ShouldApplyConfigurationsFromInfrastructureAssemblyOnly()
    {
        using var context = _fixture.CreateContext();

        // Yapılandırmalar Infrastructure assembly'sinden yüklenir; Domain sınıflarında
        // hiçbir EF Core özniteliği/yapılandırması bulunmamalıdır.
        var configurationTypes = typeof(AdessoWorldLeague.Infrastructure.Persistence.AppDbContext).Assembly
            .GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false }
                        && t.GetInterfaces().Any(i => i.IsGenericType
                            && i.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>)))
            .ToList();

        Assert.Equal(5, configurationTypes.Count);
    }

    private IEntityType GetEntityType(Type clrType)
    {
        using var context = _fixture.CreateContext();

        var entityType = context.Model.FindEntityType(clrType);
        Assert.NotNull(entityType);

        return entityType;
    }

    private IForeignKey GetForeignKey(Type clrType, string foreignKeyPropertyName)
    {
        var foreignKey = GetEntityType(clrType).GetForeignKeys()
            .SingleOrDefault(fk => fk.Properties.Select(p => p.Name).SequenceEqual([foreignKeyPropertyName]));

        Assert.NotNull(foreignKey);

        return foreignKey;
    }
}
