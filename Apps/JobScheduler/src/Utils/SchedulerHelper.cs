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
        /// Schedules a Drug Load job, looking up the cron schedule and the jobid from configuration.
        /// </summary>
        /// <typeparam name="T">The class of Drug Load Program to schedule.</typeparam>
        /// <param name="cfg">The Configuration to use.</param>
        /// <param name="key">The key to lookup Job configuration.</param>
        public static void ScheduleDrugLoadJob<T>(IConfiguration cfg, string key)
            where T : IDrugApp
        {
            JobConfiguration jc = GetJobConfiguration(cfg, key);
            ScheduleJob<T>(jc, DateFormatter.GetLocalTimeZone(), j => j.Process(key));
        }

        /// <summary>
        /// Schedules a Drug Load job, looking up the cron schedule and the jobid from configuration.
        /// </summary>
        /// <typeparam name="T">The class of Drug Load Program to schedule.</typeparam>
        /// <param name="cfg">The Configuration to use.</param>
        /// <param name="key">The key to lookup Job configuration.</param>
        /// <param name="methodCall">The expression to run on the class.</param>
        public static void ScheduleJob<T>(IConfiguration cfg, string key, Expression<Action<T>> methodCall)
        {
            JobConfiguration jc = GetJobConfiguration(cfg, key);
            ScheduleJob(jc, DateFormatter.GetLocalTimeZone(), methodCall);
        }

        /// <summary>
        /// Schedules a generic job, according to the Job Configuration.
        /// </summary>
        /// <typeparam name="T">The class of Drug Load Program to schedule.</typeparam>
        /// <param name="cfg">The Job Configuration to use.</param>
        /// <param name="tz">The timezone to schedule the job in.</param>
        /// <param name="methodCall">The expression to run on the class.</param>
        public static void ScheduleJob<T>(JobConfiguration cfg, TimeZoneInfo tz, Expression<Action<T>> methodCall)
        {
            RecurringJob.AddOrUpdate(cfg.Id, methodCall, cfg.Schedule, tz);
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
