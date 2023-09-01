export default interface ReportField {
    key: string;
    title?: string;
    thAlign?: "start" | "end" | "center";
    tdAlign?: "start" | "end" | "center";
    width?: number | string;
    thAttr?: { "data-testid": string };
    tdAttr?: { "data-testid": string };
}
