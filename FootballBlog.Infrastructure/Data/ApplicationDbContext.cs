using FootballBlog.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace FootballBlog.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<PostTag> PostTags => Set<PostTag>();
    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
    public DbSet<LiveMatch> LiveMatches => Set<LiveMatch>();
    public DbSet<MatchEvent> MatchEvents => Set<MatchEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // PostTag — composite primary key
        modelBuilder.Entity<PostTag>()
            .HasKey(pt => new { pt.PostId, pt.TagId });

        modelBuilder.Entity<PostTag>()
            .HasOne(pt => pt.Post)
            .WithMany(p => p.PostTags)
            .HasForeignKey(pt => pt.PostId);

        modelBuilder.Entity<PostTag>()
            .HasOne(pt => pt.Tag)
            .WithMany(t => t.PostTags)
            .HasForeignKey(pt => pt.TagId);

        // Post
        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasIndex(p => p.Slug).IsUnique();
            entity.Property(p => p.Title).HasMaxLength(500).IsRequired();
            entity.Property(p => p.Slug).HasMaxLength(500).IsRequired();

            entity.HasOne(p => p.Category)
                  .WithMany(c => c.Posts)
                  .HasForeignKey(p => p.CategoryId);

            entity.HasOne(p => p.Author)
                  .WithMany(u => u.Posts)
                  .HasForeignKey(p => p.AuthorId);
        });

        // Category
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasIndex(c => c.Slug).IsUnique();
            entity.Property(c => c.Name).HasMaxLength(200).IsRequired();
            entity.Property(c => c.Slug).HasMaxLength(200).IsRequired();
        });

        // Tag
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasIndex(t => t.Slug).IsUnique();
            entity.Property(t => t.Name).HasMaxLength(200).IsRequired();
            entity.Property(t => t.Slug).HasMaxLength(200).IsRequired();
        });

        // ApplicationUser
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.HasIndex(u => u.Username).IsUnique();
            entity.Property(u => u.Role).HasMaxLength(50).HasDefaultValue("Author");
        });

        // LiveMatch
        modelBuilder.Entity<LiveMatch>(entity =>
        {
            entity.HasIndex(m => m.ExternalId).IsUnique();
            entity.Property(m => m.Status).HasMaxLength(50).HasDefaultValue("SCHEDULED");
        });

        // MatchEvent
        modelBuilder.Entity<MatchEvent>(entity =>
        {
            entity.HasOne(e => e.Match)
                  .WithMany(m => m.Events)
                  .HasForeignKey(e => e.MatchId);

            entity.Property(e => e.Type).HasMaxLength(50).IsRequired();
        });
    }
}
