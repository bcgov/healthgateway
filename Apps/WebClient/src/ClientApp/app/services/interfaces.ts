import { User as OidcUser, UserManagerSettings } from "oidc-client";
import {
  ExternalConfiguration,
  OpenIdConnectConfiguration
} from "@/models/configData";
import ImmsData from "@/models/immsData";
import PatientData from "@/models/patientData";
import HttpDelegate from "./httpDelegate";
import MedicationStatement from "@/models/medicationStatement";

export interface IAuthenticationService {
  initialize(config: OpenIdConnectConfiguration): void;
  getUser(): Promise<OidcUser | null>;
  logout(): Promise<void>;
  signinSilent(): Promise<OidcUser | null>;
  signinRedirect(idphint: string, redirectPath: string): Promise<void>;
  signinRedirectCallback(): Promise<OidcUser>;
  getOidcConfig(): UserManagerSettings;
}

export interface IImmsService {
  initialize(config: ExternalConfiguration, http: IHttpDelegate): void;
  getItems(): Promise<ImmsData[]>;
}

export interface IPatientService {
  initialize(config: ExternalConfiguration, http: IHttpDelegate): void;
  getPatientData(hdid: string): Promise<PatientData>;
}

export interface IMedicationService {
  initialize(config: ExternalConfiguration, http: IHttpDelegate): void;
  getPatientMedicationStatemens(hdid: string): Promise<MedicationStatement[]>;
}

export interface IConfigService {
  initialize(http: HttpDelegate): void;
  getConfiguration(): Promise<ExternalConfiguration>;
}

export interface IHttpDelegate {
  unsetAuthorizationHeader(): void;
  setAuthorizationHeader(accessToken: string): void;
  get<T>(url: string): Promise<T>;
}
