import { MessageTransform } from "./MessageTransform";
import { TransformCallback } from "stream";

// interface IncompleteBuffer {
//     buffer: Buffer;
//     size: number;
// }

interface BufferPosition {
    start: number;
    end: number;
}

export class BinaryTransform extends MessageTransform {
    private remaining: Buffer | null = null;

    public _transform(chunk: any, encoding: string, callback: TransformCallback): void {
        if (!this.enabled || !(chunk instanceof Buffer)) {
            this.push(chunk, encoding);
            callback();
            return;
        }

        let current: Buffer | null = this.remaining ? Buffer.concat([new Uint8Array(this.remaining), new Uint8Array(chunk)]) : chunk;
        const messagePositions = this.getMessagePositions(current);
        if(messagePositions.length <= 0) {
            
        } else {
            const end = messagePositions.pop()!.end;
            
            const processed = current.slice(0, end);
            this.push(processed);
            if(current.length > end - 1) {
                current = current.slice(end + 1, current.length - 1);
            } else {
                current = null;
            }
        }

        this.remaining = current;
        callback();
    }

    private getMessagePositions(chunk: Buffer): Array<BufferPosition> {
        const result: BufferPosition[] = [];
        const uint8Array = new Uint8Array(chunk);
        const maxLengthPrefixSize = 5;
        const numBitsToShift = [0, 7, 14, 21, 28];

        for (let offset = 0; offset < chunk.byteLength;) {
            let numBytes = 0;
            let size = 0;
            let byteRead;
            do {
                byteRead = uint8Array[offset + numBytes];
                size = size | ((byteRead & 0x7f) << (numBitsToShift[numBytes]));
                numBytes++;
            }
            while (numBytes < Math.min(maxLengthPrefixSize, chunk.byteLength - offset) && (byteRead & 0x80) !== 0);

            if ((byteRead & 0x80) !== 0 && numBytes < maxLengthPrefixSize) {
                break;
            }

            if (numBytes === maxLengthPrefixSize && byteRead > 7) {
                break;
            }

            if (uint8Array.byteLength >= (offset + numBytes + size)) {
                result.push({ start: offset + numBytes, end: offset + numBytes + size });
            } else {
                break;
            }

            offset = offset + numBytes + size;
        }

        return result;
    }

    // private getNextMessageWindow(chunk: Buffer): number {
    //     const uint8Array = new Uint8Array(chunk);
    //     const maxLengthPrefixSize = 5;
    //     const numBitsToShift = [0, 7, 14, 21, 28];

    //     let numBytes = 0;
    //     let size = 0;
    //     let byteRead;
    //     do {
    //         byteRead = uint8Array[numBytes];
    //         size = size | ((byteRead & 0x7f) << (numBitsToShift[numBytes]));
    //         numBytes++;
    //     }
    //     while (numBytes < Math.min(maxLengthPrefixSize, chunk.byteLength) && (byteRead & 0x80) !== 0);

    //     if ((byteRead & 0x80) !== 0 && numBytes < maxLengthPrefixSize) {
    //         return -1;
    //     }

    //     console.log("NUM BYTES: " + numBytes);
    //     console.log("SIZE: " + size);
    //     return numBytes + size;
    // }
}