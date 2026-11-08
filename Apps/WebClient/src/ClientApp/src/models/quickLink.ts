export interface QuickLink {
    name: string;
    filter: { modules: string[] };
}
export const quickLinksToString = (links: QuickLink[]) => JSON.stringify(links);
export const quickLinksFromString = (json: string): QuickLink[] =>
    JSON.parse(json) as QuickLink[];
