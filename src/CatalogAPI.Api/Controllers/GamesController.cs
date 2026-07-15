using CatalogAPI.Application.Commands;
using CatalogAPI.Application.DTOs;
using CatalogAPI.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatalogAPI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<GamesController> _logger;

    public GamesController(IMediator mediator, ILogger<GamesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Listar todos os jogos com paginação
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<GamesListResponse>> GetGames([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var query = new GetGamesQuery(page, pageSize);
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar jogos");
            return StatusCode(500, new { message = "Erro ao listar jogos" });
        }
    }

    /// <summary>
    /// Buscar jogo por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GameResponse>> GetGameById(Guid id)
    {
        try
        {
            var query = new GetGameByIdQuery(id);
            var result = await _mediator.Send(query);

            if (result == null)
            {
                return NotFound(new { message = "Jogo não encontrado" });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar jogo {GameId}", id);
            return StatusCode(500, new { message = "Erro ao buscar jogo" });
        }
    }

    /// <summary>
    /// Criar novo jogo (Admin)
    /// </summary>
    [HttpPost]
    //[Authorize]  // Comentado temporariamente para testes
    public async Task<ActionResult<GameResponse>> CreateGame([FromBody] CreateGameRequest request)
    {
        try
        {
            var command = new CreateGameCommand(request.Title, request.Description, request.Price, request.Stock);
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetGameById), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar jogo");
            return StatusCode(500, new { message = "Erro ao criar jogo" });
        }
    }

    /// <summary>
    /// Atualizar informações do jogo (Admin)
    /// </summary>
    [HttpPut("{id:guid}")]
    //[Authorize]  // Comentado temporariamente para testes
    public async Task<ActionResult<GameResponse>> UpdateGame(Guid id, [FromBody] UpdateGameRequest request)
    {
        try
        {
            var command = new UpdateGameCommand(id, request.Title, request.Description, request.Price);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Jogo não encontrado" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar jogo {GameId}", id);
            return StatusCode(500, new { message = "Erro ao atualizar jogo" });
        }
    }

    /// <summary>
    /// Atualizar estoque (Admin)
    /// </summary>
    [HttpPatch("{id:guid}/stock")]
    //[Authorize]  // Comentado temporariamente para testes
    public async Task<ActionResult> UpdateStock(Guid id, [FromBody] UpdateStockRequest request)
    {
        try
        {
            var command = new UpdateStockCommand(id, request.Quantity);
            await _mediator.Send(command);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Jogo não encontrado" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar estoque do jogo {GameId}", id);
            return StatusCode(500, new { message = "Erro ao atualizar estoque" });
        }
    }
}
