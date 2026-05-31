using FluentValidation;
using HR.Modules.Platform.Commands.CompanyConfig;

namespace HR.Modules.Platform.Validators;

public class UpdateCompanyProfileValidator : AbstractValidator<UpdateCompanyProfileCommand>
{
    public UpdateCompanyProfileValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CommercialRegistration).MaximumLength(100);
        RuleFor(x => x.VatNumber).MaximumLength(100);
        RuleFor(x => x.DefaultCurrency).MaximumLength(10);
        RuleFor(x => x.DefaultLanguage).MaximumLength(10);
        RuleFor(x => x.TimeZone).MaximumLength(100);
    }
}

public class CreatePositionValidator : AbstractValidator<CreatePositionCommand>
{
    public CreatePositionValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(100);
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
    }
}

public class UpdatePositionValidator : AbstractValidator<UpdatePositionCommand>
{
    public UpdatePositionValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
    }
}

public class CreateGradeValidator : AbstractValidator<CreateGradeCommand>
{
    public CreateGradeValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(100);
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Level).GreaterThan(0);
    }
}

public class UpdateGradeValidator : AbstractValidator<UpdateGradeCommand>
{
    public UpdateGradeValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Level).GreaterThan(0);
    }
}

public class CreateCostCenterValidator : AbstractValidator<CreateCostCenterCommand>
{
    public CreateCostCenterValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(100);
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
    }
}

public class UpdateCostCenterValidator : AbstractValidator<UpdateCostCenterCommand>
{
    public UpdateCostCenterValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
    }
}

public class CreateCalendarSettingValidator : AbstractValidator<CreateCalendarSettingCommand>
{
    public CreateCalendarSettingValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(100);
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CalendarType).NotEmpty()
            .Must(x => x == "Gregorian" || x == "Hijri")
            .WithMessage("CalendarType must be 'Gregorian' or 'Hijri'.");
        RuleFor(x => x.WorkWeekDays).NotEmpty();
    }
}

public class UpdateCalendarSettingValidator : AbstractValidator<UpdateCalendarSettingCommand>
{
    public UpdateCalendarSettingValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CalendarType).NotEmpty()
            .Must(x => x == "Gregorian" || x == "Hijri")
            .WithMessage("CalendarType must be 'Gregorian' or 'Hijri'.");
        RuleFor(x => x.WorkWeekDays).NotEmpty();
    }
}

public class CreateFiscalPeriodValidator : AbstractValidator<CreateFiscalPeriodCommand>
{
    public CreateFiscalPeriodValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(100);
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Year).GreaterThan(0);
        RuleFor(x => x.PeriodNumber).GreaterThan(0);
        RuleFor(x => x.StartDate).LessThan(x => x.EndDate)
            .WithMessage("StartDate must be before EndDate.");
    }
}

public class UpdateFiscalPeriodValidator : AbstractValidator<UpdateFiscalPeriodCommand>
{
    public UpdateFiscalPeriodValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Year).GreaterThan(0);
        RuleFor(x => x.PeriodNumber).GreaterThan(0);
        RuleFor(x => x.StartDate).LessThan(x => x.EndDate)
            .WithMessage("StartDate must be before EndDate.");
    }
}
