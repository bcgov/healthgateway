import Vue from 'vue';
import Dashboard from './views/Dashboard.vue';
import VueRouter from 'vue-router';

Vue.use(VueRouter);

const router = new VueRouter({
  mode: 'history',
  base: process.env.BASE_URL,
  routes: [
    {
      path: '/',
      name: 'dashboard',
      component: Dashboard,
    },
    {
      path: '/hangfire',
      name: 'hangfire',
      component: Dashboard,
    },
    { path: '*', redirect: '/' },
  ],
  });

export default router;


