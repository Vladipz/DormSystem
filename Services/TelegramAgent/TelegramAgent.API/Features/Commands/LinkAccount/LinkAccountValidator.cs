using FluentValidation;

namespace TelegramAgent.API.Features.Commands.LinkAccount
{
    public class LinkAccountValidator : AbstractValidator<LinkAccountCommand>
    {
        public LinkAccountValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty()
                .Length(6)
                .Matches(@"^\d{6}$")
                .WithMessage("Code must be exactly 6 digits");

            RuleFor(x => x.ChatId)
                .NotEqual(0)
                .WithMessage("Chat ID is required");
        }
    }
}