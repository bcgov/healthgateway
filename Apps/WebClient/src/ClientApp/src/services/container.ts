import { Platform } from "react-native";

import type { ExternalConfiguration } from "@/models/configData";

import type { IAuthService } from "./auth/authService";
import {
    ClinicalDocumentService,
    type IClinicalDocumentService,
} from "./clinicalDocument.service";
import {
    CommunicationService,
    type ICommunicationService,
} from "./communication.service";
import { ConfigService, type IConfigService } from "./config.service";
import { DependentService, type IDependentService } from "./dependent.service";
import { EncounterService, type IEncounterService } from "./encounter.service";
import {
    HospitalVisitService,
    type IHospitalVisitService,
} from "./hospitalVisit.service";
import { HttpClient, type IHttpClient } from "./http/httpClient";
import {
    type IImmunizationService,
    ImmunizationService,
} from "./immunization.service";
import {
    type ILaboratoryService,
    LaboratoryService,
} from "./laboratory.service";
import { logger, logLevelFromConfig } from "./logger";
import {
    type IMedicationService,
    MedicationService,
} from "./medication.service";
import {
    type INotificationService,
    NotificationService,
} from "./notification.service";
import { type IPatientService, PatientService } from "./patient.service";
import {
    type IPatientDataService,
    PatientDataService,
} from "./patientData.service";
import { type IReportService, ReportService } from "./report.service";
import {
    type ISpecialAuthorityService,
    SpecialAuthorityService,
} from "./specialAuthority.service";
import {
    createTrackingService,
    type ITrackingService,
    trackingConfigFromEnv,
} from "./tracking";
import {
    type IUserCommentService,
    UserCommentService,
} from "./userComment.service";
import {
    type IUserFeedbackService,
    UserFeedbackService,
} from "./userFeedback.service";
import { type IUserNoteService, UserNoteService } from "./userNote.service";
import {
    type IUserProfileService,
    UserProfileService,
} from "./userProfile.service";

/**
 * Minimal service locator (replaces the inversify container of the web client).
 * `http` and `config` exist from start-up; the rest are created once the configuration is loaded.
 */
class ServiceContainer {
    readonly http: IHttpClient = new HttpClient();
    readonly config: IConfigService = new ConfigService(this.http);
    /** Snowplow tracking. Web: from the app's own env configuration. Native: at start-up, from the selected environment. */
    readonly tracking: ITrackingService = (() => {
        const t = createTrackingService();
        if (Platform.OS === "web") t.init(trackingConfigFromEnv());
        return t;
    })();

    private _auth?: IAuthService;
    private _patient?: IPatientService;
    private _userProfile?: IUserProfileService;
    private _communication?: ICommunicationService;
    private _userFeedback?: IUserFeedbackService;
    private _immunization?: IImmunizationService;
    private _report?: IReportService;
    private _medication?: IMedicationService;
    private _laboratory?: ILaboratoryService;
    private _encounter?: IEncounterService;
    private _userNote?: IUserNoteService;
    private _specialAuthority?: ISpecialAuthorityService;
    private _clinicalDocument?: IClinicalDocumentService;
    private _hospitalVisit?: IHospitalVisitService;
    private _patientData?: IPatientDataService;
    private _userComment?: IUserCommentService;
    private _dependent?: IDependentService;
    private _notification?: INotificationService;

    initialize(config: ExternalConfiguration, auth: IAuthService) {
        logger.setLevel(logLevelFromConfig(config.webClient?.logLevel));
        this._auth = auth;
        this._patient = new PatientService(this.http, config);
        this._userProfile = new UserProfileService(this.http, config);
        this._communication = new CommunicationService(this.http, config);
        this._userFeedback = new UserFeedbackService(this.http, config);
        this._immunization = new ImmunizationService(this.http, config);
        this._report = new ReportService(this.http, config);
        this._medication = new MedicationService(this.http, config);
        this._laboratory = new LaboratoryService(this.http, config);
        this._encounter = new EncounterService(this.http, config);
        this._userNote = new UserNoteService(this.http, config);
        this._specialAuthority = new SpecialAuthorityService(this.http, config);
        this._clinicalDocument = new ClinicalDocumentService(this.http, config);
        this._hospitalVisit = new HospitalVisitService(this.http, config);
        this._patientData = new PatientDataService(this.http, config);
        this._userComment = new UserCommentService(this.http, config);
        this._dependent = new DependentService(this.http, config);
        this._notification = new NotificationService(this.http, config);
    }

    get isInitialized() {
        return this._auth !== undefined;
    }

    private require<T>(value: T | undefined, name: string): T {
        if (value === undefined)
            throw new Error(`Service "${name}" used before initialisation`);
        return value;
    }

    get auth() {
        return this.require(this._auth, "auth");
    }
    get patient() {
        return this.require(this._patient, "patient");
    }
    get userProfile() {
        return this.require(this._userProfile, "userProfile");
    }
    get communication() {
        return this.require(this._communication, "communication");
    }
    get userFeedback() {
        return this.require(this._userFeedback, "userFeedback");
    }
    get immunization() {
        return this.require(this._immunization, "immunization");
    }
    get report() {
        return this.require(this._report, "report");
    }
    get medication() {
        return this.require(this._medication, "medication");
    }
    get laboratory() {
        return this.require(this._laboratory, "laboratory");
    }
    get encounter() {
        return this.require(this._encounter, "encounter");
    }
    get userNote() {
        return this.require(this._userNote, "userNote");
    }
    get specialAuthority() {
        return this.require(this._specialAuthority, "specialAuthority");
    }
    get clinicalDocument() {
        return this.require(this._clinicalDocument, "clinicalDocument");
    }
    get hospitalVisit() {
        return this.require(this._hospitalVisit, "hospitalVisit");
    }
    get patientData() {
        return this.require(this._patientData, "patientData");
    }
    get userComment() {
        return this.require(this._userComment, "userComment");
    }
    get dependent() {
        return this.require(this._dependent, "dependent");
    }
    get notification() {
        return this.require(this._notification, "notification");
    }
}

export const services = new ServiceContainer();
