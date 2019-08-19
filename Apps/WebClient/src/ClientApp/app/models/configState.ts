import { ExternalConfiguration } from "./ConfigData";
import { StateType } from './storeState';

export interface ConfigState {
  config: ExternalConfiguration;
  statusMessage: string;
  error: boolean;
  stateType: StateType;
}
