export default class ErrorTranslator {
    public static getHumanReadable(errorCode: string): string {
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
            case "C":
                return "Concurreny";
            case "CE":
                return "External Communication";
            case "CI":
                return "Internal Communication";
            case "I":
                return "Invalid State";
            default:
                return "Unknown";
        }
    }

    private static getServiceName(serviceName: string): string {
        console.log(serviceName);
        switch (serviceName) {
            case "DB":
                return "Data Base";
            case "CR":
                return "Client Registries";
            case "ODR":
                return "ODR";
            default:
                return "Unknown";
        }
    }
}
