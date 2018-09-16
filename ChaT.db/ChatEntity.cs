namespace ChaT.db
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ChatEntity")]
    public partial class ChatEntity
    {
        [Key]
        public int ChatEntityId { get; set; }

        [StringLength(255)]
        public string EntityName { get; set; }

        [StringLength(1000)]
        public string EntityDescription { get; set; }

        public int ChatIntentId { get; set; }

        public DateTime UpdatedDate { get; set; }
    }
}
