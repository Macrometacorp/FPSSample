using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Security;
using UnityEngine;
using UnityEngine.Events;

public class MergeLogs : MonoBehaviour {

    public int debugCount;
    
    public UnityEvent<string> AddLine;//not working in 2019 inspector
    public PairMessageCheck pairMessageCheck;// for 2019
    
    public UnityEvent MergeComplete;
    void Start() {
        var pathRead = Application.dataPath;
#if UNITY_EDITOR
        pathRead = Path.Combine(Application.dataPath, "../Logs/Merge");
#endif
        var pathWrite = Path.Combine(pathRead, "../Merged");
        string name = "Merged";
        name += UnixTSNowFileSafe() + ".log";

        var logfiles = new DirectoryInfo(pathRead).GetFiles();
        List<LogData> logReaders = new List<LogData>();
        foreach (var logFile in logfiles) {
            if (!logFile.Name.EndsWith(".log")) {
                continue;
            }
            var sr = new StreamReader(Path.Combine(pathRead,logFile.Name));
            var line= sr.ReadLine();
            //Debug.Log("MergeLogs: "+ pathRead + " :: "+ logFile.Name + "  :: " + line);
            logReaders.Add(new LogData() {
                sr = sr,
                line = line
            });
            //Debug.Log("first** " + logFile.Name +" :: "+ logReaders[logReaders.Count-1].line);
        }

        int i = 0;
        Debug.Log("Output MergeLogs: " + (Path.Combine(pathWrite, name)));
        using (StreamWriter sw = new StreamWriter(Path.Combine(pathWrite, name), true)) {
            while (true) {
                i++;
                var removeList = logReaders.FindAll(x => x.line == null);
                foreach (var logData in removeList) {
                    logReaders.Remove(logData);
                    logData.sr.Dispose();
                }

                if (logReaders.Count == 0) break;
                var lr = logReaders[0];
                var line = lr.line;
                foreach (var logReader in logReaders) {
                    if (String.Compare(line, logReader.line) >= 0) {
                        lr = logReader;
                        line = lr.line;
                    }
                }

                //AddLine.Invoke(lr.line);
                pairMessageCheck.AddLine(lr.line);
                sw.WriteLine( lr.line);
                lr.line = lr.sr.ReadLine();
            }
        }
        Debug.Log("Output MergeLogs Finished: " );
        MergeComplete.Invoke();
    }
    
    public static string UnixTSNowFileSafe() {
        return DateTime.Now.ToUniversalTime()
            .ToString("yyyy'-'MM'-'dd'T'HH'_'mm'_'ss'_'fff'Z'");
    }

    class LogData {
        public StreamReader sr;
        public string line;
    }
}

