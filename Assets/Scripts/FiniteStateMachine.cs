// #define VERBOSE_FSM_LOGGING

using System;
using System.Collections.Generic;

public class FiniteStateMachine<T>
{
    public T State => m_state;

    public void Start(T initialState)
    {
        m_state = initialState;
        m_isInitialized = true;

        if (m_stateTable[m_state].Item1 != null)
        {
            m_stateTable[m_state].Item1();
        }
    }

    public void Update()
    {
        if(!m_isInitialized)
        {
            return;
        }

        if (m_stateTable[m_state].Item2 != null)
        {
            m_stateTable[m_state].Item2();
        }
    }

    public void AddState(T state, Action onEnter, Action onUpdate, Action onExit)
    {
        Tuple<Action, Action, Action> stateCallbacks = new(onEnter, onUpdate, onExit);
        m_stateTable.Add(state, stateCallbacks);
    }

    public void SetState(T newState)
    {
#if VERBOSE_FSM_LOGGING
        UnityEngine.Debug.Log($"Transitioning from {m_state.ToString()} to {newState.ToString()}");
#endif //VERBOSE_FSM_LOGGING

        if (m_stateTable[m_state].Item3 != null)
        {
            m_stateTable[m_state].Item3();
        }

        m_state = newState;

        if (m_stateTable[m_state].Item1 != null)
        {
            m_stateTable[m_state].Item1();
        }
    }

    // maybe shouldn't be using tuples? feels like this just makes things harder to read
    private Dictionary<T, Tuple<Action, Action, Action>> m_stateTable = new();
    private T m_state;

    bool m_isInitialized = false;
}
