import { IMedicationService } from "@/services/interfaces";

import { injectable } from "inversify";
import "reflect-metadata";

import { ExternalConfiguration } from "@/models/configData";
import HttpDelegate from "@/services/httpDelegate";
import Prescription from '@/models/prescription';

@injectable()
export class RestMedicationService implements IMedicationService {
  private readonly MEDICATION_BASE_URI: string = "v1/api/Medication/";
  private baseUri: string = "";
  private http!: HttpDelegate;
  constructor() {}

  public initialize(config: ExternalConfiguration, http: HttpDelegate): void {
    this.baseUri = config.serviceEndpoints["Medication"];
    this.http = http;
  }

  public getPatientPrescriptions(hdid: string): Promise<Prescription> {
    return this.http.get<Prescription>(
      `${this.baseUri}${this.MEDICATION_BASE_URI}${hdid}`
    );
  }
}
