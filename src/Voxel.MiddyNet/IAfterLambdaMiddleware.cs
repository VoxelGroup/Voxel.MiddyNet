using System.Threading.Tasks;

namespace Voxel.MiddyNet
{
    public interface IAfterLambdaMiddleware<TRes>
    {
        Task<TRes> After(TRes lambdaResponse, MiddyNetContext context);
    }
}