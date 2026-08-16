namespace AdessoWorldLeague.Application.Features.MakeDraw;

public record MakeDrawResponse(Guid DrawId, List<DrawGroupDto> Groups);

public record DrawGroupDto(string GroupName, List<string> Teams);
