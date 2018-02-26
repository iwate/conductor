FROM microsoft/dotnet:sdk AS build-env
WORKDIR /app

# copy everything else and build
COPY . ./
RUN dotnet publish -c Release ./Conductor.Web/Conductor.Web.csproj -o ../out

# build runtime image
FROM microsoft/dotnet:runtime
WORKDIR /app
COPY --from=build-env /app/out ./

ENV ASPNETCORE_URLS=5000
EXPOSE 5000

ENTRYPOINT ["dotnet", "Conductor.Web.dll"]