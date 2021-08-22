using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using static Macrometa.MacrometaAPI;

namespace Macrometa.Lobby {
    public class LobbyRecord {
        public string _key; //base string name
        public string value;
        public long expireAt; //unix timestamp

        static public long UnixTSNow(long offset) {
            return (long) (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds + offset;
        }

        public enum Status {
            init,
            waiting,
            playing
        }

        public static LobbyRecord GetFromLobbyValue(LobbyValue lobbyValue, long ttl) {
            return new LobbyRecord() {
                _key = lobbyValue.streamName,
                value = JsonUtility.ToJson(lobbyValue),
                expireAt = UnixTSNow(ttl)
            };
        }

        public static LobbyRecord GetInit(string clientID, string baseStreamName, long ttl) {
            var val = new LobbyValue() {
                clientId = clientID,
                streamName = baseStreamName,
                status = LobbyRecord.Status.init.ToString()
            };
            return GetFromLobbyValue(val, ttl);
        }

        public static LobbyRecord GetRecordTest(string baseStreamName, long ttl) {
            var val = new LobbyValue() {
                clientId = "",
                status = LobbyRecord.Status.waiting.ToString()
            };

            return new LobbyRecord() {
                _key = baseStreamName,
                value = JsonUtility.ToJson(val),
                expireAt = UnixTSNow(ttl)
            };
        }
    }

    [Serializable]
    public class Team {
        public string name;
        public List<TeamSlot> slots = new List<TeamSlot>();
    }

    [Serializable]
    public class TeamSlot {
        // public bool empty; is this needed??
        public string playerName;
        public Region region;
        public int ping;
        public int rtt;
        public bool runGameServer;
    }

    [Serializable]
    public class LobbyValue {

        public string clientId; // used for keeping record unique for KV collections are not ACID 
        public string gameMode;
        public string mapName;
        public int maxPlayers; // always 8?
        public string status; //init, waiting ( to start), playing
        public float ping; // only used locally not use in kv db
        public string streamName; // only used locally is also _key
        public string adminName;
        public Region region;
        public Team unassigned;
        public Team team1;
        public Team team2;
        public bool frozen;
        public bool serverClientChosen;



        public static LobbyValue FromKVValue(KVValue kvValue) {
            LobbyValue result = JsonUtility.FromJson<LobbyValue>(kvValue.value);
            result.streamName = kvValue._key;
            return result;
        }

        public static void UpdateFrom(List<LobbyValue> currRecords, List<LobbyValue> newRecords) {

            foreach (var lobbyValue in newRecords) {
                //Debug.Log( "lobbyValue.streamName:  " + lobbyValue.streamName);
                var oldRecord = currRecords.FirstOrDefault(x => x.streamName == lobbyValue.streamName);
                if (oldRecord != null) {
                    lobbyValue.ping = oldRecord.ping;
                }
            }

            newRecords.RemoveAll(x => x.ping < 0);
            currRecords.Clear();

            currRecords.AddRange(newRecords);
        }

    }
    
    [Serializable]
    public class LobbyList {
        public List<LobbyValue> lobbies= new List<LobbyValue>();
        public bool isDirty = false;
        
        /// <summary>
        /// cIs this needed for lobbies
        /// </summary>
        /// <returns></returns>
        public LobbyValue UnpingedLobby() {
            return lobbies.FirstOrDefault(lobbyValue => lobbyValue.ping == 0 && lobbyValue.status == "Active");
        }
    }
    
    
    /// <summary>
    /// Key value methods
    /// client browser mode check Lobby list 
    /// </summary>
    public class GdnKvLobbyDriver {
        private MonoBehaviour _monoBehaviour;
        private GDNData _gdnData;
        private GDNErrorhandler _gdnErrorHandler;
        public ListKVCollection listKVCollection;
        public ListKVValue listKVValues ;
        public bool kvCollectionListDone = false;
        public bool lobbiesKVCollectionExists;
        public string lobbiesKVCollectionName = "FPSGames_Lobbies";
        public bool kvValueListDone;
        public bool putKVValueDone;
        public LobbyList LobbyList = new LobbyList();
        
        /// <summary>
        /// passing in a monobehaviour to be able use StartCoroutine
        /// happens because of automatic refactoring
        /// probably can hand cleaned
        /// </summary>
        /// <param name="gdnData"></param>
        /// <param name="gdnErrorhandler"></param>
        /// <param name="monoBehaviour"></param>
        public GdnKvLobbyDriver(GDNNetworkDriver gdnNetworkDriver) {

            _gdnData = gdnNetworkDriver.baseGDNData;
            _monoBehaviour = gdnNetworkDriver;
            _gdnErrorHandler = gdnNetworkDriver.gdnErrorHandler;
        }

