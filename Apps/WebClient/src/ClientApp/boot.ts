import Vue from 'vue'
import VueRouter from 'vue-router'
import '@fortawesome/fontawesome-free/js/fontawesome'
import '@fortawesome/fontawesome-free/js/solid'
import '@fortawesome/fontawesome-free/js/regular'
import '@fortawesome/fontawesome-free/js/brands'
import './css/site.css';
import 'bootstrap-vue/dist/bootstrap-vue.css'
import BootstrapVue from 'bootstrap-vue';
import i18n from './i18n';

Vue.use(BootstrapVue);
Vue.use(VueRouter)

import App from './components/app/app.vue';

// Require does not work correctly, for now use import
import home from './components/home/home.vue.html';
import counter from './components/counter/counter.vue.html';
import fetchdata from './components/fetchdata/fetchdata.vue.html';
import immunizations from './components/immunizations/immunizations.vue.html';
import registration from './components/registration/registration.vue.html';

const routes = [
    { path: '/', component: home },
    { path: '/counter', component: counter },
    { path: '/fetchdata', component: fetchdata },
    { path: '/immunizations', component: immunizations },
    { path: '/registration', component: registration }
];

new Vue({
    el: '#app-root',
    i18n: i18n,
    router: new VueRouter({ mode: 'history', routes: routes }),
    render: h => h(App)
});