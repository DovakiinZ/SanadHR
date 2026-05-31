using HR.Domain.Engines.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HR.Infrastructure.Persistence.Configurations.Engines;

public class TokenCategoryConfiguration : IEntityTypeConfiguration<TokenCategory>
{
    public void Configure(EntityTypeBuilder<TokenCategory> builder)
    {
        builder.ToTable("engine_token_categories");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(100).IsRequired();
        builder.Property(x => x.NameEn).HasMaxLength(200).IsRequired();
        builder.Property(x => x.NameAr).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasMany(x => x.Tokens).WithOne(x => x.Category).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class TokenDefinitionConfiguration : IEntityTypeConfiguration<TokenDefinition>
{
    public void Configure(EntityTypeBuilder<TokenDefinition> builder)
    {
        builder.ToTable("engine_token_definitions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(200).IsRequired();
        builder.Property(x => x.NameEn).HasMaxLength(200).IsRequired();
        builder.Property(x => x.NameAr).HasMaxLength(200).IsRequired();
        builder.Property(x => x.DataType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ResolverKey).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => x.Code).IsUnique();
    }
}
