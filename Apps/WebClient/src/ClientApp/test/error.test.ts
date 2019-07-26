import { mount, shallowMount } from '@vue/test-utils'
import NotFoundView from '@/views/errors/notFound.vue'
import UnauthorizedView from '@/views/errors/unauthorized.vue'
import ErrorComponent from '@/components/error.vue'
import PageError from '@/models/pageError'

describe('ErrorComponent', () => {
    test('renders properties correctly', () => {
        let error: PageError = new PageError('777', 'ERROR_NAME', 'ERROR_MESSAGE')
        const wrapper = shallowMount(ErrorComponent, { propsData: { error } });

        expect(wrapper.find("h1").text()).toBe(error.code);
        expect(wrapper.find("h2").text()).toBe(error.name);
        expect(wrapper.find("p").text()).toBe(error.message);
    });
});

describe('NotFoundView', () => {
    test('renders properties correctly', () => {
        const mountWrapper = mount(NotFoundView);
        let errorDescription = mountWrapper.vm.$data.errorDescription;
        expect(mountWrapper.find("h1").text()).toBe(errorDescription.code);
        expect(mountWrapper.find("h2").text()).toBe(errorDescription.name);
        expect(mountWrapper.find("p").text()).toBe(errorDescription.message);
    });
});

describe('UnauthorizedView', () => {
    test('renders properties correctly', () => {
        const mountWrapper = mount(UnauthorizedView);
        let errorDescription = mountWrapper.vm.$data.errorDescription;
        expect(mountWrapper.find("h1").text()).toBe(errorDescription.code)
        expect(mountWrapper.find("h2").text()).toBe(errorDescription.name)
        expect(mountWrapper.find("p").text()).toBe(errorDescription.message)
    })
})
