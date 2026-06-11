
const configUtils = require("X:/MxBuild/win-mxbuild-10.24.4/modeler/tools/node/node_modules/@react-native/metro-config/dist");
const path = require("path");

const metroConfig = {
    watchFolders: [
        path.resolve(__dirname, "X:/_build/28/s/OpcenterEXSM_2510/theme"),
        path.resolve(__dirname, "X:/_build/28/s/OpcenterEXSM_2510/javascriptsource"),
        path.resolve(__dirname, "X:/_build/28/s/OpcenterEXSM_2510/themesource"),
        path.resolve(__dirname, "X:/MxBuild/win-mxbuild-10.24.4/modeler/tools/node/node_modules"),
    ],
    resolver: {
        useWatchman: true,
        platforms: ["ios", "android"],
        sourceExts: ["native.js", "js", "jsx", "ts", "tsx", "cjs", "mjs", "json", "js_commonjs-exports", "js_commonjs-module"],
        extraNodeModules: {
            "@babel/runtime": "X:/MxBuild/win-mxbuild-10.24.4/modeler/tools/node/node_modules/@babel/runtime",
            "big.js": "X:/MxBuild/win-mxbuild-10.24.4/modeler/tools/node/node_modules/big.js",
            "react": "X:/MxBuild/win-mxbuild-10.24.4/modeler/tools/node/node_modules/react",
            "react-dom": "X:/MxBuild/win-mxbuild-10.24.4/modeler/tools/node/node_modules/react-dom",
            "react-native-gesture-handler": "X:/MxBuild/win-mxbuild-10.24.4/modeler/tools/node/node_modules/react-native-gesture-handler",
            "react-native": "X:/MxBuild/win-mxbuild-10.24.4/modeler/tools/node/node_modules/react-native",
            "@react-native-community/cli": "X:/MxBuild/win-mxbuild-10.24.4/modeler/tools/node/node_modules/@react-native-community/cli",
            "@react-native-community/cli-platform-android": "X:/MxBuild/win-mxbuild-10.24.4/modeler/tools/node/node_modules/@react-native-community/cli-platform-android",
            "@react-native-community/cli-platform-ios": "X:/MxBuild/win-mxbuild-10.24.4/modeler/tools/node/node_modules/@react-native-community/cli-platform-ios",
            "react-native-device-info": "X:/MxBuild/win-mxbuild-10.24.4/modeler/tools/node/node_modules/react-native-device-info",
            "react-native-material-menu": "X:/MxBuild/win-mxbuild-10.24.4/modeler/tools/node/node_modules/react-native-material-menu",
            "@react-navigation/bottom-tabs": "X:/MxBuild/win-mxbuild-10.24.4/modeler/tools/node/node_modules/@react-navigation/bottom-tabs",
            "@react-navigation/core": "X:/MxBuild/win-mxbuild-10.24.4/modeler/tools/node/node_modules/@react-navigation/core",
            "@react-navigation/drawer": "X:/MxBuild/win-mxbuild-10.24.4/modeler/tools/node/node_modules/@react-navigation/drawer",
            "@react-navigation/native": "X:/MxBuild/win-mxbuild-10.24.4/modeler/tools/node/node_modules/@react-navigation/native",
            "@react-navigation/stack": "X:/MxBuild/win-mxbuild-10.24.4/modeler/tools/node/node_modules/@react-navigation/stack",
            "react-native-svg": "X:/MxBuild/win-mxbuild-10.24.4/modeler/tools/node/node_modules/react-native-svg",
            "react-native-tab-view": "X:/MxBuild/win-mxbuild-10.24.4/modeler/tools/node/node_modules/react-native-tab-view",
            "react-native-vector-icons": "X:/MxBuild/win-mxbuild-10.24.4/modeler/tools/node/node_modules/react-native-vector-icons",
            "react-native-fast-image": "X:/MxBuild/win-mxbuild-10.24.4/modeler/tools/node/node_modules/react-native-fast-image",
            "react-native-screens": "X:/MxBuild/win-mxbuild-10.24.4/modeler/tools/node/node_modules/react-native-screens",
            "react-native-localize": "X:/MxBuild/win-mxbuild-10.24.4/modeler/tools/node/node_modules/react-native-localize",
            "react-native-reanimated": "X:/MxBuild/win-mxbuild-10.24.4/modeler/tools/node/node_modules/react-native-reanimated",
            "react-native-safe-area-context": "X:/MxBuild/win-mxbuild-10.24.4/modeler/tools/node/node_modules/react-native-safe-area-context",
            "react-native-blob-util": "X:/MxBuild/win-mxbuild-10.24.4/modeler/tools/node/node_modules/react-native-blob-util",
            "@react-native-async-storage/async-storage": "X:/MxBuild/win-mxbuild-10.24.4/modeler/tools/node/node_modules/@react-native-async-storage/async-storage",
            "@react-native-community/datetimepicker": "X:/MxBuild/win-mxbuild-10.24.4/modeler/tools/node/node_modules/@react-native-community/datetimepicker",
            "@react-native-masked-view/masked-view": "X:/MxBuild/win-mxbuild-10.24.4/modeler/tools/node/node_modules/@react-native-masked-view/masked-view",
            "eventemitter3": "X:/MxBuild/win-mxbuild-10.24.4/modeler/tools/node/node_modules/eventemitter3",
            "@react-native-picker/picker": "X:/MxBuild/win-mxbuild-10.24.4/modeler/tools/node/node_modules/@react-native-picker/picker",
            "deprecated-react-native-prop-types": "X:/MxBuild/win-mxbuild-10.24.4/modeler/tools/node/node_modules/deprecated-react-native-prop-types",
            "metro-file-map": "X:/MxBuild/win-mxbuild-10.24.4/modeler/tools/node/node_modules/metro-file-map",
            "@react-native/metro-config": "X:/MxBuild/win-mxbuild-10.24.4/modeler/tools/node/node_modules/@react-native/metro-config",
            "@rollup/plugin-alias": "X:/MxBuild/win-mxbuild-10.24.4/modeler/tools/node/node_modules/@rollup/plugin-alias",
            "mendix": "X:/MxBuild/win-mxbuild-10.24.4/modeler/tools/node/node_modules/mendix",
            "mx-global": "X:/MxBuild/win-mxbuild-10.24.4/modeler/tools/node/node_modules/mx-global",
            "mx-api": "X:/MxBuild/win-mxbuild-10.24.4/modeler/tools/node/node_modules/mendix/mx-api",
            "mx-api/data": "X:/MxBuild/win-mxbuild-10.24.4/modeler/tools/node/node_modules/mendix/mx-api/data",
            "mx-api/session": "X:/MxBuild/win-mxbuild-10.24.4/modeler/tools/node/node_modules/mendix/mx-api/session",
            "mx-api/ui": "X:/MxBuild/win-mxbuild-10.24.4/modeler/tools/node/node_modules/mendix/mx-api/ui"
        }
    },
    cacheVersion: "77222",
};

module.exports = configUtils.mergeConfig(configUtils.getDefaultConfig(__dirname), metroConfig);
