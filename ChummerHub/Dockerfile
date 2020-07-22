#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build

#EXPOSE 80
#EXPOSE 54562
#EXPOSE 44321
#EXPOSE 443
#EXPOSE 1433
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

EXPOSE 80
EXPOSE 54562
EXPOSE 44321
EXPOSE 443
EXPOSE 1433
COPY . ./
RUN dotnet restore "ChummerHub/ChummerHub.csproj" 
RUN dotnet build "ChummerHub/ChummerHub.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ChummerHub/ChummerHub.csproj" -c Release -o /app/publish

FROM base AS final 
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ChummerHub.dll"]
