using BikeShop.Api.Domain.Customers;
using BikeShop.Api.Domain.Users;
using BikeShop.Api.Domain.WorkOrders;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BikeShop.Api.Infrastructure.Persistence.Configurations;

public sealed class WorkOrderConfiguration : IEntityTypeConfiguration<WorkOrder>
{
    public void Configure(EntityTypeBuilder<WorkOrder> builder)
    {
        builder.HasOne<Customer>().WithMany().HasForeignKey(workOrder => workOrder.CustomerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<User>().WithMany().HasForeignKey(workOrder => workOrder.AssignedToUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<User>().WithMany().HasForeignKey(workOrder => workOrder.CheckedInByUserId).OnDelete(DeleteBehavior.Restrict);

        builder.Property(workOrder => workOrder.BikeMakeModel).HasMaxLength(100);
        builder.Property(workOrder => workOrder.BikeColour).HasMaxLength(50);
        builder.Property(workOrder => workOrder.WorkRequested).HasMaxLength(2000);
        builder.Property(workOrder => workOrder.PosReceiptNumber).HasMaxLength(50);
        builder.Property(workOrder => workOrder.CancellationReason).HasMaxLength(500);

        builder.Property(workOrder => workOrder.JobType).HasConversion<string>();
        builder.Property(workOrder => workOrder.Status).HasConversion<string>();
        builder.Property(workOrder => workOrder.HoldReason).HasConversion<string>();
        builder.HasIndex(workOrder => workOrder.Status);

        builder.OwnsMany(workOrder => workOrder.LabourEntries, ConfigureLabourEntries);
        builder.OwnsMany(workOrder => workOrder.PartLines, ConfigurePartLines);
        builder.OwnsMany(workOrder => workOrder.Notes, ConfigureNotes);
    }

    private static void ConfigureLabourEntries(OwnedNavigationBuilder<WorkOrder, LabourEntry> labour)
    {
        labour.ToTable("LabourEntries");
        labour.HasKey(entry => entry.Id);
        labour.HasOne<User>().WithMany().HasForeignKey(entry => entry.MechanicUserId).OnDelete(DeleteBehavior.Restrict);
        labour.Property(entry => entry.Note).HasMaxLength(500);
    }

    private static void ConfigurePartLines(OwnedNavigationBuilder<WorkOrder, PartLine> part)
    {
        part.ToTable("PartLines");
        part.HasKey(line => line.Id);
        part.Property(line => line.Description).HasMaxLength(200);
    }

    private static void ConfigureNotes(OwnedNavigationBuilder<WorkOrder, JobNote> note)
    {
        note.ToTable("JobNotes");
        note.HasKey(jobNote => jobNote.Id);
        note.HasOne<User>().WithMany().HasForeignKey(jobNote => jobNote.WrittenByUserId).OnDelete(DeleteBehavior.Restrict);
        note.Property(jobNote => jobNote.Text).HasMaxLength(1000);
    }
}
