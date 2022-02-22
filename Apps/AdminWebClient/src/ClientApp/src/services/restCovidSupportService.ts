import { injectable } from "inversify";

import CovidCardDocumentResult from "@/models/covidCardDocumentResult";
import CovidCardMailRequest from "@/models/covidCardMailRequest";
import CovidCardPatientResult from "@/models/covidCardPatientResult";
import CovidTherapyAssessmentDetails from "@/models/covidTherapyAssessmentDetails";
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

    public getPatient(
        phn: string,
        refresh: boolean
    ): Promise<CovidCardPatientResult> {
        return new Promise((resolve, reject) => {
            this.http
                .get<RequestResult<CovidCardPatientResult>>(
                    `${this.BASE_URI}`,
                    { phn: phn, refresh: String(refresh) }
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

    public getCovidTherapyAssessmentDetails(
        phn: string
    ): Promise<CovidTherapyAssessmentDetails> {
        return new Promise((resolve, reject) => {
            this.http
                .post<RequestResult<CovidTherapyAssessmentDetails>>(
                    `${this.BASE_URI}/history`,
                    {
                        phn: phn,
                    }
                )
                .then((historyResult) => {
                    return RequestResultUtil.handleResult(
                        historyResult,
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
