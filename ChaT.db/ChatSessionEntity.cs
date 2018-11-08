namespace ChaT.db
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ChatSessionEntity")]
    public partial class ChatSessionEntity
    {
        [Key]
        public int SessionEntId { get; set; }

        public int SessionId { get; set; }

        public string EntityType { get; set; } 

        public string EntityName { get; set; }

        public string EntityValue { get; set; }

        public bool NotRecognized { get; set; }
    }
}
