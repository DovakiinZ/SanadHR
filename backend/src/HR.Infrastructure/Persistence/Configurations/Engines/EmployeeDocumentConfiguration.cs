using HR.Domain.Engines.Documents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HR.Infrastructure.Persistence.Configurations.Engines;

public class EmployeeDocumentConfiguration : IEntityTypeConfiguration<EmployeeDocument>
{
    public void Configure(EntityTypeBuilder<EmployeeDocument> builder)
    {
        builder.ToTable("employee_documents");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Type).HasMaxLength(50);
        builder.Property(x => x.Title).HasMaxLength(300);
        builder.Property(x => x.DocumentNumber).HasMaxLength(100);
        builder.Property(x => x.Notes).HasMaxLength(2000);
        builder.Property(x => x.FileUrl).HasMaxLength(1000);
        builder.Property(x => x.FileName).HasMaxLength(400);
        builder.Property(x => x.ContentType).HasMaxLength(200);
        builder.HasIndex(x => new { x.TenantId, x.EmployeeId });
        // Supports the document-expiry scan that feeds notification rules.
        builder.HasIndex(x => new { x.TenantId, x.ExpiryDate });
    }
}
