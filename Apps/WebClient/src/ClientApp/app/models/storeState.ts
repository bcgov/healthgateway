import AuthenticationData from "@/models/authenticationData";
import { ExternalConfiguration } from "@/models/configData";
import User from "@/models/user";
import MedicationResult from "@/models/medicationResult";
import Pharmacy from "@/models/pharmacy";

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

export interface UserState {
  user: User;
  statusMessage: string;
  error: boolean;
  stateType: StateType;
}

export interface MedicationState {
  medications: MedicationResult[];
  statusMessage: string;
  error: boolean;
  stateType: StateType;
}

export interface PharmacyState {
  pharmacies: Pharmacy[];
  statusMessage: string;
  error: boolean;
  stateType: StateType;
}

export interface SidebarState {
  isOpen: boolean;
}
