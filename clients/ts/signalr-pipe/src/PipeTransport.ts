import { TransferFormat } from "@aspnet/signalr";
import { ITransport } from "./ITransport";
import { createPipeName } from "./Util";
import { ISocket } from "./ISocket";
import { MessageTransform } from "./MessageTransform";
import { TextTransform } from "./TextTransform";
import { BinaryTransform } from "./BinaryTransform";

export class PipeTransport implements ITransport {
    private socket?: ISocket;
    private transform?: MessageTransform;

    onreceive: ((data: string | ArrayBuffer) => void) | null;
    onclose: ((error?: Error | undefined) => void) | null;

    constructor(private readonly pipeFactory: () => ISocket, private readonly transferFormat: TransferFormat) {
        this.onreceive = null;
        this.onclose = null;
    }

    public async connect(url: URL): Promise<void> {
        this.socket = this.pipeFactory();

        const actualPipeName = PipeTransport.getPipeName(await this.receiveActualPipeName(url));
        await this.socket.connect(actualPipeName);
        try {
            await this.socket.readLine();
        } catch (e) {
            console.error(e);
            throw e;
        }

        if (this.transferFormat === TransferFormat.Text) {
            this.transform = new TextTransform();
            this.transform.enabled = true;
        } else {
            this.transform = new BinaryTransform();
        }

        this.socket.stream.pipe(this.transform)
            .on("data", this.socketOnData.bind(this));

        this.socket.onclose = this.onclose;
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

    private socketOnData(data: string | Buffer): void {
        if (this.onreceive) {
            if (this.transferFormat === TransferFormat.Text) {
                const strData = data.toString();
                this.onreceive(strData);
            } else if (data instanceof Buffer) {
                if(data.length === 3 && this.transferFormat === TransferFormat.Binary) {
                    this.transform!.enabled = true;
                }
                this.onreceive(data)
            } else {
                this.onreceive(Buffer.from(data));
            }
        }
    }

    private async receiveActualPipeName(uri: URL): Promise<string> {
        const pipeName = PipeTransport.getPipeName(createPipeName(uri));
        const pipe = this.pipeFactory();
        try {
            await pipe.connect(pipeName);
            return await pipe.readLine();
        } finally {
            pipe.disconnect();
        }
    }

    private static getPipeName(name: string): string {
        return `\\\\.\\pipe\\${name}`;
    }
}