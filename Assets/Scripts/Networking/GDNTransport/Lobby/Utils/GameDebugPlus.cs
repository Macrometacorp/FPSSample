using System;
using UnityEngine;

//
// Logging of messages
//
// There are three different types of messages:
//
// Debug.Log/Warn/Error coming from unity (or code, e.g. packages, not using GameDebug)
//    These get caught here and sent onto the console and into our log file
// GameDebug.Log/Warn/Error coming from game
//    These gets sent onto the console and into our log file
//    *IF* we are in editor, they are also sent to Debug.* so they show up in editor Console window
// Console.Write
//    Only used for things that should not be logged. Typically reponses to user commands. Only shown on Console.
//

public static class GameDebugPlus {
    static System.IO.StreamWriter logFile = null;
    static public bool forwardToDebug = false;
    
    public const string SEPRATOR = "|";
    public const string INFO = "[I]";
    public const string WARN = "[W]";
    public const string ERROR = "[E]";
        
    public static void Init(string logfilePath, bool isServer, string logBaseName) {
        if (logFile != null) return;
        //forwardToDebug = Application.isEditor;
        Application.logMessageReceived += LogCallback;

        // Try creating logName; attempt a number of suffixxes
        string name = "";
        string cs = "Client";
        if (isServer) {
            cs = "Server";
        }
        logBaseName += cs;
        for (var i = 0; i < 10; i++)
        {
            name = logBaseName + (i == 0 ? "" : "_" + i) + "_plus.log";
            try
            {
                logFile = System.IO.File.CreateText(logfilePath + "/" + name);
                logFile.AutoFlush = true;
                break;
            }
            catch
            {
                name = "<none>";
            }
        }
        Log("Log","GameDebugPlus","Init", "Game Debug Plus initialized. Logging to " + logfilePath + "/" + name);
    }

    public static void Shutdown()
    {
        Application.logMessageReceived -= LogCallback;
        if (logFile != null)
            logFile.Close();
        logFile = null;
    }

    static void LogCallback(string message, string stack, LogType logtype)
    {
        switch (logtype)
        {
            default:
            case LogType.Log:
                GameDebugPlus._AppLog(message);
                break;
            case LogType.Warning:
                GameDebugPlus._LogWarning(message);
                break;
            case LogType.Error:
                GameDebugPlus._LogError(message);
                break;
        }
    }

    public static string UnixTSNowMS() {
        return DateTime.Now.ToUniversalTime()
            .ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");
    }

    public static string Prefix() {
        return "" + UnixTSNowMS() + SEPRATOR + Time.frameCount + SEPRATOR;
    }
    
    public static void Log(string grp, string cls, string method, string msg) {
        var log =  grp + SEPRATOR + cls + SEPRATOR + method + SEPRATOR +
                  msg;
        if (forwardToDebug)
            Debug.Log(log);
        else
            _Log(log);
    }

    static void _AppLog(string message) {
        _Log("AppLog" + SEPRATOR + "" + SEPRATOR + "" + message);
    }
    
    static void _Log(string message) {
        Console.Write(Prefix() + INFO + SEPRATOR  + message);
        if (logFile != null)
            logFile.WriteLine(Prefix() + INFO + SEPRATOR + message);
    }

    public static void LogError(string group, string className, string methodName, string message) {
        var log =  group + SEPRATOR + className + SEPRATOR + methodName + SEPRATOR +
                   message;
        if (forwardToDebug)
            Debug.LogError(log);
        else
            _LogError(log);
    }

    static void _LogError(string message) {
        Console.Write(Prefix() + ERROR + SEPRATOR  + message);
        if (logFile != null)
            logFile.WriteLine(Prefix() + ERROR + SEPRATOR  + message );
    }

    public static void LogWarning(string group, string className, string methodName, string message) {
        var log =  group + SEPRATOR + className + SEPRATOR + methodName + SEPRATOR +
                   message;
        if (forwardToDebug)
            Debug.LogWarning(log);
        else
            _LogWarning(log);
    }

    static void _LogWarning(string message) {
        Console.Write(Prefix() + WARN + SEPRATOR  + message);
        if (logFile != null)
            logFile.WriteLine(Prefix() + WARN + SEPRATOR  + message);
    }

}
