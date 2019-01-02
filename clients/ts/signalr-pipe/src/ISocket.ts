import { Duplex } from "stream";

export interface ISocket {
    //onreceive: ((data: string | ArrayBuffer) => void) | null;
    onclose: ((error?: Error) => void) | null;

    readonly stream: Duplex;

    connect(path: string): Promise<void>;
    send(data: string | ArrayBuffer): Promise<void>;
    readLine(): Promise<string>;
    readString(): Promise<string>;
   // handshakeDone(): void;
    disconnect(): void;
}