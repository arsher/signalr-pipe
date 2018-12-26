module.exports = {
    globals: {
        "ts-jest": {
            "tsConfig": "./tsconfig.jest.json",
            "diagnostics": {
                warnOnly: true
            }
        }
    },
    transform: {
        "^.+\\.tsx?$": "./common/node_modules/ts-jest"
    },
    testEnvironment: "node",
    testRegex: "(/__tests__/.*|(\\.|/)(test|spec))\\.(jsx?|tsx?)$",
    moduleNameMapper: {
        "^ts-jest$": "<rootDir>/common/node_modules/ts-jest",
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