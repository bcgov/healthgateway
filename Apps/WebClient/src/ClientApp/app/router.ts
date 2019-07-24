import Vue from 'vue'
import VueRouter from 'vue-router'

import { IAuthenticationService } from '@/services/interfaces'
import SERVICE_IDENTIFIER from "@/constants/serviceIdentifiers";
import container from "./inversify.config";

// Routes
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
    { path: '/notFound', NotFoundComponent },
    { path: '/Auth/Login' },
    { path: '*', redirect: '/notFound' } // Not found; Will catch all other paths not covered previously
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

        const authService: IAuthenticationService = container.get<IAuthenticationService>(SERVICE_IDENTIFIER.AuthenticationService);
        authService.getAuthentication().then(authData => {
            console.log(authData.token);

            if (!authData.isAuthenticated) {
                //security.init(next, to.meta.roles)
                console.log('Not authenticated');
                authService.startLoginFlow('aHint', to.path);
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
        }).catch(something => { 
            console.log('ERROR HERE' + something);
     });
    }
    else {
        next()
    }
})

export default router