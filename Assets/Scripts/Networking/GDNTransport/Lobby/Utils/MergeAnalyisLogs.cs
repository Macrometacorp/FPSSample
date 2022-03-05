using System;
using System.IO;
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

public static class MergeAnalysisLogs {
    static System.IO.StreamWriter logFile = null;
    static public bool forwardToDebug = false;
    
    public const string SEPRATOR = "|";
    public const string INFO = "[I]";
    public const string WARN = "[W]";
    public const string ERROR = "[E]";
        
    public static void Init( string logBaseName) {
        var logfilePath = Application.dataPath;
#if UNITY_EDITOR
        logfilePath = Path.Combine(Application.dataPath, "../Logs/Merge");
#endif
        var pathWrite = Path.Combine(logfilePath, "../Merged");

        logBaseName += UnixTSNowFileSafe()+ ".log";
        
       
        logFile = System.IO.File.CreateText(pathWrite + "/" + logBaseName);
        logFile.AutoFlush = true;
       
    }

    public static void Shutdown()
    {
        if (logFile != null)
            logFile.Close();
        logFile = null;
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
        return "" ;
    }
    
   
    static public void Log(string message) {
        //Console.Write(Prefix() + INFO + SEPRATOR  + message);
        if (logFile != null)
            logFile.WriteLine(Prefix() + INFO + SEPRATOR + message);
    }
    
    public static void LogError(string message) {
        Console.Write(Prefix() + ERROR + SEPRATOR  + message);
        if (logFile != null)
            logFile.WriteLine(Prefix() + ERROR + SEPRATOR  + message );
    }
    
    public static void LogWarning(string message) {
        Console.Write(Prefix() + WARN + SEPRATOR  + message);
        if (logFile != null)
            logFile.WriteLine(Prefix() + WARN + SEPRATOR  + message);
    }

}
