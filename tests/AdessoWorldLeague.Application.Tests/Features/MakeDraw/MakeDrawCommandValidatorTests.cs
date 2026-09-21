using AdessoWorldLeague.Application.Features.MakeDraw;
using FluentValidation.TestHelper;

namespace AdessoWorldLeague.Application.Tests.Features.MakeDraw;

public class MakeDrawCommandValidatorTests
{
    private readonly MakeDrawCommandValidator _validator = new();

    [Theory]
    [InlineData(4)]
    [InlineData(8)]
    public void Validate_ShouldPass_ForValidCommand(int groupCount)
    {
        var result = _validator.TestValidate(new MakeDrawCommand("Kaan Öztürk", groupCount));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldFail_WhenCreatorFullNameIsEmpty(string? creatorFullName)
    {
        var result = _validator.TestValidate(new MakeDrawCommand(creatorFullName!, 4));

        result.ShouldHaveValidationErrorFor(x => x.CreatorFullName)
            .WithErrorMessage("CreatorFullName boş olamaz.");
    }

    [Fact]
    public void Validate_ShouldFail_WhenCreatorFullNameExceeds100Characters()
    {
        var result = _validator.TestValidate(new MakeDrawCommand(new string('a', 101), 4));

        result.ShouldHaveValidationErrorFor(x => x.CreatorFullName)
            .WithErrorMessage("CreatorFullName en fazla 100 karakter olabilir.");
    }

    [Fact]
    public void Validate_ShouldPass_WhenCreatorFullNameIsExactly100Characters()
    {
        var result = _validator.TestValidate(new MakeDrawCommand(new string('a', 100), 4));

        result.ShouldNotHaveValidationErrorFor(x => x.CreatorFullName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(7)]
    [InlineData(9)]
    [InlineData(16)]
    [InlineData(-4)]
    public void Validate_ShouldFail_WhenGroupCountIsNotAcceptable(int groupCount)
    {
        var result = _validator.TestValidate(new MakeDrawCommand("Kaan Öztürk", groupCount));

        result.ShouldHaveValidationErrorFor(x => x.GroupCount)
            .WithErrorMessage("GroupCount yalnızca 4 veya 8 olabilir.");
    }

    [Fact]
    public void Validate_ShouldReportBothErrors_WhenCommandIsFullyInvalid()
    {
        var result = _validator.TestValidate(new MakeDrawCommand(string.Empty, 3));

        result.ShouldHaveValidationErrorFor(x => x.CreatorFullName);
        result.ShouldHaveValidationErrorFor(x => x.GroupCount);
    }
}
