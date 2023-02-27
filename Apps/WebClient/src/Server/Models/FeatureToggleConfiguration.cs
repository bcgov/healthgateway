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
namespace HealthGateway.WebClient.Server.Models
{
#pragma warning disable CA1819

    /// <summary>
    /// Configuration data to be used by Health Gateway web client.
    /// </summary>
    /// <param name="Homepage">Settings for the home page.</param>
    /// <param name="WaitingQueue">Settings for the waiting queue.</param>
    /// <param name="NotificationCentre">Settings for the notification centre.</param>
    /// <param name="Timeline">Settings for the timeline.</param>
    /// <param name="Datasets">Settings for the data sets.</param>
    /// <param name="Covid19">Settings for covid19 features.</param>
    /// <param name="Dependents">Settings for dependents features.</param>
    public record FeatureToggleConfiguration(
        HomepageSettings Homepage,
        WaitingQueueSettings WaitingQueue,
        NotificationCentreSettings NotificationCentre,
        TimelineSettings Timeline,
        DatasetSettings[] Datasets,
        Covid19Settings Covid19,
        DependentsSettings Dependents,
        ServicesSettings Services);

    /// <summary>
    /// Settings for the home page.
    /// </summary>
    /// <param name="ShowFederalProofOfVaccination">Toggles federal proof of vaccination.</param>
    public record HomepageSettings(
        bool ShowFederalProofOfVaccination);

    /// <summary>
    /// Settings for the waiting queue.
    /// </summary>
    /// <param name="Enabled">Toggles the waiting queue feature.</param>
    public record WaitingQueueSettings(
        bool Enabled);

    /// <summary>
    /// Settings for the notification centre.
    /// </summary>
    /// <param name="Enabled">Toggles notification centre.</param>
    public record NotificationCentreSettings(
        bool Enabled);

    /// <summary>
    /// Settings for the timeline.
    /// </summary>
    /// <param name="Comment">Toggles the comment feature.</param>
    public record TimelineSettings(
        bool Comment);

    /// <summary>
    /// Settings for the data sets.
    /// </summary>
    /// <param name="Name">Name of the data set.</param>
    /// <param name="Enabled">Toggles the data set.</param>
    public record DatasetSettings(
        string Name,
        bool Enabled);

    /// <summary>
    /// Settings for covid19 features.
    /// </summary>
    /// <param name="PcrTestEnabled">Toggles pcr test.</param>
    /// <param name="PublicCovid19">Settings for public covid19 feature.</param>
    /// <param name="ProofOfVaccination">Settings for proof of vaccination feature.</param>
    public record Covid19Settings(
        bool PcrTestEnabled,
        PublicCovid19Settings PublicCovid19,
        ProofOfVaccinationSettings ProofOfVaccination);

    /// <summary>
    /// Settings for public covid19 feature.
    /// </summary>
    /// <param name="EnableTestResults">Toggles test results.</param>
    /// <param name="ShowFederalProofOfVaccination">Toggles federal proof of vaccination.</param>
    public record PublicCovid19Settings(
        bool EnableTestResults,
        bool ShowFederalProofOfVaccination);

    /// <summary>
    /// Settings for proof of vaccination feature.
    /// </summary>
    /// <param name="ExportPdf">Toggles export of pdf feature.</param>
    public record ProofOfVaccinationSettings(
        bool ExportPdf);

    /// <summary>
    /// Settings for dependents features.
    /// </summary>
    /// <param name="Enabled">Toggles dependents features.</param>
    /// <param name="Datasets">Settings for dependents data sets.</param>
    public record DependentsSettings(
        bool Enabled,
        DatasetSettings[] Datasets);

    /// <summary>
    /// Settings for the services feature.
    /// </summary>
    /// <param name="Enabled">Toggles services feature.</param>
    public record ServicesSettings(bool Enabled);

#pragma warning restore CA1819
}
