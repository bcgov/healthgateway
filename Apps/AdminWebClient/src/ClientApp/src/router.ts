import Vue from "vue";
import Dashboard from "@/views/Dashboard.vue";
import BetaQueue from "@/views/BetaQueue.vue";
import VueRouter from "vue-router";
import FeedbackView from '@/views/Feedback.vue';

Vue.use(VueRouter);

const routes = [
  {
    path: "/",
    name: "dashboard",
    component: Dashboard
  },
  {
    path: "/hangfire",
    name: "hangfire",
    component: Dashboard
  },
  {
    path: "/beta-invites",
    name: "beta-invites",
    component: BetaQueue
  },
  {
    path: "/user-feedback",
    name: "user-feedback",
    component: FeedbackView
  },
  { path: "*", redirect: "/" }
];

const router = new VueRouter({
  mode: "history",
  base: process.env.BASE_URL,
  routes
});

router.beforeEach(async (to, from, next) => {
  console.log(to.fullPath);
  next();
});

export default router;
