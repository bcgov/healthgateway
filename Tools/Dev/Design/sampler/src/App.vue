<template>
    <div id="app">
        <h1>Welcome to the HG Sampler!</h1>
        <Samples />
    </div>
</template>

<script lang="ts">
// Load Bootstrap general plugins
import {
    AlertPlugin,
    ButtonPlugin,
    CardPlugin,
    FormCheckboxPlugin,
    FormDatepickerPlugin,
    FormGroupPlugin,
    FormInputPlugin,
    FormPlugin,
    FormRadioPlugin,
    FormRatingPlugin,
    FormSelectPlugin,
    FormTextareaPlugin,
    IconsPlugin,
    InputGroupPlugin,
    LayoutPlugin,
    LinkPlugin,
    ModalPlugin,
    NavbarPlugin,
    NavPlugin,
    PaginationNavPlugin,
    SpinnerPlugin,
    TooltipPlugin,
} from "bootstrap-vue";
import { Component, Vue } from "vue-property-decorator";

import Samples from "./components/Samples.vue";
Vue.use(LayoutPlugin);
Vue.use(NavPlugin);
Vue.use(NavbarPlugin);
Vue.use(ModalPlugin);
Vue.use(ButtonPlugin);
Vue.use(CardPlugin);
Vue.use(LinkPlugin);
Vue.use(FormPlugin);
Vue.use(FormInputPlugin);
Vue.use(FormCheckboxPlugin);
Vue.use(FormTextareaPlugin);
Vue.use(AlertPlugin);
Vue.use(SpinnerPlugin);
Vue.use(InputGroupPlugin);
Vue.use(PaginationNavPlugin);
Vue.use(TooltipPlugin);
Vue.use(FormGroupPlugin);
Vue.use(FormRadioPlugin);
Vue.use(FormRatingPlugin);
Vue.use(FormSelectPlugin);
Vue.use(FormDatepickerPlugin);
Vue.use(IconsPlugin);

// Load general icons
import { config, library } from "@fortawesome/fontawesome-svg-core";
// Prevent auto adding CSS to the header since that breaks Content security policies.
config.autoAddCss = false;
// Add font awesome styles manually
import "@fortawesome/fontawesome-svg-core/styles.css";

import {
    faAddressCard,
    faAngleDoubleLeft,
    faChartBar,
    faChartLine,
    faCheckCircle,
    faChevronDown,
    faChevronLeft,
    faChevronRight,
    faChevronUp,
    faCircle,
    faClipboardList,
    faCommentAlt,
    faEdit,
    faEllipsisV,
    faExclamationTriangle,
    faFileAlt,
    faFlask,
    faLock,
    faPills,
    faPrint,
    faSpinner,
    faStream,
    faSyringe,
    faTimes,
    faTimesCircle,
    faUser,
    faUserCircle,
    faUserFriends,
    faUserMd,
    faUserPlus,
    faUserSecret,
} from "@fortawesome/free-solid-svg-icons";
library.add(
    faAddressCard,
    faAngleDoubleLeft,
    faChartBar,
    faChartLine,
    faCheckCircle,
    faChevronDown,
    faChevronLeft,
    faChevronRight,
    faChevronUp,
    faCircle,
    faClipboardList,
    faCommentAlt,
    faEdit,
    faEllipsisV,
    faExclamationTriangle,
    faFileAlt,
    faFlask,
    faLock,
    faPills,
    faPrint,
    faSpinner,
    faStream,
    faSyringe,
    faTimes,
    faTimesCircle,
    faUser,
    faUserCircle,
    faUserFriends,
    faUserMd,
    faUserPlus,
    faUserSecret
);

@Component({
    components: {
        Samples,
    },
})
export default class App extends Vue {}
</script>
