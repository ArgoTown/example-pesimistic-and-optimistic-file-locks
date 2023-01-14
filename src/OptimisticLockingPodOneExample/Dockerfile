FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "OptimisticLockingPodOneExample/OptimisticLockingPodOneExample.csproj"

RUN dotnet publish -c Release -o /app OptimisticLockingPodOneExample/OptimisticLockingPodOneExample.csproj

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS publish
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "OptimisticLockingPodOneExample.dll"]
