[System.Serializable]
public class ActiveBuff
{
    public BuffData data;

    public float remainTime;

    public ActiveBuff(BuffData buffData)
    {
        data = buffData;
        remainTime = buffData.BuffTime;
    }

    public void Tick(float deltaTime)
    {
        remainTime -= deltaTime;
    }

    public bool IsExpired()
    {
        return remainTime <= 0;
    }
}