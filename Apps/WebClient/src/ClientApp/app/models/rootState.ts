export enum StateType {
  NONE,
  INITIALIZED,
  REQUESTED,
  ERROR
}

export interface RootState {
  version: string;
}
