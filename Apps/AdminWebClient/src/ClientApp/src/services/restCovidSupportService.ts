import { injectable } from "inversify";

import CovidCardRequestResult from "@/models/covidCardRequestResult";
import RequestResult from "@/models/requestResult";
import { ICovidSupportService, IHttpDelegate } from "@/services/interfaces";
import RequestResultUtil from "@/utility/requestResultUtil";

@injectable()
export class RestCovidSupportService implements ICovidSupportService {
    private readonly BASE_URI: string = "v1/api/CovidSupport";
    private http!: IHttpDelegate;

    public initialize(http: IHttpDelegate): void {
        this.http = http;
    }

    public searchByPHN(phn: string): Promise<CovidCardRequestResult> {
        const result = {
            patientInfo: {
                hdid: "12345",
                firstname: "Tiago",
                lastname: "Graf",
                birthdate: "04/09/1989",
                personalhealthnumber: phn,
                address: {
                    address: "450 Dallas Road",
                    address2: "APt 904",
                    city: "Victoria",
                    province: "BC",
                    country: "CA",
                    postalCode: "V8V1B1",
                },
            },

            immunizations: [
                {
                    dateOfImmunization: "09/09/2020",
                    providerOrClinic: "Test Clinic",
                    immunization: {
                        immunizationAgents: [
                            {
                                productName: "Covid",
                                lotNumber: "123",
                            },
                        ],
                    },
                },
            ],
        };
        return Promise.resolve(result);

        return new Promise((resolve, reject) => {
            this.http
                .get<RequestResult<CovidCardRequestResult>>(
                    `${this.BASE_URI}?phn=${phn}`
                )
                .then((requestResult) => {
                    return RequestResultUtil.handleResult(
                        requestResult,
                        resolve,
                        reject
                    );
                })
                .catch((err) => {
                    console.log(err);
                    return reject(err);
                });
        });
    }

    public submitCard(
        phn: string,
        address: string
    ): Promise<CovidCardRequestResult> {
        return new Promise((resolve, reject) => {
            this.http
                .post<RequestResult<CovidCardRequestResult>>(
                    `${this.BASE_URI}`,
                    { phn, address }
                )
                .then((submitResult) => {
                    return RequestResultUtil.handleResult(
                        submitResult,
                        resolve,
                        reject
                    );
                })
                .catch((err) => {
                    console.log(err);
                    return reject(err);
                });
        });
    }
}
