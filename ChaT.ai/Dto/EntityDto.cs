using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ChaT.ai.Dto
{
    public class EntitytDto
    {
        public int ChatEntityId { get; set; }

        public string EntityName { get; set; }

        public string EntityDescription { get; set; }

        public string EntityType { get; set; }

        public int ChatIntentId { get; set; }

        public string ChatIntentName { get; set; }

        public DateTime UpdatedDate { get; set; }

    }

    public class EntityOperation
    {
        public EntitytDto entity { get; set; }
        public string Operation { get; set; }
    }

}