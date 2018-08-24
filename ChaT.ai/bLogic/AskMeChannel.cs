using AskMe.ai;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ChaT.db;

namespace ChaT.ai.bLogic
{
    public class AskMeChannel
    {
        private string Message = string.Empty;
        private int Node = 0;
        private ChatDatabaseModel db;
        public AskMeChannel(string message, int node)
        {
            this.Message = message.ToLower();
            this.Node = node;
            db = new ChatDatabaseModel();
        }

        public KeyValuePair<string, string> ChatResponse()
        {
            string responseMessage = "Sorry, I did not understand";
            string intentMessage = "NoIntentMatched";
            string entity = string.Empty;
            TFIDF getVocab = new TFIDF();

            if (Message.Contains("hello") || Message.Contains("good morning") || Message.Contains("how are you"))
            {
                return new KeyValuePair<string, string>("greet", "Hello");
            }
            else if (Message.Contains("good bye") || Message.Contains("take care") || Message.Contains("cya"))
            {
                return new KeyValuePair<string, string>("goodbye", "Thanks. Have a good one");
            }

            List<string> vocabList = getVocab.GetVocabulary(Message);
            List<ChatIntent> intentList = (from intention in db.ChatIntent
                                           where intention.ChatIntentId > 2
                                           select intention).ToList();


            foreach (string vocab in vocabList)
            {
                var hasIntent = intentList.Where(x=>x.ParentId == Node).Where(x => vocab.Contains(x.IntentName) || x.IntentName.Contains(vocab));
                if (hasIntent.Any())
                {
                    Dictionary<int, string> intentDict = hasIntent.Select(t => new { t.ChatIntentId, t.IntentName}).ToList().ToDictionary(x => x.ChatIntentId, y => y.IntentName);
                    foreach (KeyValuePair<int, string> intent in intentDict)
                    {
                        entity = GetEntityforIntent(intent.Key, vocabList);
                        if (string.IsNullOrEmpty(entity))
                        {
                            responseMessage = intent.Value + " has been processed";
                        }
                        else
                        {
                            responseMessage = intent.Value + " has been processed on " + entity;
                        }
                        
                        intentMessage = intent.Value;
                        return new KeyValuePair<string, string>(intentMessage, responseMessage);
                    }
                }
            }

            Dictionary<int, string> intentNameDict = intentList.Select(t => new { t.ChatIntentId, t.IntentName }).ToList().ToDictionary(x => x.ChatIntentId, y => y.IntentName);
            LevenshteinDistance dist = new LevenshteinDistance();
            foreach (string vocab in vocabList)
            {
                foreach (KeyValuePair< int, string> intent in intentNameDict)
                {
                    if (dist.Compute(vocab, intent.Value) < 4)
                    {
                        entity = GetEntityforIntent(intent.Key, vocabList);
                        if (string.IsNullOrEmpty(entity))
                        {
                            responseMessage = intent.Value + " has been processed";
                        }
                        else
                        {
                            responseMessage = intent.Value + " has been processed on " + entity;
                        }
                        intentMessage = intent.Value;
                        return new KeyValuePair<string, string>(intentMessage, responseMessage);
                    }
                }
            }

            SimilarityCalculator similarityCalculator = new SimilarityCalculator();
            List<ChatIntentQuestion> questionList = db.ChatIntentQuestion.ToList();
            Dictionary<string, int> questions = questionList.Select(t => new { t.QuestionDesc, t.ChatIntentId }).ToList().ToDictionary(x => x.QuestionDesc, y => y.ChatIntentId);
            KeyValuePair<string, int> questionHighestMatch = new KeyValuePair<string, int>();
            double compareHigh = 0;
            foreach (KeyValuePair<string, int> question in questions)
            {
                double compare = similarityCalculator.CompareString(Message, question.Key, 1);
                if (compareHigh < compare)
                {
                    compareHigh = compare;
                    questionHighestMatch = question;
                }
            }

            if (compareHigh > 0)
            {
                string intent = db.ChatIntent.Where(x => x.ChatIntentId == questionHighestMatch.Value).Select(y => y.IntentName).FirstOrDefault();
                entity = GetEntityforIntent(questionHighestMatch.Value, vocabList);
                if (string.IsNullOrEmpty(entity))
                {
                    responseMessage = intent + " has been processed";
                }
                else
                {
                    responseMessage = intent + " has been processed on " + entity;
                }
                intentMessage = intent;
                return new KeyValuePair<string, string>(intentMessage, responseMessage);
            }

            return new KeyValuePair<string, string>(intentMessage, responseMessage);
        }

        private string GetEntityforIntent(int chatIntentId, List<string> vocabList)
        {
            string entity = string.Empty;
            List<ChatEntity> entityList = db.ChatEntity.Where(z => z.ChatIntentId == chatIntentId.ToString()).ToList();

            foreach (string vocab in vocabList)
            {
                var hasEntity = entityList.Where(x => vocab.Contains(x.EntityName) || x.EntityName.Contains(vocab));
                if (hasEntity.Any())
                {
                    List<string> withEntityList = hasEntity.Select(x => x.EntityName).ToList();
                    foreach (string entityName in withEntityList)
                    {
                        entity = entityName;
                        return entity;
                    }
                }
            }

            List<string> entityNames = entityList.Select(x => x.EntityName).ToList();
            LevenshteinDistance dist = new LevenshteinDistance();
            foreach (string vocab in vocabList)
            {
                foreach (string entityName in entityNames)
                {
                    if (dist.Compute(vocab, entityName) < 4)
                    {
                        entity = entityName;
                        return entity;
                    }
                }
            }

            return entity;
        }
    }
}