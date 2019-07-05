# This file is a part of SlimGet project.
# 
# Copyright 2019 Emzi0767
# 
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
# 
#   http://www.apache.org/licenses/LICENSE-2.0
#   
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

# Set up build environment
FROM mcr.microsoft.com/dotnet/core/sdk:2.2-alpine3.9 AS build
WORKDIR /app

# Copy all source files to build container
COPY . ./

# Build the application
RUN dotnet build -c Release -f netcoreapp2.2 src/SlimGet/SlimGet.csproj && \
    dotnet build -c Release -f netcoreapp2.2 src/SlimGet.TokenManager/SlimGet.TokenManager.csproj && \
    dotnet publish -c Release -f netcoreapp2.2 -o /app/slimget-publish

# Set up runtime environment
FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-alpine3.9
WORKDIR /app
COPY --from=build /app/slimget-publish .

# Set up storage
RUN mkdir feed
VOLUME [ "/app/feed" ]

# Run the application
ENTRYPOINT [ "dotnet" ]
CMD [ "SlimGet.dll" ]
