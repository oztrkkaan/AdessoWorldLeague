using NetArchTest.Rules;
using static AdessoWorldLeague.Architecture.Tests.LayerDependencyTests;

namespace AdessoWorldLeague.Architecture.Tests;

/// <summary>
/// İç katmanların altyapı/framework detaylarından yalıtıldığını doğrular.
/// </summary>
public class FrameworkIsolationTests
{
    [Theory]
    [InlineData("Microsoft.EntityFrameworkCore")]
    [InlineData("MediatR")]
    [InlineData("FluentValidation")]
    [InlineData("Microsoft.AspNetCore")]
    [InlineData("Microsoft.Extensions.DependencyInjection")]
    [InlineData("Swashbuckle")]
    public void Domain_ShouldNotDependOnAnyFramework(string frameworkNamespace)
    {
        var result = Types.InAssembly(ArchitectureAssemblies.Domain)
            .ShouldNot()
            .HaveDependencyOn(frameworkNamespace)
            .GetResult();

        AssertArchitecture(result, $"Domain katmanı '{frameworkNamespace}' bağımlılığına sahip olmamalı.");
    }

    [Fact]
    public void Domain_ShouldNotReferenceAnyThirdPartyPackage()
    {
        var allowedPrefixes = new[] { "System", "netstandard", "mscorlib" };

        var unexpected = ArchitectureAssemblies.ReferencedAssemblyNames(ArchitectureAssemblies.Domain)
            .Where(name => !allowedPrefixes.Any(prefix => name.StartsWith(prefix, StringComparison.Ordinal)))
            .ToList();

        Assert.True(unexpected.Count == 0,
            "Domain katmanı yalnızca BCL'e bağımlı olmalı. Beklenmeyen referanslar: " +
            string.Join(", ", unexpected));
    }

    [Theory]
    [InlineData("Microsoft.AspNetCore")]
    [InlineData("Swashbuckle")]
    public void Application_ShouldNotDependOnWebFramework(string frameworkNamespace)
    {
        var result = Types.InAssembly(ArchitectureAssemblies.Application)
            .ShouldNot()
            .HaveDependencyOn(frameworkNamespace)
            .GetResult();

        AssertArchitecture(result, $"Application katmanı '{frameworkNamespace}' bağımlılığına sahip olmamalı.");
    }

    [Fact]
    public void Application_ShouldNotDependOnSqlServerProvider()
    {
        var result = Types.InAssembly(ArchitectureAssemblies.Application)
            .ShouldNot()
            .HaveDependencyOn("Microsoft.EntityFrameworkCore.SqlServer")
            .GetResult();

        AssertArchitecture(result,
            "Application katmanı belirli bir veritabanı sağlayıcısına bağımlı olmamalı.");
    }

    [Fact]
    public void Application_ShouldAccessPersistenceThroughAbstractionOnly()
    {
        var result = Types.InAssembly(ArchitectureAssemblies.Application)
            .ShouldNot()
            .HaveDependencyOn($"{ArchitectureAssemblies.InfrastructureNamespace}.Persistence")
            .GetResult();

        AssertArchitecture(result,
            "Application katmanı AppDbContext yerine IApplicationDbContext soyutlamasını kullanmalı.");
    }

    [Fact]
    public void ApplicationHandlers_ShouldNotDependOnConcreteDbContext()
    {
        var handlers = ArchitectureAssemblies.Application.GetTypes()
            .Where(t => t.Name.EndsWith("Handler", StringComparison.Ordinal))
            .ToList();

        Assert.NotEmpty(handlers);

        foreach (var handler in handlers)
        {
            var constructorParameterTypes = handler.GetConstructors()
                .SelectMany(c => c.GetParameters())
                .Select(p => p.ParameterType)
                .ToList();

            Assert.All(constructorParameterTypes, parameterType =>
                Assert.False(
                    parameterType.Namespace?.StartsWith(ArchitectureAssemblies.InfrastructureNamespace, StringComparison.Ordinal) == true,
                    $"{handler.Name} somut bir Infrastructure tipine bağımlı olmamalı: {parameterType.FullName}"));
        }
    }
}
