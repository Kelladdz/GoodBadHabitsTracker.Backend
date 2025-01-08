FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

WORKDIR /app

EXPOSE 80
ENV ConnectionStrings:Default "Server=gbht.db;Database=GBHTdb;User=sa;Password=Danger11;Encrypt=True;TrustServerCertificate=True"

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["GoodBadHabitsTracker.Infrastructure/GoodBadHabitsTracker.Infrastructure.csproj", "GoodBadHabitsTracker.Infrastructure/"]
COPY ["GoodBadHabitsTracker.Core/GoodBadHabitsTracker.Core.csproj", "GoodBadHabitsTracker.Core/"]
COPY ["GoodBadHabitsTracker.Application/GoodBadHabitsTracker.Application.csproj", "GoodBadHabitsTracker.Application/"]
COPY ["GoodBadHabitsTracker.WebApi/GoodBadHabitsTracker.WebApi.csproj", "GoodBadHabitsTracker.WebApi/"]

RUN dotnet restore "GoodBadHabitsTracker.WebApi/GoodBadHabitsTracker.WebApi.csproj"

COPY . .

WORKDIR "/src/GoodBadHabitsTracker.WebApi"

RUN dotnet build "GoodBadHabitsTracker.WebApi.csproj" -c Development -o /app/build

FROM build AS publish 
RUN dotnet publish "GoodBadHabitsTracker.WebApi.csproj" -c Development -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .
RUN ls -l
ENTRYPOINT ["dotnet", "GoodBadHabitsTracker.WebApi.dll"]