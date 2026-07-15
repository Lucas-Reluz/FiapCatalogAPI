using CatalogAPI.Application.Commands;
using CatalogAPI.Domain.Interfaces;
using MediatR;

namespace CatalogAPI.Application.Handlers;

public class UpdateStockCommandHandler : IRequestHandler<UpdateStockCommand, bool>
{
    private readonly IGameRepository _gameRepository;

    public UpdateStockCommandHandler(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository;
    }

    public async Task<bool> Handle(UpdateStockCommand request, CancellationToken cancellationToken)
    {
        var game = await _gameRepository.GetByIdAsync(request.GameId);

        if (game == null)
        {
            throw new KeyNotFoundException($"Jogo com ID {request.GameId} não encontrado");
        }

        if (request.Quantity > 0)
        {
            game.AddStock(request.Quantity);
        }
        else
        {
            var success = game.TryReserveStock(Math.Abs(request.Quantity));
            if (!success)
            {
                throw new InvalidOperationException("Estoque insuficiente");
            }
        }

        await _gameRepository.UpdateAsync(game);
        return true;
    }
}
