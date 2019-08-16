import "reflect-metadata";
import { Container } from "inversify";

import SERVICE_IDENTIFIER from "@/constants/serviceIdentifiers";
import { IAuthenticationService, IImmsService } from "@/services/interfaces";
import { RestAuthenticationService } from "@/services/restAuthService";
import { RestImmsService } from "./services/restImmsService";

let container = new Container();
container
  .bind<IAuthenticationService>(SERVICE_IDENTIFIER.AuthenticationService)
  .to(RestAuthenticationService);
container
  .bind<IImmsService>(SERVICE_IDENTIFIER.ImmsService)
  .to(RestImmsService);

export default container;
