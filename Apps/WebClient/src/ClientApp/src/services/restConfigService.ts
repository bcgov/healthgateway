import { injectable } from "inversify";

import { ExternalConfiguration } from "@/models/configData";
import { ServiceName } from "@/models/errorInterfaces";
import { IConfigService, IHttpDelegate } from "@/services/interfaces";
import ErrorTranslator from "@/utility/errorTranslator";

@injectable()
export class RestConfigService implements IConfigService {
    private readonly CONFIG_BASE_URI: string = "/configuration";
    private http!: IHttpDelegate;

    public initialize(http: IHttpDelegate): void {
        this.http = http;
    }

    public getConfiguration(): Promise<ExternalConfiguration> {
        return new Promise((resolve, reject) => {
            this.http
                .getWithCors<ExternalConfiguration>(`${this.CONFIG_BASE_URI}/`)
                .then((result) => {
                    return resolve(result);
                })
                .catch((err) => {
                    console.log("Fetch error:" + err.toString());
                    reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceName.HealthGatewayUser
                        )
                    );
                });
        });
    }
}
