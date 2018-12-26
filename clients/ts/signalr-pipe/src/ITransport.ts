export interface ITransport {
    connect(url: URL): Promise<void>;
    send(data: string | ArrayBuffer): Promise<void>;
    stop(): Promise<void>;
    onreceive: ((data: string | ArrayBuffer) => void) | null;
    onclose: ((error?: Error) => void) | null;
}
