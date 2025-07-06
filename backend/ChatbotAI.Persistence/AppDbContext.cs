using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ChatbotAI.Domain.Entities;

namespace ChatbotAI.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();   

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {    
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        modelBuilder.Entity<Conversation>(builder =>
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.CreatedAt)
                .IsRequired();
            
            builder.HasMany(c => c.Messages)
                .WithOne(m => m.Conversation)
                .HasForeignKey(m => m.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<Message>(builder =>
        {
            builder.HasKey(m => m.Id);

            builder.Property(m => m.Text).IsRequired()
                .HasMaxLength(500);

            builder.Property(m => m.Timestamp).IsRequired();

            builder.Property(m => m.Role)
                .HasMaxLength(10)
                .HasConversion<string>()
                .IsRequired();

            builder.Property(m => m.IsPartial)
                .HasDefaultValue(false);

            builder.HasOne(m => m.Conversation)        
                .WithMany(c => c.Messages)        
                .HasForeignKey(m => m.ConversationId)  
                .OnDelete(DeleteBehavior.Cascade);
        });

        base.OnModelCreating(modelBuilder);
    }
}
