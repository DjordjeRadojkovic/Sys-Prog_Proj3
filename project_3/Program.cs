using System;
using System.IO;
using System.Net;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;
using Akka.Routing;
using project_3;
using project_3.Messages;
using System.Web;

namespace proj3
{
    public class Program
    {
        const int PORT = 5050;
        const int MAX_WORKERS = 5;
        const int MAX_CACHED_FILES = 5;
        static async Task Main(string[] args)
        {
            Config config = ConfigurationFactory.ParseString(@"
                worker-dispatcher {
                    type = Dispatcher
                    throughput = 5
                    fork-join-executor {
                        parallelism-min = 2
                        parallelism-factor = 1.0
                        parallelism-max = 8
                    }
                }");

            ActorSystem system = ActorSystem.Create("NewsAPICallSystem", config);

            IActorRef logActor = system.ActorOf(Props.Create<LogActor>(), "logger");
            Logger.Init(logActor);

            IActorRef cacheActor = system.ActorOf(Props.Create(() => new CacheActor(MAX_CACHED_FILES)), "cache");

            IActorRef workerRouter = system.ActorOf(
                Props.Create(() => new SearchActor(cacheActor))
                     .WithRouter(new RoundRobinPool(MAX_WORKERS))
                     .WithDispatcher("worker-dispatcher"),
                "workers");

            HttpListener listener = new HttpListener();
            listener.Prefixes.Add($"http://localhost:{PORT}/");
            listener.Start();
            Logger.Log("Server Started!");
            IObservable<HttpListenerContext> requestStream =
                Observable.FromAsync(() => listener.GetContextAsync())
                          .Repeat()
                          .Catch(Observable.Empty<HttpListenerContext>());

            IDisposable subscription = requestStream
                .ObserveOn(TaskPoolScheduler.Default)
                .Do(ctx => Logger.Log($"Primljen zahtev: {HttpHelper.GetKeyWord(ctx.Request.Url.Query)}"))
                .Select(ctx => new { Context = ctx, MyQuery = ctx.Request.Url.Query })
                .Where(x =>
                {
                    if (string.IsNullOrEmpty(x.MyQuery))
                    {
                        Logger.Log("ODBIJEN (400): Nije navedena kljucna rec!");
                        HttpHelper.SendError(x.Context, 400, "Navedite keyword!");
                        return false;
                    }
                    return true;
                })
                .Select(x => new RequestMsg(x.Context, x.MyQuery))
                .Subscribe(
                    msg => workerRouter.Tell(msg),
                    ex => Logger.Log($"GRESKA u stream-u: {ex.Message}"),
                    () => Logger.Log("Stream zahteva zavrsen"));

            var exit = new TaskCompletionSource<bool>();
            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                exit.TrySetResult(true);
            };
            await exit.Task;

            listener.Stop();
            subscription.Dispose();
            Logger.Log("Stopped!");
            await system.Terminate();
        }
    }
}
