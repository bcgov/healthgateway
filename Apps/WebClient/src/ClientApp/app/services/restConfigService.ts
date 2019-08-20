import axios, { AxiosResponse } from "axios";
import { IConfigService } from "@/services/interfaces";
import { injectable } from "inversify";
import "reflect-metadata";

import { ExternalConfiguration } from "@/models/ConfigData";

@injectable()
export class RestConfigService implements IConfigService {
  private readonly CONFIG_BASE_URI: string = "api/configuration";

  public getConfiguration(): Promise<ExternalConfiguration> {
    return new Promise((resolve, reject) => {
      axios
        .get<ExternalConfiguration>(`${this.CONFIG_BASE_URI}/`)
        .then((response: AxiosResponse) => {
          // Verify that the object is correct.
          if (response.data instanceof Object) {
            let config: ExternalConfiguration = response.data;
            return resolve(config);
          } else {
            console.log(response);
            return reject("invalid request");
          }
        })
        .catch(err => {
          console.log("Fetch error:" + err.toString());
          reject(err);
        });
    });
  }
}
