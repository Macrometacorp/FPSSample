using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotArray: MonoBehaviour {
    public struct BotData {
        public string name;
        public int botIndex;
        public int spawnIndex;
    }

    static public readonly BotData[] data;

    static BotArray() {
        data = new BotData[] {
            new BotData(){name = "BotA1", botIndex = 0, spawnIndex = -1},
            new BotData(){name = "BotA2", botIndex = 0, spawnIndex = -1},
            new BotData(){name = "BotA3", botIndex = 0, spawnIndex = -1},
            new BotData(){name = "BotA4", botIndex = 0, spawnIndex = -1},
            new BotData(){name = "BotB1", botIndex = 1, spawnIndex = -1},
            new BotData(){name = "BotB2", botIndex = 1, spawnIndex = -1},
            new BotData(){name = "BotB3", botIndex = 1, spawnIndex = -1},
            new BotData(){name = "BotB4", botIndex = 1, spawnIndex = -1},
        };
    }

}
