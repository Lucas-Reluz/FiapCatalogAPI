using CatalogAPI.Domain.Entities;

namespace CatalogAPI.Domain.Interfaces;

public interface IGameRepository
{
    Task<Game?> GetByIdAsync(Guid id);
    Task<List<Game>> GetAllAsync(int skip = 0, int take = 20);
    Task<Game> AddAsync(Game game);
    Task UpdateAsync(Game game);
    Task<bool> ExistsAsync(Guid id);
    Task<int> CountAsync();
}
