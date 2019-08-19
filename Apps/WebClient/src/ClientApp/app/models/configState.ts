import { StateType } from "./rootState";
import { ExternalConfiguration } from "./ConfigData";

export interface ConfigState {
  config: ExternalConfiguration;
  statusMessage: string;
  error: boolean;
  stateType: StateType;
}
