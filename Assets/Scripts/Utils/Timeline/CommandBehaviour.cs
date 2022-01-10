using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;

[Serializable]
public class CommandBehaviour : PlayableBehaviour {
    private const string cls = " CommandBehaviour";
    public string commandAtStart;

    bool triggered = false;

    public override void OnGraphStart(Playable playable)
    {
        
        triggered = false;
        base.OnGraphStart(playable);
    }

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        base.OnBehaviourPlay(playable, info);
        if(!triggered)
        {
            GameDebugPlus.Log("CMD", cls, "OnBehaviourPlay()","ClientGameLoop.isBotString: " + ClientGameLoop.isBotString.Value);
            triggered = true;
            if (ClientGameLoop.isBotString.Value != "yes") {
                GameDebugPlus.Log("CMD", cls, "OnBehaviourPlay()", commandAtStart);
                Console.EnqueueCommandNoHistory(commandAtStart);
            }
        }
    }
}
