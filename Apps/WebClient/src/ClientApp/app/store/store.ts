import Vue from 'vue';
import Vuex, { StoreOptions } from 'vuex';
import { auth } from './modules/auth/auth';
import { RootState } from '@/models/authState'

Vue.use(Vuex);

const store: StoreOptions<RootState> = {
    state: {
        version: '1.0.0' // a simple property
    },
    modules: {
        auth
    }
};

export default new Vuex.Store<RootState>(store);