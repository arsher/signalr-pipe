import { ChildProcess, exec, spawn } from "child_process";
import { EOL } from "os";
import * as path from "path";
import { Readable } from "stream";
import * as _fs from "fs";
import { promisify } from "util";

const ARTIFACTS_DIR = path.resolve(__dirname, "..", "..", "..", "..", "artifacts");
const LOGS_DIR = path.resolve(ARTIFACTS_DIR, "logs");

const fs = {
    createWriteStream: _fs.createWriteStream,
    exists: promisify(_fs.exists),
    mkdir: promisify(_fs.mkdir),
};

function runJest(): Promise<number> {
    const jestPath = path.resolve(__dirname, "..", "..", "common", "node_modules", "jest", "bin", "jest.js");
    const configPath = path.resolve(__dirname, "..", "func.jest.config.js");

    console.log("Starting Node tests using Jest.");

    // tslint:disable-next-line:variable-name
    return new Promise<number>((resolve, _reject) => {
        const logStream = fs.createWriteStream(path.resolve(LOGS_DIR, "node.functionaltests.log"));
        const p = exec(`"${process.execPath}" "${jestPath}" --config "${configPath}"`, {},
            // tslint:disable-next-line:variable-name
            (error: any, _stdout, _stderr) => {
                console.log("Finished Node tests.");
                if (error) {
                    console.log(error.message);
                    return resolve(error.code);
                }
                return resolve(0);
            });
        p.stdout.pipe(logStream);
        p.stderr.pipe(logStream);
    });
}

function waitForHostStarted(process: ChildProcess): Promise<void> {
    return new Promise<void>((resolve, reject) => {
        try {
            let lastLine = "";

            async function onData(this: Readable, chunk: string | Buffer): Promise<void> {
                try {
                    chunk = chunk.toString();

                    let lineEnd = chunk.indexOf(EOL);
                    while (lineEnd >= 0) {
                        const chunkLine = lastLine + chunk.substring(0, lineEnd);
                        lastLine = "";

                        chunk = chunk.substring(lineEnd + EOL.length);
                        if (chunk.trim() === "Hosting started") {
                            resolve();
                        }

                        lineEnd = chunk.indexOf(EOL);
                    }
                } catch (e) {
                    this.removeAllListeners("data");
                    reject(e);
                }
            }

            process.stdout.on("data", onData.bind(process.stdout));
            process.on("close", async (code, signal) => {
                global.process.exit(1);
            });
        } catch (e) {
            reject(e);
        }
    });
}

const configuration = "Debug";

(async () => {
    try {
        if (!await fs.exists(ARTIFACTS_DIR)) {
            await fs.mkdir(ARTIFACTS_DIR);
        }
        if (!await fs.exists(LOGS_DIR)) {
            await fs.mkdir(LOGS_DIR);
        }

        const serverPath = path.resolve(__dirname, "..", "app", "bin", configuration, "net461", "functional-tests.exe");
        const desiredServerUri = "signalr.pipe://testhost/testpath";

        const dotnetProcess = spawn(serverPath, ["--url", desiredServerUri]);

        function cleanup() {
            if (dotnetProcess && !dotnetProcess.killed) {
                console.log("Terminating dotnet process");
                dotnetProcess.kill();
            }
        }

        const logStream = fs.createWriteStream(path.resolve(LOGS_DIR, "ts.functionaltests.dotnet.log"));
        dotnetProcess.stdout.pipe(logStream);

        process.on("SIGINT", cleanup);
        process.on("exit", cleanup);

        console.log("Waiting for Functional Test Server to start");
        await waitForHostStarted(dotnetProcess);
        console.log("Functional Test Server has started");

        const jestExit = await runJest();

        process.exit(jestExit);
    } catch (e) {
        console.error(e);
        process.exit(1);
    }
})();
