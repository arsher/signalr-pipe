module.exports = {
    globals: {
        "ts-jest": {
            "tsConfig": "../tsconfig.jest.json",
            "diagnostics": {
                warnOnly: true
            }
        }
    },
    reporters: [
        "default",
        ["../common/node_modules/jest-junit/index.js", { "output": "../../../artifacts/test-results/" + `${process.platform}` + ".node.functional.junit.xml" }]
    ],
    transform: {
        "^.+\\.(jsx?|tsx?)$": "../common/node_modules/ts-jest"
    },
    testEnvironment: "node",
    testRegex: "(Tests)\\.(jsx?|tsx?)$",
    moduleNameMapper: {
        "^ts-jest$": "<rootDir>/../common/node_modules/ts-jest",
        "^signalr-pipe$": "<rootDir>/../signalr-pipe/dist/cjs/index.js"
    },
    moduleFileExtensions: [
        "ts",
        "tsx",
        "js",
        "jsx",
        "json",
        "node"
    ]
};