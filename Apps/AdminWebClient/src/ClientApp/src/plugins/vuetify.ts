import "@mdi/font/css/materialdesignicons.css";
import Vue from "vue";
import Vuetify from "vuetify/lib";

Vue.use(Vuetify);

export default new Vuetify({
    iconfont: "mdi", // 'md' || 'mdi' || 'fa' || 'fa4'
    theme: { dark: true }
});
