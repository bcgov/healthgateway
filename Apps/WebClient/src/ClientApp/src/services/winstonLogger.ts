import { Logger, createLogger, format, transports } from "winston";
import { ILogger } from "@/services/interfaces";
import { injectable } from "inversify";
import { ExternalConfiguration } from "@/models/configData";

@injectable()
export class WinstonLogger implements ILogger {
    private logger!: Logger;
    public addLogging: any = (callerName: string, fn: any) => (
        ...args: any[]
    ) => {
        this.logger.debug(
            `entering ${callerName}.${fn.name}: input args = ${args}`
        );
        try {
            const valueToReturn = fn(...args);
            this.logger.debug(
                `exiting ${callerName}.${fn.name}: valueToReturn = ${
                    valueToReturn !== undefined ? valueToReturn : "void"
                }`
            );
            return valueToReturn;
        } catch (thrownError) {
            this.logger.error(
                `exiting ${callerName}.${fn.name}: threw ${thrownError}`
            );
            throw thrownError;
        }
    };
    public initialize(config: ExternalConfiguration): void {
        this.logger = createLogger({
            level:
                config.webClient.logLevel !== undefined
                    ? config.webClient.logLevel.toLowerCase()
                    : "info",
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
            level: level,
            message: message,
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
