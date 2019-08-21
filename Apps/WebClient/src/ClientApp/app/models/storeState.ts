import ImmsData from "@/models/immsData";
import AuthenticationData from "@/models/authenticationData";
import { ExternalConfiguration } from "./ConfigData";

export enum StateType {
  NONE,
  INITIALIZED,
  REQUESTED,
  ERROR
}

export interface RootState {
  version: string;
}

export interface AuthState {
  authentication: AuthenticationData;
  isAuthenticated: boolean;
  statusMessage: string;
  error: any;
  stateType: StateType;
}

export interface ConfigState {
  config: ExternalConfiguration;
  statusMessage: string;
  error: boolean;
  stateType: StateType;
}
