import Vue from 'vue'
import VueRouter from 'vue-router'
import HomeComponent from './views/home.vue'
import RegistrationComponent from './views/registration.vue'
import LandingComponent from './views/landing.vue'
import ImmunizationsComponent from './views/immunizations.vue'
import NotFoundComponent from './views/errors/notFound.vue'
import LogoutComponent from './views/logout.vue'
import UnauthorizedComponent from './views/errors/unauthorized.vue'

Vue.use(VueRouter)

const routes = [
    { path: '/', component: LandingComponent },
    { path: '/registration', component: RegistrationComponent },
    {
        path: '/home',
        component: HomeComponent,
        meta: { requiresAuth: true, roles: ['user'] },
        children: [
            { path: '/immunizations', component: ImmunizationsComponent, meta: { requiresAuth: true, roles: ['user'] } }
        ]
    },
    {
        path: '/logout',
        component: LogoutComponent,
        meta: { requiresAuth: true, roles: ['user'] }
    },
    { path: '/unauthorized', component: UnauthorizedComponent }, // Unauthorized
    { path: '*', component: NotFoundComponent } // Not found; Will catch all other paths not covered previously
]

const router = new VueRouter({
    mode: 'history',
    routes
})

router.beforeEach((to, from, next) => {
    if (to.meta.requiresAuth) {
        //const auth = store.state.security.auth
        const auth = { authenticated: false };//store.state.security.auth
        console.log('Requires auth!');
        if (!auth.authenticated) {
            //security.init(next, to.meta.roles)
            next({ name: 'unauthenticated' })
            console.log('Not authenticated');
        }
        else {
            console.log('Authenticated');
            if (to.meta.roles) {
                /*if (security.roles(to.meta.roles[0])) {
                    next()
                }
                else*/ {
                    next({ name: 'unauthorized' })
                }
            }
            else {
                next()
            }
        }
    }
    else {
        next()
    }
})

export default router