import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import ProgressMap from '../../../components/ProgressMap.vue'

describe('ProgressMap', () => {
  it('renders progress and emits open-module event when node clicked', async () => {
    const wrapper = mount(ProgressMap, {
      props: {
        modules: [
          { moduleId: 1, moduleTitle: 'Модуль 1', status: 'Завершён', isMandatory: true, bestScore: 95, attemptsCount: 1 },
          { moduleId: 2, moduleTitle: 'Модуль 2', status: 'Не начат', isMandatory: false }
        ],
        progressPercentage: 50,
        completedMandatory: 1,
        totalMandatory: 2
      }
    })

    expect(wrapper.text()).toContain('50%')
    expect(wrapper.text()).toContain('1 / 2 обяз.')
    expect(wrapper.text()).toContain('Конец маршрута')

    const moduleCard = wrapper.findAll('.card').at(1)
    expect(moduleCard.exists()).toBe(true)
    await moduleCard.trigger('click')

    expect(wrapper.emitted('open-module')).toBeTruthy()
    expect(wrapper.emitted('open-module')?.[0][0]).toEqual({
      moduleId: 1,
      moduleTitle: 'Модуль 1',
      status: 'Завершён',
      isMandatory: true,
      bestScore: 95,
      attemptsCount: 1
    })
  })
})
