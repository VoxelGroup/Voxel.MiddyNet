using System.Threading.Tasks;

namespace Voxel.MiddyNet
{
    public interface ILambdaMiddleware<Req, Res>
    {
        Task Before(Req lambdaEvent, MiddyNetContext context);
        Task<Res> After(Res lambdaResponse, MiddyNetContext context);
    }
}
