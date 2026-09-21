using AdessoWorldLeague.Domain.Entities.Contants;

namespace AdessoWorldLeague.Domain.Tests.Entities;

public class DrawConstantsTests
{
    [Fact]
    public void MaxTeamsCount_ShouldBe32()
    {
        Assert.Equal(32, DrawConstants.MaxTeamsCount);
    }

    [Fact]
    public void AcceptableGroupCounts_ShouldBe4And8()
    {
        Assert.Equal([4, 8], DrawConstants.AcceptableGroupCounts);
    }

    [Fact]
    public void GroupNames_ShouldCoverLargestAcceptableGroupCount()
    {
        var largestGroupCount = DrawConstants.AcceptableGroupCounts.Max();

        Assert.True(DrawConstants.GroupNames.Length >= largestGroupCount,
            "Grup adı sayısı, izin verilen en büyük grup sayısını karşılamalı.");
    }

    [Fact]
    public void GroupNames_ShouldBeUnique()
    {
        Assert.Equal(DrawConstants.GroupNames.Length, DrawConstants.GroupNames.Distinct().Count());
    }

    [Fact]
    public void GroupNames_ShouldBeSingleUppercaseLetters()
    {
        // DrawGroupConfiguration GroupName sütununu HasMaxLength(1) ile sınırlar.
        Assert.All(DrawConstants.GroupNames, name =>
        {
            Assert.Single(name);
            Assert.Equal(name.ToUpperInvariant(), name);
        });
    }

    [Theory]
    [InlineData(4)]
    [InlineData(8)]
    public void MaxTeamsCount_ShouldBeDivisibleByEveryAcceptableGroupCount(int groupCount)
    {
        Assert.Equal(0, DrawConstants.MaxTeamsCount % groupCount);
    }
}
