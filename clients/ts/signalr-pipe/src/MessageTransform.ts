import { Transform } from "stream";

export abstract class MessageTransform extends Transform {
    public enabled: boolean;

    constructor() {
        super();

        this.enabled = false;
    }
}