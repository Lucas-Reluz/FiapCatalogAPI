using MediatR;

namespace CatalogAPI.Application.Commands;

public record UpdateStockCommand(
    Guid GameId,
    int Quantity
) : IRequest<bool>;
