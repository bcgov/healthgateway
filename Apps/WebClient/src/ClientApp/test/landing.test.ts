import { shallowMount, createLocalVue } from '@vue/test-utils'
import LandingComponent from '@/views/landing.vue'

describe('LandingComponent', () => {
  test('is a Vue instance', () => {
    const localVue = createLocalVue()
    const wrapper = shallowMount(LandingComponent, { localVue });
    expect(wrapper.isVueInstance()).toBeTruthy()
  })
})