using CatalogAPI.Application.DTOs;
using MediatR;

namespace CatalogAPI.Application.Commands;

public record CreateGameCommand(
    string Title,
    string Description,
    decimal Price,
    int Stock
) : IRequest<GameResponse>;
