FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["GoodBadHabitsTracker.Infrastructure/GoodBadHabitsTracker.Infrastructure.csproj", "GoodBadHabitsTracker.Infrastructure"]
COPY ["GoodBadHabitsTracker.Core/GoodBadHabitsTracker.Core.csproj", "GoodBadHabitsTracker.Core"]
COPY ["GoodBadHabitsTracker.Application/GoodBadHabitsTracker.Application.csproj", "GoodBadHabitsTracker.Application"]
COPY ["GoodBadHabitsTracker.WebApi/GoodBadHabitsTracker.WebApi.csproj", "GoodBadHabitsTracker.WebApi"]
RUN dotnet restore "GoodBadHabitsTracker.WebApi/GoodBadHabitsTracker.WebApi.csproj"
COPY . .
WORKDIR "/src/GoodBadHabitsTracker.WebApi"
RUN dotnet build "GoodBadHabitsTracker.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish 
ARG BUILD_CONFIGURATION=ReleaseRUN dotnet publish "GoodBadHabitsTracker.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish

FROM base  AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GoodBadHabitsTracker.WebApi.dll"]
