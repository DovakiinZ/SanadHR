using FluentValidation;
using HR.Modules.Platform.Commands.Reports;

namespace HR.Modules.Platform.Validators;

public class CreateReportValidator : AbstractValidator<CreateReportCommand>
{
    public CreateReportValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(100);
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ReportType).IsInEnum();
        RuleFor(x => x.Scope).IsInEnum();
        RuleFor(x => x.PrimaryObjectId).NotEmpty();
    }
}

public class UpdateReportValidator : AbstractValidator<UpdateReportCommand>
{
    public UpdateReportValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ReportType).IsInEnum();
        RuleFor(x => x.Scope).IsInEnum();
    }
}

public class CloneReportValidator : AbstractValidator<CloneReportCommand>
{
    public CloneReportValidator()
    {
        RuleFor(x => x.SourceReportId).NotEmpty();
        RuleFor(x => x.NewCode).NotEmpty().MaximumLength(100);
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
    }
}

public class AddReportFieldValidator : AbstractValidator<AddReportFieldCommand>
{
    public AddReportFieldValidator()
    {
        RuleFor(x => x.ReportDefinitionId).NotEmpty();
        RuleFor(x => x.FieldType).IsInEnum();
        RuleFor(x => x.FieldCode).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DisplayNameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DisplayNameAr).NotEmpty().MaximumLength(200);
    }
}

public class AddReportFilterValidator : AbstractValidator<AddReportFilterCommand>
{
    public AddReportFilterValidator()
    {
        RuleFor(x => x.ReportDefinitionId).NotEmpty();
        RuleFor(x => x.FieldCode).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Operator).IsInEnum();
    }
}

public class AddReportScheduleValidator : AbstractValidator<AddReportScheduleCommand>
{
    public AddReportScheduleValidator()
    {
        RuleFor(x => x.ReportDefinitionId).NotEmpty();
        RuleFor(x => x.Frequency).IsInEnum();
        RuleFor(x => x.ExportFormat).IsInEnum();
        RuleFor(x => x.Recipients).NotEmpty();
    }
}

public class CreateReportTemplateValidator : AbstractValidator<CreateReportTemplateCommand>
{
    public CreateReportTemplateValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(100);
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ReportType).IsInEnum();
        RuleFor(x => x.PrimaryObjectId).NotEmpty();
        RuleFor(x => x.Configuration).NotEmpty();
    }
}
