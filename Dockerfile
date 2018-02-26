FROM microsoft/dotnet:2-sdk AS build-env
WORKDIR /app

# copy everything else and build
COPY . ./
RUN dotnet restore
RUN dotnet build -c Release ./Conductor.sln
RUN dotnet publish -c Release ./Conductor.Web/Conductor.Web.csproj -o ../out

# build runtime image
FROM microsoft/dotnet:2-runtime
WORKDIR /app
COPY --from=build-env /app/out ./

EXPOSE 5000

ENTRYPOINT ["dotnet", "Conductor.Web.dll"]