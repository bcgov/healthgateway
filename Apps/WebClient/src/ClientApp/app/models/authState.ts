import AuthenticationData from '@/models/authenticationData';
import { StateType } from './rootState';

export interface AuthState {
    authentication?: AuthenticationData;
    statusMessage: string;
    error: boolean;
    stateType: StateType;
}
