using HMS.Models;
using Microsoft.EntityFrameworkCore;

namespace HMS.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<LoginSession> LoginSessions { get; set; }
        public DbSet<Topic> Topics { get; set; }
        public DbSet<LabMachine> LabMachines { get; set; }
        public DbSet<Exam> Exams { get; set; }
        public DbSet<ExamTopic> ExamTopics { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Choice> Choices { get; set; }
        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.IdNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.Gender).IsRequired().HasColumnType("char(1)");
                entity.Property(e => e.Role).IsRequired().HasConversion<string>().HasMaxLength(50);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                
            });

            // Configure LoginSession entity
            modelBuilder.Entity<LoginSession>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OtpCode).HasMaxLength(6);
                entity.Property(e => e.DeviceInfo).HasMaxLength(255);
                entity.Property(e => e.IpAddress).HasMaxLength(45); // IPv6 can be up to 45 characters
                entity.Property(e => e.IsLoggedIn).HasDefaultValue(false);
                
                // entity.Property(e => e.OtpExpireAt).IsRequired(); // Removed as OtpExpireAt is removed from LoginSession
                
                
                

                entity.HasOne<User>()
                      .WithMany()
                      .HasForeignKey(ls => ls.UserId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Topic entity
            modelBuilder.Entity<Topic>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Description).HasMaxLength(1000);
                
            });

            // Configure LabMachine entity
            modelBuilder.Entity<LabMachine>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.LabCode).IsRequired().HasMaxLength(50);
                entity.Property(e => e.MachineName).HasMaxLength(100);
                entity.Property(e => e.DeviceId).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Status).IsRequired().HasConversion<string>().HasMaxLength(20); // Store enum as string
                
            });

            // Configure Exam entity
            modelBuilder.Entity<Exam>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Description).IsRequired().HasColumnType("longtext"); // For 'text' type
                entity.Property(e => e.DurationMinutes).IsRequired();
                entity.Property(e => e.StartTime).HasDefaultValue(null); // Nullable
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.IsActive).HasDefaultValue(false);
                entity.Property(e => e.Batch).IsRequired();
                
            });

            // Configure ExamTopic entity
            modelBuilder.Entity<ExamTopic>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ExamId).IsRequired();
                entity.Property(e => e.TopicId).IsRequired();
                entity.Property(e => e.TotalQuestions).IsRequired();
                entity.Property(e => e.UserId).IsRequired();
                
                
                

                entity.HasIndex(e => new { e.ExamId, e.TopicId, e.UserId }).IsUnique(); // Unique key

                entity.HasOne(e => e.Exam)
                      .WithMany(ex => ex.ExamTopics)
                      .HasForeignKey(e => e.ExamId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Topic)
                      .WithMany()
                      .HasForeignKey(e => e.TopicId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Restrict); // Prevent deleting topic if exam topics exist

                entity.HasOne(e => e.User) // Instructor
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Restrict); // Prevent deleting user if exam topics exist
            });

            // Configure Question entity
            modelBuilder.Entity<Question>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ExamId).IsRequired();
                entity.Property(e => e.ExamTopicId).IsRequired();
                entity.Property(e => e.Content).IsRequired().HasColumnType("longtext");
                entity.Property(e => e.Marks).IsRequired();

                entity.HasOne(e => e.Exam)
                      .WithMany()
                      .HasForeignKey(e => e.ExamId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.ExamTopic)
                      .WithMany()
                      .HasForeignKey(e => e.ExamTopicId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Restrict); // Prevent deleting exam topic if questions exist
            });

            // Configure Choice entity
            modelBuilder.Entity<Choice>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.QuestionId).IsRequired();
                entity.Property(e => e.ChoiceText).IsRequired().HasColumnType("longtext");
                entity.Property(e => e.IsCorrect).HasDefaultValue(false);

                entity.HasOne(e => e.Question)
                      .WithMany(q => q.Choices)
                      .HasForeignKey(e => e.QuestionId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Cascade);
            });

            
        }
        public override int SaveChanges()
        {
            AddTimestamps();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            AddTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void AddTimestamps()
        {
            var entities = ChangeTracker.Entries()
                .Where(x => x.Entity is BaseEntity && (x.State == EntityState.Added || x.State == EntityState.Modified));

            foreach (var entity in entities)
            {
                var now = DateTime.UtcNow; // current datetime

                if (entity.State == EntityState.Added)
                {
                    ((BaseEntity)entity.Entity).CreatedAt = now;
                }
                ((BaseEntity)entity.Entity).UpdatedAt = now;
            }
        }
    }
}