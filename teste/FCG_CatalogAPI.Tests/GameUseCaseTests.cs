using FCG_CatalogAPI.Adapters.Inbound.Requests;
using FCG_CatalogAPI.Application.UseCases;
using FCG_CatalogAPI.Domain.Exceptions;
using FCG_CatalogAPI.Domain.Ports;
using Moq;
using Xunit;

namespace FCG_CatalogAPI.Tests;

public class GameUseCaseTests
{
    private readonly Mock<IGameRepository> _repository = new();
    private readonly GameUseCase _useCase;

    public GameUseCaseTests() => _useCase = new GameUseCase(_repository.Object);

    [Fact]
    public async Task GetAllAsync_MapsAllGames()
    {
        _repository.Setup(x => x.GetAllAsync(CancellationToken.None)).ReturnsAsync([TestData.Game(), TestData.Game(2, "Second")]);

        var result = (await _useCase.GetAllAsync(CancellationToken.None)).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal("Game", result[0].Name);
        Assert.Equal("Second", result[1].Name);
        Assert.Equal(25m, result[0].Price);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsMappedGame()
    {
        _repository.Setup(x => x.GetByIdAsync(4, CancellationToken.None)).ReturnsAsync(TestData.Game(4));

        var result = await _useCase.GetByIdAsync(4, CancellationToken.None);

        Assert.Equal(4, result.Id);
        Assert.Equal("Game", result.Name);
        Assert.Equal("Description", result.Description);
        Assert.True(result.IsActive);
        Assert.False(result.IsMultiplayer);
        Assert.Equal(new DateTime(2025, 1, 1), result.CreatedAt);
        Assert.Equal(new DateTime(2025, 1, 2), result.UpdatedAt);
    }

    [Fact]
    public async Task GetByIdAsync_ThrowsWhenGameDoesNotExist()
    {
        _repository.Setup(x => x.GetByIdAsync(4, CancellationToken.None))
            .ReturnsAsync((Domain.Entities.Game?)null);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _useCase.GetByIdAsync(4, CancellationToken.None));

        Assert.Equal("Jogo 4 não encontrado.", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_TrimsNameAndMapsCreatedGame()
    {
        _repository.Setup(x => x.AddAsync(It.IsAny<Domain.Entities.Game>(), CancellationToken.None))
            .ReturnsAsync((Domain.Entities.Game game, CancellationToken _) => game);

        var result = await _useCase.CreateAsync(new CreateGameRequest
        {
            Name = "  New Game  ",
            Description = "Details",
            Price = 0,
            IsMultiplayer = true
        }, CancellationToken.None);

        Assert.Equal("New Game", result.Name);
        Assert.Equal("Details", result.Description);
        Assert.Equal(0, result.Price);
        Assert.True(result.IsActive);
        Assert.True(result.IsMultiplayer);
        _repository.Verify(x => x.AddAsync(It.Is<Domain.Entities.Game>(game =>
            game.Name == "New Game" && game.IsActive && game.Price == 0), CancellationToken.None), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public async Task CreateAsync_RejectsMissingName(string name)
    {
        var exception = await Assert.ThrowsAsync<DomainException>(() => _useCase.CreateAsync(
            new CreateGameRequest { Name = name, Price = 10 }, CancellationToken.None));

        Assert.Equal("O nome do jogo é obrigatório.", exception.Message);
        _repository.Verify(x => x.AddAsync(It.IsAny<Domain.Entities.Game>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_RejectsNegativePrice()
    {
        var exception = await Assert.ThrowsAsync<DomainException>(() => _useCase.CreateAsync(
            new CreateGameRequest { Name = "Game", Price = -1 }, CancellationToken.None));

        Assert.Equal("O preço do jogo não pode ser negativo.", exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsWhenGameDoesNotExist()
    {
        _repository.Setup(x => x.GetByIdAsync(3, CancellationToken.None))
            .ReturnsAsync((Domain.Entities.Game?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _useCase.UpdateAsync(3, new UpdateGameRequest
        {
            Name = "Changed",
            Price = 10
        }, CancellationToken.None));

        _repository.Verify(x => x.UpdateAsync(It.IsAny<Domain.Entities.Game>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ValidatesNameAndPrice()
    {
        _repository.Setup(x => x.GetByIdAsync(1, CancellationToken.None)).ReturnsAsync(TestData.Game());

        await Assert.ThrowsAsync<DomainException>(() => _useCase.UpdateAsync(1,
            new UpdateGameRequest { Name = " ", Price = 10 }, CancellationToken.None));

        await Assert.ThrowsAsync<DomainException>(() => _useCase.UpdateAsync(1,
            new UpdateGameRequest { Name = "Valid", Price = -1 }, CancellationToken.None));

        _repository.Verify(x => x.UpdateAsync(It.IsAny<Domain.Entities.Game>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesExistingGameAndReturnsMappedResult()
    {
        var game = TestData.Game();
        _repository.Setup(x => x.GetByIdAsync(1, CancellationToken.None)).ReturnsAsync(game);

        var result = await _useCase.UpdateAsync(1, new UpdateGameRequest
        {
            Name = "  Changed  ",
            Description = null,
            Price = 42,
            IsMultiplayer = true,
            IsActive = false
        }, CancellationToken.None);

        Assert.Equal("Changed", game.Name);
        Assert.Null(game.Description);
        Assert.Equal(42, result.Price);
        Assert.False(result.IsActive);
        Assert.True(result.IsMultiplayer);
        Assert.NotNull(game.UpdatedAt);
        _repository.Verify(x => x.UpdateAsync(game, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ThrowsWhenGameDoesNotExist()
    {
        _repository.Setup(x => x.GetByIdAsync(9, CancellationToken.None))
            .ReturnsAsync((Domain.Entities.Game?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _useCase.DeleteAsync(9, CancellationToken.None));

        _repository.Verify(x => x.DeleteAsync(It.IsAny<Domain.Entities.Game>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_DeletesExistingGame()
    {
        var game = TestData.Game(9);
        _repository.Setup(x => x.GetByIdAsync(9, CancellationToken.None)).ReturnsAsync(game);

        await _useCase.DeleteAsync(9, CancellationToken.None);

        _repository.Verify(x => x.DeleteAsync(game, CancellationToken.None), Times.Once);
    }
}
