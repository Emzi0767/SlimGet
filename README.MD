# SlimGet
SlimGet is a lightweight implementation of a NuGet and symbol server, powered by ASP.NET Core 2.2, designed to be ran
in Docker. It's the software powering my NuGet feed at [nuget.emzi0767.com](https://nuget.emzi0767.com/).

Unlike alternative implementations, SlimGet is designed to host a single, local feed, with no upstream mirrors or
similar functionality. It's designed only with .NET packages (and debug symbols) in mind, and is therefore not
guaranteed to work with anything else (e.g. COM interop, native libraries, etc.).

The project was born out of need for a NuGet feed for CI artifacts of my projects, after my usual go-to provider
ceased responding to all my support emails, and broke my builds in the process.

The web interface provides a simple, readonly view of the feed, which allows for browsing and downloading hosted
packages, as well as viewing various metadata about them.

Provided are tools, which allow for issuing and revoking API tokens, allowing users to upload packages, as well as
manage packages in a limited fashion.

In the future, I might provide more integrated means of managing users and packages.

## Requirements
In order to run a SlimGet server, you need the following components installed and available on your system:
- .NET Core 2.2 runtime with ASP.NET Core installed
- PostgreSQL server 10 or better, with pg_trgm extension available
- Redis server

## Usage
The application is very easy and straightforward to set up, and complete guide is below. Before you begin, rename
`slimget.example.json` to `slimget.json` and open it in your favourite text editor.

## PostgreSQL Database
The application requires pre-made database with pg_trgm extension created, and a user to connect as. This is all fairly
easy to set up.

1. Connect to your PostgreSQL instance as the superuser (usually `postgres`) user. If you're unsure how to do it on
   your machine, consult PostgreSQL documentation for your operating system or distribution.
2. Create a user for the application: `create user slimget with nocreatedb nocreaterole encrypted password 'hunter2';`.
   Of course, replace `hunter2` with your desired password. Do not remove the quotes. You can also optionally replace
   `slimget` with your desired username. If you don't have an idea for a password, you can generate one on Linux using
   `dd if=/dev/random bs=1024 count=1 2>/dev/null | sha256sum - | cut -d' ' -f1`.
3. Create a database for the user: `create database slimget with owner='slimget';`. Again, you can replace `slimget`
   (the database name) with anything your heart desires, so long as it's a valid database name. If you changed the
   username in step 2, make sure to replace the owner name (`slimget`, the one in quotes) with the username you chose.
4. Connect to the newly-created database: `\c slimget`. If you changed the database name in step 3, replace `slimget`
   accordingly.
5. Create the pg_trgm extension: `create extension pg_trgm`.
6. Disconnect: `\q`.

At this point, you should switch to the editor with your SlimGet configuration, and edit the values in
`Storage.PostgreSQL` section accordingly. Below are the explanations of the various configuration options:

