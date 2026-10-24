using System.Security.Claims;
using FCG_CatalogAPI.Adapters.Inbound.Controllers;
using FCG_CatalogAPI.Adapters.Inbound.Requests;
using FCG_CatalogAPI.Adapters.Inbound.Responses;
using FCG_CatalogAPI.Domain.Ports;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FCG_CatalogAPI.Tests;

public class ControllerTests
{
    [Fact]
    public async Task GameController_ReturnsListOk()
    {
        var useCase = new Mock<IGameUseCase>();
        useCase.Setup(x => x.GetAllAsync(CancellationToken.None))
            .ReturnsAsync([new GameResponse { Name = "Game" }]);

        var result = await new GameController(useCase.Object).GetallGames(CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GameController_ReturnsGameOk()
    {
        var useCase = new Mock<IGameUseCase>();
        useCase.Setup(x => x.GetByIdAsync(2, CancellationToken.None))
            .ReturnsAsync(new GameResponse { Id = 2, Name = "Game" });

        var result = await new GameController(useCase.Object).GetGameById(2, CancellationToken.None);

        Assert.Equal(2, Assert.IsType<GameResponse>(Assert.IsType<OkObjectResult>(result).Value).Id);
    }

    [Fact]
    public async Task GameController_CreatesGameAtExpectedRoute()
    {
        var useCase = new Mock<IGameUseCase>();
        useCase.Setup(x => x.CreateAsync(It.IsAny<CreateGameRequest>(), CancellationToken.None))
            .ReturnsAsync(new GameResponse { Id = 8, Name = "Game" });

        var result = await new GameController(useCase.Object)
            .Create(new CreateGameRequest { Name = "Game" }, CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(GameController.GetGameById), created.ActionName);
        Assert.Equal(8, created.RouteValues!["id"]);
    }

    [Fact]
    public async Task GameController_UpdatesGameOk()
    {
        var useCase = new Mock<IGameUseCase>();
        useCase.Setup(x => x.UpdateAsync(2, It.IsAny<UpdateGameRequest>(), CancellationToken.None))
            .ReturnsAsync(new GameResponse { Id = 2, Name = "Updated" });

        var result = await new GameController(useCase.Object).UpdateGame(2,
            new UpdateGameRequest { Name = "Updated" }, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GameController_DeletesGameNoContent()
    {
        var useCase = new Mock<IGameUseCase>();

        var result = await new GameController(useCase.Object).DeleteGame(2, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        useCase.Verify(x => x.DeleteAsync(2, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task PromotionsController_ReturnsAllOk()
    {
        var useCase = new Mock<IPromotionUseCase>();
        useCase.Setup(x => x.GetAllAsync(CancellationToken.None)).ReturnsAsync([]);

        var result = await new PromotionsController(useCase.Object).GetAll(CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task PromotionsController_ReturnsPromotionOk()
    {
        var useCase = new Mock<IPromotionUseCase>();
        useCase.Setup(x => x.GetByIdAsync(4, CancellationToken.None))
            .ReturnsAsync(new PromotionResponse { Id = 4, Name = "Sale" });

        var result = await new PromotionsController(useCase.Object).GetById(4, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task PromotionsController_CreatesPromotionWithAuthenticatedUser()
    {
        var useCase = new Mock<IPromotionUseCase>();
        useCase.Setup(x => x.CreateAsync(It.IsAny<CreatePromotionRequest>(), 12, CancellationToken.None))
            .ReturnsAsync(new PromotionResponse { Id = 2, Name = "Sale" });
        var controller = new PromotionsController(useCase.Object);
        SetUser(controller, new Claim(ClaimTypes.NameIdentifier, "12"));

        var result = await controller.Create(new CreatePromotionRequest { Name = "Sale" }, CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(PromotionsController.GetById), created.ActionName);
        useCase.Verify(x => x.CreateAsync(It.IsAny<CreatePromotionRequest>(), 12, CancellationToken.None),
            Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("not-a-number")]
    public async Task PromotionsController_RejectsMissingOrInvalidUserClaim(string? userId)
    {
        var useCase = new Mock<IPromotionUseCase>();
        var controller = new PromotionsController(useCase.Object);
        SetUser(controller, userId is null ? null : new Claim(ClaimTypes.NameIdentifier, userId));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            controller.Create(new CreatePromotionRequest { Name = "Sale" }, CancellationToken.None));

        useCase.Verify(x => x.CreateAsync(It.IsAny<CreatePromotionRequest>(), It.IsAny<int>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task PromotionsController_DeletesPromotionNoContent()
    {
        var useCase = new Mock<IPromotionUseCase>();

        var result = await new PromotionsController(useCase.Object).Delete(3, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        useCase.Verify(x => x.DeleteAsync(3, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task UserGameController_AddsGameToLibraryOk()
    {
        var useCase = new Mock<IUserGameUseCase>();
        useCase.Setup(x => x.AddGameToLibraryAsync(7, 1, CancellationToken.None))
            .ReturnsAsync(new UserGameResponse { GameId = 7, GameName = "Game" });

        var result = await new UserGameController(useCase.Object)
            .AddGameToLibrary(7, 1, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
        useCase.Verify(x => x.AddGameToLibraryAsync(7, 1, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task UserGameController_ReturnsLibraryOk()
    {
        var useCase = new Mock<IUserGameUseCase>();
        useCase.Setup(x => x.GetUserLibraryAsync(5, CancellationToken.None)).ReturnsAsync([]);

        var result = await new UserGameController(useCase.Object).GetUserLibrary(5, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
        useCase.Verify(x => x.GetUserLibraryAsync(5, CancellationToken.None), Times.Once);
    }

    private static void SetUser(ControllerBase controller, Claim? claim)
    {
        var identity = claim is null
            ? new ClaimsIdentity()
            : new ClaimsIdentity([claim], "test");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
    }
}
