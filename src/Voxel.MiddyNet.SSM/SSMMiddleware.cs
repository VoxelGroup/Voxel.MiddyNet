using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;

namespace Voxel.MiddyNet.SSM
{
    public class SSMParameterToGet
    {
        public string Name { get; }
        public string Path { get; }

        public SSMParameterToGet(string name, string path)
        {
            Name = name;
            Path = path;
        }
    }

    public class SSMOptions
    {
        public List<SSMParameterToGet> ParametersToGet { get; set; }
    }

    public class SSMMiddleware<TReq, TRes> : ILambdaMiddleware<TReq, TRes>
    {
        private readonly SSMOptions ssmOptions;

        private readonly Func<IAmazonSimpleSystemsManagement> ssmClientFactory;

        public SSMMiddleware(SSMOptions ssmOptions) : this(ssmOptions, () => new AmazonSimpleSystemsManagementClient())
        {
        }
        
        public SSMMiddleware(SSMOptions ssmOptions, Func<IAmazonSimpleSystemsManagement> ssmClientFactory)
        {
            this.ssmOptions = ssmOptions;
            this.ssmClientFactory = ssmClientFactory;
        }

        public Task<TRes> After(TRes lambdaResponse, MiddyNetContext context)
        {
            return Task.FromResult(lambdaResponse);
        }

        public async Task Before(TReq lambdaEvent, MiddyNetContext context)
        {
            using var ssmClient = ssmClientFactory();
            foreach (var parameter in ssmOptions.ParametersToGet)
            {
                var cloudTradeAuthResponse = await ssmClient.GetParameterAsync(new GetParameterRequest
                {
                    Name = parameter.Path
                });

                context.AdditionalContext.Add(parameter.Name, cloudTradeAuthResponse.Parameter.Value);
            }
        }
    }
}
