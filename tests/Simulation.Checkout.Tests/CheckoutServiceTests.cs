namespace Simulation.Checkout.Tests;

using AutoFixture;
using Bogus;
using Bogus.DataSets;
using Castle.Core.Resource;
using FluentAssertions;
using Moq;
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

        var service = new CheckoutService(new Mock<IOrderRepository>().Object, new Mock<IInventoryService>().Object,
            new Mock<INotificationService>().Object);

        // Act
        Func<Task> act =  () =>  service.CheckoutAsync(order);


        // Assert
        await act.Should().ThrowAsync<EmptyOrderException>();
    }
    // TASK 1.2
    [Theory]
    [InlineData("apple", 0)]
    [InlineData("orange", -1)]
    public async Task CheckoutAsync_InvalidQuantity_ShouldThrow(string name, int quantity)
    {
        // Arrange
        var service = new CheckoutService(new Mock<IOrderRepository>().Object, new Mock<IInventoryService>().Object,
            new Mock<INotificationService>().Object);

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
        Func<Task> act = () => service.CheckoutAsync(order);


        // Assert
        await act.Should().ThrowAsync<InvalidQuantityException>().WithMessage($"*{name}*");
    }
    // TASK 1.3
    [Fact]
    public async Task CheckoutAsync_NegativePrice_ShouldThrow()
    {
        // Arrange
        var service = new CheckoutService(new Mock<IOrderRepository>().Object, new Mock<IInventoryService>().Object,
            new Mock<INotificationService>().Object);

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
        var inventory = new Mock<IInventoryService>();
        inventory
            .Setup(x => x.IsAvailableAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = new CheckoutService(new Mock<IOrderRepository>().Object, inventory.Object,
            new Mock<INotificationService>().Object);

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
        var receipt = await service.CheckoutAsync(order);

        // Assert
        receipt.Subtotal.Should().Be(0m);
        receipt.Total.Should().Be(0m);
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
        var inventory = new Mock<IInventoryService>();
        inventory
            .Setup(x => x.IsAvailableAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = new CheckoutService(new Mock<IOrderRepository>().Object, inventory.Object,
            new Mock<INotificationService>().Object);

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
        var inventory = new Mock<IInventoryService>();
        inventory
            .Setup(x => x.IsAvailableAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = new CheckoutService(new Mock<IOrderRepository>().Object, inventory.Object,
            new Mock<INotificationService>().Object);

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
    // Task 2
    [Fact]
    public async Task CheckoutAsync_ValidOrderAndAllItemsInStock_SavesOrderAndSendsConfirmation()
    {
        // Arrange
        var orderRepo = new Mock<IOrderRepository>();
        var inventoryService = new Mock<IInventoryService>();
        var notificationService = new Mock<INotificationService>();

        inventoryService
            .Setup(x => x.IsAvailableAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = new CheckoutService(orderRepo.Object, inventoryService.Object, notificationService.Object);

        var order = new Order
        {
            CustomerName = "a",
            CustomerEmail = "a@gmail.com",
            Id = Guid.NewGuid(),
            Items = new List<OrderItem>
            {
                new OrderItem { ProductName = "apple", Quantity = 2, ProductId = Guid.NewGuid(), UnitPrice = 10m },
                new OrderItem { ProductName = "orange", Quantity = 1, ProductId = Guid.NewGuid(), UnitPrice = 5m }
            }
        };

        // Act
        var receipt = await service.CheckoutAsync(order);

        // Assert
        orderRepo.Verify(x => x.SaveAsync(order, It.IsAny<CancellationToken>()), Times.Once());
        notificationService.Verify(x => x.SendOrderConfirmationAsync(receipt, It.IsAny<CancellationToken>()), Times.Once());
    }
    // TASK 2.2
    [Fact]
    public async Task CheckoutAsync_OneItemOutOfStock_ThrowsAndDoesNotSaveOrSend()
    {
        // Arrange
        var orderRepo = new Mock<IOrderRepository>();
        var inventoryService = new Mock<IInventoryService>();
        var notificationService = new Mock<INotificationService>();

        inventoryService
            .Setup(x => x.IsAvailableAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var service = new CheckoutService(orderRepo.Object, inventoryService.Object, notificationService.Object);

        var order = new Order
        {
            CustomerName = "a",
            CustomerEmail = "a@gmail.com",
            Id = Guid.NewGuid(),
            Items = new List<OrderItem>
            {
                new OrderItem { ProductName = "apple", Quantity = 1, ProductId = Guid.NewGuid(), UnitPrice = 5m }
            }
        };

        // Act
        Func<Task> act = () => service.CheckoutAsync(order);

        // Assert
        await act.Should().ThrowAsync<InsufficientInventoryException>();
        orderRepo.Verify(x => x.SaveAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never());
        notificationService.Verify(x => x.SendOrderConfirmationAsync(It.IsAny<CheckoutReceipt>(), It.IsAny<CancellationToken>()), Times.Never());
    }
    // TASK 2.3
    [Fact]
    public async Task CheckoutAsync_OrderQuantityEqualZero_ThrowsAndDoesNotSaveOrSend()
    {
        // Arrange
        var orderRepo = new Mock<IOrderRepository>();
        var inventoryService = new Mock<IInventoryService>();
        var notificationService = new Mock<INotificationService>();

        inventoryService
            .Setup(x => x.IsAvailableAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = new CheckoutService(orderRepo.Object, inventoryService.Object, notificationService.Object);

        var appleGuid = Guid.NewGuid();

        var order = new Order
        {
            CustomerName = "a",
            CustomerEmail = "a@gmail.com",
            Id = Guid.NewGuid(),
            Items = new List<OrderItem>
            {
                new OrderItem { ProductName = "apple", Quantity = 0, ProductId = appleGuid, UnitPrice = 5m }
            }
        };

        // Act
        Func<Task> act = () => service.CheckoutAsync(order);

        // Assert
        await act.Should().ThrowAsync<InvalidQuantityException>();
        orderRepo.Verify(x => x.SaveAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never());
        inventoryService.Verify(x => x.IsAvailableAsync(appleGuid, 0), Times.Never);
        notificationService.Verify(x => x.SendOrderConfirmationAsync(It.IsAny<CheckoutReceipt>(), It.IsAny<CancellationToken>()), Times.Never());
    }
    // Task 3.1
    [Fact]
    public async Task CheckoutAsync_ValidOrderQuantityAndPrice_CheckoutSucceeds()
    {
        // Arrange
        var fixture = new Fixture();
        var orderRepo = new Mock<IOrderRepository>();
        var inventoryService = new Mock<IInventoryService>();
        var notificationService = new Mock<INotificationService>();
        inventoryService
        .Setup(x => x.IsAvailableAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(true);

        fixture.Customize<OrderItem>(c => c
            .With(i => i.Quantity, () => 1)    // quantity > 0
            .With(i => i.UnitPrice, () => 0)); // price >= 0

        var order = fixture.Create<Order>();
        var service = new CheckoutService(orderRepo.Object, inventoryService.Object, notificationService.Object);

        // Act
        var receipt = await service.CheckoutAsync(order);

        // Assert
        receipt.Should().NotBeNull(); // if it returns the receipt , means it didnt fail or throw exception
    }
    // Task 3.2
    [Fact]
    public async Task CheckoutAsync_BogusCustomerWithFixedSeed_ReceiptShouldHaveSameNameAndEmail()
    {
        // Arrange
        var faker = new Faker();
        faker.Random = new Bogus.Randomizer(35);
        var customerName = faker.Name.FullName();
        var customerEmail = faker.Internet.Email();

        var orderRepo = new Mock<IOrderRepository>();
        var inventoryService = new Mock<IInventoryService>();
        var notificationService = new Mock<INotificationService>();
        inventoryService
            .Setup(x => x.IsAvailableAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = new CheckoutService(orderRepo.Object, inventoryService.Object, notificationService.Object);

        var order = new Order
        {
            CustomerName = customerName,
            CustomerEmail = customerEmail,
            Id = Guid.NewGuid(),
            Items = new List<OrderItem>
            {
                new OrderItem { ProductName = "apple", Quantity = 1, ProductId = Guid.NewGuid(), UnitPrice = 10m }
            }
        };

        // Act
        var receipt = await service.CheckoutAsync(order);

        // Assert
        receipt.CustomerName.Should().Be(customerName);
        receipt.CustomerEmail.Should().Be(customerEmail);
    }

}
