using AskMe.ai;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ChaT.db;
using System.Net;
using ChaT.ai.Dto;

namespace ChaT.ai.bLogic
{
    public class AskMeSynonymMatch
    {

        private string Message = string.Empty;
        private int Node = 0;
        private ChatDatabaseModel db;
        private AskMeContentManager contentManager = new AskMeContentManager();
        private string url = "https://dictionary.yandex.net/api/v1/dicservice.json/lookup?key=dict.1.1.20180917T232657Z.c6eb5a9382f056e6.f62d425e1c55499427bc0a47fd49566e5b200051&lang=en-ru&text=";
        public AskMeSynonymMatch(string message, int node)
        {
            this.Message = message.ToLower();
            this.Node = node;
            db = new ChatDatabaseModel();
        }

        public KeyValuePair<string, bool> SynonymMatch (List<string> vocabList)
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
                string json;
                url = url + vocab;
                List<string> synonymList = new List<string>();
                using (WebClient client = new WebClient())
                {
                    try
                    {
                        json = client.DownloadString(url);
                        SynonymDto synonym = Newtonsoft.Json.JsonConvert.DeserializeObject<SynonymDto>(json);                        
                        foreach(var def in synonym.def)
                        {
                            foreach(var tr in def.tr)
                            {
                                foreach ( var mean in tr.mean)
                                {
                                    synonymList.Add(mean.text);
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {

                    }                    
                }



                foreach (string intentDesc in intentList.Select(x => x.IntentDescription).ToList())
                {
                    List<string> intentvocabList = getVocab.GetVocabulary(intentDesc);

                    bool hasSynonm = synonymList.Intersect(intentvocabList).Any();
                    if (hasSynonm && counter <= 3)
                    {
                        counter = counter + 1;
                        responseList.Add(intentDesc);
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



