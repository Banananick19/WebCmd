FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
RUN apt update
RUN apt install iputils-ping -y
RUN apt install nmap -y

WORKDIR /app
EXPOSE 80


FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["WebCmd.App/WebCmd.App.csproj", "WebCmd.App/"]
RUN dotnet restore "WebCmd.App/WebCmd.App.csproj"
COPY . .
WORKDIR "/src/WebCmd.App"
RUN dotnet build "WebCmd.App.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WebCmd.App.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebCmd.App.dll"]
