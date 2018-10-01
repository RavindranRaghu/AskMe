using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChaT.ai.Dto
{
    public class EntityIdentifiedDto
    {
        public string EntityName { get; set; }
        public string EntityValue { get; set; }
        public string ChatResponse { get; set; }
        public string MatchConfidence { get; set; }        
    }
}