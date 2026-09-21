using NetArchTest.Rules;

namespace AdessoWorldLeague.Architecture.Tests;

/// <summary>
/// Clean Architecture bağımlılık kuralları:
/// Api → Infrastructure → Application → Domain. Oklar tersine dönemez.
/// </summary>
public class LayerDependencyTests
{
    [Fact]
    public void Domain_ShouldNotDependOnApplication()
    {
        var result = Types.InAssembly(ArchitectureAssemblies.Domain)
            .ShouldNot()
            .HaveDependencyOn(ArchitectureAssemblies.ApplicationNamespace)
            .GetResult();

        AssertArchitecture(result, "Domain katmanı Application katmanına bağımlı olmamalı.");
    }

    [Fact]
    public void Domain_ShouldNotDependOnInfrastructure()
    {
        var result = Types.InAssembly(ArchitectureAssemblies.Domain)
            .ShouldNot()
            .HaveDependencyOn(ArchitectureAssemblies.InfrastructureNamespace)
            .GetResult();

        AssertArchitecture(result, "Domain katmanı Infrastructure katmanına bağımlı olmamalı.");
    }

    [Fact]
    public void Domain_ShouldNotDependOnApi()
    {
        var result = Types.InAssembly(ArchitectureAssemblies.Domain)
            .ShouldNot()
            .HaveDependencyOn(ArchitectureAssemblies.ApiNamespace)
            .GetResult();

        AssertArchitecture(result, "Domain katmanı Api katmanına bağımlı olmamalı.");
    }

    [Fact]
    public void Application_ShouldNotDependOnInfrastructure()
    {
        var result = Types.InAssembly(ArchitectureAssemblies.Application)
            .ShouldNot()
            .HaveDependencyOn(ArchitectureAssemblies.InfrastructureNamespace)
            .GetResult();

        AssertArchitecture(result, "Application katmanı Infrastructure katmanına bağımlı olmamalı.");
    }

    [Fact]
    public void Application_ShouldNotDependOnApi()
    {
        var result = Types.InAssembly(ArchitectureAssemblies.Application)
            .ShouldNot()
            .HaveDependencyOn(ArchitectureAssemblies.ApiNamespace)
            .GetResult();

        AssertArchitecture(result, "Application katmanı Api katmanına bağımlı olmamalı.");
    }

    [Fact]
    public void Infrastructure_ShouldNotDependOnApi()
    {
        var result = Types.InAssembly(ArchitectureAssemblies.Infrastructure)
            .ShouldNot()
            .HaveDependencyOn(ArchitectureAssemblies.ApiNamespace)
            .GetResult();

        AssertArchitecture(result, "Infrastructure katmanı Api katmanına bağımlı olmamalı.");
    }

    [Fact]
    public void Domain_ShouldNotReferenceAnyOtherProjectAssembly()
    {
        var referenced = ArchitectureAssemblies.ReferencedAssemblyNames(ArchitectureAssemblies.Domain);

        Assert.DoesNotContain(ArchitectureAssemblies.ApplicationNamespace, referenced);
        Assert.DoesNotContain(ArchitectureAssemblies.InfrastructureNamespace, referenced);
        Assert.DoesNotContain(ArchitectureAssemblies.ApiNamespace, referenced);
    }

    [Fact]
    public void Application_ShouldReferenceOnlyDomain_AmongProjectAssemblies()
    {
        var referenced = ArchitectureAssemblies.ReferencedAssemblyNames(ArchitectureAssemblies.Application);

        Assert.Contains(ArchitectureAssemblies.DomainNamespace, referenced);
        Assert.DoesNotContain(ArchitectureAssemblies.InfrastructureNamespace, referenced);
        Assert.DoesNotContain(ArchitectureAssemblies.ApiNamespace, referenced);
    }

    [Fact]
    public void Infrastructure_ShouldNotReferenceApi_AmongProjectAssemblies()
    {
        var referenced = ArchitectureAssemblies.ReferencedAssemblyNames(ArchitectureAssemblies.Infrastructure);

        Assert.Contains(ArchitectureAssemblies.ApplicationNamespace, referenced);
        Assert.DoesNotContain(ArchitectureAssemblies.ApiNamespace, referenced);
    }

    internal static void AssertArchitecture(TestResult result, string because)
    {
        var failingTypes = result.FailingTypeNames ?? [];

        Assert.True(result.IsSuccessful,
            $"{because}{Environment.NewLine}Kuralı ihlal eden tipler:{Environment.NewLine}" +
            string.Join(Environment.NewLine, failingTypes));
    }
}
