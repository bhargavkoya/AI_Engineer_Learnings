namespace Securing_NET_AI_Apps
{
    public interface IPiiRedactor
    {
        RedactionResult Redact(string input);
    }
}
