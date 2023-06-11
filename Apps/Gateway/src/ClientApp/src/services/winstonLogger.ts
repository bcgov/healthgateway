import { format, Logger, loggers, transports } from "winston";

import { ILogger } from "@/services/interfaces";

export class WinstonLogger implements ILogger {
    private logger: Logger | undefined;
    public constructor(initializeDefault?: boolean) {
        // Allows for the logger to be initialized with the default settings
        if (initializeDefault) {
            this.initialize();
        }
    }

    public initialize(logLevel?: string, loggerName?: string): void {
        const name = loggerName ?? "default";

        this.logger = loggers.get(name);
        if (!this.logger) {
            this.logger = loggers.add(name, {
                level: logLevel !== undefined ? logLevel.toLowerCase() : "info",
                format: format.json(),
                transports: [
                    new transports.Console({
                        format: format.simple(),
                    }),
                ],
            });
        }
    }

    public log(level: string, message: string): void {
        this.logger?.log({
            level,
            message,
        });
    }

    public error(message: string): void {
        this.log("error", message);
    }

    public warn(message: string): void {
        this.log("warn", message);
    }

    public info(message: string): void {
        this.log("info", message);
    }

    public verbose(message: string): void {
        this.log("verbose", message);
    }

    public debug(message: string): void {
        this.log("debug", message);
    }
}
