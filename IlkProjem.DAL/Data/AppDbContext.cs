using Microsoft.EntityFrameworkCore;
using IlkProjem.Core.Models;
using IlkProjem.Core.Interfaces;
namespace IlkProjem.DAL.Data;

public class AppDbContext : DbContext
{
    private readonly ICurrentUserService _currentUserService;

    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserService currentUserService) 
        : base(options) 
    {
        _currentUserService = currentUserService;
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Files> Files => Set<Files>();
    public DbSet<House> Houses => Set<House>();
    public DbSet<Car> Cars => Set<Car>();
    public DbSet<ServiceLog> ServiceLog => Set<ServiceLog>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<AiChatMessage> AiChatMessages => Set<AiChatMessage>();
    public DbSet<KnowledgeDocument> KnowledgeDocuments => Set<KnowledgeDocument>();
    public DbSet<KnowledgeChunk> KnowledgeChunks => Set<KnowledgeChunk>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Files>(entity =>
        {
            entity.ToTable("Files");
            entity.Property(e => e.Metadata).HasColumnType("jsonb");
            entity.HasIndex(e => e.CreatedAt); 
            entity.HasIndex(e => e.RelativePath);
            entity.HasIndex(e => e.FileHash);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("Customers");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasOne(rt => rt.User)
                  .WithMany(u => u.RefreshTokens)
                  .HasForeignKey(rt => rt.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(rt => rt.Token).IsUnique();
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.ToTable("ChatMessages");
            entity.HasIndex(e => new { e.SenderId, e.ReceiverId });
            entity.HasIndex(e => e.SentAt);
        });

        modelBuilder.Entity<AiChatMessage>(entity =>
        {
            entity.ToTable("AiChatMessages");
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.SentAt);
        });

        modelBuilder.Entity<KnowledgeDocument>(entity =>
        {
            entity.ToTable("KnowledgeDocuments");
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.Language);
        });

        modelBuilder.Entity<KnowledgeChunk>(entity =>
        {
            entity.ToTable("KnowledgeChunks");
            entity.Property(e => e.Embedding).HasColumnType("double precision[]");
            entity.HasOne(c => c.Document)
                  .WithMany(d => d.Chunks)
                  .HasForeignKey(c => c.DocumentId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.DocumentId);
        });
    }
}

