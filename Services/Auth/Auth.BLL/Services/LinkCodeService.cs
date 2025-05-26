using Auth.BLL.Interfaces;
using Auth.BLL.Models;
using Auth.DAL.Interfaces;

namespace Auth.BLL.Services
{
    public class LinkCodeService : ILinkCodeService
    {
        private readonly ILinkCodeRepository _linkCodeRepository;
        private readonly Random _random;

        public LinkCodeService(ILinkCodeRepository linkCodeRepository)
        {
            _linkCodeRepository = linkCodeRepository;
            _random = new Random();
        }

        public async Task<LinkCodeDto> GenerateAsync(Guid userId)
        {
            // Generate random 6-digit code (000000-999999)
            var code = _random.Next(0, 1000000).ToString("D6");

            // Set expiration to 5 minutes from now
            var expiresAt = DateTime.UtcNow.AddMinutes(5);

            // Store in database
            await _linkCodeRepository.CreateAsync(userId, code, expiresAt);

            return new LinkCodeDto
            {
                Code = code,
                ExpiresAt = expiresAt,
            };
        }

        public async Task<Guid?> ValidateAsync(string code)
        {
            var linkCode = await _linkCodeRepository.FindValidAsync(code);

            if (linkCode == null)
            {
                return null;
            }

            // Mark as used
            await _linkCodeRepository.MarkUsedAsync(linkCode.Id);

            return linkCode.UserId;
        }
    }
}