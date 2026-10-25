using FCG_CatalogAPI.Domain.Entities;
using FCG_CatalogAPI.Domain.Events;

namespace FCG_CatalogAPI.Tests;

public class DomainModelTests
{
    [Fact]
    public void User_StoresIdentityAndContactInformation()
    {
        var user = new User { Id = 6, Name = "User", Email = "user@example.test" };

        Assert.Equal(6, user.Id);
        Assert.Equal("User", user.Name);
        Assert.Equal("user@example.test", user.Email);
    }

    [Fact]
    public void PaymentProcessedEvent_StoresConstructorValues()
    {
        var processedAt = "DateTime.UtcNow";
        var payment = new PaymentProcessedEvent(Guid.NewGuid(), 1, 2, "12.5", "Approved", processedAt, null);

        Assert.NotEqual(Guid.Empty, payment.OrderId);
        Assert.Equal(1, payment.UserId);
        Assert.Equal(2, payment.GameId);
        Assert.Equal("12.5", payment.Price);
        Assert.Equal("Approved", payment.Status);
        Assert.Equal(processedAt, payment.ProcessedAtUtc);
        Assert.Null(payment.Reason);
    }

    [Fact]
    public void OrderPlacedCommand_StoresConstructorValues()
    {
        var order = new OrderPlacedCommand(1, 2, 12.5m);

        Assert.Equal(1, order.UserId);
        Assert.Equal(2, order.GameId);
        Assert.Equal(12.5m, order.Price);
    }

    [Fact]
    public void EntityBase_InitializesCreationTimestampAndAllowsUpdateTimestamp()
    {
        var entity = new EntityBase();
        var update = DateTime.UtcNow;

        Assert.NotEqual(default, entity.CreatedAt);
        Assert.Null(entity.UpdatedAt);

        entity.Id = 9;
        entity.UpdatedAt = update;

        Assert.Equal(9, entity.Id);
        Assert.Equal(update, entity.UpdatedAt);
    }
}
