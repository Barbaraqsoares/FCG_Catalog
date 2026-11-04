using FCG_CatalogAPI.Application.UseCases;
using FCG_CatalogAPI.Domain.Entities;
using FCG_CatalogAPI.Domain.Events;
using FCG_CatalogAPI.Domain.Exceptions;
using FCG_CatalogAPI.Domain.Ports;
using Moq;
using Xunit;

namespace FCG_CatalogAPI.Tests;

public class UserGameUseCaseTests
{
    private readonly Mock<IUserGameRepository> _userGames = new();
    private readonly Mock<IGameRepository> _games = new();
    private readonly Mock<IEventPublisher> _publisher = new();
    private readonly UserGameUseCase _useCase;

    public UserGameUseCaseTests() => _useCase = new UserGameUseCase(
        _userGames.Object, _games.Object, _publisher.Object);

    [Fact]
    public async Task AddGameToLibraryAsync_PublishesOrderWithoutAddingGameToLibrary()
    {
        var game = TestData.Game(3, "Game", 45m);
        _games.Setup(x => x.GetByIdAsync(3, CancellationToken.None)).ReturnsAsync(game);
        _userGames.Setup(x => x.GetByUserAndGameAsync(1, 3, CancellationToken.None))
            .ReturnsAsync((UserGame?)null);
        _games.Setup(x => x.GetPriceByIdAsync(3, CancellationToken.None)).ReturnsAsync(45m);
        var result = await _useCase.AddGameToLibraryAsync(3, 1, CancellationToken.None);

        Assert.Equal(3, result.GameId);
        Assert.Equal("Game", result.GameName);
        Assert.Equal(45m, result.Price);
        Assert.Null(result.PurchasedAt);
        _userGames.Verify(x => x.AddAsync(It.IsAny<UserGame>(), It.IsAny<CancellationToken>()), Times.Never);
        _publisher.Verify(x => x.Publish(It.Is<OrderPlacedCommand>(message =>
            message.UserId == 1 && message.GameId == 3 && message.Price == 45m),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddGameToLibraryAsync_ThrowsWhenGameDoesNotExist()
    {
        _games.Setup(x => x.GetByIdAsync(404, CancellationToken.None)).ReturnsAsync((Game?)null);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _useCase.AddGameToLibraryAsync(404, 1, CancellationToken.None));

        Assert.Equal("Jogo não encontrado.", exception.Message);
        _userGames.Verify(x => x.AddAsync(It.IsAny<UserGame>(), It.IsAny<CancellationToken>()), Times.Never);
        _publisher.Verify(x => x.Publish(It.IsAny<OrderPlacedCommand>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task AddGameToLibraryAsync_ThrowsWhenGameIsAlreadyInLibrary()
    {
        _games.Setup(x => x.GetByIdAsync(4, CancellationToken.None)).ReturnsAsync(TestData.Game(4));
        _userGames.Setup(x => x.GetByUserAndGameAsync(1, 4, CancellationToken.None))
            .ReturnsAsync(new UserGame());

        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            _useCase.AddGameToLibraryAsync(4, 1, CancellationToken.None));

        Assert.Equal("O jogo já está na biblioteca do usuário.", exception.Message);
        _userGames.Verify(x => x.AddAsync(It.IsAny<UserGame>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessPaymentAsync_ApprovedPaymentAddsGameForPaymentUserWithoutRepublishingOrder()
    {
        var processedAt = new DateTime(2025, 4, 3, 2, 1, 0, DateTimeKind.Utc);
        var game = TestData.Game(3, "Game", 45m);
        var payment = new PaymentProcessedEvent(Guid.NewGuid(), 17, 3, 45.0m, "Approved", processedAt, null);
        _userGames.Setup(x => x.GetByUserAndGameAsync(17, 3, CancellationToken.None))
            .ReturnsAsync((UserGame?)null);
        _games.Setup(x => x.GetByIdAsync(3, CancellationToken.None)).ReturnsAsync(game);
        _userGames.Setup(x => x.AddAsync(It.IsAny<UserGame>(), CancellationToken.None))
            .ReturnsAsync((UserGame userGame, CancellationToken _) => userGame);

        await _useCase.ProcessPaymentAsync(payment, CancellationToken.None);

        _userGames.Verify(x => x.AddAsync(It.Is<UserGame>(entry =>
            entry.UserId == 17 && entry.GameId == 3 && entry.PurchasedAt == processedAt),
            CancellationToken.None), Times.Once);
        _publisher.Verify(x => x.Publish(It.IsAny<OrderPlacedCommand>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData("Declined")]
    [InlineData("Pending")]
    [InlineData("")]
    public async Task ProcessPaymentAsync_NonApprovedPaymentDoesNotChangeLibrary(string status)
    {
        var payment = new PaymentProcessedEvent(Guid.NewGuid(), 17, 3, 45.0m, status, DateTime.UtcNow, "reason");

        await _useCase.ProcessPaymentAsync(payment, CancellationToken.None);

        _userGames.Verify(x => x.GetByUserAndGameAsync(It.IsAny<int>(), It.IsAny<int>(),
            It.IsAny<CancellationToken>()), Times.Never);
        _userGames.Verify(x => x.AddAsync(It.IsAny<UserGame>(), It.IsAny<CancellationToken>()), Times.Never);
        _publisher.Verify(x => x.Publish(It.IsAny<OrderPlacedCommand>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessPaymentAsync_ApprovedPaymentIsIdempotentForExistingLibraryEntry()
    {
        var payment = new PaymentProcessedEvent(Guid.NewGuid(), 17, 3, 45.0m, "Approved", DateTime.UtcNow, null);
        _userGames.Setup(x => x.GetByUserAndGameAsync(17, 3, CancellationToken.None))
            .ReturnsAsync(new UserGame());

        await _useCase.ProcessPaymentAsync(payment, CancellationToken.None);

        _userGames.Verify(x => x.AddAsync(It.IsAny<UserGame>(), It.IsAny<CancellationToken>()), Times.Never);
        _publisher.Verify(x => x.Publish(It.IsAny<OrderPlacedCommand>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetUserLibraryAsync_MapsLibraryEntries()
    {
        var first = TestData.Game(1, "First", 12m);
        var second = TestData.Game(2, "Second", 24m);
        _userGames.Setup(x => x.GetByUserIdAsync(5, CancellationToken.None)).ReturnsAsync(
        [
            new UserGame { UserId = 5, GameId = 1, Game = first, PurchasedAt = new DateTime(2025, 3, 1) },
            new UserGame { UserId = 5, GameId = 2, Game = second, PurchasedAt = new DateTime(2025, 3, 2) }
        ]);

        var result = await _useCase.GetUserLibraryAsync(5, CancellationToken.None);

        Assert.Collection(result,
            item =>
            {
                Assert.Equal(1, item.GameId);
                Assert.Equal("First", item.GameName);
                Assert.Equal(12m, item.Price);
                Assert.Equal(new DateTime(2025, 3, 1), item.PurchasedAt);
            },
            item =>
            {
                Assert.Equal(2, item.GameId);
                Assert.Equal("Second", item.GameName);
                Assert.Equal(24m, item.Price);
            });
    }
}
