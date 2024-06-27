type DelegateIdentifier = "HttpDelegate";

type ServiceIdentifier = "CommunicationService" | "ConfigService" | "Logger";

export type Identifier = DelegateIdentifier | ServiceIdentifier;

export const DELEGATE_IDENTIFIER: { [key: string]: DelegateIdentifier } = {
    HttpDelegate: "HttpDelegate",
};

export const SERVICE_IDENTIFIER: { [key: string]: ServiceIdentifier } = {
    CommunicationService: "CommunicationService",
    ConfigService: "ConfigService",
    Logger: "Logger",
};
