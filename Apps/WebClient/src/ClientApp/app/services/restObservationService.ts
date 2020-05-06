import { injectable } from "inversify";
import { IHttpDelegate, IObservationService } from "@/services/interfaces";
import { ExternalConfiguration } from "@/models/configData";
import Observation from "@/models/observation";

@injectable()
export class RestObservationService implements IObservationService {
  private readonly OBSERVATION_BASE_URI: string = "v1/api/Observation/";
  private baseUri: string = "";
  private http!: IHttpDelegate;
  constructor() {}

  public initialize(config: ExternalConfiguration, http: IHttpDelegate): void {
    this.baseUri = config.serviceEndpoints["Observation"];
    this.http = http;
  }

  public getLaboratoryResults(hdid: string): Promise<Observation> {
    return this.http.get<Observation>(
      `${this.baseUri}${this.OBSERVATION_BASE_URI}${hdid}`
    );
  }
}
