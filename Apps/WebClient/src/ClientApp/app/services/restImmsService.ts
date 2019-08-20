import axios, { AxiosResponse } from "axios";
import { IImmsService } from "@/services/interfaces";

import { injectable } from "inversify";
import "reflect-metadata";

import ImmsData from "@/models/immsData";

@injectable()
export class RestImmsService implements IImmsService {
  private readonly GET_AUTH_URI: string = "api/imms/items";

  constructor() {
    console.log('Imms Rest Service...');
  }
  public getItems(): Promise<ImmsData[]> {
    return new Promise((resolve, reject) => {
      axios
        .get<ImmsData[]>(this.GET_AUTH_URI)
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
