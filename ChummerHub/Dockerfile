#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 54562
EXPOSE 44321
EXPOSE 443
EXPOSE 1433


FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["ChummerHub/ChummerHub.csproj", "ChummerHub/"]
#COPY ["ChummerHub/packages/.", "ChummerHub/packages/"]

RUN dotnet restore "ChummerHub/ChummerHub.csproj" -s https://api.nuget.org/v3/index.json
#-s /src/ChummerHub/packages
COPY . .
WORKDIR "/src/ChummerHub"
RUN dotnet build "ChummerHub.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ChummerHub.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ChummerHub.dll"]
#WORKDIR /src
#
#COPY ["ChummerHub/ChummerHub.csproj", "ChummerHub/"]
#RUN dotnet restore "ChummerHub/ChummerHub.csproj"
#COPY . .
#WORKDIR "/src/ChummerHub"
#RUN dotnet build "ChummerHub.csproj" -c Release -o /app
#
#FROM build AS publish
#RUN dotnet publish "ChummerHub.csproj" -c Release -o /app
#
#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app .
#ENTRYPOINT ["dotnet", "ChummerHub.dll"]

#EXPOSE 80
#EXPOSE 54562
#EXPOSE 44321
#EXPOSE 443
#EXPOSE 1433
#COPY . ./
#RUN dotnet restore "ChummerHub/ChummerHub.csproj" 
#RUN dotnet build "ChummerHub/ChummerHub.csproj" -c Release -o /app/build
#
#FROM build AS publish
#RUN dotnet publish "ChummerHub/ChummerHub.csproj" -c Release -o /app/publish
#
#FROM base AS final 
#WORKDIR /app
#COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "ChummerHub.dll"]

#FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
#EXPOSE 80
#EXPOSE 54562
#EXPOSE 44321
#EXPOSE 443
#EXPOSE 1433
#
#WORKDIR /app
#
## Copy csproj and restore as distinct layers
#COPY "ChummerHub/ChummerHub.csproj" ./
##RUN curl http://nuget.org
##RUN dotnet restore 
#
## Copy everything else and build
#COPY ChummerHub/** ./
#RUN dotnet publish -c Release -o out
#
## Build runtime image
#FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
#WORKDIR /app
#COPY --from=build-env /app/out .
#ENTRYPOINT ["dotnet", "ChummerHub.dll"]
