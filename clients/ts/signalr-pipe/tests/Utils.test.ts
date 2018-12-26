import { createPipeName } from "../src/Util";

describe("PipeUriGenerator", () => {
    it("Should generate pipe name", () => {
        const result = createPipeName(new URL("signalr.pipe://testhost/testpath"));
        expect(result).toBe("signalr.pipe_Ec2lnbmFsci5waXBlOi8vVEVTVEhPU1QvVEVTVFBBVEgv");
    });
    it("Should generate pipename for long uri", () => {
        const result = createPipeName(new URL("signalr.pipe://testhost/testpath/testpath2/testpath3/testpath4/testpath5/testpath6/testpath7/testpath8/testpath9/testpath10/testpath11"));
        expect(result).toBe("signalr.pipe_HfsmwH8G6TwiLVM2EIplII2CvOHs=");
    });
});
