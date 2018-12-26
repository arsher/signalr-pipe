import { ILogger, LogLevel } from "@aspnet/signalr";
import { createHash } from "crypto";

const PipeScheme: string = "signalr.pipe";

/** @private */
export function validatePipeUri(uri: URL): void {
    let protocol = uri.protocol;
    protocol = protocol.substr(0, protocol.length - 1);
    if (protocol !== PipeScheme) {
        throw new Error(`The uri should use the pipe protocol :'${PipeScheme}'`);
    }
}

/** @private */
export function createPipeName(uri: URL): string {
    const host = uri.host;
    let path: string = uri.pathname.toUpperCase();
    if (path.lastIndexOf("/") !== path.length - 1) {
        path = path + "/";
    }

    const segments = [
        PipeScheme,
        "://",
        host.toUpperCase(),
        path,
    ];

    const canonicalName = segments.join("");
    let base64: string;
    let separator: string;
    if (canonicalName.length > 128) {
        const sha1 = createHash("sha1");
        sha1.update(Buffer.from(canonicalName));
        base64 = sha1.digest("base64");
        separator = "_H";
    } else {
        base64 = Buffer.from(canonicalName).toString("base64");
        separator = "_E";
    }

    const finalSegments = [
        PipeScheme,
        separator,
        base64,
    ];

    return finalSegments.join("");
}

/** @private */
export class Arg {
    public static isRequired(val: any, name: string): void {
        if (val === null || val === undefined) {
            throw new Error(`The '${name}' argument is required.`);
        }
    }

    public static isIn(val: any, values: any, name: string): void {
        // TypeScript enums have keys for **both** the name and the value of each enum member on the type itself.
        if (!(val in values)) {
            throw new Error(`Unknown ${name} value: ${val}.`);
        }
    }
}

/** @private */
export class ConsoleLogger implements ILogger {
    private readonly minimumLogLevel: LogLevel;

    constructor(minimumLogLevel: LogLevel) {
        this.minimumLogLevel = minimumLogLevel;
    }

    public log(logLevel: LogLevel, message: string): void {
        if (logLevel >= this.minimumLogLevel) {
            switch (logLevel) {
                case LogLevel.Critical:
                case LogLevel.Error:
                    console.error(`[${new Date().toISOString()}] ${LogLevel[logLevel]}: ${message}`);
                    break;
                case LogLevel.Warning:
                    console.warn(`[${new Date().toISOString()}] ${LogLevel[logLevel]}: ${message}`);
                    break;
                case LogLevel.Information:
                    console.info(`[${new Date().toISOString()}] ${LogLevel[logLevel]}: ${message}`);
                    break;
                default:
                    // console.debug only goes to attached debuggers in Node, so we use console.log for Trace and Debug
                    console.log(`[${new Date().toISOString()}] ${LogLevel[logLevel]}: ${message}`);
                    break;
            }
        }
    }
}
