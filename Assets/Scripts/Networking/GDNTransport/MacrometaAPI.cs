using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Net;

using BestHTTP.WebSocket;
using Object = System.Object;

namespace Macrometa {

    ///documents
///_fabric/{fabric}/_api/document/{collection} Removes multiple documents delete
///_fabric/{fabric}/_api/document/{collection} Update documents  patch


/// /_fabric/{fabric}/_api/document/{collection}/{key} remove single document delete

/// /_fabric/{fabric}/_api/document/{collection}/{key} can be used for checking doc _rev Head
///_fabric/{fabric}/_api/document/{collection}/{key} Removes single document delete
///_fabric/{fabric}/_api/document/{collection}/{key} Update single document  patch

/// indexes

/// /_fabric/{fabric}/_api/index/persistent create persistent index
/*
 A JSON object with these properties is required:

fields (string): an array of attribute paths.
unique: if true, then create a unique index.
type: must be equal to "persistent".
sparse: if true, then create a sparse index.
 */
///_fabric/{fabric}/_api/index/ttl
/*
 A JSON object with these properties is required:

fields (string): an array with exactly one attribute path.
type: must be equal to "ttl".
expireAfter: The time (in seconds) after a document's creation after which the documents count as "expired".
 */

/// Collections
//    /_fabric/{fabric}/_api/collection  Post create
/*
 A JSON object with these properties is required:

keyOptions:
allowUserKeys: if set to true, then it is allowed to supply own key values in the _key attribute of a document. If set to false, then the key generator will solely be responsible for generating keys and supplying own key values in the _key attribute of documents is considered an error.
type: specifies the type of the key generator. The currently available generators are traditional, autoincrement, uuid and padded. The traditional key generator generates numerical keys in ascending order. The autoincrement key generator generates numerical keys in ascending order, the initial offset and the spacing can be configured The padded key generator generates keys of a fixed length (16 bytes) in ascending lexicographical sort order. This is ideal for usage with the RocksDB engine, which will slightly benefit keys that are inserted in lexicographically ascending order. The key generator can be used in a single-server or cluster. The uuid key generator generates universally unique 128 bit keys, which are stored in hexadecimal human-readable format. This key generator can be used in a single-server or cluster to generate "seemingly random" keys. The keys produced by this key generator are not lexicographically sorted.
increment: increment value for autoincrement key generator. Not used for other key generator types.
offset: Initial offset value for autoincrement key generator. Not used for other key generator types.
name: The name of the collection.
isSystem: If true, create a system collection. In this case collection-name should start with an underscore. End users should normally create non-system collections only. API implementors may be required to create system collections in very special occasions, but normally a regular collection will do. (The default is false)
type: The type of the collection to be created.
The following values for type are valid (The default is 2):
2: document collection
3: edge collection.
stream: If true, create a local stream for collection. (The default is false)
 */

    /// Query
    /// /_fabric/{fabric}/_api/cursor create cursor see web page
    /// /_fabric/{fabric}/_api/cursor/{cursor-identifier}  get next cursor

    [Serializable]
    public struct GDNData {
        public string apiKey;
        public string federationURL;
        public string tenant;
        public string fabric;
        public bool isGlobal;

        public string region => isGlobal ? "c8global" : "c8local";

        public string requestURL => "api-" + federationURL;

        #region Collection
        public string GetCollectionsURL() {
            return "https://" + requestURL + "/_fabric/"+fabric+ "/_api/collection?excludeSystem=true";
        }
        
        public string PostCreateCollectionsURL() {
            return "https://" + requestURL + "/_fabric/"+fabric+ "/_api/collection";
        }
        #endregion Collection
        
        #region Indexes
        public string GetIndexesURL(string collection) {
            return "https://" + requestURL + "/_fabric/"+fabric+ "/_api/index?collection="+collection;
        }
        
        public string PostPersistentIndexURL() {
            return "https://" + requestURL + "/_fabric/"+fabric+ "/_api/index/persistent";
        }
        
        public string PostTTLIndexURL() {
            return "https://" + requestURL + "/_fabric/"+fabric+ "/_api/index/ttl";
        }
        #endregion Indexes
        
        #region Document URLS

        //   409 key violation
        public string PostInsertDocumentURL(string collection) {
            return "https://" + requestURL + "/_fabric/" + fabric + "/_api/document/" + collection;
        }
        
