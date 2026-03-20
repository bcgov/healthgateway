export interface InformationModalTextSegment {
    type: "text";
    value: string;
}

export interface InformationModalBoldSegment {
    type: "bold";
    value: string;
}

export interface InformationModalLinkSegment {
    type: "link";
    text: string;
    href: string;
}

export type InformationModalSegment =
    | InformationModalTextSegment
    | InformationModalBoldSegment
    | InformationModalLinkSegment;

export interface InformationModalParagraph {
    segments: InformationModalSegment[];
}

export interface InformationModalContent {
    title: string;
    paragraphs: InformationModalParagraph[];
}
