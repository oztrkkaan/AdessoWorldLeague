using AdessoWorldLeague.Application.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetArchTest.Rules;
using static AdessoWorldLeague.Architecture.Tests.LayerDependencyTests;

namespace AdessoWorldLeague.Architecture.Tests;

/// <summary>
/// Her tipin doğru katmanda ve beklenen isimlendirme kuralıyla yer aldığını doğrular.
/// </summary>
public class LayerConventionTests
{
    [Fact]
    public void RequestHandlers_ShouldResideInApplicationLayer()
    {
        var result = Types.InAssembly(ArchitectureAssemblies.Application)
            .That()
            .ImplementInterface(typeof(IRequestHandler<,>))
            .Should()
            .ResideInNamespaceStartingWith($"{ArchitectureAssemblies.ApplicationNamespace}.Features")
            .GetResult();

        AssertArchitecture(result, "Komut/sorgu işleyicileri Application.Features altında olmalı.");
    }

    [Fact]
    public void RequestHandlers_ShouldBeNamedWithHandlerSuffix()
    {
        var result = Types.InAssembly(ArchitectureAssemblies.Application)
            .That()
            .ImplementInterface(typeof(IRequestHandler<,>))
            .Should()
            .HaveNameEndingWith("Handler")
            .GetResult();

        AssertArchitecture(result, "IRequestHandler uygulayan tipler 'Handler' ile bitmeli.");
    }

    [Fact]
    public void Validators_ShouldBeNamedWithValidatorSuffix()
    {
        var result = Types.InAssembly(ArchitectureAssemblies.Application)
            .That()
            .Inherit(typeof(FluentValidation.AbstractValidator<>))
            .Should()
            .HaveNameEndingWith("Validator")
            .GetResult();

        AssertArchitecture(result, "AbstractValidator türevleri 'Validator' ile bitmeli.");
    }

    [Fact]
    public void PipelineBehaviors_ShouldResideInBehaviorsNamespace()
    {
        var result = Types.InAssembly(ArchitectureAssemblies.Application)
            .That()
            .ImplementInterface(typeof(IPipelineBehavior<,>))
            .Should()
            .ResideInNamespace($"{ArchitectureAssemblies.ApplicationNamespace}.Behaviors")
            .GetResult();

        AssertArchitecture(result, "MediatR pipeline behavior'ları Application.Behaviors altında olmalı.");
    }

    [Fact]
    public void EntityTypeConfigurations_ShouldResideInInfrastructureLayer()
    {
        var result = Types.InAssembly(ArchitectureAssemblies.Infrastructure)
            .That()
            .ImplementInterface(typeof(IEntityTypeConfiguration<>))
            .Should()
            .ResideInNamespaceStartingWith($"{ArchitectureAssemblies.InfrastructureNamespace}.Persistence.Configurations")
            .GetResult();

        AssertArchitecture(result, "EF Core yapılandırmaları Infrastructure.Persistence.Configurations altında olmalı.");
    }

    [Fact]
    public void EntityTypeConfigurations_ShouldBeNamedWithConfigurationSuffix()
    {
        var result = Types.InAssembly(ArchitectureAssemblies.Infrastructure)
            .That()
            .ImplementInterface(typeof(IEntityTypeConfiguration<>))
            .Should()
            .HaveNameEndingWith("Configuration")
            .GetResult();

        AssertArchitecture(result, "EF Core yapılandırmaları 'Configuration' ile bitmeli.");
    }

    [Fact]
    public void DbContexts_ShouldResideInInfrastructureLayerOnly()
    {
        var applicationDbContexts = Types.InAssembly(ArchitectureAssemblies.Application)
            .That()
            .Inherit(typeof(DbContext))
            .GetTypes();

        Assert.Empty(applicationDbContexts);
    }

