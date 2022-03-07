export class QuickLink {
    public name!: string;
    public filter!: QuickLinkFilter;
}

export class QuickLinkFilter {
    public modules!: string[];
}

export class QuickLinkInformation {
    public index!: number;
    public title!: string;
    public description!: string | null;
    public icon!: string;
    public iconStyle!: string;
}
