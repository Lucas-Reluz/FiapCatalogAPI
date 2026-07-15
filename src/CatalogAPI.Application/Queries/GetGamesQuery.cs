using CatalogAPI.Application.DTOs;
using MediatR;

namespace CatalogAPI.Application.Queries;

public record GetGamesQuery(
    int Page = 1,
    int PageSize = 20
) : IRequest<GamesListResponse>;
