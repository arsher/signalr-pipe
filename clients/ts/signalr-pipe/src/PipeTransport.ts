import { TransferFormat } from "@aspnet/signalr";
import { ITransport } from "./ITransport";
import { createPipeName } from "./Util";
import { ISocket } from "./ISocket";
import { MessageTransform } from "./MessageTransform";
import { TextTransform } from "./TextTransform";
import { BinaryTransform } from "./BinaryTransform";
import { TextMessageFormat } from "./TextMessageFormat";

export class PipeTransport implements ITransport {
    private socket?: ISocket;
    private transform?: MessageTransform;
    private handshakeDone: boolean = false;
    private connectPromise?: Promise<void>;
    private connectResolve?: () => void;

    onreceive: ((data: string | ArrayBuffer) => void) | null;
    onclose: ((error?: Error | undefined) => void) | null;

    constructor(private readonly pipeFactory: () => ISocket,
        private readonly transferFormat: TransferFormat) {
        this.onreceive = null;
        this.onclose = null;
    }

    public async connect(url: URL): Promise<void> {
        if (!this.connectPromise) {
            this.connectPromise = this.connectInternal(url);
        }
        return this.connectPromise;
    }

    public send(data: string | ArrayBuffer): Promise<void> {
        if (this.socket) {
            return this.socket.send(data);
        }

        return Promise.reject("Pipe is not available.");
    }

    public stop(): Promise<void> {
        if (this.socket) {
            this.socket.disconnect();
            return Promise.resolve();
        }

        return Promise.reject("Pipe is not available.");
    }

    private async connectInternal(url: URL): Promise<void> {
        this.socket = this.pipeFactory();

        const connectionIdReceived = new Promise<void>((resolve, _reject) => {
            this.connectResolve = resolve;
        });

        const actualPipeName = PipeTransport.getPipeName(await this.receiveActualPipeName(url));
        await this.socket.connect(actualPipeName);

        //send the actual hub name here
        await this.socket.send(TextMessageFormat.write(PipeTransport.extractRoutePart(url)));

        const beforeHandshakeStream = this.socket.stream.pipe(new TextTransform());
        beforeHandshakeStream.on("data", this.socketOnDataBeforeHandshake.bind(this));

        this.socket.onclose = this.onclose;

        await connectionIdReceived;

        beforeHandshakeStream.removeAllListeners("data");
    }

    private socketOnDataBeforeHandshake(_data: string) {
        if (!this.handshakeDone) {
            this.handshakeDone = true;

            this.socket!.stream.unpipe();

            if (this.transferFormat === TransferFormat.Text) {
                this.transform = new TextTransform();
                this.transform.enabled = true;
            } else {
                this.transform = new BinaryTransform();
            }

            this.socket!.stream.pipe(this.transform)
                .on("data", this.socketOnData.bind(this));

            this.connectResolve!();
        }
    }

    private socketOnData(data: string | Buffer): void {
        if (this.onreceive) {
            if (this.transferFormat === TransferFormat.Text) {
                const strData = data.toString();
                this.onreceive(strData);
            } else if (data instanceof Buffer) {
                if (data.length === 3 && this.transferFormat === TransferFormat.Binary) {
                    this.transform!.enabled = true;
                }
                this.onreceive(data)
            } else {
                this.onreceive(Buffer.from(data));
            }
        }
    }

    private async receiveActualPipeName(uri: URL): Promise<string> {
        const significantUrlPart = PipeTransport.extractPipePart(uri);
        const pipeName = PipeTransport.getPipeName(createPipeName(new URL(significantUrlPart)));
        const pipe = this.pipeFactory();
        try {
            await pipe.connect(pipeName);
            return await pipe.readString();
        } finally {
            pipe.disconnect();
        }
    }

    private static extractRoutePart(uri: URL): string {
        return uri.pathname;
    }

    private static extractPipePart(uri: URL): string {
        return `${uri.protocol}//${uri.hostname}`;
    }

    private static getPipeName(name: string): string {
        return `\\\\.\\pipe\\${name}`;
    }
}