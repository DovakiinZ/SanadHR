using FluentValidation;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Employees.Commands;

// Conditional payment rules keyed off the selected PaymentMethod master-data code.
internal static class EmployeePaymentRules
{
    public static async Task ValidateAsync(
        ApplicationDbContext ctx,
        Guid? paymentMethodId, string? iban, Guid? bankId, string? salaryCardNumber,
        Action<string, string> addFailure, CancellationToken ct)
    {
        if (!paymentMethodId.HasValue) return;
        var code = await ctx.MasterDataItems
            .Where(m => m.Id == paymentMethodId).Select(m => m.Code).FirstOrDefaultAsync(ct);

        if (code == "BANK_TRANSFER")
        {
            if (string.IsNullOrWhiteSpace(iban)) addFailure("Iban", "رقم الآيبان (IBAN) مطلوب عند التحويل البنكي");
            if (!bankId.HasValue) addFailure("BankId", "البنك مطلوب عند التحويل البنكي");
        }
        else if (code == "SALARY_CARD")
        {
            if (string.IsNullOrWhiteSpace(salaryCardNumber)) addFailure("SalaryCardNumber", "رقم بطاقة الراتب مطلوب");
        }
    }
}

public class CreateEmployeeCommandValidator : AbstractValidator<CreateEmployeeCommand>
{
    public CreateEmployeeCommandValidator(ApplicationDbContext ctx)
    {
        RuleFor(x => x.EmployeeNumber).NotEmpty().WithMessage("الرقم الوظيفي مطلوب");
        RuleFor(x => x.FirstName).NotEmpty().WithMessage("الاسم الأول مطلوب");
        RuleFor(x => x.LastName).NotEmpty().WithMessage("اسم العائلة مطلوب");
        RuleFor(x => x.Email).NotEmpty().WithMessage("البريد الإلكتروني مطلوب").EmailAddress().WithMessage("صيغة البريد الإلكتروني غير صحيحة");
        RuleFor(x => x.BasicSalary).GreaterThanOrEqualTo(0).WithMessage("الراتب الأساسي يجب أن يكون صفراً أو أكثر");

        RuleFor(x => x).CustomAsync(async (cmd, context, ct) =>
            await EmployeePaymentRules.ValidateAsync(ctx,
                cmd.PaymentMethodId, cmd.Iban, cmd.BankId, cmd.SalaryCardNumber,
                (p, m) => context.AddFailure(p, m), ct));
    }
}

public class UpdateEmployeeCommandValidator : AbstractValidator<UpdateEmployeeCommand>
{
    public UpdateEmployeeCommandValidator(ApplicationDbContext ctx)
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FirstName).NotEmpty().WithMessage("الاسم الأول مطلوب");
        RuleFor(x => x.LastName).NotEmpty().WithMessage("اسم العائلة مطلوب");
        RuleFor(x => x.Email).NotEmpty().WithMessage("البريد الإلكتروني مطلوب").EmailAddress().WithMessage("صيغة البريد الإلكتروني غير صحيحة");
        RuleFor(x => x.BasicSalary).GreaterThanOrEqualTo(0).WithMessage("الراتب الأساسي يجب أن يكون صفراً أو أكثر");

        RuleFor(x => x).CustomAsync(async (cmd, context, ct) =>
            await EmployeePaymentRules.ValidateAsync(ctx,
                cmd.PaymentMethodId, cmd.Iban, cmd.BankId, cmd.SalaryCardNumber,
                (p, m) => context.AddFailure(p, m), ct));
    }
}