- `Hostname`: The hostname of your PostgreSQL server. If you're running locally, use `"localhost"`. Bear in mind that
  this requires the server to be listening over TCP/IP sockets. If you're unsure how to enable TCP/IP listening, check
  out these documentation pages:
  - [19.3.1. Connection Settings](https://www.postgresql.org/docs/current/runtime-config-connection.html#RUNTIME-CONFIG-CONNECTION-SETTINGS) -
    this page describes the configuration options in postgresql.conf (main configuration file) related to TCP/IP
    connections.
  - [20.1. pg_hba.conf file](https://www.postgresql.org/docs/current/auth-pg-hba-conf.html) - this page describes
    configuring authentication and connection options for users and clients.
- `Port`: The TCP port, on which the PostgreSQL server is listening.
- `Database`: The name of the database to connect to. If you did change the database name in step 3, replace
  `"slimget"` with the name you chose (remember about quotation marks).
- `Username`: The username you chose in step 2. If you did change the username, replace `"slimget"` with the name you
  chose (again, remember the quotation marks).
- `Password`: The password you chose in step 2, wrapped in quotation marks.
- `UseSsl`: If your server encrypts connections using SSL/TLS, set this to `true`, if you're unsure, or server does
  not offer encryption, set this to `false`. If you want to enable SSL/TLS on your server, check out the documentation
  on the subject:
  - [18.9. Secure TCP/IP Connections with SSL](https://www.postgresql.org/docs/current/ssl-tcp.html)
- `AlwaysTrustServerCertificate`: Whether the client should trust the server certificate unconditionally. This disables
  SSL certificate verification when connecting to your PostgreSQL server. If you're using a self-signed certificate,
  set this to `true`, otherwise it's strongly recommended you set this to `false`. This option has no effect if SSL is
  disabled.

## Redis
Redis does not require any setup on the server itself. Simply edit the values in `Storage.Redis` accordingly. Below are
explanations of the various options:

- `Hostname`: The hostname of your Redis server. If you're running locally, use `"localhost"`.
- `Port`: The TCP port your Redis server is listening on.
- `Index`: Database index to use for SlimGet. You can change this so that SlimGet does not interfere with other
  services that use Redis.
- `Password`: The password for your Redis server. If your server does not require a password, set this to `null`
  (without quotation marks). For more information, consult Redis documentation:
  - [Redis Security](https://redis.io/topics/security)
- `UseSsl`: If your Redis server encrypts connections using SSL/TLS, set this to `true`. Otherwise, set this to
  `false`.

## File system and package storage
These settings control various aspects of the actual package for your feed. Explanations for the various options are
below:

- `FileSystem`: Contains options pertaining to physical storage.
  - `StoragePath`: The path where pushed packages, symbols, and manifests will be stored. This should be an absolute
    path.
- `Packages`: Controls various aspects of the packages hosted in the feed.
  - `EnablePruning`: If enabled (set to `true`), your server will limit how many distinct versions of each package will
    be hosted at any given time. This is useful for restricting storage space.
  - `LatestVersionRetainCount`: Number of distinct package versions to retain. If pruning is enabled, this must be set
    to a positive number, which will define how many most recent package versions to retain. Any package versions over
    this limit will be deleted, starting with the oldest versions.
  - `MaxPackageSizeBytes`: Maximum size of an individual package upload, in bytes. Any packages that exceed this size
    will be rejected.
  - `DeleteEndpointUnlists`: Controls the behaviour of the package delete (DELETE /api/v2/package/{id}/{version})
    endpoint. If this option is set to `false`, calling this endpoint with a valid ID, version, and token will delete
    the requested package version completely. If set to `true`, the package version will simply become unlisted.
  - `ReadOnlyFeed`: Controls whether the feed is read-only. Setting this to `true` will disable package push, delete,
    relist, as well as debug symbol push endpoints.
  - `SymbolsEnabled`: Controls whether this feed should support hosting and pushing debug symbols. Setting this to
    `false` will disable debug symbol push and download endpoints.

## Server
Server section contains various options pertaining to the HTTP stack of SlimGet. These options are explained below:

- `SslCertificate`: Configures the location and password for the certificate. The certificate needs to be in PKCS12
  (usually .pfx or .p12) format, and needs to be protected with a non-empty password. See the certificate subsection
  for more details and information on how to generate a self-signed certificate for use with SlimGet.
  - `Location`: Full path to the certificate file. If set to `null`, HTTPS will be disabled, and SlimGet will only work
    over plain, unencrypted HTTP.
  - `PasswordFile`: Full path to the file containing the password to the certificate. Bear in mind that this file will
    be read verbatim, including all whitespace characters.
- `Listen`: List of endpoints which SlimGet will bind to, in order to listen for HTTP(S) connections.
  - `IpAddress`: IP address to listen on. Use `"0.0.0.0"` to listen on all available interfaces and
    addresses.
  - `Port`: TCP port number to listen on.
  - `UseSsl`: Whether this endpoint uses HTTPS. Setting this to `true` will enable HTTPS for this endpoint.
- `MaxRequestSizeBytes`: Maximum HTTP request size, in bytes. This should be greater than or equal to the value of
  `Storage.Packages.MaxPackageSizeBytes`.
- `TokenHmacKey`: Password to use for generating all API tokens. This value should be kept secret. If you're unsure
  about a good password, you can use `dd if=/dev/random bs=1024 count=1 2>/dev/null | sha256sum - | cut -d' ' -f1` on
  a Linux system to generate one.

### SSL certificates
SlimGet can use any certificate for SSL, be it self-signed or proper CA-issued one. If you need to generate a
self-signed certificate, you can use the following commands to do so:

```bash
echo -n "/C=US/ST=DC/L=Washington, D.C./O=White House/OU=NuGet Hosting/CN=nuget.example.com" > subject.txt
echo -n "hunter2" > certificate.pfx.pwd # replace hunter2 with your password, or...
# if you have no idea for a password, you can also do this
echo -n $(dd if=/dev/random bs=1024 count=1 2>/dev/null | sha256sum - | cut -d' ' -f1) > certificate.pfx.pwd

openssl req -x509 -days 3650 -subj "$(cat subject.txt)" -newkey rsa:4096 -keyout certificate-key.pem -out certificate.pem -nodes
openssl pkcs12 -export -in certificate.pem -inkey certificate-key.pem -out certificate.pfx -password "pass:$(cat certificate.pfx.pwd)"
rm certificate-key.pem certificate.pem subject.txt
```

Your certificate will be saved as `certificate.pfx`, and its password as `certificate.pfx.pwd`. Copy them somewhere
your server can access (perferably a secure medium, such as an encrypted filesystem).

## Docker
You can run SlimGet as a Docker container. For this purpose, a [prebuilt image](https://hub.docker.com/r/emzi0767/slimget)
is provided, allowing you to easily set up and deploy a SlimGet server. The image is `emzi0767/slimget:latest`. It
requires that you bind mount a directory for the packages, as well as the config file, and point the file via
SLIMGET_CONFIGURATION_FILE environment variable.

For example, to run SlimGet as a regular container, using `/app/feed` as the package storage location, and
`/app/slimget.json` mounted from `/mnt/slimget` as the config location, and certificate located in
`/app/certificates` mounted from `/mnt/slimget/certificates`:

```bash
docker run \
    --detach \
    --name slimget \
    --hostname slimget \
    --mount type=bind,src=/mnt/slimget/feed,dst=/app/feed \
    --mount type=bind,src=/mnt/slimget/certificates,dst=/app/certificates,readonly \
    --mount type=bind,src=/mnt/slimget/slimget.json,dst=/app/slimget.json,readonly \
    --env SLIMGET_CONFIGURATION_FILE=/app/slimget.json \
    --expose 443:5000 \
    --restart=always \
    emzi0767/slimget:latest
```

Above requires `Server.SslCertificate.Location` set to `"/app/certificates/certificate.pfx"` and
`Server.SslCertificate.PasswordFile` set to `"/app/certificates/certificate.pfx.pwd"` (assuming the corresponding files
are named like that), as well as `Storage.FileSystem.StoragePath` set to `"/app/feed"`.

You can also run SlimGet as a docker service, for example, using similar settings as above, except we use a docker
secret called `slimget-config` to host configuration, `slimget-certificate` to host the certificate, and
`slimget-certificate-password` to host certificate password, and attach the service to a network called
`service-overlay`:

```bash
docker service create \
    --replicas 1 \
    --name slimget \
    --mount type=bind,src=/mnt/slimget/feed,dst=/app/feed \
    --secret slimget-certificate \
    --secret slimget-certificate-password \
    --secret slimget-config \
    --env SLIMGET_CONFIGURATION_FILE=/run/secrets/slimget-config \
    --network service-overlay \
    --publish 443:5000 \
    emzi0767/slimget:latest
```

In this case, `Server.SslCertificate.Location` must be set to `"/run/secrets/slimget-certificate"`,
`Server.SslCertificate.PasswordFile` must be set to `"/run/secrets/slimget-certificate-password"`, and
`Storage.FileSystem.StoragePath` must be set to `"/app/feed"`.

It is possible to use Docker volumes instead of bind mounts for the feed storage. To use a volume called slimget-feed, 
simply replace `--mount type=bind,src=/mnt/slimget/feed,dst=/app/feed` with
`--mount type=volume,src=slimget-feed,dst=/app/feed`.

For more information about these subjects, refer to Docker documentation:

- [docker run](https://docs.docker.com/engine/reference/commandline/run/) - starts a container from given image.
- [docker service create](https://docs.docker.com/engine/reference/commandline/service_create/) - starts a service in
  swarm mode.
- [Manage sensisitve data with Docker secrets](https://docs.docker.com/engine/swarm/secrets/) - information about
  managing and using Docker secrets (available in swarm mode only).
- [Use volumes](https://docs.docker.com/storage/volumes/) - infromation about creating, managing, and using Docker
  volumes as means of persistent storage.
- [Use overlay networks](https://docs.docker.com/network/overlay/) - specifically the [Create overlay network](https://docs.docker.com/network/overlay/#create-an-overlay-network) 
  section, for communicating between services in a Docker swarm.

### Building Docker image
If you wish to build a SlimGet Docker image yourself, you can do so by doing `docker build .
--tag=emzi0767/slimget:latest` from the repository's root.

## Building
TODO

## Support me
Lots of effort went into making this, and sometimes even related software.

If you feel like I'm doing a good job, or just want to throw money at me, you can do so through any of the following:
- [Patreon](https://www.patreon.com/emzi0767)
- [PayPal](https://paypal.me/Emzi0767/5USD)

## Other questions
If you have other questions or would like to talk in general, feel free to visit my Discord server.

[![Emzi's Central Dispatch](https://discordapp.com/api/guilds/207879549394878464/embed.png?style=banner1)](https://discord.gg/rGKrJDR)
