FROM mcr.microsoft.com/dotnet/core/sdk:2.2-alpine3.9 AS build

COPY src/API/API.csproj src/API/API.csproj
COPY src/DomainF/DomainF.fsproj src/DomainF/DomainF.fsproj
COPY src/Projections/Projections.csproj src/Projections/Projections.csproj
COPY src/Utils.Mvc/Utils.Mvc.csproj src/Utils.Mvc/Utils.Mvc.csproj
COPY src/Domain.Abstractions/Domain.Abstractions.csproj src/Domain.Abstractions/Domain.Abstractions.csproj
COPY src/WebSockets/WebSockets.csproj src/WebSockets/WebSockets.csproj

RUN dotnet restore src/API

COPY src/API/ src/API/
COPY src/DomainF/ src/DomainF/
COPY src/Projections/ src/Projections/
COPY src/Utils.Mvc/ src/Utils.Mvc/
COPY src/Domain.Abstractions/ src/Domain.Abstractions/
COPY src/WebSockets/ src/WebSockets/

RUN dotnet publish src/API -o out -c Release

FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-alpine3.9
COPY --from=build /src/API/out /app
WORKDIR /app
ENTRYPOINT ["dotnet", "Orleans.Tournament.API.dll"]
EXPOSE 7003
