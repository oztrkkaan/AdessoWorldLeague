namespace AdessoWorldLeague.Application.Features.MakeDraw;

public record MakeDrawResponse(Guid DrawId, List<DrawGroupDto> Groups);

public record DrawGroupDto(string GroupName, List<DrawTeamDto> Teams);

public record DrawTeamDto(string Name);
