import "reflect-metadata";

import { Container } from "inversify";

import { DELEGATE_IDENTIFIER, SERVICE_IDENTIFIER } from "@/plugins/inversify";
import HttpDelegate from "@/services/httpDelegate";
import {
    IAuthenticationService,
    IConfigService,
    ICovidSupportService,
    IHttpDelegate,
    ISupportService,
} from "@/services/interfaces";
import { RestAuthenticationService } from "@/services/restAuthenticationService";
import { RestConfigService } from "@/services/restConfigService";
import { RestCovidSupportService } from "@/services/restCovidSupportService";
import { RestSupportService } from "@/services/restSupportService";

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
    .bind<ISupportService>(SERVICE_IDENTIFIER.SupportService)
    .to(RestSupportService)
    .inSingletonScope();
container
    .bind<ICovidSupportService>(SERVICE_IDENTIFIER.CovidSupportService)
    .to(RestCovidSupportService)
    .inSingletonScope();
container
    .bind<IHttpDelegate>(DELEGATE_IDENTIFIER.HttpDelegate)
    .to(HttpDelegate)
    .inSingletonScope();

export default container;
