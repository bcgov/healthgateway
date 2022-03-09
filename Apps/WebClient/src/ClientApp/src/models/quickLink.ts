export class QuickLink {
    public name!: string;
    public filter!: QuickLinkFilter;
}

export class QuickLinkFilter {
    public modules!: string[];
}
