import { HubConnection, IHubProtocol, JsonHubProtocol, NullLogger, ILogger, LogLevel } from "@aspnet/signalr";
import { Arg, ConsoleLogger, validatePipeUri } from "./Util";
import { PipeConnection } from "./PipeConnection";

export class PipeHubConnectionBuilder {
    /** @internal */
    public protocol?: IHubProtocol;
    /** @internal */
    public url?: URL;
    /** @internal */
    public logger?: ILogger;

    public configureLogging(logLevel: LogLevel): PipeHubConnectionBuilder;
    public configureLogging(logger: ILogger): PipeHubConnectionBuilder;
    public configureLogging(logging: LogLevel | ILogger): PipeHubConnectionBuilder;
    public configureLogging(logging: LogLevel | ILogger): PipeHubConnectionBuilder {
        if (isLogger(logging)) {
            this.logger = logging;
        } else {
            this.logger = new ConsoleLogger(logging);
        }

        return this;
    }

    public withUrl(url: string): PipeHubConnectionBuilder {
        Arg.isRequired(url, "url");

        const parsedUrl = new URL(url);
        validatePipeUri(parsedUrl);

        this.url = parsedUrl;

        return this;
    }

    public withHubProtocol(protocol: IHubProtocol): PipeHubConnectionBuilder {
        Arg.isRequired(protocol, "protocol");

        this.protocol = protocol;
        return this;
    }

    public build(): HubConnection {
        if (!this.url) {
            throw new Error("The 'PipeHubConnectionBuilder.withUrl' method must be called before building the connection.");
        }

        const connection = new PipeConnection(this.url);

        return (HubConnection as any).create(connection, 
            this.logger || NullLogger.instance,
            this.protocol || new JsonHubProtocol());
    }
}

function isLogger(logger: any): logger is ILogger {
    return logger.log !== undefined;
}
