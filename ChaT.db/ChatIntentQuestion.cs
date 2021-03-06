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
        public int QuestionId { get; set; }

        [StringLength(2300)]
        public string QuestionDesc { get; set; }

        public int ChatIntentId { get; set; }

        public DateTime UpdatedDate { get; set; }
    }
}
