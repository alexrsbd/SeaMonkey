using SeaMonkey.Commands;

namespace SeaMonkey
{
    public interface IMonkeyCommandFactory
    {
        IMonkeyCommand GetCommand(MonkeyBreed breed);
    }
}