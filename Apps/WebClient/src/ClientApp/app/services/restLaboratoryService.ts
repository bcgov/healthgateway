import { injectable } from "inversify";
import { IHttpDelegate, ILaboratoryService } from "@/services/interfaces";
import { ExternalConfiguration } from "@/models/configData";
import { LaboratoryReport } from "@/models/laboratory";
import RequestResult from "@/models/requestResult";
import { ResultType } from "@/constants/resulttype";

@injectable()
export class RestLaboratoryService implements ILaboratoryService {
  private readonly LABORATORY_BASE_URI: string = "v1/api/Laboratory/";
  private baseUri: string = "";
  private http!: IHttpDelegate;
  private isEnabled: boolean = false;

  constructor() {}

  public initialize(config: ExternalConfiguration, http: IHttpDelegate): void {
    this.baseUri = config.serviceEndpoints["Laboratory"];
    this.http = http;
    this.isEnabled =
      config.webClient.modules["Laboratory"] ||
      config.webClient.modules["CovidLabResults"];
  }

  public getLaboratoryReports(
    hdid: string
  ): Promise<RequestResult<LaboratoryReport[]>> {
    return new Promise((resolve, reject) => {
      //if (!this.isEnabled) {
      resolve({
        pageIndex: 0,
        pageSize: 0,
        resourcePayload: this.testData,
        resultMessage: "",
        resultStatus: ResultType.Success,
        totalResultCount: 0
      });
      return;
      //}
      /*this.http
        .getWithCors<RequestResult<LaboratoryResult[]>>(
          `${this.LABORATORY_BASE_URI}?patient=${hdid}`
        )
        .then(requestResult => {
          resolve(requestResult);
        })
        .catch(err => {
          console.log("Fetch error: " + err.toString());
          reject(err);
        });*/
    });
  }

  private testData: LaboratoryReport[] = [
    {
      id: "612d31e5-12e1-451f-a475-58d6b0a8f007",
      phn: "9735352542",
      orderingProviderIds: null,
      orderingProviders: "Davidson, Jana-Lea",
      reportingLab: "",
      location: "VCHA",
      ormOrOru: "ORU",
      messageDateTime: new Date("2020-03-18T12:17:19"),
      messageId: "20200770000196",
      additionalData: "",
      labResults: [
        {
          id: "2a16cf0d-7798-4911-a533-43692e3080dc",
          testType: "COVID19",
          outOfRange: false,
          collectedDateTime: new Date("2020-03-17T12:00:00"),
          testStatus: "Final",
          resultDescription:
            "Nasopharyngeal Swab<br>HEALTH CARE WORKER<br>Negative.<br>No COVID-19 virus (2019-nCoV) detected by NAT.<br><br>This test targets the RdRP and E gene regions of COVID-19 virus (2019-nCoV) and has not been fully validated.",
          receivedDateTime: new Date("2020-03-17T12:09:00"),
          resultDateTime: new Date("2020-03-17T12:17:00"),
          loinc: "XXX-3286",
          loincName: "COVID-19 n-Coronavirus  NAT"
        }
      ]
    }
  ];
}
