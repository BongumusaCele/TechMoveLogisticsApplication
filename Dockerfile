FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY TechMoveLogisticsApplication/TechMoveLogisticsApplication.csproj TechMoveLogisticsApplication/
RUN dotnet restore TechMoveLogisticsApplication/TechMoveLogisticsApplication.csproj
COPY TechMoveLogisticsApplication/ TechMoveLogisticsApplication/
WORKDIR /src/TechMoveLogisticsApplication
RUN dotnet publish TechMoveLogisticsApplication.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM runtime AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TechMoveLogisticsApplication.dll"]
