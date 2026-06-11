using System.Reflection;
using LvlUp.Api.Endpoints;
using NetArchTest.Rules;
using Shouldly;

namespace LvlUp.ArchitectureTests;

public class EndpointTests
{
    private static readonly Assembly ApiAssembly = typeof(IEndpoint).Assembly;

    [Fact]
    public void Endpoints_Should_BeSealed()
    {
        TestResult result = Types.InAssembly(ApiAssembly)
            .That()
            .ImplementInterface(typeof(IEndpoint))
            .Should()
            .BeSealed()
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void Endpoints_Should_NotBePublic()
    {
        TestResult result = Types.InAssembly(ApiAssembly)
            .That()
            .ImplementInterface(typeof(IEndpoint))
            .ShouldNot()
            .BePublic()
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }
}
