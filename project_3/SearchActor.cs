using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using project_3.Messages;
using System.Reactive.Linq;
using static System.Net.WebRequestMethods;
using dotenv.net;
using System.Text.Json.Nodes;
using Newtonsoft.Json.Linq;

namespace project_3
{
    public class SearchActor : ReceiveActor
    {
        IActorRef cacheActor;
        public SearchActor(IActorRef cA)
        {
            cacheActor = cA;

            Receive<RequestMsg>(msg =>
            {
                cacheActor.Tell(msg);
            });
            Receive<SearchResault>(msg => SendResponse(msg));
            Receive<CacheMiss>(msg => NewsAPICall(msg));
        }

        private void SendResponse(SearchResault msg)
        {
            var context = msg.request._context;
            string keyword = msg.request.GetKeyword();

            try
            {
                if (msg.articles == null)
                {
                    HttpHelper.SendError(context, 404, $"Artikli ne postoje: {keyword}");
                    Logger.Log($"[{Self.Path.Name}] 404: {keyword}");
                    return;
                }

                int code = 200;
                string message = string.Empty;
                foreach (var article in msg.articles)
                {
                    message += $"<li>{article.title} : {article.sentiment}</li>";
                }

                string html = $"<html><body style='font-family:sans-serif;text-align:center;padding:50px'>" +
                              $"<h1>{code}</h1><ul style='list-style-type: none;'>{message}</ul></body></html>";
                byte[] data = Encoding.UTF8.GetBytes(html);
                context.Response.StatusCode = code;
                context.Response.ContentType = "text/html; charset=utf-8";
                context.Response.ContentLength64 = data.Length;
                context.Response.OutputStream.Write(data, 0, data.Length);
                context.Response.OutputStream.Close();

                Logger.Log($"[{Self.Path.Name}] 200: {keyword}");
            }
            catch (Exception ex)
            {
                Logger.Log($"[{Self.Path.Name}] GRESKA pri slanju odgovora za {keyword}: {ex.Message}");
                try { HttpHelper.SendError(context, 500, "Interna greska servera"); } catch { }
            }
        }
        private void NewsAPICall(CacheMiss msg)
        {
            var replyTo = Sender;
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add(
            "User-Agent",
            "NewsSentimentApp/1.0");
            DotEnv.Load();
            string? api_key = Environment.GetEnvironmentVariable("API_KEY");
            string url = $"https://newsapi.org/v2/everything?q={msg.keyword.Trim()}&apiKey={api_key}";
            IObservable<string> ApiCall(string url)
            {
                return Observable.FromAsync(async ct =>
                {
                    var response = await client.GetAsync(url, ct);
                    
                    try
                    {
                        response.EnsureSuccessStatusCode();
                    }catch(HttpRequestException ex)
                    {
                        Logger.Log($"Greska pri Api pozivu: {msg.keyword}\n{ex.Message}");
                        HttpHelper.SendError(msg.request._context, 400, ex.Message);
                        Sender.Tell(new ApiError(msg.keyword));
                    }
                    return await response.Content.ReadAsStringAsync();
                });
            }

            ApiCall(url)
                .Subscribe(
                data =>
                {
                    JObject parsed = JObject.Parse(data);
                    List<Article> articles = new List<Article>();
                    foreach (var item in parsed["articles"])
                    {
                        articles.Add(new Article
                        {
                            title = item["title"].ToString(),
                            content = item["content"].ToString()
                            //senti analiza
                        });

                    }
                    replyTo.Tell(new SaveCache(msg.keyword, articles));
                },
                ex =>
                {
                    Logger.Log($"Greska pri Api pozivu: {msg.keyword}\n{ex.Message}");
                    HttpHelper.SendError(msg.request._context, 400, ex.Message);
                    replyTo.Tell(new ApiError(msg.keyword));
                });
        }
    }
}
