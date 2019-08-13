import ImmsData from "@/models/immsData";
import { StateType } from "./rootState";

export interface ImmsState {
  items?: ImmsData[];
  statusMessage: string;
  error: boolean;
  stateType: StateType;
}
