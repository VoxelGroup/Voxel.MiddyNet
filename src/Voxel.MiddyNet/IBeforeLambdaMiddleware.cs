using System.Threading.Tasks;

namespace Voxel.MiddyNet
{
    public interface IBeforeLambdaMiddleware<in TReq>
    {
        Task Before(TReq lambdaEvent, MiddyNetContext context);
    }
}
