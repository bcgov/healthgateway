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
namespace HealthGateway.Admin.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using HealthGateway.Admin.Constants;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Wrapper;

    /// <inheritdoc />
    public class DashboardService : IDashboardService
    {
        private readonly IResourceDelegateDelegate dependentDelegate;
        private readonly IUserProfileDelegate userProfileDelegate;
        private readonly IMessagingVerificationDelegate messagingVerificationDelegate;
        private readonly IPatientService patientService;
        private readonly IRatingDelegate ratingDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="DashboardService"/> class.
        /// </summary>
        /// <param name="dependentDelegate">The dependent delegate to interact with the DB.</param>
        /// <param name="userProfileDelegate">The user profile delegate to interact with the DB.</param>
        /// <param name="messagingVerificationDelegate">The Messaging verification delegate to interact with the DB.</param>
        /// <param name="patientService">The patient service to lookup HDIDs by PHN.</param>
        /// <param name="ratingDelegate">The rating delegate.</param>
        public DashboardService(
            IResourceDelegateDelegate dependentDelegate,
            IUserProfileDelegate userProfileDelegate,
            IMessagingVerificationDelegate messagingVerificationDelegate,
            IPatientService patientService,
            IRatingDelegate ratingDelegate)
        {
            this.dependentDelegate = dependentDelegate;
            this.userProfileDelegate = userProfileDelegate;
            this.messagingVerificationDelegate = messagingVerificationDelegate;
            this.patientService = patientService;
            this.ratingDelegate = ratingDelegate;
        }

        /// <inheritdoc />
        public IDictionary<DateTime, int> GetDailyRegisteredUsersCount(int timeOffset)
        {
            // Javascript offset is positive # of minutes if the local timezone is behind UTC, and negative if it is ahead.
            TimeSpan ts = new(0, timeOffset, 0);
            return this.userProfileDelegate.GetDailyRegisteredUsersCount(ts);
        }

        /// <inheritdoc />
        public IDictionary<DateTime, int> GetDailyLoggedInUsersCount(int timeOffset)
        {
            // Javascript offset is positive # of minutes if the local timezone is behind UTC, and negative if it is ahead.
            TimeSpan ts = new(0, timeOffset, 0);
            return this.userProfileDelegate.GetDailyLoggedInUsersCount(ts);
        }

        /// <inheritdoc />
        public IDictionary<DateTime, int> GetDailyDependentCount(int timeOffset)
        {
            // Javascript offset is positive # of minutes if the local timezone is behind UTC, and negative if it is ahead.
            TimeSpan ts = new(0, timeOffset, 0);
            return this.dependentDelegate.GetDailyDependentCount(ts);
        }

        /// <inheritdoc />
        public int GetRecurrentUserCount(int dayCount, string startPeriod, string endPeriod, int timeOffset)
        {
            TimeSpan ts = new(0, GetOffset(timeOffset), 0);
            DateTime startDate = DateTime.Parse(startPeriod, CultureInfo.InvariantCulture).Date.Add(ts);
            startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            DateTime endDate = DateTime.Parse(endPeriod, CultureInfo.InvariantCulture).Date.Add(ts).AddDays(1).AddMilliseconds(-1);
            endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);
            return this.userProfileDelegate.GetRecurrentUserCount(dayCount, startDate, endDate);
        }

        /// <inheritdoc />
        public RequestResult<IEnumerable<MessagingVerificationModel>> GetMessageVerifications(UserQueryType queryType, string queryString)
        {
            RequestResult<IEnumerable<MessagingVerificationModel>> retVal = new()
            {
                ResultStatus = ResultType.Error,
                ResourcePayload = Enumerable.Empty<MessagingVerificationModel>(),
            };

            DBResult<IEnumerable<MessagingVerification>>? dbResult = null;
            string phn = string.Empty;
            switch (queryType)
            {
                case UserQueryType.PHN:
                    RequestResult<PatientModel> patientResult = Task.Run(async () => await this.patientService.GetPatient(queryString, PatientIdentifierType.PHN).ConfigureAwait(true)).Result;
                    if (patientResult.ResultStatus == ResultType.Success && patientResult.ResourcePayload != null)
                    {
                        string hdid = patientResult.ResourcePayload.HdId;
                        phn = patientResult.ResourcePayload.PersonalHealthNumber;
                        dbResult = this.messagingVerificationDelegate.GetUserMessageVerifications(Database.Constants.UserQueryType.HDID, hdid);
                    }
                    else
                    {
                        retVal.ResultError = patientResult.ResultError;
                    }

                    break;
                case UserQueryType.Email:
                    dbResult = this.messagingVerificationDelegate.GetUserMessageVerifications(Database.Constants.UserQueryType.Email, queryString);
                    break;
                case UserQueryType.SMS:
                    dbResult = this.messagingVerificationDelegate.GetUserMessageVerifications(Database.Constants.UserQueryType.SMS, queryString);
                    break;
                case UserQueryType.HDID:
                    RequestResult<PatientModel> patientResultHdid = Task.Run(async () => await this.patientService.GetPatient(queryString).ConfigureAwait(true)).Result;
                    if (patientResultHdid.ResultStatus == ResultType.Success && patientResultHdid.ResourcePayload != null)
                    {
                        phn = patientResultHdid.ResourcePayload.PersonalHealthNumber;
                        dbResult = this.messagingVerificationDelegate.GetUserMessageVerifications(Database.Constants.UserQueryType.HDID, queryString);
                    }
                    else
                    {
                        retVal.ResultError = patientResultHdid.ResultError;
                    }

                    break;
            }

            if (dbResult != null && dbResult.Status == Database.Constants.DBStatusCode.Read)
            {
                retVal.ResultStatus = ResultType.Success;
                List<MessagingVerificationModel> results = new();
                if (dbResult.Payload != null)
                {
                    results.AddRange(dbResult.Payload.Select(MessagingVerificationModel.CreateFromDbModel));
                    if (queryType == UserQueryType.HDID || queryType == UserQueryType.PHN)
                    {
                        foreach (var item in results)
                        {
                            item.PersonalHealthNumber = phn;
                        }
                    }

                    retVal.ResourcePayload = results;
                }
            }

            return retVal;
        }

        /// <inheritdoc />
        public IDictionary<string, int> GetRatingSummary(string startPeriod, string endPeriod, int timeOffset)
        {
            TimeSpan ts = new(0, GetOffset(timeOffset), 0);
            DateTime startDate = DateTime.Parse(startPeriod, CultureInfo.InvariantCulture).Date.Add(ts);
            startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            DateTime endDate = DateTime.Parse(endPeriod, CultureInfo.InvariantCulture).Date.Add(ts).AddDays(1).AddMilliseconds(-1);
            endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);
            return this.ratingDelegate.GetSummary(startDate, endDate);
        }

        /// <summary>
        /// Returns an offset value that can be used to create a date in UTC.
        /// </summary>
        /// <param name="timeOffset">The offset from the client browser to UTC.</param>
        /// <returns>The offset value used to create UTC.</returns>
        private static int GetOffset(int timeOffset)
        {
            // If timeOffset is a negative value, then it means current timezone is [n] minutes behind UTC so we need to change this value to a positive when creating TimeSpan for DateTime object in UTC.
            // If timeOffset is a positive value, then it means current timezone is [n] minutes ahead of UTC so we need to change this value to a negative when creating TimeSpan for DateTime object in UTC.
            // If timeOffset is 0, then it means current timezone is UTC.
            return timeOffset * -1;
        }
    }
}