        public string GetDocumentURL(string collection, string key) {
            return "https://" + requestURL + "/_fabric/" + fabric + "/_api/document/" + collection +"/"+ key;
        }
        public string PutReplaceDocumentURL(string collection, string key) {
            return "https://" + requestURL + "/_fabric/" + fabric + "/_api/document/" + collection +"/"+ key;
        }
        
        #endregion Document URLS
    
        #region Stream URLS
        public string ClearAllBacklogs() {
            return "https://" + requestURL + "/_fabric/"+fabric+ "/_api/streams/clearbacklog";
        }
        
        public string CreateStreamURL(string streamName) {
            return "https://" + requestURL + "/_fabric/"+fabric+ "/_api/streams/" + streamName + "?global=" + isGlobal;
        }

        public string ListStreamsURL() {
            return "https://" + requestURL + "/_fabric/"+fabric+ "/_api/streams?global=" + isGlobal;
        }
        
        public string GetTTLURL() {
            return "https://" + requestURL + "/_fabric/"+fabric+ "/_api/streams/ttl";
        }
        
        public string SetTTLURL(int ttl) {
            return "https://" + requestURL + "/_fabric/"+fabric+ "/_api/streams/ttl/"+ttl;
        }
        
        //const consumerUrl = `wss://${requestURL}/_ws/ws/v2/consumer/persistent/${tenant}/${region}._system/${region}s.${STREAM_NAME}/${CONSUMER_NAME}`;
        public string ConsumerURL(string streamName, string consumerName ) {
            return "wss://" + requestURL + "/_ws/ws/v2/consumer/persistent/" + tenant + "/" + region+"."+
                fabric+ "/" + region+"s."+streamName+"/"+consumerName ;
        }
        
        //const producerUrl = `wss://${requestURL}/_ws/ws/v2/producer/persistent/${tenant}/${region}._system/${region}s.${STREAM_NAME}`;
        public string ProducerURL(string streamName) {
            return "wss://" + requestURL + "/_ws/ws/v2/producer/persistent/" + tenant + "/" + region+"."+
                   fabric+ "/" + region+"s."+streamName ;
        }
        
        #endregion stream URLS
        
        #region Autorization
        public string GetRegionURL() {
            return "https://" + requestURL + "/datacenter/local";
        }
        
        public string GetOTPURL() {
            return "https://" + requestURL + "/apid/otp";
        }
        
        public void Authorize(UnityWebRequest www) {
            www.SetRequestHeader("Authorization", "apikey " + apiKey);
        }

        #endregion Autorization

        #region KV URLS
        public string CreateKVURL(string name, bool isExpire) {
            return "https://" + requestURL + "/_fabric/" + fabric + "/_api/kv/" + name + "?expiration=" + isExpire;
        }
        
        
        public string ListKVCollectionsURL() {
            return "https://" + requestURL + "/_fabric/"+fabric+ "/kv";
        }
        
      
        public string GetKVValuesURL(string name, int offset = 0, int limit = 100) {
            return "https://" + requestURL + "/_fabric/" + fabric + "/_api/kv/"
                   + name + "/values?offset=" + offset + "&limit="+ limit ;
        }
        
        public string PutKVValueURL(string name) {
            return "https://" + requestURL + "/_fabric/" + fabric + "/kv/"
                   + name + "/value";
        }
        #endregion KV URLS
        
        public string StreamListName(string streamName) {
            return region + "s." + streamName;
        }
    }

    [Serializable]
    public struct OTPResult {
        public string otp;
    }

    [Serializable]
    public struct ListStream{
        public bool error;
        public int code;
        public List<Stream> result;
    }
    
    // a single stream are returned by ListStreamURL
    [Serializable]
    public struct Stream {
        public string topic; //moved topic to first field for better default inspector
        public string _key;
        public string _id;
        public string _rev;
        public string db;
        public bool local;
        public string tenant;
        public int type;
    }

    [Serializable]
    public struct ListCollection{
        public bool error;
        public int code;
        public List<Collection> result;
    }

    
    [Serializable]
    public struct Collection {
        public string id;
        public string name;
        public int status;
        public int type;
        public string collectionModel;
        public bool isSpot;
        public bool isLocal;
        public bool hasStream;
        public bool isSystem;
        public string globallyUniqueId;
        public bool searchEnabled;
    }

    [Serializable]
    public struct IndexParams {
    public List<string> fields;
    public bool sparse;
    public string type;
    public bool unique;
    }

    
    [Serializable]
    public struct ListIndexes{
        public bool error;
        public int code;
        public List<Index> result;
    }

    
    [Serializable]
    public struct Index {
        public string id;
        public string name;
        public string[] fields;
        public string type;
        public bool sparse;
        public bool unique;
    }
    
