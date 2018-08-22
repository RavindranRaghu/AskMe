namespace ChaT.db
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ChatIntent")]
    public partial class ChatIntent
    {
        [Key]
        [Column(Order = 0)]
        public int ChatIntentId { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(255)]
        public string IntentName { get; set; }

        [StringLength(1000)]
        public string IntentDescription { get; set; }

        [Key]
        [Column(Order = 2, TypeName = "datetime2")]
        public DateTime UpdatedDate { get; set; }
    }
}
