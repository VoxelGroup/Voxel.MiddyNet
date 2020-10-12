Welcome to MiddyNet
===================

**MiddyNet** is a lightweight middleware library for .NET Core and AWS Lambda that let you focus on what's really important for you: your business logic.

**MiddyNet** is a port to .NET Core of the fantastic `Middy <https://github.com/middyjs/middy>`_ library for Javascript.

**MiddyNet** will *force* you to organise your lambda's code in a certain way, but we think it's a reasonable and practical way to do it. 

**MiddyNet** also includes a custom logger, that will help you logging structured logs and that let you enrich your logs with custom properties.

**MiddyNet** also includes a way to read and propagate the `TraceContext <https://www.w3.org/TR/trace-context/>`_ so you can have distributed tracing without any effort.

.. toctree::
   :maxdepth: 3
   :hidden:
   :caption: About MiddyNet

   intro/howitworks
   intro/contributing

.. toctree::
   :maxdepth: 3
   :hidden:
   :caption: Getting Started Guide

   started/basics
   started/usemiddleware
   started/middycontext
   started/logger

.. toctree::
   :maxdepth: 3
   :hidden:
   :caption: Writing a Middleware

   writing/writing

.. toctree::
   :maxdepth: 3
   :hidden:
   :caption: Existing NuGet packages

   nuget/packages