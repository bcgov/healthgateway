import "reflect-metadata";
import { Container } from "inversify";

import SERVICE_IDENTIFIER from "@/constants/serviceIdentifiers";
import {
  IAuthenticationService,
  IImmsService,
  IConfigService
} from "@/services/interfaces";
import { RestAuthenticationService } from "@/services/restAuthService";
import { RestImmsService } from "./services/restImmsService";
import { RestConfigService } from "./services/restConfigService";

let container = new Container();
container
  .bind<IConfigService>(SERVICE_IDENTIFIER.ConfigService)
  .to(RestConfigService);
container
  .bind<IAuthenticationService>(SERVICE_IDENTIFIER.AuthenticationService)
  .to(RestAuthenticationService)
  .inSingletonScope();
container
  .bind<IImmsService>(SERVICE_IDENTIFIER.ImmsService)
  .to(RestImmsService);

export default container;
