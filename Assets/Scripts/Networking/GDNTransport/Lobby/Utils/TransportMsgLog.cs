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

public static class TransportMsgLog {
    static System.IO.StreamWriter logFile = null;
    static public bool forwardToDebug = false;
    
    public const string SEPARATOR = "|";
    public const string INFO = "[I]";
    public const string WARN = "[W]";
    public const string ERROR = "[E]";
    public static string appsinstanceId;
        
    public static void Init(string logfilePath, bool isServer, string logBaseName, bool appendTS= true) {
        if (logFile != null) return;
        //forwardToDebug = Application.isEditor;
       // Application.logMessageReceived += LogCallback;

        // Try creating logName; attempt a number of suffixxes
        string name = "";
        string cs = "Client";
        if (isServer) {
            cs = "Server";
        }
        logBaseName += cs;
        if (appendTS) {
            logBaseName += UnixTSNowFileSafe();
        }
        for (var i = 0; i < 10; i++)
        {
            name = logBaseName + (i == 0 ? "" : "_" + i) + "_Transport.log";
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
        Log("Log","TransportMsgLog","Init", "TransportMsgLog initialized. Logging to " + logfilePath + "/" + name);
    }

    public static void Shutdown()
    {
        //Application.logMessageReceived -= LogCallback;
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
                _AppLog(message);
                break;
            case LogType.Warning:
                _LogWarning(message);
                break;
            case LogType.Error:
                _LogError(message);
                break;
        }
    }

    
    public static string UnixTSNowFileSafe() {
        return DateTime.Now.ToUniversalTime()
            .ToString("yyyy'-'MM'-'dd'T'HH'_'mm'_'ss'_'fff'Z'");
    }
    public static string UnixTSNowMS() {
        return DateTime.Now.ToUniversalTime()
            .ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");
    }

    public static string Prefix() {
        return "" + UnixTSNowMS() + SEPARATOR + Time.frameCount + SEPARATOR;
    }
    
    public static void Log(string grp, string cls, string method, string msg) {
        //Debug.Log("Forward logging: "+forwardToDebug);
        var log = appsinstanceId + SEPARATOR + grp + SEPARATOR + cls + SEPARATOR + method +
                  SEPARATOR + msg;
       if (forwardToDebug)
           Debug.Log(log);
       else
            _Log(log);
    }

    static void _AppLog(string message) {
        _Log("AppLog" + SEPARATOR + "" + SEPARATOR + "" + message);
    }
    
    static void _Log(string message) {
        Console.Write(Prefix() + INFO + SEPARATOR  + message);
        if (logFile != null)
            logFile.WriteLine(Prefix() + INFO + SEPARATOR + message);
    }

    public static void LogError(string group, string className, string methodName, string message) {
        var log =  group + SEPARATOR + className + SEPARATOR + methodName + SEPARATOR +
                   message;
        if (forwardToDebug)
            Debug.LogError(log);
        else
            _LogError(log);
    }

    static void _LogError(string message) {
        Console.Write(Prefix() + ERROR + SEPARATOR  + message);
        if (logFile != null)
            logFile.WriteLine(Prefix() + ERROR + SEPARATOR  + message );
    }

    public static void LogWarning(string group, string className, string methodName, string message) {
        var log =  group + SEPARATOR + className + SEPARATOR + methodName + SEPARATOR +
                   message;
        if (forwardToDebug)
            Debug.LogWarning(log);
        else
            _LogWarning(log);
    }

    static void _LogWarning(string message) {
        Console.Write(Prefix() + WARN + SEPARATOR  + message);
        if (logFile != null)
            logFile.WriteLine(Prefix() + WARN + SEPARATOR  + message);
    }
    

}