    [Serializable]
    public struct Region {
        public string _key;
        public string host;
        public bool local;
        public string name;
        public int port;
        public bool spot_region;
        public int status;
        // tags not implemnented locally 
        public LocationInfo locationInfo;
    }

    [Serializable]
    public struct LocationInfo {
        public string city;
        public string countrycode;
        public string countryname;
        public float latitude;
        public float longitude;
        public string url;
    }
    /*
     {"_key":"gdn-ap-northeast","host":"139.162.118.31","local":true,"name":"gdn-ap-northeast","port":30003,"spot_region":false,"status":0,"tags":{"api":"api-gdn-ap-northeast.paas.macrometa.io","role":"c8streams-agent","url":"gdn-ap-northeast.paas.macrometa.io"},
     "locationInfo":{"city":"Tokyo","countrycode":"JP","countryname":"Japan","latitude":"35.6865","longitude":"139.7458","url":"gdn-ap-northeast.paas.macrometa.io"}}
     */
    
    [Serializable]
    public struct ListKVCollection{
        public bool error;
        public int code;
        public List<KVCollection> result;
    }

    [Serializable]
    public struct KVCollection {
        public string name;
        public bool expiration;
    }
    
    [Serializable]
    public struct ListKVValue{
        public bool error;
        public int code;
        public List<KVValue> result;
    }

    [Serializable]
    public class KVValue {
        public string _key;
        public string value;
        public int expireAt; // unix timestamp
    }
    
    [Serializable]
    public struct BaseHtttpReply {
        public bool error;
        public int code;
        public Object result;
    }
    
    [Serializable]
    public enum VirtualMsgType {
        Connect,
        Data,
        Disconnect,
        Ping,
        Pong,
        Internal, //used to send stats or other internal driver information
        Dummy     //used to simulate traffic
    }
    
    [Serializable]
    public struct SendMessage {
        public MessageProperties properties;
        public string payload;
    }
    
    [Serializable]
    public struct StatsSendMessage {
        public StatsMessageProperties properties;
        public string payload;
    }
    
    [Serializable]
    public struct ReceivedMessage {
        public string messageId;
        public string payload; 
        public MessageProperties properties; 
        public string publishTime;
        public int redeliveryCount;
    }

    //to reduce json size these fields are 1 character
    [Serializable]
    public class MessageProperties {
        public string s; //source
        public string d;//desitination
        public int p;//port  used for connection ID
        public VirtualMsgType t;//msgType
        public int i;//pingId
        public int z;//payloadByteSize
        // these are only used in TranportPing/Pong
        public int r;// last ping time consumer i.e. remote ping time
        public int o;// last ping time producer i.e. remote ping time
        public string localId;// node i.e. remote ping time
        public string host; // datacenter host from region
        public string city;
        public string countrycode;
        //public LocationInfo locationInfo; // data center info from region

    }
    
    [Serializable]
    public class StatsMessageProperties {
        public string type = "NetworkStats";
        public string version = "0.1";
        public string app = "NetworkTester";
        public PingStatsGroup.NetworkStatsData NetworkStatsData;
    }
    
    [Serializable]
    public struct AckMessage {
        public string messageId;
    }
    
