using Auth.BLL.Models;

namespace Auth.BLL.Interfaces
{
    public interface ILinkCodeService
    {
        Task<LinkCodeDto> GenerateAsync(Guid userId);

        Task<Guid?> ValidateAsync(string code);
    }
}