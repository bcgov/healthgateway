// -------------------------------------------------------------------------
//  Copyright © 2019 Province of British Columbia
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// -------------------------------------------------------------------------
namespace HealthGateway.JobScheduler.Utils
{
    using System;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Hangfire;
    using HealthGateway.Common.Data.Utils;
    using HealthGateway.DBMaintainer.Apps;
    using HealthGateway.JobScheduler.Models;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Utility class to streamline Scheduling jobs.
    /// </summary>
    public static class SchedulerHelper
    {
        /// <summary>
        /// Schedules an asynchronous Drug Load job, looking up the cron schedule and the job id from configuration.
        /// </summary>
        /// <typeparam name="T">The class of Drug Load Program to schedule.</typeparam>
        /// <param name="cfg">The Configuration to use.</param>
        /// <param name="key">The key to lookup Job configuration.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        public static void ScheduleDrugLoadJobAsync<T>(IConfiguration cfg, string key, CancellationToken ct = default)
            where T : IDrugApp
        {
            JobConfiguration jc = GetJobConfiguration(cfg, key);
            ScheduleJob<T>(jc, DateFormatter.GetLocalTimeZone(cfg), j => j.ProcessAsync(key, ct));
        }

        /// <summary>
        /// Schedules an async job, looking up the cron schedule and the job id from configuration.
        /// </summary>
        /// <typeparam name="T">The class of program to schedule.</typeparam>
        /// <param name="cfg">The Configuration to use.</param>
        /// <param name="key">The key to lookup Job configuration.</param>
        /// <param name="methodCall">The async expression to run on the class.</param>
        public static void ScheduleJobAsync<T>(IConfiguration cfg, string key, Expression<Func<T, Task>> methodCall)
        {
            JobConfiguration jc = GetJobConfiguration(cfg, key);
            ScheduleJobAsync(jc, DateFormatter.GetLocalTimeZone(cfg), methodCall);
        }

        /// <summary>
        /// Schedules a generic job, according to the Job Configuration.
        /// </summary>
        /// <typeparam name="T">The class of Drug Load Program to schedule.</typeparam>
        /// <param name="cfg">The Job Configuration to use.</param>
        /// <param name="tz">The timezone to schedule the job in.</param>
        /// <param name="methodCall">The expression to run on the class.</param>
        private static void ScheduleJob<T>(JobConfiguration cfg, TimeZoneInfo tz, Expression<Action<T>> methodCall)
        {
            RecurringJobOptions recurringJobOptions = new()
            {
                TimeZone = tz,
                MisfireHandling = MisfireHandlingMode.Relaxed,
            };

            RecurringJob.AddOrUpdate(cfg.Id, methodCall, cfg.Schedule, recurringJobOptions);
            if (cfg.Immediate)
            {
                BackgroundJob.Schedule(methodCall, TimeSpan.FromSeconds(cfg.Delay));
            }
        }

        /// <summary>
        /// Schedules a generic async job, according to the Job Configuration.
        /// </summary>
        /// <typeparam name="T">The class of program to schedule.</typeparam>
        /// <param name="cfg">The Job Configuration to use.</param>
        /// <param name="tz">The timezone to schedule the job in.</param>
        /// <param name="methodCall">The async expression to run on the class.</param>
        private static void ScheduleJobAsync<T>(JobConfiguration cfg, TimeZoneInfo tz, Expression<Func<T, Task>> methodCall)
        {
            RecurringJobOptions recurringJobOptions = new()
            {
                TimeZone = tz,
                MisfireHandling = MisfireHandlingMode.Relaxed,
            };

            RecurringJob.AddOrUpdate(cfg.Id, methodCall, cfg.Schedule, recurringJobOptions);

            if (cfg.Immediate)
            {
                BackgroundJob.Schedule(methodCall, TimeSpan.FromSeconds(cfg.Delay));
            }
        }

        private static JobConfiguration GetJobConfiguration(IConfiguration cfg, string key)
        {
            return cfg.GetRequiredSection(key).Get<JobConfiguration>()!;
        }
    }
}
