using System;
using System.Threading;
using Akka.Actor;
using project_3.Messages;

namespace project_3
{
    public class LogActor : ReceiveActor
    {
        public LogActor()
        {
            Receive<LogMessage>(msg =>
            {
                string time = msg.Time.ToString("HH:mm:ss.fff");
                Console.WriteLine($"[{time}] [T-{msg.SenderThreadId}] {msg.Text}");
            });
        }
    }
    public static class Logger
    {
        private static IActorRef logActor;

        public static void Init(IActorRef actor) => logActor = actor;

        public static void Log(string message)
        {
            var msg = new LogMessage(message, Thread.CurrentThread.ManagedThreadId);

            if (logActor != null)
                logActor.Tell(msg);
            else
                Console.WriteLine($"[{msg.Time:HH:mm:ss.fff}] [T-{msg.SenderThreadId}] {msg.Text}");
        }
    }
}
