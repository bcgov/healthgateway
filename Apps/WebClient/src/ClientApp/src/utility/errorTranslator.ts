import BannerError from "@/models/bannerError";
import { ResultError } from "@/models/requestResult";
import { ErrorType, ServiceName } from "@/models/errorInterfaces";

export default class ErrorTranslator {
    public static internalNetworkError(
        errorDetail: string,
        service: ServiceName
    ): ResultError {
        return {
            errorCode:
                "ClientApp-" + ErrorType.InternalCommunication + "-" + service,
            errorDetail: errorDetail,
        };
    }

    public static getBannerError(
        title: string,
        error?: ResultError
    ): BannerError {
        console.log(error);
        if (error) {
            return {
                title,
                description: this.getDisplayMessage(error.errorCode),
                detail: error.errorDetail,
                errorCode: error.errorCode,
            };
        } else {
            return {
                title,
                description: "",
                detail: "",
                errorCode: "",
            };
        }
    }

    public static getDisplayMessage(errorCode: string): string {
        let sections = errorCode.split("-");
        console.log(sections);
        if (sections.length === 1) {
            return sections[0];
        } else if (sections.length === 2) {
            return (
                sections[0] +
                " got an " +
                this.getErrorType(sections[1]) +
                " Error."
            );
        } else if (sections.length === 3) {
            return (
                sections[0] +
                " got an " +
                this.getErrorType(sections[1]) +
                " error while processing a " +
                this.getServiceName(sections[2]) +
                " request."
            );
        } else {
            return errorCode;
        }
    }

    private static getErrorType(errorType: string): string {
        console.log(errorType);
        switch (errorType) {
            case ErrorType.Concurreny:
                return "Concurreny";
            case ErrorType.ExternalCommunication:
                return "External Communication";
            case ErrorType.InternalCommunication:
                return "Internal Communication";
            case ErrorType.InvalidState:
                return "Invalid State";
            default:
                return "Unknown";
        }
    }

    private static getServiceName(serviceName: string): string {
        console.log(serviceName);
        switch (serviceName) {
            case ServiceName.DataBase:
                return "Data Base";
            case ServiceName.ClientRegistries:
                return "Client Registries";
            case ServiceName.ODR:
                return "ODR";
            case ServiceName.Medication:
                return "Medication";
            case ServiceName.Laboratory:
                return "Laboratory";
            case ServiceName.Immunization:
                return "Immunization";
            case ServiceName.Patient:
                return "Patient";
            default:
                return "Unknown";
        }
    }
}
