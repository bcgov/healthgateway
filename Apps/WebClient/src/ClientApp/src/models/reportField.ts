export default interface ReportField {
    key: string;
    label?: string;
    thClass: string;
    thStyle?: { width: string };
    thAttr?: { "data-testid": string };
    tdAttr?: { "data-testid": string };
}
