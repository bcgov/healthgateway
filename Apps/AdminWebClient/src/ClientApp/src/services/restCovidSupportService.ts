import { injectable } from "inversify";

import CovidCardDocumentResult from "@/models/covidCardDocumentResult";
import CovidCardMailRequest from "@/models/covidCardMailRequest";
import CovidCardPatientResult from "@/models/covidCardPatientResult";
import RequestResult from "@/models/requestResult";
import { ICovidSupportService, IHttpDelegate } from "@/services/interfaces";
import RequestResultUtil from "@/utility/requestResultUtil";

@injectable()
export class RestCovidSupportService implements ICovidSupportService {
    private readonly BASE_URI: string = "v1/api/CovidSupport/Patient";
    private http!: IHttpDelegate;

    public initialize(http: IHttpDelegate): void {
        this.http = http;
    }

    public getPatient(phn: string): Promise<CovidCardPatientResult> {
        const mockedResult = {
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
        return Promise.resolve(mockedResult);

        return new Promise((resolve, reject) => {
            this.http
                .get<RequestResult<CovidCardPatientResult>>(
                    `${this.BASE_URI}`,
                    { phn: phn }
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

    public retrieveDocument(phn: string): Promise<CovidCardDocumentResult> {
        const mockedResult = {
            document:
                "JVBERi0xLjcKCjEgMCBvYmogICUgZW50cnkgcG9pbnQKPDwKICAvVHlwZSAvQ2F0YWxvZwog" +
                "IC9QYWdlcyAyIDAgUgo+PgplbmRvYmoKCjIgMCBvYmoKPDwKICAvVHlwZSAvUGFnZXMKICAv" +
                "TWVkaWFCb3ggWyAwIDAgMjAwIDIwMCBdCiAgL0NvdW50IDEKICAvS2lkcyBbIDMgMCBSIF0K" +
                "Pj4KZW5kb2JqCgozIDAgb2JqCjw8CiAgL1R5cGUgL1BhZ2UKICAvUGFyZW50IDIgMCBSCiAg" +
                "L1Jlc291cmNlcyA8PAogICAgL0ZvbnQgPDwKICAgICAgL0YxIDQgMCBSIAogICAgPj4KICA+" +
                "PgogIC9Db250ZW50cyA1IDAgUgo+PgplbmRvYmoKCjQgMCBvYmoKPDwKICAvVHlwZSAvRm9u" +
                "dAogIC9TdWJ0eXBlIC9UeXBlMQogIC9CYXNlRm9udCAvVGltZXMtUm9tYW4KPj4KZW5kb2Jq" +
                "Cgo1IDAgb2JqICAlIHBhZ2UgY29udGVudAo8PAogIC9MZW5ndGggNDQKPj4Kc3RyZWFtCkJU" +
                "CjcwIDUwIFRECi9GMSAxMiBUZgooSGVsbG8sIHdvcmxkISkgVGoKRVQKZW5kc3RyZWFtCmVu" +
                "ZG9iagoKeHJlZgowIDYKMDAwMDAwMDAwMCA2NTUzNSBmIAowMDAwMDAwMDEwIDAwMDAwIG4g" +
                "CjAwMDAwMDAwNzkgMDAwMDAgbiAKMDAwMDAwMDE3MyAwMDAwMCBuIAowMDAwMDAwMzAxIDAw" +
                "MDAwIG4gCjAwMDAwMDAzODAgMDAwMDAgbiAKdHJhaWxlcgo8PAogIC9TaXplIDYKICAvUm9v" +
                "dCAxIDAgUgo+PgpzdGFydHhyZWYKNDkyCiUlRU9G",
            fileName: "123.pdf",
        };
        return Promise.resolve(mockedResult);
        return new Promise((resolve, reject) => {
            this.http
                .get<RequestResult<CovidCardDocumentResult>>(
                    `${this.BASE_URI}/Document`,
                    { phn: phn }
                )
                .then((docResult) => {
                    return RequestResultUtil.handleResult(
                        docResult,
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

    public mailDocument(request: CovidCardMailRequest): Promise<boolean> {
        return new Promise((resolve, reject) => {
            this.http
                .post<RequestResult<boolean>>(
                    `${this.BASE_URI}/Document`,
                    request
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
