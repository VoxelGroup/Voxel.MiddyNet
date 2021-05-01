using System.Threading.Tasks;

namespace Voxel.MiddyNet
{
    public interface ILambdaMiddleware<in TReq, TRes>
    {
        Task Before(TReq lambdaEvent, MiddyNetContext context);

        Task<TRes> After(TRes lambdaResponse, MiddyNetContext context);

        bool InterruptsExecution { get; }
    }
}
