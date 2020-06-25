export interface SnowplowWindow extends Window {
    snowplow: any;
}

declare global {
    interface Window {
        EnvVars: any;
    }
}
