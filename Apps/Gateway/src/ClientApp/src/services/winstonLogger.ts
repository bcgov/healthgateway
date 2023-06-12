import { format, Logger, loggers, transports } from "winston";

import { ILogger } from "@/services/interfaces";

export class WinstonLogger implements ILogger {
    private readonly loggerName = "default";
    private logger: Logger | undefined;

    public constructor(logLevel?: string) {
        this.logger =
            loggers.get(this.loggerName) ??
            loggers.add(this.loggerName, {
                level: logLevel?.toLowerCase() || "info",
                format: format.json(),
                transports: [
                    new transports.Console({
                        format: format.simple(),
                    }),
                ],
            });
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
