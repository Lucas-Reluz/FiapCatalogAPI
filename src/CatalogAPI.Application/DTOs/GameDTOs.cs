namespace CatalogAPI.Application.DTOs;

public record GameResponse(
    Guid Id,
    string Title,
    string Description,
    decimal Price,
    int Stock,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CreateGameRequest(
    string Title,
    string Description,
    decimal Price,
    int Stock
);

public record UpdateGameRequest(
    string Title,
    string Description,
    decimal Price
);

public record UpdateStockRequest(
    int Quantity
);

public record GamesListResponse(
    List<GameResponse> Games,
    int Total,
    int Page,
    int PageSize
);
