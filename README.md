# BFF secured ASP.NET Core application using downstream API and certificate authentication

![image.png](images/context.svg)

## Debugging

Start the Angular project from the **ui** folder

```
nx serve --ssl
```

Start the ASP.NET Core projects from the **server** folder and the **identityProvider**

```
dotnet run
```

Or just open Visual Studio and run the solution.

## Credits and used libraries

- NetEscapades.AspNetCore.SecurityHeaders
- Yarp.ReverseProxy
- OpenIddict
- ASP.NET Core
- Angular 
- Nx
- OpenIddict

## History

- 2024-10-27 Initial version

## Angular nx Updates

```
nx migrate latest

nx migrate --run-migrations=migrations.json
```

## Links

https://github.com/damienbod/bff-aspnetcore-angular

https://learn.microsoft.com/en-us/aspnet/core/introduction-to-aspnet-core

https://nx.dev/getting-started/intro

https://github.com/isolutionsag/aspnet-react-bff-proxy-example

https://github.com/openiddict

https://github.com/damienbod/bff-auth0-aspnetcore-angular

https://github.com/damienbod/bff-azureadb2c-aspnetcore-angular

https://github.com/damienbod/bff-aspnetcore-vuejs

https://github.com/damienbod/bff-MicrosoftEntraExternalID-aspnetcore-angular

https://microsoft.github.io/reverse-proxy/articles/transforms.html

https://github.com/microsoft/reverse-proxy/tree/main/samples/ReverseProxy.Transforms.Sample