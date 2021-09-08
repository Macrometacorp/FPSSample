using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using BestHTTP.WebSocket;
using Macrometa.Lobby;
using Random = UnityEngine.Random;

namespace Macrometa {
    public class GDNClientLobbyNetworkDriver2 : GDNNetworkDriver {
        
        public bool startDocumentInit =true;

        public string localId;
        public string gameMode;
        public string mapName;
        public int maxPlayers; 
        
        
        public bool tryDocumentInit = false;
        public string clientId;

        public int nextGameSerialnumber = 1;
        public GdnDocumentLobbyDriver gdnDocumentLobbyDriver;
        public LobbyValue lobbyValue;
        public bool lobbyUpdateAvail = true;
        public bool isLobbyAdmin;
        public float nextUpdateLobby = 0;
        

        public LobbyList lobbyList = new LobbyList();
        public float nextRefreshLobbyList= 0;

        protected Lobby.PingData transportPingData;
        public bool sendTransportPing = false;
        public Lobby.PingData debugPingData;
        public bool waitStreamClearing = false;
        public float streamClearTime = 0;

        static  GDNClientLobbyNetworkDriver2 _inst;
        
        public override void Awake() {
            _inst = this;
            GDNStreamDriver.isClientBrowser = true;
            GDNStreamDriver.localId = localId;
            gdnErrorHandler = new GDNErrorhandler();
            var defaultConfig = RwConfig.ReadConfig();
            RwConfig.Flush();
            baseGDNData = defaultConfig.gdnData;
            BestHTTP.HTTPManager.Setup();
            BestHTTP.HTTPManager.MaxConnectionPerServer = 64;
            gdnStreamDriver = new GDNStreamDriver(this);
            gdnDocumentLobbyDriver = new GdnDocumentLobbyDriver(this);
            gdnStreamDriver.statsGroupSize = defaultConfig.statsGroupSize;
            if (gdnStreamDriver.statsGroupSize < 1) {
                gdnStreamDriver.statsGroupSize = 10; //seconds
            }
            gdnStreamDriver.nodeId = PingStatsGroup.NodeFromGDNData(baseGDNData);
            GameDebug.Log("Setup GDNClientBrowserNetworkDriver: " + gdnStreamDriver.nodeId);
            setRandomClientName();
            gdnStreamDriver.chatStreamName = "FPSChat";
            gdnStreamDriver.chatChannelId = "_Lobby";
            lobbyList = gdnDocumentLobbyDriver.lobbyList;
            lobbyList.isDirty = true;
            MakeGDNConnection(null); //servers are all using default name server.
        }
        public override void Update() {
            Bodyloop();
        }

        public void CreateLobby() {
            startDocumentInit = true;
        }
        public void OnDisable() {
            GameDebug.Log("GDNClientBrowserNetworkDriver OnDisable");
        }

        public void MakeGDNConnection(LobbyValue aL) {
            var destination = "Server";
            if (aL != null) {
                destination = aL.clientId;
            }
                
            gdnStreamDriver.setRandomClientName();
            var connection = new GDNNetworkDriver.GDNConnection() {
                source = gdnStreamDriver.consumerName,
                destination = destination,
                port = 443
            };

            var id =gdnStreamDriver.AddOrGetConnectionId(connection);
            
        }
        
        public void setRandomClientName() {
            clientId = "Cl" + (10000000 + Random.Range(1, 89999999)).ToString();
        }

        public void JoinLobby(LobbyValue lobbyValue, bool isAdmin) {
            SetIsLobbyAdmin(isAdmin);
            GameDebug.Log("JoinLobby:" + lobbyValue.streamName);
            gdnStreamDriver.chatChannelId = lobbyValue.streamName;
            gdnStreamDriver.chatLobbyId = lobbyValue.streamName;
        }
        
        public void SetIsLobbyAdmin(bool val) {
            GDNStreamDriver.isLobbyAdmin = val;
            isLobbyAdmin = val;
        }


        public void UpdateLocalLobby(LobbyValue lobbyUpdate) {
            GameDebug.Log("UpdateLocalLobby");
            lobbyValue = lobbyUpdate;
            lobbyUpdateAvail = true;
        }
        
