namespace ChaT.db
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

        public virtual DbSet<ChatEntity> ChatEntity { get; set; }
        public virtual DbSet<ChatIntent> ChatIntent { get; set; }
        public virtual DbSet<ChatIntentQuestion> ChatIntentQuestion { get; set; }
        public virtual DbSet<ChatParameter> ChatParameter { get; set; }
        public virtual DbSet<ChatFailureResponse> ChatFailureResponse { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Database.SetInitializer<ChatDatabaseModel>(null);
            base.OnModelCreating(modelBuilder);
        }
    }
}
