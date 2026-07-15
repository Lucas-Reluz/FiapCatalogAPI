namespace CatalogAPI.Domain.Events;

public class StockInsufficientEvent
{
    public Guid OrderId { get; set; }
    public Guid GameId { get; set; }
    public string GameTitle { get; set; } = string.Empty;
    public int RequestedQuantity { get; set; }
    public int AvailableStock { get; set; }
    public DateTime OccurredAt { get; set; }

    public StockInsufficientEvent() { }

    public StockInsufficientEvent(Guid orderId, Guid gameId, string gameTitle, int requestedQuantity, int availableStock)
    {
        OrderId = orderId;
        GameId = gameId;
        GameTitle = gameTitle;
        RequestedQuantity = requestedQuantity;
        AvailableStock = availableStock;
        OccurredAt = DateTime.UtcNow;
    }
}
