using System;
using Sirenix.Serialization;


public class IncreaseScoreAction : GameAction
{
    [OdinSerialize]
    private int m_ScoreAmount = 0;
    public int ScoreAmount => m_ScoreAmount;

    public override void Execute(GameActionContext context)
    {
        EventBus.Publish(new IncreaseScorePayload(m_ScoreAmount));
    }
}