import CovidVaccineRecord from "./covidVaccineRecord";
import { ResultError } from "./errors";
import { LoadStatus } from "./storeOperations";

export default interface VaccinationRecord {
    hdid: string;
    record?: CovidVaccineRecord;
    download: boolean;
    error?: ResultError;
    status: LoadStatus;
    statusMessage: string;
    resultMessage: string;
}
