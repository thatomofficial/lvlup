using System.Reflection;
using NetArchTest.Rules;
using Shouldly;

namespace LvlUp.ArchitectureTests;

public class HandlerTests
{
    private static readonly Assembly ApplicationAssembly = typeof(Application.DependencyInjection).Assembly;

    [Fact]
    public void CommandHandlers_Should_BeSealed()
    {
        TestResult result = Types.InAssembly(ApplicationAssembly)
            .That()
            .HaveNameEndingWith("CommandHandler", StringComparison.Ordinal)
            .Should()
            .BeSealed()
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void QueryHandlers_Should_BeSealed()
    {
        TestResult result = Types.InAssembly(ApplicationAssembly)
            .That()
            .HaveNameEndingWith("QueryHandler", StringComparison.Ordinal)
            .Should()
            .BeSealed()
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void Validators_Should_NotBePublic()
    {
        TestResult result = Types.InAssembly(ApplicationAssembly)
            .That()
            .HaveNameEndingWith("Validator", StringComparison.Ordinal)
            .ShouldNot()
            .BePublic()
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }
}
