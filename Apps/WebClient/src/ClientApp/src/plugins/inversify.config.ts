import "reflect-metadata";
import { Container } from "inversify";

import { DELEGATE_IDENTIFIER, SERVICE_IDENTIFIER } from "@/plugins/inversify";
import {
    ILogger,
    IAuthenticationService,
    IBetaRequestService,
    ICommunicationService,
    IConfigService,
    IHttpDelegate,
    IImmunizationService,
    ILaboratoryService,
    IMedicationService,
    IEncounterService,
    IPatientService,
    IUserCommentService,
    IUserFeedbackService,
    IUserNoteService,
    IUserProfileService,
    IUserRatingService,
} from "@/services/interfaces";
import HttpDelegate from "@/services/httpDelegate";
import { WinstonLogger } from "@/services/winstonLogger";
import { RestAuthenticationService } from "@/services/restAuthService";
import { RestImmunizationService } from "@/services/restImmunizationService";
import { RestConfigService } from "@/services/restConfigService";
import { RestPatientService } from "@/services/restPatientService";
import { RestMedicationService } from "@/services/restMedicationService";
import { RestEncounterService } from "@/services/mockRestEncounterService";
import { RestLaboratoryService } from "@/services/restLaboratoryService";
import { RestUserProfileService } from "@/services/restUserProfileService";
import { RestUserFeedbackService } from "@/services/restUserFeedback";
import { RestBetaRequestService } from "@/services/restBetaRequestService";
import { RestUserNoteService } from "@/services/restUserNoteService";
import { RestCommunicationService } from "@/services/restCommunicationService";
import { RestUserCommentService } from "@/services/restUserCommentService";
import { RestUserRatingService } from "@/services/restUserRatingService";

const container = new Container();
container
    .bind<IConfigService>(SERVICE_IDENTIFIER.ConfigService)
    .to(RestConfigService)
    .inSingletonScope();
container
    .bind<ILogger>(SERVICE_IDENTIFIER.Logger)
    .to(WinstonLogger)
    .inSingletonScope();
container
    .bind<IAuthenticationService>(SERVICE_IDENTIFIER.AuthenticationService)
    .to(RestAuthenticationService)
    .inSingletonScope();
container
    .bind<IImmunizationService>(SERVICE_IDENTIFIER.ImmunizationService)
    .to(RestImmunizationService)
    .inSingletonScope();
container
    .bind<IPatientService>(SERVICE_IDENTIFIER.PatientService)
    .to(RestPatientService)
    .inSingletonScope();
container
    .bind<IMedicationService>(SERVICE_IDENTIFIER.MedicationService)
    .to(RestMedicationService)
    .inSingletonScope();
container
    .bind<IEncounterService>(SERVICE_IDENTIFIER.EncounterService)
    .to(RestEncounterService)
    .inSingletonScope();
container
    .bind<ILaboratoryService>(SERVICE_IDENTIFIER.LaboratoryService)
    .to(RestLaboratoryService)
    .inSingletonScope();
container
    .bind<IUserProfileService>(SERVICE_IDENTIFIER.UserProfileService)
    .to(RestUserProfileService)
    .inSingletonScope();
container
    .bind<IUserFeedbackService>(SERVICE_IDENTIFIER.UserFeedbackService)
    .to(RestUserFeedbackService)
    .inSingletonScope();
container
    .bind<IBetaRequestService>(SERVICE_IDENTIFIER.BetaRequestService)
    .to(RestBetaRequestService)
    .inSingletonScope();
container
    .bind<IUserNoteService>(SERVICE_IDENTIFIER.UserNoteService)
    .to(RestUserNoteService)
    .inSingletonScope();
container
    .bind<ICommunicationService>(SERVICE_IDENTIFIER.CommunicationService)
    .to(RestCommunicationService)
    .inSingletonScope();
container
    .bind<IUserCommentService>(SERVICE_IDENTIFIER.UserCommentService)
    .to(RestUserCommentService)
    .inSingletonScope();
container
    .bind<IUserRatingService>(SERVICE_IDENTIFIER.UserRatingService)
    .to(RestUserRatingService)
    .inSingletonScope();
container
    .bind<IHttpDelegate>(DELEGATE_IDENTIFIER.HttpDelegate)
    .to(HttpDelegate)
    .inSingletonScope();

export default container;
