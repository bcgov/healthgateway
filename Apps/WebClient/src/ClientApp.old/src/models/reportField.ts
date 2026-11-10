export default interface ReportField {
    key: string;
    title?: string;
    thAlign?: "start" | "end" | "center";
    tdAlign?: "start" | "end" | "center";
    width?: number | string;
    thAttr?: { "data-testid": string; class?: string; style?: string };
    tdAttr?: { "data-testid": string; class?: string; style?: string };
}
