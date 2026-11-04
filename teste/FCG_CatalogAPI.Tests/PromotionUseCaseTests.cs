using FCG_CatalogAPI.Adapters.Inbound.Requests;
using FCG_CatalogAPI.Application.UseCases;
using FCG_CatalogAPI.Domain.Exceptions;
using FCG_CatalogAPI.Domain.Ports;
using Moq;
using Xunit;

namespace FCG_CatalogAPI.Tests;

public class PromotionUseCaseTests
{
    private readonly Mock<IPromotionRepository> _promotions = new();
    private readonly Mock<IGameRepository> _games = new();
    private readonly PromotionUseCase _useCase;

    public PromotionUseCaseTests() => _useCase = new PromotionUseCase(_promotions.Object, _games.Object);

    [Fact]
    public async Task CreateAsync_CreatesPromotionAndMapsGameIds()
    {
        var game = TestData.Game(4);
        _games.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), CancellationToken.None))
            .ReturnsAsync([game]);
        _promotions.Setup(x => x.AddAsync(It.IsAny<Domain.Entities.Promotion>(), CancellationToken.None))
            .ReturnsAsync((Domain.Entities.Promotion promotion, CancellationToken _) => promotion);

        var result = await _useCase.CreateAsync(ValidRequest("  Summer  ", [4]), 12, CancellationToken.None);

        Assert.Equal("Summer", result.Name);
        Assert.Equal(10, result.Discount);
        Assert.Equal(12, result.CreatedByUserId);
        Assert.Equal([4], result.GameIds);
        _promotions.Verify(x => x.AddAsync(It.Is<Domain.Entities.Promotion>(p =>
            p.Name == "Summer" && p.Games.Single().Id == 4), CancellationToken.None), Times.Once);
    }

    [Theory]
    [InlineData("", 10, "2025-01-01", "2025-02-01", "O nome da promoção é obrigatório.")]
    [InlineData("Promo", 0, "2025-01-01", "2025-02-01", "Desconto precisa ser maior que 0 e até 100.")]
    [InlineData("Promo", 101, "2025-01-01", "2025-02-01", "Desconto precisa ser maior que 0 e até 100.")]
    [InlineData("Promo", 10, "2025-02-01", "2025-02-01", "A data de inicio precisa ser menor que a data fim.")]
    public async Task CreateAsync_RejectsInvalidPromotion(
        string name, decimal discount, string start, string end, string message)
    {
        var request = new CreatePromotionRequest
        {
            Name = name,
            Discount = discount,
            StartDate = DateTime.Parse(start),
            EndDate = DateTime.Parse(end),
            GameIds = [1]
        };

        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            _useCase.CreateAsync(request, 1, CancellationToken.None));

        Assert.Equal(message, exception.Message);
        _games.Verify(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_RejectsEmptyGameList()
    {
        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            _useCase.CreateAsync(ValidRequest("Promo", []), 1, CancellationToken.None));

        Assert.Equal("Ao menos 1 game precisa ser selecionado.", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_RejectsUnknownGameIds()
    {
        _games.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), CancellationToken.None))
            .ReturnsAsync([]);

        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            _useCase.CreateAsync(ValidRequest("Promo", [1, 2]), 1, CancellationToken.None));

        Assert.Equal("Um ou mais games não foram encontrados.", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_AllowsDuplicateIdsWhenEveryUniqueGameExists()
    {
        _games.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), CancellationToken.None))
            .ReturnsAsync([TestData.Game(1)]);
        _promotions.Setup(x => x.AddAsync(It.IsAny<Domain.Entities.Promotion>(), CancellationToken.None))
            .ReturnsAsync((Domain.Entities.Promotion promotion, CancellationToken _) => promotion);

        var result = await _useCase.CreateAsync(ValidRequest("Promo", [1, 1]), 2, CancellationToken.None);

        Assert.Equal([1], result.GameIds);
    }

    [Fact]
    public async Task GetAllAsync_MapsPromotions()
    {
        _promotions.Setup(x => x.GetAllAsync(CancellationToken.None))
            .ReturnsAsync([TestData.Promotion(3, "P", TestData.Game(5))]);

        var result = (await _useCase.GetAllAsync(CancellationToken.None)).Single();

        Assert.Equal(3, result.Id);
        Assert.Equal("P", result.Name);
        Assert.Equal([5], result.GameIds);
        Assert.Equal(7, result.CreatedByUserId);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsMappedPromotion()
    {
        _promotions.Setup(x => x.GetByIdAsync(3, CancellationToken.None))
            .ReturnsAsync(TestData.Promotion(3, "P", TestData.Game(5)));

        var result = await _useCase.GetByIdAsync(3, CancellationToken.None);

        Assert.Equal(3, result.Id);
        Assert.Equal(new DateTime(2025, 2, 1), result.StartDate);
        Assert.Equal(new DateTime(2025, 2, 28), result.EndDate);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task GetByIdAsync_ThrowsWhenPromotionDoesNotExist()
    {
        _promotions.Setup(x => x.GetByIdAsync(3, CancellationToken.None))
            .ReturnsAsync((Domain.Entities.Promotion?)null);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _useCase.GetByIdAsync(3, CancellationToken.None));

        Assert.Equal("Promoção 3 não encontrada.", exception.Message);
    }

    [Fact]
    public async Task DeleteAsync_DeletesExistingPromotion()
    {
        var promotion = TestData.Promotion(8);
        _promotions.Setup(x => x.GetByIdAsync(8, CancellationToken.None)).ReturnsAsync(promotion);

        await _useCase.DeleteAsync(8, CancellationToken.None);

        _promotions.Verify(x => x.DeleteAsync(promotion, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ThrowsWhenPromotionDoesNotExist()
    {
        _promotions.Setup(x => x.GetByIdAsync(8, CancellationToken.None))
            .ReturnsAsync((Domain.Entities.Promotion?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _useCase.DeleteAsync(8, CancellationToken.None));

        _promotions.Verify(x => x.DeleteAsync(It.IsAny<Domain.Entities.Promotion>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    private static CreatePromotionRequest ValidRequest(string name, List<int> ids) => new()
    {
        Name = name,
        Discount = 10,
        StartDate = new DateTime(2025, 1, 1),
        EndDate = new DateTime(2025, 2, 1),
        GameIds = ids
    };
}
