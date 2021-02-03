using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Voxel.MiddyNet.SSM.Tests
{

    public class IntegrationTests : IClassFixture<LaunchSettingsFixture>
    {
        private const string StringParameterName = "StringParameter";
        private const string StringParameterPath = "/MiddyNetTests/StringParameter";
        private const string SecureStringParameterName = "SecureStringParameter";
        private const string SecureStringParameterPath = "/MiddyNetTests/SecureStringParameter";
        private const string InvalidStringParameterName = "InvalidStringParameter";
        private const string InvalidStringParameterPath = "/MiddyNetTests/InvalidStringParameter";

        // We should set the values in the test, not manually
        [Fact]
        public async Task GetParameterValues()
        {
            var lambda = new TheLambdaFunction(new Dictionary<string, string>
            {
                {StringParameterName, StringParameterPath },
                {SecureStringParameterName, SecureStringParameterPath }
            });

            var result = await lambda.Handler(1, new FakeLambdaContext());

            result[0].Should().Be("StringParameterValue");
            result[1].Should().Be("SecureStringParameterValue");
        }

        [Fact]
        public void IfTheParameterDoesntExistTheExceptionShouldBeAddedToTheContext()
        {
            var lambda = new TheLambdaFunction(new Dictionary<string, string>
            {
                {InvalidStringParameterName, InvalidStringParameterPath }
            });

            Func<Task> act = async () => await lambda.Handler(1, new FakeLambdaContext());

            act.Should().Throw<Amazon.SimpleSystemsManagement.Model.ParameterNotFoundException>();
        }
    }

    public class LaunchSettingsFixture : IDisposable
    {
        public LaunchSettingsFixture()
        {
            var loadProfileFromLaunchSettings = Environment.GetEnvironmentVariable("IGNORE_LAUNCH_SETTINGS");

            if (loadProfileFromLaunchSettings == "true") return;

            using (var file = File.OpenText("Properties/launchSettings.json"))
            {
                var reader = new JsonTextReader(file);
                var jObject = JObject.Load(reader);

                var variables = jObject
                    .GetValue("profiles")
                    //select a proper profile here
                    .SelectMany(profiles => profiles.Children())
                    .SelectMany(profile => profile.Children<JProperty>())
                    .Where(prop => prop.Name == "environmentVariables")
                    .SelectMany(prop => prop.Value.Children<JProperty>())
                    .ToList();

                foreach (var variable in variables)
                {
                    Environment.SetEnvironmentVariable(variable.Name, variable.Value.ToString());
                }
            }
        }

        public void Dispose()
        {

        }
    }

    public class FakeLambdaContext : ILambdaContext
    {
        public string AwsRequestId { get; }
        public IClientContext ClientContext { get; }
        public string FunctionName { get; }
        public string FunctionVersion { get; }
        public ICognitoIdentity Identity { get; }
        public string InvokedFunctionArn { get; }
        public ILambdaLogger Logger { get; }
        public string LogGroupName { get; }
        public string LogStreamName { get; }
        public int MemoryLimitInMB { get; }
        public TimeSpan RemainingTime { get; }
    };

    public class TheLambdaFunction : MiddyNet<int, string[]>
    {
        public TheLambdaFunction(Dictionary<string, string> parametersToGet)
        {
            Use(new SSMMiddleware<int, string[]>(new SSMOptions
            {
                CacheExpiryInMillis = 60000,
                ParametersToGet = parametersToGet.Select(kvp => new SSMParameterToGet(kvp.Key, kvp.Value)).ToList()
            }));

            ParametersToGet = parametersToGet.Select(kvp => kvp.Key).ToList();
        }

        public List<string> ParametersToGet { get; set; }


        protected override Task<string[]> Handle(int lambdaEvent, MiddyNetContext context)
        {
            if (context.MiddlewareBeforeExceptions.Count > 0)
            {
                return Task.FromResult(new[] {context.MiddlewareBeforeExceptions[0].ToString()});
            }
            
            var results = ParametersToGet.Select(n => context.AdditionalContext[n].ToString()).ToArray();
            return Task.FromResult(results);
        }
    }
}
