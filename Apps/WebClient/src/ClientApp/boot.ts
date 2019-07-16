import Vue from 'vue'
import VueRouter from 'vue-router'
import '@fortawesome/fontawesome-free/js/fontawesome'
import '@fortawesome/fontawesome-free/js/solid'
import '@fortawesome/fontawesome-free/js/regular'
import '@fortawesome/fontawesome-free/js/brands'
import './scss/bcgov/bootstrap-theme.scss';
import 'bootstrap-vue/dist/bootstrap-vue.css'
import BootstrapVue from 'bootstrap-vue'
import i18n from './i18n'

Vue.use(BootstrapVue);
Vue.use(VueRouter);

import App from './components/app/app.vue'

// Require does not work correctly, for now use import
import home from './components/home/home.vue'
import immunizations from './components/immunizations/immunizations.vue'
import registration from './components/registration/registration.vue'
import logout from './components/logout/logout.vue'

const routes = [
    { path: '/', component: home },
    { path: '/immunizations', component: immunizations },
    { path: '/registration', component: registration },
    { path: '/logout', component: logout }
];

new Vue({
    el: '#app-root',
    i18n: i18n,
    router: new VueRouter({ mode: 'history', routes: routes }),
    render: h => h(App)
});