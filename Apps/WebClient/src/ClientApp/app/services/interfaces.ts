import { User as OidcUser, UserManagerSettings } from "oidc-client";
import {
  ExternalConfiguration,
  OpenIdConnectConfiguration
} from "@/models/configData";
import ImmsData from "@/models/immsData";
import PatientData from "@/models/patientData";
import HttpDelegate from "./httpDelegate";
import RequestResult from "@/models/requestResult";

export interface IAuthenticationService {
  initialize(config: OpenIdConnectConfiguration): void;
  getUser(): Promise<OidcUser | null>;
  logout(): Promise<void>;
  signinSilent(): Promise<OidcUser | null>;
  signinRedirect(idphint: string, redirectPath: string): Promise<void>;
  signinRedirectCallback(): Promise<OidcUser>;
  getOidcConfig(): UserManagerSettings;
  removeUser(): Promise<void>;
  clearStaleState(): Promise<void>;
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
  getPatientMedicationStatements(hdid: string): Promise<RequestResult>;
  getMedicationInformation(drugIdentifier: string): Promise<RequestResult>;
  getPharmacyInfo(pharmacyId: string): Promise<RequestResult>;
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
