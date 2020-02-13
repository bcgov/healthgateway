import "reflect-metadata";
import { Container } from "inversify";

import { SERVICE_IDENTIFIER, DELEGATE_IDENTIFIER } from "@/plugins/inversify";
import {
  IHttpDelegate,
  IBetaRequestService,
  IConfigService,
  IAuthenticationService
} from "@/services/interfaces";
import HttpDelegate from "@/services/httpDelegate";
import { RestConfigService } from "@/services/restConfigService";
import { RestBetaRequestService } from "@/services/restBetaRequestService";
import { RestAuthenticationService } from "@/services/restAuthenticationService";

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
  .bind<IBetaRequestService>(SERVICE_IDENTIFIER.BetaRequestService)
  .to(RestBetaRequestService)
  .inSingletonScope();
container
  .bind<IHttpDelegate>(DELEGATE_IDENTIFIER.HttpDelegate)
  .to(HttpDelegate)
  .inSingletonScope();

export default container;
