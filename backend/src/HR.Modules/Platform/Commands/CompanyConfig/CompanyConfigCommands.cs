using AutoMapper;
using HR.Domain.Engines.CompanyConfig;
using HR.Infrastructure.Persistence;
using HR.Modules.Platform.DTOs.CompanyConfig;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Commands.CompanyConfig;

// ─── Commands ────────────────────────────────────────────────────────────────

public record UpdateCompanyProfileCommand : IRequest<CompanyProfileDto>
{
    public Guid Id { get; init; }
    public string NameEn { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public string? LogoUrl { get; init; }
    public string? StampUrl { get; init; }
    public string? HrSignatureUrl { get; init; }
    public string? CeoSignatureUrl { get; init; }
    public string? CommercialRegistration { get; init; }
    public string? VatNumber { get; init; }
    public string? Website { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Address { get; init; }
    public string? City { get; init; }
    public string? Country { get; init; }
    public string? PostalCode { get; init; }
    public string? NationalAddress { get; init; }
    public string? ContactInfo { get; init; }
    public string? FiscalYearStart { get; init; }
    public string? DefaultCurrency { get; init; }
    public string? DefaultLanguage { get; init; }
    public string? TimeZone { get; init; }
    public string? MolNumber { get; init; }
    public string? GosiNumber { get; init; }
    public decimal GosiRate { get; init; } = 9.75m;
}

public record CreatePositionCommand : IRequest<PositionDto>
{
    public string Code { get; init; } = null!;
    public string NameEn { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public Guid? DepartmentId { get; init; }
    public Guid? ParentPositionId { get; init; }
    public string? JobDescription { get; init; }
    public int? MinGrade { get; init; }
    public int? MaxGrade { get; init; }
    public int SortOrder { get; init; }
}

public record UpdatePositionCommand : IRequest<PositionDto>
{
    public Guid Id { get; init; }
    public string NameEn { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public Guid? DepartmentId { get; init; }
    public Guid? ParentPositionId { get; init; }
    public string? JobDescription { get; init; }
    public int? MinGrade { get; init; }
    public int? MaxGrade { get; init; }
    public bool IsActive { get; init; }
    public int SortOrder { get; init; }
}

public record DeletePositionCommand(Guid Id) : IRequest;

public record CreateGradeCommand : IRequest<GradeDto>
{
    public string Code { get; init; } = null!;
    public string NameEn { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public int Level { get; init; }
    public decimal? MinSalary { get; init; }
    public decimal? MaxSalary { get; init; }
    public string? Benefits { get; init; }
    public int SortOrder { get; init; }
}

public record UpdateGradeCommand : IRequest<GradeDto>
{
    public Guid Id { get; init; }
    public string NameEn { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public int Level { get; init; }
    public decimal? MinSalary { get; init; }
    public decimal? MaxSalary { get; init; }
    public string? Benefits { get; init; }
    public bool IsActive { get; init; }
    public int SortOrder { get; init; }
}

public record DeleteGradeCommand(Guid Id) : IRequest;

public record CreateCostCenterCommand : IRequest<CostCenterDto>
{
    public string Code { get; init; } = null!;
    public string NameEn { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public Guid? ParentCostCenterId { get; init; }
    public Guid? DepartmentId { get; init; }
    public Guid? BranchId { get; init; }
    public int SortOrder { get; init; }
}

public record UpdateCostCenterCommand : IRequest<CostCenterDto>
{
    public Guid Id { get; init; }
    public string NameEn { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public Guid? ParentCostCenterId { get; init; }
    public Guid? DepartmentId { get; init; }
    public Guid? BranchId { get; init; }
    public bool IsActive { get; init; }
    public int SortOrder { get; init; }
}

public record DeleteCostCenterCommand(Guid Id) : IRequest;

public record CreateCalendarSettingCommand : IRequest<CalendarSettingDto>
{
    public string Code { get; init; } = null!;
    public string NameEn { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public string CalendarType { get; init; } = null!;
    public string WorkWeekDays { get; init; } = null!;
    public string? Holidays { get; init; }
    public TimeSpan WorkDayStart { get; init; }
    public TimeSpan WorkDayEnd { get; init; }
    public bool IsDefault { get; init; }
}

public record UpdateCalendarSettingCommand : IRequest<CalendarSettingDto>
{
    public Guid Id { get; init; }
    public string NameEn { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public string CalendarType { get; init; } = null!;
    public string WorkWeekDays { get; init; } = null!;
    public string? Holidays { get; init; }
    public TimeSpan WorkDayStart { get; init; }
    public TimeSpan WorkDayEnd { get; init; }
    public bool IsDefault { get; init; }
    public bool IsActive { get; init; }
}

public record DeleteCalendarSettingCommand(Guid Id) : IRequest;

public record CreateFiscalPeriodCommand : IRequest<FiscalPeriodDto>
{
    public string Code { get; init; } = null!;
    public string NameEn { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public int Year { get; init; }
    public int PeriodNumber { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
}

public record UpdateFiscalPeriodCommand : IRequest<FiscalPeriodDto>
{
    public Guid Id { get; init; }
    public string NameEn { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public int Year { get; init; }
    public int PeriodNumber { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
}

public record CloseFiscalPeriodCommand(Guid Id) : IRequest<FiscalPeriodDto>;

public record DeleteFiscalPeriodCommand(Guid Id) : IRequest;

// ─── Handlers ────────────────────────────────────────────────────────────────

public class UpdateCompanyProfileCommandHandler : IRequestHandler<UpdateCompanyProfileCommand, CompanyProfileDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UpdateCompanyProfileCommandHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<CompanyProfileDto> Handle(UpdateCompanyProfileCommand request, CancellationToken cancellationToken)
    {
        // Upsert: one canonical company profile per tenant (create on first save).
        var entity = request.Id != Guid.Empty
            ? await _context.Set<CompanyProfile>().FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            : await _context.Set<CompanyProfile>().FirstOrDefaultAsync(cancellationToken);
        if (entity is null)
        {
            entity = new CompanyProfile();
            _context.Set<CompanyProfile>().Add(entity);
        }

        entity.NameEn = request.NameEn;
        entity.NameAr = request.NameAr;
        entity.LogoUrl = request.LogoUrl;
        entity.StampUrl = request.StampUrl;
        entity.HrSignatureUrl = request.HrSignatureUrl;
        entity.CeoSignatureUrl = request.CeoSignatureUrl;
        entity.CommercialRegistration = request.CommercialRegistration;
        entity.VatNumber = request.VatNumber;
        entity.Website = request.Website;
        entity.Email = request.Email;
        entity.Phone = request.Phone;
        entity.Address = request.Address;
        entity.City = request.City;
        entity.Country = request.Country;
        entity.PostalCode = request.PostalCode;
        entity.NationalAddress = request.NationalAddress;
        entity.ContactInfo = request.ContactInfo;
        entity.FiscalYearStart = request.FiscalYearStart;
        entity.DefaultCurrency = request.DefaultCurrency;
        entity.DefaultLanguage = request.DefaultLanguage;
        entity.TimeZone = request.TimeZone;
        entity.MolNumber = request.MolNumber;
        entity.GosiNumber = request.GosiNumber;
        if (request.GosiRate > 0) entity.GosiRate = request.GosiRate;

        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<CompanyProfileDto>(entity);
    }
}

public class CreatePositionCommandHandler : IRequestHandler<CreatePositionCommand, PositionDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreatePositionCommandHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PositionDto> Handle(CreatePositionCommand request, CancellationToken cancellationToken)
    {
        var entity = new Position
        {
            Code = request.Code,
            NameEn = request.NameEn,
            NameAr = request.NameAr,
            DepartmentId = request.DepartmentId,
            ParentPositionId = request.ParentPositionId,
            JobDescription = request.JobDescription,
            MinGrade = request.MinGrade,
            MaxGrade = request.MaxGrade,
            SortOrder = request.SortOrder
        };

        _context.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<PositionDto>(entity);
    }
}

public class UpdatePositionCommandHandler : IRequestHandler<UpdatePositionCommand, PositionDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UpdatePositionCommandHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PositionDto> Handle(UpdatePositionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Set<Position>().FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new InvalidOperationException("Position not found");

        entity.NameEn = request.NameEn;
        entity.NameAr = request.NameAr;
        entity.DepartmentId = request.DepartmentId;
        entity.ParentPositionId = request.ParentPositionId;
        entity.JobDescription = request.JobDescription;
        entity.MinGrade = request.MinGrade;
        entity.MaxGrade = request.MaxGrade;
        entity.IsActive = request.IsActive;
        entity.SortOrder = request.SortOrder;

        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<PositionDto>(entity);
    }
}

public class DeletePositionCommandHandler : IRequestHandler<DeletePositionCommand>
{
    private readonly ApplicationDbContext _context;

    public DeletePositionCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeletePositionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Set<Position>().FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new InvalidOperationException("Position not found");

        _context.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

public class CreateGradeCommandHandler : IRequestHandler<CreateGradeCommand, GradeDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateGradeCommandHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<GradeDto> Handle(CreateGradeCommand request, CancellationToken cancellationToken)
    {
        var entity = new Grade
        {
            Code = request.Code,
            NameEn = request.NameEn,
            NameAr = request.NameAr,
            Level = request.Level,
            MinSalary = request.MinSalary,
            MaxSalary = request.MaxSalary,
            Benefits = request.Benefits,
            SortOrder = request.SortOrder
        };

        _context.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<GradeDto>(entity);
    }
}

public class UpdateGradeCommandHandler : IRequestHandler<UpdateGradeCommand, GradeDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UpdateGradeCommandHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<GradeDto> Handle(UpdateGradeCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Set<Grade>().FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new InvalidOperationException("Grade not found");

        entity.NameEn = request.NameEn;
        entity.NameAr = request.NameAr;
        entity.Level = request.Level;
        entity.MinSalary = request.MinSalary;
        entity.MaxSalary = request.MaxSalary;
        entity.Benefits = request.Benefits;
        entity.IsActive = request.IsActive;
        entity.SortOrder = request.SortOrder;

        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<GradeDto>(entity);
    }
}

public class DeleteGradeCommandHandler : IRequestHandler<DeleteGradeCommand>
{
    private readonly ApplicationDbContext _context;

    public DeleteGradeCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteGradeCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Set<Grade>().FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new InvalidOperationException("Grade not found");

        _context.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

public class CreateCostCenterCommandHandler : IRequestHandler<CreateCostCenterCommand, CostCenterDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateCostCenterCommandHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<CostCenterDto> Handle(CreateCostCenterCommand request, CancellationToken cancellationToken)
    {
        var entity = new CostCenter
        {
            Code = request.Code,
            NameEn = request.NameEn,
            NameAr = request.NameAr,
            ParentCostCenterId = request.ParentCostCenterId,
            DepartmentId = request.DepartmentId,
            BranchId = request.BranchId,
            SortOrder = request.SortOrder
        };

        _context.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<CostCenterDto>(entity);
    }
}

public class UpdateCostCenterCommandHandler : IRequestHandler<UpdateCostCenterCommand, CostCenterDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UpdateCostCenterCommandHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<CostCenterDto> Handle(UpdateCostCenterCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Set<CostCenter>().FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new InvalidOperationException("Cost center not found");

        entity.NameEn = request.NameEn;
        entity.NameAr = request.NameAr;
        entity.ParentCostCenterId = request.ParentCostCenterId;
        entity.DepartmentId = request.DepartmentId;
        entity.BranchId = request.BranchId;
        entity.IsActive = request.IsActive;
        entity.SortOrder = request.SortOrder;

        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<CostCenterDto>(entity);
    }
}

public class DeleteCostCenterCommandHandler : IRequestHandler<DeleteCostCenterCommand>
{
    private readonly ApplicationDbContext _context;

    public DeleteCostCenterCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteCostCenterCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Set<CostCenter>().FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new InvalidOperationException("Cost center not found");

        _context.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

public class CreateCalendarSettingCommandHandler : IRequestHandler<CreateCalendarSettingCommand, CalendarSettingDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateCalendarSettingCommandHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<CalendarSettingDto> Handle(CreateCalendarSettingCommand request, CancellationToken cancellationToken)
    {
        var entity = new CalendarSetting
        {
            Code = request.Code,
            NameEn = request.NameEn,
            NameAr = request.NameAr,
            CalendarType = request.CalendarType,
            WorkWeekDays = request.WorkWeekDays,
            Holidays = request.Holidays,
            WorkDayStart = request.WorkDayStart,
            WorkDayEnd = request.WorkDayEnd,
            IsDefault = request.IsDefault
        };

        _context.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<CalendarSettingDto>(entity);
    }
}

public class UpdateCalendarSettingCommandHandler : IRequestHandler<UpdateCalendarSettingCommand, CalendarSettingDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UpdateCalendarSettingCommandHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<CalendarSettingDto> Handle(UpdateCalendarSettingCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Set<CalendarSetting>().FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new InvalidOperationException("Calendar setting not found");

        entity.NameEn = request.NameEn;
        entity.NameAr = request.NameAr;
        entity.CalendarType = request.CalendarType;
        entity.WorkWeekDays = request.WorkWeekDays;
        entity.Holidays = request.Holidays;
        entity.WorkDayStart = request.WorkDayStart;
        entity.WorkDayEnd = request.WorkDayEnd;
        entity.IsDefault = request.IsDefault;
        entity.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<CalendarSettingDto>(entity);
    }
}

public class DeleteCalendarSettingCommandHandler : IRequestHandler<DeleteCalendarSettingCommand>
{
    private readonly ApplicationDbContext _context;

    public DeleteCalendarSettingCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteCalendarSettingCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Set<CalendarSetting>().FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new InvalidOperationException("Calendar setting not found");

        _context.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

public class CreateFiscalPeriodCommandHandler : IRequestHandler<CreateFiscalPeriodCommand, FiscalPeriodDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateFiscalPeriodCommandHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<FiscalPeriodDto> Handle(CreateFiscalPeriodCommand request, CancellationToken cancellationToken)
    {
        var entity = new FiscalPeriod
        {
            Code = request.Code,
            NameEn = request.NameEn,
            NameAr = request.NameAr,
            Year = request.Year,
            PeriodNumber = request.PeriodNumber,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };

        _context.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<FiscalPeriodDto>(entity);
    }
}

public class UpdateFiscalPeriodCommandHandler : IRequestHandler<UpdateFiscalPeriodCommand, FiscalPeriodDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UpdateFiscalPeriodCommandHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<FiscalPeriodDto> Handle(UpdateFiscalPeriodCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Set<FiscalPeriod>().FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new InvalidOperationException("Fiscal period not found");

        entity.NameEn = request.NameEn;
        entity.NameAr = request.NameAr;
        entity.Year = request.Year;
        entity.PeriodNumber = request.PeriodNumber;
        entity.StartDate = request.StartDate;
        entity.EndDate = request.EndDate;

        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<FiscalPeriodDto>(entity);
    }
}

public class CloseFiscalPeriodCommandHandler : IRequestHandler<CloseFiscalPeriodCommand, FiscalPeriodDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CloseFiscalPeriodCommandHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<FiscalPeriodDto> Handle(CloseFiscalPeriodCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Set<FiscalPeriod>().FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new InvalidOperationException("Fiscal period not found");

        entity.IsClosed = true;
        entity.ClosedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<FiscalPeriodDto>(entity);
    }
}

public class DeleteFiscalPeriodCommandHandler : IRequestHandler<DeleteFiscalPeriodCommand>
{
    private readonly ApplicationDbContext _context;

    public DeleteFiscalPeriodCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteFiscalPeriodCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Set<FiscalPeriod>().FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new InvalidOperationException("Fiscal period not found");

        _context.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
