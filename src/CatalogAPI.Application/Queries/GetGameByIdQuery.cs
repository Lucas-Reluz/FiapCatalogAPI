using CatalogAPI.Application.DTOs;
using MediatR;

namespace CatalogAPI.Application.Queries;

public record GetGameByIdQuery(
    Guid Id
) : IRequest<GameResponse?>;
