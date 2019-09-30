import Vue from "vue";

// Routes
import VueRouter from "vue-router";
import store from "./store/store";
import HomeComponent from "@/views/home.vue";
import CardInfoComponent from "@/views/cardInfo.vue";
import LandingComponent from "@/views/landing.vue";
import ImmunizationsComponent from "@/views/immunizations.vue";
import NotFoundComponent from "@/views/errors/notFound.vue";
import LoginComponent from "@/views/login.vue";
import LogoutComponent from "@/views/logout.vue";
import UnauthorizedComponent from "@/views/errors/unauthorized.vue";
import LoginCallback from "@/views/loginCallback.vue";
import RegistrationComponent from "@/views/registration.vue";
import TimelineComponent from "@/views/timeline.vue";

Vue.use(VueRouter);

const routes = [
  {
    path: "/",
    component: LandingComponent,
    meta: { requiresAuth: false }
  },
  {
    path: "/registration",
    component: RegistrationComponent,
    meta: { requiresAuth: true }
  },
  {
    path: "/home",
    component: HomeComponent,
    meta: { requiresRegistration: true, roles: ["user"] }
  },
  {
    path: "/cardInfo",
    component: CardInfoComponent,
    meta: { requiresAuth: false, roles: ["user"] }
  },
  {
    path: "/immunizations",
    component: ImmunizationsComponent,
    meta: { requiresRegistration: true, roles: ["user"] }
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
  if (to.meta.requiresAuth || to.meta.requiresRegistration) {
    store.dispatch("auth/oidcCheckAccess", to).then(hasAccess => {
      if (!hasAccess) {
        next({ path: "/login", query: { redirect: to.path } });
      } else {
        let userIsRegistered: boolean = store.getters["auth/userIsRegistered"];
        // If the user is registerd and is attempting to go to registration re-route
        if (userIsRegistered && to.path === "/registration") {
          next({ path: "/home" });
        } else if (to.meta.requiresRegistration && !userIsRegistered) {
          next({ path: "/unauthorized" });
        } else {
          /*if (to.meta.roles) {
                /*if (security.roles(to.meta.roles[0])) {
                    next()
                }
                else {
                    next({ name: 'unauthorized' })
                }
            }
            else {
                next()
            } */
          next();
        }
      }
    });
  } else {
    next();
  }
});

export default router;
