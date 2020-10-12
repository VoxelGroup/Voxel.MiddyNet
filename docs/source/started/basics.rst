Getting started
===============

Installation
------------

**MiddyNet** is splited in different NuGet packages so that you only import what you really use. To start using it, add the main package to your AWS Lambda project::

    dotnet package add Voxel.MiddyNet


Conventions
-----------

The best way to organise your code when working with **MiddyNet** is to have a single file for each lambda function. Once you have that, you need to derive from ``MiddyNet<TReq,TRes>``, where TReq is the type of the input event (SNS, SQS, etc) and TRes is the type of the result. If your function doesn't have to return anything, you can derive from ``MiddyNet<TReq>``.

**MiddyNet** does its work in a function called Handler. This is the function you will need to specify when configuring your lambda as your entry point. So, at this point, all your lambdas will have its own source file and all of them will expose the same method called *Handler*. Nice and easy.

**MiddyNet** will make you implement a function called *Handle* where you will need to put your code.

So, a minimum skeleton of your lambda function would be something like this::

    public class MySQSLambdaFunction : MiddyNet<SQSEvent, int>
    {
        public MySQSLambdaFunction()
        {
            // Your own initializations 
            // MiddyNet middleware definitions. More on that later.
        }

        protected override Task<int> Handle(SQSEvent lambdaEvent, MiddyNetContext context)
        {
            // Your business logic

            return Task.FromResult(0);
        }
    }

Or if you don't want to return anything::
    
    public class MySQSLambdaFunction : MiddyNet<SQSEvent>
    {
        public MySQSLambdaFunction()
        {
            // Your own initializations 
            // MiddyNet middleware definitions. More on that later.
        }

        protected override Task<int> Handle(SQSEvent lambdaEvent, MiddyNetContext context)
        {
            // Your business logic

            return Task.CompletedTask;
        }
    }