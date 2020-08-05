import Winston from "winston";
import { ILogger } from "@/services/interfaces";
import { injectable } from "inversify";

@injectable()
export class WinstonLogger implements ILogger {
    private logger!: Winston.Logger;

    public initialize(): void {
        this.logger = Winston.createLogger({
            level: "debug",
            format: Winston.format.json(),
            transports: [
                new Winston.transports.Console({
                    format: Winston.format.simple(),
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
