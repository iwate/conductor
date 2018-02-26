FROM microsoft/dotnet:sdk AS build-env
WORKDIR /app

# copy everything else and build
COPY . ./
RUN dotnet restore
RUN dotnet build -c Release ./Conductor.sln
RUN dotnet publish -c Release ./Conductor.Web/Conductor.Web.csproj -o ../out

# build runtime image
FROM microsoft/dotnet:runtime
WORKDIR /app
COPY --from=build-env /app/out ./
# ssh
ENV SSH_PASSWD "root:Docker!"
RUN apt-get update \
        && apt-get install -y --no-install-recommends dialog \
        && apt-get update \
    && apt-get install -y --no-install-recommends openssh-server \
    && echo "$SSH_PASSWD" | chpasswd 

COPY sshd_config /etc/ssh/
EXPOSE 5000 2222
ENTRYPOINT ["dotnet", "Conductor.Web.dll"]