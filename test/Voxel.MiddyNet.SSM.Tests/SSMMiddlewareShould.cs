using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Voxel.MiddyNet.SSM.Tests
{
    public class SSMMiddlewareShould
    {
        const string Param1Name = "param1Name";
        const string Param1Path = "param1Path";
        const string Param1Value = "param1Value";
        const string Param2Name = "param2Name";
        const string Param2Path = "param2Path";
        const string Param2Value = "param2Value";

        [Fact]
        public async Task GetParametersAndSetToContextOnBeforeMethod()
        {
            var ssmClient = Substitute.For<IAmazonSimpleSystemsManagement>();
            ssmClient.GetParameterAsync(Arg.Is<GetParameterRequest>(r => r.Name == Param1Path)).Returns(
                Task.FromResult(new GetParameterResponse {Parameter = new Parameter {Value = Param1Value}}));
            ssmClient.GetParameterAsync(Arg.Is<GetParameterRequest>(r => r.Name == Param2Path)).Returns(
                Task.FromResult(new GetParameterResponse { Parameter = new Parameter { Value = Param2Value } }));

            var context = new MiddyNetContext();

            var options = new SSMOptions
            {
                ParametersToGet = new List<SSMParameterToGet>
                {
                    new SSMParameterToGet(Param1Name, Param1Path),
                    new SSMParameterToGet(Param2Name, Param2Path)
                }
            };

            var ssmMiddleware = new SSMMiddleware<int, int>(options, () => ssmClient);
            await ssmMiddleware.Before(1, context);

            await ssmClient.Received().GetParameterAsync(Arg.Is<GetParameterRequest>(r => r.Name == Param1Path));
            await ssmClient.Received().GetParameterAsync(Arg.Is<GetParameterRequest>(r => r.Name == Param2Path));
            context.AdditionalContext.Should().Contain(Param1Name, Param1Value);
            context.AdditionalContext.Should().Contain(Param2Name, Param2Value);
        }
    }
}
