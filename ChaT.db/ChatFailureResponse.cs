
namespace ChaT.db
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ChatFailureResponse")]
    public partial class ChatFailureResponse
    {
        [Key]
        public int DefectId { get; set; }

        [StringLength(4000)]
        public string QuestionByUser { get; set; }

        public int ParentId { get; set; }

        public bool Reviewed { get; set; }

        public DateTime UpdatedDate { get; set; }
    }
}
