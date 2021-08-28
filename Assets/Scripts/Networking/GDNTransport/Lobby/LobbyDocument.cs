using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;
using static Macrometa.MacrometaAPI;

namespace Macrometa.Lobby {
    
    /// <summary>
    /// Key value methods
    /// client browser mode check Lobby list 
    /// </summary>
    [Serializable]
    public class GdnDocumentLobbyDriver {
        private MonoBehaviour _monoBehaviour;
        private GDNData _gdnData;
        private GDNErrorhandler _gdnErrorHandler;
        //public ListKVCollection listKVCollection;
        public ListKVValue listKVValues ;
        public ListCollection listCollection;
        public ListIndexes listIndexes;
        //public ListDocumentValue listDocumentValues ;
        public bool collectionListDone = false;
        public bool lobbiesCollectionExists;
        public string lobbiesCollectionName = "FPSGames_Lobbies_Documents";
        public bool indexesListDone = false;

        public bool documentValueListDone;
        public bool putDocumentValueDone;
        public LobbyList lobbyList = new LobbyList();
        
        /// <summary>
        /// passing in a monobehaviour to be able use StartCoroutine
        /// happens because of automatic refactoring
        /// probably can hand cleaned
        /// </summary>
        /// <param name="gdnData"></param>
        /// <param name="gdnErrorhandler"></param>
        /// <param name="monoBehaviour"></param>
        public GdnDocumentLobbyDriver(GDNNetworkDriver gdnNetworkDriver) {

            _gdnData = gdnNetworkDriver.baseGDNData;
            _monoBehaviour = gdnNetworkDriver;
            _gdnErrorHandler = gdnNetworkDriver.gdnErrorHandler;
        }

        public void CreateLobbiesDocumentCollection() {
            lobbiesCollectionExists = listCollection.result.Any
                (item => item.name == lobbiesCollectionName);
            if (!lobbiesCollectionExists) {
                _gdnErrorHandler.isWaiting = true;
                //Debug.Log("creating server in stream: " + baseGDNData.CreateStreamURL(serverInStreamName));
                _monoBehaviour.StartCoroutine(CreateCollection(_gdnData, lobbiesCollectionName,
                    CreateDocumentCollectionCallback));
            }
        }

        public void CreateDocumentCollectionCallback(UnityWebRequest www) {
            _gdnErrorHandler.isWaiting = false;
            if (www.isHttpError || www.isNetworkError) {
                GameDebug.Log("Create document collection : " + www.error);
                _gdnErrorHandler.currentNetworkErrors++;
                collectionListDone = false;
            }
            else {
                var baseHttpReply = JsonUtility.FromJson<BaseHtttpReply>(www.downloadHandler.text);
                if (baseHttpReply.error == true) {
                    GameDebug.Log("create  Collection failed:" + baseHttpReply.code);
                    _gdnErrorHandler.currentNetworkErrors++;
                    collectionListDone = false;
                }
                else {
                    GameDebug.Log("Create Collection  ");
                    lobbiesCollectionExists = true;
                    _gdnErrorHandler.currentNetworkErrors = 0;
                }
            }
        }

        public void GetListDocumentCollections() {
            _gdnErrorHandler.isWaiting = true;
            _monoBehaviour.StartCoroutine(ListDocumentCollections(_gdnData, ListDocumentCollectionsCallback));
        }

        public void ListDocumentCollectionsCallback(UnityWebRequest www) {
            _gdnErrorHandler.isWaiting = false;
            if (www.isHttpError || www.isNetworkError) {
                _gdnErrorHandler.currentNetworkErrors++;
                GameDebug.Log("List Collections: " + www.error);
            }
            else {

                //overwrite does not assign toplevel fields
                //JsonUtility.FromJsonOverwrite(www.downloadHandler.text, listStream);
                listCollection = JsonUtility.FromJson<ListCollection>(www.downloadHandler.text);
                if (listCollection.error == true) {
                    GameDebug.Log("List Collection failed:" + listCollection.code);
                    //Debug.LogWarning("ListStream failed reply:" + www.downloadHandler.text);
                    _gdnErrorHandler.currentNetworkErrors++;
                }
                else {
                    collectionListDone = true;
                    _gdnErrorHandler.currentNetworkErrors = 0;
                }
            }
        }

        
        public void GetListIndexes(string collection) {
            _gdnErrorHandler.isWaiting = true;
            _monoBehaviour.StartCoroutine(ListIndexes(_gdnData, collection, ListIndexesCallback));
        }

        public void ListIndexesCallback(UnityWebRequest www) {
            _gdnErrorHandler.isWaiting = false;
            if (www.isHttpError || www.isNetworkError) {
                _gdnErrorHandler.currentNetworkErrors++;
                GameDebug.Log("List Indexes: " + www.error);
            }
            else {

                //overwrite does not assign toplevel fields
                //JsonUtility.FromJsonOverwrite(www.downloadHandler.text, listStream);
                listIndexes= JsonUtility.FromJson<ListIndexes>(www.downloadHandler.text);
                if (listIndexes.error == true) {
                    GameDebug.Log("List Indexes failed:" + listIndexes.code);
                    //Debug.LogWarning("ListStream failed reply:" + www.downloadHandler.text);
                    _gdnErrorHandler.currentNetworkErrors++;
                }
                else {
                    indexesListDone = true;
                    _gdnErrorHandler.currentNetworkErrors = 0;
                }
            }
        }

        
        public void GetListKVValues() {
            _gdnErrorHandler.isWaiting = true;
            _monoBehaviour.StartCoroutine(GetKVValues(_gdnData, lobbiesCollectionName,
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
                    
                    documentValueListDone = true;
                    _gdnErrorHandler.currentNetworkErrors = 0;
                    var newLobbyList = new List<LobbyValue>();
                    foreach (KVValue kvv in listKVValues.result) {
                        newLobbyList.Add(LobbyValue.FromKVValue(kvv));
                    }
                    LobbyValue.UpdateFrom(lobbyList.lobbies,newLobbyList);
                    lobbyList.isDirty = true;
                    //GameDebug.Log("List KV values succeed" );
                }
            }
        }

        public void PutKVValue(LobbyRecord kvRecord) {
            string data = "[" +JsonUtility.ToJson(kvRecord)+"]"; // JsonUtility can not handle bare values
            _gdnErrorHandler.isWaiting = true;
            _monoBehaviour.StartCoroutine(MacrometaAPI.PutKVValue(_gdnData, lobbiesCollectionName,
                data, PutKVValueCallback));
        }

        public void PutKVValueCallback(UnityWebRequest www) {
            _gdnErrorHandler.isWaiting = false;
            putDocumentValueDone = false;
            if (www.isHttpError || www.isNetworkError) {
                _gdnErrorHandler.currentNetworkErrors++;
                GameDebug.Log("Put KV value: " + www.error);
            }
            else {
                //GameDebug.Log("put KV value succeed ");
                putDocumentValueDone = true;
                _gdnErrorHandler.currentNetworkErrors = 0;
            }
        }
    }

}