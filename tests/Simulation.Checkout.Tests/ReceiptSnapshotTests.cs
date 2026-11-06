namespace Simulation.Checkout.Tests;

using Moq;
using Simulation.Checkout.Application;
using Simulation.Checkout.Application.Abstractions;
using Simulation.Checkout.Domain;
using VerifyXunit;
using Xunit;

public class ReceiptSnapshotTests
{
    // TASK 4.1
    [Fact]
    public async Task CheckoutAsync_ExampleC_ShouldMatchReceiptSnapshot()
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
            CustomerName = "Jane Doe",
            CustomerEmail = "jane.doe@example.com",
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Items = new List<OrderItem>
            {
                new OrderItem
                {
                    ProductName = "apple",
                    Quantity = 3,
                    ProductId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    UnitPrice = 500m
                }
            }
        };

        // Act
        var receipt = await service.CheckoutAsync(order);

        // Assert
        await Verifier.Verify(receipt)
            .IgnoreMember(nameof(CheckoutReceipt.CreatedAt));
    }
}
