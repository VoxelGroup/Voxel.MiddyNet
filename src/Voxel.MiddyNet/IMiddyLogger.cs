namespace Voxel.MiddyNet
{
    public interface IMiddyLogger
    {
        void Log(LogLevel logLevel, string message);
        void Log(LogLevel logLevel, string message, params LogProperty[] properties);
        void EnrichWith(LogProperty logProperty);
    }
}