// Port of models/informationModal.ts
import type { Destination, Origin, Text, Type } from "@/constants/tracking";

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
    /** (sic) the web client's property name */
    analysticsUrl?: string;
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
