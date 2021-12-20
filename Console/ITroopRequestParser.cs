namespace SeaMonkey
{
    public interface ITroopRequestParser
    {
        TroopRequest Parse(string[] args);
    }
}