using AskMe.ai;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ChaT.db;


namespace ChaT.ai.bLogic
{
    public class AskMezPossibleMatch
    {

        private string Message = string.Empty;
        private int Node = 0;
        private ChatDatabaseModel db;
        private AskMeContentManager contentManager = new AskMeContentManager();        
        public AskMezPossibleMatch(string message, int node)
        {
            this.Message = message.ToLower();
            this.Node = node;
            db = new ChatDatabaseModel();
        }

        public KeyValuePair<string, bool> ProbableMatchCorrectSpelling (List<string> vocabList)
        {
            bool hasMatch = false;
            List<ChatIntent> intentList =  db.ChatIntent.ToList();
            string responseMessage = contentManager.IntentPossibleMatchedResponse;
            int counter = 0;
            foreach (string vocab in vocabList)
            {                
                foreach (var intent in intentList)
                {
                    if(intent.IntentDescription.ToLower().Contains(vocab.ToLower()) && counter <= 3)
                    {
                        responseMessage = responseMessage + "<br>";
                        responseMessage = responseMessage + intent.IntentDescription;
                        counter = counter + 1;
                    }
                }
            }

            if (counter > 0)
                return new KeyValuePair<string, bool>(responseMessage, true);            

            return new KeyValuePair<string, bool>(responseMessage, hasMatch);
        }

        public KeyValuePair<string, bool> ProbableMatchTypoError (List<string> vocabList)
        {
            bool hasMatch = false;
            List<ChatIntent> intentList = db.ChatIntent.ToList();
            string responseMessage = contentManager.IntentPossibleMatchedResponse;
            int counter = 0;

            LevenshteinDistance dist = new LevenshteinDistance();
            TFIDF getVocab = new TFIDF();
            List<string> responseList = new List<string>();
            foreach (string vocab in vocabList)
            {

                foreach (string intentDesc in intentList.Select(x => x.IntentDescription).ToList())
                {
                    List<string> intentvocabList = getVocab.GetVocabulary(intentDesc);
                    foreach (string intent in intentvocabList)
                    {
                        if (dist.Compute(vocab, intent.ToLower()) < 2 && counter <= 3)
                        {
                            if (!responseList.Where(x => x.ToString() == intentDesc).Any())
                            {
                                counter = counter + 1;
                                responseList.Add(intentDesc);
                            }
                        }
                    }
                }
            }
            responseList = (responseList.Count > 1) ? responseList.Distinct().Take(3).ToList() : responseList;
            foreach (string response in responseList)
            {
                responseMessage = responseMessage + "<br>";
                responseMessage = responseMessage + response;
            }

            responseMessage = responseMessage + "<br>" + contentManager.IntentSuggestionResponse;

            if (counter > 0)
                return new KeyValuePair<string, bool>(responseMessage, true);

            return new KeyValuePair<string, bool>(responseMessage, hasMatch);
        }

    }
}