    [Fact]
    public void ApplicationDbContextAbstraction_ShouldBeImplementedByInfrastructure()
    {
        var implementations = ArchitectureAssemblies.Infrastructure.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false }
                        && typeof(IApplicationDbContext).IsAssignableFrom(t))
            .ToList();

        Assert.NotEmpty(implementations);
    }

    [Fact]
    public void Controllers_ShouldResideInApiLayer()
    {
        var result = Types.InAssembly(ArchitectureAssemblies.Api)
            .That()
            .Inherit(typeof(ControllerBase))
            .Should()
            .ResideInNamespace($"{ArchitectureAssemblies.ApiNamespace}.Controllers")
            .GetResult();

        AssertArchitecture(result, "Controller'lar Api.Controllers altında olmalı.");
    }

    [Fact]
    public void Controllers_ShouldBeNamedWithControllerSuffix()
    {
        var result = Types.InAssembly(ArchitectureAssemblies.Api)
            .That()
            .Inherit(typeof(ControllerBase))
            .Should()
            .HaveNameEndingWith("Controller")
            .GetResult();

        AssertArchitecture(result, "Controller'lar 'Controller' ile bitmeli.");
    }

    [Fact]
    public void Controllers_ShouldNotDependOnDbContext()
    {
        var result = Types.InAssembly(ArchitectureAssemblies.Api)
            .That()
            .Inherit(typeof(ControllerBase))
            .ShouldNot()
            .HaveDependencyOn($"{ArchitectureAssemblies.InfrastructureNamespace}.Persistence")
            .GetResult();

        AssertArchitecture(result,
            "Controller'lar veritabanına doğrudan değil, MediatR üzerinden erişmeli.");
    }

    [Fact]
    public void DomainEntities_ShouldResideInEntitiesNamespace()
    {
        var result = Types.InAssembly(ArchitectureAssemblies.Domain)
            .That()
            .ArePublic()
            .Should()
            .ResideInNamespaceStartingWith($"{ArchitectureAssemblies.DomainNamespace}.Entities")
            .GetResult();

        AssertArchitecture(result, "Domain tipleri Domain.Entities altında olmalı.");
    }

    [Fact]
    public void Commands_ShouldImplementMediatRRequest()
    {
        var commands = ArchitectureAssemblies.Application.GetTypes()
            .Where(t => t.Name.EndsWith("Command", StringComparison.Ordinal)
                        || t.Name.EndsWith("Query", StringComparison.Ordinal))
            .ToList();

        Assert.NotEmpty(commands);
        Assert.All(commands, command =>
            Assert.Contains(command.GetInterfaces(),
                i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>)));
    }

    [Fact]
    public void EveryCommand_ShouldHaveMatchingHandler()
    {
        var applicationTypes = ArchitectureAssemblies.Application.GetTypes();

        var commands = applicationTypes
            .Where(t => t.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>)))
            .ToList();

        Assert.NotEmpty(commands);

        foreach (var command in commands)
        {
            var hasHandler = applicationTypes.Any(t => t.GetInterfaces()
                .Any(i => i.IsGenericType
                          && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)
                          && i.GetGenericArguments()[0] == command));

            Assert.True(hasHandler, $"{command.Name} için IRequestHandler uygulaması bulunamadı.");
        }
    }

    [Fact]
    public void EveryCommand_ShouldHaveValidator()
    {
        var applicationTypes = ArchitectureAssemblies.Application.GetTypes();

        var commands = applicationTypes
            .Where(t => t.Name.EndsWith("Command", StringComparison.Ordinal)
                        && t.GetInterfaces().Any(i => i.IsGenericType
                            && i.GetGenericTypeDefinition() == typeof(IRequest<>)))
            .ToList();

        Assert.NotEmpty(commands);

        foreach (var command in commands)
        {
            var hasValidator = applicationTypes.Any(t =>
                t.BaseType is { IsGenericType: true }
                && t.BaseType.GetGenericTypeDefinition() == typeof(FluentValidation.AbstractValidator<>)
                && t.BaseType.GetGenericArguments()[0] == command);

            Assert.True(hasValidator, $"{command.Name} için bir AbstractValidator bulunamadı.");
        }
    }
}
