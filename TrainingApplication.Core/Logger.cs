using System;
using System.Runtime.CompilerServices;

namespace TrainingApplication.Core
{
    public static class Logger
    {
        public static void Log(string message = "", [CallerFilePath] string callerClass = "failed class",
            [CallerMemberName] string callerMethod = "failed method")
        {
            string className = callerClass.Substring(callerClass.LastIndexOf("\\", StringComparison.Ordinal) + 1)
                .Replace(".cs", "");

            Console.WriteLine($"[{className}: {callerMethod}] --> {message}");
        }
    }
}