using MediatR;

namespace AdessoWorldLeague.Application.Features.MakeDraw;

public record MakeDrawCommand(string CreatorFullName, int GroupCount) : IRequest<MakeDrawResponse>;
