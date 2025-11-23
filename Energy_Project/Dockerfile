FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS restore
WORKDIR /src
COPY Energy_Project.sln .
COPY Energy_Project/Energy_Project.csproj Energy_Project/
COPY SmartHomeTests/SmartHomeTests.csproj SmartHomeTests/
RUN dotnet restore Energy_Project.sln

FROM restore AS build
COPY . .
RUN dotnet publish Energy_Project/Energy_Project.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM restore AS testrunner
COPY . .
WORKDIR /src
CMD ["dotnet", "test", "Energy_Project.sln", "-c", "Release", "--logger:trx;LogFileName=test-results.trx", "--results-directory", "/TestResults"]

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Energy_Project.dll"]
