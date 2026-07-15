namespace CatalogAPI.Domain.Events;

public class StockReservedEvent
{
    public Guid OrderId { get; set; }
    public Guid GameId { get; set; }
    public string GameTitle { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime ReservedAt { get; set; }

    public StockReservedEvent() { }

    public StockReservedEvent(Guid orderId, Guid gameId, string gameTitle, int quantity)
    {
        OrderId = orderId;
        GameId = gameId;
        GameTitle = gameTitle;
        Quantity = quantity;
        ReservedAt = DateTime.UtcNow;
    }
}
