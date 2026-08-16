using AdessoWorldLeague.Domain.Entities;
using AdessoWorldLeague.Domain.Entities.Contants;
using FluentValidation;

namespace AdessoWorldLeague.Application.Features.MakeDraw;

public class MakeDrawCommandValidator : AbstractValidator<MakeDrawCommand>
{
    public MakeDrawCommandValidator()
    {
        RuleFor(x => x.CreatorFullName)
            .NotEmpty().WithMessage("CreatorFullName boş olamaz.")
            .MaximumLength(100).WithMessage("CreatorFullName en fazla 100 karakter olabilir.");

        RuleFor(x => x.GroupCount)
            .Must(count => DrawConstants.AcceptableGroupCounts.Contains(count))
            .WithMessage($"GroupCount yalnızca {string.Join(" veya ", DrawConstants.AcceptableGroupCounts)} olabilir.");
    }
}
