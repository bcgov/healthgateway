import ImmsData from "@/models/immsData";
import AuthenticationData from "@/models/authenticationData";

export enum StateType {
  NONE,
  INITIALIZED,
  REQUESTED,
  ERROR
}

export interface RootState {
  version: string;
}

export interface ImmsState {
  items?: ImmsData[];
  statusMessage: string;
  error: boolean;
  stateType: StateType;
}

export interface AuthState {
  authentication: AuthenticationData;
  isAuthenticated: boolean;
  statusMessage: string;
  error: any;
  stateType: StateType;
}
