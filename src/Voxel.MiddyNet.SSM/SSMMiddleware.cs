using System.Threading.Tasks;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;

namespace Voxel.MiddyNet.SSM
{
    public class SSMMiddleware<Req, Res> : ILambdaMiddleware<Req, Res>
    {
        private string ParameterPath { get; }
        public SSMMiddleware(string parameterPath)
        {
            ParameterPath = parameterPath;
        }

        public Task<Res> After(Res lambdaResponse, MiddyNetContext context)
        {
            return Task.FromResult(lambdaResponse);
        }

        public async Task Before(Req lambdaEvent, MiddyNetContext context)
        {
            using var ssmClient = new AmazonSimpleSystemsManagementClient();
            var cloudTradeAuthResponse = await ssmClient.GetParameterAsync(new GetParameterRequest()
            {
                Name = ParameterPath
            });

            context.AdditionalContext.Add("SSSParameter", cloudTradeAuthResponse.Parameter.Value); // Need more work
        }
    }
}
