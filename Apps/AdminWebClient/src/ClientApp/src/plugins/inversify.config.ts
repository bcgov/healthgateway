import "reflect-metadata";

import { Container } from "inversify";

import { DELEGATE_IDENTIFIER, SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { DashboardService } from "@/services/dashboardService";
import HttpDelegate from "@/services/httpDelegate";
import {
    IAuthenticationService,
    ICommunicationService,
    IConfigService,
    IDashboardService,
    IEmailAdminService,
    IHttpDelegate,
    ISupportService,
    IUserFeedbackService,
} from "@/services/interfaces";
import { RestAuthenticationService } from "@/services/restAuthenticationService";
import { RestCommunicationService } from "@/services/restCommunicationService";
import { RestConfigService } from "@/services/restConfigService";
import { RestEmailAdminService } from "@/services/restEmailAdminService";
import { RestSupportService } from "@/services/restSupportService";
import { RestUserFeedbackService } from "@/services/restUserFeedbackService";

const container = new Container();
container
    .bind<IConfigService>(SERVICE_IDENTIFIER.ConfigService)
    .to(RestConfigService)
    .inSingletonScope();
container
    .bind<IAuthenticationService>(SERVICE_IDENTIFIER.AuthenticationService)
    .to(RestAuthenticationService)
    .inSingletonScope();
container
    .bind<IUserFeedbackService>(SERVICE_IDENTIFIER.UserFeedbackService)
    .to(RestUserFeedbackService)
    .inSingletonScope();
container
    .bind<IDashboardService>(SERVICE_IDENTIFIER.DashboardService)
    .to(DashboardService)
    .inSingletonScope();
container
    .bind<IEmailAdminService>(SERVICE_IDENTIFIER.EmailAdminService)
    .to(RestEmailAdminService)
    .inSingletonScope();
container
    .bind<ICommunicationService>(SERVICE_IDENTIFIER.CommunicationService)
    .to(RestCommunicationService)
    .inSingletonScope();
container
    .bind<ISupportService>(SERVICE_IDENTIFIER.SupportService)
    .to(RestSupportService)
    .inSingletonScope();
container
    .bind<IHttpDelegate>(DELEGATE_IDENTIFIER.HttpDelegate)
    .to(HttpDelegate)
    .inSingletonScope();

export default container;
