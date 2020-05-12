// Laboratory model
export interface LaboratoryResult {
  id: string;
  testType: string | null;
  outOfRange: boolean;
  collectedDateTime: Date;
  testStatus: string | null;
  resultDescription: string | null;
  receivedDateTime: Date;
  resultDateTime: Date;
  loinc: string | null;
  loincName: string | null;
}

export interface LaboratoryReport {
  id: string;
  phn: string | null;
  orderingProviderIds: string | null;
  orderingProviders: string | null;
  reportingLab: string | null;
  location: string | null;
  ormOrOru: string | null;
  messageDateTime: Date;
  messageId: string | null;
  additionalData: string | null;
  labResults: LaboratoryResult[];
}
