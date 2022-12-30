#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["OptimisticLockingPodOneExample/OptimisticLockingPodOneExample.csproj", "OptimisticLockingPodOneExample/"]
RUN dotnet restore "OptimisticLockingPodOneExample/OptimisticLockingPodOneExample.csproj"
COPY . .
WORKDIR "/src/OptimisticLockingPodOneExample"
RUN dotnet build "OptimisticLockingPodOneExample.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OptimisticLockingPodOneExample.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OptimisticLockingPodOneExample.dll"]