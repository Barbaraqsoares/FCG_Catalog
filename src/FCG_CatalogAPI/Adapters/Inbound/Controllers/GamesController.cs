using FCG_CatalogAPI.Adapters.Inbound.Requests;
using FCG_CatalogAPI.Adapters.Inbound.Responses;
using FCG_CatalogAPI.Domain.Ports;
using Microsoft.AspNetCore.Mvc;

namespace FCG_CatalogAPI.Adapters.Inbound.Controllers;

[ApiController]
[Route("[controller]")]
public class GameController : ControllerBase
{
    private readonly IGameUseCase _gameUseCase;

    public GameController(IGameUseCase gameUseCase)
    {
        _gameUseCase = gameUseCase;
    }

    /// <summary>
    /// Retorna todos os games disponíveis.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<GameResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetallGames()
    {
        var games = await _gameUseCase.GetAllAsync();

        return Ok(games);
    }

    /// <summary>
    /// Retorna um game específico pelo ID.
    /// </summary>
    /// <param name="id">Identificador do game.</param>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(GameResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetGameById(int id)
    {
        return Ok(await _gameUseCase.GetByIdAsync(id));
    }

    /// <summary>
    /// Cria um novo game (apenas para administradores).
    /// </summary>
    /// <param name="request">Nome, descrição, preço e se é multiplayer.</param>
    [HttpPost]
    [ProducesResponseType(typeof(GameResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateGameRequest request)
    {
        var createdGame = await _gameUseCase.CreateAsync(request);

        return CreatedAtAction(nameof(GetGameById), new { id = createdGame.Id }, createdGame);
    }

    /// <summary>
    /// Atualiza um game existente pelo ID (apenas para administradores).
    /// </summary>
    /// <param name="id">Identificador do game.</param>
    /// <param name="request">Dados do game, incluindo se continua ativo no catálogo.</param>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(GameResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateGame(int id, [FromBody] UpdateGameRequest request)
    {
        return Ok(await _gameUseCase.UpdateAsync(id, request));
    }

    /// <summary>
    /// Deleta um game existente pelo ID (apenas para administradores).
    /// </summary>
    /// <param name="id">Identificador do game.</param>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteGame(int id)
    {
        await _gameUseCase.DeleteAsync(id);

        return NoContent();
    }
}