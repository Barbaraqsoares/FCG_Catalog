using FCG_CatalogAPI.Adapters.Inbound.Responses;
using FCG_CatalogAPI.Domain.Ports;
using Microsoft.AspNetCore.Mvc;

namespace FCG_CatalogAPI.Adapters.Inbound.Controllers;

[ApiController]
[Route("api/LibraryOfGames")]
public class UserGameController : ControllerBase
{
    private readonly IUserGameUseCase _userGameUseCase;

    public UserGameController(
        IUserGameUseCase userGameUseCase)
    {
        _userGameUseCase = userGameUseCase;
    }

    /// <summary>
    /// Adiciona um jogo à biblioteca do usuário autenticado.
    /// </summary>
    [HttpPost("{gameId}")]
    [ProducesResponseType(typeof(UserGameResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddGameToLibrary(int gameId)
    {
        var userGame = await _userGameUseCase.AddGameToLibraryAsync(gameId);

        return Ok(userGame);
    }

    /// <summary>
    /// Retorna a biblioteca do usuário autenticado.
    /// </summary>
    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(List<UserGameResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserLibrary(int userId)
    {
        var library = await _userGameUseCase.GetUserLibraryAsync(userId);

        return Ok(library);
    }
}