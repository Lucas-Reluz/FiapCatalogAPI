namespace CatalogAPI.Domain.Entities;

public class Game
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int Stock { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // EF Core
    protected Game() { }

    public Game(string title, string description, decimal price, int stock)
    {
        Id = Guid.NewGuid();
        Title = title;
        Description = description;
        Price = price;
        Stock = stock;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateInfo(string title, string description, decimal price)
    {
        Title = title;
        Description = description;
        Price = price;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool TryReserveStock(int quantity)
    {
        if (Stock >= quantity)
        {
            Stock -= quantity;
            UpdatedAt = DateTime.UtcNow;
            return true;
        }
        return false;
    }

    public void AddStock(int quantity)
    {
        Stock += quantity;
        UpdatedAt = DateTime.UtcNow;
    }
}
