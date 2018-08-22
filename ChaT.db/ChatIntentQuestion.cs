namespace ChaT.db
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ChatIntentQuestion")]
    public partial class ChatIntentQuestion
    {
        [Key]
        [Column(Order = 0)]
        public int QuestionId { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(2300)]
        public string QuestionDesc { get; set; }

        public int ChatIntentId { get; set; }

        [Key]
        [Column(Order = 2, TypeName = "datetime2")]
        public DateTime UpdatedDate { get; set; }
    }
}
