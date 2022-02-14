#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
EXPOSE 54562
EXPOSE 44321
EXPOSE 1433

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["ChummerHub/ChummerHub.csproj", "ChummerHub/"]
RUN dotnet restore "ChummerHub/ChummerHub.csproj"
COPY . .
WORKDIR "/src/ChummerHub"
RUN dotnet build "ChummerHub.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ChummerHub.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ChummerHub.dll"]
