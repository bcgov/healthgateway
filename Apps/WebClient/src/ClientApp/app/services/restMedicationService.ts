import { IMedicationService } from "@/services/interfaces";

import { injectable } from "inversify";
import "reflect-metadata";

import { ExternalConfiguration } from "@/models/configData";
import HttpDelegate from "@/services/httpDelegate";
import RequestResult from "@/models/requestResult";

@injectable()
export class RestMedicationService implements IMedicationService {
  private readonly MEDICATION_STATEMENT_BASE_URI: string =
    "v1/api/MedicationStatement/";
  private readonly MEDICATION_BASE_URI: string = "v1/api/Medication/";
  private baseUri: string = "";
  private http!: HttpDelegate;
  constructor() {}

  public initialize(config: ExternalConfiguration, http: HttpDelegate): void {
    this.baseUri = config.serviceEndpoints["Medication"];
    this.http = http;
  }

  public getPatientMedicationStatemens(hdid: string): Promise<RequestResult> {
    return this.http.get<RequestResult>(
      `${this.baseUri}${this.MEDICATION_STATEMENT_BASE_URI}${hdid}`
    );
  }

  public getMedicationInformation(
    drugIdentifier: string
  ): Promise<RequestResult> {
    return this.http.get<RequestResult>(
      `${this.baseUri}${this.MEDICATION_BASE_URI}${drugIdentifier}`
    );
  }
}
