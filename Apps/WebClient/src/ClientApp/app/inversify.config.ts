import "reflect-metadata";
import { Container } from "inversify";

import SERVICE_IDENTIFIER from "@/constants/serviceIdentifiers";
import { IAuthenticationService } from "@/services/interfaces";
import { RestAuthenticationService } from "@/services/restAuthService";

let container = new Container();
container.bind<IAuthenticationService>(SERVICE_IDENTIFIER.AuthenticationService).to(RestAuthenticationService);

export default container;