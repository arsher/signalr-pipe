import { ISocket } from "./ISocket";
import { Socket } from "net";
import { Readable, Duplex } from "stream";
import { createInterface } from "readline";
import { Arg } from "./Util";
import { TextTransform } from "./TextTransform";

export class PipeSocket implements ISocket {
    private readonly socket: Socket;
    private lastError?: Error;
    private disposed: boolean = false;

    get stream(): Duplex {
        return this.socket;
    }

    public onclose: ((error?: Error | undefined) => void) | null;

    constructor(socketFactory: () => Socket) {
        Arg.isRequired(socketFactory, "socketFactory");

        this.socket = socketFactory();
        this.socket.on("close", this.onSocketClosed.bind(this));
        this.socket.on("error", this.onSocketError.bind(this));

        this.onclose = null;
    }

    public connect(path: string): Promise<void> {
        const result = new Promise<void>((resolve, reject) => {
            if (!this.rejectIfDisposed(reject)) {
                let closeHandler: ((hadError: boolean) => void) | null = null;
                closeHandler = (_hadError: boolean) => {
                    this.socket.removeListener("close", closeHandler!);
                    reject(this.lastError);
                };

                this.socket.on("close", closeHandler);
                this.socket.connect({ path: path }, () => {
                    this.socket.removeListener("close", closeHandler!);
                    resolve();
                });
            }
        });
        return result;
    }

    public send(data: string | ArrayBuffer): Promise<void> {
        const result = new Promise<void>((resolve, reject) => {
            if (!this.rejectIfDisposed(reject)) {
                let closeHandler: ((hadError: boolean) => void) | null = null;
                closeHandler = (_hadError: boolean) => {
                    this.socket.removeListener("close", closeHandler!);
                    reject(this.lastError);
                };

                let bufferData: Buffer;
                if (typeof data === "string") {
                    bufferData = Buffer.from(data);
                } else {
                    bufferData = Buffer.from(new Uint8Array(data));
                }

                this.socket.on("close", closeHandler);
                this.socket.write(bufferData, () => {
                    this.socket.removeListener("close", closeHandler!);
                    resolve();
                });
            }
        });
        return result;
    }

    public async readLine(): Promise<string> {
        const result = new Promise<string>((resolve, reject) => {
            if (!this.rejectIfDisposed(reject)) {
                let closeHandler: ((hadError: boolean) => void) | null = null;
                closeHandler = (_hadError: boolean) => {
                    this.socket.removeListener("close", closeHandler!);
                    reject(this.lastError);
                };

                this.socket.on("close", closeHandler);
                const readable = new Readable().wrap(this.socket);
                const rl = createInterface(readable);
                rl.on("line", (line) => {
                    rl.close();
                    this.socket.removeListener("close", closeHandler!);
                    resolve(line);
                });
            }
        });
        return result;
    }

    public async readString(): Promise<string> {
        return new Promise<string>((resolve, reject) => {
            if (!this.rejectIfDisposed(reject)) {
                let closeHandler: ((hadError: boolean) => void) | null = null;
                closeHandler = (_hadError: boolean) => {
                    this.socket.removeListener("close", closeHandler!);
                    reject(this.lastError);
                };

                this.socket.on("close", closeHandler);
                this.socket.pipe(new TextTransform()).on("data", d =>{
                    this.socket.unpipe();
                    resolve(d);
                });
            }
        });
    }

    public disconnect(): void {
        if (this.disposed) {
            return;
        }

        this.disposed = true;

        this.socket.destroy();

        this.onclose = null;

        this.socket.removeAllListeners("close");
        this.socket.removeAllListeners("error");
        this.socket.unref();
    }

    private rejectIfDisposed(reject: (reason?: any) => void): boolean {
        if (this.disposed) {
            reject(new Error("Already disposed."));
            return true;
        }

        return false;
    }

    private onSocketError(error?: Error): void {
        this.lastError = error;
    }

    private onSocketClosed(_hadError: boolean): void {
        if (this.onclose) {
            this.onclose(this.lastError);
        }

        this.disconnect();
    }
}