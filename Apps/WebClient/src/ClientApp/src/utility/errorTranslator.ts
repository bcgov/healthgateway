import BannerError from "@/models/bannerError";
import { ResultError } from "@/models/requestResult";
import { ErrorType, ServiceName } from "@/models/errorInterfaces";

export default class ErrorTranslator {
    public static internalNetworkError(
        resultMessage: string,
        service: ServiceName
    ): ResultError {
        return {
            errorCode:
                "ClientApp-" + ErrorType.InternalCommunication + "-" + service,
            resultMessage: resultMessage,
        };
    }

    public static toBannerError(
        title: string,
        error?: ResultError
    ): BannerError {
        console.log(error);
        if (error) {
            return {
                title,
                description: this.getDisplayMessage(error.errorCode),
                detail: error.resultMessage,
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
                " got a " +
                this.getErrorType(sections[1]) +
                " Error."
            );
        } else if (sections.length === 3) {
            return (
                sections[0] +
                " got a " +
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
                return "ODR Services";
            case ServiceName.Medication:
                return "Medication Service";
            case ServiceName.Laboratory:
                return "Laboratory Service";
            case ServiceName.Immunization:
                return "Immunization Service";
            case ServiceName.Patient:
                return "Patient Service";
            case ServiceName.PHSA:
                return "PHSA Services";
            default:
                return "Unknown";
        }
    }
}
