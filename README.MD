# SlimGet
SlimGet is a lightweight implementation of a NuGet and symbol server, powered by ASP.NET Core 2.2, designed to be ran
in Docker. It's the software powering my NuGet feed at [nuget.emzi0767.com](https://nuget.emzi0767.com/).

Unlike alternative implementations, SlimGet is designed to host a single, local feed, with no upstream mirrors or
similar functionality. It's designed only with .NET packages (and debug symbols) in mind, and is therefore not
guaranteed to work with anything else (e.g. COM interop, native libraries, etc.).

The project was born out of need for a NuGet feed for CI artifacts of my projects, after my usualy go-to provider
ceased responding to all my support emails, and broke my builds in the process.

The web interface provides a simple, readonly view of the feed, which allows for browsing and downloading hosted
packages, as well as viewing various metadata about them.

Provided are tools, which allow for issuing and revoking API tokens, allowing users to upload packages, as well as
manage packages in a limited fashion.

In the future, I might provide more integrated means of managing users and packages.

## Requirements
In order to run a SlimGet server, you need the following components installed and available on your system:
- .NET Core 2.2 runtime with ASP.NET Core installed
- PostgreSQL server 9.6 or better
- Redis server

## Usage
TODO

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
