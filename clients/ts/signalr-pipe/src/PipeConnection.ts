import { TransferFormat } from "@aspnet/signalr";
import { IConnection } from "./IConnection";
import { Arg } from "./Util";
import { Socket } from "net";
import { ITransport } from "./ITransport";
import { PipeTransport } from "./PipeTransport";
import { PipeSocket } from "./PipeSocket";

/** @private */
const enum ConnectionState {
    Connecting,
    Connected,
    Disconnected,
}

export class PipeConnection implements IConnection {
    private connectionState: ConnectionState;
    private startPromise?: Promise<void>;
    private transport?: ITransport;
    private stopError?: Error;

    public readonly features: any = {};

    public onreceive: ((data: string | ArrayBuffer) => void) | null;
    public onclose: ((error?: Error) => void) | null;

    constructor(private readonly uri: URL) {
        Arg.isRequired(uri, "uri");

        this.features.inherentKeepAlive = true;

        this.connectionState = ConnectionState.Disconnected;
        this.onreceive = null;
        this.onclose = null;
    }

    // tslint:disable-next-line:variable-name
    public start(transferFormat: TransferFormat): Promise<void> {
        if (this.connectionState !== ConnectionState.Disconnected) {
            return Promise.reject(new Error("Cannot start a connection that is not in the 'Disconnected' state."));
        }

        this.connectionState = ConnectionState.Connecting;

        this.startPromise = this.startInternal(transferFormat);
        return this.startPromise;
    }

    public send(data: string | ArrayBuffer): Promise<void> {
        if (this.connectionState !== ConnectionState.Connected) {
            throw new Error("Cannot send data if the connection is not in the 'Connected' State.");
        }

        return this.transport!.send(data);
    }

    public async stop(error?: Error): Promise<void> {
        this.connectionState = ConnectionState.Disconnected;

        this.stopError = error;

        try {
            await this.startPromise;
        } catch (e) {
            // this exception is returned to the user as a rejected Promise from the start method
        }

        if (this.transport) {
            await this.transport.stop();
            this.transport = undefined;
        }
    }

    private async startInternal(transferFormat: TransferFormat): Promise<void> {
        try {
            this.transport = new PipeTransport(
                () => new PipeSocket(() => new Socket()),
                transferFormat);

            this.transport.onreceive = this.onreceive;
            this.transport.onclose = (e) => this.stopConnection(e);

            await this.transport.connect(this.uri);

            this.changeState(ConnectionState.Connecting, ConnectionState.Connected);
        } catch (e) {
            this.connectionState = ConnectionState.Disconnected;
            this.transport = undefined;
            throw e;
        }
    }

    private stopConnection(error?: Error): void {
        this.transport = undefined;

        error = this.stopError || error;

        this.connectionState = ConnectionState.Disconnected;

        if (this.onclose) {
            this.onclose(error);
        }
    }

    private changeState(from: ConnectionState, to: ConnectionState): boolean {
        if (this.connectionState === from) {
            this.connectionState = to;
            return true;
        }
        return false;
    }
}
