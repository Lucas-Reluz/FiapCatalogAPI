using CatalogAPI.Application.DTOs;
using MediatR;

namespace CatalogAPI.Application.Commands;

public record UpdateGameCommand(
    Guid Id,
    string Title,
    string Description,
    decimal Price
) : IRequest<GameResponse>;