        public void Bodyloop() {

            if (gdnStreamDriver.lobbyUpdateAvail) {
                GameDebug.Log("gdnStreamDriver.lobbyUpdateAvail");
                UpdateLocalLobby( gdnStreamDriver.lobbyUpdate);
                gdnStreamDriver.lobbyUpdateAvail = false;

            }

            if (gdnErrorHandler.pauseNetworkErrorUntil > Time.time) return;
            if (gdnErrorHandler.currentNetworkErrors >= gdnErrorHandler.increasePauseConnectionError) {
                gdnErrorHandler.pauseNetworkError *= gdnErrorHandler.pauseNetworkErrorMultiplier;
                return;
            }

            if (gdnErrorHandler.isWaiting) return;

            if (!gdnDocumentLobbyDriver.collectionListDone) {
                GameDebug.Log("CollectionListDone not done");
                gdnDocumentLobbyDriver.GetListDocumentCollections();
                return;
            }

            
            if (!gdnDocumentLobbyDriver.lobbiesCollectionExists) {
                GameDebug.Log("Setup  lobbiesCollectionExists  A");
                gdnDocumentLobbyDriver.CreateLobbiesCollection();
                return;
            }

            if (!gdnDocumentLobbyDriver.indexesListDone) {
                GameDebug.Log(" indexesListDone not done");
                gdnDocumentLobbyDriver.GetListIndexes(gdnDocumentLobbyDriver.lobbiesCollectionName);
                return;
            }

            for (int checkIndex = 0; checkIndex < gdnDocumentLobbyDriver.indexesExist.Count; checkIndex++) {
                
                if (!gdnDocumentLobbyDriver.indexesExist[checkIndex]) {
                    gdnDocumentLobbyDriver.CreateIndex( checkIndex);
                    return;
                }
            }

            if (!gdnDocumentLobbyDriver.indexTTLExist) {
                gdnDocumentLobbyDriver.CreateTTLIndex();
                return;
            }
            
            if (!gdnStreamDriver.regionIsDone) {
                gdnStreamDriver.GetRegion();
                return;
            }
            
            if (!gdnStreamDriver.streamListDone) {
                gdnStreamDriver.GetListStream();
                return;
            }
            
            if (!gdnStreamDriver.chatStreamExists) {
                gdnStreamDriver.CreateChatStream();
                return;
            }
            
            if (!gdnStreamDriver.chatProducerExists) {
                gdnStreamDriver.CreateChatProducer(gdnStreamDriver.chatStreamName);
                return;
            }

            if (!gdnStreamDriver.chatConsumerExists) {
                gdnStreamDriver.CreateChatConsumer(gdnStreamDriver.chatStreamName, gdnStreamDriver.consumerName);
                return;
            }
            
            if (!gdnDocumentLobbyDriver.lobbyListIsDone ) {
                gdnDocumentLobbyDriver.PostLobbyListQuery();
                nextRefreshLobbyList = Time.time + 10;
                return;
            }

            if (nextRefreshLobbyList > Time.time) {
                gdnDocumentLobbyDriver.lobbyListIsDone = false;
            }
            
            
            if (startDocumentInit) {
                startDocumentInit = false;
                tryDocumentInit = true;
                gdnDocumentLobbyDriver.lobbyIsMade = false;
                gdnDocumentLobbyDriver.maxSerialIsDone = false;
                gdnDocumentLobbyDriver.postLobbyStuff = false;
            }

            if (tryDocumentInit) {
                if (!gdnDocumentLobbyDriver.maxSerialIsDone && !gdnDocumentLobbyDriver.lobbyIsMade) {
                    gdnDocumentLobbyDriver.PostMaxSerialQuery(RwConfig.ReadConfig().gameName);
                    return;
                }

                if (!gdnDocumentLobbyDriver.lobbyIsMade) {
                    if (gdnDocumentLobbyDriver.maxSerialResult.result.Count == 0) {
                        nextGameSerialnumber = 1 + gdnDocumentLobbyDriver.errorSerialIncr;
                    }
                    else {
                        nextGameSerialnumber = gdnDocumentLobbyDriver.maxSerialResult.result[0] + 1
                            + gdnDocumentLobbyDriver.errorSerialIncr;
                    }

                    //unassigned.slots = new List<TeamSlot>();
                    lobbyValue = new LobbyValue() {
                        adminName = localId,
                        gameMode = gameMode,
                        mapName = mapName,
                        clientId = clientId,
                        maxPlayers = maxPlayers,
                        baseName = RwConfig.ReadConfig().gameName,
                        streamName = RwConfig.ReadConfig().gameName + "_" + nextGameSerialnumber,
                        serialNumber = nextGameSerialnumber,
                        region = gdnStreamDriver.region,
                    };

                    lobbyValue.MoveToTeam(SelfTeamSlot(), 2);
                    AddDummyTeamSlots(0, 4);
                    AddDummyTeamSlots(1, 1);
                    AddDummyTeamSlots(2, 3);
                    GameDebug.Log("make lobby: " + lobbyValue.streamName);
                    var lobbyLobby = LobbyLobby.GetFromLobbyValue(lobbyValue);
                    gdnDocumentLobbyDriver.PostLobbyDocument(lobbyLobby);
                    return;
                }

                if (gdnDocumentLobbyDriver.postLobbyStuff) {
                    GameDebug.Log("Lobby joined");
                    JoinLobby(lobbyValue, true);
                    gdnDocumentLobbyDriver.postLobbyStuff = false;
                    tryDocumentInit = false;
                    //initial insert message comes before this joinLobby can happen
                    UpdateLobby();
                    return;
                }
            }

            if (isLobbyAdmin && gdnDocumentLobbyDriver.lobbyIsMade && Time.time > nextUpdateLobby) {
                GameDebug.Log("heartbeat update");
                UpdateLobby();
                return;
            }
            
            if (StreamsBodyLoop()) {
               debugPingData = transportPingData.Copy();
                return;
            }

        }

