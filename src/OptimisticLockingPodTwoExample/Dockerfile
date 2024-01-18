FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "src/OptimisticLockingPodTwoExample/OptimisticLockingPodTwoExample.csproj"

RUN dotnet publish -c Release -o /app src/OptimisticLockingPodTwoExample/OptimisticLockingPodTwoExample.csproj

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS publish
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "OptimisticLockingPodTwoExample.dll"]
