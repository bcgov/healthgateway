import Vue from 'vue';
import Home from './views/Home.vue';
import Dashboard from './views/Dashboard.vue';
import VueRouter from 'vue-router';
import { stringify } from 'querystring';

Vue.use(VueRouter);

const router = new VueRouter({
  mode: 'history',
  base: process.env.BASE_URL,
  routes: [
    {
      path: '/',
      name: 'home',
      component: Home,

    },
    {
      path: '/dashboard',
      name: 'dashboard',
      // component: Dashboard,
      // route level code-splitting
      // this generates a separate chunk (about.[hash].js) for this route
      // which is lazy-loaded when the route is visited.
      // component: () => import(/* webpackChunkName: "dashboard" */ './views/Dashboard.vue'),
      component: Dashboard,
    },
    { path: '*', redirect: '/' },
  ],
  });

  // const auth: string = Vue.prototype.$http.defaults.headers.common.Authorization;

export default router;


