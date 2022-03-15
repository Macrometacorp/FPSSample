using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class PairMessageCheck : MonoBehaviour {
    
    public int count = 0;
    
    public Dictionary<string, PMRecord> PMRecords = new Dictionary<string, PMRecord>();
    
    public struct PMRecord {
        public string id;
        public string source;
        public string destination;
        public int msgId;
        public string publishTime;
        public string sent;
        public string received;
        public List<string> skipped ;
        public int delay;
        public int size;
        public bool matched;

        public override string ToString() {

            string extra = "";
            foreach (var time in skipped) {
                extra +=  time + " |";
            }
            return  source + " | " + destination + " | " +  msgId + " | " + publishTime + " | " +
                    sent + " | " + received + " | " + delay  + " | " + size + " | " + matched + " | " + 
                    skipped.Count + " |" + extra ;
        }
    }

    public string[] stringFields = new[] {"Source:", "Dest:"};
    public string[] intFields = new[] { "MsgId:"};
    public string destField = "Dest:";
    public string sourceField = "Source:";
    public string msgIdField = "MsgId:";
    public string addressField = "Address:";
    public string messageIdField = "messageId@";
    public string publishTimeField  ="publishTime@";
    
    public void AddLine(string line) {
        
        string[] fields = line.Split('|');

        bool isPairMessage = false;
        foreach (var field in fields) {
            if (field.Contains("MsgType: PairMessage") && !field.Contains("msgType: Ping" )) {
                isPairMessage = true;
                break;
            }
        }
        if (!isPairMessage) return;
        foreach (var field in fields) {
            if (field.Contains("msgType: Ping" ) ||  field.Contains("msgType: Pong" ) ) {
                isPairMessage =false;
                break;
            }
        }
        
        if (!isPairMessage) return;
        count++;
        string pmKey = "";
        bool send = false;
        string address = "";
        string dest = "";
        string source = "";
        int msgId =0;
        int size = 0;
        string messageId="";
        string publishTime="";
        
        foreach (var field in fields) {
            if (stringFields.Any(field.Contains)) {
                pmKey += field;
            }
            if (intFields.Any(field.Contains)) {
                //pmKey += field;
                string[] subFields = field.Split(':');
                //Debug.Log(subFields[1] +" :: "+ int.Parse(subFields[1]) + " :: " +$"{int.Parse(subFields[1]):00000}");
                pmKey +=subFields[0]+": "+  $"{int.Parse(subFields[1]):00000} ";
            }
            
            if (field.Contains("Size:")) {
                string[] subFields = field.Split(':');
                //Debug.Log(subFields[1] +" :: "+ int.Parse(subFields[1]) + " :: " +$"{int.Parse(subFields[1]):00000}");
                size =int.Parse(subFields[1]);
            } 
            if (field.Contains(msgIdField)) {
                string[] subFields = field.Split(':');
                //Debug.Log(subFields[1] +" :: "+ int.Parse(subFields[1]) + " :: " +$"{int.Parse(subFields[1]):00000}");
                msgId =int.Parse(subFields[1]);
            }
            
            if (field.Contains("Send: True")) {
                send = true;
            }
            if (field.Contains(addressField)) {
                string[] subFields = field.Split(':');
                address = subFields[1].Trim();
            }
            if (field.Contains(destField)) {
                string[] subFields = field.Split(':');
                dest = subFields[1].Trim();
            } if (field.Contains(sourceField)) {
                string[] subFields = field.Split(':');
                source = subFields[1].Trim();
            }
            if (field.Contains(messageIdField)) {
                string[] subFields = field.Split('@');
                messageId = subFields[1].Trim();;
            }if (field.Contains(publishTimeField)) {
                string[] subFields = field.Split('@');
                publishTime = subFields[1].Trim();;
            }
        }

        var pmRecord = new PMRecord();
        pmRecord.skipped = new List<string>();
        if (PMRecords.ContainsKey(pmKey)) {
            pmRecord = PMRecords[pmKey];
        }

        pmRecord.source = source;
        pmRecord.destination = dest;
        pmRecord.msgId = msgId;
        pmRecord.id = pmKey;
        if (send) {
            pmRecord.sent = fields[0];
        }
        if (size != 0) {
            pmRecord.size = size;
        }
        else if(address == dest) {
            pmRecord.received = fields[0];
        }
        else {
            pmRecord.skipped.Add(fields[0]);
        }

        if (!String.IsNullOrEmpty(pmRecord.sent) && !String.IsNullOrEmpty(pmRecord.received)) {
            pmRecord.matched = true;
            CultureInfo provider = CultureInfo.InvariantCulture;
            var  sentDT=  DateTime.ParseExact(pmRecord.sent, "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'", provider);
            var  recvDT=  DateTime.ParseExact(pmRecord.received, "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'", provider);
            var delay = recvDT - sentDT;
            pmRecord.delay = (int)delay.TotalMilliseconds;
        }

        if (!String.IsNullOrEmpty(messageId)) {
           
        }

        if (!String.IsNullOrEmpty(publishTime)) {
            pmRecord.publishTime = publishTime;
        }
        
        PMRecords[pmKey] = pmRecord;
    }

    public void MergeComplete() {
        Debug.Log("PairMessageCheck Count: "+PMRecords.Count);
        MergeAnalysisLogs.Init("PairMessageCheck");
        var records = PMRecords.Values.ToList().OrderBy(s=> s.id);
        foreach (var record in records) {
            MergeAnalysisLogs.Log(record.ToString());
        }
        MergeAnalysisLogs.Shutdown();
    }
    
    
}
