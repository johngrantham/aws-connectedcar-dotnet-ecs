FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG TOKEN
WORKDIR /src
COPY /src ./
WORKDIR /app
RUN dotnet nuget add source \
  --username USERNAME \
  --password $TOKEN \
  --store-password-in-clear-text \
  --name github "https://nuget.pkg.github.com/johngrantham/index.json"
RUN dotnet publish "/src/ConnectedCar.Api/ConnectedCar.Api.csproj" \
  --use-current-runtime \
  -c Release \
  -o /app/output

FROM mcr.microsoft.com/dotnet/aspnet:6.0
RUN adduser \
  --disabled-password \
  --home /app \
  --gecos '' app \
  && chown -R app /app
USER app
WORKDIR /app
COPY --from=build /app/output ./
ENV DOTNET_RUNNING_IN_CONTAINER=true ASPNETCORE_URLS=http://+:5000
EXPOSE 5000
ENTRYPOINT ["dotnet", "ConnectedCar.Api.dll"]
