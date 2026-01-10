FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Tripwithfriends/Tripwithfriends.csproj", "Tripwithfriends/"]
RUN dotnet restore "Tripwithfriends/Tripwithfriends.csproj"
COPY . .
WORKDIR "/src/Tripwithfriends"
RUN dotnet build "Tripwithfriends.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Tripwithfriends.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tripwithfriends.dll"]


