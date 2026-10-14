using FluentValidation;
using HidroSense.Api.ViewModels;

namespace HidroSense.Api.Validators;

public class CreateConsumerUnitValidator : AbstractValidator<CreateConsumerUnitViewModel>
{
    public CreateConsumerUnitValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(40);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Region).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.IdleFlowThresholdLpm).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MonthlyBudgetLiters).GreaterThan(0);
    }
}

public class CreateConsumptionReadingValidator : AbstractValidator<CreateConsumptionReadingViewModel>
{
    public CreateConsumptionReadingValidator()
    {
        RuleFor(x => x.ConsumerUnitId).GreaterThan(0);
        RuleFor(x => x.VolumeLiters).GreaterThanOrEqualTo(0);
        RuleFor(x => x.FlowRateLpm).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ReadingTimeUtc)
            .Must(d => d is null || d <= DateTime.UtcNow.AddMinutes(5))
            .WithMessage("ReadingTimeUtc cannot be in the future.");
    }
}

public class LoginValidator : AbstractValidator<LoginViewModel>
{
    public LoginValidator()
    {
        RuleFor(x => x.Username).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
    }
}
