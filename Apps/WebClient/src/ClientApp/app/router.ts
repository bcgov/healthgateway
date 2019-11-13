import Vue from "vue";

// Routes
import VueRouter from "vue-router";
import store from "./store/store";
import ProfileComponent from "@/views/profile.vue";
import CardInfoComponent from "@/views/cardInfo.vue";
import LandingComponent from "@/views/landing.vue";
import NotFoundComponent from "@/views/errors/notFound.vue";
import LoginComponent from "@/views/login.vue";
import LogoutComponent from "@/views/logout.vue";
import UnauthorizedComponent from "@/views/errors/unauthorized.vue";
import LoginCallback from "@/views/loginCallback.vue";
import RegistrationComponent from "@/views/registration.vue";
import RegistrationInfoComponent from "@/views/registrationInfo.vue";
import TimelineComponent from "@/views/timeline.vue";

Vue.use(VueRouter);

const routes = [
  {
    path: "/",
    component: LandingComponent,
    meta: { requiresAuth: false }
  },
  {
    path: "/registrationInfo",
    component: RegistrationInfoComponent,
    meta: { requiresAuth: true }
  },
  {
    path: "/registration",
    component: RegistrationComponent,
    meta: { requiresAuth: true }
  },
  {
    path: "/profile",
    component: ProfileComponent,
    meta: { requiresRegistration: true, roles: ["user"] }
  },
  {
    path: "/cardInfo",
    component: CardInfoComponent,
    meta: { requiresAuth: false, roles: ["user"] }
  },
  {
    path: "/timeline",
    component: TimelineComponent,
    meta: { requiresRegistration: true, roles: ["user"] }
  },
  {
    path: "/login",
    component: LoginComponent,
    meta: { requiresAuth: false, roles: ["user"] }
  },
  {
    path: "/loginCallback",
    component: LoginCallback,
    meta: { requiresAuth: false, roles: ["user"], routeIsOidcCallback: true }
  },
  {
    path: "/logout",
    component: LogoutComponent,
    meta: { requiresAuth: false }
  },
  {
    path: "/unauthorized",
    component: UnauthorizedComponent,
    meta: { requiresAuth: false }
  }, // Unauthorized
  { path: "/*", component: NotFoundComponent } // Not found; Will catch all other paths not covered previously
];

const router = new VueRouter({
  mode: "history",
  routes
});

router.beforeEach(async (to, from, next) => {
  console.log(to.path);
  if (!to.meta.requiresAuth && !to.meta.requiresRegistration) {
    // Route does not have any special authorization requirements
    next();
    return;
  }

  store.dispatch("auth/oidcCheckAccess", to).then(hasAccess => {
    if (!hasAccess) {
      // If the user is not authenticated re-route to login
      next({ path: "/login", query: { redirect: to.path } });
      return;
    }
    let userIsRegistered: boolean = store.getters["auth/userIsRegistered"];
    if (
      userIsRegistered &&
      (to.path === "/registration" || to.path === "/registrationInfo")
    ) {
      // If the user is registerd and is attempting to go to registration re-route to timeline
      next({ path: "/timeline" });
      return;
    }

    if (to.meta.requiresRegistration && !userIsRegistered) {
      next({ path: "/unauthorized" });
      return;
    }

    next();
  });
});

export default router;
