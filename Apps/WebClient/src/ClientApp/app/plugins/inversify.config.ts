import "reflect-metadata";
import { Container } from "inversify";

import { SERVICE_IDENTIFIER, DELEGATE_IDENTIFIER } from "@/plugins/inversify";
import {
  IAuthenticationService,
  IImmunizationService,
  IConfigService,
  IHttpDelegate,
  IPatientService,
  IMedicationService,
  IUserProfileService,
  IUserFeedbackService,
  IUserEmailService,
  IBetaRequestService,
  IUserNoteService,
  ICommunicationService
} from "@/services/interfaces";
import HttpDelegate from "@/services/httpDelegate";
import { RestAuthenticationService } from "@/services/restAuthService";
import { RestImmunizationService } from "@/services/restImmunizationService";
import { RestConfigService } from "@/services/restConfigService";
import { RestPatientService } from "@/services/restPatientService";
import { RestMedicationService } from "@/services/restMedicationService";
import { RestUserProfileService } from "@/services/restUserProfileService";
import { RestUserFeedbackService } from "@/services/restUserFeedback";
import { RestUserEmailService } from "@/services/restUserEmailService";
import { RestBetaRequestService } from "@/services/restBetaRequestService";
import { RestUserNoteService } from "@/services/restUserNoteService";
import { RestCommunicationService } from "@/services/restCommunicationService";

let container = new Container();
container
  .bind<IConfigService>(SERVICE_IDENTIFIER.ConfigService)
  .to(RestConfigService)
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
  .bind<IUserProfileService>(SERVICE_IDENTIFIER.UserProfileService)
  .to(RestUserProfileService)
  .inSingletonScope();
container
  .bind<IUserFeedbackService>(SERVICE_IDENTIFIER.UserFeedbackService)
  .to(RestUserFeedbackService)
  .inSingletonScope();
container
  .bind<IUserEmailService>(SERVICE_IDENTIFIER.UserEmailService)
  .to(RestUserEmailService)
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
  .bind<IHttpDelegate>(DELEGATE_IDENTIFIER.HttpDelegate)
  .to(HttpDelegate)
  .inSingletonScope();

export default container;
