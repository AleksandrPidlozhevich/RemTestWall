using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace RemTestWall.Services
{
    public static class LogService
    {
        private static readonly string mutexGuid = Guid.NewGuid().ToString();
        private static Mutex mutex = new Mutex(false, mutexGuid);
        private static string logFileName = typeof(LogService).Namespace;
        private static string applicationPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static string logFolderPath = $@"{applicationPath}\RemTestWall_Data\RemTestWall_Log\{logFileName}";
        private static string logFilePath;
        private static bool isInitialized;

        public static void Initialize(UIApplication app)
        {
            string userName = System.Environment.UserName;
            string revitVersion = app.Application.SubVersionNumber;
            string revitFileName = app.ActiveUIDocument?.Document?.Title.Replace("_" + userName, "");

            mutex.WaitOne();
            if (!Directory.Exists(logFolderPath)) Directory.CreateDirectory(logFolderPath);
            mutex.ReleaseMutex();

            string dateStr = DateTime.Now.ToString("yyyy-MM-dd");
            string dateTimeStr = DateTime.Now.ToString("dd.MM.yyyy_HH:mm");
            logFilePath = Path.Combine(logFolderPath, $"{dateStr}_{logFileName}.log");

            mutex.WaitOne();
            if (!File.Exists(logFilePath)) File.Create(logFilePath).Close();
            mutex.ReleaseMutex();

            List<string> startString = new List<string>
            {
                "\n",
                "====================================================================================",
                $"{logFileName}_start_{dateTimeStr}",
                $"Revit: {revitVersion}  Model: {revitFileName ?? "No Revit document"}",
                "===================================================================================="
            };
            File.AppendAllLines(logFilePath, startString);
            isInitialized = true;
        }

        public static void Info(string message)
        {
            List<string> stringListInfo = new List<string>
            {
                "Info",
                message
            };
            WriteLog(stringListInfo);
        }

        public static void Warn(string message)
        {
            List<string> stringListInfo = new List<string>
            {
                "Warn",
                message
            };
            WriteLog(stringListInfo);
        }

        public static void Error(string message,
                                 [CallerMemberName] string origin = "",
                                 [CallerFilePath] string filePath = "",
                                 [CallerLineNumber] int lineNumber = 0)
        {
            List<string> stringListInfo = new List<string>
            {
                "Error",
                $"{Path.GetFileName(filePath)} > {origin}() > Line {lineNumber}",
                Environment.StackTrace,
                message
            };
            WriteLog(stringListInfo);
        }

        private static void WriteLog(List<string> message)
        {
            if (!isInitialized) return;
            try
            {
                message.Insert(0, DateTime.Now.ToLocalTime().ToString());
                message.Insert(0, "");

                mutex.WaitOne();
                File.AppendAllLines(logFilePath, message);
                mutex.ReleaseMutex();
            }
            catch { }
        }
    }
}
