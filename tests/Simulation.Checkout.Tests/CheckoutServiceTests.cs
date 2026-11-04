namespace Simulation.Checkout.Tests;

using Bogus.DataSets;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.ReceivedExtensions;
using Simulation.Checkout.Application;
using Simulation.Checkout.Application.Abstractions;
using Simulation.Checkout.Domain;
using Simulation.Checkout.Domain.Exceptions;
using Xunit;

public class CheckoutServiceTests
{

    // TASK 1.1
    [Fact]
    public async Task CheckoutAsync_EmptyItemList_ShouldThrow()
    {
        // Arrange
        IReadOnlyList<OrderItem> Items = new List<OrderItem>();
        var order = new Order { CustomerName = "a", CustomerEmail = "a@gmail.com", Id = Guid.NewGuid(), Items = Items };

        var service = new CheckoutService(Substitute.For<IOrderRepository>(), Substitute.For<IInventoryService>(),
            Substitute.For<INotificationService>());

        // Act
        Func<Task> act =  () =>  service.CheckoutAsync(order);


        // Assert
        await act.Should().ThrowAsync<EmptyOrderException>();
    }
    // TASK 1.2
    [Theory]
    [InlineData("apple",0)]
    [InlineData("orange",-1)]
    public async Task CheckoutAsync_InvalidQuantity_ShouldThrow(string name, int quantity)
    {
        // Arrange
        var service = new CheckoutService(Substitute.For<IOrderRepository>(), Substitute.For<IInventoryService>(),
            Substitute.For<INotificationService>());

        var order = new Order
        {
            CustomerName = "a",
            CustomerEmail = "a@gmail.com",
            Id = Guid.NewGuid(),
            Items = new List<OrderItem>
            {
                new OrderItem
                {
                    ProductName = name,
                    Quantity = quantity,
                    ProductId = Guid.NewGuid(),
                    UnitPrice = 1m
                }
            }
        };



        // Act
        Func <Task> act = () => service.CheckoutAsync(order);


        // Assert
        await act.Should().ThrowAsync<InvalidQuantityException>(name);
    }
    // TASK 1.3
    [Fact]
    public async Task CheckoutAsync_NegativePrice_ShouldThrow()
    {
        // Arrange
        var service = new CheckoutService(Substitute.For<IOrderRepository>(), Substitute.For<IInventoryService>(),
            Substitute.For<INotificationService>());

        var order = new Order
        {
            CustomerName = "a",
            CustomerEmail = "a@gmail.com",
            Id = Guid.NewGuid(),
            Items = new List<OrderItem>
            {
                new OrderItem
                {
                    ProductName = "apple",
                    Quantity = 2,
                    ProductId = Guid.NewGuid(),
                    UnitPrice = -0.01m
                }
            }
        };

        // Act
        Func<Task> act = () => service.CheckoutAsync(order);


        // Assert
        await act.Should().ThrowAsync<InvalidPriceException>("apple");
    }
    // TASK 1.4
    [Fact]
    public async Task CheckoutAsync_ZeroPrice_ShouldNotThrow()
    {
        // Arrange
        var service = new CheckoutService(Substitute.For<IOrderRepository>(), Substitute.For<IInventoryService>(),
            Substitute.For<INotificationService>());

        var order = new Order
        {
            CustomerName = "a",
            CustomerEmail = "a@gmail.com",
            Id = Guid.NewGuid(),
            Items = new List<OrderItem>
            {
                new OrderItem
                {
                    ProductName = "apple",
                    Quantity = 2,
                    ProductId = Guid.NewGuid(),
                    UnitPrice = 0,
                }
            }
        };

        // Act
        Func<Task> act = () => service.CheckoutAsync(order);


        // Assert
        await act.Should().NotThrowAsync<InvalidPriceException>("apple");
    }
    // TASK 1.5

    public static TheoryData<decimal, decimal> PriceAndDiscount => new()    
    {
        { 999.99m,0m},
        {1000m,100m },
        {1500m,100m }
    };

    [Theory]
    [MemberData(nameof(PriceAndDiscount))]
    public async Task CheckoutAsync_DiscountForSubTotal_ShouldBe(decimal subTotal, decimal discount)
    {
        // Arrange
        var inventory = Substitute.For<IInventoryService>();
            inventory.IsAvailableAsync(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(true);

        var service = new CheckoutService(Substitute.For<IOrderRepository>(), inventory,
            Substitute.For<INotificationService>());

        var order = new Order
        {
            CustomerName = "a",
            CustomerEmail = "a@gmail.com",
            Id = Guid.NewGuid(),
            Items = new List<OrderItem>
            {
                new OrderItem
                {
                    ProductName = "apple",
                    Quantity = 1,
                    ProductId = Guid.NewGuid(),
                    UnitPrice = subTotal,
                }
            }
        };

        // Act
        var receipt = await service.CheckoutAsync(order);


        // Assert
        receipt.Subtotal.Should().Be(subTotal);
        receipt.Discount.Should().Be(discount);
    }
    // TASK 1.6

    public static TheoryData<int, decimal,int,decimal,decimal,decimal,decimal> QuantityAndPrice => new()
    {
        { 2,150,1,400,700,0,700},
        {3,500,1,0 ,1500,100,1400}, // work around , i added second item with zero price to match OrderItem1 format
    };

    [Theory]
    [MemberData(nameof(QuantityAndPrice))]
    public async Task CheckoutAsync_CalculateSubTotalAndDiscount_ShouldBe(int item1Quantity, decimal item1Price,
        int item2Quantity, decimal item2Price, decimal ExpectedSubtotal, decimal ExpectedDiscount, decimal ExpectedTotal)
    {
        // Arrange
        var inventory = Substitute.For<IInventoryService>();
        inventory.IsAvailableAsync(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(true);

        var service = new CheckoutService(Substitute.For<IOrderRepository>(), inventory,
            Substitute.For<INotificationService>());

        var order = new Order
        {
            CustomerName = "a",
            CustomerEmail = "a@gmail.com",
            Id = Guid.NewGuid(),
            Items = new List<OrderItem>
            {
                new OrderItem
                {
                    ProductName = "apple",
                    Quantity = item1Quantity,
                    ProductId = Guid.NewGuid(),
                    UnitPrice = item1Price,
                },
                new OrderItem
                {
                    ProductName = "orange",
                    Quantity = item2Quantity,
                    ProductId = Guid.NewGuid(),
                    UnitPrice = item2Price,
                }
            }
        };

        // Act
        var receipt = await service.CheckoutAsync(order);


        // Assert
        receipt.Subtotal.Should().Be(ExpectedSubtotal);
        receipt.Discount.Should().Be(ExpectedDiscount);
        receipt.Total.Should().Be(ExpectedTotal);
    }


}
