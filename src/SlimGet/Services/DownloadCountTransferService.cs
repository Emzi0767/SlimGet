// This file is a part of SlimGet project.
//
// Copyright 2019 Emzi0767
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SlimGet.Data;

namespace SlimGet.Services
{
    public sealed class DownloadCountTransferService : IDisposable
    {
        private static TimeSpan Interval { get; } = TimeSpan.FromHours(1);

        private ILogger<DownloadCountTransferService> Logger { get; }
        private IServiceProvider Services { get; }
        private Task CurrentOperation { get; set; }
        private CancellationTokenSource CancelTokenSource { get; }
        private CancellationToken CancelToken => this.CancelTokenSource.Token;

        private bool _isDisposed = false;

        public DownloadCountTransferService(ILoggerFactory logger, IServiceProvider services)
        {
            this.Logger = logger.CreateLogger<DownloadCountTransferService>();
            this.Services = services;
            this.CancelTokenSource = new CancellationTokenSource();

            this.ScheduleNextTransfer(TimeSpan.FromSeconds(15));
        }

        private void ScheduleNextTransfer(TimeSpan timeout)
        {
            this.Logger.LogInformation("Scheduling transfer in {0:g}", timeout);
            this.CurrentOperation = Task.Delay(timeout, this.CancelToken)
                .ContinueWith(this.TaskCompletionDelegate, TaskContinuationOptions.OnlyOnRanToCompletion)
                .ContinueWith(this.TaskFailDelegate, TaskContinuationOptions.NotOnRanToCompletion);
        }

        public void Dispose()
        {
            if (this._isDisposed)
                return;

            this._isDisposed = true;
            this.CancelTokenSource.Cancel();
            this.CancelTokenSource.Dispose();
        }

        private async Task PerformTranferAsync()
        {
            var db = this.Services.GetRequiredService<SlimGetContext>();
            var redis = this.Services.GetRequiredService<RedisService>();

            var dbpackages = db.Packages
                .Include(x => x.Versions);
            foreach (var dbpackage in dbpackages)
            {
                var dbpinfo = new PackageInfo(dbpackage.Id, null);
                dbpackage.DownloadCount = await redis.GetPackageDownloadCountAsync(dbpinfo).ConfigureAwait(false);
                db.Packages.Update(dbpackage);

                foreach (var dbversion in dbpackage.Versions)
                {
                    var dbvinfo = new PackageInfo(dbpackage.Id, dbversion.NuGetVersion);
                    dbversion.DownloadCount = await redis.GetVersionDownloadCountAsync(dbvinfo).ConfigureAwait(false);
                    db.PackageVersions.Update(dbversion);
                }
            }

            await db.SaveChangesAsync();
        }

        private void TaskCompletionDelegate(Task t)
        {
            this.Logger.LogInformation("Scheduled timeout ran to completion, performing transfer and scheduling another");

            _ = this.PerformTranferAsync();
            this.ScheduleNextTransfer(Interval);
        }

        private void TaskFailDelegate(Task t)
        {
            this.Logger.LogWarning("Scheduled timeout failed, attempting another schedule");

            if (!t.IsCanceled && !this._isDisposed)
                this.ScheduleNextTransfer(Interval);
        }
    }
}
