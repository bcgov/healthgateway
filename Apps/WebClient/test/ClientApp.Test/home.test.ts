import { shallowMount, createLocalVue } from '@vue/test-utils'
import HomeComponent from '../../src/ClientApp/views/home.vue'

describe('HomeComponent', () => {
  test('is a Vue instance', () => {
    const localVue = createLocalVue()
    const wrapper = shallowMount(HomeComponent, { localVue })
    expect(wrapper.isVueInstance()).toBeTruthy()
  })
})
