using System.Reflection;

namespace AdessoWorldLeague.Architecture.Tests;

/// <summary>
/// Mimari testlerinde kullanılan katman assembly'leri ve namespace kökleri.
/// </summary>
public static class ArchitectureAssemblies
{
    public const string DomainNamespace = "AdessoWorldLeague.Domain";
    public const string ApplicationNamespace = "AdessoWorldLeague.Application";
    public const string InfrastructureNamespace = "AdessoWorldLeague.Infrastructure";
    public const string ApiNamespace = "AdessoWorldLeague.Api";

    public static Assembly Domain => typeof(Domain.Entities.Draw).Assembly;

    public static Assembly Application => typeof(Application.Abstractions.IApplicationDbContext).Assembly;

    public static Assembly Infrastructure => typeof(Infrastructure.Persistence.AppDbContext).Assembly;

    public static Assembly Api => typeof(Api.Controllers.DrawsController).Assembly;

    public static IReadOnlyList<string> ReferencedAssemblyNames(Assembly assembly)
        => assembly.GetReferencedAssemblies()
            .Select(a => a.Name!)
            .ToList();
}
