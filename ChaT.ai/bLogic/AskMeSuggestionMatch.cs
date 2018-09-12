using AskMe.ai;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ChaT.db;


namespace ChaT.ai.bLogic
{
    public class AskMeSuggestionMatch
    {

        private string Message = string.Empty;
        private int Node = 0;
        private ChatDatabaseModel db;

        public AskMeSuggestionMatch(string message, int node)
        {
            this.Message = message.ToLower();
            this.Node = node;
            db = new ChatDatabaseModel();
        }

        public KeyValuePair<int, bool> FullMatch (List<ChatIntent> intentList)
        {
            int matchingIntent = 0;
            bool hasMatch = false;
            var intentMatch = intentList.Where(x=>x.IntentDescription.ToLower() == Message);
            if (intentMatch.Any())
            {
                hasMatch = true;
                matchingIntent = intentMatch.Select(x => x.ChatIntentId).FirstOrDefault();
            }
            return new KeyValuePair<int, bool>(matchingIntent, hasMatch);
        }

        public KeyValuePair<int, bool> PartialMatch(List<ChatIntent> intentList)
        {
            bool hasMatch = false;
            int highestMatchIntent = 0;
            Dictionary<int, int> matchingIntentScore = new Dictionary<int, int>();
            LevenshteinDistance dist = new LevenshteinDistance();
            foreach (ChatIntent intent in intentList)
            {
                int matching = dist.Compute(Message, intent.IntentDescription.ToLower());
                if (matching <=4)
                {
                    matchingIntentScore.Add(intent.ChatIntentId, matching);
                    hasMatch = true;
                }
                    
            }
            if (matchingIntentScore.Count>0)
                highestMatchIntent = matchingIntentScore.OrderBy(x => x.Value).FirstOrDefault().Key;            
            return new KeyValuePair<int,bool>(highestMatchIntent, hasMatch);
        }

    }
}