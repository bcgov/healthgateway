import { injectable } from "inversify";
import { IImmsService, IHttpDelegate } from "@/services/interfaces";
import ImmsData from "@/models/immsData";
import { ExternalConfiguration } from "@/models/configData";

@injectable()
export class RestImmsService implements IImmsService {
  private readonly IMMS_BASE_URI: string = "v1/api/imms/";
  private baseUri: string = "";
  private http!: IHttpDelegate;
  constructor() {}

  public initialize(config: ExternalConfiguration, http: IHttpDelegate): void {
    this.baseUri = config.serviceEndpoints["Immunization"];
    this.http = http;
  }

  public getItems(): Promise<ImmsData[]> {
    return this.http.getWithCors<ImmsData[]>(
      `${this.baseUri}${this.IMMS_BASE_URI}items`
    );
  }
}
