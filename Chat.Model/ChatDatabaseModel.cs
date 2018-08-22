namespace ChaT.Model
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class ChatDatabaseModel : DbContext
    {
        public ChatDatabaseModel()
            : base("name=ChatDatabaseModel")
        {
        }

        public virtual DbSet<ChatEntity> ChatEntities { get; set; }
        public virtual DbSet<ChatIntent> ChatIntents { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ChatEntity>()
                .Property(e => e.EntityName)
                .IsUnicode(false);

            modelBuilder.Entity<ChatEntity>()
                .Property(e => e.EntityDescription)
                .IsUnicode(false);

            modelBuilder.Entity<ChatEntity>()
                .Property(e => e.ChatIntentId)
                .IsUnicode(false);

            modelBuilder.Entity<ChatIntent>()
                .Property(e => e.IntentName)
                .IsUnicode(false);

            modelBuilder.Entity<ChatIntent>()
                .Property(e => e.IntentDescription)
                .IsUnicode(false);
        }
    }
}