        public void CreateLobbiesKVCollection() {

            lobbiesKVCollectionExists = listKVCollection.result.Any
                (item => item.name == lobbiesKVCollectionName);
            if (!lobbiesKVCollectionExists) {
                _gdnErrorHandler.isWaiting = true;
                ;
                //Debug.Log("creating server in stream: " + baseGDNData.CreateStreamURL(serverInStreamName));
                _monoBehaviour.StartCoroutine(CreateKVCollection(_gdnData, lobbiesKVCollectionName,
                    CreateKVCollectionCallback));
            }
        }

        public void CreateKVCollectionCallback(UnityWebRequest www) {
            _gdnErrorHandler.isWaiting = false;
            if (www.isHttpError || www.isNetworkError) {
                GameDebug.Log("CreateServerInStream : " + www.error);
                _gdnErrorHandler.currentNetworkErrors++;
                kvCollectionListDone = false;
            }
            else {
                var baseHttpReply = JsonUtility.FromJson<BaseHtttpReply>(www.downloadHandler.text);
                if (baseHttpReply.error == true) {
                    GameDebug.Log("create KV Collection failed:" + baseHttpReply.code);
                    _gdnErrorHandler.currentNetworkErrors++;
                    kvCollectionListDone = false;
                }
                else {
                    GameDebug.Log("Create KV Collection  ");
                    lobbiesKVCollectionExists = true;
                    _gdnErrorHandler.currentNetworkErrors = 0;
                }
            }
        }

        public void GetListKVColecions() {
            _gdnErrorHandler.isWaiting = true;
            _monoBehaviour.StartCoroutine(ListKVCollections(_gdnData, ListKVCollectionsCallback));
        }

        public void ListKVCollectionsCallback(UnityWebRequest www) {
            _gdnErrorHandler.isWaiting = false;
            if (www.isHttpError || www.isNetworkError) {
                _gdnErrorHandler.currentNetworkErrors++;
                GameDebug.Log("List KV Collections: " + www.error);
            }
            else {

                //overwrite does not assign toplevel fields
                //JsonUtility.FromJsonOverwrite(www.downloadHandler.text, listStream);
                listKVCollection = JsonUtility.FromJson<ListKVCollection>(www.downloadHandler.text);
                if (listKVCollection.error == true) {
                    GameDebug.Log("List KV Collection failed:" + listKVCollection.code);
                    //Debug.LogWarning("ListStream failed reply:" + www.downloadHandler.text);
                    _gdnErrorHandler.currentNetworkErrors++;
                }
                else {
                    kvCollectionListDone = true;
                    _gdnErrorHandler.currentNetworkErrors = 0;
                }
            }
        }

        public void GetListKVValues() {
            _gdnErrorHandler.isWaiting = true;
            _monoBehaviour.StartCoroutine(GetKVValues(_gdnData, lobbiesKVCollectionName,
                ListKVValuesCallback));
        }

        public void ListKVValuesCallback(UnityWebRequest www) {
            _gdnErrorHandler.isWaiting = false;
            if (www.isHttpError || www.isNetworkError) {
                _gdnErrorHandler.currentNetworkErrors++;
                GameDebug.Log("List KVvalues: " + www.error);
            }
            else {

                //overwrite does not assign toplevel fields
                //JsonUtility.FromJsonOverwrite(www.downloadHandler.text, listStream);
                listKVValues = JsonUtility.FromJson<ListKVValue>(www.downloadHandler.text);
                if (listKVValues.error == true) {
                    GameDebug.Log("List KV values failed:" + listKVValues.code);
                    //Debug.LogWarning("ListStream failed reply:" + www.downloadHandler.text);
                    _gdnErrorHandler.currentNetworkErrors++;
                }
                else {
                    
                    kvValueListDone = true;
                    _gdnErrorHandler.currentNetworkErrors = 0;
                    var newLobbyList = new List<LobbyValue>();
                    foreach (KVValue kvv in listKVValues.result) {
                        newLobbyList.Add(LobbyValue.FromKVValue(kvv));
                    }
                    LobbyValue.UpdateFrom(LobbyList.lobbies,newLobbyList);
                    LobbyList.isDirty = true;
                    //GameDebug.Log("List KV values succeed" );
                }
            }
        }

        public void PutKVValue(LobbyRecord kvRecord) {
            string data = "[" +JsonUtility.ToJson(kvRecord)+"]"; // JsonUtility can not handle bare values
            _gdnErrorHandler.isWaiting = true;
            _monoBehaviour.StartCoroutine(MacrometaAPI.PutKVValue(_gdnData, lobbiesKVCollectionName,
                data, PutKVValueCallback));
        }

        public void PutKVValueCallback(UnityWebRequest www) {
            _gdnErrorHandler.isWaiting = false;
            putKVValueDone = false;
            if (www.isHttpError || www.isNetworkError) {
                _gdnErrorHandler.currentNetworkErrors++;
                GameDebug.Log("Put KV value: " + www.error);
            }
            else {
                //GameDebug.Log("put KV value succeed ");
                putKVValueDone = true;
                _gdnErrorHandler.currentNetworkErrors = 0;
            }
        }
    }

}