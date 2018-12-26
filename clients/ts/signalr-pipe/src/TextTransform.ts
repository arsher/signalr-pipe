import { MessageTransform } from "./MessageTransform";
import { TransformCallback } from "stream";

export class TextTransform extends MessageTransform {
    public static RecordSeparatorCode = 0x1e;
    public static RecordSeparator = String.fromCharCode(TextTransform.RecordSeparatorCode);

    private remaining: Buffer | null = null;

    constructor() {
        super();
    }

    public _transform(chunk: any, encoding: string, callback: TransformCallback): void {
        if (!this.enabled || !(chunk instanceof Buffer)) {
            this.push(chunk, encoding);
            callback();
            return;
        }

        const lastIndexOfSeparator = chunk.lastIndexOf(TextTransform.RecordSeparator);
        if (lastIndexOfSeparator >= 0) {
            let currentUntilLastSeparator = chunk;
            if (chunk.length > lastIndexOfSeparator + 1) {
                currentUntilLastSeparator = chunk.slice(0, lastIndexOfSeparator + 1);
            }

            if (this.remaining) {
                currentUntilLastSeparator = Buffer.concat([this.remaining, currentUntilLastSeparator]);
                this.remaining = null;
            }

            if (chunk.length > lastIndexOfSeparator + 1) {

                this.remaining = (chunk.slice(lastIndexOfSeparator + 1));
            }

            this.push(currentUntilLastSeparator, encoding);

            callback();
        } else {
            if (this.remaining !== null) {
                this.remaining = Buffer.concat([this.remaining, chunk]);
            } else {
                this.remaining = chunk;
            }
            callback();
        }
    }
}