using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;

namespace Voxel.MiddyNet.SSMMiddleware
{
    public class SSMMiddleware<TReq, TRes> : ILambdaMiddleware<TReq, TRes>
    {
        private readonly SSMOptions ssmOptions;
        private readonly Func<IAmazonSimpleSystemsManagement> ssmClientFactory;
        private readonly ITimeProvider timeProvider;

        private Dictionary<string, CachedParameter> ParametersCache = new Dictionary<string, CachedParameter>();

        public SSMMiddleware(SSMOptions ssmOptions) : this(ssmOptions, () => new AmazonSimpleSystemsManagementClient(), new SystemTimeProvider())
        {
        }
        
        public SSMMiddleware(SSMOptions ssmOptions, Func<IAmazonSimpleSystemsManagement> ssmClientFactory, ITimeProvider timeProvider)
        {
            this.ssmOptions = ssmOptions;
            this.ssmClientFactory = ssmClientFactory;
            this.timeProvider = timeProvider;
        }

        public async Task Before(TReq lambdaEvent, MiddyNetContext context)
        {
            foreach (var parameter in ssmOptions.ParametersToGet)
            {
                if (IsParameterInCacheValid(parameter.Name))
                {
                    UpdateAdditionalContext(context, parameter.Name, ParametersCache[parameter.Name].Value);
                    UpdateCache(parameter.Name, ParametersCache[parameter.Name].Value);
                }
                else
                {
                    using (var ssmClient = ssmClientFactory())
                    {
                        try
                        {
                            var response = await ssmClient.GetParameterAsync(new GetParameterRequest
                            {
                                Name = parameter.Path,
                                WithDecryption = true
                            });

                            UpdateAdditionalContext(context, parameter.Name, response.Parameter.Value);
                            UpdateCache(parameter.Name, response.Parameter.Value);
                        }
                        catch (Exception ex)
                        {
                            context.MiddlewareBeforeExceptions.Add(ex);
                        }
                    }
                }
            }
        }

        public Task<TRes> After(TRes lambdaResponse, MiddyNetContext context)
        {
            return Task.FromResult(lambdaResponse);
        }

        private bool IsParameterInCacheValid(string parameterName)
        {
            if (!ParametersCache.ContainsKey(parameterName)) return false;

            if (ParametersCache[parameterName].InsertDateTime.AddMilliseconds(ssmOptions.CacheExpiryInMillis) >
                timeProvider.UtcNow) return true;

            return false;
        }

        private void UpdateAdditionalContext(MiddyNetContext context, string parameterName, string parameterValue)
        {
            if (context.AdditionalContext.ContainsKey(parameterName))
                context.AdditionalContext.Remove(parameterName);

            context.AdditionalContext.Add(parameterName, parameterValue);
        }

        private void UpdateCache(string parameterName, string parameterValue)
        {
            if (ParametersCache.ContainsKey(parameterName))
                ParametersCache.Remove(parameterName);

            ParametersCache.Add(parameterName, new CachedParameter(timeProvider.UtcNow, parameterValue));
        }
    }
}
