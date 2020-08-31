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
import ErrorTranslator from "@/utility/errorTranslator";
import { ServiceName } from "@/models/errorInterfaces";
import Encounter from "@/models/encounter";

@injectable()
export class RestEncounterService implements IEncounterService {
    private logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    private readonly LABORATORY_BASE_URI: string = "v1/api/Encounter";
    private baseUri: string = "";
    private http!: IHttpDelegate;
    private isEnabled: boolean = false;

    constructor() {}

    public initialize(
        config: ExternalConfiguration,
        http: IHttpDelegate
    ): void {
        this.baseUri = config.serviceEndpoints["Encounter"];
        this.http = http;
        this.isEnabled = config.webClient.modules["Encounter"];
    }

    public getPatientEncounters(
        hdid: string
    ): Promise<RequestResult<Encounter[]>> {
        return new Promise((resolve, reject) => {
            resolve({
                pageIndex: 0,
                pageSize: 0,
                resourcePayload: [
                    {
                        id: "8bcbc841-899f-486a-a4b9-9b5172dcb70a",
                        practitionerName: "Smith, David",
                        specialtyDescription: "Doctor",
                        serviceDateTime: new Date(),
                        clinic: {
                            name: "Best Clinic Ever",
                            addressLine1: "1122 Fake St. V8N2C8",
                            clinicId: "38138e65-d657-4ce1-8b52-1f51f8695cb5",
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
