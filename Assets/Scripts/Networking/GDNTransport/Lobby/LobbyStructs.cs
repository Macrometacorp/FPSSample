using System;
using System.Collections.Generic;
using System.Linq;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.Esf;
using UnityEngine;
using UnityEngine.Networking;
using static Macrometa.MacrometaAPI;

namespace Macrometa.Lobby {
    
    /// <summary>
    /// A KV record
    /// </summary>
    public class LobbyRecord {
        public string _key; //base string name
        public string value;
        public long expireAt; //unix timestamp

        static public long UnixTSNow(long offset) {
            return (long) (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds + offset;
        }
        
        public static LobbyRecord GetFromLobbyValue(LobbyValue lobbyValue, long ttl) {
            return new LobbyRecord() {
                _key = lobbyValue.streamName,
                value = JsonUtility.ToJson(lobbyValue),
                expireAt = UnixTSNow(ttl)
            };
        }
        
    }

    [Serializable]
    public class LobbyDocument {
        public string baseName; //base string name
        public int serialNumber;
        public LobbyValue lobbyValue;
        
        static public long UnixTSNow(long offset) {
            return (long) (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds+ (offset);
        }
    }
    
    
    /// <summary>
    /// Can accept other document types JSON strings and not lose information
    /// i.e.  LobbyBase x = JsonUtility<LobbyBase>(other Lobby types JSON);
    /// check tags to find type
    /// </summary>
    /// [Serializable]
    public class LobbyBase : LobbyDocument {
        public bool gameMaster;
        public bool activeGame;
        public bool lobby;
    }
    
    [Serializable]
    public class LobbyGameMaster : LobbyDocument{
        public bool gameMaster = true; //this is a tag always set true
        
        public static LobbyGameMaster GetFromLobbyValue(LobbyValue lobbyValue) {
            return new LobbyGameMaster() {
                baseName = lobbyValue.streamName,
                serialNumber = lobbyValue.serialNumber,
                lobbyValue = lobbyValue
            };
        }
    }
    
    [Serializable]
    public class LobbyActiveGame : LobbyDocument {
        public bool activeGae= true; //this is a tag always set true
        public long lastUpdate = UnixTSNow(0);// in milliseconds 
        public static LobbyActiveGame GetFromLobbyValue(LobbyValue lobbyValue) {
            return new LobbyActiveGame() {
                baseName = lobbyValue.streamName,
                serialNumber = lobbyValue.serialNumber,
                lobbyValue = lobbyValue
            };
        }
    }
    
    [Serializable]
    public class LobbyLobby : LobbyDocument {
        public bool lobby= true; //this is a tag always set true
        public long lastUpdate = UnixTSNow(0); 
        
        
        public static LobbyLobby GetFromLobbyValue(LobbyValue lobbyValue) {
            return new LobbyLobby() {
                baseName = lobbyValue.baseName,
                serialNumber = lobbyValue.serialNumber,
                lobbyValue = lobbyValue
            };
        }
    }
    
    [Serializable]
    public class Team {
        public string name;
        public int maxSize;
        public List<TeamSlot> slots = new List<TeamSlot>();

        public int Find(string ClientId) {
            return slots.FindIndex(ts => ts.clientId == ClientId);
        }

        public void Remove(string clientId) {
            int i = Find(clientId);
            if (i > -1) {
                slots.RemoveAt(i);
            }
        }
    }

    [Serializable]
    public class TeamSlot {
        // public bool empty; is this needed??
        public string playerName;
        public string clientId; //player names are not unique so use clientId
        public Region region;
        public int ping;
        public int rtt;
        public bool runGameServer;
    }

    [Serializable]
    public class LobbyValue {
        public string clientId; // used for keeping record unique for KV collections are not ACID 
        public string baseName;
        public int serialNumber;
        public string gameMode;
        public string mapName;
        public int maxPlayers; // always 8?
        public string status; //init, waiting ( to start), playing
        public float ping; // only used locally not use in  db
        public string streamName; // only used locally is also _key in kv
        public string adminName;
        public Region region;
        public Team team0 = new Team() {maxSize = 4};
        public Team team1= new Team() {maxSize = 4};
        public Team unassigned= new Team() {maxSize = 0};
        public bool frozen;
        public bool serverClientChosen;
        
        public string DisplayName() {
            string displaySerial = serialNumber == 1 ? "" : " "+serialNumber.ToString();
            return baseName + displaySerial + " By " + adminName+ "\n"+ region.DisplayLocation();
        }

        #region KV
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
        #endregion kv
        public Team TeamFromIndex(int index) {
            switch (index) {
                case 0:
                    return team0;
                case 1:
                    return team1;
                case 2 :
                    return unassigned;
                default:
                    return null;
            }
        }

        public bool OnTeam(Team team, string clientId) {
            if (team == null) return false;
            return team.Find(clientId) != -1;
        }

        public bool SpaceOnTeam(Team team) {
            if (team == null) return true;
            if (team.maxSize == 0) return true;
            return team.maxSize > team.slots.Count;
        }

        public void RemoveFromOtherTeams( int teamIndex,string clientId) {
            for (int i = 0; i < 3; i++) {
                if (i == teamIndex) continue;
                TeamFromIndex(i).slots.RemoveAll(ts => ts.clientId == clientId);
            }
        }
        
        public bool MoveToTeam( TeamSlot teamSlot, int teamIndex) {
            var team = TeamFromIndex(teamIndex);
            if (OnTeam(team, teamSlot.clientId)) return false;
            if (!SpaceOnTeam(team)) return false;
            team?.slots.Add(teamSlot);
            RemoveFromOtherTeams(teamIndex, teamSlot.clientId);
            return true;
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
    
    [Serializable]
    public class PingData {
        public LobbyValue lobbyValue;
        public int pingCount;

        public PingData Copy() {
            return  new PingData() {
                lobbyValue = new LobbyValue() {
                    streamName = lobbyValue.streamName,
                    status = lobbyValue.status
                },
                pingCount = pingCount
            };
        }
    }
    
    [Serializable]
    public enum LobbyCommandType {
        RequestRoom,
        HeartBeat
    }
    
    [Serializable]
    public class LobbyCommand {
        public LobbyCommandType command;
        public int roomNumber;
        public string playerName;
        public string source; // clientId
        public bool succeed;
        public TeamSlot teamSlot;
    }
    
}