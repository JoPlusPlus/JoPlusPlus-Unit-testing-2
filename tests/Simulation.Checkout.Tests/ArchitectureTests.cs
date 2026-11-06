using FluentAssertions;
using NetArchTest.Rules;
using Simulation.Checkout.Application;
using Simulation.Checkout.Application.Abstractions;
using Simulation.Checkout.Domain;
using Simulation.Checkout.Infrastructure;


namespace Simulation.Checkout.Tests
{
    public class ArchitectureTests
    {
        // TASK 5.1
        [Fact]
        public void Domain_ShouldNotDependOnApplicationOrInfrastructure()
        {
            // Arrange
            var domain = typeof(OrderItem).Assembly;

            // Act
            var result = Types.InAssembly(domain)
                .ShouldNot()
                .HaveDependencyOnAny("Simulation.Checkout.Application", "Simulation.Checkout.Infrastructure")
                .GetResult();

            // Assert
            result.IsSuccessful.Should().BeTrue();
        }

        // TASK 5.2
        [Fact]
        public void Application_ShouldNotDependOnInfrastructure()
        {
            var application = typeof(CheckoutService).Assembly;

            var result = Types.InAssembly(application)
                 .ShouldNot()
                 .HaveDependencyOn("Simulation.Checkout.Infrastructure")
                 .GetResult();
            result.IsSuccessful.Should().BeTrue();
        }

        // TASK 5.3
        [Fact]
        public void Interfaces_ShouldStartWithI()
        {
            var assemblies = new[] { typeof(CheckoutService).Assembly, typeof(Order).Assembly, typeof(InMemoryOrderRepository).Assembly };

            var result = Types.InAssemblies(assemblies)
                .That()
                .AreInterfaces()
                .Should()
                .HaveNameStartingWith("I")
                .GetResult();

            result.IsSuccessful.Should().BeTrue();
        }

        // TASK 5.4
        [Fact]
        public void ClassesImplementingIOrderRepository_ShouldEndWithRepository()
        {
            // Arrange
            var assemblies = new[]
            {
            typeof(Order).Assembly,
            typeof(CheckoutService).Assembly,
            typeof(InMemoryOrderRepository).Assembly
        };

            // Act
            var result = Types.InAssemblies(assemblies)
                .That()
                .ImplementInterface(typeof(IOrderRepository))
                .Should()
                .HaveNameEndingWith("Repository")
                .GetResult();

            // Assert
            result.IsSuccessful.Should().BeTrue();
        }
    }
}
