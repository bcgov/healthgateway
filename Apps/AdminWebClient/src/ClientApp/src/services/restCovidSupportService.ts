import { injectable } from "inversify";

import CovidCardDocumentResult from "@/models/covidCardDocumentResult";
import CovidCardMailRequest from "@/models/covidCardMailRequest";
import CovidCardPatientResult from "@/models/covidCardPatientResult";
import CovidTreatmentAssessmentDetails from "@/models/covidTreatmentAssessmentDetails";
import CovidTreatmentAssessmentRequest from "@/models/covidTreatmentAssessmentRequest";
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

    public getPatient(
        phn: string,
        refresh: boolean
    ): Promise<CovidCardPatientResult> {
        return new Promise((resolve, reject) => {
            this.http
                .get<RequestResult<CovidCardPatientResult>>(
                    `${this.BASE_URI}/Patient`,
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
                    `${this.BASE_URI}/Patient/Document`,
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
                    `${this.BASE_URI}/Patient/Document`,
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

    public getCovidTreatmentAssessmentDetails(
        phn: string
    ): Promise<CovidTreatmentAssessmentDetails> {
        return new Promise((resolve, reject) => {
            this.http
                .get<RequestResult<CovidTreatmentAssessmentDetails>>(
                    `${this.BASE_URI}/CovidAssessmentDetails`,
                    { phn: phn }
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

    public submitCovidTreatmentAssessment(
        covidTreatmentAssessmentRequest: CovidTreatmentAssessmentRequest
    ): Promise<string> {
        return new Promise((resolve, reject) => {
            this.http
                .post<RequestResult<string>>(
                    `${this.BASE_URI}/CovidAssessment`,
                    covidTreatmentAssessmentRequest
                )
                .then((covidTreatmentResult) => {
                    return RequestResultUtil.handleResult(
                        covidTreatmentResult,
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
