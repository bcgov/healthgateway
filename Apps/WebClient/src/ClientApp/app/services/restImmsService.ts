import axios, { AxiosResponse } from "axios";
import { IImmsService } from "@/services/interfaces";

import { injectable } from "inversify";
import "reflect-metadata";

import ImmsData from "@/models/immsData";
import { ExternalConfiguration } from "@/models/ConfigData";

@injectable()
export class RestImmsService implements IImmsService {
  private baseUri: string = "";
  private readonly GET_AUTH_URI: string = "v1/api/imms/items";

  constructor() {
    console.log("Imms Rest Service...");
  }

  public initialize(config: ExternalConfiguration): void {
    this.baseUri = config.serviceEndpoints["Immunization"];
  }

  public getItems(): Promise<ImmsData[]> {
    return new Promise((resolve, reject) => {
      axios
        .get<ImmsData[]>(`${this.baseUri}${this.GET_AUTH_URI}`)
        .then((response: AxiosResponse) => {
          // Verify that the object is correct.
          if (response.data instanceof Object) {
            let items: ImmsData[] = response.data;
            return resolve(items);
          } else {
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
