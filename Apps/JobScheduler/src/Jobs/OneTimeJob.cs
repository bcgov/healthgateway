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
namespace HealthGateway.JobScheduler.Jobs
{
    using System;
    using Hangfire;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.JobScheduler.Tasks;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The OneTimeJob will run arbitrary IOneTimeTask (configurable) once.
    /// </summary>
    public class OneTimeJob
    {
        private const string JobKey = "OneTime";
        private const string OneTimeClassKey = "TaskClass";
        private const int ConcurrencyTimeout = 5 * 60; // 5 Minutes
        private readonly IApplicationSettingsDelegate applicationSettingsDelegate;

        private readonly GatewayDbContext dbContext;
        private readonly IConfiguration jobConfig;
        private readonly ILogger<OneTimeJob> logger;

        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="OneTimeJob"/> class.
        /// </summary>
        /// <param name="serviceProvider">The dotnet service provider.</param>
        /// <param name="configuration">The configuration to use.</param>
        /// <param name="logger">The logger to use.</param>
        /// <param name="dbContext">The db context to use.</param>
        /// <param name="applicationSettingsDelegate">The job settings in the DB.</param>
        public OneTimeJob(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            ILogger<OneTimeJob> logger,
            GatewayDbContext dbContext,
            IApplicationSettingsDelegate applicationSettingsDelegate)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
            this.dbContext = dbContext;
            this.applicationSettingsDelegate = applicationSettingsDelegate;
            this.jobConfig = configuration.GetSection($"{JobKey}");
        }

        /// <summary>
        /// Reads the configuration and will instantiate and run the class a single time.
        /// </summary>
        [DisableConcurrentExecution(ConcurrencyTimeout)]
        public void Process()
        {
            this.logger.LogInformation("OneTimeJob Starting");
            string? className = this.jobConfig.GetValue<string>(OneTimeClassKey);
            if (className != null)
            {
                Type? taskType = Type.GetType(className);
                if (taskType != null)
                {
                    this.logger.LogInformation("OneTimeJob will invoke {Name}", taskType.Name);
                    ApplicationSetting? hasRunAppSetting = this.applicationSettingsDelegate.GetApplicationSetting(
                        ApplicationType.JobScheduler,
                        this.GetType().Name,
                        className);
                    if (hasRunAppSetting == null)
                    {
                        this.logger.LogInformation("OneTimeJob is invoking {ClassName}", className);
                        Type? type = Type.GetType(className);
                        IOneTimeTask task = (IOneTimeTask)ActivatorUtilities.CreateInstance(this.serviceProvider, type);
                        task.Run();
                        this.logger.LogInformation("OneTimeJob is marking class {Name} as invoked", taskType.Name);
                        hasRunAppSetting = new ApplicationSetting
                        {
                            Application = ApplicationType.JobScheduler,
                            Component = this.GetType().Name,
                            Key = className,
                            Value = true.ToString(),
                        };
                        this.applicationSettingsDelegate.AddApplicationSetting(hasRunAppSetting);
                        this.logger.LogInformation("OneTimeJob is commiting DB changes");
                        this.dbContext.SaveChanges();
                    }
                    else
                    {
                        this.logger.LogInformation("OneTimeJob has invoked {Name} before and will exit", taskType.Name);
                    }
                }
                else
                {
                    throw new TypeLoadException($"Unable to find Task Type {className}");
                }
            }
            else
            {
                this.logger.LogInformation("OneTime job is not configured to run anything");
            }

            this.logger.LogInformation("OneTimeJob Finished running");
        }
    }
}
