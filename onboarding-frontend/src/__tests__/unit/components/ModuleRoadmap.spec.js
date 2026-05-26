import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import ModuleRoadmap from '../../../components/ModuleRoadmap.vue'

describe('ModuleRoadmap', () => {
  it('shows current step for module with test requirement', () => {
    const wrapper = mount(ModuleRoadmap, {
      props: {
        hasTestBefore: false,
        hasTest: true,
        isReadyForTest: false,
        allRequiredChecked: false,
        moduleStatus: 'Не начат'
      }
    })

    expect(wrapper.text()).toContain('Текущий этап: "Ознакомление"')
    expect(wrapper.text()).toContain('0 из 3 этапов завершено')
  })

  it('shows completion message when module is finished', () => {
    const wrapper = mount(ModuleRoadmap, {
      props: {
        hasTestBefore: true,
        hasTest: true,
        isReadyForTest: true,
        allRequiredChecked: true,
        moduleStatus: 'Завершён'
      }
    })

    expect(wrapper.text()).toContain('✨ Все этапы завершены!')
  })
})
