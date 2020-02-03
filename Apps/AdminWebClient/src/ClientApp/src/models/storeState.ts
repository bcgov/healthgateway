import ExternalConfiguration from "@/models/externalConfiguration";

export enum StateType {
  NONE,
  INITIALIZED,
  REQUESTED,
  ERROR
}

export interface RootState {
  version: string;
}

export interface ConfigState {
  config: ExternalConfiguration;
  statusMessage: string;
  error: boolean;
  stateType: StateType;
}
