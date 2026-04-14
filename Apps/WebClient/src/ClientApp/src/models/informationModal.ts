import {
    Destination,
    ExternalUrl,
    InternalUrl,
    Origin,
    Text,
    Type,
} from "@/plugins/extensions";

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
    analyticsText?: Text;
    analyticsOrigin?: Origin;
    analyticsDestination?: Destination;
    analyticsType?: Type;
    analysticsUrl?: InternalUrl | ExternalUrl;
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
