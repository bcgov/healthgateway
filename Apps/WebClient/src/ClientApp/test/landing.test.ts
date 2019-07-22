import { shallowMount, createLocalVue } from '@vue/test-utils'
import LandingComponent from '@/views/landing.vue'

describe('Landing view', () => {
  const localVue = createLocalVue();
  const wrapper = shallowMount(LandingComponent, { localVue });

  test('is a Vue instance', () => {
    expect(wrapper.isVueInstance()).toBeTruthy();
  })
});