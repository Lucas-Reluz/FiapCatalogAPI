using CatalogAPI.Application.Commands;
using CatalogAPI.Application.DTOs;
using CatalogAPI.Domain.Entities;
using CatalogAPI.Domain.Interfaces;
using MediatR;

namespace CatalogAPI.Application.Handlers;

public class CreateGameCommandHandler : IRequestHandler<CreateGameCommand, GameResponse>
{
    private readonly IGameRepository _gameRepository;

    public CreateGameCommandHandler(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository;
    }

    public async Task<GameResponse> Handle(CreateGameCommand request, CancellationToken cancellationToken)
    {
        var game = new Game(
            request.Title,
            request.Description,
            request.Price,
            request.Stock
        );

        await _gameRepository.AddAsync(game);

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
