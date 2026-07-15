using CatalogAPI.Application.DTOs;
using CatalogAPI.Application.Queries;
using CatalogAPI.Domain.Interfaces;
using MediatR;

namespace CatalogAPI.Application.Handlers;

public class GetGamesQueryHandler : IRequestHandler<GetGamesQuery, GamesListResponse>
{
    private readonly IGameRepository _gameRepository;

    public GetGamesQueryHandler(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository;
    }

    public async Task<GamesListResponse> Handle(GetGamesQuery request, CancellationToken cancellationToken)
    {
        var skip = (request.Page - 1) * request.PageSize;
        var games = await _gameRepository.GetAllAsync(skip, request.PageSize);
        var total = await _gameRepository.CountAsync();

        var gamesResponse = games.Select(g => new GameResponse(
            g.Id,
            g.Title,
            g.Description,
            g.Price,
            g.Stock,
            g.CreatedAt,
            g.UpdatedAt
        )).ToList();

        return new GamesListResponse(gamesResponse, total, request.Page, request.PageSize);
    }
}
