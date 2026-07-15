using CatalogAPI.Application.Commands;
using CatalogAPI.Application.DTOs;
using CatalogAPI.Domain.Interfaces;
using MediatR;

namespace CatalogAPI.Application.Handlers;

public class UpdateGameCommandHandler : IRequestHandler<UpdateGameCommand, GameResponse>
{
    private readonly IGameRepository _gameRepository;

    public UpdateGameCommandHandler(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository;
    }

    public async Task<GameResponse> Handle(UpdateGameCommand request, CancellationToken cancellationToken)
    {
        var game = await _gameRepository.GetByIdAsync(request.Id);

        if (game == null)
        {
            throw new KeyNotFoundException($"Jogo com ID {request.Id} não encontrado");
        }

        game.UpdateInfo(request.Title, request.Description, request.Price);
        await _gameRepository.UpdateAsync(game);

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
