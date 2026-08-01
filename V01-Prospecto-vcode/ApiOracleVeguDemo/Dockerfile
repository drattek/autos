FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["ApiOracleVeguDemo.csproj", "./"]
RUN dotnet restore "ApiOracleVeguDemo.csproj"
COPY . .
RUN dotnet publish "ApiOracleVeguDemo.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ApiOracleVeguDemo.dll"]