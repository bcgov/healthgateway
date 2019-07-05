//import { mount } from '@vue/test-utils'
//import AppComponent from '../../src/ClientApp/components/app/app';
import sum from '../../src/ClientApp/sum';
// describe('Vue Components', () => {
//   test('AppComponent is a Vue instance', () => {
//     const wrapper = mount(AppComponent)
//     expect(wrapper.isVueInstance()).toBeTruthy()
//   })
// })
describe('Typescript Files', () => {
    test('Sum 1 + 2', () => {
        expect(sum(1, 2)).toEqual(3);
    });
});
