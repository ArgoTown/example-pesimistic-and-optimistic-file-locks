FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "src/PessimisticLockingPodTwoExample/PessimisticLockingPodTwoExample.csproj"

RUN dotnet publish -c Release -o /app src/PessimisticLockingPodTwoExample/PessimisticLockingPodTwoExample.csproj

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS publish
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "PessimisticLockingPodTwoExample.dll"]
