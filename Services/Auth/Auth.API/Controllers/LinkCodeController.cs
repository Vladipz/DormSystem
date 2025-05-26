using System.Security.Claims;

using Auth.API.Mappins;
using Auth.API.Models;
using Auth.BLL.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth.API.Controllers
{
    [ApiController]
    [Route("api/link-codes")]
    public class LinkCodeController : ControllerBase
    {
        private readonly ILinkCodeService _linkCodeService;

        public LinkCodeController(ILinkCodeService linkCodeService)
        {
            _linkCodeService = linkCodeService;
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<LinkCodeResponse>> GenerateLinkCode()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid user ID in token");
            }

            var linkCodeDto = await _linkCodeService.GenerateAsync(userId);

            var response = linkCodeDto.ToResponse();

            return Ok(response);
        }

        [HttpPost("validate")]
        public async Task<ActionResult<ValidateLinkCodeResponse>> ValidateLinkCode([FromBody] ValidateLinkCodeRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = await _linkCodeService.ValidateAsync(request.ToCode());

            if (userId == null)
            {
                return NotFound("Code not found, expired, or already used");
            }

            var response = userId.Value.ToResponse();

            return Ok(response);
        }
    }
}