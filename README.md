# OwaspHeaders.IsolatedFunction
A .NET Core middleware for injecting the Owasp recommended HTTP Headers into Azure Isolated Functions

![](images/OwaspAzureFuncIcon.png)

## NuGet Package
The nuget package can be accessed [here](https://www.nuget.org/packages/OwaspHeaders.IsolatedFunction/1.1.0)

## Story
The first thing is a big thank to GaProgMan for creating [OwaspHeaders.Core](https://github.com/GaProgMan/OwaspHeaders.Core)
This library is just an extension to his work to support Azure Isolated 
function.

## Usage
```c#
IConfigurationRoot _configuration = null;
var host = new HostBuilder()
   .ConfigureFunctionsWorkerDefaults(builder =>
   {
      builder.UseMiddleware<OwaspHandlerMiddleware>();
   })
```

## Configuration
The configuration is based on the original project. 
So please find the coniguration [here](https://github.com/GaProgMan/OwaspHeaders.Core/blob/master/README.md#configuration)