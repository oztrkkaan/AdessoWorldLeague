using AdessoWorldLeague.Application.Behaviors;
using AdessoWorldLeague.Application.Features.MakeDraw;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using NSubstitute;

namespace AdessoWorldLeague.Application.Tests.Behaviors;

public class ValidationBehaviorTests
{
    private static readonly MakeDrawCommand ValidCommand = new("Kaan Öztürk", 4);
    private static readonly MakeDrawResponse Response = new(Guid.CreateVersion7(), []);

    [Fact]
    public async Task Handle_ShouldCallNext_WhenNoValidatorsRegistered()
    {
        var nextCalled = false;
        var behavior = CreateBehavior();

        var result = await behavior.Handle(ValidCommand, Next(() => nextCalled = true), CancellationToken.None);

        Assert.True(nextCalled);
        Assert.Same(Response, result);
    }

    [Fact]
    public async Task Handle_ShouldCallNext_WhenAllValidatorsPass()
    {
        var nextCalled = false;
        var behavior = CreateBehavior(PassingValidator(), PassingValidator());

        var result = await behavior.Handle(ValidCommand, Next(() => nextCalled = true), CancellationToken.None);

        Assert.True(nextCalled);
        Assert.Same(Response, result);
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenValidatorFails()
    {
        var behavior = CreateBehavior(FailingValidator(new ValidationFailure("GroupCount", "GroupCount geçersiz.")));

        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => behavior.Handle(ValidCommand, Next(), CancellationToken.None));

        var failure = Assert.Single(exception.Errors);
        Assert.Equal("GroupCount", failure.PropertyName);
        Assert.Equal("GroupCount geçersiz.", failure.ErrorMessage);
    }

    [Fact]
    public async Task Handle_ShouldNotCallNext_WhenValidationFails()
    {
        var nextCalled = false;
        var behavior = CreateBehavior(FailingValidator(new ValidationFailure("GroupCount", "GroupCount geçersiz.")));

        await Assert.ThrowsAsync<ValidationException>(
            () => behavior.Handle(ValidCommand, Next(() => nextCalled = true), CancellationToken.None));

        Assert.False(nextCalled);
    }

    [Fact]
    public async Task Handle_ShouldAggregateFailures_FromAllValidators()
    {
        var behavior = CreateBehavior(
            FailingValidator(new ValidationFailure("CreatorFullName", "CreatorFullName boş olamaz.")),
            FailingValidator(
                new ValidationFailure("GroupCount", "GroupCount geçersiz."),
                new ValidationFailure("GroupCount", "GroupCount 4 veya 8 olmalı.")));

        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => behavior.Handle(ValidCommand, Next(), CancellationToken.None));

        Assert.Equal(3, exception.Errors.Count());
    }

    [Fact]
    public async Task Handle_ShouldAggregateFailures_WhenOnlySomeValidatorsFail()
    {
        var behavior = CreateBehavior(
            PassingValidator(),
            FailingValidator(new ValidationFailure("GroupCount", "GroupCount geçersiz.")));

        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => behavior.Handle(ValidCommand, Next(), CancellationToken.None));

        Assert.Single(exception.Errors);
    }

    [Fact]
    public async Task Handle_ShouldRunEveryValidator_EvenWhenAnEarlierOneFails()
    {
        var first = FailingValidator(new ValidationFailure("CreatorFullName", "Hata"));
        var second = PassingValidator();
        var behavior = CreateBehavior(first, second);

        await Assert.ThrowsAsync<ValidationException>(
            () => behavior.Handle(ValidCommand, Next(), CancellationToken.None));

        await second.Received(1).ValidateAsync(
            Arg.Any<ValidationContext<MakeDrawCommand>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationTokenToValidators()
    {
        using var cts = new CancellationTokenSource();
        var validator = PassingValidator();
        var behavior = CreateBehavior(validator);

        await behavior.Handle(ValidCommand, Next(), cts.Token);

        await validator.Received(1).ValidateAsync(
            Arg.Any<ValidationContext<MakeDrawCommand>>(), cts.Token);
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationTokenToNext()
    {
        using var cts = new CancellationTokenSource();
        var behavior = CreateBehavior();
        CancellationToken observedToken = default;

        await behavior.Handle(ValidCommand, token =>
        {
            observedToken = token;
            return Task.FromResult(Response);
        }, cts.Token);

        Assert.Equal(cts.Token, observedToken);
    }

    [Fact]
    public async Task Handle_ShouldValidateTheIncomingRequestInstance()
    {
        var validator = PassingValidator();
        var behavior = CreateBehavior(validator);

        await behavior.Handle(ValidCommand, Next(), CancellationToken.None);

        await validator.Received(1).ValidateAsync(
            Arg.Is<ValidationContext<MakeDrawCommand>>(ctx => ReferenceEquals(ctx.InstanceToValidate, ValidCommand)),
            Arg.Any<CancellationToken>());
    }

    private static ValidationBehavior<MakeDrawCommand, MakeDrawResponse> CreateBehavior(
        params IValidator<MakeDrawCommand>[] validators)
        => new(validators);

    private static RequestHandlerDelegate<MakeDrawResponse> Next(Action? onCalled = null)
        => _ =>
        {
            onCalled?.Invoke();
            return Task.FromResult(Response);
        };

    private static IValidator<MakeDrawCommand> PassingValidator()
    {
        var validator = Substitute.For<IValidator<MakeDrawCommand>>();
        validator
            .ValidateAsync(Arg.Any<ValidationContext<MakeDrawCommand>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        return validator;
    }

    private static IValidator<MakeDrawCommand> FailingValidator(params ValidationFailure[] failures)
    {
        var validator = Substitute.For<IValidator<MakeDrawCommand>>();
        validator
            .ValidateAsync(Arg.Any<ValidationContext<MakeDrawCommand>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(failures));

        return validator;
    }
}
