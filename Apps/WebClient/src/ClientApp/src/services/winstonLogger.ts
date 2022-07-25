import { injectable } from "inversify";
import { createLogger, format, Logger, transports } from "winston";

import { ILogger } from "@/services/interfaces";

@injectable()
export class WinstonLogger implements ILogger {
    private logger!: Logger;

    public initialize(logLevel?: string): void {
        this.logger = createLogger({
            level: logLevel !== undefined ? logLevel.toLowerCase() : "info",
            format: format.json(),
            transports: [
                new transports.Console({
                    format: format.simple(),
                }),
            ],
        });
    }

    public log(level: string, message: string): void {
        this.logger.log({
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