    public class MacrometaAPI {
        public static UnityWebRequest WebPost(string url, string data,GDNData gdnData) {
            UnityWebRequest www = new UnityWebRequest(url, "POST");
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(data);
            www.uploadHandler = (UploadHandler) new UploadHandlerRaw(jsonToSend);
            www.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Authorization", "apikey " + gdnData.apiKey);
            return www;
        }
        public static string Base64Encode(string plainText) {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        
        public static string Base64Decode(string base64EncodedData) {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
        public static IEnumerator CreateStream(GDNData gdnData, string streamName,
            Action<UnityWebRequest> callback) {
            List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
            GameDebug.Log("stream url: "+ gdnData.CreateStreamURL(streamName));
            UnityWebRequest www = UnityWebRequest.Post(gdnData.CreateStreamURL(streamName), formData);
            www.SetRequestHeader("Authorization", "apikey " + gdnData.apiKey);
            yield return www.SendWebRequest();
            if (callback != null)
                callback(www);
        }

        public static IEnumerator ClearAllBacklogs(GDNData gdnData, Action<UnityWebRequest> callback) {
            List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
            UnityWebRequest www = UnityWebRequest.Post(gdnData.ClearAllBacklogs(), formData);
            www.SetRequestHeader("Authorization", "apikey " + gdnData.apiKey);
            yield return www.SendWebRequest();
            if (callback != null)
                callback(www);
        }

        public static IEnumerator GetRegion(GDNData gdnData, Action<UnityWebRequest> callback) {
            UnityWebRequest www = UnityWebRequest.Get(gdnData.GetRegionURL());
            www.SetRequestHeader("Authorization", "apikey " + gdnData.apiKey);
            yield return www.SendWebRequest();
            if (callback != null)
                callback(www);
        }

        public static IEnumerator ListStreams(GDNData gdnData, Action<UnityWebRequest> callback) {
            UnityWebRequest www = UnityWebRequest.Get(gdnData.ListStreamsURL());
            www.SetRequestHeader("Authorization", "apikey " + gdnData.apiKey);
            yield return www.SendWebRequest();
            if (callback != null)
                callback(www);
        }

        // Testing Callback to act on our response data
        public static void UnityWebRequestCallback(UnityWebRequest www) {
            if (www.isHttpError || www.isNetworkError) {
                Debug.Log(www.error);
            }
            else {
                Debug.Log("text : " + www.downloadHandler.text + "\n");
                Debug.Log("size: " + www.downloadedBytes);
            }
        }

        // /_fabric/{fabric}/_api/streams/ttl
        public static IEnumerator GetTTL(GDNData gdnData, Action<UnityWebRequest> callback) {
            UnityWebRequest www = UnityWebRequest.Get(gdnData.GetTTLURL());
            www.SetRequestHeader("Authorization", "apikey " + gdnData.apiKey);
            yield return www.SendWebRequest();
            if (callback != null)
                callback(www);
        }

        // /_fabric/{fabric}/_api/streams/ttl/{ttl}
        public static IEnumerator SetTTL(GDNData gdnData, int ttl, Action<UnityWebRequest> callback) {
            List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
            UnityWebRequest www = UnityWebRequest.Post(gdnData.SetTTLURL(ttl), formData);
            Debug.Log("SetTTLURL: "+gdnData.SetTTLURL(ttl));
            www.SetRequestHeader("Authorization", "apikey " + gdnData.apiKey);
            yield return www.SendWebRequest();
                
            if (callback != null)
                callback(www);
        }
        
        public static IEnumerator Consumer(GDNData gdnData, string streamName, string consumerName,
            Action<WebSocket, string> callback) {
            List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
            OTPResult otp = new OTPResult();
            using (UnityWebRequest www = UnityWebRequest.Post(gdnData.GetOTPURL(), formData)) {
                www.SetRequestHeader("Authorization", "apikey " + gdnData.apiKey);
                yield return www.SendWebRequest();
                
                otp = JsonUtility.FromJson<OTPResult>(www.downloadHandler.text);
            }

            callback(new WebSocket(new Uri(gdnData.ConsumerURL(streamName, consumerName) + "?otp=" + otp.otp)),
                "LIVE consumer "+ streamName);
            //callback(new WebSocket(gdnData.ConsumerURLDebug(streamName, consumerName) ), "LIVE");
        }
        
        public static IEnumerator Producer(GDNData gdnData, string streamName, Action<WebSocket, string> callback) {
            List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
            OTPResult otp = new OTPResult();
            using (UnityWebRequest www = UnityWebRequest.Post(gdnData.GetOTPURL(), formData)) {
                www.SetRequestHeader("Authorization", "apikey " + gdnData.apiKey);
                yield return www.SendWebRequest();
                otp = JsonUtility.FromJson<OTPResult>(www.downloadHandler.text);
            }
            callback(new WebSocket(new Uri(gdnData.ProducerURL(streamName) + "?otp=" + otp.otp)),
                "LIVE producer "+ streamName);
        }
        
        public static IEnumerator ProducerMulti(GDNData gdnData, string streamName, int id, Action<WebSocket, string, int> callback) {
            List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
            OTPResult otp = new OTPResult();
            using (UnityWebRequest www = UnityWebRequest.Post(gdnData.GetOTPURL(), formData)) {
                www.SetRequestHeader("Authorization", "apikey " + gdnData.apiKey);
                yield return www.SendWebRequest();
                otp = JsonUtility.FromJson<OTPResult>(www.downloadHandler.text);
            }
            callback(new WebSocket(new Uri(gdnData.ProducerURL(streamName) + "?otp=" + otp.otp)),
                "LIVE producer "+ streamName, id);
        }

        #region KVCollection

        public static IEnumerator ListKVCollections(GDNData gdnData, Action<UnityWebRequest> callback) {
            
            UnityWebRequest www = UnityWebRequest.Get(gdnData.ListKVCollectionsURL());
            www.SetRequestHeader("Authorization", "apikey " + gdnData.apiKey);
            yield return www.SendWebRequest();
            if (callback != null)
                callback(www);
        }

      
        public static IEnumerator CreateKVCollection(GDNData gdnData, string collectionName,
            Action<UnityWebRequest> callback) {
            List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
            GameDebug.Log("KV url: "+ gdnData.CreateStreamURL(collectionName));
            UnityWebRequest www = UnityWebRequest.Post(gdnData.CreateKVURL(collectionName,true), formData);
            www.SetRequestHeader("Authorization", "apikey " + gdnData.apiKey);
            yield return www.SendWebRequest();
            
            if (callback != null)
                callback(www);
        }
        
        public static IEnumerator GetKVValues(GDNData gdnData, string collectionName,
            Action<UnityWebRequest> callback) {
            List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
            var url = gdnData.GetKVValuesURL(collectionName);
            //GameDebug.Log("KV url: "+ url);
            UnityWebRequest www = UnityWebRequest.Post(url, formData);
            www.SetRequestHeader("Authorization", "apikey " + gdnData.apiKey);
            yield return www.SendWebRequest();
            
            if (callback != null)
                callback(www);
        }
        public static IEnumerator PutKVValue(GDNData gdnData, string collectionName, string kvRecord,
            Action<UnityWebRequest> callback) {
            byte[] putData = System.Text.Encoding.UTF8.GetBytes(kvRecord);
            var url = gdnData.PutKVValueURL(collectionName);
           // GameDebug.Log("put URL: " + url);
            UnityWebRequest www = UnityWebRequest.Put(url, putData);
            www.SetRequestHeader("Authorization", "apikey " + gdnData.apiKey);
            yield return www.SendWebRequest();
            
            if (callback != null)
                callback(www);
        }
        #endregion
        #region DocumentCollection

        public static IEnumerator ListDocumentCollections(GDNData gdnData, Action<UnityWebRequest> callback) {
            
            UnityWebRequest www = UnityWebRequest.Get(gdnData.GetCollectionsURL());
            www.SetRequestHeader("Authorization", "apikey " + gdnData.apiKey);
            yield return www.SendWebRequest();
            if (callback != null)
                callback(www);
        }

      
        public static IEnumerator ListIndexes(GDNData gdnData, string collection, Action<UnityWebRequest> callback) {

            UnityWebRequest www = UnityWebRequest.Get(gdnData.GetIndexesURL(collection));
            www.SetRequestHeader("Authorization", "apikey " + gdnData.apiKey);
            yield return www.SendWebRequest();
            if (callback != null)
                callback(www);
        }

        public static IEnumerator CreateCollection(GDNData gdnData, string collectionName,
            Action<UnityWebRequest> callback) {
           // var data= "{ \"name\": \" "+collectionName+"\" }";
            var data= "{ \"name\": \""+collectionName +"\",\"stream\": true }";
            var www = WebPost(gdnData.PostCreateCollectionsURL(), data, gdnData);
            yield return www.SendWebRequest();
            
            if (callback != null)
                callback(www);
        }
        /*
        public static IEnumerator GetKVValues(GDNData gdnData, string collectionName,
            Action<UnityWebRequest> callback) {
            List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
            var url = gdnData.GetKVValuesURL(collectionName);
            //GameDebug.Log("KV url: "+ url);
            UnityWebRequest www = UnityWebRequest.Post(url, formData);
            www.SetRequestHeader("Authorization", "apikey " + gdnData.apiKey);
            yield return www.SendWebRequest();
            
            if (callback != null)
                callback(www);
        }
        public static IEnumerator PutKVValue(GDNData gdnData, string collectionName, string kvRecord,
            Action<UnityWebRequest> callback) {
            byte[] putData = System.Text.Encoding.UTF8.GetBytes(kvRecord);
            var url = gdnData.PutKVValueURL(collectionName);
           // GameDebug.Log("put URL: " + url);
            UnityWebRequest www = UnityWebRequest.Put(url, putData);
            www.SetRequestHeader("Authorization", "apikey " + gdnData.apiKey);
            yield return www.SendWebRequest();
            
            if (callback != null)
                callback(www);
        }
        
       */
        
        
        #endregion
    }


}


