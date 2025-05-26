using Auth.DAL.Data;
using Auth.DAL.Entities;
using Auth.DAL.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace Auth.DAL.Repositories
{
    public class LinkCodeRepository : ILinkCodeRepository
    {
        private readonly AuthDbContext _context;

        public LinkCodeRepository(AuthDbContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(Guid userId, string code, DateTime expires)
        {
            var linkCode = new LinkCode
            {
                UserId = userId,
                Code = code,
                ExpiresAt = expires,
                Used = false,
            };

            _context.LinkCodes.Add(linkCode);
            await _context.SaveChangesAsync();
        }

        public async Task<LinkCode?> FindValidAsync(string code)
        {
            var now = DateTime.UtcNow;

            return await _context.LinkCodes
                .FirstOrDefaultAsync(lc => lc.Code == code
                                         && !lc.Used
                                         && lc.ExpiresAt > now);
        }

        public async Task MarkUsedAsync(Guid id)
        {
            var linkCode = await _context.LinkCodes.FindAsync(id);
            if (linkCode != null)
            {
                linkCode.Used = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}