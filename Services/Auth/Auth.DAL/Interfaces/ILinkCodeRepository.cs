using Auth.DAL.Entities;

namespace Auth.DAL.Interfaces
{
    public interface ILinkCodeRepository
    {
        Task CreateAsync(Guid userId, string code, DateTime expires);
        Task<LinkCode?> FindValidAsync(string code);
        Task MarkUsedAsync(Guid id);
    }
} 