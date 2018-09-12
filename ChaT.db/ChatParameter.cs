namespace ChaT.db
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("dbo.ChatParameter")]
    public partial class ChatParameter
    {
        [Key]
        public int ParameterId { get; set; }

        [StringLength(2300)]
        public string ParameterName { get; set; }

        [StringLength(2300)]
        public string ParameterValue { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime UpdatedDate { get; set; }
    }
}
