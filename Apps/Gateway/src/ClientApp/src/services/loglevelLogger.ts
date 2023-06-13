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
            log.setLevel(level);
        } else {
            log.setLevel("info", false);
        }
    }

    error = log.error.bind(log);
    warn = log.warn.bind(log);
    info = log.info.bind(log);
    verbose = log.debug.bind(log);
    debug = log.debug.bind(log);
}
