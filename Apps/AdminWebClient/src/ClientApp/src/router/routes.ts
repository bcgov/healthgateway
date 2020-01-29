/**
 * Define all of your application routes here
 * for more information on routes, see the
 * official documentation https://router.vuejs.org/en/
 */
import Home from '@/views/Home.vue';
import Dashboard from '@/views/Dashboard.vue';

export default [
  {
    path: '',
    // Relative to /src/views
    view: 'Dashboard',
  },
  {
    path: '/user-profile',
    name: 'User Profile',
    view: 'UserProfile',
  },
  {
    path: '/table-list',
    name: 'Table List',
    view: 'TableList',
  },
  {
    path: '/notifications',
    view: 'Notifications',
  },
  { path: '*', redirect: '/' },
];
