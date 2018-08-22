namespace ChaT.Model
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
        [Column(Order = 0)]
        public int ChatEntityId { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(255)]
        public string EntityName { get; set; }

        [StringLength(1000)]
        public string EntityDescription { get; set; }

        [StringLength(1000)]
        public string ChatIntentId { get; set; }

        [Key]
        [Column(Order = 2, TypeName = "datetime2")]
        public DateTime UpdatedDate { get; set; }
    }
}
