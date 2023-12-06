import { container } from "@/ioc/container";
import { DELEGATE_IDENTIFIER, SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { HttpDelegate } from "@/services/httpDelegate";
import {
    IAuthenticationService,
    IClinicalDocumentService,
    ICommunicationService,
    IConfigService,
    IDependentService,
    IEncounterService,
    IHospitalVisitService,
    IHttpDelegate,
    IImmunizationService,
    ILaboratoryService,
    ILogger,
    IMedicationService,
    INotificationService,
    IPatientDataService,
    IPatientService,
    IPcrTestService,
    IReportService,
    ISpecialAuthorityService,
    ITrackingService,
    IUserCommentService,
    IUserFeedbackService,
    IUserNoteService,
    IUserProfileService,
    IUserRatingService,
    IVaccinationStatusService,
} from "@/services/interfaces";
import { LoglevelLogger } from "@/services/loglevelLogger";
import { RestAuthenticationService } from "@/services/restAuthService";
import { RestClinicalDocumentService } from "@/services/restClinicalDocumentService";
import { RestCommunicationService } from "@/services/restCommunicationService";
import { RestConfigService } from "@/services/restConfigService";
import { RestDependentService } from "@/services/restDependentService";
import { RestEncounterService } from "@/services/restEncounterService";
import { RestHospitalVisitService } from "@/services/restHospitalVisitService";
import { RestImmunizationService } from "@/services/restImmunizationService";
import { RestLaboratoryService } from "@/services/restLaboratoryService";
import { RestMedicationService } from "@/services/restMedicationService";
import { RestNotificationService } from "@/services/restNotificationService";
import { RestPatientDataService } from "@/services/restPatientDataService";
import { RestPatientService } from "@/services/restPatientService";
import { RestPcrTestService } from "@/services/restPcrTestService";
import { RestReportService } from "@/services/restReportService";
import { RestSpecialAuthorityService } from "@/services/restSpecialAuthorityService";
import { RestTrackingService } from "@/services/restTrackingService";
import { RestUserCommentService } from "@/services/restUserCommentService";
import { RestUserFeedbackService } from "@/services/restUserFeedback";
import { RestUserNoteService } from "@/services/restUserNoteService";
import { RestUserProfileService } from "@/services/restUserProfileService";
import { RestUserRatingService } from "@/services/restUserRatingService";
import { RestVaccinationStatusService } from "@/services/restVaccinationStatusService";
import { useConfigStore } from "@/stores/config";

export async function initializeServices(): Promise<void> {
    const configStore = useConfigStore();

    container.set<ILogger>(
        SERVICE_IDENTIFIER.Logger,
        () => new LoglevelLogger(configStore.webConfig?.logLevel?.toLowerCase())
    );

    container.set<IHttpDelegate>(
        DELEGATE_IDENTIFIER.HttpDelegate,
        (c) => new HttpDelegate(c.get<ILogger>(SERVICE_IDENTIFIER.Logger))
    );

    container.set<IConfigService>(
        SERVICE_IDENTIFIER.ConfigService,
        (c) =>
            new RestConfigService(
                c.get<ILogger>(SERVICE_IDENTIFIER.Logger),
                c.get<IHttpDelegate>(DELEGATE_IDENTIFIER.HttpDelegate)
            )
    );

    const authenticationService = await RestAuthenticationService.GetService(
        container.get<ILogger>(SERVICE_IDENTIFIER.Logger),
        configStore.openIdConnect
    );
    container.set<IAuthenticationService>(
        SERVICE_IDENTIFIER.AuthenticationService,
        () => authenticationService
    );

    container.set<IClinicalDocumentService>(
        SERVICE_IDENTIFIER.ClinicalDocumentService,
        (c) =>
            new RestClinicalDocumentService(
                c.get<ILogger>(SERVICE_IDENTIFIER.Logger),
                c.get<IHttpDelegate>(DELEGATE_IDENTIFIER.HttpDelegate),
                configStore.config
            )
    );

    container.set<ICommunicationService>(
        SERVICE_IDENTIFIER.CommunicationService,
        (c) =>
            new RestCommunicationService(
                c.get<ILogger>(SERVICE_IDENTIFIER.Logger),
                c.get<IHttpDelegate>(DELEGATE_IDENTIFIER.HttpDelegate),
                configStore.config
            )
    );

    container.set<IDependentService>(
        SERVICE_IDENTIFIER.DependentService,
        (c) =>
            new RestDependentService(
                c.get<ILogger>(SERVICE_IDENTIFIER.Logger),
                c.get<IHttpDelegate>(DELEGATE_IDENTIFIER.HttpDelegate),
                configStore.config
            )
    );

    container.set<IEncounterService>(
        SERVICE_IDENTIFIER.EncounterService,
        (c) =>
            new RestEncounterService(
                c.get<ILogger>(SERVICE_IDENTIFIER.Logger),
                c.get<IHttpDelegate>(DELEGATE_IDENTIFIER.HttpDelegate),
                configStore.config
            )
    );

    container.set<IHospitalVisitService>(
        SERVICE_IDENTIFIER.HospitalVisitService,
        (c) =>
            new RestHospitalVisitService(
                c.get<ILogger>(SERVICE_IDENTIFIER.Logger),
                c.get<IHttpDelegate>(DELEGATE_IDENTIFIER.HttpDelegate),
                configStore.config
            )
    );

    container.set<IImmunizationService>(
        SERVICE_IDENTIFIER.ImmunizationService,
        (c) =>
            new RestImmunizationService(
                c.get<ILogger>(SERVICE_IDENTIFIER.Logger),
                c.get<IHttpDelegate>(DELEGATE_IDENTIFIER.HttpDelegate),
                configStore.config
            )
    );

    container.set<ILaboratoryService>(
        SERVICE_IDENTIFIER.LaboratoryService,
        (c) =>
            new RestLaboratoryService(
                c.get<ILogger>(SERVICE_IDENTIFIER.Logger),
                c.get<IHttpDelegate>(DELEGATE_IDENTIFIER.HttpDelegate),
                configStore.config
            )
    );

    container.set<IMedicationService>(
        SERVICE_IDENTIFIER.MedicationService,
        (c) =>
            new RestMedicationService(
                c.get<ILogger>(SERVICE_IDENTIFIER.Logger),
                c.get<IHttpDelegate>(DELEGATE_IDENTIFIER.HttpDelegate),
                configStore.config
            )
    );

    container.set<INotificationService>(
        SERVICE_IDENTIFIER.NotificationService,
        (c) =>
            new RestNotificationService(
                c.get<ILogger>(SERVICE_IDENTIFIER.Logger),
                c.get<IHttpDelegate>(DELEGATE_IDENTIFIER.HttpDelegate),
                configStore.config
            )
    );

    container.set<IPatientDataService>(
        SERVICE_IDENTIFIER.PatientDataService,
        (c) =>
            new RestPatientDataService(
                c.get<ILogger>(SERVICE_IDENTIFIER.Logger),
                c.get<IHttpDelegate>(DELEGATE_IDENTIFIER.HttpDelegate),
                configStore.config
            )
    );

    container.set<IPatientService>(
        SERVICE_IDENTIFIER.PatientService,
        (c) =>
            new RestPatientService(
                c.get<ILogger>(SERVICE_IDENTIFIER.Logger),
                c.get<IHttpDelegate>(DELEGATE_IDENTIFIER.HttpDelegate),
                configStore.config
            )
    );

    container.set<IPcrTestService>(
        SERVICE_IDENTIFIER.PcrTestService,
        (c) =>
            new RestPcrTestService(
                c.get<ILogger>(SERVICE_IDENTIFIER.Logger),
                c.get<IHttpDelegate>(DELEGATE_IDENTIFIER.HttpDelegate),
                configStore.config
            )
    );

    container.set<IReportService>(
        SERVICE_IDENTIFIER.ReportService,
        (c) =>
            new RestReportService(
                c.get<ILogger>(SERVICE_IDENTIFIER.Logger),
                c.get<IHttpDelegate>(DELEGATE_IDENTIFIER.HttpDelegate),
                configStore.config
            )
    );

    container.set<ISpecialAuthorityService>(
        SERVICE_IDENTIFIER.SpecialAuthorityService,
        (c) =>
            new RestSpecialAuthorityService(
                c.get<ILogger>(SERVICE_IDENTIFIER.Logger),
                c.get<IHttpDelegate>(DELEGATE_IDENTIFIER.HttpDelegate),
                configStore.config
            )
    );

    container.set<ITrackingService>(
        SERVICE_IDENTIFIER.TrackingService,
        (c) =>
            new RestTrackingService(c.get<ILogger>(SERVICE_IDENTIFIER.Logger))
    );

    container.set<IUserCommentService>(
        SERVICE_IDENTIFIER.UserCommentService,
        (c) =>
            new RestUserCommentService(
                c.get<ILogger>(SERVICE_IDENTIFIER.Logger),
                c.get<IHttpDelegate>(DELEGATE_IDENTIFIER.HttpDelegate),
                configStore.config
            )
    );

    container.set<IUserFeedbackService>(
        SERVICE_IDENTIFIER.UserFeedbackService,
        (c) =>
            new RestUserFeedbackService(
                c.get<ILogger>(SERVICE_IDENTIFIER.Logger),
                c.get<IHttpDelegate>(DELEGATE_IDENTIFIER.HttpDelegate),
                configStore.config
            )
    );

    container.set<IUserNoteService>(
        SERVICE_IDENTIFIER.UserNoteService,
        (c) =>
            new RestUserNoteService(
                c.get<ILogger>(SERVICE_IDENTIFIER.Logger),
                c.get<IHttpDelegate>(DELEGATE_IDENTIFIER.HttpDelegate),
                configStore.config
            )
    );

    container.set<IUserProfileService>(
        SERVICE_IDENTIFIER.UserProfileService,
        (c) =>
            new RestUserProfileService(
                c.get<ILogger>(SERVICE_IDENTIFIER.Logger),
                c.get<IHttpDelegate>(DELEGATE_IDENTIFIER.HttpDelegate),
                configStore.config
            )
    );

    container.set<IUserRatingService>(
        SERVICE_IDENTIFIER.UserRatingService,
        (c) =>
            new RestUserRatingService(
                c.get<ILogger>(SERVICE_IDENTIFIER.Logger),
                c.get<IHttpDelegate>(DELEGATE_IDENTIFIER.HttpDelegate),
                configStore.config
            )
    );

    container.set<IVaccinationStatusService>(
        SERVICE_IDENTIFIER.VaccinationStatusService,
        (c) =>
            new RestVaccinationStatusService(
                c.get<ILogger>(SERVICE_IDENTIFIER.Logger),
                c.get<IHttpDelegate>(DELEGATE_IDENTIFIER.HttpDelegate),
                configStore.config
            )
    );
}
