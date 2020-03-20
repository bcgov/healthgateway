import { injectable } from "inversify";
import { Dictionary } from "vue-router/types/router";
import { IMedicationService, IHttpDelegate } from "@/services/interfaces";
import { ExternalConfiguration } from "@/models/configData";
import RequestResult from "@/models/requestResult";
import MedicationStatement from "@/models/medicationStatement";
import { ResultType } from "@/constants/resulttype";
import MedicationResult from "@/models/medicationResult";
import Pharmacy from "@/models/pharmacy";

@injectable()
export class RestODRMedicationService implements IMedicationService {
  private readonly MEDICATION_STATEMENT_BASE_URI: string =
    "v1/api/ODRMedicationStatement/";
  private readonly MEDICATION_BASE_URI: string = "v1/api/Medication/";
  private baseUri: string = "";
  private http!: IHttpDelegate;
  private readonly FETCH_ERROR = "Fetch error:";

  constructor() {}

  public initialize(config: ExternalConfiguration, http: IHttpDelegate): void {
    this.baseUri = config.serviceEndpoints["ODRMedication"];
    this.http = http;
  }

  public getPatientMedicationStatements(
    hdid: string,
    protectiveWord?: string
  ): Promise<RequestResult<MedicationStatement[]>> {
    let headers: Dictionary<string> = {};
    if (protectiveWord) {
      headers["protectiveWord"] = protectiveWord;
    }
    return new Promise((resolve, reject) => {
      this.http
        .getWithCors<RequestResult<MedicationStatement[]>>(
          `${this.baseUri}${this.MEDICATION_STATEMENT_BASE_URI}${hdid}`,
          headers
        )
        .then(requestResult => {
          resolve(requestResult);
        })
        .catch(err => {
          console.log(this.FETCH_ERROR + err.toString());
          reject(err);
        });
    });
  }

  public getMedicationInformation(
    drugIdentifier: string
  ): Promise<MedicationResult> {
    return new Promise((resolve, reject) => {
      return this.http
        .getWithCors<RequestResult<MedicationResult>>(
          `${this.baseUri}${this.MEDICATION_BASE_URI}${drugIdentifier}`
        )
        .then(requestResult => {
          this.handleResult(requestResult, resolve, reject);
        })
        .catch(err => {
          console.log(this.FETCH_ERROR + err.toString());
          reject(err);
        });
    });
  }

  public getPharmacyInfo(pharmacyId: string): Promise<Pharmacy> {
    return new Promise((resolve, reject) => {
      resolve();
    });
  }

  private handleResult(
    requestResult: RequestResult<any>,
    resolve: any,
    reject: any
  ) {
    //console.log(requestResult);
    if (requestResult.resultStatus === ResultType.Success) {
      resolve(requestResult.resourcePayload);
    } else {
      reject(requestResult.resultMessage);
    }
  }
}
