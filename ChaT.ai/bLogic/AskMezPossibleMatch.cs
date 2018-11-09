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

        public KeyValuePair<string, bool> ProbableMatchCorrectSpelling (List<string> vocabList, List<ChatIntent> intentList)
        {
            bool hasMatch = false;
            string responseMessage = contentManager.IntentPossibleMatchedResponse;
            int counter = 0;
            foreach (string vocab in vocabList)
            {                
                foreach (var intent in intentList)
                {
                    bool IsRedirect = CheckIfRedirect(intent, intentList);
                    if (!IsRedirect && intent.IntentDescription.ToLower().Contains(vocab.ToLower()) && counter <= 3)
                    {
                        responseMessage = responseMessage + "<br>";
                        responseMessage = responseMessage + intent.IntentDescription;
                        counter = counter + 1;
                    }
                }
            }
            responseMessage = responseMessage + "<br>" + contentManager.IntentSuggestionResponse;

            if (counter > 0)
                return new KeyValuePair<string, bool>(responseMessage, true);            

            return new KeyValuePair<string, bool>(responseMessage, hasMatch);
        }

        public KeyValuePair<string, bool> ProbableMatchTypoError (List<string> vocabList, List<ChatIntent> intentList)
        {
            bool hasMatch = false;
            string responseMessage = contentManager.IntentPossibleMatchedResponse;
            int counter = 0;

            LevenshteinDistance dist = new LevenshteinDistance();
            TFIDF getVocab = new TFIDF();
            List<string> responseList = new List<string>();
            foreach (string vocab in vocabList)
            {

                foreach (ChatIntent intent in intentList)
                {
                    bool IsRedirect = CheckIfRedirect(intent, intentList);
                    if (IsRedirect)
                        continue;
                    List<string> intentvocabList = getVocab.GetVocabulary(intent.IntentDescription);
                    foreach (string intentVocab in intentvocabList)
                    {                        
                        if (dist.Compute(vocab, intentVocab.ToLower()) < 2 && counter <= 3)
                        {
                            if (!responseList.Where(x => x.ToString() == intent.IntentDescription).Any())
                            {
                                counter = counter + 1;
                                responseList.Add(intent.IntentDescription);
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

        public bool CheckIfRedirect (ChatIntent intent, List<ChatIntent> intentList)
        {
            if (intent.IsRedirect)
                return true;
            ChatIntent parent = intentList.Where(x => x.ChatIntentId == intent.ParentId).FirstOrDefault();
            if (parent.IsRedirect)
                return true;
            if (parent.ChatIntentId == 0)
                return false;

            return CheckIfRedirect(parent, intentList);
        }

    }
}



