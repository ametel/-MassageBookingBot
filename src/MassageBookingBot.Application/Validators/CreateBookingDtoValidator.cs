using FluentValidation;
using MassageBookingBot.Application.DTOs;

namespace MassageBookingBot.Application.Validators;

public class CreateBookingDtoValidator : AbstractValidator<CreateBookingDto>
{
    public CreateBookingDtoValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("UserId must be greater than 0");

        RuleFor(x => x.ServiceId)
            .GreaterThan(0)
            .WithMessage("ServiceId must be greater than 0");

        RuleFor(x => x.BookingDateTime)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("Cannot book appointments in the past");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Notes))
            .WithMessage("Notes cannot exceed 500 characters");
    }
}
