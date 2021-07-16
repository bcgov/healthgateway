import BootstrapVue, { IconsPlugin } from "bootstrap-vue";
import Vue from "vue";

import { SnowplowWindow } from "@/plugins/extensions";

import { voidMethod } from "./stubs/util";

import { enableFetchMocks } from "jest-fetch-mock";
enableFetchMocks();

Vue.use(BootstrapVue);
Vue.use(IconsPlugin);

declare let window: SnowplowWindow;
window.snowplow = voidMethod;
