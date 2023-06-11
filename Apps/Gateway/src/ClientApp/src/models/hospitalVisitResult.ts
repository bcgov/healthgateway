import { HospitalVisit } from "@/models/encounter";

export default interface HospitalVisitResult {
    loaded: boolean;
    queued: boolean;
    retryin: number;
    hospitalVisits: HospitalVisit[];
}
