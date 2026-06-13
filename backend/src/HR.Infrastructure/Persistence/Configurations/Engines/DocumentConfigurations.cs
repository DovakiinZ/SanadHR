using HR.Domain.Engines.Documents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HR.Infrastructure.Persistence.Configurations.Engines;

public class DocumentTemplateConfiguration : IEntityTypeConfiguration<DocumentTemplate>
{
    public void Configure(EntityTypeBuilder<DocumentTemplate> builder)
    {
        builder.ToTable("engine_document_templates");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(100).IsRequired();
        builder.Property(x => x.NameEn).HasMaxLength(200).IsRequired();
        builder.Property(x => x.NameAr).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.Module).HasMaxLength(100).IsRequired();
        builder.Property(x => x.LayoutJson).HasColumnType("jsonb");
        builder.Property(x => x.PageSettings).HasColumnType("jsonb");
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        builder.HasMany(x => x.Tokens).WithOne(x => x.DocumentTemplate).HasForeignKey(x => x.DocumentTemplateId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Versions).WithOne(x => x.DocumentTemplate).HasForeignKey(x => x.DocumentTemplateId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.GeneratedDocuments).WithOne(x => x.DocumentTemplate).HasForeignKey(x => x.DocumentTemplateId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<PageTemplate>().WithMany().HasForeignKey(x => x.PageTemplateId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class PageTemplateConfiguration : IEntityTypeConfiguration<PageTemplate>
{
    public void Configure(EntityTypeBuilder<PageTemplate> builder)
    {
        builder.ToTable("engine_page_templates");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(100).IsRequired();
        builder.Property(x => x.NameEn).HasMaxLength(200).IsRequired();
        builder.Property(x => x.NameAr).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.HeaderConfig).HasColumnType("jsonb");
        builder.Property(x => x.FooterConfig).HasColumnType("jsonb");
        builder.Property(x => x.Margins).HasColumnType("jsonb");
        builder.Property(x => x.Watermark).HasColumnType("jsonb");
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
    }
}

public class DocumentTemplateVersionConfiguration : IEntityTypeConfiguration<DocumentTemplateVersion>
{
    public void Configure(EntityTypeBuilder<DocumentTemplateVersion> builder)
    {
        builder.ToTable("engine_document_template_versions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.BodyTemplate).IsRequired();
        builder.Property(x => x.ChangeNotes).HasMaxLength(500);
        builder.Property(x => x.CreatedBy).HasMaxLength(200);
    }
}

public class DocumentTemplateTokenConfiguration : IEntityTypeConfiguration<DocumentTemplateToken>
{
    public void Configure(EntityTypeBuilder<DocumentTemplateToken> builder)
    {
        builder.ToTable("engine_document_template_tokens");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TokenCode).HasMaxLength(200).IsRequired();
        builder.Property(x => x.DefaultValue).HasMaxLength(500);
    }
}

public class GeneratedDocumentConfiguration : IEntityTypeConfiguration<GeneratedDocument>
{
    public void Configure(EntityTypeBuilder<GeneratedDocument> builder)
    {
        builder.ToTable("engine_generated_documents");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EntityType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.FileUrl).HasMaxLength(1000);
        builder.Property(x => x.FileName).HasMaxLength(500);
        builder.Property(x => x.TokenValues).HasColumnType("jsonb");
        builder.Property(x => x.ErrorMessage).HasMaxLength(2000);
        builder.HasIndex(x => new { x.EntityType, x.EntityId });
    }
}

public class CompanyBrandingConfiguration : IEntityTypeConfiguration<CompanyBranding>
{
    public void Configure(EntityTypeBuilder<CompanyBranding> builder)
    {
        builder.ToTable("engine_company_branding");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ImageUrl).HasMaxLength(1000);
        builder.Property(x => x.Configuration).HasColumnType("jsonb");
        builder.HasIndex(x => new { x.TenantId, x.ElementType });
    }
}

public class DocumentWorkflowLinkConfiguration : IEntityTypeConfiguration<DocumentWorkflowLink>
{
    public void Configure(EntityTypeBuilder<DocumentWorkflowLink> builder)
    {
        builder.ToTable("engine_document_workflow_links");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TriggerType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.TriggerCondition).HasColumnType("jsonb");
    }
}
