using Microsoft.EntityFrameworkCore;
using IlkProjem.Core.Models;
using IlkProjem.Core.Interfaces;
using IlkProjem.Core.Enums;
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

    // --- WORKFLOW SYSTEM ---
    public DbSet<Workflow> Workflows => Set<Workflow>();
    public DbSet<WorkflowStep> WorkflowSteps => Set<WorkflowStep>();
    public DbSet<WorkflowAction> WorkflowActions => Set<WorkflowAction>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<WorkflowHistory> WorkflowHistory => Set<WorkflowHistory>();

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

        // --- WORKFLOW SYSTEM ---
        modelBuilder.Entity<Workflow>(entity =>
        {
            entity.ToTable("Workflows");
            entity.HasIndex(e => e.WorkflowNo).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.RequestedByUserId);
            entity.HasIndex(e => e.AssignedToUserId);
            entity.HasIndex(e => e.CreatedAt);
            entity.Property(e => e.Data).HasColumnType("jsonb");
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.Type).HasConversion<string>();

            entity.HasOne(w => w.RequestedByUser)
                  .WithMany()
                  .HasForeignKey(w => w.RequestedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(w => w.AssignedToUser)
                  .WithMany()
                  .HasForeignKey(w => w.AssignedToUserId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<WorkflowStep>(entity =>
        {
            entity.ToTable("WorkflowSteps");
            entity.Property(e => e.Status).HasConversion<string>();
            entity.HasOne(s => s.Workflow)
                  .WithMany(w => w.Steps)
                  .HasForeignKey(s => s.WorkflowId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(s => s.AssignedToUser)
                  .WithMany()
                  .HasForeignKey(s => s.AssignedToUserId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<WorkflowAction>(entity =>
        {
            entity.ToTable("WorkflowActions");
            entity.Property(e => e.Action).HasConversion<string>();
            entity.HasIndex(e => e.WorkflowId);
            entity.HasOne(a => a.Workflow)
                  .WithMany(w => w.Actions)
                  .HasForeignKey(a => a.WorkflowId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(a => a.ByUser)
                  .WithMany()
                  .HasForeignKey(a => a.ByUserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs");
            entity.HasIndex(e => e.WorkflowId);
            entity.HasIndex(e => e.CreatedAt);
            entity.Property(e => e.OldValue).HasColumnType("jsonb");
            entity.Property(e => e.NewValue).HasColumnType("jsonb");
            entity.HasOne(a => a.Workflow)
                  .WithMany()
                  .HasForeignKey(a => a.WorkflowId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<WorkflowHistory>(entity =>
        {
            entity.ToTable("WorkflowHistory");
            entity.HasIndex(e => e.WorkflowId);
            entity.Property(e => e.FromStatus).HasConversion<string>();
            entity.Property(e => e.ToStatus).HasConversion<string>();
            entity.HasOne(h => h.Workflow)
                  .WithMany(w => w.History)
                  .HasForeignKey(h => h.WorkflowId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

