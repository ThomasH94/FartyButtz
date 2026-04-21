public class IncreaseScorePayload
{
    public int ScoreAmount { get; set; }
    public IncreaseScorePayload(int scoreAmount)
    {
        ScoreAmount = scoreAmount;
    }
}