import { injectable } from "inversify";

import { IDashboardService, IHttpDelegate } from "@/services/interfaces";

@injectable()
export class DashboardService implements IDashboardService {
    private readonly BASE_URI: string = "v1/api/Dashboard";
    private http!: IHttpDelegate;

    public initialize(http: IHttpDelegate): void {
        this.http = http;
    }

    public getRegisteredUsersCount(): Promise<{ [key: string]: number }> {
        return new Promise((resolve, reject) => {
            this.http
                .get<{ [key: string]: number }>(
                    `${this.BASE_URI}/RegisteredCount?timeOffset=${
                        new Date().getTimezoneOffset() * -1
                    }`
                )
                .then((requestResult) => {
                    resolve(requestResult);
                })
                .catch((err) => {
                    console.log(err);
                    return reject(err);
                });
        });
    }

    public getLoggedInUsersCount(): Promise<{ [key: string]: number }> {
        return new Promise((resolve, reject) => {
            this.http
                .get<{ [key: string]: number }>(
                    `${this.BASE_URI}/LoggedInCount?timeOffset=${
                        new Date().getTimezoneOffset() * -1
                    }`
                )
                .then((requestResult) => {
                    resolve(requestResult);
                })
                .catch((err) => {
                    console.log(err);
                    return reject(err);
                });
        });
    }

    public getDependentCount(): Promise<{ [key: string]: number }> {
        return new Promise((resolve, reject) => {
            this.http
                .get<{ [key: string]: number }>(
                    `${this.BASE_URI}/DependentCount?timeOffset=${
                        new Date().getTimezoneOffset() * -1
                    }`
                )
                .then((requestResult) => {
                    resolve(requestResult);
                })
                .catch((err) => {
                    console.log(err);
                    return reject(err);
                });
        });
    }

    public getRecurrentUserCount(
        days: number,
        startPeriodDate: string,
        endPeriodDate: string
    ): Promise<number> {
        return new Promise((resolve, reject) => {
            this.http
                .get<number>(
                    `${
                        this.BASE_URI
                    }/RecurringUsers?days=${days}&startPeriod=${startPeriodDate}&endPeriod=${endPeriodDate}&timeOffset=${new Date().getTimezoneOffset()}`
                )
                .then((requestResult) => {
                    resolve(requestResult);
                })
                .catch((err) => {
                    console.log(err);
                    return reject(err);
                });
        });
    }

    public getRatings(
        startPeriodDate: string,
        endPeriodDate: string
    ): Promise<{ [key: string]: number }> {
        return new Promise((resolve, reject) => {
            this.http
                .get<Promise<{ [key: string]: number }>>(
                    `${
                        this.BASE_URI
                    }/Ratings/Summary?startPeriod=${startPeriodDate}&endPeriod=${endPeriodDate}&timeOffset=${new Date().getTimezoneOffset()}`
                )
                .then((requestResult) => {
                    resolve(requestResult);
                })
                .catch((err) => {
                    console.log(err);
                    return reject(err);
                });
        });
    }
}
