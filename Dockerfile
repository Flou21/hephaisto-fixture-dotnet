# Shop.Api - the image each fixture branch is published as.
#
#   docker build -t shop-api:dev .
#
# Multi-stage, non-root, port 8080. The runtime stage carries the portable PDBs that
# `dotnet publish` emits by default, so an unhandled exception's stack trace names the
# file and line - which is what makes a crash in this image diagnosable from its log alone.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Shop.sln ./
COPY src/Shop.Api/Shop.Api.csproj src/Shop.Api/
RUN dotnet restore src/Shop.Api/Shop.Api.csproj

COPY src/ src/
RUN dotnet publish src/Shop.Api/Shop.Api.csproj \
        --configuration Release \
        --no-restore \
        --output /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

# tini is PID 1, not dotnet, and a fixture that crashes depends on it.
#
# On an unhandled exception the .NET runtime ends the process with abort(), i.e. SIGABRT
# raised against itself. As PID 1 of a container's PID namespace the kernel drops that signal
# - init ignores every signal it has no handler for - and the process neither exits nor
# serves: measured with `docker run` on this image, the NullReferenceException is printed and
# the container then stays Up until it is killed. In Kubernetes that is a Running pod with
# zero restarts, not a CrashLoopBackOff, and no crash-loop alert ever fires.
#
# Under tini the runtime is an ordinary child, SIGABRT kills it, and tini exits 128+6 = 134.
# tini also forwards SIGTERM, so the healthy image on main still stops promptly.
RUN apt-get update \
 && apt-get install -y --no-install-recommends tini \
 && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /app ./

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

# The non-root user the aspnet image ships (uid 1654).
USER $APP_UID

ENTRYPOINT ["/usr/bin/tini", "--", "dotnet", "/app/Shop.Api.dll"]
