export default interface ReportHeader {
    phn: string;
    name: string;
    dateOfBirth: string;
    datePrinted: string;
    filterText: string;
    isRedacted?: boolean;
}