        public void UpdateLobby() {
            nextUpdateLobby = Time.time + 10;
            var lobbyLobby = LobbyLobby.GetFromLobbyValue(lobbyValue);
            var key = gdnDocumentLobbyDriver.lobbyKey;
            gdnDocumentLobbyDriver.UpdateLobbyDocument(lobbyLobby, key);
        }

        /// <summary>
        /// returns true if containing loop should return
        /// </summary>
        /// <returns></returns>
        public bool  StreamsBodyLoop() {
            //GameDebug.Log("StreamsBodyLoop A");
            if (!gdnStreamDriver.lobbyDocumentReaderExists) {
               
                gdnStreamDriver.CreateDocuomentReader(gdnDocumentLobbyDriver.lobbiesCollectionName, clientId);
                return false;
            }
            //GameDebug.Log("StreamsBodyLoop B");
            if (waitStreamClearing) {
                if (Time.time > streamClearTime) {
                    FinishClearStreams();
                }
                return false; // none streams action OK
            }
            gdnStreamDriver.ExecuteLobbyCommands();
           // GameDebug.Log("StreamsBodyLoop C");
            SetPings();
            if (transportPingData == null) {
                return false;
            }
            GameDebug.Log("StreamsBodyLoop D");
            if (!gdnStreamDriver.producerExists) {
                gdnStreamDriver.CreateProducer(gdnStreamDriver.producerStreamName);
                return true;
            }
            GameDebug.Log("StreamsBodyLoop E");
            if (!gdnStreamDriver.consumerExists) {
                gdnStreamDriver.CreateConsumerPongOnly(gdnStreamDriver.consumerStreamName, gdnStreamDriver.consumerName);
                return false;
            }
            GameDebug.Log("StreamsBodyLoop F");
            if (sendTransportPing) {
                GameDebug.Log(" PingBodyLoop() sendTransportPing");
                gdnStreamDriver.SendSimpleTransportPing();
                sendTransportPing = false;
            }

            if (TransportPings.PingTime() > 15000) {

                // what should gdnStreamDriver.receivedPongOnly
                // be set to?
                lobbyValue.ping = -1;
                StartClearStreams();
                transportPingData = null;
                TransportPings.Clear();
                sendTransportPing = false;
                gdnStreamDriver.receivedPongOnly = false;
            }

            if (gdnStreamDriver.receivedPongOnly) {
                gdnStreamDriver.receivedPongOnly = false;
                transportPingData.pingCount++;
                if (transportPingData.pingCount > 3) {
                    GameDebug.Log("pingCount set "+ lobbyValue.streamName + " : "+ gdnStreamDriver.pongOnlyRtt) ;
                    lobbyValue.ping = gdnStreamDriver.pongOnlyRtt;
                    StartClearStreams();
                    transportPingData = null;
                }
                else {
                    sendTransportPing = true;
                }
            }
            GameDebug.Log("StreamsBodyLoop Z");
           
            return false;
        }

