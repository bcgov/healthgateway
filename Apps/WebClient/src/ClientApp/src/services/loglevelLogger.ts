import log from "loglevel";

import { ILogger } from "@/services/interfaces";

export class LoglevelLogger implements ILogger {
    public constructor(level?: string) {
        if (
            level === "error" ||
            level === "warn" ||
            level === "info" ||
            level === "debug"
        ) {
            log.setLevel(level, false);
        } else {
            log.setLevel("info", false);
        }
    }

    public error(message: string): void {
        log.error(message);
    }

    public warn(message: string): void {
        log.warn(message);
    }

    public info(message: string): void {
        log.info(message);
    }

    public verbose(message: string): void {
        log.debug(message);
    }

    public debug(message: string): void {
        log.debug(message);
    }
}
