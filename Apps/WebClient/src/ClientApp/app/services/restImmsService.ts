import { IImmsService } from "@/services/interfaces";

import { injectable } from "inversify";
import "reflect-metadata";

import ImmsData from "@/models/immsData";
import { ExternalConfiguration } from "@/models/configData";
import HttpDelegate from "@/services/httpDelegate";

@injectable()
export class RestImmsService implements IImmsService {
  private readonly IMMS_BASE_URI: string = "v1/api/imms/";
  private baseUri: string = "";
  private http!: HttpDelegate;
  constructor() {}

  public initialize(config: ExternalConfiguration, http: HttpDelegate): void {
    this.baseUri = config.serviceEndpoints["Immunization"];
    this.http = http;
  }

  public getItems(): Promise<ImmsData[]> {
    return this.http.get<ImmsData[]>(
      `${this.baseUri}${this.IMMS_BASE_URI}items`
    );
  }
}