        public void SetPings() {
            if (transportPingData == null) {
                var unpingedLobby = lobbyList.UnpingedLobby();
                if (unpingedLobby != null) {
                    transportPingData = new Lobby.PingData() {
                        lobbyValue = unpingedLobby
                    };
                    //ClearStreams();
                    gdnStreamDriver.producerStreamName = unpingedLobby.streamName + "_InStream";
                    gdnStreamDriver.consumerStreamName  = unpingedLobby.streamName + "_OutStream";
                    sendTransportPing = true;
                    lobbyValue = unpingedLobby;
                }
            }
            
        }

        public void StartClearStreams() {
            GameDebug.Log("ClearStreams");
            if (gdnStreamDriver.consumer1 != null) {
                gdnStreamDriver.consumer1.Close();
            }
            if (gdnStreamDriver.producer1 != null) {
                gdnStreamDriver.producer1.Close();
            }

            waitStreamClearing = true;
            streamClearTime = Time.time + 0.5f;
        }

        public void FinishClearStreams() {
            gdnStreamDriver.consumer1 = null;
            gdnStreamDriver.producer1  = null;
            
            gdnStreamDriver.producerExists = false;
            gdnStreamDriver.consumerExists = false;
            
            waitStreamClearing = false;
        }
        
        #region LobbyManipulation
        public Lobby.TeamSlot SelfTeamSlot() {
            var result = new TeamSlot() {
                playerName = localId,
                clientId = clientId,
                region = gdnStreamDriver.region,
                ping = gdnStreamDriver.chatProducer1.Latency,
            };
            return result;
        }

        public static Lobby.TeamSlot MakeSelfTeamSlot() {
           return  _inst.SelfTeamSlot();
        }

        /// <summary>
        /// This is static to make calling from UI easier
        /// </summary>
        /// <param name="teamIndex"></param>
        static public void MoveToTeam(int teamIndex) {
            _inst.gdnStreamDriver.ChatSendRoomRequest(teamIndex);
        }

        static public void TeamNameChanged(string teamName, int teamIndex) {
            /// this change goes directo lobby and updates
            _inst.lobbyValue.TeamFromIndex( teamIndex).name = teamName;
            _inst.UpdateLobby();
        }
        
        static public bool MoveToTeam(TeamSlot teamSlot,int teamIndex) {
            var val = _inst.lobbyValue.MoveToTeam(teamSlot, teamIndex);
            _inst.UpdateLobby();
            return val;
        }

        //testing

        public void AddDummyTeamSlots(int teamIndex, int count) {
            for (int i = 0; i < count; i++) {
                var dummy = DummyTeamSlot();
                lobbyValue.MoveToTeam(dummy, teamIndex);
            }
        }
        
        public Lobby.TeamSlot DummyTeamSlot() {
            var result = new TeamSlot() {
                playerName = "Dummy "+ Random.Range(1,1000).ToString(),
                clientId = Random.Range(1,1000).ToString(),
                region = gdnStreamDriver.region,
                ping = gdnStreamDriver.chatProducer1.Latency,
            };
            return result;
        }
        
        
        
        #endregion LobbyManipulation
        
    }
}