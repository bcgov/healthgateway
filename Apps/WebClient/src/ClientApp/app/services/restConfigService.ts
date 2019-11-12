import { IConfigService } from "@/services/interfaces";
import { injectable } from "inversify";
import "reflect-metadata";

import { ExternalConfiguration } from "@/models/configData";
import HttpDelegate from "@/services/httpDelegate";

@injectable()
export class RestConfigService implements IConfigService {
  private readonly CONFIG_BASE_URI: string = "v1/api/configuration";
  private http!: HttpDelegate;

  public initialize(http: HttpDelegate): void {
    this.http = http;
  }
  public getConfiguration(): Promise<ExternalConfiguration> {
    return new Promise((resolve, reject) => {
      this.http
        .get<ExternalConfiguration>(`${this.CONFIG_BASE_URI}/`)
        .then(result => {
          return resolve(result);
        })
        .catch(err => {
          console.log("Fetch error:" + err.toString());
          reject(err);
        });
    });
  }
}
