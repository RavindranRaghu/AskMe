using ChaT.db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChaT.ai.Dto
{
    public class FeatureDto
    {
        public List<ChatFeatureList> feature { get; set; }
        public List<ChatSubFeatureList> subfeature { get; set; }
    }

    public class FeatureOperation
    {
        public ChatFeatureList feature { get; set; }
        public string Operation { get; set; }
    }

    public class SubFeatureOperation
    {
        public ChatSubFeatureList subfeature { get; set; }
        public string Operation { get; set; }
    }
}