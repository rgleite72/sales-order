using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalesOrder.Domain.Orders.Entities;

namespace SalesOrder.Infrastructure.Persistence.Configurations;

public class SalesOrderDocumentConfiguration: IEntityTypeConfiguration<OrderDocument>
{
    
    public void Configure(EntityTypeBuilder<OrderDocument> builder)
    {
        builder.ToTable("OrderDocuments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FileName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.BlobUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.UploadedAt)
            .IsRequired();        

    }
}
