export interface SnowplowWindow extends Window {
    snowplow(eventName: string): void;
}
