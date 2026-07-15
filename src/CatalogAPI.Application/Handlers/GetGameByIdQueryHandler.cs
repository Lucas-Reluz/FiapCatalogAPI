using CatalogAPI.Application.DTOs;
using CatalogAPI.Application.Queries;
using CatalogAPI.Domain.Interfaces;
using MediatR;

namespace CatalogAPI.Application.Handlers;

public class GetGameByIdQueryHandler : IRequestHandler<GetGameByIdQuery, GameResponse?>
{
    private readonly IGameRepository _gameRepository;

    public GetGameByIdQueryHandler(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository;
    }

    public async Task<GameResponse?> Handle(GetGameByIdQuery request, CancellationToken cancellationToken)
    {
        var game = await _gameRepository.GetByIdAsync(request.Id);

        if (game == null)
        {
            return null;
        }

        return new GameResponse(
            game.Id,
            game.Title,
            game.Description,
            game.Price,
            game.Stock,
            game.CreatedAt,
            game.UpdatedAt
        );
    }
}
