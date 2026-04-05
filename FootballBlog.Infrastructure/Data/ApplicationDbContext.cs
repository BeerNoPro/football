using FootballBlog.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FootballBlog.Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>(options)
{
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<PostTag> PostTags => Set<PostTag>();
    public DbSet<LiveMatch> LiveMatches => Set<LiveMatch>();
    public DbSet<MatchEvent> MatchEvents => Set<MatchEvent>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<MatchPrediction> MatchPredictions => Set<MatchPrediction>();

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

        // LiveMatch
        modelBuilder.Entity<LiveMatch>(entity =>
        {
            entity.HasIndex(m => m.ExternalId).IsUnique();
            entity.Property(m => m.Status)
                  .HasConversion<string>()
                  .HasMaxLength(20)
                  .HasDefaultValue(MatchStatus.Scheduled);

            // FK → Match (nullable: LiveMatch có thể tồn tại trước khi Match được fetch)
            entity.HasOne(m => m.Match)
                  .WithOne(m => m.LiveMatch)
                  .HasForeignKey<LiveMatch>(m => m.MatchId)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // MatchEvent
        modelBuilder.Entity<MatchEvent>(entity =>
        {
            entity.HasOne(e => e.LiveMatch)
                  .WithMany(m => m.Events)
                  .HasForeignKey(e => e.LiveMatchId);

            entity.Property(e => e.Type).HasMaxLength(50).IsRequired();
        });

        // Match (từ Football API)
        modelBuilder.Entity<Match>(entity =>
        {
            entity.HasIndex(m => m.ExternalId).IsUnique();
            entity.Property(m => m.HomeTeam).HasMaxLength(200).IsRequired();
            entity.Property(m => m.AwayTeam).HasMaxLength(200).IsRequired();
            entity.Property(m => m.LeagueName).HasMaxLength(200).IsRequired();
            entity.Property(m => m.Season).HasMaxLength(20).IsRequired();
            entity.Property(m => m.Round).HasMaxLength(100);
            entity.Property(m => m.VenueName).HasMaxLength(200);
            entity.Property(m => m.RefereeName).HasMaxLength(200);
            entity.Property(m => m.Status)
                  .HasConversion<string>()
                  .HasMaxLength(20)
                  .HasDefaultValue(MatchStatus.Scheduled);
        });

        // MatchPrediction — 1-to-1 với Match
        modelBuilder.Entity<MatchPrediction>(entity =>
        {
            entity.HasIndex(p => p.MatchId).IsUnique();
            entity.Property(p => p.AIProvider).HasMaxLength(50).IsRequired();
            entity.Property(p => p.AIModel).HasMaxLength(100).IsRequired();
            entity.Property(p => p.PredictedOutcome).HasMaxLength(20).IsRequired();
            entity.Property(p => p.ConfidenceScore).HasPrecision(5, 2);

            entity.HasOne(p => p.Match)
                  .WithOne(m => m.Prediction)
                  .HasForeignKey<MatchPrediction>(p => p.MatchId);

            entity.HasOne(p => p.BlogPost)
                  .WithMany()
                  .HasForeignKey(p => p.BlogPostId)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
