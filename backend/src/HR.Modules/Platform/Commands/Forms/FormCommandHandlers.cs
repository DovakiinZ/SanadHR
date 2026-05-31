using AutoMapper;
using HR.Application.Common.Exceptions;
using HR.Application.Common.Interfaces;
using HR.Domain.Engines.Forms;
using HR.Domain.Enums;
using HR.Infrastructure.Persistence;
using HR.Modules.Platform.DTOs.Forms;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Commands.Forms;

public class CreateFormDefinitionCommandHandler : IRequestHandler<CreateFormDefinitionCommand, FormDefinitionDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateFormDefinitionCommandHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<FormDefinitionDto> Handle(CreateFormDefinitionCommand request, CancellationToken cancellationToken)
    {
        var entity = new FormDefinition
        {
            Code = request.Code,
            NameEn = request.NameEn,
            NameAr = request.NameAr,
            Description = request.Description,
            Module = request.Module
        };

        _context.FormDefinitions.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<FormDefinitionDto>(entity);
    }
}

public class UpdateFormDefinitionCommandHandler : IRequestHandler<UpdateFormDefinitionCommand, FormDefinitionDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UpdateFormDefinitionCommandHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<FormDefinitionDto> Handle(UpdateFormDefinitionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.FormDefinitions.FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new NotFoundException("FormDefinition", request.Id);

        entity.NameEn = request.NameEn;
        entity.NameAr = request.NameAr;
        entity.Description = request.Description;
        entity.IsPublished = request.IsPublished;
        entity.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<FormDefinitionDto>(entity);
    }
}

public class SubmitFormCommandHandler : IRequestHandler<SubmitFormCommand, FormSubmissionDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public SubmitFormCommandHandler(ApplicationDbContext context, IMapper mapper, ICurrentUserService currentUser)
    {
        _context = context;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<FormSubmissionDto> Handle(SubmitFormCommand request, CancellationToken cancellationToken)
    {
        var submission = new FormSubmission
        {
            FormDefinitionId = request.FormDefinitionId,
            SubmittedById = _currentUser.UserId,
            SubmittedAt = DateTime.UtcNow,
            Status = FormSubmissionStatus.Submitted
        };

        foreach (var val in request.Values)
        {
            submission.Values.Add(new FormSubmissionValue
            {
                FormFieldId = val.FormFieldId,
                FieldCode = val.FieldCode,
                Value = val.Value,
                FileUrl = val.FileUrl
            });
        }

        _context.FormSubmissions.Add(submission);
        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<FormSubmissionDto>(submission);
    }
}
