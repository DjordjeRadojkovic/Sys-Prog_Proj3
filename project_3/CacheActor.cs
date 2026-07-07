using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using project_3.Messages;

namespace project_3
{
    internal class CacheActor : ReceiveActor
    {
        private class CacheEntry
        {
            public DateTime cachedAt {  get; set; }
            public List<Article> articles { get; set; }
        }
        private int maxCount;

        Dictionary<string, CacheEntry> _cache = new Dictionary<string, CacheEntry>();
        Dictionary<string, List<(IActorRef requester, RequestMsg request)>> _waiting = new Dictionary<string, List<(IActorRef,RequestMsg)>>();
        public CacheActor(int max)
        {
            maxCount = max;

            Receive<RequestMsg>(msg  => HandleRequest(msg));
            Receive<SaveCache>(msg => HandleSaveCache(msg));
            Receive<ApiError>(msg => {
                if (!_waiting.TryGetValue(msg.keyword, out var waiters))
                    return;
                _waiting.Remove(msg.keyword);
            });
        }

        public void HandleRequest(RequestMsg msg)
        {
            //check cache
            if(_cache.TryGetValue(msg.GetKeyword(), out CacheEntry entry))
            {
                entry.cachedAt = DateTime.Now;
                Logger.Log($"Cache HIT: {msg.GetKeyword()}");
                Sender.Tell(new SearchResault(msg,entry.articles));
                return;
            }

            if(_waiting.TryGetValue(msg.GetKeyword(),out var waiters))
            {
                Logger.Log($"Cache wait: {msg.GetKeyword()}");
                waiters.Add((Sender,msg));
                return;
            }

            _waiting[msg.GetKeyword()] = new List<(IActorRef,RequestMsg)> { (Sender, msg) };
            Sender.Tell(new CacheMiss(msg.GetKeyword(),msg));

            /*Logger.Log($"Cache MISS: {msg.GetKeyword()}");
            Observable.Start(() => NewsAPICall(msg), TaskPoolScheduler.Default)
                .ToTask()
                .PipeTo(Self,
                success: data => new SearchResault(msg, data),
                failure: ex =>
                {
                    Logger.Log($"Greska pri API pozivu: {msg.GetKeyword()}: {ex.Message}");
                    return new SearchResault(msg, null);
                });*/
        }
        private void HandleSaveCache(SaveCache msg)
        {
            if (!_waiting.TryGetValue(msg.KeyWord, out var waiters))
                return;
            _waiting.Remove(msg.KeyWord);

            if(msg.articles != null)
            {
                if(_cache.Count >=5)
                {
                    KeyValuePair<string,CacheEntry> oldest = _cache.OrderBy(x=>x.Value.cachedAt).FirstOrDefault();
                    _cache.Remove(oldest.Key);
                    Logger.Log($"Cache evict: {oldest.Key}");
                }
                _cache[msg.KeyWord] = new CacheEntry { articles = msg.articles, cachedAt=DateTime.Now };
                Logger.Log($"Cache add: {msg.KeyWord}");
            }
            foreach(var (requester, request) in waiters)
            {
                requester.Tell(new SearchResault(request, msg.articles));
            }
        }
    }
}
