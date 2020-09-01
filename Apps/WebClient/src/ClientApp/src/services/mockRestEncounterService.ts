import { injectable } from "inversify";
import container from "@/plugins/inversify.config";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import {
    ILogger,
    IHttpDelegate,
    IEncounterService,
} from "@/services/interfaces";
import { ExternalConfiguration } from "@/models/configData";
import RequestResult from "@/models/requestResult";
import { ResultType } from "@/constants/resulttype";
import Encounter from "@/models/encounter";

@injectable()
export class RestEncounterService implements IEncounterService {
    constructor() {}

    public initialize(
        config: ExternalConfiguration,
        http: IHttpDelegate
    ): void {}

    public getPatientEncounters(
        hdid: string
    ): Promise<RequestResult<Encounter[]>> {
        return new Promise((resolve) => {
            resolve({
                pageIndex: 0,
                pageSize: 0,
                resourcePayload: [
                    {
                        id: "8bcbc841-899f-486a-a4b9-9b5172dcb70a",
                        practitionerName: "Smith, David",
                        specialtyDescription: "Doctor",
                        encounterDate: new Date(),
                        clinic: {
                            clinicId: "8bcbc841-899f-486a-a4b9-9b5172dcb70b",
                            name: "Best Clinic Ever",
                            addressLine1: "Unit 123",
                            addressLine2: "1122 Fake St.",
                            addressLine3: "",
                            addressLine4: "",
                            postalCode: "V8N2C8",
                            city: "Monako",
                            province: "BC",
                            phoneNumber: "2508582268",
                        },
                    },
                ],
                resultStatus: ResultType.Success,
                totalResultCount: 1,
            });
        });
    }
}
